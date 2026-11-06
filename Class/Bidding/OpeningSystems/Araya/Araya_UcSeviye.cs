using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Araya
{
    /// <summary>
    /// AÇIŞIMIZA 3 SEVİYESİNDE ARAYA GİRİLİRSE (SAS kitabı s.120), zayıf NT versiyonu (StrongNT=false). Ortağımız 1x açtı, sağdaki rakip 3y dedi.
    ///   Dört özel dizi: 1♦-(3♥), 1♣-(3♥), 1♥-(3♦), 1♠-(3♦):
    ///     Dbl = 4+ "diğer majör" (♠; 1♠ açışında ♥) ve 9+ HCP, ya da girilen renkte stoper yok (11+ HCP, 3NT araştırması)
    ///     3 diğer majör = 5+ kart, FG (12+) · 4♦ (majör açılmışsa) = 3+M cuebid (13+)
    ///     Açanın 3M'i (Dbl sonrası, 4+ kart FG) üzerine cevapçı: majörde 3+ kart varsa 4M, yoksa 3NT.
    ///   Diğer 3 seviyesi girişlerde: Dbl = 11+ HCP, 3NT araştırması.
    /// Basitleştirmeler: "stoper yok" Dbl'si için 11+ HCP eşiği seçildi; kitaptaki "3x 5+x FG" yalnız 3 seviyesinde söylenebilen diğer majör için kodlandı;
    /// 4x cuebid'i yalnız 3♦ girişlerinde (1♥/1♠ açışı), 3M'den sonra açanın başka hamleleri genel motora bırakıldı.
    /// </summary>
    public class Araya_UcSeviye : IAcilisGelisimi
    {
        public int Oncelik { get { return 30; } }
        public string Aciklama { get { return "Açışımıza 3 seviyesinde araya girişlerde cevaplar (kitap s.120)"; } }

        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13 || d.Anlasma == null || d.Anlasma.StrongNT) return null;
            var g = d.Gecmis;
            int i = g.FindIndex(h => h.GercekTeklifMi);
            if (i < 0 || g.Count < i + 2) return null;
            string o = g[i].Teklif;
            if (o != "1♣" && o != "1♦" && o != "1♥" && o != "1♠") return null;
            for (int k = 0; k < i; k++) if (g[k].Teklif != "Pas") return null;
            var acici = g[i].Oyuncu;
            if (d.Rakipler.Contains(acici)) return null;
            string a = g[i + 1].Teklif;
            if (!d.Rakipler.Contains(g[i + 1].Oyuncu)) return null;
            if (a.Length != 2 || a[0] != '3' || "♣♦♥♠".IndexOf(a[1]) < 0 || a[1] == o[1]) return null;
            char x = a[1]; char oc = o[1];
            bool ozel = (oc == '♦' && x == '♥') || (oc == '♣' && x == '♥') || (oc == '♥' && x == '♦') || (oc == '♠' && x == '♦');
            char om = oc == '♠' ? '♥' : '♠';                  // diğer majör
            bool majorAcik = oc == '♥' || oc == '♠';
            var e = new OcEl(el);
            int hp = e.Hcp;
            var ortak = IhaleDurumu.PartnerBul(acici);

            if (g.Count == i + 2 && d.AktifOyuncu == ortak)
            {
                if (!ozel) return hp >= 11 ? "Dbl" : (hp >= 10 ? null : "Pas");
                if (majorAcik && x == '♦' && Uz(e, oc) >= 3 && hp >= 13) return "4♦";
                if (Uz(e, om) >= 5 && hp >= 12) return "3" + om;
                if ((Uz(e, om) >= 4 && hp >= 9) || (hp >= 11 && !e.Stopper(Ad(x)))) return "Dbl";
                return hp >= 10 ? null : "Pas";
            }
            if (!ozel) return null;
            // Açan: 1x-(3y)-Dbl-(Pas): diğer majörde 4+ → 3M (FG)
            if (g.Count == i + 4 && d.AktifOyuncu == acici && g[i + 2].Teklif == "Dbl" && g[i + 2].Oyuncu == ortak && g[i + 3].Teklif == "Pas")
                return Uz(e, om) >= 4 ? "3" + om : null;
            // Cevapçı: açan 3M dedi
            if (g.Count == i + 6 && d.AktifOyuncu == ortak && g[i + 2].Teklif == "Dbl" && g[i + 4].Teklif == "3" + om && g[i + 3].Teklif == "Pas" && g[i + 5].Teklif == "Pas")
                return Uz(e, om) >= 3 ? "4" + om : "3NT";
            return null;
        }
    }
}
