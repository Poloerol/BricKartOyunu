using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Minor Transfer Konvansiyonu (SAYC Standart ve Profesyonel Yaklaşım).
    ///
    /// Kural:
    /// - Partner 1NT açtı.
    /// - 5+ minör el var.
    /// - Standart SAYC'de 1NT sonrası minörler doğrudan teklif edilmez,
    ///   ancak bazı modern varyasyonlarda 2♠ Sinek transferi olarak kullanılır.
    ///
    /// Dikkat: 2NT asla transfer için kullanılmaz (Slam invite veya dengeli eldir).
    /// </summary>
    public class MinorTransfer : IKonvansiyon
    {
        public string Ad => "Minor Transfer";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 25;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Partner 1NT açtı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            if (durum.PartnerTeklifleri.Last() != "1NT") return false;

            // Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            // Rakip müdahale etmedi
            bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipGercekTeklifVerdi) return false;

            var el = durum.AktifOyuncuEli;

            // Sadece 5+ Sinek için 2♠ transferi desteklenir (SAYC varyasyonu)
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            return sinek >= 5;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            // Sinek transferi -> 2♠
            // (Partner buna 2NT veya 3♣ ile cevap verir)
            return "2♠";
        }
    }
}
