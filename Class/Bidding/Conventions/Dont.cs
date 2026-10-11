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
    /// Örnek:
    ///   (1NT) - Dbl - (Pas) - 2♣ (zorunlu) - (Pas) - 2♥/2♠/3♣/3♦ (gerçek renk)
    ///   (1NT) - 2♣ - (Pas) - Pas (3+ ♣) / 2♦ (diğer renk sorusu)
    ///   (1NT) - 2♦ - (Pas) - 2♥ (majör sorusu) / 2♠ (uzun ♠)
    /// </summary>
    public class Dont : IKonvansiyon
    {
        public string Ad => "DONT";
        public bool AktifMi { get; set; } = true;

        // Cappelletti (12)'den sonra, ama diğerlerinden önce
        public int Oncelik => 12;

        private static readonly string[] DontTeklifleri =
            { "Dbl", "2♣", "2♦", "2♥", "2♠", "2NT" };

        private static readonly string[] Renkler =
            { "Maça", "Kupa", "Karo", "Sinek" };

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI: Partner DONT müdahalesi yaptı mı?
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Rakip 1NT açtıktan sonra partnerin yaptığı tek Pas-olmayan teklif
        /// bir DONT teklifiyse onu döner, değilse null.
        /// 
        /// DİKKAT: Dbl "gerçek teklif" değildir (GercekTeklifMi=false), bu yüzden
        /// durum.PartnerTeklifleri veya GercekTeklifMi filtresi yerine tüm
        /// geçmişe (Gecmis) bakılır.
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

            // Partner 1NT'den sonra yalnızca bir kez teklif yapmış ve bu DONT teklifi olmalı
            if (partnerTeklifleri.Count == 1
                && DontTeklifleri.Contains(partnerTeklifleri[0].Teklif))
            {
                return partnerTeklifleri[0].Teklif;
            }

            return null;
        }

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.Gecmis == null) return false;
            if (durum.AktifOyuncuEli == null) return false;

            // ─── SENARYO B (önce): Partner DONT yaptı, biz cevap veriyoruz ───
            // Cevap, el dağılımından bağımsızdır (ör. Dbl'a 2♣ zorunlu),
            // bu yüzden Senaryo A'nın el kontrolünden ÖNCE bakılır.
            if (PartnerDontTeklifi(durum) != null) return true;

            // ─── SENARYO A: Rakip 1NT açtı, biz müdahale ediyoruz ───
            var gercekTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            if (gercekTeklifler.Count < 1) return false;

            var sonTeklif = gercekTeklifler.Last();
            if (sonTeklif.Teklif == "1NT"
                && durum.Rakipler.Contains(sonTeklif.Oyuncu))
            {
                var el = durum.AktifOyuncuEli;

                // En az 6'lı renk var mı?
                if (Renkler.Any(r => ElDegerlendirici.RenkUzunlugu(el, r) >= 6))
                    return true;

                // İki renkli el var mı? (en az 4-4)
                if (Renkler.Count(r => ElDegerlendirici.RenkUzunlugu(el, r) >= 4) >= 2)
                    return true;
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

            // ─── ÖNCE: Partner DONT müdahalesi yaptı mı? ───
            string partnerTeklif = PartnerDontTeklifi(durum);
            if (partnerTeklif != null)
                return PartnerCevabi(el, partnerTeklif);

            // ─── SONRA: Rakip 1NT açtı, biz müdahale edeceğiz ───
            var gercekTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            if (gercekTeklifler.Count >= 1)
            {
                var sonTeklif = gercekTeklifler.Last();
                if (sonTeklif.Teklif == "1NT"
                    && durum.Rakipler.Contains(sonTeklif.Oyuncu))
                {
                    return MudahaleTeklifi(el);
                }
            }

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // RAKİP 1NT'YE MÜDAHALE
        // ═══════════════════════════════════════════════════════════════════

        private string MudahaleTeklifi(List<Card> el)
        {
            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // ─── 2♣: ♣ + daha yüksek bir renk ───
            if (sinek >= 4)
            {
                if (macca >= 4 || kupa >= 4 || karo >= 4)
                    return "2♣";
            }

            // ─── 2♦: ♦ + bir majör ───
            if (karo >= 4)
            {
                if (macca >= 4 || kupa >= 4)
                    return "2♦";
            }

            // ─── 2♥: ♥ + ♠ (majörler) ───
            if (kupa >= 4 && macca >= 4)
                return "2♥";

            // ─── 2♠: Doğal, 6+ ♠ ───
            if (macca >= 6)
                return "2♠";

            // ─── Dbl: Tek renkli el (6+ kart) ───
            if (macca >= 6 || kupa >= 6 || karo >= 6 || sinek >= 6)
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
            {
                return "2♣";
            }

            // ─── Partner 2♣ dedi (♣ + yüksek renk) ───
            if (partnerTeklif == "2♣")
            {
                int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
                // 3+ ♣ varsa Pas
                if (sinek >= 3) return "Pas";
                // Yoksa 2♦ sor (diğer renk)
                return "2♦";
            }

            // ─── Partner 2♦ dedi (♦ + majör) ───
            if (partnerTeklif == "2♦")
            {
                int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                // 3+ ♦ varsa Pas
                if (karo >= 3) return "Pas";
                // Yoksa 2♥ sor (majör sorusu)
                return "2♥";
            }

            // ─── Partner 2♥ dedi (♥ + ♠) ───
            if (partnerTeklif == "2♥")
            {
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                // ♥ daha iyiyse Pas
                if (kupa >= macca) return "Pas";
                // ♠ daha iyiyse 2♠
                return "2♠";
            }

            // ─── Partner 2♠ dedi (doğal 6+ ♠) → Pas ───
            if (partnerTeklif == "2♠")
            {
                return "Pas";
            }

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
