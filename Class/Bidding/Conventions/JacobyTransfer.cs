using System;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Jacoby Transfer Konvansiyonu (2 yönlü).
    /// 
    /// Rol 1: Partner 1NT açtı, benim 5+ majörüm var → 2♦/2♥ transfer
    ///   - 2♦ → 5+ Kupa gösterir (partner 2♥ der)
    ///   - 2♥ → 5+ Maça gösterir (partner 2♠ der)
    /// 
    /// Rol 2: Ben 1NT açtım, partner 2♦/2♥ transfer dedi → kabul ederim
    ///   - Partner 2♦ → ben 2♥ derim
    ///   - Partner 2♥ → ben 2♠ derim
    /// 
    /// Öncelik: 10 (Stayman ile aynı seviyede)
    /// 
    /// NOT: Stayman (4'lü majör) ve Transfer (5+ majör) çakışmaz:
    /// - 4'lü majör → Stayman (2♣)
    /// - 5+ majör → Transfer (2♦/2♥)
    /// </summary>
    public class JacobyTransfer : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "Jacoby Transfer";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 15;   // Stayman (10) sonra, BasitCevap (100) önce

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            var el = durum.AktifOyuncuEli;

            // ── ROL 1: Partner 1NT açtı, ben transfer sorusu soracağım ──
            if (durum.PartnerTeklifleri.LastOrDefault() == "1NT"
                && durum.KendiTeklifleri.Count == 0
                && !durum.RakipActiMi())
            {
                // 5+ majör var mı?
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                // 5+ Maça veya 5+ Kupa
                // Ama 4'lü majörü Stayman ele alır → 5+ olmalı
                if (macca >= 5 || kupa >= 5)
                {
                    // En az 6 HP gerekli (zayıf el bile transfer edebilir)
                    if (ElDegerlendirici.HCP(el) >= 6)
                    {
                        return true;
                    }
                }
            }

            // ── ROL 2: Ben 1NT açtım, partner 2♦/2♥ transfer dedi ──
            if (durum.KendiTeklifleri.LastOrDefault() == "1NT")
            {
                string partnerSon = durum.PartnerTeklifleri.LastOrDefault();
                if (partnerSon == "2♦" || partnerSon == "2♥")
                {
                    return true;
                }
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            // ── ROL 1: Transfer sorusu ──
            if (durum.PartnerTeklifleri.LastOrDefault() == "1NT"
                && durum.KendiTeklifleri.Count == 0)
            {
                var el = durum.AktifOyuncuEli;
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                // 5+ Maça → 2♥ (Maça transferi)
                // 5+ Kupa → 2♦ (Kupa transferi)
                // İkisi de varsa → uzun olanı seç
                if (macca >= 5 && kupa >= 5)
                {
                    if (macca >= kupa) return "2♥";   // Maça transferi
                    return "2♦";                        // Kupa transferi
                }

                if (macca >= 5) return "2♥";
                if (kupa >= 5) return "2♦";
            }

            // ── ROL 2: Transfer kabulü ──
            if (durum.KendiTeklifleri.LastOrDefault() == "1NT")
            {
                string partnerSon = durum.PartnerTeklifleri.LastOrDefault();

                if (partnerSon == "2♦") return "2♥";   // Kupa transferi kabul
                if (partnerSon == "2♥") return "2♠";   // Maça transferi kabul
            }

            return null;
        }
    }
}