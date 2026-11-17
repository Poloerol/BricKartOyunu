using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;   // KozYardimcisi için

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Negatif Kontra Devam (açıcının ikinci teklifi): ben açtım (1 seviye renk ya da zayıf iki), sol rakip renk söyledi,
    /// ortak negatif kontra yaptı, sağ rakip pas dedi, sıra bende.
    ///
    /// Motor Karnesi (30.000 bord): bu dizide açıcı rakibin rengi 3 kart ya da daha kısayken kontrayı pas geçiyordu
    /// (ceza pası); 269 bordda ortalama sapma 8,3 IMP (genel ortalama 5,3). Negatif kontra cezaya çevrilmeyecekse
    /// açıcı teklif vermelidir.
    ///
    /// Kurallar (rakibin renginde 4+ kartım varsa konvansiyon uygulanmaz: ceza pası serbest, eski mantık karar verir):
    ///   1) Rakibin söylemediği, benim de açmadığım majörde 4+ kart: o majörü en ucuz seviyede söylerim.
    ///   2) 6+ kartlı kendi rengim: en ucuz seviyede tekrar.
    ///   3) Dengeli ve rakip renginde durdurucu: en ucuz NT (15-17 HP: bir seviye yukarı).
    ///   4) Aksi halde: rakip dışındaki en uzun renk (eşitlikte açış rengi) en ucuz seviyede.
    ///
    /// Öncelik 22: Responsive Double Cevap ile aynı grup; Temel Mantık yedeğinden önce.
    /// </summary>
    public class NegatifKontraDevam : IKonvansiyon
    {
        public string Ad => "Negatif Kontra Devam";
        public string Aciklama => "Ortağın negatif kontrasından sonra açıcının ikinci teklifi";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 22;

        private static bool Dizi(IhaleDurumu d, out IhaleHamlesi acis, out IhaleHamlesi mud)
        {
            acis = null; mud = null;
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13 || d.IhaleBittiMi()) return false;
            var son = d.Gecmis.LastOrDefault();
            if (son == null || !son.PasMi || !d.Rakipler.Contains(son.Oyuncu)) return false;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi || h.KontrMi || h.SKontrMi).ToList();
            if (g.Count != 3) return false;                                  // açış, müdahale, kontra: başka teklif yok
            if (g[0].Oyuncu != d.AktifOyuncu || !g[0].GercekTeklifMi || g[0].Koz == "NT") return false;
            if (g[0].Seviye > 2) return false;                               // 1 seviye renk ya da zayıf iki
            if (!d.Rakipler.Contains(g[1].Oyuncu) || !g[1].GercekTeklifMi || g[1].Koz == "NT") return false;
            if (g[1].Seviye > 3) return false;
            if (!g[2].KontrMi || g[2].Oyuncu != d.Partner) return false;
            acis = g[0]; mud = g[1];
            return true;
        }

        public bool UygunMu(IhaleDurumu durum)
        {
            IhaleHamlesi a, m;
            if (!Dizi(durum, out a, out m)) return false;
            return ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, m.Koz) <= 3;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            IhaleHamlesi acis, mud;
            if (!Dizi(durum, out acis, out mud)) return "Pas";
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            string[] sira = { "Sinek", "Karo", "Kupa", "Maça" };
            string son = mud.Teklif;

            Func<string, string> En = renk => KozYardimcisi.YasalaYukselt("1" + KozYardimcisi.KozSembolu(renk), son);

            // 1) Ortağın gösterdiği majör (rakibin söylemediği, benim açmadığım) 4+ kart
            string major = new[] { "Kupa", "Maça" }
                .Where(r => r != mud.Koz && r != acis.Koz && ElDegerlendirici.RenkUzunlugu(el, r) >= 4)
                .OrderByDescending(r => ElDegerlendirici.RenkUzunlugu(el, r))
                .ThenBy(r => Array.IndexOf(sira, r))
                .FirstOrDefault();
            if (major != null) return En(major);

            // 2) 6+ kartlı kendi rengim
            if (ElDegerlendirici.RenkUzunlugu(el, acis.Koz) >= 6) return En(acis.Koz);

            // 3) Dengeli + rakip renginde durdurucu: NT
            if (ElDegerlendirici.DengeliEl(el) && ElDegerlendirici.StopperVar(el, mud.Koz))
            {
                string nt = KozYardimcisi.YasalaYukselt("1NT", son);
                if (hp >= 15 && hp <= 17) nt = KozYardimcisi.YasalaYukselt("2NT", son);
                else if (hp >= 18) nt = KozYardimcisi.YasalaYukselt("3NT", son);
                return nt;
            }

            // 4) Rakip dışındaki en uzun renk (eşitlikte açış rengi, sonra yüksek renk)
            string en = sira.Where(r => r != mud.Koz)
                .OrderByDescending(r => ElDegerlendirici.RenkUzunlugu(el, r))
                .ThenByDescending(r => r == acis.Koz ? 1 : 0)
                .ThenByDescending(r => Array.IndexOf(sira, r))
                .First();
            return En(en);
        }
    }
}
