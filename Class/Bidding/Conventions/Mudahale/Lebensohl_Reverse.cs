using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Lebensohl Konvansiyonu (Açıcının Reverse'ü Üzerine).
    /// 
    /// SENARYO: Açıcı reverse yaptı (2♠/2♥), cevapçı 2NT Lebensohl der.
    /// 
    /// Örnek:
    ///   1♦ - 1♠ - 2♥ - ?  (açıcı 2♥ reverse)
    ///   1♣ - 1♥ - 2♠ - ?  (açıcı 2♠ reverse)
    /// 
    /// AMAÇ:
    /// 1. Zayıf (5-9 OP) elleri göstermek
    /// 2. Sign-off imkanı vermek
    /// 3. Açıcının zon kararını kolaylaştırmak
    /// 
    /// Öncelik: 6 (diğer Lebensohl'lardan önce)
    /// </summary>
    public class Lebensohl_Reverse : IKonvansiyon
    {
        public string Ad => "Lebensohl (Reverse)";
        public string Aciklama => "Reverse sekansına Lebensohl cevapları";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 6;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // 1. Partner (açıcı) reverse yaptı mı? (Pas'lar sayılmaz; reverse ortağın 2. gerçek teklifi)
            var ortakGercek = durum.PartnerTeklifleri.Where(t => t != "Pas").ToList();
            var benGercek = durum.KendiTeklifleri.Where(t => t != "Pas").ToList();
            if (ortakGercek.Count < 2) return false;

            string partnerIlk = ortakGercek[0];
            string partnerSon = ortakGercek[1];

            // 2. Ben ilk konuşmamı yaptım mı?
            if (benGercek.Count == 0) return false;

            string benIlk = benGercek[0];

            // 3. Reverse senaryoları:
            //    - 1♣ - 1♠ - 2♠
            //    - 1♦ - 1♠ - 2♥
            //    - 1♣ - 1♥ - 2♠
            //    - 1♦ - 1♥ - 2♠ (nadir)
            //    - 1♣ - 1♦ - 2♦ (nadir)

            bool reverseVar = false;

            // NOT: 1♣ - 1♠ - 2♠ reverse DEĞİL, cevapçının majörüne basit destektir (Lebensohl uygulanmaz).
            // 1♦ - 1♠ - 2♥
            if (partnerIlk == "1♦" && benIlk == "1♠" && partnerSon == "2♥")
                reverseVar = true;
            // 1♣ - 1♥ - 2♠
            else if (partnerIlk == "1♣" && benIlk == "1♥" && partnerSon == "2♠")
                reverseVar = true;
            // 1♦ - 1♥ - 2♠
            else if (partnerIlk == "1♦" && benIlk == "1♥" && partnerSon == "2♠")
                reverseVar = true;

            if (!reverseVar) return false;

            // 4. Sıra bende mi? (Partner son teklifi yaptı)
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;

            // 5. 2NT Lebensohl henüz yapılmadıysa (DURUM 1)
            //    VEYA 2NT yapıldı ve partner 3♣ dedi (DURUM 2)
            if (durum.KendiTeklifleri.Contains("2NT"))
            {
                // DURUM 2: 2NT sonrası, partner 3♣ dedi mi?
                if (ortakGercek.LastOrDefault() == "3♣")
                    return true;
                return false;
            }

            return true;  // DURUM 1: 2NT Lebensohl yapabilirim
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);

            // ═══════════════════════════════════════════════════════════════
            // DURUM 2: 2NT yaptım, partner 3♣ dedi → sign-off veya düzelt
            // ═══════════════════════════════════════════════════════════════
            if (durum.KendiTeklifleri.Contains("2NT") &&
                durum.PartnerTeklifleri.Where(t => t != "Pas").LastOrDefault() == "3♣")
            {
                // 5'li ilk rengimi tekrar göstermek için
                string benIlk = durum.KendiTeklifleri.First(t => t != "Pas");

                // 5'li ♠ (1♠ cevabıysa)
                if (benIlk == "1♠")
                {
                    // Pas (3♣ oynamak için) veya 3♠ sign-off
                    // Eğer ♠ 5'liyse 3♠
                    if (ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 5)
                        return "3♠";
                    return "Pas";
                }

                // 5'li ♥ (1♥ cevabıysa)
                if (benIlk == "1♥")
                {
                    if (ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 5)
                        return "3♥";
                    return "Pas";
                }

                // 5'li ♦ (1♦ cevabıysa)
                if (benIlk == "1♦")
                {
                    if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 5)
                        return "3♦";
                    return "Pas";
                }

                // Varsayılan
                return "Pas";
            }

            // ═══════════════════════════════════════════════════════════════
            // DURUM 1: 2NT Lebensohl yapıyorum
            // ═══════════════════════════════════════════════════════════════

            // 9+ OP: Natürel forcing
            if (hp >= 9)
            {
                // Açıcının reverse renginde 4+ kart (majör fit) → önce fiti göster (3 seviye, forcing)
                string revKoz = durum.PartnerTeklifleri.Where(t => t != "Pas").ElementAt(1).Substring(1);
                string revIsim = revKoz == "♥" ? "Kupa" : revKoz == "♠" ? "Maça" : null;
                if (revIsim != null && ElDegerlendirici.RenkUzunlugu(el, revIsim) >= 4)
                    return "3" + revKoz;
                // 4+ ♣
                if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 4) return "3♣";
                // 4+ ♦
                if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 4) return "3♦";
                // 5+ ♠
                if (ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 5) return "3♠";
                // 4+ ♥
                if (ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4) return "3♥";

                return "3♣";
            }

            // 5-8 OP: 2NT Lebensohl
            return "2NT";
        }
    }
}