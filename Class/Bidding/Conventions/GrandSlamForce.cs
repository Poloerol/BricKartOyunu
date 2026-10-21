using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    public class GrandSlamForce : IKonvansiyon
    {
        public string Ad => "Grand Slam Force";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 26;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            if (durum.PartnerTeklifleri.Contains("1NT")) return false;

            // Partner 5NT dedi → cevap veriyoruz
            if (durum.SonGercekTeklif == "5NT" && durum.SonGercekTeklifSahibi == durum.Partner)
            {
                // Eğer bu bir Pick-a-Slam ise (partner daha önce 3+ seviyede rebid yaptıysa)
                // Grand Slam Force devreye girmesin.
                bool pickASlamSekansi = durum.PartnerTeklifleri.Any(t => {
                    int sev = KozYardimcisi.TekliftenSeviyeCikar(t);
                    return sev >= 3;
                });

                if (pickASlamSekansi) return false;

                // 4NT veya Kickback sonrası 5NT ise Grand Slam Force'tur
                bool oncekiSoruVar = durum.PartnerTeklifleri.Any(t =>
                    t == "4NT" || t == "4♠" || t == "4♥" || t == "4♦");

                return oncekiSoruVar;
            }

            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 16) return false;

            int seviye = 0;
            if (!string.IsNullOrEmpty(durum.SonGercekTeklif))
                seviye = KozYardimcisi.TekliftenSeviyeCikar(durum.SonGercekTeklif);
            if (seviye < 4) return false;

            if (durum.KendiTeklifleri.Contains("5NT")) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;

            if (durum.SonGercekTeklif == "5NT" && durum.SonGercekTeklifSahibi == durum.Partner)
            {
                int papazSayisi = 0;
                string[] renkler = { "Maça", "Kupa", "Karo", "Sinek" };
                foreach (var renk in renkler)
                {
                    if (ElDegerlendirici.PapazVar(el, renk)) papazSayisi++;
                }

                switch (papazSayisi)
                {
                    case 0:
                    case 3:
                        return "6♣";
                    case 1:
                    case 4:
                        return "6♦";
                    case 2:
                        return "6♥";
                    default:
                        return "6♠";
                }
            }

            return "5NT";
        }
    }
}