using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoDiamond
{
    /// <summary>2♦-2♥ gelişimi (kitap s.65): açan 2♠ = 2/3♠, 2NT = 4♠</summary>
    public class TwoDiamond_TwoHeart : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♦"; } }
        protected override string Cevap { get { return "2♥"; } }
        public override string Aciklama { get { return "2♦-2♥ gelişimi (kitap s.65): açan 2♠ = 2/3♠, 2NT = 4♠"; } }

 
        internal override string AciciRebid(OcEl e) { return e.S >= 4 ? "2NT" : "2♠"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            bool sinirli = e.Hcp <= 6;
            if (rebid == "2♠")                                                           // açanın 2-3 kozu var
            {
                if (e.S >= 5 && sinirli) return "Pas";
                if (e.S == 4 && e.H == 4) return "3♥";
                if (e.S >= 6 && e.Hcp >= 13)
                {
                    if (e.C == 1) return "4♣";
                    if (e.D == 1) return "4♦";
                    if (e.H == 1) return "4♥";
                }
                if (e.S >= 5 && e.C >= 4) return "3♣";
                if (e.S >= 5 && e.D >= 4) return "3♦";
                if (e.S >= 5) return "3♣";                                               // 5♠332
                if (e.Hcp >= 13) return "2NT";                                           // 4♠ dengeli SI
                return "3♠";                                                             // 4♠ dengeli: transfer 3NT
            }
            // açan 4♠ gösterdi (2NT)
            if (e.S >= 5 && sinirli) return "3♥";                                        // transfer 3♠ SO
            if (e.Hcp >= 13)
            {
                if (e.C == 1) return "4♣";
                if (e.D == 1) return "4♦";
                if (e.H == 1) return "4♥";
            }
            if (e.C >= 4) return "3♣";
            if (e.D >= 4) return "3♦";
            if (e.S == 4 && e.Dengeli && e.Hcp <= 12) return "3NT";
            return "3♥";                                                                 // transfer 3♠ (FG)
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid == "2NT" && devam == "3♥") return "3♠";                            // transfer kabulü
            if (rebid == "2♠" && devam == "3♠") return "3NT";                            // 4♠ dengeli oyun
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (rebid == "2NT" && devam == "3♥" && roleCevap == "3♠")
                return e.Hcp <= 6 ? "Pas" : (e.Hcp >= 13 ? "4NT" : "4♠");
            return null;
        }

        internal override bool KararTanimli(string rebid, string devam) { return rebid == "2NT" && devam == "3♥"; }

    }
}
