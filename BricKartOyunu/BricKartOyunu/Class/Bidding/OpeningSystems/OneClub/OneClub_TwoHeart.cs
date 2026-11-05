using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 2♥ GELİŞİMİ (SAS kitabı s.23): cevapçı 5♠ 4+♥ 3-8 HCP.
    /// Açan: Pas, 2♠ tercih S.O., 2NT SAS davet+, 3♣ 6+♣ iyi el (majörlere fit yok), 4♥/4♠ oynamak için.
    /// Cevapçı 2NT'ye: 3♣ min, 3♦ 5♠5♥, 3♥ 5♠4♥2♦2♣ max, 3♠ 5♠4♥3♦1♣ max, 3NT 5♠4♥3♣1♦ max.
    /// </summary>
    public class OneClub_TwoHeart : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "2♥"; } }
        public override string Aciklama { get { return "1♣-2♥ gelişimi"; } }

        internal override string AciciRebid(OcEl e)
        {
            return Sec(new[] {
                K("2NT", x => x.Hcp >= 15),                                            // SAS davet+
                K("3♣", x => x.C >= 6 && x.H <= 2 && x.S <= 2),
                K("4♥", x => x.H >= 4 && x.Hcp >= 13),
                K("4♠", x => x.S >= 4 && x.Hcp >= 13),
                K("2♠", x => x.S >= 3 && x.S >= x.H),                                   // tercih S.O.
            }, e, "Pas");
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "2NT")
            {
                bool max = e.Hcp >= 6;
                if (!max) return "3♣";
                if (e.S >= 5 && e.H >= 5) return "3♦";
                if (e.D == 2 && e.C == 2) return "3♥";
                if (e.D == 3 && e.C == 1) return "3♠";
                if (e.C == 3 && e.D == 1) return "3NT";
                return "3♣";
            }
            return "Pas";
        }
    }
}
