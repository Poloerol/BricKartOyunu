using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Forms;

namespace BricKartOyunu
{
    // Çift Dummy Analizi ve Motor Karnesi menü bağlantıları
    public partial class BricOyna
    {
        private static int FizikselIndeks(Player p)
        {
            switch (p)
            {
                case Player.Kuzey: return 0;
                case Player.Dogu: return 1;
                case Player.Guney: return 2;
                default: return 3;
            }
        }

        private static Player IcOyuncu(string ad)
        {
            switch (ad)
            {
                case "Bati": case "Batı": return Player.Bati;
                case "Kuzey": return Player.Kuzey;
                case "Dogu": case "Doğu": return Player.Dogu;
                default: return Player.Guney;
            }
        }

        private bool _ddMenuKuruldu;

        private void DDMenuKur()
        {
            if (_ddMenuKuruldu) return;
            _ddMenuKuruldu = true;
            DDMenuKurIc();
        }

        private void DDMenuKurIc()
        {
            var ust = IhaleYorumuToolStripMenuItem.OwnerItem as ToolStripMenuItem;
            if (ust == null) return;
            var analiz = new ToolStripMenuItem("Çift Dummy Analizi");
            analiz.Click += (s, e) => DDAnalizAc();
            var karne = new ToolStripMenuItem("Motor Karnesi…");
            karne.Click += (s, e) => { using (var f = new DDKarneForm()) f.ShowDialog(this); };
            ust.DropDownItems.Add(new ToolStripSeparator());
            ust.DropDownItems.Add(analiz);
            ust.DropDownItems.Add(karne);
        }

        private void DDAnalizAc()
        {
            if (_sonIhaleGecmisi == null || _sonIhaleGecmisi.Count == 0)
            {
                MessageBox.Show(this, "Önce bir bord dağıtılıp ihale yapılmalı.", "Çift Dummy Analizi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // başlangıçtaki eller = eldeki kartlar + oynanmış kartlar (fiziksel koltuk sırası N E S W)
            var eller = new Dictionary<Player, List<Card>>();
            foreach (var p in new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati })
            {
                var liste = new List<Card>();
                var el = OyuncuEli(p);
                if (el != null) liste.AddRange(el);
                foreach (var k in _oynananTumEller.Where(t => t.Oyuncu == p).Select(t => t.Kart))
                    if (!liste.Contains(k)) liste.Add(k);
                eller[KoltukHaritasi.Fiz(p)] = liste;
            }
            if (eller.Values.Any(l => l.Count != 13))
            {
                MessageBox.Show(this, "Eller henüz tam değil (13'er kart bulunamadı).", "Çift Dummy Analizi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var maske = DDMotor.Maskeler(eller);
            int dagitici = FizikselIndeks(KoltukHaritasi.Fiz(IcOyuncu(_ihaleBaslangicOyuncusu)));
            bool ns, ew; ZonDurumu(out ns, out ew);
            if (KoltukHaritasi.CiftlerTers) { bool g = ns; ns = ew; ew = g; }   // fiziksel zon
            var sonuc = IhaleSonucu.Coz(_sonIhaleGecmisi, dagitici);
            string metin = string.Join(" ", _sonIhaleGecmisi);
            using (var f = new DDAnalizForm(maske, dagitici, ns, ew, sonuc, metin, "Bord " + _currentTour + " — Çift Dummy Analizi"))
                f.ShowDialog(this);
        }
    }
}
