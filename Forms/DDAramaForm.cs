using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Çift Dummy Arama: dört elin de açık olduğu varsayımıyla, sırası gelen oyuncunun yasal kartlarını değerlendirir.
    /// Hızlı Arama: en iyi kart(lar). Ayrıntılı Arama: her yasal kartın löve değeri. Tam Arama: ayrıntılı değerler +
    /// iki tarafın en iyi oynamasıyla oyunun devamı. Yazdır: listedeki metni yazdırır.
    /// </summary>
    public class DDAramaForm : Form
    {
        private static readonly string[] Semboller = { "♠", "♥", "♦", "♣" };
        private static readonly string[] Siralar = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        private readonly ulong[] _eller;            // 4 koltuğun kalan kartları (koltuk sırası K, D, G, B)
        private readonly int _lider;                // eli açan koltuk
        private readonly int[] _el;                 // elde oynanmış kartlar (kod), liderden başlayarak
        private readonly int _elSayisi;
        private readonly int _koz;                  // renk no (0 ♠ ... 3 ♣), -1 sanzatu
        private readonly Func<int, string> _koltukAdi;

        private readonly ListBox _liste = new ListBox();
        private readonly Button _btnHizli = new Button();
        private readonly Button _btnAyrintili = new Button();
        private readonly Button _btnTam = new Button();
        private readonly Button _btnCikis = new Button();
        private readonly Button _btnYazdir = new Button();
        private bool _calisiyor;
        private int _yazdirSatir;

        /// <summary>Verilirse pencere bu ekran noktasının (masa panelinin merkezi) tam ortasına açılır.</summary>
        public Point? MerkezEkran { get; set; }

        public DDAramaForm(ulong[] eller, int lider, int[] el, int elSayisi, int koz, Func<int, string> koltukAdi)
        {
            _eller = (ulong[])eller.Clone();
            _lider = lider;
            _el = (int[])el.Clone();
            _elSayisi = elSayisi;
            _koz = koz;
            _koltukAdi = koltukAdi;

            Text = "Çift Dummy Arama";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(347, 405);
            BackColor = Color.FromArgb(240, 240, 240);

            _liste.Location = new Point(11, 11);
            _liste.Size = new Size(218, 383);
            _liste.IntegralHeight = false;
            _liste.HorizontalScrollbar = true;
            _liste.BorderStyle = BorderStyle.Fixed3D;
            _liste.Font = new Font("Consolas", 9f);
            Controls.Add(_liste);

            Dugme(_btnHizli, "Hızlı Arama", 11);
            Dugme(_btnAyrintili, "Ayrıntılı Arama", 44);
            Dugme(_btnTam, "Tam Arama", 76);
            Dugme(_btnCikis, "Çıkış", 109);
            Dugme(_btnYazdir, "Yazdır", 377 - 11 - 24 + 11);   // listenin alt kenarı hizasında
            _btnYazdir.Top = _liste.Bottom - _btnYazdir.Height;

            _btnHizli.Click += (s, e) => Ara(Mod.Hizli);
            _btnAyrintili.Click += (s, e) => Ara(Mod.Ayrintili);
            _btnTam.Click += (s, e) => Ara(Mod.Tam);
            _btnCikis.Click += (s, e) => Close();
            _btnYazdir.Click += (s, e) => Yazdir();
            CancelButton = _btnCikis;

            Load += (s, e) =>
            {
                if (MerkezEkran.HasValue)
                {
                    var p = MerkezEkran.Value;
                    Location = new Point(p.X - Width / 2, p.Y - Height / 2);
                }
                Baslik();
            };
        }

        private void Dugme(Button b, string metin, int top)
        {
            b.Text = metin;
            b.Location = new Point(238, top);
            b.Size = new Size(98, 24);
            b.UseVisualStyleBackColor = true;
            Controls.Add(b);
        }

        private enum Mod { Hizli, Ayrintili, Tam }

        private static string KartMetni(int kod) { return Semboller[kod >> 4] + Siralar[kod & 15]; }

        private static int Pop(ulong x)
        {
            int n = 0;
            while (x != 0) { x &= x - 1; n++; }
            return n;
        }

        private void Baslik()
        {
            _liste.Items.Clear();
            int mover = (_lider + _elSayisi) & 3;
            _liste.Items.Add("Sıra: " + _koltukAdi(mover));
            _liste.Items.Add("Koz: " + (_koz < 0 ? "Sanzatu" : Semboller[_koz]));
            _liste.Items.Add("Kalan löve: " + Pop(_eller[mover]));
            if (_elSayisi > 0)
            {
                var masa = new List<string>();
                for (int i = 0; i < _elSayisi; i++) masa.Add(_koltukAdi((_lider + i) & 3) + " " + KartMetni(_el[i]));
                _liste.Items.Add("Masada: " + string.Join(", ", masa));
            }
            _liste.Items.Add("");
        }

        private void Mesgul(bool mesgul)
        {
            _calisiyor = mesgul;
            _btnHizli.Enabled = _btnAyrintili.Enabled = _btnTam.Enabled = _btnYazdir.Enabled = !mesgul;
            UseWaitCursor = mesgul;
        }

        private async void Ara(Mod mod)
        {
            if (_calisiyor) return;
            Mesgul(true);
            Baslik();
            _liste.Items.Add("Aranıyor...");
            List<string> satirlar = null;
            try
            {
                satirlar = await Task.Run(() => Hesapla(mod));
            }
            catch (Exception ex)
            {
                satirlar = new List<string> { "Arama hatası: " + ex.Message };
            }
            if (IsDisposed) return;
            Baslik();
            foreach (var s in satirlar) _liste.Items.Add(s);
            Mesgul(false);
        }

        private List<string> Hesapla(Mod mod)
        {
            var sonuc = new List<string>();
            var cozucu = new DDSolver(_koz);
            int mover = (_lider + _elSayisi) & 3;
            int kalan = Pop(_eller[mover]);
            if (kalan == 0) { sonuc.Add("Oynanacak kart kalmadı."); return sonuc; }

            int[] kartlar, degerler;
            cozucu.Degerler(_eller, _lider, _el, _elSayisi, out kartlar, out degerler);
            bool benNS = (mover & 1) == 0;
            // değer: çift sayılı koltukların (K-G) alacağı löve; sırası gelen tarafın löve sayısına çevrilir
            var puanlar = kartlar.Select((k, i) => new { Kart = k, Löve = benNS ? degerler[i] : kalan - degerler[i] })
                                 .OrderByDescending(x => x.Löve).ThenBy(x => x.Kart >> 4).ThenByDescending(x => x.Kart & 15).ToList();
            int en = puanlar.Max(x => x.Löve);

            if (mod == Mod.Hizli)
            {
                sonuc.Add("En iyi kart(lar):");
                foreach (var p in puanlar.Where(x => x.Löve == en)) sonuc.Add("  " + KartMetni(p.Kart));
                sonuc.Add("");
                sonuc.Add(_koltukAdi(mover) + " tarafı " + en + " löve alır.");
                return sonuc;
            }

            sonuc.Add("Kart değerleri (" + _koltukAdi(mover) + " tarafının löveleri):");
            foreach (var p in puanlar) sonuc.Add("  " + KartMetni(p.Kart).PadRight(4) + " → " + p.Löve + (p.Löve == en ? "  *" : ""));
            sonuc.Add("");
            sonuc.Add(_koltukAdi(mover) + " tarafı " + en + " löve alır.");
            if (mod == Mod.Ayrintili) return sonuc;

            sonuc.Add("");
            sonuc.Add("En iyi oyunla devamı:");
            OyunDevami(cozucu, sonuc);
            return sonuc;
        }

        /// <summary>İki taraf da çift dummy en iyi oynarsa elin kalanının dökümü (löve löve).</summary>
        private void OyunDevami(DDSolver cozucu, List<string> sonuc)
        {
            var h = (ulong[])_eller.Clone();
            int lider = _lider, k = _elSayisi;
            var tc = new int[4];
            Array.Copy(_el, tc, _elSayisi);
            int nsLove = 0, ewLove = 0;
            int loveNo = 1;
            var bu = new List<string>();
            for (int i = 0; i < k; i++) bu.Add(_koltukAdi((lider + i) & 3) + " " + KartMetni(tc[i]));

            while (h.Any(x => x != 0) || k > 0)
            {
                int mover = (lider + k) & 3;
                if (h[mover] == 0) break;
                int[] kartlar, degerler;
                cozucu.Degerler(h, lider, tc, k, out kartlar, out degerler);
                bool max = (mover & 1) == 0;
                int secili = 0;
                for (int i = 1; i < kartlar.Length; i++)
                    if (max ? degerler[i] > degerler[secili] : degerler[i] < degerler[secili]) secili = i;
                int kart = kartlar[secili];
                h[mover] &= ~(1UL << kart);
                tc[k] = kart;
                bu.Add(_koltukAdi(mover) + " " + KartMetni(kart));
                k++;
                if (k == 4)
                {
                    int kazanan = KazananBul(tc, lider);
                    if ((kazanan & 1) == 0) nsLove++; else ewLove++;
                    sonuc.Add(loveNo + ". " + string.Join(" ", bu) + " → " + _koltukAdi(kazanan));
                    loveNo++;
                    bu.Clear();
                    lider = kazanan; k = 0;
                }
            }
            sonuc.Add("");
            sonuc.Add("K-G: " + nsLove + " löve, D-B: " + ewLove + " löve (bu elden itibaren)");
        }

        private int KazananBul(int[] tc, int lider)
        {
            int best = 0;
            int bs = tc[0] >> 4, br = tc[0] & 15;
            for (int i = 1; i < 4; i++)
            {
                int s = tc[i] >> 4, r = tc[i] & 15;
                if (s == bs) { if (r > br) { best = i; br = r; } }
                else if (s == _koz && bs != _koz) { best = i; bs = s; br = r; }
            }
            return (lider + best) & 3;
        }

        private void Yazdir()
        {
            if (_liste.Items.Count == 0) return;
            try
            {
                using (var belge = new PrintDocument())
                using (var dlg = new PrintDialog { Document = belge })
                {
                    belge.DocumentName = "Çift Dummy Arama";
                    _yazdirSatir = 0;
                    belge.PrintPage += (s, e) =>
                    {
                        using (var yazi = new Font("Consolas", 10f))
                        {
                            float satirYuksekligi = yazi.GetHeight(e.Graphics);
                            float y = e.MarginBounds.Top;
                            while (_yazdirSatir < _liste.Items.Count && y + satirYuksekligi <= e.MarginBounds.Bottom)
                            {
                                e.Graphics.DrawString(Convert.ToString(_liste.Items[_yazdirSatir]), yazi, Brushes.Black, e.MarginBounds.Left, y);
                                y += satirYuksekligi;
                                _yazdirSatir++;
                            }
                            e.HasMorePages = _yazdirSatir < _liste.Items.Count;
                        }
                    };
                    if (dlg.ShowDialog(this) == DialogResult.OK) belge.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Yazdırılamadı: " + ex.Message, "Çift Dummy Arama", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
