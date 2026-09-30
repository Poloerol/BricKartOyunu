using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    public partial class IhaleGecmisiForm : Form
    {
        private readonly ListView lstGecmis;

        public IhaleGecmisiForm(List<string> teklifler, string baslangicOyuncusu)
        {
            InitializeComponent();

            this.Text = "İhale Geçmişi";
            this.Size = new Size(200, 230);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 🔹 Formun görev çubuğunda görünmesini engeller
            this.ShowInTaskbar = false;

            // ListView boyutlandırması
            lstGecmis = new ListView
            {
                Location = new Point(6, 6),
                Size = new Size(172, 178),
                View = View.Details,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                OwnerDraw = true,
                BorderStyle = BorderStyle.FixedSingle,
                GridLines = false
            };

            // Kolon genişlikleri "Güney" kelimesinin sığması için 41 piksel yapıldı
            lstGecmis.Columns.Add("Batı", 41, HorizontalAlignment.Center);
            lstGecmis.Columns.Add("Kuzey", 41, HorizontalAlignment.Center);
            lstGecmis.Columns.Add("Doğu", 41, HorizontalAlignment.Center);
            lstGecmis.Columns.Add("Güney", 41, HorizontalAlignment.Center);

            lstGecmis.DrawColumnHeader += LstGecmis_DrawColumnHeader;
            lstGecmis.DrawSubItem += LstGecmis_DrawSubItem;

            this.Controls.Add(lstGecmis);

            GecmisiYukle(teklifler, baslangicOyuncusu);
        }

        private void GecmisiYukle(List<string> teklifler, string baslangicOyuncusu)
        {
            if (teklifler == null || teklifler.Count == 0) return;

            int baslangicSutun = 0;
            switch (baslangicOyuncusu)
            {
                case "Bati": case "Batı": baslangicSutun = 0; break;
                case "Kuzey": baslangicSutun = 1; break;
                case "Dogu": case "Doğu": baslangicSutun = 2; break;
                case "Guney": case "Güney": baslangicSutun = 3; break;
            }

            int mevcutSutun = baslangicSutun;
            ListViewItem satir = new ListViewItem(new string[] { "", "", "", "" });

            foreach (string teklif in teklifler)
            {
                satir.SubItems[mevcutSutun].Text = teklif;
                mevcutSutun++;

                if (mevcutSutun > 3)
                {
                    lstGecmis.Items.Add(satir);
                    satir = new ListViewItem(new string[] { "", "", "", "" });
                    mevcutSutun = 0;
                }
            }

            if (mevcutSutun > 0)
            {
                lstGecmis.Items.Add(satir);
            }
        }

        private void LstGecmis_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawBackground();
            using (Font headerFont = new Font("Arial", 8.5F, FontStyle.Regular))
            using (StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            })
            {
                e.Graphics.DrawString(e.Header.Text, headerFont, Brushes.Black, e.Bounds, sf);
            }
        }

        private void LstGecmis_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();
            string text = e.SubItem.Text;
            if (string.IsNullOrEmpty(text)) return;

            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Font font = new Font("Arial", 8.5F, FontStyle.Bold);

            if (text.Contains("♦") || text.Contains("♥"))
            {
                string seviye = text.Substring(0, text.Length - 1);
                string simge = text.Substring(text.Length - 1);

                SizeF levelSize = g.MeasureString(seviye, font);
                int startX = e.Bounds.X + (e.Bounds.Width - (int)levelSize.Width - 10) / 2;

                using (SolidBrush blackBrush = new SolidBrush(Color.Black))
                {
                    g.DrawString(seviye, font, blackBrush, startX, e.Bounds.Y + 2);
                }

                using (SolidBrush redBrush = new SolidBrush(Color.FromArgb(220, 0, 0)))
                {
                    g.DrawString(simge, font, redBrush, startX + (int)levelSize.Width - 3, e.Bounds.Y + 2);
                }
            }
            else
            {
                using (StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                })
                using (SolidBrush blackBrush = new SolidBrush(Color.Black))
                {
                    g.DrawString(text, font, blackBrush, e.Bounds, sf);
                }
            }
        }

        private void IhaleGecmisiForm_Load(object sender, EventArgs e)
        {

        }
    }
}