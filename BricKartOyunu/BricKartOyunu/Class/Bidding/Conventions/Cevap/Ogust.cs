using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Ogust: zayıf iki açılışına 2NT = soru; cevap 3♣ min-zayıf renk, 3♦ min-iyi renk, 3♥ maks-zayıf, 3♠ maks-iyi, 3NT = AKQ'lu.</summary>
    internal sealed class Ogust : YeniKonvTaban
    {
        public override string Ad { get { return "Ogust"; } }
        public override string Aciklama { get { return "Zayıf iki + 2NT: 3♣ min/zayıf, 3♦ min/iyi, 3♥ maks/zayıf, 3♠ maks/iyi, 3NT sağlam"; } }

        private static string ZayifIkiRengi(string t)
        {
            if (t == "2♦") return "Karo"; if (t == "2♥") return "Kupa"; if (t == "2♠") return "Maça"; return null;
        }

        private static bool Benjamin2D(Dz z, string x) { return x == "Karo" && z.D.Anlasma != null && z.D.Anlasma.BenjaminTwoBids; }

        protected override string Karar(Dz z)
        {
            // Soru: ortak 2x açtı, ben güçlüyüm
            if (z.Hepsi.Count == 1 && z.Hepsi[0].StartsWith("O:"))
            {
                string x = ZayifIkiRengi(YK.Cagri(z.Hepsi[0]));
                if (x == null || Benjamin2D(z, x)) return null;
                return (z.Hp >= 15 || (z.Hp >= 14 && z.Uz(x) >= 3)) ? "2NT" : null;
            }
            // Cevap: ben 2x açtım, ortak 2NT
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[1] == "O:2NT")
            {
                string x = ZayifIkiRengi(YK.Cagri(z.Hepsi[0]));
                if (x == null || Benjamin2D(z, x)) return null;
                int ust = YK.ÜstHonor(z.El, x);
                bool maks = z.Hp >= 8 || (z.Hp >= 7 && ust >= 2);
                if (YK.AKQ(z.El, x)) return "3NT";
                bool iyi = ust >= 2;
                if (!maks) return iyi ? "3♦" : "3♣";
                return iyi ? "3♠" : "3♥";
            }
            // Cevaptan sonra karar (soran)
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1] == "B:2NT" && z.Hepsi[2].StartsWith("O:"))
            {
                string x = ZayifIkiRengi(YK.Cagri(z.Hepsi[0]));
                string c = YK.Cagri(z.Hepsi[2]);
                if (x == null || Benjamin2D(z, x)) return null;
                int acici = c == "3♣" ? 6 : c == "3♦" ? 7 : c == "3♥" ? 9 : c == "3♠" ? 10 : c == "3NT" ? 10 : 0;
                if (acici == 0) return null;
                int toplam = z.Hp + acici;
                bool major = YK.Major(x);
                if (c == "3NT") return major ? YK.T(4, x) : "Pas";
                if (major && toplam >= 25) return YK.T(4, x);
                if (!major && toplam >= 27) return "3NT";
                // Oyun yok: x rengini 3 seviyesinde bırak (cevaptan yüksekse), değilse geç
                string sign = YK.T(3, x);
                return YK2.Yuksek(sign, c) ? sign : "Pas";
            }
            return null;
        }
    }
}
