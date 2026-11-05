using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 2♣ GELİŞİMİ (SAS kitabı s.22): cevapçı 12+ HCP FG röle (2+♣ dengeli veya 5+♣).
    /// Açan: 2♦ min (11-14), 2♥/2♠ 5+♣ 4M, 2NT 18+ (6+♣ veya 4-4-4-1), 3♣ 6+♣ 15-17 SNG'li, 3♦ SNG'siz,
    /// 3♥/3♠/3NT 4-4-4-1 15-17. Röle cevapları kitaptaki DETAY bloklarına göredir.
    /// </summary>
    public class OneClub_TwoClub : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "2♣"; } }
        public override string Aciklama { get { return "1♣-2♣ gelişimi"; } }

        private static bool Dortlu(OcEl x, out string kisa)
        {
            kisa = null;
            if (x.C == 4 && x.S == 4 && x.H == 4 && x.D == 1) kisa = "D";
            else if (x.C == 4 && x.S == 4 && x.D == 4 && x.H == 1) kisa = "H";
            else if (x.C == 4 && x.H == 4 && x.D == 4 && x.S == 1) kisa = "S";
            return kisa != null;
        }

        internal override string AciciRebid(OcEl e)
        {
            string kisa;
            bool dortlu = Dortlu(e, out kisa);
            if (e.Hcp >= 18 && (e.C >= 6 || dortlu)) return "2NT";
            if (dortlu && e.Hcp >= 15) return kisa == "D" ? "3♥" : kisa == "H" ? "3♠" : "3NT";
            if (e.C >= 6 && e.Hcp >= 15 && e.Hcp <= 17) return e.Singleton ? "3♣" : "3♦";
            if (e.C >= 5 && e.H >= 4 && e.Hcp >= 11) return "2♥";
            if (e.C >= 5 && e.S >= 4 && e.Hcp >= 11) return "2♠";
            return "2♦";   // minimum
        }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            switch (rebid)
            {
                case "2♦":   // min
                    return Sec(new[] {
                        K("3♣", x => x.C >= 6 && x.Singleton),                          // 6+♣ SNG
                        K("3♦", x => x.C >= 5 && (x.S >= 4 || x.H >= 4) && x.D <= 1),   // 5♣4M SNG ♦
                        K("3♥", x => x.C >= 5 && x.S >= 4 && x.H <= 1),
                        K("3♠", x => x.C >= 5 && x.H >= 4 && x.S <= 1),
                        K("2NT", x => x.Dengeli && x.Hcp <= 14),                        // 12-14 dengeli
                        K("2♥", x => x.Hcp >= 16),                                      // röle SI
                    }, e, "2♠");                                                        // röle FG
                case "2♥":   // 5+♣ 4♥
                    return e.H >= 4 ? "2NT" : "2♠";                                     // SAS
                case "2♠":   // 5+♣ 4♠
                    return "2NT";                                                       // SAS
                case "2NT":  // 18+
                    return "3♣";                                                        // röle
                case "3♣":   // 6+♣ 15-17 SNG'li
                    return "3♦";                                                        // röle (SNG ♦; ♥; ♠)
                case "3♥":   // 4-4-4-1 15-17
                case "3♠":
                case "3NT":
                    return rebid == "3NT" ? "Pas" : (e.H >= 3 && rebid == "3♥" ? "4♥" : (e.S >= 3 ? "4♠" : "3NT"));
                default:     // 3♦: gelişim natürel
                    return "3NT";
            }
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            string kisa;
            bool dortlu = Dortlu(e, out kisa);
            if (rebid == "2♦" && devam == "2♥")      // min cevapları
            {
                if (e.C >= 5 && e.D >= 4) return "2♠";
                if (e.Dengeli && e.Hcp <= 14) return "2NT";
                if (e.C >= 6) return e.Singleton ? "3♣" : "3♦";
                if (dortlu) return kisa == "D" ? "3♥" : kisa == "H" ? "3♠" : "3NT";
                return "2NT";
            }
            if (rebid == "2♦" && devam == "2♠")      // röle FG
            {
                if (e.C >= 5 && e.D >= 4)
                {
                    if (e.S == 2 && e.H == 2 && e.Hcp >= 15) return "3♥";
                    if (e.S == 3 && e.H == 1 && e.Hcp >= 15) return "3♠";
                    if (e.S == 1 && e.H == 3 && e.Hcp >= 18) return "4♣";
                    if (e.S == 1 && e.H == 3 && e.Hcp >= 15) return "3NT";
                    if (e.Singleton) return "3♦";
                }
                if (e.C >= 6 || dortlu) return "3♣";
                return "2NT";
            }
            // SAS: 1♣-2♣-2♥-2♠/2NT ve 1♣-2♣-2♠-2NT. Açan iki renkli eli adımlarla anlatır (kitap s.91):
            //   3♣ = 5-4, 3♦ = 6-4, 3♥ = 5-5 küçük kısa, 3♠ = 5-5 büyük kısa, 3NT = 5-4-4
            if ((rebid == "2♥" && (devam == "2♠" || devam == "2NT")) || (rebid == "2♠" && devam == "2NT"))
            {
                string ikinci = rebid == "2♥" ? "H" : "S";
                int ik = ikinci == "H" ? e.H : e.S;
                int diger = ikinci == "H" ? e.S : e.H;
                if (e.C >= 5 && ik >= 5 && e.C == 5)
                    return (e.D <= diger ? "3♥" : "3♠");                              // 5-5: kısa lower/higher
                if (e.C >= 6 && ik == 4) return "3♦";
                if (e.C >= 5 && ik == 4 && (e.D == 4 || diger == 4)) return "3NT";     // 5-4-4
                return "3♣";                                                          // 5-4
            }
            if (rebid == "2NT" && devam == "3♣")     // 18+ röle: 6+♣ / SNG ♦, ♥, ♠
                return e.C >= 6 ? "3♦" : (e.D == 1 ? "3♥" : e.H == 1 ? "3♠" : "3NT");
            if (rebid == "3♣" && devam == "3♦")      // SNG'li 6+♣: SNG ♦ / ♥ / ♠
                return e.D <= 1 ? "3♥" : (e.H <= 1 ? "3♠" : "3NT");
            return null;
        }
    }
}
