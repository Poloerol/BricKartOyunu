using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Last Train (son tren): koz uyumu kurulduktan ve slam ilgisi gösterildikten sonra, oyunun bir altındaki teklif
    /// "başka bir slam ilgisi" gösterir; kontrol (cue-bid) göstermez.
    /// Desteklenen dizi (♠ koz, splinter sonrası): 1♠ – 4♣/4♦ (splinter) – 4♥ (Last Train, açıcı 17+ HP) –
    /// cevapçı splinterde üst uçtaysa (11+ HP) 4NT (RKCB) der, aksi halde 4♠ ile bırakır.
    /// Açıcı 17 HP'nin altında ise doğrudan 4♠ der.
    /// </summary>
    internal sealed class LastTrain : YeniKonvTaban
    {
        public override string Ad { get { return "Last Train"; } }
        public override string Aciklama { get { return "1♠–4♣/4♦ splinter sonrası 4♥ = Last Train (slam ilgisi, kontrol değil)"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("B:1♠", "O:4♣") || z.Es("B:1♠", "O:4♦"))
                return z.Hp >= 17 ? "4♥" : "4♠";
            if (z.Es("O:1♠", "B:4♣", "O:4♥") || z.Es("O:1♠", "B:4♦", "O:4♥"))
                return z.Hp >= 11 ? "4NT" : "4♠";
            return null;
        }
    }
}
