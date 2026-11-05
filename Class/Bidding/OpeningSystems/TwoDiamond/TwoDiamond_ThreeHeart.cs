using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoDiamond
{
    /// <summary>2♦-3♥ gelişimi (kitap s.68): 5/4+ minör SNG ♥</summary>
    public class TwoDiamond_ThreeHeart : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♦"; } }
        protected override string Cevap { get { return "3♥"; } }
        public override string Aciklama { get { return "2♦-3♥ gelişimi (kitap s.68): 5/4+ minör SNG ♥"; } }

 
        internal override string AciciRebid(OcEl e)
        {
            bool iyi = e.Hcp >= 19;
            if (iyi && (e.C >= 4 || e.D >= 4)) return e.C >= e.D ? "4♣" : "4♦";           // SI
            if (iyi && e.C == 3 && e.D == 3) return "4♥";                                 // iyi el 3-3 minör
            if (e.S == 4) return "3♠";                                                    // 4-3 oynama arzusu
            return "3NT";
        }

        internal override bool DevamTanimli(string rebid) { return false; }
        internal override string CevapciDevam(OcEl e, string rebid) { return "Pas"; }

    }
}
