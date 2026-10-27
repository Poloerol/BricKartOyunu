using System;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Jacoby Transfer Konvansiyonu (2 yönlü).
    /// 
    /// Rol 1: Partner 1NT açtı, benim 5+ majörüm var → 2♦/2♥ transfer
    ///   - 2♦ → 5+ Kupa gösterir (partner 2♥ der)
    ///   - 2♥ → 5+ Maça gösterir (partner 2♠ der)
    /// 
    /// Rol 2: Ben 1NT açtım, partner 2♦/2♥ transfer dedi → kabul ederim
    ///   - Partner 2♦ → ben 2♥ derim
    ///   - Partner 2♥ → ben 2♠ derim
    /// 
    /// Öncelik: 10 (Stayman ile aynı seviyede)
    /// 
    /// NOT: Stayman (4'lü majör) ve Transfer (5+ majör) çakışmaz:
    /// - 4'lü majör → Stayman (2♣)
    /// - 5+ majör → Transfer (2♦/2♥)
    /// </summary>
    public class JacobyTransfer : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "Jacoby Transfer";
        public string Aciklama => "NT açılışına majör transferi";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 15;   // Stayman (10) sonra, BasitCevap (100) önce

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            var el = durum.AktifOyuncuEli;

            // ── ROL 1: Partner 1NT açtı, ben transfer sorusu soracağım ──
            if (durum.PartnerTeklifleri.LastOrDefault() == "1NT"
                && GercekTeklifSayisi(durum) == 0   // önceki Pas'lar sayılmaz
                && !durum.RakipActiMi())
            {
                // 5+ majör var mı?
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                // 5+ Maça veya 5+ Kupa
                // Ama 4'lü majörü Stayman ele alır → 5+ olmalı
                if (macca >= 5 || kupa >= 5)
                {
                    // En az 6 HP gerekli (zayıf el bile transfer edebilir)
                    if (ElDegerlendirici.HCP(el) >= 6)
                    {
                        return true;
                    }
                }
            }

            // ── ROL 2: Ben 1NT açtım, partner 2♦/2♥ transfer dedi ──
            if (durum.KendiTeklifleri.LastOrDefault() == "1NT")
            {
                string partnerSon = durum.PartnerTeklifleri.LastOrDefault();
                if (partnerSon == "2♦" || partnerSon == "2♥")
                {
                    return true;
                }
            }

            // ── ROL 3: Transfer yaptım, ortak tamamladı → 5'li majör + oyun gücü = 3NT ──
            if (Rol3Teklifi(durum) != null) return true;

            return false;
        }

        private static int GercekTeklifSayisi(IhaleDurumu d)
        {
            return d.KendiTeklifleri.Count(t => t != "Pas");
        }

        /// <summary>
        /// 1NT – 2♥ – 2♠ (ya da 1NT – 2♦ – 2♥) sonrası: tam 5 kartlık majör, 10-14 HP ve
        /// tekil/void yok → 3NT (açıcı 3'lü destekle 4M'ye düzeltebilir). Aksi halde null.
        /// </summary>
        private static string Rol3Teklifi(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null || el.Count != 13) return null;

            var kendi = durum.KendiTeklifleri.Where(t => t != "Pas").ToList();
            var ortak = durum.PartnerTeklifleri.Where(t => t != "Pas").ToList();
            if (kendi.Count != 1 || ortak.Count != 2 || ortak[0] != "1NT") return null;
            if (durum.RakipActiMi()) return null;

            string major;
            if (kendi[0] == "2♥" && ortak[1] == "2♠") major = "Maça";
            else if (kendi[0] == "2♦" && ortak[1] == "2♥") major = "Kupa";
            else return null;

            if (ElDegerlendirici.RenkUzunlugu(el, major) != 5) return null;
            int hp = ElDegerlendirici.HCP(el);
            if (hp < 10 || hp > 14) return null;
            foreach (var r in new[] { "Maça", "Kupa", "Karo", "Sinek" })
                if (ElDegerlendirici.RenkUzunlugu(el, r) <= 1) return null;   // tekil/void → 4M
            return "3NT";
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            // ── ROL 1: Transfer sorusu ──
            if (durum.PartnerTeklifleri.LastOrDefault() == "1NT"
                && GercekTeklifSayisi(durum) == 0)
            {
                var el = durum.AktifOyuncuEli;
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                // 5+ Maça → 2♥ (Maça transferi)
                // 5+ Kupa → 2♦ (Kupa transferi)
                // İkisi de varsa → uzun olanı seç
                if (macca >= 5 && kupa >= 5)
                {
                    if (macca >= kupa) return "2♥";   // Maça transferi
                    return "2♦";                        // Kupa transferi
                }

                if (macca >= 5) return "2♥";
                if (kupa >= 5) return "2♦";
            }

            // ── ROL 3: transfer tamamlandı, 5'li majör + oyun gücü ──
            var r3 = Rol3Teklifi(durum);
            if (r3 != null) return r3;

            // ── ROL 2: Transfer kabulü ──
            if (durum.KendiTeklifleri.LastOrDefault() == "1NT")
            {
                string partnerSon = durum.PartnerTeklifleri.LastOrDefault();

                if (partnerSon == "2♦") return "2♥";   // Kupa transferi kabul
                if (partnerSon == "2♥") return "2♠";   // Maça transferi kabul
            }

            return null;
        }
    }
}