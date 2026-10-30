using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>New Minor Forcing: 1x–1y–1NT sonrası yeni minör (2♣, açılış ♣ ise 2♦) = 11+ HP, y desteği / dörtlü majör sorusu.</summary>
    internal sealed class NewMinorForcing : YeniKonvTaban
    {
        public override string Ad { get { return "New Minor Forcing"; } }
        public override string Aciklama { get { return "1x–1y–1NT sonrası yeni minör: oyun daveti+, y'de 3'lü destek / diğer majörde 4'lü sorusu"; } }

        private static string Yeni(string acilis) { return acilis == "Sinek" ? "2♦" : "2♣"; }

        protected override string Karar(Dz z)
        {
            // Cevaplayan: ortak x açtı, ben y dedim, ortak 1NT
            if (z.Hepsi.Count == 3 && z.Hepsi[2] == "O:1NT")
            {
                string x = YK2.BirRenk(z.Hepsi[0], "O"), y = YK2.BirRenk(z.Hepsi[1], "B");
                if (x == null || y == null || !YK.Major(y) || x == y || z.Hp < 11) return null;
                return Yeni(x);
            }
            // Açıcı cevabı
            if (z.Hepsi.Count == 4 && z.Hepsi[2] == "B:1NT")
            {
                string x = YK2.BirRenk(z.Hepsi[0], "B"), y = YK2.BirRenk(z.Hepsi[1], "O");
                if (x == null || y == null || !YK.Major(y) || x == y) return null;
                if (YK.Cagri(z.Hepsi[3]) != Yeni(x)) return null;
                if (z.Uz(y) >= 3) return YK.T(2, y);
                string diger = y == "Kupa" ? "Maça" : "Kupa";
                if (z.Uz(diger) >= 4 && YK2.Yuksek(YK.T(2, diger), Yeni(x))) return YK.T(2, diger);
                return "2NT";
            }
            // Cevaplayanın devamı
            if (z.Hepsi.Count == 5 && z.Hepsi[2] == "O:1NT" && z.Hepsi[3].StartsWith("B:"))
            {
                string x = YK2.BirRenk(z.Hepsi[0], "O"), y = YK2.BirRenk(z.Hepsi[1], "B");
                if (x == null || y == null || !YK.Major(y) || YK.Cagri(z.Hepsi[3]) != Yeni(x)) return null;
                string c = YK.Cagri(z.Hepsi[4]);
                if (c == YK.T(2, y)) return z.Hp >= 13 ? YK.T(4, y) : YK.T(3, y);
                string diger = y == "Kupa" ? "Maça" : "Kupa";
                if (c == YK.T(2, diger)) return z.Uz(diger) >= 4 ? YK.T(4, diger) : "3NT";
                if (c == "2NT") return z.Hp >= 13 ? "3NT" : "Pas";
            }
            return null;
        }
    }
}
