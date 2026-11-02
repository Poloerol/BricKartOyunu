using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Inverted Bergen Raises: 1M–3♣ = 4'lü destek 10-12 HP (limit), 1M–3♦ = 4'lü destek 7-9 HP.</summary>
    internal sealed class InvertedBergenRaises : YeniKonvTaban
    {
        public override string Ad { get { return "Inverted Bergen Raises"; } }
        public override string Aciklama { get { return "1M–3♣: limit (10-12), 1M–3♦: kurucu (7-9), ikisi de 4'lü destek"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 1 && z.Hepsi[0].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                if (!YK.Major(m) || z.Uz(m) < 4) return null;
                if (z.Hp >= 10 && z.Hp <= 12) return "3♣";
                if (z.Hp >= 7 && z.Hp <= 9) return "3♦";
                return null;
            }
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[1].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                string c = YK.Cagri(z.Hepsi[1]);
                if (!YK.Major(m) || (c != "3♣" && c != "3♦")) return null;
                if (c == "3♣") return z.Hp >= 14 ? YK.T(4, m) : YK.T(3, m);
                return z.Hp >= 17 ? YK.T(4, m) : YK.T(3, m);
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("B:") && z.Hepsi[2].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                string b = YK.Cagri(z.Hepsi[1]);
                if (!YK.Major(m) || (b != "3♣" && b != "3♦")) return null;
                if (YK.Cagri(z.Hepsi[2]) == YK.T(3, m)) return "Pas";
            }
            return null;
        }
    }
}
