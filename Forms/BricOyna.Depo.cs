using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Forms;

namespace BricKartOyunu
{
    // Dağılım Deposu: oyun masası boş açılır, depodan seçilen bord masaya dağıtılır;
    // "Evet" → o bordun en yüksek oynanabilir kontratı ihale edilir, "Hayır" → normal ihale süreci.
    public partial class BricOyna
    {
        /// <summary>true ise masa "Dağılım Deposu" olarak açılır (kartsız masa + Dağılım Deposu penceresi).</summary>
        public bool DepoModu { get; set; }

        // Depodan seçilen bordun 52 kartı (iç koltuk sırası K, D, G, B) ve dağıtıcısı; yeni bord başlayınca silinir
        private List<string> _depoDeste;
        private Player _depoDagitici;
        private List<Control> _depoGizli;   // Dağılım Deposu boş masasında gizlenen kontroller (null: gizleme yok)

        /// <summary>Açılışta çağrılır: masayı boş gösterir, Dağılım Deposu penceresini üstte açar.</summary>
        private void DepoAcilisi()
        {
            this.Text = "Dağılım Deposu";
            this.ShowInTaskbar = false;
            DepoKapagiKoy();
            // Pencere yeniden boyutlanınca/etkinleşince kart yerleşimi kartları öne alır; kapak yeniden en üste çıkar
            // Kart yerleşimi yeniden boyutlanmada/etkinleşmede kartları görünür yapabilir; boş masa sürdükçe tekrar gizle
            this.Resize += (s, e) => { if (_depoGizli != null) DepoKapagiKoy(); };
            this.Activated += (s, e) => { if (_depoGizli != null) DepoKapagiKoy(); };

            var depo = new DagilimDepo();
            depo.StartPosition = FormStartPosition.Manual;
            Rectangle alan = Screen.FromControl(this).WorkingArea;   // ekranın tam ortası
            depo.Location = new Point(alan.Left + (alan.Width - depo.Width) / 2, alan.Top + (alan.Height - depo.Height) / 2);
            depo.Show(this);
            depo.BringToFront();
        }

        /// <summary>İhaleyi Düzenle: DekBasForm ekranın tam ortasında açılır; Elleri Değiştir ve Sonraki Dağılım pasiftir.</summary>
        public void DepoIhaleDuzenleAc()
        {
            var dek = new DekBasForm(this) { DepoDuzenleModu = true, StartPosition = FormStartPosition.Manual };
            Rectangle alan = Screen.FromControl(this).WorkingArea;
            dek.Location = new Point(alan.Left + (alan.Width - dek.Width) / 2, alan.Top + (alan.Height - dek.Height) / 2);
            dek.Show(this);
            dek.BringToFront();
            dek.Activate();
        }

        /// <summary>Menü/araç/durum çubukları hariç masadaki her şeyi (kartlar, bilgi panelleri) gizler: boş yeşil masa kalır.</summary>
        private void DepoKapagiKoy()
        {
            if (_depoGizli == null) _depoGizli = new List<Control>();
            foreach (Control c in this.Controls)
            {
                if (c is MenuStrip || c is ToolStrip || c is StatusStrip || !c.Visible) continue;
                _depoGizli.Add(c);
                c.Visible = false;
            }
        }

        /// <summary>Gizlenen kontrolleri geri getirir ve gizlemeyi bitirir (bord yüklenirken).</summary>
        private void DepoKapaginiKaldir()
        {
            if (_depoGizli == null) return;
            var liste = _depoGizli;
            _depoGizli = null;
            foreach (Control c in liste) if (!c.IsDisposed) c.Visible = true;
        }

