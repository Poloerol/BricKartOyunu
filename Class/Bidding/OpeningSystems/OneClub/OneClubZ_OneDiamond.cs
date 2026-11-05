using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// ZAYIF NT VERSİYONU: 1♣ ⇒ 1♦ (cevapçının 4+♥'ü, transfer) GELİŞİMİ (SAS kitabı s.76-78).
    /// Yalnızca OneClub_AnswerSistemi açık ve zayıf NT (StrongNT=false) iken çalışır.
    /// Açan: 1♥ (3-4♥ dengesiz) | 1♠ (4+♣ 4♠, ♥ en çok 2) | 1NT (dengeli/yarı dengeli 14-17) | 2♣ (6+♣ 11-14 ya da 5♣4♦ 11-16)
    ///       | 2♥ (4♥ dengeli 15-17).
    /// Cevapçı gücü: zayıf 4-8, davet 9-11, oyun 12+ HCP.
    /// Basitleştirmeler: kitaptaki "Gelişmeler natürel" satırlarında ve KK/BK min-max kodlarında sınıf devreye girmez
    /// (genel motor sürdürür); 1♥ ve 1♠ rebidlerinden sonra açanın ikinci turu yalnızca röle cevaplarıyla sınırlıdır.
    /// </summary>
    public class OneClubZ_OneDiamond : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "1♦"; } }
        public override string Aciklama { get { return "Zayıf NT: 1♣-1♦ (4+♥) gelişimi (kitap s.76-78)"; } }
        public override int Oncelik { get { return 40; } }

        private static bool Bal(OcEl e) { return e.Hcp >= 14 && e.Hcp <= 17 && OneClub_Opening.ZayifNTSinekDengeli(e.El); }

        // ───────── AÇANIN YENİDEN TEKLİFİ ─────────
        private static string Rebid(OcEl e)
        {
            int hcp = e.Hcp;
            if (Bal(e) && e.H == 4 && hcp >= 15) return "2♥";
            if (Bal(e)) return "1NT";
            if (e.H == 3 || e.H == 4) return "1♥";
            if (e.C >= 4 && e.S >= 4 && e.H <= 2) return "1♠";
            if ((e.C >= 6 && hcp <= 14) || (e.C >= 5 && e.D >= 4 && hcp <= 16)) return "2♣";
            return null;
        }
        internal override bool AciciRebidTanimli(OcEl e) { return Rebid(e) != null; }
        internal override string AciciRebid(OcEl e) { return Rebid(e) ?? "1NT"; }

        // ───────── CEVAPÇININ DEVAMI (4+♥ gösterilmiş) ─────────
        internal override bool DevamTanimli(string rebid) { return rebid == "1♥" || rebid == "1♠" || rebid == "1NT" || rebid == "2♣" || rebid == "2♥"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            bool zayif = hcp <= 8, davet = hcp >= 9 && hcp <= 11, oyun = hcp >= 12;
            switch (rebid)
            {
                case "1♥":   // 3-4♥ dengesiz (s.76)
                    return Sec(new[] {
                        K("2♦", x => oyun),                                              // röle FG
                        K("2♠", x => davet && x.H >= 6 && x.S >= 5),                     // 6♥5♠ davet
                        K("3♣", x => davet && x.H >= 5 && x.C >= 5),                     // 5♥5♣ davet
                        K("3♦", x => davet && x.H >= 5 && x.D >= 5),                     // 5♥5♦ davet
                        K("1♠", x => hcp >= 6 && x.H >= 4 && x.S >= 4),                  // F1R
                        K("2♣", x => davet),                                             // davet rölesi
                        K("2♥", x => zayif && x.H >= 5),                                 // S.O
                        K("2NT", x => zayif && x.C >= 6),                                // 2NTt (3♣ transfer)
                        K("1NT", x => zayif && x.H >= 4 && x.Dengeli),                   // natürel dengeli
                    }, e, "Pas");
                case "1♠":   // 4+♣ 4♠ 11-17, ♥ en çok 2 (s.76)
                    return Sec(new[] {
                        K("4♣", x => oyun && x.H >= 4 && x.S >= 4 && x.C <= 1),         // SNG ♣ SI
                        K("4♦", x => oyun && x.H >= 4 && x.S >= 4 && x.D <= 1),         // SNG ♦ SI
                        K("3NT", x => hcp >= 13 && hcp <= 15 && x.H == 4 && x.Dengeli),  // 4♥ 13-15
                        K("2♦", x => oyun && x.H >= 5 && x.C >= 4),                      // 4. renk FG
                        K("2♣", x => oyun),                                              // röle
                        K("3♠", x => davet && x.H >= 4 && x.S >= 4),                     // davet
                        K("3♥", x => davet && x.H >= 6),
                        K("3♣", x => davet && x.H >= 5 && x.C >= 5),
                        K("3♦", x => davet && x.H >= 5 && x.D >= 5),
                        K("1NT", x => davet),                                            // natürel (4+♥)
                        K("2♠", x => hcp >= 5 && x.H >= 4 && x.S >= 4),                  // 5-9 HCP
                        K("2♥", x => zayif && x.H >= 5),                                 // S.O
                        K("2NT", x => zayif && x.C >= 6),
                        K("1NT", x => zayif && x.H >= 4),
                    }, e, "Pas");
                case "1NT":  // dengeli (14)15-17 (s.77)
                    return Sec(new[] {
                        K("3♠", x => oyun && x.H == 4 && x.D == 4 && x.C == 4 && x.S == 1),   // 4♥4♦4♣1♠ FG
                        K("2♣", x => oyun && x.H >= 5),                                  // puppet to 2♦ (5+♥ FG)
                        K("2♠", x => oyun && x.H == 4 && (x.D >= 5 || x.C >= 5)),        // puppet to 2NT (4♥ 5m FG)
                        K("3NT", x => oyun && x.H == 4),                                 // natürel
                        K("3♣", x => davet && x.H == 4 && x.C >= 5),                     // 4♥ 5+♣ davet
                        K("3♦", x => davet && x.H == 4 && x.D >= 5),                     // 4♥ 5+♦ davet
                        K("2♥", x => davet && x.H == 4 && x.S == 4),                     // 4♥4♠ davet+
                        K("2NT", x => davet && x.H == 4),                                // davet
                        K("2♦", x => x.H >= 5),                                          // 2♥'e transfer
                    }, e, "Pas");
                case "2♣":   // 6+♣ 11-14 / 5♣4♦ 11-16 (s.78)
                    return Sec(new[] {
                        K("3♠", x => oyun && x.H >= 5 && x.C >= 3 && x.S <= 1),          // 5♥ 3+♣ SNG ♠ SI
                        K("4♦", x => oyun && x.H >= 5 && x.C >= 3 && x.D <= 1),          // SNG ♦ SI
                        K("2♠", x => oyun && x.H >= 5 && x.S >= 4),                      // 5+♥ 4♠ FG
                        K("3NT", x => oyun && x.Dengeli && x.C <= 2),                    // natürel
                        K("2♦", x => oyun),                                              // röle
                        K("3♥", x => davet && x.H >= 6),                                 // 6+♥ davet
                        K("3♦", x => davet && x.H >= 5 && x.D >= 5),                     // 5♥5♦ davet
                        K("3♣", x => davet && x.C >= 3),                                 // 3+♣ davet
                        K("2♥", x => zayif && x.H >= 6),                                 // S.O
                        K("2NT", x => zayif && x.C >= 6 && x.H < 6),                     // 2NTt
                    }, e, "Pas");
                case "2♥":   // 4♥ dengeli 15-17 (s.78)
                    return Sec(new[] {
                        K("3♠", x => oyun && x.H >= 4 && x.S <= 1),                      // 4+♥ SNG ♠ SI
                        K("4♣", x => oyun && x.H >= 4 && x.C <= 1),
                        K("4♦", x => oyun && x.H >= 4 && x.D <= 1),
                        K("2NT", x => oyun && x.H >= 5 && x.S >= 4),                     // 5+♥ 4♠ FG+
                        K("3♣", x => oyun && x.H >= 5 && x.C >= 4),
                        K("3♦", x => oyun && x.H >= 5 && x.D >= 4),
                        K("2♠", x => oyun),                                              // röle (dağılım sorar)
                    }, e, "Pas");
                default:
                    return "Pas";
            }
        }

        // ───────── AÇANIN RÖLE CEVAPLARI ─────────
        private static string RelayCevap(OcEl e)
        {
            // 1♥-2♣/2♦ röle sonrası: açanın 3♥'li dağılım tanımı (s.76 detay)
            int hcp = e.Hcp;
            if (e.C == 5 && e.H == 4 && e.D == 2 && e.S == 2 && hcp >= 14 && hcp <= 15) return "3♥";
            if (e.H == 4) return "2♥";
            if (e.C >= 6 && e.H == 3 && hcp >= 14) return "3♣";
            if (e.C == 5 && e.S == 4 && e.H == 3 && e.D == 1 && hcp >= 14) return "2♠";
            if (e.C == 5 && e.D == 4 && e.H == 3 && e.S == 1 && hcp >= 14) return "2NT";
            if (e.C >= 5 && e.H == 3) return "2♦";
            return null;
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (devam == "2NT" && (rebid == "1♥" || rebid == "1♠" || rebid == "2♣")) return "3♣";   // 2NTt → 3♣
            if (rebid == "1♥" && (devam == "2♣" || devam == "2♦")) return RelayCevap(e);
            if (rebid == "1♠" && devam == "2♣")                                                       // s.77 detay
            {
                int hcp = e.Hcp;
                if (e.C >= 6 && e.S == 4 && e.H == 1 && hcp >= 14) return "3♣";
                if (e.C >= 5 && e.S == 4 && e.H == 1 && hcp >= 15) return "2NT";
                if (e.C >= 5 && e.S == 4 && e.H == 1 && !e.Stopper("Karo")) return "2♠";
                if (e.C >= 5 && e.S == 4 && e.H == 1 && hcp <= 14) return "2♥";
                return "2♦";                                                                          // min ya da max
            }
            if (rebid == "1NT")
            {
                if (devam == "2♣") return "2♦";                                                       // puppet
                if (devam == "2♦") return "2♥";                                                       // transfer
                if (devam == "2♠") return "2NT";                                                      // puppet
                return null;
            }
            if (rebid == "2♣" && devam == "2♦")                                                       // s.78-79 detay
            {
                if (e.C >= 6 && e.D == 4 && e.Hcp >= 14) return "3♦";
                if (e.C >= 5 && e.D >= 4 && e.C <= 5) return "2♠";
                if (e.C >= 6) return e.Hcp >= 13 ? "2NT" : "3♣";
                return "2♥";
            }
            return null;
        }

        // ───────── CEVAPÇININ KARARI ─────────
        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            int hcp = e.Hcp;
            if (devam == "2NT" && roleCevap == "3♣") return "Pas";                    // 2NTt tamam: ♣ oynanır
            if (rebid == "1NT" && devam == "2♦" && roleCevap == "2♥") return hcp <= 8 ? "Pas" : null;   // transfer tamam
            if (rebid == "1NT" && devam == "2♣" && roleCevap == "2♦")                  // puppet: 5+♥ FG, tanım
                return Sec(new[] {
                    K("3♦", x => (x.H >= 6 && x.D >= 4) || (x.H >= 5 && x.D >= 5)),
                    K("2NT", x => x.H >= 6),
                    K("3♣", x => x.H == 5 && x.D == 4),
                    K("2♠", x => x.H >= 5 && x.C >= 4),
                    K("2♥", x => x.H >= 5 && x.S >= 4),
                }, e, null);
            if (rebid == "1NT" && devam == "2♠" && roleCevap == "2NT")                 // puppet 2NT: 4♥ 5m FG
                return e.C >= e.D ? "3♣" : "3♦";
            if (rebid == "1♠" && devam == "2♣" && roleCevap == "2♦")
                return Sec(new[] {
                    K("3♠", x => x.H >= 4 && x.S >= 4 && hcp >= 12),                  // 4+♥ 4♠ SI
                    K("3♥", x => x.H >= 6),                                           // 6+♥ SI
                    K("3♣", x => hcp >= 9 && x.H >= 4 && x.C >= 4),                   // 4+♥ 4+♣ davet
                    K("2♠", x => hcp >= 10 && hcp <= 11 && x.H == 4 && x.S == 4 && x.Dengeli),   // 4♥4♠ 10-11 dengeli
                    K("2NT", x => hcp >= 9 && hcp <= 11),                             // natürel
                }, e, "2♥");                                                          // röle
            if (rebid == "2♣" && devam == "2♦") return null;                          // gelişme natürel
            return null;
        }

        internal override bool KararTanimli(string rebid, string devam)
        {
            return (rebid == "1NT" && (devam == "2♣" || devam == "2♠" || devam == "2♦")) || devam == "2NT";
        }

        internal override string AciciSon(OcEl e, string rebid, string devam, string roleCevap, string karar)
        {
            if (rebid == "1NT" && devam == "2♣" && roleCevap == "2♦")                  // SAS: sıradaki alt basamak
            {
                if (karar == "2♥") return "2♠";
                if (karar == "2♠") return "2NT";
                if (karar == "2NT") return "3♣";
                if (karar == "3♣") return "3♦";
                if (karar == "3♦") return "3♥";
            }
            if (rebid == "1♠" && devam == "2♣" && roleCevap == "2♦" && karar == "2♥")  // s.77: 2♠ min, 2NT max, 3♣ 6♣4♠2♥
            {
                if (e.C >= 6 && e.S == 4 && e.H == 2) return "3♣";
                return e.Hcp <= 13 ? "2♠" : "2NT";
            }
            return null;
        }
    }
}
