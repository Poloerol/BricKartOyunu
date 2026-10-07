using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Splinter Konvansiyonu — Majör açılışına çift zıplama ile kısa renk gösterimi.
    /// 
    /// Kural:
    /// - Partner 1♠ veya 1♥ açtı
    /// - Elimizde 4+ majör destek + 13+ HP + bir renkte singleton/void
    /// - O rengi çift zıplama ile teklif ederiz
    /// 
    /// Örnek:
    ///   1♠ - 4♣ = Sinek'te singleton/void + Maça desteği
    ///   1♥ - 3♠ = Maça'da singleton/void + Kupa desteği (çift zıplama)
    ///   1♥ - 4♣ = Sinek'te singleton/void + Kupa desteği
    ///   1♥ - 4♦ = Karo'da singleton/void + Kupa desteği
    /// 
    /// Öncelik: 17
    /// </summary>
    public class Splinter : IKonvansiyon
    {
        public string Ad => "Splinter";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 16;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            if (durum.PartnerTeklifleri.Count == 0) return false;
            string partnerSon = durum.PartnerTeklifleri.Last();
            if (partnerSon != "1♠" && partnerSon != "1♥") return false;

            if (durum.KendiTeklifleri.Count > 0) return false;
            bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
    durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipGercekTeklifVerdi) return false;

            string koz = partnerSon == "1♠" ? "Maça" : "Kupa";
            int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, koz);
            if (destek < 4) return false;

            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 13) return false;

            // Kısa renk var mı? (singleton veya void)
            var sayilar = ElDegerlendirici.RenkSayilari(durum.AktifOyuncuEli);
            foreach (var kv in sayilar)
            {
                if (kv.Key == koz) continue;
                if (kv.Value <= 1) return true;
            }

            return false;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string partnerSon = durum.PartnerTeklifleri.Last();
            string koz = partnerSon == "1♠" ? "Maça" : "Kupa";

            var sayilar = ElDegerlendirici.RenkSayilari(el);

            // Kısa rengi bul
            foreach (var kv in sayilar)
            {
                if (kv.Key == koz) continue;
                if (kv.Value <= 1)
                {
                    // Seviye: koz Maça ise 4, koz Kupa ise 3 (Maça kısa ise) veya 4
                    int seviye;
                    if (koz == "Maça") seviye = 4;
                    else if (kv.Key == "Maça") seviye = 3;  // 1♥ - 3♠
                    else seviye = 4;                         // 1♥ - 4♣/4♦

                    return $"{seviye}{KozSembolu(kv.Key)}";
                }
            }

            return "Pas";
        }

        private string KozSembolu(string renk)
        {
            switch (renk)
            {
                case "Maça": return "♠";
                case "Kupa": return "♥";
                case "Karo": return "♦";
                case "Sinek": return "♣";
                default: return "?";
            }
        }
    }
}