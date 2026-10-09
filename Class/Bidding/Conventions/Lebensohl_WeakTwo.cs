using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Lebensohl Konvansiyonu (Rakibin Zayıf 2 Açışları Üzerine).
    /// 
    /// SENARYO: (2♥/2♠ zayıf) - DBL (Partner) - ?
    /// 
    /// AMAÇ:
    /// 1. Zayıf (0-6 OP) ve davet (7-11 OP) ve kuvvetli (12+ OP) elleri ayırt etmek
    /// 2. DBL atanın ortağına sign-off imkanı vermek
    /// 
    /// Öncelik: 7 (Lebensohl 8'den önce)
    /// </summary>
    public class Lebensohl_WeakTwo : IKonvansiyon
    {
        public string Ad => "Lebensohl (Zayıf 2)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 7;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // 1. Rakip zayıf 2 açtı mı?
            if (durum.RakipTeklifleri.Count == 0) return false;
            string rakipSon = durum.RakipTeklifleri.LastOrDefault();
            if (rakipSon != "2♥" && rakipSon != "2♠") return false;

            // 2. Partner DBL attı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            if (durum.PartnerTeklifleri.LastOrDefault() != "Dbl") return false;

            // 3. Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string rakipSon = durum.RakipTeklifleri.LastOrDefault();
            int hp = ElDegerlendirici.HCP(el);

            string rakipKozu = rakipSon == "2♥" ? "Kupa" : "Maça";

            // ═══════════════════════════════════════════════════════════════
            // DURUM 1: İlk cevabım (rakip zayıf 2 açtı, partner DBL)
            // ═══════════════════════════════════════════════════════════════
            if (durum.KendiTeklifleri.Count == 0)
            {
                int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                bool durdurucuVar = ElDegerlendirici.StopperVar(el, rakipKozu);

                // 0-6 OP zayıf
                if (hp <= 6)
                {
                    // Rakip rengi 2♥ ise 2♠ natürel
                    if (rakipSon == "2♥" && maca >= 5) return "2♠";

                    // Diğer durumlar → 2NT Lebensohl
                    return "2NT";
                }

                // 7-11 OP davet
                if (hp >= 7 && hp <= 11)
                {
                    // 5'li majör (rakip rengi değil)
                    if (rakipSon == "2♥" && maca >= 5) return "3♠";
                    if (rakipSon == "2♠" && kupa >= 5) return "3♥";

                    // 6'lı minör
                    if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 6) return "3♦";
                    if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 6) return "3♣";

                    // Varsayılan davet
                    return "3♣";
                }

                // 12+ OP kuvvetli
                if (hp >= 12)
                {
                    // Rakip renginde durdurucu + 4'lü majör yok → 3NT
                    if (durdurucuVar && maca < 4 && kupa < 4) return "3NT";

                    // 4'lü majör varsa → Cue-bid (Stayman benzeri)
                    // 2♥ için: 3♥ = Stayman
                    // 2♠ için: 3♠ = Stayman
                    if (rakipSon == "2♥" && maca >= 4) return "3♥";
                    if (rakipSon == "2♠" && kupa >= 4) return "3♠";

                    // 5'li majör (forcing)
                    if (rakipSon == "2♥" && maca >= 5) return "3♠";
                    if (rakipSon == "2♠" && kupa >= 5) return "3♥";

                    return "3NT";
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // DURUM 2: Lebensohl 2NT sonrası (partner 3♣ dedi)
            // ═══════════════════════════════════════════════════════════════
            if (durum.KendiTeklifleri.Contains("2NT") &&
                durum.PartnerTeklifleri.LastOrDefault() == "3♣")
            {
                // 0-6 OP sign-off
                if (hp <= 6)
                {
                    int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                    int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                    // 5'li majör varsa
                    if (rakipSon == "2♥" && maca >= 5) return "3♠";
                    if (rakipSon == "2♠" && kupa >= 5) return "3♥";

                    // 6'lı minör
                    if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 6) return "3♦";

                    // 3♣ oynamak için
                    return "Pas";
                }

                // Diğer durumlar
                int m2 = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int k2 = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                if (rakipSon == "2♥" && m2 >= 5) return "3♠";
                if (rakipSon == "2♠" && k2 >= 5) return "3♥";

                return "3NT";
            }

            return "Pas";
        }
    }
}