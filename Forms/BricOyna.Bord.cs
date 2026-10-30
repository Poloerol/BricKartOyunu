using BricKartOyunu.Class;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;


namespace BricKartOyunu
{
    // Bord yönetimi: dağıtım, takas, deklaran/dummy gösterimi, ihale sonrası geçiş
    public partial class BricOyna
    {
        /// <summary>
        /// Bord bittiğinde dört oyuncunun 13'er kartlık ellerini açık gösterir.
        /// Kartlar oynandıkça ellerden çıkarıldığı için eller, oynanan kart kaydından
        /// (_oynananTumEller) yeniden kurulur. Renk sırası GetSuitSirasi ile aynıdır
        /// (koz ilk grup; NT'de ♠ ♥ ♣ ♦). Masadaki son elin kartları temizlenir.
        /// </summary>
        private void BordSonuElleriniGoster()
        {
            foreach (var oyuncu in new[] { Player.Kuzey, Player.Guney, Player.Bati, Player.Dogu })
            {
                var el = OyuncuEli(oyuncu);
                if (el == null) continue;

                // Eldeki (henüz oynanmamış) kartlar + oynanmış kartlar = başlangıçtaki 13 kart.
                // Normal bitişte el boştur; Claim'de kalan kartlar elde durur.
                foreach (var k in _oynananTumEller.Where(t => t.Oyuncu == oyuncu).Select(t => t.Kart))
                    if (!el.Contains(k)) el.Add(k);
            }

            // Son elin kartları masada kalır (sonuç mesajı sırasında görülebilsin);
            // masa, mesaj kapandıktan sonra BordTamamlandiFormuAc içinde temizlenir.
            this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
            try
            {
                HepsiniGoster();
            }
            finally
            {
                this.ResumeLayout(true); EkranYardimci.CizimiAc(this);
            }

            this.Refresh();
        }

        // Bir önceki bordun oynanan elleri: yeni bord dağıtılınca da "Oynanan Eller"den görülebilsin
        private List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)> _oncekiBordEller;
        private string _oncekiBordBaslik;

        // Bord sonu akışı (sonuç mesajı → Deal Complete → yeni bord / tekrar / ana menü) BordSonuAkisi sınıfındadır.
        private BordSonuAkisi _bordSonu;

        /// <summary>Maçta bordun sonucunu (K-G tarafının skoru) kaydeder. Aynı bord tekrar oynanırsa sonuç güncellenir.</summary>
        private void MacBordSonucunuKaydet()
        {
            if (!MacDurumu.Aktif) return;
            try
            {
                var s = SkorHesapla();
                int ns = 0;
                if (!s.PasGecildi)
                {
                    bool nsDeklaran = _kontratDeklaran == Player.Kuzey || _kontratDeklaran == Player.Guney;
                    ns = nsDeklaran ? s.Toplam : -s.Toplam;
                }
                if (KoltukHaritasi.CiftlerTers) ns = -ns;   // fiziksel K-G = iç D-B
                string kontrat = s.PasGecildi ? "Pas"
                    : s.Seviye + s.Koz + (s.KontrEki != "" ? " " + s.KontrEki : "") + ", " + KoltukHaritasi.Ad(_kontratDeklaran);
                MacDurumu.Tamamla(ns, kontrat, s.PasGecildi ? -1 : s.AlinanLove, _seed);
            }
            catch { /* sonuç alınamazsa bord 0 sayılır (SonucYoksaYaz) */ }
        }

        /// <summary>Yeni bord başlamadan önce: maçta sıradaki borda geçer; maç bittiyse özeti gösterir, pencereyi kapatır ve false döner.</summary>
        private bool MacSonrakiBordaHazirla()
        {
            if (!MacDurumu.Aktif) return true;
            if (MacDurumu.SonrakiBordVar) { MacDurumu.Ilerle(); return true; }

            // Maç bitti: DealCompleteForm (Bu Elin Puanlaması / İhaleyi Göster / Oyunu Göster / Ana Menüye Dön).
            // Maç durumu pencere kapanırken (FormClosed) sıfırlanır; bord sayısı o zaman 4'e döner.
            BordSonuAkisiniHazirla().MacBitisiniGoster();
            return false;
        }

        /// <summary>13 el tamamlandığında (ya da claim sonrası) bord sonu akışını başlatır.</summary>
        private BordSonuAkisi BordSonuAkisiniHazirla()
        {
            if (_bordSonu == null)
            {
                _bordSonu = new BordSonuAkisi(this, new BordSonuBaglantisi
                {
                    // AI zamanlayıcısını durdur, "el bitti" bekleyişini kapat
                    AIDurdur = () =>
                    {
                        _aiTimer?.Stop();
                        _aiTimer?.Dispose();
                        _aiTimer = null;
                        _elBittiBekliyor = false;
                    },
                    // Dört eli aç, masayı temizle
                    ElleriAcMasayiTemizle = () =>
                    {
                        BordSonuElleriniGoster();
                        MasadakiKartlariTemizle();
                        this.Refresh();
                    },
                    // Oynanan elleri sakla (yeni bord dağıtılınca da incelenebilsin)
                    BordiSakla = () =>
                    {
                        _oncekiBordEller = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>(_oynananTumEller);
                        _oncekiBordBaslik = "Oynanan Eller — son bord (Seed " + _seed + ")";
                    },
                    Skor = SkorHesapla,
                    Deklaran = () => _kontratDeklaran,
                    OrtalaKontrol = () => playZonePanel,
                    Tekrar = BordTekrariBaslat,
                    Yeni = YeniBordBaslat
                });
            }
            return _bordSonu;
        }

