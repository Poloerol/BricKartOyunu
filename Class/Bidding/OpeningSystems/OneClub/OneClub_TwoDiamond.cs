using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 2♦ GELİŞİMİ (SAS kitabı s.23): cevapçı 5/4 minör 7-9 HCP. Gelişmeler natüreldir;
    /// açanın 2NT'si FG röledir: 3♣ min, 3♦ 5-4-2-2 max, 3♥ 5-4 minör 3♠1♥ max, 3♠ 5-4 minör 3♥1♠ max.
    /// </summary>
    public class OneClub_TwoDiamond : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "2♦"; } }
        public override string Aciklama { get { return "1♣-2♦ gelişimi"; } }

        internal override string AciciRebid(OcEl e)
        {
            return Sec(new[] {
                K("2NT", x => x.Hcp >= 15),                                            // FG röle
                K("3♣", x => x.C >= 6),
                K("3♦", x => x.D >= 4 && x.Hcp >= 12),
                K("2♥", x => x.H >= 5),
                K("2♠", x => x.S >= 5),
            }, e, "Pas");
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "2NT")   // 5/4 minör cevapçı: 3♣ min; max ise dağılımı söyler
            {
                bool max = e.Hcp >= 9;
                if (!max) return "3♣";
                if (e.S == 2 && e.H == 2) return "3♦";
                if (e.S == 3 && e.H == 1) return "3♥";
                if (e.H == 3 && e.S == 1) return "3♠";
                return "3♦";
            }
            return "Pas";
        }
    }
}
