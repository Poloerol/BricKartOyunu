using BricKartOyunu.Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    public partial class DeklarasyonForm : Form
    {
        private readonly BricOyna _bricOyna;

        private bool _kapanmayaIzinVerildi = false;
        private int _sonTeklifIndex = -1;

        private string _aktifOyuncu;
        private static readonly string[] OyuncuSirasi = { "Bati", "Kuzey", "Dogu", "Guney" };

        private int _baslangicSutunu;
        private int _teklifSayisi = 0;

        // İhale Mantığı Değişkenleri
        private int _ustUstePasSayisi = 0;
        private string _sonKontratTeklifi = ""; // Örn: "4♠"
        private string _sonKontratVeren = "";   // Örn: "Guney"
        private string _kontratKozu = "";       // Örn: "♠" veya "NT"

        // DeklarasyonForm.cs sınıf seviyesine ekleyin:
        private readonly List<string> _bids = new List<string>();

        // Grid Ölçüleri
        private const int SatirSayisi = 7;
        private const int SutunSayisi = 5;
        private const int NumW = 22;
        private const int HucreW = 26;
        private const int HucreH = 26;

        // Türkçe Briç Terimleri ve Simgeleri
        private static readonly string[] KozLabel = { "♣", "♦", "♥", "♠", "NT" };
        private static readonly Color[] KozRenk = {
            Color.Black,
            Color.FromArgb(220, 0, 0),
            Color.FromArgb(220, 0, 0),
            Color.Black,
            Color.Black
        };

        public DeklarasyonForm(BricOyna bricOyna)
        {
            InitializeComponent();
            _bricOyna = bricOyna;
            StartPosition = FormStartPosition.Manual;

            if (_bricOyna != null) this.Owner = _bricOyna;

            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            EnableDoubleBuffer(panelGrid);
            EnableDoubleBuffer(lstIhale);

            this.FormClosing += DeklarasyonForm_FormClosing;
            this.FormClosed += DeklarasyonForm_FormClosed;
        }

        private static void EnableDoubleBuffer(Control c)
        {
            if (c == null) return;
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, c, new object[] { true });
        }

        public void IzinliKapat()
        {
            _kapanmayaIzinVerildi = true;
            this.Close();
        }

        private void DeklarasyonForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_kapanmayaIzinVerildi && e.CloseReason != CloseReason.UserClosing)
            {
                e.Cancel = true;
                return;
            }

            if (_kapanmayaIzinVerildi) return;

            _bricOyna?.Close();
        }

        private void DeklarasyonForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _bricOyna?.SetGameControlsEnabled(true);
        }

        private void DeklarasyonForm_Load(object sender, EventArgs e)
        {
            if (_bricOyna?.playZonePanel != null)
            {
                Point ekran = _bricOyna.playZonePanel.PointToScreen(Point.Empty);
                Size boyut = _bricOyna.playZonePanel.Size;
                this.Location = new Point(
                    ekran.X + (boyut.Width - this.Width) / 2,
                    ekran.Y + (boyut.Height - this.Height) / 2
                );
            }

            TurkcelestirButonlar();
            _aktifOyuncu = _bricOyna?.ActivePlayer.ToString();
            _baslangicSutunu = Math.Max(0, OyuncuSutunIndex(_aktifOyuncu));
            IsaretleAktifOyuncu();

            if (btnDouble != null) btnDouble.Enabled = false;
            if (btnRedouble != null) btnRedouble.Enabled = false;
            if (btnPass != null) btnPass.Enabled = true;

            this.BackColor = Color.FromArgb(236, 233, 216);
            if (lstIhale != null) lstIhale.BackColor = Color.White;
            if (panelGrid != null) panelGrid.BackColor = Color.FromArgb(236, 233, 216);

            _bricOyna?.SetGameControlsEnabled(false);
        }

        private void IsaretleAktifOyuncu()
        {
            // DeklarasyonForm üzerindeki etiketler
            if (lblWest != null) lblWest.Text = "Batı";
            if (lblNorth != null) lblNorth.Text = "Kuzey";
            if (lblEast != null) lblEast.Text = "Doğu";
            if (lblSouth != null) lblSouth.Text = "Güney";

            switch (_aktifOyuncu)
            {
                case "Bati": if (lblWest != null) lblWest.Text = "Batı*"; break;
                case "Kuzey": if (lblNorth != null) lblNorth.Text = "Kuzey*"; break;
                case "Dogu": if (lblEast != null) lblEast.Text = "Doğu*"; break;
                case "Guney": if (lblSouth != null) lblSouth.Text = "Güney*"; break;
            }

            // Ana oyun formundaki (BricOyna) kart altı isim etiketini SARI yap
            _bricOyna?.AktifOyuncuEtiketiniGuncelle(_aktifOyuncu);
        }

        private void SonrakiOyuncuyaGec()
        {
            if (string.IsNullOrEmpty(_aktifOyuncu)) return;

            int idx = Array.IndexOf(OyuncuSirasi, _aktifOyuncu);
            if (idx < 0) return;

            idx = (idx + 1) % OyuncuSirasi.Length;
            _aktifOyuncu = OyuncuSirasi[idx];

            // Hem İhale paneli hem BricOyna üzerindeki Sarı etiketi günceller
            IsaretleAktifOyuncu();

            // Kontr ve Sürkontr buton durumlarını sıradaki oyuncuya göre aktif/pasif yapabilirsiniz
            KontrolluButonlariGuncelle();
        }

        private void TurkcelestirButonlar()
        {
           if (btnInterpret != null) btnInterpret.Text = "İhale Analizi";
            if (btnFlowcharts != null) btnFlowcharts.Text = "Akış Şeması";
            if (btnHint != null) btnHint.Text = "İpucu";
            if (btnPass != null) btnPass.Text = "Pas";
            if (btnDouble != null) btnDouble.Text = "Kontr";
            if (btnRedouble != null) btnRedouble.Text = "S.Kontr";
        }

        private static int LinearIndex(int r, int s) => r * SutunSayisi + s;

        private void PanelGrid_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color bgGri = Color.FromArgb(180, 180, 180);
            Color cizgiKoyu = Color.FromArgb(100, 100, 100);

            using (Font fontKoz = new Font("Arial", 16F, FontStyle.Bold))
            using (Font fontSA = new Font("Arial", 9.5F, FontStyle.Bold))
            using (Font fontNum = new Font("Arial", 10F, FontStyle.Regular))
            using (StringFormat sfCenter = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                for (int r = 0; r < SatirSayisi; r++)
                {
                    int seviye = r + 1;
                    int y = r * HucreH;

                    RectangleF numRect = new RectangleF(0, y, NumW, HucreH);
                    using (SolidBrush numBg = new SolidBrush(this.panelGrid.BackColor))
                        g.FillRectangle(numBg, numRect);

                    g.DrawString(seviye.ToString() + ".", fontNum, Brushes.Black, numRect, sfCenter);

                    for (int s = 0; s < SutunSayisi; s++)
                    {
                        int x = NumW + s * HucreW;
                        Rectangle rect = new Rectangle(x, y, HucreW, HucreH);

                        using (SolidBrush bgBr = new SolidBrush(bgGri))
                            g.FillRectangle(bgBr, rect);

                        using (Pen p = new Pen(cizgiKoyu))
                            g.DrawRectangle(p, rect);

                        bool aktif = LinearIndex(r, s) > _sonTeklifIndex;

                        if (aktif)
                        {
                            Font kf = (s == 4) ? fontSA : fontKoz;

                            using (SolidBrush kozBr = new SolidBrush(KozRenk[s]))
                            {
                                RectangleF textRect = rect;
                                if (s < 4) textRect.Y -= 1;

                                g.DrawString(KozLabel[s], kf, kozBr, textRect, sfCenter);
                            }
                        }
                    }
                }

                using (Pen outerPen = new Pen(cizgiKoyu, 1f))
                    g.DrawRectangle(outerPen, 0, 0, NumW + SutunSayisi * HucreW, SatirSayisi * HucreH);
            }
        }

        

        private static int OyuncuSutunIndex(string oyuncu)
        {
            switch (oyuncu)
            {
                case "Bati": return 0;
                case "Kuzey": return 1;
                case "Dogu": return 2;
                case "Guney": return 3;
                default: return -1;
            }
        }

        private void EkleIhaleGecmisi(string oyuncu, string deger)
        {
            int col = OyuncuSutunIndex(oyuncu);
            if (col < 0) return;

            int globalIndex = _baslangicSutunu + _teklifSayisi;
            int row = globalIndex / 4;

            while (lstIhale.Items.Count <= row)
                lstIhale.Items.Add(new ListViewItem(new string[] { "", "", "", "" }));

            ListViewItem satir = lstIhale.Items[row];
            while (satir.SubItems.Count < 4) satir.SubItems.Add("");

            // Yorum satırları kaldırıldı, değer hücreye yazılıyor:
            satir.SubItems[col].Text = deger;
            lstIhale.EnsureVisible(row);
            lstIhale.Refresh();

            // teklifleri kaydet
            _bids.Add(deger);

            _teklifSayisi++;
        }

        private void LstIhale_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            // Arka planı çiz (seçili durum vs. için)
            e.DrawBackground();

            string text = e.SubItem.Text;
            if (string.IsNullOrEmpty(text)) return;

            // Özel Çizim Ayarları
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Varsayılan font ve hizalama
            Font font = e.Header.ListView.Font;
            Rectangle rect = e.Bounds;

            // Pas, Dbl, RDbl veya Kozsuz (NT) / Siyah Simgeler (♣, ♠)
            if (text.Contains("♦") || text.Contains("♥"))
            {
                // Karo ve Kupa içeren tekliflerde metni parçalayarak renklendiriyoruz
                // Örn: "1♦" -> '1' Siyah, '♦' Kırmızı
                string level = text.Substring(0, text.Length - 1);
                string suit = text.Substring(text.Length - 1);

                // Seviye rakamını siyah çiz
                SizeF levelSize = g.MeasureString(level, font);
                using (SolidBrush blackBrush = new SolidBrush(Color.Black))
                {
                    g.DrawString(level, font, blackBrush, rect.X + 2, rect.Y + 4);
                }

                // Karo veya Kupa simgesini kırmızı çiz
                using (SolidBrush redBrush = new SolidBrush(Color.FromArgb(220, 0, 0)))
                {
                    using (Font suitFont = new Font("Arial", 10F, FontStyle.Bold))
                    {
                        g.DrawString(suit, suitFont, redBrush, rect.X + 2 + (int)levelSize.Width - 3, rect.Y + 3);
                    }
                }
            }
            else
            {
                // Diğer tüm teklifler (Pas, Dbl, RDbl, ♣, ♠, NT) standart siyah çizilir
                using (SolidBrush textBrush = new SolidBrush(Color.Black))
                {
                    g.DrawString(text, font, textBrush, rect.X + 2, rect.Y + 4);
                }
            }
        }

        // --- Buton Tıklama Olayları ---
        
        private void BtnInterpret_Click(object sender, EventArgs e)
        {
            // İhale Analizi mantığı
        }

        private void BtnPass_Click(object sender, EventArgs e)
        {
            EkleIhaleGecmisi(_aktifOyuncu, "Pas");
            _ustUstePasSayisi++;

            // İhale Bitiş Kontrolü
            if (IhaleBittiMi())
            {
                IhaleyiSonlandir();
                return;
            }

            SonrakiOyuncuyaGec();
        }


        private void BtnFlowcharts_Click(object sender, EventArgs e)
        {
            // Akış Şeması gösterimi
        }

        private void BtnHint_Click(object sender, EventArgs e)
        {
            // İpucu gösterimi
        }

        private void BtnDouble_Click(object sender, EventArgs e)
        {
            EkleIhaleGecmisi(_aktifOyuncu, "Dbl");
            SonrakiOyuncuyaGec();
        }

        private void BtnRedouble_Click(object sender, EventArgs e)
        {
            EkleIhaleGecmisi(_aktifOyuncu, "RDbl");
            SonrakiOyuncuyaGec();
        }
        private bool IhaleBittiMi()
        {
            // Açılışta herkes pas dediyse (4 Pas)
            if (string.IsNullOrEmpty(_sonKontratTeklifi) && _ustUstePasSayisi == 4)
            {
                return true;
            }

            // Bir teklif verildikten sonra üst üste 3 pas geldiyse
            if (!string.IsNullOrEmpty(_sonKontratTeklifi) && _ustUstePasSayisi == 3)
            {
                return true;
            }

            return false;
        }
        private void IhaleyiSonlandir()
        {
            if (string.IsNullOrEmpty(_sonKontratTeklifi))
            {
                MessageBox.Show("Tüm oyuncular Pas geçti. El oynanmadan kapatılıyor.", "İhale Bitti", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _bricOyna?.DortPasSonrasiYeniEliBaslat();
                IzinliKapat();
                return;
            }

            string deklaran = DeklaraniBul(_sonKontratVeren, _kontratKozu);
            string solRakip = SolRakipBul(deklaran);

            // İhale geçmişini ve başlayan oyuncuyu BricOyna'ya kaydet
            _bricOyna?.IhaleGecmisiniKaydet(_bids, OyuncuSirasi[_baslangicSutunu]);

            // Oynatma ekranına bilgileri aktar
            _bricOyna?.IhaleTamamlandiBaslat(_sonKontratTeklifi, deklaran, solRakip);

            IzinliKapat();
        }

        // Son teklifi veren tarafın (kendisinin veya ortağının) bu kozu İLK kimin söylediğini bulur
        private string DeklaraniBul(string sonTeklifVeren, string koz)
        {
            string ortak = OrtakBul(sonTeklifVeren);

            // İhale geçmişini (ListView) baştan sona tarayıp bu kozu ilk söyleyen taraf üyesini bulıyoruz
            foreach (ListViewItem row in lstIhale.Items)
            {
                for (int col = 0; col < 4; col++)
                {
                    string teklif = row.SubItems[col].Text;
                    if (!string.IsNullOrEmpty(teklif) && teklif.Contains(koz))
                    {
                        string teklifYapan = OyuncuSirasi[col];
                        if (teklifYapan == sonTeklifVeren || teklifYapan == ortak)
                        {
                            return teklifYapan; // Kozu ilk deklare eden ortak Deklaran olur!
                        }
                    }
                }
            }

            return sonTeklifVeren;
        }

        private string OrtakBul(string oyuncu)
        {
            switch (oyuncu)
            {
                case "Kuzey": return "Guney";
                case "Guney": return "Kuzey";
                case "Bati": return "Dogu";
                case "Dogu": return "Bati";
                default: return "";
            }
        }

        private string SolRakipBul(string oyuncu)
        {
            int idx = Array.IndexOf(OyuncuSirasi, oyuncu);
            return OyuncuSirasi[(idx + 1) % 4];
        }

        private string OyuncuIsmiTurkce(string oyuncu)
        {
            switch (oyuncu)
            {
                case "Bati": return "Batı";
                case "Kuzey": return "Kuzey";
                case "Dogu": return "Doğu";
                case "Guney": return "Güney";
                default: return oyuncu;
            }
        }
        private void PanelGrid_MouseClick(object sender, MouseEventArgs e)
        {
            int lx = e.X - NumW;
            int ly = e.Y;
            if (lx < 0 || ly < 0) return;

            int s = lx / HucreW;
            int r = ly / HucreH;
            if (s >= SutunSayisi || r >= SatirSayisi) return;

            int idx = LinearIndex(r, s);
            if (idx <= _sonTeklifIndex) return; // Önceki tekliften daha düşük teklif verilemez

            _sonTeklifIndex = idx;
            _ustUstePasSayisi = 0; // Bir teklif verildiği için pas sayısı sıfırlanır

            int seviye = r + 1;
            string koz = KozLabel[s];
            string deger = seviye.ToString() + koz;

            _sonKontratTeklifi = deger;
            _sonKontratVeren = _aktifOyuncu;
            _kontratKozu = koz;

            EkleIhaleGecmisi(_aktifOyuncu, deger);
            SonrakiOyuncuyaGec();
            panelGrid.Invalidate();
        }
        /// <summary>
        /// Deklarasyon butonlarının aktiflik/pasiflik durumlarını kontrol eder.
        /// </summary>
        private void KontrolluButonlariGuncelle()
        {
            UpdateBiddingButtonsState();
        }
        // DeklarasyonForm.cs içinde

        private void UpdateBiddingButtonsState()
        {
            if (_bids == null || btnDouble == null) return;

            // "Pas", "Dbl", "RDbl" dışındaki gerçek renk/sanzatu tekliflerini alıyoruz
            var realBids = _bids.Where(b => b != "Pas" && b != "Dbl" && b != "RDbl" && b != "PAS").ToList();

            if (realBids.Count > 0)
            {
                string lastRealBid = realBids.Last();
                int lastRealBidIndex = _bids.LastIndexOf(lastRealBid);

                // Bu tekliften sonra Kontr (Dbl) atılmış mı?
                bool isAlreadyDoubled = _bids.Skip(lastRealBidIndex + 1).Any(b => b == "Dbl" || b == "Kontur" || b == "X");

                // Teklifi yapan sütun indeksi ile şu anki aktif oyuncunun sütun indeksini karşılaştırıp rakip mi bakıyoruz
                int teklifGlobalIndex = _baslangicSutunu + lastRealBidIndex;
                int teklifSutunCol = teklifGlobalIndex % 4; // 0:Bati, 1:Kuzey, 2:Dogu, 3:Guney

                int aktifSutunCol = OyuncuSutunIndex(_aktifOyuncu);

                // Briçte rakipler tek/çift indeks mantığıdır (0 ve 2 kumpanya, 1 ve 3 kumpanya)
                bool isOpponent = (teklifSutunCol % 2) != (aktifSutunCol % 2);

                btnDouble.Enabled = isOpponent && !isAlreadyDoubled;
            }
            else
            {
                btnDouble.Enabled = false;
            }
        }

        private void BtnElDegerlendirme_Click(object sender, EventArgs e)
        {
            // Ana formu bul
            BricOyna bricOyna = Application.OpenForms.OfType<BricOyna>().FirstOrDefault();

            // Güney'in 13 kartını al (BricOyna üzerindeki listenizden çekiyoruz)
            List<Card> guneyKartlari = bricOyna != null ? bricOyna.GetGuneyElinKartlari() : new List<Card>();

            // Kartları form yapıcısına aktar ve nesne başlatıcıyı doğru şekilde bağla
            ElDegerForm frm = new ElDegerForm(guneyKartlari)
            {
                ShowInTaskbar = false
            };

            if (bricOyna != null && bricOyna.playZonePanel != null)
            {
                Point panelCenterOnScreen = bricOyna.playZonePanel.PointToScreen(
                    new Point(bricOyna.playZonePanel.Width / 2, bricOyna.playZonePanel.Height / 2)
                );

                frm.StartPosition = FormStartPosition.Manual;
                frm.Location = new Point(
                    panelCenterOnScreen.X - (frm.Width / 2),
                    panelCenterOnScreen.Y - (frm.Height / 2)
                );

                frm.Show(this);
            }
            else
            {
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.Show(this);
            }
        }

    }
}