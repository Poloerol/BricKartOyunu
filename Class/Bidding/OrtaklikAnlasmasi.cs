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
        /// <summary>Good-Bad 2NT: 1♦–(…)–(2M) sonrası açanın 2NT'si iyi/kötü röle (Sasson kitabı)</summary>
        public bool GoodBad2NT { get; set; } = false;
        /// <summary>Inverted DBL: 1x–(Dbl)–Rdbl–(y) sonrası açanın Dbl = y'de 2-3 kart, Pas = 4+ ya da tekil+iyi el</summary>
        public bool InvertedDbl { get; set; } = false;
        /// <summary>Mixed Raise: (1x)–1M–(Pas)–3x atlamalı cue-bid = 4+ destek, 10-12 HP</summary>
        public bool MixedRaise { get; set; } = false;
        /// <summary>FSJS: (1x)–1M–(Pas)–atlamalı yeni renk = 5+ renk, 4+ majör desteği</summary>
        public bool FSJS { get; set; } = false;
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
        /// <summary>Feature-asking 2NT: zayıf iki + 2NT = yan renk özelliği sorusu (Ogust ile birlikte kullanılmaz)</summary>
        public bool FeatureAsking2NT { get; set; } = false;
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
        /// <summary>Ortağın 1NT müdahalesinden sonra sistemler açık (Stayman/transfer) — yalnızca profilde saklanır</summary>
        public bool NT1OvercallSystemsOn { get; set; } = false;
        /// <summary>Rakip 1NT'sine karşı savunma geri (balancing) koltukta da geçerli — yalnızca profilde saklanır</summary>
        public bool NTDefenseBalancing { get; set; } = false;
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

        /// <summary>1♦ açışına özel cevap/gelişim sistemi (SAS kitabı s.24-34). Varsayılan kapalı.</summary>
        public bool OneDiamond_AnswerSistemi { get; set; } = false;
        public bool OneHeart_AnswerSistemi { get; set; } = false;
        public bool OneSpade_AnswerSistemi { get; set; } = false;
        public bool OneNT_AnswerSistemi { get; set; } = false;
        public bool TwoClub_AnswerSistemi { get; set; } = false;
        public bool TwoDiamond_AnswerSistemi { get; set; } = false;
        public bool TwoHeart_AnswerSistemi { get; set; } = false;
        public bool TwoSpade_AnswerSistemi { get; set; } = false;
        public bool TwoNT_AnswerSistemi { get; set; } = false;
        public bool Role2C_AnswerSistemi { get; set; } = false;
        public bool Araya_AnswerSistemi { get; set; } = false;

        // ═══════════════════════════════════════════════════════════════════
        // SİSTEM — REVERSE
        // ═══════════════════════════════════════════════════════════════════

        public bool Reverse { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════════
        // 2/1 SİSTEMİ
        // ═══════════════════════════════════════════════════════════════════

        public bool ForcingNT { get; set; } = true;

        /// <summary>2/1 Game Force: 1♥/1♠ açılışına sıçramasız 2 seviyesi yeni renk cevabı oyuna kadar zorlayıcıdır (5'li majör gerekir).</summary>
        public bool IkiBirOyunZorlama { get; set; } = true;

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

        // Sinyaller formu seçimleri (tam sayı: radyo = seçenek sırası, onay kutusu = 0/1). Bayrak olmadıkları için
        // konvansiyon listelerinde görünmezler. UDCA/Tek-Çift/Lavinthal/Smith echo yukarıdaki bayraklara da yansır.
        public int SigAnySignal { get; set; } = 0;              // 0 evet, 1 hayır
        public int SigFirstDiscardRelevant { get; set; } = 0;   // 0 evet, 1 hayır
        public int SigPrimary { get; set; } = 1;                // 0 sayı, 1 tutum, 2 tek/çift
        public int SigSmithPartner { get; set; } = 0;           // 0 yok, 1 yüksek-alçak, 2 alçak-yüksek
        public int SigSmithLeader { get; set; } = 0;
        public int SigCountHighLow { get; set; } = 0;           // 0 çift, 1 tek
        public int SigCountOpp { get; set; } = 0;               // 0 evet, 1 hayır
        public int SigFirstDiscard { get; set; } = 0;           // 0 tutum, 1 sayı, 2 tek/çift, 3 Lavinthal, 4 dönüşümlü
        public int SigAttitudeHigh { get; set; } = 0;           // 0 teşvik, 1 caydırır
        public int SigTrump { get; set; } = 2;                  // 0 Vinje, 1 yok, 2 3+ ile yüksek-alçak
        public int LeadSuitStyle { get; set; } = 1;            // 0 üçüncü/beşinci, 1 dördüncü
        public int LeadNTStyle { get; set; } = 1;              // 0 üçüncü/beşinci, 1 dördüncü, 2 en düşük
        public int LeadSuitRusinow { get; set; } = 0;
        public int LeadSuitJournalist { get; set; } = 0;
        public int LeadSuitZeroTwo { get; set; } = 0;
        public int LeadSuitMud { get; set; } = 0;
        public int LeadNTJournalist { get; set; } = 0;
        public int LeadNTZeroTwo { get; set; } = 0;
        public int LeadNTMud { get; set; } = 0;

        // Ortaklık sihirbazı: motorda henüz karşılığı olmayan seçenekler profilde saklanır (tam sayı alanları).
        // StrongNT, Stayman, MinorTransfer vb. bayraklarla birlikte çözümlenir; bayrak asıl kaynaktır.
        public int NT1Range { get; set; } = 5;                 // 0 10-12, 1 11-13, 2 12-14, 3 13-15, 4 14-16, 5 15-17, 6 16-18
        public int NT1Allow5422 { get; set; } = 1;
        public int NT1AllowSingletonHonor { get; set; } = 0;
        public int NT1Allow5Major { get; set; } = 1;
        public int NT1Allow6Minor { get; set; } = 1;
        public int NT1Puppet { get; set; } = 0;                // 1NT-2♣ Puppet Stayman
        public int NT1MinorBid { get; set; } = 0;              // 2 = 1NT-2♠ röle, 3 = süper kabullü minör transfer
        public int NT1DefenseX { get; set; } = 0;              // 1 Brozel, 2 Astro
        public int NT1RunoutX { get; set; } = 0;               // 3 D.O.N.T. kaçış, 4 Helvic
        public int MajorRaiseLimit { get; set; } = 0;          // 1 limit majör raise'ler
        public int GameTryShort { get; set; } = 0;             // 1 kısa renk denemesi
        public int NTSemiForcing { get; set; } = 0;            // 1 yarı zorlayıcı 1NT
        public int MinorShortClub { get; set; } = 0;           // 1 kısa sinek
        public int CheckbackCrowhurst { get; set; } = 0;
        public int FourthOneRound { get; set; } = 0;           // 1 dördüncü renk bir tur zorlayıcı
        public int LebensohlWeakTwo { get; set; } = 1;
        public int NT2Range { get; set; } = 1;                 // 0-4: 19-20 … 23-24; 5-8: 19-21 … 22-24
        public int NT2Natural { get; set; } = 0;               // 1 = 2NT-3♣ doğal
        public int GrandSlamForce { get; set; } = 1;
        public int ForumD { get; set; } = 0;
        public int NegDblLimit { get; set; } = 4;              // 0 1♠ … 4 2♠ … 8 3♠
        public int UnusualOverUnusual { get; set; } = 0;
        public int CueStyle { get; set; } = 0;                 // 1 batı, 2 doğu
        public int LaMajeure { get; set; } = 0;
        public int LeadDirectingDbl { get; set; } = 0;

        // ═══════════════════════════════════════════════════════════════════
        // AGRESİFLİK VE BECERİ (Bridge Baron tarzı profil ayarları)
        // -2 = Çok temkinli, -1 = Temkinli, 0 = Orta (varsayılan davranış), +1 = Atak, +2 = Çok atak
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Açılış: -1 yalnız 12+ HP, 0 12+ HP veya 11 HP + 20 kuralı, +1 10 HP + 20 kuralı da açar.</summary>
        public int AcilisAgresifligi { get; set; } = 0;
        /// <summary>Müdahale (overcall): -1 eşikler +2 HP, +1 eşikler -1 HP.</summary>
        public int MudahaleAgresifligi { get; set; } = 0;
        /// <summary>Önleyici açılış: -1 yalnız 0-8 HP ve 2+ honörlü 7'li renk, +1 10 HP'ye kadar.</summary>
        public int PreemptAgresifligi { get; set; } = 0;
        /// <summary>Kart oyunu becerisi 1-20. 20 = hatasız (varsayılan); düşükse yapay zekâ ara sıra rastgele yasal kart oynar.</summary>
        public int BeceriDuzeyi { get; set; } = 20;

        // Düzey ayarları (Bridge Baron "Level Settings"): karar başına en çok düşünme süresi, saniye (0-15).
        // Kart oyunu motoru (PIMC) bu süre içinde olabildiğince çok olası dağılım çözer; 0 = o elde sezgisel kurallar.
        // İhale ve açılış atağı kural/sezgisel tabanlıdır: bu iki ayar saklanır, karar vermeyi etkilemez.
        public int SureIhale { get; set; } = 1;
        public int SureAtak { get; set; } = 1;
        public int SureEl1 { get; set; } = 1;     // 1. elde atak dışındaki kartlar
        public int SureEl2 { get; set; } = 1;
        public int SureEl3 { get; set; } = 1;
        public int SureEl4 { get; set; } = 1;
        public int SureEl5 { get; set; } = 1;
        public int SureEl6 { get; set; } = 1;
        public int SureEl7 { get; set; } = 1;
        public int SureEl8 { get; set; } = 1;
        public int SureEl9 { get; set; } = 1;
        public int SureEl10 { get; set; } = 1;
        public int SureEl11 { get; set; } = 1;
        public int SureEl12 { get; set; } = 1;

        /// <summary>El başına süre sınırı (ms); dizin = biten el sayısı (0 = 1. el … 11 = 12. el).</summary>
        public int[] ElSureleriMs()
        {
            int[] s = { SureEl1, SureEl2, SureEl3, SureEl4, SureEl5, SureEl6, SureEl7, SureEl8, SureEl9, SureEl10, SureEl11, SureEl12 };
            for (int i = 0; i < s.Length; i++) s[i] = Math.Max(0, Math.Min(15, s[i])) * 1000;
            return s;
        }

        // ═══════════════════════════════════════════════════════════════════
        // FABRİKA METOTLARI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Varsayılan anlaşma + adı verilen ek (Bridge Baron) konvansiyonu açık; pratik modu için.</summary>
        public static OrtaklikAnlasmasi Varsayilan()
        {
            return new OrtaklikAnlasmasi
            {
                BesliMajor = true,
                InvertedMinors = true,
                ForcingNT = true,
                IkiBirOyunZorlama = true,
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

        /// <summary>
        /// "2/1 Game Force - Temel" (Bridge Baron "Two Over One - Basic"): 5'li majör, güçlü NT (15-17) + Stayman + Jacoby transfer
        /// + 1NT-2♠ röle (3♣ ile minör bırakma) + 5'li majörlü 1NT; limit majör raise'ler, zorlayıcı 1NT, uzun renk oyun denemeleri,
        /// Jacoby 2NT; 4. renk oyuna zorlar; zayıf iki, 2♣ güçlü (2♦ bekleme); Blackwood + kontrol gösteren cue-bid;
        /// Unusual 2NT, Michaels, 2♠'ya kadar negatif kontra, zayıf sıçramalı müdahale; 2NT 20-21 + Stayman.
        /// Resimde listelenmeyen konvansiyonlar kapalıdır; Konvansiyon listesinden eklenebilir.
        /// </summary>
        public static OrtaklikAnlasmasi IkiBirTemel()
        {
            var a = Varsayilan();
            // açık
            a.BesliMajor = true; a.ForcingNT = true; a.IkiBirOyunZorlama = true; a.StrongNT = true;
            a.IkiliSinekGuclu = true; a.ZayifIki = true; a.Preempt = true;
            a.Stayman = true; a.JacobyTransfer = true; a.TrialBid = true; a.FourthSuitForcing = true;
            a.Blackwood = true; a.Jacoby2NT = true; a.CueBid = true; a.Reverse = true;
            a.NegativeDouble = true; a.TakeoutDouble = true; a.Michaels = true; a.Unusual2NT = true; a.WeakJumpOvercalls = true;
            // kapalı (resimde yok)
            a.InvertedMinors = false; a.BergenRaises = false; a.Smolen = false; a.MinorTransfer = false; a.PuppetStayman = false;
            a.TwoWayCheckback = false; a.XYZ = false; a.ThirdSuitForcing = false; a.Dont = false; a.Serious3NT = false;
            a.RKCB = false; a.RKCB1430 = false; a.Gerber = false; a.Splinter = false; a.Drury = false;
            a.SupportDouble = false; a.ResponsiveDouble = false; a.Lebensohl = false;
            // seçenekler
            a.NT1Range = 5; a.NT1Allow5Major = 1; a.NT1MinorBid = 2; a.NT1Puppet = 0;
            a.MajorRaiseLimit = 1; a.GameTryShort = 0; a.NegDblLimit = 4; a.NT2Range = 6; a.LebensohlWeakTwo = 0;
            a.GrandSlamForce = 0;
            a.FeatureAsking2NT = true; a.Ogust = false;
            a.FitCueBid = true; a.OneMinorTwoNTInv = false; a.WeakJumpShiftResp = false; a.FourthOneRound = 0; a.Gambling3NT = false;   // resim: Feature asking 2NT (zayıf ikilere)
            return a;
        }

        /// <summary>
        /// "2/1 Game Force - Orta" (kullanıcı listesi): Temel'e ek olarak Inverted Minors, New Minor Forcing, Truscott 2NT,
        /// Splinter, Reverse Drury (Drury kapalı); Smolen, Texas, Minor Stayman, Lebensohl, Cappelletti; Puppet Stayman (2NT),
        /// Gambling 3NT; Unusual over Unusual, Sandwich NT, Responsive/Support DBL, Limit-Plus Cue-Bids, negatif DBL 3♠'ya kadar;
        /// RKCB, DOPI, Grand Slam Force, Voluntary 5 Major; zayıf ikilere karşı Lebensohl.
        /// </summary>
        public static OrtaklikAnlasmasi IkiBirOrta()
        {
            var a = IkiBirTemel();
            a.InvertedMinors = true; a.NewMinorForcing = true; a.Splinter = true;
            a.ReverseDrury = true; a.Drury = false; a.Truscott2NT = true;
            a.Smolen = true; a.TexasTransfers = true; a.MinorStayman = true; a.NT1MinorBid = 0; a.MinorTransfer = false;
            a.Lebensohl = true; a.Cappelletti = true; a.Dont = false;
            a.NT1OvercallSystemsOn = true; a.NTDefenseBalancing = true;
            a.PuppetStayman = true; a.Gambling3NT = true;
            a.UnusualOverUnusual = 1; a.Sandwich1NT = true; a.ResponsiveDouble = true; a.SupportDouble = true;
            a.LimitPlusCueBids = true; a.NegDblLimit = 8;
            a.RKCB = true; a.RKCB1430 = false; a.SpecificKingAsks = false; a.Gerber = false;
            a.DopiRopi = true; a.GrandSlamForce = 1; a.VoluntaryFiveMajor = true; a.LebensohlWeakTwo = 1;
            return a;
        }

        /// <summary>
        /// "Sasson (SAS - Kısa Sinek)": Orta profilin üzerine kısa ♣ açılışı (1♣ = 2+ kart) ve Sasson rekabet ekleri
        /// (Good-Bad 2NT, Inverted DBL, Mixed Raise, FSJS) açık gelir. Kitaptaki diğer röle/checkback sistemleri
        /// henüz motorda yoktur; bu profil mevcut konvansiyonlarla Sasson'a en yakın ayarı verir.
        /// </summary>
        public static OrtaklikAnlasmasi Sasson()
        {
            var a = IkiBirOrta();
            a.MinorShortClub = 1;
            a.GoodBad2NT = true; a.InvertedDbl = true; a.MixedRaise = true; a.FSJS = true;
            return a;
        }

        /// <summary>
        /// "2/1 Game Force - Gelişmiş" (kullanıcı listesi): içerik Orta ile aynıdır (Limit Major Raises, Reverse Drury,
        /// Truscott 2NT, Splinter, Texas, Minor Stayman, Lebensohl, Cappelletti, Puppet Stayman, Gambling 3NT, RKCB, DOPI, GSF, Feature Asking 2NT ...).
        /// </summary>
        public static OrtaklikAnlasmasi IkiBirGelismis()
        {
            return IkiBirOrta();
        }

        public static OrtaklikAnlasmasi Basit()
        {
            return new OrtaklikAnlasmasi
            {
                BesliMajor = false,
                InvertedMinors = false,
                ForcingNT = false,
                IkiBirOyunZorlama = false,
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
            string nt = (StrongNT ? "Strong NT (" : "Weak NT (") + NTA.Lo1(this) + "-" + NTA.Hi1(this) + ")";

            var aktifler = new System.Collections.Generic.List<string>();

            if (IkiliSinekGuclu) aktifler.Add("2♣ Güçlü");
            if (ZayifIki && !Multi2D) aktifler.Add("Zayıf İki");
            if (Multi2D) aktifler.Add("Multi 2♦");
            if (Preempt) aktifler.Add("Preempt");
            if (Namyats) aktifler.Add("Namyats");

            // 1♣ cevapları
            if (OneClub_AnswerSistemi) aktifler.Add("1♣ Cevap Sistemi");   // ← DEĞİŞTİ
            if (OneDiamond_AnswerSistemi) aktifler.Add("1♦ Cevap Sistemi");
            if (OneHeart_AnswerSistemi) aktifler.Add("1♥ Cevap Sistemi");
            if (OneSpade_AnswerSistemi) aktifler.Add("1♠ Cevap Sistemi");
            if (OneNT_AnswerSistemi) aktifler.Add("1NT Cevap Sistemi");
            if (TwoClub_AnswerSistemi) aktifler.Add("2♣ Cevap Sistemi");
            if (TwoDiamond_AnswerSistemi) aktifler.Add("2♦ Açılış Sistemi");
            if (TwoHeart_AnswerSistemi) aktifler.Add("2♥ Cevap Sistemi");
            if (TwoSpade_AnswerSistemi) aktifler.Add("2♠ Cevap Sistemi");
            if (TwoNT_AnswerSistemi) aktifler.Add("2NT Cevap Sistemi");
            if (Role2C_AnswerSistemi) aktifler.Add("2♣ Röle Sistemi");
            if (Araya_AnswerSistemi) aktifler.Add("Araya Giriş Cevapları");

            if (Stayman) aktifler.Add("Stayman");
            if (JacobyTransfer) aktifler.Add("Transfer");
            if (PuppetStayman) aktifler.Add("Puppet");
            if (Smolen) aktifler.Add("Smolen");
            if (MinorTransfer) aktifler.Add("Min.Transfer");
            if (TwoWayCheckback) aktifler.Add("2-Way Checkback");
            if (XYZ) aktifler.Add("XYZ");

            if (ForcingNT) aktifler.Add("Forcing NT");
            if (IkiBirOyunZorlama) aktifler.Add("2/1 Game Force");
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
            if (GoodBad2NT) ekler.Add("Good-Bad 2NT");
            if (InvertedDbl) ekler.Add("Inverted DBL");
            if (MixedRaise) ekler.Add("Mixed Raise");
            if (FSJS) ekler.Add("FSJS");
            if (PreemptiveReraises) ekler.Add("Preemptive Reraises");
            if (LongShortGameTries) ekler.Add("Long & Short Game Tries");
            if (NewMinorForcing) ekler.Add("New Minor Forcing");
            if (WeakJumpShiftResp) ekler.Add("Weak Jump-Shift cevapları");
            if (Ogust) ekler.Add("Ogust");
            if (FeatureAsking2NT) ekler.Add("Feature Asking 2NT");
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