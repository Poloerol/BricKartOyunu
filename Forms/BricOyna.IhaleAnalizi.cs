using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Forms;

namespace BricKartOyunu
{
    // Eylemler > İhale Analizi: Par kontratı, çift dummy tablosu, en yüksek/en iyi skorlu kontratlar (Bridge Baron "Par Contract")
    public partial class BricOyna
    {
        private bool _ihaleAnaliziMenusuKuruldu;

        private void IhaleAnaliziMenusunuKur()
        {
            if (_ihaleAnaliziMenusuKuruldu) return;
            _ihaleAnaliziMenusuKuruldu = true;
            var ust = eylmlerToolStripMenuItem;
            if (ust == null) return;
            var oge = new ToolStripMenuItem("İhale Analizi");
            oge.Click += (s, e) => IhaleAnaliziAc();
            int konum = ust.DropDownItems.IndexOf(doubleDummySearchToolStripMenuItem1);
            if (konum >= 0) ust.DropDownItems.Insert(konum + 1, oge); else ust.DropDownItems.Add(oge);
        }

        private void IhaleAnaliziAc()
        {
            var acik = Application.OpenForms.OfType<IhaleAnaliziForm>().FirstOrDefault();
            if (acik != null) { acik.Close(); }

            // tam eller = eldeki kartlar + bu bordda oynanmış kartlar (fiziksel koltuk sırası N E S W)
            var eller = new Dictionary<Player, List<Card>>();
            foreach (var p in new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati })
            {
                var liste = new List<Card>();
                var el = OyuncuEli(p);
                if (el != null) liste.AddRange(el);
                foreach (var k in _oynananTumEller.Where(t => t.Oyuncu == p).Select(t => t.Kart))
                    if (!liste.Contains(k)) liste.Add(k);
                foreach (var k in _buEldeOynananlar.Where(t => t.Oyuncu == p).Select(t => t.Kart))
                    if (!liste.Contains(k)) liste.Add(k);
                eller[KoltukHaritasi.Fiz(p)] = liste;
            }
            if (eller.Values.Any(l => l.Count != 13))
            {
                MessageBox.Show(this, "Eller henüz tam değil (13'er kart bulunamadı). Önce bir bord dağıtılmalı.", "İhale Analizi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ParAnalizi analiz;
            try
            {
                Cursor = Cursors.WaitCursor;
                var tablo = DDMotor.Hesapla(DDMotor.Maskeler(eller));
                int dagitici = FizikselIndeks(KoltukHaritasi.Fiz(_dealer));
                bool ns, ew; ZonDurumu(out ns, out ew);
                if (KoltukHaritasi.CiftlerTers) { bool g = ns; ns = ew; ew = g; }   // fiziksel zon
                analiz = ParAnalizi.Hesapla(tablo, dagitici, ns, ew);
            }
            finally { Cursor = Cursors.Default; }
            var f = new IhaleAnaliziForm(analiz, "Bord " + _currentTour);
            // Oyun masasının (oyun alanı panelinin) tam ortasında açılır; ekrandan taşarsa çalışma alanına toplanır
            f.StartPosition = FormStartPosition.Manual;
            if (playZonePanel != null)
            {
                var merkez = playZonePanel.PointToScreen(new System.Drawing.Point(playZonePanel.Width / 2, playZonePanel.Height / 2));
                var alan = Screen.FromControl(this).WorkingArea;
                int x = Math.Max(alan.Left, Math.Min(merkez.X - f.Width / 2, alan.Right - f.Width));
                int y = Math.Max(alan.Top, Math.Min(merkez.Y - f.Height / 2, alan.Bottom - f.Height));
                f.Location = new System.Drawing.Point(x, y);
            }
            else f.StartPosition = FormStartPosition.CenterParent;
            f.Show(this);
        }
    }
}
