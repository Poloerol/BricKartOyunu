using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Kart Dağıtıcı (Bridge Baron "Deal Generator" düzeninde): her el için puan ve renk uzunluğu
    /// aralıkları ile dağıtıcı seçilir; koşulu sağlayan dağıtımlar seed taranarak bulunur.
    /// Bulunan dağıtımlar listelenir, PBN olarak kaydedilir veya oynanır.
    /// </summary>
    public partial class KartDagitici : Form
    {
        private const int AzamiDeneme = 3000000;
        private static readonly string[] Koltuklar = { "KUZEY", "DOĞU", "GÜNEY", "BATI" };   // DagitimUretici sırası
        private static readonly string[] KoltukAd = { "Kuzey", "Doğu", "Güney", "Batı" };
        private static readonly string[] Satirlar = { "Toplam puan", "Yüksek kart", "♠ uzunluk", "♥ uzunluk", "♦ uzunluk", "♣ uzunluk" };

        // [koltuk, satır, 0=min/1=max]
        private readonly TextBox[,,] _kutu = new TextBox[4, 6, 2];
        private ComboBox _cmbDagitici;
        private Button _btnIhale;
        private ListView _lvIhale;
        private List<string> _ihale = new List<string>();   // İhale Belirle: dağıtıcıdan başlayan teklifler
        private int _ihaleDagitici;
        private TextBox _txtSeed, _txtAdet;
        private Label _lblZaman, _lblTest, _lblGecerli;
        private ListBox _lstDagitimlar;
        private TextBox _txtEller;
        private Button _btnCalistir, _btnKaydet, _btnOyna, _btnTemizle;
        private System.Windows.Forms.Timer _zamanlayici;

        private CancellationTokenSource _iptal;
        private readonly List<int> _bulunan = new List<int>();
        private readonly object _kilit = new object();
        private int _test;
        private readonly Stopwatch _saat = new Stopwatch();
        private bool _calisiyor;

        public KartDagitici()
        {
            InitializeComponent();
            Kur();
            VarsayilanlariYukle();
        }

        // ───────────────────────── Arayüz ─────────────────────────

        // Maça siyah, Kupa kırmızı, Karo turuncu, Sinek yeşil
        private static Color SuitRengi(char c)
        {
            switch (c)
            {
                case '\u2665': return Color.Red;
                case '\u2666': return Color.DarkOrange;
                case '\u2663': return Color.Green;
                default: return SystemColors.WindowText;
            }
        }

        private void Kur()
        {
            // Yerleşim "Deal Generator" penceresinin birebir düzenidir: görünen pencere 465x440
            // (başlık çubuğu dahil), istemci alanı 463x409. Koordinatlar referans görüntüden alınmıştır.
            Text = "Kart Dağıtıcı";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Microsoft Sans Serif", 8.25F);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.None;   // boyut sabit
            ClientSize = new Size(463, 409);

            // Etiket yardımcısı (konum, ebeveyne göre)
            Func<string, int, int, int, int, ContentAlignment, Label> L = (metin, x, y, w, h, hiza) =>
                new Label { Text = metin, AutoSize = false, Location = new Point(x, y), Size = new Size(w, h), TextAlign = hiza };

            // ───── Eller (sol üst): referans görüntüde (10,41) – (325,231) ─────
            var gEller = new GroupBox { Text = "Eller", Location = new Point(9, 11), Size = new Size(323, 190) };
            // Grup içi koordinat = görüntü koordinatı - (10, 41)
            string[] satirAd = { "Toplam:", "Yüksek kart:", "Maça uzunluk:", "Kupa uzunluk:", "Karo uzunluk:", "Sinek uzunluk:" };
            gEller.Controls.Add(L("PUAN", 4, 20, 70, 13, ContentAlignment.MiddleLeft));
            for (int k = 0; k < 4; k++)
            {
                int px = 75 + k * 62;                                           // çift (min,max) başlangıcı
                gEller.Controls.Add(L(Koltuklar[k], px - 4, 4, 55, 13, ContentAlignment.MiddleCenter));
                gEller.Controls.Add(L("min", px, 20, 22, 13, ContentAlignment.MiddleCenter));
                gEller.Controls.Add(L("max", px + 25, 20, 22, 13, ContentAlignment.MiddleCenter));
            }
            for (int r = 0; r < 6; r++)
            {
                int y = 36 + r * 25;
                var lbl = L(satirAd[r], 0, y + 1, 74, 18, ContentAlignment.MiddleRight);
                if (r >= 2)
                {
                    // Yalnızca suit adı kendi renginde, "uzunluk:" normal renkte çizilir
                    int bosluk = satirAd[r].IndexOf(' ');
                    string simge = satirAd[r].Substring(0, bosluk), kalan = satirAd[r].Substring(bosluk);
                    Color sr = r == 3 ? Color.Red : r == 4 ? Color.DarkOrange : r == 5 ? Color.Green : SystemColors.WindowText;
                    lbl.Text = "";
                    lbl.Paint += (s, ev) =>
                    {
                        var f = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter;
                        var kw = TextRenderer.MeasureText(ev.Graphics, kalan, lbl.Font, new Size(int.MaxValue, int.MaxValue), f).Width;
                        var sw = TextRenderer.MeasureText(ev.Graphics, simge, lbl.Font, new Size(int.MaxValue, int.MaxValue), f).Width;
                        int x0 = lbl.Width - kw - sw;
                        TextRenderer.DrawText(ev.Graphics, simge, lbl.Font, new Rectangle(x0, 0, sw + 2, lbl.Height), sr, f);
                        TextRenderer.DrawText(ev.Graphics, kalan, lbl.Font, new Rectangle(x0 + sw, 0, kw + 2, lbl.Height), lbl.ForeColor, f);
                    };
                }
                gEller.Controls.Add(lbl);
                for (int k = 0; k < 4; k++)
                    for (int m = 0; m < 2; m++)
                    {
                        var t = new TextBox { Location = new Point(75 + k * 62 + m * 25, y), Size = new Size(22, 20), MaxLength = 2, TextAlign = HorizontalAlignment.Center };
                        t.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
                        _kutu[k, r, m] = t;
                        gEller.Controls.Add(t);
                    }
            }

            // ───── Dağıtıcı + İhale Belirle ─────
            var lblDagitici = L("Dağıtıcı", 9, 207, 46, 14, ContentAlignment.MiddleLeft);
            _cmbDagitici = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(56, 204), Size = new Size(84, 21) };
            _cmbDagitici.Items.AddRange(new object[] { "Herhangi", "Kuzey", "Doğu", "Güney", "Batı" });
            _btnIhale = new Button { Text = "İhale Belirle", Location = new Point(146, 203), Size = new Size(83, 24) };
            _btnIhale.Click += (s, e) => IhaleBelirle();

            // İhale listesi: sütunlar Batı Kuzey Doğu Güney
            var lblIhaleBas = L("Batı", 9, 236, 54, 13, ContentAlignment.MiddleLeft);
            var lblIhaleBas2 = L("Kuzey", 63, 236, 54, 13, ContentAlignment.MiddleLeft);
            var lblIhaleBas3 = L("Doğu", 117, 236, 54, 13, ContentAlignment.MiddleLeft);
            var lblIhaleBas4 = L("Güney", 171, 236, 54, 13, ContentAlignment.MiddleLeft);
            _lvIhale = new ListView
            {
                View = View.Details, HeaderStyle = ColumnHeaderStyle.None, Location = new Point(9, 249), Size = new Size(220, 118),
                BorderStyle = BorderStyle.Fixed3D, FullRowSelect = false, MultiSelect = false, Scrollable = true
            };
            for (int c = 0; c < 4; c++) _lvIhale.Columns.Add("", 54);
            // Teklifteki suit simgeleri kendi renginde çizilir
            _lvIhale.OwnerDraw = true;
            _lvIhale.DrawColumnHeader += (s, ev) => ev.DrawDefault = true;
            _lvIhale.DrawItem += (s, ev) => { };
            _lvIhale.DrawSubItem += (s, ev) =>
            {
                string t = ev.SubItem.Text ?? "";
                var bayrak = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
                int x = ev.Bounds.X + 2;
                foreach (char ch in t)
                {
                    string p = ch.ToString();
                    var boyut = TextRenderer.MeasureText(ev.Graphics, p, _lvIhale.Font, new Size(int.MaxValue, int.MaxValue), bayrak);
                    TextRenderer.DrawText(ev.Graphics, p, _lvIhale.Font, new Rectangle(x, ev.Bounds.Y, boyut.Width + 2, ev.Bounds.Height), SuitRengi(ch), bayrak);
                    x += boyut.Width;
                }
            };

            // ───── Üret (orta alt): (238,237) – (327,397) ─────
            var gUret = new GroupBox { Text = "Oluştur", Location = new Point(243, 207), Size = new Size(89, 160) };
            gUret.Controls.Add(L("Seed:", 4, 16, 80, 14, ContentAlignment.MiddleCenter));
            _txtSeed = new TextBox { Location = new Point(8, 31), Size = new Size(70, 20), MaxLength = 9, TextAlign = HorizontalAlignment.Center };
            _txtSeed.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            gUret.Controls.Add(_txtSeed);
            gUret.Controls.Add(L("Bulunacak", 4, 58, 80, 14, ContentAlignment.MiddleCenter));
            _txtAdet = new TextBox { Location = new Point(8, 73), Size = new Size(70, 20), MaxLength = 3, TextAlign = HorizontalAlignment.Center };
            _txtAdet.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            gUret.Controls.Add(_txtAdet);
            gUret.Controls.Add(L("Süre:", 4, 104, 80, 14, ContentAlignment.MiddleCenter));
            _lblZaman = L("00:00:00", 4, 118, 80, 14, ContentAlignment.MiddleCenter);
            gUret.Controls.Add(_lblZaman);
            _btnCalistir = new Button { Text = "Çalıştır", Location = new Point(7, 134), Size = new Size(75, 23) };
            _btnCalistir.Click += (s, e) => { if (_calisiyor) Durdur(); else Calistir(); };
            gUret.Controls.Add(_btnCalistir);

            // ───── Dağıtımlar (sağ): (330,41) – (453,390) ─────
            var gDag = new GroupBox { Text = "Dağıtımlar:", Location = new Point(345, 11), Size = new Size(108, 349) };
            gDag.Controls.Add(L("Denenen:", 8, 18, 47, 14, ContentAlignment.MiddleLeft));
            _lblTest = L("0", 52, 18, 52, 14, ContentAlignment.MiddleRight);
            gDag.Controls.Add(_lblTest);
            gDag.Controls.Add(L("Geçerli:", 8, 34, 47, 14, ContentAlignment.MiddleLeft));
            _lblGecerli = L("0", 52, 34, 52, 14, ContentAlignment.MiddleRight);
            gDag.Controls.Add(_lblGecerli);
            _lstDagitimlar = new ListBox { Location = new Point(8, 56), Size = new Size(92, 234), IntegralHeight = false, SelectionMode = SelectionMode.MultiExtended };
            _lstDagitimlar.SelectedIndexChanged += (s, e) => SeciliGoster();
            gDag.Controls.Add(_lstDagitimlar);
            _btnKaydet = new Button { Text = "Kaydet", Location = new Point(16, 296), Size = new Size(76, 22), Enabled = false };
            _btnOyna = new Button { Text = "Oyna", Location = new Point(16, 322), Size = new Size(76, 22), Enabled = false };
            _btnKaydet.Click += (s, e) => PbnKaydet();
            _btnOyna.Click += (s, e) => Oyna();
            gDag.Controls.Add(_btnKaydet);
            gDag.Controls.Add(_btnOyna);

            // ───── Alt sıra ─────
            // Alt sıra: sabit yerleşim (tüm butonlar 108x23, aralık 4 px; yazı 94 px civarı, sığar)
            var btnNS = new Button { Text = "N/S Ortaklık Profili", Location = new Point(9, 370), Size = new Size(108, 23), Padding = Padding.Empty };
            var btnEW = new Button { Text = "E/W Ortaklık Profili", Location = new Point(121, 370), Size = new Size(108, 23), Padding = Padding.Empty };
            btnNS.Click += (s, e) => AnlasmaForm.Goster(this, Taraf.NS);
            btnEW.Click += (s, e) => AnlasmaForm.Goster(this, Taraf.EW);
            _btnTemizle = new Button { Text = "Temizle", Location = new Point(250, 370), Size = new Size(75, 23) };
            _btnTemizle.Click += (s, e) => { if (!_calisiyor) VarsayilanlariYukle(); };
            var btnCikis = new Button { Text = "Çıkış", Location = new Point(361, 370), Size = new Size(75, 23) };
            btnCikis.Click += (s, e) => Cikis();

            // Seçili dağıtımın metni artık pencerede gösterilmez (liste + Oyna/Kaydet yeterli); alan yalnızca iç kullanım içindir.
            _txtEller = new TextBox { Multiline = true, ReadOnly = true };

            _zamanlayici = new System.Windows.Forms.Timer { Interval = 150 };
            _zamanlayici.Tick += (s, e) => Guncelle();

            Controls.AddRange(new Control[] { gEller, lblDagitici, _cmbDagitici, _btnIhale,
                lblIhaleBas, lblIhaleBas2, lblIhaleBas3, lblIhaleBas4, _lvIhale, gUret, gDag, btnNS, btnEW, _btnTemizle, btnCikis });
            FormClosing += (s, e) => { if (_iptal != null) _iptal.Cancel(); };
        }

        private void VarsayilanlariYukle()
        {
            for (int k = 0; k < 4; k++)
            {
                for (int r = 0; r < 2; r++) { _kutu[k, r, 0].Text = "0"; _kutu[k, r, 1].Text = "37"; }
                for (int r = 2; r < 6; r++) { _kutu[k, r, 0].Text = "0"; _kutu[k, r, 1].Text = "13"; }
            }
            _ihale = new List<string>();
            IhaleEtiketiGuncelle();
            _cmbDagitici.SelectedIndex = 0;
            _txtSeed.Text = new Random().Next(10000000, 99999999).ToString();
            _txtAdet.Text = "10";
            _lstDagitimlar.Items.Clear();
            lock (_kilit) { _bulunan.Clear(); }
            _test = 0;
            _lblTest.Text = "0"; _lblGecerli.Text = "0"; _lblZaman.Text = "00:00:00";
            _txtEller.Text = "";
            _btnKaydet.Enabled = false; _btnOyna.Enabled = false;
        }

        // ───────────────────────── İhale koşulu ─────────────────────────

        private void IhaleBelirle()
        {
            if (_calisiyor) return;
            if (_ihale.Count == 0 && _cmbDagitici.SelectedIndex <= 0)
            {
                MessageBox.Show(this, "İhale dizisi girmeden önce lütfen bir dağıtıcı seçin!", "Kart Dağıtıcı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int dag = _ihale.Count > 0 ? _ihaleDagitici : Math.Max(0, _cmbDagitici.SelectedIndex - 1);
            using (var f = new IhaleBelirleForm(dag, _ihale))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                _ihale = f.Teklifler;
                _ihaleDagitici = f.Dagitici;
                IhaleEtiketiGuncelle();
            }
        }

        private void IhaleEtiketiGuncelle()
        {
            _lvIhale.Items.Clear();
            if (_ihale.Count == 0)
            {
                _cmbDagitici.Enabled = true;
                return;
            }
            // Sütunlar: Batı, Kuzey, Doğu, Güney (saat yönü). Koltuk sırası (Kuzey=0, Doğu=1, Güney=2, Batı=3) → sütun
            int[] sutun = { 1, 2, 3, 0 };
            int baslangic = sutun[_ihaleDagitici];
            for (int i = 0; i < _ihale.Count; i++)
            {
                int idx = baslangic + i, satir = idx / 4, kol = idx % 4;
                while (_lvIhale.Items.Count <= satir) _lvIhale.Items.Add(new ListViewItem(new[] { "", "", "", "" }));
                _lvIhale.Items[satir].SubItems[kol].Text = _ihale[i];
            }
            _cmbDagitici.SelectedIndex = _ihaleDagitici + 1;   // dağıtıcı ihalenin ilk teklifini yapan oyuncudur
            _cmbDagitici.Enabled = false;
        }

        // ───────────────────────── Koşullar ─────────────────────────

        private sealed class Kosul
        {
            public int[,,] Aralik = new int[4, 6, 2];
            public List<string> Ihale = new List<string>();   // boş: ihale koşulu yok
            public int Dagitici = -1;   // -1: herhangi, 0..3: Kuzey, Doğu, Güney, Batı
        }

        private Kosul KosulOku(out string hata)
        {
            hata = null;
            var kosul = new Kosul();
            for (int k = 0; k < 4; k++)
                for (int r = 0; r < 6; r++)
                    for (int m = 0; m < 2; m++)
                    {
                        int v;
                        if (!int.TryParse(_kutu[k, r, m].Text, out v))
                        {
                            hata = Koltuklar[k] + " / " + Satirlar[r] + " alanı boş veya geçersiz.";
                            return null;
                        }
                        kosul.Aralik[k, r, m] = v;
                    }
            for (int k = 0; k < 4; k++)
                for (int r = 0; r < 6; r++)
                    if (kosul.Aralik[k, r, 0] > kosul.Aralik[k, r, 1])
                    {
                        hata = Koltuklar[k] + " / " + Satirlar[r] + ": en az değeri en çoktan büyük.";
                        return null;
                    }
            // Koltuk başına alt sınır toplamı 13 kartı aşamaz (renk uzunlukları)
            for (int k = 0; k < 4; k++)
            {
                int toplam = 0;
                for (int r = 2; r < 6; r++) toplam += kosul.Aralik[k, r, 0];
                if (toplam > 13) { hata = Koltuklar[k] + ": renk uzunluklarının alt sınırları toplamı 13'ü aşıyor."; return null; }
            }
            kosul.Dagitici = _ihale.Count > 0 ? _ihaleDagitici : _cmbDagitici.SelectedIndex - 1;
            kosul.Ihale = new List<string>(_ihale);
            return kosul;
        }

        private static bool Uygun(List<string> deste, Kosul c)
        {
            for (int k = 0; k < 4; k++)
            {
                int hp = 0; var uz = new int[4];
                for (int i = 0; i < 13; i++)
                {
                    string ad = deste[k * 13 + i];
                    char renk = ad[ad.Length - 5];
                    string rank = ad.Substring(0, ad.Length - 5);
                    uz[renk == 'S' ? 0 : renk == 'H' ? 1 : renk == 'D' ? 2 : 3]++;
                    hp += rank == "A" ? 4 : rank == "K" ? 3 : rank == "Q" ? 2 : rank == "J" ? 1 : 0;
                }
                int dag = 0;
                for (int s = 0; s < 4; s++) dag += uz[s] == 0 ? 3 : uz[s] == 1 ? 2 : uz[s] == 2 ? 1 : 0;
                int toplam = hp + dag;

                bool toplamSerbest = c.Aralik[k, 0, 0] <= 0 && c.Aralik[k, 0, 1] >= 37;
                if (!toplamSerbest && (toplam < c.Aralik[k, 0, 0] || toplam > c.Aralik[k, 0, 1])) return false;
                if (hp < c.Aralik[k, 1, 0] || hp > c.Aralik[k, 1, 1]) return false;
                for (int s = 0; s < 4; s++)
                    if (uz[s] < c.Aralik[k, 2 + s, 0] || uz[s] > c.Aralik[k, 2 + s, 1]) return false;
            }
            return true;
        }

        // ───────────────────────── Üretim ─────────────────────────

        /// <summary>Hiçbir el koşulu değiştirilmemiş mi (puan 0-37, uzunluk 0-13)?</summary>
        private bool TumKosullarVarsayilanMi()
        {
            for (int k = 0; k < 4; k++)
                for (int r = 0; r < 6; r++)
                {
                    string enAz = _kutu[k, r, 0].Text.Trim(), enCok = _kutu[k, r, 1].Text.Trim();
                    if ((enAz != "" && enAz != "0") || (enCok != "" && enCok != (r < 2 ? "37" : "13"))) return false;
                }
            return true;
        }

        private void Calistir()
        {
            if (_ihale.Count == 0 && TumKosullarVarsayilanMi())
            {
                MessageBox.Show(this, "Anlaşma özellikleri boş! Tekrar deneyin!", "Kart Dağıtıcı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string hata;
            var kosul = KosulOku(out hata);
            int baslangic, adet;
            if (kosul == null) { Bildir(hata); return; }
            if (!int.TryParse(_txtSeed.Text, out baslangic)) { Bildir("Başlangıç seed sayı olmalı."); return; }
            if (!int.TryParse(_txtAdet.Text, out adet) || adet < 1) { Bildir("Bulunacak dağıtım sayısı en az 1 olmalı."); return; }

            _lstDagitimlar.Items.Clear();
            lock (_kilit) { _bulunan.Clear(); }
            _test = 0;
            _txtEller.Text = "";
            _btnKaydet.Enabled = false; _btnOyna.Enabled = false;
            _calisiyor = true;
            _btnCalistir.Text = "Durdur";
            _btnTemizle.Enabled = false;
            _saat.Restart();
            _zamanlayici.Start();

            _iptal = new CancellationTokenSource();
            var jeton = _iptal.Token;
            // İhale koşulu: taraf başına ortaklık anlaşması ve motor (bu iş parçacığına özel örnekler)
            var anlasmaNS = AnlasmaYonetici.Mevcut(Taraf.NS);
            var anlasmaEW = AnlasmaYonetici.Mevcut(Taraf.EW);
            IhaleMotoru motorNS = null, motorEW = null;
            if (kosul.Ihale.Count > 0) { motorNS = new IhaleMotoru(anlasmaNS); motorEW = new IhaleMotoru(anlasmaEW); }
            Task.Run(() =>
            {
                int deneme = 0;
                for (long i = 0; i < AzamiDeneme && !jeton.IsCancellationRequested; i++)
                {
                    int seed = unchecked(baslangic + (int)i);
                    if (kosul.Dagitici >= 0 && (((long)seed % 4 + 4) % 4) != kosul.Dagitici) continue;   // dağıtıcı seed'den belirlenir
                    deneme++;
                    Interlocked.Exchange(ref _test, deneme);
                    var deste = DagitimUretici.Uret(DagitimProfili.Rastgele, null, seed);
                    if (!Uygun(deste, kosul)) continue;
                    if (kosul.Ihale.Count > 0 && !KonvansiyonPratigi.IhaleEslesir(motorNS, motorEW, anlasmaNS, anlasmaEW, deste, seed, kosul.Ihale)) continue;
                    int bulunanAdet;
                    lock (_kilit) { _bulunan.Add(seed); bulunanAdet = _bulunan.Count; }
                    if (bulunanAdet >= adet) break;
                }
            }, jeton).ContinueWith(t =>
            {
                try { BeginInvoke(new Action(Bitti)); } catch { }
            });
        }

        private void Durdur()
        {
            if (_iptal != null) _iptal.Cancel();
        }

        private void Bitti()
        {
            _zamanlayici.Stop();
            _saat.Stop();
            _calisiyor = false;
            _btnCalistir.Text = "Çalıştır";
            _btnTemizle.Enabled = true;
            Guncelle();
            if (_bulunan.Count == 0 && !(_iptal != null && _iptal.IsCancellationRequested))
                Bildir("Koşulu sağlayan dağıtım bulunamadı (" + AzamiDeneme.ToString("N0") + " denemeye kadar arandı). Koşulları gevşetmeyi dene.");
        }

        private void Guncelle()
        {
            _lblTest.Text = Volatile.Read(ref _test).ToString("N0");
            List<int> kopya;
            lock (_kilit) { kopya = _bulunan.ToList(); }
            _lblGecerli.Text = kopya.Count.ToString();
            _lblZaman.Text = _saat.Elapsed.ToString(@"hh\:mm\:ss");

            if (_lstDagitimlar.Items.Count != kopya.Count)
            {
                for (int i = _lstDagitimlar.Items.Count; i < kopya.Count; i++)
                {
                    int seed = kopya[i];
                    // Dağıtıcı sabitse (ihale girilmiş ya da kutuda seçili) satırda yalnızca seed; "Herhangi" ise kısa harf (K/D/G/B) eklenir
                    bool sabit = _ihale.Count > 0 || _cmbDagitici.SelectedIndex > 0;
                    _lstDagitimlar.Items.Add(sabit ? "#" + seed : "#" + seed + "  " + KoltukAd[DagiticiOf(seed)].Substring(0, 1));
                }
                bool var = kopya.Count > 0;
                _btnKaydet.Enabled = var; _btnOyna.Enabled = var;
                if (_lstDagitimlar.SelectedIndex < 0 && var) _lstDagitimlar.SelectedIndex = 0;
            }
        }

        private static int DagiticiOf(int seed)
        {
            return (int)(((long)seed % 4 + 4) % 4);
        }

        // ───────────────────────── Sonuçlar ─────────────────────────

        private List<int> Secilenler(bool bosaHepsi)
        {
            List<int> hepsi;
            lock (_kilit) { hepsi = _bulunan.ToList(); }
            var sec = _lstDagitimlar.SelectedIndices.Cast<int>().Where(i => i < hepsi.Count).Select(i => hepsi[i]).ToList();
            return (sec.Count == 0 && bosaHepsi) ? hepsi : sec;
        }

        private static string[] El(List<string> deste, int koltuk)
        {
            // ♠♥♦♣ sırasıyla, her renk yüksekten alçağa
            var renkler = new[] { 'S', 'H', 'D', 'C' };
            var sonuc = new string[4];
            for (int s = 0; s < 4; s++)
            {
                var kartlar = new List<string>();
                for (int i = 0; i < 13; i++)
                {
                    string ad = deste[koltuk * 13 + i];
                    if (ad[ad.Length - 5] == renkler[s]) kartlar.Add(ad.Substring(0, ad.Length - 5));
                }
                kartlar.Sort((a, b) => Deger(b).CompareTo(Deger(a)));
                sonuc[s] = kartlar.Count == 0 ? "-" : string.Join("", kartlar.Select(x => x == "10" ? "T" : x));
            }
            return sonuc;
        }

        private static int Deger(string r)
        {
            switch (r) { case "A": return 14; case "K": return 13; case "Q": return 12; case "J": return 11; default: return int.Parse(r); }
        }

        private void SeciliGoster()
        {
            var sec = Secilenler(false);
            if (sec.Count == 0) { _txtEller.Text = ""; return; }
            int seed = sec[0];
            var deste = DagitimUretici.Uret(DagitimProfili.Rastgele, null, seed);
            var n = El(deste, 0); var e = El(deste, 1); var s = El(deste, 2); var w = El(deste, 3);
            var sb = new StringBuilder();
            sb.AppendLine("Dağıtım #" + seed + "   Dağıtıcı: " + KoltukAd[DagiticiOf(seed)]);
            if (_ihale.Count > 0) sb.AppendLine("İhale: " + string.Join(" ", _ihale));
            sb.AppendLine();
            string bosluk = new string(' ', 14);
            sb.AppendLine(bosluk + "KUZEY");
            for (int i = 0; i < 4; i++) sb.AppendLine(bosluk + "♠♥♦♣"[i] + " " + n[i]);
            sb.AppendLine();
            for (int i = 0; i < 4; i++)
                sb.AppendLine(("♠♥♦♣"[i] + " " + w[i]).PadRight(24) + "♠♥♦♣"[i] + " " + e[i]);
            sb.AppendLine("  BATI".PadRight(24) + "DOĞU");
            sb.AppendLine();
            sb.AppendLine(bosluk + "GÜNEY");
            for (int i = 0; i < 4; i++) sb.AppendLine(bosluk + "♠♥♦♣"[i] + " " + s[i]);
            _txtEller.Text = sb.ToString();
        }

        private void PbnKaydet()
        {
            var seedler = Secilenler(true);
            if (seedler.Count == 0) return;
            using (var f = new SaveFileDialog { Filter = "PBN dosyası (*.pbn)|*.pbn", FileName = "dagitimlar.pbn", Title = "Dağıtımları kaydet" })
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine("% PBN 2.1");
                sb.AppendLine("% Kart Dağıtıcı çıktısı");
                int no = 1;
                foreach (int seed in seedler)
                {
                    var deste = DagitimUretici.Uret(DagitimProfili.Rastgele, null, seed);
                    string[] pbnSira = { "N", "E", "S", "W" };
                    int d = DagiticiOf(seed);
                    var parcalar = new List<string>();
                    for (int k = 0; k < 4; k++)
                        parcalar.Add(string.Join(".", El(deste, k)).Replace("-", ""));
                    sb.AppendLine();
                    sb.AppendLine("[Event \"Kart Dağıtıcı\"]");
                    sb.AppendLine("[Board \"" + no++ + "\"]");
                    sb.AppendLine("[Dealer \"" + pbnSira[d] + "\"]");
                    sb.AppendLine("[Deal \"N:" + string.Join(" ", parcalar) + "\"]");
                    sb.AppendLine("[Note \"seed " + seed + "\"]");
                    if (_ihale.Count > 0)
                    {
                        sb.AppendLine("[Auction \"" + pbnSira[d] + "\"]");
                        sb.AppendLine(string.Join(" ", _ihale.Select(PbnTeklif)));
                    }
                }
                try { File.WriteAllText(f.FileName, sb.ToString(), new UTF8Encoding(false)); }
                catch (Exception ex) { Bildir("Dosya yazılamadı: " + ex.Message); }
            }
        }

        private static string PbnTeklif(string t)
        {
            switch (t)
            {
                case "Pas": return "Pass";
                case "Dbl": return "X";
                case "RDbl": return "XX";
                default: return t.Replace("♣", "C").Replace("♦", "D").Replace("♥", "H").Replace("♠", "S");
            }
        }

        private void Oyna()
        {
            // Oyna: birden fazla dağıtım seçildiyse yalnızca onlar; tek seçim (listede otomatik seçili ilk satır dahil) ya da seçim yoksa
            // seçili dağıtımdan başlayarak listedeki TÜM dağıtımlar sırayla oynanır
            List<int> hepsi;
            lock (_kilit) { hepsi = _bulunan.ToList(); }
            var seciliSirasi = _lstDagitimlar.SelectedIndices.Cast<int>().Where(i => i < hepsi.Count).ToList();
            List<int> seedler;
            if (seciliSirasi.Count > 1) seedler = seciliSirasi.Select(i => hepsi[i]).ToList();
            else seedler = hepsi.Skip(seciliSirasi.Count == 1 ? seciliSirasi[0] : 0).ToList();
            if (seedler.Count == 0) return;
            var anaSayfa = (Owner as AnaSayfa) ?? Application.OpenForms.OfType<AnaSayfa>().FirstOrDefault();
            if (anaSayfa == null) { Bildir("Ana pencere bulunamadı."); return; }
            var liste = seedler.Select(sd => new PratikDagitim { Seed = sd, Konvansiyon = "Kart Dağıtıcı", Profil = DagitimProfili.Rastgele }).ToList();
            // Oyun penceresi açıkken dağıtıcı gizlenir; pratik bitince geri gelmez, doğrudan ana ekrana dönülür
            Hide();
            // duyur=true: her dağıtımda "Pratik n/N" duyurusu; liste bitince "Pratik tamamlandı" mesajı
            try { anaSayfa.PratikOyunuAc(liste, true); }
            finally { if (!IsDisposed) Close(); }   // ana ekranı PratikOyunuAc zaten geri getirir
        }

        private void Cikis()
        {
            var anaSayfa = (Owner as AnaSayfa) ?? Application.OpenForms.OfType<AnaSayfa>().FirstOrDefault();
            if (anaSayfa != null)
            {
                anaSayfa.ResetToInitialState();
                anaSayfa.Show();
            }
            Close();
        }

        private void Bildir(string mesaj)
        {
            MessageBox.Show(this, mesaj, "Kart Dağıtıcı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
