using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Multi-Landy — Rakip 1NT açışına müdahale konvansiyonu.
    /// 
    /// Cappelletti'nin 2♣ ve 2♦ anlamları yer değiştirilmiş versiyonu.
    /// 
    /// Yapı:
    ///   Dbl   = Ceza (16+ HP)
    ///   2♣    = Majörler (5-4 veya 5-5)
    ///   2♦    = Tek majör (6+ kart)
    ///   2♥    = ♥ (5+) + bilinmeyen minör (4+)
    ///   2♠    = ♠ (5+) + bilinmeyen minör (4+)
    ///   2NT   = Minörler (5-5)
    ///   3♣/3♦/3♥/3♠ = Doğal, oynamak için
    /// </summary>
    public class MultiLandy : IKonvansiyon
    {
        public string Ad => "Multi-Landy";
        public bool AktifMi { get; set; } = true;

        // Cappelletti (12) ile aynı öncelik
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

            if (gercekTeklifler.Count < 1) return false;

            // ─── SENARYO A: Rakip 1NT açtı, biz müdahale ediyoruz ───
            var sonTeklif = gercekTeklifler.Last();
            if (sonTeklif.Teklif == "1NT"
                && durum.Rakipler.Contains(sonTeklif.Oyuncu))
            {
                var el = durum.AktifOyuncuEli;

                // En az 6'lı renk var mı? (2♦ için)
                string[] renkler = { "Maça", "Kupa", "Karo", "Sinek" };
                bool altılıRenkVar = false;
                foreach (var renk in renkler)
                {
                    if (ElDegerlendirici.RenkUzunlugu(el, renk) >= 6)
                    {
                        altılıRenkVar = true;
                        break;
                    }
                }
                if (altılıRenkVar) return true;

                // İki renkli el var mı? (en az 4-4) (2♣, 2♥, 2♠ için)
                int dortluRenkSayisi = 0;
                foreach (var renk in renkler)
                {
                    if (ElDegerlendirici.RenkUzunlugu(el, renk) >= 4)
                        dortluRenkSayisi++;
                }
                if (dortluRenkSayisi >= 2) return true;

                // Minörler 5-5 (2NT için)
                if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 5
                    && ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 5)
                    return true;

                return false;
            }

            // ─── SENARYO B: Partner Multi-Landy müdahalesi yaptı ───
            var partnerTeklifleri = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner)
                .ToList();

            if (partnerTeklifleri.Count >= 1)
            {
                var pTeklif = partnerTeklifleri.Last();

                if (pTeklif.Teklif == "Dbl"
                    || pTeklif.Teklif == "2♣"
                    || pTeklif.Teklif == "2♦"
                    || pTeklif.Teklif == "2♥"
                    || pTeklif.Teklif == "2♠"
                    || pTeklif.Teklif == "2NT")
                {
                    // Bu tekliften önce rakip 1NT açmış mı?
                    var oncekiRakipTeklif = gercekTeklifler
                        .Where(h => durum.Rakipler.Contains(h.Oyuncu)
                                 && h.Teklif == "1NT")
                        .FirstOrDefault();

                    if (oncekiRakipTeklif != null)
                    {
                        // Partnerin bu teklifi ilk teklifi mi?
                        var partnerTumTeklifler = durum.Gecmis
                            .Where(h => h.Oyuncu == durum.Partner)
                            .ToList();

                        if (partnerTumTeklifler.Count >= 1
                            && partnerTumTeklifler[0] == pTeklif)
                        {
                            return true;
                        }
                    }
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

            // Rakip 1NT açtı, biz müdahale ediyoruz
            if (sonTeklif.Teklif == "1NT"
                && durum.Rakipler.Contains(sonTeklif.Oyuncu))
            {
                return MudahaleTeklifi(durum, el);
            }

            // Partner Multi-Landy müdahalesi yaptı, biz cevap veriyoruz
            return PartnerCevabi(durum, el, gercekTeklifler);
        }

        // ═══════════════════════════════════════════════════════════════════
        // RAKİP 1NT'YE MÜDAHALE
        // ═══════════════════════════════════════════════════════════════════

        private string MudahaleTeklifi(IhaleDurumu durum, List<Card> el)
        {
            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // ─── Dbl = Ceza (16+ HP dengeli) ───
            if (hp >= 16 && dengeli)
                return "Dbl";

            if (hp < 8) return "Pas";

            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // ─── 2♣ = Majörler (5-4 veya 5-5) ───
            if (macca >= 4 && kupa >= 4)
                return "2♣";

            // ─── 2♦ = Tek majör (6+ kart) ───
            if (macca >= 6) return "2♦";
            if (kupa >= 6) return "2♦";

            // ─── 2♥ = ♥ (5+) + minör (4+) ───
            if (kupa >= 5)
            {
                if (karo >= 4 || sinek >= 4)
                    return "2♥";
            }

            // ─── 2♠ = ♠ (5+) + minör (4+) ───
            if (macca >= 5)
            {
                if (karo >= 4 || sinek >= 4)
                    return "2♠";
            }

            // ─── 2NT = Minörler (5-5) ───
            if (karo >= 5 && sinek >= 5)
                return "2NT";

            // ─── 3♣/3♦/3♥/3♠ = Doğal, oynamak için ───
            if (macca >= 7) return "3♠";
            if (kupa >= 7) return "3♥";
            if (karo >= 7) return "3♦";
            if (sinek >= 7) return "3♣";

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // PARTNERİN MULTI-LANDY MÜDAHALESİNE CEVAP
        // ═══════════════════════════════════════════════════════════════════

        private string PartnerCevabi(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            var partnerTeklif = durum.PartnerTeklifleri.Last();

            // ─── Partner Dbl dedi → Pas (ceza) ───
            if (partnerTeklif == "Dbl")
            {
                return "Pas";
            }

            // ─── Partner 2♣ dedi (majörler) ───
            if (partnerTeklif == "2♣")
            {
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                // Uzun majörü seç
                if (macca > kupa) return "2♠";
                if (kupa > macca) return "2♥";
                // Eşit uzunlukta 2♦ sor
                return "2♦";
            }

            // ─── Partner 2♦ dedi (tek majör) ───
            if (partnerTeklif == "2♦")
            {
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                // Pass or correct
                if (kupa >= macca) return "2♥";
                return "2♠";
            }

            // ─── Partner 2♥/2♠ dedi (majör + minör) ───
            if (partnerTeklif == "2♥" || partnerTeklif == "2♠")
            {
                string majör = partnerTeklif == "2♥" ? "Kupa" : "Maça";
                string sembol = KozYardimcisi.KozSembolu(majör);

                int majörUzunluk = ElDegerlendirici.RenkUzunlugu(el, majör);

                // Majör fiti varsa Pas
                if (majörUzunluk >= 3) return "Pas";

                // Fit yoksa minör sorusu (3♣ pass or correct)
                return "3♣";
            }

            // ─── Partner 2NT dedi (minörler) ───
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