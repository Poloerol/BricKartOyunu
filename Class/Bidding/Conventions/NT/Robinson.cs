using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Robinson: ortağın 1NT'si kontra edilince 2♣ Stayman, 2♦/2♥ transfer, S.Kontra = ♣ transferi.</summary>
    internal sealed class Robinson : YeniKonvTaban
    {
        public override string Ad { get { return "Robinson"; } }
        public override string Aciklama { get { return "1NT–(Dbl) sonrası: 2♣ Stayman, 2♦/2♥ majör transferi, RDbl = ♣ transferi"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1NT", "R:Dbl"))
            {
                if (z.Kupa >= 5 && z.Kupa >= z.Maca) return "2♦";
                if (z.Maca >= 5) return "2♥";
                if ((z.Maca >= 4 || z.Kupa >= 4) && z.Hp >= 8) return "2♣";
                if (z.Sinek >= 5 && z.Hp < 8) return "RDbl";
                return null;
            }
            if (z.Es("B:1NT", "L:Dbl", "O:2♦")) return "2♥";
            if (z.Es("B:1NT", "L:Dbl", "O:2♥")) return "2♠";
            if (z.Es("B:1NT", "L:Dbl", "O:RDbl")) return "2♣";
            if (z.Es("B:1NT", "L:Dbl", "O:2♣")) return z.Kupa >= 4 ? "2♥" : z.Maca >= 4 ? "2♠" : "2♦";
            return null;
        }
    }

    /// <summary>
    /// Hardy: rakibin 1NT'mize yaptığı 2♣ müdahalesi YAPAY ise (Cappelletti / Multi-Landy) cevapçının kontrası Stayman'dır
    /// ve sistem aynen sürer. Doğal 2♣ için Lebensohl geçerlidir (bu sınıf konuşmaz).
    /// </summary>
    internal sealed class ArtifisyelIkiSinekStayman : YeniKonvTaban
    {
        public override string Ad { get { return "Yapay 2♣ Sonrası Kontra Stayman"; } }
        public override string Aciklama { get { return "1NT–(yapay 2♣): Dbl = Stayman; açıcı 2♥/2♠/2♦ ile cevap verir"; } }
        protected override string Karar(Dz z)
        {
            var an = z.D.Anlasma;
            if (an == null || !(an.Cappelletti || an.MultiLandy)) return null;
            if (z.Es("O:1NT", "R:2♣"))
                return (z.Hp >= 8 && (z.Maca >= 4 || z.Kupa >= 4)) ? "Dbl" : null;
            // Rakip araya Cappelletti rölesi (2♦) soktuysa Stayman cevabı aynen sürer
            if (z.Es("B:1NT", "L:2♣", "O:Dbl") || z.Es("B:1NT", "L:2♣", "O:Dbl", "R:2♦"))
                return z.Kupa >= 4 ? "2♥" : z.Maca >= 4 ? "2♠" : "2♦";
            return null;
        }
    }
}
