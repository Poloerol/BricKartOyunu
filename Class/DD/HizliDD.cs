using System;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Tam elli (13 kart) çift dummy çözücü. Sıfır pencereli ("NS en az X löve alır mı?") alfa-beta araması +
    /// bölüm (partition) araması: arama sırasında hangi kartların löveyi kazandığı izlenir; yer değiştirme tablosu
    /// yalnızca bu "ilgili" kartların sahiplerini (ve el boylarını) anahtar olarak saklar, daha küçük kartlar
    /// birbirinin yerine geçebilir. Eşdeğer kartlar tek hamle sayılır.
    /// Koltuk sırası oyun sırasıdır: 0=N, 1=E, 2=S, 3=W.
    /// Kart/el kodu: renk * 16 + sıra (sıra 0 = iki ... 12 = As); renk sırası Maça, Kupa, Karo, Sinek.
    /// Bir örnek tek bir koz için (ya da sanzatu) kullanılır; tablo aynı koz için tüm lider/hedef çözümlerinde paylaşılır.
    /// </summary>
    public sealed class HizliDD
    {
        private readonly int _koz;                  // -1 sanzatu, 0..3
        private readonly int[] _h = new int[16];    // [koltuk*4 + renk] = 13 bitlik rütbe maskesi
        private int _kalan;                         // kalan löve sayısı
        private int _lider;

        // elde oynanan kartlar
        private readonly int[] _oynanan = new int[4];
        private int _ledRenk, _kazanan, _kazanRenk, _kazanRutbe;

        // ilgili (löveyi kazanabilen) en düşük rütbe, renk başına; 13 = ilgili kart yok
        private int _r0, _r1, _r2, _r3;
        private int _q0, _q1, _q2, _q3;

        // ── bölüm tablosu ─────────────────────────────────────────────
        private const int EBoyut = 1 << 21;
        private const int KBoyut = 1 << 20;
        [ThreadStatic] private static int[] _sBas;
        [ThreadStatic] private static ulong[] _sShape;
        [ThreadStatic] private static uint[] _sMeta;       // lider(2) | tur(1) | deger(5+)
        [ThreadStatic] private static uint[] _sO0, _sO1, _sO2, _sO3;
        [ThreadStatic] private static ushort[] _sP;        // 4 x 4 bit ilgili üst kart sayısı
        [ThreadStatic] private static int[] _sSonraki;
        [ThreadStatic] private static short[] _sBm;
        private short[] _bm;
        private int _ipucu = -1, _karar = -1;
        private int _sayac;
        private int[] _bas; private ulong[] _shape; private uint[] _meta; private uint[] _o0, _o1, _o2, _o3;
        private ushort[] _p; private int[] _sonraki;

        public long Dugum, Isabet;
        /// <summary>0 = sınırsız. Aşılırsa Coz -1 döndürür (çözülemedi).</summary>
        public int ZamanSiniriMs;
        private int _bitis;
        private bool _iptal;

        private readonly int[][] _mvRenk = new int[64][];
        private readonly int[][] _mvRutbe = new int[64][];
        private readonly int[][] _mvPuan = new int[64][];
        private readonly int[][] _allTmp = MakeTmp();
        private static int[][] MakeTmp() { var t = new int[16][]; for (int i = 0; i < 16; i++) t[i] = new int[4]; return t; }

        public HizliDD(int koz)
        {
            _koz = koz;
            if (_sBas == null)
            {
                _sBas = new int[KBoyut];
                _sShape = new ulong[EBoyut]; _sMeta = new uint[EBoyut];
                _sO0 = new uint[EBoyut]; _sO1 = new uint[EBoyut]; _sO2 = new uint[EBoyut]; _sO3 = new uint[EBoyut];
                _sP = new ushort[EBoyut]; _sSonraki = new int[EBoyut]; _sBm = new short[EBoyut];
            }
            Array.Clear(_sBas, 0, KBoyut);
            _sayac = 1;   // 0 = boş
            _bas = _sBas; _shape = _sShape; _meta = _sMeta; _o0 = _sO0; _o1 = _sO1; _o2 = _sO2; _o3 = _sO3; _p = _sP; _sonraki = _sSonraki; _bm = _sBm;
            for (int i = 0; i < 64; i++) { _mvRenk[i] = new int[14]; _mvRutbe[i] = new int[14]; _mvPuan[i] = new int[14]; }
        }

        private static int Pop(int x)
        {
            x = x - ((x >> 1) & 0x55555555);
            x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
            x = (x + (x >> 4)) & 0x0F0F0F0F;
            return (x * 0x01010101) >> 24;
        }

        private static int YuksekBit(int m) // en yüksek set bitin indeksi (m != 0)
        {
            int r = 0;
            if ((m & 0xFF00) != 0) { m >>= 8; r += 8; }
            if ((m & 0xF0) != 0) { m >>= 4; r += 4; }
            if ((m & 0xC) != 0) { m >>= 2; r += 2; }
            if ((m & 0x2) != 0) r += 1;
            return r;
        }

        private void IlgiYok() { _r0 = _r1 = _r2 = _r3 = 13; }
        private void IlgiyiAl(int a0, int a1, int a2, int a3) { _r0 = a0; _r1 = a1; _r2 = a2; _r3 = a3; }

        /// <summary>eller: 4 koltuğun elleri (renk*16+sıra bit konumu). lider: ilk löveyi açan koltuk.
        /// Dönen değer: N/S'nin (0 ve 2. koltuk) en iyi oyunla alacağı toplam löve.</summary>
        public bool Dene(ulong[] eller, int lider, int hedef)
        {
            Hazirla(eller, lider);
            return Lov(hedef);
        }

        private int Hazirla(ulong[] eller, int lider)
        {
            int n = 0;
            for (int s = 0; s < 4; s++)
                for (int r = 0; r < 4; r++)
                    _h[s * 4 + r] = (int)((eller[s] >> (r * 16)) & 0x1FFF);
            for (int r = 0; r < 4; r++) _oynanan[r] = 0;
            for (int r = 0; r < 4; r++) n += Pop(_h[r]);
            _kalan = n;
            _lider = lider;
            return n;
        }

        public int Coz(ulong[] eller, int lider, int tahmin = -1)
        {
            _iptal = false;
            _bitis = ZamanSiniriMs > 0 ? Environment.TickCount + ZamanSiniriMs : 0;
            if (_bitis == 0 && ZamanSiniriMs > 0) _bitis = 1;
            int sonuc = CozIc(eller, lider, tahmin);
            if (_iptal)
            {
                // yarıda kesilen aramanın tablo girdileri güvenilmez: sonraki hücreler için tabloyu sıfırla
                Array.Clear(_bas, 0, _bas.Length); _sayac = 1;
                return -1;
            }
            return sonuc;
        }

        private int CozIc(ulong[] eller, int lider, int tahmin)
        {
            int n = 0;
            for (int s = 0; s < 4; s++)
                for (int r = 0; r < 4; r++)
                    _h[s * 4 + r] = (int)((eller[s] >> (r * 16)) & 0x1FFF);
            for (int r = 0; r < 4; r++) _oynanan[r] = 0;
            for (int r = 0; r < 4; r++) n += Pop(_h[r]);
            _kalan = n;
            _lider = lider;
            int g = tahmin < 0 ? n / 2 : Math.Min(Math.Max(tahmin, 0), n);
            if (Lov(g))
            {
                while (g < n && Lov(g + 1)) g++;
                return g;
            }
            g--;
            while (g > 0 && !Lov(g)) g--;
            return g < 0 ? 0 : g;
        }

        // Rütbe bazlı ilgili kart sayısını (üstten) bulur
        private static int UstSayi(int all, int L)
        {
            if (L >= 13) return 0;
            return Pop(all & ~((1 << L) - 1));
        }

        // üstten P. kartın rütbesi (P kart ilgili ise en düşük ilgili rütbe); P=0 → 13 (ilgili yok)
        private static int AltRutbe(int all, int P)
        {
            if (P == 0) return 13;
            int a = all;
            int top = 0;
            for (int i = 0; i < P; i++) { top = YuksekBit(a); a ^= 1 << top; }
            return top;
        }

        // ── Löve başı arama ───────────────────────────────────────────────
        private bool Lov(int hedef)
        {
            if (_iptal) { IlgiYok(); return false; }
            if (hedef <= 0) { IlgiYok(); return true; }
            if (hedef > _kalan) { IlgiYok(); return false; }
            if (_kalan == 1) return SonLove(hedef);

            // hızlı löveler
            {
                int qt = HizliLove(_lider);
                if ((_lider & 1) == 0) { if (qt >= hedef) { IlgiyiAl(_q0, _q1, _q2, _q3); return true; } }
                else { if (qt > _kalan - hedef) { IlgiyiAl(_q0, _q1, _q2, _q3); return false; } }
            }

            // biçim anahtarı + sahip dizileri
            ulong shape = 0;
            int[] all = _allTmp[_kalan];
            uint ow0 = 0, ow1 = 0, ow2 = 0, ow3 = 0;
            for (int r = 0; r < 4; r++)
            {
                int a = _h[r] | _h[4 + r] | _h[8 + r] | _h[12 + r];
                all[r] = a;
                shape = (shape << 4) | (uint)Pop(_h[r]);
                shape = (shape << 4) | (uint)Pop(_h[4 + r]);
                shape = (shape << 4) | (uint)Pop(_h[8 + r]);
                shape = (shape << 4) | (uint)Pop(_h[12 + r]);
                uint ow = 0; int p = 0;
                while (a != 0)
                {
                    int top = YuksekBit(a);
                    int bit = 1 << top;
                    uint sahip = (_h[r] & bit) != 0 ? 0u : (_h[4 + r] & bit) != 0 ? 1u : (_h[8 + r] & bit) != 0 ? 2u : 3u;
                    ow |= sahip << (2 * p);
                    p++; a ^= bit;
                }
                if (r == 0) ow0 = ow; else if (r == 1) ow1 = ow; else if (r == 2) ow2 = ow; else ow3 = ow;
            }
            ulong hh = (shape + (ulong)_lider) * 0x9E3779B97F4A7C15UL;
            hh ^= hh >> 31;
            int bk = (int)(hh & (KBoyut - 1));

            // arama
            {
                int e = _bas[bk];
                int say = 0;
                _ipucu = -1;
                while (e != 0 && say < 40)
                {
                    say++;
                    if (_shape[e] == shape && (int)(_meta[e] & 3) == _lider)
                    {
                        ushort pp = _p[e];
                        int P0 = pp & 15, P1 = (pp >> 4) & 15, P2 = (pp >> 8) & 15, P3 = (pp >> 12) & 15;
                        if ((((_o0[e] ^ ow0) & ((1u << (2 * P0)) - 1)) | ((_o1[e] ^ ow1) & ((1u << (2 * P1)) - 1)) |
                             ((_o2[e] ^ ow2) & ((1u << (2 * P2)) - 1)) | ((_o3[e] ^ ow3) & ((1u << (2 * P3)) - 1))) == 0)
                        {
                            bool ub = ((_meta[e] >> 2) & 1) != 0;
                            int v = (int)(_meta[e] >> 3);
                            if (_bm[e] >= 0 && _ipucu < 0) _ipucu = _bm[e];
                            bool cevap;
                            if (!ub) { if (v < hedef) { e = _sonraki[e]; continue; } cevap = true; }
                            else { if (v >= hedef) { e = _sonraki[e]; continue; } cevap = false; }
                            Isabet++;
                            IlgiyiAl(AltRutbe(all[0], P0), AltRutbe(all[1], P1), AltRutbe(all[2], P2), AltRutbe(all[3], P3));
                            return cevap;
                        }
                    }
                    e = _sonraki[e];
                }
            }

            Dugum++;
            if (_bitis != 0 && (Dugum & 4095) == 0 && Environment.TickCount - _bitis > 0) _iptal = true;
            int ipucuYerel = _ipucu;
            _ipucu = ipucuYerel;
            bool sonuc = Kart(0, _lider, hedef);
            int karar = _karar;
            int c0 = _r0, c1 = _r1, c2 = _r2, c3 = _r3;

            // kaydet
            {
                if (_sayac >= EBoyut - 2) { Array.Clear(_bas, 0, KBoyut); _sayac = 1; }
                int e = _sayac++;
                int P0 = UstSayi(all[0], c0), P1 = UstSayi(all[1], c1), P2 = UstSayi(all[2], c2), P3 = UstSayi(all[3], c3);
                _shape[e] = shape;
                _o0[e] = ow0; _o1[e] = ow1; _o2[e] = ow2; _o3[e] = ow3;
                _p[e] = (ushort)(P0 | (P1 << 4) | (P2 << 8) | (P3 << 12));
                uint deger = (uint)(sonuc ? hedef : hedef - 1);
                _meta[e] = (uint)_lider | ((sonuc ? 0u : 1u) << 2) | (deger << 3);
                _bm[e] = (short)karar;
                _sonraki[e] = _bas[bk];
                _bas[bk] = e;
            }
            IlgiyiAl(c0, c1, c2, c3);
            return sonuc;
        }

        // Liderin, elindeki ardışık "efendi" kartlarla kesin alacağı löveler (ilgili rütbeler _q0.._q3'e).
        private int HizliLove(int seat)
        {
            int toplam = 0;
            int q0 = 13, q1 = 13, q2 = 13, q3 = 13;
            int sol = (seat + 1) & 3, sag = (seat + 3) & 3;
            for (int r = 0; r < 4; r++)
            {
                int mine = _h[seat * 4 + r];
                if (mine == 0) continue;
                int aa = mine | _h[sol * 4 + r] | _h[((seat + 2) & 3) * 4 + r] | _h[sag * 4 + r];
                int a = aa;
                int m = 0;
                while (a != 0)
                {
                    int top = YuksekBit(a);
                    if ((mine & (1 << top)) == 0) break;
                    m++; a ^= 1 << top;
                }
                if (m == 0) continue;
                if (_koz >= 0 && r != _koz)
                {
                    if (_h[sol * 4 + _koz] != 0) { int lo = Pop(_h[sol * 4 + r]); if (lo < m) m = lo; }
                    if (_h[sag * 4 + _koz] != 0) { int lo = Pop(_h[sag * 4 + r]); if (lo < m) m = lo; }
                    if (m == 0) continue;
                }
                int dusuk = AltRutbe(aa, m);
                toplam += m;
                if (r == 0) q0 = dusuk; else if (r == 1) q1 = dusuk; else if (r == 2) q2 = dusuk; else q3 = dusuk;
            }
            _q0 = q0; _q1 = q1; _q2 = q2; _q3 = q3;
            return toplam;
        }

        // Her elde tek kart kaldığında doğrudan hesapla
        private bool SonLove(int hedef)
        {
            int kz = -1, kzRenk = -1, kzRutbe = -1;
            int seat = _lider;
            for (int k = 0; k < 4; k++)
            {
                int rn = -1, rt = -1;
                for (int r = 0; r < 4; r++) if (_h[seat * 4 + r] != 0) { rn = r; rt = YuksekBit(_h[seat * 4 + r]); break; }
                if (k == 0) { kz = seat; kzRenk = rn; kzRutbe = rt; }
                else
                {
                    bool yener = rn == kzRenk ? rt > kzRutbe : (rn == _koz);
                    if (yener) { kz = seat; kzRenk = rn; kzRutbe = rt; }
                }
                seat = (seat + 1) & 3;
            }
            IlgiYok();
            if (kzRenk == 0) _r0 = kzRutbe; else if (kzRenk == 1) _r1 = kzRutbe; else if (kzRenk == 2) _r2 = kzRutbe; else _r3 = kzRutbe;
            return (kz & 1) == 0 && hedef <= 1;
        }

        // ── Löve içi kart seçimi ─────────────────────────────────────────
        private bool Kart(int k, int seat, int hedef)
        {
            bool maks = (seat & 1) == 0;
            int derin = (_kalan * 4 + k) & 63;
            int[] mr = _mvRenk[derin];
            int[] mt = _mvRutbe[derin];
            int[] mp = _mvPuan[derin];
            int adet = 0;

            int ben = seat * 4;
            if (k == 0)
            {
                for (int r = 0; r < 4; r++)
                {
                    int m = _h[ben + r];
                    if (m == 0) continue;
                    int all = _h[r] | _h[4 + r] | _h[8 + r] | _h[12 + r];
                    adet = Grupla(m, all, r, mr, mt, mp, adet, 0, seat);
                }
            }
            else
            {
                int led = _ledRenk;
                int m = _h[ben + led];
                if (m != 0)
                {
                    int all = _h[led] | _h[4 + led] | _h[8 + led] | _h[12 + led] | _oynanan[led];
                    adet = Grupla(m, all, led, mr, mt, mp, adet, k, seat);
                }
                else
                {
                    for (int r = 0; r < 4; r++)
                    {
                        int mm = _h[ben + r];
                        if (mm == 0) continue;
                        int all = _h[r] | _h[4 + r] | _h[8 + r] | _h[12 + r] | _oynanan[r];
                        adet = Grupla(mm, all, r, mr, mt, mp, adet, k, seat);
                    }
                }
            }

            for (int i = 1; i < adet; i++)
            {
                int pr = mr[i], pt = mt[i], pp = mp[i];
                int j = i - 1;
                while (j >= 0 && mp[j] < pp) { mr[j + 1] = mr[j]; mt[j + 1] = mt[j]; mp[j + 1] = mp[j]; j--; }
                mr[j + 1] = pr; mt[j + 1] = pt; mp[j + 1] = pp;
            }

            int ipucu = -1;
            if (k == 0)
            {
                ipucu = _ipucu;
                if (ipucu >= 0)
                {
                    int ir = ipucu >> 4, it = ipucu & 15;
                    for (int i = 0; i < adet; i++) if (mr[i] == ir && mt[i] == it) { mp[i] += 1000; break; }
                    // yeniden sırala
                    for (int i = 1; i < adet; i++)
                    {
                        int pr = mr[i], pt = mt[i], pp = mp[i];
                        int j = i - 1;
                        while (j >= 0 && mp[j] < pp) { mr[j + 1] = mr[j]; mt[j + 1] = mt[j]; mp[j + 1] = mp[j]; j--; }
                        mr[j + 1] = pr; mt[j + 1] = pt; mp[j + 1] = pp;
                    }
                }
            }
            bool sonuc = !maks;
            int sk = _kazanan, skr = _kazanRenk, skt = _kazanRutbe, sld = _ledRenk;
            int a0 = 13, a1 = 13, a2 = 13, a3 = 13;   // birleşik ilgi (karar vermeyen çocuklar)
            for (int i = 0; i < adet; i++)
            {
                int r = mr[i], rt = mt[i];
                int bit = 1 << rt;
                _h[ben + r] ^= bit;
                _oynanan[r] |= bit;
                if (k == 0) { _ledRenk = r; _kazanan = seat; _kazanRenk = r; _kazanRutbe = rt; }
                else
                {
                    bool yener = (r == _kazanRenk) ? rt > _kazanRutbe : (r == _koz);
                    if (yener) { _kazanan = seat; _kazanRenk = r; _kazanRutbe = rt; }
                }

                bool s;
                int c0, c1, c2, c3;
                if (k == 3)
                {
                    int w = _kazanan;
                    int wr = _kazanRenk, wt = _kazanRutbe;
                    int ns = (w & 1) == 0 ? 1 : 0;
                    int eskiLider = _lider;
                    _kalan--;
                    _lider = w;
                    int o0 = _oynanan[0], o1 = _oynanan[1], o2 = _oynanan[2], o3 = _oynanan[3];
                    _oynanan[0] = _oynanan[1] = _oynanan[2] = _oynanan[3] = 0;
                    s = Lov(hedef - ns);
                    _oynanan[0] = o0; _oynanan[1] = o1; _oynanan[2] = o2; _oynanan[3] = o3;
                    _kalan++;
                    _lider = eskiLider;
                    c0 = _r0; c1 = _r1; c2 = _r2; c3 = _r3;
                    // löveyi kazanan kart ilgilidir
                    if (wr == 0) { if (wt < c0) c0 = wt; }
                    else if (wr == 1) { if (wt < c1) c1 = wt; }
                    else if (wr == 2) { if (wt < c2) c2 = wt; }
                    else { if (wt < c3) c3 = wt; }
                }
                else
                {
                    s = Kart(k + 1, (seat + 1) & 3, hedef);
                    c0 = _r0; c1 = _r1; c2 = _r2; c3 = _r3;
                }

                // geri al
                _h[ben + r] ^= bit;
                _oynanan[r] &= ~bit;
                _kazanan = sk; _kazanRenk = skr; _kazanRutbe = skt; _ledRenk = sld;

                if (maks)
                {
                    if (s) { _r0 = c0; _r1 = c1; _r2 = c2; _r3 = c3; if (k == 0) _karar = r * 16 + rt; return true; }
                }
                else
                {
                    if (!s) { _r0 = c0; _r1 = c1; _r2 = c2; _r3 = c3; if (k == 0) _karar = r * 16 + rt; return false; }
                }
                if (c0 < a0) a0 = c0;
                if (c1 < a1) a1 = c1;
                if (c2 < a2) a2 = c2;
                if (c3 < a3) a3 = c3;
            }
            _r0 = a0; _r1 = a1; _r2 = a2; _r3 = a3;
            if (k == 0) _karar = -1;
            return sonuc;
        }

        // m: elimdeki bu renk maskesi, all: bu renkte oyundaki tüm kartlar (eşdeğerlik için).
        private int Grupla(int m, int all, int renk, int[] mr, int[] mt, int[] mp, int adet, int k, int seat)
        {
            int kalanM = m;
            int onceki = -1;
            bool ilk = true;
            while (kalanM != 0)
            {
                int c = YuksekBit(kalanM);
                kalanM ^= 1 << c;
                bool esdeger = false;
                if (!ilk)
                {
                    int aralik = ((1 << onceki) - 1) & ~((1 << (c + 1)) - 1);
                    if ((all & aralik) == 0) esdeger = true;
                }
                ilk = false;
                onceki = c;
                if (esdeger) continue;
                int puan;
                if (k == 0)
                {
                    int topAll = YuksekBit(all);
                    puan = (c == topAll ? 100 : 0) + Pop(m) * 2 + (c >= topAll - 1 ? 10 : 0) + c;
                }
                else
                {
                    bool ortakKazaniyor = ((_kazanan ^ seat) & 1) == 0;
                    if (renk == _ledRenk && renk == _kazanRenk)
                    {
                        bool yener = c > _kazanRutbe;
                        if (ortakKazaniyor) puan = 50 - c;
                        else if (yener) puan = 200 - c;
                        else puan = 50 - c;
                    }
                    else if (renk == _koz && _koz >= 0 && _kazanRenk != _koz) puan = ortakKazaniyor ? 10 - c : 150 - c;
                    else if (renk == _koz && _koz >= 0 && _kazanRenk == _koz)
                    {
                        bool yener = c > _kazanRutbe;
                        puan = yener && !ortakKazaniyor ? 140 - c : 5 - c;
                    }
                    else puan = 20 - c;
                }
                mr[adet] = renk; mt[adet] = c; mp[adet] = puan; adet++;
            }
            return adet;
        }
    }
}
