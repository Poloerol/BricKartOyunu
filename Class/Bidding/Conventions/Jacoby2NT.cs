using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Jacoby 2NT Konvansiyonu — Majör açılışına 2NT yapay güçlü destek.
    ///
    /// Kural:
    /// - Partner 1♠ veya 1♥ açtı
    /// - Rakip müdahale etmedi
    /// - Elimizde 4+ majör destek + 13+ HP var
    /// - 2NT deriz → "forcing, slam denemesi"
    ///
    /// Cevap:
    /// - Majör yoksa → 3♦
    /// - En az bir 4'lü majör varsa → 3♣
    ///
    /// Öncelik: 16 (Blackwood 15 sonrası)
    /// </summary>
    public class Jacoby2NT : IKonvansiyon
    {
        public string Ad => "Jacoby 2NT";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 17;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // ── ROL 1: Cevapçı (Partner 1 majör açtı, ben 2NT soracağım) ──
            if (durum.PartnerTeklifleri.Count > 0)
            {
                string partnerSon = durum.PartnerTeklifleri.Last();
                if ((partnerSon == "1♠" || partnerSon == "1♥") && durum.KendiTeklifleri.Count == 0)
                {
                    // Rakip gerçek bir teklif verdi mi?
                    bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
                        durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
                    if (rakipGercekTeklifVerdi) return false;

                    string koz = partnerSon == "1♠" ? "Maça" : "Kupa";
                    int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, koz);
                    int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);

                    if (destek >= 4 && hp >= 13) return true;
                }
            }

            // ── ROL 2: Açıcı (Ben 1 majör açtım, partner 2NT dedi) ──
            if (durum.KendiTeklifleri.Count > 0)
            {
                string kendiSon = durum.KendiTeklifleri.Last();
                if ((kendiSon == "1♠" || kendiSon == "1♥") && durum.PartnerTeklifleri.LastOrDefault() == "2NT")
                {
                    return true;
                }
            }

            return false;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            // ── ROL 1: Soru Sorma ──
            if (durum.PartnerTeklifleri.Count > 0 && durum.KendiTeklifleri.Count == 0)
            {
                return "2NT";
            }

            // ── ROL 2: Standart Cevap verme ──
            if (durum.KendiTeklifleri.Count > 0 && durum.PartnerTeklifleri.LastOrDefault() == "2NT")
            {
                var el = durum.AktifOyuncuEli;
                bool dortluMaca = ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4;
                bool dortluKupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4;

                if (dortluMaca || dortluKupa) return "3♣"; // En az bir 4'lü majör var
                return "3♦"; // Majör yok
            }

            return null;
        }
    }
}
