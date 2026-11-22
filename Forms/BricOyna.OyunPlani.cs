using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Plan;
using BricKartOyunu.Forms;

namespace BricKartOyunu
{
    // Yardım > Oyun Planı Yardımcısı: deklaran planı ve savunma (atak, sinyal) yardımı veren etkileşimli pencere.
    // Kaldırmak için bu dosyayı, Forms/OyunPlaniYardimcisiForm.cs ve Class/Plan klasörünü silip
    // BricOyna.cs'deki OyunPlaniYardimcisiMenusunuKur() çağrısını ve proje kayıtlarını kaldırmak yeterlidir.
    public partial class BricOyna
    {
        private bool _oyunPlaniMenusuKuruldu;
        private OyunPlaniYardimcisiForm _oyunPlaniPenceresi;

        private void OyunPlaniYardimcisiMenusunuKur()
        {
            if (_oyunPlaniMenusuKuruldu) return;
            _oyunPlaniMenusuKuruldu = true;
            var ust = yardımToolStripMenuItem;
            if (ust == null) return;
            var oge = new ToolStripMenuItem("Oyun Planı Yardımcısı");
            oge.Click += (s, e) => OyunPlaniYardimcisiAc();
            ust.DropDownItems.Add(oge);
        }

        /// <summary>Pencerenin her hesabında UI iş parçacığında çağrılır; anlık görüntüyü kopyalar.</summary>
        private OyunPlaniDurumu OyunPlaniDurumuAl()
        {
            var d = new OyunPlaniDurumu();
            if (_oyunAsamasi != OyunAsamasi.Oyun || string.IsNullOrEmpty(_kontrat))
            {
                d.Mesaj = "İhale bitip oyun başlayınca kullanılabilir.";
                return d;
            }
            Player dek = _kontratDeklaran;
            Player dummy = IhaleDurumu.PartnerBul(dek);
            Player ben = Player.Guney;

            if (ben == dek || ben == dummy)
            {
                if (!_dummyAcildi || !OyuncuEliAcikMi(dek) || !OyuncuEliAcikMi(dummy))
                {
                    d.Mesaj = "Dummy açılınca (atak yapıldıktan sonra) deklaran planı hazır olur.";
                    return d;
                }
                d.Mod = 1;
                d.Deklaran = new PlanGirdisi
                {
                    Kontrat = _kontrat,
                    Koz = KontratKozu(),
                    Seviye = KontratSeviyesi(),
                    Deklaran = dek,
                    Dummy = dummy,
                    DeklaranEli = new List<Card>(OyuncuEli(dek) ?? new List<Card>()),
                    DummyEli = new List<Card>(OyuncuEli(dummy) ?? new List<Card>()),
                    Masa = new List<(Player Oyuncu, Card Kart)>(_buEldeOynananlar),
                    Gecmis = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>(_oynananTumEller),
                    IhaleGecmisi = _sonIhaleGecmisi == null ? new List<string>() : new List<string>(_sonIhaleGecmisi),
                    IhaleBaslangici = IcOyuncu(_ihaleBaslangicOyuncusu),
                    Ad = p => KoltukHaritasi.Ad(p)
                };
                return d;
            }

            // Savunma: yalnız kendi elim + (açıldıysa) dummy bilinir
            d.Mod = 2;
            d.Savunma = new SavunmaGirdisi
            {
                Kontrat = _kontrat,
                Koz = KontratKozu(),
                Seviye = KontratSeviyesi(),
                Deklaran = dek,
                Dummy = dummy,
                Ben = ben,
                Ortak = OyunKurallari.Ortak(ben),
                Elim = new List<Card>(OyuncuEli(ben) ?? new List<Card>()),
                DummyAcik = _dummyAcildi,
                DummyEli = _dummyAcildi ? new List<Card>(OyuncuEli(dummy) ?? new List<Card>()) : new List<Card>(),
                Masa = new List<(Player Oyuncu, Card Kart)>(_buEldeOynananlar),
                Gecmis = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>(_oynananTumEller),
                IhaleGecmisi = _sonIhaleGecmisi == null ? new List<string>() : new List<string>(_sonIhaleGecmisi),
                IhaleBaslangici = IcOyuncu(_ihaleBaslangicOyuncusu),
                Ad = p => KoltukHaritasi.Ad(p),
                Anlasma = AnlasmaYonetici.Mevcut(Taraf.NS)
            };
            if (activePlayer == ben && InsanKontrolundeMi(ben) && !_elBittiBekliyor && _oynananToplamEl < 13)
                d.Savunma.Baglam = OyunBaglamiOlustur(ben);
            return d;
        }

        private void OyunPlaniYardimcisiAc()
        {
            if (_oyunPlaniPenceresi != null && !_oyunPlaniPenceresi.IsDisposed)
            {
                _oyunPlaniPenceresi.Activate();
                return;
            }
            _oyunPlaniPenceresi = new OyunPlaniYardimcisiForm(OyunPlaniDurumuAl);
            _oyunPlaniPenceresi.Show(this);   // modsuz: oyun açıkken kart oynanabilir, "Yenile" ile güncellenir
        }
    }
}
