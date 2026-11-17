using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Oyun Analizi: oynanan her kartı çift dummy ile değerlendirir. "Kayıp" = o pozisyonda en iyi kartla oynanan kart arasındaki
    /// löve farkı (oynayan tarafın gözüyle). Kırmızı: deklaran tarafı, mavi: savunma. Yalnız oynanmış kartlar analiz edilir;
    /// bord sürerken de açılabilir (o ana kadarki hamleler).
    /// </summary>
    public sealed class OyunAnaliziForm : Form
    {
        private readonly Dictionary<Player, List<Card>> _eller;
        private readonly List<Tuple<Player, Card>> _oynananlar;
        private readonly string _koz;
        private readonly Player _deklaran;
        private readonly Func<Player, string> _ad;
        private readonly Label _ozet;
        private readonly ProgressBar _ilerleme;
        private readonly CheckBox _sadeceHata;
        private readonly ListView _liste;
        private OyunAnaliziSonuc _sonuc;
        private CancellationTokenSource _iptal;

        public OyunAnaliziForm(Dictionary<Player, List<Card>> eller, List<Tuple<Player, Card>> oynananlar, string koz,
                               Player deklaran, string baslik, Func<Player, string> ad)
        {
            _eller = eller; _oynananlar = oynananlar; _koz = koz; _deklaran = deklaran; _ad = ad ?? (p => p.ToString());
            Text = "Oyun Analizi — " + baslik;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(860, 640);
            MinimumSize = new Size(700, 460);
            ShowInTaskbar = false;

            _ilerleme = new ProgressBar { Dock = DockStyle.Top, Height = 6, Maximum = Math.Max(1, oynananlar.Count) };
            _ozet = new Label { Dock = DockStyle.Top, Height = 72, Padding = new Padding(12, 8, 12, 4), Text = "Çift dummy analizi yapılıyor…" };
            _sadeceHata = new CheckBox { Dock = DockStyle.Top, Height = 26, Padding = new Padding(10, 0, 0, 0), Text = "Yalnız kayıplı hamleleri göster" };
            _sadeceHata.CheckedChanged += (s, e) => ListeyiDoldur();
            _liste = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, HideSelection = false };
            foreach (var h in new[] { "El", "Oyuncu", "Oynanan", "Kayıp", "En iyi kart(lar)", "Not" })
                _liste.Columns.Add(h, h == "En iyi kart(lar)" ? 160 : h == "Not" ? 260 : 80, HorizontalAlignment.Left);

            Controls.Add(_liste);
            Controls.Add(_sadeceHata);
            Controls.Add(_ozet);
            Controls.Add(_ilerleme);
            Shown += (s, e) => Baslat();
            FormClosing += (s, e) => { if (_iptal != null) _iptal.Cancel(); };
        }

        private void Baslat()
        {
            _iptal = new CancellationTokenSource();
            var jeton = _iptal.Token;
            Task.Run(() =>
            {
                OyunAnaliziSonuc r = null; string hata = null;
                try
                {
                    r = OyunAnalizi.Analiz(_eller, _oynananlar, _koz, (i, n) =>
                    {
                        try { BeginInvoke((Action)(() => { _ilerleme.Value = Math.Min(_ilerleme.Maximum, i); })); } catch { }
                    }, jeton);
                }
                catch (Exception ex) { hata = ex.Message; }
                if (jeton.IsCancellationRequested) return;
                try
                {
                    BeginInvoke((Action)(() =>
                    {
                        if (r == null) { _ozet.Text = "Analiz yapılamadı: " + hata; return; }
                        _sonuc = r;
                        _ilerleme.Value = _ilerleme.Maximum;
                        OzetYaz();
                        ListeyiDoldur();
                    }));
                }
                catch { }
            });
        }

        private bool DeklaranTarafiMi(Player p) { return p == _deklaran || p == IhaleDurumuPartner(_deklaran); }
        private static Player IhaleDurumuPartner(Player p) { return BricKartOyunu.Class.Bidding.IhaleDurumu.PartnerBul(p); }

        private void OzetYaz()
        {
            bool deklaranCift = PimcOyuncu.Koltuk(_deklaran) % 2 == 0;        // deklaran tarafı K-G (çift koltuklar) mı?
            int deklKayip = deklaranCift ? _sonuc.NSKayip : _sonuc.EWKayip, deklHamle = deklaranCift ? _sonuc.NSKayipHamle : _sonuc.EWKayipHamle;
            int savKayip = deklaranCift ? _sonuc.EWKayip : _sonuc.NSKayip, savHamle = deklaranCift ? _sonuc.EWKayipHamle : _sonuc.NSKayipHamle;
            int baslangicDekl = deklaranCift ? _sonuc.BaslangicDeger : 13 - _sonuc.BaslangicDeger;
            var sb = new StringBuilder();
            sb.AppendLine("Atak öncesi çift dummy: deklaran tarafı " + baslangicDekl + " löve alabilirdi.");
            sb.AppendLine("Deklaran tarafı: " + deklHamle + " kayıplı hamle, toplam " + deklKayip + " löve   |   Savunma: " + savHamle + " kayıplı hamle, toplam " + savKayip + " löve");
            int gercekDekl = baslangicDekl - deklKayip + savKayip;
            sb.AppendLine("Oynanan hamlelerle deklaran tarafı " + gercekDekl + " löve" + (_sonuc.Adimlar.Count < 52 ? " (oyun sürüyor: o ana kadarki değer)" : "") + ".");
            _ozet.Text = sb.ToString();
        }

        private static string baslangicMetni(int n) { return n.ToString(); }

        private void ListeyiDoldur()
        {
            if (_sonuc == null) return;
            _liste.BeginUpdate();
            _liste.Items.Clear();
            foreach (var a in _sonuc.Adimlar)
            {
                if (_sadeceHata.Checked && a.Kayip == 0) continue;
                string en = a.Kayip > 0 ? string.Join(" ", a.EnIyiler.Select(OyunAnalizi.KartMetni)) : "";
                bool dekl = DeklaranTarafiMi(a.Oyuncu);
                string not = a.Kayip == 0 ? "" : (dekl ? "Deklaran tarafı: " : "Savunma: ") + a.Kayip + " löve kaybettirdi";
                var it = new ListViewItem(new[] { (a.ElNo + 1).ToString(), _ad(a.Oyuncu), OyunAnalizi.KartMetni(a.Kart), a.Kayip == 0 ? "" : "-" + a.Kayip, en, not });
                if (a.Kayip > 0) it.ForeColor = dekl ? Color.FromArgb(190, 30, 30) : Color.FromArgb(30, 100, 190);
                if (a.Sira == 0 && a.ElNo > 0) it.BackColor = Color.FromArgb(246, 246, 246);
                _liste.Items.Add(it);
            }
            _liste.EndUpdate();
        }
    }
}
