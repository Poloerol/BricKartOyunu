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
            var ihaleAciklama = new ToolStripMenuItem("Teklif Açıklamaları…");
            ihaleAciklama.Click += (s, e) => IhaleAciklamaAc();
            ust.DropDownItems.Add(ihaleAciklama);
            var oyunAnaliz = new ToolStripMenuItem("Oyun Analizi (hamle hamle)…");
            oyunAnaliz.Click += (s, e) => OyunAnaliziAc();
            ust.DropDownItems.Add(oyunAnaliz);
            var oynama = new ToolStripMenuItem("Oynama Karnesi…");
            oynama.Click += (s, e) => { using (var f = new OynamaKarneForm()) f.ShowDialog(this); };
            ust.DropDownItems.Add(oynama);
        }

        /// <summary>Eylemler > Double Dummy Search: oynanmakta olan eldeki pozisyonu çift dummy arama penceresine verir.</summary>
        private void DDAramaAc()
        {
            if (_oyunAsamasi != OyunAsamasi.Oyun || string.IsNullOrEmpty(_kontrat))
            {
                MessageBox.Show(this, "Çift dummy arama için kart oynanmaya başlamış olmalı (ihale bitmeli).", "Çift Dummy Arama", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // koltuk sırası (iç koltuklar): Kuzey, Doğu, Güney, Batı; kartlar: kalan eller (masadaki kartlar hariç)
            var eller = new ulong[4];
            var sira = new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            for (int k = 0; k < 4; k++)
            {
                var el = OyuncuEli(sira[k]);
                if (el == null) continue;
                foreach (var c in el) eller[k] |= 1UL << PimcOyuncu.Kod(c);
            }
            int masaSayisi = _buEldeOynananlar.Count;
            if (masaSayisi >= 4) return;   // el tamamlanmış, sonuç bekleniyor
            int lider = masaSayisi > 0 ? PimcOyuncu.Koltuk(_buEldeOynananlar[0].Oyuncu) : PimcOyuncu.Koltuk(activePlayer);
            var masa = new int[4];
            for (int i = 0; i < masaSayisi; i++) masa[i] = PimcOyuncu.Kod(_buEldeOynananlar[i].Kart);
            string koz = KontratKozu();
            int kozNo = koz == null ? -1 : Array.IndexOf(OyunKurallari.Renkler, koz);

            using (var f = new DDAramaForm(eller, lider, masa, masaSayisi, kozNo, k => KoltukHaritasi.Ad(sira[k & 3])))
            {
                if (playZonePanel != null)
                    f.MerkezEkran = playZonePanel.PointToScreen(new System.Drawing.Point(playZonePanel.Width / 2, playZonePanel.Height / 2));
                f.ShowDialog(this);
            }
        }

        /// <summary>Eylemler > Teklif Açıklamaları: son ihaleyi tüm ellerle yeniden oynatıp her teklifin anlamını gösterir.</summary>
        private void IhaleAciklamaAc()
        {
            if (_sonIhaleGecmisi == null || _sonIhaleGecmisi.Count == 0)
            {
                MessageBox.Show(this, "Önce bir bord dağıtılıp ihale yapılmalı.", "Teklif Açıklamaları", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
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
                eller[p] = liste;
            }
            if (eller.Values.Any(l => l.Count != 13))
            {
                MessageBox.Show(this, "Eller tam değil (13'er kart bulunamadı); açıklama yapılamıyor.", "Teklif Açıklamaları", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var f = new IhaleAciklamaForm(new List<string>(_sonIhaleGecmisi), IcOyuncu(_ihaleBaslangicOyuncusu), eller, p => KoltukHaritasi.Ad(p), "Bord " + _currentTour))
                f.ShowDialog(this);
        }

        /// <summary>Eylemler > Oyun Analizi: o ana kadar oynanan her kartı çift dummy ile değerlendirir (hamle hamle kayıp).</summary>
        private void OyunAnaliziAc()
        {
            var oynananlar = new List<Tuple<Player, Card>>();
            foreach (var t in _oynananTumEller) oynananlar.Add(Tuple.Create(t.Oyuncu, t.Kart));
            foreach (var t in _buEldeOynananlar) oynananlar.Add(Tuple.Create(t.Oyuncu, t.Kart));
            if (oynananlar.Count == 0 || string.IsNullOrEmpty(_kontrat))
            {
                MessageBox.Show(this, "Oyun analizi için en az bir kart oynanmış olmalı (ihale bitip atak yapılmalı).", "Oyun Analizi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // oyun başlamadan önceki eller = eldeki kartlar + oynanmış kartlar (iç koltuklar; DD yalnız çift/tek koltuk ilişkisini kullanır)
            var eller = new Dictionary<Player, List<Card>>();
            foreach (var p in new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati })
            {
                var liste = new List<Card>();
                var el = OyuncuEli(p);
                if (el != null) liste.AddRange(el);
                foreach (var k in oynananlar.Where(t => t.Item1 == p).Select(t => t.Item2))
                    if (!liste.Contains(k)) liste.Add(k);
                eller[p] = liste;
            }
            if (eller.Values.Any(l => l.Count != 13))
            {
                MessageBox.Show(this, "Eller tam değil (13'er kart bulunamadı); analiz yapılamıyor.", "Oyun Analizi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string baslik = "Bord " + _currentTour + " — " + _kontrat;
            using (var f = new OyunAnaliziForm(eller, oynananlar, KontratKozu(), _kontratDeklaran, baslik, p => KoltukHaritasi.Ad(p)))
                f.ShowDialog(this);
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
