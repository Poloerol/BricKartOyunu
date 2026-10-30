using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Bergen Raises (Standart)
    /// 
    /// Marty Bergen tarafından 1982'de geliştirildi.
    /// 5'li majör sisteminde 1♥/1♠ açışına verilen yapay cevaplar.
    /// 
    /// Toplam Löve Kuralı: Ortaklıkta 9 koz varsa, HP'den bağımsız
    /// olarak en az 3 seviyesine kadar rekabet edilmelidir.
    /// 
    /// Standart yapı:
    ///   1M - 2M     = Zayıf basit destek (7-10 HP, 3'lü)
    ///   1M - 3♣     = Yapıcı (7-10 HP, 4'lü)
    ///   1M - 3♦     = Limit raise / davet (11-12 HP, 4'lü)
    ///   1M - 3M     = Preemptive (0-6 HP, 4'lü)
    ///   1M - 4M     = Preemptive (0-6 HP, 5'li)
    /// 
    /// 3♣ sonrası açıcının 3♦ sorusu:
    ///   Cevapçı 7-8 HP → 3M (signoff)
    ///   Cevapçı 9-10 HP → 4M (kabul)
    /// </summary>
    public class BergenRaises : IKonvansiyon
    {
        public string Ad => "Bergen Raises";
        public string Aciklama => "Majör açılışına Bergen destek cevapları";
        public bool AktifMi { get; set; } = true;

        // BesliMajor (10)'dan sonra, Stayman (11)'dan sonra,
        // diğer konvansiyonlardan önce
        public int Oncelik => 12;

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

            // Rakip araya girmiş mi?
            if (durum.RakipActiMi()) return false;

            // SENARYO A: Cevapçının Bergen başlatma sırası
            // Geçmiş SADECE 1 teklif içermeli: partner'in 1♥/1♠ açışı.
            // Başka teklif varsa (rebid, XYZ, vs.) Bergen tetiklenmez.
            if (gercekTeklifler.Count == 1)
            {
                var tek = gercekTeklifler[0];
                if (tek.Oyuncu == durum.Partner
                    && tek.Seviye == 1
                    && (tek.Koz == "Kupa" || tek.Koz == "Maça"))
                {
                    // Destek kontrolü: en az 4'lü destek yoksa Bergen tetiklenmez
                    int destek = ElDegerlendirici.RenkUzunlugu(
                        durum.AktifOyuncuEli, tek.Koz);
                    if (destek >= 4)
                    {
                        // HP kontrolü: sadece Bergen aralığında (0-12) tetiklen
                        int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
                        if (hp <= 12)
                            return true;
                    }
                    return false;
                }
                return false;
            }

            // SENARYO B: Açıcının 3♣'e cevap sırası
            // Geçmiş: 1M (aktif) - 3♣ (partner)
            // SENARYO C: 1M (partner) - 3♣ (ben) - 3♦ (partner, yapay soru) → cevapçı signoff / kabul
            if (gercekTeklifler.Count == 3)
                return CevapciDevamMi(durum, gercekTeklifler);

            if (gercekTeklifler.Count == 2)
            {
                var son = gercekTeklifler[1];
                var onceki = gercekTeklifler[0];

                if (son.Oyuncu == durum.Partner
                    && (son.Teklif == "3♣" || son.Teklif == "3♦")
                    && onceki.Oyuncu == durum.AktifOyuncu
                    && onceki.Seviye == 1
                    && (onceki.Koz == "Kupa" || onceki.Koz == "Maça"))
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        private bool CevapciDevamMi(IhaleDurumu durum, List<IhaleHamlesi> g)
        {
            if (g.Count != 3) return false;
            var a = g[0]; var b2 = g[1]; var c = g[2];
            return a.Oyuncu == durum.Partner && a.Seviye == 1 && (a.Koz == "Kupa" || a.Koz == "Maça")
                && b2.Oyuncu == durum.AktifOyuncu && b2.Teklif == "3♣"
                && c.Oyuncu == durum.Partner && c.Teklif == "3♦";
        }

        /// <summary>
        /// Cevapçının Bergen başlatma sırası mı?
        /// 
        /// Senaryo: 1♥ (partner) - ? veya 1♠ (partner) - ?
        /// </summary>
        private bool CevapciSirasiMi(IhaleDurumu durum,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int n = gercekTeklifler.Count;
            if (n < 1) return false;

            // Son teklif: partner 1♥ veya 1♠ açtı
            var sonTeklif = gercekTeklifler[n - 1];
            if (sonTeklif.Oyuncu != durum.Partner) return false;
            if (sonTeklif.Seviye != 1) return false;
            if (sonTeklif.Koz != "Kupa" && sonTeklif.Koz != "Maça") return false;

            // Sıra cevapçıda (aktif oyuncu)
            return true;
        }

        /// <summary>
        /// Açıcının Bergen cevabı sırası mı?
        /// 
        /// Senaryo: 1♥ (aktif) - 3♣ (partner) - ? (açıcı 3♦ sorusu)
        /// </summary>
        private bool AciciCevapSirasiMi(IhaleDurumu durum,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int n = gercekTeklifler.Count;
            if (n < 2) return false;

            // Son teklif: partner 3♣ dedi
            var sonTeklif = gercekTeklifler[n - 1];
            if (sonTeklif.Oyuncu != durum.Partner) return false;
            if (sonTeklif.Teklif != "3♣" && sonTeklif.Teklif != "3♦") return false;

            // Sondan bir önceki: aktif oyuncu 1♥/1♠ açtı
            var oncekiTeklif = gercekTeklifler[n - 2];
            if (oncekiTeklif.Oyuncu != durum.AktifOyuncu) return false;
            if (oncekiTeklif.Seviye != 1) return false;
            if (oncekiTeklif.Koz != "Kupa" && oncekiTeklif.Koz != "Maça") return false;

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

            if (gercekTeklifler.Count == 3 && CevapciDevamMi(durum, gercekTeklifler))
            {
                // 3♦ yapay sorusuna cevap: 7-8 HP → 3M (signoff), 9-10 HP → 4M (kabul)
                string m = gercekTeklifler[0].Koz;
                string sm = KozYardimcisi.KozSembolu(m);
                return ElDegerlendirici.HCP(el) >= 9 ? "4" + sm : "3" + sm;
            }

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAPÇININ BERGEN BAŞLATMASI
        // ═══════════════════════════════════════════════════════════════════

        private string CevapciTeklifi(IhaleDurumu durum, List<Card> el,
    List<IhaleHamlesi> gercekTeklifler)
        {
            int hp = ElDegerlendirici.HCP(el);

            var sonTeklif = gercekTeklifler.Last();
            string majör = sonTeklif.Koz;
            string sembol = KozYardimcisi.KozSembolu(majör);

            int destek = ElDegerlendirici.RenkUzunlugu(el, majör);

            // 5'li destek + çok zayıf (0-6 HP) → 4M
            if (destek >= 5 && hp <= 6)
                return "4" + sembol;

            // 4'lü destek + çok zayıf (0-6 HP) → 3M
            if (hp <= 6)
                return "3" + sembol;

            // 4'lü destek + zayıf (7-10 HP) → 3♣
            if (hp >= 7 && hp <= 10)
                return "3♣";

            // 4'lü destek + davet (11-12 HP) → 3♦
            if (hp >= 11 && hp <= 12)
                return "3♦";

            // 13+ HP → Jacoby 2NT veya Splinter devralır
            // (UygunMu zaten hp <= 12 kontrolü yapıyor, buraya düşmemeli)
            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ BERGEN CEVABI (3♣ SONRASI)
        // ═══════════════════════════════════════════════════════════════════
        // 
        // 3♣ (zayıf, 7-10 HP) sonrası açıcı 3♦ yapay sorusu sorar:
        //   Cevapçı 7-8 HP → 3M (signoff)
        //   Cevapçı 9-10 HP → 4M (kabul)
        // 
        // ═══════════════════════════════════════════════════════════════════

        private string AciciCevap(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            // Açıcının majör rengini bul
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

            // 3♦ = limit raise (11-12 HP): açıcı 15+ HP (ya da 14 + 6'lı koz) ile 4M, aksi halde 3M
            if (gercekTeklifler[gercekTeklifler.Count - 1].Teklif == "3♦")
            {
                string smj = KozYardimcisi.KozSembolu(majör);
                int ah = ElDegerlendirici.HCP(el);
                int kozUz = ElDegerlendirici.RenkUzunlugu(el, majör);
                return (ah >= 15 || (ah >= 14 && kozUz >= 6)) ? "4" + smj : "3" + smj;
            }

            // 3♣ sonrası açıcı 3♦ yapay sorusu sorar
            // Bu soruyu her zaman sormak zorunda değil, ama standart olarak sorar
            return "3♦";
        }
    }
}