using System;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Minör Açılış Konvansiyonu (1♦ / 1♣).
    ///
    /// Kural:
    /// - 12-21 HP aralığında
    /// - 5'li majör YOK (BesliMajor uygun değil)
    /// - Dengeli/Yarı-Dengeli + 15-17 HP değil (StrongNT uygun değil)
    /// - 22+ HP değil (IkiliSinekGuclu uygun değil)
    ///
    /// Öncelik: 30 (açılış konvansiyonları arasında son)
    /// </summary>
    public class MinorAcilis : IKonvansiyon
    {
        public string Ad => "Minör Açılış";
        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 30 — diğer açılışlardan sonra denenir.
        /// </summary>
        public int Oncelik => 30;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // Sadece ilk teklifte geçerli
            if (!durum.IlkTeklifMi()) return false;

            var el = durum.AktifOyuncuEli;

            // HP kontrolü: 12-21 arası
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 12 || hp > 21) return false;

            // 5'li majör VARSA BesliMajor önce devreye girer
            if (ElDegerlendirici.BesliMajorVar(el)) return false;

            // Dengeli/Yarı-Dengeli + 15-17 HP ise StrongNT önce devreye girer
            if (hp >= 15 && hp <= 17 && (ElDegerlendirici.DengeliEl(el) || ElDegerlendirici.YariDengeliEl(el)))
                return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;

            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // SAYC Standart:
            // 4-4 minör → 1♦
            // 3-3 minör → 1♣
            // Uzun olan minör tercih edilir.
            if (karo > sinek) return "1♦";
            if (sinek > karo) return "1♣";

            // Eşitlik durumunda: 4-4 ise 1♦, 3-3 ise 1♣
            return (karo == 4) ? "1♦" : "1♣";
        }
    }
}
