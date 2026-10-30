using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>1m–2NT Invitational: minör açılışına 2NT = 11-12 HP dengeli, 4'lü majör yok; açıcı 15+ ile 3NT, aksi hâlde geçer.</summary>
    internal sealed class OneMinorTwoNTInv : YeniKonvTaban
    {
        public override string Ad { get { return "1m-2NT Invitational"; } }
        public override string Aciklama { get { return "1♣/1♦–2NT: 11-12 HP dengeli, majör yok (davet)"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 1)
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                if (m != "Sinek" && m != "Karo") return null;
                if (z.Hp < 11 || z.Hp > 12 || !ElDegerlendirici.DengeliEl(z.El) || z.Maca >= 4 || z.Kupa >= 4) return null;
                return "2NT";
            }
            if (z.Hepsi.Count == 2 && z.Hepsi[1] == "O:2NT")
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                if (m != "Sinek" && m != "Karo") return null;
                return z.Hp >= 15 ? "3NT" : "Pas";
            }
            return null;
        }
    }
}
