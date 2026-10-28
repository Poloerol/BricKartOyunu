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
    /// "İhale Belirle" (Bridge Baron "Set Auction"): Kart Dağıtıcı'nın arayacağı ihaleyi girer.
    /// İhale dağıtıcıdan başlar; kısmi ihale de olabilir (girilen teklifler motorun vereceği tekliflerle eşleşmelidir).
    /// </summary>
    public class IhaleBelirleForm : Form
    {
        private static readonly Player[] KoltukSirasi = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };   // dağıtıcı kutusu
        private static readonly string[] KoltukAd = { "Kuzey", "Doğu", "Güney", "Batı" };
        private static readonly Player[] SutunSirasi = { Player.Bati, Player.Kuzey, Player.Dogu, Player.Guney };      // BB tablosu: W N E S
        private static readonly string[] SutunAd = { "Batı", "Kuzey", "Doğu", "Güney" };

        private readonly ComboBox _cmbDagitici = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
        private readonly ListView _liste = new ListView { View = View.Details, GridLines = true, FullRowSelect = true, HideSelection = true };
        private readonly Label _lblDurum = new Label { AutoSize = false, Height = 20 };
        private readonly BiddingState _durum = new BiddingState();
        private readonly List<string> _teklifler = new List<string>();

        /// <summary>Seçilen dağıtıcı: 0 Kuzey, 1 Doğu, 2 Güney, 3 Batı (DagitimUretici sırası).</summary>
        public int Dagitici { get { return Math.Max(0, _cmbDagitici.SelectedIndex); } }
        /// <summary>Dağıtıcıdan başlayarak girilen teklifler (boş = ihale koşulu yok).</summary>
        public List<string> Teklifler { get { return new List<string>(_teklifler); } }

        public IhaleBelirleForm(int dagitici, List<string> mevcut)
        {
            Text = "İhale Belirle";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            ClientSize = new Size(470, 420);
            Font = new Font("Segoe UI", 9F);

            Controls.Add(new Label { Text = "Dağıtıcı:", AutoSize = true, Location = new Point(12, 16) });
            _cmbDagitici.Location = new Point(76, 12);
            _cmbDagitici.Items.AddRange(KoltukAd);
            _cmbDagitici.SelectedIndex = dagitici >= 0 && dagitici < 4 ? dagitici : 0;
            _cmbDagitici.SelectedIndexChanged += (s, e) => Yeniden(new List<string>());
            Controls.Add(_cmbDagitici);

            _liste.Location = new Point(12, 46); _liste.Size = new Size(446, 150);
            foreach (var ad in SutunAd) _liste.Columns.Add(ad, 108);
            Controls.Add(_liste);

            // Teklif düğmeleri: seviye satırları 1..7, sütunlar ♣ ♦ ♥ ♠ NT
            string[] sembol = { "♣", "♦", "♥", "♠", "NT" };
            for (int sv = 1; sv <= 7; sv++)
                for (int r = 0; r < 5; r++)
                {
                    string t = sv + sembol[r];
                    var b = new Button { Text = t, Location = new Point(12 + r * 56, 206 + (sv - 1) * 26), Size = new Size(54, 24), Tag = t };
                    b.ForeColor = (r == 1 || r == 2) ? Color.Firebrick : Color.Black;
                    b.Click += (s, e) => Ekle((string)((Button)s).Tag);
                    Controls.Add(b);
                }
            var bPas = new Button { Text = "Pas", Location = new Point(310, 206), Size = new Size(70, 28) };
            var bDbl = new Button { Text = "Dbl", Location = new Point(388, 206), Size = new Size(70, 28) };
            var bRdbl = new Button { Text = "RDbl", Location = new Point(310, 240), Size = new Size(70, 28) };
            var bGeri = new Button { Text = "Geri Al", Location = new Point(388, 240), Size = new Size(70, 28) };
            var bTemiz = new Button { Text = "Temizle", Location = new Point(310, 274), Size = new Size(70, 28) };
            bPas.Click += (s, e) => Ekle("Pas");
            bDbl.Click += (s, e) => Ekle("Dbl");
            bRdbl.Click += (s, e) => Ekle("RDbl");
            bGeri.Click += (s, e) => { if (_teklifler.Count > 0) { var k = new List<string>(_teklifler); k.RemoveAt(k.Count - 1); Yeniden(k); } };
            bTemiz.Click += (s, e) => Yeniden(new List<string>());
            Controls.AddRange(new Control[] { bPas, bDbl, bRdbl, bGeri, bTemiz });

            _lblDurum.Location = new Point(12, 392); _lblDurum.Size = new Size(280, 20);
            Controls.Add(_lblDurum);
            var tamam = new Button { Text = "Tamam", Location = new Point(300, 384), Size = new Size(76, 28), DialogResult = DialogResult.OK };
            var iptal = new Button { Text = "İptal", Location = new Point(382, 384), Size = new Size(76, 28), DialogResult = DialogResult.Cancel };
            Controls.AddRange(new Control[] { tamam, iptal });
            AcceptButton = tamam; CancelButton = iptal;

            Yeniden(mevcut ?? new List<string>());
        }

        private Player Dagiticiyi() { return KoltukSirasi[_cmbDagitici.SelectedIndex]; }

        /// <summary>Durumu sıfırlayıp verilen teklifleri baştan uygular.</summary>
        private void Yeniden(List<string> liste)
        {
            _teklifler.Clear();
            _durum.Reset(Dagiticiyi());
            foreach (var t in liste)
            {
                var bid = Bid.Parse(t);
                if (!BidValidator.IsLegal(_durum, _durum.CurrentPlayer, bid)) break;
                _durum.ApplyBid(_durum.CurrentPlayer, bid);
                _teklifler.Add(bid.ToDisplayString());
            }
            Goster();
        }

        private void Ekle(string metin)
        {
            if (_durum.AuctionFinished) { _lblDurum.Text = "İhale bitti."; return; }
            var bid = Bid.Parse(metin);
            if (!BidValidator.IsLegal(_durum, _durum.CurrentPlayer, bid))
            {
                _lblDurum.Text = "Geçersiz teklif: " + bid.ToDisplayString();
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
            _lblDurum.Text = _durum.AuctionFinished ? "İhale tamamlandı."
                : _teklifler.Count == 0 ? "İhale koşulu yok."
                : "Sıradaki: " + KoltukAdi(_durum.CurrentPlayer);
        }

        private static string KoltukAdi(Player p)
        {
            return KoltukAd[Array.IndexOf(KoltukSirasi, p)];
        }
    }
}
