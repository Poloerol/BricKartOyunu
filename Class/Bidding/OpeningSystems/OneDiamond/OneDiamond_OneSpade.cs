using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>
    /// 1♦ ⇒ 1♠ GELİŞİMİ (SAS kitabı s.28-30): açanın yeniden teklifi, cevapçı devamı, röle cevapları.
    /// </summary>
    public class OneDiamond_OneSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♦"; } }
        protected override string Cevap { get { return "1♠"; } }
        public override string Aciklama { get { return "1♦-1♠ gelişimi (kitap s.28-30)"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            if (d >= 6 && s >= 4 && hcp >= 13 && hcp <= 16)
            {
                if (hcp >= 14 && c <= 1) return "4♣";      // KK
                if (hcp >= 14 && h <= 1) return "4♦";      // BK
                return "4♠";
            }
            if (hcp >= 18 && hcp <= 20 && d == 5 && s == 4 && h == 2 && c == 2) return "3NT";
            if (hcp >= 18 && d == 4 && h == 4 && s == 4 && c == 1) return "3♥";
            if (hcp >= 18 && (d >= 6 || (d >= 5 && s >= 4))) return "2NT";
            if (hcp >= 18 && d >= 5 && h >= 4) return "2♥";
            if (hcp >= 14 && hcp <= 17 && d >= 5 && s >= 4 && hcp >= 15) return "3♠";
            if (hcp >= 14 && hcp <= 17 && d == 5 && c == 5) return "3♣";
            if (hcp >= 14 && hcp <= 17 && d >= 6) return "3♦";
            if (s >= 3 && hcp <= 15) return "2♠";
            if (s >= 4) return "3♠";                         // SEÇİM: 16+ HCP 4♠ fit
            if (d >= 5 && c >= 4) return "2♣";
            if (d >= 6) return "2♦";
            return "1NT";
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "1NT" || rebid == "2♣" || rebid == "2♦" || rebid == "2♥" || rebid == "2♠" || rebid == "2NT";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            bool zayif = hcp <= 8, davet = hcp >= 9 && hcp <= 11, oyun = hcp >= 12;
            switch (rebid)
            {
                case "1NT":  // 4+♦ dengeli 11-14
                    return Sec(new[] {
                        K("3♣", x => oyun && x.S >= 5 && x.D >= 4 && x.C <= 1),
                        K("3♥", x => oyun && x.S >= 5 && x.D >= 4 && x.H <= 1),
                        K("3♠", x => oyun && x.S >= 6),
                        K("3NT", x => oyun && x.Dengeli),
                        K("2♦", x => oyun),
                        K("3♦", x => davet && x.S == 4 && x.D >= 5),
                        K("2♥", x => hcp <= 9 && x.S >= 6 && x.H >= 4),
                        K("2♠", x => hcp <= 9 && x.S >= 5),
                        K("2NT", x => zayif && x.C >= 6),
                        K("2♣", x => zayif && x.D >= 5),
                    }, e, "Pas");
                case "2♣":   // 5+♦ 4+♣ F1R
                    return Sec(new[] {
                        K("4♣", x => hcp >= 15 && x.C >= 3 && x.S >= 4),                // RKCB ♣
                        K("3NT", x => oyun && x.S == 4 && x.Dengeli && x.Stopper("Kupa")),
                        K("3♠", x => davet && x.S >= 6),
                        K("3♥", x => davet && x.S >= 5 && x.H >= 5),
                        K("3♣", x => davet && x.S >= 4 && x.C >= 4),
                        K("3♦", x => davet && x.S >= 4 && x.D >= 4),
                        K("2♥", x => oyun),                                            // 4. renk F1R
                        K("2♠", x => hcp <= 9 && x.S >= 5),
                        K("2NT", x => zayif && x.C >= 4),
                    }, e, "2♦");
                case "2♦":   // 6+♦ 11-16
                    return Sec(new[] {
                        K("4♦", x => hcp >= 15 && x.D >= 3),
                        K("4♣", x => oyun && x.D >= 3 && x.C <= 1),
                        K("3NT", x => oyun && x.S == 4 && x.Dengeli),
                        K("3♠", x => davet && x.S >= 6),
                        K("3♥", x => davet && x.S >= 5 && x.H >= 5),
                        K("3♣", x => davet && x.S >= 5 && x.C >= 5),
                        K("3♦", x => davet && x.S == 4 && x.D >= 2),
                        K("2♥", x => hcp >= 10),                                       // röle davet+
                        K("2♠", x => hcp <= 9 && x.S >= 5),
                        K("2NT", x => zayif && x.C >= 6),
                    }, e, "Pas");
                case "2♥":   // 5+♦ 4♥ 18+
                    return Sec(new[] {
                        K("2NT", x => hcp <= 6),
                        K("4♣", x => x.S >= 4 && x.H >= 4 && x.C <= 1),
                        K("4♦", x => x.S >= 4 && x.H >= 4 && x.D <= 1),
                        K("4♥", x => x.S >= 4 && x.H >= 4 && hcp <= 8),
                        K("3♥", x => x.S >= 4 && x.H >= 4),
                        K("3NT", x => hcp <= 10 && x.S >= 4 && x.Stopper("Sinek")),
                        K("3♠", x => x.S >= 6),
                        K("3♦", x => x.D >= 3),
                        K("2♠", x => hcp <= 8 && x.S >= 5),
                    }, e, "3♣");
                case "2♠":   // 3+♠ 11-15
                    return Sec(new[] {
                        K("4♣", x => oyun && x.S >= 5 && x.C <= 1),
                        K("4♦", x => oyun && x.S >= 5 && x.D <= 1),
                        K("4♥", x => oyun && x.S >= 5 && x.H <= 1),
                        K("3♣", x => oyun && x.S >= 5 && x.C >= 4),
                        K("3♥", x => oyun && x.S >= 5 && x.H >= 4),
                        K("3NT", x => oyun && x.S == 4 && x.Dengeli),
                        K("4♠", x => oyun && x.S >= 5),
                        K("3♦", x => davet && x.S == 4 && x.D >= 3),
                        K("3♠", x => zayif && x.S >= 6),
                        K("2NT", x => hcp >= 9),
                    }, e, "Pas");
                case "2NT":  // 18+ 6+♦ ya da 5+♦ 4♠
                    return Sec(new[] {
                        K("4♦", x => hcp >= 12 && x.D >= 3),
                        K("4♣", x => hcp >= 7 && x.S >= 5 && x.D >= 3 && x.C <= 1),
                        K("3♥", x => hcp >= 7 && x.S >= 5 && x.D >= 3 && x.H <= 1),
                        K("3♠", x => hcp >= 7 && x.S >= 6),
                        K("3NT", x => hcp <= 8 && x.S == 4 && x.Dengeli),
                        K("3♣", x => hcp >= 9),
                    }, e, "3♦");
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            bool min = e.Hcp <= 13;
            if (devam == "2NT" && (rebid == "1NT" || rebid == "2♦" || rebid == "2♥")) return "3♣";    // 2NTt / Lebensohl
            if (rebid == "2♣" && devam == "2♥")      // 2♠ 5♦4♣2♠ ♠ onör; 2NT min ♥ stoper; 3♣ 5-5; 3♦ 6♦4♣; 3♥ 15+ stoper sorar; 3♠ 3♠ 15+; 3NT max stoper
            {
                bool onor = e.El.Any(k => k.Suit == "Maça" && k.Value >= 12);
                if (e.D >= 5 && e.C >= 5 && e.Hcp >= 18) return "4♣";
                if (e.D >= 6 && e.C >= 4 && e.Hcp >= 18) return "4♦";
                if (e.D >= 5 && e.C >= 5) return "3♣";
                if (e.D >= 6) return "3♦";
                if (e.S >= 3 && e.Hcp >= 15) return "3♠";
                if (e.Hcp >= 15) return e.Stopper("Kupa") ? "3NT" : "3♥";
                if (e.S == 2 && onor) return "2♠";
                return "2NT";
            }
            if (rebid == "2♦" && devam == "2♥")     // 2♠ 6♦3♠ min; 3♠ 6♦3♠ max; 3♣ 6♦4♥ min; 3♥ 5+♦4♥ max; 3♦ min w/o; 2NT max w/o
            {
                if (e.S >= 3) return min ? "2♠" : "3♠";
                if (e.H >= 4) return min ? "3♣" : "3♥";
                return min ? "3♦" : "2NT";
            }
            if (rebid == "2♠" && devam == "2NT")     // 3♣ 4♠ SNG; 3♦ 3♠ min; 3♥ 3♠ max; 3♠ 4♠ min; 3NT 4♠ max
            {
                if (e.S == 3) return min ? "3♦" : "3♥";
                if (e.Singleton) return "3♣";
                return min ? "3♠" : "3NT";
            }
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (devam == "2NT" && roleCevap == "3♣" && (rebid == "1NT" || rebid == "2♦" || rebid == "2♥")) return "Pas";
            if (rebid == "2♦" && devam == "2♥" && roleCevap == "2♠")
                return e.Hcp >= 12 ? "2NT" : (e.Hcp >= 9 ? "3♠" : "Pas");
            if (rebid == "2♠" && devam == "2NT" && roleCevap == "3♣") return "3♦";                // röle
            return null;
        }

        internal override string AciciSon(OcEl e, string rebid, string devam, string roleCevap, string karar)
        {
            if (rebid == "2♦" && devam == "2♥" && roleCevap == "2♠" && karar == "2NT")   // 3♣ = 2-2; 3♦ = KK; 3♥ = BK
            {
                if (e.C >= 2 && e.H >= 2) return "3♣";
                return e.C <= 1 ? "3♦" : "3♥";
            }
            if (rebid == "2♠" && devam == "2NT" && roleCevap == "3♣" && karar == "3♦")   // KK min, BK min, KK max, BK max
            {
                bool min = e.Hcp <= 13, kk = e.C <= 1;
                if (min) return kk ? "3♥" : "3♠";
                return kk ? "3NT" : "4♣";
            }
            return null;
        }
    }
}
