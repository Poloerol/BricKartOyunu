using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoNT
{
    /// <summary>2NT-3♦ gelişimi (kitap s.73): transfer ♥; açan 3♥ (3+♥) / 3♠,4♣,4♦ (4+♥ max cue) / 3NT (2♥) / 4♥</summary>
    public class TwoNT_ThreeDiamond : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2NT"; } }
        protected override string Cevap { get { return "3♦"; } }
        public override string Aciklama { get { return "2NT-3♦ gelişimi (kitap s.73): transfer ♥; açan 3♥ (3+♥) / 3♠,4♣,4♦ (4+♥ max cue) / 3NT (2♥) / 4♥"; } }

 
        internal override string AciciRebid(OcEl e)
        {
            if (e.H >= 4 && e.Hcp >= 22) return "4♥";
            if (e.H >= 3) return "3♥";
            if (e.H == 2) return "3NT";                                                  // iki kupa: oynamak için
            return "3♥";                                                                  // 3'ten az ise de transfer kabulü
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3NT")
            {
                if (e.Hcp >= 13 && e.C >= 4 && e.H >= 5) return "4♣";
                if (e.Hcp >= 13 && e.D >= 4 && e.H >= 5) return "4♦";
                return e.H >= 5 && e.Hcp >= 4 ? "4♥" : "Pas";
            }
            // 3♥: transfer tamamlandı
            if (e.Hcp <= 3) return "Pas";
            if (e.Hcp >= 13)
            {
                if (e.H >= 5 && e.C >= 4) return "4♣";
                if (e.H >= 5 && e.D >= 4) return "4♦";
                return e.H >= 6 ? "4NT" : "3♠";                                          // SI
            }
            return e.H >= 5 ? "4♥" : "3NT";
        }

    }
}
