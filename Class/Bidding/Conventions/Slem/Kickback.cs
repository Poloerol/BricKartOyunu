using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Kickback (Roman Key Card Blackwood'un alternatif tetikleyicisi)
    /// 
    /// Koz renginin 1 üstünde soru yapılır (4NT yerine):
    ///   ♠ koz → 4NT (klasik RKCB, bu sınıf devre dışı)
    ///   ♥ koz → 4♠ (Kickback)
    ///   ♦ koz → 4♥ (Kickback)
    ///   ♣ koz → 4♦ (Kickback)
    /// 
    /// Kilit kart = 4 As + koz Papazı
    /// Cevap şeması (1430):
    ///   Kickback sorusunun bir üstünden başlar:
    ///   1. kademe = 0/3 kilit kart
    ///   2. kademe = 1/4 kilit kart
    ///   3. kademe = 2 kilit kart, koz Q yok
    ///   4. kademe = 2 kilit kart + koz Q
    /// 
    /// Öncelik: 27
    /// NOT: Koz ♠ iken devre dışı (klasik RKCB 4NT kullanılır).
    /// </summary>
    public class Kickback : IKonvansiyon
    {
        public string Ad => "Kickback";
        public string Aciklama => "Alternatif kilit kart sorusu";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 27;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            string koz = KozBul(durum);
            if (string.IsNullOrEmpty(koz)) return false;
            if (koz == "NT") return false;

            // Koz ♠ ise Kickback devre dışı
            if (koz == "Maça") return false;

            // Kickback teklifini hesapla (kozun 1 üstü)
            string kickbackTeklif = KickbackTeklifi(koz);
            if (string.IsNullOrEmpty(kickbackTeklif)) return false;

            // ─── DURUM 1: Partner Kickback yaptı → cevap veriyoruz ────
            if (PartnerKickbackYaptiMi(durum, kickbackTeklif))
                return true;

            // ─── DURUM 2: Biz Kickback yapacağız ──────────────────────
            // Daha önce Kickback yapılmadı
            if (durum.KendiTeklifleri.Contains(kickbackTeklif)) return false;

            // 4NT veya 4♣ (Gerber) daha önce kullanılmadıysa
            if (durum.KendiTeklifleri.Contains("4NT")) return false;
            if (durum.KendiTeklifleri.Contains("4♣")) return false;

            // 4 seviyesinin altında olmalıyız (3. seviyede koz)
            int seviye = durum.KacinciSeviye();
            if (seviye != 3) return false;

            // En az 3 gerçek teklif olmalı
            var gercelTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();
            if (gercelTeklifler.Count < 3) return false;

            // Koz en az 2 kez teklif edilmiş olmalı
            int kozSayisi = gercelTeklifler.Count(h => h.Koz == koz);
            if (kozSayisi < 2) return false;

            // Son teklif 3. seviyede koz olmalı
            var sonTeklif = gercelTeklifler.Last();
            if (sonTeklif.Seviye != 3) return false;
            if (sonTeklif.Koz != koz) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            string koz = KozBul(durum);
            if (string.IsNullOrEmpty(koz)) return "Pas";

            string kickbackTeklif = KickbackTeklifi(koz);
            if (string.IsNullOrEmpty(kickbackTeklif)) return "Pas";

            // ─── Cevap: Partner Kickback yaptı ────────────────────────
            if (PartnerKickbackYaptiMi(durum, kickbackTeklif))
            {
                return CevapVer(durum, el, koz);
            }

            // ─── Soru: Biz Kickback yapacağız ─────────────────────────
            return kickbackTeklif;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Koz Tespiti
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Anlaşılan kozu bulur. Cevap sırasında durum.AnlasilanKoz() partnerin
        /// Kickback teklifini (4♠ / 4♥ / 4♦) koz sanar; bu yüzden önce her aday
        /// koz için "partner bu kozun Kickback'ini mi sordu" diye bakılır.
        /// Hiçbiri tutmazsa (soru tarafı) AnlasilanKoz() kullanılır.
        /// </summary>
        private static string KozBul(IhaleDurumu durum)
        {
            foreach (var aday in new[] { "Kupa", "Karo", "Sinek" })
            {
                if (PartnerKickbackYaptiMi(durum, KickbackTeklifi(aday)) &&
                    OrtaklikKozuSoyledi(durum, aday))
                    return aday;
            }
            return durum.AnlasilanKoz();
        }

        /// <summary>
        /// Kickback sorusundan ÖNCE ortaklık (ben + partner) bu rengi
        /// en az 2 kez söylemiş mi? (Ör. 1♥ - 3♥ - 4♠ → ♥ iki kez söylendi.)
        /// </summary>
        private static bool OrtaklikKozuSoyledi(IhaleDurumu durum, string koz)
        {
            if (durum.Gecmis == null) return false;

            var bizim = durum.Gecmis
                .Where(h => h.GercekTeklifMi &&
                            (h.Oyuncu == durum.AktifOyuncu || h.Oyuncu == durum.Partner))
                .ToList();
            if (bizim.Count < 3) return false;

            bizim.RemoveAt(bizim.Count - 1);   // son teklif = Kickback sorusunun kendisi
            return bizim.Count(h => h.Koz == koz) >= 2;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Kickback Teklifi (Kozun 1 Üstü)
        // ═══════════════════════════════════════════════════════════════

        private static string KickbackTeklifi(string koz)
        {
            switch (koz)
            {
                case "Kupa": return "4♠";   // ♥ koz → 4♠
                case "Karo": return "4♥";   // ♦ koz → 4♥
                case "Sinek": return "4♦";   // ♣ koz → 4♦
                default: return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Partner Kickback Yaptı mı?
        // ═══════════════════════════════════════════════════════════════

        private static bool PartnerKickbackYaptiMi(
            IhaleDurumu durum, string kickbackTeklif)
        {
            if (durum.Gecmis == null || durum.Gecmis.Count == 0)
                return false;

            var partnerTeklifleri = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .ToList();

            if (partnerTeklifleri.Count == 0) return false;

            string sonPartnerTeklifi = partnerTeklifleri.Last().Teklif;
            if (sonPartnerTeklifi != kickbackTeklif) return false;

            var sonGercek = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .LastOrDefault();
            if (sonGercek == null) return false;
            if (sonGercek.Oyuncu != durum.Partner) return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Cevap Ver
        // ═══════════════════════════════════════════════════════════════

        private static string CevapVer(
            IhaleDurumu durum, List<Card> el, string koz)
        {
            int kilitKart = ElDegerlendirici.KilitKartSayisi(el, koz);
            bool kozQVar = ElDegerlendirici.KizVar(el, koz);

            bool rkcb1430 = durum.Anlasma == null || durum.Anlasma.RKCB1430;

            string[] kademeler = KademeleriAl(koz);
            if (kademeler == null) return "Pas";

            int kademeIndeks;

            if (rkcb1430)
            {
                switch (kilitKart)
                {
                    case 0:
                    case 3: kademeIndeks = 0; break;
                    case 1:
                    case 4: kademeIndeks = 1; break;
                    case 2: kademeIndeks = kozQVar ? 3 : 2; break;
                    default: kademeIndeks = 0; break;
                }
            }
            else // 0314
            {
                switch (kilitKart)
                {
                    case 1:
                    case 4: kademeIndeks = 0; break;
                    case 0:
                    case 3: kademeIndeks = 1; break;
                    case 2: kademeIndeks = kozQVar ? 3 : 2; break;
                    default: kademeIndeks = 0; break;
                }
            }

            if (kademeIndeks < 0 || kademeIndeks >= kademeler.Length)
                return "Pas";

            return kademeler[kademeIndeks];
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Koz Rengine Göre Cevap Kademeleri
        // ═══════════════════════════════════════════════════════════════

        private static string[] KademeleriAl(string koz)
        {
            switch (koz)
            {
                case "Kupa":  // ♥ koz, kickback = 4♠ → cevap 4NT, 5♣, 5♦, 5♥
                    return new[] { "4NT", "5♣", "5♦", "5♥" };

                case "Karo":  // ♦ koz, kickback = 4♥ → cevap 4♠, 4NT, 5♣, 5♦
                    return new[] { "4♠", "4NT", "5♣", "5♦" };

                case "Sinek": // ♣ koz, kickback = 4♦ → cevap 4♥, 4♠, 4NT, 5♣
                    return new[] { "4♥", "4♠", "4NT", "5♣" };

                default:
                    return null;
            }
        }
    }
}