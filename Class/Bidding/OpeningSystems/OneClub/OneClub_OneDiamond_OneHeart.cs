using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 1♦ ⇒ 1♥ GELİŞİMİ.
    /// Aşama A: 1♣ (partner) - 1♦ (biz) - 1♥ (partner) - sıra bizde → CEVAPÇI DEVAM
    /// Aşama B: 1♣ (biz) - 1♦ (partner) - 1♥ (biz) - 2♣ (partner) - sıra bizde → AÇICI RÖLE CEVABI
    /// </summary>
    public class OneClub_OneDiamond_OneHeart : IAcilisGelisimi
    {
        public string Aciklama => "1♣-1♦-1♥ gelişimi";
        public int Oncelik => 45;   // 40'tan yüksek, çünkü daha spesifik

        public bool UygunMu(List<Card> el, IhaleDurumu durum)
        {
            if (durum == null || el == null) return false;
            if (el.Count != 13) return false;

            // Koltuktan bağımsız: yalnızca GERÇEK teklifler (Pas/Dbl sayılmaz) ve rakip sessiz
            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (gercek.Any(h => durum.Rakipler.Contains(h.Oyuncu))) return false;

            // Aşama A: 1♣ (partner) - 1♦ (biz) - 1♥ (partner) - sıra bizde → CEVAPÇI DEVAM
            if (gercek.Count == 3
                && gercek[0].Teklif == "1♣" && gercek[1].Teklif == "1♦" && gercek[2].Teklif == "1♥"
                && gercek[0].Oyuncu == durum.Partner
                && gercek[1].Oyuncu == durum.AktifOyuncu
                && gercek[2].Oyuncu == durum.Partner)
                return true;

            // Aşama B: 1♣ (biz) - 1♦ (partner) - 1♥ (biz) - 2♣ (partner) - sıra bizde → AÇICI RÖLE CEVABI
            if (gercek.Count == 4
                && gercek[0].Teklif == "1♣" && gercek[1].Teklif == "1♦"
                && gercek[2].Teklif == "1♥" && gercek[3].Teklif == "2♣"
                && gercek[0].Oyuncu == durum.AktifOyuncu
                && gercek[1].Oyuncu == durum.Partner
                && gercek[2].Oyuncu == durum.AktifOyuncu
                && gercek[3].Oyuncu == durum.Partner)
                return true;

            return false;
        }

        public string TeklifVer(List<Card> el, IhaleDurumu durum)
        {
            // Aşama B mi? (Açıcı röle cevabı)
            if (durum.SonGercekTeklif == "2♣")
                return AciciRoleCevap(el, durum);

            // Aşama A (Cevapçı devam)
            return CevapciDevam(el, durum);
        }

        private string CevapciDevam(List<Card> el, IhaleDurumu durum)
        {
            if (el == null) return "Pas";

            int hcp = ElDegerlendirici.HCP(el);
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            bool dengeli = ElDegerlendirici.DengeliEl(el);
            bool sStopper = ElDegerlendirici.StopperVar(el, "Maça");

            // 1) 3NT: 13+ HCP dengeli, ♠ stopper, majör yok, 6♦ DEĞİL
            if (hcp >= 13 && dengeli && sStopper && s < 4 && d <= 4) return "3NT";

            // 2) 1♠: 4+♦ 4♠ F1R (9-12 HCP, 3♦ yok)
            if (d >= 4 && s >= 4 && hcp >= 9 && hcp <= 12 && d <= 4 && c >= 3) return "1♠";

            // 3) 3♦: 6♦ 4♠ FG (12+ HCP)
            if (d >= 6 && s >= 4 && hcp >= 12) return "3♦";

            // 4) 2♣: Röle SI (12+ HCP, 4+♦)
            if (hcp >= 12 && d >= 4) return "2♣";

            // 5) 2NT: Limit (11-12 dengeli)
            if (hcp >= 11 && hcp <= 12 && dengeli && s < 4) return "2NT";

            // 6) 3♣: 5♦4♠3♣1♥ (9-11 HCP)
            if (d >= 5 && s >= 4 && c >= 3 && h >= 1 && hcp >= 9 && hcp <= 11) return "3♣";

            // 7) 2♦: 6♦4♠ (9-11 HCP)
            if (d >= 6 && s >= 4 && hcp >= 9 && hcp <= 11) return "2♦";

            // 8) 1NT: 6-9 HCP dengeli, majör yok, ♠ stopper
            if (hcp >= 6 && hcp <= 9 && dengeli && s < 4 && sStopper) return "1NT";

            // Fallback
            return "1NT";
        }

        private string AciciRoleCevap(List<Card> el, IhaleDurumu durum)
        {
            if (el == null) return "Pas";

            int hcp = ElDegerlendirici.HCP(el);
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            bool max = hcp >= 16;

            if (d >= 4 && h >= 4 && hcp >= 13) return "3♥";          // 4♦ + 4♥, 13+ HCP (SI)
            if (d >= 5 && hcp >= 10 && hcp <= 11) return "3♦";        // 5♦, 10-11 HCP (MIN)
            if (d >= 4 && hcp >= 11 && hcp <= 12) return "2NT";       // 4♦, 11-12 HCP (MIN)
            if (s >= 4 && hcp >= 11 && hcp <= 14) return "2♠";        // 4♠, 11-14 HCP
            if (h >= 4 && c >= 4 && hcp >= 10 && hcp <= 11) return "3♣"; // 4♥ + 4♣, 10-11 HCP
            if (d >= 3 && !max) return "2♦";                          // 3♦, MIN
            if (d >= 3 && max) return "2♥";                           // 3♦, MAX
            return "Pas";
        }
    }
}