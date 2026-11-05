using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>1♦ ⇒ 1NT GELİŞİMİ (SAS kitabı s.31): cevapçı 5-10 HCP.</summary>
    public class OneDiamond_OneNT : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♦"; } }
        protected override string Cevap { get { return "1NT"; } }
        public override string Aciklama { get { return "1♦-1NT gelişimi (kitap s.31)"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, h = e.H, s = e.S, d = e.D, c = e.C;
            if (hcp >= 18 && d >= 6) return "2NT";
            if (hcp >= 17 && d >= 5 && h >= 4) return "2♥";
            if (hcp >= 17 && d >= 5 && s >= 4) return "2♠";
            if (hcp >= 16) return "2♣";                                                   // röle
            if (hcp >= 14 && d == 5 && c == 5) return "3♣";
            if (hcp >= 14 && d >= 6 && h >= 5) return "3♥";
            if (hcp >= 14 && d >= 6 && s >= 5) return "3♠";
            if (hcp >= 14 && d >= 6) return "3♦";
            if (d >= 6) return "2♦";
            return "Pas";                                                                  // 1NT'de kalır
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "2♣" || rebid == "2♥" || rebid == "2♠" || rebid == "2NT";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            switch (rebid)
            {
                case "2♣":   // 16+ röle: 2♦ min, 2♥ 5♣, 2♠ 4♦3-3-3 max, 2NT 4♣3-3-3 max, 3♣ 4♣4♦ max
                    return Sec(new[] {
                        K("3♣", x => hcp >= 8 && x.C >= 4 && x.D >= 4),
                        K("2NT", x => hcp >= 8 && x.C == 4 && x.D == 3 && x.H == 3 && x.S == 3),
                        K("2♠", x => hcp >= 8 && x.D == 4 && x.C == 3 && x.H == 3 && x.S == 3),
                        K("2♥", x => x.C >= 5),
                    }, e, "2♦");
                case "2♥":   // 5+♦ 4♥ 17+
                case "2♠":   // 5+♦ 4♠ 17+
                    return Sec(new[] {
                        K("2NT", x => hcp <= 6),                                          // Lebensohl
                        K("3NT", x => hcp >= 9),                                          // max
                    }, e, rebid == "2♥" ? "2♠" : "3♣");                                    // değerler
                case "2NT":  // 6+♦ 18+
                    return Sec(new[] {
                        K("3♥", x => hcp >= 7 && x.D >= 3 && x.H <= 1),
                        K("3♠", x => hcp >= 7 && x.D >= 3 && x.S <= 1),
                        K("3♣", x => hcp >= 9),                                           // röle FG
                        K("3NT", x => hcp >= 6 && x.Dengeli),
                    }, e, "3♦");                                                           // min
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (devam == "2NT" && (rebid == "2♥" || rebid == "2♠")) return "3♣";           // Lebensohl
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (devam == "2NT" && roleCevap == "3♣" && (rebid == "2♥" || rebid == "2♠")) return "Pas";
            return null;
        }
    }
}
