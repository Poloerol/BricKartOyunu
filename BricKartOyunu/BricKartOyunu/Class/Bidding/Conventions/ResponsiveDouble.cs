using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;   // KozYardimcisi için

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
        public string Aciklama => "Rakip renk anlaşmasına responsive kontr";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 22;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // STANDART biçim: ortak kontr/müdahale yaptı, sağdaki rakip rengini yükseltti
            if (OrtakMudahaleliMi(durum)) return true;

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
            string ilkKoz = KozYardimcisi.TekliftenKozCikar(ilkTeklif.Teklif);
            string sonKoz = KozYardimcisi.TekliftenKozCikar(sonTeklif.Teklif);
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
            foreach (var renk in KozYardimcisi.TumRenkler)
            {
                if (renk == ilkKoz) continue;
                if (ElDegerlendirici.RenkUzunlugu(el, renk) >= 4)
                    dortluRenkSayisi++;
            }

            return dortluRenkSayisi >= 2;
        }

        /// <summary>
        /// Standart Responsive Double: rakip 1x açtı, ORTAK kontr (takeout) ya da 1 seviyesinde
        /// renk müdahalesi yaptı, sağdaki rakip x'i 2 veya 3 seviyesine yükseltti → sıra bende.
        /// Dbl = kalan iki rengi gösterir (her birinde 4+ kart), 8+ HP.
        ///   1m (Dbl) 2m → ♠ + ♥        1M (Dbl) 2M → ♣ + ♦
        ///   1x (1y) 2x  → kalan iki renk
        /// </summary>
        private static bool OrtakMudahaleliMi(IhaleDurumu durum)
        {
            var g = durum.Gecmis.Where(h => h.GercekTeklifMi || h.KontrMi || h.PasMi).ToList();
            if (g.Count < 3) return false;
            var son = g[g.Count - 1];           // rakip yükseltmesi
            var ort = g[g.Count - 2];           // sıra: ortak (kontr/müdahale)
            // Beklenen dizi: rakip 1x – ortak (Dbl|1y) – rakip 2x/3x – (ben)
            var acis = g[g.Count - 3];
            if (!durum.Rakipler.Contains(son.Oyuncu) || !son.GercekTeklifMi) return false;
            if (son.Seviye < 2 || son.Seviye > 3) return false;
            if (ort.Oyuncu != durum.Partner) return false;
            if (!(ort.KontrMi || (ort.GercekTeklifMi && ort.Seviye == 1))) return false;
            if (!durum.Rakipler.Contains(acis.Oyuncu) || !acis.GercekTeklifMi || acis.Seviye != 1) return false;
            if (acis.Koz != son.Koz || son.Koz == "NT") return false;
            if (durum.KendiTeklifleri.Any(t => t != "Pas")) return false;

            var el = durum.AktifOyuncuEli;
            if (ElDegerlendirici.HCP(el) < 8) return false;

            var kalan = new List<string>();
            foreach (var r in KozYardimcisi.TumRenkler)
            {
                if (r == son.Koz) continue;
                if (ort.GercekTeklifMi && r == ort.Koz) continue;
                kalan.Add(r);
            }
            if (ort.KontrMi)
            {
                // Rakip minörse majörler, rakip majörse minörler
                bool minor = son.Koz == "Sinek" || son.Koz == "Karo";
                kalan = minor ? new List<string> { "Maça", "Kupa" } : new List<string> { "Karo", "Sinek" };
            }
            if (kalan.Count != 2) return false;
            return kalan.All(r => ElDegerlendirici.RenkUzunlugu(el, r) >= 4);
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "Dbl";
        }
    }
}