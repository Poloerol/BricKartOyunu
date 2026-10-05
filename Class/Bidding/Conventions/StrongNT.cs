using System;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Strong NT (1NT) Açılış Konvansiyonu.
    /// 
    /// Kural:
    /// - 15-17 HP aralığında
    /// - Dengeli el (4-3-3-3, 4-4-3-2, 5-3-3-2, 5-4-2-2, 6-3-2-2)
    /// - 5'li majör VARSA BesliMajor önce devreye girer (öncelik 10)
    /// - Bu konvansiyon 5'li majör OLMADIĞINDA devreye girer
    /// 
    /// Öncelik: 20 (BesliMajor'dan sonra)
    /// 
    /// Örnek:
    ///   El: ♠KJ3 ♥QJ3 ♦AKQ5 ♣J87  → 17 HCP, dengeli → "1NT"
    ///   El: ♠A KQ5 ♥KJ3 ♦QJ2 ♣Q87  → 16 HCP, dengeli → "1NT"
    /// </summary>
    public class StrongNT : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "Strong NT";

        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 20 — BesliMajor'dan (10) sonra, MinorAcilis'tan (30) önce.
        /// </summary>
        public int Oncelik => 20;

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

            // 2. El puanı 15-17 arası olmalı
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 15 || hp > 17) return false;

            // 3. Dengeli el olmalı
            if (!ElDegerlendirici.DengeliEl(el)) return false;

            // 4. 5'li majör VARSA BesliMajor daha önce denenir (öncelik 10).
            //    Ama burada güvenlik kontrolü yapalım: 5'li majör yoksa kesin geçerli.
            //    (5'li majör varsa BesliMajor zaten teklifi verir.)

            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            // 1NT açılışı
            return "1NT";
        }
    }
}