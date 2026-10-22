using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    public class KozQSorusu : IKonvansiyon
    {
        public string Ad => "Koz Q Sorusu";
        public string Aciklama => "Koz kızını soran 5NT";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 27;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Partner 5NT dediyse → Koz Q cevabı veriyoruz
            if (durum.SonGercekTeklif == "5NT" && durum.SonGercekTeklifSahibi == durum.Partner)
            {
                // Sadece 4NT veya Kickback sonrası 5NT ise Koz Q sorusudur
                // (Yani partner daha önce 4NT veya Kickback yapmış olmalı)
                bool oncekiSoruVar = durum.PartnerTeklifleri.Any(t =>
                    t == "4NT" || t == "4♠" || t == "4♥" || t == "4♦");

                if (oncekiSoruVar) return true;
                return false;
            }

            // Biz 5NT soruyorsak (Koz Q sorusu)
            if (durum.KendiTeklifleri.Contains("4NT") && !durum.KendiTeklifleri.Contains("5NT"))
            {
                int seviye = 0;
                if (!string.IsNullOrEmpty(durum.SonGercekTeklif))
                    seviye = KozYardimcisi.TekliftenSeviyeCikar(durum.SonGercekTeklif);

                if (seviye == 5) return true;
            }

            // Kickback sonrası 5NT
            if (durum.KendiTeklifleri.Contains("4♠") || durum.KendiTeklifleri.Contains("4♥") ||
                durum.KendiTeklifleri.Contains("4♦"))
            {
                if (!durum.KendiTeklifleri.Contains("5NT"))
                {
                    int seviye = 0;
                    if (!string.IsNullOrEmpty(durum.SonGercekTeklif))
                        seviye = KozYardimcisi.TekliftenSeviyeCikar(durum.SonGercekTeklif);
                    if (seviye == 5) return true;
                }
            }

            return false;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string koz = durum.AnlasilanKoz();
            if (string.IsNullOrEmpty(koz)) koz = "Maça";

            // Partner 5NT dedi → Koz Q cevabı veriyoruz
            if (durum.SonGercekTeklif == "5NT" && durum.SonGercekTeklifSahibi == durum.Partner)
            {
                bool kozQVar = ElDegerlendirici.KizVar(el, koz);

                // Yan renklerde Papaz var mı?
                var yanRenkler = new List<string> { "Maça", "Kupa", "Karo", "Sinek" };
                yanRenkler.Remove(koz);

                string yanPapaz = null;
                foreach (var renk in yanRenkler)
                {
                    if (ElDegerlendirici.PapazVar(el, renk))
                    {
                        yanPapaz = renk;
                        break;
                    }
                }

                if (!kozQVar && yanPapaz != null)
                {
                    switch (yanPapaz)
                    {
                        case "Sinek": return "6♣";
                        case "Karo": return "6♦";
                        case "Kupa": return "6♥";
                        case "Maça": return "6♠";
                    }
                }

                if (kozQVar && yanPapaz == null)
                {
                    return "6NT";
                }

                if (kozQVar && yanPapaz != null)
                {
                    return "7" + KozYardimcisi.KozSembolu(koz);
                }

                return "6" + KozYardimcisi.KozSembolu(koz);
            }

            // 5NT soruyoruz
            return "5NT";
        }
    }
}