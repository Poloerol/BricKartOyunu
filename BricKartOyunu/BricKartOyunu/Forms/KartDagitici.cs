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
        private Label _lblIhale;
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

        private void Kur()
        {
            Text = "Kart Dağıtıcı";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ClientSize = new Size(760, 560);
            Font = new Font("Segoe UI", 9F);

            var gEller = new GroupBox { Text = "Eller", Location = new Point(10, 8), Size = new Size(470, 262) };
            for (int k = 0; k < 4; k++)
                gEller.Controls.Add(new Label
                {
                    Text = Koltuklar[k], AutoSize = false, TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font(Font, FontStyle.Bold), Location = new Point(110 + k * 88, 18), Size = new Size(84, 18)
                });
            for (int k = 0; k < 4; k++)
                gEller.Controls.Add(new Label
                {
                    Text = "min - max", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font(Font.FontFamily, 7.5F), Location = new Point(110 + k * 88, 36), Size = new Size(84, 14)
                });
            for (int r = 0; r < 6; r++)
            {
                int y = 56 + r * 33;
                gEller.Controls.Add(new Label { Text = Satirlar[r], AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Location = new Point(8, y), Size = new Size(98, 22) });
                for (int k = 0; k < 4; k++)
                    for (int m = 0; m < 2; m++)
                    {
                        var t = new TextBox { Location = new Point(112 + k * 88 + m * 40, y), Size = new Size(36, 22), MaxLength = 2, TextAlign = HorizontalAlignment.Center };
                        t.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
                        _kutu[k, r, m] = t;
                        gEller.Controls.Add(t);
                    }
            }

            var lblDagitici = new Label { Text = "Dağıtıcı:", AutoSize = true, Location = new Point(14, 284) };
            _cmbDagitici = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(74, 280), Width = 110 };
            _cmbDagitici.Items.AddRange(new object[] { "Herhangi", "Kuzey", "Doğu", "Güney", "Batı" });

            _btnIhale = new Button { Text = "İhale Belirle…", Location = new Point(194, 278), Size = new Size(110, 26) };
            _btnIhale.Click += (s, e) => IhaleBelirle();
            _lblIhale = new Label { AutoSize = false, AutoEllipsis = true, Location = new Point(310, 283), Size = new Size(170, 18), ForeColor = Color.DimGray };

            var lblSecili = new Label { Text = "Seçili dağıtım:", AutoSize = true, Location = new Point(14, 314) };
            _txtEller = new TextBox { Location = new Point(10, 334), Size = new Size(470, 150), Multiline = true, ReadOnly = true, Font = new Font("Consolas", 9F), ScrollBars = ScrollBars.Vertical };

            var btnNS = new Button { Text = "NS Ortaklık Profili", Location = new Point(10, 496), Size = new Size(140, 28) };
            var btnEW = new Button { Text = "EW Ortaklık Profili", Location = new Point(160, 496), Size = new Size(140, 28) };
            btnNS.Click += (s, e) => { using (var f = new AnlasmaForm(Taraf.NS)) f.ShowDialog(this); };
            btnEW.Click += (s, e) => { using (var f = new AnlasmaForm(Taraf.EW)) f.ShowDialog(this); };

            // Üret
            var gUret = new GroupBox { Text = "Üret", Location = new Point(496, 8), Size = new Size(246, 262) };
            gUret.Controls.Add(new Label { Text = "Başlangıç seed:", AutoSize = true, Location = new Point(14, 24) });
            _txtSeed = new TextBox { Location = new Point(14, 44), Size = new Size(110, 22), MaxLength = 9, TextAlign = HorizontalAlignment.Center };
            _txtSeed.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            gUret.Controls.Add(_txtSeed);
            gUret.Controls.Add(new Label { Text = "Bulunacak dağıtım:", AutoSize = true, Location = new Point(14, 78) });
            _txtAdet = new TextBox { Location = new Point(14, 98), Size = new Size(60, 22), MaxLength = 3, TextAlign = HorizontalAlignment.Center };
            _txtAdet.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            gUret.Controls.Add(_txtAdet);
            gUret.Controls.Add(new Label { Text = "Süre:", AutoSize = true, Location = new Point(14, 132) });
            _lblZaman = new Label { Text = "00:00:00", AutoSize = true, Location = new Point(14, 150) };
            gUret.Controls.Add(_lblZaman);
            _btnCalistir = new Button { Text = "Çalıştır", Location = new Point(14, 180), Size = new Size(100, 30) };
            _btnTemizle = new Button { Text = "Tümünü Temizle", Location = new Point(14, 218), Size = new Size(100, 30) };
            _btnCalistir.Click += (s, e) => { if (_calisiyor) Durdur(); else Calistir(); };
            _btnTemizle.Click += (s, e) => { if (!_calisiyor) VarsayilanlariYukle(); };
            gUret.Controls.Add(_btnCalistir);
            gUret.Controls.Add(_btnTemizle);

            // Dağıtımlar
            var gDag = new GroupBox { Text = "Dağıtımlar", Location = new Point(496, 280), Size = new Size(246, 204) };
            gDag.Controls.Add(new Label { Text = "Denenen:", AutoSize = true, Location = new Point(12, 22) });
            _lblTest = new Label { Text = "0", AutoSize = true, Location = new Point(110, 22) };
            gDag.Controls.Add(_lblTest);
            gDag.Controls.Add(new Label { Text = "Geçerli:", AutoSize = true, Location = new Point(12, 42) });
            _lblGecerli = new Label { Text = "0", AutoSize = true, Location = new Point(110, 42) };
            gDag.Controls.Add(_lblGecerli);
            _lstDagitimlar = new ListBox { Location = new Point(12, 64), Size = new Size(222, 130), IntegralHeight = false, SelectionMode = SelectionMode.MultiExtended };
            _lstDagitimlar.SelectedIndexChanged += (s, e) => SeciliGoster();
            gDag.Controls.Add(_lstDagitimlar);

            _btnKaydet = new Button { Text = "Kaydet (PBN)", Location = new Point(496, 494), Size = new Size(100, 30), Enabled = false };
            _btnOyna = new Button { Text = "Oyna", Location = new Point(606, 494), Size = new Size(60, 30), Enabled = false };
            var btnCikis = new Button { Text = "Çıkış", Location = new Point(676, 494), Size = new Size(66, 30) };
            _btnKaydet.Click += (s, e) => PbnKaydet();
            _btnOyna.Click += (s, e) => Oyna();
            btnCikis.Click += (s, e) => Cikis();

            _zamanlayici = new System.Windows.Forms.Timer { Interval = 150 };
            _zamanlayici.Tick += (s, e) => Guncelle();

            Controls.AddRange(new Control[] { gEller, lblDagitici, _cmbDagitici, _btnIhale, _lblIhale, lblSecili, _txtEller, btnNS, btnEW, gUret, gDag, _btnKaydet, _btnOyna, btnCikis });
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
            if (_ihale.Count == 0)
            {
                _lblIhale.Text = "İhale koşulu yok";
                _cmbDagitici.Enabled = true;
                return;
            }
            _lblIhale.Text = KoltukAd[_ihaleDagitici] + ": " + string.Join(" ", _ihale);
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

        private void Calistir()
        {
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
                    _lstDagitimlar.Items.Add("#" + seed + "  (" + KoltukAd[DagiticiOf(seed)] + ")");
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
            var seedler = Secilenler(true);
            if (seedler.Count == 0) return;
            var anaSayfa = (Owner as AnaSayfa) ?? Application.OpenForms.OfType<AnaSayfa>().FirstOrDefault();
            if (anaSayfa == null) { Bildir("Ana pencere bulunamadı."); return; }
            var liste = seedler.Select(sd => new PratikDagitim { Seed = sd, Konvansiyon = "Kart Dağıtıcı", Profil = DagitimProfili.Rastgele }).ToList();
            // Oyun penceresi açıkken dağıtıcı gizlenir; bulunan dağıtımlar korunur, oyun bitince geri gelir
            Hide();
            try { anaSayfa.PratikOyunuAc(liste, false); }
            finally { if (!IsDisposed) Show(); }
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
