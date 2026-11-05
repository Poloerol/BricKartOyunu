using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneHeart
{
    public class OneHeart_TwoDiamond : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♥"; } }
        protected override string Cevap { get { return "2♦"; } }
        public override string Aciklama { get { return "1♥-2♦ gelişimi (kitap s.42): 5+♦ FG / 6+♦ davet"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            if (h >= 5 && d >= 5) return hcp >= 14 ? "4♣" : "4♦";
            if (d >= 4) return h >= 6 ? "3♦" : "3♣";
            if (d == 3)
            {
                if (h >= 6) return "3♥";
                if (OneClub_OneDiamond.Duz(e) && hcp >= 15 && hcp <= 17) return "3NT";
                return "3♠";                                                              // 5♥ 3♦ 3-2 11-14 / 18+
            }
            if (s == 4) return "2♠";
            if (c >= 4) return "2♥";
            return "2NT";
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "2♥" || rebid == "2♠" || rebid == "2NT" || rebid == "3♣" || rebid == "3♦" || rebid == "3♥";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            switch (rebid)
            {
                case "2♥":   // 5+♥ 4+♣
                    return Sec(new[] {
                        K("3♥", x => x.D >= 5 && x.H >= 3 && hcp >= 14),
                        K("3♦", x => x.D >= 6 && hcp <= 11),
                        K("2NT", x => x.C >= 4),
                    }, e, "2♠");
                case "2♠":   // 5+♥ 4♠
                    return Sec(new[] {
                        K("3♠", x => x.D >= 5 && x.S >= 4 && hcp >= 14),
                        K("3♥", x => x.D >= 5 && x.H >= 3 && hcp >= 14),
                        K("3♦", x => x.D >= 6 && hcp <= 11),
                    }, e, "2NT");
                case "2NT":  // 5+♥ w/o 3♦
                    return Sec(new[] {
                        K("3♥", x => x.D >= 5 && x.H >= 3 && hcp >= 14),
                        K("3♦", x => x.D >= 6 && hcp <= 11),
                    }, e, "3♣");
                case "3♣":   // 5♥ 4♦
                    return Sec(new[] {
                        K("3♥", x => x.D >= 5 && x.H >= 3 && hcp >= 14),
                    }, e, "3♦");
                case "3♦":   // 6♥ 4♦
                    return "3♥";                                                          // röle
                case "3♥":   // 6♥ 3♦
                    return Sec(new[] {
                        K("3NT", x => x.Dengeli && hcp <= 14),
                    }, e, "3♠");                                                          // röle (DNG ; KK ; BK)
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid == "3♥" && devam == "3♠")                                           // DNG ; KK ; BK
            {
                if (e.C <= 1) return "4♣";
                if (e.S <= 1) return "4♦";
                return "3NT";
            }
            return null;
        }
    }
}
