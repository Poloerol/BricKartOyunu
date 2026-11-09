namespace BricKartOyunu.Forms
{
    partial class DagilimDepo
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
            this.BtnAnaSayfa = new System.Windows.Forms.Button();
            this.BtnDepodanYukle = new System.Windows.Forms.Button();
            this.BtnDepoyaYukle = new System.Windows.Forms.Button();
            this.BtnDepodanSil = new System.Windows.Forms.Button();
            this.BtnDepodanDuzenle = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnAnaSayfa
            // 
            this.BtnAnaSayfa.Location = new System.Drawing.Point(12, 189);
            this.BtnAnaSayfa.Name = "BtnAnaSayfa";
            this.BtnAnaSayfa.Size = new System.Drawing.Size(159, 38);
            this.BtnAnaSayfa.TabIndex = 0;
            this.BtnAnaSayfa.Text = "Ana Sayfaya Dön";
            this.BtnAnaSayfa.UseVisualStyleBackColor = true;
            this.BtnAnaSayfa.Click += new System.EventHandler(this.button1_Click);
            // 
            // BtnDepodanYukle
            // 
            this.BtnDepodanYukle.Location = new System.Drawing.Point(12, 13);
            this.BtnDepodanYukle.Name = "BtnDepodanYukle";
            this.BtnDepodanYukle.Size = new System.Drawing.Size(159, 38);
            this.BtnDepodanYukle.TabIndex = 1;
            this.BtnDepodanYukle.Text = "Depodan İhale Dosyası Yükle";
            this.BtnDepodanYukle.UseVisualStyleBackColor = true;
            // 
            // BtnDepoyaYukle
            // 
            this.BtnDepoyaYukle.Location = new System.Drawing.Point(12, 57);
            this.BtnDepoyaYukle.Name = "BtnDepoyaYukle";
            this.BtnDepoyaYukle.Size = new System.Drawing.Size(159, 38);
            this.BtnDepoyaYukle.TabIndex = 2;
            this.BtnDepoyaYukle.Text = "Depoya İhale Dosyası Girin";
            this.BtnDepoyaYukle.UseVisualStyleBackColor = true;
            // 
            // BtnDepodanSil
            // 
            this.BtnDepodanSil.Location = new System.Drawing.Point(12, 101);
            this.BtnDepodanSil.Name = "BtnDepodanSil";
            this.BtnDepodanSil.Size = new System.Drawing.Size(159, 38);
            this.BtnDepodanSil.TabIndex = 3;
            this.BtnDepodanSil.Text = "Depodan İhale Dosyası Sil";
            this.BtnDepodanSil.UseVisualStyleBackColor = true;
            // 
            // BtnDepodanDuzenle
            // 
            this.BtnDepodanDuzenle.Location = new System.Drawing.Point(12, 145);
            this.BtnDepodanDuzenle.Name = "BtnDepodanDuzenle";
            this.BtnDepodanDuzenle.Size = new System.Drawing.Size(159, 38);
            this.BtnDepodanDuzenle.TabIndex = 4;
            this.BtnDepodanDuzenle.Text = "Depodan İhale Düzenle";
            this.BtnDepodanDuzenle.UseVisualStyleBackColor = true;
            // 
            // DagilimDepo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(184, 237);
            this.Controls.Add(this.BtnDepodanDuzenle);
            this.Controls.Add(this.BtnDepodanSil);
            this.Controls.Add(this.BtnDepoyaYukle);
            this.Controls.Add(this.BtnDepodanYukle);
            this.Controls.Add(this.BtnAnaSayfa);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DagilimDepo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Kişisel Oyun Deposu";
            this.Load += new System.EventHandler(this.DagilimDepo_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnAnaSayfa;
        private System.Windows.Forms.Button BtnDepodanYukle;
        private System.Windows.Forms.Button BtnDepoyaYukle;
        private System.Windows.Forms.Button BtnDepodanSil;
        private System.Windows.Forms.Button BtnDepodanDuzenle;
    }
}