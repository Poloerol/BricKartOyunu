using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Ortaklık profili ekranı (Bridge Baron "Pair Profile" düzeninde): profil seçimi/yönetimi,
    /// sistem ön ayarı, beceri düzeyi, agresiflik ve seçili konvansiyonların kategori özetleri.
    /// Konvansiyonların ayrıntılı seçimi "Düzenle" ile açılır. Tamam ile tüm profiller kaydedilir.
    /// </summary>
    public sealed class AnlasmaForm : Form
    {
        private const string Ozel = "Özel";

        private sealed class OnAyar
        {
            public string Ad; public Func<OrtaklikAnlasmasi> Uret;
            public OnAyar(string ad, Func<OrtaklikAnlasmasi> uret) { Ad = ad; Uret = uret; }
        }

        private static readonly OnAyar[] OnAyarlar =
        {
            new OnAyar("Standart (5'li majör, güçlü NT)", () => OrtaklikAnlasmasi.Varsayilan()),
            new OnAyar("Başlangıç (basit)", () => OrtaklikAnlasmasi.Basit()),
            new OnAyar("Zayıf NT (12–14)", () => { var a = OrtaklikAnlasmasi.Varsayilan(); a.StrongNT = false; return a; })
        };

        // (özellik adı, görünen ad) — özet kutularında seçili olanlar listelenir
        private static readonly string[] AgrAdlari = { "Temkinli", "Orta", "Atak" };

        private sealed class Kutu
        {
            public string Baslik; public GroupBox Grup; public Label Metin;
            public Func<OrtaklikAnlasmasi, string> Ozet;
        }

        private readonly Taraf _taraf;
        private List<AnlasmaProfil> _profiller;
        private AnlasmaProfil _secili;
        private bool _yukleniyor;

        private ComboBox _cmbProfil, _cmbSistem, _cmbAcilis, _cmbMudahale, _cmbPreempt;
        private TrackBar _beceri;
        private Label _lblBeceri;
        private CheckBox _chkOgrenme;
        private Button _btnSil, _btnDuzenle;
        private readonly List<Kutu> _kutular = new List<Kutu>();

        public AnlasmaForm(Taraf taraf)
        {
            _taraf = taraf;
            _profiller = AnlasmaYonetici.Profiller(taraf);
            string aktif = AnlasmaYonetici.AktifProfilAdi(taraf);
            _secili = _profiller.FirstOrDefault(p => p.Ad == aktif) ?? _profiller[0];
            Kur();
            ProfilListesiniDoldur();
            SeciliyiGoster();
        }

        // ───────────────────────── Özet metinleri ─────────────────────────

        private static string Liste(OrtaklikAnlasmasi a, params string[] oge)
        {
            // oge: "Alan=Görünen ad" çiftleri; yalnız açık olanlar
            var secili = new List<string>();
            foreach (var o in oge)
            {
                int i = o.IndexOf('=');
                var p = typeof(OrtaklikAnlasmasi).GetProperty(o.Substring(0, i));
                if (p != null && (bool)p.GetValue(a, null)) secili.Add(o.Substring(i + 1));
            }
            return secili.Count == 0 ? "—" : string.Join(", ", secili);
        }

        private void KutulariTanimla()
        {
            Ekle("1NT açılışı", a =>
                (a.StrongNT ? "15–17 HP" : "12–14 HP") + ", " + Liste(a,
                    "Stayman=Stayman", "JacobyTransfer=Jacoby transfer", "Smolen=Smolen", "MinorTransfer=Minör transferi",
                    "Lebensohl=Lebensohl", "Cappelletti=Cappelletti", "Dont=DONT", "MultiLandy=Multi-Landy"));
            Ekle("Minör açılış ve gelişimler", a => Liste(a,
                "InvertedMinors=Ters minörler", "OneClub_AnswerSistemi=1♣ cevap sistemi", "Reverse=Reverse",
                "XYZ=XYZ", "TwoWayCheckback=İki yönlü checkback", "ThirdSuitForcing=3. renk zorlaması", "FourthSuitForcing=4. renk zorlaması"));
            Ekle("Majör açılış ve gelişimler", a =>
                (a.BesliMajor ? "5'li majör" : "4'lü majör") + ", " + Liste(a,
                    "BergenRaises=Bergen", "Jacoby2NT=Jacoby 2NT", "Splinter=Splinter", "TrialBid=Deneme teklifi",
                    "Drury=Drury", "ReverseDrury=Ters Drury", "ForcingNT=Zorlayıcı 1NT"));
            Ekle("2NT / 3NT", a => "2NT: 20–21 HP, " + Liste(a, "PuppetStayman=Puppet Stayman"));
            Ekle("Yüksek seviye açılışlar", a => Liste(a,
                "IkiliSinekGuclu=2♣ güçlü", "ZayifIki=Zayıf iki", "Multi2D=Multi 2♦", "Preempt=Önleyici açılışlar", "Namyats=Namyats"));
            Ekle("Slam konvansiyonları", a => Liste(a,
                "Blackwood=Blackwood", "RKCB=RKCB", "RKCB1430=RKCB 1430", "Gerber=Gerber", "CueBid=Cue bid",
                "Serious3NT=Ciddi 3NT", "NonSerious3NT=Ciddi olmayan 3NT", "LastTrain=Last Train"));
            Ekle("Rekabet ve savunma", a => Liste(a,
                "NegativeDouble=Negatif kontr", "TakeoutDouble=Takeout kontr", "SupportDouble=Support kontr",
                "ResponsiveDouble=Responsive kontr", "Michaels=Michaels", "Unusual2NT=Unusual 2NT"));
            Ekle("İşaretleşme", a => Liste(a, "StandartSignal=Standart", "UDCA=UDCA"));
        }

        private void Ekle(string baslik, Func<OrtaklikAnlasmasi, string> ozet)
        {
            var k = new Kutu { Baslik = baslik, Ozet = ozet };
            k.Grup = new GroupBox { Text = baslik, Dock = DockStyle.Fill, Margin = new Padding(4) };
            k.Metin = new Label { Dock = DockStyle.Fill, AutoSize = false, Padding = new Padding(4, 2, 4, 2) };
            k.Grup.Controls.Add(k.Metin);
            _kutular.Add(k);
        }

        // ───────────────────────── Arayüz ─────────────────────────

        private static ComboBox AgrKutusu()
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
            c.Items.AddRange(AgrAdlari);
            return c;
        }

        private void Kur()
        {
            Text = (_taraf == Taraf.NS ? "NS" : "EW") + " Ortaklık Profili";
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(860, 600);
            ClientSize = new Size(940, 640);
            Font = new Font("Segoe UI", 9F);

            // Sol sütun
            var sol = new Panel { Dock = DockStyle.Left, Width = 330, Padding = new Padding(10) };

            var btnYeni = new Button { Text = "Yeni", Location = new Point(10, 10), Size = new Size(70, 28) };
            var btnKopya = new Button { Text = "Kopyala", Location = new Point(86, 10), Size = new Size(70, 28) };
            _btnDuzenle = new Button { Text = "Düzenle", Location = new Point(162, 10), Size = new Size(76, 28) };
            _btnSil = new Button { Text = "Sil", Location = new Point(244, 10), Size = new Size(70, 28) };
            btnYeni.Click += (s, e) => YeniProfil(false);
            btnKopya.Click += (s, e) => YeniProfil(true);
            _btnDuzenle.Click += (s, e) => KonvansiyonlariDuzenle();
            _btnSil.Click += (s, e) => ProfilSil();

            var lblProfil = new Label { Text = "Profil:", AutoSize = true, Location = new Point(10, 52) };
            _cmbProfil = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(10, 70), Width = 304 };
            _cmbProfil.SelectedIndexChanged += (s, e) => ProfilDegisti();

            var lblSistem = new Label { Text = "İhale sistemi:", AutoSize = true, Location = new Point(10, 104) };
            _cmbSistem = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(10, 122), Width = 304 };
            _cmbSistem.Items.AddRange(OnAyarlar.Select(o => (object)o.Ad).ToArray());
            _cmbSistem.Items.Add(Ozel);
            _cmbSistem.SelectionChangeCommitted += (s, e) => OnAyarUygula();

            _lblBeceri = new Label { AutoSize = true, Location = new Point(10, 160) };
            _beceri = new TrackBar { Minimum = 1, Maximum = 20, TickFrequency = 1, Location = new Point(4, 180), Width = 316, Height = 40 };
            _beceri.ValueChanged += (s, e) => BeceriDegisti();
            var lblBeceriIpucu = new Label
            {
                Text = "20 = hatasız. Düşük değerde yapay zekâ kart oynarken arada hata yapar.",
                AutoSize = false, Size = new Size(304, 32), Location = new Point(10, 220), ForeColor = Color.DimGray
            };

            var gAgr = new GroupBox { Text = "Agresiflik", Location = new Point(10, 262), Size = new Size(304, 120) };
            gAgr.Controls.Add(new Label { Text = "Müdahale:", AutoSize = true, Location = new Point(10, 28) });
            gAgr.Controls.Add(new Label { Text = "Önleyici açılış:", AutoSize = true, Location = new Point(10, 60) });
            gAgr.Controls.Add(new Label { Text = "Açılış:", AutoSize = true, Location = new Point(10, 92) });
            _cmbMudahale = AgrKutusu(); _cmbMudahale.Location = new Point(170, 24);
            _cmbPreempt = AgrKutusu(); _cmbPreempt.Location = new Point(170, 56);
            _cmbAcilis = AgrKutusu(); _cmbAcilis.Location = new Point(170, 88);
            foreach (var c in new[] { _cmbMudahale, _cmbPreempt, _cmbAcilis })
            {
                c.SelectedIndexChanged += (s, e) => AgresiflikDegisti();
                gAgr.Controls.Add(c);
            }

            _chkOgrenme = new CheckBox
            {
                Text = "Öğrenme modu (teklifleri motor önerisiyle karşılaştır)",
                AutoSize = false, Size = new Size(304, 36), Location = new Point(10, 396),
                Checked = AnlasmaYonetici.OgrenmeModuAcik
            };

            sol.Controls.AddRange(new Control[] { btnYeni, btnKopya, _btnDuzenle, _btnSil, lblProfil, _cmbProfil,
                lblSistem, _cmbSistem, _lblBeceri, _beceri, lblBeceriIpucu, gAgr, _chkOgrenme });

            // Alt: Tamam / İptal
            var alt = new Panel { Dock = DockStyle.Bottom, Height = 48 };
            var btnTamam = new Button { Text = "Tamam", Size = new Size(96, 30), Anchor = AnchorStyles.Right | AnchorStyles.Top };
            var btnIptal = new Button { Text = "İptal", Size = new Size(96, 30), Anchor = AnchorStyles.Right | AnchorStyles.Top };
            btnTamam.Location = new Point(alt.Width - 216, 9);
            btnIptal.Location = new Point(alt.Width - 108, 9);
            btnTamam.Click += BtnTamam_Click;
            btnIptal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            alt.Controls.Add(btnTamam);
            alt.Controls.Add(btnIptal);
            AcceptButton = btnTamam;
            CancelButton = btnIptal;

            // Sağ: kategori özet kutuları (iki sütun)
            KutulariTanimla();
            var tablo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = (_kutular.Count + 1) / 2, Padding = new Padding(4) };
            tablo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tablo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < tablo.RowCount; i++) tablo.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / tablo.RowCount));
            for (int i = 0; i < _kutular.Count; i++)
                tablo.Controls.Add(_kutular[i].Grup, i % 2, i / 2);

            Controls.Add(tablo);
            Controls.Add(sol);
            Controls.Add(alt);
        }

        // ───────────────────────── Davranış ─────────────────────────

        private void ProfilListesiniDoldur()
        {
            _yukleniyor = true;
            try
            {
                _cmbProfil.Items.Clear();
                foreach (var p in _profiller) _cmbProfil.Items.Add(p.Ad);
                _cmbProfil.SelectedItem = _secili.Ad;
            }
            finally { _yukleniyor = false; }
            _btnSil.Enabled = _profiller.Count > 1;
        }

        private void ProfilDegisti()
        {
            if (_yukleniyor) return;
            var p = _profiller.FirstOrDefault(x => x.Ad == (string)_cmbProfil.SelectedItem);
            if (p == null) return;
            _secili = p;
            SeciliyiGoster();
        }

        private void SeciliyiGoster()
        {
            _yukleniyor = true;
            try
            {
                var a = _secili.Anlasma;
                _beceri.Value = Math.Max(1, Math.Min(20, a.BeceriDuzeyi));
                _cmbAcilis.SelectedIndex = Math.Max(0, Math.Min(2, a.AcilisAgresifligi + 1));
                _cmbMudahale.SelectedIndex = Math.Max(0, Math.Min(2, a.MudahaleAgresifligi + 1));
                _cmbPreempt.SelectedIndex = Math.Max(0, Math.Min(2, a.PreemptAgresifligi + 1));
                _lblBeceri.Text = "Beceri düzeyi: " + _beceri.Value;
                SistemEtiketiniGuncelle();
            }
            finally { _yukleniyor = false; }
            OzetleriGuncelle();
        }

        private static readonly string[] BayrakAdlari = typeof(OrtaklikAnlasmasi).GetProperties()
            .Where(p => p.PropertyType == typeof(bool) && p.CanWrite).Select(p => p.Name).ToArray();

        private static bool AyniBayraklar(OrtaklikAnlasmasi x, OrtaklikAnlasmasi y)
        {
            foreach (var n in BayrakAdlari)
            {
                var p = typeof(OrtaklikAnlasmasi).GetProperty(n);
                if ((bool)p.GetValue(x, null) != (bool)p.GetValue(y, null)) return false;
            }
            return true;
        }

        private void SistemEtiketiniGuncelle()
        {
            for (int i = 0; i < OnAyarlar.Length; i++)
            {
                if (AyniBayraklar(_secili.Anlasma, OnAyarlar[i].Uret()))
                {
                    _cmbSistem.SelectedIndex = i;
                    return;
                }
            }
            _cmbSistem.SelectedItem = Ozel;
        }

        private void OnAyarUygula()
        {
            int i = _cmbSistem.SelectedIndex;
            if (i < 0 || i >= OnAyarlar.Length) return;   // "Özel" seçimi bir şey değiştirmez
            var yeni = OnAyarlar[i].Uret();
            var hedef = _secili.Anlasma;
            foreach (var n in BayrakAdlari)
            {
                var p = typeof(OrtaklikAnlasmasi).GetProperty(n);
                p.SetValue(hedef, p.GetValue(yeni, null), null);
            }
            OzetleriGuncelle();
        }

        private void BeceriDegisti()
        {
            if (_yukleniyor) return;
            _secili.Anlasma.BeceriDuzeyi = _beceri.Value;
            _lblBeceri.Text = "Beceri düzeyi: " + _beceri.Value;
        }

        private void AgresiflikDegisti()
        {
            if (_yukleniyor) return;
            var a = _secili.Anlasma;
            a.AcilisAgresifligi = _cmbAcilis.SelectedIndex - 1;
            a.MudahaleAgresifligi = _cmbMudahale.SelectedIndex - 1;
            a.PreemptAgresifligi = _cmbPreempt.SelectedIndex - 1;
        }

        private void OzetleriGuncelle()
        {
            foreach (var k in _kutular) k.Metin.Text = k.Ozet(_secili.Anlasma);
        }

        private void KonvansiyonlariDuzenle()
        {
            using (var f = new KonvansiyonSecimForm(_secili.Anlasma))
            {
                if (f.ShowDialog(this) == DialogResult.OK && f.Sonuc != null)
                {
                    // Agresiflik/beceri bu ekranda; yalnız konvansiyon bayrakları geri alınır
                    foreach (var n in BayrakAdlari)
                    {
                        var p = typeof(OrtaklikAnlasmasi).GetProperty(n);
                        p.SetValue(_secili.Anlasma, p.GetValue(f.Sonuc, null), null);
                    }
                    _yukleniyor = true;
                    try { SistemEtiketiniGuncelle(); } finally { _yukleniyor = false; }
                    OzetleriGuncelle();
                }
            }
        }

        private string BenzersizAd(string ad)
        {
            string aday = ad; int n = 2;
            while (_profiller.Any(p => string.Equals(p.Ad, aday, StringComparison.CurrentCultureIgnoreCase)))
                aday = ad + " " + n++;
            return aday;
        }

        private void YeniProfil(bool kopya)
        {
            string onerilen = kopya ? _secili.Ad + " (kopya)" : "Yeni profil";
            string ad = AdSor(kopya ? "Kopyanın adı:" : "Yeni profilin adı:", onerilen);
            if (string.IsNullOrWhiteSpace(ad)) return;
            ad = BenzersizAd(ad.Trim());
            var yeni = kopya ? _secili.Kopya(ad)
                             : new AnlasmaProfil { Ad = ad, Anlasma = OrtaklikAnlasmasi.Varsayilan() };
            _profiller.Add(yeni);
            _secili = yeni;
            ProfilListesiniDoldur();
            SeciliyiGoster();
        }

        private void ProfilSil()
        {
            if (_profiller.Count <= 1) return;
            if (MessageBox.Show(this, "\"" + _secili.Ad + "\" profili silinsin mi?", "Profil sil",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int i = _profiller.IndexOf(_secili);
            _profiller.Remove(_secili);
            _secili = _profiller[Math.Max(0, i - 1)];
            ProfilListesiniDoldur();
            SeciliyiGoster();
        }

        private string AdSor(string soru, string varsayilan)
        {
            using (var f = new Form())
            {
                f.Text = "Profil adı";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowInTaskbar = false;
                f.MinimizeBox = false; f.MaximizeBox = false;
                f.ClientSize = new Size(320, 110);
                var l = new Label { Text = soru, AutoSize = true, Location = new Point(12, 12) };
                var t = new TextBox { Text = varsayilan, Location = new Point(12, 34), Width = 296 };
                var ok = new Button { Text = "Tamam", DialogResult = DialogResult.OK, Location = new Point(138, 70), Size = new Size(80, 28) };
                var iptal = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Location = new Point(228, 70), Size = new Size(80, 28) };
                f.Controls.AddRange(new Control[] { l, t, ok, iptal });
                f.AcceptButton = ok; f.CancelButton = iptal;
                t.SelectAll();
                return f.ShowDialog(this) == DialogResult.OK ? t.Text : null;
            }
        }

        private void BtnTamam_Click(object sender, EventArgs e)
        {
            if (!AnlasmaYonetici.Kaydet(_taraf, _profiller, _secili.Ad))
            {
                MessageBox.Show(this, "Profiller kaydedilemedi. Bu oturumda geçerli olacak.", "Ortaklık Profili",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            AnlasmaYonetici.OgrenmeModuAcik = _chkOgrenme.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
