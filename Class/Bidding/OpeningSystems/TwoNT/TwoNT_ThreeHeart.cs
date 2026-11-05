using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoNT
{
    /// <summary>2NT-3♥ gelişimi (kitap s.73): transfer ♠; açan 3♠ (3+♠) / 3NT (2♠) / 4♥,4m</summary>
    public class TwoNT_ThreeHeart : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2NT"; } }
        protected override string Cevap { get { return "3♥"; } }
        public override string Aciklama { get { return "2NT-3♥ gelişimi (kitap s.73): transfer ♠; açan 3♠ (3+♠) / 3NT (2♠) / 4♥,4m"; } }

 
        internal override string AciciRebid(OcEl e)
        {
            if (e.S >= 4 && e.Hcp >= 22) return "4♥";
            if (e.S >= 3) return "3♠";
            if (e.S == 2) return "3NT";
            return "3♠";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3NT")
            {
                if (e.Hcp >= 13 && e.C >= 4 && e.S >= 5) return "4♣";
                if (e.Hcp >= 13 && e.D >= 4 && e.S >= 5) return "4♦";
                return e.S >= 5 && e.Hcp >= 4 ? "4♠" : "Pas";
            }
            if (e.Hcp <= 3) return "Pas";
            if (e.Hcp >= 13)
            {
                if (e.S >= 5 && e.C >= 4) return "4♣";
                if (e.S >= 5 && e.D >= 4) return "4♦";
                if (e.S >= 5 && e.H >= 5) return "4♥";
                return e.S >= 6 ? "4NT" : "3NT";
            }
            return e.S >= 5 ? "4♠" : "3NT";
        }

    }
}
