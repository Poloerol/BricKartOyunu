using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Rubber / Chicago Rubber / Chicago Duplicate skor penceresi: bordların sırayla puanları
    /// (K-G ve D-B, çizgi altı / üstü), oyun ve rubber bilgisi, oturum toplamı.
    /// </summary>
    public sealed class KlasikSkorForm : Form
    {
        private readonly BricOyna _oyun;
        private readonly PuanTuru _tur;
        private readonly RichTextBox _kutu = new RichTextBox();

        public KlasikSkorForm(BricOyna oyun, PuanTuru tur)
        {
            _oyun = oyun;
            _tur = tur;
            Text = tur == PuanTuru.Rubber ? "Rubber Skor" : tur == PuanTuru.ChicagoRubber ? "Chicago Rubber Skor" : "Chicago Duplicate Skor";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
            ClientSize = new Size(640, 470);
            Font = new Font("Microsoft Sans Serif", 10F);

            _kutu.Left = 10; _kutu.Top = 10; _kutu.Width = 620; _kutu.Height = 400;
            _kutu.ReadOnly = true; _kutu.BackColor = Color.White;
            _kutu.Font = new Font("Consolas", 10F);
            _kutu.WordWrap = false;
            Controls.Add(_kutu);

            var tamam = new Button { Text = "Tamam", Left = 160, Top = 420, Width = 140, Height = 36, DialogResult = DialogResult.OK };
            var temizle = new Button { Text = "Temizle", Left = 340, Top = 420, Width = 140, Height = 36 };
            temizle.Click += (s, e) =>
            {
                if (MessageBox.Show(this, "Oturumun tüm bord puanları silinsin mi? (Yeni rubber / yeni Chicago başlar.)", Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                PuanOturumu.Temizle();
                Doldur();
            };
            Controls.Add(tamam); Controls.Add(temizle);
            AcceptButton = tamam; CancelButton = tamam;
            Doldur();
        }

        private static string Sut(int n) { return n == 0 ? "" : n.ToString(); }

        private void Doldur()
        {
            var d = PuanOturumu.Durum(_tur, _oyun != null ? _oyun.SiradakiFizikselDagitici() : Player.Kuzey);
            var sb = new StringBuilder();
            bool dup = _tur == PuanTuru.ChicagoDuplicate;
            sb.AppendLine(string.Format("{0,-5}{1,-14}{2,-5}{3,-5}{4,8}{5,8}  {6,8}{7,8}  {8}",
                "Bord", "Kontrat", "Sonuç", "Zon", dup ? "K-G" : "K-G üst", dup ? "D-B" : "D-B üst", dup ? "" : "K-G alt", dup ? "" : "D-B alt", "Not"));
            sb.AppendLine(new string('─', 90));
            foreach (var s in d.Satirlar)
            {
                string zon = s.NsZon && s.EwZon ? "hepsi" : s.NsZon ? "K-G" : s.EwZon ? "D-B" : "-";
                if (dup)
                    sb.AppendLine(string.Format("{0,-5}{1,-14}{2,-5}{3,-5}{4,8}{5,8}  {6,8}{7,8}  {8}",
                        s.BordNo, s.Kontrat, s.Sonuc, zon, Sut(s.NsAlt), Sut(s.EwAlt), "", "", s.Not));
                else
                    sb.AppendLine(string.Format("{0,-5}{1,-14}{2,-5}{3,-5}{4,8}{5,8}  {6,8}{7,8}  {8}",
                        s.BordNo, s.Kontrat, s.Sonuc, zon, Sut(s.NsUst), Sut(s.EwUst), Sut(s.NsAlt), Sut(s.EwAlt), s.Not));
            }
            if (d.Satirlar.Count == 0) sb.AppendLine("(Henüz oynanan bord yok)");
            sb.AppendLine(new string('─', 90));
            sb.AppendLine();
            sb.Append(d.Ozet());
            if (d.Biten > 0)
                sb.AppendLine((_tur == PuanTuru.Rubber ? "Biten rubber: " : "Biten Chicago (4 bord): ") + d.Biten);
            _kutu.Text = sb.ToString();
        }
    }
}
