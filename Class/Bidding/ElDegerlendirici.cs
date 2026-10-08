using System;
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
    /// - Dengeli/yarı-dengeli el kontrolü
    /// - Majör/minör uzunlukları
    /// - Stopper (durdurucu) kontrolü
    /// </summary>
    public static class ElDegerlendirici
    {
        // ═══════════════════════════════════════════════════════════════════
        // PUAN HESAPLAMA
        // ═══════════════════════════════════════════════════════════════════

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

        public static int ToplamPuan(List<Card> el)
        {
            return HCP(el) + DagilimPuani(el);
        }

        // ═══════════════════════════════════════════════════════════════════
        // RENK SAYILARI
        // ═══════════════════════════════════════════════════════════════════

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

        public static int RenkUzunlugu(List<Card> el, string renk)
        {
            if (el == null || string.IsNullOrEmpty(renk)) return 0;
            return el.Count(c => c.Suit == renk);
        }

        public static string EnUzunRenk(List<Card> el)
        {
            var sayilar = RenkSayilari(el);
            return sayilar.OrderByDescending(kv => kv.Value).First().Key;
        }

        public static string IkinciEnUzunRenk(List<Card> el)
        {
            var sayilar = RenkSayilari(el);
            return sayilar.OrderByDescending(kv => kv.Value).Skip(1).First().Key;
        }

        public static bool RenkVar(List<Card> el, string renk, int minimumAdet)
        {
            return RenkUzunlugu(el, renk) >= minimumAdet;
        }

        // ═══════════════════════════════════════════════════════════════════
        // DAĞILIM KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Tam dengeli dağılımlar: 4-3-3-3, 4-4-3-2, 5-3-3-2
        /// </summary>
        public static bool DengeliEl(List<Card> el)
        {
            if (el == null || el.Count != 13) return false;
            var s = RenkSayilari(el).Values.OrderByDescending(x => x).ToArray();

            return (s[0] == 4 && s[1] == 3 && s[2] == 3 && s[3] == 3) ||
                   (s[0] == 4 && s[1] == 4 && s[2] == 3 && s[3] == 2) ||
                   (s[0] == 5 && s[1] == 3 && s[2] == 3 && s[3] == 2);
        }

        /// <summary>
        /// Yarı-dengeli dağılımlar: 5-4-2-2, 6-3-2-2
        /// </summary>
        public static bool YariDengeliEl(List<Card> el)
        {
            if (el == null || el.Count != 13) return false;
            var s = RenkSayilari(el).Values.OrderByDescending(x => x).ToArray();

            return (s[0] == 5 && s[1] == 4 && s[2] == 2 && s[3] == 2) ||
                   (s[0] == 6 && s[1] == 3 && s[2] == 2 && s[3] == 2);
        }

        public static bool BesliMajorVar(List<Card> el)
        {
            return RenkUzunlugu(el, "Maça") >= 5 ||
                   RenkUzunlugu(el, "Kupa") >= 5;
        }

        public static bool DortluMajorVar(List<Card> el)
        {
            return RenkUzunlugu(el, "Maça") >= 4 ||
                   RenkUzunlugu(el, "Kupa") >= 4;
        }

        public static bool BesliMinorVar(List<Card> el)
        {
            return RenkUzunlugu(el, "Karo") >= 5 ||
                   RenkUzunlugu(el, "Sinek") >= 5;
        }

        // ═══════════════════════════════════════════════════════════════════
        // STOPPER KONTROLÜ (NT için gelişmiş yapı)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Belirli bir renkte stopper kontrolü yapar.
        /// Puanlama: As=3, Kx=2, KQ=2, QJ=1. Toplam >= 2 ise güçlü stopper.
        /// </summary>
        public static bool StopperVar(List<Card> el, string renk)
        {
            if (el == null) return false;

            var renkKartlari = el.Where(c => c.Suit == renk)
                                 .OrderByDescending(c => c.Value)
                                 .ToList();

            if (renkKartlari.Count == 0) return false;

            // En yüksek kart As ise kesin stopper
            if (renkKartlari[0].Value == 14) return true;

            // Diğer kombinasyonlar için puanlama
            int stopperPuan = 0;
            if (renkKartlari[0].Value == 13 && renkKartlari.Count >= 2) stopperPuan += 2; // Kx
            if (renkKartlari.Count >= 2 && renkKartlari[0].Value == 13 && renkKartlari[1].Value == 12) stopperPuan += 1; // KQ ekstra güç
            if (renkKartlari.Count >= 2 && renkKartlari[0].Value == 12 && renkKartlari[1].Value == 11) stopperPuan += 1; // QJ

            return stopperPuan >= 2;
        }

        public static int StopperSayisi(List<Card> el)
        {
            int sayi = 0;
            if (StopperVar(el, "Maça")) sayi++;
            if (StopperVar(el, "Kupa")) sayi++;
            if (StopperVar(el, "Karo")) sayi++;
            if (StopperVar(el, "Sinek")) sayi++;
            return sayi;
        }

        public static bool NTUygun(List<Card> el)
        {
            // Tam dengeli veya yarı-dengeli + yeterli stopper
            return (DengeliEl(el) || YariDengeliEl(el)) && StopperSayisi(el) >= 3;
        }

        // ═══════════════════════════════════════════════════════════════════
        // HIZLI EL SAYISI (Standard Quick Tricks Table)
        // ═══════════════════════════════════════════════════════════════════

        public static double HizliElSayisi(List<Card> el)
        {
            if (el == null) return 0;

            double toplamSayı = 0;
            var renkler = new[] { "Maça", "Kupa", "Karo", "Sinek" };

            foreach (var renk in renkler)
            {
                var c = el.Where(card => card.Suit == renk).OrderByDescending(card => card.Value).ToList();
                if (c.Count == 0) continue;

                // Standart Quick Trick Tablosu
                if (c[0].Value == 14) // As var
                {
                    if (c.Count < 2) toplamSayı += 1.0;
                    else if (c[1].Value == 13) toplamSayı += 2.0; // AK
                    else if (c[1].Value == 12) toplamSayı += 1.5; // AQ
                    else if (c[1].Value == 11) toplamSayı += 1.0; // AJ
                    else toplamSayı += 1.0;
                }
                else if (c[0].Value == 13) // Papaz en yüksek
                {
                    if (c.Count < 2) toplamSayı += 0.0;
                    else if (c[1].Value == 12) toplamSayı += 1.0; // KQ
                    else if (c[1].Value == 11) toplamSayı += 0.5; // KJ
                    else toplamSayı += 0.5; // Kx
                }
                else if (c[0].Value == 12) // Kız en yüksek
                {
                    if (c.Count < 2) toplamSayı += 0.0;
                    else if (c[1].Value == 11) toplamSayı += 0.5; // QJ
                    else toplamSayı += 0.0;
                }
            }

            return toplamSayı;
        }

        // ═══════════════════════════════════════════════════════════════════
        // AS / PAPAZ / KİLİT KART SAYIMI
        // ═══════════════════════════════════════════════════════════════════

        public static int AsSayisi(List<Card> el)
        {
            if (el == null) return 0;
            return el.Count(c => c.Value == 14);
        }

        public static int PapazSayisi(List<Card> el)
        {
            if (el == null) return 0;
            return el.Count(c => c.Value == 13);
        }

        public static int KontrolSayisi(List<Card> el)
        {
            return AsSayisi(el) + PapazSayisi(el);
        }

        public static int KilitKartSayisi(List<Card> el, string koz)
        {
            if (el == null) return 0;
            int sayi = AsSayisi(el);
            if (string.IsNullOrEmpty(koz) || koz == "NT") return sayi;
            if (el.Any(c => c.Suit == koz && c.Value == 13)) sayi++;
            return sayi;
        }

        public static bool PapazVar(List<Card> el, string renk)
        {
            if (el == null || string.IsNullOrEmpty(renk)) return false;
            return el.Any(c => c.Suit == renk && c.Value == 13);
        }

        public static bool KizVar(List<Card> el, string renk)
        {
            if (el == null || string.IsNullOrEmpty(renk)) return false;
            return el.Any(c => c.Suit == renk && c.Value == 12);
        }

        public static string Ozet(List<Card> el)
        {
            if (el == null) return "Boş el";
            var sayilar = RenkSayilari(el);
            string dagilim = $"Maça:{sayilar["Maça"]} Kupa:{sayilar["Kupa"]} " +
                            $"Karo:{sayilar["Karo"]} Sinek:{sayilar["Sinek"]}";
            string dengeli = DengeliEl(el) ? "Dengeli" : (YariDengeliEl(el) ? "Yarı-Dengeli" : "Dengesiz");
            return $"{HCP(el)} HCP, {ToplamPuan(el)} TP, {dengeli}, {dagilim}";
        }
    }
}
