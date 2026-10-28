using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;

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
            this.Load += (s, e) => SayfaOrtasinaAl(this);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(194, 100);
            this.BackColor = Color.FromArgb(240, 240, 240);

            Dugme("Yeni Bir Maç Başlat", 10, (s, e) => YeniMac());
            Dugme("Kayıtlı Maçı Yükle", 38, (s, e) => KayitliMaciYukle());
            Dugme("Ana Menüye Dön", 66, (s, e) => AnaMenuyeDon());
        }

        private Button Dugme(string metin, int y, EventHandler tikla)
        {
            var b = new Button
            {
                Text = metin,
                Location = new Point(11, y),
                Size = new Size(172, 24),
                BackColor = Color.White,
                UseVisualStyleBackColor = false
            };
            b.Click += tikla;
            this.Controls.Add(b);
            return b;
        }

        /// <summary>Formu sahibinin (ana sayfanın) istemci alanının tam ortasına yerleştirir; ekrandan taşmaz.</summary>
        internal static void SayfaOrtasinaAl(Form f)
        {
            try
            {
                var sahip = f.Owner;
                if (sahip == null) return;
                var merkez = sahip.PointToScreen(new Point(sahip.ClientSize.Width / 2, sahip.ClientSize.Height / 2));
                var wa = Screen.FromControl(sahip).WorkingArea;
                int x = Math.Max(wa.Left, Math.Min(merkez.X - f.Width / 2, wa.Right - f.Width));
                int y = Math.Max(wa.Top, Math.Min(merkez.Y - f.Height / 2, wa.Bottom - f.Height));
                f.StartPosition = FormStartPosition.Manual;
                f.Location = new Point(x, y);
            }
            catch { }
        }

        private void YeniMac()
        {
            var anaSayfa = (this.Owner as AnaSayfa) ?? Application.OpenForms.OfType<AnaSayfa>().FirstOrDefault();
            if (anaSayfa == null) return;

            // Maç menüsü gizlenir, Maç Opsiyonları ana sayfanın ortasında açılır
            this.Hide();
            using (var f = new MacOpsiyonlari())
            {
                if (f.ShowDialog(anaSayfa) != DialogResult.OK)
                {
                    this.Show();
                    this.Activate();
                    return;
                }
                MacDurumu.Baslat(f.OyuncuAdi, f.BordSayisi, f.TopluTekrar, f.SeedRastgele, f.Seed, f.Puanlama);
            }

            // Oyun penceresi seçilen maç ayarlarıyla açılır; maç bitince menü de kapanır
            try { anaSayfa.MacOyunuBaslat(); }
            finally { MacDurumu.Bitir(); this.Close(); }
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

        private void BricMaci_Load(object sender, EventArgs e)
        {

        }
    }
}
