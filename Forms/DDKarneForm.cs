using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Motor Karnesi: ihale motorunu iki tarafın etkin anlaşmasıyla seri bordlarda oynatır, her ihaleyi çift dummy par'ı ile
    /// karşılaştırır. Hata sınıflarının dağılımı, ortalama IMP sapması ve "en kötü eller" listesi çıkar;
    /// bir satıra çift tıklayınca o bord Çift Dummy Analizi'nde açılır. Bord numarası seed'dir (aynı bord tekrar üretilebilir).
    /// </summary>
    public sealed class DDKarneForm : Form
    {
        private readonly NumericUpDown _ilk, _adet;
        private readonly Button _basla;
        private readonly ProgressBar _ilerleme;
        private readonly Label _durum;
        private readonly ListView _liste;
        private readonly OzetKanvasi _ozet;
        private readonly ComboBox _filtre;
        private readonly System.Windows.Forms.Timer _zamanlayici;
        private readonly ConcurrentQueue<KarneBord> _gelen = new ConcurrentQueue<KarneBord>();
        private readonly List<KarneBord> _bordlar = new List<KarneBord>();
        private CancellationTokenSource _iptal;
        private int _hedef, _bitti;
        private bool _calisiyor;
        private System.Diagnostics.Stopwatch _sure;

        public DDKarneForm()
        {
            this.Text = "Motor Karnesi";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(1160, 640);
            this.ClientSize = new Size(1200, 700);
            this.ShowInTaskbar = false;
            this.BackColor = OzetKanvasi.Zemin;
            this.ForeColor = OzetKanvasi.Metin;
            this.Font = new Font("Segoe UI", 9f);

            var ust = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = OzetKanvasi.Zemin };
            ust.Controls.Add(EtiketYap("İlk Bord:", 16));
            _ilk = new NumericUpDown { Minimum = 1, Maximum = 1000000, Value = 1, Width = 80, Location = new Point(74, 13) };
            ust.Controls.Add(_ilk);
            ust.Controls.Add(EtiketYap("Bord Sayısı:", 172));
            _adet = new NumericUpDown { Minimum = 1, Maximum = 5000, Value = 200, Width = 70, Location = new Point(246, 13) };
            ust.Controls.Add(_adet);
            _basla = new Button { Text = "Başlat", Width = 96, Height = 28, Location = new Point(332, 11), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(46, 140, 96), ForeColor = Color.White };
            _basla.FlatAppearance.BorderSize = 0;
            _basla.Click += (s, e) => { if (_calisiyor) Durdur(); else Baslat(); };
            ust.Controls.Add(_basla);
            ust.Controls.Add(EtiketYap("Filtre:", 450));
            _filtre = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190, Location = new Point(492, 13) };
            _filtre.Items.Add("Tümü");
            foreach (IhaleHatasi h in Enum.GetValues(typeof(IhaleHatasi))) _filtre.Items.Add(MotorKarnesi.HataAdi(h));
            _filtre.SelectedIndex = 0;
            _filtre.SelectedIndexChanged += (s, e) => ListeyiDoldur();
            ust.Controls.Add(_filtre);
            _ilerleme = new ProgressBar { Width = 140, Height = 10, Location = new Point(700, 20), Style = ProgressBarStyle.Continuous };
            ust.Controls.Add(_ilerleme);
            _durum = new Label { AutoSize = false, Width = 160, Height = 20, Location = new Point(850, 17), ForeColor = OzetKanvasi.Soluk, Text = "Hazır" };
            ust.Controls.Add(_durum);
            var kopya = new Button { Text = "Panoya Kopyala", Width = 120, Height = 28, Location = new Point(1000, 11), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(40, 62, 52), ForeColor = Color.White };
            kopya.FlatAppearance.BorderSize = 0;
            kopya.Click += (s, e) => PanoyaKopyala();
            ust.Controls.Add(kopya);
            ust.Resize += (s, e) => { kopya.Left = Math.Max(1010, ust.ClientSize.Width - kopya.Width - 16); };
            kopya.BringToFront();

            _ozet = new OzetKanvasi { Dock = DockStyle.Top, Height = 262 };

            _liste = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = false, HideSelection = false,
                BackColor = Color.FromArgb(18, 31, 25), ForeColor = OzetKanvasi.Metin, BorderStyle = BorderStyle.None
            };
            _liste.Columns.Add("Bord", 62);
            _liste.Columns.Add("Dağıtıcı", 70);
            _liste.Columns.Add("İhale", 330);
            _liste.Columns.Add("Kontrat", 90);
            _liste.Columns.Add("DD", 44);
            _liste.Columns.Add("Par", 150);
            _liste.Columns.Add("IMP", 50);
            _liste.Columns.Add("Sapma", 56);
            _liste.Columns.Add("Sorumlu", 66);
            _liste.Columns.Add("Değerlendirme", 190);
            _liste.DoubleClick += (s, e) => Ac();
            _liste.ColumnClick += (s, e) => Sirala(e.Column);

            this.Controls.Add(_liste);
            this.Controls.Add(_ozet);
            this.Controls.Add(ust);

            _zamanlayici = new System.Windows.Forms.Timer { Interval = 350 };
            _zamanlayici.Tick += (s, e) => Boşalt();
            this.FormClosing += (s, e) => { if (_iptal != null) _iptal.Cancel(); _zamanlayici.Stop(); };
        }

        private static Label EtiketYap(string s, int x) { return new Label { Text = s, AutoSize = true, Location = new Point(x, 17), ForeColor = OzetKanvasi.Soluk }; }

        private static readonly string[] Harf = { "N", "E", "S", "W" };
        private int _siraKol = 7; private bool _siraArtan;

        private void Baslat()
        {
            if (!DDMotor.DdsVar)
            {
                MessageBox.Show(this, "Hızlı çift dummy çözücüsü (dds.dll) yüklenemedi; yerleşik çözücü bord başına dakikalar sürer ve işlemciyi doldurur.\n\nMotor Karnesi bu durumda çalıştırılmaz.\n\nAyrıntı: " + DDMotor.DdsHata, "Motor Karnesi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _bordlar.Clear(); _liste.Items.Clear(); KarneBord ignore; while (_gelen.TryDequeue(out ignore)) { }
            _hedef = (int)_adet.Value; _bitti = 0; _ilerleme.Maximum = _hedef; _ilerleme.Value = 0;
            _iptal = new CancellationTokenSource(); _calisiyor = true; _basla.Text = "Durdur";
            _sure = System.Diagnostics.Stopwatch.StartNew();
            _durum.Text = "Çalışıyor…";
            int ilk = (int)_ilk.Value;
            var ns = AnlasmaYonetici.Mevcut(Taraf.NS); var ew = AnlasmaYonetici.Mevcut(Taraf.EW);
            var token = _iptal.Token;
            _zamanlayici.Start();
            Task.Run(() =>
            {
                try { MotorKarnesi.Calistir(ilk, _hedef, ns, ew, (b, n) => _gelen.Enqueue(b), token); }
                catch { }
                finally { try { BeginInvoke(new Action(Bitti)); } catch { } }
            });
        }

        /// <summary>Tüm bordları (Sapma'ya göre sıralı) sekmeyle ayrılmış metin olarak panoya koyar; Excel'e ya da sohbete yapıştırılabilir.</summary>
        private void PanoyaKopyala()
        {
            var l = _bordlar.Where(b => b.Degerlendirme != null).OrderByDescending(b => b.Degerlendirme.Sapma).ToList();
            if (l.Count == 0) return;
            var o = MotorKarnesi.Ozetle(l);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("OZET\tbord=" + o.Bord + "\tpar%=" + (100.0 * o.ParAlan / o.Bord).ToString("0.0") + "\tort_sapma=" + o.OrtalamaImpKaybi.ToString("0.00") +
                          "\tNS_sapma=" + o.NsSapma.ToString("0") + "\tEW_sapma=" + o.EwSapma.ToString("0") + "\tilk_bord=" + l.Min(b => b.BordNo) + "\tmotor=" + (DDMotor.DdsVar ? "DDS" : "yedek"));
            foreach (var kv in o.Hatalar.OrderByDescending(x => x.Value)) sb.AppendLine("SINIF\t" + MotorKarnesi.HataAdi(kv.Key) + "\t" + kv.Value);
            sb.AppendLine("Bord\tDagitici\tIhale\tKontrat\tDD\tPar\tIMP\tSapma\tSorumlu\tDegerlendirme\tN\tE\tS\tW");
            foreach (var b in l)
            {
                var d = b.Degerlendirme;
                sb.AppendLine(b.BordNo + "\t" + Harf[b.Dagitici] + "\t" + b.IhaleMetni + "\t" + Kontrat(b) + "\t" + (b.Sonuc.Pas ? "-" : d.DdLove.ToString()) + "\t" +
                              (d.Par.Pas ? "0" : d.Par.Metin.Replace("Par: ", "")) + "\t" + d.ImpDeklaran + "\t" + d.Sapma + "\t" + (d.Sorumlu == 0 ? "NS" : d.Sorumlu == 1 ? "EW" : "-") + "\t" +
                              MotorKarnesi.HataAdi(d.Hata) + "\t" + El(b.Eller[0]) + "\t" + El(b.Eller[1]) + "\t" + El(b.Eller[2]) + "\t" + El(b.Eller[3]));
            }
            try { Clipboard.SetText(sb.ToString()); _durum.Text = l.Count + " bord panoya kopyalandı"; }
            catch (Exception ex) { MessageBox.Show(this, "Panoya kopyalanamadı: " + ex.Message, "Motor Karnesi"); }
        }

        private static string El(ulong m)
        {
            string r = "AKQJT98765432"; var sb = new System.Text.StringBuilder();
            string[] sim = { "S", "H", "D", "C" };
            for (int su = 0; su < 4; su++)
            {
                sb.Append(sim[su]);
                for (int k = 12; k >= 0; k--) if ((m >> (su * 16 + k) & 1UL) != 0) sb.Append(r[12 - k]);
                if (su < 3) sb.Append(' ');
            }
            return sb.ToString();
        }

        private void Durdur() { if (_iptal != null) _iptal.Cancel(); }

        private void Bitti()
        {
            Boşalt();
            _zamanlayici.Stop();
            _calisiyor = false; _basla.Text = "Başlat";
            _durum.Text = _bordlar.Count + " bord • " + (_sure.Elapsed.TotalSeconds).ToString("0.0") + " sn" + (DDMotor.DdsVar ? " • DDS" : " • yerleşik çözücü");
            _siraKol = 7; _siraArtan = false; ListeyiDoldur();
        }

        private void Boşalt()
        {
            KarneBord b; bool yeni = false;
            while (_gelen.TryDequeue(out b)) { _bordlar.Add(b); _bitti++; yeni = true; }
            if (!yeni) return;
            _ilerleme.Value = Math.Min(_ilerleme.Maximum, _bitti);
            _durum.Text = _bitti + " / " + _hedef;
            _ozet.Guncelle(_bordlar);
            if (_bordlar.Count % 25 < 3 || !_calisiyor) ListeyiDoldur();
        }

        private IEnumerable<KarneBord> Gorunen()
        {
            IEnumerable<KarneBord> l = _bordlar;
            if (_filtre.SelectedIndex > 0)
            {
                var h = (IhaleHatasi)(_filtre.SelectedIndex - 1);
                l = l.Where(b => b.Degerlendirme != null && b.Degerlendirme.Hata == h);
            }
            return l.Where(b => b.Degerlendirme != null);
        }

        private static string Kontrat(KarneBord b)
        {
            return b.Sonuc.Pas ? "Pas" : ParHesabi.KontratMetni(b.Sonuc.Seviye, b.Sonuc.Koz, b.Sonuc.Kontr) + " " + Harf[b.Sonuc.Deklaran];
        }

        private void ListeyiDoldur()
        {
            Func<KarneBord, IComparable> anahtar;
            switch (_siraKol)
            {
                case 0: anahtar = b => b.BordNo; break;
                case 1: anahtar = b => b.Dagitici; break;
                case 2: anahtar = b => b.IhaleMetni; break;
                case 3: anahtar = b => Kontrat(b); break;
                case 4: anahtar = b => b.Degerlendirme.DdLove; break;
                case 5: anahtar = b => b.Degerlendirme.ParNsSkor; break;
                case 6: anahtar = b => b.Degerlendirme.ImpDeklaran; break;
                case 8: anahtar = b => b.Degerlendirme.Sorumlu; break;
                case 9: anahtar = b => b.Degerlendirme.Hata.ToString(); break;
                default: anahtar = b => b.Degerlendirme.Sapma; break;   // 7
            }
            var sirali = _siraArtan ? Gorunen().OrderBy(anahtar) : Gorunen().OrderByDescending(anahtar);
            _liste.BeginUpdate();
            _liste.Items.Clear();
            foreach (var b in sirali.Take(2000))
            {
                var d = b.Degerlendirme;
                var it = new ListViewItem(new[]
                {
                    b.BordNo.ToString(), Harf[b.Dagitici], b.IhaleMetni, Kontrat(b),
                    b.Sonuc.Pas ? "–" : d.DdLove.ToString(), d.Par.Pas ? "0" : d.Par.Metin.Replace("Par: ", ""),
                    d.ImpDeklaran.ToString(), d.Sapma.ToString(), d.Sorumlu == 0 ? "N/S" : d.Sorumlu == 1 ? "E/W" : "–", MotorKarnesi.HataAdi(d.Hata)
                }) { Tag = b };
                if (d.Hata == IhaleHatasi.Yok) it.ForeColor = Color.FromArgb(120, 220, 160);
                else if (d.Sapma >= 10) it.ForeColor = Color.FromArgb(255, 130, 110);
                else if (d.Sapma >= 5) it.ForeColor = Color.FromArgb(255, 200, 100);
                _liste.Items.Add(it);
            }
            _liste.EndUpdate();
        }

        private void Sirala(int kolon)
        {
            if (kolon == _siraKol) _siraArtan = !_siraArtan; else { _siraKol = kolon; _siraArtan = kolon == 0; }
            ListeyiDoldur();
        }

        private void Ac()
        {
            if (_liste.SelectedItems.Count == 0) return;
            var b = (KarneBord)_liste.SelectedItems[0].Tag;
            string baslik = "Bord " + b.BordNo + " — Motor Karnesi";
            using (var f = new DDAnalizForm(b.Eller, b.Dagitici, b.NsZon, b.EwZon, b.Sonuc, b.IhaleMetni, baslik, b.Tablo))
                f.ShowDialog(this);
        }

        /// <summary>Test/ekran görüntüsü için sonuçları verip özeti çizer.</summary>
        public Bitmap OzetGoruntusu(List<KarneBord> bordlar)
        {
            _ozet.Guncelle(bordlar);
            var bmp = new Bitmap(_ozet.Width > 0 ? _ozet.Width : 1040, _ozet.Height);
            using (var g = Graphics.FromImage(bmp)) _ozet.Ciz(g, bmp.Width, bmp.Height);
            return bmp;
        }

        // ═════════════════════════════════════════════════════════════
        private sealed class OzetKanvasi : Control
        {
            public static readonly Color Zemin = Color.FromArgb(13, 24, 19);
            public static readonly Color Panel = Color.FromArgb(22, 38, 30);
            public static readonly Color Cizgi = Color.FromArgb(46, 70, 58);
            public static readonly Color Metin = Color.FromArgb(232, 239, 233);
            public static readonly Color Soluk = Color.FromArgb(146, 170, 156);
            private KarneOzeti _o = new KarneOzeti();
            private List<KarneBord> _b = new List<KarneBord>();
            private readonly Font _fBuyuk = new Font("Segoe UI Semibold", 22f, FontStyle.Bold);
            private readonly Font _fOrta = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
            private readonly Font _fKucuk = new Font("Segoe UI", 8.5f);

            public OzetKanvasi()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }

            public void Guncelle(List<KarneBord> b) { _b = new List<KarneBord>(b); _o = MotorKarnesi.Ozetle(_b); Invalidate(); }
            protected override void OnPaint(PaintEventArgs e) { Ciz(e.Graphics, Width, Height); }

            private static GraphicsPath Yuvarlak(RectangleF r, float y)
            {
                var p = new GraphicsPath(); float d = y * 2;
                p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                p.CloseFigure(); return p;
            }

            private static void Dolgu(Graphics g, RectangleF r, Color c, float y)
            {
                using (var p = Yuvarlak(r, y)) using (var b = new SolidBrush(c)) g.FillPath(b, p);
            }

            private static void Yazi(Graphics g, string s, Font f, Color c, RectangleF r, StringAlignment h = StringAlignment.Near)
            {
                using (var sf = new StringFormat { Alignment = h, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
                using (var br = new SolidBrush(c)) g.DrawString(s, f, br, r, sf);
            }

            private static Color HataRengi(IhaleHatasi h)
            {
                switch (h)
                {
                    case IhaleHatasi.Yok: return Color.FromArgb(46, 160, 100);
                    case IhaleHatasi.ParUstu: return Color.FromArgb(70, 190, 150);
                    case IhaleHatasi.RakipBuyukSlam: return Color.FromArgb(160, 60, 160);
                    case IhaleHatasi.RakipSlam: return Color.FromArgb(190, 80, 170);
                    case IhaleHatasi.RakipOyun: return Color.FromArgb(225, 135, 70);
                    case IhaleHatasi.RakipFedakarlik2: return Color.FromArgb(90, 120, 200);
                    case IhaleHatasi.RakipRekabet: return Color.FromArgb(140, 150, 90);
                    case IhaleHatasi.KucukFark: return Color.FromArgb(150, 160, 70);
                    case IhaleHatasi.YanlisKoz: return Color.FromArgb(220, 160, 60);
                    case IhaleHatasi.OyunKacirildi: return Color.FromArgb(230, 120, 60);
                    case IhaleHatasi.SlamKacirildi:
                    case IhaleHatasi.BuyukSlamKacirildi: return Color.FromArgb(190, 80, 170);
                    case IhaleHatasi.Batis: return Color.FromArgb(210, 70, 60);
                    case IhaleHatasi.RakipFedakarlik: return Color.FromArgb(100, 140, 210);
                    default: return Color.FromArgb(130, 130, 150);
                }
            }

            public void Ciz(Graphics g, int w, int h)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.Clear(Zemin);
                var kart = new RectangleF(12, 4, 300, h - 12);
                Dolgu(g, kart, Panel, 12);
                Yazi(g, "BORD", _fKucuk, Soluk, new RectangleF(30, 12, 100, 18));
                Yazi(g, _o.Bord.ToString(), _fBuyuk, Metin, new RectangleF(30, 30, 120, 40));
                double parYuz = _o.Bord == 0 ? 0 : 100.0 * _o.ParAlan / _o.Bord;
                Yazi(g, "PAR SONUCU", _fKucuk, Soluk, new RectangleF(170, 12, 120, 18));
                Yazi(g, parYuz.ToString("0") + "%", _fBuyuk, Color.FromArgb(120, 220, 160), new RectangleF(170, 30, 130, 40));
                Yazi(g, "PAR'DAN SAPMA / BORD", _fKucuk, Soluk, new RectangleF(30, 82, 280, 18));
                Yazi(g, _o.OrtalamaImpKaybi.ToString("0.0") + " IMP", _fBuyuk, _o.OrtalamaImpKaybi > 3 ? Color.FromArgb(255, 140, 110) : Metin, new RectangleF(30, 100, 200, 40));
                Yazi(g, _o.Bord == 0 ? "" : "N/S sapma " + _o.NsSapma.ToString("0") + " • E/W sapma " + _o.EwSapma.ToString("0") + " IMP", _fKucuk, Soluk, new RectangleF(30, 164, 270, 18));
                Yazi(g, _o.Bord == 0 ? "" : "çift dummy " + (_o.DdMs / Math.Max(1, _o.Bord)).ToString("0") + " ms/bord", _fKucuk, Soluk, new RectangleF(30, 184, 270, 18));

                // hata dağılımı
                var br0 = new RectangleF(328, 4, w - 328 - 12, h - 12);
                Dolgu(g, br0, Panel, 12);
                Yazi(g, "HATA SINIFLARI", _fKucuk, Soluk, new RectangleF(br0.X + 18, br0.Y + 8, 160, 18));
                var sirali = _o.Hatalar.OrderByDescending(kv => kv.Value).Take(10).ToList();
                int en = Math.Max(1, sirali.Count == 0 ? 1 : sirali[0].Value);
                float y = br0.Y + 30; float bw = br0.Width - 36 - 190 - 50;
                foreach (var kv in sirali)
                {
                    Yazi(g, MotorKarnesi.HataAdi(kv.Key), _fKucuk, Metin, new RectangleF(br0.X + 18, y, 170, 18));
                    var bar = new RectangleF(br0.X + 190, y + 2, Math.Max(3, bw * kv.Value / en), 14);
                    Dolgu(g, bar, HataRengi(kv.Key), 5);
                    Yazi(g, kv.Value.ToString(), _fKucuk, Metin, new RectangleF(bar.Right + 6, y, 60, 18));
                    y += 21;
                }
            }
        }
    }
}
