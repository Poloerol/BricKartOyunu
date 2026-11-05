using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    /// <summary>
    /// 1NT AÇIŞINA CEVAPLAR (SAS kitabı s.51-52). OneNT_AnswerSistemi açıkken çalışır. 1NT = (14)15-17 dengeli / yarı dengeli.
    /// 2♣ NF Stayman; 2♦/2♥ transfer (♥/♠); 2♠ aralık sorusu (5+♣ S.O); 2NT 5'li majör sorusu;
    /// 3♣ 3♦'ye zorla (5+♦ S.O / 6+♣ FG SNG majör); 3♦ 6+♦ FG SNG majör; 3♥/3♠ 5/5 minör SNG ♥/♠;
    /// 3NT oynamak için; 4♦/4♥ Texas; 4NT/6NT slem davet / kapama.
    /// SEÇİM: zayıf ≤7, davet 8-9, oyun 10-15, slem daveti 16-17, 18+ 6NT.
    /// </summary>
    public class OneNT_Answer : IKonvansiyon
    {
        public string Ad => "1NT Cevap Sistemi";
        public string Aciklama => "1NT açışına cevap sistemi (SAS)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 5;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.SonGercekTeklif != "1NT") return false;
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;
            if (durum.Gecmis.Count(h => h.GercekTeklifMi) != 1) return false;      // yalnızca 1NT AÇILIŞI
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

            // 1) Texas: 6+ majör, oyun gücü (10-14)
            if (hcp >= 10 && hcp <= 14)
            {
                if (h >= 6 && h > s) return "4♦";
                if (s >= 6 && s > h) return "4♥";
            }
            // 2) 5♠ + davet: Stayman (kitap 2♣ c)
            if (s == 5 && h < 5 && hcp >= 8 && hcp <= 9) return "2♣";
            // 3) 5+ majör: transfer (5/5 → ♠ transferi, sonra 3♥)
            if (s >= 5 && h >= 5) return "2♥";
            if (h >= 5) return "2♦";
            if (s >= 5) return "2♥";
            // 4) 4'lü majör: Stayman (8+; zayıf 4-4 majör S.O)
            if ((s == 4 || h == 4) && hcp >= 8) return "2♣";
            if (s == 4 && h == 4) return "2♣";
            // 5) Minör elleri
            if (hcp >= 10 && d >= 6 && (s <= 1 || h <= 1)) return "3♦";               // 6+♦ FG SNG majör
            if (hcp >= 10 && c >= 5 && d >= 5 && h <= 1) return "3♥";                  // 5/5 minör SNG♥
            if (hcp >= 10 && c >= 5 && d >= 5 && s <= 1) return "3♠";                  // 5/5 minör SNG♠
            if (hcp >= 10 && c >= 6 && (s <= 1 || h <= 1)) return "3♣";                // 6+♣ FG SNG majör
            if (hcp <= 7 && d >= 5) return "3♣";                                       // 5+♦ S.O (açan 3♦)
            if (hcp <= 7 && c >= 5) return "2♠";                                       // 5+♣ S.O (aralık sorusu)
            // 6) Majör sorunu olmayan eller
            if (hcp >= 18) return "6NT";
            if (hcp >= 16) return "4NT";
            if (hcp >= 10)
                return (s == 3 || h == 3) ? "2NT" : "3NT";                             // 2NT = 5'li majör sorusu
            if (hcp >= 8) return "2♣";                                                  // davet: Stayman, sonra 2NT
            return "Pas";
        }
    }
}
