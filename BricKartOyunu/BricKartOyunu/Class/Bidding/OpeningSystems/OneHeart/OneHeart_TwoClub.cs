using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneHeart
{
    public class OneHeart_TwoClub : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♥"; } }
        protected override string Cevap { get { return "2♣"; } }
        public override string Aciklama { get { return "1♥-2♣ gelişimi (kitap s.40-41): FG röle"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp;
            if (e.H >= 5 && e.S == 4) return "2♥";                                        // 5+♥ 4♠ 11+
            if (hcp >= 16 && e.C >= 4) return "2♠";                                       // 5+♥ 4+♣ 16+
            if (hcp >= 15) return "2NT";                                                  // 5+♥ 15+
            return "2♦";                                                                  // 11-15 min
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "2♦" || rebid == "2♥" || rebid == "2♠" || rebid == "2NT";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            switch (rebid)
            {
                case "2♦":
                    return Sec(new[] {
                        K("3♠", x => hcp >= 14 && x.C >= 5 && x.H >= 3 && x.S <= 1),    // SNG♠ SI
                        K("4♦", x => hcp >= 14 && x.C >= 5 && x.H >= 3 && x.D <= 1),    // SNG♦ SI
                        K("3♣", x => hcp >= 9 && hcp <= 11 && x.C >= 6),                // davet
                        K("3NT", x => hcp <= 15 && x.Dengeli && x.H <= 2),
                        K("4♥", x => hcp <= 15 && x.H >= 4 && hcp >= 12 && x.Singleton),
                    }, e, "2♥");                                                          // röle
                case "2♥":
                    return Sec(new[] {
                        K("3♣", x => hcp >= 9 && hcp <= 11 && x.C >= 6),
                        K("2NT", x => x.S >= 4),                                         // SAS w 4♠
                    }, e, "2♠");                                                          // SAS
                case "2♠":
                case "2NT":
                    return rebid == "2♠" ? "2NT" : Sec(new[] {
                        K("3♥", x => hcp >= 14),                                         // tutuş SI
                        K("3♦", x => x.C >= 6),                                          // 6+♣ FG
                    }, e, "3♣");                                                          // röle
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            int hcp = e.Hcp;
            bool k4441 = false;
            if (rebid == "2♦" && devam == "2♥")                                            // röle
            {
                if (e.Singleton && e.H == 5 && e.D == 4 && e.S == 3 && e.C == 1 && hcp >= 14) return "3♠";
                if (e.H == 5 && e.D == 4 && e.S == 2 && e.C == 2 && hcp >= 14) return "3♥";
                if (e.H == 5 && e.D == 4 && e.S == 1 && e.C == 3 && hcp >= 14) return "3NT";
                if (e.H == 5 && e.D == 4 && hcp <= 13) return "3♣";
                if ((e.H >= 6 && e.D >= 4) || (e.H >= 5 && e.D >= 5)) return "3♦";
                if (e.H >= 5 && e.C >= 4) return "2♠";
                return "2NT";
            }
            if (rebid == "2♥" && devam == "2♠")                                            // SAS
            {
                if (e.H >= 6 && e.S >= 5) return "2NT";
                if (e.H >= 6 && e.S == 4) return "3♦";
                if (e.H == 5 && e.S == 4 && e.D == 2 && e.C == 2 && hcp >= 16) return "3♥";
                if (e.H == 5 && e.S == 4 && e.D == 3 && e.C == 1 && hcp >= 16) return "3♠";
                if (e.H == 5 && e.S == 4 && e.C == 3 && e.D == 1) return hcp >= 19 ? "4♣" : "3NT";
                if (e.H == 5 && e.S == 4 && hcp <= 15) return "3♣";
                return "2NT";
            }
            if (rebid == "2♥" && devam == "2NT")                                           // SAS w 4♠
            {
                if (e.H >= 6) return "3♥";
                if (e.D >= 4 && e.S == 4) return "3♠";
                if (e.C >= 4 && e.S == 4) return hcp >= 19 ? "4♣" : "3NT";
                return hcp >= 16 ? "3♣" : "3♦";
            }
            if (rebid == "2♠" && devam == "2NT")                                           // SAS (4+♣ 16+)
            {
                if (e.D >= 4 && e.C >= 4) return hcp >= 19 ? "4♣" : "3NT";
                if (e.C >= 5 && e.D <= 1) return "3♥";
                if (e.C >= 5 && e.S <= 1) return "3♠";
                if (e.H >= 6) return "3♦";
                return "3♣";
            }
            if (rebid == "2NT" && devam == "3♣")                                           // röle cevapları
            {
                if (e.H >= 6 && e.Singleton) return "3♦";
                if (e.H >= 6) return "3♥";
                if (OneClub_OneDiamond.Duz(e)) return hcp >= 18 ? "3♠" : "3NT";
                return "3♣";
            }
            return null;
        }
    }
}