        private void BordTamamlandiFormuAc()
        {
            var akis = BordSonuAkisiniHazirla();
            MacBordSonucunuKaydet();
            akis.Baslat();
        }

        /// <summary>
        /// "Go to Next Deal" basıldığında çağrılır.
        /// Yeni bord başlatır: sayaçları sıfırlar, kartları yeniden dağıtır,
        /// yeni DekBasForm açar.
        /// </summary>
        /// <summary>
        /// Replay: aynı bord (aynı seed, aynı dealer) ilk dağıtım konumuna döner,
        /// yalnızca Güney açık, DekBasForm açılır.
        /// </summary>
        /// <summary>
        /// Bir bord başlarken (yeni bord, tekrar, dört pas sonrası) bordun TÜM oyun durumunu
        /// tek yerden sıfırlar. Yeni bir bord-bazlı alan eklenirse buraya eklenmelidir.
        /// </summary>
        private void BordDurumunuSifirla()
        {
            OyunPlaniniUygula();
            // Oynanan eller ve geri al / ileri al geçmişi
            _oynananTumEller.Clear();
            OyunGecmisiniSifirla();            // geçmiş + ileri al yığını + otomatik oyun durdurma
            OynananElleriFormlariniGuncelle();   // açık pencere önceki bordu göstermeye devam eder

            // Sayaçlar ve skor girdileri
            _oynananToplamEl = 0;
            _kazanilanElNS = 0;
            _kazanilanElEW = 0;
            _kontrDurumu = KontrDurumu.Yok;

            // Masa ve el durumu
            MasadakiKartlariTemizle();
            _buEldeOynananlar.Clear();
            _buEldeAtakRengi = null;
            _elBittiBekliyor = false;
            _dummyAcildi = false;

            // Eller ihale sonrası takas edildiyse (Kuzey deklaran) bayrağı geri al
            _ellerIhaleSonrasiTakasli = false;

            // Önceki bordun ihale kaydı yeni bordda görünmesin
            _sonIhaleGecmisi = new List<string>();
        }

        /// <summary>
        /// Oyun Planı'ndaki dağıtıcı ayarına göre (_currentTour'a bakarak) iç dağıtıcıyı belirler
        /// ve bord numarasını kalıcı olarak kaydeder.
        /// </summary>
        private void DagiticiyiAyarla()
        {
            if (PratikModu)
            {
                _dealer = KonvansiyonPratigi.Dagitici(PratikDagitimlari[_pratikIndeks].Seed);
                return;
            }
            var plan = AnaSayfa.OyunPlani ?? new OyunPlaniSecenekleri();
            _dealer = KoltukHaritasi.Ic(plan.FizikselDagitici(_currentTour));
            try
            {
                if (plan.SonBordNo != _currentTour)
                {
                    plan.SonBordNo = _currentTour;
                    plan.Kaydet();
                }
            }
            catch { /* kayıt başarısızsa oyun sürer */ }
        }

        /// <summary>"En iyi el" ayarını iç koltuk dizisine (Kuzey, Doğu, Güney, Batı) çevirir; null = fark etmez.</summary>
        private bool[] EnIyiElIzinli(EnIyiEl secim)
        {
            Player[] fizikselListe;
            switch (secim)
            {
                case EnIyiEl.KuzeyGuney: fizikselListe = new[] { Player.Kuzey, Player.Guney }; break;
                case EnIyiEl.DoguBati: fizikselListe = new[] { Player.Dogu, Player.Bati }; break;
                case EnIyiEl.Kuzey: fizikselListe = new[] { Player.Kuzey }; break;
                case EnIyiEl.Dogu: fizikselListe = new[] { Player.Dogu }; break;
                case EnIyiEl.Guney: fizikselListe = new[] { Player.Guney }; break;
                case EnIyiEl.Bati: fizikselListe = new[] { Player.Bati }; break;
                default: return null;
            }
            Player[] sira = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            var izin = new bool[4];
            foreach (var f in fizikselListe)
                izin[Array.IndexOf(sira, KoltukHaritasi.Ic(f))] = true;
            return izin;
        }

        /// <summary>Dağıtım kaynağı ve en iyi el ayarlarına göre 52 kartı üretir (Kuzey, Doğu, Güney, Batı).</summary>
        private List<string> DagitimiUret()
        {
            if (PratikModu) return KonvansiyonPratigi.Deste(PratikDagitimlari[_pratikIndeks]);
            if (_manuelDeste != null && _manuelDeste.Count == 52 && ManuelGirisModu()) return new List<string>(_manuelDeste);   // Kullanıcı girişi
            var plan = AnaSayfa.OyunPlani ?? new OyunPlaniSecenekleri();
            DagitimProfili profil;
            int seed = _seed;
            switch (plan.DagitimKaynagi)
            {
                case DagitimKaynagi.RastgeleParsiyel: profil = DagitimProfili.Parsiyel; break;
                case DagitimKaynagi.RastgeleOyun: profil = DagitimProfili.Oyun; break;
                case DagitimKaynagi.RastgeleSlam: profil = DagitimProfili.Slam; break;
                case DagitimKaynagi.Kozsuz: profil = DagitimProfili.Kozsuz; break;
                case DagitimKaynagi.NumaraliRastgele:
                    profil = DagitimProfili.Rastgele;
                    seed = _currentTour * 7919 + 17;   // aynı bord numarası = aynı el
                    break;
                case DagitimKaynagi.Uretilmis:
                    // Bord numarasına göre sırayla parsiyel, oyun, kozsuz, slam elleri
                    DagitimProfili[] dongu = { DagitimProfili.Parsiyel, DagitimProfili.Oyun, DagitimProfili.Kozsuz, DagitimProfili.Slam };
                    profil = dongu[(((_currentTour - 1) % 4) + 4) % 4];
                    break;
                default: profil = DagitimProfili.Rastgele; break;   // Rastgele ve Kullanıcı girişi
            }
            return DagitimUretici.Uret(profil, EnIyiElIzinli(plan.EnIyiEl), seed);
        }

