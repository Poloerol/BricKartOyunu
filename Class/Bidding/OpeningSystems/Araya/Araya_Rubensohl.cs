using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Araya
{
    /// <summary>
    /// RUBENSOHL (SAS kitabı s.102-104): rakip x açtı, ortak y ile araya girdi, SAĞDAKİ rakip de konuştu (yeni renk / x'i
    /// yükseltme / 1NT). Ortağın araya girişine cevap:
    ///   - 2 seviyesinde yeni renk NF (5+ renk, 1 seviyesinde 4+).
    ///   - 2NT, 3♣, 3♦, 3♥ = bir sonraki çağrıya TRANSFER (2NT→3♣, 3♣→3♦, 3♦→3♥, 3♥→3♠).
    ///   - Transfer edilen renk 2 seviyesinde söylenebiliyorsa davet+, söylenemiyorsa kompetitif (zayıf) ya da kuvvetli.
    ///   - Ekonomik cuebid (x'in cuebid'i, 2 seviyesinde) y'de 3+ tutuş 9+ HCP; pahalı cuebid (rakibin renginin cuebid'i)
    ///     4+ tutuş 10+ HCP; mixed raise (4 tutuş 7-9) = x'in 3 seviyesindeki cuebid'ine transfer.
    ///     Ekonomik cuebid 2 seviyesinde söylenemiyorsa 9+ HCP tutuş DBL ile gösterilir.
    ///   - DBL: 7-10 HCP, söylenmemiş majörde 4 (2 seviyesinde gösterilemiyorsa 5) kart, y'de 3'lü tutuş yok.
    ///   - 1NT doğal 8-11 (rakip renkte stoper), 3NT doğal 13+ stoperli, 3y blokatif (5+ tutuş zayıf).
    /// Araya girenin transfer kabulü: hedef çağrıyı söyler. Cevapçının ikinci hamlesi: oyun (kuvvetliyse) ya da Pas.
    /// Basitleştirmeler: stoper-sorma/gösterme sonrası gelişimler (transfer cuebid'in ardından açanın stoper göstermesi),
    /// 1NT/x-yükseltme cevaplarındaki ince ayrımlar ve araya girenin davet kabulü yok.
    /// </summary>
    public class Araya_Rubensohl : IAcilisGelisimi
    {
        public int Oncelik { get { return 21; } }
        public string Aciklama { get { return "Ortağın araya girişine Rubensohl cevapları (sağdaki rakip konuşmuşsa)"; } }

        private const string Sira = "♣♦♥♠";
        private static readonly string[] Merdiven = { "2NT", "3♣", "3♦", "3♥", "3♠" };

        private static List<IhaleHamlesi> Gercek(IhaleDurumu d) { return d.Gecmis.Where(h => h.GercekTeklifMi).ToList(); }
        private static int Ri(char c) { return Sira.IndexOf(c); }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }
        private static bool Major(char c) { return c == '♥' || c == '♠'; }

        /// <summary>Son teklif (seviye, renk; renk 'N' ise NT) üzerinde z renginde en ucuz seviye.</summary>
        private static int Seviye(int lv, char r, char z)
        {
            if (r == 'N') return lv + 1;
            return Ri(z) > Ri(r) ? lv : lv + 1;
        }

        /// <summary>"3"+w çağrısını hedefleyen transfer çağrısı (merdivenin bir altı); yoksa null.</summary>
        private static string Kaynak(char w)
        {
            int i = Array.IndexOf(Merdiven, "3" + w);
            return i > 0 ? Merdiven[i - 1] : null;
        }

        /// <summary>Transfer çağrısının hedef rengi (2NT→♣ ... 3♥→♠); transfer değilse null.</summary>
        private static char? Hedef(string t)
        {
            int i = Array.IndexOf(Merdiven, t);
            if (i < 0 || i + 1 >= Merdiven.Length) return null;
            return Merdiven[i + 1][1];
        }

        private static bool Gecerli(char x, int L, char y)
        {
            if (x == y) return false;
            if (L == 1) return Ri(y) > Ri(x) && x != '♠';
            if (L == 2) return (x == '♦' && y == '♣') || (x == '♥' && (y == '♣' || y == '♦')) || (x == '♠' && (y == '♣' || y == '♦' || y == '♥'));
            return false;
        }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = Gercek(d);
            if (g.Count < 3 || g.Count > 5) return null;
            if (!d.Rakipler.Contains(g[0].Oyuncu)) return null;
            string a = g[0].Teklif, oc = g[1].Teklif, rb = g[2].Teklif;
            if (a == null || oc == null || rb == null) return null;
            if (a.Length != 2 || a[0] != '1' || oc.Length != 2 || !char.IsDigit(oc[0])) return null;
            char x = a[1], y = oc[1];
            if (!Sira.Contains(x) || !Sira.Contains(y)) return null;
            int L = oc[0] - '0';
            if (!Gecerli(x, L, y)) return null;

            // Sağdaki rakibin teklifi
            int lr; char r;
            if (rb == "1NT") { lr = 1; r = 'N'; }
            else
            {
                if (rb.Length != 2 || !char.IsDigit(rb[0]) || !Sira.Contains(rb[1])) return null;
                lr = rb[0] - '0'; r = rb[1];
                if (lr > 2 || r == y) return null;
                if (!(lr > L || (lr == L && Ri(r) > Ri(y)))) return null;
            }

            Player ortak = IhaleDurumu.PartnerBul(d.AktifOyuncu);
            if (d.Rakipler.Contains(g[1].Oyuncu) || !d.Rakipler.Contains(g[2].Oyuncu)) return null;
            int ix = d.Gecmis.IndexOf(g[0]);
            if (ix < 0 || ix + 2 >= d.Gecmis.Count) return null;
            if (!ReferenceEquals(d.Gecmis[ix + 1], g[1]) || !ReferenceEquals(d.Gecmis[ix + 2], g[2])) return null;
            var e = new OcEl(el);

            // Cevapçı: x - y - R - (ben)
            if (g.Count == 3)
            {
                if (g[1].Oyuncu != ortak || d.Gecmis.Count != ix + 3) return null;
                return Cevap(e, x, y, lr, r);
            }

            // Çağrı transfer mi? (ben = araya giren) x - y - R - transfer - Pas - (ben)
            if (g.Count == 4)
            {
                if (g[1].Oyuncu != d.AktifOyuncu || g[3].Oyuncu != ortak) return null;
                if (d.Gecmis.Count != ix + 5 || !ReferenceEquals(d.Gecmis[ix + 3], g[3]) || d.Gecmis[ix + 4].Teklif != "Pas") return null;
                char? w = Hedef(g[3].Teklif);
                if (w == null || g[3].Teklif == "3" + y) return null;      // 3y blokatif yükseltme transfer değil
                if (w.Value == y) return null;
                return "3" + w.Value;
            }

            // Cevapçı ikinci hamle: x - y - R - transfer - Pas - hedef - Pas - (ben)
            if (g[1].Oyuncu != ortak || g[3].Oyuncu != d.AktifOyuncu || g[4].Oyuncu != ortak) return null;
            if (d.Gecmis.Count != ix + 7 || !ReferenceEquals(d.Gecmis[ix + 3], g[3]) || d.Gecmis[ix + 4].Teklif != "Pas"
                || !ReferenceEquals(d.Gecmis[ix + 5], g[4]) || d.Gecmis[ix + 6].Teklif != "Pas") return null;
            char? hedef = Hedef(g[3].Teklif);
            if (hedef == null || g[4].Teklif != "3" + hedef.Value) return null;
            return IkinciHamle(e, x, y, r, hedef.Value);
        }

        private static string IkinciHamle(OcEl e, char x, char y, char r, char w)
        {
            int hp = e.Hcp, s = Uz(e, y);
            if (w == y) return null;                                        // minör mixed raise: 3y söylendi, bitti
            if (w == r)                                                     // pahalı cuebid: tutuş 4+ 10+
            {
                if (Major(y)) return hp >= 12 ? "4" + y : (Ri(y) > Ri(w) ? "3" + y : null);
                return hp >= 13 ? "3NT" : null;
            }
            if (w == x && Major(y))                                         // mixed raise: 3y'ye çık
                return Ri(y) > Ri(x) ? "3" + y : null;
            // Transfer edilen kendi rengim (6+): kuvvetliyse oyun
            if (Uz(e, w) >= 6 && hp >= 13)
                return Major(w) ? "4" + w : "3NT";
            return null;
        }

        private static string Cevap(OcEl e, char x, char y, int lr, char r)
        {
            int hp = e.Hcp, s = Uz(e, y);
            bool rRenk = r != 'N';
            string y3 = "3" + y;
            string Cx = (rRenk && r != x) ? Seviye(lr, r, x).ToString() + x : null;   // ekonomik cuebid (x'in cuebid'i)
            string Cr = rRenk ? (lr + 1).ToString() + r : null;                      // pahalı cuebid (rakip rengin cuebid'i)

            // Blokatif: 5+ tutuş zayıf → 3y
            if (s >= 5 && hp >= 4 && hp <= 7) return y3;

            // Pahalı cuebid: 4+ tutuş 10+ HCP (2 seviyesinde direkt, 3 seviyesinde transfer ile)
            if (rRenk && s >= (r == x ? 3 : 4) && hp >= 10)
            {
                if (lr + 1 <= 2) return Cr;
                string src = Kaynak(r);
                return (src != null && src != y3) ? src : Cr;
            }

            // Mixed raise: 4+ tutuş 7-9 HCP
            if (s >= 4 && hp >= 7 && hp <= 9)
            {
                string src = null;
                if (Major(y)) { if (rRenk && r != x) src = Kaynak(x); }       // x'in 3 seviyesindeki cuebid'ine transfer
                else src = Kaynak(y);                                          // minör: 3y'ye transfer
                if (src != null && src != y3) return src;
            }

            // Ekonomik cuebid 9+ (2 seviyesinde söylenemiyorsa DBL)
            if (Cx != null && s >= 3 && hp >= 9)
                return Seviye(lr, r, x) <= 2 ? Cx : "Dbl";

            // Yükseltme 5-9 (2 seviyesinde)
            if (s >= 3 && hp >= 5 && hp <= 9)
            {
                int ly = Seviye(lr, r, y);
                if (ly == 2) return "2" + y;
            }

            // 6+ renk transferi: 2 seviyesinde söylenebiliyorsa davet+, söylenemiyorsa kompetitif
            foreach (char z in "♠♥♦♣".OrderByDescending(c => Uz(e, c)))
            {
                if (z == x || z == y || z == r || Uz(e, z) < 6) continue;
                int lz = Seviye(lr, r, z);
                if ((lz <= 2 && hp >= 10) || (lz == 3 && hp >= 5))
                {
                    string src = Kaynak(z);
                    if (src != null && src != y3) return src;
                }
            }

            // Doğal yeni renk NF
            foreach (char z in "♠♥♦♣".OrderByDescending(c => Uz(e, c)))
            {
                if (z == x || z == y || z == r) continue;
                int lz = Seviye(lr, r, z), u = Uz(e, z);
                if (lz == 1 && u >= 4 && hp >= 7) return "1" + z;
                if (lz == 2 && u >= 5 && hp >= 7) return "2" + z;
            }

            // DBL: 7-10 HCP, söylenmemiş renkte 4 (2 seviyesinde) / 5 (3 seviyesinde) kart
            if (rRenk && hp >= 7 && hp <= 10 && s < 3)
                foreach (char z in "♠♥♦♣")
                {
                    if (z == x || z == y || z == r) continue;
                    if (Uz(e, z) >= (Seviye(lr, r, z) <= 2 ? 4 : 5)) return "Dbl";
                }

            // NT
            if (rRenk && lr == 1 && hp >= 8 && hp <= 11 && e.Dengeli && e.Stopper(Ad(r))) return "1NT";
            if (hp >= 13 && e.Dengeli && e.Stopper(Ad(x)) && (!rRenk || e.Stopper(Ad(r)))) return "3NT";
            return "Pas";
        }
    }
}
