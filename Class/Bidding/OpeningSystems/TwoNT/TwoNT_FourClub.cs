using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoNT
{
    /// <summary>2NT-4♣ gelişimi (kitap s.74): transfer ♦; açan 4♦ uygunluk var / 4NT yok</summary>
    public class TwoNT_FourClub : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2NT"; } }
        protected override string Cevap { get { return "4♣"; } }
        public override string Aciklama { get { return "2NT-4♣ gelişimi (kitap s.74): transfer ♦; açan 4♦ uygunluk var / 4NT yok"; } }

 
        internal override string AciciRebid(OcEl e)
        {
            return (e.D >= 3 || e.Hcp >= 22) ? "4♦" : "4NT";
        }

        internal override bool DevamTanimli(string rebid) { return false; }
        internal override string CevapciDevam(OcEl e, string rebid) { return "Pas"; }

    }
}
