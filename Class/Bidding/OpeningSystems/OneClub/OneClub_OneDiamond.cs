using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 1♦ GELİŞİMİ (SAS kitabı s.11-14): açanın yeniden teklifi, cevapçının devamı ve röle cevapları.
    /// 1♥ yeniden teklifinin ayrıntıları (1♠ / 2♣ röle devamları) OneClub_OneDiamond_OneHeart içindedir.
    /// Cevapçı gücü: zayıf 4-8, davet 9-11, oyun 12+ HCP (kitapta açık olmayan yerler "SEÇİM").
    /// </summary>
    public class OneClub_OneDiamond : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "1♦"; } }
        public override string Aciklama { get { return "1♣-1♦ gelişimi (kitap s.11-14)"; } }
        public override int Oncelik { get { return 40; } }

        /// <summary>Kesin dengeli dağılım: 4-3-3-3, 4-4-3-2, 5-3-3-2.</summary>
        internal static bool Duz(OcEl e)
        {
            var l = new[] { e.S, e.H, e.D, e.C }.OrderByDescending(x => x).ToArray();
            return (l[0] == 4 && l[1] == 3 && l[2] == 3) || (l[0] == 4 && l[1] == 4 && l[2] == 3) || (l[0] == 5 && l[1] == 3 && l[2] == 3 && l[3] == 2);
        }

        // ───────── AÇANIN YENİDEN TEKLİFİ ─────────
        internal override string AciciRebid(OcEl e)
        {
            int hcp = e.Hcp, s = e.S, h = e.H, d = e.D, c = e.C;
            // 18-21 HCP 4-4-4-1: tekli rengi gösterir (3♥: ♥ tek, 3♠: ♠ tek, 3NT: ♦ tek)
            if (hcp >= 18 && c == 4 && d == 4 && s == 4 && h == 1) return "3♥";
            if (hcp >= 18 && c == 4 && d == 4 && h == 4 && s == 1) return "3♠";
            if (hcp >= 18 && c == 4 && h == 4 && s == 4 && d == 1) return "3NT";
            // 18+ HCP: 5+♣ 4♥ → 2♥, 5+♣ 4♠ → 2♠, 6+♣ veya 5♣4♦ → 2NT
            if (hcp >= 18 && c >= 5 && h >= 4) return "2♥";
            if (hcp >= 18 && c >= 5 && s >= 4) return "2♠";
            if (hcp >= 18 && (c >= 6 || (c >= 5 && d >= 4))) return "2NT";
            // 11-14 dengeli (4'lü majör olabilir): 1NT
            if (Duz(e) && hcp <= 14) return "1NT";
            // 4+♣ ve 4'lü majör: 1♥ / 1♠ (11-17)
            if (c >= 4 && h >= 4) return "1♥";
            if (c >= 4 && s >= 4) return "1♠";
            // 15-17 HCP: 6+♣ → 3♣, 5+♣4♦ → 3♦
            if (hcp >= 15 && c >= 6) return "3♣";
            if (hcp >= 15 && c >= 5 && d >= 4) return "3♦";
            // 11-14: 4+♣4♦ → 2♦ (SEÇİM: 15-17 HCP 4♣4♦ de 2♦), 6+♣ → 2♣
            if (c >= 4 && d >= 4) return "2♦";
            if (c >= 6) return "2♣";
            if (hcp >= 18) return "2NT";     // SEÇİM: kitapta tanımsız güçlü dengeli el
            return "1NT";
        }

        // ───────── CEVAPÇININ DEVAMI ─────────
        internal override bool DevamTanimli(string rebid) { return !(rebid == "3♣" || rebid == "3♦" || rebid == "3♥" || rebid == "3♠"); }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            bool zayif = hcp <= 8, davet = hcp >= 9 && hcp <= 11, oyun = hcp >= 12;
            switch (rebid)
            {
                case "1♥":   // 4+♣ 4♥ 11-17 (s.11)
                    return Sec(new[] {
                        K("3NT", x => hcp >= 12 && hcp <= 14 && x.D == 4 && x.S == 3 && x.H == 3 && x.C == 3),   // 4♦-3-3-3
                        K("4♣", x => oyun && x.D >= 4 && x.H >= 4 && x.C <= 1),        // 4♥ SNG ♣ hafif SI
                        K("3♠", x => oyun && x.D >= 4 && x.H >= 4 && x.S <= 1),        // 4♥ SNG ♠ hafif SI
                        K("2♠", x => oyun && x.D >= 5 && x.C >= 4),                    // 4. renk 5+♦ 4+♣ SI (SAS)
                        K("3♥", x => davet && x.D >= 4 && x.H >= 4 && x.Singleton),    // davet w SNG
                        K("2♥", x => davet && x.D >= 4 && x.H >= 4),                   // davet w/o SNG
                        K("1♠", x => hcp >= 9 && x.D >= 4 && x.S >= 4),                // doğal F1R
                        K("3♦", x => hcp >= 10 && hcp <= 11 && x.D >= 6),              // 6+♦ davet
                        K("2♦", x => hcp >= 5 && hcp <= 9 && x.D >= 6),                // 6+♦ 5-9
                        K("3♣", x => davet && x.D >= 5 && x.C >= 4 && x.C <= 5),       // 5+♦ 4/5♣ davet
                        K("2♣", x => oyun),                                            // röle
                        K("2NT", x => zayif && x.C >= 6),                              // 2NTt (3♣ transfer)
                    }, e, "Pas");
                case "1♠":   // 4+♣ 4♠ 11-17 (s.12)
                    return Sec(new[] {
                        K("3NT", x => hcp >= 12 && hcp <= 14 && x.D == 4 && x.S == 3 && x.H == 3 && x.C == 3),
                        K("4♣", x => oyun && x.D >= 4 && x.S >= 4 && x.C <= 1),        // 4♠ SNG ♣ hafif SI
                        K("3♥", x => oyun && x.D >= 5 && x.C >= 4 && x.H <= 1),        // 5+♦ 4+♣ SNG ♥ hafif SI
                        K("2♥", x => oyun && x.D >= 5 && x.C >= 4),                    // 4. renk 5+♦ 4+♣ SI (SAS)
                        K("3♠", x => davet && x.D >= 4 && x.S >= 4 && x.Singleton),    // davet w SNG
                        K("2♠", x => davet && x.D >= 4 && x.S >= 4),                   // davet w/o SNG
                        K("3♦", x => hcp >= 10 && hcp <= 11 && x.D >= 6),
                        K("2♦", x => hcp >= 5 && hcp <= 9 && x.D >= 6),
                        K("3♣", x => davet && x.D >= 5 && x.C >= 4 && x.C <= 5),
                        K("2♣", x => oyun),
                        K("2NT", x => zayif && x.C >= 6),
                    }, e, "Pas");
                case "1NT":  // 11-14 dengeli, 4'lü majör olabilir (s.12-13)
                    return Sec(new[] {
                        K("3♥", x => oyun && x.D >= 5 && x.S >= 4 && x.H <= 1),        // 5♦ 4♠ SNG ♥
                        K("3♠", x => oyun && x.D >= 5 && x.H >= 4 && x.S <= 1),        // 5♦ 4♥ SNG ♠
                        K("3♦", x => oyun && x.D >= 6),                                // 6+♦ FG
                        K("3NT", x => oyun && x.Dengeli),                              // doğal
                        K("2♦", x => oyun),                                            // FG röle
                        K("2♥", x => davet && x.D >= 4 && x.H >= 4),                   // 4♦ 4♥ davet
                        K("2♠", x => davet && x.D >= 4 && x.S >= 4),                   // 4♦ 4♠ davet
                        K("3♣", x => davet && x.D >= 4 && x.C >= 5),                   // 4♦ 5+♣ davet
                        K("2NT", x => zayif && x.C >= 6),                              // 2NTt
                        K("2♣", x => zayif && x.D >= 5),                               // 2♦'e puppet
                    }, e, "Pas");
                case "2♣":   // 6+♣ 11-16 (s.13)
                    return Sec(new[] {
                        K("4♣", x => hcp >= 17 && x.C >= 3),                           // RKCB ♣ (SEÇİM: 17+)
                        K("3♥", x => oyun && x.C >= 3 && x.H <= 1),                    // 3+♣ SNG ♥ FG
                        K("3♠", x => oyun && x.C >= 3 && x.S <= 1),                    // 3+♣ SNG ♠ FG
                        K("2♠", x => oyun && x.D >= 5 && x.S >= 4),                    // 5+♦ 4♠ FG (açan 2NT = SAS)
                        K("3NT", x => oyun && x.Dengeli),                              // doğal
                        K("3♦", x => davet && x.D >= 6),                               // 6+♦ davet
                        K("3♣", x => davet && x.C >= 3),                               // davet
                        K("2♥", x => oyun),                                            // röle
                        K("2♦", x => zayif && x.D >= 6),                               // S.O
                    }, e, "Pas");
                case "2♦":   // 4+♣ 4♦ 11-14 (s.13)
                    return Sec(new[] {
                        K("3♥", x => oyun && x.D >= 4 && x.H <= 1),                    // 4+♦ SNG ♥ FG
                        K("3♠", x => oyun && x.D >= 4 && x.S <= 1),                    // 4+♦ SNG ♠ FG
                        K("2♠", x => oyun && x.D >= 5 && x.S >= 4),                    // 5+♦ 4♠ FG (açan 2NT = SAS)
                        K("3NT", x => oyun && x.Dengeli && x.D <= 3),                  // doğal
                        K("3♣", x => davet && x.D >= 5 && x.C >= 4),                   // 5♦ 4♣ 9-11
                        K("3♦", x => zayif && x.D >= 7),                               // bomba (blokaj)
                        K("2♥", x => hcp >= 9 && x.D >= 4),                            // röle (SEÇİM: 9+)
                        K("2NT", x => zayif && x.C >= 6),                              // 2NTt
                    }, e, "Pas");
                case "2♥":   // 5+♣ 4♥ 18+ FG (s.13)
                    return Sec(new[] {
                        K("2NT", x => hcp <= 7),                                       // Lebensohl (3♣ transfer)
                        K("4♣", x => x.D >= 4 && x.H >= 4 && x.C <= 1),                // 4♥ SNG ♣
                        K("3♠", x => x.D >= 4 && x.H >= 4 && x.S <= 1),                // 4♥ SNG ♠
                        K("3♥", x => x.D >= 4 && x.H >= 4),                            // 4♥ FG w/o SNG
                        K("3NT", x => hcp <= 10 && x.D >= 4 && x.Stopper("Maça")),     // 8-10 HCP ♠ stoper
                        K("3♦", x => x.D >= 6),                                        // 6+♦ FG
                        K("2♠", x => x.S >= 4),                                        // 4. renk FG
                    }, e, "3♣");                                                       // 3♣ tutuş FG
                case "2♠":   // 5+♣ 4♠ 18+ FG (s.13)
                    return Sec(new[] {
                        K("2NT", x => hcp <= 7),
                        K("4♥", x => x.D >= 4 && x.S >= 4 && x.H <= 1),                // 4♠ SNG ♥
                        K("4♣", x => x.D >= 4 && x.S >= 4 && x.C <= 1),                // 4♠ SNG ♣
                        K("3♠", x => x.D >= 4 && x.S >= 4),                            // 4♠ FG w/o SNG
                        K("3NT", x => hcp <= 10 && x.D >= 4 && x.Stopper("Kupa")),     // 8-10 HCP ♥ stoper
                        K("3♦", x => x.D >= 6),
                        K("3♥", x => x.H >= 4),                                        // 4. renk
                    }, e, "3♣");
                case "2NT":  // 18+ 6+♣ veya 5♣4♦ (s.14)
                    return Sec(new[] {
                        K("3♥", x => hcp >= 8 && x.C >= 3 && x.H <= 1),                // 3+♣ SNG ♥
                        K("3♠", x => hcp >= 8 && x.C >= 3 && x.S <= 1),                // 3+♣ SNG ♠
                        K("3♦", x => hcp >= 8 && x.D >= 6),                            // 6+♦ FG
                        K("3NT", x => hcp <= 9 && x.Dengeli),                          // MIN dengeli
                    }, e, "3♣");                                                       // MIN
                case "3NT":  // 4♣ 4♥ 4♠ 1♦ 18-21
                    return Sec(new[] {
                        K("4♥", x => hcp >= 8 && x.D >= 4 && x.H >= 4),                // SI
                        K("4♠", x => hcp >= 8 && x.D >= 4 && x.S >= 4),
                        K("4NT", x => hcp >= 8 && x.D >= 4 && x.C >= 4),
                        K("4♣", x => x.H >= 5),                                        // 4♥ transfer
                        K("4♦", x => x.S >= 5),                                        // 4♠ transfer
                    }, e, "Pas");
                default:
                    return "Pas";
            }
        }

        // ───────── AÇANIN RÖLE / İKİNCİ YENİDEN TEKLİFİ ─────────
        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid == "1♥") return OneClub_OneDiamond_OneHeart.AciciCevap(e, devam);
            if (rebid == "3NT") { if (devam == "4♣") return "4♥"; if (devam == "4♦") return "4♠"; return null; }
            if (devam == "2NT" && rebid != "2NT" && rebid != "2♥" && rebid != "2♠") return "3♣";   // 2NTt → 3♣
            if ((rebid == "2♥" || rebid == "2♠") && devam == "2NT") return "3♣";                    // Lebensohl → 3♣
            if (rebid == "1NT" && devam == "2♣") return "2♦";                                       // puppet → 2♦
            if (rebid == "1NT" && devam == "2♦") return OneNTRole.AciciCevap(e, '♦', '♣');                 // FG röle: öncelik sırası (s.92)
            bool min;
            if (rebid == "2♣" && devam == "2♥")   // röle: 2♠ min w 3♦ / 2NT max / 3♣ min / 3♦ max w 3♦
            {
                min = e.Hcp <= 13;
                if (e.D >= 3) return min ? "2♠" : "3♦";
                return min ? "3♣" : "2NT";
            }
            if (rebid == "2♦" && devam == "2♥")   // röle: 2♠ 5♣4♦ min / 2NT 4♣4♦ dengeli / 3♣ 5♣4♦ max
            {
                if (e.C >= 5) return e.Hcp <= 12 ? "2♠" : "3♣";
                return "2NT";
            }
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (rebid == "1♥") return OneClub_OneDiamond_OneHeart.CevapciKarar(e, devam, roleCevap);
            if (devam == "2NT" && roleCevap == "3♣" && rebid != "2NT") return "Pas";                // transfer tamam: ♣ oynanır
            if (rebid == "1NT" && devam == "2♦") return OneNTRole.CevapciKarar(e, '♦', '♣', roleCevap);
            if (rebid == "2♦" && devam == "2♥")
            {
                if (roleCevap == "2♠") return e.Hcp >= 12 ? "2NT" : "3♦";                           // SAS / 4+♦ 9-11 S.O
                if (roleCevap == "2NT") return "3♦";                                                // 4+♦ davet
                if (roleCevap == "3♣") return "3♦";                                                 // SAS
            }
            return null;
        }

        internal override string AciciSon(OcEl e, string rebid, string devam, string roleCevap, string karar)
        {
            if (rebid == "1♥") return OneClub_OneDiamond_OneHeart.AciciSon(e, devam, roleCevap, karar);
            return null;
        }
    }
}
