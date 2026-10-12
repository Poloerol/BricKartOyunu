using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Multi 2♦ — Zayıf İki'ye alternatif açılış sistemi.
    /// 
    /// AÇILIŞ: 2♦ = 6'lı ♥ VEYA 6'lı ♠ (hangisi olduğu bilinmez), 5-10 HP.
    ///   - Tam 6 kartlı majör (7+ kart → 3 seviyesi Preempt)
    ///   - Diğer majörde en fazla 3 kart
    ///   - Güçlü eller (20+ HP) bu sürümde Multi'ye dahil DEĞİL → 2♣ / 2NT açılır
    ///
    /// CEVAPÇI (2♦ sonrası, rakip müdahalesi yok):
    ///   - 0-10 HP : 2♥ = "pas ya da düzelt" (♥ ≥ ♠ ise)
    ///               2♠ = "pas ya da düzelt" (♠ > ♥ ise)
    ///   - 11+ HP  : 2NT = soru (açıcı renk + güç anlatır)
    /// 
    /// AÇICI REBID:
    ///   2♦ - 2♥ : ♥ varsa Pas, ♠ varsa 2♠ (düzelt)
    ///   2♦ - 2♠ : ♠ varsa Pas, ♥ varsa 3♥ (düzelt)
    ///   2♦ - 2NT: 3♣ = minimum (5-7) ♥ | 3♦ = minimum ♠
    ///             3♥ = maksimum (8-10) ♥ | 3♠ = maksimum ♠
    /// 
    /// Anlaşma: OrtaklikAnlasmasi.Multi2D (varsayılan KAPALI).
    /// Açıkken Zayıf İki devre dışı kalır (2♦ artık Multi'dir).
    /// 
    /// Öncelik: 7 (Zayıf İki 8'den önce)
    /// </summary>
    public class Multi2D : IKonvansiyon
    {
        public string Ad => "Multi 2♦";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 7;

        private const int MinHp = 5;
        private const int MaxHp = 10;
        private const int MinimumUstSinir = 7;   // 5-7 minimum, 8-10 maksimum
        private const int SoruMinHp = 11;        // 2NT sorusu için

        // ═══════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();

            if (gercek.Count == 0) return AcilisUygunMu(durum);
            if (gercek.Count == 1) return CevapUygunMu(durum, gercek);
            if (gercek.Count == 2) return AciciRebidUygunMu(durum, gercek);
            return false;
        }

        private static bool AcilisUygunMu(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            if (hp < MinHp || hp > MaxHp) return false;
            return AciciMajoru(el) != null;
        }

        private static bool CevapUygunMu(IhaleDurumu durum, List<IhaleHamlesi> gercek)
        {
            // Tek gerçek teklif: partnerin 2♦ açılışı (rakip müdahalesi yok)
            return gercek[0].Oyuncu == durum.Partner && gercek[0].Teklif == "2♦";
        }

        private static bool AciciRebidUygunMu(IhaleDurumu durum, List<IhaleHamlesi> gercek)
        {
            // Ben 2♦ açtım, partner 2♥/2♠/2NT dedi, rakip sessiz
            if (gercek[0].Oyuncu != durum.AktifOyuncu || gercek[0].Teklif != "2♦") return false;
            if (gercek[1].Oyuncu != durum.Partner) return false;

            string t = gercek[1].Teklif;
            if (t != "2♥" && t != "2♠" && t != "2NT") return false;

            return AciciMajoru(durum.AktifOyuncuEli) != null;
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            if (!UygunMu(durum)) return "Pas";

            var el = durum.AktifOyuncuEli;
            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();

            // ─── Açılış ───────────────────────────────────────────────
            if (gercek.Count == 0) return "2♦";

            // ─── Cevapçı ──────────────────────────────────────────────
            if (gercek.Count == 1)
            {
                int hp = ElDegerlendirici.HCP(el);
                if (hp >= SoruMinHp) return "2NT";

                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                return kupa >= maca ? "2♥" : "2♠";
            }

            // ─── Açıcı rebid ──────────────────────────────────────────
            string majör = AciciMajoru(el);
            string cevap = gercek[1].Teklif;

            if (cevap == "2♥") return majör == "Kupa" ? "Pas" : "2♠";
            if (cevap == "2♠") return majör == "Maça" ? "Pas" : "3♥";

            // 2NT sorusu: minimum/maksimum + renk
            bool minimum = ElDegerlendirici.HCP(el) <= MinimumUstSinir;
            if (majör == "Kupa") return minimum ? "3♣" : "3♥";
            return minimum ? "3♦" : "3♠";
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Multi açılışına uygun majörü bulur: tam 6 kart, diğer majör ≤ 3.
        /// Uygun değilse null.
        /// </summary>
        private static string AciciMajoru(List<Card> el)
        {
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");

            if (kupa == 6 && maca <= 3) return "Kupa";
            if (maca == 6 && kupa <= 3) return "Maça";
            return null;
        }
    }
}
