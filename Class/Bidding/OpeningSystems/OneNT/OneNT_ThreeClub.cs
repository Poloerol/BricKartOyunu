using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    public class OneNT_ThreeClub : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1NT"; } }
        protected override string Cevap { get { return "3♣"; } }
        public override string Aciklama { get { return "1NT-3♣ gelişimi (kitap s.51,57): 3♦'ye zorla"; } }

        internal override string AciciRebid(OcEl e)
        {
            return (e.D >= 3 && e.Hcp >= 16) ? "3NT" : "3♦";                              // ♦ fiti MAX ile 3NT
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "3♦"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (e.D >= 5 && e.Hcp <= 7) return "Pas";                                     // 5+♦ S.O
            if (e.C >= 6)                                                                 // 6+♣ FG
            {
                if (e.H <= 1) return "3♥";                                                // SNG♥
                if (e.S <= 1) return "3♠";                                                // SNG♠
                return e.Hcp >= 16 ? "4♣" : "3NT";
            }
            return "3NT";
        }
    }
}
