using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoDiamond
{
    /// <summary>2♦-2♠ gelişimi (kitap s.66): açan daima 2NT; cevapçı Pas / 3♣ / 3♦ / 3♥ transfer 3NT</summary>
    public class TwoDiamond_TwoSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♦"; } }
        protected override string Cevap { get { return "2♠"; } }
        public override string Aciklama { get { return "2♦-2♠ gelişimi (kitap s.66): açan daima 2NT; cevapçı Pas / 3♣ / 3♦ / 3♥ transfer 3NT"; } }

 
        internal override string AciciRebid(OcEl e) { return "2NT"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (e.H >= 6 && e.Hcp >= 13)
            {
                if (e.S == 1) return "3♠";
                if (e.C == 1) return "4♣";
                if (e.D == 1) return "4♦";
            }
            if (e.Hcp <= 6) return "Pas";                                                // dengeli zayıf: 2NT'de kal
            if (e.H >= 5 && e.C >= 4) return "3♣";
            if (e.H >= 5 && e.D >= 4) return "3♦";
            if (e.H >= 5) return "3♣";                                                   // 5♥332
            if (e.H == 4 && e.Dengeli && e.Hcp >= 13) return "3♥";                       // 4♥ dengeli SI (transfer 3NT)
            return "3NT";
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            return devam == "3♥" ? "3NT" : null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (devam == "3♥" && roleCevap == "3NT") return e.H == 4 && e.Hcp >= 13 ? "4NT" : null;
            return null;
        }

    }
}
