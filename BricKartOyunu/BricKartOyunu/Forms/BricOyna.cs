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
        // Ekran ölçeği: 1.0 = tasarım ölçüsü (≈1080p). Küçük ekranlarda (ör. 1366x768) tüm düzen orantılı küçültülür.
        private float _olcek = 1f;
        private int Ol(int v) { return _olcek >= 0.999f ? v : (int)Math.Round(v * _olcek); }
        private int _kartGenislik { get { return Ol(90); } }
        private int _kartYukseklik { get { return Ol(135); } }

        // Ölçeklenmeyen sabit paylar: üstte menü+araç çubuğu (40), altta oyuncu rozeti + durum çubuğu (50)
        private const int UstSabitPay = 40;
        private const int AltSabitPay = 50;
        // Ölçeklenen bölüm (172 + 4 kart + 3×2 + 10 + 1 kart) tasarım yüksekliği
        private const int OlceklenenTasarimYuksekligi = 172 + (4 * 135 + 3 * 2) + 10 + 135;

        /// <summary>Mevcut pencere yüksekliğine göre düzen ölçeğini hesaplar; değiştiyse kartları/masayı yeniden boyutlar.</summary>
        private void OlcegiGuncelle()
        {
            float yeni = (float)(this.ClientSize.Height - UstSabitPay - AltSabitPay) / OlceklenenTasarimYuksekligi;
            if (yeni > 1f) yeni = 1f;
            if (yeni < 0.5f) yeni = 0.5f;
            if (yeni >= 0.98f) yeni = 1f;   // küçük farkları yok say: 1080p'de görünüm değişmesin
            if (Math.Abs(yeni - _olcek) < 0.005f) return;
            _olcek = yeni;

            foreach (var dizi in new[] { batiKartlar, kuzeyKartlar, doguKartlar, guneyKartlar })
                foreach (var pb in dizi)
                    if (pb != null) pb.Size = new Size(_kartGenislik, _kartYukseklik);

            if (playZonePanel != null) playZonePanel.Size = new Size(Ol(500), Ol(500));
            foreach (var kv in _masadakiKartlar)
            {
                if (kv.Value == null) continue;
                kv.Value.Size = new Size(MasaKartGenislik, MasaKartYukseklik);
                kv.Value.Location = OyuncuIcinMasaPozisyonu(kv.Key);
            }
        }
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
                DurumCubuguGuncelle();
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
        private int MasaKartGenislik { get { return Ol(70); } }
        private int MasaKartYukseklik { get { return Ol(105); } }

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

        // ── Konvansiyon pratiği: bu liste doluysa dağıtımlar sırayla buradan gelir ──
        public List<PratikDagitim> PratikDagitimlari { get; set; }
        public bool PratikDuyur { get; set; } = true;
        private int _pratikIndeks = 0;

        private bool PratikModu
        {
            get { return PratikDagitimlari != null && _pratikIndeks >= 0 && _pratikIndeks < PratikDagitimlari.Count; }
        }

        /// <summary>Pratik listesinde sıradaki dağıtıma geçer; liste bitince pratik modu kapanır.</summary>
        private void PratikIlerlet()
        {
            if (PratikDagitimlari == null) return;
            _pratikIndeks++;
            if (_pratikIndeks >= PratikDagitimlari.Count)
            {
                PratikDagitimlari = null;
                MessageBox.Show(this, "Pratik tamamlandı. Bundan sonra normal (rastgele) dağıtımla devam edilir.",
                    "Konvansiyon Pratiği", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _seed = PratikDagitimlari[_pratikIndeks].Seed;
        }

        private void PratikDuyurusu()
        {
            if (!PratikModu || !PratikDuyur) return;
            var p = PratikDagitimlari[_pratikIndeks];
            MessageBox.Show(this,
                "Pratik " + (_pratikIndeks + 1) + "/" + PratikDagitimlari.Count + "\n\n" +
                "Konvansiyon: " + p.Konvansiyon + "\n" +
                "Dağıtım no: " + p.Seed + "\n" +
                "Dağıtıcı: " + PlayerIsmiTurkce(_dealer),
                "Konvansiyon Pratiği", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Son kullanılan seed (UI'da göstermek için).
        /// </summary>
        public int Seed => _seed;
        public string DealerAdi => PlayerIsmiTurkce(_dealer);
        public string ZonAdi => ZonDurumuMetni();

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

        private Panel _acilisKapagi;

        /// <summary>Form tamamen çizilip görünür olduğunda (açılış geçişi bittiğinde) tetiklenir.</summary>
        public event EventHandler Hazir;
        private Action _acilisSonrasi;

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
                EkranYardimci.TumKontrollerdeCiftTampon(this);

                // Açılışta beyaz kare görünmesin: masa rengiyle başla
                this.BackColor = AnaSayfa.MasaRengi;

                // Görev çubuğunda ayrı simge göstermesin (AnaSayfa'nın simgesi yeterli)
                this.ShowInTaskbar = false;

                // Oyun penceresi kapanınca (ana menüye dönüş, X, maç sonu) maç durumu sıfırlanır; bord sayısı 4'e döner
                this.FormClosed += (s0, e0) => MacDurumu.Bitir();

                // Başlık: Briç Maçı sırasında (maç bitene kadar) "Briç Maçı", normal oyunda "Briç Kart Oyunu"
                this.Text = MacDurumu.Aktif ? "Briç Maçı" : "Briç Kart Oyunu";

                // Tam çizilene kadar görünmez; böylece AnaSayfa ile karışan ara kare olmaz.
                this.Opacity = 0;

                // Açılışta yarım çizilmiş ara kareler (beyaz zemin, kontrol çerçeveleri) görünmesin:
                // form yüklenirken her şeyi örten masa renginde bir kapak durur, yükleme bitince kalkar.
                _acilisKapagi = new Panel
                {
                    BackColor = AnaSayfa.MasaRengi,
                    Location = Point.Empty,
                    Size = this.ClientSize,
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };
                this.Controls.Add(_acilisKapagi);
                _acilisKapagi.BringToFront();
                this.Shown += (s, ev) =>
                {
                    if (_acilisKapagi != null) _acilisKapagi.BringToFront();
                    var t = new System.Windows.Forms.Timer { Interval = 250 };
                    t.Tick += (s2, e2) =>
                    {
                        t.Stop(); t.Dispose();
                        if (this.IsDisposed || _acilisKapagi == null) return;
                        this.Controls.Remove(_acilisKapagi);
                        _acilisKapagi.Dispose();
                        _acilisKapagi = null;
                        this.Invalidate(true);
                        this.Update();
                        this.Opacity = 1;
                        Hazir?.Invoke(this, EventArgs.Empty);
                        var sonra = _acilisSonrasi; _acilisSonrasi = null;
                        if (sonra != null) sonra();
                    };
                    t.Start();
                };
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
            this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
            try
            {
                KartYerlesimiHazirla();
            }
            finally
            {
                this.ResumeLayout(true); EkranYardimci.CizimiAc(this);
            }
        }

        /// <summary>Araç çubuğu: kart arka yüzünü iki resimden biriyle değiştirir; ekrandaki kapalı kartlar hemen güncellenir.</summary>
        private void BrnKartArka_Click(object sender, EventArgs e)
        {
            string mevcut = Path.GetFileName(_currentBackImagePath ?? AppState.ArkaYuzYolu());
            using (var f = new KartArkaYuzSecForm(mevcut))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;

                Image eski = GetBackImage();
                AppState.ArkaYuzDosya = f.Secilen;
                AppState.ArkaYuzuKaydet();
                CurrentBackImage = AppState.ArkaYuzYolu();
                Image yeni = GetBackImage();
                if (yeni == null) return;

                // Şu an arka yüz gösteren tüm kartları yeni resimle değiştir (açık kartlara dokunma)
                foreach (var dizi in new[] { batiKartlar, kuzeyKartlar, doguKartlar, guneyKartlar })
                    foreach (var pb in dizi)
                        if (pb != null && eski != null && ReferenceEquals(pb.Image, eski)) pb.Image = yeni;
                this.Invalidate(true);
            }
        }

        private void BricOyna_Load(object sender, EventArgs e)
        {
            KoltukHaritasi.Uygula(AnaSayfa.OyunPlani == null ? null : AnaSayfa.OyunPlani.InsanMi);
            if (this.DesignMode) return;

            // İpucu: oynanması önerilen kartı sarıyla işaretler (araç çubuğu düğmesi ve menü)
            BtnIpucu.Click += BtnIpucu_Click;
            BrnKartArka.Click += BrnKartArka_Click;
            BtnMasaRenk.Click += MasaRengiToolStripMenuItem_Click;
            ipucuToolStripMenuItem.Click += BtnIpucu_Click;

            // 🔹 Araç çubuğu AnaSayfa ile aynı boyutta: 16x16 simge, otomatik yükseklik
            toolStrip1.ImageScalingSize = new Size(16, 16);
            toolStrip1.AutoSize = true;

            ////Seed numarası
            //_seed = 1196129724;  // TEST: aynı seed

            // 🔹 Toolbar butonlarının basılı/basılmamış resimlerini ayarla
            ToolbarButonResimleriniAyarla(BtnHepsi, "allHandsalt.png", "allHandsUst.png");
            ToolbarButonResimleriniAyarla(BtnEW, "EWalt.png", "EWUst.png");
            ToolbarButonResimleriniAyarla(BtnNS, "NSalt.png", "NSUst.png");
            ToolbarButonResimleriniAyarla(BtnGuney, "Southalt.png", "SouthUst.png");

            _oyunAsamasi = OyunAsamasi.Baslangic;   // ← EKLE

            this.BackColor = AnaSayfa.MasaRengi;
            DurumCubuguRenginiSabitle();
            PlayZoneRenginiGuncelle();

            // Bord numarası ve dağıtıcı Oyun Planı'ndan gelir
            if (PratikModu)
            {
                _seed = PratikDagitimlari[_pratikIndeks].Seed;
                _currentTour = _pratikIndeks + 1;
                DagiticiyiAyarla();
            }
            else
            {
                var plan = AnaSayfa.OyunPlani ?? new OyunPlaniSecenekleri();
                _currentTour = (MacDurumu.Aktif || plan.YeniBordNumaralari) ? 1 : MacDurumu.SonrakiBordNo(Math.Max(0, plan.SonBordNo));   // bord numaraları 16'dan sonra 1'e döner
                DagiticiyiAyarla();
            }
            activePlayer = _dealer;
            _declarer = _dealer;   // İhale henüz yok; geçici

            // Aktif oyuncu Güney ise "aktif oyuncu" butonu pasif olmalı
            BtnGuney.Checked = false;
            BtnGuney.Enabled = (activePlayer != _humanPlayer);

            //Butonlar inaktif yapılıyor
            ButonlariInaktifYap();   // <-- 7 buton burada inaktif yapılıyor

            // 🔹 Seed geçmişini önce yükle (KartlariDagit üzerine yazmasın!)
            SeedHafizasiniYukle();

            // Maçta "Seçilen" tohum: ilk bord tohumdan dağıtılır
            if (MacDurumu.Aktif && !MacDurumu.SeedRastgele) _seed = MacDurumu.BordSeed();


            EkranYardimci.CizimiDondur(this);
            try
            {
            KartlariDagit(AnaSayfa.SeciliKartSeti);
            UpdateInfoBoard(_dealer);
            UpdatePlayerBadges();

            // 🔹 YENİ: Oyun bilgi panelini güncelle (Dealer, Zon görünsün)
            OyunBilgiPaneliniGuncelle();

            // 🔹 TEST: İhale motoru çalışıyor mu?
            if (IhaleMotoruTestiAcik) TestIhaleMotoru();   // her açılışta yüzlerce yapay el çalıştırıyordu (beyaz ekran gecikmesi)
            }
            finally { EkranYardimci.CizimiAc(this); }

            if (_acilisKapagi != null) _acilisKapagi.BringToFront();   // yükleme sırasında öne alınan kontrollerin üstünde kalsın

            // Duyuru ve DekBasForm, form görünür olduktan sonra açılır (geçişte tek başlarına belirmesinler)
            _acilisSonrasi = () =>
            {
                // Kullanıcı girişi: form görünür olduktan sonra eller elle girilir (İptal: oyun açılmaz)
                if (ManuelGirisModu())
                {
                    if (!ManuelGirisAl()) { Close(); return; }
                    ManuelYenidenDagit();
                }
                PratikDuyurusu(); YeniDekBasFormAc();
            };
        }

        /// <summary>
        /// Test amaçlı — ihale motorunu çalıştırır ve Debug'a yazar.
        /// </summary>
        /// <summary>İhale motoru yapay el testi (geliştirme için). Açılışı yavaşlattığı için varsayılan kapalı.</summary>
        private static bool IhaleMotoruTestiAcik = false;

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
            return _kuzeyY + Ol(172);
        }

        private void KartYerlesimiHazirla()
        {
            OlcegiGuncelle();
            int maxEWToplamYukseklik = (4 * _kartYukseklik) + (3 * 2);
            int toplamBlokYukseklik = _kartYukseklik + maxEWToplamYukseklik + _kartYukseklik;

            int ustSınır = 40;
            int dikeyKaydırma = (this.ClientSize.Height - toplamBlokYukseklik) / 2;
            // Küçük ekranda alt kenarı (rozet + durum çubuğu) taşırmasın
            int altaKadar = Ol(172) + maxEWToplamYukseklik + Ol(10) + _kartYukseklik;
            int altSinir = this.ClientSize.Height - AltSabitPay - altaKadar;
            if (dikeyKaydırma > altSinir) dikeyKaydırma = altSinir;
            if (dikeyKaydırma < ustSınır) dikeyKaydırma = ustSınır;

            _kuzeyY = dikeyKaydırma;
            int ewBaslangicY = _kuzeyY + Ol(172);
            _guneyY = ewBaslangicY + maxEWToplamYukseklik + Ol(10);

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

            // Doğu eli: konumu elde kalan kart sayısına göre DEĞİŞMEMELİ (Hepsi/NS/EW geçişlerinde
            // ve oyun sırasında Doğu sütunu kaymasın). Kuzey elinin en geniş hali (13 kart, renk
            // grupları arası boşluklu) esas alınır; oyun alanının sağ kenarının da dışında kalır.
            int kuzeyEnGenisBitisX = kuzeyX + (9 * Ol(18)) + (3 * (_kartGenislik + Ol(10))) + _kartGenislik;
            int doguX = kuzeyEnGenisBitisX + yanBosluk;
            int oyunAlaniSagKenar = ((this.ClientSize.Width - Ol(500)) / 6) + 30 + Ol(500);
            if (doguX < oyunAlaniSagKenar + 40) doguX = oyunAlaniSagKenar + 40;

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

    }
}
