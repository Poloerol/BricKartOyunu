using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>1♦ ⇒ 2♠ GELİŞİMİ (SAS kitabı s.33): 5+♦ davet / 5-4 minör FG / 6+♣ davet. Açanın 2NT röleye cevapları.</summary>
    public class OneDiamond_TwoSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♦"; } }
        protected override string Cevap { get { return "2♠"; } }
        public override string Aciklama { get { return "1♦-2♠ gelişimi (kitap s.33)"; } }

        internal override bool AciciRebidTanimli(OcEl e) { return e.Hcp >= 13; }
        internal override string AciciRebid(OcEl e) { return "2NT"; }
        internal override bool DevamTanimli(string rebid) { return rebid == "2NT"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            bool minorlar = e.C >= 4 && e.D >= 4 && e.C + e.D >= 9 && e.Hcp >= 12;
            if (minorlar)
            {
                if (e.H <= 1) return "3♥";
                if (e.S <= 1) return "3♠";
                if (e.S == 2 && e.H == 2) return "3NT";
            }
            if (e.C >= 6) return "3♣";
            return "3♦";
        }
    }
}
