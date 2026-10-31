using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// PIMC (Perfect Information Monte Carlo) kart seçimi.
    /// Görünmeyen kartlar iki gizli ele (kart sayıları ve renksiz kalma bilgisiyle tutarlı) rastgele dağıtılır;
    /// her "dünya" DDSolver ile çözülür ve her yasal kartın başarısı dünyalar üzerinden toplanır.
    /// Kontrat hedefi biliniyorsa ölçüt "tarafımın hedef löveye ulaşma olasılığı", bilinmiyorsa ortalama löve sayısıdır.
    /// Bilgi yetersizse, çözücü çok yavaş kalacaksa ya da kartlar arasında fark yoksa null döner
    /// ve OyunZekasi'nın sezgisel kuralları devreye girer (eşitlikte de sezgiselin seçimi korunur).
    /// </summary>
    public static class PimcOyuncu
    {
        public static bool Aktif = true;
        /// <summary>Bir karar için en çok üretilecek dünya sayısı.</summary>
        public static int DunyaSayisi = 40;
        /// <summary>Bir karar için zaman sınırı (ms). Dünyalar bu süre dolana dek üretilir.</summary>
        public static int ZamanSiniriMs = 900;
        /// <summary>Güvenilir karar için gereken en az dünya sayısı; altında null döner.</summary>
        public static int MinDunya = 6;
        /// <summary>Elimde bundan fazla kart varsa (ilk 3 el) çözücü çok yavaştır → sezgisel kurallar.</summary>
        public static int EnFazlaKalanEl = 10;
        /// <summary>Hedef yokken tam (ortalama löve) çözüm için en çok kalan el.</summary>
        public static int TamCozumKalanEl = 8;
        public static Random Rnd = new Random();
        public static string SonOzet = "";

        public static int Koltuk(Player p)
        {
            switch (p)
            {
                case Player.Kuzey: return 0;
                case Player.Dogu: return 1;
                case Player.Guney: return 2;
                default: return 3;
            }
        }

        private static Player KoltuktenOyuncu(int k)
        {
            switch (k & 3)
            {
                case 0: return Player.Kuzey;
                case 1: return Player.Dogu;
                case 2: return Player.Guney;
                default: return Player.Bati;
            }
        }

        private static int RenkNo(string suit) { return Array.IndexOf(OyunKurallari.Renkler, suit); }
        public static int Kod(Card c) { return RenkNo(c.Suit) * 16 + (c.Value - 2); }

        private static void Tara(List<(Player Oyuncu, Card Kart)> kartlar, Player A, Player B, bool[] bosA, bool[] bosB)
        {
            if (kartlar.Count == 0) return;
            int lead = RenkNo(kartlar[0].Kart.Suit);
            for (int i = 1; i < kartlar.Count; i++)
            {
                if (RenkNo(kartlar[i].Kart.Suit) == lead) continue;
                if (kartlar[i].Oyuncu == A) bosA[lead] = true;
                else if (kartlar[i].Oyuncu == B) bosB[lead] = true;
            }
        }

        /// <summary>
        /// legal: yasal kartlar; sezgisel: OyunZekasi'nın seçimi (eşitlikte tercih edilir, null olabilir).
        /// Dönüş: seçilen kart (legal içinden) ya da null (PIMC kullanılamadı / karar vermedi).
        /// </summary>
        public static Card Sec(OyunBaglami b, List<Card> legal, Card sezgisel, out string neden)
        {
            neden = "";
            SonOzet = "";
            try { return SecIc(b, legal, sezgisel, out neden); }
            catch (Exception ex)
            {
                Debug.WriteLine("[PIMC] hata: " + ex.Message);
                neden = "";
                return null;
            }
        }

        private static Card SecIc(OyunBaglami b, List<Card> legal, Card sezgisel, out string neden)
        {
            neden = "";
            if (!Aktif || b == null || b.Elim == null || legal == null || legal.Count < 2) return null;
            int kalan = b.Elim.Count;                       // kalan el sayısı (mevcut el dahil)
            if (kalan > EnFazlaKalanEl || kalan < 2) return null;
            if (b.Masa == null) b.Masa = new List<(Player Oyuncu, Card Kart)>();
            if (b.Gecmis == null) b.Gecmis = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>();

            Player ben = b.Ben;
            int bs = Koltuk(ben);
            Player ortak = OyunKurallari.Ortak(ben);
            Player dummy = OyunKurallari.Ortak(b.Deklaran);

            // Bilinen eller
            var bilinen = new Dictionary<Player, List<Card>>();
            bilinen[ben] = b.Elim;
            if (b.OrtakEli != null) bilinen[ortak] = b.OrtakEli;
            else if (b.DummyEli != null && dummy != ben) bilinen[dummy] = b.DummyEli;
            if (bilinen.Count != 2) return null;            // açılış atağı gibi: iki gizli el yetmez

            Func<Player, int> adet = p => 13 - b.ElNo - (b.Masa.Any(m => m.Oyuncu == p) ? 1 : 0);
            foreach (var kv in bilinen)
                if (kv.Value.Count != adet(kv.Key)) return null;

            var gizli = new List<Player>();
            foreach (Player p in new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati })
                if (!bilinen.ContainsKey(p)) gizli.Add(p);
            Player A = gizli[0], B = gizli[1];
            int adetA = adet(A), adetB = adet(B);

            // Görünmeyen kartlar
            var gorulen = new HashSet<int>();
            foreach (var kv in bilinen) foreach (var c in kv.Value) gorulen.Add(Kod(c));
            foreach (var m in b.Masa) gorulen.Add(Kod(m.Kart));
            foreach (var g in b.Gecmis) gorulen.Add(Kod(g.Kart));
            var unseen = new List<int>();
            for (int s = 0; s < 4; s++) for (int r = 0; r < 13; r++) { int k = s * 16 + r; if (!gorulen.Contains(k)) unseen.Add(k); }
            if (unseen.Count != adetA + adetB) return null;

            // Renksiz kalma bilgisi (renge uymayan gizli oyuncu)
            var bosA = new bool[4]; var bosB = new bool[4];
            foreach (var grp in b.Gecmis.GroupBy(g => g.ElNo))
                Tara(grp.Select(g => (g.Oyuncu, g.Kart)).ToList(), A, B, bosA, bosB);
            Tara(b.Masa, A, B, bosA, bosB);

            // Sıra bende mi?
            int lider = b.Masa.Count > 0 ? Koltuk(b.Masa[0].Oyuncu) : bs;
            int elSayisi = b.Masa.Count;
            if (((lider + elSayisi) & 3) != bs) return null;
            var elKartlari = new int[4];
            for (int i = 0; i < elSayisi; i++) elKartlari[i] = Kod(b.Masa[i].Kart);

            int koz = b.Koz == null ? -1 : RenkNo(b.Koz);
            bool benNS = (bs & 1) == 0;

            // Hedef (kontrat) bilgisi
            bool hedefVar = false; int gerek = 0;
            if (b.Seviye >= 1 && b.Seviye <= 7)
            {
                int dekEl = 0, savEl = 0;
                foreach (var g in b.Gecmis.Where(x => x.Kazandi))
                {
                    if (g.Oyuncu == b.Deklaran || g.Oyuncu == dummy) dekEl++; else savEl++;
                }
                bool dekTarafi = (ben == b.Deklaran || ben == dummy);
                gerek = dekTarafi ? (6 + b.Seviye - dekEl) : ((8 - b.Seviye) - savEl);
                hedefVar = gerek >= 1 && gerek <= kalan;
            }
            if (!hedefVar && kalan > TamCozumKalanEl) return null;
            int hedefNS = hedefVar ? (benNS ? gerek : kalan - gerek + 1) : -1;

            // Dünyaları üret ve çöz
            var toplam = new Dictionary<int, double>();
            int dunya = 0;
            var sw = Stopwatch.StartNew();
            var sabit = new ulong[4];
            foreach (var kv in bilinen) { ulong m = 0; foreach (var c in kv.Value) m |= 1UL << Kod(c); sabit[Koltuk(kv.Key)] = m; }
            int[] unseenArr = unseen.ToArray();

            while (dunya < DunyaSayisi && (dunya < MinDunya || sw.ElapsedMilliseconds < ZamanSiniriMs))
            {
                if (dunya >= 1 && sw.ElapsedMilliseconds > ZamanSiniriMs) break;
                ulong ea, eb;
                if (!DunyaOrnekleyici.Uret(Rnd, unseenArr, adetA, adetB, bosA, bosB, out ea, out eb)) return null;
                var eller = (ulong[])sabit.Clone();
                eller[Koltuk(A)] = ea; eller[Koltuk(B)] = eb;

                var cozucu = new DDSolver(koz);
                int[] kartlar, degerler;
                cozucu.Degerler(eller, lider, elKartlari, elSayisi, hedefNS, out kartlar, out degerler);
                for (int i = 0; i < kartlar.Length; i++)
                {
                    double puan;
                    int v = degerler[i];
                    if (hedefVar) puan = (benNS ? v >= hedefNS : v < hedefNS) ? 1.0 : 0.0;
                    else puan = benNS ? v : kalan - v;
                    double eski; toplam.TryGetValue(kartlar[i], out eski);
                    toplam[kartlar[i]] = eski + puan;
                }
                dunya++;
                if (sw.ElapsedMilliseconds > ZamanSiniriMs * 3) break;   // tek dünya çok yavaşsa kes
            }
            if (dunya < MinDunya) return null;

            // En iyi yasal kartlar
            double en = double.MinValue;
            var skor = new Dictionary<Card, double>();
            foreach (var c in legal)
            {
                double p;
                if (!toplam.TryGetValue(Kod(c), out p)) return null;   // tutarsızlık: güvenme
                skor[c] = p;
                if (p > en) en = p;
            }
            var iyiler = legal.Where(c => skor[c] >= en - 1e-9).ToList();
            if (iyiler.Count == legal.Count) return null;              // kartlar arasında fark yok → sezgisel karar versin

            Card secim;
            if (sezgisel != null && iyiler.Contains(sezgisel)) secim = sezgisel;
            else secim = iyiler.OrderBy(c => c.Value).First();

            string olcu = hedefVar
                ? "hedef " + gerek + " el: başarı %" + (int)Math.Round(100.0 * en / dunya)
                : "ort. " + (en / dunya).ToString("0.0") + " el";
            SonOzet = "PIMC: " + dunya + " dünya, " + olcu + " (" + sw.ElapsedMilliseconds + " ms)";
            neden = SonOzet;
            return secim;
        }
    }
}
