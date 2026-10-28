using System;
using System.Collections.Generic;

namespace BricKartOyunu.Class
{
    /// <summary>Dağıtım profili (Oyun Planı > Dağıtım Kaynağı).</summary>
    public enum DagitimProfili { Rastgele, Parsiyel, Oyun, Slam, Kozsuz, KgOyun, KgDavet, KgSlam }

    /// <summary>
    /// Seed'e bağlı (tekrarlanabilir) dağıtım üretir. Profil ve "en iyi el" koşulu
    /// sağlanana dek aynı Random ile yeniden karıştırır; böylece aynı seed + aynı ayar
    /// her zaman aynı eli verir. Varsayılan (Rastgele, koşulsuz) ilk karıştırmayı aynen kullanır.
    /// Çıktı: 52 kart dosya adı, sıra Kuzey, Doğu, Güney, Batı (13'er kart; iç koltuklar).
    /// </summary>
    public static class DagitimUretici
    {
        private const int AzamiDeneme = 40000;
        private static readonly string[] Rank = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        private static readonly string[] Suit = { "S", "H", "D", "C" };

        public static List<string> Uret(DagitimProfili profil, bool[] enIyiIzinli, int seed)
        {
            var rng = new Random(seed);
            var kartlar = new List<string>(52);
            foreach (var s in Suit)
                foreach (var r in Rank)
                    kartlar.Add(r + s + ".png");

            bool kosulYok = profil == DagitimProfili.Rastgele && enIyiIzinli == null;
            List<string> son = null;
            for (int deneme = 0; deneme < (kosulYok ? 1 : AzamiDeneme); deneme++)
            {
                for (int i = kartlar.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    string t = kartlar[i]; kartlar[i] = kartlar[j]; kartlar[j] = t;
                }
                son = new List<string>(kartlar);
                if (kosulYok || Uygun(son, profil, enIyiIzinli)) return son;
            }
            return son; // koşul bulunamadıysa son karıştırma (pratikte olmaz)
        }

        /// <summary>El değeri: A=4 K=3 Q=2 J=1.</summary>
        public static int Hcp(string kart)
        {
            string rank = kart.Substring(0, kart.Length - 5);
            switch (rank)
            {
                case "A": return 4;
                case "K": return 3;
                case "Q": return 2;
                case "J": return 1;
                default: return 0;
            }
        }

        public static int ElHcp(List<string> deste, int koltuk)
        {
            int h = 0;
            for (int i = 0; i < 13; i++) h += Hcp(deste[koltuk * 13 + i]);
            return h;
        }

        /// <summary>4-3-3-3, 4-4-3-2 veya 5-3-3-2 dağılımı.</summary>
        public static bool Dengeli(List<string> deste, int koltuk)
        {
            var say = new Dictionary<char, int> { { 'S', 0 }, { 'H', 0 }, { 'D', 0 }, { 'C', 0 } };
            for (int i = 0; i < 13; i++)
            {
                string k = deste[koltuk * 13 + i];
                say[k[k.Length - 5]]++;
            }
            int ikili = 0;
            foreach (var c in say.Values)
            {
                if (c < 2 || c > 5) return false;
                if (c == 2) ikili++;
            }
            return ikili <= 1;
        }

        public static bool Uygun(List<string> deste, DagitimProfili profil, bool[] enIyiIzinli)
        {
            int[] h = new int[4];
            for (int k = 0; k < 4; k++) h[k] = ElHcp(deste, k);

            if (enIyiIzinli != null)
            {
                int en = 0;
                for (int k = 1; k < 4; k++) if (h[k] > h[en]) en = k;
                int adet = 0;
                for (int k = 0; k < 4; k++) if (h[k] == h[en]) adet++;
                if (adet != 1 || !enIyiIzinli[en]) return false;
            }

            // Koltuklar: 0 Kuzey, 1 Doğu, 2 Güney, 3 Batı → K-G ve D-B çiftleri
            int ns = h[0] + h[2], ew = h[1] + h[3];
            bool nsGuclu = ns >= ew;
            int guclu = nsGuclu ? ns : ew;
            int a = nsGuclu ? 0 : 1, b = nsGuclu ? 2 : 3;

            switch (profil)
            {
                // Güney-Kuzey (insan tarafı) güçlü: konvansiyon pratiği için yönlendirilmiş dağıtımlar
                case DagitimProfili.KgDavet: return ns >= 22 && ns <= 25 && ew <= 18;
                case DagitimProfili.KgOyun: return ns >= 25 && ns <= 31 && ew <= 14;
                case DagitimProfili.KgSlam: return ns >= 32 && ew <= 10;
                case DagitimProfili.Parsiyel: return guclu >= 20 && guclu <= 24;
                case DagitimProfili.Oyun: return guclu >= 25 && guclu <= 31;
                case DagitimProfili.Slam: return guclu >= 33;
                case DagitimProfili.Kozsuz:
                    return guclu >= 25 && guclu <= 32 && Dengeli(deste, a) && Dengeli(deste, b);
                default: return true;
            }
        }
    }
}
