using BricKartOyunu.Class;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;


namespace BricKartOyunu
{
    public partial class BricOyna : Form
    {
        // 4 oyuncu için kart dizileri
        private readonly PictureBox[] kuzeyKartlar = new PictureBox[13];
        private readonly PictureBox[] guneyKartlar = new PictureBox[13];
        private readonly PictureBox[] doguKartlar = new PictureBox[13];
        private readonly PictureBox[] batiKartlar = new PictureBox[13];


        // Eller için listeler
        private readonly List<Card> kuzeyEl = new List<Card>();
        private readonly List<Card> guneyEl = new List<Card>();
        private readonly List<Card> batiEl = new List<Card>();
        private readonly List<Card> doguEl = new List<Card>();

        // 🔹 SINIF SEVİYESİNDE KOORDİNATLAR VE BOYUTLAR
        private readonly int _kartGenislik = 90;
        private readonly int _kartYukseklik = 135;
        private int _kuzeyY = 0;
        private int _guneyY = 0;

        // INFO BOARD
        private Panel infoBoardPanel;
        private Label lblTour, lblNorth, lblEast, lblSouth, lblWest;
        private int _currentTour = 1;

        // OYUNCU ETİKETLERİ (Badges)
        private Label lblN_Name, lblE_Name, lblS_Name, lblW_Name;
        private Label lblN_Dir, lblE_Dir, lblS_Dir, lblW_Dir;

        // OYUN ALANI (Merkez Bölge)
        public Panel playZonePanel;

        // Sınıf seviyesine eklenecek alanlar
        private List<string> _sonIhaleGecmisi = new List<string>();
        private string _ihaleBaslangicOyuncusu = "Guney";

        // 🔹 Badge font'ları — tüm BricOyna örnekleri arasında paylaşılır.
        // Her CreateBadge çağrısında yeni Font yaratmak yerine tek instance.
        private static readonly Font _badgeDirFont = new Font("Arial", 8, FontStyle.Bold);
        private static readonly Font _badgeNameFont = new Font("Arial", 9);

        // IMAGE CACHING
        // 🔹 ÖNEMLİ: static olduğu için uygulama açık kaldığı sürece TEK SEFER diskten okunur.
        // Önceden BricOyna her açıldığında (ve her yeni el dağıtıldığında) 52 kart resmi
        // Image.FromFile ile diskten tekrar tekrar okunuyordu; bu da geçişlerin yavaş
        // hissedilmesinin ve ekranın "takılıp" masaüstünü göstermesinin ana nedeniydi.
        private static readonly Dictionary<string, Image> _cardImageCache = new Dictionary<string, Image>();
        private string _currentBackImagePath;
        public string CurrentBackImage
        {
            get => _currentBackImagePath;
            set { _currentBackImagePath = value; }
        }

        private static Image GetCachedImage(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (_cardImageCache.TryGetValue(path, out Image img)) return img;
            if (!File.Exists(path)) return null;

            // Dosyayı kilitlemeden oku (stream kapansa da Image kopyası hafızada kalır)
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var temp = Image.FromStream(fs))
            {
                img = new Bitmap(temp);
            }
            _cardImageCache[path] = img;
            return img;
        }
        /// <summary>
        /// Kart resim önbelleğini temizler ve tüm resim kaynaklarını serbest bırakır.
        /// Uygulama kapanmadan önce çağrılmalıdır. Idempotent'tir (birden çok kez
        /// çağrılabilir, ikinci çağrı hiçbir şey yapmaz).
        /// </summary>
        public static void CacheTemizle()
        {
            if (_cardImageCache == null || _cardImageCache.Count == 0) return;

            foreach (var img in _cardImageCache.Values)
            {
                img?.Dispose();
            }
            _cardImageCache.Clear();

            System.Diagnostics.Debug.WriteLine(
                "[BricOyna] Kart resim önbelleği temizlendi.");
        }

        private Player _activePlayer = Player.Guney;

        /// <summary>
        /// Aktif oyuncu. Set edildiğinde BtnGuney butonu ve oyuncu badge'leri
        /// otomatik olarak güncellenir. Böylece "activePlayer = X" sonrası
        /// tekrar tekrar BtnGuney.Enabled ve UpdatePlayerBadges() çağırmaya
        /// gerek kalmaz.
        /// </summary>
        private Player activePlayer
        {
            get => _activePlayer;
            set
            {
                if (_activePlayer == value) return;
                _activePlayer = value;

                if (BtnGuney != null)
                {
                    BtnGuney.Checked = false;
                    BtnGuney.Enabled = (_activePlayer != _humanPlayer);
                }

                UpdatePlayerBadges();
            }
        }
        private Player _declarer = Player.Guney;

        private Player _dealer = Player.Guney;   // Yeni elin dağıtanı (ihaleyi başlatan)

        // DeklarasyonForm ilk açıldığında hangi oyuncunun "eli" (deal eden / sırası gelen
        // oyuncu) olduğunu gösterebilmesi için dışarıya salt-okunur erişim sağlıyoruz.
        public Player ActivePlayer => activePlayer;

        private readonly Player _humanPlayer = Player.Guney;

        // 🔹 AI hamlesini geciktirmek için kullanılan timer.
        // Yerel değişken olarak tanımlanırsa GC tarafından toplanıp
        // AI oynamaz. Sınıf alanı olarak tutulmalı.
        private System.Windows.Forms.Timer _aiTimer;

        // ====================================================================
        // OYUN AŞAMASI
        // --------------------------------------------------------------------
        // BricOyna'nın hangi aşamada olduğunu belirtir. Kart tıklama vs.
        // sadece Oyun aşamasında çalışır. Başlangıç ve Deklarasyon
        // aşamalarında kart tıklaması reddedilir.
        // ====================================================================
        public enum OyunAsamasi
        {
            Baslangic,      // BricOyna açıldı, ihale henüz başlamadı (DekBasForm açık)
            Deklarasyon,    // DeklarasyonForm açık, ihale devam ediyor
            Oyun            // İhale bitti, kart oynanıyor
        }

        // 🔹 Mevcut oyun aşaması — kart tıklama kontrolü için kullanılır
        private OyunAsamasi _oyunAsamasi = OyunAsamasi.Baslangic;

        // ====================================================================
        // SIRA TAKİBİ — Oyun Sırası
        // ====================================================================
        // Bu elde masaya oynanan kartlar (sıra ile) — el tamamlanınca temizlenir
        private readonly List<(Player Oyuncu, Card Kart)> _buEldeOynananlar
            = new List<(Player, Card)>();

        // Atak rengi (bu elde ilk oynanan kartın rengi) — 4 kart tamamlanınca sıfırlanır
        private string _buEldeAtakRengi = null;

        // 🔹 El bitti mi? (Kullanıcı PlayZone'a tıklayınca yeni el başlayacak)
        private bool _elBittiBekliyor = false;

        // 🔹 Son elin kazananı (PlayZone tıklamasında yeni eli başlatacak)
        private Player _sonElKazanani = Player.Guney;

        // 🔹 Bu bordda kaç el oynandı? (13'e ulaşınca bord biter)
        private int _oynananToplamEl = 0;

