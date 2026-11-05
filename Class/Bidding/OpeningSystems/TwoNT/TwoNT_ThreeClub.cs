using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoNT
{
    /// <summary>2NT-3♣ gelişimi (kitap s.72): açan 3♦ majör yok, 3♥ 4/5♥, 3♠ 4/5♠, 3NT 4♥4♠</summary>
    public class TwoNT_ThreeClub : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2NT"; } }
        protected override string Cevap { get { return "3♣"; } }
        public override string Aciklama { get { return "2NT-3♣ gelişimi (kitap s.72): açan 3♦ majör yok, 3♥ 4/5♥, 3♠ 4/5♠, 3NT 4♥4♠"; } }

 
        internal override string AciciRebid(OcEl e)
        {
            if (e.H >= 4 && e.S >= 4) return "3NT";
            if (e.H >= 4) return "3♥";
            if (e.S >= 4) return "3♠";
            return "3♦";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3♦")                                                           // 4'lü majör yok
            {
                if (e.H == 4 && e.S >= 5) return "3♥";                                    // Smolen
                if (e.S == 4 && e.H >= 5) return "3♠";                                    // Smolen
                if (e.Hcp >= 13 && (e.C >= 5 || e.D >= 5) && (e.H == 4 || e.S == 4)) return e.C >= e.D ? "4♣" : "4♦";
                if (e.Hcp >= 11) return "4NT";
                return "3NT";
            }
            if (rebid == "3♥")
            {
                if (e.H >= 4) return e.Hcp >= 13 ? "3♠" : "4♥";                           // SI / oyun
                if (e.S == 4 && e.C >= 5) return "4♣";
                if (e.S == 4 && e.D >= 5) return "4♦";
                return e.Hcp >= 11 ? "4NT" : "3NT";
            }
            if (rebid == "3♠")
            {
                if (e.S >= 4) return e.Hcp >= 13 ? "4♥" : "4♠";
                if (e.H == 4 && e.C >= 5) return "4♣";
                if (e.H == 4 && e.D >= 5) return "4♦";
                return e.Hcp >= 11 ? "4NT" : "3NT";
            }
            // 3NT: 4♥ 4♠
            if (e.H >= 4 && e.S < 4) return e.Hcp >= 13 ? "4♥" : "4♣";                    // 4♣ transfer 4♥
            if (e.S >= 4) return e.Hcp >= 13 ? "4♠" : "4♦";                               // 4♦ transfer 4♠
            return "Pas";
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid == "3NT" && devam == "4♣") return "4♥";
            if (rebid == "3NT" && devam == "4♦") return "4♠";
            return null;
        }

    }
}
