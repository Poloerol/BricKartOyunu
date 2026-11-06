using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class
{
    /// <summary>İhale çağrı listesinden (Pas, 1♣ ... 7NT, Dbl, RDbl) nihai kontratı çıkarır.</summary>
    public sealed class IhaleSonucu
    {
        public bool Pas;
        public int Seviye;
        public int Koz;               // 0=♠ 1=♥ 2=♦ 3=♣ 4=NT
        public int Deklaran;          // koltuk 0=N 1=E 2=S 3=W
        public KontrDurumu Kontr;

        private static int KozNo(string teklif)
        {
            if (teklif.EndsWith("NT")) return 4;
            switch (teklif[teklif.Length - 1])
            {
                case '♠': return 0;
                case '♥': return 1;
                case '♦': return 2;
                case '♣': return 3;
            }
            return -1;
        }

        /// <param name="teklifler">Dağıtıcıdan başlayarak tüm çağrılar.</param>
        /// <param name="dagiticiKoltuk">0=N 1=E 2=S 3=W</param>
        public static IhaleSonucu Coz(IList<string> teklifler, int dagiticiKoltuk)
        {
            var s = new IhaleSonucu { Pas = true };
            int son = -1;
            for (int i = 0; i < teklifler.Count; i++)
            {
                string t = teklifler[i];
                if (!string.IsNullOrEmpty(t) && char.IsDigit(t[0])) son = i;
            }
            if (son < 0) return s;
            string b = teklifler[son];
            s.Pas = false; s.Seviye = b[0] - '0'; s.Koz = KozNo(b);
            int taraf = (dagiticiKoltuk + son) & 1;
            for (int i = 0; i <= son; i++)
            {
                string t = teklifler[i];
                if (string.IsNullOrEmpty(t) || !char.IsDigit(t[0]) || KozNo(t) != s.Koz) continue;
                if (((dagiticiKoltuk + i) & 1) != taraf) continue;
                s.Deklaran = (dagiticiKoltuk + i) & 3;
                break;
            }
            for (int i = son + 1; i < teklifler.Count; i++)
            {
                if (teklifler[i] == "Dbl") s.Kontr = KontrDurumu.Kontr;
                else if (teklifler[i] == "RDbl") s.Kontr = KontrDurumu.SKontr;
            }
            return s;
        }
    }

    /// <summary>Bord numarasından zon: bord 1-16 döngüsü (standart).</summary>
    public static class DDZon
    {
        public static void Hesapla(int bordNo, out bool ns, out bool ew)
        {
            int m = ((bordNo - 1) % 16) + 1;
            ns = (m == 2 || m == 5 || m == 12 || m == 15 || m == 4 || m == 7 || m == 10 || m == 16);
            ew = (m == 3 || m == 6 || m == 9 || m == 13 || m == 4 || m == 7 || m == 10 || m == 16);
        }
    }
}
