using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Standart Trial Bid (Yardım Rengi Denemesi / Help Suit Game Try)
    /// 
    /// Tetikleyici: 1M - 2M sonrası açıcının sırası (M = ♥ veya ♠)
    /// 
    /// Amaç: Açıcı, 5'li majör açıp cevapçıdan 2 seviyesinde basit destek
    /// aldıktan sonra (6-9 HP, 3+ destek), zona gidip gidemeyeceğini
    /// anlamak için "yardıma ihtiyaç duyduğu rengi" sorar.
    /// 
    /// Mantık:
    ///   - Açıcının teklif ettiği yeni renk = yardım ihtiyacı olan renk
    ///   - Cevapçı o renkte A veya K (veya kısa renk) tutuyorsa → 4M (kabul)
    ///   - Cevapçı o renkte 3 kayıp taş tutuyorsa → 3M (ret, signoff)
    ///   - Cevapçı o renkte 2 kayıp taş tutuyorsa → puan durumuna göre
    /// 
    /// Örnek:
    ///   1♥ - 2♥ - 2♠ = "♠'den yardım lazım, oyun oynayalım mı?"
    ///   Cevapçı ♠ A veya K tutuyorsa → 4♥
    ///   Cevapçı ♠'de 3 küçük kart tutuyorsa → 3♥
    /// </summary>
    public class TrialBid : IKonvansiyon
    {
        public string Ad => "Trial Bid";
        public bool AktifMi { get; set; } = true;

        // BasitCevap (100)'dan önce, ama diğer slam konvansiyonlarından sonra.
        // Smolen (26), GSF (28) ile çakışmasın diye 27'ye koyuyoruz.
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

            if (gercekTeklifler.Count < 2) return false;

            // İki senaryo:
            // A) Açıcının Trial Bid başlatma sırası (1M - 2M sonrası)
            // B) Cevapçının Trial Bid cevap verme sırası (Trial sonrası)

            if (AciTrialBidBaslatmaMi(durum, gercekTeklifler))
                return true;

            if (CevapciTrialCevapMi(durum, gercekTeklifler))
                return true;

            return false;
        }

        /// <summary>
        /// Açıcının Trial Bid başlatma sırası mı?
        /// İhale: 1M (partner) - 2M (aktif oyuncu = açıcı... hmm)
        /// 
        /// DİKKAT: Açıcı "partner" değil, "aktif oyuncu" olabilir.
        /// Sen 1M'yi açtın, partner 2M dedi → aktif oyuncu = SEN (açıcı)
        /// Bu yüzden: partner son konuşan (2M), aktif oyuncu (bizim açtığımız 1M var).
        /// </summary>
        private bool AciTrialBidBaslatmaMi(IhaleDurumu durum,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int n = gercekTeklifler.Count;
            if (n < 2) return false;

            // Son iki teklif: 1M (aktif oyuncu) - 2M (partner)
            var sonTeklif = gercekTeklifler[n - 1];
            var oncekiTeklif = gercekTeklifler[n - 2];

            // 2M partner tarafından
            if (sonTeklif.Oyuncu != durum.Partner) return false;
            if (sonTeklif.Seviye != 2) return false;
            if (sonTeklif.Koz != "Kupa" && sonTeklif.Koz != "Maça") return false;

            // 1M aktif oyuncu (bizim) tarafından
            if (oncekiTeklif.Oyuncu != durum.AktifOyuncu) return false;
            if (oncekiTeklif.Seviye != 1) return false;
            if (oncekiTeklif.Koz != sonTeklif.Koz) return false; // Aynı majör

            // Partner 2M = basit destek (6-9 HP, 3+ destek)
            // Biz 1M açtık → Trial Bid başlatabiliriz
            return true;
        }

        /// <summary>
        /// Cevapçının Trial Bid cevabı sırası mı?
        /// İhale: 1M (partner) - 2M (biz) - [Trial] (partner) - ?
        /// 
        /// Partner 1M açtı, biz 2M dedik, partner Trial Bid başlattı.
        /// Şimdi bizim cevap verme sıramız.
        /// </summary>
        private bool CevapciTrialCevapMi(IhaleDurumu durum,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int n = gercekTeklifler.Count;
            if (n < 3) return false;

            // Son teklif: Trial Bid (partner tarafından)
            var trialTeklif = gercekTeklifler[n - 1];
            if (trialTeklif.Oyuncu != durum.Partner) return false;
            if (trialTeklif.Seviye < 2 || trialTeklif.Seviye > 3) return false;
            if (trialTeklif.Koz == "NT") return false; // NT Trial değil

            // Bu teklif bir "yeni renk" mi? (kozal değil)
            // Koyal majörü bulalım
            string kozMajör = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.Partner && h.Seviye == 1
                    && (h.Koz == "Kupa" || h.Koz == "Maça"))
                {
                    kozMajör = h.Koz;
                    break;
                }
            }
            if (string.IsNullOrEmpty(kozMajör)) return false;

            // Trial teklifi kozal majör değil mi? (yeni renk olmalı)
            if (trialTeklif.Koz == kozMajör) return false;

            // Biz 2M dedik mi?
            bool bizIkiMDedik = gercekTeklifler.Any(h =>
                h.Oyuncu == durum.AktifOyuncu
                && h.Seviye == 2
                && h.Koz == kozMajör);
            if (!bizIkiMDedik) return false;

            // Başka teklif var mı arada? (Rakip teklifleri sayılmaz)
            // Sadece partner ve bizim tekliflerimiz olmalı
            var ortaklikTeklifleri = gercekTeklifler
                .Where(h => h.Oyuncu == durum.AktifOyuncu
                         || h.Oyuncu == durum.Partner)
                .ToList();

            // Bu 3 teklif olmalı: 1M (partner), 2M (biz), Trial (partner)
            if (ortaklikTeklifleri.Count != 3) return false;

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

            // Açıcı mıyız? (Trial Bid başlatacağız)
            if (AciTrialBidBaslatmaMi(durum, gercekTeklifler))
                return AciTrialBid(durum, el, gercekTeklifler);

            // Cevapçı mıyız? (Trial Bid cevabı vereceğiz)
            if (CevapciTrialCevapMi(durum, gercekTeklifler))
                return CevapciTrialCevap(durum, el, gercekTeklifler);

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ TRIAL BID BAŞLATMASI
        // ═══════════════════════════════════════════════════════════════════
        // 
        // 1♥ - 2♥ sonrası:
        //   2♠ = yardım sorusu (♠ renginde yardım ihtiyacı)
        //   Açıcı ♠ dışında yardım arıyorsa Trial Bid başlatmaz
        //   (Basit 4♥ der veya Pas der)
        // 
        // 1♠ - 2♠ sonrası:
        //   3♣/3♦/3♥ = yardım sorusu (hangi renk)
        //   Açıcı yardım ihtiyacı duyduğu rengi söyler
        // 
        // Basitleştirme: Açıcı Trial Bid başlatmak için
        //   - 14+ HP olmalı (manş denemesi)
        //   - Yardım ihtiyacı olan bir renk olmalı (A veya K yok, 3+ kart)
        // ═══════════════════════════════════════════════════════════════════

        private string AciTrialBid(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            int hp = ElDegerlendirici.HCP(el);

            // Kozal majörü bul
            string kozMajör = gercekTeklifler
                .Last(h => h.Oyuncu == durum.AktifOyuncu && h.Seviye == 1)
                .Koz;

            // 1♥ - 2♥ sonrası → sadece 2♠ Trial Bid
            if (kozMajör == "Kupa")
            {
                // Yardım ihtiyacı var mı? ♠'de A veya K yok mu?
                // (Basitleştirme: ♠'de 3+ kart ve A/K yok ise yardım iste)
                int spUzunluk = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                bool spAKVar = el.Any(c => c.Suit == "Maça"
                    && (c.Value == 14 || c.Value == 13));

                // 14+ HP ve ♠'de yardım ihtiyacı varsa 2♠ Trial
                if (hp >= 14 && !spAKVar && spUzunluk >= 3)
                    return "2♠";

                // Yardım ihtiyacı yok ama manş denemesi → 3♥ veya 4♥
                if (hp >= 16)
                    return "4♥"; // Direkt manş

                return "Pas";
            }

            // 1♠ - 2♠ sonrası → 3♣/3♦/3♥ Trial Bid
            // Açıcı yardım ihtiyacı duyduğu rengi söyler.
            // Öncelik: yardım ihtiyacı olan rengi bul
            if (kozMajör == "Maça")
            {
                // ♥, ♦, ♣ sırasında yardım ihtiyacı ara
                // (Uzun ama A/K olmayan ilk renk)

                // ♥ kontrolü
                int kupaUzunluk = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                bool kupaAKVar = el.Any(c => c.Suit == "Kupa"
                    && (c.Value == 14 || c.Value == 13));
                if (hp >= 14 && !kupaAKVar && kupaUzunluk >= 3)
                    return "3♥"; // ♥'den yardım

                // ♦ kontrolü
                int karoUzunluk = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                bool karoAKVar = el.Any(c => c.Suit == "Karo"
                    && (c.Value == 14 || c.Value == 13));
                if (hp >= 14 && !karoAKVar && karoUzunluk >= 3)
                    return "3♦"; // ♦'dan yardım

                // ♣ kontrolü
                int sinekUzunluk = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
                bool sinekAKVar = el.Any(c => c.Suit == "Sinek"
                    && (c.Value == 14 || c.Value == 13));
                if (hp >= 14 && !sinekAKVar && sinekUzunluk >= 3)
                    return "3♣"; // ♣'ten yardım

                // Yardım ihtiyacı yok ama manş denemesi → 4♠
                if (hp >= 16)
                    return "4♠";

                return "Pas";
            }

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAPÇININ TRIAL BID CEVABI
        // ═══════════════════════════════════════════════════════════════════
        // 
        // Partner Trial Bid başlattı → bizim cevap sıramız.
        // Partner'in istediği renkte A veya K varsa → kabul et (4M)
        // 3 küçük kart varsa → ret (3M)
        // 2 kayıp kart varsa → puan durumuna göre
        // ═══════════════════════════════════════════════════════════════════

        private string CevapciTrialCevap(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            // Trial Bid hangi renk?
            var trialTeklif = gercekTeklifler.Last();
            string trialRenk = trialTeklif.Koz;

            // Kozal majörü bul
            string kozMajör = null;
            foreach (var h in gercekTeklifler)
            {
                if (h.Oyuncu == durum.Partner && h.Seviye == 1
                    && (h.Koz == "Kupa" || h.Koz == "Maça"))
                {
                    kozMajör = h.Koz;
                    break;
                }
            }
            if (string.IsNullOrEmpty(kozMajör)) return "Pas";

            string kozSembol = KozYardimcisi.KozSembolu(kozMajör);

            // Trial renginde yardım var mı?
            bool yardimVar = YardimVarMi(el, trialRenk);

            // Yardım varsa → 4M (manş kabul)
            if (yardimVar)
                return "4" + kozSembol;

            // Yardım yoksa → 3M (signoff, daha düşük seviyede kal)
            return "3" + kozSembol;
        }

        /// <summary>
        /// Trial renginde yardım var mı?
        /// Basitleştirilmiş kayıp taş hesabı:
        ///   - A varsa → 0 kayıp → yardım var
        ///   - K varsa → 1 kayıp → yardım var (Kx veya Kxx)
        ///   - QJ varsa → 1-2 kayıp → yardım var
        ///   - 3+ küçük kart → 3 kayıp → yardım yok
        ///   - 0-1 kart → kısa renk → yardım var (void/singleton)
        /// </summary>
        private bool YardimVarMi(List<Card> el, string renk)
        {
            var renkKartlari = el
                .Where(c => c.Suit == renk)
                .OrderByDescending(c => c.Value)
                .ToList();

            // 0-1 kart (void veya singleton) → yardım var
            if (renkKartlari.Count <= 1) return true;

            // A veya K var mı?
            if (renkKartlari.Any(c => c.Value == 14)) return true; // A
            if (renkKartlari.Any(c => c.Value == 13)) return true; // K

            // QJ kombinasyonu var mı? (Q ve J yan yana)
            bool qVar = renkKartlari.Any(c => c.Value == 12);
            bool jVar = renkKartlari.Any(c => c.Value == 11);
            if (qVar && jVar) return true;

            // 2 kart varsa ve en az Q varsa → yardım var
            if (renkKartlari.Count == 2 && qVar) return true;

            // Diğer durumlar → yardım yok
            return false;
        }
    }
}