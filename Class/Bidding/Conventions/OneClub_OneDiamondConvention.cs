using System.Collections.Generic;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    public static class OneClub_OneDiamondConvention
    {
        public static readonly Dictionary<string, string> Rules = new Dictionary<string, string>
        {
            { "1♥", "4+♣ 4♥ 11-17 HCP → Pas" },
            { "1♠", "Natürel 4+♦ 4♠ FIR" },
            { "2♣", "Röle" },
            { "2♦", "6+♦ → 5-9 HCP" },
            { "2♥", "4+♦ 4♥ davet w/o SNG" },
            { "2♠", "4.Renk 5+♦ 4+♣ SI (SAS uygulanır)" },
            { "2NT", "2NTt → Transfer 3♣" },
            { "3♣", "5+♦ 4/5♣ davet" },
            { "3♦", "6+♦ davet" },
            { "3♥", "4+♦ 4♥ davet w SNG" },
            { "3♠", "4+♦ 4♥ SNG ♠ hafif SI" },
            { "3NT", "4♦-3-3-3 → 12-14 HCP 0-3 kontrol" }
        };
    }
}
