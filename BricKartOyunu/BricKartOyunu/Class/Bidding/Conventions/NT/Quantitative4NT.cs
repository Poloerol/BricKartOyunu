using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Nicel (quantitative) 4NT: 1NT (15-17) sonrası 4NT = 6NT daveti, dengeli 16-17 HP; as sorusu DEĞİLDİR.
    /// Açıcı 17 HP ile (ya da 16 HP + 5 kartlık renk ile) 6NT der, aksi halde pas geçer.
    /// 2NT (20-21) sonrası 4NT = 11-12 HP dengeli; açıcı 21 HP ile 6NT der, 20 ile pas geçer.
    /// Yalnızca ortağın NT açılışından hemen sonra (rakip susarken) geçerlidir.
    /// </summary>
    internal sealed class Quantitative4NT : YeniKonvTaban
    {
        public override string Ad { get { return "Quantitative 4NT"; } }
        public override string Aciklama { get { return "1NT–4NT: 16-17 HP dengeli, 6NT daveti (as sorusu değil); 2NT–4NT: 11-12 HP"; } }
        protected override string Karar(Dz z)
        {
            bool guclu = z.D.Anlasma == null || z.D.Anlasma.StrongNT;
            bool dengeli = ElDegerlendirici.DengeliEl(z.El);
            // Cevapçı
            if (z.Es("O:1NT")) return guclu && dengeli && (z.Hp == 16 || z.Hp == 17) ? "4NT" : null;
            if (z.Es("O:2NT")) return dengeli && (z.Hp == 11 || z.Hp == 12) ? "4NT" : null;
            // Açıcı
            if (z.Es("B:1NT", "O:4NT"))
            {
                if (z.Hp >= 17) return "6NT";
                int enUzun = Math.Max(Math.Max(z.Maca, z.Kupa), Math.Max(z.Karo, z.Sinek));
                if (z.Hp == 16 && enUzun >= 5) return "6NT";
                return "Pas";
            }
            if (z.Es("B:2NT", "O:4NT")) return z.Hp >= 21 ? "6NT" : "Pas";
            return null;
        }
    }
}
