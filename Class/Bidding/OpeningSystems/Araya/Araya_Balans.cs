using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Araya
{
    /// <summary>
    /// UYANDIRMA (BALANCING) TABLOSU (SAS kitabı s.109): 1x - Pas - Pas - (ben).
    ///   2NT = dengeli 19-20 (x'te stoper)      1NT = dengeli 12-15 (minör açılışta 12-14), x'te stoper
    ///   2x (cue) = x♣: 5+♦ ve 5+majör, x♦: 5+♣ ve 5+majör, x♥: 5+♠ ve 5+minör, x♠: 5+♥ ve 5+minör (11-15)
    ///   3y (çifte atlama) = 7+ kart, 5-11 HCP (blokaj)     2y (atlamalı) = 6+ kart, 11-15 HCP
    ///   1y / 2z doğal 5+ kart (1 seviye 8-15; 2 seviye 9-15 ve iyi renk)
    ///   DBL = 9+ HCP, diğer üç renkte 3+ (x'te en çok 3) ya da 16+; 1y (4 kartlı) doğal 8-15.
    /// Uyandırma kontrasına cevaplar: Araya_TOCevap (2 HCP eklenir).
    /// Basitleştirmeler: 3x (stoper sorma) ve 3NT (kapalı renk) girişleri yok.
    /// </summary>
    public class Araya_Balans : IAcilisGelisimi
    {
        public int Oncelik { get { return 27; } }
        public string Aciklama { get { return "Uyandırma (balancing) girişleri: 1x-Pas-Pas-?"; } }

        private const string Sira = "♣♦♥♠";
        private static List<IhaleHamlesi> Gercek(IhaleDurumu d) { return d.Gecmis.Where(h => h.GercekTeklifMi).ToList(); }
        private static int Ri(char c) { return Sira.IndexOf(c); }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }
        private static string Ucuz(int lv, char son, char z) { return (Ri(z) > Ri(son) ? lv : lv + 1).ToString() + z; }
        private static int RenkHcp(OcEl e, char c)
        {
            string ad = Ad(c); int s = 0;
            foreach (var k in e.El) if (k.Suit == ad && k.Value >= 11) s += k.Value - 10;
            return s;
        }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = Gercek(d);
            if (g.Count != 1 || !d.Rakipler.Contains(g[0].Oyuncu)) return null;
            string a = g[0].Teklif;
            if (a == null || a.Length != 2 || a[0] != '1' || !Sira.Contains(a[1])) return null;
            int ix = d.Gecmis.IndexOf(g[0]);
            if (ix < 0 || d.Gecmis.Count != ix + 3 || d.Gecmis[ix + 1].Teklif != "Pas" || d.Gecmis[ix + 2].Teklif != "Pas") return null;
            return Balans(new OcEl(el), a[1]);
        }

        private static string Balans(OcEl e, char x)
        {
            int hp = e.Hcp;
            bool major = x == '♥' || x == '♠';
            bool stx = e.Stopper(Ad(x));
            if (e.Dengeli && stx)
            {
                if (hp >= 19 && hp <= 20) return "2NT";
                if (hp >= 12 && hp <= (major ? 15 : 14)) return "1NT";
            }
            // Cue: iki renkli (açılış değerleri)
            if (hp >= 11 && hp <= 15)
            {
                bool cue = false;
                if (x == '♣') cue = e.D >= 5 && (e.H >= 5 || e.S >= 5);
                else if (x == '♦') cue = e.C >= 5 && (e.H >= 5 || e.S >= 5);
                else if (x == '♥') cue = e.S >= 5 && (e.C >= 5 || e.D >= 5);
                else cue = e.H >= 5 && (e.C >= 5 || e.D >= 5);
                if (cue) return "2" + x;
            }
            var renkler = Sira.Where(c => c != x).OrderByDescending(c => Uz(e, c)).ThenByDescending(c => Ri(c)).ToList();
            char en = renkler[0]; int uz = Uz(e, en);
            string ucuz = Ucuz(1, x, en);
            // Blokaj / atlamalı
            if (uz >= 7 && hp <= 11 && hp >= 5) { string j = Ucuz(3, x, en); if (j[0] <= '4') return j; }
            if (uz >= 6 && hp >= 11 && hp <= 15) { string j = Ucuz(2, x, en); if (j[0] <= '3') return j; }
            // Doğal 5+ kart
            if (uz >= 5)
            {
                if (ucuz[0] == '1' && hp >= 8 && hp <= 15) return ucuz;
                if (ucuz[0] == '2' && hp >= 9 && hp <= 15 && RenkHcp(e, en) >= 3) return ucuz;
            }
            // Kontra
            bool sekil = Sira.Where(c => c != x).All(c => Uz(e, c) >= 3) && Uz(e, x) <= 3;
            if (hp >= 16 || (hp >= 9 && sekil)) return "Dbl";
            // 4 kartlı doğal 1 seviye
            if (hp >= 8 && hp <= 15 && uz >= 4 && ucuz[0] == '1') return ucuz;
            return null;
        }
    }
}
