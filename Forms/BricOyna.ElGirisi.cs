using BricKartOyunu.Class;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BricKartOyunu
{
    // Kullanıcı girişi dağıtımı: eller elle girilir, dağıtıcı/zon seçilir; elleri saat yönünde döndürme
    public partial class BricOyna
    {
        // Elle girilen deste: iç koltuk sırası Kuzey, Doğu, Güney, Batı (13'er kart; DagitimiUret ile aynı biçim)
        private List<string> _manuelDeste;

        // Kullanıcının seçtiği zon (fiziksel pusula): 0 zonsuz, 1 K/G, 2 D/B, 3 herkes. null: bord numarasından.
        private int? _manuelZon;

        /// <summary>Oyun Planı'nda "Kullanıcı girişi" seçili mi (pratik modunda hiçbir zaman)?</summary>
        private bool ManuelGirisModu()
        {
            if (PratikModu) return false;
            var plan = AnaSayfa.OyunPlani ?? new OyunPlaniSecenekleri();
            return plan.DagitimKaynagi == DagitimKaynagi.KullaniciGirisi;
        }

        /// <summary>
        /// Kullanıcı girişi seçiliyse El Girişi ve Dağıtıcı/Zon formlarını açar; eller, dağıtıcı ve zon saklanır.
        /// Kullanıcı girişi seçili değilse eski elle giriş verisini temizler. El Girişi'nde İptal: false döner.
        /// </summary>
        private bool ManuelGirisAl()
        {
            if (!ManuelGirisModu()) { _manuelDeste = null; _manuelZon = null; return true; }

            Dictionary<Player, List<string>> eller;
            using (var giris = new ElGirisiForm())
            {
                giris.EllerDegisti += ManuelOnizleme;   // girilen kartlar masada canlı görünür
                ManuelOnizleme(new Dictionary<Player, List<string>>());   // masa boş başlar
                if (giris.ShowDialog(this) != DialogResult.OK) return false;
                eller = giris.Eller;
            }

            bool ns, ew;
            FizikselZon(out ns, out ew);
            using (var dz = new DagiticiZonForm(KoltukHaritasi.Fiz(_dealer), (ns ? 1 : 0) | (ew ? 2 : 0)))
            {
                dz.ShowDialog(this);
                _dealer = KoltukHaritasi.Ic(dz.Dagitici);
                _manuelZon = dz.Zon;
            }

            var deste = new List<string>(52);
            foreach (var ic in new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati })
                deste.AddRange(eller[KoltukHaritasi.Fiz(ic)]);
            _manuelDeste = deste;
            return true;
        }

        /// <summary>El Girişi açıkken: o ana kadar girilen kartları dört elde de açık olarak masada gösterir.</summary>
        private void ManuelOnizleme(Dictionary<Player, List<string>> eller)
        {
            string kartSeti = AnaSayfa.SeciliKartSeti;
            Action<List<Card>, Player> doldur = (liste, ic) =>
            {
                liste.Clear();
                List<string> dosyalar;
                if (!eller.TryGetValue(KoltukHaritasi.Fiz(ic), out dosyalar)) return;
                var gecici = dosyalar.Select(f => ParseCard(f, kartSeti)).ToList();
                liste.AddRange(CardSorter.SiraliEl(gecici));
            };

            this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
            try
            {
                doldur(kuzeyEl, Player.Kuzey);
                doldur(doguEl, Player.Dogu);
                doldur(guneyEl, Player.Guney);
                doldur(batiEl, Player.Bati);
                HepsiniGoster();
            }
            finally
            {
                this.ResumeLayout(true); EkranYardimci.CizimiAc(this);
            }
        }

        /// <summary>Elle girilen eller ve dağıtıcı kaydedildikten sonra masayı yeniden kurar (ilk açılış).</summary>
        private void ManuelYenidenDagit()
        {
            activePlayer = _dealer;
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;
            _kontrat = null;
            _kontratDeklaran = _dealer;
            KartlariDagit(AnaSayfa.SeciliKartSeti);
            UpdateInfoBoard(_dealer);
            UpdatePlayerBadges();
            OyunBilgiPaneliniGuncelle();
        }

        /// <summary>Zon (K/G, D/B) fiziksel pusulaya göre: kullanıcı seçtiyse o, yoksa 16'lı bord döngüsü.</summary>
        private void FizikselZon(out bool ns, out bool ew)
        {
            if (_manuelZon.HasValue)
            {
                ns = (_manuelZon.Value & 1) != 0;
                ew = (_manuelZon.Value & 2) != 0;
                return;
            }
            int boardMod = ((_currentTour - 1) % 16) + 1;
            ns = (boardMod == 2 || boardMod == 5 || boardMod == 12 || boardMod == 15 ||
                  boardMod == 4 || boardMod == 7 || boardMod == 10 || boardMod == 16);
            ew = (boardMod == 3 || boardMod == 6 || boardMod == 9 || boardMod == 13 ||
                  boardMod == 4 || boardMod == 7 || boardMod == 10 || boardMod == 16);
        }

        /// <summary>
        /// Elleri saat yönünde bir koltuk kaydırır: Güney'in eli Batı'ya, Batı'nınki Kuzey'e,
        /// Kuzey'inki Doğu'ya, Doğu'nunki Güney'e geçer (kutular ekran konumlarıdır).
        /// </summary>
        public void ElleriSaatYonundeDondur()
        {
            var g = new List<Card>(guneyEl);
            var b = new List<Card>(batiEl);
            var k = new List<Card>(kuzeyEl);
            var d = new List<Card>(doguEl);

            guneyEl.Clear(); guneyEl.AddRange(d);
            batiEl.Clear(); batiEl.AddRange(g);
            kuzeyEl.Clear(); kuzeyEl.AddRange(b);
            doguEl.Clear(); doguEl.AddRange(k);

            this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
            try
            {
                SadeceGuneyiGoster();
            }
            finally
            {
                this.ResumeLayout(true); EkranYardimci.CizimiAc(this);
            }
        }
    }
}
