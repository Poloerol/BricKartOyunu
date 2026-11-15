using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Savunma işaretleşme kuralları (saf fonksiyonlar, test edilebilir).
    /// Tek-Çift (Odd/Even), Lavinthal ve UDCA atma seçimi; Smith echo ve Lightner atağı.
    /// </summary>
    public static class SinyalKurallari
    {
        public enum AtmaYontemi { Standart, UDCA, TekCift, Lavinthal, Romen }

        static readonly string[] Siralama = { "Sinek", "Karo", "Kupa", "Maça" }; // düşükten yükseğe

        public static AtmaYontemi Yontem(Bidding.OrtaklikAnlasmasi a)
        {
            if (a == null) return AtmaYontemi.Standart;
            if (a.RomanDiscards) return AtmaYontemi.Romen;
            if (a.OddEvenDiscards) return AtmaYontemi.TekCift;
            if (a.Lavinthal) return AtmaYontemi.Lavinthal;
            if (a.UDCA) return AtmaYontemi.UDCA;
            return AtmaYontemi.Standart;
        }

        /// <summary>
        /// Atılacak rengin (istenmeyen) içinden hangi kartın atılacağını seçer.
        /// kartlar: o renkteki kartlar (sıra önemsiz). tercihRengi: ortağın tercih etmesini istediğim renk (yoksa null).
        /// Atılan kart ilgisizliği (istemiyorum) bildirir; Lavinthal'da ayrıca tercih rengini gösterir.
        /// </summary>
        public static Card AtmaKarti(List<Card> kartlar, AtmaYontemi y, string tercihRengi, string atilanRenk)
        {
            var c = kartlar.OrderBy(k => k.Value).ToList();
            if (c.Count == 1) return c[0];
            switch (y)
            {
                case AtmaYontemi.UDCA:
                    // Düşük = istiyorum, yüksek = istemiyorum → istemediğim renkte onör olmayan en yüksek
                    return c.Where(k => k.Value <= 9).OrderByDescending(k => k.Value).FirstOrDefault() ?? c[0];
                case AtmaYontemi.TekCift:
                    // Çift = istemiyorum (yüksek çift: yüksek renk tercihi), tek = istiyorum
                    {
                        var ciftler = c.Where(k => k.Value % 2 == 0 && k.Value <= 10).ToList();
                        if (ciftler.Count == 0) return c[0];
                        bool yuksek = tercihRengi != null && Array.IndexOf(Siralama, tercihRengi) > Array.IndexOf(Siralama, atilanRenk);
                        return yuksek ? ciftler[ciftler.Count - 1] : ciftler[0];
                    }
                case AtmaYontemi.Lavinthal:
                    {
                        // Yüksek kart = diğer renklerin yükseği, düşük kart = alçağı
                        var orta = c.Where(k => k.Value <= 10).ToList();
                        if (orta.Count == 0) return c[0];
                        var digerler = Siralama.Where(s => s != atilanRenk).ToList();
                        bool yuksek = tercihRengi != null && Array.IndexOf(Siralama, tercihRengi) >= Array.IndexOf(Siralama, digerler[digerler.Count - 1]);
                        return yuksek ? orta[orta.Count - 1] : orta[0];
                    }
                default:
                    return c[0];
            }
        }

        private static bool TekKart(int v) { return v == 3 || v == 5 || v == 7 || v == 9; }

        /// <summary>
        /// Romen atma (Roman discards): atılan kart ortağa hangi rengi oynamasını istediğimi söyler.
        ///   Tek kart (3, 5, 7, 9) = atılan rengin kendisi.
        ///   Küçük çift (2, 4)     = koz ve atılan kartın rengi dışındaki renklerden KÜÇÜK rütbeli olan (♣ < ♦ < ♥ < ♠).
        ///   Büyük çift (6, 8)     = koz ve atılan kartın rengi dışındaki renklerden BÜYÜK rütbeli olan.
        /// Onörler (10, J, Q, K, A) sinyal kartı sayılmaz.
        /// atmaSirasi: atılacak renk adayları (en uygun atma rengi önde). yanRenkler: koz dışındaki kartlar.
        /// Uygun kart bulunamazsa null (çağıran standart seçime döner).
        /// </summary>
        public static Card RomenAtma(List<string> atmaSirasi, List<Card> yanRenkler, string kozRengi, Func<Card, int> hcp, out string aciklama)
        {
            aciklama = null;
            if (atmaSirasi == null || yanRenkler == null || yanRenkler.Count == 0) return null;
            if (hcp == null) hcp = k => k.Value >= 11 ? k.Value - 10 : 0;

            // 1) Tek kart: onör dizisi olan, 4+ kartlık bir renkten küçük tek kart atılırsa o rengi oynamasını isterim.
            string iste = null; int istePuan = -1;
            foreach (var g in yanRenkler.GroupBy(c => c.Suit))
            {
                if (g.Count() >= 4 && g.Count(k => k.Value >= 11) >= 2 && g.Any(k => TekKart(k.Value)))
                {
                    int p = g.Sum(k => hcp(k));
                    if (p > istePuan) { istePuan = p; iste = g.Key; }
                }
            }
            if (iste != null)
            {
                var tek = yanRenkler.Where(k => k.Suit == iste && TekKart(k.Value)).OrderBy(k => k.Value).First();
                aciklama = "Romen atma: tek kart (" + tek.Value + ") = " + iste + " rengini oynayın";
                return tek;
            }

            // 2) Çift kart: kalan renkler arasında en güçlü olanı küçük (2, 4) ya da büyük (6, 8) çiftle göster.
            foreach (string renk in atmaSirasi)
            {
                var kartlar = yanRenkler.Where(k => k.Suit == renk).ToList();
                if (kartlar.Count == 0) continue;
                var kalan = Siralama.Where(s => s != renk && s != kozRengi).ToList();
                string tercih = kalan.Where(s => yanRenkler.Any(k => k.Suit == s))
                                     .OrderByDescending(s => yanRenkler.Where(k => k.Suit == s).Sum(k => hcp(k)))
                                     .FirstOrDefault();
                // büyük rütbeli renk istiyorsam büyük çift; yoksa (ya da tercih yoksa) küçük çift
                bool yuksek = tercih != null && kalan.Count >= 2 &&
                              Array.IndexOf(Siralama, tercih) == kalan.Max(s => Array.IndexOf(Siralama, s));
                int[] istenen = yuksek ? new[] { 8, 6 } : new[] { 2, 4 };
                Card kart = null;
                foreach (int v in istenen) { kart = kartlar.FirstOrDefault(k => k.Value == v); if (kart != null) break; }
                if (kart != null)
                {
                    aciklama = "Romen atma: " + (yuksek ? "büyük" : "küçük") + " çift (" + kart.Value + ") = " +
                               (yuksek ? "büyük" : "küçük") + " rütbeli rengi (koz ve atılan renk dışında) oynayın";
                    return kart;
                }
            }
            return null;
        }
        /// <summary>Smith echo: SA'da deklaran bir renge ilk kez oynayınca yüksek-düşük = ilk atağın rengini sürmeni isterim.</summary>
        public static Card SmithEcho(List<Card> kartlar, bool atakRengiIstiyorum, bool ilkKartMi)
        {
            var c = kartlar.OrderBy(k => k.Value).ToList();
            if (c.Count < 2) return c[0];
            // yüksek-düşük: önce yüksek (küçükler arasından), sonra düşük
            if (atakRengiIstiyorum) return ilkKartMi ? c[1] : c[0];
            return c[0];
        }

        /// <summary>Lightner: dummy'nin ilk (yapay olmayan) yan rengini seç; yoksa null.</summary>
        public static string LightnerRengi(List<string> gec, string baslangic, Player ortagim,
            Player dummy, Player deklaran, Func<Player, Bidding.OrtaklikAnlasmasi> anlasmaOf, string kozRengi)
        {
            Player bs;
            if (gec == null || !Enum.TryParse(baslangic, out bs)) return null;
            Player[] sira = { Player.Bati, Player.Kuzey, Player.Dogu, Player.Guney };
            int bas = Array.IndexOf(sira, bs);
            if (bas < 0) return null;
            for (int i = 0; i < gec.Count; i++)
            {
                string t = gec[i];
                if (string.IsNullOrEmpty(t) || t == "Pas" || t == "Dbl" || t == "RDbl" || t.EndsWith("NT")) continue;
                if (sira[(bas + i) % 4] != dummy) continue;
                if (Bidding.IhaleYorumcusu.Yorumla(gec, baslangic, i, anlasmaOf) != null) continue;
                string r = Ctx2.Renk(t[t.Length - 1]);
                if (r != null && r != kozRengi) return r;
            }
            return null;
        }

        static class Ctx2
        {
            public static string Renk(char ch)
            {
                switch (ch) { case '♠': return "Maça"; case '♥': return "Kupa"; case '♦': return "Karo"; case '♣': return "Sinek"; }
                return null;
            }
        }
    }
}
