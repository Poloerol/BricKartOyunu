using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Wolff Signoff: 1x–1y–1NT sonrası 2♣ = 2♦'e röle; zayıf, uzun minörlü el oradan oturur.</summary>
    internal sealed class WolffSignoff : YeniKonvTaban
    {
        public override string Ad { get { return "Wolff Signoff"; } }
        public override string Aciklama { get { return "1x–1y–1NT sonrası 2♣: 2♦ röle, zayıf uzun minörle bırakma"; } }

        private static bool Bir(string etiketli, string kim)
        {
            if (!etiketli.StartsWith(kim + ":")) return false;
            string t = YK.Cagri(etiketli);
            return YK.Sev(t) == 1 && YK.KozOf(t) != null && YK.KozOf(t) != "NT";
        }

        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 3 && Bir(z.Hepsi[0], "O") && Bir(z.Hepsi[1], "B") && z.Hepsi[2] == "O:1NT")
                return (z.Hp <= 7 && (z.Karo >= 6 || z.Sinek >= 6)) ? "2♣" : null;
            if (z.Hepsi.Count == 4 && Bir(z.Hepsi[0], "B") && Bir(z.Hepsi[1], "O") && z.Hepsi[2] == "B:1NT" && z.Hepsi[3] == "O:2♣")
                return "2♦";
            if (z.Hepsi.Count == 5 && Bir(z.Hepsi[0], "O") && Bir(z.Hepsi[1], "B") && z.Hepsi[2] == "O:1NT" &&
                z.Hepsi[3] == "B:2♣" && z.Hepsi[4] == "O:2♦")
                return z.Karo >= 6 ? "Pas" : "3♣";
            return null;
        }
    }
}
