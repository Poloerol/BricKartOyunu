using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 1NT GELİŞİMİ (SAS kitabı s.21). Cevapçı 7-11 HCP, 4'lü majör yok.
    /// Açan: Pas (11-14 dengeli), 2♣ 6+♣ 11-14, 2♦ röle 16+, 2♥/2♠ 5+♣ 4M 17+, 2NT 18+ 6+♣,
    /// 3♣ 6+♣ 14-17, 3♦/3♥/3♠ 6+♣ 5x. Cevapçı "min" 7-8, "max" 9-11 HCP.
    /// </summary>
    public class OneClub_OneNT : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "1NT"; } }
        public override string Aciklama { get { return "1♣-1NT gelişimi"; } }

        internal override string AciciRebid(OcEl e)
        {
            return Sec(new[] {
                K("2NT", x => x.Hcp >= 18 && x.C >= 6),
                K("2♥", x => x.Hcp >= 17 && x.C >= 5 && x.H >= 4),
                K("2♠", x => x.Hcp >= 17 && x.C >= 5 && x.S >= 4),
                K("2♦", x => x.Hcp >= 16),                                            // röle: dağılım ve güç sorar
                K("3♦", x => x.C >= 6 && x.D >= 5),
                K("3♥", x => x.C >= 6 && x.H >= 5),
                K("3♠", x => x.C >= 6 && x.S >= 5),
                K("3♣", x => x.Hcp >= 14 && x.Hcp <= 17 && x.C >= 6),
                K("2♣", x => x.Hcp >= 11 && x.Hcp <= 14 && x.C >= 6),
            }, e, "Pas");
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            bool max = e.Hcp >= 9;
            switch (rebid)
            {
                case "2♦":   // röle: 2♥ min, 2♠ 5♦, 2NT 4♦-3-3-3 max, 3♣ 4/5♣ max, 3♦ 4♣4♦ max
                    return Sec(new[] {
                        K("3♦", x => max && x.C >= 4 && x.D >= 4),
                        K("3♣", x => max && x.C >= 4),
                        K("2NT", x => max && x.D == 4 && x.Dengeli),
                        K("2♠", x => x.D >= 5),
                    }, e, "2♥");
                case "2♥":   // 5+♣ 4♥ 17+
                    return Sec(new[] {
                        K("3♣", x => x.Hcp >= 10 && x.Stopper("Kupa")),                 // tutuş FG
                        K("3NT", x => max && x.Stopper("Kupa") && x.Dengeli),
                        K("3♦", x => max),                                              // değerler max
                        K("2♠", x => x.Hcp >= 8),
                    }, e, "2NT");
                case "2♠":   // 5+♣ 4♠ 17+
                    return Sec(new[] {
                        K("3♣", x => x.Hcp >= 10 && x.Stopper("Maça")),
                        K("3NT", x => max && x.Stopper("Maça") && x.Dengeli),
                        K("3♦", x => max),
                    }, e, "2NT");
                case "2NT":  // 18+ 6+♣
                    return Sec(new[] {
                        K("3NT", x => x.Hcp >= 6 && x.Stopper("Maça") && x.Stopper("Kupa") && x.Stopper("Karo")),
                    }, e, "3♣");
                default:     // 2♣, 3♣, 3♦, 3♥, 3♠: gelişim natürel
                    return "Pas";
            }
        }
    }
}
