using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>1♦ ⇒ 2♥ GELİŞİMİ (SAS kitabı s.33): 5♠ 4+♥ 3-8 HCP. Açan Pas/2♠ tercih ya da 2NT röle (davet+).</summary>
    public class OneDiamond_TwoHeart : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♦"; } }
        protected override string Cevap { get { return "2♥"; } }
        public override string Aciklama { get { return "1♦-2♥ gelişimi (kitap s.33)"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.Hcp >= 16) return "2NT";
            if (e.S > e.H) return "2♠";                                                    // tercih
            return "Pas";
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "2NT"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (e.S >= 5 && e.H >= 5) return "3♦";
            if (e.Hcp <= 5) return "3♣";                                                   // min
            if (e.D == 3 && e.C == 1) return "3♠";
            if (e.D == 1 && e.C == 3) return "3NT";
            return "3♥";                                                                   // max 2♦2♣ (SEÇİM: diğerleri)
        }
    }
}
