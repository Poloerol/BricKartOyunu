using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    public class OneNT_FourDiamond : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1NT"; } }
        protected override string Cevap { get { return "4♦"; } }
        public override string Aciklama { get { return "1NT-4♦ Texas transferi (kitap s.51): açan 4♥ ile kabul eder"; } }

        internal override string AciciRebid(OcEl e) { return "4♥"; }

        internal override bool DevamTanimli(string rebid) { return false; }

        internal override string CevapciDevam(OcEl e, string rebid) { return "Pas"; }
    }
}
