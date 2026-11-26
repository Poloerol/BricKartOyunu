using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Ortağın güçlü 2♣ açılışına cevap: her elle 2♦ (yapay bekleme). Doğal 2NT / renk cevabı yoktur;
    /// açıcı 2♦ sonrası 2NT (dengeli 23+), 2M, 3M ya da yeni renkle devam eder, cevapçı oradan karar verir.
    /// 2♣ Cevap Sistemi (TwoClub) açıksa onun kuralları geçerlidir, bu kural devreye girmez.
    /// </summary>
    internal sealed class IkiSinekBeklemeCevabi : YeniKonvTaban
    {
        public override string Ad { get { return "2♣ Bekleme Cevabı"; } }
        public override string Aciklama { get { return "Ortağın güçlü 2♣ açılışına her elle 2♦ (yapay bekleme)"; } }
        public override int Oncelik { get { return 5; } }

        protected override string Karar(Dz z)
        {
            if (Deney.Al("BK_W2", 1) == 0) return null;
            return z.Es("O:2♣") ? "2♦" : null;
        }
    }
}
