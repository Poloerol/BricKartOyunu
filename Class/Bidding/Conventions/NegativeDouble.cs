using System;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Negative Double (Negatif Kontr) Konvansiyonu.
    /// 
    /// Kural:
    /// - Rakip bir renk açtı (1♠/1♥/1♦/1♣)
    /// - Benim elimde gösteremediğim bir renk var (özellikle 4'lü majör)
    /// - Dbl derim → "Diğer renkleri oynayabilirim" demek
    /// 
    /// Öncelik: 20 (açılışlardan sonra, cevaplardan önce)
    /// 
    /// Örnek:
    ///   Rakip 1♠ → benim elimde 4'lü Kupa + 8+ HP → Dbl
    ///   Rakip 1♥ → benim elimde 4'lü Maça + 8+ HP → Dbl
    ///   Rakip 1♦ → benim elimde 4'lü Maça + 4'lü Kupa + 8+ HP → Dbl
    /// </summary>
    public class NegativeDouble : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "Negative Double";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 20;   // Stayman (10) sonra, BasitCevap (100) önce

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. Rakip bir renk açtı mı?
            if (!durum.RakipActiMi()) return false;

            // 2. Rakip 2+ seviyede açmadı mı? (1 seviye olmalı)
            string sonRakipTeklifi = durum.RakipTeklifleri.LastOrDefault();
            if (string.IsNullOrEmpty(sonRakipTeklifi)) return false;

            // 3. Ben henüz konuşmadım mı?
            if (durum.KendiTeklifleri.Count > 0) return false;

            // 4. Partner henüz konuşmadı mı? (veya Pas dedi?)
            if (durum.PartnerTeklifleri.Count > 0)
            {
                // Partner zaten konuştuysa Negatif Kontr değil
                return false;
            }

            // 5. El analizi
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 8) return false;

            // 6. Rakip rengini çıkar
            string rakipKozu = TekliftenKozCikar(sonRakipTeklifi);
            if (string.IsNullOrEmpty(rakipKozu)) return false;

            // 7. Gösteremediğimiz majör var mı?
            //    Rakip Maça açtıysa → Kupa göstermek isteriz (4'lü)
            //    Rakip Kupa açtıysa → Maça göstermek isteriz (4'lü)
            //    Rakip minör açtıysa → 4'lü Maça veya 4'lü Kupa göstermek isteriz

            int dortluMaca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int dortluKupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

            // Rakip Maça açtıysa → 4'lü Kupa göstermek isteriz
            if (rakipKozu == "Maça")
            {
                if (dortluKupa >= 4) return true;
            }
            // Rakip Kupa açtıysa → 4'lü Maça göstermek isteriz
            else if (rakipKozu == "Kupa")
            {
                if (dortluMaca >= 4) return true;
            }
            // Rakip minör açtıysa → 4'lü Maça VEYA 4'lü Kupa göstermek isteriz
            else if (rakipKozu == "Karo" || rakipKozu == "Sinek")
            {
                if (dortluMaca >= 4 || dortluKupa >= 4) return true;
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            // Negatif Kontr → Dbl
            return "Dbl";
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Tekliften kozu çıkarır.
        /// </summary>
        private string TekliftenKozCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif)) return null;

            char sonKarakter = teklif[teklif.Length - 1];
            switch (sonKarakter)
            {
                case '♠': return "Maça";
                case '♥': return "Kupa";
                case '♦': return "Karo";
                case '♣': return "Sinek";
                case 'T':
                case 't': return "NT";
                default: return null;
            }
        }
    }
}