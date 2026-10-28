using System;
using System.Drawing;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Öğrenme modunda ihale sırasında açık duran not penceresi: her teklifin anlamı ve
    /// (insan teklifinde) motor önerisiyle karşılaştırma burada birikir.
    /// </summary>
    public sealed class OgrenmeNotlariForm : Form
    {
        private readonly RichTextBox _kutu;

        public OgrenmeNotlariForm()
        {
            Text = "Öğrenme Notları";
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            ShowInTaskbar = false;
            Size = new Size(380, 460);
            BackColor = Color.FromArgb(24, 40, 28);

            _kutu = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(24, 40, 28),
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 10F),
                DetectUrls = false
            };
            Controls.Add(_kutu);
        }

        /// <summary>Başlıklı bir not ekler. Kullanıcı kapatsa bile form yeniden açılabilir (Gizle).</summary>
        public void Yaz(string baslik, string metin, Color baslikRengi)
        {
            if (IsDisposed) return;
            _kutu.SelectionStart = _kutu.TextLength;
            _kutu.SelectionFont = new Font(_kutu.Font, FontStyle.Bold);
            _kutu.SelectionColor = baslikRengi;
            _kutu.AppendText(baslik + "\n");
            _kutu.SelectionFont = _kutu.Font;
            _kutu.SelectionColor = Color.Gainsboro;
            if (!string.IsNullOrEmpty(metin)) _kutu.AppendText(metin + "\n");
            _kutu.AppendText("\n");
            _kutu.SelectionStart = _kutu.TextLength;
            _kutu.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Kullanıcı X'e basarsa gizle; ihale ekranı kapanınca sahip form bunu gerçekten kapatır
            if (e.CloseReason == CloseReason.UserClosing && Owner != null && !Owner.IsDisposed)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            base.OnFormClosing(e);
        }
    }
}
