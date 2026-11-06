using System.Collections.Generic;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoNT
{
    /// <summary>
    /// 2NT-4♦ gelişimi (kitap s.74): 5♠5♥ ya da 6+♠, slam daveti. Açan tercih eder: uzun majörünü söyler (♠ ≥ ♥ ise 4♠, değilse 4♥).
    /// Cevapçı 4♥ tercihine 6+♠ ise 4♠, 5-5 ise 4NT (RKCB ♥); 4♠ tercihine 4NT (RKCB ♠).
    /// </summary>
    public class TwoNT_FourDiamond : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2NT"; } }
        protected override string Cevap { get { return "4♦"; } }
        public override string Aciklama { get { return "2NT-4♦ gelişimi (kitap s.74): 5♠5♥ veya 6+♠ SI; açan 4♥/4♠ tercih, cevapçı 4♠ (6+♠) / 4NT RKCB"; } }

        internal override string AciciRebid(OcEl e) { return e.S >= e.H ? "4♠" : "4♥"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "4♥") return e.S >= 6 ? "4♠" : "4NT";
            return "4NT";
        }
    }

    /// <summary>2NT-4♥ gelişimi (kitap s.74): 5/4 minör, ♥ singleton, slam daveti. Açan asgari (≤21 HCP) 4NT (oynamak için), azami (22) 5m RKCB (uzun minör, eşitse ♦).</summary>
    public class TwoNT_FourHeart : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2NT"; } }
        protected override string Cevap { get { return "4♥"; } }
        public override string Aciklama { get { return "2NT-4♥ gelişimi (kitap s.74): 5/4 minör SNG ♥; açan 4NT oynamak için / 5m RKCB"; } }

        internal override string AciciRebid(OcEl e) { return TwoNT_Minor.Rebid(e); }
        internal override bool DevamTanimli(string rebid) { return rebid == "4NT"; }
        internal override string CevapciDevam(OcEl e, string rebid) { return "Pas"; }
    }

    /// <summary>2NT-4♠ gelişimi (kitap s.74): 5/4 minör, ♠ singleton, slam daveti (4♥ ile aynı devam).</summary>
    public class TwoNT_FourSpade : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2NT"; } }
        protected override string Cevap { get { return "4♠"; } }
        public override string Aciklama { get { return "2NT-4♠ gelişimi (kitap s.74): 5/4 minör SNG ♠; açan 4NT oynamak için / 5m RKCB"; } }

        internal override string AciciRebid(OcEl e) { return TwoNT_Minor.Rebid(e); }
        internal override bool DevamTanimli(string rebid) { return rebid == "4NT"; }
        internal override string CevapciDevam(OcEl e, string rebid) { return "Pas"; }
    }

    internal static class TwoNT_Minor
    {
        public static string Rebid(OcEl e) { return e.Hcp >= 22 ? (e.C > e.D ? "5♣" : "5♦") : "4NT"; }
    }
}
