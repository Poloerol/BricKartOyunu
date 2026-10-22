using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// DONT (Disturb Opponents' No Trump) — Rakip 1NT açışına müdahale.
    /// 
    /// Marty Bergen tarafından geliştirildi. Amaç: Rakibin 1NT açışını bozmak.
    /// 
    /// Yapı:
    ///   Dbl   = Tek renkli el (6+ kart). Partner 2♣ der, doubler gerçek rengini söyler.
    ///   2♣    = ♣ + daha yüksek bir renk (4-4 veya 5-4)
    ///   2♦    = ♦ + bir majör (4-4 veya 5-4)
    ///   2♥    = ♥ + ♠ (majörler)
    ///   2♠    = Doğal, 6+ ♠
    ///   2NT   = Güçlü iki renkli el
    /// 
    /// HP eşikleri:
    ///   Direct seat  : 9+ HP
    ///   Balancing    : 7+ HP  (1NT - Pas - Pas sonrası)
    /// 
    /// Örnek:
    ///   (1NT) - Dbl - (Pas) - 2♣ (zorunlu) - (Pas) - 2♥/2♠/3♣/3♦ (gerçek renk)
    ///   (1NT) - 2♣ - (Pas) - Pas (3+ ♣) / 2♦ (diğer renk sorusu)
    ///   (1NT) - 2♦ - (Pas) - 2♥ (majör sorusu) / 2♠ (uzun ♠)
    /// 
    /// Değişiklik günlüğü:
    ///   - UygunMu Durum 2: Dbl "gerçek teklif" olmadığı için SonGercekTeklif/PartnerTeklifleri
    ///     üzerinden aranıyordu ve hiç tetiklenmiyordu. Artık PartnerDontTeklifi (Gecmis tabanlı)
    ///     kullanılıyor; Dbl ve 2♣/2♦/2♥/2♠/2NT cevapları aynı kapıdan geçiyor.
    ///   - Balancing (1NT - Pas - Pas) için ayrı HP eşiği (7+), direct için 9+.
    ///   - MudahaleTeklifi: doğal 6+ ♠ (2♠) iki renkli dallardan önce kontrol edilir.
    ///   - Dbl - 2♣ sonrası partner gerçek rengini söyleyince Pas (devam dalı).
    /// </summary>
    public class Dont : IKonvansiyon
    {
        public string Ad => "DONT";
        public string Aciklama => "1NT açışına DONT müdahalesi";
        public bool AktifMi { get; set; } = true;

        // Cappelletti (12)'den sonra, ama diğerlerinden önce
        public int Oncelik => 12;

        private const int DirectHpEsigi = 9;
        private const int BalancingHpEsigi = 7;

        private static readonly string[] DontTeklifleri =
            { "Dbl", "2♣", "2♦", "2♥", "2♠", "2NT" };

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI METOTLAR
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Rakip 1NT açtıktan sonra partnerin yaptığı tek Pas-olmayan teklif
        /// bir DONT teklifiyse onu döner, değilse null.
        /// 
        /// DİKKAT: Dbl "gerçek teklif" değildir (GercekTeklifMi=false), bu yüzden
        /// tüm geçmişe (Gecmis) bakılır.
        /// </summary>
        private string PartnerDontTeklifi(IhaleDurumu durum)
        {
            var liste = durum.Gecmis.ToList();
            int birNtIndex = liste.FindIndex(h =>
                h.Teklif == "1NT" && durum.Rakipler.Contains(h.Oyuncu));
            if (birNtIndex < 0) return null;

            var partnerTeklifleri = liste
                .Skip(birNtIndex + 1)
                .Where(h => h.Oyuncu == durum.Partner && h.Teklif != "Pas")
                .ToList();

            if (partnerTeklifleri.Count == 1
                && DontTeklifleri.Contains(partnerTeklifleri[0].Teklif))
            {
                return partnerTeklifleri[0].Teklif;
            }

            return null;
        }

        /// <summary>
        /// 1NT'den sonra BEN (ne partner ne rakip olan oyuncu) Pas-olmayan
        /// kaç teklif yaptım?
        /// </summary>
        private int BenimTekliflerimSayisi(IhaleDurumu durum)
        {
            var liste = durum.Gecmis.ToList();
            int birNtIndex = liste.FindIndex(h =>
                h.Teklif == "1NT" && durum.Rakipler.Contains(h.Oyuncu));
            if (birNtIndex < 0) return 0;

            return liste
                .Skip(birNtIndex + 1)
                .Count(h => h.Oyuncu != durum.Partner
                         && !durum.Rakipler.Contains(h.Oyuncu)
                         && h.Teklif != "Pas");
        }

        /// <summary>
        /// Dbl - 2♣ dizisinden sonra partner gerçek rengini söyledi mi?
        /// (Partner: Dbl + ikinci bir renk teklifi, ben: 2♣.)
        /// </summary>
        private bool DblSonrasiPartnerGercekRenkSoyledi(IhaleDurumu durum)
        {
            var liste = durum.Gecmis.ToList();
            int birNtIndex = liste.FindIndex(h =>
                h.Teklif == "1NT" && durum.Rakipler.Contains(h.Oyuncu));
            if (birNtIndex < 0) return false;

            var sonrasi = liste.Skip(birNtIndex + 1).Where(h => h.Teklif != "Pas").ToList();
            if (sonrasi.Count < 3) return false;

            return sonrasi[0].Oyuncu == durum.Partner && sonrasi[0].Teklif == "Dbl"
                && sonrasi[1].Oyuncu != durum.Partner
                && !durum.Rakipler.Contains(sonrasi[1].Oyuncu)
                && sonrasi[1].Teklif == "2♣"
                && sonrasi[2].Oyuncu == durum.Partner;
        }

        /// <summary>Balancing seat: 1NT - Pas - Pas - ?</summary>
        private bool BalancingMi(IhaleDurumu durum)
        {
            return durum.UstUstePasSayisi >= 2;
        }

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // ═══════════════════════════════════════════════════════════════
            // DURUM 2: Partnerim DONT ile müdahale etti → cevap ver
            // (Dbl dahil; Dbl gerçek teklif olmadığından Gecmis üzerinden bakılır)
            // ═══════════════════════════════════════════════════════════════
            if (PartnerDontTeklifi(durum) != null && BenimTekliflerimSayisi(durum) == 0)
                return true;

            // ═══════════════════════════════════════════════════════════════
            // DURUM 3: Dbl - 2♣ sonrası partner gerçek rengini söyledi → Pas
            // ═══════════════════════════════════════════════════════════════
            if (DblSonrasiPartnerGercekRenkSoyledi(durum))
                return true;

            // ═══════════════════════════════════════════════════════════════
            // DURUM 1: Ben DONT ile müdahale ediyorum (Rakip 1NT açtı)
            // ═══════════════════════════════════════════════════════════════
            if (durum.SonGercekTeklif == "1NT" &&
                durum.SonGercekTeklifSahibi != null &&
                durum.Rakipler.Contains(durum.SonGercekTeklifSahibi.Value))
            {
                // Sadece ilk açılış 1NT ise (1♦-1♥-1NT gibi rebid değilse)
                var gercekTeklifler = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
                if (gercekTeklifler.Count == 1)
                {
                    int esik = BalancingMi(durum) ? BalancingHpEsigi : DirectHpEsigi;

                    bool besliRenkVar = (macca >= 5 || kupa >= 5 || karo >= 5 || sinek >= 5);
                    bool dortluIkiRenkVar = ((macca >= 4 && kupa >= 4) || (macca >= 4 && karo >= 4) ||
                                             (macca >= 4 && sinek >= 4) || (kupa >= 4 && karo >= 4) ||
                                             (kupa >= 4 && sinek >= 4) || (karo >= 4 && sinek >= 4));
                    bool altiliRenkVar = (macca >= 6 || kupa >= 6 || karo >= 6 || sinek >= 6);

                    if (hp >= esik && (besliRenkVar || dortluIkiRenkVar || altiliRenkVar))
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

            // ─── Dbl - 2♣ - (partner gerçek renk) → Pas ───
            if (DblSonrasiPartnerGercekRenkSoyledi(durum))
                return "Pas";

            // ─── Partner DONT müdahalesi yaptı mı? ───
            string partnerTeklif = PartnerDontTeklifi(durum);
            if (partnerTeklif != null)
                return PartnerCevabi(el, partnerTeklif);

            // ─── Rakip 1NT açtı, biz müdahale edeceğiz ───
            var gercekTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            if (gercekTeklifler.Count >= 1)
            {
                var sonTeklif = gercekTeklifler.Last();
                if (sonTeklif.Teklif == "1NT"
                    && durum.Rakipler.Contains(sonTeklif.Oyuncu))
                {
                    int esik = BalancingMi(durum) ? BalancingHpEsigi : DirectHpEsigi;
                    return MudahaleTeklifi(el, esik);
                }
            }

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // RAKİP 1NT'YE MÜDAHALE
        // ═══════════════════════════════════════════════════════════════════

        private string MudahaleTeklifi(List<Card> el, int hpEsigi)
        {
            int hp = ElDegerlendirici.HCP(el);
            if (hp < hpEsigi) return "Pas";

            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // ─── 2♠: Doğal, 6+ ♠ (iki renkli dallardan ÖNCE) ───
            if (macca >= 6)
                return "2♠";

            // ─── 2♣: ♣ + daha yüksek bir renk ───
            if (sinek >= 4 && (macca >= 4 || kupa >= 4 || karo >= 4))
                return "2♣";

            // ─── 2♦: ♦ + bir majör ───
            if (karo >= 4 && (macca >= 4 || kupa >= 4))
                return "2♦";

            // ─── 2♥: ♥ + ♠ (majörler) ───
            if (kupa >= 4 && macca >= 4)
                return "2♥";

            // ─── Dbl: Tek renkli el (6+ kart) ───
            if (kupa >= 6 || karo >= 6 || sinek >= 6)
                return "Dbl";

            // ─── 2NT: Güçlü iki renkli el (basitleştirilmiş) ───
            if (karo >= 5 && sinek >= 5)
                return "2NT";

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // PARTNERİN DONT MÜDAHALESİNE CEVAP
        // ═══════════════════════════════════════════════════════════════════

        private string PartnerCevabi(List<Card> el, string partnerTeklif)
        {
            // ─── Partner Dbl dedi (tek renkli) → 2♣ zorunlu ───
            if (partnerTeklif == "Dbl")
                return "2♣";

            // ─── Partner 2♣ dedi (♣ + yüksek renk) ───
            if (partnerTeklif == "2♣")
            {
                int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
                if (sinek >= 3) return "Pas";
                return "2♦";
            }

            // ─── Partner 2♦ dedi (♦ + majör) ───
            if (partnerTeklif == "2♦")
            {
                int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                if (karo >= 3) return "Pas";
                return "2♥";
            }

            // ─── Partner 2♥ dedi (♥ + ♠) ───
            if (partnerTeklif == "2♥")
            {
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                if (kupa >= macca) return "Pas";
                return "2♠";
            }

            // ─── Partner 2♠ dedi (doğal 6+ ♠) → Pas ───
            if (partnerTeklif == "2♠")
                return "Pas";

            // ─── Partner 2NT dedi (güçlü iki renkli: ♦ + ♣) ───
            if (partnerTeklif == "2NT")
            {
                int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
                if (karo >= sinek) return "3♦";
                return "3♣";
            }

            return "Pas";
        }
    }
}