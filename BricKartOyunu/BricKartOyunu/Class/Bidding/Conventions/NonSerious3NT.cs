using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Non-Serious 3NT (Frivolous 3NT)
    /// 
    /// Serious 3NT'nin tersi mantık:
    ///   - 3NT = "slam için ciddi DEĞİLİM, minimum elim"
    ///   - Cue-bid = "slam için ciddiyim"
    /// 
    /// Teorik olarak bilgi sızıntısını önlediği için daha üstündür.
    /// Ancak bazı ortaklıklar Serious 3NT'yi tercih eder.
    /// 
    /// Örnek sekanslar:
    ///   1♠ - 2♣ - 2♠ - 3♠ - 3NT = Non-Serious (minimum)
    ///   1♠ - 2NT (Jacoby) - 3♣ - 3NT = Non-Serious
    /// 
    /// Öncelik: 24 (Serious3NT ile aynı; ikisi aynı anda aktif olamaz)
    /// </summary>
    public class NonSerious3NT : IKonvansiyon
    {
        public string Ad => "Non-Serious 3NT";
        public string Aciklama => "Minimum el gösteren 3NT";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 24;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // ─── Anlaşma aktif mi? ────────────────────────────────────
            if (!durum.Anlasma.NonSerious3NT) return false;

            // ─── Serious3NT ile çakışma olmasın ───────────────────────
            if (durum.Anlasma.Serious3NT) return false;

            // ─── Rakip müdahalesi olmamalı ────────────────────────────
            bool rakipMudahalesiVar = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipMudahalesiVar) return false;

            // ─── 3NT henüz teklif edilmedi ────────────────────────────
            if (durum.Gecmis.Any(h => h.GercekTeklifMi && h.Teklif == "3NT"))
                return false;

            // ─── RKCB/Kickback/GSF sekansı başlamadı ──────────────────
            if (durum.Gecmis.Any(h =>
                h.GercekTeklifMi && h.Teklif == "4NT"))
                return false;

            // ─── Game forcing durum ───────────────────────────────────
            if (!GameForcingMi(durum)) return false;

            // ─── 8+ kart majör fit ────────────────────────────────────
            string majör = MajörFitBul(durum);
            if (majör == null) return false;

            // ─── Fit 3. seviyede gösterilmiş ──────────────────────────
            if (!FitUcSeviyedeGosterildi(durum, majör)) return false;

            // ─── Sıra bizde, son teklif partnerimizin ─────────────────
            var sonGercek = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .LastOrDefault();
            if (sonGercek == null) return false;
            if (sonGercek.Oyuncu != durum.Partner) return false;

            // ─── Non-Serious için el 11-13 HP arası (minimum) ─────────
            int hcp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hcp < 11 || hcp > 14) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "3NT";
        }

        // ─── Yardımcı metotlar (Serious3NT ile aynı) ──────────────────
        // (Serious3NT.cs'teki private metotları kopyalayın veya
        //  ortak bir yardımcı sınıfta toplayın)

        private static bool GameForcingMi(IhaleDurumu durum)
        {
            if (durum.Gecmis == null || durum.Gecmis.Count < 2) return false;

            var gercelTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            if (gercelTeklifler.Count < 2) return false;

            var acilis = gercelTeklifler[0];
            var cevap = gercelTeklifler[1];

            if (acilis.Seviye == 1 &&
                (acilis.Koz == "Maça" || acilis.Koz == "Kupa") &&
                cevap.Seviye == 2 &&
                cevap.Koz != acilis.Koz)
                return true;

            if (acilis.Seviye == 1 &&
                (acilis.Koz == "Maça" || acilis.Koz == "Kupa") &&
                cevap.Teklif == "2NT")
                return true;

            if (acilis.Teklif == "2♣") return true;

            return false;
        }

        private static string MajörFitBul(IhaleDurumu durum)
        {
            int macaOyuncu = ElDegerlendirici.RenkUzunlugu(
                durum.AktifOyuncuEli, "Maça");
            int kupaOyuncu = ElDegerlendirici.RenkUzunlugu(
                durum.AktifOyuncuEli, "Kupa");

            var partnerTeklifleri = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .ToList();

            bool partnerMaca = partnerTeklifleri.Any(h => h.Koz == "Maça");
            bool partnerKupa = partnerTeklifleri.Any(h => h.Koz == "Kupa");

            if (partnerMaca && macaOyuncu >= 4) return "Maça";
            if (partnerKupa && kupaOyuncu >= 4) return "Kupa";

            return null;
        }

        private static bool FitUcSeviyedeGosterildi(
            IhaleDurumu durum, string majör)
        {
            string sembol = KozYardimcisi.KozSembolu(majör);

            var sonTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .Reverse().Take(4).Reverse()
                .ToList();

            return sonTeklifler.Any(h =>
                h.Teklif == $"3{sembol}" &&
                (h.Oyuncu == durum.AktifOyuncu || h.Oyuncu == durum.Partner));
        }
    }
}