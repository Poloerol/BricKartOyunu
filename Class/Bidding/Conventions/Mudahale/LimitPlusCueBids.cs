using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Limit-Plus Cue-Bids: 1M–(rakip rengi c)–cue-bid = 3+ destekle limit raise ya da daha iyisi (10+ HP).</summary>
    internal sealed class LimitPlusCueBids : YeniKonvTaban
    {
        public override string Ad { get { return "Limit-Plus Cue-Bids"; } }
        public override string Aciklama { get { return "1M–(c)–cue-bid: 3+ destekle limit raise ya da üstü"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("R:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                string c = YK.Cagri(z.Hepsi[1]); string x = YK.KozOf(c);
                if (!YK.Major(m) || x == null || x == "NT" || x == m || YK.Sev(c) > 2) return null;
                return (z.Uz(m) >= 3 && z.Hp >= 10) ? YK.T(YK.Sev(c) + 1, x) : null;
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[1].StartsWith("L:") && z.Hepsi[2].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                string c = YK.Cagri(z.Hepsi[1]); string x = YK.KozOf(c);
                if (!YK.Major(m) || x == null || x == "NT" || x == m || YK.Sev(c) > 2) return null;
                string cue = YK.Cagri(z.Hepsi[2]);
                if (cue != YK.T(YK.Sev(c) + 1, x)) return null;
                bool oyun = z.Hp >= 15 || (z.Hp >= 13 && z.Uz(m) >= 6);
                string uc = YK.T(3, m);
                if (oyun || !YK2.Yuksek(uc, cue)) return YK.T(4, m);
                return uc;
            }
            return null;
        }
    }
}
