using BricKartOyunu.Class;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BricKartOyunu
{
    /// <summary>
    /// BricOyna'nın bord sonu bağlantıları. Akış sınıfı oyun formunun özel alanlarına
    /// doğrudan erişmez; ihtiyaç duyduğu her şey buradaki delegelerle verilir.
    /// </summary>
    internal sealed class BordSonuBaglantisi
    {
        /// <summary>AI zamanlayıcısını durdurur ve "el bitti bekleniyor" bayrağını kapatır.</summary>
        public Action AIDurdur;
        /// <summary>Dört eli açık gösterir ve masadaki kartları temizler.</summary>
        public Action ElleriAcMasayiTemizle;
        /// <summary>Oynanan elleri "önceki bord" olarak saklar.</summary>
        public Action BordiSakla;
        public Func<SkorSonucu> Skor;
        public Func<Player> Deklaran;
        /// <summary>Sonuç mesajının ortalanacağı kontrol (oyun alanı).</summary>
        public Func<Control> OrtalaKontrol;
        /// <summary>"Replay": aynı bord baştan.</summary>
        public Action Tekrar;
        /// <summary>"Go to Next Deal": yeni bord.</summary>
        public Action Yeni;
    }

    /// <summary>
    /// Bord bitince çalışan akış: sonuç mesajı → DealCompleteForm → (yeni bord | tekrar | ana menü).
    /// Mesaj ve DealCompleteForm modal DEĞİLDİR: ana pencerenin kontrolleri kapatılır
    /// (form kendisi etkin kalır), böylece "Oynanan Eller" gibi sahip olunan pencereler
    /// ve menü tıklanabilir kalır.
    /// </summary>
    internal sealed class BordSonuAkisi
    {
        private readonly BricOyna _oyna;
        private readonly BordSonuBaglantisi _b;
        private Dictionary<Control, bool> _girisDurumu;

        public BordSonuAkisi(BricOyna oyna, BordSonuBaglantisi baglanti)
        {
            _oyna = oyna;
            _b = baglanti;
        }

        /// <summary>
        /// Son kart bir tıklama/zamanlayıcı olayının içinde oynanır; modal pencereyi o olay
        /// bitmeden açmak fare yakalamasını pencerede bırakır. Bu yüzden akış mesaj döngüsüne
        /// bırakılıp sonra başlatılır.
        /// </summary>
        public void Baslat()
        {
            _b.AIDurdur();

            if (_oyna.IsHandleCreated)
                _oyna.BeginInvoke(new Action(GercekBaslat));
            else
                GercekBaslat();
        }

        private void GercekBaslat()
        {
            if (_oyna.IsDisposed) return;
            _oyna.Capture = false;
            _b.AIDurdur();

            System.Diagnostics.Debug.WriteLine("[BordTamamlandi] sonuç mesajı ve DealCompleteForm açılıyor...");

            // Bord bitti: dört elin tamamı açık, masada hiç kart kalmasın
            _b.ElleriAcMasayiTemizle();

            // Oynanan eller saklanır (yeni bord dağıtılınca da incelenebilsin)
            _b.BordiSakla();

            // Sonuç mesajı ve DealCompleteForm modal DEĞİL: ana pencerenin girişi kapatılır,
            // fakat "Oynanan Eller" penceresi her zaman tıklanabilir.
            AnaPencereGirisi(false);
            SonucMesajiGoster(DealCompleteFormunuAc);
        }

        /// <summary>Kontrat metni: "4♣", "4♦", "3NT" (semboller renkli pencerede kendi rengiyle görünür).</summary>
        private static string KontratYazisi(SkorSonucu s)
        {
            return s.Seviye + s.Koz + (s.KontrEki != "" ? " " + s.KontrEki : "");
        }

        /// <summary>
        /// DealCompleteForm'dan önce küçük bir sonuç mesajı gösterir:
        /// kontrat yapıldı mı / kaç batıldı ve skor (kullanıcı açısından).
        /// </summary>
        private void SonucMesajiGoster(Action sonra)
        {
            try
            {
                var s = _b.Skor();
                if (s.PasGecildi) { sonra?.Invoke(); return; }

                string kontrat = KontratYazisi(s);
                Player deklaran = _b.Deklaran();
                bool deklaranNS = deklaran == Player.Kuzey || deklaran == Player.Guney;
                // Kullanıcı (iç Güney) K-G tarafında: deklaran onunla aynı taraftaysa "biz" deklaranız
                bool bizDeklaran = deklaranNS;
                int fark = s.AlinanLove - s.GerekenLove;   // + fazla löve, - batış
                int bizimSkor = bizDeklaran ? s.Toplam : -s.Toplam;
                string skor = "Skor: " + (bizimSkor > 0 ? "+" : "") + bizimSkor;

                string govde;
                if (s.Yapildi)
                {
                    string fazla = fark > 0 ? " (+" + fark + " fazla löve)" : "";
                    govde = bizDeklaran
                        ? "Tebrikler!\nİhaleyi " + kontrat + " olarak gerçekleştirdiniz." + fazla
                        : "Rakipler " + kontrat + " kontratını gerçekleştirdi." + fazla;
                }
                else
                {
                    int bat = -fark;
                    govde = bizDeklaran
                        ? "Maalesef kontratı yapamadınız. Oyun " + bat + " battı."
                        : "Rakiplerin " + kontrat + " kontratı " + bat + " battı. Tebrikler!";
                }

                // Modal değil: mesaj açıkken "Oynanan Eller" gibi pencereler kullanılabilir.
                DeklarasyonForm.RenkliMesajGosterModelsiz(_oyna, "Bord Sonucu",
                    "İhale = " + kontrat + "\n\n" + govde + "\n\n" + skor, sonra, _b.OrtalaKontrol());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[BordSonucuBilgisi] " + ex.Message);
                sonra?.Invoke();
            }
        }

        private void DealCompleteFormunuAc()
        {
            if (_oyna.IsDisposed) { return; }
            var form = new DealCompleteForm(_oyna);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormClosed += (s, e) =>
            {
                var sonuc = form.DialogResult;
                AnaPencereGirisi(true);
                if (_oyna.IsDisposed) return;
                _oyna.Activate();

                if (sonuc == DialogResult.Retry)
                {
                    // "Replay" → aynı bord baştan
                    _b.Tekrar();
                }
                else if (sonuc == DialogResult.OK)
                {
                    // "Go to Next Deal" → yeni bord
                    System.Diagnostics.Debug.WriteLine("[BordTamamlandi] Kullanıcı yeni bord seçti.");
                    _b.Yeni();
                }
                else
                {
                    // "Return to Main Menu" veya X → AnaSayfa
                    System.Diagnostics.Debug.WriteLine("[BordTamamlandi] Kullanıcı ana menüye döndü.");
                    _oyna.Close();
                }
            };
            form.Show(_oyna);
        }

        /// <summary>
        /// Ana pencerenin kontrollerini kapatır/açar. Form kendisi (ve menü çubuğu) etkin kalır;
        /// böylece sahip olunan pencereler ("Oynanan Eller" vb.) ve menü tıklanabilir.
        /// </summary>
        private void AnaPencereGirisi(bool acik)
        {
            try
            {
                if (_oyna.IsDisposed) return;
                if (!acik)
                {
                    if (_girisDurumu != null) return;
                    _girisDurumu = new Dictionary<Control, bool>();
                    foreach (Control c in _oyna.Controls)
                    {
                        if (c is ToolStrip) continue;   // MenuStrip ("Eylemler") ve araç çubukları bord sonunda da kullanılabilir
                        _girisDurumu[c] = c.Enabled;
                        c.Enabled = false;
                    }
                }
                else if (_girisDurumu != null)
                {
                    foreach (var kv in _girisDurumu)
                        if (!kv.Key.IsDisposed) kv.Key.Enabled = kv.Value;
                    _girisDurumu = null;
                }
            }
            catch { }
        }
    }
}
