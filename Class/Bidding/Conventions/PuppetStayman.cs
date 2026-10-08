using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Puppet Stayman Konvansiyonu.
    /// 
    /// Kural:
    /// - Partner 2NT açtı (20-21 HP dengeli)
    /// - Ben 3♣ derim → "5'li majör var mı?"
    /// - Partner cevap verir:
    ///   - 3♦ = 4'lü majör YOK
    ///   - 3♥ = 5'li ♥ var
    ///   - 3♠ = 5'li ♠ var
    /// 
    /// Amaç: 2NT açılışına karşı 5'li majörü bulmak.
    /// 
    /// Öncelik: 27
    /// </summary>
    public class PuppetStayman : IKonvansiyon
    {
        public string Ad => "Puppet Stayman";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 27;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // 1. Partner 2NT açtı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            if (durum.PartnerTeklifleri.Last() != "2NT") return false;

            // 2. Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            // 3. Rakip müdahale etmedi
            bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipGercekTeklifVerdi) return false;

            // 4. Elimde en az 4'lü bir majör var mı?
            var el = durum.AktifOyuncuEli;
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

            return maca >= 4 || kupa >= 4;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            // 3♣ yapay
            return "3♣";
        }
    }
}