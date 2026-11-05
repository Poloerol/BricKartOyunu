using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoClub
{
    /// <summary>2♣-3♠ (5♣332 / 5♦332 / 5-4 minör) gelişimi (kitap s.63): 3NT röle → 4♣ / 4♦ / 4♥ / 4♠ / 4NT.</summary>
    public class TwoClub_ThreeSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♣"; } }
        protected override string Cevap { get { return "3♠"; } }
        public override string Aciklama { get { return "2♣-3♠ gelişimi (kitap s.63)"; } }

        internal override string AciciRebid(OcEl e) { return "3NT"; }                     // röle

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (e.C >= 5) return "4♣";                                                    // 5♣ 3-3-2 veya 5♣ 4♦
            if (e.D >= 5 && e.C == 4)
            {
                if (e.H == 1) return "4♥";
                if (e.S == 1) return "4♠";
                return "4NT";                                                             // 2-2 (8-9)
            }
            return "4♦";                                                                  // 5♦ 3-3-2
        }
    }
}
