using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Takeout Kontra Yükseltme (kontra yapanın ikinci teklifi): rakip açtı (1-3 seviye renk), ben takeout kontra yaptım,
    /// sağ rakip pas dedi, ortak majörde en ucuz seviyede cevap verdi, sağ rakip pas dedi, sıra bende.
    ///
    /// Ortağın cevabı asgaridir (0-8 HP); kontra yapan güçlü elini bu turda göstermelidir. Motor Karnesi / Vugraph
    /// bord 14: 3♦ (rakip) - Dbl (20 HP) - Pas - 3♠ (0 HP) - Pas ... açıcı 20 HP ile pas geçip oyunu kaçırıyordu.
    ///
    /// Majör cevabında (kendimde 3+ destek): 1 seviyede 20+ HP → 4M, 17-19 HP → 3M; 2 seviyede 19+ → 4M, 17-18 → 3M;
    /// 3 seviyede (engel açılışına) 18+ → 4M. 3 kartlık destekte eşikler 1 HP yukarı.
    ///
    /// Öncelik 22; diğer devam sınıflarıyla çakışmaz (yalnızca düz majör cevabı).
    /// </summary>
    public class TakeoutKontraYukseltme : IKonvansiyon
    {
        public string Ad => "Takeout Double Yükseltme";
        public string Aciklama => "Takeout kontra yapanın, ortağın majör cevabına güçlü yükseltmesi";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 22;

        private static string Bul(IhaleDurumu d)
        {
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13 || d.IhaleBittiMi() || d.Gecmis == null) return null;
            int n = d.Gecmis.Count;
            if (n < 2 || !d.Gecmis[n - 1].PasMi || !d.Rakipler.Contains(d.Gecmis[n - 1].Oyuncu)) return null;
            var g = d.Gecmis.Where(h => !h.PasMi).ToList();
            if (g.Count != 3) return null;
            if (!d.Rakipler.Contains(g[0].Oyuncu) || !g[0].GercekTeklifMi || g[0].Koz == "NT") return null;
            if (g[0].Seviye > 3) return null;
            if (g[1].Oyuncu != d.AktifOyuncu || !g[1].KontrMi) return null;
            if (g[2].Oyuncu != d.Partner || !g[2].GercekTeklifMi || g[2].Koz == "NT") return null;
            if (g[2].Koz != "Kupa" && g[2].Koz != "Maça") return null;
            if (g[2].Koz == g[0].Koz) return null;
            if (!ReferenceEquals(d.Gecmis[n - 2], g[2])) return null;   // ortak cevap verdi, sağ rakip pas dedi

            var el = d.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            int dest = ElDegerlendirici.RenkUzunlugu(el, g[2].Koz);
            if (dest < 3) return null;
            int lv = g[2].Seviye;
            int ek = dest >= 4 ? 0 : 1;
            string sem = KozYardimcisi.KozSembolu(g[2].Koz);
            if (lv >= 4) return null;
            int oyun = lv == 1 ? 20 : lv == 2 ? 19 : 18;
            if (hp >= oyun + ek) return "4" + sem;
            if (lv <= 2 && hp >= 17 + ek) return "3" + sem;
            return null;
        }

        public bool UygunMu(IhaleDurumu durum) { return Bul(durum) != null; }
        public string TeklifVer(IhaleDurumu durum) { return Bul(durum) ?? "Pas"; }
    }
}
