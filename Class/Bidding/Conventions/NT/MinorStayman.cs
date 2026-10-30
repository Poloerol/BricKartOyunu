using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Minör Stayman: 1NT–2♠ = oyun zorlayan, en az 4-4 minörlü, 4'lü majörü olmayan el (10+ HP).
    /// Açıcı: 2NT = 4'lü minör yok; 3♣ / 3♦ = o minörde 4+ kart (ikisi de varsa uzun olan, eşitse ♣).
    /// Cevapçı: 2NT sonrası 3NT. Minör fiti bulunursa 15+ HP ile minörü 4 seviyede söyler (şlem denemesi), aksi halde 3NT.
    /// MinorTransfer ile birlikte kullanılamaz (ikisi de 2♠'yı kullanır).
    /// </summary>
    internal sealed class MinorStayman : YeniKonvTaban
    {
        public override string Ad { get { return "Minor Stayman"; } }
        public override string Aciklama { get { return "1NT–2♠: oyun zorlayan, 4-4 minör sorusu; 2NT yok, 3♣/3♦ 4'lü minör"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1NT"))
                return z.Hp >= 10 && z.Maca < 4 && z.Kupa < 4 && z.Sinek >= 4 && z.Karo >= 4 ? "2♠" : null;
            if (z.Es("B:1NT", "O:2♠"))
            {
                if (z.Sinek < 4 && z.Karo < 4) return "2NT";
                return z.Karo > z.Sinek ? "3♦" : "3♣";
            }
            if (z.Es("O:1NT", "B:2♠", "O:2NT")) return "3NT";
            if (z.Es("O:1NT", "B:2♠", "O:3♣")) return z.Hp >= 15 ? "4♣" : "3NT";
            if (z.Es("O:1NT", "B:2♠", "O:3♦")) return z.Hp >= 15 ? "4♦" : "3NT";
            return null;
        }
    }
}
