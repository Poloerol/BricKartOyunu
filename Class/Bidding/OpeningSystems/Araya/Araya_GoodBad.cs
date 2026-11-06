using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Araya
{
    /// <summary>
    /// 2NT GOODBAD GELİŞMELERİ (SAS kitabı s.95-96): 1♦ açtıktan sonra rakip araya girince açanın 2. hamlesi.
    ///   Dizi A: 1♦-(Pas)-1♠-(2♥)-?      Dizi B: 1♦-(1♠)-Dbl(negatif)-(2♠)-?        (R = rakibin rengi: A'da ♥, B'de ♠)
    /// AÇAN: Pas = minimum · Dbl = iyi el (15+) · A: 2♠ = 4♠ 11-14, 3♠ = 4+♦ 4♠ 15-17 · B: 3♥ = 4+♦ 4♥ 12-15 (daveti kabul eden)
    ///   3♣ = 5♦5♣ 15-17 · 3♦ = 6+♦ rekabetçi · 3R = cuebid, stoper sorar (6+♦ ya da 5♦5♣ 18+, R'de stoper yok) · 2NT = Goodbad.
    ///   2NT elleri: 5♦5♣ rekabetçi (≤14) · 6+♦ 15-17 · 5♦5♣ 18+ stoperli · 6+♦ 18+ stoperli · A: 5♦ 4♠ 18+ · B: 4+♦ 4♥ 16+.
    /// CEVAPÇI (2NT'den sonra): 3♣ = tercih (röle) · 3♦ = tercih, minimum · A: 3♠ 6+♠ davet, B: 3♥ 6+♥ 5-8 · 3R (cuebid) = FG.
    /// AÇANIN 2. HAMLESİ (3♣/3♦'dan sonra): Pas (5♦5♣ rekabetçi; 3♦'dan sonra 6+♦ davet de) · 3♦ (3♣'dan sonra: 6+♦ 15-17) ·
    ///   3R (5♦5♣ 18+ stoperli) · A: 3♠ (5♦ 4♠ 18+) · B: 3♥ (4+♦ 4♥ 16+) · 3NT (6+♦ 18+ stoperli).
    /// Basitleştirmeler: kitaptaki "DBL = iyi el" yalnızca 15+ HCP, "Pas = minimum" geri kalan eller sayıldı; 2NT'den sonra cevapçının 3♣/3♦ ayrımı
    /// (3♦ = 7 HCP ve altı, ♦ ≥ ♣) ve dizi A'nın 3♥ satırı (kitapta "4♥" basılı, maça olarak okundu: A'da 3♠) yorumdur.
    /// </summary>
    public class Araya_GoodBad : IAcilisGelisimi
    {
        public int Oncelik { get { return 29; } }
        public string Aciklama { get { return "1♦ açanın araya girişe 2NT Goodbad gelişmeleri (kitap s.95-96)"; } }

        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = d.Gecmis;
            int i = g.FindIndex(h => h.GercekTeklifMi);
            if (i < 0 || g[i].Teklif != "1♦") return null;
            for (int k = 0; k < i; k++) if (g[k].Teklif != "Pas") return null;
            if (g.Count < i + 4) return null;
            var acici = g[i].Oyuncu;
            if (d.Rakipler.Contains(acici)) return null;
            bool ortakMi = acici != d.AktifOyuncu;                   // aktif = cevapçı mı?
            bool dizA = g[i + 1].Teklif == "Pas" && g[i + 2].Teklif == "1♠" && g[i + 3].Teklif == "2♥";
            bool dizB = g[i + 1].Teklif == "1♠" && g[i + 2].Teklif == "Dbl" && g[i + 3].Teklif == "2♠";
            if (!dizA && !dizB) return null;
            // Oyuncu doğrulaması: rakipler 1♠/2♥/2♠ söyleyen, cevapçı ortak
            if (dizA && (g[i + 2].Oyuncu != IhaleDurumu.PartnerBul(acici) || !d.Rakipler.Contains(g[i + 3].Oyuncu))) return null;
            if (dizB && (g[i + 2].Oyuncu != IhaleDurumu.PartnerBul(acici) || !d.Rakipler.Contains(g[i + 1].Oyuncu) || !d.Rakipler.Contains(g[i + 3].Oyuncu))) return null;
            if (d.AktifOyuncu != acici && d.AktifOyuncu != IhaleDurumu.PartnerBul(acici)) return null;

            char r = dizA ? '♥' : '♠';
            var e = new OcEl(el);
            int n = g.Count - (i + 4);
            if (n == 0 && !ortakMi) return Acici(e, dizA, r);
            if (n == 2 && ortakMi && g[i + 4].Teklif == "2NT" && g[i + 5].Teklif == "Pas") return Cevapci(e, dizA, r);
            if (n == 4 && !ortakMi && g[i + 4].Teklif == "2NT" && g[i + 5].Teklif == "Pas" && g[i + 7].Teklif == "Pas"
                && (g[i + 6].Teklif == "3♣" || g[i + 6].Teklif == "3♦"))
                return Acici2(e, dizA, r, g[i + 6].Teklif);
            return null;
        }

        /// <summary>2NT ile gösterilen el tipi (yoksa 0): 1=5♦5♣ rekabetçi 2=6+♦ davet 3=5♦5♣ 18+ stoperli 4=6+♦ 18+ stoperli 5=majörlü güçlü.</summary>
        private static int Tip(OcEl e, bool dizA, char r)
        {
            int hp = e.Hcp; bool dur = e.Stopper(Ad(r));
            int m = dizA ? e.S : e.H;
            if (e.D >= 5 && e.C >= 5)
            {
                if (hp <= 14) return 1;
                if (hp >= 18 && dur) return 3;
                return 0;
            }
            if (e.D >= 6)
            {
                if (hp >= 15 && hp <= 17) return 2;
                if (hp >= 18 && dur) return 4;
                return 0;
            }
            if (dizA && m >= 4 && e.D >= 5 && hp >= 18) return 5;
            if (!dizA && m >= 4 && e.D >= 4 && hp >= 16) return 5;
            return 0;
        }

        private static string Acici(OcEl e, bool dizA, char r)
        {
            int hp = e.Hcp; int m = dizA ? e.S : e.H;
            // Ortağın majöründe uyum (A: ♠ 4'lü destek; B: ♥ 4'lü, 4+♦)
            if (dizA)
            {
                if (e.S >= 4 && hp >= 11 && hp <= 14) return "2♠";
                if (e.S >= 4 && e.D >= 4 && hp >= 15 && hp <= 17) return "3♠";
            }
            else if (e.H >= 4 && e.D >= 4 && hp >= 12 && hp <= 15) return "3♥";
            // Minör elleri
            if (e.D >= 5 && e.C >= 5)
            {
                if (hp >= 18) return e.Stopper(Ad(r)) ? "2NT" : "3" + r;
                if (hp >= 15) return "3♣";
                return "2NT";
            }
            if (e.D >= 6)
            {
                if (hp >= 18) return e.Stopper(Ad(r)) ? "2NT" : "3" + r;
                if (hp >= 15) return "2NT";
                return "3♦";
            }
            if (Tip(e, dizA, r) == 5) return "2NT";
            return hp >= 15 ? "Dbl" : "Pas";
        }

        private static string Cevapci(OcEl e, bool dizA, char r)
        {
            int hp = e.Hcp;
            if (hp >= 11) return "3" + r;                                // cuebid: FG
            if (dizA && e.S >= 6 && hp >= 8) return "3♠";                // davet
            if (!dizA && e.H >= 6 && hp >= 5 && hp <= 8) return "3♥";
            return (hp <= 7 && e.D >= e.C) ? "3♦" : "3♣";                // tercih
        }

        private static string Acici2(OcEl e, bool dizA, char r, string cevap)
        {
            int tip = Tip(e, dizA, r);
            switch (tip)
            {
                case 1: return "Pas";
                case 2: return cevap == "3♣" ? "3♦" : "Pas";
                case 3: return "3" + r;
                case 4: return "3NT";
                case 5: return dizA ? "3♠" : "3♥";
                default: return null;
            }
        }
    }
}
