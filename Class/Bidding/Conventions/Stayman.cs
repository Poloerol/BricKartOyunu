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
        // YARDIMCILAR (Pas'lar sayılmaz — yalnızca GERÇEK teklifler)
        // ═══════════════════════════════════════════════════════════════════

        private static List<string> Gercek(IhaleDurumu d, Player p)
        {
            return d.Gecmis.Where(h => h.Oyuncu == p && h.GercekTeklifMi)
                           .Select(h => h.Teklif).ToList();
        }

        /// <summary>
        /// ROL 4: Ben 1NT açtım, Stayman'a 2♦/2♥/2♠ ile cevap verdim,
        /// ortak ikinci turda DAVET etti (2NT veya cevabımın majörüne 3 seviye raise).
        /// </summary>
        private static bool Rol4Davet(IhaleDurumu durum, out string davet)
        {
            davet = null;
            var benim = Gercek(durum, durum.AktifOyuncu);
            var ortak = Gercek(durum, durum.Partner);
            if (benim.Count != 2 || ortak.Count != 2) return false;
            if (benim[0] != "1NT" || ortak[0] != "2♣") return false;

            string cevabim = benim[1];
            if (cevabim != "2♦" && cevabim != "2♥" && cevabim != "2♠") return false;

            // Son gerçek teklif ortağın daveti olmalı (rakip araya girmediyse)
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;

            string y = ortak[1];
            if (y == "2NT") { davet = y; return true; }
            if ((cevabim == "2♥" && y == "3♥") || (cevabim == "2♠" && y == "3♠"))
            {
                davet = y;
                return true;
            }
            return false;
        }

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
            if (Gercek(durum, durum.Partner).LastOrDefault() == "1NT"
                && Gercek(durum, durum.AktifOyuncu).Count == 0
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
                    if (Gercek(durum, durum.AktifOyuncu).Count == 1)
                        return true;
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // ROL 4: Ben 1NT açtım, Stayman'a cevap verdim, ortak davet etti
            // ═══════════════════════════════════════════════════════════════
            if (Rol4Davet(durum, out _)) return true;

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;

            // ═══════════════════════════════════════════════════════════════
            // ROL 4: Ortağın davetine karar ver (minimum → Pas, maksimum → oyun)
            // ═══════════════════════════════════════════════════════════════
            if (Rol4Davet(durum, out string davet))
            {
                int hp4 = ElDegerlendirici.HCP(el);
                if (davet == "2NT") return hp4 >= 16 ? "3NT" : "Pas";

                // Majör raise daveti (3♥ / 3♠)
                string sembol = davet.Substring(1);
                return hp4 >= 16 ? "4" + sembol : "Pas";
            }

            // ═══════════════════════════════════════════════════════════════
            // ROL 1: Stayman sorusu
            // ═══════════════════════════════════════════════════════════════
            if (Gercek(durum, durum.Partner).LastOrDefault() == "1NT"
                && Gercek(durum, durum.AktifOyuncu).Count == 0)
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
                Gercek(durum, durum.AktifOyuncu).Count == 1)
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