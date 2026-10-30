using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>German 2♣ Relays: 2♣–2♦–2NT (22-24) sonrası Stayman/transfer sistemi açık: 3♣ Stayman, 3♦/3♥ majör transferi.</summary>
    internal sealed class German2CRelays : YeniKonvTaban
    {
        public override string Ad { get { return "German 2C Relays"; } }
        public override string Aciklama { get { return "2♣–2♦–2NT sonrası: 3♣ Stayman, 3♦→♥, 3♥→♠ transferleri"; } }
        protected override string Karar(Dz z)
        {
            // Cevaplayan
            if (z.Es("O:2♣", "B:2♦", "O:2NT"))
            {
                if (z.Kupa >= 5 && z.Kupa >= z.Maca) return "3♦";
                if (z.Maca >= 5) return "3♥";
                if (z.Kupa >= 4 || z.Maca >= 4) return "3♣";
                return "3NT";
            }
            // Açıcı
            if (z.Es("B:2♣", "O:2♦", "B:2NT", "O:3♣")) return z.Kupa >= 4 ? "3♥" : z.Maca >= 4 ? "3♠" : "3♦";
            if (z.Es("B:2♣", "O:2♦", "B:2NT", "O:3♦")) return "3♥";
            if (z.Es("B:2♣", "O:2♦", "B:2NT", "O:3♥")) return "3♠";
            // Cevaplayanın devamı
            if (z.Es("O:2♣", "B:2♦", "O:2NT", "B:3♣", "O:3♦")) return "3NT";
            if (z.Es("O:2♣", "B:2♦", "O:2NT", "B:3♣", "O:3♥")) return z.Kupa >= 4 ? "4♥" : "3NT";
            if (z.Es("O:2♣", "B:2♦", "O:2NT", "B:3♣", "O:3♠")) return z.Maca >= 4 ? "4♠" : "3NT";
            if (z.Es("O:2♣", "B:2♦", "O:2NT", "B:3♦", "O:3♥")) return "4♥";
            if (z.Es("O:2♣", "B:2♦", "O:2NT", "B:3♥", "O:3♠")) return "4♠";
            return null;
        }
    }
}
