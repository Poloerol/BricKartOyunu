using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Long &amp; Short Game Tries: 1M–2M sonrası açıcının 3. seviyede söylediği yan renk = kısa renk (tekil/void) denemesi; cevaplayan o renkteki boşa giden değerlere göre karar verir.</summary>
    internal sealed class LongShortGameTries : YeniKonvTaban
    {
        public override string Ad { get { return "Long & Short Game Tries"; } }
        public override string Aciklama { get { return "1M–2M–3x: x'te kısalık denemesi; boşa giden değer varsa 3M, yoksa 4M"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("B:1♠", "O:2♠") || z.Es("B:1♥", "O:2♥"))
            {
                string m = z.Hepsi[0] == "B:1♠" ? "Maça" : "Kupa";
                if (z.Hp < 15 || z.Hp > 17 || z.Uz(m) < 5) return null;
                // kısa renk: tekil ya da void (en düşük renk)
                foreach (var r in new[] { "Sinek", "Karo", "Kupa", "Maça" })
                    if (r != m && z.Uz(r) <= 1) return YK.T(3, r);
                return null;
            }
            if (z.Hepsi.Count == 3 && (z.Basi("O:1♠", "B:2♠") || z.Basi("O:1♥", "B:2♥")))
            {
                string m = z.Hepsi[0] == "O:1♠" ? "Maça" : "Kupa";
                string c = YK.Cagri(z.Hepsi[2]);
                string x = YK.KozOf(c);
                if (YK.Sev(c) != 3 || x == null || x == "NT" || x == m) return null;
                int boş = YK.Hcp(z.El, x);
                return (boş <= 1 && z.Hp >= 6) ? YK.T(4, m) : YK.T(3, m);
            }
            return null;
        }
    }
}
