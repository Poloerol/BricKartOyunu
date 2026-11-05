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
    /// sistem ön ayarı, Zorluk Seviyesi, agresiflik ve seçili konvansiyonların kategori özetleri.
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
            new OnAyar("Standart (5'li Majör, Güçlü NT)", () => OrtaklikAnlasmasi.Varsayilan()),
            new OnAyar("2/1 Game Force - Temel", () => OrtaklikAnlasmasi.IkiBirTemel()),
            new OnAyar("2/1 Game Force - Orta", () => OrtaklikAnlasmasi.IkiBirOrta()),
            new OnAyar("2/1 Game Force - Gelişmiş", () => OrtaklikAnlasmasi.IkiBirGelismis()),
            new OnAyar("Sasson (SAS - Kısa Sinek)", () => OrtaklikAnlasmasi.Sasson()),
            new OnAyar("Başlangıç (Basit)", () => OrtaklikAnlasmasi.Basit()),
            new OnAyar("Zayıf NT (12–14)", () => { var a = OrtaklikAnlasmasi.Varsayilan(); a.StrongNT = false; return a; })
        };

        // (özellik adı, görünen ad) — özet kutularında seçili olanlar listelenir
        private static readonly string[] AgrAdlari = { "Çok Temkinli", "Temkinli", "Orta", "Atak", "Çok Atak" };

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
        private Button _btnSil, _btnDuzenle, _btnBeceri;
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
            Ekle("1NT Açılışı", a =>
                (a.StrongNT ? "15–17 HP" : "12–14 HP") + ", " + Liste(a,
                    "Stayman=Stayman", "JacobyTransfer=Jacoby Transfer", "Smolen=Smolen", "MinorTransfer=Minör Transferi",
                    "Lebensohl=Lebensohl", "Cappelletti=Cappelletti", "Dont=DONT", "MultiLandy=Multi-Landy",
                    "ExtendedStayman=Extended Stayman", "Robinson=Robinson", "ExitTransfer=Exit transfer", "HelloOver1NT=Hello",
                    "Quantitative4NT=Nicel 4NT", "TexasTransfers=Texas", "MinorStayman=Minör Stayman", "Rubensohl=Rubensohl",
                    "NT1OvercallSystemsOn=Systems After 1NT Overcall", "NTDefenseBalancing=NT Defence in Balancing Seat"));
            Ekle("Minör Açılış ve Gelişimler", a => Liste(a,
                "InvertedMinors=İnverted Minörler", "Reverse=Reverse",
                "XYZ=XYZ", "TwoWayCheckback=İki Yönlü Checkback", "ThirdSuitForcing=3. Renk Forsingi", "FourthSuitForcing=4. Renk Forsingi",
                "OneMinorTwoNTInv=1m–2NT Davet", "Walsh=Walsh", "NewMinorForcing=Yeni Minör Forsingi", "WolffSignoff=Wolff bırakma", "Kokish=Kokish"));
            Ekle("Majör Açılış ve Gelişimler", a =>
                (a.BesliMajor ? "5'li Majör" : "4'lü Majör") + ", " + Liste(a,
                    "BergenRaises=Bergen", "Jacoby2NT=Jacoby 2NT", "Splinter=Splinter", "TrialBid=Deneme Teklifi",
                    "Drury=Drury", "ReverseDrury=Ters Drury", "ForcingNT=Forsing 1NT",
                    "InvertedBergenRaises=Ters Bergen", "LongShortGameTries=Uzun&Kısa Deneme", "Truscott2NT=Truscott 2NT",
                    "Artificial2C2D=Yapay 2♣/2♦", "PreemptiveReraises=Önleyici reraise", "WeakJumpShiftResp=Zayıf Sıçrama Cevabı"));
            Ekle("2NT / 3NT", a => "2NT: " + (a.NT2Range < 5 ? (19 + Math.Max(0, a.NT2Range)) + "–" + (20 + Math.Max(0, a.NT2Range)) : (14 + Math.Min(8, a.NT2Range)) + "–" + (16 + Math.Min(8, a.NT2Range))) + " HP, " + Liste(a, "PuppetStayman=Puppet Stayman"));
            Ekle("Yüksek Seviye Açılışlar", a => Liste(a,
                "IkiliSinekGuclu=2♣ Güçlü", "ZayifIki=Zayıf İki", "Multi2D=Multi 2♦", "Preempt=Blokatif Açılışlar", "Namyats=Namyats",
                "BenjaminTwoBids=Benjamin", "Gambling3NT=Gambling Three Notrump (Uzun Sağlam Minör Açılışı)", "Flannery=Flannery 2♦", "MiniRoman2D=Mini-Roman 2♦", "Ogust=Ogust", "FeatureAsking2NT=Feature asking 2NT", "German2CRelays=Alman 2♣ Röleleri"));
            Ekle("Şlem Konvansiyonları", a => Liste(a,
                "Blackwood=Blackwood", "RKCB=RKCB", "RKCB1430=RKCB 1430", "Gerber=Gerber", "CueBid=Cue bid",
                "Serious3NT=Ciddi 3NT", "NonSerious3NT=Ciddi Olmayan 3NT", "LastTrain=Last Train", "FitCueBid=Fit Cue-bid", "DopiRopi=DOPI/ROPI",
                "SpecificKingAsks=Belirli Papaz Sorusu", "VoluntaryFiveMajor=Gönüllü Beşli Majör Çıkışı", "MatheAskingBids=Mathe", "ControlAskingBids=Kontrol Soruları"));
            Ekle("Diğer Konvansiyonlar", a => Liste(a,
                "NegativeDouble=Negatif DBL", "TakeoutDouble=Takeout DBL", "SupportDouble=Support DBL",
                "ResponsiveDouble=Responsive DBL", "Michaels=Michaels", "Unusual2NT=Unusual 2NT",
                "Sandwich1NT=Sandwich 1NT", "Ghestem=Ghestem", "WeakJumpOvercalls=Zayıf Sıçramalı Müdahale", "MaximalOvercallDbl=Maximal DBL",
                "InvisibleCueBids=Görünmez Cue-bid", "LimitPlusCueBids=Limit-üstü Cue-bid",
                "GoodBad2NT=Good-Bad 2NT", "InvertedDbl=Inverted DBL", "MixedRaise=Mixed Raise", "FSJS=FSJS",
                "LeapingMichaels=Leaping Michaels", "LightnerDouble=Lightner DBL"));
            Ekle("Precision / Güçlü Kulüp", a => Liste(a, "OneClub_AnswerSistemi=1♣ Cevap Sistemi"));
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
            c.Bounds = new Rectangle(x * 800 / 820, y * 470 / 500, w * 800 / 820, h * 470 / 500);
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
            ClientSize = new Size(800, 470);
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

            var lblProfil = new Label { Text = "Profil Adı:", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            Yerlestir(lblProfil, SolX, 46, 64, 24);
            _cmbProfil = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            Yerlestir(_cmbProfil, SolX + 66, 46, SolW - 66, 24);
            _cmbProfil.SelectedIndexChanged += (s, e) => ProfilDegisti();

            var lblSistemB = new Label { Text = "İhale Sistemi:", AutoSize = true };
            Yerlestir(lblSistemB, SolX, 84, 120, 18);
            _lblSistem = new Label { AutoSize = false, Font = new Font(Font, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            Yerlestir(_lblSistem, SolX, 102, SolW, 22);

            var lblBeceriB = new Label { Text = "Zorluk Seviyesi:", AutoSize = true };
            Yerlestir(lblBeceriB, SolX, 132, 120, 18);
            _lblBeceri = new Label { AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Font, FontStyle.Bold) };
            Yerlestir(_lblBeceri, SolX, 150, SolW, 22);
            _btnBeceri = new Button { Text = "Zorluk Seviyesini Seçmek İçin Tıklayın" };
            var btnBeceri = _btnBeceri;
            Yerlestir(btnBeceri, SolX, 176, SolW, 26);
            btnBeceri.Click += (s, e) => BeceriSec();
            var ip = new ToolTip();
            string ipucu = "20 = hatasız. Düşük değerde yapay zekâ kart oynarken arada hata yapar.";
            ip.SetToolTip(btnBeceri, ipucu);
            ip.SetToolTip(_lblBeceri, ipucu);

            _chkOgrenme = new CheckBox { Text = "Öğrenme Modu", Checked = AnlasmaYonetici.OgrenmeModuAcik };
            Yerlestir(_chkOgrenme, SolX, 210, SolW, 22);
            ip.SetToolTip(_chkOgrenme, "Teklifleri motor önerisiyle karşılaştırır.");

            var gAgr = new GroupBox { Text = "Agresiflik" };
            Yerlestir(gAgr, SolX, 240, SolW, 90);
            string[] ad = { "Araya Giriş:", "Blokatif Açılış:", "Açılış:" };
            for (int i = 0; i < 3; i++)
            {
                gAgr.Controls.Add(new Label { Text = ad[i], AutoSize = true, Location = new Point(10, 17 + 19 * i) });
                // Yalnız gösterim: değiştirme profil sihirbazından (Düzenle) yapılır
                _lblAgr[i] = new Label
                {
                    AutoSize = false, Location = new Point(110, 17 + 19 * i), Size = new Size(130, 18),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                gAgr.Controls.Add(_lblAgr[i]);
            }

            var bir = _kutular[0].Grup;
            Yerlestir(bir, SolX, 338, SolW, 114);
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
            var btnSinyal = new Button { Text = "Sinyalleri Göster" };
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

            Controls.AddRange(new Control[] { btnYeni, _btnDuzenle, _btnSil, lblProfil, _cmbProfil, lblSistemB, _lblSistem,
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
            ButonlariGuncelle();
        }

        /// <summary>Hazır (sabit) profil seçiliyken Düzenle ve Sil kapalıdır.</summary>
        private void ButonlariGuncelle()
        {
            _btnDuzenle.Enabled = !_secili.Sabit;
            _btnSil.Enabled = !_secili.Sabit;
            if (_btnBeceri != null) _btnBeceri.Enabled = !_secili.Sabit;
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
                ButonlariGuncelle();
            }
            finally { _yukleniyor = false; }
            OzetleriGuncelle();
        }

        internal static readonly string[] AyarAdlari = typeof(OrtaklikAnlasmasi).GetProperties()
            .Where(p => (p.PropertyType == typeof(bool) || p.PropertyType == typeof(int)) && p.CanWrite).Select(p => p.Name).ToArray();

        internal static readonly string[] BayrakAdlari = typeof(OrtaklikAnlasmasi).GetProperties()
            .Where(p => p.PropertyType == typeof(bool) && p.CanWrite).Select(p => p.Name).ToArray();

        /// <summary>Ön ayarın seçimlerini (bayraklar + 1NT/2NT, limit raise vb. tam sayı seçenekleri) kopyalar; agresiflik, beceri, süre ve sinyal/atak ayarlarına dokunmaz.</summary>
        internal static void OnAyarKopyala(OrtaklikAnlasmasi hedef, OrtaklikAnlasmasi yeni)
        {
            foreach (var n in BayrakAdlari)
            {
                var p = typeof(OrtaklikAnlasmasi).GetProperty(n);
                p.SetValue(hedef, p.GetValue(yeni, null), null);
            }
            foreach (var pi in typeof(OrtaklikAnlasmasi).GetProperties())
            {
                if (pi.PropertyType != typeof(int) || !pi.CanWrite) continue;
                string n = pi.Name;
                if (n.EndsWith("Agresifligi") || n == "BeceriDuzeyi" || n.StartsWith("Sure") || n.StartsWith("Lead")) continue;
                pi.SetValue(hedef, pi.GetValue(yeni, null), null);
            }
        }

        /// <summary>Anlaşmanın seçimleri bir ön ayara uyuyorsa onun adı, uymuyorsa "Özel".</summary>
        internal static string SistemAdiBul(OrtaklikAnlasmasi a)
        {
            for (int i = 0; i < OnAyarlar.Length; i++)
                if (AyniBayraklar(a, OnAyarlar[i].Uret())) return OnAyarlar[i].Ad;
            return Ozel;
        }

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

        /// <summary>"İhale Sistemi:" satırı: seçili profilde kayıtlı sistem adı.</summary>
        private void SistemEtiketiniGuncelle()
        {
            if (string.IsNullOrEmpty(_secili.Sistem)) _secili.SistemAdiniBelirle();
            _lblSistem.Text = _secili.Sistem;
        }

        private void BeceriSec()
        {
            using (var f = new SeviyeAyarlariForm(_secili.Anlasma))
                if (f.ShowDialog(this) == DialogResult.OK) _lblBeceri.Text = _secili.Anlasma.BeceriDuzeyi + " / 20";
        }

        private void SinyalleriGoster()
        {
            using (var f = new SinyallerForm(_secili.Anlasma)) f.ShowDialog(this);
        }

        private void Yazdir()
        {
            var satirlar = new List<string> { Text + " — " + _secili.Ad, "İhale sistemi: " + _lblSistem.Text,
                "Zorluk Seviyesi: " + _secili.Anlasma.BeceriDuzeyi + " / 20",
                "Agresiflik: müdahale " + AgrAdlari[_agr[0] + 2] + ", Blokatif " + AgrAdlari[_agr[1] + 2] + ", Açılış " + AgrAdlari[_agr[2] + 2], "" };
            foreach (var k in _kutular) { satirlar.Add(k.Baslik + ":"); satirlar.Add("   " + k.Ozet(_secili.Anlasma)); }
            satirlar.Add("Sinyalleşme: " + Liste(_secili.Anlasma, "StandartSignal=Standart", "UDCA=UDCA", "OddEvenDiscards=Tek-Çift Atış", "Lavinthal=Lavinthal", "SmithEcho=Smith echo"));
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
            if (_secili.Sabit) return;
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
                    secili.SistemAdiniBelirle();
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
            string ad = AdSor("Yeni Profilin Adı:", _secili.Ad + " (kopya)", out kopya);
            if (string.IsNullOrWhiteSpace(ad)) return;
            ad = BenzersizAd(ad.Trim());
            var yeni = kopya ? _secili.Kopya(ad)
                             : new AnlasmaProfil { Ad = ad, Anlasma = OrtaklikAnlasmasi.Varsayilan(), Sistem = OnAyarlar[0].Ad };
            _profiller.Add(yeni);
            _secili = yeni;
            ProfilListesiniDoldur();
            SeciliyiGoster();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AnlasmaForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "AnlasmaForm";
            this.Load += new System.EventHandler(this.AnlasmaForm_Load);
            this.ResumeLayout(false);

        }

        private void AnlasmaForm_Load(object sender, EventArgs e)
        {

        }

        private void ProfilSil()
        {
            if (_secili.Sabit || _profiller.Count <= 1) return;
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
                f.Text = "Yeni Profil";
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
            var p = new TabPage("Genel Sinyaller") { BackColor = SystemColors.Control };
            p.Controls.Add(Grup("Her Türlü Sinyal", new Rectangle(4, 6, 104, 80), "SigAnySignal", "Evet", "Hayır"));
            p.Controls.Add(Grup("İlk Atma Anlamlı", new Rectangle(112, 6, 124, 80), "SigFirstDiscardRelevant", "Evet", "Hayır"));
            p.Controls.Add(Grup("Birincil Sinyal", new Rectangle(240, 6, 104, 100), "SigPrimary", "Sayı", "Tutum", "Tek/Çift"));
            p.Controls.Add(Grup("Smith (Ortak)", new Rectangle(348, 6, 104, 100), "SigSmithPartner", "Yok", "Yüksek-Alçak", "Alçak-Yüksek"));
            p.Controls.Add(Grup("Sayı Sinyali", new Rectangle(4, 120, 104, 80), "SigCountHighLow", "Çift", "Tek"));
            p.Controls.Add(Grup("Rakibe Sayı", new Rectangle(112, 120, 124, 80), "SigCountOpp", "Evet", "Hayır"));
            p.Controls.Add(Grup("İlk Atma Şekli", new Rectangle(240, 120, 104, 140), "SigFirstDiscard", "Tutum", "Sayı", "Tek/Çift", "Lavinthal", "Dönüşümlü"));
            p.Controls.Add(Grup("Smith (Açıcı)", new Rectangle(348, 120, 104, 100), "SigSmithLeader", "Yok", "Yüksek-Alçak", "Alçak-Yüksek"));
            p.Controls.Add(Grup("Tutum (Yüksek)", new Rectangle(112, 234, 124, 80), "SigAttitudeHigh", "Teşvik eder", "Caydırır"));
            p.Controls.Add(Grup("Koz Sinyali", new Rectangle(348, 234, 104, 100), "SigTrump", "Vinje", "Yok", "3+ Yüks-Alç"));
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
            "Yüksek Seviye Açılışlar", "Yüksek NT Açılışları", "Slam Teklifleri", "Rekabetli İhale",
            "SAS Cevap Sistemleri"
        };

        // Aynı anda yalnızca biri açık olabilen alanlar (KonvansiyonSecimForm ile aynı kurallar)
        internal static readonly string[][] Dislayan =
        {
            new[] { "Cappelletti", "Dont", "MultiLandy", "HelloOver1NT" },
            new[] { "Serious3NT", "NonSerious3NT" },
            new[] { "Ogust", "FeatureAsking2NT" },
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

        internal static readonly Dictionary<string, string> Gerektirir = new Dictionary<string, string>
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
            _sayfalar.Add(Sayfa5()); _sayfalar.Add(Sayfa6()); _sayfalar.Add(Sayfa7()); _sayfalar.Add(Sayfa8()); _sayfalar.Add(Sayfa9());
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

        private int Gi(string ad) { return (int)Pr(ad).GetValue(_a, null); }
        private void Si(string ad, int v) { Pr(ad).SetValue(_a, v, null); }

        /// <summary>Tam sayı (0/1) alanına bağlı onay kutusu: motorda henüz karşılığı olmayan seçenekler profilde saklanır.</summary>
        private CheckBox CbI(Control ust, string alan, string t, int x, int y, int w = 200)
        {
            var c = new CheckBox { Text = t, AutoSize = false, Bounds = new Rectangle(x, y, w, 20) };
            c.Click += (s, e) => { Si(alan, c.Checked ? 1 : 0); Hepsi(); };
            _yenile.Add(() => c.Checked = Gi(alan) != 0);
            ust.Controls.Add(c);
            return c;
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
            c.Items.AddRange(new object[] { "Çok Temkinli", "Temkinli", "Orta", "Atak", "Çok atak" });
            c.SelectedIndexChanged += (s, e) => { if (!_sync && c.SelectedIndex >= 0) koy(c.SelectedIndex - 2); };
            _yenile.Add(() => c.SelectedIndex = Math.Max(0, Math.Min(4, al() + 2)));
            ust.Controls.Add(c);
            return c;
        }

        // ───────────── Sayfa 1: Ortaklık profili ─────────────

        private Panel Sayfa1()
        {
            var p = YeniSayfa();
            Et(p, "Profil Adı:", 60, 46, 84).TextAlign = ContentAlignment.MiddleRight;
            p.Controls.Add(new TextBox { Text = _ad, Enabled = false, Bounds = new Rectangle(151, 42, 170, 24) });

            Et(p, "İhale Sistemi:", 40, 80, 104).TextAlign = ContentAlignment.MiddleRight;
            var sis = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(151, 76, 260, 24) };
            foreach (var o in AnlasmaForm.OnAyarlar) sis.Items.Add(o.Ad);
            sis.Items.Add("Özel");
            sis.SelectionChangeCommitted += (s, e) =>
            {
                int i = sis.SelectedIndex;
                if (i < 0 || i >= AnlasmaForm.OnAyarlar.Length) { Hepsi(); return; }
                var yeni = AnlasmaForm.OnAyarlar[i].Uret();
                AnlasmaForm.OnAyarKopyala(_a, yeni);
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

            Et(p, "İki Seviye Açılışlar:", 28, 126);
            var g = Gr(p, 152, 120, 270, 84);
            Rb(g, "Yalnız Güçlü 2♣", 0, 0, 250, () => !Al("BenjaminTwoBids") && !Al("ZayifIki") && !Al("Multi2D"),
                () => { Koy("BenjaminTwoBids", false); Koy("ZayifIki", false); Koy("Multi2D", false); Koy("IkiliSinekGuclu", true); });
            Rb(g, "Zayıf 2'ler (Güçlü 2♣ ile)", 0, 26, 250, () => !Al("BenjaminTwoBids") && (Al("ZayifIki") || Al("Multi2D")),
                () => { Koy("BenjaminTwoBids", false); Koy("IkiliSinekGuclu", true); Koy("ZayifIki", true); });
            Rb(g, "Benjamin", 0, 52, 250, () => Al("BenjaminTwoBids"), () => Koy("BenjaminTwoBids", true));

            var gb = new GroupBox { Text = "İhale Agresifliği", Bounds = new Rectangle(44, 224, 350, 116) };
            Et(gb, "Müdahale:", 20, 28, 80).TextAlign = ContentAlignment.MiddleRight;
            Et(gb, "Blokatif Açılış:", 4, 58, 96).TextAlign = ContentAlignment.MiddleRight;
            Et(gb, "Açılış:", 20, 88, 80).TextAlign = ContentAlignment.MiddleRight;
            Agr(gb, 108, 24, 220, () => _a.MudahaleAgresifligi, v => _a.MudahaleAgresifligi = v);
            Agr(gb, 108, 54, 220, () => _a.PreemptAgresifligi, v => _a.PreemptAgresifligi = v);
            Agr(gb, 108, 84, 220, () => _a.AcilisAgresifligi, v => _a.AcilisAgresifligi = v);
            p.Controls.Add(gb);

            var sy = new Button { Text = "Sinyalleri Göster", Bounds = new Rectangle(123, 358, 173, 28) };
            sy.Click += (s, e) => { using (var f = new SinyallerForm(_a)) f.ShowDialog(this); };
            p.Controls.Add(sy);
            return p;
        }

        // ───────────── Sayfa 2: 1NT ─────────────

        private int NT1Aralik()
        {
            // StrongNT bayrağı ile uyumlu etkin aralık dizini (10-12, 11-13, 12-14, 13-15, 14-16, 15-17, 16-18)
            int r = Gi("NT1Range");
            if (_a.StrongNT) return r >= 4 ? r : 5;
            return r <= 3 ? r : 2;
        }

        private Panel Sayfa2()
        {
            var p = YeniSayfa();
            Et(p, "1NT Açılış Aralığı:", 22, 12);
            string[] ar = { "10-12", "11-13", "12-14", "13-15", "14-16", "15-17", "16-18" };
            int[] sx = { 0, 0, 1, 1, 2, 2, 3 }, sy = { 0, 1, 0, 1, 0, 1, 0 };
            for (int k = 0; k < ar.Length; k++)
            {
                int kk = k;
                Rb(p, ar[k], 150 + 68 * sx[k], 8 + 22 * sy[k], 66, () => NT1Aralik() == kk, () => { Si("NT1Range", kk); _a.StrongNT = kk >= 4; });
            }
            CbI(p, "NT1Allow5422", "5-4-2-2 El ile 1NT", 56, 54, 150);
            CbI(p, "NT1AllowSingletonHonor", "Tekil A/K ile 1NT", 214, 54, 170);
            CbI(p, "NT1Allow5Major", "5'li Majör ile 1NT", 56, 74, 150);
            CbI(p, "NT1Allow6Minor", "6'lı Minör ile 1NT", 214, 74, 170);

            Et(p, "1NT Açılışlarına Cevaplar:", 22, 100);
            Et(p, "2♣:", 40, 124);
            var g = Gr(p, 72, 120, 190, 70);
            Rb(g, "Naturel", 0, 0, 90, () => !Al("Stayman"), () => { Koy("Stayman", false); Koy("Smolen", false); Koy("ExtendedStayman", false); Si("NT1Puppet", 0); });
            Rb(g, "Stayman", 90, 0, 98, () => Al("Stayman") && !Al("ExtendedStayman") && Gi("NT1Puppet") == 0, () => { Koy("Stayman", true); Koy("ExtendedStayman", false); Si("NT1Puppet", 0); });
            Rb(g, "Extended Stayman", 0, 24, 188, () => Al("ExtendedStayman") && Gi("NT1Puppet") == 0, () => { Koy("ExtendedStayman", true); Si("NT1Puppet", 0); });
            Rb(g, "Puppet Stayman", 0, 48, 188, () => Al("Stayman") && Gi("NT1Puppet") == 1, () => { Koy("Stayman", true); Koy("ExtendedStayman", false); Si("NT1Puppet", 1); });
            Et(p, "Transfer Teklifleri:", 276, 102);
            Cb(p, "JacobyTransfer", "Jacoby Transferleri", 266, 122, 160);
            Cb(p, "TexasTransfers", "Texas Transferleri", 266, 144, 160);
            Cb(p, "Smolen", "Smolen", 266, 166, 160);

            Et(p, "Konvansiyonel Minör Teklifleri:", 22, 194);
            var m = Gr(p, 40, 214, 390, 70);
            Func<bool> hicbiri = () => !Al("MinorStayman") && !Al("MinorTransfer");
            Rb(m, "Yok", 0, 0, 60, () => hicbiri() && Gi("NT1MinorBid") == 0, () => { Koy("MinorStayman", false); Koy("MinorTransfer", false); Si("NT1MinorBid", 0); });
            Rb(m, "Minör Stayman", 70, 0, 120, () => Al("MinorStayman"), () => { Koy("MinorStayman", true); Si("NT1MinorBid", 0); });
            Rb(m, "1NT-2♠ → 3♣ röle", 200, 0, 180, () => hicbiri() && Gi("NT1MinorBid") == 2, () => { Koy("MinorStayman", false); Koy("MinorTransfer", false); Si("NT1MinorBid", 2); });
            Rb(m, "Minör Transferleri (Süper kabullü, 2NT→♦)", 0, 24, 380, () => hicbiri() && Gi("NT1MinorBid") == 3, () => { Koy("MinorStayman", false); Koy("MinorTransfer", false); Si("NT1MinorBid", 3); });
            Rb(m, "Minör Transferleri (Süper kabulsüz, 2NT→♦)", 0, 48, 380, () => Al("MinorTransfer"), () => { Koy("MinorTransfer", true); Si("NT1MinorBid", 0); });

            Cb(p, "Lebensohl", "Lebensohl", 40, 290, 150);
            Cb(p, "Rubensohl", "Rubensohl", 200, 290, 150);

            Et(p, "1NT'ye Karşı Savunma:", 22, 318);
            var d = Gr(p, 150, 314, 280, 46);
            Func<bool> hicS = () => !Al("Cappelletti") && !Al("Dont") && !Al("MultiLandy") && !Al("HelloOver1NT");
            Action<int> digerS = v => { Koy("Cappelletti", false); Koy("Dont", false); Koy("MultiLandy", false); Koy("HelloOver1NT", false); Si("NT1DefenseX", v); };
            Rb(d, "Naturel", 0, 0, 64, () => hicS() && Gi("NT1DefenseX") == 0, () => digerS(0));
            Rb(d, "D.O.N.T.", 66, 0, 74, () => Al("Dont"), () => { Koy("Dont", true); Si("NT1DefenseX", 0); });
            Rb(d, "Brozel", 142, 0, 66, () => hicS() && Gi("NT1DefenseX") == 1, () => digerS(1));
            Rb(d, "Hello", 210, 0, 66, () => Al("HelloOver1NT"), () => { Koy("HelloOver1NT", true); Si("NT1DefenseX", 0); });
            Rb(d, "Cappelletti", 0, 22, 90, () => Al("Cappelletti"), () => { Koy("Cappelletti", true); Si("NT1DefenseX", 0); });
            Rb(d, "Astro", 92, 22, 56, () => hicS() && Gi("NT1DefenseX") == 2, () => digerS(2));
            Rb(d, "Landy", 150, 22, 62, () => Al("MultiLandy"), () => { Koy("MultiLandy", true); Si("NT1DefenseX", 0); });

            Et(p, "1NT'ye Penaltı DBL\nsonrası Kaçış:", 22, 368);
            var k2 = Gr(p, 150, 364, 280, 46);
            Func<bool> hicK = () => !Al("Robinson") && !Al("ExitTransfer");
            Action<int> digerK = v => { Koy("Robinson", false); Koy("ExitTransfer", false); Si("NT1RunoutX", v); };
            Rb(k2, "Naturel", 0, 0, 64, () => hicK() && Gi("NT1RunoutX") == 0, () => digerK(0));
            Rb(k2, "Robinson", 66, 0, 80, () => Al("Robinson"), () => { Koy("Robinson", true); Si("NT1RunoutX", 0); });
            Rb(k2, "Exit transfer", 150, 0, 120, () => Al("ExitTransfer"), () => { Koy("ExitTransfer", true); Si("NT1RunoutX", 0); });
            Rb(k2, "D.O.N.T. (Kaçış)", 0, 22, 130, () => hicK() && Gi("NT1RunoutX") == 3, () => digerK(3));
            Rb(k2, "Helvic", 150, 22, 80, () => hicK() && Gi("NT1RunoutX") == 4, () => digerK(4));
            return p;
        }

        // ───────────── Sayfa 3: Majör ─────────────

        private Panel Sayfa3()
        {
            var p = YeniSayfa();
            Et(p, "Majör Açılışlara Cevaplar:", 22, 18);
            var g = Gr(p, 40, 40, 390, 52);
            Func<bool> raiseYok = () => !Al("BergenRaises") && !Al("InvertedBergenRaises");
            Action<int> raiseDiger = v => { Koy("BergenRaises", false); Koy("InvertedBergenRaises", false); Si("MajorRaiseLimit", v); };
            Rb(g, "Standart Yükseltmeler", 0, 0, 190, () => raiseYok() && Gi("MajorRaiseLimit") == 0, () => raiseDiger(0));
            Rb(g, "Bergen Yükseltmeleri", 0, 26, 190, () => Al("BergenRaises"), () => { Koy("BergenRaises", true); Si("MajorRaiseLimit", 0); });
            Rb(g, "Limit Majör Yükseltmeleri", 190, 0, 190, () => raiseYok() && Gi("MajorRaiseLimit") == 1, () => raiseDiger(1));
            Rb(g, "Ters Bergen Yükseltmeleri", 190, 26, 190, () => Al("InvertedBergenRaises"), () => { Koy("InvertedBergenRaises", true); Si("MajorRaiseLimit", 0); });

            Cb(p, "Jacoby2NT", "Jacoby 2NT", 40, 112, 170);
            Cb(p, "Truscott2NT", "DBL Sonrası Truscott 2NT", 210, 112, 220);
            Cb(p, "Splinter", "Splinter'lar", 40, 136, 170);
            Cb(p, "PreemptiveReraises", "Blokatif Yükseltmeler", 210, 136, 220);
            Cb(p, "Artificial2C2D", "DBL Sonrası Yapay 2♣/2♦ Yükseltmeleri", 40, 160, 380);

            Et(p, "Oyun denemeleri:", 22, 196);
            var t = Gr(p, 40, 218, 390, 52);
            Func<bool> denemeYok = () => !Al("TrialBid") && !Al("LongShortGameTries");
            Action<int> denemeDiger = v => { Koy("TrialBid", false); Koy("LongShortGameTries", false); Si("GameTryShort", v); };
            Rb(t, "Yok", 0, 0, 190, () => denemeYok() && Gi("GameTryShort") == 0, () => denemeDiger(0));
            Rb(t, "Uzun Renk", 0, 26, 190, () => Al("TrialBid"), () => { Koy("TrialBid", true); Si("GameTryShort", 0); });
            Rb(t, "Kısa Renk", 190, 0, 190, () => denemeYok() && Gi("GameTryShort") == 1, () => denemeDiger(1));
            Rb(t, "Hem uzun hem kısa renk", 190, 26, 190, () => Al("LongShortGameTries"), () => { Koy("LongShortGameTries", true); Si("GameTryShort", 0); });

            Et(p, "1NT:", 22, 292);
            var n = Gr(p, 40, 314, 390, 26);
            Rb(n, "Naturel", 0, 0, 100, () => !Al("ForcingNT") && Gi("NTSemiForcing") == 0, () => { Koy("ForcingNT", false); Si("NTSemiForcing", 0); });
            Rb(n, "Forcing", 110, 0, 110, () => Al("ForcingNT"), () => { Koy("ForcingNT", true); Si("NTSemiForcing", 0); });
            Rb(n, "Yarım Forcing", 230, 0, 140, () => !Al("ForcingNT") && Gi("NTSemiForcing") == 1, () => { Koy("ForcingNT", false); Si("NTSemiForcing", 1); });

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
            Rb(p, "En İyi Minör", 40, 32, 190, () => Gi("MinorShortClub") == 0, () => Si("MinorShortClub", 0));
            Rb(p, "Kısa Sinek", 240, 32, 170, () => Gi("MinorShortClub") == 1, () => Si("MinorShortClub", 1));

            Et(p, "Minör Açılışlara Cevaplar:", 22, 62);
            var g = Gr(p, 40, 82, 190, 48);
            Rb(g, "Standart", 0, 0, 180, () => !Al("InvertedMinors"), () => Koy("InvertedMinors", false));
            Rb(g, "İnverted Minör Yükseltmeleri", 0, 24, 180, () => Al("InvertedMinors"), () => Koy("InvertedMinors", true));
            Cb(p, "Walsh", "Walsh", 240, 82, 170);
            Cb(p, "OneClub_AnswerSistemi", "1♣ Cevap Sistemi", 240, 106, 170);
            Cb(p, "OneDiamond_AnswerSistemi", "1♦ Cevap Sistemi", 240, 130, 170);

            Et(p, "2NT'ye Sıçrama:", 22, 136);
            var j = Gr(p, 40, 156, 190, 48);
            Rb(j, "Forcing", 0, 0, 180, () => !Al("OneMinorTwoNTInv"), () => Koy("OneMinorTwoNTInv", false));
            Rb(j, "Davet", 0, 24, 180, () => Al("OneMinorTwoNTInv"), () => Koy("OneMinorTwoNTInv", true));
            Cb(p, "XYZ", "XYZ", 240, 156, 170);
            Cb(p, "ThirdSuitForcing", "3. Renk Forcing", 240, 180, 170);

            Et(p, "Özel Checkback Deklereleri:", 22, 210);
            var c = Gr(p, 40, 230, 190, 76);
            Func<bool> cbYok = () => !Al("TwoWayCheckback") && !Al("NewMinorForcing");
            Action<int> cbDiger = v => { Koy("TwoWayCheckback", false); Koy("NewMinorForcing", false); Si("CheckbackCrowhurst", v); };
            Rb(c, "Yok", 0, 0, 180, () => cbYok() && Gi("CheckbackCrowhurst") == 0, () => cbDiger(0));
            Rb(c, "Yeni Minör Forcingi", 0, 24, 180, () => Al("NewMinorForcing"), () => { Koy("NewMinorForcing", true); Si("CheckbackCrowhurst", 0); });
            Rb(c, "Crowhurst", 0, 48, 180, () => cbYok() && Gi("CheckbackCrowhurst") == 1, () => cbDiger(1));
            Cb(p, "WolffSignoff", "Wolff Sign Off", 240, 230, 170);
            var c2 = Gr(p, 240, 254, 180, 26);
            Rb(c2, "Two Way Checkback", 0, 0, 180, () => Al("TwoWayCheckback"), () => { Koy("TwoWayCheckback", true); Si("CheckbackCrowhurst", 0); });
            Cb(p, "German2CRelays", "Alman Konvansiyonu", 240, 284, 170);

            Et(p, "Sıçrama Cevapları:", 22, 312);
            var sj = Gr(p, 40, 334, 400, 24);
            Rb(sj, "Güçlü Sıçrama-shift", 0, 0, 190, () => !Al("WeakJumpShiftResp"), () => Koy("WeakJumpShiftResp", false));
            Rb(sj, "Zayıf Sıçrama-shift", 200, 0, 190, () => Al("WeakJumpShiftResp"), () => Koy("WeakJumpShiftResp", true));

            Et(p, "Dördüncü Renk Deklereleri:", 22, 364);
            var f = Gr(p, 40, 386, 400, 24);
            Rb(f, "Naturel", 0, 0, 100, () => !Al("FourthSuitForcing") && Gi("FourthOneRound") == 0, () => { Koy("FourthSuitForcing", false); Si("FourthOneRound", 0); });
            Rb(f, "Zon Forcing", 100, 0, 170, () => Al("FourthSuitForcing"), () => { Koy("FourthSuitForcing", true); Si("FourthOneRound", 0); });
            Rb(f, "Bir Tur Forcing", 270, 0, 130, () => !Al("FourthSuitForcing") && Gi("FourthOneRound") == 1, () => { Koy("FourthSuitForcing", false); Si("FourthOneRound", 1); });
            return p;
        }

        // ───────────── Sayfa 5: Yüksek seviye ─────────────

        private Panel Sayfa5()
        {
            var p = YeniSayfa();
            Et(p, "Konvansiyonel İki Seviye Açılışlar:", 22, 18);
            Cb(p, "Multi2D", "Multi 2♦", 40, 42, 120);
            Cb(p, "Flannery", "Flannery 2♦", 164, 42, 120);
            Cb(p, "MiniRoman2D", "Mini-Roman 2♦", 290, 42, 130);

            Et(p, "Güçlü 2♣ Açılışına Cevap:", 22, 88);
            Cb(p, "German2CRelays", "Alman Konvansiyonu", 40, 112, 190);
            Cb(p, "Kokish", "Kokish (2♣–2♦–2♥)", 240, 112, 180);

            Et(p, "Zayıf İki Açılışlarına Konvansiyonel Cevaplar:", 22, 158);
            Cb(p, "Ogust", "Ogust", 40, 182, 190);
            Cb(p, "FeatureAsking2NT", "Feature asking 2NT", 240, 182, 220);

            Et(p, "Zayıf İki Açılışlarına Karşı Savunma:", 22, 228);
            CbI(p, "LebensohlWeakTwo", "Zayıf İkilere Karşı Lebensohl", 40, 252, 260);

            Et(p, "Dört Seviye Minör Açılışlar:", 22, 298);
            Cb(p, "Namyats", "Namyats", 40, 322, 190);

            Et(p, "Blokatif Açılışlar:", 22, 368);
            Cb(p, "Preempt", "3. ve 4. Seviye Blokatif Açılışlar", 40, 392, 300);
            return p;
        }

        // ───────────── Sayfa 6: Yüksek NT ─────────────

        private Panel Sayfa6()
        {
            var p = YeniSayfa();
            Et(p, "2NT Açılışı:", 22, 18);
            string[] r1 = { "19-20", "20-21", "21-22", "22-23", "23-24" };
            string[] r2 = { "19-21", "20-22", "21-23", "22-24" };
            for (int k = 0; k < r1.Length; k++) { int kk = k; Rb(p, r1[k], 24 + 80 * k, 42, 76, () => Gi("NT2Range") == kk, () => Si("NT2Range", kk)); }
            for (int k = 0; k < r2.Length; k++) { int kk = 5 + k; Rb(p, r2[k], 24 + 80 * k, 66, 76, () => Gi("NT2Range") == kk, () => Si("NT2Range", kk)); }

            Et(p, "2NT Açılışına Cevaplar:", 22, 112);
            Et(p, "3♣:", 40, 138);
            var g = Gr(p, 80, 134, 300, 78);
            Rb(g, "Naturel", 0, 0, 200, () => !Al("PuppetStayman") && Gi("NT2Natural") == 1, () => { Koy("PuppetStayman", false); Si("NT2Natural", 1); });
            Rb(g, "Stayman", 0, 26, 200, () => !Al("PuppetStayman") && Gi("NT2Natural") == 0, () => { Koy("PuppetStayman", false); Si("NT2Natural", 0); });
            Rb(g, "Puppet Stayman", 0, 52, 200, () => Al("PuppetStayman"), () => { Koy("PuppetStayman", true); Si("NT2Natural", 0); });

            Et(p, "3NT Açılışı:", 22, 236);
            var t = Gr(p, 60, 260, 340, 52);
            Rb(t, "25+ HP dengeli", 0, 0, 300, () => !Al("Gambling3NT"), () => Koy("Gambling3NT", false));
            Rb(t, "Gambling Three Notrump (Uzun Sağlam Minör Açılışı)", 0, 26, 340, () => Al("Gambling3NT"), () => Koy("Gambling3NT", true));
            return p;
        }

        // ───────────── Sayfa 7: Slam ─────────────

        private Panel Sayfa7()
        {
            var p = YeniSayfa();
            Et(p, "As Soran Deklereler:", 22, 18);
            var g = Gr(p, 40, 40, 390, 52);
            Rb(g, "Blackwood", 0, 0, 190, () => !Al("RKCB") && !Al("RKCB1430"), () => { Koy("RKCB1430", false); Koy("RKCB", false); Koy("Blackwood", true); });
            Rb(g, "RKC 1430", 0, 26, 190, () => Al("RKCB1430"), () => { Koy("RKCB", true); Koy("RKCB1430", true); Koy("Blackwood", true); });
            Rb(g, "Roman Keycard Blackwood", 190, 0, 200, () => Al("RKCB") && !Al("RKCB1430"), () => { Koy("RKCB", true); Koy("RKCB1430", false); Koy("Blackwood", true); });

            Et(p, "Blackwood sonrası papaz soran 5NT:", 22, 112);
            var k = Gr(p, 40, 134, 390, 26);
            Rb(k, "Standart", 0, 0, 190, () => !Al("SpecificKingAsks"), () => Koy("SpecificKingAsks", false));
            Rb(k, "Belirli papazlar", 190, 0, 190, () => Al("SpecificKingAsks"), () => Koy("SpecificKingAsks", true));

            Et(p, "Diğer Şlem Konvansiyonları:", 22, 184);
            Cb(p, "DopiRopi", "DOPI / ROPI", 40, 208, 170);
            CbI(p, "GrandSlamForce", "Grand Şlem Forcingi", 220, 208, 200);
            Cb(p, "VoluntaryFiveMajor", "Gönüllü 5 Majör", 40, 232, 170);
            Cb(p, "CueBid", "Kontrol gösteren Cue-Bid'ler", 220, 232, 210);
            Cb(p, "MatheAskingBids", "Mathe Soru Deklereler", 40, 256, 170);
            Cb(p, "ControlAskingBids", "Kontrol Soran Deklereler", 220, 256, 210);
            Cb(p, "Gerber", "Gerber", 40, 280, 170);
            Cb(p, "FitCueBid", "Fit cue-bid (müdahalede)", 220, 280, 210);
            Cb(p, "Serious3NT", "Ciddi 3NT", 40, 304, 170);
            Cb(p, "NonSerious3NT", "Ciddi Olmayan 3NT", 220, 304, 210);
            Cb(p, "LastTrain", "Last Train", 40, 328, 170);
            Cb(p, "LightnerDouble", "Lightner DBL", 220, 328, 210);
            return p;
        }

        // ───────────── Sayfa 8: Rekabet ─────────────

        private Panel Sayfa9()
        {
            var p = YeniSayfa();
            Et(p, "SAS (Sasson) cevap ve gelişim sistemleri:", 22, 14);
            Cb(p, "OneHeart_AnswerSistemi", "1♥ Cevap Sistemi", 40, 40, 300);
            Cb(p, "OneSpade_AnswerSistemi", "1♠ Cevap Sistemi", 40, 66, 300);
            Cb(p, "OneNT_AnswerSistemi", "1NT Cevap Sistemi", 40, 92, 300);
            Cb(p, "TwoClub_AnswerSistemi", "2♣ Cevap Sistemi", 40, 118, 300);
            Cb(p, "TwoDiamond_AnswerSistemi", "2♦ Açılış Sistemi", 40, 144, 300);
            Cb(p, "TwoHeart_AnswerSistemi", "2♥ Cevap Sistemi", 40, 170, 300);
            Cb(p, "TwoSpade_AnswerSistemi", "2♠ Cevap Sistemi", 40, 196, 300);
            Cb(p, "TwoNT_AnswerSistemi", "2NT Cevap Sistemi", 40, 222, 300);
            Cb(p, "Role2C_AnswerSistemi", "2♣ Röle Sistemi", 40, 248, 300);
            Cb(p, "Araya_AnswerSistemi", "Araya Giriş Cevapları (1♣)", 40, 274, 300);
            Et(p, "1♣ ve 1♦ cevap sistemleri 'Minör Açılışlar' sayfasındadır.", 22, 312, 400);
            return p;
        }

        private Panel Sayfa8()
        {
            var p = YeniSayfa();
            Et(p, "İki Renkli Deklereler:", 22, 14);
            Cb(p, "Unusual2NT", "Unusual 2NT", 40, 36, 150);
            Cb(p, "Michaels", "Michaels", 40, 58, 150);

            Et(p, "Kapsamlı İki Renkli Teklifler:", 222, 14);
            var gh = Gr(p, 232, 34, 200, 72);
            Rb(gh, "Kapalı", 0, 0, 90, () => !Al("Ghestem") && Gi("LaMajeure") == 0, () => { Koy("Ghestem", false); Si("LaMajeure", 0); });
            Rb(gh, "Ghestem", 0, 24, 120, () => Al("Ghestem"), () => { Koy("Ghestem", true); Si("LaMajeure", 0); });
            Rb(gh, "La Majeure 2 Renkli", 0, 48, 190, () => !Al("Ghestem") && Gi("LaMajeure") == 1, () => { Koy("Ghestem", false); Si("LaMajeure", 1); });

            Et(p, "Gecikmeli iki renkli teklifler:", 22, 96);
            CbI(p, "ForumD", "Forum D", 40, 118, 150);

            Et(p, "Rakip Renk Cue-Bid'leri:", 222, 112);
            var cu = Gr(p, 232, 132, 200, 100);
            Rb(cu, "Güç gösteren", 0, 0, 190, () => !Al("LimitPlusCueBids") && Gi("CueStyle") == 0, () => { Koy("LimitPlusCueBids", false); Si("CueStyle", 0); });
            Rb(cu, "Batı cue-bid'leri", 0, 22, 190, () => !Al("LimitPlusCueBids") && Gi("CueStyle") == 1, () => { Koy("LimitPlusCueBids", false); Si("CueStyle", 1); });
            Rb(cu, "Doğu cue-bid'leri", 0, 44, 190, () => !Al("LimitPlusCueBids") && Gi("CueStyle") == 2, () => { Koy("LimitPlusCueBids", false); Si("CueStyle", 2); });
            Rb(cu, "Ortağın majörüne limit raise ya da üstü", 0, 66, 190, () => Al("LimitPlusCueBids"), () => { Koy("LimitPlusCueBids", true); Si("CueStyle", 0); }).Height = 34;

            Et(p, "Negatif DBL sınırı:", 22, 146);
            var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(40, 170, 60, 24) };
            cmb.Items.AddRange(new object[] { "1♠", "2♣", "2♦", "2♥", "2♠", "3♣", "3♦", "3♥", "3♠" });
            cmb.SelectedIndexChanged += (s, e) => { if (!_sync && cmb.SelectedIndex >= 0) Si("NegDblLimit", cmb.SelectedIndex); };
            _yenile.Add(() => cmb.SelectedIndex = Math.Max(0, Math.Min(8, Gi("NegDblLimit"))));
            p.Controls.Add(cmb);

            Et(p, "İki renkli tekliflere karşı savunma:", 22, 214);
            var sv = Gr(p, 40, 236, 190, 70);
            Rb(sv, "Naturel", 0, 0, 180, () => !Al("InvisibleCueBids") && Gi("UnusualOverUnusual") == 0, () => { Koy("InvisibleCueBids", false); Si("UnusualOverUnusual", 0); });
            Rb(sv, "Unusual over Unusual", 0, 22, 180, () => !Al("InvisibleCueBids") && Gi("UnusualOverUnusual") == 1, () => { Koy("InvisibleCueBids", false); Si("UnusualOverUnusual", 1); });
            Rb(sv, "Görünmez Cue-Bid'ler", 0, 44, 180, () => Al("InvisibleCueBids"), () => { Koy("InvisibleCueBids", true); Si("UnusualOverUnusual", 0); });

            Et(p, "Diğer Yarışma Teklifleri:", 234, 238);
            Cb(p, "WeakJumpOvercalls", "Zayıf Sıçramalı Müdahale", 232, 258, 200);
            Cb(p, "Sandwich1NT", "Sandwich 1NT", 232, 280, 200);
            Cb(p, "LeapingMichaels", "Leaping Michaels", 232, 302, 200);

            Et(p, "Özel DBL'lar:", 22, 332);
            Cb(p, "NegativeDouble", "Negatif DBL", 40, 356, 170);
            Cb(p, "ResponsiveDouble", "Responsive DBL", 40, 378, 170);
            CbI(p, "LeadDirectingDbl", "Atak Yönlendiren DBL", 40, 400, 180);
            Cb(p, "SupportDouble", "Support DBL", 232, 356, 200);
            Cb(p, "MaximalOvercallDbl", "MOD = Maksimal Araya Giriş DBL", 232, 378, 200);
            Cb(p, "TakeoutDouble", "Takeout DBL", 232, 400, 200);
            return p;
        }
    }

    /// <summary>Başlık biçimi: her sözcüğün ilk harfi büyük (Türkçe kurallarıyla; ör. "Profil Adı:").</summary>
    /// <summary>
    /// Basit N/S ve Basit E/W Ortaklık Anlaşması (Bridge Baron "Pair Profile" düzeni): yürürlükteki / yürürlükte olmayan
    /// konvansiyon listeleri, ok düğmeleriyle aktarım, agresiflik, 1NT ve 2NT aralığı, Zorluk Seviyesi ve sinyaller.
    /// Form, etkin profilin basit anlaşma uygulanmış kopyasıyla açılır; Tamam'a basılana kadar hiçbir şey kaydedilmez.
    /// </summary>
    public sealed class BasitAnlasmaForm : Form
    {
        private static readonly Dictionary<string, string> Adlar = new Dictionary<string, string>
        {
            { "BesliMajor", "5'li Majör Açılışı" },
            { "IkiliSinekGuclu", "2♣ Güçlü" },
            { "ZayifIki", "Zayıf İki" },
            { "Preempt", "Blokatif Açılışlar" },
            { "Multi2D", "Multi 2♦" },
            { "Namyats", "Namyats" },
            { "BenjaminTwoBids", "Benjamin" },
            { "ExtendedStayman", "Extended Stayman" },
            { "HelloOver1NT", "Hello" },
            { "Truscott2NT", "Truscott 2NT" },
            { "PreemptiveReraises", "Blokatif Tekrar Yükseltmeler" },
            { "LongShortGameTries", "Uzun&kısa Deneme" },
            { "NewMinorForcing", "Yeni Minör Forcing" },
            { "WeakJumpShiftResp", "Zayıf Sıçrama Cevabı" },
            { "Ogust", "Ogust" },
            { "FeatureAsking2NT", "Feature asking 2NT" },
            { "Gambling3NT", "Gambling Three Notrump (Uzun Sağlam Minör Açılışı)" },
            { "SpecificKingAsks", "Belirli Papaz Sorusu" },
            { "VoluntaryFiveMajor", "Gönüllü 5 Majör" },
            { "MatheAskingBids", "Mathe" },
            { "ControlAskingBids", "Kontrol Soruları" },
            { "Ghestem", "Ghestem" },
            { "InvisibleCueBids", "Görünmez cue-bid" },
            { "LimitPlusCueBids", "Limit-üstü Cue-Bid" },
            { "GoodBad2NT", "Good-Bad 2NT" },
            { "InvertedDbl", "Inverted DBL" },
            { "MixedRaise", "Mixed Raise" },
            { "FSJS", "FSJS" },
            { "WeakJumpOvercalls", "Zayıf Sıçramalı Araya Giriş" },
            { "Sandwich1NT", "Sandwich 1NT" },
            { "MaximalOvercallDbl", "Maximal DBL" },
            { "OneMinorTwoNTInv", "1m–2NT Davet" },
            { "German2CRelays", "Alman Konvansiyonu" },
            { "Walsh", "Walsh" },
            { "MiniRoman2D", "Mini-Roman 2♦" },
            { "Robinson", "Robinson" },
            { "InvertedBergenRaises", "Ters Bergen" },
            { "ExitTransfer", "Exit Transfer" },
            { "Artificial2C2D", "Yapay 2♣/2♦" },
            { "WolffSignoff", "Wolff Sign Off" },
            { "OneClub_AnswerSistemi", "1♣ Cevap Sistemi" },
            { "OneDiamond_AnswerSistemi", "1♦ Cevap Sistemi" },
            { "OneHeart_AnswerSistemi", "1♥ Cevap Sistemi" },
            { "OneSpade_AnswerSistemi", "1♠ Cevap Sistemi" },
            { "OneNT_AnswerSistemi", "1NT Cevap Sistemi" },
            { "TwoClub_AnswerSistemi", "2♣ Cevap Sistemi" },
            { "TwoDiamond_AnswerSistemi", "2♦ Açılış Sistemi" },
            { "TwoHeart_AnswerSistemi", "2♥ Cevap Sistemi" },
            { "TwoSpade_AnswerSistemi", "2♠ Cevap Sistemi" },
            { "TwoNT_AnswerSistemi", "2NT Cevap Sistemi" },
            { "Role2C_AnswerSistemi", "2♣ Röle Sistemi" },
            { "Araya_AnswerSistemi", "Araya Giriş Cevapları" },
            { "Reverse", "Reverse" },
            { "ForcingNT", "Forcing 1NT" },
            { "IkiBirOyunZorlama", "2/1 Zon Forcing" },
            { "Stayman", "Stayman" },
            { "JacobyTransfer", "Jacoby Transfer" },
            { "PuppetStayman", "Puppet Stayman" },
            { "Smolen", "Smolen" },
            { "MinorTransfer", "Minör Transferi" },
            { "BergenRaises", "Bergen" },
            { "MultiLandy", "Multi-Landy" },
            { "InvertedMinors", "İnverted Minörler" },
            { "TwoWayCheckback", "Two Way Checkback" },
            { "XYZ", "XYZ" },
            { "TrialBid", "Trial Bid" },
            { "ThirdSuitForcing", "3. Renk Forcing" },
            { "FourthSuitForcing", "4. Renk Forcing" },
            { "Blackwood", "Blackwood" },
            { "RKCB", "RKCB" },
            { "RKCB1430", "RKCB 1430" },
            { "Gerber", "Gerber" },
            { "Jacoby2NT", "Jacoby 2NT" },
            { "Splinter", "Splinter" },
            { "Drury", "Drury" },
            { "ReverseDrury", "Ters Drury" },
            { "CueBid", "Cue Bid" },
            { "Serious3NT", "Ciddi 3NT" },
            { "NonSerious3NT", "Ciddi olmayan 3NT" },
            { "LastTrain", "Last Train" },
            { "DopiRopi", "DOPI/ROPI" },
            { "FitCueBid", "Fit Cue-Bid" },
            { "Quantitative4NT", "Nicel 4NT" },
            { "TexasTransfers", "Texas" },
            { "MinorStayman", "Minör Stayman" },
            { "Flannery", "Flannery 2♦" },
            { "Kokish", "Kokish" },
            { "Rubensohl", "Rubensohl" },
            { "LeapingMichaels", "Leaping Michaels" },
            { "LightnerDouble", "Lightner DBL" },
            { "NegativeDouble", "Negatif DBL" },
            { "TakeoutDouble", "Takeout DBL" },
            { "SupportDouble", "Support DBL" },
            { "ResponsiveDouble", "Responsive DBL" },
            { "Lebensohl", "Lebensohl" },
            { "Michaels", "Michaels" },
            { "Unusual2NT", "Unusual 2NT" },
            { "Cappelletti", "Cappelletti" },
            { "Dont", "DONT" }
        };

        private static readonly string[] AgrAdlari = { "Çok Temkinli", "Temkinli", "Orta", "Atak", "Çok atak" };
        private static readonly string[] Ar1 = { "10-12", "11-13", "12-14", "13-15", "14-16", "15-17", "16-18" };
        private static readonly string[] Ar2 = { "19-20", "20-21", "21-22", "22-23", "23-24", "19-21", "20-22", "21-23", "22-24" };

        private readonly Taraf _taraf;
        private readonly List<AnlasmaProfil> _profiller;
        private AnlasmaProfil _secili;
        private bool _yukleniyor;

        private ComboBox _cmbProfil;
        private Label _lblSistem, _lblBeceri;
        private readonly ComboBox[] _cmbAgr = new ComboBox[3];   // 0 müdahale, 1 önleyici, 2 açılış
        private CheckBox _chkOgrenme;
        private Button _btnSil, _btnBeceri;
        private Control[] _duzenlenir;     // hazır (sabit) profilde kapalı olan denetimler
        private ListBox _lstIcinde, _lstDisinda;
        private readonly RadioButton[] _rb1 = new RadioButton[7];
        private readonly RadioButton[] _rb2 = new RadioButton[9];

        public BasitAnlasmaForm(Taraf taraf)
        {
            _taraf = taraf;
            _profiller = AnlasmaYonetici.Profiller(taraf);
            string aktif = AnlasmaYonetici.AktifProfilAdi(taraf);
            var etkin = _profiller.FirstOrDefault(p => p.Ad == aktif) ?? _profiller[0];
            // Kullanıcının "Basit Anlaşma" profili: ilk açılışta basit anlaşmadan oluşturulur, sonra kaydedilen hâliyle açılır.
            _secili = _profiller.FirstOrDefault(p => !p.Sabit && p.Ad == BasitProfilAdi);
            if (_secili == null)
            {
                _secili = new AnlasmaProfil { Ad = BasitProfilAdi, Anlasma = OrtaklikAnlasmasi.Basit() };
                var m = etkin.Anlasma;
                _secili.Anlasma.AcilisAgresifligi = m.AcilisAgresifligi; _secili.Anlasma.MudahaleAgresifligi = m.MudahaleAgresifligi;
                _secili.Anlasma.PreemptAgresifligi = m.PreemptAgresifligi; _secili.Anlasma.BeceriDuzeyi = m.BeceriDuzeyi;
                _secili.SistemAdiniBelirle();
                _profiller.Add(_secili);
            }
            Kur();
            ProfilListesiniDoldur();
            SeciliyiGoster();
        }

        private const string BasitProfilAdi = "Basit Anlaşma";

        private static PropertyInfo Pr(string ad) { return typeof(OrtaklikAnlasmasi).GetProperty(ad); }
        private bool Acik(string ad) { return (bool)Pr(ad).GetValue(_secili.Anlasma, null); }

        private T Yer<T>(T c, int x, int y, int w, int h) where T : Control
        {
            c.Bounds = new Rectangle(x, y - 14, w, h);
            Controls.Add(c);
            return c;
        }

        private void Kur()
        {
            Text = "Basit " + (_taraf == Taraf.NS ? "N/S" : "E/W") + " Ortaklık Anlaşması";
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            ClientSize = new Size(800, 475);
            Font = new Font("Segoe UI", 9F);

            // ── Sol sütun ──
            var btnYeni = Yer(new Button { Text = "Yeni" }, 11, 31, 76, 23);
            _btnSil = Yer(new Button { Text = "Sil" }, 97, 31, 80, 23);
            btnYeni.Click += (s, e) => YeniProfil();
            _btnSil.Click += (s, e) => ProfilSil();

            Yer(new Label { Text = "Profil Adı:", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft }, 12, 72, 66, 22);
            _cmbProfil = Yer(new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, 80, 72, 192, 23);
            _cmbProfil.SelectedIndexChanged += (s, e) => ProfilDegisti();

            Yer(new Label { Text = "İhale sistemi:", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft }, 12, 102, 84, 20);
            _lblSistem = Yer(new Label { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft }, 98, 102, 174, 20);

            Yer(new Label { Text = "Zorluk Seviyesi:", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft }, 12, 128, 100, 20);
            _lblBeceri = Yer(new Label { AutoSize = false, TextAlign = ContentAlignment.MiddleCenter }, 12, 160, 258, 20);
            var btnBeceri = _btnBeceri = Yer(new Button { Text = "Zorluk Seviyesini seçmek için tıklayın" }, 12, 190, 258, 24);
            btnBeceri.Click += (s, e) => BeceriSec();

            _chkOgrenme = Yer(new CheckBox { Text = "Öğrenme modu", AutoSize = false, Checked = AnlasmaYonetici.OgrenmeModuAcik }, 12, 224, 200, 20);

            var gAgr = Yer(new GroupBox { Text = "Agresiflik" }, 12, 250, 258, 92);
            string[] ad = { "Araya Giriş:", "Blokatif Açılış:", "Açılış:" };
            for (int i = 0; i < 3; i++)
            {
                int k = i;
                gAgr.Controls.Add(new Label { Text = ad[i], AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, Bounds = new Rectangle(8, 18 + 24 * i, 84, 21) });
                var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(94, 17 + 24 * i, 150, 23) };
                c.Items.AddRange(AgrAdlari);
                c.SelectedIndexChanged += (s, e) => AgresiflikDegisti(k);
                gAgr.Controls.Add(c);
                _cmbAgr[i] = c;
            }

            var g1 = Yer(new GroupBox { Text = "1NT Aralığı" }, 12, 348, 258, 62);
            int[] s1 = { 0, 2, 4, 6, 1, 3, 5 };
            for (int j = 0; j < 7; j++)
            {
                int kk = s1[j];
                var rb = new RadioButton { Text = Ar1[kk], AutoSize = false, Bounds = new Rectangle(8 + 62 * (j % 4), 16 + 20 * (j / 4), 62, 20) };
                rb.CheckedChanged += (s, e) => { if (rb.Checked && !_yukleniyor) { _secili.Anlasma.NT1Range = kk; _secili.Anlasma.StrongNT = kk >= 4; SistemGuncelle(); } };
                g1.Controls.Add(rb); _rb1[kk] = rb;
            }
            var g2 = Yer(new GroupBox { Text = "2NT Aralığı" }, 12, 414, 300, 62);
            for (int kk = 0; kk < 9; kk++)
            {
                int k2 = kk, sat = kk / 5, sut = kk % 5;
                var rb = new RadioButton { Text = Ar2[kk], AutoSize = false, Bounds = new Rectangle(8 + 58 * sut, 16 + 20 * sat, 58, 20) };
                rb.CheckedChanged += (s, e) => { if (rb.Checked && !_yukleniyor) _secili.Anlasma.NT2Range = k2; };
                g2.Controls.Add(rb); _rb2[kk] = rb;
            }

            // ── Konvansiyon listeleri ──
            Yer(new Label { Text = "Aktif Konvansiyonlar", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter }, 286, 48, 212, 18);
            Yer(new Label { Text = "Aktif olmayan konvansiyonlar", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter }, 577, 48, 213, 18);
            _lstIcinde = Yer(new ListBox { SelectionMode = SelectionMode.MultiExtended, IntegralHeight = false }, 286, 68, 212, 313);
            _lstDisinda = Yer(new ListBox { SelectionMode = SelectionMode.MultiExtended, IntegralHeight = false }, 577, 68, 213, 313);
            var btnEkle = Yer(new Button { Text = "←" }, 516, 203, 44, 23);
            var btnCikar = Yer(new Button { Text = "→" }, 516, 235, 44, 23);
            _duzenlenir = new Control[] { gAgr, g1, g2, _lstIcinde, _lstDisinda, btnEkle, btnCikar };
            btnEkle.Click += (s, e) => Tasi(_lstDisinda, true);
            btnCikar.Click += (s, e) => Tasi(_lstIcinde, false);
            _lstDisinda.DoubleClick += (s, e) => Tasi(_lstDisinda, true);
            _lstIcinde.DoubleClick += (s, e) => Tasi(_lstIcinde, false);

            // ── Alt satır ──
            var btnSinyal = Yer(new Button { Text = "Sinyaller" }, 418, 424, 80, 23);
            btnSinyal.Click += (s, e) => { using (var f = new SinyallerForm(_secili.Anlasma)) f.ShowDialog(this); };
            var btnIptal = Yer(new Button { Text = "İptal" }, 582, 424, 80, 23);
            var btnTamam = Yer(new Button { Text = "Tamam" }, 669, 424, 80, 23);
            btnTamam.Click += BtnTamam_Click;
            btnIptal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            AcceptButton = btnTamam; CancelButton = btnIptal;

            BaslikBicimi.Uygula(this, true);
        }

        // ───────────────────────── Profiller ─────────────────────────

        private void ProfilListesiniDoldur()
        {
            _yukleniyor = true;
            try
            {
                _cmbProfil.Items.Clear();
                foreach (var p in _profiller) _cmbProfil.Items.Add(p.Ad);
                _cmbProfil.SelectedIndex = _profiller.IndexOf(_secili);
            }
            finally { _yukleniyor = false; }
            SabitKilidi();
        }

        /// <summary>Hazır (sabit) profil seçiliyken seçimler değiştirilemez ve profil silinemez.</summary>
        private void SabitKilidi()
        {
            _btnSil.Enabled = !_secili.Sabit;
            if (_btnBeceri != null) _btnBeceri.Enabled = !_secili.Sabit;
            if (_duzenlenir != null) foreach (var c in _duzenlenir) c.Enabled = !_secili.Sabit;
        }

        private void ProfilDegisti()
        {
            if (_yukleniyor || _cmbProfil.SelectedIndex < 0) return;
            _secili = _profiller[_cmbProfil.SelectedIndex];
            SeciliyiGoster();
        }

        private void SeciliyiGoster()
        {
            _yukleniyor = true;
            try
            {
                var a = _secili.Anlasma;
                int[] v = { a.MudahaleAgresifligi, a.PreemptAgresifligi, a.AcilisAgresifligi };
                for (int i = 0; i < 3; i++) _cmbAgr[i].SelectedIndex = Math.Max(0, Math.Min(4, v[i] + 2));
                int r1 = NTA.Lo1(a) - 10;
                for (int k = 0; k < _rb1.Length; k++) _rb1[k].Checked = k == r1;
                int r2 = Math.Max(0, Math.Min(8, a.NT2Range));
                for (int k = 0; k < _rb2.Length; k++) _rb2[k].Checked = k == r2;
                _lblBeceri.Text = a.BeceriDuzeyi + " / 20";
            }
            finally { _yukleniyor = false; }
            SabitKilidi();
            SistemGoster();
            ListeleriDoldur(null);
        }

        /// <summary>"İhale Sistemi:" satırı: profilde kayıtlı sistem adı.</summary>
        private void SistemGoster()
        {
            if (string.IsNullOrEmpty(_secili.Sistem)) _secili.SistemAdiniBelirle();
            _lblSistem.Text = _secili.Sistem;
        }

        /// <summary>Konvansiyon / aralık seçimi değişince sistem adı yeniden belirlenir (ön ayara uyuyorsa onun adı, değilse "Özel").</summary>
        private void SistemGuncelle()
        {
            _secili.SistemAdiniBelirle();
            _lblSistem.Text = _secili.Sistem;
        }

        private void AgresiflikDegisti(int i)
        {
            if (_yukleniyor) return;
            int v = _cmbAgr[i].SelectedIndex - 2;
            if (i == 0) _secili.Anlasma.MudahaleAgresifligi = v;
            else if (i == 1) _secili.Anlasma.PreemptAgresifligi = v;
            else _secili.Anlasma.AcilisAgresifligi = v;
        }

        private void YeniProfil()
        {
            string ad = AdSor("Yeni Profilin Adı:", "Yeni Profil");
            if (string.IsNullOrWhiteSpace(ad)) return;
            ad = ad.Trim(); string aday = ad; int n = 2;
            while (_profiller.Any(p => string.Equals(p.Ad, aday, StringComparison.CurrentCultureIgnoreCase))) aday = ad + " " + n++;
            var yeni = new AnlasmaProfil { Ad = aday, Anlasma = OrtaklikAnlasmasi.Basit() };
            yeni.SistemAdiniBelirle();
            _profiller.Add(yeni);
            _secili = yeni;
            ProfilListesiniDoldur();
            SeciliyiGoster();
        }

        private void ProfilSil()
        {
            if (_secili.Sabit || _profiller.Count <= 1) return;
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
                f.Text = "Yeni Profil";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowInTaskbar = false; f.MinimizeBox = false; f.MaximizeBox = false;
                f.ClientSize = new Size(320, 108);
                var l = new Label { Text = soru, AutoSize = true, Location = new Point(12, 12) };
                var t = new TextBox { Text = varsayilan, Location = new Point(12, 34), Width = 296 };
                var ok = new Button { Text = "Tamam", DialogResult = DialogResult.OK, Location = new Point(138, 68), Size = new Size(80, 28) };
                var iptal = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Location = new Point(228, 68), Size = new Size(80, 28) };
                f.Controls.AddRange(new Control[] { l, t, ok, iptal });
                f.AcceptButton = ok; f.CancelButton = iptal;
                t.SelectAll();
                return f.ShowDialog(this) == DialogResult.OK ? t.Text : null;
            }
        }

        private void BeceriSec()
        {
            using (var f = new SeviyeAyarlariForm(_secili.Anlasma))
                if (f.ShowDialog(this) == DialogResult.OK) _lblBeceri.Text = _secili.Anlasma.BeceriDuzeyi + " / 20";
        }

        // ───────────────────────── Konvansiyon listeleri ─────────────────────────

        private sealed class Oge
        {
            public string Alan, Ad;
            public override string ToString() { return Ad; }
        }

        private void ListeleriDoldur(HashSet<string> secilecek)
        {
            var oges = Adlar.Select(k => new Oge { Alan = k.Key, Ad = k.Value }).OrderBy(o => o.Ad, StringComparer.CurrentCultureIgnoreCase).ToList();
            _lstIcinde.BeginUpdate(); _lstDisinda.BeginUpdate();
            try
            {
                _lstIcinde.Items.Clear(); _lstDisinda.Items.Clear();
                foreach (var o in oges)
                {
                    var l = Acik(o.Alan) ? _lstIcinde : _lstDisinda;
                    int i = l.Items.Add(o);
                    if (secilecek != null && secilecek.Contains(o.Alan)) l.SetSelected(i, true);
                }
            }
            finally { _lstIcinde.EndUpdate(); _lstDisinda.EndUpdate(); }
        }

        /// <summary>ekle = true: seçili maddeler yürürlüğe girer ("&lt;----"); false: yürürlükten çıkar ("----&gt;").</summary>
        private void Tasi(ListBox kaynak, bool ekle)
        {
            var secilen = kaynak.SelectedItems.Cast<Oge>().ToList();
            if (secilen.Count == 0) return;
            foreach (var o in secilen) Koy(o.Alan, ekle);
            ListeleriDoldur(new HashSet<string>(secilen.Select(o => o.Alan)));
            SistemGuncelle();
        }

        private void Koy(string ad, bool v)
        {
            Pr(ad).SetValue(_secili.Anlasma, v, null);
            if (v)
            {
                foreach (var g in ProfilSihirbazi.Dislayan)
                {
                    if (!g.Contains(ad)) continue;
                    foreach (var d in g) if (d != ad) Pr(d).SetValue(_secili.Anlasma, false, null);
                }
                string gerek;
                if (ProfilSihirbazi.Gerektirir.TryGetValue(ad, out gerek)) Pr(gerek).SetValue(_secili.Anlasma, true, null);
            }
            else
            {
                foreach (var kv in ProfilSihirbazi.Gerektirir) if (kv.Value == ad) Pr(kv.Key).SetValue(_secili.Anlasma, false, null);
            }
        }

        private void BtnTamam_Click(object sender, EventArgs e)
        {
            if (!AnlasmaYonetici.Kaydet(_taraf, _profiller, _secili.Ad))
                MessageBox.Show(this, "Profiller kaydedilemedi. Bu oturumda geçerli olacak.", "Ortaklık Anlaşması",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            AnlasmaYonetici.OgrenmeModuAcik = _chkOgrenme.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }

        public static void Goster(IWin32Window sahip, Taraf taraf)
        {
            using (var f = new BasitAnlasmaForm(taraf)) f.ShowDialog(sahip);
        }
    }

    /// <summary>
    /// Düzey Ayarları (Bridge Baron "Level Settings"): her karar için en çok düşünme süresi (0-15 sn) ve Zorluk Seviyesi.
    /// Kart oyunu motoru (PIMC) bu süre içinde olabildiğince çok olası dağılım çözer; ihale ve açılış atağı
    /// kural tabanlı olduğundan o iki sürgü yalnız saklanır. Tamam'a basılınca değerler anlaşmaya yazılır.
    /// </summary>
    public sealed class SeviyeAyarlariForm : Form
    {
        private static readonly string[] Alanlar =
        {
            "SureIhale", "SureAtak", "SureEl1", "SureEl2", "SureEl3", "SureEl4", "SureEl5",
            "SureEl6", "SureEl7", "SureEl8", "SureEl9", "SureEl10", "SureEl11", "SureEl12"
        };
        private static readonly string[] Adlar =
        {
            "Her İhale / Teklif", "Açılış Atağı", "1. elde diğer kartlar", "2. El Kartları", "3. El Kartları", "4. El Kartları", "5. El Kartları",
            "6. El Kartları", "7. El Kartları", "8. El Kartları", "9. El Kartları", "10. El Kartları", "11. El Kartları", "12. El Kartları"
        };
        private static readonly string[] OnAyarlar =
        {
            "Varsayılan Oyun / Ana Sayfa Sürgülerini Etkinleştir", "Anında Oyun", "Çok Hızlı Oyun", "Hızlı Oyun",
            "Orta Hızlı Oyun", "Düşünceli Oyun", "Çok Düşünceli Oyun", "Özel Oyun"
        };
        // sn: Varsayılan (ölçülmüş ayar) 1, Anında 0 (sezgisel kurallar), Çok hızlı 1, Hızlı 2, Orta 4, Düşünceli 7, Çok düşünceli 15
        private static readonly int[] OnAyarSure = { 1, 0, 1, 2, 4, 7, 15 };
        private const int OzelIndeks = 7;

        private readonly OrtaklikAnlasmasi _a;
        private readonly TrackBar[] _tb = new TrackBar[14];
        private readonly Label[] _lb = new Label[14];
        private TrackBar _tbBeceri;
        private Label _lblBeceri;
        private ComboBox _cmbOnAyar;
        private bool _yukleniyor;

        public SeviyeAyarlariForm(OrtaklikAnlasmasi anlasma)
        {
            _a = anlasma;
            Text = "Seviye Ayarları";
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            ClientSize = new Size(730, 372);
            Font = new Font("Segoe UI", 9F);

            _cmbOnAyar = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = new Rectangle(12, 10, 385, 23) };
            _cmbOnAyar.Items.AddRange(OnAyarlar);
            _cmbOnAyar.SelectedIndexChanged += (s, e) => { if (!_yukleniyor && _cmbOnAyar.SelectedIndex < OzelIndeks) OnAyarUygula(_cmbOnAyar.SelectedIndex); };
            Controls.Add(_cmbOnAyar);
            var btnVar = new Button { Text = "Varsayılan", Bounds = new Rectangle(412, 8, 90, 27) };
            var btnTamam = new Button { Text = "Tamam", Bounds = new Rectangle(518, 8, 90, 27) };
            var btnIptal = new Button { Text = "İptal", Bounds = new Rectangle(624, 8, 90, 27) };
            btnVar.Click += (s, e) => OnAyarUygula(0);
            btnTamam.Click += (s, e) => Kaydet();
            btnIptal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.AddRange(new Control[] { btnVar, btnTamam, btnIptal });
            AcceptButton = btnTamam; CancelButton = btnIptal;

            var ip = new ToolTip { AutoPopDelay = 12000 };
            for (int i = 0; i < 14; i++)
            {
                int k = i;
                int sut = i < 7 ? 0 : 1, sat = i < 7 ? i : i - 7;
                int x = 8 + 355 * sut, y = 50 + 38 * sat;
                var ad = new Label { Text = Adlar[i], AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Bounds = new Rectangle(x, y, 108, 32) };
                var tb = new TrackBar { Minimum = 0, Maximum = 15, TickFrequency = 1, LargeChange = 1, SmallChange = 1, Bounds = new Rectangle(x + 112, y - 2, 150, 36) };
                var lb = new Label { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, Bounds = new Rectangle(x + 266, y, 84, 32) };
                tb.ValueChanged += (s, e) => { lb.Text = "en çok: " + tb.Value + " sn"; if (!_yukleniyor) _cmbOnAyar.SelectedIndex = OzelIndeks; };
                Controls.AddRange(new Control[] { ad, tb, lb });
                _tb[i] = tb; _lb[i] = lb;
                if (i < 3)
                {
                    string ipucu = i == 0 ? "Teklif motoru kural tabanlıdır, arama yapmaz: bu ayar saklanır ama ihaleyi etkilemez."
                                 : i == 1 ? "Açılış atağı sezgisel kurallarla seçilir (üç gizli el çözülemez): bu ayar saklanır ama atağı etkilemez."
                                 : "1. elde 13 kartlık çözüm çok yavaştır; PIMC bu elde devreye girmez. Ayar saklanır.";
                    ip.SetToolTip(ad, ipucu); ip.SetToolTip(tb, ipucu); ip.SetToolTip(lb, ipucu);
                }
            }
            // Zorluk Seviyesi (1-20)
            int by = 50 + 38 * 7 + 6;
            Controls.Add(new Label { Text = "Zorluk Seviyesi", AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Bounds = new Rectangle(8, by, 108, 32) });
            _tbBeceri = new TrackBar { Minimum = 1, Maximum = 20, TickFrequency = 1, LargeChange = 1, Bounds = new Rectangle(120, by - 2, 160, 36) };
            _lblBeceri = new Label { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, Bounds = new Rectangle(284, by, 70, 32) };
            _tbBeceri.ValueChanged += (s, e) => _lblBeceri.Text = _tbBeceri.Value + " / 20";
            Controls.AddRange(new Control[] { _tbBeceri, _lblBeceri,
                new Label { Text = "20 = hatasız; düşük değerde yapay zekâ kart oynarken arada rastgele yasal kart seçer.", AutoSize = false,
                            TextAlign = ContentAlignment.MiddleLeft, Bounds = new Rectangle(363, by, 360, 32) } });

            _yukleniyor = true;
            try
            {
                for (int i = 0; i < 14; i++)
                {
                    int v = (int)typeof(OrtaklikAnlasmasi).GetProperty(Alanlar[i]).GetValue(_a, null);
                    _tb[i].Value = Math.Max(0, Math.Min(15, v));
                    _lb[i].Text = "en çok: " + _tb[i].Value + " sn";
                }
                _tbBeceri.Value = Math.Max(1, Math.Min(20, _a.BeceriDuzeyi));
                _lblBeceri.Text = _tbBeceri.Value + " / 20";
                _cmbOnAyar.SelectedIndex = OnAyarBul();
            }
            finally { _yukleniyor = false; }
            BaslikBicimi.Uygula(this, false);
        }

        private int OnAyarBul()
        {
            for (int p = 0; p < OnAyarSure.Length; p++)
                if (_tb.All(t => t.Value == OnAyarSure[p])) return p;
            return OzelIndeks;
        }

        private void OnAyarUygula(int p)
        {
            _yukleniyor = true;
            try
            {
                foreach (var t in _tb) t.Value = OnAyarSure[p];
                _cmbOnAyar.SelectedIndex = p;
            }
            finally { _yukleniyor = false; }
        }

        private void Kaydet()
        {
            for (int i = 0; i < 14; i++) typeof(OrtaklikAnlasmasi).GetProperty(Alanlar[i]).SetValue(_a, _tb[i].Value, null);
            _a.BeceriDuzeyi = _tbBeceri.Value;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

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
