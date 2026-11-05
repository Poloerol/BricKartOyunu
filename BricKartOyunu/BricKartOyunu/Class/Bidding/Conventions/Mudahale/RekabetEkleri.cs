using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Good-Bad 2NT (Sasson kitabı, s. 95-96). Açan 1♦ açtı, ortak majörle cevapladı (ya da negatif kontra yaptı),
    /// sağ rakip 2 seviyesinde majörle araya girdi. Açanın 2NT'si "iyi/kötü" röleidir: ortak 3♣ (tercih) ya da 3♦ (min. tercih) der;
    /// açan Pas ile (kötü, rekabetçi el) ya da yeni teklifle (iyi el) devam eder. 3♥/3♠ ortağın cue-bid'i = oyun zorlayan el.
    ///   S1: 1♦–(Pas)–1M–(2N)      S2: 1♦–(1M)–Dbl–(2M)
    /// </summary>
    internal sealed class GoodBad2NT : YeniKonvTaban
    {
        public override string Ad { get { return "Good-Bad 2NT"; } }
        public override string Aciklama { get { return "1♦–…–(2M)–2NT: kötü/iyi röle; 3♣ tercih, 3♦ min. tercih; açan Pas = kötü, yeni teklif = iyi"; } }

        private static bool Mj(string r) { return r == "Maça" || r == "Kupa"; }

        protected override string Karar(Dz z)
        {
            var h = z.Hepsi;
            // ── Açanın ilk kararı (2NT) ve ikinci kararı ────────────────────────────────
            string M, N; bool s2;    // M: ortağın gösterdiği majör (S1: cevap majörü, S2: negatif kontranın gösterdiği majör); N: sağ rakibin rengi
            int ix = AcanBagla(h, out M, out N, out s2);
            if (ix > 0)
            {
                int d = z.Karo, c = z.Sinek, mUz = z.Uz(M), hp = z.Hp;
                bool dur = ElDegerlendirici.StopperVar(z.El, N);
                if (ix == 1)       // henüz 2NT demedi
                {
                    bool kotu = d >= 5 && c >= 5 && hp <= 14;                         // 5♦5♣ rekabetçi
                    bool davet = d >= 6 && hp >= 15 && hp <= 17;                      // 6+♦ davet
                    bool guclu55 = d >= 5 && c >= 5 && hp >= 18 && dur;               // 5♦5♣ 18+ stoperli
                    bool guclu6 = d >= 6 && hp >= 18 && dur;                          // 6+♦ 18+ stoperli
                    bool fitli = !s2 ? (d >= 5 && mUz >= 4 && hp >= 18) : (d >= 4 && mUz >= 4 && hp >= 16);
                    return (kotu || davet || guclu55 || guclu6 || fitli) ? "2NT" : null;
                }
                // ix == 2: 2NT'ye ortak 3♣ ya da 3♦ dedi
                string cevap = YK.Cagri(h[h.Count - 1]);
                if (cevap != "3♣" && cevap != "3♦") return null;
                string cue = YK.T(3, N), maj3 = YK.T(3, M);
                if (s2)
                {
                    if (d >= 4 && mUz >= 4 && hp >= 16) return maj3;                  // 3♥ = 4+♦ 4♥ 16+
                    if (d >= 5 && c >= 5 && hp >= 18 && dur) return cue;              // 3♠ = 5♦5♣ 18+ stoperli
                }
                else
                {
                    if (d >= 5 && c >= 5 && hp >= 18 && dur) return cue;              // 3♥ = 5♦5♣ 18+ stoperli
                    if (d >= 5 && mUz >= 4 && hp >= 18) return maj3;                  // 3♠ = 5♦ 4♠ 18+
                }
                if (d >= 6 && hp >= 18 && dur) return "3NT";
                if (cevap == "3♣" && d >= 6 && hp >= 15 && hp <= 17) return "3♦";    // 6+♦ davet (3♣ tercihinden düzeltme)
                return "Pas";                                                         // kötü el: tercih edilen minörde oynanır
            }

            // ── Cevapçının kararı (ortak 2NT dedi) ──────────────────────────────────────
            string Mc, Nc; bool s2c;
            if (CevapciBagla(h, out Mc, out Nc, out s2c))
            {
                int hp = z.Hp, mUz = z.Uz(Mc);
                if (!s2c)
                {
                    if (hp >= 12) return YK.T(3, Nc);                                 // 3♥ = cue-bid, oyun zorlayan
                    if (mUz >= 6 && hp >= 8) return YK.T(3, Mc);                      // 3♠ = 6+♠ davet
                }
                else
                {
                    if (hp >= 12) return YK.T(3, Nc);                                 // 3♠ = cue-bid, oyun zorlayan
                    if (mUz >= 6 && hp <= 8) return YK.T(3, Mc);                      // 3♥ = 6+♥ 5-8 HP
                }
                return (z.Karo >= z.Sinek && hp <= 8) ? "3♦" : "3♣";                  // tercih (min.)
            }
            return null;
        }

        /// <summary>Açan gözüyle: ix=1 → 2NT öncesi, ix=2 → 2NT + ortağın 3♣/3♦ cevabı sonrası. Değilse 0.</summary>
        private static int AcanBagla(List<string> h, out string M, out string N, out bool s2)
        {
            M = N = null; s2 = false;
            // S1: B:1♦ O:1M R:2N [B:2NT O:3x]
            if (h.Count >= 3 && h[0] == "B:1♦" && h[1].StartsWith("O:") && h[2].StartsWith("R:"))
            {
                string m = YK.KozOf(YK.Cagri(h[1])), n = YK.KozOf(YK.Cagri(h[2]));
                if (YK.Sev(YK.Cagri(h[1])) == 1 && Mj(m) && YK.Sev(YK.Cagri(h[2])) == 2 && Mj(n) && m != n)
                { M = m; N = n; return Son(h, 3); }
            }
            // S2: B:1♦ L:1M O:Dbl R:2M
            if (h.Count >= 4 && h[0] == "B:1♦" && h[1].StartsWith("L:") && h[2] == "O:Dbl" && h[3].StartsWith("R:"))
            {
                string ov = YK.KozOf(YK.Cagri(h[1])), n = YK.KozOf(YK.Cagri(h[3]));
                if (YK.Sev(YK.Cagri(h[1])) == 1 && Mj(ov) && YK.Sev(YK.Cagri(h[3])) == 2 && n == ov)
                { N = ov; M = ov == "Maça" ? "Kupa" : "Maça"; s2 = true; return Son(h, 4); }
            }
            return 0;
        }

        private static int Son(List<string> h, int n)
        {
            if (h.Count == n) return 1;
            if (h.Count == n + 2 && h[n] == "B:2NT" && (h[n + 1] == "O:3♣" || h[n + 1] == "O:3♦")) return 2;
            return 0;
        }

        /// <summary>Cevapçı gözüyle: O:1♦ … O:2NT (son çağrı).</summary>
        private static bool CevapciBagla(List<string> h, out string M, out string N, out bool s2)
        {
            M = N = null; s2 = false;
            // S1: O:1♦ B:1M L:2N O:2NT
            if (h.Count == 4 && h[0] == "O:1♦" && h[1].StartsWith("B:") && h[2].StartsWith("L:") && h[3] == "O:2NT")
            {
                string m = YK.KozOf(YK.Cagri(h[1])), n = YK.KozOf(YK.Cagri(h[2]));
                if (YK.Sev(YK.Cagri(h[1])) == 1 && Mj(m) && YK.Sev(YK.Cagri(h[2])) == 2 && Mj(n) && m != n) { M = m; N = n; return true; }
            }
            // S2: O:1♦ R:1M B:Dbl L:2M O:2NT
            if (h.Count == 5 && h[0] == "O:1♦" && h[1].StartsWith("R:") && h[2] == "B:Dbl" && h[3].StartsWith("L:") && h[4] == "O:2NT")
            {
                string ov = YK.KozOf(YK.Cagri(h[1])), n = YK.KozOf(YK.Cagri(h[3]));
                if (YK.Sev(YK.Cagri(h[1])) == 1 && Mj(ov) && YK.Sev(YK.Cagri(h[3])) == 2 && n == ov)
                { N = ov; M = ov == "Maça" ? "Kupa" : "Maça"; s2 = true; return true; }
            }
            return false;
        }
    }

    /// <summary>
    /// Inverted DBL (Sasson kitabı, s. 7): açan 1x açtı, rakip kontra etti, ortak Rdbl dedi (oyun zorlayan durum) ve sağ rakip y rengini söyledi.
    /// Açanın cevabı: Dbl = y renginde 2-3 kart; Pas = y'de 4+ kart (ceza) ya da tekil + iyi el (14+ HP); Bid (min.) = yeni renk gösterirse y tekil/yok
    /// (kendi rengini tekrarlıyorsa tekil şart değil) — Bid kısmı normal ihale mantığına bırakılır.
    /// </summary>
    internal sealed class InvertedDbl : YeniKonvTaban
    {
        public override string Ad { get { return "Inverted DBL"; } }
        public override string Aciklama { get { return "1x–(Dbl)–Rdbl–(y): Dbl = y'de 2-3 kart, Pas = y'de 4+ (ceza) ya da tekil+iyi el, Bid = y tekil/yok"; } }

        private static bool Rd(string c) { return c != null && (c.Equals("Rdbl", StringComparison.OrdinalIgnoreCase)); }

        protected override string Karar(Dz z)
        {
            var h = z.Hepsi;
            if (h.Count != 4 || !h[0].StartsWith("B:") || h[1] != "L:Dbl" || !Rd(YK.Cagri(h[2])) || !h[2].StartsWith("O:") || !h[3].StartsWith("R:")) return null;
            string ac = YK.Cagri(h[0]);
            if (YK.Sev(ac) != 1 || YK.KozOf(ac) == "NT") return null;
            string y = YK.KozOf(YK.Cagri(h[3]));
            if (y == null || y == "NT" || YK.Sev(YK.Cagri(h[3])) > 3) return null;
            if (y == YK.KozOf(ac)) return null;                       // rakip bizim rengimizi söylemez; güvenlik
            int n = z.Uz(y);
            if (n >= 4) return "Pas";
            if (n == 1 && z.Hp >= 14) return "Pas";
            if (n == 2 || n == 3) return "Dbl";
            return null;                                              // y'de tekil/yok (min.): normal ihale mantığı teklif eder
        }
    }

    /// <summary>
    /// Mixed Raise (Sasson kitabı, s. 97-101): rakip 1x açtı, ortak 1M (majör) ile araya girdi, sağ rakip Pas.
    /// Atlamalı cue-bid (3x) = 4+ M desteği ve 10-12 HP.
    /// </summary>
    internal sealed class MixedRaise : YeniKonvTaban
    {
        public override string Ad { get { return "Mixed Raise (Jump Cue-Bid)"; } }
        public override string Aciklama { get { return "(1x)–1M–(Pas)–3x: 4+ destekli mixed raise (10-12 HP)"; } }

        protected override string Karar(Dz z)
        {
            var h = z.Hepsi;
            string x, M;
            if (Arayagirildi(h, out x, out M, false))     // ortağın araya girişine cevap
            {
                if (z.Uz(M) < 4 || z.Hp < 10 || z.Hp > 12) return null;
                var an = z.D.Anlasma;
                if (an != null && an.FSJS && FSJS.YanRenk(z, M, x) != null) return null;   // FSJS önceliklidir
                return YK.T(3, x);
            }
            if (Arayagirildi(h, out x, out M, true))      // araya girenin cevabı: [R:1x B:1M O:3x]
            {
                if (YK.Cagri(h[2]) != YK.T(3, x)) return null;
                return z.Hp >= 13 ? YK.T(4, M) : YK.T(3, M);
            }
            return null;
        }

        /// <summary>false: [L:1x O:1M] (cevapçı), true: [R:1x B:1M O:…] (araya giren). x = rakip açılışı, M = ortağın/benim majörüm.</summary>
        internal static bool Arayagirildi(List<string> h, out string x, out string M, bool aracinGozuyle)
        {
            x = M = null;
            if (!aracinGozuyle)
            {
                if (h.Count != 2 || !h[0].StartsWith("L:") || !h[1].StartsWith("O:")) return false;
                return Coz(YK.Cagri(h[0]), YK.Cagri(h[1]), out x, out M);
            }
            if (h.Count != 3 || !h[0].StartsWith("R:") || !h[1].StartsWith("B:") || !h[2].StartsWith("O:")) return false;
            return Coz(YK.Cagri(h[0]), YK.Cagri(h[1]), out x, out M);
        }

        private static bool Coz(string acilis, string mud, out string x, out string M)
        {
            x = M = null;
            if (YK.Sev(acilis) != 1 || YK.Sev(mud) != 1) return false;
            string a = YK.KozOf(acilis), m = YK.KozOf(mud);
            if (a == null || a == "NT" || m == null || !YK.Major(m) || a == m) return false;
            if (YK2.Sira(m) <= YK2.Sira(a)) return false;
            x = a; M = m; return true;
        }
    }

    /// <summary>
    /// FSJS — Fit-Showing Jump Shift (Sasson kitabı, s. 97-101): rakip 1x açtı, ortak 1M ile araya girdi, sağ rakip Pas.
    /// Atlamalı yeni renk = o renkte 5+ kart, ortağın majöründe 4+ destek (10+ HP). Araya giren: 14+ HP ile 4M, aksi hâlde 3M.
    /// </summary>
    internal sealed class FSJS : YeniKonvTaban
    {
        public override string Ad { get { return "FSJS"; } }
        public override string Aciklama { get { return "(1x)–1M–(Pas)–atlamalı yeni renk: 5+ o renk, 4+ M desteği (10+ HP)"; } }

        /// <summary>Elde FSJS yapılabilecek renk (5+ kart, en az 3 HP'lik, açılış rengi ve M dışı); yoksa null.</summary>
        internal static string YanRenk(Dz z, string M, string x)
        {
            if (z.Uz(M) < 4 || z.Hp < 10) return null;
            string en = null; int enUz = 0;
            foreach (var r in YK.Renk)
            {
                if (r == M || r == x) continue;
                int u = z.Uz(r);
                if (u >= 5 && YK.Hcp(z.El, r) >= 3 && u > enUz) { en = r; enUz = u; }
            }
            return en;
        }

        protected override string Karar(Dz z)
        {
            var h = z.Hepsi; string x, M;
            if (MixedRaise.Arayagirildi(h, out x, out M, false))
            {
                string s = YanRenk(z, M, x);
                if (s == null) return null;
                // atlamalı: M'den yüksek renk 1M üstünde 2 seviyesi, düşük renk 3 seviyesi
                int sev = YK2.Sira(s) > YK2.Sira(M) ? 2 : 3;
                return YK.T(sev, s);
            }
            if (MixedRaise.Arayagirildi(h, out x, out M, true))
            {
                string c = YK.Cagri(h[2]); string s = YK.KozOf(c);
                if (s == null || s == "NT" || s == x || s == M) return null;
                int sev = YK2.Sira(s) > YK2.Sira(M) ? 2 : 3;
                if (YK.Sev(c) != sev) return null;                       // atlamalı değil: FSJS değil
                return z.Hp >= 14 ? YK.T(4, M) : YK.T(3, M);
            }
            return null;
        }
    }
}
