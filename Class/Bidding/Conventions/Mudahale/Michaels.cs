using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Michaels Cue Bid — rakip 1 seviye açılışına cue bid ile iki renk.
    ///
    /// Cue bid (müdahale):
    ///   Rakip 1♠ → 2♠ = 5+ ♥ + 5+ minör
    ///   Rakip 1♥ → 2♥ = 5+ ♠ + 5+ minör
    ///   Rakip 1♦/1♣ → 2♦/2♣ = 5+ ♠ + 5+ ♥
    ///
    /// Partner cevabı (advancer) — ÖNEMLİ:
    ///   Cue bid RAKİP rengidir, doğal yükseltme YAPILMAZ.
    ///   1♥ - 2♥(M) - ?  → 2♠/3♠ (maça tercihi), 2NT (minör sor), Pas; asla 3♥ değil.
    ///   1♠ - 2♠(M) - ?  → 3♥/4♥ (kupa), 2NT (minör sor), Pas; asla 3♠ değil.
    ///
    /// Öncelik: 19
    /// </summary>
    public class Michaels : IKonvansiyon
    {
        public string Ad => "Michaels";
        public string Aciklama => "İki renkli Michaels cue-bid";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 19;

        private enum Rol { CueBid, Cevap }

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // (Genel 5-5 kısayolu kaldırıldı: yalnızca gerçek cue-bid / cevap durumları)

            // Eğer özel CueBid veya Cevap koşulları varsa onları da çağırarak kontrol et
            if (CueBidUygunMu(durum)) return true;
            if (CevapUygunMu(durum)) return true;

            return false;
        }


        public string TeklifVer(IhaleDurumu durum)
        {
            if (!UygunMu(durum))
                return "Pas"; // uygun değilse motor yanlış teklif üretmesin

            if (CueBidUygunMu(durum))
                return CueBidTeklifi(durum);

            if (CevapUygunMu(durum))
                return CevapTeklifi(durum);

            return "Pas";
        }


        // ═══════════════════════════════════════════════════════════════
        // CUE BID (müdahale eden)
        // ═══════════════════════════════════════════════════════════════

        private static bool CueBidUygunMu(IhaleDurumu durum)
        {
            if (durum.RakipTeklifleri.Count == 0) return false;
            string rakipSon = durum.RakipTeklifleri.Last();
            if (rakipSon != "1♠" && rakipSon != "1♥" &&
                rakipSon != "1♦" && rakipSon != "1♣") return false;

            if (durum.KendiGercekTeklifSayisi > 0) return false;

            // Partner henüz gerçek teklif vermemiş olmalı (direkt oturuş)
            if (durum.PartnerTeklifleri.Any(t => t != "Pas" && t != "Dbl" && t != "Rdbl"))
                return false;

            string rakipKoz = KozYardimcisi.TekliftenKozCikar(rakipSon);
            if (string.IsNullOrEmpty(rakipKoz)) return false;

            var el = durum.AktifOyuncuEli;
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            if (rakipKoz == "Maça")
                return kupa >= 5 && (karo >= 5 || sinek >= 5);
            if (rakipKoz == "Kupa")
                return maca >= 5 && (karo >= 5 || sinek >= 5);
            if (rakipKoz == "Karo" || rakipKoz == "Sinek")
                return maca >= 5 && kupa >= 5;

            return false;
        }

        private static string CueBidTeklifi(IhaleDurumu durum)
        {
            string rakipSon = durum.RakipTeklifleri.Last();
            string rakipKoz = KozYardimcisi.TekliftenKozCikar(rakipSon);
            return "2" + KozYardimcisi.KozSembolu(rakipKoz);
        }

        // ═══════════════════════════════════════════════════════════════
        // CEVAP (partner Michaels yaptı)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Partner, rakibin 1X açılışına 2X cue bid yaptı mı?
        /// Örnek: ... Rakip 1♥ - Partner 2♥  veya  Rakip 1♥ - (Pas) - Partner 2♥
        /// </summary>
        private static bool CevapUygunMu(IhaleDurumu durum)
        {
            if (durum.PartnerTeklifleri.Count == 0) return false;
            if (durum.KendiGercekTeklifSayisi > 0) return false; // henüz konuşmadık

            // Partner'ın son gerçek teklifi 2 seviye cue bid olmalı
            string partnerSon = durum.PartnerTeklifleri.LastOrDefault(t =>
                t != "Pas" && t != "Dbl" && t != "Rdbl" && t != "RDbl");
            if (string.IsNullOrEmpty(partnerSon)) return false;
            if (partnerSon.Length < 2 || partnerSon[0] != '2') return false;

            string partnerKoz = KozYardimcisi.TekliftenKozCikar(partnerSon);
            if (string.IsNullOrEmpty(partnerKoz) || partnerKoz == "NT") return false;

            // Rakip 1 seviyesinde AYNI rengi açmış olmalı (cue bid tanımı)
            // (rakibin 1 seviyesindeki herhangi bir rengi olabilir: 1♣ – 1♥ – 2♥ gibi)
            var rakipAcilis = durum.Gecmis
                .Where(h => h.GercekTeklifMi && durum.Rakipler.Contains(h.Oyuncu)
                            && h.Teklif != null && h.Teklif.StartsWith("1")
                            && KozYardimcisi.TekliftenKozCikar(h.Teklif) == partnerKoz)
                .Select(h => h.Teklif)
                .LastOrDefault();
            if (string.IsNullOrEmpty(rakipAcilis)) return false;

            string rakipKoz = KozYardimcisi.TekliftenKozCikar(rakipAcilis);
            if (rakipKoz != partnerKoz) return false; // aynı renk cue bid

            // Partner gerçekten 2{rakipKoz} demiş
            if (partnerSon != "2" + KozYardimcisi.KozSembolu(rakipKoz))
                return false;

            return true;
        }

        private static string CevapTeklifi(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);

            string partnerSon = durum.PartnerTeklifleri.Last(t =>
                t != "Pas" && t != "Dbl" && t != "Rdbl" && t != "RDbl");
            string cueKoz = KozYardimcisi.TekliftenKozCikar(partnerSon); // rakibin rengi

            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // ─── Rakip 1♥ / cue 2♥ → partner 5♠ + 5 minör ───────────────
            if (cueKoz == "Kupa")
            {
                // ASLA kupa yükseltme (rakip rengi / Güney'in açılışı)
                // Maça tercihi (partner 5♠ vaat etti)
                if (maca >= 3)
                {
                    if (hp >= 10 && maca >= 4) return "3♠";
                    return "2♠";
                }
                // 8+ HP, maça yok → 2NT (partner minörünü söylesin)
                if (hp >= 8) return "2NT";
                // Zayıf + maça fit yok → ASLA Pas (2♥ rakip rengi, oynanacak kontrat değil):
                // uzun minörü söyle, partner düzeltebilir
                return karo >= sinek ? "3♦" : "3♣";
            }

            // ─── Rakip 1♠ / cue 2♠ → partner 5♥ + 5 minör ───────────────
            if (cueKoz == "Maça")
            {
                // ASLA maça yükseltme
                if (kupa >= 3)
                {
                    if (hp >= 10 && kupa >= 4) return "4♥";
                    return "3♥"; // 2♥ cue sonrası kupa 3 seviyesinde
                }
                if (hp >= 8) return "2NT";
                // ASLA Pas (2♠ rakip rengi): uzun minörü söyle
                return karo >= sinek ? "3♦" : "3♣";
            }

            // ─── Rakip minör / cue 2♣ veya 2♦ → partner her iki majör ───
            if (cueKoz == "Sinek" || cueKoz == "Karo")
            {
                // Daha uzun majörü tercih et
                if (maca >= kupa && maca >= 3)
                {
                    if (hp >= 10 && maca >= 4) return "3♠";
                    return "2♠";
                }
                if (kupa >= 3)
                {
                    if (hp >= 10 && kupa >= 4) return "3♥";
                    return "2♥";
                }
                // Partner iki majörü de gösterdi (5+5): her durumda en az 7 kartlık majör uyumu var.
                // ASLA Pas (2♣/2♦ rakip rengi): uzun majörü söyle
                return maca >= kupa ? "2♠" : "2♥";
            }

            return "Pas";
        }
    }
}

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Michaels yapanın, ortağın tercihinden sonraki devamı.
    ///   Ortağın tercihi (2M, ya da 1♠ açılışında 3M) 0-9 HP'lik zayıf bir tercihtir:
    ///     17+ HP → 4M | 15-16 HP → 3M davet (1♠ açılışında 4M) | aksi halde PAS.
    ///   Ortağın daveti (tercihten bir seviye yukarısı, 10+ HP ve 4+ uyum): 13+ HP → 4M, aksi halde PAS.
    /// Öncelik: 18
    /// </summary>
    public class MichaelsDevam : IKonvansiyon, IBilincliPas
    {
        public string Ad => "Michaels Devam";
        public string Aciklama => "Michaels yapanın ortağın tercih/davetinden sonraki devamı";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 18;

        public bool UygunMu(IhaleDurumu durum) { return Bul(durum) != null; }
        public string TeklifVer(IhaleDurumu durum) { return Bul(durum) ?? "Pas"; }

        private static int Sira(char c) { return "♣♦♥♠".IndexOf(c); }

        private static string Bul(IhaleDurumu d)
        {
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13) return null;
            if (d.IhaleBittiMi()) return null;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            var benim = g.Where(h => h.Oyuncu == d.AktifOyuncu).ToList();
            var ortak = g.Where(h => h.Oyuncu == IhaleDurumu.PartnerBul(d.AktifOyuncu)).ToList();
            if (benim.Count != 1 || ortak.Count != 1) return null;
            if (g.Count != 3 || !d.Rakipler.Contains(g[0].Oyuncu)) return null;
            if (!ReferenceEquals(g[1], benim[0]) || !ReferenceEquals(g[2], ortak[0])) return null;
            // Ortağın teklifinden hemen sonra rakip Pas demiş olmalı
            int n = d.Gecmis.Count;
            if (n < 2 || !ReferenceEquals(d.Gecmis[n - 2], g[2]) || d.Gecmis[n - 1].Teklif != "Pas") return null;

            string acilis = g[0].Teklif, cue = g[1].Teklif, tercih = g[2].Teklif;
            if (acilis.Length != 2 || acilis[0] != '1' || cue.Length != 2 || cue[0] != '2' || cue[1] != acilis[1]) return null;
            if (tercih.Length != 2 || !char.IsDigit(tercih[0]) || Sira(tercih[1]) < 0 || tercih[1] == cue[1]) return null;
            char M = tercih[1];
            if (M != '♥' && M != '♠') return null;
            int seviye = tercih[0] - '0';
            int min = Sira(M) > Sira(cue[1]) ? 2 : 3;
            string sz = M == '♠' ? "Maça" : "Kupa";
            if (ElDegerlendirici.RenkUzunlugu(d.AktifOyuncuEli, sz) < 5) return null;
            int hp = ElDegerlendirici.HCP(d.AktifOyuncuEli);
            string oyun = "4" + M;

            if (seviye == min)                                   // tercih
            {
                if (hp >= 17) return oyun;
                if (hp >= 15) return min == 2 ? "3" + M : oyun;
                return "Pas";
            }
            if (seviye == min + 1 && seviye < 4)                 // davet
                return hp >= 13 ? oyun : "Pas";
            return null;
        }
    }
}

