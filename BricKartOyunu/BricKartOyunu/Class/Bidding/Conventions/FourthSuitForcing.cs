using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Dördüncü Renk Forsingi (Fourth Suit Forcing - 4SF)
    /// 
    /// Tetikleyici: Ortaklık üç farklı renk konuştuktan sonra, cevapçının
    /// dördüncü rengi (sıçramasız) yapay forsing olarak söylemesi.
    /// 
    /// Örnek:
    ///   1♦ - 1♥ - 2♣ - 2♠ (2♠ = 4SF)
    ///   1♣ - 1♠ - 2♦ - 2♥ (2♥ = 4SF)
    /// 
    /// XYZ ile ÇAKIŞMA ÖNLEMİ:
    ///   - XYZ 1 seviyesindeki sekanslarda (1X-1Y-1Z) geçerli
    ///   - 4SF sadece 2+ seviyesindeki sekanslarda tetiklenir
    ///   - Böylece iki konvansiyon çakışmaz
    /// 
    /// Amaç:
    ///   - 3NT için durdurucu (stopper) araştırmak
    ///   - Uygun kontratı bulmak (fit, NT, vs.)
    /// 
    /// Açıcının cevap önceliği (sırayla):
    ///   1) Cevapçının ilk rengine 3'lü tutuş
    ///   2) Dördüncü renkte durdurucu → NT
    ///   3) Dördüncü renkte 4'lü
    ///   4) En doğal rebid (5'li rengi tekrar)
    /// </summary>
    public class FourthSuitForcing : IKonvansiyon
    {
        public string Ad => "4. Renk Forsingi";
        public string Aciklama => "Dördüncü renk forsingi";
        public bool AktifMi { get; set; } = true;

        // XYZ (24) ve diğer konvansiyonlardan sonra, BasitCevap (100)'dan önce
        public int Oncelik => 29;

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
            // A) Cevapçının 4SF başlatma sırası
            // B) Açıcının 4SF'ye cevap sırası

            if (CevapciSirasiMi(durum, gercekTeklifler))
                return true;

            if (AciciCevapSirasiMi(durum, gercekTeklifler))
                return true;

            return false;
        }

        /// <summary>
        /// Cevapçının 4SF başlatma sırası mı?
        /// 
        /// Senaryo: 1♦ (partner) - 1♥ (aktif) - 2♣ (partner) - ?
        /// VEYA: 1♣ (partner) - 1♠ (aktif) - 2♦ (partner) - ?
        /// 
        /// Kritik: Açıcının ikinci rengi 2 SEVİYESİNDE olmalı (XYZ çakışması önlemi).
        /// </summary>
        private bool CevapciSirasiMi(IhaleDurumu durum,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int n = gercekTeklifler.Count;
            if (n < 3) return false;

            var teklif3 = gercekTeklifler[n - 1]; // açıcı: 2. renk
            var teklif2 = gercekTeklifler[n - 2]; // cevapçı: 1 seviye renk
            var teklif1 = gercekTeklifler[n - 3]; // açıcı: 1 seviye açış

            // Sıra kontrolü
            if (teklif1.Oyuncu != durum.Partner) return false;
            if (teklif3.Oyuncu != durum.Partner) return false;
            if (teklif2.Oyuncu != durum.AktifOyuncu) return false;

            // teklif1: 1 seviyesinde renk açış (minör veya majör)
            if (teklif1.Seviye != 1) return false;
            if (teklif1.Koz == "NT") return false;

            // teklif2: 1 seviyesinde renk cevabı
            if (teklif2.Seviye != 1) return false;
            if (teklif2.Koz == "NT") return false;

            // ★ KRİTİK: teklif3 (açıcının 2. rengi) 2 SEVİYESİNDE olmalı
            // Böylece XYZ (1 seviyesindeki sekanslar) ile çakışmaz
            if (teklif3.Seviye != 2) return false;
            if (teklif3.Koz == "NT") return false;

            // Üç farklı renk mi?
            var renkler = new HashSet<string> { teklif1.Koz, teklif2.Koz, teklif3.Koz };
            if (renkler.Count != 3) return false;

            // Ortaklık dışı teklif var mı? (rakip teklifleri sayılmaz)
            var ortaklikTeklifleri = gercekTeklifler
                .Where(h => h.Oyuncu == durum.AktifOyuncu
                         || h.Oyuncu == durum.Partner)
                .ToList();

            if (ortaklikTeklifleri.Count != 3) return false;

            return true;
        }

        /// <summary>
        /// Açıcının 4SF'ye cevap sırası mı?
        /// 
        /// Senaryo: 1♦ (aktif) - 1♥ (partner) - 2♣ (aktif) - 2♠ (partner, 4SF) - ?
        /// </summary>
        private bool AciciCevapSirasiMi(IhaleDurumu durum,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int n = gercekTeklifler.Count;
            if (n < 4) return false;

            var teklif4 = gercekTeklifler[n - 1]; // cevapçı: 4SF
            var teklif3 = gercekTeklifler[n - 2]; // açıcı: 2. renk
            var teklif2 = gercekTeklifler[n - 3]; // cevapçı: 1 seviye renk
            var teklif1 = gercekTeklifler[n - 4]; // açıcı: 1 seviye açış

            // Sıra kontrolü
            if (teklif1.Oyuncu != durum.AktifOyuncu) return false;
            if (teklif3.Oyuncu != durum.AktifOyuncu) return false;
            if (teklif2.Oyuncu != durum.Partner) return false;
            if (teklif4.Oyuncu != durum.Partner) return false;

            // teklif1: 1 seviye açış
            if (teklif1.Seviye != 1) return false;
            if (teklif1.Koz == "NT") return false;

            // teklif2: 1 seviye cevap
            if (teklif2.Seviye != 1) return false;
            if (teklif2.Koz == "NT") return false;

            // teklif3: açıcının 2. rengi, 2 seviyesinde
            if (teklif3.Seviye != 2) return false;
            if (teklif3.Koz == "NT") return false;

            // teklif4: 4SF (yeni renk, 2 seviyesinde sıçramasız)
            if (teklif4.Seviye != 2) return false;
            if (teklif4.Koz == "NT") return false;

            // Üç farklı renk + 4SF = 4 farklı renk olmalı
            var renkler = new HashSet<string>
            {
                teklif1.Koz, teklif2.Koz, teklif3.Koz, teklif4.Koz
            };
            if (renkler.Count != 4) return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VER
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            // Ham teklif yasadışı kalırsa (ör. 2♠'den sonra 2♥) yasal en düşük seviyeye çıkar
            return KozYardimcisi.YasalaYukselt(TeklifVerHam(durum), durum.SonGercekTeklif);
        }

        private string TeklifVerHam(IhaleDurumu durum)
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
        // CEVAPÇININ 4SF BAŞLATMASI
        // ═══════════════════════════════════════════════════════════════════

        private string CevapciTeklifi(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int hp = ElDegerlendirici.HCP(el);

            // En düşük seviyede söylenebilecek yeni renk = 4SF
            // Renk sırası: ♣, ♦, ♥, ♠
            string[] renkSirasi = { "Sinek", "Karo", "Kupa", "Maça" };

            foreach (var renk in renkSirasi)
            {
                bool konusuldu = gercekTeklifler.Any(h =>
                    h.Koz == renk
                    && (h.Oyuncu == durum.AktifOyuncu || h.Oyuncu == durum.Partner));

                if (konusuldu) continue;

                // 4SF için en az 11 OP (davet) olmalı
                if (hp < 11) return "Pas";

                string sembol = KozYardimcisi.KozSembolu(renk);
                return "2" + sembol;
            }

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ 4SF'YE CEVABI
        // ═══════════════════════════════════════════════════════════════════
        // Öncelik sırası:
        //   1) Cevapçının ilk rengine 3'lü tutuş
        //   2) Dördüncü renkte durdurucu → NT
        //   3) Dördüncü renkte 4'lü
        //   4) En doğal rebid (5'li rengi tekrar)
        // ═══════════════════════════════════════════════════════════════════

        private string AciciCevap(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            // Cevapçının ilk rengini bul
            string yRenk = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.Partner && h.Seviye == 1)
                {
                    yRenk = h.Koz;
                    break;
                }
            }
            if (string.IsNullOrEmpty(yRenk)) return "Pas";

            // 4SF hangi renk?
            var fsfTeklif = gercekTeklifler.Last();
            string fsfRenk = fsfTeklif.Koz;

            // Açıcının ilk rengini bul (1 seviye)
            string xRenk = null;
            // Açıcının 2. rengini bul (2 seviye)
            string zRenk = null;

            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.AktifOyuncu)
                {
                    if (h.Seviye == 1 && string.IsNullOrEmpty(xRenk))
                        xRenk = h.Koz;
                    else if (h.Seviye == 2 && string.IsNullOrEmpty(zRenk))
                        zRenk = h.Koz;
                }
            }

            int yUzunluk = ElDegerlendirici.RenkUzunlugu(el, yRenk);
            int fsfUzunluk = ElDegerlendirici.RenkUzunlugu(el, fsfRenk);
            int hp = ElDegerlendirici.HCP(el);

            // ─── 1) Cevapçının ilk rengine 3'lü tutuş ─────────────
            if (yUzunluk >= 3)
            {
                string sembol = KozYardimcisi.KozSembolu(yRenk);
                // Minimum ise 2 seviyesinde, maksimum ise 3 seviyesinde
                if (hp >= 14)
                    return "3" + sembol;
                return "2" + sembol;
            }

            // ─── 2) Dördüncü renkte durdurucu → NT ────────────────
            bool fsfStopper = ElDegerlendirici.StopperVar(el, fsfRenk);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            if (fsfStopper && dengeli && hp >= 11 && hp <= 13)
                return "2NT";
            if (fsfStopper && dengeli && hp >= 14)
                return "3NT";

            // ─── 3) Dördüncü renkte 4'lü ─────────────────────────
            if (fsfUzunluk >= 4)
            {
                string sembol = KozYardimcisi.KozSembolu(fsfRenk);
                if (hp >= 14)
                    return "3" + sembol;
                return "2" + sembol;
            }

            // ─── 4) En doğal rebid: 5'li rengi tekrar ─────────────
            // Açıcının ilk rengi 5+ ise tekrar et
            if (!string.IsNullOrEmpty(xRenk))
            {
                int xUzunluk = ElDegerlendirici.RenkUzunlugu(el, xRenk);
                if (xUzunluk >= 5)
                {
                    string sembol = KozYardimcisi.KozSembolu(xRenk);
                    if (hp >= 14)
                        return "3" + sembol;
                    return "2" + sembol;
                }
            }

            // Açıcının 2. rengi 5+ ise tekrar et
            if (!string.IsNullOrEmpty(zRenk))
            {
                int zUzunluk = ElDegerlendirici.RenkUzunlugu(el, zRenk);
                if (zUzunluk >= 5)
                {
                    string sembol = KozYardimcisi.KozSembolu(zRenk);
                    if (hp >= 14)
                        return "3" + sembol;
                    return "2" + sembol;
                }
            }

            // Son çare: 2NT (dengeli)
            if (dengeli)
                return "2NT";

            // Fallback: 3NT
            return "3NT";
        }
    }
}