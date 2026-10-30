using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Mini-Roman 2♦: ♦ tekil/void, ♠♥♣ 4'lü (11-15 HP); cevaplayan en uzun renge geçer/düzeltir, açıcı geçer.</summary>
    internal sealed class MiniRoman2D : YeniKonvTaban
    {
        public override string Ad { get { return "Mini-Roman 2D"; } }
        public override string Aciklama { get { return "2♦ açılışı: ♦ tekil/void, ♠♥♣ 4+ (11-15 HP); cevap: geç/düzelt"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 0)
                return (z.Hp >= 11 && z.Hp <= 15 && z.Karo <= 1 && z.Maca >= 4 && z.Kupa >= 4 && z.Sinek >= 4) ? "2♦" : null;
            if (z.Es("O:2♦"))
            {
                int s = z.Maca, h = z.Kupa, c = z.Sinek;
                if (h >= s && h >= c) return "2♥";
                if (s >= c) return "2♠";
                return "3♣";
            }
            if (z.Es("B:2♦", "O:2♥") || z.Es("B:2♦", "O:2♠") || z.Es("B:2♦", "O:3♣")) return "Pas";
            return null;
        }
    }
}
