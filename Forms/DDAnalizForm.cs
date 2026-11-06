using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Çift Dummy Analizi: dört el, 5x4 çift dummy tablosu (renkli ısı haritası), taraf gücü, par ve
    /// oynanan kontratın par ile karşılaştırması (IMP kaybı, hata sınıfı, açıklama).
    /// Hesaplama arka planda yapılır (DDS ya da yerleşik çözücü).
    /// </summary>
    public sealed class DDAnalizForm : Form
    {
        private readonly ulong[] _eller;
        private readonly int _dagitici;
        private readonly bool _nsZon, _ewZon;
        private readonly IhaleSonucu _sonuc;
        private readonly string _ihaleMetni;
        private readonly string _baslik;
        private DDTablo _tablo;
        private ParSonuc _par;
        private IhaleDegerlendirme _deg;
        private bool _seviyeGoster;
        private readonly DDKanvas _kanvas;
        private readonly CheckBox _seviyeKutusu;
        private readonly Button _kapat;

        public DDAnalizForm(ulong[] eller, int dagitici, bool nsZon, bool ewZon, IhaleSonucu sonuc,
                            string ihaleMetni, string baslik, DDTablo hazirTablo = null)
        {
            _eller = eller; _dagitici = dagitici; _nsZon = nsZon; _ewZon = ewZon;
            _sonuc = sonuc; _ihaleMetni = ihaleMetni ?? ""; _baslik = baslik ?? "Çift Dummy Analizi";

            this.Text = "Çift Dummy Analizi";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false; this.MinimizeBox = false; this.ShowInTaskbar = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(DDKanvas.Genislik, DDKanvas.Yukseklik + 44);
            this.BackColor = DDKanvas.Zemin;
            this.DoubleBuffered = true;

            _kanvas = new DDKanvas(this) { Location = new Point(0, 0), Size = new Size(DDKanvas.Genislik, DDKanvas.Yukseklik) };
            this.Controls.Add(_kanvas);

            _seviyeKutusu = new CheckBox
            {
                Text = "Löve yerine seviye göster", ForeColor = DDKanvas.Soluk, BackColor = DDKanvas.Zemin,
                AutoSize = true, Location = new Point(20, DDKanvas.Yukseklik + 12), FlatStyle = FlatStyle.Flat
            };
            _seviyeKutusu.CheckedChanged += (s, e) => { _seviyeGoster = _seviyeKutusu.Checked; _kanvas.Invalidate(); };
            this.Controls.Add(_seviyeKutusu);

            _kapat = new Button
            {
                Text = "Kapat", Size = new Size(96, 28), Location = new Point(DDKanvas.Genislik - 96 - 20, DDKanvas.Yukseklik + 8),
                DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, ForeColor = DDKanvas.Metin, BackColor = DDKanvas.Panel
            };
            _kapat.FlatAppearance.BorderColor = DDKanvas.Cizgi;
            this.Controls.Add(_kapat);
            this.AcceptButton = _kapat; this.CancelButton = _kapat;

            if (hazirTablo != null) Hazirla(hazirTablo);
            else
            {
                var el = _eller;
                Task.Run(() => DDMotor.Hesapla(el)).ContinueWith(t =>
                {
                    if (IsDisposed) return;
                    try { BeginInvoke(new Action(() => Hazirla(t.Status == TaskStatus.RanToCompletion ? t.Result : null))); } catch { }
                });
            }
        }

        private void Hazirla(DDTablo t)
        {
            _tablo = t;
            if (t != null)
            {
                _par = ParHesabi.Hesapla(t, _dagitici, _nsZon, _ewZon);
                if (_sonuc != null)
                    _deg = IhaleDegerlendirme.Degerlendir(t, _dagitici, _nsZon, _ewZon, _sonuc.Pas, _sonuc.Seviye, _sonuc.Koz, _sonuc.Deklaran, _sonuc.Kontr);
            }
            _kanvas.Invalidate();
        }

        /// <summary>Test/ekran görüntüsü için formu kanvasıyla bir bitmap'e çizer.</summary>
        public Bitmap GoruntuAl()
        {
            var bmp = new Bitmap(DDKanvas.Genislik, DDKanvas.Yukseklik);
            using (var g = Graphics.FromImage(bmp)) _kanvas.Ciz(g);
            return bmp;
        }

        public void TabloAyarla(DDTablo t) { Hazirla(t); }
        public bool SeviyeGoster { get { return _seviyeGoster; } set { _seviyeGoster = value; } }

        // ═════════════════════════════════════════════════════════════
        private sealed class DDKanvas : Control
        {
            public const int Genislik = 960, Yukseklik = 640;
            public static readonly Color Zemin = Color.FromArgb(13, 24, 19);
            public static readonly Color Panel = Color.FromArgb(22, 38, 30);
            public static readonly Color Cizgi = Color.FromArgb(46, 70, 58);
            public static readonly Color Metin = Color.FromArgb(232, 239, 233);
            public static readonly Color Soluk = Color.FromArgb(146, 170, 156);
            public static readonly Color Altin = Color.FromArgb(240, 192, 64);
            public static readonly Color NsRenk = Color.FromArgb(94, 200, 255);
            public static readonly Color EwRenk = Color.FromArgb(255, 143, 112);
            private static readonly Color[] RenkRenkleri =
                { Color.FromArgb(214, 224, 236), Color.FromArgb(255, 86, 86), Color.FromArgb(255, 170, 40), Color.FromArgb(70, 222, 130) };

            private readonly DDAnalizForm _f;
            private readonly Font _fBaslik = Mk("Segoe UI Semibold", 15f, FontStyle.Bold);
            private readonly Font _fEtiket = Mk("Segoe UI", 9f, FontStyle.Regular);
            private readonly Font _fKucuk = Mk("Segoe UI", 8f, FontStyle.Regular);
            private readonly Font _fEl = Mk("Consolas", 12f, FontStyle.Bold);
            private readonly Font _fBuyuk = Mk("Segoe UI Semibold", 22f, FontStyle.Bold);
            private readonly Font _fHucre = Mk("Segoe UI Semibold", 17f, FontStyle.Bold);
            private readonly Font _fSembol = Mk("Segoe UI Symbol", 14f, FontStyle.Bold);
            private readonly Font _fOrta = Mk("Segoe UI Semibold", 11f, FontStyle.Bold);

            private static Font Mk(string ad, float boyut, FontStyle st) { return new Font(ad, boyut, st, GraphicsUnit.Point); }

            public DDKanvas(DDAnalizForm f)
            {
                _f = f;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }

            protected override void OnPaint(PaintEventArgs e) { Ciz(e.Graphics); }

            // ── yardımcılar ───────────────────────────────────────────
            private static GraphicsPath Yuvarlak(RectangleF r, float yaricap)
            {
                var p = new GraphicsPath(); float d = yaricap * 2;
                p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                p.CloseFigure(); return p;
            }

            private static void Dolgu(Graphics g, RectangleF r, Color c, float yaricap, Color? kenar = null, float kalin = 1f)
            {
                using (var p = Yuvarlak(r, yaricap))
                {
                    using (var b = new SolidBrush(c)) g.FillPath(b, p);
                    if (kenar.HasValue) using (var pen = new Pen(kenar.Value, kalin)) g.DrawPath(pen, p);
                }
            }

            private static void Yazi(Graphics g, string s, Font f, Color c, RectangleF r, StringAlignment h = StringAlignment.Near, StringAlignment v = StringAlignment.Center)
            {
                using (var sf = new StringFormat { Alignment = h, LineAlignment = v, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
                using (var b = new SolidBrush(c)) g.DrawString(s, f, b, r, sf);
            }

            private static readonly string Sirali = "AKQJT98765432";

            private static string[] ElSatirlari(ulong el)
            {
                var s = new string[4];
                for (int r = 0; r < 4; r++)
                {
                    var sb = new System.Text.StringBuilder();
                    for (int k = 12; k >= 0; k--)
                        if ((el & (1UL << (r * 16 + k))) != 0) { char ch = Sirali[12 - k]; sb.Append(ch == 'T' ? "10" : ch.ToString()); sb.Append(' '); }
                    s[r] = sb.Length == 0 ? "—" : sb.ToString().TrimEnd();
                }
                return s;
            }

            private static int Hcp(ulong el)
            {
                int h = 0;
                for (int r = 0; r < 4; r++)
                    for (int k = 9; k <= 12; k++)
                        if ((el & (1UL << (r * 16 + k))) != 0) h += k - 8;
                return h;
            }

            private static readonly string[] Harf = { "N", "E", "S", "W" };
            private static readonly string[] KoltukAd = { "Kuzey", "Doğu", "Güney", "Batı" };

            // ── ana çizim ─────────────────────────────────────────────
            public void Ciz(Graphics g)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.Clear(Zemin);

                // başlık
                Yazi(g, _f._baslik, _fBaslik, Metin, new RectangleF(24, 14, 560, 30));
                string kaynak = _f._tablo == null ? "Hesaplanıyor…" : _f._tablo.Kaynak + " • " + (_f._tablo.Ms < 1000 ? _f._tablo.Ms.ToString("0") + " ms" : (_f._tablo.Ms / 1000).ToString("0.0") + " sn");
                var rk = new RectangleF(Genislik - 24 - 200, 16, 200, 26);
                Dolgu(g, rk, Panel, 13, Cizgi);
                Yazi(g, kaynak, _fKucuk, _f._tablo == null ? Altin : Soluk, rk, StringAlignment.Center);

                CizEller(g, new RectangleF(24, 56, 360, 330));
                CizTablo(g, new RectangleF(404, 56, 532, 330));
                CizSonuc(g, new RectangleF(24, 400, Genislik - 48, Yukseklik - 400 - 16));
            }

            // dört el, pusula düzeninde
            private void CizEller(Graphics g, RectangleF r)
            {
                Dolgu(g, r, Panel, 14, Cizgi);
                float hw = 124, hh = 92;
                var yer = new[]
                {
                    new PointF(r.X + (r.Width - hw) / 2, r.Y + 12),               // N
                    new PointF(r.Right - hw - 8, r.Y + (r.Height - hh) / 2),        // E
                    new PointF(r.X + (r.Width - hw) / 2, r.Bottom - hh - 12),       // S
                    new PointF(r.X + 8, r.Y + (r.Height - hh) / 2)                  // W
                };
                // ortadaki dağıtıcı / zon göstergesi
                var mr = new RectangleF(r.X + r.Width / 2 - 32, r.Y + r.Height / 2 - 32, 64, 64);
                Dolgu(g, mr, Color.FromArgb(18, 32, 25), 10, Cizgi);
                for (int k = 0; k < 4; k++)
                {
                    bool ns = (k & 1) == 0; bool zon = ns ? _f._nsZon : _f._ewZon;
                    var c = new RectangleF(0, 0, 26, 18);
                    switch (k)
                    {
                        case 0: c.Location = new PointF(mr.X + 19, mr.Y + 2); break;
                        case 1: c.Location = new PointF(mr.Right - 27, mr.Y + 23); break;
                        case 2: c.Location = new PointF(mr.X + 19, mr.Bottom - 20); break;
                        default: c.Location = new PointF(mr.X + 1, mr.Y + 23); break;
                    }
                    bool dag = k == _f._dagitici;
                    Dolgu(g, c, zon ? Color.FromArgb(190, 50, 50) : Color.FromArgb(40, 60, 50), 6, dag ? Altin : (Color?)null, dag ? 1.6f : 1f);
                    Yazi(g, Harf[k], _fKucuk, Metin, c, StringAlignment.Center);
                }
                for (int k = 0; k < 4; k++)
                {
                    var satir = ElSatirlari(_f._eller[k]);
                    var kr = new RectangleF(yer[k].X, yer[k].Y, hw, hh);
                    bool ns = (k & 1) == 0;
                    Yazi(g, KoltukAd[k] + "  " + Hcp(_f._eller[k]) + " HP", _fKucuk, ns ? NsRenk : EwRenk, new RectangleF(kr.X, kr.Y - 2, hw, 16));
                    for (int s = 0; s < 4; s++)
                    {
                        var yr = new RectangleF(kr.X, kr.Y + 16 + s * 18, hw, 18);
                        Yazi(g, ParHesabi.KozSembol[s], _fSembol, RenkRenkleri[s], new RectangleF(yr.X, yr.Y - 2, 22, 22));
                        Yazi(g, satir[s], _fEl, Metin, new RectangleF(yr.X + 22, yr.Y, hw - 22, 18));
                    }
                }
            }

            private static readonly int[] TabloSirasi = { 4, 0, 1, 2, 3 };   // NT ♠ ♥ ♦ ♣

            private static Color HucreRengi(int koz, int lov)
            {
                int sev = lov - 6;
                int oyun = koz == 4 ? 3 : koz <= 1 ? 4 : 5;
                if (lov < 0) return Color.FromArgb(40, 52, 46);
                if (sev <= 0) return Color.FromArgb(38, 52, 45);
                if (sev == 7) return Color.FromArgb(150, 60, 180);
                if (sev >= 6) return Color.FromArgb(196, 84, 56);
                if (sev >= oyun) return Color.FromArgb(176, 140, 36);
                return Color.FromArgb(52, 96, 78);
            }

            private void CizTablo(Graphics g, RectangleF r)
            {
                Dolgu(g, r, Panel, 14, Cizgi);
                Yazi(g, "Çift Dummy Tablosu", _fOrta, Metin, new RectangleF(r.X + 16, r.Y + 8, 260, 24));
                Yazi(g, _f._seviyeGoster ? "hücre = yapılan seviye" : "hücre = deklaranın löveleri", _fKucuk, Soluk, new RectangleF(r.Right - 236, r.Y + 8, 220, 24), StringAlignment.Far);

                float x0 = r.X + 64, y0 = r.Y + 56, cw = 74, ch = 46, gap = 4;
                // başlıklar
                for (int k = 0; k < 4; k++)
                    Yazi(g, Harf[k], _fOrta, (k & 1) == 0 ? NsRenk : EwRenk, new RectangleF(x0 + k * (cw + gap), y0 - 26, cw, 22), StringAlignment.Center);
                Yazi(g, "N/S", _fKucuk, NsRenk, new RectangleF(x0 + 4 * (cw + gap) + 8, y0 - 26, 50, 22), StringAlignment.Center);
                Yazi(g, "E/W", _fKucuk, EwRenk, new RectangleF(x0 + 4 * (cw + gap) + 8 + 52, y0 - 26, 50, 22), StringAlignment.Center);

                var t = _f._tablo;
                for (int ri = 0; ri < 5; ri++)
                {
                    int koz = TabloSirasi[ri];
                    float y = y0 + ri * (ch + gap);
                    string sem = koz == 4 ? "NT" : ParHesabi.KozSembol[koz];
                    Yazi(g, sem, koz == 4 ? _fOrta : _fSembol, koz == 4 ? Metin : RenkRenkleri[koz], new RectangleF(r.X + 12, y, 46, ch), StringAlignment.Center);
                    for (int k = 0; k < 4; k++)
                    {
                        var cr = new RectangleF(x0 + k * (cw + gap), y, cw, ch);
                        int lov = t == null ? -1 : t.Al(koz, k);
                        Dolgu(g, cr, HucreRengi(koz, lov), 9);
                        string s = lov < 0 ? "·" : _f._seviyeGoster ? (lov - 6 >= 1 ? (lov - 6).ToString() : "–") : lov.ToString();
                        Yazi(g, s, _fHucre, lov - 6 >= 1 ? Color.White : Soluk, cr, StringAlignment.Center);
                        if (_f._par != null && !_f._par.Pas && _f._par.Koz == koz && _f._par.Deklaran == k)
                        {
                            using (var p = Yuvarlak(RectangleF.Inflate(cr, 2, 2), 10))
                            using (var pen = new Pen(Altin, 2f) { DashStyle = DashStyle.Dash }) g.DrawPath(pen, p);
                            Yazi(g, "PAR", _fKucuk, Altin, new RectangleF(cr.X, cr.Bottom - 15, cr.Width - 4, 14), StringAlignment.Far);
                        }
                        if (_f._sonuc != null && !_f._sonuc.Pas && _f._sonuc.Koz == koz && _f._sonuc.Deklaran == k)
                        {
                            using (var p = Yuvarlak(RectangleF.Inflate(cr, 4, 4), 11))
                            using (var pen = new Pen(Color.White, 2.4f)) g.DrawPath(pen, p);
                            Yazi(g, "OYNANAN", _fKucuk, Color.White, new RectangleF(cr.X + 4, cr.Y + 1, cr.Width, 13), StringAlignment.Near);
                        }
                    }
                    // taraf gücü çubukları
                    for (int sd = 0; sd < 2; sd++)
                    {
                        float bx = x0 + 4 * (cw + gap) + 8 + sd * 52;
                        int best = t == null ? -1 : t.TarafLove(koz, sd);
                        var br = new RectangleF(bx, y + 6, 46, ch - 12);
                        Dolgu(g, br, Color.FromArgb(18, 30, 24), 6);
                        if (best >= 0)
                        {
                            float h = (br.Height) * Math.Max(0, best) / 13f;
                            var fr = new RectangleF(br.X, br.Bottom - h, br.Width, h);
                            if (h > 1) Dolgu(g, fr, Color.FromArgb(120, (sd == 0 ? NsRenk : EwRenk)), 6);
                            Yazi(g, best.ToString(), _fEtiket, Metin, br, StringAlignment.Center);
                        }
                    }
                }
                // dipnot: zon
                string zon = (_f._nsZon ? "N/S zonda" : "N/S zonda değil") + "  •  " + (_f._ewZon ? "E/W zonda" : "E/W zonda değil") + "  •  Dağıtıcı: " + KoltukAd[_f._dagitici];
                Yazi(g, zon, _fKucuk, Soluk, new RectangleF(r.X + 16, r.Bottom - 26, r.Width - 32, 20));
            }

            private void CizSonuc(Graphics g, RectangleF r)
            {
                Dolgu(g, r, Panel, 14, Cizgi);
                float colW = (r.Width - 48) / 3f;
                var par = _f._par; var d = _f._deg;

                // 1) PAR
                var pr = new RectangleF(r.X + 16, r.Y + 12, colW, r.Height - 24);
                Yazi(g, "PAR", _fKucuk, Soluk, new RectangleF(pr.X, pr.Y, 120, 18));
                if (par == null) Yazi(g, "…", _fBuyuk, Soluk, new RectangleF(pr.X, pr.Y + 18, pr.Width, 44));
                else
                {
                    string sk = par.Pas ? "0" : (par.NsSkor > 0 ? "+" : "") + par.NsSkor;
                    Yazi(g, sk, _fBuyuk, par.NsSkor >= 0 ? NsRenk : EwRenk, new RectangleF(pr.X, pr.Y + 16, 130, 44));
                    Yazi(g, par.Pas ? "N/S görüşüyle" : "N/S görüşüyle", _fKucuk, Soluk, new RectangleF(pr.X + 2, pr.Y + 58, 130, 16));
                    string metin = par.Pas ? "Dört pas" : (par.Taraf == 0 ? "N/S " : "E/W ") + par.KontratMetni + " (" + Harf[par.Deklaran] + ")" +
                        (par.Fedakar ? "  " + (par.Lov - par.Seviye - 6) + " batış" : "  yapılır");
                    Yazi(g, metin, _fOrta, Metin, new RectangleF(pr.X + 134, pr.Y + 22, pr.Width - 134, 32));
                }

                // 2) OYNANAN KONTRAT
                var or = new RectangleF(pr.Right + 16, pr.Y, colW, pr.Height);
                Yazi(g, "OYNANAN KONTRAT", _fKucuk, Soluk, new RectangleF(or.X, or.Y, 160, 18));
                if (_f._sonuc == null) Yazi(g, "İhale bilgisi yok", _fEtiket, Soluk, new RectangleF(or.X, or.Y + 24, or.Width, 24));
                else if (_f._sonuc.Pas) Yazi(g, "Pas", _fBuyuk, Metin, new RectangleF(or.X, or.Y + 16, or.Width, 44));
                else if (d != null)
                {
                    string kt = ParHesabi.KontratMetni(d.Seviye, d.Koz, d.Kontr);
                    Yazi(g, kt, _fBuyuk, Metin, new RectangleF(or.X, or.Y + 16, 110, 44));
                    string dl = Harf[d.Deklaran] + "  •  DD: " + d.DdLove + " löve";
                    Yazi(g, dl, _fEtiket, Metin, new RectangleF(or.X + 112, or.Y + 20, or.Width - 112, 22));
                    string ds = "K-G " + (d.DdNsSkor >= 0 ? "+" : "") + d.DdNsSkor;
                    Yazi(g, ds, _fKucuk, Soluk, new RectangleF(or.X + 112, or.Y + 40, or.Width - 112, 20));
                }

                // 3) DEĞERLENDİRME
                var vr = new RectangleF(or.Right + 16, pr.Y, colW, pr.Height);
                Yazi(g, "DEĞERLENDİRME", _fKucuk, Soluk, new RectangleF(vr.X, vr.Y, 160, 18));
                if (d != null)
                {
                    Color rozet = d.Hata == IhaleHatasi.Yok || d.Hata == IhaleHatasi.ParUstu ? Color.FromArgb(46, 160, 100)
                                : d.Hata == IhaleHatasi.KucukFark ? Color.FromArgb(120, 130, 60) : Color.FromArgb(190, 70, 60);
                    var rr = new RectangleF(vr.X, vr.Y + 20, vr.Width, 28);
                    Dolgu(g, rr, rozet, 14);
                    Yazi(g, d.Baslik, _fOrta, Color.White, new RectangleF(rr.X + 12, rr.Y, rr.Width - 24, rr.Height));
                    int imp = d.ImpNs;
                    string it = d.Pas ? "" : "IMP: " + (d.ImpDeklaran > 0 ? "+" : "") + d.ImpDeklaran + " (deklaran tarafı)";
                    Yazi(g, it, _fEtiket, d.ImpDeklaran < 0 ? EwRenk : NsRenk, new RectangleF(vr.X, vr.Y + 52, vr.Width, 20));
                }
                else if (par != null && _f._sonuc == null) Yazi(g, "—", _fEtiket, Soluk, new RectangleF(vr.X, vr.Y + 20, vr.Width, 24));

                // notlar + ihale
                float ny = pr.Y + 82;
                var satirlar = new List<string>();
                if (d != null) satirlar.AddRange(d.Notlar);
                if (!string.IsNullOrEmpty(_f._ihaleMetni)) satirlar.Add("İhale: " + _f._ihaleMetni);
                foreach (var s in satirlar.Take(5))
                {
                    Yazi(g, "• " + s, _fEtiket, Metin, new RectangleF(pr.X, ny, r.Width - 32, 20));
                    ny += 21;
                }
            }
        }
    }
}
