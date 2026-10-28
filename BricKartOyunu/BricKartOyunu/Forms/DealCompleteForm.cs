using System;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// 13 el tamamlandığında açılan form.
    /// Kullanıcı yeni bord başlatabilir veya ana menüye dönebilir.
    /// </summary>
    public partial class DealCompleteForm : Form
    {
        private readonly BricOyna _bricOyna;

        public DealCompleteForm(BricOyna bricOyna)
        {
            InitializeComponent();
            _bricOyna = bricOyna;

            // 🔹 Görev çubuğunda görünmesin
            this.ShowInTaskbar = false;

            // 🔹 Küçük başlık çubuklu, yeniden boyutlandırılamayan, sadece X'li pencere.
            // Border stili değişince istemci alanı kaymasın diye boyut korunur.
            var istemciBoyutu = this.ClientSize;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ClientSize = istemciBoyutu;

            // 🔹 Konum: playZonePanel'in tam ortası (hesaplama Load'da yapılır; Manual olmalı).
            this.StartPosition = FormStartPosition.Manual;

            // X davranışını FormClosing'de yöneteceğiz
            this.FormClosing += DealCompleteForm_FormClosing;
        }

        private void DealCompleteForm_Load(object sender, EventArgs e)
        {
            // Başlık
            this.Text = "Deal Complete";

            // 🔹 playZonePanel'in tam ortasında aç (panel yoksa BricOyna'nın ortasına düş)
            PlayZoneOrtasinaKonumla();

            // ═══════════════════════════════════════════════════════════════
            // BUTON EVENT'LERİNİ BAĞLA
            // ═══════════════════════════════════════════════════════════════
            if (btnScore != null) btnScore.Click += BtnScore_Click;
            if (btnReplay != null) btnReplay.Click += BtnReplay_Click;
            if (btnReviewBidding != null) btnReviewBidding.Click += BtnReviewBidding_Click;
            if (btnReviewPlay != null) btnReviewPlay.Click += BtnReviewPlay_Click;
            if (btnSavePPL != null) btnSavePPL.Click += BtnSavePPL_Click;
            if (btnPrint != null) btnPrint.Click += BtnPrint_Click;
            if (btnNextDeal != null) btnNextDeal.Click += BtnNextDeal_Click;
            if (btnMainMenu != null) btnMainMenu.Click += BtnMainMenu_Click;

            System.Diagnostics.Debug.WriteLine(
                "[DealComplete] Buton event'leri bağlandı.");
        }

        /// <summary>Formu BricOyna.playZonePanel'in tam ortasına yerleştirir; ekran dışına taşmaz.</summary>
        private void PlayZoneOrtasinaKonumla()
        {
            try
            {
                System.Drawing.Point merkez;
                if (_bricOyna != null && _bricOyna.playZonePanel != null && !_bricOyna.playZonePanel.IsDisposed)
                {
                    var p = _bricOyna.playZonePanel;
                    merkez = p.PointToScreen(new System.Drawing.Point(p.Width / 2, p.Height / 2));
                }
                else if (_bricOyna != null)
                {
                    merkez = new System.Drawing.Point(
                        _bricOyna.Left + _bricOyna.Width / 2, _bricOyna.Top + _bricOyna.Height / 2);
                }
                else return;

                int x = merkez.X - this.Width / 2;
                int y = merkez.Y - this.Height / 2;

                // Ekranın çalışma alanı içinde kal
                var alan = Screen.FromPoint(merkez).WorkingArea;
                x = Math.Max(alan.Left, Math.Min(x, alan.Right - this.Width));
                y = Math.Max(alan.Top, Math.Min(y, alan.Bottom - this.Height));

                this.Location = new System.Drawing.Point(x, y);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[DealComplete] Konumlandırma hatası: " + ex.Message);
            }
        }

        private void DealCompleteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // X butonu → AnaSayfa'ya dön
            if (e.CloseReason == CloseReason.UserClosing &&
                this.DialogResult == DialogResult.None)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[DealComplete] X tıklandı → Return to Main Menu davranışı.");

                this.DialogResult = DialogResult.Cancel;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // BUTON TIKLAMALARI
        // ═══════════════════════════════════════════════════════════════

        private void BtnScore_Click(object sender, EventArgs e)
        {
            if (_bricOyna == null) return;
            using (var f = new DuplicateSkorForm(_bricOyna))
                f.ShowDialog(this);
        }

        private void BtnReplay_Click(object sender, EventArgs e)
        {
            // BricOyna bu sonucu görünce aynı dağılımı baştan kurar
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        private void BtnReviewBidding_Click(object sender, EventArgs e)
        {
            _bricOyna?.IhaleGecmisiniGoster();
        }

        private void BtnReviewPlay_Click(object sender, EventArgs e)
        {
            _bricOyna?.OynananElleriGoster();
        }

        /// <summary>
        /// Bordu El Kütüphanesi dosyasına (PBN, *.pbn) kaydeder. Bridge Baron Deal Library
        /// ve diğer briç programları okuyabilir. Var olan bir dosya seçilirse bord sonuna
        /// eklenir; böylece aynı dosyada birden çok bord saklanır.
        /// </summary>
        private void BtnSavePPL_Click(object sender, EventArgs e)
        {
            if (_bricOyna == null) return;

            string ad, aciklama, yorum;
            if (!KayitBilgisiSor(out ad, out aciklama, out yorum)) return;

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "El Kütüphanesi'ne Kaydet";
                dlg.Filter = "Taşınabilir Briç Notasyonu (*.pbn)|*.pbn";
                dlg.DefaultExt = "pbn";
                dlg.OverwritePrompt = false;   // var olan dosyaya ekleme yapılır
                dlg.FileName = "Eller.pbn";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    string pbn = _bricOyna.PbnUret(ad, aciklama, yorum);
                    var utf8 = new System.Text.UTF8Encoding(false);
                    bool varMi = System.IO.File.Exists(dlg.FileName) &&
                                 new System.IO.FileInfo(dlg.FileName).Length > 0;

                    // Bordlar boş satırla ayrılır
                    System.IO.File.AppendAllText(dlg.FileName,
                        (varMi ? "\r\n" : "") + pbn, utf8);

                    MessageBox.Show(
                        varMi ? "Bord dosyanın sonuna eklendi." : "Bord kaydedildi.",
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kaydedilemedi: " + ex.Message, "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Bord adı, kısa açıklama ve yorum için küçük giriş penceresi.</summary>
        private bool KayitBilgisiSor(out string ad, out string aciklama, out string yorum)
        {
            ad = aciklama = yorum = "";
            using (var f = new Form())
            {
                f.Text = "Bordu Kaydet";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.ShowInTaskbar = false;
                f.ClientSize = new System.Drawing.Size(380, 250);

                var lAd = new Label { Text = "Ad:", Left = 12, Top = 14, AutoSize = true };
                var tAd = new TextBox { Left = 12, Top = 32, Width = 356, Text = "Bord" };
                var lAc = new Label { Text = "Kısa açıklama:", Left = 12, Top = 62, AutoSize = true };
                var tAc = new TextBox { Left = 12, Top = 80, Width = 356 };
                var lYo = new Label { Text = "Yorum (bord numarasını da yazabilirsiniz):", Left = 12, Top = 110, AutoSize = true };
                var tYo = new TextBox { Left = 12, Top = 128, Width = 356, Height = 70, Multiline = true,
                                        ScrollBars = ScrollBars.Vertical };
                var ok = new Button { Text = "Kaydet", Left = 212, Top = 212, Width = 75, DialogResult = DialogResult.OK };
                var iptal = new Button { Text = "İptal", Left = 293, Top = 212, Width = 75, DialogResult = DialogResult.Cancel };
                f.Controls.AddRange(new Control[] { lAd, tAd, lAc, tAc, lYo, tYo, ok, iptal });
                f.AcceptButton = ok;
                f.CancelButton = iptal;

                if (f.ShowDialog(this) != DialogResult.OK) return false;
                ad = tAd.Text; aciklama = tAc.Text; yorum = tYo.Text;
                return true;
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (_bricOyna == null) return;

            string metin = _bricOyna.BordOzetiMetni();
            using (var doc = new System.Drawing.Printing.PrintDocument())
            using (var yazi = new System.Drawing.Font("Consolas", 10F))
            {
                doc.DocumentName = "Briç bordu";
                var satirlar = metin.Replace("\r", "").Split('\n');
                int sira = 0;

                doc.PrintPage += (s2, pe) =>
                {
                    float y = pe.MarginBounds.Top;
                    float satirYuksekligi = yazi.GetHeight(pe.Graphics);
                    while (sira < satirlar.Length && y + satirYuksekligi <= pe.MarginBounds.Bottom)
                    {
                        pe.Graphics.DrawString(satirlar[sira], yazi, System.Drawing.Brushes.Black,
                            pe.MarginBounds.Left, y);
                        y += satirYuksekligi;
                        sira++;
                    }
                    pe.HasMorePages = sira < satirlar.Length;
                };

                using (var dlg = new PrintDialog { Document = doc })
                {
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;
                    try { doc.Print(); }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Yazdırılamadı: " + ex.Message, "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnNextDeal_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                "[DealComplete] 'Go to Next Deal' tıklandı.");

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnMainMenu_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                "[DealComplete] 'Return to Main Menu' tıklandı.");

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnNextDeal_Click_1(object sender, EventArgs e)
        {

        }
    }
}