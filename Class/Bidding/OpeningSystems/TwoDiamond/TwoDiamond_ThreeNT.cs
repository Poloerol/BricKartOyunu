using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoDiamond
{
    /// <summary>2♦-3NT gelişimi (kitap s.68): 5+♠/5+♥ FG; açan 4♣/4♦ SI, 4♥/4♠ MIN</summary>
    public class TwoDiamond_ThreeNT : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♦"; } }
        protected override string Cevap { get { return "3NT"; } }
        public override string Aciklama { get { return "2♦-3NT gelişimi (kitap s.68): 5+♠/5+♥ FG; açan 4♣/4♦ SI, 4♥/4♠ MIN"; } }

 
        internal override string AciciRebid(OcEl e)
        {
            bool max = e.Hcp >= 19;
            if (e.H >= 3 && e.H >= e.S) return max ? "4♣" : "4♥";
            if (e.S >= 3) return max ? "4♦" : "4♠";
            return "4♥";
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "4♣" || rebid == "4♦"; }
        internal override string CevapciDevam(OcEl e, string rebid) { return rebid == "4♣" ? "4♦" : "4♥"; }   // transferi al

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid == "4♣" && devam == "4♦") return "4♥";
            if (rebid == "4♦" && devam == "4♥") return "4♠";
            return null;
        }

    }
}
