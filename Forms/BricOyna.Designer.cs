namespace BricKartOyunu
{
    partial class BricOyna
    {
        /// <summary>
        ///Gerekli tasarımcı değişkeni.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        ///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer üretilen kod

        /// <summary>
        /// Tasarımcı desteği için gerekli metot - bu metodun 
        ///içeriğini kod düzenleyici ile değiştirmeyin.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BricOyna));
            this.btnGeri = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.çıkışToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ayarlarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sonrakiElToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eylmlerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ihaleGösterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.görünümToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.masaRengiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.değerlendirmeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ellerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hepsiniGösterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eWGösterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nSGösterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oynayanOyuncuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ipucuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.programToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.yardımToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.BrnKartArka = new System.Windows.Forms.ToolStripButton();
            this.BtnMasaRenk = new System.Windows.Forms.ToolStripButton();
            this.BtnHighLight = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.BtnOtoOyna = new System.Windows.Forms.ToolStripButton();
            this.BtnClaim = new System.Windows.Forms.ToolStripButton();
            this.BtnGeriAl = new System.Windows.Forms.ToolStripButton();
            this.BtnileriAl = new System.Windows.Forms.ToolStripButton();
            this.BtnIpucu = new System.Windows.Forms.ToolStripButton();
            this.BtnSonrakiEl = new System.Windows.Forms.ToolStripButton();
            this.BtnExit = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.BtnHepsi = new System.Windows.Forms.ToolStripButton();
            this.BtnEW = new System.Windows.Forms.ToolStripButton();
            this.BtnNS = new System.Windows.Forms.ToolStripButton();
            this.BtnGuney = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.oynananElleriGösterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGeri
            // 
            this.btnGeri.Location = new System.Drawing.Point(867, 465);
            this.btnGeri.Name = "btnGeri";
            this.btnGeri.Size = new System.Drawing.Size(128, 38);
            this.btnGeri.TabIndex = 1;
            this.btnGeri.Text = "Geri";
            this.btnGeri.UseVisualStyleBackColor = true;
            this.btnGeri.Click += new System.EventHandler(this.BtnGeri_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dosyaToolStripMenuItem,
            this.ayarlarToolStripMenuItem,
            this.eylmlerToolStripMenuItem,
            this.görünümToolStripMenuItem,
            this.değerlendirmeToolStripMenuItem,
            this.ellerToolStripMenuItem,
            this.ipucuToolStripMenuItem,
            this.programToolStripMenuItem,
            this.yardımToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1099, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // dosyaToolStripMenuItem
            // 
            this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.çıkışToolStripMenuItem});
            this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
            this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.dosyaToolStripMenuItem.Text = "Dosya";
            // 
            // çıkışToolStripMenuItem
            // 
            this.çıkışToolStripMenuItem.Name = "çıkışToolStripMenuItem";
            this.çıkışToolStripMenuItem.Size = new System.Drawing.Size(99, 22);
            this.çıkışToolStripMenuItem.Text = "Çıkış";
            this.çıkışToolStripMenuItem.Click += new System.EventHandler(this.ÇıkışToolStripMenuItem_Click);
            // 
            // ayarlarToolStripMenuItem
            // 
            this.ayarlarToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sonrakiElToolStripMenuItem});
            this.ayarlarToolStripMenuItem.Name = "ayarlarToolStripMenuItem";
            this.ayarlarToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.ayarlarToolStripMenuItem.Text = "Ayarlar";
            // 
            // sonrakiElToolStripMenuItem
            // 
            this.sonrakiElToolStripMenuItem.Name = "sonrakiElToolStripMenuItem";
            this.sonrakiElToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.sonrakiElToolStripMenuItem.Text = "Sonraki El";
            this.sonrakiElToolStripMenuItem.Click += new System.EventHandler(this.SonrakiElToolStripMenuItem_Click);
            // 
            // eylmlerToolStripMenuItem
            // 
            this.eylmlerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ihaleGösterToolStripMenuItem,
            this.oynananElleriGösterToolStripMenuItem});
            this.eylmlerToolStripMenuItem.Name = "eylmlerToolStripMenuItem";
            this.eylmlerToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.eylmlerToolStripMenuItem.Text = "Eylemler";
            // 
            // ihaleGösterToolStripMenuItem
            // 
            this.ihaleGösterToolStripMenuItem.Name = "ihaleGösterToolStripMenuItem";
            this.ihaleGösterToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.ihaleGösterToolStripMenuItem.Text = "İhaleyi Göster";
            this.ihaleGösterToolStripMenuItem.Click += new System.EventHandler(this.IhaleGösterToolStripMenuItem_Click);
            // 
            // görünümToolStripMenuItem
            // 
            this.görünümToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.masaRengiToolStripMenuItem});
            this.görünümToolStripMenuItem.Name = "görünümToolStripMenuItem";
            this.görünümToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.görünümToolStripMenuItem.Text = "Görünüm";
            // 
            // masaRengiToolStripMenuItem
            // 
            this.masaRengiToolStripMenuItem.Name = "masaRengiToolStripMenuItem";
            this.masaRengiToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.masaRengiToolStripMenuItem.Text = "Masa Rengi";
            this.masaRengiToolStripMenuItem.Click += new System.EventHandler(this.MasaRengiToolStripMenuItem_Click);
            // 
            // değerlendirmeToolStripMenuItem
            // 
            this.değerlendirmeToolStripMenuItem.Name = "değerlendirmeToolStripMenuItem";
            this.değerlendirmeToolStripMenuItem.Size = new System.Drawing.Size(97, 20);
            this.değerlendirmeToolStripMenuItem.Text = "Değerlendirme";
            // 
            // ellerToolStripMenuItem
            // 
            this.ellerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hepsiniGösterToolStripMenuItem,
            this.eWGösterToolStripMenuItem,
            this.nSGösterToolStripMenuItem,
            this.oynayanOyuncuToolStripMenuItem});
            this.ellerToolStripMenuItem.Name = "ellerToolStripMenuItem";
            this.ellerToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.ellerToolStripMenuItem.Text = "Eller";
            // 
            // hepsiniGösterToolStripMenuItem
            // 
            this.hepsiniGösterToolStripMenuItem.Name = "hepsiniGösterToolStripMenuItem";
            this.hepsiniGösterToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.hepsiniGösterToolStripMenuItem.Text = "Hepsini Göster";
            this.hepsiniGösterToolStripMenuItem.Click += new System.EventHandler(this.HepsiniGösterToolStripMenuItem_Click);
            // 
            // eWGösterToolStripMenuItem
            // 
            this.eWGösterToolStripMenuItem.Name = "eWGösterToolStripMenuItem";
            this.eWGösterToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.eWGösterToolStripMenuItem.Text = "E/W Göster";
            this.eWGösterToolStripMenuItem.Click += new System.EventHandler(this.EWGösterToolStripMenuItem_Click);
            // 
            // nSGösterToolStripMenuItem
            // 
            this.nSGösterToolStripMenuItem.Name = "nSGösterToolStripMenuItem";
            this.nSGösterToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.nSGösterToolStripMenuItem.Text = "N/S Göster";
            this.nSGösterToolStripMenuItem.Click += new System.EventHandler(this.NSGösterToolStripMenuItem_Click);
            // 
            // oynayanOyuncuToolStripMenuItem
            // 
            this.oynayanOyuncuToolStripMenuItem.Name = "oynayanOyuncuToolStripMenuItem";
            this.oynayanOyuncuToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.oynayanOyuncuToolStripMenuItem.Text = "Oynayan Oyuncu";
            this.oynayanOyuncuToolStripMenuItem.Click += new System.EventHandler(this.OynayanOyuncuToolStripMenuItem_Click);
            // 
            // ipucuToolStripMenuItem
            // 
            this.ipucuToolStripMenuItem.Name = "ipucuToolStripMenuItem";
            this.ipucuToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
            this.ipucuToolStripMenuItem.Text = "İpucu";
            // 
            // programToolStripMenuItem
            // 
            this.programToolStripMenuItem.Name = "programToolStripMenuItem";
            this.programToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.programToolStripMenuItem.Text = "Program";
            // 
            // yardımToolStripMenuItem
            // 
            this.yardımToolStripMenuItem.Name = "yardımToolStripMenuItem";
            this.yardımToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.yardımToolStripMenuItem.Text = "Yardım";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BrnKartArka,
            this.BtnMasaRenk,
            this.BtnHighLight,
            this.toolStripSeparator1,
            this.BtnOtoOyna,
            this.BtnClaim,
            this.BtnGeriAl,
            this.BtnileriAl,
            this.BtnIpucu,
            this.BtnSonrakiEl,
            this.BtnExit,
            this.toolStripSeparator2,
            this.BtnHepsi,
            this.BtnEW,
            this.BtnNS,
            this.BtnGuney,
            this.toolStripSeparator3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1099, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // BrnKartArka
            // 
            this.BrnKartArka.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BrnKartArka.Image = ((System.Drawing.Image)(resources.GetObject("BrnKartArka.Image")));
            this.BrnKartArka.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BrnKartArka.Name = "BrnKartArka";
            this.BrnKartArka.Size = new System.Drawing.Size(23, 22);
            this.BrnKartArka.Text = "toolStripButton1";
            this.BrnKartArka.ToolTipText = "Kart Arka Yüzü";
            // 
            // BtnMasaRenk
            // 
            this.BtnMasaRenk.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnMasaRenk.Image = ((System.Drawing.Image)(resources.GetObject("BtnMasaRenk.Image")));
            this.BtnMasaRenk.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnMasaRenk.Name = "BtnMasaRenk";
            this.BtnMasaRenk.Size = new System.Drawing.Size(23, 22);
            this.BtnMasaRenk.Text = "toolStripButton2";
            this.BtnMasaRenk.ToolTipText = "Masa Rengi";
            // 
            // BtnHighLight
            // 
            this.BtnHighLight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnHighLight.Image = ((System.Drawing.Image)(resources.GetObject("BtnHighLight.Image")));
            this.BtnHighLight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnHighLight.Name = "BtnHighLight";
            this.BtnHighLight.Size = new System.Drawing.Size(23, 22);
            this.BtnHighLight.Text = "toolStripButton3";
            this.BtnHighLight.ToolTipText = "Kazanan El Rengi";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // BtnOtoOyna
            // 
            this.BtnOtoOyna.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnOtoOyna.Image = ((System.Drawing.Image)(resources.GetObject("BtnOtoOyna.Image")));
            this.BtnOtoOyna.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnOtoOyna.Name = "BtnOtoOyna";
            this.BtnOtoOyna.Size = new System.Drawing.Size(23, 22);
            this.BtnOtoOyna.Text = "toolStripButton4";
            this.BtnOtoOyna.ToolTipText = "Otomatik Oynama";
            // 
            // BtnClaim
            // 
            this.BtnClaim.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnClaim.Image = ((System.Drawing.Image)(resources.GetObject("BtnClaim.Image")));
            this.BtnClaim.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnClaim.Name = "BtnClaim";
            this.BtnClaim.Size = new System.Drawing.Size(23, 22);
            this.BtnClaim.Text = "toolStripButton5";
            this.BtnClaim.ToolTipText = "Claim";
            // 
            // BtnGeriAl
            // 
            this.BtnGeriAl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnGeriAl.Image = ((System.Drawing.Image)(resources.GetObject("BtnGeriAl.Image")));
            this.BtnGeriAl.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnGeriAl.Name = "BtnGeriAl";
            this.BtnGeriAl.Size = new System.Drawing.Size(23, 22);
            this.BtnGeriAl.Text = "toolStripButton6";
            this.BtnGeriAl.ToolTipText = "Geri Al";
            // 
            // BtnileriAl
            // 
            this.BtnileriAl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnileriAl.Image = ((System.Drawing.Image)(resources.GetObject("BtnileriAl.Image")));
            this.BtnileriAl.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnileriAl.Name = "BtnileriAl";
            this.BtnileriAl.Size = new System.Drawing.Size(23, 22);
            this.BtnileriAl.Text = "toolStripButton7";
            this.BtnileriAl.ToolTipText = "İleri Al";
            // 
            // BtnIpucu
            // 
            this.BtnIpucu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnIpucu.Image = ((System.Drawing.Image)(resources.GetObject("BtnIpucu.Image")));
            this.BtnIpucu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnIpucu.Name = "BtnIpucu";
            this.BtnIpucu.Size = new System.Drawing.Size(23, 22);
            this.BtnIpucu.Text = "toolStripButton8";
            this.BtnIpucu.ToolTipText = "İpucu";
            // 
            // BtnSonrakiEl
            // 
            this.BtnSonrakiEl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnSonrakiEl.Image = ((System.Drawing.Image)(resources.GetObject("BtnSonrakiEl.Image")));
            this.BtnSonrakiEl.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnSonrakiEl.Name = "BtnSonrakiEl";
            this.BtnSonrakiEl.Size = new System.Drawing.Size(23, 22);
            this.BtnSonrakiEl.Text = "toolStripButton9";
            this.BtnSonrakiEl.ToolTipText = "Sonraki El";
            this.BtnSonrakiEl.Click += new System.EventHandler(this.BtnSonrakiEl_Click);
            // 
            // BtnExit
            // 
            this.BtnExit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnExit.Image = ((System.Drawing.Image)(resources.GetObject("BtnExit.Image")));
            this.BtnExit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Size = new System.Drawing.Size(23, 22);
            this.BtnExit.Text = "toolStripButton10";
            this.BtnExit.ToolTipText = "Oyundan Çık";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // BtnHepsi
            // 
            this.BtnHepsi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnHepsi.Image = ((System.Drawing.Image)(resources.GetObject("BtnHepsi.Image")));
            this.BtnHepsi.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnHepsi.Name = "BtnHepsi";
            this.BtnHepsi.Size = new System.Drawing.Size(23, 22);
            this.BtnHepsi.Text = "toolStripButton11";
            this.BtnHepsi.ToolTipText = "Hepsini Göster";
            this.BtnHepsi.Click += new System.EventHandler(this.BtnHepsi_Click);
            // 
            // BtnEW
            // 
            this.BtnEW.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnEW.Image = ((System.Drawing.Image)(resources.GetObject("BtnEW.Image")));
            this.BtnEW.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnEW.Name = "BtnEW";
            this.BtnEW.Size = new System.Drawing.Size(23, 22);
            this.BtnEW.Text = "toolStripButton12";
            this.BtnEW.ToolTipText = "E/W Göster";
            this.BtnEW.Click += new System.EventHandler(this.BtnEW_Click);
            // 
            // BtnNS
            // 
            this.BtnNS.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnNS.Image = ((System.Drawing.Image)(resources.GetObject("BtnNS.Image")));
            this.BtnNS.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnNS.Name = "BtnNS";
            this.BtnNS.Size = new System.Drawing.Size(23, 22);
            this.BtnNS.Text = "toolStripButton13";
            this.BtnNS.ToolTipText = "N/S Göster";
            this.BtnNS.Click += new System.EventHandler(this.BtnNS_Click);
            // 
            // BtnGuney
            // 
            this.BtnGuney.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BtnGuney.Image = ((System.Drawing.Image)(resources.GetObject("BtnGuney.Image")));
            this.BtnGuney.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BtnGuney.Name = "BtnGuney";
            this.BtnGuney.Size = new System.Drawing.Size(23, 22);
            this.BtnGuney.Text = "toolStripButton14";
            this.BtnGuney.ToolTipText = "Aktif Oyuncu Elini Göster";
            this.BtnGuney.Click += new System.EventHandler(this.BtnGuney_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 555);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1099, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // oynananElleriGösterToolStripMenuItem
            // 
            this.oynananElleriGösterToolStripMenuItem.Name = "oynananElleriGösterToolStripMenuItem";
            this.oynananElleriGösterToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.oynananElleriGösterToolStripMenuItem.Text = "Oynanan Elleri Göster";
            this.oynananElleriGösterToolStripMenuItem.Click += new System.EventHandler(this.oynananElleriGösterToolStripMenuItem_Click);
            // 
            // BricOyna
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1099, 577);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.btnGeri);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "BricOyna";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Birç Oyna";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BricOyna_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnGeri;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem dosyaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem çıkışToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ayarlarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eylmlerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem görünümToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem değerlendirmeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ellerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ipucuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem programToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yardımToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton BrnKartArka;
        private System.Windows.Forms.ToolStripButton BtnMasaRenk;
        private System.Windows.Forms.ToolStripButton BtnHighLight;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton BtnOtoOyna;
        private System.Windows.Forms.ToolStripButton BtnClaim;
        private System.Windows.Forms.ToolStripButton BtnGeriAl;
        private System.Windows.Forms.ToolStripButton BtnileriAl;
        private System.Windows.Forms.ToolStripButton BtnIpucu;
        private System.Windows.Forms.ToolStripButton BtnSonrakiEl;
        private System.Windows.Forms.ToolStripButton BtnExit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton BtnHepsi;
        private System.Windows.Forms.ToolStripButton BtnEW;
        private System.Windows.Forms.ToolStripButton BtnNS;
        private System.Windows.Forms.ToolStripButton BtnGuney;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem masaRengiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hepsiniGösterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eWGösterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nSGösterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oynayanOyuncuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sonrakiElToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem ihaleGösterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oynananElleriGösterToolStripMenuItem;
    }
}

