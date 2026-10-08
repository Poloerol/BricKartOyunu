using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Unusual 2NT Konvansiyonu (SAYC Standard).
    ///
    /// Kural:
    /// - Rakip 1 seviyesinde bir renk açtı.
    /// - Ben 2NT diyerek, rakip rengi hariç kalan renklerden en küçük iki tanesinde 5+ kartım olduğunu bildiririm.
    ///
    /// Rakip rengine göre anlamlar:
    ///   Rakip 1♣ → 2NT = 5♥ + 5♦
    ///   Rakip 1♦ → 2NT = 5♥ + 5♣
    ///   Rakip 1♥ → 2NT = 5♦ + 5♣
    ///   Rakip 1♠ → 2NT = 5♦ + 5♣
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
            if (string.IsNullOrEmpty(rakipSon) || TekliftenSeviyeCikar(rakipSon) != 1) return false;

            // Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            string rakipKozu = TekliftenKozCikar(rakipSon);
            if (string.IsNullOrEmpty(rakipKozu)) return false;

            // SAYC Unusual 2NT Mantığı:
            // Rakip rengi hariç kalan renklerden en düşük iki tanesini gösterir.
            // Renk sıralaması: Sinek < Karo < Kupa < Maça
            var el = durum.AktifOyuncuEli;
            var renkSiralama = new List<string> { "Sinek", "Karo", "Kupa", "Maça" };

            // Rakip rengi hariç tut ve ilk iki düşük rengi seç
            var adayRenkler = renkSiralama
                .Where(r => r != rakipKozu)
                .Take(2)
                .ToList();

            // Bu iki rengin her birinde 5+ kart olmalı
            foreach (var renk in adayRenkler)
            {
                if (ElDegerlendirici.RenkUzunlugu(el, renk) < 5) return false;
            }

            return adayRenkler.Count == 2;
        }

        public string TeklifKodu(IhaleDurumu durum)
        {
            return "2NT";
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "2NT";
        }

        private string TekliftenKozCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif)) return null;
            char son = teklif[teklif.Length - 1];
            return son switch { '♠' => "Maça", '♥' => "Kupa", '♦' => "Karo", '♣' => "Sinek", 'T' => "NT", 't' => "NT", _ => null };
        }

        private int TekliftenSeviyeCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2) return 0;
            if (int.TryParse(teklif.Substring(0, teklif.Length - 1), out int s)) return s;
            return 0;
        }
    }
}
