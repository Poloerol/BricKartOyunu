using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Araya
{
    /// <summary>
    /// 2NT LEBENSOHL GELİŞMELERİ (SAS kitabı s.93-94): ortağımız rakibin açışına T/O kontra yaptı, sağdaki rakip Pas dedi / yükseltti,
    /// konuşma sırası bizde (ortağın kontrasına cevap zorunluluğu). Geçerli diziler (L=sol rakip, R=sağ rakip, kontra ortağın):
    ///   (2♥/2♠)-Dbl-(Pas)    (1♥/1♠)-Dbl-(2♥/2♠ yükseltme)    (2♦)-Dbl-(2♥/2♠ yeni renk)    (2♥)-Dbl-(2♠)
    /// CEVAPÇI (ilk hamle): basit (atlamasız) doğal renk 4+ kart 0-7 HCP · atlamalı doğal 5+ kart 8-10 HCP ·
    ///   2NT = Lebensohl (davet 8-10 ya da 0-7 ve 5+♣ zayıf) · 3♣ = FG röle · cuebid = FG, stoper sorar · 3NT = doğal 10-14 HCP (stoperli).
    /// DBL ATAN (2NT'den sonra, sağdaki Pas): 3♣ = minimum · 3♦ 5+♦ max · 3M 5+M max · cuebid = max, stoper sorar · 3NT = max dengeli.
    /// CEVAPÇININ 2. HAMLESİ: 3♣'dan sonra Pas (zayıf ♣) / 3♦ (5+♦ davet) / 3M (4M min davet) / cuebid (4M max davet);
    ///   3♦ ya da 3M'den sonra Pas ya da davet kabulü.
    /// Basitleştirmeler: kitabın "min/max" sınırı belirtilmediği için DBL atan 15+ HCP ile max sayılır; "3NT 14-17" satırı (2NT
    /// davetinden sonra mümkün olmadığından) ve "4♦ tutuş SI" satırları yok; 3♣ röle, cuebid, 3NT ve doğal hamlelerden sonrası genel motora kalır.
    /// </summary>
    public class Araya_Leben2NT : IAcilisGelisimi
    {
        public int Oncelik { get { return 28; } }
        public string Aciklama { get { return "Ortağın T/O kontrasına 2NT Lebensohl cevapları (kitap s.93-94)"; } }

        private const string Sira = "♣♦♥♠";
        private static int Ri(char c) { return Sira.IndexOf(c); }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }
        private static bool Renkli(string t) { return t != null && t.Length == 2 && char.IsDigit(t[0]) && Sira.IndexOf(t[1]) >= 0; }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = d.Gecmis;
            int i = g.FindIndex(h => h.GercekTeklifMi);
            if (i < 0) return null;
            for (int k = 0; k < i; k++) if (g[k].Teklif != "Pas") return null;
            if (g.Count < i + 3) return null;
            var A = g[i]; var D = g[i + 1]; var C = g[i + 2];
            if (!d.Rakipler.Contains(A.Oyuncu)) return null;
            if (D.Teklif != "Dbl" || d.Rakipler.Contains(D.Oyuncu)) return null;
            if (!d.Rakipler.Contains(C.Oyuncu) || D.Oyuncu == A.Oyuncu) return null;
            if (!Renkli(A.Teklif)) return null;
            char ar = A.Teklif[1]; int al = A.Teklif[0] - '0';

            // Dizi bağlamı: R = rakiplerin son renk (trump), opp = rakip renkleri
            char r;
            if (C.Teklif == "Pas")
            {
                if (al != 2 || (ar != '♥' && ar != '♠')) return null;
                r = ar;
            }
            else
            {
                if (!Renkli(C.Teklif) || C.Teklif[0] != '2') return null;
                char cr = C.Teklif[1];
                bool gecerli = (al == 1 && (ar == '♥' || ar == '♠') && cr == ar)
                            || (al == 2 && ar == '♦' && (cr == '♥' || cr == '♠'))
                            || (al == 2 && ar == '♥' && cr == '♠');
                if (!gecerli) return null;
                r = cr;
            }
            var opp = new HashSet<char> { ar, r };
            var e = new OcEl(el);
            int n = g.Count - (i + 3);

            // Ben cevapçıyım (ortak = kontracı), ilk hamlem
            if (n == 0 && D.Oyuncu == d.Partner) return Cevapci(e, r, opp);
            // Ben kontracıyım: ortak 2NT dedi, sağdaki rakip Pas
            if (n == 2 && D.Oyuncu == d.AktifOyuncu && g[i + 3].Teklif == "2NT" && g[i + 4].Teklif == "Pas" && g[i + 3].Oyuncu == d.Partner)
                return Kontraci(e, r, opp);
            // Cevapçının 2. hamlesi
            if (n == 4 && D.Oyuncu == d.Partner && g[i + 3].Teklif == "2NT" && g[i + 3].Oyuncu == d.AktifOyuncu
                && g[i + 4].Teklif == "Pas" && g[i + 6].Teklif == "Pas")
                return Cevapci2(e, r, opp, g[i + 5].Teklif);
            return null;
        }

        private static int Ucuz(char s, char r) { return Ri(s) > Ri(r) ? 2 : 3; }

        private static string Cevapci(OcEl e, char r, HashSet<char> opp)
        {
            int hp = e.Hcp;
            bool dur = e.Stopper(Ad(r));
            // Oyun değerinde
            if (hp >= 11)
            {
                if (!dur) return "3" + r;                                 // cuebid: FG, stoper sorar
                if (hp <= 14 && e.Dengeli) return "3NT";
                return "3♣";                                              // FG röle
            }
            var renkler = "♠♥♦".Where(s => !opp.Contains(s)).OrderByDescending(s => Uz(e, s)).ThenByDescending(s => Ri(s)).ToList();
            // Davet 8-10: atlamalı doğal 5+ (yalnız 2 seviyesinde söylenebilen renk için)
            if (hp >= 8)
            {
                foreach (char s in renkler) if (Uz(e, s) >= 5 && Ucuz(s, r) == 2) return "3" + s;
                return "2NT";
            }
            // 0-7: basit doğal 4+ renk
            foreach (char s in renkler)
                if (Uz(e, s) >= 4) return Ucuz(s, r).ToString() + s;
            if (e.C >= 5 && !opp.Contains('♣')) return "2NT";           // ♣ zayıf
            return "Pas";
        }

        private static string Kontraci(OcEl e, char r, HashSet<char> opp)
        {
            if (e.Hcp < 15) return "3♣";                                 // minimum
            var major = "♥♠".Where(s => !opp.Contains(s) && Uz(e, s) >= 5).OrderByDescending(s => Uz(e, s)).ThenByDescending(s => Ri(s)).ToList();
            if (major.Count > 0) return "3" + major[0];
            if (!opp.Contains('♦') && e.D >= 5) return "3♦";
            if (e.Dengeli && e.Stopper(Ad(r))) return "3NT";
            return "3" + r;                                              // cuebid: max, stoper sorar
        }

        private static string Cevapci2(OcEl e, char r, HashSet<char> opp, string cevap)
        {
            int hp = e.Hcp;
            var majors = "♥♠".Where(s => !opp.Contains(s)).ToList();
            char? m = majors.Where(s => Uz(e, s) >= 4).OrderByDescending(s => Uz(e, s)).Cast<char?>().FirstOrDefault();
            if (cevap == "3♣")
            {
                if (hp <= 7) return "Pas";                                // zayıf ♣
                if (!opp.Contains('♦') && e.D >= 5) return "3♦";          // 5+♦ davet
                if (m.HasValue) return hp >= 10 ? "3" + r : "3" + m.Value;   // 4M davet: min = 3M, max = cuebid
                return "Pas";
            }
            if (cevap == "3♦")
            {
                if (hp >= 8 && m.HasValue) return "3" + m.Value;
                return "Pas";
            }
            if (cevap.Length == 2 && (cevap[1] == '♥' || cevap[1] == '♠') && cevap[0] == '3' && cevap[1] != r)
            {
                if (hp >= 8 && Uz(e, cevap[1]) >= 3) return "4" + cevap[1];
                return "Pas";
            }
            return null;                                                  // cuebid / 3NT: genel motor
        }
    }
}
