using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    public partial class TestForm : Form
    {
        // Sonuç kuyruğu
        private List<TestSonucu> _sonuclar;
        private int _gecenSayisi;
        private int _kalanSayisi;

        // Sonuç gösterimi için kontroller
        private Form _sonucForm;
        private RichTextBox _txtSonuclar;
        private Label _lblOzet;

        public TestForm()
        {
            InitializeComponent();
            // Butonlar Designer'da bağlı (btnTestleriCalistir_Click, btnExit_Click)
            this.ShowInTaskbar = false;
        }

        // Designer'da butona çift tıklayınca otomatik oluşur
        private void btnTestleriCalistir_Click(object sender, EventArgs e)
        {
            BtnTestleriCalistir_Click(sender, e);
        }

        // Designer'da butona çift tıklayınca otomatik oluşur
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnTestleriCalistir_Click(object sender, EventArgs e)
        {
            // Sonuç formunu hazırla
            SonucFormunuHazirla();

            // Sayaçları sıfırla
            _sonuclar = new List<TestSonucu>();
            _gecenSayisi = 0;
            _kalanSayisi = 0;

            // Callback'i bağla
            TestRunner.SonucYaz = (mesaj) =>
            {
                if (InvokeRequired)
                    BeginInvoke(new Action(() => SonucEkle(mesaj)));
                else
                    SonucEkle(mesaj);
            };

            // Testleri çalıştır
            try
            {
                SonucEkle("════════════════════════════════════════");
                SonucEkle("  TÜM TESTLER BAŞLIYOR");
                SonucEkle("════════════════════════════════════════");
                SonucEkle("");

                TestRunner.TumTestleriCalistir();

                SonucEkle("");
                SonucEkle("════════════════════════════════════════");
                SonucEkle($"  ÖZET: ✅ {_gecenSayisi} geçti, ❌ {_kalanSayisi} başarısız");
                SonucEkle("════════════════════════════════════════");

                OzetiGuncelle();
            }
            catch (Exception ex)
            {
                SonucEkle($"❌ HATA: {ex.Message}");
            }
        }

        private void SonucEkle(string mesaj)
        {
            Color renk = Color.LightGray;
            if (mesaj.Contains("✅")) { renk = Color.LightGreen; _gecenSayisi++; }
            else if (mesaj.Contains("❌")) { renk = Color.Salmon; _kalanSayisi++; }
            else if (mesaj.Contains("═══")) renk = Color.Cyan;
            else if (mesaj.Contains("TEST")) renk = Color.Yellow;
            else if (mesaj.Contains("ÖZET")) renk = Color.White;

            _sonuclar.Add(new TestSonucu { Mesaj = mesaj, Renk = renk });

            if (_txtSonuclar != null)
            {
                _txtSonuclar.SelectionStart = _txtSonuclar.TextLength;
                _txtSonuclar.SelectionLength = 0;
                _txtSonuclar.SelectionColor = renk;
                _txtSonuclar.AppendText(mesaj + Environment.NewLine);
                _txtSonuclar.SelectionColor = _txtSonuclar.ForeColor;
                _txtSonuclar.ScrollToCaret();
                Application.DoEvents();
            }
        }

        private void SonucFormunuHazirla()
        {
            if (_sonucForm == null || _sonucForm.IsDisposed)
            {
                _sonucForm = new Form
                {
                    Text = "İhale Sistemi Test Sonuçları",
                    Size = new Size(1100, 750),
                    StartPosition = FormStartPosition.CenterScreen,
                    BackColor = Color.FromArgb(30, 30, 30),
                    ShowInTaskbar = false   // ← YENİ: Görev çubuğunda gösterme
                };

                _lblOzet = new Label
                {
                    Text = "📊 Test çalışıyor...",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(20, 15),
                    AutoSize = true
                };
                _sonucForm.Controls.Add(_lblOzet);

                _txtSonuclar = new RichTextBox
                {
                    Location = new Point(20, 55),
                    Size = new Size(1050, 600),
                    Font = new Font("Consolas", 9),
                    BackColor = Color.FromArgb(20, 20, 20),
                    ForeColor = Color.LightGray,
                    ReadOnly = true,
                    WordWrap = false,
                    ScrollBars = RichTextBoxScrollBars.Both
                };
                _sonucForm.Controls.Add(_txtSonuclar);

                var btnKapat = new Button
                {
                    Text = "✕ Kapat",
                    Location = new Point(940, 665),
                    Size = new Size(130, 35),
                    BackColor = Color.Firebrick,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnKapat.Click += (s, ev) => _sonucForm.Close();
                _sonucForm.Controls.Add(btnKapat);

                var btnKopyala = new Button
                {
                    Text = "📋 Kopyala",
                    Location = new Point(790, 665),
                    Size = new Size(130, 35),
                    BackColor = Color.DimGray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnKopyala.Click += (s, ev) =>
                {
                    if (!string.IsNullOrEmpty(_txtSonuclar.Text))
                    {
                        Clipboard.SetText(_txtSonuclar.Text);
                        MessageBox.Show("Sonuçlar panoya kopyalandı!", "Bilgi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };
                _sonucForm.Controls.Add(btnKopyala);
            }

            _txtSonuclar?.Clear();
            _sonucForm.Show();
        }

        private void OzetiGuncelle()
        {
            if (_lblOzet != null)
            {
                _lblOzet.Text = $"📊 Test Sonuçları — ✅ {_gecenSayisi} geçti, ❌ {_kalanSayisi} başarısız";
                _lblOzet.ForeColor = _kalanSayisi == 0 ? Color.LightGreen : Color.Salmon;
            }
        }

        private class TestSonucu
        {
            public string Mesaj { get; set; }
            public Color Renk { get; set; }
        }

        private void BtnTestleriCalistir_Click_1(object sender, EventArgs e)
        {
            BtnTestleriCalistir_Click(sender, e);
        }

        private void BtnExit_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TestForm_Load(object sender, EventArgs e)
        {

        }
    }
}