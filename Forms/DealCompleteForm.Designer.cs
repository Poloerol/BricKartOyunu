namespace BricKartOyunu.Forms
{
    partial class DealCompleteForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnScore = new System.Windows.Forms.Button();
            this.btnReplay = new System.Windows.Forms.Button();
            this.btnReviewBidding = new System.Windows.Forms.Button();
            this.btnReviewPlay = new System.Windows.Forms.Button();
            this.btnSavePPL = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnNextDeal = new System.Windows.Forms.Button();
            this.btnMainMenu = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnScore
            // 
            this.btnScore.Location = new System.Drawing.Point(12, 13);
            this.btnScore.Name = "btnScore";
            this.btnScore.Size = new System.Drawing.Size(200, 40);
            this.btnScore.TabIndex = 0;
            this.btnScore.Text = "Bu Elin Puanı";
            this.btnScore.UseVisualStyleBackColor = true;
            // 
            // btnReplay
            // 
            this.btnReplay.Location = new System.Drawing.Point(12, 52);
            this.btnReplay.Name = "btnReplay";
            this.btnReplay.Size = new System.Drawing.Size(200, 40);
            this.btnReplay.TabIndex = 1;
            this.btnReplay.Text = "Bu Eli Tekrar Oyna";
            this.btnReplay.UseVisualStyleBackColor = true;
            // 
            // btnReviewBidding
            // 
            this.btnReviewBidding.Location = new System.Drawing.Point(12, 91);
            this.btnReviewBidding.Name = "btnReviewBidding";
            this.btnReviewBidding.Size = new System.Drawing.Size(200, 40);
            this.btnReviewBidding.TabIndex = 2;
            this.btnReviewBidding.Text = "İhaleyi Göster";
            this.btnReviewBidding.UseVisualStyleBackColor = true;
            // 
            // btnReviewPlay
            // 
            this.btnReviewPlay.Location = new System.Drawing.Point(12, 130);
            this.btnReviewPlay.Name = "btnReviewPlay";
            this.btnReviewPlay.Size = new System.Drawing.Size(200, 40);
            this.btnReviewPlay.TabIndex = 3;
            this.btnReviewPlay.Text = "Oyunu Göster";
            this.btnReviewPlay.UseVisualStyleBackColor = true;
            // 
            // btnSavePPL
            // 
            this.btnSavePPL.Location = new System.Drawing.Point(12, 169);
            this.btnSavePPL.Name = "btnSavePPL";
            this.btnSavePPL.Size = new System.Drawing.Size(200, 40);
            this.btnSavePPL.TabIndex = 4;
            this.btnSavePPL.Text = "Dağılımı PPL Olarak Kaydet";
            this.btnSavePPL.UseVisualStyleBackColor = true;
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(12, 208);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(200, 40);
            this.btnPrint.TabIndex = 5;
            this.btnPrint.Text = "Bu Dağılımı Yazdır";
            this.btnPrint.UseVisualStyleBackColor = true;
            // 
            // btnNextDeal
            // 
            this.btnNextDeal.Location = new System.Drawing.Point(12, 247);
            this.btnNextDeal.Name = "btnNextDeal";
            this.btnNextDeal.Size = new System.Drawing.Size(200, 40);
            this.btnNextDeal.TabIndex = 6;
            this.btnNextDeal.Text = "Sonraki Dağılıma Geç";
            this.btnNextDeal.UseVisualStyleBackColor = true;
            // 
            // btnMainMenu
            // 
            this.btnMainMenu.Location = new System.Drawing.Point(12, 286);
            this.btnMainMenu.Name = "btnMainMenu";
            this.btnMainMenu.Size = new System.Drawing.Size(200, 40);
            this.btnMainMenu.TabIndex = 7;
            this.btnMainMenu.Text = "Anasayfa\'ya Dön";
            this.btnMainMenu.UseVisualStyleBackColor = true;
            // 
            // DealCompleteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(224, 361);
            this.Controls.Add(this.btnMainMenu);
            this.Controls.Add(this.btnNextDeal);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.btnSavePPL);
            this.Controls.Add(this.btnReviewPlay);
            this.Controls.Add(this.btnReviewBidding);
            this.Controls.Add(this.btnReplay);
            this.Controls.Add(this.btnScore);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DealCompleteForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "İhale Sonucu";
            this.Load += new System.EventHandler(this.DealCompleteForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnScore;
        private System.Windows.Forms.Button btnReplay;
        private System.Windows.Forms.Button btnReviewBidding;
        private System.Windows.Forms.Button btnReviewPlay;
        private System.Windows.Forms.Button btnSavePPL;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnNextDeal;
        private System.Windows.Forms.Button btnMainMenu;
    }
}