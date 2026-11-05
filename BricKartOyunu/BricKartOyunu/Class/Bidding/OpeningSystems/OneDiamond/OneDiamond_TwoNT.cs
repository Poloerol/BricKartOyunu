using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>1♦ ⇒ 2NT GELİŞİMİ (SAS kitabı s.34): 11-12 dengeli. Açan Pas (min), 3♣ puppet ya da 16+ sıçramalar.</summary>
    public class OneDiamond_TwoNT : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♦"; } }
        protected override string Cevap { get { return "2NT"; } }
        public override string Aciklama { get { return "1♦-2NT gelişimi (kitap s.34)"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp;
            if (hcp >= 16 && e.D >= 6 && e.Singleton)
            {
                if (e.C <= 1) return "3♦";
                if (e.H <= 1) return "3♥";
                return "3♠";
            }
            if (hcp >= 16 && e.D >= 5 && e.C >= 4) return "3♣";                           // puppet to 3♦
            if (e.D >= 6 && hcp <= 15) return "3♣";                                        // puppet: 3♦ S.O
            if (hcp >= 13) return "3NT";
            return "Pas";
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "3♣"; }
        internal override string CevapciDevam(OcEl e, string rebid) { return "3♦"; }       // puppet'a uyar

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid != "3♣" || devam != "3♦") return null;
            if (e.D >= 6 && e.Hcp <= 15) return "Pas";                                      // 6+♦ S.O
            if (e.D >= 5 && e.C >= 5 && e.Hcp >= 17) return "3♥";
            if (e.Hcp >= 18) return "4♣";
            if (e.Hcp >= 16) return "3♠";
            return "3NT";
        }
    }
}
