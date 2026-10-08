using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Support Double Konvansiyonu (SAYC Standard).
    ///
    /// Kural:
    /// - Partner 1♠ veya 1♥ açtı.
    /// - Rakip müdahale etti (1 veya 2 seviyesinde).
    /// - Ben Dbl diyerek "Partnerin majöründe TAM 3'lü desteğim var" mesajı veririm.
    ///
    /// Amaç: 3'lü desteği göstermek. (4+ destek varsa direkt destek teklif edilir).
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

            // 1. Partner 1 majör açmış olmalı
            if (durum.PartnerTeklifleri.Count == 0) return false;
            string partnerSon = durum.PartnerTeklifleri.Last();
            if (partnerSon != "1♠" && partnerSon != "1♥") return false;

            // 2. Rakip müdahale etmiş olmalı (son teklif rakibe ait)
            if (durum.SonGercekTeklifSahibi != durum.Partner)
            {
                // Rakip konuşmuş, kontrol et
                if (!durum.RakipActiMi()) return false;
            }
            else
            {
                // Son teklif partnerin ise, rakip araya girmiş mi?
                if (!durum.RakipActiMi()) return false;
            }

            // 3. Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            // 4. Partnerin majöründe TAM 3'lü destek var mı?
            string partnerKozu = partnerSon == "1♠" ? "Maça" : "Kupa";
            int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, partnerKozu);

            // Support Double kuralı: TAM 3 kart.
            // 4+ kart varsa direkt destek (2♠/2♥) verilir.
            if (destek != 3) return false;

            // 5. El gücü kontrolü (Genellikle 6-12 HP arası)
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
