using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoDiamond
{
    /// <summary>2♦ AÇILIŞI (SAS kitabı s.64): 18-20 HCP dengeli, 5'li majör yok (2NT aralığı dışında).</summary>
    public class TwoDiamond_Opening : IAcilisGelisimi
    {
        public int Oncelik => 3;
        public string Aciklama { get; private set; } = "2♦: 18-20 HCP dengeli, 5'li majör yok";

        public bool UygunMu(List<Card> el, IhaleDurumu durum)
        {
            if (durum == null || el == null || el.Count != 13) return false;
            if (!durum.IlkTeklifMi()) return false;
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 18 || hp > 20) return false;
            if (!ElDegerlendirici.DengeliEl(el)) return false;
            if (ElDegerlendirici.BesliMajorVar(el)) return false;
            if (NTA.Acilis2NT(el, durum.Anlasma) || NTA.Acilis1NT(el, durum.Anlasma)) return false;
            return true;
        }

        public string TeklifVer(List<Card> el, IhaleDurumu durum) { return "2♦"; }
    }
}
