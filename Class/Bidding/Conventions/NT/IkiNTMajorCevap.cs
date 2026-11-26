using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 2NT (20-21, dengeli) açılışına uzun majörlü cevaplar. Açıcıda her renkte en az 2 kart vardır:
    ///  • 6+ kartlık majör = fit bulunmuş demektir: slam değerinde (12+ HP) doğrudan 6M; daha azında Texas/yedek mantık (4M çağrısı Texas 4♦/4♥ ile çakışır).
    ///  • 5+ kartlık majör: Jacoby transfer 3♦ = ♥, 3♥ = ♠ (açıcı kabul eder; sonra 0-3 HP Pas, 4+ HP 5 kartla 3NT / 6 kartla 4M; açıcı 3NT sonrası 3+ destekle 4M, yoksa Pas). 2♣-2♦-2NT dizisinde doğal 3M.
    ///  • 4'lü majörler Puppet Stayman'ın işidir (5+ kartlık majörde Puppet Stayman kullanılmaz).
    /// </summary>
    internal sealed class IkiNTMajorCevap : YeniKonvTaban
    {
        public override string Ad { get { return "2NT Majör Cevabı"; } }
        public override string Aciklama { get { return "2NT–3♦ = Jacoby transfer (5+♥), 2NT–3♥ = Jacoby transfer (5+♠): açıcı 3♥/3♠ ile kabul eder; cevapçı sonra Pas (0-3 HP), 3NT (5 kart, açıcı 3+ destekle 4M) ya da 4M (6+ kart). 6+ majör slam değerinde (12+ HP) doğrudan 6M; majörsüz 11-12 HP nicel 4NT, 13+ HP 6NT"; } }
        public override int Oncelik { get { return 12; } }

        protected override string Karar(Dz z)
        {
            if (z.D != null && z.D.Anlasma != null && z.D.Anlasma.TwoNT_AnswerSistemi) return null;

            // 2♣ (güçlü) - 2♦ (bekleme) - 2NT: açıcı kesin dengeli 23+ HP, dizi oyuna kadar zorlayıcı
            bool ikiSinek = z.Es("O:2♣", "B:2♦", "O:2NT");

            // 2NT (20-22) sonrası majörsüz cevapçı: toplam güç slam bölgesindeyse 3NT yerine nicel 4NT (6NT daveti) / doğrudan 6NT.
            // Açıcı 4NT'ye 22+ HP ile 6NT der, aksi halde pas. (Quantitative 4NT anlaşması kapalı olsa da çalışır.)
            if (!ikiSinek && z.Es("O:2NT") && z.Maca < 4 && z.Kupa < 4)
            {
                if (z.Hp >= Deney.Al("BK_Q6", 99)) return "6NT";
                if (z.Hp >= Deney.Al("BK_Q4", 99)) return "4NT";
            }
            if (z.Es("B:2NT", "O:4NT")) return z.Hp >= Deney.Al("BK_QO", 22) ? "6NT" : "Pas";

            // 2NT'de Jacoby transfer: 3♦ = 5+♥, 3♥ = 5+♠. Açıcı kabul eder (3♥ / 3♠); cevapçı sonra
            // 0-3 HP Pas, 4-11 HP 5 kartla 3NT (açıcı 3+ destekle 4M, yoksa Pas), 6+ kartla 4M.
            if (!ikiSinek)
            {
                if (z.Es("B:2NT", "O:3♦")) return "3♥";
                if (z.Es("B:2NT", "O:3♥")) return "3♠";
                if (z.Es("O:2NT", "B:3♦", "O:3♥") || z.Es("O:2NT", "B:3♥", "O:3♠"))
                {
                    bool kupaTr = z.Es("O:2NT", "B:3♦", "O:3♥");
                    int uz = kupaTr ? z.Kupa : z.Maca;
                    string rk = kupaTr ? "♥" : "♠";
                    if (z.Hp < 4) return "Pas";
                    if (z.Hp >= Deney.Al("BK_JQ", 99)) return "4NT";   // 11-12 HP: nicel slam daveti (açıcı 6M / 6NT / pas)
                    return uz >= 6 ? "4" + rk : "3NT";
                }
                if (z.Es("B:2NT", "O:3♦", "B:3♥", "O:3NT")) return z.Kupa >= 3 ? "4♥" : "Pas";
                if (z.Es("B:2NT", "O:3♥", "B:3♠", "O:3NT")) return z.Maca >= 3 ? "4♠" : "Pas";
                // Transfer tamamlandıktan sonra cevapçı nicel 4NT dedi: 22+ HP ile 3+ destekte 6M, destek yoksa 6NT; aksi halde pas
                if (z.Es("B:2NT", "O:3♦", "B:3♥", "O:4NT")) return z.Hp >= Deney.Al("BK_QO", 22) ? (z.Kupa >= 3 ? "6♥" : "6NT") : "Pas";
                if (z.Es("B:2NT", "O:3♥", "B:3♠", "O:4NT")) return z.Hp >= Deney.Al("BK_QO", 22) ? (z.Maca >= 3 ? "6♠" : "6NT") : "Pas";
                if (z.Es("O:2NT"))
                {
                    int m5 = Math.Max(z.Maca, z.Kupa);
                    if (m5 >= 5)
                    {
                        string r5 = z.Kupa >= z.Maca ? "♥" : "♠";
                        if (m5 >= 6 && z.Hp >= 12) return "6" + r5;     // slam değeri: doğrudan (eski kural)
                        if (m5 >= 6 && z.Hp >= 4) return null;          // oyun değeri 6+: Texas (4♦/4♥)
                        return z.Kupa >= z.Maca ? "3♦" : "3♥";           // transfer
                    }
                    return null;
                }
            }

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
