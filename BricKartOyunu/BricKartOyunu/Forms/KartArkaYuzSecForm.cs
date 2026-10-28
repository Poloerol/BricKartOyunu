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
                    Left = 30 + i * 150, Top = 40, Width = 110, Height = 150,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Cursor = Cursors.Hand,
                    Image = ResmiYukle(_dosyalar[i]),
                    Padding = new Padding(4)
                };
                pb.Click += (s, e) => { Secilen = _dosyalar[k]; Vurgula(); };
                pb.DoubleClick += (s, e) => { Secilen = _dosyalar[k]; DialogResult = DialogResult.OK; };
                _kutular[i] = pb;
                Controls.Add(pb);
                Controls.Add(new Label { Text = _adlar[i], Left = pb.Left, Top = 194, Width = pb.Width, TextAlign = ContentAlignment.MiddleCenter });
            }

            var tamam = new Button { Text = "Tamam", Left = 160, Top = 216, Width = 75, DialogResult = DialogResult.OK };
            var iptal = new Button { Text = "İptal", Left = 241, Top = 216, Width = 75, DialogResult = DialogResult.Cancel };
            Controls.Add(tamam); Controls.Add(iptal);
            AcceptButton = tamam; CancelButton = iptal;
            Vurgula();
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

        private void Vurgula()
        {
            for (int i = 0; i < 2; i++)
                _kutular[i].BackColor = (_dosyalar[i] == Secilen) ? Color.Gold : SystemColors.Control;
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
