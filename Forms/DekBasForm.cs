using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    public partial class DekBasForm : Form
    {
        private readonly BricOyna _bricOyna;
        public DekBasForm(BricOyna bricOyna)
        {
            InitializeComponent();
            _bricOyna = bricOyna;
            this.StartPosition = FormStartPosition.Manual;

            // 🔹 Titreme önlemi: standart double-buffer kalıbı.
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        private void DekBasForm_Load(object sender, EventArgs e)
        {
            // Hangi yoldan açılırsa açılsın (ilk açılış, Sonraki el, 4 Pas) form,
            // BricOyna.playZonePanel'in tam ortasında görünür.
            if (_bricOyna?.playZonePanel != null)
            {
                Point ekran = _bricOyna.playZonePanel.PointToScreen(Point.Empty);
                Size boyut = _bricOyna.playZonePanel.Size;
                this.Location = new Point(
                    ekran.X + (boyut.Width - this.Width) / 2,
                    ekran.Y + (boyut.Height - this.Height) / 2
                );
            }
        }

        private DeklarasyonForm _dekForm;

        private void BtnDeklarasyon_Click(object sender, EventArgs e)
        {
            if (_dekForm == null || _dekForm.IsDisposed)
                _dekForm = new DeklarasyonForm(_bricOyna);

            if (!_dekForm.Visible)
            {
                Point ekran = _bricOyna.playZonePanel.PointToScreen(Point.Empty);
                Size boyut = _bricOyna.playZonePanel.Size;
                _dekForm.Location = new Point(
                    ekran.X + (boyut.Width - _dekForm.Width) / 2,
                    ekran.Y + (boyut.Height - _dekForm.Height) / 2
                );

                // 🔹 YENİ: DeklarasyonForm açılmadan hemen önce 7 butonu aktif et
                _bricOyna.DeklarasyonAsamasindaButonlariAktifEt();

                // DeklarasyonForm'u göster
                _dekForm.Show(_bricOyna);
            }
            else
            {
                _dekForm.BringToFront();
            }

            // DekBasForm'u kapat
            this.Close();
        }

        private void BtnOyundanCik_Click(object sender, EventArgs e)
        {
            // 🔹 DÜZELTME: Burada AnaSayfa'yı AYRICA aramıyor/oluşturmuyoruz.
            // BricOyna.Close() zaten şu iki yoldan mevcut (var olan) AnaSayfa örneğini
            // gösteriyor:
            //   1) AnaSayfa.OrtaButon_Click içindeki ShowDialog() çağrısı dönünce
            //      çalışan kod (ResetToInitialState + Show).
            //   2) BricOyna_FormClosing içindeki güvenlik ağı.
            // Burada AYRICA "bulamazsam yeni oluştur" mantığı çalıştırmak, bu iki yolla
            // aynı anda/yarışarak çalışıp EKRANDA 2 TANE AnaSayfa PENCERESİ açılmasına
            // sebep oluyordu.

            var forms = Application.OpenForms.Cast<Form>().ToList();

            // BricOyna'yı kapat
            var bricFormu = forms.OfType<BricOyna>().FirstOrDefault();
            bricFormu?.Close();

            // DekBasForm'u kapat (kendisi)
            this.Close();
        }

        private void BtnSonrakiDagilim_Click(object sender, EventArgs e)
        {
            // Açık olan BricOyna formunu bul
            BricOyna bricFormu = Application.OpenForms.OfType<BricOyna>().FirstOrDefault();

            if (bricFormu != null)
            {
                // 🔹 BricOyna.BtnNextHand_Click kaldırıldı; bunun yerine Ayarlar
                // menüsündeki "Sonraki el" ile aynı onaylı/sıfırdan-başlatan akış
                // kullanılıyor (bkz. BricOyna.SonrakiElIsteği).
                bricFormu.SonrakiElIsteği();
            }
            else
            {
                MessageBox.Show("BricOyna formu bulunamadı!", "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnElDegistir_Click(object sender, EventArgs e)
        {
            // ElleriDegistir formunu oluştur
            ElleriDegistir elDegistirForm = new ElleriDegistir();

            // playZonePanel'in ekran koordinatlarını al
            Point panelEkranKonumu = _bricOyna.playZonePanel.PointToScreen(Point.Empty);
            Size panelBoyutu = _bricOyna.playZonePanel.Size;

            // Formu manuel konumlandır
            elDegistirForm.StartPosition = FormStartPosition.Manual;

            // Panelin tam ortasına yerleştir
            elDegistirForm.Location = new Point(
                panelEkranKonumu.X + (panelBoyutu.Width - elDegistirForm.Width) / 2,
                panelEkranKonumu.Y + (panelBoyutu.Height - elDegistirForm.Height) / 2
            );

            // Formu göster (owner olarak BricOyna veriliyor; aksi halde ElleriDegistir
            // içindeki "this.Owner is BricOyna" kontrolü hep false dönüp BtnOK_Click'te
            // DeklarasyonForm'un hiç açılmamasına sebep oluyordu).
            elDegistirForm.Show(_bricOyna);

            // Bu formu kapat
            this.Close();
        }
    }
}