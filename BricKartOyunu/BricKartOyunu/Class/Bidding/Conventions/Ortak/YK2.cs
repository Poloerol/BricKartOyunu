using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    internal static class YK2
    {
        /// <summary>Renk sırası: Sinek 0 &lt; Karo 1 &lt; Kupa 2 &lt; Maça 3.</summary>
        public static int Sira(string renk) { return Array.IndexOf(new[] { "Sinek", "Karo", "Kupa", "Maça" }, renk); }

        /// <summary>"O:1♠" gibi etiketli çağrı, verilen kimde ve 1 seviyesinde bir renk açılışı mı? Renk adını döndürür (yoksa null).</summary>
        public static string BirRenk(string etiketli, string kim)
        {
            if (etiketli == null || !etiketli.StartsWith(kim + ":")) return null;
            string t = YK.Cagri(etiketli);
            if (YK.Sev(t) != 1) return null;
            string k = YK.KozOf(t);
            return k == "NT" ? null : k;
        }

        /// <summary>İki çağrı arasında 'sonraki' çağrı ('t' seviye+renk) önceki çağrıdan yüksek mi?</summary>
        public static bool Yuksek(string yeni, string onceki)
        {
            int ys = YK.Sev(yeni), os = YK.Sev(onceki);
            if (ys != os) return ys > os;
            string yk = YK.KozOf(yeni), ok = YK.KozOf(onceki);
            int yr = yk == "NT" ? 4 : Sira(yk), orr = ok == "NT" ? 4 : Sira(ok);
            return yr > orr;
        }
    }
}
