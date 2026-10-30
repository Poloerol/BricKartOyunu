using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Extended Stayman: 1NT'ye karşı zayıf (0-7 HP) 4-4 majörlü el 2♣ ile başlar; 2♦ cevabına 2♥ (geçer/düzelt) der.</summary>
    internal sealed class ExtendedStayman : YeniKonvTaban
    {
        public override string Ad { get { return "Extended Stayman"; } }
        public override string Aciklama { get { return "1NT–2♣ ile zayıf 4-4 majör; 2♦ sonrası 2♥ bırakma teklifi"; } }
        protected override string Karar(Dz z)
        {
            bool dortDort = z.Maca == 4 && z.Kupa == 4 && z.Hp <= 7;
            if (z.Es("O:1NT")) return dortDort ? "2♣" : null;
            if (!dortDort && !z.Es("B:1NT", "O:2♣", "B:2♦", "O:2♥")) return null;
            if (z.Es("O:1NT", "B:2♣", "O:2♦")) return "2♥";
            if (z.Es("O:1NT", "B:2♣", "O:2♥") || z.Es("O:1NT", "B:2♣", "O:2♠")) return "Pas";
            if (z.Es("B:1NT", "O:2♣", "B:2♦", "O:2♥")) return "Pas";   // açıcı: ortağın zayıf bırakması
            return null;
        }
    }
}
