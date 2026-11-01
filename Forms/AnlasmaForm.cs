using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Printing;
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

        private ComboBox _cmbProfil;
        private Label _lblSistem, _lblBeceri;
        private readonly Label[] _lblAgr = new Label[3];   // 0 müdahale, 1 önleyici, 2 açılış
        private readonly int[] _agr = new int[3];          // -1 temkinli, 0 orta, 1 atak
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
                    "Lebensohl=Lebensohl", "Cappelletti=Cappelletti", "Dont=DONT", "MultiLandy=Multi-Landy",
                    "ExtendedStayman=Extended Stayman", "Robinson=Robinson", "ExitTransfer=Exit transfer", "HelloOver1NT=Hello",
                    "Quantitative4NT=Nicel 4NT", "TexasTransfers=Texas", "MinorStayman=Minör Stayman", "Rubensohl=Rubensohl"));
            Ekle("Minör açılış ve gelişimler", a => Liste(a,
                "InvertedMinors=Ters minörler", "Reverse=Reverse",
                "XYZ=XYZ", "TwoWayCheckback=İki yönlü checkback", "ThirdSuitForcing=3. renk zorlaması", "FourthSuitForcing=4. renk zorlaması",
                "OneMinorTwoNTInv=1m–2NT davet", "Walsh=Walsh", "NewMinorForcing=Yeni minör zorlaması", "WolffSignoff=Wolff bırakma", "Kokish=Kokish"));
            Ekle("Majör açılış ve gelişimler", a =>
                (a.BesliMajor ? "5'li majör" : "4'lü majör") + ", " + Liste(a,
                    "BergenRaises=Bergen", "Jacoby2NT=Jacoby 2NT", "Splinter=Splinter", "TrialBid=Deneme teklifi",
                    "Drury=Drury", "ReverseDrury=Ters Drury", "ForcingNT=Zorlayıcı 1NT",
                    "InvertedBergenRaises=Ters Bergen", "LongShortGameTries=Uzun&kısa deneme", "Truscott2NT=Truscott 2NT",
                    "Artificial2C2D=Yapay 2♣/2♦", "PreemptiveReraises=Önleyici reraise", "WeakJumpShiftResp=Zayıf sıçrama cevabı"));
            Ekle("2NT / 3NT", a => "2NT: 20–21 HP, " + Liste(a, "PuppetStayman=Puppet Stayman"));
            Ekle("Yüksek seviye açılışlar", a => Liste(a,
                "IkiliSinekGuclu=2♣ güçlü", "ZayifIki=Zayıf iki", "Multi2D=Multi 2♦", "Preempt=Önleyici açılışlar", "Namyats=Namyats",
                "BenjaminTwoBids=Benjamin", "Gambling3NT=Gambling 3NT", "Flannery=Flannery 2♦", "MiniRoman2D=Mini-Roman 2♦", "Ogust=Ogust", "German2CRelays=Alman 2♣ röleleri"));
            Ekle("Slam konvansiyonları", a => Liste(a,
                "Blackwood=Blackwood", "RKCB=RKCB", "RKCB1430=RKCB 1430", "Gerber=Gerber", "CueBid=Cue bid",
                "Serious3NT=Ciddi 3NT", "NonSerious3NT=Ciddi olmayan 3NT", "LastTrain=Last Train", "FitCueBid=Fit cue-bid", "DopiRopi=DOPI/ROPI",
                "SpecificKingAsks=Belirli papaz sorusu", "VoluntaryFiveMajor=Gönüllü 5 majör", "MatheAskingBids=Mathe", "ControlAskingBids=Kontrol soruları"));
            Ekle("Diğer konvansiyonlar", a => Liste(a,
                "NegativeDouble=Negatif kontr", "TakeoutDouble=Takeout kontr", "SupportDouble=Support kontr",
                "ResponsiveDouble=Responsive kontr", "Michaels=Michaels", "Unusual2NT=Unusual 2NT",
                "Sandwich1NT=Sandwich 1NT", "Ghestem=Ghestem", "WeakJumpOvercalls=Zayıf sıçramalı müdahale", "MaximalOvercallDbl=Maximal kontr",
                "InvisibleCueBids=Görünmez cue-bid", "LimitPlusCueBids=Limit-üstü cue-bid",
                "LeapingMichaels=Leaping Michaels", "LightnerDouble=Lightner kontra"));
            Ekle("Precision / güçlü kulüp", a => Liste(a, "OneClub_AnswerSistemi=1♣ cevap sistemi"));
        }

        private void Ekle(string baslik, Func<OrtaklikAnlasmasi, string> ozet)
        {
            var k = new Kutu { Baslik = baslik, Ozet = ozet };
            k.Grup = new GroupBox { Text = baslik };
            k.Metin = new Label { Dock = DockStyle.Fill, AutoSize = false, Padding = new Padding(4, 2, 4, 2) };
            k.Grup.Controls.Add(k.Metin);
            _kutular.Add(k);
        }

        // ───────────────────────── Arayüz ─────────────────────────

        private Control Yerlestir(Control c, int x, int y, int w, int h)
        {
            c.Bounds = new Rectangle(x, y, w, h);
            return c;
        }

        private void Kur()
        {
            Text = (_taraf == Taraf.NS ? "NS" : "EW") + " Ortaklık Profili";
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(820, 500);
            Font = new Font("Segoe UI", 9F);
            KutulariTanimla();

            const int SolX = 8, SolW = 260, OrtaX = 276, OrtaW = 270, SagX = 554, SagW = 258;

            // ── Sol sütun ──
            var btnYeni = new Button { Text = "Yeni" };
            _btnDuzenle = new Button { Text = "Düzenle" };
            _btnSil = new Button { Text = "Sil" };
            Yerlestir(btnYeni, SolX, 8, 82, 26);
            Yerlestir(_btnDuzenle, SolX + 89, 8, 82, 26);
            Yerlestir(_btnSil, SolX + 178, 8, 82, 26);
            btnYeni.Click += (s, e) => YeniProfil();
            _btnDuzenle.Click += (s, e) => KonvansiyonlariDuzenle();
            _btnSil.Click += (s, e) => ProfilSil();

            var lblProfil = new Label { Text = "Profil adı:", AutoSize = true };
            Yerlestir(lblProfil, SolX, 46, 120, 18);
            _cmbProfil = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            Yerlestir(_cmbProfil, SolX, 64, SolW, 24);
            _cmbProfil.SelectedIndexChanged += (s, e) => ProfilDegisti();

            var lblSistemB = new Label { Text = "İhale sistemi:", AutoSize = true };
            Yerlestir(lblSistemB, SolX, 98, 120, 18);
            _lblSistem = new Label { AutoSize = false, Font = new Font(Font, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            Yerlestir(_lblSistem, SolX, 116, SolW - 84, 22);
            var lnkSistem = new LinkLabel { Text = "değiştir", AutoSize = false, TextAlign = ContentAlignment.MiddleRight };
            Yerlestir(lnkSistem, SolX + SolW - 80, 116, 80, 22);
            lnkSistem.LinkClicked += (s, e) => SistemMenusu(lnkSistem);

            var lblBeceriB = new Label { Text = "Beceri düzeyi:", AutoSize = true };
            Yerlestir(lblBeceriB, SolX, 146, 120, 18);
            _lblBeceri = new Label { AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Font, FontStyle.Bold) };
            Yerlestir(_lblBeceri, SolX, 164, SolW, 22);
            var btnBeceri = new Button { Text = "Beceri düzeyini seçmek için tıklayın" };
            Yerlestir(btnBeceri, SolX, 190, SolW, 26);
            btnBeceri.Click += (s, e) => BeceriSec();
            var ip = new ToolTip();
            string ipucu = "20 = hatasız. Düşük değerde yapay zekâ kart oynarken arada hata yapar.";
            ip.SetToolTip(btnBeceri, ipucu);
            ip.SetToolTip(_lblBeceri, ipucu);

            _chkOgrenme = new CheckBox { Text = "Öğrenme modu", Checked = AnlasmaYonetici.OgrenmeModuAcik };
            Yerlestir(_chkOgrenme, SolX, 224, SolW, 22);
            ip.SetToolTip(_chkOgrenme, "Teklifleri motor önerisiyle karşılaştırır.");

            var gAgr = new GroupBox { Text = "Agresiflik" };
            Yerlestir(gAgr, SolX, 254, SolW, 86);
            string[] ad = { "Müdahale:", "Önleyici açılış:", "Açılış:" };
            for (int i = 0; i < 3; i++)
            {
                int k = i;
                gAgr.Controls.Add(new Label { Text = ad[i], AutoSize = true, Location = new Point(10, 20 + 21 * i) });
                _lblAgr[i] = new Label
                {
                    AutoSize = false, Location = new Point(140, 18 + 21 * i), Size = new Size(110, 20),
                    BorderStyle = BorderStyle.Fixed3D, BackColor = SystemColors.Window, Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                _lblAgr[i].Click += (s, e) => AgresiflikDegistir(k);
                ip.SetToolTip(_lblAgr[i], "Değiştirmek için tıklayın (Temkinli → Orta → Atak).");
                gAgr.Controls.Add(_lblAgr[i]);
            }

            var bir = _kutular[0].Grup;
            Yerlestir(bir, SolX, 348, SolW, 104);
            bir.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            // ── Orta sütun ──
            Yerlestir(_kutular[1].Grup, OrtaX, 8, OrtaW, 182);
            Yerlestir(_kutular[2].Grup, OrtaX, 196, OrtaW, 166);
            Yerlestir(_kutular[3].Grup, OrtaX, 368, OrtaW, 84);

            // ── Sağ sütun ──
            Yerlestir(_kutular[4].Grup, SagX, 8, SagW, 102);
            Yerlestir(_kutular[5].Grup, SagX, 116, SagW, 120);
            Yerlestir(_kutular[6].Grup, SagX, 242, SagW, 130);
            Yerlestir(_kutular[7].Grup, SagX, 378, SagW, 74);

            foreach (var k in _kutular)
            {
                k.Metin.Dock = DockStyle.Fill;
                k.Grup.Controls.Add(k.Metin);
            }

            // ── Alt satır ──
            var btnYaz = new Button { Text = "Yazdır" };
            var btnIsaret = new Button { Text = "İşaretleri göster" };
            Yerlestir(btnYaz, OrtaX, 464, 100, 28);
            Yerlestir(btnIsaret, OrtaX + 108, 464, 130, 28);
            btnYaz.Click += (s, e) => Yazdir();
            btnIsaret.Click += (s, e) => IsaretleriGoster();

            var btnIptal = new Button { Text = "İptal" };
            var btnTamam = new Button { Text = "Tamam" };
            Yerlestir(btnIptal, 820 - 8 - 180 - 8, 464, 90, 28);
            Yerlestir(btnTamam, 820 - 8 - 90, 464, 90, 28);
            btnTamam.Click += BtnTamam_Click;
            btnIptal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            AcceptButton = btnTamam;
            CancelButton = btnIptal;

            Controls.AddRange(new Control[] { btnYeni, _btnDuzenle, _btnSil, lblProfil, _cmbProfil, lblSistemB, _lblSistem, lnkSistem,
                lblBeceriB, _lblBeceri, btnBeceri, _chkOgrenme, gAgr, bir,
                _kutular[1].Grup, _kutular[2].Grup, _kutular[3].Grup,
                _kutular[4].Grup, _kutular[5].Grup, _kutular[6].Grup, _kutular[7].Grup,
                btnYaz, btnIsaret, btnIptal, btnTamam });
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
                _agr[0] = Math.Max(-1, Math.Min(1, a.MudahaleAgresifligi));
                _agr[1] = Math.Max(-1, Math.Min(1, a.PreemptAgresifligi));
                _agr[2] = Math.Max(-1, Math.Min(1, a.AcilisAgresifligi));
                for (int i = 0; i < 3; i++) _lblAgr[i].Text = AgrAdlari[_agr[i] + 1];
                BeceriEtiketi();
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

        private void BeceriEtiketi()
        {
            _lblBeceri.Text = _secili.Anlasma.BeceriDuzeyi + " / 20";
        }

        private void SistemEtiketiniGuncelle()
        {
            for (int i = 0; i < OnAyarlar.Length; i++)
            {
                if (AyniBayraklar(_secili.Anlasma, OnAyarlar[i].Uret()))
                {
                    _lblSistem.Text = OnAyarlar[i].Ad;
                    return;
                }
            }
            _lblSistem.Text = Ozel;
        }

        private void SistemMenusu(Control yer)
        {
            var m = new ContextMenuStrip();
            foreach (var o in OnAyarlar)
            {
                var oa = o;
                m.Items.Add(oa.Ad, null, (s, e) => OnAyarUygula(oa));
            }
            m.Show(yer, new Point(0, yer.Height));
        }

        private void OnAyarUygula(OnAyar secim)
        {
            var yeni = secim.Uret();
            var hedef = _secili.Anlasma;
            foreach (var n in BayrakAdlari)
            {
                var p = typeof(OrtaklikAnlasmasi).GetProperty(n);
                p.SetValue(hedef, p.GetValue(yeni, null), null);
            }
            SistemEtiketiniGuncelle();
            OzetleriGuncelle();
        }

        private void BeceriSec()
        {
            using (var f = new Form())
            {
                f.Text = "Beceri düzeyi";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowInTaskbar = false; f.MinimizeBox = false; f.MaximizeBox = false;
                f.ClientSize = new Size(340, 130);
                var l = new Label { AutoSize = false, Location = new Point(12, 10), Size = new Size(316, 20), TextAlign = ContentAlignment.MiddleCenter };
                var tb = new TrackBar { Minimum = 1, Maximum = 20, TickFrequency = 1, Location = new Point(8, 34), Width = 324, Height = 40,
                    Value = Math.Max(1, Math.Min(20, _secili.Anlasma.BeceriDuzeyi)) };
                Action g = () => l.Text = tb.Value + " / 20" + (tb.Value == 20 ? " (hatasız)" : "");
                tb.ValueChanged += (s, e) => g();
                g();
                var ok = new Button { Text = "Tamam", DialogResult = DialogResult.OK, Location = new Point(158, 90), Size = new Size(80, 28) };
                var iptal = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Location = new Point(248, 90), Size = new Size(80, 28) };
                f.Controls.AddRange(new Control[] { l, tb, ok, iptal });
                f.AcceptButton = ok; f.CancelButton = iptal;
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    _secili.Anlasma.BeceriDuzeyi = tb.Value;
                    BeceriEtiketi();
                }
            }
        }

        private void AgresiflikDegistir(int i)
        {
            _agr[i] = _agr[i] >= 1 ? -1 : _agr[i] + 1;
            _lblAgr[i].Text = AgrAdlari[_agr[i] + 1];
            var a = _secili.Anlasma;
            a.MudahaleAgresifligi = _agr[0];
            a.PreemptAgresifligi = _agr[1];
            a.AcilisAgresifligi = _agr[2];
        }

        private void IsaretleriGoster()
        {
            string s = Liste(_secili.Anlasma, "StandartSignal=Standart", "UDCA=UDCA", "OddEvenDiscards=Tek-Çift atış",
                "Lavinthal=Lavinthal", "SmithEcho=Smith echo");
            MessageBox.Show(this, s.Replace(", ", "\n"), "Seçili işaretleşme yöntemleri", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Yazdir()
        {
            var satirlar = new List<string> { Text + " — " + _secili.Ad, "İhale sistemi: " + _lblSistem.Text,
                "Beceri düzeyi: " + _secili.Anlasma.BeceriDuzeyi + " / 20",
                "Agresiflik: müdahale " + AgrAdlari[_agr[0] + 1] + ", önleyici " + AgrAdlari[_agr[1] + 1] + ", açılış " + AgrAdlari[_agr[2] + 1], "" };
            foreach (var k in _kutular) { satirlar.Add(k.Baslik + ":"); satirlar.Add("   " + k.Ozet(_secili.Anlasma)); }
            satirlar.Add("İşaretleşme: " + Liste(_secili.Anlasma, "StandartSignal=Standart", "UDCA=UDCA", "OddEvenDiscards=Tek-Çift atış", "Lavinthal=Lavinthal", "SmithEcho=Smith echo"));
            try
            {
                var pd = new PrintDocument();
                var yazi = new Font("Segoe UI", 10F);
                pd.PrintPage += (s, e) =>
                {
                    float y = e.MarginBounds.Top;
                    foreach (var t in satirlar)
                    {
                        var r = new RectangleF(e.MarginBounds.Left, y, e.MarginBounds.Width, 400);
                        var sz = e.Graphics.MeasureString(t, yazi, e.MarginBounds.Width);
                        e.Graphics.DrawString(t, yazi, Brushes.Black, new RectangleF(r.X, r.Y, r.Width, sz.Height));
                        y += sz.Height + 2;
                    }
                };
                using (var dlg = new PrintDialog { Document = pd })
                    if (dlg.ShowDialog(this) == DialogResult.OK) pd.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Yazdırılamadı: " + ex.Message, "Yazdır", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
                    SistemEtiketiniGuncelle();
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

        private void YeniProfil()
        {
            bool kopya;
            string ad = AdSor("Yeni profilin adı:", _secili.Ad + " (kopya)", out kopya);
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

        private string AdSor(string soru, string varsayilan, out bool kopya)
        {
            using (var f = new Form())
            {
                f.Text = "Yeni profil";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowInTaskbar = false;
                f.MinimizeBox = false; f.MaximizeBox = false;
                f.ClientSize = new Size(320, 138);
                var l = new Label { Text = soru, AutoSize = true, Location = new Point(12, 12) };
                var t = new TextBox { Text = varsayilan, Location = new Point(12, 34), Width = 296 };
                var c = new CheckBox { Text = "Mevcut profilin kopyası olsun", Checked = true, AutoSize = true, Location = new Point(12, 64) };
                var ok = new Button { Text = "Tamam", DialogResult = DialogResult.OK, Location = new Point(138, 98), Size = new Size(80, 28) };
                var iptal = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Location = new Point(228, 98), Size = new Size(80, 28) };
                c.CheckedChanged += (s, e) => { if (c.Checked && t.Text == "Yeni profil") t.Text = _secili.Ad + " (kopya)"; else if (!c.Checked && t.Text == _secili.Ad + " (kopya)") t.Text = "Yeni profil"; };
                f.Controls.AddRange(new Control[] { l, t, c, ok, iptal });
                f.AcceptButton = ok; f.CancelButton = iptal;
                t.SelectAll();
                bool tamam = f.ShowDialog(this) == DialogResult.OK;
                kopya = c.Checked;
                return tamam ? t.Text : null;
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
