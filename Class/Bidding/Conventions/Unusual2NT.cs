using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Unusual 2NT Konvansiyonu ("Diğer İki Renk" Yaklaşımı).
    /// 
    /// Kural:
    /// - Rakip 1 seviyesinde bir renk açtı (1♣/1♦/1♥/1♠)
    /// - Ben 2NT derim → "Rakip rengi hariç, kalan renklerden 
    ///                    EN KÜÇÜK ikisini 5+5 olarak gösteriyorum"
    /// 
    /// Rakip rengine göre anlamlar:
    ///   Rakip 1♣ → 2NT = 5♥ + 5♦
    ///   Rakip 1♦ → 2NT = 5♥ + 5♣
    ///   Rakip 1♥ → 2NT = 5♦ + 5♣
    ///   Rakip 1♠ → 2NT = 5♦ + 5♣
    /// 
    /// NOT: Bu konvansiyon, klasik Michaels ile ÇAKIŞMAZ.
    ///      Michaels = rakip rengine göre MAJÖR bazlı iki renk gösterir.
    ///      Unusual 2NT = rakip rengine göre EN KÜÇÜK iki rengi gösterir.
    /// 
    /// Öncelik: 23
    /// </summary>
    public class Unusual2NT : IKonvansiyon
    {
        public string Ad => "Unusual 2NT";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 23;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Rakip 1 seviyesinde bir renk açtı mı?
            if (durum.RakipTeklifleri.Count == 0) return false;
            string rakipSon = durum.RakipTeklifleri.Last();
            if (rakipSon != "1♠" && rakipSon != "1♥" &&
                rakipSon != "1♦" && rakipSon != "1♣") return false;

            // Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            // Rakip rengini çıkar
            string rakipKozu = TekliftenKozCikar(rakipSon);
            if (string.IsNullOrEmpty(rakipKozu)) return false;

            // Rakip rengi hariç, kalan 3 renkten EN KÜÇÜK İKİSİNİ bul
            // Sıralama: Sinek (en küçük) → Karo → Kupa → Maça (en büyük)
            var el = durum.AktifOyuncuEli;
            var renkSiralama = new List<string> { "Sinek", "Karo", "Kupa", "Maça" };
            var adayRenkler = renkSiralama
                .Where(r => r != rakipKozu)
                .Take(2)
                .ToList();

            // İki aday renkte de 5+ kart olmalı
            foreach (var renk in adayRenkler)
            {
                if (ElDegerlendirici.RenkUzunlugu(el, renk) < 5) return false;
            }

            return adayRenkler.Count == 2;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "2NT";
        }

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