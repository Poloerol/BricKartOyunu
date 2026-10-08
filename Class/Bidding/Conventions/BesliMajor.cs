using System;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 5'li Majör Açılış Konvansiyonu (SAYC Standard).
    ///
    /// Kural:
    /// - 12-21 HP aralığında
    /// - Elde 5'li bir majör (Maça veya Kupa) var
    /// - Öncelik: 10 (açılışta ilk denenmeli, 2♣ Güçlü'den hemen sonra)
    ///
    /// Not: 15-17 HP ve Dengeli el durumunda, 5'li majör varsa genellikle majör açılış tercih edilir.
    /// </summary>
    public class BesliMajor : IKonvansiyon
    {
        public string Ad => "5'li Majör";
        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 10 — açılış konvansiyonları arasında ilklerden biri.
        /// (2♣ Güçlü: 5, BesliMajor: 10, StrongNT: 20, MinorAcilis: 30)
        /// </summary>
        public int Oncelik => 10;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // Sadece ilk teklifte geçerli
            if (!durum.IlkTeklifMi()) return false;

            // El puanı 12-21 arası olmalı (22+ HP -> 2♣ Güçlü)
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 12 || hp > 21) return false;

            // Elde 5'li majör olmalı
            if (!ElDegerlendirici.BesliMajorVar(durum.AktifOyuncuEli)) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

            // İkisi de 5+ ise en uzun olanı aç
            if (macca >= 5 && kupa >= 5)
            {
                return (macca >= kupa) ? "1♠" : "1♥";
            }

            if (macca >= 5) return "1♠";
            if (kupa >= 5) return "1♥";

            return "Pas";
        }
    }
}
