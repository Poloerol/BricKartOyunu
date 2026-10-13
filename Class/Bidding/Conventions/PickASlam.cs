using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 5NT Pick-a-Slam
    /// 
    /// "Slam oynayacağız ama nerede oynayacağımızı bilmiyorum,
    /// sen en iyi rengi seç" mesajı verir.
    /// 
    /// Tetikleyici: 5NT sıçramalı (geçmişte 4NT/Kickback YOK, 
    ///              GSF/KozQ sekansı YOK).
    /// 
    /// Cevap şeması:
    ///   - En iyi minör (veya majör) slam'i seç
    ///   - 6NT = "fark etmez, NT'de oynayalım"
    /// 
    /// Örnek sekanslar:
    ///   1♦ - 1♥ - 3♦ - 5NT = Pick-a-Slam
    ///   1NT - 2♣ - 2♦ - 5NT = Pick-a-Slam
    ///   1♣ - 1♠ - 2♣ - 5NT = Pick-a-Slam
    /// 
    /// Öncelik: 25 (GSF 26 ve KozQ 27 öncesi)
    /// </summary>
    public class PickASlam : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════
        // IKonvansiyon ÜYELERİ
        // ═══════════════════════════════════════════════════════════════

        public string Ad => "Pick-a-Slam";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 25;

        // ═══════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // ═══════════════════════════════════════════════════════════════
            // DURUM 1: Partner Pick-a-Slam yaptı → biz cevap veriyoruz
            // ═══════════════════════════════════════════════════════════════
            if (PartnerPickASlamYaptiMi(durum))
                return true;

            // ═══════════════════════════════════════════════════════════════
            // DURUM 2: Biz Pick-a-Slam yapacağız
            // ═══════════════════════════════════════════════════════════════

            // ─── 1. GSF/KozQ/RKCB sekansı olmamalı ───────────────────────
            if (GsfVeyaKozQSekansiVar(durum)) return false;

            // ─── 2. 5NT daha önce kullanılmadı ───────────────────────────
            if (durum.KendiTeklifleri.Contains("5NT")) return false;

            // ─── 3. 4NT veya Kickback kullanılmadı ───────────────────────
            if (durum.KendiTeklifleri.Contains("4NT")) return false;
            if (durum.KendiTeklifleri.Contains("4♠") ||
                durum.KendiTeklifleri.Contains("4♥") ||
                durum.KendiTeklifleri.Contains("4♦"))
                return false;

            // ─── 4. Geçmişte 5♣/5♦/5♥/5♠ RKCB cevabı yok ───────────────
            if (durum.Gecmis.Any(h =>
                h.GercekTeklifMi &&
                (h.Teklif == "5♣" || h.Teklif == "5♦" ||
                 h.Teklif == "5♥" || h.Teklif == "5♠")))
                return false;

            // ─── 5. Geçmişte 4NT sorusu yok ──────────────────────────────
            if (durum.Gecmis.Any(h => h.GercekTeklifMi && h.Teklif == "4NT"))
                return false;

            // ─── 6. Rakip müdahalesi olmamalı ────────────────────────────
            bool rakipMudahalesiVar = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipMudahalesiVar) return false;

            // ─── 7. (Kaldırıldı: Pas sonrasında KacinciSeviye() 0 döndüğü için engelliyordu) ───

            // ─── 8. En az 3 gerçek teklif olmalı ─────────────────────────
            var gercelTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();
            if (gercelTeklifler.Count < 3) return false;

            // ─── 9. Son teklif bizim veya partnerimizin olmalı ───────────
            var sonTeklif = gercelTeklifler.Last();
            if (sonTeklif.Oyuncu != durum.AktifOyuncu &&
                sonTeklif.Oyuncu != durum.Partner)
                return false;

            // ─── 10. Son teklif 3. seviyede bir renk olmalı ──────────────
            if (sonTeklif.Seviye != 3) return false;
            if (string.IsNullOrEmpty(sonTeklif.Koz)) return false;

            // ─── 11. Son teklif, DAHA ÖNCE teklif edilmiş bir renk olmalı
            string sonKoz = sonTeklif.Koz;
            bool dahaOnceTeklifEdilmis = durum.Gecmis.Any(h =>
                h.GercekTeklifMi &&
                h.Sira < sonTeklif.Sira &&
                h.Koz == sonKoz);

            if (!dahaOnceTeklifEdilmis) return false;

            // ─── 12. En az 1 minör 4+ kart VEYA her iki majör 4+ kart ─────
            // Pick-a-Slam "hangi slam?" sorusudur. Cevapçının elinde:
            //   • 4+ bir minör varsa → minör slama yönelebilir
            //   • 4-4 (veya 5-4) her iki majör varsa → majör slam mümkün
            // Aksi halde (örn. 6-3-2-2) Pick-a-Slam anlamsızdır.
            int karo = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Sinek");
            int kupa = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Kupa");
            int maca = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Maça");

            bool minorVar = (karo >= 4 || sinek >= 4);
            bool ikiMajorVar = (kupa >= 4 && maca >= 4);

            if (!minorVar && !ikiMajorVar) return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            // ─── Cevap: Partner Pick-a-Slam yaptı ────────────────────
            if (PartnerPickASlamYaptiMi(durum))
            {
                return PickASlamCevabi(el);
            }

            // ─── Soru: Biz 5NT diyeceğiz ─────────────────────────────
            return "5NT";
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Partner Pick-a-Slam Yaptı mı?
        // ═══════════════════════════════════════════════════════════════

        private static bool PartnerPickASlamYaptiMi(IhaleDurumu durum)
        {
            if (durum.Gecmis == null || durum.Gecmis.Count == 0)
                return false;

            var partnerTeklifleri = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .ToList();

            if (partnerTeklifleri.Count == 0) return false;

            string sonPartnerTeklifi = partnerTeklifleri.Last().Teklif;
            if (sonPartnerTeklifi != "5NT") return false;

            // Partner hâlâ son gerçek teklifi veren mi?
            var sonGercek = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .LastOrDefault();
            if (sonGercek == null) return false;
            if (sonGercek.Oyuncu != durum.Partner) return false;

            // GSF/KozQ sekansı olmamalı
            if (GsfVeyaKozQSekansiVar(durum)) return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: GSF/KozQ Sekansı Var mı?
        // ═══════════════════════════════════════════════════════════════

        private static bool GsfVeyaKozQSekansiVar(IhaleDurumu durum)
        {
            if (durum.Gecmis == null) return false;

            // 4NT veya Kickback var mı?
            bool rkcbSoruVar = durum.Gecmis.Any(h =>
                h.GercekTeklifMi &&
                (h.Teklif == "4NT" || h.Teklif == "4♠" ||
                 h.Teklif == "4♥" || h.Teklif == "4♦"));

            // 5♣/5♦/5♥/5♠ RKCB cevabı var mı?
            bool rkcbCevapVar = durum.Gecmis.Any(h =>
                h.GercekTeklifMi &&
                (h.Teklif == "5♣" || h.Teklif == "5♦" ||
                 h.Teklif == "5♥" || h.Teklif == "5♠"));

            // İkisi de varsa → GSF/KozQ sekansı
            if (rkcbSoruVar && rkcbCevapVar) return true;

            // 4NT var ama 5 seviyesi cevap yoksa (sorulmuş ama cevap yok)
            if (rkcbSoruVar && !rkcbCevapVar) return false;

            return false;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Pick-a-Slam Cevabı
        // ═══════════════════════════════════════════════════════════════

        private static string PickASlamCevabi(List<Card> el)
        {
            // En iyi minör rengi seç (4+ kart)
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");

            // Öncelik sırası: ♣ → ♦ → ♥ → ♠
            // (En az 4'lü olan en uzun rengi seç)

            int enIyiMinörUzunluk = 0;
            string enIyiMinör = null;

            if (sinek >= 4 && sinek > enIyiMinörUzunluk)
            {
                enIyiMinörUzunluk = sinek;
                enIyiMinör = "Sinek";
            }
            if (karo >= 4 && karo > enIyiMinörUzunluk)
            {
                enIyiMinörUzunluk = karo;
                enIyiMinör = "Karo";
            }

            // Minör bulunduysa onu oyna
            if (enIyiMinör != null)
            {
                string sembol = KozYardimcisi.KozSembolu(enIyiMinör);
                return $"6{sembol}";
            }

            // Minör yoksa, en uzun majörü seç
            int enIyiMajorUzunluk = 0;
            string enIyiMajor = null;

            if (kupa >= 4 && kupa > enIyiMajorUzunluk)
            {
                enIyiMajorUzunluk = kupa;
                enIyiMajor = "Kupa";
            }
            if (maca >= 4 && maca > enIyiMajorUzunluk)
            {
                enIyiMajorUzunluk = maca;
                enIyiMajor = "Maça";
            }

            if (enIyiMajor != null)
            {
                string sembol = KozYardimcisi.KozSembolu(enIyiMajor);
                return $"6{sembol}";
            }

            // Hiçbir renk 4+ değilse → 6NT
            return "6NT";
        }
    }
}