using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Monte Carlo + çift dummy kart seçici (GIB tarzı). Görünmeyen kartların olası dağılımlarını örnekler,
    /// her örnekte tüm yasal kartları çift dummy çözücüyle değerlendirir ve kontratın tutma ihtimali
    /// (hedef yoksa ortalama löve) en yüksek kartı seçer. Yalnızca meşru bilgiyi kullanır.
    /// </summary>
    public static class MonteCarloOyun
    {
        public static bool Aktif = true;
        /// <summary>Elimizdeki kart sayısı bu değerden büyükse devreye girmez (çözücü yavaşlar).</summary>
        public static int EnFazlaElKart = 8;
        /// <summary>Kontrat hedefi bilinirken (yalnız "tutar mı" sorusu) daha hızlı çözüldüğü için izin verilen kart sayısı.</summary>
        public static int EnFazlaElKartHedefli = 10;
        /// <summary>Bir karar için zaman bütçesi (ms).</summary>
        public static int ButceMs = 700;
        /// <summary>Bu süre aşılınca (en az bir örnek varsa) aramayı bırakır.</summary>
        public static int SertSinirMs = 2500;
        /// <summary>İhaleden çıkarılan HCP / renk uzunluğu bilgisini örneklemede kullan (IhaleAnalizi). Ölçümde (640 bord) fark yok (-0.02 ± 0.03 löve), bu yüzden varsayılan kapalı.</summary>
        public static bool IhaleKullan = false;
        /// <summary>Bir örnek için en çok kaç dağılım denenip ihale bilgisine uyan aranır; bulunamazsa kısıtsız örnek alınır.</summary>
        public static int KisitDenemeSiniri = 300;
        /// <summary>Pas diyen koltuğun HCP üst sınırını da kullan. IhaleAnalizi kalibre edildi: pas koltuğunun aralığı son gerçek çağrısından gelir.</summary>
        public static bool PasUstSiniriKullan = true;
        public static int HcUstTolerans = 2;
        public static int IstatKisitli, IstatKisitAsildi;
        public static int EnAzOrnek = 6;
        public static int EnCokOrnek = 100;
        public static Random Rnd = new Random();
        /// <summary>Tanılama: uygun görünen kararlarda kaç kez çalıştı / veri tutarsızlığı yüzünden vazgeçti.</summary>
        public static int IstatCalisti, IstatVazgecti;

        public sealed class Sonuc
        {
            public List<Card> Eniyiler = new List<Card>();   // eşit en iyi kartlar
            public int Ornek;
            public double Ihtimal;                           // en iyi kartın hedefi tutturma ihtimali (hedef yoksa -1)
            public double OrtLove;                           // en iyi kartın ortalama löve (kalan eller için, bizim taraf)
            public string Aciklama;
        }

        private static readonly string[] Renkler = { "Maça", "Kupa", "Karo", "Sinek" };
        private static int RenkNo(string r)
        {
            for (int i = 0; i < 4; i++) if (Renkler[i] == r) return i;
            return -1;
        }
        private static int Kod(Card c) { return RenkNo(c.Suit) * 16 + (c.Value - 2); }

        /// <summary>Seçim yapar; koşullar uygun değilse (kart sayısı, dummy görünmez, tutarsız veri) null döner.</summary>
        public static Sonuc Sec(OyunBaglami b, List<Card> legal)
        {
            var s0 = SecIc(b, legal);
            if (b != null && b.Elim != null && b.Elim.Count <= EnFazlaElKart && legal != null && legal.Count > 1 &&
                (b.OrtakEli != null || b.DummyEli != null))
            { if (s0 == null) IstatVazgecti++; else IstatCalisti++; }
            return s0;
        }

        private static Sonuc SecIc(OyunBaglami b, List<Card> legal)
        {
            if (!Aktif || b == null || legal == null || legal.Count < 2) return null;
            int n = b.Elim.Count;
            if (n > Math.Max(EnFazlaElKart, EnFazlaElKartHedefli)) return null;

            Player ben = b.Ben;
            Player[] koltuk = new Player[4];
            koltuk[0] = ben;
            koltuk[1] = OyunKurallari.Sonraki(ben);
            koltuk[2] = OyunKurallari.Ortak(ben);
            koltuk[3] = OyunKurallari.Sonraki(koltuk[2]);
            Func<Player, int> seatOf = p => { for (int i = 0; i < 4; i++) if (koltuk[i] == p) return i; return -1; };

            Player dummy = OyunKurallari.Ortak(b.Deklaran);
            bool deklaranTarafi = (ben == b.Deklaran || ben == dummy);
            List<Card> gorunenEl = deklaranTarafi ? b.OrtakEli : b.DummyEli;
            if (gorunenEl == null) return null;
            int gorunenSeat = deklaranTarafi ? 2 : seatOf(dummy);
            if (gorunenSeat <= 0) return null;

            // Oynanmış kartlar ve kimin kaç kart oynadığı
            var oynanan = new HashSet<int>();
            int[] oynadi = new int[4];
            if (b.Gecmis != null)
                foreach (var g in b.Gecmis) { oynanan.Add(Kod(g.Kart)); int s = seatOf(g.Oyuncu); if (s >= 0) oynadi[s]++; }
            if (b.Masa != null)
                foreach (var m in b.Masa) { oynanan.Add(Kod(m.Kart)); int s = seatOf(m.Oyuncu); if (s >= 0) oynadi[s]++; }

            ulong[] el = new ulong[4];
            foreach (var c in b.Elim) el[0] |= 1UL << Kod(c);
            foreach (var c in gorunenEl) { int k = Kod(c); if (!oynanan.Contains(k)) el[gorunenSeat] |= 1UL << k; }

            int[] kalan = new int[4];
            for (int i = 0; i < 4; i++) kalan[i] = 13 - oynadi[i];
            if (PopC(el[0]) != kalan[0] || PopC(el[gorunenSeat]) != kalan[gorunenSeat]) return null;   // tutarsız

            // Her koltuğun oynadığı kartlardan HCP ve renk adetleri (ihale bilgisiyle ilk elin karşılaştırılması için)
            int[] oynHcp = new int[4]; int[][] oynUz = new int[4][];
            for (int i = 0; i < 4; i++) oynUz[i] = new int[4];
            Action<Player, Card> kaydet = (p, c) =>
            {
                int s = seatOf(p); if (s < 0) return;
                oynUz[s][RenkNo(c.Suit)]++;
                if (c.Value >= 11) oynHcp[s] += c.Value - 10;
            };
            if (b.Gecmis != null) foreach (var g in b.Gecmis) kaydet(g.Oyuncu, g.Kart);
            if (b.Masa != null) foreach (var m in b.Masa) kaydet(m.Oyuncu, m.Kart);

            // Gizli koltuklar
            int[] gizli = new int[2]; int gi = 0;
            for (int i = 1; i < 4; i++) if (i != gorunenSeat) gizli[gi++] = i;
            if (gi != 2) return null;

            // Görünmeyen kartlar
            var unseen = new List<int>();
            for (int s = 0; s < 4; s++)
                for (int r = 0; r < 13; r++)
                {
                    int code = s * 16 + r;
                    ulong bit = 1UL << code;
                    if ((el[0] & bit) != 0 || (el[gorunenSeat] & bit) != 0 || oynanan.Contains(code)) continue;
                    unseen.Add(code);
                }
            if (unseen.Count != kalan[gizli[0]] + kalan[gizli[1]]) return null;

            // Renksiz kalma bilgisi (lider rengini takip etmeyen oyuncu o renkte boştur)
            bool[][] bos = new bool[4][];
            for (int i = 0; i < 4; i++) bos[i] = new bool[4];
            if (b.Gecmis != null)
            {
                int cur = int.MinValue; int lead = -1;
                foreach (var g in b.Gecmis)
                {
                    if (g.ElNo != cur) { cur = g.ElNo; lead = RenkNo(g.Kart.Suit); continue; }
                    int s = seatOf(g.Oyuncu);
                    if (s >= 0 && RenkNo(g.Kart.Suit) != lead && lead >= 0) bos[s][lead] = true;
                }
            }
            int[] tc = new int[4]; int tk = 0; int leaderSeat = 0;
            if (b.Masa != null && b.Masa.Count > 0)
            {
                leaderSeat = seatOf(b.Masa[0].Oyuncu);
                if (leaderSeat < 0) return null;
                int lead = RenkNo(b.Masa[0].Kart.Suit);
                foreach (var m in b.Masa)
                {
                    tc[tk++] = Kod(m.Kart);
                    int s = seatOf(m.Oyuncu);
                    if (s >= 0 && RenkNo(m.Kart.Suit) != lead) bos[s][lead] = true;
                }
            }
            if (((leaderSeat + tk) & 3) != 0) return null;       // sıra bende değil → tutarsız

            // Hedef: bizim taraf kaç löve daha almalı
            int soFar = 0;
            if (b.Gecmis != null)
                foreach (var g in b.Gecmis)
                    if (g.Kazandi) { int s = seatOf(g.Oyuncu); if (s == 0 || s == 2) soFar++; }
            int need = int.MaxValue;
            if (b.Seviye >= 1 && b.Seviye <= 7)
                need = (deklaranTarafi ? 6 + b.Seviye : 8 - b.Seviye) - soFar;

            bool hedefli = need != int.MaxValue;
            if (hedefli && (need <= 0 || need > n)) return null;     // kontrat tamam ya da imkânsız: sezgisel oyun yeter
            if (n > (hedefli ? EnFazlaElKartHedefli : EnFazlaElKart)) return null;

            int koz = b.Koz == null ? -1 : RenkNo(b.Koz);

            // İhale kısıtları (yalnız gizli iki koltuk için)
            Bidding.KoltukYorumu[] kis = new Bidding.KoltukYorumu[4];
            bool kisitVar = false;
            if (IhaleKullan && b.IhaleGecmisi != null && b.IhaleGecmisi.Count > 0 && !string.IsNullOrEmpty(b.IhaleBaslangici))
            {
                try
                {
                    var an = Bidding.IhaleAnalizi.Analiz(b.IhaleGecmisi, b.IhaleBaslangici, b.AnlasmaOf);
                    for (int i = 0; i < 2; i++)
                    {
                        Bidding.KoltukYorumu y;
                        if (an.TryGetValue(koltuk[gizli[i]], out y) && y.KonustuMu) { kis[gizli[i]] = y; kisitVar = true; }
                    }
                }
                catch (Exception) { kisitVar = false; kis = new Bidding.KoltukYorumu[4]; }
            }

            // Adaylar
            int[] aday = legal.Select(c => Kod(c)).ToArray();
            double[] toplam = new double[aday.Length];
            int[] basari = new int[aday.Length];

            var unseenArr = unseen.ToArray();
            var cozucu = new DDSolver(koz);
            var sw = Stopwatch.StartNew();
            int ornek = 0, deneme = 0;
            while (ornek < EnCokOrnek && deneme < EnCokOrnek * 20)
            {
                deneme++;
                int enAz = n >= 9 ? Math.Min(EnAzOrnek, 4) : EnAzOrnek;
                if (ornek >= enAz && sw.ElapsedMilliseconds > ButceMs) break;
                if (sw.ElapsedMilliseconds > SertSinirMs) break;
                ulong a, bb;
                bool uretildi = false, uydu = !kisitVar;
                a = 0; bb = 0;
                for (int t = 0; t < (kisitVar ? KisitDenemeSiniri : 1); t++)
                {
                    if (!DunyaOrnekleyici.Uret(Rnd, unseenArr, kalan[gizli[0]], kalan[gizli[1]], bos[gizli[0]], bos[gizli[1]], out a, out bb))
                    { uretildi = false; break; }
                    uretildi = true;
                    if (!kisitVar) break;
                    if (Uygun(kis[gizli[0]], a, oynHcp[gizli[0]], oynUz[gizli[0]]) && Uygun(kis[gizli[1]], bb, oynHcp[gizli[1]], oynUz[gizli[1]]))
                    { uydu = true; break; }
                }
                if (!uretildi) continue;
                if (kisitVar) { IstatKisitli++; if (!uydu) IstatKisitAsildi++; }
                ulong[] eller = new ulong[4];
                eller[0] = el[0]; eller[gorunenSeat] = el[gorunenSeat]; eller[gizli[0]] = a; eller[gizli[1]] = bb;

                cozucu.Temizle();
                int[] kartlar, degerler;
                if (hedefli) cozucu.Degerler(eller, leaderSeat, tc, tk, need, out kartlar, out degerler);
                else cozucu.Degerler(eller, leaderSeat, tc, tk, out kartlar, out degerler);
                var harita = new Dictionary<int, int>();
                for (int i = 0; i < kartlar.Length; i++) harita[kartlar[i]] = degerler[i];
                for (int i = 0; i < aday.Length; i++)
                {
                    int v;
                    if (!harita.TryGetValue(aday[i], out v)) continue;
                    toplam[i] += v;
                    if (v >= need) basari[i]++;
                }
                ornek++;
            }
            if (ornek < 1 || (n >= 9 && ornek < 3)) return null;

            // Puan: önce ihtimal, sonra ortalama
            double bestP = -1, bestA = -1;
            for (int i = 0; i < aday.Length; i++)
            {
                double p = need == int.MaxValue ? 0 : (double)basari[i] / ornek;
                double av = toplam[i] / ornek;
                if (p > bestP + 1e-9 || (Math.Abs(p - bestP) <= 1e-9 && av > bestA + 1e-9)) { bestP = p; bestA = av; }
            }
            var sonuc = new Sonuc { Ornek = ornek, Ihtimal = need == int.MaxValue ? -1 : bestP, OrtLove = bestA };
            for (int i = 0; i < aday.Length; i++)
            {
                double p = need == int.MaxValue ? 0 : (double)basari[i] / ornek;
                double av = toplam[i] / ornek;
                if (Math.Abs(p - bestP) <= 1e-9 && Math.Abs(av - bestA) <= 1e-9) sonuc.Eniyiler.Add(legal[i]);
            }
            sonuc.Aciklama = "Monte Carlo (" + ornek + " dağılım" +
                (need == int.MaxValue ? "" : ", hedef " + Math.Max(need, 0) + " löve: %" + (bestP * 100).ToString("0")) +
                ", ort. " + bestA.ToString("0.00") + " löve)";
            return sonuc;
        }

        /// <summary>Örneklenen elin (oynananlarla birlikte ilk 13 kart) ihale bilgisine uyup uymadığı. HCP aralığına 1 puan tolerans tanınır.</summary>
        private static bool Uygun(Bidding.KoltukYorumu y, ulong el, int oynananHcp, int[] oynananUz)
        {
            if (y == null) return true;
            int hcp = oynananHcp;
            for (int s = 0; s < 4; s++)
            {
                ulong m = (el >> (s * 16)) & 0x1FFFUL;
                int len = PopC(m) + oynananUz[s];
                if (len < y.MinUz[s]) return false;
                if (y.Dengeli && len < 2) return false;
                hcp += PopC(m & (1UL << 9)) + 2 * PopC(m & (1UL << 10)) + 3 * PopC(m & (1UL << 11)) + 4 * PopC(m & (1UL << 12));
            }
            if (hcp < y.HcMin - 1) return false;
            if (y.HcMax < 40 && (PasUstSiniriKullan || y.SonCagri != "Pas") && hcp > y.HcMax + HcUstTolerans) return false;
            return true;
        }

        private static int PopC(ulong x) { int c = 0; while (x != 0) { x &= x - 1; c++; } return c; }
    }
}
