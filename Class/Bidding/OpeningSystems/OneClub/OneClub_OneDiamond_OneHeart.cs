using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 1♦ ⇒ 1♥ GELİŞİMİNİN AYRINTILARI (SAS kitabı s.11-12, "DETAYLAR").
    /// Bağımsız bir gelişim sınıfı değildir: OneClub_OneDiamond, 1♥ yeniden teklifinden sonraki
    /// açan/cevapçı turlarında bu tabloları kullanır.
    ///   1♣-1♦-1♥-1♠  → açanın ikinci yeniden teklifi (1NT/2♣/2♦/2♥/2♠/2NT/3♣/3♦/3♠), cevapçı devamı ve röle cevapları
    ///   1♣-1♦-1♥-2♣  → röle: 2♦ (3+♦) / 2♥ (min, 3♦ yok) / 2♠ (max dengesiz) / 2NT (max dengeli)
    /// </summary>
    internal static class OneClub_OneDiamond_OneHeart
    {
        // Açanın 1♥'den sonra cevapçının devamına cevabı.
        internal static string AciciCevap(OcEl e, string devam)
        {
            switch (devam)
            {
                case "2NT": return "3♣";                                  // 2NTt
                case "1♠": return IkinciRebid(e);
                case "2♣":                                                // röle
                    if (e.D >= 3) return "2♦";                            // 3+♦ (min ya da max)
                    if (e.Hcp <= 14) return "2♥";                         // min, 3♦ yok
                    return OneClub_OneDiamond.Duz(e) ? "2NT" : "2♠";                      // max
                default: return null;
            }
        }

        // 1♣-1♦-1♥-1♠: açanın ikinci yeniden teklifi (4+♣ 4♥ 11-17)
        internal static string IkinciRebid(OcEl e)
        {
            int hcp = e.Hcp, c = e.C, h = e.H, d = e.D, s = e.S;
            bool iyiSinek = e.El.Count(k => k.Suit == "Sinek" && k.Value >= 12) >= 2;
            if (c == 4 && h == 4 && s == 4 && d == 1) return hcp >= 15 ? "3♠" : "2♠";   // ♠ fiti, ♦ tek
            if (c == 4 && h == 4 && d == 4 && s == 1 && hcp >= 15) return "3♦";
            if ((c == 5 && h == 4 && d == 3 && s == 1) || (c == 4 && h == 4 && d == 4 && s == 1)) return "2♦";
            if (c == 5 && h == 4 && hcp >= 15) return "2NT";
            if (c >= 6 && hcp >= 15) return iyiSinek ? "3♣" : "2♥";
            if (c >= 6) return "2♣";
            if (hcp >= 15) return "2NT";                                  // SEÇİM: kitapta tanımsız güçlü dengeli el
            return "1NT";
        }

        // Cevapçının kararı: (açan, 1♥) - cevapçı devam - açan cevap → cevapçı
        internal static string CevapciKarar(OcEl e, string devam, string rc)
        {
            int hcp = e.Hcp;
            if (devam == "1♠")
            {
                switch (rc)
                {
                    case "1NT":
                        return Sec(new[] {
                            K("3♦", x => hcp >= 12 && x.D >= 6 && x.S >= 4),                    // 6♦4♠ FG
                            K("3♣", x => hcp >= 9 && hcp <= 11 && x.D >= 5 && x.S >= 4 && x.C >= 3 && x.H == 1),
                            K("2♦", x => hcp >= 9 && hcp <= 11 && x.D >= 6 && x.S >= 4),
                            K("2♣", x => hcp >= 13),                                            // röle SI → gelişmeler doğal
                            K("3NT", x => hcp >= 12),                                           // oynamak için
                            K("2NT", x => hcp >= 9 && hcp <= 11),                               // limit
                        }, e, "Pas");
                    case "2♣":
                        return Sec(new[] {
                            K("2♦", x => hcp >= 10 && hcp <= 11 && x.D >= 6 && x.S >= 4),
                            K("2♥", x => hcp >= 10 && hcp <= 11 && x.D >= 5 && x.S >= 4 && x.H == 3 && x.C == 1),
                            K("2♠", x => hcp >= 12),                                            // röle
                        }, e, "Pas");
                    case "2♦":
                    case "2♥": return "2♠";                                                     // röle
                    case "2NT": return "3♣";                                                    // röle
                    default: return null;
                }
            }
            if (devam == "2♣")
            {
                if (rc == "2♦")                                                                 // 3+♦: min ya da max
                    return Sec(new[] {
                        K("3♥", x => hcp >= 12 && x.D >= 4 && x.H >= 4),                        // 4+♦ 4♥ SI
                        K("2♠", x => hcp >= 9 && hcp <= 11 && x.S >= 4),                        // 4♠ 9-11
                        K("2NT", x => hcp >= 11 && hcp <= 12 && x.D >= 4),                      // 4♦ 11-12
                        K("3♣", x => hcp >= 10 && hcp <= 11 && x.H >= 4 && x.C >= 4),           // 4♥ 4♣ 10-11
                        K("3♦", x => hcp >= 10 && hcp <= 11 && x.D >= 5),                       // 5♦ 10-11
                    }, e, "2♥");                                                                // röle
                if (rc == "2♥") return "2♠";                                                    // röle (min, 3♦ yok)
                return null;                                                                    // 2♠/2NT: gelişim doğal
            }
            return null;
        }

        // Cevapçının kararından sonra açanın cevabı
        internal static string AciciSon(OcEl e, string devam, string rc, string karar)
        {
            int hcp = e.Hcp;
            if (devam == "2♣" && rc == "2♦" && karar == "2♥")     // 3♦'li açan: 2♠/2NT dengeli, 3♣/3♦ 5431
            {
                if (OneClub_OneDiamond.Duz(e)) return hcp <= 12 ? "2♠" : "2NT";
                return hcp <= 14 ? "3♣" : "3♦";
            }
            if (devam == "2♣" && rc == "2♥" && karar == "2♠")     // min, 3♦ yok: 2NT dengeli, 3♣ 11-14, 3♦ 15-17 (5431 ♦ tek)
            {
                if (OneClub_OneDiamond.Duz(e)) return "2NT";
                return hcp <= 14 ? "3♣" : "3♦";
            }
            if (devam == "1♠")
            {
                if ((rc == "2♣" || rc == "2♥") && karar == "2♠") return e.D <= 1 ? "2NT" : "3♣";   // 2NT SNG ♦, 3♣ SNG ♠
                if (rc == "2♦" && karar == "2♠")                                                  // 2NT 5431 14-16, 3♣ 5431 13-14, 3♦ 5431/4441 min
                {
                    if (e.C == 5) return hcp >= 14 ? "2NT" : (hcp >= 13 ? "3♣" : "3♦");
                    return "3♦";
                }
                if (rc == "2NT" && karar == "3♣")                                                 // 3♦ 5422, 3♥ KK, 3♠ BK
                {
                    if (e.D >= 2 && e.S >= 2) return "3♦";
                    return e.D <= 1 ? "3♥" : "3♠";
                }
            }
            return null;
        }
    }
}
