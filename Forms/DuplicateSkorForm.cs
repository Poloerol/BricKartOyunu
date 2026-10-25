using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Duplicate skor penceresi: 1. masa sonucu (kontrat, löve puanı, fazla/eksik puan, bonus),
    /// "Sizin skor" (Kuzey-Güney açısından) ve oturum toplamı. 2. masa (rakip) alanları
    /// ileride karşılaştırma için boş bırakılmıştır.
    /// </summary>
    public class DuplicateSkorForm : Form
    {
        // Oturum boyunca bord numarası → K-G skoru (aynı bord iki kez sayılmaz)
        private static readonly Dictionary<int, int> _oturumSkorlari = new Dictionary<int, int>();

        private readonly BricOyna _oyun;
        private readonly Font _yazi = new Font("Microsoft Sans Serif", 12F);
        private readonly Font _kalin = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);

        private Label _lblKontrat1, _lblLove1, _lblFazla1, _lblBonus1;
        private Label _lblSizinSkor, _lblToplamSizin, _lblToplamRakip;
        private Button _btnTamam, _btnTemizle, _btnYazdir;
        private Panel _icerik;

        public DuplicateSkorForm(BricOyna oyun)
        {
            _oyun = oyun;
            KurulumYap();
            Doldur();
        }

        private void KurulumYap()
        {
            Text = "Duplicate Skor";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(590, 460);
            BackColor = SystemColors.Control;
            Font = _yazi;

            _icerik = new Panel { Left = 0, Top = 0, Width = 590, Height = 395, BackColor = SystemColors.Control };
            Controls.Add(_icerik);

            // Başlıklar ve çizgiler
            Baslik("1. Masa Sonucu", 15, 10, 270);
            Baslik("2. Masa Sonucu", 305, 10, 270);

            // Sol sütun etiketleri / değerleri
            _lblKontrat1 = SatirEkle("Kontrat:", 15, 55, 125, 140);
            _lblLove1 = SatirEkle("Löve puanı:", 15, 90, 125, 140);
            _lblFazla1 = SatirEkle("Fazla/Eksik:", 15, 125, 125, 140);
            _lblBonus1 = SatirEkle("Bonus:", 15, 160, 125, 140);

            // Sağ sütun (2. masa) — boş
            SatirEkle("Kontrat:", 305, 55, 125, 140);
            SatirEkle("Löve puanı:", 305, 90, 125, 140);
            SatirEkle("Fazla/Eksik:", 305, 125, 125, 140);
            SatirEkle("Bonus:", 305, 160, 125, 140);

            // Skor tablosu
            int x0 = 190, kolon = 135, y0 = 205;
            _icerik.Controls.Add(Hucre("Sizin Skor", x0, y0, kolon, ContentAlignment.MiddleCenter));
            _icerik.Controls.Add(Hucre("Rakip Skor", x0 + kolon, y0, kolon, ContentAlignment.MiddleCenter));

            var cizgiUst = new Panel { Left = x0, Top = y0 + 26, Width = kolon * 2, Height = 1, BackColor = Color.Gray };
            _icerik.Controls.Add(cizgiUst);

            _lblSizinSkor = Hucre("", x0, y0 + 32, kolon, ContentAlignment.MiddleCenter);
            _icerik.Controls.Add(_lblSizinSkor);
            _icerik.Controls.Add(Hucre("", x0 + kolon, y0 + 32, kolon, ContentAlignment.MiddleCenter));

            _icerik.Controls.Add(Etiket("Fark", 70, y0 + 66, 110, ContentAlignment.MiddleRight));
            _icerik.Controls.Add(Etiket("IMP", 70, y0 + 100, 110, ContentAlignment.MiddleRight));
            _icerik.Controls.Add(Etiket("Toplam", 70, y0 + 134, 110, ContentAlignment.MiddleRight));

            _lblToplamSizin = Hucre("0", x0, y0 + 134, kolon, ContentAlignment.MiddleCenter);
            _lblToplamRakip = Hucre("0", x0 + kolon, y0 + 134, kolon, ContentAlignment.MiddleCenter);
            _icerik.Controls.Add(_lblToplamSizin);
            _icerik.Controls.Add(_lblToplamRakip);

            // Izgara çizgileri (Fark, IMP, Toplam satırlarının üstü + orta dikey çizgi)
            foreach (int dy in new[] { 62, 96, 130 })
                _icerik.Controls.Add(new Panel { Left = x0, Top = y0 + dy, Width = kolon * 2, Height = 1, BackColor = Color.Gray });
            _icerik.Controls.Add(new Panel { Left = x0 + kolon, Top = y0, Width = 1, Height = 164, BackColor = Color.Gray });

            // Düğmeler
            _btnTamam = DugmeEkle("Tamam", 40, 408, 150);
            _btnTemizle = DugmeEkle("Temizle", 220, 408, 150);
            _btnYazdir = DugmeEkle("PDF Kaydet", 400, 408, 150);
            _btnTamam.DialogResult = DialogResult.OK;
            AcceptButton = _btnTamam;
            CancelButton = _btnTamam;
            _btnTemizle.Click += (s, e) => { _oturumSkorlari.Clear(); Doldur(); };
            _btnYazdir.Click += BtnYazdir_Click;
        }

        private void Baslik(string metin, int x, int y, int genislik)
        {
            _icerik.Controls.Add(Etiket(metin, x, y, genislik, ContentAlignment.MiddleCenter));
            _icerik.Controls.Add(new Panel { Left = x, Top = y + 28, Width = genislik, Height = 1, BackColor = Color.Gray });
        }

        private Label Etiket(string metin, int x, int y, int w, ContentAlignment hiza)
        {
            return new Label
            {
                Text = metin, Left = x, Top = y, Width = w, Height = 26,
                TextAlign = hiza, AutoSize = false, Font = _yazi
            };
        }

        private Label Hucre(string metin, int x, int y, int w, ContentAlignment hiza)
        {
            return Etiket(metin, x, y, w, hiza);
        }

        /// <summary>Sol etiket (sağa yaslı) + değer etiketi ekler; değer etiketini döndürür.</summary>
        private Label SatirEkle(string etiket, int x, int y, int etiketGenislik, int degerGenislik)
        {
            _icerik.Controls.Add(Etiket(etiket, x, y, etiketGenislik, ContentAlignment.MiddleRight));
            var deger = Etiket("", x + etiketGenislik + 8, y, degerGenislik, ContentAlignment.MiddleLeft);
            _icerik.Controls.Add(deger);
            return deger;
        }

        private Button DugmeEkle(string metin, int x, int y, int w)
        {
            var b = new Button
            {
                Text = metin, Left = x, Top = y, Width = w, Height = 38,
                Font = _yazi, UseVisualStyleBackColor = true
            };
            Controls.Add(b);
            return b;
        }

        private static string Isaretli(int n)
        {
            return n > 0 ? "+" + n : n.ToString();
        }

        private void Doldur()
        {
            SkorSonucu s = _oyun != null ? _oyun.SkorHesapla() : null;
            int bord = _oyun != null ? _oyun.BordNo : 0;

            if (s == null || s.PasGecildi)
            {
                _lblKontrat1.Text = "Dört pas";
                _lblLove1.Text = "0";
                _lblFazla1.Text = "0";
                _lblBonus1.Text = "0";
                _lblSizinSkor.Text = "0";
                if (bord > 0) _oturumSkorlari[bord] = 0;
            }
            else
            {
                Player dek = _oyun.KontratDeklaran;
                bool deklaranNS = dek == Player.Kuzey || dek == Player.Guney;

                _lblKontrat1.Text = s.KontratMetni + " — " + BricOyna.PlayerIsmiTurkce(dek);
                _lblLove1.Text = s.Yapildi ? s.Anlasma.ToString() : "0";
                _lblFazla1.Text = s.Yapildi ? s.FazlaLove.ToString() : (-s.Ceza).ToString();
                _lblBonus1.Text = s.Yapildi
                    ? (s.OyunBonusu + s.SlamBonusu + s.KontrIkrami).ToString()
                    : "0";

                int ns = deklaranNS ? s.Toplam : -s.Toplam;   // "Sizin" taraf = Kuzey-Güney
                _lblSizinSkor.Text = Isaretli(ns);
                if (bord > 0) _oturumSkorlari[bord] = ns;
            }

            _lblToplamSizin.Text = _oturumSkorlari.Values.Sum().ToString();
            _lblToplamRakip.Text = "0";
        }

        private void BtnYazdir_Click(object sender, EventArgs e)
        {
            int bord = _oyun != null ? _oyun.BordNo : 0;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Skoru PDF olarak kaydet";
                dlg.Filter = "PDF dosyası (*.pdf)|*.pdf";
                dlg.DefaultExt = "pdf";
                dlg.AddExtension = true;
                dlg.FileName = "Skor_Bord" + bord + ".pdf";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    using (var bmp = new Bitmap(_icerik.Width, _icerik.Height))
                    {
                        _icerik.DrawToBitmap(bmp, new Rectangle(0, 0, _icerik.Width, _icerik.Height));
                        PdfYaz(dlg.FileName, bmp);
                    }
                    try { System.Diagnostics.Process.Start(dlg.FileName); } catch { }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PDF kaydedilemedi: " + ex.Message, "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Resmi A4 sayfaya yerleştirip dış kütüphane kullanmadan PDF yazar.</summary>
        private static void PdfYaz(string yol, Bitmap bmp)
        {
            byte[] jpg;
            using (var ms = new System.IO.MemoryStream())
            {
                var kodlayici = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                    .First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                using (var ayar = new System.Drawing.Imaging.EncoderParameters(1))
                {
                    ayar.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                        System.Drawing.Imaging.Encoder.Quality, 95L);
                    bmp.Save(ms, kodlayici, ayar);
                }
                jpg = ms.ToArray();
            }

            var ci = System.Globalization.CultureInfo.InvariantCulture;
            double genislik = 480.0;
            double yukseklik = genislik * bmp.Height / bmp.Width;
            double x = (595.0 - genislik) / 2.0;
            double y = 790.0 - yukseklik;
            string icerik = string.Format(ci, "q {0:0.##} 0 0 {1:0.##} {2:0.##} {3:0.##} cm /Im0 Do Q",
                genislik, yukseklik, x, y);

            var ascii = System.Text.Encoding.ASCII;
            var offsets = new List<long>();

            using (var fs = new System.IO.FileStream(yol, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                Action<string> yaz = t => { var b = ascii.GetBytes(t); fs.Write(b, 0, b.Length); };
                Action<string> nesneAc = t => { offsets.Add(fs.Position); yaz(t); };

                yaz("%PDF-1.4\n");
                nesneAc("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
                nesneAc("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
                nesneAc("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] " +
                        "/Resources << /XObject << /Im0 4 0 R >> >> /Contents 5 0 R >>\nendobj\n");

                nesneAc(string.Format(ci,
                    "4 0 obj\n<< /Type /XObject /Subtype /Image /Width {0} /Height {1} " +
                    "/ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {2} >>\nstream\n",
                    bmp.Width, bmp.Height, jpg.Length));
                fs.Write(jpg, 0, jpg.Length);
                yaz("\nendstream\nendobj\n");

                nesneAc(string.Format(ci, "5 0 obj\n<< /Length {0} >>\nstream\n{1}\nendstream\nendobj\n",
                    icerik.Length, icerik));

                long xref = fs.Position;
                yaz("xref\n0 6\n0000000000 65535 f \n");
                foreach (long o in offsets)
                    yaz(o.ToString("D10", ci) + " 00000 n \n");
                yaz(string.Format(ci, "trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{0}\n%%EOF\n", xref));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _yazi.Dispose();
                _kalin.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
