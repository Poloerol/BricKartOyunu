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
        // EL BİLGİ / TEST PANELİ
        private Panel elBilgiPanel;
        //private Label lblElBilgiNumara;
        private Label lblNewLabel;
        private Label lblOldLabel;
       
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
        // KART OYNAMA GEÇMİŞİ — BtnGeriAl / BtnileriAl (oyun aşaması)
        // ====================================================================
        // Her oynanan kart için, o hamleden ÖNCEKİ durumun kopyası saklanır.
        private sealed class OyunHamlesi
        {
            public Player Oyuncu;
            public Card Kart;
            public string AtakRengi;
            public bool ElBittiBekliyor;
            public Player SonElKazanani;
            public int ToplamEl;
            public int NS;
            public int EW;
            public bool DummyAcildi;
            public List<(Player Oyuncu, Card Kart)> BuEldeOnce;
            public int TumEllerSayisi;
        }

        private readonly List<OyunHamlesi> _oyunGecmisi = new List<OyunHamlesi>();       // oynananlar
        private readonly List<OyunHamlesi> _oyunIleriAlYigini = new List<OyunHamlesi>(); // geri alınanlar
        private bool _ileriAliniyor = false;

        private void OyunGecmisiniSifirla()
        {
            _oyunGecmisi.Clear();
            _oyunIleriAlYigini.Clear();
            _ileriAliniyor = false;
            TumAIyiDurdur();
        }

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
        // ═══════════════════════════════════════════════════════════════════
        // SEED TABANLI KART DAĞITIMI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Bu elin seed değeri. Aynı seed → aynı dağıtım.
        /// 0 veya negatif ise rastgele üretilir.
        /// </summary>
        private int _seed = 0;

        /// <summary>
        /// Son kullanılan seed (UI'da göstermek için).
        /// </summary>
        public int Seed => _seed;

        /// <summary>
        /// Yeni bir seed üretir (rastgele).
        /// </summary>
        private static int YeniSeed()
        {
            return new Random().Next(1, int.MaxValue);
        }
        // ═══════════════════════════════════════════════════════════════════
        // SEED YÖNETİMİ — ComboBox + Butonlar
        // ═══════════════════════════════════════════════════════════════════

        private ComboBox cmbSeed;
        private Button btnSeedSil;
        private Button btnSeedDagit;

        /// <summary>
        /// Son 10 seed (FIFO). Uygulama kapanınca kaybolur.
        /// </summary>
        private readonly Queue<int> _seedHafizasi = new Queue<int>(10);
        private const int MaxSeedHafiza = 10;
        private Label lblBilgiSeed;

        // 🔹 EKLE: Seed'lerin kaydedileceği dosya yolu
        private static readonly string SeedDosyaYolu = Path.Combine(Application.StartupPath, "seedler.txt");

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

            // 🔹 Otomatik oynatma butonu
            if (BtnOtoOyna != null)
                BtnOtoOyna.Click += BtnOtoOyna_Click;
            FirstClickSorununuCoz(BtnOtoOyna, BtnOtoOyna_Click);

            // 🔹 Claim butonu
            if (BtnClaim != null)
                BtnClaim.Click += BtnClaim_Click;
            FirstClickSorununuCoz(BtnClaim, BtnClaim_Click);
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

            ////Seed numarası
            //_seed = 1196129724;  // TEST: aynı seed

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

            // 🔹 Seed geçmişini önce yükle (KartlariDagit üzerine yazmasın!)
            SeedHafizasiniYukle();


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
                // TEST 2: Yapay eller
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

                // El 3: 17 HP dengeli → 1NT beklenir
                var el3 = YapayElOlustur(
                    "♠KJ3 ♥QJ3 ♦AKQ5 ♣J87");
                TestTekEl(motor, el3, "Yapay-3: 17 HP dengeli (StrongNT aralığı)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1NT, Gelen: {motor.TeklifVer(DurumOlustur(el3, anlasma))}");

                // El 4: 8 HP → Pas beklenir
                var el4 = YapayElOlustur(
                    "♠KJ3 ♥Q432 ♦J52 ♣J87");
                TestTekEl(motor, el4, "Yapay-4: 8 HP");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: Pas, Gelen: {motor.TeklifVer(DurumOlustur(el4, anlasma))}");

                // El 5: 16 HP dengeli, 5'li majör yok → 1NT beklenir
                var el5 = YapayElOlustur(
                    "♠KJ3 ♥QJ3 ♦AKQ5 ♣J87");
                TestTekEl(motor, el5, "Yapay-5: 16 HP dengeli, 5'li majör yok");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1NT, Gelen: {motor.TeklifVer(DurumOlustur(el5, anlasma))}");

                // El 6: 15 HP + 5'li Maça → 1♠ beklenir
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

                // El 9: 20 HP + 5'li Maça → 1♠ beklenir
                var el9 = YapayElOlustur(
                    "♠AKQ54 ♥KQ3 ♦KQ2 ♣T8");
                TestTekEl(motor, el9, "Yapay-9: 20 HP + 5'li Maça → 1♠ (2♣ değil)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♠, Gelen: {motor.TeklifVer(DurumOlustur(el9, anlasma))}");

                // El 10: 12 HP dengeli, 4'lü Karo → 1♦ beklenir
                var el10 = YapayElOlustur(
                    "♠KJ3 ♥Q43 ♦KQ52 ♣J87");
                TestTekEl(motor, el10, "Yapay-10: 12 HP dengeli, 4'lü Karo");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♦, Gelen: {motor.TeklifVer(DurumOlustur(el10, anlasma))}");

                // El 11: 14 HP + 4'lü Karo → 1♦ beklenir
                var el11 = YapayElOlustur(
                    "♠KJ3 ♥Q43 ♦QJ52 ♣AJ8");
                TestTekEl(motor, el11, "Yapay-11: 14 HP, 4'lü Karo");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♦, Gelen: {motor.TeklifVer(DurumOlustur(el11, anlasma))}");

                // El 12: 13 HP + 4'lü Karo, 4'lü Sinek → 1♦ beklenir
                var el12 = YapayElOlustur(
                    "♠KJ3 ♥QJ4 ♦KQ52 ♣J87");
                TestTekEl(motor, el12, "Yapay-12: 13 HP, 4'lü Karo, 4'lü Sinek");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♦, Gelen: {motor.TeklifVer(DurumOlustur(el12, anlasma))}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 3: BasitCevap Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- BASİT CEVAP TESTLERİ ---");

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

                // Yapay-14: Partner 1♠, ben 14 HP + 5 Kupa dengeli → 2NT beklenir
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
                    "[TEST] Yapay-14: Partner 1♠, ben 14 HP + 5 Kupa dengeli → 2NT beklenir");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el14)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum14)}");

                // Yapay-15: Partner 1♠, ben 12 HP dengeli → 2NT beklenir
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

                // ═══════════════════════════════════════════════════════════════
                // TEST 4: Stayman Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- STAYMAN TESTLERİ ---");

                // Yapay-16: Partner 1NT, ben 4'lü Maça + 10 HP → 2♣ (Stayman sorusu)
                var el16 = YapayElOlustur("♠KJ43 ♥Q43 ♦J52 ♣QJ8");
                var durum16 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el16,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-16: Partner 1NT, ben 4'lü Maça + 10 HP → 2♣ (Stayman)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el16)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum16)}");

                // Yapay-17: Ben 1NT açtım, partner 2♣ dedi, 4'lü Maça → 2♠
                var el17 = YapayElOlustur("♠KJ43 ♥Q43 ♦J52 ♣QJ8");
                var durum17 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Kuzey,
                    AktifOyuncuEli = el17,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-17: Ben 1NT, partner 2♣ (Stayman), 4'lü Maça → 2♠");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el17)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum17)}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 5: JacobyTransfer Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- JACOBY TRANSFER TESTLERİ ---");

                // Yapay-18: Partner 1NT, ben 5+ Kupa → 2♦ (Kupa transferi)
                var el18 = YapayElOlustur("♠Q43 ♥KJ543 ♦J52 ♣QJ");
                var durum18 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el18,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-18: Partner 1NT, ben 5+ Kupa → 2♦ (Kupa transferi)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el18)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum18)}");

                // Yapay-19: Partner 1NT, ben 5+ Maça → 2♥ (Maça transferi)
                var el19 = YapayElOlustur("♠KJ543 ♥Q43 ♦J52 ♣QJ");
                var durum19 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el19,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-19: Partner 1NT, ben 5+ Maça → 2♥ (Maça transferi)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el19)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum19)}");

                // Yapay-20: Ben 1NT, partner 2♦ (Kupa transferi) → 2♥ kabul
                var el20 = YapayElOlustur("♠KJ3 ♥Q43 ♦KQ52 ♣AJ8");
                var durum20 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Kuzey,
                    AktifOyuncuEli = el20,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♦", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-20: Ben 1NT, partner 2♦ (Kupa transferi) → 2♥ kabul");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el20)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum20)}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 6: NegativeDouble Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- NEGATİF KONTR TESTLERİ ---");

                // Yapay-21: Rakip 1♠ açtı, ben 4'lü Kupa + 10 HP → Dbl
                var el21 = YapayElOlustur("♠Q43 ♥KJ43 ♦QJ2 ♣Q87");
                var durum21 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el21,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "1♠", Sira = 1 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-21: Rakip 1♠ açtı, ben 4'lü Kupa + 10 HP → Dbl");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el21)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum21)}");

                // Yapay-22: Rakip 1♥ açtı, ben 4'lü Maça + 10 HP → Dbl
                var el22 = YapayElOlustur("♠KJ43 ♥Q43 ♦QJ2 ♣Q87");
                var durum22 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el22,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "1♥", Sira = 1 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-22: Rakip 1♥ açtı, ben 4'lü Maça + 10 HP → Dbl");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el22)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum22)}");

                // Yapay-23: Rakip 1♦ açtı, ben 4'lü Maça + 4'lü Kupa → Dbl
                var el23 = YapayElOlustur("♠KJ43 ♥QJ43 ♦Q2 ♣Q87");
                var durum23 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el23,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "1♦", Sira = 1 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-23: Rakip 1♦ açtı, ben 4'lü Maça + 4'lü Kupa → Dbl");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el23)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum23)}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 7: Blackwood / RKCB Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- BLACKWOOD / RKCB TESTLERİ ---");

                // Yapay-31: Partner 1♠ açtı, ben 3♠ dedim, partner Pas → ben 4NT soracağım
                var el31 = YapayElOlustur("♠KQ54 ♥AJ3 ♦KQ2 ♣K32");  // ♣A K 2 = 3 kart
                var durum31 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el31,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 5 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 6 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-31: Partner 1♠, ben 3♠, partner Pas → ben Blackwood sorusu → 4NT");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el31)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el31)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum31)}");

                // Yapay-32: Partner 4NT dedi, ben 0 As → 5♣
                var el32 = YapayElOlustur("♠Q43 ♥KJ43 ♦QJ2 ♣Q87");
                var durum32 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el32,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4NT", Sira = 5 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 6 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-32: Partner 4NT dedi, ben 0 As → 5♣");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el32)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el32)}, " +
                    $"Kilit kart (♠): {ElDegerlendirici.KilitKartSayisi(el32, "Maça")}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum32)}");

                // Yapay-33: Partner 4NT dedi, ben 1 As → 5♦
                var el33 = YapayElOlustur("♠A43 ♥KJ43 ♦QJ2 ♣Q87");
                var durum33 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el33,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4NT", Sira = 5 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 6 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-33: Partner 4NT dedi, ben 1 As → 5♦");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el33)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el33)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum33)}");

                // Yapay-34: Partner 4NT dedi, ben 2 As → 5♥
                var el34 = YapayElOlustur("♠A43 ♥AJ43 ♦QJ2 ♣Q87");
                var durum34 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el34,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4NT", Sira = 5 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 6 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-34: Partner 4NT dedi, ben 2 As → 5♥");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el34)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el34)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum34)}");

                // Yapay-35: Partner 4NT dedi, ben 2 kilit kart (1 As + koz K) → 5♠
                var el35 = YapayElOlustur("♠KJ43 ♥AJ43 ♦QJ2 ♣Q8");
                var durum35 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el35,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4NT", Sira = 5 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 6 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-35: Partner 4NT dedi, ben 2 kilit kart (1 As + ♠K) → 5♠");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el35)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el35)}, " +
                    $"Papaz: {ElDegerlendirici.PapazSayisi(el35)}, " +
                    $"Kilit kart (♠): {ElDegerlendirici.KilitKartSayisi(el35, "Maça")}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum35)}");
                System.Diagnostics.Debug.WriteLine("");

                // ═══════════════════════════════════════════════════════════════
                // TEST 8: Cue Bid Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- CUE BID TESTLERİ ---");

                // Yapay-41: 1♠-3♠ sonrası, elimde ♣A var → 4♣ cue bid
                var el41 = YapayElOlustur("♠KQ54 ♥AJ3 ♦KQ2 ♣A32");
                var durum41 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el41,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-41: Partner 1♠, ben 3♠, elimde ♣A → 4♣ cue bid");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el41)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el41)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum41)}");

                // Yapay-42: 1♥-3♥ sonrası, elimde ♣K var → 4♣ cue bid
                var el42 = YapayElOlustur("♠AJ3 ♥KQ54 ♦KQ2 ♣K32");
                var durum42 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el42,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♥", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-42: Partner 1♥, ben 3♥, elimde ♣K → 4♣ cue bid");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el42)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum42)}");

                // Yapay-43: 1♠-3♠ sonrası, ♣'te kontrol yok ama ♦'da A var → 4♦ cue bid
                var el43 = YapayElOlustur("♠KQ54 ♥AJ3 ♦A32 ♣Q87");
                var durum43 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el43,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-43: Partner 1♠, ben 3♠, ♣'te kontrol yok ama ♦'da A → 4♦ cue bid");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el43)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum43)}");

                // Yapay-44: 1♠-3♠ sonrası, sadece 12 HP → cue bid değil, Blackwood da değil
                var el44 = YapayElOlustur("♠KQ54 ♥QJ3 ♦QJ2 ♣Q87");
                var durum44 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el44,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-44: Partner 1♠, ben 3♠, sadece 12 HP → cue bid yapılmaz");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el44)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum44)}");
                System.Diagnostics.Debug.WriteLine("");

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

            // 🔹 Kontrol: 13 kart olmalı
            if (el.Count != 13)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[YapayElOlustur] UYARI: El {el.Count} kart! " +
                    $"Beklenen: 13. Girdi: '{metin}'");
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

            // 2. KUZEY KARTLARI (İlklendirme + dummy tıklama)
            for (int i = 0; i < 13; i++)
            {
                if (kuzeyKartlar[i] == null)
                {
                    kuzeyKartlar[i] = new PictureBox
                    {
                        Width = _kartGenislik,
                        Height = _kartYukseklik,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        BorderStyle = BorderStyle.FixedSingle,
                        Cursor = Cursors.Hand
                    };
                    this.Controls.Add(kuzeyKartlar[i]);

                    // Deklaran Güney iken dummy (Kuzey) kartları tıklanabilir
                    int kartIndex = i;
                    kuzeyKartlar[i].Click += (s, ev) => KuzeyKartTiklandi(kartIndex);
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
            SetupSeedPanel();
            if (oyunBilgiPanel != null)
            {
                oyunBilgiPanel.Location = new Point(
                    this.ClientSize.Width - oyunBilgiPanel.Width - BilgiPanelSagBosluk,
                    _kuzeyY   // ← Kuzey elinin Y koordinatı
                );
                OyunBilgiPaneliniGuncelle();
            }
            // 🔹 Seed paneli: Oyun Bilgisi panelinin hemen altında (10 px boşluk)
            if (oyunBilgiPanel != null && seedPanel != null)
            {
                seedPanel.Location = new Point(
                    oyunBilgiPanel.Left,
                    oyunBilgiPanel.Bottom + 10
                );
                seedPanel.Width = oyunBilgiPanel.Width;
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
                // Başlangıç boyutu, dinamik olarak güncellenecek
                Size = new Size(BilgiPanelGenislik, 300),
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
            lblBilgiSeed = BilgiSatiriOlustur(icerik, "Seed:", y); y += satirYuksekligi;

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
            y += lblBilgiKazanilanEl.Height; // 🔹 En alt elemanın yüksekliğini de ekle

            // ═══════════════════════════════════════════════════════════════
            // DÜZELTME: Paneli içeriğin gerçek yüksekliğine göre boyutlandır
            // ═══════════════════════════════════════════════════════════════
            int icerikGercekYuksekligi = y + icerik.Padding.Top + icerik.Padding.Bottom;
            int yeniPanelYuksekligi = lblBaslik.Height + icerikGercekYuksekligi + 4; // +4 kenarlık payı

            oyunBilgiPanel.Height = yeniPanelYuksekligi;

            this.Controls.Add(oyunBilgiPanel);
            oyunBilgiPanel.BringToFront();

            // 🔹 SEED KISMI BURADAN TAMAMEN SİLİNDİ. 
            // SetupSeedPanel() zaten kendi panelini oluşturuyor.
        }

        /// <summary>
        /// Oyun bilgi panelinin hemen altına Seed Yönetimi panelini koyar.
        /// </summary>
        private void SetupSeedPanel()
        {
            if (seedPanel != null) return;

            seedPanel = new Panel
            {
                Size = new Size(BilgiPanelGenislik, 90),
                BackColor = Color.FromArgb(245, 245, 220),
                BorderStyle = BorderStyle.FixedSingle
            };
            EnableDoubleBuffer(seedPanel);

            // ─── Başlık ───
            var lblBaslik = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Text = "SEED YÖNETİMİ",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 90)
            };
            seedPanel.Controls.Add(lblBaslik);

            // ─── İçerik paneli ───
            var icerik = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(6, 4, 6, 4),
                BackColor = Color.Transparent
            };
            seedPanel.Controls.Add(icerik);
            icerik.BringToFront();

            // ─── ComboBox ───
            int y = 4;
            var lblSeed = new Label
            {
                Location = new Point(6, y),
                Size = new Size(42, 22),
                Text = "Seed:",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
            icerik.Controls.Add(lblSeed);

            cmbSeed = new ComboBox
            {
                Location = new Point(50, y),
                Size = new Size(155, 22),
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            icerik.Controls.Add(cmbSeed);

            // ─── Butonlar (ComboBox altında) ───
            y += 30;

            btnSeedSil = new Button
            {
                Location = new Point(6, y),
                Size = new Size(70, 26),
                Text = "Sil",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true
            };
            btnSeedSil.Click += BtnSeedSil_Click;
            icerik.Controls.Add(btnSeedSil);

            btnSeedDagit = new Button
            {
                Location = new Point(82, y),
                Size = new Size(90, 26),
                Text = "Dağıt",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true
            };
            btnSeedDagit.Click += BtnSeedDagit_Click;
            icerik.Controls.Add(btnSeedDagit);

            this.Controls.Add(seedPanel);
            seedPanel.BringToFront();

            System.Diagnostics.Debug.WriteLine(
                "[SetupSeedPanel] Seed paneli oluşturuldu.");
        }

        /// <summary>
        /// ComboBox'ı günceller (son seed'ler).
        /// </summary>
        private void ComboBoxGuncelle()
        {
            if (cmbSeed == null) return;

            cmbSeed.Items.Clear();
            foreach (var seed in _seedHafizasi)
            {
                cmbSeed.Items.Add(seed);
            }

            if (cmbSeed.Items.Count > 0)
                cmbSeed.SelectedIndex = cmbSeed.Items.Count - 1;
        }



        /// <summary>
        /// Dosyadan seed geçmişini okur.
        /// </summary>
        private void SeedHafizasiniYukle()
        {
            if (!File.Exists(SeedDosyaYolu)) return;

            try
            {
                foreach (var satir in File.ReadAllLines(SeedDosyaYolu))
                {
                    if (int.TryParse(satir.Trim(), out int seed))
                    {
                        if (!_seedHafizasi.Contains(seed))
                        {
                            if (_seedHafizasi.Count >= MaxSeedHafiza)
                                _seedHafizasi.Dequeue();
                            _seedHafizasi.Enqueue(seed);
                        }
                    }
                }
                ComboBoxGuncelle();
                System.Diagnostics.Debug.WriteLine($"[SeedYukle] {_seedHafizasi.Count} seed dosyadan yüklendi.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SeedYukle] Hata: {ex.Message}");
            }
        }




        /// <summary>
        /// Bellekteki seed geçmişini dosyaya yazar.
        /// </summary>
        private void SeedHafizasiniKaydet()
        {
            try
            {
                File.WriteAllLines(SeedDosyaYolu, _seedHafizasi.Select(s => s.ToString()));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SeedKaydet] Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Seed'i hafızaya ekler (FIFO, max 10).
        /// </summary>
        private void SeedHafizayaEkle(int seed)
        {
            if (_seedHafizasi.Contains(seed)) return;

            if (_seedHafizasi.Count >= MaxSeedHafiza)
                _seedHafizasi.Dequeue();

            _seedHafizasi.Enqueue(seed);
            ComboBoxGuncelle();

            // 🔹 EKLE: Her yeni seed eklendiğinde dosyaya kaydet
            SeedHafizasiniKaydet();

            System.Diagnostics.Debug.WriteLine(
                $"[SeedHafizasi] Eklendi: {seed} (toplam: {_seedHafizasi.Count})");
        }

        /// <summary>
        /// "Sil" butonu: seçili seed'i siler.
        /// </summary>
        private void BtnSeedSil_Click(object sender, EventArgs e)
        {
            if (cmbSeed == null || string.IsNullOrWhiteSpace(cmbSeed.Text)) return;

            if (!int.TryParse(cmbSeed.Text.Trim(), out int seciliSeed)) return;

            var gecici = _seedHafizasi.Where(s => s != seciliSeed).ToList();
            _seedHafizasi.Clear();
            foreach (var s in gecici)
                _seedHafizasi.Enqueue(s);

            ComboBoxGuncelle();
            SeedHafizasiniKaydet();

            System.Diagnostics.Debug.WriteLine(
                $"[SeedSil] Silindi: {seciliSeed} (kalan: {_seedHafizasi.Count})");
        }

        /// <summary>
        /// "Dağıt" butonu: seçili seed ile el dağıtır.
        /// </summary>
        private void BtnSeedDagit_Click(object sender, EventArgs e)
        {
            if (cmbSeed == null || string.IsNullOrWhiteSpace(cmbSeed.Text))
            {
                MessageBox.Show("Lütfen bir seed değeri girin veya seçin.", "Seed Dağıt",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 🔹 Değişiklik: Sadece seçili elemanı değil, kutudaki metni oku (manuel giriş için)
            if (!int.TryParse(cmbSeed.Text.Trim(), out int seciliSeed))
            {
                MessageBox.Show("Geçersiz seed formatı! Lütfen sadece sayı girin.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var cevap = MessageBox.Show(
                $"Seed: {seciliSeed}\n\n" +
                "Mevcut el kaybolacak ve seçili seed ile yeni el dağıtılacak.\n" +
                "Devam etmek istiyor musunuz?",
                "Seed ile Dağıt",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (cevap != DialogResult.Yes) return;

            KartlariDagitSeedIle(AnaSayfa.SeciliKartSeti, seciliSeed);

            _oyunAsamasi = OyunAsamasi.Baslangic;
            _currentTour++;
            _dealer = SonrakiOyuncu(_dealer);
            activePlayer = _dealer;
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;
            _kontrat = null;
            _kontratDeklaran = _dealer;

            ButonlariInaktifYap();
            UpdateInfoBoard(_dealer);
            OyunBilgiPaneliniGuncelle();
            YeniDekBasFormAc();

            System.Diagnostics.Debug.WriteLine(
                $"[SeedDagit] Manuel giriş/Seçim ile dağıtıldı: {seciliSeed}");
        }

        /// <summary>
        /// OYUN BİLGİSİ panelinin 10px altında yer alan el numarası panelini oluşturur.
        /// </summary>
        private void SetupElBilgiPanel()
        {
            if (elBilgiPanel != null) return;

            // İki stil numarasını da rahatça barındırabilecek yükseklikte bir panel
            elBilgiPanel = new Panel
            {
                Size = new Size(BilgiPanelGenislik, 95),
                BackColor = AnaSayfa.MasaRengi, // Masa rengiyle aynı
                BorderStyle = BorderStyle.FixedSingle
            };
            EnableDoubleBuffer(elBilgiPanel);

            // 1. New Style Başlık ve Değer
            var lblNewBaslik = new Label
            {
                Location = new Point(5, 4),
                Size = new Size(BilgiPanelGenislik - 10, 18),
                Text = "New Style Deal #:",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.Black
            };
            elBilgiPanel.Controls.Add(lblNewBaslik);

            lblNewLabel = new Label
            {
                Location = new Point(5, 22),
                Size = new Size(BilgiPanelGenislik - 10, 25),
                Text = "N0112-49594-...",
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.Black
            };
            elBilgiPanel.Controls.Add(lblNewLabel);

            // 2. Old Style Başlık ve Değer
            var lblOldBaslik = new Label
            {
                Location = new Point(5, 50),
                Size = new Size(BilgiPanelGenislik - 10, 18),
                Text = "Old Style Deal #:",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.Black
            };
            elBilgiPanel.Controls.Add(lblOldBaslik);

            lblOldLabel = new Label
            {
                Location = new Point(5, 68),
                Size = new Size(BilgiPanelGenislik - 10, 20),
                Text = "1955946097",
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.Black
            };
            elBilgiPanel.Controls.Add(lblOldLabel);

            this.Controls.Add(elBilgiPanel);
            elBilgiPanel.BringToFront();
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

            // 🔹 Seed
            if (lblBilgiSeed != null)
                lblBilgiSeed.Text = _seed.ToString();

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

            // "D" harfi HER ZAMAN dağıtıcıyı (dealer) gösterir
            UpdateBox(lblNorth, nsZonda, _dealer == Player.Kuzey);
            UpdateBox(lblSouth, nsZonda, _dealer == Player.Guney);
            UpdateBox(lblEast, ewZonda, _dealer == Player.Dogu);
            UpdateBox(lblWest, ewZonda, _dealer == Player.Bati);

            // 🔹 YENİ: Oyun bilgi panelindeki Zon ve Dealer'ı da güncelle
            OyunBilgiPaneliniGuncelle();
        }

        private void UpdateBox(Label lbl, bool isZonActive, bool dagiticiMi = false)
        {
            // Dağıtıcının kutusunda "D" harfi gösterilir
            lbl.Text = dagiticiMi ? "D" : string.Empty;

            // Zonda ise Kırmızı, zonsuz ise Beyaz yapıyoruz
            lbl.BackColor = isZonActive ? Color.Red : Color.White;
            lbl.ForeColor = isZonActive ? Color.White : Color.Black;
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
            if (!InsanKontrolundeMi(activePlayer))
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
        /// Bord bittiğinde dört oyuncunun 13'er kartlık ellerini açık gösterir.
        /// Kartlar oynandıkça ellerden çıkarıldığı için eller, oynanan kart kaydından
        /// (_oynananTumEller) yeniden kurulur. Renk sırası GetSuitSirasi ile aynıdır
        /// (koz ilk grup; NT'de ♠ ♥ ♣ ♦). Masadaki son elin kartları temizlenir.
        /// </summary>
        private void BordSonuElleriniGoster()
        {
            foreach (var oyuncu in new[] { Player.Kuzey, Player.Guney, Player.Bati, Player.Dogu })
            {
                var el = OyuncuEli(oyuncu);
                if (el == null) continue;

                // Eldeki (henüz oynanmamış) kartlar + oynanmış kartlar = başlangıçtaki 13 kart.
                // Normal bitişte el boştur; Claim'de kalan kartlar elde durur.
                foreach (var k in _oynananTumEller.Where(t => t.Oyuncu == oyuncu).Select(t => t.Kart))
                    if (!el.Contains(k)) el.Add(k);
            }

            MasadakiKartlariTemizle();

            this.SuspendLayout();
            try
            {
                HepsiniGoster();
            }
            finally
            {
                this.ResumeLayout(true);
            }

            this.Refresh();
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

            // Bord bitti: dört elin tamamı açık, masa boş olsun (form açılmadan önce çizilir)
            BordSonuElleriniGoster();

            using (var form = new DealCompleteForm(this))
            {
                form.StartPosition = FormStartPosition.CenterParent;
                var sonuc = form.ShowDialog(this);

                if (sonuc == DialogResult.Retry)
                {
                    // "Replay" → aynı bord baştan
                    BordTekrariBaslat();
                }
                else if (sonuc == DialogResult.OK)
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
        /// <summary>
        /// Replay: aynı bord (aynı seed, aynı dealer) ilk dağıtım konumuna döner,
        /// yalnızca Güney açık, DekBasForm açılır.
        /// </summary>
        public void BordTekrariBaslat()
        {
            _oynananTumEller.Clear();
            OyunGecmisiniSifirla();
            _oynananToplamEl = 0;
            _kazanilanElNS = 0;
            _kazanilanElEW = 0;
            MasadakiKartlariTemizle();
            _buEldeOynananlar.Clear();
            _buEldeAtakRengi = null;
            _elBittiBekliyor = false;
            _dummyAcildi = false;

            _oyunAsamasi = OyunAsamasi.Baslangic;
            activePlayer = _dealer;
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;
            _kontrat = null;
            _kontratDeklaran = _dealer;

            ButonlariInaktifYap();
            UpdateInfoBoard(_dealer);
            OyunBilgiPaneliniGuncelle();

            // Aynı seed ile ilk dağıtım (takaslar geri alınır, sadece Güney açık)
            KartlariDagitSeedIle(AnaSayfa.SeciliKartSeti, _seed);

            YeniDekBasFormAc();
        }

        public void YeniBordBaslat()
        {
            System.Diagnostics.Debug.WriteLine(
                "[YeniBord] Yeni bord başlatılıyor...");

            // 🔹 Oynanan tüm eller listesini temizle
            _oynananTumEller.Clear();
            OyunGecmisiniSifirla();

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
            KartlariDagitRastgele(AnaSayfa.SeciliKartSeti);

            // 8) Yeni DekBasForm aç
            YeniDekBasFormAc();
        }

        private Image GetBackImage()
        {
            return GetCachedImage(CurrentBackImage);
        }

        private void KartlariDagit(string kartSeti)
        {
            // 🔹 Seed yoksa üret
            if (_seed <= 0)
            {
                _seed = YeniSeed();
            }

            // 🔹 Seed'e bağlı deterministik Random
            var rng = new Random(_seed);

            System.Diagnostics.Debug.WriteLine(
                $"[KartlariDagit] Seed: {_seed}");

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

                // 🔹 Fisher-Yates shuffle (seed'li rng)
                for (int i = kartlar.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
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
                // 🔹 Seed'i hafızaya ekle
                SeedHafizayaEkle(_seed);
            }

            finally
            {
                this.ResumeLayout(true);
            }
        }

        /// <summary>
        /// Belirli bir seed ile el dağıtır.
        /// Aynı seed + aynı kart seti → aynı dağıtım.
        /// Test için: aynı eli tekrar dağıtmak istediğinde kullan.
        /// </summary>
        // Deklaran Kuzey olunca eller N↔S ve E↔W takas edilir (180° döndürme).
        // PBN dışa aktarmada dağıtıcı/ihale koltukları buna göre döndürülür.
        private bool _ellerIhaleSonrasiTakasli = false;

        public void KartlariDagitSeedIle(string kartSeti, int seed)
        {
            _ellerIhaleSonrasiTakasli = false;
            _seed = seed;
            KartlariDagit(kartSeti);
            System.Diagnostics.Debug.WriteLine(
                $"[KartlariDagitSeedIle] Seed: {seed}");
        }

        /// <summary>
        /// Rastgele seed ile el dağıtır (mevcut davranış).
        /// Yeni bord başlarken kullanılır.
        /// </summary>
        public void KartlariDagitRastgele(string kartSeti)
        {
            _seed = YeniSeed();
            KartlariDagit(kartSeti);
            System.Diagnostics.Debug.WriteLine(
                $"[KartlariDagitRastgele] Yeni seed: {_seed}");
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

        /// <summary>
        /// İki oyuncunun ellerini KENDİ ARALARINDA takas eder (Güney'e dokunmaz).
        /// ElleriTakasEt'ten farklı olarak Güney'i karıştırmaz.
        /// </summary>
        public void ElleriKendiAralarindaTakasEt(Player a, Player b)
        {
            var elA = OyuncuEli(a);
            var elB = OyuncuEli(b);
            if (elA == null || elB == null) return;

            var gecici = new List<Card>(elA);
            elA.Clear();
            elA.AddRange(elB);
            elB.Clear();
            elB.AddRange(gecici);
        }

        /// <summary>
        /// İhale Kuzey'de kaldığında çağrılır.
        /// N↔S ve E↔W elleri takas edilir. Sonuç:
        ///   - Güney (insan) eski Kuzey elini oynar → Deklaran
        ///   - Kuzey dummy olur (eski Güney eli)
        ///   - Doğu↔Batı döner → ihalede söylenen renk pozisyonu kaymaz
        ///   - Batı (eski Doğu eli) açılış kartını oynar
        /// </summary>
        public void DeklarasyonElDegisiminiUygula()
        {
            ElleriKendiAralarindaTakasEt(Player.Kuzey, Player.Guney);
            ElleriKendiAralarindaTakasEt(Player.Dogu, Player.Bati);
            _ellerIhaleSonrasiTakasli = true;

            System.Diagnostics.Debug.WriteLine(
                "[ElDegisimi] N↔S ve E↔W takas edildi.");
        }

        /// <summary>
        /// Briç kuralı: Sadece dummy açıktır. İnsan (Güney) kendi elini her zaman
        /// görür (açık). Deklaran Güney değilse kapalı kalır. Diğer defans oyuncusu
        /// kapalıdır.
        /// 
        /// Açık gösterilecekler:
        ///   - Güney (insan) → her zaman
        ///   - Dummy (deklaranın ortağı)
        /// Kapalı gösterilecekler:
        ///   - Deklaran (Güney değilse)
        ///   - Diğer defans oyuncusu
        /// </summary>
        private void DeklaranVeDummyGoster()
        {
            // 🔹 KRİTİK DEĞİŞİKLİK: KartYerlesimiHazirla() ÇAĞIRMIYORUZ!
            // Çünkü o metot tüm kartları açık şekilde yerleştiriyor.
            // Bunun yerine sadece Güney ve Dummy'yi yerleştireceğiz.

            Image backImg = GetBackImage();

            // 1) TÜM KARTLARI KAPAT (Batı, Doğu, Kuzey)
            // (Güney'i en son açacağız)
            for (int i = 0; i < 13; i++)
            {
                if (kuzeyKartlar[i] != null)
                {
                    kuzeyKartlar[i].Image = backImg;
                    kuzeyKartlar[i].Visible = true;
                    if (backImg == null) kuzeyKartlar[i].BackColor = AnaSayfa.MasaRengi;
                }
                if (batiKartlar[i] != null)
                {
                    batiKartlar[i].Image = backImg;
                    batiKartlar[i].Visible = true;
                    if (backImg == null) batiKartlar[i].BackColor = AnaSayfa.MasaRengi;
                }
                if (doguKartlar[i] != null)
                {
                    doguKartlar[i].Image = backImg;
                    doguKartlar[i].Visible = true;
                    if (backImg == null) doguKartlar[i].BackColor = AnaSayfa.MasaRengi;
                }
                if (guneyKartlar[i] != null)
                {
                    guneyKartlar[i].Image = backImg;
                    guneyKartlar[i].Visible = true;
                    if (backImg == null) guneyKartlar[i].BackColor = AnaSayfa.MasaRengi;
                }
            }

            // 2) GÜNEY'İN ELİNİ AÇ (insan oyuncu)
            BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);

            // 3) DUMMY'Yİ AÇ
            Player dummy = IhaleDurumu.PartnerBul(_kontratDeklaran);

            if (dummy == Player.Kuzey)
            {
                BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, 250, _kuzeyY, false, true);
            }
            else if (dummy == Player.Bati)
            {
                BricStandartlarinaGoreDiz(batiKartlar, batiEl, 40, GetCenteredEWStartingY());
            }
            else if (dummy == Player.Dogu)
            {
                BricStandartlarinaGoreDiz(doguKartlar, doguEl, doguKartlar[0]?.Left ?? 930, GetCenteredEWStartingY());
            }

            // 4) Oyuncu badge'lerini güncelle
            SetupPlayerBadges();
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
                // Renk sırası: ihale sonrası koz en solda, sonra renkler dönüşümlü (bkz. GetSuitSirasi)
                List<List<Card>> tumRenkGruplari = GetSuitSirasi()
                    .Select(renk => eldekiKartlar.Where(k => k.Suit == renk).OrderByDescending(k => k.Value).ToList())
                    .ToList();
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
            // Renk sırası: ihale sonrası koz en üstte, sonra renkler dönüşümlü (bkz. GetSuitSirasi)
            List<List<Card>> tumSatirlar = GetSuitSirasi()
                .Select(renk => eldekiKartlar.Where(k => k.Suit == renk).OrderByDescending(k => k.Value).ToList())
                .Where(satir => satir.Count > 0)
                .ToList();
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

        // 🔹 Toolstrip "Oyundan Çık": tüm yan formları (deklarasyon, dekbaş vb.) kapatır,
        // ardından BricOyna'yı kapatır. AnaSayfa, BricOyna'yı ShowDialog ile açtığı için
        // dönüşte kendini gösterip ResetToInitialState() ile baştan başlar.
        private void BtnOyundanExit_Click(object sender, EventArgs e)
        {
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

            this.Close();
        }

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
        /// <summary>
        /// Bu oyuncunun kartlarını insan mı seçer?
        /// Güney her zaman; deklaran Güney ise dummy (Kuzey) de insan kontrolünde.
        /// </summary>
        private bool InsanKontrolundeMi(Player oyuncu)
        {
            if (_tumAI && _oyunAsamasi == OyunAsamasi.Oyun) return false;   // otomatik oynatma: herkes AI

            if (oyuncu == _humanPlayer) return true;

            if (_oyunAsamasi == OyunAsamasi.Oyun &&
                _kontratDeklaran == Player.Guney)
            {
                Player dummy = IhaleDurumu.PartnerBul(_kontratDeklaran);
                if (oyuncu == dummy) return true;
            }
            return false;
        }

        /// <summary>
        /// Renk takibi zorunluluğu: atak rengi varsa ve elde o renkten kart duruyorsa
        /// yalnızca o renkten kart oynanabilir. Atak yoksa her kart yasaldır.
        /// </summary>
        private bool YasalKartMi(Player oyuncu, Card kart)
        {
            if (string.IsNullOrEmpty(_buEldeAtakRengi)) return true;
            if (kart == null || kart.Suit == _buEldeAtakRengi) return true;

            var el = OyuncuEli(oyuncu);
            return el == null || !el.Any(c => c.Suit == _buEldeAtakRengi);
        }

        private void YasadisiKartUyarisi(Player oyuncu, Card kart)
        {
            System.Media.SystemSounds.Beep.Play();
            System.Diagnostics.Debug.WriteLine(
                $"[RenkTakibi] {oyuncu} {kart.Suit} {kart.Value} oynayamaz — {_buEldeAtakRengi} rengi takip edilmeli.");
        }

        private void GuneyKartTiklandi(int kartIndex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[GuneyKartTiklandi] index: {kartIndex}, aktif: {activePlayer}, aşama: {_oyunAsamasi}");

            if (_oyunAsamasi != OyunAsamasi.Oyun) return;
            if (_tumAI) return;   // otomatik oynatma sürerken elle oynanmaz

            // Kendi elinden oynama: sıra Güney'de olmalı
            if (activePlayer != Player.Guney)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[GuneyKartTiklandi] Sıra Güney'de değil — aktif: {activePlayer}");
                return;
            }

            if (kartIndex < 0 || kartIndex >= guneyKartlar.Length) return;

            var pb = guneyKartlar[kartIndex];
            if (pb == null || !pb.Visible) return;

            if (!(pb.Tag is Card card))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[GuneyKartTiklandi] Tag'de Card yok (#{kartIndex}).");
                return;
            }

            if (!YasalKartMi(Player.Guney, card))
            {
                YasadisiKartUyarisi(Player.Guney, card);
                return;
            }

            KartOyna(Player.Guney, card, pb.Image);
        }

        /// <summary>
        /// Dummy (Kuzey) kartına tıklama.
        /// Deklaran Güney iken sıra dummy'deyken insan Kuzey elinden oynar.
        /// </summary>
        private void KuzeyKartTiklandi(int kartIndex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[KuzeyKartTiklandi] index: {kartIndex}, aktif: {activePlayer}, " +
                $"deklaran: {_kontratDeklaran}, aşama: {_oyunAsamasi}");

            if (_oyunAsamasi != OyunAsamasi.Oyun) return;
            if (_tumAI) return;   // otomatik oynatma sürerken elle oynanmaz

            // Sadece deklaran Güney + sıra Kuzey (dummy)
            if (_kontratDeklaran != Player.Guney) return;
            if (activePlayer != Player.Kuzey)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[KuzeyKartTiklandi] Sıra dummy'de değil — aktif: {activePlayer}");
                return;
            }

            if (kartIndex < 0 || kartIndex >= kuzeyKartlar.Length) return;

            var pb = kuzeyKartlar[kartIndex];
            if (pb == null || !pb.Visible) return;

            if (!(pb.Tag is Card card))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[KuzeyKartTiklandi] Tag'de Card yok (#{kartIndex}).");
                return;
            }

            if (!YasalKartMi(Player.Kuzey, card))
            {
                YasadisiKartUyarisi(Player.Kuzey, card);
                return;
            }

            KartOyna(Player.Kuzey, card, pb.Image);
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

            // El bitti, masadaki 4 kart henüz temizlenmedi (PlayZone tıklaması bekleniyor).
            // Bu durumda kart oynanırsa önceki elin kartlarıyla karışır, oyuncular
            // aynı elde iki kez oynar ve eller dengesiz biter. Kartı kabul etme.
            if (_elBittiBekliyor || _oynananToplamEl >= 13)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] {oyuncu} kartı reddedildi — el/bord bitti, yeni el başlamadan kart oynanamaz.");
                return;
            }

            // Sıra bu oyuncuda mı?
            if (activePlayer != oyuncu)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] Sıra {oyuncu}'da değil, aktif: {activePlayer}");
                return;
            }

            // ── Geri Al / İleri Al: bu hamleden önceki durumu kaydet ──
            _oyunGecmisi.Add(new OyunHamlesi
            {
                Oyuncu = oyuncu,
                Kart = kart,
                AtakRengi = _buEldeAtakRengi,
                ElBittiBekliyor = _elBittiBekliyor,
                SonElKazanani = _sonElKazanani,
                ToplamEl = _oynananToplamEl,
                NS = _kazanilanElNS,
                EW = _kazanilanElEW,
                DummyAcildi = _dummyAcildi,
                BuEldeOnce = new List<(Player Oyuncu, Card Kart)>(_buEldeOynananlar),
                TumEllerSayisi = _oynananTumEller.Count
            });
            if (!_ileriAliniyor) _oyunIleriAlYigini.Clear();   // yeni bir kart = ileri alınacak bölüm geçersiz

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

            // İlk kart (açılış atağı) oynandı → dummy açılır
            if (!_dummyAcildi) DummyyiAc();

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

                // 🔹 Eli kazanan kartı masada sarıyla işaretle
                KazananKartiVurgula(kazanan);

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
                    GeriAlIleriAlButonlariniGuncelle();
                    BordTamamlandiFormuAc();
                    return;
                }

                // El bitti ama bord devam ediyor
                _sonElKazanani = kazanan;
                _elBittiBekliyor = true;
                GeriAlIleriAlButonlariniGuncelle();
                if (_tumAI && _tumAIDuraklamasiz) TumAIYeniEleZamanla();

                System.Diagnostics.Debug.WriteLine(
                    "[KartOyna] El bitti. PlayZone'a tıklayınca yeni el başlayacak.");

                return;
            }

            // ═══════════════════════════════════════════════════════════════
            // EL HENÜZ BİTMEDİ — Sırayı bir sonraki oyuncuya geçir
            // ═══════════════════════════════════════════════════════════════
            activePlayer = SonrakiOyuncu(activePlayer);
            GeriAlIleriAlButonlariniGuncelle();

            // Sıra AI'da mı? Otomatik oynat (İleri Al sırasında AI tetiklenmez)
            if (!_ileriAliniyor && !InsanKontrolundeMi(activePlayer))
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

            // ─── 2b. Kapalı Batı/Doğu: başlangıçtaki düz dikey yığın düzeni ───
            // (Renk renk dizim kalan kartlardan dağılım bilgisi sızdırır.)
            if ((oyuncu == Player.Bati || oyuncu == Player.Dogu) && !OyuncuEliAcikMi(oyuncu))
            {
                KapaliDikeyYiginiDuzenle(kartResimleri, el.Count, baslangicX);
                return;
            }

            // ─── 3. Kalan kartları yeniden diz ───
            // NOT: Zaten diğer kartlar dizili ama oynanan kart gizlenmediği için
            //      yeniden dizmek en temiz yol.
            if (yatayDizim)
            {
                BricStandartlarinaGoreDizYatay(kartResimleri, el, baslangicX, baslangicY, false, OyuncuEliAcikMi(oyuncu));
            }
            else
            {
                BricStandartlarinaGoreDiz(kartResimleri, el, baslangicX, baslangicY);
            }

            // ─── 4. Açık olmaması gereken oyuncunun kartlarını tekrar kapat ───
            // Dizim metotları kartları açık yüzle çizer; defans oyuncusu (ve deklaran
            // Güney değilse) yeniden dizimden sonra kapalı kalmalı.
            if (!OyuncuEliAcikMi(oyuncu))
            {
                Image backImg = GetBackImage();
                for (int i = 0; i < kartResimleri.Length; i++)
                {
                    if (kartResimleri[i] == null || !kartResimleri[i].Visible) continue;
                    if (backImg != null) kartResimleri[i].Image = backImg;
                    else { kartResimleri[i].Image = null; kartResimleri[i].BackColor = AnaSayfa.MasaRengi; }
                }
            }
        }

        /// <summary>
        /// Kapalı Batı/Doğu elini KartYerlesimiHazirla ile aynı düz dikey yığın olarak çizer.
        /// Kalan kart sayısı kadar kutu görünür; yığın aralığı ve konumu değişmez,
        /// böylece renk dağılımı hakkında hiçbir ipucu verilmez.
        /// </summary>
        private void KapaliDikeyYiginiDuzenle(PictureBox[] kartResimleri, int kalanKartSayisi, int x)
        {
            int maxEWToplamYukseklik = (4 * _kartYukseklik) + (3 * 2);
            int ewBaslangicY = _kuzeyY + 172;
            float dikeyAdim = (maxEWToplamYukseklik - _kartYukseklik) / 12f;
            if (dikeyAdim < 0) dikeyAdim = 0;

            Image backImg = GetBackImage();

            for (int i = 0; i < kartResimleri.Length; i++)
            {
                var pb = kartResimleri[i];
                if (pb == null) continue;

                if (i < kalanKartSayisi)
                {
                    if (backImg != null) pb.Image = backImg;
                    else { pb.Image = null; pb.BackColor = AnaSayfa.MasaRengi; }
                    pb.Left = x;
                    pb.Top = ewBaslangicY + (int)(i * dikeyAdim);
                    pb.Visible = true;
                    pb.BringToFront();
                }
                else
                {
                    pb.Visible = false;
                }
            }
        }

        /// <summary>
        /// Oyun aşamasında bu oyuncunun eli açık mı gösterilmeli?
        /// Güney (insan) her zaman; dummy (deklaranın ortağı) açık; diğerleri kapalı.
        /// "Hepsi / E-W / N-S göster" butonları aktifse o oyuncular da açık kalır.
        /// </summary>
        // Dummy, açılış atağı oynanana kadar kapalıdır.
        private bool _dummyAcildi = false;

        /// <summary>Açılış atağından sonra dummy'yi açar (yatay: Kuzey, dikey: Batı/Doğu).</summary>
        private void DummyyiAc()
        {
            if (_dummyAcildi) return;
            _dummyAcildi = true;

            Player dummy = IhaleDurumu.PartnerBul(_kontratDeklaran);
            if (dummy == Player.Kuzey)
                BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, 250, _kuzeyY, false, true);
            else if (dummy == Player.Bati)
                BricStandartlarinaGoreDiz(batiKartlar, batiEl, 40, GetCenteredEWStartingY());
            else if (dummy == Player.Dogu)
                BricStandartlarinaGoreDiz(doguKartlar, doguEl, doguKartlar[0]?.Left ?? 930, GetCenteredEWStartingY());

            System.Diagnostics.Debug.WriteLine($"[Dummy] {dummy} açıldı.");
        }

        private bool OyuncuEliAcikMi(Player oyuncu)
        {
            if (oyuncu == Player.Guney) return true;
            if (_oyunAsamasi != OyunAsamasi.Oyun) return true; // ihale aşamasında mevcut davranış korunur

            if (BtnHepsi != null && BtnHepsi.Checked) return true;
            if (BtnEW != null && BtnEW.Checked && (oyuncu == Player.Dogu || oyuncu == Player.Bati)) return true;
            if (BtnNS != null && BtnNS.Checked && oyuncu == Player.Kuzey) return true;

            return _dummyAcildi && oyuncu == IhaleDurumu.PartnerBul(_kontratDeklaran); // dummy (atak sonrası)
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

            // İnsan veya dummy (deklaran Güney iken Kuzey) → AI yok
            if (InsanKontrolundeMi(activePlayer))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AITetikle] {activePlayer} insan kontrolünde, AI yok.");
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

                if (!InsanKontrolundeMi(oynayacak))
                {
                    AIOyna(oynayacak);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[AITetikle] {oynayacak} insan kontrolünde, AIOyna yok.");
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
        /// Aktif oyuncu AI ise otomatik oynar.
        /// Basit ama briç mantığına uygun kart seçimi:
        /// 1. Koz varsa: kozu oyna (en düşük koz — küçük koz)
        /// 2. Renk takip zorunluysa: 
        ///    - Ortak As/K oynadıysa → en düşük
        ///    - Rakip yüksek oynadıysa → en yüksek (üçüncü el yüksek)
        ///    - İkinci el → en düşük (second hand low)
        ///    - Dördüncü el → kazanacak en düşük
        ///    - Diğer → en düşük
        /// 3. Renk yoksa: kozu oyna veya en düşük kart
        /// </summary>
        /// <summary>
        /// Aktif oyuncu AI ise otomatik oynar.
        /// 
        /// Briç mantığına uygun kart seçimi:
        /// 1. Açılış atağı: AtakKurallari (As, Rua, Dam atağı vs.)
        /// 2. Renk takip zorunluysa:
        ///    - Ortak As/K oynadıysa → en düşük
        ///    - Rakip Q+ oynadıysa → en yüksek
        ///    - İkinci el → en düşük (second hand low)
        ///    - Üçüncü el → en yüksek (third hand high)
        ///    - Dördüncü el → kazanacak en düşük, kazanamıyorsa en düşük
        /// 3. Renk yoksa:
        ///    - Rakip kazanıyorsa → en yüksek koz (trick'i kazan)
        ///    - Ortak kazanıyorsa → en düşük koz (üstüne koz verme)
        ///    - Koz yoksa → en düşük kart (ıskarta)
        /// Açılış atağında sıralı onör / marka kuralları (As, Rua, Dam atağı) uygulanır,
        /// diğer durumlarda renk takip + rastgele.
        /// </summary>
        private void AIOyna(Player oyuncu)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[AIOyna] Çağrıldı — oyuncu: {oyuncu}, aktif: {activePlayer}, " +
                $"aşama: {_oyunAsamasi}");

            if (_oyunAsamasi != OyunAsamasi.Oyun)
            {
                System.Diagnostics.Debug.WriteLine("[AIOyna] Aşama Oyun değil, çıkılıyor.");
                return;
            }
            if (InsanKontrolundeMi(oyuncu))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AIOyna] {oyuncu} insan kontrolünde, çıkılıyor.");
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

            // ═══════════════════════════════════════════════════════════════
            // DEBUG: Elin tüm kartlarını ve renk dağılımını yazdır
            // ═══════════════════════════════════════════════════════════════
            System.Diagnostics.Debug.WriteLine($"[DEBUG] {oyuncu}'ın eli ({el.Count} kart):");
            foreach (var kart in el)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"  - Suit='{kart.Suit}', Value={kart.Value}");
            }
            System.Diagnostics.Debug.WriteLine(
                $"[DEBUG] RenkUzunlugu: " +
                $"♠={ElDegerlendirici.RenkUzunlugu(el, "Maça")}, " +
                $"♥={ElDegerlendirici.RenkUzunlugu(el, "Kupa")}, " +
                $"♦={ElDegerlendirici.RenkUzunlugu(el, "Karo")}, " +
                $"♣={ElDegerlendirici.RenkUzunlugu(el, "Sinek")}");
            System.Diagnostics.Debug.WriteLine(
                $"[DEBUG] HP={ElDegerlendirici.HCP(el)}, " +
                $"Dengeli={ElDegerlendirici.DengeliEl(el)}");

            // Kart seçimi OyunZekasi'na devredildi (OyunZekasi.cs):
            //  - açılış atağı, savunma/deklaran ataklari, renk takibi, kesme, atma
            //  - yalnızca yasal kartlar (renk takibi zorunluluğu) döner
            var baglam = OyunBaglamiOlustur(oyuncu);
            string neden;
            Card secilen = OyunZekasi.KartSec(baglam, out neden);

            if (secilen == null)
            {
                secilen = OyunKurallari.LegalKartlar(el, _buEldeAtakRengi)
                                       .OrderBy(c => c.Value).First();
                neden = "Yedek: en düşük yasal kart";
            }

            System.Diagnostics.Debug.WriteLine($"[AIOyna] Karar: {neden}");

            System.Diagnostics.Debug.WriteLine(
                $"[AIOyna] {oyuncu} AI oynadı: {secilen.Suit} {secilen.Value}");

            KartOyna(oyuncu, secilen, secilen.Image);
        }
        /// <summary>
        /// AI'ın karar verirken kullanabileceği meşru bilgiyi toplar:
        /// kendi eli, deklaran tarafıysa diğer eli (deklaran↔dummy), savunmaysa
        /// açılmış dummy eli (açılış atağından sonra), masa, geçmiş eller, ihale.
        /// </summary>
        /// <summary>"4♠" → 4, "3NT X" → 3. Okunamazsa 0 (hedef mantığı devre dışı).</summary>
        private int KontratSeviyesi()
        {
            if (string.IsNullOrEmpty(_kontrat)) return 0;
            char ch = _kontrat.Trim()[0];
            return (ch >= '1' && ch <= '7') ? ch - '0' : 0;
        }

        private OyunBaglami OyunBaglamiOlustur(Player oyuncu)
        {
            Player dummy = IhaleDurumu.PartnerBul(_kontratDeklaran);
            bool atakYapildi = _oynananToplamEl > 0 || _buEldeOynananlar.Count > 0;
            bool deklaranTarafi = (oyuncu == _kontratDeklaran || oyuncu == dummy);

            var b = new OyunBaglami
            {
                Ben = oyuncu,
                Deklaran = _kontratDeklaran,
                Koz = KontratKozu(),
                Elim = new List<Card>(OyuncuEli(oyuncu)),
                Masa = new List<(Player Oyuncu, Card Kart)>(_buEldeOynananlar),
                Gecmis = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>(_oynananTumEller),
                ElNo = _oynananToplamEl,
                IhaleGecmisi = _sonIhaleGecmisi,
                IhaleBaslangici = _ihaleBaslangicOyuncusu,
                Seviye = KontratSeviyesi()
            };

            if (deklaranTarafi)
                b.OrtakEli = new List<Card>(OyuncuEli(IhaleDurumu.PartnerBul(oyuncu)));
            else if (atakYapildi)
                b.DummyEli = new List<Card>(OyuncuEli(dummy));

            return b;
        }

        /// <summary>
        /// Şu anki trick'te (henüz 4 kart tamamlanmadan) kim kazanıyor?
        /// Koz varsa ve oynanmışsa → en yüksek koz
        /// Yoksa → atak rengindeki en yüksek kart
        /// </summary>
        private Player SuAnkiTrickKazanani()
        {
            if (_buEldeOynananlar == null || _buEldeOynananlar.Count == 0)
                return activePlayer;

            return OyunKurallari.ElKazanani(_buEldeOynananlar, KontratKozu()).Oyuncu;
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

            return OyunKurallari.ElKazanani(_buEldeOynananlar, KontratKozu()).Oyuncu;
        }
        /// <summary>
        /// Belirtilen oyuncunun kartını masaya (playZonePanel içine) yerleştirir.
        /// O kart zaten masada varsa önce eskisini kaldırır.
        /// </summary>
        /// <summary>
        /// El tamamlandığında masadaki kazanan kartı sarı tonla vurgular
        /// (kart yüzü sarıya boyanır, diğer üç kart olduğu gibi kalır).
        /// Vurgu, masa temizlenirken kartla birlikte kaybolur.
        /// </summary>
        private void KazananKartiVurgula(Player kazanan)
        {
            if (!_masadakiKartlar.TryGetValue(kazanan, out var pb) || pb == null || pb.IsDisposed) return;
            if (pb.Image == null) return;

            try
            {
                // Orijinal (paylaşılan/önbellekli) resme dokunma: sarı tonlu kopya üret.
                var src = pb.Image;
                var tinted = new Bitmap(src.Width, src.Height);
                using (var g = Graphics.FromImage(tinted))
                using (var ia = new System.Drawing.Imaging.ImageAttributes())
                {
                    var cm = new System.Drawing.Imaging.ColorMatrix(new float[][]
                    {
                        new float[] {1f, 0f, 0f,    0f, 0f},
                        new float[] {0f, 1f, 0f,    0f, 0f},
                        new float[] {0f, 0f, 0.45f, 0f, 0f},   // maviyi kıs → sarı ton
                        new float[] {0f, 0f, 0f,    1f, 0f},
                        new float[] {0f, 0f, 0f,    0f, 1f}
                    });
                    ia.SetColorMatrix(cm);
                    g.DrawImage(src, new Rectangle(0, 0, tinted.Width, tinted.Height),
                                0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
                }
                pb.Image = tinted;   // masa PictureBox'ı temizlenirken Dispose edilir
                pb.BringToFront();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[KazananKartiVurgula] {ex.Message}");
            }
        }

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
            _seed++;  // 🔹 Yeni el → yeni seed
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

        // ═══════════════════════════════════════════════════════════════════
        // SEED PANELİ (oyun bilgi panelinin altında)
        // ═══════════════════════════════════════════════════════════════════

        private Panel seedPanel;

        //private Button btnSeedSil;
        //private Button btnSeedDagit;
        //private readonly Queue<int> _seedHafizasi = new Queue<int>(10);
        //private const int MaxSeedHafiza = 10;



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

            // NT — koz yok ("3NT", "3NT X" ...)
            if (_kontrat.IndexOf("NT", StringComparison.OrdinalIgnoreCase) >= 0) return null;

            // Kontralı kontratlarda ("4♠X", "4♠ XX") son karakter X olabilir;
            // bu yüzden son renk sembolü aranır.
            int idx = _kontrat.LastIndexOfAny(new[] { '♠', '♥', '♦', '♣' });
            if (idx < 0) return null;

            switch (_kontrat[idx])
            {
                case '♠': return "Maça";
                case '♥': return "Kupa";
                case '♦': return "Karo";
                case '♣': return "Sinek";
                default: return null;
            }
        }

        /// <summary>
        /// Elde renklerin dizilme sırası.
        ///   - İhale sırasında / NT kontratında: Maça, Kupa, Sinek, Karo (değişmez).
        ///   - Renk kontratında: koz 1. sırada (N/S'de en solda, E/W'de en üstte),
        ///     kalan renkler siyah-kırmızı dönüşümlü:
        ///       Maça  koz → Maça, Kupa, Sinek, Karo
        ///       Sinek koz → Sinek, Kupa, Maça, Karo
        ///       Kupa  koz → Kupa, Maça, Karo, Sinek
        ///       Karo  koz → Karo, Maça, Kupa, Sinek
        /// </summary>
        private string[] GetSuitSirasi()
        {
            string[] varsayilan = { "Maça", "Kupa", "Sinek", "Karo" };

            if (_oyunAsamasi != OyunAsamasi.Oyun) return varsayilan;

            switch (KontratKozu())
            {
                case "Maça": return new[] { "Maça", "Kupa", "Sinek", "Karo" };
                case "Sinek": return new[] { "Sinek", "Kupa", "Maça", "Karo" };
                case "Kupa": return new[] { "Kupa", "Maça", "Karo", "Sinek" };
                case "Karo": return new[] { "Karo", "Maça", "Kupa", "Sinek" };
                default: return varsayilan; // NT
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
            _oyunAsamasi = OyunAsamasi.Oyun;
            OyunGecmisiniSifirla();

            if (BtnClaim != null) BtnClaim.Enabled = true;   // yer oyununda Claim kullanılabilir

            // 🔹 Oyun başında "göster" butonlarını sıfırla: ihale sırasında E/W, N/S ya da Hepsi
            //    açılmış olsa bile oyun başlarken yalnızca Güney açık olsun (dummy ilk atıştan sonra açılır).
            BtnHepsi.Checked = false;
            BtnEW.Checked = false;
            BtnNS.Checked = false;
            BtnGuney.Checked = false;
            hepsiniGösterToolStripMenuItem.Checked = false;
            eWGösterToolStripMenuItem.Checked = false;
            nSGösterToolStripMenuItem.Checked = false;
            // İlk kart atağına kadar Geri Al / İleri Al kapalı
            if (BtnGeriAl != null) BtnGeriAl.Enabled = false;
            if (BtnileriAl != null) BtnileriAl.Enabled = false;
            _kontrat = kontrat;
            _kontratDeklaran = OyuncuyaCevir(deklaran);
            _declarer = _kontratDeklaran;
            _atakYapacakOyuncu = OyuncuyaCevir(solRakip);
            // NOT: _dealer bord dağıtıcısıdır; açılış atağını yapanla değiştirilmez
            // (aksi halde "D" harfi, Dealer bilgisi, Replay ve sonraki bord dağıtıcısı kayar).
            activePlayer = _atakYapacakOyuncu;

            OyunBilgiPaneliniGuncelle();

            this.SuspendLayout();
            try
            {
                // 🔹 1. ADIM: TÜM PictureBox'ları gizle ve görseli temizle
                for (int i = 0; i < 13; i++)
                {
                    if (kuzeyKartlar[i] != null) kuzeyKartlar[i].Visible = false;
                    if (guneyKartlar[i] != null) guneyKartlar[i].Visible = false;
                    if (batiKartlar[i] != null) batiKartlar[i].Visible = false;
                    if (doguKartlar[i] != null) doguKartlar[i].Visible = false;
                }

                // 🔹 2. ADIM: KartYerlesimiHazirla() ÇAĞIR (konumları ayarlar, kartları yerleştirir)
                KartYerlesimiHazirla();

                // 🔹 3. ADIM: Görseli arka yüz resmiyle değiştir
                Image backImg = GetBackImage();
                for (int i = 0; i < 13; i++)
                {
                    if (batiKartlar[i] != null)
                    {
                        batiKartlar[i].Image = backImg;
                        if (backImg == null) batiKartlar[i].BackColor = AnaSayfa.MasaRengi;
                    }
                    if (doguKartlar[i] != null)
                    {
                        doguKartlar[i].Image = backImg;
                        if (backImg == null) doguKartlar[i].BackColor = AnaSayfa.MasaRengi;
                    }
                }

                // 🔹 4. ADIM: Güney'in elini aç
                BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);

                // 🔹 5. ADIM: Dummy KAPALI başlar — açılış atağından sonra
                //             KartOyna içinde DummyyiAc() ile açılır (briç kuralı).
                _dummyAcildi = false;

                // 🔹 6. ADIM: Güney dışındaki tüm eller kapalı (Kuzey dahil)
                for (int i = 0; i < 13; i++)
                {
                    foreach (var kapali in new[] { kuzeyKartlar, batiKartlar, doguKartlar })
                    {
                        if (kapali[i] == null) continue;
                        kapali[i].Image = backImg;
                        if (backImg == null) kapali[i].BackColor = AnaSayfa.MasaRengi;
                    }
                }
            }
            finally
            {
                this.ResumeLayout(true);
            }

            // 🔹 7. ADIM: AI'ı tetikle
            if (!InsanKontrolundeMi(activePlayer))
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


        // ═══════════════════════════════════════════════════════════════════
        // PBN (Portable Bridge Notation) DIŞA AKTARMA
        // Bridge Baron Deal Library, Deep Finesse vb. programlar okuyabilir.
        // ═══════════════════════════════════════════════════════════════════

        private static string PbnTemiz(string t)
        {
            return (t ?? "").Replace("\"", "'").Replace("\r", " ").Replace("\n", " ");
        }

        private static char PbnKoltuk(Player p)
        {
            switch (p)
            {
                case Player.Kuzey: return 'N';
                case Player.Dogu: return 'E';
                case Player.Guney: return 'S';
                default: return 'W';
            }
        }

        private static Player Karsi(Player p)
        {
            switch (p)
            {
                case Player.Kuzey: return Player.Guney;
                case Player.Guney: return Player.Kuzey;
                case Player.Dogu: return Player.Bati;
                default: return Player.Dogu;
            }
        }

        private static char PbnRenk(string suit)
        {
            switch (suit)
            {
                case "Maça": return 'S';
                case "Kupa": return 'H';
                case "Karo": return 'D';
                default: return 'C';
            }
        }

        private static char PbnDeger(int v)
        {
            switch (v)
            {
                case 14: return 'A';
                case 13: return 'K';
                case 12: return 'Q';
                case 11: return 'J';
                case 10: return 'T';
                default: return (char)('0' + v);
            }
        }

        private static string PbnTeklif(string t)
        {
            if (string.IsNullOrEmpty(t)) return "Pass";
            if (t == "Pas" || t == "PAS") return "Pass";
            if (t == "Dbl") return "X";
            if (t == "RDbl") return "XX";
            // "1♦" → "1D", "3NT" → "3NT"
            if (t.EndsWith("♣")) return t.Substring(0, t.Length - 1) + "C";
            if (t.EndsWith("♦")) return t.Substring(0, t.Length - 1) + "D";
            if (t.EndsWith("♥")) return t.Substring(0, t.Length - 1) + "H";
            if (t.EndsWith("♠")) return t.Substring(0, t.Length - 1) + "S";
            return t;
        }

        /// <summary>
        /// Başlangıç (13 kartlık) eli: elde kalan + oynanmış kartlar.
        /// </summary>
        private List<Card> BaslangicEli(Player oyuncu)
        {
            var kartlar = new List<Card>(OyuncuEli(oyuncu) ?? new List<Card>());
            foreach (var k in _oynananTumEller.Where(t => t.Oyuncu == oyuncu).Select(t => t.Kart))
                if (!kartlar.Any(c => c.Suit == k.Suit && c.Value == k.Value)) kartlar.Add(k);
            return kartlar;
        }

        private string PbnEl(Player oyuncu)
        {
            var el = BaslangicEli(oyuncu);
            var parcalar = new List<string>();
            foreach (var renk in new[] { "Maça", "Kupa", "Karo", "Sinek" })
            {
                var dizi = el.Where(c => c.Suit == renk).OrderByDescending(c => c.Value)
                             .Select(c => PbnDeger(c.Value).ToString());
                parcalar.Add(string.Concat(dizi));
            }
            return string.Join(".", parcalar);
        }

        /// <summary>
        /// Bu bordu PBN metni olarak üretir.
        /// </summary>
        public string PbnUret(string ad, string aciklama, string yorum)
        {
            var sb = new System.Text.StringBuilder();
            string nlx = "\r\n";

            // Eller 180° takaslıysa dağıtıcı ve ihale koltukları da döndürülür
            Player dagitici = _ellerIhaleSonrasiTakasli ? Karsi(_dealer) : _dealer;

            // Zon (Vulnerable): 16'lı bord döngüsü
            int boardMod = ((_currentTour - 1) % 16) + 1;
            bool ns = (boardMod == 2 || boardMod == 5 || boardMod == 12 || boardMod == 15 ||
                       boardMod == 4 || boardMod == 7 || boardMod == 10 || boardMod == 16);
            bool ew = (boardMod == 3 || boardMod == 6 || boardMod == 9 || boardMod == 13 ||
                       boardMod == 4 || boardMod == 7 || boardMod == 10 || boardMod == 16);
            string zon = ns && ew ? "All" : ns ? "NS" : ew ? "EW" : "None";

            sb.Append("[Event \"" + PbnTemiz(string.IsNullOrWhiteSpace(ad) ? "BricKartOyunu" : ad) + "\"]" + nlx);
            sb.Append("[Site \"BricKartOyunu\"]" + nlx);
            sb.Append("[Date \"" + DateTime.Now.ToString("yyyy.MM.dd") + "\"]" + nlx);
            sb.Append("[Board \"" + _currentTour + "\"]" + nlx);
            sb.Append("[Dealer \"" + PbnKoltuk(dagitici) + "\"]" + nlx);
            sb.Append("[Vulnerable \"" + zon + "\"]" + nlx);

            // Deal: ilk elin koltuğu N, saat yönünde N E S W
            sb.Append("[Deal \"N:" + PbnEl(Player.Kuzey) + " " + PbnEl(Player.Dogu) + " " +
                      PbnEl(Player.Guney) + " " + PbnEl(Player.Bati) + "\"]" + nlx);

            // İhale
            var bids = _sonIhaleGecmisi ?? new List<string>();
            sb.Append("[Auction \"" + PbnKoltuk(dagitici) + "\"]" + nlx);
            for (int i = 0; i < bids.Count; i++)
            {
                sb.Append(PbnTeklif(bids[i]));
                sb.Append((i % 4 == 3 || i == bids.Count - 1) ? nlx : " ");
            }

            // Kontrat
            if (string.IsNullOrEmpty(_kontrat))
            {
                sb.Append("[Contract \"Pass\"]" + nlx);
            }
            else
            {
                string kontrat = PbnTeklif(_kontrat);
                int sonGercek = -1;
                for (int i = bids.Count - 1; i >= 0; i--)
                    if (bids[i] != "Pas" && bids[i] != "PAS" && bids[i] != "Dbl" && bids[i] != "RDbl") { sonGercek = i; break; }
                if (sonGercek >= 0)
                {
                    var sonrasi = bids.Skip(sonGercek + 1).ToList();
                    if (sonrasi.Contains("RDbl")) kontrat += "XX";
                    else if (sonrasi.Contains("Dbl")) kontrat += "X";
                }
                sb.Append("[Contract \"" + kontrat + "\"]" + nlx);
                sb.Append("[Declarer \"" + PbnKoltuk(_kontratDeklaran) + "\"]" + nlx);

                bool deklaranNS = _kontratDeklaran == Player.Kuzey || _kontratDeklaran == Player.Guney;
                sb.Append("[Result \"" + (deklaranNS ? _kazanilanElNS : _kazanilanElEW) + "\"]" + nlx);
            }

            // Oyun: açılış atağı yapan + her el için dört kart (oynanış sırasıyla)
            if (_oynananTumEller.Count > 0)
            {
                var gruplar = _oynananTumEller.GroupBy(t => t.ElNo).OrderBy(g => g.Key).ToList();
                sb.Append("[Play \"" + PbnKoltuk(gruplar[0].First().Oyuncu) + "\"]" + nlx);
                foreach (var grup in gruplar)
                {
                    var kartlar = grup.Select(t => "" + PbnRenk(t.Kart.Suit) + PbnDeger(t.Kart.Value)).ToList();
                    while (kartlar.Count < 4) kartlar.Add("-");
                    sb.Append(string.Join(" ", kartlar) + nlx);
                }
            }

            // Yorum bloğu (ad, açıklama, yorum)
            var yorumSatirlari = new List<string>();
            if (!string.IsNullOrWhiteSpace(aciklama)) yorumSatirlari.Add(aciklama.Trim());
            if (!string.IsNullOrWhiteSpace(yorum)) yorumSatirlari.Add(yorum.Trim());
            if (yorumSatirlari.Count > 0)
                sb.Append("{" + string.Join(" — ", yorumSatirlari).Replace("{", "(").Replace("}", ")") + "}" + nlx);

            return sb.ToString();
        }

        public void IhaleGecmisiniKaydet(List<string> bids, string baslangicOyuncusu)
        {
            _sonIhaleGecmisi = new List<string>(bids);
            _ihaleBaslangicOyuncusu = baslangicOyuncusu;
            IhaleGecmisiFormlariniGuncelle();
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

        private void IhaleYorumuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1) Form zaten açık mı?
            foreach (Form f in Application.OpenForms)
            {
                if (f is IhaleYorumuForm)
                {
                    f.BringToFront();
                    return;
                }
            }

            // 2) Yeni formu oluştur
            var form = new IhaleYorumuForm(new List<string>(), "Kuzey");

            // 3) Manuel konum ayarı
            form.StartPosition = FormStartPosition.Manual;

            // ⚠️ BURAYI KENDİ EKRANINA GÖRE AYARLA
            // Aşağıdaki değerler 2. resimdeki gibi hizalamak için tahmini değerlerdir.
            // Eğer form çok sağda/solda/yukarıda/aşağıda açılırsa bu sayıları değiştir.
            form.Location = new Point(755, 450);

            // 4) Formu göster
            form.Show(this);
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
            BtnOyundanExit.Enabled = false;
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
            BtnOyundanExit.Enabled = true;
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
            else if (_oyunAsamasi == OyunAsamasi.Oyun)
            {
                // İlk kart atağından sonra Geri Al aktif; geri alınan kart varsa İleri Al aktif.
                BtnGeriAl.Enabled = _oyunGecmisi.Count > 0 && _oynananToplamEl < 13;
                BtnileriAl.Enabled = _oyunIleriAlYigini.Count > 0;
            }
            else
            {
                BtnGeriAl.Enabled = false;
                BtnileriAl.Enabled = false;
            }
        }

        /// <summary>
        /// Oyun aşamasında son oynanan kartı geri alır: kart oynayanın eline döner,
        /// masa / sıra / skor / dummy / oynanan eller önceki duruma getirilir.
        /// Sıra AI'daysa oyun duraklar; İleri Al aynı kartı tekrar oynatır.
        /// </summary>
        // ====================================================================
        // OTOMATİK OYNATMA (BtnOtoOyna)
        // ====================================================================
        // Evet de Hayır da bu bord boyunca dört oyuncuyu (Güney ve dummy dahil) AI yapar;
        // her kart AI zamanlayıcısıyla oynanır.
        //   Evet  → eller arasında duraklama yok (el bitince kendiliğinden devam eder)
        //   Hayır → her el bitince durur, oyun alanına tıklanınca bir sonraki ele geçilir (normal oyun gibi)
        private bool _tumAI = false;
        private bool _tumAIDuraklamasiz = false;
        private System.Windows.Forms.Timer _tumAIElTimer;

        private void BtnOtoOyna_Click(object sender, EventArgs e)
        {
            // Yalnızca yer oyunu (kart oynama) aşamasında çalışır
            if (_oyunAsamasi != OyunAsamasi.Oyun || _tumAI) return;
            if (_oynananToplamEl >= 13) return;

            DialogResult sonuc;
            using (var onay = new OtoOynaOnayForm())
            {
                sonuc = onay.ShowDialog(this);
            }

            if (sonuc == DialogResult.Yes) OtomatikOyna(true);
            else if (sonuc == DialogResult.No) OtomatikOyna(false);
            // pencere X ile kapatıldıysa hiçbir şey yapma
        }

        private void OtomatikOyna(bool duraklamasiz)
        {
            _oyunIleriAlYigini.Clear();
            _tumAI = true;
            _tumAIDuraklamasiz = duraklamasiz;
            GeriAlIleriAlButonlariniGuncelle();

            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            if (_elBittiBekliyor)
            {
                if (duraklamasiz) TumAIYeniEle();   // önceki el bitmiş: sonraki ele geç
                // duraklamalıysa oyun alanına tıklanmasını bekler (normal oyun)
            }
            else AITetikle();                        // sıradaki oyuncu (insan olsa da) AI gibi oynar
        }

        /// <summary>Tam AI modunda el bitince kısa bekleyip (oyun alanına tıklanmadan) yeni ele geçer.</summary>
        private void TumAIYeniEleZamanla()
        {
            _tumAIElTimer?.Stop();
            _tumAIElTimer?.Dispose();
            _tumAIElTimer = new System.Windows.Forms.Timer { Interval = 800 };
            _tumAIElTimer.Tick += (s, e) =>
            {
                _tumAIElTimer?.Stop();
                _tumAIElTimer?.Dispose();
                _tumAIElTimer = null;
                if (!IsDisposed) TumAIYeniEle();
            };
            _tumAIElTimer.Start();
        }

        private void TumAIYeniEle()
        {
            if (!_tumAI || !_elBittiBekliyor || _oyunAsamasi != OyunAsamasi.Oyun) return;
            PlayZonePanel_Click(null, EventArgs.Empty);   // masayı temizler, kazananı başlatır, AI'ı tetikler
        }

        /// <summary>Otomatik oynatmayı bırakır (kullanıcı geri/ileri aldığında ya da yeni bordda).</summary>
        private void TumAIyiDurdur()
        {
            _tumAI = false;
            _tumAIDuraklamasiz = false;
            _tumAIElTimer?.Stop();
            _tumAIElTimer?.Dispose();
            _tumAIElTimer = null;
        }

        // ====================================================================
        // CLAIM (BtnClaim)
        // ====================================================================
        private static int KoltukNo(Player p)
        {
            switch (p)
            {
                case Player.Bati: return 0;
                case Player.Kuzey: return 1;
                case Player.Dogu: return 2;
                default: return 3;   // Güney
            }
        }

        private static int RenkNo(string renk)
        {
            switch (renk)
            {
                case "Maça": return 0;
                case "Kupa": return 1;
                case "Karo": return 2;
                case "Sinek": return 3;
                default: return -1;
            }
        }

        /// <summary>
        /// İhaleyi kazanan ortaklığın, bütün eller açıkmış gibi en iyi oyunla kalan ellerden kaçını
        /// alacağını hesaplar ve "n el daha alacağım. Kabul ediyor musun?" diye sorar.
        /// Hayır → oyun kaldığı yerden sürer. Evet → kalan eller dağıtılır, bütün kartlar
        /// başlangıçtaki gibi dizilir ve DealCompleteForm açılır.
        /// </summary>
        private void BtnClaim_Click(object sender, EventArgs e)
        {
            if (_oyunAsamasi != OyunAsamasi.Oyun || _tumAI || _oynananToplamEl >= 13) return;

            // Hesap el başında yapılır: masada yarım kalan el varsa önce o tamamlanmalı
            if (_buEldeOynananlar.Count > 0)
            {
                MessageBox.Show(
                    "Claim, bir el tamamlandıktan sonra (yeni ele başlamadan önce) yapılabilir.",
                    "Claim", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Bekleyen AI hamlesi varsa durdur (Hayır denirse aşağıda devam ettirilir)
            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            bool kabul = false;
            Cursor eskiImlec = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                var eller = new ulong[4];
                foreach (Player p in new[] { Player.Bati, Player.Kuzey, Player.Dogu, Player.Guney })
                {
                    foreach (var c in OyuncuEli(p))
                    {
                        int r = RenkNo(c.Suit);
                        if (r < 0) continue;
                        eller[KoltukNo(p)] |= 1UL << (r * 13 + (c.Value - 2));
                    }
                }

                int lider = KoltukNo(_elBittiBekliyor ? _sonElKazanani : activePlayer);
                string kozAdi = KontratKozu();
                int koz = kozAdi == null ? -1 : RenkNo(kozAdi);
                int taraf = KoltukNo(_kontratDeklaran) & 1;   // 1: Kuzey/Güney, 0: Batı/Doğu

                var cozucu = new ClaimCozucu(eller, lider, koz, taraf, 15000000);
                int n = cozucu.Coz();
                Cursor.Current = eskiImlec;

                if (cozucu.Asildi)
                {
                    MessageBox.Show(
                        "Bu aşamada kalan el sayısı güvenle hesaplanamadı.\nBirkaç el daha oynayıp tekrar deneyin.",
                        "Claim", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult sonuc;
                using (var onay = new ClaimOnayForm(n))
                {
                    sonuc = onay.ShowDialog(this);
                }
                if (sonuc != DialogResult.Yes) return;

                kabul = true;
                ClaimiUygula(n, taraf);
            }
            finally
            {
                Cursor.Current = eskiImlec;

                // Hayır / iptal / hesaplanamadı: oyun kaldığı yerden devam etsin
                if (!kabul && !IsDisposed && _oyunAsamasi == OyunAsamasi.Oyun && !_elBittiBekliyor &&
                    _buEldeOynananlar.Count == 0 && _oyunIleriAlYigini.Count == 0 &&
                    !InsanKontrolundeMi(activePlayer))
                {
                    AITetikle();
                }
            }
        }

        /// <summary>Kabul edilen claim'i uygular: elleri işler, bord sonu ekranını açar.</summary>
        private void ClaimiUygula(int n, int deklaranTarafi)
        {
            int kalan = 13 - _oynananToplamEl;
            int savunma = kalan - n;
            if (deklaranTarafi == 1) { _kazanilanElNS += n; _kazanilanElEW += savunma; }
            else { _kazanilanElEW += n; _kazanilanElNS += savunma; }

            _oynananToplamEl = 13;
            _elBittiBekliyor = false;
            _buEldeOynananlar.Clear();
            _buEldeAtakRengi = null;

            OyunBilgiPaneliniGuncelle();
            GeriAlIleriAlButonlariniGuncelle();

            // Dört eli başlangıçtaki gibi (13'er kart) açar, DealCompleteForm'u gösterir
            BordTamamlandiFormuAc();
        }

        private void OyunSonHamleyiGeriAl()
        {
            if (_oyunAsamasi != OyunAsamasi.Oyun) return;
            if (_oyunGecmisi.Count == 0 || _oynananToplamEl >= 13) return;
            TumAIyiDurdur();   // kullanıcı müdahale etti: normal moda dön

            // Bekleyen AI hamlesi varsa iptal et
            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            var h = _oyunGecmisi[_oyunGecmisi.Count - 1];
            _oyunGecmisi.RemoveAt(_oyunGecmisi.Count - 1);
            _oyunIleriAlYigini.Add(h);

            // Kartı oyuncunun eline geri koy
            var el = OyuncuEli(h.Oyuncu);
            if (el != null && !el.Contains(h.Kart)) el.Add(h.Kart);

            // Sayaçlar ve listeler
            _buEldeOynananlar.Clear();
            _buEldeOynananlar.AddRange(h.BuEldeOnce);
            _buEldeAtakRengi = h.AtakRengi;
            _elBittiBekliyor = h.ElBittiBekliyor;
            _sonElKazanani = h.SonElKazanani;
            _oynananToplamEl = h.ToplamEl;
            _kazanilanElNS = h.NS;
            _kazanilanElEW = h.EW;
            if (_oynananTumEller.Count > h.TumEllerSayisi)
                _oynananTumEller.RemoveRange(h.TumEllerSayisi, _oynananTumEller.Count - h.TumEllerSayisi);

            bool dummyKapanacak = _dummyAcildi && !h.DummyAcildi;
            _dummyAcildi = h.DummyAcildi;

            // Masa: o hamleden önceki kartlar
            MasadakiKartlariTemizle();
            foreach (var (o, k) in h.BuEldeOnce)
                KartiMasayaYerlestir(o, k, k.Image);

            // Eller (görsel)
            OyuncuEliniGuncelle(h.Oyuncu, h.Kart);
            if (dummyKapanacak)
                OyuncuEliniGuncelle(IhaleDurumu.PartnerBul(_kontratDeklaran), null);   // açılış atağı geri alındı → dummy tekrar kapalı

            activePlayer = h.Oyuncu;
            OyunBilgiPaneliniGuncelle();
            OynananElleriFormlariniGuncelle();
            GeriAlIleriAlButonlariniGuncelle();
        }

        /// <summary>
        /// Oyun aşamasında en son geri alınan kartı tekrar oynatır (aynı oyuncu, aynı kart).
        /// Geri alınan tüm kartlar ileri alındığında sıra AI'daysa oyun kaldığı yerden sürer.
        /// </summary>
        private void OyunSonHamleyiIleriAl()
        {
            if (_oyunAsamasi != OyunAsamasi.Oyun) return;
            if (_oyunIleriAlYigini.Count == 0) return;
            TumAIyiDurdur();

            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            var h = _oyunIleriAlYigini[_oyunIleriAlYigini.Count - 1];
            _oyunIleriAlYigini.RemoveAt(_oyunIleriAlYigini.Count - 1);

            int onceki = _oyunGecmisi.Count;
            _ileriAliniyor = true;
            try
            {
                KartOyna(h.Oyuncu, h.Kart, h.Kart.Image);
            }
            finally
            {
                _ileriAliniyor = false;
            }

            // Kart kabul edilmediyse (durum tutarsız) ileri alma yığını geçersizdir
            if (_oyunGecmisi.Count == onceki)
                _oyunIleriAlYigini.Clear();

            OynananElleriFormlariniGuncelle();

            // Geri alınanların hepsi ileri alındı ve sıra AI'da → oyun devam etsin
            if (_oyunIleriAlYigini.Count == 0 && !_elBittiBekliyor && _oynananToplamEl < 13 &&
                !InsanKontrolundeMi(activePlayer))
            {
                AITetikle();
            }

            GeriAlIleriAlButonlariniGuncelle();
        }

        /// <summary>Açık IhaleGecmisiForm varsa içeriğini güncel ihale geçmişiyle yeniler.</summary>
        private void IhaleGecmisiFormlariniGuncelle()
        {
            foreach (var f in Application.OpenForms.OfType<IhaleGecmisiForm>().ToList())
                f.Guncelle(_sonIhaleGecmisi, _ihaleBaslangicOyuncusu);
        }

        /// <summary>Açık OynananElleriGosterForm varsa içeriğini güncel eller listesiyle yeniler.</summary>
        private void OynananElleriFormlariniGuncelle()
        {
            foreach (var f in Application.OpenForms.OfType<OynananElleriGosterForm>().ToList())
                f.Guncelle(_oynananTumEller);
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


    /// <summary>
    /// "Otomatik oynatma" onay penceresi: başlık satırı, açıklama satırı ve Evet / Hayır butonları.
    /// Evet → DialogResult.Yes, Hayır → DialogResult.No, X ile kapatma → DialogResult.Cancel.
    /// </summary>
    internal sealed class OtoOynaOnayForm : Form
    {
        public Button BtnEvet { get; private set; }
        public Button BtnHayir { get; private set; }

        public OtoOynaOnayForm()
        {
            Text = "Otomatik oynatma";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(340, 130);

            var lbl1 = new Label
            {
                Text = "Otomatik oynatma",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 14),
                Size = new Size(320, 28)
            };
            var lbl2 = new Label
            {
                Text = "(Oynanan eller arasında duraklama yok)",
                Font = new Font("Segoe UI", 9.5F),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 46),
                Size = new Size(320, 24)
            };

            BtnEvet = new Button
            {
                Name = "BtnEvet",
                Text = "Evet",
                DialogResult = DialogResult.Yes,
                Location = new Point(70, 84),
                Size = new Size(90, 30)
            };
            BtnHayir = new Button
            {
                Name = "BtnHayir",
                Text = "Hayır",
                DialogResult = DialogResult.No,
                Location = new Point(180, 84),
                Size = new Size(90, 30)
            };

            Controls.AddRange(new Control[] { lbl1, lbl2, BtnEvet, BtnHayir });
            AcceptButton = BtnEvet;
        }
    }


    /// <summary>
    /// Claim onay penceresi: "n el daha alacağım. Kabul ediyor musun?" + Evet / Hayır.
    /// Evet → DialogResult.Yes, Hayır → DialogResult.No, X ile kapatma → DialogResult.Cancel.
    /// </summary>
    internal sealed class ClaimOnayForm : Form
    {
        public Button BtnEvet { get; private set; }
        public Button BtnHayir { get; private set; }

        public ClaimOnayForm(int n)
        {
            Text = "Claim";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(360, 120);

            var lbl = new Label
            {
                Text = n + " el daha alacağım. Kabul ediyor musun?",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 14),
                Size = new Size(340, 36)
            };

            BtnEvet = new Button
            {
                Name = "BtnEvet",
                Text = "Evet",
                DialogResult = DialogResult.Yes,
                Location = new Point(80, 72),
                Size = new Size(90, 30)
            };
            BtnHayir = new Button
            {
                Name = "BtnHayir",
                Text = "Hayır",
                DialogResult = DialogResult.No,
                Location = new Point(190, 72),
                Size = new Size(90, 30)
            };

            Controls.AddRange(new Control[] { lbl, BtnEvet, BtnHayir });
            AcceptButton = BtnEvet;
        }
    }

    /// <summary>
    /// Çift açık (double dummy) el çözücü: dört elin de açık olduğu varsayımıyla, iki tarafın
    /// da en iyi oynaması durumunda "maksimum taraf"ın kalan elleri kaç tane alacağını hesaplar.
    /// Koltuklar saat yönünde: 0=Batı, 1=Kuzey, 2=Doğu, 3=Güney (taraf = koltuk &amp; 1).
    /// Kart numarası = renk*13 + (değer-2); renk: 0=Maça, 1=Kupa, 2=Karo, 3=Sinek.
    /// </summary>
    internal sealed class ClaimCozucu
    {
        private struct Anahtar : IEquatable<Anahtar>
        {
            public ulong A, B, C, D;
            public int Lider;
            public bool Equals(Anahtar o) { return A == o.A && B == o.B && C == o.C && D == o.D && Lider == o.Lider; }
            public override bool Equals(object o) { return o is Anahtar && Equals((Anahtar)o); }
            public override int GetHashCode()
            {
                unchecked
                {
                    ulong h = A * 0x9E3779B97F4A7C15UL ^ (B + 0x7F4A7C15UL) * 0xC2B2AE3D27D4EB4FUL
                              ^ C * 0x165667B19E3779F9UL ^ D * 0x85EBCA77C2B2AE63UL ^ (ulong)Lider;
                    return (int)(h ^ (h >> 32));
                }
            }
        }

        private readonly ulong[] _el = new ulong[4];
        private readonly int[] _masa = new int[4];
        private int _masaSayisi, _lider, _ilkRenk;
        private readonly int _koz;          // -1 = kozsuz
        private readonly int _maxTaraf;     // 0: Batı/Doğu, 1: Kuzey/Güney
        private readonly Dictionary<Anahtar, int> _tt = new Dictionary<Anahtar, int>();
        private long _dugum;
        private readonly long _limit;

        /// <summary>Düğüm sınırı aşıldıysa sonuç güvenilmezdir.</summary>
        public bool Asildi { get; private set; }

        public ClaimCozucu(ulong[] eller, int lider, int koz, int maxTaraf, long dugumSiniri)
        {
            for (int i = 0; i < 4; i++) _el[i] = eller[i];
            _lider = lider; _koz = koz; _maxTaraf = maxTaraf; _limit = dugumSiniri;
        }

        private static int Say(ulong x)
        {
            int n = 0;
            while (x != 0) { x &= x - 1; n++; }
            return n;
        }

        /// <summary>El başında (masa boş) maksimum tarafın alacağı el sayısı.</summary>
        public int Coz()
        {
            int kalan = Say(_el[_lider]);
            return Ara(-1, kalan + 1);
        }

        private bool Yener(int c, int w)
        {
            int cs = c / 13, ws = w / 13;
            if (cs == ws) return c > w;
            return cs == _koz;
        }

        private int Ara(int alfa, int beta)
        {
            if (++_dugum > _limit) { Asildi = true; return 0; }

            int koltuk = (_lider + _masaSayisi) & 3;
            bool sinir = _masaSayisi == 0;
            Anahtar k = default(Anahtar);
            int lo = 0, hi = 0;
            int alfa0 = alfa, beta0 = beta;

            if (sinir)
            {
                if ((_el[0] | _el[1] | _el[2] | _el[3]) == 0) return 0;
                k = new Anahtar { A = _el[0], B = _el[1], C = _el[2], D = _el[3], Lider = _lider };
                int kalan = Say(_el[koltuk]);
                lo = 0; hi = kalan;
                int v;
                if (_tt.TryGetValue(k, out v))
                {
                    lo = v & 0xFF; hi = (v >> 8) & 0xFF;
                    if (lo >= beta) return lo;
                    if (hi <= alfa) return hi;
                    if (lo == hi) return lo;
                    if (lo > alfa) alfa = lo;
                    if (hi < beta) beta = hi;
                }
                alfa0 = alfa; beta0 = beta;
            }

            // Yasal kartlar
            ulong el = _el[koltuk];
            ulong yasal = el;
            if (_masaSayisi > 0)
            {
                ulong renkMaske = 0x1FFFUL << (_ilkRenk * 13);
                if ((el & renkMaske) != 0) yasal = el & renkMaske;
            }

            // Masadaki kartlar da "arada kart var mı" kontrolüne katılır
            ulong hepsi = _el[0] | _el[1] | _el[2] | _el[3];
            for (int i = 0; i < _masaSayisi; i++) hepsi |= 1UL << _masa[i];

            // Denk kartları ele: aralarında başka kart olmayan bitişik kartlardan yalnızca en yükseği denenir
            int[] hamle = new int[13];
            int hs = 0;
            for (int renk = 0; renk < 4; renk++)
            {
                int m = (int)((yasal >> (renk * 13)) & 0x1FFF);
                if (m == 0) continue;
                int hm = (int)((hepsi >> (renk * 13)) & 0x1FFF);
                int onceki = -1;
                for (int b = 12; b >= 0; b--)
                {
                    if ((m & (1 << b)) == 0) continue;
                    if (onceki >= 0)
                    {
                        int arasi = ((1 << onceki) - 1) & ~((1 << (b + 1)) - 1);   // b+1 .. onceki-1
                        if ((hm & arasi) == 0) { onceki = b; continue; }          // öncekiyle denk
                    }
                    hamle[hs++] = renk * 13 + b;
                    onceki = b;
                }
            }

            // Sıralama: ön tarafta yüksek (açarken), takip ederken kazananın en düşüğü sonra düşükler
            if (_masaSayisi == 0)
            {
                Array.Sort(hamle, 0, hs, DescComparer.Ornek);
            }
            else
            {
                int kazanan = _masa[0];
                for (int i = 1; i < _masaSayisi; i++) if (Yener(_masa[i], kazanan)) kazanan = _masa[i];
                int[] skor = new int[hs];
                for (int i = 0; i < hs; i++) skor[i] = (Yener(hamle[i], kazanan) ? 0 : 100) + (hamle[i] % 13);
                Array.Sort(skor, hamle, 0, hs);
                // skor dizisi tam uzunlukta; Array.Sort(keys, items, index, length) hs kadarını sıralar
            }

            bool max = (koltuk & 1) == _maxTaraf;
            int best = max ? int.MinValue : int.MaxValue;
            int a = alfa, bb = beta;

            for (int hi2 = 0; hi2 < hs; hi2++)
            {
                int c = hamle[hi2];
                ulong bit = 1UL << c;
                _el[koltuk] ^= bit;
                int eskiSayi = _masaSayisi, eskiIlk = _ilkRenk, eskiLider = _lider;
                int m0 = _masa[0], m1 = _masa[1], m2 = _masa[2], m3 = _masa[3];
                _masa[_masaSayisi++] = c;
                if (_masaSayisi == 1) _ilkRenk = c / 13;

                int val;
                if (_masaSayisi == 4)
                {
                    int bi = 0;
                    for (int i = 1; i < 4; i++) if (Yener(_masa[i], _masa[bi])) bi = i;
                    int w = (_lider + bi) & 3;
                    int puan = ((w & 1) == _maxTaraf) ? 1 : 0;
                    _lider = w; _masaSayisi = 0;
                    val = puan + Ara(a - puan, bb - puan);
                }
                else
                {
                    val = Ara(a, bb);
                }

                _lider = eskiLider; _masaSayisi = eskiSayi; _ilkRenk = eskiIlk;
                _masa[0] = m0; _masa[1] = m1; _masa[2] = m2; _masa[3] = m3;
                _el[koltuk] ^= bit;

                if (Asildi) return 0;

                if (max) { if (val > best) best = val; if (best > a) a = best; }
                else { if (val < best) best = val; if (best < bb) bb = best; }
                if (a >= bb) break;
            }

            if (sinir)
            {
                int nlo = lo, nhi = hi;
                if (best <= alfa0) nhi = Math.Min(nhi, best);
                else if (best >= beta0) nlo = Math.Max(nlo, best);
                else { nlo = best; nhi = best; }
                if (_tt.Count > 4000000) _tt.Clear();
                _tt[k] = nlo | (nhi << 8);
            }
            return best;
        }

        private sealed class DescComparer : System.Collections.Generic.IComparer<int>
        {
            public static readonly DescComparer Ornek = new DescComparer();
            public int Compare(int x, int y) { return (y % 13).CompareTo(x % 13); }
        }
    }
}
