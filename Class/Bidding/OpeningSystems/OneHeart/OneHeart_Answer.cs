using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneHeart
{
    /// <summary>
    /// 1♥ AÇIŞINA CEVAPLAR (SAS kitabı s.35). OneHeart_AnswerSistemi açıkken çalışır.
    /// 1♠ 5+HCP 4+♠; 1NT 5-11 (3♥ yok) / 9-11 (3♥); 2♣ FG röle (12+ dengeli/5+♣, 6+♣ davet);
    /// 2♦ 5+♦ FG / 6+♦ davet; 2♥ 3+♥ 7-10; 2♠ 5/5 minör 7-11; 2NT 3♥ davet (4♠ yok);
    /// 3♣ 4+♥ dengeli davet; 3♦ 4+♥ SNG 6-9; 3♠ 4+♥ SNG 10-12; 3NT/4♣/4♦ 4+♥ void (♣/♦/♠) 8-12; 3♥/4♥/4♠ önleyici.
    /// </summary>
    public class OneHeart_Answer : IKonvansiyon
    {
        public string Ad => "1♥ Cevap Sistemi";
        public string Aciklama => "1♥ açışına cevap sistemi (SAS)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 5;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.SonGercekTeklif != "1♥") return false;
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
            bool duz = OneClub.OneClub_OneDiamond.Duz(new OneClub.OcEl(el));
            bool tekli = s == 1 || h == 1 || d == 1 || c == 1;

            // 1) 4+♥ void: 3NT ♣, 4♣ ♦, 4♦ ♠ (8-12 HCP)
            if (h >= 4 && hcp >= 8 && hcp <= 12)
            {
                if (c == 0) return "3NT";
                if (d == 0) return "4♣";
                if (s == 0) return "4♦";
            }
            // 2) 4+♥ singleton: 3♠ 10-12, 3♦ 6-9
            if (h >= 4 && tekli && hcp >= 10 && hcp <= 12) return "3♠";
            if (h >= 4 && tekli && hcp >= 6 && hcp <= 9) return "3♦";
            // 3) 4+♥ dengeli davet
            if (h >= 4 && duz && hcp >= 9 && hcp <= 11) return "3♣";
            // 4) Önleyiciler
            if (h >= 7 && hcp <= 10) return "4♥";
            if (s >= 7 && hcp <= 10) return "4♠";
            if (h >= 5 && hcp <= 6 && (h >= 6 || tekli)) return "3♥";
            // 5) 2♠: 5/5 minör 7-11
            if (c >= 5 && d >= 5 && h <= 2 && hcp >= 7 && hcp <= 11) return "2♠";
            // 6) Oyun gücü (12+)
            if (hcp >= 12)
            {
                if (d >= 5 && s < 5 && d >= c) return "2♦";
                if (s >= 4) return "1♠";
                return "2♣";                                      // röle (dengeli / 5+♣ / 3+♥)
            }
            // 7) 2NT: 3♥ davet, 4♠ yok
            if (h == 3 && s < 4 && hcp >= 10 && hcp <= 11) return "2NT";
            // 8) 2♥: 3+♥ yapıcı 7-10
            if (h >= 3 && s < 5 && hcp >= 7 && hcp <= 10) return "2♥";
            // 9) 1♠: 5+ HCP 4+♠
            if (s >= 4 && hcp >= 5) return "1♠";
            // 10) 6+♦ / 6+♣ davet (9-11)
            if (hcp >= 9 && hcp <= 11 && d >= 6) return "2♦";
            if (hcp >= 9 && hcp <= 11 && c >= 6) return "2♣";
            // 11) 1NT
            if (h <= 2 && hcp >= 5 && hcp <= 11) return "1NT";
            if (h >= 3 && hcp >= 9 && hcp <= 11) return "1NT";
            return "Pas";
        }
    }
}
