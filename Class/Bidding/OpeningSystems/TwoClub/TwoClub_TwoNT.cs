using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoClub
{
    /// <summary>2♣-2NT (5/4+ majörler 7+) gelişimi (kitap s.62): 3♣ röle → 3♦ 5♠5♥ / 3♥ 4♥ 5+♠ / 3♠ 4♠ 5+♥; 3♦ 6+♦; 3♥/3♠ SI doğal.</summary>
    public class TwoClub_TwoNT : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♣"; } }
        protected override string Cevap { get { return "2NT"; } }
        public override string Aciklama { get { return "2♣-2NT gelişimi (kitap s.62)"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.D >= 6) return "3♦";
            if ((e.H >= 4 || e.S >= 4) && e.Hcp >= 27) return e.H >= e.S ? "3♥" : "3♠";
            return "3♣";                                                                  // röle
        }

        internal override bool DevamTanimli(string rebid) { return TcDevam.Tanimli(rebid); }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid != "3♣") return TcDevam.Dogal(e, rebid);
            if (e.S >= 5 && e.H >= 5) return "3♦";
            if (e.S >= 5) return "3♥";                                                    // 4♥ 5+♠
            return "3♠";                                                                  // 4♠ 5+♥
        }
    }
}
