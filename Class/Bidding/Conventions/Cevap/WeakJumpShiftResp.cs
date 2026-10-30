using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Weak Jump-Shift Responses: ortağın 1x açılışına sıçramalı yeni renk (2♦/2♥/2♠) = 6+ renk, 0-6 HP; açıcı geçer.</summary>
    internal sealed class WeakJumpShiftResp : YeniKonvTaban
    {
        public override string Ad { get { return "Weak Jump-Shift Resp."; } }
        public override string Aciklama { get { return "1x–2y (sıçrama) = zayıf, 6+ y rengi"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 1)
            {
                string x = YK2.BirRenk(z.Hepsi[0], "O");
                if (x == null || z.Hp > 6) return null;
                foreach (var y in new[] { "Maça", "Kupa", "Karo" })
                    if (YK2.Sira(y) > YK2.Sira(x) && z.Uz(y) >= 6 && YK2.Sira(y) - YK2.Sira(x) >= 1)
                    {
                        // sıçrama: 1 seviyede o renk mümkündü (zaten 'yüksek'), 2 seviyeden söylenir
                        return YK.T(2, y);
                    }
                return null;
            }
            if (z.Hepsi.Count == 2)
            {
                string x = YK2.BirRenk(z.Hepsi[0], "B");
                if (x == null) return null;
                string t = YK.Cagri(z.Hepsi[1]);
                if (!z.Hepsi[1].StartsWith("O:") || YK.Sev(t) != 2) return null;
                string y = YK.KozOf(t);
                if (y == null || y == "NT" || YK2.Sira(y) <= YK2.Sira(x)) return null;
                return z.Hp >= 19 ? null : "Pas";                 // açıcı: zayıf sıçrama, geçer
            }
            return null;
        }
    }
}
