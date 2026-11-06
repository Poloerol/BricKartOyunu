using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond
{
    /// <summary>
    /// 1♦ AÇIŞIMIZA ARAYA GİRERLERSE CEVAPÇININ ILK HAMLESİ (SAS kitabı s.113-115), zayıf NT versiyonu (StrongNT=false).
    ///   1♥ üzerine: Dbl 4♠ 8+ / 5+♠ 5-10 · 1♠ majörsüz 8+ · 1NT doğal 8-10 · 2♣ 4+♦ 5-9 ya da FG (transfer) · 2♦ 5+♣ 10+ (transfer cuebid) ·
    ///               2♥ 5+♠ 11+ / 6+♠ 8-10 · 2♠ 5+♦ davet / 5-4 minör 12-14 · 2NT 5♠5♣ 7-10 · 3♣ / 3♠ NV blokaj, V iyi renk davet · 3♥ stoper sorar
    ///   1♠ üzerine: Dbl 4♥ 8+ / 5+♥ 5-10 · 1NT doğal 8-10 · 2♣ 4+♦ 5-9 ya da FG (transfer) · 2♦ 5+♥ 11+ / 6+♥ 8-10 ·
    ///               2♥ 5+♣ 10+ (transfer cuebid) · 2♠ 9+ ♥ yok · 2NT 5♥5♣ 7-10 · 3♣ / 3♥ blokaj ya da davet · 3♠ stoper sorar
    ///   1NT üzerine: 2♣ majörler · 2♦ 4+♦ rekabetçi · 2M 5+M NF · 2NT Lebensohl (6+M davet ya da 4+♦ davet) · 3x blokaj
    ///   2♣ üzerine: Dbl 8+ (majör 4+) ya da 12+ 5+M · 2♦ 4+♦ 5-9 · 2♥ / 2♠ 5+ NF · 2NT Lebensohl (4+♦ davet ya da 5M ♣ stoperli FG) · 3♣ stoper sorar
    ///   Zayıf 2♥ / 2♠ üzerine: kitaptaki Dbl / doğal / rekabetçi satırlar (1♣ tablolarına benzer), 3NT doğal 13-16
    /// Açan, transfer çağrılarını hedef rengi söyleyerek tamamlar. Basitleştirmeler 1♣ tablolarıyla aynıdır (OneClubZ_Araya):
    /// HCP verilmeyen satırlara SEÇİM eşikleri, "bomb" = 6+ kart ≤7 HCP, "transfer cuebid" satırları (zayıf 2'lere karşı) kodlanmadı.
    /// </summary>
    public class OneDiamondZ_Araya : IAcilisGelisimi
    {
        public int Oncelik { get { return 43; } }
        public string Aciklama { get { return "Zayıf NT: 1♦ açışına araya girişlerde cevaplar (kitap s.113-115)"; } }

        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }
        private const string Sira = "♣♦♥♠";
        private static int Ri(char c) { return Sira.IndexOf(c); }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private static char? Hedef(string araya, string cagri)
        {
            switch (araya + ">" + cagri)
            {
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
            if (i < 0 || g[i].Teklif != "1♦") return null;
            for (int k = 0; k < i; k++) if (g[k].Teklif != "Pas") return null;
            if (g.Count < i + 2) return null;
            var acici = g[i].Oyuncu;
            if (d.Rakipler.Contains(acici)) return null;
            var araya = g[i + 1];
            if (!d.Rakipler.Contains(araya.Oyuncu)) return null;
            string a = araya.Teklif;
            var e = new OcEl(el);
            bool zonda = ElDegerlendirici.ZondaMi(d.ZonNS, d.ZonEW, d.AktifOyuncu);

            if (g.Count == i + 4 && d.AktifOyuncu == acici && g[i + 3].Teklif == "Pas" && g[i + 2].Oyuncu == IhaleDurumu.PartnerBul(acici))
            {
                string cg = g[i + 2].Teklif;
                var t = Hedef(a, cg);
                if (!t.HasValue) return null;
                int lv = cg[0] - '0'; char cs = cg[1];
                return (Ri(t.Value) > Ri(cs) ? lv : lv + 1).ToString() + t.Value;
            }
            if (g.Count != i + 2 || d.AktifOyuncu != IhaleDurumu.PartnerBul(acici)) return null;
            switch (a)
            {
                case "1♥": return Karar(KRenk('♥', zonda), e);
                case "1♠": return Karar(KRenk('♠', zonda), e);
                case "1NT": return Karar(K1NT(), e);
                case "2♣": return Karar(K2Sinek(zonda), e);
                case "2♥": return Karar(KZayif2('♥'), e);
                case "2♠": return Karar(KZayif2('♠'), e);
            }
            return null;
        }

        /// <summary>Tabloda satır yoksa: 10+ HCP ise genel motor devralır (null), değilse Pas.</summary>
        private static string Karar(List<OcKural> kurallar, OcEl e)
        {
            // Açanın rengi ♦'da büyük uyum / oyun gücü: slam araştırması için genel motor devralır
            if ((e.D >= 6 && e.Hcp >= 10) || (e.D >= 5 && e.Hcp >= 12 && e.S < 5 && e.H < 5)) return null;
            string r = Sec(kurallar, e, null);
            if (r != null) return r;
            return e.Hcp >= 10 ? null : "Pas";
        }

        private static bool Bomba(OcEl e, char s) { return Uz(e, s) >= 6 && e.Hcp <= 7; }
        private static bool IyiDavet(OcEl e, char s) { return Uz(e, s) >= 6 && e.Hcp >= 8 && e.Hcp <= 10 && ElDegerlendirici.IyiRenk(e.El, Ad(s)); }
        private static bool IkiMinor(OcEl e) { return (e.C >= 5 && e.D >= 4) || (e.D >= 5 && e.C >= 4); }

        private static IEnumerable<OcKural> Bloklar(bool zonda, string suits)
        {
            foreach (char s in suits)
            {
                char c = s;
                yield return K("3" + c, x => zonda ? IyiDavet(x, c) : Bomba(x, c));
            }
        }

        /// <summary>1♦-(1♥) / 1♦-(1♠) (kitap s.113-114).</summary>
        private static List<OcKural> KRenk(char r, bool zonda)
        {
            var l = new List<OcKural>();
            string rAd = Ad(r);
            if (r == '♥')
            {
                l.Add(K("2♥", x => (x.S >= 5 && x.Hcp >= 11) || (x.S >= 6 && x.Hcp >= 8 && x.Hcp <= 10)));
                l.Add(K("2♦", x => x.C >= 5 && x.Hcp >= 10));
                l.Add(K("2♠", x => (x.D >= 5 && x.Hcp >= 9 && x.Hcp <= 11) || (IkiMinor(x) && x.Hcp >= 12 && x.Hcp <= 14)));
                l.Add(K("2NT", x => x.S >= 5 && x.C >= 5 && x.Hcp >= 7 && x.Hcp <= 10));
                l.Add(K("2♣", x => x.D >= 4 && ((x.Hcp >= 5 && x.Hcp <= 9) || x.Hcp >= 12)));
                l.Add(K("3NT", x => x.Hcp >= 13 && x.Dengeli && x.Stopper(rAd)));
                l.Add(K("3♥", x => x.Hcp >= 12 && x.Dengeli && !x.Stopper(rAd)));
                l.Add(K("Dbl", x => (x.S == 4 && x.Hcp >= 8) || (x.S >= 5 && x.Hcp >= 5 && x.Hcp <= 10)));
                l.Add(K("1NT", x => x.Hcp >= 8 && x.Hcp <= 10 && x.Dengeli && x.Stopper(rAd)));
                l.Add(K("1♠", x => x.Hcp >= 8 && x.S < 4));
            }
            else
            {
                l.Add(K("2♦", x => (x.H >= 5 && x.Hcp >= 11) || (x.H >= 6 && x.Hcp >= 8 && x.Hcp <= 10)));
                l.Add(K("2♥", x => x.C >= 5 && x.Hcp >= 10));
                l.Add(K("2NT", x => x.H >= 5 && x.C >= 5 && x.Hcp >= 7 && x.Hcp <= 10));
                l.Add(K("2♣", x => x.D >= 4 && ((x.Hcp >= 5 && x.Hcp <= 9) || x.Hcp >= 12)));
                l.Add(K("3NT", x => x.Hcp >= 13 && x.Dengeli && x.Stopper(rAd)));
                l.Add(K("3♠", x => x.Hcp >= 12 && x.Dengeli && !x.Stopper(rAd)));
                l.Add(K("Dbl", x => (x.H == 4 && x.Hcp >= 8) || (x.H >= 5 && x.Hcp >= 5 && x.Hcp <= 10)));
                l.Add(K("1NT", x => x.Hcp >= 8 && x.Hcp <= 10 && x.Dengeli && x.Stopper(rAd)));
                l.Add(K("2♠", x => x.Hcp >= 9 && x.H < 4));
            }
            string blok = r == '♥' ? "♣♠" : "♣♥";
            l.AddRange(Bloklar(zonda, blok));
            return l;
        }

        /// <summary>1♦-(1NT) (kitap s.114).</summary>
        private static List<OcKural> K1NT()
        {
            return new List<OcKural> {
                K("2NT", x => ((x.S >= 6 || x.H >= 6) && x.Hcp >= 8 && x.Hcp <= 10) || (x.D >= 4 && x.Hcp >= 9 && x.Hcp <= 11)),
                K("2♣", x => (x.S >= 5 && x.H >= 4 || x.H >= 5 && x.S >= 4) && x.Hcp <= 10),
                K("2♠", x => x.S >= 5 && x.Hcp <= 10),
                K("2♥", x => x.H >= 5 && x.Hcp <= 10),
                K("2♦", x => x.D >= 4 && x.Hcp <= 8),
                K("3♠", x => x.S >= 7 && x.Hcp <= 7),
                K("3♥", x => x.H >= 7 && x.Hcp <= 7),
                K("3♣", x => x.C >= 7 && x.Hcp <= 7),
            };
        }

        /// <summary>1♦-(2♣) (kitap s.114).</summary>
        private static List<OcKural> K2Sinek(bool zonda)
        {
            var l = new List<OcKural> {
                K("3NT", x => x.Hcp >= 13 && x.Dengeli && x.Stopper("Sinek")),
                K("3♣", x => x.Hcp >= 13 && x.Dengeli && !x.Stopper("Sinek")),
                K("2NT", x => (x.D >= 4 && x.Hcp >= 9 && x.Hcp <= 11) || ((x.S >= 5 || x.H >= 5) && x.Hcp >= 12 && x.Stopper("Sinek"))),
                K("Dbl", x => x.Hcp >= 12 && (x.S >= 5 || x.H >= 5)),
                K("2♠", x => x.S >= 5 && x.Hcp >= 8),
                K("2♥", x => x.H >= 5 && x.Hcp >= 8),
                K("2♦", x => x.D >= 4 && x.Hcp >= 5 && x.Hcp <= 9),
                K("Dbl", x => x.Hcp >= 8 && (x.S >= 4 || x.H >= 4)),
            };
            l.AddRange(Bloklar(zonda, "♥♠"));
            return l;
        }

        /// <summary>1♦-(zayıf 2♥ / 2♠) (kitap s.115).</summary>
        private static List<OcKural> KZayif2(char r)
        {
            var l = new List<OcKural>();
            if (r == '♥')
            {
                l.Add(K("3NT", x => x.Hcp >= 13 && x.Hcp <= 16 && x.Dengeli && x.Stopper("Kupa")));
                l.Add(K("3♠", x => x.S >= 5 && x.Hcp >= 12 && x.Stopper("Sinek")));
                l.Add(K("3♥", x => x.S >= 6 && x.Hcp >= 10));
                l.Add(K("Dbl", x => (x.S == 4 && x.Hcp >= 8) || (x.S < 4 && x.Hcp >= 10) || (x.S >= 5 && x.Hcp >= 12)));
                l.Add(K("2♠", x => x.S >= 5 && x.Hcp >= 8));
                l.Add(K("2NT", x => x.C >= 5 && x.Hcp >= 7));
                l.Add(K("3♣", x => x.D >= 4 && x.Hcp >= 7));
            }
            else
            {
                l.Add(K("3NT", x => x.Hcp >= 13 && x.Hcp <= 16 && x.Dengeli && x.Stopper("Maça")));
                l.Add(K("3♥", x => x.H >= 5 && x.Hcp >= 10 && x.Hcp <= 12));
                l.Add(K("3♦", x => x.H >= 5 && ((x.Hcp >= 7 && x.Hcp <= 9) || x.Hcp >= 13)));
                l.Add(K("Dbl", x => (x.Hcp >= 9 && x.H == 4) || (x.Hcp >= 12 && x.H >= 5)));
                l.Add(K("2NT", x => x.C >= 5 && x.Hcp >= 7));
                l.Add(K("3♣", x => x.D >= 4 && x.Hcp >= 7));
            }
            return l;
        }
    }
}
