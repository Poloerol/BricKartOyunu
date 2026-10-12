using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Exclusion Blackwood (Voidwood)
    /// 
    /// Bir renkte şikanınız varsa, o rengin Asını saymadan kalan kilit
    /// kartları sormanızı sağlar. Sıçramalı teklif "bu renkte void'im var" der.
    /// 
    /// Tetikleyici: Zon üstü sıçramalı teklif
    ///   5♣ = ♣ void
    ///   5♦ = ♦ void
    ///   5♥ = ♥ void
    ///   5♠ = ♠ void
    /// 
    /// Kilit kart = 4 As (şikan hariç) + koz Papazı (toplam 4 kart)
    /// 
    /// Cevap şeması (1430):
    ///   1. kademe = 0 veya 3 kilit kart
    ///   2. kademe = 1 veya 4 kilit kart
    ///   3. kademe = 2 kilit kart, koz Q yok
    ///   4. kademe = 2 kilit kart + koz Q
    /// 
    /// 0314 için 1. ve 2. kademe yer değiştirir.
    /// 
    /// Örnek:
    ///   1♠ - 3♠ - 5♦ = ♠ koz, ♦ void, kalan 4 kilit kartı sor
    /// 
    /// Öncelik: 27 (RKCB 28 ve TrialBid 27 arası)
    /// </summary>
    public class ExclusionBlackwood : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════
        // IKonvansiyon ÜYELERİ
        // ═══════════════════════════════════════════════════════════════

        public string Ad => "Exclusion Blackwood";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 27;

        // ═══════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // ─── DURUM 1: Partner Exclusion yaptı → cevap veriyoruz ────
            if (PartnerExclusionYaptiMi(durum))
                return true;

            // ─── DURUM 2: Biz Exclusion yapacağız ──────────────────────
            // Koz anlaşılmış olmalı
            string koz = durum.AnlasilanKoz();
            if (string.IsNullOrEmpty(koz)) return false;
            if (koz == "NT") return false;   // NT'de Exclusion olmaz

            // Daha önce 5♣/5♦/5♥/5♠ sormadık
            if (durum.KendiTeklifleri.Any(t =>
                t == "5♣" || t == "5♦" || t == "5♥" || t == "5♠"))
                return false;

            // Zaten 4NT veya 4♣ (Gerber) kullanılmadıysa (slam sorusu yoksa)
            if (durum.KendiTeklifleri.Contains("4NT")) return false;
            if (durum.KendiTeklifleri.Contains("4♣")) return false;

            // 4 seviyesinde veya 5 seviyesinde olmalıyız
            int seviye = durum.KacinciSeviye();
            if (seviye < 3 || seviye > 4) return false;

            // En az 3 gerçek teklif olmalı (açılış + cevap + destek)
            var gercelTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();
            if (gercelTeklifler.Count < 3) return false;

            // Koz en az 2 kez teklif edilmiş olmalı
            int kozSayisi = gercelTeklifler.Count(h => h.Koz == koz);
            if (kozSayisi < 2) return false;

            // Şikan rengi var mı? (en az 1 renkte void olmalı)
            string sikanRengi = SikanRenginiBul(durum.AktifOyuncuEli, koz);
            if (string.IsNullOrEmpty(sikanRengi)) return false;

            // Sıçramalı teklif 5 seviyesinde olmalı
            // (koz ♠ ise 4NT'den sonra 5♣/5♦/5♥/5♠)
            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            // ─── Cevap: Partner Exclusion yaptı ────────────────────────
            if (PartnerExclusionYaptiMi(durum))
            {
                string koz = durum.AnlasilanKoz();
                string sikanRengi = PartnerSikanRenginiAl(durum);
                return CevapVer(durum, el, koz, sikanRengi);
            }

            // ─── Soru: Biz Exclusion yapacağız ─────────────────────────
            // Şikan rengini bul ve 5 seviyesinde teklif et
            string kozBiz = durum.AnlasilanKoz();
            string sikan = SikanRenginiBul(el, kozBiz);
            if (string.IsNullOrEmpty(sikan)) return "Pas";

            // 5 seviyesinde sıçramalı teklif
            string sembol = KozYardimcisi.KozSembolu(sikan);
            return $"5{sembol}";
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Partner Exclusion Yaptı mı?
        // ═══════════════════════════════════════════════════════════════

        private static bool PartnerExclusionYaptiMi(IhaleDurumu durum)
        {
            if (durum.Gecmis == null || durum.Gecmis.Count == 0)
                return false;

            // Partnerin son teklifi
            var partnerTeklifleri = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .ToList();

            if (partnerTeklifleri.Count == 0) return false;

            string sonPartnerTeklifi = partnerTeklifleri.Last().Teklif;

            // 5♣/5♦/5♥/5♠ mı?
            if (sonPartnerTeklifi != "5♣" && sonPartnerTeklifi != "5♦" &&
                sonPartnerTeklifi != "5♥" && sonPartnerTeklifi != "5♠")
                return false;

            // Partner hâlâ son gerçek teklifi veren mi?
            var sonGercek = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .LastOrDefault();
            if (sonGercek == null) return false;
            if (sonGercek.Oyuncu != durum.Partner) return false;

            // Bu partnerin İLK Exclusion'ı mı?
            // (Daha önce 5♣/5♦/5♥/5♠ dememiş olmalı)
            int exclusionSayisi = partnerTeklifleri.Count(t =>
    t.Teklif == "5♣" || t.Teklif == "5♦" ||
    t.Teklif == "5♥" || t.Teklif == "5♠");
            if (exclusionSayisi > 1) return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Partner Şikan Rengi
        // ═══════════════════════════════════════════════════════════════

        private static string PartnerSikanRenginiAl(IhaleDurumu durum)
        {
            var partnerTeklif = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .LastOrDefault();

            if (partnerTeklif == null) return null;

            return KozYardimcisi.TekliftenKozCikar(partnerTeklif.Teklif);
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Şikan Rengini Bul
        // ═══════════════════════════════════════════════════════════════

        private static string SikanRenginiBul(List<Card> el, string koz)
        {
            var renkler = new[] { "Sinek", "Karo", "Kupa", "Maça" };

            foreach (var renk in renkler)
            {
                if (renk == koz) continue;   // Koz rengini atla

                int uzunluk = ElDegerlendirici.RenkUzunlugu(el, renk);
                if (uzunluk == 0) return renk;   // Void bulundu
            }

            return null;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Cevap Ver
        // ═══════════════════════════════════════════════════════════════

        private static string CevapVer(
            IhaleDurumu durum, List<Card> el, string koz, string sikanRengi)
        {
            // Şikan rengini hariç tutarak kilit kart say
            int kilitKart = KilitKartSayisiSikanHaric(el, koz, sikanRengi);

            // Koz Q var mı?
            bool kozQVar = ElDegerlendirici.KizVar(el, koz);

            // 1430 mı 0314 mü?
            bool rkcb1430 = durum.Anlasma == null || durum.Anlasma.RKCB1430;

            // Şikan rengine göre cevap kademeleri
            // Kademe 1, 2, 3, 4 → sıçramalı teklifin bir üstünden başlar
            return KademeliCevap(sikanRengi, kilitKart, kozQVar, rkcb1430);
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Kademeli Cevap
        // ═══════════════════════════════════════════════════════════════

        private static string KademeliCevap(
            string sikanRengi, int kilitKart, bool kozQVar, bool rkcb1430)
        {
            // Her şikan rengi için kademe teklifleri
            // 1. kademe = sıçramalı teklifin 1 üstü
            // 2. kademe = 2 üstü
            // 3. kademe = 3 üstü
            // 4. kademe = 4 üstü

            string[] kademeler = KademeleriAl(sikanRengi);
            if (kademeler == null) return "Pas";

            // 1430: 1. kademe = 0/3, 2. kademe = 1/4, 3. kademe = 2 (Q yok), 4. kademe = 2 + Q
            // 0314: 1. kademe = 1/4, 2. kademe = 0/3, 3. kademe = 2 (Q yok), 4. kademe = 2 + Q

            int kademeIndeks;

            if (rkcb1430)
            {
                switch (kilitKart)
                {
                    case 0:
                    case 3:
                        kademeIndeks = 0;
                        break;
                    case 1:
                    case 4:
                        kademeIndeks = 1;
                        break;
                    case 2:
                        kademeIndeks = kozQVar ? 3 : 2;
                        break;
                    default:
                        kademeIndeks = 0;
                        break;
                }
            }
            else // 0314
            {
                switch (kilitKart)
                {
                    case 1:
                    case 4:
                        kademeIndeks = 0;
                        break;
                    case 0:
                    case 3:
                        kademeIndeks = 1;
                        break;
                    case 2:
                        kademeIndeks = kozQVar ? 3 : 2;
                        break;
                    default:
                        kademeIndeks = 0;
                        break;
                }
            }

            if (kademeIndeks < 0 || kademeIndeks >= kademeler.Length)
                return "Pas";

            return kademeler[kademeIndeks];
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Şikan Rengine Göre Kademe Teklifleri
        // ═══════════════════════════════════════════════════════════════

        private static string[] KademeleriAl(string sikanRengi)
        {
            // 1. kademe, 2. kademe, 3. kademe, 4. kademe

            switch (sikanRengi)
            {
                case "Sinek":   // 5♣ → 5♦, 5♥, 5♠, 5NT
                    return new[] { "5♦", "5♥", "5♠", "5NT" };

                case "Karo":    // 5♦ → 5♥, 5♠, 5NT, 6♣
                    return new[] { "5♥", "5♠", "5NT", "6♣" };

                case "Kupa":    // 5♥ → 5♠, 5NT, 6♣, 6♦
                    return new[] { "5♠", "5NT", "6♣", "6♦" };

                case "Maça":    // 5♠ → 5NT, 6♣, 6♦, 6♥
                    return new[] { "5NT", "6♣", "6♦", "6♥" };

                default:
                    return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Kilit Kart Sayısı (Şikan Hariç)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Şikan rengindeki Ası saymadan kilit kart sayısını hesaplar.
        /// Kilit kart = 4 As (şikan hariç) + koz Papazı.
        /// </summary>
        private static int KilitKartSayisiSikanHaric(
            List<Card> el, string koz, string sikanRengi)
        {
            int sayi = 0;

            // As'ları say (şikan rengi hariç)
            foreach (var kart in el)
            {
                if (kart.Value == 14)   // As
                {
                    if (kart.Suit == sikanRengi) continue;   // Şikan rengi Ası sayma
                    sayi++;
                }
            }

            // Koz Papazı'nı say
            if (koz != "NT")
            {
                bool kozPapaziVar = el.Any(c => c.Suit == koz && c.Value == 13);
                if (kozPapaziVar) sayi++;
            }

            return sayi;
        }
    }
}