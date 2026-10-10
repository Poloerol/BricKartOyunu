using BricKartOyunu.Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;

namespace BricKartOyunu.Forms
{
    public partial class DeklarasyonForm : Form
    {
        private readonly BricOyna _bricOyna;

        private bool _kapanmayaIzinVerildi = false;

        // ═══════════════════════════════════════════════════════════════════
        // İHALE MOTORU — AI'ların teklif vermesi için
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// İhale motoru — AI'ların teklif vermesi için.
        /// </summary>
        private IhaleMotoru _motor;

        /// <summary>
        /// Ortaklık anlaşması (konvansiyon ayarları).
        /// </summary>
        private OrtaklikAnlasmasi _anlasma;

        /// <summary>
        /// AI hamlesini geciktirmek için timer (düşünme efekti).
        /// </summary>
        private System.Windows.Forms.Timer _aiTimer;

        /// <summary>
        /// AI oynayacak mı? (test için kapatılabilir)
        /// </summary>
        private bool _aiAktif = true;

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

        private readonly List<IhaleHamlesi> _ihaleHamleleri = new List<IhaleHamlesi>();

        // İleri alınan (redo) hamleleri tutar
        private readonly List<(string Oyuncu, string Teklif)> _ileriAlinanHamleler
            = new List<(string, string)>();

        /// <summary>
        /// Geri alınacak hamle var mı?
        /// </summary>
        public bool GeriAlinabilirMi => _teklifSayisi > 0;

        /// <summary>
        /// İleri alınacak (redo) hamle var mı?
        /// </summary>
        public bool IleriAlinabilirMi => _ileriAlinanHamleler.Count > 0;

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

            // 🔹 İhale motorunu başlat
            _anlasma = OrtaklikAnlasmasi.Varsayilan();
            _motor = new IhaleMotoru(_anlasma);

            System.Diagnostics.Debug.WriteLine(
                $"[DeklarasyonForm] Motor başlatıldı: {_motor.Ozet()}");
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
            // Uygulama tamamen kapanıyorsa engel koyma
            if (e.CloseReason == CloseReason.ApplicationExitCall ||
                e.CloseReason == CloseReason.WindowsShutDown ||
                e.CloseReason == CloseReason.TaskManagerClosing)
            {
                return;
            }

            // İzinli kapatma ise devam et
            if (_kapanmayaIzinVerildi)
            {
                return;
            }

            // Kullanıcı X'e bastı → tüm formları kapat, AnaSayfa'ya dön
            if (e.CloseReason == CloseReason.UserClosing)
            {
                _kapanmayaIzinVerildi = true;

                // Açık DekBasForm'ları kapat
                foreach (Form f in Application.OpenForms.Cast<Form>()
                            .Where(f => f is DekBasForm)
                            .ToArray())
                {
                    f.Close();
                }

                // BricOyna'yı kapat
                _bricOyna?.Close();
            }
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

            System.Diagnostics.Debug.WriteLine(
    $"[DeklarasyonForm_Load] Aktif oyuncu: {_aktifOyuncu}, " +
    $"BricOyna.ActivePlayer: {_bricOyna?.ActivePlayer}, " +
    $"baslangicSutunu: {_baslangicSutunu}");
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
            System.Diagnostics.Debug.WriteLine(
        $"[SonrakiOyuncuyaGec] ÇAĞRILDI — aktif: {_aktifOyuncu}, " +
        $"çağıran: {new System.Diagnostics.StackTrace().GetFrame(1)?.GetMethod()?.Name}");

            if (string.IsNullOrEmpty(_aktifOyuncu)) return;

            int idx = Array.IndexOf(OyuncuSirasi, _aktifOyuncu);
            if (idx < 0) return;

            idx = (idx + 1) % OyuncuSirasi.Length;
            _aktifOyuncu = OyuncuSirasi[idx];

            IsaretleAktifOyuncu();
            KontrolluButonlariGuncelle();

            // 🔹 AI sırası mı? Tetikle
            AITetikle();
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
            // 🔹 BricOyna'daki BtnGeriAl / BtnileriAl durumlarını güncelle
            _bricOyna?.GeriAlIleriAlButonlariniGuncelle();

