using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>
    /// ZAYIF NT VERSİYONU: 1♦ ⇒ 1♥ / 1♦ ⇒ 1♠ GELİŞİMLERİNİN 1NT REBİDİ (SAS kitabı s.85-86).
    /// Açan 5+♦ ve cevapçının majöründe tam 3 kart (11-17 HCP) ile 1NT der; diğer rebidler standart tablodaki gibidir
    /// (kitap: "diğer açışlar ve gelişimleri sistemdeki gibidir"), bu yüzden sınıf yalnızca bu elde devreye girer.
    /// Cevapçı: 2♣ röle, 2♦ zayıf (4M 3+♦), 2M zayıf (5+M), diğer majör 4'lü ile 2M FG, 2NTt, 3 seviyesi davetler, tekli SI.
    /// Röle cevapları: 2♦ 6+♦ 11-13, 2♥/2♠ (♦ 5'li, 11-14), 2♠/2♥ (4 kartlı diğer majör, tekli ♣, 15-17), 2NT (4♣, tekli diğer majör, 15-17), 3♦ 6+♦ 14-17.
    /// Basitleştirmeler: "KK/BK min-max" kodları ve röle cevaplarından sonraki natürel gelişim genel motora bırakıldı.
    /// </summary>
    public abstract class OneDiamondZ_Taban : OneClub_GelisimTabani
    {
        protected abstract char M { get; }                       // cevapçının majörü
        private char Diger { get { return M == '♥' ? '♠' : '♥'; } }
        private int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }

        protected override string Acilis { get { return "1♦"; } }
        public override int Oncelik { get { return 42; } }      // aynı rebid için standart tablodan (41) önce

        internal override bool AciciRebidTanimli(OcEl e) { return e.D >= 5 && Uz(e, M) == 3 && e.Hcp >= 11 && e.Hcp <= 17; }
        internal override string AciciRebid(OcEl e) { return "1NT"; }

        internal override bool DevamTanimli(string rebid) { return rebid == "1NT"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hcp = e.Hcp, m = Uz(e, M), dg = Uz(e, Diger);
            bool davet = hcp >= 9 && hcp <= 11, oyun = hcp >= 12, zayif = hcp <= 8;
            string m1 = "2" + M;
            return Sec(new[] {
                K("4♣", x => oyun && m >= 5 && x.C <= 1),                                       // 5+M SNG ♣ SI
                K("4♦", x => oyun && m >= 5 && x.D <= 1),                                       // SNG ♦ SI
                K(M == '♥' ? "3♠" : "4♥", x => oyun && m >= 5 && dg <= 1),                                 // SNG diğer majör SI
                K(M == '♥' ? "2♠" : "2♥", x => oyun && m >= 5 && dg >= 4),                      // 5+M 4 diğer FG
                K("2♣", x => oyun),                                                             // röle
                K("3" + M, x => davet && m >= 6),                                               // 6+M davet
                K("3♣", x => davet && m >= 5 && x.C >= 5),
                K("3♦", x => davet && m >= 5 && x.D >= 5),
                K("3♥", x => M == '♠' && davet && m >= 5 && dg >= 5),                  // 5♠5♥ davet
                K("2♣", x => davet),                                                            // röle
                K(m1, x => zayif && m >= 5),                                                    // S.O
                K("2♦", x => zayif && m == 4 && x.D >= 3),                                      // 4M 3+♦ S.O
                K("2NT", x => zayif && x.C >= 6),                                               // 2NTt
            }, e, "Pas");
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            int hcp = e.Hcp;
            if (devam == "2NT") return "3♣";                                                    // 2NTt
            if (devam != "2♣") return null;
            // 2♣ röle: kitaptaki dağılım tanımları
            if (e.D == 5 && Uz(e, Diger) == 4 && e.C == 1 && hcp >= 15) return "2" + Diger;      // 5♦ 4D 3M 1♣
            if (e.D == 5 && e.C == 4 && Uz(e, Diger) == 1 && hcp >= 15) return "2NT";           // 5♦ 4♣ 3M 1D
            if (e.D >= 6 && hcp >= 14) return "3♦";
            if (e.D >= 6 && hcp <= 13) return "2♦";
            if (e.D == 5 && hcp <= 14) return "2" + M;                                          // 5♦ 3M min
            return "2NT";
        }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (devam == "2NT" && roleCevap == "3♣") return "Pas";                              // 2NTt tamam: ♣ oynanır
            return null;                                                                        // geri kalanı natürel
        }
    }

    public class OneDiamondZ_OneHeart : OneDiamondZ_Taban
    {
        protected override char M { get { return '♥'; } }
        protected override string Cevap { get { return "1♥"; } }
        public override string Aciklama { get { return "Zayıf NT: 1♦-1♥ gelişimi, 1NT rebidi (kitap s.85)"; } }
    }

    public class OneDiamondZ_OneSpade : OneDiamondZ_Taban
    {
        protected override char M { get { return '♠'; } }
        protected override string Cevap { get { return "1♠"; } }
        public override string Aciklama { get { return "Zayıf NT: 1♦-1♠ gelişimi, 1NT rebidi (kitap s.86)"; } }
    }
}
