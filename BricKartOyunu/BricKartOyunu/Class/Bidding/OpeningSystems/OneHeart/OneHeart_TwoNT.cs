using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneHeart
{
    public class OneHeart_TwoNT : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♥"; } }
        protected override string Cevap { get { return "2NT"; } }
        public override string Aciklama { get { return "1♥-2NT gelişimi (kitap s.43): 3♥ davet, 4♠ yok"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp;
            if (hcp <= 12) return "3♥";                                                   // oynamak için
            if (hcp <= 14) return "3♦";                                                   // davet
            if (e.C == 1) return "3NT";                                                   // SNG♣ SI
            if (e.D == 1) return "4♣";                                                    // SNG♦ SI
            if (e.S == 1) return "4♦";                                                    // SNG♠ SI
            if (e.S >= 4) return "3♠";                                                    // 5+♥ 4♠ SI
            return "3♣";                                                                  // röle SI
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "3♣" || rebid == "3♦"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3♦") return e.Hcp >= 11 ? "4♥" : "3♥";                           // max / min
            if (e.C <= 1) return "3♥";                                                    // SNG♣
            if (e.D <= 1) return "3♠";                                                    // SNG♦
            if (e.S <= 1) return "3NT";                                                   // SNG♠
            if (e.C >= 5) return "4♣";
            if (e.D >= 5) return "4♦";
            return e.Hcp >= 11 ? "3♦" : "4♥";                                             // max / min
        }
    }
}
