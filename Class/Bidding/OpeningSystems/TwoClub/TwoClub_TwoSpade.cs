using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoClub
{
    /// <summary>2♣-2♠ (6+♣ 8+) gelişimi (kitap s.62): 2NT dengeli; 3♣ 3+♣ SI; 3♦/3♥/3♠ doğal. Cevapçı SNG gösterir.</summary>
    public class TwoClub_TwoSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♣"; } }
        protected override string Cevap { get { return "2♠"; } }
        public override string Aciklama { get { return "2♣-2♠ gelişimi (kitap s.62)"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.C >= 3 && e.Hcp >= 27) return "3♣";                                     // SI
            if (e.Dengeli) return "2NT";
            if (e.D >= 5) return "3♦";
            if (e.H >= 5) return "3♥";
            if (e.S >= 5) return "3♠";
            return e.C >= 3 ? "3♣" : "2NT";
        }

        internal override bool DevamTanimli(string rebid) { return TcDevam.Tanimli(rebid); }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid != "2NT" && rebid != "3♣") return TcDevam.Dogal(e, rebid);
            bool si = rebid == "3♣";
            if (e.C == 5 && e.D == 5) return "3♦";
            if (e.H == 1) return "3♠";                                                    // 6+♣ SNG ♥
            if (e.S == 1) return si ? "3NT" : (e.Hcp >= 12 ? "4♣" : "3NT");                // 6+♣ SNG ♠
            if (!si && e.C == 5 && e.Dengeli) return "3♥";                                // 5♣ 3-3-2
            return si ? "3♥" : "3♣";                                                      // SNG yok
        }
    }
}
