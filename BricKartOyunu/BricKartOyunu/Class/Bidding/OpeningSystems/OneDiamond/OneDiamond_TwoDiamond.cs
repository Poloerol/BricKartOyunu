using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>1♦ ⇒ 2♦ GELİŞİMİ (SAS kitabı s.33): 5/4 minör 7-9 HCP. Gelişmeler natürel; açanın 2NT FG rölesi.</summary>
    public class OneDiamond_TwoDiamond : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♦"; } }
        protected override string Cevap { get { return "2♦"; } }
        public override string Aciklama { get { return "1♦-2♦ gelişimi (kitap s.33)"; } }

        internal override bool AciciRebidTanimli(OcEl e) { return e.Hcp >= 16; }
        internal override string AciciRebid(OcEl e) { return "2NT"; }                     // FG röle
        internal override bool DevamTanimli(string rebid) { return rebid == "2NT"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            // 3♣ min; 3♦ max 2♠2♥; 3♥ max 3♠ 1♥; 3♠ max 3♥ 1♠
            if (e.Hcp <= 8) return "3♣";
            if (e.S == 2 && e.H == 2) return "3♦";
            if (e.S == 3 && e.H == 1) return "3♥";
            if (e.H == 3 && e.S == 1) return "3♠";
            return "3♦";
        }
    }
}
