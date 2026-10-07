using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Cue Bid (Kontrol Gösterimi) Konvansiyonu.
    /// 
    /// Kural:
    /// - Partner ile bir MAJÖR kozda anlaşıldı (Maça veya Kupa)
    /// - Seviye 3 veya 4'te (genelde 3. seviyeden sonra)
    /// - Elimde 15+ HP var (slam denemesi)
    /// - Koz dışında bir renkte KONTROL (A veya K) var
    /// - O rengi teklif ederim → "bu renkte kontrolüm var, slam düşünelim"
    /// 
    /// Öncelik: 14 (Blackwood 15'ten ÖNCE)
    /// 
    /// Örnek:
    ///   1♠ (Partner) - 3♠ (Ben) - Pas - Pas
    ///   Ben: 4♣ derim (Sinek'te kontrolüm var)
    ///   Partner: 4♦ der (Karo'da kontrolüm var) → ikimiz de cue bid yaptık
    ///   Ben: 4NT derim (Blackwood, artık As sorabilirim)
    /// 
    /// NOT: Cue Bid, Blackwood'dan ÖNCE denenir. Çünkü önce kontrol
    ///      gösterilir, sonra As sorulur.
    /// </summary>
    public class CueBid : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "Cue Bid";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 14;   // Blackwood (15) ÖNCESİ

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. İhale zaten bitmişse → hayır
            if (durum.IhaleBittiMi()) return false;

            // 2. Bir koz anlaşması var mı?
            string koz = durum.AnlasilanKoz();
            if (string.IsNullOrEmpty(koz)) return false;

            // 3. Koz MAJÖR mü? (Maça veya Kupa)
            if (koz != "Maça" && koz != "Kupa") return false;

            // 4. Seviye 3 veya 4 mü?
            int seviye = durum.KacinciSeviye();
            if (seviye < 3 || seviye > 4) return false;

            // 5. Elinde yeterli puan var mı? (slam denemesi için 15+ HP)
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 15) return false;

            // 6. Bu elde daha önce cue bid yapılmış mı?
            //    (Yani KendiTeklifleri içinde zaten bir cue bid var mı?)
            if (DahaOnceCueBidYapildiMi(durum, koz)) return false;

            // 7. Kontrolü olan bir yan renk var mı?
            string kontrolRengi = KontroluOlanYanRengiBul(
                durum.AktifOyuncuEli, koz, durum);

            if (string.IsNullOrEmpty(kontrolRengi)) return false;

            // Tüm koşullar sağlandı → Cue Bid yapılabilir
            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string koz = durum.AnlasilanKoz();

            // En ucuz kontrolü olan yan rengi bul
            string kontrolRengi = KontroluOlanYanRengiBul(el, koz, durum);
            if (string.IsNullOrEmpty(kontrolRengi)) return "Pas";

            // Seviyeyi belirle (mevcut seviyeden 1 fazlası veya 4)
            int seviye = durum.KacinciSeviye();

            // Cue bid genelde 4. seviyede yapılır
            if (seviye < 4) seviye = 4;

            // Rengi sembole çevir
            string sembol = KozSembolu(kontrolRengi);
            return $"{seviye}{sembol}";
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI METOTLAR
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Bu elde daha önce cue bid yapılmış mı?
        /// (Hem benim hem partnerin tekliflerini kontrol eder.)
        /// </summary>
        private bool DahaOnceCueBidYapildiMi(IhaleDurumu durum, string koz)
        {
            // Tüm gerçek teklifleri tara
            var teklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            foreach (var h in teklifler)
            {
                // Koz dışında bir renk mi teklif edilmiş?
                if (!string.IsNullOrEmpty(h.Koz) &&
                    h.Koz != koz &&
                    h.Koz != "NT")
                {
                    // Ve bu renk cue bid için uygun mu?
                    // (Yani partnerin gösterdiği bir renk değil, yeni bir renk)
                    // Basit kontrol: Bu renk daha önce "doğal" olarak teklif edilmiş mi?
                    bool dogalTeklif = durum.Gecmis.Any(gg =>
                        gg.GercekTeklifMi &&
                        gg.Koz == h.Koz &&
                        gg.Sira < h.Sira &&
                        (gg.Seviye == 1 || gg.Seviye == 2));

                    // Eğer 1-2 seviyesinde doğal teklif edilmişse ve şimdi 4 seviyesinde
                    // teklif ediliyorsa → cue bid
                    if (dogalTeklif && h.Seviye >= 4)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Koz dışında, elde kontrol (A veya K) olan en ucuz yan rengi bulur.
        /// Öncelik: Sinek → Karo → Kupa (Maça koz ise)
        /// </summary>
        private string KontroluOlanYanRengiBul(
            List<Card> el, string koz, IhaleDurumu durum)
        {
            // Yan renkleri sırayla dene: Sinek → Karo → diğer majör
            var renkSirasi = new List<string> { "Sinek", "Karo", "Kupa", "Maça" };

            foreach (var renk in renkSirasi)
            {
                // Koz rengi atla
                if (renk == koz) continue;

                // Bu renk daha önce cue bid olarak kullanıldı mı?
                if (BuRenkDahaOnceTeklifEdildiMi(durum, renk, koz)) continue;

                // Bu renkte kontrol var mı? (A veya K)
                if (ElDegerlendirici.PapazVar(el, renk) ||
                    ElDegerlendirici.AsSayisi(el.Where(c => c.Suit == renk).ToList()) > 0)
                {
                    // Bu renk uygun → döndür
                    return renk;
                }
            }

            return null;
        }

        /// <summary>
        /// Bu renk daha önce (cue bid dışı) teklif edilmiş mi?
        /// Eğer partner zaten bu rengi doğal olarak teklif ettiyse, cue bid anlamsız.
        /// </summary>
        private bool BuRenkDahaOnceTeklifEdildiMi(
            IhaleDurumu durum, string renk, string koz)
        {
            // Bu renk daha önce (1-2 seviyesinde) teklif edildiyse ve 
            // şu an 4 seviyesinde cue bid yapılacaksa → zaten bilinen bir renk, atla
            bool dogalTeklifVar = durum.Gecmis.Any(h =>
                h.GercekTeklifMi &&
                h.Koz == renk &&
                h.Seviye >= 1 &&
                h.Seviye <= 2);

            return dogalTeklifVar;
        }

        /// <summary>
        /// Renk adını sembole çevirir.
        /// </summary>
        private string KozSembolu(string koz)
        {
            switch (koz)
            {
                case "Maça": return "♠";
                case "Kupa": return "♥";
                case "Karo": return "♦";
                case "Sinek": return "♣";
                case "NT": return "NT";
                default: return "?";
            }
        }
    }
}