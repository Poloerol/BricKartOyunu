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
    new IkiliSinekGuclu(),
    new BesliMajor(),
    new StrongNT(),
    new MinorAcilis(),

    new Gerber(),
    new CueBid(),
    new Blackwood(),
    new GrandSlamForce(),     // ← YENİ (öncelik 28)

    new Splinter(),
    new Jacoby2NT(),
    new SupportDouble(),

    new Michaels(),
    new NegativeDouble(),
    new ResponsiveDouble(),
    new Unusual2NT(),

    new MinorTransfer(),
    new Drury(),
    new Smolen(),
    new PuppetStayman(),      // ← YENİ (öncelik 27)
    new Lebensohl_Reverse(),   // ← YENİ (öncelik 6)
    new Lebensohl_WeakTwo(),   // ← YENİ (öncelik 7)
    new Lebensohl(),

    new Stayman(),
    new JacobyTransfer(),
    new BasitCevap(),
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
        /// 
        /// Sıra:
        /// 1. İhale bittiyse → "Pas"
        /// 2. Aktif konvansiyonları sırayla dene (önceliğe göre)
        /// 3. Hiçbiri uymazsa → temel mantık
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

            // Konvansiyonları dene
            var aktifler = _konvansiyonlar
                .Where(k => k.AktifMi)
                .OrderBy(k => k.Oncelik)
                .ToList();

            foreach (var k in aktifler)
            {
                try
                {
                    if (k.UygunMu(durum))
                    {
                        string teklif = k.TeklifVer(durum);

                        System.Diagnostics.Debug.WriteLine(
                            $"[IhaleMotoru] {k.Ad} uygulandı → {teklif}");

                        return teklif;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[IhaleMotoru] {k.Ad} hatası: {ex.Message}");
                    // Bir sonraki konvansiyona geç
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
                // 22+ HP → 2♣ (yapay güçlü)
                if (hp >= 22) return "2♣";

                // 20-21 HP dengeli → 2NT
                if (hp >= 20 && hp <= 21 && dengeli) return "2NT";

                // 15-17 HP dengeli → 1NT
                if (hp >= 15 && hp <= 17 && dengeli) return "1NT";

                // 12+ HP → en uzun rengi aç
                if (hp >= 12) return EnUzunRengiAc(el);

                // 12 HP altı → Pas
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
                string partnerKozu = TekliftenKozCikar(sonPartnerTeklifi);

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
            // Destek varsa destek ver
            if (durum.PartnerActiMi())
            {
                string sonPartnerTeklifi = durum.PartnerTeklifleri.Last();
                string partnerKozu = TekliftenKozCikar(sonPartnerTeklifi);

                if (!string.IsNullOrEmpty(partnerKozu) && partnerKozu != "NT")
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 3 && hp >= 6)
                    {
                        return DestekTeklifi(durum, partnerKozu, hp);
                    }
                }
            }

            // Aksi halde Pas
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
            string partnerKozu = TekliftenKozCikar(partnerTeklifi);

            // ─── Partner 1NT açtı ───────────────────────────────────────
            if (partnerTeklifi == "1NT")
            {
                // 10+ HP dengeli → 3NT (game)
                if (hp >= 10 && dengeli) return "3NT";
                // 8-9 HP dengeli → 2NT (invite)
                if (hp >= 8 && hp <= 9 && dengeli) return "2NT";
                // Zayıf → Pas
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
                // 1-5 HP → Pas
                if (hp < 6) return "Pas";

                // Majör desteği ara
                if (partnerKozu == "Maça" || partnerKozu == "Kupa")
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 4)
                    {
                        // 4'lü majör desteği
                        if (hp >= 13) return "2NT";       // Jacoby 2NT
                        if (hp >= 10 && hp <= 12) return "3" + KozSembolu(partnerKozu); // Limit raise
                        if (hp >= 6 && hp <= 9) return "2" + KozSembolu(partnerKozu);   // Basit destek
                    }
                    else if (destek == 3)
                    {
                        // 3'lü majör desteği
                        if (hp >= 10 && hp <= 12) return "2" + KozSembolu(partnerKozu);
                        if (hp >= 6 && hp <= 9) return "2" + KozSembolu(partnerKozu);
                    }
                }

                // Yeni renk teklif et (6+ HP)
                // Majör varsa önce onu göster
                if (partnerKozu != "Maça" && ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4)
                    return "1♠";
                if (partnerKozu != "Kupa" && ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4)
                    return "1♥";

                // 5+ minör
                if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 5 && partnerKozu != "Karo")
                    return "2♦";
                if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 5 && partnerKozu != "Sinek")
                    return "2♣";

                // Dengeli ve 10-12 HP → 2NT (invite)
                if (dengeli && hp >= 10 && hp <= 12) return "2NT";

                // Dengeli ve 13-15 HP → 3NT (game)
                if (dengeli && hp >= 13 && hp <= 15) return "3NT";

                // Fallback: minör varsa göster
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
        /// Basit müdahale: 5+ renk + 8+ HP → o rengi teklif et.
        /// </summary>
        private string RakipAcilisinaMudahale(IhaleDurumu durum, List<Card> el, int hp)
        {
            if (hp < 8) return "Pas";

            string sonRakipTeklifi = durum.RakipTeklifleri.Last();
            string rakipKozu = TekliftenKozCikar(sonRakipTeklifi);

            // Sırayla dene: uzun majör önce, sonra minör
            if (rakipKozu != "Maça" && ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 5)
                return "1♠";
            if (rakipKozu != "Kupa" && ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 5)
                return "1♥";
            if (rakipKozu != "Karo" && ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 5)
                return "2♦";
            if (rakipKozu != "Sinek" && ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 5)
                return "2♣";

            // Müdahale edecek 5'li renk yok
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

            // 5'li majör öncelik
            if (macca >= 5) return "1♠";
            if (kupa >= 5) return "1♥";

            // 4-4 minör → karo (daha kuvvetli)
            if (karo >= 4 && karo >= sinek) return "1♦";
            if (sinek >= 3) return "1♣";
            if (karo >= 3) return "1♦";

            // Varsayılan
            return "1♣";
        }

        /// <summary>
        /// Partner teklifine destek teklifi döndürür.
        /// </summary>
        private string DestekTeklifi(IhaleDurumu durum, string koz, int hp)
        {
            var el = durum.AktifOyuncuEli;
            int destek = ElDegerlendirici.RenkUzunlugu(el, koz);
            string sembol = KozSembolu(koz);

            // Partnerin son teklifinin seviyesi
            int partnerSeviye = 1;
            var partnerTeklifleri = durum.PartnerTeklifleri;
            if (partnerTeklifleri.Count > 0)
            {
                string son = partnerTeklifleri.Last();
                if (son.Length > 0 && char.IsDigit(son[0]))
                    partnerSeviye = son[0] - '0';
            }

            // ═══════════════════════════════════════════════════════════════
            // DESTEK SEVİYESİ BELİRLEME
            // ═══════════════════════════════════════════════════════════════

            // 1. Game forcing el (13+ HP + 4+ destek) → game seviyesi
            if (hp >= 13 && destek >= 4)
            {
                if (koz == "Maça" || koz == "Kupa")
                    return $"4{sembol}";  // Majör game
                                          // Minör game 5 seviyesi (nadir)
                if (hp >= 15) return $"5{sembol}";
                return $"3{sembol}";  // 3NT denemesi
            }

            // 2. Limit raise (10-12 HP + 4+ destek) → 3 seviyesi
            if (hp >= 10 && destek >= 4)
            {
                int hedef = 3;
                if (hedef <= partnerSeviye) hedef = partnerSeviye + 1;
                if (hedef > 4) return "Pas";  // Çok yüksek → Pas
                return $"{hedef}{sembol}";
            }

            // 3. Basit destek (6-9 HP) → 2 seviyesi
            if (hp >= 6 && hp <= 9)
            {
                int hedef = 2;
                // Partner zaten 2 demişse → 3'e çıkmak yerine Pas
                if (hedef <= partnerSeviye) return "Pas";
                return $"{hedef}{sembol}";
            }

            // 4. Çok zayıf (< 6 HP) → Pas
            return "Pas";
        }

        /// <summary>
        /// Teklif metninden kozu çıkarır.
        /// "1♠" → "Maça"
        /// </summary>
        private string TekliftenKozCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif)) return null;

            char sonKarakter = teklif[teklif.Length - 1];
            switch (sonKarakter)
            {
                case '♠': return "Maça";
                case '♥': return "Kupa";
                case '♦': return "Karo";
                case '♣': return "Sinek";
                case 'T':
                case 't': return "NT";
                default: return null;
            }
        }

        /// <summary>
        /// Koz adını sembole çevirir.
        /// "Maça" → "♠"
        /// </summary>
        private string KozSembolu(string koz)
        {
            switch (koz)
            {
                case "Maça": return "♠";
                case "Kupa": return "♥";
                case "Karo": return "♦";
                case "Sinek": return "♣";
                case "NT": return "NT";
                default: return "?";
            }
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
                case "Stayman": return _anlasma.Stayman;
                case "Jacoby Transfer": return _anlasma.JacobyTransfer;
                case "Puppet Stayman": return _anlasma.PuppetStayman;
                case "Smolen": return _anlasma.Smolen;
                case "Minor Transfer": return _anlasma.MinorTransfer;

                // Slam
                case "Blackwood": return _anlasma.Blackwood;
                case "RKCB": return _anlasma.RKCB;
                case "Gerber": return _anlasma.Gerber;
                case "Grand Slam Force": return true;   // ← YENİ (anlaşmada bayrak yok, her zaman aktif)

                // Ortaklık
                case "Jacoby 2NT": return _anlasma.Jacoby2NT;
                case "Splinter": return _anlasma.Splinter;
                case "Drury": return _anlasma.Drury;
                case "Reverse Drury": return _anlasma.ReverseDrury;
                case "Cue Bid": return _anlasma.CueBid;

                // Rakip müdahalesi
                case "Negative Double": return _anlasma.NegativeDouble;
                case "Support Double": return _anlasma.SupportDouble;
                case "Responsive Double": return _anlasma.ResponsiveDouble;
                case "Lebensohl (Reverse)": return _anlasma.Lebensohl;
                case "Lebensohl (Zayıf 2)": return _anlasma.Lebensohl;
                case "Lebensohl": return _anlasma.Lebensohl;
                case "Michaels": return _anlasma.Michaels;
                case "Unusual 2NT": return _anlasma.Unusual2NT;

                default:
                    // Bilinmeyen konvansiyon → varsayılan aktif
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