namespace BricKartOyunu.Forms
{
    partial class DekBasForm
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
            this.BtnDeklarasyon = new System.Windows.Forms.Button();
            this.BtnKontratAyar = new System.Windows.Forms.Button();
            this.BtnElDegistir = new System.Windows.Forms.Button();
            this.BtnSonrakiDagilim = new System.Windows.Forms.Button();
            this.BtnOyundanCik = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnDeklarasyon
            // 
            this.BtnDeklarasyon.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BtnDeklarasyon.Location = new System.Drawing.Point(24, 13);
            this.BtnDeklarasyon.Name = "BtnDeklarasyon";
            this.BtnDeklarasyon.Size = new System.Drawing.Size(121, 33);
            this.BtnDeklarasyon.TabIndex = 0;
            this.BtnDeklarasyon.Text = "Deklarasyon";
            this.BtnDeklarasyon.UseVisualStyleBackColor = true;
            this.BtnDeklarasyon.Click += new System.EventHandler(this.BtnDeklarasyon_Click);
            // 
            // BtnKontratAyar
            // 
            this.BtnKontratAyar.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BtnKontratAyar.Location = new System.Drawing.Point(24, 52);
            this.BtnKontratAyar.Name = "BtnKontratAyar";
            this.BtnKontratAyar.Size = new System.Drawing.Size(121, 33);
            this.BtnKontratAyar.TabIndex = 1;
            this.BtnKontratAyar.Text = "Kontratı Ayarla";
            this.BtnKontratAyar.UseVisualStyleBackColor = true;
            // 
            // BtnElDegistir
            // 
            this.BtnElDegistir.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BtnElDegistir.Location = new System.Drawing.Point(24, 91);
            this.BtnElDegistir.Name = "BtnElDegistir";
            this.BtnElDegistir.Size = new System.Drawing.Size(121, 33);
            this.BtnElDegistir.TabIndex = 2;
            this.BtnElDegistir.Text = "Elleri Değiştir";
            this.BtnElDegistir.UseVisualStyleBackColor = true;
            this.BtnElDegistir.Click += new System.EventHandler(this.BtnElDegistir_Click);
            // 
            // BtnSonrakiDagilim
            // 
            this.BtnSonrakiDagilim.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BtnSonrakiDagilim.Location = new System.Drawing.Point(24, 130);
            this.BtnSonrakiDagilim.Name = "BtnSonrakiDagilim";
            this.BtnSonrakiDagilim.Size = new System.Drawing.Size(121, 33);
            this.BtnSonrakiDagilim.TabIndex = 3;
            this.BtnSonrakiDagilim.Text = "Sonraki Dağılım";
            this.BtnSonrakiDagilim.UseVisualStyleBackColor = true;
            this.BtnSonrakiDagilim.Click += new System.EventHandler(this.BtnSonrakiDagilim_Click);
            // 
            // BtnOyundanCik
            // 
            this.BtnOyundanCik.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BtnOyundanCik.Location = new System.Drawing.Point(24, 169);
            this.BtnOyundanCik.Name = "BtnOyundanCik";
            this.BtnOyundanCik.Size = new System.Drawing.Size(121, 33);
            this.BtnOyundanCik.TabIndex = 4;
            this.BtnOyundanCik.Text = "Oyundan Çık";
            this.BtnOyundanCik.UseVisualStyleBackColor = true;
            this.BtnOyundanCik.Click += new System.EventHandler(this.BtnOyundanCik_Click);
            // 
            // DekBasForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(172, 219);
            this.Controls.Add(this.BtnOyundanCik);
            this.Controls.Add(this.BtnSonrakiDagilim);
            this.Controls.Add(this.BtnElDegistir);
            this.Controls.Add(this.BtnKontratAyar);
            this.Controls.Add(this.BtnDeklarasyon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DekBasForm";
            this.ShowInTaskbar = false;
            this.Text = "Deklarasyon Başlangıcı";
            this.Load += new System.EventHandler(this.DekBasForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnDeklarasyon;
        private System.Windows.Forms.Button BtnKontratAyar;
        private System.Windows.Forms.Button BtnElDegistir;
        private System.Windows.Forms.Button BtnSonrakiDagilim;
        private System.Windows.Forms.Button BtnOyundanCik;
    }
}