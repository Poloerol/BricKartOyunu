using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Takeout Double (Çağrı Kontrası) — rakibin 1 seviyesindeki renk açılışına karşı.
    /// 
    /// KONTRA YAPAN (direkt oturuş, rakip hemen sağımızda 1 seviyesinde renk açtı):
    ///   - 12+ HP
    ///   - Rakibin renginde en fazla 2 kart (kısalık)
    ///   - Diğer her üç renkte en az 3 kart (partner hangisini seçerse destek)
    ///   - 5+ kartlı yan renk varsa ve 17 HP'nin altındaysa kontra yerine doğal
    ///     müdahale tercih edilir (rakip rengi 0-1 kart ise yine kontra)
    ///   Teklif: Dbl
    /// 
    /// PARTNER (kontraya cevap veren — sağındaki rakip PAS dedi, cevap zorunlu):
    ///   - Rakip renginde 5+ kart ve 3+ üst onör (A K Q J 10) → Pas (ceza)
    ///   - 13+ HP, dengeli, rakip rengi durduruyor, 4'lü yan majör yok → 3NT
    ///   - 12+ HP                      → rakibin rengine cue bid (2 seviye, zorlayıcı)
    ///   - 9-11 HP                     → en iyi renkte ATLAYARAK (davet) / dengeliyse 2NT
    ///   - 6-8 HP, dengeli, durdurucu  → 1NT
    ///   - 0-8 HP                      → en iyi yan renk, en ucuz seviyede
    ///   En iyi renk: 4+ kartlı yan majör (uzun olan); yoksa en uzun yan renk.
    /// 
    /// KAPSAM DIŞI (bu sürümde yok): 2 seviye zayıf iki'ye kontra, balancing (geç oturuş)
    /// kontrası, kontra yapanın rebid'i, sağdaki rakip teklif verdiyse cevap.
    /// 
    /// Anlaşma: OrtaklikAnlasmasi.TakeoutDouble
    /// Öncelik: 24 (Michaels 19 ve Unusual 2NT 23'ten sonra — iki renkli eller onlara gider)
    /// </summary>
    public class TakeoutDouble : IKonvansiyon
    {
        public string Ad => "Takeout Double";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 24;

        private const int MinHp = 11; // 5'li majör: 4'lü majör + kisa rakip için 11 yeter
        private const int GucluHp = 17;

        private static readonly string[] Renkler = { "Sinek", "Karo", "Kupa", "Maça" };

        private enum Rol { Kontra, Cevap }

        private class Analiz
        {
            public Rol Rol;
            public string RakipRengi;
        }

        // ═══════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            var analiz = Cozumle(durum);
            if (analiz == null) return false;

            if (analiz.Rol == Rol.Cevap) return true;
            return KontraEliMi(durum.AktifOyuncuEli, analiz.RakipRengi);
        }

        /// <summary>
        /// İhale yapısını çözer:
        ///   [Pas...] rakip 1-renk                 → Kontra durumu (rakip hemen sağımızda)
        ///   [Pas...] rakip 1-renk, Dbl (partner), Pas (diğer rakip) → Cevap durumu
        /// Başka her şey için null.
        /// </summary>
        private static Analiz Cozumle(IhaleDurumu durum)
        {
            var g = durum.Gecmis;
            if (g == null || g.Count == 0) return null;

            // İlk Pas olmayan hamle = açılış
            int i = -1;
            for (int k = 0; k < g.Count; k++)
            {
                if (g[k].Teklif != "Pas") { i = k; break; }
            }
            if (i < 0) return null;

            var acilis = g[i];
            if (!durum.Rakipler.Contains(acilis.Oyuncu)) return null;

            string renk = RenkAdi(acilis.Teklif);
            if (renk == null || acilis.Teklif[0] != '1') return null;   // sadece 1 seviyesi renk açılışı

            int kalan = g.Count - (i + 1);

            // Kontra: geçmişte yalnızca rakip açılışı var (doğrudan oturuş
            // veya testlerin kısalttığı 1♠ - ? sekansı). Balancing (geç oturuş)
            // partner/diğer rakip Pas'larından sonra gelir, burada yok.
            if (kalan == 0)
            {
                return new Analiz { Rol = Rol.Kontra, RakipRengi = renk };
            }

            // Cevap durumu: açılış - partner Dbl - diğer rakip Pas
            if (kalan == 2 &&
                g[i + 1].Oyuncu == durum.Partner && g[i + 1].Teklif == "Dbl" &&
                g[i + 2].Teklif == "Pas" &&
                durum.Rakipler.Contains(g[i + 2].Oyuncu) &&
                g[i + 2].Oyuncu != acilis.Oyuncu)
            {
                return new Analiz { Rol = Rol.Cevap, RakipRengi = renk };
            }

            return null;
        }

        private static bool KontraEliMi(List<Card> el, string rakip)
        {
            int hp = ElDegerlendirici.HCP(el);
            if (hp < MinHp) return false;

            int rakipUzunluk = ElDegerlendirici.RenkUzunlugu(el, rakip);
            if (rakipUzunluk > 2) return false;

            // 5'li majör sistemi:
            //   Rakip 1♥/1♠ açtıysa diğer majörde 4+ kart takeout'un temel sözü.
            //   Overcall 5+ istediği için 4'lü majör → Dbl (1♠ overcall değil).
            bool rakipMajor = (rakip == "Kupa" || rakip == "Maça");
            string digerMajor = rakip == "Kupa" ? "Maça" : (rakip == "Maça" ? "Kupa" : null);

            if (rakipMajor && digerMajor != null)
            {
                int digerMaj = ElDegerlendirici.RenkUzunlugu(el, digerMajor);
                // 5+ diğer majör → doğal 1 seviye overcall (Dbl değil)
                if (digerMaj >= 5) return false;
                // 4'lü diğer majör → Dbl (overcall 5+ istediği için)
                if (digerMaj < 4) return false;
            }

            int zayifYan = 0; // 2 kartlı yan renk sayısı
            bool besliYanRenk = false;
            foreach (var r in Renkler)
            {
                if (r == rakip) continue;
                int u = ElDegerlendirici.RenkUzunlugu(el, r);
                if (u < 2) return false;          // 0-1 kartlı ikinci renk → şekil bozuk
                if (u == 2) zayifYan++;
                if (u >= 5) besliYanRenk = true;
            }

            // Klasik: her yan 3+. 5'li majör esnetmesi: diğer majör 4+ ise
            // bir minörde 2 kart kabul (ör. 4♠-1♥-2♦-5♣).
            if (zayifYan > 1) return false;
            if (zayifYan == 1 && !(rakipMajor && digerMajor != null &&
                ElDegerlendirici.RenkUzunlugu(el, digerMajor) >= 4))
                return false;

            // İyi 5'li renk + orta güç → doğal overcall tercih (kısa 0-1 ise yine Dbl)
            // 5'li majör kuralı: 5'li yan minör olsa bile diğer majör 4+ ise Dbl kalır
            // (2♣ majörü inkar eder). Sadece diğer majör YOKSA doğal overcall'a bırak.
            if (besliYanRenk && hp < GucluHp && rakipUzunluk >= 2)
            {
                if (!(rakipMajor && digerMajor != null &&
                      ElDegerlendirici.RenkUzunlugu(el, digerMajor) >= 4))
                    return false;
            }

            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            if (!UygunMu(durum)) return "Pas";

            var analiz = Cozumle(durum);
            if (analiz.Rol == Rol.Kontra) return "Dbl";

            return CevapTeklifi(durum.AktifOyuncuEli, analiz.RakipRengi);
        }

        private static string CevapTeklifi(List<Card> el, string rakip)
        {
            int hp = ElDegerlendirici.HCP(el);
            string rakipSembol = Sembol(rakip);

            // ─── Ceza pası: rakip renginde 5+ kart ve 3+ üst onör ─────
            var rakipKartlari = el.Where(c => c.Suit == rakip).ToList();
            if (rakipKartlari.Count >= 5 &&
                rakipKartlari.Count(c => c.Value >= 10) >= 3)
                return "Pas";

            bool dengeli = ElDegerlendirici.DengeliEl(el);
            bool durdurucu = ElDegerlendirici.StopperVar(el, rakip);
            bool yanMajor4 =
                (rakip != "Maça" && ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4) ||
                (rakip != "Kupa" && ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4);
            bool ntEli = dengeli && durdurucu && !yanMajor4;

            // ─── 13+ HP ───────────────────────────────────────────────
            if (hp >= 13 && ntEli) return "3NT";

            // ─── 12+ HP: cue bid (zorlayıcı) ──────────────────────────
            if (hp >= 12) return "2" + rakipSembol;

            // ─── En iyi yan renk ve en ucuz seviye ────────────────────
            string en = EnIyiRenk(el, rakip);
            int seviye = Sira(en) > Sira(rakip) ? 1 : 2;

            // ─── 9-11 HP: davet ───────────────────────────────────────
            if (hp >= 9)
                return ntEli ? "2NT" : (seviye + 1) + Sembol(en);

            // ─── 6-8 HP dengeli + durdurucu ───────────────────────────
            if (hp >= 6 && ntEli) return "1NT";

            // ─── 0-8 HP: en ucuz seviyede en iyi renk ─────────────────
            return seviye + Sembol(en);
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════════════════════════════

        /// <summary>4+ kartlı yan majör (uzun olan, eşitse ♠); yoksa en uzun yan renk.</summary>
        private static string EnIyiRenk(List<Card> el, string rakip)
        {
            var adaylar = Renkler.Where(r => r != rakip).ToList();

            var majorler = adaylar
                .Where(r => (r == "Maça" || r == "Kupa") &&
                            ElDegerlendirici.RenkUzunlugu(el, r) >= 4)
                .OrderByDescending(r => ElDegerlendirici.RenkUzunlugu(el, r))
                .ThenByDescending(r => Sira(r))
                .ToList();
            if (majorler.Count > 0) return majorler[0];

            return adaylar
                .OrderByDescending(r => ElDegerlendirici.RenkUzunlugu(el, r))
                .ThenByDescending(r => Sira(r))
                .First();
        }

        /// <summary>Sinek &lt; Karo &lt; Kupa &lt; Maça</summary>
        private static int Sira(string renk)
        {
            return System.Array.IndexOf(Renkler, renk);
        }

        /// <summary>"1♥" → "Kupa". Renk teklifi değilse (1NT, Dbl, Pas) null.</summary>
        private static string RenkAdi(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2 || !char.IsDigit(teklif[0]))
                return null;

            switch (teklif[teklif.Length - 1])
            {
                case '♣': return "Sinek";
                case '♦': return "Karo";
                case '♥': return "Kupa";
                case '♠': return "Maça";
                default: return null;
            }
        }

        private static string Sembol(string renk)
        {
            switch (renk)
            {
                case "Sinek": return "♣";
                case "Karo": return "♦";
                case "Kupa": return "♥";
                case "Maça": return "♠";
                default: return "";
            }
        }
    }
}