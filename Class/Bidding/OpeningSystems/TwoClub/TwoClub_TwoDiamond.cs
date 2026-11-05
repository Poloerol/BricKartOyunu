using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoClub
{
    /// <summary>
    /// 2♣-2♦ gelişimi (kitap s.59-60). Açan: 2♥ 5+♥ / 25+ dengeli; 2♠ 5+♠ OG; 3♣ 5+♣; 3♦ 6+♦; 3♥/3♠ 4M 5+♦.
    /// 2♥ → cevapçı 2♠ (0-7) / 2NT (5/4 majör 3-6) / 3m... ; 2♠ → 2NT (0-7) / 3♣.. ; 3♣ → 3♦ röle.
    /// Röle sonrası açan: 2NT 25+ dengeli, 3♣ 5M 4x SI, 3♦ 6+M, 3♥/3♠/3NT 5M 4♣/4♦/4.M, 4M.
    /// </summary>
    public class TwoClub_TwoDiamond : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♣"; } }
        protected override string Cevap { get { return "2♦"; } }
        public override string Aciklama { get { return "2♣-2♦ gelişimi (kitap s.59-60)"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.H >= 5 && e.H >= e.S) return "2♥";
            if (e.S >= 5) return "2♠";
            if (e.Dengeli && e.Hcp >= 25) return "2♥";
            if (e.D >= 6) return "3♦";
            if (e.C >= 5) return "3♣";
            if (e.H == 4 && e.D >= 5) return "3♥";
            if (e.S == 4 && e.D >= 5) return "3♠";
            return "2NT";                                                                 // diğer eller: dengeli/yedek
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "2♥" || rebid == "2♠" || TcDevam.Tanimli(rebid); }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3♣") return "3♦";                                               // röle
            if (rebid != "2♥" && rebid != "2♠") return TcDevam.Dogal(e, rebid);
            if (rebid == "2♥")
            {
                if (e.Hcp <= 7)
                {
                    bool majorlar = (e.S >= 5 && e.H >= 4) || (e.H >= 5 && e.S >= 4);
                    if (majorlar && e.Hcp >= 3 && e.Hcp <= 6) return "2NT";
                    return "2♠";
                }
                if (e.H >= 6) return "3♦";
                if (e.H >= 5 && e.D >= 4) return "3♣";
                if (e.H >= 5 && e.C >= 4) return "3♠";
                if (e.H >= 4 && e.S >= 4) return "3♥";
                return "3NT";
            }
            // rebid 2♠ (5+♠ OG)
            if (e.Hcp <= 7) return "2NT";
            if (e.H >= 6) return "3♦";
            if (e.H >= 5 && e.D >= 4) return "3♣";
            if (e.H >= 5 && e.C >= 4) return "3♠";
            if (e.S >= 4 && e.C <= 1) return "4♣";
            if (e.S >= 4 && e.D <= 1) return "4♦";
            if (e.H >= 5 && e.S >= 3) return "3♥";
            return "3NT";
        }

        private static bool Ana(string rebid, string devam)
        {
            return (rebid == "2♥" && devam == "2♠") || (rebid == "2♠" && devam == "2NT");
        }

        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            if (rebid == "3♣" && devam == "3♦")                                          // 5+♣ OG, cevapçı röle
            {
                if (e.H == 4) return "3♥";
                if (e.S == 4) return "3♠";
                return "3NT";
            }
            if ((rebid == "2♥" || rebid == "2♠") && devam != "2♠" && devam != "2NT" && devam != "3NT")
            {                                                                            // cevapçının doğal/özel OG teklifi
                bool k = rebid == "2♥";
                int m1 = k ? e.H : e.S;
                if (devam == "3♥" && k) return e.H >= 4 ? "4♥" : (e.S >= 4 ? "4♠" : "3NT");
                if (devam == "3♥") return "4♠";                                          // ♠ desteği (3+♠)
                if (devam == "4♣" || devam == "4♦") return "4♠";                         // ♠ uyumu + kısalık
                return e.H >= 3 ? "4♥" : (m1 >= 3 && !k ? "4♠" : "3NT");
            }
            if (rebid == "2NT")                                                          // yedek: cevapçı doğal OG
            {
                if (devam == "3♥") return e.H >= 3 ? "4♥" : "3NT";
                if (devam == "3♠") return e.S >= 3 ? "4♠" : "3NT";
                return null;
            }
            if (!Ana(rebid, devam)) return null;
            bool kupa = rebid == "2♥";
            int m = kupa ? e.H : e.S;
            if (kupa && e.Dengeli && e.Hcp >= 25) return "2NT";
            if (m >= 6) return "3♦";
            if (m < 5) return kupa ? "2NT" : "3NT";
            // 5'li majör + yan renkler
            int dort = (e.C == 4 ? 1 : 0) + (e.D == 4 ? 1 : 0) + ((kupa ? e.S : e.H) == 4 ? 1 : 0);
            if (dort >= 2) return "3♣";                                                   // SI: iki 4'lü yan renk
            if (e.C == 4) return "3♥";
            if (e.D == 4) return "3♠";
            if ((kupa ? e.S : e.H) == 4) return "3NT";
            return kupa ? "4♥" : "4♠";
        }

        private static bool Honour(OcEl e, string renk) { return e.El.Any(c => c.Suit == renk && c.Value >= 12); }

        internal override string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap)
        {
            if (rebid == "3♣" && devam == "3♦")
            {
                if (roleCevap == "3♥") return e.H >= 3 ? "4♥" : "3NT";
                if (roleCevap == "3♠") return e.S >= 3 ? "4♠" : "3NT";
                return null;
            }
            if (!Ana(rebid, devam)) return null;
            bool kupa = rebid == "2♥";
            int m = kupa ? e.H : e.S;
            if (roleCevap == "2NT")                                                       // 25+ dengeli: oyun
            {
                if (e.S >= 5) return "3♠";
                if (e.H >= 5) return "3♥";
                return "3NT";
            }
            string renk = kupa ? "Kupa" : "Maça";
            if (roleCevap == "3♣")                                                        // 5M 4x SI
            {
                int diger = kupa ? e.S : e.H;
                if (m >= 3)
                {
                    if (e.C >= 4) return "3♥";
                    if (e.D >= 4) return "3♠";
                    if (diger >= 4) return "3NT";
                    return kupa ? "4♣" : "4♣";                                           // 4x = destek + kontrol
                }
                return "3♦";                                                              // röle: uyum yok
            }
            if (roleCevap == "3♦")                                                        // 6+M: honör / kısalık
            {
                if (m >= 3) return "4♣";                                                  // 3+ destek: koz kontrol
                if (m == 2) return "3NT";
                if (m <= 1) return "3♠";
                return null;
            }
            return null;
        }

        internal override string AciciSon(OcEl e, string rebid, string devam, string roleCevap, string karar)
        {
            if (!Ana(rebid, devam)) return null;
            bool kupa = rebid == "2♥";
            if (roleCevap == "3♦" && (karar == "3♠" || karar == "3NT"))                   // 6+M: karşıda 0-2 koz
                return kupa ? "4♥" : "4♠";
            if (roleCevap != "3♣" || karar != "3♦") return null;                          // SI sorusuna uyum yok
            if (e.C == 4) return "3♥";
            if (e.D == 4) return "3♠";
            return "3NT";
        }
    }
}
