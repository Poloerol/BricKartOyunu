using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneHeart
{
    /// <summary>
    /// 1♥ AÇIŞIMIZA ARAYA GİRERLERSE CEVAPÇININ İLK HAMLESİ (SAS kitabı s.115-117), zayıf NT versiyonu (StrongNT=false).
    ///   Dbl: RDbl 10+ (♥ desteği yoksa ya da 11+) · 1♠ 6+ HCP 4+♠ · 1NT 5+♣ (transfer) · 2♣ 5+♦ (transfer) · 2♦ 3+♥ yapıcı 7-10 ·
    ///        2♥ 3+♥ tutuş 4-6 · 2♠ 5+♠ 4+♥ davet+ · 2NT 4+♥ davet · 3♣/3♦ 5+m 4+♥ davet+ · 3♥ blokaj · 3♠/4♣/4♦ 4+♥ tek kart · 3NT 4+♥ FG
    ///   1♠: Dbl 8+ (♥ desteği yok) · 1NT / 2♣ 5+♣ / 5+♦ transfer · 2♦ 3+♥ yapıcı · 2♥ 3+♥ tutuş · 2♠ cuebid 3+♥ FG · 2NT 3+♥ davet ·
    ///       3♣/3♦ 5+m 4+♥ davet+ · 4♣/4♦ tek kart · 3NT doğal
    ///   1NT: Dbl ceza · 2x doğal rekabetçi · 2NT Lebensohl (5-5 minör zayıf / 4+♥ davet / 6+♠ davet) · 3x blokaj
    ///   2♣ / 2♦: Dbl (4♠ 8+, 5+♠ 12+ ya da ♠ yok 10+) · 2♥ tutuş · 2♠ 5+♠ NF · 2NT 3+♥ davet · cuebid 3+♥ FG · 3♣/3♦ 5+ renk FG · tek kart sıçramaları
    ///   2♥ (Michaels): Dbl ceza teklifi · 2♠ cuebid FG · 2NT davet · 3♣/3♦ FG · 3♥ rekabetçi
    ///   2♠ (zayıf): Dbl 10+ · 2NT davet · 3♣/3♦ FG · 3♥ rekabetçi · 3♠ cuebid FG
    /// Açan transfer çağrılarını tamamlar (1NT → 2♣, 2♣ → 2♦). Basitleştirmeler: HCP verilmeyen satırlara eşik seçildi (2♥ 4-6 / 2♦ 7-10 çakışması 7'de 2♦),
    /// "bomb" = ≤7 HCP uzun kart ya da tek kartlı 5+♥ (4♥ için 7+♥), tek kart sıçramaları 10-14 HCP; Dbl sonrası "2♠ 5+♠ 4+♥" satırı
    /// kitapta basıldığı gibi okundu; hedefte karşılığı bulunmayan eller 10+ HCP ise genel motora bırakılır.
    /// </summary>
    public class OneHeartZ_Araya : IAcilisGelisimi
    {
        public int Oncelik { get { return 43; } }
        public string Aciklama { get { return "Zayıf NT: 1♥ açışına araya girişlerde cevaplar (kitap s.115-117)"; } }

        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13 || d.Anlasma == null || d.Anlasma.StrongNT) return null;
            var g = d.Gecmis;
            int i = g.FindIndex(h => h.GercekTeklifMi);
            if (i < 0 || g[i].Teklif != "1♥") return null;
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
            if (g.Count == i + 4 && d.AktifOyuncu == acici && g[i + 3].Teklif == "Pas" && g[i + 2].Oyuncu == IhaleDurumu.PartnerBul(acici)
                && (a == "Dbl" || a == "1♠"))
            {
                string cg = g[i + 2].Teklif;
                if (cg == "1NT") return "2♣";
                if (cg == "2♣") return "2♦";
                return null;
            }
            if (g.Count != i + 2 || d.AktifOyuncu != IhaleDurumu.PartnerBul(acici)) return null;
            switch (a)
            {
                case "Dbl": return Karar(KDbl(), e);
                case "1♠": return Karar(KSpade(), e);
                case "1NT": return Karar(K1NT(), e);
                case "2♣": return Karar(KIki('♣', zonda), e);
                case "2♦": return Karar(KIki('♦', zonda), e);
                case "2♥": return Karar(KMichaels(), e);
                case "2♠": return Karar(KZayif2(), e);
            }
            return null;
        }

        private static string Karar(List<OcKural> kurallar, OcEl e)
        {
            if (e.H >= 6 && e.Hcp >= 10) return null;      // büyük ♥ uyumu: slam araştırması genel motorda
            string r = Sec(kurallar, e, null);
            if (r != null) return r;
            return e.Hcp >= 10 ? null : "Pas";
        }

        private static bool Tek(OcEl e, char s) { return Uz(e, s) <= 1; }
        private static bool Bomba(OcEl e) { return e.H >= 5 && e.Hcp <= 7 && (e.S <= 1 || e.D <= 1 || e.C <= 1 || e.H >= 6); }

        /// <summary>Tek kart sıçramaları (4+♥, 10-14 HCP): verilen renkler için.</summary>
        private static void TekKart(List<OcKural> l, string suits, char opp1, char opp2)
        {
            foreach (char s in suits)
            {
                char c = s;
                string cagri = (c == '♠' ? "3" : "4") + c;
                l.Add(K(cagri, x => x.H >= 4 && Tek(x, c) && x.Hcp >= 10 && x.Hcp <= 14));
            }
        }

        /// <summary>1♥-(Dbl) (kitap s.115).</summary>
        private static List<OcKural> KDbl()
        {
            var l = new List<OcKural>();
            l.Add(K("4♥", x => x.H >= 7 && x.Hcp <= 9));
            TekKart(l, "♠♣♦", ' ', ' ');
            l.Add(K("3NT", x => x.H >= 4 && x.Hcp >= 13));
            l.Add(K("3♣", x => x.C >= 5 && x.H >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            l.Add(K("3♦", x => x.D >= 5 && x.H >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            l.Add(K("2♠", x => x.S >= 5 && x.H >= 4 && x.Hcp >= 10));
            l.Add(K("2NT", x => x.H >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            l.Add(K("3♥", x => Bomba(x)));
            l.Add(K("RDbl", x => x.Hcp >= 10 && (x.H <= 2 || x.Hcp >= 11)));
            l.Add(K("2♦", x => x.H >= 3 && x.Hcp >= 7 && x.Hcp <= 10));
            l.Add(K("2♥", x => x.H >= 3 && x.Hcp >= 4 && x.Hcp <= 6));
            l.Add(K("1♠", x => x.S >= 4 && x.Hcp >= 6));
            l.Add(K("1NT", x => x.C >= 5 && x.Hcp >= 6 && x.Hcp <= 9));
            l.Add(K("2♣", x => x.D >= 5 && x.Hcp >= 6 && x.Hcp <= 9));
            return l;
        }

        /// <summary>1♥-(1♠) (kitap s.115).</summary>
        private static List<OcKural> KSpade()
        {
            var l = new List<OcKural>();
            l.Add(K("4♥", x => x.H >= 7 && x.Hcp <= 9));
            TekKart(l, "♣♦", ' ', ' ');
            l.Add(K("2♠", x => x.H >= 3 && x.Hcp >= 12));
            l.Add(K("3♣", x => x.C >= 5 && x.H >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            l.Add(K("3♦", x => x.D >= 5 && x.H >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            l.Add(K("2NT", x => x.H >= 3 && x.Hcp >= 10 && x.Hcp <= 11));
            l.Add(K("3♥", x => Bomba(x)));
            l.Add(K("2♦", x => x.H >= 3 && x.Hcp >= 7 && x.Hcp <= 10));
            l.Add(K("2♥", x => x.H >= 3 && x.Hcp >= 4 && x.Hcp <= 6));
            l.Add(K("3NT", x => x.Hcp >= 13 && x.Dengeli && x.Stopper("Maça")));
            l.Add(K("Dbl", x => x.Hcp >= 8 && x.H <= 2));
            l.Add(K("1NT", x => x.C >= 5 && x.Hcp >= 6 && x.Hcp <= 9));
            l.Add(K("2♣", x => x.D >= 5 && x.Hcp >= 6 && x.Hcp <= 9));
            return l;
        }

        /// <summary>1♥-(1NT) (kitap s.116).</summary>
        private static List<OcKural> K1NT()
        {
            return new List<OcKural> {
                K("3♠", x => x.S >= 7 && x.Hcp <= 7),
                K("3♦", x => x.D >= 7 && x.Hcp <= 7),
                K("3♣", x => x.C >= 7 && x.Hcp <= 7),
                K("2NT", x => (x.C >= 5 && x.D >= 5 && x.Hcp <= 7) || (x.H >= 4 && x.Hcp >= 8 && x.Hcp <= 10) || (x.S >= 6 && x.Hcp >= 8 && x.Hcp <= 10)),
                K("2♠", x => x.S >= 5 && x.Hcp <= 9),
                K("2♦", x => x.D >= 5 && x.Hcp <= 9),
                K("2♣", x => x.C >= 5 && x.Hcp <= 9),
                K("2♥", x => x.H >= 3 && x.Hcp <= 7),
                K("Dbl", x => x.Hcp >= 10 && x.H < 4),
            };
        }

        /// <summary>1♥-(2♣) ve 1♥-(2♦) (kitap s.116).</summary>
        private static List<OcKural> KIki(char r, bool zonda)
        {
            var l = new List<OcKural>();
            string rAd = Ad(r);
            l.Add(K("4♠", x => x.S >= 7 && x.Hcp <= 8));
            l.Add(K("4♥", x => x.H >= 7 && x.Hcp <= 9));
            TekKart(l, r == '♣' ? "♠♦♣" : "♠♣♦", ' ', ' ');
            l.Add(K("3NT", x => x.Hcp >= 13 && x.Dengeli && x.H < 3 && x.Stopper(rAd)));
            l.Add(K("3" + r, x => x.H >= 3 && x.Hcp >= 12));
            if (r == '♣') l.Add(K("3♦", x => x.D >= 5 && x.H >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            else l.Add(K("3♣", x => x.C >= 5 && x.Hcp >= 12));
            l.Add(K("2NT", x => x.H >= 3 && x.Hcp >= 10 && x.Hcp <= 11));
            l.Add(K("3♥", x => Bomba(x)));
            l.Add(K("2♥", x => x.H >= 3 && x.Hcp >= 5 && x.Hcp <= 9));
            if (r == '♣') l.Add(K("2♦", x => x.D >= 5 && x.Hcp >= 10));
            l.Add(K("2♠", x => x.S >= 5 && x.Hcp >= 8 && x.Hcp <= 11));
            l.Add(K("Dbl", x => (x.S == 4 && x.Hcp >= 8) || (x.S >= 5 && x.Hcp >= 12) || (x.S < 4 && x.Hcp >= 10)));
            return l;
        }

        /// <summary>1♥-(2♥ Michaels) (kitap s.117).</summary>
        private static List<OcKural> KMichaels()
        {
            var l = new List<OcKural>();
            l.Add(K("4♥", x => x.H >= 7 && x.Hcp <= 9));
            TekKart(l, "♠♣♦", ' ', ' ');
            l.Add(K("3NT", x => x.Hcp >= 13 && x.Dengeli && x.H < 3));
            l.Add(K("2♠", x => x.H >= 3 && x.Hcp >= 12));
            l.Add(K("3♣", x => x.C >= 5 && x.Hcp >= 12));
            l.Add(K("3♦", x => x.D >= 5 && x.Hcp >= 12));
            l.Add(K("2NT", x => x.H >= 3 && x.Hcp >= 10 && x.Hcp <= 11));
            l.Add(K("3♥", x => x.H >= 3 && x.Hcp >= 5 && x.Hcp <= 9));
            l.Add(K("Dbl", x => x.Hcp >= 10 && x.H <= 2));
            return l;
        }

        /// <summary>1♥-(2♠ zayıf) (kitap s.117).</summary>
        private static List<OcKural> KZayif2()
        {
            var l = new List<OcKural>();
            l.Add(K("4♥", x => x.H >= 7 && x.Hcp <= 9));
            TekKart(l, "♣♦", ' ', ' ');
            l.Add(K("3NT", x => x.Hcp >= 13 && x.Dengeli && x.H < 3 && x.Stopper("Maça")));
            l.Add(K("3♠", x => x.H >= 3 && x.Hcp >= 12));
            l.Add(K("3♣", x => x.C >= 5 && x.Hcp >= 12));
            l.Add(K("3♦", x => x.D >= 5 && x.Hcp >= 12));
            l.Add(K("2NT", x => x.H >= 3 && x.Hcp >= 10 && x.Hcp <= 11));
            l.Add(K("3♥", x => x.H >= 3 && x.Hcp >= 5 && x.Hcp <= 9));
            l.Add(K("Dbl", x => x.Hcp >= 10 && x.H <= 2));
            return l;
        }
    }
}
