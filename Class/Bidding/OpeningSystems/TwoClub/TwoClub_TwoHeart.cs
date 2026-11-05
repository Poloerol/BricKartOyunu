using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoClub
{
    /// <summary>2♣-2♥ (5+♠ 7+) gelişimi (kitap s.61): 2♠ 3+♠ tutuş; 2NT 23+ dengeli → 3♣/3♦/3♥/3♠/3NT; 3m/3♥ doğal OG; 3♠ 4+♠ SI.</summary>
    public class TwoClub_TwoHeart : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♣"; } }
        protected override string Cevap { get { return "2♥"; } }
        public override string Aciklama { get { return "2♣-2♥ gelişimi (kitap s.61)"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.S >= 4 && e.Hcp >= 26) return "3♠";
            if (e.S >= 3) return "2♠";
            if (e.Dengeli) return "2NT";
            if (e.C >= 5) return "3♣";
            if (e.D >= 5) return "3♦";
            if (e.H >= 5) return "3♥";
            return "2NT";
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "2♠" || TcDevam.Tanimli(rebid); }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "2♠") return "4♠";
            if (rebid != "2NT") return TcDevam.Dogal(e, rebid);
            if (e.S >= 6) return "3♥";
            if (e.S >= 5)
            {
                if (e.D >= 4) return "3♣";
                if (e.H >= 4) return "3♦";
                if (e.C >= 4) return "3♠";
            }
            return "3NT";                                                                 // 5♠ 3-3-2, 7-8 HCP
        }
    }
}
