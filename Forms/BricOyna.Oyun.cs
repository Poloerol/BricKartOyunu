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
    // Kart oynama, löve, yapay zekâ
    public partial class BricOyna
    {
        /// <summary>
        /// PlayZonePanel'e tıklandığında çalışır.
        /// El bittiyse → kartları temizler ve yeni eli başlatır.
        /// </summary>
        private void PlayZonePanel_Click(object sender, EventArgs e)
        {
            // El bitmediyse hiçbir şey yapma
            if (!_elBittiBekliyor)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[PlayZone] Tıklandı ama el henüz bitmedi.");
                return;
            }

            System.Diagnostics.Debug.WriteLine(
                $"[PlayZone] El bitti onaylandı — yeni el başlıyor. Kazanan: {_sonElKazanani}");

            // Bayrağı sıfırla
            _elBittiBekliyor = false;

            // Masadaki kartları temizle
            MasadakiKartlariTemizle();

            // Yeni elin aktif oyuncusu = son elin kazananı
            activePlayer = _sonElKazanani;

            System.Diagnostics.Debug.WriteLine(
                $"[PlayZone] Aktif oyuncu: {activePlayer}");

            // Sıra AI'da mı? Otomatik oynat
            if (!InsanKontrolundeMi(activePlayer))
            {
                AITetikle();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    "[PlayZone] Sıra kullanıcıda, kart bekleniyor.");
            }
        }
        /// <summary>
        /// Kullanıcı Güney'in elindeki bir karta tıkladığında çalışır.
        /// Kartı elden kaldırır ve masaya (playZonePanel içine) yerleştirir.
        /// </summary>
        /// <summary>
        /// Bu oyuncunun kartlarını insan mı seçer?
        /// Güney her zaman; deklaran Güney ise dummy (Kuzey) de insan kontrolünde.
        /// </summary>
        private bool InsanKontrolundeMi(Player oyuncu)
        {
            if (_tumAI && _oyunAsamasi == OyunAsamasi.Oyun) return false;   // otomatik oynatma: herkes AI

            if (oyuncu == _humanPlayer) return true;

            if (_oyunAsamasi == OyunAsamasi.Oyun &&
                _kontratDeklaran == Player.Guney)
            {
                Player dummy = IhaleDurumu.PartnerBul(_kontratDeklaran);
                if (oyuncu == dummy) return true;
            }
            return false;
        }

        /// <summary>
        /// Renk takibi zorunluluğu: atak rengi varsa ve elde o renkten kart duruyorsa
        /// yalnızca o renkten kart oynanabilir. Atak yoksa her kart yasaldır.
        /// </summary>
        private bool YasalKartMi(Player oyuncu, Card kart)
        {
            if (string.IsNullOrEmpty(_buEldeAtakRengi)) return true;
            if (kart == null || kart.Suit == _buEldeAtakRengi) return true;

            var el = OyuncuEli(oyuncu);
            return el == null || !el.Any(c => c.Suit == _buEldeAtakRengi);
        }

        private void YasadisiKartUyarisi(Player oyuncu, Card kart)
        {
            System.Media.SystemSounds.Beep.Play();
            System.Diagnostics.Debug.WriteLine(
                $"[RenkTakibi] {oyuncu} {kart.Suit} {kart.Value} oynayamaz — {_buEldeAtakRengi} rengi takip edilmeli.");
        }

        private void GuneyKartTiklandi(int kartIndex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[GuneyKartTiklandi] index: {kartIndex}, aktif: {activePlayer}, aşama: {_oyunAsamasi}");

            if (_oyunAsamasi != OyunAsamasi.Oyun) return;
            if (_tumAI) return;   // otomatik oynatma sürerken elle oynanmaz

            // Kendi elinden oynama: sıra Güney'de olmalı
            if (activePlayer != Player.Guney)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[GuneyKartTiklandi] Sıra Güney'de değil — aktif: {activePlayer}");
                return;
            }

            if (kartIndex < 0 || kartIndex >= guneyKartlar.Length) return;

            var pb = guneyKartlar[kartIndex];
            if (pb == null || !pb.Visible) return;

            if (!(pb.Tag is Card card))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[GuneyKartTiklandi] Tag'de Card yok (#{kartIndex}).");
                return;
            }

            if (!YasalKartMi(Player.Guney, card))
            {
                YasadisiKartUyarisi(Player.Guney, card);
                return;
            }

            KartOyna(Player.Guney, card, pb.Image);
        }

        /// <summary>
        /// Dummy (Kuzey) kartına tıklama.
        /// Deklaran Güney iken sıra dummy'deyken insan Kuzey elinden oynar.
        /// </summary>
        private void KuzeyKartTiklandi(int kartIndex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[KuzeyKartTiklandi] index: {kartIndex}, aktif: {activePlayer}, " +
                $"deklaran: {_kontratDeklaran}, aşama: {_oyunAsamasi}");

            if (_oyunAsamasi != OyunAsamasi.Oyun) return;
            if (_tumAI) return;   // otomatik oynatma sürerken elle oynanmaz

            // Sadece deklaran Güney + sıra Kuzey (dummy)
            if (_kontratDeklaran != Player.Guney) return;
            if (activePlayer != Player.Kuzey)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[KuzeyKartTiklandi] Sıra dummy'de değil — aktif: {activePlayer}");
                return;
            }

            if (kartIndex < 0 || kartIndex >= kuzeyKartlar.Length) return;

            var pb = kuzeyKartlar[kartIndex];
            if (pb == null || !pb.Visible) return;

            if (!(pb.Tag is Card card))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[KuzeyKartTiklandi] Tag'de Card yok (#{kartIndex}).");
                return;
            }

            if (!YasalKartMi(Player.Kuzey, card))
            {
                YasadisiKartUyarisi(Player.Kuzey, card);
                return;
            }

            KartOyna(Player.Kuzey, card, pb.Image);
        }

        /// <summary>
        /// Bir oyuncunun kart oynamasını işler. Sıra takibini yönetir:
        /// - Kart masaya gider
        /// - Oyuncunun elinden çıkar
        /// - _buEldeOynananlar listesine eklenir
        /// - Sıra bir sonraki oyuncuya geçer
        /// - Sıra AI'da ise otomatik oynar (AI hamlesi tetiklenir)
        /// </summary>
        private void KartOyna(Player oyuncu, Card kart, Image kartResmi)
        {
            if (kart == null) return;
            if (_oyunAsamasi != OyunAsamasi.Oyun) return;

            // İpucu sarı tonlu kopyayı kartın resmi yapmıştı; masaya özgün resim gitmeli
            // (sarı kopya aşağıda dispose edilir, masada kullanılırsa "Parametre geçerli değil" olur).
            if (_ipucuKarti != null && ReferenceEquals(kartResmi, _ipucuKarti.Image) && kart.Image != null)
                kartResmi = kart.Image;
            IpucuVurgusunuKaldir();

            // El bitti, masadaki 4 kart henüz temizlenmedi (PlayZone tıklaması bekleniyor).
            // Bu durumda kart oynanırsa önceki elin kartlarıyla karışır, oyuncular
            // aynı elde iki kez oynar ve eller dengesiz biter. Kartı kabul etme.
            if (_elBittiBekliyor || _oynananToplamEl >= 13)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] {oyuncu} kartı reddedildi — el/bord bitti, yeni el başlamadan kart oynanamaz.");
                return;
            }

            // Sıra bu oyuncuda mı?
            if (activePlayer != oyuncu)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] Sıra {oyuncu}'da değil, aktif: {activePlayer}");
                return;
            }

            // ── Geri Al / İleri Al: bu hamleden önceki durumu kaydet ──
            _oyunGecmisi.Add(new OyunHamlesi
            {
                Oyuncu = oyuncu,
                Kart = kart,
                AtakRengi = _buEldeAtakRengi,
                ElBittiBekliyor = _elBittiBekliyor,
                SonElKazanani = _sonElKazanani,
                ToplamEl = _oynananToplamEl,
                NS = _kazanilanElNS,
                EW = _kazanilanElEW,
                DummyAcildi = _dummyAcildi,
                BuEldeOnce = new List<(Player Oyuncu, Card Kart)>(_buEldeOynananlar),
                TumEllerSayisi = _oynananTumEller.Count
            });
            if (!_ileriAliniyor) _oyunIleriAlYigini.Clear();   // yeni bir kart = ileri alınacak bölüm geçersiz

            // Bu elde ilk kart mı? Atak rengini belirle
            if (_buEldeAtakRengi == null)
            {
                _buEldeAtakRengi = kart.Suit;
            }

            // Kartı masaya yerleştir
            KartiMasayaYerlestir(oyuncu, kart, kartResmi);

            // Kartı oyuncunun elinden çıkar
            var el = OyuncuEli(oyuncu);
            el?.Remove(kart);

            // Kart oynandı — oyuncunun elini görsel olarak güncelle
            OyuncuEliniGuncelle(oyuncu, kart);

            // Bu elde oynananlar listesine ekle
            _buEldeOynananlar.Add((oyuncu, kart));

            // İlk kart (açılış atağı) oynandı → dummy açılır
            if (!_dummyAcildi) DummyyiAc();

            System.Diagnostics.Debug.WriteLine(
                $"[KartOyna] {oyuncu} oynadı: {kart.Suit} {kart.Value} " +
                $"(bu elde {_buEldeOynananlar.Count}/4)");

            // ═══════════════════════════════════════════════════════════════
            // 4 KART TAMAMLANDI MI? (Yani el bitti mi?)
            // ═══════════════════════════════════════════════════════════════
            if (_buEldeOynananlar.Count >= 4)
            {
                // El kazananını belirle
                Player kazanan = ElKazananiniBelirle();

                // 🔹 Bu eli kaydet
                int elNo = _oynananToplamEl + 1;
                foreach (var (o, k) in _buEldeOynananlar)
                {
                    bool kazandi = (o == kazanan);
                    _oynananTumEller.Add((elNo, o, k, kazandi));
                }

                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] El #{elNo} kaydedildi (kazanan: {kazanan})");

                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] El tamamlandı — kazanan: {kazanan}");

                // Skor artışı
                if (kazanan == Player.Kuzey || kazanan == Player.Guney)
                    _kazanilanElNS++;
                else
                    _kazanilanElEW++;

                // 🔹 Eli kazanan kartı masada sarıyla işaretle
                KazananKartiVurgula(kazanan);

                OyunBilgiPaneliniGuncelle();

                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] Skor → NS: {_kazanilanElNS}, EW: {_kazanilanElEW}");

                // El sayacını artır
                _oynananToplamEl++;

                System.Diagnostics.Debug.WriteLine(
                    $"[KartOyna] Bu bordda {_oynananToplamEl}/13 el oynandı.");

                // Bu elde oynananları sıfırla (yeni el için hazırlık)
                _buEldeAtakRengi = null;
                _buEldeOynananlar.Clear();

                // ═══════════════════════════════════════════════════════════
                // BORD BİTTİ Mİ? (13 el tamamlandı mı?)
                // ═══════════════════════════════════════════════════════════
                if (_oynananToplamEl >= 13)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[KartOyna] BORD TAMAMLANDI! Deal Complete formu açılacak.");

                    _elBittiBekliyor = false;
                    GeriAlIleriAlButonlariniGuncelle();
                    BordTamamlandiFormuAc();
                    return;
                }

                // El bitti ama bord devam ediyor
                _sonElKazanani = kazanan;
                _elBittiBekliyor = true;
                GeriAlIleriAlButonlariniGuncelle();
                if (_tumAI && _tumAIDuraklamasiz) TumAIYeniEleZamanla();
                else if (!_tumAI && AnaSayfa.OyunPlani != null && !AnaSayfa.OyunPlani.ElSonrasiDuraklat)
                    ElSonrasiOtomatikDevamZamanla();   // "El sonrası duraklat: Hayır"


                System.Diagnostics.Debug.WriteLine(
                    "[KartOyna] El bitti. PlayZone'a tıklayınca yeni el başlayacak.");

                return;
            }

            // ═══════════════════════════════════════════════════════════════
            // EL HENÜZ BİTMEDİ — Sırayı bir sonraki oyuncuya geçir
            // ═══════════════════════════════════════════════════════════════
            activePlayer = SonrakiOyuncu(activePlayer);
            GeriAlIleriAlButonlariniGuncelle();

            // Sıra AI'da mı? Otomatik oynat (İleri Al sırasında AI tetiklenmez)
            if (!_ileriAliniyor && !InsanKontrolundeMi(activePlayer))
            {
                AITetikle();
            }
        }

        /// <summary>
        /// Belirtilen oyuncunun kartını görsel olarak da elden kaldırır.
        /// Yani kart PictureBox'ını gizler ve eldeki kartları yeniden dizer.
        /// </summary>
        private void OyuncuEliniGuncelle(Player oyuncu, Card oynananKart)
        {
            // ─── 1. Oyuncunun kart dizisini seç ───
            PictureBox[] kartResimleri;
            List<Card> el;
            bool yatayDizim = false;
            int baslangicX = 0;
            int baslangicY = 0;

            switch (oyuncu)
            {
                case Player.Kuzey:
                    kartResimleri = kuzeyKartlar;
                    el = kuzeyEl;
                    yatayDizim = true;
                    baslangicX = 250;
                    baslangicY = _kuzeyY;
                    break;
                case Player.Guney:
                    kartResimleri = guneyKartlar;
                    el = guneyEl;
                    yatayDizim = true;
                    baslangicX = 250;
                    baslangicY = _guneyY;
                    break;
                case Player.Bati:
                    kartResimleri = batiKartlar;
                    el = batiEl;
                    yatayDizim = false;
                    baslangicX = 40;
                    baslangicY = GetCenteredEWStartingY();
                    break;
                case Player.Dogu:
                    kartResimleri = doguKartlar;
                    el = doguEl;
                    yatayDizim = false;
                    baslangicX = doguKartlar[0]?.Left ?? 930;
                    baslangicY = GetCenteredEWStartingY();
                    break;
                default:
                    return;
            }

            // ─── 2. Tüm PictureBox'ları gizle ───
            for (int i = 0; i < kartResimleri.Length; i++)
            {
                if (kartResimleri[i] != null)
                    kartResimleri[i].Visible = false;
            }

            // ─── 2b. Kapalı Batı/Doğu: başlangıçtaki düz dikey yığın düzeni ───
            // (Renk renk dizim kalan kartlardan dağılım bilgisi sızdırır.)
            if ((oyuncu == Player.Bati || oyuncu == Player.Dogu) && !OyuncuEliAcikMi(oyuncu))
            {
                KapaliDikeyYiginiDuzenle(kartResimleri, el.Count, baslangicX);
                return;
            }

            // ─── 3. Kalan kartları yeniden diz ───
            // NOT: Zaten diğer kartlar dizili ama oynanan kart gizlenmediği için
            //      yeniden dizmek en temiz yol.
            if (yatayDizim)
            {
                BricStandartlarinaGoreDizYatay(kartResimleri, el, baslangicX, baslangicY, false, OyuncuEliAcikMi(oyuncu));
            }
            else
            {
                BricStandartlarinaGoreDiz(kartResimleri, el, baslangicX, baslangicY);
            }

            // ─── 4. Açık olmaması gereken oyuncunun kartlarını tekrar kapat ───
            // Dizim metotları kartları açık yüzle çizer; defans oyuncusu (ve deklaran
            // Güney değilse) yeniden dizimden sonra kapalı kalmalı.
            if (!OyuncuEliAcikMi(oyuncu))
            {
                Image backImg = GetBackImage();
                for (int i = 0; i < kartResimleri.Length; i++)
                {
                    if (kartResimleri[i] == null || !kartResimleri[i].Visible) continue;
                    if (backImg != null) kartResimleri[i].Image = backImg;
                    else { kartResimleri[i].Image = null; kartResimleri[i].BackColor = AnaSayfa.MasaRengi; }
                }
            }
        }

        /// <summary>
        /// Kapalı Batı/Doğu elini KartYerlesimiHazirla ile aynı düz dikey yığın olarak çizer.
        /// Kalan kart sayısı kadar kutu görünür; yığın aralığı ve konumu değişmez,
        /// böylece renk dağılımı hakkında hiçbir ipucu verilmez.
        /// </summary>
        private void KapaliDikeyYiginiDuzenle(PictureBox[] kartResimleri, int kalanKartSayisi, int x)
        {
            int maxEWToplamYukseklik = (4 * _kartYukseklik) + (3 * 2);
            int ewBaslangicY = _kuzeyY + Ol(172);
            float dikeyAdim = (maxEWToplamYukseklik - _kartYukseklik) / 12f;
            if (dikeyAdim < 0) dikeyAdim = 0;

            Image backImg = GetBackImage();

            for (int i = 0; i < kartResimleri.Length; i++)
            {
                var pb = kartResimleri[i];
                if (pb == null) continue;

                if (i < kalanKartSayisi)
                {
                    if (backImg != null) pb.Image = backImg;
                    else { pb.Image = null; pb.BackColor = AnaSayfa.MasaRengi; }
                    pb.Left = x;
                    pb.Top = ewBaslangicY + (int)(i * dikeyAdim);
                    pb.Visible = true;
                    pb.BringToFront();
                }
                else
                {
                    pb.Visible = false;
                }
            }
        }

        /// <summary>
        /// Oyun aşamasında bu oyuncunun eli açık mı gösterilmeli?
        /// Güney (insan) her zaman; dummy (deklaranın ortağı) açık; diğerleri kapalı.
        /// "Hepsi / E-W / N-S göster" butonları aktifse o oyuncular da açık kalır.
        /// </summary>
        // Dummy, açılış atağı oynanana kadar kapalıdır.
        private bool _dummyAcildi = false;

        /// <summary>Açılış atağından sonra dummy'yi açar (yatay: Kuzey, dikey: Batı/Doğu).</summary>
        private void DummyyiAc()
        {
            if (_dummyAcildi) return;
            _dummyAcildi = true;

            Player dummy = IhaleDurumu.PartnerBul(_kontratDeklaran);
            if (dummy == Player.Kuzey)
                BricStandartlarinaGoreDizYatay(kuzeyKartlar, kuzeyEl, 250, _kuzeyY, false, true);
            else if (dummy == Player.Bati)
                BricStandartlarinaGoreDiz(batiKartlar, batiEl, 40, GetCenteredEWStartingY());
            else if (dummy == Player.Dogu)
                BricStandartlarinaGoreDiz(doguKartlar, doguEl, doguKartlar[0]?.Left ?? 930, GetCenteredEWStartingY());

            System.Diagnostics.Debug.WriteLine($"[Dummy] {dummy} açıldı.");
        }

        private bool OyuncuEliAcikMi(Player oyuncu)
        {
            if (oyuncu == Player.Guney) return true;
            if (_oyunAsamasi != OyunAsamasi.Oyun) return true; // ihale aşamasında mevcut davranış korunur

            if (BtnHepsi != null && BtnHepsi.Checked) return true;
            if (BtnEW != null && BtnEW.Checked && (oyuncu == Player.Dogu || oyuncu == Player.Bati)) return true;
            if (BtnNS != null && BtnNS.Checked && oyuncu == Player.Kuzey) return true;

            return _dummyAcildi && oyuncu == IhaleDurumu.PartnerBul(_kontratDeklaran); // dummy (atak sonrası)
        }

        /// <summary>
        /// PlayZone'daki tüm masa kartlarını kaldırır ve dispose eder.
        /// </summary>
        private void MasadakiKartlariTemizle()
        {
            if (playZonePanel == null) return;

            foreach (var pb in _masadakiKartlar.Values)
            {
                if (pb != null && !pb.IsDisposed)
                {
                    playZonePanel.Controls.Remove(pb);
                    pb.Dispose();
                }
            }
            _masadakiKartlar.Clear();

            System.Diagnostics.Debug.WriteLine(
                "[YeniEl] Masadaki kartlar temizlendi.");
        }
        /// <summary>
        /// Aktif oyuncu AI ise, kısa bir gecikme ile AI'ın oynamasını tetikler.
        /// Timer sınıf alanı olarak tutulur — yoksa GC toplayıp oyun takılır.
        /// </summary>
        /// <summary>
        /// "Tek kartı otomatik oyna": insan kontrolündeki oyuncunun yalnızca bir yasal kartı varsa
        /// (renk takibi mecburi veya son kart) kısa gecikmeyle o kartı kendisi oynar.
        /// </summary>
        private void TekKartiOtomatikOynatDene()
        {
            var plan = AnaSayfa.OyunPlani;
            if (plan == null || !plan.TekKartiOtomatikOyna) return;
            if (_oyunAsamasi != OyunAsamasi.Oyun || _tumAI || _elBittiBekliyor || _oynananToplamEl >= 13) return;

            Player oyuncu = activePlayer;
            var el = OyuncuEli(oyuncu);
            if (el == null) return;
            var yasal = el.Where(c => YasalKartMi(oyuncu, c)).ToList();
            if (yasal.Count != 1) return;

            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = new System.Windows.Forms.Timer { Interval = plan.AiGecikmesiMs() };
            _aiTimer.Tick += (s, e) =>
            {
                _aiTimer?.Stop();
                _aiTimer?.Dispose();
                _aiTimer = null;
                if (IsDisposed || _oyunAsamasi != OyunAsamasi.Oyun || _tumAI || _elBittiBekliyor) return;
                if (activePlayer != oyuncu || !InsanKontrolundeMi(oyuncu)) return;

                var kartlar = oyuncu == Player.Guney ? guneyKartlar : oyuncu == Player.Kuzey ? kuzeyKartlar : null;
                if (kartlar == null) return;
                var kart = yasal[0];
                foreach (var pb in kartlar)
                {
                    var c = pb == null ? null : pb.Tag as Card;
                    if (pb != null && pb.Visible && c != null && c.Suit == kart.Suit && c.Value == kart.Value)
                    {
                        KartOyna(oyuncu, c, pb.Image);
                        return;
                    }
                }
            };
            _aiTimer.Start();
        }

        private void AITetikle()
        {
            System.Diagnostics.Debug.WriteLine(
                $"[AITetikle] Çağrıldı — aktif: {activePlayer}, aşama: {_oyunAsamasi}, " +
                $"insan: {_humanPlayer}");

            // İnsan veya dummy (deklaran Güney iken Kuzey) → AI yok
            if (InsanKontrolundeMi(activePlayer))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AITetikle] {activePlayer} insan kontrolünde, AI yok.");
                TekKartiOtomatikOynatDene();
                return;
            }

            // Önceki timer varsa temizle
            _aiTimer?.Stop();
            _aiTimer?.Dispose();

            System.Diagnostics.Debug.WriteLine(
                "[AITetikle] Yeni timer oluşturuluyor (800ms).");

            _aiTimer = new System.Windows.Forms.Timer { Interval = (AnaSayfa.OyunPlani ?? new OyunPlaniSecenekleri()).AiGecikmesiMs() };
            _aiTimer.Tick += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AITetikle] Timer tick — aktif: {activePlayer}");

                _aiTimer?.Stop();
                _aiTimer?.Dispose();
                _aiTimer = null;

                var oynayacak = activePlayer;
                System.Diagnostics.Debug.WriteLine(
                    $"[AITetikle] AIOyna çağrılacak: {oynayacak}");

                if (!InsanKontrolundeMi(oynayacak))
                {
                    AIOyna(oynayacak);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[AITetikle] {oynayacak} insan kontrolünde, AIOyna yok.");
                }
            };

            System.Diagnostics.Debug.WriteLine(
                "[AITetikle] Timer.Start() çağrılıyor.");
            _aiTimer.Start();
            System.Diagnostics.Debug.WriteLine(
                "[AITetikle] Timer başlatıldı.");
        }

        /// <summary>
        /// Belirtilen oyuncunun elini döndürür.
        /// </summary>
        private List<Card> OyuncuEli(Player oyuncu)
        {
            switch (oyuncu)
            {
                case Player.Kuzey: return kuzeyEl;
                case Player.Guney: return guneyEl;
                case Player.Bati: return batiEl;
                case Player.Dogu: return doguEl;
                default: return null;
            }
        }

        /// <summary>
        /// Aktif oyuncu AI ise otomatik oynar.
        /// Basit ama briç mantığına uygun kart seçimi:
        /// 1. Koz varsa: kozu oyna (en düşük koz — küçük koz)
        /// 2. Renk takip zorunluysa: 
        ///    - Ortak As/K oynadıysa → en düşük
        ///    - Rakip yüksek oynadıysa → en yüksek (üçüncü el yüksek)
        ///    - İkinci el → en düşük (second hand low)
        ///    - Dördüncü el → kazanacak en düşük
        ///    - Diğer → en düşük
        /// 3. Renk yoksa: kozu oyna veya en düşük kart
        /// </summary>
        /// <summary>
        /// Aktif oyuncu AI ise otomatik oynar.
        /// 
        /// Briç mantığına uygun kart seçimi:
        /// 1. Açılış atağı: AtakKurallari (As, Rua, Dam atağı vs.)
        /// 2. Renk takip zorunluysa:
        ///    - Ortak As/K oynadıysa → en düşük
        ///    - Rakip Q+ oynadıysa → en yüksek
        ///    - İkinci el → en düşük (second hand low)
        ///    - Üçüncü el → en yüksek (third hand high)
        ///    - Dördüncü el → kazanacak en düşük, kazanamıyorsa en düşük
        /// 3. Renk yoksa:
        ///    - Rakip kazanıyorsa → en yüksek koz (trick'i kazan)
        ///    - Ortak kazanıyorsa → en düşük koz (üstüne koz verme)
        ///    - Koz yoksa → en düşük kart (ıskarta)
        /// Açılış atağında sıralı onör / marka kuralları (As, Rua, Dam atağı) uygulanır,
        /// diğer durumlarda renk takip + rastgele.
        /// </summary>
        private void AIOyna(Player oyuncu)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[AIOyna] Çağrıldı — oyuncu: {oyuncu}, aktif: {activePlayer}, " +
                $"aşama: {_oyunAsamasi}");

            if (_oyunAsamasi != OyunAsamasi.Oyun)
            {
                System.Diagnostics.Debug.WriteLine("[AIOyna] Aşama Oyun değil, çıkılıyor.");
                return;
            }
            if (InsanKontrolundeMi(oyuncu))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AIOyna] {oyuncu} insan kontrolünde, çıkılıyor.");
                return;
            }
            if (activePlayer != oyuncu)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AIOyna] Sıra {oyuncu}'da değil, aktif: {activePlayer}");
                return;
            }

            var el = OyuncuEli(oyuncu);
            if (el == null || el.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AIOyna] {oyuncu}'ın eli boş, çıkılıyor.");
                return;
            }

            // ═══════════════════════════════════════════════════════════════
            // DEBUG: Elin tüm kartlarını ve renk dağılımını yazdır
            // ═══════════════════════════════════════════════════════════════
            System.Diagnostics.Debug.WriteLine($"[DEBUG] {oyuncu}'ın eli ({el.Count} kart):");
            foreach (var kart in el)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"  - Suit='{kart.Suit}', Value={kart.Value}");
            }
            System.Diagnostics.Debug.WriteLine(
                $"[DEBUG] RenkUzunlugu: " +
                $"♠={ElDegerlendirici.RenkUzunlugu(el, "Maça")}, " +
                $"♥={ElDegerlendirici.RenkUzunlugu(el, "Kupa")}, " +
                $"♦={ElDegerlendirici.RenkUzunlugu(el, "Karo")}, " +
                $"♣={ElDegerlendirici.RenkUzunlugu(el, "Sinek")}");
            System.Diagnostics.Debug.WriteLine(
                $"[DEBUG] HP={ElDegerlendirici.HCP(el)}, " +
                $"Dengeli={ElDegerlendirici.DengeliEl(el)}");

            // Kart seçimi OyunZekasi'na devredildi (OyunZekasi.cs):
            //  - açılış atağı, savunma/deklaran ataklari, renk takibi, kesme, atma
            //  - yalnızca yasal kartlar (renk takibi zorunluluğu) döner
            var baglam = OyunBaglamiOlustur(oyuncu);
            string neden;
            Card secilen = OyunZekasi.KartSec(baglam, out neden);

            // Beceri düzeyi (ortaklık profilinden): düşükse arada rastgele yasal kart
            OyunZekasi.BeceriDuzeyi = AnlasmaYonetici.Mevcut(AnlasmaYonetici.TarafOf(oyuncu)).BeceriDuzeyi;
            var hata = OyunZekasi.BeceriHatasi(OyunKurallari.LegalKartlar(el, _buEldeAtakRengi));
            if (hata != null) { secilen = hata; neden = "Beceri düzeyi: hatalı oyun"; }

            if (secilen == null)
            {
                secilen = OyunKurallari.LegalKartlar(el, _buEldeAtakRengi)
                                       .OrderBy(c => c.Value).First();
                neden = "Yedek: en düşük yasal kart";
            }

            System.Diagnostics.Debug.WriteLine($"[AIOyna] Karar: {neden}");

            System.Diagnostics.Debug.WriteLine(
                $"[AIOyna] {oyuncu} AI oynadı: {secilen.Suit} {secilen.Value}");

            KartOyna(oyuncu, secilen, secilen.Image);
        }
        /// <summary>
        /// AI'ın karar verirken kullanabileceği meşru bilgiyi toplar:
        /// kendi eli, deklaran tarafıysa diğer eli (deklaran↔dummy), savunmaysa
        /// açılmış dummy eli (açılış atağından sonra), masa, geçmiş eller, ihale.
        /// </summary>
        /// <summary>"4♠" → 4, "3NT X" → 3. Okunamazsa 0 (hedef mantığı devre dışı).</summary>
        private int KontratSeviyesi()
        {
            if (string.IsNullOrEmpty(_kontrat)) return 0;
            char ch = _kontrat.Trim()[0];
            return (ch >= '1' && ch <= '7') ? ch - '0' : 0;
        }

        private OyunBaglami OyunBaglamiOlustur(Player oyuncu)
        {
            Player dummy = IhaleDurumu.PartnerBul(_kontratDeklaran);
            bool atakYapildi = _oynananToplamEl > 0 || _buEldeOynananlar.Count > 0;
            bool deklaranTarafi = (oyuncu == _kontratDeklaran || oyuncu == dummy);

            var b = new OyunBaglami
            {
                Ben = oyuncu,
                Deklaran = _kontratDeklaran,
                Koz = KontratKozu(),
                Elim = new List<Card>(OyuncuEli(oyuncu)),
                Masa = new List<(Player Oyuncu, Card Kart)>(_buEldeOynananlar),
                Gecmis = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>(_oynananTumEller),
                ElNo = _oynananToplamEl,
                IhaleGecmisi = _sonIhaleGecmisi,
                IhaleBaslangici = _ihaleBaslangicOyuncusu,
                AnlasmaOf = (k) => AnlasmaYonetici.Mevcut((k == Player.Kuzey || k == Player.Guney) ? Taraf.NS : Taraf.EW),
                Seviye = KontratSeviyesi()
            };

            if (deklaranTarafi)
                b.OrtakEli = new List<Card>(OyuncuEli(IhaleDurumu.PartnerBul(oyuncu)));
            else if (atakYapildi)
                b.DummyEli = new List<Card>(OyuncuEli(dummy));

            return b;
        }

        /// <summary>
        /// Şu anki trick'te (henüz 4 kart tamamlanmadan) kim kazanıyor?
        /// Koz varsa ve oynanmışsa → en yüksek koz
        /// Yoksa → atak rengindeki en yüksek kart
        /// </summary>
        private Player SuAnkiTrickKazanani()
        {
            if (_buEldeOynananlar == null || _buEldeOynananlar.Count == 0)
                return activePlayer;

            return OyunKurallari.ElKazanani(_buEldeOynananlar, KontratKozu()).Oyuncu;
        }
        /// <summary>
        /// Bu elde oynanan 4 kart arasında kazananı belirler.
        /// Kurallar:
        /// - Atak rengi (ilk oynanan kartın rengi) belirleyicidir
        /// - Aynı renkten en yüksek kart kazanır
        /// - Koz varsa ve oynanmışsa, en yüksek koz kazanır
        /// </summary>
        private Player ElKazananiniBelirle()
        {
            if (_buEldeOynananlar.Count != 4) return activePlayer;

            return OyunKurallari.ElKazanani(_buEldeOynananlar, KontratKozu()).Oyuncu;
        }
        /// <summary>
        /// Belirtilen oyuncunun kartını masaya (playZonePanel içine) yerleştirir.
        /// O kart zaten masada varsa önce eskisini kaldırır.
        /// </summary>
        /// <summary>
        /// El tamamlandığında masadaki kazanan kartı sarı tonla vurgular
        /// (kart yüzü sarıya boyanır, diğer üç kart olduğu gibi kalır).
        /// Vurgu, masa temizlenirken kartla birlikte kaybolur.
        /// </summary>
        private void KazananKartiVurgula(Player kazanan)
        {
            if (!_masadakiKartlar.TryGetValue(kazanan, out var pb) || pb == null || pb.IsDisposed) return;
            if (pb.Image == null) return;

            try
            {
                // Orijinal (paylaşılan/önbellekli) resme dokunma: sarı tonlu kopya üret.
                var src = pb.Image;
                var tinted = new Bitmap(src.Width, src.Height);
                using (var g = Graphics.FromImage(tinted))
                using (var ia = new System.Drawing.Imaging.ImageAttributes())
                {
                    var cm = new System.Drawing.Imaging.ColorMatrix(new float[][]
                    {
                        new float[] {1f, 0f, 0f,    0f, 0f},
                        new float[] {0f, 1f, 0f,    0f, 0f},
                        new float[] {0f, 0f, 0.45f, 0f, 0f},   // maviyi kıs → sarı ton
                        new float[] {0f, 0f, 0f,    1f, 0f},
                        new float[] {0f, 0f, 0f,    0f, 1f}
                    });
                    ia.SetColorMatrix(cm);
                    g.DrawImage(src, new Rectangle(0, 0, tinted.Width, tinted.Height),
                                0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
                }
                pb.Image = tinted;   // masa PictureBox'ı temizlenirken Dispose edilir
                pb.BringToFront();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[KazananKartiVurgula] {ex.Message}");
            }
        }

        private void KartiMasayaYerlestir(Player oyuncu, Card card, Image image)
        {
            if (playZonePanel == null) return;

            // Aynı oyuncunun masada kartı varsa önce kaldır
            if (_masadakiKartlar.TryGetValue(oyuncu, out var eskiPb) && eskiPb != null)
            {
                playZonePanel.Controls.Remove(eskiPb);
                eskiPb.Dispose();
                _masadakiKartlar.Remove(oyuncu);
            }

            // Yeni PictureBox oluştur
            var masaPb = new PictureBox
            {
                Width = MasaKartGenislik,
                Height = MasaKartYukseklik,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle,
                Image = image,
                Tag = card
            };

            // Oyuncuya göre pozisyon
            Point pozisyon = OyuncuIcinMasaPozisyonu(oyuncu);
            masaPb.Location = pozisyon;

            playZonePanel.Controls.Add(masaPb);
            masaPb.BringToFront();
            _masadakiKartlar[oyuncu] = masaPb;

            System.Diagnostics.Debug.WriteLine(
                $"[BricOyna] {oyuncu} kartı masaya kondu: {card.Suit} {card.Value} @ {pozisyon}");
        }

        /// <summary>
        /// Belirtilen oyuncu için masadaki kart pozisyonunu döndürür.
        /// </summary>
        private Point OlcekliNokta(Point p) { return new Point(Ol(p.X), Ol(p.Y)); }

        private Point OyuncuIcinMasaPozisyonu(Player oyuncu)
        {
            switch (oyuncu)
            {
                case Player.Guney: return OlcekliNokta(MasaPozisyonGuney);
                case Player.Kuzey: return OlcekliNokta(MasaPozisyonKuzey);
                case Player.Bati: return OlcekliNokta(MasaPozisyonBati);
                case Player.Dogu: return OlcekliNokta(MasaPozisyonDogu);
                default: return new Point(0, 0);
            }
        }

        /// <summary>
        /// DeklarasyonForm'dan gelen aktif oyuncu bilgisine göre BricOyna üzerindeki 
        /// ilgili oyuncu etiketini SARI renge boyar, diğerlerini normale çeker.
        /// </summary>
        public void AktifOyuncuEtiketiniGuncelle(string aktifOyuncu)
        {
            activePlayer = OyuncuyaCevir(aktifOyuncu);
            // Property setter'ı otomatik olarak BtnGuney ve badge'leri günceller
        }

        private PictureBox[] KartKutulari(Player oyuncu)
        {
            switch (oyuncu)
            {
                case Player.Kuzey: return kuzeyKartlar;
                case Player.Guney: return guneyKartlar;
                case Player.Dogu: return doguKartlar;
                default: return batiKartlar;
            }
        }

        /// <summary>Önceki ipucu vurgusunu kaldırır (kartın özgün resmini geri koyar).</summary>
        private void IpucuVurgusunuKaldir()
        {
            var pb = _ipucuKarti;
            _ipucuKarti = null;
            if (pb == null || pb.IsDisposed) return;
            var kart = pb.Tag as Card;
            var eski = pb.Image;
            if (kart != null && kart.Image != null && !ReferenceEquals(eski, kart.Image))
            {
                pb.Image = kart.Image;
                if (eski != null) eski.Dispose();   // sarı tonlu kopya
            }
        }

        private void BtnIpucu_Click(object sender, EventArgs e)
        {
            if (_oyunAsamasi != OyunAsamasi.Oyun)
            {
                // İhale sırasında ipucu, deklarasyon penceresindeki İpucu düğmesinden alınır
                return;
            }
            if (_elBittiBekliyor || _oynananToplamEl >= 13) return;

            IpucuVurgusunuKaldir();

            Player oyuncu = activePlayer;
            if (!InsanKontrolundeMi(oyuncu)) return;   // AI sırasındaysa ipucu verilmez

            var el = OyuncuEli(oyuncu);
            if (el == null || el.Count == 0) return;

            string neden;
            Card oneri = null;
            try { oneri = OyunZekasi.KartSec(OyunBaglamiOlustur(oyuncu), out neden); }
            catch { oneri = null; }
            if (oneri == null)
                oneri = OyunKurallari.LegalKartlar(el, _buEldeAtakRengi).OrderBy(c => c.Value).FirstOrDefault();
            if (oneri == null) return;

            // Önerilen kartın görünen PictureBox'ı: Tag'inde Card nesnesi, aynı renk/değer
            PictureBox hedef = null;
            foreach (var pb in KartKutulari(oyuncu))
            {
                var c = pb == null ? null : pb.Tag as Card;
                if (pb != null && pb.Visible && c != null && c.Suit == oneri.Suit && c.Value == oneri.Value)
                { hedef = pb; break; }
            }
            if (hedef == null || hedef.Image == null) return;

            try
            {
                var src = hedef.Image;
                var tinted = new Bitmap(src.Width, src.Height);
                using (var g = Graphics.FromImage(tinted))
                using (var ia = new System.Drawing.Imaging.ImageAttributes())
                {
                    var cm = new System.Drawing.Imaging.ColorMatrix(new float[][]
                    {
                        new float[] {1f, 0f, 0f,    0f, 0f},
                        new float[] {0f, 1f, 0f,    0f, 0f},
                        new float[] {0f, 0f, 0.45f, 0f, 0f},   // maviyi kıs → sarı ton
                        new float[] {0f, 0f, 0f,    1f, 0f},
                        new float[] {0f, 0f, 0f,    0f, 1f}
                    });
                    ia.SetColorMatrix(cm);
                    g.DrawImage(src, new Rectangle(0, 0, tinted.Width, tinted.Height),
                                0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
                }
                hedef.Image = tinted;
                _ipucuKarti = hedef;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[İpucu] " + ex.Message);
            }
        }

    }
}
