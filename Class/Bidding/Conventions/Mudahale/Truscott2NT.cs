using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Truscott 2NT: 1M–(Dbl)–2NT = 4+ destekli limit raise (10-12 HP); açıcı 15+ ile oyun, aksi hâlde 3M.</summary>
    internal sealed class Truscott2NT : YeniKonvTaban
    {
        public override string Ad { get { return "Truscott 2NT"; } }
        public override string Aciklama { get { return "1M–(Dbl)–2NT: 4+ destekli limit raise (10-12 HP)"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1♠", "R:Dbl") || z.Es("O:1♥", "R:Dbl"))
            {
                string m = z.Hepsi[0] == "O:1♠" ? "Maça" : "Kupa";
                return (z.Uz(m) >= 4 && z.Hp >= 10 && z.Hp <= 12) ? "2NT" : null;
            }
            if (z.Es("B:1♠", "L:Dbl", "O:2NT") || z.Es("B:1♥", "L:Dbl", "O:2NT"))
            {
                string m = z.Hepsi[0] == "B:1♠" ? "Maça" : "Kupa";
                return z.Hp >= 15 ? YK.T(4, m) : YK.T(3, m);
            }
            return null;
        }
    }
}
