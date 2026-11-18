using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 2♣ (güçlü) - 2♦ (bekleme) sonrası açıcının ikinci teklifi:
    ///  • 2NT = dengeli 23+ HCP (5'li majörlü 5-3-3-2 olabilir; bu sınıf onu 2NT'ye bırakır);
    ///  • 2M = 5+ kartlık majör + 23+ HCP (dengesiz el);
    ///  • 3M = 17-22 HCP, 6+ kartlık güçlü majör (en az 8-8,5 oyun löveyi tek başına taşır).
    /// Diğer eller yedek mantığa kalır.
    /// </summary>
    internal sealed class IkiSinekAciciTekrar : YeniKonvTaban
    {
        public override string Ad { get { return "2♣ Açıcı Tekrarı"; } }
        public override string Aciklama { get { return "2♣-2♦-2M = 5+ majör + 23+ HP; 2♣-2♦-3M = 17+ HP uzun majör (8-8,5 löve); 2NT = dengeli 23+"; } }
        public override int Oncelik { get { return 5; } }

        protected override string Karar(Dz z)
        {
            if (!z.Es("B:2♣", "O:2♦")) return null;
            int m = Math.Max(z.Maca, z.Kupa);
            if (m < 5) return null;
            string renk = z.Kupa > z.Maca ? "Kupa" : "Maça";
            string sem = renk == "Kupa" ? "♥" : "♠";
            bool dengeli = ElDegerlendirici.DengeliEl(z.El);

            if (z.Hp >= 23)
            {
                if (dengeli) return null;   // 5-3-3-2: 2NT (açıcı 23+ dengeli, 5'li majörlü olabilir)
                return "2" + sem;
            }
            if (z.Hp >= 17 && m >= 6)
            {
                int onur = z.El.Count(c => c.Suit == renk && c.Value >= 12);   // A, K, Q sayısı
                if (onur >= 2) return "3" + sem;
            }
            return null;
        }
    }
}
