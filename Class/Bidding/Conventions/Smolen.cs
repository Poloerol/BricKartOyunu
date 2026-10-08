using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Smolen Konvansiyonu (SAYC Standard).
    ///
    /// Kural:
    /// - Partner 1NT açtı -> Ben 2♣ (Stayman) dedim -> Partner 2♦ (Majör yok) dedi.
    /// - Ben 3♥ derim → "5♠ + 4♥" ( Majörlerden uzun olanı ters renk ile gösteririm).
    /// - Ben 3♠ derim → "5♥ + 4♠".
    ///
    /// Amaç: 5-4 majör dağılımı olduğunu bildirmek ve partnerin en uygun majörü seçmesini sağlamak.
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

            // 5-4 majör dağılımı kontrolü
            bool besliMacaDortluKupa = (maca >= 5 && kupa == 4);
            bool besliKupaDortluMaca = (kupa >= 5 && maca == 4);

            if (!besliMacaDortluKupa && !besliKupaDortluMaca) return false;

            // 5. Puan kontrolü (8+ HP yeterlidir)
            if (ElDegerlendirici.HCP(el) < 8) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

            // Smolen mantığı: Uzun olan majörü "ters" majör ile bildir.
            // 5+ Maça + 4 Kupa → 3♥ (SAYC: 3♥ teklifi 5♠'yi gösterir)
            if (maca >= 5 && kupa == 4) return "3♥";

            // 5+ Kupa + 4 Maça → 3♠ (SAYC: 3♠ teklifi 5♥'yi gösterir)
            if (kupa >= 5 && maca == 4) return "3♠";

            return "Pas";
        }
    }
}
