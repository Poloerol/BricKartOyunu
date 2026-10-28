using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class
{
    /// <summary>Renk planlayıcının bir ilk hamle adayı.</summary>
    public sealed class RenkAday
    {
        /// <summary>Oynanacak kart (2..14).</summary>
        public int Kart;
        /// <summary>İhtiyaç duyulan love sayısına (ya da fazlasına) ulaşma olasılığı (0..1).</summary>
        public double YapmaSansi;
        public double BeklenenLove;
    }

    public sealed class RenkPlanSonucu
    {
        public bool Basarili;
        /// <summary>Çözülemediyse nedeni (kart sayısı çok fazla, düğüm sınırı aşıldı ...).</summary>
        public string Neden = "";
        public int Kart;
        public double YapmaSansi;
        public double BeklenenLove;
        public List<RenkAday> Adaylar = new List<RenkAday>();
    }

    /// <summary>
    /// Tek renk kombinasyonu çözücü (Melih Özdil, "Kart Oyun Tekniği": empas, çift empas, ters / iki yönlü / derin empas,
    /// düşürme, sağlama). Dışardaki kartların tüm olası dağılımlarını (rakiplerin kalan kart sayılarına göre ağırlıklı) sayar;
    /// deklaran, rakiplerin oynadığı kartlara bakarak her turda en iyi kartı seçer (tek-dummy bilgi: dağılımı bilmez);
    /// rakipler dağılımı bilerek en iyi savunmayı yapar (çift dummy savunma).
    /// Kabuller: (1) iki elde de gerekli girişler vardır (kazanılan lovenin ardından istenen elden oynanabilir),
    /// (2) bu renkte kesme yoktur (sanzatu ya da rakiplerin kozu bitmiş), (3) rakipler bu rengi açmaz.
    /// </summary>
    public static class RenkPlanlayici
    {
        public static long DugumSiniri = 600000;
        public const int EnFazlaDisariKart = 8;

        public static RenkPlanSonucu Coz(int[] eliA, int[] eliB, int[] disari, int rakipXKart, int rakipYKart,
                                         bool xRenksiz, bool yRenksiz, int ihtiyac)
        {
            var c = new Cozucu(eliA, eliB, disari, rakipXKart, rakipYKart, xRenksiz, yRenksiz, ihtiyac);
            return c.Calistir();
        }

        private static int Bit(int rank) { return 1 << (rank - 2); }
        private static int Pop(int m) { int n = 0; while (m != 0) { m &= m - 1; n++; } return n; }
        private static int Maske(int[] r) { int m = 0; if (r != null) foreach (int x in r) m |= Bit(x); return m; }

        /// <summary>Elin (hand) denk kartlarının temsilcileri: arada başka elde / dışarıda kart olmayan ardışık kartlar tek sayılır.</summary>
        private static List<int> Temsilciler(int hand, int union, bool ust)
        {
            var sonuc = new List<int>();
            bool kosu = false; int kosuUst = 0, kosuAlt = 0;
            for (int r = 14; r >= 2; r--)
            {
                int b = Bit(r);
                if ((union & b) == 0) continue;
                if ((hand & b) != 0)
                {
                    if (!kosu) { kosu = true; kosuUst = r; }
                    kosuAlt = r;
                }
                else if (kosu) { sonuc.Add(ust ? kosuUst : kosuAlt); kosu = false; }
            }
            if (kosu) sonuc.Add(ust ? kosuUst : kosuAlt);
            return sonuc;
        }

        private sealed class SinirAsildi : Exception { }

        private struct Deger
        {
            public double P, E;
            public void Ekle(Deger d) { P += d.P; E += d.E; }
        }

        private sealed class Cozucu
        {
            private readonly int _a0, _b0, _dis, _ihtiyac;
            private readonly int _remX, _remY;
            private readonly bool _xBos, _yBos;
            private int[] _xMask;      // her dağılım için X'in (A'dan sonra oynayan rakip) kartları
            private double[] _w;       // ağırlıklar (toplam 1)
            private readonly Dictionary<long, int> _memo = new Dictionary<long, int>();
            private long _dugum;
            // eller: 0=A, 1=B, 2=X (A'dan sonra), 3=Y (B'den sonra)
            private static readonly int[] SiraA = { 0, 2, 1, 3 };
            private static readonly int[] SiraB = { 1, 3, 0, 2 };

            public Cozucu(int[] a, int[] b, int[] dis, int remX, int remY, bool xBos, bool yBos, int ihtiyac)
            {
                _a0 = Maske(a); _b0 = Maske(b); _dis = Maske(dis);
                _remX = remX; _remY = remY; _xBos = xBos; _yBos = yBos; _ihtiyac = ihtiyac;
            }

            private void Say() { if (++_dugum > DugumSiniri) throw new SinirAsildi(); }

            public RenkPlanSonucu Calistir()
            {
                var s = new RenkPlanSonucu();
                int k = Pop(_dis);
                if (k > EnFazlaDisariKart) { s.Neden = "Dışarıda " + k + " kart var (en fazla " + EnFazlaDisariKart + ")"; return s; }
                if ((_a0 | _b0) == 0) { s.Neden = "Bu renkte kartımız yok"; return s; }
                if (!AgirliklariKur(k)) { s.Neden = "Dağılım ağırlıkları kurulamadı"; return s; }

                var tum = new List<int>();
                for (int i = 0; i < _w.Length; i++) tum.Add(i);

                try
                {
                    int union = _a0 | _b0 | _dis;
                    foreach (int c in Temsilciler(_a0, union, true))
                    {
                        Deger v = Hamle(tum, _a0, _b0, 0, 0, 0, 0, c);
                        s.Adaylar.Add(new RenkAday { Kart = c, YapmaSansi = v.P, BeklenenLove = v.E });
                    }
                }
                catch (SinirAsildi)
                {
                    s.Neden = "Düğüm sınırı aşıldı";
                    return s;
                }
                if (s.Adaylar.Count == 0) { s.Neden = "Oynanabilir kart yok"; return s; }

                RenkAday en = null;
                foreach (var ad in s.Adaylar)
                    if (en == null || Daha(ad.YapmaSansi, ad.BeklenenLove, en.YapmaSansi, en.BeklenenLove)) en = ad;
                s.Basarili = true;
                s.Kart = en.Kart; s.YapmaSansi = en.YapmaSansi; s.BeklenenLove = en.BeklenenLove;
                return s;
            }

            private bool Daha(double p1, double e1, double p2, double e2)
            {
                if (_ihtiyac > 0)
                {
                    if (Math.Abs(p1 - p2) > 1e-12) return p1 > p2;
                    return e1 > e2 + 1e-12;
                }
                return e1 > e2 + 1e-12;
            }

            private bool AgirliklariKur(int k)
            {
                var ranks = new List<int>();
                for (int r = 14; r >= 2; r--) if ((_dis & Bit(r)) != 0) ranks.Add(r);
                int m = _remX + _remY;
                if (m < k || m <= 0) return false;
                var xm = new List<int>(); var w = new List<double>();
                double top = 0;
                for (int s = 0; s < (1 << k); s++)
                {
                    int a = Pop(s);
                    if (a > _remX || (k - a) > _remY) continue;
                    if (_xBos && a != 0) continue;
                    if (_yBos && a != k) continue;
                    int mask = 0;
                    for (int i = 0; i < k; i++) if ((s & (1 << i)) != 0) mask |= Bit(ranks[i]);
                    double p = DagilimOlasiliklari.AltKumeOlasiligi(k, a, _remX, _remY);
                    if (p <= 0) continue;
                    xm.Add(mask); w.Add(p); top += p;
                }
                if (xm.Count == 0 || top <= 0) return false;
                _xMask = xm.ToArray();
                _w = w.Select(x => x / top).ToArray();
                return true;
            }

            // ───────────── Çift dummy (tüm kartlar açık) değer: deklaran tarafının alacağı love ─────────────

            private int DD(int a, int b, int x, int y)
            {
                if ((a | b) == 0) return 0;
                if ((x | y) == 0) return Math.Max(Pop(a), Pop(b));
                long key = (long)a | ((long)b << 13) | ((long)x << 26) | ((long)y << 39);
                int v;
                if (_memo.TryGetValue(key, out v)) return v;
                Say();
                int best = -1;
                int union = a | b | x | y;
                for (int side = 0; side < 2; side++)
                {
                    int hand = side == 0 ? a : b;
                    if (hand == 0) continue;
                    foreach (int c in Temsilciler(hand, union, false))
                    {
                        var m = new int[] { a, b, x, y };
                        int val = DDOyna(m, side == 0 ? SiraA : SiraB, 0, 0, -1, 0, c);
                        if (val > best) best = val;
                    }
                }
                _memo[key] = best;
                return best;
            }

            /// <summary>Turun pos'taki oyuncusu oynar. forced>=0 ise yalnız o kart denenir. trick: bu turda oynanan kartların maskesi.</summary>
            private int DDOyna(int[] m, int[] sira, int pos, int bestRank, int bestHand, int trick, int forced)
            {
                if (pos == 4)
                {
                    bool deklaran = bestHand <= 1;
                    return (deklaran ? 1 : 0) + DD(m[0], m[1], m[2], m[3]);
                }
                int h = sira[pos];
                int mask = m[h];
                if (mask == 0) return DDOyna(m, sira, pos + 1, bestRank, bestHand, trick, -2);
                bool maks = h <= 1;
                int best = maks ? -1 : int.MaxValue;
                IEnumerable<int> adaylar;
                if (forced >= 0) adaylar = new int[] { forced };
                else adaylar = Temsilciler(mask, m[0] | m[1] | m[2] | m[3] | trick, false);
                foreach (int c in adaylar)
                {
                    var m2 = (int[])m.Clone();
                    m2[h] &= ~Bit(c);
                    int nb = c > bestRank ? c : bestRank;
                    int nh = c > bestRank ? h : bestHand;
                    int v = DDOyna(m2, sira, pos + 1, nb, nh, trick | Bit(c), -2);
                    if (maks) { if (v > best) best = v; } else { if (v < best) best = v; }
                }
                return best;
            }

            /// <summary>Rakibin (pos'taki) çift dummy en iyi kartı; eşitlikte en küçük kart.</summary>
            private int RakipKarti(int[] m, int[] sira, int pos, int bestRank, int bestHand, int trick)
            {
                int h = sira[pos];
                int mask = m[h];
                if (mask == 0) return -1;
                int enIyi = -1, enIyiV = int.MaxValue;
                var adaylar = Temsilciler(mask, m[0] | m[1] | m[2] | m[3] | trick, false);   // alttan üste
                adaylar.Sort();
                foreach (int c in adaylar)
                {
                    int v = DDOyna(m, sira, pos, bestRank, bestHand, trick, c);
                    if (v < enIyiV) { enIyiV = v; enIyi = c; }
                }
                return enIyi;
            }

            // ───────────── Tek dummy (deklaran) arama ─────────────

            private Deger Bitir(List<int> grup, int love)
            {
                double w = 0;
                foreach (int i in grup) w += _w[i];
                Deger d;
                d.P = (_ihtiyac <= 0 || love >= _ihtiyac) ? w : 0;
                d.E = w * love;
                return d;
            }

            private bool Daha(Deger x, Deger y) { return Daha(x.P, x.E, y.P, y.E); }

            /// <summary>Deklaran bir tur başlatır (ilk turda yalnız A elinden, ilk hamle verilmişse yalnız o kart).</summary>
            private Deger Lead(List<int> grup, int a, int b, int oynanan, int love, bool ilk)
            {
                Say();
                if ((a | b) == 0) return Bitir(grup, love);
                int disKalan = _dis & ~oynanan;
                if (disKalan == 0) return Bitir(grup, love + Math.Max(Pop(a), Pop(b)));

                bool var_ = false; Deger best = default(Deger);
                int union = a | b | disKalan;
                for (int side = 0; side < 2; side++)
                {
                    if (ilk && side != 0) continue;
                    int hand = side == 0 ? a : b;
                    if (hand == 0) continue;
                    foreach (int c in Temsilciler(hand, union, true))
                    {
                        Deger v = Hamle(grup, a, b, oynanan, love, side, 0, c);
                        if (!var_ || Daha(v, best)) { best = v; var_ = true; }
                    }
                }
                return best;
            }

            private void ElleriVer(int i, int oynanan, out int x, out int y)
            {
                x = _xMask[i] & ~oynanan;
                y = _dis & ~_xMask[i] & ~oynanan;
            }

            /// <summary>Bir tur: lider "side" elinden c kartıyla başlar.</summary>
            private Deger Hamle(List<int> grup, int a, int b, int oynanan, int love, int side, int _unused, int c)
            {
                Say();
                int[] sira = side == 0 ? SiraA : SiraB;
                int a1 = side == 0 ? a & ~Bit(c) : a;
                int b1 = side == 1 ? b & ~Bit(c) : b;
                int trick1 = Bit(c);

                // 2. oyuncu: rakip
                var bol1 = new Dictionary<int, List<int>>();
                foreach (int i in grup)
                {
                    int x, y; ElleriVer(i, oynanan, out x, out y);
                    var m = new int[] { a1, b1, x, y };
                    int k1 = RakipKarti(m, sira, 1, c, side, trick1);
                    List<int> l;
                    if (!bol1.TryGetValue(k1, out l)) { l = new List<int>(); bol1[k1] = l; }
                    l.Add(i);
                }

                Deger toplam = default(Deger);
                foreach (var kv in bol1)
                {
                    int c1 = kv.Key;
                    var alt = kv.Value;
                    int bestRank1 = c; int bestHand1 = side;
                    int trick2 = trick1;
                    if (c1 >= 0) { trick2 |= Bit(c1); if (c1 > bestRank1) { bestRank1 = c1; bestHand1 = sira[1]; } }
                    toplam.Ekle(UcuncuOyuncu(alt, a1, b1, oynanan, love, sira, c, c1, bestRank1, bestHand1, trick2));
                }
                return toplam;
            }

            /// <summary>3. oyuncu: deklaran tarafının diğer eli; 2. rakibin kartını gördükten sonra seçer.</summary>
            private Deger UcuncuOyuncu(List<int> grup, int a1, int b1, int oynanan, int love, int[] sira,
                                       int c, int c1, int bestRank, int bestHand, int trick)
            {
                int h2 = sira[2];
                int el2 = h2 == 0 ? a1 : b1;
                var secenekler = new List<int>();
                if (el2 == 0) secenekler.Add(-1);
                else
                {
                    int disKalan = _dis & ~oynanan;
                    secenekler.AddRange(Temsilciler(el2, a1 | b1 | disKalan | trick, false));
                }

                bool var_ = false; Deger best = default(Deger);
                foreach (int o in secenekler)
                {
                    int a2 = a1, b2 = b1;
                    if (o >= 0) { if (h2 == 0) a2 &= ~Bit(o); else b2 &= ~Bit(o); }
                    int br = bestRank, bh = bestHand, tr = trick;
                    if (o >= 0) { tr |= Bit(o); if (o > br) { br = o; bh = h2; } }

                    var bol = new Dictionary<int, List<int>>();
                    foreach (int i in grup)
                    {
                        int x, y; ElleriVer(i, oynanan, out x, out y);
                        if (c1 >= 0) { if (sira[1] == 2) x &= ~Bit(c1); else y &= ~Bit(c1); }
                        var m = new int[] { a2, b2, x, y };
                        int k3 = RakipKarti(m, sira, 3, br, bh, tr);
                        List<int> l;
                        if (!bol.TryGetValue(k3, out l)) { l = new List<int>(); bol[k3] = l; }
                        l.Add(i);
                    }

                    Deger toplam = default(Deger);
                    foreach (var kv in bol)
                    {
                        int c3 = kv.Key;
                        int br2 = br, bh2 = bh;
                        if (c3 >= 0 && c3 > br2) { br2 = c3; bh2 = sira[3]; }
                        int yeniOynanan = oynanan;
                        if (c1 >= 0) yeniOynanan |= Bit(c1);
                        if (c3 >= 0) yeniOynanan |= Bit(c3);
                        int yeniLove = love + (bh2 <= 1 ? 1 : 0);
                        toplam.Ekle(Lead(kv.Value, a2, b2, yeniOynanan, yeniLove, false));
                    }
                    if (!var_ || Daha(toplam, best)) { best = toplam; var_ = true; }
                }
                return best;
            }
        }
    }
}
