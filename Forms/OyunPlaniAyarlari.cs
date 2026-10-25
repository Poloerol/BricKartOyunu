using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    // FORM
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Oyun planı ayarları penceresi (400x280): Dağıtım, Oyun, Giriş,
    /// Oyuncu Türleri ve Oyuncu Resimleri sekmeleri. Tamam ile Secenekler güncellenir.
    /// </summary>
    public class OyunPlaniAyarlari : Form
    {
        private readonly Font _yazi = new Font("Microsoft Sans Serif", 8.25F);

        private TabControl _sekmeler;
        private Button _btnTamam, _btnIptal, _btnYardim, _btnVarsayilan;

        // Dağıtım
        private RadioButton[] _rbKaynak, _rbEnIyi, _rbDagitici;
        private CheckBox _chkYeniNo;
        // Oyun
        private RadioButton[] _rbOyunTuru, _rbGecikme;
        private CheckBox _chkGorunmez, _chkHemen, _chkHizli, _chkKontratIhale, _chkOk, _chkDondurme, _chkTekKart;
        // Giriş
        private RadioButton[] _rbIpucu, _rbDuraklat;
        private CheckBox _chkIhale, _chkOyun, _chkTumEller, _chkIpucuSes, _chkOyunSes, _chkSelam;
        // Oyuncu türleri
        private RadioButton[] _rbBilgisayar = new RadioButton[4];
        private RadioButton[] _rbInsan = new RadioButton[4];
        // Resimler
        private PictureBox[] _resimKutulari = new PictureBox[4];
        private readonly string[] _resimYollari = { "", "", "", "" };

        private static readonly string[] OyuncuAdlari = { "Kuzey", "Doğu", "Güney", "Batı" };

        /// <summary>Düzenlenen ayarlar. Tamam'a basılınca güncel değerlerle değişir.</summary>
        public OyunPlaniSecenekleri Secenekler { get; private set; }

        public OyunPlaniAyarlari() : this(null) { }

        public OyunPlaniAyarlari(OyunPlaniSecenekleri mevcut)
        {
            Secenekler = mevcut != null ? mevcut.Klonla() : new OyunPlaniSecenekleri();
            Kurulum();
            Yukle(Secenekler);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // OyunPlaniAyarlari
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "OyunPlaniAyarlari";
            this.Load += new System.EventHandler(this.OyunPlaniAyarlari_Load);
            this.ResumeLayout(false);

        }

        private void OyunPlaniAyarlari_Load(object sender, EventArgs e)
        {

        }

        // ───────────────────────────── kurulum ─────────────────────────────

        private void Kurulum()
        {
            Text = "Oyun Planı Ayarları";
            Font = _yazi;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            Size = new Size(400, 280);

            _sekmeler = new TabControl { Left = 6, Top = 6, Width = 372, Height = 196, Font = _yazi };
            Controls.Add(_sekmeler);

            DagitimSekmesi();
            OyunSekmesi();
            GirisSekmesi();
            OyuncuTurleriSekmesi();
            ResimlerSekmesi();

            _btnVarsayilan = Dugme("Varsayılan", 6, 208);
            _btnVarsayilan.Width = 84;
            _btnVarsayilan.Click += (s, e) =>
            {
                // Formdaki seçimleri varsayılana çevirir; kalıcı olması için Tamam'a basılmalı
                Yukle(new OyunPlaniSecenekleri());
            };
            _btnTamam = Dugme("Tamam", 134, 208);
            _btnIptal = Dugme("İptal", 218, 208);
            _btnYardim = Dugme("Yardım", 302, 208);
            _btnTamam.Click += (s, e) => { Kaydet(); DialogResult = DialogResult.OK; Close(); };
            _btnIptal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            _btnYardim.Click += (s, e) => MessageBox.Show(
                "Dağıtım: kartların nasıl dağıtılacağı.\nOyun: oyun türü ve oyun akışı.\n" +
                "Giriş: ipucu, duraklama ve ses.\nOyuncu Türleri: hangi koltuğu bilgisayar, hangisini siz oynarsınız.\n" +
                "Oyuncu Resimleri: koltuklara resim atayın.\n\n" +
                "Tamam ayarları kalıcı olarak kaydeder (Varsayılan düğmesi yalnızca formu sıfırlar).\n" +
                "Dosya: " + AyarDeposu.DosyaYolu(OyunPlaniSecenekleri.DepoAdi),
                "Yardım", MessageBoxButtons.OK, MessageBoxIcon.Information);
            AcceptButton = _btnTamam;
            CancelButton = _btnIptal;
        }

        private Button Dugme(string metin, int x, int y)
        {
            var b = new Button { Text = metin, Left = x, Top = y, Width = 76, Height = 26, Font = _yazi, UseVisualStyleBackColor = true };
            Controls.Add(b);
            return b;
        }

        private TabPage YeniSekme(string baslik)
        {
            var sayfa = new TabPage(baslik) { UseVisualStyleBackColor = true, Font = _yazi };
            _sekmeler.TabPages.Add(sayfa);
            return sayfa;
        }

        private GroupBox Grup(Control ust, string baslik, int x, int y, int w, int h)
        {
            var g = new GroupBox { Text = baslik, Left = x, Top = y, Width = w, Height = h, Font = _yazi };
            ust.Controls.Add(g);
            return g;
        }

        private RadioButton[] RadyoDizisi(Control kap, string[] metinler, int x, int y, int adim, int w)
        {
            var dizi = new RadioButton[metinler.Length];
            for (int i = 0; i < metinler.Length; i++)
            {
                dizi[i] = new RadioButton
                {
                    Text = metinler[i], Left = x, Top = y + i * adim, Width = w, Height = 17,
                    AutoSize = false, Font = _yazi
                };
                kap.Controls.Add(dizi[i]);
            }
            return dizi;
        }

        private CheckBox Onay(Control kap, string metin, int x, int y, int w, int h = 17)
        {
            var c = new CheckBox { Text = metin, Left = x, Top = y, Width = w, Height = h, AutoSize = false, Font = _yazi };
            kap.Controls.Add(c);
            return c;
        }

        private void DagitimSekmesi()
        {
            var s = YeniSekme("Dağıtım");

            var g1 = Grup(s, "Dağıtım Kaynağı", 4, 2, 152, 160);
            _rbKaynak = RadyoDizisi(g1, new[] {
                "Kullanıcı Girişli Dağıtım", "Rastgele Dağıtım", "Rastgele Part Skor", "Rastgele Oyun Eli",
                "Rastgele Şlem Eli", "Üretilmiş Dağılım", "Numaralı Rastgele Eller", "Sanzatu Dağılımları" },
                8, 16, 17, 140);

            var g2 = Grup(s, "En İyi El", 162, 2, 98, 160);
            _rbEnIyi = RadyoDizisi(g2, new[] {
                "Herhangi biri", "Kuzey-Güney", "Doğu-Batı", "Kuzey", "Doğu", "Güney", "Batı" },
                8, 16, 19, 86);

            var g3 = Grup(s, "Dağıtıcı", 266, 2, 92, 108);
            _rbDagitici = RadyoDizisi(g3, new[] { "Dönüşümlü", "Kuzey", "Doğu", "Güney", "Batı" },
                8, 16, 17, 80);

            _chkYeniNo = Onay(s, "Yeni bord numaralarını kullan", 268, 114, 92, 40);
        }

        private void OyunSekmesi()
        {
            var s = YeniSekme("Oyun");

            var g1 = Grup(s, "Oyun Türü", 4, 2, 136, 106);
            _rbOyunTuru = RadyoDizisi(g1, new[] { "Duplicate", "Rubber", "Chicago Duplicate", "Chicago Rubber", "Tek El" },
                8, 16, 17, 124);

            var g2 = Grup(s, "Duplicate Tekrarı", 146, 2, 212, 42);
            _chkGorunmez = Onay(g2, "Görünmez", 10, 18, 90);
            _chkHemen = Onay(g2, "Hemen", 112, 18, 90);

            var g3 = Grup(s, "Yapay Zekâ İhale/Oyun Gecikmesi", 146, 48, 212, 44);
            _rbGecikme = new RadioButton[3];
            string[] gecikme = { "0 sn", "1 sn", "2 sn" };
            for (int i = 0; i < 3; i++)
            {
                _rbGecikme[i] = new RadioButton { Text = gecikme[i], Left = 10 + i * 66, Top = 18, Width = 60, Height = 17, AutoSize = false, Font = _yazi };
                g3.Controls.Add(_rbGecikme[i]);
            }

            _chkHizli = Onay(s, "Her zaman hızlı otomatik oyna", 8, 114, 170);
            _chkKontratIhale = Onay(s, "Her zaman kontrat için ihale", 8, 134, 170);
            _chkOk = Onay(s, "Oku gösterme", 190, 100, 168);
            _chkDondurme = Onay(s, "Elleri döndürme", 190, 120, 168);
            _chkTekKart = Onay(s, "Tek kartı otomatik oyna", 190, 140, 168);
        }

        private void GirisSekmesi()
        {
            var s = YeniSekme("Giriş");

            var g1 = Grup(s, "İpucu Gösterimi", 4, 2, 170, 86);
            _rbIpucu = RadyoDizisi(g1, new[] { "İstendiğinde", "Farklıysa duyur", "Farklıysa göster", "Her zaman göster" },
                8, 16, 17, 156);

            var g2 = Grup(s, "El Sonrası Duraklat", 4, 92, 170, 42);
            _rbDuraklat = new RadioButton[2];
            _rbDuraklat[0] = new RadioButton { Text = "Hayır", Left = 10, Top = 18, Width = 70, Height = 17, AutoSize = false, Font = _yazi };
            _rbDuraklat[1] = new RadioButton { Text = "Evet", Left = 90, Top = 18, Width = 70, Height = 17, AutoSize = false, Font = _yazi };
            g2.Controls.Add(_rbDuraklat[0]);
            g2.Controls.Add(_rbDuraklat[1]);

            var g3 = Grup(s, "Dağıtım Dosyası Açılırken", 180, 2, 178, 68);
            _chkIhale = Onay(g3, "İhaleyi göster", 10, 16, 160);
            _chkOyun = Onay(g3, "Oyunu göster", 10, 32, 160);
            _chkTumEller = Onay(g3, "Tüm elleri göster", 10, 48, 160);

            var g4 = Grup(s, "Ses", 180, 74, 178, 78);
            _chkIpucuSes = Onay(g4, "İpuçlarını duyur", 10, 16, 160);
            _chkOyunSes = Onay(g4, "Oyun ve ihaleyi duyur", 10, 34, 160);
            _chkSelam = Onay(g4, "Selamlamalar", 10, 52, 160);
        }

        private void OyuncuTurleriSekmesi()
        {
            var s = YeniSekme("Oyuncu Türleri");

            s.Controls.Add(new Label { Text = "Bilgisayar", Left = 100, Top = 8, Width = 70, Height = 16, Font = _yazi });
            s.Controls.Add(new Label { Text = "İnsan", Left = 200, Top = 8, Width = 60, Height = 16, Font = _yazi });

            for (int i = 0; i < 4; i++)
            {
                int y = 32 + i * 32;
                s.Controls.Add(new Label { Text = OyuncuAdlari[i], Left = 20, Top = y, Width = 70, Height = 18, Font = _yazi });

                // Her satırın radyoları kendi panelinde: satırlar birbirinden bağımsız seçilir
                var panel = new Panel { Left = 100, Top = y - 3, Width = 200, Height = 24 };
                _rbBilgisayar[i] = new RadioButton { Left = 22, Top = 3, Width = 18, Height = 18, AutoSize = false };
                _rbInsan[i] = new RadioButton { Left = 122, Top = 3, Width = 18, Height = 18, AutoSize = false };
                panel.Controls.Add(_rbBilgisayar[i]);
                panel.Controls.Add(_rbInsan[i]);
                s.Controls.Add(panel);
            }
        }

        private void ResimlerSekmesi()
        {
            var s = YeniSekme("Oyuncu Resimleri");

            // Sıra ekrandaki gibi: Batı, Kuzey, Doğu, Güney
            int[] sira = { 3, 0, 1, 2 };
            for (int k = 0; k < 4; k++)
            {
                int oyuncu = sira[k];
                int x = 6 + k * 89;

                var kutu = new PictureBox
                {
                    Left = x, Top = 6, Width = 82, Height = 100,
                    BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.White
                };
                kutu.Paint += (snd, pe) =>
                {
                    var pb = (PictureBox)snd;
                    if (pb.Image == null)
                    {
                        var yz = "Resim\nYok";
                        var f = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        using (var kalin = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold))
                            pe.Graphics.DrawString(yz, kalin, Brushes.Black, pb.ClientRectangle, f);
                    }
                };
                s.Controls.Add(kutu);
                _resimKutulari[oyuncu] = kutu;

                s.Controls.Add(new Label
                {
                    Text = OyuncuAdlari[oyuncu] + " Oyuncu", Left = x - 2, Top = 110, Width = 86, Height = 16,
                    TextAlign = ContentAlignment.MiddleCenter, Font = _yazi
                });

                var degistir = new Button { Text = "Değiştir", Left = x + 8, Top = 130, Width = 66, Height = 22, Font = _yazi, UseVisualStyleBackColor = true };
                int idx = oyuncu;
                degistir.Click += (snd, ev) => ResimSec(idx);
                s.Controls.Add(degistir);
            }
        }

        // ───────────────────────────── davranış ─────────────────────────────

        private void ResimSec(int oyuncu)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = OyuncuAdlari[oyuncu] + " için resim seç";
                dlg.Filter = "Resim dosyaları|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Tüm dosyalar|*.*";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                ResimAta(oyuncu, dlg.FileName);
            }
        }

        private void ResimAta(int oyuncu, string yol)
        {
            var kutu = _resimKutulari[oyuncu];
            if (kutu.Image != null) { kutu.Image.Dispose(); kutu.Image = null; }
            _resimYollari[oyuncu] = "";

            if (!string.IsNullOrEmpty(yol) && File.Exists(yol))
            {
                try
                {
                    // Dosyayı kilitlememek için bellekteki kopyayı kullan
                    using (var kaynak = new Bitmap(yol))
                        kutu.Image = new Bitmap(kaynak);
                    _resimYollari[oyuncu] = yol;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Resim yüklenemedi: " + ex.Message, "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            kutu.Invalidate();
        }

        private static void Sec(RadioButton[] dizi, int indeks)
        {
            if (dizi == null || indeks < 0 || indeks >= dizi.Length) return;
            dizi[indeks].Checked = true;
        }

        private static int Secili(RadioButton[] dizi)
        {
            for (int i = 0; i < dizi.Length; i++) if (dizi[i].Checked) return i;
            return 0;
        }

        private void Yukle(OyunPlaniSecenekleri o)
        {
            Sec(_rbKaynak, (int)o.DagitimKaynagi);
            Sec(_rbEnIyi, (int)o.EnIyiEl);
            Sec(_rbDagitici, (int)o.Dagitici);
            _chkYeniNo.Checked = o.YeniBordNumaralari;

            Sec(_rbOyunTuru, (int)o.OyunTuru);
            _chkGorunmez.Checked = o.TekrarGorunmez;
            _chkHemen.Checked = o.TekrarHemen;
            Sec(_rbGecikme, Math.Max(0, Math.Min(2, o.YapayZekaGecikmesiSn)));
            _chkHizli.Checked = o.HizliOtomatikOyna;
            _chkKontratIhale.Checked = o.HerZamanKontratIcinIhale;
            _chkOk.Checked = o.OkuGosterme;
            _chkDondurme.Checked = o.ElleriDondurme;
            _chkTekKart.Checked = o.TekKartiOtomatikOyna;

            Sec(_rbIpucu, (int)o.IpucuGosterimi);
            Sec(_rbDuraklat, o.ElSonrasiDuraklat ? 1 : 0);
            _chkIhale.Checked = o.DosyadanIhaleyiGoster;
            _chkOyun.Checked = o.DosyadanOyunuGoster;
            _chkTumEller.Checked = o.DosyadanTumElleriGoster;
            _chkIpucuSes.Checked = o.IpuclariniSesliDuyur;
            _chkOyunSes.Checked = o.OyunVeIhaleyiSesliDuyur;
            _chkSelam.Checked = o.Selamlamalar;

            for (int i = 0; i < 4; i++)
            {
                bool insan = o.InsanMi != null && i < o.InsanMi.Length && o.InsanMi[i];
                _rbInsan[i].Checked = insan;
                _rbBilgisayar[i].Checked = !insan;
                if (o.ResimYollari != null && i < o.ResimYollari.Length)
                    ResimAta(i, o.ResimYollari[i]);
            }
        }

        private void Kaydet()
        {
            var o = Secenekler;
            o.DagitimKaynagi = (DagitimKaynagi)Secili(_rbKaynak);
            o.EnIyiEl = (EnIyiEl)Secili(_rbEnIyi);
            o.Dagitici = (DagiticiSecimi)Secili(_rbDagitici);
            o.YeniBordNumaralari = _chkYeniNo.Checked;

            o.OyunTuru = (OyunTuru)Secili(_rbOyunTuru);
            o.TekrarGorunmez = _chkGorunmez.Checked;
            o.TekrarHemen = _chkHemen.Checked;
            o.YapayZekaGecikmesiSn = Secili(_rbGecikme);
            o.HizliOtomatikOyna = _chkHizli.Checked;
            o.HerZamanKontratIcinIhale = _chkKontratIhale.Checked;
            o.OkuGosterme = _chkOk.Checked;
            o.ElleriDondurme = _chkDondurme.Checked;
            o.TekKartiOtomatikOyna = _chkTekKart.Checked;

            o.IpucuGosterimi = (IpucuGosterimi)Secili(_rbIpucu);
            o.ElSonrasiDuraklat = Secili(_rbDuraklat) == 1;
            o.DosyadanIhaleyiGoster = _chkIhale.Checked;
            o.DosyadanOyunuGoster = _chkOyun.Checked;
            o.DosyadanTumElleriGoster = _chkTumEller.Checked;
            o.IpuclariniSesliDuyur = _chkIpucuSes.Checked;
            o.OyunVeIhaleyiSesliDuyur = _chkOyunSes.Checked;
            o.Selamlamalar = _chkSelam.Checked;

            for (int i = 0; i < 4; i++)
            {
                o.InsanMi[i] = _rbInsan[i].Checked;
                o.ResimYollari[i] = _resimYollari[i] ?? "";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var kutu in _resimKutulari)
                    if (kutu != null && kutu.Image != null) kutu.Image.Dispose();
                _yazi.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
