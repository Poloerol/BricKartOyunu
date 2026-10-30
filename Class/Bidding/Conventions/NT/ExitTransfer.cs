using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Exit Transfers: 1NT kontra edilince zayıf (0-7 HP) 5+ renkli ele "bir alttaki renk" transferi; RDbl = oynamalık güç.</summary>
    internal sealed class ExitTransfer : YeniKonvTaban
    {
        public override string Ad { get { return "Exit Transfer 1NT Runouts"; } }
        public override string Aciklama { get { return "1NT–(Dbl): 2♣→♦, 2♦→♥, 2♥→♠, 2♠→♣ zayıf kaçış transferleri; RDbl 9+ HP"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1NT", "R:Dbl"))
            {
                if (z.Hp >= 9) return "RDbl";
                if (z.Hp > 7) return null;
                int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
                int en = Math.Max(Math.Max(s, h), Math.Max(d, c));
                if (en < 5) return null;
                if (h == en) return "2♦";
                if (s == en) return "2♥";
                if (d == en) return "2♣";
                return "2♠";   // ♣
            }
            if (z.Es("B:1NT", "L:Dbl", "O:2♣")) return "2♦";
            if (z.Es("B:1NT", "L:Dbl", "O:2♦")) return "2♥";
            if (z.Es("B:1NT", "L:Dbl", "O:2♥")) return "2♠";
            if (z.Es("B:1NT", "L:Dbl", "O:2♠")) return "3♣";
            return null;
        }
    }
}
