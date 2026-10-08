using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Gerber Konvansiyonu — NT açılışlarında As sormak için 4♣ kullanılır.
    ///
    /// Kural:
    /// - Partner 1NT/2NT açtı (veya NT'de anlaşıldı)
    /// - 4♣ sorar → As sayısı
    ///   4♦ = 0 veya 4 As
    ///   4♥ = 1 As
    ///   4♠ = 2 As
    ///   4NT = 3 As
    ///
    /// Öncelik: 13 (CueBid 14 ve Blackwood 15'ten ÖNCE)
    /// </summary>
    public class Gerber : IKonvansiyon
    {
        public string Ad => "Gerber";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 13;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Sadece NT anlaşması varsa
            string koz = durum.AnlasilanKoz();
            if (koz != "NT") return false;

            // 4♣ daha önce kullanılmadıysa
            if (durum.KendiTeklifleri.Contains("4♣")) return false;

            // Partner 4♣ dediyse → cevap veriyoruz
            if (durum.SonGercekTeklif == "4♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
                return true;

            // Biz soracaksak:
            // Gerber genellikle 1NT/2NT açılışları sonrası, slam ilgisi olduğunda sorulur.
            // Seviye kontrolü: 1NT (seviye 1), 2NT (seviye 2) veya 3NT (seviye 3) sonrası.
            int seviye = durum.KacinciSeviye();
            if (seviye < 1 || seviye > 3) return false;

            // Slam potansiyeli için HP ve Quick Trick kontrolü
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            double qt = ElDegerlendirici.HizliElSayisi(durum.AktifOyuncuEli);

            // 14+ HP veya (12+ HP ve 5+ Quick Trick) ise Gerber sorabilir
            if (hp >= 14 || (hp >= 12 && qt >= 5.0))
            {
                return true;
            }

            return false;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;

            if (durum.SonGercekTeklif == "4♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                int asSayisi = ElDegerlendirici.AsSayisi(el);
                return asSayisi switch
                {
                    0 => "4♦",
                    1 => "4♥",
                    2 => "4♠",
                    3 => "4NT",
                    4 => "4♦",
                    _ => "4♦"
                };
            }

            return "4♣";
        }
    }
}
