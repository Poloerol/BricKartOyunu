using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Walsh: 1♣ açılışına 4♥+4♠ ile 1♥; tek 4'lü majör, 5'li majör yok, 4+♦ ve 6-10 HP ile majör atlanıp 1♦ denir.</summary>
    internal sealed class Walsh : YeniKonvTaban
    {
        public override string Ad { get { return "Walsh"; } }
        public override string Aciklama { get { return "1♣–1♦ majör atlayarak (4+♦, 6-10 HP); 4-4 majörde 1♥"; } }
        protected override string Karar(Dz z)
        {
            if (!z.Es("O:1♣") || z.Hp < 6) return null;
            if (z.Maca >= 5 || z.Kupa >= 5) return null;
            if (z.Maca == 4 && z.Kupa == 4) return "1♥";
            bool birDortlu = (z.Maca == 4) != (z.Kupa == 4);
            if (birDortlu && z.Karo >= 4 && z.Hp <= 10) return "1♦";
            return null;
        }
    }
}
