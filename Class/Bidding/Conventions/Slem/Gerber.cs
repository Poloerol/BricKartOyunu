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
    public class Gerber : IKonvansiyon
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
    }
}
