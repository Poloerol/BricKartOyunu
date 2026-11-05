using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Araya
{
    /// <summary>
    /// T/O KONTRADAN SONRA AÇANIN ORTAĞI KONUŞURSA (SAS kitabı s.108): rakip 1x açtı, ortak Dbl, sağdaki rakip R (1-2 seviye renk).
    ///   - Doğal rekabet: R'nin üstünde en ucuz seviyede 4+ kartlı renk (4-8 HCP, en çok 2 seviye).
    ///   - Rubensohl: 2NT→3♣, 3♣→3♦, 3♦→3♥, 3♥→3♠ transferleri (8+ HCP; majör 4+, minör 5+ kart).
    ///   - Dengeli oyun zorlayıcı el (13+): x'te stoper varsa x'in, yoksa R'nin cuebid'ine transfer.
    ///   - Ekonomik cue (2x): iki açılmamış renk de 4+, 5-9 HCP.   Dbl = ceza (R'de 4+, dengeli 9+ ile 3+).
    ///   - 1NT = doğal 7-10, x ve R'de stoper.
    /// Kontra yapan transferi 3 seviyesinde tamamlar; cevapçı 13+ ile oyun söyler.
    /// Basitleştirmeler: R'nin x'e destek olduğu (responsive) durum, davet/süper kabul ve kontra yapanın ikinci hamlesi yok.
    /// </summary>
    public class Araya_TORubensohl : IAcilisGelisimi
    {
        public int Oncelik { get { return 26; } }
        public string Aciklama { get { return "T/O kontradan sonra açanın ortağı konuşursa (Rubensohl)"; } }

        private const string Sira = "♣♦♥♠";
        private static readonly string[] Merdiven = { "2NT", "3♣", "3♦", "3♥", "3♠" };
        private static List<IhaleHamlesi> Gercek(IhaleDurumu d) { return d.Gecmis.Where(h => h.GercekTeklifMi).ToList(); }
        private static int Ri(char c) { return Sira.IndexOf(c); }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }
        private static bool Major(char c) { return c == '♥' || c == '♠'; }
        private static string Kaynak(char hedef) { int i = Array.IndexOf(Merdiven, "3" + hedef); return i <= 0 ? null : Merdiven[i - 1]; }
        private static string Ucuz(int lv, char son, char z) { return (Ri(z) > Ri(son) ? lv : lv + 1).ToString() + z; }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = Gercek(d);
            if (g.Count < 2 || g.Count > 4) return null;
            if (!d.Rakipler.Contains(g[0].Oyuncu) || !d.Rakipler.Contains(g[1].Oyuncu)) return null;
            string a = g[0].Teklif, r = g[1].Teklif;
            if (a == null || a.Length != 2 || a[0] != '1' || !Sira.Contains(a[1])) return null;
            if (r == null || r.Length != 2 || (r[0] != '1' && r[0] != '2') || !Sira.Contains(r[1]) || r[1] == a[1]) return null;
            char x = a[1], R = r[1]; int lv = r[0] - '0';
            int ix = d.Gecmis.IndexOf(g[0]);
            if (ix < 0 || ix + 3 > d.Gecmis.Count) return null;
            if (d.Gecmis[ix + 1].Teklif != "Dbl" || !ReferenceEquals(d.Gecmis[ix + 2], g[1])) return null;
            Player ortak = IhaleDurumu.PartnerBul(d.AktifOyuncu);
            var e = new OcEl(el);

            // Cevapçı: x - Dbl(ortak) - R - (ben)
            if (g.Count == 2)
            {
                if (d.Gecmis[ix + 1].Oyuncu != ortak || d.Gecmis.Count != ix + 3) return null;
                return Cevap(e, x, R, lv);
            }
            // Kontra yapan: x - Dbl(ben) - R - transfer - Pas - (ben)
            if (g.Count == 3)
            {
                if (d.Gecmis[ix + 1].Oyuncu != d.AktifOyuncu || g[2].Oyuncu != ortak) return null;
                if (d.Gecmis.Count != ix + 5 || !ReferenceEquals(d.Gecmis[ix + 3], g[2]) || d.Gecmis[ix + 4].Teklif != "Pas") return null;
                int i = Array.IndexOf(Merdiven, g[2].Teklif);
                if (i < 0 || i >= Merdiven.Length - 1) return null;
                return Merdiven[i + 1];
            }
            // Cevapçı ikinci hamle: x - Dbl - R - transfer(ben) - Pas - tamamlama - Pas - (ben)
            if (d.Gecmis[ix + 1].Oyuncu != ortak || g[2].Oyuncu != d.AktifOyuncu || g[3].Oyuncu != ortak) return null;
            if (d.Gecmis.Count != ix + 7 || !ReferenceEquals(d.Gecmis[ix + 3], g[2]) || d.Gecmis[ix + 4].Teklif != "Pas"
                || !ReferenceEquals(d.Gecmis[ix + 5], g[3]) || d.Gecmis[ix + 6].Teklif != "Pas") return null;
            if (Array.IndexOf(Merdiven, g[2].Teklif) < 0) return null;
            char hedef = g[3].Teklif[1];
            if (e.Hcp < 13) return "Pas";
            if (hedef == x || hedef == R) return "3NT";
            return Major(hedef) ? "4" + hedef : (e.Stopper(Ad(x)) ? "3NT" : "5" + hedef);
        }

        private static string Cevap(OcEl e, char x, char R, int lv)
        {
            int hp = e.Hcp, ur = Uz(e, R);
            // Ceza kontrası
            if (ur >= 4 && hp >= 7) return "Dbl";
            if (ur >= 3 && hp >= 9 && e.Dengeli) return "Dbl";
            // Dengeli oyun zorlayıcı: x'in (stoper varsa) ya da R'nin cuebid'ine transfer
            var adayR = Sira.Where(c => c != x && c != R && Uz(e, c) >= (Major(c) ? 4 : 5)).ToList();
            if (hp >= 13 && e.Dengeli && adayR.Count == 0)
            {
                string k = e.Stopper(Ad(x)) ? Kaynak(x) : (e.Stopper(Ad(R)) ? Kaynak(R) : null);
                if (k != null) return k;
            }
            // 8+ HCP: uzun renge Rubensohl transferi
            if (hp >= 8 && adayR.Count > 0)
            {
                char en = adayR.OrderByDescending(c => Uz(e, c)).ThenByDescending(c => Ri(c)).First();
                string k = Kaynak(en);
                if (k != null) return k;
            }
            // Ekonomik cue: iki açılmamış renk 4+, 5-9 HCP
            var acik = Sira.Where(c => c != x && c != R).ToList();
            if (hp >= 5 && hp <= 9 && acik.All(c => Uz(e, c) >= 4))
            {
                if (lv == 1 || Ri(x) > Ri(R)) return "2" + x;
            }
            // 1NT
            if (hp >= 7 && hp <= 10 && e.Dengeli && e.Stopper(Ad(x)) && e.Stopper(Ad(R))) return "1NT";
            // Doğal: R'nin üstünde en ucuz yeni renk (en çok 2 seviye)
            foreach (char c in Sira.Where(c => c != x && c != R).OrderByDescending(c => Uz(e, c)).ThenByDescending(c => Major(c) ? 1 : 0).ThenByDescending(c => Ri(c)))
            {
                if (Uz(e, c) < 4) break;
                string b = Ucuz(lv, R, c);
                if (b[0] <= '2') return b;
            }
            return "Pas";
        }
    }
}
