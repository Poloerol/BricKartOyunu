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

        internal sealed class OnAyar
        {
            public string Ad; public Func<OrtaklikAnlasmasi> Uret;
            public OnAyar(string ad, Func<OrtaklikAnlasmasi> uret) { Ad = ad; Uret = uret; }
        }

        internal static readonly OnAyar[] OnAyarlar =
        {
            new OnAyar("Standart (5'li majör, güçlü NT)", () => OrtaklikAnlasmasi.Varsayilan()),
            new OnAyar("Başlangıç (basit)", () => OrtaklikAnlasmasi.Basit()),
            new OnAyar("Zayıf NT (12–14)", () => { var a = OrtaklikAnlasmasi.Varsayilan(); a.StrongNT = false; return a; })
        };

        // (özellik adı, görünen ad) — özet kutularında seçili olanlar listelenir
        private static readonly string[] AgrAdlari = { "Çok temkinli", "Temkinli", "Orta", "Atak", "Çok atak" };

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

        public AnlasmaForm(Taraf taraf) : this(taraf, null, null, null) { }

        internal AnlasmaForm(Taraf taraf, List<AnlasmaProfil> devir, string seciliAd, bool? ogrenme)
        {
            _taraf = taraf;
            _profiller = devir ?? AnlasmaYonetici.Profiller(taraf);
            string aktif = seciliAd ?? AnlasmaYonetici.AktifProfilAdi(taraf);
            _secili = _profiller.FirstOrDefault(p => p.Ad == aktif) ?? _profiller[0];
            Kur();
            if (ogrenme.HasValue) _chkOgrenme.Checked = ogrenme.Value;
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
            Text = (_taraf == Taraf.NS ? "N/S" : "E/W") + " Ortaklık Profili";
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
            var lnkSistem = new LinkLabel { Text = "değiştir", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
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
                gAgr.Controls.Add(new Label { Text = ad[i], AutoSize = true, Location = new Point(10, 20 + 21 * i) });
                // Yalnız gösterim: değiştirme profil sihirbazından (Düzenle) yapılır
                _lblAgr[i] = new Label
                {
                    AutoSize = false, Location = new Point(110, 20 + 21 * i), Size = new Size(130, 18),
                    TextAlign = ContentAlignment.MiddleLeft
                };
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
            var btnSinyal = new Button { Text = "Sinyalleri göster" };
            Yerlestir(btnYaz, OrtaX, 464, 100, 28);
            Yerlestir(btnSinyal, OrtaX + 108, 464, 130, 28);
            btnYaz.Click += (s, e) => Yazdir();
            btnSinyal.Click += (s, e) => SinyalleriGoster();

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
                btnYaz, btnSinyal, btnIptal, btnTamam });
            BaslikBicimi.Uygula(this, true);
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
                _agr[0] = Math.Max(-2, Math.Min(2, a.MudahaleAgresifligi));
                _agr[1] = Math.Max(-2, Math.Min(2, a.PreemptAgresifligi));
                _agr[2] = Math.Max(-2, Math.Min(2, a.AcilisAgresifligi));
                for (int i = 0; i < 3; i++) _lblAgr[i].Text = AgrAdlari[_agr[i] + 2];
                BeceriEtiketi();
                SistemEtiketiniGuncelle();
            }
            finally { _yukleniyor = false; }
            OzetleriGuncelle();
        }

        internal static readonly string[] AyarAdlari = typeof(OrtaklikAnlasmasi).GetProperties()
            .Where(p => (p.PropertyType == typeof(bool) || p.PropertyType == typeof(int)) && p.CanWrite).Select(p => p.Name).ToArray();

        internal static readonly string[] BayrakAdlari = typeof(OrtaklikAnlasmasi).GetProperties()
            .Where(p => p.PropertyType == typeof(bool) && p.CanWrite).Select(p => p.Name).ToArray();

        internal static bool AyniBayraklar(OrtaklikAnlasmasi x, OrtaklikAnlasmasi y)
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

        private void SinyalleriGoster()
        {
            using (var f = new SinyallerForm(_secili.Anlasma)) f.ShowDialog(this);
        }

        private void Yazdir()
        {
            var satirlar = new List<string> { Text + " — " + _secili.Ad, "İhale sistemi: " + _lblSistem.Text,
                "Beceri düzeyi: " + _secili.Anlasma.BeceriDuzeyi + " / 20",
                "Agresiflik: müdahale " + AgrAdlari[_agr[0] + 2] + ", önleyici " + AgrAdlari[_agr[1] + 2] + ", açılış " + AgrAdlari[_agr[2] + 2], "" };
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
            // Bu form kapanır; Goster() ortaklık profili sihirbazını açar (bkz. Goster).
            DuzenleIstendi = true;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        internal bool DuzenleIstendi;
        internal List<AnlasmaProfil> Profiller { get { return _profiller; } }
        internal AnlasmaProfil Secili { get { return _secili; } }
        internal bool OgrenmeIsareti { get { return _chkOgrenme.Checked; } }

        /// <summary>
        /// Ortaklık profili ekranını gösterir. Düzenle'ye basılırsa bu form kapanır ve Ortaklık Profili sihirbazı açılır;
        /// sihirbaz Son ile kapanınca uyarı sorulur. Evet: değişiklikler uygulanır ve profil ekranı yeniden açılır
        /// (ekrandaki kaydedilmemiş düzenlemeler korunur). Hayır: değişiklikler atılır, profil ekranı açılmaz.
        /// Sihirbazda İptal: profil ekranı yeniden açılır.
        /// </summary>
        public static void Goster(IWin32Window sahip, Taraf taraf)
        {
            List<AnlasmaProfil> devir = null; string seciliAd = null; bool? ogrenme = null;
            while (true)
            {
                AnlasmaProfil secili;
                using (var f = new AnlasmaForm(taraf, devir, seciliAd, ogrenme))
                {
                    f.ShowDialog(sahip);
                    if (!f.DuzenleIstendi) return;
                    devir = f.Profiller; secili = f.Secili; seciliAd = secili.Ad; ogrenme = f.OgrenmeIsareti;
                }
                using (var w = new ProfilSihirbazi(secili.Anlasma, secili.Ad))
                {
                    if (w.ShowDialog(sahip) != DialogResult.OK || w.Sonuc == null) continue;   // İptal: profil ekranı geri gelir
                    var c = MessageBox.Show(sahip, "Yaptığınız değişiklikler ortaklık profiline uygulansın mı?", "Uyarı",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (c != DialogResult.Yes) return;                                           // Hayır
                    foreach (var n in AyarAdlari)
                    {
                        var p = typeof(OrtaklikAnlasmasi).GetProperty(n);
                        p.SetValue(secili.Anlasma, p.GetValue(w.Sonuc, null), null);
                    }
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

    /// <summary>
    /// Sinyaller ekranı (Bridge Baron "Signals"): açılış kartı kuralları ve genel işaretleşme.
    /// Tamam'a basılınca seçimler anlaşmaya yazılır; İptal hiçbir şeyi değiştirmez.
    /// UDCA, Tek-Çift, Lavinthal ve Smith echo seçimleri kart oyunu yapay zekâsının kullandığı bayraklara da yansır;
    /// diğer seçenekler profilde saklanır.
    /// </summary>
    public sealed class SinyallerForm : Form
    {
        private static readonly string[][] RenkKartlari =
        {
            new[] { "[x]x", " x x[x]", " x x x[x]", " x x x[x]x", " T x[x]", "[T]9 x", " H x[x]", " H x x[x]", "[K]Q x +", "[A]K x +", " A[K]", "[Q]J x +" },
            new[] { "[J]T 9", " K[Q]T 9", " K[J]T x", " K[T]9 x", " Q[T]9 x", " A[Q]J", " A[J]T", " A[T]9", "[J]T x", "[K]Q T x", "[Q]J 9 x", "[J]T 8 x" },
            new[] { "[K]Q", "[Q]J", "[J]T", "[T]9", " 9 x[x]", "[9]8 x" }
        };
        private static readonly string[][] NtKartlari =
        {
            new[] { "[x]x", "[x]x x", "[x]x x x", " x x x[x]x", " T x[x]", "[T]9 x", " H x[x]", " H x x[x]", "A[K]x +", "A[K]", "A[K]J x", "A[J]T x" },
            new[] { "[K]Q J x", "[Q]J T x", "[J]T 9 x", " A[Q]J x", " A[T]9 x", " K[Q]T 9", " Q[T]9 x", "[A]K J T", "[A]K Q T", " K[J]T x", " K[T]9 x", "[K]Q x" },
            new[] { "[Q]J x", "[J]T x", "[K]Q T x", "[Q]J 9 x", "[J]T 8 x", "[9]x x", "[9]8 x" }
        };

        // Anlaşmadaki tam sayı alanları (radyo: seçenek sırası; onay kutusu: 0/1)
        private static readonly string[] Alanlar =
        {
            "SigAnySignal", "SigFirstDiscardRelevant", "SigPrimary", "SigSmithPartner", "SigSmithLeader", "SigCountHighLow",
            "SigCountOpp", "SigFirstDiscard", "SigAttitudeHigh", "SigTrump", "LeadSuitStyle", "LeadNTStyle",
            "LeadSuitRusinow", "LeadSuitJournalist", "LeadSuitZeroTwo", "LeadSuitMud",
            "LeadNTJournalist", "LeadNTZeroTwo", "LeadNTMud"
        };

        private readonly OrtaklikAnlasmasi _a;
        private readonly Dictionary<string, int> _v = new Dictionary<string, int>();

        public SinyallerForm(OrtaklikAnlasmasi a)
        {
            _a = a;
            foreach (var n in Alanlar) _v[n] = (int)typeof(OrtaklikAnlasmasi).GetProperty(n).GetValue(a, null);
            // Atış/çağrı bayrakları asıl kaynaktır
            _v["SigAttitudeHigh"] = a.UDCA ? 1 : 0;
            if (a.Lavinthal) _v["SigFirstDiscard"] = 3;
            else if (a.OddEvenDiscards) _v["SigFirstDiscard"] = 2;
            else if (_v["SigFirstDiscard"] >= 2 && _v["SigFirstDiscard"] != 4) _v["SigFirstDiscard"] = 0;
            if (a.SmithEcho && _v["SigSmithPartner"] == 0) _v["SigSmithPartner"] = 1;
            if (!a.SmithEcho) _v["SigSmithPartner"] = 0;

            Text = "Sinyaller";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false; MinimizeBox = false; MaximizeBox = false;
            ClientSize = new Size(480, 450);
            Font = new Font("Segoe UI", 9F);

            var tab = new TabControl { Location = new Point(6, 6), Size = new Size(468, 394) };
            tab.TabPages.Add(KartSayfasi("Renge karşı açılış", RenkKartlari, false));
            tab.TabPages.Add(KartSayfasi("NT'ye karşı açılış", NtKartlari, true));
            tab.TabPages.Add(GenelSayfa());
            if (Environment.GetEnvironmentVariable("SINTAB") == "2") tab.SelectedIndex = 2;

            var tamam = new Button { Text = "Tamam", Location = new Point(306, 410), Size = new Size(80, 28) };
            var iptal = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Location = new Point(392, 410), Size = new Size(80, 28) };
            tamam.Click += (s, e) => { Uygula(); DialogResult = DialogResult.OK; Close(); };
            // Yardım: şimdilik boş; içerik daha sonra eklenecek (YardimTiklandi içine)
            var yardim = new Button { Text = "Yardım", Location = new Point(220, 410), Size = new Size(80, 28) };
            yardim.Click += (s, e) => YardimTiklandi();
            Controls.AddRange(new Control[] { tab, yardim, tamam, iptal });
            AcceptButton = tamam; CancelButton = iptal;
            BaslikBicimi.Uygula(this, false);   // kart etiketlerine dokunulmaz
        }

        private void YardimTiklandi()
        {
            // TODO: Sinyaller yardım içeriği
        }

        private void Uygula()
        {
            foreach (var n in Alanlar) typeof(OrtaklikAnlasmasi).GetProperty(n).SetValue(_a, _v[n], null);
            _a.UDCA = _v["SigAttitudeHigh"] == 1;
            _a.StandartSignal = !_a.UDCA;
            _a.Lavinthal = _v["SigFirstDiscard"] == 3;
            _a.OddEvenDiscards = _v["SigFirstDiscard"] == 2;
            _a.SmithEcho = _v["SigSmithPartner"] != 0;
        }

        private CheckBox Onay(string alan, string t, int x, int y)
        {
            var c = new CheckBox { Text = t, AutoSize = true, Location = new Point(x, y), Checked = _v[alan] != 0 };
            c.Click += (s, e) => _v[alan] = c.Checked ? 1 : 0;
            return c;
        }

        private RadioButton Radyo(string alan, int deger, string t, int x, int y)
        {
            var r = new RadioButton { Text = t, AutoSize = true, Location = new Point(x, y), Checked = _v[alan] == deger };
            r.Click += (s, e) => _v[alan] = deger;
            return r;
        }

        private TabPage KartSayfasi(string baslik, string[][] sutunlar, bool nt)
        {
            var p = new TabPage(baslik) { BackColor = SystemColors.Control };
            var f = new Font("Arial", 11F, FontStyle.Bold);
            for (int s = 0; s < sutunlar.Length; s++)
                for (int i = 0; i < sutunlar[s].Length; i++)
                    p.Controls.Add(new Label { Text = sutunlar[s][i], Font = f, AutoSize = false, Location = new Point(10 + 150 * s, 10 + 24 * i), Size = new Size(140, 22) });

            string pre = nt ? "LeadNT" : "LeadSuit";
            var ozel = nt ? new[] { Tuple.Create("LeadNTJournalist", "Gazeteci (Journalist)"), Tuple.Create("LeadNTZeroTwo", "Sıfır ya da iki üst"), Tuple.Create("LeadNTMud", "MUD") }
                          : new[] { Tuple.Create("LeadSuitRusinow", "Rusinow"), Tuple.Create("LeadSuitJournalist", "Gazeteci (Journalist)"), Tuple.Create("LeadSuitZeroTwo", "Sıfır ya da iki üst"), Tuple.Create("LeadSuitMud", "MUD") };
            int y = nt ? 196 : 190;
            foreach (var o in ozel) { p.Controls.Add(Onay(o.Item1, o.Item2, 316, y)); y += 24; }
            var g = new GroupBox { Text = nt ? "NT'ye karşı" : "Renge karşı", Location = new Point(312, nt ? 272 : 290), Size = new Size(140, nt ? 84 : 66) };
            g.Controls.Add(Radyo(pre + "Style", 0, "üçüncü/beşinci", 10, 16));
            g.Controls.Add(Radyo(pre + "Style", 1, "dördüncü", 10, 36));
            if (nt) g.Controls.Add(Radyo(pre + "Style", 2, "en düşük", 10, 56));
            p.Controls.Add(g);
            return p;
        }

        private GroupBox Grup(string baslik, Rectangle r, string alan, params string[] secenekler)
        {
            var g = new GroupBox { Text = baslik, Bounds = r };
            for (int i = 0; i < secenekler.Length; i++) g.Controls.Add(Radyo(alan, i, secenekler[i], 8, 20 + 20 * i));
            return g;
        }

        private TabPage GenelSayfa()
        {
            var p = new TabPage("Genel sinyaller") { BackColor = SystemColors.Control };
            p.Controls.Add(Grup("Her türlü sinyal", new Rectangle(4, 6, 104, 80), "SigAnySignal", "Evet", "Hayır"));
            p.Controls.Add(Grup("İlk atma anlamlı", new Rectangle(112, 6, 124, 80), "SigFirstDiscardRelevant", "Evet", "Hayır"));
            p.Controls.Add(Grup("Birincil sinyal", new Rectangle(240, 6, 104, 100), "SigPrimary", "Sayı", "Tutum", "Tek/Çift"));
            p.Controls.Add(Grup("Smith (ortak)", new Rectangle(348, 6, 104, 100), "SigSmithPartner", "Yok", "Yüksek-alçak", "Alçak-yüksek"));
            p.Controls.Add(Grup("Sayı sinyali", new Rectangle(4, 120, 104, 80), "SigCountHighLow", "Çift", "Tek"));
            p.Controls.Add(Grup("Rakibe sayı", new Rectangle(112, 120, 124, 80), "SigCountOpp", "Evet", "Hayır"));
            p.Controls.Add(Grup("İlk atma şekli", new Rectangle(240, 120, 104, 140), "SigFirstDiscard", "Tutum", "Sayı", "Tek/Çift", "Lavinthal", "Dönüşümlü"));
            p.Controls.Add(Grup("Smith (açıcı)", new Rectangle(348, 120, 104, 100), "SigSmithLeader", "Yok", "Yüksek-alçak", "Alçak-yüksek"));
            p.Controls.Add(Grup("Tutum (yüksek)", new Rectangle(112, 234, 124, 80), "SigAttitudeHigh", "Teşvik eder", "Caydırır"));
            p.Controls.Add(Grup("Koz sinyali", new Rectangle(348, 234, 104, 100), "SigTrump", "Vinje", "Yok", "3+ yüks-alç"));
            return p;
        }
    }

    /// <summary>
    /// Ortaklık profili sihirbazı (Bridge Baron "Pair Profile" sayfaları): 8 sayfa, Geri / Sonraki / Son / İptal / Yardım.
    /// Çalışma kopyası üzerinde düzenler; Son ile Sonuc'a yazılır. Motorda karşılığı olmayan seçenekler soluk (pasif) gösterilir.
    /// </summary>
    public sealed class ProfilSihirbazi : Form
    {
        private static readonly string[] Basliklar =
        {
            "Ortaklık Profili", "1NT Açılışları", "Majör Açılışlar", "Minör Açılışlar",
            "Yüksek Seviye Açılışlar", "Yüksek NT Açılışları", "Slam Teklifleri", "Rekabetli İhale"
        };

        // Aynı anda yalnızca biri açık olabilen alanlar (KonvansiyonSecimForm ile aynı kurallar)
        private static readonly string[][] Dislayan =
        {
            new[] { "Cappelletti", "Dont", "MultiLandy", "HelloOver1NT" },
            new[] { "Serious3NT", "NonSerious3NT" },
            new[] { "Drury", "ReverseDrury" },
            new[] { "ZayifIki", "Multi2D" },
            new[] { "IkiliSinekGuclu", "BenjaminTwoBids" },
            new[] { "BergenRaises", "InvertedBergenRaises" },
            new[] { "TrialBid", "LongShortGameTries" },
            new[] { "TwoWayCheckback", "NewMinorForcing", "WolffSignoff" },
            new[] { "Robinson", "ExitTransfer" },
            new[] { "Ghestem", "Michaels" },
            new[] { "Ghestem", "Unusual2NT" },
            new[] { "MinorTransfer", "MinorStayman" },
            new[] { "Flannery", "ZayifIki" },
            new[] { "Flannery", "Multi2D" },
            new[] { "Flannery", "MiniRoman2D" },
            new[] { "Lebensohl", "Rubensohl" }
        };

        private static readonly Dictionary<string, string> Gerektirir = new Dictionary<string, string>
        {
            { "Smolen", "Stayman" },
            { "RKCB1430", "RKCB" },
            { "SpecificKingAsks", "RKCB" },
            { "Kokish", "IkiliSinekGuclu" },
            { "IkiBirOyunZorlama", "BesliMajor" },
            { "ExtendedStayman", "Stayman" }
        };

        private const int SayfaY = 420;

        private readonly OrtaklikAnlasmasi _a;
        private readonly string _ad;
        private readonly List<Panel> _sayfalar = new List<Panel>();
        private readonly List<Action> _yenile = new List<Action>();
        private Button _geri, _ileri;
        private int _i;
        private bool _sync;

        /// <summary>Son ile onaylanan düzenlenmiş anlaşma; İptal'de null.</summary>
        public OrtaklikAnlasmasi Sonuc { get; private set; }

        public ProfilSihirbazi(OrtaklikAnlasmasi anlasma, string profilAdi, int baslangicSayfa = 0)
        {
            _a = anlasma.Kopya();
            _ad = profilAdi;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false; MinimizeBox = false; MaximizeBox = false;
            ClientSize = new Size(440, 470);
            Font = new Font("Segoe UI", 9F);

            _sayfalar.Add(Sayfa1()); _sayfalar.Add(Sayfa2()); _sayfalar.Add(Sayfa3()); _sayfalar.Add(Sayfa4());
            _sayfalar.Add(Sayfa5()); _sayfalar.Add(Sayfa6()); _sayfalar.Add(Sayfa7()); _sayfalar.Add(Sayfa8());
            foreach (var p in _sayfalar) { p.Visible = false; Controls.Add(p); }

            var cizgi = new Label { BorderStyle = BorderStyle.Fixed3D, Bounds = new Rectangle(12, SayfaY + 2, 416, 2) };
            _geri = Dugme("< Geri", 12); _ileri = Dugme("Sonraki >", 95);
            var son = Dugme("Son", 180); var iptal = Dugme("İptal", 266); var yardim = Dugme("Yardım", 351);
            _geri.Click += (s, e) => Git(_i - 1);
            _ileri.Click += (s, e) => Git(_i + 1);
            son.Click += (s, e) => { Sonuc = _a; DialogResult = DialogResult.OK; Close(); };
            iptal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            yardim.Click += (s, e) => { /* TODO: yardım içeriği */ };
            Controls.AddRange(new Control[] { cizgi, _geri, _ileri, son, iptal, yardim });
            CancelButton = iptal;
            foreach (var pg in _sayfalar) BaslikBicimi.Uygula(pg, true);
            Hepsi();
            Git(Math.Max(0, Math.Min(_sayfalar.Count - 1, baslangicSayfa)));
        }

        private Button Dugme(string t, int x) { return new Button { Text = t, Bounds = new Rectangle(x, SayfaY + 12, 78, 28) }; }

        private void Git(int i)
        {
            if (i < 0 || i >= _sayfalar.Count) return;
            _sayfalar[_i].Visible = false;
            _i = i;
            _sayfalar[_i].Visible = true;
            Text = BaslikBicimi.Bas(Basliklar[_i]);
            _geri.Enabled = _i > 0;
            _ileri.Enabled = _i < _sayfalar.Count - 1;
        }

        // ───────────── Veri erişimi ─────────────

        private static PropertyInfo Pr(string ad) { return typeof(OrtaklikAnlasmasi).GetProperty(ad); }
        private bool Al(string ad) { return (bool)Pr(ad).GetValue(_a, null); }

        private void Koy(string ad, bool v)
        {
            Pr(ad).SetValue(_a, v, null);
            if (v)
            {
                foreach (var g in Dislayan)
                {
                    if (!g.Contains(ad)) continue;
                    foreach (var d in g) if (d != ad) Pr(d).SetValue(_a, false, null);
                }
                string gerek;
                if (Gerektirir.TryGetValue(ad, out gerek)) Pr(gerek).SetValue(_a, true, null);
            }
            else
            {
                foreach (var kv in Gerektirir) if (kv.Value == ad) Pr(kv.Key).SetValue(_a, false, null);
            }
        }

        private void Hepsi()
        {
            _sync = true;
            try { foreach (var y in _yenile) y(); }
            finally { _sync = false; }
        }

        // ───────────── Denetim üreticileri ─────────────

        private static Panel YeniSayfa()
        {
            return new Panel { Bounds = new Rectangle(0, 0, 440, SayfaY) };
        }

        private static Label Et(Control ust, string t, int x, int y, int w = 0)
        {
            var l = new Label { Text = t, Location = new Point(x, y) };
            if (w > 0) { l.AutoSize = false; l.Size = new Size(w, 20); } else l.AutoSize = true;
            ust.Controls.Add(l);
            return l;
        }

        private static Panel Gr(Control ust, int x, int y, int w, int h)
        {
            var p = new Panel { Bounds = new Rectangle(x, y, w, h) };
            ust.Controls.Add(p);
            return p;
        }

        private CheckBox Cb(Control ust, string alan, string t, int x, int y, int w = 200)
        {
            var c = new CheckBox { Text = t, AutoSize = false, Bounds = new Rectangle(x, y, w, 20) };
            c.Click += (s, e) => { Koy(alan, c.Checked); Hepsi(); };
            _yenile.Add(() => c.Checked = Al(alan));
            ust.Controls.Add(c);
            return c;
        }

        private static CheckBox CbPasif(Control ust, string t, int x, int y, int w = 200)
        {
            var c = new CheckBox { Text = t, AutoSize = false, Bounds = new Rectangle(x, y, w, 20), Enabled = false };
            ust.Controls.Add(c);
            return c;
        }

        /// <summary>al == null: motorda karşılığı yok, pasif.</summary>
        private RadioButton Rb(Control ust, string t, int x, int y, int w, Func<bool> al, Action koy = null, bool pasifIsaretli = false)
        {
            var r = new RadioButton { Text = t, AutoSize = false, Bounds = new Rectangle(x, y, w, 20) };
            if (al == null) { r.Enabled = false; r.Checked = pasifIsaretli; }
            else
            {
                r.Click += (s, e) => { koy(); Hepsi(); };
                _yenile.Add(() => r.Checked = al());
            }
            ust.Controls.Add(r);
            return r;
        }

        private ComboBox Agr(Control ust, int x, int y, int w, Func<int> al, Action<int> koy)
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(x, y, w, 24) };
            c.Items.AddRange(new object[] { "Çok temkinli", "Temkinli", "Orta", "Atak", "Çok atak" });
            c.SelectedIndexChanged += (s, e) => { if (!_sync && c.SelectedIndex >= 0) koy(c.SelectedIndex - 2); };
            _yenile.Add(() => c.SelectedIndex = Math.Max(0, Math.Min(4, al() + 2)));
            ust.Controls.Add(c);
            return c;
        }

        // ───────────── Sayfa 1: Ortaklık profili ─────────────

        private Panel Sayfa1()
        {
            var p = YeniSayfa();
            Et(p, "Profil adı:", 60, 46, 84).TextAlign = ContentAlignment.MiddleRight;
            p.Controls.Add(new TextBox { Text = _ad, Enabled = false, Bounds = new Rectangle(151, 42, 170, 24) });

            Et(p, "İhale sistemi:", 40, 80, 104).TextAlign = ContentAlignment.MiddleRight;
            var sis = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(151, 76, 260, 24) };
            foreach (var o in AnlasmaForm.OnAyarlar) sis.Items.Add(o.Ad);
            sis.Items.Add("Özel");
            sis.SelectionChangeCommitted += (s, e) =>
            {
                int i = sis.SelectedIndex;
                if (i < 0 || i >= AnlasmaForm.OnAyarlar.Length) { Hepsi(); return; }
                var yeni = AnlasmaForm.OnAyarlar[i].Uret();
                foreach (var n in AnlasmaForm.BayrakAdlari) Pr(n).SetValue(_a, Pr(n).GetValue(yeni, null), null);
                Hepsi();
            };
            _yenile.Add(() =>
            {
                int k = AnlasmaForm.OnAyarlar.Length;
                for (int i = 0; i < AnlasmaForm.OnAyarlar.Length; i++)
                    if (AnlasmaForm.AyniBayraklar(_a, AnlasmaForm.OnAyarlar[i].Uret())) { k = i; break; }
                sis.SelectedIndex = k;
            });
            p.Controls.Add(sis);

            Et(p, "İki seviye açılışlar:", 28, 126);
            var g = Gr(p, 152, 120, 270, 84);
            Rb(g, "Yalnız güçlü 2♣", 0, 0, 250, () => !Al("BenjaminTwoBids") && !Al("ZayifIki") && !Al("Multi2D"),
                () => { Koy("BenjaminTwoBids", false); Koy("ZayifIki", false); Koy("Multi2D", false); Koy("IkiliSinekGuclu", true); });
            Rb(g, "Zayıf 2'ler (güçlü 2♣ ile)", 0, 26, 250, () => !Al("BenjaminTwoBids") && (Al("ZayifIki") || Al("Multi2D")),
                () => { Koy("BenjaminTwoBids", false); Koy("IkiliSinekGuclu", true); Koy("ZayifIki", true); });
            Rb(g, "Benjamin", 0, 52, 250, () => Al("BenjaminTwoBids"), () => Koy("BenjaminTwoBids", true));

            var gb = new GroupBox { Text = "İhale agresifliği", Bounds = new Rectangle(44, 224, 350, 116) };
            Et(gb, "Müdahale:", 20, 28, 80).TextAlign = ContentAlignment.MiddleRight;
            Et(gb, "Önleyici açılış:", 4, 58, 96).TextAlign = ContentAlignment.MiddleRight;
            Et(gb, "Açılış:", 20, 88, 80).TextAlign = ContentAlignment.MiddleRight;
            Agr(gb, 108, 24, 220, () => _a.MudahaleAgresifligi, v => _a.MudahaleAgresifligi = v);
            Agr(gb, 108, 54, 220, () => _a.PreemptAgresifligi, v => _a.PreemptAgresifligi = v);
            Agr(gb, 108, 84, 220, () => _a.AcilisAgresifligi, v => _a.AcilisAgresifligi = v);
            p.Controls.Add(gb);

            var sy = new Button { Text = "Sinyalleri göster", Bounds = new Rectangle(123, 358, 173, 28) };
            sy.Click += (s, e) => { using (var f = new SinyallerForm(_a)) f.ShowDialog(this); };
            p.Controls.Add(sy);
            return p;
        }

        // ───────────── Sayfa 2: 1NT ─────────────

        private Panel Sayfa2()
        {
            var p = YeniSayfa();
            Et(p, "1NT açılış aralığı:", 22, 12);
            string[] ar = { "10-12", "11-13", "12-14", "13-15", "14-16", "15-17", "16-18" };
            int[] sx = { 0, 0, 1, 1, 2, 2, 3 }, sy = { 0, 1, 0, 1, 0, 1, 0 };
            for (int k = 0; k < ar.Length; k++)
            {
                int x = 150 + 68 * sx[k], y = 8 + 22 * sy[k];
                if (ar[k] == "12-14") Rb(p, ar[k], x, y, 66, () => !_a.StrongNT, () => _a.StrongNT = false);
                else if (ar[k] == "15-17") Rb(p, ar[k], x, y, 66, () => _a.StrongNT, () => _a.StrongNT = true);
                else Rb(p, ar[k], x, y, 66, null);
            }
            CbPasif(p, "5-4-2-2 el ile 1NT", 56, 54, 150);
            CbPasif(p, "Tekil A/K ile 1NT", 214, 54, 170);
            CbPasif(p, "5'li majör ile 1NT", 56, 74, 150);
            CbPasif(p, "6'lı minör ile 1NT", 214, 74, 170);

            Et(p, "1NT açılışlarına cevaplar:", 22, 100);
            Et(p, "2♣:", 40, 124);
            var g = Gr(p, 72, 120, 190, 70);
            Rb(g, "Doğal", 0, 0, 90, () => !Al("Stayman"), () => { Koy("Stayman", false); Koy("Smolen", false); Koy("ExtendedStayman", false); });
            Rb(g, "Stayman", 90, 0, 98, () => Al("Stayman") && !Al("ExtendedStayman"), () => { Koy("Stayman", true); Koy("ExtendedStayman", false); });
            Rb(g, "Extended Stayman", 0, 24, 188, () => Al("ExtendedStayman"), () => Koy("ExtendedStayman", true));
            Rb(g, "Puppet Stayman", 0, 48, 188, null);
            Et(p, "Transfer teklifleri:", 276, 102);
            Cb(p, "JacobyTransfer", "Jacoby transferleri", 266, 122, 160);
            Cb(p, "TexasTransfers", "Texas transferleri", 266, 144, 160);
            Cb(p, "Smolen", "Smolen", 266, 166, 160);

            Et(p, "Konvansiyonel minör teklifleri:", 22, 194);
            var m = Gr(p, 40, 214, 390, 70);
            Rb(m, "Yok", 0, 0, 60, () => !Al("MinorStayman") && !Al("MinorTransfer"), () => { Koy("MinorStayman", false); Koy("MinorTransfer", false); });
            Rb(m, "Minör Stayman", 70, 0, 120, () => Al("MinorStayman"), () => Koy("MinorStayman", true));
            Rb(m, "1NT-2♠ → 3♣ röle", 200, 0, 180, null);
            Rb(m, "Minör transferleri (süper kabullü, 2NT→♦)", 0, 24, 380, null);
            Rb(m, "Minör transferleri (süper kabulsüz, 2NT→♦)", 0, 48, 380, () => Al("MinorTransfer"), () => Koy("MinorTransfer", true));

            Cb(p, "Lebensohl", "Lebensohl", 40, 290, 150);
            Cb(p, "Rubensohl", "Rubensohl", 200, 290, 150);

            Et(p, "1NT'ye karşı savunma:", 22, 318);
            var d = Gr(p, 150, 314, 280, 46);
            Func<bool> dogal = () => !Al("Cappelletti") && !Al("Dont") && !Al("MultiLandy") && !Al("HelloOver1NT");
            Rb(d, "Doğal", 0, 0, 64, dogal, () => { Koy("Cappelletti", false); Koy("Dont", false); Koy("MultiLandy", false); Koy("HelloOver1NT", false); });
            Rb(d, "D.O.N.T.", 66, 0, 74, () => Al("Dont"), () => Koy("Dont", true));
            Rb(d, "Brozel", 142, 0, 66, null);
            Rb(d, "Hello", 210, 0, 66, () => Al("HelloOver1NT"), () => Koy("HelloOver1NT", true));
            Rb(d, "Cappelletti", 0, 22, 90, () => Al("Cappelletti"), () => Koy("Cappelletti", true));
            Rb(d, "Astro", 92, 22, 56, null);
            Rb(d, "Landy", 150, 22, 62, () => Al("MultiLandy"), () => Koy("MultiLandy", true));

            Et(p, "1NT'ye penaltı kontra\nsonrası kaçış:", 22, 368);
            var k2 = Gr(p, 150, 364, 280, 46);
            Rb(k2, "Doğal", 0, 0, 64, () => !Al("Robinson") && !Al("ExitTransfer"), () => { Koy("Robinson", false); Koy("ExitTransfer", false); });
            Rb(k2, "Robinson", 66, 0, 80, () => Al("Robinson"), () => Koy("Robinson", true));
            Rb(k2, "Exit transfer", 150, 0, 120, () => Al("ExitTransfer"), () => Koy("ExitTransfer", true));
            Rb(k2, "D.O.N.T. (kaçış)", 0, 22, 130, null);
            Rb(k2, "Helvic", 150, 22, 80, null);
            return p;
        }

        // ───────────── Sayfa 3: Majör ─────────────

        private Panel Sayfa3()
        {
            var p = YeniSayfa();
            Et(p, "Majör açılışlara cevaplar:", 22, 18);
            var g = Gr(p, 40, 40, 390, 52);
            Rb(g, "Standart raise'ler", 0, 0, 190, () => !Al("BergenRaises") && !Al("InvertedBergenRaises"), () => { Koy("BergenRaises", false); Koy("InvertedBergenRaises", false); });
            Rb(g, "Bergen raise'ler", 0, 26, 190, () => Al("BergenRaises"), () => Koy("BergenRaises", true));
            Rb(g, "Limit majör raise'ler", 190, 0, 190, null);
            Rb(g, "Ters Bergen raise'ler", 190, 26, 190, () => Al("InvertedBergenRaises"), () => Koy("InvertedBergenRaises", true));

            Cb(p, "Jacoby2NT", "Jacoby 2NT", 40, 112, 170);
            Cb(p, "Truscott2NT", "Kontra sonrası Truscott 2NT", 210, 112, 220);
            Cb(p, "Splinter", "Splinter'lar", 40, 136, 170);
            Cb(p, "PreemptiveReraises", "Önleyici reraise'ler", 210, 136, 220);
            Cb(p, "Artificial2C2D", "Kontra sonrası yapay 2♣/2♦ raise'ler", 40, 160, 380);

            Et(p, "Oyun denemeleri:", 22, 196);
            var t = Gr(p, 40, 218, 390, 52);
            Rb(t, "Yok", 0, 0, 190, () => !Al("TrialBid") && !Al("LongShortGameTries"), () => { Koy("TrialBid", false); Koy("LongShortGameTries", false); });
            Rb(t, "Uzun renk", 0, 26, 190, () => Al("TrialBid"), () => Koy("TrialBid", true));
            Rb(t, "Kısa renk", 190, 0, 190, null);
            Rb(t, "Hem uzun hem kısa renk", 190, 26, 190, () => Al("LongShortGameTries"), () => Koy("LongShortGameTries", true));

            Et(p, "1NT:", 22, 292);
            var n = Gr(p, 40, 314, 390, 26);
            Rb(n, "Doğal", 0, 0, 100, () => !Al("ForcingNT"), () => Koy("ForcingNT", false));
            Rb(n, "Zorlayıcı", 110, 0, 110, () => Al("ForcingNT"), () => Koy("ForcingNT", true));
            Rb(n, "Yarı zorlayıcı", 230, 0, 140, null);

            Et(p, "Üçüncü el açılışlarına konvansiyonel cevaplar:", 22, 360);
            var d = Gr(p, 40, 384, 390, 26);
            Rb(d, "Yok", 0, 0, 100, () => !Al("Drury") && !Al("ReverseDrury"), () => { Koy("Drury", false); Koy("ReverseDrury", false); });
            Rb(d, "Drury", 110, 0, 110, () => Al("Drury"), () => Koy("Drury", true));
            Rb(d, "Ters Drury", 230, 0, 140, () => Al("ReverseDrury"), () => Koy("ReverseDrury", true));
            return p;
        }

        // ───────────── Sayfa 4: Minör ─────────────

        private Panel Sayfa4()
        {
            var p = YeniSayfa();
            Et(p, "Minör açılışlar:", 22, 12);
            Rb(p, "En iyi minör", 40, 32, 190, () => true, () => { }, true).Enabled = false;
            Rb(p, "Kısa sinek", 240, 32, 170, null);

            Et(p, "Minör açılışlara cevaplar:", 22, 62);
            var g = Gr(p, 40, 82, 190, 48);
            Rb(g, "Standart", 0, 0, 180, () => !Al("InvertedMinors"), () => Koy("InvertedMinors", false));
            Rb(g, "Ters minör raise'ler", 0, 24, 180, () => Al("InvertedMinors"), () => Koy("InvertedMinors", true));
            Cb(p, "Walsh", "Walsh", 240, 82, 170);
            Cb(p, "OneClub_AnswerSistemi", "1♣ cevap sistemi", 240, 106, 170);

            Et(p, "2NT'ye sıçrama:", 22, 136);
            var j = Gr(p, 40, 156, 190, 48);
            Rb(j, "Zorlayıcı", 0, 0, 180, () => !Al("OneMinorTwoNTInv"), () => Koy("OneMinorTwoNTInv", false));
            Rb(j, "Davet", 0, 24, 180, () => Al("OneMinorTwoNTInv"), () => Koy("OneMinorTwoNTInv", true));
            Cb(p, "XYZ", "XYZ", 240, 156, 170);
            Cb(p, "ThirdSuitForcing", "3. renk zorlaması", 240, 180, 170);

            Et(p, "Özel checkback teklifleri:", 22, 210);
            var c = Gr(p, 40, 230, 190, 76);
            Rb(c, "Yok", 0, 0, 180, () => !Al("TwoWayCheckback") && !Al("NewMinorForcing"), () => { Koy("TwoWayCheckback", false); Koy("NewMinorForcing", false); });
            Rb(c, "Yeni minör zorlaması", 0, 24, 180, () => Al("NewMinorForcing"), () => Koy("NewMinorForcing", true));
            Rb(c, "Crowhurst", 0, 48, 180, null);
            Cb(p, "WolffSignoff", "Wolff bırakma", 240, 230, 170);
            var c2 = Gr(p, 240, 254, 180, 26);
            Rb(c2, "İki yönlü checkback", 0, 0, 180, () => Al("TwoWayCheckback"), () => Koy("TwoWayCheckback", true));
            Cb(p, "German2CRelays", "Alman 2♣ röleleri", 240, 284, 170);

            Et(p, "Sıçrama cevapları:", 22, 312);
            var sj = Gr(p, 40, 334, 400, 24);
            Rb(sj, "Güçlü sıçrama-shift", 0, 0, 190, () => !Al("WeakJumpShiftResp"), () => Koy("WeakJumpShiftResp", false));
            Rb(sj, "Zayıf sıçrama-shift", 200, 0, 190, () => Al("WeakJumpShiftResp"), () => Koy("WeakJumpShiftResp", true));

            Et(p, "Dördüncü renk teklifleri:", 22, 364);
            var f = Gr(p, 40, 386, 400, 24);
            Rb(f, "Doğal", 0, 0, 100, () => !Al("FourthSuitForcing"), () => Koy("FourthSuitForcing", false));
            Rb(f, "Oyuna kadar zorlayıcı", 100, 0, 170, () => Al("FourthSuitForcing"), () => Koy("FourthSuitForcing", true));
            Rb(f, "Bir tur zorlayıcı", 270, 0, 130, null);
            return p;
        }

        // ───────────── Sayfa 5: Yüksek seviye ─────────────

        private Panel Sayfa5()
        {
            var p = YeniSayfa();
            Et(p, "Konvansiyonel iki seviye açılışlar:", 22, 18);
            Cb(p, "Multi2D", "Multi 2♦", 40, 42, 120);
            Cb(p, "Flannery", "Flannery 2♦", 164, 42, 120);
            Cb(p, "MiniRoman2D", "Mini-Roman 2♦", 290, 42, 130);

            Et(p, "Güçlü 2♣ açılışına cevap:", 22, 88);
            Cb(p, "German2CRelays", "Alman 2♣ röleleri", 40, 112, 190);
            Cb(p, "Kokish", "Kokish (2♣–2♦–2♥)", 240, 112, 180);

            Et(p, "Zayıf iki açılışlarına konvansiyonel cevaplar:", 22, 158);
            Cb(p, "Ogust", "Ogust", 40, 182, 190);

            Et(p, "Zayıf iki açılışlarına karşı savunma:", 22, 228);
            CbPasif(p, "Zayıf ikilere karşı Lebensohl", 40, 252, 260);

            Et(p, "Dört seviye minör açılışlar:", 22, 298);
            Cb(p, "Namyats", "Namyats", 40, 322, 190);

            Et(p, "Önleyici açılışlar:", 22, 368);
            Cb(p, "Preempt", "3. ve 4. seviye önleyici açılışlar", 40, 392, 300);
            return p;
        }

        // ───────────── Sayfa 6: Yüksek NT ─────────────

        private Panel Sayfa6()
        {
            var p = YeniSayfa();
            Et(p, "2NT açılışı:", 22, 18);
            string[] r1 = { "19-20", "20-21", "21-22", "22-23", "23-24" };
            string[] r2 = { "19-21", "20-22", "21-23", "22-24" };
            for (int k = 0; k < r1.Length; k++)
            {
                if (r1[k] == "20-21") Rb(p, r1[k], 24 + 80 * k, 42, 76, () => true, () => { });
                else Rb(p, r1[k], 24 + 80 * k, 42, 76, null);
            }
            for (int k = 0; k < r2.Length; k++) Rb(p, r2[k], 24 + 80 * k, 66, 76, null);

            Et(p, "2NT açılışına cevaplar:", 22, 112);
            Et(p, "3♣:", 40, 138);
            var g = Gr(p, 80, 134, 300, 78);
            Rb(g, "Doğal", 0, 0, 200, null);
            Rb(g, "Stayman", 0, 26, 200, () => !Al("PuppetStayman"), () => Koy("PuppetStayman", false));
            Rb(g, "Puppet Stayman", 0, 52, 200, () => Al("PuppetStayman"), () => Koy("PuppetStayman", true));

            Et(p, "3NT açılışı:", 22, 236);
            var t = Gr(p, 60, 260, 340, 52);
            Rb(t, "25+ HP dengeli", 0, 0, 300, () => !Al("Gambling3NT"), () => Koy("Gambling3NT", false));
            Rb(t, "Gambling 3NT", 0, 26, 300, () => Al("Gambling3NT"), () => Koy("Gambling3NT", true));
            return p;
        }

        // ───────────── Sayfa 7: Slam ─────────────

        private Panel Sayfa7()
        {
            var p = YeniSayfa();
            Et(p, "As soran teklifler:", 22, 18);
            var g = Gr(p, 40, 40, 390, 52);
            Rb(g, "Blackwood", 0, 0, 190, () => !Al("RKCB") && !Al("RKCB1430"), () => { Koy("RKCB1430", false); Koy("RKCB", false); Koy("Blackwood", true); });
            Rb(g, "RKC 1430", 0, 26, 190, () => Al("RKCB1430"), () => { Koy("RKCB", true); Koy("RKCB1430", true); Koy("Blackwood", true); });
            Rb(g, "Roman Keycard Blackwood", 190, 0, 200, () => Al("RKCB") && !Al("RKCB1430"), () => { Koy("RKCB", true); Koy("RKCB1430", false); Koy("Blackwood", true); });

            Et(p, "Blackwood sonrası papaz soran 5NT:", 22, 112);
            var k = Gr(p, 40, 134, 390, 26);
            Rb(k, "Standart", 0, 0, 190, () => !Al("SpecificKingAsks"), () => Koy("SpecificKingAsks", false));
            Rb(k, "Belirli papazlar", 190, 0, 190, () => Al("SpecificKingAsks"), () => Koy("SpecificKingAsks", true));

            Et(p, "Diğer slam konvansiyonları:", 22, 184);
            Cb(p, "DopiRopi", "DOPI / ROPI", 40, 208, 170);
            CbPasif(p, "Büyük slam zorlaması", 220, 208, 200);
            Cb(p, "VoluntaryFiveMajor", "Gönüllü 5 majör", 40, 232, 170);
            Cb(p, "CueBid", "Kontrol gösteren cue-bid'ler", 220, 232, 210);
            Cb(p, "MatheAskingBids", "Mathe soru teklifleri", 40, 256, 170);
            Cb(p, "ControlAskingBids", "Kontrol soran teklifler", 220, 256, 210);
            Cb(p, "Gerber", "Gerber", 40, 280, 170);
            Cb(p, "FitCueBid", "Fit cue-bid (müdahalede)", 220, 280, 210);
            Cb(p, "Serious3NT", "Ciddi 3NT", 40, 304, 170);
            Cb(p, "NonSerious3NT", "Ciddi olmayan 3NT", 220, 304, 210);
            Cb(p, "LastTrain", "Last Train", 40, 328, 170);
            Cb(p, "LightnerDouble", "Lightner kontra", 220, 328, 210);
            return p;
        }

        // ───────────── Sayfa 8: Rekabet ─────────────

        private Panel Sayfa8()
        {
            var p = YeniSayfa();
            Et(p, "İki renkli teklifler:", 22, 14);
            Cb(p, "Unusual2NT", "Unusual 2NT", 40, 36, 150);
            Cb(p, "Michaels", "Michaels", 40, 58, 150);

            Et(p, "Kapsamlı iki renkli teklifler:", 222, 14);
            var gh = Gr(p, 232, 34, 200, 70);
            Rb(gh, "Kapalı", 0, 0, 90, () => !Al("Ghestem"), () => Koy("Ghestem", false));
            Rb(gh, "Ghestem", 0, 24, 120, () => Al("Ghestem"), () => Koy("Ghestem", true));
            Rb(gh, "La Majeure 2 renkli", 90, 0, 110, null).Visible = false;

            Et(p, "Gecikmeli iki renkli teklifler:", 22, 96);
            CbPasif(p, "Forum D", 40, 118, 150);

            Et(p, "Rakip renk cue-bid'leri:", 222, 108);
            var cu = Gr(p, 232, 128, 200, 100);
            Rb(cu, "Güç gösteren", 0, 0, 190, () => !Al("LimitPlusCueBids"), () => Koy("LimitPlusCueBids", false));
            Rb(cu, "Batı cue-bid'leri", 0, 22, 190, null);
            Rb(cu, "Doğu cue-bid'leri", 0, 44, 190, null);
            Rb(cu, "Ortağın majörüne limit raise ya da üstü", 0, 66, 190, () => Al("LimitPlusCueBids"), () => Koy("LimitPlusCueBids", true)).Height = 34;

            Et(p, "Negatif kontra sınırı:", 22, 146);
            var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(40, 170, 60, 24), Enabled = false };
            cmb.Items.Add("2♠"); cmb.SelectedIndex = 0;
            p.Controls.Add(cmb);

            Et(p, "İki renkli tekliflere karşı savunma:", 22, 214);
            var sv = Gr(p, 40, 236, 190, 70);
            Rb(sv, "Doğal", 0, 0, 180, () => !Al("InvisibleCueBids"), () => Koy("InvisibleCueBids", false));
            Rb(sv, "Unusual over Unusual", 0, 22, 180, null);
            Rb(sv, "Görünmez cue-bid'ler", 0, 44, 180, () => Al("InvisibleCueBids"), () => Koy("InvisibleCueBids", true));

            Et(p, "Diğer rekabet teklifleri:", 234, 234);
            Cb(p, "WeakJumpOvercalls", "Zayıf sıçramalı müdahale", 232, 256, 200);
            Cb(p, "Sandwich1NT", "Sandwich 1NT", 232, 278, 200);
            Cb(p, "LeapingMichaels", "Leaping Michaels", 232, 300, 200);

            Et(p, "Özel kontralar:", 22, 332);
            Cb(p, "NegativeDouble", "Negatif kontra", 40, 356, 170);
            Cb(p, "ResponsiveDouble", "Responsive kontra", 40, 378, 170);
            CbPasif(p, "Atak yönlendiren kontra", 40, 400, 180);
            Cb(p, "SupportDouble", "Support kontra", 232, 356, 200);
            Cb(p, "MaximalOvercallDbl", "Maximal müdahale kontrası", 232, 378, 200);
            Cb(p, "TakeoutDouble", "Takeout kontra", 232, 400, 200);
            return p;
        }
    }

    /// <summary>Başlık biçimi: her sözcüğün ilk harfi büyük (Türkçe kurallarıyla; ör. "Profil Adı:").</summary>
    internal static class BaslikBicimi
    {
        private static readonly System.Globalization.CultureInfo Tr = new System.Globalization.CultureInfo("tr-TR");

        public static string Bas(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var ch = s.ToCharArray();
            bool bas = true;
            for (int i = 0; i < ch.Length; i++)
            {
                char c = ch[i];
                if (char.IsWhiteSpace(c) || c == '(' || c == '/') { bas = true; continue; }
                if (bas) ch[i] = char.ToUpper(c, Tr);
                bas = false;
            }
            return new string(ch);
        }

        /// <summary>Düğme, grup başlığı, sekme ve (isteğe bağlı) etiket yazılarına uygular; onay/radyo seçeneklerine dokunmaz.</summary>
        public static void Uygula(Control kok, bool etiketler)
        {
            foreach (Control c in kok.Controls)
            {
                if (c is Button || c is GroupBox) c.Text = Bas(c.Text);
                else if (c is Label && etiketler && c.Cursor != Cursors.Hand) c.Text = Bas(c.Text);
                var tc = c as TabControl;
                if (tc != null) foreach (TabPage tp in tc.TabPages) tp.Text = Bas(tp.Text);
                Uygula(c, etiketler);
            }
        }
    }
}
