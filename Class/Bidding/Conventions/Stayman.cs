using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Stayman Konvansiyonu (2 yönlü + cevap sistemi).
    /// 
    /// ROL 1: Partner 1NT açtı, benim 4'lü majörüm var → 2♣ sorarım
    /// ROL 2: Ben 1NT açtım, partner 2♣ dedi → 2♦/2♥/2♠ cevap veririm
    /// ROL 3: Açıcı cevap verdi, ben (cevapçı) 2. turumu oynarım
    /// 
    /// Öncelik: 10
    /// </summary>
    public class Stayman : IKonvansiyon
    {
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
            if (durum.IhaleBittiMi()) return false;

            var el = durum.AktifOyuncuEli;

            // ═══════════════════════════════════════════════════════════════
            // ROL 1: Partner 1NT açtı, ben Stayman sorusu soracağım
            // ═══════════════════════════════════════════════════════════════
            if (durum.PartnerTeklifleri.LastOrDefault() == "1NT"
                && durum.KendiTeklifleri.Count == 0
                && !durum.RakipActiMi())
            {
                // 5+ majör varsa → Transfer kullan (Stayman değil)
                bool besliMaca = ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 5;
                bool besliKupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 5;
                if (besliMaca || besliKupa) return false;

                // 4'lü majör + 8+ HP → Stayman
                bool dortluMaca = ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4;
                bool dortluKupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4;
                if ((dortluMaca || dortluKupa) && ElDegerlendirici.HCP(el) >= 8)
                    return true;
            }

            // ═══════════════════════════════════════════════════════════════
            // ROL 2: Ben 1NT açtım, partner 2♣ (Stayman) dedi
            // ═══════════════════════════════════════════════════════════════
            if (durum.KendiTeklifleri.LastOrDefault() == "1NT"
                && durum.PartnerTeklifleri.LastOrDefault() == "2♣")
            {
                return true;
            }

            // ═══════════════════════════════════════════════════════════════
            // ROL 3: Açıcı cevap verdi (2♦/2♥/2♠), ben (cevapçı) 2. tur
            // ═══════════════════════════════════════════════════════════════
            // Senaryo: Ben 1NT açtım DEĞİL, partner açtı.
            //         Ben 2♣ dedim.
            //         Partner 2♦/2♥/2♠ dedi.
            //         Şimdi benim sıram.
            if (durum.KendiTeklifleri.Count > 0 &&
                durum.KendiTeklifleri.Contains("2♣") &&
                durum.PartnerTeklifleri.Contains("1NT"))
            {
                // Partner son teklifi 2♦/2♥/2♠ mi?
                string partnerSon = durum.PartnerTeklifleri.LastOrDefault();
                if (partnerSon == "2♦" || partnerSon == "2♥" || partnerSon == "2♠")
                {
                    // Bu benim 2. turum mu? (yani KendiTekliflerim sadece 2♣ mü?)
                    if (durum.KendiTeklifleri.Count == 1)
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
            var el = durum.AktifOyuncuEli;

            // ═══════════════════════════════════════════════════════════════
            // ROL 1: Stayman sorusu
            // ═══════════════════════════════════════════════════════════════
            if (durum.PartnerTeklifleri.LastOrDefault() == "1NT"
                && durum.KendiTeklifleri.Count == 0)
            {
                return "2♣";
            }

            // ═══════════════════════════════════════════════════════════════
            // ROL 2: Stayman cevabı (açıcı olarak)
            // ═══════════════════════════════════════════════════════════════
            if (durum.KendiTeklifleri.LastOrDefault() == "1NT"
                && durum.PartnerTeklifleri.LastOrDefault() == "2♣")
            {
                bool dortluKupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4;
                bool dortluMaca = ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4;

                if (dortluMaca) return "2♠";   // 4-4'te Maça öncelikli
                if (dortluKupa) return "2♥";
                return "2♦";   // Majör yok
            }

            // ═══════════════════════════════════════════════════════════════
            // ROL 3: Cevapçının 2. turu
            // ═══════════════════════════════════════════════════════════════
            if (durum.KendiTeklifleri.Contains("2♣") &&
                durum.PartnerTeklifleri.Contains("1NT") &&
                durum.KendiTeklifleri.Count == 1)
            {
                string partnerSon = durum.PartnerTeklifleri.LastOrDefault();
                int hp = ElDegerlendirici.HCP(el);

                // ─── Açıcı 2♦ dedi (majör yok) ───────────────────────────
                if (partnerSon == "2♦")
                {
                    // 5'li ♥ varsa → Smolen (3♥ = 5♠+4♥ ise tersi)
                    // Aslında Smolen kuralı:
                    // 5♠+4♥ → 3♥
                    // 5♥+4♠ → 3♠
                    int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                    int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                    if (maca == 5 && kupa == 4) return "3♥"; // Smolen
                    if (kupa == 5 && maca == 4) return "3♠"; // Smolen

                    // Invite (8-9 HP) → 2NT
                    if (hp >= 8 && hp <= 9) return "2NT";

                    // Game (10+ HP) → 3NT
                    if (hp >= 10) return "3NT";

                    return "2NT";
                }

                // ─── Açıcı 2♥ dedi (4'lü ♥ var) ──────────────────────────
                if (partnerSon == "2♥")
                {
                    int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                    // ♥ fit var mı? (4+ kart)
                    if (kupa >= 4)
                    {
                        // Invite (8-9 HP) → 3♥
                        if (hp >= 8 && hp <= 9) return "3♥";
                        // Game (10+ HP) → 4♥
                        if (hp >= 10) return "4♥";
                        return "3♥";
                    }

                    // ♥ fit yok, 4'lü ♠ var mı? (puppet)
                    int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                    if (maca >= 4) return "2♠";  // "4'lü ♠ var mı?"

                    // Fit yok → NT
                    if (hp >= 10) return "3NT";
                    return "2NT";
                }

                // ─── Açıcı 2♠ dedi (4'lü ♠ var) ──────────────────────────
                if (partnerSon == "2♠")
                {
                    int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");

                    // ♠ fit var mı? (4+ kart)
                    if (maca >= 4)
                    {
                        // Invite (8-9 HP) → 3♠
                        if (hp >= 8 && hp <= 9) return "3♠";
                        // Game (10+ HP) → 4♠
                        if (hp >= 10) return "4♠";
                        return "3♠";
                    }

                    // Fit yok → NT
                    if (hp >= 10) return "3NT";
                    return "2NT";
                }
            }

            return null;
        }
    }
}