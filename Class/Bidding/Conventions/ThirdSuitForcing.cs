using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Üçüncü Renk Forsingi (Third Suit Forcing)
    /// 
    /// Tetikleyici: Açıcı açış rengini sıçramasız tekrar ettikten sonra,
    /// cevapçının sıçramasız yeni bir renk söylemesi.
    /// 
    /// Örnek: 1♣ - 1♥ - 2♣ - 2♦ (2♦ = 3. Renk Forsingi)
    /// 
    /// Koşullar:
    ///   - Rakip araya girmemiş
    ///   - Cevapçıda 11+ OP (ekonomik) / 13+ OP (pahalı)
    ///   - Açıcının renk tekrarı sıçramasız
    ///   - İlk cevap 2/1 cevabı değil
    /// 
    /// Amaç: Açıcıya sorar:
    ///   - İlk rengime 3'lü tutuşun var mı?
    ///   - 4. renkte durdurucun var mı?
    ///   - İkinci rengim 4'lü mü?
    /// 
    /// Açıcının cevap önceliği (sırayla):
    ///   1) Cevapçının ilk rengine 3'lü KALİTELİ tutuş (en az 1 onör)
    ///   2) Diğer majör 4'lü
    ///   3) 3. renkte 4'lü + durdurucu
    ///   4) Diğer renkte durdurucu → NT
    ///   5) Dördüncü rengi söyle (durdurucu sorusu)
    /// </summary>
    public class ThirdSuitForcing : IKonvansiyon
    {
        public string Ad => "3. Renk Forsingi";
        public string Aciklama => "Üçüncü renk forsingi";
        public bool AktifMi { get; set; } = true;

        // TrialBid (27) ile GSF (28) arasında
        public int Oncelik => 27;

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

            if (gercekTeklifler.Count < 3) return false;

            // Rakip araya girmiş mi?
            if (durum.RakipActiMi()) return false;

            // İki senaryo:
            // A) Cevapçının 3. Renk Forsingi başlatma sırası
            // B) Açıcının 3. Renk Forsingi'ne cevap sırası

            if (CevapciSirasiMi(durum, gercekTeklifler))
                return true;

            if (AciciCevapSirasiMi(durum, gercekTeklifler))
                return true;

            return false;
        }

        /// <summary>
        /// Cevapçının 3. Renk Forsingi başlatma sırası mı?
        /// 
        /// Senaryo: 1♣ (partner) - 1♥ (aktif oyuncu) - 2♣ (partner) - ?
        /// </summary>
        private bool CevapciSirasiMi(IhaleDurumu durum,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int n = gercekTeklifler.Count;
            if (n < 3) return false;

            var teklif3 = gercekTeklifler[n - 1]; // açıcı: minör tekrar
            var teklif2 = gercekTeklifler[n - 2]; // cevapçı: 1 seviye majör
            var teklif1 = gercekTeklifler[n - 3]; // açıcı: minör açış

            if (teklif1.Oyuncu != durum.Partner) return false;
            if (teklif3.Oyuncu != durum.Partner) return false;
            if (teklif2.Oyuncu != durum.AktifOyuncu) return false;

            if (teklif1.Koz != "Sinek" && teklif1.Koz != "Karo") return false;
            if (teklif1.Seviye != 1) return false;

            if (teklif2.Koz != "Kupa" && teklif2.Koz != "Maça") return false;
            if (teklif2.Seviye != 1) return false;

            if (teklif3.Koz != teklif1.Koz) return false;
            if (teklif3.Seviye != 2) return false;

            var ortaklikTeklifleri = gercekTeklifler
                .Where(h => h.Oyuncu == durum.AktifOyuncu
                         || h.Oyuncu == durum.Partner)
                .ToList();

            if (ortaklikTeklifleri.Count != 3) return false;

            return true;
        }

        /// <summary>
        /// Açıcının 3. Renk Forsingi'ne cevap sırası mı?
        /// 
        /// Senaryo: 1♣ (aktif) - 1♥ (partner) - 2♣ (aktif) - 2♦ (partner) - ?
        /// </summary>
        private bool AciciCevapSirasiMi(IhaleDurumu durum,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int n = gercekTeklifler.Count;
            if (n < 4) return false;

            var teklif4 = gercekTeklifler[n - 1]; // cevapçı: 3. renk forsing
            var teklif3 = gercekTeklifler[n - 2]; // açıcı: minör tekrar
            var teklif2 = gercekTeklifler[n - 3]; // cevapçı: 1 seviye majör
            var teklif1 = gercekTeklifler[n - 4]; // açıcı: minör açış

            if (teklif1.Oyuncu != durum.AktifOyuncu) return false;
            if (teklif3.Oyuncu != durum.AktifOyuncu) return false;
            if (teklif2.Oyuncu != durum.Partner) return false;
            if (teklif4.Oyuncu != durum.Partner) return false;

            if (teklif1.Koz != "Sinek" && teklif1.Koz != "Karo") return false;
            if (teklif1.Seviye != 1) return false;

            if (teklif2.Koz != "Kupa" && teklif2.Koz != "Maça") return false;
            if (teklif2.Seviye != 1) return false;

            if (teklif3.Koz != teklif1.Koz) return false;
            if (teklif3.Seviye != 2) return false;

            if (teklif4.Koz == teklif1.Koz) return false;
            if (teklif4.Koz == teklif2.Koz) return false;
            if (teklif4.Koz == "NT") return false;

            return true;
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

            if (CevapciSirasiMi(durum, gercekTeklifler))
                return CevapciTeklifi(durum, el, gercekTeklifler);

            if (AciciCevapSirasiMi(durum, gercekTeklifler))
                return AciciCevap(durum, el, gercekTeklifler);

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAPÇININ 3. RENK FORSİNGİ BAŞLATMASI
        // ═══════════════════════════════════════════════════════════════════

        private string CevapciTeklifi(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int hp = ElDegerlendirici.HCP(el);

            string yRenk = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.AktifOyuncu && h.Seviye == 1
                    && (h.Koz == "Kupa" || h.Koz == "Maça"))
                {
                    yRenk = h.Koz;
                    break;
                }
            }
            if (string.IsNullOrEmpty(yRenk)) return "Pas";

            string xRenk = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.Partner && h.Seviye == 1
                    && (h.Koz == "Sinek" || h.Koz == "Karo"))
                {
                    xRenk = h.Koz;
                    break;
                }
            }
            if (string.IsNullOrEmpty(xRenk)) return "Pas";

            // En düşük seviyede söylenebilecek yeni renk
            string tsrRenk = null;
            int tsrSeviye = 2;

            string[] renkSirasi = { "Sinek", "Karo", "Kupa", "Maça" };

            foreach (var renk in renkSirasi)
            {
                bool konusuldu = gercekTeklifler.Any(h =>
                    h.Koz == renk
                    && (h.Oyuncu == durum.AktifOyuncu || h.Oyuncu == durum.Partner));

                if (konusuldu) continue;

                tsrRenk = renk;
                break;
            }

            if (string.IsNullOrEmpty(tsrRenk)) return "Pas";

            if (hp < 11) return "Pas";

            string sembol = KozYardimcisi.KozSembolu(tsrRenk);
            return tsrSeviye + sembol;
        }

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ 3. RENK FORSİNGİ'NE CEVABI
        // ═══════════════════════════════════════════════════════════════════
        // Öncelik sırası:
        //   1) Cevapçının ilk rengine 3'lü KALİTELİ tutuş (en az 1 onör)
        //   2) Diğer majör 4'lü
        //   3) 3. renkte 4'lü + durdurucu
        //   4) Diğer renkte durdurucu → NT
        //   5) Dördüncü rengi söyle (durdurucu sorusu)
        // ═══════════════════════════════════════════════════════════════════

        private string AciciCevap(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            string yRenk = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.Partner && h.Seviye == 1
                    && (h.Koz == "Kupa" || h.Koz == "Maça"))
                {
                    yRenk = h.Koz;
                    break;
                }
            }
            if (string.IsNullOrEmpty(yRenk)) return "Pas";

            var tsrTeklif = gercekTeklifler.Last();
            string tsrRenk = tsrTeklif.Koz;

            string xRenk = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.AktifOyuncu && h.Seviye == 1
                    && (h.Koz == "Sinek" || h.Koz == "Karo"))
                {
                    xRenk = h.Koz;
                    break;
                }
            }

            int yUzunluk = ElDegerlendirici.RenkUzunlugu(el, yRenk);
            int tsrUzunluk = ElDegerlendirici.RenkUzunlugu(el, tsrRenk);
            int hp = ElDegerlendirici.HCP(el);

            // ─── 1) Cevapçının ilk rengine 3'lü KALİTELİ tutuş ─────────
            if (yUzunluk >= 3 && KaliteliDestekMi(el, yRenk))
            {
                string sembol = KozYardimcisi.KozSembolu(yRenk);
                return "2" + sembol;
            }

            // ─── 2) Diğer majör 4'lü ────────────────────────────────
            string digerMajör = (yRenk == "Maça") ? "Kupa" : "Maça";
            if (ElDegerlendirici.RenkUzunlugu(el, digerMajör) >= 4)
            {
                string sembol = KozYardimcisi.KozSembolu(digerMajör);
                return "2" + sembol;
            }

            // ─── 3) 3. renkte 4'lü + durdurucu ──────────────────────
            if (tsrUzunluk >= 4)
            {
                string sembol = KozYardimcisi.KozSembolu(tsrRenk);
                return "3" + sembol;
            }

            // ─── 4) Diğer renkte durdurucu → NT ─────────────────────
            bool dengeli = ElDegerlendirici.DengeliEl(el);
            if (dengeli && hp >= 11 && hp <= 13)
                return "2NT";
            if (dengeli && hp >= 14)
                return "3NT";

            // ─── 5) Dördüncü rengi söyle (durdurucu sorusu) ─────────
            string[] tumRenkler = { "Sinek", "Karo", "Kupa", "Maça" };
            foreach (var renk in tumRenkler)
            {
                bool konusuldu = gercekTeklifler.Any(h => h.Koz == renk);
                if (!konusuldu && renk != tsrRenk)
                {
                    string sembol = KozYardimcisi.KozSembolu(renk);
                    return "3" + sembol;
                }
            }

            return "3NT";
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Bir renge 3'lü destek "kaliteli" mi?
        /// 3+ kart VE en az 1 onör (A, K, Q) olmalı.
        /// 
        /// Not: Onörsüz 3'lü destek (3 küçük kart) kalitesiz sayılır.
        /// Bu durumda açıcı 2 seviyesinde destek vermek yerine
        /// 2NT (dengeli) veya başka bir yol izler.
        /// </summary>
        private bool KaliteliDestekMi(List<Card> el, string renk)
        {
            var renkKartlari = el
                .Where(c => c.Suit == renk)
                .OrderByDescending(c => c.Value)
                .ToList();

            if (renkKartlari.Count < 3) return false;

            // En az 1 onör (A=14, K=13, Q=12)
            return renkKartlari.Any(c => c.Value >= 12);
        }
    }
}