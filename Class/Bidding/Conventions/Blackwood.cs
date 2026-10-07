using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Blackwood ve RKCB (Roman Key Card Blackwood) Konvansiyonu.
    /// 
    /// KLASİK BLACKWOOD (RKCB=false):
    ///   4NT sorar → As sayısı
    ///   5♣ = 0 veya 3 As
    ///   5♦ = 1 veya 4 As
    ///   5♥ = 2 As
    ///   5♠ = 2 As + 1 Papaz (bazı sistemlerde)
    /// 
    /// RKCB (RKCB=true, RKCB1430=false):
    ///   4NT sorar → Kilit kart sayısı (4 As + koz K)
    ///   5♣ = 0 veya 3 kilit kart
    ///   5♦ = 1 veya 4 kilit kart
    ///   5♥ = 2 veya 5 kilit kart, koz K'si YOK
    ///   5♠ = 2 veya 5 kilit kart, koz K'si VAR
    /// 
    /// RKCB 1430 (RKCB1430=true):
    ///   5♣ = 1 veya 4 kilit kart
    ///   5♦ = 0 veya 3 kilit kart
    ///   5♥ = 2 veya 5 kilit kart, koz K'si YOK
    ///   5♠ = 2 veya 5 kilit kart, koz K'si VAR
    /// 
    /// Öncelik: 15 (Stayman 10, JacobyTransfer 15 ile aynı seviye)
    /// </summary>
    public class Blackwood : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "Blackwood";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 15;  // Stayman (10) sonra, BasitCevap (100) önce

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. İhale zaten bitmişse → hayır
            if (durum.IhaleBittiMi()) return false;

            // 2. Bir koz anlaşması var mı?
            string koz = durum.AnlasilanKoz();
            if (string.IsNullOrEmpty(koz)) return false;

            // 3. Aktif oyuncu daha önce 4NT demedi mi?
            if (durum.KendiTeklifleri.Contains("4NT")) return false;

            // ═══════════════════════════════════════════════════════════════
            // DURUM A: BEN CEVAP VERİYORUM (Partner 4NT dedi)
            // ═══════════════════════════════════════════════════════════════
            if (durum.SonGercekTeklif == "4NT" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                // Cevap verirken HP kontrolü YAPILMAZ — kilit kart sayısına göre cevap verilir
                return true;
            }

            // ═══════════════════════════════════════════════════════════════
            // DURUM B: BEN SORUYORUM (4NT açacağım)
            // ═══════════════════════════════════════════════════════════════

            // 👇👇👇 YENİ EKLENEN KISIM — BURAYA 👇👇👇
            // 4. Sadece 3. seviyeden sonra Blackwood sorulabilir
            //    (2♥ transfer kabulü gibi düşük seviyeli anlaşmalarda Blackwood tetiklenmesin)
            int seviye = durum.KacinciSeviye();
            if (seviye < 3) return false;
            if (seviye > 4) return false;   // 4NT zaten 4. seviye
            // 👆👆👆 YENİ EKLENEN KISIM SONU 👆👆👆

            // 5. Elimizde yeterli puan var mı? (Slam denemesi için 15+ HP)
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 15) return false;

            // 6. Partner ile aynı kozda anlaşılmış olmalı
            //    (AnlasilanKoz zaten kontrol etti)

            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string koz = durum.AnlasilanKoz();

            // Bu Blackwood sorusu mu, yoksa Blackwood cevabı mı?
            // Son gerçek teklif 4NT mi?
            if (durum.SonGercekTeklif == "4NT" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                // PARTNER 4NT dedi → BEN CEVAP VERİYORUM
                return BlackwoodCevabi(durum, el, koz);
            }

            // Aksi halde → BEN 4NT SORUYORUM
            return "4NT";
        }

        // ═══════════════════════════════════════════════════════════════════
        // BLACKWOOD SORUSUNA CEVAP
        // ═══════════════════════════════════════════════════════════════════

        private string BlackwoodCevabi(IhaleDurumu durum, List<Card> el, string koz)
        {
            // RKCB aktif mi?
            bool rkcbAktif = durum.Anlasma.RKCB;
            bool rkcb1430 = durum.Anlasma.RKCB1430;

            if (rkcbAktif)
            {
                return RkcbCevabi(el, koz, rkcb1430);
            }
            else
            {
                return KlasikBlackwoodCevabi(el);
            }
        }

        /// <summary>
        /// Klasik Blackwood cevabı: As sayısı.
        /// </summary>
        private string KlasikBlackwoodCevabi(List<Card> el)
        {
            int asSayisi = ElDegerlendirici.AsSayisi(el);
            int papazSayisi = ElDegerlendirici.PapazSayisi(el);

            switch (asSayisi)
            {
                case 0: return "5♣";  // 0 veya 3
                case 1: return "5♦";  // 1 veya 4
                case 2:
                    // 2 As + Papaz var mı?
                    if (papazSayisi >= 1) return "5♠";
                    return "5♥";
                case 3: return "5♣";  // 0 veya 3
                case 4: return "5♦";  // 1 veya 4
                default: return "5♣";
            }
        }

        /// <summary>
        /// RKCB cevabı: Kilit kart sayısı (4 As + koz K).
        /// </summary>
        private string RkcbCevabi(List<Card> el, string koz, bool rkcb1430)
        {
            int kilitKart = ElDegerlendirici.KilitKartSayisi(el, koz);
            bool kozPapaziVar = ElDegerlendirici.PapazVar(el, koz);

            if (rkcb1430)
            {
                // 1430: 1/4 → 5♣, 0/3 → 5♦
                switch (kilitKart)
                {
                    case 0: return "5♦";
                    case 1: return "5♣";
                    case 2:
                        return kozPapaziVar ? "5♠" : "5♥";  // 2/5
                    case 3: return "5♦";
                    case 4: return "5♣";
                    case 5:
                        return kozPapaziVar ? "5♠" : "5♥";
                    default: return "5♦";
                }
            }
            else
            {
                // Standart: 0/3 → 5♣, 1/4 → 5♦
                switch (kilitKart)
                {
                    case 0: return "5♣";
                    case 1: return "5♦";
                    case 2:
                        return kozPapaziVar ? "5♠" : "5♥";  // 2/5
                    case 3: return "5♣";
                    case 4: return "5♦";
                    case 5:
                        return kozPapaziVar ? "5♠" : "5♥";
                    default: return "5♣";
                }
            }
        }
    }
}