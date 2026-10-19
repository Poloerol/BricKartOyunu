using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 1♣ AÇIŞINA CEVAPLAR (Kuvvetli 1♣ / Polish Club hibrit).
    ///
    /// Bu konvansiyon 1♣ açıldıktan SONRA devreye girer.
    /// Zon bağımlı preemptler içerir.
    ///
    /// Öncelik: 5 (BasitCevap'tan önce, diğer açılış konvansiyonlarından sonra)
    /// </summary>
    public class BirClubCevap : IKonvansiyon
    {
        public string Ad => "1♣ Cevap Sistemi";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 5;

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // Son gerçek teklif 1♣ mü?
            if (durum.SonGercekTeklif != "1♣") return false;

            // 1♣'ü PARTNER mi açtı? (rakip değil)
            // SonGercekTeklifSahibi partner olmalı
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;

            // Rakip araya girmediyse (bu ilk cevap)
            // Rakip gerçek teklif verdiyse (kontr değil) bu konvansiyon devre dışı
            if (durum.RakipTeklifleri.Any(t => !string.IsNullOrEmpty(t)
                && t != "Pas" && t != "Dbl" && t != "RDbl"))
                return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME — Öncelik sırasıyla kontrol et
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            int hcp = ElDegerlendirici.HCP(el);
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            bool dengeli = ElDegerlendirici.DengeliEl(el);
            bool tehlikeli = ElDegerlendirici.ZondaMi(
                durum.ZonNS, durum.ZonEW, durum.AktifOyuncu);

            // ─── 1) FG RÖLE: 2♣ = 12+ HCP dengeli VEYA 12+ HCP 5+♣ ───
            if (hcp >= 12 && (dengeli || c >= 5))
                return "2♣";

            // ─── 2) 3NT: 12-14 HCP, 4♣, 3-3-3, 0-3 kontrol ───
            if (hcp >= 12 && hcp <= 14 && c == 4
                && ElDegerlendirici.Dagilim4333(el, "Sinek")
                && ElDegerlendirici.KontrolSayisi(el) <= 3)
                return "3NT";

            // ─── 3) 2NT: 11-12 HCP dengeli, majör yok ───
            if (hcp >= 11 && hcp <= 12 && dengeli && s < 4 && h < 4)
                return "2NT";

            // ─── 4) 2♠ multi-purpose ───
            // (a) 6+♣ iyi renk davet
            if (c >= 6 && ElDegerlendirici.IyiRenk(el, "Sinek") && hcp >= 10)
                return "2♠";
            // (b) 5/4 minör FG
            int karo, sinek;
            if (ElDegerlendirici.IkiMinorDagilimi(el, out karo, out sinek) && hcp >= 12)
                return "2♠";

            // ─── 5) 2♥: 5♠ 4+♥, 3-8 HCP (ters Flannery) ───
            if (s >= 5 && h >= 4 && hcp >= 3 && hcp <= 8)
                return "2♥";

            // ─── 6) 2♦: 5/4 minör, 7-9 HCP ───
            if (ElDegerlendirici.IkiMinorDagilimi(el, out karo, out sinek)
                && hcp >= 7 && hcp <= 9)
                return "2♦";

            // ─── 7) 1NT: 7-11 HCP, majör yok ───
            if (hcp >= 7 && hcp <= 11 && s < 4 && h < 4)
                return "1NT";

            // ─── 8) Majörler (4+ HCP, 4+ kart) — önce uzun olan ───
            if (hcp >= 4 && s >= 4 && s >= h) return "1♠";
            if (hcp >= 4 && h >= 4) return "1♥";

            // ─── 9) 1♦: iki anlamlı ───
            if (hcp >= 10 && d >= 4) return "1♦";                        // (a) doğal
            if (hcp >= 5 && hcp <= 9 && d >= 5 && !dengeli) return "1♦"; // (b) dengesiz

            // ─── 10) Preemptler (zon bağımlı) ───
            if (tehlikeli)
            {
                if (c >= 6 && ElDegerlendirici.IyiRenk(el, "Sinek") && hcp >= 5 && hcp <= 8)
                    return "3♣";
                if (d >= 6 && ElDegerlendirici.IyiRenk(el, "Karo") && hcp >= 5 && hcp <= 8)
                    return "3♦";
                if (h >= 6 && ElDegerlendirici.IyiRenk(el, "Kupa") && hcp >= 5 && hcp <= 8)
                    return "3♥";
                if (s >= 6 && ElDegerlendirici.IyiRenk(el, "Maça") && hcp >= 6 && hcp <= 10)
                    return "3♠";
            }
            else
            {
                if (c >= 6 && hcp >= 5 && hcp <= 8) return "3♣";
                if (d >= 6 && hcp >= 5 && hcp <= 8) return "3♦";
                if (h >= 6 && hcp >= 5 && hcp <= 8) return "3♥";
                if (s >= 6 && hcp >= 5 && hcp <= 8) return "3♠";
            }

            // ─── 11) 4♣/4♦/4♥/4♠: 7+ kart preempt ───
            if (c >= 7) return "4♣";
            if (d >= 7) return "4♦";
            if (h >= 7) return "4♥";
            if (s >= 7) return "4♠";

            // ─── 12) Fallback ───
            return "Pas";
        }
    }
}