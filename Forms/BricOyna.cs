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

        private Player activePlayer = Player.Guney;
        private Player _declarer = Player.Guney;

        private Player _dealer = Player.Guney;   // Yeni elin dağıtanı (ihaleyi başlatan)

        // DeklarasyonForm ilk açıldığında hangi oyuncunun "eli" (deal eden / sırası gelen
        // oyuncu) olduğunu gösterebilmesi için dışarıya salt-okunur erişim sağlıyoruz.
        public Player ActivePlayer => activePlayer;

        private readonly Player _humanPlayer = Player.Guney;

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

            this.BackColor = AnaSayfa.MasaRengi;

            // 🔹 İlk açılışta form üzerindeki tüm MenuStrip bileşenlerini pasif yapıyoruz
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is MenuStrip menuStrip)
                {
                    menuStrip.Enabled = false;
                }
            }

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

            YeniDekBasFormAc();
        }

        // 🔹 DekBasForm'u oluşturup playZonePanel'in tam ortasında gösterir.
        // Hem ilk açılışta (BricOyna_Load) hem de "Sonraki el" ile yeni bir
        // dağılıma geçildiğinde aynı mantık kullanıldığı için ayrı bir metoda alındı.
        private void YeniDekBasFormAc()
        {
            try
            {
                if (playZonePanel != null)
                {
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
            }
            catch
            {
                // Form açılırken oluşabilecek hataları burada sessizce geç
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
                    guneyKartlar[i] = new PictureBox { Width = _kartGenislik, Height = _kartYukseklik, SizeMode = PictureBoxSizeMode.StretchImage, BorderStyle = BorderStyle.FixedSingle };
                    this.Controls.Add(guneyKartlar[i]);
                }
            }
            BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, guneyX, _guneyY, false, false);

            SetupInfoBoard();
            SetupPlayerBadges();
            SetupPlayZone();

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
            if (lblN_Dir != null) this.Controls.Remove(lblN_Dir);
            if (lblN_Name != null) this.Controls.Remove(lblN_Name);
            if (lblS_Dir != null) this.Controls.Remove(lblS_Dir);
            if (lblS_Name != null) this.Controls.Remove(lblS_Name);
            if (lblW_Dir != null) this.Controls.Remove(lblW_Dir);
            if (lblW_Name != null) this.Controls.Remove(lblW_Name);
            if (lblE_Dir != null) this.Controls.Remove(lblE_Dir);
            if (lblE_Name != null) this.Controls.Remove(lblE_Name);

            int bosluk = 1;

            int kuzeyY_Alt = _kuzeyY + _kartYukseklik + bosluk;
            int kuzeyX_Baslangic = 250;
            CreateBadge(out lblN_Dir, out lblN_Name, "N", "Oyuncu 3", kuzeyX_Baslangic, kuzeyY_Alt);

            int guneyY_Alt = _guneyY + _kartYukseklik + bosluk;
            int guneyX_Baslangic = 250;
            CreateBadge(out lblS_Dir, out lblS_Name, "G", "Siz", guneyX_Baslangic, guneyY_Alt);

            int ewStartingY = GetCenteredEWStartingY();
            int suitBoslugu = 2;
            int maxEWToplamYukseklik = (4 * _kartYukseklik) + (3 * suitBoslugu);
            int gercekEwY_Alt = ewStartingY + maxEWToplamYukseklik + bosluk;

            int batiX = 40;
            CreateBadge(out lblW_Dir, out lblW_Name, "B", "Oyuncu 2", batiX, gercekEwY_Alt);

            // 💡 DÜZELTME: Varsayılan yedek koordinat değeri de 30 piksel artırılarak 930 yapıldı
            int doguX = doguKartlar[0] != null ? doguKartlar[0].Left : 930;
            CreateBadge(out lblE_Dir, out lblE_Name, "E", "Oyuncu 4", doguX, gercekEwY_Alt);

            UpdatePlayerBadges();
        }

        private void CreateBadge(out Label dirLbl, out Label nameLbl, string dir, string name, int x, int y)
        {
            dirLbl = new Label { Text = dir, Size = new Size(20, 20), Location = new Point(x, y), BackColor = Color.Gray, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Arial", 8, FontStyle.Bold) };
            nameLbl = new Label { Text = name, Size = new Size(80, 20), Location = new Point(x + 22, y), BackColor = Color.Gray, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Arial", 9) };
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

                Random rnd = new Random();
                kartlar = kartlar.OrderBy(x => rnd.Next()).ToList();

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

        private void ÇıkışToolStripMenuItem_Click(object sender, EventArgs e) { Environment.Exit(0); }
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
            // Onay mekanizması olmadan doğrudan açık alt formları kapatır ve yeni eli başlatır
            foreach (Form f in Application.OpenForms.Cast<Form>()
                         .Where(f => f != this && !(f is AnaSayfa))
                         .ToArray())
            {
                if (f is DeklarasyonForm dek)
                    dek.IzinliKapat();
                else
                    f.Close();
            }

            // 4 Pas sonrasındaki tüm adımları kapsayan döngüsel süreci çalıştır
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
            if (e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.WindowsShutDown) return;
            // Güvenlik ağı: form BtnGeri dışında bir yoldan (ör. pencere X butonu) kapatılırsa
            // AnaSayfa ekranda görünür olduğundan emin ol.
            if (Application.OpenForms["AnaSayfa"] is AnaSayfa anaSayfa && !anaSayfa.Visible)
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
        /// DeklarasyonForm'dan gelen aktif oyuncu bilgisine göre BricOyna üzerindeki 
        /// ilgili oyuncu etiketini SARI renge boyar, diğerlerini normale çeker.
        /// </summary>
        public void AktifOyuncuEtiketiniGuncelle(string aktifOyuncu)
        {
            activePlayer = OyuncuyaCevir(aktifOyuncu);

            // Aktif oyuncu butonunun durumunu da güncelle
            BtnGuney.Enabled = (activePlayer != _humanPlayer);

            UpdatePlayerBadges();
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
            // Adım 1: Tur sayısını artır (Bord numarası)
            _currentTour++;

            // Adım 2: DAĞITAN oyuncuyu saat yönünde ilerlet
            _dealer = SonrakiOyuncu(_dealer);

            // Adım 3: İhale başlangıcında aktif oyuncu = dağıtan
            activePlayer = _dealer;

            // Adım 4: Yeni elde ihale henüz yapılmadı — deklaran geçici olarak dağıtan
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;

            // Adım 5: Aktif oyuncu butonunun durumunu güncelle
            BtnGuney.Checked = false;
            BtnGuney.Enabled = (activePlayer != _humanPlayer);

            // Adım 6: Etiketleri güncelle
            UpdatePlayerBadges();
            UpdateInfoBoard(_dealer);

            // Adım 7: Kartları yeniden dağıt
            // (KartlariDagit içinde KartYerlesimiHazirla + SadeceGuneyiGoster çağrılıyor)
            KartlariDagit(AnaSayfa.SeciliKartSeti);

            // Adım 8: DeklarasyonForm'u aç
            YeniDekBasFormAc();
        }

        private void BtnSonrakiEl_Click(object sender, EventArgs e)
        {
            bool deklarasyonAcik = Application.OpenForms.OfType<DeklarasyonForm>()
                                                    .Any(f => f.Visible);

            
            SonrakiElIsteği();
        }

        // Kontrat, deklaran ve atak (açılış) yapacak oyuncu bilgileri
        private string _kontrat;
        private Player _kontratDeklaran;
        private Player _atakYapacakOyuncu;

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
            _kontrat = kontrat;
            _kontratDeklaran = OyuncuyaCevir(deklaran);
            _declarer = _kontratDeklaran;               // Deklaran belirlendi
            _atakYapacakOyuncu = OyuncuyaCevir(solRakip);

            // Oyun başlangıcında sıra atak yapacak oyuncuda
            activePlayer = _atakYapacakOyuncu;

            // Aktif oyuncu butonunun durumu
            BtnGuney.Checked = false;
            BtnGuney.Enabled = (activePlayer != _humanPlayer);

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
            BtnOtoOyna.Enabled = true;
            BtnIpucu.Enabled = true;
            BtnSonrakiEl.Enabled = true;
            BtnExit.Enabled = true;
            BtnHepsi.Enabled = true;
            BtnEW.Enabled = true;
            BtnNS.Enabled = true;
        }
    }
}