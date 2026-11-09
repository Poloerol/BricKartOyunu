using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    public class OneNT_TwoClub : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1NT"; } }
        protected override string Cevap { get { return "2♣"; } }
        public override string Aciklama { get { return "1NT-2♣ gelişimi (kitap s.53-54): NF Stayman"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.H >= 4) return "2♥";                                                    // 4/5♥ (4♠ olabilir)
            if (e.S >= 4) return "2♠";                                                    // 4/5♠
            return "2♦";                                                                  // 4'lü majör yok
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            switch (rebid)
            {
                case "2♦":
                    if (e.H == 4 && e.S == 4 && hcp <= 7) return "2♥";                    // 4/5♥ 4♠ S.O
                    if (e.S >= 5 && hcp >= 8 && hcp <= 9) return "2♠";                    // 5+♠ davet
                    if (hcp <= 7) return "Pas";
                    if (hcp <= 9) return "2NT";                                           // davet
                    if (hcp >= 18) return "6NT";
                    if (hcp >= 16) return "4NT";
                    return "3NT";
                case "2♥":
                    if (e.H >= 4) return hcp <= 7 ? "Pas" : (hcp <= 9 ? "3♥" : "4♥");     // fit: davet / oyun
                    if (e.S >= 5 && hcp >= 8 && hcp <= 9) return "2♠";                    // 5+♠ davet
                    if (e.S >= 4 && hcp >= 10) return "3♠";                               // 4+♠ bilinmeyen SNG sorusu
                    if (hcp <= 7) return "Pas";
                    if (hcp <= 9) return "2NT";
                    return "3NT";
                case "2♠":
                    if (e.S >= 4) return hcp <= 7 ? "Pas" : (hcp <= 9 ? "3♠" : "4♠");     // fit: davet / oyun
                    if (hcp <= 7) return "Pas";
                    if (hcp <= 9) return "2NT";
                    return "3NT";
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            bool max = e.Hcp >= 16;
            // 2NT daveti (8-9 HP): MAX kabul eder
            if (devam == "2NT" && (rebid == "2♦" || rebid == "2♥" || rebid == "2♠")) return max ? "3NT" : "Pas";
            // 3M daveti (fit, 8-9 HP)
            if (devam == "3♥" && rebid == "2♥") return (max || (e.Hcp >= 15 && e.H >= 5)) ? "4♥" : "Pas";
            if (devam == "3♠" && rebid == "2♠") return (max || (e.Hcp >= 15 && e.S >= 5)) ? "4♠" : "Pas";
            // 2♦ sonrası / 2♥ sonrası 5♠ daveti
            if (devam == "2♠" && (rebid == "2♦" || rebid == "2♥")) return max ? (e.S >= 3 ? "4♠" : "3NT") : (e.S >= 3 ? "3♠" : "Pas");
            if (rebid == "2♥" && devam == "3♠")                                           // 4+♠ bilinmeyen SNG sorusu
            {
                if (e.C <= 1) return "3NT";
                if (e.D <= 1) return "4♣";
                if (e.S <= 1) return "4♥";
                return e.S >= 4 ? "4♠" : "3NT";
            }
            return null;
        }
    }
}
