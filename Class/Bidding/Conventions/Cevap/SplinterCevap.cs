using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Splinter Cevabı (açıcı): 1M - 4m (ya da 1♥ - 3♠) splinter'ına açıcının cevabı.
    /// Kısa renkte boşa giden değer varsa ve el güçsüzse 4M ile oyunu kapat.
    /// 17+ HP ve kısa renk dışında bir asım varsa ucuz kontrol (cue) göster; yoksa 4M.
    /// </summary>
    public class SplinterCevap : IKonvansiyon
    {
        public string Ad => "Splinter Cevap";
        public string Aciklama => "Splinter sonrası açıcının cevabı";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 15;

        private static bool Dizi(IhaleDurumu d, out string koz, out string kisa)
        {
            koz = null; kisa = null;
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13 || d.IhaleBittiMi()) return false;
            if (d.Anlasma != null && !d.Anlasma.Splinter) return false;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (g.Count != 2) return false;
            if (g[0].Oyuncu != d.AktifOyuncu || g[1].Oyuncu != d.Partner) return false;
            if (g[0].Seviye != 1 || (g[0].Koz != "Maça" && g[0].Koz != "Kupa")) return false;
            var r = g[1];
            bool ok = (r.Seviye == 4 && (r.Koz == "Karo" || r.Koz == "Sinek")) ||
                      (g[0].Koz == "Kupa" && r.Seviye == 3 && r.Koz == "Maça");
            if (!ok) return false;
            koz = g[0].Koz; kisa = r.Koz;
            return true;
        }

        public bool UygunMu(IhaleDurumu durum)
        {
            string k, s;
            return Dizi(durum, out k, out s);
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            string koz, kisa;
            if (!Dizi(durum, out koz, out kisa)) return "Pas";
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            string son = "4" + KozYardimcisi.KozSembolu(koz);
            if (hp >= 17)
            {
                string[] sira = { "Sinek", "Karo", "Kupa", "Maça" };
                int kisaSira = Array.IndexOf(sira, kisa);
                int kozSira = Array.IndexOf(sira, koz);
                foreach (var r in sira)
                {
                    int i = Array.IndexOf(sira, r);
                    if (r == koz || r == kisa) continue;
                    // 4 seviyesinde, 4M'nin altında olan ve splinter teklifinden sonra gelen renkler
                    bool asagida = (kisa == "Maça") ? true : i > kisaSira;
                    if (!asagida || i > kozSira) continue;
                    if (el.Any(c => c.Suit == r && c.Value == 14) || ElDegerlendirici.RenkUzunlugu(el, r) == 0)
                        return "4" + KozYardimcisi.KozSembolu(r);
                }
            }
            return son;
        }
    }
}
