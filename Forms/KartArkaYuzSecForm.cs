using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    /// <summary>Kart arka yüzü seçimi: Resources\Cards\Backs içindeki iki resimden biri seçilir.</summary>
    public class KartArkaYuzSecForm : Form
    {
        /// <summary>Seçilen dosya adı (AppState.ArkaYuzKirmizi / ArkaYuzSiyah).</summary>
        public string Secilen { get; private set; }

        private readonly PictureBox[] _kutular = new PictureBox[2];
        private readonly string[] _dosyalar = { AppState.ArkaYuzKirmizi, AppState.ArkaYuzSiyah };
        private readonly string[] _adlar = { "Kırmızı", "Siyah" };

        public KartArkaYuzSecForm(string mevcutDosya)
        {
            Secilen = (mevcutDosya == AppState.ArkaYuzSiyah) ? AppState.ArkaYuzSiyah : AppState.ArkaYuzKirmizi;

            Text = "Kart Arka Yüzü";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
            ClientSize = new Size(330, 250);

            Controls.Add(new Label { Text = "Kartların arka yüzünü seçin:", Left = 16, Top = 12, Width = 300, Height = 20 });

            for (int i = 0; i < 2; i++)
            {
                int k = i;
                var pb = new PictureBox
                {
                    // 102x136: kartın 3:4 oranıyla aynı, resmin çevresinde boşluk ya da renkli çerçeve kalmaz
                    Left = 34 + i * 150, Top = 44, Width = 102, Height = 136,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Cursor = Cursors.Hand,
                    Image = ResmiYukle(_dosyalar[i])
                };
                // Seçili kart, kartın altındaki seçim düğmesiyle gösterilir
                var secim = new RadioButton { Text = _adlar[i], Left = pb.Left, Top = 188, Width = pb.Width, Height = 20, CheckAlign = ContentAlignment.MiddleLeft, Padding = new Padding(22, 0, 0, 0) };
                secim.CheckedChanged += (s, e) => { if (secim.Checked) Secilen = _dosyalar[k]; };
                secim.Checked = _dosyalar[i] == Secilen;
                pb.Click += (s, e) => secim.Checked = true;
                pb.DoubleClick += (s, e) => { secim.Checked = true; DialogResult = DialogResult.OK; };
                _kutular[i] = pb;
                Controls.Add(pb);
                Controls.Add(secim);
            }

            var tamam = new Button { Text = "Tamam", Left = 160, Top = 216, Width = 75, DialogResult = DialogResult.OK };
            var iptal = new Button { Text = "İptal", Left = 241, Top = 216, Width = 75, DialogResult = DialogResult.Cancel };
            Controls.Add(tamam); Controls.Add(iptal);
            AcceptButton = tamam; CancelButton = iptal;
        }

        private static Image ResmiYukle(string dosya)
        {
            try
            {
                string yol = AppState.ArkaYuzYolu(dosya);
                if (!File.Exists(yol)) return null;
                using (var fs = new FileStream(yol, FileMode.Open, FileAccess.Read))
                using (var temp = Image.FromStream(fs)) return new Bitmap(temp);   // dosyayı kilitleme
            }
            catch { return null; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                foreach (var pb in _kutular)
                    if (pb != null && pb.Image != null) { pb.Image.Dispose(); pb.Image = null; }
            base.Dispose(disposing);
        }
    }
}
