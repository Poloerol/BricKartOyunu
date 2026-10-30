using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Support Double Cevabı: ortak (açıcı) Support Double yaptı, sol rakip Pas dedi, sıra bende (cevapçı).
    /// Ortağın TAM 3'lü desteği var → majörümü en ucuz seviyede söylerim (ölçümde davet eklemek fark yaratmadı).
    /// Rakibin renginde güçlü uzunluk (5+ kart, 3+ onör) varsa ceza pası.
    /// </summary>
    public class SupportDoubleCevap : IKonvansiyon
    {
        public string Ad => "Support Double Cevap";
        public string Aciklama => "Ortağın Support Double'ına cevap";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 18;

        private static bool Dizi(IhaleDurumu d, out string majorum, out IhaleHamlesi rakipTeklif)
        {
            majorum = null; rakipTeklif = null;
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13 || d.IhaleBittiMi()) return false;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi || h.KontrMi || h.SKontrMi).ToList();
            if (g.Count < 4) return false;
            var son = d.Gecmis.LastOrDefault();
            if (son == null || !son.PasMi || !d.Rakipler.Contains(son.Oyuncu)) return false;
            var dbl = g[g.Count - 1];
            if (!dbl.KontrMi || dbl.Oyuncu != d.Partner) return false;
            var rk = g[g.Count - 2];
            if (!rk.GercekTeklifMi || !d.Rakipler.Contains(rk.Oyuncu) || rk.Koz == "NT" || rk.Seviye > 2) return false;
            var benim = g.Where(h => h.Oyuncu == d.AktifOyuncu).ToList();
            if (benim.Count != 1 || !benim[0].GercekTeklifMi || benim[0].Seviye != 1) return false;
            if (benim[0].Koz != "Maça" && benim[0].Koz != "Kupa") return false;
            var ortak = g.Where(h => h.Oyuncu == d.Partner && !h.KontrMi).ToList();
            if (ortak.Count != 1 || ortak[0].Seviye != 1 || ortak[0].Koz == "NT" || ortak[0].Koz == benim[0].Koz) return false;
            majorum = benim[0].Koz; rakipTeklif = rk;
            return true;
        }

        public bool UygunMu(IhaleDurumu durum)
        {
            string m; IhaleHamlesi r;
            return Dizi(durum, out m, out r);
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            string m; IhaleHamlesi r;
            if (!Dizi(durum, out m, out r)) return "Pas";
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            int ru = ElDegerlendirici.RenkUzunlugu(el, r.Koz);
            int onor = el.Count(c => c.Suit == r.Koz && c.Value >= 10);
            if (ru >= 5 && onor >= 3) return "Pas";
            string sem = KozYardimcisi.KozSembolu(m);
            return KozYardimcisi.YasalaYukselt("2" + sem, r.Teklif);
        }
    }
}
