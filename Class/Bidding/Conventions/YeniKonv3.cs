using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    // ═══════════════════════════════════════════════════════════════════════
    // REKABET (müdahale ve rakip girişleri)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Weak Jump Overcalls: (1x) sonrası sıçramalı 2♦/2♥/2♠ = 6 kartlık iyi renk, 5-9 HP; ilerletici destekle yükseltir.</summary>
    internal sealed class WeakJumpOvercalls : YeniKonvTaban
    {
        public override string Ad { get { return "Weak Jump Overcalls"; } }
        public override string Aciklama { get { return "(1x)–2y sıçrama: 6 kart, 5-9 HP, iyi renk"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 1 && z.Hepsi[0].StartsWith("R:"))
            {
                string x = YK2.BirRenk(z.Hepsi[0], "R");
                if (x == null || z.Hp < 5 || z.Hp > 9) return null;
                foreach (var y in new[] { "Maça", "Kupa", "Karo" })
                    if (YK2.Sira(y) > YK2.Sira(x) && z.Uz(y) == 6 && YK.Hcp(z.El, y) >= 3) return YK.T(2, y);
                return null;
            }
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("L:") && z.Hepsi[1].StartsWith("O:"))
            {
                string x = YK2.BirRenk(z.Hepsi[0], "L");
                string c = YK.Cagri(z.Hepsi[1]);
                string y = YK.KozOf(c);
                if (x == null || y == null || y == "NT" || YK.Sev(c) != 2 || YK2.Sira(y) <= YK2.Sira(x)) return null;
                if (z.Uz(y) < 3) return null;
                if (YK.Major(y)) return z.Hp >= 13 ? YK.T(4, y) : YK.T(3, y);
                return z.Hp >= 14 ? "3NT" : "3♦";
            }
            return null;
        }
    }

    /// <summary>Maximal Overcall Double: 1M–2M raise, rakip 3 seviyesinde alt bir renk söyleyince açıcının kontrası oyun daveti (15+ HP).</summary>
    internal sealed class MaximalOvercallDbl : YeniKonvTaban
    {
        public override string Ad { get { return "Maximal Overcall Dbl."; } }
        public override string Aciklama { get { return "1M–2M–(3x alt renk)–Dbl: oyun daveti; maks → 4M, min → 3M"; } }
        protected override string Karar(Dz z)
        {
            // Açıcı
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[2].StartsWith("R:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                if (!YK.Major(m) || z.Hepsi[1] != "O:" + YK.T(2, m)) return null;
                string c = YK.Cagri(z.Hepsi[2]); string x = YK.KozOf(c);
                if (YK.Sev(c) != 3 || x == null || x == "NT" || YK2.Sira(x) >= YK2.Sira(m)) return null;
                return z.Hp >= 15 ? "Dbl" : null;
            }
            // Raise eden
            if (z.Hepsi.Count == 4 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("B:") &&
                z.Hepsi[2].StartsWith("L:") && z.Hepsi[3] == "O:Dbl")
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                if (!YK.Major(m) || z.Hepsi[1] != "B:" + YK.T(2, m)) return null;
                string c = YK.Cagri(z.Hepsi[2]); string x = YK.KozOf(c);
                if (YK.Sev(c) != 3 || x == null || x == "NT" || YK2.Sira(x) >= YK2.Sira(m)) return null;
                return z.Hp >= 8 ? YK.T(4, m) : YK.T(3, m);
            }
            return null;
        }
    }

    /// <summary>Ghestem: (1x) sonrası iki renkli (5-5) eller: cue-bid = kalan iki yüksek renk, 2NT = iki alçak, 3♣ = en alçak + en yüksek.</summary>
    internal sealed class Ghestem : YeniKonvTaban
    {
        public override string Ad { get { return "Ghestem"; } }
        public override string Aciklama { get { return "Cue = iki yüksek, 2NT = iki alçak, 3♣ = alçak+yüksek (kalan üç renkten)"; } }

        private static List<string> Kalan(string x)
        {
            return new[] { "Sinek", "Karo", "Kupa", "Maça" }.Where(r => r != x).ToList();   // alçaktan yükseğe
        }

        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 1 && z.Hepsi[0].StartsWith("R:"))
            {
                string x = YK2.BirRenk(z.Hepsi[0], "R");
                if (x == null || z.Hp < 6) return null;
                var u = Kalan(x);   // u[0] < u[1] < u[2]
                if (z.Uz(u[1]) >= 5 && z.Uz(u[2]) >= 5) return YK.T(2, x);   // iki yüksek
                if (z.Uz(u[0]) >= 5 && z.Uz(u[1]) >= 5) return "2NT";        // iki alçak
                if (z.Uz(u[0]) >= 5 && z.Uz(u[2]) >= 5) return "3♣";         // alçak + yüksek
                return null;
            }
            // İlerletici: ortak iki renk gösterdi
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("L:") && z.Hepsi[1].StartsWith("O:"))
            {
                string x = YK2.BirRenk(z.Hepsi[0], "L");
                if (x == null) return null;
                string c = YK.Cagri(z.Hepsi[1]);
                var u = Kalan(x);
                string a, b;
                if (c == YK.T(2, x)) { a = u[1]; b = u[2]; }
                else if (c == "2NT") { a = u[0]; b = u[1]; }
                else if (c == "3♣") { a = u[0]; b = u[2]; }
                else return null;
                string sec = z.Uz(a) > z.Uz(b) ? a : (z.Uz(b) > z.Uz(a) ? b : (YK2.Sira(a) > YK2.Sira(b) ? a : b));
                if (c == "3♣" && sec == "Sinek") return "Pas";
                for (int lv = 2; lv <= 5; lv++)
                    if (YK2.Yuksek(YK.T(lv, sec), c)) return YK.T(lv, sec);
                return null;
            }
            return null;
        }
    }

    /// <summary>Invisible Cue-Bids: 1m–(1M) sonrası rakip majörünün cue-bid'i = minörde oyun zorlayan raise (4+ destek, 12+ HP).</summary>
    internal sealed class InvisibleCueBids : YeniKonvTaban
    {
        public override string Ad { get { return "Invisible Cue-Bids"; } }
        public override string Aciklama { get { return "1m–(1M)–2M: minör açılışına oyun zorlayan raise (4+ destek, 12+ HP)"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("R:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                string c = YK.Cagri(z.Hepsi[1]);
                string M = YK.KozOf(c);
                if ((m != "Sinek" && m != "Karo") || YK.Sev(c) != 1 || !YK.Major(M)) return null;
                return (z.Uz(m) >= 4 && z.Hp >= 12) ? YK.T(2, M) : null;
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[1].StartsWith("L:") && z.Hepsi[2].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                string c = YK.Cagri(z.Hepsi[1]); string M = YK.KozOf(c);
                if ((m != "Sinek" && m != "Karo") || YK.Sev(c) != 1 || !YK.Major(M)) return null;
                if (YK.Cagri(z.Hepsi[2]) != YK.T(2, M)) return null;
                return ElDegerlendirici.StopperVar(z.El, M) ? "3NT" : YK.T(3, m);
            }
            return null;
        }
    }

    /// <summary>Limit-Plus Cue-Bids: 1M–(rakip rengi c)–cue-bid = 3+ destekle limit raise ya da daha iyisi (10+ HP).</summary>
    internal sealed class LimitPlusCueBids : YeniKonvTaban
    {
        public override string Ad { get { return "Limit-Plus Cue-Bids"; } }
        public override string Aciklama { get { return "1M–(c)–cue-bid: 3+ destekle limit raise ya da üstü"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("R:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                string c = YK.Cagri(z.Hepsi[1]); string x = YK.KozOf(c);
                if (!YK.Major(m) || x == null || x == "NT" || x == m || YK.Sev(c) > 2) return null;
                return (z.Uz(m) >= 3 && z.Hp >= 10) ? YK.T(YK.Sev(c) + 1, x) : null;
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[1].StartsWith("L:") && z.Hepsi[2].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                string c = YK.Cagri(z.Hepsi[1]); string x = YK.KozOf(c);
                if (!YK.Major(m) || x == null || x == "NT" || x == m || YK.Sev(c) > 2) return null;
                string cue = YK.Cagri(z.Hepsi[2]);
                if (cue != YK.T(YK.Sev(c) + 1, x)) return null;
                bool oyun = z.Hp >= 15 || (z.Hp >= 13 && z.Uz(m) >= 6);
                string uc = YK.T(3, m);
                if (oyun || !YK2.Yuksek(uc, cue)) return YK.T(4, m);
                return uc;
            }
            return null;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SLAM VE KONTROL SORULARI
    // ═══════════════════════════════════════════════════════════════════════

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

    /// <summary>Control Asking Bids: 1M–3M sonrası açıcının 4♣ (♠ kozda 4♦ da) teklifi = o renkte 1. ya da 2. tur kontrol sorusu; 4M = yok, bir üst adım = var.</summary>
    internal sealed class ControlAskingBids : YeniKonvTaban
    {
        public override string Ad { get { return "Control Asking Bids"; } }
        public override string Aciklama { get { return "1M–3M–4♣/4♦: o renkte A/K/tekil/void sorusu; 4M = yok, yukarı adım = var"; } }

        private static string Cevap(string m, string ask)
        {
            // var: ask'ın hemen üstündeki ilk koz-dışı teklif
            if (ask == "4♣") return "4♦";
            return "4♥";   // ask 4♦ (yalnız ♠ kozda)
        }

        protected override string Karar(Dz z)
        {
            if (z.Es("B:1♥", "O:3♥") || z.Es("B:1♠", "O:3♠"))
            {
                string m = z.Hepsi[0] == "B:1♥" ? "Kupa" : "Maça";
                if (z.Hp < 18 || z.Uz(m) < 5) return null;
                bool zayif = (z.Sinek > 0 && YK.AK(z.El, "Sinek") == 0);
                if (YK.Renk.Any(r => r != m && z.Uz(r) >= 4)) return null;   // Mathe bölgesi (yan renk var)
                if (z.Sinek >= 1 && YK.AK(z.El, "Sinek") == 0) return "4♣";
                if (m == "Maça" && z.Karo >= 1 && YK.AK(z.El, "Karo") == 0) return "4♦";
                return null;
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("B:") && z.Hepsi[2].StartsWith("O:"))
            {
                string m = z.Hepsi[0] == "O:1♥" ? "Kupa" : z.Hepsi[0] == "O:1♠" ? "Maça" : null;
                if (m == null || z.Hepsi[1] != "B:" + YK.T(3, m)) return null;
                string ask = YK.Cagri(z.Hepsi[2]);
                if (ask != "4♣" && !(m == "Maça" && ask == "4♦")) return null;
                string r = ask == "4♣" ? "Sinek" : "Karo";
                bool kontrol = z.Uz(r) <= 1 || YK.AK(z.El, r) > 0;
                return kontrol ? Cevap(m, ask) : YK.T(4, m);
            }
            if (z.Hepsi.Count == 4 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[2].StartsWith("B:"))
            {
                string m = z.Hepsi[0] == "B:1♥" ? "Kupa" : z.Hepsi[0] == "B:1♠" ? "Maça" : null;
                if (m == null || z.Hepsi[1] != "O:" + YK.T(3, m)) return null;
                string ask = YK.Cagri(z.Hepsi[2]);
                if (ask != "4♣" && !(m == "Maça" && ask == "4♦")) return null;
                string rep = YK.Cagri(z.Hepsi[3]);
                if (rep == YK.T(4, m)) return "Pas";
                if (rep == Cevap(m, ask)) return z.Hp >= 19 ? "4NT" : YK.T(4, m);
            }
            return null;
        }
    }

    /// <summary>Specific King Asks: RKCB (4NT) cevabından sonra 5 seviyesinde yan renk teklifi = o renkte papaz sorusu; 6M = var, 5M = yok.</summary>
    internal sealed class SpecificKingAsks : YeniKonvTaban
    {
        public override string Ad { get { return "Specific King Asks"; } }
        public override string Aciklama { get { return "4NT–5x sonrası 5y: y papazı sorusu; 6M = var, 5M = yok"; } }
        public override int Oncelik { get { return 5; } }

        protected override string Karar(Dz z)
        {
            string koz = z.D.AnlasilanKoz();
            if (!YK.Major(koz)) return null;
            // Soran: 4NT'm ve ortağın 5. seviye cevabı
            if (z.Hepsi.Count >= 2)
            {
                int n = z.Hepsi.Count;
                if (z.Hepsi[n - 2] == "B:4NT" && z.Hepsi[n - 1].StartsWith("O:5") && YK.KozOf(YK.Cagri(z.Hepsi[n - 1])) != "NT")
                {
                    string rep = YK.Cagri(z.Hepsi[n - 1]);
                    int kk = ElDegerlendirici.KilitKartSayisi(z.El, koz);
                    if (kk < 3 || z.Hp < 15) return null;
                    foreach (var r in new[] { "Sinek", "Karo", "Kupa", "Maça" })
                    {
                        if (r == koz || z.El.Any(c => c.Suit == r && c.Value == 13)) continue;
                        string ask = YK.T(5, r);
                        if (!YK2.Yuksek(ask, rep) || !YK2.Yuksek(YK.T(5, koz), ask)) continue;
                        if (z.Uz(r) == 0) continue;
                        return ask;
                    }
                    return null;
                }
                // Cevaplayan: ortağın 4NT'si, benim cevabım, ortağın 5y sorusu
                if (n >= 3 && z.Hepsi[n - 3] == "O:4NT" && z.Hepsi[n - 2].StartsWith("B:5") && z.Hepsi[n - 1].StartsWith("O:5"))
                {
                    string y = YK.KozOf(YK.Cagri(z.Hepsi[n - 1]));
                    if (y == null || y == "NT" || y == koz) return null;
                    bool papaz = z.El.Any(c => c.Suit == y && c.Value == 13);
                    return papaz ? YK.T(6, koz) : YK.T(5, koz);
                }
                // Soran, cevap aldı
                if (n >= 4 && z.Hepsi[n - 4] == "B:4NT" && z.Hepsi[n - 2].StartsWith("B:5") && z.Hepsi[n - 1].StartsWith("O:"))
                {
                    string c = YK.Cagri(z.Hepsi[n - 1]);
                    if (c == YK.T(6, koz) || c == YK.T(5, koz)) return "Pas";
                }
            }
            return null;
        }
    }

    /// <summary>Voluntary Five Major Bid: ortağın majör rengine karşı rakip 4 majör deyince, 4+ destekli, rakip rengi kısa, ≤9 HP el 5M ile fedakârlık yapar.</summary>
    internal sealed class VoluntaryFiveMajor : YeniKonvTaban
    {
        public override string Ad { get { return "Voluntary Five Major Bid"; } }
        public override string Aciklama { get { return "(4M) karşısında ortağın majörüne 4+ destek, rakip rengi kısa, ≤9 HP: 5M fedakârlığı"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count != 2 || !z.Hepsi[0].StartsWith("O:") || !z.Hepsi[1].StartsWith("R:")) return null;
            string y = YK2.BirRenk(z.Hepsi[0], "O");
            string c = YK.Cagri(z.Hepsi[1]);
            string m = YK.KozOf(c);
            if (!YK.Major(y) || !YK.Major(m) || m == y || c != YK.T(4, m)) return null;
            return (z.Uz(y) >= 4 && z.Uz(m) <= 1 && z.Hp <= 9) ? YK.T(5, y) : null;
        }
    }
}
