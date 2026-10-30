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
        /// <summary>Motorun kullandığı ortaklık anlaşması.</summary>
        public OrtaklikAnlasmasi Anlasma { get { return _anlasma; } }

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
                // ─── EK KONVANSİYONLAR (anlaşmada açıksa; Bridge Baron listesi) ───
                new BenjaminTwoBids(),
                new ExtendedStayman(),
                new HelloOver1NT(),
                new Truscott2NT(),
                new PreemptiveReraises(),
                new LongShortGameTries(),
                new NewMinorForcing(),
                new WeakJumpShiftResp(),
                new Ogust(),
                new Gambling3NT(),
                new SpecificKingAsks(),
                new VoluntaryFiveMajor(),
                new MatheAskingBids(),
                new ControlAskingBids(),
                new Ghestem(),
                new InvisibleCueBids(),
                new LimitPlusCueBids(),
                new WeakJumpOvercalls(),
                new Sandwich1NT(),
                new MaximalOvercallDbl(),
                new OneMinorTwoNTInv(),
                new German2CRelays(),
                new Walsh(),
                new MiniRoman2D(),
                new Robinson(),
                new InvertedBergenRaises(),
                new ExitTransfer(),
                new Artificial2C2D(),
                new WolffSignoff(),
                new Quantitative4NT(),
                new TexasTransfers(),
                new MinorStayman(),
                new Flannery(),
                new Kokish(),
                new Rubensohl(),
                new LeapingMichaels(),
                new LightnerDouble(),
                new LastTrain(),
                new IkiBirOyunZorlama(),

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

        /// <summary>Tek bir konvansiyonu dener; uygun ve yasal bir teklif varsa onu, yoksa null döndürür.</summary>
        private string KonvansiyonDene(IhaleDurumu durum, IKonvansiyon konvansiyon)
        {

                if (!konvansiyon.AktifMi) return null;

                string teklif;
                try
                {
                    if (!konvansiyon.UygunMu(durum)) return null;
                    teklif = konvansiyon.TeklifVer(durum);
                }
                catch (Exception ex)
                {
                    // Tek bir konvansiyondaki hata tüm ihaleyi çökertmesin
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                        $"[IhaleMotoru] {konvansiyon.Ad} hata: {ex.Message}");
                    if (HatalardaFirlat) throw;
                    return null;
                }
                if (string.IsNullOrEmpty(teklif)) return null;

                // Konvansiyonların koruma satırlarındaki "Pas" (ör. el == null) görüş sayılmaz;
                // ama IBilincliPas ile işaretlenmiş konvansiyonun Pas'ı gerçek bir karardır.
                if (PasiAtla && teklif.Equals("Pas", StringComparison.OrdinalIgnoreCase) &&
                    !(konvansiyon is TemelMantik) && !(konvansiyon is IBilincliPas))
                {
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                        $"[PAS-ATLANDI] {konvansiyon.Ad} Pas dedi, sıradakine geçiliyor.");
                    return null;
                }

                string yasal = SeviyeYukseltme ? YasalSeviyeyeYukselt(durum, teklif) : teklif;
                if (KorumaAktif && !IhaleKoruma.Izin(durum, yasal, konvansiyon.Ad))
                {
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                        $"[KORUMA-RED] {konvansiyon.Ad} → {yasal}");
                    return null;
                }
                if (!GecerliTeklifMi(durum, yasal))
                {
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                        $"[GECERSIZ-RED] {konvansiyon.Ad} → {yasal} (son teklif: {durum.SonGercekTeklif})");
                    return null;
                }


                if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                    $"[KAZANAN] {konvansiyon.Ad} (Oncelik={konvansiyon.Oncelik}) → {yasal}");

                if (yasal != teklif)
                    if (Ayrinti) System.Diagnostics.Debug.WriteLine(
                        $"[IhaleMotoru] {konvansiyon.Ad} seviye yükseltildi: {teklif} → {yasal}");

                SonKonvansiyonAdi = konvansiyon.Ad;
                return yasal;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            SonKonvansiyonAdi = null;
            if (durum == null) return "Pas";
            SirayiHazirla();

            // 0. Aşama: Ek konvansiyonlar (Bridge Baron listesi; ortaklık anlaşmasında açıksa) — açılış sistemlerinden önce
            foreach (var konvansiyon in _siraliKonvansiyonlar)
            {
                if (!(konvansiyon is YeniKonvTaban) || !konvansiyon.AktifMi) continue;
                string ek = KonvansiyonDene(durum, konvansiyon);
                if (ek != null) return ek;
            }

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
                if (konvansiyon is YeniKonvTaban) continue;   // zaten 0. aşamada denendi
                string sonuc = KonvansiyonDene(durum, konvansiyon);
                if (sonuc != null) return sonuc;
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
                case "2♣ Güçlü": return _anlasma.IkiliSinekGuclu && !_anlasma.BenjaminTwoBids;
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
                case "Minor Transfer": return _anlasma.MinorTransfer && !_anlasma.MinorStayman;

                case "İki Yönlü Checkback": return _anlasma.TwoWayCheckback && !_anlasma.NewMinorForcing && !_anlasma.WolffSignoff;
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
                case "Fit Cue-Bid": return _anlasma.FitCueBid;
                case "Serious 3NT": return _anlasma.Serious3NT && !_anlasma.NonSerious3NT;
                case "Non-Serious 3NT": return _anlasma.NonSerious3NT;
                case "DOPI-ROPI": return _anlasma.DopiRopi;
                case "Bergen Raises": return _anlasma.BergenRaises && !_anlasma.InvertedBergenRaises;
                case "Trial Bid": return _anlasma.TrialBid && !_anlasma.LongShortGameTries;
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
                case "Lebensohl": return _anlasma.Lebensohl && !_anlasma.Rubensohl;
                case "Michaels": return _anlasma.Michaels && !_anlasma.Ghestem;
                case "Unusual 2NT": return _anlasma.Unusual2NT && !_anlasma.Ghestem;

                case "Cappelletti": return _anlasma.Cappelletti && !_anlasma.HelloOver1NT;
                case "DONT": return _anlasma.Dont && !_anlasma.HelloOver1NT;
                case "Multi-Landy": return _anlasma.MultiLandy && !_anlasma.HelloOver1NT;

                case "Benjamin Two-Bids": return _anlasma.BenjaminTwoBids;
                case "Extended Stayman": return _anlasma.ExtendedStayman;
                case "Hello over 1NT": return _anlasma.HelloOver1NT;
                case "Truscott 2NT": return _anlasma.Truscott2NT;
                case "Preemptive Reraises": return _anlasma.PreemptiveReraises;
                case "Long & Short Game Tries": return _anlasma.LongShortGameTries;
                case "New Minor Forcing": return _anlasma.NewMinorForcing;
                case "Weak Jump-Shift Resp.": return _anlasma.WeakJumpShiftResp;
                case "Ogust": return _anlasma.Ogust;
                case "Gambling 3NT": return _anlasma.Gambling3NT;
                case "Specific King Asks": return _anlasma.SpecificKingAsks;
                case "Voluntary Five Major Bid": return _anlasma.VoluntaryFiveMajor;
                case "Mathe Asking Bids": return _anlasma.MatheAskingBids;
                case "Control Asking Bids": return _anlasma.ControlAskingBids;
                case "Ghestem": return _anlasma.Ghestem;
                case "Invisible Cue-Bids": return _anlasma.InvisibleCueBids;
                case "Limit-Plus Cue-Bids": return _anlasma.LimitPlusCueBids;
                case "Weak Jump Overcalls": return _anlasma.WeakJumpOvercalls;
                case "Sandwich 1NT": return _anlasma.Sandwich1NT;
                case "Maximal Overcall Dbl.": return _anlasma.MaximalOvercallDbl;
                case "1m-2NT Invitational": return _anlasma.OneMinorTwoNTInv;
                case "German 2C Relays": return _anlasma.German2CRelays;
                case "Walsh": return _anlasma.Walsh;
                case "Mini-Roman 2D": return _anlasma.MiniRoman2D;
                case "Robinson": return _anlasma.Robinson;
                case "Inverted Bergen Raises": return _anlasma.InvertedBergenRaises;
                case "Exit Transfer 1NT Runouts": return _anlasma.ExitTransfer && !_anlasma.Robinson;
                case "Artif. 2C/2D Resp. to 1M-X": return _anlasma.Artificial2C2D;
                case "Wolff Signoff": return _anlasma.WolffSignoff;

                case "Quantitative 4NT": return _anlasma.Quantitative4NT;
                case "Texas Transfers": return _anlasma.TexasTransfers;
                case "Leaping Michaels": return _anlasma.LeapingMichaels;
                case "Lightner Double": return _anlasma.LightnerDouble;
                case "Minor Stayman": return _anlasma.MinorStayman && !_anlasma.MinorTransfer;
                case "Flannery": return _anlasma.Flannery && !_anlasma.ZayifIki && !_anlasma.Multi2D && !_anlasma.MiniRoman2D;
                case "Kokish": return _anlasma.Kokish && _anlasma.IkiliSinekGuclu;
                case "Rubensohl": return _anlasma.Rubensohl && !_anlasma.Lebensohl;
                case "Last Train": return _anlasma.LastTrain;
                case "2/1 Game Force": return _anlasma.IkiBirOyunZorlama && _anlasma.BesliMajor;

                case "Temel Mantık (Fallback)": return true; // Her zaman aktif

                default:
                    return true;
            }
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