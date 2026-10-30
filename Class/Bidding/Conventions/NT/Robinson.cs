using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Robinson: ortağın 1NT'si kontra edilince 2♣ Stayman, 2♦/2♥ transfer, S.Kontra = ♣ transferi.</summary>
    internal sealed class Robinson : YeniKonvTaban
    {
        public override string Ad { get { return "Robinson"; } }
        public override string Aciklama { get { return "1NT–(Dbl) sonrası: 2♣ Stayman, 2♦/2♥ majör transferi, RDbl = ♣ transferi"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1NT", "R:Dbl"))
            {
                if (z.Kupa >= 5 && z.Kupa >= z.Maca) return "2♦";
                if (z.Maca >= 5) return "2♥";
                if ((z.Maca >= 4 || z.Kupa >= 4) && z.Hp >= 8) return "2♣";
                if (z.Sinek >= 5 && z.Hp < 8) return "RDbl";
                return null;
            }
            if (z.Es("B:1NT", "L:Dbl", "O:2♦")) return "2♥";
            if (z.Es("B:1NT", "L:Dbl", "O:2♥")) return "2♠";
            if (z.Es("B:1NT", "L:Dbl", "O:RDbl")) return "2♣";
            if (z.Es("B:1NT", "L:Dbl", "O:2♣")) return z.Kupa >= 4 ? "2♥" : z.Maca >= 4 ? "2♠" : "2♦";
            return null;
        }
    }
}
