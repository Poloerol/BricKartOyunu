using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// İhale çağrılarını kart oyunu yapay zekâsı için yorumlar: bir çağrı doğal mı (söylenen renk gerçekten gösterilir),
    /// yoksa yapay mı (konvansiyon) ve yapaysa gerçekte hangi renkleri gösterir?
    /// Yalnızca çağrıyı yapan tarafın ortaklık anlaşmasında AÇIK olan ek konvansiyonlar yorumlanır; böylece
    /// "Stayman 2♣ = ortağın rengi sinek" gibi yanlış çıkarımlarla atak/oyun planı bozulmaz.
    /// </summary>
    public static class IhaleYorumcusu
    {
        private static readonly Player[] Cw = { Player.Guney, Player.Bati, Player.Kuzey, Player.Dogu };       // saat yönü
        private static readonly Player[] IhaleSirasi = { Player.Bati, Player.Kuzey, Player.Dogu, Player.Guney };  // OyunBaglami.IhaleBaslangici sırası

        private const string S = "Maça", H = "Kupa", D = "Karo", C = "Sinek";
        private static readonly string[] Yok = new string[0];

        /// <summary>
        /// i. çağrının yorumu. null = doğal (söylenen renk gösterilir); aksi hâlde gerçekte gösterilen renkler (boş olabilir).
        /// </summary>
        public static string[] Yorumla(List<string> ihale, string baslangic, int i, Func<Player, OrtaklikAnlasmasi> anlasmaOf)
        {
            if (ihale == null || i < 0 || i >= ihale.Count || anlasmaOf == null) return null;
            Player bas;
            if (!Enum.TryParse(baslangic, out bas)) return null;
            int b0 = Array.IndexOf(IhaleSirasi, bas);
            if (b0 < 0) return null;
            Player koltuk = IhaleSirasi[(b0 + i) % 4];
            string teklif = ihale[i];
            if (string.IsNullOrEmpty(teklif) || teklif == "Pas") return null;
            var an = anlasmaOf(koltuk);
            if (an == null) return null;

            // Çağrı dizisini bu çağrıyı yapanın gözüyle (B/L/O/R) kur; Pas'lar yazılmaz
            int ben = Array.IndexOf(Cw, koltuk);
            var h = new List<string>();
            for (int j = 0; j <= i; j++)
            {
                if (string.IsNullOrEmpty(ihale[j]) || ihale[j] == "Pas") continue;
                Player k = IhaleSirasi[(b0 + j) % 4];
                int rel = (Array.IndexOf(Cw, k) - ben + 4) % 4;
                h.Add("BLOR"[rel] + ":" + ihale[j]);
            }
            return Kurallar(h, an);
        }

        private static bool Es(List<string> h, params string[] s) { return h.SequenceEqual(s); }

        private static string Birinci(string etiket, string kim)
        {
            if (!etiket.StartsWith(kim + ":")) return null;
            string t = etiket.Substring(2);
            if (t.Length < 2 || t[0] != '1') return null;
            switch (t.Substring(1))
            {
                case "♠": return S; case "♥": return H; case "♦": return D; case "♣": return C;
            }
            return null;
        }

        private static string RenkAdi(string sembol)
        {
            switch (sembol) { case "♠": return S; case "♥": return H; case "♦": return D; case "♣": return C; }
            return null;
        }
        private static string Sembol(string renk)
        {
            switch (renk) { case S: return "♠"; case H: return "♥"; case D: return "♦"; case C: return "♣"; }
            return "";
        }
        private static int Sira(string renk) { return Array.IndexOf(new[] { C, D, H, S }, renk); }

        private static string[] Kurallar(List<string> h, OrtaklikAnlasmasi an)
        {
            if (h.Count == 0) return null;
            string son = h[h.Count - 1];
            string cag = son.Substring(2);

            // ── 1NT çevresi
            if (an.ExtendedStayman && (Es(h, "O:1NT", "B:2♣") || Es(h, "O:1NT", "B:2♣", "O:2♦", "B:2♥"))) return new[] { S, H };
            if (an.HelloOver1NT)
            {
                if (Es(h, "R:1NT", "B:2♣") || Es(h, "L:1NT", "B:2♣") || Es(h, "R:1NT", "B:2♦") || Es(h, "L:1NT", "B:2♦")) return new[] { S, H };
                if (Es(h, "R:1NT", "B:2NT") || Es(h, "L:1NT", "B:2NT")) return new[] { D, C };
                if (Es(h, "L:1NT", "O:2♦", "B:2♥")) return Yok;
            }
            if (an.Sandwich1NT && h.Count == 3 && cag == "1NT" && son.StartsWith("B:"))
            {
                string x = Birinci(h[0], "L"), y = Birinci(h[1], "R");
                if (x != null && y != null && x != y) return new[] { S, H, D, C }.Where(r => r != x && r != y).ToArray();
            }
            if (an.Robinson)
            {
                if (Es(h, "O:1NT", "R:Dbl", "B:2♦")) return new[] { H };
                if (Es(h, "O:1NT", "R:Dbl", "B:2♥")) return new[] { S };
                if (Es(h, "O:1NT", "R:Dbl", "B:RDbl")) return new[] { C };
                if (Es(h, "O:1NT", "R:Dbl", "B:2♣")) return Yok;
                if (Es(h, "B:1NT", "L:Dbl", "O:2♦", "B:2♥") || Es(h, "B:1NT", "L:Dbl", "O:2♥", "B:2♠") || Es(h, "B:1NT", "L:Dbl", "O:RDbl", "B:2♣")) return Yok;
            }
            if (an.ExitTransfer && !an.Robinson)
            {
                if (Es(h, "O:1NT", "R:Dbl", "B:2♣")) return new[] { D };
                if (Es(h, "O:1NT", "R:Dbl", "B:2♦")) return new[] { H };
                if (Es(h, "O:1NT", "R:Dbl", "B:2♥")) return new[] { S };
                if (Es(h, "O:1NT", "R:Dbl", "B:2♠")) return new[] { C };
                if (Es(h, "B:1NT", "L:Dbl", "O:2♣", "B:2♦") || Es(h, "B:1NT", "L:Dbl", "O:2♦", "B:2♥") ||
                    Es(h, "B:1NT", "L:Dbl", "O:2♥", "B:2♠") || Es(h, "B:1NT", "L:Dbl", "O:2♠", "B:3♣")) return Yok;
            }
            if (an.Gambling3NT && Es(h, "O:3NT", "B:4♣")) return Yok;

            // ── Açılış ve cevaplar
            if (an.BenjaminTwoBids && (Es(h, "B:2♣") || Es(h, "B:2♦") || Es(h, "O:2♣", "B:2♦") || Es(h, "O:2♦", "B:2♥"))) return Yok;
            if (an.MiniRoman2D && Es(h, "B:2♦")) return new[] { S, H, C };
            if (an.Ogust && h.Count == 3 && h[0].StartsWith("B:2") && h[1] == "O:2NT" && son.StartsWith("B:3") && !cag.EndsWith("NT"))
            {
                string x = RenkAdi(h[0].Substring(3));
                if (x != null) return new[] { x };
            }
            if (an.InvertedBergenRaises && h.Count == 2 && (cag == "3♣" || cag == "3♦") && son.StartsWith("B:"))
            {
                string m = Birinci(h[0], "O");
                if (m == S || m == H) return new[] { m };
            }
            if (an.German2CRelays)
            {
                if (Es(h, "O:2♣", "B:2♦", "O:2NT", "B:3♣")) return Yok;
                if (Es(h, "O:2♣", "B:2♦", "O:2NT", "B:3♦")) return new[] { H };
                if (Es(h, "O:2♣", "B:2♦", "O:2NT", "B:3♥")) return new[] { S };
                if (Es(h, "B:2♣", "O:2♦", "B:2NT", "O:3♦", "B:3♥") || Es(h, "B:2♣", "O:2♦", "B:2NT", "O:3♥", "B:3♠")) return Yok;
            }
            if (an.NewMinorForcing && h.Count == 4 && son.StartsWith("B:") && h[3] == "B:" + (Birinci(h[0], "O") == C ? "2♦" : "2♣") &&
                Birinci(h[0], "O") != null && Birinci(h[1], "B") != null && h[2] == "O:1NT") return Yok;
            if (an.Artificial2C2D && h.Count == 3 && h[1] == "R:Dbl" && (cag == "2♣" || cag == "2♦") && son.StartsWith("B:"))
            {
                string m = Birinci(h[0], "O");
                if (m == S || m == H) return new[] { m };
            }
            if (an.WolffSignoff && h.Count == 4 && h[2] == "O:1NT" && h[3] == "B:2♣" && Birinci(h[0], "O") != null && Birinci(h[1], "B") != null) return Yok;
            if (an.LongShortGameTries && h.Count == 3 && h[0].StartsWith("B:1") && son.StartsWith("B:3"))
            {
                string m = Birinci(h[0], "B");
                if ((m == S || m == H) && h[1] == "O:" + (m == S ? "2♠" : "2♥")) return Yok;
            }

            // ── Rekabet
            if (an.Ghestem && h.Count == 2 && h[0].StartsWith("R:1") && son.StartsWith("B:"))
            {
                string x = Birinci(h[0], "R");
                if (x != null)
                {
                    var u = new[] { C, D, H, S }.Where(r => r != x).ToList();
                    if (cag == "2" + Sembol(x)) return new[] { u[1], u[2] };
                    if (cag == "2NT") return new[] { u[0], u[1] };
                    if (cag == "3♣") return new[] { u[0], u[2] };
                }
            }
            if (an.InvisibleCueBids && h.Count == 3 && son.StartsWith("B:"))
            {
                string m = Birinci(h[0], "O"), M = Birinci(h[1], "R");
                if ((m == C || m == D) && (M == S || M == H) && cag == "2" + Sembol(M)) return new[] { m };
            }
            if (an.LimitPlusCueBids && h.Count == 3 && son.StartsWith("B:"))
            {
                string m = Birinci(h[0], "O");
                string c = h[1].StartsWith("R:") && h[1].Length >= 4 ? h[1].Substring(2) : null;
                if ((m == S || m == H) && c != null && c.Length == 2 && char.IsDigit(c[0]) && c[0] <= '2')
                {
                    string x = RenkAdi(c.Substring(1));
                    if (x != null && x != m && cag == (char)(c[0] + 1) + Sembol(x)) return new[] { m };
                }
            }

            // ── Slam ve kontrol soruları
            if (an.MatheAskingBids)
            {
                if (Es(h, "B:1♥", "O:3♥", "B:3♠") || Es(h, "B:1♠", "O:3♠", "B:3NT")) return new[] { h[0] == "B:1♥" ? H : S };
                if (Es(h, "O:1♥", "B:3♥", "O:3♠", son) && son.StartsWith("B:") && cag != "4♥") return Yok;
                if (Es(h, "O:1♠", "B:3♠", "O:3NT", son) && son.StartsWith("B:") && cag != "4♠") return Yok;
            }
            if (an.ControlAskingBids)
            {
                if ((Es(h, "B:1♥", "O:3♥", "B:4♣")) || Es(h, "B:1♠", "O:3♠", "B:4♣") || Es(h, "B:1♠", "O:3♠", "B:4♦"))
                    return new[] { h[0] == "B:1♥" ? H : S };
                if (h.Count == 4 && son.StartsWith("B:") && h[0].StartsWith("O:1") && (h[1] == "B:3♥" || h[1] == "B:3♠") &&
                    (h[2] == "O:4♣" || h[2] == "O:4♦") && cag != (h[0] == "O:1♥" ? "4♥" : "4♠")) return Yok;
            }
            if (an.SpecificKingAsks && h.Count >= 3)
            {
                int n = h.Count;
                if (son.StartsWith("B:5") && h[n - 3] == "B:4NT" && h[n - 2].StartsWith("O:5") && !cag.EndsWith("NT")) return Yok;
            }
            return null;
        }
    }
}
