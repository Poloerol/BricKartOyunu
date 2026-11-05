using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneHeart
{
    public class OneHeart_OneSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♥"; } }
        protected override string Cevap { get { return "1♠"; } }
        public override string Aciklama { get { return "1♥-1♠ gelişimi (kitap s.36-37)"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            bool duz = OneClub_OneDiamond.Duz(e);
            if (duz && h == 5 && s == 2 && hcp >= 20) return "3NT";
            if (h >= 6 && s >= 4 && hcp >= 13 && hcp <= 16)
            {
                if (hcp >= 14 && c <= 1) return "4♣";      // KK
                if (hcp >= 14 && d <= 1) return "4♦";      // BK
                return "4♠";
            }
            if (h == 5 && s >= 4 && hcp >= 14 && hcp <= 17) return "3♠";
            if (h >= 7) return "4♥";
            if (h >= 6 && (c >= 4 || d >= 4) && hcp >= 15) return "2NT";
            if (h >= 6 && hcp >= 15 && hcp <= 17) return "3♥";
            if (h == 5 && c == 5 && hcp >= 14 && hcp <= 17) return "3♣";
            if (h == 5 && d == 5 && hcp >= 14 && hcp <= 17) return "3♦";
            if (s >= 4 && h >= 5 && hcp <= 13) return "2♠";
            if (h >= 6 && hcp <= 14) return "2♥";
            if (hcp >= 16) return "2♣";
            if (duz && h == 5 && hcp <= 14) return "1NT";
            if (c >= 3) return "2♣";
            if (d >= 3) return "2♦";
            return "1NT";
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "1NT" || rebid == "2♣" || rebid == "2♦" || rebid == "2♥" || rebid == "2♠" || rebid == "2NT" || rebid == "3♠";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            bool zayif = hcp <= 8, davet = hcp >= 9 && hcp <= 11, oyun = hcp >= 12;
            switch (rebid)
            {
                case "1NT":   // 5♥ dengeli 11-14
                    return Sec(new[] {
                        K("4♣", x => hcp >= 14 && x.S >= 5 && x.H >= 3 && x.C <= 1),     // SNG♣ SI
                        K("4♦", x => hcp >= 14 && x.S >= 5 && x.H >= 3 && x.D <= 1),     // SNG♦ SI
                        K("3♠", x => oyun && x.S >= 6),
                        K("3♥", x => oyun && x.S >= 5 && x.H >= 3),
                        K("3NT", x => oyun && x.Dengeli),
                        K("2♦", x => oyun),                                              // röle FG
                        K("3♣", x => davet && x.S == 4 && x.C >= 5),
                        K("3♦", x => davet && x.S == 4 && x.D >= 5),
                        K("2NT", x => zayif && x.C >= 6),                                // 2NTt → 3♣
                        K("2♣", x => zayif && x.D >= 5),                                 // puppet → 2♦
                        K("2♠", x => zayif && x.S >= 6),
                        K("2♥", x => zayif && x.H >= 3),
                    }, e, "Pas");
                case "2♣":    // 5♥ 3+♣ 11-15 / 5+♥ 16+
                    return Sec(new[] {
                        K("3♠", x => davet && x.S >= 6),
                        K("3♥", x => davet && x.S >= 4 && x.H == 3),
                        K("2NT", x => hcp >= 10 && hcp <= 12 && x.S == 4 && x.Dengeli),
                        K("2♦", x => hcp >= 8),                                          // röle
                        K("3♣", x => x.S == 4 && x.C >= 6),
                        K("3♦", x => x.S == 4 && x.D >= 6),
                        K("2♠", x => x.S >= 5),
                        K("2♥", x => x.S >= 4 && x.H >= 2),
                    }, e, "Pas");
                case "2♦":    // 5♥ 3+♦ 11-15
                    return Sec(new[] {
                        K("4♣", x => hcp >= 14 && x.S >= 5 && x.H >= 3 && x.C <= 1),
                        K("3♠", x => davet && x.S >= 6),
                        K("3♥", x => davet && x.S >= 4 && x.H >= 3),
                        K("3♦", x => davet && x.S >= 4 && x.D >= 4),
                        K("3NT", x => oyun && x.Dengeli),
                        K("3♣", x => oyun),                                              // 4. renk
                        K("2NT", x => hcp >= 10 && x.S == 4 && x.Dengeli),
                        K("2♠", x => x.S >= 5),
                    }, e, "2♥");
                case "2♥":    // 6+♥ 11-14
                    return Sec(new[] {
                        K("4♣", x => hcp >= 14 && x.S >= 5 && x.H >= 3 && x.C <= 1),
                        K("4♦", x => hcp >= 14 && x.S >= 5 && x.H >= 3 && x.D <= 1),
                        K("3♣", x => oyun && x.S >= 5 && x.C >= 3),
                        K("3♦", x => oyun && x.S >= 5 && x.D >= 3),
                        K("3♠", x => davet && x.S >= 6),
                        K("3♥", x => davet && x.S >= 4),
                        K("3NT", x => oyun && x.Dengeli),
                        K("2NT", x => hcp >= 10),
                    }, e, "Pas");
                case "2♠":    // 5♥ 4♠ 11-14
                    return Sec(new[] {
                        K("2NT", x => oyun),                                             // röle: KK/BK/DNG
                        K("3♠", x => hcp >= 9),
                    }, e, "Pas");                                                         // SEÇİM: 3♣/3♦ deneme teklifleri kodlanmadı
                case "2NT":   // 6♥ 4+m 15+
                    return Sec(new[] {
                        K("3♣", x => hcp >= 8),                                          // röle FG
                        K("3♠", x => x.S >= 6),
                    }, e, "3♥");                                                          // S.O
                case "3♠":    // 5♥ 4♠ 14-17
                    return Sec(new[] {
                        K("3NT", x => hcp >= 12),                                        // röle
                        K("4♠", x => hcp >= 8),
                    }, e, "Pas");
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            int hcp = e.Hcp;
            if (rebid == "1NT" && devam == "2♣") return "2♦";                              // puppet
            if (rebid == "1NT" && devam == "2NT") return "3♣";                             // 2NTt
            if (rebid == "2♣" && devam == "2♦")
            {
                if (hcp <= 15 && e.C >= 3 && !(e.S >= 3 && hcp >= 15)) return "2♥";         // min
                if (e.S >= 3 && e.Singleton && hcp >= 15) return "3♠";
                if (OneClub_OneDiamond.Duz(e) && e.S == 2 && hcp >= 15) return "3NT";
                if (e.H >= 6 && hcp >= 16) return "3♥";
                if (e.D >= 4 && hcp >= 16) return "3♦";
                if (e.C >= 4 && hcp >= 16) return "3♣";
                if (e.S >= 3 && hcp >= 16) return "2♠";
                if (hcp >= 18) return "2NT";
                return "2♥";
            }
            if (rebid == "2♠" && devam == "2NT")                                          // KK ; BK ; DNG max ; DNG min
            {
                if (e.C <= 1) return "3♣";
                if (e.D <= 1) return "3♦";
                return hcp >= 13 ? "3♥" : "3♠";
            }
            if (rebid == "2NT" && devam == "3♣")
            {
                if (e.H >= 6 && e.C >= 5) return "4♣";
                if (e.H >= 6 && e.D >= 5) return "4♦";
                if (e.D >= 4) return hcp >= 18 ? "3NT" : "3♥";
                return hcp >= 18 ? "3♠" : "3♦";
            }
            if (rebid == "3♠" && devam == "3NT")
            {
                if (e.C <= 1) return "4♦";                                                // KK
                if (e.D <= 1) return "4♥";                                                // BK
                return hcp >= 16 ? "4♣" : "4♠";                                           // DNG max / min
            }
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if ((rebid == "1NT" && devam == "2♣" && roleCevap == "2♦") ||
                (rebid == "1NT" && devam == "2NT" && roleCevap == "3♣")) return "Pas";
            return null;
        }
    }
}
