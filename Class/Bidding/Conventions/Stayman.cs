using System;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Stayman Konvansiyonu (2 yönlü).
    /// 
    /// Rol 1: Partner 1NT açtı, benim 4'lü majörüm var → 2♣ sorarım
    /// Rol 2: Ben 1NT açtım, partner 2♣ dedi → 2♦/2♥/2♠ cevap veririm
    /// 
    /// Öncelik: 10 (BasitCevap'tan önce)
    /// </summary>
    public class Stayman : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "Stayman";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 10;

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            var el = durum.AktifOyuncuEli;

            // ── ROL 1: Partner 1NT açtı, ben Stayman sorusu soracağım ──
            if (durum.PartnerTeklifleri.LastOrDefault() == "1NT"
                && durum.KendiTeklifleri.Count == 0
                && !durum.RakipActiMi())
            {
                // 4'lü majör var mı?
                if (ElDegerlendirici.DortluMajorVar(el)
                    && ElDegerlendirici.HCP(el) >= 8)
                {
                    return true;
                }
            }

            // ── ROL 2: Ben 1NT açtım, partner 2♣ (Stayman) dedi ──
            if (durum.KendiTeklifleri.LastOrDefault() == "1NT"
                && durum.PartnerTeklifleri.LastOrDefault() == "2♣")
            {
                return true;
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            // ── ROL 1: Stayman sorusu ──
            if (durum.PartnerTeklifleri.LastOrDefault() == "1NT"
                && durum.KendiTeklifleri.Count == 0)
            {
                return "2♣";
            }

            // ── ROL 2: Stayman cevabı ──
            if (durum.KendiTeklifleri.LastOrDefault() == "1NT"
                && durum.PartnerTeklifleri.LastOrDefault() == "2♣")
            {
                var el = durum.AktifOyuncuEli;
                bool dortluKupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4;
                bool dortluMaca = ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4;

                if (dortluMaca) return "2♠";   // 4-4'te Maça öncelikli
                if (dortluKupa) return "2♥";
                return "2♦";   // Majör yok
            }

            return null;
        }
    }
}