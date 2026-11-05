using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.IkiRenk
{
    /// <summary>
    /// İKİ RENKLİ ELLERLE ARAYA GİRİŞLER (SAS kitabı s.105-107), rakip 1 renk açtı, doğrudan koltuk.
    /// Girişler (x = açılış rengi, "diğerleri" = x dışındaki 3 renk, ♣♦♥♠ sırasıyla o[0]&lt;o[1]&lt;o[2]):
    ///   2x (cuebid)  = en yüksek 2 renk (5-5);  1♣ açılışında 2♣ = majörler 5-4 (10-14 HCP), 2♦ = majörler 5-5.
    ///   2NT          = en yüksek + en alçak (top and bottom) 5-5.
    ///   3♣           = en alçak 2 renk (5-5).
    /// Cevapçı: basit tutuş = tercih (en ucuz seviyeden), atlamalı tutuş = blokatif, 2NT (ya da rakibin rengi) = davet+,
    /// yeni renk = F1R (zorlayıcı). Araya giren davete: maksimumsa oyun, değilse uzun rengini söyler.
    /// Basitleştirmeler: zon / pastan gelen ortak ayrımı (defansif - yapıcı) yok, 8-15 HCP ile 5-5 sabit aralık;
    /// 4. pozisyon (DBL / 1NT dağılımsal), 3x kuvvetli versiyonu ve P/C (3♣, 3♦, 3♥) cevapları yok.
    /// </summary>
    public class IkiRenk_Araya : IAcilisGelisimi
    {
        public int Oncelik { get { return 22; } }
        public string Aciklama { get { return "İki renkli araya girişler (cue, 2NT, 3♣) ve cevapları"; } }

        private const string Sira = "♣♦♥♠";
        private static List<IhaleHamlesi> Gercek(IhaleDurumu d) { return d.Gecmis.Where(h => h.GercekTeklifMi).ToList(); }
        private static int Ri(char c) { return Sira.IndexOf(c); }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static bool Major(char c) { return c == '♥' || c == '♠'; }

        /// <summary>Son teklif (seviye, renk; 'N' = NT) üzerinde z renginde en ucuz teklif.</summary>
        private static string Ucuz(int lv, char son, char z)
        {
            int l = son == 'N' ? lv + 1 : (Ri(z) > Ri(son) ? lv : lv + 1);
            return l > 7 ? null : l.ToString() + z;
        }

        private static List<char> Digerleri(char x) { return Sira.Where(c => c != x).ToList(); }

        /// <summary>Girişin gösterdiği iki renk (cue / 2NT / 3♣); tanınmazsa null.</summary>
        private static char[] Renkler(char x, string giris)
        {
            var o = Digerleri(x);                              // artan sıra
            if (giris == "2" + x) return new[] { o[2], o[1] };
            if (giris == "2NT") return new[] { o[2], o[0] };
            if (giris == "3♣" && x != '♣') return new[] { o[1], o[0] };
            if (giris == "3♣" && x == '♣') return new[] { o[1], o[0] };
            if (x == '♣' && giris == "2♦") return new[] { '♠', '♥' };
            return null;
        }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = Gercek(d);
            if (g.Count < 1 || g.Count > 3) return null;
            if (!d.Rakipler.Contains(g[0].Oyuncu)) return null;
            string a = g[0].Teklif;
            if (a == null || a.Length != 2 || a[0] != '1' || !Sira.Contains(a[1])) return null;
            char x = a[1];
            int ix = d.Gecmis.IndexOf(g[0]);
            if (ix < 0) return null;
            var e = new OcEl(el);
            Player ortak = IhaleDurumu.PartnerBul(d.AktifOyuncu);

            // ── Doğrudan koltuk: açılışın hemen ardından ben ──
            if (g.Count == 1)
            {
                if (d.Gecmis.Count != ix + 1) return null;
                return Giris(e, x);
            }

            string oc = g[1].Teklif;
            if (oc == null) return null;
            var R = Renkler(x, oc);
            if (R == null) return null;
            if (!ReferenceEquals(d.Gecmis.ElementAtOrDefault(ix + 1), g[1])) return null;

            // ── Cevapçı: x - giriş - Pas - (ben) ──
            if (g.Count == 2)
            {
                if (g[1].Oyuncu != ortak || d.Gecmis.Count != ix + 3 || d.Gecmis[ix + 2].Teklif != "Pas") return null;
                return Cevap(e, x, oc, R);
            }

            // ── Araya giren: x - giriş - Pas - davet - Pas - (ben) ──
            if (g[1].Oyuncu != d.AktifOyuncu || g[2].Oyuncu != ortak) return null;
            if (d.Gecmis.Count != ix + 5 || d.Gecmis[ix + 2].Teklif != "Pas" || !ReferenceEquals(d.Gecmis[ix + 3], g[2]) || d.Gecmis[ix + 4].Teklif != "Pas") return null;
            if (g[2].Teklif != DavetCagrisi(x, oc)) return null;
            return DavetCevabi(e, x, R);
        }

        // ───────────────────────── araya giren ─────────────────────────
        private static string Giris(OcEl e, char x)
        {
            int hp = e.Hcp;
            var o = Digerleri(x);
            // 1♣ açılışında majörler 5-4 (10-14 HCP): 2♣; 5-5 majörler: 2♦
            if (x == '♣')
            {
                if (e.S >= 5 && e.H >= 5 && hp >= 8 && hp <= 15) return "2♦";
                if (((e.S >= 5 && e.H == 4) || (e.S == 4 && e.H >= 5)) && hp >= 10 && hp <= 14) return "2♣";
            }
            if (hp < 8 || hp > 15) return null;
            var bes = o.Where(c => Uz(e, c) >= 5).ToList();
            if (bes.Count != 2) return null;
            if (Uz(e, x) >= 5) return null;                       // açılan renkte de uzunluk varsa dağılımsal değil
            Func<char, char, bool> Var = (p, q) => bes.Contains(p) && bes.Contains(q);
            if (Var(o[2], o[1])) return x == '♣' ? null : "2" + x;         // iki yüksek (1♣'da 2♦ yukarıda)
            if (Var(o[2], o[0])) return "2NT";                              // top and bottom
            if (Var(o[1], o[0])) return "3♣";                               // iki alçak
            return null;
        }

        // ───────────────────────── cevapçı ─────────────────────────
        private static string DavetCagrisi(char x, string oc)
        {
            if (oc == "2" + x || (x == '♣' && oc == "2♦")) return "2NT";
            if (oc == "2NT") return "3" + x;
            return x == '♣' ? "3NT" : "3" + x;                // 3♣ girişinde rakibin rengi (1♣'da yok)
        }

        private static int SonSeviye(string oc) { return oc[0] - '0'; }
        private static char SonRenk(string oc) { return oc.Length >= 2 && oc[1] == 'N' ? 'N' : oc[1]; }

        private static string Cevap(OcEl e, char x, string oc, char[] R)
        {
            int hp = e.Hcp;
            char a = R[0], b = R[1];
            int sa = Uz(e, a), sb = Uz(e, b);
            int lv = SonSeviye(oc); char son = SonRenk(oc);
            // Tercih: daha uzun tutuş; eşitse majör / yüksek renk
            char tercih = sa > sb ? a : (sb > sa ? b : (Ri(a) > Ri(b) ? a : b));
            int st = Uz(e, tercih);
            string ucuz = Ucuz(lv, son, tercih);

            // Atlamalı tutuş: 5+ tutuş, zayıf → blokatif
            if (st >= 5 && hp <= 7)
            {
                string j = Ucuz(lv + 1, son, tercih);
                if (j != null && j[0] <= '5') return j;
            }
            // Kuvvetli el ve tutuş: oyun / davet
            if (st >= 3 && hp >= 13)
            {
                if (Major(tercih)) return "4" + tercih;
                if (hp >= 15) return "3NT";
            }
            if (st >= 3 && hp >= 10)
            {
                string dv = DavetCagrisi(x, oc);
                if (dv != null && !(x == '♣' && oc == "3♣")) return dv;
            }
            // Yeni renk F1R: 5+ renk ve 10+ HCP (tutuş yok)
            if (st < 3 && hp >= 10)
                foreach (char z in "♠♥♦♣".OrderByDescending(c => Uz(e, c)))
                {
                    if (z == x || z == a || z == b || Uz(e, z) < 5) continue;
                    string nb = Ucuz(lv, son, z);
                    if (nb != null && nb[0] <= '3') return nb;
                }
            // Basit tutuş = tercih
            return ucuz;
        }

        private static string DavetCevabi(OcEl e, char x, char[] R)
        {
            int hp = e.Hcp;
            char a = R[0], b = R[1];
            int sa = Uz(e, a), sb = Uz(e, b);
            char uzun = sa > sb ? a : (sb > sa ? b : (Ri(a) > Ri(b) ? a : b));
            if (hp >= 12)
            {
                if (Major(uzun)) return "4" + uzun;
                return "3NT";
            }
            return "3" + uzun;
        }
    }
}
