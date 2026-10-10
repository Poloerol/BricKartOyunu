using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;   // KozYardimcisi için

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Michaels Cue Bid Konvansiyonu — Rakip açılışına cue bid ile iki renk gösterme.
    /// 
    /// Kural:
    /// - Rakip 1♠/1♥/1♦/1♣ açtı
    /// - Elimizde 5+ diğer majör + 5+ bir minör (veya her iki majör) var
    /// - Rakip rengini cue bid olarak teklif ederiz
    /// 
    /// Örnek:
    ///   Rakip 1♠ → 2♠ = 5+ Kupa + 5+ bir minör
    ///   Rakip 1♥ → 2♥ = 5+ Maça + 5+ bir minör
    ///   Rakip 1♦ → 2♦ = 5+ Maça + 5+ Kupa (her iki majör)
    ///   Rakip 1♣ → 2♣ = 5+ Maça + 5+ Kupa (her iki majör)
    /// 
    /// Öncelik: 19
    /// </summary>
    public class Michaels : IKonvansiyon
    {
        public string Ad => "Michaels";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 19;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Rakip 1 seviyesinde açtı mı?
            if (durum.RakipTeklifleri.Count == 0) return false;
            string rakipSon = durum.RakipTeklifleri.Last();
            if (rakipSon != "1♠" && rakipSon != "1♥" &&
                rakipSon != "1♦" && rakipSon != "1♣") return false;

            // Biz henüz konuşmadık
            if (durum.KendiTeklifleri.Count > 0) return false;

            string rakipKoz = KozYardimcisi.TekliftenKozCikar(rakipSon);
            if (string.IsNullOrEmpty(rakipKoz)) return false;

            var el = durum.AktifOyuncuEli;

            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // Rakip Maça açtıysa → 5+ Kupa + 5+ minör
            if (rakipKoz == "Maça")
                return kupa >= 5 && (karo >= 5 || sinek >= 5);

            // Rakip Kupa açtıysa → 5+ Maça + 5+ minör
            if (rakipKoz == "Kupa")
                return maca >= 5 && (karo >= 5 || sinek >= 5);

            // Rakip minör açtıysa → 5+ Maça + 5+ Kupa
            if (rakipKoz == "Karo" || rakipKoz == "Sinek")
                return maca >= 5 && kupa >= 5;

            return false;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            string rakipSon = durum.RakipTeklifleri.Last();
            string rakipKoz = KozYardimcisi.TekliftenKozCikar(rakipSon);
            string sembol = KozYardimcisi.KozSembolu(rakipKoz);

            // Cue bid: rakip rengini 2 seviyesinde teklif et
            return $"2{sembol}";
        }
    }
}