using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// ZAYIF NT VERSİYONU: 1♣ ⇒ 1♠ (cevapçının 4+♦'ü, transfer) GELİŞİMİ (SAS kitabı s.83).
    /// Yalnızca OneClub_AnswerSistemi açık ve zayıf NT (StrongNT=false) iken çalışır.
    /// Açan: 1NT (dengeli/yarı dengeli 14-17) | 2♣ (5+♣ 11-16).
    /// Cevapçı: 1NT'den sonra transferler (2♦→♥, 2♥→♠, 2♠→♣), 2♣ puppet (6+♦), davet 2NT/3♣/3♦, tekli gösteren 3♥/3♠;
    /// 2♣'den sonra röle 2♦, 4. renk 2♥/2♠, 2NTt.
    /// Basitleştirmeler: kitapta "Gelişmeler natürel" denen satırlarda sınıf devreye girmez (genel motor sürdürür).
    /// </summary>
    public class OneClubZ_OneSpade : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "1♠"; } }
        public override string Aciklama { get { return "Zayıf NT: 1♣-1♠ (4+♦) gelişimi (kitap s.83)"; } }
        public override int Oncelik { get { return 40; } }

        private static bool Bal(OcEl e) { return e.Hcp >= 14 && e.Hcp <= 17 && OneClub_Opening.ZayifNTSinekDengeli(e.El); }

        private static string Rebid(OcEl e)
        {
            if (Bal(e)) return "1NT";
            if (e.C >= 5 && e.Hcp >= 11 && e.Hcp <= 16) return "2♣";
            return null;
        }
        internal override bool AciciRebidTanimli(OcEl e) { return Rebid(e) != null; }
        internal override string AciciRebid(OcEl e) { return Rebid(e) ?? "1NT"; }

        internal override bool DevamTanimli(string rebid) { return rebid == "1NT" || rebid == "2♣"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp;
            bool davet = hcp >= 9 && hcp <= 11, oyun = hcp >= 12;
            switch (rebid)
            {
                case "1NT":  // dengeli (14)15-17 (s.83)
                    return Sec(new[] {
                        K("3♥", x => oyun && x.D >= 5 && x.C >= 4 && x.S >= 4 && x.H <= 1),   // 5♦4♣4♠ SNG ♥ FG
                        K("3♠", x => oyun && x.D >= 5 && x.C >= 4 && x.H >= 4 && x.S <= 1),   // 5♦4♣4♥ SNG ♠ FG
                        K("2♠", x => hcp >= 15 && x.D >= 5 && x.C >= 4),                      // ♣'e transfer 15+
                        K("2♦", x => oyun && x.D >= 5 && x.H >= 4),                           // ♥'e transfer FG
                        K("2♥", x => oyun && x.D >= 5 && x.S >= 4),                           // ♠'e transfer FG
                        K("2♣", x => oyun && x.D >= 6),                                       // puppet to 2♦
                        K("3NT", x => oyun && x.D >= 4),                                      // natürel
                        K("3♣", x => davet && x.D >= 5 && x.C >= 5),                          // 5♦5♣ davet
                        K("3♦", x => davet && x.D >= 6),                                      // 6+♦ davet w/o SNG
                        K("2NT", x => davet && x.D >= 4 && x.Dengeli),                        // dengeli davet
                    }, e, "Pas");
                case "2♣":   // 5+♣ 11-16 (s.83)
                    return Sec(new[] {
                        K("3♥", x => oyun && x.D >= 5 && x.S >= 4 && x.C >= 4 && x.H <= 1),   // 5♦4♠4♣1♥ FG
                        K("3♠", x => oyun && x.D >= 5 && x.H >= 4 && x.C >= 4 && x.S <= 1),   // 5♦4♥4♣1♠ FG
                        K("2♥", x => oyun && x.D >= 5 && x.H >= 4),                           // 5+♦ 4♥ FG
                        K("2♠", x => oyun && x.D >= 5 && x.S >= 4),                           // 5+♦ 4♠ FG
                        K("2♦", x => oyun),                                                   // röle
                        K("3♦", x => davet && x.D >= 6),                                      // 6+♦ davet
                        K("3♣", x => davet && x.C >= 3),                                      // davet
                        K("2NT", x => hcp <= 8 && x.C >= 6),                                  // 2NTt (3♣ transfer)
                    }, e, "Pas");
                default:
                    return "Pas";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid == "1NT")
            {
                if (devam == "2♣") return "2♦";                                                       // puppet
                if (devam == "2♦") return "2♥";                                                       // transfer ♥
                if (devam == "2♥") return "2♠";                                                       // transfer ♠
                if (devam == "2♠") return "3♣";                                                       // transfer ♣
                if (devam == "2NT") return e.Hcp >= 16 ? "3NT" : "Pas";                               // davet
                return null;
            }
            if (rebid == "2♣")
            {
                if (devam == "2NT") return "3♣";                                                      // 2NTt → 3♣
                if (devam == "2♦")                                                                    // s.83 detay
                {
                    if (e.C >= 6 && e.D == 3 && e.H == 1) return "3♥";
                    if (e.C >= 6 && e.D == 3 && e.S == 1) return "3♠";
                    if (e.C >= 6 && e.D == 3) return "3♦";
                    if (e.C >= 5 && e.H == 4) return "2♥";
                    if (e.C >= 5 && e.S == 4) return "2♠";
                    if (e.C >= 6) return e.Hcp >= 13 ? "2NT" : "3♣";
                    return "2♥";
                }
            }
            return null;
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            int hcp = e.Hcp;
            if (devam == "2NT" && roleCevap == "3♣") return "Pas";                                  // 2NTt tamam
            if (rebid == "1NT" && devam == "2♣" && roleCevap == "2♦" && e.D >= 6)                  // puppet: 6+♦, tekli gösterilir
            {
                if (hcp >= 12)
                    return Sec(new[] {
                        K("3♣", x => x.C <= 1), K("3♥", x => x.H <= 1), K("3♠", x => x.S <= 1),
                    }, e, "3♦");                                                                   // 3♦ = tekli yok, SI
                if (hcp >= 9)
                    return Sec(new[] {
                        K("2♥", x => x.C <= 1), K("2♠", x => x.H <= 1), K("2NT", x => x.S <= 1),
                    }, e, null);
            }
            return null;
        }
    }
}
