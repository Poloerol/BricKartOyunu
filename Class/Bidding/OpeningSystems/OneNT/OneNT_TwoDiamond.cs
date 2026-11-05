using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    public class OneNT_TwoDiamond : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1NT"; } }
        protected override string Cevap { get { return "2♦"; } }
        public override string Aciklama { get { return "1NT-2♦ gelişimi (kitap s.55): ♥ transferi"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.H >= 4 && e.Hcp >= 16) return "3♥";                                     // 4/5♥ MAX
            if (e.H == 3 && e.Hcp >= 16) return "2NT";                                    // 3♥ MAX
            return "2♥";                                                                  // transferin kabulü
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "2♥"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            if (hcp <= 7) return "Pas";                                                   // 5+♥ S.O
            if (e.H >= 5 && hcp >= 10 && e.S == 4) return "3♦";                           // 5+♥ 4♠ FG
            if (e.H >= 5 && hcp >= 10 && e.C >= 4) return "2NT";                          // 5+♥ 4+♣ FG
            if (e.H >= 5 && hcp >= 10 && e.D >= 4) return "3♣";                           // 5+♥ 4+♦ FG
            if (hcp <= 9) return e.H >= 6 ? "3♥" : "2♠";                                  // 6+♥ davet / 5♥ davet
            if (e.H == 5 && e.Dengeli) return "3NT";                                      // 5♥ dengeli COG
            return "4♥";
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid != "2♥") return null;
            bool max = e.Hcp >= 16;
            if (devam == "2♠") return max ? (e.H >= 3 ? "4♥" : "3NT") : (e.H >= 3 ? "3♥" : "2NT");   // 5♥ davet
            if (devam == "3♥") return (max || (e.Hcp >= 15 && e.H >= 3)) ? "4♥" : "Pas";            // 6+♥ davet
            return null;
        }
    }
}
