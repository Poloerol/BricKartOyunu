using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Ghestem: (1x) sonrası iki renkli (5-5) eller: cue-bid = kalan iki yüksek renk, 2NT = iki alçak, 3♣ = en alçak + en yüksek.</summary>
    internal sealed class Ghestem : YeniKonvTaban
    {
        public override string Ad { get { return "Ghestem"; } }
        public override string Aciklama { get { return "Cue = iki yüksek, 2NT = iki alçak, 3♣ = alçak+yüksek (kalan üç renkten)"; } }

        private static List<string> Kalan(string x)
        {
            return new[] { "Sinek", "Karo", "Kupa", "Maça" }.Where(r => r != x).ToList();   // alçaktan yükseğe
        }

        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 1 && z.Hepsi[0].StartsWith("R:"))
            {
                string x = YK2.BirRenk(z.Hepsi[0], "R");
                if (x == null || z.Hp < 6) return null;
                var u = Kalan(x);   // u[0] < u[1] < u[2]
                if (z.Uz(u[1]) >= 5 && z.Uz(u[2]) >= 5) return YK.T(2, x);   // iki yüksek
                if (z.Uz(u[0]) >= 5 && z.Uz(u[1]) >= 5) return "2NT";        // iki alçak
                if (z.Uz(u[0]) >= 5 && z.Uz(u[2]) >= 5) return "3♣";         // alçak + yüksek
                return null;
            }
            // İlerletici: ortak iki renk gösterdi
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("L:") && z.Hepsi[1].StartsWith("O:"))
            {
                string x = YK2.BirRenk(z.Hepsi[0], "L");
                if (x == null) return null;
                string c = YK.Cagri(z.Hepsi[1]);
                var u = Kalan(x);
                string a, b;
                if (c == YK.T(2, x)) { a = u[1]; b = u[2]; }
                else if (c == "2NT") { a = u[0]; b = u[1]; }
                else if (c == "3♣") { a = u[0]; b = u[2]; }
                else return null;
                string sec = z.Uz(a) > z.Uz(b) ? a : (z.Uz(b) > z.Uz(a) ? b : (YK2.Sira(a) > YK2.Sira(b) ? a : b));
                if (c == "3♣" && sec == "Sinek") return "Pas";
                for (int lv = 2; lv <= 5; lv++)
                    if (YK2.Yuksek(YK.T(lv, sec), c)) return YK.T(lv, sec);
                return null;
            }
            return null;
        }
    }
}
