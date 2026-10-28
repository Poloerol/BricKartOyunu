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

        /// <summary>
        /// Özdil "Ataklar" kitabındaki ek kurallar (Journalist, sanzatuda kart seçimi, ortağın rengi).
        /// Kapalıyken yalnızca As / Rua / Dam atağı kuralları çalışır.
        /// </summary>
        public static bool AtakKitabiAktif = true;

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
            if (Q && J && !A && !K && n >= 3 &&
                !(AtakKitabiAktif && n >= 4 && !T && !k.Any(c => c.Value == 9)))   // QJ75, QJ865: dördüncü kart
                return Olustur(renk, 12, "Dam atağı",
                    "Dam ile başlayan sekans / dış sekans",
                    "QJ10, QJ9, QJx, QJ109, QJ98...: dam atılır.");

            // --- AK (Q ve J yok) ---
            if (A && K && !Q && !J && (n == 3 || n == 4))
                return Olustur(renk, 14, "As atağı",
                    "Üçlü veya dörtlü, çok kuvvetli olmayan renk",
                    "AKx / AKxx: as atılır.");

            // --- Journalist metodu (Özdil, Sanzatu Kontratlarına Karşı Defans) ---
            if (AtakKitabiAktif)
            {
                bool dokuz = k.Any(c => c.Value == 9);

                // Vale atağı: J10 ile başlayan sekans (J109x, J10x, J10)
                if (J && T && !Q && !A && !K && n >= 2 && (n <= 3 || dokuz))   // J10x, J109x; J1063 gibi dörtlüde dördüncü kart
                    return Olustur(renk, 11, "Vale atağı",
                        "Vale ile başlayan sekans (J10x)",
                        "J10x / J109x: vale atılır (Journalist).");

                // Onlu atağı: üstte aralıklı onör olan iç sekans (AJ10x, KJ10x, A109x, K109x, Q109x, AQ109x ...)
                if (T && !Q_ve_J_sekansi(k) && n >= 4 && (A || K || Q) &&
                    ((J && !Q) || (dokuz && !J)))
                    return Olustur(renk, 10, "Onlu atağı",
                        "İç sekans: onlu, vale veya dokuzlu ile bağlı, üstte aralıklı onör var",
                        "AJ10x / KJ10x / A109x / K109x / Q109x: onlu atılır (Journalist).");
            }

            return null;
        }

        private static bool Q_ve_J_sekansi(List<Card> k)
        {
            return k.Any(c => c.Value == 12) && k.Any(c => c.Value == 11);
        }

        // ──────────────────────────────────────────────────────────────
        // SANZATU: kural kapsamında olmayan renklerde atılacak kart
        // ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Sanzatu kontratına karşı, seçilen renkten hangi kartın atılacağı (kitaptaki "Onörlü Uzunluklardan",
        /// "Onörsüz Uzunluklardan" ve "Ortağın Rengini Atak Etmek" bölümleri).
        /// </summary>
        public static Card SanzatuKarti(List<Card> renk, bool ortakRengi)
        {
            var k = renk.OrderByDescending(c => c.Value).ToList();
            int n = k.Count;
            if (n == 1) return k[0];
            if (ortakRengi) return OrtaginRengiKarti(k);

            bool dokuz = k.Any(c => c.Value == 9);
            bool sekiz = k.Any(c => c.Value == 8);
            bool onlu = k.Any(c => c.Value == 10);
            bool vale = k.Any(c => c.Value == 11);

            // Dokuzlu atağı (Journalist): 9x, 98x, 1094, J987, Q986, 9875, 10985
            if (AtakKitabiAktif && dokuz && k[0].Value <= 12 && !(vale && onlu) &&
                ((n == 2 && k[0].Value == 9) || (n >= 3 && (sekiz || (onlu && !vale)))))
                return k.First(c => c.Value == 9);

            bool onorlu = k[0].Value >= 10;
            if (onorlu)
            {
                bool bitisikUst = k[0].Value - k[1].Value == 1 && k[1].Value >= 10;
                if (n == 2) return k[0];
                if (n == 3) return bitisikUst ? k[0] : k[2];     // bitişik: tepeden; aralık/tek onör: üçüncü
                return k[3];                                       // dördüncü en iyi
            }

            // Onörsüz renkler
            if (n >= 5) return k[3];
            if (n == 4)
            {
                if (!dokuz) return k[0];                           // dokuzlu yoksa tepeden
                if (k[0].Value == 9 && sekiz) return k[0];         // 98.. sıralı: tepeden
                return k[1];                                       // dokuzludan sonra sekizli yoksa ikinci
            }
            return k[0];                                           // üç ve iki boş: tepeden
        }

        /// <summary>Ortağın rengini atak ederken kart seçimi.</summary>
        public static Card OrtaginRengiKarti(List<Card> k)
        {
            int n = k.Count;
            if (n == 1) return k[0];
            if (n == 2) return k[0];                                // ikili: üstteki / onör
            bool bitisikUst = k[0].Value - k[1].Value == 1 && k[0].Value >= 10;
            if (bitisikUst) return k[0];                            // AK5, KQ7, QJ6, J104, 1093
            bool icSekans = n >= 3 && k[1].Value - k[2].Value == 1 && k[1].Value >= 10 && k[0].Value >= 11;
            if (icSekans) return k[1];                              // A109 -> 10, AJ10 -> J
            if (n == 3) return k[2];                                // Hxx / aralık onörlü üçlü / üç boş: en küçük
            if (n == 4) return k[2];                                // Hxxx, dört boş: üçüncü
            return k[3];                                            // beş ve fazlası: dördüncü
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
