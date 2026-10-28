using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// "İhale Bitti" bilgi penceresi: MessageBox gibi görünür ama kontrattaki renk simgesi renkli
    /// (♥ kırmızı, ♣ yeşil, ♦ turuncu) ve El No kalındır. "Tamam" düğmesi alt şeritte ortalanır.
    /// </summary>
    public sealed class IhaleBilgiForm : Form
    {
        public static readonly Color KupaRengi = Color.Red;
        public static readonly Color SinekRengi = Color.FromArgb(0, 140, 0);
        public static readonly Color KaroRengi = Color.FromArgb(255, 140, 0);

        private struct Parca
        {
            public string Metin; public Color? Renk; public bool Kalin;
        }

        private readonly List<Parca> _parcalar = new List<Parca>();

        private IhaleBilgiForm() { }

        /// <summary>Kontrat satırını ("4♥", "3NT", "2♠ X") renkli simgeyle ekler.</summary>
        private void KontratEkle(string kontrat, string kontrEk)
        {
            _parcalar.Add(new Parca { Metin = "Kontrat: " });
            foreach (char ch in kontrat ?? "")
            {
                Color? renk = null;
                if (ch == '♥') renk = KupaRengi;
                else if (ch == '♣') renk = SinekRengi;
                else if (ch == '♦') renk = KaroRengi;
                _parcalar.Add(new Parca { Metin = ch.ToString(), Renk = renk });
            }
            _parcalar.Add(new Parca { Metin = (kontrEk ?? "") + "\n" });
        }

        private void Satir(string metin) { _parcalar.Add(new Parca { Metin = metin + "\n" }); }

        /// <summary>
        /// İhale bilgisini gösterir. dagitici / zon / elNo boş bırakılırsa o satırlar yazılmaz.
        /// ek: bilgi satırlarından sonra eklenen serbest metin (boş satırla ayrılır).
        /// </summary>
        public static void Goster(IWin32Window sahip, string baslik, string kontrat, string kontrEk,
                                  string deklaran, string atak, string dagitici, string zon, string elNo, string ek = null)
        {
            var f = new IhaleBilgiForm();
            f.KontratEkle(kontrat, kontrEk);
            f.Satir("Deklaran: " + deklaran);
            f.Satir("Atak: " + atak);
            if (!string.IsNullOrEmpty(dagitici)) f.Satir("Dağıtıcı: " + dagitici);
            if (!string.IsNullOrEmpty(zon)) f.Satir("Zon: " + zon);
            if (!string.IsNullOrEmpty(elNo))
            {
                f._parcalar.Add(new Parca { Metin = "El No: " });
                f._parcalar.Add(new Parca { Metin = elNo, Kalin = true });
                f._parcalar.Add(new Parca { Metin = "\n" });
            }
            if (!string.IsNullOrEmpty(ek))
            {
                f._parcalar.Add(new Parca { Metin = "\n" + ek });
            }
            f.Kur(baslik);
            try { f.ShowDialog(sahip); } finally { f.Dispose(); }
        }

        private void Kur(string baslik)
        {
            this.Text = baslik;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.Font = SystemFonts.MessageBoxFont;
            this.BackColor = SystemColors.Control;

            // Satır sayısı ve en geniş satır
            var duz = new System.Text.StringBuilder();
            foreach (var p in _parcalar) duz.Append(p.Metin);
            string[] satirlar = duz.ToString().TrimEnd('\n').Split('\n');
            int satirYuk = this.Font.Height + 2;
            int en = 0;
            foreach (var s in satirlar) en = Math.Max(en, TextRenderer.MeasureText(s, this.Font).Width);
            int metinGen = Math.Max(160, en + 24);
            int metinYuk = satirlar.Length * satirYuk + 4;

            const int sol = 22, ikon = 32, bosluk = 16, ust = 22, altSerit = 48;
            int istGen = sol + ikon + bosluk + metinGen + 22;
            int ustYuk = Math.Max(ikon, metinYuk) + ust * 2;
            this.ClientSize = new Size(Math.Max(istGen, 230), ustYuk + altSerit);

            var ustPanel = new Panel { Dock = DockStyle.Top, Height = ustYuk, BackColor = Color.White };
            var alt = new Panel { Dock = DockStyle.Fill, BackColor = SystemColors.Control };
            this.Controls.Add(alt);
            this.Controls.Add(ustPanel);

            var pic = new PictureBox
            {
                Image = SystemIcons.Information.ToBitmap(),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Size = new Size(ikon, ikon),
                Location = new Point(sol, ust)
            };
            ustPanel.Controls.Add(pic);

            var rtb = new RichTextBox
            {
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                BackColor = Color.White,
                ScrollBars = RichTextBoxScrollBars.None,
                TabStop = false,
                Cursor = Cursors.Default,
                WordWrap = false,
                DetectUrls = false,
                Font = this.Font,
                Location = new Point(sol + ikon + bosluk, ust),
                Size = new Size(this.ClientSize.Width - (sol + ikon + bosluk) - 12, metinYuk)
            };
            ustPanel.Controls.Add(rtb);
            foreach (var p in _parcalar)
            {
                rtb.SelectionStart = rtb.TextLength;
                rtb.SelectionLength = 0;
                rtb.SelectionColor = p.Renk ?? Color.Black;
                rtb.SelectionFont = new Font(this.Font, p.Kalin ? FontStyle.Bold : FontStyle.Regular);
                rtb.AppendText(p.Metin);
            }
            rtb.SelectionStart = 0;
            rtb.SelectionLength = 0;

            var btn = new Button { Text = "Tamam", Size = new Size(88, 26), DialogResult = DialogResult.OK };
            alt.Controls.Add(btn);
            Action ortala = () => btn.Location = new Point((alt.ClientSize.Width - btn.Width) / 2, (alt.ClientSize.Height - btn.Height) / 2);
            alt.Resize += (s, e) => ortala();
            ortala();
            this.AcceptButton = btn;
            this.CancelButton = btn;
            this.ActiveControl = btn;
            this.Shown += (s, e) => { ortala(); btn.Focus(); };
        }
    }
}
