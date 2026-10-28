using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// "Briç Maçı Skoru": maçta oynanan bordların kontratı, löve sayısı ve Kuzey-Güney skoru.
    /// Sağdaki tablo karşılaştırma masaları içindir; bu oyunda karşılaştırma masası olmadığından boştur
    /// (IMP / Board-a-Match 0 puana göre hesaplanır). Yazdır düğmesi yoktur.
    /// </summary>
    public sealed class MacSkorForm : Form
    {
        private readonly BricOyna _oyna;

        public MacSkorForm(BricOyna oyna)
        {
            _oyna = oyna;
            Kur();
            this.Load += (s, e) => OyunOrtasinaAl(this, _oyna);
        }

        /// <summary>Formu oyun sayfasının (BricOyna.playZonePanel) tam ortasına koyar; ekrandan taşmaz.</summary>
        internal static void OyunOrtasinaAl(Form f, BricOyna oyna)
        {
            try
            {
                Point merkez;
                if (oyna != null && oyna.playZonePanel != null && !oyna.playZonePanel.IsDisposed)
                {
                    var p = oyna.playZonePanel;
                    merkez = p.PointToScreen(new Point(p.Width / 2, p.Height / 2));
                }
                else if (oyna != null)
                    merkez = new Point(oyna.Left + oyna.Width / 2, oyna.Top + oyna.Height / 2);
                else return;

                var alan = Screen.FromPoint(merkez).WorkingArea;
                int x = Math.Max(alan.Left, Math.Min(merkez.X - f.Width / 2, alan.Right - f.Width));
                int y = Math.Max(alan.Top, Math.Min(merkez.Y - f.Height / 2, alan.Bottom - f.Height));
                f.StartPosition = FormStartPosition.Manual;
                f.Location = new Point(x, y);
            }
            catch { }
        }

        private static ListView Liste(int x, int y, int gen, int yuk)
        {
            return new ListView
            {
                Location = new Point(x, y),
                Size = new Size(gen, yuk),
                View = View.Details,
                GridLines = true,
                FullRowSelect = true,
                HideSelection = true,
                MultiSelect = false,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                BackColor = Color.White
            };
        }

        private void Kur()
        {
            this.Text = "Briç Maçı Skoru";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(560, 372);
            this.BackColor = Color.FromArgb(240, 240, 240);

            string ucuncu = MacDurumu.Puanlama == MacPuanlama.BordaMac ? "BAM"
                          : MacDurumu.Puanlama == MacPuanlama.ToplamPuan ? "Toplam" : "IMP";

            var baslik = new Label
            {
                Text = MacDurumu.OyuncuAdi + "  (Kuzey-Güney)",
                Location = new Point(12, 10),
                AutoSize = true,
                Font = new Font("Arial", 10F, FontStyle.Bold)
            };
            this.Controls.Add(baslik);

            var sol = Liste(12, 36, 352, 262);
            sol.Columns.Add("Bord", 46);
            sol.Columns.Add("Kontrat", 118);
            sol.Columns.Add("Löve", 48, HorizontalAlignment.Right);
            sol.Columns.Add("Skor", 70, HorizontalAlignment.Right);
            sol.Columns.Add(ucuncu, 56, HorizontalAlignment.Right);

            var kayitlar = MacDurumu.Kayitlar();
            int topSkor = 0, topImp = 0; double topBam = 0;
            foreach (var k in kayitlar)
            {
                topSkor += k.NsSkor;
                topImp += MacDurumu.Imp(k.NsSkor);
                topBam += k.NsSkor > 0 ? 1 : k.NsSkor == 0 ? 0.5 : 0;
                var it = new ListViewItem(k.Sira.ToString());
                it.SubItems.Add(k.Kontrat);
                it.SubItems.Add(k.Love >= 0 ? k.Love.ToString() : "");
                it.SubItems.Add(k.NsSkor.ToString());
                it.SubItems.Add(MacDurumu.BordDegeri(k.NsSkor));
                sol.Items.Add(it);
            }
            var top = new ListViewItem("Toplam");
            top.Font = new Font(sol.Font, FontStyle.Bold);
            top.SubItems.Add("");
            top.SubItems.Add("");
            top.SubItems.Add(topSkor.ToString());
            top.SubItems.Add(MacDurumu.Puanlama == MacPuanlama.BordaMac ? topBam.ToString("0.#")
                           : MacDurumu.Puanlama == MacPuanlama.ToplamPuan ? topSkor.ToString() : topImp.ToString());
            sol.Items.Add(top);
            this.Controls.Add(sol);

            // Karşılaştırma tablosu: bu oyunda karşılaştırma masası yok → boş
            var sag = Liste(374, 36, 174, 262);
            sag.Columns.Add("Karşılaştırma", 100);
            sag.Columns.Add("Skor", 70, HorizontalAlignment.Right);
            this.Controls.Add(sag);

            string tohum = "";
            if (kayitlar.Count > 0)
                tohum = "Oynadığınız bordlar: " + kayitlar.Min(k => k.Seed) + " --- " + kayitlar.Max(k => k.Seed);
            this.Controls.Add(new Label { Text = tohum, Location = new Point(12, 308), AutoSize = true });

            var ok = new Button { Text = "OK", Size = new Size(90, 28), Location = new Point(12, 334), DialogResult = DialogResult.OK };
            var imp = new Button { Text = "IMP Tablosu", Size = new Size(110, 28), Location = new Point(110, 334) };
            imp.Click += (s, e) =>
            {
                using (var f = new ImpTablosuForm(_oyna)) f.ShowDialog(this);
            };
            this.Controls.Add(ok);
            this.Controls.Add(imp);
            this.AcceptButton = ok;
            this.CancelButton = ok;
        }
    }
}
