using System;
using System.Collections.Generic;

namespace BricKartOyunu.Class
{
    /// <summary>Bir bölünmenin olasılığı: dışardaki kartlar Buyuk-Kucuk olarak bölünür (iki rakip arasında yer fark etmez).</summary>
    public sealed class BolunmeOlasiligi
    {
        public int Buyuk;
        public int Kucuk;
        /// <summary>0..1</summary>
        public double Olasilik;
    }

    /// <summary>
    /// Dağılım olasılıkları (Melih Özdil, "Kart Oyun Tekniği: Dağılımlar"). Tablolar kitaptaki yüzdelerdir;
    /// hesaplamalar kesin (hipergeometrik) olup tablolar bu hesapla doğrulanır.
    /// </summary>
    public static class DagilimOlasiliklari
    {
        /// <summary>C(n, k); geçersizse 0.</summary>
        public static double Kombinasyon(int n, int k)
        {
            if (k < 0 || n < 0 || k > n) return 0;
            if (k > n - k) k = n - k;
            double r = 1;
            for (int i = 1; i <= k; i++) r = r * (n - k + i) / i;
            return r;
        }

        /// <summary>
        /// Dışarıdaki k kartın belirli a tanesinin (belirli bir alt küme) solda olma olasılığı.
        /// solBos / sagBos: rakiplerin elinde, bu kartların dışında kalan bilinmeyen yer sayıları değil,
        /// ellerinde kalan TOPLAM bilinmeyen kart sayısıdır (tüm renkler).
        /// </summary>
        public static double AltKumeOlasiligi(int k, int a, int solKart, int sagKart)
        {
            int m = solKart + sagKart;
            if (a < 0 || a > k || a > solKart || (k - a) > sagKart) return 0;
            return Kombinasyon(m - k, solKart - a) / Kombinasyon(m, solKart);
        }

        /// <summary>Solda tam a kart olma olasılığı (k kartın hangileri olduğuna bakılmaksızın).</summary>
        public static double SoldaTamAKart(int k, int a, int solKart, int sagKart)
        {
            return Kombinasyon(k, a) * AltKumeOlasiligi(k, a, solKart, sagKart);
        }

        /// <summary>Dışarıda k kart varken bölünmelerin olasılıkları (rakiplerin elleri eşit, 13-13 kabul edilir). Büyük bölünmeden küçüğe.</summary>
        public static List<BolunmeOlasiligi> Bolunmeler(int k)
        {
            return Bolunmeler(k, 13, 13);
        }

        public static List<BolunmeOlasiligi> Bolunmeler(int k, int solKart, int sagKart)
        {
            var sonuc = new List<BolunmeOlasiligi>();
            for (int kucuk = k / 2; kucuk >= 0; kucuk--)
            {
                int buyuk = k - kucuk;
                double p = SoldaTamAKart(k, buyuk, solKart, sagKart);
                if (buyuk != kucuk) p += SoldaTamAKart(k, kucuk, solKart, sagKart);
                sonuc.Add(new BolunmeOlasiligi { Buyuk = buyuk, Kucuk = kucuk, Olasilik = p });
            }
            // Dengeli bölünme (3-2, 4-3 ...) önce gelsin
            sonuc.Sort((x, y) => y.Olasilik.CompareTo(x.Olasilik));
            return sonuc;
        }

        /// <summary>Dışardaki k kartın bir rakipte tam h kart olma olasılığı (iki rakipten belirli birinde).</summary>
        /// <summary>Dışardaki k kart içindeki belirli bir onörün, iki rakipten herhangi birinde, o rakibin toplam h kartı olması olasılığı.</summary>
        /// <summary>Bir onör, belirli bir rakipte (örn. solda) ve o rakibin h kartı olma olasılığı.</summary>
        // ───────────────────────── Kitap tabloları ─────────────────────────

        /// <summary>Kitaptaki "Dışardaki kartların dağılımı" tablosu: {dışarıdaki kart, büyük, küçük, yüzde}.</summary>
        public static readonly double[][] KitapBolunmeTablosu = new double[][]
        {
            new double[] { 2, 1, 1, 52 }, new double[] { 2, 2, 0, 48 },
            new double[] { 3, 2, 1, 78 }, new double[] { 3, 3, 0, 22 },
            new double[] { 4, 3, 1, 50 }, new double[] { 4, 2, 2, 40 }, new double[] { 4, 4, 0, 10 },
            new double[] { 5, 3, 2, 68 }, new double[] { 5, 4, 1, 28 }, new double[] { 5, 5, 0, 4 },
            new double[] { 6, 4, 2, 48 }, new double[] { 6, 3, 3, 36 }, new double[] { 6, 5, 1, 14.5 }, new double[] { 6, 6, 0, 1.5 },
            new double[] { 7, 4, 3, 62 }, new double[] { 7, 5, 2, 30.5 }, new double[] { 7, 6, 1, 7 }, new double[] { 7, 7, 0, 0.5 },
            new double[] { 8, 5, 3, 47 }, new double[] { 8, 4, 4, 33 }, new double[] { 8, 6, 2, 17 }, new double[] { 8, 7, 1, 2.9 }, new double[] { 8, 8, 0, 0.1 },
            new double[] { 9, 5, 4, 59 }, new double[] { 9, 6, 3, 31.5 }, new double[] { 9, 7, 2, 8.5 }, new double[] { 9, 8, 1, 1 }, new double[] { 9, 9, 0, 0.05 },
            new double[] { 10, 6, 4, 46.2 }, new double[] { 10, 5, 5, 31.1 }, new double[] { 10, 7, 3, 18.5 }, new double[] { 10, 8, 2, 3.8 }, new double[] { 10, 9, 1, 0.35 }, new double[] { 10, 10, 0, 0.01 },
        };

        /// <summary>13 kartlık el dağılımları (sıralı: en uzun renkten en kısaya) ve kitaptaki yüzdeleri.</summary>
        public static readonly int[][] KitapElDagilimlari = new int[][]
        {
            new[] {4,4,3,2}, new[] {5,3,3,2}, new[] {5,4,3,1}, new[] {5,4,2,2}, new[] {4,3,3,3}, new[] {6,3,2,2},
            new[] {6,4,2,1}, new[] {6,3,3,1}, new[] {5,5,2,1}, new[] {4,4,4,1}, new[] {7,3,2,1}, new[] {6,4,3,0},
            new[] {5,4,4,0}, new[] {5,5,3,0}, new[] {6,5,1,1}, new[] {6,5,2,0}, new[] {7,2,2,2}, new[] {7,4,1,1},
            new[] {7,4,2,0}, new[] {7,3,3,0},
        };
        public static readonly double[] KitapElDagilimYuzdeleri =
        {
            21.55, 15.52, 12.93, 10.58, 10.54, 5.64, 4.70, 3.45, 3.17, 2.99, 1.88, 1.33, 1.24, 0.90, 0.71, 0.65, 0.51, 0.39, 0.36, 0.27
        };

        /// <summary>13 kartlık elin dağılım kalıbının (renk uzunlukları, sırası önemsiz) kesin olasılığı (0..1).</summary>
        /// <summary>İki yapılacak empastan kaçı, "olanak" kadar empas fırsatından "gereken" tanesinin tutması olasılığı (her biri %50).</summary>
        public static double EmpasSansi(int olanak, int gereken)
        {
            if (gereken <= 0) return 1;
            if (gereken > olanak) return 0;
            double toplam = 0;
            for (int i = gereken; i <= olanak; i++) toplam += Kombinasyon(olanak, i);
            return toplam / Math.Pow(2, olanak);
        }
    }
}
