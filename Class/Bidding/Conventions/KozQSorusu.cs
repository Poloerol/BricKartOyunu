using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Koz Q Sorusu (Queen Ask)
    /// 
    /// RKCB veya Kickback cevabı sonrası 5NT ile koz Q sorulur.
    /// 
    /// Tetikleyici: Partner 5♣/5♦/5♥/5♠ cevabı verdi (RKCB/Kickback),
    /// biz 5NT diyoruz.
    /// 
    /// Cevap şeması:
    ///   6♣ = Koz Q yok, ♣K var
    ///   6♦ = Koz Q yok, ♦K var
    ///   6♥ = Koz Q yok, ♥K var
    ///   6♠ = Koz Q yok, ♠K var
    ///   6NT = Koz Q var, yan K yok
    ///   7 koz = Koz Q var + yan reyonda K var
    /// 
    /// Özel durum: RKCB cevabı 5♠ (2 + Q) ise 5NT → Specific King Ask
    /// 
    /// Öncelik: 27
    /// </summary>
    public class KozQSorusu : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════
        // IKonvansiyon ÜYELERİ
        // ═══════════════════════════════════════════════════════════════

        public string Ad => "Koz Q Sorusu";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 27;

        // ═══════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Koz anlaşılmış olmalı (NT değil)
            string koz = durum.AnlasilanKoz();
            if (string.IsNullOrEmpty(koz)) return false;
            if (koz == "NT") return false;

            // ─── DURUM 1: Partner 5♣/5♦/5♥/5♠ cevabı verdi → biz 5NT diyeceğiz
            // VEYA partner 5NT dedi → biz cevap vereceğiz
            if (PartnerKozQSorusuSordu(durum))
                return true;   // Partner 5NT dedi, biz cevap veriyoruz

            if (BizKozQSorusuSoracak(durum))
                return true;   // Biz 5NT diyeceğiz

            return false;
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            string koz = durum.AnlasilanKoz();
            if (string.IsNullOrEmpty(koz)) return "Pas";

            // ─── Cevap: Partner 5NT dedi (Koz Q sorusu) ────────────────
            if (PartnerKozQSorusuSordu(durum))
            {
                return KozQCevabi(el, koz);
            }

            // ─── Soru: Biz 5NT diyeceğiz ──────────────────────────────
            return "5NT";
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Partner 5NT mi Dedi? (Koz Q Sorusu)
        // ═══════════════════════════════════════════════════════════════

        private static bool PartnerKozQSorusuSordu(IhaleDurumu durum)
        {
            if (durum.Gecmis == null || durum.Gecmis.Count == 0)
                return false;

            // Partnerin son teklifi 5NT mi?
            var partnerTeklifleri = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .ToList();

            if (partnerTeklifleri.Count == 0) return false;

            string sonPartnerTeklifi = partnerTeklifleri.Last().Teklif;
            if (sonPartnerTeklifi != "5NT") return false;

            // Partner hâlâ son gerçek teklifi veren mi?
            var sonGercek = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .LastOrDefault();
            if (sonGercek == null) return false;
            if (sonGercek.Oyuncu != durum.Partner) return false;

            // Geçmişte RKCB (4NT) veya Kickback (4♠/4♥/4♦) var mı?
            bool rkcbVar = durum.Gecmis.Any(h =>
                h.GercekTeklifMi && h.Teklif == "4NT");

            bool kickbackVar = durum.Gecmis.Any(h =>
                h.GercekTeklifMi &&
                (h.Teklif == "4♠" || h.Teklif == "4♥" || h.Teklif == "4♦"));

            if (!rkcbVar && !kickbackVar) return false;

            // Geçmişte 5 seviyesinde bir cevap var mı?
            // (5♣/5♦/5♥/5♠ — RKCB/Kickback cevabı)
            bool cevapVar = durum.Gecmis.Any(h =>
                h.GercekTeklifMi &&
                (h.Teklif == "5♣" || h.Teklif == "5♦" ||
                 h.Teklif == "5♥" || h.Teklif == "5♠"));

            if (!cevapVar) return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Biz 5NT mi Soracağız?
        // ═══════════════════════════════════════════════════════════════

        private static bool BizKozQSorusuSoracak(IhaleDurumu durum)
        {
            // Kendi tekliflerimizde 5NT yok
            if (durum.KendiTeklifleri.Contains("5NT")) return false;

            // Partner son gerçek teklifi veren mi?
            var sonGercek = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .LastOrDefault();
            if (sonGercek == null) return false;
            if (sonGercek.Oyuncu != durum.Partner) return false;

            // Partnerin son teklifi 5♣/5♦/5♥/5♠ mi?
            string sonPartnerTeklifi = sonGercek.Teklif;
            if (sonPartnerTeklifi != "5♣" && sonPartnerTeklifi != "5♦" &&
                sonPartnerTeklifi != "5♥" && sonPartnerTeklifi != "5♠")
                return false;

            // Geçmişte RKCB (4NT) veya Kickback var mı?
            bool rkcbVar = durum.KendiTeklifleri.Contains("4NT");
            bool kickbackVar = durum.KendiTeklifleri.Contains("4♠") ||
                               durum.KendiTeklifleri.Contains("4♥") ||
                               durum.KendiTeklifleri.Contains("4♦");

            if (!rkcbVar && !kickbackVar) return false;

            // Son cevap 5♠ ise (2 + Q var), 5NT = Specific King Ask
            // Diğer cevaplar (5♣/5♦/5♥) → Koz Q sorusu

            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Koz Q Cevabı
        // ═══════════════════════════════════════════════════════════════

        private static string KozQCevabi(List<Card> el, string koz)
        {
            bool kozQVar = ElDegerlendirici.KizVar(el, koz);

            if (!kozQVar)
            {
                // Koz Q yok → hangi yan renkte K var?
                // Klasik: 6♣/6♦/6♥/6♠ sırasıyla

                // Yan renkler (koz dışında)
                var yanRenkler = new[] { "Sinek", "Karo", "Kupa", "Maça" };

                foreach (var renk in yanRenkler)
                {
                    if (renk == koz) continue;

                    bool renkKVar = ElDegerlendirici.PapazVar(el, renk);
                    if (renkKVar)
                    {
                        // 6 seviyesinde bu rengi göster
                        return $"6{KozYardimcisi.KozSembolu(renk)}";
                    }
                }

                // Hiç yan K yok → 6NT
                return "6NT";
            }
            else
            {
                // Koz Q var
                // Yan reyonda K var mı?
                var yanRenkler = new[] { "Sinek", "Karo", "Kupa", "Maça" };
                bool yanKVar = false;

                foreach (var renk in yanRenkler)
                {
                    if (renk == koz) continue;
                    if (ElDegerlendirici.PapazVar(el, renk))
                    {
                        yanKVar = true;
                        break;
                    }
                }

                if (yanKVar)
                {
                    // 7 koz (grand slam)
                    return $"7{KozYardimcisi.KozSembolu(koz)}";
                }
                else
                {
                    // 6NT (Q var, yan K yok)
                    return "6NT";
                }
            }
        }
    }
}