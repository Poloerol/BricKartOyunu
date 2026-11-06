using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>
    /// 1♦ AÇIŞINA CEVAPLAR (SAS kitabı s.24). OneDiamond_AnswerSistemi açıkken çalışır.
    /// 1♥/1♠ 4+HCP 4+kart, 1NT 5-10, 2♣ (12+ dengeli 2+♣ ya da 4+♦ FG), 2♦ 5/4 minör 7-9,
    /// 2♥ 5♠4+♥ 3-8, 2♠ (5+♦ davet / 5-4 minör FG / 6+♣ davet), 2NT 11-12 dengeli, 3NT 4♦ 3-3-3 12-14,
    /// 3'lü ve 4'lü sıçramalar önleyici.
    /// </summary>
    public class OneDiamond_Answer : IKonvansiyon
    {
        public string Ad => "1♦ Cevap Sistemi";
        public string Aciklama => "1♦ açışına cevap sistemi (SAS)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 5;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.SonGercekTeklif != "1♦") return false;
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;
            if (durum.RakipTeklifleri.Any(t => !string.IsNullOrEmpty(t) && t != "Pas" && t != "Dbl" && t != "RDbl"))
                return false;
            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            int hcp = ElDegerlendirici.HCP(el);
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            bool dengeli = ElDegerlendirici.DengeliEl(el);
            bool tehlikeli = ElDegerlendirici.ZondaMi(durum.ZonNS, durum.ZonEW, durum.AktifOyuncu);
            bool zayifNT = durum.Anlasma != null && !durum.Anlasma.StrongNT;   // SAS s.84: 1♥/1♠ 5+ HCP, 1NT 7-10, 2♣ 5+♣, 3♦ 4+♦ blokaj

            // 1) 3NT: 4♦ 3-3-3, 12-14 HCP, 0-3 kontrol
            if (hcp >= 12 && hcp <= 14 && d == 4 && s == 3 && h == 3 && c == 3 && ElDegerlendirici.KontrolSayisi(el) <= 3)
                return "3NT";

            // 2) 2♠: 5/4 minör FG (12+), 5+♦ davet (9-11), 6+♣ davet (9-11)
            if (hcp >= 12 && ((c >= 5 && d >= 4) || (d >= 5 && c >= 4)) && s < 4 && h < 4) return "2♠";
            if (hcp >= 9 && hcp <= 11 && d >= 5 && s < 4 && h < 4) return "2♠";
            if (hcp >= 9 && hcp <= 11 && c >= 6 && s < 4 && h < 4) return "2♠";

            // 3) 2♣: 12+ HCP dengeli 2+♣ ya da 4+♦ FG
            if (hcp >= 12 && ((dengeli && s < 5 && h < 5) || (zayifNT ? c >= 5 : d >= 4))) return "2♣";

            // 4) 2♥: 5♠ 4+♥ 3-8 HCP
            if (s >= 5 && h >= 4 && hcp >= 3 && hcp <= 8) return "2♥";

            // 5) 2♦: 5/4 minör 7-9 HCP
            int karo, sinek;
            if (ElDegerlendirici.IkiMinorDagilimi(el, out karo, out sinek) && hcp >= 7 && hcp <= 9 && s < 4 && h < 4) return "2♦";

            // 6) 2NT: 11-12 dengeli, majör yok
            if (hcp >= 11 && hcp <= 12 && dengeli && s < 4 && h < 4) return "2NT";

            // 7) 1NT: 5-10 HCP, 4'lü majör yok
            if (hcp >= (zayifNT ? 7 : 5) && hcp <= 10 && s < 4 && h < 4 && !(c >= 6 && hcp <= 8)) return "1NT"; // 6+♣ zayıf → 3♣ önleyici

            // 8) Majörler: 4+ HCP, 4+ kart
            int minMaj = zayifNT ? 5 : 4;
            if (hcp >= minMaj && s >= 4 && s > h) return "1♠";
            if (hcp >= minMaj && h >= 4) return "1♥";
            if (hcp >= minMaj && s >= 4) return "1♠";
            if (zayifNT && d >= 4 && hcp <= 8) return "3♦";   // 4+♦ blokaj

            // 9) Önleyiciler
            if (tehlikeli)
            {
                if (c >= 6 && ElDegerlendirici.IyiRenk(el, "Sinek") && hcp >= 5 && hcp <= 8) return "3♣";
                if (h >= 6 && ElDegerlendirici.IyiRenk(el, "Kupa") && hcp >= 5 && hcp <= 8) return "3♥";
                if (s >= 6 && ElDegerlendirici.IyiRenk(el, "Maça") && hcp >= 5 && hcp <= 8) return "3♠";
            }
            else
            {
                if (c >= 6 && hcp >= 5 && hcp <= 8) return "3♣";
                if (h >= 6 && hcp >= 5 && hcp <= 8) return "3♥";
                if (s >= 6 && hcp >= 5 && hcp <= 8) return "3♠";
            }
            if (d >= 6 && hcp <= 8) return "3♦";
            if (c >= 7 && ElDegerlendirici.IyiRenk(el, "Sinek")) return "4♣";
            if (d >= 7 && ElDegerlendirici.IyiRenk(el, "Karo")) return "4♦";
            if (h >= 7 && ElDegerlendirici.IyiRenk(el, "Kupa")) return "4♥";
            if (s >= 7 && ElDegerlendirici.IyiRenk(el, "Maça")) return "4♠";

            return "Pas";
        }
    }
}
