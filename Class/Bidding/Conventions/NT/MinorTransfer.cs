using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Minor Transfer Konvansiyonu.
    /// 
    /// Kural:
    /// - Partner 1NT açtı
    /// - Ben 2♠ derim → "5+ Sinek var, transfer" (partner 3♣ diyecek)
    /// - Ben 2NT derim → "5+ Karo var, transfer" (partner 3♦ diyecek)
    /// 
    /// Amaç: 1NT açılışından sonra minör eli göstermek.
    /// 
    /// Öncelik: 24
    /// </summary>
    public class MinorTransfer : IKonvansiyon
    {
        public string Ad => "Minor Transfer";
        public string Aciklama => "NT açılışına minör transferi";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 24;

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
            if (durum.KendiTeklifleri.Any(t => t != "Pas")) return false;   // önceki Pas'lar sayılmaz

            // Rakip müdahale etmedi
            bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipGercekTeklifVerdi) return false;

            var el = durum.AktifOyuncuEli;

            // 6+ Sinek veya 6+ Karo
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");

            return sinek >= 6 || karo >= 6;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");

            // Sinek daha uzunsa → 2♠ (Sinek transfer)
            if (sinek >= 6 && sinek >= karo) return "2♠";

            // Karo → 2NT (Karo transfer)
            if (karo >= 6) return "2NT";

            return "Pas";
        }
    }
}