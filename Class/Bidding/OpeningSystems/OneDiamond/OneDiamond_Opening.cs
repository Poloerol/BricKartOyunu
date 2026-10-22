using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>
    /// 1♦ AÇIŞI — Polish Club (henüz MinorAcilis'ten birebir taşındı).
    /// </summary>
    public class OneDiamond_Opening : IAcilisGelisimi
    {
        public int Oncelik => 5;
        public string Aciklama { get; private set; } = "";   // ← private set EKLE

        public bool UygunMu(List<Card> el, IhaleDurumu durum)
        {
            if (durum == null || el == null) return false;
            if (el.Count != 13) return false;

            if (!durum.IlkTeklifMi()) return false;

            int hp = ElDegerlendirici.HCP(el);
            if (hp < 12 || hp > 21) return false;

            if (ElDegerlendirici.BesliMajorVar(el)) return false;

            if (hp >= 15 && hp <= 17 && ElDegerlendirici.DengeliEl(el)) return false;

            return TeklifVer(el, durum) == "1♦";
        }

        public string TeklifVer(List<Card> el, IhaleDurumu durum)
        {
            if (el == null) return "Pas";

            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            if (karo >= 4 && karo >= sinek)
            {
                if (hp >= 12 && hp <= 14 && dengeli)
                    Aciklama = $"1♦: Dengeli 12-14 HCP, 4+♦";
                else if (karo >= 5 && !dengeli)
                    Aciklama = $"1♦: Dengesiz {hp} HCP, 5+♦";
                else
                    Aciklama = $"1♦: {hp} HCP, {karo}+♦";
                return "1♦";
            }

            Aciklama = "1♣";
            return "1♣";
        }
    }
}