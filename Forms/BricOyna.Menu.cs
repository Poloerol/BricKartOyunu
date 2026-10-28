using BricKartOyunu.Class;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;


namespace BricKartOyunu
{
    // Menü ve düğme olayları
    public partial class BricOyna
    {
        private void ÇıkışToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void MasaRengiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    AnaSayfa.MasaRengi = cd.Color;
                    this.BackColor = AnaSayfa.MasaRengi;
            DurumCubuguRenginiSabitle();
            PlayZoneRenginiGuncelle();
                }
            }
        }

        // 🔹 "Sonraki el" akışı: onay mesajı sorar, Evet ise BricOyna üzerindeki tüm
        // formları kapatıp senaryoyu sıfırdan başlatır. Hem Ayarlar menüsündeki
        // sonrakiElToolStripMenuItem hem de DekBasForm'daki BtnSonrakiDagilim butonu
        // bu tek metodu çağırıyor; böylece iki tetikleyici de aynı davranışa sahip.
        public void SonrakiElIsteği()
        {

            // 🔹 Masadaki kartları temizle
            MasadakiKartlariTemizle();

            foreach (Form f in Application.OpenForms.Cast<Form>()
                         .Where(f => f != this && !(f is AnaSayfa))
                         .ToArray())
            {
                if (f is DeklarasyonForm dek)
                    dek.IzinliKapat();
                else if (f is DekBasForm dekBas)
                    dekBas.ProgramatikKapat();
                else
                    f.Close();
            }

            DortPasSonrasiYeniEliBaslat();
        }

        private void SonrakiElToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SonrakiElIsteği();
        }

        private void HepsiniGösterToolStripMenuItem_Click(object sender, EventArgs e) { BtnHepsi_Click(sender, e); }
        private void NSGösterToolStripMenuItem_Click(object sender, EventArgs e) { BtnNS_Click(sender, e); }
        private void EWGösterToolStripMenuItem_Click(object sender, EventArgs e) { BtnEW_Click(sender, e); }
        private void OynayanOyuncuToolStripMenuItem_Click(object sender, EventArgs e) { BtnGuney_Click(sender, e); }

        // 🔹 Toolstrip "Oyundan Çık": tüm yan formları (deklarasyon, dekbaş vb.) kapatır,
        // ardından BricOyna'yı kapatır. AnaSayfa, BricOyna'yı ShowDialog ile açtığı için
        // dönüşte kendini gösterip ResetToInitialState() ile baştan başlar.
        private void BtnOyundanExit_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms.Cast<Form>()
                         .Where(f => f != this && !(f is AnaSayfa))
                         .ToArray())
            {
                if (f is DeklarasyonForm dek)
                    dek.IzinliKapat();
                else if (f is DekBasForm dekBas)
                    dekBas.ProgramatikKapat();
                else
                    f.Close();
            }

            this.Close();
        }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            // 🔹 AnaSayfa'yı burada Show() ETMİYORUZ.
            // Bu formu açan AnaSayfa.OrtaButon_Click zaten ShowDialog() dönüşünde
            // AnaSayfa'yı gösterip yeniden düzenliyor (ResetToInitialState). Burada da
            // Show() çağırmak aynı işlemi iki kez tetikleyip fazladan titremeye yol açıyordu.
            this.Close();
        }

        private void BricOyna_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Timer'ları temizle (kapanışta hata vermemesi için)
            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            if (e.CloseReason == CloseReason.ApplicationExitCall ||
                e.CloseReason == CloseReason.WindowsShutDown)
                return;

            // Güvenlik ağı: Form, BtnGeri dışında bir yoldan (ör. pencere X butonu)
            // kapatılırsa AnaSayfa'nın ekranda görünür olduğundan emin ol.
            //
            // NOT: Eskiden Application.OpenForms["AnaSayfa"] şeklinde string ile arama
            // yapılıyordu; bu, Designer'da form adı değiştiğinde sessizce bozuluyordu.
            // Artık tipe göre arıyoruz — daha güvenli ve refactor dostu.
            var anaSayfa = Application.OpenForms.OfType<AnaSayfa>().FirstOrDefault();

            if (anaSayfa != null && !anaSayfa.Visible)
            {
                anaSayfa.ShowInTaskbar = true;
                anaSayfa.Show();
            }
        }

        private void BtnHepsi_Click(object sender, EventArgs e)
        {
            hepsiniGösterToolStripMenuItem.Checked = !hepsiniGösterToolStripMenuItem.Checked;
            BtnHepsi.Checked = hepsiniGösterToolStripMenuItem.Checked;
            BtnEW.Checked = false;
            BtnNS.Checked = false;
            BtnGuney.Checked = false;
            eWGösterToolStripMenuItem.Checked = false;
            nSGösterToolStripMenuItem.Checked = false;

            this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
            try
            {
                if (hepsiniGösterToolStripMenuItem.Checked)
                    HepsiniGoster();
                else
                    SadeceGuneyiGoster();
            }
            finally
            {
                this.ResumeLayout(true); EkranYardimci.CizimiAc(this);
            }
        }

        private void BtnEW_Click(object sender, EventArgs e)
        {
            BtnEW.Checked = !BtnEW.Checked;
            if (BtnEW.Checked)
            {
                BtnGuney.Checked = false; BtnHepsi.Checked = false; BtnNS.Checked = false;
                hepsiniGösterToolStripMenuItem.Checked = false; nSGösterToolStripMenuItem.Checked = false; eWGösterToolStripMenuItem.Checked = true;
                this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
                try
                {
                    KartYerlesimiHazirla();
                    Image backImg = GetBackImage();
                    if (backImg != null)
                        for (int j = 0; j < kuzeyKartlar.Length; j++) if (kuzeyKartlar[j] != null) kuzeyKartlar[j].Image = backImg;
                    BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);
                    int doguXPos = doguKartlar[0] != null ? doguKartlar[0].Left : 930;
                    int ewStartingY = GetCenteredEWStartingY();
                    BricStandartlarinaGoreDiz(doguKartlar, doguEl, doguXPos, ewStartingY);
                    BricStandartlarinaGoreDiz(batiKartlar, batiEl, 40, ewStartingY);
                    SetupPlayerBadges();
                }
                finally { this.ResumeLayout(true); EkranYardimci.CizimiAc(this); }
            }
            else
            {
                eWGösterToolStripMenuItem.Checked = false;
                BtnGuney.Checked = false;

                this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
                try
                {
                    SadeceGuneyiGoster();
                }
                finally
                {
                    this.ResumeLayout(true); EkranYardimci.CizimiAc(this);
                }
            }
        }


        private void BtnNS_Click(object sender, EventArgs e)
        {
            BtnNS.Checked = !BtnNS.Checked;
            if (BtnNS.Checked)
            {
                BtnGuney.Checked = false; BtnHepsi.Checked = false; BtnEW.Checked = false;
                hepsiniGösterToolStripMenuItem.Checked = false; eWGösterToolStripMenuItem.Checked = false; nSGösterToolStripMenuItem.Checked = true;
                this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
                try
                {
                    KartYerlesimiHazirla();
                    Image backImg = GetBackImage();
                    if (backImg != null)
                    {
                        for (int j = 0; j < batiKartlar.Length; j++) if (batiKartlar[j] != null) batiKartlar[j].Image = backImg;
                        for (int j = 0; j < doguKartlar.Length; j++) if (doguKartlar[j] != null) doguKartlar[j].Image = backImg;
                    }
                    BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, 250, _kuzeyY, false, true);
                    BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);
                    SetupPlayerBadges();
                }
                finally { this.ResumeLayout(true); EkranYardimci.CizimiAc(this); }
            }
            else
            {
                nSGösterToolStripMenuItem.Checked = false;
                BtnGuney.Checked = false;

                this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
                try
                {
                    SadeceGuneyiGoster();
                }
                finally
                {
                    this.ResumeLayout(true); EkranYardimci.CizimiAc(this);
                }
            }

        }
        private void BtnGuney_Click(object sender, EventArgs e)
        {
            // Aktif oyuncu insan oyuncu ise bu buton zaten inaktif olmalı, işlem yapma
            if (activePlayer == _humanPlayer) return;

            BtnGuney.Checked = !BtnGuney.Checked;
            BtnHepsi.Checked = false;
            BtnEW.Checked = false;
            BtnNS.Checked = false;
            hepsiniGösterToolStripMenuItem.Checked = false;
            eWGösterToolStripMenuItem.Checked = false;
            nSGösterToolStripMenuItem.Checked = false;

            this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
            try
            {
                if (BtnGuney.Checked)
                {
                    // İnsan oyuncu (Güney) + aktif oyuncunun eli açık
                    KartYerlesimiHazirla();

                    Image backImg = GetBackImage();
                    if (backImg != null)
                    {
                        for (int i = 0; i < 13; i++)
                        {
                            if (kuzeyKartlar[i] != null) kuzeyKartlar[i].Image = backImg;
                            if (batiKartlar[i] != null) batiKartlar[i].Image = backImg;
                            if (doguKartlar[i] != null) doguKartlar[i].Image = backImg;
                        }
                    }

                    BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);

                    switch (activePlayer)
                    {
                        case Player.Kuzey:
                            BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, 250, _kuzeyY, false, true);
                            break;
                        case Player.Bati:
                            BricStandartlarinaGoreDiz(batiKartlar, batiEl, 40, GetCenteredEWStartingY());
                            break;
                        case Player.Dogu:
                            BricStandartlarinaGoreDiz(doguKartlar, doguEl, doguKartlar[0]?.Left ?? 930, GetCenteredEWStartingY());
                            break;
                    }

                    SetupPlayerBadges();
                }
                else
                {
                    SadeceGuneyiGoster();
                }
            }
            finally
            {
                this.ResumeLayout(true); EkranYardimci.CizimiAc(this);
            }
        }


        private void IhaleGösterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IhaleGecmisiniGoster();
        }



        public List<Card> GetGuneyElinKartlari()
        {
            // Lütfen buradaki 'guneyElinKartlari' veya 'guneyOyuncu.Kartlar' 
            // değişken adını BricOyna formunuzda Güney'in kartlarını tuttuğunuz listenin adıyla değiştirin.
            return guneyEl;
        }



        private void oynananElleriGösterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OynananElleriGoster();
        }

        private void BricOyna_Load_1(object sender, EventArgs e)
        {

        }

        // ═════════════════════════════════════════════════════════════
        // İPUCU: oynanması gereken kartı sarı işaretle
        // ═════════════════════════════════════════════════════════════
        private PictureBox _ipucuKarti;

        // Kullanıcı oyun esnasında ihale geçmişini görmek istediğinde çağrılacak metot
        public void IhaleGecmisiniGoster()
        {
            if (_sonIhaleGecmisi == null || _sonIhaleGecmisi.Count == 0)
            {
                MessageBox.Show("Henüz tamamlanmış bir ihale bulunmuyor.", "İhale Geçmişi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (IhaleGecmisiForm form = new IhaleGecmisiForm(_sonIhaleGecmisi, _ihaleBaslangicOyuncusu))
            {
                form.ShowDialog(this);
            }
        }

        private void IhaleYorumuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1) Form zaten açık mı?
            foreach (Form f in Application.OpenForms)
            {
                if (f is IhaleYorumuForm)
                {
                    f.BringToFront();
                    return;
                }
            }

            // 2) Yeni formu oluştur
            var form = new IhaleYorumuForm(new List<string>(), "Kuzey");

            // 3) Manuel konum ayarı
            form.StartPosition = FormStartPosition.Manual;

            // ⚠️ BURAYI KENDİ EKRANINA GÖRE AYARLA
            // Aşağıdaki değerler 2. resimdeki gibi hizalamak için tahmini değerlerdir.
            // Eğer form çok sağda/solda/yukarıda/aşağıda açılırsa bu sayıları değiştir.
            form.Location = new Point(755, 450);

            // 4) Formu göster
            form.Show(this);
        }

        /// <summary>
        /// Oynanan tüm elleri bir formda gösterir.
        /// Deal Complete formundaki "Oyunu Göster" butonu da bunu çağırır.
        /// </summary>
        public void OynananElleriGoster()
        {
            if ((_oynananTumEller == null || _oynananTumEller.Count == 0) &&
                (_oncekiBordEller == null || _oncekiBordEller.Count == 0))
            {
                MessageBox.Show(
                    "Henüz oynanmış bir el bulunmuyor.",
                    "Oynanan Eller",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Modal değil: sonuç mesajı / DealCompleteForm açıkken de kullanılabilir.
            var liste = _oynananTumEller;
            string baslik = null;
            if (liste.Count == 0 && _oncekiBordEller != null && _oncekiBordEller.Count > 0)
            {
                liste = _oncekiBordEller; baslik = _oncekiBordBaslik;
            }

            var acik = Application.OpenForms.OfType<OynananElleriGosterForm>().FirstOrDefault();
            if (acik != null)
            {
                acik.Guncelle(liste, baslik);
                acik.Show(); acik.BringToFront(); acik.Activate();
                return;
            }

            var form = new OynananElleriGosterForm(liste, baslik);
            form.StartPosition = FormStartPosition.Manual;
            try
            {
                var pz = playZonePanel.PointToScreen(new Point(10, 10));
                var wa = Screen.FromControl(this).WorkingArea;
                int x = Math.Max(wa.Left, Math.Min(pz.X, wa.Right - form.Width));
                int y = Math.Max(wa.Top, Math.Min(pz.Y, wa.Bottom - form.Height));
                form.Location = new Point(x, y);
            }
            catch { form.StartPosition = FormStartPosition.CenterParent; }
            form.Show(this);
        }
    }
}
