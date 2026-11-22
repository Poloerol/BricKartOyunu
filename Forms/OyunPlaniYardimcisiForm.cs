using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Plan;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Yardım > Oyun Planı Yardımcısı. Deklaran tarafındaysanız: önce kendi sayımınızı (kesin löve, ana el, ilk renk) girersiniz,
    /// program kontrol edip farkı açıklar. Savunmadaysanız: açılış atağını önce siz seçersiniz, program kurallarla ve motorla karşılaştırır;
    /// atak sonrası dummy analizi, onbir kuralı ve ortağın sinyallerinin yorumu gösterilir.
    /// "Kart Takibi" sekmesi oynanan/kalan kartları, "Rakipler" sekmesi ihaleden çıkarımları gösterir.
    /// </summary>
    public sealed class OyunPlaniYardimcisiForm : Form
    {
        private readonly Func<OyunPlaniDurumu> _durumAl;
        private OyunPlaniDurumu _durum;
        private PlanSonucu _sonuc;
        private SavunmaSonucu _savunma;
        private bool _hesaplaniyor;
        private bool _cevapGirildi, _gosterildi;
        private string _modAnahtari = "";

        private readonly Label _ozet, _bas, _l1, _l2, _l3;
        private readonly TabControl _sekmeler;
        private readonly TabPage _tAtak;
        private readonly NumericUpDown _kesinSayi;
        private readonly ComboBox _anaEl, _ilkRenk;
        private readonly Button _kontrol, _goster, _yenile;
        private readonly RichTextBox _plan, _rakipler;
        private readonly ListView _takip, _atakListe;
        private readonly Panel _ust;

        public OyunPlaniYardimcisiForm(Func<OyunPlaniDurumu> durumAl)
        {
            _durumAl = durumAl;
            Text = "Oyun Planı Yardımcısı";
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10f);
            Size = new Size(900, 740);
            MinimumSize = new Size(720, 560);
            ShowInTaskbar = false;

            _ozet = new Label { Dock = DockStyle.Top, Height = 54, Padding = new Padding(10, 8, 10, 4), Font = new Font("Segoe UI", 10.5f, FontStyle.Bold) };

            _ust = new Panel { Dock = DockStyle.Top, Height = 118, Padding = new Padding(10, 6, 10, 6), BackColor = Color.FromArgb(245, 247, 250) };
            _bas = new Label { Text = "", AutoSize = true, Left = 10, Top = 6, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            _l1 = new Label { Text = "Kesin löve sayınız:", AutoSize = true, Left = 10, Top = 36 };
            _kesinSayi = new NumericUpDown { Left = 150, Top = 32, Width = 55, Minimum = 0, Maximum = 13 };
            _l2 = new Label { Text = "Ana el:", AutoSize = true, Left = 230, Top = 36 };
            _anaEl = new ComboBox { Left = 300, Top = 32, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            _l3 = new Label { Text = "Önce hangi renk?", AutoSize = true, Left = 470, Top = 36 };
            _ilkRenk = new ComboBox { Left = 590, Top = 32, Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            _kontrol = new Button { Text = "Kontrol et", Left = 10, Top = 72, Width = 120, Height = 32 };
            _goster = new Button { Text = "Cevabı doğrudan göster", Left = 140, Top = 72, Width = 190, Height = 32 };
            _yenile = new Button { Text = "Yenile (durum değişti)", Left = 340, Top = 72, Width = 190, Height = 32 };
            _ust.Controls.AddRange(new Control[] { _bas, _l1, _kesinSayi, _l2, _anaEl, _l3, _ilkRenk, _kontrol, _goster, _yenile });

            _plan = Metin();
            _rakipler = Metin();
            _takip = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, Font = new Font("Segoe UI", 11f) };
            _takip.Columns.Add("Renk", 60); _takip.Columns.Add("Bizde (iki el)", 170); _takip.Columns.Add("Oynanan", 170);
            _takip.Columns.Add("Dışarıda", 220); _takip.Columns.Add("Adet", 55); _takip.Columns.Add("En büyük", 80);

            _atakListe = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, Font = new Font("Segoe UI", 10.5f) };
            _atakListe.Columns.Add("Renk", 55); _atakListe.Columns.Add("Elinizde", 130); _atakListe.Columns.Add("Atak", 60);
            _atakListe.Columns.Add("Kural", 150); _atakListe.Columns.Add("Ortağa mesaj", 260); _atakListe.Columns.Add("Puan", 50); _atakListe.Columns.Add("Nedenler", 380);

            _sekmeler = new TabControl { Dock = DockStyle.Fill };
            var t1 = new TabPage("Plan"); t1.Controls.Add(_plan);
            var t2 = new TabPage("Kart Takibi"); t2.Controls.Add(_takip);
            var t3 = new TabPage("Rakipler (ihale)"); t3.Controls.Add(_rakipler);
            _tAtak = new TabPage("Atak seçenekleri"); _tAtak.Controls.Add(_atakListe);
            _sekmeler.TabPages.AddRange(new[] { t1, t2, t3 });

            Controls.Add(_sekmeler);
            Controls.Add(_ust);
            Controls.Add(_ozet);

            _anaEl.SelectedIndexChanged += (s, e) => { if (_durum != null && _durum.Mod == 2 && _savunma != null && _savunma.AtakKarari) KartlariDoldur(); };
            _kontrol.Click += (s, e) => { _cevapGirildi = true; _gosterildi = true; Hesapla(); };
            _goster.Click += (s, e) => { _cevapGirildi = false; _gosterildi = true; Hesapla(); };
            _yenile.Click += (s, e) => Hesapla();
            Shown += (s, e) => Hesapla();
        }

        private static RichTextBox Metin()
        {
            return new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10.5f), BackColor = Color.White };
        }

        private async void Hesapla()
        {
            if (_hesaplaniyor) return;
            _hesaplaniyor = true;
            Cursor = Cursors.WaitCursor;
            try
            {
                _durum = _durumAl();
                if (_durum.Mod == 0)
                {
                    _ozet.Text = "Henüz oyun planı yapılamaz.";
                    _plan.Clear(); Yaz(_plan, _durum.Mesaj);
                    return;
                }
                _ozet.Text = "Hesaplanıyor…";
                if (_durum.Mod == 1)
                {
                    var girdi = _durum.Deklaran;
                    _sonuc = await Task.Run(() => OyunPlanAnalizi.Coz(girdi));
                    ModuKur(1, "dek", girdi);
                    DoldurDeklaran(girdi);
                }
                else
                {
                    var girdi = _durum.Savunma;
                    _savunma = await Task.Run(() => SavunmaPlanAnalizi.Coz(girdi));
                    string anahtar = "sav" + (_savunma.AtakKarari ? "A" : "O");
                    ModuKur(2, anahtar, null);
                    DoldurSavunma(girdi);
                }
            }
            catch (Exception ex)
            {
                _plan.Text = "Hesap sırasında hata: " + ex.Message;
            }
            finally
            {
                _hesaplaniyor = false;
                Cursor = Cursors.Default;
            }
        }

        // ── Arayüzü role göre kur ──
        private void ModuKur(int mod, string anahtar, PlanGirdisi dg)
        {
            if (anahtar == _modAnahtari) return;
            _modAnahtari = anahtar;
            _gosterildi = mod == 2 && anahtar == "savO";    // atak sonrası savunmada cevap istenmez
            _cevapGirildi = false;
            bool girdili = anahtar == "dek" || anahtar == "savA";
            _l1.Visible = _kesinSayi.Visible = anahtar == "dek";
            _l2.Visible = _anaEl.Visible = _l3.Visible = _ilkRenk.Visible = girdili;
            _kontrol.Visible = _goster.Visible = girdili;
            _ust.Height = girdili ? 118 : 70;
            _yenile.Top = girdili ? 72 : 32; _yenile.Left = girdili ? 340 : 10;

            if (anahtar == "dek")
            {
                _bas.Text = "1) Önce siz sayın:";
                _l2.Text = "Ana el:"; _l3.Text = "Önce hangi renk?";
                _l2.Left = 230; _anaEl.Left = 300; _l3.Left = 470; _ilkRenk.Left = 590;
                _anaEl.Items.Clear(); _ilkRenk.Items.Clear();
                _ilkRenk.Items.AddRange(new object[] { "(seçin)", "♠", "♥", "♦", "♣", "Koz çek", "Fark etmez" });
                _ilkRenk.SelectedIndex = 0;
                _anaEl.Items.AddRange(new object[] { "(seçin)", "Deklaran", "Dummy" });
                _anaEl.SelectedIndex = 0;
                _plan.Clear(); Yaz(_plan, "Cevabınızı girin ve \"Kontrol et\"e basın. İsterseniz \"Cevabı doğrudan göster\"i seçebilirsiniz.");
                if (_sekmeler.TabPages.Contains(_tAtak)) _sekmeler.TabPages.Remove(_tAtak);
            }
            else if (anahtar == "savA")
            {
                _bas.Text = "1) Önce siz seçin: açılış atağı";
                _l2.Text = "Atak rengi:"; _l3.Text = "Atak kartı:";
                _l2.Left = 10; _anaEl.Left = 100; _l3.Left = 280; _ilkRenk.Left = 380;
                _anaEl.Items.Clear(); _ilkRenk.Items.Clear();
                _anaEl.Items.Add("(seçin)");
                foreach (var r in new[] { "Maça", "Kupa", "Karo", "Sinek" })
                    if (_durum.Savunma.Elim.Any(c => c.Suit == r)) _anaEl.Items.Add(OyunPlanAnalizi.Sembol(r));
                _anaEl.SelectedIndex = 0;
                _ilkRenk.Items.Add("(önce renk)"); _ilkRenk.SelectedIndex = 0;
                _plan.Clear(); Yaz(_plan, "Atak rengini ve kartını seçin, sonra \"Kontrol et\"e basın. İsterseniz \"Cevabı doğrudan göster\"i seçebilirsiniz.");
                if (!_sekmeler.TabPages.Contains(_tAtak)) _sekmeler.TabPages.Insert(1, _tAtak);
            }
            else
            {
                _bas.Text = "";
                if (_sekmeler.TabPages.Contains(_tAtak)) _sekmeler.TabPages.Remove(_tAtak);
            }
        }

        private void KartlariDoldur()
        {
            _ilkRenk.Items.Clear();
            string sem = _anaEl.SelectedItem as string;
            if (sem == null || sem == "(seçin)") { _ilkRenk.Items.Add("(önce renk)"); _ilkRenk.SelectedIndex = 0; return; }
            string renk = sem == "♠" ? "Maça" : sem == "♥" ? "Kupa" : sem == "♦" ? "Karo" : "Sinek";
            _ilkRenk.Items.Add("(seçin)");
            foreach (var v in _durum.Savunma.Elim.Where(c => c.Suit == renk).Select(c => c.Value).OrderByDescending(v => v))
                _ilkRenk.Items.Add(sem + OyunPlanAnalizi.Sira(v));
            _ilkRenk.SelectedIndex = 0;
        }

        private void TakipDoldur(List<TakipSatiri> takip)
        {
            _takip.Items.Clear();
            foreach (var t in takip)
            {
                var it = new ListViewItem(t.Sembol);
                it.SubItems.Add(t.Bizde); it.SubItems.Add(t.Oynanan); it.SubItems.Add(t.Disarida);
                it.SubItems.Add(t.DisariAdet.ToString()); it.SubItems.Add(t.EnBuyuk);
                it.ForeColor = (t.Renk == "Kupa" || t.Renk == "Karo") ? Color.Firebrick : Color.Black;
                _takip.Items.Add(it);
            }
        }

        // ═══════════════ DEKLARAN ═══════════════
        private void DoldurDeklaran(PlanGirdisi g)
        {
            var s = _sonuc;
            if (s == null || !s.Gecerli) { _ozet.Text = "Plan hazır değil"; _plan.Text = s == null ? "" : s.Hata; return; }
            _ozet.Text = s.Ozet;

            string dekAd = g.Ad(g.Deklaran) + " (deklaran)", dumAd = g.Ad(g.Dummy) + " (dummy)";
            int secili = _anaEl.SelectedIndex;
            _anaEl.Items.Clear(); _anaEl.Items.AddRange(new object[] { "(seçin)", dekAd, dumAd });
            _anaEl.SelectedIndex = secili < 0 ? 0 : secili;

            TakipDoldur(s.Takip);

            _rakipler.Clear();
            Yaz(_rakipler, "İhaleden ve oyundan çıkarımlar", true);
            foreach (var l in s.Rakipler) Yaz(_rakipler, "• " + l);
            Yaz(_rakipler, "");
            Yaz(_rakipler, "Dışarıda kalan kartlar", true);
            foreach (var l in s.KalanKartlar) Yaz(_rakipler, "• " + l);

            _plan.Clear();
            if (!_gosterildi)
            {
                Yaz(_plan, "Cevabınızı girin ve \"Kontrol et\"e basın.", true);
                return;
            }

            if (_cevapGirildi)
            {
                Yaz(_plan, "Sizin cevabınız ile programın sayımı", true);
                int kesin = (int)_kesinSayi.Value;
                string k = kesin == s.KesinToplam ? "✔ Kesin löve sayınız doğru: " + kesin
                         : "✘ Kesin löve: siz " + kesin + " dediniz, program " + s.KesinToplam + " buldu. " +
                           (kesin > s.KesinToplam ? "Fazla saydınız: üst kartın bağlantısı (ara kart / giriş) ya da rakibin onörü sizi yanıltmış olabilir."
                                                  : "Eksik saydınız: aşağıdaki tabloda hangi renkten daha çok löve çıktığına bakın.");
                Yaz(_plan, k);
                if (_anaEl.SelectedIndex > 0)
                {
                    bool dogru = (_anaEl.SelectedIndex == 1 && s.AnaEl == g.Deklaran) || (_anaEl.SelectedIndex == 2 && s.AnaEl == g.Dummy);
                    Yaz(_plan, (dogru ? "✔ Ana el: " : "✘ Ana el: programa göre ") + g.Ad(s.AnaEl) + ". " + s.AnaElNeden);
                }
                if (_ilkRenk.SelectedIndex > 0)
                {
                    string sec = (string)_ilkRenk.SelectedItem;
                    string onerilen = s.IlkRenk == "" ? "" : OyunPlanAnalizi.Sembol(s.IlkRenk);
                    if (sec == "Koz çek")
                    {
                        bool kozlu = g.Koz != null;
                        bool cek = s.KozKarari.Any(x => x.Contains("Önce kozları çekin"));
                        Yaz(_plan, (kozlu && cek ? "✔ " : "✘ ") + "Koz çekme kararı: " +
                            (kozlu ? (cek ? "program da önce çekmeyi öneriyor." : "program önce çekmemenizi öneriyor (aşağıya bakın).") : "kontrat sanzatu, çekilecek koz yok."));
                    }
                    else if (sec == "Fark etmez") Yaz(_plan, "• Başlangıç rengi: program kaynak olarak " + (onerilen == "" ? "yeni renk önermiyor." : onerilen + " rengini öneriyor."));
                    else Yaz(_plan, (onerilen == sec ? "✔ " : "✘ ") + "Başlangıç: siz " + sec + " dediniz, program " + (onerilen == "" ? "özel bir renk önermiyor." : onerilen + " öneriyor."));
                }
                Yaz(_plan, "");
            }

            Yaz(_plan, "Renk renk sayım", true);
            foreach (var r in s.Renkler)
            {
                string t = r.Sembol + "  elde " + r.DeklaranKartlari + "  |  dummy " + r.DummyKartlari + "  →  kesin " + r.Kesin;
                if (r.Hesaplandi) t += ", beklenen " + r.Beklenen.ToString("0.0");
                if (r.Kesme > 0) t += ", kesme fırsatı " + r.Kesme;
                if (r.Not != "") t += "  (" + r.Not + ")";
                Yaz(_plan, t);
            }
            Yaz(_plan, "Toplam: kesin " + s.KesinToplam + ", beklenen " + s.BeklenenToplam.ToString("0.0") + " (hedefe " + s.Kalan + " löve gerek).");
            Yaz(_plan, "");
            Yaz(_plan, "Ana el", true);
            Yaz(_plan, g.Ad(s.AnaEl) + ": " + s.AnaElNeden);
            Yaz(_plan, "");
            Yaz(_plan, "Giriş kartları", true);
            foreach (var l in s.Girisler) Yaz(_plan, "• " + l);
            Yaz(_plan, "");
            Yaz(_plan, "Plan", true);
            foreach (var l in s.Plan) Yaz(_plan, l);
            if (s.KozKarari.Count > 0)
            {
                Yaz(_plan, "");
                Yaz(_plan, "Koz kararı: önce kozları mı çekmeli?", true);
                foreach (var l in s.KozKarari) Yaz(_plan, "• " + l);
            }
            Yaz(_plan, "");
            Yaz(_plan, "Not: Beklenen löveler, rakiplerin kalan kartlarının tüm dağılımlarına göre hesaplanır; giriş sorunları ve rakip kesmesi hesaba katılmaz. " +
                       "Bu bir öğretme aracıdır; son kararı sizin okumanız verir.");
        }

        // ═══════════════ SAVUNMA ═══════════════
        private void DoldurSavunma(SavunmaGirdisi g)
        {
            var s = _savunma;
            if (s == null || !s.Gecerli) { _ozet.Text = "Savunma planı hazır değil"; _plan.Text = s == null ? "" : s.Hata; return; }
            _ozet.Text = s.Ozet;
            TakipDoldur(s.Takip);

            _rakipler.Clear();
            Yaz(_rakipler, "İhale ve güç bilgisi", true);
            foreach (var l in s.Rakipler) Yaz(_rakipler, "• " + l);

            _plan.Clear();
            _atakListe.Items.Clear();

            if (s.AtakKarari)
            {
                if (!_gosterildi)
                {
                    Yaz(_plan, "Atak kararı sizde. Önce rengi ve kartı siz seçin, sonra \"Kontrol et\"e basın.", true);
                    return;
                }
                if (_cevapGirildi && _anaEl.SelectedIndex > 0 && _ilkRenk.SelectedIndex > 0)
                {
                    string secim = (string)_ilkRenk.SelectedItem;
                    Yaz(_plan, "Sizin atağınız: " + secim, true);
                    bool kuralDogru = secim == s.Onerilen;
                    Yaz(_plan, (kuralDogru ? "✔ " : "✘ ") + "Kurallara göre önerilen atak: " + s.Onerilen + (kuralDogru ? " — seçiminiz aynı." : "."));
                    if (s.MotorKart != "")
                        Yaz(_plan, (secim == s.MotorKart ? "✔ " : "✘ ") + "Motorun seçimi: " + s.MotorKart + (secim == s.MotorKart ? " — seçiminiz aynı." : "."));
                    var sec = s.Atak.FirstOrDefault(x => x.KartYazi == secim);
                    if (sec == null)
                    {
                        var rk = s.Atak.FirstOrDefault(x => x.Sembol == (string)_anaEl.SelectedItem);
                        if (rk != null)
                            Yaz(_plan, "Seçtiğiniz rengin standart atak kartı " + rk.KartYazi + " (" + rk.Kural + "). " + rk.Notlar +
                                       (secim != rk.KartYazi ? " Standart dışı kart seçtiniz: ortak bunu başka şekilde yorumlayabilir." : ""));
                    }
                    else Yaz(_plan, "Bu renk için sıra: " + (s.Atak.IndexOf(sec) + 1) + ". seçenek (" + sec.Notlar + ").");
                    Yaz(_plan, "");
                }
                foreach (var a in s.Atak)
                {
                    var it = new ListViewItem(a.Sembol);
                    it.SubItems.Add(a.Kartlar); it.SubItems.Add(a.KartYazi); it.SubItems.Add(a.Kural);
                    it.SubItems.Add(a.Gosterir); it.SubItems.Add(a.Puan.ToString("0.0")); it.SubItems.Add(a.Notlar);
                    it.ForeColor = (a.Renk == "Kupa" || a.Renk == "Karo") ? Color.Firebrick : Color.Black;
                    _atakListe.Items.Add(it);
                }
                Yaz(_plan, "Atak ilkeleri", true);
                foreach (var l in s.Ilkeler) Yaz(_plan, l);
                Yaz(_plan, "");
                Yaz(_plan, "Öneri", true);
                foreach (var l in s.Plan) Yaz(_plan, "• " + l);
                Yaz(_plan, "");
                Yaz(_plan, "Sinyal / atak stili", true);
                foreach (var l in s.Sinyaller) Yaz(_plan, "• " + l);
                Yaz(_plan, "");
                Yaz(_plan, "Not: Puanlar açıklanabilir sezgilerdir (ortağın rengi, sıralı onör, uzunluk, rakibin rengi, kesme). Motor ise çift dummy örneklemesiyle seçer; ikisi ayrışırsa nedenine bakın.");
                return;
            }

            Yaz(_plan, "Savunma planı", true);
            foreach (var l in s.Plan) Yaz(_plan, "• " + l);
            Yaz(_plan, "");
            Yaz(_plan, "Ortağın kartları ve sinyaller", true);
            foreach (var l in s.Sinyaller) Yaz(_plan, "• " + l);
            Yaz(_plan, "");
            Yaz(_plan, "Not: Ortağın elini bilmediğiniz için plan, dummy ve sizin elinizden çıkan olasılıklara dayanır. Sinyal yorumları takımınızın işaretleşme anlaşmasına göredir (Ortaklık Anlaşması).");
        }

        private static void Yaz(RichTextBox rtb, string satir, bool baslik = false)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionFont = new Font(rtb.Font, baslik ? FontStyle.Bold : FontStyle.Regular);
            rtb.SelectionColor = baslik ? Color.FromArgb(25, 70, 140) : Color.Black;
            rtb.AppendText(satir + "\n");
        }
    }
}
