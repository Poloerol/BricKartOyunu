using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ AÇIŞIMIZA ARAYA GİRERLERSE CEVAPÇININ ILK HAMLESİ (SAS kitabı s.111-113). Zayıf NT versiyonunda (StrongNT=false),
    /// ortak 1♣ açtı, sağdaki rakip araya girdi. Dizi ve satırlar:
    ///   Dbl   : RDbl 10+ (ceza) · 1♦ 4♥ 8+ / 5+♥ 5-10 · 1♥ 4♠ 8+ / 5+♠ 5-10 · 1♠ majör yok 7-9 · 1NT 5+♣ zayıf ya da FG · 2♣ 5+♦ zayıf ·
    ///           2♦ 5+♥ 11+ / 6+♥ 8-10 · 2♥ 5+♠ 11+ / 6+♠ 8-10 · 2♠ 5+♣ davet / 5-4 minör 12-14 · 2NT 11-12 dengeli 4+♣ · 3NT 4333 12-14
    ///   1♦/1♥/1♠ : Dbl = 4'lü (ya da 5+) kupa/maça (kitaptaki satırlar) · 1NT doğal 8-10 · 2NT iki diğer renk 5-5 7-10 ·
    ///           transfer / cuebid transfer ve davet satırları · 3x NV 6+ blokaj, V 6+ iyi renk davet
    ///   1NT   : 2♣ majörler · 2x 5+ NF · 2NT Lebensohl (6+M davet ya da 5+♦/5M FG) · 3x blokaj
    ///   2♣ (majörler): Pas/Dbl ceza · 2♦ 5+♦ NF · 2♥ 5+♣ davet+ · 2♠ 5+♦ FG · 2NT doğal · 3♣ rekabet · 3♦ blokaj/iyi renk davet · 3NT doğal
    ///   Zayıf 2♦/2♥/2♠ : Dbl 9+ ya da 12+ 5+M · doğal NF majörler · 2NT/3♣/3♦ rekabetçi+ ve 3NT 13-16 ("transfer cuebid" satırlarının anlamı belirsiz olduğu için kodlanmadı)
    /// Açan, transfer çağrılarını (kitaptaki "transfer"/"transfer cuebid") hedef rengi söyleyerek tamamlar.
    /// Basitleştirmeler: HCP sınırı verilmeyen satırlar (RDbl, doğal, 3♦ "stoper sorar") SEÇİM eşikleriyle kodlandı; "bomb" = 6+ kart,
    /// ≤7 HCP; satırlar yukarıdan aşağı sırayla denenir; transfer sonrası cevapçı ve açanın diğer devamları genel motora bırakıldı.
    /// "2♣ (NATÜREL)" tablosu (1♣ üzerine 2♣ doğal olamayacağı için) kodlanmadı.
    /// </summary>
    public class OneClubZ_Araya : IAcilisGelisimi
    {
        public int Oncelik { get { return 41; } }
        public string Aciklama { get { return "Zayıf NT: 1♣ açışına araya girişlerde cevaplar (kitap s.111-113)"; } }

        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }
        private const string Sira = "♣♦♥♠";
        private static int Ri(char c) { return Sira.IndexOf(c); }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        // Transfer çağrısı → hedef renk (açanın tamamlaması)
        private static char? Hedef(string araya, string cagri)
        {
            switch (araya + ">" + cagri)
            {
                case "1♦>2♦": return '♥';
                case "1♦>2♥": return '♠';
                case "1♥>2♣": return '♦';
                case "1♥>2♦": return '♣';
                case "1♥>2♥": return '♠';
                case "1♠>2♣": return '♦';
                case "1♠>2♦": return '♥';
                case "1♠>2♥": return '♣';
            }
            return null;
        }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13 || d.Anlasma == null || d.Anlasma.StrongNT) return null;
            var g = d.Gecmis;
            int i = g.FindIndex(h => h.GercekTeklifMi);
            if (i < 0 || g[i].Teklif != "1♣") return null;
            for (int k = 0; k < i; k++) if (g[k].Teklif != "Pas") return null;
            if (g.Count < i + 2) return null;
            var acici = g[i].Oyuncu;
            if (d.Rakipler.Contains(acici)) return null;
            var araya = g[i + 1];
            if (!d.Rakipler.Contains(araya.Oyuncu)) return null;
            string a = araya.Teklif;
            var e = new OcEl(el);
            bool zonda = ElDegerlendirici.ZondaMi(d.ZonNS, d.ZonEW, d.AktifOyuncu);

            // Açan: transfer tamamlama
            if (g.Count == i + 4 && d.AktifOyuncu == acici && g[i + 3].Teklif == "Pas" && g[i + 2].Oyuncu == IhaleDurumu.PartnerBul(acici))
            {
                string cg = g[i + 2].Teklif;
                var t = Hedef(a, cg);
                if (!t.HasValue) return null;
                int lv = cg[0] - '0'; char cs = cg[1];
                return (Ri(t.Value) > Ri(cs) ? lv : lv + 1).ToString() + t.Value;
            }
            // Cevapçı ilk hamle
            if (g.Count != i + 2 || d.AktifOyuncu != IhaleDurumu.PartnerBul(acici)) return null;
            switch (a)
            {
                case "Dbl": return Sec(KDbl(zonda), e, "Pas");
                case "1♦": return Sec(KRenk('♦', zonda), e, "Pas");
                case "1♥": return Sec(KRenk('♥', zonda), e, "Pas");
                case "1♠": return Sec(KRenk('♠', zonda), e, "Pas");
                case "1NT": return Sec(K1NT(), e, "Pas");
                case "2♣": return Sec(KMajor(zonda), e, "Pas");
                case "2♦": return Sec(KZayif2('♦'), e, "Pas");
                case "2♥": return Sec(KZayif2('♥'), e, "Pas");
                case "2♠": return Sec(KZayif2('♠'), e, "Pas");
            }
            return null;
        }

        private static bool Bomba(OcEl e, char s) { return Uz(e, s) >= 6 && e.Hcp <= 7; }
        private static bool IyiDavet(OcEl e, char s) { return Uz(e, s) >= 6 && e.Hcp >= 8 && e.Hcp <= 10 && ElDegerlendirici.IyiRenk(e.El, Ad(s)); }
        private static bool IkiMinor(OcEl e) { return (e.C >= 5 && e.D >= 4) || (e.D >= 5 && e.C >= 4); }

        /// <summary>3 seviyesi blokaj (NV) / iyi renk daveti (V) satırları, verilen suit'ler için.</summary>
        private static IEnumerable<OcKural> Bloklar(bool zonda, string suits)
        {
            foreach (char s in suits)
            {
                char c = s;
                yield return K("3" + c, x => zonda ? IyiDavet(x, c) : Bomba(x, c));
            }
        }

        private static List<OcKural> KDbl(bool zonda)
        {
            var l = new List<OcKural> {
                K("3NT", x => x.C == 4 && x.Hcp >= 12 && x.Hcp <= 14 && ElDegerlendirici.Dagilim4333(x.El, "Sinek") && ElDegerlendirici.KontrolSayisi(x.El) <= 3),
                K("2♦", x => (x.H >= 5 && x.Hcp >= 11) || (x.H >= 6 && x.Hcp >= 8 && x.Hcp <= 10)),
                K("2♥", x => (x.S >= 5 && x.Hcp >= 11) || (x.S >= 6 && x.Hcp >= 8 && x.Hcp <= 10)),
                K("2♠", x => (x.C >= 5 && x.Hcp >= 9 && x.Hcp <= 11) || (IkiMinor(x) && x.Hcp >= 12 && x.Hcp <= 14)),
                K("2NT", x => x.Hcp >= 11 && x.Hcp <= 12 && x.Dengeli && x.C >= 4),
                K("1♦", x => (x.H == 4 && x.Hcp >= 8) || (x.H >= 5 && x.Hcp >= 5 && x.Hcp <= 10)),
                K("1♥", x => (x.S == 4 && x.Hcp >= 8) || (x.S >= 5 && x.Hcp >= 5 && x.Hcp <= 10)),
                K("1♠", x => x.Hcp >= 7 && x.Hcp <= 9 && x.S < 4 && x.H < 4),
                K("RDbl", x => x.Hcp >= 10),
                K("1NT", x => x.C >= 5 && (x.Hcp <= 8 || x.Hcp >= 12)),
                K("2♣", x => x.D >= 5 && x.Hcp <= 8),
            };
            l.Add(K("3♣", x => x.C >= 7 && x.Hcp <= 7));
            l.AddRange(Bloklar(zonda, "♦♥♠"));
            return l;
        }

        /// <summary>1♣-(1R): R = ♦ / ♥ / ♠ (kitap s.111).</summary>
        private static List<OcKural> KRenk(char r, bool zonda)
        {
            var l = new List<OcKural>();
            // Transfer ve cuebid transfer satırları
            if (r == '♦')
            {
                l.Add(K("2♥", x => (x.S >= 5 && x.Hcp >= 11) || (x.S >= 6 && x.Hcp >= 8 && x.Hcp <= 10)));
                l.Add(K("2♦", x => (x.H >= 5 && x.Hcp >= 11) || (x.H >= 6 && x.Hcp >= 8 && x.Hcp <= 10)));
                l.Add(K("2♣", x => x.C >= 5 && x.Hcp >= 10));
                l.Add(K("2♠", x => x.C >= 5 && x.Hcp >= 8 && x.Hcp <= 9));
                l.Add(K("2NT", x => x.S >= 5 && x.H >= 5 && x.Hcp >= 7 && x.Hcp <= 10));
                l.Add(K("Dbl", x => (x.H == 4 && x.Hcp >= 8) || (x.H >= 5 && x.Hcp >= 5 && x.Hcp <= 10)));
                l.Add(K("1♥", x => (x.S == 4 && x.Hcp >= 8) || (x.S >= 5 && x.Hcp >= 5 && x.Hcp <= 10)));
                l.Add(K("1NT", x => x.Hcp >= 8 && x.Hcp <= 10 && x.Dengeli && x.Stopper("Karo")));
                l.Add(K("1♠", x => x.Hcp >= 8 && x.S < 4 && x.H < 4));
            }
            else if (r == '♥')
            {
                l.Add(K("2♥", x => (x.S >= 5 && x.Hcp >= 11) || (x.S >= 6 && x.Hcp >= 8 && x.Hcp <= 10)));
                l.Add(K("2♣", x => x.D >= 5 && x.Hcp >= 10));
                l.Add(K("2♦", x => x.C >= 5 && x.Hcp >= 10));
                l.Add(K("2♠", x => (x.C >= 5 && x.Hcp >= 8 && x.Hcp <= 9) || (IkiMinor(x) && x.Hcp >= 12 && x.Hcp <= 14)));
                l.Add(K("2NT", x => x.S >= 5 && x.D >= 5 && x.Hcp >= 7 && x.Hcp <= 10));
                l.Add(K("Dbl", x => (x.S == 4 && x.Hcp >= 8) || (x.S >= 5 && x.Hcp >= 5 && x.Hcp <= 10)));
                l.Add(K("1NT", x => x.Hcp >= 8 && x.Hcp <= 10 && x.Dengeli && x.Stopper("Kupa")));
                l.Add(K("1♠", x => x.Hcp >= 8 && x.S < 4));
            }
            else
            {
                l.Add(K("2♦", x => (x.H >= 5 && x.Hcp >= 11) || (x.H >= 6 && x.Hcp >= 8 && x.Hcp <= 10)));
                l.Add(K("2♣", x => x.D >= 5 && x.Hcp >= 10));
                l.Add(K("2♥", x => x.C >= 5 && x.Hcp >= 10));
                l.Add(K("2NT", x => x.H >= 5 && x.D >= 5 && x.Hcp >= 7 && x.Hcp <= 10));
                l.Add(K("Dbl", x => (x.H == 4 && x.Hcp >= 8) || (x.H >= 5 && x.Hcp >= 5 && x.Hcp <= 10)));
                l.Add(K("1NT", x => x.Hcp >= 8 && x.Hcp <= 10 && x.Dengeli && x.Stopper("Maça")));
                l.Add(K("2♠", x => x.Hcp >= 9 && x.H < 4));
            }
            l.Add(K("3♣", x => x.C >= 7 && x.Hcp <= 7));
            string diger = "♦♥♠".Replace(r.ToString(), "");
            l.AddRange(Bloklar(zonda, diger));
            return l;
        }

        /// <summary>1♣-(1NT) (kitap s.112): 2♣ majörler, 2x 5+ NF, 2NT Lebensohl, 3x blokaj.</summary>
        private static List<OcKural> K1NT()
        {
            return new List<OcKural> {
                K("2NT", x => (x.S >= 6 || x.H >= 6) && x.Hcp >= 8 || ((x.D >= 5 && (x.S >= 5 || x.H >= 5)) && x.Hcp >= 12)),
                K("2♣", x => x.S >= 5 && x.H >= 4 && x.Hcp <= 10 || x.H >= 5 && x.S >= 4 && x.Hcp <= 10),
                K("2♠", x => x.S >= 5 && x.Hcp <= 10),
                K("2♥", x => x.H >= 5 && x.Hcp <= 10),
                K("2♦", x => x.D >= 5 && x.Hcp <= 10),
                K("3♠", x => x.S >= 7 && x.Hcp <= 7),
                K("3♥", x => x.H >= 7 && x.Hcp <= 7),
                K("3♦", x => x.D >= 7 && x.Hcp <= 7),
            };
        }

        /// <summary>1♣-(2♣ majörler) (kitap s.112).</summary>
        private static List<OcKural> KMajor(bool zonda)
        {
            var l = new List<OcKural> {
                K("2♠", x => x.D >= 5 && x.Hcp >= 12),
                K("2♥", x => x.C >= 5 && x.Hcp >= 10),
                K("3♥", x => x.Hcp >= 12 && x.H <= 1 && x.Stopper("Sinek")),
                K("3♠", x => x.Hcp >= 12 && x.S <= 1 && x.Stopper("Sinek")),
                K("3NT", x => x.Hcp >= 13 && x.Dengeli),
                K("2♦", x => x.D >= 5 && x.Hcp <= 9),
                K("2NT", x => x.Hcp >= 11 && x.Hcp <= 12 && x.Dengeli),
                K("3♣", x => x.C >= 5 && x.Hcp <= 9),
                K("3♦", x => zonda ? IyiDavet(x, '♦') : Bomba(x, '♦')),
                K("Dbl", x => x.Hcp >= 10),
            };
            return l;
        }

        /// <summary>1♣-(zayıf 2R) (kitap s.113).</summary>
        private static List<OcKural> KZayif2(char r)
        {
            var l = new List<OcKural>();
            if (r == '♦')
            {
                l.Add(K("3NT", x => x.Hcp >= 13 && x.Hcp <= 16 && x.Dengeli && x.Stopper("Karo")));
                l.Add(K("3♠", x => x.S >= 5 && x.Hcp >= 12 && x.Stopper("Sinek")));
                l.Add(K("3♦", x => x.H >= 6 && x.Hcp >= 10));
                l.Add(K("3♥", x => x.S >= 6 && x.Hcp >= 10));
                l.Add(K("Dbl", x => x.Hcp >= 9 && (x.H == 4 || x.S == 4) || (x.Hcp >= 12 && (x.H >= 5 || x.S >= 5))));
                l.Add(K("2♥", x => x.H >= 5 && x.Hcp >= 8));
                l.Add(K("2♠", x => x.S >= 5 && x.Hcp >= 8));
                l.Add(K("2NT", x => x.C >= 5 && x.Hcp >= 7));
            }
            else if (r == '♥')
            {
                l.Add(K("3NT", x => x.Hcp >= 13 && x.Hcp <= 16 && x.Dengeli && x.Stopper("Kupa")));
                l.Add(K("3♠", x => x.S >= 5 && x.Hcp >= 12 && x.Stopper("Sinek")));
                l.Add(K("3♥", x => x.S >= 6 && x.Hcp >= 10));
                l.Add(K("Dbl", x => (x.Hcp >= 9 && x.S == 4) || (x.Hcp >= 12 && x.S >= 5)));
                l.Add(K("2♠", x => x.S >= 5 && x.Hcp >= 8));
                l.Add(K("2NT", x => x.C >= 5 && x.Hcp >= 7));
                l.Add(K("3♣", x => x.D >= 5 && x.Hcp >= 7));
            }
            else
            {
                l.Add(K("3NT", x => x.Hcp >= 13 && x.Hcp <= 16 && x.Dengeli && x.Stopper("Maça")));
                l.Add(K("3♥", x => x.H >= 5 && x.Hcp >= 10 && x.Hcp <= 12));
                l.Add(K("3♦", x => x.H >= 5 && ((x.Hcp >= 7 && x.Hcp <= 9) || x.Hcp >= 13)));
                l.Add(K("Dbl", x => (x.Hcp >= 9 && x.H == 4) || (x.Hcp >= 12 && x.H >= 5)));
                l.Add(K("2NT", x => x.C >= 5 && x.Hcp >= 7));
                l.Add(K("3♣", x => x.D >= 5 && x.Hcp >= 7));
            }
            return l;
        }
    }
}
