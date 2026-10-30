using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Serious 3NT — Eric Rodwell
    /// 
    /// Majör fit + game forcing durumda 3NT yapay olarak
    /// "ciddi slam ilgisi" gösterir.
    /// 
    /// Cue-bid'ler ise "slam için ciddi değilim, sadece kontrol
    /// gösteriyorum" anlamına gelir (partner ciddiyse devam eder).
    /// 
    /// Tetikleyici:
    ///   - Game forcing durum (2/1 veya Jacoby 2NT sonrası)
    ///   - 8+ kart majör fit kurulmuş
    ///   - Fit 3 seviyesinde gösterilmiş
    ///   - Her iki taraf da sınırsız (bir taraf 12-14 sınırlamamış)
    ///   - Rakip müdahalesi YOK
    ///   - RKCB/Kickback/GSF henüz başlamamış
    ///   - 3NT henüz teklif edilmemiş
    /// 
    /// Örnek sekanslar:
    ///   1♠ - 2♣ - 2♠ - 3♠ - 3NT = Serious (ciddi slam ilgisi)
    ///   1♠ - 2NT (Jacoby) - 3♣ - 3NT = Serious
    ///   1♥ - 2♦ - 3♥ - 3NT = Serious
    /// 
    /// Öncelik: 13 (CueBid 14 ve Blackwood/RKCB öncesi — 3NT, cue-bid'den önce gelir)
    /// </summary>
    public class Serious3NT : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════
        // IKonvansiyon ÜYELERİ
        // ═══════════════════════════════════════════════════════════════

        public string Ad => "Serious 3NT";
        public string Aciklama => "Ciddi slam ilgisi gösteren 3NT";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 13;   // CueBid (14) ÖNCESİ: 3NT cue-bid'den önce denenmeli

        // ═══════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // ─── 1. Anlaşma aktif mi? ─────────────────────────────────
            if (!durum.Anlasma.Serious3NT) return false;

            // ─── 2. Rakip müdahalesi olmamalı ─────────────────────────
            bool rakipMudahalesiVar = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipMudahalesiVar) return false;

            // ─── 3. 3NT henüz teklif edilmedi ─────────────────────────
            if (durum.KendiTeklifleri.Contains("3NT")) return false;
            if (durum.Gecmis.Any(h => h.GercekTeklifMi && h.Teklif == "3NT"))
                return false;

            // ─── 4. RKCB/Kickback/GSF sekansı başlamadı ────────────────
            if (GsfVeyaRkcSekansiVar(durum)) return false;

            // ─── 5. Game forcing durum olmalı ─────────────────────────
            if (!GameForcingMi(durum)) return false;

            // ─── 6. 8+ kart majör fit kurulmuş olmalı ─────────────────
            string majör = MajörFitBul(durum);
            if (majör == null) return false;

            // ─── 7. Fit 3 seviyesinde gösterilmiş olmalı ──────────────
            // (Son teklifimizin fit gösteren 3M olması gerekir)
            if (!FitUcSeviyedeGosterildi(durum, majör)) return false;

            // ─── 8. Her iki taraf da sınırsız olmalı ──────────────────
            if (SinirliElVar(durum)) return false;

            // ─── 9. Sıra bizde ve son teklif partnerimizin ────────────
            var sonGercek = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .LastOrDefault();
            if (sonGercek == null) return false;
            if (sonGercek.Oyuncu != durum.Partner) return false;

            // ─── 10. Aktif oyuncunun eli slam ilgisi için uygun ───────
            int hcp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hcp < 14) return false;  // Serious için minimum 14 HP

            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            return "3NT";
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Game Forcing Durum mu?
        // ═══════════════════════════════════════════════════════════════

        private static bool GameForcingMi(IhaleDurumu durum)
        {
            if (durum.Gecmis == null || durum.Gecmis.Count < 2) return false;

            var gercelTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            if (gercelTeklifler.Count < 2) return false;

            // ─── 2/1 Game Force tespiti ───────────────────────────────
            // Açılış 1M, cevap 2 seviyesinde yeni renk → GF
            var acilis = gercelTeklifler[0];
            var cevap = gercelTeklifler[1];

            if (acilis.Seviye == 1 &&
                (acilis.Koz == "Maça" || acilis.Koz == "Kupa") &&
                cevap.Seviye == 2 &&
                cevap.Koz != acilis.Koz)
            {
                return true;
            }

            // ─── Jacoby 2NT tespiti ───────────────────────────────────
            // 1M - 2NT → GF
            if (acilis.Seviye == 1 &&
                (acilis.Koz == "Maça" || acilis.Koz == "Kupa") &&
                cevap.Teklif == "2NT")
            {
                return true;
            }

            // ─── 2♣ Güçlü açılış ──────────────────────────────────────
            if (acilis.Teklif == "2♣")
                return true;

            // ─── 3. seviyede fit gösterimi zaten GF ───────────────────
            // 1M - 3M (Bergen hariç) → Bazı sistemlerde GF
            // Ancak Bergen aktifse bu zayıf olabilir
            // Basitlik için: fit 3 seviyesinde ise GF varsayıyoruz

            return false;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Majör Fit Bul
        // ═══════════════════════════════════════════════════════════════

        private static string MajörFitBul(IhaleDurumu durum)
        {
            // Aktif oyuncunun majör uzunluğu + partnerinin majör teklifi
            int macaOyuncu = ElDegerlendirici.RenkUzunlugu(
                durum.AktifOyuncuEli, "Maça");
            int kupaOyuncu = ElDegerlendirici.RenkUzunlugu(
                durum.AktifOyuncuEli, "Kupa");

            // Partner'in majör teklifi var mı?
            var partnerTeklifleri = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .ToList();

            bool partnerMaca = partnerTeklifleri.Any(h => h.Koz == "Maça");
            bool partnerKupa = partnerTeklifleri.Any(h => h.Koz == "Kupa");

            // 8+ kart fit kontrolü
            if (partnerMaca && macaOyuncu >= 4) return "Maça";
            if (partnerKupa && kupaOyuncu >= 4) return "Kupa";

            // Kendi majörümüzü teklif ettiysek ve partner destek verdiyse
            var kendiTeklifleri = durum.Gecmis
                .Where(h => h.Oyuncu == durum.AktifOyuncu && h.GercekTeklifMi)
                .ToList();

            bool kendiMaca = kendiTeklifleri.Any(h => h.Koz == "Maça");
            bool kendiKupa = kendiTeklifleri.Any(h => h.Koz == "Kupa");

            if (kendiMaca && partnerMaca && macaOyuncu >= 4) return "Maça";
            if (kendiKupa && partnerKupa && kupaOyuncu >= 4) return "Kupa";

            return null;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Fit 3. Seviyede Gösterildi mi?
        // ═══════════════════════════════════════════════════════════════

        private static bool FitUcSeviyedeGosterildi(
            IhaleDurumu durum, string majör)
        {
            string sembol = KozYardimcisi.KozSembolu(majör);

            // Son 4 teklif içinde 3M var mı?
            var sonTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .Reverse().Take(4).Reverse()
                .ToList();

            return sonTeklifler.Any(h =>
                h.Teklif == $"3{sembol}" &&
                (h.Oyuncu == durum.AktifOyuncu || h.Oyuncu == durum.Partner));
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Sınırlı El Var mı?
        // ═══════════════════════════════════════════════════════════════

        private static bool SinirliElVar(IhaleDurumu durum)
        {
            // 1NT açılış → 15-17 sınırlı
            // 2NT açılış → 20-21 sınırlı
            // Zayıf 2 → 6-10 sınırlı
            // Preempt → 5-9 sınırlı
            // Fit jump 3M (Bergen 3♣/3♦) → sınırlı
            var sinirliTeklifler = new[]
            {
                "1NT", "2NT", "2♥", "2♠", "3♣", "3♦", "3♥", "3♠"
            };

            // Açılış sınırlı mı?
            var ilkTeklif = durum.Gecmis
                .FirstOrDefault(h => h.GercekTeklifMi);

            if (ilkTeklif != null)
            {
                // 1NT açılış → sınırlı
                if (ilkTeklif.Teklif == "1NT") return true;
                if (ilkTeklif.Teklif == "2NT") return true;

                // Zayıf 2 (açılışta 2♥/2♠) → sınırlı
                if ((ilkTeklif.Teklif == "2♥" || ilkTeklif.Teklif == "2♠") &&
                    durum.Anlasma.ZayifIki)
                    return true;
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: GSF/RKCB Sekansı Var mı?
        // ═══════════════════════════════════════════════════════════════

        private static bool GsfVeyaRkcSekansiVar(IhaleDurumu durum)
        {
            if (durum.Gecmis == null) return false;

            // RKCB/Kickback/GSF sorusu veya cevabı var mı?
            bool rkcVar = durum.Gecmis.Any(h =>
                h.GercekTeklifMi &&
                (h.Teklif == "4NT" || h.Teklif == "5NT" ||
                 h.Teklif == "4♣" && h.Seviye == 4 && h.Koz == "Sinek" && false ||  // 4♣ Gerber olabilir, atla
                 h.Teklif == "5♣" && false));

            // 4NT Blackwood/RKCB sorusu → sekans başladı
            if (durum.Gecmis.Any(h =>
                h.GercekTeklifMi && h.Teklif == "4NT"))
                return true;

            // Kickback (kozun bir altındaki renkte 4 seviyesi)
            string koz = durum.AnlasilanKoz();
            if (koz != null && koz != "NT")
            {
                string kickback = KickbackTeklifi(koz);
                if (durum.Gecmis.Any(h =>
                    h.GercekTeklifMi && h.Teklif == kickback))
                    return true;
            }

            return false;
        }

        private static string KickbackTeklifi(string koz)
        {
            switch (koz)
            {
                case "Sinek": return "4♦";
                case "Karo": return "4♥";
                case "Kupa": return "4♠";
                case "Maça": return "4NT";
                default: return null;
            }
        }
    }
}