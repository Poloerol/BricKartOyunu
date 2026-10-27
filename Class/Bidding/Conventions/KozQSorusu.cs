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

            // Ben 5NT (papaz sorusu) sordum, ortak 6. seviyede cevap verdi → papaz sayısına göre 6M / 7M
            if (durum.KendiTeklifleri.Contains("5NT") && durum.SonGercekTeklifSahibi == durum.Partner &&
                KozYardimcisi.TekliftenSeviyeCikar(durum.SonGercekTeklif) == 6)
            {
                string k = durum.AnlasilanKoz();
                if (!string.IsNullOrEmpty(k) && k != "NT" && PapazCevabi(durum.SonGercekTeklif) >= 0) return true;
            }

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

        /// <summary>Papaz sorusunun cevabından yan renk papaz sayısı: 6♣=0, 6♦=1, 6♥=2, 6♠=3, 6NT=4; diğerleri -1.</summary>
        private static int PapazCevabi(string teklif)
        {
            switch (teklif)
            {
                case "6♣": return 0;
                case "6♦": return 1;
                case "6♥": return 2;
                case "6♠": return 3;
                case "6NT": return 4;
                default: return -1;
            }
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string koz = durum.AnlasilanKoz();
            if (string.IsNullOrEmpty(koz)) koz = "Maça";

            // Papaz sorusunu ben sordum, cevap geldi: yan renklerde toplam 3+ papaz → büyük slam (7M), değilse 6M
            if (durum.KendiTeklifleri.Contains("5NT") && durum.SonGercekTeklifSahibi == durum.Partner &&
                KozYardimcisi.TekliftenSeviyeCikar(durum.SonGercekTeklif) == 6 && PapazCevabi(durum.SonGercekTeklif) >= 0)
            {
                int benimPapaz = 0;
                foreach (var renk in new[] { "Maça", "Kupa", "Karo", "Sinek" })
                    if (renk != koz && ElDegerlendirici.PapazVar(el, renk)) benimPapaz++;
                int toplam = benimPapaz + PapazCevabi(durum.SonGercekTeklif);
                string sembol = KozYardimcisi.KozSembolu(koz);
                string hedef = (toplam >= 3 ? "7" : "6") + sembol;
                return KozYardimcisi.YasalaYukselt(hedef, durum.SonGercekTeklif);
            }

            // Partner 5NT dedi → Koz Q cevabı veriyoruz
            if (durum.SonGercekTeklif == "5NT" && durum.SonGercekTeklifSahibi == durum.Partner)
            {
                // Standart RKCB papaz sorusu: kozun papazı hariç, yan renklerdeki papaz sayısı
                // 6♣ = 0, 6♦ = 1, 6♥ = 2, 6♠ = 3, 6NT = 4
                int yanPapaz = 0;
                foreach (var renk in new[] { "Maça", "Kupa", "Karo", "Sinek" })
                    if (renk != koz && ElDegerlendirici.PapazVar(el, renk)) yanPapaz++;
                switch (yanPapaz)
                {
                    case 0: return "6♣";
                    case 1: return "6♦";
                    case 2: return "6♥";
                    case 3: return "6♠";
                    default: return "6NT";
                }
            }

            // 5NT soruyoruz
            return "5NT";
        }
    }
}