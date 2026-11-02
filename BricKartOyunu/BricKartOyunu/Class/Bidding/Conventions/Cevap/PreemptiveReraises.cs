using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Preemptive Reraises: 1M–2M raise sonrası rakip 3 seviyesine kadar yarışırsa, 6+ kozlu ve ≤14 HP açıcı 4M ile önler.</summary>
    internal sealed class PreemptiveReraises : YeniKonvTaban
    {
        public override string Ad { get { return "Preemptive Reraises"; } }
        public override string Aciklama { get { return "1M–2M, rakip yarışınca 6+ kozla önleyici 4M"; } }
        protected override string Karar(Dz z)
        {
            int n = z.Hepsi.Count;
            if (n != 3 && n != 4) return null;
            if (!z.Hepsi[0].StartsWith("B:")) return null;
            string m = YK2.BirRenk(z.Hepsi[0], "B");
            if (!YK.Major(m)) return null;
            string raise = YK.T(2, m);
            string son = z.Hepsi[n - 1];
            if (!son.StartsWith("R:")) return null;
            string ortak = z.Hepsi[n - 2];
            if (ortak != "O:" + raise) return null;
            if (n == 4 && !z.Hepsi[1].StartsWith("L:")) return null;
            string c = YK.Cagri(son);
            if (YK.Sev(c) < 2 || YK.Sev(c) > 3) return null;
            return (z.Uz(m) >= 6 && z.Hp <= 14) ? YK.T(4, m) : null;
        }
    }
}
