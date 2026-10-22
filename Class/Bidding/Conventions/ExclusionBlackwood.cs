using System;
using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Exclusion Blackwood (Voidwood)
    /// 
    /// Bir renkte şikanınız varsa, o rengin Asını saymadan kalan kilit
    /// kartları sormanızı sağlar. Sıçramalı 5 seviyesi teklif "bu renkte void'im var" der.
    ///   5♣ = ♣ void, 5♦ = ♦ void, 5♥ = ♥ void, 5♠ = ♠ void
    /// 
    /// Kilit kart = 3 As (şikan hariç) + koz Papazı (toplam 4 kart)
    /// 
    /// Cevap şeması (1430):
    ///   1. kademe = 0 veya 3, 2. kademe = 1 veya 4,
    ///   3. kademe = 2 (koz Q yok), 4. kademe = 2 + koz Q
    /// (0314'te 1. ve 2. kademe yer değiştirir.)
    /// 
    /// GÜVENLİK ŞARTLARI (yanlış tetiklenmeyi önler):
    /// - Anlaşılan koz MAJÖR olmalı (Maça/Kupa)
    /// - Ortaklığın ilk gerçek teklifi NT olmamalı (NT sekansında Exclusion yok)
    /// - Ortaklık 4♣ (Gerber/cue) veya 4NT (Blackwood/RKCB) kullanmamış olmalı
    /// - Şikan rengi koz olamaz
    /// - Soran el en az 15 HP olmalı
    /// 
    /// Üç rol:
    ///   1) SORU     : şikanım var → 5 seviyesinde şikan rengini teklif et
    ///   2) CEVAP    : ortak sordu → kademeli cevap ver
    ///   3) TAKİP    : ben sordum, ortak cevapladı → slam/oyun kararı ver
    /// 
    /// Öncelik: 27
    /// </summary>
    public class ExclusionBlackwood : IKonvansiyon
    {
        public string Ad => "Exclusion Blackwood";
        public string Aciklama => "Şikan üzerinden kilit kart sorusu";
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

            string koz;
            if (!ExclusionOrtami(durum, out koz)) return false;

            // ROL 2: Ortak Exclusion sordu → cevap
            if (PartnerExclusionYaptiMi(durum, koz)) return true;

            // ROL 3: Ben sordum, ortak cevapladı → takip kararı
            if (TakipDurumu(durum, koz, out _, out _)) return true;

            // ROL 1: Ben soracağım
            if (durum.KendiTeklifleri.Any(EsBesSeviyeRenk)) return false;   // zaten sordum

            int seviye = durum.KacinciSeviye();
            if (seviye < 3 || seviye > 4) return false;

            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (gercek.Count < 3) return false;

            // Koz en az 2 kez teklif edilmiş olmalı
            if (gercek.Count(h => h.Koz == koz) < 2) return false;

            // Slam ilgisi: en az 15 HP
            if (ElDegerlendirici.HCP(durum.AktifOyuncuEli) < 15) return false;

            // Şikan rengi var mı?
            return !string.IsNullOrEmpty(SikanRenginiBul(durum.AktifOyuncuEli, koz));
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            string koz;
            if (!ExclusionOrtami(durum, out koz)) return "Pas";

            // ROL 2: Cevap
            if (PartnerExclusionYaptiMi(durum, koz))
            {
                string sikanRengi = PartnerSikanRenginiAl(durum);
                return CevapVer(durum, el, koz, sikanRengi);
            }

            // ROL 3: Takip
            string sikanTakip, cevap;
            if (TakipDurumu(durum, koz, out sikanTakip, out cevap))
                return TakipKarari(durum, el, koz, sikanTakip, cevap);

            // ROL 1: Soru
            string sikan = SikanRenginiBul(el, koz);
            if (string.IsNullOrEmpty(sikan)) return "Pas";
            return $"5{KozYardimcisi.KozSembolu(sikan)}";
        }

        // ═══════════════════════════════════════════════════════════════
        // ORTAM KONTROLÜ
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Exclusion Blackwood'un geçerli olabileceği bir ihale mi?
        /// (Majör koz anlaşılmış, NT/Gerber/Blackwood sekansı değil.)
        /// </summary>
        private static bool ExclusionOrtami(IhaleDurumu durum, out string koz)
        {
            koz = durum.AnlasilanKoz();
            if (koz != "Maça" && koz != "Kupa") return false;

            // Benim ve ortağımın gerçek teklifleri (sırayla)
            var bizim = durum.Gecmis
                .Where(h => h.GercekTeklifMi &&
                            (h.Oyuncu == durum.AktifOyuncu || h.Oyuncu == durum.Partner))
                .ToList();
            if (bizim.Count == 0) return false;

            // NT ile başlayan sekansta Exclusion yok
            // (AnlasilanKoz, Stayman 2♣ / Gerber 4♣ gibi yapay teklifleri "koz" sanabiliyor)
            if (bizim[0].Koz == "NT") return false;

            // Gerber/cue (4♣) veya Blackwood/RKCB (4NT) kullanılmışsa Exclusion yok
            if (bizim.Any(h => h.Teklif == "4♣" || h.Teklif == "4NT")) return false;

            return true;
        }

        private static bool EsBesSeviyeRenk(string t)
        {
            return t == "5♣" || t == "5♦" || t == "5♥" || t == "5♠";
        }

        // ═══════════════════════════════════════════════════════════════
        // ROL 2: Partner Exclusion Yaptı mı?
        // ═══════════════════════════════════════════════════════════════

        private static bool PartnerExclusionYaptiMi(IhaleDurumu durum, string koz)
        {
            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (gercek.Count < 2) return false;

            var son = gercek[gercek.Count - 1];
            if (son.Oyuncu != durum.Partner) return false;
            if (!EsBesSeviyeRenk(son.Teklif)) return false;

            // Şikan rengi koz olamaz
            if (son.Koz == koz) return false;

            // Sıçrama olmalı: öncesindeki gerçek teklif en fazla 4 seviyesinde
            var onceki = gercek[gercek.Count - 2];
            if (onceki.Seviye > 4) return false;

            // Partner birden fazla 5-seviye renk dediyse ilki soru, sonrası cevap/takip
            if (gercek.Count(h => h.Oyuncu == durum.Partner && EsBesSeviyeRenk(h.Teklif)) > 1)
                return false;

            // Ben zaten sorduysam bu bir cevap olabilir, soru değil
            if (durum.KendiTeklifleri.Any(EsBesSeviyeRenk)) return false;

            return true;
        }

        private static string PartnerSikanRenginiAl(IhaleDurumu durum)
        {
            var partnerTeklif = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .LastOrDefault();
            if (partnerTeklif == null) return null;
            return KozYardimcisi.TekliftenKozCikar(partnerTeklif.Teklif);
        }

        // ═══════════════════════════════════════════════════════════════
        // ROL 3: Takip (ben sordum, ortak cevapladı)
        // ═══════════════════════════════════════════════════════════════

        private static bool TakipDurumu(
            IhaleDurumu durum, string koz, out string sikan, out string cevap)
        {
            sikan = null;
            cevap = null;

            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (gercek.Count < 2) return false;

            int iSon = gercek.Count - 1;
            if (gercek[iSon].Oyuncu != durum.Partner) return false;

            // Benim sorum, ortağın cevabından hemen önce olmalı (araya rakip teklif girmemiş)
            var soru = gercek[iSon - 1];
            if (soru.Oyuncu != durum.AktifOyuncu) return false;
            if (!EsBesSeviyeRenk(soru.Teklif)) return false;

            string sikanRengi = soru.Koz;
            if (string.IsNullOrEmpty(sikanRengi) || sikanRengi == koz) return false;

            // Gerçekten o rengte void'im olmalı (sorum Exclusion idi)
            if (ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, sikanRengi) != 0) return false;

            string[] kademeler = KademeleriAl(sikanRengi);
            if (kademeler == null) return false;
            if (Array.IndexOf(kademeler, gercek[iSon].Teklif) < 0) return false;

            sikan = sikanRengi;
            cevap = gercek[iSon].Teklif;
            return true;
        }

        /// <summary>
        /// Ortağın cevabından sonra karar: toplam kilit kart ≥ 3 → 6 koz, değilse 5 koz.
        /// Belirsiz kademelerde (0/3, 1/4) toplam 4'ü aşmayan EN KÜÇÜK değer alınır.
        /// </summary>
        private static string TakipKarari(
            IhaleDurumu durum, List<Card> el, string koz, string sikan, string cevap)
        {
            string[] kademeler = KademeleriAl(sikan);
            int adim = Array.IndexOf(kademeler, cevap);
            bool r1430 = durum.Anlasma == null || durum.Anlasma.RKCB1430;

            int[] adaylar;
            if (adim >= 2) adaylar = new[] { 2 };
            else if (r1430) adaylar = adim == 0 ? new[] { 0, 3 } : new[] { 1, 4 };
            else adaylar = adim == 0 ? new[] { 1, 4 } : new[] { 0, 3 };

            int benim = KilitKartSayisiSikanHaric(el, koz, sikan);
            int ortak = adaylar.Where(v => benim + v <= 4).DefaultIfEmpty(adaylar.Min()).Min();
            int toplam = benim + ortak;

            int hedef = toplam >= 3 ? 6 : 5;
            string sembol = KozYardimcisi.KozSembolu(koz);

            var son = durum.Gecmis.Where(h => h.GercekTeklifMi).Last();
            int sonRank = TeklifRank(son.Seviye, son.Koz);

            // Hedef seviye geçerli değilse (ör. cevap 5NT oldu) bir üst seviyeyi dene
            for (int s = hedef; s <= 6; s++)
            {
                if (TeklifRank(s, koz) > sonRank) return s + sembol;
            }
            return "Pas";   // cevap zaten kozun kendisi (ör. 6♥) → oynayalım
        }

        private static int TeklifRank(int seviye, string koz)
        {
            string[] sira = { "Sinek", "Karo", "Kupa", "Maça", "NT" };
            int r = Array.IndexOf(sira, koz);
            if (r < 0) r = 0;
            return seviye * 5 + r;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Şikan Rengini Bul
        // ═══════════════════════════════════════════════════════════════

        private static string SikanRenginiBul(List<Card> el, string koz)
        {
            var renkler = new[] { "Sinek", "Karo", "Kupa", "Maça" };

            foreach (var renk in renkler)
            {
                if (renk == koz) continue;
                if (ElDegerlendirici.RenkUzunlugu(el, renk) == 0) return renk;
            }
            return null;
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI: Cevap Ver (ortak sordu)
        // ═══════════════════════════════════════════════════════════════

        private static string CevapVer(
            IhaleDurumu durum, List<Card> el, string koz, string sikanRengi)
        {
            int kilitKart = KilitKartSayisiSikanHaric(el, koz, sikanRengi);
            bool kozQVar = ElDegerlendirici.KizVar(el, koz);
            bool rkcb1430 = durum.Anlasma == null || durum.Anlasma.RKCB1430;
            return KademeliCevap(sikanRengi, kilitKart, kozQVar, rkcb1430);
        }

        private static string KademeliCevap(
            string sikanRengi, int kilitKart, bool kozQVar, bool rkcb1430)
        {
            string[] kademeler = KademeleriAl(sikanRengi);
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

        private static string[] KademeleriAl(string sikanRengi)
        {
            switch (sikanRengi)
            {
                case "Sinek": return new[] { "5♦", "5♥", "5♠", "5NT" };
                case "Karo": return new[] { "5♥", "5♠", "5NT", "6♣" };
                case "Kupa": return new[] { "5♠", "5NT", "6♣", "6♦" };
                case "Maça": return new[] { "5NT", "6♣", "6♦", "6♥" };
                default: return null;
            }
        }

        /// <summary>
        /// Şikan rengindeki Ası saymadan kilit kart sayısı (As'lar + koz Papazı).
        /// </summary>
        private static int KilitKartSayisiSikanHaric(
            List<Card> el, string koz, string sikanRengi)
        {
            int sayi = 0;

            foreach (var kart in el)
            {
                if (kart.Value == 14 && kart.Suit != sikanRengi) sayi++;
            }

            if (koz != "NT" && el.Any(c => c.Suit == koz && c.Value == 13))
                sayi++;

            return sayi;
        }
    }
}
