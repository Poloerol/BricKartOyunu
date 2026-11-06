using System;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// ÇİFT YÖNLÜ CHECKBACK STAYMAN: 2♦ FG RÖLESİ (SAS kitabı s.92-93). Açanın 1NT rebidine (1♣-1♦-1NT ve 1♥-1♠-1NT)
    /// cevapçı 2♦ (oyun zorlaması) dedi: açanın cevabı ÖNCELİK SIRASIYLA
    ///   1- 4'lü majör söyle (2♥ / 2♠)       2- cevapçının renginde 3'lü tutuşu belirt (x=♦: 3♦, x=♠: 2♠)
    ///   3- 5'li iyi renk ya da 6'lı kötü rengi göster (kendi renginin en ucuz basamağı: ♣ için 3♣, ♥ için 2♥)
    ///   4- ("5/4 minör": diğer minörü okumak) — kitaptaki bu satır, 2. maddenin tutuşuyla örtüştüğü için ayrıca kodlanmadı   5- 2NT.
    /// Cevapçının 2. hamlesi (kitapta ayrıntılı değil; SEÇİM): açanın söylediği majörde 4+ kartla 4M; cevapçının renginde uyum
    /// (♠ için 3+) 4M; kendi rengini söylemişse ve destek varsa 4M; diğer durumlarda 3NT; 6+ renk ve zayıf eşleşmede 4x/5x.
    /// </summary>
    internal static class OneNTRole
    {
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static int Onur(OcEl e, char c)
        {
            string ad = c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek";
            int n = 0;
            foreach (var k in e.El) if (k.Suit == ad && k.Value >= 12) n++;      // A K Q
            return n;
        }

        /// <summary>x = cevapçının renk (♦ ya da ♠), a = açanın renk (♣ ya da ♥).</summary>
        public static string AciciCevap(OcEl e, char x, char a)
        {
            if (a == '♣')
            {
                if (e.H == 4) return "2♥";
                if (e.S == 4) return "2♠";
            }
            if (Uz(e, x) >= 3) return x == '♠' ? "2♠" : "3" + x;
            int n = Uz(e, a);
            if ((n == 5 && Onur(e, a) >= 2) || n >= 6) return a == '♥' ? "2♥" : "3" + a;
            return "2NT";
        }

        public static string CevapciKarar(OcEl e, char x, char a, string cevap)
        {
            if (cevap == "2♥" || cevap == "2♠")
            {
                char m = cevap[1];
                int destek = (m == a) ? 3 : (m == x ? 3 : 4);                  // a=♥: 5'li renk, 3 destek yeter
                if (Uz(e, m) >= destek) return "4" + m;
                return "3NT";
            }
            if (cevap == "3♦" || cevap == "3♣") return "3NT";
            if (cevap == "2NT")
            {
                if (x == '♠' && e.S >= 6) return "4♠";
                return "3NT";
            }
            return null;
        }
    }
}
