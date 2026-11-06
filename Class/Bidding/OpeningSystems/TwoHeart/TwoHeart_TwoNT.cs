using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoHeart
{
    /// <summary>
    /// 2♥-2NT gelişimi (kitap s.70). 2NT = 5+♠ veya 5+minör, F1R (zorlayıcı bir tur). Açan (6+♥):
    ///   3NT = kapalı 6♥ (AKQ'dan en az ikisi) · 3♠ = 4♠ azami (9+ HCP) · 4♣/4♦ = 4 kart minör ve ♠ en fazla ikili (6-4 ♥/m)
    ///   3♥ = ♣ ve ♦ ikili, ♠ üçlü · 3♣/3♦ = tercih (o minörde 3+ kart; ♣ ≥ ♦ ise 3♣)
    /// Cevapçı: 3♣/3♦ sonrası 6+♠ ise 3♠ (FG) · 3♥ sonrası 5+♠ ise 3♠ (FG) · 4♣/4♦ sonrası 5+♠ ise 4♠, değilse 4NT (oynamak için)
    /// 3♠/3NT sonrası "gelişim natürel": sınıf devreye girmez.
    /// Basitleştirme: "kapalı" = ♥'de A/K/Q'dan en az ikisi; kitapta kuralların sırası yazmıyor, yukarıdaki sıra seçildi.
    /// </summary>
    public class TwoHeart_TwoNT : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "2♥"; } }
        protected override string Cevap { get { return "2NT"; } }
        public override string Aciklama { get { return "2♥-2NT gelişimi (kitap s.70): açan 3♣/3♦ tercih, 3♥ 2-2 minör, 3♠ 4♠ azami, 3NT kapalı, 4m 4 kart minör"; } }

        internal override string AciciRebid(OcEl e)
        {
            int ust = e.El.Count(c => c.Suit == "Kupa" && c.Value >= 12);
            if (e.H >= 6 && ust >= 2 && e.Hcp >= 9) return "3NT";                          // kapalı renk
            if (e.S == 4 && e.Hcp >= 9) return "3♠";                                         // 6♥ 4♠ azami
            if ((e.C == 4 || e.D == 4) && e.S <= 2) return e.C == 4 ? "4♣" : "4♦";         // 6♥ 4m 2♠
            if (e.C == 2 && e.D == 2 && e.S == 3) return "3♥";
            if (e.C >= 3 && e.C >= e.D) return "3♣";
            if (e.D >= 3) return "3♦";
            return "3♥";
        }

        internal override bool DevamTanimli(string rebid) { return rebid == "3♣" || rebid == "3♦" || rebid == "3♥" || rebid == "4♣" || rebid == "4♦"; }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            if (rebid == "3♣" || rebid == "3♦") return e.S >= 6 ? "3♠" : null;              // 3♠ 6+♠ FG
            if (rebid == "3♥") return e.S >= 5 ? "3♠" : null;                                // 3♠ 5+♠ FG
            return e.S >= 5 ? "4♠" : "4NT";                                                  // 4m: oynamak için
        }
    }
}
