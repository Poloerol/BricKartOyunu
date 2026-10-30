using System.Collections.Generic;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣-1♦ sonrası açanın 2. tur rebidlerine cevapçının cevapları ve açanın röle cevapları.
    /// Kaynak: SAS kitabı s.11-14 ("1♣ ⇒ 1♦ gelişmeleri"). Cevapçı: 10+ HCP 4+♦ veya 5+♦ dengesiz 5-9 HCP.
    /// Güç sınıfları: zayıf ≤ 9, davet 9-11, zon forsing / şlem daveti (SI) 12+.
    /// Tabloda karşılığı olmayan durumlarda null döner (sınıf devreye girmez, diğer konvansiyonlar çalışır).
    /// </summary>
    public static class OneClub_Devam
    {
        public static string CevapciDevam(List<Card> el, string aciciRebid)
        {
            switch (aciciRebid)
            {
                case "1♥": return MajorRebidCevap(el, true);
                case "1♠": return MajorRebidCevap(el, false);
                case "1NT": return NTRebidCevap(el);
                case "2♣": return IkiSinekRebidCevap(el);
                case "2♦": return IkiKaroRebidCevap(el);
            }
            return null;
        }

        /// <summary>
        /// 1♣-1♦-1M (4+♣ 4M 11-17) sonrası cevapçı. acanKupa=true: 1♥, false: 1♠.
        /// </summary>
        public static string MajorRebidCevap(List<Card> el, bool acanKupa)
        {
            int hcp = ElDegerlendirici.HCP(el);
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // 3NT: 4♦-3-3-3 12-14 HCP 0-3 kontrol
            if (hcp >= 12 && hcp <= 14 && ElDegerlendirici.Dagilim4333(el, "Karo")
                && ElDegerlendirici.KontrolSayisi(el) <= 3)
                return "3NT";

            // 2♦: 6+♦ 5-9 HCP
            if (d >= 6 && hcp <= 9) return "2♦";

            // Fit: 4+♦ 4M (açanın majörüne 4 kart)
            int fit = acanKupa ? h : s;
            if (fit >= 4 && d >= 4)
            {
                if (hcp >= 12)
                {
                    if (acanKupa && s <= 1) return "3♠";   // 4+♦ 4♥ SNG ♠ hafif SI
                    if (c <= 1) return "4♣";                // 4+♦ 4M SNG ♣ hafif SI
                    return "2♣";                            // Röle (SI)
                }
                if (hcp >= 9)
                {
                    bool sng = s <= 1 || h <= 1 || c <= 1;
                    if (sng) return acanKupa ? "3♥" : "3♠"; // davet w SNG
                    return acanKupa ? "2♥" : "2♠";          // davet w/o SNG
                }
                return "Pas";
            }

            // 1♠: 4+♦ 4♠ natürel F1R (yalnız 1♥ rebidinden sonra)
            if (acanKupa && s >= 4 && d >= 4) return "1♠";

            // 3♦: 6+♦ davet
            if (d >= 6 && hcp >= 10 && hcp <= 11) return "3♦";

            // 5+♦ 4+♣
            if (d >= 5 && c >= 4)
            {
                if (hcp >= 12)
                {
                    if (!acanKupa && h <= 1) return "3♥";   // 1♠ rebidinden sonra: 5+♦ 4+♣ SNG ♥ hafif SI
                    return acanKupa ? "2♠" : "2♥";          // 4. renk (SI)
                }
                if (hcp >= 9) return "3♣";                  // 5+♦ 4/5♣ davet
            }

            if (hcp >= 12) return "2♣";                     // Röle
            return "Pas";
        }

        // 1♣-1♦-1NT (11-14 dengeli, 4'lü majör olabilir)
        private static string NTRebidCevap(List<Card> el)
        {
            int hcp = ElDegerlendirici.HCP(el);
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            if (hcp >= 12)
            {
                if (d >= 6) return "3♦";                          // 6+♦ FG
                if (d >= 5 && s >= 4 && h <= 1) return "3♥";      // 5♦ 4♠ SNG ♥ FG
                if (d >= 5 && h >= 4 && s <= 1) return "3♠";      // 5♦ 4♥ SNG ♠ FG
                return "2♦";                                      // FG röle
            }
            if (hcp >= 9)
            {
                if (d >= 4 && h >= 4) return "2♥";                // 4♦ 4♥ davet
                if (d >= 4 && s >= 4) return "2♠";                // 4♦ 4♠ davet
                if (d >= 4 && c >= 5) return "3♣";                // 4♦ 5+♣ davet
            }
            if (hcp <= 9 && d >= 5) return "2♣";                  // Puppet to 2♦ (zayıf, ♦'da oynamak)
            return "Pas";
        }

        // 1♣-1♦-2♣ (6+♣ 11-16)
        private static string IkiSinekRebidCevap(List<Card> el)
        {
            int hcp = ElDegerlendirici.HCP(el);
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            if (d >= 6 && hcp <= 9) return "2♦";                  // S.O.
            if (hcp >= 12)
            {
                if (d >= 5 && s >= 4) return "2♠";                // 5+♦ 4♠ FG
                if (c >= 3 && h <= 1) return "3♥";                // 3+♣ SNG ♥ FG
                if (c >= 3 && s <= 1) return "3♠";                // 3+♣ SNG ♠ FG
                return "2♥";                                      // Röle
            }
            if (hcp >= 9)
            {
                if (d >= 6) return "3♦";                          // 6+♦ davet
                if (c >= 3) return "3♣";                          // davet
            }
            return "Pas";
        }

        // 1♣-1♦-2♦ (4+♣ 4♦ 11-14, ♦ desteği)
        private static string IkiKaroRebidCevap(List<Card> el)
        {
            int hcp = ElDegerlendirici.HCP(el);
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            if (hcp >= 12)
            {
                if (d >= 5 && s >= 4) return "2♠";                // 5+♦ 4♠ FG
                if (h <= 1) return "3♥";                          // 4+♦ SNG ♥ FG
                if (s <= 1) return "3♠";                          // 4+♦ SNG ♠ FG
                return "2♥";                                      // Röle
            }
            if (hcp >= 9 && d >= 5 && c >= 4) return "3♣";       // 5♦ 4♣ 9-11
            return "Pas";
        }

        /// <summary>Açanın, cevapçının 2. tur hamlesine (röle vb.) cevabı. Tabloda yoksa null.</summary>
        public static string AciciCevap(List<Card> el, string aciciRebid, string cevapciHamle)
        {
            int hcp = ElDegerlendirici.HCP(el);
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            if (aciciRebid == "1NT" && cevapciHamle == "2♣")
                return "2♦";                                      // Puppet to 2♦

            if (aciciRebid == "2♣" && cevapciHamle == "2♥")
            {
                // 6+♣ 11-16: min 11-13, max 14-16. 2♠ min w 3♦ · 2NT max · 3♣ min · 3♦ max w 3♦
                bool max = hcp >= 14;
                if (d >= 3) return max ? "3♦" : "2♠";
                return max ? "2NT" : "3♣";
            }

            if (aciciRebid == "2♦" && cevapciHamle == "2♥")
            {
                // 2♠ = 5♣4♦ MIN · 2NT = 4♣4♦ dengeli · 3♣ = 5♣4♦ MAX
                bool max = hcp >= 13;
                if (c >= 5) return max ? "3♣" : "2♠";
                return "2NT";
            }
            return null;
        }
    }
}