        public void BordTekrariBaslat()
        {
            BordDurumunuSifirla();

            _oyunAsamasi = OyunAsamasi.Baslangic;
            activePlayer = _dealer;
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;
            _kontrat = null;
            _kontratDeklaran = _dealer;

            ButonlariInaktifYap();
            UpdateInfoBoard(_dealer);
            OyunBilgiPaneliniGuncelle();

            // Aynı seed ile ilk dağıtım (takaslar geri alınır, sadece Güney açık)
            KartlariDagitSeedIle(AnaSayfa.SeciliKartSeti, _seed);

            YeniDekBasFormAc();
        }

        public void YeniBordBaslat()
        {
            System.Diagnostics.Debug.WriteLine(
                "[YeniBord] Yeni bord başlatılıyor...");

            if (!MacSonrakiBordaHazirla()) return;   // maç bittiyse yeni bord yok

            PratikIlerlet();
            BordDurumunuSifirla();

            // 4) Aktif oyuncu, dealer, kontrat vs. sıfırlanır — DortPasSonrasiYeniEliBaslat ile aynı
            _oyunAsamasi = OyunAsamasi.Baslangic;
            _currentTour = MacDurumu.SonrakiBordNo(_currentTour);   // 16'dan sonra 1
            DagiticiyiAyarla();
            if (!ManuelGirisAl()) { Close(); return; }   // Kullanıcı girişi: eller elle girilir (İptal: oyun kapanır)
            activePlayer = _dealer;
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;

            _kontrat = null;
            _kontratDeklaran = _dealer;

            // 5) Butonları inaktif yap
            ButonlariInaktifYap();

            // 6) Info board ve oyun bilgi panelini güncelle
            UpdateInfoBoard(_dealer);
            OyunBilgiPaneliniGuncelle();

            // 7) Yeni kartları dağıt
            if (MacDurumu.Aktif && !MacDurumu.SeedRastgele) KartlariDagitSeedIle(AnaSayfa.SeciliKartSeti, MacDurumu.BordSeed());
            else KartlariDagitRastgele(AnaSayfa.SeciliKartSeti);

            // 8) Yeni DekBasForm aç
            PratikDuyurusu();
            YeniDekBasFormAc();
        }

        private Image GetBackImage()
        {
            return GetCachedImage(CurrentBackImage);
        }

        private void KartlariDagit(string kartSeti)
        {
            // 🔹 Seed yoksa üret
            if (_seed <= 0)
            {
                _seed = YeniSeed();
            }

            // 🔹 Seed'e bağlı deterministik Random
            System.Diagnostics.Debug.WriteLine(
                $"[KartlariDagit] Seed: {_seed}");

            this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
            try
            {
                // ─── 1. Kartları karıştır ve 4 oyuncuya böl ───
                // Dağıtım kaynağı + en iyi el ayarlarına göre (varsayılan: eski rastgele dağıtımla aynı)
                List<string> kartlar = DagitimiUret();

                var kuzey = kartlar.Take(13).ToList();
                var dogu = kartlar.Skip(13).Take(13).ToList();
                var guney = kartlar.Skip(26).Take(13).ToList();
                var bati = kartlar.Skip(39).Take(13).ToList();

                // ─── 2. Elleri temizle ve geçici listelere doldur ───
                kuzeyEl.Clear(); guneyEl.Clear(); batiEl.Clear(); doguEl.Clear();
                List<Card> geciciKuzey = new List<Card>();
                List<Card> geciciGuney = new List<Card>();
                List<Card> geciciBati = new List<Card>();
                List<Card> geciciDogu = new List<Card>();

                foreach (var file in kuzey) geciciKuzey.Add(ParseCard(file, kartSeti));
                foreach (var file in guney) geciciGuney.Add(ParseCard(file, kartSeti));
                foreach (var file in bati) geciciBati.Add(ParseCard(file, kartSeti));
                foreach (var file in dogu) geciciDogu.Add(ParseCard(file, kartSeti));

                kuzeyEl.AddRange(CardSorter.SiraliEl(geciciKuzey));
                guneyEl.AddRange(CardSorter.SiraliEl(geciciGuney));
                batiEl.AddRange(CardSorter.SiraliEl(geciciBati));
                doguEl.AddRange(CardSorter.SiraliEl(geciciDogu));

                // ─── 3. Tüm göster butonlarını sıfırla ───
                BtnHepsi.Checked = false;
                BtnEW.Checked = false;
                BtnNS.Checked = false;
                BtnGuney.Checked = false;
                hepsiniGösterToolStripMenuItem.Checked = false;
                eWGösterToolStripMenuItem.Checked = false;
                nSGösterToolStripMenuItem.Checked = false;

                // Aktif oyuncu = insan oyuncu ise buton pasif olmalı
                BtnGuney.Enabled = (activePlayer != _humanPlayer);

                // ─── 4. Layout'u hazırla (kart konumları, info board, badge'ler, play zone) ───
                //KartYerlesimiHazirla();

                // ─── 5. Görsel gösterim: sadece insan oyuncunun (Güney) eli açık ───
                SadeceGuneyiGoster();
                // 🔹 Seed'i hafızaya ekle
                SeedHafizayaEkle(_seed);
            }

            finally
            {
                this.ResumeLayout(true); EkranYardimci.CizimiAc(this);
            }
        }

