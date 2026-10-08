using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Responsive Double Konvansiyonu.
    /// 
    /// Kural:
    /// - Partner bir renk açtı (1♠/1♥/1♦/1♣) VEYA overcall yaptı
    /// - Rakip de overcall yaptı
    /// - Rakip kendi rengini 2 seviyesinde destekledi
    /// - Ben Dbl derim → "Diğer iki renkte değerim var"
    /// 
    /// Amaç: Rakipler bir renkte anlaştığında, kalan iki rengi göstermek.
    /// 
    /// Örnek:
    ///   1♠ (Rakip) - Pas - 2♠ (Rakip) - Dbl (Ben) = ♥ + bir minör
    ///   1♥ (Rakip) - Pas - 2♥ (Rakip) - Dbl (Ben) = ♠ + bir minör
    ///   1♦ (Rakip) - Pas - 2♦ (Rakip) - Dbl (Ben) = ♠ + ♥ (iki majör)
    /// 
    /// Öncelik: 22
    /// </summary>
    public class ResponsiveDouble : IKonvansiyon
    {
        public string Ad => "Responsive Double";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 22;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // 1. Son 3 teklifi kontrol et:
            //    [0] = rakip açtı
            //    [1] = partner pas
            //    [2] = rakip destekledi
            var sonTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi || h.PasMi)
                .ToList();

            if (sonTeklifler.Count < 3) return false;

            // Son teklif rakip mi?
            var sonTeklif = sonTeklifler[sonTeklifler.Count - 1];
            if (!durum.Rakipler.Contains(sonTeklif.Oyuncu)) return false;

            // Son teklif 2 seviyesinde mi?
            if (string.IsNullOrEmpty(sonTeklif.Teklif)) return false;
            if (sonTeklif.Teklif.Length == 0) return false;
            if (!char.IsDigit(sonTeklif.Teklif[0])) return false;
            int seviye = sonTeklif.Teklif[0] - '0';
            if (seviye != 2) return false;

            // 2 teklif önce (yani benim konuşma sıramdan önce) partner Pas demiş mi?
            // Aslında: Rakip 1X - Partner Pas - Rakip 2X formatına bakıyoruz.
            // Bu formatta "Rakip 1X - Partner Pas" olması gerekiyor.
            var oncekiTeklif = sonTeklifler[sonTeklifler.Count - 2];
            if (oncekiTeklif.Oyuncu != durum.Partner) return false;
            if (!oncekiTeklif.PasMi) return false;

            // 3 teklif önce rakip 1 seviyesinde mi açtı?
            var ilkTeklif = sonTeklifler[sonTeklifler.Count - 3];
            if (!durum.Rakipler.Contains(ilkTeklif.Oyuncu)) return false;
            if (string.IsNullOrEmpty(ilkTeklif.Teklif)) return false;
            if (ilkTeklif.Teklif.Length == 0) return false;
            if (!char.IsDigit(ilkTeklif.Teklif[0])) return false;
            int ilkSeviye = ilkTeklif.Teklif[0] - '0';
            if (ilkSeviye != 1) return false;

            // Aynı rakip rengi mi desteklendi?
            string ilkKoz = TekliftenKozCikar(ilkTeklif.Teklif);
            string sonKoz = TekliftenKozCikar(sonTeklif.Teklif);
            if (ilkKoz != sonKoz) return false;

            // 2. Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            // 3. Elimde diğer renklerde değer var mı?
            //    (en az 8 HP ve en az 2 tane 4+ kart rengi)
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 8) return false;

            // 4. En az iki tane 4+ kart rengi (rakip kozu hariç)
            int dortluRenkSayisi = 0;
            foreach (var renk in new[] { "Maça", "Kupa", "Karo", "Sinek" })
            {
                if (renk == ilkKoz) continue;
                if (ElDegerlendirici.RenkUzunlugu(el, renk) >= 4)
                    dortluRenkSayisi++;
            }

            return dortluRenkSayisi >= 2;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "Dbl";
        }

        private string TekliftenKozCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif)) return null;
            char sonKarakter = teklif[teklif.Length - 1];
            switch (sonKarakter)
            {
                case '♠': return "Maça";
                case '♥': return "Kupa";
                case '♦': return "Karo";
                case '♣': return "Sinek";
                case 'T':
                case 't': return "NT";
                default: return null;
            }
        }
    }
}