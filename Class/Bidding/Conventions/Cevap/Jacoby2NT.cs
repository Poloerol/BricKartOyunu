using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Jacoby 2NT Konvansiyonu — Majör açılışına 2NT yapay güçlü destek.
    /// 
    /// Kural:
    /// - Partner 1♠ veya 1♥ açtı
    /// - Rakip müdahale etmedi
    /// - Elimizde 4+ majör destek + 13+ HP var
    /// - 2NT deriz → "forcing, slam denemesi"
    /// 
    /// Öncelik: 16 (Blackwood 15 sonrası)
    /// </summary>
    public class Jacoby2NT : IKonvansiyon, IBilincliPas
    {
        public string Ad => "Jacoby 2NT";
        public string Aciklama => "Majör açılışına güçlü 2NT desteği";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 17;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Jacoby 2NT sonrası devam (açıcının cevabı ve cevaplayanın oyun kararı)
            if (DevamDurumu(durum) != 0) return true;

            // Partner 1♠ veya 1♥ açtı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            string partnerSon = durum.PartnerTeklifleri.Last();
            if (partnerSon != "1♠" && partnerSon != "1♥") return false;

            // Biz henüz konuşmadık
            if (durum.KendiTeklifleri.Count > 0) return false;

            // Rakip gerçek bir teklif verdi mi? (Pas hariç)
            bool rakipGercekTeklifVerdi = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            if (rakipGercekTeklifVerdi) return false;

            // Majör desteği 4+
            string koz = partnerSon == "1♠" ? "Maça" : "Kupa";
            int destek = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, koz);
            if (destek < 4) return false;

            // 13+ HP
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 13) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            int devam = DevamDurumu(durum);
            if (devam == 1) return AciciCevabi(durum);
            if (devam == 2) return CevaplayanDevami(durum);
            return "2NT";
        }

        // ═══════════════════════════════════════════════════════════════════
        // JACOBY 2NT SONRASI (Max Hardy "Two Over One" + Konvansiyonlar\Jacoby 2NT.doc)
        //   Açıcı:  kısa renk (singleton/void) → o rengi 3 seviyesinde
        //           kaliteli 5'li yan renk     → 4 seviyesinde
        //           kısa renk yok: 16+ HP → 3M, 14-15 HP → 3NT, minimum → 4M (hızlı varış)
        //   Cevaplayan: açıcı 4M dediyse pas (19+ HP: başka konvansiyonlara bırak);
        //               aksi halde 17+ HP slam araştırması başkasına bırakılır, daha azıyla 4M.
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>0: uygun değil, 1: açıcı cevabı, 2: cevaplayanın devamı.</summary>
        private static int DevamDurumu(IhaleDurumu durum)
        {
            if (durum.Gecmis == null) return 0;
            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (gercek.Count != 2 && gercek.Count != 3) return 0;

            // Rakipler gerçek teklif vermemiş olmalı
            if (gercek.Any(h => durum.Rakipler.Contains(h.Oyuncu))) return 0;

            string acilis = gercek[0].Teklif;
            if (acilis != "1♠" && acilis != "1♥") return 0;
            if (gercek[1].Teklif != "2NT") return 0;
            if (gercek[0].Oyuncu == gercek[1].Oyuncu) return 0;

            if (gercek.Count == 2)
                return gercek[0].Oyuncu == durum.AktifOyuncu ? 1 : 0;

            // Count == 3: açıcı cevap verdi, sıra cevaplayanda
            if (gercek[2].Oyuncu != gercek[0].Oyuncu) return 0;
            return gercek[1].Oyuncu == durum.AktifOyuncu ? 2 : 0;
        }

        private static string AciciCevabi(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            string koz = gercek[0].Teklif == "1♠" ? "Maça" : "Kupa";
            string kozSem = KozYardimcisi.KozSembolu(koz);
            int hp = ElDegerlendirici.HCP(el);

            string[] yanlar = { "Sinek", "Karo", "Kupa", "Maça" };

            // 1) Kısa renk (singleton veya void): rengi 3 seviyesinde söyle
            foreach (string r in yanlar)
            {
                if (r == koz) continue;
                if (ElDegerlendirici.RenkUzunlugu(el, r) <= 1)
                    return "3" + KozYardimcisi.KozSembolu(r);
            }

            // 2) Kaliteli 5'li yan renk: 4 seviyesinde
            foreach (string r in yanlar)
            {
                if (r == koz) continue;
                if (ElDegerlendirici.IyiRenk(el, r))
                    return "4" + KozYardimcisi.KozSembolu(r);
            }

            // 3) Kısa renk yok
            if (hp >= 16) return "3" + kozSem;        // ekstra değerli
            if (hp >= 14) return "3NT";               // 14-15, dengeli
            return "4" + kozSem;                      // minimum: doğrudan oyun
        }

        private static string CevaplayanDevami(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            string koz = gercek[0].Teklif == "1♠" ? "Maça" : "Kupa";
            string kozOyun = "4" + KozYardimcisi.KozSembolu(koz);
            int hp = ElDegerlendirici.HCP(el);
            string son = gercek[2].Teklif;

            if (son == kozOyun) return hp >= 19 ? null : "Pas";   // açıcı minimum, oyunda
            if (hp >= 17) return null;                            // slam araştırması: Blackwood/cue-bid vb.
            return kozOyun;
        }
    }
}