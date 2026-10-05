using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// Briç elini değerlendiren yardımcı sınıf.
    /// 
    /// Bu sınıf şunları hesaplar:
    /// - HCP (High Card Points): A=4, K=3, Q=2, J=1
    /// - Dağılım puanı: void=3, singleton=2, doubleton=1
    /// - Toplam puan: HCP + Dağılım
    /// - Renk sayıları (her renkten kaç kart)
    /// - Dengeli/dengesiz el kontrolü
    /// - Majör/minör uzunlukları
    /// - Stopper (durdurucu) kontrolü
    /// 
    /// Kullanım:
    ///   var el = guneyEl;
    ///   int hcp = ElDegerlendirici.HCP(el);        // 15
    ///   bool dengeli = ElDegerlendirici.DengeliEl(el);  // true
    /// </summary>
    public static class ElDegerlendirici
    {
        // ═══════════════════════════════════════════════════════════════════
        // PUAN HESAPLAMA
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// HCP (High Card Points) hesaplar.
        /// As = 4, Papaz = 3, Kız = 2, Vale = 1
        /// </summary>
        public static int HCP(List<Card> el)
        {
            if (el == null) return 0;

            int puan = 0;
            foreach (var c in el)
            {
                switch (c.Value)
                {
                    case 14: puan += 4; break;  // As
                    case 13: puan += 3; break;  // Papaz
                    case 12: puan += 2; break;  // Kız
                    case 11: puan += 1; break;  // Vale
                }
            }
            return puan;
        }

        /// <summary>
        /// Dağılım puanı hesaplar.
        /// Void (şikan) = 3, Singleton = 2, Doubleton = 1
        /// </summary>
        public static int DagilimPuani(List<Card> el)
        {
            if (el == null) return 0;

            int puan = 0;
            var sayilar = RenkSayilari(el);

            foreach (var sayi in sayilar.Values)
            {
                if (sayi == 0) puan += 3;       // Void / Şikan
                else if (sayi == 1) puan += 2;  // Singleton
                else if (sayi == 2) puan += 1;  // Doubleton
            }

            return puan;
        }

        /// <summary>
        /// Toplam puan = HCP + Dağılım puanı.
        /// </summary>
        public static int ToplamPuan(List<Card> el)
        {
            return HCP(el) + DagilimPuani(el);
        }

        // ═══════════════════════════════════════════════════════════════════
        // RENK SAYILARI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Her renkten kaç kart olduğunu döndürür.
        /// Örnek: { "Maça": 5, "Kupa": 3, "Karo": 3, "Sinek": 2 }
        /// </summary>
        public static Dictionary<string, int> RenkSayilari(List<Card> el)
        {
            var sayilar = new Dictionary<string, int>
            {
                { "Maça", 0 },
                { "Kupa", 0 },
                { "Karo", 0 },
                { "Sinek", 0 }
            };

            if (el == null) return sayilar;

            foreach (var c in el)
            {
                if (sayilar.ContainsKey(c.Suit))
                    sayilar[c.Suit]++;
            }

            return sayilar;
        }

        /// <summary>
        /// Belirli bir renkten kaç kart olduğunu döndürür.
        /// </summary>
        public static int RenkUzunlugu(List<Card> el, string renk)
        {
            if (el == null || string.IsNullOrEmpty(renk)) return 0;
            return el.Count(c => c.Suit == renk);
        }

        /// <summary>
        /// En uzun rengi döndürür.
        /// </summary>
        public static string EnUzunRenk(List<Card> el)
        {
            var sayilar = RenkSayilari(el);
            return sayilar.OrderByDescending(kv => kv.Value).First().Key;
        }

        /// <summary>
        /// İkinci en uzun rengi döndürür.
        /// </summary>
        public static string IkinciEnUzunRenk(List<Card> el)
        {
            var sayilar = RenkSayilari(el);
            return sayilar.OrderByDescending(kv => kv.Value).Skip(1).First().Key;
        }

        /// <summary>
        /// Belirli bir renkte en az X kart var mı?
        /// </summary>
        public static bool RenkVar(List<Card> el, string renk, int minimumAdet)
        {
            return RenkUzunlugu(el, renk) >= minimumAdet;
        }

        // ═══════════════════════════════════════════════════════════════════
        // DAĞILIM KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// El dengeli mi?
        /// Dengeli dağılımlar: 4-3-3-3, 4-4-3-2, 5-3-3-2, 5-4-2-2, 6-3-2-2
        /// </summary>
        public static bool DengeliEl(List<Card> el)
        {
            if (el == null || el.Count != 13) return false;

            var sayilar = RenkSayilari(el).Values.OrderByDescending(x => x).ToArray();

            // 4-3-3-3
            if (sayilar[0] == 4 && sayilar[1] == 3 && sayilar[2] == 3 && sayilar[3] == 3)
                return true;

            // 4-4-3-2
            if (sayilar[0] == 4 && sayilar[1] == 4 && sayilar[2] == 3 && sayilar[3] == 2)
                return true;

            // 5-3-3-2
            if (sayilar[0] == 5 && sayilar[1] == 3 && sayilar[2] == 3 && sayilar[3] == 2)
                return true;

            // 5-4-2-2
            if (sayilar[0] == 5 && sayilar[1] == 4 && sayilar[2] == 2 && sayilar[3] == 2)
                return true;

            // 6-3-2-2
            if (sayilar[0] == 6 && sayilar[1] == 3 && sayilar[2] == 2 && sayilar[3] == 2)
                return true;

            return false;
        }

        /// <summary>
        /// Elde 5'li majör var mı? (Maça veya Kupa 5+)
        /// </summary>
        public static bool BesliMajorVar(List<Card> el)
        {
            return RenkUzunlugu(el, "Maça") >= 5 ||
                   RenkUzunlugu(el, "Kupa") >= 5;
        }

        /// <summary>
        /// Elde 4'lü majör var mı? (Maça veya Kupa 4+)
        /// </summary>
        public static bool DortluMajorVar(List<Card> el)
        {
            return RenkUzunlugu(el, "Maça") >= 4 ||
                   RenkUzunlugu(el, "Kupa") >= 4;
        }

        /// <summary>
        /// Elde 5'li minör var mı? (Karo veya Sinek 5+)
        /// </summary>
        public static bool BesliMinorVar(List<Card> el)
        {
            return RenkUzunlugu(el, "Karo") >= 5 ||
                   RenkUzunlugu(el, "Sinek") >= 5;
        }

        // ═══════════════════════════════════════════════════════════════════
        // STOPPER KONTROLÜ (NT için önemli)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Belirli bir renkte stopper (durdurucu) var mı?
        /// Stopper: As, Kx, QJx, J10xx, vs.
        /// Bu basit versiyon sadece As ve Kx kontrol eder.
        /// </summary>
        public static bool StopperVar(List<Card> el, string renk)
        {
            if (el == null) return false;

            var renkKartlari = el.Where(c => c.Suit == renk)
                                 .OrderByDescending(c => c.Value)
                                 .ToList();

            if (renkKartlari.Count == 0) return false;

            // As var mı?
            if (renkKartlari[0].Value == 14) return true;

            // Kx var mı? (Papaz + en az 1 kart)
            if (renkKartlari[0].Value == 13 && renkKartlari.Count >= 2) return true;

            // QJ var mı?
            if (renkKartlari.Count >= 2 &&
                renkKartlari[0].Value == 12 &&
                renkKartlari[1].Value == 11) return true;

            return false;
        }

        /// <summary>
        /// Elde kaç renkte stopper var? (0-4)
        /// </summary>
        public static int StopperSayisi(List<Card> el)
        {
            int sayi = 0;
            if (StopperVar(el, "Maça")) sayi++;
            if (StopperVar(el, "Kupa")) sayi++;
            if (StopperVar(el, "Karo")) sayi++;
            if (StopperVar(el, "Sinek")) sayi++;
            return sayi;
        }

        /// <summary>
        /// NT (No Trump) için el uygun mu?
        /// Dengeli + 3-4 stopper
        /// </summary>
        public static bool NTUygun(List<Card> el)
        {
            return DengeliEl(el) && StopperSayisi(el) >= 3;
        }

        // ═══════════════════════════════════════════════════════════════════
        // HIZLI EL SAYISI (QUICK TRICKS)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Hızlı el sayısı tahmini.
        /// As = 1, Kx = 0.5, KQ = 1, QJx = 0.5
        /// </summary>
        public static double HizliElSayisi(List<Card> el)
        {
            if (el == null) return 0;

            double sayi = 0;
            var renkler = new[] { "Maça", "Kupa", "Karo", "Sinek" };

            foreach (var renk in renkler)
            {
                var renkKartlari = el.Where(c => c.Suit == renk)
                                     .OrderByDescending(c => c.Value)
                                     .ToList();

                if (renkKartlari.Count == 0) continue;

                // As = 1
                if (renkKartlari[0].Value == 14) sayi += 1;

                // Kx = 0.5
                if (renkKartlari[0].Value == 13 && renkKartlari.Count >= 2) sayi += 0.5;
                else if (renkKartlari.Count >= 2 && renkKartlari[0].Value == 13) sayi += 0.5;

                // KQ = 1
                if (renkKartlari.Count >= 2 &&
                    renkKartlari[0].Value == 13 &&
                    renkKartlari[1].Value == 12) sayi += 0.5; // KQ toplam 1

                // QJx = 0.5
                if (renkKartlari.Count >= 2 &&
                    renkKartlari[0].Value == 12 &&
                    renkKartlari[1].Value == 11) sayi += 0.5;
            }

            return sayi;
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Elin özet metni (debug için).
        /// Örnek: "15 HCP, 6 TP, Dengeli, Maça:5 Kupa:3 Karo:3 Sinek:2"
        /// </summary>
        public static string Ozet(List<Card> el)
        {
            if (el == null) return "Boş el";

            var sayilar = RenkSayilari(el);
            string dagilim = $"Maça:{sayilar["Maça"]} Kupa:{sayilar["Kupa"]} " +
                            $"Karo:{sayilar["Karo"]} Sinek:{sayilar["Sinek"]}";

            string dengeli = DengeliEl(el) ? "Dengeli" : "Dengesiz";

            return $"{HCP(el)} HCP, {ToplamPuan(el)} TP, {dengeli}, {dagilim}";
        }
    }
}