using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Oynama Karnesi: ihale motorunun ulaştığı kontratları kart oynama motoruyla (OyunZekasi + PIMC) dört elde oynar ve
    /// deklaran tarafının aldığı löveyi çift dummy lövesiyle karşılaştırır. Fark = gerçek - DD löve:
    /// negatif → deklaran/atak tarafı DD'nin gerisinde, pozitif → savunma DD'nin gerisinde.
    /// Satıra çift tıklayınca o bordun oynanışı gösterilir. Bord numarası seed'dir (Motor Karnesi ile aynı dağıtımlar).
    /// </summary>
    public sealed class OynamaKarneForm : Form
    {
        private readonly NumericUpDown _ilk, _adet;
        private readonly ComboBox _pimc;
        private readonly Button _basla;
        private readonly ProgressBar _ilerleme;
        private readonly Label _durum;
        private readonly TextBox _ozet;
        private readonly ListView _liste;
        private CancellationTokenSource _iptal;
        private bool _calisiyor;
        private readonly List<OynamaBordu> _sonuclar = new List<OynamaBordu>();

        private static readonly string[] Sem = { "♠", "♥", "♦", "♣", "NT" };
        private static readonly string[] Dek = { "K", "D", "G", "B" };

        public OynamaKarneForm()
        {
            Text = "Oynama Karnesi";
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(980, 660);
            MinimumSize = new Size(820, 520);
            ShowInTaskbar = false;

            var ust = new Panel { Dock = DockStyle.Top, Height = 48 };
            ust.Controls.Add(new Label { Text = "İlk Bord:", AutoSize = true, Location = new Point(12, 15) });
            _ilk = new NumericUpDown { Minimum = 1, Maximum = 1000000, Value = 1, Width = 76, Location = new Point(70, 11) };
            ust.Controls.Add(_ilk);
            ust.Controls.Add(new Label { Text = "Bord Sayısı:", AutoSize = true, Location = new Point(160, 15) });
            _adet = new NumericUpDown { Minimum = 1, Maximum = 5000, Value = 50, Width = 64, Location = new Point(236, 11) };
            ust.Controls.Add(_adet);
            ust.Controls.Add(new Label { Text = "Oynama gücü:", AutoSize = true, Location = new Point(314, 15) });
            _pimc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, Location = new Point(396, 11) };
            _pimc.Items.AddRange(new object[] { "Hızlı (yalnız kurallar, PIMC kapalı)", "Orta (PIMC 300 ms)", "Tam (PIMC 900 ms)" });
            _pimc.SelectedIndex = 1;
            ust.Controls.Add(_pimc);
            _basla = new Button { Text = "Başlat", Width = 90, Height = 28, Location = new Point(670, 9) };
            _basla.Click += (s, e) => { if (_calisiyor) Durdur(); else Baslat(); };
            ust.Controls.Add(_basla);

            _ilerleme = new ProgressBar { Dock = DockStyle.Top, Height = 6 };
            _durum = new Label { Dock = DockStyle.Top, Height = 24, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), Text = "Hazır. Bord başına yaklaşık 1-8 sn sürer (oynama gücüne göre)." };
            _ozet = new TextBox { Dock = DockStyle.Top, Height = 92, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9.5f), BackColor = SystemColors.Window };
            _liste = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, HideSelection = false };
            foreach (var h in new[] { "Bord", "Kontrat", "Deklaran", "Atak", "DD löve", "Gerçek", "Fark", "IMP" })
                _liste.Columns.Add(h, h == "Bord" ? 60 : h == "Atak" ? 70 : 74, HorizontalAlignment.Left);
            _liste.DoubleClick += (s, e) =>
            {
                if (_liste.SelectedItems.Count == 0) return;
                var o = _liste.SelectedItems[0].Tag as OynamaBordu;
                if (o != null) MessageBox.Show(this, "Bord " + o.BordNo + "  " + o.Sonuc.Seviye + Sem[o.Sonuc.Koz] + " (" + Dek[o.Sonuc.Deklaran] + ")\nDD " + o.DdLove + "  gerçek " + o.GercekLove + "\n\n" + o.Oynanis.Replace(" | ", "\n"), "Oynanış", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            Controls.Add(_liste);
            Controls.Add(_ozet);
            Controls.Add(_durum);
            Controls.Add(_ilerleme);
            Controls.Add(ust);
            FormClosing += (s, e) => { if (_iptal != null) _iptal.Cancel(); };
        }

        private void Durdur()
        {
            if (_iptal != null) _iptal.Cancel();
            _durum.Text = "Durduruluyor… (süren bord bitince)";
        }

        private void Baslat()
        {
            _calisiyor = true;
            _basla.Text = "Durdur";
            _sonuclar.Clear();
            _liste.Items.Clear();
            _ozet.Text = "";
            int ilk = (int)_ilk.Value, adet = (int)_adet.Value;
            int pimcSecim = _pimc.SelectedIndex;
            _ilerleme.Maximum = adet; _ilerleme.Value = 0;
            _iptal = new CancellationTokenSource();
            var jeton = _iptal.Token;
            var anlasmaNS = AnlasmaYonetici.Mevcut(Taraf.NS); var anlasmaEW = AnlasmaYonetici.Mevcut(Taraf.EW);
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // PIMC ayarlarını geçici değiştir, işi bitince geri koy (gerçek oyunu etkilemesin)
            bool eskiAktif = PimcOyuncu.Aktif; int eskiMs = PimcOyuncu.ZamanSiniriMs; int[] eskiSure = PimcOyuncu.ElSureleriMs;
            PimcOyuncu.ElSureleriMs = null;
            if (pimcSecim == 0) PimcOyuncu.Aktif = false; else { PimcOyuncu.Aktif = true; PimcOyuncu.ZamanSiniriMs = pimcSecim == 1 ? 300 : 900; }

            Task.Run(() =>
            {
                var mNS = new IhaleMotoru(anlasmaNS); var mEW = new IhaleMotoru(anlasmaEW);
                int biten = 0;
                for (int no = ilk; no < ilk + adet && !jeton.IsCancellationRequested; no++)
                {
                    OynamaBordu o = null;
                    try
                    {
                        var b = MotorKarnesi.Ihale(no, no, mNS, mEW);
                        MotorKarnesi.Degerlendir(b);
                        o = OynamaKarnesi.Oyna(b, OynamaKarnesi.ElleriYukle(no), k => AnlasmaYonetici.Mevcut((k == Player.Kuzey || k == Player.Guney) ? Taraf.NS : Taraf.EW));
                    }
                    catch { }
                    biten++;
                    var oo = o; int bb = biten;
                    try
                    {
                        BeginInvoke((Action)(() =>
                        {
                            if (oo != null) { _sonuclar.Add(oo); SatirEkle(oo); }
                            _ilerleme.Value = Math.Min(_ilerleme.Maximum, bb);
                            _durum.Text = bb + " / " + adet + " bord  (" + (sw.ElapsedMilliseconds / 1000) + " sn)";
                            _ozet.Text = OzetMetni(adet);
                        }));
                    }
                    catch { }
                }
                PimcOyuncu.Aktif = eskiAktif; PimcOyuncu.ZamanSiniriMs = eskiMs; PimcOyuncu.ElSureleriMs = eskiSure;
                try
                {
                    BeginInvoke((Action)(() =>
                    {
                        _calisiyor = false; _basla.Text = "Başlat";
                        _durum.Text = "Bitti: " + biten + " bord, " + (sw.ElapsedMilliseconds / 1000) + " sn";
                        _ozet.Text = OzetMetni(adet);
                    }));
                }
                catch { }
            });
        }

        private void SatirEkle(OynamaBordu o)
        {
            var it = new ListViewItem(new[]
            {
                o.BordNo.ToString(), o.Sonuc.Seviye + Sem[o.Sonuc.Koz], Dek[o.Sonuc.Deklaran], o.AtakKarti ?? "",
                o.DdLove.ToString(), o.GercekLove.ToString(), (o.Fark > 0 ? "+" : "") + o.Fark, (o.Imp > 0 ? "+" : "") + o.Imp
            });
            it.Tag = o;
            if (o.Fark < 0) it.ForeColor = Color.FromArgb(190, 30, 30);
            else if (o.Fark > 0) it.ForeColor = Color.FromArgb(30, 100, 190);
            _liste.Items.Add(it);
        }

        private string OzetMetni(int adet)
        {
            var oz = OynamaKarnesi.Ozetle(_sonuclar, adet);
            if (oz.Oynanan == 0) return "";
            var sb = new StringBuilder();
            sb.AppendLine("Oynanan kontrat: " + oz.Oynanan + "   DD ile aynı: " + oz.Tam + " (%" + (100 * oz.Tam / oz.Oynanan) + ")");
            sb.AppendLine("Deklaran/atak tarafı DD'nin gerisinde (kırmızı): " + oz.Eksik + " bord, toplam " + oz.ToplamEksikLove + " löve");
            sb.AppendLine("Savunma DD'nin gerisinde (mavi): " + oz.Fazla + " bord, toplam " + oz.ToplamFazlaLove + " löve");
            sb.AppendLine("Ortalama fark (gerçek - DD löve): " + oz.OrtalamaFark.ToString("0.000") + "   Ortalama IMP (deklaran gözüyle): " + oz.OrtalamaImp.ToString("0.00"));
            return sb.ToString();
        }
    }
}
