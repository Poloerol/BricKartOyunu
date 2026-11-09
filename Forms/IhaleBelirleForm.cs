using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// "Özel İhale Oluşturma" (Bridge Baron "Set Auction"): Kart Dağıtıcı'nın arayacağı ihaleyi girer.
    /// İhale dağıtıcıdan başlar; kısmi ihale de olabilir (girilen teklifler motorun vereceği tekliflerle eşleşmelidir).
    /// </summary>
    public class IhaleBelirleForm : Form
    {
        private static readonly Player[] KoltukSirasi = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };   // dağıtıcı sırası
        private static readonly string[] KoltukAd = { "Kuzey", "Doğu", "Güney", "Batı" };
        private static readonly Player[] SutunSirasi = { Player.Bati, Player.Kuzey, Player.Dogu, Player.Guney };      // tablo: W N E S
        private static readonly string[] SutunAd = { "Batı", "Kuzey", "Doğu", "Güney" };
        private static readonly string[] Sembol = { "♣", "♦", "♥", "♠", "NT" };

        /// <summary>"Herhangi": o sıradaki çağrı ne olursa olsun eşleşir (Bridge Baron "Any Call").</summary>
        public const string Herhangi = "Herhangi";

        private readonly int _dagitici;
        private readonly Button[,] _teklifDugmeleri = new Button[7, 5];   // [seviye-1, ♣ ♦ ♥ ♠ NT]
        private readonly ListView _liste = new ListView { View = View.Details, HeaderStyle = ColumnHeaderStyle.None, FullRowSelect = false, HideSelection = true, MultiSelect = false, BorderStyle = BorderStyle.Fixed3D };
        private readonly BiddingState _durum = new BiddingState();
        private readonly List<string> _teklifler = new List<string>();
        private Button _bDbl, _bRdbl, _bPas, _bHer;   // ardışık 3 pastan sonra pasifleşir
        private bool _joker;   // "Herhangi" girildi: sonraki çağrıların durumu bilinemez, yasallık denetlenmez

        /// <summary>Seçilen dağıtıcı: 0 Kuzey, 1 Doğu, 2 Güney, 3 Batı (DagitimUretici sırası).</summary>
        public int Dagitici { get { return _dagitici; } }
        /// <summary>Dağıtıcıdan başlayarak girilen teklifler (boş = ihale koşulu yok). "Herhangi" her çağrıyla eşleşir.</summary>
        public List<string> Teklifler { get { return new List<string>(_teklifler); } }

        // Maça siyah, Kupa kırmızı, Karo turuncu, Sinek yeşil
        private static Color SuitRengi(char c)
        {
            switch (c)
            {
                case '♥': return Color.Red;
                case '♦': return Color.DarkOrange;
                case '♣': return Color.Green;
                default: return SystemColors.WindowText;
            }
        }

        public IhaleBelirleForm(int dagitici, List<string> mevcut)
        {
            _dagitici = dagitici >= 0 && dagitici < 4 ? dagitici : 0;
            Text = "Özel İhale Oluşturma";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.None;
            Font = new Font("Microsoft Sans Serif", 8.25F);
            ClientSize = new Size(348, 209);          // pencere 350x240

            // Sütun başlıkları (dağıtıcının altı çizilir)
            for (int c = 0; c < 4; c++)
            {
                var l = new Label { Text = SutunAd[c], AutoSize = false, Location = new Point(15 + c * 36, 5), Size = new Size(36, 14), TextAlign = ContentAlignment.MiddleLeft };
                if (SutunSirasi[c] == KoltukSirasi[_dagitici]) l.Font = new Font(Font, FontStyle.Underline);   // dağıtıcı: altı çizili (kalın yazı sütuna sığmıyor)
                Controls.Add(l);
            }

            _liste.Location = new Point(13, 22); _liste.Size = new Size(167, 112);
            for (int c = 0; c < 4; c++) _liste.Columns.Add("", 36);
            _liste.OwnerDraw = true;
            _liste.DrawColumnHeader += (s, e) => e.DrawDefault = true;
            _liste.DrawItem += (s, e) => { };
            _liste.DrawSubItem += ListeHucreCiz;
            Controls.Add(_liste);

            // Teklif düğmeleri: seviye satırları 1..7, sütunlar ♣ ♦ ♥ ♠ NT
            for (int sv = 1; sv <= 7; sv++)
            {
                Controls.Add(new Label { Text = sv + ".", AutoSize = false, Location = new Point(182, 2 + (sv - 1) * 24), Size = new Size(20, 23), TextAlign = ContentAlignment.MiddleRight });
                for (int r = 0; r < 5; r++)
                {
                    string t = sv + Sembol[r];
                    var b = new Button { Text = Sembol[r], Location = new Point(204 + r * 27, 2 + (sv - 1) * 24), Size = new Size(26, 23), Tag = t, Padding = Padding.Empty };
                    b.Font = r == 4 ? new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold) : new Font("Segoe UI Symbol", 11F, FontStyle.Bold);
                    b.ForeColor = r < 4 ? SuitRengi(Sembol[r][0]) : Color.Black;
                    b.UseCompatibleTextRendering = false;
                    b.Click += (s, e) => Ekle((string)((Button)s).Tag);
                    _teklifDugmeleri[sv - 1, r] = b;
                    Controls.Add(b);
                }
            }

            var tamam = new Button { Text = "Tamam", Location = new Point(13, 142), Size = new Size(76, 23), DialogResult = DialogResult.OK };
            var rebid = new Button { Text = "Rebid", Location = new Point(104, 142), Size = new Size(76, 23) };
            var bDbl = new Button { Text = "Dbl", Location = new Point(13, 173), Size = new Size(76, 23) };
            var bRdbl = new Button { Text = "RDbl", Location = new Point(93, 173), Size = new Size(76, 23) };
            var bPas = new Button { Text = "Pas", Location = new Point(176, 173), Size = new Size(70, 23) };
            var bHer = new Button { Text = "Herhangi", Location = new Point(252, 173), Size = new Size(87, 23) };
            _bDbl = bDbl; _bRdbl = bRdbl; _bPas = bPas; _bHer = bHer;
            rebid.Click += (s, e) => Yeniden(new List<string>());     // ihaleyi baştan girmek üzere temizler
            bDbl.Click += (s, e) => Ekle("Dbl");
            bRdbl.Click += (s, e) => Ekle("RDbl");
            bPas.Click += (s, e) => Ekle("Pas");
            bHer.Click += (s, e) => Ekle(Herhangi);
            Controls.AddRange(new Control[] { tamam, rebid, bDbl, bRdbl, bPas, bHer });
            AcceptButton = tamam;

            Yeniden(mevcut ?? new List<string>());
        }

        private void ListeHucreCiz(object sender, DrawListViewSubItemEventArgs e)
        {
            using (var arka = new SolidBrush(_liste.BackColor)) e.Graphics.FillRectangle(arka, e.Bounds);
            string t = e.SubItem.Text ?? "";
            if (t == Herhangi) t = "Herh.";    // dar sütuna sığması için kısaltılır
            var bayrak = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            int x = e.Bounds.X + 2;
            foreach (char ch in t)
            {
                string p = ch.ToString();
                var boyut = TextRenderer.MeasureText(e.Graphics, p, _liste.Font, new Size(int.MaxValue, int.MaxValue), bayrak);
                TextRenderer.DrawText(e.Graphics, p, _liste.Font, new Rectangle(x, e.Bounds.Y, boyut.Width + 2, e.Bounds.Height), SuitRengi(ch), bayrak);
                x += boyut.Width;
            }
        }

        /// <summary>Deklarasyon penceresindeki gibi: son gerçek teklif ve ondan düşük/eşit teklif düğmelerinin yazısı silinir.</summary>
        private void TeklifDugmeleriniGuncelle()
        {
            int son = -1;
            for (int i = _teklifler.Count - 1; i >= 0; i--)
            {
                string t = _teklifler[i];
                if (t.Length >= 2 && t[0] >= '1' && t[0] <= '7')
                {
                    int r = Array.IndexOf(Sembol, t.Substring(1));
                    if (r >= 0) son = (t[0] - '1') * 5 + r;
                    break;
                }
            }
            for (int sv = 0; sv < 7; sv++)
                for (int r = 0; r < 5; r++)
                {
                    var d = _teklifDugmeleri[sv, r];
                    bool aktif = sv * 5 + r > son;
                    d.Text = aktif ? Sembol[r] : "";
                    d.Enabled = aktif;
                }

            // Ardışık 3 pastan sonra Dbl, RDbl, Pas ve Herhangi pasif olur
            int n = _teklifler.Count;
            bool ikiPas = n >= 3 && _teklifler[n - 1] == "Pas" && _teklifler[n - 2] == "Pas" && _teklifler[n - 3] == "Pas";
            _bDbl.Enabled = _bRdbl.Enabled = _bPas.Enabled = _bHer.Enabled = !ikiPas;
        }

        private Player Dagiticiyi() { return KoltukSirasi[_dagitici]; }

        /// <summary>Durumu sıfırlayıp verilen teklifleri baştan uygular.</summary>
        private void Yeniden(List<string> liste)
        {
            _teklifler.Clear();
            _joker = false;
            _durum.Reset(Dagiticiyi());
            foreach (var t in liste)
            {
                if (t == Herhangi) { _joker = true; _teklifler.Add(Herhangi); continue; }
                if (_joker) { _teklifler.Add(t); continue; }
                var bid = Bid.Parse(t);
                if (!BidValidator.IsLegal(_durum, _durum.CurrentPlayer, bid)) break;
                _durum.ApplyBid(_durum.CurrentPlayer, bid);
                _teklifler.Add(bid.ToDisplayString());
            }
            Goster();
        }

        private void Ekle(string metin)
        {
            if (metin == Herhangi) { _joker = true; _teklifler.Add(Herhangi); Goster(); return; }
            if (_joker)
            {
                // Herhangi'den sonra durum bilinmez: yalnızca biçim denetimi yapılır
                _teklifler.Add(Bid.Parse(metin).ToDisplayString());
                Goster();
                return;
            }
            if (_durum.AuctionFinished) { System.Media.SystemSounds.Beep.Play(); return; }
            var bid = Bid.Parse(metin);
            if (!BidValidator.IsLegal(_durum, _durum.CurrentPlayer, bid))
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }
            _durum.ApplyBid(_durum.CurrentPlayer, bid);
            _teklifler.Add(bid.ToDisplayString());
            Goster();
        }

        private void Goster()
        {
            _liste.BeginUpdate();
            _liste.Items.Clear();
            int ilk = Array.IndexOf(SutunSirasi, Dagiticiyi());
            int toplam = ilk + _teklifler.Count;
            for (int satir = 0; satir * 4 < toplam; satir++)
            {
                var hucre = new string[4];
                for (int c = 0; c < 4; c++)
                {
                    int idx = satir * 4 + c - ilk;
                    hucre[c] = idx >= 0 && idx < _teklifler.Count ? _teklifler[idx] : "";
                }
                _liste.Items.Add(new ListViewItem(hucre));
            }
            _liste.EndUpdate();
            if (_liste.Items.Count > 0) _liste.EnsureVisible(_liste.Items.Count - 1);
            TeklifDugmeleriniGuncelle();
        }
    }
}
