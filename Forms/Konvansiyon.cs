using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BricKartOyunu
{
    /// <summary>
    /// Konvansiyon Pratiği: pratik edilecek konvansiyonlar seçilir, her biri için (ihale motoruyla
    /// doğrulanmış) dağıtım numaraları üretilir ve BricOyna bu dağıtımlarla açılır.
    /// </summary>
    public partial class Konvansiyon : Form
    {
        // Önce rastgele dağıtım; yetmezse güçlü/özel profillerle Güney-Kuzey'in güçlü olduğu yönlendirilmiş dağıtım (nadir konvansiyonlar için).
        private static readonly KeyValuePair<DagitimProfili, int>[] Asamalar =
        {
            new KeyValuePair<DagitimProfili, int>(DagitimProfili.Rastgele, 1000),
            new KeyValuePair<DagitimProfili, int>(DagitimProfili.KgOyun, 5000),
            new KeyValuePair<DagitimProfili, int>(DagitimProfili.KgSlam, 4000),
            new KeyValuePair<DagitimProfili, int>(DagitimProfili.KgDavet, 3000),
        };
        private static int AzamiDenemeKonvansiyonBasina { get { return Asamalar.Sum(a => a.Value); } }

        private TextBox _txtAdet, _txtBaslangic, _txtGecerli;
        private CheckBox _chkDuyur, _chkYakinKacis;
        private Label _lblDenenen;
        private ListBox _lstSecili, _lstMevcut, _lstDagitimlar;
        private Button _btnEkle, _btnCikar, _btnUret, _btnPratik, _btnKaydet;

        private readonly List<string> _tumKonvansiyonlar;
        private readonly Dictionary<string, List<PratikDagitim>> _sonuclar = new Dictionary<string, List<PratikDagitim>>();
        private bool _uretiyor, _iptal;

        public Konvansiyon()
        {
            InitializeComponent();

            _tumKonvansiyonlar = KonvansiyonPratigi.Konvansiyonlar();
            ArayuzuKur();
            MevcutListeyiDoldur();
        }

        // ═════════════════════════════════════════════════════════════
        // ARAYÜZ
        // ═════════════════════════════════════════════════════════════
        private void ArayuzuKur()
        {
            Text = "Konvansiyon Pratiği";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 412);
            Font = new Font("Microsoft Sans Serif", 8.25F);

            Controls.Add(new Label { Text = "Konvansiyon başına\npratik eli sayısı", Left = 10, Top = 8, Width = 120, Height = 32 });
            _txtAdet = new TextBox { Text = "8", Left = 134, Top = 10, Width = 44 };
            Controls.Add(_txtAdet);

            Controls.Add(new Label { Text = "Başlangıç", Left = 10, Top = 50, Width = 56 });
            _txtBaslangic = new TextBox { Text = "958097384", Left = 70, Top = 47, Width = 108 };
            Controls.Add(_txtBaslangic);

            _chkDuyur = new CheckBox { Text = "Dağıtımdan önce konvansiyonu duyur", Left = 192, Top = 10, Width = 220, Checked = true };
            Controls.Add(_chkDuyur);
            _chkYakinKacis = new CheckBox { Text = "Yakın kaçışları da üret ++ (yakında)", Left = 192, Top = 38, Width = 220, Enabled = false };
            Controls.Add(_chkYakinKacis);

            Controls.Add(new Label { Text = "Denenen:", Left = 424, Top = 10, Width = 60 });
            _lblDenenen = new Label { Text = "0", Left = 490, Top = 10, Width = 60, TextAlign = ContentAlignment.MiddleLeft };
            Controls.Add(_lblDenenen);
            Controls.Add(new Label { Text = "Geçerli el:", Left = 424, Top = 38, Width = 60 });
            _txtGecerli = new TextBox { Text = "0", Left = 490, Top = 35, Width = 56, ReadOnly = true, TextAlign = HorizontalAlignment.Center };
            Controls.Add(_txtGecerli);

            Controls.Add(new Label { Text = "Pratik edilecek konvansiyonlar", Left = 8, Top = 84, Width = 170 });
            _lstSecili = new ListBox { Left = 8, Top = 104, Width = 170, Height = 188, SelectionMode = SelectionMode.MultiExtended, IntegralHeight = false };
            _lstSecili.SelectedIndexChanged += (s, e) => DagitimListesiniGoster();
            _lstSecili.DoubleClick += (s, e) => Cikar();
            Controls.Add(_lstSecili);

            _btnCikar = new Button { Text = "<-----", Left = 186, Top = 150, Width = 80, Height = 24 };
            _btnCikar.Click += (s, e) => Ekle();   // "<-----": secili konvansiyonu sol listeye ekler
            Controls.Add(_btnCikar);
            _btnEkle = new Button { Text = "----->", Left = 186, Top = 182, Width = 80, Height = 24 };
            _btnEkle.Click += (s, e) => Cikar();   // "----->": sol listeden cikarir
            Controls.Add(_btnEkle);

            Controls.Add(new Label { Text = "Kullanılabilir konvansiyonlar", Left = 274, Top = 84, Width = 170 });
            _lstMevcut = new ListBox { Left = 274, Top = 104, Width = 170, Height = 188, SelectionMode = SelectionMode.MultiExtended, IntegralHeight = false };
            _lstMevcut.DoubleClick += (s, e) => Ekle();
            Controls.Add(_lstMevcut);

            Controls.Add(new Label { Text = "El numaraları", Left = 452, Top = 84, Width = 98 });
            _lstDagitimlar = new ListBox { Left = 452, Top = 104, Width = 98, Height = 188, IntegralHeight = false };
            Controls.Add(_lstDagitimlar);

            _btnUret = new Button { Text = "Üret", Left = 186, Top = 300, Width = 80, Height = 26 };
            _btnUret.Click += BtnUret_Click;
            Controls.Add(_btnUret);

            // Tasarımcıdaki 'btnGeri' düğmesi İptal olarak kullanılır
            btnGeri.Text = "İptal";
            btnGeri.SetBounds(186, 332, 80, 26);
            btnGeri.BringToFront();

            _btnPratik = new Button { Text = "Pratik", Left = 452, Top = 300, Width = 98, Height = 26, Enabled = false };
            _btnPratik.Click += BtnPratik_Click;
            Controls.Add(_btnPratik);
            _btnKaydet = new Button { Text = "Elleri Kaydet", Left = 452, Top = 332, Width = 98, Height = 26, Enabled = false };
            _btnKaydet.Click += BtnKaydet_Click;
            Controls.Add(_btnKaydet);

            Controls.Add(new Label
            {
                Text = "Dağıtımlar, ihale motoruyla doğrulanır: seçilen konvansiyon sizin tarafınızda (Güney/Kuzey) " +
                       "gerçekten kullanılıyorsa el geçerli sayılır. Varsayılan ortaklık anlaşmasında kapalı olanlar listede yoktur.",
                Left = 8, Top = 366, Width = 542, Height = 40
            });
        }

        private void MevcutListeyiDoldur()
        {
            _lstMevcut.Items.Clear();
            foreach (var ad in _tumKonvansiyonlar)
                if (!_lstSecili.Items.Contains(ad)) _lstMevcut.Items.Add(ad);
        }

        private void Ekle()
        {
            if (_uretiyor) return;
            foreach (var o in _lstMevcut.SelectedItems.Cast<object>().ToList())
            {
                _lstSecili.Items.Add(o);
                _lstMevcut.Items.Remove(o);
            }
            SiralaVeDurumuGuncelle();
        }

        private void Cikar()
        {
            if (_uretiyor) return;
            foreach (var o in _lstSecili.SelectedItems.Cast<object>().ToList())
            {
                _lstSecili.Items.Remove(o);
                _sonuclar.Remove((string)o);
            }
            MevcutListeyiSirala();
            SiralaVeDurumuGuncelle();
        }

        private void MevcutListeyiSirala()
        {
            _lstMevcut.Items.Clear();
            foreach (var ad in _tumKonvansiyonlar)
                if (!_lstSecili.Items.Contains(ad)) _lstMevcut.Items.Add(ad);
        }

        private void SiralaVeDurumuGuncelle()
        {
            // Seçili listeyi motor sırasında tut
            var secili = _lstSecili.Items.Cast<string>().OrderBy(a => _tumKonvansiyonlar.IndexOf(a)).ToList();
            _lstSecili.Items.Clear();
            foreach (var a in secili) _lstSecili.Items.Add(a);
            DagitimListesiniGoster();
            PratikDugmeleriniGuncelle();
        }

        private void DagitimListesiniGoster()
        {
            _lstDagitimlar.Items.Clear();
            if (_lstSecili.SelectedItem == null) return;
            List<PratikDagitim> liste;
            if (_sonuclar.TryGetValue((string)_lstSecili.SelectedItem, out liste))
                foreach (var pd in liste) _lstDagitimlar.Items.Add(DagitimMetni(pd));
        }

        private static string DagitimMetni(PratikDagitim pd)
        {
            return pd.Profil == DagitimProfili.Rastgele ? pd.Seed.ToString() : pd.Seed + " (" + pd.Profil + ")";
        }

        private void PratikDugmeleriniGuncelle()
        {
            bool varMi = _sonuclar.Values.Any(l => l.Count > 0);
            _btnPratik.Enabled = varMi && !_uretiyor;
            _btnKaydet.Enabled = varMi && !_uretiyor;
        }

        // ═════════════════════════════════════════════════════════════
        // ÜRET
        // ═════════════════════════════════════════════════════════════
        private void BtnUret_Click(object sender, EventArgs e)
        {
            if (_uretiyor) return;
            if (_lstSecili.Items.Count == 0)
            {
                MessageBox.Show(this, "Önce pratik edilecek konvansiyonları seçin.", "Konvansiyon Pratiği",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int adet, baslangic;
            if (!int.TryParse(_txtAdet.Text.Trim(), out adet) || adet < 1 || adet > 100)
            {
                MessageBox.Show(this, "Pratik eli sayısı 1 ile 100 arasında olmalıdır.", "Konvansiyon Pratiği",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(_txtBaslangic.Text.Trim(), out baslangic) || baslangic < 1)
            {
                MessageBox.Show(this, "Başlangıç numarası pozitif bir tam sayı olmalıdır.", "Konvansiyon Pratiği",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _uretiyor = true; _iptal = false;
            _btnUret.Enabled = _btnEkle.Enabled = _btnCikar.Enabled = false;
            _btnPratik.Enabled = _btnKaydet.Enabled = false;
            _sonuclar.Clear();
            _lstDagitimlar.Items.Clear();

            int denenen = 0, gecerli = 0;
            var eksik = new List<string>();
            string otoSecilen = null;
            try
            {
                var motor = new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan());
                foreach (var ad in _lstSecili.Items.Cast<string>().ToList())
                {
                    var bulunan = new List<PratikDagitim>();
                    _sonuclar[ad] = bulunan;   // canlı: seçilince o ana kadar bulunan eller görünür
                    // Kullanıcı kendi seçmediyse, üretilen konvansiyonu seçili göster
                    if (_lstSecili.SelectedItem == null || (string)_lstSecili.SelectedItem == otoSecilen)
                    {
                        _lstSecili.ClearSelected();
                        _lstSecili.SetSelected(_lstSecili.Items.IndexOf(ad), true);
                        otoSecilen = ad;
                        _lstDagitimlar.Items.Clear();   // yeni konvansiyon: liste baştan dolacak
                    }
                    foreach (var asama in Asamalar)
                    {
                        if (bulunan.Count >= adet || _iptal) break;
                        long seed = baslangic;
                        int bu = 0;
                        while (bulunan.Count < adet && bu < asama.Value && !_iptal)
                        {
                            if (seed > int.MaxValue) seed = 1;
                            if (KonvansiyonPratigi.Gecerli(motor, (int)seed, ad, asama.Key))
                            {
                                var pd = new PratikDagitim { Seed = (int)seed, Konvansiyon = ad, Profil = asama.Key };
                                bulunan.Add(pd);
                                gecerli++;
                                if (_lstSecili.SelectedItem != null && (string)_lstSecili.SelectedItem == ad)
                                {
                                    _lstDagitimlar.Items.Add(DagitimMetni(pd));
                                    _lstDagitimlar.Refresh();
                                }
                            }
                            seed++; bu++; denenen++;
                            if (denenen % 25 == 0 || bu == 1)
                            {
                                _lblDenenen.Text = denenen.ToString();
                                _txtGecerli.Text = gecerli.ToString();
                                Application.DoEvents();
                            }
                        }
                    }
                    if (bulunan.Count < adet && !_iptal) eksik.Add(ad + " (" + bulunan.Count + "/" + adet + ")");
                    if (_iptal) break;
                }
            }
            finally
            {
                _lblDenenen.Text = denenen.ToString();
                _txtGecerli.Text = gecerli.ToString();
                _uretiyor = false;
                _btnUret.Enabled = _btnEkle.Enabled = _btnCikar.Enabled = true;
                if (_lstSecili.SelectedItem == null && _lstSecili.Items.Count > 0) _lstSecili.SelectedIndex = 0;
                DagitimListesiniGoster();
                PratikDugmeleriniGuncelle();
            }

            if (_iptal)
                MessageBox.Show(this, "Üretim durduruldu.", "Konvansiyon Pratiği", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (eksik.Count > 0)
                MessageBox.Show(this,
                    "Bu konvansiyonlar için istenen sayıda el bulunamadı (" + AzamiDenemeKonvansiyonBasina + " denemede):\n\n" +
                    string.Join("\n", eksik) +
                    "\n\nBu konvansiyonlar ihale motorunda nadiren devreye giriyor ya da varsayılan anlaşmada etkisiz.",
                    "Konvansiyon Pratiği", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ═════════════════════════════════════════════════════════════
        // PRATİK / KAYDET
        // ═════════════════════════════════════════════════════════════
        private List<PratikDagitim> PratikListesi()
        {
            var liste = new List<PratikDagitim>();
            foreach (var ad in _lstSecili.Items.Cast<string>())
            {
                List<PratikDagitim> bulunanlar;
                if (!_sonuclar.TryGetValue(ad, out bulunanlar)) continue;
                liste.AddRange(bulunanlar);
            }
            return liste;
        }

        private void BtnPratik_Click(object sender, EventArgs e)
        {
            var liste = PratikListesi();
            if (liste.Count == 0) return;

            var anaSayfa = (this.Owner as AnaSayfa)
                           ?? Application.OpenForms.OfType<AnaSayfa>().FirstOrDefault();
            if (anaSayfa == null) return;
            anaSayfa.PratikOyunuAc(liste, _chkDuyur.Checked);
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            var liste = PratikListesi();
            if (liste.Count == 0) return;
            using (var dlg = new SaveFileDialog
            {
                Title = "Elleri Kaydet",
                Filter = "Metin dosyası (*.txt)|*.txt",
                FileName = "konvansiyon_pratigi.txt"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    File.WriteAllLines(dlg.FileName, liste.Select(p => p.Konvansiyon + ";" + p.Seed + ";" + p.Profil), new UTF8Encoding(true));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Dosya kaydedilemedi:\n" + ex.Message, "Elleri Kaydet",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ═════════════════════════════════════════════════════════════
        // İPTAL / KAPAT
        // ═════════════════════════════════════════════════════════════
        private void BtnGeri_Click(object sender, EventArgs e)
        {
            // Üretim sürerken İptal yalnızca üretimi durdurur
            if (_uretiyor) { _iptal = true; return; }

            // Tip bazlı arama: form adı değişse de bozulmaz
            var anaSayfa = (this.Owner as AnaSayfa)
                           ?? Application.OpenForms.OfType<AnaSayfa>().FirstOrDefault();
            if (anaSayfa != null)
            {
                anaSayfa.ResetToInitialState(); // AnaSayfa’yı ilk haline döndür
                anaSayfa.Show();                // AnaSayfa’yı görünür yap
            }
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _iptal = true;
            base.OnFormClosing(e);
        }
    }
}