            _ihaleHamleleri.Add(new IhaleHamlesi
            {
                Oyuncu = OyuncuyaCevir(oyuncu),
                Teklif = deger,
                Sira = _ihaleHamleleri.Count + 1,
                GecerliMi = true
            });

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

            // 🔹 YENİ: Pas yapıldı, geri alınabilir hale geldi
            _bricOyna?.GeriAlButonunuAktifEt();

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

            // 🔹 Kontr da bir tekliftir → pas sayacını sıfırla
            _ustUstePasSayisi = 0;

            // 🔹 Geri al butonunu aktif et
            _bricOyna?.GeriAlButonunuAktifEt();

            SonrakiOyuncuyaGec();
        }

        private void BtnRedouble_Click(object sender, EventArgs e)
        {
            EkleIhaleGecmisi(_aktifOyuncu, "RDbl");

            // 🔹 S.Kontr da bir tekliftir → pas sayacını sıfırla
            _ustUstePasSayisi = 0;

            // 🔹 Geri al butonunu aktif et
            _bricOyna?.GeriAlButonunuAktifEt();

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
        /// <summary>
        /// Son yapılan teklifi/pası/kontru geri alır.
        /// Hem ListView hem grid panelinin görünümünü doğru state'e döndürür.
        /// </summary>
        public void SonHamleyiGeriAl()
        {
            if (_teklifSayisi == 0) return;
            if (lstIhale == null || lstIhale.Items.Count == 0) return;

            // 1. Son satırın son dolu hücresini bul ve temizle
            ListViewItem sonSatir = lstIhale.Items[lstIhale.Items.Count - 1];
            int sonSutun = -1;
            string silinenTeklif = "";
            string silinenOyuncu = "";

            for (int i = sonSatir.SubItems.Count - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(sonSatir.SubItems[i].Text))
                {
                    sonSutun = i;
                    silinenTeklif = sonSatir.SubItems[i].Text;
                    silinenOyuncu = OyuncuSirasi[i];
                    sonSatir.SubItems[i].Text = "";
                    break;
                }
            }

            if (sonSutun == -1) return;

            // 2. Satır tamamen boşsa satırı sil
            bool bosMu = sonSatir.SubItems.Cast<ListViewItem.ListViewSubItem>()
                                .All(s => string.IsNullOrEmpty(s.Text));
            if (bosMu)
            {
                lstIhale.Items.Remove(sonSatir);
            }

            // 3. _bids'ten son elemanı çıkar
            if (_bids.Count > 0)
            {
                _bids.RemoveAt(_bids.Count - 1);
            }

            // 3b. _ihaleHamleleri'nden de son elemanı çıkar
            if (_ihaleHamleleri.Count > 0)
            {
                _ihaleHamleleri.RemoveAt(_ihaleHamleleri.Count - 1);
            }

            // 4. İleri almak için sakla
            _ileriAlinanHamleler.Add((silinenOyuncu, silinenTeklif));

            // 5. Sayacı azalt
            _teklifSayisi--;

            // 6. Aktif oyuncuyu bir öncekine al
            int idx = Array.IndexOf(OyuncuSirasi, _aktifOyuncu);
            if (idx >= 0)
            {
                idx = (idx - 1 + OyuncuSirasi.Length) % OyuncuSirasi.Length;
                _aktifOyuncu = OyuncuSirasi[idx];
            }
            IsaretleAktifOyuncu();

            // 🔹 7. KRİTİK DÜZELTME: Grid'i doğru state'e döndür
            //     Önce tüm değişkenleri sıfırla
            _sonKontratTeklifi = "";
            _sonKontratVeren = "";
            _kontratKozu = "";
            _sonTeklifIndex = -1;
            _ustUstePasSayisi = 0;

            // 🔹 Sonra kalan teklifleri tarayıp en son "gerçek" teklifin indeksini bul
            //    "_bids" listesindeki her bir teklif için hangi hücreye denk geldiğini hesapla
            //    (Aynı mantıkla: teklif -> seviye ve koz, seviye-1 ve koz indeksinden hesapla)
            if (_bids.Count > 0)
            {
                // _bids'teki son GERÇEK teklifi bul (Pas, Dbl, RDbl değil)
                string sonGercekTeklif = "";
                for (int i = _bids.Count - 1; i >= 0; i--)
                {
                    string b = _bids[i];
                    if (b != "Pas" && b != "Dbl" && b != "RDbl")
                    {
                        sonGercekTeklif = b;
                        break;
                    }
                }

                // Eğer son gerçek teklif varsa, grid indeksini hesapla
                if (!string.IsNullOrEmpty(sonGercekTeklif))
                {
                    _sonTeklifIndex = TeklifGridIndexHesapla(sonGercekTeklif);

                    // Ayrıca bu teklifin kontrat bilgilerini geri yükle
                    // (örn. teklif "2♣" ise, kontrat 2♣, kozu Sinek olur)
                    string koz = sonGercekTeklif.Length > 0
                        ? sonGercekTeklif.Last().ToString()
                        : "";

                    int seviye = 0;
                    if (sonGercekTeklif.Length > 1)
                        int.TryParse(sonGercekTeklif.Substring(0, sonGercekTeklif.Length - 1), out seviye);

                    _sonKontratTeklifi = sonGercekTeklif;
                    _kontratKozu = koz;
                    // _sonKontratVeren'i kesin bilmiyoruz, bırakabiliriz
                    // (IhaleyiSonlandir sırasında zaten liste üzerinden bulunuyor)
                }
            }

            // 8. Grid'i yenile — ARTIK _sonTeklifIndex doğru değere sahip
            panelGrid?.Invalidate();

            // 9. Buton durumlarını güncelle
            UpdateBiddingButtonsState();
        }

