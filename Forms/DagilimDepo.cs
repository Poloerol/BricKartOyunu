using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    public partial class DagilimDepo : Form
    {
        public DagilimDepo()
        {
            InitializeComponent();
            ShowInTaskbar = false;   // görev çubuğunda ayrı düğme olmasın (sahip form: oyun masası)
            BtnDepoyaYukle.Click += BtnDepoyaYukle_Click;

            // Var olan .pbn dosyalarını depoya kopyalayan ek düğme ("Ana Sayfaya Dön"ün üstüne yerleşir)
            var btnAktar = new Button
            {
                Text = "PBN Dosyasını Depoya Aktar",
                Location = new Point(BtnAnaSayfa.Left, BtnAnaSayfa.Top),
                Size = BtnAnaSayfa.Size,
                UseVisualStyleBackColor = true
            };
            btnAktar.Click += BtnPbnAktar_Click;
            BtnAnaSayfa.Top += 44;
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + 44);
            Controls.Add(btnAktar);
            BtnDepodanYukle.Click += BtnDepodanYukle_Click;
            BtnDepodanSil.Click += BtnDepodanSil_Click;
            BtnDepodanDuzenle.Click += BtnDepodanDuzenle_Click;

            // X (veya Alt+F4) ile kapatılırsa tüm formlar kapanır, AnaSayfa açılıştaki gibi baştan yüklenir.
            // Programatik kapatmalar (bord yüklenirken vb.) etkilenmez: onların CloseReason değeri UserClosing değildir.
            FormClosing += (s, e) =>
            {
                if (_anaSayfayaDonuluyor || e.CloseReason != CloseReason.UserClosing) return;
                e.Cancel = true;
                _anaSayfayaDonuluyor = true;   // AnaSayfayaDon içindeki Close() bu işleyiciye yeniden takılıp sonsuz döngüye girmesin
                BeginInvoke(new Action(AnaSayfayaDon));
            };
        }

        /// <summary>Depodaki ihale (PBN) dosyalarının tutulduğu klasör: %AppData%\BricKartOyunu\DagilimDeposu</summary>
        public static string DepoKlasoru
        {
            get { return Path.Combine(AyarDeposu.KlasorYolu, "DagilimDeposu"); }
        }

        /// <summary>
        /// El Girişi formunu açar; girilen dört el depodaki seçilen (veya yeni) PBN dosyasına yeni bord olarak eklenir.
        /// Kayıttan sonra başka bord girmek istenirse form yeniden açılır.
        /// </summary>
        private void BtnDepoyaYukle_Click(object sender, EventArgs e)
        {
            while (true)
            {
                Dictionary<Player, List<string>> eller;
                using (var giris = new ElGirisiForm())
                {
                    if (giris.ShowDialog(this) != DialogResult.OK || giris.Eller == null) return;
                    eller = giris.Eller;
                }

                string deal;
                try { deal = PbnDealMetni(eller); }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Eller işlenemedi:\n" + ex.Message, "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string ad = DepoDosyasiAdiSor();
                if (ad == null) return;

                int no;
                try { no = BordEkle(ad, deal); }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Bord depoya kaydedilemedi:\n" + ex.Message, "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (MessageBox.Show(this, "Bord " + no + ", \"" + ad + ".pbn\" dosyasına eklendi.\n\nBaşka bir bord girmek ister misiniz?",
                        "Dağılım Deposu", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes)
                    return;
            }
        }

        /// <summary>El Girişi'nin koltuk başına kart dosya adlarını ("10H.png") PBN Deal metnine çevirir: "N:S.H.D.C E... S... W...".</summary>
        private static string PbnDealMetni(Dictionary<Player, List<string>> eller)
        {
            const string degerSirasi = "AKQJT98765432";
            char[] renkler = { 'S', 'H', 'D', 'C' };
            var sira = new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            var eller4 = new List<string>();
            foreach (var oyuncu in sira)
            {
                List<string> el;
                if (!eller.TryGetValue(oyuncu, out el) || el.Count != 13)
                    throw new InvalidOperationException("Her elde 13 kart olmalıdır.");
                var renk = renkler.ToDictionary(r => r, r => new List<char>());
                foreach (string kart in el)
                {
                    string ad = Path.GetFileNameWithoutExtension(kart);          // ör. "10H"
                    char suit = char.ToUpperInvariant(ad[ad.Length - 1]);
                    string deger = ad.Substring(0, ad.Length - 1);
                    renk[suit].Add(deger == "10" ? 'T' : char.ToUpperInvariant(deger[0]));
                }
                eller4.Add(string.Join(".", renkler.Select(r => renk[r].Count == 0
                    ? "-"
                    : new string(renk[r].OrderBy(c => degerSirasi.IndexOf(c)).ToArray()))));
            }
            return "N:" + string.Join(" ", eller4);
        }

        /// <summary>Bordu depodaki dosyanın sonuna ekler (dosya yoksa oluşturur); eklenen bordun numarasını döndürür.</summary>
        private static int BordEkle(string dosyaAdi, string deal)
        {
            Directory.CreateDirectory(DepoKlasoru);
            string yol = Path.Combine(DepoKlasoru, dosyaAdi + ".pbn");
            int no = 1;
            string metin;
            if (File.Exists(yol))
            {
                no = PbnOkuyucu.Oku(yol).Count + 1;
                metin = File.ReadAllText(yol);
                if (metin.Length > 0 && !metin.EndsWith("\n")) metin += "\r\n";
                metin += "\r\n";
            }
            else metin = "% PBN 2.1\r\n\r\n";

            metin += "[Event \"Bord " + no + "\"]\r\n" +
                     "[Dealer \"" + "NESW"[(no - 1) % 4] + "\"]\r\n" +
                     "[Deal \"" + deal + "\"]\r\n";
            File.WriteAllText(yol, metin, new System.Text.UTF8Encoding(false));
            return no;
        }

        /// <summary>Bordun ekleneceği dosya adını sorar: depodaki bir dosya seçilir ya da yeni ad yazılır. İptalde null.</summary>
        private string DepoDosyasiAdiSor()
        {
            using (var f = new Form())
            {
                f.Text = "Depoya kaydet";
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MinimizeBox = false; f.MaximizeBox = false; f.ShowInTaskbar = false;
                f.ClientSize = new Size(380, 110);

                f.Controls.Add(new Label { Text = "Dosya adı (mevcut dosya seçilirse bord sonuna eklenir):", Location = new Point(10, 10), AutoSize = true });
                var cmb = new ComboBox { Location = new Point(10, 34), Width = 360, DropDownStyle = ComboBoxStyle.DropDown };
                if (Directory.Exists(DepoKlasoru))
                    foreach (string y in Directory.GetFiles(DepoKlasoru, "*.pbn").OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase))
                        cmb.Items.Add(Path.GetFileNameWithoutExtension(y));
                cmb.Text = "Yeni ihale";
                f.Controls.Add(cmb);

                var tamam = new Button { Text = "Kaydet", Location = new Point(190, 70), Size = new Size(85, 28) };
                var iptal = new Button { Text = "Vazgeç", Location = new Point(285, 70), Size = new Size(85, 28), DialogResult = DialogResult.Cancel };
                tamam.Click += (s, e) =>
                {
                    string ad = cmb.Text.Trim();
                    if (ad.EndsWith(".pbn", StringComparison.OrdinalIgnoreCase)) ad = ad.Substring(0, ad.Length - 4).Trim();
                    if (ad.Length == 0 || ad.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    {
                        MessageBox.Show(f, "Geçerli bir dosya adı girin (\\ / : * ? \" < > | karakterleri kullanılamaz).",
                            "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    cmb.Text = ad;
                    f.DialogResult = DialogResult.OK;
                };
                f.Controls.AddRange(new Control[] { tamam, iptal });
                f.AcceptButton = tamam; f.CancelButton = iptal;

                return f.ShowDialog(this) == DialogResult.OK ? cmb.Text : null;
            }
        }

        /// <summary>Seçilen PBN dosyalarını doğrulayıp depo klasörüne kopyalar (aynı ad varsa numaralandırır).</summary>
        private void BtnPbnAktar_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Depoya eklenecek ihale (PBN) dosyaları";
                dlg.Filter = "Taşınabilir Briç Notasyonu (*.pbn)|*.pbn|Tüm dosyalar|*.*";
                dlg.Multiselect = true;
                // Depo klasöründe açılmasın: depodaki dosyalar zaten depoda, aktarılacak dosya dışarıdadır
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try { Directory.CreateDirectory(DepoKlasoru); }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Depo klasörü oluşturulamadı:\n" + ex.Message, "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int eklenen = 0, bord = 0;
                var atlanan = new List<string>();
                foreach (string yol in dlg.FileNames)
                {
                    string ad = Path.GetFileName(yol);
                    if (string.Equals((Path.GetDirectoryName(yol) ?? "").TrimEnd('\\'), DepoKlasoru.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    {
                        atlanan.Add(ad + " (zaten depoda)");
                        continue;
                    }
                    try
                    {
                        int adet = PbnOkuyucu.Oku(yol).Count;
                        if (adet == 0) { atlanan.Add(ad + " (okunabilir bord yok)"); continue; }
                        File.Copy(yol, BenzersizHedef(ad));
                        eklenen++; bord += adet;
                    }
                    catch (Exception ex) { atlanan.Add(ad + " (" + ex.Message + ")"); }
                }

                string mesaj = eklenen + " dosya depoya eklendi (" + bord + " bord).";
                if (atlanan.Count > 0) mesaj += "\n\nEklenemeyenler:\n• " + string.Join("\n• ", atlanan);
                MessageBox.Show(this, mesaj, "Dağılım Deposu", MessageBoxButtons.OK,
                    atlanan.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Depodaki dosyaları (ad, bord sayısı, eklenme tarihi) listeleyen seçim penceresi.
        /// Seçilen dosyanın tam yolunu döndürür; iptal ya da boş depoda null. Sil/Düzenle de bunu kullanır.
        /// </summary>
        private string DepodanSec(string baslik, string onayMetni)
        {
            string[] dosyalar = Directory.Exists(DepoKlasoru) ? Directory.GetFiles(DepoKlasoru, "*.pbn") : new string[0];
            if (dosyalar.Length == 0)
            {
                MessageBox.Show(this, "Depoda hiç ihale dosyası yok.\nÖnce \"Depoya İhale Dosyası Girin\" ile dosya ekleyin.",
                    "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            using (var f = new Form())
            {
                f.Text = baslik;
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MinimizeBox = false; f.MaximizeBox = false; f.ShowInTaskbar = false;
                f.ClientSize = new Size(500, 330);

                var lv = new ListView { Location = new Point(10, 10), Size = new Size(480, 265), View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false };
                lv.Columns.Add("Dosya", 270);
                lv.Columns.Add("Bord", 55, HorizontalAlignment.Right);
                lv.Columns.Add("Eklenme", 135);
                foreach (string yol in dosyalar.OrderBy(x => Path.GetFileName(x), StringComparer.CurrentCultureIgnoreCase))
                {
                    var oge = new ListViewItem(Path.GetFileName(yol)) { Tag = yol };
                    oge.SubItems.Add(PbnOkuyucu.Oku(yol).Count.ToString());
                    oge.SubItems.Add(File.GetCreationTime(yol).ToString("dd.MM.yyyy HH:mm"));
                    lv.Items.Add(oge);
                }

                var tamam = new Button { Text = onayMetni, Location = new Point(300, 288), Size = new Size(90, 30), DialogResult = DialogResult.OK, Enabled = false };
                var iptal = new Button { Text = "Vazgeç", Location = new Point(400, 288), Size = new Size(90, 30), DialogResult = DialogResult.Cancel };
                lv.SelectedIndexChanged += (s, e) => tamam.Enabled = lv.SelectedItems.Count == 1;
                lv.DoubleClick += (s, e) => { if (lv.SelectedItems.Count == 1) f.DialogResult = DialogResult.OK; };
                f.Controls.AddRange(new Control[] { lv, tamam, iptal });
                f.AcceptButton = tamam; f.CancelButton = iptal;

                if (f.ShowDialog(this) != DialogResult.OK || lv.SelectedItems.Count != 1) return null;
                return (string)lv.SelectedItems[0].Tag;
            }
        }

        /// <summary>
        /// "Oynanacak Bord Seç" penceresi: dosyadaki bordları (numara, ad, açıklama) listeler, altta bulunan bord sayısı yazar.
        /// Seçilen bordun sıfır tabanlı indeksini döndürür; İptal'de -1.
        /// </summary>
        private int BordSec(List<PbnBord> bordlar)
        {
            string[] dagiticiAdi = { "Kuzey", "Doğu", "Güney", "Batı" };
            var satirlar = new List<string[]>();
            foreach (var b in bordlar)
            {
                string acik = !string.IsNullOrEmpty(b.Aciklama) ? b.Aciklama
                            : b.DagiticiVar ? "Dağıtıcı: " + dagiticiAdi[Array.IndexOf(new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati }, b.Dagitici)]
                            : "";
                satirlar.Add(new[] { b.Ad, acik });
            }
            return BordSec("Oynanacak Bord Seç", satirlar);
        }

        /// <summary>Bord listesi penceresi (satır: ad, açıklama). Seçilen bordun sıfır tabanlı indeksi; İptal'de -1.</summary>
        private int BordSec(string baslik, List<string[]> bordlar)
        {
            using (var f = new Form())
            {
                f.Text = baslik;
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MinimizeBox = false; f.MaximizeBox = false; f.ShowInTaskbar = false;
                f.ClientSize = new Size(420, 270);

                var lv = new ListView { Location = new Point(10, 10), Size = new Size(400, 215), View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false, GridLines = true };
                lv.Columns.Add("Bord", 45, HorizontalAlignment.Right);
                lv.Columns.Add("Ad", 90);
                lv.Columns.Add("Açıklama", 245);
                for (int i = 0; i < bordlar.Count; i++)
                {
                    var oge = new ListViewItem((i + 1).ToString());
                    oge.SubItems.Add(bordlar[i][0]);
                    oge.SubItems.Add(bordlar[i][1]);
                    lv.Items.Add(oge);
                }
                f.Controls.Add(lv);
                f.Controls.Add(new Label { Text = "Bulunan bord: " + bordlar.Count, Location = new Point(10, 242), AutoSize = true });

                var tamam = new Button { Text = "Tamam", Location = new Point(235, 235), Size = new Size(85, 28), DialogResult = DialogResult.OK };
                var iptal = new Button { Text = "İptal", Location = new Point(325, 235), Size = new Size(85, 28), DialogResult = DialogResult.Cancel };
                f.Controls.AddRange(new Control[] { tamam, iptal });
                f.AcceptButton = tamam; f.CancelButton = iptal;
                lv.DoubleClick += (s, e) => { if (lv.SelectedIndices.Count == 1) f.DialogResult = DialogResult.OK; };
                lv.Items[0].Selected = true;

                if (f.ShowDialog(this) != DialogResult.OK || lv.SelectedIndices.Count != 1) return -1;
                return lv.SelectedIndices[0];
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr FindWindow(string sinif, string baslik);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr pencere, out RECT r);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr pencere, IntPtr sonra, int x, int y, int g, int y2, uint bayrak);

        /// <summary>Standart "Aç" penceresi (depo klasöründe açılır). Yalnızca depodaki dosya kabul edilir; iptalde/uygunsuzsa null.</summary>
        private string DepoDosyasiAc()
        {
            string yol;
            try { Directory.CreateDirectory(DepoKlasoru); } catch (Exception) { }
            using (var dlg = new OpenFileDialog())
            using (var zamanlayici = new System.Windows.Forms.Timer { Interval = 30 })
            {
                dlg.Title = "Aç";
                dlg.InitialDirectory = DepoKlasoru;
                dlg.Filter = "Taşınabilir Briç Notasyonu (*.pbn)|*.pbn";
                dlg.CheckFileExists = true;
                dlg.Multiselect = false;

                // Standart dosya penceresinin konumu ayarlanamaz; açılınca bulunup ekranın çalışma alanının ortasına taşınır
                Rectangle alan = Screen.FromControl(this).WorkingArea;
                zamanlayici.Tick += (s, e) =>
                {
                    IntPtr pencere = FindWindow("#32770", "Aç");
                    RECT r;
                    if (pencere == IntPtr.Zero || !GetWindowRect(pencere, out r)) return;
                    zamanlayici.Stop();
                    int g = r.Right - r.Left, y = r.Bottom - r.Top;
                    SetWindowPos(pencere, IntPtr.Zero, alan.Left + (alan.Width - g) / 2, alan.Top + (alan.Height - y) / 2, 0, 0, 0x0001 | 0x0004 | 0x0010);   // SIZE | ZORDER | NOACTIVATE yok sayılır
                };
                zamanlayici.Start();
                DialogResult sonuc = dlg.ShowDialog(this);
                zamanlayici.Stop();
                if (sonuc != DialogResult.OK) return null;
                yol = dlg.FileName;
            }

            // Başka klasördeki dosya için "PBN Dosyasını Depoya Aktar" kullanılır
            string klasor = Path.GetDirectoryName(yol) ?? "";
            if (!string.Equals(klasor.TrimEnd('\\'), DepoKlasoru.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "Yalnızca depodaki dosyalar açılabilir.\nBaşka bir klasördeki dosya için önce \"PBN Dosyasını Depoya Aktar\" düğmesini kullanın.",
                    "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return yol;
        }

        private void BtnDepodanYukle_Click(object sender, EventArgs e)
        {
            string yol = DepoDosyasiAc();
            if (yol == null) return;

            var bordlar = PbnOkuyucu.Oku(yol);
            int adet = bordlar.Count;
            if (adet == 0)
            {
                MessageBox.Show(this, "Bu dosyada okunabilir bord bulunamadı.", "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // "Oynanacak Bord Seç" penceresi: dosyada tek bord varsa da gösterilir
            int secilen = BordSec(bordlar);
            if (secilen < 0) return;

            // Seçilen bord oyun masasına dağıtılır; bu pencere (ve diğer depo pencereleri) kapanır, masa
            // "Eller girildiği gibi ihale edilip oynansın mı?" diye sorar.
            var masa = (this.Owner as BricOyna) ?? Application.OpenForms.OfType<BricOyna>().FirstOrDefault();
            if (masa == null)
            {
                MessageBox.Show(this, "Bord yalnızca oyun masasından (Araçlar > Dağılım Deposu) yüklenebilir.", "Dağılım Deposu",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var bord = bordlar[secilen];
            int bordNo = secilen + 1;
            this.Close();
            masa.BeginInvoke(new Action(() => masa.DepoBordunuYukle(bord, bordNo)));
        }

        /// <summary>Depodan seçilen dosyayı onay alarak siler; oyun planında kullanılıyorsa kaynağı rastgeleye döndürür.</summary>
        private void BtnDepodanSil_Click(object sender, EventArgs e)
        {
            string yol = DepodanSec("Depodan ihale dosyası sil", "Sil");
            if (yol == null) return;

            string ad = Path.GetFileName(yol);
            int adet = PbnOkuyucu.Oku(yol).Count;
            var plan = AnaSayfa.OyunPlani;
            bool kullanimda = string.Equals(plan.PbnYolu, yol, StringComparison.OrdinalIgnoreCase);

            string soru = "\"" + ad + "\" (" + adet + " bord) depodan kalıcı olarak silinecek.\nBu işlem geri alınamaz; depoya eklemeden önceki özgün dosyanıza dokunulmaz.";
            if (kullanimda) soru += "\n\nBu dosya şu an oyun planında kullanılıyor; silinirse dağıtım kaynağı \"Rastgele\" olarak değişir.";
            if (MessageBox.Show(this, soru + "\n\nSilinsin mi?", "Dağılım Deposu", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            try { File.Delete(yol); }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Dosya silinemedi:\n" + ex.Message, "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string mesaj = "\"" + ad + "\" depodan silindi.";
            if (kullanimda)
            {
                plan.PbnYolu = "";
                plan.DagitimKaynagi = DagitimKaynagi.Rastgele;
                if (!plan.Kaydet()) mesaj += "\n\nUyarı: oyun planı diske kaydedilemedi.";
                else mesaj += "\nOyun planı dağıtım kaynağı \"Rastgele\" yapıldı.";
            }
            MessageBox.Show(this, mesaj, "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ───────────────────────── Depodan düzenle ─────────────────────────

        /// <summary>PBN dosyasındaki tek bord bloğu (ham satırlar korunur; Auction/Result vb. etiketler kaybolmaz).</summary>
        private class PbnBlok
        {
            public List<string> Satirlar = new List<string>();
            public string Ad = "", Dagitici = "", Deal = "";
            public bool DealVar;
        }

        /// <summary>Düzenleme için ham metin düzeyinde okunan PBN dosyası: başlık + bord blokları + kuyruk.</summary>
        private class PbnDosyasi
        {
            private static readonly System.Text.RegularExpressions.Regex Etiket =
                new System.Text.RegularExpressions.Regex("^\\[(\\w+)\\s+\"(.*)\"\\]\\s*$");

            public List<string> Baslik = new List<string>();
            public List<PbnBlok> Bordlar = new List<PbnBlok>();
            public List<string> Kuyruk = new List<string>();

            public static PbnDosyasi Oku(string yol)
            {
                var d = new PbnDosyasi();
                PbnBlok blok = null;
                foreach (string satir in File.ReadAllLines(yol))
                {
                    var m = Etiket.Match(satir.Trim());
                    if (m.Success)
                    {
                        string tag = m.Groups[1].Value, deger = m.Groups[2].Value;
                        // Okuyucuyla aynı kural: Event/Deal, dağıtımı olan bir bloktan sonra gelirse yeni bord başlar
                        if (blok == null || ((tag == "Event" || tag == "Deal") && blok.DealVar))
                        {
                            blok = new PbnBlok();
                            d.Bordlar.Add(blok);
                        }
                        if (tag == "Event") blok.Ad = deger;
                        else if (tag == "Dealer") blok.Dagitici = deger;
                        else if (tag == "Deal") { blok.Deal = deger; blok.DealVar = true; }
                    }
                    if (blok == null) d.Baslik.Add(satir); else blok.Satirlar.Add(satir);
                }
                // Dağıtımı olmayan son blok bord sayılmaz; olduğu gibi dosya sonunda korunur
                if (d.Bordlar.Count > 0 && !d.Bordlar[d.Bordlar.Count - 1].DealVar)
                {
                    d.Kuyruk.AddRange(d.Bordlar[d.Bordlar.Count - 1].Satirlar);
                    d.Bordlar.RemoveAt(d.Bordlar.Count - 1);
                }
                return d;
            }

            public string Yaz()
            {
                var tum = new List<string>(Baslik);
                foreach (var b in Bordlar) tum.AddRange(b.Satirlar);
                tum.AddRange(Kuyruk);
                return string.Join("\r\n", tum) + "\r\n";
            }
        }

        private static string BenzersizHedef(string dosyaAdi)
        {
            string ad = Path.GetFileNameWithoutExtension(dosyaAdi), uz = Path.GetExtension(dosyaAdi);
            string hedef = Path.Combine(DepoKlasoru, dosyaAdi);
            for (int i = 2; File.Exists(hedef); i++)
                hedef = Path.Combine(DepoKlasoru, ad + " (" + i + ")" + uz);
            return hedef;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AnaSayfayaDon();
        }

        private bool _anaSayfayaDonuluyor;

        /// <summary>Dağılım Deposu ve (varsa) oyun masasını kapatır; AnaSayfa açılıştaki haline döner.</summary>
        private void AnaSayfayaDon()
        {
            _anaSayfayaDonuluyor = true;
            // Dağılım Deposu oyun masası üzerinde açıldıysa masayı kapat: AnaSayfa kendiliğinden geri gelir
            var masa = (this.Owner as BricOyna) ?? Application.OpenForms.OfType<BricOyna>().FirstOrDefault();
            if (masa != null)
            {
                this.Close();
                masa.Close();
                return;
            }

            // Tip bazlı arama: form adı değişse de bozulmaz
            var anaSayfa = (this.Owner as AnaSayfa)
                           ?? Application.OpenForms.OfType<AnaSayfa>().FirstOrDefault();
            if (anaSayfa != null)
            {
                anaSayfa.ResetToInitialState(); // AnaSayfa’yı ilk haline döndür
                anaSayfa.Show();
            }
            this.Close();
        }

        private void DagilimDepo_Load(object sender, EventArgs e)
        {

        }
    }
}
