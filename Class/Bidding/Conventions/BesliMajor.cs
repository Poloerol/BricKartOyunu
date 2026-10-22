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
        public string Aciklama => "5'li majör açılışı";

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
            if (!durum.IlkTeklifMi()) return false;

            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // Dengeli ve 20-21 veya 15-17 puan aralığındaysa majör açışını engelle (NT önceliklidir)
            if (dengeli && ((hp >= 15 && hp <= 17) || (hp >= 20 && hp <= 21)))
            {
                return false;
            }

            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

            return hp >= 12 && (macca >= 5 || kupa >= 5);
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