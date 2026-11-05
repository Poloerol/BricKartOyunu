using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneSpade
{
    public class OneSpade_TwoClub : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♠"; } }
        protected override string Cevap { get { return "2♣"; } }
        public override string Aciklama { get { return "1♠-2♣ gelişimi (kitap s.47): FG röle"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            if (s >= 5 && d >= 4 && hcp >= 16)
            {
                if (s + d >= 10) return "3♦";                                              // 10 kart ♠ ve ♦
                if (s == 5 && d == 4 && hcp >= 18)
                {
                    if (h == 2 && c == 2) return "3♥";
                    if (h == 3 && c == 1) return "3♠";
                    if (c == 3 && h == 1) return hcp >= 21 ? "4♣" : "3NT";
                }
                if (hcp <= 18) return "3♣";
            }
            if (s >= 5 && h >= 4) return "2♥";
            if (hcp >= 16 && s >= 5 && c >= 4) return "2♠";
            if (hcp >= 15) return "2NT";
            return "2♦";                                                                   // 11-15 min
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "2♦" || rebid == "2♥" || rebid == "2♠" || rebid == "2NT" || rebid == "3♣" || rebid == "3♦";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            switch (rebid)
            {
                case "2♦":
                    return Sec(new[] {
                        K("3♣", x => hcp >= 9 && hcp <= 11 && x.C >= 6),
                        K("4♠", x => x.S >= 3 && hcp <= 14),
                        K("3NT", x => x.S <= 2 && hcp <= 15 && x.Dengeli),
                    }, e, "2♥");                                                          // röle SI
                case "2♥":
                    return Sec(new[] {
                        K("2NT", x => x.H >= 4),                                         // SAS w 4♥
                    }, e, "2♠");                                                          // SAS SI
                case "2♠":
                    return "2NT";                                                         // SAS
                case "2NT":
                    return Sec(new[] {
                        K("3♦", x => x.C >= 6),                                          // 6+♣ SI
                    }, e, "3♣");                                                          // röle
                case "3♣":
                    return "3♦";                                                          // röle
                case "3♦":
                    return "3♥";                                                          // röle
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            int hcp = e.Hcp;
            if (rebid == "2♦" && devam == "2♥")                                            // röle
            {
                if (e.S >= 5 && e.C >= 4 && hcp <= 13) return "2♠";
                if (e.S + e.D >= 10) return "3♦";
                if (e.S == 5 && e.D == 4 && hcp >= 14)
                {
                    if (e.H == 2 && e.C == 2) return "3♥";
                    if (e.H == 3 && e.C == 1) return "3♠";
                    if (e.C == 3 && e.H == 1) return "3NT";
                }
                if (e.S == 5 && e.D == 4) return "3♣";
                return "2NT";
            }
            if (rebid == "2NT" && devam == "3♣")                                           // 1NT ⇒ 2NT ile aynı
            {
                if (e.S >= 6 && e.C >= 5) return "4♣";
                if (e.S >= 6 && e.D >= 5) return "4♦";
                if (e.D >= 4) return hcp >= 18 ? "3NT" : "3♥";
                return hcp >= 18 ? "3♠" : "3♦";
            }
            if (rebid == "3♦" && devam == "3♥") return e.S >= 6 ? "3♠" : "3NT";            // 6-4 min ; 5-5 min (SEÇİM)
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (rebid == "2♦" && devam == "2♥" && roleCevap == "2♠") return "2NT";        // SAS
            return null;
        }
    }
}
