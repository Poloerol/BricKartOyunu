using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;   // KozYardimcisi için

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Stayman Konvansiyonu (2 yönlü + cevap sistemi).
    /// 
    /// ROL 1: Partner 1NT açtı, benim 4'lü majörüm var → 2♣ sorarım
    /// ROL 2: Ben 1NT açtım, partner 2♣ dedi → 2♦/2♥/2♠ cevap veririm
    /// ROL 3: Açıcı cevap verdi, ben (cevapçı) 2. turumu oynarım
    /// 
    /// Öncelik: 11
    /// </summary>
    public class Stayman : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "Stayman";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 11;   // Gerber'den (13) ÖNCE

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
                // 5+ majör kontrolü
                bool besliMaca = ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 5;
                bool besliKupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 5;

                // 4+ majör kontrolü
                bool dortluMaca = ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4;
                bool dortluKupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4;

                // 5'li + 4'lü DİĞER majör varsa → Smolen için Stayman yap
                if ((besliMaca && dortluKupa) || (besliKupa && dortluMaca))
                {
                    return true;  // Stayman → sonra Smolen
                }

                // Sadece 5'li majör varsa → Transfer
                if (besliMaca || besliKupa) return false;

                // 4'lü majör + 8+ HP → Stayman
                if ((dortluMaca || dortluKupa) && ElDegerlendirici.HCP(el) >= 8)
                {
                    return true;
                }
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
            if (durum.KendiTeklifleri.Count > 0 &&
                durum.KendiTeklifleri.Contains("2♣") &&
                durum.PartnerTeklifleri.Contains("1NT"))
            {
                string partnerSon = durum.PartnerTeklifleri.LastOrDefault();
                if (partnerSon == "2♦" || partnerSon == "2♥" || partnerSon == "2♠")
                {
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
                    int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                    int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                    if (maca == 5 && kupa == 4) return "3♥"; // Smolen
                    if (kupa == 5 && maca == 4) return "3♠"; // Smolen

                    if (hp >= 8 && hp <= 9) return "2NT";
                    if (hp >= 10) return "3NT";

                    return "2NT";
                }

                // ─── Açıcı 2♥ dedi (4'lü ♥ var) ──────────────────────────
                if (partnerSon == "2♥")
                {
                    int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                    if (kupa >= 4)
                    {
                        if (hp >= 8 && hp <= 9) return "3♥";
                        if (hp >= 10) return "4♥";
                        return "3♥";
                    }

                    int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                    if (maca >= 4) return "2♠";

                    if (hp >= 10) return "3NT";
                    return "2NT";
                }

                // ─── Açıcı 2♠ dedi (4'lü ♠ var) ──────────────────────────
                if (partnerSon == "2♠")
                {
                    int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");

                    if (maca >= 4)
                    {
                        if (hp >= 8 && hp <= 9) return "3♠";
                        if (hp >= 10) return "4♠";
                        return "3♠";
                    }

                    if (hp >= 10) return "3NT";
                    return "2NT";
                }
            }

            return "Pas";
        }
    }
}