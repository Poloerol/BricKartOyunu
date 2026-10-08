using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Splinter Konvansiyonu (SAYC Standard).
    ///
    /// Kural:
    /// - Partner 1♠ veya 1♥ açtı.
    /// - Elimizde 4+ majör destek + 13+ HP + bir renkte singleton veya void var.
    /// - Kısa olan rengi "çift zıplama" (jump shift) ile teklif ederek slam ilgisini ve kısa rengi bildiririz.
    ///
    /// Örnekler:
    ///   1♠ - 4♣ = Sinek'te singleton/void + Maça desteği.
    ///   1♥ - 3♠ = Maça'da singleton/void + Kupa desteği.
    ///   1♥ - 4♣ = Sinek'te singleton/void + Kupa desteği.
    ///
    /// Öncelik: 16 (Jacoby 2NT'den önce, BasitCevap'tan sonra)
    /// </summary>
    public class Splinter : IKonvansiyon
    {
        public string Ad => "Splinter";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 16;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            if (durum.PartnerTeklifleri.Count == 0) return false;
            string partnerSon = durum.PartnerTeklifleri.Last();
            if (partnerSon != "1♠" && partnerSon != "1♥") return false;

            if (durum.KendiTeklifleri.Count > 0) return false;

            // Rakip müdahale ettiyse Splinter genellikle kullanılmaz (doğal teklifler önceliklidir)
            bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipGercekTeklifVerdi) return false;

            string koz = partnerSon == "1♠" ? "Maça" : "Kupa";
            int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, koz);
            if (destek < 4) return false;

            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 13) return false;

            // Kısa renk kontrolü (singleton veya void)
            var sayilar = ElDegerlendirici.RenkSayilari(durum.AktifOyuncuEli);
            foreach (var kv in sayilar)
            {
                if (kv.Key == koz) continue;
                if (kv.Value <= 1) return true;
            }

            return false;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string partnerSon = durum.PartnerTeklifleri.Last();
            string koz = partnerSon == "1♠" ? "Maça" : "Kupa";

            var sayilar = ElDegerlendirici.RenkSayilari(el);

            // En kısa rengi bul (singleton/void)
            string kisaRenk = sayilar
                .Where(kv => kv.Key != koz && kv.Value <= 1)
                .OrderBy(kv => kv.Value)
                .ThenByDescending(kv => kv.Key == "Maça" || kv.Key == "Kupa") // Majör kısa renkleri önceliklendir
                .Select(kv => kv.Key)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(kisaRenk)) return "Pas";

            // Seviye hesaplama (Çift Zıplama/Jump Shift)
            // 1♠ açılışına: 2 (tek), 3 (tek), 4 (çift zıplama)
            // 1♥ açılışına: 2 (tek), 3 (Majörse tek, Minörse çift zıplama), 4 (minörse çift zıplama)

            int seviye;
            if (koz == "Maça")
            {
                seviye = 4; // 1♠ -> 4 (Sinek/Karo)
            }
            else // koz == "Kupa"
            {
                if (kisaRenk == "Maça") seviye = 3; // 1♥ -> 3♠ (çift zıplama)
                else seviye = 4;                    // 1♥ -> 4♣/4♦ (çift zıplama)
            }

            return $"{seviye}{KozSembolu(kisaRenk)}";
        }

        private string KozSembolu(string renk)
        {
            return renk switch
            {
                "Maça" => "♠",
                "Kupa" => "♥",
                "Karo" => "♦",
                "Sinek" => "♣",
                _ => "?"
            };
        }
    }
}
