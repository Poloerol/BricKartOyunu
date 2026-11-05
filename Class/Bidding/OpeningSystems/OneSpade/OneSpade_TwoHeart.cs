using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneSpade
{
    public class OneSpade_TwoHeart : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♠"; } }
        protected override string Cevap { get { return "2♥"; } }
        public override string Aciklama { get { return "1♠-2♥ gelişimi (kitap s.49): 5+♥ FG / 6+♥ 8-11"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            if (s >= 5 && h >= 5)
            {
                if (hcp >= 14 && c <= 1) return "4♣";                                     // SNG♣
                if (hcp >= 14 && d <= 1) return "4♦";                                     // SNG♦
                return "4♥";
            }
            if (h >= 4) return s >= 6 ? "3♦" : "3♣";
            if (h == 3)
            {
                if (s >= 6) return "3♥";
                if (OneClub_OneDiamond.Duz(e) && hcp >= 16 && hcp <= 17) return "3NT";
                return hcp <= 13 ? "2♠" : "3♠";                                          // 14-15 / 18+
            }
            if (c >= 4 || d >= 4) return "2♠";
            return "2NT";
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "2♠" || rebid == "2NT" || rebid == "3♣" || rebid == "3♦" || rebid == "3♥";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            switch (rebid)
            {
                case "2♠":   // 5♠ 4m 11+
                    return Sec(new[] {
                        K("3♥", x => x.H >= 6 && hcp <= 11),
                        K("3♣", x => x.H >= 5 && x.C >= 4 && hcp >= 12),
                        K("3♦", x => x.H >= 5 && x.D >= 4 && hcp >= 12),
                    }, e, "2NT");                                                         // röle
                case "2NT":  // 5+♠ w/o 3♥
                    return Sec(new[] {
                        K("3♥", x => x.H >= 6 && hcp <= 11),
                    }, e, "3♣");
                case "3♣":   // 5♠ 4♥
                    return "3♦";                                                          // SAS
                case "3♦":   // 6♠ 4♥
                    return "3♥";                                                          // röle
                case "3♥":   // 6♠ 3♥
                    return "3♠";                                                          // röle (Min ; DNG ; KK ; BK)
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            int hcp = e.Hcp;
            if (rebid == "2♠" && devam == "2NT")
            {
                if (OneClub_OneDiamond.Duz(e) && e.H == 3 && hcp <= 13) return "3NT";
                if (e.H == 3) return hcp <= 14 ? "3♥" : "3♠";
                if (e.C >= 4) return "3♣";
                if (e.D >= 4) return "3♦";
                return "3♣";
            }
            if (rebid == "3♥" && devam == "3♠")                                           // Min ; DNG ; KK ; BK
            {
                if (e.C <= 1) return "4♦";
                if (e.D <= 1) return "4♥";
                return hcp >= 13 ? "4♣" : "3NT";
            }
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (rebid == "2♠" && devam == "2NT")
            {
                if (roleCevap == "3♥") return "3♠";                                        // röle: KK ; BK
                if (roleCevap == "3♠") return "3NT";
            }
            return null;
        }

        internal override string AciciSon(OcEl e, string rebid, string devam, string roleCevap, string karar)
        {
            if (rebid == "2♠" && devam == "2NT" && ((roleCevap == "3♥" && karar == "3♠") || (roleCevap == "3♠" && karar == "3NT")))
                return e.C <= 1 ? "4♣" : "4♦";                                             // KK ; BK
            return null;
        }
    }
}
