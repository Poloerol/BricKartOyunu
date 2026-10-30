using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// El Girişi: "Kullanıcı girişi" dağıtım kaynağında kartlar dağılmadan önce açılır.
    /// Her koltuğun eli, renk sütunlarındaki onay kutularıyla elle girilir. Başka bir koltuğa girilmiş
    /// kartlar gri ve seçilemez görünür. OK'te 13'ten az kartlı el varsa kalanı rastgele dağıtmayı önerir.
    /// Koltuklar fiziksel pusulaya göredir (Güney, Batı, Kuzey, Doğu).
    /// </summary>
    public class ElGirisiForm : Form
    {
        private static readonly string[] Degerler = { "A", "K", "Q", "J", "10", "9", "8", "7", "6", "5", "4", "3", "2" };
        private static readonly string[] RenkAdlari = { "Sinek", "Karo", "Kupa", "Maça" };
        private static readonly string[] RenkHarfleri = { "C", "D", "H", "S" };   // dosya adı: ör. "10H.png"
        private static readonly Player[] Koltuklar = { Player.Guney, Player.Bati, Player.Kuzey, Player.Dogu };
        private static readonly string[] KoltukAdlari = { "Güney", "Batı", "Kuzey", "Doğu" };

        private readonly CheckBox[,] _kutu = new CheckBox[4, 13];
        private readonly int[,] _sahip = new int[4, 13];   // -1: kimsede değil, 0..3: Koltuklar dizisindeki indeks
        private readonly ComboBox _cmbKoltuk = new ComboBox();
        private readonly Label _lblSayi = new Label();
        private bool _guncelleniyor;

        /// <summary>OK ile kapandığında her koltuğun 13 kartı (dosya adları, ör. "AS.png").</summary>
        public Dictionary<Player, List<string>> Eller { get; private set; }

        /// <summary>Her kutu değişiminde o ana kadar girilen (eksik olabilen) eller bildirilir; masada canlı gösterim içindir.</summary>
        public event Action<Dictionary<Player, List<string>>> EllerDegisti;

        public ElGirisiForm()
        {
            Text = "El Girişi";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(312, 262);

            for (int s = 0; s < 4; s++)
            {
                var grup = new GroupBox { Text = RenkAdlari[s], Left = 8 + s * 52, Top = 4, Width = 50, Height = 250 };
                for (int d = 0; d < 13; d++)
                {
                    _sahip[s, d] = -1;
                    var kutu = new CheckBox { Text = Degerler[d], Left = 5, Top = 14 + d * 17, Width = 42, Height = 17 };
                    int renk = s, deger = d;
                    kutu.CheckedChanged += (o, e) => KutuDegisti(renk, deger);
                    _kutu[s, d] = kutu;
                    grup.Controls.Add(kutu);
                }
                Controls.Add(grup);
            }

            var btnOk = YeniDugme("OK", 8);
            var btnIptal = YeniDugme("İptal", 36);
            var btnTemizle = YeniDugme("Eller Temizle", 64);
            var btnKalan = YeniDugme("Kalanı Dağıt", 92);
            btnOk.Click += (o, e) => Tamam();
            btnIptal.DialogResult = DialogResult.Cancel;
            btnTemizle.Click += (o, e) => ElleriTemizle();
            btnKalan.Click += (o, e) => KalaniDagit();
            AcceptButton = btnOk; CancelButton = btnIptal;

            _lblSayi.Left = 222; _lblSayi.Top = 148; _lblSayi.Width = 84; _lblSayi.Height = 16;
            Controls.Add(_lblSayi);
            Controls.Add(new Label { Text = "Koltuk", Left = 222, Top = 186, Width = 84, Height = 16 });
            _cmbKoltuk.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbKoltuk.Left = 222; _cmbKoltuk.Top = 204; _cmbKoltuk.Width = 82;
            foreach (var ad in KoltukAdlari) _cmbKoltuk.Items.Add(ad);
            _cmbKoltuk.SelectedIndex = 0;
            _cmbKoltuk.SelectedIndexChanged += (o, e) => Yenile();
            Controls.Add(_cmbKoltuk);

            Yenile();
        }

        private Button YeniDugme(string metin, int ust)
        {
            var b = new Button { Text = metin, Left = 222, Top = ust, Width = 82, Height = 24 };
            Controls.Add(b);
            return b;
        }

        private int Sayi(int koltuk)
        {
            int n = 0;
            for (int s = 0; s < 4; s++)
                for (int d = 0; d < 13; d++)
                    if (_sahip[s, d] == koltuk) n++;
            return n;
        }

        private void KutuDegisti(int renk, int deger)
        {
            if (_guncelleniyor) return;
            int koltuk = _cmbKoltuk.SelectedIndex;
            if (_kutu[renk, deger].Checked)
            {
                if (Sayi(koltuk) >= 13)   // bir elde en fazla 13 kart
                {
                    System.Media.SystemSounds.Beep.Play();
                    _sahip[renk, deger] = -1;
                }
                else _sahip[renk, deger] = koltuk;
            }
            else _sahip[renk, deger] = -1;
            Yenile();
        }

        /// <summary>Kutuları seçili koltuğa göre günceller: başkasının kartı işaretli ve pasif görünür.</summary>
        private void Yenile()
        {
            int koltuk = _cmbKoltuk.SelectedIndex;
            _guncelleniyor = true;
            try
            {
                for (int s = 0; s < 4; s++)
                    for (int d = 0; d < 13; d++)
                    {
                        int sahip = _sahip[s, d];
                        _kutu[s, d].Checked = sahip != -1;
                        _kutu[s, d].Enabled = sahip == -1 || sahip == koltuk;
                    }
            }
            finally { _guncelleniyor = false; }
            _lblSayi.Text = "Kartlar: " + Sayi(koltuk);
            var bildir = EllerDegisti;
            if (bildir != null) bildir(MevcutEller());
        }

        /// <summary>O ana kadar girilen kartlar, koltuk başına dosya adları (ör. "10H.png"); eksik eller de dahil.</summary>
        private Dictionary<Player, List<string>> MevcutEller()
        {
            var eller = new Dictionary<Player, List<string>>();
            for (int k = 0; k < 4; k++)
            {
                var el = new List<string>();
                for (int s = 0; s < 4; s++)
                    for (int d = 0; d < 13; d++)
                        if (_sahip[s, d] == k) el.Add(Degerler[d] + RenkHarfleri[s] + ".png");
                eller[Koltuklar[k]] = el;
            }
            return eller;
        }

        private void ElleriTemizle()
        {
            for (int s = 0; s < 4; s++)
                for (int d = 0; d < 13; d++) _sahip[s, d] = -1;
            Yenile();
        }

        /// <summary>Kimsede olmayan kartları karıştırıp 13'ten az kartlı ellere tamamlar.</summary>
        private void KalaniDagit()
        {
            var kalan = new List<int[]>();
            for (int s = 0; s < 4; s++)
                for (int d = 0; d < 13; d++)
                    if (_sahip[s, d] == -1) kalan.Add(new[] { s, d });

            var rnd = new Random();
            for (int i = kalan.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var t = kalan[i]; kalan[i] = kalan[j]; kalan[j] = t;
            }

            int sira = 0;
            for (int k = 0; k < 4; k++)
            {
                int eksik = 13 - Sayi(k);
                for (int n = 0; n < eksik && sira < kalan.Count; n++, sira++)
                    _sahip[kalan[sira][0], kalan[sira][1]] = k;
            }
            Yenile();
        }

        private void Tamam()
        {
            bool eksikVar = false;
            for (int k = 0; k < 4; k++) if (Sayi(k) != 13) eksikVar = true;

            if (eksikVar)
            {
                var cevap = MessageBox.Show(this,
                    "Her elde 13 kart olmalıdır. Kalan kartlar rastgele dağıtılsın mı?",
                    "El Girişi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (cevap != DialogResult.Yes) return;
                KalaniDagit();
            }

            Eller = MevcutEller();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
