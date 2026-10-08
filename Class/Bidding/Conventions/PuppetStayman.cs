using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Puppet Stayman Konvansiyonu (SAYC Standard).
    ///
    /// 2NT açılışına karşı 4-4 majör fitlerini veya 5'li majörleri bulmak için kullanılır.
    ///
    /// AŞAMA 1: 2NT -> 3♣ (Soru)
    ///   3♦  = Majör yok veya sadece 4'lü majör var (SAYC'de genellikle majör yok anlamındadır)
    ///   3♥  = 5'li ♥ var
    ///   3♠  = 5'li ♠ var
    ///   3NT = Majör yok
    ///
    /// AŞAMA 2: 2NT -> 3♣ -> 3♦ (Yani "majör yok" cevabına karşılık)
    ///   3♥   = "4'lü ♠ var mı?" sorusu
    ///   3♠   = "4'lü ♥ var mı?" sorusu
    ///   3NT  = Sign-off
    ///   4♣   = 4-4 majör, slam ilgisi
    ///   4♦   = 4-4 majör, game seviyesi
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

            // ── DURUM 1: Partner 2NT açtı, ben 3♣ soracağım ──
            if (durum.PartnerTeklifleri.Count > 0 &&
                durum.PartnerTeklifleri.Last() == "2NT" &&
                durum.KendiTeklifleri.Count == 0)
            {
                bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
                    durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
                if (rakipGercekTeklifVerdi) return false;

                int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                return maca >= 4 || kupa >= 4;
            }

            // ── DURUM 2: Partner 3♣ dedi (ben 2NT açmıştım), cevap vereceğim ──
            if (durum.SonGercekTeklif == "3♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("2NT"))
            {
                return true;
            }

            // ── DURUM 3: Partner 3♦ dedi, ben (3♣ soran) cevap vereceğim ──
            if (durum.SonGercekTeklif == "3♦" &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("3♣"))
            {
                return true;
            }

            // ── DURUM 4: Partner 3♥/3♠ dedi (5'li majör), ben (3♣ soran) cevap vereceğim ──
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

            // ── DURUM 1: Soru sorma (Partner 2NT açtı) ──
            if (durum.PartnerTeklifleri.LastOrDefault() == "2NT" &&
                durum.KendiTeklifleri.Count == 0)
            {
                return "3♣";
            }

            // ── DURUM 2: Cevap verme (Ben 2NT açmıştım, partner 3♣ dedi) ──
            if (durum.SonGercekTeklif == "3♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("2NT"))
            {
                if (kupa >= 5) return "3♥";
                if (maca >= 5) return "3♠";
                if (kupa == 4 || maca == 4) return "3♦";
                return "3NT";
            }

            // ── DURUM 3: 2. Tur cevap (Ben 3♣ sormuştum, partner 3♦ dedi) ──
            if (durum.SonGercekTeklif == "3♦" &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("3♣"))
            {
                bool dortluMacaVar = maca == 4;
                bool dortluKupaVar = kupa == 4;

                if (dortluMacaVar && dortluKupaVar)
                {
                    return (ElDegerlendirici.HCP(el) >= 16) ? "4♣" : "4♦";
                }

                if (dortluMacaVar && !dortluKupaVar) return "3♥"; // "4'lü ♠ var mı?" sorusu
                if (dortluKupaVar && !dortluMacaVar) return "3♠"; // "4'lü ♥ var mı?" sorusu

                return "3NT";
            }

            // ── DURUM 4: 5'li Majör cevabına karşılık ──
            if ((durum.SonGercekTeklif == "3♥" || durum.SonGercekTeklif == "3♠") &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("3♣"))
            {
                string okunan = durum.SonGercekTeklif == "3♥" ? "Kupa" : "Maça";
                if (ElDegerlendirici.RenkUzunlugu(el, okunan) >= 3)
                {
                    return okunan == "Kupa" ? "4♥" : "4♠";
                }
                return "3NT";
            }

            return "Pas";
        }
    }
}
