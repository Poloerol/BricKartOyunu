using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 2NT (20-21, dengeli) açılışına uzun majörlü cevaplar. Açıcıda her renkte en az 2 kart vardır:
    ///  • 6+ kartlık majör = fit bulunmuş demektir: slam değerinde (12+ HP) doğrudan 6M; daha azında Texas/yedek mantık (4M çağrısı Texas 4♦/4♥ ile çakışır).
    ///  • Tam 5 kartlık majör: doğal 3M (oyuna kadar zorlayıcı); açıcı 3+ destekle 4M, yoksa 3NT der.
    ///  • 4'lü majörler Puppet Stayman'ın işidir (5+ kartlık majörde Puppet Stayman kullanılmaz).
    /// </summary>
    internal sealed class IkiNTMajorCevap : YeniKonvTaban
    {
        public override string Ad { get { return "2NT Majör Cevabı"; } }
        public override string Aciklama { get { return "2NT–3M = 5'li majör (açıcı 3+ destekle 4M, yoksa 3NT); 6+ majör: 4M ya da slam değerinde 6M"; } }
        public override int Oncelik { get { return 12; } }

        protected override string Karar(Dz z)
        {
            if (z.D != null && z.D.Anlasma != null && z.D.Anlasma.TwoNT_AnswerSistemi) return null;

            // 2♣ (güçlü) - 2♦ (bekleme) - 2NT: açıcı kesin dengeli 23+ HP, dizi oyuna kadar zorlayıcı
            bool ikiSinek = z.Es("O:2♣", "B:2♦", "O:2NT");

            // Cevapçı: 2NT sonrası
            if (z.Es("O:2NT") || ikiSinek)
            {
                if (!ikiSinek && z.Hp < 4) return null;
                int m = Math.Max(z.Maca, z.Kupa);
                string renk = z.Kupa >= z.Maca ? "♥" : "♠";
                if (m >= 6)
                {
                    if (z.Hp >= (ikiSinek ? 9 : 12)) return "6" + renk;   // 20+12 / 23+9: şlem
                    // 2NT açılışında oyun değeri: Texas (4♦/4♥) ya da yedek mantık, doğrudan 4M Texas ile çakışır; 2♣ dizisinde Texas yok: doğrudan 4M
                    return ikiSinek ? "4" + renk : null;
                }
                if (m == 5) return "3" + renk;
                return null;
            }
            // Açıcı: cevapçı doğal 3M dedi
            if (z.Es("B:2NT", "O:3♥") || z.Es("B:2♣", "O:2♦", "B:2NT", "O:3♥")) return z.Kupa >= 3 ? "4♥" : "3NT";
            if (z.Es("B:2NT", "O:3♠") || z.Es("B:2♣", "O:2♦", "B:2NT", "O:3♠")) return z.Maca >= 3 ? "4♠" : "3NT";
            return null;
        }
    }
}
