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
            // Anlaşmadaki sınır (1♠ … 3♠): rakip müdahalesi bu çağrıdan yüksek olmamalı
            {
                int lim = durum.Anlasma != null ? durum.Anlasma.NegDblLimit : 4;
                int[] limPoz = { 3, 5, 6, 7, 8, 10, 11, 12, 13 };
                if (lim < 0 || lim > 8) lim = 4;
                string rk0 = KozYardimcisi.TekliftenKozCikar(sonRakipTeklifi);
                int rkSira = rk0 == "Sinek" ? 0 : rk0 == "Karo" ? 1 : rk0 == "Kupa" ? 2 : rk0 == "Maça" ? 3 : 4;
                if ((rakipSeviye - 1) * 5 + rkSira > limPoz[lim]) return false;
            }

            // ═══════════════════════════════════════════════════════════════
            // 5. EL ANALİZİ
            // ═══════════════════════════════════════════════════════════════
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);

            // Tabloya göre 1 seviyesi için 6+ HP yeterli
            if (hp < (rakipSeviye >= 3 ? 10 : rakipSeviye == 2 ? 8 : 6)) return false;

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

    /// <summary>
    /// Ortak 1M açtı, rakip 2 seviyesinde bir renkle araya girdi, ben henüz konuşmadım:
    /// dengeli el + rakibin rengini durduruyorum + açıcının majöründe 3+ destek yok + kullanılmamış majörde 4+ kart yok
    ///   11-12 HP → 2NT (davet, doğal), 13-16 HP → 3NT.
    /// Destekli eller (raise / cue) ve 4'lü majörlü eller (negatif kontra / yeni renk) kendi kurallarına kalır.
    /// </summary>
    public class RakipMudahalesiNT : IKonvansiyon
    {
        public string Ad => "Müdahale Sonrası NT Cevap";
        public string Aciklama => "1M - (2 seviyesi müdahale) - dengeli, durdurucu: 2NT (11-12) / 3NT (13-16)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 25;

        public bool UygunMu(IhaleDurumu durum) { return Bul(durum) != null; }
        public string TeklifVer(IhaleDurumu durum) { return Bul(durum) ?? "Pas"; }

        private static string Bul(IhaleDurumu d)
        {
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13) return null;
            if (d.IhaleBittiMi()) return null;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (g.Count != 2) return null;
            if (g[0].Oyuncu != d.Partner || g[0].Seviye != 1 || (g[0].Koz != "Maça" && g[0].Koz != "Kupa")) return null;
            if (!d.Rakipler.Contains(g[1].Oyuncu) || g[1].Seviye != 2 || g[1].Koz == "NT") return null;
            if (g[1].Koz == g[0].Koz) return null;

            var el = d.AktifOyuncuEli;
            if (!ElDegerlendirici.DengeliEl(el)) return null;
            if (!ElDegerlendirici.StopperVar(el, g[1].Koz)) return null;
            if (ElDegerlendirici.RenkUzunlugu(el, g[0].Koz) >= 3) return null;
            // Kullanılmamış majörde 4+ kart → negatif kontra / yeni renk
            foreach (var m in new[] { "Maça", "Kupa" })
                if (m != g[0].Koz && m != g[1].Koz && ElDegerlendirici.RenkUzunlugu(el, m) >= 4) return null;

            int hp = ElDegerlendirici.HCP(el);
            if (hp >= 11 && hp <= 12) return "2NT";
            if (hp >= 13 && hp <= 16) return "3NT";
            return null;
        }
    }
}