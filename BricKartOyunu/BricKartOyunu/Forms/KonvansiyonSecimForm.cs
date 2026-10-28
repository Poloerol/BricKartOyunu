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
    /// Bridge Baron "Edit" ekranı düzeninde konvansiyon seçimi: solda oynanan, sağda oynanmayan konvansiyonlar,
    /// ortada ekle/çıkar okları; altta 1NT ve 2NT aralığı ve işaretleşme (Signals).
    /// Tamam ile Sonuc'a düzenlenmiş anlaşma kopyası yazılır; kaydetme profil ekranının işidir.
    /// </summary>
    public sealed class KonvansiyonSecimForm : Form
    {
        private sealed class Madde
        {
            public string Alan, Baslik, Aciklama;
            public Madde(string alan, string baslik, string aciklama) { Alan = alan; Baslik = baslik; Aciklama = aciklama; }
        }

        private sealed class Grup
        {
            public string Ad;
            public Madde[] Maddeler;
            public Grup(string ad, params Madde[] m) { Ad = ad; Maddeler = m; }
        }

        private static readonly Grup[] Gruplar =
        {
            new Grup("Açılış sistemi",
                new Madde("BesliMajor", "5'li majör açılışı", "1♥/1♠ açılışı 5+ kart ister. Kapalıysa 4'lü majör açılır."),
                new Madde("IkiliSinekGuclu", "2♣ güçlü açılış", "22+ HP ya da oyun zorlayan eller."),
                new Madde("ZayifIki", "Zayıf iki açılışları", "2♦/2♥/2♠ ile 6 kartlık zayıf el gösterilir."),
                new Madde("Multi2D", "Multi 2♦", "Zayıf iki majörünü 2♦ altında toplar; Zayıf İki'nin yerini alır."),
                new Madde("Preempt", "Önleyici açılışlar", "3. ve 4. seviyede 7+ kartlık zayıf eller."),
                new Madde("Namyats", "Namyats", "4♣/4♦ açılışları güçlü 4♥/4♠ önleyicilerini gösterir."),
                new Madde("OneClub_AnswerSistemi", "1♣ cevap sistemi (Polish hibrit)", "1♣ açılışına özel cevap tablosu."),
                new Madde("Reverse", "Reverse gelişimi", "Açıcının ikinci rengi daha yüksekse güçlü el (16+) gösterir."),
                new Madde("ForcingNT", "Zorlayıcı 1NT", "1M açılışına 1NT cevabı bir tur zorlayıcıdır."),
                new Madde("BenjaminTwoBids", "Benjamin iki'ler", "2♣ çok güçlü, 2♦ güçlü iki (Acol); 2♥/2♠ zayıf kalır. 2♣ güçlü açılışın yerini alır."),
                new Madde("Gambling3NT", "Gambling 3NT", "3NT açılışı: sağlam 7+ minör, dışarıda A/K yok. Ortak durdurucu yoksa 4♣ ile düzeltir."),
                new Madde("MiniRoman2D", "Mini-Roman 2♦", "2♦: ♦ tekil/void, ♠♥♣ dörtlü (11-15 HP); cevap geç/düzelt.")),
            new Grup("Cevap ve gelişim",
                new Madde("InvertedMinors", "Ters minörler", "1m–2m destek davetidir (1m–3m önleyici)."),
                new Madde("BergenRaises", "Bergen destekleri", "Majör açılışına 3♣/3♦ ile 4'lü destek gösterir."),
                new Madde("Jacoby2NT", "Jacoby 2NT", "Majör açılışına 2NT: oyun zorlayan 4'lü destek."),
                new Madde("Splinter", "Splinter", "Atlayan yeni renk: fit + o renkte kısalık."),
                new Madde("TrialBid", "Deneme teklifi", "Fit sonrası oyun davetini renkle sorar."),
                new Madde("Drury", "Drury", "3. ve 4. el majör açılışına 2♣ ile destekli davet."),
                new Madde("ReverseDrury", "Ters Drury", "Drury'nin ters biçimi (2♦ destekli, 2♣ doğal)."),
                new Madde("ThirdSuitForcing", "Üçüncü renk zorlaması", "Söylenmemiş rengin teklifi zorlayıcı soru olur."),
                new Madde("FourthSuitForcing", "Dördüncü renk zorlaması", "Dördüncü renk oyun zorlayan soru olur."),
                new Madde("TwoWayCheckback", "İki yönlü checkback", "1NT sonrası 2♣/2♦ ile majör sorgusu."),
                new Madde("XYZ", "XYZ", "1♣/1♦–1M–1NT gibi dizilerde yapay devam teklifleri."),
                new Madde("WeakJumpShiftResp", "Zayıf sıçrama cevapları", "1x–2y sıçrama: 6+ renk, 0-6 HP; açıcı geçer."),
                new Madde("Ogust", "Ogust", "Zayıf iki + 2NT: 3♣ min/zayıf, 3♦ min/iyi, 3♥ maks/zayıf, 3♠ maks/iyi, 3NT sağlam."),
                new Madde("InvertedBergenRaises", "Ters Bergen destekleri", "1M–3♣: limit (10-12), 3♦: kurucu (7-9). Bergen destekleri'nin yerini alır."),
                new Madde("OneMinorTwoNTInv", "1m–2NT davet", "1♣/1♦–2NT: 11-12 HP dengeli, majör yok."),
                new Madde("German2CRelays", "Alman 2♣ röleleri", "2♣–2♦–2NT sonrası Stayman (3♣) ve transferler (3♦/3♥)."),
                new Madde("Walsh", "Walsh", "1♣–1♦ ile majör atlanır (4+♦, 6-10 HP); 4-4 majörde 1♥."),
                new Madde("NewMinorForcing", "Yeni minör zorlaması", "1x–1y–1NT sonrası yeni minör: majör desteği sorusu. İki yönlü checkback ile birlikte seçilmez."),
                new Madde("WolffSignoff", "Wolff bırakma", "1x–1y–1NT sonrası 2♣ = 2♦ röle; zayıf minör bırakma."),
                new Madde("Truscott2NT", "Truscott 2NT", "1M–(Dbl)–2NT: 4+ destekli limit raise."),
                new Madde("Artificial2C2D", "Yapay 2♣/2♦ (1M–Dbl)", "1M–(Dbl)–2♣: 3'lü destekli limit, 2♦: oyun zorlayan raise."),
                new Madde("PreemptiveReraises", "Önleyici reraise", "1M–2M, rakip yarışınca 6+ kozla 4M."),
                new Madde("LongShortGameTries", "Uzun & kısa oyun denemeleri", "1M–2M sonrası kısa renk denemesi. Deneme teklifi ile birlikte seçilmez.")),
            new Grup("NT yapıları",
                new Madde("Stayman", "Stayman", "1NT'ye 2♣: 4'lü majör sorusu."),
                new Madde("JacobyTransfer", "Jacoby transfer", "1NT'ye 2♦/2♥: 5'li majöre transfer."),
                new Madde("Smolen", "Smolen", "Stayman'a 2♦ cevabı sonrası 3♥/3♠ ile uzun majörü gösterir (Stayman gerekir)."),
                new Madde("PuppetStayman", "Puppet Stayman", "2NT/3NT açılışında 5'li majör sorusu."),
                new Madde("MinorTransfer", "Minör transferi", "1NT'ye 2♠/2NT ile minöre transfer."),
                new Madde("Lebensohl", "Lebensohl", "Rakip müdahalesinde 2NT ile zayıf/güçlü ayrımı."),
                new Madde("ExtendedStayman", "Extended Stayman", "1NT–2♣ ile zayıf 4-4 majör; 2♦ sonrası 2♥ bırakma."),
                new Madde("Robinson", "Robinson", "1NT–(Dbl): 2♣ Stayman, 2♦/2♥ transfer, RDbl = ♣ transferi. Exit transfer ile birlikte seçilmez."),
                new Madde("ExitTransfer", "Exit transferler", "1NT–(Dbl): zayıf kaçış transferleri (2♣→♦, 2♦→♥, 2♥→♠, 2♠→♣).")),
            new Grup("Slam konvansiyonları",
                new Madde("Blackwood", "Blackwood", "4NT: as sorusu."),
                new Madde("RKCB", "RKCB", "Koz kilit kart sorusu (4NT)."),
                new Madde("RKCB1430", "RKCB 1430", "Cevaplar 1 / 4 · 3 / 0 olarak okunur (RKCB gerekir)."),
                new Madde("Gerber", "Gerber", "NT'ye karşı 4♣ ile as sorusu."),
                new Madde("CueBid", "Cue bid (kontrol gösterme)", "Fit sonrası yan renkte as/kontrol gösterir."),
                new Madde("Serious3NT", "Ciddi 3NT", "Slam ilgisi gösteren 3NT."),
                new Madde("NonSerious3NT", "Ciddi olmayan 3NT", "Slam ilgisi olmadığını gösteren 3NT."),
                new Madde("SpecificKingAsks", "Belirli papaz sorusu", "RKCB cevabından sonra 5 seviyesinde yan renk = o renkte papaz sorusu (RKCB gerekir)."),
                new Madde("VoluntaryFiveMajor", "Gönüllü 5 majör", "Rakip 4M deyince destekli majörde 5M fedakârlığı."),
                new Madde("MatheAskingBids", "Mathe soru teklifleri", "1M–3M sonrası açıcının en ucuz teklifi: kısa renk sorusu."),
                new Madde("ControlAskingBids", "Kontrol soru teklifleri", "1M–3M sonrası 4♣/4♦: o renkte kontrol sorusu.")),
            new Grup("Rekabet ve savunma",
                new Madde("NegativeDouble", "Negatif kontr", "Müdahaleden sonra cevapçının kontrü: kalan majör(ler)."),
                new Madde("TakeoutDouble", "Takeout kontr", "Rakip açılışına kontr: kalan renkleri ister."),
                new Madde("SupportDouble", "Support kontr", "Açıcının kontrü: ortağın majöründe tam 3'lü destek."),
                new Madde("ResponsiveDouble", "Responsive kontr", "Rakipler fit bulunca kontr: kalan iki renk."),
                new Madde("Michaels", "Michaels cue bid", "Rakip rengi: iki renkli el (majör+minör)."),
                new Madde("Unusual2NT", "Unusual 2NT", "Rakip açılışına 2NT: iki en düşük renk."),
                new Madde("Cappelletti", "Cappelletti", "1NT'ye karşı müdahale sistemi (tek seçilir)."),
                new Madde("Dont", "DONT", "1NT'ye karşı müdahale sistemi (tek seçilir)."),
                new Madde("MultiLandy", "Multi-Landy", "1NT'ye karşı müdahale sistemi (tek seçilir)."),
                new Madde("HelloOver1NT", "Hello (1NT'ye karşı)", "Rakip 1NT'sine: 2♣ iki majör, 2♦ uzun majör, 2♥/2♠ majör+minör, 2NT iki minör."),
                new Madde("Sandwich1NT", "Sandwich 1NT", "(1x)–Pas–(1y)–1NT: kalan iki renk."),
                new Madde("Ghestem", "Ghestem", "Cue = iki yüksek, 2NT = iki alçak, 3♣ = alçak+yüksek. Michaels/Unusual 2NT yerine."),
                new Madde("WeakJumpOvercalls", "Zayıf sıçramalı müdahale", "(1x)–2y sıçrama: 6 kart, 5-9 HP."),
                new Madde("MaximalOvercallDbl", "Maximal müdahale kontrü", "1M–2M–(3x alt renk)–Dbl: oyun daveti."),
                new Madde("InvisibleCueBids", "Görünmez cue-bid", "1m–(1M)–2M: minörde oyun zorlayan raise."),
                new Madde("LimitPlusCueBids", "Limit-üstü cue-bid", "1M–(c)–cue: 3+ destekli limit raise ya da üstü."))
        };

        // Aynı anda yalnızca biri açık olabilen alan grupları
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
            new[] { "Ghestem", "Unusual2NT" }
        };

        // alan → onsuz anlamsız olduğu alan
        private static readonly Dictionary<string, string> Gerektirir = new Dictionary<string, string>
        {
            { "Smolen", "Stayman" },
            { "RKCB1430", "RKCB" },
            { "SpecificKingAsks", "RKCB" }
        };

        private readonly OrtaklikAnlasmasi _anlasma;

        /// <summary>Tamam'a basılınca düzenlenmiş anlaşma; İptal'de null.</summary>
        public OrtaklikAnlasmasi Sonuc { get; private set; }

        private readonly Dictionary<string, Madde> _madde = new Dictionary<string, Madde>();
        private readonly HashSet<string> _oynanan = new HashSet<string>();
        private ListBox _lstOynanan, _lstOynanmayan;
        private Label _lblAciklama, _lblDurum;
        private RadioButton _rb1Zayif, _rb1Guclu;

        // Motorun desteklediği aralıklar; diğerleri BB'deki gibi görünür ama seçilemez
        private static readonly string[] Aralik1NT = { "10-12", "12-14", "14-16", "16-18", "11-13", "13-15", "15-17" };
        private static readonly string[] Aralik2NT = { "19-20", "20-21", "21-22", "22-23", "23-24", "19-21", "20-22", "21-23", "22-24" };

        public KonvansiyonSecimForm(OrtaklikAnlasmasi anlasma)
        {
            _anlasma = anlasma.Kopya();
            foreach (var g in Gruplar)
                foreach (var m in g.Maddeler)
                {
                    if (Ozellik(m.Alan) == null) continue;
                    _madde[m.Alan] = m;
                    if ((bool)Ozellik(m.Alan).GetValue(_anlasma, null)) _oynanan.Add(m.Alan);
                }
            Kur();
            ListeleriDoldur();
        }

        private static PropertyInfo Ozellik(string ad)
        {
            return typeof(OrtaklikAnlasmasi).GetProperty(ad);
        }

        private void Kur()
        {
            Text = "Konvansiyonlar";
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(800, 640);
            ClientSize = new Size(860, 660);
            Font = new Font("Segoe UI", 9F);

            var alt = new Panel { Dock = DockStyle.Bottom, Height = 200 };
            var orta = new Panel { Dock = DockStyle.Fill };

            // Listeler ve oklar
            var lblSol = new Label { Text = "Oynanan konvansiyonlar", AutoSize = true, Location = new Point(10, 8) };
            var lblSag = new Label { Text = "Oynanmayan konvansiyonlar", AutoSize = true };
            _lstOynanan = new ListBox { SelectionMode = SelectionMode.MultiExtended, IntegralHeight = false };
            _lstOynanmayan = new ListBox { SelectionMode = SelectionMode.MultiExtended, IntegralHeight = false };
            var btnEkle = new Button { Text = "◄ Ekle", Size = new Size(84, 30) };
            var btnCikar = new Button { Text = "Çıkar ►", Size = new Size(84, 30) };
            btnEkle.Click += (s, e) => Tasi(_lstOynanmayan, true);
            btnCikar.Click += (s, e) => Tasi(_lstOynanan, false);
            _lstOynanan.DoubleClick += (s, e) => Tasi(_lstOynanan, false);
            _lstOynanmayan.DoubleClick += (s, e) => Tasi(_lstOynanmayan, true);
            _lstOynanan.SelectedIndexChanged += (s, e) => AciklamaGoster(_lstOynanan);
            _lstOynanmayan.SelectedIndexChanged += (s, e) => AciklamaGoster(_lstOynanmayan);

            _lblAciklama = new Label { Dock = DockStyle.Bottom, Height = 36, ForeColor = Color.DimGray, Padding = new Padding(10, 4, 10, 0) };
            _lblDurum = new Label { Dock = DockStyle.Bottom, Height = 22, ForeColor = Color.DarkOrange, Padding = new Padding(10, 0, 10, 0) };

            orta.Controls.Add(lblSol);
            orta.Controls.Add(lblSag);
            orta.Controls.Add(_lstOynanan);
            orta.Controls.Add(_lstOynanmayan);
            orta.Controls.Add(btnEkle);
            orta.Controls.Add(btnCikar);
            orta.Controls.Add(_lblDurum);
            orta.Controls.Add(_lblAciklama);
            orta.Resize += (s, e) => ListeleriDuzenle(orta, lblSol, lblSag, btnEkle, btnCikar);
            ListeleriDuzenle(orta, lblSol, lblSag, btnEkle, btnCikar);

            // Alt: 1NT / 2NT aralığı, Signals, Tamam/İptal
            var g1 = new GroupBox { Text = "1NT aralığı", Location = new Point(10, 8), Size = new Size(400, 84) };
            var ip = new ToolTip();
            for (int i = 0; i < Aralik1NT.Length; i++)
            {
                string a = Aralik1NT[i];
                var rb = new RadioButton { Text = a.Replace("-", "–"), AutoSize = true, Location = new Point(14 + (i % 4) * 92, 22 + (i / 4) * 26) };
                if (a == "15-17") { _rb1Guclu = rb; rb.Checked = _anlasma.StrongNT; }
                else if (a == "12-14") { _rb1Zayif = rb; rb.Checked = !_anlasma.StrongNT; }
                else { rb.Enabled = false; ip.SetToolTip(rb, "Motor bu 1NT aralığını henüz desteklemiyor."); }
                g1.Controls.Add(rb);
            }
            var g2 = new GroupBox { Text = "2NT aralığı", Location = new Point(10, 98), Size = new Size(400, 90) };
            for (int i = 0; i < Aralik2NT.Length; i++)
            {
                string a = Aralik2NT[i];
                var rb = new RadioButton { Text = a.Replace("-", "–"), AutoSize = true, Location = new Point(14 + (i % 5) * 76, 20 + (i / 5) * 26) };
                if (a == "20-21") rb.Checked = true;
                else { rb.Enabled = false; ip.SetToolTip(rb, "Motor yalnız 20–21 HP'lik 2NT açılışını destekliyor."); }
                g2.Controls.Add(rb);
            }

            var btnSignals = new Button { Text = "Signals...", Size = new Size(110, 30), Location = new Point(440, 30) };
            btnSignals.Click += (s, e) => SignalsAc();
            var btnTamam = new Button { Text = "Tamam", Size = new Size(96, 30), Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
            var btnIptal = new Button { Text = "İptal", Size = new Size(96, 30), Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
            btnTamam.Location = new Point(alt.Width - 216, alt.Height - 44);
            btnIptal.Location = new Point(alt.Width - 108, alt.Height - 44);
            btnTamam.Click += BtnTamam_Click;
            btnIptal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            alt.Controls.Add(g1);
            alt.Controls.Add(g2);
            alt.Controls.Add(btnSignals);
            alt.Controls.Add(btnTamam);
            alt.Controls.Add(btnIptal);
            AcceptButton = btnTamam;
            CancelButton = btnIptal;

            Controls.Add(orta);
            Controls.Add(alt);
        }

        private static void ListeleriDuzenle(Panel p, Label lblSol, Label lblSag, Button btnEkle, Button btnCikar)
        {
            int boru = 100, kenar = 10, ust = 28;
            int w = (p.ClientSize.Width - boru - 2 * kenar) / 2;
            int h = Math.Max(60, p.ClientSize.Height - ust - 70);
            var sol = p.Controls.OfType<ListBox>().First();
            var sag = p.Controls.OfType<ListBox>().Last();
            sol.SetBounds(kenar, ust, w, h);
            sag.SetBounds(kenar + w + boru, ust, w, h);
            lblSag.Location = new Point(kenar + w + boru, 8);
            btnEkle.Location = new Point(kenar + w + (boru - btnEkle.Width) / 2, ust + h / 2 - 40);
            btnCikar.Location = new Point(kenar + w + (boru - btnCikar.Width) / 2, ust + h / 2 + 4);
        }

        private void ListeleriDoldur()
        {
            string secSol = _lstOynanan.SelectedItem as string, secSag = _lstOynanmayan.SelectedItem as string;
            _lstOynanan.BeginUpdate(); _lstOynanmayan.BeginUpdate();
            _lstOynanan.Items.Clear(); _lstOynanmayan.Items.Clear();
            foreach (var m in _madde.Values.OrderBy(x => x.Baslik, StringComparer.CurrentCultureIgnoreCase))
                (_oynanan.Contains(m.Alan) ? _lstOynanan : _lstOynanmayan).Items.Add(m.Baslik);
            _lstOynanan.EndUpdate(); _lstOynanmayan.EndUpdate();
        }

        private Madde BaslikToMadde(string baslik)
        {
            return _madde.Values.FirstOrDefault(m => m.Baslik == baslik);
        }

        private void AciklamaGoster(ListBox kutu)
        {
            var m = BaslikToMadde(kutu.SelectedItem as string);
            _lblAciklama.Text = m == null ? "" : m.Baslik + " — " + m.Aciklama;
        }

        private void Tasi(ListBox kaynak, bool ekle)
        {
            var secilen = kaynak.SelectedItems.Cast<string>().Select(BaslikToMadde).Where(m => m != null).ToList();
            var notlar = new List<string>();
            foreach (var m in secilen)
            {
                if (ekle)
                {
                    _oynanan.Add(m.Alan);
                    foreach (var grup in Dislayan)
                    {
                        if (!grup.Contains(m.Alan)) continue;
                        foreach (var diger in grup)
                            if (diger != m.Alan && _oynanan.Remove(diger) && _madde.ContainsKey(diger))
                                notlar.Add(_madde[diger].Baslik + " çıkarıldı (" + m.Baslik + " ile birlikte oynanmaz)");
                    }
                    string gerek;
                    if (Gerektirir.TryGetValue(m.Alan, out gerek) && _madde.ContainsKey(gerek) && _oynanan.Add(gerek))
                        notlar.Add(_madde[gerek].Baslik + " da eklendi (" + m.Baslik + " için gerekli)");
                }
                else
                {
                    _oynanan.Remove(m.Alan);
                    foreach (var kv in Gerektirir)
                        if (kv.Value == m.Alan && _oynanan.Remove(kv.Key) && _madde.ContainsKey(kv.Key))
                            notlar.Add(_madde[kv.Key].Baslik + " da çıkarıldı (" + m.Baslik + " gerekiyor)");
                }
            }
            _lblDurum.Text = string.Join("; ", notlar);
            ListeleriDoldur();
        }

        private void SignalsAc()
        {
            using (var f = new Form())
            {
                f.Text = "İşaretleşme";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowInTaskbar = false;
                f.MinimizeBox = false; f.MaximizeBox = false;
                f.ClientSize = new Size(300, 130);
                var std = new RadioButton { Text = "Standart (yüksek: çift, düşük: tek)", AutoSize = true, Location = new Point(14, 14), Checked = !_anlasma.UDCA };
                var ud = new RadioButton { Text = "UDCA (yukarı-aşağı çağrı/atma)", AutoSize = true, Location = new Point(14, 42), Checked = _anlasma.UDCA };
                var ok = new Button { Text = "Tamam", DialogResult = DialogResult.OK, Location = new Point(114, 88), Size = new Size(80, 28) };
                var iptal = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Location = new Point(204, 88), Size = new Size(80, 28) };
                f.Controls.AddRange(new Control[] { std, ud, ok, iptal });
                f.AcceptButton = ok; f.CancelButton = iptal;
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    _anlasma.UDCA = ud.Checked;
                    _anlasma.StandartSignal = std.Checked;
                }
            }
        }

        private void BtnTamam_Click(object sender, EventArgs e)
        {
            foreach (var kv in _madde)
                Ozellik(kv.Key).SetValue(_anlasma, _oynanan.Contains(kv.Key), null);
            _anlasma.StrongNT = _rb1Guclu.Checked;
            Sonuc = _anlasma;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
