using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Inverted Minors — Minör desteklerinin gücünü tersine çevirir.
    /// 
    /// Standart:
    ///   1♣ - 2♣ = 6-9 HP, zayıf destek
    ///   1♣ - 3♣ = 10-12 HP, limit raise
    /// 
    /// Inverted:
    ///   1♣ - 2♣ = 10+ HP, FORCING (bir tur)
    ///   1♣ - 3♣ = 0-6/7 HP, PREEMPTIVE (zayıf)
    /// 
    /// Aynı şekilde 1♦ için de geçerli.
    /// 
    /// Koşullar:
    ///   - 4'lü majör varsa minör desteklenmez
    ///   - 5+ kart destek (♦ için nadiren 4'lü)
    ///   - Rakip müdahalesinden sonra geçersiz
    /// </summary>
    public class InvertedMinors : IKonvansiyon
    {
        public string Ad => "Inverted Minors";
        public string Aciklama => "Ters minör destekleri";
        public bool AktifMi { get; set; } = true;

        // BesliMajor (10)'dan sonra, Stayman (11)'dan önce
        public int Oncelik => 10;

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. Partner açtı mı?
            if (!durum.PartnerActiMi()) return false;

            // 2. Partnerin teklifi 1♣ veya 1♦ mi?
            string partnerTeklif = durum.PartnerTeklifleri.LastOrDefault();
            if (partnerTeklif != "1♣" && partnerTeklif != "1♦") return false;

            // 3. Rakip araya girmedi mi?
            if (durum.RakipActiMi()) return false;

            // 4. Ben henüz konuşmadım mı?
            if (durum.KendiGercekTeklifSayisi > 0) return false;

            var el = durum.AktifOyuncuEli;
            string partnerKozu = KozYardimcisi.TekliftenKozCikar(partnerTeklif);

            // 5. 4+ majör varsa Inverted Minors KULLANILMAZ
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            if (maca >= 4 || kupa >= 4) return false;

            // 6. Partnerin minöründen 5+ kart var mı? (veya çok iyi 4'lü)
            int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
            if (destek < 5) return false; // Basitleştirme: 5+ kart şart

            // 7. HP kontrolü yok: 0-8 HP → 3m (preemptive), 9+ HP → 2m (forcing).
            //    (Eskiden hp < 5 reddediliyordu; 1♣ - 3♣ blokajı 0-7 HP de içerir.)
            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VER
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string partnerTeklif = durum.PartnerTeklifleri.Last();
            string partnerKozu = KozYardimcisi.TekliftenKozCikar(partnerTeklif);
            string sembol = KozYardimcisi.KozSembolu(partnerKozu);

            int hp = ElDegerlendirici.HCP(el);
            int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);

            // 0-9 HP → 3m (zayıf, preemptive)
            if (hp <= 9)
                return "3" + sembol;

            // 10+ HP → 2m (forcing). Partnerin minörü ne ise o: 1♣ → 2♣, 1♦ → 2♦.
            // (Eskiden 10-12 HP için "2♦" sabit yazılıydı; 1♣ açılışında yanlıştı,
            //  9 HP ise hiç karşılanmıyor ve Pas dönüyordu.)
            return "2" + sembol;
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAPÇININ INVERTED MINÖR DESTEĞİ
        // ═══════════════════════════════════════════════════════════════════

        private string CevapciTeklifi(IhaleDurumu durum, List<Card> el, string minör)
        {
            int hp = ElDegerlendirici.HCP(el);
            string sembol = KozYardimcisi.KozSembolu(minör);

            // 13+ HP → 2m (Zon Forcing)
            if (hp >= 13)
                return "2" + sembol;

            // 0-8 HP → 3m (Preemptive)
            if (hp <= 8)
                return "3" + sembol;

            // 9-12 HP → 2m (1 tur Forcing)
            return "2" + sembol;
        }

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ INVERTED MINÖR'E CEVABI
        // ═══════════════════════════════════════════════════════════════════

        private string AciciCevabi(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // Minör rengi bul
            string minör = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.AktifOyuncu && h.Seviye == 1
                    && (h.Koz == "Sinek" || h.Koz == "Karo"))
                {
                    minör = h.Koz;
                    break;
                }
            }
            if (string.IsNullOrEmpty(minör)) return "Pas";

            string sembol = KozYardimcisi.KozSembolu(minör);
            int minörUzunluk = ElDegerlendirici.RenkUzunlugu(el, minör);

            // ─── 11-14 veya 18-19 dengeli → 2NT ───
            if (dengeli && hp >= 11 && hp <= 14)
                return "2NT";
            if (dengeli && hp >= 18 && hp <= 19)
                return "2NT";

            // ─── 11-13 minimum, signoff → 3m ───
            if (hp >= 11 && hp <= 13 && !dengeli)
                return "3" + sembol;

            // ─── Stopper gösterme (yeni majör) ───
            // 2♥ veya 2♠: Forcing, stopper gösterir
            // Basitleştirme: 4'lü majör yoksa ve stopper varsa göster
            // (Nadir durum, atlanabilir)

            // ─── Splinter (sıçramalı renk) ───
            // 15+ HP + singleton → GF
            // (Nadir durum, atlanabilir)

            // Fallback
            if (dengeli) return "2NT";
            return "3" + sembol;
        }
    }
}