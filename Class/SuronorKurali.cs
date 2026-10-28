using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Süronör kuralı (Melih Özdil, "Sanzatu Kontratlarına Karşı Defans"): rakip bir onör oynadığında
    /// üstüne daha büyük onör koyup koymamak. Kural yalnızca kazanma ihtimali varsa uygulanır;
    /// "mantık bütün kuralların üstündedir".
    /// Kartlar renk içindeki sıra değerleridir (2-14).
    /// </summary>
    public static class SuronorKurali
    {
        /// <summary>Kapalıyken eski davranış (yalnızca sağlam kartla alma) kullanılır.</summary>
        public static bool Aktif = true;

        public const int Uygulanamaz = -1;   // kural bu durumu kapsamıyor
        public const int KucukVer = 0;       // süronör yapma, küçük ver

        /// <summary>
        /// benim: elimdeki o renk kartları;
        /// yer: yerin (dummy) o renkteki kartları (oynanan kart dahil olabilir);
        /// atilan: masadaki onör (10, vale, dam, rua);
        /// yerOynadi: onörü yer mi oynadı (true: yer sağımda; false: kapalı el sağımda, yer arkamda);
        /// gorunmeyen: ne bende ne yerde olan, oynanmamış kartlar (ortak + deklaran).
        /// Dönüş: süronör kartının değeri, KucukVer ya da Uygulanamaz.
        /// </summary>
        public static int Karar(IEnumerable<int> benim, IEnumerable<int> yer, int atilan, bool yerOynadi,
                                IEnumerable<int> gorunmeyen)
        {
            if (!Aktif) return Uygulanamaz;
            if (atilan < 10 || atilan > 13) return Uygulanamaz;       // yalnız onlu, vale, dam, rua üstüne

            var my = benim.OrderBy(v => v).ToList();
            var yerK = yer.Where(v => v != atilan).OrderBy(v => v).ToList();
            var gor = gorunmeyen.Where(v => v != atilan).ToList();

            var ustler = my.Where(v => v > atilan).ToList();
            if (ustler.Count == 0) return Uygulanamaz;                // geçecek kartım yok
            int ort = ustler[0];                                      // üstüne koyacağım en küçük kart

            if (yerOynadi)
            {
                // Yer sağımda. Yerde atılanın hemen altında bitişik bir onör/marka varsa (QJ.., J10.., 109..)
                // sonuncu onör beklenir; yalnız ikinci bir onörüm varsa ya da rengim ikiliyse hemen girilir.
                bool bitisikAlt = yerK.Contains(atilan - 1) && atilan - 1 >= 9;
                if (bitisikAlt)
                    return (ustler.Count >= 2 || my.Count <= 2) ? ort : KucukVer;
                return ort;                                           // tek / son onör: süronör yapılır
            }

            // Kapalı el sağımda (onör oradan oynandı), yer arkamda.
            int yerEnBuyuk = yerK.Count > 0 ? yerK.Max() : 0;
            if (yerEnBuyuk < atilan) return ort;                      // yerde daha büyük kart yok: kazanır

            int yerOnor = yerK.Count(v => v >= 11);                   // yerdeki onörler (A K Q J)
            int benOnor = my.Count(v => v >= 10);                     // bende 10 ve üstü
            bool aday = yerOnor >= 2 || benOnor >= 2;
            if (!aday) return KucukVer;                               // tek onör: ilk turda süronör yok

            // Mantık: kazancı olabilecek, yerde olmayan bir marka var mı?
            int yerEnKucuk = yerK.Count > 0 ? yerK.Min() : 0;
            bool kazanc = my.Any(v => v != ort && v >= 9 && v < atilan && !yerK.Contains(v) && v > yerEnKucuk) ||
                          gor.Any(v => v >= 9 && v < atilan && !yerK.Contains(v) && v > yerEnKucuk);
            return kazanc ? ort : KucukVer;
        }
    }
}
