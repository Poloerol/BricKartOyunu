using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoDiamond
{
    /// <summary>2♦-3♣ gelişimi (kitap s.67): 4'lü majör sorusu; açan 3♦ yok / 3♥ 4♥ / 3♠ 4♠</summary>
    public class TwoDiamond_ThreeClub : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♦"; } }
        protected override string Cevap { get { return "3♣"; } }
        public override string Aciklama { get { return "2♦-3♣ gelişimi (kitap s.67): 4'lü majör sorusu; açan 3♦ yok / 3♥ 4♥ / 3♠ 4♠"; } }

 
        internal override string AciciRebid(OcEl e)
        {
            if (e.H >= 4) return "3♥";
            if (e.S >= 4) return "3♠";
            return "3♦";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3♦")                                                           // 4'lü majör yok
            {
                if (e.Hcp >= 13 && (e.C >= 5 || e.D >= 5) && (e.H == 4 || e.S == 4)) return e.C >= e.D ? "4♣" : "4♦";
                if (e.H == 5 && e.S == 4) return "3♠";                                    // 5♥4♠ FG
                return "3♥";                                                              // 4♥4♠ SO
            }
            string m = rebid == "3♥" ? "♥" : "♠";
            if (e.Hcp <= 6) return "Pas";
            if (e.Hcp >= 13) return rebid == "3♥" ? "3♠" : "3NT";                        // tutuş / slam arayışı
            return rebid == "3♥" ? "4♥" : "4♠";                                          // oyun
        }

    }
}
