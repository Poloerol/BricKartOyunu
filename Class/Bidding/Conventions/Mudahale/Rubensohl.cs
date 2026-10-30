using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Rubensohl (transfer Lebensohl) — rakibin 1NT'sine 2♦ müdahalesi karşısında (ortak 1NT açtı, sağ rakip 2♦ dedi):
    ///   Kontra: ceza (bu sınıf vermez) · 2♥ / 2♠: doğal, zayıf (5+ kart)
    ///   2NT: ♣ transferi (3♣) · 3♣: ♦ transferi = Stayman (açıcı 4'lü majörünü söyler, yoksa 3♦)
    ///   3♦: ♥ transferi, 3♥: ♠ transferi (en az davet, 10+ HP) · 3♠: 3NT transferi (oyun değeri, majör yok, ♦ durdurucusu yok)
    ///   3NT: oynamak için (♦ durdurucusuyla).
    /// Açıcı her transferi tamamlar. Yalnızca 2♦ müdahalesi desteklenir; Lebensohl ile birlikte kullanılamaz.
    /// </summary>
    internal sealed class Rubensohl : YeniKonvTaban
    {
        public override string Ad { get { return "Rubensohl"; } }
        public override string Aciklama { get { return "1NT–(2♦): 2NT=♣, 3♣=Stayman, 3♦=♥, 3♥=♠, 3♠=3NT transferi"; } }
        protected override string Karar(Dz z)
        {
            // Cevapçı
            if (z.Es("O:1NT", "R:2♦"))
            {
                bool durdurucu = ElDegerlendirici.StopperVar(z.El, "Karo");
                bool majorVar = z.Maca >= 4 || z.Kupa >= 4;
                if (z.Hp < 10)
                {
                    if (z.Kupa >= 5 && z.Kupa >= z.Maca) return "2♥";
                    if (z.Maca >= 5) return "2♠";
                    if (z.Sinek >= 6) return "2NT";
                    return null;
                }
                if (z.Kupa >= 5 && z.Kupa >= z.Maca) return "3♦";
                if (z.Maca >= 5) return "3♥";
                if (majorVar) return "3♣";
                return durdurucu ? "3NT" : "3♠";
            }
            // Açıcı: transferleri tamamlar
            if (z.Es("B:1NT", "L:2♦", "O:2NT")) return "3♣";
            if (z.Es("B:1NT", "L:2♦", "O:3♦")) return "3♥";
            if (z.Es("B:1NT", "L:2♦", "O:3♥")) return "3♠";
            if (z.Es("B:1NT", "L:2♦", "O:3♠")) return "3NT";
            if (z.Es("B:1NT", "L:2♦", "O:3♣")) return z.Kupa >= 4 ? "3♥" : z.Maca >= 4 ? "3♠" : "3♦";
            // Cevapçı: tamamlamadan sonra
            if (z.Es("O:1NT", "R:2♦", "B:2NT", "O:3♣")) return "Pas";
            if (z.Es("O:1NT", "R:2♦", "B:3♦", "O:3♥")) return z.Hp >= 12 ? "4♥" : "Pas";
            if (z.Es("O:1NT", "R:2♦", "B:3♥", "O:3♠")) return z.Hp >= 12 ? "4♠" : "Pas";
            if (z.Es("O:1NT", "R:2♦", "B:3♣", "O:3♥")) return z.Kupa >= 4 ? "4♥" : "3NT";
            if (z.Es("O:1NT", "R:2♦", "B:3♣", "O:3♠")) return z.Maca >= 4 ? "4♠" : "3NT";
            if (z.Es("O:1NT", "R:2♦", "B:3♣", "O:3♦")) return "3NT";
            return null;
        }

    }
}
