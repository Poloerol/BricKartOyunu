using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// İhale Analizi (Eylemler menüsü): Bridge Baron'daki "Par Contract" penceresinin karşılığı.
    /// Par kontratı, çift dummy löve tablosu, en yüksek yapılan kontrat, en iyi skorlu kontrat
    /// (her iki taraf, yalnız Kuzey-Güney, yalnız Doğu-Batı). "Yazdır" formu yazıcıya gönderir.
    /// </summary>
    public sealed class IhaleAnaliziForm : Form
    {
        private readonly ParAnalizi _a;
        private readonly string _baslik;
        private static readonly Font Yazi = new Font("Segoe UI", 9F);
        private static readonly Font Kalin = new Font("Segoe UI", 9F, FontStyle.Bold);

        public IhaleAnaliziForm(ParAnalizi analiz, string baslik)
        {
            _a = analiz;
            _baslik = baslik ?? "";
            Text = "İhale Analizi" + (string.IsNullOrEmpty(_baslik) ? "" : " — " + _baslik);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = SystemColors.Control;
            Font = Yazi;
            ClientSize = new Size(470, 560);

            var yazdir = new Button { Text = "Yazdır", Left = 10, Top = 8, Width = 100, Height = 26, UseVisualStyleBackColor = true };
            yazdir.Click += (s, e) => Yazdir();
            Controls.Add(yazdir);
            var kapat = new Button { Text = "Kapat", Left = 360, Top = 524, Width = 100, Height = 26, UseVisualStyleBackColor = true };
            kapat.Click += (s, e) => Close();   // form Show ile (modsuz) açılır; DialogResult düğmesi kapatmaz
            Controls.Add(kapat);
            CancelButton = kapat;

            // Par kontratı
            var g = Grup("Par Kontratı", 10, 70, 215, 118);
            Satir(g, 0, "Kontrat:", analiz.ParKontrati.Kontrat);
            Satir(g, 1, "Löve:", analiz.ParKontrati.Var ? analiz.ParKontrati.Lov.ToString() : "-");
            Satir(g, 2, "Skor:", analiz.ParKontrati.Var ? analiz.ParKontrati.Skor.ToString() : "0");
            Satir(g, 3, "Deklaran:", analiz.ParKontrati.Deklaran);
            Satir(g, 4, "Alternatif deklaran:", analiz.AlternatifDeklaran);

            // Löve tablosu
            Controls.Add(new Label { Text = "Koz rengi:", Left = 300, Top = 12, Width = 160, Height = 18, Font = Yazi });
            Controls.Add(new Label { Text = "Deklaran olursa\r\nlöveler:", Left = 232, Top = 48, Width = 70, Height = 36, Font = Yazi });
            string[] simge = { "♣", "♦", "♥", "♠", "NT" };
            Color[] renk = { Color.FromArgb(0, 128, 0), Color.FromArgb(230, 120, 0), Color.Red, Color.Black, Color.Black };
            for (int c = 0; c < 5; c++)
            {
                var b = new Label
                {
                    Text = simge[c], Left = 308 + c * 31, Top = 34, Width = 28, Height = 28,
                    TextAlign = ContentAlignment.MiddleCenter, BorderStyle = BorderStyle.Fixed3D,
                    BackColor = SystemColors.ControlLight, ForeColor = renk[c], Font = new Font("Segoe UI", c == 4 ? 8F : 12F, FontStyle.Bold)
                };
                Controls.Add(b);
            }
            for (int k = 0; k < 4; k++)
            {
                Controls.Add(new Label { Text = ParAnalizi.KoltukAdi[k], Left = 232, Top = 92 + k * 24 + (k < 2 ? 0 : 0), Width = 64, Height = 20 });
                for (int c = 0; c < 5; c++)
                {
                    int lov = analiz.Tablo.Al(ParAnalizi.KozSutunu[c], k);
                    Controls.Add(new Label
                    {
                        Text = lov < 0 ? "-" : lov.ToString(), Left = 308 + c * 31, Top = 92 + k * 24, Width = 28, Height = 20,
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                }
            }
            // Dört satırın yüksekliği: ilk grup tablonun solunda; aşağıdaki kutular tablonun altından başlar
            int y = 200;
            Kutu("En Yüksek Yapılan Kontrat", 10, y, analiz.EnYuksek);
            Kutu("En İyi Skorlu Kontrat", 236, y, analiz.EnIyiSkor);
            y += 108;
            Kutu("K-G En Yüksek Yapılan Kontrat", 10, y, analiz.KGEnYuksek);
            Kutu("K-G En İyi Skorlu Kontrat", 236, y, analiz.KGEnIyiSkor);
            y += 108;
            Kutu("D-B En Yüksek Yapılan Kontrat", 10, y, analiz.DBEnYuksek);
            Kutu("D-B En İyi Skorlu Kontrat", 236, y, analiz.DBEnIyiSkor);
        }

        private GroupBox Grup(string baslik, int x, int y, int w, int h)
        {
            var g = new GroupBox { Text = baslik, Left = x, Top = y, Width = w, Height = h, Font = Yazi };
            Controls.Add(g);
            return g;
        }

        private static void Satir(Control kap, int i, string etiket, string deger)
        {
            kap.Controls.Add(new Label { Text = etiket, Left = 8, Top = 20 + i * 18, Width = 112, Height = 17, Font = Yazi });
            kap.Controls.Add(new Label { Text = deger, Left = 118, Top = 20 + i * 18, Width = 94, Height = 17, Font = Yazi, AutoEllipsis = true });
        }

        private void Kutu(string baslik, int x, int y, KontratBilgisi k)
        {
            var g = Grup(baslik, x, y, 224, 100);
            Satir2(g, 0, "Kontrat:", k.Var ? k.Kontrat : "-");
            Satir2(g, 1, "Skor:", k.Var ? k.Skor.ToString() : "-");
            Satir2(g, 2, "Löve:", k.Var ? k.Lov.ToString() : "-");
            Satir2(g, 3, "Deklaran:", k.Var ? k.Deklaran : "-");
        }

        private static void Satir2(Control kap, int i, string etiket, string deger)
        {
            kap.Controls.Add(new Label { Text = etiket, Left = 8, Top = 20 + i * 18, Width = 70, Height = 17, Font = Yazi });
            kap.Controls.Add(new Label { Text = deger, Left = 84, Top = 20 + i * 18, Width = 134, Height = 17, Font = Yazi, AutoEllipsis = true });
        }

        private void Yazdir()
        {
            try
            {
                var bmp = new Bitmap(ClientSize.Width, ClientSize.Height);
                DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                using (var pd = new PrintDocument())
                {
                    pd.DocumentName = "İhale Analizi";
                    pd.PrintPage += (s, e) =>
                    {
                        var alan = e.MarginBounds;
                        float oran = Math.Min((float)alan.Width / bmp.Width, (float)alan.Height / bmp.Height);
                        e.Graphics.DrawImage(bmp, alan.Left, alan.Top, bmp.Width * oran, bmp.Height * oran);
                        e.HasMorePages = false;
                    };
                    using (var dlg = new PrintDialog { Document = pd })
                        if (dlg.ShowDialog(this) == DialogResult.OK) pd.Print();
                }
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Yazdırılamadı: " + ex.Message, "İhale Analizi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
