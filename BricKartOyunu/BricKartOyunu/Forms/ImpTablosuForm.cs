using System;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>"IMP Tablosu": puan farkı aralığından IMP karşılığı (0-24).</summary>
    public sealed class ImpTablosuForm : Form
    {
        private readonly BricOyna _oyna;

        public ImpTablosuForm(BricOyna oyna)
        {
            _oyna = oyna;
            this.Text = "IMP Tablosu";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(420, 296);
            this.BackColor = Color.FromArgb(240, 240, 240);

            int n = MacDurumu.ImpSayisi;
            int ilkYari = (n + 1) / 2;
            this.Controls.Add(Tablo(12, 0, ilkYari));
            this.Controls.Add(Tablo(214, ilkYari, n));

            var ok = new Button { Text = "OK", Size = new Size(90, 28), Location = new Point(165, 256), DialogResult = DialogResult.OK };
            this.Controls.Add(ok);
            this.AcceptButton = ok;
            this.CancelButton = ok;
            this.Load += (s, e) => MacSkorForm.OyunOrtasinaAl(this, _oyna);
        }

        private static ListView Tablo(int x, int bas, int son)
        {
            var lv = new ListView
            {
                Location = new Point(x, 12),
                Size = new Size(194, 238),
                View = View.Details,
                GridLines = true,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                BackColor = Color.White,
                Scrollable = false
            };
            lv.Columns.Add("IMP", 44, HorizontalAlignment.Right);
            lv.Columns.Add("Puan farkı", 146);
            for (int i = bas; i < son; i++)
            {
                int alt, ust;
                MacDurumu.ImpAraligi(i, out alt, out ust);
                var it = new ListViewItem(i.ToString());
                it.SubItems.Add(ust < 0 ? alt + " ve üstü" : alt + " – " + ust);
                lv.Items.Add(it);
            }
            return lv;
        }
    }
}
