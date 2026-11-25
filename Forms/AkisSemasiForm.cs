using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Akış Şeması: Bridge Baron .fv şemalarını gösterir. Soldan şema seçilir; sağda
    /// "Adım adım" (soruları Evet/Hayır ile yürüt), "Tüm şema" (açılır ağaç) ve "Açıklama" sekmeleri.
    /// Dosyalar projeye kopyalanmaz; klasör ilk açılışta bulunur (ya da sorulur) ve hatırlanır.
    /// </summary>
    public class AkisSemasiForm : Form
    {
        private const string VarsayilanKlasor = @"D:\Games\Flowcharts";

        private sealed class Oge
        {
            public AkisSemasi Sema;
            public override string ToString() { return Sema.Baslik; }
        }

        private sealed class Adim
        {
            public AkisSemasi.Dugum Dugum;
            public string Cevap;          // bu düğümde verilen cevap ("Evet"/"Hayır"); sonuncuda null
            public string Neden;          // otomatik cevaplandıysa gerekçesi; elle cevaplandıysa null
            public string Sema;           // bu düğümün şeması (başlık); şema değişimini yolda göstermek için
        }

        private readonly TextBox _txtAra = new TextBox();
        private readonly ListBox _lst = new ListBox();
        private readonly Label _lblKlasor = new Label();
        private readonly Button _btnKlasor = new Button();
        private readonly CheckBox _chkTurkce = new CheckBox();
        private readonly TabControl _sekmeler = new TabControl();

        // Adım adım
        private readonly Label _lblSoru = new Label();
        private readonly RichTextBox _rtbAciklama = new RichTextBox();
        private readonly Button _btnEvet = new Button();
        private readonly Button _btnHayir = new Button();
        private readonly Button _btnGeri = new Button();
        private readonly Button _btnBasa = new Button();
        private readonly Button _btnGit = new Button();
        private readonly Button _btnOto = new Button();
        private readonly Label _lblDurum = new Label();

        /// <summary>Otomatik yol için güncel ihale durumunu (sıradaki oyuncunun eli ve ihale geçmişi) verir; atanmazsa düğme gizlenir.</summary>
        public Func<BricKartOyunu.Class.Bidding.IhaleDurumu> DurumKaynagi { get; set; }
        private bool _otomatik;          // elime göre yol modu: elle cevaptan sonra da otomatik ilerlenir
        private bool _yoluKoru;          // şema değişirken izlenen yol silinmesin (otomatik şema geçişi)
        private readonly ListBox _lstYol = new ListBox();

        // Tüm şema
        private readonly TreeView _agac = new TreeView();
        private readonly RichTextBox _rtbDugum = new RichTextBox();

        // Açıklama
        private readonly RichTextBox _rtbGiris = new RichTextBox();

        private string _klasor;
        private readonly List<Oge> _hepsi = new List<Oge>();
        private AkisSemasi _sema;
        private readonly List<Adim> _yol = new List<Adim>();
        private AkisSemasi.Dugum _simdiki;
        private bool _listeyiYukluyor;

        public AkisSemasiForm()
        {
            Text = "Akış Şeması";
            ShowInTaskbar = false;       // görev çubuğunda ayrı düğme olmasın (sahip form üzerinden açılır)
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9.5F);
            Size = new Size(1100, 720);
            MinimumSize = new Size(800, 520);
            KurArayuz();
        }

        // ───────────────────────── Klasör ve yükleme ─────────────────────────

        private static string AyarDosyasi()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BricKartOyunu", "akis_semasi_klasor.txt");
        }

        private static bool FvVar(string klasor)
        {
            try { return !string.IsNullOrEmpty(klasor) && Directory.Exists(klasor) && Directory.GetFiles(klasor, "*.fv").Length > 0; }
            catch { return false; }
        }

        private static string KlasorBul()
        {
            try
            {
                string kayit = File.Exists(AyarDosyasi()) ? File.ReadAllText(AyarDosyasi()).Trim() : null;
                if (FvVar(kayit)) return kayit;
            }
            catch { }
            if (FvVar(VarsayilanKlasor)) return VarsayilanKlasor;
            string yanda = Path.Combine(Application.StartupPath, "Flowcharts");
            if (FvVar(yanda)) return yanda;
            return null;
        }

        private static void KlasoruKaydet(string klasor)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(AyarDosyasi()));
                File.WriteAllText(AyarDosyasi(), klasor);
            }
            catch { }
        }

        /// <summary>Klasörü bulur ya da kullanıcıya sorar; yoksa false (form açılmaz).</summary>
        public bool Hazirla()
        {
            _klasor = KlasorBul();
            if (_klasor == null && !KlasorSor()) return false;
            return SemalariYukle();
        }

        private bool KlasorSor()
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Akış şeması (.fv) dosyalarının bulunduğu klasörü seçin (ör. Bridge Baron\\Flowcharts)";
                if (!string.IsNullOrEmpty(_klasor) && Directory.Exists(_klasor)) dlg.SelectedPath = _klasor;
                if (dlg.ShowDialog(this) != DialogResult.OK) return false;
                if (!FvVar(dlg.SelectedPath))
                {
                    MessageBox.Show(this, "Seçilen klasörde .fv dosyası bulunamadı.", "Akış Şeması", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                _klasor = dlg.SelectedPath;
                KlasoruKaydet(_klasor);
                return true;
            }
        }

        private bool SemalariYukle()
        {
            _hepsi.Clear();
            var hatalar = new List<string>();
            foreach (string f in Directory.GetFiles(_klasor, "*.fv").OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                try { _hepsi.Add(new Oge { Sema = AkisSemasi.Yukle(f) }); }
                catch (Exception ex) { hatalar.Add(Path.GetFileName(f) + ": " + ex.Message); }
            }
            _hepsi.Sort((a, b) => string.Compare(a.Sema.Baslik, b.Sema.Baslik, StringComparison.OrdinalIgnoreCase));
            if (_hepsi.Count == 0)
            {
                MessageBox.Show(this, "Okunabilen akış şeması bulunamadı.\n" + string.Join("\n", hatalar.Take(5)), "Akış Şeması", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            _lblKlasor.Text = _klasor;
            ListeyiDoldur();
            return true;
        }

        private void ListeyiDoldur()
        {
            string ara = (_txtAra.Text ?? "").Trim();
            _listeyiYukluyor = true;
            try
            {
                var secili = _lst.SelectedItem as Oge;
                _lst.BeginUpdate();
                _lst.Items.Clear();
                foreach (var o in _hepsi)
                    if (ara.Length == 0 || o.Sema.Baslik.IndexOf(ara, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        o.Sema.KisaAd.IndexOf(ara, StringComparison.OrdinalIgnoreCase) >= 0)
                        _lst.Items.Add(o);
                _lst.EndUpdate();
                if (secili != null && _lst.Items.Contains(secili)) _lst.SelectedItem = secili;
            }
            finally { _listeyiYukluyor = false; }
            if (_lst.SelectedItem == null && _lst.Items.Count > 0) _lst.SelectedIndex = 0;
        }

        // ───────────────────────── Arayüz ─────────────────────────

        private void KurArayuz()
        {
            var ana = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };
            Controls.Add(ana);
            // Ayırıcı konumu form boyutlandıktan sonra verilmeli (yoksa varsayılan genişliğe sıkışır)
            Load += delegate { try { ana.SplitterDistance = 360; } catch { } };

            // Sol: arama + liste + klasör
            _txtAra.Dock = DockStyle.Top;
            _txtAra.TextChanged += delegate { ListeyiDoldur(); };
            var ipucu = new Label { Text = "Ara (başlık):", Dock = DockStyle.Top, Height = 22, Padding = new Padding(2, 4, 0, 0) };
            _lst.Dock = DockStyle.Fill;
            _lst.IntegralHeight = false;
            _lst.SelectedIndexChanged += delegate
            {
                if (_listeyiYukluyor) return;
                var o = _lst.SelectedItem as Oge;
                if (o != null) SemaSec(o.Sema);
            };
            var alt = new Panel { Dock = DockStyle.Bottom, Height = 80 };
            _chkTurkce.Text = "Türkçe metin (kapalıyken orijinal İngilizce)";
            _chkTurkce.Dock = DockStyle.Top;
            _chkTurkce.Height = 26;
            _chkTurkce.Checked = AkisSemasi.TurkceKullan;
            _chkTurkce.CheckedChanged += delegate
            {
                AkisSemasi.TurkceKullan = _chkTurkce.Checked;
                string secili = _sema == null ? null : _sema.DosyaAdi;
                if (!SemalariYukle()) return;
                var yeni = _hepsi.FirstOrDefault(x => x.Sema.DosyaAdi == secili);
                if (yeni != null && _lst.Items.Contains(yeni)) _lst.SelectedItem = yeni;
            };
            _btnKlasor.Text = "Klasör seç…";
            _btnKlasor.Dock = DockStyle.Right;
            _btnKlasor.Width = 96;
            _btnKlasor.Click += delegate
            {
                if (KlasorSor()) SemalariYukle();
            };
            _lblKlasor.Dock = DockStyle.Fill;
            _lblKlasor.AutoEllipsis = true;
            _lblKlasor.ForeColor = SystemColors.GrayText;
            _lblKlasor.TextAlign = ContentAlignment.MiddleLeft;
            alt.Controls.Add(_lblKlasor);
            alt.Controls.Add(_btnKlasor);
            alt.Controls.Add(_chkTurkce);
            ana.Panel1.Controls.Add(_lst);
            ana.Panel1.Controls.Add(alt);
            ana.Panel1.Controls.Add(_txtAra);
            ana.Panel1.Controls.Add(ipucu);

            // Sağ: sekmeler
            _sekmeler.Dock = DockStyle.Fill;
            var tpAdim = new TabPage("Adım adım");
            var tpAgac = new TabPage("Tüm şema");
            var tpGiris = new TabPage("Açıklama");
            _sekmeler.TabPages.Add(tpAdim);
            _sekmeler.TabPages.Add(tpAgac);
            _sekmeler.TabPages.Add(tpGiris);
            ana.Panel2.Controls.Add(_sekmeler);

            KurAdimSekmesi(tpAdim);
            KurAgacSekmesi(tpAgac);
            _rtbGiris.Dock = DockStyle.Fill;
            _rtbGiris.ReadOnly = true;
            _rtbGiris.BorderStyle = BorderStyle.None;
            _rtbGiris.BackColor = SystemColors.Window;
            tpGiris.Controls.Add(_rtbGiris);
        }

        private void KurAdimSekmesi(TabPage tp)
        {
            _lblSoru.Dock = DockStyle.Top;
            _lblSoru.Font = new Font("Segoe UI Semibold", 14F);
            _lblSoru.Height = 64;
            _lblSoru.Padding = new Padding(4, 8, 4, 4);

            _rtbAciklama.Dock = DockStyle.Fill;
            _rtbAciklama.ReadOnly = true;
            _rtbAciklama.BorderStyle = BorderStyle.None;
            _rtbAciklama.BackColor = SystemColors.Window;

            var dugmeler = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(2, 8, 2, 2) };
            Action<Button, string, int> Ayarla = delegate(Button b, string t, int w) { b.Text = t; b.Width = w; b.Height = 34; dugmeler.Controls.Add(b); };
            Ayarla(_btnEvet, "Evet", 110);
            Ayarla(_btnHayir, "Hayır", 110);
            Ayarla(_btnGeri, "◄ Geri", 90);
            Ayarla(_btnBasa, "Başa dön", 100);
            Ayarla(_btnGit, "Bu şemaya git ►", 160);
            Ayarla(_btnOto, "Elime göre yol", 140);
            _btnOto.Click += delegate { ElimeGoreYol(); };
            _lblDurum.Dock = DockStyle.Top;
            _lblDurum.Height = 40;
            _lblDurum.Padding = new Padding(4, 2, 4, 2);
            _lblDurum.ForeColor = Color.FromArgb(0, 90, 160);
            _btnEvet.Click += delegate { Ilerle(true); };
            _btnHayir.Click += delegate { Ilerle(false); };
            _btnGeri.Click += delegate { Geri(); };
            _btnBasa.Click += delegate { Basa(); };
            _btnGit.Click += delegate { HedefeGit(); };

            var yolBaslik = new Label { Text = "İzlenen yol:", Dock = DockStyle.Top, Height = 22, Padding = new Padding(2, 4, 0, 0) };
            _lstYol.Dock = DockStyle.Fill;
            _lstYol.IntegralHeight = false;
            var yolPanel = new Panel { Dock = DockStyle.Bottom, Height = 150 };
            yolPanel.Controls.Add(_lstYol);
            yolPanel.Controls.Add(yolBaslik);

            tp.Controls.Add(_rtbAciklama);
            tp.Controls.Add(_lblDurum);
            tp.Controls.Add(_lblSoru);
            tp.Controls.Add(yolPanel);
            tp.Controls.Add(dugmeler);
        }

        private void KurAgacSekmesi(TabPage tp)
        {
            var bol = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            bol.SplitterDistance = 380;
            _agac.Dock = DockStyle.Fill;
            _agac.HideSelection = false;
            _agac.BeforeExpand += AgacAcilirken;
            _agac.AfterSelect += delegate
            {
                var d = _agac.SelectedNode == null ? null : _agac.SelectedNode.Tag as AkisSemasi.Dugum;
                DugumYaz(_rtbDugum, d);
            };
            _rtbDugum.Dock = DockStyle.Fill;
            _rtbDugum.ReadOnly = true;
            _rtbDugum.BorderStyle = BorderStyle.None;
            _rtbDugum.BackColor = SystemColors.Window;
            bol.Panel1.Controls.Add(_agac);
            bol.Panel2.Controls.Add(_rtbDugum);
            tp.Controls.Add(bol);
        }

        // ───────────────────────── Şema seçimi ve yürütme ─────────────────────────

        private void SemaSec(AkisSemasi sema)
        {
            _sema = sema;
            // Açıklama sekmesi
            _rtbGiris.Clear();
            Yaz(_rtbGiris, sema.Baslik + "\n\n", new Font(Font.FontFamily, 13F, FontStyle.Bold), _rtbGiris.ForeColor);
            Yaz(_rtbGiris, string.IsNullOrEmpty(sema.Giris) ? "(Bu şema için giriş açıklaması yok.)" : sema.Giris.Replace("\n", "\n\n"), Font, _rtbGiris.ForeColor);
            // Ağaç
            _agac.BeginUpdate();
            _agac.Nodes.Clear();
            var kok = AgacDugumu(sema.Kok, null);
            _agac.Nodes.Add(kok);
            kok.Expand();
            _agac.EndUpdate();
            // Adım adım (otomatik şema geçişinde izlenen yol korunur)
            if (_yoluKoru) { _simdiki = sema.Kok; Goster(); }
            else Basa();
            _sekmeler.SelectedIndex = 0;
        }

        private void Basa()
        {
            _yol.Clear();
            _otomatik = false;
            _simdiki = _sema == null ? null : _sema.Kok;
            _lblDurum.Text = "";
            Goster();
        }

        private void Ilerle(bool evet)
        {
            if (_simdiki == null) return;
            var sonraki = evet ? _simdiki.Evet : _simdiki.Hayir;
            if (sonraki == null) return;
            _yol.Add(new Adim { Dugum = _simdiki, Cevap = evet ? "Evet" : "Hayır", Sema = _sema.Baslik });
            _simdiki = sonraki;
            Goster();
            if (_otomatik) OtomatikIlerle();
        }

        /// <summary>Şemanın başından elime göre yürür: cevabı kesin olan soruları kendisi geçer, emin olamadığında durup cevabı sizden ister.</summary>
        private void ElimeGoreYol()
        {
            if (_sema == null) return;
            if (DurumKaynagi == null || DurumKaynagi() == null)
            {
                MessageBox.Show(this, "Elime göre yol için bir ihale sürüyor olmalı (el ve ihale geçmişi gerekir).", "Akış Şeması", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Basa();
            _otomatik = true;
            OtomatikIlerle();
        }

        private void OtomatikIlerle()
        {
            var durum = DurumKaynagi == null ? null : DurumKaynagi();
            if (durum == null) { _otomatik = false; return; }
            for (int sayac = 0; sayac < 300 && _simdiki != null; sayac++)
            {
                var d = _simdiki;
                if (d.Yaprak)
                {
                    _lblDurum.Text = "Yol tamamlandı: sonuç bulundu.";
                    return;
                }
                if (d.Kutu.Hedef != null)
                {
                    var hedef = _hepsi.FirstOrDefault(x => string.Equals(x.Sema.KisaAd, d.Kutu.Hedef, StringComparison.OrdinalIgnoreCase) ||
                                                           string.Equals(x.Sema.DosyaAdi, d.Kutu.Hedef, StringComparison.OrdinalIgnoreCase));
                    if (hedef == null) { _lblDurum.Text = "Hedef şema bulunamadı: " + d.Kutu.Hedef; return; }
                    _yol.Add(new Adim { Dugum = d, Cevap = "şemaya geçildi: " + hedef.Sema.Baslik, Neden = "otomatik", Sema = _sema.Baslik });
                    _yoluKoru = true;
                    try { _txtAra.Text = ""; ListeyiDoldur(); _lst.SelectedItem = hedef; }
                    finally { _yoluKoru = false; }
                    _otomatik = true;
                    continue;
                }
                var c = AkisYolBulucu.Cevapla(_sema.DosyaAdi, d, durum);
                if (c == null)
                {
                    _lblDurum.Text = "Bu soruyu eldeki bilgiyle otomatik cevaplayamadım. Lütfen Evet ya da Hayır'ı siz seçin; sonrasında otomatik devam edilir.";
                    Goster();
                    return;
                }
                _yol.Add(new Adim { Dugum = d, Cevap = c.Evet ? "Evet" : "Hayır", Neden = c.Neden, Sema = _sema.Baslik });
                _simdiki = c.Evet ? d.Evet : d.Hayir;
                if (_simdiki == null) { _lblDurum.Text = "Şemada bu cevabın devamı yok."; Goster(); return; }
            }
            _lblDurum.Text = "Yol çok uzadı, otomatik ilerleme durduruldu.";
            Goster();
        }

        private void Geri()
        {
            if (_yol.Count == 0) return;
            _simdiki = _yol[_yol.Count - 1].Dugum;
            _yol.RemoveAt(_yol.Count - 1);
            Goster();
        }

        private void HedefeGit()
        {
            if (_simdiki == null || _simdiki.Kutu.Hedef == null) return;
            string h = _simdiki.Kutu.Hedef;
            var o = _hepsi.FirstOrDefault(x => string.Equals(x.Sema.KisaAd, h, StringComparison.OrdinalIgnoreCase) ||
                                               string.Equals(x.Sema.DosyaAdi, h, StringComparison.OrdinalIgnoreCase));
            if (o == null)
            {
                MessageBox.Show(this, "Hedef şema bulunamadı: " + h, "Akış Şeması", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _txtAra.Text = "";
            ListeyiDoldur();
            _lst.SelectedItem = o;       // SemaSec tetiklenir
        }

        private void Goster()
        {
            var d = _simdiki;
            _btnGeri.Enabled = _yol.Count > 0;
            _btnBasa.Enabled = _yol.Count > 0;
            _lstYol.BeginUpdate();
            _lstYol.Items.Clear();
            _btnOto.Visible = DurumKaynagi != null;
            foreach (var a in _yol)
                _lstYol.Items.Add(AkisSemasi.Simgeler(a.Dugum.Kutu.Etiket) + "  →  " + a.Cevap + (string.IsNullOrEmpty(a.Neden) || a.Neden == "otomatik" ? "" : "   (otomatik: " + a.Neden + ")"));
            _lstYol.EndUpdate();
            if (_lstYol.Items.Count > 0) _lstYol.TopIndex = _lstYol.Items.Count - 1;

            if (d == null)
            {
                _lblSoru.Text = "";
                _rtbAciklama.Clear();
                _btnEvet.Enabled = _btnHayir.Enabled = false;
                _btnGit.Visible = false;
                return;
            }
            bool yaprak = d.Yaprak;
            _lblSoru.Text = (yaprak ? "Sonuç: " : "") + AkisSemasi.Simgeler(d.Kutu.Etiket);
            _btnEvet.Enabled = d.Evet != null;
            _btnHayir.Enabled = d.Hayir != null;
            _btnEvet.Visible = _btnHayir.Visible = !yaprak;
            _btnGit.Visible = d.Kutu.Hedef != null;
            DugumYaz(_rtbAciklama, d);
        }

        // ───────────────────────── Ağaç ─────────────────────────

        private static TreeNode AgacDugumu(AkisSemasi.Dugum d, string onek)
        {
            var t = new TreeNode((onek == null ? "" : onek + " ") + AkisSemasi.Simgeler(d.Kutu.Etiket)) { Tag = d };
            if (!d.Yaprak) t.Nodes.Add(new TreeNode("…") { Name = "bos" });     // tembel açılım (graf, ağaç olarak açılırsa çok büyür)
            return t;
        }

        private void AgacAcilirken(object sender, TreeViewCancelEventArgs e)
        {
            var t = e.Node;
            if (t.Nodes.Count == 1 && t.Nodes[0].Name == "bos")
            {
                t.Nodes.Clear();
                var d = t.Tag as AkisSemasi.Dugum;
                if (d == null) return;
                if (d.Evet != null) t.Nodes.Add(AgacDugumu(d.Evet, "Evet:"));
                if (d.Hayir != null) t.Nodes.Add(AgacDugumu(d.Hayir, "Hayır:"));
            }
        }

        // ───────────────────────── Metin yazımı ─────────────────────────

        private void DugumYaz(RichTextBox rtb, AkisSemasi.Dugum d)
        {
            rtb.Clear();
            if (d == null) return;
            string metin = d.Kutu.Aciklama;
            if (string.IsNullOrEmpty(metin)) metin = d.Yaprak ? "(Açıklama yok.)" : "";
            Yaz(rtb, metin.Replace("\n", "\n\n"), new Font(Font.FontFamily, 11F), rtb.ForeColor);
            if (d.Kutu.Hedef != null)
                Yaz(rtb, "\n\n➜ Bu kutu başka bir şemaya yönlendirir: " + d.Kutu.Hedef, new Font(Font.FontFamily, 10F, FontStyle.Italic), SystemColors.GrayText);
        }

        /// <summary>Metni yazar; ♣ yeşil, ♦ turuncu, ♥ kırmızı olur ($C $D $H $S önce simgeye çevrilir).</summary>
        private static void Yaz(RichTextBox rtb, string metin, Font font, Color renk)
        {
            metin = AkisSemasi.Simgeler(metin);
            var sb = new StringBuilder();
            Action<Color> Bosalt = delegate(Color c)
            {
                if (sb.Length == 0) return;
                rtb.SelectionStart = rtb.TextLength;
                rtb.SelectionLength = 0;
                rtb.SelectionFont = font;
                rtb.SelectionColor = c;
                rtb.AppendText(sb.ToString());
                sb.Length = 0;
            };
            foreach (char ch in metin)
            {
                Color? ozel = ch == '\u2663' ? Color.FromArgb(0, 140, 0)
                            : ch == '\u2666' ? Color.FromArgb(230, 120, 0)
                            : ch == '\u2665' ? Color.FromArgb(210, 0, 0)
                            : (Color?)null;
                if (ozel.HasValue)
                {
                    Bosalt(renk);
                    sb.Append(ch);
                    Bosalt(ozel.Value);
                }
                else sb.Append(ch);
            }
            Bosalt(renk);
        }
    }
}
