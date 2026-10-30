using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Artif. 2C/2D Resp. to 1M-X: 1M–(Dbl) sonrası 2♣ = 3'lü destekle limit raise (10-12), 2♦ = 3+ destekle oyun zorlayan raise (13+).</summary>
    internal sealed class Artificial2C2D : YeniKonvTaban
    {
        public override string Ad { get { return "Artif. 2C/2D Resp. to 1M-X"; } }
        public override string Aciklama { get { return "1M–(Dbl)–2♣ 3'lü destekli limit, 2♦ oyun zorlayan raise"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1♠", "R:Dbl") || z.Es("O:1♥", "R:Dbl"))
            {
                string m = z.Hepsi[0] == "O:1♠" ? "Maça" : "Kupa";
                int u = z.Uz(m);
                if (u >= 3 && z.Hp >= 13) return "2♦";
                if (u == 3 && z.Hp >= 10 && z.Hp <= 12) return "2♣";
                return null;
            }
            if (z.Es("B:1♠", "L:Dbl", "O:2♣") || z.Es("B:1♥", "L:Dbl", "O:2♣"))
            {
                string m = z.Hepsi[0] == "B:1♠" ? "Maça" : "Kupa";
                return z.Hp >= 15 ? YK.T(4, m) : YK.T(3, m);
            }
            if (z.Es("B:1♠", "L:Dbl", "O:2♦") || z.Es("B:1♥", "L:Dbl", "O:2♦"))
            {
                string m = z.Hepsi[0] == "B:1♠" ? "Maça" : "Kupa";
                return YK.T(4, m);
            }
            return null;
        }
    }
}
