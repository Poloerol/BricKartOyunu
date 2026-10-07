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

            // Partner 4♣ dediyse → cevap veriyoruz (HP kontrolü yok)
            if (durum.SonGercekTeklif == "4♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
                return true;

            // Biz soracaksak: tam olarak 3. seviyede olmalı
            int seviye = durum.KacinciSeviye();
            // Gerber, 1NT/2NT açılışlarından SONRA sorulur.
            // 1NT → seviye 1, 2NT → seviye 2, 3NT → seviye 3
            if (seviye < 1 || seviye > 3) return false;

            // En az 15 HP
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 15) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;

            // Partner 4♣ dedi → cevap ver
            if (durum.SonGercekTeklif == "4♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                int asSayisi = ElDegerlendirici.AsSayisi(el);
                switch (asSayisi)
                {
                    case 0: return "4♦";
                    case 1: return "4♥";
                    case 2: return "4♠";
                    case 3: return "4NT";
                    case 4: return "4♦";
                    default: return "4♦";
                }
            }

            // Biz soruyoruz
            return "4♣";
        }
    }
}