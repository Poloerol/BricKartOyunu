using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ AÇIŞINA CEVAPLAR (Kuvvetli 1♣ / Polish Club hibrit).
    /// Tablodaki kurallara tam uyumludur.
    /// </summary>
    public class OneClub_Answer : IKonvansiyon
    {
        public string Ad => "1♣ Cevap Sistemi";
        public string Aciklama => "Kuvvetli 1♣ açışına cevap sistemi";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 5;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            if (durum.SonGercekTeklif != "1♣") return false;
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;

            if (durum.RakipTeklifleri.Any(t => !string.IsNullOrEmpty(t)
                && t != "Pas" && t != "Dbl" && t != "RDbl"))
                return false;

            return true;
        }

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

            // ─── 1) 3NT: 12-14 HCP, 4♣, 3-3-3, 0-3 kontrol (En spesifik olduğu için en üstte) ───
            if (hcp >= 12 && hcp <= 14 && c == 4
                && ElDegerlendirici.Dagilim4333(el, "Sinek")
                && ElDegerlendirici.KontrolSayisi(el) <= 3)
                return "3NT";

            // ─── 2) 2♠ multi-purpose (6+♣ iyi renk davet veya 5/4 minör FG) ───
            if (c >= 6 && ElDegerlendirici.IyiRenk(el, "Sinek") && hcp >= 10)
                return "2♠";
            int karo, sinek;
            // 5/4 minör FG (5+♣4+♦ veya 4+♣5+♦, 12+ HCP)
            if (hcp >= 12 && (c + d) >= 9 && c >= 4 && d >= 4)
                return "2♠";

            // ─── 3) 2♣: FG Röle (12+ HCP dengeli veya 12+ HCP 5+♣) ───
            if (hcp >= 12 && (dengeli || c >= 5))
                return "2♣";

            // ─── 4) 2♥: 5♠ 4+♥, 3-8 HCP (ters Flannery) ───
            if (s >= 5 && h >= 4 && hcp >= 3 && hcp <= 8)
                return "2♥";

            // ─── 5) 2♦: 5/4 minör, 7-9 HCP ───
            if (ElDegerlendirici.IkiMinorDagilimi(el, out karo, out sinek)
                && hcp >= 7 && hcp <= 9)
                return "2♦";

            // ─── 6) 2NT: Natürel 11-12 HCP dengeli, majör yok ───
            if (hcp >= 11 && hcp <= 12 && dengeli && s < 4 && h < 4)
                return "2NT";

            // ─── 7) 1NT: 7-11 HCP, 4'lü majör yok ───
            if (hcp >= 7 && hcp <= 11 && s < 4 && h < 4)
                return "1NT";

            // ─── 8) Majörler (4+ HCP, 4+ kart) ───
            if (hcp >= 4 && s >= 4 && s >= h) return "1♠";
            if (hcp >= 4 && h >= 4) return "1♥";

            // ─── 9) 1♦: İki anlamlı ───
            if (hcp >= 10 && d >= 4) return "1♦";                        // (a) doğal 10+ HCP
            if (hcp >= 5 && hcp <= 9 && d >= 5 && !dengeli) return "1♦"; // (b) dengesiz 5+♦ 5-9 HCP

            // ─── 10) Preemptler (Tabloya tam uyumlu) ───
            if (tehlikeli) // ZONLU (V)
            {
                if (c >= 6 && ElDegerlendirici.IyiRenk(el, "Sinek") && hcp >= 5 && hcp <= 8) return "3♣";
                if (d >= 6 && ElDegerlendirici.IyiRenk(el, "Karo") && hcp >= 5 && hcp <= 8) return "3♦";
                if (h >= 6 && ElDegerlendirici.IyiRenk(el, "Kupa") && hcp >= 5 && hcp <= 8) return "3♥";
                if (s >= 6 && ElDegerlendirici.IyiRenk(el, "Maça") && hcp >= 6 && hcp <= 10) return "3♠";
            }
            else // ZONSUZ (NV)
            {
                // Tablo: NV 6+♣ (Preempt) - sadece 5-8 HCP
                if (c >= 6 && hcp >= 5 && hcp <= 8) return "3♣";
                if (d >= 6 && hcp >= 5 && hcp <= 8) return "3♦";
                if (h >= 6 && hcp >= 5 && hcp <= 8) return "3♥";
                if (s >= 6 && hcp >= 5 && hcp <= 8) return "3♠";
            }

            // ─── 11) 4♣/4♦/4♥/4♠: 7+ kart preempt (İyi renk şartı) ───
            if (c >= 7 && ElDegerlendirici.IyiRenk(el, "Sinek")) return "4♣";
            if (d >= 7 && ElDegerlendirici.IyiRenk(el, "Karo")) return "4♦";
            if (h >= 7 && ElDegerlendirici.IyiRenk(el, "Kupa")) return "4♥";
            if (s >= 7 && ElDegerlendirici.IyiRenk(el, "Maça")) return "4♠";

            // ─── 12) Fallback ───
            return "Pas";
        }
    }
}