        /// <summary>
        /// Depodan seçilen bordu masaya dağıtır, "Eller girildiği gibi ihale edilip oynansın mı?" diye sorar.
        /// Evet: DeklarasyonForm açılır ve bordun en yüksek oynanabilir kontratı ihale edilir.
        /// Hayır: normal akış (DekBasForm → Deklarasyon) devam eder.
        /// </summary>
        public void DepoBordunuYukle(PbnBord bord, int bordNo)
        {
            if (bord == null || this.IsDisposed) return;

            var plan = AnaSayfa.OyunPlani ?? new OyunPlaniSecenekleri();
            Player fizDagitici = bord.DagiticiVar ? bord.Dagitici : plan.FizikselDagitici(bordNo);

            _depoDeste = PbnOkuyucu.Deste(bord);
            _depoDagitici = KoltukHaritasi.Ic(fizDagitici);

            DepoKapaginiKaldir();
            BordDurumunuSifirla();
            _oyunAsamasi = OyunAsamasi.Baslangic;
            _currentTour = bordNo;
            _dealer = _depoDagitici;
            activePlayer = _dealer;
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;
            _kontrat = null;
            _kontratDeklaran = _dealer;

            ButonlariInaktifYap();
            UpdateInfoBoard(_dealer);
            OyunBilgiPaneliniGuncelle();
            KartlariDagitSeedIle(AnaSayfa.SeciliKartSeti, _seed);
            this.Refresh();

            var cevap = MessageBox.Show(this, "Eller girildiği gibi ihale edilip oynansın mı?", "Dağılım Deposu",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (this.IsDisposed) return;

            if (cevap != DialogResult.Yes)
            {
                YeniDekBasFormAc();   // normal deklarasyon süreci
                return;
            }

            List<string> ihale;
            try { ihale = DepoMaksimumIhale(bord, fizDagitici); }
            catch (Exception ex)
            {
                MessageBox.Show(this, "En yüksek kontrat hesaplanamadı, normal ihale başlatılıyor:\n" + ex.Message,
                    "Dağılım Deposu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                YeniDekBasFormAc();
                return;
            }

            var dek = new DeklarasyonForm(this) { SenaryoIhalesi = new Queue<string>(ihale) };
            if (playZonePanel != null)
            {
                Point ekran = playZonePanel.PointToScreen(Point.Empty);
                Size boyut = playZonePanel.Size;
                dek.Location = new Point(ekran.X + (boyut.Width - dek.Width) / 2, ekran.Y + (boyut.Height - dek.Height) / 2);
            }
            DeklarasyonAsamasindaButonlariAktifEt();
            dek.Show(this);
        }

        /// <summary>
        /// Bordun çift dummy'de oynanabilir en yüksek skorlu kontratını bulur ve bunu veren ihale dizisini üretir:
        /// dağıtıcıdan başlayarak kontratı söyleyecek oyuncuya kadar pas, kontrat, ardından üç pas.
        /// Oynanabilir kontrat yoksa dört pas.
        /// </summary>
        private List<string> DepoMaksimumIhale(PbnBord bord, Player fizDagitici)
        {
            var fiz = new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            string[] renkAdi = { "Maça", "Kupa", "Karo", "Sinek" };
            var eller = new Dictionary<Player, List<Card>>();
            for (int k = 0; k < 4; k++)
            {
                var liste = new List<Card>();
                string[] parca = bord.El[k].Split('.');   // S.H.D.C
                for (int r = 0; r < 4; r++)
                    foreach (char c in parca[r])
                        liste.Add(new Card { Suit = renkAdi[r], Value = KartDegeri(c) });
                eller[fiz[k]] = liste;
            }

            var tablo = DDMotor.Hesapla(eller);
            bool ns, ew; ZonDurumu(out ns, out ew);
            if (KoltukHaritasi.CiftlerTers) { bool g = ns; ns = ew; ew = g; }   // fiziksel zon

            int dagiticiTarafi = FizikselIndeks(fizDagitici) & 1;   // 0 = K-G, 1 = D-B
            int[] kozSirasi = { 3, 2, 1, 0, 4 };                   // ♣ ♦ ♥ ♠ NT (DDS koz numarası)
            int enIyiSkor = int.MinValue, enIyiSeviye = 0, enIyiKoz = -1, enIyiDeklaran = -1;
            foreach (int taraf in new[] { dagiticiTarafi, 1 - dagiticiTarafi })
            {
                foreach (int koz in kozSirasi)
                {
                    int d = tablo.TarafDeklaran(koz, taraf);
                    int lov = tablo.Al(koz, d);
                    int seviye = lov - 6;
                    if (seviye < 1) continue;
                    int skor = ParHesabi.Skor(seviye, koz, false, taraf == 0 ? ns : ew, lov);
                    if (skor > enIyiSkor) { enIyiSkor = skor; enIyiSeviye = seviye; enIyiKoz = koz; enIyiDeklaran = d; }
                }
            }

            var ihale = new List<string>();
            if (enIyiKoz < 0)
            {
                for (int i = 0; i < 4; i++) ihale.Add("Pas");
                return ihale;
            }

            Player teklifci = KoltukHaritasi.Ic(fiz[enIyiDeklaran]);
            var sira = new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            int idx = Array.IndexOf(sira, _dealer);
            for (int guard = 0; sira[idx % 4] != teklifci && guard < 4; guard++, idx++) ihale.Add("Pas");
            ihale.Add(enIyiSeviye + ParHesabi.KozSembol[enIyiKoz]);
            ihale.Add("Pas"); ihale.Add("Pas"); ihale.Add("Pas");
            return ihale;
        }

        private static int KartDegeri(char c)
        {
            switch (char.ToUpperInvariant(c))
            {
                case 'A': return 14;
                case 'K': return 13;
                case 'Q': return 12;
                case 'J': return 11;
                case 'T': return 10;
                default: return c - '0';
            }
        }
    }
}