        /// <summary>
        /// "2♣" gibi bir teklifi grid indeksine çevirir.
        /// Grid: Satır = seviye (1-7), Sütun = koz (♣=0, ♦=1, ♥=2, ♠=3, NT=4)
        /// </summary>
        private static int TeklifGridIndexHesapla(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2) return -1;

            string koz = teklif.Last().ToString();
            string seviyeStr = teklif.Substring(0, teklif.Length - 1);

            int seviye;
            if (!int.TryParse(seviyeStr, out seviye)) return -1;

            if (seviye < 1 || seviye > 7) return -1;

            int sutun = -1;
            switch (koz)
            {
                case "♣": sutun = 0; break;
                case "♦": sutun = 1; break;
                case "♥": sutun = 2; break;
                case "♠": sutun = 3; break;
                case "NT":
                case "N":
                case "T": sutun = 4; break;
                default: return -1;
            }

            int satir = seviye - 1;
            return satir * 5 + sutun;
        }

        /// <summary>
        /// Geri alınan son hamleyi ileri alır (redo).
        /// BricOyna'daki BtnileriAl tarafından çağrılır.
        /// </summary>
        public void SonHamleyiIleriAl()
        {
            if (_ileriAlinanHamleler.Count == 0) return;

            // Son geri alınan hamleyi al
            var (Oyuncu, Teklif) = _ileriAlinanHamleler[_ileriAlinanHamleler.Count - 1];
            _ileriAlinanHamleler.RemoveAt(_ileriAlinanHamleler.Count - 1);

            // Aktif oyuncuyu hamlenin sahibine ayarla
            _aktifOyuncu = Oyuncu;
            IsaretleAktifOyuncu();

            // Teklifi tekrar ekle
            EkleIhaleGecmisi(Oyuncu, Teklif);

            // Sonraki oyuncuya geç
            SonrakiOyuncuyaGec();

            // Yeni teklif eklendi, _sonTeklifIndex güncellenmeli
            _sonTeklifIndex = TeklifGridIndexHesapla(Teklif);

            panelGrid?.Invalidate();
            UpdateBiddingButtonsState();
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
            if (idx <= _sonTeklifIndex) return;

            _sonTeklifIndex = idx;
            _ustUstePasSayisi = 0;

            int seviye = r + 1;
            string koz = KozLabel[s];
            string deger = seviye.ToString() + koz;

            _sonKontratTeklifi = deger;
            _sonKontratVeren = _aktifOyuncu;
            _kontratKozu = koz;

            EkleIhaleGecmisi(_aktifOyuncu, deger);
            SonrakiOyuncuyaGec();
            panelGrid.Invalidate();

            // 🔹 YENİ: İlk teklif yapıldı, geri alınabilir hale geldi
            _bricOyna?.GeriAlButonunuAktifEt();
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
            if (_bids == null || btnDouble == null || btnRedouble == null) return;

            // "Pas", "Dbl", "RDbl" dışındaki gerçek renk/sanzatu tekliflerini al
            var realBids = _bids
                .Where(b => b != "Pas" && b != "Dbl" && b != "RDbl" && b != "PAS")
                .ToList();

            // Varsayılan: her ikisi de pasif
            btnDouble.Enabled = false;
            btnRedouble.Enabled = false;

            if (realBids.Count == 0) return;

            // Son gerçek teklifi ve indeksini bul
            string lastRealBid = realBids.Last();
            int lastRealBidIndex = _bids.LastIndexOf(lastRealBid);

            // Bu tekliften sonra atılan hamleler
            var sonrakiHamleler = _bids.Skip(lastRealBidIndex + 1).ToList();

            // Bu teklife Kontr atılmış mı?
            bool isAlreadyDoubled = sonrakiHamleler.Any(b => b == "Dbl");

            // Bu teklife S.Kontr atılmış mı?
            bool isAlreadyRedoubled = sonrakiHamleler.Any(b => b == "RDbl");

            // Son teklifi yapan sütun
            int teklifGlobalIndex = _baslangicSutunu + lastRealBidIndex;
            int teklifSutunCol = teklifGlobalIndex % 4; // 0:Bati, 1:Kuzey, 2:Dogu, 3:Guney

            // Aktif oyuncunun sütunu
            int aktifSutunCol = OyuncuSutunIndex(_aktifOyuncu);

            // Aynı takımda mı? (0 ve 2 bir takım, 1 ve 3 diğer takım)
            bool ayniTakim = (teklifSutunCol % 2) == (aktifSutunCol % 2);
            bool rakipTakim = !ayniTakim;

            // ── KONTR (Dbl) KURALI ──
            // Rakip takım, son teklife kontr atabilir (henüz atılmamışsa)
            btnDouble.Enabled = rakipTakim && !isAlreadyDoubled;

            // ── S.KONTR (RDbl) KURALI ──
            // Kontr atılmış ve henüz s.kontr atılmamışsa,
            // kontr atan tarafın RAKİBİ (yani teklifi yapan takım) s.kontr atabilir
            if (isAlreadyDoubled && !isAlreadyRedoubled)
            {
                // Kontr'u atan oyuncuyu bul
                int kontrIndex = _bids.LastIndexOf("Dbl");
                int kontrGlobalIndex = _baslangicSutunu + kontrIndex;
                int kontrSutunCol = kontrGlobalIndex % 4;

                // Kontr atan takımın rakibi (yani teklif sahibi takım) s.kontr atabilir
                bool kontrAtanRakip = (kontrSutunCol % 2) != (aktifSutunCol % 2);
                btnRedouble.Enabled = kontrAtanRakip;
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

        // ═══════════════════════════════════════════════════════════════════
        // AI İHALE
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aktif oyuncu AI ise, kısa bir gecikmeyle motoru çalıştırır.
        /// Şimdilik SADECE Batı AI. Diğerleri insan.
        /// </summary>
        private void AITetikle()
        {
            System.Diagnostics.Debug.WriteLine(
        $"[AITetikle] ÇAĞRILDI — aktif: {_aktifOyuncu}, " +
        $"çağıran: {new System.Diagnostics.StackTrace().GetFrame(1)?.GetMethod()?.Name}");

            // AI aktif mi?
            if (!_aiAktif) return;
            if (_motor == null) return;

            // Aktif oyuncu AI mi? (Şimdilik SADECE Batı)
            if (!AIMi(_aktifOyuncu))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AI] {_aktifOyuncu} insan, AI tetiklenmiyor.");
                return;
            }

            System.Diagnostics.Debug.WriteLine(
                $"[AI] {_aktifOyuncu} AI sırası — motor çalışacak.");

            // Önceki timer'ı temizle
            _aiTimer?.Stop();
            _aiTimer?.Dispose();

            // 800ms sonra AI oynasın
            _aiTimer = new System.Windows.Forms.Timer { Interval = 800 };
            _aiTimer.Tick += (s, e) =>
            {
                _aiTimer.Stop();
                _aiTimer.Dispose();
                _aiTimer = null;

                AIHamlesiYap();
            };
            _aiTimer.Start();
        }

        /// <summary>
        /// Belirtilen oyuncu AI mı?
        /// Şimdilik SADECE Batı AI. Diğerleri insan.
        /// </summary>
        private bool AIMi(string oyuncu)
        {
            // Batı, Kuzey, Doğu → AI
            // Güney → insan
            return oyuncu != "Guney";
        }

        /// <summary>
        /// AI'ın hamlesini yapar: motoru çağırır, teklifi uygular.
        /// </summary>
        private void AIHamlesiYap()
        {
            try
            {
                // 1. İhale durumunu oluştur
                var durum = IhaleDurumuOlustur();

                // ═══════════════════════════════════════════════════════════════
                // 🔹 DEBUG: Durum bilgisi
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("════════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] AktifOyuncu: {durum.AktifOyuncu}");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] AktifOyuncuEli: {durum.AktifOyuncuEli?.Count ?? 0} kart");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] HCP: {ElDegerlendirici.HCP(durum.AktifOyuncuEli)}");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] TP: {ElDegerlendirici.ToplamPuan(durum.AktifOyuncuEli)}");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] DengeliMi: {ElDegerlendirici.DengeliEl(durum.AktifOyuncuEli)}");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] Gecmis.Count: {durum.Gecmis.Count}");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] IlkTeklifMi: {durum.IlkTeklifMi()}");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] IhaleBittiMi: {durum.IhaleBittiMi()}");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] Partner: {durum.Partner}");
                System.Diagnostics.Debug.WriteLine($"[AI DEBUG] Rakipler: [{string.Join(", ", durum.Rakipler)}]");

                // Geçmiş hamleleri listele
                if (durum.Gecmis.Count > 0)
                {
                    var gecmisStr = string.Join(" | ",
                        durum.Gecmis.Select(h => $"{h.Oyuncu}:{h.Teklif}"));
                    System.Diagnostics.Debug.WriteLine($"[AI DEBUG] Gecmis: {gecmisStr}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AI DEBUG] Gecmis: (boş)");
                }

                // El dağılımı
                var sayilar = ElDegerlendirici.RenkSayilari(durum.AktifOyuncuEli);
                System.Diagnostics.Debug.WriteLine(
                    $"[AI DEBUG] Dağılım: ♠{sayilar["Maça"]} ♥{sayilar["Kupa"]} ♦{sayilar["Karo"]} ♣{sayilar["Sinek"]}");

                System.Diagnostics.Debug.WriteLine("────────────────────────────────────────");

                // 2. Motor'dan teklif al
                string teklif = _motor.TeklifVer(durum);

                System.Diagnostics.Debug.WriteLine(
                    $"[AI] {_aktifOyuncu} motor teklifi: {teklif}");
                System.Diagnostics.Debug.WriteLine("════════════════════════════════════════");

                // 3. Teklifi uygula
                AIHamlesiniUygula(teklif);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AI] HATA: {ex.Message}\n{ex.StackTrace}");

                // Güvenli fallback: Pas
                AIHamlesiniUygula("Pas");
            }
        }

        /// <summary>
        /// AI teklifini uygular (Pas, teklif, kontr, vs.).
        /// </summary>
        /// <summary>
        /// AI teklifini uygular (Pas, teklif, kontr, vs.).
        /// KozYardimcisi kullanarak parse eder (1NT dahil).
        /// </summary>
        private void AIHamlesiniUygula(string teklif)
        {
            if (string.IsNullOrEmpty(teklif)) teklif = "Pas";

            System.Diagnostics.Debug.WriteLine(
                $"[AI] Uygulanıyor: {_aktifOyuncu} → {teklif}");

            // Pas mı?
            if (teklif == "Pas" || teklif == "PAS")
            {
                EkleIhaleGecmisi(_aktifOyuncu, "Pas");
                _ustUstePasSayisi++;

                if (IhaleBittiMi())
                {
                    IhaleyiSonlandir();
                    return;
                }

                SonrakiOyuncuyaGec();
                return;
            }

            // Dbl / RDbl
            if (teklif == "Dbl" || teklif == "Kontr")
            {
                EkleIhaleGecmisi(_aktifOyuncu, "Dbl");
                _ustUstePasSayisi = 0;
                SonrakiOyuncuyaGec();
                return;
            }

            if (teklif == "RDbl" || teklif == "S.Kontr")
            {
                EkleIhaleGecmisi(_aktifOyuncu, "RDbl");
                _ustUstePasSayisi = 0;
                SonrakiOyuncuyaGec();
                return;
            }

            // ─── GERÇEK TEKLİF ───────────────────────────────────────────────
            // KozYardimcisi kullanarak parse et (1NT dahil)
            int seviye = KozYardimcisi.TekliftenSeviyeCikar(teklif);
            string koz = KozYardimcisi.TekliftenKozCikar(teklif);

            if (seviye == 0 || string.IsNullOrEmpty(koz))
            {
                // Parse edilemedi — Pas yap
                System.Diagnostics.Debug.WriteLine(
                    $"[AI] HATA: '{teklif}' parse edilemedi, Pas yapılıyor.");
                EkleIhaleGecmisi(_aktifOyuncu, "Pas");
                _ustUstePasSayisi++;
                if (IhaleBittiMi()) { IhaleyiSonlandir(); return; }
                SonrakiOyuncuyaGec();
                return;
            }

            // Kontrat bilgilerini güncelle
            _sonKontratTeklifi = teklif;
            _sonKontratVeren = _aktifOyuncu;
            _kontratKozu = KozYardimcisi.KozSembolu(koz);

            // Grid indeksini hesapla
            _sonTeklifIndex = TeklifGridIndexHesapla(teklif);

            // ListView'a ekle
            EkleIhaleGecmisi(_aktifOyuncu, teklif);

            // Pas sayacını sıfırla
            _ustUstePasSayisi = 0;

            // Grid'i yenile
            panelGrid?.Invalidate();

            // Sonraki oyuncuya geç
            SonrakiOyuncuyaGec();
        }

        /// <summary>
        /// Mevcut ihale durumunu motor için oluşturur.
        /// </summary>
        private IhaleDurumu IhaleDurumuOlustur()
        {
            var durum = new IhaleDurumu
            {
                AktifOyuncu = OyuncuyaCevir(_aktifOyuncu),
                Anlasma = _anlasma,
                TurNo = 1,
                ZonNS = false,   // TODO: BricOyna'dan al
                ZonEW = false,   // TODO: BricOyna'dan al
                AktifOyuncuEli = _bricOyna?.GetOyuncuEli(OyuncuyaCevir(_aktifOyuncu))
            };

            // Basit geçmiş aktarımı — sıra numarası ve oyuncu ataması yapılamıyor
            // (Bu yüzden motor "ilk teklif" sanabilir.)
            // Şimdilik idare ediyoruz.

            durum.Gecmis = new List<IhaleHamlesi>(_ihaleHamleleri);
            return durum;
        }

        /// <summary>
        /// Bir Player enum'unu string'e çevirir (mevcut sistem uyumu için).
        /// </summary>
        private static Player OyuncuyaCevir(string oyuncu)
        {
            switch (oyuncu)
            {
                case "Kuzey": return Player.Kuzey;
                case "Dogu": return Player.Dogu;
                case "Bati": return Player.Bati;
                default: return Player.Guney;
            }
        }

    }
}