using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Gerber Konvansiyonu — DOĞRUDAN NT açılışına 4♣ ile As sorma.
    /// 
    /// SORMA (yalnızca şu koşulların HEPSİNDE):
    /// - Ortağın bu ihaledeki TEK gerçek teklifi 1NT veya 2NT (doğrudan Gerber)
    /// - Ben henüz gerçek teklif vermedim (Stayman/transfer vb. sonrası Gerber yok)
    /// - Rakipler gerçek teklif vermedi
    /// - Elimde 4+ kartlı majör yok (Stayman/Transfer önce gelir)
    /// - Slam gücü: 1NT (15-17) karşısında 16+ HP, 2NT (20-21) karşısında 12+ HP
    /// 
    /// CEVAP (ortak 4♣ sordu ve 4♣'dan önceki gerçek teklif NT ise):
    ///   4♦ = 0 veya 4 As, 4♥ = 1 As, 4♠ = 2 As, 4NT = 3 As
    /// 
    /// Öncelik: 13
    /// </summary>
    public class Gerber : IKonvansiyon, IBilincliPas
    {
        public string Ad => "Gerber";
        public string Aciklama => "NT sekansında As sorusu";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 13;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Puppet Stayman sekansında Gerber yok
            if (durum.KendiTeklifleri.Contains("3♣") || durum.PartnerTeklifleri.Contains("3♣"))
                return false;

            // ─── DEVAM: Gerber sorusunun cevabı geldi (soran) ya da soranın "pas/niceliksel" 4NT'si (cevaplayan) ──
            if (DevamDurumu(durum) != null) return true;

            // 4♣'ı ben zaten kullandıysam tekrar Gerber yok
            if (durum.KendiTeklifleri.Contains("4♣")) return false;

            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (gercek.Count == 0) return false;

            // Rakip gerçek teklif verdiyse Gerber yok (müdahale)
            if (gercek.Any(h => durum.Rakipler.Contains(h.Oyuncu))) return false;

            var son = gercek[gercek.Count - 1];
            if (son.Oyuncu != durum.Partner) return false;

            // ─── CEVAP: ortak 4♣ sordu ───────────────────────────────────
            if (son.Teklif == "4♣")
            {
                if (gercek.Count < 2) return false;
                var onceki = gercek[gercek.Count - 2];
                return onceki.Koz == "NT" && onceki.Seviye <= 3;
            }

            // ─── SORMA: doğrudan Gerber ──────────────────────────────────
            // Ortağın TEK gerçek teklifi 1NT/2NT olmalı (ben hiç gerçek teklif vermedim)
            if (gercek.Count != 1) return false;
            if (son.Koz != "NT" || son.Seviye > 2) return false;

            var el = durum.AktifOyuncuEli;

            // 4+ majör varsa Stayman/Transfer yolu
            if (ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4) return false;
            if (ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4) return false;

            int hp = ElDegerlendirici.HCP(el);
            int gerekli = son.Seviye == 1 ? 16 : 12;
            return hp >= gerekli;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;

            string dv = DevamDurumu(durum);
            if (dv == "SORAN") return SoranDevami(durum, el);
            if (dv == "CEVAPLAYAN") return "Pas";   // soran 4NT ile bitirdi (as eksik): oyun 4NT'de kalır

            // Ortak 4♣ sordu → As sayısına göre cevap
            if (durum.SonGercekTeklif == "4♣" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                int asSayisi = ElDegerlendirici.AsSayisi(el);
                switch (asSayisi)
                {
                    case 0: return "4♦";
                    case 1: return "4♥";
                    case 2: return "4♠";
                    case 3: return "4NT";
                    default: return "4♦";   // 4 As
                }
            }

            // Biz soruyoruz
            return "4♣";
        }

        // ═══════════════════════════════════════════════════════════════════
        // GERBER SONRASI DEVAM
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// "SORAN": benim 4♣ Gerber sorumun cevabı geldi (NT – 4♣ – 4♦/4♥/4♠/4NT) ve karar bende.
        /// "CEVAPLAYAN": ben cevap verdim, soran 4NT ile devam etti (oyun/niceliksel değil, as eksik → bitir).
        /// Aksi halde null.
        /// </summary>
        private static string DevamDurumu(IhaleDurumu durum)
        {
            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (gercek.Count < 3) return null;
            if (gercek.Any(h => durum.Rakipler.Contains(h.Oyuncu))) return null;
            var son = gercek[gercek.Count - 1];
            if (son.Oyuncu != durum.Partner) return null;

            var onceki = gercek[gercek.Count - 2];
            if (onceki.Oyuncu == durum.AktifOyuncu && onceki.Teklif == "4♣" && gercek.Count >= 3)
            {
                var nt = gercek[gercek.Count - 3];
                if (nt.Oyuncu == durum.Partner && nt.Koz == "NT" && nt.Seviye <= 2 &&
                    (son.Teklif == "4♦" || son.Teklif == "4♥" || son.Teklif == "4♠" || son.Teklif == "4NT"))
                    return "SORAN";
            }
            if (son.Teklif == "4NT" && gercek.Count >= 4 && onceki.Oyuncu == durum.AktifOyuncu &&
                (onceki.Teklif == "4♦" || onceki.Teklif == "4♥" || onceki.Teklif == "4♠"))
            {
                var g = gercek[gercek.Count - 3];
                var nt = gercek[gercek.Count - 4];
                if (g.Oyuncu == durum.Partner && g.Teklif == "4♣" && nt.Oyuncu == durum.AktifOyuncu && nt.Koz == "NT")
                    return "CEVAPLAYAN";
            }
            return null;
        }

        private static string SoranDevami(IhaleDurumu durum, List<Card> el)
        {
            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            var son = gercek[gercek.Count - 1];
            var nt = gercek[gercek.Count - 3];
            int benAs = ElDegerlendirici.AsSayisi(el);
            int ortakAs = son.Teklif == "4♦" ? 0 : son.Teklif == "4♥" ? 1 : son.Teklif == "4♠" ? 2 : 3;
            int hp = ElDegerlendirici.HCP(el);
            int ortakOrta = nt.Seviye == 1 ? 16 : 20;   // 1NT: 15-17, 2NT: 20-21
            int toplamHp = hp + ortakOrta;

            if (benAs + ortakAs >= 4)
            {
                if (toplamHp >= 37) return "7NT";
                if (toplamHp >= 33) return "6NT";
            }
            // Bir as eksik ya da güç yetersiz: oyunu bitir (4NT bir sonraki basamaktır; cevap 4NT ise zaten orada)
            return son.Teklif == "4NT" ? "Pas" : "4NT";
        }
    }
}
