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
    /// Öncelik: 16 (Blackwood 15 sonrası)
    /// </summary>
    public class Jacoby2NT : IKonvansiyon
    {
        public string Ad => "Jacoby 2NT";
        public string Aciklama => "Majör açılışına güçlü 2NT desteği";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 17;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            if (AciciCevabiMi(durum)) return true;
            if (CevapciDevamMi(durum)) return true;

            // Partner 1♠ veya 1♥ açtı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            string partnerSon = durum.PartnerTeklifleri.Last();
            if (partnerSon != "1♠" && partnerSon != "1♥") return false;

            // Biz henüz konuşmadık
            if (durum.KendiTeklifleri.Count > 0) return false;

            // Rakip gerçek bir teklif verdi mi? (Pas hariç)
            bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipGercekTeklifVerdi) return false;

            // Majör desteği 4+
            string koz = partnerSon == "1♠" ? "Maça" : "Kupa";
            int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, koz);
            if (destek < 4) return false;

            // 13+ HP
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 13) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            if (AciciCevabiMi(durum)) return AciciCevabi(durum);
            if (CevapciDevamMi(durum)) return "4" + KozYardimcisi.KozSembolu(durum.Gecmis.First(h => h.GercekTeklifMi).Koz);
            return "2NT";
        }

        /// <summary>1M - 2NT - açıcının cevabı (4M dışında) - ? : Jacoby cevapçısı oyuna (4M) taşır.</summary>
        private static bool CevapciDevamMi(IhaleDurumu durum)
        {
            var g = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (g.Count != 3) return false;
            if (g[0].Oyuncu != durum.Partner || g[1].Oyuncu != durum.AktifOyuncu || g[2].Oyuncu != durum.Partner) return false;
            if (g[0].Teklif != "1♠" && g[0].Teklif != "1♥") return false;
            if (g[1].Teklif != "2NT") return false;
            if (g[2].Seviye > 4 || (g[2].Seviye == 4 && g[2].Koz == g[0].Koz)) return false;
            if (durum.Gecmis.Any(h => durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi)) return false;
            return true;
        }

        /// <summary>1M - (Pas) - 2NT - (Pas) - ? : açıcının Jacoby 2NT cevabı sırası mı?</summary>
        private static bool AciciCevabiMi(IhaleDurumu durum)
        {
            var g = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (g.Count != 2) return false;
            if (g[0].Oyuncu != durum.AktifOyuncu || g[1].Oyuncu != durum.Partner) return false;
            if (g[0].Teklif != "1♠" && g[0].Teklif != "1♥") return false;
            if (g[1].Teklif != "2NT") return false;
            return true;
        }

        /// <summary>
        /// Açıcının cevabı: yan renkte tek/void (12-17 HP) → 3 seviyesinde o renk; 5+ kartlık ikinci renk → 4 o renk;
        /// 18+ → 3M; 14-17 → 3NT; asgari → 4M.
        /// </summary>
        private static string AciciCevabi(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string koz = durum.Gecmis.First(h => h.GercekTeklifMi).Koz;
            string sem = KozYardimcisi.KozSembolu(koz);
            int hp = ElDegerlendirici.HCP(el);

            if (hp >= 18) return "3" + sem;

            // 5+ kartlık ikinci renk (4 seviyesi)
            foreach (var r in new[] { "Sinek", "Karo", "Kupa" })
            {
                if (r == koz) continue;
                if (ElDegerlendirici.RenkUzunlugu(el, r) >= 5) return "4" + KozYardimcisi.KozSembolu(r);
            }
            // kısalık (tek/void), 3 seviyesinde: 3♣/3♦/3♥
            if (hp <= 17)
            {
                foreach (var r in new[] { "Sinek", "Karo", "Kupa", "Maça" })
                {
                    if (r == koz) continue;
                    if (ElDegerlendirici.RenkUzunlugu(el, r) <= 1)
                        return "3" + KozYardimcisi.KozSembolu(r);
                }
            }
            if (hp >= 14) return "3NT";
            return "4" + sem;
        }
    }
}