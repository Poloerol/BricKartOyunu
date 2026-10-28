using System;
using System.IO;
using Newtonsoft.Json;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// Ortaklık anlaşması — hangi konvansiyonlar kullanılıyor?
    /// </summary>
    public class OrtaklikAnlasmasi
    {
        // ═══════════════════════════════════════════════════════════════════
        // AÇILIŞ SİSTEMİ
        // ═══════════════════════════════════════════════════════════════════

        public bool BesliMajor { get; set; } = true;
        public bool StrongNT { get; set; } = true;
        public bool IkiliSinekGuclu { get; set; } = true;
        public bool ZayifIki { get; set; } = false;
        public bool Preempt { get; set; } = true;
        public bool Multi2D { get; set; } = false;

        // ═══════════════════════════════════════════════════════════════════
        // AÇILIŞ SİSTEMİ — GÜÇLÜ MAJÖR AÇILIŞLARI
        // ═══════════════════════════════════════════════════════════════════

        public bool Namyats { get; set; } = false;

        // ═══════════════════════════════════════════════════════════════════
        // 1♣ CEVAP SİSTEMİ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// 1♣ açışına özel cevap sistemi (Kuvvetli 1♣ / Polish Club hibrit).
        /// true ise OneClub_Answer konvansiyonu devreye girer.
        /// </summary>
        public bool OneClub_AnswerSistemi { get; set; } = true;   // ← DEĞİŞTİ

        // ═══════════════════════════════════════════════════════════════════
        // SİSTEM — REVERSE
        // ═══════════════════════════════════════════════════════════════════

        public bool Reverse { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // 2/1 SİSTEMİ
        // ═══════════════════════════════════════════════════════════════════

        public bool ForcingNT { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // 1NT/2NT CEVAP SİSTEMİ
        // ═══════════════════════════════════════════════════════════════════

        public bool Stayman { get; set; } = true;
        public bool JacobyTransfer { get; set; } = true;
        public bool PuppetStayman { get; set; } = false;
        public bool Smolen { get; set; } = false;
        public bool MinorTransfer { get; set; } = false;

        // ═══════════════════════════════════════════════════════════════════
        // ORTAKLIK — MAJÖR DESTEK SİSTEMİ
        // ═══════════════════════════════════════════════════════════════════

        public bool BergenRaises { get; set; } = true;
        public bool MultiLandy { get; set; } = false;

        // ═══════════════════════════════════════════════════════════════════
        // MİNÖR DESTEK SİSTEMİ
        // ═══════════════════════════════════════════════════════════════════

        public bool InvertedMinors { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // 1NT REBID SONRASI SİSTEM (1X - 1Y - 1NT)
        // ═══════════════════════════════════════════════════════════════════

        public bool TwoWayCheckback { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // XYZ KONVANSİYONU (1X - 1Y - 1Z sonrası)
        // ═══════════════════════════════════════════════════════════════════

        public bool XYZ { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // ORTAKLIK — DESTEK SONRASI DENEME
        // ═══════════════════════════════════════════════════════════════════

        public bool TrialBid { get; set; } = true;
        public bool ThirdSuitForcing { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // ORTAKLIK — 4. RENK FORSİNGİ
        // ═══════════════════════════════════════════════════════════════════

        public bool FourthSuitForcing { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // SLAM KONVANSİYONLARI
        // ═══════════════════════════════════════════════════════════════════

        public bool Blackwood { get; set; } = true;
        public bool RKCB { get; set; } = true;
        public bool RKCB1430 { get; set; } = true;
        public bool Gerber { get; set; } = false;

        // ═══════════════════════════════════════════════════════════════════
        // ORTAKLIK KONVANSİYONLARI
        // ═══════════════════════════════════════════════════════════════════

        public bool Jacoby2NT { get; set; } = true;
        public bool Splinter { get; set; } = true;
        public bool Drury { get; set; } = false;
        public bool ReverseDrury { get; set; } = false;
        public bool CueBid { get; set; } = true;
        public bool Serious3NT { get; set; } = true;
        public bool NonSerious3NT { get; set; } = false;
        public bool LastTrain { get; set; } = false;

        // ═══════════════════════════════════════════════════════════════════
        // RAKİP MÜDAHALESİ
        // ═══════════════════════════════════════════════════════════════════

        public bool NegativeDouble { get; set; } = true;
        public bool TakeoutDouble { get; set; } = true;
        public bool SupportDouble { get; set; } = false;
        public bool ResponsiveDouble { get; set; } = false;
        public bool Lebensohl { get; set; } = true;
        public bool Michaels { get; set; } = true;
        public bool Unusual2NT { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // 1NT MÜDAHALE SİSTEMİ
        // ═══════════════════════════════════════════════════════════════════

        public bool Cappelletti { get; set; } = false;
        public bool Dont { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // İŞARETLEŞME
        // ═══════════════════════════════════════════════════════════════════

        public bool UDCA { get; set; } = false;
        public bool StandartSignal { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // FABRİKA METOTLARI
        // ═══════════════════════════════════════════════════════════════════

        public static OrtaklikAnlasmasi Varsayilan()
        {
            return new OrtaklikAnlasmasi
            {
                BesliMajor = true,
                InvertedMinors = true,
                ForcingNT = true,
                StrongNT = true,
                IkiliSinekGuclu = true,
                ZayifIki = true,
                Multi2D = false,
                Preempt = true,
                Namyats = false,

                OneClub_AnswerSistemi = false,   // ← DEĞİŞTİ (zaten böyleydi)

                Stayman = true,
                BergenRaises = true,
                JacobyTransfer = true,
                Smolen = true,
                MinorTransfer = true,
                PuppetStayman = true,

                TwoWayCheckback = true,
                XYZ = true,

                TrialBid = true,
                ThirdSuitForcing = true,
                FourthSuitForcing = true,

                Cappelletti = false,
                Dont = true,
                MultiLandy = false,

                Serious3NT = true,
                NonSerious3NT = false,

                Blackwood = true,
                RKCB = true,
                RKCB1430 = true,
                Gerber = true,

                Jacoby2NT = true,
                Splinter = true,
                Drury = true,
                CueBid = true,
                Reverse = true,

                NegativeDouble = true,
                TakeoutDouble = true,
                SupportDouble = true,
                ResponsiveDouble = true,
                Lebensohl = true,
                Michaels = true,
                Unusual2NT = true,

                StandartSignal = true
            };
        }

        /// <summary>Anlaşmanın bağımsız bir kopyası (yalnızca bool alanlar olduğundan yüzeysel kopya yeterli).</summary>
        public OrtaklikAnlasmasi Kopya()
        {
            return (OrtaklikAnlasmasi)MemberwiseClone();
        }

        public static OrtaklikAnlasmasi Basit()
        {
            return new OrtaklikAnlasmasi
            {
                BesliMajor = false,
                InvertedMinors = false,
                ForcingNT = false,
                StrongNT = false,
                IkiliSinekGuclu = false,
                Preempt = false,

                Stayman = true,
                BergenRaises = false,
                JacobyTransfer = false,
                Smolen = false,
                MinorTransfer = false,
                PuppetStayman = false,

                TwoWayCheckback = false,
                XYZ = false,

                TrialBid = false,
                ThirdSuitForcing = false,
                FourthSuitForcing = false,

                Blackwood = true,
                RKCB = false,
                RKCB1430 = false,
                Gerber = false,

                Jacoby2NT = false,
                Splinter = false,
                Drury = false,
                CueBid = false,
                Reverse = false,

                NegativeDouble = true,
                TakeoutDouble = false,
                SupportDouble = false,
                ResponsiveDouble = false,
                Lebensohl = false,
                Michaels = false,
                Unusual2NT = false,

                Cappelletti = false,
                Dont = false,
                MultiLandy = false,

                StandartSignal = true
            };
        }

        public string Ozet()
        {
            string acilis = BesliMajor ? "5'li majör" : "4'lü majör";
            string nt = StrongNT ? "Strong NT (15-17)" : "Weak NT (12-14)";

            var aktifler = new System.Collections.Generic.List<string>();

            if (IkiliSinekGuclu) aktifler.Add("2♣ Güçlü");
            if (ZayifIki && !Multi2D) aktifler.Add("Zayıf İki");
            if (Multi2D) aktifler.Add("Multi 2♦");
            if (Preempt) aktifler.Add("Preempt");
            if (Namyats) aktifler.Add("Namyats");

            // 1♣ cevapları
            if (OneClub_AnswerSistemi) aktifler.Add("1♣ Cevap Sistemi");   // ← DEĞİŞTİ

            if (Stayman) aktifler.Add("Stayman");
            if (JacobyTransfer) aktifler.Add("Transfer");
            if (PuppetStayman) aktifler.Add("Puppet");
            if (Smolen) aktifler.Add("Smolen");
            if (MinorTransfer) aktifler.Add("Min.Transfer");
            if (TwoWayCheckback) aktifler.Add("2-Way Checkback");
            if (XYZ) aktifler.Add("XYZ");

            if (ForcingNT) aktifler.Add("Forcing NT");
            if (InvertedMinors) aktifler.Add("Inverted Minors");
            if (Jacoby2NT) aktifler.Add("Jacoby2NT");
            if (Splinter) aktifler.Add("Splinter");
            if (BergenRaises) aktifler.Add("Bergen Raises");
            if (TrialBid) aktifler.Add("Trial Bid");
            if (ThirdSuitForcing) aktifler.Add("3. Renk Forsingi");
            if (FourthSuitForcing) aktifler.Add("4. Renk Forsingi");
            if (Reverse) aktifler.Add("Reverse");
            if (Drury) aktifler.Add("Drury");
            if (ReverseDrury) aktifler.Add("Reverse Drury");
            if (CueBid) aktifler.Add("Cue Bid");

            if (Blackwood) aktifler.Add("Blackwood");
            if (RKCB) aktifler.Add("RKCB");
            if (RKCB1430) aktifler.Add("RKCB 1430");
            if (Gerber) aktifler.Add("Gerber");

            if (NegativeDouble) aktifler.Add("Neg.Dbl");
            if (TakeoutDouble) aktifler.Add("Takeout Dbl");
            if (SupportDouble) aktifler.Add("Supp.Dbl");
            if (ResponsiveDouble) aktifler.Add("Resp.Dbl");
            if (Lebensohl) aktifler.Add("Lebensohl");
            if (Michaels) aktifler.Add("Michaels");
            if (Unusual2NT) aktifler.Add("Unusual2NT");

            if (Cappelletti) aktifler.Add("Cappelletti");
            if (Dont) aktifler.Add("DONT");
            if (MultiLandy) aktifler.Add("Multi-Landy");

            if (UDCA) aktifler.Add("UDCA");
            else if (StandartSignal) aktifler.Add("Standart Signal");

            return $"Açılış: {acilis}, {nt}\nAktif: {string.Join(", ", aktifler)}";
        }

        // ═══════════════════════════════════════════════════════════════════
        // KAYDET / YÜKLE
        // ═══════════════════════════════════════════════════════════════════

        private const string DosyaAdi = "ortaklik_anlasmasi.json";

        public void Kaydet(string dosyaYolu = null)
        {
            try
            {
                string yol = dosyaYolu ?? Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, DosyaAdi);

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(yol, json);

                System.Diagnostics.Debug.WriteLine(
                    $"[OrtaklikAnlasmasi] Kaydedildi: {yol}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OrtaklikAnlasmasi] Kaydetme hatası: {ex.Message}");
            }
        }

        public static OrtaklikAnlasmasi Yukle(string dosyaYolu = null)
        {
            try
            {
                string yol = dosyaYolu ?? Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, DosyaAdi);

                if (!File.Exists(yol))
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[OrtaklikAnlasmasi] Dosya yok, varsayılan kullanılıyor.");
                    return Varsayilan();
                }

                string json = File.ReadAllText(yol);
                var anlasma = JsonConvert.DeserializeObject<OrtaklikAnlasmasi>(json);

                System.Diagnostics.Debug.WriteLine(
                    $"[OrtaklikAnlasmasi] Yüklendi: {yol}");

                return anlasma ?? Varsayilan();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OrtaklikAnlasmasi] Yükleme hatası: {ex.Message}");
                return Varsayilan();
            }
        }
    }
}