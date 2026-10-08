using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Puppet Stayman Konvansiyonu (Doğal versiyon).
    /// 
    /// 2NT açılışına 5-3 veya 4-4 majör fit bulmak için 3♣ kullanılır.
    /// 
    /// AŞAMA 1: 2NT - 3♣ - ?
    ///   3♦  = 5'li majör yok, 4'lü majör var
    ///   3♥  = 5'li ♥ var
    ///   3♠  = 5'li ♠ var
    ///   3NT = hiç majör yok
    /// 
    /// AŞAMA 2: 2NT - 3♣ - 3♦ - ?
    ///   3♥   = "4'lü ♠ var mı?"
    ///   3♠   = "4'lü ♥ var mı?"
    ///   3NT  = sign-off
    ///   4♣   = 4-4 majör, slam ilgisi
    ///   4♦   = 4-4 majör, slam ilgisi yok
    /// 
    /// NOT: Aşama 3, 4, Key-Card sonra eklenecek.
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

            var el = durum.AktifOyuncuEli;

            // ─────────────────────────────────────────────────────
            // DURUM 1: Partner 2NT açtı, ben 3♣ soracağım
            // ─────────────────────────────────────────────────────
            if (durum.PartnerTeklifleri.Count > 0 &&
                durum.PartnerTeklifleri.Last() == "2NT" &&
                durum.KendiTeklifleri.Count == 0)
            {
                // Rakip müdahale etmedi
                bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
                    durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
                if (rakipGercekTeklifVerdi) return false;

                // En az 4'lü majör var mı?
                int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                return maca >= 4 || kupa >= 4;
            }

            // ─────────────────────────────────────────────────────
            // DURUM 2: Partner 3♣ dedi (ben 2NT açmıştım), cevap vereceğim
            // ─────────────────────────────────────────────────────
            if (durum.SonGercekTeklif == "3♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("2NT"))
            {
                return true;
            }

            // ─────────────────────────────────────────────────────
            // DURUM 3: Partner 3♦ dedi, ben (3♣ soran) cevap vereceğim
            // ─────────────────────────────────────────────────────
            if (durum.SonGercekTeklif == "3♦" &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("3♣") &&
                durum.KendiTeklifleri.Contains("2NT") == false)
            {
                return true;
            }

            // ─────────────────────────────────────────────────────
            // DURUM 4: Partner 3♥/3♠ dedi (5'li majör), ben (3♣ soran) cevap vereceğim
            // ─────────────────────────────────────────────────────
            if ((durum.SonGercekTeklif == "3♥" || durum.SonGercekTeklif == "3♠") &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("3♣"))
            {
                return true;
            }

            return false;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

            // ─────────────────────────────────────────────────────
            // DURUM 1: Ben 3♣ soruyorum (partner 2NT açtı)
            // ─────────────────────────────────────────────────────
            if (durum.PartnerTeklifleri.Last() == "2NT" &&
                durum.KendiTeklifleri.Count == 0)
            {
                return "3♣";
            }

            // ─────────────────────────────────────────────────────
            // DURUM 2: Ben 2NT açmıştım, partner 3♣ dedi → cevap veriyorum
            // ─────────────────────────────────────────────────────
            if (durum.SonGercekTeklif == "3♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("2NT"))
            {
                // 5'li majör var mı?
                if (kupa >= 5) return "3♥";  // 5'li ♥ (doğal)
                if (maca >= 5) return "3♠";  // 5'li ♠ (doğal)

                // 4'lü majör var mı?
                if (kupa == 4 || maca == 4) return "3♦";

                // Hiç majör yok
                return "3NT";
            }

            // ─────────────────────────────────────────────────────
            // DURUM 3: Ben 3♣ sormuştum, partner 3♦ dedi → 2. tur
            // ─────────────────────────────────────────────────────
            if (durum.SonGercekTeklif == "3♦" &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("3♣"))
            {
                // 4'lü majörlerim var mı?
                bool dortluMacaVar = maca == 4;
                bool dortluKupaVar = kupa == 4;

                // 4-4 majör var mı? (ikisi de 4'lü)
                if (dortluMacaVar && dortluKupaVar)
                {
                    // Slam ilgisi var mı? (16+ HP)
                    int hp = ElDegerlendirici.HCP(el);
                    if (hp >= 16) return "4♣";  // Slam ilgisi
                    return "4♦";  // Slam ilgisi yok
                }

                // Sadece 4'lü ♠ var → "4'lü ♠ var mı?" sorusu (3♥)
                if (dortluMacaVar && !dortluKupaVar) return "3♥";

                // Sadece 4'lü ♥ var → "4'lü ♥ var mı?" sorusu (3♠)
                if (dortluKupaVar && !dortluMacaVar) return "3♠";

                // Majör yok → sign-off
                return "3NT";
            }

            // ─────────────────────────────────────────────────────
            // DURUM 4: Ben 3♣ sormuştum, partner 3♥/3♠ (5'li majör) dedi
            // ─────────────────────────────────────────────────────
            if ((durum.SonGercekTeklif == "3♥" || durum.SonGercekTeklif == "3♠") &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("3♣"))
            {
                // "Okunan majörde" fit var mı?
                string okunan = durum.SonGercekTeklif == "3♥" ? "Kupa" : "Maça";
                int fit = ElDegerlendirici.RenkUzunlugu(el, okunan);

                // Fit VAR (3+ kart) → 4 seviyesinde destek
                if (fit >= 3)
                {
                    // Kontrol göstermek için 4♣/4♦ (basit yaklaşım)
                    // Ama şimdilik sadece 4 seviyesi destek diyelim
                    return okunan == "Kupa" ? "4♥" : "4♠";
                }

                // Fit YOK → sign-off
                return "3NT";
            }

            return "Pas";
        }
    }
}