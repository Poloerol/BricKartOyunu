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
    public class Dont : IKonvansiyon, IBilincliPas
    {
        public string Ad => "DONT";
        public string Aciklama => "1NT açışına DONT müdahalesi";
        public bool AktifMi { get; set; } = true;

        // Cappelletti (12)'den sonra, ama diğerlerinden önce
        public int Oncelik => 12;

        private const int DirectHpEsigi = 9;
        private const int BalancingHpEsigi = 7;

        private static readonly string[] DontTeklifleri =
            { "Dbl", "2♣", "2♦", "2♥", "2♠" };

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

        /// <summary>
        /// Müdahaleci devamı: ben (1NT'ye) DONT teklifi yaptım, partner TEK bir cevap verdi.
        ///   2♦ (♦+majör) - 2♥ (röle)  → majörüm ♥ ise Pas, ♠ ise 2♠
        ///   2♣ (♣+yüksek) - 2♦ (röle) → yüksek rengimi söyle (♦ ise Pas)
        ///   Dbl (tek renk) - 2♣       → gerçek rengimi söyle (♣ ise Pas)
        ///   2♥ (majörler) - 2♠        → Pas
        /// Bu devam yoktu: partner 2♥ röle deyince Kuzey 3 kupa ile 2♥'de kalıyordu.
        /// </summary>
        private bool MudahaleciDevami(IhaleDurumu durum, List<Card> el, out string teklif)
        {
            teklif = null;
            var liste = durum.Gecmis.ToList();
            int birNt = liste.FindIndex(h => h.Teklif == "1NT" && durum.Rakipler.Contains(h.Oyuncu));
            if (birNt < 0) return false;

            var sonrasi = liste.Skip(birNt + 1).ToList();
            int benIdx = sonrasi.FindIndex(h => h.Oyuncu == durum.AktifOyuncu && h.Teklif != "Pas");
            if (benIdx < 0) return false;
            string benim = sonrasi[benIdx].Teklif;
            if (!DontTeklifleri.Contains(benim)) return false;

            // Ben bir daha konuşmadım; benden önce partner bir şey dememişti
            if (sonrasi.Skip(benIdx + 1).Any(h => h.Oyuncu == durum.AktifOyuncu && h.Teklif != "Pas")) return false;
            if (sonrasi.Take(benIdx).Any(h => h.Oyuncu == durum.Partner && h.Teklif != "Pas")) return false;

            var cevaplar = sonrasi.Skip(benIdx + 1)
                .Where(h => h.Oyuncu == durum.Partner && h.Teklif != "Pas").ToList();
            if (cevaplar.Count != 1) return false;
            string r = cevaplar[0].Teklif;

            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            if (benim == "2♦" && r == "2♥")
            {
                // majörüm: uzun olan (eşitse ♠)
                teklif = (kupa > macca) ? "Pas" : "2♠";
                return true;
            }
            if (benim == "2♣" && r == "2♦")
            {
                // ♣ + daha yüksek renk: en uzun yüksek renk (eşitse majör tercih)
                if (macca >= kupa && macca >= karo) teklif = "2♠";
                else if (kupa >= karo) teklif = "2♥";
                else teklif = "Pas";
                return true;
            }
            if (benim == "Dbl" && r == "2♣")
            {
                if (macca >= 6) teklif = "2♠";
                else if (kupa >= 6) teklif = "2♥";
                else if (karo >= 6) teklif = "3♦";
                else teklif = "Pas";    // ♣ tek renk
                return true;
            }
            if (benim == "2♥" && r == "2♠") { teklif = "Pas"; return true; }

            // Partner 2NT (güçlü davet): ikinci rengimi 3'lü seviyede söyle
            if (r == "2NT")
            {
                if (benim == "2♣")
                {
                    if (macca >= kupa && macca >= karo) teklif = "3♠";
                    else if (kupa >= karo) teklif = "3♥";
                    else teklif = "3♦";
                    return true;
                }
                if (benim == "2♦") { teklif = (kupa > macca) ? "3♥" : "3♠"; return true; }
                if (benim == "2♥") { teklif = "3♠"; return true; }
            }

            // Partner kendi uzun rengini doğal söyledi (röle olmayan yeni renk) → Pas
            if (benim == "2♣" || benim == "2♦" || benim == "Dbl")
            {
                teklif = "Pas";
                return true;
            }
            return false;
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
            // DURUM 4: BEN DONT ile müdahale ettim, partner cevap verdi (röle) → devam
            // ═══════════════════════════════════════════════════════════════
            string devam;
            if (MudahaleciDevami(durum, el, out devam)) return true;

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

                    // Evrensel DONT: en az 5-4 iki renkli ya da 6+ kartlık tek renkli
                    bool altiliRenkVar = (macca >= 6 || kupa >= 6 || karo >= 6 || sinek >= 6);

                    if (hp >= esik && (IkiRenkliTeklifi(el) != null || altiliRenkVar))
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

            // ─── Ben DONT ile müdahale ettim, partner röle / cevap verdi → pass-or-correct ───
            string devamTeklifi;
            if (MudahaleciDevami(durum, el, out devamTeklifi))
                return devamTeklifi;

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

        /// <summary>
        /// Evrensel DONT iki renkli kuralı: en az 5-4 dağılım; ALT rengi söyle.
        ///   ♣ + daha yüksek bir renk → 2♣
        ///   ♦ + bir majör            → 2♦
        ///   ♥ + ♠                    → 2♥
        /// En alttaki uygun çifti seçer. Uygun çift yoksa null.
        /// </summary>
        private string IkiRenkliTeklifi(List<Card> el)
        {
            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            Func<int, int, bool> ikiRenk = (alt, ust) => (alt >= 5 && ust >= 4) || (alt >= 4 && ust >= 5);

            if (ikiRenk(sinek, Math.Max(karo, Math.Max(kupa, macca)))) return "2♣";
            if (ikiRenk(karo, Math.Max(kupa, macca))) return "2♦";
            if (ikiRenk(kupa, macca)) return "2♥";
            return null;
        }

        private string MudahaleTeklifi(List<Card> el, int hpEsigi)
        {
            int hp = ElDegerlendirici.HCP(el);
            if (hp < hpEsigi) return "Pas";

            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // ─── İki renkli (en az 5-4): alt rengi söyle ───
            string iki = IkiRenkliTeklifi(el);
            if (iki != null) return iki;

            // ─── 2♠: doğal, 6+ ♠ ───
            if (macca >= 6) return "2♠";

            // ─── Dbl: 6+ kartlık tek renkli el (♣/♦/♥) ───
            if (kupa >= 6 || karo >= 6 || sinek >= 6)
                return "Dbl";

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // PARTNERİN DONT MÜDAHALESİNE CEVAP
        // ═══════════════════════════════════════════════════════════════════

        private string PartnerCevabi(List<Card> el, string partnerTeklif)
        {
            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int hp = ElDegerlendirici.HCP(el);

            // ─── Partner Dbl dedi (tek renkli) → 2♣ (çoğu elle); 6 kartlık majörüm varsa doğal ───
            if (partnerTeklif == "Dbl")
            {
                if (macca >= 6) return "2♠";
                if (kupa >= 6) return "2♥";
                return "2♣";
            }

            // ─── Partner 2♣ dedi (♣ + yüksek renk) ───
            if (partnerTeklif == "2♣")
            {
                if (sinek >= 3) return "Pas";                       // 3 kart destek → oyna
                if (hp >= 11 && karo >= 3 && kupa >= 3 && macca >= 3) return "2NT";   // güçlü davet
                if (kupa >= 6) return "2♥";                         // kendi 6'lı rengim (doğal)
                if (macca >= 6) return "2♠";
                return "2♦";                                        // röle: diğer rengini söyle / pas
            }

            // ─── Partner 2♦ dedi (♦ + majör) ───
            if (partnerTeklif == "2♦")
            {
                if (karo >= 3) return "Pas";                        // 3 kart destek → oyna
                if (hp >= 11 && kupa >= 3 && macca >= 3) return "2NT";                 // güçlü davet
                if (macca >= 6) return "2♠";                        // kendi 6'lı rengim (doğal)
                return "2♥";                                        // röle: majörünü söyle / pas
            }

            // ─── Partner 2♥ dedi (♥ + ♠): tercih ───
            if (partnerTeklif == "2♥")
            {
                if (hp >= 11 && kupa >= 3 && macca >= 3) return "2NT";                 // güçlü davet
                if (kupa >= macca) return "Pas";
                return "2♠";
            }

            // ─── Partner 2♠ dedi (doğal 6+ ♠) → Pas ───
            if (partnerTeklif == "2♠")
                return "Pas";

            return "Pas";
        }
    }
}
