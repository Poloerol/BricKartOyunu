using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Hello over 1NT: rakibin 1NT'sine karşı müdahale (2♣ iki majör, 2♦ tek renkli majör, 2♥/2♠ majör+minör, 2NT iki minör).</summary>
    internal sealed class HelloOver1NT : YeniKonvTaban
    {
        public override string Ad { get { return "Hello over 1NT"; } }
        public override string Aciklama { get { return "Rakip 1NT'sine: 2♣ iki majör, 2♦ uzun majör, 2♥/2♠ majör+minör, 2NT iki minör"; } }
        protected override string Karar(Dz z)
        {
            int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
            if (z.Es("R:1NT") || z.Es("L:1NT"))
            {
                if (z.Hp < 8 || z.Hp > 15) return null;
                if (s >= 4 && h >= 4 && (s >= 5 || h >= 5)) return "2♣";
                if (s >= 6 || h >= 6) return "2♦";
                if (s >= 5 && (d >= 4 || c >= 4)) return "2♠";
                if (h >= 5 && (d >= 4 || c >= 4)) return "2♥";
                if (d >= 5 && c >= 5) return "2NT";
                return null;
            }
            // Ortak müdahale etti, sıra bende (ilerletici)
            if (z.Es("L:1NT", "O:2♣"))
            {
                string m = h >= s ? "Kupa" : "Maça";
                int u = z.Uz(m);
                if (z.Hp >= 10 && u >= 4) return YK.T(3, m);
                return YK.T(2, m);
            }
            if (z.Es("L:1NT", "O:2♦")) return "2♥";                       // geçer ya da düzelt
            if (z.Es("L:1NT", "O:2NT")) return d >= c ? "3♦" : "3♣";
            // Müdahaleci: 2♦ → 2♥ cevabından sonra
            if (z.Es("R:1NT", "B:2♦", "O:2♥") || z.Es("L:1NT", "B:2♦", "O:2♥")) return h >= 6 ? "Pas" : "2♠";
            return null;
        }
    }
    /// <summary>1NT savunma konvansiyonları için yardımcılar (R:1NT = sağ rakip açtı; L:1NT = sol rakip açtı, 4. el).</summary>
    internal static class NTSav
    {
        /// <summary>Dizi "R:1NT" ya da "L:1NT" ile başlayıp tam olarak bu devamla mı sürüyor?</summary>
        public static bool Ac(Dz z, params string[] devam)
        {
            if (z.Hepsi.Count != devam.Length + 1) return false;
            if (z.Hepsi[0] != "R:1NT" && z.Hepsi[0] != "L:1NT") return false;
            for (int i = 0; i < devam.Length; i++) if (z.Hepsi[i + 1] != devam[i]) return false;
            return true;
        }
        public static bool Sag1NT(Dz z) { return z.Es("R:1NT") || z.Es("L:1NT"); }
        public static string EnUzunRenk(Dz z, bool minorDahil = true)
        {
            string en = "Maça"; int n = -1;
            foreach (var r in new[] { "Maça", "Kupa", "Karo", "Sinek" })
            { if (!minorDahil && (r == "Karo" || r == "Sinek")) continue; if (z.Uz(r) > n) { n = z.Uz(r); en = r; } }
            return en;
        }
    }

    /// <summary>
    /// Brozel (rakip 1NT'sine): Dbl = tek renkli (ortak 2♣ röle eder), 2♣ ♣+♥, 2♦ ♦+♥, 2♥ ♥+♠, 2♠ ♠+minör, 2NT ♣+♦.
    /// </summary>
    internal sealed class Brozel : YeniKonvTaban
    {
        public override string Ad { get { return "Brozel"; } }
        public override string Aciklama { get { return "Rakip 1NT'sine: Dbl tek renk, 2♣ ♣+♥, 2♦ ♦+♥, 2♥ ♥+♠, 2♠ ♠+minör, 2NT minörler"; } }
        protected override string Karar(Dz z)
        {
            int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
            if (NTSav.Sag1NT(z))
            {
                if (z.Hp < 10 || z.Hp > 17) return null;
                if (h >= 4 && s >= 4 && (h >= 5 || s >= 5)) return "2♥";
                if (s >= 4 && (c >= 4 || d >= 4) && s >= 5) return "2♠";
                if (h >= 5 && c >= 4) return "2♣";
                if (h >= 5 && d >= 4) return "2♦";
                if (c >= 5 && d >= 4 || d >= 5 && c >= 4) return "2NT";
                if (c >= 4 && h >= 5) return "2♣";
                if (Math.Max(Math.Max(s, h), Math.Max(c, d)) >= 6) return "Dbl";
                return null;
            }
            // İlerletici
            if (NTSav.Ac(z, "O:2♣")) return c > h ? "Pas" : "2♥";
            if (NTSav.Ac(z, "O:2♦")) return d > h ? "Pas" : "2♥";
            if (NTSav.Ac(z, "O:2♥")) return h >= s ? "Pas" : "2♠";
            if (NTSav.Ac(z, "O:2♠")) return s >= 3 ? "Pas" : "2NT";
            if (NTSav.Ac(z, "O:2NT")) return d > c ? "3♦" : "3♣";
            if (NTSav.Ac(z, "O:Dbl")) return "2♣";
            // Müdahaleci ikinci tur
            if (z.Hepsi.Count == 3 && z.Hepsi[0] == "R:1NT" && z.Hepsi[1] == "B:2♠" && z.Hepsi[2] == "O:2NT") return d > c ? "3♦" : "3♣";
            if (z.Hepsi.Count == 3 && z.Hepsi[0] == "R:1NT" && z.Hepsi[1] == "B:Dbl" && z.Hepsi[2] == "O:2♣")
            {
                int en = Math.Max(Math.Max(s, h), Math.Max(c, d));
                if (c == en) return "Pas";
                return s == en ? "2♠" : h == en ? "2♥" : "2♦";
            }
            return null;
        }
    }

    /// <summary>Astro (rakip 1NT'sine): Dbl ceza, 2♣ ♥+minör, 2♦ ♠+başka renk, 2♥/2♠ doğal, 2NT minörler.</summary>
    internal sealed class Astro : YeniKonvTaban
    {
        public override string Ad { get { return "Astro"; } }
        public override string Aciklama { get { return "Rakip 1NT'sine: Dbl ceza, 2♣ ♥+minör, 2♦ ♠+başka, 2♥/2♠ doğal 5+, 2NT minörler"; } }
        protected override string Karar(Dz z)
        {
            int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
            if (NTSav.Sag1NT(z))
            {
                if (z.Hp < 10 || z.Hp > 17) return null;
                if (s >= 5 && (h >= 4 || c >= 4 || d >= 4)) return "2♦";
                if (h >= 5 && s >= 4) return "2♦";
                if (h >= 5 && (c >= 4 || d >= 4)) return "2♣";
                if (s >= 5) return "2♠";
                if (h >= 5) return "2♥";
                if (c >= 5 && d >= 4 || d >= 5 && c >= 4) return "2NT";
                return null;
            }
            if (NTSav.Ac(z, "O:2♣")) { if (z.Hp >= 12) return "2NT"; return h >= 3 ? "2♥" : "2♦"; }
            if (NTSav.Ac(z, "O:2♦")) { if (z.Hp >= 12) return "2NT"; return s >= 3 ? "2♠" : "2♥"; }
            if (NTSav.Ac(z, "O:2NT")) return d > c ? "3♦" : "3♣";
            // Müdahaleci ikinci tur
            if (z.Hepsi.Count == 3 && z.Hepsi[0] == "R:1NT" && z.Hepsi[1] == "B:2♣")
            {
                if (z.Hepsi[2] == "O:2♦") return d > c ? "Pas" : "3♣";
                if (z.Hepsi[2] == "O:2NT") return d > c ? "3♦" : "3♣";
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0] == "R:1NT" && z.Hepsi[1] == "B:2♦")
            {
                if (z.Hepsi[2] == "O:2♥") { if (h >= 4 && h >= c && h >= d) return "Pas"; return c >= d ? "3♣" : "3♦"; }
                if (z.Hepsi[2] == "O:2NT") return h >= 4 ? "3♥" : (c >= d ? "3♣" : "3♦");
            }
            return null;
        }
    }
}
