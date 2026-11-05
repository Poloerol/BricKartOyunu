using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Araya
{
    /// <summary>
    /// 1NT ARAYA GİRİŞE (15-18 HCP) CEVAPLAR (SAS kitabı s.106): rakip 1x açtı, ortak 1NT ile girdi, sağdaki rakip Pas.
    ///   2♣  = minör açılmışsa Stayman; majör açılmışsa 2♦'ye transfer
    ///   2♦  = ♥'e transfer (1♥ açılmışsa: 4♠ davet+)
    ///   2♥  = ♠'e transfer (1♠ açılmışsa: ♣'e transfer)
    ///   2♠  = ♣'e transfer, ♣ rakibin rengiyse ♦'e (1♠ açılmışsa: 4♥ davet+)
    ///   2NT = doğal davet, 3NT = doğal, 4♦/4♥ = 4♥/4♠'e transfer (Texas).
    /// Araya giren transferi en ucuz seviyeden tamamlar, Stayman'a majörünü gösterir. Cevapçının ikinci hamlesi:
    /// transfer sonrası 10+ HCP ile majörde oyun (minörde 3NT), Stayman sonrası uyuma göre oyun/davet.
    /// Basitleştirmeler: kitaptaki satırlar OCR'dan okunduğu için bazı satırlar (2♦/1♥ ve 2♠/1♠ davet+, minör transfer
    /// satırları) en makul yorumla uygulandı; 5'li majörle davet (2NT), blokatif 3x ve transfer sonrası ince gelişimler yok.
    /// </summary>
    public class Araya_NTCevap : IAcilisGelisimi
    {
        public int Oncelik { get { return 23; } }
        public string Aciklama { get { return "1NT araya girişe (15-18) transferli cevaplar ve Stayman"; } }

        private const string Sira = "♣♦♥♠";
        private static readonly string[] Cagrilar = { "2♣", "2♦", "2♥", "2♠" };

        private static List<IhaleHamlesi> Gercek(IhaleDurumu d) { return d.Gecmis.Where(h => h.GercekTeklifMi).ToList(); }
        private static int Ri(char c) { return Sira.IndexOf(c); }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }
        private static bool Major(char c) { return c == '♥' || c == '♠'; }

        /// <summary>Çağrının transfer hedefi (renk); transfer değilse '\0'.</summary>
        private static char Hedef(char x, string c)
        {
            switch (c)
            {
                case "2♣": return Major(x) ? '♦' : '\0';
                case "2♦": return x == '♥' ? '\0' : '♥';
                case "2♥": return x == '♠' ? '♣' : '♠';
                case "2♠": return x == '♠' ? '\0' : (x == '♣' ? '♦' : '♣');
            }
            return '\0';
        }
        private static bool Stayman(char x, string c) { return c == "2♣" && !Major(x); }
        /// <summary>Davet+ cue: açılış majörse diğer majörde 4'lü kart gösterir.</summary>
        private static char Cue4(char x, string c)
        {
            if (x == '♥' && c == "2♦") return '♠';
            if (x == '♠' && c == "2♠") return '♥';
            return '\0';
        }

        private static string Ucuz(int lv, char son, char z) { return (Ri(z) > Ri(son) ? lv : lv + 1).ToString() + z; }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = Gercek(d);
            if (g.Count < 2 || g.Count > 5) return null;
            if (!d.Rakipler.Contains(g[0].Oyuncu)) return null;
            string a = g[0].Teklif;
            if (a == null || a.Length != 2 || a[0] != '1' || !Sira.Contains(a[1])) return null;
            if (g[1].Teklif != "1NT") return null;
            char x = a[1];
            int ix = d.Gecmis.IndexOf(g[0]);
            if (ix < 0 || ix + 2 >= d.Gecmis.Count) return null;
            if (!ReferenceEquals(d.Gecmis[ix + 1], g[1]) || d.Gecmis[ix + 2].Teklif != "Pas") return null;
            Player ortak = IhaleDurumu.PartnerBul(d.AktifOyuncu);
            var e = new OcEl(el);

            // Cevapçı: x - 1NT - Pas - (ben)
            if (g.Count == 2)
            {
                if (g[1].Oyuncu != ortak || d.Gecmis.Count != ix + 3) return null;
                return Cevap(e, x);
            }

            string r1 = g[2].Teklif;
            if (!ReferenceEquals(d.Gecmis.ElementAtOrDefault(ix + 3), g[2]) || d.Gecmis.ElementAtOrDefault(ix + 4) == null || d.Gecmis[ix + 4].Teklif != "Pas") return null;

            // Araya giren: x - 1NT(ben) - Pas - cevap - Pas - (ben)
            if (g.Count == 3)
            {
                if (g[1].Oyuncu != d.AktifOyuncu || g[2].Oyuncu != ortak || d.Gecmis.Count != ix + 5) return null;
                return Tamamla(e, x, r1);
            }

            // Araya giren üçüncü hamle: x - 1NT(ben) - Pas - cevap - Pas - tamamlama(ben) - Pas - cevapçının davet/oyun teklifi - Pas - (ben)
            if (g.Count == 5)
            {
                if (g[1].Oyuncu != d.AktifOyuncu || g[2].Oyuncu != ortak || g[3].Oyuncu != d.AktifOyuncu || g[4].Oyuncu != ortak) return null;
                if (d.Gecmis.Count != ix + 9 || !ReferenceEquals(d.Gecmis[ix + 5], g[3]) || d.Gecmis[ix + 6].Teklif != "Pas"
                    || !ReferenceEquals(d.Gecmis[ix + 7], g[4]) || d.Gecmis[ix + 8].Teklif != "Pas") return null;
                string r2 = g[4].Teklif; int hpx = e.Hcp;
                if (r2 == "2NT") return hpx >= 17 ? "3NT" : "Pas";
                if (r2.Length == 2 && r2[0] == '3' && Major(r2[1])) return hpx >= 16 ? "4" + r2[1] : "Pas";
                return "Pas";
            }

            // Cevapçı ikinci hamle: x - 1NT - Pas - cevap(ben) - Pas - tamamlama - Pas - (ben)
            if (g[1].Oyuncu != ortak || g[2].Oyuncu != d.AktifOyuncu || g[3].Oyuncu != ortak) return null;
            if (d.Gecmis.Count != ix + 7 || !ReferenceEquals(d.Gecmis[ix + 5], g[3]) || d.Gecmis[ix + 6].Teklif != "Pas") return null;
            return IkinciHamle(e, x, r1, g[3].Teklif);
        }

        // ───────────────────────── cevapçı ─────────────────────────
        private static string TransferCagrisi(char x, char hedef)
        {
            foreach (var c in Cagrilar) if (Hedef(x, c) == hedef) return c;
            return null;
        }

        private static string Cevap(OcEl e, char x)
        {
            int hp = e.Hcp;
            // Texas: 6+ majör ve 10+ HCP
            foreach (char m in new[] { '♠', '♥' })
                if (m != x && Uz(e, m) >= 6 && hp >= 10) return m == '♥' ? "4♦" : "4♥";
            // Zayıf: uzun renge transfer ya da Pas
            if (hp < 8)
            {
                foreach (char m in new[] { '♠', '♥' })
                    if (m != x && Uz(e, m) >= 5) return TransferCagrisi(x, m) ?? "Pas";
                foreach (char m in new[] { '♦', '♣' })
                    if (m != x && Uz(e, m) >= 6) return TransferCagrisi(x, m) ?? "Pas";
                return "Pas";
            }
            // 5+ majör: transfer
            foreach (char m in new[] { '♠', '♥' })
                if (m != x && Uz(e, m) >= 5) { var t = TransferCagrisi(x, m); if (t != null) return t; }
            // 4'lü majör: Stayman / davet+ cue
            if (!Major(x) && (e.H >= 4 || e.S >= 4)) return "2♣";
            if (x == '♥' && e.S >= 4) return "2♦";
            if (x == '♠' && e.H >= 4) return "2♠";
            // Uzun minör
            if (hp >= 10)
                foreach (char m in new[] { '♦', '♣' })
                    if (m != x && Uz(e, m) >= 6) { var t = TransferCagrisi(x, m); if (t != null) return t; }
            // Doğal NT
            if (e.Dengeli || hp >= 10)
            {
                if (hp >= 10 && hp <= 15 && e.Stopper(Ad(x))) return "3NT";
                if (hp <= 9) return "2NT";
            }
            return "Pas";
        }

        // ───────────────────────── araya giren ─────────────────────────
        private static string Tamamla(OcEl e, char x, string r1)
        {
            int hp = e.Hcp;
            if (r1 == "4♦") return "4♥";
            if (r1 == "4♥") return "4♠";
            if (r1 == "2NT") return hp >= 17 ? "3NT" : "Pas";
            if (r1 == "3NT") return "Pas";
            if (Stayman(x, r1)) return e.H >= 4 ? "2♥" : (e.S >= 4 ? "2♠" : "2♦");
            char cue = Cue4(x, r1);
            if (cue != '\0')
            {
                if (Uz(e, cue) >= 4) return hp >= 16 ? "4" + cue : "3" + cue;
                return hp >= 16 ? "3NT" : "2NT";
            }
            char t = Hedef(x, r1);
            if (t != '\0') return Ucuz(2, r1[1], t);
            return null;
        }

        private static string IkinciHamle(OcEl e, char x, string r1, string cevap)
        {
            int hp = e.Hcp;
            if (r1 == "4♦" || r1 == "4♥" || r1 == "2NT" || r1 == "3NT") return "Pas";
            if (Cue4(x, r1) != '\0') return "Pas";
            if (Stayman(x, r1))
            {
                char m = cevap == "2♥" ? '♥' : (cevap == "2♠" ? '♠' : '\0');
                if (m != '\0' && Uz(e, m) >= 4) return hp >= 10 ? "4" + m : "3" + m;
                if (hp >= 10) return "3NT";
                return "2NT";
            }
            char t = Hedef(x, r1);
            if (t == '\0' || cevap == null || cevap.Length != 2 || cevap[1] != t) return null;
            if (hp >= 10) return Major(t) ? "4" + t : "3NT";
            return "Pas";
        }
    }
}
