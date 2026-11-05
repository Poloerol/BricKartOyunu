using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>
    /// 1♦ ⇒ 1♥ GELİŞİMİ (SAS kitabı s.25-27): açanın yeniden teklifi, cevapçı devamı, röle cevapları.
    /// Cevapçı gücü: zayıf 4-8, davet 9-11, oyun 12+ HCP (kitapta açık olmayan yerler "SEÇİM").
    /// </summary>
    public class OneDiamond_OneHeart : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♦"; } }
        protected override string Cevap { get { return "1♥"; } }
        public override string Aciklama { get { return "1♦-1♥ gelişimi (kitap s.25-27)"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            if (d >= 6 && h >= 4 && hcp >= 13 && hcp <= 16)
            {
                if (hcp >= 14 && c <= 1) return "4♣";      // 6♦4♥ SNG ♣
                if (hcp >= 14 && s <= 1) return "4♦";      // 6♦4♥ SNG ♠
                return "4♥";
            }
            if (hcp >= 18 && hcp <= 20 && d == 5 && h == 4 && s == 2 && c == 2) return "3NT";
            if (hcp >= 18 && d == 4 && h == 4 && s == 4 && c == 1) return "3♠";
            if (hcp >= 18 && (d >= 6 || (d >= 5 && h >= 4))) return "2NT";
            if (hcp >= 18 && d >= 5 && s >= 4) return "2♠";
            if (hcp >= 14 && hcp <= 17 && ((d >= 5 && h >= 4 && hcp >= 15) || (d == 4 && h == 4 && c == 4 && s == 1 && hcp <= 16))) return "3♥";
            if (hcp >= 14 && hcp <= 17 && d == 5 && c == 5) return "3♣";
            if (hcp >= 15 && hcp <= 17 && d >= 6) return "3♦";
            if (h >= 3 && hcp <= 15) return "2♥";
            if (h >= 4) return "3♥";                         // SEÇİM: 16+ HCP 4♥ fit
            if (s >= 4 && d >= 4) return "1♠";
            if (d >= 5 && c >= 4) return "2♣";
            if (d >= 6) return "2♦";
            return "1NT";
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "1♠" || rebid == "1NT" || rebid == "2♣" || rebid == "2♦" || rebid == "2♥" || rebid == "2♠" || rebid == "2NT";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            bool zayif = hcp <= 8, davet = hcp >= 9 && hcp <= 11, oyun = hcp >= 12;
            switch (rebid)
            {
                case "1♠":   // 4+♦ 4♠ 11-17
                    return Sec(new[] {
                        K("3NT", x => hcp >= 12 && hcp <= 14 && x.H == 4 && x.S == 3 && x.D == 3 && x.C == 3),
                        K("4♣", x => oyun && x.H >= 4 && x.S >= 4 && x.C <= 1),
                        K("4♦", x => oyun && x.H >= 4 && x.S >= 4 && x.D <= 1),
                        K("3♣", x => oyun && x.H >= 5 && x.D >= 4),                    // 5+♥ 4+♦ FG
                        K("3♠", x => davet && x.H >= 4 && x.S >= 4),                   // davet
                        K("2♠", x => hcp <= 9 && x.H >= 4 && x.S >= 4),                // 7-9 destek
                        K("3♦", x => davet && x.H >= 4 && x.D >= 4),
                        K("3♥", x => davet && x.H >= 6),
                        K("2♥", x => zayif && x.H >= 5),
                        K("2NT", x => zayif && x.C >= 6),                              // 2NTt
                        K("2♦", x => zayif && x.D >= 4),
                        K("2♣", x => hcp >= 9),                                        // röle davet+
                    }, e, "1NT");
                case "1NT":  // 4+♦ dengeli 11-14
                    return Sec(new[] {
                        K("3♣", x => oyun && x.H >= 5 && x.D >= 4 && x.C <= 1),
                        K("3♠", x => oyun && x.H >= 5 && x.D >= 4 && x.S <= 1),
                        K("3♥", x => oyun && x.H >= 6),
                        K("3NT", x => oyun && x.Dengeli),
                        K("2♦", x => oyun),                                            // röle FG
                        K("3♦", x => davet && x.H == 4 && x.D >= 5),
                        K("2♠", x => davet && x.H >= 4 && x.S >= 4),
                        K("2♥", x => hcp <= 9 && x.H >= 5),
                        K("2NT", x => zayif && x.C >= 6),
                        K("2♣", x => zayif && x.D >= 5),                               // 2♦'e puppet
                    }, e, "Pas");
                case "2♣":   // 5+♦ 4+♣ 11-21 F1R
                    return Sec(new[] {
                        K("3♠", x => oyun && x.H >= 6 && x.S >= 5),
                        K("3NT", x => oyun && x.Dengeli && x.H == 4 && x.Stopper("Maça")),
                        K("3♥", x => davet && x.H >= 6),
                        K("3♣", x => davet && x.H >= 4 && x.C >= 4),
                        K("3♦", x => davet && x.H >= 4 && x.D >= 4),
                        K("2♠", x => oyun),                                            // 4. renk F1R
                        K("2♥", x => hcp <= 9 && x.H >= 5),
                        K("2NT", x => zayif && x.C >= 4),
                    }, e, "2♦");
                case "2♦":   // 6+♦ 11-16
                    return Sec(new[] {
                        K("4♦", x => hcp >= 15 && x.D >= 3),                           // RKCB ♦
                        K("4♣", x => oyun && x.D >= 3 && x.C <= 1),
                        K("3♠", x => oyun && x.D >= 3 && x.S <= 1),
                        K("3NT", x => oyun && x.H == 4 && x.Dengeli),
                        K("3♥", x => davet && x.H >= 6),
                        K("3♣", x => davet && x.H >= 5 && x.C >= 5),
                        K("3♦", x => davet && x.D >= 2),
                        K("2♠", x => hcp >= 10),                                       // röle F1R
                        K("2♥", x => hcp <= 9 && x.H >= 5),
                        K("2NT", x => zayif && x.C >= 6),
                    }, e, "Pas");
                case "2♥":   // 3+♥ 11-15
                    return Sec(new[] {
                        K("4♣", x => oyun && x.H >= 5 && x.C <= 1),
                        K("4♦", x => oyun && x.H >= 5 && x.D <= 1),
                        K("3♠", x => oyun && x.H >= 5 && x.S <= 1),
                        K("2NT", x => oyun && x.H >= 5 && x.S >= 4),
                        K("3♣", x => oyun && x.H >= 5 && x.C >= 4),
                        K("3NT", x => oyun && x.H == 4 && x.Dengeli),
                        K("4♥", x => oyun && x.H >= 5),
                        K("3♦", x => davet && x.H == 4 && x.D >= 3),
                        K("3♥", x => zayif && x.H >= 6),                               // bomba
                        K("2♠", x => hcp >= 9),                                        // röle davet+
                    }, e, "Pas");
                case "2♠":   // 5+♦ 4♠ 18+ FG
                    return Sec(new[] {
                        K("2NT", x => hcp <= 6),                                       // Lebensohl
                        K("4♣", x => x.H >= 4 && x.S >= 4 && x.C <= 1),
                        K("4♦", x => x.H >= 4 && x.S >= 4 && x.D <= 1),
                        K("4♠", x => x.H >= 4 && x.S >= 4 && hcp <= 8),
                        K("3♥", x => x.H >= 4 && x.S >= 4),
                        K("3NT", x => hcp <= 10 && x.H >= 4 && x.Stopper("Sinek")),
                        K("3♠", x => x.H >= 6),
                        K("3♦", x => x.D >= 3),
                    }, e, "3♣");                                                       // 4. renk
                case "2NT":  // 18+ 6+♦ ya da 5+♦ 4♥
                    return Sec(new[] {
                        K("4♦", x => hcp >= 12 && x.D >= 3),                           // RKCB
                        K("4♣", x => hcp >= 7 && x.H >= 5 && x.D >= 3 && x.C <= 1),
                        K("3♠", x => hcp >= 7 && x.H >= 5 && x.D >= 3 && x.S <= 1),
                        K("3♥", x => hcp >= 7 && x.H >= 6),
                        K("3NT", x => hcp <= 8 && x.H == 4 && x.Dengeli),
                        K("3♣", x => hcp >= 9),                                        // röle
                    }, e, "3♦");                                                       // min
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            bool min = e.Hcp <= 13;
            if (devam == "2NT" && (rebid == "1♠" || rebid == "1NT" || rebid == "2♦" || rebid == "2♠")) return "3♣";   // 2NTt / Lebensohl
            if (rebid == "2♣" && devam == "2♠")      // 4. renk: 2NT 5♦4♣ min ♠ stoper; 3♣ 5-5; 3♦ 6♦4♣; 3♥ 3♥ 16+; 3NT max; 3♠ stoper yok 15+
            {
                bool stop = e.Stopper("Maça");
                if (e.D >= 5 && e.C >= 5 && e.Hcp >= 18) return "4♣";
                if (e.D >= 5 && e.C >= 5) return "3♣";
                if (e.D >= 6 && e.C >= 4) return "3♦";
                if (e.H >= 3 && e.Hcp >= 16) return "3♥";
                if (e.Hcp >= 15 && stop) return "3NT";
                if (e.Hcp >= 15) return "3♠";
                return "2NT";
            }
            if (rebid == "2♦" && devam == "2♠")      // röle: 3♣ min w 3♥ / 3♦ min w/o 3♥ / 2NT max w/o 3♥ / 3♥ max w 3♥
            {
                if (e.H >= 3) return min ? "3♣" : "3♥";
                return min ? "3♦" : "2NT";
            }
            if (rebid == "2♥" && devam == "2♠")      // 2NT 4♥ SNG; 3♣ 3♥ min; 3♦ 3♥ max; 3♥ 4♥ SNG'siz min; 3♠ max
            {
                if (e.H == 3) return min ? "3♣" : "3♦";
                if (e.Singleton) return "2NT";
                return min ? "3♥" : "3♠";
            }
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (devam == "2NT" && roleCevap == "3♣" && (rebid == "1♠" || rebid == "1NT" || rebid == "2♦" || rebid == "2♠")) return "Pas";
            if (rebid == "2♦" && devam == "2♠")
            {
                if (roleCevap == "2NT") return e.H >= 6 ? "3♥" : null;
                if (roleCevap == "3♣") return e.H >= 6 ? "3♥" : "3♦";
                if (roleCevap == "3♦") return (e.Hcp >= 12 && e.H >= 6) ? "3♥" : null;
                if (roleCevap == "3♥") return e.Hcp >= 12 ? "3♠" : null;     // ♠ stoper sorar
            }
            if (rebid == "2♥" && devam == "2♠" && roleCevap == "2NT") return "3♣";                // röle
            return null;
        }

        internal override string AciciSon(OcEl e, string rebid, string devam, string roleCevap, string karar)
        {
            if (rebid == "2♥" && devam == "2♠" && roleCevap == "2NT" && karar == "3♣")   // KK min / BK min / KK max / BK max
            {
                bool min = e.Hcp <= 13, kk = e.C <= 1;
                if (min) return kk ? "3♦" : "3♥";
                return kk ? "3♠" : "3NT";
            }
            return null;
        }
    }
}
