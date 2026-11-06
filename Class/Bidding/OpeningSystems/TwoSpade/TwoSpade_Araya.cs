using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.TwoSpade
{
    /// <summary>
    /// 2♠ AÇIŞIMIZA ARAYA GİRERLERSE (SAS kitabı s.71: "2♥ açışındaki gibi transferlidir"). Ortak 2♠ açtı, sağdaki rakip araya girdi, sıra bende.
    /// Kitap yalnızca bu cümleyi veriyor; 2♥ tablosunun (s.70) kayık hâli kodlandı (♥ yerine ♠ koz, doğal NF renk ♠ yerine ♥):
    ///   2♠ - (Dbl): RDbl = puan (10+ HCP) ya da ♠ honör · 2♥ doğal NF (5+♥) · 2NT 6+♣ · 3♣ 6+♦ · 3♥ = 3+♠ davet+ · 3♠ oynamak için
    ///   2♠ - (3♣/3♦/3♥): Dbl = ceza · yeni renk doğal F1R (5+ kart, 11+ HCP, 3 seviyesinde) · 3♠ oynamak için
    /// Basitleştirmeler 2♥ açışındakiyle aynıdır (RDbl eşikleri, ♠ honör = 7+ HCP ve ♠ ikili; "tutuş LD" kodlanmadı).
    /// Açıcının devamı: ortağın 2♥/2NT/3♣/3♠/RDbl'ine Pas; 3♥ daveti: azami (9+ HCP) 4♠, değilse 3♠.
    /// </summary>
    public class TwoSpade_Araya : IAcilisGelisimi
    {
        public int Oncelik { get { return 45; } }
        public string Aciklama { get { return "2♠ açışımıza araya girişlerde cevaplar (kitap s.71)"; } }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = d.Gecmis;
            int i = g.FindIndex(h => h.GercekTeklifMi);
            // Açıcının devamı: 2♠ (ben) - (Dbl) - ortak aksiyonu - (Pas)
            if (i >= 0 && g.Count == i + 4 && g[i].Teklif == "2♠" && g[i].Oyuncu == d.AktifOyuncu && g[i + 1].Teklif == "Dbl" &&
                d.Rakipler.Contains(g[i + 1].Oyuncu) && g[i + 2].Oyuncu == d.Partner && g[i + 3].Teklif == "Pas" &&
                d.Rakipler.Contains(g[i + 3].Oyuncu))
            {
                for (int k = 0; k < i; k++) if (g[k].Teklif != "Pas") return null;
                string pa = g[i + 2].Teklif;
                if (pa == "3♥") return new OcEl(el).Hcp >= 9 ? "4♠" : "3♠";
                if (pa == "RDbl" || pa == "2♥" || pa == "2NT" || pa == "3♣" || pa == "3♠") return "Pas";
                return null;
            }
            if (i < 0 || g.Count != i + 2) return null;                      // 2♠ + rakibin tek hamlesi, sıra bende
            if (g[i].Teklif != "2♠" || g[i].Oyuncu != d.Partner) return null;
            for (int k = 0; k < i; k++) if (g[k].Teklif != "Pas") return null;
            if (!d.Rakipler.Contains(g[i + 1].Oyuncu)) return null;
            string a = g[i + 1].Teklif;
            var e = new OcEl(el);
            int hp = e.Hcp;
            bool honor = el.Any(c => c.Suit == "Maça" && c.Value >= 12);

            if (a == "Dbl")
            {
                if (hp >= 13) return "RDbl";
                if (e.S >= 3 && hp >= 10) return "3♥";
                if (hp >= 10) return "RDbl";
                if (e.S >= 3) return "3♠";
                if (e.S == 2 && honor && hp >= 7) return "RDbl";
                if (e.H >= 5) return "2♥";
                if (e.C >= 6) return "2NT";
                if (e.D >= 6) return "3♣";
                return "Pas";
            }

            if (a != "3♣" && a != "3♦" && a != "3♥") return null;
            char x = a[1];
            if (hp >= 10 && Uz(e, x) >= 4) return "Dbl";                     // ceza
            if (hp >= 11)
            {
                char best = '\0'; int bu = 4;
                foreach (char r in new[] { '♥', '♦', '♣' })
                {
                    if (r == x || Uz(e, r) < 5 || Uz(e, r) < bu) continue;
                    bool ucte = (r == '♥' && x != '♥') || (r == '♦' && x == '♣');   // 3 seviyesinde söylenebilen renkler
                    if (!ucte) continue;
                    best = r; bu = Uz(e, r);
                }
                if (best != '\0') return "3" + best;
                return null;
            }
            if (e.S >= 3) return "3♠";
            return "Pas";
        }
    }
}
