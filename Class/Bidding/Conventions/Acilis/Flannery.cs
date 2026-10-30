using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Flannery 2♦: tam 4 maça + 5 kupa, 11-15 HP. Cevaplar:
    ///   zayıf (0-9): 2♠ (5+♠), 2♥, ya da ♦'da pas; davet (10-11): 3♥ / 3♠; oyun (12-15): 4♥ / 4♠ / 3NT; 16+: 2NT (zorlayıcı soru).
    /// Açıcının 2NT sonrası cevabı (şekil): 3♣ = 4-5-1-3, 3♦ = 4-5-3-1, 3♥ = 4-5-2-2 minimum (11-13),
    /// 3♠ = 4-5-2-2 maksimum (14-15) zayıf minör ikililer, 3NT = 4-5-2-2 maksimum, iyi ikililer (minörlerde 3+ HP),
    /// 4♣ = 4-5-0-4, 4♦ = 4-5-4-0.
    /// Zayıf İki, Multi 2♦ ve Mini-Roman 2♦ ile birlikte kullanılamaz (2♦ açılışını paylaşırlar).
    /// </summary>
    internal sealed class Flannery : YeniKonvTaban
    {
        public override string Ad { get { return "Flannery"; } }
        public override string Aciklama { get { return "2♦ açılışı: 4♠ 5♥, 11-15 HP; 2NT zorlayıcı, açıcı şekil gösterir"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 0)
                return z.Maca == 4 && z.Kupa == 5 && z.Hp >= 11 && z.Hp <= 15 ? "2♦" : null;

            // Cevapçı
            if (z.Es("O:2♦"))
            {
                if (z.Hp >= 16) return "2NT";
                if (z.Hp >= 12)
                {
                    if (z.Kupa >= 3) return "4♥";
                    if (z.Maca >= 4) return "4♠";
                    return "3NT";
                }
                if (z.Hp >= 10)
                {
                    if (z.Kupa >= 3) return "3♥";
                    if (z.Maca >= 4) return "3♠";
                }
                if (z.Maca >= 5) return "2♠";
                if (z.Kupa >= 2) return "2♥";
                return z.Karo >= 3 ? "Pas" : "2♥";
            }

            // Açıcı
            if (z.Es("B:2♦", "O:2NT"))
            {
                int d = z.Karo, c = z.Sinek;
                if (d == 1 && c == 3) return "3♣";
                if (d == 3 && c == 1) return "3♦";
                if (d == 0 && c == 4) return "4♣";
                if (d == 4 && c == 0) return "4♦";
                if (d == 2 && c == 2)
                {
                    if (z.Hp <= 13) return "3♥";
                    int minorHp = YK.Hcp(z.El, "Karo") + YK.Hcp(z.El, "Sinek");
                    return minorHp >= 3 ? "3NT" : "3♠";
                }
                return null;
            }
            if (z.Es("B:2♦", "O:3♥")) return z.Hp >= 13 ? "4♥" : "Pas";
            if (z.Es("B:2♦", "O:3♠")) return z.Hp >= 13 ? "4♠" : "Pas";
            return null;
        }
    }
}
