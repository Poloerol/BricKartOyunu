using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoClub
{
    /// <summary>2♣-3♦ (4♥ OG) gelişimi (kitap s.63): 3♥ 4+♥ → 3♠ koz/♠ kontrol / 3NT / 4♣ 5+♣ / 4♦ 5+♦.</summary>
    public class TwoClub_ThreeDiamond : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♣"; } }
        protected override string Cevap { get { return "3♦"; } }
        public override string Aciklama { get { return "2♣-3♦ gelişimi (kitap s.63)"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.H >= 4) return "3♥";
            if (e.S >= 5) return "3♠";
            if (e.Dengeli) return "3NT";
            return e.C >= e.D ? "4♣" : "4♦";
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "3♥" || rebid == "3♠"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3♠") return TcDevam.Dogal(e, rebid);
            if (e.C >= 5) return "4♣";
            if (e.D >= 5) return "4♦";
            if (e.S <= 1 || e.El.Any(c => c.Suit == "Maça" && c.Value >= 13)) return "3♠";   // ♠ kontrol
            return "3NT";
        }
    }
}
