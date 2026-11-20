using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 1NT GELİŞİMİ (SAS kitabı s.21). Cevapçı 7-11 HCP, 4'lü majör yok.
    /// Açan: Pas (11-14 dengeli), 2♣ 6+♣ dengesiz 11-16, 2♦/2♥/2♠/3♣ reverse gücünde 17+ (2♦ 4+♦ sinek uzun; 2♥/2♠ 5+♣ 4M; 3♣ 6+♣), 2NT 18-19 dengeli (limit; kaptan cevapçı),
    /// 3♦/3♥/3♠ 6+♣ 5x. Cevapçı "min" 7-8, "max" 9-11 HCP.
    /// </summary>
    public class OneClub_OneNT : OneClub_GelisimTabani
    {
        protected override string Cevap { get { return "1NT"; } }
        public override string Aciklama { get { return "1♣-1NT gelişimi"; } }

        internal override string AciciRebid(OcEl e)
        {
            return Sec(new[] {
                K("2NT", x => x.Hcp >= 18 && x.Hcp <= 19 && x.Dengeli),   // limit bildirimi: dengeli 18-19 (majör uzunluğu önemsiz); kaptan cevapçı
                K("2♥", x => x.Hcp >= 17 && x.C >= 5 && x.H >= 4),
                K("2♠", x => x.Hcp >= 17 && x.C >= 5 && x.S >= 4),
                K("2♦", x => x.Hcp >= 17 && x.D >= 4 && x.C > x.D),                  // reverse: önce ucuz renk (♣), sonra pahalı renk (♦); karo sinekten kısa
                K("3♦", x => x.C >= 6 && x.D >= 5),
                K("3♥", x => x.C >= 6 && x.H >= 5),
                K("3♠", x => x.C >= 6 && x.S >= 5),
                K("3♣", x => x.Hcp >= 17 && x.C >= 6),                                 // reverse gücünde (17+), zorlayıcı
                K("2♣", x => x.Hcp >= 11 && x.Hcp <= 16 && x.C >= 6 && !x.Dengeli),
            }, e, "Pas");
        }

        // Cevapçı kaptandır (7-11 HP, 4'lü majör yok). Açıcının 2♦/2♥/2♠/3♣ tekrarı 17+ HP, üst sınırı yok:
        // cevapçı uyumunu ve gücünü bildirir; açıcı oyun seviyesine çıkar. (Slam araması bu ağaçta yok; genel motor.)
        internal override string CevapciDevam(OcEl e, string rebid)
        {
            switch (rebid)
            {
                case "2♦":   // 4+♦ (sinek uzun) 17+: karo uyumu → 3♦, sinek uyumu → 3♣, aksi halde 2NT
                    return Sec(new[] {
                        K("3♦", x => x.D >= 4),
                        K("3♣", x => x.C >= 3),
                    }, e, "2NT");
                case "2♥":   // 5+♣ 4+♥ 17+: 3+ kupa → 3♥ (uyum), sinek desteği → 3♣, aksi halde 2NT
                    return Sec(new[] {
                        K("3♥", x => x.H >= 3),
                        K("3♣", x => x.C >= 3),
                    }, e, "2NT");
                case "2♠":   // 5+♣ 4+♠ 17+
                    return Sec(new[] {
                        K("3♠", x => x.S >= 3),
                        K("3♣", x => x.C >= 3),
                    }, e, "2NT");
                case "3♣":   // 6+♣ 17+: 8+ HP ile 3NT, 7 HP ile sinek oyunu
                    return Sec(new[] {
                        K("3NT", x => x.Hcp >= 8),
                    }, e, "4♣");
                case "2NT":  // dengeli 18-19: 7-10 HP 3NT, 11 HP (29-30) niceliksel 4NT (slam daveti)
                    return Sec(new[] {
                        K("4NT", x => x.Hcp >= 11),
                    }, e, "3NT");
                default:     // 2♣, 3♦, 3♥, 3♠: gelişim natürel
                    return "Pas";
            }
        }

        // Açıcının 2. tekrarı: cevapçının uyum/güç bildirimine oyun seviyesinde cevap (17-18 ile 19+ ayrımı slam aramasına bırakıldı)
        internal override string AciciRoleCevap(OcEl e, string rebid, string devam)
        {
            switch (rebid)
            {
                case "2♦":
                    if (devam == "3♦" || devam == "3♣" || devam == "2NT") return "3NT";
                    return null;
                case "2♥":
                    if (devam == "3♥") return "4♥";
                    if (devam == "3♣" || devam == "2NT") return "3NT";
                    return null;
                case "2♠":
                    if (devam == "3♠") return "4♠";
                    if (devam == "3♣" || devam == "2NT") return "3NT";
                    return null;
                case "2NT":
                    if (devam == "4NT") return e.Hcp >= 19 ? "6NT" : "Pas";   // 29-30 puan: 19 ile slam, 18 ile pas
                    return null;
                default:
                    return null;
            }
        }
    }
}
