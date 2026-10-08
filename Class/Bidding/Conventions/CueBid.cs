using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Cue Bid (Kontrol Gösterimi) Konvansiyonu (SAYC Standard).
    ///
    /// Kural:
    /// - Partner ile bir MAJÖR kozda anlaşıldı (Maça veya Kupa).
    /// - Seviye genellikle 3 veya 4'te, slam denemesi sırasında kullanılır.
    /// - Elimde 15+ HP ve koz dışındaki yan renklerde kontrol (As veya Papaz) var.
    /// - O rengi teklif ederek "bu renkte kontrolüm var, slam düşünelim" mesajı verilir.
    ///
    /// Öncelik: 14 (Blackwood 15'ten ÖNCE).
    /// </summary>
    public class CueBid : IKonvansiyon
    {
        public string Ad => "Cue Bid";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 14;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            if (durum.IhaleBittiMi()) return false;

            // 1. Koz anlaşması majör olmalı
            string koz = durum.AnlasilanKoz();
            if (koz != "Maça" && koz != "Kupa") return false;

            // 2. Seviye kontrolü (Genellikle 3. veya 4. seviyede slam hazırlığı)
            int seviye = durum.KacinciSeviye();
            if (seviye < 3 || seviye > 4) return false;

            // 3. Slam potansiyeli (15+ HP veya güçlü Quick Tricks)
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            double qt = ElDegerlendirici.HizliElSayisi(durum.AktifOyuncuEli);
            if (hp < 15 && qt < 6.0) return false;

            // 4. Yan renklerde kontrol var mı?
            string kontrolRengi = KontroluOlanYanRengiBul(durum.AktifOyuncuEli, koz, durum);
            if (string.IsNullOrEmpty(kontrolRengi)) return false;

            // 5. Bu renk daha önce cue bid olarak kullanıldı mı?
            if (DahaOnceCueBidYapildiMi(durum, koz)) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string koz = durum.AnlasilanKoz();

            string kontrolRengi = KontroluOlanYanRengiBul(el, koz, durum);
            if (string.IsNullOrEmpty(kontrolRengi)) return "Pas";

            // Cue bid genellikle mevcut seviyeyi artırarak veya 4 seviyesinde yapılır
            int seviye = durum.KacinciSeviye();
            if (seviye < 4) seviye = 4;

            return $"{seviye}{KozSembolu(kontrolRengi)}";
        }

        private bool DahaOnceCueBidYapildiMi(IhaleDurumu durum, string koz)
        {
            var teklifler = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();

            foreach (var h in teklifler)
            {
                if (!string.IsNullOrEmpty(h.Koz) && h.Koz != koz && h.Koz != "NT")
                {
                    // Eğer bu teklif 3. veya 4. seviyede yapıldıysa cue bid sayılır
                    if (h.Seviye >= 3) return true;
                }
            }
            return false;
        }

        private string KontroluOlanYanRengiBul(List<Card> el, string koz, IhaleDurumu durum)
        {
            // Öncelik sırası: Sinek -> Karo -> Diğer Majör
            var renkSirasi = new List<string> { "Sinek", "Karo", "Kupa", "Maça" };

            foreach (var renk in renkSirasi)
            {
                if (renk == koz) continue;

                // Bu renk daha önce doğal olarak teklif edildiyse cue bid anlamsızdır
                if (BuRenkDahaOnceTeklifEdildiMi(durum, renk)) continue;

                // Kontrol var mı? (As veya Papaz)
                if (ElDegerlendirici.PapazVar(el, renk) ||
                    el.Any(c => c.Suit == renk && c.Value == 14))
                {
                    return renk;
                }
            }
            return null;
        }

        private bool BuRenkDahaOnceTeklifEdildiMi(IhaleDurumu durum, string renk)
        {
            return durum.Gecmis.Any(h => h.GercekTeklifMi && h.Koz == renk && h.Seviye <= 2);
        }

        private string KozSembolu(string koz)
        {
            return koz switch
            {
                "Maça" => "♠",
                "Kupa" => "♥",
                "Karo" => "♦",
                "Sinek" => "♣",
                "NT" => "NT",
                _ => "?"
            };
        }
    }
}
