using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoClub
{
    /// <summary>2♣-3♣ (6+♦ 8+) gelişimi (kitap s.62): 3♦ 2+♦ → 3♥ 5+♦ SNG yok / 3♠ SNG ♥ / 3NT SNG ♠ / 4♣ SNG ♠ SI.</summary>
    public class TwoClub_ThreeClub : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♣"; } }
        protected override string Cevap { get { return "3♣"; } }
        public override string Aciklama { get { return "2♣-3♣ gelişimi (kitap s.62)"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.D >= 2) return "3♦";
            if (e.H >= 5) return "3♥";
            if (e.S >= 5) return "3♠";
            return "3NT";
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "3♦" || rebid == "3♥" || rebid == "3♠"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid != "3♦") return TcDevam.Dogal(e, rebid);
            if (e.H == 1) return "3♠";
            if (e.S == 1) return e.Hcp >= 12 ? "4♣" : "3NT";
            return "3♥";
        }
    }
}
