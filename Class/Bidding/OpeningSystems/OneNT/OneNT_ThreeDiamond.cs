using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    public class OneNT_ThreeDiamond : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1NT"; } }
        protected override string Cevap { get { return "3♦"; } }
        public override string Aciklama { get { return "1NT-3♦ gelişimi (kitap s.51): 6+♦ FG SNG majör"; } }

        internal override string AciciRebid(OcEl e)
        {
            bool k = e.Stopper("Kupa"), m = e.Stopper("Maça");
            if (k && m) return "3NT";                                                     // majörlerde problem yok
            if (k) return "3♥";                                                           // iyi ♥, problem ♠
            if (m) return "3♠";                                                           // iyi ♠, problem ♥
            return "4♦";
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "3♥" || rebid == "3♠"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3♥") return e.S <= 1 ? "4♦" : "3NT";                            // SNG♠ ise açanın problemi ♠: ♦ oyna
            return e.H <= 1 ? "4♦" : "3NT";                                               // 3♠: problem ♥
        }
    }
}
