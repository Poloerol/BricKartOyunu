using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    public class OneNT_TwoNT : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1NT"; } }
        protected override string Cevap { get { return "2NT"; } }
        public override string Aciklama { get { return "1NT-2NT gelişimi (kitap s.56): 5'li majör sorusu"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.H >= 5) return "3♥";
            if (e.S >= 5) return "3♠";
            return "3♣";                                                                  // 5'li majör yok
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3♥") return e.H >= 3 ? "4♥" : "3NT";
            if (rebid == "3♠") return e.S >= 3 ? "4♠" : "3NT";
            return "3NT";
        }
    }
}
