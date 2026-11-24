using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Yardım penceresi: exe yanındaki "Yardim" klasöründeki .md dosyalarını konu olarak listeler.
    /// Dosya adı sırayı belirler (01-baslangic.md, 02-...). İlk "# " satırı konu başlığıdır.
    /// Desteklenen biçim: # / ## / ### başlık, "- " madde, **kalın**, boş satır.
    /// Yeni konu eklemek için Yardim klasörüne .md dosyası koymak yeterli (csproj joker karakterle kopyalar).
    /// </summary>
    public sealed class YardimForm : Form
    {
        private sealed class Konu
        {
            public string Dosya, Baslik, Metin;
            public override string ToString() { return Baslik; }
        }

        private readonly List<Konu> _konular = new List<Konu>();
        private readonly TextBox _ara = new TextBox();
        private readonly ListBox _liste = new ListBox();
        private readonly RichTextBox _icerik = new RichTextBox();

        public static string YardimKlasoru
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Yardim"); }
        }

        public YardimForm(string baslangicKonu = null)
        {
            Text = "Yardım";
            Size = new Size(900, 620);
            MinimumSize = new Size(600, 400);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            Font = new Font("Segoe UI", 9F);
            KeyPreview = true;

            var sol = new Panel { Dock = DockStyle.Left, Width = 230, Padding = new Padding(6) };
            _ara.Dock = DockStyle.Top;
            _ara.TextChanged += (s, e) => ListeyiDoldur();
            _liste.Dock = DockStyle.Fill;
            _liste.IntegralHeight = false;
            _liste.SelectedIndexChanged += (s, e) => KonuGoster(_liste.SelectedItem as Konu);
            sol.Controls.Add(_liste);
            sol.Controls.Add(_ara);   // Top, Fill'den sonra eklenince üstte kalır
            var etiket = new Label { Dock = DockStyle.Top, Height = 20, Text = "Ara:" };
            sol.Controls.Add(etiket);

            _icerik.Dock = DockStyle.Fill;
            _icerik.ReadOnly = true;
            _icerik.BackColor = Color.White;
            _icerik.BorderStyle = BorderStyle.None;
            _icerik.Margin = new Padding(12);
            _icerik.DetectUrls = true;
            _icerik.LinkClicked += (s, e) => { try { System.Diagnostics.Process.Start(e.LinkText); } catch { } };

            var ic = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.White };
            ic.Controls.Add(_icerik);

            Controls.Add(ic);
            Controls.Add(new Splitter { Dock = DockStyle.Left });
            Controls.Add(sol);

            KonulariYukle();
            ListeyiDoldur(baslangicKonu);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.F) { _ara.Focus(); e.Handled = true; return; }
            base.OnKeyDown(e);
        }

        private void KonulariYukle()
        {
            _konular.Clear();
            string klasor = YardimKlasoru;
            if (!Directory.Exists(klasor)) return;
            foreach (string yol in Directory.GetFiles(klasor, "*.md").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                string metin;
                try { metin = File.ReadAllText(yol, Encoding.UTF8); } catch { continue; }
                string ad = Path.GetFileNameWithoutExtension(yol);
                string baslik = ad;
                foreach (string satir in metin.Split('\n'))
                {
                    string s = satir.Trim();
                    if (s.StartsWith("# ")) { baslik = s.Substring(2).Trim(); break; }
                }
                _konular.Add(new Konu { Dosya = ad, Baslik = baslik, Metin = metin });
            }
        }

        private void ListeyiDoldur(string secilecekDosya = null)
        {
            string aranan = (_ara.Text ?? "").Trim();
            var secilen = _liste.SelectedItem as Konu;
            _liste.BeginUpdate();
            _liste.Items.Clear();
            foreach (var k in _konular)
            {
                if (aranan.Length == 0
                    || k.Baslik.IndexOf(aranan, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || k.Metin.IndexOf(aranan, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    _liste.Items.Add(k);
            }
            _liste.EndUpdate();

            Konu hedef = null;
            if (secilecekDosya != null)
                hedef = _liste.Items.Cast<Konu>().FirstOrDefault(k => k.Dosya.IndexOf(secilecekDosya, StringComparison.OrdinalIgnoreCase) >= 0);
            if (hedef == null && secilen != null && _liste.Items.Contains(secilen)) hedef = secilen;
            if (hedef != null) _liste.SelectedItem = hedef;
            else if (_liste.Items.Count > 0) _liste.SelectedIndex = 0;
            else KonuGoster(null);
        }

        private void KonuGoster(Konu k)
        {
            _icerik.Clear();
            if (k == null)
            {
                Ekle(Directory.Exists(YardimKlasoru)
                    ? "Aramayla eşleşen konu yok."
                    : "Yardım klasörü bulunamadı:\n" + YardimKlasoru, 10F, false);
                return;
            }
            foreach (string ham in k.Metin.Replace("\r", "").Split('\n'))
            {
                string s = ham.TrimEnd();
                if (s.StartsWith("### ")) Ekle(s.Substring(4) + "\n", 10.5F, true);
                else if (s.StartsWith("## ")) Ekle(s.Substring(3) + "\n", 12.5F, true);
                else if (s.StartsWith("# ")) Ekle(s.Substring(2) + "\n", 16F, true);
                else if (s.StartsWith("- ")) Satir("  •  " + s.Substring(2));
                else Satir(s);
            }
            _icerik.SelectionStart = 0;
            _icerik.ScrollToCaret();
        }

        // **kalın** bölümlerini ayırarak ekler
        private void Satir(string s)
        {
            string[] parca = s.Split(new[] { "**" }, StringSplitOptions.None);
            for (int i = 0; i < parca.Length; i++)
                if (parca[i].Length > 0) Ekle(parca[i], 10F, i % 2 == 1);
            Ekle("\n", 10F, false);
        }

        private void Ekle(string metin, float boyut, bool kalin)
        {
            _icerik.SelectionStart = _icerik.TextLength;
            _icerik.SelectionLength = 0;
            _icerik.SelectionFont = new Font("Segoe UI", boyut, kalin ? FontStyle.Bold : FontStyle.Regular);
            _icerik.SelectionColor = kalin && boyut > 10.5F ? Color.FromArgb(20, 70, 120) : Color.Black;
            _icerik.AppendText(metin);
        }
    }
}
