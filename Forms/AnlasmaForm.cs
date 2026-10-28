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
    /// Ortaklık anlaşması ekranı: hangi açılış sistemi ve konvansiyonların kullanılacağını seçer.
    /// Seçim kaydedilir; sonraki ihaleler (yapay zekâ ortak ve rakipler dahil) bu anlaşmayla oynanır.
    /// </summary>
    public sealed class AnlasmaForm : Form
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
                new Madde("StrongNT", "Güçlü 1NT (15–17 HP)", "Kapalıysa zayıf 1NT (12–14 HP) oynanır."),
                new Madde("IkiliSinekGuclu", "2♣ güçlü açılış", "22+ HP ya da oyun zorlayan eller."),
                new Madde("ZayifIki", "Zayıf iki açılışları", "2♦/2♥/2♠ ile 6 kartlık zayıf el gösterilir."),
                new Madde("Multi2D", "Multi 2♦", "Zayıf iki majörünü 2♦ altında toplar; Zayıf İki'nin yerini alır."),
                new Madde("Preempt", "Önleyici açılışlar", "3. ve 4. seviyede 7+ kartlık zayıf eller."),
                new Madde("Namyats", "Namyats", "4♣/4♦ açılışları güçlü 4♥/4♠ önleyicilerini gösterir."),
                new Madde("OneClub_AnswerSistemi", "1♣ cevap sistemi (Polish hibrit)", "1♣ açılışına özel cevap tablosu."),
                new Madde("Reverse", "Reverse gelişimi", "Açıcının ikinci rengi daha yüksekse güçlü el (16+) gösterir."),
                new Madde("ForcingNT", "Zorlayıcı 1NT", "1M açılışına 1NT cevabı bir tur zorlayıcıdır.")),
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
                new Madde("XYZ", "XYZ", "1♣/1♦–1M–1NT gibi dizilerde yapay devam teklifleri.")),
            new Grup("NT yapıları",
                new Madde("Stayman", "Stayman", "1NT'ye 2♣: 4'lü majör sorusu."),
                new Madde("JacobyTransfer", "Jacoby transfer", "1NT'ye 2♦/2♥: 5'li majöre transfer."),
                new Madde("Smolen", "Smolen", "Stayman'a 2♦ cevabı sonrası 3♥/3♠ ile uzun majörü gösterir (Stayman gerekir)."),
                new Madde("PuppetStayman", "Puppet Stayman", "2NT/3NT açılışında 5'li majör sorusu."),
                new Madde("MinorTransfer", "Minör transferi", "1NT'ye 2♠/2NT ile minöre transfer."),
                new Madde("Lebensohl", "Lebensohl", "Rakip müdahalesinde 2NT ile zayıf/güçlü ayrımı.")),
            new Grup("Slam konvansiyonları",
                new Madde("Blackwood", "Blackwood", "4NT: as sorusu."),
                new Madde("RKCB", "RKCB", "Koz kilit kart sorusu (4NT)."),
                new Madde("RKCB1430", "RKCB 1430", "Cevaplar 1 / 4 · 3 / 0 olarak okunur (RKCB gerekir)."),
                new Madde("Gerber", "Gerber", "NT'ye karşı 4♣ ile as sorusu."),
                new Madde("CueBid", "Cue bid (kontrol gösterme)", "Fit sonrası yan renkte as/kontrol gösterir."),
                new Madde("Serious3NT", "Ciddi 3NT", "Slam ilgisi gösteren 3NT."),
                new Madde("NonSerious3NT", "Ciddi olmayan 3NT", "Slam ilgisi olmadığını gösteren 3NT.")),
            new Grup("Rekabet ve savunma",
                new Madde("NegativeDouble", "Negatif kontr", "Müdahaleden sonra cevapçının kontrü: kalan majör(ler)."),
                new Madde("TakeoutDouble", "Takeout kontr", "Rakip açılışına kontr: kalan renkleri ister."),
                new Madde("SupportDouble", "Support kontr", "Açıcının kontrü: ortağın majöründe tam 3'lü destek."),
                new Madde("ResponsiveDouble", "Responsive kontr", "Rakipler fit bulunca kontr: kalan iki renk."),
                new Madde("Michaels", "Michaels cue bid", "Rakip rengi: iki renkli el (majör+minör)."),
                new Madde("Unusual2NT", "Unusual 2NT", "Rakip açılışına 2NT: iki en düşük renk."),
                new Madde("Cappelletti", "Cappelletti", "1NT'ye karşı müdahale sistemi (tek seçilir)."),
                new Madde("Dont", "DONT", "1NT'ye karşı müdahale sistemi (tek seçilir)."),
                new Madde("MultiLandy", "Multi-Landy", "1NT'ye karşı müdahale sistemi (tek seçilir)."))
        };

        // Aynı anda yalnızca biri açık olabilen alan grupları
        private static readonly string[][] Dislayan =
        {
            new[] { "Cappelletti", "Dont", "MultiLandy" },
            new[] { "Serious3NT", "NonSerious3NT" },
            new[] { "Drury", "ReverseDrury" },
            new[] { "ZayifIki", "Multi2D" }
        };

        // alan → onsuz anlamsız olduğu alan
        private static readonly Dictionary<string, string> Gerektirir = new Dictionary<string, string>
        {
            { "Smolen", "Stayman" },
            { "RKCB1430", "RKCB" }
        };

        private readonly OrtaklikAnlasmasi _anlasma;
        private readonly Dictionary<string, CheckBox> _kutular = new Dictionary<string, CheckBox>();
        private CheckBox _chkOgrenme;
        private bool _guncelleniyor;

        public AnlasmaForm()
        {
            _anlasma = AnlasmaYonetici.Mevcut();
            Kur();
            KutulariDoldur();
        }

        private static PropertyInfo Ozellik(string ad)
        {
            return typeof(OrtaklikAnlasmasi).GetProperty(ad);
        }

        private void Kur()
        {
            Text = "Ortaklık Anlaşması";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(700, 520);
            ClientSize = new Size(820, 700);
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.FromArgb(244, 246, 244);

            // Üst: açıklama + hazır ayarlar
            var ust = new Panel { Dock = DockStyle.Top, Height = 74, Padding = new Padding(12, 8, 12, 4) };
            var baslik = new Label
            {
                Text = "Hangi sistem ve konvansiyonları oynuyorsunuz?",
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 8)
            };
            var alt = new Label
            {
                Text = "Yapay zekâ ortağınız ve rakipler aynı anlaşmayı kullanır. Varsayılan dışındaki bileşimler daha az denenmiştir.",
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(12, 30)
            };
            var btnStandart = new Button { Text = "Standart (önerilen)", Size = new Size(150, 26), Location = new Point(12, 46) };
            btnStandart.Click += (s, e) => HazirAyar(OrtaklikAnlasmasi.Varsayilan());
            var btnBasit = new Button { Text = "Başlangıç (basit)", Size = new Size(130, 26), Location = new Point(170, 46) };
            btnBasit.Click += (s, e) => HazirAyar(OrtaklikAnlasmasi.Basit());
            ust.Controls.Add(baslik);
            ust.Controls.Add(alt);
            ust.Controls.Add(btnStandart);
            ust.Controls.Add(btnBasit);

            // Alt: öğrenme modu + düğmeler
            var alta = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(12, 8, 12, 8) };
            _chkOgrenme = new CheckBox
            {
                Text = "Öğrenme modu (teklifleri motor önerisiyle karşılaştır, anlamlarını açıkla)",
                AutoSize = true,
                Location = new Point(12, 17),
                Checked = AnlasmaYonetici.OgrenmeModuAcik
            };
            var btnKaydet = new Button { Text = "Kaydet", Size = new Size(96, 30), Anchor = AnchorStyles.Right | AnchorStyles.Top };
            var btnVazgec = new Button { Text = "Vazgeç", Size = new Size(96, 30), Anchor = AnchorStyles.Right | AnchorStyles.Top };
            btnKaydet.Location = new Point(alta.Width - 2 * 96 - 24, 12);
            btnVazgec.Location = new Point(alta.Width - 96 - 12, 12);
            btnKaydet.Click += BtnKaydet_Click;
            btnVazgec.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            alta.Controls.Add(_chkOgrenme);
            alta.Controls.Add(btnKaydet);
            alta.Controls.Add(btnVazgec);
            AcceptButton = btnKaydet;
            CancelButton = btnVazgec;

            // Orta: kaydırmalı gruplar
            var orta = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            var ipucu = new ToolTip();
            int y = 8;
            foreach (var g in Gruplar)
            {
                var gb = new GroupBox
                {
                    Text = g.Ad,
                    Location = new Point(10, y),
                    Width = 770,
                    Font = new Font(Font, FontStyle.Bold),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left
                };
                int iy = 24;
                foreach (var m in g.Maddeler)
                {
                    var chk = new CheckBox
                    {
                        Text = m.Baslik,
                        Font = new Font(Font.FontFamily, 9.5F, FontStyle.Bold),
                        AutoSize = true,
                        Location = new Point(14, iy),
                        Tag = m.Alan
                    };
                    chk.CheckedChanged += Kutu_CheckedChanged;
                    var acik = new Label
                    {
                        Text = m.Aciklama,
                        Font = new Font(Font.FontFamily, 9F, FontStyle.Regular),
                        ForeColor = Color.DimGray,
                        AutoSize = false,
                        Size = new Size(720, 18),
                        Location = new Point(34, iy + 20)
                    };
                    ipucu.SetToolTip(chk, m.Aciklama);
                    gb.Controls.Add(chk);
                    gb.Controls.Add(acik);
                    _kutular[m.Alan] = chk;
                    iy += 46;
                }
                gb.Height = iy + 6;
                orta.Controls.Add(gb);
                y += gb.Height + 8;
            }

            Controls.Add(orta);
            Controls.Add(alta);
            Controls.Add(ust);
        }

        private void KutulariDoldur()
        {
            _guncelleniyor = true;
            try
            {
                foreach (var kv in _kutular)
                {
                    var p = Ozellik(kv.Key);
                    kv.Value.Checked = p != null && (bool)p.GetValue(_anlasma, null);
                }
            }
            finally { _guncelleniyor = false; }
            BagimliligiGuncelle();
        }

        private void HazirAyar(OrtaklikAnlasmasi hazir)
        {
            foreach (var kv in _kutular)
            {
                var p = Ozellik(kv.Key);
                if (p != null) p.SetValue(_anlasma, p.GetValue(hazir, null), null);
            }
            KutulariDoldur();
        }

        private void Kutu_CheckedChanged(object sender, EventArgs e)
        {
            if (_guncelleniyor) return;
            var chk = (CheckBox)sender;
            string alan = (string)chk.Tag;

            _guncelleniyor = true;
            try
            {
                if (chk.Checked)
                {
                    // Birbirini dışlayan seçeneklerden yalnızca biri açık kalır
                    foreach (var grup in Dislayan)
                    {
                        if (!grup.Contains(alan)) continue;
                        foreach (var diger in grup)
                            if (diger != alan && _kutular.ContainsKey(diger)) _kutular[diger].Checked = false;
                    }
                    // Gerekli konvansiyon kapalıysa onu da aç
                    string gerek;
                    if (Gerektirir.TryGetValue(alan, out gerek) && _kutular.ContainsKey(gerek))
                        _kutular[gerek].Checked = true;
                }
                else
                {
                    // Bu konvansiyona bağlı olanları kapat
                    foreach (var kv in Gerektirir)
                        if (kv.Value == alan && _kutular.ContainsKey(kv.Key)) _kutular[kv.Key].Checked = false;
                }
            }
            finally { _guncelleniyor = false; }
            BagimliligiGuncelle();
        }

        private void BagimliligiGuncelle()
        {
            foreach (var kv in Gerektirir)
            {
                if (!_kutular.ContainsKey(kv.Key) || !_kutular.ContainsKey(kv.Value)) continue;
                _kutular[kv.Key].Enabled = _kutular[kv.Value].Checked;
            }
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            foreach (var kv in _kutular)
            {
                var p = Ozellik(kv.Key);
                if (p != null) p.SetValue(_anlasma, kv.Value.Checked, null);
            }

            if (!AnlasmaYonetici.Kaydet(_anlasma))
            {
                MessageBox.Show(this, "Anlaşma kaydedilemedi. Bu oturumda geçerli olacak.", "Ortaklık Anlaşması",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            AnlasmaYonetici.OgrenmeModuAcik = _chkOgrenme.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
