using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    public class OneNT_ThreeHeart : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1NT"; } }
        protected override string Cevap { get { return "3♥"; } }
        public override string Aciklama { get { return "1NT-3♥ gelişimi (kitap s.51,57): 5/5 minör SNG ♥"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.Stopper("Kupa")) return "3NT";                                            // SNG ♥ için durdurucu
            return e.C >= e.D ? "4♣" : "4♦";                                              // minör oyna
        }

        internal override bool DevamTanimli(string rebid) { return false; }                // gelişim natürel

        internal override string CevapciDevam(OcEl e, string rebid) { return "Pas"; }
    }
}
