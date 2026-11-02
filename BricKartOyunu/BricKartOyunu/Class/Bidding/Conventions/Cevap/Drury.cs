using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Drury Konvansiyonu (Reverse Drury).
    /// 
    /// Kural:
    /// - Partner daha önce Pas dedi (3. veya 4. pozisyon)
    /// - Ben 1♥ veya 1♠ açtım
    /// - Partner 2♣ der → "Yapay, 3+ majör desteği + 10+ HP"
    /// 
    /// Amaç: Pas - 1M açılışlarına yapay 2♣ ile game denemesi.
    /// 
    /// Öncelik: 25
    /// </summary>
    public class Drury : IKonvansiyon
    {
        public string Ad => "Drury";
        public string Aciklama => "Pas sonrası majör açılışına Drury cevabı";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 25;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Partner 1♥ veya 1♠ açtı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            string partnerSon = durum.PartnerTeklifleri.Last();
            if (partnerSon != "1♥" && partnerSon != "1♠") return false;

            // Partner ilk konuşan mıydı? Yani partnerden önce pas var mı?
            var partnerIlkHamle = durum.Gecmis
                .FirstOrDefault(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi);
            if (partnerIlkHamle == null) return false;

            // Partnerden önce pas var mı? (3. veya 4. pozisyon)
            // Drury: CEVAPÇI (ben) daha önce Pas demiş olmalı (passed hand)
            bool benOncePasDedim = durum.Gecmis.Any(h =>
                h.Oyuncu == durum.AktifOyuncu && h.Sira < partnerIlkHamle.Sira && h.PasMi);
            if (!benOncePasDedim) return false;
            // Rakip müdahale ettiyse Drury yok
            if (durum.RakipActiMi()) return false;

            // Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Any(t => t != "Pas")) return false;

            // Partnerin majöründe 3+ destek var mı?
            string partnerKozu = partnerSon == "1♥" ? "Kupa" : "Maça";
            int destek = ElDegerlendirici.RenkUzunlugu(
                durum.AktifOyuncuEli, partnerKozu);
            if (destek < 3) return false;

            // 10+ HP
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 10) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            // Yapay 2♣
            return "2♣";
        }
    }
}