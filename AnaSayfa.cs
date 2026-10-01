using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    public partial class AnaSayfa : Form
    {
        // ====================================================================
        // FAÇADE PROPERTY'LER (Geçiş süreci)
        // --------------------------------------------------------------------
        // Bu iki property daha önce burada tanımlıydı. Şimdi asıl değerler
        // AppState sınıfında tutuluyor. Diğer formlar (BricOyna, KartDagitici
        // vb.) hâlâ AnaSayfa.SeciliKartSeti / AnaSayfa.MasaRengi şeklinde
        // eriştiği için burada facade olarak bırakıldı. Tüm formlar AppState'e
        // geçtiğinde bu property'ler güvenle silinebilir.
        // ====================================================================

        public static string SeciliKartSeti
        {
            get => AppState.SeciliKartSeti;
            set => AppState.SeciliKartSeti = value;
        }

        public static Color MasaRengi
        {
            get => AppState.MasaRengi;
            set => AppState.MasaRengi = value;
        }

        // ====================================================================
        // SABİTLER
        // ====================================================================

        private static readonly string KirmiziArkaYol = Path.Combine(
            Application.StartupPath, "Resources", "Cards", "Backs", "Back.png");

        private static readonly string MaviArkaYol = Path.Combine(
            Application.StartupPath, "Resources", "Cards", "Backs", "Back01.png");

        private static readonly string AnaCoverYol = Path.Combine(
            Application.StartupPath, "Images", "Backgrounds", "AnaCover.png");

        // ====================================================================
        // ALANLAR
        // ====================================================================

        // Merkezi konteyner; içinde başlık paneli ve ButonPaneli olacak
        private Panel merkezPanel;

        // Proje başlığını gösterecek panel ve etiket
        private Panel projeBaslikPanel;
        private Label projeBaslikLabel;
        private Label projeBaslikShadow;

        // Dinamik orta butonlar
        private Button ortaButon;
        private Button ortaButon2;
        private Button ortaButon3;

        // Dinamik butonları merkezi olarak takip etmek için liste.
        // ResetToInitialState içinde sadece bu listedeki butonlar temizlenir;
        // merkezPanel içine sonradan eklenebilecek başka Buton'lara dokunulmaz.
        private readonly List<Button> dinamikButonlar = new List<Button>();

        private enum ActiveMain { None, Oyna, Ogrenme, Yarisma, Araclar }
        private ActiveMain activeMain = ActiveMain.None;
        private bool mainButtonsLocked = false;

        private string currentBackImage = KirmiziArkaYol;

        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        public AnaSayfa()
        {
            InitializeComponent();

            // Varsayılan olarak Classic seçili olsun
            classicToolStripMenuItem.Checked = true;
            sayısalToolStripMenuItem.Checked = false;

            // Çift tamponlama — dinamik yerleşimde titremeyi azaltır
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            this.UpdateStyles();
        }

        // ====================================================================
        // MENÜ — KART SETİ SEÇİMİ
        // ====================================================================

        private void ClassicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SeciliKartSeti = "Classic";
            classicToolStripMenuItem.Checked = true;
            sayısalToolStripMenuItem.Checked = false;
        }

        private void SayısalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SeciliKartSeti = "Sembols";
            sayısalToolStripMenuItem.Checked = true;
            classicToolStripMenuItem.Checked = false;
        }

        // ====================================================================
        // FORM YÜKLEME
        // ====================================================================

        private void AnaSayfa_Load(object sender, EventArgs e)
        {
            InitializeDynamicControls();
            WireMainButtonEvents();
            LoadBackgroundImage();

            // İlk konumlandırmayı yap
            CenterPanel();
        }

        private void InitializeDynamicControls()
        {
            // Merkezi konteyner
            if (merkezPanel == null)
            {
                merkezPanel = new Panel
                {
                    BackColor = Color.Transparent,
                    AutoSize = false
                };
                this.Controls.Add(merkezPanel);
                merkezPanel.BringToFront();
            }

            // Başlık paneli
            if (projeBaslikPanel == null)
            {
                projeBaslikPanel = new Panel
                {
                    Height = 80,
                    BackColor = Color.Transparent
                };
                this.Controls.Add(projeBaslikPanel);
                projeBaslikPanel.BringToFront();
            }

            // Başlık etiketleri (gölge + ana)
            if (projeBaslikLabel == null)
            {
                projeBaslikShadow = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 42F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(160, 0, 0, 0),
                    BackColor = Color.Transparent
                };

                projeBaslikLabel = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 42F, FontStyle.Bold),
                    ForeColor = Color.GreenYellow,
                    BackColor = Color.Transparent,
                    Text = "BRİÇ KART OYUNU"
                };

                projeBaslikShadow.Text = projeBaslikLabel.Text;

                projeBaslikPanel.Controls.Add(projeBaslikShadow);
                projeBaslikPanel.Controls.Add(projeBaslikLabel);
                projeBaslikLabel.BringToFront();
            }

            // ButonPaneli'ni merkezPanel içine taşı (sadece bir kez)
            if (ButonPaneli.Parent != merkezPanel)
            {
                merkezPanel.Controls.Add(ButonPaneli);
            }

            // ButonPaneli arka planını saydam yap
            try
            {
                ButonPaneli.BackColor = Color.Transparent;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AnaSayfa] ButonPaneli.BackColor ayarlanamadı: {ex.Message}");
            }
        }

        private void WireMainButtonEvents()
        {
            // Tüm ana butonlar için ortak handler ata (tekrar bağlanmayı önle)
            BindClickOnce(BtnOyna, MainButton_Click);
            BindClickOnce(BtnOgrenme, MainButton_Click);
            BindClickOnce(BtnYarisma, MainButton_Click);
            BindClickOnce(BtnAraclar, MainButton_Click);
        }

        private static void BindClickOnce(Button btn, EventHandler handler)
        {
            if (btn == null) return;
            btn.Click -= handler;
            btn.Click += handler;
        }

        private void LoadBackgroundImage()
        {
            try
            {
                if (File.Exists(AnaCoverYol))
                {
                    using (var fs = File.OpenRead(AnaCoverYol))
                    using (var img = Image.FromStream(fs))
                    {
                        this.BackgroundImage = new Bitmap(img);
                    }
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AnaSayfa] Arka plan resmi yüklenemedi: {ex.Message}");
            }
        }

        // ====================================================================
        // FORM OLAYLARI
        // ====================================================================

        private void AnaSayfa_Resize(object sender, EventArgs e)
        {
            CenterPanel();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 🔹 AnaSayfa kapanırken (yani uygulama kapanırken) kart resim
            // önbelleğini temizle. Program.cs'deki finally bloğu da aynı işi
            // yapar; ancak Environment.Exit gibi yollarla erken kapanmalarda
            // finally atlanabilir. İki katmanlı güvence.
            try
            {
                BricOyna.CacheTemizle();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AnaSayfa] Cache temizlenirken hata: {ex.Message}");
            }

            base.OnFormClosing(e);
        }

        // ====================================================================
        // ANA BUTON KİLİTLEME
        // ====================================================================

        private void SetMainButtonsLocked(bool locked)
        {
            mainButtonsLocked = locked;

            Color lockedBack = Color.FromArgb(60, 60, 60);
            Color normalBack = Color.Black;
            Color fore = Color.Yellow;

            ApplyLockStyle(BtnOyna, locked, normalBack, lockedBack, fore);
            ApplyLockStyle(BtnOgrenme, locked, normalBack, lockedBack, fore);
            ApplyLockStyle(BtnYarisma, locked, normalBack, lockedBack, fore);
            ApplyLockStyle(BtnAraclar, locked, normalBack, lockedBack, fore);
        }

        private static void ApplyLockStyle(Button btn, bool locked,
            Color normalBack, Color lockedBack, Color fore)
        {
            if (btn == null) return;

            btn.UseVisualStyleBackColor = false;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = locked ? lockedBack : normalBack;
            btn.ForeColor = fore;
        }

        // ====================================================================
        // MERKEZİ YERLEŞİM
        // ====================================================================

        private void CenterPanel()
        {
            if (merkezPanel == null) return;

            int spacing = 8;
            int btnSpacing = 12;
            int groupWidth = 0;

            if (ortaButon != null)
            {
                EnsureAdditionalOrtaButtons();
                groupWidth = ortaButon.Width * 3 + btnSpacing * 2;
            }

            int labelWidth = (projeBaslikLabel != null) ? projeBaslikLabel.Width + 12 : 0;
            int contentWidth = Math.Max(ButonPaneli.Width, Math.Max(groupWidth, labelWidth));
            merkezPanel.Width = contentWidth;

            // Sabit yükseklik: sadece ana butonları ve biraz üstünü kapsasın
            merkezPanel.Height = ButonPaneli.Height + 150;

            // merkezPanel'i form ortasına yerleştir
            merkezPanel.Location = new Point(
                (this.ClientSize.Width - merkezPanel.Width) / 2,
                (this.ClientSize.Height - merkezPanel.Height) / 2
            );

            LayoutTitlePanel();
            LayoutMainButtons();
            LayoutMiddleButtons(spacing, btnSpacing, groupWidth);
        }

        private void LayoutTitlePanel()
        {
            if (projeBaslikPanel == null) return;

            projeBaslikPanel.Width = merkezPanel.Width;

            int topReference = 0;
            if (toolStrip1 != null && toolStrip1.Visible)
                topReference = toolStrip1.Bottom;
            else if (menuStrip1 != null)
                topReference = menuStrip1.Bottom;

            int headerTop = topReference + 100;
            if (headerTop + projeBaslikPanel.Height > this.ClientSize.Height)
                headerTop = Math.Max(8, this.ClientSize.Height - projeBaslikPanel.Height - 8);

            int headerLeft = (this.ClientSize.Width - projeBaslikPanel.Width) / 2;
            projeBaslikPanel.Location = new Point(headerLeft, headerTop);

            if (projeBaslikLabel != null)
            {
                int lx = (projeBaslikPanel.ClientSize.Width - projeBaslikLabel.Width) / 2;
                int ly = (projeBaslikPanel.ClientSize.Height - projeBaslikLabel.Height) / 2;
                if (lx < 0) lx = 6;
                if (ly < 0) ly = 6;

                if (projeBaslikShadow != null)
                    projeBaslikShadow.Location = new Point(lx + 3, ly + 3);

                projeBaslikLabel.Location = new Point(lx, ly);
            }
        }

        private void LayoutMainButtons()
        {
            int butonY = merkezPanel.Height - ButonPaneli.Height;
            ButonPaneli.Location = new Point(
                (merkezPanel.Width - ButonPaneli.Width) / 2, butonY);
        }

        private void LayoutMiddleButtons(int spacing, int btnSpacing, int groupWidth)
        {
            if (ortaButon == null || ortaButon2 == null || ortaButon3 == null) return;

            int butonY = merkezPanel.Height - ButonPaneli.Height;
            int startX = (merkezPanel.Width - groupWidth) / 2;
            int ortaY = butonY - ortaButon.Height - spacing;

            ortaButon.Location = new Point(startX, ortaY);
            ortaButon2.Location = new Point(startX + ortaButon.Width + btnSpacing, ortaY);
            ortaButon3.Location = new Point(startX + (ortaButon.Width + btnSpacing) * 2, ortaY);
        }

        // ====================================================================
        // MENÜ OLAYLARI
        // ====================================================================

        private void ÇıkışToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AraçÇubuğuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip1.Visible = araçÇubuğuToolStripMenuItem.Checked;
        }

        private void DurumÇubuğuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip1.Visible = durumÇubuğuToolStripMenuItem.Checked;
        }

        private void MasaRengiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    MasaRengi = cd.Color;
                    this.BackColor = MasaRengi;
                }
            }
        }

        private void KırmızıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentBackImage = KirmiziArkaYol;
            kırmızıToolStripMenuItem.Checked = true;
            maviToolStripMenuItem.Checked = false;
        }

        private void MaviToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentBackImage = MaviArkaYol;
            maviToolStripMenuItem.Checked = true;
            kırmızıToolStripMenuItem.Checked = false;
        }

        // ====================================================================
        // ANA BUTONLAR
        // ====================================================================

        private void BtnOyna_Click(object sender, EventArgs e)
        {
            // Deprecated: artık ana butonlar MainButton_Click ile işleniyor
            MainButton_Click(sender, e);
        }

        private void MainButton_Click(object sender, EventArgs e)
        {
            // Butonlar kilitliyse işlem yapma
            if (mainButtonsLocked) return;

            // Hangi ana buton tıklandı?
            if (sender == BtnOyna)
                activeMain = ActiveMain.Oyna;
            else if (sender == BtnOgrenme)
                activeMain = ActiveMain.Ogrenme;
            else if (sender == BtnYarisma)
                activeMain = ActiveMain.Yarisma;
            else if (sender == BtnAraclar)
                activeMain = ActiveMain.Araclar;

            // Orta butonları oluştur (yoksa)
            if (ortaButon == null || ortaButon.IsDisposed)
            {
                ortaButon = new Button
                {
                    Size = new Size(140, 37),
                    BackColor = Color.Black,
                    ForeColor = Color.Yellow,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    UseVisualStyleBackColor = false
                };
                merkezPanel.Controls.Add(ortaButon);
                ortaButon.BringToFront();
                dinamikButonlar.Add(ortaButon);
            }

            EnsureAdditionalOrtaButtons();
            SetMiddleButtonTexts();

            // Event bağla (tekrar bağlanmayı önle)
            BindClickOnce(ortaButon, OrtaButon_Click);
            BindClickOnce(ortaButon2, OrtaButon2_Click);
            BindClickOnce(ortaButon3, OrtaButon3_Click);

            // Ana butonları kilitle
            SetMainButtonsLocked(true);

            CenterPanel();
        }

        private void SetMiddleButtonTexts()
        {
            switch (activeMain)
            {
                case ActiveMain.Oyna:
                    ortaButon.Text = "Briç Oyna";
                    ortaButon2.Text = "İnternette Briç";
                    ortaButon3.Text = "Geri";
                    break;
                case ActiveMain.Ogrenme:
                    ortaButon.Text = "Konvansiyonlar";
                    ortaButon2.Text = "Zor Eller";
                    ortaButon3.Text = "Geri";
                    break;
                case ActiveMain.Yarisma:
                    ortaButon.Text = "Briç Maçı";
                    ortaButon2.Text = "Turnuva";
                    ortaButon3.Text = "Geri";
                    break;
                case ActiveMain.Araclar:
                    ortaButon.Text = "Kart Dağıtıcı";
                    ortaButon2.Text = "Dağılım Kütüphanesi";
                    ortaButon3.Text = "Geri";
                    break;
            }
        }

        // ====================================================================
        // ORTA BUTONLAR
        // ====================================================================

        private void OrtaButon_Click(object sender, EventArgs e)
        {
            switch (activeMain)
            {
                case ActiveMain.Oyna:
                    // BricOyna: AnaSayfa kapanır, BricOyna tek başına kalır.
                    // Kapatınca AnaSayfa tekrar açılır ve durum sıfırlanır.
                    var bricOynaForm = new BricOyna
                    {
                        CurrentBackImage = currentBackImage
                    };

                    this.Hide();

                    try
                    {
                        bricOynaForm.ShowDialog();
                    }
                    finally
                    {
                        this.Show();
                        this.Activate();
                        ResetToInitialState();
                    }
                    break;

                case ActiveMain.Ogrenme:
                    ShowChildForm(new Konvansiyon());
                    break;

                case ActiveMain.Yarisma:
                    ShowChildForm(new BricMaci());
                    break;

                case ActiveMain.Araclar:
                    ShowChildForm(new KartDagitici());
                    break;
            }
        }

        private void OrtaButon2_Click(object sender, EventArgs e)
        {
            switch (activeMain)
            {
                case ActiveMain.Oyna:
                    MessageBox.Show("Hızlı Oyna seçildi.");
                    break;
                case ActiveMain.Ogrenme:
                    ShowChildForm(new ZorEller());
                    break;
                case ActiveMain.Yarisma:
                    ShowChildForm(new Turnuva());
                    break;
                case ActiveMain.Araclar:
                    ShowChildForm(new DagilimDepo());
                    break;
            }
        }
        /// <summary>
        /// Alt formları görev çubuğunda göstermeden açar.
        /// AnaSayfa açık kalır, alt form onun önünde gösterilir.
        /// </summary>
        private void ShowChildForm(Form childForm)
        {
            if (childForm == null) return;

            // Görev çubuğunda görünmesin
            childForm.ShowInTaskbar = false;

            // AnaSayfa'nın sahibi olarak ayarla → her zaman üstte kalır,
            // AnaSayfa kapanınca otomatik kapanır.
            childForm.Owner = this;

            // AnaSayfa'yı kapatma, sadece alt formu aç
            childForm.Show();
        }

        private void OrtaButon3_Click(object sender, EventArgs e)
        {
            ResetToInitialState();
        }

        // ====================================================================
        // DURUM SIFIRLAMA
        // ====================================================================

        /// <summary>
        /// AnaSayfa'yı başlangıç durumuna döndürür: orta butonları temizler,
        /// ana butonların kilidini açar, arayüzü yeniden çizer.
        /// </summary>
        public void ResetToInitialState()
        {
            // 1) Aktif kategoriyi sıfırla
            activeMain = ActiveMain.None;

            // 2) Ana butonların kilidini aç
            SetMainButtonsLocked(false);
            foreach (Button btn in new[] { BtnOyna, BtnOgrenme, BtnYarisma, BtnAraclar })
            {
                if (btn == null) continue;
                btn.Visible = true;
                btn.Enabled = true;
            }

            // 3) Sadece bizim oluşturduğumuz dinamik butonları temizle
            foreach (var btn in dinamikButonlar)
            {
                if (btn == null) continue;
                merkezPanel?.Controls.Remove(btn);
                btn.Dispose();
            }
            dinamikButonlar.Clear();

            ortaButon = null;
            ortaButon2 = null;
            ortaButon3 = null;

            // 4) Arayüzü yeniden hesapla ve zorla çiz
            CenterPanel();
            if (merkezPanel != null)
            {
                merkezPanel.Invalidate();
                merkezPanel.Update();
            }
            this.Refresh();
        }

        // ====================================================================
        // YARDIMCI: ORTA BUTON OLUŞTURMA & STİL
        // ====================================================================

        private void EnsureAdditionalOrtaButtons()
        {
            if (merkezPanel == null) return;

            int btnWidth = (ortaButon != null) ? ortaButon.Width : 140;
            int btnHeight = (ortaButon != null) ? ortaButon.Height : 40;

            if (ortaButon2 == null)
            {
                ortaButon2 = new Button { Text = "Seçenek 2", Size = new Size(btnWidth, btnHeight) };
                merkezPanel.Controls.Add(ortaButon2);
                ortaButon2.BringToFront();
                dinamikButonlar.Add(ortaButon2);
            }
            if (ortaButon3 == null)
            {
                ortaButon3 = new Button { Text = "Geri", Size = new Size(btnWidth, btnHeight) };
                merkezPanel.Controls.Add(ortaButon3);
                ortaButon3.BringToFront();
                dinamikButonlar.Add(ortaButon3);
            }

            StyleAllButtons();
        }

        private void StyleAllButtons()
        {
            // Ana butonlar
            ApplyButtonStyle(BtnOyna, Color.Black, Color.Yellow, Color.DimGray);
            ApplyButtonStyle(BtnOgrenme, Color.Black, Color.Yellow, Color.DimGray);
            ApplyButtonStyle(BtnYarisma, Color.Black, Color.Yellow, Color.DimGray);
            ApplyButtonStyle(BtnAraclar, Color.Black, Color.Yellow, Color.DimGray);

            // Orta butonlar
            ApplyButtonStyle(ortaButon, Color.Black, Color.Yellow, Color.DimGray);
            ApplyButtonStyle(ortaButon2, Color.Black, Color.Yellow, Color.DimGray);
            ApplyButtonStyle(ortaButon3, Color.DarkRed, Color.Yellow, Color.Red);
        }

        /// <summary>
        /// Bir butona modern stil uygular. MouseEnter/MouseLeave handler'ları
        /// her çağrıda yeniden bağlanmaz — Tag üzerinden kontrol edilir.
        /// Böylece event handler birikmesi (memory leak) önlenir.
        /// </summary>
        private void ApplyButtonStyle(Button btn, Color normalBack, Color fore, Color hoverBack)
        {
            if (btn == null) return;

            btn.UseVisualStyleBackColor = false;
            btn.BackColor = normalBack;
            btn.ForeColor = fore;
            btn.FlatStyle = FlatStyle.Flat;

            // Daha önce stil uygulanmış mı?
            const string StyleTag = "__stilUygulandi";
            if (btn.Tag as string == StyleTag) return;

            btn.Tag = StyleTag;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = hoverBack;
                btn.ForeColor = fore;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = normalBack;
                btn.ForeColor = fore;
            };
        }
    }
}