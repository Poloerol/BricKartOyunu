using BricKartOyunu.Class.Bidding.Conventions;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    public class IhaleMotoru
    {
        private readonly List<IAcilisGelisimi> _acilisGelismleri;
        private readonly List<IKonvansiyon> _konvansiyonlar;
        private readonly OrtaklikAnlasmasi _anlasma;

        public bool HatalardaFirlat { get; set; } = false;

        public IhaleMotoru(OrtaklikAnlasmasi anlasma)
        {
            _anlasma = anlasma ?? OrtaklikAnlasmasi.Varsayilan();

            _konvansiyonlar = new List<IKonvansiyon>
            {
                // ─── AÇILIŞLAR ───────────────────────────────────────────────
                new IkiliSinekGuclu(),
                new ZayifIki(),
                new Multi2D(),
                new Namyats(),
                new Preempt(),
                new Lebensohl_Reverse(),
                new Lebensohl_WeakTwo(),
                new Lebensohl(),

                // ─── MAJÖR/MİNÖR AÇILIŞLARI ──────────────────────────────────
                new BesliMajor(),
                new InvertedMinors(),
                new ForcingNT(),
                new Stayman(),
                new BergenRaises(),
                new Cappelletti(),
                new Dont(),
                new MultiLandy(),
                new Gerber(),
                new PuppetStayman(),
                new CueBid(),
                new KozQSorusu(),
                new Kickback(),
                new ExclusionBlackwood(),
                new RKCB(),
                new Blackwood(),
                new DopiRopi(),
                new JacobyTransfer(),
                new Splinter(),
                new Jacoby2NT(),
                new SupportDouble(),
                new Michaels(),
                new StrongNT(),
                new NegativeDouble(),
                new ResponsiveDouble(),
                new Unusual2NT(),
                new TakeoutDouble(),
                new XYZ(),
                new MinorTransfer(),
                new Serious3NT(),
                new NonSerious3NT(),
                new Drury(),
                new TwoWayCheckback(),
                new PickASlam(),
                new Smolen(),
                new TrialBid(),
                new ThirdSuitForcing(),
                new GrandSlamForce(),
                new Reverse(),
                new FourthSuitForcing(),
                

                // ─── 1♣ CEVAP SİSTEMİ ──────────────────────────────────────
                new OneClub_Answer(),

                // ─── FALLBACK ────────────────────────────────────────────────
                new BasitCevap(),
            };

            _acilisGelismleri = new List<IAcilisGelisimi>
{
       new OneClub_Opening(),           // ← YENİ
    new OneDiamond_Opening(),        // ← YENİ
    new OneClub_OneDiamond(),
    new OneClub_OneDiamond_OneHeart(),
};
            AnlasmayiUygula();

            System.Diagnostics.Debug.WriteLine(
                $"[IhaleMotoru] Oluşturuldu. Aktif konvansiyon sayısı: " +
                $"{_konvansiyonlar.Count(k => k.AktifMi)}");
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            if (durum == null) return "Pas";
            if (durum.AktifOyuncuEli != null)
            {
                foreach (var gelisim in _acilisGelismleri.OrderByDescending(g => g.Oncelik))
                {
                    try
                    {
                        if (!gelisim.UygunMu(durum.AktifOyuncuEli, durum)) continue;

                        string yeniTeklif = gelisim.TeklifVer(durum.AktifOyuncuEli, durum);
                        if (string.IsNullOrEmpty(yeniTeklif)) continue;

                        string yasalYeni = YasalSeviyeyeYukselt(durum, yeniTeklif);
                        if (!GecerliTeklifMi(durum, yasalYeni)) continue;

                        System.Diagnostics.Debug.WriteLine(
                            $"[KAZANAN-AcilisGelismi] {gelisim.GetType().Name} → {yasalYeni}");
                        return yasalYeni;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[AcilisGelismi] {gelisim.GetType().Name} hata: {ex.Message}");
                        if (HatalardaFirlat) throw;
                    }
                }
            }

            foreach (var konvansiyon in _konvansiyonlar.OrderBy(k => k.Oncelik))
            {
                if (!konvansiyon.AktifMi) continue;
                if (!konvansiyon.UygunMu(durum)) continue;

                string teklif = konvansiyon.TeklifVer(durum);
                if (string.IsNullOrEmpty(teklif)) continue;

                string yasal = YasalSeviyeyeYukselt(durum, teklif);
                if (!GecerliTeklifMi(durum, yasal))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[IhaleMotoru] {konvansiyon.Ad} yasadışı teklif reddedildi: {teklif}");
                    continue;
                }

                System.Diagnostics.Debug.WriteLine(
                    $"[KAZANAN] {konvansiyon.Ad} (Oncelik={konvansiyon.Oncelik}) → {yasal}");

                if (yasal != teklif)
                    System.Diagnostics.Debug.WriteLine(
                        $"[IhaleMotoru] {konvansiyon.Ad} seviye yükseltildi: {teklif} → {yasal}");

                return yasal;
            }

            System.Diagnostics.Debug.WriteLine("[KAZANAN] TemelMantik (fallback)");

            string temel = TemelMantik(durum);
            string temelYasal = YasalSeviyeyeYukselt(durum, temel);
            if (!GecerliTeklifMi(durum, temelYasal))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[IhaleMotoru] TemelMantik yasadışı teklif reddedildi: {temel} → Pas");
                return "Pas";
            }
            return temelYasal;
        }

        private static bool GecerliTeklifMi(IhaleDurumu durum, string teklif)
        {
            System.Diagnostics.Debug.WriteLine(
        $"[GecerliTeklifMi] teklif={teklif}, SonGercekTeklif={durum?.SonGercekTeklif}");

            if (string.IsNullOrEmpty(teklif)) return false;

            string t = teklif.Trim();
            if (t.Equals("Pas", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.Equals("Dbl", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("Kontr", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.Equals("Rdbl", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("RDbl", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("S.Kontr", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("SKontr", StringComparison.OrdinalIgnoreCase)) return true;

            string son = durum?.SonGercekTeklif;
            if (string.IsNullOrEmpty(son)) return true;

            int yeniSev = KozYardimcisi.TekliftenSeviyeCikar(t);
            string yeniKoz = KozYardimcisi.TekliftenKozCikar(t);
            int sonSev = KozYardimcisi.TekliftenSeviyeCikar(son);
            string sonKoz = KozYardimcisi.TekliftenKozCikar(son);

            if (yeniSev <= 0 || string.IsNullOrEmpty(yeniKoz)) return false;
            if (yeniSev > sonSev) return true;
            if (yeniSev < sonSev) return false;

            return KozSirasi(yeniKoz) > KozSirasi(sonKoz);
        }

        private static string YasalSeviyeyeYukselt(IhaleDurumu durum, string teklif)
        {
            if (string.IsNullOrEmpty(teklif)) return teklif;
            if (GecerliTeklifMi(durum, teklif)) return teklif;

            string t = teklif.Trim();
            if (t.Equals("Pas", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("Dbl", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("Kontr", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("Rdbl", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("RDbl", StringComparison.OrdinalIgnoreCase) ||
                t.Equals("S.Kontr", StringComparison.OrdinalIgnoreCase))
                return t;

            int sev = KozYardimcisi.TekliftenSeviyeCikar(t);
            string koz = KozYardimcisi.TekliftenKozCikar(t);
            if (sev <= 0 || string.IsNullOrEmpty(koz)) return teklif;

            for (int s = sev + 1; s <= 7; s++)
            {
                string aday = KozYardimcisi.TeklifOlustur(s, koz);
                if (GecerliTeklifMi(durum, aday))
                    return aday;
            }
            return teklif;
        }

        private static int KozSirasi(string koz)
        {
            if (string.IsNullOrEmpty(koz)) return -1;
            switch (koz)
            {
                case "Sinek": case "♣": return 0;
                case "Karo": case "♦": return 1;
                case "Kupa": case "♥": return 2;
                case "Maça": case "♠": return 3;
                case "NT": return 4;
                default: return -1;
            }
        }

        private string TemelMantik(IhaleDurumu durum)
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
    // 4. AÇICI REBID — KRİTİK DEĞİŞİKLİK BURADA
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
            // Partner bir renk gösterdiyse ve bende 3+ destek varsa, 
            // fit vardır. NT veya başka şey düşünme, DESTEK VER!
            if (!string.IsNullOrEmpty(partnerKozu) && partnerKozu != "NT")
            {
                int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                if (destek >= 3)
                {
                    // Partnerin 1♥/1♠/1♦/1♣'ine destek
                    if (partnerSeviye == 1)
                    {
                                // Minimum fit → 2 seviyesinde destek
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
                                // Partner 2 seviyesinde gösterdiyse (ör. 2♥), 3 seviyesinde destek
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

            // Yeni 5'li renk gösterme
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
        System.Diagnostics.Debug.WriteLine($"[TemelMantik] İhale çok yükseldi (Seviye {guvenlikSonSeviye}, HP {hp}) → Pas");
        return "Pas";
    }

    return "Pas";
}
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

        private string DestekTeklifi(IhaleDurumu durum, string koz, int hp)
        {
            var el = durum.AktifOyuncuEli;
            int destek = ElDegerlendirici.RenkUzunlugu(el, koz);
            string sembol = KozYardimcisi.KozSembolu(koz);

            int partnerSeviye = 1;
            var partnerTeklifleri = durum.PartnerTeklifleri;
            if (partnerTeklifleri.Count > 0)
            {
                string son = partnerTeklifleri.Last();
                if (son.Length > 0 && char.IsDigit(son[0]))
                    partnerSeviye = son[0] - '0';
            }

            if (hp >= 13 && destek >= 4)
            {
                if (koz == "Maça" || koz == "Kupa")
                    return $"4{sembol}";
                if (hp >= 15) return $"5{sembol}";
                return $"3{sembol}";
            }

            if (hp >= 10 && destek >= 4)
            {
                int hedef = 3;
                if (hedef <= partnerSeviye) hedef = partnerSeviye + 1;
                if (hedef > 4) return "Pas";
                return $"{hedef}{sembol}";
            }

            if (hp >= 6 && hp <= 9)
            {
                int hedef = 2;
                if (hedef <= partnerSeviye) return "Pas";
                return $"{hedef}{sembol}";
            }

            return "Pas";
        }

        private void AnlasmayiUygula()
        {
            foreach (var k in _konvansiyonlar)
            {
                k.AktifMi = AnlasmayaGoreAktifMi(k.Ad);
            }

            var cappelletti = _konvansiyonlar.FirstOrDefault(k => k.Ad == "Cappelletti");
            var dont = _konvansiyonlar.FirstOrDefault(k => k.Ad == "DONT");
            if (cappelletti != null && dont != null && cappelletti.AktifMi && dont.AktifMi)
            {
                dont.AktifMi = false;
                System.Diagnostics.Debug.WriteLine(
                    "[IhaleMotoru] UYARI: Cappelletti ve DONT birlikte aktifti, DONT kapatıldı.");
            }
        }

        private bool AnlasmayaGoreAktifMi(string konvansiyonAdi)
        {
            switch (konvansiyonAdi)
            {
                case "5'li Majör": return _anlasma.BesliMajor;
                case "4'lü Majör": return !_anlasma.BesliMajor;
                case "Strong NT": return _anlasma.StrongNT;
                case "Weak NT": return !_anlasma.StrongNT;
                case "2♣ Güçlü": return _anlasma.IkiliSinekGuclu;
                case "Zayıf İki": return _anlasma.ZayifIki && !_anlasma.Multi2D;
                case "Multi 2♦": return _anlasma.Multi2D;
                case "Preempt": return _anlasma.Preempt;
                case "Namyats": return _anlasma.Namyats;

                case "1♣ Cevap Sistemi": return _anlasma.OneClub_AnswerSistemi;
                case "1♣-1♦ Gelişimi": return _anlasma.OneClub_AnswerSistemi;
                case "1♣-1♦-1♥ Gelişimi": return _anlasma.OneClub_AnswerSistemi;

                case "Inverted Minors": return _anlasma.InvertedMinors;
                case "Forcing NT": return _anlasma.ForcingNT;
                case "Stayman": return _anlasma.Stayman;
                case "Jacoby Transfer": return _anlasma.JacobyTransfer;
                case "Puppet Stayman": return _anlasma.PuppetStayman;
                case "Smolen": return _anlasma.Smolen;
                case "Minor Transfer": return _anlasma.MinorTransfer;

                case "İki Yönlü Checkback": return _anlasma.TwoWayCheckback;
                case "XYZ (X-Y-Z)": return _anlasma.XYZ;

                case "Koz Q Sorusu": return _anlasma.RKCB;
                case "Kickback": return _anlasma.RKCB;
                case "Exclusion Blackwood": return _anlasma.RKCB;
                case "Blackwood": return _anlasma.Blackwood;
                case "RKCB": return _anlasma.RKCB;
                case "Gerber": return _anlasma.Gerber;
                case "Pick-a-Slam": return _anlasma.RKCB;
                case "Grand Slam Force": return true;

                case "Jacoby 2NT": return _anlasma.Jacoby2NT;
                case "Splinter": return _anlasma.Splinter;
                case "Drury": return _anlasma.Drury;
                case "Reverse Drury": return _anlasma.ReverseDrury;
                case "Cue Bid": return _anlasma.CueBid;
                case "Bergen Raises": return _anlasma.BergenRaises;
                case "Trial Bid": return _anlasma.TrialBid;
                case "3. Renk Forsingi": return _anlasma.ThirdSuitForcing;
                case "4. Renk Forsingi": return _anlasma.FourthSuitForcing;
                case "Reverse": return _anlasma.Reverse;

                case "Negative Double": return _anlasma.NegativeDouble;
                case "Takeout Double": return _anlasma.TakeoutDouble;
                case "Support Double": return _anlasma.SupportDouble;
                case "Responsive Double": return _anlasma.ResponsiveDouble;
                case "Lebensohl (Reverse)": return _anlasma.Lebensohl;
                case "Lebensohl (Zayıf 2)": return _anlasma.Lebensohl;
                case "Lebensohl": return _anlasma.Lebensohl;
                case "Michaels": return _anlasma.Michaels;
                case "Unusual 2NT": return _anlasma.Unusual2NT;

                case "Cappelletti": return _anlasma.Cappelletti;
                case "DONT": return _anlasma.Dont;
                case "Multi-Landy": return _anlasma.MultiLandy;

                default:
                    return true;
            }
        }

        public void AnlasmayiDegistir(OrtaklikAnlasmasi yeniAnlasma)
        {
            if (yeniAnlasma == null) return;
            throw new NotSupportedException(
                "Anlaşma değişikliği için IhaleMotoru'nu yeniden oluşturun.");
        }

        public List<string> AktifKonvansiyonlar()
        {
            return _konvansiyonlar
                .Where(k => k.AktifMi)
                .Select(k => k.Ad)
                .ToList();
        }

        public List<string> TumKonvansiyonlar()
        {
            return _konvansiyonlar
                .Select(k => k.Ad)
                .ToList();
        }

        private List<IKonvansiyon> AktifSirali()
        {
            return _konvansiyonlar
                .Where(k => k.AktifMi)
                .OrderBy(k => k.Oncelik)
                .ThenBy(k => _konvansiyonlar.IndexOf(k))
                .ToList();
        }

        public List<IKonvansiyon> UygunKonvansiyonlar(IhaleDurumu durum)
        {
            var sonuc = new List<IKonvansiyon>();
            if (durum == null) return sonuc;

            foreach (var k in AktifSirali())
            {
                try
                {
                    if (k.UygunMu(durum)) sonuc.Add(k);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[IhaleMotoru] {k.Ad} UygunMu hatası: {ex}");
                    if (HatalardaFirlat) throw;
                }
            }
            return sonuc;
        }

        public string CakismaRaporu(IhaleDurumu durum)
        {
            var uygunlar = UygunKonvansiyonlar(durum);
            if (uygunlar.Count == 0)
                return "Uygun konvansiyon yok → TemelMantik";

            string baslik = uygunlar.Count > 1
                ? $"ÇAKIŞMA: {uygunlar.Count} konvansiyon uygun (ilk satır kazanır)"
                : "Tek konvansiyon uygun";

            var satirlar = uygunlar.Select((k, i) =>
                $"{(i == 0 ? "▶" : " ")} [{k.Oncelik,3}] {k.Ad}");

            return baslik + Environment.NewLine
                 + string.Join(Environment.NewLine, satirlar);
        }

        public List<string> OncelikCakismalari()
        {
            return _konvansiyonlar
                .Where(k => k.AktifMi)
                .GroupBy(k => k.Oncelik)
                .Where(g => g.Count() > 1)
                .OrderBy(g => g.Key)
                .Select(g => $"[{g.Key}] " + string.Join(", ", g.Select(k => k.Ad)))
                .ToList();
        }

        public string Ozet()
        {
            var aktifler = AktifKonvansiyonlar();
            return $"IhaleMotoru: {aktifler.Count} aktif konvansiyon " +
                   $"({string.Join(", ", aktifler)})";
        }
    }
}