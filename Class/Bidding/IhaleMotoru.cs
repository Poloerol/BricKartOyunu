using BricKartOyunu.Class.Bidding.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// İhale motoru — tüm konvansiyonları yönetir ve doğru teklifi verir.
    /// 
    /// Çalışma mantığı:
    /// 1. Kayıtlı tüm konvansiyonları önceliğe göre sırala
    /// 2. Her birini sırayla dene:
    ///    - Konvansiyon aktif mi? (Anlaşmaya göre)
    ///    - Bu durumda uygun mu? (UygunMu)
    ///    - Uygunsa → teklifini al ve döndür
    /// 3. Hiçbiri uymadıysa → temel mantık (fallback)
    /// 
    /// Kullanım:
    ///   var motor = new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan());
    ///   string teklif = motor.TeklifVer(durum);
    /// </summary>
    public class IhaleMotoru
    {
        // ═══════════════════════════════════════════════════════════════════
        // ALANLAR
        // ═══════════════════════════════════════════════════════════════════

        private readonly List<IKonvansiyon> _konvansiyonlar;
        private readonly OrtaklikAnlasmasi _anlasma;

        /// <summary>
        /// true ise konvansiyonlardaki istisnalar yutulmaz, yeniden fırlatılır.
        /// Testlerde açın: gizli hatalar (ör. boş listede Last()) sessizce
        /// TemelMantik'e düşüp yanlış teklife dönüşmesin.
        /// Üretimde false kalmalı (oyun çökmesin).
        /// </summary>
        public bool HatalardaFirlat { get; set; } = false;

        // ═══════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Motoru, verilen ortaklık anlaşmasıyla oluşturur.
        /// Tüm bilinen konvansiyonları kaydeder ve anlaşmaya göre aktifleştirir.
        /// </summary>
        public IhaleMotoru(OrtaklikAnlasmasi anlasma)
        {
            _anlasma = anlasma ?? OrtaklikAnlasmasi.Varsayilan();

            _konvansiyonlar = new List<IKonvansiyon>
            {
                // ─── AÇILIŞLAR ───────────────────────────────────────────────
                new IkiliSinekGuclu(),      // 22+ HP yapay 2♣
                new Lebensohl_Reverse(),    // Reverse üzerine Lebensohl
                new Lebensohl_WeakTwo(),    // Zayıf 2'ye karşı Lebensohl
                new Lebensohl(),            // 1NT overcall Lebensohl

                // ─── MAJÖR/MİNÖR AÇILIŞLARI ──────────────────────────────────
                new BesliMajor(),           // 5'li majör
                new ForcingNT(),            // 11 — Forcing NT (1♥/1♠ sonrası 1NT)
                new Stayman(),              // 1NT Stayman
                new BergenRaises(),         // Bergen Raises
                new Cappelletti(),          // 1NT'ye müdahale (Gerber'den önce)
                new Dont(),                 // DONT (1NT'ye müdahale)
                new Gerber(),               // 4♣ As sorusu (NT)
                new PuppetStayman(),        // 2NT Stayman
                new CueBid(),               // kontrol gösterme
                new Blackwood(),            // 4NT As sorusu
                new DopiRopi(),             // Blackwood müdahalesi: Dbl/Rdbl = 0|3, Pas = 1|4
                new JacobyTransfer(),       // majör transfer
                new Splinter(),             // kısa renk
                new Jacoby2NT(),            // majör desteği (13+ HP)
                new SupportDouble(),        // 3'lü destek
                new Michaels(),             // iki renk
                new StrongNT(),             // 15-17 dengeli
                new NegativeDouble(),       // negatif kontr
                new ResponsiveDouble(),     // rakip anlaştı
                new Unusual2NT(),           // iki minör
                new XYZ(),                  // 1X-1Y-1Z sonrası yapay 2♣/2♦
                new MinorTransfer(),        // minör transfer
                new Drury(),                // Pas-1M-2♣
                new TwoWayCheckback(),      // 1X-1Y-1NT sonrası yapay 2♣/2♦
                new Smolen(),               // 5-4 majör
                new TrialBid(),             // 1M-2M sonrası yardım sınaması
                new ThirdSuitForcing(),     // 3. Renk Forsingi
                new GrandSlamForce(),       // 5NT Papaz sorma
                new FourthSuitForcing(),      // 4. Renk Forsingi (2 seviye)
                new MinorAcilis(),          // minör açılış

                // ─── FALLBACK ────────────────────────────────────────────────
                new BasitCevap(),           // basit cevap
            };

            // Anlaşmaya göre aktif/pasif ayarla
            AnlasmayiUygula();

            System.Diagnostics.Debug.WriteLine(
                $"[IhaleMotoru] Oluşturuldu. Aktif konvansiyon sayısı: " +
                $"{_konvansiyonlar.Count(k => k.AktifMi)}");
        }

        // ═══════════════════════════════════════════════════════════════════
        // ANA METOT
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verilen ihale durumu için bir sonraki teklifi döndürür.
        /// </summary>
        public string TeklifVer(IhaleDurumu durum)
        {
            if (durum == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[IhaleMotoru] HATA: durum null!");
                return "Pas";
            }

            // İhale bitti mi?
            if (durum.IhaleBittiMi())
            {
                System.Diagnostics.Debug.WriteLine(
                    "[IhaleMotoru] İhale bitti, Pas dönüyor.");
                return "Pas";
            }

            // Konvansiyonları dene (öncelik sırasına göre)
            foreach (var k in AktifSirali())
            {
                try
                {
                    if (!k.UygunMu(durum)) continue;

                    string teklif = k.TeklifVer(durum);

                    // null → "bu konvansiyon bu eli aslında yönetmiyor", sıradakine geç.
                    // "Pas" ise meşru bir tekliftir ve burada biter.
                    if (teklif == null)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[IhaleMotoru] {k.Ad} uygun dedi ama teklif vermedi (null), sıradakine geçiliyor.");
                        continue;
                    }

                    System.Diagnostics.Debug.WriteLine(
                        $"[IhaleMotoru] {k.Ad} uygulandı → {teklif}");

                    return teklif;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[IhaleMotoru] {k.Ad} hatası: {ex}");

                    if (HatalardaFirlat) throw;
                }
            }

            // Hiçbiri uymadı → temel mantık
            string fallback = TemelMantik(durum);
            System.Diagnostics.Debug.WriteLine(
                $"[IhaleMotoru] Temel mantık → {fallback}");

            return fallback;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEMEL MANTIK (FALLBACK) — GÜÇLENDİRİLMİŞ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Hiçbir konvansiyon uymadığında çalışan temel mantık.
        /// 
        /// Katmanlar:
        /// 1. İlk teklif (açılış)
        /// 2. Partner açtı, ben cevap veriyorum (yeni renk / NT)
        /// 3. Partner açtı, ben destek veriyorum
        /// 4. Rakip açtı, ben müdahale ediyorum
        /// 5. Orta/geç ihale (fallback)
        /// </summary>
        private string TemelMantik(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null || el.Count != 13) return "Pas";

            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

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
            // 2. PARTNER AÇTI, BEN CEVAP VERİYORUM (FORCING)
            // ═══════════════════════════════════════════════════════════════
            if (durum.PartnerActiMi() && durum.KendiTeklifleri.Count == 0)
            {
                return PartnerAcilisinaCevap(durum, el, hp, dengeli);
            }

            // ═══════════════════════════════════════════════════════════════
            // 3. PARTNER AÇTI, BEN DESTEK VERİYORUM
            // ═══════════════════════════════════════════════════════════════
            if (durum.PartnerActiMi())
            {
                string sonPartnerTeklifi = durum.PartnerTeklifleri.Last();
                string partnerKozu = KozYardimcisi.TekliftenKozCikar(sonPartnerTeklifi);

                if (!string.IsNullOrEmpty(partnerKozu) && partnerKozu != "NT")
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 3)
                    {
                        return DestekTeklifi(durum, partnerKozu, hp);
                    }
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // 4. RAKİP AÇTI, BEN MÜDAHALE EDİYORUM
            // ═══════════════════════════════════════════════════════════════
            if (durum.RakipActiMi() && durum.KendiTeklifleri.Count == 0)
            {
                return RakipAcilisinaMudahale(durum, el, hp);
            }

            // ═══════════════════════════════════════════════════════════════
            // 5. ORTA/GEÇ İHALE — FALLBACK
            // ═══════════════════════════════════════════════════════════════
            if (durum.PartnerActiMi())
            {
                string sonPartnerTeklifi = durum.PartnerTeklifleri.Last();
                string partnerKozu = KozYardimcisi.TekliftenKozCikar(sonPartnerTeklifi);

                if (!string.IsNullOrEmpty(partnerKozu) && partnerKozu != "NT")
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 3 && hp >= 6)
                    {
                        return DestekTeklifi(durum, partnerKozu, hp);
                    }
                }
            }

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI: PARTNER AÇILIŞINA CEVAP
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Partner açtı (1♠/1♥/1♦/1♣/1NT), ben ilk kez konuşuyorum.
        /// </summary>
        private string PartnerAcilisinaCevap(IhaleDurumu durum, List<Card> el, int hp, bool dengeli)
        {
            string partnerTeklifi = durum.PartnerTeklifleri.Last();
            string partnerKozu = KozYardimcisi.TekliftenKozCikar(partnerTeklifi);

            // ─── Partner 1NT açtı ───────────────────────────────────────
            if (partnerTeklifi == "1NT")
            {
                if (hp >= 10 && dengeli) return "3NT";
                if (hp >= 8 && hp <= 9 && dengeli) return "2NT";
                return "Pas";
            }

            // ─── Partner 2NT açtı ───────────────────────────────────────
            if (partnerTeklifi == "2NT")
            {
                if (hp >= 5) return "3NT";
                return "Pas";
            }

            // ─── Partner 1 seviyesinde renk açtı ────────────────────────
            if (partnerKozu != null && partnerKozu != "NT")
            {
                if (hp < 6) return "Pas";

                // Majör desteği ara
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

                // Yeni renk teklif et
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

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI: RAKİP AÇILIŞINA MÜDAHALE
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Rakip açtı, ben ilk kez konuşuyorum.
        /// </summary>
        private string RakipAcilisinaMudahale(IhaleDurumu durum, List<Card> el, int hp)
        {
            if (hp < 8) return "Pas";

            string sonRakipTeklifi = durum.RakipTeklifleri.Last();
            string rakipKozu = KozYardimcisi.TekliftenKozCikar(sonRakipTeklifi);

            if (rakipKozu != "Maça" && ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 5)
                return "1♠";
            if (rakipKozu != "Kupa" && ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 5)
                return "1♥";
            if (rakipKozu != "Karo" && ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 5)
                return "2♦";
            if (rakipKozu != "Sinek" && ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 5)
                return "2♣";

            return "Pas";
        }

        /// <summary>
        /// En uzun rengi açar (5'li majör önceliği ile).
        /// </summary>
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
            if (karo >= 3) return "1♦";

            return "1♣";
        }

        /// <summary>
        /// Partner teklifine destek teklifi döndürür.
        /// </summary>
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

            // 1. Game forcing el (13+ HP + 4+ destek)
            if (hp >= 13 && destek >= 4)
            {
                if (koz == "Maça" || koz == "Kupa")
                    return $"4{sembol}";
                if (hp >= 15) return $"5{sembol}";
                return $"3{sembol}";
            }

            // 2. Limit raise (10-12 HP + 4+ destek)
            if (hp >= 10 && destek >= 4)
            {
                int hedef = 3;
                if (hedef <= partnerSeviye) hedef = partnerSeviye + 1;
                if (hedef > 4) return "Pas";
                return $"{hedef}{sembol}";
            }

            // 3. Basit destek (6-9 HP)
            if (hp >= 6 && hp <= 9)
            {
                int hedef = 2;
                if (hedef <= partnerSeviye) return "Pas";
                return $"{hedef}{sembol}";
            }

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // ANLAŞMA YÖNETİMİ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Anlaşmadaki ayarlara göre konvansiyonları aktif/pasif yapar.
        /// </summary>
        private void AnlasmayiUygula()
        {
            foreach (var k in _konvansiyonlar)
            {
                k.AktifMi = AnlasmayaGoreAktifMi(k.Ad);
            }

            // Cappelletti ve DONT aynı anda aktif olamaz (ikisi de 1NT'ye müdahale eder).
            var cappelletti = _konvansiyonlar.FirstOrDefault(k => k.Ad == "Cappelletti");
            var dont = _konvansiyonlar.FirstOrDefault(k => k.Ad == "DONT");
            if (cappelletti != null && dont != null && cappelletti.AktifMi && dont.AktifMi)
            {
                dont.AktifMi = false;
                System.Diagnostics.Debug.WriteLine(
                    "[IhaleMotoru] UYARI: Cappelletti ve DONT birlikte aktifti, DONT kapatıldı.");
            }
        }

        /// <summary>
        /// Belirtilen konvansiyon, anlaşmaya göre aktif mi?
        /// </summary>
        private bool AnlasmayaGoreAktifMi(string konvansiyonAdi)
        {
            switch (konvansiyonAdi)
            {
                // Açılışlar
                case "5'li Majör": return _anlasma.BesliMajor;
                case "4'lü Majör": return !_anlasma.BesliMajor;
                case "Strong NT": return _anlasma.StrongNT;
                case "Weak NT": return !_anlasma.StrongNT;
                case "2♣ Güçlü": return _anlasma.IkiliSinekGuclu;
                case "Zayıf 2": return _anlasma.ZayifIki;
                case "Preempt": return _anlasma.Preempt;

                // 1NT Cevapları
                case "Forcing NT": return _anlasma.ForcingNT;
                case "Stayman": return _anlasma.Stayman;
                case "Jacoby Transfer": return _anlasma.JacobyTransfer;
                case "Puppet Stayman": return _anlasma.PuppetStayman;
                case "Smolen": return _anlasma.Smolen;
                case "Minor Transfer": return _anlasma.MinorTransfer;

                // 1NT rebid sonrası
                case "İki Yönlü Checkback": return _anlasma.TwoWayCheckback;

                // XYZ (1X-1Y-1Z)
                case "XYZ (X-Y-Z)": return _anlasma.XYZ;

                // Slam
                case "Blackwood": return _anlasma.Blackwood;
                case "RKCB": return _anlasma.RKCB;
                case "Gerber": return _anlasma.Gerber;
                case "Grand Slam Force": return true;

                // Ortaklık
                case "Jacoby 2NT": return _anlasma.Jacoby2NT;
                case "Splinter": return _anlasma.Splinter;
                case "Drury": return _anlasma.Drury;
                case "Reverse Drury": return _anlasma.ReverseDrury;
                case "Cue Bid": return _anlasma.CueBid;
                case "Bergen Raises": return _anlasma.BergenRaises;
                case "Trial Bid": return _anlasma.TrialBid;
                case "3. Renk Forsingi": return _anlasma.ThirdSuitForcing;
                case "4. Renk Forsingi": return _anlasma.FourthSuitForcing;

                // Rakip müdahalesi
                case "Negative Double": return _anlasma.NegativeDouble;
                case "Support Double": return _anlasma.SupportDouble;
                case "Responsive Double": return _anlasma.ResponsiveDouble;
                case "Lebensohl (Reverse)": return _anlasma.Lebensohl;
                case "Lebensohl (Zayıf 2)": return _anlasma.Lebensohl;
                case "Lebensohl": return _anlasma.Lebensohl;
                case "Michaels": return _anlasma.Michaels;
                case "Unusual 2NT": return _anlasma.Unusual2NT;

                // 1NT müdahale sistemi (Cappelletti ve DONT aynı anda aktif olamaz)
                case "Cappelletti": return _anlasma.Cappelletti;
                case "DONT": return _anlasma.Dont;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Anlaşmayı değiştirir ve konvansiyonları yeniden aktifleştirir.
        /// </summary>
        public void AnlasmayiDegistir(OrtaklikAnlasmasi yeniAnlasma)
        {
            if (yeniAnlasma == null) return;

            System.Diagnostics.Debug.WriteLine(
                "[IhaleMotoru] Anlaşma değişikliği için yeni motor oluşturun.");

            throw new NotSupportedException(
                "Anlaşma değişikliği için IhaleMotoru'nu yeniden oluşturun.");
        }

        // ═══════════════════════════════════════════════════════════════════
        // BİLGİ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aktif konvansiyonların listesini döndürür.
        /// </summary>
        public List<string> AktifKonvansiyonlar()
        {
            return _konvansiyonlar
                .Where(k => k.AktifMi)
                .Select(k => k.Ad)
                .ToList();
        }

        /// <summary>
        /// Tüm konvansiyonların listesini döndürür.
        /// </summary>
        public List<string> TumKonvansiyonlar()
        {
            return _konvansiyonlar
                .Select(k => k.Ad)
                .ToList();
        }

        /// <summary>
        /// Aktif konvansiyonları öncelik sırasına göre döndürür.
        /// Eşit önceliklerde kayıt (constructor) sırası belirleyicidir.
        /// </summary>
        private List<IKonvansiyon> AktifSirali()
        {
            return _konvansiyonlar
                .Where(k => k.AktifMi)
                .OrderBy(k => k.Oncelik)
                .ThenBy(k => _konvansiyonlar.IndexOf(k))
                .ToList();
        }

        /// <summary>
        /// Verilen durumda UygunMu() == true dönen TÜM aktif konvansiyonlar (öncelik sırasıyla).
        /// Birden fazla varsa çakışma vardır; ilk sıradaki kazanır.
        /// </summary>
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

        /// <summary>
        /// Çakışma raporu: bu durumda hangi konvansiyonlar uygun, hangisi kazanır.
        /// Testlerde Yaz(motor.CakismaRaporu(durum)) ile kullanın.
        /// </summary>
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

        /// <summary>
        /// Aynı öncelik numarasını paylaşan aktif konvansiyon grupları.
        /// Sıra bu durumda yalnızca kayıt sırasına bağlıdır.
        /// </summary>
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

        /// <summary>
        /// Debug için özet.
        /// </summary>
        public string Ozet()
        {
            var aktifler = AktifKonvansiyonlar();
            return $"IhaleMotoru: {aktifler.Count} aktif konvansiyon " +
                   $"({string.Join(", ", aktifler)})";
        }
    }
}