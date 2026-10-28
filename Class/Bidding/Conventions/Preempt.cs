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
        public string Aciklama => "Zayıf preempt açılışı";
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
            if (durum == null || durum.Gecmis == null || !durum.IlkTeklifMi())
                return false;

            if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13)
                return false;

            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);

            int agr = durum.Anlasma != null ? durum.Anlasma.PreemptAgresifligi : 0;
            int ust = agr < 0 ? 8 : (agr > 0 ? 10 : 9);
            if (hp > ust) return false;
            if (hp >= 22) return false;
            if (agr < 0 && !HonorluYedili(durum.AktifOyuncuEli)) return false;

            if (!YediArtıRenkVarMi(durum.AktifOyuncuEli))
                return false;

            // Namyats aktifse, 8'li minör + Namyats HP aralığı (10-15) → Namyats'a bırak
            if (durum.Anlasma != null && durum.Anlasma.Namyats)
            {
                int karo = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Karo");
                int sinek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Sinek");

                // Sadece Namyats'ın alacağı ellerde çekil (10-15 HP)
                if (hp >= 10 && hp <= 15)
                {
                    if (karo >= 8) return false;
                    if (sinek >= 8) return false;
                }
                // hp < 10 → Preempt alır (zayıf 8'li minör)
            }

            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            bool namyatsAktif = durum.Anlasma != null && durum.Anlasma.Namyats;

            // ─── 8'li minör renk → 4 seviyesi ─────────────────────────
            // (Namyats aktifse, 4♣/4♦ Namyats'a bırakılır)
            if (!namyatsAktif)
            {
                if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 8) return "4♦";
                if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 8) return "4♣";
            }

            // ─── 7'li renk → 3 seviyesi ───────────────────────────────
            if (ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 7) return "3♠";
            if (ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 7) return "3♥";
            if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 7) return "3♦";
            if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 7) return "3♣";

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════════════════════════════

        /// <summary>7+ kartlı ve A/K/Q'dan en az ikisine sahip renk var mı? (temkinli önleyici)</summary>
        private static bool HonorluYedili(List<Card> el)
        {
            foreach (var r in new[] { "Maça", "Kupa", "Karo", "Sinek" })
                if (ElDegerlendirici.RenkUzunlugu(el, r) >= 7 && el.Count(c => c.Suit == r && c.Value >= 12) >= 2)
                    return true;
            return false;
        }

        private static bool YediArtıRenkVarMi(List<Card> el)
        {
            return ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 7
                || ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 7
                || ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 7
                || ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 7;
        }
    }
}