using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Pick-a-Slam konvansiyonu (5NT).
    ///
    /// Sekans: 1♦ - 1♥ - 3♦ - Pas - ?
    ///   Açıcının 3. seviye minör sıçrama rebid'i 16+ HCP ve 6+ renk gösterir.
    ///   Cevapçı 5NT ile "hangi slama gideceğiz?" diye sorar.
    ///   Açıcı en uzun minörünü (yoksa majörünü) 6 seviyesinde gösterir.
    ///
    /// ÖNEMLİ: RKCB / Kickback / Blackwood / Gerber sekanslarındaki 5NT bu
    /// konvansiyona ait DEĞİLDİR (Koz Q Sorusu / Grand Slam Force'a aittir).
    /// Önceliği 25 olduğu için (KozQSorusu = 27) bu ayrım UygunMu'da yapılmalı.
    ///
    /// Öncelik: 25
    /// </summary>
    public class PickASlam : IKonvansiyon
    {
        public string Ad => "Pick-a-Slam";
        public string Aciklama => "5NT ile slam seçimi";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 25;

        // ═══════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            var el = durum.AktifOyuncuEli;
            if (el == null || el.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Partner 1NT açtıysa Pick-a-Slam kullanılmaz
            if (durum.PartnerTeklifleri.Contains("1NT")) return false;

            // RKCB / Kickback / Blackwood sekansı → Koz Q Sorusu'na bırak
            if (AsSorusuSekansiVar(durum)) return false;

            // Cevap aşaması: partner 5NT dedi
            if (PartnerSordu(durum)) return true;

            // Soru aşaması: biz 5NT diyeceğiz
            return SoruAsamasi(durum, el);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;

            // Cevap: partner 5NT dedi
            if (PartnerSordu(durum))
            {
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

                // En uzun minör (eşitse Sinek)
                if (sinek >= karo && sinek >= 4) return "6♣";
                if (karo >= sinek && karo >= 4) return "6♦";

                // Minör yoksa en uzun majör
                if (macca >= kupa && macca >= 4) return "6♠";
                if (kupa >= macca && kupa >= 4) return "6♥";

                return "6NT";
            }

            // Soru
            return "5NT";
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCILAR
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Gecmiş'te 4NT veya 5 seviyesinde bir renk teklifi varsa bu bir
        /// as sorma (RKCB/Kickback/Blackwood) sekansıdır.
        /// </summary>
        private static bool AsSorusuSekansiVar(IhaleDurumu durum)
        {
            if (durum.Gecmis == null) return false;

            return durum.Gecmis.Any(h =>
                h.GercekTeklifMi &&
                (h.Teklif == "4NT" ||
                 h.Teklif == "5♣" || h.Teklif == "5♦" ||
                 h.Teklif == "5♥" || h.Teklif == "5♠"));
        }

        /// <summary>
        /// Biz 3. seviyede (sıçrama) bir renk teklifi vermişiz mi?
        /// (Açıcının 3♦/3♣ rebid'i.)
        /// </summary>
        private static bool KendiUcSeviyeTeklifimizVar(IhaleDurumu durum)
        {
            return durum.KendiTeklifleri.Any(t =>
                KozYardimcisi.TekliftenSeviyeCikar(t) == 3 &&
                !string.IsNullOrEmpty(KozYardimcisi.TekliftenKozCikar(t)) &&
                KozYardimcisi.TekliftenKozCikar(t) != "NT");
        }

        /// <summary>
        /// Partner 5NT dedi ve bu 5NT bizim 3. seviye sıçrama rebid'imize
        /// verilmiş bir Pick-a-Slam sorusu mu?
        /// </summary>
        private static bool PartnerSordu(IhaleDurumu durum)
        {
            if (durum.SonGercekTeklif != "5NT") return false;
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;
            if (!KendiUcSeviyeTeklifimizVar(durum)) return false;
            return true;
        }

        /// <summary>
        /// 5NT sorusunu sorma koşulları:
        ///  - Partner açtı, son gerçek teklif partnerin 3♣/3♦ sıçrama rebid'i
        ///    (partner aynı minörü 1 seviyesinde açmış)
        ///  - Biz 5NT demedik
        ///  - 15+ HCP
        ///  - İki majör veya bir minörde 4+ kart
        /// </summary>
        private static bool SoruAsamasi(IhaleDurumu durum, List<Card> el)
        {
            if (durum.KendiTeklifleri.Contains("5NT")) return false;
            if (!durum.PartnerActiMi()) return false;

            if (string.IsNullOrEmpty(durum.SonGercekTeklif)) return false;
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;

            // Son gerçek teklif 3. seviyede bir minör olmalı (3♣ veya 3♦)
            int seviye = KozYardimcisi.TekliftenSeviyeCikar(durum.SonGercekTeklif);
            if (seviye != 3) return false;

            string sonKoz = KozYardimcisi.TekliftenKozCikar(durum.SonGercekTeklif);
            if (sonKoz != "Sinek" && sonKoz != "Karo") return false;

            // Partnerin ilk açılış teklifi 1 seviyesinde aynı minör olmalı
            if (durum.PartnerTeklifleri == null || durum.PartnerTeklifleri.Count == 0) return false;
            string ilk = durum.PartnerTeklifleri.First();
            if (KozYardimcisi.TekliftenSeviyeCikar(ilk) != 1) return false;
            if (KozYardimcisi.TekliftenKozCikar(ilk) != sonKoz) return false;

            // El gücü ve dağılım şartları (Pick-a-Slam için uygun el)
            if (ElDegerlendirici.HCP(el) < 13) return false;

            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            bool ikiMajor = macca >= 4 && kupa >= 4;
            bool minorVar = karo >= 4 || sinek >= 4;
            return ikiMajor || minorVar;
        }
    }
}