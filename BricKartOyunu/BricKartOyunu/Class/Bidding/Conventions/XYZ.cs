using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// XYZ Konvansiyonu (X-Y-Z)
    /// 
    /// Tetikleyici: 1X - 1Y - 1Z (üç teklif de 1 seviyesinde, üç farklı renk)
    /// Örnek: 1♦ - 1♥ - 1♠ veya 1♣ - 1♥ - 1♠ veya 1♦ - 1♠ - 1NT
    /// 
    /// Ana prensipler:
    ///   2♣ → Puppet (zorunlu 2♦'ya). Zayıf el veya davet eli.
    ///   2♦ → Game Forcing (GF). Tüm forsing eller.
    ///   3♣ → Oynamak için (sign-off).
    ///   3♦/3♥/3♠ → GF, iyi renk (slam olasılığı).
    ///   3NT → Doğal, oynamak için.
    /// 
    /// Özel kurallar (1♦-1♥-1♠ sonrası):
    ///   1NT → Oynamak için (signoff)
    ///   2♥  → 5+ ♥ ile oynamak için (signoff)
    ///   2♠  → 4'lü ♠ fiti ile oynamak için (signoff)
    /// 
    /// İki Yönlü Checkback'ten farkı:
    ///   - XY(Z) 1 seviyesindeki 3 farklı renkten sonra tetiklenir
    ///   - Two Way Checkback 1NT rebid'inden sonra tetiklenir
    /// </summary>
    public class XYZ : IKonvansiyon
    {
        public string Ad => "XYZ (X-Y-Z)";
        public string Aciklama => "XYZ yapay 2♣ ve 2♦ sekansları";
        public bool AktifMi { get; set; } = true;

        // Two Way Checkback (25)'ten sonra, ama BasitCevap (100)'dan önce.
        // Smolen (26), GSF (28) ile çakışmasın diye 24'e koyuyoruz.
        public int Oncelik => 24;

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

            int n = gercekTeklifler.Count;

            // İki senaryo var:
            // SENARYO A — Cevapçının sırası (XYZ tetiklenmesi)
            //   Son üç teklif: 1X (partner), 1Y (aktif oyuncu), 1Z (partner)
            //   Yani aktif oyuncu bu turda konuştu, partner son sözü söyledi.
            //
            // SENARYO B — Açıcının sırası (XYZ cevabı)
            //   Son teklifler: ... 2♣ (aktif oyuncunun ortağı), ...
            //   veya 2♦ (aktif oyuncunun ortağı), ...
            //   Yani cevapçı 2♣/2♦ dedi, şimdi açıcının cevap vermesi gerekiyor.

            // ─── SENARYO A: Cevapçının XYZ tetiklemesi ─────────────────
            if (CevapciSirasiMi(durum, gercekTeklifler, n))
                return true;

            // ─── SENARYO B: Açıcının 2♣/2♦'ye cevabı ────────────────────
            if (AciciCevapSirasiMi(durum, gercekTeklifler, n))
                return true;

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // SENARYO KONTROLLERİ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Cevapçının XYZ tetiklemesi mi?
        /// 1X - 1Y - 1Z şeklinde, üç teklif de 1 seviyesinde ve farklı renkler.
        /// Sıra: aktif oyuncu (cevapçı) şimdi konuşacak.
        /// </summary>
        private bool CevapciSirasiMi(IhaleDurumu durum,
            List<IhaleHamlesi> gercekTeklifler, int n)
        {
            if (n < 3) return false;

            // Son üç teklif
            var teklif3 = gercekTeklifler[n - 1]; // açıcı: 1Z
            var teklif2 = gercekTeklifler[n - 2]; // cevapçı: 1Y
            var teklif1 = gercekTeklifler[n - 3]; // açıcı: 1X

            // Hepsi 1 seviyesinde mi?
            if (teklif1.Seviye != 1 || teklif2.Seviye != 1 || teklif3.Seviye != 1)
                return false;

            // Üçü de NT değil mi? (1NT XYZ'yi tetiklemez; TwoWayCheckback konusu)
            if (teklif1.Koz == "NT" || teklif2.Koz == "NT" || teklif3.Koz == "NT")
                return false;

            // Üç farklı renk mi?
            var renkler = new HashSet<string> { teklif1.Koz, teklif2.Koz, teklif3.Koz };
            if (renkler.Count != 3) return false;

            // Sıra doğru mu? 
            // teklif1 açıcı (partner), teklif2 cevapçı (aktif), teklif3 açıcı (partner)
            if (teklif1.Oyuncu != durum.Partner) return false;
            if (teklif2.Oyuncu != durum.AktifOyuncu) return false;
            if (teklif3.Oyuncu != durum.Partner) return false;

            // Y'nin majör olduğundan emin olalım (1♥ veya 1♠)
            if (teklif2.Koz != "Kupa" && teklif2.Koz != "Maça") return false;

            return true;
        }

        /// <summary>
        /// Açıcının 2♣/2♦'ye cevap sırası mı?
        /// Geçmiş: 1X (partner) - 1Y (aktif) - 1Z (partner) - 2♣/2♦ (aktif) - ...
        /// Şimdi sıra açıcıda (partner'da).
        /// </summary>
        private bool AciciCevapSirasiMi(IhaleDurumu durum,
            List<IhaleHamlesi> gercekTeklifler, int n)
        {
            if (n < 4) return false;

            // Son teklif: 2♣ veya 2♦ (aktif oyuncunun ortağı tarafından)
            var sonTeklif = gercekTeklifler[n - 1];
            if (sonTeklif.Oyuncu != durum.Partner) return false;
            if (sonTeklif.Teklif != "2♣" && sonTeklif.Teklif != "2♦") return false;

            // Bu tekliften önce 1X - 1Y - 1Z dizisi var mı?
            if (n < 4) return false;
            var teklif3 = gercekTeklifler[n - 2]; // aktif oyuncunun teklifi
            var teklif2 = gercekTeklifler[n - 3]; // partner
            var teklif1 = gercekTeklifler[n - 4]; // aktif oyuncu

            if (teklif1.Seviye != 1 || teklif2.Seviye != 1 || teklif3.Seviye != 1)
                return false;

            var renkler = new HashSet<string> { teklif1.Koz, teklif2.Koz, teklif3.Koz };
            if (renkler.Count != 3) return false;

            // Sıra: teklif1 (aktif), teklif2 (partner), teklif3 (aktif) → 1X-1Y-1Z
            if (teklif1.Oyuncu != durum.AktifOyuncu) return false;
            if (teklif2.Oyuncu != durum.Partner) return false;
            if (teklif3.Oyuncu != durum.AktifOyuncu) return false;

            // Y majör olmalı (1Y)
            if (teklif2.Koz != "Kupa" && teklif2.Koz != "Maça") return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VER
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null || el.Count != 13) return "Pas";

            var gercekTeklifler = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            int n = gercekTeklifler.Count;

            // Açıcı mıyız? (partnerimiz 2♣/2♦ dedi, biz cevap vereceğiz)
            if (AciciCevapSirasiMi(durum, gercekTeklifler, n))
            {
                var sonTeklif = gercekTeklifler[n - 1];
                if (sonTeklif.Teklif == "2♣")
                    return AciciIkiSinekeCevap(durum, el);
                else
                    return AciciIkiKaroyaCevap(durum, el);
            }

            // Cevapçıyız (XYZ tetikleyicisi)
            return CevapciTeklifi(durum, el);
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAPÇI TEKLİFİ — XYZ
        // ═══════════════════════════════════════════════════════════════════

        private string CevapciTeklifi(IhaleDurumu durum, List<Card> el)
        {
            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // Cevapçının Y rengini bul (1♥ veya 1♠)
            string yRenk = null;
            foreach (var h in durum.Gecmis)
            {
                if (h.Oyuncu == durum.AktifOyuncu && h.GercekTeklifMi)
                {
                    yRenk = h.Koz;
                    break;
                }
            }
            if (string.IsNullOrEmpty(yRenk)) return "Pas";

            // Açıcının X rengini bul (1♣ veya 1♦)
            string xRenk = null;
            foreach (var h in durum.Gecmis)
            {
                if (h.Oyuncu == durum.Partner && h.GercekTeklifMi && h.Seviye == 1)
                {
                    xRenk = h.Koz;
                    break;
                }
            }

            // Açıcının Z rengini bul (1Z, son teklif)
            var sonTeklif = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .LastOrDefault();
            string zRenk = sonTeklif?.Koz;

            int yUzunluk = ElDegerlendirici.RenkUzunlugu(el, yRenk);
            int spUzunluk = ElDegerlendirici.RenkUzunlugu(el, "Maça");

            // ═══════════════════════════════════════════════════════════════
            // OYUN ZORLAYICI (13+ HP) — 2♦ GF checkback
            // ═══════════════════════════════════════════════════════════════
            if (hp >= 13)
            {
                // Sıçramalı 6'lı kaliteli renk varsa forsing
                if (yUzunluk >= 6 && KaliteliRenkMi(el, yRenk))
                {
                    string sembol = KozYardimcisi.KozSembolu(yRenk);
                    return "3" + sembol;
                }

                // 5-5 majör varsa forsing
                if (yUzunluk >= 5)
                {
                    string digerMajör = yRenk == "Maça" ? "Kupa" : "Maça";
                    if (ElDegerlendirici.RenkUzunlugu(el, digerMajör) >= 5)
                    {
                        string sembol = KozYardimcisi.KozSembolu(digerMajör);
                        return "3" + sembol;
                    }
                }

                // Standart 2♦ GF checkback
                return "2♦";
            }

            // ═══════════════════════════════════════════════════════════════
            // ZAYIF EL (0-7 HP): 1♦-1♥-1♠ diziliminde özel kurallar
            // ═══════════════════════════════════════════════════════════════
            if (hp <= 7)
            {
                // Özel dizilim: 1♦-1♥-1♠
                if (xRenk == "Karo" && yRenk == "Kupa" && zRenk == "Maça")
                {
                    // 1NT = oynamak için (5+♥ yok, 4'lü ♠ yok)
                    if (yUzunluk < 5 && spUzunluk < 4)
                        return "1NT";

                    // 2♥ = 5+ ♥ ile oynamak için
                    if (yUzunluk >= 5)
                        return "2♥";

                    // 2♠ = 4'lü ♠ fiti ile oynamak için
                    if (spUzunluk >= 4)
                        return "2♠";

                    return "1NT";
                }

                // Diğer dizilimler: 5+ Y ile 2Y signoff
                if (yUzunluk >= 5)
                {
                    string ySembol = KozYardimcisi.KozSembolu(yRenk);
                    return "2" + ySembol;
                }

                // Uzun minör varsa 2♣ puppet ile göster
                int karoUzunluk = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                int sinekUzunluk = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

                if (karoUzunluk >= 5 || sinekUzunluk >= 5)
                    return "2♣";

                return "Pas";
            }

            // ═══════════════════════════════════════════════════════════════
            // DAVET ELİ (8-12 HP)
            // ═══════════════════════════════════════════════════════════════

            // Natürel 2NT davet (dengeli, 10-12 HP, majör fit yok)
            if (hp >= 10 && hp <= 12 && dengeli
                && yUzunluk < 5 && spUzunluk < 4)
            {
                return "2NT";
            }

            // 1♦-1♥-1♠ diziliminde 2♣ üzerinden davet
            if (xRenk == "Karo" && yRenk == "Kupa" && zRenk == "Maça")
            {
                return "2♣";
            }

            // Diğer dizilimlerde davet elleri 2♣'den geçer
            return "2♣";
        }

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

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ 2♣'E CEVABI (zorunlu 2♦)
        // ═══════════════════════════════════════════════════════════════════

        private string AciciIkiSinekeCevap(IhaleDurumu durum, List<Card> el)
        {
            // Puppet: her zaman zorunlu 2♦
            return "2♦";
        }

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ 2♦'YE CEVABI (GF checkback)
        // ═══════════════════════════════════════════════════════════════════
        // Öncelik sırası (standart XYZ):
        //   1) Cevapçının Y rengine 3+ destek
        //   2) Diğer 4'lü majör
        //   3) 5+ X (açılış rengi)
        //   4) 2NT (dengeli)
        // ═══════════════════════════════════════════════════════════════════

        private string AciciIkiKaroyaCevap(IhaleDurumu durum, List<Card> el)
        {
            // Y (cevapçının ilk rengi)
            string yRenk = null;
            // X (açıcının ilk rengi)
            string xRenk = null;
            // Z (açıcının rebid rengi)
            string zRenk = null;

            var gercekTeklifler = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();

            for (int i = 0; i < gercekTeklifler.Count; i++)
            {
                var h = gercekTeklifler[i];
                if (h.Oyuncu == durum.AktifOyuncu && h.Seviye == 1 && i == 0)
                    xRenk = h.Koz;
                else if (h.Oyuncu != durum.AktifOyuncu && h.Seviye == 1)
                    yRenk = h.Koz;
            }

            // Son 1Z (açıcının rebid'i)
            var sonBirSeviye = gercekTeklifler
                .Where(h => h.Oyuncu == durum.AktifOyuncu && h.Seviye == 1)
                .LastOrDefault();
            if (sonBirSeviye != null && sonBirSeviye != gercekTeklifler[0])
                zRenk = sonBirSeviye.Koz;

            int yUzunluk = !string.IsNullOrEmpty(yRenk)
                ? ElDegerlendirici.RenkUzunlugu(el, yRenk)
                : 0;

            int zUzunluk = !string.IsNullOrEmpty(zRenk)
                ? ElDegerlendirici.RenkUzunlugu(el, zRenk)
                : 0;

            // 1) Y'ye 3+ destek
            if (yUzunluk >= 3)
            {
                string sembol = KozYardimcisi.KozSembolu(yRenk);
                return "2" + sembol;
            }

            // 2) Diğer 4'lü majör
            string digerMajör = null;
            if (yRenk == "Maça") digerMajör = "Kupa";
            else if (yRenk == "Kupa") digerMajör = "Maça";

            if (digerMajör != null
                && ElDegerlendirici.RenkUzunlugu(el, digerMajör) >= 4)
            {
                string sembol = KozYardimcisi.KozSembolu(digerMajör);
                return "2" + sembol;
            }

            // 3) Z'ye 5+ destek (açıcının rebid rengi)
            if (zUzunluk >= 5)
            {
                string sembol = KozYardimcisi.KozSembolu(zRenk);
                return "3" + sembol;
            }

            // 4) 2NT (dengeli, minör ağırlıklı)
            return "2NT";
        }
    }
}