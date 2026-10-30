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
        // EK KONVANSİYONLAR (Bridge Baron listesi; varsayılan KAPALI)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Benjamin Two-Bids: 2♣ çok güçlü, 2♦ güçlü iki (Acol)</summary>
        public bool BenjaminTwoBids { get; set; } = false;
        /// <summary>Extended Stayman: zayıf 4-4 majör</summary>
        public bool ExtendedStayman { get; set; } = false;
        /// <summary>Hello: rakip 1NT'sine müdahale</summary>
        public bool HelloOver1NT { get; set; } = false;
        /// <summary>Truscott 2NT: 1M-(Dbl)-2NT limit raise</summary>
        public bool Truscott2NT { get; set; } = false;
        /// <summary>Preemptive Reraises</summary>
        public bool PreemptiveReraises { get; set; } = false;
        /// <summary>Long & Short Game Tries (TrialBid ile birlikte kullanılmaz)</summary>
        public bool LongShortGameTries { get; set; } = false;
        /// <summary>New Minor Forcing (İki Yönlü Checkback ile birlikte kullanılmaz)</summary>
        public bool NewMinorForcing { get; set; } = false;
        /// <summary>Weak Jump-Shift cevapları</summary>
        public bool WeakJumpShiftResp { get; set; } = false;
        /// <summary>Ogust: zayıf iki + 2NT</summary>
        public bool Ogust { get; set; } = false;
        /// <summary>Gambling 3NT açılışı</summary>
        public bool Gambling3NT { get; set; } = false;
        /// <summary>Specific King Asks</summary>
        public bool SpecificKingAsks { get; set; } = false;
        /// <summary>Voluntary Five Major Bid</summary>
        public bool VoluntaryFiveMajor { get; set; } = false;
        /// <summary>Mathe Asking Bids</summary>
        public bool MatheAskingBids { get; set; } = false;
        /// <summary>Control Asking Bids</summary>
        public bool ControlAskingBids { get; set; } = false;
        /// <summary>Ghestem (Michaels ve Unusual 2NT yerine)</summary>
        public bool Ghestem { get; set; } = false;
        /// <summary>Invisible Cue-Bids</summary>
        public bool InvisibleCueBids { get; set; } = false;
        /// <summary>Limit-Plus Cue-Bids</summary>
        public bool LimitPlusCueBids { get; set; } = false;
        /// <summary>Weak Jump Overcalls</summary>
        public bool WeakJumpOvercalls { get; set; } = false;
        /// <summary>Sandwich 1NT</summary>
        public bool Sandwich1NT { get; set; } = false;
        /// <summary>Maximal Overcall Double</summary>
        public bool MaximalOvercallDbl { get; set; } = false;
        /// <summary>1m-2NT Invitational</summary>
        public bool OneMinorTwoNTInv { get; set; } = false;
        /// <summary>German 2♣ Relays</summary>
        public bool German2CRelays { get; set; } = false;
        /// <summary>Walsh</summary>
        public bool Walsh { get; set; } = false;
        /// <summary>Mini-Roman 2♦</summary>
        public bool MiniRoman2D { get; set; } = false;
        /// <summary>Robinson (Exit Transfer ile birlikte kullanılmaz)</summary>
        public bool Robinson { get; set; } = false;
        /// <summary>Inverted Bergen Raises (Bergen Raises yerine)</summary>
        public bool InvertedBergenRaises { get; set; } = false;
        /// <summary>Exit Transfers: 1NT kontra sonrası zayıf kaçış</summary>
        public bool ExitTransfer { get; set; } = false;
        /// <summary>Artif. 2♣/2♦ cevapları: 1M-(Dbl)</summary>
        public bool Artificial2C2D { get; set; } = false;
        /// <summary>Wolff Signoff (İki Yönlü Checkback ile birlikte kullanılmaz)</summary>
        public bool WolffSignoff { get; set; } = false;

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
        public bool DopiRopi { get; set; } = true;      // Blackwood/RKCB'ye rakip müdahalesinde cevap (DOPI/ROPI)
        public bool FitCueBid { get; set; } = true;     // Rakip müdahalesinde fit gösteren cue-bid

        // ── Çekirdek ek konvansiyonlar (varsayılan KAPALI) ──
        public bool Quantitative4NT { get; set; } = false;
        public bool TexasTransfers { get; set; } = false;
        public bool MinorStayman { get; set; } = false;
        public bool Flannery { get; set; } = false;
        public bool Kokish { get; set; } = false;
        public bool Rubensohl { get; set; } = false;
        public bool LeapingMichaels { get; set; } = false;
        public bool LightnerDouble { get; set; } = false;

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
        public bool OddEvenDiscards { get; set; } = false;
        public bool Lavinthal { get; set; } = false;
        public bool SmithEcho { get; set; } = false;

        // ═══════════════════════════════════════════════════════════════════
        // AGRESİFLİK VE BECERİ (Bridge Baron tarzı profil ayarları)
        // -1 = Temkinli, 0 = Orta (varsayılan davranış), +1 = Atak
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Açılış: -1 yalnız 12+ HP, 0 12+ HP veya 11 HP + 20 kuralı, +1 10 HP + 20 kuralı da açar.</summary>
        public int AcilisAgresifligi { get; set; } = 0;
        /// <summary>Müdahale (overcall): -1 eşikler +2 HP, +1 eşikler -1 HP.</summary>
        public int MudahaleAgresifligi { get; set; } = 0;
        /// <summary>Önleyici açılış: -1 yalnız 0-8 HP ve 2+ honörlü 7'li renk, +1 10 HP'ye kadar.</summary>
        public int PreemptAgresifligi { get; set; } = 0;
        /// <summary>Kart oyunu becerisi 1-20. 20 = hatasız (varsayılan); düşükse yapay zekâ ara sıra rastgele yasal kart oynar.</summary>
        public int BeceriDuzeyi { get; set; } = 20;

        // ═══════════════════════════════════════════════════════════════════
        // FABRİKA METOTLARI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Varsayılan anlaşma + adı verilen ek (Bridge Baron) konvansiyonu açık; pratik modu için.</summary>
        public static OrtaklikAnlasmasi EkKonvansiyonIcin(string ad)
        {
            var a = Varsayilan();
            switch (ad)
            {
                case "Benjamin Two-Bids": a.BenjaminTwoBids = true; break;
                case "Extended Stayman": a.ExtendedStayman = true; break;
                case "Hello over 1NT": a.HelloOver1NT = true; break;
                case "Truscott 2NT": a.Truscott2NT = true; break;
                case "Preemptive Reraises": a.PreemptiveReraises = true; break;
                case "Long & Short Game Tries": a.LongShortGameTries = true; break;
                case "New Minor Forcing": a.NewMinorForcing = true; break;
                case "Weak Jump-Shift Resp.": a.WeakJumpShiftResp = true; break;
                case "Ogust": a.Ogust = true; break;
                case "Gambling 3NT": a.Gambling3NT = true; break;
                case "Specific King Asks": a.SpecificKingAsks = true; break;
                case "Voluntary Five Major Bid": a.VoluntaryFiveMajor = true; break;
                case "Mathe Asking Bids": a.MatheAskingBids = true; break;
                case "Control Asking Bids": a.ControlAskingBids = true; break;
                case "Ghestem": a.Ghestem = true; break;
                case "Invisible Cue-Bids": a.InvisibleCueBids = true; break;
                case "Limit-Plus Cue-Bids": a.LimitPlusCueBids = true; break;
                case "Weak Jump Overcalls": a.WeakJumpOvercalls = true; break;
                case "Sandwich 1NT": a.Sandwich1NT = true; break;
                case "Maximal Overcall Dbl.": a.MaximalOvercallDbl = true; break;
                case "1m-2NT Invitational": a.OneMinorTwoNTInv = true; break;
                case "German 2C Relays": a.German2CRelays = true; break;
                case "Walsh": a.Walsh = true; break;
                case "Mini-Roman 2D": a.MiniRoman2D = true; break;
                case "Robinson": a.Robinson = true; break;
                case "Inverted Bergen Raises": a.InvertedBergenRaises = true; break;
                case "Exit Transfer 1NT Runouts": a.ExitTransfer = true; break;
                case "Artif. 2C/2D Resp. to 1M-X": a.Artificial2C2D = true; break;
                case "Wolff Signoff": a.WolffSignoff = true; break;
                case "Quantitative 4NT": a.Quantitative4NT = true; break;
                case "Texas Transfers": a.TexasTransfers = true; break;
                case "Minor Stayman": a.MinorStayman = true; break;
                case "Flannery": a.Flannery = true; break;
                case "Kokish": a.Kokish = true; break;
                case "Rubensohl": a.Rubensohl = true; break;
                case "Leaping Michaels": a.LeapingMichaels = true; break;
                case "Lightner Double": a.LightnerDouble = true; break;
                case "Last Train": a.LastTrain = true; break;
            }
            return a;
        }

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
            if (FitCueBid) aktifler.Add("Fit Cue-Bid");
            if (Serious3NT) aktifler.Add("Ciddi 3NT");
            if (NonSerious3NT) aktifler.Add("Ciddi olmayan 3NT");
            if (DopiRopi) aktifler.Add("DOPI/ROPI");

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

            var ekler = new System.Collections.Generic.List<string>();
            if (BenjaminTwoBids) ekler.Add("Benjamin Two-Bids");
            if (ExtendedStayman) ekler.Add("Extended Stayman");
            if (HelloOver1NT) ekler.Add("Hello");
            if (Truscott2NT) ekler.Add("Truscott 2NT");
            if (PreemptiveReraises) ekler.Add("Preemptive Reraises");
            if (LongShortGameTries) ekler.Add("Long & Short Game Tries");
            if (NewMinorForcing) ekler.Add("New Minor Forcing");
            if (WeakJumpShiftResp) ekler.Add("Weak Jump-Shift cevapları");
            if (Ogust) ekler.Add("Ogust");
            if (Gambling3NT) ekler.Add("Gambling 3NT açılışı");
            if (SpecificKingAsks) ekler.Add("Specific King Asks");
            if (VoluntaryFiveMajor) ekler.Add("Voluntary Five Major Bid");
            if (MatheAskingBids) ekler.Add("Mathe Asking Bids");
            if (ControlAskingBids) ekler.Add("Control Asking Bids");
            if (Ghestem) ekler.Add("Ghestem");
            if (InvisibleCueBids) ekler.Add("Invisible Cue-Bids");
            if (LimitPlusCueBids) ekler.Add("Limit-Plus Cue-Bids");
            if (WeakJumpOvercalls) ekler.Add("Weak Jump Overcalls");
            if (Sandwich1NT) ekler.Add("Sandwich 1NT");
            if (MaximalOvercallDbl) ekler.Add("Maximal Overcall Double");
            if (OneMinorTwoNTInv) ekler.Add("1m-2NT Invitational");
            if (German2CRelays) ekler.Add("German 2♣ Relays");
            if (Walsh) ekler.Add("Walsh");
            if (MiniRoman2D) ekler.Add("Mini-Roman 2♦");
            if (Robinson) ekler.Add("Robinson");
            if (InvertedBergenRaises) ekler.Add("Inverted Bergen Raises");
            if (ExitTransfer) ekler.Add("Exit Transfers");
            if (Artificial2C2D) ekler.Add("Artif. 2♣/2♦ cevapları");
            if (WolffSignoff) ekler.Add("Wolff Signoff");
            if (Quantitative4NT) ekler.Add("Quantitative 4NT");
            if (TexasTransfers) ekler.Add("Texas transferleri");
            if (MinorStayman) ekler.Add("Minor Stayman");
            if (Flannery) ekler.Add("Flannery 2♦");
            if (Kokish) ekler.Add("Kokish");
            if (Rubensohl) ekler.Add("Rubensohl");
            if (LeapingMichaels) ekler.Add("Leaping Michaels");
            if (LightnerDouble) ekler.Add("Lightner kontra");
            if (LastTrain) ekler.Add("Last Train");
            aktifler.AddRange(ekler);

            if (UDCA) aktifler.Add("UDCA");
            else if (StandartSignal) aktifler.Add("Standart Signal");
            if (OddEvenDiscards) aktifler.Add("Tek-Çift atma");
            if (Lavinthal) aktifler.Add("Lavinthal");
            if (SmithEcho) aktifler.Add("Smith echo");

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