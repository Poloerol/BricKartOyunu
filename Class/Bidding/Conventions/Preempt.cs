using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Preempt (Bozucu Açılışlar)
    /// 
    /// 3♣/3♦/3♥/3♠ ve 4♣/4♦ açılışları 7+ kartlı renk + zayıf el gösterir.
    /// Amaç: rakiplerin konuşma alanını daraltmak, ortağa net bilgi vermek.
    /// 
    /// Kapsam:
    /// - 3♣, 3♦, 3♥, 3♠ → 7'li renk, 0-9 HP
    /// - 4♣, 4♦        → 8'li renk, 0-9 HP (daha agresif)
    /// 
    /// Not: 4♥/4♠ bu sınıfta DEĞİL — ayrı bir konvansiyon (Namyats) gerektirir.
    /// 
    /// Zayıf İki (2♦/2♥/2♠) ayrı konvansiyondur (ZayifIki.cs).
    /// </summary>
    public class Preempt : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════
        // IKonvansiyon ÜYELERİ
        // ═══════════════════════════════════════════════════════════════

        public string Ad => "Preempt";
        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 9: Zayıf İki (8) sonrası, BesliMajor (10) öncesi.
        /// </summary>
        public int Oncelik => 9;

        // ═══════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            // Sadece açılış (geçmiş boş)
            if (durum == null || durum.Gecmis == null || durum.Gecmis.Count > 0)
                return false;

            if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13)
                return false;

            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);

            // Preempt için zayıf el: 0-9 HP
            // 22+ HP → 2♣ Güçlü'ye bırak
            if (hp > 9) return false;
            if (hp >= 22) return false;

            // 7+ kartlı bir renk var mı?
            return YediArtıRenkVarMi(durum.AktifOyuncuEli);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            // ─── 8'li minör renk → 4 seviyesi ─────────────────────────
            // (4♥/4♠ bu sınıfta yok — Namyats ayrı konvansiyon)
            if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 8) return "4♦";
            if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 8) return "4♣";

            // ─── 7'li renk → 3 seviyesi ───────────────────────────────
            // Majörler önce (♠ > ♥ > ♦ > ♣)
            if (ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 7) return "3♠";
            if (ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 7) return "3♥";
            if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 7) return "3♦";
            if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 7) return "3♣";

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════════════════════════════

        private static bool YediArtıRenkVarMi(List<Card> el)
        {
            return ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 7
                || ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 7
                || ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 7
                || ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 7;
        }
    }
}