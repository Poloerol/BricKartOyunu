using System;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 5'li Majör Açılış Konvansiyonu.
    /// 
    /// Kural:
    /// - 12-21 HP aralığında
    /// - Elde 5'li bir majör (Maça veya Kupa) var
    /// - Dengesiz el (dengeliyse StrongNT önce devreye girer)
    /// 
    /// Öncelik: 10 (açılışta ilk denenmeli)
    /// 
    /// Örnek:
    ///   El: ♠AKQ54 ♥KJ3 ♦Q2 ♣T87  → 15 HCP, 5'li Maça → "1♠"
    ///   El: ♠KJ3 ♥AKQ54 ♦Q2 ♣T87  → 15 HCP, 5'li Kupa → "1♥"
    /// </summary>
    public class BesliMajor : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "5'li Majör";

        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 10 — açılış konvansiyonları arasında ilk denenmeli.
        /// (1NT 20'de, minörler 30'da, 2♣ 5'te — sıralama önemli.)
        /// </summary>
        public int Oncelik => 10;

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. Sadece ilk teklifte geçerli (henüz kimse konuşmadı veya herkes pas dedi)
            if (!durum.IlkTeklifMi()) return false;

            // 2. El puanı 12-21 arası olmalı
            //    (22+ HP → 2♣ yapay güçlü devreye girer)
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 12 || hp > 21) return false;

            // 3. Elde 5'li majör olmalı
            if (!ElDegerlendirici.BesliMajorVar(durum.AktifOyuncuEli)) return false;

            // 4. Dengeli el (15-17 HP) ise StrongNT önce gelmeli
            //    Bu kural BesliMajor'un devreye girmesini engellemez —
            //    sadece öncelik sıralamasıyla StrongNT önce dener (Oncelik 20).
            //    Ama dengeli 15-17 HP'de 5'li majör varsa (5-3-3-2 dengeli + 5 majör)
            //    genelde önce majör açılır. Bu yüzden burada engel yok.

            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

            // Maça 5+ mi?
            bool besliMacca = macca >= 5;
            bool besliKupa = kupa >= 5;

            // İkisi de 5+ ise uzun olanı seç
            if (besliMacca && besliKupa)
            {
                if (macca >= kupa) return "1♠";
                return "1♥";
            }

            // Sadece Maça 5+
            if (besliMacca) return "1♠";

            // Sadece Kupa 5+
            if (besliKupa) return "1♥";

            // Buraya gelmemeli (UygunMu zaten kontrol etti)
            return "Pas";
        }
    }
}