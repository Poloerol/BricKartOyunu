using System;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Strong NT (1NT) Açılış Konvansiyonu.
    ///
    /// Kural:
    /// - 15-17 HP aralığında
    /// - Dengeli veya Yarı-Dengeli el (4-3-3-3, 4-4-3-2, 5-3-3-2, 5-4-2-2, 6-3-2-2)
    /// - 5'li majör VARSA BesliMajor önce devreye girer (öncelik 10)
    /// - Bu konvansiyon 5'li majör OLMADIĞINDA devreye girer
    ///
    /// Öncelik: 20 (BesliMajor'dan sonra)
    /// </summary>
    public class StrongNT : IKonvansiyon
    {
        public string Ad => "Strong NT";
        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 20 — BesliMajor'dan (10) sonra, MinorAcilis'tan (30) önce.
        /// </summary>
        public int Oncelik => 20;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // Sadece ilk teklifte geçerli
            if (!durum.IlkTeklifMi()) return false;

            var el = durum.AktifOyuncuEli;

            // El puanı 15-17 arası olmalı
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 15 || hp > 17) return false;

            // Dengeli veya Yarı-Dengeli el olmalı
            if (!ElDegerlendirici.DengeliEl(el) && !ElDegerlendirici.YariDengeliEl(el))
                return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            // 1NT açılışı
            return "1NT";
        }
    }
}
