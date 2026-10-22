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
            if (durum == null || durum.Gecmis == null) return false;
            if (durum.AktifOyuncuEli == null) return false;

            var gercekTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            if (gercekTeklifler.Count < 1) return false;

            // ─── SENARYO A: Cevapçının Inverted Minör desteği ───
            var sonTeklif = gercekTeklifler.Last();
            if (sonTeklif.Oyuncu == durum.Partner
                && sonTeklif.Seviye == 1
                && (sonTeklif.Koz == "Sinek" || sonTeklif.Koz == "Karo"))
            {
                // Rakip araya girmiş mi?
                if (durum.RakipActiMi()) return false;

                // 4'lü majör varsa minör desteklenmez
                int macca = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Kupa");
                if (macca >= 4 || kupa >= 4) return false;

                // Minör desteği 5+ mı?
                int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, sonTeklif.Koz);
                // ♦ için nadiren 4'lü olabilir
                if (sonTeklif.Koz == "Sinek" && destek < 5) return false;
                if (sonTeklif.Koz == "Karo" && destek < 4) return false;

                int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);

                // 2m: 10+ HP (Forcing)
                if (hp >= 10) return true;

                // 3m: 0-7 HP (Preemptive)
                if (hp <= 7) return true;

                // 8-9 HP: gri bölge, judgement
                // Basitleştirme: 8-9 HP için Inverted Minors tetiklenmez
                return false;
            }

            // ─── SENARYO B: Açıcının Inverted Minör'e cevabı ───
            // Geçmiş: 1m (aktif) - 2m (partner) - ? (açıcı cevap verecek)
            if (gercekTeklifler.Count == 2)
            {
                var teklif2 = gercekTeklifler[1];
                var teklif1 = gercekTeklifler[0];

                if (teklif1.Oyuncu == durum.AktifOyuncu
                    && teklif1.Seviye == 1
                    && (teklif1.Koz == "Sinek" || teklif1.Koz == "Karo")
                    && teklif2.Oyuncu == durum.Partner
                    && teklif2.Teklif == teklif1.Teklif) // 2m = Inverted
                {
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

            // Cevapçı mıyız? (Inverted minör desteği vereceğiz)
            if (sonTeklif.Oyuncu == durum.Partner
                && sonTeklif.Seviye == 1
                && (sonTeklif.Koz == "Sinek" || sonTeklif.Koz == "Karo"))
            {
                return CevapciTeklifi(durum, el, sonTeklif.Koz);
            }

            // Açıcı mıyız? (Inverted minör'e cevap vereceğiz)
            return AciciCevabi(durum, el, gercekTeklifler);
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAPÇININ INVERTED MINÖR DESTEĞİ
        // ═══════════════════════════════════════════════════════════════════

        private string CevapciTeklifi(IhaleDurumu durum, List<Card> el, string minör)
        {
            int hp = ElDegerlendirici.HCP(el);
            string sembol = KozYardimcisi.KozSembolu(minör);

            // 10+ HP → 2m (Forcing)
            if (hp >= 10)
                return "2" + sembol;

            // 0-7 HP → 3m (Preemptive)
            if (hp <= 7)
                return "3" + sembol;

            // 8-9 HP → gri bölge, 2m veya 3m
            // Basitleştirme: 2m (daha güvenli)
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