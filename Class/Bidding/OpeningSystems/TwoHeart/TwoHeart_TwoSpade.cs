using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoHeart
{
    /// <summary>2♥-2♠ gelişimi (kitap s.69): açan 2NT 6♥ 4x → 3♣ röle; 3♣ SNG minör; 3♦ SNG ♠; 3♥ SNG yok min; 3♠ SNG yok max</summary>
    public class TwoHeart_TwoSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♥"; } }
        protected override string Cevap { get { return "2♠"; } }
        public override string Aciklama { get { return "2♥-2♠ gelişimi (kitap s.69): açan 2NT 6♥ 4x → 3♣ röle; 3♣ SNG minör; 3♦ SNG ♠; 3♥ SNG yok min; 3♠ SNG yok max"; } }

 
        internal override string AciciRebid(OcEl e)
        {
            bool max = e.Hcp >= 9;
            int dort = (e.S >= 4 ? 1 : 0) + (e.D >= 4 ? 1 : 0) + (e.C >= 4 ? 1 : 0);
            if (e.C == 1 || e.D == 1) return "3♣";                                       // 6♥ SNG minör
            if (e.S == 1) return "3♦";                                                   // 6♥ SNG ♠
            if (dort > 0) return "2NT";                                                  // 6♥ 4x
            return max ? "3♠" : "3♥";                                                    // SNG yok
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "2NT") return "3♣";                                             // röle: hangi 4'lü
            if (rebid == "3♥") return e.Hcp >= 13 ? "4♥" : "Pas";                        // asgari
            if (rebid == "3♠") return "4♥";                                              // azami
            return e.Hcp >= 12 ? "4♥" : "3♥";                                            // SNG: oyun / partskor
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid == "2NT" && devam == "3♣") return e.C >= 4 ? "3♦" : (e.D >= 4 ? "3♥" : "3♠");
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (rebid != "2NT" || devam != "3♣") return null;
            // 4'lü yan rengin türüne göre oyun: ♥ oynanır (4♥), zayıfsa 3♥
            return e.Hcp >= 12 ? "4♥" : "3♥";
        }

        internal override bool KararTanimli(string rebid, string devam) { return rebid == "2NT"; }

    }
}
