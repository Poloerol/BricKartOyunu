using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Michaels Cue Bid — rakip 1 seviye açılışına cue bid ile iki renk.
    ///
    /// Cue bid (müdahale):
    ///   Rakip 1♠ → 2♠ = 5+ ♥ + 5+ minör
    ///   Rakip 1♥ → 2♥ = 5+ ♠ + 5+ minör
    ///   Rakip 1♦/1♣ → 2♦/2♣ = 5+ ♠ + 5+ ♥
    ///
    /// Partner cevabı (advancer) — ÖNEMLİ:
    ///   Cue bid RAKİP rengidir, doğal yükseltme YAPILMAZ.
    ///   1♥ - 2♥(M) - ?  → 2♠/3♠ (maça tercihi), 2NT (minör sor), Pas; asla 3♥ değil.
    ///   1♠ - 2♠(M) - ?  → 3♥/4♥ (kupa), 2NT (minör sor), Pas; asla 3♠ değil.
    ///
    /// Öncelik: 19
    /// </summary>
    public class Michaels : IKonvansiyon
    {
        public string Ad => "Michaels";
        public string Aciklama => "İki renkli Michaels cue-bid";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 19;

        private enum Rol { CueBid, Cevap }

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // (Genel 5-5 kısayolu kaldırıldı: yalnızca gerçek cue-bid / cevap durumları)

            // Eğer özel CueBid veya Cevap koşulları varsa onları da çağırarak kontrol et
            if (CueBidUygunMu(durum)) return true;
            if (CevapUygunMu(durum)) return true;

            return false;
        }


        public string TeklifVer(IhaleDurumu durum)
        {
            if (!UygunMu(durum))
                return "Pas"; // uygun değilse motor yanlış teklif üretmesin

            if (CueBidUygunMu(durum))
                return CueBidTeklifi(durum);

            if (CevapUygunMu(durum))
                return CevapTeklifi(durum);

            return "Pas";
        }


        // ═══════════════════════════════════════════════════════════════
        // CUE BID (müdahale eden)
        // ═══════════════════════════════════════════════════════════════

        private static bool CueBidUygunMu(IhaleDurumu durum)
        {
            if (durum.RakipTeklifleri.Count == 0) return false;
            string rakipSon = durum.RakipTeklifleri.Last();
            if (rakipSon != "1♠" && rakipSon != "1♥" &&
                rakipSon != "1♦" && rakipSon != "1♣") return false;

            if (durum.KendiTeklifleri.Count > 0) return false;

            // Partner henüz gerçek teklif vermemiş olmalı (direkt oturuş)
            if (durum.PartnerTeklifleri.Any(t => t != "Pas" && t != "Dbl" && t != "Rdbl"))
                return false;

            string rakipKoz = KozYardimcisi.TekliftenKozCikar(rakipSon);
            if (string.IsNullOrEmpty(rakipKoz)) return false;

            var el = durum.AktifOyuncuEli;
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            if (rakipKoz == "Maça")
                return kupa >= 5 && (karo >= 5 || sinek >= 5);
            if (rakipKoz == "Kupa")
                return maca >= 5 && (karo >= 5 || sinek >= 5);
            if (rakipKoz == "Karo" || rakipKoz == "Sinek")
                return maca >= 5 && kupa >= 5;

            return false;
        }

        private static string CueBidTeklifi(IhaleDurumu durum)
        {
            string rakipSon = durum.RakipTeklifleri.Last();
            string rakipKoz = KozYardimcisi.TekliftenKozCikar(rakipSon);
            return "2" + KozYardimcisi.KozSembolu(rakipKoz);
        }

        // ═══════════════════════════════════════════════════════════════
        // CEVAP (partner Michaels yaptı)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Partner, rakibin 1X açılışına 2X cue bid yaptı mı?
        /// Örnek: ... Rakip 1♥ - Partner 2♥  veya  Rakip 1♥ - (Pas) - Partner 2♥
        /// </summary>
        private static bool CevapUygunMu(IhaleDurumu durum)
        {
            if (durum.PartnerTeklifleri.Count == 0) return false;
            if (durum.KendiTeklifleri.Count > 0) return false; // henüz konuşmadık

            // Partner'ın son gerçek teklifi 2 seviye cue bid olmalı
            string partnerSon = durum.PartnerTeklifleri.LastOrDefault(t =>
                t != "Pas" && t != "Dbl" && t != "Rdbl" && t != "RDbl");
            if (string.IsNullOrEmpty(partnerSon)) return false;
            if (partnerSon.Length < 2 || partnerSon[0] != '2') return false;

            string partnerKoz = KozYardimcisi.TekliftenKozCikar(partnerSon);
            if (string.IsNullOrEmpty(partnerKoz) || partnerKoz == "NT") return false;

            // Rakip 1 seviyesinde AYNI rengi açmış olmalı (cue bid tanımı)
            var rakipAcilis = durum.Gecmis
                .Where(h => h.GercekTeklifMi && durum.Rakipler.Contains(h.Oyuncu))
                .Select(h => h.Teklif)
                .FirstOrDefault(t => t != null && t.StartsWith("1"));
            if (string.IsNullOrEmpty(rakipAcilis)) return false;

            string rakipKoz = KozYardimcisi.TekliftenKozCikar(rakipAcilis);
            if (rakipKoz != partnerKoz) return false; // aynı renk cue bid

            // Partner gerçekten 2{rakipKoz} demiş
            if (partnerSon != "2" + KozYardimcisi.KozSembolu(rakipKoz))
                return false;

            return true;
        }

        private static string CevapTeklifi(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);

            string partnerSon = durum.PartnerTeklifleri.Last(t =>
                t != "Pas" && t != "Dbl" && t != "Rdbl" && t != "RDbl");
            string cueKoz = KozYardimcisi.TekliftenKozCikar(partnerSon); // rakibin rengi

            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // ─── Rakip 1♥ / cue 2♥ → partner 5♠ + 5 minör ───────────────
            if (cueKoz == "Kupa")
            {
                // ASLA kupa yükseltme (rakip rengi / Güney'in açılışı)
                // Maça tercihi (partner 5♠ vaat etti)
                if (maca >= 3)
                {
                    if (hp >= 10 && maca >= 4) return "3♠";
                    return "2♠";
                }
                // 8+ HP, maça yok → 2NT (partner minörünü söylesin)
                if (hp >= 8) return "2NT";
                // Zayıf + maça fit yok → Pas (3 seviye minör tahmini riskli)
                return "Pas";
            }

            // ─── Rakip 1♠ / cue 2♠ → partner 5♥ + 5 minör ───────────────
            if (cueKoz == "Maça")
            {
                // ASLA maça yükseltme
                if (kupa >= 3)
                {
                    if (hp >= 10 && kupa >= 4) return "4♥";
                    return "3♥"; // 2♥ cue sonrası kupa 3 seviyesinde
                }
                if (hp >= 8) return "2NT";
                if (sinek >= karo && sinek >= 3) return "3♣";
                if (karo >= 3) return "3♦";
                return "Pas";
            }

            // ─── Rakip minör / cue 2♣ veya 2♦ → partner her iki majör ───
            if (cueKoz == "Sinek" || cueKoz == "Karo")
            {
                // Daha uzun majörü tercih et
                if (maca >= kupa && maca >= 3)
                {
                    if (hp >= 10 && maca >= 4) return "3♠";
                    return "2♠";
                }
                if (kupa >= 3)
                {
                    if (hp >= 10 && kupa >= 4) return "3♥";
                    return "2♥";
                }
                if (hp >= 8) return "2NT";
                return "Pas";
            }

            return "Pas";
        }
    }
}
