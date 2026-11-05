using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoDiamond
{
    /// <summary>2♦-3♦ gelişimi (kitap s.68): 5+♠ 4♥ FG; açan 3♥/3♠ MAX, 4♥/4♠ MIN, 3NT</summary>
    public class TwoDiamond_ThreeDiamond : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♦"; } }
        protected override string Cevap { get { return "3♦"; } }
        public override string Aciklama { get { return "2♦-3♦ gelişimi (kitap s.68): 5+♠ 4♥ FG; açan 3♥/3♠ MAX, 4♥/4♠ MIN, 3NT"; } }

 
        internal override string AciciRebid(OcEl e)
        {
            bool max = e.Hcp >= 19;
            if (e.S >= 3) return max ? "3♠" : "4♠";
            if (e.H >= 4) return max ? "3♥" : "4♥";
            return "3NT";
        }

        internal override bool DevamTanimli(string rebid) { return false; }
        internal override string CevapciDevam(OcEl e, string rebid) { return "Pas"; }

    }
}
