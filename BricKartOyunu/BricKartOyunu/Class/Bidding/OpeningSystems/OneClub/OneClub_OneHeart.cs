using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 1♥ GELİŞİMİ (SAS kitabı s.15-17).
    ///   Aşama A: açanın yeniden teklifi (1♠, 1NT, 2♣, 2♦, 2♥, 2NT, 3♣, 3♦, 3♥, 3♠, 3NT, 4♣, 4♦, 4♥)
    ///   Aşama B: cevapçının devamı (kitaptaki "CEVAPÇI" sütunu)
    ///   Aşama C: açanın 2NTt (transfer 3♣) ve 2♥-2♠ röle cevapları
    /// Kitap her satırda HCP aralığı vermediği için cevapçı gücü şöyle okunur:
    /// zayıf 4-8, davet 9-11, oyun 12+ HCP (kodda "SEÇİM" ile işaretli yerler kitapta açık değildir).
    /// </summary>
    public class OneClub_OneHeart : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "1♥"; } }
        public override string Aciklama { get { return "1♣-1♥ gelişimi"; } }

        internal override string AciciRebid(OcEl e)
        {
            var el = e.El; int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C; bool dengeli = e.Dengeli;

            // 1) 18+ HCP: 4-4-4-1 üç renkli eller
            if (hcp >= 18 && c >= 4 && h >= 4 && s >= 4 && d == 1) return "3♦";
            if (hcp >= 18 && c >= 4 && h >= 4 && d >= 4 && s == 1) return "3♠";

            // 2) 3NT: 5♣ 4♥ 2-2, 18-20
            if (hcp >= 18 && hcp <= 20 && c == 5 && h == 4 && s == 2 && d == 2) return "3NT";

            // 3) 6♣ 4♥ 14-16 sıçramalar (♣'da onur şartı)
            if (hcp >= 14 && hcp <= 16 && c >= 6 && h >= 4)
            {
                bool as_ = el.Any(k => k.Suit == "Sinek" && k.Value == 14);
                bool kral = ElDegerlendirici.PapazVar(el, "Sinek");
                bool asVeKral = as_ && kral;
                bool asVeyaKral = as_ || kral;
                if (asVeKral && d == 1) return "4♣";   // küçük kısa (♦ singleton)
                if (asVeKral && s == 1) return "4♦";   // büyük kısa (♠ singleton)
                if (asVeyaKral) return "4♥";
            }

            // 4) 18+ HCP: 2NT (6+♣ veya 5+♣ 4♥), 2♠ (5+♣ 4♠)
            if (hcp >= 18 && c >= 5 && s >= 4 && h < 4) return "2♠";
            if (hcp >= 18 && (c >= 6 || (c >= 5 && h >= 4))) return "2NT";

            // 5) 3♣: 6+♣ 15-17 (4'lü ♥ yoksa)
            if (hcp >= 15 && hcp <= 17 && c >= 6 && h < 4) return "3♣";

            // 6) 2♦: 5+♣ 4♦ 17+
            if (hcp >= 17 && c >= 5 && d >= 4) return "2♦";

            // 7) 3♥: 4+♣ 4♥ 14-16 (SEÇİM: ♥ desteği 4+ iken ♠ göstermeden önce gelir)
            if (hcp >= 14 && hcp <= 16 && c >= 4 && h >= 4) return "3♥";

            // 8) 2♣: 6+♣ veya 5♣4♦, 11-16 (SEÇİM: 6+♣ varken 3'lü ♥ desteğinden önce gelir)
            if (hcp >= 11 && hcp <= 16 && (c >= 6 || (c == 5 && d >= 4))) return "2♣";

            // 9) 2♥: 3+♥ 11-15 (4'lü ♥ varsa ♠ göstermeden destek verilir)
            if (hcp >= 11 && hcp <= 15 && (h >= 4 || (h == 3 && s < 4))) return "2♥";

            // 10) 1♠: 4+♣ 4♠ 11-17
            if (hcp >= 11 && hcp <= 17 && c >= 4 && s >= 4) return "1♠";

            // 11) 1NT: dengeli 11-14
            if (hcp >= 11 && hcp <= 14 && dengeli) return "1NT";

            // 12) Tabloya girmeyenler: ♥ desteği varsa 2♥, yoksa 1NT
            if (h >= 3) return "2♥";
            return "1NT";
        }

        // ─────────────────────────────────────────────────────────────
        // AŞAMA B: CEVAPÇININ DEVAMI (cevapçı 4+♥ 4+ HCP göstermişti)
        // ─────────────────────────────────────────────────────────────
        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            bool zayif = hcp <= 8, davet = hcp >= 9 && hcp <= 11, oyun = hcp >= 12;

            switch (rebid)
            {
                case "1♠":   // 4+♣ 4♠ 11-17
                    return Sec(new[] {
                        K("2♦", x => x.Hcp >= 12 && x.H >= 5 && x.C >= 4),            // 4. renk, 5♥ 4+♣ FG
                        K("4♣", x => x.S >= 4 && x.Hcp >= 13 && x.C <= 1),             // 4♠ uyumu, ♣ singleton
                        K("4♦", x => x.S >= 4 && x.Hcp >= 13 && x.D <= 1),             // 4♠ uyumu, ♦ singleton
                        K("3♠", x => x.S >= 4 && x.Hcp >= 9),                          // 4♠ davet
                        K("2♠", x => x.S >= 4),                                        // 4♠ zayıf
                        K("3♥", x => davet && x.H >= 6),                               // 6+♥ davet
                        K("3♦", x => davet && x.H >= 5 && x.D >= 5),                   // 5♥5♦ davet
                        K("3♣", x => davet && x.H >= 4 && x.C >= 4),                   // 4♥ 4/5♣ davet
                        K("2NT", x => zayif && x.C >= 6),                              // 2NTt: ♣'da oynamak
                        K("2♥", x => zayif && x.H >= 5),                               // 5+♥ S.O.
                        K("3NT", x => oyun && x.Dengeli),
                        K("2♣", x => oyun),                                            // röle
                        K("1NT", x => x.Hcp >= 6),
                    }, e, "Pas");

                case "1NT":  // dengeli 11-14 (♠ singleton da olabilir)
                    return Sec(new[] {
                        K("3♥", x => oyun && x.H >= 6),                                // 6+♥ SNG FG
                        K("3♦", x => oyun && x.H >= 5 && x.C >= 4 && x.D <= 1),        // 5♥4♣ ♦ SNG FG
                        K("3♠", x => oyun && x.H >= 5 && x.C >= 4 && x.S <= 1),        // 5♥4♣ ♠ SNG FG
                        K("3NT", x => oyun && x.Dengeli),
                        K("2♦", x => oyun),                                            // röle FG
                        K("2♠", x => davet && x.H >= 4 && x.S >= 4),                   // 4♥4♠ davet
                        K("3♣", x => davet && x.H >= 4 && x.C >= 4),                   // 4♥ 4/5♣ davet
                        K("2♥", x => x.H >= 5 && x.Hcp >= 6),                          // 5+♥ S.O.
                        K("2NT", x => x.C >= 6 && x.Hcp <= 8),                         // 2NTt
                        K("2♣", x => x.Hcp >= 6),                                      // 2♦'e puppet
                    }, e, "Pas");

                case "2♣":   // 6+♣ 11-16 veya 5♣4♦/4♥ 14-16
                    return Sec(new[] {
                        K("4♣", x => x.Hcp >= 17 && x.C >= 3),                         // RKCB ♣
                        K("4♦", x => x.Hcp >= 14 && x.C >= 3 && x.D <= 1),             // ♦ SNG SI
                        K("3♠", x => x.Hcp >= 14 && x.C >= 3 && x.S <= 1),             // ♠ SNG SI
                        K("3NT", x => oyun && x.Dengeli && x.C < 3),
                        K("2♠", x => oyun && x.H >= 5 && x.S >= 4),                    // 5+♥ 4♠ FG
                        K("3♥", x => davet && x.H >= 6),
                        K("3♦", x => davet && x.H >= 5 && x.D >= 5),
                        K("3♣", x => davet && x.C >= 3),                               // 3+♣ davet
                        K("2♦", x => x.Hcp >= 9),                                      // röle davet+
                        K("2♥", x => x.H >= 5 && x.Hcp >= 6),
                        K("2NT", x => x.C >= 6 && x.Hcp <= 8),
                    }, e, "Pas");

                case "2♦":   // 5+♣ 4♦ 17+ (FG)
                    return Sec(new[] {
                        K("3♠", x => x.Hcp >= 11 && x.H >= 6 && x.S >= 5),             // 6♥5♠ 11+
                        K("3♥", x => x.H >= 6 && x.Hcp >= 7),                          // 6+♥ FG
                        K("3NT", x => x.Hcp >= 7 && x.Hcp <= 10 && x.H >= 4 && x.Stopper("Maça")),
                        K("3♣", x => x.Hcp >= 8 && x.C >= 3),                          // 3+♣ FG
                        K("3♦", x => x.Hcp >= 8 && x.D >= 4),                          // 4+♦ FG
                        K("2♠", x => x.Hcp >= 8),                                      // 4. renk FG
                        K("2♥", x => x.H >= 5),                                        // 5+♥ S.O.
                        K("2NT", x => true),                                           // Lebensohl (transfer 3♣)
                    }, e, "2NT");

                case "2♥":   // 3+♥ 11-15
                    return Sec(new[] {
                        K("4♣", x => oyun && x.H >= 5 && x.C <= 1),                    // ♣ SNG SI
                        K("4♦", x => oyun && x.H >= 5 && x.D <= 1),                    // ♦ SNG SI
                        K("3♠", x => oyun && x.H >= 5 && x.S <= 1),                    // ♠ SNG SI
                        K("2NT", x => oyun && x.H >= 5 && x.S >= 4),                   // 5+♥ 4♠ SI
                        K("3♦", x => oyun && x.H >= 5 && x.D >= 4),                    // 5♥ 4+♦ SI
                        K("3NT", x => oyun && x.H == 4 && x.Dengeli),                  // 4♥-3-3-3 COG
                        K("2♠", x => oyun || davet),                                   // röle davet+
                    }, e, "Pas");

                case "2NT":  // 18+ 6+♣ veya 5+♣ 4♥
                    return Sec(new[] {
                        K("3♣", x => x.Hcp <= 7),                                      // min
                        K("3♥", x => x.H >= 6),                                        // 6+♥ FG
                        K("3♠", x => x.H >= 4 && x.C >= 3 && x.S <= 1),                // ♠ SNG SI
                        K("4♦", x => x.H >= 4 && x.C >= 3 && x.D <= 1),                // ♦ SNG SI
                        K("3NT", x => x.H == 4 && x.Dengeli && x.Hcp <= 9),            // 4♥-3-3-3 min COG
                        K("4♣", x => x.Hcp >= 14),                                     // RKCB ♣
                        K("3♦", x => true),                                            // röle FG
                    }, e, "3♦");

                case "3♥":   // 4+♣ 4♥ 14-16
                    return Sec(new[] {
                        K("3♠", x => x.Hcp >= 13),                                     // röle (max / KK / BK / min)
                        K("4♥", x => x.Hcp >= 7),
                    }, e, "Pas");

                case "3♦":   // 4♣4♠4♥ 1♦ 18+
                case "3♠":   // 4♣4♦4♥ 1♠ 18+
                    return e.Hcp >= 5 ? "4♥" : "Pas";   // SEÇİM: kitap "gelişim natürel" der, ♥ uyumu varsa oyun

                case "4♣":
                case "4♦":
                    return e.Hcp >= 4 ? "4♥" : "Pas";   // SEÇİM: 6♣4♥ kısa gösterisi, gelişim natürel
                default:     // 3♣, 3NT, 4♥ ... gelişim natürel
                    if (rebid == "3♣") return Sec(new[] {
                        K("3NT", x => x.Hcp >= 9 && x.Dengeli),
                        K("3♥", x => x.Hcp >= 8 && x.H >= 5),
                    }, e, "Pas");
                    return "Pas";
            }
        }

        // ─────────────────────────────────────────────────────────────
        // AŞAMA C: AÇANIN RÖLE CEVABI
        // ─────────────────────────────────────────────────────────────
        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            // 2NTt: açan 3♣ der (cevapçı Pas veya düzeltir)
            if (devam == "2NT" && rebid != "2♦" && rebid != "2♥") return "3♣";

            // 1♣-1♥-2♣-2♦ (röle, kitap s.15-16 DETAY): 2♥ 6♣+3♥ min/max, 2♠ 5+♣4♦ min/max, 2NT 6♣ max (3♥ yok),
            // 3♣ 6♣ min (3♥ yok), 3♦ 6♣4♦ max
            if (rebid == "2♣" && devam == "2♦")
            {
                bool max = e.Hcp >= 14;
                if (e.C >= 6 && e.D == 4 && max) return "3♦";
                if (e.C >= 6 && e.H >= 3) return "2♥";
                if (e.C >= 5 && e.D >= 4) return "2♠";
                return max ? "2NT" : "3♣";
            }
            // 1♣-1♥-2NT-3♦ (röle FG, DETAY): 3♥ 6+♣ 3♥, 3♠ 5♣4♥ ♠ SNG, 3NT 6+♣ (3♥ yok), 4♣ 6♣4♥, 4♦ 5♣4♥ ♦ SNG
            if (rebid == "2NT" && devam == "3♦")
            {
                if (e.C >= 6 && e.H >= 4) return "4♣";
                if (e.C >= 6 && e.H == 3) return "3♥";
                if (e.C >= 6) return "3NT";
                if (e.H >= 4 && e.S <= 1) return "3♠";
                if (e.H >= 4 && e.D <= 1) return "4♦";
                return "3NT";
            }
            // 1♣-1♥-2♥-2♠ (röle): 2NT = 4♥ SNG'li, 3♣ = 3♥ min, 3♦ = 3♥ max, 3♥ = 4♥ min SNG'siz, 3♠ = 4♥ max SNG'siz
            if (rebid == "2♥" && devam == "2♠")
            {
                bool max = e.Hcp >= 14;
                if (e.H >= 4 && e.Singleton) return "2NT";
                if (e.H >= 4) return max ? "3♠" : "3♥";
                return max ? "3♦" : "3♣";
            }
            return null;
        }

        // AŞAMA D: 1♣-1♥-2♥-2♠ röle cevabından sonra cevapçı (9+ HCP, ♥ uyumu): max cevapta veya 10+ HCP ile 4♥
        internal override bool KararTanimli(string rebid, string devam) { return rebid == "2♥" && devam == "2♠"; }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (rebid == "2♥" && devam == "2♠")
            {
                bool acicMax = roleCevap == "3♦" || roleCevap == "3♠";
                if (acicMax || e.Hcp >= 11) return "4♥";
                return roleCevap == "3♥" ? "Pas" : "3♥";
            }
            return null;
        }
    }
}
