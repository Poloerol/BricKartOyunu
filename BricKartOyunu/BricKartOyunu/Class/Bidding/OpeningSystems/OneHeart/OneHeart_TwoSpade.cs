using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneHeart
{
    public class OneHeart_TwoSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♥"; } }
        protected override string Cevap { get { return "2♠"; } }
        public override string Aciklama { get { return "1♥-2♠ gelişimi (kitap s.43): 5/5 minör 7-11"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.H >= 6 && e.Hcp >= 15) return "3♥";                                     // iyi el davet
            if (e.Hcp >= 15) return "2NT";                                                // röle
            return e.C >= e.D ? "3♣" : "3♦";                                              // tercih
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "2NT"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (e.C + e.D >= 11 && e.H <= 1 && e.S <= 1) return "3♦";                     // 6/5 minör 1-1
            if (e.H <= 1) return "3♥";                                                    // SNG♥
            if (e.S <= 1) return "3♠";                                                    // SNG♠
            return "3♣";                                                                  // min
        }
    }
}
