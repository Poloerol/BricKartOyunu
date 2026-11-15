using System;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// El Numarası: "Numaralı Rastgele Eller" dağıtım kaynağında oyun başlarken açılır (Bridge Baron'daki "Deal Number" gibi).
    /// Numara dağıtımın seed'idir; − / + bir önceki/sonraki numaraya geçer, Tamam kapatır, "Tüm elleri göster" dört eli açık dağıtır.
    /// </summary>
    public class ElNumarasiForm : Form
    {
        private readonly TextBox _txtNo = new TextBox();
        private readonly CheckBox _chkHepsi = new CheckBox();

        /// <summary>Seçilen el numarası (seed).</summary>
        public int Numara { get; private set; }
        /// <summary>true: dört el de açık gösterilir.</summary>
        public bool TumElleriGoster { get; private set; }

        /// <summary>Verilirse pencere bu ekran noktasının (masa panelinin merkezi) tam ortasına açılır.</summary>
        public Point? MerkezEkran { get; set; }

        /// <summary>"Tüm elleri göster" kutusu değişince (işaretli: dört el açık, değil: yalnızca Güney) anında bildirilir.</summary>
        public event Action<bool> TumElleriDegisti;

        public ElNumarasiForm(int baslangicNo)
        {
            Numara = baslangicNo > 0 ? baslangicNo : 1;

            this.Text = "Dağılım Numarası";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(246, 150);
            this.BackColor = Color.FromArgb(240, 240, 240);

            this.Controls.Add(new Label
            {
                Text = "Eski tarz el numarasını girin:",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 12),
                Size = new Size(246, 20)
            });

            _txtNo.Text = Numara.ToString();
            _txtNo.TextAlign = HorizontalAlignment.Center;
            _txtNo.MaxLength = 10;
            _txtNo.Location = new Point(61, 38);
            _txtNo.Size = new Size(124, 23);
            this.Controls.Add(_txtNo);

            var eksi = new Button { Text = "-", Size = new Size(34, 26), Location = new Point(20, 76) };
            var tamam = new Button { Text = "TAMAM", Size = new Size(88, 26), Location = new Point(79, 76) };
            var arti = new Button { Text = "+", Size = new Size(34, 26), Location = new Point(192, 76) };
            eksi.Click += (s, e) => Adim(-1);
            arti.Click += (s, e) => Adim(+1);
            tamam.Click += (s, e) => Onayla();
            this.Controls.Add(eksi);
            this.Controls.Add(tamam);
            this.Controls.Add(arti);

            _chkHepsi.Text = "Tüm elleri göster";
            _chkHepsi.AutoSize = true;
            _chkHepsi.Location = new Point(62, 116);
            this.Controls.Add(_chkHepsi);
            _chkHepsi.CheckedChanged += (s, e) => { TumElleriGoster = _chkHepsi.Checked; var h = TumElleriDegisti; if (h != null) h(_chkHepsi.Checked); };

            this.AcceptButton = tamam;
            this.Load += (s, e) => Ortala();
            this.Shown += (s, e) => { _txtNo.Focus(); _txtNo.SelectAll(); };
        }

        /// <summary>Masa panelinin merkezi verildiyse oraya yerleşir; ekrandan taşmaz.</summary>
        private void Ortala()
        {
            if (!MerkezEkran.HasValue) return;
            try
            {
                var m = MerkezEkran.Value;
                var wa = Screen.FromPoint(m).WorkingArea;
                int x = Math.Max(wa.Left, Math.Min(m.X - this.Width / 2, wa.Right - this.Width));
                int y = Math.Max(wa.Top, Math.Min(m.Y - this.Height / 2, wa.Bottom - this.Height));
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(x, y);
            }
            catch { }
        }
        private bool NoOku(out int no)
        {
            return int.TryParse(_txtNo.Text.Trim(), out no) && no > 0;
        }

        private void Adim(int fark)
        {
            int no;
            if (!NoOku(out no)) no = Numara;
            long yeni = (long)no + fark;
            if (yeni < 1) yeni = 1;
            if (yeni > int.MaxValue) yeni = int.MaxValue;
            _txtNo.Text = yeni.ToString();
            _txtNo.Focus();
            _txtNo.SelectAll();
        }

        private void Onayla()
        {
            int no;
            if (!NoOku(out no))
            {
                MessageBox.Show(this, "Lütfen 1 ile " + int.MaxValue + " arasında bir el numarası girin.", "Dağılım Numarası",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNo.Focus();
                _txtNo.SelectAll();
                return;
            }
            Numara = no;
            TumElleriGoster = _chkHepsi.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}