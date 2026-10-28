using System;
using System.Drawing;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    /// <summary>Maç Opsiyonları: yeni maç başlatılmadan önce açılan seçenekler formu (içeriği sonradan doldurulacak).</summary>
    public class MacOpsiyonlari : Form
    {
        public MacOpsiyonlari()
        {
            this.Text = "Maç Opsiyonları";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(380, 240);
            this.BackColor = Color.FromArgb(240, 240, 240);

            var not = new Label
            {
                Text = "Maç seçenekleri burada ayarlanacak.",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 10),
                Size = new Size(360, 170)
            };
            this.Controls.Add(not);

            var tamam = new Button { Text = "Başlat", Size = new Size(88, 28), DialogResult = DialogResult.OK };
            var iptal = new Button { Text = "İptal", Size = new Size(88, 28), DialogResult = DialogResult.Cancel };
            tamam.Location = new Point(this.ClientSize.Width / 2 - 88 - 8, 195);
            iptal.Location = new Point(this.ClientSize.Width / 2 + 8, 195);
            this.Controls.Add(tamam);
            this.Controls.Add(iptal);
            this.AcceptButton = tamam;
            this.CancelButton = iptal;
        }
    }
}
