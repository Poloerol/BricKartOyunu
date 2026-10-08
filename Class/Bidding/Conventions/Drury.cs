using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Drury Konvansiyonu (SAYC Standard).
    ///
    /// Kural:
    /// - Partner 3. veya 4. pozisyonda 1 majör (1♠ veya 1♥) açtı.
    /// - Ben 2♣ diyerek "yapay" bir teklif veririm.
    /// - Bu teklif: 3+ majör desteği ve 10+ HP olduğunu gösterir.
    ///
    /// Amaç: İkinci pozisyonda yapılamayan "Limit Raise" veya "Game" denemesini
    /// 3. veya 4. pozisyonda yapabilmek.
    ///
    /// Öncelik: 25
    /// </summary>
    public class Drury : IKonvansiyon
    {
        public string Ad => "Drury";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 25;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // 1. Partner bir majör açtı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            string partnerSon = durum.PartnerTeklifleri.Last();
            if (partnerSon != "1♥" && partnerSon != "1♠") return false;

            // 2. Partner 3. veya 4. pozisyonda mı açtı?
            // Yani partnerden önce en az bir kişi pas geçmiş olmalı.
            var partnerIlkHamle = durum.Gecmis
                .FirstOrDefault(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi);
            if (partnerIlkHamle == null) return false;

            bool partnerdenOncePasVar = durum.Gecmis.Any(h =>
                h.Sira < partnerIlkHamle.Sira && h.PasMi);
            if (!partnerdenOncePasVar) return false;

            // 3. Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            // 4. Partnerin majöründe 3+ destek var mı?
            string partnerKozu = partnerSon == "1♥" ? "Kupa" : "Maça";
            int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, partnerKozu);
            if (destek < 3) return false;

            // 5. 10+ HP kontrolü
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
