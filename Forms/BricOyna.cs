using BricKartOyunu.Class;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;


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
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, c, new object[] { true });
        }

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

            YeniDekBasFormAc();
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
                    BilgiPanelUstBosluk
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
            int panelY = 70;

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
                    Size = new Size(500, 500)
                };
                EnableDoubleBuffer(playZonePanel);
                this.Controls.Add(playZonePanel);
            }

            playZonePanel.Left = ((this.ClientSize.Width - playZonePanel.Width) / 6) + 30;
            playZonePanel.Top = (_kuzeyY + _guneyY) / 2 - (playZonePanel.Height / 3);
            playZonePanel.BringToFront();
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
            foreach (Form f in Application.OpenForms.Cast<Form>()
                         .Where(f => f != this && !(f is AnaSayfa))
                         .ToArray())
            {
                if (f is DeklarasyonForm dek)
                    dek.IzinliKapat();
                else if (f is DekBasForm dekBas)
                    dekBas.ProgramatikKapat();   // 🔹 Programatik kapat — tüm formlar kapanmasın
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
            // Timer'ı durdur (yoksa kapanışta hata verebilir)
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

            // 6. Görsel olarak kartı elden kaldır
            pb.Visible = false;
            pb.Tag = null;
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

            // Bu elde oynananlar listesine ekle
            _buEldeOynananlar.Add((oyuncu, kart));

            System.Diagnostics.Debug.WriteLine(
                $"[KartOyna] {oyuncu} oynadı: {kart.Suit} {kart.Value} " +
                $"(bu elde {_buEldeOynananlar.Count}/4)");

            
            // 4 kart tamamlandı mı?
            if (_buEldeOynananlar.Count >= 4)
            {
                // 🔹 El kazananını belirle
                Player kazanan = ElKazananiniBelirle();

                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] El tamamlandı — kazanan: {kazanan}");

                // 🔹 Skor artışı (NS mi, EW mi?)
                if (kazanan == Player.Kuzey || kazanan == Player.Guney)
                {
                    _kazanilanElNS++;
                }
                else
                {
                    _kazanilanElEW++;
                }

                // 🔹 Oyun bilgi panelini güncelle
                OyunBilgiPaneliniGuncelle();

                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] Skor → NS: {_kazanilanElNS}, EW: {_kazanilanElEW}");

                // TODO (Adım 3d): Kartları temizle, 2 saniye bekle, yeni el başlat
                // Şimdilik: sadece aktif oyuncuyu kazanana çevir

                _buEldeAtakRengi = null;
                _buEldeOynananlar.Clear();
                activePlayer = kazanan;   // Kazanan yeni eli başlatır
                return;
            }

            // Sırayı bir sonraki oyuncuya geçir
            // Sırayı bir sonraki oyuncuya geçir
            activePlayer = SonrakiOyuncu(activePlayer);

            // Sıra AI'da mı? Otomatik oynat
            if (activePlayer != _humanPlayer)
            {
                AITetikle();
            }
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
       
    }
}