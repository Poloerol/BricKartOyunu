using System;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Minör Açılış Konvansiyonu (1♦ / 1♣).
    ///
    /// Kural:
    /// - 12-21 HP aralığında
    /// - 5'li majör YOK (BesliMajor uygun değil)
    /// - Dengeli + 15-17 HP değil (StrongNT uygun değil)
    /// - 22+ HP değil (IkiliSinekGuclu uygun değil)
    ///
    /// Teklif seçimi (SAYC / better-minor uyumlu, 4+ karo zorunlu):
    /// - 1♦ → her zaman en az 4 karo; karo ≥ sinek (4-4 dahil)
    /// - 1♣ → aksi halde (3-3 minör, daha uzun sinek, veya 4+ karo yok)
    ///
    /// Öncelik: 30 (açılış konvansiyonları arasında son)
    ///
    /// Örnek:
    ///   ♠KJ3 ♥Q43 ♦AKQ5 ♣J87  → 3-3-4-3 → "1♦"  (4 karo)
    ///   ♠KJ3 ♥Q43 ♦QJ5  ♣AJ82 → 3-3-3-4 → "1♣"  (3 karo → 1♦ yok)
    ///   ♠KJ3 ♥Q43 ♦AQ52 ♣AJ8  → 3-3-4-3 → "1♦"
    ///   ♠KJ3 ♥Q4  ♦AQ5  ♣AJ832 → 3-2-3-5 → "1♣"  (sinek daha uzun)
    ///   ♠KJ3 ♥Q43 ♦AQ52 ♣AJ8x → 4-4 minör → "1♦"
    /// </summary>
    public class MinorAcilis : IKonvansiyon
    {
        public string Ad => "Minör Açılış";

        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 30 — diğer açılışlardan sonra denenir.
        /// </summary>
        public int Oncelik => 30;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. Sadece ilk teklifte geçerli (paslar dahil)
            if (!durum.IlkTeklifMi()) return false;

            var el = durum.AktifOyuncuEli;

            // 2. HP: 12-21
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 12 || hp > 21) return false;

            // 3. 5'li majör → BesliMajor
            if (ElDegerlendirici.BesliMajorVar(el)) return false;

            // 4. Dengeli 15-17 → StrongNT
            if (hp >= 15 && hp <= 17 && ElDegerlendirici.DengeliEl(el)) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // 1♦ = her zaman 4+ karo.
            //   - 4-4 → 1♦
            //   - karo > sinek ve karo ≥ 4 → 1♦
            // 1♣ = 4+ karo yoksa veya sinek daha uzunsa
            //   - 3-3 → 1♣
            //   - sinek > karo → 1♣
            if (karo >= 4 && karo >= sinek)
                return "1♦";

            return "1♣";
        }
    }
}
