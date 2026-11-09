using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneHeart
{
    public class OneHeart_OneNT : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1♥"; } }
        protected override string Cevap { get { return "1NT"; } }
        public override string Aciklama { get { return "1♥-1NT gelişimi (kitap s.38-39)"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            bool duz = OneClub_OneDiamond.Duz(e);
            if (duz && hcp >= 20) return "3NT";
            if (h >= 7) return "4♥";
            if (h >= 6 && c >= 5 && hcp >= 11 && hcp <= 14) return "4♣";                  // 4/5 kayıp
            if (h >= 6 && d >= 5 && hcp >= 11 && hcp <= 14) return "4♦";
            if (h >= 6 && s >= 5 && hcp >= 11) return "3♠";
            if (h == 5 && (c >= 4 || d >= 4) && hcp >= 19) return "2♠";                   // FG
            if (h >= 6 && (c >= 4 || d >= 4) && hcp >= 15) return "2NT";
            if (h >= 6 && hcp >= 15 && hcp <= 17) return "3♥";
            if (h == 5 && c == 5 && hcp >= 14 && hcp <= 17) return "3♣";
            if (h == 5 && d == 5 && hcp >= 15 && hcp <= 17) return "3♦";
            if (h >= 6 && hcp <= 14) return "2♥";
            if (hcp >= 16) return "2♣";
            if (c >= 3) return "2♣";
            if (d >= 3) return "2♦";
            return "2♣";
        }

        internal override bool DevamTanimli(string rebid)
        {
            return rebid == "2♣" || rebid == "2♦" || rebid == "2♥" || rebid == "2♠" || rebid == "2NT";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            switch (rebid)
            {
                case "2♣":    // 5♥ 3+♣ 11-15 / 5+♥ 16+
                    return Sec(new[] {
                        K("3♥", x => hcp >= 9 && x.H >= 3),                              // davet
                        K("2♠", x => hcp >= 8 && (x.C >= 6 || x.D >= 6)),                // puppet → 2NT (6+ minör davet)
                        K("2♦", x => hcp >= 8),                                          // röle
                        K("3♣", x => x.C >= 6),
                        K("3♦", x => x.D >= 6),
                        K("2NT", x => x.C >= 5 && x.D >= 4 && x.H <= 1),
                        K("2♥", x => x.H >= 2),
                    }, e, "Pas");
                case "2♦":    // 5♥ 3+♦ 11-15
                    return Sec(new[] {
                        K("3♥", x => hcp >= 9 && x.H >= 3),
                        K("2♠", x => hcp >= 8 && (x.C >= 6 || x.D >= 6)),
                        K("2NT", x => hcp >= 8 && x.Dengeli),
                        K("3♣", x => x.C >= 6),
                        K("2♥", x => x.H >= 2),
                    }, e, "Pas");
                case "2♥":    // 6+♥ 11-14
                    return Sec(new[] {
                        K("3♥", x => hcp >= 9),                                          // davet
                        K("2♠", x => hcp >= 8 && (x.C >= 6 || x.D >= 6)),
                        K("2NT", x => hcp >= 8 && x.Dengeli),
                        K("3♣", x => x.C >= 6),
                        K("3♦", x => x.D >= 6),
                    }, e, "Pas");
                case "2♠":    // 5♥ 4+m 19+ FG
                    return Sec(new[] {
                        K("3♥", x => x.H >= 3 && hcp <= 6),                              // şaka: ortak Pas
                        K("3♣", x => hcp <= 7 && x.C >= 6),
                        K("3♦", x => hcp <= 7 && x.D >= 6),
                    }, e, "2NT");                                                         // röle
                case "2NT":   // 6♥ 4+m 15+
                    return Sec(new[] {
                        K("3♣", x => hcp >= 5),                                          // röle FG
                    }, e, "3♦");                                                          // S.O
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            int hcp = e.Hcp;
            if (rebid == "2♣" && devam == "2♠") return "2NT";                              // puppet
            if (rebid == "2♣" && devam == "2♦")
            {
                if (hcp <= 15 && e.C >= 3) return "2♥";
                if (OneClub_OneDiamond.Duz(e) && hcp >= 18) return "3NT";
                if (OneClub_OneDiamond.Duz(e) && hcp >= 16) return "2NT";
                if (e.S >= 4 && hcp >= 16) return "2♠";
                if (e.H >= 6 && hcp >= 16) return "3♥";
                if (e.D >= 4 && hcp >= 16) return "3♦";
                if (e.C >= 4 && hcp >= 16) return "3♣";
                return "2♥";
            }
            if (rebid == "2♦" && devam == "2♠") return "2NT";                              // puppet
            if (rebid == "2♥" && devam == "2♠") return "2NT";
            if (rebid == "2♠" && devam == "2NT")                                           // 5♥4♣ ; 5♥4♦ ; 5♥5♣ ; 5♥5♦ ; 6+♥
            {
                if (e.H >= 6) return "3NT";
                if (e.C >= 5) return "3♥";
                if (e.D >= 5) return "3♠";
                return e.C >= 4 ? "3♣" : "3♦";
            }
            if (rebid == "2NT" && devam == "3♦")                                           // S.O (pass or correct): karoda 4+ varsa Pas, yoksa majör
                return e.D >= 4 ? "Pas" : "3♥";
            if (rebid == "2NT" && devam == "3♣")
            {
                if (e.H >= 6 && e.C >= 5) return "4♣";
                if (e.H >= 6 && e.D >= 5) return "4♦";
                if (e.D >= 4) return e.Hcp >= 18 ? "3NT" : "3♥";
                return e.Hcp >= 18 ? "3♠" : "3♦";
            }
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (devam == "2♠" && roleCevap == "2NT" && (rebid == "2♣" || rebid == "2♦" || rebid == "2♥"))   // puppet: 6+ minör
                return e.C >= 6 ? "3♣" : "3♦";
            return null;
        }

        // Puppet sekansi (2♣/2♦/2♥ - 2♠ - 2NT - 3♣/3♦): cevapci 6+ minor gosterdi; acan sozlesmeyi kabul eder, 17+ HP ile 3NT dener.
        internal override bool KararTanimli(string rebid, string devam) { return devam == "2♠" && (rebid == "2♣" || rebid == "2♦" || rebid == "2♥"); }
        internal override string AciciSon(OcEl e, string rebid, string devam, string roleCevap, string karar)
        {
            if (devam == "2♠" && roleCevap == "2NT" && (rebid == "2♣" || rebid == "2♦" || rebid == "2♥") && (karar == "3♣" || karar == "3♦"))
                return e.Hcp >= 17 ? "3NT" : "Pas";
            return null;
        }
    }
}
