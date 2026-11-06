using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    /// <summary>
    /// GÜÇLÜ NT (15-17) AÇIŞIMIZA CEZA KONTRASI YAPILIRSA (SAS kitabı s.120): 1NT-(Dbl)-?
    ///   Pas = RDBL transferi (dengeli zayıf ya da 7+ HCP; açan RDBL der) · RDBL = 2♣ transferi (5+♣ ya da 5+♦, zayıf) · 2♣ = Stayman ·
    ///   2♦ = ♥ transferi · 2♥ = ♠ transferi · 2♠ = ♣ transferi (davet+) · 2NT = ♦ transferi (davet+) · 3x = blokaj (7+ kart).
    /// Açan transferleri tamamlar (Pas→RDbl, RDbl→2♣, 2♦→2♥, 2♥→2♠, 2♠→3♣, 2NT→3♦, 3x→Pas); RDBL-2♣ sonrası cevapçı ♣'de Pas, ♦'de 2♦ der.
    /// Koşullu kontrada "sistem geçerli" ve renk girişinde Rubensohl mevcut konvansiyon bayraklarına bırakıldı (kontranın ceza / koşullu ayrımı yapılamadığından
    /// her 1NT-(Dbl) ceza sayıldı). Basitleştirmeler: HCP verilmeyen satırlara eşik seçildi (davet+ = 8+, zayıf = 7 ve altı), 10+ HCP dengeli ve satırı olmayan eller genel motora kalır.
    /// </summary>
    public class OneNT_Araya : IAcilisGelisimi
    {
        public int Oncelik { get { return 43; } }
        public string Aciklama { get { return "Güçlü NT: 1NT-(Dbl ceza) sonrası cevaplar (kitap s.120)"; } }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13 || d.Anlasma == null || !d.Anlasma.StrongNT) return null;
            var g = d.Gecmis;
            int i = g.FindIndex(h => h.GercekTeklifMi);
            if (i < 0 || g[i].Teklif != "1NT" || g.Count < i + 2) return null;
            for (int k = 0; k < i; k++) if (g[k].Teklif != "Pas") return null;
            var acici = g[i].Oyuncu;
            if (d.Rakipler.Contains(acici) || g[i + 1].Teklif != "Dbl" || !d.Rakipler.Contains(g[i + 1].Oyuncu)) return null;
            var ortak = IhaleDurumu.PartnerBul(acici);
            var e = new OcEl(el);

            if (g.Count == i + 2 && d.AktifOyuncu == ortak) return Cevapci(e);
            if (g.Count == i + 4 && d.AktifOyuncu == acici && g[i + 3].Teklif == "Pas")
            {
                switch (g[i + 2].Teklif)
                {
                    case "Pas": return "RDbl";
                    case "RDbl": return "2♣";
                    case "2♦": return "2♥";
                    case "2♥": return "2♠";
                    case "2♠": return "3♣";
                    case "2NT": return "3♦";
                    case "3♣": case "3♦": case "3♥": case "3♠": return "Pas";
                }
                return null;
            }
            if (g.Count == i + 6 && d.AktifOyuncu == ortak && g[i + 2].Teklif == "RDbl" && g[i + 3].Teklif == "Pas" && g[i + 4].Teklif == "2♣" && g[i + 5].Teklif == "Pas")
            {
                if (e.C >= 5 && e.C >= e.D) return "Pas";
                if (e.D >= 5) return "2♦";
            }
            return null;
        }

        private static string Cevapci(OcEl e)
        {
            int hp = e.Hcp;
            if (e.S >= 7 && hp <= 7) return "3♠";
            if (e.H >= 7 && hp <= 7) return "3♥";
            if (e.D >= 7 && hp <= 7) return "3♦";
            if (e.C >= 7 && hp <= 7) return "3♣";
            if (e.S >= 5 && e.S >= e.H) return "2♥";
            if (e.H >= 5) return "2♦";
            if (hp >= 8 && e.C >= 5 && e.C >= e.D) return "2♠";
            if (hp >= 8 && e.D >= 5) return "2NT";
            if (hp >= 8 && (e.H == 4 || e.S == 4)) return "2♣";
            if (hp <= 7 && (e.C >= 5 || e.D >= 5)) return "RDbl";
            return hp >= 10 ? null : "Pas";
        }
    }
}
