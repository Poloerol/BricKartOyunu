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

            // X davranışını FormClosing'de yöneteceğiz
            this.FormClosing += DealCompleteForm_FormClosing;
        }

        private void DealCompleteForm_Load(object sender, EventArgs e)
        {
            // Başlık
            this.Text = "Deal Complete";

            // Açılınca ortala
            this.StartPosition = FormStartPosition.CenterParent;

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
            MessageBox.Show(
                "Bu bordun tekrarı özelliği henüz geliştirilmedi.",
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReviewBidding_Click(object sender, EventArgs e)
        {
            _bricOyna?.IhaleGecmisiniGoster();
        }

        private void BtnReviewPlay_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Oynanan ellerin incelemesi özelliği henüz geliştirilmedi.\n" +
                "(İleride eklenecek)",
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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