using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    /// <summary>
    /// ZAYIF NT VERSİYONU: 12-14 NT AÇIŞIMIZA ARAYA GİRİLİRSE (SAS kitabı s.90): 1NT - (DBL) - ?
    ///   Pas        = zayıf (0-9), 5'li renk yok; ortağın 5'li rengini, RDBL veya Pas demesini ister
    ///   RDBL       = 10+ HCP
    ///   2♣/♦/♥/♠   = doğal, 5-6 kartlı renk, 0-9 HCP          3♣/♦/♥/♠ = doğal blokaj (7+ kart, 0-9 HCP)
    ///   2NT        = iki renkli (5-5+) oyun zorlaması (13+)
    /// Açıcının cevabı (Pas - Pas'tan sonra): 5+ kartlı en uzun renk (2x) / renk yoksa maksimumda (14) Pas, değilse RDBL.
    /// RDBL'den sonra cevapçı en uzun rengini (2 seviyesi) söyler. 2x / 3x'e açan Pas der.
    /// 2NT'ye açan 3♣ (röle) der; cevapçı iki rengin yükseğini söyler; açan majörde 3+ ile 4M, yoksa 3NT.
    /// Doğal müdahalelerde (2x) Rubensohl/Lebensohl mevcut konvansiyon bayraklarına bırakıldı.
    /// RDBL sonrası rakip kaçarsa (1NT-(Dbl)-RDbl-(2x)) açan INVERTED DBL kullanır (kitap s.6): Dbl = x'te 2-3 kart, Pas = 4+ kart (cevapçı kontra eder),
    /// x tek kartsa yeni renk. 2NT röle sonrası 5-5 majörlerde (3♠ → açan 3NT) cevapçı ikinci rengi 4♥ ile gösterir.
    /// Basitleştirmeler: RDBL sonrası açanın 10+ el devamları, 2NT'den sonra minör alt renk gösterimi ve slam araması yok;
    /// yalnızca ardışık Pas'larla süren dizilerde devreye girer (rakip ikinci kez konuşursa genel motor).
    /// </summary>
    public class OneNTZ_Araya : IAcilisGelisimi
    {
        public int Oncelik { get { return 43; } }
        public string Aciklama { get { return "Zayıf NT: 1NT-(Dbl) sonrası cevaplar (kitap s.90)"; } }

        private const string Sira = "♣♦♥♠";
        private static int Ri(char c) { return Sira.IndexOf(c); }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13 || d.Anlasma == null || d.Anlasma.StrongNT) return null;
            var g = d.Gecmis;
            int i = g.FindIndex(h => h.GercekTeklifMi);
            if (i < 0 || g[i].Teklif != "1NT") return null;
            for (int k = 0; k < i; k++) if (g[k].Teklif != "Pas") return null;
            if (g.Count < i + 2 || g[i + 1].Teklif != "Dbl") return null;
            var acici = g[i].Oyuncu;
            int n = g.Count - (i + 2);                  // kontradan sonraki teklif sayısı
            var e = new OcEl(el);
            bool aciciBen = d.AktifOyuncu == acici;
            bool ortak = d.Partner == acici;
            if (!aciciBen && !ortak) return null;

            if (ortak && n == 0) return Cevapci(e);
            // RDBL sonrası rakip kaçtı (1NT-(Dbl)-RDbl-(2x)): açanın INVERTED DBL'ı, sonra cevapçının devamı (kitap s.6)
            if (aciciBen && n == 2 && g[i + 2].Teklif == "RDbl" && Renkli(g[i + 3].Teklif) && d.Rakipler.Contains(g[i + 3].Oyuncu))
                return InvertedAcici(e, g[i + 3].Teklif);
            if (ortak && n == 4 && g[i + 2].Teklif == "RDbl" && Renkli(g[i + 3].Teklif) && d.Rakipler.Contains(g[i + 3].Oyuncu)
                && g[i + 5].Teklif == "Pas")
                return InvertedCevapci(e, g[i + 3].Teklif, g[i + 4].Teklif);
            if (aciciBen && n == 2 && g[i + 3].Teklif == "Pas") return Acici(e, g[i + 2].Teklif, d);
            if (ortak && n == 4 && g[i + 2].Teklif == "Pas" && g[i + 3].Teklif == "Pas" && g[i + 4].Teklif == "RDbl" && g[i + 5].Teklif == "Pas")
                return RdblSonrasi(e);
            if (ortak && n == 4 && g[i + 2].Teklif == "2NT" && g[i + 3].Teklif == "Pas" && g[i + 4].Teklif == "3♣" && g[i + 5].Teklif == "Pas")
                return IkiRenkYuksek(e);
            if (aciciBen && n == 6 && g[i + 2].Teklif == "2NT" && g[i + 3].Teklif == "Pas" && g[i + 4].Teklif == "3♣" && g[i + 5].Teklif == "Pas" && g[i + 7].Teklif == "Pas")
                return IkiRenkAciciSon(e, g[i + 6].Teklif);
            // 2NT röle: açan 3NT dedi (majörde uyum yok), cevapçı 5-5 majörlerde ikinci rengi (♥) 4 seviyesinde söyler
            if (ortak && n == 8 && g[i + 2].Teklif == "2NT" && g[i + 3].Teklif == "Pas" && g[i + 4].Teklif == "3♣" && g[i + 5].Teklif == "Pas"
                && g[i + 6].Teklif == "3♠" && g[i + 7].Teklif == "Pas" && g[i + 8].Teklif == "3NT" && g[i + 9].Teklif == "Pas")
                return e.H >= 5 && e.S >= 5 ? "4♥" : "Pas";
            return null;
        }

        private static bool Renkli(string t) { return t != null && t.Length == 2 && char.IsDigit(t[0]) && Sira.IndexOf(t[1]) >= 0; }

        /// <summary>1NT-(Dbl)-RDbl-(2x): açan. Kitap s.6 INVERTED DBL: Dbl = x'te 2-3 kart · Pas = 4+ kart (ceza, ortak kontra eder) ·
        /// teklif = x tek kart, yeni renk (4+ kart, en ucuz seviyede).</summary>
        private static string InvertedAcici(OcEl e, string araya)
        {
            char x = araya[1]; int lv = araya[0] - '0';
            int uz = Uz(e, x);
            if (uz >= 4) return "Pas";
            if (uz >= 2) return "Dbl";
            var yeni = Sira.Where(c => c != x && Uz(e, c) >= 4).OrderByDescending(c => Uz(e, c)).ThenByDescending(c => Ri(c)).ToList();
            if (yeni.Count == 0) return null;
            char s = yeni[0];
            return (Ri(s) > Ri(x) ? lv : lv + 1).ToString() + s;
        }

        /// <summary>Açan Pas dedi (4+ x): cevapçı ceza kontrası yapar. Açan Dbl dedi (2-3 x): x'te 4+ kart varsa Pas (ceza),
        /// yoksa x'te stoperle 3NT. Açan yeni renk dediyse genel motor.</summary>
        private static string InvertedCevapci(OcEl e, string araya, string aciciCagri)
        {
            char x = araya[1];
            if (aciciCagri == "Pas") return "Dbl";
            if (aciciCagri == "Dbl")
            {
                if (Uz(e, x) >= 4) return "Pas";
                return e.Stopper(x == '♠' ? "Maça" : x == '♥' ? "Kupa" : x == '♦' ? "Karo" : "Sinek") ? "3NT" : null;
            }
            return null;
        }

        /// <summary>1NT-(Dbl)-? cevapçı.</summary>
        private static string Cevapci(OcEl e)
        {
            int hp = e.Hcp;
            var renkler = Sira.OrderByDescending(c => Uz(e, c)).ThenByDescending(c => Ri(c)).ToList();
            char en = renkler[0]; int uz = Uz(e, en);
            if (hp >= 13)
            {
                var bes = Sira.Where(c => Uz(e, c) >= 5).ToList();
                if (bes.Count >= 2) return "2NT";
            }
            if (hp >= 10) return "RDbl";
            if (uz >= 7) return "3" + en;
            if (uz >= 5) return "2" + en;
            return "Pas";
        }

        /// <summary>1NT-(Dbl)-Pas-(Pas)-? açan; cevapçının hamlesi sonrası.</summary>
        private static string Acici(OcEl e, string cevap, IhaleDurumu d)
        {
            if (cevap == "Pas")
            {
                var renkler = Sira.OrderByDescending(c => Uz(e, c)).ThenByDescending(c => Ri(c)).ToList();
                char en = renkler[0];
                if (Uz(e, en) >= 5) return "2" + en;
                return e.Hcp >= NTA.Hi1(d.Anlasma) ? "Pas" : "RDbl";
            }
            if (cevap == "2NT") return "3♣";
            return "Pas";                                // doğal 2x/3x, RDBL: Pas
        }

        /// <summary>Açanın RDBL'si sonrası cevapçı (dengeli, zayıf): en uzun renk.</summary>
        private static string RdblSonrasi(OcEl e)
        {
            char en = Sira.OrderByDescending(c => Uz(e, c)).ThenByDescending(c => Ri(c)).First();
            return "2" + en;
        }

        /// <summary>2NT'ye açanın 3♣ rölesinden sonra cevapçı: iki rengin yükseğini söyler.</summary>
        private static string IkiRenkYuksek(OcEl e)
        {
            char yuksek = Sira.Where(c => Uz(e, c) >= 5).OrderByDescending(c => Ri(c)).First();
            if (yuksek == '♣') return "Pas";
            return "3" + yuksek;
        }

        private static string IkiRenkAciciSon(OcEl e, string yuksek)
        {
            if (yuksek.Length == 2 && (yuksek[1] == '♥' || yuksek[1] == '♠'))
            {
                if (Uz(e, yuksek[1]) >= 3) return "4" + yuksek[1];
            }
            return "3NT";
        }
    }
}
