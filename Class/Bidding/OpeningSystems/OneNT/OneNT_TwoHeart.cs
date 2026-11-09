using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    public class OneNT_TwoHeart : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1NT"; } }
        protected override string Cevap { get { return "2♥"; } }
        public override string Aciklama { get { return "1NT-2♥ gelişimi (kitap s.56): ♠ transferi"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.S >= 4 && e.Hcp >= 16) return "3♠";                                     // 4/5♠ MAX
            if (e.S == 3 && e.Hcp >= 16) return "2NT";                                    // 3♠ MAX
            return "2♠";                                                                  // transferin kabulü
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "2♠" || rebid == "3♠" || rebid == "2NT"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            // Süper kabul (3♠ = 4♠ MAX) ve 3♠ MAX (2NT): cevapçı oyuna kadar devam eder
            if (rebid == "3♠") return hcp >= 8 ? "4♠" : "Pas";
            if (rebid == "2NT")
            {
                if (hcp <= 7) return "3♠";
                if (hcp <= 9) return "4♠";
                return (e.S == 5 && e.Dengeli) ? "3NT" : "4♠";
            }
            if (hcp <= 7) return "Pas";                                                   // 5+♠ S.O
            if (e.S >= 5 && hcp >= 10 && e.H >= 5) return "3♥";                           // 5+♠ 5+♥ FG
            if (e.S >= 5 && hcp >= 10 && e.H == 4) return "3♦";                           // 5+♠ 4♥ FG
            if (e.S >= 5 && hcp >= 10 && e.C >= 4) return "2NT";                          // 5+♠ 4+♣ FG
            if (e.S >= 5 && hcp >= 10 && e.D >= 4) return "3♣";                           // 5+♠ 4+♦ FG
            if (hcp <= 9) return e.S >= 6 ? "3♠" : "Pas";                                 // 6+♠ davet (5♠ davet Stayman ile gelir)
            if (e.S == 5 && e.Dengeli) return "3NT";                                      // 5♠ dengeli COG
            return "4♠";
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid != "2♠") return null;
            if (devam == "3♠") return (e.Hcp >= 16 || (e.Hcp >= 15 && e.S >= 3)) ? "4♠" : "Pas";   // 6+♠ davet
            return null;
        }
    }
}