        // 🔹 Bu bordda oynanan tüm eller — OynananElleriGosterForm için saklanır
        // Tuple: (ElNo, Oyuncu, Kart, KazandiMi)
        private readonly List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)> _oynananTumEller
            = new List<(int, Player, Card, bool)>();

        // ====================================================================
        // OYUN OYNAMA — Masa Kartları
        // ====================================================================
        // Masa kartı boyutu (elden biraz küçük — 4 kart masada sığsın)
        private const int MasaKartGenislik = 70;
        private const int MasaKartYukseklik = 105;

        // Masa kartlarının playZonePanel içindeki göreli konumları (merkez noktaları)
        // Bu değerleri değiştirerek kartların masada nerede duracağını ayarlayabilirsin.
        private static readonly Point MasaPozisyonGuney = new Point(215, 380);  // alt orta
        private static readonly Point MasaPozisyonKuzey = new Point(215, 15);   // üst orta
        private static readonly Point MasaPozisyonBati = new Point(15, 200);    // sol orta
        private static readonly Point MasaPozisyonDogu = new Point(415, 200);   // sağ orta

        // Şu anki elin masadaki kartları (4 kart — bir el tamamlanınca temizlenir)
        private readonly Dictionary<Player, PictureBox> _masadakiKartlar
            = new Dictionary<Player, PictureBox>();

        // 🔹 Kart karıştırma için tek Random instance. Her dağıtımda yeni Random()
        // oluşturmak, aynı milisaniyede yapılan ardışık çağrılarda aynı seed'e
        // düşüp aynı dağıtıma sebep olabiliyordu. Tek instance bunu engeller.
        private static readonly Random _rng = new Random();



        public BricOyna()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                this.DoubleBuffered = true;
                this.SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, true);
                this.UpdateStyles();
            }
            this.Load += BricOyna_Load;
            this.Resize += BricOyna_Resize;

            // 🔹 Geri Al / İleri Al butonlarının tıklama olaylarını bağla
            if (BtnGeriAl != null)
                BtnGeriAl.Click += BtnGeriAl_Click;
            if (BtnileriAl != null)
                BtnileriAl.Click += BtnileriAl_Click;

            // 🔹 YENİ: ToolStrip butonlarında "ilk tıklama odağa gidiyor" sorununu çözmek için
            // Click yerine MouseDown kullan. Click bağlamasını çıkar.
            FirstClickSorununuCoz(BtnHepsi, BtnHepsi_Click);
            FirstClickSorununuCoz(BtnEW, BtnEW_Click);
            FirstClickSorununuCoz(BtnNS, BtnNS_Click);
            FirstClickSorununuCoz(BtnGuney, BtnGuney_Click);
            FirstClickSorununuCoz(BtnGeriAl, BtnGeriAl_Click);
            FirstClickSorununuCoz(BtnileriAl, BtnileriAl_Click);
            FirstClickSorununuCoz(BtnSonrakiEl, BtnSonrakiEl_Click);
        }

        /// <summary>
        /// Bir ToolStrip butonunda "ilk tıklama odağa gidiyor" sorununu çözer.
        /// Click bağlamasını çıkarıp MouseDown bağlar.
        /// </summary>
        private static void FirstClickSorununuCoz(ToolStripItem item, EventHandler handler)
        {
            if (item == null || handler == null) return;
            item.Click -= handler;                  // Designer'dan gelen Click bağlamasını çıkar
            item.MouseDown += (s, e) =>              // MouseDown bağla
            {
                if (e.Button == MouseButtons.Left)
                    handler(s, e);
            };
        }

        // ⚠️ NOT: WS_EX_COMPOSITED kaldırıldı.
        // Bu form onlarca PictureBox/Label/Panel içeriyor; WS_EX_COMPOSITED, DWM'e her
        // güncellemede TÜM pencere ağacının offscreen bir kopyasını oluşturtur. Az sayıda
        // kontrolde flicker'ı azaltır ama bu kadar çok child control ile tam tersine ekranı
        // "titretip" geçişleri yavaşlatan asıl sebeplerden biri budur. Standart double-buffer
        // (aşağıdaki SetStyle) + SuspendLayout/ResumeLayout kombinasyonu bu form için yeterli.

        // Panel gibi kontroller varsayılan olarak double-buffered DEĞİLDİR; içine çok sayıda
        // PictureBox eklenip taşındığında (KartYerlesimiHazirla) bu, gözle görülür titremeye
        // sebep olur. Reflection ile ilgili panelin DoubleBuffered stilini açıyoruz.
        private static void EnableDoubleBuffer(Control c)
        {
            if (c == null) return;
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, c, new object[] { true });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // TOOLBAR BUTON RESİMLERİ — Basılı / Basılmamış
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Resources/ToolBarImage klasöründeki bir resmi yükler.
        /// </summary>
        private static Image ToolbarResmiYukle(string dosyaAdi)
        {
            try
            {
                string yol = Path.Combine(
                    Application.StartupPath, "Resources", "ToolBarImage", dosyaAdi);
                if (!File.Exists(yol)) return null;

                using (var fs = new FileStream(yol, FileMode.Open, FileAccess.Read))
                using (var temp = Image.FromStream(fs))
                {
                    return new Bitmap(temp);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ToolbarResmiYukle] {dosyaAdi} yüklenemedi: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Bir ToolStripButton için basılı/basılmamış resim çiftini ayarlar.
        /// Checked durumuna göre resim otomatik değişir.
        /// </summary>
        private void ToolbarButonResimleriniAyarla(
            ToolStripButton btn, string basilmisResim, string basilmamisResim)
        {
            if (btn == null) return;

            Image altResim = ToolbarResmiYukle(basilmisResim);
            Image ustResim = ToolbarResmiYukle(basilmamisResim);

            if (altResim == null && ustResim == null) return;

            // 🔹 Resmi ToolStrip boyutuna otomatik ölçekle
            btn.ImageScaling = ToolStripItemImageScaling.SizeToFit;

            // Başlangıç: basılmamış resim
            btn.Image = ustResim ?? btn.Image;

            // Tag'e her iki resmi de sakla
            btn.Tag = new ToolbarResimCifti
            {
                Alt = altResim,
                Ust = ustResim
            };

            // Checked değiştiğinde resmi değiştir (önce çıkar, sonra ekle)
            btn.CheckedChanged -= ToolbarButon_CheckedChanged;
            btn.CheckedChanged += ToolbarButon_CheckedChanged;

            // Başlangıç durumuna göre resmi ayarla
            ToolbarButon_CheckedChanged(btn, EventArgs.Empty);
        }

        /// <summary>
        /// Toolbar butonunun Checked durumuna göre resmini değiştirir.
        /// </summary>
        private void ToolbarButon_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is ToolStripButton btn)) return;
            if (!(btn.Tag is ToolbarResimCifti cift)) return;

            btn.Image = btn.Checked ? cift.Alt : cift.Ust;
        }

        /// <summary>
        /// Basılı / basılmamış resim çiftini tutar.
        /// </summary>
        private class ToolbarResimCifti
        {
            public Image Alt { get; set; }   // Basılı (Checked=true)
            public Image Ust { get; set; }   // Basılmamış (Checked=false)
        }
        
        
                
        /// <summary>
        /// Basılı / basılmamış resim çiftini tutar.
        /// </summary>
               private void BricOyna_Resize(object sender, EventArgs e)
        {
            this.SuspendLayout();
            try
            {
                KartYerlesimiHazirla();
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        private void BricOyna_Load(object sender, EventArgs e)
        {


            if (this.DesignMode) return;

            // 🔹 ToolStrip butonlarının görsel boyutunu büyüt (32x32 → 48x48)
            toolStrip1.ImageScalingSize = new Size(32, 32);
            toolStrip1.AutoSize = false;
            toolStrip1.Height = 40;

            // 🔹 Toolbar butonlarının basılı/basılmamış resimlerini ayarla
            ToolbarButonResimleriniAyarla(BtnHepsi, "allHandsalt.png", "allHandsUst.png");
            ToolbarButonResimleriniAyarla(BtnEW, "EWalt.png", "EWUst.png");
            ToolbarButonResimleriniAyarla(BtnNS, "NSalt.png", "NSUst.png");
            ToolbarButonResimleriniAyarla(BtnGuney, "Southalt.png", "SouthUst.png");

            _oyunAsamasi = OyunAsamasi.Baslangic;   // ← EKLE

            this.BackColor = AnaSayfa.MasaRengi;

            // İlk elin dağıtanı Güney'dir
            _dealer = Player.Guney;
            activePlayer = _dealer;
            _declarer = _dealer;   // İhale henüz yok; geçici

            // Aktif oyuncu Güney ise "aktif oyuncu" butonu pasif olmalı
            BtnGuney.Checked = false;
            BtnGuney.Enabled = (activePlayer != _humanPlayer);

            //Butonlar inaktif yapılıyor
            ButonlariInaktifYap();   // <-- 7 buton burada inaktif yapılıyor


            KartlariDagit(AnaSayfa.SeciliKartSeti);
            UpdateInfoBoard(_dealer);
            UpdatePlayerBadges();

            // 🔹 YENİ: Oyun bilgi panelini güncelle (Dealer, Zon görünsün)
            OyunBilgiPaneliniGuncelle();

            // 🔹 TEST: İhale motoru çalışıyor mu?
            TestIhaleMotoru();

            YeniDekBasFormAc();
        }

        /// <summary>
        /// Test amaçlı — ihale motorunu çalıştırır ve Debug'a yazar.
        /// Gerçek entegrasyon sonraki adımda yapılacak.
        /// </summary>
        private void TestIhaleMotoru()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════");
                System.Diagnostics.Debug.WriteLine("[TEST] İhale Motoru Testi");
                System.Diagnostics.Debug.WriteLine("═══════════════════════");

                var anlasma = OrtaklikAnlasmasi.Varsayilan();
                var motor = new IhaleMotoru(anlasma);

                System.Diagnostics.Debug.WriteLine($"[TEST] {motor.Ozet()}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 1: Gerçek eller
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- GERÇEK ELLER ---");

                Player[] oyuncular = { Player.Guney, Player.Bati, Player.Kuzey, Player.Dogu };
                foreach (var oyuncu in oyuncular)
                {
                    var el = OyuncuEli(oyuncu);
                    if (el == null || el.Count != 13) continue;

                    var durum = new IhaleDurumu
                    {
                        AktifOyuncu = oyuncu,
                        AktifOyuncuEli = el,
                        Anlasma = anlasma
                    };

                    string teklif = motor.TeklifVer(durum);
                    string ozet = ElDegerlendirici.Ozet(el);

                    System.Diagnostics.Debug.WriteLine(
                        $"[TEST] {oyuncu} ({ozet}) → {teklif}");
                }

                // ═══════════════════════════════════════════════════════════════
                // TEST 2: Yapay eller (BesliMajor'ı test et)
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- YAPAY ELLER ---");

                // El 1: 15 HP + 5'li Maça → 1♠ beklenir
                var el1 = YapayElOlustur(
                    "♠AKQ54 ♥KJ3 ♦Q2 ♣T87");
                TestTekEl(motor, el1, "Yapay-1: 15 HP + 5'li Maça");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♠, Gelen: {motor.TeklifVer(DurumOlustur(el1, anlasma))}");

                // El 2: 15 HP + 5'li Kupa → 1♥ beklenir
                var el2 = YapayElOlustur(
                    "♠KJ3 ♥AKQ54 ♦Q2 ♣T87");
                TestTekEl(motor, el2, "Yapay-2: 15 HP + 5'li Kupa");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♥, Gelen: {motor.TeklifVer(DurumOlustur(el2, anlasma))}");

                // El 3: 13 HP ama 5'li majör yok → fallback (1♦ veya 1♣)
                var el3 = YapayElOlustur(
                    "♠KJ3 ♥QJ3 ♦AKQ5 ♣J87");
                TestTekEl(motor, el3, "Yapay-3: 17 HP dengeli (StrongNT aralığı)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1NT, Gelen: {motor.TeklifVer(DurumOlustur(el3, anlasma))}");

                // El 4: 8 HP → Pas beklenir
                var el4 = YapayElOlustur(
                    "♠KJ3 ♥Q43 ♦J52 ♣J87");
                TestTekEl(motor, el4, "Yapay-4: 8 HP");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: Pas, Gelen: {motor.TeklifVer(DurumOlustur(el4, anlasma))}");

                // El 5: 16 HP + dengeli (5'li majör yok) → 1NT beklenir
                var el5 = YapayElOlustur(
                    "♠KJ3 ♥QJ3 ♦AKQ5 ♣J87");
                TestTekEl(motor, el5, "Yapay-5: 16 HP dengeli, 5'li majör yok");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1NT, Gelen: {motor.TeklifVer(DurumOlustur(el5, anlasma))}");

                // El 6: 15 HP + 5'li Maça → BesliMajor (1♠) beklenir
                var el6 = YapayElOlustur(
                    "♠AKQ54 ♥QJ3 ♦Q2 ♣J87");
                TestTekEl(motor, el6, "Yapay-6: 15 HP + 5'li Maça (BesliMajor önce)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♠, Gelen: {motor.TeklifVer(DurumOlustur(el6, anlasma))}");

                // El 7: 22+ HP → 2♣ beklenir
                var el7 = YapayElOlustur(
    "♠AKQ54 ♥AKQ3 ♦AK ♣T8");
                TestTekEl(motor, el7, "Yapay-7: 22 HP → 2♣ (yapay güçlü)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 2♣, Gelen: {motor.TeklifVer(DurumOlustur(el7, anlasma))}");

                // El 8: 24 HP + 5-5 → 2♣ beklenir
                var el8 = YapayElOlustur(
    "♠AKQ54 ♥AKQJ3 ♦KQ ♣T");
                TestTekEl(motor, el8, "Yapay-8: 24 HP + 5-5 → 2♣ (9 tricks)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 2♣, Gelen: {motor.TeklifVer(DurumOlustur(el8, anlasma))}");

                // El 9: 20 HP + 5'li Maça → BesliMajor (1♠) beklenir (2♣ değil!)
                var el9 = YapayElOlustur(
    "♠AKQ54 ♥KQ3 ♦KQ2 ♣T8");
                TestTekEl(motor, el9, "Yapay-9: 20 HP + 5'li Maça → 1♠ (2♣ değil)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♠, Gelen: {motor.TeklifVer(DurumOlustur(el9, anlasma))}");

                // El 10: 13 HP + 3-3-4-3 → 1♦ beklenir (MinörAcilis)
                var el10 = YapayElOlustur("♠KJ3 ♥Q43 ♦KQ52 ♣J87");
                TestTekEl(motor, el10, "Yapay-10: 13 HP dengeli, 4'lü Karo");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♦, Gelen: {motor.TeklifVer(DurumOlustur(el10, anlasma))}");

                // El 11: 12 HP + 3-3-4-3 → 1♦ beklenir
                var el11 = YapayElOlustur("♠KJ3 ♥Q43 ♦QJ52 ♣AJ8");
                TestTekEl(motor, el11, "Yapay-11: 12 HP, 4'lü Karo");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♦, Gelen: {motor.TeklifVer(DurumOlustur(el11, anlasma))}");

                // El 12: 14 HP + 3-4-4-3 → 1♦ beklenir (karo ≥ sinek)
                var el12 = YapayElOlustur("♠KJ3 ♥QJ4 ♦KQ52 ♣J87");
                TestTekEl(motor, el12, "Yapay-12: 14 HP, 4'lü Karo, 4'lü Sinek");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♦, Gelen: {motor.TeklifVer(DurumOlustur(el12, anlasma))}");

                // ─── BasitCevap Testleri ───

                // Yapay-13: Partner 1♠ açtı, ben 8 HP + 3 Maça → 2♠ beklenir
                var el13 = YapayElOlustur("♠J43 ♥Q43 ♦J52 ♣QJ87");
                var durum13 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el13,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-13: Partner 1♠, ben 8 HP + 3 Maça → 2♠ beklenir");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el13)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum13)}");

                // Yapay-14: Partner 1♠ açtı, ben 14 HP + 5 Kupa → 2♥ beklenir (2/1 GF)
                var el14 = YapayElOlustur("♠J43 ♥AKQ54 ♦QJ2 ♣87");
                var durum14 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el14,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-14: Partner 1♠, ben 14 HP + 5 Kupa → 2♥ beklenir (2/1 GF)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el14)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum14)}");

                // Yapay-15: Partner 1♠ açtı, ben 12 HP dengeli → 2NT beklenir
                var el15 = YapayElOlustur("♠J43 ♥QJ4 ♦KQ52 ♣AJ8");
                var durum15 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el15,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-15: Partner 1♠, ben 12 HP dengeli → 2NT beklenir");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el15)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum15)}");
                System.Diagnostics.Debug.WriteLine("═══════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST] HATA: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Test için tek el çalıştırır.
        /// </summary>
        private void TestTekEl(IhaleMotoru motor, List<Card> el, string aciklama)
        {
            System.Diagnostics.Debug.WriteLine($"[TEST] {aciklama}");
            System.Diagnostics.Debug.WriteLine($"       {ElDegerlendirici.Ozet(el)}");
        }

        /// <summary>
        /// Test için el oluşturur.
        /// Format: "♠AKQ54 ♥KJ3 ♦Q2 ♣T87"
        /// </summary>
        private List<Card> YapayElOlustur(string metin)
        {
            var el = new List<Card>();
            string[] renkGruplari = metin.Split(' ');

            foreach (var grup in renkGruplari)
            {
                if (string.IsNullOrEmpty(grup) || grup.Length < 2) continue;

                char renkSembolu = grup[0];
                string renk = "";
                switch (renkSembolu)
                {
                    case '♠': renk = "Maça"; break;
                    case '♥': renk = "Kupa"; break;
                    case '♦': renk = "Karo"; break;
                    case '♣': renk = "Sinek"; break;
                    default: continue;
                }

                string kartlar = grup.Substring(1);
                foreach (char k in kartlar)
                {
                    int deger;
                    switch (k)
                    {
                        case 'A': deger = 14; break;
                        case 'K': deger = 13; break;
                        case 'Q': deger = 12; break;
                        case 'J': deger = 11; break;
                        case 'T': deger = 10; break;
                        default:
                            if (!int.TryParse(k.ToString(), out deger)) continue;
                            break;
                    }
                    el.Add(new Card { Suit = renk, Value = deger });
                }
            }

            return el;
        }

        /// <summary>
        /// Test için ihale durumu oluşturur.
        /// </summary>
        private IhaleDurumu DurumOlustur(List<Card> el, OrtaklikAnlasmasi anlasma)
        {
            return new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = anlasma
            };
        }

        // 🔹 DekBasForm'u oluşturup playZonePanel'in tam ortasında gösterir.
        // Hem ilk açılışta (BricOyna_Load) hem de "Sonraki el" ile yeni bir
        // dağılıma geçildiğinde aynı mantık kullanıldığı için ayrı bir metoda alındı.
        private void YeniDekBasFormAc()
        {
            try
            {
                if (playZonePanel == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[BricOyna] YeniDekBasFormAc: playZonePanel null, DekBasForm açılamadı.");
                    return;
                }

                DekBasForm dekForm = new DekBasForm(this)
                {
                    StartPosition = FormStartPosition.Manual
                };

                // playZonePanel'in ekran üzerindeki merkezini bul
                Point panelCenterScreen = playZonePanel.PointToScreen(
                    new Point(
                        playZonePanel.Width / 2,
                        playZonePanel.Height / 2
                    ));

                // DekBasForm'un merkezini playZonePanel merkezine getir
                dekForm.Location = new Point(
                    panelCenterScreen.X - dekForm.Width / 2,
                    panelCenterScreen.Y - dekForm.Height / 2
                );

                // BricOyna'nın sahibi olduğu form olarak aç
                dekForm.Show(this);
                dekForm.BringToFront();
                dekForm.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[BricOyna] DekBasForm açılamadı: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // 🔹 "Sonraki el" onaylandığında senaryoyu sıfırdan başlatır: aktif oyuncu/eli
        // Güney'e döner, tur sayacı sıfırlanır, kartlar yeniden dağıtılır ve yeni bir
        // DekBasForm açılır. BricOyna_Load ile aynı başlangıç durumunu üretir.


        // 🔹 DeklarasyonForm (ya da ileride açılabilecek başka bir alt form) ekrandayken
        // BricOyna üzerinde sadece menüstrip ve toolstrip aktif kalmalı; oyun alanındaki
        // her şey (kartlar, playZonePanel, info board, badge'ler vb.) pasifleştirilmeli.
        // Designer'daki değişken adlarına bağımlı kalmamak için tip kontrolü kullanılıyor:
        // MenuStrip/ToolStrip DIŞINDAKİ tüm doğrudan child kontroller etkilenir.
        public void SetGameControlsEnabled(bool enabled)
        {
            foreach (Control c in this.Controls)
            {
                if (c is MenuStrip || c is ToolStrip) continue;
                c.Enabled = enabled;
            }
        }

        public Point GetPlayZoneCenterScreen()
        {
            if (playZonePanel == null)
                return Point.Empty;

            Point panelScreenLocation = playZonePanel.PointToScreen(Point.Empty);

            int centerX = panelScreenLocation.X + (playZonePanel.Width / 2);
            int centerY = panelScreenLocation.Y + (playZonePanel.Height / 2);

            return new Point(centerX, centerY);
        }
        private int GetCenteredEWStartingY()
        {
            return _kuzeyY + 172;
        }

        private void KartYerlesimiHazirla()
        {
            int maxEWToplamYukseklik = (4 * _kartYukseklik) + (3 * 2);
            int toplamBlokYukseklik = _kartYukseklik + maxEWToplamYukseklik + _kartYukseklik;

            int ustSınır = 40;
            int dikeyKaydırma = (this.ClientSize.Height - toplamBlokYukseklik) / 2;
            if (dikeyKaydırma < ustSınır) dikeyKaydırma = ustSınır;

            _kuzeyY = dikeyKaydırma;
            int ewBaslangicY = _kuzeyY + 172;
            _guneyY = ewBaslangicY + maxEWToplamYukseklik + 10;

            float dikeyAdim = (maxEWToplamYukseklik - _kartYukseklik) / 12f;
            if (dikeyAdim < 0) dikeyAdim = 0;

            int batiX = 40; // Batı sabit kaldı

            // 1. BATI KARTLARI
            for (int i = 0; i < 13; i++)
            {
                if (batiKartlar[i] == null)
                {
                    batiKartlar[i] = new PictureBox { Width = _kartGenislik, Height = _kartYukseklik, SizeMode = PictureBoxSizeMode.StretchImage, BorderStyle = BorderStyle.FixedSingle };
                    this.Controls.Add(batiKartlar[i]);
                }
                batiKartlar[i].Left = batiX;
                batiKartlar[i].Top = ewBaslangicY + (int)(i * dikeyAdim);
                batiKartlar[i].Visible = true;
                batiKartlar[i].BringToFront();
            }

            int batiBitisX = batiX + _kartGenislik;
            int kuzeyX = 250;
            int guneyX = 250;

            int yanBosluk = kuzeyX - batiBitisX;

            // Kuzey elini dizip bittiği dinamik ucu alıyoruz
            int kuzeyBitisX = BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, kuzeyX, _kuzeyY, false, false);

            // Doğu eli, Kuzey'in bitiş hizasına göre simetrik konumlanıyor
            int doguX = kuzeyBitisX + yanBosluk;

            // 3. DOĞU KARTLARI
            for (int i = 0; i < 13; i++)
            {
                if (doguKartlar[i] == null)
                {
                    doguKartlar[i] = new PictureBox { Width = _kartGenislik, Height = _kartYukseklik, SizeMode = PictureBoxSizeMode.StretchImage, BorderStyle = BorderStyle.FixedSingle };
                    this.Controls.Add(doguKartlar[i]);
                }
                doguKartlar[i].Left = doguX;
                doguKartlar[i].Top = ewBaslangicY + (int)(i * dikeyAdim);
                doguKartlar[i].Visible = true;
                doguKartlar[i].BringToFront();
            }

            // 2. KUZEY KARTLARI (İlklendirme)
            for (int i = 0; i < 13; i++)
            {
                if (kuzeyKartlar[i] == null)
                {
                    kuzeyKartlar[i] = new PictureBox { Width = _kartGenislik, Height = _kartYukseklik, SizeMode = PictureBoxSizeMode.StretchImage, BorderStyle = BorderStyle.FixedSingle };
                    this.Controls.Add(kuzeyKartlar[i]);
                }
            }

            // 4. GÜNEY KARTLARI (İlklendirme ve dizim)
            for (int i = 0; i < 13; i++)
            {
                if (guneyKartlar[i] == null)
                {
                    guneyKartlar[i] = new PictureBox
                    {
                        Width = _kartGenislik,
                        Height = _kartYukseklik,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        BorderStyle = BorderStyle.FixedSingle,
                        Cursor = Cursors.Hand   // 🔹 Tıklanabilir hissi
                    };
                    this.Controls.Add(guneyKartlar[i]);

                    // 🔹 Tıklama event'i — closure için index'i kopyala
                    int kartIndex = i;
                    guneyKartlar[i].Click += (s, ev) => GuneyKartTiklandi(kartIndex);
                }
            }
            BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, guneyX, _guneyY, false, false);

            SetupInfoBoard();
            SetupPlayerBadges();
            SetupPlayZone();

            // 🔹 YENİ: Oyun bilgi panelini oluştur ve sağ üste konumlandır
            SetupOyunBilgiPanel();
            if (oyunBilgiPanel != null)
            {
                oyunBilgiPanel.Location = new Point(
                    this.ClientSize.Width - oyunBilgiPanel.Width - BilgiPanelSagBosluk,
                    _kuzeyY   // ← Kuzey elinin Y koordinatı
                );
                OyunBilgiPaneliniGuncelle();
            }

            int guneyBitisX = guneyX + (12 * (_kartGenislik / 2)) + _kartGenislik;
            int btnY = this.ClientSize.Height - 70;
            btnGeri.Left = guneyBitisX + 10;
            btnGeri.Top = btnY;
            btnGeri.BringToFront();
        }

        private void SetupInfoBoard()
        {
            // Infoboard boyutunu büyütüyoruz
            int size = 50;
            int panelX = 20;
            int panelY = _kuzeyY;   // ← Kuzey elinin Y koordinatı

            if (infoBoardPanel == null)
            {
                infoBoardPanel = new Panel { Location = new Point(panelX, panelY), Size = new Size(size * 3, size * 3), BackColor = Color.Transparent };
                EnableDoubleBuffer(infoBoardPanel);

                lblTour = new Label { Text = _currentTour.ToString(), Size = new Size(size, size), Location = new Point(size, size), BackColor = Color.LightGray, TextAlign = ContentAlignment.MiddleCenter, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Arial", 14F, FontStyle.Bold) };
                lblNorth = CreateInfoBox(size, size, 0);
                lblEast = CreateInfoBox(size, size * 2, size);
                lblSouth = CreateInfoBox(size, size, size * 2);
                lblWest = CreateInfoBox(size, 0, size);

                infoBoardPanel.Controls.Add(lblTour);
                infoBoardPanel.Controls.Add(lblNorth);
                infoBoardPanel.Controls.Add(lblEast);
                infoBoardPanel.Controls.Add(lblSouth);
                infoBoardPanel.Controls.Add(lblWest);
                this.Controls.Add(infoBoardPanel);
            }
            infoBoardPanel.Location = new Point(panelX, panelY);
            infoBoardPanel.BringToFront();
        }

        /// <summary>
        /// Sağ üst köşede oyun bilgi panelini (Dealer, Zon, Kontrat, Deklaran,
        /// Kazanılan El) oluşturur. Panel her zaman görünür.
        /// </summary>
        private void SetupOyunBilgiPanel()
        {
            if (oyunBilgiPanel != null) return;

            oyunBilgiPanel = new Panel
            {
                Size = new Size(BilgiPanelGenislik, BilgiPanelYukseklik),
                BackColor = Color.FromArgb(245, 245, 220),
                BorderStyle = BorderStyle.FixedSingle
            };
            EnableDoubleBuffer(oyunBilgiPanel);

            // ─── Başlık ───
            var lblBaslik = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Text = "OYUN BİLGİSİ",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 90)
            };
            oyunBilgiPanel.Controls.Add(lblBaslik);

            // ─── İçerik paneli ───
            var icerik = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 6, 8, 6),
                BackColor = Color.Transparent
            };
            oyunBilgiPanel.Controls.Add(icerik);
            icerik.BringToFront();

            // ─── Bilgi satırları ───
            int y = 0;
            const int satirYuksekligi = 22;

            lblBilgiTur = BilgiSatiriOlustur(icerik, "Tur:", y); y += satirYuksekligi;
            lblBilgiDealer = BilgiSatiriOlustur(icerik, "Dealer:", y); y += satirYuksekligi;
            lblBilgiZon = BilgiSatiriOlustur(icerik, "Zon:", y); y += satirYuksekligi;
            lblBilgiKontrat = BilgiSatiriOlustur(icerik, "Kontrat:", y); y += satirYuksekligi;
            lblBilgiDeklaran = BilgiSatiriOlustur(icerik, "Deklaran:", y); y += satirYuksekligi + 4;

            // ─── Kazanılan El başlığı ───
            var lblKazanilanBaslik = new Label
            {
                Location = new Point(8, y),
                Size = new Size(icerik.Width - 16, 20),
                Text = "Kazanılan El",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
            icerik.Controls.Add(lblKazanilanBaslik);
            y += 22;

            lblBilgiKazanilanEl = new Label
            {
                Location = new Point(8, y),
                Size = new Size(icerik.Width - 16, 22),
                Text = "NS: 0   •   EW: 0",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                TextAlign = ContentAlignment.MiddleCenter
            };
            icerik.Controls.Add(lblBilgiKazanilanEl);

            this.Controls.Add(oyunBilgiPanel);
            oyunBilgiPanel.BringToFront();
        }

        /// <summary>
        /// Etiket + değer satırı oluşturur (örn. "Dealer: Kuzey").
        /// </summary>
        private Label BilgiSatiriOlustur(Panel parent, string baslik, int y)
        {
            var lblBaslik = new Label
            {
                Location = new Point(8, y),
                Size = new Size(70, 20),
                Text = baslik,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(lblBaslik);

            var lblDeger = new Label
            {
                Location = new Point(80, y),
                Size = new Size(parent.Width - 88, 20),
                Text = "—",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(lblDeger);

            return lblDeger;
        }

        /// <summary>
        /// Oyun bilgi panelini günceller.
        /// </summary>
        private void OyunBilgiPaneliniGuncelle()
        {
            if (oyunBilgiPanel == null) return;

            // 🔹 Tur
            if (lblBilgiTur != null)
                lblBilgiTur.Text = _currentTour.ToString();

            // Dealer
            if (lblBilgiDealer != null)
                lblBilgiDealer.Text = PlayerIsmiTurkce(_dealer);

            // Zon
            if (lblBilgiZon != null)
                lblBilgiZon.Text = ZonDurumuMetni();

            // Kontrat
            if (lblBilgiKontrat != null)
                lblBilgiKontrat.Text = string.IsNullOrEmpty(_kontrat) ? "—" : _kontrat;

            // Deklaran
            if (lblBilgiDeklaran != null)
                lblBilgiDeklaran.Text = string.IsNullOrEmpty(_kontrat)
                    ? "—"
                    : PlayerIsmiTurkce(_kontratDeklaran);

            // Kazanılan El
            if (lblBilgiKazanilanEl != null)
                lblBilgiKazanilanEl.Text = $"NS: {_kazanilanElNS}   •   EW: {_kazanilanElEW}";

            oyunBilgiPanel.BringToFront();
        }

        private Label CreateInfoBox(int size, int x, int y)
        {
            return new Label { Size = new Size(size, size), Location = new Point(x, y), TextAlign = ContentAlignment.MiddleCenter, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Arial", 12F, FontStyle.Bold) };
        }

        public void UpdateInfoBoard(Player deklaran)
        {
            _declarer = deklaran;
            if (lblTour == null || lblNorth == null || lblEast == null ||
                lblSouth == null || lblWest == null) return;

            lblTour.Text = _currentTour.ToString();

            // 16 Bordluk Standart Briç Zon Döngüsü Hesaplaması:
            // _currentTour değerinin 1-16 arası modunu alıyoruz.
            int boardMod = ((_currentTour - 1) % 16) + 1;

            // Standard Briç Zon Tanımları (16'lı Bord Döngüsü):
            // Zonsuz (Nobody): 1, 8, 11, 14
            // N-S Zonda: 2, 5, 12, 15
            // E-W Zonda: 3, 6, 9, 13
            // Herkes Zonda (Both): 4, 7, 10, 16
            bool nsZonda = (boardMod == 2 || boardMod == 5 || boardMod == 12 || boardMod == 15 ||
                            boardMod == 4 || boardMod == 7 || boardMod == 10 || boardMod == 16);

            bool ewZonda = (boardMod == 3 || boardMod == 6 || boardMod == 9 || boardMod == 13 ||
                            boardMod == 4 || boardMod == 7 || boardMod == 10 || boardMod == 16);

            UpdateBox(lblNorth, nsZonda);
            UpdateBox(lblSouth, nsZonda);
            UpdateBox(lblEast, ewZonda);
            UpdateBox(lblWest, ewZonda);

            // 🔹 YENİ: Oyun bilgi panelindeki Zon ve Dealer'ı da güncelle
            OyunBilgiPaneliniGuncelle();
        }

        private void UpdateBox(Label lbl, bool isZonActive)
        {
            // İçindeki metni temizliyoruz
            lbl.Text = string.Empty;

            // Zonda ise Kırmızı, zonsuz ise Beyaz yapıyoruz
            lbl.BackColor = isZonActive ? Color.Red : Color.White;
        }

        private void SetupPlayerBadges()
        {
            // 🔹 İlk çağrıda Label'ları oluştur (SADECE BİR KEZ)
            // Sonraki çağrılarda yeniden yaratma, sadece konumlarını güncelle.
            // Bu, her Resize'da 8 Label × (handle + GDI) sızıntısını önler.
            if (lblN_Dir == null)
            {
                CreateBadge(out lblN_Dir, out lblN_Name, "N", "Oyuncu 3", 0, 0);
                CreateBadge(out lblS_Dir, out lblS_Name, "G", "Siz", 0, 0);
                CreateBadge(out lblW_Dir, out lblW_Name, "B", "Oyuncu 2", 0, 0);
                CreateBadge(out lblE_Dir, out lblE_Name, "E", "Oyuncu 4", 0, 0);
            }

            int bosluk = 1;

            // ─── KUZEY ───
            int kuzeyY_Alt = _kuzeyY + _kartYukseklik + bosluk;
            int kuzeyX_Baslangic = 250;
            lblN_Dir.Location = new Point(kuzeyX_Baslangic, kuzeyY_Alt);
            lblN_Name.Location = new Point(kuzeyX_Baslangic + 22, kuzeyY_Alt);

            // ─── GÜNEY ───
            int guneyY_Alt = _guneyY + _kartYukseklik + bosluk;
            int guneyX_Baslangic = 250;
            lblS_Dir.Location = new Point(guneyX_Baslangic, guneyY_Alt);
            lblS_Name.Location = new Point(guneyX_Baslangic + 22, guneyY_Alt);

            // ─── BATI ve DOĞU ortak Y ───
            int ewStartingY = GetCenteredEWStartingY();
            int suitBoslugu = 2;
            int maxEWToplamYukseklik = (4 * _kartYukseklik) + (3 * suitBoslugu);
            int gercekEwY_Alt = ewStartingY + maxEWToplamYukseklik + bosluk;

            // ─── BATI ───
            int batiX = 40;
            lblW_Dir.Location = new Point(batiX, gercekEwY_Alt);
            lblW_Name.Location = new Point(batiX + 22, gercekEwY_Alt);

            // ─── DOĞU ───
            int doguX = doguKartlar[0] != null ? doguKartlar[0].Left : 930;
            lblE_Dir.Location = new Point(doguX, gercekEwY_Alt);
            lblE_Name.Location = new Point(doguX + 22, gercekEwY_Alt);

            // Aktif oyuncunun rengini güncelle
            UpdatePlayerBadges();
        }

        private void CreateBadge(out Label dirLbl, out Label nameLbl, string dir, string name, int x, int y)
        {
            dirLbl = new Label
            {
                Text = dir,
                Size = new Size(20, 20),
                Location = new Point(x, y),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                Font = _badgeDirFont  // 🔹 Static font
            };

            nameLbl = new Label
            {
                Text = name,
                Size = new Size(80, 20),
                Location = new Point(x + 22, y),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                Font = _badgeNameFont  // 🔹 Static font
            };

            this.Controls.Add(dirLbl);
            this.Controls.Add(nameLbl);
            dirLbl.BringToFront();
            nameLbl.BringToFront();
        }

        private void UpdatePlayerBadges()
        {
            Label[] names = { lblN_Name, lblE_Name, lblS_Name, lblW_Name };
            Label[] dirs = { lblN_Dir, lblE_Dir, lblS_Dir, lblW_Dir };
            Player[] players = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };

            for (int i = 0; i < 4; i++)
            {
                if (names[i] == null || dirs[i] == null) continue;
                bool isActive = (activePlayer == players[i]);
                names[i].BackColor = isActive ? Color.Yellow : Color.Gray;
                names[i].ForeColor = isActive ? Color.Black : Color.White;
                dirs[i].BackColor = isActive ? Color.Yellow : Color.Gray;
                dirs[i].ForeColor = isActive ? Color.Black : Color.White;
            }
        }

        private void SetupPlayZone()
        {
            if (playZonePanel == null)
            {
                playZonePanel = new Panel
                {
                    BackColor = Color.FromArgb(0, 100, 0),
                    BorderStyle = BorderStyle.FixedSingle,
                    Size = new Size(500, 500),
                    Cursor = Cursors.Hand   // 🔹 Tıklanabilir hissi
                };
                EnableDoubleBuffer(playZonePanel);
                this.Controls.Add(playZonePanel);

                // 🔹 YENİ: Panel tıklaması — el bittiğinde yeni el başlatır
                playZonePanel.Click += PlayZonePanel_Click;
            }

            playZonePanel.Left = ((this.ClientSize.Width - playZonePanel.Width) / 6) + 30;
            playZonePanel.Top = (_kuzeyY + _guneyY) / 2 - (playZonePanel.Height / 3);
            playZonePanel.BringToFront();
        }

        /// <summary>
        /// PlayZonePanel'e tıklandığında çalışır.
        /// El bittiyse → kartları temizler ve yeni eli başlatır.
        /// </summary>
        private void PlayZonePanel_Click(object sender, EventArgs e)
        {
            // El bitmediyse hiçbir şey yapma
            if (!_elBittiBekliyor)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[PlayZone] Tıklandı ama el henüz bitmedi.");
                return;
            }

            System.Diagnostics.Debug.WriteLine(
                $"[PlayZone] El bitti onaylandı — yeni el başlıyor. Kazanan: {_sonElKazanani}");

            // Bayrağı sıfırla
            _elBittiBekliyor = false;

            // Masadaki kartları temizle
            MasadakiKartlariTemizle();

            // Yeni elin aktif oyuncusu = son elin kazananı
            activePlayer = _sonElKazanani;

            System.Diagnostics.Debug.WriteLine(
                $"[PlayZone] Aktif oyuncu: {activePlayer}");

            // Sıra AI'da mı? Otomatik oynat
            if (activePlayer != _humanPlayer)
            {
                AITetikle();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    "[PlayZone] Sıra kullanıcıda, kart bekleniyor.");
            }
        }
        /// <summary>
        /// 13 el tamamlandığında Deal Complete formunu açar.
        /// Form kapandığında ne olacağını form kendi belirler
        /// (yeni el, ana menü vs.).
        /// </summary>
        private void BordTamamlandiFormuAc()
        {
            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            _elBittiBekliyor = false;

            System.Diagnostics.Debug.WriteLine(
                "[BordTamamlandi] DealCompleteForm açılıyor...");

            using (var form = new DealCompleteForm(this))
            {
                var sonuc = form.ShowDialog(this);

                if (sonuc == DialogResult.OK)
                {
                    // "Go to Next Deal" → yeni bord
                    System.Diagnostics.Debug.WriteLine(
                        "[BordTamamlandi] Kullanıcı yeni bord seçti.");
                    YeniBordBaslat();
                }
                else
                {
                    // "Return to Main Menu" veya X → AnaSayfa
                    System.Diagnostics.Debug.WriteLine(
                        "[BordTamamlandi] Kullanıcı ana menüye döndü.");
                    this.Close();
                }
            }
        }
        /// <summary>
        /// "Go to Next Deal" basıldığında çağrılır.
        /// Yeni bord başlatır: sayaçları sıfırlar, kartları yeniden dağıtır,
        /// yeni DekBasForm açar.
        /// </summary>
        public void YeniBordBaslat()
        {
            System.Diagnostics.Debug.WriteLine(
                "[YeniBord] Yeni bord başlatılıyor...");

            // 🔹 Oynanan tüm eller listesini temizle
            _oynananTumEller.Clear();

            // 1) El sayacını sıfırla
            _oynananToplamEl = 0;

            // 2) Masadaki kartları temizle
            MasadakiKartlariTemizle();

            // 3) Bu elde oynananları sıfırla
            _buEldeOynananlar.Clear();
            _buEldeAtakRengi = null;
            _elBittiBekliyor = false;

            // 4) Aktif oyuncu, dealer, kontrat vs. sıfırlanır — DortPasSonrasiYeniEliBaslat ile aynı
            _oyunAsamasi = OyunAsamasi.Baslangic;
            _currentTour++;
            _dealer = SonrakiOyuncu(_dealer);
            activePlayer = _dealer;
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;

            _kontrat = null;
            _kontratDeklaran = _dealer;

            // 5) Butonları inaktif yap
            ButonlariInaktifYap();

            // 6) Info board ve oyun bilgi panelini güncelle
            UpdateInfoBoard(_dealer);
            OyunBilgiPaneliniGuncelle();

            // 7) Yeni kartları dağıt
            KartlariDagit(AnaSayfa.SeciliKartSeti);

            // 8) Yeni DekBasForm aç
            YeniDekBasFormAc();
        }

        private Image GetBackImage()
        {
            return GetCachedImage(CurrentBackImage);
        }

        private void KartlariDagit(string kartSeti)
        {
            this.SuspendLayout();
            try
            {
                // ─── 1. Kartları karıştır ve 4 oyuncuya böl ───
                List<string> kartlar = new List<string>();
                string[] rank = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
                string[] suit = { "S", "H", "D", "C" };
                foreach (var s in suit)
                    foreach (var r in rank)
                        kartlar.Add(r + s + ".png");

                // 🔹 Fisher-Yates shuffle
                for (int i = kartlar.Count - 1; i > 0; i--)
                {
                    int j = _rng.Next(i + 1);
                    (kartlar[i], kartlar[j]) = (kartlar[j], kartlar[i]);
                }

                var kuzey = kartlar.Take(13).ToList();
                var dogu = kartlar.Skip(13).Take(13).ToList();
                var guney = kartlar.Skip(26).Take(13).ToList();
                var bati = kartlar.Skip(39).Take(13).ToList();

                // ─── 2. Elleri temizle ve geçici listelere doldur ───
                kuzeyEl.Clear(); guneyEl.Clear(); batiEl.Clear(); doguEl.Clear();
                List<Card> geciciKuzey = new List<Card>();
                List<Card> geciciGuney = new List<Card>();
                List<Card> geciciBati = new List<Card>();
                List<Card> geciciDogu = new List<Card>();

                foreach (var file in kuzey) geciciKuzey.Add(ParseCard(file, kartSeti));
                foreach (var file in guney) geciciGuney.Add(ParseCard(file, kartSeti));
                foreach (var file in bati) geciciBati.Add(ParseCard(file, kartSeti));
                foreach (var file in dogu) geciciDogu.Add(ParseCard(file, kartSeti));

                kuzeyEl.AddRange(CardSorter.SiraliEl(geciciKuzey));
                guneyEl.AddRange(CardSorter.SiraliEl(geciciGuney));
                batiEl.AddRange(CardSorter.SiraliEl(geciciBati));
                doguEl.AddRange(CardSorter.SiraliEl(geciciDogu));

                // ─── 3. Tüm göster butonlarını sıfırla ───
                BtnHepsi.Checked = false;
                BtnEW.Checked = false;
                BtnNS.Checked = false;
                BtnGuney.Checked = false;
                hepsiniGösterToolStripMenuItem.Checked = false;
                eWGösterToolStripMenuItem.Checked = false;
                nSGösterToolStripMenuItem.Checked = false;

                // Aktif oyuncu = insan oyuncu ise buton pasif olmalı
                BtnGuney.Enabled = (activePlayer != _humanPlayer);

                // ─── 4. Layout'u hazırla (kart konumları, info board, badge'ler, play zone) ───
                //KartYerlesimiHazirla();

                // ─── 5. Görsel gösterim: sadece insan oyuncunun (Güney) eli açık ───
                SadeceGuneyiGoster();
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }
        // 🔹 "Elleri Değiştir" formundan (ElleriDegistir.BtnOK_Click) çağrılır: Güney'in
        // eli (kullanıcının eli), seçilen yöndeki oyuncunun eliyle takas edilir.
        // guneyEl/batiEl/kuzeyEl/doguEl alanları 'readonly' olduğu için referans
        // değişimi yapılamaz; bunun yerine listelerin İÇERİĞİ takas edilir.
        public void ElleriTakasEt(Player digerYon)
        {
            List<Card> digerEl;
            switch (digerYon)
            {
                case Player.Bati: digerEl = batiEl; break;
                case Player.Kuzey: digerEl = kuzeyEl; break;
                case Player.Dogu: digerEl = doguEl; break;
                default: return; // Güney seçilemez/anlamsız; güvenlik için çık.
            }

            List<Card> guneyGecici = new List<Card>(guneyEl);
            List<Card> digerGecici = new List<Card>(digerEl);

            guneyEl.Clear();
            guneyEl.AddRange(digerGecici);

            digerEl.Clear();
            digerEl.AddRange(guneyGecici);

            this.SuspendLayout();
            try
            {
                SadeceGuneyiGoster();
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }


        private void HepsiniGoster()
        {
            KartYerlesimiHazirla();

            int doguXPos = doguKartlar[0] != null ? doguKartlar[0].Left : 930;
            int ewStartingY = GetCenteredEWStartingY();

            BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, 250, _kuzeyY, false, true);
            BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);
            BricStandartlarinaGoreDiz(doguKartlar, doguEl, doguXPos, ewStartingY);
            BricStandartlarinaGoreDiz(batiKartlar, batiEl, 40, ewStartingY);

            SetupPlayerBadges();
        }

        private void SadeceGuneyiGoster()
        {
            KartYerlesimiHazirla();

            Image backImg = GetBackImage();

            if (backImg != null)
            {
                for (int i = 0; i < 13; i++)
                {
                    if (kuzeyKartlar[i] != null) kuzeyKartlar[i].Image = backImg;
                    if (batiKartlar[i] != null) batiKartlar[i].Image = backImg;
                    if (doguKartlar[i] != null) doguKartlar[i].Image = backImg;
                }
            }
            else
            {
                // Arka yüz resmi yoksa, rakip kartların yüzünü gizle (masa rengi)
                for (int i = 0; i < 13; i++)
                {
                    if (kuzeyKartlar[i] != null) { kuzeyKartlar[i].Image = null; kuzeyKartlar[i].BackColor = AnaSayfa.MasaRengi; }
                    if (batiKartlar[i] != null) { batiKartlar[i].Image = null; batiKartlar[i].BackColor = AnaSayfa.MasaRengi; }
                    if (doguKartlar[i] != null) { doguKartlar[i].Image = null; doguKartlar[i].BackColor = AnaSayfa.MasaRengi; }
                }
            }

            BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);
            SetupPlayerBadges();
        }

        private void CurrentPlayerGoster()
        {
            Image backImg = GetBackImage();
            if (backImg != null)
            {
                for (int i = 0; i < 13; i++)
                {
                    if (kuzeyKartlar[i] != null) kuzeyKartlar[i].Image = backImg;
                    if (guneyKartlar[i] != null) guneyKartlar[i].Image = backImg;
                    if (batiKartlar[i] != null) batiKartlar[i].Image = backImg;
                    if (doguKartlar[i] != null) doguKartlar[i].Image = backImg;
                }
            }
            switch (activePlayer)
            {
                case Player.Kuzey: BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, 250, _kuzeyY, false, true); break;
                case Player.Guney: BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true); break;
                case Player.Bati: BricStandartlarinaGoreDiz(batiKartlar, batiEl, 40, GetCenteredEWStartingY()); break;
                // 💡 DÜZELTME: Varsayılan değer 930 yapıldı
                case Player.Dogu: BricStandartlarinaGoreDiz(doguKartlar, doguEl, doguKartlar[0]?.Left ?? 930, GetCenteredEWStartingY()); break;
            }
            SetupPlayerBadges();
        }

        private int BricStandartlarinaGoreDizYatay(PictureBox[] kartResimleri, List<Card> eldekiKartlar, int baslangicX, int baslangicY, bool sikiDizim = false, bool groupSuits = true)
        {
            for (int i = 0; i < kartResimleri.Length; i++) if (kartResimleri[i] != null) kartResimleri[i].Visible = false;

            int guncelX = baslangicX;
            int kartIçiSıkışmaBoslugu = 18;
            int toplamYerlestirilen = 0;
            int enSonKartBitisX = baslangicX + _kartGenislik;

            if (groupSuits)
            {
                var siraliMaça = eldekiKartlar.Where(k => k.Suit == "Maça").OrderByDescending(k => k.Value).ToList();
                var siraliKupa = eldekiKartlar.Where(k => k.Suit == "Kupa").OrderByDescending(k => k.Value).ToList();
                var siraliSinek = eldekiKartlar.Where(k => k.Suit == "Sinek").OrderByDescending(k => k.Value).ToList();
                var siraliKaro = eldekiKartlar.Where(k => k.Suit == "Karo").OrderByDescending(k => k.Value).ToList();

                List<List<Card>> tumRenkGruplari = new List<List<Card>> { siraliMaça, siraliKupa, siraliSinek, siraliKaro };
                int renkBloklarArasiBosluk = sikiDizim ? 0 : 10;

                foreach (var renkGrubu in tumRenkGruplari)
                {
                    if (renkGrubu.Count == 0) continue;
                    for (int i = 0; i < renkGrubu.Count; i++)
                    {
                        if (toplamYerlestirilen >= kartResimleri.Length) break;
                        PictureBox pb = kartResimleri[toplamYerlestirilen];
                        if (pb != null)
                        {
                            pb.Image = renkGrubu[i].Image;
                            pb.Tag = renkGrubu[i];   // 🔹 Card nesnesini Tag'de sakla
                            pb.Location = new Point(guncelX, baslangicY);
                            pb.Visible = true;
                            pb.BringToFront();
                        }
                        enSonKartBitisX = guncelX + _kartGenislik;
                        toplamYerlestirilen++;
                        guncelX += (i < renkGrubu.Count - 1) ? kartIçiSıkışmaBoslugu : (_kartGenislik + renkBloklarArasiBosluk);
                    }
                }
            }
            else
            {
                foreach (var card in eldekiKartlar)
                {
                    if (toplamYerlestirilen >= kartResimleri.Length) break;

                    PictureBox pb = kartResimleri[toplamYerlestirilen];
                    if (pb != null)
                    {
                        pb.Image = card.Image;
                        pb.Tag = card;   // 🔹 Card nesnesini Tag'de sakla
                        pb.Location = new Point(guncelX, baslangicY);
                        pb.Visible = true;
                        pb.BringToFront();
                    }

                    enSonKartBitisX = guncelX + _kartGenislik;
                    toplamYerlestirilen++;
                    guncelX += (_kartGenislik - 55);
                }
            }
            return enSonKartBitisX;
        }

        private void BricStandartlarinaGoreDiz(PictureBox[] kartResimleri, List<Card> eldekiKartlar, int baslangicX, int baslangicY)
        {
            for (int i = 0; i < kartResimleri.Length; i++) if (kartResimleri[i] != null) kartResimleri[i].Visible = false;
            var siraliMaça = eldekiKartlar.Where(k => k.Suit == "Maça").OrderByDescending(k => k.Value).ToList();
            var siraliKupa = eldekiKartlar.Where(k => k.Suit == "Kupa").OrderByDescending(k => k.Value).ToList();
            var siraliSinek = eldekiKartlar.Where(k => k.Suit == "Sinek").OrderByDescending(k => k.Value).ToList();
            var siraliKaro = eldekiKartlar.Where(k => k.Suit == "Karo").OrderByDescending(k => k.Value).ToList();
            List<List<Card>> tumSatirlar = new List<List<Card>> { siraliMaça, siraliKupa, siraliSinek, siraliKaro }.Where(satir => satir.Count > 0).ToList();
            int yatayKayma = 20;
            int dikeySatirAraligi = _kartYukseklik + 2;
            int toplamKullanilanKart = 0;
            for (int satirIndeks = 0; satirIndeks < tumSatirlar.Count; satirIndeks++)
            {
                List<Card> satirKartlari = tumSatirlar[satirIndeks];
                for (int kartIndeks = 0; kartIndeks < satirKartlari.Count; kartIndeks++)
                {
                    if (toplamKullanilanKart >= kartResimleri.Length) break;
                    PictureBox pb = kartResimleri[toplamKullanilanKart];
                    if (pb != null)
                    {
                        // Eğer E/W göstergesi aktifse ve Kuzey eli gösterimi devre dışı bırakılmak isteniyorsa
                        Image faceImg = satirKartlari[kartIndeks].Image;
                        Image backImg = GetBackImage();
                        if (kartResimleri == kuzeyKartlar && BtnEW != null && BtnEW.Checked)
                        {
                            // Eğer arka yüz resmi varsa onu göster; yoksa yüzü gizle (boş bırak)
                            if (backImg != null)
                            {
                                pb.Image = backImg;
                            }
                            else
                            {
                                pb.Image = null;
                                pb.BackColor = AnaSayfa.MasaRengi;
                            }
                        }
                        else
                        {
                            pb.Image = faceImg;
                        }
                        pb.Location = new Point(baslangicX + (kartIndeks * yatayKayma), baslangicY + (satirIndeks * dikeySatirAraligi));
                        pb.Visible = true;
                        pb.BringToFront();
                    }
                    toplamKullanilanKart++;
                }
            }
        }

        private Card ParseCard(string fileName, string kartSeti)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string suitChar = nameWithoutExt.Last().ToString();
            string rankStr = nameWithoutExt.Substring(0, nameWithoutExt.Length - 1);
            int value;
            switch (rankStr)
            {
                case "A": value = 14; break;
                case "K": value = 13; break;
                case "Q": value = 12; break;
                case "J": value = 11; break;
                default: value = int.Parse(rankStr); break;
            }
            string suit;
            switch (suitChar)
            {
                case "S": suit = "Maça"; break;
                case "H": suit = "Kupa"; break;
                case "C": suit = "Sinek"; break;
                case "D": suit = "Karo"; break;
                default: suit = ""; break;
            }
            string imgPath = Path.Combine(Application.StartupPath, "Resources", "Cards", kartSeti ?? "Default", fileName);
            return new Card { Suit = suit, Value = value, Image = GetCachedImage(imgPath) };
        }

        private void ÇıkışToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void MasaRengiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    AnaSayfa.MasaRengi = cd.Color;
                    this.BackColor = AnaSayfa.MasaRengi;
                }
            }
        }

        // 🔹 "Sonraki el" akışı: onay mesajı sorar, Evet ise BricOyna üzerindeki tüm
        // formları kapatıp senaryoyu sıfırdan başlatır. Hem Ayarlar menüsündeki
        // sonrakiElToolStripMenuItem hem de DekBasForm'daki BtnSonrakiDagilim butonu
        // bu tek metodu çağırıyor; böylece iki tetikleyici de aynı davranışa sahip.
        public void SonrakiElIsteği()
        {

            // 🔹 Masadaki kartları temizle
            MasadakiKartlariTemizle();

            foreach (Form f in Application.OpenForms.Cast<Form>()
                         .Where(f => f != this && !(f is AnaSayfa))
                         .ToArray())
            {
                if (f is DeklarasyonForm dek)
                    dek.IzinliKapat();
                else if (f is DekBasForm dekBas)
                    dekBas.ProgramatikKapat();
                else
                    f.Close();
            }

            DortPasSonrasiYeniEliBaslat();
        }

        private void SonrakiElToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SonrakiElIsteği();
        }

        private void HepsiniGösterToolStripMenuItem_Click(object sender, EventArgs e) { BtnHepsi_Click(sender, e); }
        private void NSGösterToolStripMenuItem_Click(object sender, EventArgs e) { BtnNS_Click(sender, e); }
        private void EWGösterToolStripMenuItem_Click(object sender, EventArgs e) { BtnEW_Click(sender, e); }
        private void OynayanOyuncuToolStripMenuItem_Click(object sender, EventArgs e) { BtnGuney_Click(sender, e); }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            // 🔹 AnaSayfa'yı burada Show() ETMİYORUZ.
            // Bu formu açan AnaSayfa.OrtaButon_Click zaten ShowDialog() dönüşünde
            // AnaSayfa'yı gösterip yeniden düzenliyor (ResetToInitialState). Burada da
            // Show() çağırmak aynı işlemi iki kez tetikleyip fazladan titremeye yol açıyordu.
            this.Close();
        }

        private void BricOyna_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Timer'ları temizle (kapanışta hata vermemesi için)
            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            if (e.CloseReason == CloseReason.ApplicationExitCall ||
                e.CloseReason == CloseReason.WindowsShutDown)
                return;

            // Güvenlik ağı: Form, BtnGeri dışında bir yoldan (ör. pencere X butonu)
            // kapatılırsa AnaSayfa'nın ekranda görünür olduğundan emin ol.
            //
            // NOT: Eskiden Application.OpenForms["AnaSayfa"] şeklinde string ile arama
            // yapılıyordu; bu, Designer'da form adı değiştiğinde sessizce bozuluyordu.
            // Artık tipe göre arıyoruz — daha güvenli ve refactor dostu.
            var anaSayfa = Application.OpenForms.OfType<AnaSayfa>().FirstOrDefault();

            if (anaSayfa != null && !anaSayfa.Visible)
            {
                anaSayfa.ShowInTaskbar = true;
                anaSayfa.Show();
            }
        }

        private void BtnHepsi_Click(object sender, EventArgs e)
        {
            hepsiniGösterToolStripMenuItem.Checked = !hepsiniGösterToolStripMenuItem.Checked;
            BtnHepsi.Checked = hepsiniGösterToolStripMenuItem.Checked;
            BtnEW.Checked = false;
            BtnNS.Checked = false;
            BtnGuney.Checked = false;
            eWGösterToolStripMenuItem.Checked = false;
            nSGösterToolStripMenuItem.Checked = false;

            this.SuspendLayout();
            try
            {
                if (hepsiniGösterToolStripMenuItem.Checked)
                    HepsiniGoster();
                else
                    SadeceGuneyiGoster();
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        private void BtnEW_Click(object sender, EventArgs e)
        {
            BtnEW.Checked = !BtnEW.Checked;
            if (BtnEW.Checked)
            {
                BtnGuney.Checked = false; BtnHepsi.Checked = false; BtnNS.Checked = false;
                hepsiniGösterToolStripMenuItem.Checked = false; nSGösterToolStripMenuItem.Checked = false; eWGösterToolStripMenuItem.Checked = true;
                this.SuspendLayout();
                try
                {
                    KartYerlesimiHazirla();
                    Image backImg = GetBackImage();
                    if (backImg != null)
                        for (int j = 0; j < kuzeyKartlar.Length; j++) if (kuzeyKartlar[j] != null) kuzeyKartlar[j].Image = backImg;
                    BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);
                    int doguXPos = doguKartlar[0] != null ? doguKartlar[0].Left : 930;
                    int ewStartingY = GetCenteredEWStartingY();
                    BricStandartlarinaGoreDiz(doguKartlar, doguEl, doguXPos, ewStartingY);
                    BricStandartlarinaGoreDiz(batiKartlar, batiEl, 40, ewStartingY);
                    SetupPlayerBadges();
                }
                finally { this.ResumeLayout(true); }
            }
            else
            {
                eWGösterToolStripMenuItem.Checked = false;
                BtnGuney.Checked = false;

                this.SuspendLayout();
                try
                {
                    SadeceGuneyiGoster();
                }
                finally
                {
                    this.ResumeLayout(true);
                }
            }
        }


        private void BtnNS_Click(object sender, EventArgs e)
        {
            BtnNS.Checked = !BtnNS.Checked;
            if (BtnNS.Checked)
            {
                BtnGuney.Checked = false; BtnHepsi.Checked = false; BtnEW.Checked = false;
                hepsiniGösterToolStripMenuItem.Checked = false; eWGösterToolStripMenuItem.Checked = false; nSGösterToolStripMenuItem.Checked = true;
                this.SuspendLayout();
                try
                {
                    KartYerlesimiHazirla();
                    Image backImg = GetBackImage();
                    if (backImg != null)
                    {
                        for (int j = 0; j < batiKartlar.Length; j++) if (batiKartlar[j] != null) batiKartlar[j].Image = backImg;
                        for (int j = 0; j < doguKartlar.Length; j++) if (doguKartlar[j] != null) doguKartlar[j].Image = backImg;
                    }
                    BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, 250, _kuzeyY, false, true);
                    BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);
                    SetupPlayerBadges();
                }
                finally { this.ResumeLayout(true); }
            }
            else
            {
                nSGösterToolStripMenuItem.Checked = false;
                BtnGuney.Checked = false;

                this.SuspendLayout();
                try
                {
                    SadeceGuneyiGoster();
                }
                finally
                {
                    this.ResumeLayout(true);
                }
            }

        }
        private void BtnGuney_Click(object sender, EventArgs e)
        {
            // Aktif oyuncu insan oyuncu ise bu buton zaten inaktif olmalı, işlem yapma
            if (activePlayer == _humanPlayer) return;

            BtnGuney.Checked = !BtnGuney.Checked;
            BtnHepsi.Checked = false;
            BtnEW.Checked = false;
            BtnNS.Checked = false;
            hepsiniGösterToolStripMenuItem.Checked = false;
            eWGösterToolStripMenuItem.Checked = false;
            nSGösterToolStripMenuItem.Checked = false;

            this.SuspendLayout();
            try
            {
                if (BtnGuney.Checked)
                {
                    // İnsan oyuncu (Güney) + aktif oyuncunun eli açık
                    KartYerlesimiHazirla();

                    Image backImg = GetBackImage();
                    if (backImg != null)
                    {
                        for (int i = 0; i < 13; i++)
                        {
                            if (kuzeyKartlar[i] != null) kuzeyKartlar[i].Image = backImg;
                            if (batiKartlar[i] != null) batiKartlar[i].Image = backImg;
                            if (doguKartlar[i] != null) doguKartlar[i].Image = backImg;
                        }
                    }

                    BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);

                    switch (activePlayer)
                    {
                        case Player.Kuzey:
                            BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, 250, _kuzeyY, false, true);
                            break;
                        case Player.Bati:
                            BricStandartlarinaGoreDiz(batiKartlar, batiEl, 40, GetCenteredEWStartingY());
                            break;
                        case Player.Dogu:
                            BricStandartlarinaGoreDiz(doguKartlar, doguEl, doguKartlar[0]?.Left ?? 930, GetCenteredEWStartingY());
                            break;
                    }

                    SetupPlayerBadges();
                }
                else
                {
                    SadeceGuneyiGoster();
                }
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }


        /// <summary>
        /// Kullanıcı Güney'in elindeki bir karta tıkladığında çalışır.
        /// Kartı elden kaldırır ve masaya (playZonePanel içine) yerleştirir.
        /// </summary>
        private void GuneyKartTiklandi(int kartIndex)
        {
            System.Diagnostics.Debug.WriteLine(
        $"[GuneyKartTiklandi] Çağrıldı — index: {kartIndex}, " +
        $"aktif: {activePlayer}, aşama: {_oyunAsamasi}");

            // 1. Aşama kontrolü
            if (_oyunAsamasi != OyunAsamasi.Oyun)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[BricOyna] Kart tıklaması reddedildi — aşama: {_oyunAsamasi}");
                return;
            }

            // 2. Sıra kontrolü
            if (activePlayer != _humanPlayer)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[BricOyna] Sıra sizde değil — aktif: {activePlayer}");
                return;
            }

            // 3. Index kontrolü
            if (kartIndex < 0 || kartIndex >= guneyKartlar.Length) return;

            // 4. Kart kontrolü
            var pb = guneyKartlar[kartIndex];
            if (pb == null || !pb.Visible) return;

            if (!(pb.Tag is Card card))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[BricOyna] Güney kart #{kartIndex} tıklandı ama Tag'de Card nesnesi yok.");
                return;
            }

            // 5. Kartı oyna (merkezi metot üzerinden)
            KartOyna(Player.Guney, card, pb.Image);

        }

        /// <summary>
        /// Bir oyuncunun kart oynamasını işler. Sıra takibini yönetir:
        /// - Kart masaya gider
        /// - Oyuncunun elinden çıkar
        /// - _buEldeOynananlar listesine eklenir
        /// - Sıra bir sonraki oyuncuya geçer
        /// - Sıra AI'da ise otomatik oynar (AI hamlesi tetiklenir)
        /// </summary>
        private void KartOyna(Player oyuncu, Card kart, Image kartResmi)
        {
            if (kart == null) return;
            if (_oyunAsamasi != OyunAsamasi.Oyun) return;

            // Sıra bu oyuncuda mı?
            if (activePlayer != oyuncu)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] Sıra {oyuncu}'da değil, aktif: {activePlayer}");
                return;
            }

            // Bu elde ilk kart mı? Atak rengini belirle
            if (_buEldeAtakRengi == null)
            {
                _buEldeAtakRengi = kart.Suit;
            }

            // Kartı masaya yerleştir
            KartiMasayaYerlestir(oyuncu, kart, kartResmi);

            // Kartı oyuncunun elinden çıkar
            var el = OyuncuEli(oyuncu);
            el?.Remove(kart);

            // Kart oynandı — oyuncunun elini görsel olarak güncelle
            OyuncuEliniGuncelle(oyuncu, kart);

            // Bu elde oynananlar listesine ekle
            _buEldeOynananlar.Add((oyuncu, kart));

            System.Diagnostics.Debug.WriteLine(
                $"[KartOyna] {oyuncu} oynadı: {kart.Suit} {kart.Value} " +
                $"(bu elde {_buEldeOynananlar.Count}/4)");

            // ═══════════════════════════════════════════════════════════════
            // 4 KART TAMAMLANDI MI? (Yani el bitti mi?)
            // ═══════════════════════════════════════════════════════════════
            if (_buEldeOynananlar.Count >= 4)
            {
                // El kazananını belirle
                Player kazanan = ElKazananiniBelirle();

                // 🔹 Bu eli kaydet
                int elNo = _oynananToplamEl + 1;
                foreach (var (o, k) in _buEldeOynananlar)
                {
                    bool kazandi = (o == kazanan);
                    _oynananTumEller.Add((elNo, o, k, kazandi));
                }

                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] El #{elNo} kaydedildi (kazanan: {kazanan})");

                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] El tamamlandı — kazanan: {kazanan}");

                // Skor artışı
                if (kazanan == Player.Kuzey || kazanan == Player.Guney)
                    _kazanilanElNS++;
                else
                    _kazanilanElEW++;

                OyunBilgiPaneliniGuncelle();

                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] Skor → NS: {_kazanilanElNS}, EW: {_kazanilanElEW}");

                // El sayacını artır
                _oynananToplamEl++;

                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] Bu bordda {_oynananToplamEl}/13 el oynandı.");

                // Bu elde oynananları sıfırla (yeni el için hazırlık)
                _buEldeAtakRengi = null;
                _buEldeOynananlar.Clear();

                // ═══════════════════════════════════════════════════════════
                // BORD BİTTİ Mİ? (13 el tamamlandı mı?)
                // ═══════════════════════════════════════════════════════════
                if (_oynananToplamEl >= 13)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[KartOyna] BORD TAMAMLANDI! Deal Complete formu açılacak.");

                    _elBittiBekliyor = false;
                    BordTamamlandiFormuAc();
                    return;
                }

                // El bitti ama bord devam ediyor
                _sonElKazanani = kazanan;
                _elBittiBekliyor = true;

                System.Diagnostics.Debug.WriteLine(
                    "[KartOyna] El bitti. PlayZone'a tıklayınca yeni el başlayacak.");

                return;
            }

            // ═══════════════════════════════════════════════════════════════
            // EL HENÜZ BİTMEDİ — Sırayı bir sonraki oyuncuya geçir
            // ═══════════════════════════════════════════════════════════════
            activePlayer = SonrakiOyuncu(activePlayer);

            // Sıra AI'da mı? Otomatik oynat
            if (activePlayer != _humanPlayer)
            {
                AITetikle();
            }
        }

        /// <summary>
        /// Belirtilen oyuncunun kartını görsel olarak da elden kaldırır.
        /// Yani kart PictureBox'ını gizler ve eldeki kartları yeniden dizer.
        /// </summary>
        private void OyuncuEliniGuncelle(Player oyuncu, Card oynananKart)
        {
            // ─── 1. Oyuncunun kart dizisini seç ───
            PictureBox[] kartResimleri;
            List<Card> el;
            bool yatayDizim = false;
            int baslangicX = 0;
            int baslangicY = 0;

            switch (oyuncu)
            {
                case Player.Kuzey:
                    kartResimleri = kuzeyKartlar;
                    el = kuzeyEl;
                    yatayDizim = true;
                    baslangicX = 250;
                    baslangicY = _kuzeyY;
                    break;
                case Player.Guney:
                    kartResimleri = guneyKartlar;
                    el = guneyEl;
                    yatayDizim = true;
                    baslangicX = 250;
                    baslangicY = _guneyY;
                    break;
                case Player.Bati:
                    kartResimleri = batiKartlar;
                    el = batiEl;
                    yatayDizim = false;
                    baslangicX = 40;
                    baslangicY = GetCenteredEWStartingY();
                    break;
                case Player.Dogu:
                    kartResimleri = doguKartlar;
                    el = doguEl;
                    yatayDizim = false;
                    baslangicX = doguKartlar[0]?.Left ?? 930;
                    baslangicY = GetCenteredEWStartingY();
                    break;
                default:
                    return;
            }

            // ─── 2. Tüm PictureBox'ları gizle ───
            for (int i = 0; i < kartResimleri.Length; i++)
            {
                if (kartResimleri[i] != null)
                    kartResimleri[i].Visible = false;
            }

            // ─── 3. Kalan kartları yeniden diz ───
            // NOT: Zaten diğer kartlar dizili ama oynanan kart gizlenmediği için
            //      yeniden dizmek en temiz yol.
            if (yatayDizim)
            {
                BricStandartlarinaGoreDizYatay(kartResimleri, el, baslangicX, baslangicY, false, true);
            }
            else
            {
                BricStandartlarinaGoreDiz(kartResimleri, el, baslangicX, baslangicY);
            }
        }

        /// <summary>
        /// PlayZone'daki tüm masa kartlarını kaldırır ve dispose eder.
        /// </summary>
        private void MasadakiKartlariTemizle()
        {
            if (playZonePanel == null) return;

            foreach (var pb in _masadakiKartlar.Values)
            {
                if (pb != null && !pb.IsDisposed)
                {
                    playZonePanel.Controls.Remove(pb);
                    pb.Dispose();
                }
            }
            _masadakiKartlar.Clear();

            System.Diagnostics.Debug.WriteLine(
                "[YeniEl] Masadaki kartlar temizlendi.");
        }
        /// <summary>
        /// Aktif oyuncu AI ise, kısa bir gecikme ile AI'ın oynamasını tetikler.
        /// Timer sınıf alanı olarak tutulur — yoksa GC toplayıp oyun takılır.
        /// </summary>
        private void AITetikle()
        {
            System.Diagnostics.Debug.WriteLine(
                $"[AITetikle] Çağrıldı — aktif: {activePlayer}, aşama: {_oyunAsamasi}, " +
                $"insan: {_humanPlayer}");

            if (activePlayer == _humanPlayer)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[AITetikle] Aktif oyuncu insan, çıkılıyor.");
                return;
            }

            // Önceki timer varsa temizle
            _aiTimer?.Stop();
            _aiTimer?.Dispose();

            System.Diagnostics.Debug.WriteLine(
                "[AITetikle] Yeni timer oluşturuluyor (800ms).");

            _aiTimer = new System.Windows.Forms.Timer { Interval = 800 };
            _aiTimer.Tick += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AITetikle] Timer tick — aktif: {activePlayer}");

                _aiTimer?.Stop();
                _aiTimer?.Dispose();
                _aiTimer = null;

                var oynayacak = activePlayer;
                System.Diagnostics.Debug.WriteLine(
                    $"[AITetikle] AIOyna çağrılacak: {oynayacak}");

                if (oynayacak != _humanPlayer)
                {
                    AIOyna(oynayacak);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[AITetikle] Ama aktif oyuncu insan, AIOyna çağrılmıyor.");
                }
            };

            System.Diagnostics.Debug.WriteLine(
                "[AITetikle] Timer.Start() çağrılıyor.");
            _aiTimer.Start();
            System.Diagnostics.Debug.WriteLine(
                "[AITetikle] Timer başlatıldı.");
        }

        /// <summary>
        /// Belirtilen oyuncunun elini döndürür.
        /// </summary>
        private List<Card> OyuncuEli(Player oyuncu)
        {
            switch (oyuncu)
            {
                case Player.Kuzey: return kuzeyEl;
                case Player.Guney: return guneyEl;
                case Player.Bati: return batiEl;
                case Player.Dogu: return doguEl;
                default: return null;
            }
        }

        /// <summary>
        /// Aktif oyuncu AI ise otomatik oynar (basit: renk takip + rastgele).
        /// </summary>
        private void AIOyna(Player oyuncu)
        {
            System.Diagnostics.Debug.WriteLine(
        $"[AIOyna] Çağrıldı — oyuncu: {oyuncu}, aktif: {activePlayer}, " +
        $"aşama: {_oyunAsamasi}");

            if (_oyunAsamasi != OyunAsamasi.Oyun)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[AIOyna] Aşama Oyun değil, çıkılıyor.");
                return;
            }
            if (oyuncu == _humanPlayer)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[AIOyna] Oyuncu insan, çıkılıyor.");
                return;
            }
            if (activePlayer != oyuncu)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AIOyna] Sıra {oyuncu}'da değil, aktif: {activePlayer}");
                return;
            }

            var el = OyuncuEli(oyuncu);
            if (el == null || el.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AIOyna] {oyuncu}'ın eli boş, çıkılıyor.");
                return;
            }

            // Renk takip: atak renginden var mı?
            Card secilen = null;
            if (!string.IsNullOrEmpty(_buEldeAtakRengi))
            {
                var ayniRenk = el.Where(c => c.Suit == _buEldeAtakRengi).ToList();
                if (ayniRenk.Count > 0)
                {
                    secilen = ayniRenk[_rng.Next(ayniRenk.Count)];
                }
            }

            // Yoksa rastgele
            if (secilen == null)
            {
                secilen = el[_rng.Next(el.Count)];
            }

            System.Diagnostics.Debug.WriteLine(
                $"[AIOyna] {oyuncu} AI oynadı: {secilen.Suit} {secilen.Value}");

            // Kartı oyna (aynı merkezi metot)
            KartOyna(oyuncu, secilen, secilen.Image);
        }

        /// <summary>
        /// Bu elde oynanan 4 kart arasında kazananı belirler.
        /// Kurallar:
        /// - Atak rengi (ilk oynanan kartın rengi) belirleyicidir
        /// - Aynı renkten en yüksek kart kazanır
        /// - Koz varsa ve oynanmışsa, en yüksek koz kazanır
        /// </summary>
        private Player ElKazananiniBelirle()
        {
            if (_buEldeOynananlar.Count != 4) return activePlayer;

            string koz = KontratKozu();
            string atakRengi = _buEldeAtakRengi;

            // En yüksek kozu bul (varsa)
            Card enYuksekKoz = null;
            Player kozSahibi = activePlayer;

            // En yüksek atak rengi kartını bul
            Card enYuksekAtak = null;
            Player atakSahibi = activePlayer;

            foreach (var (oyuncu, kart) in _buEldeOynananlar)
            {
                // Koz kontrolü
                if (!string.IsNullOrEmpty(koz) && kart.Suit == koz)
                {
                    if (enYuksekKoz == null || kart.Value > enYuksekKoz.Value)
                    {
                        enYuksekKoz = kart;
                        kozSahibi = oyuncu;
                    }
                }

                // Atak rengi kontrolü
                if (kart.Suit == atakRengi)
                {
                    if (enYuksekAtak == null || kart.Value > enYuksekAtak.Value)
                    {
                        enYuksekAtak = kart;
                        atakSahibi = oyuncu;
                    }
                }
            }

            // Koz varsa ve oynanmışsa → koz kazanır
            if (enYuksekKoz != null) return kozSahibi;

            // Koz yoksa veya oynanmamışsa → atak rengindeki en yüksek kazanır
            return atakSahibi;
        }

        /// <summary>
        /// Belirtilen oyuncunun kartını masaya (playZonePanel içine) yerleştirir.
        /// O kart zaten masada varsa önce eskisini kaldırır.
        /// </summary>
        private void KartiMasayaYerlestir(Player oyuncu, Card card, Image image)
        {
            if (playZonePanel == null) return;

            // Aynı oyuncunun masada kartı varsa önce kaldır
            if (_masadakiKartlar.TryGetValue(oyuncu, out var eskiPb) && eskiPb != null)
            {
                playZonePanel.Controls.Remove(eskiPb);
                eskiPb.Dispose();
                _masadakiKartlar.Remove(oyuncu);
            }

            // Yeni PictureBox oluştur
            var masaPb = new PictureBox
            {
                Width = MasaKartGenislik,
                Height = MasaKartYukseklik,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle,
                Image = image,
                Tag = card
            };

            // Oyuncuya göre pozisyon
            Point pozisyon = OyuncuIcinMasaPozisyonu(oyuncu);
            masaPb.Location = pozisyon;

            playZonePanel.Controls.Add(masaPb);
            masaPb.BringToFront();
            _masadakiKartlar[oyuncu] = masaPb;

            System.Diagnostics.Debug.WriteLine(
                $"[BricOyna] {oyuncu} kartı masaya kondu: {card.Suit} {card.Value} @ {pozisyon}");
        }

        /// <summary>
        /// Belirtilen oyuncu için masadaki kart pozisyonunu döndürür.
        /// </summary>
        private Point OyuncuIcinMasaPozisyonu(Player oyuncu)
        {
            switch (oyuncu)
            {
                case Player.Guney: return MasaPozisyonGuney;
                case Player.Kuzey: return MasaPozisyonKuzey;
                case Player.Bati: return MasaPozisyonBati;
                case Player.Dogu: return MasaPozisyonDogu;
                default: return new Point(0, 0);
            }
        }

        /// <summary>
        /// DeklarasyonForm'dan gelen aktif oyuncu bilgisine göre BricOyna üzerindeki 
        /// ilgili oyuncu etiketini SARI renge boyar, diğerlerini normale çeker.
        /// </summary>
        public void AktifOyuncuEtiketiniGuncelle(string aktifOyuncu)
        {
            activePlayer = OyuncuyaCevir(aktifOyuncu);
            // Property setter'ı otomatik olarak BtnGuney ve badge'leri günceller
        }

        private void IhaleGösterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IhaleGecmisiniGoster();
        }



        public List<Card> GetGuneyElinKartlari()
        {
            // Lütfen buradaki 'guneyElinKartlari' veya 'guneyOyuncu.Kartlar' 
            // değişken adını BricOyna formunuzda Güney'in kartlarını tuttuğunuz listenin adıyla değiştirin.
            return guneyEl;
        }



        public void DortPasSonrasiYeniEliBaslat()
        {
            _oyunAsamasi = OyunAsamasi.Baslangic;   // ← EKLE
            _currentTour++;
            _dealer = SonrakiOyuncu(_dealer);
            activePlayer = _dealer;
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;

            // 🔹 Yeni el başlıyor — kontrat bilgilerini sıfırla
            _kontrat = null;
            _kontratDeklaran = _dealer;

            ButonlariInaktifYap();
            UpdateInfoBoard(_dealer);
            KartlariDagit(AnaSayfa.SeciliKartSeti);

            // 🔹 YENİ: Bilgi panelini güncelle (yeni Dealer ve Zon görünsün)
            OyunBilgiPaneliniGuncelle();

            YeniDekBasFormAc();
        }

        private void BtnSonrakiEl_Click(object sender, EventArgs e)
        {
            // Açık bir deklarasyon formu varsa kullanıcıyı uyar
            bool deklarasyonAcik = Application.OpenForms
                .OfType<DeklarasyonForm>()
                .Any(f => f.Visible);

            if (deklarasyonAcik)
            {
                var cevap = MessageBox.Show(
                    "Devam eden bir deklarasyon var. Yine de sonraki ele geçmek istiyor musunuz?\n" +
                    "Mevcut el kaybolacak.",
                    "Sonraki El Onayı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (cevap != DialogResult.Yes) return;
            }

            SonrakiElIsteği();
        }

        // Kontrat, deklaran ve atak (açılış) yapacak oyuncu bilgileri
        private string _kontrat;
        private Player _kontratDeklaran;
        private Player _atakYapacakOyuncu;


        // 🔹 Kazanılan el sayıları (oyun sırasında artacak)
        private int _kazanilanElNS = 0;
        private int _kazanilanElEW = 0;

        // 🔹 UI'ya yansıtmak için public erişim
        public string Kontrat => _kontrat;
        public Player KontratDeklaran => _kontratDeklaran;
        public Player AtakYapacakOyuncu => _atakYapacakOyuncu;
        public int KazanilanElNS => _kazanilanElNS;
        public int KazanilanElEW => _kazanilanElEW;

        // ====================================================================
        // OYUN BİLGİ PANELİ (Sağ üst köşe)
        // --------------------------------------------------------------------
        // Dealer, Zon, Kontrat, Deklaran ve kazanılan el bilgilerini gösterir.
        // Konum ve boyut burada; değiştirmek için alanları güncelle.
        // ====================================================================
        private Panel oyunBilgiPanel;
        private Label lblBilgiTur;
        private Label lblBilgiDealer;
        private Label lblBilgiZon;
        private Label lblBilgiKontrat;
        private Label lblBilgiDeklaran;
        private Label lblBilgiKazanilanEl;

        private const int BilgiPanelGenislik = 220;
        private const int BilgiPanelYukseklik = 190;
        private const int BilgiPanelSagBosluk = 20;
        private const int BilgiPanelUstBosluk = 70;

        /// <summary>
        /// Bir Player enum değerini Türkçe oyuncu ismine çevirir.
        /// </summary>
        public static string PlayerIsmiTurkce(Player p)
        {
            switch (p)
            {
                case Player.Kuzey: return "Kuzey";
                case Player.Guney: return "Güney";
                case Player.Bati: return "Batı";
                case Player.Dogu: return "Doğu";
                default: return "—";
            }
        }
        /// <summary>
        /// Mevcut kontrattan koz rengini çıkarır.
        /// Örnek: "4♠" → "Maça", "3NT" → null (koz yok).
        /// </summary>
        private string KontratKozu()
        {
            if (string.IsNullOrEmpty(_kontrat)) return null;

            char sonKarakter = _kontrat.Last();

            switch (sonKarakter)
            {
                case '♠': return "Maça";
                case '♥': return "Kupa";
                case '♦': return "Karo";
                case '♣': return "Sinek";
                case 'T': return null;   // NT — koz yok
                case 't': return null;
                default: return null;
            }
        }

        /// <summary>
        /// Zon durumunu Türkçe metin olarak döndürür.
        /// </summary>
        private string ZonDurumuMetni()
        {
            int boardMod = ((_currentTour - 1) % 16) + 1;

            bool nsZonda = (boardMod == 2 || boardMod == 5 || boardMod == 12 || boardMod == 15 ||
                            boardMod == 4 || boardMod == 7 || boardMod == 10 || boardMod == 16);

            bool ewZonda = (boardMod == 3 || boardMod == 6 || boardMod == 9 || boardMod == 13 ||
                            boardMod == 4 || boardMod == 7 || boardMod == 10 || boardMod == 16);

            if (nsZonda && ewZonda) return "Herkes";
            if (nsZonda) return "Kuzey/Güney";
            if (ewZonda) return "Batı/Doğu";
            return "Zonsuz";
        }

        private static Player OyuncuyaCevir(string oyuncu)
        {
            switch (oyuncu)
            {
                case "Kuzey": return Player.Kuzey;
                case "Dogu": return Player.Dogu;
                case "Bati": return Player.Bati;
                default: return Player.Guney;
            }
        }

        /// <summary>
        /// DeklarasyonForm'da ihale normal şekilde bittiğinde çağrılır.
        /// Kontratı ve deklaranı saklar; sırayı atak yapacak rakibe (deklaranın solu) verir.
        /// </summary>
        public void IhaleTamamlandiBaslat(string kontrat, string deklaran, string solRakip)
        {
            _oyunAsamasi = OyunAsamasi.Oyun;   // ← EKLE
            _kontrat = kontrat;
            _kontratDeklaran = OyuncuyaCevir(deklaran);
            _declarer = _kontratDeklaran;               // Deklaran belirlendi
            _atakYapacakOyuncu = OyuncuyaCevir(solRakip);

            // Oyun başlangıcında sıra atak yapacak oyuncuda
            activePlayer = _atakYapacakOyuncu;

            // 🔹 YENİ: Kontrat ve deklaran bilgisini panele yansıt
            OyunBilgiPaneliniGuncelle();

            // Görsel: aktif oyuncu insan oyuncu değilse, onun elini gösterme
            // (Kullanıcı isterse BtnGuney ile açabilir)
            this.SuspendLayout();
            try
            {
                SadeceGuneyiGoster();
            }
            finally
            {
                this.ResumeLayout(true);
            }
            // 🔹 İhale bittiğinde sıra AI'da ise otomatik oynat
            if (activePlayer != _humanPlayer)
            {
                AITetikle();
            }


            UpdatePlayerBadges();
        }

        private Player SonrakiOyuncu(Player mevcut)
        {
            switch (mevcut)
            {
                case Player.Guney: return Player.Bati;
                case Player.Bati: return Player.Kuzey;
                case Player.Kuzey: return Player.Dogu;
                case Player.Dogu: return Player.Guney;
                default: return Player.Guney;
            }
        }


        public void IhaleGecmisiniKaydet(List<string> bids, string baslangicOyuncusu)
        {
            _sonIhaleGecmisi = new List<string>(bids);
            _ihaleBaslangicOyuncusu = baslangicOyuncusu;
        }

        private void oynananElleriGösterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OynananElleriGoster();
        }

        private void BricOyna_Load_1(object sender, EventArgs e)
        {

        }

        // Kullanıcı oyun esnasında ihale geçmişini görmek istediğinde çağrılacak metot
        public void IhaleGecmisiniGoster()
        {
            if (_sonIhaleGecmisi == null || _sonIhaleGecmisi.Count == 0)
            {
                MessageBox.Show("Henüz tamamlanmış bir ihale bulunmuyor.", "İhale Geçmişi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (IhaleGecmisiForm form = new IhaleGecmisiForm(_sonIhaleGecmisi, _ihaleBaslangicOyuncusu))
            {
                form.ShowDialog(this);
            }
        }

        /// <summary>
        /// Oynanan tüm elleri bir formda gösterir.
        /// Deal Complete formundaki "Oyunu Göster" butonu da bunu çağırır.
        /// </summary>
        public void OynananElleriGoster()
        {
            if (_oynananTumEller == null || _oynananTumEller.Count == 0)
            {
                MessageBox.Show(
                    "Henüz oynanmış bir el bulunmuyor.",
                    "Oynanan Eller",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using (var form = new OynananElleriGosterForm(_oynananTumEller))
            {
                form.ShowDialog(this);
            }
        }
        private void ButonlariInaktifYap()
        {
            BtnOtoOyna.Enabled = false;
            BtnClaim.Enabled = false;
            BtnGeriAl.Enabled = false;
            BtnileriAl.Enabled = false;
            BtnIpucu.Enabled = false;
            BtnSonrakiEl.Enabled = false;
            BtnExit.Enabled = false;
            BtnHepsi.Enabled = false;
            BtnEW.Enabled = false;
            BtnNS.Enabled = false;
            BtnGuney.Enabled = false;
        }

        // DekBasForm'daki BtnDeklarasyon tıklandığında çağrılır.
        // DeklarasyonForm açılırken bu 7 buton aktif hale gelir.
        public void DeklarasyonAsamasindaButonlariAktifEt()
        {
            _oyunAsamasi = OyunAsamasi.Deklarasyon;   // ← EKLE
            BtnOtoOyna.Enabled = true;
            BtnIpucu.Enabled = true;
            BtnSonrakiEl.Enabled = true;
            BtnExit.Enabled = true;
            BtnHepsi.Enabled = true;
            BtnEW.Enabled = true;
            BtnNS.Enabled = true;
        }
        // DeklarasyonForm içinde herhangi bir teklif/pas/kontr yapıldığında çağrılır.
        // Artık geri alınacak bir hamle olduğu için BtnGeriAl aktif edilir.
        public void GeriAlButonunuAktifEt()
        {
            if (BtnGeriAl != null)
                BtnGeriAl.Enabled = true;
        }
        private void BtnGeriAl_Click(object sender, EventArgs e)
        {
            var dekForm = Application.OpenForms
                .OfType<DeklarasyonForm>()
                .FirstOrDefault(f => f.Visible);

            if (dekForm != null)
                dekForm.SonHamleyiGeriAl();
            else
                OyunSonHamleyiGeriAl();

            GeriAlIleriAlButonlariniGuncelle();
        }

        private void BtnileriAl_Click(object sender, EventArgs e)
        {
            var dekForm = Application.OpenForms
                .OfType<DeklarasyonForm>()
                .FirstOrDefault(f => f.Visible);

            if (dekForm != null)
                dekForm.SonHamleyiIleriAl();
            else
                OyunSonHamleyiIleriAl();

            GeriAlIleriAlButonlariniGuncelle();
        }

        public void GeriAlIleriAlButonlariniGuncelle()
        {
            if (BtnGeriAl == null || BtnileriAl == null) return;

            var dekForm = Application.OpenForms
                .OfType<DeklarasyonForm>()
                .FirstOrDefault(f => f.Visible);

            if (dekForm != null)
            {
                BtnGeriAl.Enabled = dekForm.GeriAlinabilirMi;
                BtnileriAl.Enabled = dekForm.IleriAlinabilirMi;
            }
            else
            {
                BtnGeriAl.Enabled = false;
                BtnileriAl.Enabled = false;
            }
        }

        private void OyunSonHamleyiGeriAl()
        {
            System.Diagnostics.Debug.WriteLine(
                "[BricOyna] OyunSonHamleyiGeriAl: Henüz implemente edilmedi.");
        }

        private void OyunSonHamleyiIleriAl()
        {
            System.Diagnostics.Debug.WriteLine(
                "[BricOyna] OyunSonHamleyiIleriAl: Henüz implemente edilmedi.");
        }

        /// <summary>
        /// Belirtilen oyuncunun elini döndürür.
        /// (Ihale motoru için.)
        /// </summary>
        public List<Card> GetOyuncuEli(Player oyuncu)
        {
            switch (oyuncu)
            {
                case Player.Kuzey: return kuzeyEl;
                case Player.Guney: return guneyEl;
                case Player.Bati: return batiEl;
                case Player.Dogu: return doguEl;
                default: return null;
            }
        }

    }
}
