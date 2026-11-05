using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoDiamond
{
    /// <summary>2♦-2NT gelişimi (kitap s.66): transfer 3♣; cevapçı Pas / 3♦ (♥) / 3♥ (♠) / 3♠ / 3NT / 4m</summary>
    public class TwoDiamond_TwoNT : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♦"; } }
        protected override string Cevap { get { return "2NT"; } }
        public override string Aciklama { get { return "2♦-2NT gelişimi (kitap s.66): transfer 3♣; cevapçı Pas / 3♦ (♥) / 3♥ (♠) / 3♠ / 3NT / 4m"; } }

 
        internal override string AciciRebid(OcEl e) { return "3♣"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (e.Hcp <= 6)
            {
                if (e.C >= 5) return "Pas";                                              // 5+♣ SO
                if (e.H >= 5) return "3♦";                                               // transfer 3♥ SO
                return "Pas";
            }
            if (e.H >= 6) return "3♦";                                                   // transfer 3♥ SI
            if (e.S >= 6) return "3♥";                                                   // 6+♠ SI
            if (e.C >= 6 || e.D >= 6 || (e.C >= 5 && e.D >= 5)) return "3♠";            // transfer 3NT: minör SI
            if (e.C == 5 && e.Dengeli) return "4♣";
            if (e.D == 5 && e.Dengeli) return "4♦";
            if (e.H == 1 && (e.C >= 5 || e.D >= 5)) return "4♥";
            if (e.S == 1 && (e.C >= 5 || e.D >= 5)) return "4♠";
            return "3NT";
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (devam == "3♦") return "3♥";
            if (devam == "3♠") return "3NT";
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (devam == "3♦" && roleCevap == "3♥") return e.Hcp <= 6 ? "Pas" : "4NT";   // 6+♥ SI: RKCB
            return null;
        }

        internal override bool KararTanimli(string rebid, string devam) { return devam == "3♦"; }

    }
}
