using System;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Minör Açılış Konvansiyonu (1♦ / 1♣).
    /// 
    /// Kural:
    /// - 12-21 HP aralığında
    /// - 5'li majör YOK (BesliMajor uygun değil)
    /// - Dengeli + 15-17 HP değil (StrongNT uygun değil)
    /// - 22+ HP değil (IkiliSinekGuclu uygun değil)
    /// 
    /// Öncelik: 30 (açılış konvansiyonları arasında son)
    /// 
    /// Örnek:
    ///   El: ♠KJ3 ♥Q43 ♦AKQ5 ♣J87  → 3-3-4-3 → "1♦"
    ///   El: ♠KJ3 ♥Q43 ♦QJ52 ♣AJ8  → 3-3-4-3 → "1♦"
    /// </summary>
    public class MinorAcilis : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "Minör Açılış";

        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 30 — diğer açılışlardan sonra denenir.
        /// </summary>
        public int Oncelik => 30;

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. Sadece ilk teklifte geçerli
            if (!durum.IlkTeklifMi()) return false;

            var el = durum.AktifOyuncuEli;

            // 2. HP kontrolü: 12-21 arası
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 12 || hp > 21) return false;

            // 3. 5'li majör VARSA BesliMajor önce devreye girer
            if (ElDegerlendirici.BesliMajorVar(el)) return false;

            // 4. Dengeli + 15-17 HP ise StrongNT önce devreye girer
            if (hp >= 15 && hp <= 17 && ElDegerlendirici.DengeliEl(el)) return false;

            // 5. Buraya geldiyse: minör aç
            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;

            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // Klasik minör açılış kuralı:
            // - Karo daha uzunsa → 1♦
            // - Sinek daha uzunsa → 1♣
            // - Eşit (4-4 veya 3-3) → 1♦ (klasik: karo önce)
            //   Not: Bazı sistemler 3-3'te Sinek der (better minor).
            //        Klasik SAYC: 4-4 → 1♦, 3-3 → 1♣.
            //        Basit tutuyoruz: Karo ≥ Sinek → 1♦.

            if (karo >= sinek) return "1♦";
            return "1♣";
        }
    }
}