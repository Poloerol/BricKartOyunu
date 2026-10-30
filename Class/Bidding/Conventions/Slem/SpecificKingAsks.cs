using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Specific King Asks: RKCB (4NT) cevabından sonra 5 seviyesinde yan renk teklifi = o renkte papaz sorusu; 6M = var, 5M = yok.</summary>
    internal sealed class SpecificKingAsks : YeniKonvTaban
    {
        public override string Ad { get { return "Specific King Asks"; } }
        public override string Aciklama { get { return "4NT–5x sonrası 5y: y papazı sorusu; 6M = var, 5M = yok"; } }
        public override int Oncelik { get { return 5; } }

        protected override string Karar(Dz z)
        {
            string koz = z.D.AnlasilanKoz();
            if (!YK.Major(koz)) return null;
            // Soran: 4NT'm ve ortağın 5. seviye cevabı
            if (z.Hepsi.Count >= 2)
            {
                int n = z.Hepsi.Count;
                if (z.Hepsi[n - 2] == "B:4NT" && z.Hepsi[n - 1].StartsWith("O:5") && YK.KozOf(YK.Cagri(z.Hepsi[n - 1])) != "NT")
                {
                    string rep = YK.Cagri(z.Hepsi[n - 1]);
                    int kk = ElDegerlendirici.KilitKartSayisi(z.El, koz);
                    if (kk < 3 || z.Hp < 15) return null;
                    foreach (var r in new[] { "Sinek", "Karo", "Kupa", "Maça" })
                    {
                        if (r == koz || z.El.Any(c => c.Suit == r && c.Value == 13)) continue;
                        string ask = YK.T(5, r);
                        if (!YK2.Yuksek(ask, rep) || !YK2.Yuksek(YK.T(5, koz), ask)) continue;
                        if (z.Uz(r) == 0) continue;
                        return ask;
                    }
                    return null;
                }
                // Cevaplayan: ortağın 4NT'si, benim cevabım, ortağın 5y sorusu
                if (n >= 3 && z.Hepsi[n - 3] == "O:4NT" && z.Hepsi[n - 2].StartsWith("B:5") && z.Hepsi[n - 1].StartsWith("O:5"))
                {
                    string y = YK.KozOf(YK.Cagri(z.Hepsi[n - 1]));
                    if (y == null || y == "NT" || y == koz) return null;
                    bool papaz = z.El.Any(c => c.Suit == y && c.Value == 13);
                    return papaz ? YK.T(6, koz) : YK.T(5, koz);
                }
                // Soran, cevap aldı
                if (n >= 4 && z.Hepsi[n - 4] == "B:4NT" && z.Hepsi[n - 2].StartsWith("B:5") && z.Hepsi[n - 1].StartsWith("O:"))
                {
                    string c = YK.Cagri(z.Hepsi[n - 1]);
                    if (c == YK.T(6, koz) || c == YK.T(5, koz)) return "Pas";
                }
            }
            return null;
        }
    }
}
