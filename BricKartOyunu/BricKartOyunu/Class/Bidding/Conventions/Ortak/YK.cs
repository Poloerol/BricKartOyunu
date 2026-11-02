using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Yeni konvansiyonlar için küçük yardımcılar.</summary>
    internal static class YK
    {
        public static readonly string[] Renk = { "Maça", "Kupa", "Karo", "Sinek" };
        public static readonly string[] Sem = { "♠", "♥", "♦", "♣" };

        public static string S(string renk) { int i = Array.IndexOf(Renk, renk); return i < 0 ? "" : Sem[i]; }
        public static string T(int sev, string renk) { return sev + (renk == "NT" ? "NT" : S(renk)); }

        /// <summary>"1♠" → "Maça", "2NT" → "NT"; çağrı değilse null.</summary>
        public static string KozOf(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2 || !char.IsDigit(teklif[0])) return null;
            if (teklif.EndsWith("NT")) return "NT";
            int i = Array.IndexOf(Sem, teklif.Substring(teklif.Length - 1));
            return i < 0 ? null : Renk[i];
        }
        public static int Sev(string teklif) { return string.IsNullOrEmpty(teklif) || !char.IsDigit(teklif[0]) ? 0 : teklif[0] - '0'; }
        /// <summary>"O:1♠" gibi etiketli çağrıdan çağrının kendisi.</summary>
        public static string Cagri(string etiketli) { return etiketli.Substring(2); }

        public static bool Major(string renk) { return renk == "Maça" || renk == "Kupa"; }

        /// <summary>En uzun renk (eşitlikte Maça, Kupa, Karo, Sinek sırası).</summary>
        public static string EnUzun(List<Card> el)
        {
            string en = Renk[0]; int n = -1;
            foreach (var r in Renk) { int u = el.Count(c => c.Suit == r); if (u > n) { n = u; en = r; } }
            return en;
        }
        public static int Uz(List<Card> el, string renk) { return el.Count(c => c.Suit == renk); }

        /// <summary>Verilen renkte A, K, Q'dan kaçı var.</summary>
        public static int ÜstHonor(List<Card> el, string renk) { return el.Count(c => c.Suit == renk && c.Value >= 12); }
        public static bool AKQ(List<Card> el, string renk) { return ÜstHonor(el, renk) == 3; }
        /// <summary>Renkteki A/K sayısı.</summary>
        public static int AK(List<Card> el, string renk) { return el.Count(c => c.Suit == renk && c.Value >= 13); }
        public static int Hcp(List<Card> el, string renk)
        {
            int p = 0;
            foreach (var c in el) if (c.Suit == renk && c.Value >= 11) p += c.Value - 10;
            return p;
        }
    }
}
