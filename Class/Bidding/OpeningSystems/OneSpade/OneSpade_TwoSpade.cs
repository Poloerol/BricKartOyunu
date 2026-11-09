using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneSpade
{
    public class OneSpade_TwoSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♠"; } }
        protected override string Cevap { get { return "2♠"; } }
        public override string Aciklama { get { return "1♠-2♠ gelişimi (kitap s.50): 3+♠ yapıcı 7-10"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            if (hcp <= 11) return "Pas";
            if (s >= 5 && h >= 5) return "4♥";                                            // SI
            if (s >= 5 && c >= 5) return "4♣";                                            // SI
            if (s >= 5 && d >= 5) return "4♦";                                            // SI
            if (hcp >= 14 && s >= 5 && h == 4) return "3♥";                               // oyun / slem denemesi
            if (OneClub_OneDiamond.Duz(e) && hcp >= 15) return "2NT";                     // dengeli davet / SI
            if (hcp >= 14) return "4♠";
            if (c >= 4 && c >= d) return "3♣";                                            // yardımcı renk oyun denemesi
            if (d >= 4) return "3♦";
            if (h == 4) return "3♥";
            return OneClub_OneDiamond.Duz(e) ? "2NT" : "Pas";
        }

        // Acanin deneme teklifleri (2NT / 3♥ / 3♣ / 3♦) zorlayici: cevapci 9-10 HP ile oyuna, daha azla 3♠ ile cevap verir.
        internal override bool DevamTanimli(string rebid) { return rebid == "2NT" || rebid == "3♥" || rebid == "3♣" || rebid == "3♦" || rebid == "4♣" || rebid == "4♦" || rebid == "4♥"; }
        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "4♣" || rebid == "4♦" || rebid == "4♥") return "4♠";   // 5-5 iki renkli: majöre dön
            return e.Hcp >= 9 ? "4♠" : "3♠";
        }
    }
}
