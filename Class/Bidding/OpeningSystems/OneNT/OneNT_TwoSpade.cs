using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    public class OneNT_TwoSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1NT"; } }
        protected override string Cevap { get { return "2♠"; } }
        public override string Aciklama { get { return "1NT-2♠ gelişimi (kitap s.56): aralık sorusu"; } }

        internal override string AciciRebid(OcEl e)
        {
            return e.Hcp >= 16 ? "3♣" : "2NT";                                            // MAX / MIN
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "2NT") return "3♣";                                              // 5+♣ S.O
            return "Pas";                                                                 // 3♣ MAX: Pas
        }
    }
}
