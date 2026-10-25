using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    public partial class IhaleYorumuForm : Form
    {
        private readonly List<string> _teklifler;
        private readonly string _baslangicOyuncusu;
        private readonly List<string> _yorumlar;

        public IhaleYorumuForm(List<string> teklifler, string baslangicOyuncusu, List<string> yorumlar = null)
        {
            InitializeComponent();

            _teklifler = teklifler ?? new List<string>();
            _baslangicOyuncusu = baslangicOyuncusu ?? "Kuzey";
            _yorumlar = yorumlar ?? new List<string>();

            // Form Ayarları (Boyu 10 piksel uzatıldı)
            this.Text = "İhale Yorumu";
            this.Size = new Size(420, 460);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(235, 235, 235);

            ArayuzuOlustur();
        }

        private void ArayuzuOlustur()
        {
            // İhale Geçmişini Hesapla
            string kuzey = "", dogu = "", guney = "", bati = "";
            var kuzeyYorum = new List<string>(); var doguYorum = new List<string>(); var guneyYorum = new List<string>(); var batiYorum = new List<string>();

            string[] yonler = { "Kuzey", "Dogu", "Guney", "Bati" };
            int mevcutYonIdx = Array.IndexOf(yonler, _baslangicOyuncusu);
            if (mevcutYonIdx < 0) mevcutYonIdx = 0;

            for (int i = 0; i < _teklifler.Count; i++)
            {
                string teklif = _teklifler[i];
                string yorum = i < _yorumlar.Count ? _yorumlar[i] : "Naturel";

                switch (yonler[mevcutYonIdx])
                {
                    case "Kuzey": kuzey += teklif + "\n"; kuzeyYorum.Add(yorum); break;
                    case "Dogu": dogu += teklif + "\n"; doguYorum.Add(yorum); break;
                    case "Guney": guney += teklif + "\n"; guneyYorum.Add(yorum); break;
                    case "Bati": bati += teklif + "\n"; batiYorum.Add(yorum); break;
                }
                mevcutYonIdx = (mevcutYonIdx + 1) % 4;
            }

            // ─── Kutuları Yerleştir ───
            // Kuzey (Üst kutu) — kuzey verisi
            EkleKutu("Kuzey", kuzey, kuzeyYorum, 110, 25, 200, 60);

            // Batı (Sol alt kutu) — bati verisi
            EkleKutu("Batı", bati, batiYorum, 10, 135, 190, 60);

            // Doğu (Sağ alt kutu) — dogu verisi
            EkleKutu("Doğu", dogu, doguYorum, 210, 135, 190, 60);

            // Güney (En alt orta kutu) — guney verisi
            EkleKutu("Güney", guney, guneyYorum, 110, 245, 200, 60);

            // OK Butonu
            Button btnOk = new Button
            {
                Text = "OK",
                Location = new Point(165, 350),
                Size = new Size(80, 25)
            };
            btnOk.Click += (s, e) => this.Close();
            this.Controls.Add(btnOk);
        }

        private void EkleKutu(string yonAdi, string teklifler, List<string> yorumlar, int x, int y, int genislik, int yukseklik)
        {
            // Yön Etiketi
            Label lblYon = new Label
            {
                Text = yonAdi,
                Location = new Point(x + (genislik / 2) - 20, y - 18),
                AutoSize = true,
                Font = new Font("Arial", 8.5F, FontStyle.Regular)
            };
            this.Controls.Add(lblYon);

            // Liste Kutusu
            ListBox lst = new ListBox
            {
                Location = new Point(x, y),
                Size = new Size(genislik, yukseklik),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Arial", 8.5F, FontStyle.Regular),
                SelectionMode = SelectionMode.One
            };

            if (!string.IsNullOrWhiteSpace(teklifler))
            {
                foreach (var t in teklifler.Trim().Split('\n'))
                {
                    lst.Items.Add(t.Trim());
                }
            }
            this.Controls.Add(lst);

            // Yorum Etiketi
            Label lblYorum = new Label
            {
                Text = yorumlar.Count > 0 ? yorumlar[yorumlar.Count - 1] : "",
                Location = new Point(x, y + yukseklik + 2),
                AutoSize = true,
                Font = new Font("Arial", 8F, FontStyle.Regular),
                ForeColor = Color.DimGray
            };
            this.Controls.Add(lblYorum);

            // Seçilen teklifin yorumunu göster (varsayılan: bu oyuncunun son teklifi)
            if (lst.Items.Count > 0) lst.SelectedIndex = lst.Items.Count - 1;
            lst.SelectedIndexChanged += (s, e) =>
            {
                int i = lst.SelectedIndex;
                lblYorum.Text = (i >= 0 && i < yorumlar.Count) ? yorumlar[i] : "";
            };
        }

        private void IhaleYorumuForm_Load(object sender, EventArgs e)
        {

        }
    }
}