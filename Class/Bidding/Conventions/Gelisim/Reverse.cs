using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Reverse — Açıcının 2. renginin ilk renginden yüksek olduğu,
    /// 16+ OP gösteren ve 1 tur forcing olan teklif.
    /// 
    /// Örnek:
    ///   1♣ - 1♠ - 2♦ (Reverse, ♣ < ♦)
    ///   1♦ - 1♥ - 2♠ (Reverse, ♦ < ♠)
    /// 
    /// Reverse DEĞİL:
    ///   1♥ - 1♠ - 2♣ (♥ > ♣, normal)
    ///   1♠ - 1NT - 2♥ (Forcing NT sonrası)
    /// 
    /// Koşullar:
    ///   - Açıcının ilk rengi 5+ kart
    ///   - İkinci renk 4+ kart ve daha yüksek
    ///   - 16+ OP
    ///   - 1 tur forcing
    /// </summary>
    public class Reverse : IKonvansiyon
    {
        public string Ad => "Reverse";
        public string Aciklama => "Reverse rebid";
        public bool AktifMi { get; set; } = true;

        // TemelMantik'dan önce, ama diğer konvansiyonlardan sonra
        public int Oncelik => 29;

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

            if (gercekTeklifler.Count < 2) return false;

            // ─── SENARYO C: 1X (ben) - 1Y - 2Z (ben, Reverse) - 2NT (asgari, uyumsuz): 17+ HP ile oyun ───
            if (gercekTeklifler.Count == 4)
            {
                var c0 = gercekTeklifler[0]; var c1 = gercekTeklifler[1]; var c2 = gercekTeklifler[2]; var c3 = gercekTeklifler[3];
                return c0.Oyuncu == durum.AktifOyuncu && c2.Oyuncu == durum.AktifOyuncu
                    && c1.Oyuncu == durum.Partner && c3.Oyuncu == durum.Partner
                    && c0.Seviye == 1 && c0.Koz != "NT" && c1.Seviye == 1 && c1.Koz != "NT"
                    && c2.Seviye == 2 && c2.Koz != "NT" && c2.Koz != c1.Koz && RenkSirasiKucukMu(c0.Koz, c2.Koz)
                    && c3.Teklif == "2NT";
            }

            // ─── SENARYO A: Açıcının Reverse yapması ───
            if (gercekTeklifler.Count == 2)
            {
                var teklif2 = gercekTeklifler[1];
                var teklif1 = gercekTeklifler[0];

                if (teklif1.Oyuncu != durum.AktifOyuncu) return false;
                if (teklif1.Seviye != 1) return false;
                if (teklif1.Koz == "NT") return false;

                if (teklif2.Oyuncu != durum.Partner) return false;
                if (teklif2.Seviye != 1) return false;
                if (teklif2.Koz == "NT") return false;

                if (teklif1.Koz == teklif2.Koz) return false;

                // ★ YENİ: HP kontrolü
                int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
                if (hp < 16) return false;

                if (ReverseMumkunMu(durum.AktifOyuncuEli, teklif1.Koz, teklif2.Koz))
                    return true;

                return false;
            }

            // ─── SENARYO B: Cevapçının Reverse'e cevabı ───
            // Geçmiş: 1X (partner) - 1Y (aktif) - 2Z (partner, Reverse) - ?
            if (gercekTeklifler.Count == 3)
            {
                var teklif3 = gercekTeklifler[2];
                var teklif2 = gercekTeklifler[1];
                var teklif1 = gercekTeklifler[0];

                // 1. teklif partner, 1 seviyesi
                if (teklif1.Oyuncu != durum.Partner) return false;
                if (teklif1.Seviye != 1) return false;

                // 2. teklif aktif oyuncu, 1 seviyesi
                if (teklif2.Oyuncu != durum.AktifOyuncu) return false;
                if (teklif2.Seviye != 1) return false;

                // 3. teklif partner, 2 seviyesi (Reverse)
                if (teklif3.Oyuncu != durum.Partner) return false;
                if (teklif3.Seviye != 2) return false;
                if (teklif3.Koz == "NT") return false;
                if (teklif3.Koz == teklif2.Koz) return false;   // benim rengimin desteği (1♣–1♥–2♥) reverse değildir

                // Reverse mi?
                // teklif1.Koz < teklif3.Koz olmalı (renk sırası)
                if (!RenkSirasiKucukMu(teklif1.Koz, teklif3.Koz)) return false;

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

            var gercekTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            if (gercekTeklifler.Count == 4)
                return ElDegerlendirici.HCP(el) >= 17 ? "3NT" : "Pas";

            // Açıcı mıyız? (Reverse yapacağız)
            if (gercekTeklifler.Count == 2)
            {
                return AciciReverse(durum, el, gercekTeklifler);
            }

            // Cevapçı mıyız? (Reverse'e cevap vereceğiz)
            return CevapciCevabi(durum, el, gercekTeklifler);
        }

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ REVERSE YAPMASI
        // ═══════════════════════════════════════════════════════════════════

        private string AciciReverse(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            var teklif1 = gercekTeklifler[0];
            var teklif2 = gercekTeklifler[1];

            string xRenk = teklif1.Koz;    // Açıcının ilk rengi
            string yRenk = teklif2.Koz;    // Cevapçının rengi

            int hp = ElDegerlendirici.HCP(el);

            // 16+ OP kontrolü
            if (hp < 16) return "Pas";

            // Reverse yapılacak rengi bul
            // xRenk'ten yüksek, yRenk'ten farklı, 4+ kart
            string reverseRenk = null;

            string[] renkSirasi = { "Sinek", "Karo", "Kupa", "Maça" };
            int xIndex = Array.IndexOf(renkSirasi, xRenk);
            if (xIndex < 0) return "Pas";

            // xRenk'ten yüksek renkleri sırayla dene
            for (int i = xIndex + 1; i < renkSirasi.Length; i++)
            {
                string renk = renkSirasi[i];
                if (renk == yRenk) continue;   // Cevapçının rengi olamaz

                int uzunluk = ElDegerlendirici.RenkUzunlugu(el, renk);
                if (uzunluk >= 4)
                {
                    reverseRenk = renk;
                    break;
                }
            }

            if (string.IsNullOrEmpty(reverseRenk))
            {
                // Reverse yapılamıyor, normal cevap
                return "Pas";
            }

            // İlk renk 5+ kart mı?
            int xUzunluk = ElDegerlendirici.RenkUzunlugu(el, xRenk);
            if (xUzunluk < 5) return "Pas";

            string sembol = KozYardimcisi.KozSembolu(reverseRenk);
            return "2" + sembol;
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAPÇININ REVERSE'E CEVABI
        // ═══════════════════════════════════════════════════════════════════

        private string CevapciCevabi(IhaleDurumu durum, List<Card> el,
    List<IhaleHamlesi> gercekTeklifler)
        {
            // Geçmiş: 1X (partner) - 1Y (aktif) - 2Z (partner, Reverse)
            var teklif1 = gercekTeklifler[0];   // Partner'in ilk rengi (X)
            var teklif2 = gercekTeklifler[1];   // Bizim rengimiz (Y)
            var teklif3 = gercekTeklifler[2];   // Partner'in Reverse'i (Z)

            string xRenk = teklif1.Koz;
            string yRenk = teklif2.Koz;
            string zRenk = teklif3.Koz;

            int hp = ElDegerlendirici.HCP(el);

            // Fit kontrolü
            int xUzunluk = ElDegerlendirici.RenkUzunlugu(el, xRenk);
            int yUzunluk = ElDegerlendirici.RenkUzunlugu(el, yRenk);
            int zUzunluk = ElDegerlendirici.RenkUzunlugu(el, zRenk);

            // ★ 1) Partner'in ilk rengi (X) fiti → 3X veya 4X
            if (xUzunluk >= 3)
            {
                string sembol = KozYardimcisi.KozSembolu(xRenk);
                // 3X: minimum (6-9 HP), 4X: game (10+ HP)
                if (hp >= 10)
                    return "4" + sembol;
                return "3" + sembol;
            }

            // ★ 2) Partner'in Reverse rengi (Z) fiti → 4Z veya 3Z
            if (zUzunluk >= 3)
            {
                string sembol = KozYardimcisi.KozSembolu(zRenk);
                if (hp >= 10)
                    return "4" + sembol;
                return "3" + sembol;
            }

            // ★ 3) Fit yok, minimum → 2NT
            if (hp >= 6 && hp <= 9)
                return "2NT";

            // ★ 4) Fit yok, game → 3NT
            if (hp >= 10)
                return "3NT";

            // Fallback: kendi rengini tekrar et
            string ySembol = KozYardimcisi.KozSembolu(yRenk);
            return "2" + ySembol;
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Açıcının reverse yapması mümkün mü?
        /// X (ilk renk) &lt; Z (ikinci renk) olmalı ve Z 4+ kart.
        /// </summary>
        private bool ReverseMumkunMu(List<Card> el, string xRenk, string yRenk)
        {
            string[] renkSirasi = { "Sinek", "Karo", "Kupa", "Maça" };
            int xIndex = Array.IndexOf(renkSirasi, xRenk);
            if (xIndex < 0) return false;

            // xRenk'ten yüksek renk var mı?
            for (int i = xIndex + 1; i < renkSirasi.Length; i++)
            {
                string renk = renkSirasi[i];
                if (renk == yRenk) continue;
                if (ElDegerlendirici.RenkUzunlugu(el, renk) >= 4)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Renk sırası: Sinek &lt; Karo &lt; Kupa &lt; Maça
        /// </summary>
        private bool RenkSirasiKucukMu(string ilk, string ikinci)
        {
            string[] renkSirasi = { "Sinek", "Karo", "Kupa", "Maça" };
            int i1 = Array.IndexOf(renkSirasi, ilk);
            int i2 = Array.IndexOf(renkSirasi, ikinci);
            if (i1 < 0 || i2 < 0) return false;
            return i1 < i2;
        }
    }

    /// <summary>
    /// Cevapçı İkinci Tur — 1x - 1Y - (açıcının minimum yeniden teklifi) - ?
    /// Cevapçı 11+ OP ile Pas yerine davet/oyun teklif eder.
    /// </summary>
    public class CevapciIkinciTur : IKonvansiyon
    {
        public string Ad => "Cevapçı İkinci Tur";
        public string Aciklama => "1x-1Y-minimum yeniden teklif sonrası cevapçı 11+ OP ile davet / oyun teklif eder";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 26;

        public bool UygunMu(IhaleDurumu durum) { return Bul(durum) != null; }
        public string TeklifVer(IhaleDurumu durum) { return Bul(durum) ?? "Pas"; }

        private static string Sem(string koz)
        {
            switch (koz) { case "Maça": return "♠"; case "Kupa": return "♥"; case "Karo": return "♦"; case "Sinek": return "♣"; }
            return null;
        }
        private static bool Major(string k) { return k == "Maça" || k == "Kupa"; }

        private static string Bul(IhaleDurumu d)
        {
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13) return null;
            if (d.IhaleBittiMi()) return null;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (g.Count != 3) return null;
            if (g[0].Oyuncu != d.Partner || g[1].Oyuncu != d.AktifOyuncu || g[2].Oyuncu != d.Partner) return null;
            if (g[0].Seviye != 1 || g[0].Koz == "NT") return null;
            if (g[1].Seviye != 1 || g[1].Koz == "NT" || g[1].Koz == "Sinek") return null;
            if (g[1].Koz == g[0].Koz) return null;

            var el = d.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 11) return null;
            bool dengeli = ElDegerlendirici.DengeliEl(el);
            string x = g[0].Koz, y = g[1].Koz, z = g[2].Koz;
            int zs = g[2].Seviye;
            int yLen = ElDegerlendirici.RenkUzunlugu(el, y);
            // Başka bir rengi 6+ kartsa kendi rengini teklif etmek daha doğru
            foreach (var r in new[] { "Maça", "Kupa", "Karo", "Sinek" })
                if (r != y && ElDegerlendirici.RenkUzunlugu(el, r) >= 6) return null;

            // (a) 1NT
            if (z == "NT" && zs == 1)
            {
                if (hp == 11) return null;
                if (hp == 12) return "2NT";
                if (Major(y) && yLen >= 6) return "4" + Sem(y);
                return "3NT";
            }
            // (b) açıcı kendi rengini tekrarladı
            if (z == x && zs == 2)
            {
                int xLen = ElDegerlendirici.RenkUzunlugu(el, x);
                if (hp >= 14)
                {
                    if (Major(x) && xLen >= 2) return "4" + Sem(x);
                    return "3NT";
                }
                if (hp >= 12)
                {
                    if (Major(x) && xLen >= 3) return "3" + Sem(x);
                    return null;   // 2NT daveti açıcının minimum rebidinden sonra veriler ışığında kârsız
                }
                return null;
            }
            // (c) açıcı benim majörümü yükseltti
            if (z == y && zs == 2 && Major(y))
            {
                if (hp >= 13) return "4" + Sem(y);
                if (hp >= 12) return "3" + Sem(y);
                return null;
            }
            // (d) 1 seviyede yeni majör
            if (zs == 1 && Major(z) && z != y && z != x)
            {
                int zLen = ElDegerlendirici.RenkUzunlugu(el, z);
                if (zLen >= 4)
                {
                    if (hp >= 13) return "4" + Sem(z);
                    if (hp >= 10) return "3" + Sem(z);
                    return null;
                }
                if (dengeli)
                {
                    if (hp >= 13) return "3NT";
                    if (hp >= 11) return "2NT";
                }
                return null;
            }
            return null;
        }
    }

    /// <summary>
    /// Oyun Daveti Kabulü — 1M - 2M - 3M(açıcının daveti) - ?
    /// Cevapçı 2M ile 6-10 OP gösterdi; üst düzeyde (10 OP) daveti kabul edip 4M der.
    /// </summary>
    public class OyunDavetiKabul : IKonvansiyon
    {
        public string Ad => "Oyun Daveti Kabulü";
        public string Aciklama => "1M-2M-3M sonrası cevapçı 10+ OP ile 4M teklif eder";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 26;

        public bool UygunMu(IhaleDurumu durum) { return Bul(durum) != null; }
        public string TeklifVer(IhaleDurumu durum) { return Bul(durum) ?? "Pas"; }

        private static string Bul(IhaleDurumu d)
        {
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13) return null;
            if (d.IhaleBittiMi()) return null;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (g.Count != 3) return null;
            if (g[0].Oyuncu != d.Partner || g[1].Oyuncu != d.AktifOyuncu || g[2].Oyuncu != d.Partner) return null;
            string m = g[0].Koz;
            if (m != "Maça" && m != "Kupa") return null;
            if (g[0].Seviye != 1 || g[1].Seviye != 2 || g[1].Koz != m) return null;
            if (g[2].Seviye != 3 || g[2].Koz != m) return null;
            if (ElDegerlendirici.HCP(d.AktifOyuncuEli) < 10) return null;
            return "4" + (m == "Maça" ? "♠" : "♥");
        }
    }

    /// <summary>
    /// Minör Davet Cevabı — 1m - (Pas) - 3m(limit raise, 10-15 OP) - (Pas) - ?
    /// Açıcı 14+ OP ile daveti kabul eder: dört rengi de tutuk ve dengeli ise 3NT, değilse 5m.
    /// Asgari elle Pas kalır.
    /// </summary>
    public class MinorDavetCevabi : IKonvansiyon
    {
        public string Ad => "Minör Davet Cevabı";
        public string Aciklama => "1m-3m sonrası açıcı 14+ OP ile oyuna gider (3NT veya 5m)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 26;

        public bool UygunMu(IhaleDurumu durum) { return Bul(durum) != null; }
        public string TeklifVer(IhaleDurumu durum) { return Bul(durum) ?? "Pas"; }

        private static string Bul(IhaleDurumu d)
        {
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13) return null;
            if (d.IhaleBittiMi()) return null;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (g.Count != 2) return null;
            if (g[0].Oyuncu != d.AktifOyuncu || g[1].Oyuncu != d.Partner) return null;
            string m = g[0].Koz;
            if (m != "Karo" && m != "Sinek") return null;
            if (g[0].Seviye != 1 || g[1].Seviye != 3 || g[1].Koz != m) return null;
            var el = d.AktifOyuncuEli;
            if (ElDegerlendirici.HCP(el) < 14) return null;
            if (ElDegerlendirici.DengeliEl(el))
            {
                bool hepsiTutuk = true;
                foreach (var r in new[] { "Maça", "Kupa", "Karo", "Sinek" })
                    if (!ElDegerlendirici.StopperVar(el, r)) { hepsiTutuk = false; break; }
                if (hepsiTutuk) return "3NT";
            }
            return "5" + (m == "Karo" ? "♦" : "♣");
        }
    }
}
