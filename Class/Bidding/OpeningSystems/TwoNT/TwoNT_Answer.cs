using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoNT
{
    /// <summary>2NT açışına cevaplar (SAS s.72): 3♣ Stayman; 3♦/3♥ transfer ♥/♠; 3♠ transfer ♣/minörler; 3NT oyun; 4♣ transfer ♦; 4♦ 5/5 majör; 4♥/4♠ 5/4 minör SNG; 4NT niceliksel.</summary>
    public class TwoNT_Answer : IKonvansiyon
    {
        public string Ad => "2NT Cevap Sistemi";
        public string Aciklama => "2NT açışına cevaplar (SAS s.72): 3♣ Stayman; 3♦/3♥ transfer ♥/♠; 3♠ transfer ♣/minörler; 3NT oyun; 4♣ transfer ♦; 4♦ 5/5 majör; 4♥/4♠ 5/4 minör SNG; 4NT niceliksel.";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 5;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.SonGercekTeklif != "2NT") return false;
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
            // 5+ majör: transfer (zayıf elle de, oynamak için)
            if (hcp >= 11 && e.S >= 5 && e.H >= 5) return "4♦";                          // 5/5 majör
            if (hcp >= 4 && (e.S >= 6 || e.H >= 6) || (e.S >= 5 || e.H >= 5))
            {
                if (hcp >= 11 && e.H == 1 && (e.C >= 5 || e.D >= 5)) return "4♥";        // 5/4 minör SNG ♥
                if (e.H >= 5 && e.H >= e.S) return "3♦";                                  // transfer ♥
                return "3♥";                                                              // transfer ♠
            }
            if (hcp >= 11 && e.H == 1 && (e.C >= 5 || e.D >= 5)) return "4♥";
            if (hcp >= 11 && e.S == 1 && (e.C >= 5 || e.D >= 5)) return "4♠";
            if (hcp >= 4 && (e.S == 4 || e.H == 4)) return "3♣";                         // Stayman
            if (hcp >= 11 && e.D >= 6) return "4♣";                                      // transfer ♦
            if (hcp >= 11 && (e.C >= 6 || (e.C >= 5 && e.D >= 5))) return "3♠";          // minör transferi
            if (hcp >= 11 && e.Dengeli) return "4NT";                                     // niceliksel
            if (hcp >= 4) return "3NT";
            return "Pas";
        }
    }
}
