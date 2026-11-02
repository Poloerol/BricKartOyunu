using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Texas transferleri: 1NT (ya da 2NT) – 4♦ = ♥ transferi, 4♥ = ♠ transferi.
    /// Cevapçı 6+ kartlık majörle oyun değerinde (10-15 HP; 2NT sonrası 4+ HP) ve şlem ilgisi olmadan kullanır.
    /// Açıcı transferi her zaman kabul eder (4♦→4♥, 4♥→4♠), böylece güçlü el gizli kalır ve açıcı deklaran olur.
    /// Cevapçı kabulden sonra pas geçer.
    /// </summary>
    internal sealed class TexasTransfers : YeniKonvTaban
    {
        public override string Ad { get { return "Texas Transfers"; } }
        public override string Aciklama { get { return "1NT/2NT–4♦ = ♥, 4♥ = ♠ transferi (6+ majör, oyun değeri, şlem ilgisi yok)"; } }
        protected override string Karar(Dz z)
        {
            foreach (string nt in new[] { "1NT", "2NT" })
            {
                int altSinir = nt == "1NT" ? 10 : 4;
                int ustSinir = nt == "1NT" ? 15 : 11;
                if (z.Es("O:" + nt))
                {
                    if (z.Hp < altSinir || z.Hp > ustSinir) return null;
                    if (z.Kupa >= 6 && z.Kupa >= z.Maca) return "4♦";
                    if (z.Maca >= 6) return "4♥";
                    return null;
                }
                if (z.Es("B:" + nt, "O:4♦")) return "4♥";
                if (z.Es("B:" + nt, "O:4♥")) return "4♠";
                if (z.Es("O:" + nt, "B:4♦", "O:4♥")) return "Pas";
                if (z.Es("O:" + nt, "B:4♥", "O:4♠")) return "Pas";
            }
            return null;
        }
    }
}
