using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Gambling 3NT: sağlam (AKQ) 7+ kartlık minör, dışarıda en çok bir A/K; ortak durdurucu yoksa 4♣ (geç/düzelt) der.</summary>
    internal sealed class Gambling3NT : YeniKonvTaban
    {
        public override string Ad { get { return "Gambling 3NT"; } }
        public override string Aciklama { get { return "3NT açılışı: sağlam 7+ minör, dışarıda A/K yok (en çok bir)"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 0)
            {
                foreach (var m in new[] { "Sinek", "Karo" })
                {
                    if (z.Uz(m) < 7 || !YK.AKQ(z.El, m)) continue;
                    int disAK = z.El.Count(c => c.Suit != m && c.Value >= 13);
                    if (disAK <= 1 && z.Hp <= 13) return "3NT";
                }
                return null;
            }
            if (z.Es("O:3NT"))
            {
                int dur = YK.Renk.Count(r => ElDegerlendirici.StopperVar(z.El, r));
                return dur >= 3 ? "Pas" : "4♣";
            }
            if (z.Es("B:3NT", "O:4♣")) return z.Sinek >= 7 ? "Pas" : "5♦";
            return null;
        }
    }
}
