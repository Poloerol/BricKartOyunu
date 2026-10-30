using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Kokish rölesi: 2♣ – 2♦ (bekleme) – 2♥ ile açıcı iki elden birini gösterir:
    ///   • dengeli 25-27 HP (22-24 dengeli doğrudan 2NT der), ya da
    ///   • oyun zorlayan ♥ rengi.
    /// Cevapçı 2♠ ile röleyi tamamlar. Açıcı sonra 2NT (dengeli 25-27) ya da 3♥ (kupa rengi) der.
    /// Kokish, güçlü 2♣ açılışı (2♣ Güçlü) gerektirir.
    /// </summary>
    internal sealed class Kokish : YeniKonvTaban
    {
        public override string Ad { get { return "Kokish"; } }
        public override string Aciklama { get { return "2♣–2♦–2♥ röle: dengeli 25-27 ya da kupa rengi; 2♠ tamamlar, 2NT / 3♥ devam"; } }
        protected override string Karar(Dz z)
        {
            bool dengeli = ElDegerlendirici.DengeliEl(z.El);
            // Açıcının ikinci teklifi
            if (z.Es("B:2♣", "O:2♦"))
            {
                if (dengeli && z.Hp >= 25 && z.Hp <= 27) return "2♥";
                if (!dengeli && z.Kupa >= 5 && z.Kupa >= z.Maca) return "2♥";
                return null;
            }
            // Cevapçı röleyi tamamlar
            if (z.Es("O:2♣", "B:2♦", "O:2♥")) return "2♠";
            // Açıcı: röle tamamlandıktan sonra
            if (z.Es("B:2♣", "O:2♦", "B:2♥", "O:2♠")) return dengeli && z.Hp >= 25 ? "2NT" : "3♥";
            // Cevapçı: açıcının 2NT'si (dengeli 25-27) ya da 3♥'i (kupa rengi)
            if (z.Es("O:2♣", "B:2♦", "O:2♥", "B:2♠", "O:2NT"))
            {
                if (z.Maca >= 4 || z.Kupa >= 4) return "3♣";
                return z.Hp >= 8 ? "4NT" : "3NT";
            }
            if (z.Es("O:2♣", "B:2♦", "O:2♥", "B:2♠", "O:3♥")) return z.Kupa >= 3 ? "4♥" : "3NT";
            return null;
        }
    }
}
