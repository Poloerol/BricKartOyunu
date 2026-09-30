namespace BricKartOyunu.Forms
{
    partial class DeklarasyonForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblWest = new System.Windows.Forms.Label();
            this.lblNorth = new System.Windows.Forms.Label();
            this.lblEast = new System.Windows.Forms.Label();
            this.lblSouth = new System.Windows.Forms.Label();
            this.lstIhale = new System.Windows.Forms.ListView();
            this.colBati = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colKuzey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDogu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colGuney = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelGrid = new System.Windows.Forms.Panel();
            this.btnElDegerlendirme = new System.Windows.Forms.Button();
            this.btnInterpret = new System.Windows.Forms.Button();
            this.btnFlowcharts = new System.Windows.Forms.Button();
            this.btnHint = new System.Windows.Forms.Button();
            this.btnPass = new System.Windows.Forms.Button();
            this.btnDouble = new System.Windows.Forms.Button();
            this.btnRedouble = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblWest
            // 
            this.lblWest.AutoSize = true;
            this.lblWest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblWest.Location = new System.Drawing.Point(16, 10);
            this.lblWest.Name = "lblWest";
            this.lblWest.Size = new System.Drawing.Size(29, 13);
            this.lblWest.TabIndex = 0;
            this.lblWest.Text = "Batı";
            // 
            // lblNorth
            // 
            this.lblNorth.AutoSize = true;
            this.lblNorth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblNorth.Location = new System.Drawing.Point(62, 10);
            this.lblNorth.Name = "lblNorth";
            this.lblNorth.Size = new System.Drawing.Size(41, 13);
            this.lblNorth.TabIndex = 1;
            this.lblNorth.Text = "Kuzey";
            // 
            // lblEast
            // 
            this.lblEast.AutoSize = true;
            this.lblEast.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblEast.Location = new System.Drawing.Point(108, 10);
            this.lblEast.Name = "lblEast";
            this.lblEast.Size = new System.Drawing.Size(37, 13);
            this.lblEast.TabIndex = 2;
            this.lblEast.Text = "Doğu";
            // 
            // lblSouth
            // 
            this.lblSouth.AutoSize = true;
            this.lblSouth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblSouth.Location = new System.Drawing.Point(154, 10);
            this.lblSouth.Name = "lblSouth";
            this.lblSouth.Size = new System.Drawing.Size(43, 13);
            this.lblSouth.TabIndex = 3;
            this.lblSouth.Text = "Güney";
            // 
            // lstIhale
            // 
            this.lstIhale.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstIhale.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colBati,
            this.colKuzey,
            this.colDogu,
            this.colGuney});
            this.lstIhale.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstIhale.HideSelection = false;
            this.lstIhale.Location = new System.Drawing.Point(12, 30);
            this.lstIhale.Name = "lstIhale";
            this.lstIhale.OwnerDraw = true;
            this.lstIhale.Size = new System.Drawing.Size(185, 180);
            this.lstIhale.TabIndex = 4;
            this.lstIhale.UseCompatibleStateImageBehavior = false;
            this.lstIhale.View = System.Windows.Forms.View.Details;
            this.lstIhale.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.LstIhale_DrawSubItem);
            // 
            // colBati
            // 
            this.colBati.Text = "Batı";
            this.colBati.Width = 44;
            // 
            // colKuzey
            // 
            this.colKuzey.Text = "Kuzey";
            this.colKuzey.Width = 46;
            // 
            // colDogu
            // 
            this.colDogu.Text = "Doğu";
            this.colDogu.Width = 44;
            // 
            // colGuney
            // 
            this.colGuney.Text = "Güney";
            this.colGuney.Width = 46;
            // 
            // panelGrid
            // 
            this.panelGrid.Location = new System.Drawing.Point(205, 28);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Size = new System.Drawing.Size(155, 182);
            this.panelGrid.TabIndex = 5;
            this.panelGrid.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelGrid_Paint);
            this.panelGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PanelGrid_MouseClick);
            // 
            // btnElDegerlendirme
            // 
            this.btnElDegerlendirme.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnElDegerlendirme.Location = new System.Drawing.Point(12, 218);
            this.btnElDegerlendirme.Name = "btnElDegerlendirme";
            this.btnElDegerlendirme.Size = new System.Drawing.Size(110, 32);
            this.btnElDegerlendirme.TabIndex = 6;
            this.btnElDegerlendirme.Text = "El Değerlendirme";
            this.btnElDegerlendirme.UseVisualStyleBackColor = true;
            this.btnElDegerlendirme.Click += new System.EventHandler(this.BtnElDegerlendirme_Click);
            // 
            // btnInterpret
            // 
            this.btnInterpret.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnInterpret.Location = new System.Drawing.Point(128, 218);
            this.btnInterpret.Name = "btnInterpret";
            this.btnInterpret.Size = new System.Drawing.Size(110, 32);
            this.btnInterpret.TabIndex = 7;
            this.btnInterpret.Text = "İhale Analizi";
            this.btnInterpret.UseVisualStyleBackColor = true;
            this.btnInterpret.Click += new System.EventHandler(this.BtnInterpret_Click);
            // 
            // btnFlowcharts
            // 
            this.btnFlowcharts.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnFlowcharts.Location = new System.Drawing.Point(12, 256);
            this.btnFlowcharts.Name = "btnFlowcharts";
            this.btnFlowcharts.Size = new System.Drawing.Size(110, 32);
            this.btnFlowcharts.TabIndex = 8;
            this.btnFlowcharts.Text = "Akış Şeması";
            this.btnFlowcharts.UseVisualStyleBackColor = true;
            this.btnFlowcharts.Click += new System.EventHandler(this.BtnFlowcharts_Click);
            // 
            // btnHint
            // 
            this.btnHint.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnHint.Location = new System.Drawing.Point(128, 256);
            this.btnHint.Name = "btnHint";
            this.btnHint.Size = new System.Drawing.Size(110, 32);
            this.btnHint.TabIndex = 9;
            this.btnHint.Text = "İpucu";
            this.btnHint.UseVisualStyleBackColor = true;
            this.btnHint.Click += new System.EventHandler(this.BtnHint_Click);
            // 
            // btnPass
            // 
            this.btnPass.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnPass.Location = new System.Drawing.Point(244, 218);
            this.btnPass.Name = "btnPass";
            this.btnPass.Size = new System.Drawing.Size(116, 32);
            this.btnPass.TabIndex = 10;
            this.btnPass.Text = "Pas";
            this.btnPass.UseVisualStyleBackColor = true;
            this.btnPass.Click += new System.EventHandler(this.BtnPass_Click);
            // 
            // btnDouble
            // 
            this.btnDouble.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnDouble.Location = new System.Drawing.Point(244, 256);
            this.btnDouble.Name = "btnDouble";
            this.btnDouble.Size = new System.Drawing.Size(56, 32);
            this.btnDouble.TabIndex = 11;
            this.btnDouble.Text = "Kontr";
            this.btnDouble.UseVisualStyleBackColor = true;
            this.btnDouble.Click += new System.EventHandler(this.BtnDouble_Click);
            // 
            // btnRedouble
            // 
            this.btnRedouble.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnRedouble.Location = new System.Drawing.Point(304, 256);
            this.btnRedouble.Name = "btnRedouble";
            this.btnRedouble.Size = new System.Drawing.Size(56, 32);
            this.btnRedouble.TabIndex = 12;
            this.btnRedouble.Text = "S.Kontr";
            this.btnRedouble.UseVisualStyleBackColor = true;
            this.btnRedouble.Click += new System.EventHandler(this.BtnRedouble_Click);
            // 
            // DeklarasyonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 300);
            this.Controls.Add(this.btnRedouble);
            this.Controls.Add(this.btnDouble);
            this.Controls.Add(this.btnPass);
            this.Controls.Add(this.btnHint);
            this.Controls.Add(this.btnFlowcharts);
            this.Controls.Add(this.btnInterpret);
            this.Controls.Add(this.btnElDegerlendirme);
            this.Controls.Add(this.panelGrid);
            this.Controls.Add(this.lstIhale);
            this.Controls.Add(this.lblSouth);
            this.Controls.Add(this.lblEast);
            this.Controls.Add(this.lblNorth);
            this.Controls.Add(this.lblWest);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeklarasyonForm";
            this.ShowInTaskbar = false;
            this.Text = "Deklarasyon";
            this.Load += new System.EventHandler(this.DeklarasyonForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblWest;
        private System.Windows.Forms.Label lblNorth;
        private System.Windows.Forms.Label lblEast;
        private System.Windows.Forms.Label lblSouth;
        private System.Windows.Forms.ListView lstIhale;
        private System.Windows.Forms.ColumnHeader colBati;
        private System.Windows.Forms.ColumnHeader colKuzey;
        private System.Windows.Forms.ColumnHeader colDogu;
        private System.Windows.Forms.ColumnHeader colGuney;
        private System.Windows.Forms.Panel panelGrid;
        private System.Windows.Forms.Button btnElDegerlendirme;
        private System.Windows.Forms.Button btnInterpret;
        private System.Windows.Forms.Button btnFlowcharts;
        private System.Windows.Forms.Button btnHint;
        private System.Windows.Forms.Button btnPass;
        private System.Windows.Forms.Button btnDouble;
        private System.Windows.Forms.Button btnRedouble;
    }
}