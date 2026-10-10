using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;   // KozYardimcisi için

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 2♣ Yapay Güçlü Açılış Konvansiyonu.
    /// 
    /// Kural:
    /// - 22+ HP VEYA
    /// - 19+ HP + 9 hızlı löve
    /// 
    /// Amaç: Çok güçlü elleri tek seferde göstermek.
    /// 
    /// Öncelik: 5
    /// </summary>
    public class IkiliSinekGuclu : IKonvansiyon
    {
        public string Ad => "2♣ Güçlü";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 5;

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // Sadece ilk teklifte geçerli
            if (!durum.IlkTeklifMi()) return false;

            var el = durum.AktifOyuncuEli;

            // 22+ HP → 2♣
            int hp = ElDegerlendirici.HCP(el);
            if (hp >= 22) return true;

            // 19+ HP + 9 hızlı löve → 2♣
            if (hp >= 19)
            {
                int tricks = HizliLoveSayisi(el);
                if (tricks >= 9) return true;
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // HIZLI LÖVE SAYISI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Hızlı löve sayısını hesaplar.
        /// 
        /// Kural:
        /// - Her renkte A = 1 löve
        /// - Her renkte K = 1 löve (A yoksa)
        /// - Her renkte Q = 1 löve (A ve K yoksa, 3+ kart)
        /// - 6+ kart uzun renk = +2 löve
        /// - 7+ kart uzun renk = +1 löve (ek)
        /// 
        /// Örnek:
        ///   ♠ AKQ54 ♥AKQ3 ♦AK ♣T87 → 7 hızlı löve (A K Q × 2 + A K)
        ///   ♠ AK ♥AQ8632 ♦A ♣Q1086 → 5 hızlı löve (A K + A Q + A)
        /// </summary>
        private int HizliLoveSayisi(List<Card> el)
        {
            int tricks = 0;

            // Her renkte A K Q kontrolü
            foreach (var renk in KozYardimcisi.TumRenkler)
            {
                var renkKartlari = el
                    .Where(c => c.Suit == renk)
                    .OrderByDescending(c => c.Value)
                    .ToList();

                if (renkKartlari.Count == 0) continue;

                // A = 1 löve
                if (renkKartlari[0].Value == 14)
                {
                    tricks++;
                }
                // K = 1 löve (A yoksa)
                else if (renkKartlari[0].Value == 13)
                {
                    tricks++;
                }
                // Q = 1 löve (A ve K yoksa, 3+ kart)
                else if (renkKartlari.Count >= 3 && renkKartlari[0].Value == 12)
                {
                    tricks++;
                }
            }

            // Uzun renk löve bonusu
            var sayilar = ElDegerlendirici.RenkSayilari(el)
                .Values.OrderByDescending(x => x).ToArray();

            if (sayilar[0] >= 6) tricks += 2;
            if (sayilar[0] >= 7) tricks += 1;

            return tricks;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            return "2♣";
        }
    }
}