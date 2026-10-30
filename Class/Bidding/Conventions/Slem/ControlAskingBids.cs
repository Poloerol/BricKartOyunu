using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Control Asking Bids: 1M–3M sonrası açıcının 4♣ (♠ kozda 4♦ da) teklifi = o renkte 1. ya da 2. tur kontrol sorusu; 4M = yok, bir üst adım = var.</summary>
    internal sealed class ControlAskingBids : YeniKonvTaban
    {
        public override string Ad { get { return "Control Asking Bids"; } }
        public override string Aciklama { get { return "1M–3M–4♣/4♦: o renkte A/K/tekil/void sorusu; 4M = yok, yukarı adım = var"; } }

        private static string Cevap(string m, string ask)
        {
            // var: ask'ın hemen üstündeki ilk koz-dışı teklif
            if (ask == "4♣") return "4♦";
            return "4♥";   // ask 4♦ (yalnız ♠ kozda)
        }

        protected override string Karar(Dz z)
        {
            if (z.Es("B:1♥", "O:3♥") || z.Es("B:1♠", "O:3♠"))
            {
                string m = z.Hepsi[0] == "B:1♥" ? "Kupa" : "Maça";
                if (z.Hp < 18 || z.Uz(m) < 5) return null;
                bool zayif = (z.Sinek > 0 && YK.AK(z.El, "Sinek") == 0);
                if (YK.Renk.Any(r => r != m && z.Uz(r) >= 4)) return null;   // Mathe bölgesi (yan renk var)
                if (z.Sinek >= 1 && YK.AK(z.El, "Sinek") == 0) return "4♣";
                if (m == "Maça" && z.Karo >= 1 && YK.AK(z.El, "Karo") == 0) return "4♦";
                return null;
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("B:") && z.Hepsi[2].StartsWith("O:"))
            {
                string m = z.Hepsi[0] == "O:1♥" ? "Kupa" : z.Hepsi[0] == "O:1♠" ? "Maça" : null;
                if (m == null || z.Hepsi[1] != "B:" + YK.T(3, m)) return null;
                string ask = YK.Cagri(z.Hepsi[2]);
                if (ask != "4♣" && !(m == "Maça" && ask == "4♦")) return null;
                string r = ask == "4♣" ? "Sinek" : "Karo";
                bool kontrol = z.Uz(r) <= 1 || YK.AK(z.El, r) > 0;
                return kontrol ? Cevap(m, ask) : YK.T(4, m);
            }
            if (z.Hepsi.Count == 4 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[2].StartsWith("B:"))
            {
                string m = z.Hepsi[0] == "B:1♥" ? "Kupa" : z.Hepsi[0] == "B:1♠" ? "Maça" : null;
                if (m == null || z.Hepsi[1] != "O:" + YK.T(3, m)) return null;
                string ask = YK.Cagri(z.Hepsi[2]);
                if (ask != "4♣" && !(m == "Maça" && ask == "4♦")) return null;
                string rep = YK.Cagri(z.Hepsi[3]);
                if (rep == YK.T(4, m)) return "Pas";
                if (rep == Cevap(m, ask)) return z.Hp >= 19 ? "4NT" : YK.T(4, m);
            }
            return null;
        }
    }
}
