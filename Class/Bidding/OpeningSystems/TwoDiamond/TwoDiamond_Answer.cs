using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoDiamond
{
    /// <summary>2♦ açışına cevaplar (SAS s.64): Pas 4+♦ SO; 2♥ 4+♠; 2♠ 4+♥ / dengeli; 2NT transfer 3♣; 3♣ 4'lü majör sorar; 3♦ 5♠4♥ FG; 3♥/3♠ 5/4+ minör SNG; 3NT 5/5 majör.</summary>
    public class TwoDiamond_Answer : IKonvansiyon
    {
        public string Ad => "2♦ Açılış Sistemi";
        public string Aciklama => "2♦ açışına cevaplar (SAS s.64): Pas 4+♦ SO; 2♥ 4+♠; 2♠ 4+♥ / dengeli; 2NT transfer 3♣; 3♣ 4'lü majör sorar; 3♦ 5♠4♥ FG; 3♥/3♠ 5/4+ minör SNG; 3NT 5/5 majör.";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 5;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.SonGercekTeklif != "2♦") return false;
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
            bool zayif = hcp <= 6, oyun = hcp >= 7 && hcp <= 12, si = hcp >= 13;
            bool minor6 = e.C >= 6 || e.D >= 6;
            if (zayif)
            {
                if (e.D >= 4) return "Pas";                                               // 2♦ oyna
                if (e.S >= 5) return "2♥";                                                // 5+♠ SO
                if (e.H >= 5) return "2NT";                                               // transfer 3♣ → 3♥ SO
                if (e.C >= 5) return "2NT";                                               // 5+♣ SO
                if (hcp >= 5 && e.Dengeli) return "2♠";                                   // dengeli, 2NT'de kal
                return "Pas";
            }
            if (e.S >= 5 && e.H >= 5) return oyun ? "3NT" : "2NT";
            if (e.S >= 5 && e.H == 4) return oyun ? "3♦" : "2♥";
            if (e.H >= 5 && e.S == 4) return oyun ? "3♣" : "3♣";                          // 5♥4♠: Stayman
            if (oyun)
            {
                if (e.S >= 4) return "2♥";
                if (e.H >= 4) return "2♠";
                if (e.H == 1 && (e.C >= 4 || e.D >= 4) && (e.C >= 5 || e.D >= 5)) return "3♥";
                if (e.S == 1 && (e.C >= 4 || e.D >= 4) && (e.C >= 5 || e.D >= 5)) return "3♠";
                return "2♠";                                                              // dengeli / 4'lü majör yok
            }
            // slam daveti (13+)
            if (minor6 || (e.C >= 5 && e.D >= 5)) return "2NT";
            if ((e.C >= 5 || e.D >= 5) && (e.S == 4 || e.H == 4)) return "3♣";
            if (e.S >= 4) return "2♥";
            if (e.H >= 4) return "2♠";
            return "2NT";                                                                 // dengeli SI

        }
    }
}
