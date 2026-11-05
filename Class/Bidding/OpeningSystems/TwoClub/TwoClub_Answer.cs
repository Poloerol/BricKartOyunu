using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoClub
{
    /// <summary>
    /// 2♣ AÇIŞINA CEVAPLAR (SAS kitabı s.58). TwoClub_AnswerSistemi açıkken çalışır.
    /// 2♣ = 23+ dengeli / OG iki renkli / tek renkli 3-4 kayıp.
    /// 2♦ 5+♥ OG veya röle (diğer tüm eller); 2♥ 5+♠ 7+; 2♠ 6+♣ 8+; 2NT 5/4+ majörler 7+; 3♣ 6+♦ 8+;
    /// 3♦ 4♥ OG; 3♥ 4♠ OG; 3♠ 5♣332 / 5♦332 / 5-4 minör; 3NT 5♣5♦ 0-6; 4♣/4♦ 7♥/7♠ (A veya KJ veya QJ10).
    /// </summary>
    /// <summary>2♣ OG: açanın doğal yeniden teklifine cevapçı devamı kitapta yoksa oyun gücünü sürdüren yedek cevap.</summary>
    internal static class TcDevam
    {
        public static string Dogal(BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcEl e, string rebid)
        {
            if (rebid == "2NT")
            {
                if (e.S >= 5 && e.S >= e.H) return "3♠";
                if (e.H >= 5) return "3♥";
                return "3NT";
            }
            if (rebid.Length == 2 && rebid[0] == '3')
            {
                if (rebid[1] == '♥' && e.H >= 3) return "4♥";
                if (rebid[1] == '♠' && e.S >= 3) return "4♠";
                return "3NT";
            }
            return "Pas";
        }
        public static bool Tanimli(string rebid)
        {
            return rebid == "2NT" || (rebid.Length == 2 && rebid[0] == '3' && (rebid[1] == '♣' || rebid[1] == '♦' || rebid[1] == '♥' || rebid[1] == '♠'));
        }
    }

    public class TwoClub_Answer : IKonvansiyon
    {
        public string Ad => "2♣ Cevap Sistemi";
        public string Aciklama => "2♣ açışına cevap sistemi (SAS)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 5;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.SonGercekTeklif != "2♣") return false;
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;
            if (durum.Gecmis.Count(h => h.GercekTeklifMi) != 1) return false;      // yalnızca 2♣ AÇILIŞI
            if (durum.RakipTeklifleri.Any(t => !string.IsNullOrEmpty(t) && t != "Pas" && t != "Dbl" && t != "RDbl"))
                return false;
            return true;
        }

        private static bool UstKart(List<Card> el, string renk)
        {
            var k = el.Where(c => c.Suit == renk).Select(c => c.Value).OrderByDescending(v => v).ToList();
            if (k.Count == 0) return false;
            if (k[0] == 14) return true;                                              // A
            if (k.Contains(13) && k.Contains(11)) return true;                         // KJ
            return k.Contains(12) && k.Contains(11) && k.Contains(10);                 // QJ10
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

            // 4♣ / 4♦ : 7'li majör (♥ / ♠), üst kart
            if (h >= 7 && UstKart(el, "Kupa")) return "4♣";
            if (s >= 7 && UstKart(el, "Maça")) return "4♦";
            // 3NT : 5/5 minör, 0-6
            if (c >= 5 && d >= 5 && hcp <= 6) return "3NT";
            if (hcp >= 7)
            {
                if ((s >= 5 && h >= 4) || (h >= 5 && s >= 4)) return "2NT";           // 5/4+ majörler
                if (s >= 5) return "2♥";
                if (h >= 5) return "2♦";
                if (hcp >= 8)
                {
                    if (c >= 6) return "2♠";
                    if (d >= 6) return "3♣";
                    if (h == 4) return "3♦";
                    if (s == 4) return "3♥";
                    if ((c >= 5 && d >= 4) || (d >= 5 && c >= 4) || (c == 5 && d == 3) || (d == 5 && c == 3)) return "3♠";
                }
            }
            return "2♦";                                                              // röle
        }
    }
}
