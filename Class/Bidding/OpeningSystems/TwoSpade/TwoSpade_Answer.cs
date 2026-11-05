using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoSpade
{
    /// <summary>2♠ açışına cevaplar (SAS s.71): 2NT röle / ♠ davet+; 3♣ 5+♥ F1R; 3♦ 5+♦ F1R; 3♥ 5+♣ F1R; 3♠/3NT oyun; 4♣/4♦/4♥ 5+ renk 3+♠ FG.</summary>
    public class TwoSpade_Answer : IKonvansiyon
    {
        public string Ad => "2♠ Cevap Sistemi";
        public string Aciklama => "2♠ açışına cevaplar (SAS s.71): 2NT röle / ♠ davet+; 3♣ 5+♥ F1R; 3♦ 5+♦ F1R; 3♥ 5+♣ F1R; 3♠/3NT oyun; 4♣/4♦/4♥ 5+ renk 3+♠ FG.";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 5;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.SonGercekTeklif != "2♠") return false;
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;
            if (durum.Gecmis.Count(h => h.GercekTeklifMi) != 1) return false;
            if (durum.RakipTeklifleri.Any(t => !string.IsNullOrEmpty(t) && t != "Pas" && t != "Dbl" && t != "RDbl"))
                return false;
            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";
            var e = new OcEl(el);
            int hcp = e.Hcp;
            if (e.S >= 3 && hcp >= 14) return "4♠";
            if (e.S >= 3 && hcp >= 12 && e.C >= 5) return "4♣";
            if (e.S >= 3 && hcp >= 12 && e.D >= 5) return "4♦";
            if (e.S >= 3 && hcp >= 12 && e.H >= 5) return "4♥";
            if (e.S >= 3 && hcp <= 5) return e.S >= 4 ? "3♠" : "Pas";                    // önleyici raise
            if (hcp >= 10 && e.S >= 2) return "2NT";                                     // röle / davet
            if (hcp >= 11 && e.H >= 5) return "3♣";                                      // F1R
            if (hcp >= 11 && e.D >= 5) return "3♦";
            if (hcp >= 11 && e.C >= 5) return "3♥";
            if (hcp >= 14 && e.Dengeli) return "3NT";
            if (e.S >= 3 && hcp >= 6) return "3♠";
            return "Pas";
        }
    }
}
