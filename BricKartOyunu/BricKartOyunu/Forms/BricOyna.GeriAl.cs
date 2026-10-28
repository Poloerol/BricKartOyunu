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
    // Geri al / ileri al, otomatik oyun, claim
    public partial class BricOyna
    {
        private void ButonlariInaktifYap()
        {
            BtnOtoOyna.Enabled = false;
            BtnClaim.Enabled = false;
            BtnGeriAl.Enabled = false;
            BtnileriAl.Enabled = false;
            BtnIpucu.Enabled = false;
            BtnSonrakiEl.Enabled = false;
            BtnOyundanExit.Enabled = false;
            BtnHepsi.Enabled = false;
            BtnEW.Enabled = false;
            BtnNS.Enabled = false;
            BtnGuney.Enabled = false;
        }

        // DekBasForm'daki BtnDeklarasyon tıklandığında çağrılır.
        // DeklarasyonForm açılırken bu 7 buton aktif hale gelir.
        public void DeklarasyonAsamasindaButonlariAktifEt()
        {
            _oyunAsamasi = OyunAsamasi.Deklarasyon;   // ← EKLE
            DurumCubuguGuncelle();
            BtnOtoOyna.Enabled = true;
            BtnIpucu.Enabled = true;
            BtnSonrakiEl.Enabled = true;
            BtnOyundanExit.Enabled = true;
            BtnHepsi.Enabled = true;
            BtnEW.Enabled = true;
            BtnNS.Enabled = true;
        }
        // DeklarasyonForm içinde herhangi bir teklif/pas/kontr yapıldığında çağrılır.
        // Artık geri alınacak bir hamle olduğu için BtnGeriAl aktif edilir.
        public void GeriAlButonunuAktifEt()
        {
            if (BtnGeriAl != null)
                BtnGeriAl.Enabled = true;
        }
        private void BtnGeriAl_Click(object sender, EventArgs e)
        {
            var dekForm = Application.OpenForms
                .OfType<DeklarasyonForm>()
                .FirstOrDefault(f => f.Visible);

            if (dekForm != null)
                dekForm.SonHamleyiGeriAl();
            else
                OyunSonHamleyiGeriAl();

            GeriAlIleriAlButonlariniGuncelle();
        }

        private void BtnileriAl_Click(object sender, EventArgs e)
        {
            var dekForm = Application.OpenForms
                .OfType<DeklarasyonForm>()
                .FirstOrDefault(f => f.Visible);

            if (dekForm != null)
                dekForm.SonHamleyiIleriAl();
            else
                OyunSonHamleyiIleriAl();

            GeriAlIleriAlButonlariniGuncelle();
        }

        public void GeriAlIleriAlButonlariniGuncelle()
        {
            if (BtnGeriAl == null || BtnileriAl == null) return;

            var dekForm = Application.OpenForms
                .OfType<DeklarasyonForm>()
                .FirstOrDefault(f => f.Visible);

            if (dekForm != null)
            {
                BtnGeriAl.Enabled = dekForm.GeriAlinabilirMi;
                BtnileriAl.Enabled = dekForm.IleriAlinabilirMi;
            }
            else if (_oyunAsamasi == OyunAsamasi.Oyun)
            {
                // İlk kart atağından sonra Geri Al aktif; geri alınan kart varsa İleri Al aktif.
                BtnGeriAl.Enabled = _oyunGecmisi.Count > 0 && _oynananToplamEl < 13;
                BtnileriAl.Enabled = _oyunIleriAlYigini.Count > 0;
            }
            else
            {
                BtnGeriAl.Enabled = false;
                BtnileriAl.Enabled = false;
            }
        }

        /// <summary>
        /// Oyun aşamasında son oynanan kartı geri alır: kart oynayanın eline döner,
        /// masa / sıra / skor / dummy / oynanan eller önceki duruma getirilir.
        /// Sıra AI'daysa oyun duraklar; İleri Al aynı kartı tekrar oynatır.
        /// </summary>
        // ====================================================================
        // OTOMATİK OYNATMA (BtnOtoOyna)
        // ====================================================================
        // Evet de Hayır da bu bord boyunca dört oyuncuyu (Güney ve dummy dahil) AI yapar;
        // her kart AI zamanlayıcısıyla oynanır.
        //   Evet  → eller arasında duraklama yok (el bitince kendiliğinden devam eder)
        //   Hayır → her el bitince durur, oyun alanına tıklanınca bir sonraki ele geçilir (normal oyun gibi)
        private bool _tumAI = false;
        private bool _tumAIDuraklamasiz = false;
        private System.Windows.Forms.Timer _tumAIElTimer;

        private void BtnOtoOyna_Click(object sender, EventArgs e)
        {
            // Yalnızca yer oyunu (kart oynama) aşamasında çalışır
            if (_oyunAsamasi != OyunAsamasi.Oyun || _tumAI) return;
            if (_oynananToplamEl >= 13) return;

            DialogResult sonuc;
            using (var onay = new OtoOynaOnayForm())
            {
                sonuc = onay.ShowDialog(this);
            }

            if (sonuc == DialogResult.Yes) OtomatikOyna(true);
            else if (sonuc == DialogResult.No) OtomatikOyna(false);
            // pencere X ile kapatıldıysa hiçbir şey yapma
        }

        private void OtomatikOyna(bool duraklamasiz)
        {
            _oyunIleriAlYigini.Clear();
            _tumAI = true;
            _tumAIDuraklamasiz = duraklamasiz;
            GeriAlIleriAlButonlariniGuncelle();

            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            if (_elBittiBekliyor)
            {
                if (duraklamasiz) TumAIYeniEle();   // önceki el bitmiş: sonraki ele geç
                // duraklamalıysa oyun alanına tıklanmasını bekler (normal oyun)
            }
            else AITetikle();                        // sıradaki oyuncu (insan olsa da) AI gibi oynar
        }

        /// <summary>"El sonrası duraklat" kapalıyken, el bitince oyun alanına tıklanmadan devam eder.</summary>
        private void ElSonrasiOtomatikDevamZamanla()
        {
            _tumAIElTimer?.Stop();
            _tumAIElTimer?.Dispose();
            _tumAIElTimer = new System.Windows.Forms.Timer { Interval = AnaSayfa.OyunPlani.OtomatikOynaGecikmesiMs() < 600 ? 600 : AnaSayfa.OyunPlani.OtomatikOynaGecikmesiMs() };
            _tumAIElTimer.Tick += (s, e) =>
            {
                _tumAIElTimer?.Stop();
                _tumAIElTimer?.Dispose();
                _tumAIElTimer = null;
                if (!IsDisposed && !_tumAI && _elBittiBekliyor && _oyunAsamasi == OyunAsamasi.Oyun)
                    PlayZonePanel_Click(null, EventArgs.Empty);
            };
            _tumAIElTimer.Start();
        }

        /// <summary>Tam AI modunda el bitince kısa bekleyip (oyun alanına tıklanmadan) yeni ele geçer.</summary>
        private void TumAIYeniEleZamanla()
        {
            _tumAIElTimer?.Stop();
            _tumAIElTimer?.Dispose();
            _tumAIElTimer = new System.Windows.Forms.Timer { Interval = (AnaSayfa.OyunPlani ?? new OyunPlaniSecenekleri()).OtomatikOynaGecikmesiMs() };
            _tumAIElTimer.Tick += (s, e) =>
            {
                _tumAIElTimer?.Stop();
                _tumAIElTimer?.Dispose();
                _tumAIElTimer = null;
                if (!IsDisposed) TumAIYeniEle();
            };
            _tumAIElTimer.Start();
        }

        private void TumAIYeniEle()
        {
            if (!_tumAI || !_elBittiBekliyor || _oyunAsamasi != OyunAsamasi.Oyun) return;
            PlayZonePanel_Click(null, EventArgs.Empty);   // masayı temizler, kazananı başlatır, AI'ı tetikler
        }

        /// <summary>Otomatik oynatmayı bırakır (kullanıcı geri/ileri aldığında ya da yeni bordda).</summary>
        private void TumAIyiDurdur()
        {
            _tumAI = false;
            _tumAIDuraklamasiz = false;
            _tumAIElTimer?.Stop();
            _tumAIElTimer?.Dispose();
            _tumAIElTimer = null;
        }

        // ====================================================================
        // CLAIM (BtnClaim)
        // ====================================================================
        private static int KoltukNo(Player p)
        {
            switch (p)
            {
                case Player.Bati: return 0;
                case Player.Kuzey: return 1;
                case Player.Dogu: return 2;
                default: return 3;   // Güney
            }
        }

        private static int RenkNo(string renk)
        {
            switch (renk)
            {
                case "Maça": return 0;
                case "Kupa": return 1;
                case "Karo": return 2;
                case "Sinek": return 3;
                default: return -1;
            }
        }

        /// <summary>
        /// İhaleyi kazanan ortaklığın, bütün eller açıkmış gibi en iyi oyunla kalan ellerden kaçını
        /// alacağını hesaplar ve "n el daha alacağım. Kabul ediyor musun?" diye sorar.
        /// Hayır → oyun kaldığı yerden sürer. Evet → kalan eller dağıtılır, bütün kartlar
        /// başlangıçtaki gibi dizilir ve DealCompleteForm açılır.
        /// </summary>
        private void BtnClaim_Click(object sender, EventArgs e)
        {
            if (_oyunAsamasi != OyunAsamasi.Oyun || _tumAI || _oynananToplamEl >= 13) return;

            // Hesap el başında yapılır: masada yarım kalan el varsa önce o tamamlanmalı
            if (_buEldeOynananlar.Count > 0)
            {
                MessageBox.Show(
                    "Claim, bir el tamamlandıktan sonra (yeni ele başlamadan önce) yapılabilir.",
                    "Claim", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Bekleyen AI hamlesi varsa durdur (Hayır denirse aşağıda devam ettirilir)
            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            bool kabul = false;
            Cursor eskiImlec = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                var eller = new ulong[4];
                foreach (Player p in new[] { Player.Bati, Player.Kuzey, Player.Dogu, Player.Guney })
                {
                    foreach (var c in OyuncuEli(p))
                    {
                        int r = RenkNo(c.Suit);
                        if (r < 0) continue;
                        eller[KoltukNo(p)] |= 1UL << (r * 13 + (c.Value - 2));
                    }
                }

                int lider = KoltukNo(_elBittiBekliyor ? _sonElKazanani : activePlayer);
                string kozAdi = KontratKozu();
                int koz = kozAdi == null ? -1 : RenkNo(kozAdi);
                int taraf = KoltukNo(_kontratDeklaran) & 1;   // 1: Kuzey/Güney, 0: Batı/Doğu

                var cozucu = new ClaimCozucu(eller, lider, koz, taraf, 15000000);
                int n = cozucu.Coz();
                Cursor.Current = eskiImlec;

                if (cozucu.Asildi)
                {
                    MessageBox.Show(
                        "Bu aşamada kalan el sayısı güvenle hesaplanamadı.\nBirkaç el daha oynayıp tekrar deneyin.",
                        "Claim", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult sonuc;
                using (var onay = new ClaimOnayForm(n))
                {
                    sonuc = onay.ShowDialog(this);
                }
                if (sonuc != DialogResult.Yes) return;

                kabul = true;
                ClaimiUygula(n, taraf);
            }
            finally
            {
                Cursor.Current = eskiImlec;

                // Hayır / iptal / hesaplanamadı: oyun kaldığı yerden devam etsin
                if (!kabul && !IsDisposed && _oyunAsamasi == OyunAsamasi.Oyun && !_elBittiBekliyor &&
                    _buEldeOynananlar.Count == 0 && _oyunIleriAlYigini.Count == 0 &&
                    !InsanKontrolundeMi(activePlayer))
                {
                    AITetikle();
                }
            }
        }

        /// <summary>Kabul edilen claim'i uygular: elleri işler, bord sonu ekranını açar.</summary>
        private void ClaimiUygula(int n, int deklaranTarafi)
        {
            int kalan = 13 - _oynananToplamEl;
            int savunma = kalan - n;
            if (deklaranTarafi == 1) { _kazanilanElNS += n; _kazanilanElEW += savunma; }
            else { _kazanilanElEW += n; _kazanilanElNS += savunma; }

            _oynananToplamEl = 13;
            _elBittiBekliyor = false;
            _buEldeOynananlar.Clear();
            _buEldeAtakRengi = null;

            OyunBilgiPaneliniGuncelle();
            GeriAlIleriAlButonlariniGuncelle();

            // Dört eli başlangıçtaki gibi (13'er kart) açar, DealCompleteForm'u gösterir
            BordTamamlandiFormuAc();
        }

        private void OyunSonHamleyiGeriAl()
        {
            if (_oyunAsamasi != OyunAsamasi.Oyun) return;
            if (_oyunGecmisi.Count == 0 || _oynananToplamEl >= 13) return;
            TumAIyiDurdur();   // kullanıcı müdahale etti: normal moda dön

            // Bekleyen AI hamlesi varsa iptal et
            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            var h = _oyunGecmisi[_oyunGecmisi.Count - 1];
            _oyunGecmisi.RemoveAt(_oyunGecmisi.Count - 1);
            _oyunIleriAlYigini.Add(h);

            // Kartı oyuncunun eline geri koy
            var el = OyuncuEli(h.Oyuncu);
            if (el != null && !el.Contains(h.Kart)) el.Add(h.Kart);

            // Sayaçlar ve listeler
            _buEldeOynananlar.Clear();
            _buEldeOynananlar.AddRange(h.BuEldeOnce);
            _buEldeAtakRengi = h.AtakRengi;
            _elBittiBekliyor = h.ElBittiBekliyor;
            _sonElKazanani = h.SonElKazanani;
            _oynananToplamEl = h.ToplamEl;
            _kazanilanElNS = h.NS;
            _kazanilanElEW = h.EW;
            if (_oynananTumEller.Count > h.TumEllerSayisi)
                _oynananTumEller.RemoveRange(h.TumEllerSayisi, _oynananTumEller.Count - h.TumEllerSayisi);

            bool dummyKapanacak = _dummyAcildi && !h.DummyAcildi;
            _dummyAcildi = h.DummyAcildi;

            // Masa: o hamleden önceki kartlar
            MasadakiKartlariTemizle();
            foreach (var (o, k) in h.BuEldeOnce)
                KartiMasayaYerlestir(o, k, k.Image);

            // Eller (görsel)
            OyuncuEliniGuncelle(h.Oyuncu, h.Kart);
            if (dummyKapanacak)
                OyuncuEliniGuncelle(IhaleDurumu.PartnerBul(_kontratDeklaran), null);   // açılış atağı geri alındı → dummy tekrar kapalı

            activePlayer = h.Oyuncu;
            OyunBilgiPaneliniGuncelle();
            OynananElleriFormlariniGuncelle();
            GeriAlIleriAlButonlariniGuncelle();
        }

        /// <summary>
        /// Oyun aşamasında en son geri alınan kartı tekrar oynatır (aynı oyuncu, aynı kart).
        /// Geri alınan tüm kartlar ileri alındığında sıra AI'daysa oyun kaldığı yerden sürer.
        /// </summary>
        private void OyunSonHamleyiIleriAl()
        {
            if (_oyunAsamasi != OyunAsamasi.Oyun) return;
            if (_oyunIleriAlYigini.Count == 0) return;
            TumAIyiDurdur();

            _aiTimer?.Stop();
            _aiTimer?.Dispose();
            _aiTimer = null;

            var h = _oyunIleriAlYigini[_oyunIleriAlYigini.Count - 1];
            _oyunIleriAlYigini.RemoveAt(_oyunIleriAlYigini.Count - 1);

            int onceki = _oyunGecmisi.Count;
            _ileriAliniyor = true;
            try
            {
                KartOyna(h.Oyuncu, h.Kart, h.Kart.Image);
            }
            finally
            {
                _ileriAliniyor = false;
            }

            // Kart kabul edilmediyse (durum tutarsız) ileri alma yığını geçersizdir
            if (_oyunGecmisi.Count == onceki)
                _oyunIleriAlYigini.Clear();

            OynananElleriFormlariniGuncelle();

            // Geri alınanların hepsi ileri alındı ve sıra AI'da → oyun devam etsin
            if (_oyunIleriAlYigini.Count == 0 && !_elBittiBekliyor && _oynananToplamEl < 13 &&
                !InsanKontrolundeMi(activePlayer))
            {
                AITetikle();
            }

            GeriAlIleriAlButonlariniGuncelle();
        }

        /// <summary>Açık IhaleGecmisiForm varsa içeriğini güncel ihale geçmişiyle yeniler.</summary>
        private void IhaleGecmisiFormlariniGuncelle()
        {
            foreach (var f in Application.OpenForms.OfType<IhaleGecmisiForm>().ToList())
                f.Guncelle(_sonIhaleGecmisi, _ihaleBaslangicOyuncusu);
        }

        /// <summary>Açık OynananElleriGosterForm varsa içeriğini güncel eller listesiyle yeniler.</summary>
        private void OynananElleriFormlariniGuncelle()
        {
            var liste = _oynananTumEller;
            string baslik = null;
            if (liste.Count == 0 && _oncekiBordEller != null && _oncekiBordEller.Count > 0)
            {
                liste = _oncekiBordEller; baslik = _oncekiBordBaslik;
            }
            foreach (var f in Application.OpenForms.OfType<OynananElleriGosterForm>().ToList())
                f.Guncelle(liste, baslik);
        }

        /// <summary>
        /// Belirtilen oyuncunun elini döndürür.
        /// (Ihale motoru için.)
        /// </summary>
        public List<Card> GetOyuncuEli(Player oyuncu)
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

    }
}
