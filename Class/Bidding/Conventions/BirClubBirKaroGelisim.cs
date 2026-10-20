using System;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 1♣ ⇒ 1♦ GELİŞİMİ — AŞAMA 1: AÇICI REBİD.
    ///
    /// Sıra: 1♣ (biz) - 1♦ (partner) - ? (biz)
    ///
    /// Tablodaki rebid'ler (5'li majör mantığıyla):
    ///   1♥    = 4+♣ 4♥, 11-17 HCP
    ///   1♠    = 4+♣ 4♠, 11-17 HCP
    ///   1NT   = 11-14 dengeli (4'lü majör olabilir)
    ///   2♣    = 6+♣, 11-16 HCP
    ///   2♦    = 4+♣ 4♦, 11-14 HCP
    ///   2♥    = 5+♣ 4♥, 18+ HCP
    ///   2♠    = 5+♣ 4♠, 18+ HCP
    ///   2NT   = 6+♣ veya 5♣4♦, 18+ HCP
    ///   3♣    = 6+♣, 15-17 HCP
    ///   3♦    = 5+♣ 4♦, 15-17 HCP
    ///   3♥    = 4♣4♦4♠1♥, 18-21 HCP
    ///   3♠    = 4♣4♦4♥1♠, 18-21 HCP
    ///   3NT   = 4♣4♥4♠1♦, 18-21 HCP
    /// </summary>
    public class BirClubBirKaroGelisim : IKonvansiyon
    {
        public string Ad => "1♣-1♦ Gelişimi";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 6;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            if (durum.KendiTeklifleri.Count == 0) return false;
            if (durum.KendiTeklifleri[0] != "1♣") return false;

            if (durum.SonGercekTeklif != "1♦") return false;
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;

            int gercekTeklifSayisi = durum.Gecmis.Count(h => h.GercekTeklifMi);
            if (gercekTeklifSayisi > 2) return false;

            if (durum.RakipTeklifleri.Any(t => !string.IsNullOrEmpty(t)
                && t != "Pas" && t != "Dbl" && t != "RDbl"))
                return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            int hcp = ElDegerlendirici.HCP(el);
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // 1) 18-21 HCP özel dağılımlar (3♥/3♠/3NT)
            if (hcp >= 18 && hcp <= 21)
            {
                if (c >= 4 && d >= 4 && s >= 4 && h <= 1) return "3♥";
                if (c >= 4 && d >= 4 && h >= 4 && s <= 1) return "3♠";
                if (c >= 4 && h >= 4 && s >= 4 && d <= 1) return "3NT";
            }

            // 2) 18+ HCP, 5+♣ iki renkli
            if (hcp >= 18)
            {
                if (c >= 5 && h >= 4 && s < 4) return "2♥";
                if (c >= 5 && s >= 4 && h < 4) return "2♠";
                if (c >= 6 || (c >= 5 && d >= 4)) return "2NT";
                if (dengeli) return "2NT";
            }

            // 3) 15-17 HCP, 6+♣ veya 5♣4♦
            if (hcp >= 15 && hcp <= 17)
            {
                if (c >= 6) return "3♣";
                if (c >= 5 && d >= 4) return "3♦";
            }

            // 4) 11-17 HCP, 4+♣ 4+ yan majör (1♥/1♠)
            if (hcp >= 11 && hcp <= 17 && c >= 4)
            {
                if (h >= 4 && s >= 4)
                {
                    if (s >= h) return "1♠";
                    return "1♥";
                }
                if (h >= 4) return "1♥";
                if (s >= 4) return "1♠";
            }

            // 5) 11-14 HCP, 4+♣ 4+♦ (2♦)
            if (hcp >= 11 && hcp <= 14 && c >= 4 && d >= 4
                && h < 4 && s < 4)
                return "2♦";

            // 6) 11-16 HCP, 6+♣ (2♣)
            if (hcp >= 11 && hcp <= 16 && c >= 6
                && h < 4 && s < 4)
                return "2♣";

            // 7) 11-14 dengeli (1NT)
            if (hcp >= 11 && hcp <= 14 && dengeli)
                return "1NT";

            // 8) Fallback
            return "1NT";
        }
    }
}