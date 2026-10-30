using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Sandwich 1NT: (1x)–Pas–(1y)–1NT = kullanılmamış iki rengi gösterir (5-4); ilerletici uzun olanı seçer.</summary>
    internal sealed class Sandwich1NT : YeniKonvTaban
    {
        public override string Ad { get { return "Sandwich 1NT"; } }
        public override string Aciklama { get { return "(1x)–Pas–(1y) sonrası 1NT: kalan iki renk (5-4+), 8-15 HP"; } }

        private static List<string> Bos(string x, string y)
        {
            return YK.Renk.Where(r => r != x && r != y).ToList();
        }

        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("L:") && z.Hepsi[1].StartsWith("R:"))
            {
                string a = YK.Cagri(z.Hepsi[0]), b = YK.Cagri(z.Hepsi[1]);
                string x = YK.KozOf(a), y = YK.KozOf(b);
                if (YK.Sev(a) != 1 || YK.Sev(b) != 1 || x == null || y == null || x == "NT" || y == "NT" || x == y) return null;
                if (z.Hp < 8 || z.Hp > 15) return null;
                var bos = Bos(x, y);
                int u1 = z.Uz(bos[0]), u2 = z.Uz(bos[1]);
                if (Math.Max(u1, u2) >= 5 && Math.Min(u1, u2) >= 4) return "1NT";
                return null;
            }
            // İlerletici (ortak 1NT dedi)
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("R:") && z.Hepsi[1].StartsWith("L:") && z.Es(z.Hepsi[0], z.Hepsi[1], "O:1NT"))
            {
                string a = YK.Cagri(z.Hepsi[0]), b = YK.Cagri(z.Hepsi[1]);
                string x = YK.KozOf(a), y = YK.KozOf(b);
                if (YK.Sev(a) != 1 || YK.Sev(b) != 1 || x == null || y == null || x == "NT" || y == "NT" || x == y) return null;
                var bos = Bos(x, y);
                string sec = z.Uz(bos[0]) >= z.Uz(bos[1]) ? bos[0] : bos[1];
                if (z.Uz(sec) < 3) return null;
                return YK.T(2, sec);
            }
            return null;
        }
    }
}
