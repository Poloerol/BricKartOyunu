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
        public int Oncelik => 13;

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
            // DURUM 4: Ben 3♣ sormuştum, partner 3♥/3♠ (5'li majör) dedi
            // ⚠️ Sadece tek turlu senaryo için
            // ─────────────────────────────────────────────────────
            if ((durum.SonGercekTeklif == "3♥" || durum.SonGercekTeklif == "3♠") &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("3♣") &&
                !durum.KendiTeklifleri.Contains("3♥") &&
                !durum.KendiTeklifleri.Contains("3♠"))
            {
                return true;   // ← SADECE BU!
            }

            // ─────────────────────────────────────────────────────
            // DURUM 5: Partner 3♥/3♠ (Aşama 3) — Açıcı cevap veriyor
            // Senaryo: 2NT - 3♣ - 3♦ - 3♥ (partner "4'lü ♠ var mı?" sordu)
            //          Ben 2NT açan olarak 3♠ (tutuş var) veya 3NT (yok) diyeceğim
            // ─────────────────────────────────────────────────────
            if (durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("2NT") &&
                durum.KendiTeklifleri.Contains("3♦"))
            {
                // Partner 3♥ veya 3♠ dedi mi?
                if (durum.SonGercekTeklif == "3♥" || durum.SonGercekTeklif == "3♠")
                    return true;
            }

            // ─────────────────────────────────────────────────────
            // DURUM 6: Partner 3♠/4♥ (Aşama 4) — Cevapçı kontrol gösterecek
            // Senaryo: 2NT - 3♣ - 3♦ - 3♥ (partner "4'lü ♠ var mı?" sordu)
            //          Ben 3♠ (tutuş var) dedim
            //          Şimdi partner kontrol gösterecek
            // ─────────────────────────────────────────────────────
            if (durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("3♣") &&
                durum.KendiTeklifleri.Contains("3♥"))  // veya 3♠
            {
                // Partner 3♠ (benim 3♥'ye cevap) veya 4♥ (benim 3♠'ye cevap) dedi mi?
                if ((durum.KendiTeklifleri.Contains("3♥") && durum.SonGercekTeklif == "3♠") ||
                    (durum.KendiTeklifleri.Contains("3♠") && durum.SonGercekTeklif == "4♥"))
                    return true;
            }

            // ─────────────────────────────────────────────────────
            // DURUM 7: Partner 4♣ dedi (6 Key-Card sorusu), ben cevap vereceğim
            // Senaryo: 2NT - 3♣ - 3♦ - 4♣ - ?
            // ─────────────────────────────────────────────────────
            if (durum.SonGercekTeklif == "4♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("2NT") &&
                durum.KendiTeklifleri.Contains("3♦"))
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
            // ⚠️ Sadece tek turlu senaryo için
            // ─────────────────────────────────────────────────────
            if ((durum.SonGercekTeklif == "3♥" || durum.SonGercekTeklif == "3♠") &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("3♣") &&
                !durum.KendiTeklifleri.Contains("3♥") &&
                !durum.KendiTeklifleri.Contains("3♠"))
            {
                // "Okunan majörde" fit var mı?
                string okunan = durum.SonGercekTeklif == "3♥" ? "Kupa" : "Maça";
                int fit = ElDegerlendirici.RenkUzunlugu(el, okunan);

                // Fit VAR (3+ kart) → kontrol göster
                if (fit >= 3)
                {
                    // Önce ♣ kontrolü var mı?
                    if (PapazVeyaAsVarMi(el, "Sinek"))
                        return "4♣";

                    // Sonra ♦ kontrolü var mı?
                    if (PapazVeyaAsVarMi(el, "Karo"))
                        return "4♦";

                    // Okunan ♠ ise ♥ kontrolüne bak
                    if (okunan == "Maça" && PapazVeyaAsVarMi(el, "Kupa"))
                        return "4♥";

                    // Okunan ♥ ise ♠ kontrolüne bak
                    if (okunan == "Kupa" && PapazVeyaAsVarMi(el, "Maça"))
                        return "4♠";

                    // Hiç kontrol yok → sign-off
                    return okunan == "Kupa" ? "4♥" : "4♠";
                }

                // Fit YOK → sign-off
                return "3NT";
            }

            // ─────────────────────────────────────────────────────
            // DURUM 5: Partner 3♥/3♠ (Aşama 3) — Açıcı cevap veriyor
            // ─────────────────────────────────────────────────────
            if (durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("2NT") &&
                durum.KendiTeklifleri.Contains("3♦") &&
                (durum.SonGercekTeklif == "3♥" || durum.SonGercekTeklif == "3♠"))
            {
                // Partner 3♥ dedi → "4'lü ♠ var mı?" soruyor
                if (durum.SonGercekTeklif == "3♥")
                {
                    // Bende 4'lü ♠ var mı?
                    if (maca == 4)
                        return "3♠";  // Tutuş VAR
                    else
                        return "3NT"; // Tutuş YOK
                }

                // Partner 3♠ dedi → "4'lü ♥ var mı?" soruyor
                if (durum.SonGercekTeklif == "3♠")
                {
                    // Bende 4'lü ♥ var mı?
                    if (kupa == 4)
                        return "4♥";  // Tutuş VAR
                    else
                        return "3NT"; // Tutuş YOK
                }
            }

            // ─────────────────────────────────────────────────────
            // DURUM 6: Partner 3♠/4♥ (Aşama 4) — Ben kontrol göstereceğim
            // ─────────────────────────────────────────────────────
            if (durum.SonGercekTeklifSahibi == durum.Partner &&
                ((durum.KendiTeklifleri.Contains("3♥") && durum.SonGercekTeklif == "3♠") ||
                 (durum.KendiTeklifleri.Contains("3♠") && durum.SonGercekTeklif == "4♥")))
            {
                // Okunan renk hangisi?
                string okunanRenk = durum.KendiTeklifleri.Contains("3♥") ? "Maça" : "Kupa";

                // Önce ♣ kontrolü var mı?
                if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 1 &&
                    (ElDegerlendirici.AsSayisi(el.Where(c => c.Suit == "Sinek").ToList()) > 0 ||
                     ElDegerlendirici.PapazVar(el, "Sinek")))
                    return "4♣";

                // Sonra ♦ kontrolü var mı?
                if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 1 &&
                    (ElDegerlendirici.AsSayisi(el.Where(c => c.Suit == "Karo").ToList()) > 0 ||
                     ElDegerlendirici.PapazVar(el, "Karo")))
                    return "4♦";

                // Sonra ♥ kontrolü var mı? (eğer okunan ♠ ise)
                if (okunanRenk == "Maça" &&
                    (ElDegerlendirici.AsSayisi(el.Where(c => c.Suit == "Kupa").ToList()) > 0 ||
                     ElDegerlendirici.PapazVar(el, "Kupa")))
                    return "4♥";

                // Hiç kontrol yoksa → RKCB
                return "4NT";
            }

            // ─────────────────────────────────────────────────────
            // DURUM 7: Partner 4♣ dedi (6 Key-Card sorusu), ben cevap veriyorum
            // ─────────────────────────────────────────────────────
            if (durum.SonGercekTeklif == "4♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner &&
                durum.KendiTeklifleri.Contains("2NT") &&
                durum.KendiTeklifleri.Contains("3♦"))
            {
                // 6 Key-Card = 4 As + 2 majör K
                int asSayisi = ElDegerlendirici.AsSayisi(el);
                int majörPapazSayisi = 0;
                if (ElDegerlendirici.PapazVar(el, "Maça")) majörPapazSayisi++;
                if (ElDegerlendirici.PapazVar(el, "Kupa")) majörPapazSayisi++;

                int keyCardSayisi = asSayisi + majörPapazSayisi;

                // Majör Dam sayısı (♠ Q + ♥ Q)
                int majörDamSayisi = 0;
                if (ElDegerlendirici.KizVar(el, "Maça")) majörDamSayisi++;
                if (ElDegerlendirici.KizVar(el, "Kupa")) majörDamSayisi++;

                switch (keyCardSayisi)
                {
                    case 0:
                    case 1:
                        return "4♦";  // 0-1 Key-Card (nadir)
                    case 2:
                        return "4♦";  // 2 Key-Card
                    case 3:
                        return "4♠";  // 3 Key-Card
                    case 4:
                        // 4 Key-Card → Dam sayısına göre
                        if (majörDamSayisi >= 2) return "5♦";  // 4 + 2 Q
                        if (majörDamSayisi == 1) return "5♣";  // 4 + 1 Q
                        return "4NT";  // 4 Key-Card (Dam yok)
                    case 5:
                        return "5♦";  // 5 Key-Card (nadir)
                    case 6:
                        return "5♦";  // 6 Key-Card (nadir)
                    default:
                        return "4♦";
                }
            }

            return "Pas";
        }
        /// <summary>
        /// Belirtilen renkte Papaz veya As var mı?
        /// </summary>
        private bool PapazVeyaAsVarMi(List<Card> el, string renk)
        {
            return el.Any(c => c.Suit == renk && (c.Value == 14 || c.Value == 13));
        }
    }
}