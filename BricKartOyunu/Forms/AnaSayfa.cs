using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace BricKartOyunu.Forms
{
    public partial class AnaSayfa : Form
    {
        // Merkezi konteyner; içinde başlık paneli ve ButonPaneli olacak
        private Panel merkezPanel;
        // Proje başlığını gösterecek panel ve etiket
        private Panel projeBaslikPanel;
        private Label projeBaslikLabel;
        private Label projeBaslikShadow;

        public AnaSayfa()
        {
            InitializeComponent();
        }

        private void AnaSayfa_Load(object sender, EventArgs e)
        {
            // Tasarımcı (Design mode) sırasında bazı API'ler çağrılmamalı.
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            // Merkezi konteyner ve başlık paneli oluştur
            if (merkezPanel == null)
            {
                merkezPanel = new Panel();
                merkezPanel.BackColor = Color.Transparent;
                merkezPanel.AutoSize = false;
                this.Controls.Add(merkezPanel);
                merkezPanel.BringToFront();
            }
            if (projeBaslikPanel == null)
            {
                projeBaslikPanel = new Panel();
                projeBaslikPanel.Height = 80; // başlık panel yüksekliği
                projeBaslikPanel.BackColor = Color.Transparent;
                merkezPanel.Controls.Add(projeBaslikPanel);
                projeBaslikPanel.BringToFront();
            }
            if (projeBaslikLabel == null)
            {
                // Shadow (gölge) etiketi
                projeBaslikShadow = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 42F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(160, 0, 0, 0),
                    BackColor = Color.Transparent
                };
                // Asıl başlık etiketi
                projeBaslikLabel = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 42F, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    BackColor = Color.Transparent
                };
                // Sabit başlık metni
                projeBaslikLabel.Text = "BRİÇ KART OYUNU";
                projeBaslikShadow.Text = projeBaslikLabel.Text;
                // Gölgeyi önce, başlığı sonra ekle
                projeBaslikPanel.Controls.Add(projeBaslikShadow);
                projeBaslikPanel.Controls.Add(projeBaslikLabel);
                projeBaslikLabel.BringToFront();
            }
            // ButonPaneli'ni merkezPanel içine taşı (sadece bir kez)
            if (ButonPaneli.Parent != merkezPanel)
            {
                merkezPanel.Controls.Add(ButonPaneli);
            }
            // İlk konumlandırmayı yap
            CenterPanel();
            // Arka plan resmini yükle (Images\Bacjkgrounds\AnaCover.png)
            try
            {
                string imgPath = Path.Combine(Application.StartupPath, "Images", "Backgrounds", "AnaCover.png");
                if (File.Exists(imgPath))
                {
                    using (var fs = File.OpenRead(imgPath))
                    using (var img = Image.FromStream(fs))
                    {
                        this.BackgroundImage = new Bitmap(img);
                    }
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch
            {
                // Yükleme hatasıyı yoksay
            }
            // Panelin arka planını sayfanın arka planı ile saydam göster
            try
            {
                ButonPaneli.BackColor = Color.Transparent;
            }
            catch
            {
                // Hata yok say
            }
        }
        private void AnaSayfa_Resize(object sender, EventArgs e)
        {
            CenterPanel();
        }
        private void CenterPanel()
        {
            // Merkez panelin boyutunu hesapla ve ortala
            if (merkezPanel != null)
            {
                int spacing = 8;
                // merkezPanel genişliği, içeriğin genişliğine göre ayarlanır
                int contentWidth = Math.Max(ButonPaneli.Width, projeBaslikLabel?.Width + 12 ?? ButonPaneli.Width);
                merkezPanel.Width = contentWidth;
                merkezPanel.Height = projeBaslikPanel?.Height + spacing + ButonPaneli.Height ?? ButonPaneli.Height;
                merkezPanel.Location = new Point(
                    (this.ClientSize.Width - merkezPanel.Width) / 2,
                    (this.ClientSize.Height - merkezPanel.Height) / 2
                );

                // ButonPaneli'ni merkezPanel içinde konumlandır (alt orta)
                ButonPaneli.Location = new Point((merkezPanel.Width - ButonPaneli.Width) / 2, projeBaslikPanel.Height + spacing);

                // Başlık panelini merkezPanel içinde üstte yerleştir ve etiket ortala
                if (projeBaslikPanel != null)
                {
                    projeBaslikPanel.Width = merkezPanel.Width;
                    projeBaslikPanel.Location = new Point(0, 0);
                    if (projeBaslikLabel != null)
                    {
                        int lx = (projeBaslikPanel.ClientSize.Width - projeBaslikLabel.Width) / 2;
                        int ly = (projeBaslikPanel.ClientSize.Height - projeBaslikLabel.Height) / 2;
                        if (lx < 0) lx = 6;
                        if (ly < 0) ly = 6;
                        // Gölgeyi biraz sağa ve aşağı ofsetle
                        if (projeBaslikShadow != null)
                        {
                            projeBaslikShadow.Location = new Point(lx + 3, ly + 3);
                        }
                        projeBaslikLabel.Location = new Point(lx, ly);
                    }
                }
            }
        }
        private void ÇıkışToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AraçÇubuğuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip1.Visible = araçÇubuğuToolStripMenuItem.Checked;
        }

        private void DurumÇubuğuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip1.Visible = durumÇubuğuToolStripMenuItem.Checked;
        }
             
    }
}
