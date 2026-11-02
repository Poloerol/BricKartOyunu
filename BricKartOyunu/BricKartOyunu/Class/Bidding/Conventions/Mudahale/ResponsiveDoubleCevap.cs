using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;   // KozYardimcisi için

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Responsive Double Cevabı (advancer): ortak Responsive Double yaptı, açıcı Pas dedi, sıra bende.
    /// Rakibin yükselttiği rengi hariç, ortağın gösterdiği iki rengin uzununu en ucuz seviyede söylerim.
    /// Rakip renginde 5+ kart ve 3+ onör varsa ceza pası.
    /// </summary>
    public class ResponsiveDoubleCevap : IKonvansiyon
    {
        public string Ad => "Responsive Double Cevap";
        public string Aciklama => "Ortağın Responsive Double'ına cevap";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 22;

        private static bool Dizi(IhaleDurumu d, out IhaleHamlesi yuk, out List<string> adaylar)
        {
            yuk = null; adaylar = null;
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13 || d.IhaleBittiMi()) return false;
            var son = d.Gecmis.LastOrDefault();
            if (son == null || !son.PasMi || !d.Rakipler.Contains(son.Oyuncu)) return false;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi || h.KontrMi || h.SKontrMi).ToList();
            if (g.Count < 3) return false;
            var dbl = g[g.Count - 1];
            if (!dbl.KontrMi || dbl.Oyuncu != d.Partner) return false;
            var yk = g[g.Count - 2];
            if (!yk.GercekTeklifMi || !d.Rakipler.Contains(yk.Oyuncu) || yk.Koz == "NT") return false;
            if (yk.Seviye < 2 || yk.Seviye > 3) return false;
            var acis = g.FirstOrDefault(h => d.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (acis == null || acis.Seviye != 1 || acis.Koz != yk.Koz || acis.Oyuncu == yk.Oyuncu) return false;
            // benim önceki eylemim: yok, 1 seviyesinde renk ya da kontr
            var benim = g.Where(h => h.Oyuncu == d.AktifOyuncu).ToList();
            if (benim.Count > 1) return false;
            var ort = g.Where(h => h.Oyuncu == d.Partner).ToList();
            if (ort.Count != 1) return false;   // ortak yalnız bu kontrü yaptı
            var kalan = KozYardimcisi.TumRenkler.Where(r => r != yk.Koz).ToList();
            if (benim.Count == 1)
            {
                if (benim[0].GercekTeklifMi)
                {
                    if (benim[0].Seviye != 1 || benim[0].Koz == "NT") return false;
                    kalan.Remove(benim[0].Koz);
                }
                else if (benim[0].KontrMi)
                {
                    bool minor = yk.Koz == "Sinek" || yk.Koz == "Karo";
                    kalan = minor ? new List<string> { "Maça", "Kupa" } : new List<string> { "Karo", "Sinek" };
                }
                else return false;
            }
            yuk = yk; adaylar = kalan;
            return true;
        }

        public bool UygunMu(IhaleDurumu durum)
        {
            IhaleHamlesi y; List<string> a;
            return Dizi(durum, out y, out a);
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            IhaleHamlesi y; List<string> a;
            if (!Dizi(durum, out y, out a)) return "Pas";
            var el = durum.AktifOyuncuEli;
            int ru = ElDegerlendirici.RenkUzunlugu(el, y.Koz);
            int onor = el.Count(c => c.Suit == y.Koz && c.Value >= 10);
            if (ru >= 5 && onor >= 3) return "Pas";
            string renk = a.OrderByDescending(r => ElDegerlendirici.RenkUzunlugu(el, r))
                           .ThenByDescending(r => Array.IndexOf(new[] { "Sinek", "Karo", "Kupa", "Maça" }, r)).First();
            return KozYardimcisi.YasalaYukselt("2" + KozYardimcisi.KozSembolu(renk), y.Teklif);
        }
    }
}
