using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Exit Transfers: 1NT kontra edilince zayıf (0-7 HP) 5+ renkli ele "bir alttaki renk" transferi; RDbl = oynamalık güç.</summary>
    internal sealed class ExitTransfer : YeniKonvTaban
    {
        public override string Ad { get { return "Exit Transfer 1NT Runouts"; } }
        public override string Aciklama { get { return "1NT–(Dbl): 2♣→♦, 2♦→♥, 2♥→♠, 2♠→♣ zayıf kaçış transferleri; RDbl 9+ HP"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1NT", "R:Dbl"))
            {
                if (z.Hp >= 9) return "RDbl";
                if (z.Hp > 7) return null;
                int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
                int en = Math.Max(Math.Max(s, h), Math.Max(d, c));
                if (en < 5) return null;
                if (h == en) return "2♦";
                if (s == en) return "2♥";
                if (d == en) return "2♣";
                return "2♠";   // ♣
            }
            if (z.Es("B:1NT", "L:Dbl", "O:2♣")) return "2♦";
            if (z.Es("B:1NT", "L:Dbl", "O:2♦")) return "2♥";
            if (z.Es("B:1NT", "L:Dbl", "O:2♥")) return "2♠";
            if (z.Es("B:1NT", "L:Dbl", "O:2♠")) return "3♣";
            return null;
        }
    }
    /// <summary>
    /// Helvic Wriggle: 1NT–(Dbl). Pas = ♣+♥ ya da ♦+♠ (açıcı XX ile röle eder, sonra 2♣/2♦);
    /// XX = tek renk (açıcı 2♣ röle); 2♣ ♣+♦, 2♦ ♦+♥, 2♥ ♥+♠, 2♠ ♣+♠.
    /// </summary>
    internal sealed class Helvic : YeniKonvTaban
    {
        public override string Ad { get { return "Helvic"; } }
        public override string Aciklama { get { return "1NT–(Dbl) kaçışı: Pas = ♣♥ / ♦♠, XX = tek renk, 2♣ ♣♦, 2♦ ♦♥, 2♥ ♥♠, 2♠ ♣♠"; } }
        protected override string Karar(Dz z)
        {
            int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
            if (z.Es("O:1NT", "R:Dbl"))
            {
                if (z.Hp >= 8) return null;
                int en = Math.Max(Math.Max(s, h), Math.Max(c, d));
                if (en >= 5) return "RDbl";
                if (c >= 4 && h >= 4) return "Pas";
                if (d >= 4 && s >= 4) return "Pas";
                if (c >= 4 && d >= 4) return "2♣";
                if (d >= 4 && h >= 4) return "2♦";
                if (h >= 4 && s >= 4) return "2♥";
                if (c >= 4 && s >= 4) return "2♠";
                return null;
            }
            // Açıcı: ortak Pas dedi → XX (röle isteği)
            if (z.Es("B:1NT", "L:Dbl")) return "RDbl";
            if (z.Es("B:1NT", "L:Dbl", "O:RDbl")) return "2♣";
            // Cevaplayan: açıcı XX yaptı (ortak Pas'tan sonra) → 2♣ ya da 2♦
            if (z.Es("O:1NT", "R:Dbl", "O:RDbl")) return (c >= 4 && h >= 4) ? "2♣" : (d >= 4 && s >= 4) ? "2♦" : null;
            // Cevaplayan: XX sonrası açıcının 2♣ rölesi → ♣ ile Pas, yoksa renk
            if (z.Es("O:1NT", "R:Dbl", "B:RDbl", "O:2♣"))
            {
                int en = Math.Max(Math.Max(s, h), Math.Max(c, d));
                if (c == en) return "Pas";
                return s == en ? "2♠" : h == en ? "2♥" : "2♦";
            }
            // Açıcı: ortağın zayıf iki renkli kaçışı
            if (z.Es("B:1NT", "L:Dbl", "O:2♣")) return c >= 3 ? "Pas" : "2♦";    // ♣+♦ ya da ♣+♥ (Pas sonrası 2♣)
            if (z.Es("B:1NT", "L:Dbl", "B:RDbl", "O:2♣")) return c >= 3 ? "Pas" : "2♥";
            if (z.Es("B:1NT", "L:Dbl", "O:2♦")) return d >= 3 ? "Pas" : "2♥";
            if (z.Es("B:1NT", "L:Dbl", "B:RDbl", "O:2♦")) return d >= 3 ? "Pas" : "2♠";
            if (z.Es("B:1NT", "L:Dbl", "O:2♥")) return h >= 3 ? "Pas" : "2♠";
            if (z.Es("B:1NT", "L:Dbl", "O:2♠")) return s >= 3 ? "Pas" : "3♣";
            return null;
        }
    }

    /// <summary>D.O.N.T. tarzı kaçış: 1NT–(Dbl). XX = tek renk (2♣ röle), 2♣ ♣+üst renk, 2♦ ♦+üst renk, 2♥ ♥+♠, 2♠ doğal.</summary>
    internal sealed class DontEscape : YeniKonvTaban
    {
        public override string Ad { get { return "D.O.N.T. Escape"; } }
        public override string Aciklama { get { return "1NT–(Dbl) kaçışı: XX tek renk, 2♣ ♣+üst renk, 2♦ ♦+üst, 2♥ ♥+♠, 2♠ doğal"; } }
        protected override string Karar(Dz z)
        {
            int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
            if (z.Es("O:1NT", "R:Dbl"))
            {
                if (z.Hp >= 8) return null;
                if (h >= 4 && s >= 4) return "2♥";
                if (s >= 5) return "2♠";
                if (c >= 4 && (d >= 4 || h >= 4 || s >= 4) && c <= 5) return "2♣";
                if (d >= 4 && (h >= 4 || s >= 4)) return "2♦";
                if (Math.Max(Math.Max(h, d), c) >= 5) return "RDbl";
                return null;
            }
            if (z.Es("B:1NT", "L:Dbl", "O:RDbl")) return "2♣";
            if (z.Es("B:1NT", "L:Dbl", "O:2♣")) return "2♦";
            if (z.Es("B:1NT", "L:Dbl", "O:2♦")) return "2♥";
            if (z.Es("B:1NT", "L:Dbl", "O:2♥")) return h >= 3 ? "Pas" : "2♠";
            if (z.Es("B:1NT", "L:Dbl", "O:2♠")) return "Pas";
            if (z.Es("O:1NT", "R:Dbl", "B:RDbl", "O:2♣"))
            {
                int en = Math.Max(Math.Max(h, d), c);
                if (c == en) return "Pas";
                return d == en ? "2♦" : "2♥";
            }
            if (z.Es("O:1NT", "R:Dbl", "B:2♣", "O:2♦"))
            { if (d >= 4 && d >= h && d >= s) return "Pas"; return h >= 4 && h >= s ? "2♥" : "2♠"; }
            if (z.Es("O:1NT", "R:Dbl", "B:2♦", "O:2♥"))
            { if (h >= 4 && h >= s) return "Pas"; return "2♠"; }
            return null;
        }
    }
}
