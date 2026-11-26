using BricKartOyunu.Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    public class PbnDonusturucuForm : Form
    {
        private static readonly string[] AyAdlari = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

        private const string SiteKoku = "https://clubs.vugraph.com";
        private const string KulupListesiUrl = SiteKoku + "/?lang=tr";
        private const string VarsayilanKulup = "marmaris";
        private static readonly string KulupDosyasi = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PBNDonusturucu", "kulup.txt");

        private TextBox txtLink;
        private ComboBox cmbKulup, cmbAy;
        private NumericUpDown nudYil;
        private Button btnKulup, btnTakvim, btnLink, btnCiftler, btnKaydet;
        private ListBox lstTurnuva, lstCift;
        private CheckBox chkSaha;
        private Label lblDurum, lblTurnuvaBilgi;

        private HttpClient client;
        private TurnuvaBilgisi turnuva;

        public PbnDonusturucuForm()
        {
            // Site TLS 1.2 ister; eski varsayılanlara güvenme
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            ArayuzuOlustur();
            FormClosed += (s, e) => client.Dispose();
            Shown += async (s, e) => await KuluplariGetir();
        }

        // ───────────────────────── Arayüz ─────────────────────────

        private void ArayuzuOlustur()
        {
            Text = "Vugraph Briç Kulüpleri PBN Dönüştürücü";
            ClientSize = new Size(700, 640);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 9.5f);

            var kalin = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            var lblKulup = new Label { Text = "Kulüp:", Location = new Point(20, 22), AutoSize = true, Font = kalin };
            cmbKulup = new ComboBox
            {
                Location = new Point(75, 18), Width = 495, DropDownStyle = ComboBoxStyle.DropDown, DropDownHeight = 300,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems
            };
            cmbKulup.SelectionChangeCommitted += (s, e) => KulupDegisti();
            cmbKulup.Leave += (s, e) => KulupDegisti();
            btnKulup = new Button { Text = "Kulüpleri Yenile", Location = new Point(580, 16), Width = 100, Height = 28 };
            btnKulup.Click += async (s, e) => await KuluplariGetir();

            var lblTakvim = new Label { Text = "1) Turnuva takvimi:", Location = new Point(20, 58), AutoSize = true, Font = kalin };
            cmbAy = new ComboBox { Location = new Point(20, 84), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAy.Items.AddRange(AyAdlari);
            cmbAy.SelectedIndex = DateTime.Now.Month - 1;
            nudYil = new NumericUpDown { Location = new Point(150, 84), Width = 70, Minimum = 2005, Maximum = 2100, Value = DateTime.Now.Year };
            btnTakvim = new Button { Text = "Takvimi Getir", Location = new Point(232, 82), Width = 120, Height = 28 };
            btnTakvim.Click += async (s, e) => await TakvimiGetir();

            lstTurnuva = new ListBox { Location = new Point(20, 118), Width = 660, Height = 120 };
            lstTurnuva.DoubleClick += async (s, e) => await CiftleriGetir(null);

            btnCiftler = new Button { Text = "2) Seçili Turnuvanın Çiftlerini Getir", Location = new Point(20, 246), Width = 320, Height = 30, Font = kalin };
            btnCiftler.Click += async (s, e) => await CiftleriGetir(null);

            txtLink = new TextBox { Location = new Point(350, 249), Width = 250 };
            btnLink = new Button { Text = "Linkten Aç", Location = new Point(606, 247), Width = 74, Height = 28 };
            btnLink.Click += async (s, e) => await LinktenAc();
            var ipucu = new Label { Text = "(isteğe bağlı: turnuva sonuç bağlantısını yapıştırın)", Location = new Point(350, 280), AutoSize = true, ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8f) };

            lblTurnuvaBilgi = new Label { Text = "", Location = new Point(20, 300), AutoSize = true, ForeColor = Color.DarkBlue, Font = kalin };
            lstCift = new ListBox { Location = new Point(20, 326), Width = 660, Height = 190 };

            chkSaha = new CheckBox { Text = "Sahanın diğer sonuçlarını da açıklama satırı olarak ekle", Location = new Point(20, 524), AutoSize = true, Checked = true };

            btnKaydet = new Button
            {
                Text = "3) Seçili Çiftin Bordlarını .PBN Olarak Kaydet...",
                Location = new Point(20, 552), Width = 660, Height = 40, Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnKaydet.Click += (s, e) => Kaydet();

            lblDurum = new Label { Text = "Hazır. Ay ve yılı seçip 'Takvimi Getir'e basın.", Location = new Point(20, 614), AutoSize = true, ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8.5f) };

            Controls.AddRange(new Control[] { lblKulup, cmbKulup, btnKulup, lblTakvim, cmbAy, nudYil, btnTakvim, lstTurnuva, btnCiftler, txtLink, btnLink, ipucu,
                                              lblTurnuvaBilgi, lstCift, chkSaha, btnKaydet, lblDurum });
        }

        private void Mesgul(bool mesgul)
        {
            btnTakvim.Enabled = btnCiftler.Enabled = btnLink.Enabled = btnKulup.Enabled = cmbKulup.Enabled = !mesgul;
            btnKaydet.Enabled = !mesgul && lstCift.Items.Count > 0;
            Cursor = mesgul ? Cursors.WaitCursor : Cursors.Default;
        }

        private KulupOge SeciliKulup { get { return cmbKulup.SelectedItem as KulupOge; } }

        private string Taban
        {
            get
            {
                var k = SeciliKulup;
                return SiteKoku + "/" + (k != null ? k.Kod : VarsayilanKulup);
            }
        }

        // ───────────────────────── 0) Kulüp seçimi ─────────────────────────

        private async Task KuluplariGetir()
        {
            Mesgul(true);
            try
            {
                lblDurum.Text = "Kayıtlı kulüpler okunuyor (clubs.vugraph.com)...";
                string html = await Indir(KulupListesiUrl);
                var liste = Vugraph.KulupListesiAyristir(html);
                if (liste.Count == 0) throw new Exception("Kulüp listesi sayfada bulunamadı.");

                string istenen = SeciliKulup != null ? SeciliKulup.Kod : SonKulupKodu();
                cmbKulup.Items.Clear();
                foreach (var k in liste) cmbKulup.Items.Add(k);
                var sec = liste.Find(k => string.Equals(k.Kod, istenen, StringComparison.OrdinalIgnoreCase))
                          ?? liste.Find(k => string.Equals(k.Kod, VarsayilanKulup, StringComparison.OrdinalIgnoreCase))
                          ?? liste[0];
                cmbKulup.SelectedItem = sec;
                KulupDegisti();
                lblDurum.Text = liste.Count + " kulüp listelendi. Kulübü seçip 'Takvimi Getir'e basın.";
            }
            catch (Exception ex)
            {
                // Liste alınamazsa varsayılan kulüple devam edilebilsin
                if (cmbKulup.Items.Count == 0)
                {
                    var k = new KulupOge { Kod = VarsayilanKulup, Ad = "MARMARIS BRIÇ KULÜBÜ", Yer = "MARMARIS/MUGLA" };
                    cmbKulup.Items.Add(k); cmbKulup.SelectedItem = k;
                }
                Hata("Kulüp listesi okunamadı (varsayılan kulüp kullanılıyor)", ex);
            }
            finally { Mesgul(false); }
        }

        private string SonKulupKodu()
        {
            try { if (File.Exists(KulupDosyasi)) return File.ReadAllText(KulupDosyasi).Trim(); } catch { }
            return VarsayilanKulup;
        }

        private string oncekiKulup = "";

        /// <summary>Kulüp değişince eski kulübün turnuva ve çift listelerini temizler; seçimi hatırlar.</summary>
        private void KulupDegisti()
        {
            var k = SeciliKulup;
            if (k == null)
            {
                // Metin yazılarak seçilmişse listede eşleşen kulübü bul
                string yazi = cmbKulup.Text.Trim();
                foreach (var o in cmbKulup.Items) if (o is KulupOge && string.Equals(o.ToString(), yazi, StringComparison.CurrentCultureIgnoreCase)) { cmbKulup.SelectedItem = o; k = (KulupOge)o; break; }
                if (k == null) return;
            }
            if (string.Equals(oncekiKulup, k.Kod, StringComparison.OrdinalIgnoreCase)) return;
            oncekiKulup = k.Kod;
            lstTurnuva.Items.Clear();
            lstCift.Items.Clear();
            lblTurnuvaBilgi.Text = "";
            turnuva = null;
            btnKaydet.Enabled = false;
            lblDurum.Text = k.Ad + " seçildi. 'Takvimi Getir'e basın.";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(KulupDosyasi));
                File.WriteAllText(KulupDosyasi, k.Kod);
            }
            catch { }
        }

        private async Task<string> Indir(string url)
        {
            byte[] bytes = await client.GetByteArrayAsync(url);
            return Encoding.UTF8.GetString(bytes);
        }

        // ───────────────────────── 1) Takvim ─────────────────────────

        private async Task TakvimiGetir()
        {
            Mesgul(true);
            try
            {
                int ay = cmbAy.SelectedIndex + 1, yil = (int)nudYil.Value;
                lblDurum.Text = AyAdlari[ay - 1] + " " + yil + " takvimi okunuyor...";
                string html = await Indir(Taban + "/calendar/" + yil + "-" + ay.ToString("00"));
                var liste = Vugraph.TakvimAyristir(html, ay, yil);
                lstTurnuva.Items.Clear();
                foreach (var t in liste) lstTurnuva.Items.Add(t);
                if (liste.Count > 0)
                {
                    lstTurnuva.SelectedIndex = lstTurnuva.Items.Count - 1;
                    lblDurum.Text = liste.Count + " turnuva listelendi. Birini seçip çift tıklayın ya da 'Çiftlerini Getir'e basın.";
                }
                else lblDurum.Text = "Bu ayda turnuva bulunamadı.";
            }
            catch (Exception ex) { Hata("Takvim okunamadı", ex); }
            finally { Mesgul(false); }
        }

        // ───────────────────────── 2) Turnuva → çiftler ─────────────────────────

        private async Task LinktenAc()
        {
            var m = Regex.Match(txtLink.Text, @"clubs\.vugraph\.com/([^/?#]+)/results/(\d{4}-\d{2}-\d{2}(?:-\d+)?)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                MessageBox.Show(this, "Geçerli bir turnuva sonuç linki girin\n(https://clubs.vugraph.com/kulüp/results/tarih).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Linkteki kulüp listede varsa seç, yoksa ekle
            string kod = m.Groups[1].Value;
            KulupOge bulunan = null;
            foreach (var o in cmbKulup.Items) { var k = o as KulupOge; if (k != null && string.Equals(k.Kod, kod, StringComparison.OrdinalIgnoreCase)) { bulunan = k; break; } }
            if (bulunan == null) { bulunan = new KulupOge { Kod = kod, Ad = kod }; cmbKulup.Items.Add(bulunan); }
            cmbKulup.SelectedItem = bulunan;
            KulupDegisti();
            await CiftleriGetir(m.Groups[2].Value);
        }

        private async Task CiftleriGetir(string eventId)
        {
            if (eventId == null)
            {
                var secili = lstTurnuva.SelectedItem as TurnuvaOge;
                if (secili == null)
                {
                    MessageBox.Show(this, "Önce takvimden bir turnuva seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                eventId = secili.EventId;
            }

            Mesgul(true);
            try
            {
                lblDurum.Text = "Turnuva sonuçları okunuyor...";
                string html = await Indir(Taban + "/results/" + eventId);
                turnuva = Vugraph.TurnuvaAyristir(html, eventId);
                turnuva.Kulup = SeciliKulup != null ? SeciliKulup.Ad : "";
                lstCift.Items.Clear();
                foreach (var c in turnuva.Ciftler) lstCift.Items.Add(c);
                if (turnuva.Ciftler.Count == 0)
                {
                    lblTurnuvaBilgi.Text = "";
                    lblDurum.Text = "Bu turnuvada çift listesi bulunamadı (takım turnuvası ya da henüz yayınlanmamış olabilir).";
                    return;
                }
                lstCift.SelectedIndex = 0;
                lblTurnuvaBilgi.Text = turnuva.Ad + (turnuva.Tarih.HasValue ? "  (" + turnuva.Tarih.Value.ToString("dd.MM.yyyy") + " " + turnuva.Saat + ")" : "")
                    + "  -  " + turnuva.Ciftler.Count + " çift";
                lblDurum.Text = "Bir çift seçip kaydet düğmesine basın.";
            }
            catch (Exception ex) { Hata("Turnuva okunamadı", ex); }
            finally { Mesgul(false); }
        }

        // ───────────────────────── 3) Seçili çiftin bordlarını PBN'e kaydet ─────────────────────────

        private void Kaydet()
        {
            var cift = lstCift.SelectedItem as CiftOge;
            if (cift == null || turnuva == null) return;

            // Turnuvanın tüm verisi çiftlerle birlikte indi; burada yeniden indirme yok
            var atlananlar = new List<string>();
            var bordlar = Vugraph.CiftinBordlari(turnuva, cift, atlananlar);
            if (bordlar.Count == 0)
            {
                string neden = atlananlar.Count > 0 ? "\n\n" + string.Join("\n", atlananlar) : "";
                MessageBox.Show(this, "Bu çift için kaydedilebilecek bord bulunamadı." + neden, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tarihAd = turnuva.Tarih.HasValue ? turnuva.Tarih.Value.ToString("yyyy-MM-dd") + " " : "";
            string onerilen = Vugraph.DosyaAdi(tarihAd + Vugraph.Ascii(turnuva.Ad) + " - " + Vugraph.Ascii(cift.Ad)) + ".pbn";
            string yol;
            using (var sfd = new SaveFileDialog
            {
                Filter = "PBN Dosyası (*.pbn)|*.pbn",
                Title = "PBN Dosyasını Kaydet",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = onerilen,
                OverwritePrompt = true
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                yol = sfd.FileName;
            }

            try
            {
                var pbn = new StringBuilder();
                pbn.Append(Vugraph.PbnBaslik(turnuva, cift, bordlar.Count));
                foreach (var bord in bordlar) pbn.Append(Vugraph.PbnBord(turnuva, cift, bord, chkSaha.Checked));
                File.WriteAllText(yol, pbn.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n"), new UTF8Encoding(false));
                lblDurum.Text = "Tamamlandı: " + bordlar.Count + " bord kaydedildi.";

                string mesaj = bordlar.Count + " bord kaydedildi:\n" + yol;
                if (atlananlar.Count > 0) mesaj += "\n\nKaydedilemeyen bordlar:\n" + string.Join("\n", atlananlar);
                MessageBox.Show(this, mesaj, "İşlem Tamam", MessageBoxButtons.OK, atlananlar.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex) { Hata("PBN dosyası oluşturulamadı", ex); }
        }

        private void Hata(string baslik, Exception ex)
        {
            lblDurum.Text = baslik + ".";
            MessageBox.Show(this, baslik + ":\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
