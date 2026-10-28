using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    public class KozQSorusu : IKonvansiyon
    {
        /// <summary>Tek kilit kart eksikken küçük slam için gereken tahmini toplam HCP (ölçümle seçildi).</summary>
        public static int SlamEsik = 32;

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
                // Büyük slam için koz kızı da bilinmeli (RKCB'de 5♠ cevabı kızı gösterir)
                bool kizBilinen = ElDegerlendirici.KizVar(el, koz) || PartnerYaniti(durum, "4NT") == "5♠"
                                  || (durum.Anlasma != null && !durum.Anlasma.RKCB);
                string hedef = (toplam >= 3 && kizBilinen ? "7" : "6") + sembol;
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

            // 4NT'ye cevap geldi: kilit kart toplamına göre karar ver (körü körüne 5NT sorma)
            if (durum.KendiTeklifleri.Contains("4NT") && !durum.KendiTeklifleri.Contains("5NT"))
                return SoranKarar(durum, el, koz);

            // 5NT soruyoruz
            return "5NT";
        }

        /// <summary>Benim ilk 4NT sorumdan sonra ortağın ilk gerçek teklifi (cevap). Yoksa null.</summary>
        private static string PartnerYaniti(IhaleDurumu durum, string benimSoru)
        {
            int idx = -1;
            for (int i = 0; i < durum.Gecmis.Count; i++)
                if (durum.Gecmis[i].Oyuncu == durum.AktifOyuncu && durum.Gecmis[i].Teklif == benimSoru) idx = i;
            if (idx < 0) return null;
            for (int j = idx + 1; j < durum.Gecmis.Count; j++)
                if (durum.Gecmis[j].GercekTeklifMi && durum.Gecmis[j].Oyuncu == durum.Partner)
                    return durum.Gecmis[j].Teklif;
            return null;
        }

        /// <summary>
        /// 4NT (Blackwood/RKCB) cevabından sonra soran taraf: toplam as/kilit kart sayısına göre
        /// kapat (5M), 6M, ya da papaz sor (5NT). Belirsiz durumda eski davranış (5NT).
        /// </summary>
        private static string SoranKarar(IhaleDurumu durum, List<Card> el, string koz)
        {
            if (string.IsNullOrEmpty(koz) || koz == "NT") return "5NT";
            string yanit = PartnerYaniti(durum, "4NT");
            if (string.IsNullOrEmpty(yanit)) return "5NT";

            bool rkcb = durum.Anlasma == null || durum.Anlasma.RKCB;
            bool b1430 = durum.Anlasma == null || durum.Anlasma.RKCB1430;
            int benim = rkcb ? ElDegerlendirici.KilitKartSayisi(el, koz) : ElDegerlendirici.AsSayisi(el);
            int enFazla = rkcb ? 5 : 4;
            int[] secenek;
            bool kizGosterdi = false;
            if (rkcb)
            {
                switch (yanit)
                {
                    case "5♣": secenek = b1430 ? new[] { 1, 4 } : new[] { 0, 3 }; break;
                    case "5♦": secenek = b1430 ? new[] { 0, 3 } : new[] { 1, 4 }; break;
                    case "5♥": secenek = new[] { 2, 5 }; break;
                    case "5♠": secenek = new[] { 2, 5 }; kizGosterdi = true; break;
                    default: return "5NT";
                }
            }
            else
            {
                switch (yanit)
                {
                    case "5♣": secenek = new[] { 0, 4 }; break;
                    case "5♦": secenek = new[] { 1 }; break;
                    case "5♥": secenek = new[] { 2 }; break;
                    case "5♠": secenek = new[] { 3 }; break;
                    default: return "5NT";
                }
            }

            // Toplam enFazla'yı aşamaz: imkânsız seçenekleri ele, kalanın en küçüğünü varsay (temkinli)
            var olasi = secenek.Where(s => s + benim <= enFazla).ToList();
            int toplam = (olasi.Count > 0 ? olasi.Min() : secenek.Min()) + benim;

            string sembol = KozYardimcisi.KozSembolu(koz);
            int eksik = enFazla - toplam;   // eksik as / kilit kart
            if (eksik >= 2)   // kapat: cevap zaten 5M ise Pas
                return yanit == "5" + sembol ? "Pas" : KozYardimcisi.YasalaYukselt("5" + sembol, yanit);
            if (eksik == 1)
            {
                // Tek kilit kart eksik: küçük slam için toplam güç yeterli mi? (ortak aralığı tahmini)
                var ar = IhaleKoruma.PartnerAraligi(durum);
                double est = ElDegerlendirici.HCP(el) + ar.Min + 0.25 * (ar.Max - ar.Min);
                if (est < SlamEsik)
                    return yanit == "5" + sembol ? "Pas" : KozYardimcisi.YasalaYukselt("5" + sembol, yanit);
                return KozYardimcisi.YasalaYukselt("6" + sembol, yanit);   // küçük slam
            }
            // Hepsi bizde
            bool kizBilinen = ElDegerlendirici.KizVar(el, koz) || kizGosterdi;
            if (rkcb && !kizBilinen) return KozYardimcisi.YasalaYukselt("6" + sembol, yanit);
            return "5NT";   // papaz sor → 7M kararı
        }
    }
}