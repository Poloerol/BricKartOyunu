using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Leaping Michaels: rakibin zayıf iki / üç seviye önleyicisine 4♣ ya da 4♦ sıçraması = 5-5 iki renkli, oyun zorlayan el (15+ HP).
    ///   (2♥/3♥): 4♣ = ♣+♠, 4♦ = ♦+♠        (2♠/3♠): 4♣ = ♣+♥, 4♦ = ♦+♥
    ///   (2♦/3♦): 4♣ = ♣+bir majör, 4♦ = iki majör      (3♣): 4♣ = iki majör, 4♦ = ♦+bir majör
    /// İlerletici iki rengin uygununu seçer; "bir majör" olduğunda 4♦ ile sorar (4♦ iki majörse 4♥ geç/düzelt).
    /// </summary>
    internal sealed class LeapingMichaels : YeniKonvTaban
    {
        public override string Ad { get { return "Leaping Michaels"; } }
        public override string Aciklama { get { return "Önleyiciye 4♣/4♦ sıçraması: 5-5 iki renkli, oyun zorlayan"; } }

        private static readonly string[] Acilislar = { "2♦", "3♦", "2♥", "3♥", "2♠", "3♠", "3♣" };

        protected override string Karar(Dz z)
        {
            // Müdahaleci
            if (z.Hepsi.Count == 1 && z.Hepsi[0].StartsWith("R:") && Array.IndexOf(Acilislar, YK.Cagri(z.Hepsi[0])) >= 0)
            {
                if (z.Hp < 15) return null;
                string op = YK.Cagri(z.Hepsi[0]);
                int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
                bool majorBir = (s >= 5 && h < 5) || (h >= 5 && s < 5) || (s >= 5 && h >= 5);
                switch (op)
                {
                    case "2♥": case "3♥":
                        if (c >= 5 && s >= 5) return "4♣";
                        if (d >= 5 && s >= 5) return "4♦";
                        return null;
                    case "2♠": case "3♠":
                        if (c >= 5 && h >= 5) return "4♣";
                        if (d >= 5 && h >= 5) return "4♦";
                        return null;
                    case "2♦": case "3♦":
                        if (s >= 5 && h >= 5) return "4♦";
                        if (c >= 5 && (s >= 5 || h >= 5)) return "4♣";
                        return null;
                    case "3♣":
                        if (s >= 5 && h >= 5) return "4♣";
                        if (d >= 5 && (s >= 5 || h >= 5)) return "4♦";
                        return null;
                }
                return null;
            }

            // İlerletici (ortak sıçradı)
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("L:") && z.Hepsi[1].StartsWith("O:"))
            {
                string op = YK.Cagri(z.Hepsi[0]);
                string sic = YK.Cagri(z.Hepsi[1]);
                if (Array.IndexOf(Acilislar, op) < 0 || (sic != "4♣" && sic != "4♦")) return null;
                int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
                switch (op)
                {
                    case "2♥": case "3♥":
                        return sic == "4♣" ? (s >= 3 || s >= c ? "4♠" : "5♣") : (s >= 3 || s >= d ? "4♠" : "5♦");
                    case "2♠": case "3♠":
                        return sic == "4♣" ? (h >= 3 || h >= c ? "4♥" : "5♣") : (h >= 3 || h >= d ? "4♥" : "5♦");
                    case "2♦": case "3♦":
                        return sic == "4♣" ? "4♦" : (h >= s ? "4♥" : "4♠");
                    case "3♣":
                        return sic == "4♣" ? (h >= s ? "4♥" : "4♠") : "4♥";
                }
                return null;
            }

            // Müdahaleci: ilerleticinin sorusuna / geç-düzelt teklifine cevap
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("R:") && z.Hepsi[1].StartsWith("B:") && z.Hepsi[2].StartsWith("O:"))
            {
                string op = YK.Cagri(z.Hepsi[0]);
                string benim = YK.Cagri(z.Hepsi[1]);
                string o = YK.Cagri(z.Hepsi[2]);
                if (Array.IndexOf(Acilislar, op) < 0) return null;
                if ((op == "2♦" || op == "3♦") && benim == "4♣" && o == "4♦")
                    return z.Maca > z.Kupa ? "4♠" : "4♥";
                if ((op == "2♦" || op == "3♦") && benim == "4♦" && o == "4♥")
                    return z.Kupa >= z.Maca ? "Pas" : "4♠";
                if (op == "3♣" && benim == "4♦" && o == "4♥")
                    return z.Kupa >= 5 ? "Pas" : "4♠";
            }
            return null;
        }
    }
}
