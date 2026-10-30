using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Weak Jump Overcalls: (1x) sonrası sıçramalı 2♦/2♥/2♠ = 6 kartlık iyi renk, 5-9 HP; ilerletici destekle yükseltir.</summary>
    internal sealed class WeakJumpOvercalls : YeniKonvTaban
    {
        public override string Ad { get { return "Weak Jump Overcalls"; } }
        public override string Aciklama { get { return "(1x)–2y sıçrama: 6 kart, 5-9 HP, iyi renk"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 1 && z.Hepsi[0].StartsWith("R:"))
            {
                string x = YK2.BirRenk(z.Hepsi[0], "R");
                if (x == null || z.Hp < 5 || z.Hp > 9) return null;
                foreach (var y in new[] { "Maça", "Kupa", "Karo" })
                    if (YK2.Sira(y) > YK2.Sira(x) && z.Uz(y) == 6 && YK.Hcp(z.El, y) >= 3) return YK.T(2, y);
                return null;
            }
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("L:") && z.Hepsi[1].StartsWith("O:"))
            {
                string x = YK2.BirRenk(z.Hepsi[0], "L");
                string c = YK.Cagri(z.Hepsi[1]);
                string y = YK.KozOf(c);
                if (x == null || y == null || y == "NT" || YK.Sev(c) != 2 || YK2.Sira(y) <= YK2.Sira(x)) return null;
                if (z.Uz(y) < 3) return null;
                if (YK.Major(y)) return z.Hp >= 13 ? YK.T(4, y) : YK.T(3, y);
                return z.Hp >= 14 ? "3NT" : "3♦";
            }
            return null;
        }
    }
}
