using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 2♠ GELİŞİMİ (SAS kitabı s.23): cevapçı 6+♣ iyi renk davet veya 5/4 minör FG.
    /// Açan: 2NT röle (3♣ 5+♣ davet, 3♦ 5/4 minör 2♠2♥ FG, 3♥ 5/4 minör 3♠1♥ FG, 3♠ 5/4 minör 3♥1♠ FG)
    /// veya 3♣ (4+♣ min dengesiz, natürel).
    /// </summary>
    public class OneClub_TwoSpade : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "2♠"; } }
        public override string Aciklama { get { return "1♣-2♠ gelişimi"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.C >= 4 && !e.Dengeli && e.Hcp <= 14) return "3♣";
            return "2NT";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "2NT")
            {
                if (e.C >= 6) return "3♣";                                              // 6+♣ iyi renk davet
                if (e.S == 2 && e.H == 2) return "3♦";
                if (e.S == 3 && e.H == 1) return "3♥";
                if (e.S == 1 && e.H == 3) return "3♠";
                return "3♣";
            }
            return "Pas";
        }
    }
}
