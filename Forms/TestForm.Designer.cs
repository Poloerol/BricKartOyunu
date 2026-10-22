namespace BricKartOyunu.Forms
{
    partial class TestForm
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
            this.BtnExit = new System.Windows.Forms.Button();
            this.BtnTestleriCalistir = new System.Windows.Forms.Button();
            this.BtnOneClubTestleri = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnExit
            // 
            this.BtnExit.Location = new System.Drawing.Point(30, 500);
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Size = new System.Drawing.Size(116, 39);
            this.BtnExit.TabIndex = 87;
            this.BtnExit.Text = "Exit";
            this.BtnExit.UseVisualStyleBackColor = true;
            this.BtnExit.Click += new System.EventHandler(this.BtnExit_Click_1);
            // 
            // BtnTestleriCalistir
            // 
            this.BtnTestleriCalistir.Location = new System.Drawing.Point(12, 12);
            this.BtnTestleriCalistir.Name = "BtnTestleriCalistir";
            this.BtnTestleriCalistir.Size = new System.Drawing.Size(200, 40);
            this.BtnTestleriCalistir.TabIndex = 88;
            this.BtnTestleriCalistir.Text = "Testleri Çalıştır";
            this.BtnTestleriCalistir.UseVisualStyleBackColor = true;
            this.BtnTestleriCalistir.Click += new System.EventHandler(this.BtnTestleriCalistir_Click_1);
            // 
            // BtnOneClubTestleri
            // 
            this.BtnOneClubTestleri.Location = new System.Drawing.Point(12, 58);
            this.BtnOneClubTestleri.Name = "BtnOneClubTestleri";
            this.BtnOneClubTestleri.Size = new System.Drawing.Size(200, 40);
            this.BtnOneClubTestleri.TabIndex = 89;
            this.BtnOneClubTestleri.Text = "1♣ Testleri";
            this.BtnOneClubTestleri.UseVisualStyleBackColor = true;
            this.BtnOneClubTestleri.Click += new System.EventHandler(this.BtnOneClubTestleri_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1354, 671);
            this.Controls.Add(this.BtnOneClubTestleri);
            this.Controls.Add(this.BtnTestleriCalistir);
            this.Controls.Add(this.BtnExit);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.TestForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button BtnExit;
        private System.Windows.Forms.Button BtnTestleriCalistir;
        private System.Windows.Forms.Button BtnOneClubTestleri;
    }
}