using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Benjamin Two-Bids: 2♣ = çok güçlü (23+ HP ya da 20+ HP ve 7+ renk), 2♦ = Acol tipi güçlü iki (17-22 HP, 6+ renk ya da 5-5); 2♥/2♠ zayıf.</summary>
    internal sealed class BenjaminTwoBids : YeniKonvTaban
    {
        public override string Ad { get { return "Benjamin Two-Bids"; } }
        public override string Aciklama { get { return "2♣ çok güçlü, 2♦ güçlü iki (Acol), 2♥/2♠ zayıf"; } }

        private static string Dogal(Dz z, bool ikiKaroAcildi)
        {
            // Açıcının/olumlu cevabın doğal rengi: en uzun renk, mümkün olan en düşük seviyede
            string en = YK.EnUzun(z.El);
            if (ikiKaroAcildi && en == "Karo")
            {
                // 2♦'nin ikinci rengi
                en = YK.Renk.Where(r => r != "Karo").OrderByDescending(r => z.Uz(r)).ThenBy(r => Array.IndexOf(YK.Renk, r)).First();
            }
            return en;
        }

        protected override string Karar(Dz z)
        {
            int en = YK.Renk.Max(r => z.Uz(r));
            // ── Açılış
            if (z.Hepsi.Count == 0)
            {
                if (z.Hp >= 23 || (z.Hp >= 20 && en >= 7)) return "2♣";
                bool ikiBes = YK.Renk.Count(r => z.Uz(r) >= 5) >= 2;
                if (z.Hp >= 17 && z.Hp <= 22 && (en >= 6 || ikiBes) && !ElDegerlendirici.DengeliEl(z.El)) return "2♦";
                return null;
            }
            // ── 2♣'e cevap
            if (z.Es("O:2♣"))
            {
                if (z.Hp < 8) return "2♦";                       // bekleme / negatif
                if (z.Kupa >= 5 && z.Kupa >= z.Maca) return "2♥";
                if (z.Maca >= 5) return "2♠";
                if (z.Sinek >= 5 && z.Sinek >= z.Karo) return "3♣";
                if (z.Karo >= 5) return "3♦";
                return "2NT";
            }
            // ── 2♦'e cevap
            if (z.Es("O:2♦"))
            {
                if (z.Hp < 8) return "2♥";                       // negatif
                if (z.Maca >= 5) return "2♠";
                if (z.Sinek >= 5) return "3♣";
                if (z.Kupa >= 5) return "3♥";
                return "2NT";
            }
            // ── Açıcının ikinci teklifi
            if (z.Es("B:2♣", "O:2♦"))
            {
                if (ElDegerlendirici.DengeliEl(z.El)) return z.Hp >= 25 ? "3NT" : "2NT";
                string r = Dogal(z, false);
                if (r == "Karo") return "3♦";
                if (r == "Sinek") return "3♣";
                return YK.T(2, r);
            }
            if (z.Es("B:2♦", "O:2♥"))
            {
                string r = Dogal(z, true);
                if (r == "Maça") return "2♠";
                if (r == "Kupa") return "3♥";
                if (r == "Sinek") return "3♣";
                return "3♦";
            }
            return null;
        }
    }
}
