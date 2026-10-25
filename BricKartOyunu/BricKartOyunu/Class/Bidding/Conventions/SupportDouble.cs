using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Support Double Konvansiyonu.
    /// 
    /// Kural:
    /// - Partner 1♠/1♥ açtı
    /// - Rakip 1 seviyesinde müdahale etti (1♠/1♥/1♦/1♣)
    /// - Ben Dbl derim → "Partnerin majöründe 3'lü desteğim var"
    /// 
    /// Amaç: 3'lü desteği göstermek (2 seviyesi destek 4'lü gösterir).
    /// 
    /// Örnek:
    ///   1♥ (Partner) - 1♠ (Rakip) - Dbl (Ben) = 3'lü ♥ desteği
    ///   1♠ (Partner) - 2♣ (Rakip) - Dbl (Ben) = 3'lü ♠ desteği
    /// 
    /// Öncelik: 18
    /// </summary>
    public class SupportDouble : IKonvansiyon
    {
        public string Ad => "Support Double";
        public string Aciklama => "Üçlü majör desteğini gösteren kontr";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 18;

        private static int Sira(string koz)
        {
            switch (koz) { case "Sinek": return 1; case "Karo": return 2; case "Kupa": return 3; case "Maça": return 4; }
            return 0;
        }

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Support Double'ı AÇICI yapar (cevapçının kontrü Negatif Kontr'dür):
            // 1. Ben açtım (tek gerçek teklifim 1 seviyesinde bir renk)
            var benim = durum.Gecmis.Where(h => h.Oyuncu == durum.AktifOyuncu && h.GercekTeklifMi).ToList();
            if (benim.Count != 1 || benim[0].Seviye != 1 || benim[0].Koz == "NT") return false;
            if (durum.Gecmis.Any(h => h.Oyuncu == durum.AktifOyuncu && h.KontrMi)) return false;

            // 2. Ortak yeni bir MAJÖR renk söyledi (1 seviyesinde, tek gerçek teklifi)
            var ortak = durum.Gecmis.Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi).ToList();
            if (ortak.Count != 1 || ortak[0].Seviye != 1) return false;
            string partnerKozu = ortak[0].Koz;
            if (partnerKozu != "Maça" && partnerKozu != "Kupa") return false;
            if (partnerKozu == benim[0].Koz) return false;

            // 3. Rakip renk söyleyerek araya girdi, ben henüz cevap vermedim
            var son = durum.Gecmis.Where(h => h.GercekTeklifMi).LastOrDefault();
            if (son == null || !durum.Rakipler.Contains(son.Oyuncu)) return false;
            if (son.Koz == "NT" || son.Seviye > 2) return false;
            if (son.Sira < ortak[0].Sira) return false;
            // 2 seviyesinde müdahalede ortağın majörünü 2 seviyesinde destekleyebilmeliyim
            if (son.Seviye == 2 && Sira(son.Koz) > Sira(partnerKozu)) return false;

            // 4. Ortağın majöründe TAM 3'lü destek (4+ ise doğrudan destek verilir)
            int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, partnerKozu);
            return destek == 3;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "Dbl";
        }
    }
}