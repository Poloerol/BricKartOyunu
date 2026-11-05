using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>1♦ ⇒ 2♣ GELİŞİMİ (SAS kitabı s.32): 12+ dengeli 2+♣ ya da 4+♦ FG.</summary>
    public class OneDiamond_TwoClub : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♦"; } }
        protected override string Cevap { get { return "2♣"; } }
        public override string Aciklama { get { return "1♦-2♣ gelişimi (kitap s.32)"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, h = e.H, s = e.S, d = e.D, c = e.C;
            bool k4441 = d == 4 && h == 4 && s == 4 && c == 1;
            if (hcp >= 18 && (d >= 6 || k4441)) return "2NT";
            if (k4441 && hcp >= 15) return "3NT";
            if (hcp >= 15 && hcp <= 17 && d >= 6)
            {
                if (e.Singleton) return "3♣";                                              // 6+♦ SNG
                if (c == 3 && e.El.Any(k => k.Suit == "Kupa" && k.Value >= 12)) return "3♥";
                if (c == 3 && e.El.Any(k => k.Suit == "Maça" && k.Value >= 12)) return "3♠";
                return "3♦";
            }
            if (d >= 5 && h >= 4) return "2♥";
            if (d >= 5 && s >= 4) return "2♠";
            return "2♦";                                                                   // min / 5♦4♣
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "2♦" || rebid == "2♥" || rebid == "2♠" || rebid == "2NT" || rebid == "3♣";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            switch (rebid)
            {
                case "2♦":
                    return Sec(new[] {
                        K("3♣", x => x.C >= 6 && x.D <= 1),
                        K("3♥", x => x.C >= 5 && x.S >= 4 && x.H <= 1),
                        K("3♠", x => x.C >= 5 && x.H >= 4 && x.S <= 1),
                        K("3♦", x => x.C >= 5 && (x.H >= 4 || x.S >= 4) && x.D <= 1),
                        K("2NT", x => x.C >= 4 && x.C <= 5 && x.Dengeli && hcp <= 14),
                        K("2♥", x => hcp >= 16),                                           // röle SI
                    }, e, "2♠");                                                           // röle FG
                case "2♥":   // 5+♦ 4♥ 11+ : SAS
                    return Sec(new[] { K("2NT", x => x.H >= 4) }, e, "2♠");
                case "2♠":   // 5+♦ 4♠ 11+ : SAS
                    return "2NT";
                case "2NT":  // 18+ 6+♦ / 4441
                    return "3♣";                                                           // röle
                case "3♣":   // 6+♦ SNG 15-17
                    return "3♦";                                                           // röle
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            bool k4441 = e.D == 4 && e.H == 4 && e.S == 4 && e.C == 1;
            if (rebid == "2♦" && devam == "2♥")     // röle SI
            {
                if (e.D >= 5 && e.C >= 4) return "2♠";
                if (k4441) return "3NT";
                if (OneClub.OneClub_OneDiamond.Duz(e) && e.Hcp >= 12 && e.Hcp <= 14) return "2NT";
                if (e.D >= 6 && e.Singleton) return "3♣";
                if (e.D >= 6 && e.C == 3 && e.El.Any(k => k.Suit == "Kupa" && k.Value >= 12)) return "3♥";
                if (e.D >= 6 && e.C == 3 && e.El.Any(k => k.Suit == "Maça" && k.Value >= 12)) return "3♠";
                return "3♦";
            }
            if (rebid == "2♦" && devam == "2♠")     // röle FG
            {
                if (e.D == 5 && e.C == 4 && e.H == 3 && e.S == 1) return e.Hcp >= 18 ? "3NT" : "3♠";
                if (e.D == 5 && e.C == 4 && e.S == 3 && e.H == 1 && e.Hcp >= 15) return "3♥";
                if (e.D == 5 && e.C == 4 && e.S == 2 && e.H == 2 && e.Hcp >= 15) return "3♦";
                if ((e.D >= 6 || k4441) && e.Hcp <= 14) return "3♣";
                return "2NT";
            }
            if (rebid == "2♥" && devam == "2♠") return "2NT";                               // SAS ilk basamak (SEÇİM)
            if (rebid == "2NT" && devam == "3♣")    // 3♦ 4441; 3♥ SNG; 3♠ dengesiz
            {
                if (k4441) return "3♦";
                return e.Singleton ? "3♥" : "3♠";
            }
            if (rebid == "3♣" && devam == "3♦")     // KK / OK / BK
            {
                if (e.C <= 1) return "3♥";
                if (e.H <= 1) return "3♠";
                return "3NT";
            }
            return null;
        }
    }
}
