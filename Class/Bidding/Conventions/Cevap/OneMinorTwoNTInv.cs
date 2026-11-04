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
    /// <summary>Yarı zorlayıcı 1NT: 1M–1NT sonrası açıcı 5'li majörle dengeli asgari (12-14) elde Pas der.</summary>
    internal sealed class SemiForcingPas : YeniKonvTaban
    {
        public override string Ad { get { return "Semi-Forcing 1NT"; } }
        public override string Aciklama { get { return "1M–1NT (6-10 HP): açıcı 5-3-3-2 asgari (12-14) elde Pas der"; } }
        protected override string Karar(Dz z)
        {
            if (!(z.Es("B:1♠", "O:1NT") || z.Es("B:1♥", "O:1NT"))) return null;
            string m = z.Hepsi[0] == "B:1♠" ? "Maça" : "Kupa";
            if (z.Uz(m) != 5 || z.Hp > 14 || !ElDegerlendirici.DengeliEl(z.El)) return null;
            return "Pas";
        }
    }
}
