using System;
using System.Linq;
using BricKartOyunu.Class.Bidding;   // KozYardimcisi için

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Negative Double (Negatif Kontr) Konvansiyonu.
    /// 
    /// Kural:
    /// - Partner bir renk açtı (1♣/1♦/1♥/1♠)
    /// - Rakip araya girdi (overcall veya DBL)
    /// - Benim elimde gösteremediğim bir renk var (özellikle 4'lü majör)
    /// - Dbl derim → "Diğer renkleri oynayabilirim" demek
    /// 
    /// Öncelik: 20
    /// </summary>
    public class NegativeDouble : IKonvansiyon
    {
        public string Ad => "Negative Double";
        public string Aciklama => "Rakip müdahalesine negatif kontr";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 20;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // ═══════════════════════════════════════════════════════════════
            // 1. PARTNER AÇTI MI? (Negative Double, partnerin açışına karşı yapılır)
            // ═══════════════════════════════════════════════════════════════
            if (!durum.PartnerActiMi()) return false;

            // ═══════════════════════════════════════════════════════════════
            // 2. RAKİP ARAYA GİRDİ Mİ? (Overcall veya DBL olmalı)
            // ═══════════════════════════════════════════════════════════════
            if (!durum.RakipActiMi()) return false;

            // ═══════════════════════════════════════════════════════════════
            // 3. BEN HENÜZ KONUŞMADIM MI?
            // ═══════════════════════════════════════════════════════════════
            if (durum.KendiTeklifleri.Count > 0) return false;

            // ═══════════════════════════════════════════════════════════════
            // 4. RAKİP 1 SEVİYESİNDE Mİ AÇTI? (2+ seviyede Negative Double farklı)
            // ═══════════════════════════════════════════════════════════════
            string sonRakipTeklifi = durum.RakipTeklifleri.LastOrDefault();
            if (string.IsNullOrEmpty(sonRakipTeklifi)) return false;

            int rakipSeviye = KozYardimcisi.TekliftenSeviyeCikar(sonRakipTeklifi);
            if (rakipSeviye != 1) return false;

            // ═══════════════════════════════════════════════════════════════
            // 5. EL ANALİZİ
            // ═══════════════════════════════════════════════════════════════
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);

            // Tabloya göre 1 seviyesi için 6+ HP yeterli
            if (hp < 6) return false;

            // ═══════════════════════════════════════════════════════════════
            // 6. RAKİP KOZUNU ÇIKAR
            // ═══════════════════════════════════════════════════════════════
            string rakipKozu = KozYardimcisi.TekliftenKozCikar(sonRakipTeklifi);
            if (string.IsNullOrEmpty(rakipKozu) || rakipKozu == "NT") return false;

            // ═══════════════════════════════════════════════════════════════
            // 7. 5-5 MAJÖR KONTROLÜ — Michaels'a bırak
            int macaUzun = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupaUzun = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            if (macaUzun >= 5 && kupaUzun >= 5) return false;

            // 8. Standart kural: Negatif kontr, HENÜZ TEKLİF EDİLMEMİŞ renkleri gösterir.
            //    - İki majör de boşta (1m-(1m)): 4-4 majör
            //    - Bir majör boşta (1♦-(1♥), 1♥-(2♣)...): o majörde 4+
            //    - İki majör de alınmış (1♥-(1♠)): 4-4 minör
            var partnerAcis = durum.Gecmis.FirstOrDefault(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi);
            string partnerKozu = partnerAcis != null ? partnerAcis.Koz : null;
            bool macaBosta = partnerKozu != "Maça" && rakipKozu != "Maça";
            bool kupaBosta = partnerKozu != "Kupa" && rakipKozu != "Kupa";
            // 5+ kartlı boştaki majör varsa Dbl DEĞİL, doğal renk teklifi (1♠/1♥) verilir;
            // negatif kontr "tam 4'lü" majörü gösterir.
            if (macaBosta && kupaBosta) return macaUzun == 4 && kupaUzun == 4;
            if (macaBosta) return macaUzun == 4;
            if (kupaBosta) return kupaUzun == 4;
            return ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 4 &&
                   ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 4;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "Dbl";
        }
    }
}