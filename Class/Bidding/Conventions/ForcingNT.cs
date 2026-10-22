using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Forcing NT — 2/1 sisteminde 1♥/1♠ açışına verilen 1NT cevabı.
    /// 
    /// Yapay ve forcing (1 tur). Cevapçıda:
    ///   - 5+ - 12 OP
    ///   - 3'lü majör desteği (davet amaçlı)
    ///   - 2 seviyesine çıkamayacak eller
    /// 
    /// Açıcının Forcing NT'ye rebidleri:
    ///   2♥/2♠ (aynı majör, 2 seviyesi) → 6'lı, min (12-15)
    ///   3♥/3♠ (aynı majör, 3 seviyesi) → 6'lı, 15-18 (zon forsing DEĞİL)
    ///   4♥/4♠                          → 7'li güzel, desteğe ihtiyaç yok
    ///   2♣/2♦                          → Doğal 3+ (nadiren 4+)
    ///   2NT                            → 18-19 dengeli, zon forsing
    ///   3♣/3♦ (sıçramalı)              → Zon forsing, 19+ OP
    ///   Yeni renk (2♥/2♠)              → Doğal, 5-4
    /// </summary>
    public class ForcingNT : IKonvansiyon
    {
        public string Ad => "Forcing NT";
        public string Aciklama => "Majör açılışına forcing 1NT cevabı";
        public bool AktifMi { get; set; } = true;

        // BesliMajor (10)'dan sonra, Stayman (11)'dan önce
        public int Oncelik => 11;

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.Gecmis == null) return false;
            if (durum.AktifOyuncuEli == null) return false;

            var gercekTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            if (gercekTeklifler.Count < 1) return false;

            // ─── SENARYO A: Cevapçının Forcing NT cevabı ───
            var sonTeklif = gercekTeklifler.Last();
            if (sonTeklif.Oyuncu == durum.Partner
                && sonTeklif.Seviye == 1
                && (sonTeklif.Koz == "Kupa" || sonTeklif.Koz == "Maça"))
            {
                // Rakip araya girmiş mi?
                if (durum.RakipActiMi()) return false;

                // ★ KRİTİK 1: Cevapçı daha önce PAS geçmiş mi?
                // Forcing NT passed hand için geçerli DEĞİL (Drury kullanılır)
                bool pasGectiMi = durum.Gecmis.Any(h =>
                    h.Oyuncu == durum.AktifOyuncu && h.PasMi);
                if (pasGectiMi) return false;

                // ★ KRİTİK 2: 3+ majör desteği varsa Forcing NT değil
                // (Basit destek veya Bergen)
                int destek = ElDegerlendirici.RenkUzunlugu(
                    durum.AktifOyuncuEli, sonTeklif.Koz);
                if (destek >= 3) return false;

                // ★ KRİTİK 3: 2NT davet aralığındaki dengeli el Forcing NT değil
                // (10-12 HP dengeli → 2NT davet)
                int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
                bool dengeli = ElDegerlendirici.DengeliEl(durum.AktifOyuncuEli);

                // 5+ - 12 HP aralığı
                if (hp < 5 || hp > 12) return false;

                // ★ KRİTİK 4: 10-12 HP dengeli el → 2NT davet (Forcing NT değil)
                // (ama 2'li majör ve dengeli ve 10-12 HP ise 2NT)
                // Bunu Forcing NT olarak bırakırsak 2NT'yi engelleriz.
                // 2NT davet sadece 10-12 HP dengeli ve 3'ten az majörde.
                // Ama Forcing NT de aynı koşulda tetiklenir.
                // 
                // Doğru davranış: 10-12 HP dengeli → Forcing NT (1NT) değil,
                // 2NT davet olmalı. Ama Forcing NT'nin doğası gereği 1NT yapaydır
                // ve daha sonra 2NT'ye yükseltilebilir.
                // 
                // Ancak test 5.2 "1♠ - ?" diziliminde 2NT bekliyor.
                // Basitleştirme: 10-12 HP dengeli ise Forcing NT'yi tetikleme,
                // bırak TemelMantik devralsın.
                if (dengeli && hp >= 10 && hp <= 12 && destek < 3)
                {
                    return false;
                }

                return true;
            }

            // ─── SENARYO B: Açıcının Forcing NT'ye rebidi ───
            if (gercekTeklifler.Count == 2)
            {
                var teklif2 = gercekTeklifler[1];
                var teklif1 = gercekTeklifler[0];

                if (teklif1.Oyuncu == durum.AktifOyuncu
                    && teklif1.Seviye == 1
                    && (teklif1.Koz == "Kupa" || teklif1.Koz == "Maça")
                    && teklif2.Oyuncu == durum.Partner
                    && teklif2.Teklif == "1NT")
                {
                    // ★ KRİTİK: Cevapçı pas geçmişse Forcing NT değil
                    bool cevapciPasGectiMi = durum.Gecmis.Any(h =>
                        h.Oyuncu == durum.Partner && h.PasMi);
                    if (cevapciPasGectiMi) return false;

                    return true;
                }
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VER
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null || el.Count != 13) return "Pas";

            var gercekTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            var sonTeklif = gercekTeklifler.Last();

            // Cevapçı mıyız? (1NT diyeceğiz)
            if (sonTeklif.Oyuncu == durum.Partner
                && sonTeklif.Seviye == 1
                && (sonTeklif.Koz == "Kupa" || sonTeklif.Koz == "Maça"))
            {
                return "1NT";
            }

            // Açıcı mıyız? (Forcing NT'ye rebid)
            return AciciCevabi(durum, el, gercekTeklifler);
        }

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ FORCING NT'YE REBİDİ
        // ═══════════════════════════════════════════════════════════════════

        private string AciciCevabi(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // Majör rengi bul
            string majör = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.AktifOyuncu && h.Seviye == 1
                    && (h.Koz == "Kupa" || h.Koz == "Maça"))
                {
                    majör = h.Koz;
                    break;
                }
            }
            if (string.IsNullOrEmpty(majör)) return "Pas";

            string sembol = KozYardimcisi.KozSembolu(majör);
            int majörUzunluk = ElDegerlendirici.RenkUzunlugu(el, majör);
            string digerMajör = (majör == "Maça") ? "Kupa" : "Maça";

            // ═══════════════════════════════════════════════════════════════
            // 1) Aynı majör tekrarı (6+ majör)
            // ═══════════════════════════════════════════════════════════════
            if (majörUzunluk >= 6)
            {
                // 4M: 7'li güzel majör, çok güçlü
                if (majörUzunluk >= 7 && hp >= 15)
                {
                    return "4" + sembol;
                }

                // 3M: 6'lı, 15-18 OP (zon forsing değil)
                if (hp >= 15 && hp <= 18)
                {
                    return "3" + sembol;
                }

                // 2M: 6'lı, min (12-15)
                return "2" + sembol;
            }

            // ═══════════════════════════════════════════════════════════════
            // 2) 2NT: 18-19 dengeli (5-3-3-2), zon forsing
            // ═══════════════════════════════════════════════════════════════
            if (dengeli && hp >= 18 && hp <= 19 && majörUzunluk == 5)
            {
                return "2NT";
            }

            // ═══════════════════════════════════════════════════════════════
            // 3) 2 seviyesinde yeni renk (4+ majör veya minör)
            // ═══════════════════════════════════════════════════════════════
            // Önce diğer majör (4+ ise) göster
            int digerMajörUzunluk = ElDegerlendirici.RenkUzunlugu(el, digerMajör);
            if (digerMajörUzunluk >= 4)
            {
                // Reverse kontrolü: 1♥ - 1NT - 2♠ → reverse (16+ OP)
                // 1♠ - 1NT - 2♥ → reverse değil (min-max arası)
                if (majör == "Kupa" && digerMajör == "Maça")
                {
                    // Reverse: 16+ OP
                    if (hp >= 16)
                    {
                        return "2♠";
                    }
                    // Reverse değil, min-minör göster
                    // (Aşağıdaki minör mantığına düşer)
                }
                else
                {
                    // 1♠ - 1NT - 2♥: reverse değil, 12-18 OP
                    return "2♥";
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // 4) Minör göster (3+ kart, genellikle 3'lü)
            // ═══════════════════════════════════════════════════════════════
            // Önce 4'lü minör var mı?
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // 4'lü minör
            if (karo >= 4) return "2♦";
            if (sinek >= 4) return "2♣";

            // 3'lü minör (5-3-3-2 dağılım)
            // Doküman: "3'lü minörlerden ucuz olanı" → ♦ önce
            if (karo >= 3) return "2♦";
            if (sinek >= 3) return "2♣";

            // Nadir: 2'li ♣ (1♥ - 1NT - 2♣)
            if (majör == "Kupa" && sinek >= 2) return "2♣";

            // Son çare
            if (majörUzunluk >= 5) return "2" + sembol;
            return "Pas";
        }
    }
}