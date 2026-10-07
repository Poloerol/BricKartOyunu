using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Jacoby 2NT Konvansiyonu — Majör açılışına 2NT yapay güçlü destek.
    /// 
    /// Kural:
    /// - Partner 1♠ veya 1♥ açtı
    /// - Rakip müdahale etmedi
    /// - Elimizde 4+ majör destek + 13+ HP var
    /// - 2NT deriz → "forcing, slam denemesi"
    /// 
    /// Öncelik: 16 (Blackwood 15 sonrası)
    /// </summary>
    public class Jacoby2NT : IKonvansiyon
    {
        public string Ad => "Jacoby 2NT";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 17;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Partner 1♠ veya 1♥ açtı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            string partnerSon = durum.PartnerTeklifleri.Last();
            if (partnerSon != "1♠" && partnerSon != "1♥") return false;

            // Biz henüz konuşmadık
            if (durum.KendiTeklifleri.Count > 0) return false;

            // Rakip gerçek bir teklif verdi mi? (Pas hariç)
            bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipGercekTeklifVerdi) return false;

            // Majör desteği 4+
            string koz = partnerSon == "1♠" ? "Maça" : "Kupa";
            int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, koz);
            if (destek < 4) return false;

            // 13+ HP
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 13) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "2NT";
        }
    }
}