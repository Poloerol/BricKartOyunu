using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoNT
{
    /// <summary>2NT-3♠ gelişimi (kitap s.74): transfer ♣ / minörler; açan 3NT uygunluk yok, 4♣ uygunluk var</summary>
    public class TwoNT_ThreeSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2NT"; } }
        protected override string Cevap { get { return "3♠"; } }
        public override string Aciklama { get { return "2NT-3♠ gelişimi (kitap s.74): transfer ♣ / minörler; açan 3NT uygunluk yok, 4♣ uygunluk var"; } }

 
        internal override string AciciRebid(OcEl e)
        {
            return (e.C >= 3 || e.Hcp >= 22) ? "4♣" : "3NT";                              // ♣ uygunluğu
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3NT")
            {
                if (e.C >= 6 && e.S <= 1) return "4♣";
                if (e.C >= 6) return "4♦";
                if (e.C == 5 && e.D == 5 && e.H == 1) return "4♥";
                if (e.C == 5 && e.D == 5 && e.S == 1) return "4♠";
                return "4NT";
            }
            if (e.C >= 6) return "4♦";                                                   // SI
            if (e.C == 5 && e.D == 5 && e.H == 1) return "4♥";
            if (e.C == 5 && e.D == 5 && e.S == 1) return "4♠";
            return "4NT";
        }

    }
}
