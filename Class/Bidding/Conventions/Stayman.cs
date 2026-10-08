using System;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Stayman Konvansiyonu (SAYC Standard).
    ///
    /// Rol 1: Partner 1NT açtı, benim 4'lü majörüm var → 2♣ sorarım.
    /// Rol 2: Ben 1NT açtım, partner 2♣ dedi → Majörlerimi bildiririm.
    ///
    /// Kurallar:
    /// - Cevapçı: 8+ HP ve en az bir 4'lü majör.
    /// - Açıcı Cevaplar: 2♠ (4+ Maça), 2♥ (4+ Kupa), 2♦ (Majör yok).
    /// - Not: 4-4 majör varsa Maça önceliklidir.
    /// </summary>
    public class Stayman : IKonvansiyon
    {
        public string Ad => "Stayman";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 10;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            var el = durum.AktifOyuncuEli;

            // ── ROL 1: Partner 1NT açtı, ben Stayman soracağım ──
            if (durum.PartnerTeklifleri.LastOrDefault() == "1NT"
                && durum.KendiTeklifleri.Count == 0
                && !durum.RakipActiMi())
            {
                // 5+ majör varsa → Transfer konvansiyonu (JacobyTransfer) devreye girer.
                if (ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 5 ||
                    ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 5)
                    return false;

                // 4'lü majör + 8+ HP → Stayman
                bool dortluMaca = ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4;
                bool dortluKupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4;

                if ((dortluMaca || dortluKupa) && ElDegerlendirici.HCP(el) >= 8)
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

                // Maça önceliklidir
                if (dortluMaca) return "2♠";
                if (dortluKupa) return "2♥";

                // Majör yoksa 2♦
                return "2♦";
            }

            return null;
        }
    }
}
