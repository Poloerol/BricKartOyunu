using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Yeni konvansiyon adlarının listesi (koruma katmanının güç tahmini denetiminden muaf tutulurlar).</summary>
    public static class YeniKonvansiyonlar
    {
        public static readonly string[] Adlar =
        {
            "Benjamin Two-Bids", "Extended Stayman", "Hello over 1NT", "Truscott 2NT", "Preemptive Reraises",
            "Long & Short Game Tries", "New Minor Forcing", "Weak Jump-Shift Resp.", "Ogust", "Gambling 3NT",
            "Specific King Asks", "Voluntary Five Major Bid", "Mathe Asking Bids", "Control Asking Bids", "Ghestem",
            "Invisible Cue-Bids", "Limit-Plus Cue-Bids", "Weak Jump Overcalls", "Sandwich 1NT", "Maximal Overcall Dbl.",
            "1m-2NT Invitational", "German 2C Relays", "Walsh", "Mini-Roman 2D", "Robinson",
            "Inverted Bergen Raises", "Exit Transfer 1NT Runouts", "Artif. 2C/2D Resp. to 1M-X", "Wolff Signoff",
            "Quantitative 4NT", "Texas Transfers", "Minor Stayman", "Flannery", "Kokish", "Rubensohl", "Leaping Michaels", "Lightner Double", "Last Train", "2/1 Game Force"
        };
        public static bool Mu(string ad) { return ad != null && Array.IndexOf(Adlar, ad) >= 0; }
    }
}
