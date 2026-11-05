using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 1♠ GELİŞİMİ (SAS kitabı s.18-20): açanın yeniden teklifi, cevapçının devamı, açanın röle cevapları.
    /// Cevapçı gücü: zayıf 4-8, davet 9-11, oyun 12+ HCP (kitapta açık olmayan yerler "SEÇİM").
    /// </summary>
    public class OneClub_OneSpade : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "1♠"; } }
        public override string Aciklama { get { return "1♣-1♠ gelişimi"; } }

        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            bool asK = e.El.Any(k => k.Suit == "Sinek" && k.Value == 14) && ElDegerlendirici.PapazVar(e.El, "Sinek");
            bool asVeyaK = e.El.Any(k => k.Suit == "Sinek" && k.Value == 14) || ElDegerlendirici.PapazVar(e.El, "Sinek");

            // 18+ HCP 4-4-4-1: ♦ tek → 3♦, ♥ tek → 3♥
            if (hcp >= 18 && c >= 4 && s >= 4 && h >= 4 && d == 1) return "3♦";
            if (hcp >= 18 && c >= 4 && s >= 4 && d >= 4 && h == 1) return "3♥";
            // 3NT: 5♣ 4♠ 2♥ 2♦ 18-20
            if (hcp >= 18 && hcp <= 20 && c == 5 && s == 4 && h == 2 && d == 2) return "3NT";
            // 6+♣ 4♠ sıçramaları
            if (c >= 6 && s >= 4 && hcp >= 13 && hcp <= 16 && asVeyaK)
            {
                if (hcp >= 14 && asK && d == 1) return "4♣";    // küçük kısa (♦)
                if (hcp >= 14 && asK && h == 1) return "4♦";    // büyük kısa (♥)
                return "4♠";
            }
            // 18+ HCP
            if (hcp >= 18 && (c >= 6 || (c >= 5 && s >= 4))) return "2NT";
            // 17+ HCP 5+♣ ve 4'lü ♥ / ♦
            if (hcp >= 17 && c >= 5 && h >= 4) return "2♥";
            if (hcp >= 17 && c >= 5 && d >= 4) return "2♦";
            // 3♣: 6+♣ 15-17
            if (hcp >= 15 && hcp <= 17 && c >= 6 && s < 4) return "3♣";
            // 3♠: 5+♣ 4♠ 14-17 (SEÇİM: 16-17 HCP 4♠ iken ♣ 5 değilse de 3♠)
            if (s >= 4 && ((c >= 5 && hcp >= 14 && hcp <= 17) || (hcp >= 16 && hcp <= 17))) return "3♠";
            // 2♣: 6+♣ 11-16, 5♣4♦ veya 5♣4♥ 14-16
            if (hcp >= 11 && hcp <= 16 && s < 3 && (c >= 6 || (c == 5 && (d >= 4 || h >= 4) && hcp >= 14))) return "2♣";
            // 2♠: 3+♠ 11-15 (15 HCP ise ♠ 3'lü)
            if (s >= 3 && hcp >= 11 && hcp <= 15) return "2♠";
            // 1NT: dengeli 11-14 veya ♠ singleton'lı 1-4-4-4 / 1-4-3-5 / 1-3-4-5
            return "1NT";
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            bool zayif = hcp <= 8, davet = hcp >= 9 && hcp <= 11, oyun = hcp >= 12;
            switch (rebid)
            {
                case "1NT":  // 11-14 dengeli
                    return Sec(new[] {
                        K("3♠", x => oyun && x.S >= 6),                                // 6+♠ SNG FG
                        K("3♦", x => oyun && x.S >= 5 && x.C >= 4 && x.D <= 1),        // 5♠4♣ ♦ SNG
                        K("3♥", x => oyun && x.S >= 5 && x.C >= 4 && x.H <= 1),        // 5♠4♣ ♥ SNG
                        K("3NT", x => oyun && x.Dengeli),
                        K("2♦", x => oyun),                                            // röle FG
                        K("3♣", x => davet && x.S >= 4 && x.C >= 5),                   // 4♠ 5+♣ davet
                        K("2♥", x => x.S >= 6 && x.H >= 4 && x.Hcp >= 6),              // 6♠4♥ S.O.
                        K("2♠", x => x.S >= 5 && x.Hcp >= 6),                          // 5+♠ S.O.
                        K("2NT", x => x.C >= 6 && x.Hcp <= 8),                         // 2NTt
                        K("2♣", x => x.Hcp >= 6),                                      // 2♦'e puppet
                    }, e, "Pas");
                case "2♣":   // 6+♣ 11-16 veya 5♣4♦/4♥ 14-16
                    return Sec(new[] {
                        K("3NT", x => oyun && x.S == 4 && x.Hcp <= 14 && x.Dengeli),   // 4♠ 12-14
                        K("3♠", x => davet && x.S >= 6),
                        K("3♦", x => davet && x.S >= 5 && x.D >= 5),
                        K("3♥", x => davet && x.S >= 5 && x.H >= 5),
                        K("3♣", x => davet && x.C >= 3),                               // davet
                        K("2♥", x => oyun && x.S >= 5 && x.H >= 4),                    // 5+♠ 4♥ FG
                        K("2♦", x => x.Hcp >= 9),                                      // röle davet+
                        K("2♠", x => x.S >= 5 && x.Hcp >= 6),
                        K("2NT", x => x.C >= 6 && x.Hcp <= 8),
                    }, e, "Pas");
                case "2♦":   // 5+♣ 4♦ 17+ FG
                    return Sec(new[] {
                        K("3♠", x => x.S >= 6 && x.Hcp >= 7),
                        K("3♥", x => x.S >= 5 && x.H >= 5 && x.Hcp >= 7),
                        K("3NT", x => x.Hcp >= 7 && x.Hcp <= 10 && x.S >= 4 && x.Stopper("Kupa")),
                        K("3♣", x => x.Hcp >= 8 && x.C >= 3),
                        K("3♦", x => x.Hcp >= 8 && x.D >= 4),
                        K("2♥", x => x.Hcp >= 8),                                      // 4. renk FG
                        K("2♠", x => x.S >= 5),
                    }, e, "2NT");
                case "2♥":   // 5+♣ 4♥ 17+ FG
                    return Sec(new[] {
                        K("3♠", x => x.S >= 6 && x.Hcp >= 7),
                        K("3♥", x => x.S >= 5 && x.H >= 4 && x.Hcp >= 7),
                        K("3NT", x => x.Hcp >= 7 && x.Hcp <= 10 && x.S >= 4 && x.Stopper("Karo")),
                        K("3♣", x => x.Hcp >= 8 && x.C >= 3),
                        K("3♦", x => x.Hcp >= 8),                                      // 4. renk FG
                        K("2♠", x => x.S >= 5),
                    }, e, "2NT");
                case "2♠":   // 3+♠ 11-15
                    return Sec(new[] {
                        K("4♣", x => oyun && x.S >= 5 && x.C <= 1),
                        K("4♦", x => oyun && x.S >= 5 && x.D <= 1),
                        K("4♥", x => oyun && x.S >= 5 && x.H <= 1),
                        K("3♦", x => oyun && x.S >= 5 && x.D >= 4),                    // 5♠ 4+♦ SI
                        K("3♥", x => oyun && x.S >= 5 && x.H >= 4),                    // 5♠ 4+♥ SI
                        K("3NT", x => oyun && x.S == 4 && x.Dengeli),                  // COG
                        K("3♣", x => davet && x.S >= 4 && x.C >= 3),                   // 4♠ 3+♣ davet
                        K("2NT", x => oyun || davet),                                  // röle davet+
                    }, e, "Pas");
                case "2NT":  // 18+
                    return Sec(new[] {
                        K("3♣", x => x.Hcp <= 7),
                        K("3♠", x => x.S >= 6),
                        K("3♥", x => x.S >= 4 && x.C >= 3 && x.H <= 1),
                        K("4♦", x => x.S >= 4 && x.C >= 3 && x.D <= 1),
                        K("3NT", x => x.S == 4 && x.Dengeli && x.Hcp <= 9),
                        K("4♣", x => x.Hcp >= 14),
                        K("3♦", x => true),
                    }, e, "3♦");
                case "3♠":   // 5+♣ 4♠ 14-17
                    return e.Hcp >= 13 ? "3NT" : (e.Hcp >= 7 ? "4♠" : "Pas");        // 3NT = röle
                case "3♦":
                case "3♥":
                    return e.Hcp >= 5 ? "4♠" : "Pas";                                  // SEÇİM: gelişim natürel, ♠ uyumu
                case "4♣":
                case "4♦":
                    return e.Hcp >= 4 ? "4♠" : "Pas";
                case "3♣":
                    return Sec(new[] {
                        K("3NT", x => x.Hcp >= 9 && x.Dengeli),
                        K("3♠", x => x.Hcp >= 8 && x.S >= 5),
                    }, e, "Pas");
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (devam == "2NT" && rebid != "2♦" && rebid != "2♥" && rebid != "2♠") return "3♣";   // 2NTt
            // 1♣-1♠-2♣-2♦ (röle, kitap s.18 DETAY): 2♥ 5♣ 4♥/4♦ min-max, 2♠ 6♣+3♠ min, 2NT max, 3♣ 6♣ min,
            // 3♦ 6♣4♦ max, 3♥ 6♣4♥ max, 3♠ 6♣3♠ max
            if (rebid == "2♣" && devam == "2♦")
            {
                bool max = e.Hcp >= 14;
                if (e.C >= 5 && e.C <= 5 && (e.H >= 4 || e.D >= 4)) return "2♥";
                if (e.C >= 6 && max)
                {
                    if (e.D == 4) return "3♦";
                    if (e.H == 4) return "3♥";
                    if (e.S == 3) return "3♠";
                    return "2NT";
                }
                if (e.C >= 6 && e.S == 3) return "2♠";
                return max ? "2NT" : "3♣";
            }
            // 1♣-1♠-2♠-2NT röle: 3♣ = 4♠ SNG'li, 3♦ = 3♠ min, 3♥ = 3♠ max, 3♠ = 4♠ min SNG'siz, 3NT = 4♠ max SNG'siz
            if (rebid == "2♠" && devam == "2NT")
            {
                bool max = e.Hcp >= 14;
                if (e.S >= 4 && e.Singleton) return "3♣";
                if (e.S >= 4) return max ? "3NT" : "3♠";
                return max ? "3♥" : "3♦";
            }
            return null;
        }

        // AŞAMA D: 1♣-1♠-2♠-2NT röle cevabından sonra cevapçı: max cevapta veya 11+ HCP ile 4♠
        internal override bool KararTanimli(string rebid, string devam) { return rebid == "2♠" && devam == "2NT"; }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (rebid == "2♠" && devam == "2NT")
            {
                bool acicMax = roleCevap == "3♥" || roleCevap == "3NT";
                if (acicMax || e.Hcp >= 11) return "4♠";
                return roleCevap == "3♠" ? "Pas" : "3♠";
            }
            return null;
        }
    }
}
