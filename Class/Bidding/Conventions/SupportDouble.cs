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
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 18;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // 1. Partner 1♠ veya 1♥ açtı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            string partnerSon = durum.PartnerTeklifleri.Last();
            if (partnerSon != "1♠" && partnerSon != "1♥") return false;

            // 2. Rakip müdahale etti mi? (son teklif rakip mi?)
            if (durum.SonGercekTeklifSahibi == durum.Partner) return false;
            if (!durum.RakipActiMi()) return false;

            // 3. Rakip 1 veya 2 seviyesinde mi açtı?
            string rakipSon = durum.RakipTeklifleri.Last();
            if (rakipSon != "1♠" && rakipSon != "1♥" &&
                rakipSon != "1♦" && rakipSon != "1♣" &&
                rakipSon != "2♠" && rakipSon != "2♥" &&
                rakipSon != "2♦" && rakipSon != "2♣") return false;

            // 4. Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            // 5. Partnerin majöründe tam 3'lü destek var mı?
            string partnerKozu = partnerSon == "1♠" ? "Maça" : "Kupa";
            int destek = ElDegerlendirici.RenkUzunlugu(
                durum.AktifOyuncuEli, partnerKozu);

            // Support Double: TAM 3'lü destek (4+ ise direkt destek verilir)
            if (destek != 3) return false;

            // 6. En az 6 HP
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 6) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "Dbl";
        }
    }
}