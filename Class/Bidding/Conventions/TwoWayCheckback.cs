using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// İki Yönlü Checkback (Two-Way Checkback) — Kit Woolsey tarafından tanıtılmıştır.
    /// Açanın 1NT rebidinden sonra 2♣/2♦ deklereleri yapaydır.
    /// Yeni Minör Forsing (NMF) yerine kullanılır.
    /// 
    /// Kullanım alanı: 1X - 1Y - 1NT sonrası (X = ♣/♦/♥, Y = ♥/♠)
    /// 
    /// Ana prensipler:
    ///   1) 2♣ → 2♦'ya transfer (puppet). Zayıf el veya davet el.
    ///   2) 2♦ → Oyun zorlayıcı (GF). Tüm forsing eller.
    ///   3) Sıçramalar → Forsing, kaliteli 6'lı veya 5-5 renk.
    /// 
    /// Örnek:
    ///   1♣ - 1♥ - 1NT - 2♣ (puppet) - 2♦ (zorunlu) - Pas (zayıf ♦) / 3♣ (zayıf ♣) / 2NT (davet)
    ///   1♦ - 1♠ - 1NT - 2♦ (GF) - 2♥/2♠ (3'lü destek) / 2NT (diğer) ...
    /// </summary>
    public class TwoWayCheckback : IKonvansiyon
    {
        public string Ad => "İki Yönlü Checkback";
        public string Aciklama => "1NT rebid sonrası iki yönlü checkback";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 25;   // Stayman (10) ve Jacoby Transfer (15)'ten sonra,
                                    // BasitCevap (100)'dan önce.

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.Gecmis == null) return false;
            if (durum.AktifOyuncuEli == null) return false;

            // Uygun olduğumuz tek durum: cevapçının sırası ve 1X-1Y-1NT geçmişi olmalı.
            // (Açıcının 2♣/2♦'ye cevap vermesi ayrı bir değerlendirmedir; ama bu
            // konvansiyonun kapsamına girer.)

            // Geçmişteki gerçek teklifleri al
            var gercekTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            if (gercekTeklifler.Count < 3) return false;

            // Son üç teklif: 1X (açıcı), 1Y (cevapçı), 1NT (açıcı) olmalı
            int n = gercekTeklifler.Count;

            // 1NT son teklif olmalı
            var sonTeklif = gercekTeklifler[n - 1];
            if (sonTeklif.Teklif != "1NT") return false;

            // 1NT'yi veren, aktif oyuncunun partneri olmalı (açıcı)
            if (sonTeklif.Oyuncu != durum.Partner) return false;

            // Sondan bir önceki: cevapçının 1 seviyesi renk teklifi (1♥ veya 1♠)
            var cevapciTeklifi = gercekTeklifler[n - 2];
            if (cevapciTeklifi.Oyuncu != durum.AktifOyuncu) return false;
            if (cevapciTeklifi.Seviye != 1) return false;
            if (cevapciTeklifi.Koz != "Kupa" && cevapciTeklifi.Koz != "Maça")
                return false;

            // Sondan iki önceki: açıcının 1 seviyesi renk teklifi (1♣, 1♦, 1♥)
            var aciciTeklifi = gercekTeklifler[n - 3];
            if (aciciTeklifi.Oyuncu != durum.Partner) return false;
            if (aciciTeklifi.Seviye != 1) return false;
            if (aciciTeklifi.Koz == "NT") return false;

            // 1♥ - 1♠ - 1NT de geçerli
            if (aciciTeklifi.Koz == "Kupa" && cevapciTeklifi.Koz != "Maça")
                return false;

            // Ekstra güvence: aktif oyuncunun bu turda henüz teklifi yok mu?
            // (Bu, konvansiyonun ilk tetiklenmesi için gerekli.)
            // İkinci tur cevaplar da bu konvansiyonun parçası olduğu için bu kontrolü esnetiyoruz.
            // Sadece aktif oyuncunun "son" teklifinin bu turda olmadığından emin olalım.

            // Bu turda, aktif oyuncu henüz teklif vermedi mi?
            // Yani geçmişte aktif oyuncunun teklifi cevapçı teklifinden sonra yok.
            // (Bu, 1X-1Y-1NT sonrası cevapçının sırası olduğunu doğrular.)
            int sonCevapciIndex = -1;
            for (int i = gercekTeklifler.Count - 1; i >= 0; i--)
            {
                if (gercekTeklifler[i].Oyuncu == durum.AktifOyuncu)
                {
                    sonCevapciIndex = i;
                    break;
                }
            }

            // Aktif oyuncunun son teklifi, cevapçı teklifi olmalı (1Y).
            // Yani 1NT'den sonra aktif oyuncu henüz konuşmadı.
            if (sonCevapciIndex != n - 2) return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VER
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null || el.Count != 13) return "Pas";

            // Bu konvansiyon iki farklı aktör için teklif üretebilir:
            //   1) Cevapçı (1X-1Y-1NT sonrası ilk konuşan)
            //   2) Açıcı (cevapçı 2♣/2♦ dedikten sonra cevap veren)
            // Hangisi olduğumuzu geçmişe bakarak anlarız.

            var gercekTeklifler = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            int n = gercekTeklifler.Count;

            // Son teklif 2♣ ise → açıcıyız (puppet'a cevap)
            if (n >= 1 && gercekTeklifler[n - 1].Teklif == "2♣")
            {
                // Zorunlu 2♦ cevabı
                return "2♦";
            }

            // Son teklif 2♦ (puppet'a cevap değil, gerçek 2♦) ve bir önceki 1NT ise →
            // cevapçı 2♦ dedi, şimdi açıcıyız.
            // Bu durumu "cevapçı 2♦ dedi, açıcının sırası" olarak tanımlayalım:
            // n >= 2, gercekTeklifler[n-1].Teklif == "2♦",
            // gercekTeklifler[n-2].Teklif == "1NT"
            if (n >= 2
                && gercekTeklifler[n - 1].Teklif == "2♦"
                && gercekTeklifler[n - 2].Teklif == "1NT")
            {
                // 2♦ (GF) sonrası açıcının cevabı.
                return AciciIkiKaroyaCevap(durum, el, gercekTeklifler);
            }

            // Aksi halde cevapçıyız (1X-1Y-1NT sonrası ilk konuşan)
            return CevapciTeklifi(durum, el);
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAPÇI TEKLİFİ
        // ═══════════════════════════════════════════════════════════════════

        private string CevapciTeklifi(IhaleDurumu durum, List<Card> el)
        {
            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // Cevapçının ilk teklifini bul (1♥ veya 1♠)
            string cevapciIlkRenk = null;
            foreach (var h in durum.Gecmis)
            {
                if (h.Oyuncu == durum.AktifOyuncu && h.GercekTeklifMi)
                {
                    cevapciIlkRenk = h.Koz;
                    break;
                }
            }
            if (string.IsNullOrEmpty(cevapciIlkRenk)) return "Pas";

            int majörUzunluk = ElDegerlendirici.RenkUzunlugu(el, cevapciIlkRenk);

            // ─── ZAYIF ELLER ────────────────────────────────────────────────
            // 2 seviyesinde majör tekrarı (signoff) veya 1♠-1NT-2♥ zayıf
            if (hp < 8)
            {
                // 1♠ cevabı sonrası 2♥ zayıf (5+♠ + 4+♥)
                if (cevapciIlkRenk == "Maça"
                    && ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4)
                {
                    return "2♥";
                }

                // Majör tekrarı (5+ renk)
                if (majörUzunluk >= 5)
                {
                    string sembol = KozYardimcisi.KozSembolu(cevapciIlkRenk);
                    return "2" + sembol;
                }

                // Zayıf ♦ eli → 2♣ ile puppet, sonra Pas
                // (Burada 2♣ diyerek puppet başlatıyoruz; sonraki turda Pas diyeceğiz.)
                int karoUzunluk = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                int sinekUzunluk = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

                if (karoUzunluk >= 5)
                {
                    // 2♣ puppet → açıcı 2♦ diyecek → biz Pas diyeceğiz.
                    return "2♣";
                }

                // Zayıf ♣ eli → 2♣ puppet, sonra 3♣
                if (sinekUzunluk >= 5)
                {
                    return "2♣";
                }

                // Çok zayıf, majör de yok → Pas
                return "Pas";
            }

            // ─── OYUN ZORLAYICI (GF) ELLER ──────────────────────────────────
            // 2♦ checkback
            if (hp >= 13)
            {
                // Sıçramalı renk (6'lı kaliteli veya 5-5) forsing
                // 3 seviyesinde sıçrama yapılabiliyorsa onu tercih et.

                // 6'lı kaliteli renk kontrolü
                if (majörUzunluk >= 6 && KaliteliRenkMi(el, cevapciIlkRenk))
                {
                    // Aynı renge sıçra (3♥/3♠)
                    string sembol = KozYardimcisi.KozSembolu(cevapciIlkRenk);
                    return "3" + sembol;
                }

                // 5-5 iki renk kontrolü
                if (majörUzunluk >= 5)
                {
                    // Diğer majör veya iyi minör 5'li mi?
                    string digerMajör = cevapciIlkRenk == "Maça" ? "Kupa" : "Maça";
                    if (ElDegerlendirici.RenkUzunlugu(el, digerMajör) >= 5)
                    {
                        string sembol = KozYardimcisi.KozSembolu(digerMajör);
                        return "3" + sembol;   // 5-5 majör forsing
                    }
                }

                // Aksi halde 2♦ GF checkback
                return "2♦";
            }

            // ─── DAVET ELLERİ (8-12 HP) ─────────────────────────────────────
            // 2NT natürel davet
            if (hp >= 10 && hp <= 12 && dengeli)
            {
                return "2NT";
            }

            // 2♣ üzerinden davet
            // 2 seviyesinde majör: 5+ renk davet
            // veya 1♠ sonrası 2♥: 5+♠ + 4+♥ davet
            if (hp >= 8 && hp <= 12)
            {
                // 1♠-2♥ davet (5+♠ + 4+♥)
                if (cevapciIlkRenk == "Maça"
                    && ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4
                    && hp >= 10)
                {
                    // Bu 2♥ doğrudan davet olur mu? Evet, 2 seviyesinde majör davet.
                    return "2♥";
                }

                // 2 seviyesinde majör tekrarı (5+ renk, 8-12 HP davet)
                if (majörUzunluk >= 5)
                {
                    string sembol = KozYardimcisi.KozSembolu(cevapciIlkRenk);
                    return "2" + sembol;
                }

                // Diğer davet elleri 2♣'den geçer
                return "2♣";
            }

            // Çok zayıf ve uygun değil
            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ 2♦'YE CEVABI (GF)
        // ═══════════════════════════════════════════════════════════════════
        // Standart öncelik sırası:
        //   1) Ortağın majörüne 3'lü destek
        //   2) Diğer 4'lü majör
        //   3) İyi minör tekrarı
        //   4) 2NT
        // ═══════════════════════════════════════════════════════════════════

        private string AciciIkiKaroyaCevap(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            // Ortağın majör rengini bul (1X - 1Y - 1NT - 2♦)
            // Y = ortağın ilk teklifi (1♥ veya 1♠)
            string ortakMajör = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu != durum.AktifOyuncu
                    && h.Seviye == 1
                    && (h.Koz == "Kupa" || h.Koz == "Maça"))
                {
                    ortakMajör = h.Koz;
                    break;
                }
            }

            if (string.IsNullOrEmpty(ortakMajör)) return "2NT";

            int ortakMajörUzunlugu = ElDegerlendirici.RenkUzunlugu(el, ortakMajör);
            int digerMajörUzunlugu = 0;
            string digerMajör = null;
            if (ortakMajör == "Maça")
            {
                digerMajör = "Kupa";
                digerMajörUzunlugu = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            }
            else
            {
                digerMajör = "Maça";
                digerMajörUzunlugu = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            }

            // 1) Ortağın majörüne 3'lü destek
            if (ortakMajörUzunlugu >= 3)
            {
                string sembol = KozYardimcisi.KozSembolu(ortakMajör);
                return "2" + sembol;
            }

            // 2) Diğer 4'lü majör
            if (digerMajörUzunlugu >= 4)
            {
                string sembol = KozYardimcisi.KozSembolu(digerMajör);
                return "2" + sembol;
            }

            // 3) İyi minör tekrarı
            // Açıcının ilk rengini bul (1♣ veya 1♦)
            string aciciMinör = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.AktifOyuncu
                    && h.Seviye == 1
                    && (h.Koz == "Sinek" || h.Koz == "Karo"))
                {
                    aciciMinör = h.Koz;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(aciciMinör))
            {
                int minörUzunluk = ElDegerlendirici.RenkUzunlugu(el, aciciMinör);
                if (minörUzunluk >= 4 && KaliteliRenkMi(el, aciciMinör))
                {
                    string sembol = KozYardimcisi.KozSembolu(aciciMinör);
                    return "3" + sembol;
                }
            }

            // 4) 2NT
            return "2NT";
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Bir renk "kaliteli" mi? (En az 2 onör: A, K, Q, J)
        /// </summary>
        private bool KaliteliRenkMi(List<Card> el, string renk)
        {
            var onörler = el
                .Where(c => c.Suit == renk && c.Value >= 11)
                .ToList();
            return onörler.Count >= 2;
        }
    }
}