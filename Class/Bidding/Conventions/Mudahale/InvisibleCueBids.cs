using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Invisible Cue-Bids: 1m–(1M) sonrası rakip majörünün cue-bid'i = minörde oyun zorlayan raise (4+ destek, 12+ HP).</summary>
    internal sealed class InvisibleCueBids : YeniKonvTaban
    {
        public override string Ad { get { return "Invisible Cue-Bids"; } }
        public override string Aciklama { get { return "1m–(1M)–2M: minör açılışına oyun zorlayan raise (4+ destek, 12+ HP)"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("R:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                string c = YK.Cagri(z.Hepsi[1]);
                string M = YK.KozOf(c);
                if ((m != "Sinek" && m != "Karo") || YK.Sev(c) != 1 || !YK.Major(M)) return null;
                return (z.Uz(m) >= 4 && z.Hp >= 12) ? YK.T(2, M) : null;
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[1].StartsWith("L:") && z.Hepsi[2].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                string c = YK.Cagri(z.Hepsi[1]); string M = YK.KozOf(c);
                if ((m != "Sinek" && m != "Karo") || YK.Sev(c) != 1 || !YK.Major(M)) return null;
                if (YK.Cagri(z.Hepsi[2]) != YK.T(2, M)) return null;
                return ElDegerlendirici.StopperVar(z.El, M) ? "3NT" : YK.T(3, m);
            }
            return null;
        }
    }
}
