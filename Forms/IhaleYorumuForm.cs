using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// İhale Yorumu: her koltuğun ihaleden çıkarılan eli (toplam puan, HCP, renk uzunlukları, en iyi bilinen uyum)
    /// ve son çağrısının anlamı. Ayrıntılı çağrı listesi için İhale Geçmişi formu vardır.
    /// </summary>
    public partial class IhaleYorumuForm : Form
    {
        private List<string> _teklifler;
        private string _baslangicOyuncusu;
        private readonly Func<Player, OrtaklikAnlasmasi> _anlasmaOf;

        /// <param name="teklifler">İhale çağrıları sırayla ("1♥", "Pas", ...).</param>
        /// <param name="baslangicOyuncusu">İlk çağrıyı yapan koltuk ("Bati", "Kuzey", "Dogu", "Guney").</param>
        /// <param name="yorumlar">Eski parametre; artık kullanılmıyor (yorum ihaleden hesaplanır).</param>
        /// <param name="anlasmaOf">Koltuğun taraf anlaşması; null ise kayıtlı etkin anlaşma kullanılır.</param>
        public IhaleYorumuForm(List<string> teklifler, string baslangicOyuncusu, List<string> yorumlar = null,
                               Func<Player, OrtaklikAnlasmasi> anlasmaOf = null)
        {
            InitializeComponent();

            _teklifler = teklifler ?? new List<string>();
            _baslangicOyuncusu = baslangicOyuncusu ?? "Kuzey";
            _anlasmaOf = anlasmaOf ?? (k => AnlasmaYonetici.Mevcut(AnlasmaYonetici.TarafOf(k)));

            this.Text = "İhale Yorumu";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(410, 450);
            this.BackColor = Color.FromArgb(235, 235, 235);

            ArayuzuOlustur();
            this.Load += (s, e) => EkranaSigdir();
        }

        /// <summary>Açık formu güncel ihale ile yeniler (yeni çağrı / geri al).</summary>
        public void Guncelle(List<string> teklifler, string baslangicOyuncusu)
        {
            _teklifler = teklifler ?? new List<string>();
            _baslangicOyuncusu = baslangicOyuncusu ?? _baslangicOyuncusu;
            ArayuzuOlustur();
        }

        // Küçük ekranlarda (ör. 1366x768) form çalışma alanının dışına taşmasın
        private void EkranaSigdir()
        {
            try
            {
                var wa = Screen.FromControl(this).WorkingArea;
                int x = Math.Min(Math.Max(this.Left, wa.Left), Math.Max(wa.Left, wa.Right - this.Width));
                int y = Math.Min(Math.Max(this.Top, wa.Top), Math.Max(wa.Top, wa.Bottom - this.Height));
                this.Location = new Point(x, y);
            }
            catch { }
        }

        private void ArayuzuOlustur()
        {
            this.SuspendLayout();
            foreach (Control c in new List<Control>(this.Controls.Cast<Control>())) { this.Controls.Remove(c); c.Dispose(); }

            var analiz = IhaleAnalizi.Analiz(_teklifler, _baslangicOyuncusu, _anlasmaOf);

            EkleKutu(Player.Kuzey, analiz, 105, 26, 200, 88);
            EkleKutu(Player.Bati, analiz, 10, 168, 190, 88);
            EkleKutu(Player.Dogu, analiz, 210, 168, 190, 88);
            EkleKutu(Player.Guney, analiz, 105, 310, 200, 88);

            var btnOk = new Button { Text = "OK", Location = new Point(165, 414), Size = new Size(80, 25) };
            btnOk.Click += (s, e) => this.Close();
            this.Controls.Add(btnOk);
            this.AcceptButton = btnOk;
            this.ResumeLayout(false);
        }

        private void EkleKutu(Player oyuncu, Dictionary<Player, KoltukYorumu> analiz, int x, int y, int genislik, int yukseklik)
        {
            var yorum = analiz[oyuncu];

            this.Controls.Add(new Label
            {
                Text = KoltukHaritasi.Ad(oyuncu),
                Location = new Point(x + (genislik / 2) - 20, y - 18),
                AutoSize = true,
                Font = new Font("Arial", 8.5F, FontStyle.Regular)
            });

            var lst = new ListBox
            {
                Location = new Point(x, y),
                Size = new Size(genislik, yukseklik),
                BorderStyle = BorderStyle.Fixed3D,
                Font = new Font("Arial", 8.5F, FontStyle.Regular),
                SelectionMode = SelectionMode.None,
                IntegralHeight = false,
                BackColor = Color.White
            };
            foreach (var s in yorum.Satirlar()) lst.Items.Add(s);
            this.Controls.Add(lst);

            this.Controls.Add(new Label
            {
                Text = yorum.KonustuMu ? yorum.SonAciklama : "",
                Location = new Point(x, y + yukseklik + 2),
                AutoSize = true,
                Font = new Font("Arial", 8F, FontStyle.Regular),
                ForeColor = Color.DimGray
            });
        }

        private void IhaleYorumuForm_Load(object sender, EventArgs e)
        {
        }
    }
}
