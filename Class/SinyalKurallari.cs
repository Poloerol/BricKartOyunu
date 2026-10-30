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
        public enum AtmaYontemi { Standart, UDCA, TekCift, Lavinthal }

        static readonly string[] Siralama = { "Sinek", "Karo", "Kupa", "Maça" }; // düşükten yükseğe

        public static AtmaYontemi Yontem(Bidding.OrtaklikAnlasmasi a)
        {
            if (a == null) return AtmaYontemi.Standart;
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
