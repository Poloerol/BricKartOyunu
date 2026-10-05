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

            // Kayıtlı tüm konvansiyonlar
            _konvansiyonlar = new List<IKonvansiyon>
{
    // Açılış konvansiyonları (öncelik sırasına göre)
    new BesliMajor(),       // Öncelik 10
    new StrongNT(),         // Öncelik 20
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
        // TEMEL MANTIK (FALLBACK)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Hiçbir konvansiyon uymadığında çalışan basit mantık.
        /// 
        /// Kurallar:
        /// - 12+ HP → en uzun rengi aç (1♠/1♥/1♦/1♣)
        /// - 15-17 HP dengeli → 1NT
        /// - 20+ HP → 2♣ (yapay güçlü)
        /// - Aksi halde → Pas
        /// </summary>
        private string TemelMantik(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null || el.Count != 13)
            {
                // El bilinmiyor veya eksik → güvenli taraf: Pas
                return "Pas";
            }

            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // 1. İlk teklif mi?
            if (durum.IlkTeklifMi())
            {
                // 22+ HP → 2♣ (yapay güçlü)
                if (hp >= 22) return "2♣";

                // 20-21 HP dengeli → 2NT
                if (hp >= 20 && hp <= 21 && dengeli) return "2NT";

                // 15-17 HP dengeli → 1NT
                if (hp >= 15 && hp <= 17 && dengeli) return "1NT";

                // 12+ HP → en uzun rengi aç
                if (hp >= 12)
                {
                    return EnUzunRengiAc(el);
                }

                // 12 HP altı → Pas
                return "Pas";
            }

            // 2. Orta ihale — basit kural
            // 6+ HP → Pas değil, ama en azından destek ver
            if (hp >= 6)
            {
                // Partner açtıysa ve destek varsa destek ver
                var partnerTeklifleri = durum.PartnerTeklifleri;
                if (partnerTeklifleri.Count > 0)
                {
                    string sonPartnerTeklifi = partnerTeklifleri.Last();
                    string partnerKozu = TekliftenKozCikar(sonPartnerTeklifi);

                    if (!string.IsNullOrEmpty(partnerKozu))
                    {
                        int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);

                        // 3+ destek → destek teklifi
                        if (destek >= 3)
                        {
                            return DestekTeklifi(durum, partnerKozu, hp);
                        }
                    }
                }
            }

            // Varsayılan: Pas
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
            int destek = ElDegerlendirici.RenkUzunlugu(
                durum.AktifOyuncuEli, koz);

            // Destek sayısına ve HP'ye göre seviye
            int seviye = 2;  // Varsayılan

            if (hp >= 10 && destek >= 4) seviye = 3;
            if (hp >= 13 && destek >= 4) seviye = 4;  // Game

            // Koz sembolünü al
            string kozSembol = KozSembolu(koz);
            return $"{seviye}{kozSembol}";
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

            // _anlasma readonly → yeni bir motor gerekir
            // Ama şimdilik sadece uyarı ver
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