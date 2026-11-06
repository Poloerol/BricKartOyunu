using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneSpade
{
    /// <summary>
    /// 1♠ AÇIŞIMIZA ARAYA GİRERLERSE CEVAPÇININ İLK HAMLESİ (SAS kitabı s.118-119), zayıf NT versiyonu (StrongNT=false).
    ///   Dbl: RDbl 10+ (♠ desteği yoksa ya da 11+) · 1NT / 2♣ / 2♦ = 5+♣ / 5+♦ / 5+♥ transfer · 2♥ 3+♠ yapıcı 7-10 · 2♠ 3+♠ tutuş 4-6 ·
    ///        2NT 4+♠ davet · 3♣/3♦/3♥ 5+m 4+♠ davet+ · 3♠ blokaj · 3NT 4+♠ FG · 4♣/4♦/4♥ 4+♠ tek kart
    ///   1NT: Dbl ceza · 2x doğal rekabetçi · 2NT Lebensohl (5-5 minör zayıf / 4+♠ davet / 6+♥ davet) · 3x blokaj
    ///   2♣: 2♦ 5+♦ 10+ ya da 6+♦ 8-10 · 2♥ 5+♥ 11+ ya da 6+♥ 8-10 · 2♠ tutuş · 2NT davet · 3♣ cuebid FG · 3♦/3♥ 5+m 4+♠ davet+ · 4♣/4♦ tek kart
    ///   2♦: Dbl (4♥ 8+, ♥ yok 10+) · 2♥ 5+♥ 11+ ya da 6+♥ 8-10 · 2♠ tutuş · 2NT davet · 3♦ cuebid FG · 3♣ 5+♣ FG · 3♥ 5+♥ 4+♠ davet+
    ///   2♥: Dbl 10+ · 2NT davet · 3♣/3♦ 5+ FG · 3♥ cuebid FG · 3♠ blokaj · tek kart sıçramaları
    ///   2♠ (Michaels): Dbl ceza teklifi · 2NT davet · 3♣/3♦ 5+ FG · 3♥ cuebid FG · 3♠ rekabetçi · tek kart sıçramaları
    /// Açan transfer çağrılarını tamamlar (1NT → 2♣, 2♣ → 2♦, 2♦ → 2♥). Basitleştirmeler 1♥ tablolarıyla aynıdır (OneHeartZ_Araya):
    /// HCP verilmeyen satırlara eşik seçildi (2♠ 4-6 / 2♥ 7-10), "bomb" = ≤7 HCP uzun kart ya da tek kartlı 5+♠ (4♠ için 7+♠),
    /// 4♥ "oynamak için" satırları 7+♥ ≤9 HCP olarak kodlandı, tek kart sıçramaları 10-14 HCP; 2♣ / 2♦ üzerine 10+ HCP'de satırı olmayan eller genel motorda.
    /// </summary>
    public class OneSpadeZ_Araya : IAcilisGelisimi
    {
        public int Oncelik { get { return 43; } }
        public string Aciklama { get { return "Zayıf NT: 1♠ açışına araya girişlerde cevaplar (kitap s.118-119)"; } }

        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13 || d.Anlasma == null || d.Anlasma.StrongNT) return null;
            var g = d.Gecmis;
            int i = g.FindIndex(h => h.GercekTeklifMi);
            if (i < 0 || g[i].Teklif != "1♠") return null;
            for (int k = 0; k < i; k++) if (g[k].Teklif != "Pas") return null;
            if (g.Count < i + 2) return null;
            var acici = g[i].Oyuncu;
            if (d.Rakipler.Contains(acici)) return null;
            var araya = g[i + 1];
            if (!d.Rakipler.Contains(araya.Oyuncu)) return null;
            string a = araya.Teklif;
            var e = new OcEl(el);

            if (g.Count == i + 4 && d.AktifOyuncu == acici && g[i + 3].Teklif == "Pas" && g[i + 2].Oyuncu == IhaleDurumu.PartnerBul(acici) && a == "Dbl")
            {
                string cg = g[i + 2].Teklif;
                if (cg == "1NT") return "2♣";
                if (cg == "2♣") return "2♦";
                if (cg == "2♦") return "2♥";
                return null;
            }
            if (g.Count != i + 2 || d.AktifOyuncu != IhaleDurumu.PartnerBul(acici)) return null;
            switch (a)
            {
                case "Dbl": return Karar(KDbl(), e);
                case "1NT": return Karar(K1NT(), e);
                case "2♣": return Karar(KIki('♣'), e);
                case "2♦": return Karar(KIki('♦'), e);
                case "2♥": return Karar(KKupa(), e);
                case "2♠": return Karar(KMichaels(), e);
            }
            return null;
        }

        private static string Karar(List<OcKural> kurallar, OcEl e)
        {
            if (e.S >= 6 && e.Hcp >= 10) return null;      // büyük ♠ uyumu: slam araştırması genel motorda
            string r = Sec(kurallar, e, null);
            if (r != null) return r;
            return e.Hcp >= 10 ? null : "Pas";
        }

        private static bool Tek(OcEl e, char s) { return Uz(e, s) <= 1; }
        private static bool Bomba(OcEl e) { return e.S >= 5 && e.Hcp <= 7 && (e.H <= 1 || e.D <= 1 || e.C <= 1 || e.S >= 6); }

        private static void TekKart(List<OcKural> l, string suits)
        {
            foreach (char s in suits)
            {
                char c = s;
                l.Add(K("4" + c, x => x.S >= 4 && Tek(x, c) && x.Hcp >= 10 && x.Hcp <= 14));
            }
        }

        /// <summary>1♠-(Dbl) (kitap s.117).</summary>
        private static List<OcKural> KDbl()
        {
            var l = new List<OcKural>();
            l.Add(K("4♠", x => x.S >= 7 && x.Hcp <= 9));
            TekKart(l, "♥♣♦");
            l.Add(K("3NT", x => x.S >= 4 && x.Hcp >= 13));
            l.Add(K("3♣", x => x.C >= 5 && x.S >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            l.Add(K("3♦", x => x.D >= 5 && x.S >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            l.Add(K("3♥", x => x.H >= 5 && x.S >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            l.Add(K("2NT", x => x.S >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            l.Add(K("3♠", x => Bomba(x)));
            l.Add(K("RDbl", x => x.Hcp >= 10 && (x.S <= 2 || x.Hcp >= 11)));
            l.Add(K("2♥", x => x.S >= 3 && x.Hcp >= 7 && x.Hcp <= 10));
            l.Add(K("2♠", x => x.S >= 3 && x.Hcp >= 4 && x.Hcp <= 6));
            l.Add(K("1NT", x => x.C >= 5 && x.Hcp >= 6 && x.Hcp <= 9));
            l.Add(K("2♣", x => x.D >= 5 && x.Hcp >= 6 && x.Hcp <= 9));
            l.Add(K("2♦", x => x.H >= 5 && x.Hcp >= 6 && x.Hcp <= 9));
            return l;
        }

        /// <summary>1♠-(1NT) (kitap s.118).</summary>
        private static List<OcKural> K1NT()
        {
            return new List<OcKural> {
                K("3♥", x => x.H >= 7 && x.Hcp <= 7),
                K("3♦", x => x.D >= 7 && x.Hcp <= 7),
                K("3♣", x => x.C >= 7 && x.Hcp <= 7),
                K("2NT", x => (x.C >= 5 && x.D >= 5 && x.Hcp <= 7) || (x.S >= 4 && x.Hcp >= 8 && x.Hcp <= 10) || (x.H >= 6 && x.Hcp >= 8 && x.Hcp <= 10)),
                K("2♥", x => x.H >= 5 && x.Hcp <= 9),
                K("2♦", x => x.D >= 5 && x.Hcp <= 9),
                K("2♣", x => x.C >= 5 && x.Hcp <= 9),
                K("2♠", x => x.S >= 3 && x.Hcp <= 7),
                K("Dbl", x => x.Hcp >= 10 && x.S < 4),
            };
        }

        /// <summary>1♠-(2♣) ve 1♠-(2♦) (kitap s.118-119).</summary>
        private static List<OcKural> KIki(char r)
        {
            var l = new List<OcKural>();
            string rAd = Ad(r);
            l.Add(K("4♠", x => x.S >= 7 && x.Hcp <= 9));
            l.Add(K("4♥", x => x.H >= 7 && x.Hcp <= 9));
            TekKart(l, r == '♣' ? "♦♣" : "♣♦");
            l.Add(K("3NT", x => x.Hcp >= 13 && x.Dengeli && x.S < 3 && x.Stopper(rAd)));
            l.Add(K("3" + r, x => x.S >= 3 && x.Hcp >= 12));
            if (r == '♣') l.Add(K("3♦", x => x.D >= 5 && x.S >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            else l.Add(K("3♣", x => x.C >= 5 && x.Hcp >= 12));
            l.Add(K("3♥", x => x.H >= 5 && x.S >= 4 && x.Hcp >= 10 && x.Hcp <= 12));
            l.Add(K("2NT", x => x.S >= 3 && x.Hcp >= 10 && x.Hcp <= 11));
            l.Add(K("3♠", x => Bomba(x)));
            l.Add(K("2♠", x => x.S >= 3 && x.Hcp >= 5 && x.Hcp <= 9));
            if (r == '♣') l.Add(K("2♦", x => (x.D >= 5 && x.Hcp >= 10) || (x.D >= 6 && x.Hcp >= 8 && x.Hcp <= 10)));
            l.Add(K("2♥", x => (x.H >= 5 && x.Hcp >= 11) || (x.H >= 6 && x.Hcp >= 8 && x.Hcp <= 10)));
            if (r == '♦') l.Add(K("Dbl", x => (x.H == 4 && x.Hcp >= 8) || (x.H < 4 && x.Hcp >= 10)));
            return l;
        }

        /// <summary>1♠-(2♥) (kitap s.119).</summary>
        private static List<OcKural> KKupa()
        {
            var l = new List<OcKural>();
            l.Add(K("4♠", x => x.S >= 7 && x.Hcp <= 9));
            TekKart(l, "♥♣♦");
            l.Add(K("3NT", x => x.Hcp >= 13 && x.Dengeli && x.S < 3 && x.Stopper("Kupa")));
            l.Add(K("3♥", x => x.S >= 3 && x.Hcp >= 12));
            l.Add(K("3♣", x => x.C >= 5 && x.Hcp >= 12));
            l.Add(K("3♦", x => x.D >= 5 && x.Hcp >= 12));
            l.Add(K("2NT", x => x.S >= 3 && x.Hcp >= 10 && x.Hcp <= 11));
            l.Add(K("3♠", x => Bomba(x)));
            l.Add(K("Dbl", x => x.Hcp >= 10 && x.S <= 2));
            return l;
        }

        /// <summary>1♠-(2♠ Michaels) (kitap s.119).</summary>
        private static List<OcKural> KMichaels()
        {
            var l = new List<OcKural>();
            l.Add(K("4♠", x => x.S >= 7 && x.Hcp <= 9));
            TekKart(l, "♥♣♦");
            l.Add(K("3NT", x => x.Hcp >= 13 && x.Dengeli && x.S < 3));
            l.Add(K("3♥", x => x.S >= 3 && x.Hcp >= 12));
            l.Add(K("3♣", x => x.C >= 5 && x.Hcp >= 12));
            l.Add(K("3♦", x => x.D >= 5 && x.Hcp >= 12));
            l.Add(K("2NT", x => x.S >= 3 && x.Hcp >= 10 && x.Hcp <= 11));
            l.Add(K("3♠", x => x.S >= 3 && x.Hcp >= 5 && x.Hcp <= 9));
            l.Add(K("Dbl", x => x.Hcp >= 10 && x.S <= 2));
            return l;
        }
    }
}