        /// <summary>
        /// Belirli bir seed ile el dağıtır.
        /// Aynı seed + aynı kart seti → aynı dağıtım.
        /// Test için: aynı eli tekrar dağıtmak istediğinde kullan.
        /// </summary>
        // Deklaran Kuzey olunca eller N↔S ve E↔W takas edilir (180° döndürme).
        // PBN dışa aktarmada dağıtıcı/ihale koltukları buna göre döndürülür.
        private bool _ellerIhaleSonrasiTakasli = false;

        public void KartlariDagitSeedIle(string kartSeti, int seed)
        {
            _ellerIhaleSonrasiTakasli = false;
            _seed = seed;
            KartlariDagit(kartSeti);
            System.Diagnostics.Debug.WriteLine(
                $"[KartlariDagitSeedIle] Seed: {seed}");
        }

        /// <summary>
        /// Rastgele seed ile el dağıtır (mevcut davranış).
        /// Yeni bord başlarken kullanılır.
        /// </summary>
        public void KartlariDagitRastgele(string kartSeti)
        {
            if (!PratikModu) _seed = YeniSeed();
            KartlariDagit(kartSeti);
            System.Diagnostics.Debug.WriteLine(
                $"[KartlariDagitRastgele] Yeni seed: {_seed}");
        }


        // 🔹 "Elleri Değiştir" formundan (ElleriDegistir.BtnOK_Click) çağrılır: Güney'in
        // eli (kullanıcının eli), seçilen yöndeki oyuncunun eliyle takas edilir.
        // guneyEl/batiEl/kuzeyEl/doguEl alanları 'readonly' olduğu için referans
        // değişimi yapılamaz; bunun yerine listelerin İÇERİĞİ takas edilir.
        public void ElleriTakasEt(Player digerYon)
        {
            List<Card> digerEl;
            switch (digerYon)
            {
                case Player.Bati: digerEl = batiEl; break;
                case Player.Kuzey: digerEl = kuzeyEl; break;
                case Player.Dogu: digerEl = doguEl; break;
                default: return; // Güney seçilemez/anlamsız; güvenlik için çık.
            }

            List<Card> guneyGecici = new List<Card>(guneyEl);
            List<Card> digerGecici = new List<Card>(digerEl);

            guneyEl.Clear();
            guneyEl.AddRange(digerGecici);

            digerEl.Clear();
            digerEl.AddRange(guneyGecici);

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

        /// <summary>
        /// İki oyuncunun ellerini KENDİ ARALARINDA takas eder (Güney'e dokunmaz).
        /// ElleriTakasEt'ten farklı olarak Güney'i karıştırmaz.
        /// </summary>
        public void ElleriKendiAralarindaTakasEt(Player a, Player b)
        {
            var elA = OyuncuEli(a);
            var elB = OyuncuEli(b);
            if (elA == null || elB == null) return;

            var gecici = new List<Card>(elA);
            elA.Clear();
            elA.AddRange(elB);
            elB.Clear();
            elB.AddRange(gecici);
        }

        /// <summary>
        /// İhale Kuzey'de kaldığında çağrılır.
        /// N↔S ve E↔W elleri takas edilir. Sonuç:
        ///   - Güney (insan) eski Kuzey elini oynar → Deklaran
        ///   - Kuzey dummy olur (eski Güney eli)
        ///   - Doğu↔Batı döner → ihalede söylenen renk pozisyonu kaymaz
        ///   - Batı (eski Doğu eli) açılış kartını oynar
        /// </summary>
        public void DeklarasyonElDegisiminiUygula()
        {
            ElleriKendiAralarindaTakasEt(Player.Kuzey, Player.Guney);
            ElleriKendiAralarindaTakasEt(Player.Dogu, Player.Bati);
            _ellerIhaleSonrasiTakasli = true;

            System.Diagnostics.Debug.WriteLine(
                "[ElDegisimi] N↔S ve E↔W takas edildi.");
        }

        /// <summary>
        /// Briç kuralı: Sadece dummy açıktır. İnsan (Güney) kendi elini her zaman
        /// görür (açık). Deklaran Güney değilse kapalı kalır. Diğer defans oyuncusu
        /// kapalıdır.
        /// 
        /// Açık gösterilecekler:
        ///   - Güney (insan) → her zaman
        ///   - Dummy (deklaranın ortağı)
        /// Kapalı gösterilecekler:
        ///   - Deklaran (Güney değilse)
        ///   - Diğer defans oyuncusu
        /// </summary>

        private void HepsiniGoster()
        {
            KartYerlesimiHazirla();

            int doguXPos = doguKartlar[0] != null ? doguKartlar[0].Left : 930;
            int ewStartingY = GetCenteredEWStartingY();

            BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, 250, _kuzeyY, false, true);
            BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);
            BricStandartlarinaGoreDiz(doguKartlar, doguEl, doguXPos, ewStartingY);
            BricStandartlarinaGoreDiz(batiKartlar, batiEl, 40, ewStartingY);

