using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Fit Cue-Bid (Rakip Müdahalesine Karşı).
    /// 
    /// Kural:
    /// - Partner 1 seviyesinde bir MAJÖR açtı (1♥ veya 1♠).
    /// - Rakip araya girdi (örneğin 1♠ veya 2♣).
    /// - Benim elimde 12+ HP ve partnerin rengine 3+ destek var.
    /// - Rakibin rengini cue-bid ederim (örneğin 2♠).
    /// 
    /// Anlamı: "Partnerim, benim 12+ HP'm var ve senin renginde fitim var.
    /// Manş (25+ HP) neredeyse garanti. Slam arayalım."
    /// 
    /// Örnek:
    ///   1♥ (Partner) - 1♠ (Rakip) - 2♠ (Ben, Cue-Bid) - Pas
    ///   Partner: 3♥ veya 4♥ veya 4NT (Blackwood) der.
    /// 
    /// Öncelik: 13 (CueBid'den önce, çünkü daha erken bağlam).
    /// </summary>
    public class FitCueBid : IKonvansiyon
    {
        public string Ad => "Fit Cue-Bid";
        public string Aciklama => "Rakip müdahalesine karşı fit cue-bid";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 13;  // CueBid (14) ve Blackwood (15)'ten önce

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. İhale bitmemiş olmalı
            if (durum.IhaleBittiMi()) return false;

            // 2. Partner açtı mı?
            if (!durum.PartnerActiMi()) return false;

            // 3. Rakip müdahale etti mi?
            if (!durum.RakipActiMi()) return false;

            // 4. Partnerin son teklifi 1 seviyesinde mi?
            var partnerSonTeklif = durum.PartnerTeklifleri.LastOrDefault();
            if (string.IsNullOrEmpty(partnerSonTeklif)) return false;
            int partnerSeviye = KozYardimcisi.TekliftenSeviyeCikar(partnerSonTeklif);
            if (partnerSeviye != 1) return false;

            // 5. Partnerin açtığı renk MAJÖR mü? (♥ veya ♠)
            string partnerKozu = KozYardimcisi.TekliftenKozCikar(partnerSonTeklif);
            if (partnerKozu != "Kupa" && partnerKozu != "Maça") return false;

            // 6. Benim elimde 3+ destek var mı?
            int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, partnerKozu);
            if (destek < 3) return false;

            // 7. Benim elimde 12+ HP var mı?
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 12) return false;

            // 8. Bu elde daha önce fit cue-bid yapılmış mı?
            // (Yani benim veya partnerin daha önce rakip rengi teklif etmiş mi?)
            var rakipKozu = RakipKozunuBul(durum);
            if (string.IsNullOrEmpty(rakipKozu)) return false;

            // Daha önce bu rakip rengi teklif edilmiş mi?
            bool dahaOnceTeklifEdildi = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .Any(h => h.Koz == rakipKozu);
            if (dahaOnceTeklifEdildi) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            string rakipKozu = RakipKozunuBul(durum);
            if (string.IsNullOrEmpty(rakipKozu)) return "Pas";

            string sembol = KozYardimcisi.KozSembolu(rakipKozu);

            // Rakibin rengini 2 seviyesinde cue-bid et
            // (Eğer rakip 1♠ dediyse, ben 2♠ derim)
            return "2" + sembol;
        }

        private string RakipKozunuBul(IhaleDurumu durum)
        {
            // Rakibin son gerçek teklifini bul
            var sonRakipTeklifi = durum.Gecmis
                .Where(h => h.GercekTeklifMi && durum.Rakipler.Contains(h.Oyuncu))
                .LastOrDefault();

            if (sonRakipTeklifi == null) return null;
            return sonRakipTeklifi.Koz;
        }
    }
}