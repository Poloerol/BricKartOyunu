using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneSpade
{
    public class OneSpade_OneNT : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♠"; } }
        protected override string Cevap { get { return "1NT"; } }
        public override string Aciklama { get { return "1♠-1NT gelişimi (kitap s.45-46)"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            bool duz = OneClub_OneDiamond.Duz(e);
            if (duz && s == 5 && hcp >= 20) return "3NT";
            if (s >= 7) return "4♠";
            if (s >= 6 && c >= 5 && hcp >= 11 && hcp <= 14) return "4♣";
            if (s >= 6 && d >= 5 && hcp >= 11 && hcp <= 14) return "4♦";
            if (s >= 6 && h >= 5 && hcp >= 11 && hcp <= 14) return "4♥";
            if (s >= 6 && (c >= 4 || d >= 4) && hcp >= 15) return "2NT";
            if (s >= 6 && hcp >= 15 && hcp <= 17) return "3♠";
            if (s == 5 && c == 5 && hcp >= 15 && hcp <= 17) return "3♣";
            if (s == 5 && d == 5 && hcp >= 15 && hcp <= 17) return "3♦";
            if (s == 5 && h == 5 && hcp >= 15 && hcp <= 17) return "3♥";
            if (s >= 6 && hcp <= 14) return "2♠";
            if (s >= 5 && h >= 4 && hcp <= 15) return "2♥";
            if (s >= 5 && d == 4 && hcp <= 15) return "2♦";
            return "2♣";                                                                  // 5♠ 3+♣ 11-15 / 5+♠ 16+
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "2♣" || rebid == "2♦" || rebid == "2♥" || rebid == "2♠" || rebid == "2NT";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            switch (rebid)
            {
                case "2♣":    // 5♠ 3+♣ 11-15 / 5+♠ 16+
                    return Sec(new[] {
                        K("3♠", x => hcp >= 9 && hcp <= 11 && x.S >= 3),
                        K("2♦", x => hcp >= 8),                                          // röle
                        K("3♣", x => x.C >= 6),
                        K("3♦", x => x.D >= 6),
                        K("3♥", x => x.H >= 6),
                        K("2NT", x => x.S <= 1 && x.C >= 4 && x.D >= 4),                 // 5/4 minör SNG♠
                        K("2♥", x => x.H >= 4 && x.S <= 1),
                        K("2♠", x => x.S >= 2),
                    }, e, "Pas");
                case "2♦":    // 5♠ 4♦ 11-15
                    return Sec(new[] {
                        K("3♠", x => hcp >= 9 && x.S >= 3),
                        K("3♦", x => hcp >= 9 && x.D >= 4),
                        K("2NT", x => hcp >= 10 && x.Dengeli),
                        K("3♣", x => x.C >= 6),
                        K("2♥", x => x.H >= 5),
                        K("2♠", x => x.S >= 2 && hcp >= 5),
                    }, e, "Pas");
                case "2♥":    // 5♠ 4♥ 11-15
                    return Sec(new[] {
                        K("4♥", x => hcp >= 12 && x.H >= 4),
                        K("3♥", x => hcp >= 9 && x.H >= 4),
                        K("3♠", x => hcp >= 9 && x.S >= 3),
                        K("2NT", x => hcp >= 10 && x.Dengeli),
                        K("3♣", x => x.C >= 6),
                        K("3♦", x => x.D >= 6),
                    }, e, "2♠");
                case "2♠":    // 6♠ 11-15
                    return Sec(new[] {
                        K("4♠", x => hcp >= 12 && x.S >= 2),
                        K("3♠", x => hcp >= 9 && x.S >= 2),
                        K("2NT", x => hcp >= 10 && x.Dengeli),
                        K("3♣", x => x.C >= 6),
                        K("3♦", x => x.D >= 6),
                        K("3♥", x => x.H >= 6),
                    }, e, "Pas");
                case "2NT":   // 6♠ 4m 15+
                    return Sec(new[] {
                        K("3♥", x => x.H >= 6 && hcp <= 7),                              // S.O
                        K("3♣", x => hcp >= 8),                                          // röle
                        K("3♠", x => x.S >= 3),
                    }, e, "3♦");
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            int hcp = e.Hcp;
            if (rebid == "2♣" && devam == "2♦")
            {
                if (hcp <= 15) return "2♠";                                                // 5♠ 3+♣ 11-15 (2NT=SAS ; 3♣=S.O)
                if (OneClub_OneDiamond.Duz(e) && e.S == 5 && hcp <= 17) return "2NT";
                if (e.S == 5 && e.C == 4 && hcp <= 18) return "3♣";
                if (e.S == 5 && e.D == 4 && hcp <= 18) return "3♦";
                if (e.S == 5 && e.H == 5) return "3♥";
                if (e.S >= 6) return "3♠";
                if (e.S == 5 && e.H == 2 && hcp >= 18) return "3NT";
                return "2♥";                                                               // 16+: şekil sorusu
            }
            if (rebid == "2NT" && devam == "3♦")                                           // S.O (pass or correct): karoda 4+ varsa Pas, yoksa majör
                return e.D >= 4 ? "Pas" : "3♠";
            if (rebid == "2NT" && devam == "3♣")
            {
                if (e.S >= 6 && e.C >= 5) return "4♣";
                if (e.S >= 6 && e.D >= 5) return "4♦";
                if (e.D >= 4) return hcp >= 18 ? "3NT" : "3♥";
                return hcp >= 18 ? "3♠" : "3♦";
            }
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (rebid == "2♣" && devam == "2♦")
            {
                if (roleCevap == "2♥") return "2♠";                                        // şekil sorusu
                if (roleCevap == "2♠") return e.Hcp >= 10 ? "2NT" : (e.C >= 4 ? "3♣" : "Pas");   // SAS / S.O
            }
            return null;
        }

        internal override string AciciSon(OcEl e, string rebid, string devam, string roleCevap, string karar)
        {
            if (rebid == "2♣" && devam == "2♦" && roleCevap == "2♥" && karar == "2♠")     // 16+ şekil
            {
                if (e.S >= 6) return "3NT";
                if (e.S == 5 && e.H == 5) return "3♠";                                    // ♠ + ♦ ya da ♣: SEÇİM
                if (e.S == 5 && e.C >= 5) return "3♥";
                if (e.S == 5 && e.D >= 4) return "3♦";
                if (e.S == 5 && e.C >= 4) return "3♣";
                return "2NT";                                                              // 5♠4♥
            }
            return null;
        }
    }
}
