using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Smolen Konvansiyonu.
    /// 
    /// Kural:
    /// - Partner 1NT açtı
    /// - Ben 2♣ (Stayman) dedim
    /// - Partner 2♦ dedi (4'lü majör YOK)
    /// - Ben 3♥ derim → "5♠ + 4♥"
    /// - Ben 3♠ derim → "5♥ + 4♠"
    /// 
    /// Amaç: 5-4 majör dağılımını göstermek. Partner NT'de
    /// doğru majörü seçebilir.
    /// 
    /// Öncelik: 26
    /// </summary>
    public class Smolen : IKonvansiyon
    {
        public string Ad => "Smolen";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 26;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // 1. Partner 1NT açtı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            if (durum.PartnerTeklifleri[0] != "1NT") return false;

            // 2. Ben Stayman 2♣ dedim mi?
            if (durum.KendiTeklifleri.Count == 0) return false;
            if (!durum.KendiTeklifleri.Contains("2♣")) return false;

            // 3. Son teklif partnerden mi ve 2♦ mı?
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;
            if (durum.SonGercekTeklif != "2♦") return false;

            // 4. El analizi: 5-4 majör dağılımı
            var el = durum.AktifOyuncuEli;
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

            // 5♠ + 4♥ VEYA 5♥ + 4♠
            bool besliMacaDortluKupa = (maca == 5 && kupa == 4);
            bool besliKupaDortluMaca = (kupa == 5 && maca == 4);

            if (!besliMacaDortluKupa && !besliKupaDortluMaca) return false;

            // 5. Yeterli puan (game forcing, 8+ HP)
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 8) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

            // 5♠ + 4♥ → 3♥ (yapay, "5♠ var" demek)
            if (maca == 5 && kupa == 4) return "3♥";

            // 5♥ + 4♠ → 3♠ (yapay, "5♥ var" demek)
            if (kupa == 5 && maca == 4) return "3♠";

            return "Pas";
        }
    }
}