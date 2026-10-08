using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Grand Slam Force Konvansiyonu (5NT Papaz Sorma).
    /// 
    /// Kural:
    /// - Blackwood 4NT sonrası (As'lar öğrenildi)
    /// - Ben 5NT derim → "Papazları say"
    /// - Partner cevap verir:
    ///   - 6♣ = 0 Papaz
    ///   - 6♦ = 1 Papaz
    ///   - 6♥ = 2 Papaz
    ///   - 6♠ = 3 Papaz
    ///   - 6NT = 4 Papaz
    /// 
    /// Amaç: Grand slam denemesi için Papaz sayısını öğrenmek.
    /// 
    /// Öncelik: 28
    /// </summary>
    public class GrandSlamForce : IKonvansiyon
    {
        public string Ad => "Grand Slam Force";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 28;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // 1. Bir koz anlaşması var mı?
            string koz = durum.AnlasilanKoz();
            if (string.IsNullOrEmpty(koz)) return false;

            // 2. Partner 5NT dedi mi? (cevap veriyorum)
            if (durum.SonGercekTeklif == "5NT" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                return true;
            }

            // 3. Ben 5NT soracak mıyım?
            //    - Blackwood sormuş olmalıyım (KendiTekliflerim'de 4NT var)
            //    - Partner 5 seviyesinde cevap verdi (5♣/5♦/5♥/5♠)
            //    - Ben 5NT diyeceğim

            if (!durum.KendiTeklifleri.Contains("4NT")) return false;

            // Son teklif partnerden mi ve 5 seviyesinde mi?
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;
            string sonPartnerTeklif = durum.SonGercekTeklif;
            if (string.IsNullOrEmpty(sonPartnerTeklif)) return false;
            if (sonPartnerTeklif.Length == 0) return false;
            if (!char.IsDigit(sonPartnerTeklif[0])) return false;
            int seviye = sonPartnerTeklif[0] - '0';
            if (seviye != 5) return false;

            // 5NT zaten kullanılmadıysa
            if (durum.KendiTeklifleri.Contains("5NT")) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;

            // Partner 5NT dedi → Papaz sayısını söyle
            if (durum.SonGercekTeklif == "5NT" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                int papazSayisi = ElDegerlendirici.PapazSayisi(el);
                switch (papazSayisi)
                {
                    case 0: return "6♣";
                    case 1: return "6♦";
                    case 2: return "6♥";
                    case 3: return "6♠";
                    case 4: return "6NT";
                    default: return "6♣";
                }
            }

            // Ben soruyorum
            return "5NT";
        }
    }
}