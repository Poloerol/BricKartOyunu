using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    // Depodan İhale Düzenle: Aç → Düzenlenecek Dağılımı Seç → Dağılım Düzenle (ana pencere) →
    // Elleri Düzenle (El Girişi + Dağıtıcı/Zon), İhaleyi Düzenle (İhale Başlat), İhale Dipnotu, Son Kritik El.
    // Düzenleme, bordun ham PBN satırları üzerinde yapılır; tanınmayan etiketler korunur.
    public partial class DagilimDepo
    {
        private static readonly Regex TagRx = new Regex("^\\[(\\w+)\\s+\"(.*)\"\\]\\s*$");
        private static readonly Regex CagriRx = new Regex("^([1-7](C|D|H|S|NT|N)|PASS|P|X|XX)$", RegexOptions.IgnoreCase);

        // ───────────────────────── PBN blok yardımcıları ─────────────────────────

        private static int TagIndeks(PbnBlok b, string tag)
        {
            for (int i = 0; i < b.Satirlar.Count; i++)
            {
                var m = TagRx.Match(b.Satirlar[i].Trim());
                if (m.Success && m.Groups[1].Value == tag) return i;
            }
            return -1;
        }

        private static string TagAl(PbnBlok b, string tag)
        {
            int i = TagIndeks(b, tag);
            return i < 0 ? "" : TagRx.Match(b.Satirlar[i].Trim()).Groups[2].Value;
        }

        /// <summary>Etiketi yazar (yoksa [Deal]'ın altına ekler); değer boşsa etiketi siler.</summary>
        private static void TagYaz(PbnBlok b, string tag, string deger)
        {
            int i = TagIndeks(b, tag);
            if (string.IsNullOrEmpty(deger))
            {
                if (i >= 0) b.Satirlar.RemoveAt(i);
                return;
            }
            string satir = "[" + tag + " \"" + deger.Replace("\"", "'") + "\"]";
            if (i >= 0) { b.Satirlar[i] = satir; return; }
            int ekle;
            if (tag == "Event") ekle = 0;
            else
            {
                int d = TagIndeks(b, "Deal");
                ekle = d < 0 ? b.Satirlar.Count : d + 1;
            }
            b.Satirlar.Insert(ekle, satir);
        }

        /// <summary>Etiketin altındaki veri satırlarının bitişi (Auction/Play gibi): boş satır, yeni etiket veya yorumda biter.</summary>
        private static int VeriBitisi(PbnBlok b, int tagIndeksi)
        {
            int i = tagIndeksi + 1;
            while (i < b.Satirlar.Count)
            {
                string t = b.Satirlar[i].Trim();
                if (t.Length == 0 || TagRx.IsMatch(t) || t.StartsWith("{")) break;
                i++;
            }
            return i;
        }

        /// <summary>Etiketi ve (varsa) altındaki veri satırlarını siler.</summary>
        private static void TagVeriyleSil(PbnBlok b, string tag)
        {
            int i = TagIndeks(b, tag);
            if (i < 0) return;
            b.Satirlar.RemoveRange(i, VeriBitisi(b, i) - i);
        }

        private static string YorumAl(PbnBlok b)
        {
            var sb = new StringBuilder();
            bool ic = false;
            foreach (string s in b.Satirlar)
            {
                string satir = s;
                if (!ic)
                {
                    if (!satir.TrimStart().StartsWith("{")) continue;
                    satir = satir.Substring(satir.IndexOf('{') + 1);
                    ic = true;
                }
                int k = satir.IndexOf('}');
                if (k >= 0) { sb.Append(satir.Substring(0, k)); ic = false; if (sb.Length > 0) sb.Append("\r\n"); }
                else sb.Append(satir).Append("\r\n");
            }
            return sb.ToString().Trim();
        }

        private static void YorumYaz(PbnBlok b, string metin)
        {
            var yeni = new List<string>();
            bool ic = false;
            foreach (string s in b.Satirlar)
            {
                if (!ic && s.TrimStart().StartsWith("{")) ic = true;
                if (ic) { if (s.Contains("}")) ic = false; continue; }
                yeni.Add(s);
            }
            int bos = 0;
            while (yeni.Count > 0 && yeni[yeni.Count - 1].Trim().Length == 0) { yeni.RemoveAt(yeni.Count - 1); bos++; }

            metin = (metin ?? "").Replace("{", "(").Replace("}", ")").Trim();
            if (metin.Length > 0)
            {
                string[] p = metin.Replace("\r\n", "\n").Split('\n');
                p[0] = "{" + p[0];
                p[p.Length - 1] += "}";
                yeni.AddRange(p);
            }
            for (int i = 0; i < Math.Max(1, bos); i++) yeni.Add("");
            b.Satirlar = yeni;
        }

        // ───────────────────────── Eller / ihale / oyun verisi ─────────────────────────

        /// <summary>PBN Deal metnini ("N:S.H.D.C ...") El Girişi'nin kabul ettiği koltuk → dosya adı listesine çevirir; okunamazsa null.</summary>
        private static Dictionary<Player, List<string>> DealEller(string deal)
        {
            if (string.IsNullOrEmpty(deal) || deal.Length < 3 || deal[1] != ':') return null;
            int bas = "NESW".IndexOf(char.ToUpperInvariant(deal[0]));
            if (bas < 0) return null;
            string[] el = deal.Substring(2).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (el.Length != 4) return null;

            var sira = new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            char[] renk = { 'S', 'H', 'D', 'C' };
            var sonuc = new Dictionary<Player, List<string>>();
            for (int k = 0; k < 4; k++)
            {
                string[] parca = el[k].Split('.');
                if (parca.Length != 4) return null;
                var liste = new List<string>();
                for (int r = 0; r < 4; r++)
                    foreach (char c in parca[r])
                    {
                        if (c == '-') continue;
                        char u = char.ToUpperInvariant(c);
                        liste.Add((u == 'T' ? "10" : u.ToString()) + renk[r] + ".png");
                    }
                sonuc[sira[(bas + k) % 4]] = liste;
            }
            return sonuc;
        }

        private class Cagri
        {
            public string Metin;   // "1C", "Pass", "X", "XX", "3NT"
            public int Not;        // dipnot numarası (yoksa 0)
        }

        private static string CagriGoster(string m)
        {
            if (m == "Pass") return "Pas";
            return m.Replace("C", "♣").Replace("D", "♦").Replace("H", "♥").Replace("S", "♠");
        }

        private static string KartGoster(string kart)
        {
            if (string.IsNullOrEmpty(kart) || kart.Length < 2) return "";
            string renk = CagriGoster(kart.Substring(0, 1).ToUpperInvariant());
            string deger = kart.Substring(1).ToUpperInvariant();
            return renk + (deger == "T" ? "10" : deger);
        }

        /// <summary>[Auction] bölümünü okur; ihale başlatan koltuğun harfini (N/E/S/W) döndürür.</summary>
        private static List<Cagri> IhaleAl(PbnBlok b, out string baslayan)
        {
            var liste = new List<Cagri>();
            int i = TagIndeks(b, "Auction");
            baslayan = i >= 0 ? TagAl(b, "Auction") : TagAl(b, "Dealer");
            if (baslayan.Length == 0 || "NESW".IndexOf(char.ToUpperInvariant(baslayan[0])) < 0) baslayan = "N";
            baslayan = baslayan.Substring(0, 1).ToUpperInvariant();
            if (i < 0) return liste;

            int son = VeriBitisi(b, i);
            for (int j = i + 1; j < son; j++)
                foreach (string tok in b.Satirlar[j].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (tok.Length > 2 && tok[0] == '=' && tok[tok.Length - 1] == '=')
                    {
                        int n;
                        if (liste.Count > 0 && int.TryParse(tok.Trim('='), out n)) liste[liste.Count - 1].Not = n;
                        continue;
                    }
                    if (!CagriRx.IsMatch(tok)) continue;
                    string m = tok.ToUpperInvariant();
                    if (m == "P" || m == "PASS") m = "Pass";
                    else if (m.EndsWith("N")) m += "T";
                    liste.Add(new Cagri { Metin = m });
                }
            return liste;
        }

        private static void IhaleYaz(PbnBlok b, string baslayan, List<Cagri> liste)
        {
            int i = TagIndeks(b, "Auction"), ekle;
            if (i >= 0)
            {
                b.Satirlar.RemoveRange(i, VeriBitisi(b, i) - i);
                ekle = i;
            }
            else
            {
                int d = TagIndeks(b, "Deal");
                ekle = d < 0 ? b.Satirlar.Count : d + 1;
            }
            var yeni = new List<string> { "[Auction \"" + baslayan + "\"]" };
            var satir = new StringBuilder();
            int n = 0;
            foreach (var c in liste)
            {
                if (satir.Length > 0) satir.Append(' ');
                satir.Append(c.Metin);
                if (c.Not > 0) satir.Append(" =").Append(c.Not).Append('=');
                if (++n % 4 == 0) { yeni.Add(satir.ToString()); satir.Length = 0; }
            }
            if (satir.Length > 0) yeni.Add(satir.ToString());
            b.Satirlar.InsertRange(ekle, yeni);
        }

        /// <summary>[Note "n:metin"] etiketleri: dipnot numarası → metin.</summary>
        private static SortedDictionary<int, string> NotlarAl(PbnBlok b)
        {
            var notlar = new SortedDictionary<int, string>();
            foreach (string s in b.Satirlar)
            {
                var m = TagRx.Match(s.Trim());
                if (!m.Success || m.Groups[1].Value != "Note") continue;
                string deger = m.Groups[2].Value;
                int k = deger.IndexOf(':');
                int n;
                if (k > 0 && int.TryParse(deger.Substring(0, k), out n)) notlar[n] = deger.Substring(k + 1);
            }
            return notlar;
        }

        private static void NotlarYaz(PbnBlok b, SortedDictionary<int, string> notlar)
        {
            b.Satirlar.RemoveAll(s => { var m = TagRx.Match(s.Trim()); return m.Success && m.Groups[1].Value == "Note"; });
            int a = TagIndeks(b, "Auction"), ekle;
            if (a >= 0) ekle = VeriBitisi(b, a);
            else
            {
                int d = TagIndeks(b, "Deal");
                ekle = d < 0 ? b.Satirlar.Count : d + 1;
            }
            var yeni = notlar.Select(k => "[Note \"" + k.Key + ":" + k.Value.Replace("\"", "'") + "\"]").ToList();
            b.Satirlar.InsertRange(ekle, yeni);
        }

        /// <summary>[Play] bölümü: her satır bir el (4 kart); sütunlar açılış yapan koltuktan başlar. Gösterim için W-N-E-S sütun sırasına çevrilir.</summary>
        private static List<string[]> OyunAl(PbnBlok b)
        {
            var satirlar = new List<string[]>();
            int i = TagIndeks(b, "Play");
            if (i < 0) return satirlar;
            string acan = TagAl(b, "Play");
            int acanIdx = acan.Length > 0 ? "WNES".IndexOf(char.ToUpperInvariant(acan[0])) : -1;
            if (acanIdx < 0) acanIdx = 0;

            int son = VeriBitisi(b, i);
            for (int j = i + 1; j < son && satirlar.Count < 13; j++)
            {
                string[] tok = b.Satirlar[j].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (tok.Length == 0) continue;
                var hucre = new string[4];
                for (int seat = 0; seat < 4; seat++)
                {
                    int kolon = (seat - acanIdx + 4) % 4;
                    hucre[seat] = kolon < tok.Length ? KartGoster(tok[kolon]) : "";
                }
                satirlar.Add(hucre);
            }
            return satirlar;
        }

        // ───────────────────────── Akış ─────────────────────────

        /// <summary>
        /// Depodan seçilen dosyadaki bir dağılımı düzenler: Aç → Düzenlenecek Dağılımı Seç → Dağılım Düzenle.
        /// Değişiklikler yalnızca "Kaydet"te dosyaya yazılır.
        /// </summary>
        private void BtnDepodanDuzenle_Click(object sender, EventArgs e)
        {
            // Düzenleme sürerken Dağılım Deposu penceresi görünmez; akış bitince ya da vazgeçilince geri gelir
            Opacity = 0;
            _duzenlemeKaydedildi = false;
            _ihaleDuzenleAcik = false;
            try { DuzenleAkisi(); }
            finally { if (!IsDisposed && !_ihaleDuzenleAcik) Opacity = 1; }
            // Kaydet'ten sonra Dağılım Deposu ve masa kapanır, AnaSayfa baştan yüklenir
            if (_duzenlemeKaydedildi) AnaSayfayaDon();
        }

        private bool _duzenlemeKaydedildi;
        private bool _ihaleDuzenleAcik;   // true: DekBasForm açık, Dağılım Deposu görünmez kalır

        /// <summary>Oyun masasında DekBasForm'u (ihale düzenleme) ekranın ortasında açar; Dağılım Deposu gizli kalır.</summary>
        private void IhaleDuzenleAc()
        {
            var masa = (Owner as BricOyna) ?? Application.OpenForms.OfType<BricOyna>().FirstOrDefault();
            if (masa == null)
            {
                MessageBox.Show(this, "Oyun masası bulunamadı.", "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _ihaleDuzenleAcik = true;
            masa.DepoIhaleDuzenleAc();
        }

        private void DuzenleAkisi()
        {
            string yol = DepoDosyasiAc();
            if (yol == null) return;

            PbnDosyasi pbn;
            try { pbn = PbnDosyasi.Oku(yol); }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Dosya okunamadı:\n" + ex.Message, "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (pbn.Bordlar.Count == 0)
            {
                MessageBox.Show(this, "Bu dosyada düzenlenecek dağılım bulunamadı.", "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var liste = pbn.Bordlar.Select(b => new[] { b.Ad, TagAl(b, "Description") }).ToList();
            int idx = BordSec("Düzenlenecek Dağılımı Seç", liste);
            if (idx < 0) return;

            var asil = pbn.Bordlar[idx];
            var calisma = new PbnBlok { Satirlar = new List<string>(asil.Satirlar), DealVar = asil.DealVar };
            string once = string.Join("\n", calisma.Satirlar);
            using (var f = new DagilimAnaFormu(calisma))
            {
                if (f.ShowDialog(this) != DialogResult.OK)
                {
                    if (f.IhaleDuzenleIstendi) IhaleDuzenleAc();
                    return;
                }
            }
            if (string.Join("\n", calisma.Satirlar) == once) { _duzenlemeKaydedildi = true; return; }

            calisma.Ad = TagAl(calisma, "Event");
            calisma.Dagitici = TagAl(calisma, "Dealer");
            calisma.Deal = TagAl(calisma, "Deal");
            pbn.Bordlar[idx] = calisma;

            try
            {
                string gecici = yol + ".tmp";
                File.WriteAllText(gecici, pbn.Yaz(), new UTF8Encoding(false));
                File.Delete(yol);
                File.Move(gecici, yol);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Değişiklikler kaydedilemedi:\n" + ex.Message, "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Oyun planı bu dosyayı kullanıyorsa bord sırası baştan başlasın
            var plan = AnaSayfa.OyunPlani;
            if (plan != null && string.Equals(plan.PbnYolu, yol, StringComparison.OrdinalIgnoreCase))
            {
                plan.SonBordNo = 0;
                plan.Kaydet();
            }
            _duzenlemeKaydedildi = true;
        }

        // ───────────────────────── Formlar ─────────────────────────

        /// <summary>Renkli çağrı/kart tablosu: her satır 4 hücre (Batı, Kuzey, Doğu, Güney); ♥ ♦ kırmızı.</summary>
        private class IzgaraPanel : Panel
        {
            private const int SatirYuksekligi = 18;
            private List<string[]> _satirlar = new List<string[]>();
            private readonly Font _yazi = new Font("Segoe UI", 9f);

            public int SatirNoGenisligi;   // 0 değilse solda satır numarası sütunu
            public int SutunGenisligi = 40;

            public IzgaraPanel()
            {
                DoubleBuffered = true;
                AutoScroll = true;
                BackColor = SystemColors.Window;
                BorderStyle = BorderStyle.FixedSingle;
                ResizeRedraw = true;
            }

            public void Doldur(List<string[]> satirlar)
            {
                _satirlar = satirlar;
                AutoScrollMinSize = new Size(0, satirlar.Count * SatirYuksekligi + 4);
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                int kaydir = AutoScrollPosition.Y;
                for (int r = 0; r < _satirlar.Count; r++)
                {
                    int y = 2 + r * SatirYuksekligi + kaydir;
                    if (y + SatirYuksekligi < 0 || y > Height) continue;
                    if (SatirNoGenisligi > 0)
                        TextRenderer.DrawText(e.Graphics, (r + 1).ToString(), _yazi, new Rectangle(0, y, SatirNoGenisligi - 4, SatirYuksekligi),
                            SystemColors.ControlText, TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    for (int c = 0; c < 4; c++)
                    {
                        string t = _satirlar[r][c];
                        if (string.IsNullOrEmpty(t)) continue;
                        Color renk = (t.Contains("♥") || t.Contains("♦")) ? Color.Red : Color.Black;
                        TextRenderer.DrawText(e.Graphics, t, _yazi, new Rectangle(SatirNoGenisligi + c * SutunGenisligi, y, SutunGenisligi, SatirYuksekligi),
                            renk, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing) _yazi.Dispose();
                base.Dispose(disposing);
            }
        }

        /// <summary>Dağılım Düzenle (ana pencere): ad, açıklama, yorum ve alt pencerelere giden düğmeler. Kaydet ile OK döner.</summary>
        private class DagilimAnaFormu : Form
        {
            private readonly PbnBlok _b;
            private readonly TextBox _txtAd, _txtAcik, _txtYorum;

            public DagilimAnaFormu(PbnBlok blok)
            {
                _b = blok;
                Text = "Dağılım Düzenle";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MinimizeBox = false; MaximizeBox = false; ShowInTaskbar = false;
                ClientSize = new Size(350, 260);

                Controls.Add(new Label { Text = "Dağılım adı:", Location = new Point(8, 12), AutoSize = true });
                _txtAd = new TextBox { Location = new Point(90, 8), Size = new Size(150, 22), Text = TagAl(blok, "Event") };
                Controls.Add(new Label { Text = "Dağılım açıklaması:", Location = new Point(8, 38), AutoSize = true });
                _txtAcik = new TextBox { Location = new Point(8, 56), Size = new Size(232, 22), Text = TagAl(blok, "Description") };
                Controls.Add(new Label { Text = "Yorum:", Location = new Point(8, 86), AutoSize = true });
                _txtYorum = new TextBox
                {
                    Location = new Point(8, 104), Size = new Size(232, 146), Multiline = true,
                    ScrollBars = ScrollBars.Vertical, AcceptsReturn = true, Text = YorumAl(blok)
                };
                Controls.AddRange(new Control[] { _txtAd, _txtAcik, _txtYorum });

                var kaydet = Dugme("Kaydet", 0);
                var ellerBtn = Dugme("Elleri Düzenle", 1);
                var ihaleBtn = Dugme("İhaleyi Düzenle", 2);
                var oyunBtn = Dugme("Oyunu Düzenle", 3);
                var dipnotBtn = Dugme("İhale Dipnotu", 4);
                var kritikBtn = Dugme("Son Kritik El", 5);
                var iptal = Dugme("Vazgeç", 6);
                oyunBtn.Enabled = false;   // bu sürümde yok
                dipnotBtn.Enabled = false;
                kritikBtn.Enabled = false;
                iptal.DialogResult = DialogResult.Cancel;
                CancelButton = iptal;

                kaydet.Click += (s, e) => Kaydet();
                ellerBtn.Click += (s, e) => ElleriDuzenle();
                ihaleBtn.Click += (s, e) => IhaleyiDuzenle();
                dipnotBtn.Click += (s, e) => { using (var f = new IhaleDipnotuFormu(_b)) f.ShowDialog(this); };
                kritikBtn.Click += (s, e) => { using (var f = new SonKritikElFormu(_b)) f.ShowDialog(this); };
            }

            private Button Dugme(string metin, int sira)
            {
                var b = new Button { Text = metin, Location = new Point(252, 8 + sira * 36), Size = new Size(90, 28), UseVisualStyleBackColor = true };
                Controls.Add(b);
                return b;
            }

            private void Kaydet()
            {
                string ad = _txtAd.Text.Trim();
                if (ad.Length > 0) TagYaz(_b, "Event", ad);
                TagYaz(_b, "Description", _txtAcik.Text.Trim());
                YorumYaz(_b, _txtYorum.Text);
                DialogResult = DialogResult.OK;
            }

            /// <summary>El Girişi (mevcut eller yüklü) → Dağıtıcı ve Zon seçimi. Eller değişirse eski ihale/oyun bilgisi silinir.</summary>
            private void ElleriDuzenle()
            {
                // El Girişi ve Dağıtıcı/Zon açıkken bu pencere görünmez; bitince (ya da vazgeçilince) geri gelir
                Opacity = 0;
                try { ElleriDuzenleAkisi(); }
                finally { if (!IsDisposed) Opacity = 1; }
            }

            private void ElleriDuzenleAkisi()
            {
                string eskiDeal = TagAl(_b, "Deal");
                var eller = DealEller(eskiDeal);
                string deal;
                using (var g = new ElGirisiForm())
                {
                    if (eller != null) g.ElleriYukle(eller);
                    if (g.ShowDialog(this) != DialogResult.OK || g.Eller == null) return;
                    try { deal = PbnDealMetni(g.Eller); }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Eller işlenemedi:\n" + ex.Message, "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                string eskiNormal = null;
                try { if (eller != null) eskiNormal = PbnDealMetni(eller); } catch (Exception) { }
                if (eskiNormal != deal)
                {
                    TagVeriyleSil(_b, "Auction");
                    TagVeriyleSil(_b, "Play");
                    foreach (string t in new[] { "Note", "LastCriticalTrick", "Contract", "Declarer", "Result" })
                        while (TagIndeks(_b, t) >= 0) TagVeriyleSil(_b, t);
                    TagYaz(_b, "Deal", deal);
                }

                using (var d = new DagiticiZonFormu(TagAl(_b, "Dealer"), TagAl(_b, "Vulnerable")))
                {
                    d.ShowDialog(this);
                    TagYaz(_b, "Dealer", d.Dagitici);
                    TagYaz(_b, "Vulnerable", d.Zon);
                }
            }

            /// <summary>İhaleyi Düzenle: bu pencere kapanır; çağıran DekBasForm'u açar (kaydedilmemiş değişiklikler dosyaya yazılmaz).</summary>
            public bool IhaleDuzenleIstendi { get; private set; }

            private void IhaleyiDuzenle()
            {
                IhaleDuzenleIstendi = true;
                DialogResult = DialogResult.Cancel;
            }
        }

        /// <summary>Dağıtıcı ve Zon seçimi. Pencere kapatılırsa başlangıçtaki değerler korunur.</summary>
        private class DagiticiZonFormu : Form
        {
            private static readonly string[] DagiticiHarf = { "N", "E", "S", "W" };
            private static readonly string[] DagiticiAd = { "Kuzey", "Doğu", "Güney", "Batı" };
            private static readonly string[] ZonDeger = { "None", "NS", "EW", "All" };
            private static readonly string[] ZonAd = { "Hiçbiri", "K/G", "D/B", "Her ikisi" };
            private readonly RadioButton[] _dag = new RadioButton[4];
            private readonly RadioButton[] _zon = new RadioButton[4];

            public string Dagitici { get; private set; }
            public string Zon { get; private set; }

            public DagiticiZonFormu(string dagitici, string zon)
            {
                Text = "Dağıtıcı ve Zon Seç";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MinimizeBox = false; MaximizeBox = false; ShowInTaskbar = false;
                ClientSize = new Size(215, 190);

                int dIdx = Math.Max(0, Array.IndexOf(DagiticiHarf, (dagitici ?? "").ToUpperInvariant()));
                int zIdx = Math.Max(0, Array.IndexOf(ZonDeger, zon ?? ""));
                Dagitici = DagiticiHarf[dIdx];
                Zon = ZonDeger[zIdx];

                var g1 = new GroupBox { Text = "Dağıtıcı", Location = new Point(8, 6), Size = new Size(95, 125) };
                var g2 = new GroupBox { Text = "Zon", Location = new Point(110, 6), Size = new Size(98, 125) };
                for (int i = 0; i < 4; i++)
                {
                    _dag[i] = new RadioButton { Text = DagiticiAd[i], Location = new Point(10, 20 + i * 24), AutoSize = true, Checked = i == dIdx };
                    _zon[i] = new RadioButton { Text = ZonAd[i], Location = new Point(10, 20 + i * 24), AutoSize = true, Checked = i == zIdx };
                    g1.Controls.Add(_dag[i]);
                    g2.Controls.Add(_zon[i]);
                }
                var tamam = new Button { Text = "Tamam", Location = new Point(70, 144), Size = new Size(75, 28), UseVisualStyleBackColor = true };
                tamam.Click += (s, e) =>
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (_dag[i].Checked) Dagitici = DagiticiHarf[i];
                        if (_zon[i].Checked) Zon = ZonDeger[i];
                    }
                    DialogResult = DialogResult.OK;
                };
                Controls.AddRange(new Control[] { g1, g2, tamam });
                AcceptButton = tamam;
            }
        }

        /// <summary>İhale Dipnotu: ihale tablosu ve üç (ihale no → dipnot) satırı. Tamam'da etiketler ve =n= işaretleri yazılır.</summary>
        private class IhaleDipnotuFormu : Form
        {
            private readonly PbnBlok _b;
            private readonly string _baslayan;
            private readonly List<Cagri> _cagrilar;
            private readonly NumericUpDown[] _no = new NumericUpDown[3];
            private readonly TextBox[] _metin = new TextBox[3];

            public IhaleDipnotuFormu(PbnBlok blok)
            {
                _b = blok;
                Text = "İhale Dipnotu";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MinimizeBox = false; MaximizeBox = false; ShowInTaskbar = false;
                ClientSize = new Size(470, 260);

                _cagrilar = IhaleAl(blok, out _baslayan);
                string[] baslik = { "Batı", "Kuzey", "Doğu", "Güney" };
                var izgara = new IzgaraPanel { Location = new Point(10, 30), Size = new Size(230, 180), SutunGenisligi = 52 };
                for (int c = 0; c < 4; c++)
                    Controls.Add(new Label { Text = baslik[c], Location = new Point(10 + c * 52, 10), Size = new Size(52, 16), TextAlign = ContentAlignment.MiddleCenter });
                var satirlar = new List<string[]>();
                int kolon = "WNES".IndexOf(_baslayan);
                var tum = new List<string>();
                for (int i = 0; i < kolon; i++) tum.Add("");
                foreach (var c in _cagrilar) tum.Add(CagriGoster(c.Metin));
                for (int i = 0; i < tum.Count; i += 4)
                {
                    var hucre = new string[4];
                    for (int k = 0; k < 4 && i + k < tum.Count; k++) hucre[k] = tum[i + k];
                    satirlar.Add(hucre);
                }
                izgara.Doldur(satirlar);
                Controls.Add(izgara);

                var notlar = NotlarAl(blok);
                var mevcut = new List<KeyValuePair<int, string>>(notlar);
                for (int k = 0; k < 3; k++)
                {
                    int y = 8 + k * 56;
                    Controls.Add(new Label { Text = "İhale no:", Location = new Point(260, y + 4), AutoSize = true });
                    _no[k] = new NumericUpDown { Location = new Point(400, y), Size = new Size(60, 22), Minimum = 0, Maximum = Math.Max(0, _cagrilar.Count) };
                    Controls.Add(new Label { Text = "Dipnot:", Location = new Point(260, y + 30), AutoSize = true });
                    _metin[k] = new TextBox { Location = new Point(318, y + 26), Size = new Size(142, 22) };
                    Controls.AddRange(new Control[] { _no[k], _metin[k] });

                    if (k < mevcut.Count)
                    {
                        _metin[k].Text = mevcut[k].Value;
                        int cagriNo = _cagrilar.FindIndex(c => c.Not == mevcut[k].Key);
                        _no[k].Value = cagriNo >= 0 ? cagriNo + 1 : 0;
                    }
                }

                var tamam = new Button { Text = "Tamam", Location = new Point(10, 224), Size = new Size(80, 28), UseVisualStyleBackColor = true };
                var iptal = new Button { Text = "Vazgeç", Location = new Point(380, 224), Size = new Size(80, 28), DialogResult = DialogResult.Cancel, UseVisualStyleBackColor = true };
                tamam.Click += (s, e) => Tamam();
                Controls.AddRange(new Control[] { tamam, iptal });
                AcceptButton = tamam; CancelButton = iptal;
            }

            private void Tamam()
            {
                var notlar = new SortedDictionary<int, string>();
                var no = new int[3];
                for (int k = 0; k < 3; k++)
                {
                    no[k] = (int)_no[k].Value;
                    string metin = _metin[k].Text.Trim();
                    if (no[k] > 0 && metin.Length == 0)
                    {
                        MessageBox.Show(this, (k + 1) + ". satırda ihale numarası var ama dipnot yazılmamış.", "Dağılım Deposu",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _metin[k].Focus();
                        return;
                    }
                    if (no[k] > 0) notlar[k + 1] = metin;
                }

                if (_cagrilar.Count > 0)
                {
                    foreach (var c in _cagrilar) c.Not = 0;
                    for (int k = 0; k < 3; k++)
                        if (no[k] > 0) _cagrilar[no[k] - 1].Not = k + 1;
                    IhaleYaz(_b, _baslayan, _cagrilar);
                }
                NotlarYaz(_b, notlar);
                DialogResult = DialogResult.OK;
            }
        }

        /// <summary>Son Kritik El: oynanan eller tablosu ve son kritik el numarası ([LastCriticalTrick]).</summary>
        private class SonKritikElFormu : Form
        {
            private readonly PbnBlok _b;
            private readonly NumericUpDown _no;

            public SonKritikElFormu(PbnBlok blok)
            {
                _b = blok;
                Text = "Son Kritik El";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MinimizeBox = false; MaximizeBox = false; ShowInTaskbar = false;
                ClientSize = new Size(305, 295);

                string[] baslik = { "Batı", "Kuzey", "Doğu", "Güney" };
                var izgara = new IzgaraPanel { Location = new Point(8, 28), Size = new Size(204, 256), SatirNoGenisligi = 22, SutunGenisligi = 42 };
                for (int c = 0; c < 4; c++)
                    Controls.Add(new Label { Text = baslik[c], Location = new Point(8 + 22 + c * 42, 8), Size = new Size(42, 16), TextAlign = ContentAlignment.MiddleCenter });
                izgara.Doldur(OyunAl(blok));
                Controls.Add(izgara);

                Controls.Add(new Label { Text = "Son Kritik El:", Location = new Point(222, 90), AutoSize = true });
                int mevcut;
                int.TryParse(TagAl(blok, "LastCriticalTrick"), out mevcut);
                _no = new NumericUpDown { Location = new Point(222, 112), Size = new Size(60, 22), Minimum = 0, Maximum = 13, Value = Math.Max(0, Math.Min(13, mevcut)) };
                Controls.Add(_no);

                var tamam = new Button { Text = "Tamam", Location = new Point(222, 200), Size = new Size(75, 28), UseVisualStyleBackColor = true };
                var iptal = new Button { Text = "Vazgeç", Location = new Point(222, 236), Size = new Size(75, 28), DialogResult = DialogResult.Cancel, UseVisualStyleBackColor = true };
                tamam.Click += (s, e) =>
                {
                    int n = (int)_no.Value;
                    TagYaz(_b, "LastCriticalTrick", n > 0 ? n.ToString() : "");
                    DialogResult = DialogResult.OK;
                };
                Controls.AddRange(new Control[] { tamam, iptal });
                AcceptButton = tamam; CancelButton = iptal;
            }
        }
    }
}
