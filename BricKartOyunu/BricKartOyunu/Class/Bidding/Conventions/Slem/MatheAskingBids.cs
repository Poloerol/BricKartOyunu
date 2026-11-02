using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Mathe Asking Bids: 1M–3M (limit raise) sonrası açıcının en ucuz yeni teklifi (3♠ / 1♠'te 3NT) = kısa renk sorusu.</summary>
    internal sealed class MatheAskingBids : YeniKonvTaban
    {
        public override string Ad { get { return "Mathe Asking Bids"; } }
        public override string Aciklama { get { return "1M–3M–3♠(♥ koz)/3NT(♠ koz): kısa renk sorusu; 3NT/4♣/4♦/4♥ cevap"; } }
        public override int Oncelik { get { return 5; } }

        private static string[] Siralar(string m) { return m == "Kupa" ? new[] { "Maça", "Sinek", "Karo" } : new[] { "Sinek", "Karo", "Kupa" }; }
        private static string[] Cevaplar(string m) { return m == "Kupa" ? new[] { "3NT", "4♣", "4♦" } : new[] { "4♣", "4♦", "4♥" }; }

        protected override string Karar(Dz z)
        {
            // Açıcı soruyor
            if (z.Es("B:1♥", "O:3♥") || z.Es("B:1♠", "O:3♠"))
            {
                string m = z.Hepsi[0] == "B:1♥" ? "Kupa" : "Maça";
                if (z.Hp < 17 || z.Uz(m) < 5) return null;
                bool yanRenk = YK.Renk.Any(r => r != m && z.Uz(r) >= 4);
                return yanRenk ? (m == "Kupa" ? "3♠" : "3NT") : null;
            }
            // Cevaplayan
            if (z.Es("O:1♥", "B:3♥", "O:3♠") || z.Es("O:1♠", "B:3♠", "O:3NT"))
            {
                string m = z.Hepsi[0] == "O:1♥" ? "Kupa" : "Maça";
                var sr = Siralar(m); var cv = Cevaplar(m);
                for (int i = 0; i < 3; i++) if (z.Uz(sr[i]) <= 1) return cv[i];
                return YK.T(4, m);
            }
            // Açıcının devamı
            if (z.Hepsi.Count == 4 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[2].StartsWith("B:"))
            {
                string m = z.Hepsi[0] == "B:1♥" ? "Kupa" : z.Hepsi[0] == "B:1♠" ? "Maça" : null;
                if (m == null) return null;
                if (z.Hepsi[1] != "O:" + YK.T(3, m)) return null;
                if (z.Hepsi[2] != (m == "Kupa" ? "B:3♠" : "B:3NT")) return null;
                string c = YK.Cagri(z.Hepsi[3]);
                if (c == YK.T(4, m)) return "Pas";
                var sr = Siralar(m); var cv = Cevaplar(m);
                int i = Array.IndexOf(cv, c);
                if (i < 0) return null;
                return (z.Uz(sr[i]) >= 3 && z.Hp >= 18) ? "4NT" : YK.T(4, m);
            }
            return null;
        }
    }
}
