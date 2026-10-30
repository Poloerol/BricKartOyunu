using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Namyats (Güçlü Majör Açılışları)
    /// 
    /// 4♣ = güçlü 4♥ açılışı (uzun ve sağlam kör)
    /// 4♦ = güçlü 4♠ açılışı (uzun ve sağlam pik)
    /// 
    /// Amaç: doğrudan 4♥/4♠ açmaya çok güçlü ama 2♣ açmaya yeterince
    /// güçlü olmayan elleri tanımlamak. Ortak, slam ilgisi gösterebilir.
    /// 
    /// "Güçlü" = 8+ löve, sağlam majör rengi (en fazla 1 kayıp),
    /// yan renklerde en fazla 1 As.
    /// 
    /// 4♥/4♠ normal preemptif açılışlar (Preempt.cs) olarak kalır.
    /// 
    /// ÖNEMLİ: Bu konvansiyon aktifken, Preempt.cs'teki 4♣/4♦
    /// devre dışı kalır (OrtaklikAnlasmasi.Namyats = true).
    /// </summary>
    public class Namyats : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════
        // IKonvansiyon ÜYELERİ
        // ═══════════════════════════════════════════════════════════════

        public string Ad => "Namyats";
        public string Aciklama => "Güçlü 4♣/4♦ majör açılışı";
        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 7: Preempt (9) ve Zayıf İki (8) öncesi çalışsın.
        /// Çünkü 4♣/4♦ Preempt'ten önce kontrol edilmeli.
        /// </summary>
        public int Oncelik => 7;

        // ═══════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;

            // ─── DURUM 1: Açılış (geçmiş boş) ────────────────────────────
            if (durum.Gecmis == null || durum.Gecmis.Count == 0)
            {
                if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13)
                    return false;

                int kupa = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Kupa");
                int maca = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Maça");
                if (kupa < 8 && maca < 8) return false;

                int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
                if (hp < 10 || hp > 15) return false;

                return true;
            }

            // ─── DURUM 2: Cevap (partner 4♣/4♦ açtı, sıra bende) ────────
            if (PartnerNamyatsActiMi(durum))
                return true;

            // ─── DURUM 3: Ara adım sonrası açıcı tekrar konuşuyor ───────
            if (AraAdimSonrasiAciyorMu(durum))
                return true;

            return false;
        }
        private static bool PartnerNamyatsActiMi(IhaleDurumu durum)
        {
            if (durum.Gecmis == null || durum.Gecmis.Count == 0)
                return false;

            var partnerTeklifleri = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .ToList();

            if (partnerTeklifleri.Count == 0) return false;

            string sonPartnerTeklifi = partnerTeklifleri.Last().Teklif;

            if (sonPartnerTeklifi != "4♣" && sonPartnerTeklifi != "4♦")
                return false;

            var sonGercekTeklif = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .LastOrDefault();

            if (sonGercekTeklif == null) return false;
            if (sonGercekTeklif.Oyuncu != durum.Partner) return false;

            // Partnerin İLK Namyats teklifi mi?
            if (partnerTeklifleri.Count == 1) return true;

            return false;
        }

        private static bool AraAdimSonrasiAciyorMu(IhaleDurumu durum)
        {
            if (durum.Gecmis == null || durum.Gecmis.Count < 3)
                return false;

            var ilkNamyats = durum.Gecmis
                .FirstOrDefault(h => h.GercekTeklifMi
                                  && (h.Teklif == "4♣" || h.Teklif == "4♦"));

            if (ilkNamyats == null) return false;

            if (durum.AktifOyuncu != ilkNamyats.Oyuncu) return false;

            string beklenenAraAdim = ilkNamyats.Teklif == "4♣" ? "4♦" : "4♥";

            bool araAdimVar = durum.Gecmis.Any(h =>
                h.GercekTeklifMi
                && h.Teklif == beklenenAraAdim
                && h.Oyuncu != ilkNamyats.Oyuncu
                && h.Sira > ilkNamyats.Sira);

            return araAdimVar;
        }
        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            // ─── DURUM 1: Açılış ─────────────────────────────────────────
            if (durum.Gecmis == null || durum.Gecmis.Count == 0)
            {
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");

                if (kupa >= 8 && kupa >= maca) return "4♣";
                if (maca >= 8) return "4♦";
                return "Pas";
            }

            // ─── DURUM 2: Cevap (partner 4♣/4♦ açtı, sıra bende) ────────
            if (PartnerNamyatsActiMi(durum))
                return CevapVer(durum, el);

            // ─── DURUM 3: Ara adım sonrası açıcı tekrar konuşuyor ───────
            if (AraAdimSonrasiAciyorMu(durum))
                return AraAdimSonrasiAciyorCevap(durum, el);

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI: DURUM 2 — Cevap Ver (ortağın 4♣/4♦'suna)
        // ═══════════════════════════════════════════════════════════════════

        private string CevapVer(IhaleDurumu durum, List<Card> el)
        {
            // Partnerin son teklifi: 4♣ (güçlü ♥) mı, 4♦ (güçlü ♠) mı?
            string partnerTeklif = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .Last().Teklif;

            bool kupaKoz = partnerTeklif == "4♣";   // 4♣ = güçlü ♥
            string koz = kupaKoz ? "Kupa" : "Maça";

            int kozUzunluk = ElDegerlendirici.RenkUzunlugu(el, koz);
            int hp = ElDegerlendirici.HCP(el);
            int asSayisi = ElDegerlendirici.AsSayisi(el);

            // ─── 1. Blackwood: 3+ As ve kozda 2+ kart ────────────────────
            if (asSayisi >= 3 && kozUzunluk >= 2)
                return "4NT";

            // ─── 2. Sign-off: 0-7 HP (slam ilgisi yok) ────────────────────
            if (hp <= 7)
                return kupaKoz ? "4♥" : "4♠";

            // ─── 3. Direkt slam: 16+ HP + 2+ As ──────────────────────────
            if (hp >= 16 && asSayisi >= 2)
                return kupaKoz ? "6♥" : "6♠";

            // ─── 4. Ara adım: 8+ HP (slam ilgisi var) ────────────────────
            // 4♣ → 4♦, 4♦ → 4♥
            if (hp >= 8)
                return kupaKoz ? "4♦" : "4♥";

            // ─── 5. Fallback: sign-off ───────────────────────────────────
            return kupaKoz ? "4♥" : "4♠";
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI: DURUM 3 — Ara Adım Sonrası Açıcının Cevabı
        // ═══════════════════════════════════════════════════════════════════

        private string AraAdimSonrasiAciyorCevap(IhaleDurumu durum, List<Card> el)
        {
            // İlk Namyats teklifini bul (4♣ veya 4♦)
            var ilkNamyats = durum.Gecmis
                .FirstOrDefault(h => h.GercekTeklifMi
                                  && (h.Teklif == "4♣" || h.Teklif == "4♦"));

            if (ilkNamyats == null) return "Pas";

            bool kupaKoz = ilkNamyats.Teklif == "4♣";
            string koz = kupaKoz ? "Kupa" : "Maça";
            string digerMajor = kupaKoz ? "Maça" : "Kupa";

            int hp = ElDegerlendirici.HCP(el);
            int asSayisi = ElDegerlendirici.AsSayisi(el);

            // ─── 1. Grand Slam Force: 13+ HP + 2+ As ─────────────────────
            if (hp >= 13 && asSayisi >= 2)
                return "5NT";

            // ─── 2. Kontrol göstergesi ───────────────────────────────────
            // Diğer majör kontrolü (A veya K)
            bool digerMajorKontrol = el.Any(c =>
                c.Suit == digerMajor && (c.Value == 14 || c.Value == 13));
            if (digerMajorKontrol)
                return kupaKoz ? "5♠" : "5♥";

            // ♦ kontrolü (A veya K)
            bool karoKontrol = el.Any(c =>
                c.Suit == "Karo" && (c.Value == 14 || c.Value == 13));
            if (karoKontrol) return "5♦";

            // ♣ kontrolü (A veya K)
            bool sinekKontrol = el.Any(c =>
                c.Suit == "Sinek" && (c.Value == 14 || c.Value == 13));
            if (sinekKontrol) return "5♣";

            // ─── 3. Minimum: kozu tekrar et (sign-off) ───────────────────
            return kupaKoz ? "4♥" : "4♠";
        }
    }
}