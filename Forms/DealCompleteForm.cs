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
            MessageBox.Show(
                "Skor hesaplama özelliği henüz geliştirilmedi.",
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void BtnSavePPL_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "PPL dosyasına kaydetme özelliği henüz geliştirilmedi.",
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Yazdırma özelliği henüz geliştirilmedi.",
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}