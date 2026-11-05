using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// ZAYIF NT VERSİYONU: 1♣ ⇒ 1♥ (cevapçının 4+♠'ü, transfer) GELİŞİMİ (SAS kitabı s.79-81).
    /// Yalnızca OneClub_AnswerSistemi açık ve zayıf NT (StrongNT=false) iken çalışır; 1♣-1♦ (4+♥) gelişiminin ♠ aynası.
    /// Açan: 1♠ (3-4♠ dengesiz) | 1NT (dengeli/yarı dengeli 14-17) | 2♣ (6+♣) | 2♠ (4♠ dengeli 15-17).
    /// Cevapçı gücü: zayıf 4-8, davet 9-11, oyun 12+ HCP.
    /// Basitleştirmeler: "Gelişmeler natürel" satırlarında ve KK/BK min-max kodlarında sınıf devreye girmez (genel motor sürdürür).
    /// </summary>
    public class OneClubZ_OneHeart : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "1♥"; } }
        public override string Aciklama { get { return "Zayıf NT: 1♣-1♥ (4+♠) gelişimi (kitap s.79-81)"; } }
        public override int Oncelik { get { return 40; } }

        private static bool Bal(OcEl e) { return e.Hcp >= 14 && e.Hcp <= 17 && OneClub_Opening.ZayifNTSinekDengeli(e.El); }

        // ───────── AÇANIN YENİDEN TEKLİFİ ─────────
        private static string Rebid(OcEl e)
        {
            if (Bal(e) && e.S == 4 && e.Hcp >= 15) return "2♠";
            if (Bal(e)) return "1NT";
            if (e.S == 3 || e.S == 4) return "1♠";
            if (e.C >= 6) return "2♣";
            return null;
        }
        internal override bool AciciRebidTanimli(OcEl e) { return Rebid(e) != null; }
        internal override string AciciRebid(OcEl e) { return Rebid(e) ?? "1NT"; }

        // ───────── CEVAPÇININ DEVAMI (4+♠ gösterilmiş) ─────────
        internal override bool DevamTanimli(string rebid) { return rebid == "1♠" || rebid == "1NT" || rebid == "2♣" || rebid == "2♠"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            bool zayif = hcp <= 8, davet = hcp >= 9 && hcp <= 11, oyun = hcp >= 12;
            switch (rebid)
            {
                case "1♠":   // 3-4♠ dengesiz (s.79)
                    return Sec(new[] {
                        K("2♦", x => oyun),                                              // röle FG
                        K("3♥", x => davet && x.S >= 5 && x.H >= 5),                     // 5♠5♥ davet
                        K("3♣", x => davet && x.S >= 5 && x.C >= 5),                     // 5♠5♣ davet
                        K("3♦", x => davet && x.S >= 5 && x.D >= 5),                     // 5♠5♦ davet
                        K("3♠", x => davet && x.S >= 6),                                 // 6+♠ davet
                        K("2♣", x => davet),                                             // davet rölesi
                        K("2♥", x => zayif && x.S >= 6 && x.H >= 4),                     // 6♠4♥ S.O
                        K("2♠", x => zayif && x.S >= 5),                                 // S.O
                        K("2NT", x => zayif && x.C >= 6),                                // 2NTt (3♣ transfer)
                        K("1NT", x => zayif && x.S >= 4 && x.Dengeli),                   // natürel
                    }, e, "Pas");
                case "1NT":  // dengeli (14)15-17 (s.79)
                    return Sec(new[] {
                        K("2♣", x => oyun && x.S >= 5),                                  // puppet to 2♦ (5+♠ FG)
                        K("2♠", x => oyun && x.S == 4 && (x.D >= 5 || x.C >= 5)),        // puppet to 2NT (4♠ 5m FG)
                        K("3NT", x => oyun && x.S == 4),                                 // natürel
                        K("3♥", x => davet && x.S >= 5 && x.H >= 5),                     // 5♠5♥ davet
                        K("3♠", x => davet && x.S >= 6),                                 // 6+♠ davet
                        K("3♣", x => davet && x.S == 4 && x.C >= 5),                     // 4♠ 5+♣ davet
                        K("3♦", x => davet && x.S == 4 && x.D >= 5),                     // 4♠ 5+♦ davet
                        K("2NT", x => davet && x.S == 4),                                // davet
                        K("2♦", x => x.S >= 5 && x.H >= 4),                              // 2♥'e transfer (5+♠ 4♥ S.O / davet)
                        K("2♥", x => x.S >= 5),                                          // 2♠'e transfer
                    }, e, "Pas");
                case "2♣":   // 6+♣ (s.80)
                    return Sec(new[] {
                        K("4♦", x => oyun && x.S >= 5 && x.C >= 3 && x.D <= 1),          // 5♠ 4♣ SNG ♦ SI
                        K("4♥", x => oyun && x.S >= 5 && x.C >= 3 && x.H <= 1),          // SNG ♥ SI
                        K("2♥", x => oyun && x.S >= 5 && x.H >= 4),                      // 5+♠ 4♥ FG
                        K("3NT", x => oyun && x.Dengeli && x.C <= 2),                    // natürel
                        K("2♦", x => oyun),                                              // röle
                        K("3♠", x => davet && x.S >= 6),                                 // 6+♠ davet
                        K("3♥", x => davet && x.S >= 5 && x.H >= 5),                     // 5♠5♥ davet
                        K("3♦", x => davet && x.S >= 5 && x.D >= 5),                     // 5♠5♦ davet
                        K("3♣", x => davet && x.C >= 3),                                 // 3+♣ davet
                        K("2♠", x => zayif && x.S >= 5),                                 // S.O
                        K("2NT", x => zayif && x.C >= 6 && x.S < 5),                     // 2NTt
                    }, e, "Pas");
                case "2♠":   // 4♠ dengeli 15-17 (s.80)
                    return Sec(new[] {
                        K("4♣", x => oyun && x.S >= 4 && x.C <= 1),                      // 4+♠ SNG ♣ SI
                        K("4♦", x => oyun && x.S >= 4 && x.D <= 1),
                        K("4♥", x => oyun && x.S >= 4 && x.H <= 1),
                        K("3♣", x => oyun && x.S >= 5 && x.C >= 4),                      // 5+♠ 4♣ FG+
                        K("3♦", x => oyun && x.S >= 5 && x.D >= 4),
                        K("3♥", x => oyun && x.S >= 5 && x.H >= 4),
                        K("2NT", x => oyun),                                             // röle
                    }, e, "Pas");
                default:
                    return "Pas";
            }
        }

        // ───────── AÇANIN RÖLE CEVAPLARI ─────────
        private static string RelayCevap(OcEl e)
        {
            int hcp = e.Hcp;
            if (e.C == 5 && e.S == 4 && e.H == 2 && e.D == 2 && hcp >= 14 && hcp <= 15) return "3♠";
            if (e.S == 4) return "2♠";                                                                 // 4+♣ 4♠ dengesiz
            if (e.C >= 6 && e.S == 3 && hcp >= 14) return "3♣";
            if (e.C == 5 && e.H == 4 && e.S == 3 && e.D == 1 && hcp >= 14) return "2♥";
            if (e.C == 5 && e.D == 4 && e.S == 3 && e.H == 1 && hcp >= 14) return "2NT";
            if (e.C >= 5 && e.S == 3) return "2♦";
            return null;
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (devam == "2NT" && (rebid == "1♠" || rebid == "2♣")) return "3♣";                       // 2NTt → 3♣
            if (rebid == "1♠" && (devam == "2♣" || devam == "2♦")) return RelayCevap(e);
            if (rebid == "1NT")
            {
                if (devam == "2♣") return "2♦";                                                       // puppet
                if (devam == "2♦") return "2♥";                                                       // transfer (♥)
                if (devam == "2♥") return "2♠";                                                       // transfer (♠)
                if (devam == "2♠") return "2NT";                                                      // puppet
                return null;
            }
            if (rebid == "2♣" && devam == "2♦")                                                       // s.80 detay
            {
                if (e.C >= 6 && e.H == 4 && e.Hcp >= 14) return "3♥";
                if (e.C >= 6 && e.D == 4 && e.Hcp >= 14) return "3♦";
                if (e.C == 5 && e.H >= 4) return "2♥";
                if (e.C == 5 && e.D >= 4) return "2♠";
                if (e.C >= 6) return e.Hcp >= 13 ? "2NT" : "3♣";
                return "2♥";
            }
            if (rebid == "2♠" && devam == "2NT")                                                      // s.80-81 detay
            {
                if (e.Hcp <= 15) return "3♠";                                                         // min
                if (e.Dengeli && e.C == 3 && e.D == 3 && e.H == 3) return "3NT";
                if (e.C == 4) return "3♣";
                if (e.D == 4) return "3♦";
                if (e.H == 4) return "3♥";
                return "3NT";
            }
            return null;
        }

        // ───────── CEVAPÇININ KARARI ─────────
        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            int hcp = e.Hcp;
            if (devam == "2NT" && roleCevap == "3♣") return "Pas";                                  // 2NTt tamam
            if (rebid == "1NT" && devam == "2♥" && roleCevap == "2♠") return hcp <= 8 ? "Pas" : null;   // transfer tamam
            if (rebid == "1NT" && devam == "2♦" && roleCevap == "2♥") return hcp <= 8 ? "Pas" : null;
            if (rebid == "1NT" && devam == "2♣" && roleCevap == "2♦")                               // puppet: 5+♠ FG
                return Sec(new[] {
                    K("3♦", x => (x.S >= 6 && x.D >= 4) || (x.S >= 5 && x.D >= 5)),
                    K("2NT", x => x.S >= 6),
                    K("3♣", x => x.S == 5 && x.D == 4),
                    K("2♠", x => x.S >= 5 && x.C >= 4),
                    K("2♥", x => x.S >= 5 && x.H >= 4),
                }, e, null);
            if (rebid == "1NT" && devam == "2♠" && roleCevap == "2NT")                              // puppet: 4♠ 5m FG
                return e.C >= e.D ? "3♣" : "3♦";
            return null;
        }

        internal override bool KararTanimli(string rebid, string devam)
        {
            return (rebid == "1NT" && (devam == "2♣" || devam == "2♠" || devam == "2♦" || devam == "2♥")) || devam == "2NT";
        }

        internal override string AciciSon(OcEl e, string rebid, string devam, string roleCevap, string karar)
        {
            if (rebid == "1NT" && devam == "2♣" && roleCevap == "2♦")                               // SAS: sıradaki basamak
            {
                if (karar == "2♥") return "2♠";
                if (karar == "2♠") return "2NT";
                if (karar == "2NT") return "3♣";
                if (karar == "3♣") return "3♦";
                if (karar == "3♦") return "3♥";
            }
            return null;
        }
    }
}
