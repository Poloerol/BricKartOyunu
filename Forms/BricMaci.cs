using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    /// <summary>Maç Menüsü: yeni maç başlat, kayıtlı maçı yükle, ana menüye dön.</summary>
    public partial class BricMaci : Form
    {
        /// <summary>Kayıtlı maçların varsayılan klasörü: %AppData%\BricKartOyunu\Kayıtlı Maçlar</summary>
        public static string KayitKlasoru
        {
            get
            {
                string yol = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BricKartOyunu", "Kayıtlı Maçlar");
                try { Directory.CreateDirectory(yol); } catch { }
                return yol;
            }
        }

        public BricMaci()
        {
            InitializeComponent();

            this.Text = "Maç Menüsü";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(300, 150);
            this.BackColor = Color.FromArgb(240, 240, 240);

            Dugme("Yeni Bir Maç Başlat", 15, (s, e) => YeniMac());
            Dugme("Kayıtlı Maçı Yükle", 55, (s, e) => KayitliMaciYukle());
            Dugme("Ana Menüye Dön", 95, (s, e) => AnaMenuyeDon());
        }

        private Button Dugme(string metin, int y, EventHandler tikla)
        {
            var b = new Button
            {
                Text = metin,
                Location = new Point(20, y),
                Size = new Size(260, 30),
                BackColor = Color.White,
                UseVisualStyleBackColor = false
            };
            b.Click += tikla;
            this.Controls.Add(b);
            return b;
        }

        private void YeniMac()
        {
            using (var f = new MacOpsiyonlari())
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    // Maç başlatma henüz geliştirilmedi: seçenekler alındı, menü kapanır.
                    this.Close();
                }
            }
        }

        private void KayitliMaciYukle()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Kayıtlı Maçı Yükle";
                dlg.InitialDirectory = KayitKlasoru;
                dlg.Filter = "Briç Maçı Dosyaları (*.mac)|*.mac|Tüm Dosyalar (*.*)|*.*";
                dlg.CheckFileExists = true;
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                // Kayıtlı maç biçimi henüz tanımlı değil; dosya seçimi çalışıyor, yükleme sonraki adım.
                MessageBox.Show(this,
                    "Seçilen dosya:\n" + dlg.FileName + "\n\nKayıtlı maçı yükleme henüz geliştirilmedi.",
                    "Kayıtlı Maçı Yükle", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AnaMenuyeDon()
        {
            var anaSayfa = (this.Owner as AnaSayfa)
                           ?? Application.OpenForms.OfType<AnaSayfa>().FirstOrDefault();
            if (anaSayfa != null)
            {
                anaSayfa.ResetToInitialState();
                anaSayfa.Show();
            }
            this.Close();
        }
    }
}
