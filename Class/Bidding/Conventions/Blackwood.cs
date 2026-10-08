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
    ///   5♠ = 2 As + 1 Papaz
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
    /// </summary>
    public class Blackwood : IKonvansiyon
    {
        public string Ad => "Blackwood";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 15;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            if (durum.IhaleBittiMi()) return false;

            // Bir koz anlaşması var mı?
            string koz = durum.AnlasilanKoz();
            if (string.IsNullOrEmpty(koz)) return false;

            // Aktif oyuncu daha önce 4NT demedi mi?
            if (durum.KendiTeklifleri.Contains("4NT")) return false;

            // ── DURUM A: BEN CEVAP VERİYORUM (Partner 4NT dedi) ──
            if (durum.SonGercekTeklif == "4NT" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                return true;
            }

            // ── DURUM B: BEN SORUYORUM (4NT açacağım) ──

            // 4NT sorabilmek için genellikle 3. veya 4. seviyede bir koz anlaşması olmalı
            int seviye = durum.KacinciSeviye();
            if (seviye < 2) return false;

            // El gücü kontrolü: Sadece HP'ye değil, Quick Tricks'e de bakıyoruz.
            // Slam denemesi için elin yeterince kuvvetli olması gerekir.
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            double qt = ElDegerlendirici.HizliElSayisi(durum.AktifOyuncuEli);

            // 15+ HP veya (12+ HP ve 6+ Quick Trick) ise slam denemesi yapabilir
            if (hp >= 15 || (hp >= 12 && qt >= 6.0))
            {
                return true;
            }

            return false;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string koz = durum.AnlasilanKoz();

            if (durum.SonGercekTeklif == "4NT" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                return BlackwoodCevabi(durum, el, koz);
            }

            return "4NT";
        }

        private string BlackwoodCevabi(IhaleDurumu durum, List<Card> el, string koz)
        {
            bool rkcbAktif = durum.Anlasma.RKCB;
            bool rkcb1430 = durum.Anlasma.RKCB1430;

            if (rkcbAktif)
            {
                return RkcbCevabi(el, koz, rkcb1430);
            }

            return KlasikBlackwoodCevabi(el);
        }

        private string KlasikBlackwoodCevabi(List<Card> el)
        {
            int asSayisi = ElDegerlendirici.AsSayisi(el);
            int papazSayisi = ElDegerlendirici.PapazSayisi(el);

            switch (asSayisi)
            {
                case 0: return "5♣";
                case 1: return "5♦";
                case 2: return (papazSayisi >= 1) ? "5♠" : "5♥";
                case 3: return "5♣";
                case 4: return "5♦";
                default: return "5♣";
            }
        }

        private string RkcbCevabi(List<Card> el, string koz, bool rkcb1430)
        {
            int kilitKart = ElDegerlendirici.KilitKartSayisi(el, koz);
            bool kozPapaziVar = ElDegerlendirici.PapazVar(el, koz);

            if (rkcb1430)
            {
                switch (kilitKart)
                {
                    case 0: return "5♦";
                    case 1: return "5♣";
                    case 2: return kozPapaziVar ? "5♠" : "5♥";
                    case 3: return "5♦";
                    case 4: return "5♣";
                    case 5: return kozPapaziVar ? "5♠" : "5♥";
                    default: return "5♦";
                }
            }
            else
            {
                switch (kilitKart)
                {
                    case 0: return "5♣";
                    case 1: return "5♦";
                    case 2: return kozPapaziVar ? "5♠" : "5♥";
                    case 3: return "5♣";
                    case 4: return "5♦";
                    case 5: return kozPapaziVar ? "5♠" : "5♥";
                    default: return "5♣";
                }
            }
        }
    }
}
