using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Maximal Overcall Double: 1M–2M raise, rakip 3 seviyesinde alt bir renk söyleyince açıcının kontrası oyun daveti (15+ HP).</summary>
    internal sealed class MaximalOvercallDbl : YeniKonvTaban
    {
        public override string Ad { get { return "Maximal Overcall Dbl."; } }
        public override string Aciklama { get { return "1M–2M–(3x alt renk)–Dbl: oyun daveti; maks → 4M, min → 3M"; } }
        protected override string Karar(Dz z)
        {
            // Açıcı
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[2].StartsWith("R:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                if (!YK.Major(m) || z.Hepsi[1] != "O:" + YK.T(2, m)) return null;
                string c = YK.Cagri(z.Hepsi[2]); string x = YK.KozOf(c);
                if (YK.Sev(c) != 3 || x == null || x == "NT" || YK2.Sira(x) >= YK2.Sira(m)) return null;
                return z.Hp >= 15 ? "Dbl" : null;
            }
            // Raise eden
            if (z.Hepsi.Count == 4 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("B:") &&
                z.Hepsi[2].StartsWith("L:") && z.Hepsi[3] == "O:Dbl")
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                if (!YK.Major(m) || z.Hepsi[1] != "B:" + YK.T(2, m)) return null;
                string c = YK.Cagri(z.Hepsi[2]); string x = YK.KozOf(c);
                if (YK.Sev(c) != 3 || x == null || x == "NT" || YK2.Sira(x) >= YK2.Sira(m)) return null;
                return z.Hp >= 8 ? YK.T(4, m) : YK.T(3, m);
            }
            return null;
        }
    }
}
