using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Minör Stayman: 1NT–2♠ = oyun zorlayan, en az 4-4 minörlü, 4'lü majörü olmayan, şlem ilgili el (15+ HP).
    /// 10-14 HP'de sıra hep 3NT'de biteceği için (açıcının minörü şlem denemesi yapmaz) doğrudan 3NT söylenir;
    /// 2♠ yalnız rakiplere bilgi verir.
    /// Açıcı: 2NT = 4'lü minör yok; 3♣ / 3♦ = o minörde 4+ kart (ikisi de varsa uzun olan, eşitse ♣).
    /// Cevapçı: 2NT sonrası 3NT. Minör fiti bulunursa 15+ HP ile minörü 4 seviyede söyler (şlem denemesi), aksi halde 3NT.
    /// MinorTransfer ile birlikte kullanılamaz (ikisi de 2♠'yı kullanır).
    /// </summary>
    internal sealed class MinorStayman : YeniKonvTaban
    {
        public override string Ad { get { return "Minor Stayman"; } }
        public override string Aciklama { get { return "1NT–2♠: oyun zorlayan, 4-4 minör sorusu; 2NT yok, 3♣/3♦ 4'lü minör"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1NT"))
                return z.Hp >= 15 && z.Maca < 4 && z.Kupa < 4 && z.Sinek >= 4 && z.Karo >= 4 ? "2♠" : null;
            if (z.Es("B:1NT", "O:2♠"))
            {
                if (z.Sinek < 4 && z.Karo < 4) return "2NT";
                return z.Karo > z.Sinek ? "3♦" : "3♣";
            }
            if (z.Es("O:1NT", "B:2♠", "O:2NT")) return "3NT";
            if (z.Es("O:1NT", "B:2♠", "O:3♣")) return z.Hp >= 15 ? "4♣" : "3NT";
            if (z.Es("O:1NT", "B:2♠", "O:3♦")) return z.Hp >= 15 ? "4♦" : "3NT";
            return null;
        }
    }
    /// <summary>
    /// 1NT–2♣ Puppet Stayman: 2♣ = 5'li majör sorusu (4'lü majör + 8+ HP). Açıcı: 2♥/2♠ = 5'li majör, 2♦ = 5'li majör yok.
    /// 2♦ sonrası cevaplayan 4'lü majörünü 2♥/2♠ ile gösterir; açıcı fitte 3M/4M, fitsizde 2NT/3NT der.
    /// </summary>
    internal sealed class Puppet1NT : YeniKonvTaban
    {
        public override string Ad { get { return "Puppet Stayman (1NT)"; } }
        public override string Aciklama { get { return "1NT–2♣: 5'li majör sorusu; 2♦ = yok, 2♥/2♠ = 5'li majör; 2♦ sonrası 2♥/2♠ 4'lü majör"; } }
        protected override string Karar(Dz z)
        {
            int s = z.Maca, h = z.Kupa;
            if (z.Es("O:1NT"))
                return (z.Hp >= 8 && s < 5 && h < 5 && (s == 4 || h == 4)) ? "2♣" : null;
            if (z.Es("B:1NT", "O:2♣")) return h >= 5 && h >= s ? "2♥" : s >= 5 ? "2♠" : "2♦";
            if (z.Es("O:1NT", "B:2♣", "O:2♥") || z.Es("O:1NT", "B:2♣", "O:2♠"))
            {
                string m = z.Hepsi[2] == "O:2♥" ? "Kupa" : "Maça"; string sm = m == "Kupa" ? "♥" : "♠";
                if (z.Uz(m) >= 3) return z.Hp >= 10 ? "4" + sm : "3" + sm;
                return z.Hp >= 10 ? "3NT" : "2NT";
            }
            if (z.Es("O:1NT", "B:2♣", "O:2♦")) return h >= 4 ? "2♥" : s >= 4 ? "2♠" : null;
            if (z.Es("B:1NT", "O:2♣", "B:2♦", "O:2♥") || z.Es("B:1NT", "O:2♣", "B:2♦", "O:2♠"))
            {
                string m = z.Hepsi[3] == "O:2♥" ? "Kupa" : "Maça"; string sm = m == "Kupa" ? "♥" : "♠";
                if (z.Uz(m) >= 4) return z.Hp >= 16 ? "4" + sm : "3" + sm;
                return z.Hp >= 16 ? "3NT" : "2NT";
            }
            if (z.Es("B:1NT", "O:2♣", "B:2♥", "O:3♥") || z.Es("B:1NT", "O:2♣", "B:2♠", "O:3♠"))
                return z.Hp >= 16 ? "4" + (z.Hepsi[2] == "B:2♥" ? "♥" : "♠") : "Pas";
            if (z.Es("B:1NT", "O:2♣", "B:2♥", "O:2NT") || z.Es("B:1NT", "O:2♣", "B:2♠", "O:2NT") || z.Es("B:1NT", "O:2♣", "B:2♦", "O:2NT"))
                return z.Hp >= 16 ? "3NT" : "Pas";
            // Açıcının fitli/fitsiz cevabı sonrası cevaplayan
            if (z.Es("O:1NT", "B:2♣", "O:2♦", "B:2♥", "O:3♥") || z.Es("O:1NT", "B:2♣", "O:2♦", "B:2♠", "O:3♠"))
                return z.Hp >= 10 ? "4" + (z.Hepsi[3] == "B:2♥" ? "♥" : "♠") : "Pas";
            if (z.Es("O:1NT", "B:2♣", "O:2♦", "B:2♥", "O:2NT") || z.Es("O:1NT", "B:2♣", "O:2♦", "B:2♠", "O:2NT"))
                return z.Hp >= 10 ? "3NT" : "Pas";
            return null;
        }
    }

    /// <summary>1NT–2♠ minör rölesi: açıcı 3♣ demek zorundadır; cevaplayan ♣ ile Pas, ♦ ile 3♦ der (zayıf, 6+ minör).</summary>
    internal sealed class MinorRelay : YeniKonvTaban
    {
        public override string Ad { get { return "Minor Relay (1NT-2♠)"; } }
        public override string Aciklama { get { return "1NT–2♠: 3♣ rölesi; cevaplayan 3♣'de Pas ya da 3♦'e düzeltir"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1NT")) return (z.Hp < 8 && (z.Sinek >= 6 || z.Karo >= 6)) ? "2♠" : null;
            if (z.Es("B:1NT", "O:2♠")) return "3♣";
            if (z.Es("O:1NT", "B:2♠", "O:3♣")) return z.Karo > z.Sinek ? "3♦" : "Pas";
            return null;
        }
    }
}
