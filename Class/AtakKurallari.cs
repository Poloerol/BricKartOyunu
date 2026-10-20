using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Sıralı onör ve markalardan açılış ataklarının (As / Rua / Dam atağı) kural motoru.
    /// Kural kapsamında olmayan dağılımlarda null döner; çağıran taraf başka stratejiye geçer.
    /// </summary>
    public static class AtakKurallari
    {
        /// <summary>8, 9, 10 "marka" sayılır (KQ108, AKJ9, AKQ9 ...).</summary>
        public const int YuksekMarkaEsigi = 8;

        public class AtakOnerisi
        {
            public Card Kart { get; set; }
            public string Kural { get; set; }     // "As atağı", "Rua atağı", "Dam atağı"
            public string Gosterir { get; set; }  // Ortağa verilen mesaj
            public string Aciklama { get; set; }  // Neden bu kart
        }

        /// <summary>
        /// Eldeki tüm renkleri tarar, kurala giren renkler için öneri döndürür.
        /// </summary>
        public static List<AtakOnerisi> AdaylariBul(List<Card> el)
        {
            var sonuc = new List<AtakOnerisi>();
            if (el == null) return sonuc;

            foreach (var grup in el.GroupBy(c => c.Suit))
            {
                var oneri = RenkAtagi(grup.ToList());
                if (oneri != null) sonuc.Add(oneri);
            }
            return sonuc;
        }

        /// <summary>
        /// Tek bir renk (aynı renkten kartlar) için atak önerisi. Kapsam dışıysa null.
        /// </summary>
        public static AtakOnerisi RenkAtagi(List<Card> renk)
        {
            if (renk == null || renk.Count == 0) return null;

            var k = renk.OrderByDescending(c => c.Value).ToList();
            int n = k.Count;

            bool A = k.Any(c => c.Value == 14);
            bool K = k.Any(c => c.Value == 13);
            bool Q = k.Any(c => c.Value == 12);
            bool J = k.Any(c => c.Value == 11);
            bool T = k.Any(c => c.Value == 10);

            int dorduncu = n >= 4 ? k[3].Value : 0;
            bool dorduncuKucuk = n >= 4 && dorduncu < YuksekMarkaEsigi;

            // --- AKQ ---
            if (A && K && Q)
            {
                if (n == 4 && dorduncuKucuk)
                    return Olustur(renk, 12, "Dam atağı", "Dam ile başlayan sekans",
                        "AKQx: ortağın debloke olmasını önlemek için dam atılır.");
                if (n >= 4)
                    return Olustur(renk, 13, "Rua atağı",
                        "Dörtlü veya daha uzun, en az üç onörlü sekans",
                        "AKQ + marka/uzunluk: rua atılır.");
                return null; // AKQ üçlü: kapsam dışı
            }

            // --- KQJ (dörtlü veya daha uzun) ---
            if (K && Q && J && !A && n >= 4)
                return Olustur(renk, 13, "Rua atağı",
                    "Dörtlü veya daha uzun, en az üç onörlü sekans",
                    "KQJ sekansı, dörtlü veya daha uzun.");

            // --- AKJ ---
            if (A && K && J && !Q)
            {
                if (n >= 5 || (n == 4 && dorduncu >= YuksekMarkaEsigi))
                    return Olustur(renk, 13, "Rua atağı",
                        "Dörtlü veya daha uzun, en az üç onörlü sekans/dış sekans",
                        "AKJ + marka veya beşli: rua atılır (AKJ10, AKJ9, AKJ63).");
                if (n == 3 || n == 4)
                    return Olustur(renk, 14, "As atağı",
                        "Üçlü veya dörtlü, çok kuvvetli olmayan renk",
                        "AKJ / AKJx: as atılır.");
                return null;
            }

            // --- AQJ (dörtlü veya daha uzun) ---
            if (A && Q && J && !K && n >= 4)
                return Olustur(renk, 12, "Dam atağı",
                    "Dam ile başlayan sekans / dış sekans",
                    "AQJ10x, AQJxx: dam atılır.");

            // --- KQ10 / KQx ---
            if (K && Q && !A && !J)
            {
                if (T)
                {
                    if (n == 3)
                        return Olustur(renk, 12, "Dam atağı",
                            "Dam ile başlayan sekans / iç sekans",
                            "KQ10: ortağı bulmak ümidiyle dam atılır.");
                    if (n == 4 && dorduncuKucuk)
                        return Olustur(renk, 12, "Dam atağı",
                            "Dam ile başlayan sekans / iç sekans",
                            "KQ10x: ortağın debloke olmasını önlemek için dam atılır.");
                    if (n >= 4)
                        return Olustur(renk, 13, "Rua atağı",
                            "Dörtlü veya daha uzun, en az üç onörlü sekans",
                            "KQ10 + marka veya beşli: rua atılır (KQ108, KQ109, KQ1052).");
                }
                else if (n == 3)
                {
                    return Olustur(renk, 12, "Dam atağı",
                        "Dam ile başlayan sekans",
                        "KQx: ortağı bulmak ümidiyle dam atılır.");
                }
                return null;
            }

            // --- QJ (A ve K yok) ---
            if (Q && J && !A && !K && n >= 3)
                return Olustur(renk, 12, "Dam atağı",
                    "Dam ile başlayan sekans / dış sekans",
                    "QJ10, QJ9, QJx, QJ109, QJ98...: dam atılır.");

            // --- AK (Q ve J yok) ---
            if (A && K && !Q && !J && (n == 3 || n == 4))
                return Olustur(renk, 14, "As atağı",
                    "Üçlü veya dörtlü, çok kuvvetli olmayan renk",
                    "AKx / AKxx: as atılır.");

            return null;
        }

        private static AtakOnerisi Olustur(List<Card> renk, int deger, string kural,
                                           string gosterir, string aciklama)
        {
            return new AtakOnerisi
            {
                Kart = renk.First(c => c.Value == deger),
                Kural = kural,
                Gosterir = gosterir,
                Aciklama = aciklama
            };
        }
    }
}
