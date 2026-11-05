using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Araya
{
    /// <summary>
    /// ORTAĞIN ARAYA GİRİŞİNE CEVAPLAR (SAS kitabı s.97-101): rakip x açtı, ortak y ile araya girdi, sağdaki rakip Pas.
    /// Kitaptaki 12 tablonun ortak kuralı:
    ///   - Transfer renkleri: x'ten sonra y'ye kadar (çevrimsel ♣♦♥♠) kalan renkler; x'in cuebid'i ilk renge,
    ///     her transfer rengi bir sonrakine transferdir (en ucuz seviyeden).
    ///   - Ekonomik cuebid (y'nin hemen altındaki renk): y'de 3+ tutuş ve 9+ HCP (ya da 15+ dengeli).
    ///   - Yükseltme (2y) 5-9 HCP; mixed raise = x'in sıçramalı cuebid'i (3x); splinter; FSJS; blokatif 3y.
    ///   - 1NT 8-11 doğal (x'te stoper), 2 seviyesindeki girişte 2NT doğal 9-11; 3x = stoper sorar.
    /// Araya girenin transfer kabulü: transfer rengini en ucuz seviyeden söyler.
    /// Basitleştirmeler: HCP eşikleri ve blokatif kararları sabit; sonraki gelişimler (max/min kabul, süper kabul) yok.
    /// </summary>
    public class Araya_Cevap : IAcilisGelisimi
    {
        public int Oncelik { get { return 20; } }
        public string Aciklama { get { return "Ortağın araya girişine cevaplar (transferler, mixed raise, splinter)"; } }

        private const string Siralama = "♣♦♥♠";
        private static List<IhaleHamlesi> Gercek(IhaleDurumu d) { return d.Gecmis.Where(h => h.GercekTeklifMi).ToList(); }
        private static int R(char c) { return Siralama.IndexOf(c); }
        private static char Sonraki(char c) { return Siralama[(R(c) + 1) % 4]; }
        private static char Onceki(char c) { return Siralama[(R(c) + 3) % 4]; }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }
        private static bool Major(char c) { return c == '♥' || c == '♠'; }

        /// <summary>Son teklif (seviye, renk) üzerinden z renginde en ucuz seviye.</summary>
        private static string Ucuz(int sonSeviye, char sonRenk, char z)
        {
            int lv = R(z) > R(sonRenk) ? sonSeviye : sonSeviye + 1;
            if (lv > 7) return null;
            return lv.ToString() + z;
        }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private static bool Gecerli(char x, int L, char y)
        {
            if (x == y) return false;
            if (L == 1) return R(y) > R(x) && !(x == '♠');
            if (L == 2) return (x == '♦' && y == '♣') || (x == '♥' && (y == '♣' || y == '♦')) || (x == '♠' && (y == '♣' || y == '♦' || y == '♥'));
            return false;
        }

        private static List<char> TransferRenkleri(char x, char y)
        {
            var t = new List<char>();
            for (char c = Sonraki(x); c != y; c = Sonraki(c)) t.Add(c);
            return t;
        }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = Gercek(d);
            if (g.Count < 2 || g.Count > 3) return null;
            if (!d.Rakipler.Contains(g[0].Oyuncu)) return null;
            string a = g[0].Teklif, oc = g[1].Teklif;
            if (a.Length != 2 || a[0] != '1' || oc.Length != 2) return null;
            char x = a[1], y = oc[1];
            if (!Siralama.Contains(x) || !Siralama.Contains(y)) return null;
            int L = oc[0] - '0';
            if (!Gecerli(x, L, y)) return null;
            Player ortak = IhaleDurumu.PartnerBul(d.AktifOyuncu);
            if (g[1].Oyuncu != ortak && g[1].Oyuncu != d.AktifOyuncu) return null;

            // Ardışık sekans: x - araya giriş - Pas - (ben)   veya   x - araya giriş - Pas - transfer - Pas - (ben)
            int ix = d.Gecmis.IndexOf(g[0]);
            if (ix < 0 || ix + 1 >= d.Gecmis.Count || !ReferenceEquals(d.Gecmis[ix + 1], g[1])) return null;
            if (ix + 2 >= d.Gecmis.Count || d.Gecmis[ix + 2].Teklif != "Pas") return null;
            if (g.Count == 2 && d.Gecmis.Count != ix + 3) return null;
            if (g.Count == 3 && (ix + 4 >= d.Gecmis.Count || !ReferenceEquals(d.Gecmis[ix + 3], g[2]) || d.Gecmis.Count != ix + 5)) return null;
            var e = new OcEl(el);
            var T = TransferRenkleri(x, y);

            // ── Araya girenin transfer kabulü ──
            if (g.Count == 3)
            {
                if (g[1].Oyuncu != d.AktifOyuncu || g[2].Oyuncu != ortak) return null;
                string t = g[2].Teklif;
                if (t.Length != 2 || !char.IsDigit(t[0])) return null;
                char tr = t[1];
                int tl = t[0] - '0';
                int k = tr == x ? -1 : T.IndexOf(tr);
                if (tr != x && k < 0) return null;
                if (k + 1 >= T.Count) return null;                         // son transfer renginden sonrası transfer değil
                if (Ucuz(L, y, tr) != t) return null;                      // transfer beklenen en ucuz seviyede olmalı
                return Ucuz(tl, tr, T[k + 1]);
            }

            if (g[1].Oyuncu != ortak) return null;
            return Cevap(e, x, y, L, T);
        }

        private static string Cevap(OcEl e, char x, char y, int L, List<char> T)
        {
            int hp = e.Hcp;
            bool stp = e.Stopper(Ad(x));
            int s = Uz(e, y);
            char pred = Onceki(y);

            // 3NT: oynamak için (15+ dengeli, x'te stoper; 2 seviyesinde girişte 12+)
            if (L == 1 && hp >= 15 && e.Dengeli && stp) return "3NT";
            if (L == 2 && hp >= 12 && e.Dengeli && stp && hp >= 13) return "3NT";

            // Blokatif yükseltme: 5+ tutuş, zayıf el
            if (s >= 5 && hp <= 7) return "3" + y;

            // ── Tutuş (y'de 3+ kart) ──
            if (Major(y) && L == 1)
            {
                string raise2 = Ucuz(L, y, y);                              // 2y: 5-9 HCP
                string cue9 = Ucuz(L, y, pred);                             // ekonomik cuebid: 9+ HCP
                string fs = Fsjs(e, x, y, L, s, hp);
                if (fs != null) return fs;
                if (s >= 4)
                {
                    string spl = Splinter(e, y);
                    if (spl != null && hp >= 10) return spl;
                    if (hp >= 12) return "2NT";                             // 4+ tutuş 12+
                    if (hp >= 10) return "3" + x;                           // mixed raise: x'in sıçramalı cuebid'i
                    if (hp >= 9) return cue9;
                    if (hp >= 5) return raise2;
                }
                else if (s == 3)
                {
                    if (hp >= 9) return cue9;
                    if (hp >= 5) return raise2;
                }
            }
            else if (Major(y) && L == 2)
            {
                // 1♠ - (2♥) : 2NT 4+ tutuş 12+, 3♦ 3+ tutuş 9+, splinter
                if (s >= 4)
                {
                    string spl = Splinter(e, y);
                    if (spl != null && hp >= 10) return spl;
                    if (hp >= 12) return "2NT";
                }
                if (s >= 3 && hp >= 9) return Ucuz(L, y, pred);
            }
            else if (L == 1)
            {
                // y == ♦ (1♣ açılışı): 2♣ cuebid davet+, 2♦ yükseltme 5-9
                if (s >= 3 && hp >= 10) return Ucuz(L, y, pred);
                if (s >= 3 && hp >= 5 && !(hp >= 7 && (e.H >= 4 || e.S >= 4))) return Ucuz(L, y, y);
                if (hp >= 12 && hp <= 14 && e.Dengeli && stp && e.H < 4 && e.S < 4) return "2NT";
            }
            else
            {
                // minör ile 2 seviyesinde giriş: ekonomik cuebid 9+ HCP (3+ tutuş) veya 15+ dengeli
                if (s >= 3 && hp >= 9) return Ucuz(L, y, pred);
                if (hp >= 15 && e.Dengeli) return Ucuz(L, y, pred);
            }

            // 2 seviyesinde girişte 4M oyun: 6+ majör ve 12+ HCP
            if (L == 2 && hp >= 12)
                foreach (char m in new[] { '♠', '♥' })
                    if (m != x && m != y && Uz(e, m) >= 6) return "4" + m;

            // ── Transferler: 5+ renk, 7+ HCP ──
            int best = -1, bl = 0;
            for (int k = 0; k < T.Count; k++)
                if (Uz(e, T[k]) >= 5 && Uz(e, T[k]) > bl) { best = k; bl = Uz(e, T[k]); }
            if (best >= 0 && hp >= 7)
            {
                char kaynak = best == 0 ? x : T[best - 1];
                return Ucuz(L, y, kaynak);
            }
            // Zayıf 6+ renk: sıçrama (blokatif / zon durumuna göre davet)
            if (L == 2 && hp >= 5 && hp <= 8)
                foreach (char m in new[] { '♠', '♥', '♦' })
                    if (m != x && m != y && Uz(e, m) >= 6) return "3" + m;

            // ── Doğal renkler (transfer olmayanlar) ──
            foreach (char z in "♠♥♦♣".OrderByDescending(c => Uz(e, c)))
            {
                if (z == x || z == y || T.Contains(z)) continue;
                int u = Uz(e, z);
                string b = Ucuz(L, y, z);
                if (b == null) continue;
                bool birSeviye = b[0] == '1';
                if (birSeviye && u >= 4 && hp >= 7) return b;               // 1 seviyesi: F1R 4+ 7+
                if (!birSeviye && u >= 5 && hp >= 10) return b;             // 2 seviyesi: F1R 5+
            }

            // ── NT ──
            if (L == 1 && hp >= 8 && hp <= 11 && e.Dengeli && stp) return "1NT";
            if (L == 2 && hp >= 9 && hp <= 11 && e.Dengeli && stp) return "2NT";
            // 2 seviyesinde girişte stoper sorusu
            if (L == 2 && hp >= 13 && e.Dengeli && !stp) return "3" + x;

            // 1♣ - (1♦): 6+ majör ile 2M (davet veya blokatif), 7+ ile 3M
            if (L == 1 && y == '♦')
            {
                if (hp >= 5 && hp <= 11 && e.H >= 6) return "2♥";
                if (hp >= 5 && hp <= 11 && e.S >= 6) return "2♠";
                if (hp >= 5 && hp <= 12 && e.H >= 7) return "3♥";
                if (hp >= 5 && hp <= 12 && e.S >= 7) return "3♠";
            }
            return null;
        }

        /// <summary>FSJS: y'de 4+ tutuş, 10+ HCP ve başka bir 5+ renk → sıçramalı yeni renk.</summary>
        private static string Fsjs(OcEl e, char x, char y, int L, int s, int hp)
        {
            if (s < 4 || hp < 10) return null;
            foreach (char z in new[] { '♠', '♥', '♦', '♣' })
            {
                if (z == x || z == y || Uz(e, z) < 5) continue;
                return R(z) > R(y) ? "2" + z : "3" + z;
            }
            return null;
        }

        /// <summary>Splinter: y'de tutuş + kısalık (tek kart / void) → sıçrama: ♠ kısalığı 3♠ (♥ girişinde), diğerleri 4. seviye.</summary>
        private static string Splinter(OcEl e, char y)
        {
            foreach (char z in new[] { '♠', '♣', '♦', '♥' })
            {
                if (z == y) continue;
                if (Uz(e, z) > 1) continue;
                if (z == '♠' && R(y) < R('♠') && y == '♥') return "3♠";
                return "4" + z;
            }
            return null;
        }
    }
}
