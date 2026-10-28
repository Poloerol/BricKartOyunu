using System;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>Maç Opsiyonları: oyuncu adı, bord sayısı, tekrar, maç tohumu (seed) ve puanlama yöntemi.</summary>
    public class MacOpsiyonlari : Form
    {
        private readonly TextBox _txtAd = new TextBox();
        private readonly RadioButton[] _bordRadyo = new RadioButton[5];
        private static readonly int[] BordSecenekleri = { 1, 2, 4, 8, 16 };
        private readonly CheckBox _chkToplu = new CheckBox();
        private readonly RadioButton _rdRastgele = new RadioButton();
        private readonly RadioButton _rdSecilen = new RadioButton();
        private readonly TextBox _txtSeed = new TextBox();
        private readonly RadioButton _rdImp = new RadioButton();
        private readonly RadioButton _rdBam = new RadioButton();
        private readonly RadioButton _rdToplam = new RadioButton();

        /// <summary>Oyuncunun adı (boşsa "İnsan Oyuncu").</summary>
        public string OyuncuAdi { get; private set; }
        public int BordSayisi { get; private set; }
        /// <summary>Maç bittikten sonra tüm bordları toplu tekrar oynatma.</summary>
        public bool TopluTekrar { get; private set; }
        /// <summary>true: her bord için rastgele dağıtım; false: Seed kullanılır.</summary>
        public bool SeedRastgele { get; private set; }
        public int Seed { get; private set; }
        public MacPuanlama Puanlama { get; private set; }

        public MacOpsiyonlari()
        {
            this.Text = "Maç Opsiyonları";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Load += (s, e) => BricMaci.SayfaOrtasinaAl(this);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(410, 252);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Oyuncu adı
            this.Controls.Add(new Label { Text = "Adınızı girin:", AutoSize = true, Location = new Point(14, 24) });
            _txtAd.Text = "İnsan Oyuncu";
            _txtAd.Location = new Point(110, 20);
            _txtAd.Size = new Size(120, 23);
            _txtAd.MaxLength = 24;
            this.Controls.Add(_txtAd);

            // Tekrar
            var gbTekrar = Grup("Tekrar", 248, 6, 148, 56);
            _chkToplu.Text = "Toplu Tekrar";
            _chkToplu.AutoSize = true;
            _chkToplu.Location = new Point(14, 24);
            gbTekrar.Controls.Add(_chkToplu);

            // Bord sayısı
            var gbBord = Grup("Bord Sayısı", 14, 72, 220, 70);
            for (int i = 0; i < BordSecenekleri.Length; i++)
            {
                _bordRadyo[i] = new RadioButton
                {
                    Text = BordSecenekleri[i].ToString(),
                    AutoSize = false,
                    Size = new Size(40, 20),
                    Location = new Point(12 + i * 40, 30)
                };
                gbBord.Controls.Add(_bordRadyo[i]);
                _bordRadyo[i].Checked = BordSecenekleri[i] == MacDurumu.VarsayilanBordSayisi;
            }

            // Maç tohumu
            var gbSeed = Grup("Maç Tohumu", 248, 72, 148, 70);
            _rdRastgele.Text = "Rastgele"; _rdRastgele.AutoSize = true; _rdRastgele.Checked = true;
            _rdRastgele.Location = new Point(12, 20);
            _rdSecilen.Text = "Seçilen"; _rdSecilen.AutoSize = true;
            _rdSecilen.Location = new Point(12, 44);
            _txtSeed.Location = new Point(78, 42);
            _txtSeed.Size = new Size(62, 23);
            _txtSeed.MaxLength = 9;
            _txtSeed.Enabled = false;
            _rdSecilen.CheckedChanged += (s, e) => { _txtSeed.Enabled = _rdSecilen.Checked; if (_rdSecilen.Checked) _txtSeed.Focus(); };
            gbSeed.Controls.Add(_rdRastgele);
            gbSeed.Controls.Add(_rdSecilen);
            gbSeed.Controls.Add(_txtSeed);

            // Puanlama yöntemi
            var gbPuan = Grup("Puanlama Yöntemi", 14, 152, 382, 52);
            _rdImp.Text = "IMP"; _rdImp.Checked = true; _rdImp.Location = new Point(14, 22);
            _rdBam.Text = "Board-a-Match"; _rdBam.Location = new Point(124, 22);
            _rdToplam.Text = "Toplam Puan"; _rdToplam.Location = new Point(264, 22);
            foreach (var r in new[] { _rdImp, _rdBam, _rdToplam }) { r.AutoSize = true; gbPuan.Controls.Add(r); }

            // Düğmeler
            var tamam = new Button { Text = "Tamam", Size = new Size(92, 26), Location = new Point(70, 216) };
            var iptal = new Button { Text = "İptal", Size = new Size(92, 26), Location = new Point(248, 216), DialogResult = DialogResult.Cancel };
            tamam.Click += (s, e) => Onayla();
            this.Controls.Add(tamam);
            this.Controls.Add(iptal);
            this.AcceptButton = tamam;
            this.CancelButton = iptal;
        }

        private GroupBox Grup(string baslik, int x, int y, int w, int h)
        {
            var g = new GroupBox { Text = baslik, Location = new Point(x, y), Size = new Size(w, h) };
            this.Controls.Add(g);
            return g;
        }

        private void Onayla()
        {
            int seed = 0;
            bool rastgele = _rdRastgele.Checked;
            if (!rastgele && (!int.TryParse(_txtSeed.Text.Trim(), out seed) || seed < 0))
            {
                MessageBox.Show(this, "Lütfen maç tohumu için geçerli bir sayı girin.", "Maç Opsiyonları",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _txtSeed.Focus();
                return;
            }

            OyuncuAdi = string.IsNullOrWhiteSpace(_txtAd.Text) ? "İnsan Oyuncu" : _txtAd.Text.Trim();
            BordSayisi = 4;
            for (int i = 0; i < _bordRadyo.Length; i++) if (_bordRadyo[i].Checked) BordSayisi = BordSecenekleri[i];
            TopluTekrar = _chkToplu.Checked;
            SeedRastgele = rastgele;
            Seed = seed;
            Puanlama = _rdBam.Checked ? MacPuanlama.BordaMac : _rdToplam.Checked ? MacPuanlama.ToplamPuan : MacPuanlama.IMP;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