            SetupPlayerBadges();
        }

        private void SadeceGuneyiGoster()
        {
            KartYerlesimiHazirla();

            Image backImg = GetBackImage();

            if (backImg != null)
            {
                for (int i = 0; i < 13; i++)
                {
                    if (kuzeyKartlar[i] != null) kuzeyKartlar[i].Image = backImg;
                    if (batiKartlar[i] != null) batiKartlar[i].Image = backImg;
                    if (doguKartlar[i] != null) doguKartlar[i].Image = backImg;
                }
            }
            else
            {
                // Arka yüz resmi yoksa, rakip kartların yüzünü gizle (masa rengi)
                for (int i = 0; i < 13; i++)
                {
                    if (kuzeyKartlar[i] != null) { kuzeyKartlar[i].Image = null; kuzeyKartlar[i].BackColor = AnaSayfa.MasaRengi; }
                    if (batiKartlar[i] != null) { batiKartlar[i].Image = null; batiKartlar[i].BackColor = AnaSayfa.MasaRengi; }
                    if (doguKartlar[i] != null) { doguKartlar[i].Image = null; doguKartlar[i].BackColor = AnaSayfa.MasaRengi; }
                }
            }

            BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);
            SetupPlayerBadges();
        }

        private int BricStandartlarinaGoreDizYatay(PictureBox[] kartResimleri, List<Card> eldekiKartlar, int baslangicX, int baslangicY, bool sikiDizim = false, bool groupSuits = true)
        {
            for (int i = 0; i < kartResimleri.Length; i++) if (kartResimleri[i] != null) kartResimleri[i].Visible = false;

            int guncelX = baslangicX;
            int kartIçiSıkışmaBoslugu = Ol(18);
            int toplamYerlestirilen = 0;
            int enSonKartBitisX = baslangicX + _kartGenislik;

            if (groupSuits)
            {
                // Renk sırası: ihale sonrası koz en solda, sonra renkler dönüşümlü (bkz. GetSuitSirasi)
                List<List<Card>> tumRenkGruplari = GetSuitSirasi()
                    .Select(renk => eldekiKartlar.Where(k => k.Suit == renk).OrderByDescending(k => k.Value).ToList())
                    .ToList();
                int renkBloklarArasiBosluk = sikiDizim ? 0 : Ol(10);

                foreach (var renkGrubu in tumRenkGruplari)
                {
                    if (renkGrubu.Count == 0) continue;
                    for (int i = 0; i < renkGrubu.Count; i++)
                    {
                        if (toplamYerlestirilen >= kartResimleri.Length) break;
                        PictureBox pb = kartResimleri[toplamYerlestirilen];
                        if (pb != null)
                        {
                            pb.Image = renkGrubu[i].Image;
                            pb.Tag = renkGrubu[i];   // 🔹 Card nesnesini Tag'de sakla
                            pb.Location = new Point(guncelX, baslangicY);
                            pb.Visible = true;
                            pb.BringToFront();
                        }
                        enSonKartBitisX = guncelX + _kartGenislik;
                        toplamYerlestirilen++;
                        guncelX += (i < renkGrubu.Count - 1) ? kartIçiSıkışmaBoslugu : (_kartGenislik + renkBloklarArasiBosluk);
                    }
                }
            }
            else
            {
                foreach (var card in eldekiKartlar)
                {
                    if (toplamYerlestirilen >= kartResimleri.Length) break;

                    PictureBox pb = kartResimleri[toplamYerlestirilen];
                    if (pb != null)
                    {
                        pb.Image = card.Image;
                        pb.Tag = card;   // 🔹 Card nesnesini Tag'de sakla
                        pb.Location = new Point(guncelX, baslangicY);
                        pb.Visible = true;
                        pb.BringToFront();
                    }

                    enSonKartBitisX = guncelX + _kartGenislik;
                    toplamYerlestirilen++;
                    guncelX += (_kartGenislik - Ol(55));
                }
            }
            return enSonKartBitisX;
        }

        private void BricStandartlarinaGoreDiz(PictureBox[] kartResimleri, List<Card> eldekiKartlar, int baslangicX, int baslangicY)
        {
            for (int i = 0; i < kartResimleri.Length; i++) if (kartResimleri[i] != null) kartResimleri[i].Visible = false;
            // Renk sırası: ihale sonrası koz en üstte, sonra renkler dönüşümlü (bkz. GetSuitSirasi)
            List<List<Card>> tumSatirlar = GetSuitSirasi()
                .Select(renk => eldekiKartlar.Where(k => k.Suit == renk).OrderByDescending(k => k.Value).ToList())
                .Where(satir => satir.Count > 0)
                .ToList();
            int yatayKayma = Ol(20);
            int dikeySatirAraligi = _kartYukseklik + 2;
            int toplamKullanilanKart = 0;
            for (int satirIndeks = 0; satirIndeks < tumSatirlar.Count; satirIndeks++)
            {
                List<Card> satirKartlari = tumSatirlar[satirIndeks];
                for (int kartIndeks = 0; kartIndeks < satirKartlari.Count; kartIndeks++)
                {
                    if (toplamKullanilanKart >= kartResimleri.Length) break;
                    PictureBox pb = kartResimleri[toplamKullanilanKart];
                    if (pb != null)
                    {
                        // Eğer E/W göstergesi aktifse ve Kuzey eli gösterimi devre dışı bırakılmak isteniyorsa
                        Image faceImg = satirKartlari[kartIndeks].Image;
                        Image backImg = GetBackImage();
                        if (kartResimleri == kuzeyKartlar && BtnEW != null && BtnEW.Checked)
                        {
                            // Eğer arka yüz resmi varsa onu göster; yoksa yüzü gizle (boş bırak)
                            if (backImg != null)
                            {
                                pb.Image = backImg;
                            }
                            else
                            {
                                pb.Image = null;
                                pb.BackColor = AnaSayfa.MasaRengi;
                            }
                        }
                        else
                        {
                            pb.Image = faceImg;
                        }
                        pb.Location = new Point(baslangicX + (kartIndeks * yatayKayma), baslangicY + (satirIndeks * dikeySatirAraligi));
                        pb.Visible = true;
                        pb.BringToFront();
                    }
                    toplamKullanilanKart++;
                }
            }
        }

        private Card ParseCard(string fileName, string kartSeti)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string suitChar = nameWithoutExt.Last().ToString();
            string rankStr = nameWithoutExt.Substring(0, nameWithoutExt.Length - 1);
            int value;
            switch (rankStr)
            {
                case "A": value = 14; break;
                case "K": value = 13; break;
                case "Q": value = 12; break;
                case "J": value = 11; break;
                default: value = int.Parse(rankStr); break;
            }
            string suit;
            switch (suitChar)
            {
                case "S": suit = "Maça"; break;
                case "H": suit = "Kupa"; break;
                case "C": suit = "Sinek"; break;
                case "D": suit = "Karo"; break;
                default: suit = ""; break;
            }
            string imgPath = Path.Combine(Application.StartupPath, "Resources", "Cards", kartSeti ?? "Default", fileName);
            return new Card { Suit = suit, Value = value, Image = GetCachedImage(imgPath) };
        }

        public void DortPasSonrasiYeniEliBaslat()
        {
            if (MacDurumu.Aktif) MacDurumu.PasYaz(_seed);   // dört pas / atlanan bord: 0
            if (!MacSonrakiBordaHazirla()) return;

            PratikIlerlet();
            BordDurumunuSifirla();
            _oyunAsamasi = OyunAsamasi.Baslangic;   // ← EKLE
            _currentTour = MacDurumu.SonrakiBordNo(_currentTour);   // 16'dan sonra 1
            DagiticiyiAyarla();
            if (!ManuelGirisAl()) { Close(); return; }   // Kullanıcı girişi: eller elle girilir (İptal: oyun kapanır)
            activePlayer = _dealer;
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;

            // 🔹 Yeni el başlıyor — kontrat bilgilerini sıfırla
            _kontrat = null;
            _kontratDeklaran = _dealer;

            ButonlariInaktifYap();
            UpdateInfoBoard(_dealer);
            if (MacDurumu.Aktif && !MacDurumu.SeedRastgele) _seed = MacDurumu.BordSeed();
            else if (!PratikModu) _seed++;  // 🔹 Yeni el → yeni seed (pratikte liste belirler)
            KartlariDagit(AnaSayfa.SeciliKartSeti);

            // 🔹 YENİ: Bilgi panelini güncelle (yeni Dealer ve Zon görünsün)
            OyunBilgiPaneliniGuncelle();

            PratikDuyurusu();
            YeniDekBasFormAc();
        }

        private void BtnSonrakiEl_Click(object sender, EventArgs e)
        {
            // Açık bir deklarasyon formu varsa kullanıcıyı uyar
            bool deklarasyonAcik = Application.OpenForms
                .OfType<DeklarasyonForm>()
                .Any(f => f.Visible);

            if (deklarasyonAcik)
            {
                var cevap = MessageBox.Show(
                    "Devam eden bir deklarasyon var. Yine de sonraki ele geçmek istiyor musunuz?\n" +
                    "Mevcut el kaybolacak.",
                    "Sonraki El Onayı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (cevap != DialogResult.Yes) return;
            }

            SonrakiElIsteği();
        }

        // Kontrat, deklaran ve atak (açılış) yapacak oyuncu bilgileri
        private string _kontrat;
        private Player _kontratDeklaran;
        private Player _atakYapacakOyuncu;


        // 🔹 Kazanılan el sayıları (oyun sırasında artacak)
        private int _kazanilanElNS = 0;
        private int _kazanilanElEW = 0;

        // 🔹 UI'ya yansıtmak için public erişim
        public string Kontrat => _kontrat;
        public Player KontratDeklaran => _kontratDeklaran;
        public int BordNo => _currentTour;
        public Player AtakYapacakOyuncu => _atakYapacakOyuncu;
        public int KazanilanElNS => _kazanilanElNS;
        public int KazanilanElEW => _kazanilanElEW;

        // ====================================================================
        // OYUN BİLGİ PANELİ (Sağ üst köşe)
        // --------------------------------------------------------------------
        // Dealer, Zon, Kontrat, Deklaran ve kazanılan el bilgilerini gösterir.
        // Konum ve boyut burada; değiştirmek için alanları güncelle.
        // ====================================================================
        private Panel oyunBilgiPanel;
        private Label lblBilgiTur;
        private Label lblBilgiDealer;
        private Label lblBilgiZon;
        private Label lblBilgiKontrat;
        private Label lblBilgiDeklaran;
        private Label lblBilgiKazanilanEl;

        // ═══════════════════════════════════════════════════════════════════
        // SEED PANELİ (oyun bilgi panelinin altında)
        // ═══════════════════════════════════════════════════════════════════

        private Panel seedPanel;

        //private Button btnSeedSil;
        //private Button btnSeedDagit;
        //private readonly Queue<int> _seedHafizasi = new Queue<int>(10);
        //private const int MaxSeedHafiza = 10;



        private const int BilgiPanelGenislik = 220;
        private const int BilgiPanelYukseklik = 190;
        private const int BilgiPanelSagBosluk = 20;
        private const int BilgiPanelUstBosluk = 70;

        /// <summary>
        /// Bir Player enum değerini Türkçe oyuncu ismine çevirir.
        /// </summary>
        public static string PlayerIsmiTurkce(Player p)
        {
            switch (KoltukHaritasi.Fiz(p))
            {
                case Player.Kuzey: return "Kuzey";
                case Player.Guney: return "Güney";
                case Player.Bati: return "Batı";
                case Player.Dogu: return "Doğu";
                default: return "—";
            }
        }
        /// <summary>
        /// Mevcut kontrattan koz rengini çıkarır.
        /// Örnek: "4♠" → "Maça", "3NT" → null (koz yok).
        /// </summary>
        private string KontratKozu()
        {
            if (string.IsNullOrEmpty(_kontrat)) return null;

            // NT — koz yok ("3NT", "3NT X" ...)
            if (_kontrat.IndexOf("NT", StringComparison.OrdinalIgnoreCase) >= 0) return null;

            // Kontralı kontratlarda ("4♠X", "4♠ XX") son karakter X olabilir;
            // bu yüzden son renk sembolü aranır.
            int idx = _kontrat.LastIndexOfAny(new[] { '♠', '♥', '♦', '♣' });
            if (idx < 0) return null;

            switch (_kontrat[idx])
            {
                case '♠': return "Maça";
                case '♥': return "Kupa";
                case '♦': return "Karo";
                case '♣': return "Sinek";
                default: return null;
            }
        }

        /// <summary>
        /// Elde renklerin dizilme sırası.
        ///   - İhale sırasında / NT kontratında: Maça, Kupa, Sinek, Karo (değişmez).
        ///   - Renk kontratında: koz 1. sırada (N/S'de en solda, E/W'de en üstte),
        ///     kalan renkler siyah-kırmızı dönüşümlü:
        ///       Maça  koz → Maça, Kupa, Sinek, Karo
        ///       Sinek koz → Sinek, Kupa, Maça, Karo
        ///       Kupa  koz → Kupa, Maça, Karo, Sinek
        ///       Karo  koz → Karo, Maça, Kupa, Sinek
        /// </summary>
        private string[] GetSuitSirasi()
        {
            string[] varsayilan = { "Maça", "Kupa", "Sinek", "Karo" };

            if (_oyunAsamasi != OyunAsamasi.Oyun) return varsayilan;

            switch (KontratKozu())
            {
                case "Maça": return new[] { "Maça", "Kupa", "Sinek", "Karo" };
                case "Sinek": return new[] { "Sinek", "Kupa", "Maça", "Karo" };
                case "Kupa": return new[] { "Kupa", "Maça", "Karo", "Sinek" };
                case "Karo": return new[] { "Karo", "Maça", "Kupa", "Sinek" };
                default: return varsayilan; // NT
            }
        }

        /// <summary>
        /// Zon durumunu Türkçe metin olarak döndürür.
        /// </summary>
        private string ZonDurumuMetni()
        {
            bool nsZonda, ewZonda;
            FizikselZon(out nsZonda, out ewZonda);

            if (nsZonda && ewZonda) return "Herkes";
            if (nsZonda) return "Kuzey/Güney";
            if (ewZonda) return "Batı/Doğu";
            return "Zonsuz";
        }

        private static Player OyuncuyaCevir(string oyuncu)
        {
            switch (oyuncu)
            {
                case "Kuzey": return Player.Kuzey;
                case "Dogu": return Player.Dogu;
                case "Bati": return Player.Bati;
                default: return Player.Guney;
            }
        }

        /// <summary>
        /// DeklarasyonForm'da ihale normal şekilde bittiğinde çağrılır.
        /// Kontratı ve deklaranı saklar; sırayı atak yapacak rakibe (deklaranın solu) verir.
        /// </summary>
        public void IhaleTamamlandiBaslat(string kontrat, string deklaran, string solRakip, int kontrCarpani = 0)
        {
            _kontrDurumu = kontrCarpani == 2 ? KontrDurumu.SKontr : kontrCarpani == 1 ? KontrDurumu.Kontr : KontrDurumu.Yok;
            _oyunAsamasi = OyunAsamasi.Oyun;
            OyunGecmisiniSifirla();

            if (BtnClaim != null) BtnClaim.Enabled = true;   // yer oyununda Claim kullanılabilir

            // 🔹 Oyun başında "göster" butonlarını sıfırla: ihale sırasında E/W, N/S ya da Hepsi
            //    açılmış olsa bile oyun başlarken yalnızca Güney açık olsun (dummy ilk atıştan sonra açılır).
            BtnHepsi.Checked = false;
            BtnEW.Checked = false;
            BtnNS.Checked = false;
            BtnGuney.Checked = false;
            hepsiniGösterToolStripMenuItem.Checked = false;
            eWGösterToolStripMenuItem.Checked = false;
            nSGösterToolStripMenuItem.Checked = false;
            // İlk kart atağına kadar Geri Al / İleri Al kapalı
            if (BtnGeriAl != null) BtnGeriAl.Enabled = false;
            if (BtnileriAl != null) BtnileriAl.Enabled = false;
            _kontrat = kontrat;
            _kontratDeklaran = OyuncuyaCevir(deklaran);
            _declarer = _kontratDeklaran;
            _atakYapacakOyuncu = OyuncuyaCevir(solRakip);
            // NOT: _dealer bord dağıtıcısıdır; açılış atağını yapanla değiştirilmez
            // (aksi halde "D" harfi, Dealer bilgisi, Replay ve sonraki bord dağıtıcısı kayar).
            activePlayer = _atakYapacakOyuncu;

            OyunBilgiPaneliniGuncelle();

            this.SuspendLayout(); EkranYardimci.CizimiDondur(this);
            try
            {
                // 🔹 1. ADIM: TÜM PictureBox'ları gizle ve görseli temizle
                for (int i = 0; i < 13; i++)
                {
                    if (kuzeyKartlar[i] != null) kuzeyKartlar[i].Visible = false;
                    if (guneyKartlar[i] != null) guneyKartlar[i].Visible = false;
                    if (batiKartlar[i] != null) batiKartlar[i].Visible = false;
                    if (doguKartlar[i] != null) doguKartlar[i].Visible = false;
                }

                // 🔹 2. ADIM: KartYerlesimiHazirla() ÇAĞIR (konumları ayarlar, kartları yerleştirir)
                KartYerlesimiHazirla();

                // 🔹 3. ADIM: Görseli arka yüz resmiyle değiştir
                Image backImg = GetBackImage();
                for (int i = 0; i < 13; i++)
                {
                    if (batiKartlar[i] != null)
                    {
                        batiKartlar[i].Image = backImg;
                        if (backImg == null) batiKartlar[i].BackColor = AnaSayfa.MasaRengi;
                    }
                    if (doguKartlar[i] != null)
                    {
                        doguKartlar[i].Image = backImg;
                        if (backImg == null) doguKartlar[i].BackColor = AnaSayfa.MasaRengi;
                    }
                }

                // 🔹 4. ADIM: Güney'in elini aç
                BricStandartlarinaGoreDizYatay(guneyKartlar, guneyEl, 250, _guneyY, false, true);

                // 🔹 5. ADIM: Dummy KAPALI başlar — açılış atağından sonra
                //             KartOyna içinde DummyyiAc() ile açılır (briç kuralı).
                _dummyAcildi = false;

                // 🔹 6. ADIM: Güney dışındaki tüm eller kapalı (Kuzey dahil)
                for (int i = 0; i < 13; i++)
                {
                    foreach (var kapali in new[] { kuzeyKartlar, batiKartlar, doguKartlar })
                    {
                        if (kapali[i] == null) continue;
                        kapali[i].Image = backImg;
                        if (backImg == null) kapali[i].BackColor = AnaSayfa.MasaRengi;
                    }
                }
            }
            finally
            {
                this.ResumeLayout(true); EkranYardimci.CizimiAc(this);
            }

            // 🔹 7. ADIM: AI'ı tetikle
            if (!InsanKontrolundeMi(activePlayer))
            {
                AITetikle();
            }

            UpdatePlayerBadges();
        }

        private Player SonrakiOyuncu(Player mevcut)
        {
            switch (mevcut)
            {
                case Player.Guney: return Player.Bati;
                case Player.Bati: return Player.Kuzey;
                case Player.Kuzey: return Player.Dogu;
                case Player.Dogu: return Player.Guney;
                default: return Player.Guney;
            }
        }


        // ═══════════════════════════════════════════════════════════════════
        // PBN (Portable Bridge Notation) DIŞA AKTARMA
        // Bridge Baron Deal Library, Deep Finesse vb. programlar okuyabilir.
        // ═══════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════
        // SKOR HESABI (duplicate) VE BORD ÖZETİ
        // ═══════════════════════════════════════════════════════════════════

        private KontrDurumu _kontrDurumu = KontrDurumu.Yok;

        private string KontrEki()
        {
            return _kontrDurumu == KontrDurumu.SKontr ? " XX" : _kontrDurumu == KontrDurumu.Kontr ? " X" : "";
        }

        /// <summary>16'lı bord döngüsüne göre zon (risk) durumu.</summary>
        private void ZonDurumu(out bool ns, out bool ew)
        {
            FizikselZon(out ns, out ew);
            // İnsan Doğu/Batı ise iç K-G çifti fiziksel D-B çiftidir
            if (KoltukHaritasi.CiftlerTers) { bool gecici = ns; ns = ew; ew = gecici; }
        }

    }
}
