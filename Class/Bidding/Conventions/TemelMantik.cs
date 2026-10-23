using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Temel Mantık (Fallback) — Briç kurallarının evrensel değişmezleri.
    /// 
    /// Bu konvansiyon her zaman en sona (Oncelik = 999) çalışır. 
    /// Diğer özel konvansiyonlar (Stayman, Jacoby Transfer, vb.) devreye 
    /// girmediğinde, bu sınıf briç matematiğine göre en mantıklı teklifi verir.
    /// 
    /// İçerdiği kurallar:
    /// - Açılış: 22+ HP → 2♣, 20-21 dengeli → 2NT, 15-17 dengeli → 1NT, 12+ → En uzun renk
    /// - Rakip müdahalesi: 8+ HP ve 5'li renk ile 1. seviyede, 11+ HP ile 2. seviyede
    /// - Partner açılışına cevap: Fit varsa destek, yoksa yeni renk, NT, Pas
    /// - Açıcı rebid: ÖNCE FİT (partnerin rengi 3+ destek ise), SONRA NT/yeni renk
    /// </summary>
    public class TemelMantik : IKonvansiyon
    {
        public string Ad => "Temel Mantık (Fallback)";
        public string Aciklama => "Evrensel briç kuralları (fit, NT, puan)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 999;  // Her zaman en son çalışır

        /// <summary>
        /// TemelMantik her zaman uygundur. Çünkü o "son çare"dir.
        /// Diğer konvansiyonlar devreye girmezse, bu çalışır.
        /// </summary>
        public bool UygunMu(IhaleDurumu durum)
        {
            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[TemelMantik] Çağrıldı! SonGercekTeklif={durum?.SonGercekTeklif}, " +
                $"KendiTeklifleri=[{string.Join(",", durum?.KendiTeklifleri ?? new List<string>())}]");

            var el = durum.AktifOyuncuEli;
            if (el == null || el.Count != 13) return "Pas";

            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // ═══════════════════════════════════════════════════════════════
            // 0. PARTNER 1NT AÇTIYSA (Cevapçı olarak)
            // ═══════════════════════════════════════════════════════════════
            if (durum.PartnerTeklifleri.Count > 0 &&
                durum.PartnerTeklifleri.Last() == "1NT" &&
                durum.KendiTeklifleri.Count == 0)
            {
                return "Pas";
            }

            // Partner 1NT açtı ve ben transfer/stayman yaptım, şimdi cevap veriyorum
            if (durum.PartnerTeklifleri.Count > 0 &&
                durum.PartnerTeklifleri.Contains("1NT") &&
                durum.KendiTeklifleri.Count > 0)
            {
                if (durum.SonGercekTeklif == "2♦" && durum.SonGercekTeklifSahibi == durum.Partner)
                    return "2♥";
                if (durum.SonGercekTeklif == "2♥" && durum.SonGercekTeklifSahibi == durum.Partner)
                    return "2♠";
                if (durum.SonGercekTeklif == "2♣" && durum.SonGercekTeklifSahibi == durum.Partner)
                {
                    int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                    int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                    if (macca >= 4) return "2♠";
                    if (kupa >= 4) return "2♥";
                    return "2♦";
                }
                return "Pas";
            }

            // ═══════════════════════════════════════════════════════════════
            // 0.5. PARTNER 3+ SEVİYEDE SIÇRAMA YAPTIYSA (Pick-a-Slam)
            // ═══════════════════════════════════════════════════════════════
            if (durum.PartnerActiMi() && durum.KendiTeklifleri.Count > 0 && hp >= 15)
            {
                var partnerSonTeklif = durum.PartnerTeklifleri.LastOrDefault();
                if (!string.IsNullOrEmpty(partnerSonTeklif))
                {
                    int partnerSeviye = KozYardimcisi.TekliftenSeviyeCikar(partnerSonTeklif);
                    if (partnerSeviye >= 3)
                    {
                        int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                        int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                        int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                        int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
                        bool ikiMajor = (macca >= 4 && kupa >= 4);
                        bool minorVar = (karo >= 4 || sinek >= 4);

                        if (ikiMajor || minorVar)
                        {
                            if (!durum.KendiTeklifleri.Contains("5NT"))
                                return "5NT";
                        }
                    }
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // 1. İLK TEKLİF (AÇILIŞ)
            // ═══════════════════════════════════════════════════════════════
            if (durum.IlkTeklifMi())
            {
                if (hp >= 22) return "2♣";
                if (hp >= 20 && hp <= 21 && dengeli) return "2NT";
                if (hp >= 15 && hp <= 17 && dengeli) return "1NT";
                if (hp >= 12) return EnUzunRengiAc(el);
                return "Pas";
            }

            // ═══════════════════════════════════════════════════════════════
            // 2. RAKİP AÇTI, BEN MÜDAHALE EDİYORUM
            // ═══════════════════════════════════════════════════════════════
            if (durum.RakipActiMi() && durum.KendiTeklifleri.Count == 0)
            {
                return RakipAcilisinaMudahale(durum, el, hp);
            }

            // ═══════════════════════════════════════════════════════════════
            // 3. PARTNER AÇTI, BEN İLK KEZ CEVAP VERİYORUM
            // ═══════════════════════════════════════════════════════════════
            if (durum.PartnerActiMi() && durum.KendiTeklifleri.Count == 0)
            {
                return PartnerAcilisinaCevap(durum, el, hp, dengeli);
            }

            // ═══════════════════════════════════════════════════════════════
            // 4. AÇICI REBID — ÖNCE FİT, SONRA NT
            // ═══════════════════════════════════════════════════════════════
            if (durum.PartnerActiMi() && durum.KendiTeklifleri.Count > 0)
            {
                var partnerSonTeklif = durum.PartnerTeklifleri.LastOrDefault();
                if (!string.IsNullOrEmpty(partnerSonTeklif))
                {
                    if (partnerSonTeklif == "1NT") return "Pas";

                    string partnerKozu = KozYardimcisi.TekliftenKozCikar(partnerSonTeklif);
                    int partnerSeviye = KozYardimcisi.TekliftenSeviyeCikar(partnerSonTeklif);
                    if (partnerSeviye <= 0) partnerSeviye = 1;

                    int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                    int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                    int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                    int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

                    // ═══ KRİTİK: ÖNCE FİT KONTROLÜ ═══
                    if (!string.IsNullOrEmpty(partnerKozu) && partnerKozu != "NT")
                    {
                        int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                        if (destek >= 3)
                        {
                            if (partnerSeviye == 1)
                            {
                                string hedef = null;
                                if (partnerKozu == "Kupa") hedef = "2♥";
                                else if (partnerKozu == "Maça") hedef = "2♠";
                                else if (partnerKozu == "Karo") hedef = "2♦";
                                else if (partnerKozu == "Sinek") hedef = "2♣";

                                if (hedef != null && !durum.KendiTeklifleri.Contains(hedef))
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        $"[TemelMantik] FİT: Partner {partnerKozu} dedi, bende {destek} destek var → {hedef}");
                                    return hedef;
                                }
                            }
                            else if (partnerSeviye == 2)
                            {
                                string hedef = null;
                                if (partnerKozu == "Kupa") hedef = "3♥";
                                else if (partnerKozu == "Maça") hedef = "3♠";
                                else if (partnerKozu == "Karo") hedef = "3♦";
                                else if (partnerKozu == "Sinek") hedef = "3♣";

                                if (hedef != null && !durum.KendiTeklifleri.Contains(hedef))
                                    return hedef;
                            }
                        }
                    }

                    // ═══ FİT YOKSA: NT veya yeni renk ═══
                    if (dengeli)
                    {
                        if (hp >= 18 && hp <= 19 && !durum.KendiTeklifleri.Contains("2NT"))
                            return "2NT";
                        if (hp >= 12 && hp <= 14 && !durum.KendiTeklifleri.Contains("1NT"))
                            return "1NT";
                    }

                    if (macca >= 5 && partnerKozu != "Maça" && !durum.KendiTeklifleri.Contains("2♠"))
                        return "2♠";
                    if (kupa >= 5 && partnerKozu != "Kupa" && !durum.KendiTeklifleri.Contains("2♥"))
                        return "2♥";
                    if (karo >= 5 && partnerKozu != "Karo" && !durum.KendiTeklifleri.Contains("2♦"))
                        return "2♦";
                    if (sinek >= 5 && partnerKozu != "Sinek" && !durum.KendiTeklifleri.Contains("2♣"))
                        return "2♣";
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // 5. GÜVENLİK KİLİDİ
            // ═══════════════════════════════════════════════════════════════
            int guvenlikSonSeviye = 0;
            var guvenlikSonTeklif = durum.SonGercekTeklif;
            if (!string.IsNullOrEmpty(guvenlikSonTeklif))
                guvenlikSonSeviye = KozYardimcisi.TekliftenSeviyeCikar(guvenlikSonTeklif);

            if (guvenlikSonSeviye >= 4 && hp < 15)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[TemelMantik] İhale çok yükseldi (Seviye {guvenlikSonSeviye}, HP {hp}) → Pas");
                return "Pas";
            }

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════
        // YARDIMCI METOTLAR
        // ═══════════════════════════════════════════════════════════════

        private string PartnerAcilisinaCevap(IhaleDurumu durum, List<Card> el, int hp, bool dengeli)
        {
            string partnerTeklifi = durum.PartnerTeklifleri.Last();
            string partnerKozu = KozYardimcisi.TekliftenKozCikar(partnerTeklifi);

            if (partnerTeklifi == "1NT")
            {
                if (hp >= 10 && dengeli) return "3NT";
                if (hp >= 8 && hp <= 9 && dengeli) return "2NT";
                return "Pas";
            }

            if (partnerTeklifi == "2NT")
            {
                if (hp >= 5) return "3NT";
                return "Pas";
            }

            if (partnerKozu != null && partnerKozu != "NT")
            {
                if (hp < 6) return "Pas";

                if (partnerKozu == "Maça" || partnerKozu == "Kupa")
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 4)
                    {
                        if (hp >= 13) return "2NT";
                        if (hp >= 10 && hp <= 12) return "3" + KozYardimcisi.KozSembolu(partnerKozu);
                        if (hp >= 6 && hp <= 9) return "2" + KozYardimcisi.KozSembolu(partnerKozu);
                    }
                    else if (destek == 3)
                    {
                        if (hp >= 10 && hp <= 12) return "2" + KozYardimcisi.KozSembolu(partnerKozu);
                        if (hp >= 6 && hp <= 9) return "2" + KozYardimcisi.KozSembolu(partnerKozu);
                    }
                }

                if (partnerKozu != "Maça" && ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4)
                    return "1♠";
                if (partnerKozu != "Kupa" && ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4)
                    return "1♥";

                if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 5 && partnerKozu != "Karo")
                    return "2♦";
                if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 5 && partnerKozu != "Sinek")
                    return "2♣";

                if (dengeli && hp >= 10 && hp <= 12) return "2NT";
                if (dengeli && hp >= 13 && hp <= 15) return "3NT";

                if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 4) return "2♦";
                if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 4) return "2♣";
            }

            return "Pas";
        }

        private string RakipAcilisinaMudahale(IhaleDurumu durum, List<Card> el, int hp)
        {
            if (hp < 8) return "Pas";

            if (durum.Gecmis.Any(h => h.GercekTeklifMi && h.Oyuncu == durum.Partner))
                return "Pas";

            var sonRakip = durum.Gecmis
                .Where(h => h.GercekTeklifMi && durum.Rakipler.Contains(h.Oyuncu))
                .LastOrDefault();
            if (sonRakip == null || string.IsNullOrEmpty(sonRakip.Teklif)) return "Pas";

            string rakipKozu = KozYardimcisi.TekliftenKozCikar(sonRakip.Teklif);
            if (string.IsNullOrEmpty(rakipKozu) || rakipKozu == "NT") return "Pas";

            if (!char.IsDigit(sonRakip.Teklif[0]) || sonRakip.Teklif[0] != '1') return "Pas";

            var sira = new Dictionary<string, int>
            {
                { "Sinek", 0 }, { "Karo", 1 }, { "Kupa", 2 }, { "Maça", 3 }
            };
            if (!sira.ContainsKey(rakipKozu)) return "Pas";

            if (rakipKozu != "Maça" && sira["Maça"] > sira[rakipKozu])
            {
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                if (macca >= 5 && hp >= 8) return "1♠";
            }
            if (rakipKozu != "Kupa" && sira["Kupa"] > sira[rakipKozu])
            {
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                if (kupa >= 5 && hp >= 8) return "1♥";
            }

            var adaylar = sira.Keys
                .Where(r => r != rakipKozu && ElDegerlendirici.RenkUzunlugu(el, r) >= 5)
                .OrderByDescending(r => ElDegerlendirici.RenkUzunlugu(el, r))
                .ThenByDescending(r => sira[r])
                .ToList();

            foreach (var renk in adaylar)
            {
                int seviye = sira[renk] > sira[rakipKozu] ? 1 : 2;
                if (seviye == 1 && hp >= 8) return "1" + KozYardimcisi.KozSembolu(renk);
                if (seviye == 2 && hp >= 11) return "2" + KozYardimcisi.KozSembolu(renk);
            }

            return "Pas";
        }

        private string EnUzunRengiAc(List<Card> el)
        {
            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            if (macca >= 5) return "1♠";
            if (kupa >= 5) return "1♥";
            if (karo >= 4 && karo >= sinek) return "1♦";
            if (sinek >= 3) return "1♣";
            return "1♣";
        }
    }
}