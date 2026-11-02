using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Voluntary Five Major Bid: ortağın majör rengine karşı rakip 4 majör deyince, 4+ destekli, rakip rengi kısa, ≤9 HP el 5M ile fedakârlık yapar.</summary>
    internal sealed class VoluntaryFiveMajor : YeniKonvTaban
    {
        public override string Ad { get { return "Voluntary Five Major Bid"; } }
        public override string Aciklama { get { return "(4M) karşısında ortağın majörüne 4+ destek, rakip rengi kısa, ≤9 HP: 5M fedakârlığı"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count != 2 || !z.Hepsi[0].StartsWith("O:") || !z.Hepsi[1].StartsWith("R:")) return null;
            string y = YK2.BirRenk(z.Hepsi[0], "O");
            string c = YK.Cagri(z.Hepsi[1]);
            string m = YK.KozOf(c);
            if (!YK.Major(y) || !YK.Major(m) || m == y || c != YK.T(4, m)) return null;
            return (z.Uz(y) >= 4 && z.Uz(m) <= 1 && z.Hp <= 9) ? YK.T(5, y) : null;
        }
    }
}
