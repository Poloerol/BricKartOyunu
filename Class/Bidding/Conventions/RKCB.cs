using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Roman Key Card Blackwood (RKCB) — 1430/0314
    /// </summary>
    public class RKCB : IKonvansiyon
    {
        public string Ad => "RKCB";
        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 28: TrialBid, TwoWayCheckback, XYZ'den SONRA çalışsın
        /// ki onların sekanslarını bozmasın.
        /// </summary>
        public int Oncelik => 28;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // ─── DURUM 1: Partner 4NT dedi → cevap veriyoruz ─────────
            if (durum.SonGercekTeklif == "4NT" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                string kozCevap = durum.AnlasilanKoz();
                if (string.IsNullOrEmpty(kozCevap)) return false;
                if (kozCevap == "NT") return false;   // NT'de klasik Blackwood
                return true;
            }

            // ─── DURUM 2: Biz 4NT diyeceğiz ──────────────────────────
            string koz = durum.AnlasilanKoz();
            if (string.IsNullOrEmpty(koz)) return false;
            if (koz == "NT") return false;   // NT'de RKCB değil, Blackwood

            // Daha önce 4NT sormadık
            if (durum.KendiTeklifleri.Contains("4NT")) return false;

            // 4 seviyesinin altında olmalı
            int seviye = durum.KacinciSeviye();
            if (seviye < 3 || seviye > 4) return false;   // en az 3. seviye

            // En az 3 gerçek teklif olmalı (açılış + cevap + destek)
            var gercelTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();
            if (gercelTeklifler.Count < 3) return false;

            // Koz en az 2 kez teklif edilmiş olmalı (anlaşma)
            int kozSayisi = gercelTeklifler.Count(h => h.Koz == koz);
            if (kozSayisi < 2) return false;

            // Son teklif bizim VEYA partnerimizin olmalı
            var sonTeklif = gercelTeklifler.Last();
            if (sonTeklif.Oyuncu != durum.AktifOyuncu &&
                sonTeklif.Oyuncu != durum.Partner)
                return false;

            // Son teklif 3+ seviyede olmalı (slam bölgesi)
            if (sonTeklif.Seviye < 3) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            // ─── Cevap ───────────────────────────────────────────────
            if (durum.SonGercekTeklif == "4NT" &&
                durum.SonGercekTeklifSahibi == durum.Partner)
            {
                string koz = durum.AnlasilanKoz();
                return KilitKartCevabi(durum, el, koz);
            }

            // ─── Soru ────────────────────────────────────────────────
            return "4NT";
        }

        private static string KilitKartCevabi(IhaleDurumu durum, List<Card> el, string koz)
        {
            int kilitKart = ElDegerlendirici.KilitKartSayisi(el, koz);
            bool kozQVar = ElDegerlendirici.KizVar(el, koz);
            bool rkcb1430 = durum.Anlasma == null || durum.Anlasma.RKCB1430;

            if (rkcb1430)
            {
                switch (kilitKart)
                {
                    case 0: return "5♦";
                    case 1: return "5♣";
                    case 2: return kozQVar ? "5♠" : "5♥";
                    case 3: return "5♦";
                    case 4: return "5♣";
                    case 5: return "5♣";
                    default: return "5♦";
                }
            }
            else
            {
                switch (kilitKart)
                {
                    case 0: return "5♣";
                    case 1: return "5♦";
                    case 2: return kozQVar ? "5♠" : "5♥";
                    case 3: return "5♣";
                    case 4: return "5♦";
                    case 5: return "5♦";
                    default: return "5♣";
                }
            }
        }
    }
}