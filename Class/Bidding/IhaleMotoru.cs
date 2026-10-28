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

        /// <summary>Son TeklifVer çağrısında teklifi üreten konvansiyon/gelişim adı (yoksa null).</summary>
        public string SonKonvansiyonAdi { get; private set; }

        private static string AcilisGelisimAdi(IAcilisGelisimi g)
        {
            string ad = g.GetType().Name;
            if (ad.StartsWith("OneClub_OneDiamond", StringComparison.Ordinal)) return "1♣ Cevap Sistemi";
            if (ad.StartsWith("OneClub_", StringComparison.Ordinal)) return "1♣ Açılış Sistemi";
            if (ad.StartsWith("OneDiamond_", StringComparison.Ordinal)) return "1♦ Açılış Sistemi";
            return ad;
        }

        /// <summary>
        /// Pratik edilebilecek (ortaklık anlaşmasında AÇIK) konvansiyon adları; yedek (fallback) kurallar hariç.
        /// </summary>
        public List<string> PratikKonvansiyonAdlari()
        {
            var liste = new List<string>();
            foreach (var g in _acilisGelismleri)
            {
                if (!AcilisGelisimiAktifMi(g)) continue;
                string ad = AcilisGelisimAdi(g);
                if (!liste.Contains(ad)) liste.Add(ad);
            }
            foreach (var k in _konvansiyonlar)
            {
                if (!k.AktifMi) continue;
                if (k is TemelMantik || k is BasitCevap) continue;
                if (!liste.Contains(k.Ad)) liste.Add(k.Ad);
            }
            return liste;
        }

        /// <summary>true: her teklifte ayrıntılı Debug çıktısı yazılır (Visual Studio'da yavaşlatır; varsayılan kapalı).</summary>
        public static bool Ayrinti = false;

        private List<IKonvansiyon> _siraliKonvansiyonlar;
        private List<IAcilisGelisimi> _siraliGelismeler;
        private int _siraliSayi = -1;

        private void SirayiHazirla()
        {
            if (_siraliSayi == _konvansiyonlar.Count + _acilisGelismleri.Count && _siraliKonvansiyonlar != null) return;
            _siraliKonvansiyonlar = _konvansiyonlar.OrderBy(k => k.Oncelik).ToList();
            _siraliGelismeler = _acilisGelismleri.OrderByDescending(g => g.Oncelik).ToList();
            _siraliSayi = _konvansiyonlar.Count + _acilisGelismleri.Count;
        }

        public bool KorumaAktif { get; set; } = true;       // IhaleKoruma denetimi
        public bool SeviyeYukseltme { get; set; } = false;  // eski "yasadışı teklifi 1 üst seviyeye çıkar" hilesi
        public bool PasiAtla { get; set; } = true;          // konvansiyon "Pas" derse görüşü yok say
                                                            // (IBilincliPas işaretli konvansiyonlar hariç)

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
                new PreemptCevap(),          // ortağın 3'lü bozucu açılışına cevap
                new ZayifIkiCevap(),         // ortağın zayıf iki açılışına cevap
                new ZayifIkiMudahale(),      // rakibin zayıf iki açılışına müdahale
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
                new FitCueBid(),   // ← YENİ: Rakip müdahalesine karşı fit cue-bid
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
                new SupportDoubleCevap(),
                new Michaels(),
                new StrongNT(),
                new NegativeDouble(),
                new ResponsiveDouble(),
                new ResponsiveDoubleCevap(),
                new Unusual2NT(),
                new TakeoutDouble(),
                new TakeoutDoubleCevap(),
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

                // ─── FALLBACK (EN SON ÇARE) ──────────────────────────────────
                new BasitCevap(),
                new TemelMantik(),   // ← YENİ: Evrensel briç kuralları, en son çalışır
            };

            _acilisGelismleri = new List<IAcilisGelisimi>
            {
                new OneClub_Opening(),
                new OneDiamond_Opening(),
                new OneClub_OneDiamond(),
                new OneClub_OneDiamond_OneHeart(),
            };

            AnlasmayiUygula();

            if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                $"[IhaleMotoru] Oluşturuldu. Aktif konvansiyon sayısı: " +
                $"{_konvansiyonlar.Count(k => k.AktifMi)}");
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            SonKonvansiyonAdi = null;
            if (durum == null) return "Pas";
            SirayiHazirla();

            // 1. Aşama: Açılış Gelişimleri
            if (durum.AktifOyuncuEli != null)
            {
                foreach (var gelisim in _siraliGelismeler)
                {
                    try
                    {
                        if (!AcilisGelisimiAktifMi(gelisim)) continue;
                        if (!gelisim.UygunMu(durum.AktifOyuncuEli, durum)) continue;

                        string yeniTeklif = gelisim.TeklifVer(durum.AktifOyuncuEli, durum);
                        if (string.IsNullOrEmpty(yeniTeklif)) continue;

                        if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                            $"[AcilisGelismi] {gelisim.GetType().Name} uygun → {yeniTeklif}");

                        string yasalYeni = YasalSeviyeyeYukselt(durum, yeniTeklif);
                        if (!GecerliTeklifMi(durum, yasalYeni)) continue;

                        if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                            $"[KAZANAN-AcilisGelismi] {gelisim.GetType().Name} → {yasalYeni}");
                        SonKonvansiyonAdi = AcilisGelisimAdi(gelisim);
                        return yasalYeni;
                    }
                    catch (Exception ex)
                    {
                        if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                            $"[AcilisGelismi] {gelisim.GetType().Name} hata: {ex.Message}");
                        if (HatalardaFirlat) throw;
                    }
                }
            }

            // 2. Aşama: Standart Konvansiyonlar (TemelMantik dahil)
            foreach (var konvansiyon in _siraliKonvansiyonlar)
            {
                if (!konvansiyon.AktifMi) continue;

                string teklif;
                try
                {
                    if (!konvansiyon.UygunMu(durum)) continue;
                    teklif = konvansiyon.TeklifVer(durum);
                }
                catch (Exception ex)
                {
                    // Tek bir konvansiyondaki hata tüm ihaleyi çökertmesin
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                        $"[IhaleMotoru] {konvansiyon.Ad} hata: {ex.Message}");
                    if (HatalardaFirlat) throw;
                    continue;
                }
                if (string.IsNullOrEmpty(teklif)) continue;

                // Konvansiyonların koruma satırlarındaki "Pas" (ör. el == null) görüş sayılmaz;
                // ama IBilincliPas ile işaretlenmiş konvansiyonun Pas'ı gerçek bir karardır.
                if (PasiAtla && teklif.Equals("Pas", StringComparison.OrdinalIgnoreCase) &&
                    !(konvansiyon is TemelMantik) && !(konvansiyon is IBilincliPas))
                {
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                        $"[PAS-ATLANDI] {konvansiyon.Ad} Pas dedi, sıradakine geçiliyor.");
                    continue;
                }

                string yasal = SeviyeYukseltme ? YasalSeviyeyeYukselt(durum, teklif) : teklif;
                if (KorumaAktif && !IhaleKoruma.Izin(durum, yasal, konvansiyon.Ad))
                {
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                        $"[KORUMA-RED] {konvansiyon.Ad} → {yasal}");
                    continue;
                }
                if (!GecerliTeklifMi(durum, yasal))
                {
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                        $"[GECERSIZ-RED] {konvansiyon.Ad} → {yasal} (son teklif: {durum.SonGercekTeklif})");
                    continue;
                }


                if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                    $"[KAZANAN] {konvansiyon.Ad} (Oncelik={konvansiyon.Oncelik}) → {yasal}");

                if (yasal != teklif)
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                        $"[IhaleMotoru] {konvansiyon.Ad} seviye yükseltildi: {teklif} → {yasal}");

                SonKonvansiyonAdi = konvansiyon.Ad;
                return yasal;
            }

            // 3. Aşama: Hiçbir şey uymadıysa
            if (Ayrinti) System.Diagnostics.Debug.WriteLine("[IhaleMotoru] Hiçbir konvansiyon uygun değil → Pas");
            return "Pas";
        }

        /// <summary>
        /// 1♣ cevap gelişimleri (OneClub_OneDiamond, OneClub_OneDiamond_OneHeart)
        /// yalnızca OneClub_AnswerSistemi açıkken çalışır. OneClub_Opening bir AÇILIŞ
        /// gelişimidir, bayraktan etkilenmez.
        /// </summary>
        private bool AcilisGelisimiAktifMi(IAcilisGelisimi gelisim)
        {
            string ad = gelisim.GetType().Name;
            if (ad.StartsWith("OneClub_OneDiamond", StringComparison.Ordinal))
                return _anlasma.OneClub_AnswerSistemi;
            return true;
        }

        private static bool GecerliTeklifMi(IhaleDurumu durum, string teklif)
        {
            if (Ayrinti) System.Diagnostics.Debug.WriteLine(
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

        private void AnlasmayiUygula()
        {
            foreach (var k in _konvansiyonlar)
            {
                k.AktifMi = AnlasmayaGoreAktifMi(k.Ad);
            }

            // Multi 2♦ açıksa zayıf iki cevap/müdahale 2♦'yı ele almaz
            foreach (var k in _konvansiyonlar)
            {
                var zc = k as ZayifIkiCevap; if (zc != null) zc.Multi2DAcik = _anlasma.Multi2D;
                var zm = k as ZayifIkiMudahale; if (zm != null) zm.Multi2DAcik = _anlasma.Multi2D;
            }

            var cappelletti = _konvansiyonlar.FirstOrDefault(k => k.Ad == "Cappelletti");
            var dont = _konvansiyonlar.FirstOrDefault(k => k.Ad == "DONT");
            if (cappelletti != null && dont != null && cappelletti.AktifMi && dont.AktifMi)
            {
                dont.AktifMi = false;
                if (Ayrinti) System.Diagnostics.Debug.WriteLine(
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
                case "Preempt Cevap": return _anlasma.Preempt;
                case "Zayıf İki Cevap": return _anlasma.ZayifIki && !_anlasma.Multi2D;
                case "Zayıf İki Müdahale": return _anlasma.ZayifIki && !_anlasma.Multi2D;
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
                case "Takeout Double Cevap": return _anlasma.TakeoutDouble;
                case "Support Double": return _anlasma.SupportDouble;
                case "Support Double Cevap": return _anlasma.SupportDouble;
                case "Responsive Double Cevap": return _anlasma.ResponsiveDouble;
                case "Responsive Double": return _anlasma.ResponsiveDouble;
                case "Lebensohl (Reverse)": return _anlasma.Lebensohl;
                case "Lebensohl (Zayıf 2)": return _anlasma.Lebensohl;
                case "Lebensohl": return _anlasma.Lebensohl;
                case "Michaels": return _anlasma.Michaels;
                case "Unusual 2NT": return _anlasma.Unusual2NT;

                case "Cappelletti": return _anlasma.Cappelletti;
                case "DONT": return _anlasma.Dont;
                case "Multi-Landy": return _anlasma.MultiLandy;

                case "Temel Mantık (Fallback)": return true; // Her zaman aktif

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

        /// <summary>Verilen konvansiyon adının kısa açıklaması (öğrenme modu için); bulunamazsa null.</summary>
        public string KonvansiyonAciklamasi(string ad)
        {
            if (string.IsNullOrEmpty(ad)) return null;
            var k = _konvansiyonlar.FirstOrDefault(x => x.Ad == ad);
            return k == null ? null : k.Aciklama;
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
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
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
                return "Uygun konvansiyon yok → Pas";

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

    /// <summary>
    /// İşaret arayüzü: bu konvansiyon UygunMu = true iken "Pas" döndürürse bu
    /// BİLİNÇLİ bir karardır (ör. DOPI/ROPI'de 1 anahtar kart = Pas).
    /// IhaleMotoru bu Pas'ı atlamaz, doğrudan teklif olarak kabul eder.
    /// Kullanım: public class DopiRopi : IKonvansiyon, IBilincliPas
    /// </summary>
    public interface IBilincliPas { }
}