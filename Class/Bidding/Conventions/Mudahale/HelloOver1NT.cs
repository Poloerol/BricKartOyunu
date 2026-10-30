using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Hello over 1NT: rakibin 1NT'sine karşı müdahale (2♣ iki majör, 2♦ tek renkli majör, 2♥/2♠ majör+minör, 2NT iki minör).</summary>
    internal sealed class HelloOver1NT : YeniKonvTaban
    {
        public override string Ad { get { return "Hello over 1NT"; } }
        public override string Aciklama { get { return "Rakip 1NT'sine: 2♣ iki majör, 2♦ uzun majör, 2♥/2♠ majör+minör, 2NT iki minör"; } }
        protected override string Karar(Dz z)
        {
            int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
            if (z.Es("R:1NT") || z.Es("L:1NT"))
            {
                if (z.Hp < 8 || z.Hp > 15) return null;
                if (s >= 4 && h >= 4 && (s >= 5 || h >= 5)) return "2♣";
                if (s >= 6 || h >= 6) return "2♦";
                if (s >= 5 && (d >= 4 || c >= 4)) return "2♠";
                if (h >= 5 && (d >= 4 || c >= 4)) return "2♥";
                if (d >= 5 && c >= 5) return "2NT";
                return null;
            }
            // Ortak müdahale etti, sıra bende (ilerletici)
            if (z.Es("L:1NT", "O:2♣"))
            {
                string m = h >= s ? "Kupa" : "Maça";
                int u = z.Uz(m);
                if (z.Hp >= 10 && u >= 4) return YK.T(3, m);
                return YK.T(2, m);
            }
            if (z.Es("L:1NT", "O:2♦")) return "2♥";                       // geçer ya da düzelt
            if (z.Es("L:1NT", "O:2NT")) return d >= c ? "3♦" : "3♣";
            // Müdahaleci: 2♦ → 2♥ cevabından sonra
            if (z.Es("R:1NT", "B:2♦", "O:2♥") || z.Es("L:1NT", "B:2♦", "O:2♥")) return h >= 6 ? "Pas" : "2♠";
            return null;
        }
    }
}
