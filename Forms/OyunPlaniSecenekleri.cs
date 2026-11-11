using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    // SEÇENEK MODELİ
    // ═══════════════════════════════════════════════════════════════════════

    public enum DagitimKaynagi
    {
        KullaniciGirisi, Rastgele, RastgeleParsiyel, RastgeleOyun, RastgeleSlam,
        Uretilmis, NumaraliRastgele, Kozsuz
    }

    public enum EnIyiEl { Herhangi, KuzeyGuney, DoguBati, Kuzey, Dogu, Guney, Bati }

    public enum DagiticiSecimi { Donusumlu, Kuzey, Dogu, Guney, Bati }

    public enum OyunTuru { Duplicate, Rubber, ChicagoDuplicate, ChicagoRubber, TekEl }

    public enum IpucuGosterimi { Istendiginde, FarkliysaDuyur, FarkliysaGoster, HerZamanGoster }

    /// <summary>Oyun planı ayarlarının tutulduğu veri sınıfı.</summary>
    public class OyunPlaniSecenekleri
    {
        /// <summary>Kayıt biçimi sürümü (ileride alan değişirse eski dosyalar uyarlanır).</summary>
        public int SurumNo = 1;

        // Dağıtım sekmesi
        public DagitimKaynagi DagitimKaynagi = Forms.DagitimKaynagi.Rastgele;
        public EnIyiEl EnIyiEl = Forms.EnIyiEl.Herhangi;
        public DagiticiSecimi Dagitici = DagiticiSecimi.Donusumlu;
        public bool YeniBordNumaralari = false;
        /// <summary>"Kullanıcı girişi" kaynağında oynanacak bordların okunduğu PBN dosyası.</summary>
        public string PbnYolu = "";

        // Oyun sekmesi
        public OyunTuru OyunTuru = Forms.OyunTuru.Duplicate;
        public bool TekrarGorunmez = false;
        public bool TekrarHemen = false;
        public int YapayZekaGecikmesiSn = 0;           // 0, 1 veya 2
        public bool HizliOtomatikOyna = false;
        public bool HerZamanKontratIcinIhale = false;
        public bool OkuGosterme = false;
        public bool ElleriDondurme = false;
        public bool TekKartiOtomatikOyna = true;

        // Giriş sekmesi
        public IpucuGosterimi IpucuGosterimi = Forms.IpucuGosterimi.Istendiginde;
        public bool ElSonrasiDuraklat = true;
        public bool DosyadanIhaleyiGoster = false;
        public bool DosyadanOyunuGoster = false;
        public bool DosyadanTumElleriGoster = false;
        public bool IpuclariniSesliDuyur = true;
        public bool OyunVeIhaleyiSesliDuyur = false;
        public bool Selamlamalar = true;

        // Bord numaralaması: son oynanan bord (Yeni numara kapalıysa sonraki oturum buradan devam eder)
        public int SonBordNo = 0;

        // Oyuncu türleri: sıra Kuzey, Doğu, Güney, Batı (true = insan)
        public bool[] InsanMi = { false, false, true, false };

        // Oyuncu resimleri: sıra Kuzey, Doğu, Güney, Batı (boş = resim yok)
        public string[] ResimYollari = { "", "", "", "" };

        /// <summary>Kalıcı depodaki ad (dosya: %AppData%\BricKartOyunu\oyun_plani.json).</summary>
        public const string DepoAdi = "oyun_plani";

        /// <summary>Kayıtlı ayarları yükler; dosya yoksa ya da bozuksa varsayılanlar gelir.</summary>
        public static OyunPlaniSecenekleri Yukle()
        {
            var o = AyarDeposu.Yukle<OyunPlaniSecenekleri>(DepoAdi);
            o.Duzelt();
            return o;
        }

        /// <summary>Ayarları kalıcı olarak kaydeder. Başarılıysa true.</summary>
        public bool Kaydet()
        {
            Duzelt();
            return AyarDeposu.Kaydet(DepoAdi, this);
        }

        /// <summary>Kayıtlı dosyayı siler; bir sonraki açılışta varsayılanlar kullanılır.</summary>
        /// <summary>Elle düzenlenmiş ya da eski dosyalardaki geçersiz değerleri onarır.</summary>
        private void Duzelt()
        {
            if (InsanMi == null || InsanMi.Length != 4)
            {
                var yeni = new bool[] { false, false, true, false };
                if (InsanMi != null)
                    for (int i = 0; i < Math.Min(4, InsanMi.Length); i++) yeni[i] = InsanMi[i];
                InsanMi = yeni;
            }
            if (ResimYollari == null || ResimYollari.Length != 4)
            {
                var yeni = new string[] { "", "", "", "" };
                if (ResimYollari != null)
                    for (int i = 0; i < Math.Min(4, ResimYollari.Length); i++) yeni[i] = ResimYollari[i] ?? "";
                ResimYollari = yeni;
            }
            for (int i = 0; i < 4; i++) if (ResimYollari[i] == null) ResimYollari[i] = "";
            if (YapayZekaGecikmesiSn < 0) YapayZekaGecikmesiSn = 0;
            if (YapayZekaGecikmesiSn > 2) YapayZekaGecikmesiSn = 2;
            if (SurumNo < 1) SurumNo = 1;
        }

        /// <summary>Yapay zekâ kart/ihale gecikmesi (ms). 0 sn = anlık sayılır (30 ms).</summary>
        public int AiGecikmesiMs()
        {
            return YapayZekaGecikmesiSn <= 0 ? 30 : YapayZekaGecikmesiSn * 1000;
        }

        /// <summary>Otomatik oynatma (tüm eller AI) adım gecikmesi (ms).</summary>
        public int OtomatikOynaGecikmesiMs()
        {
            return HizliOtomatikOyna ? 10 : AiGecikmesiMs();
        }

        /// <summary>Bord numarasına göre fiziksel dağıtıcı (Dönüşümlü: 1=Kuzey, 2=Doğu, 3=Güney, 4=Batı...).</summary>
        public Player FizikselDagitici(int bordNo)
        {
            switch (Dagitici)
            {
                case DagiticiSecimi.Kuzey: return Player.Kuzey;
                case DagiticiSecimi.Dogu: return Player.Dogu;
                case DagiticiSecimi.Guney: return Player.Guney;
                case DagiticiSecimi.Bati: return Player.Bati;
            }
            Player[] sira = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            return sira[(((bordNo - 1) % 4) + 4) % 4];
        }

        public OyunPlaniSecenekleri Klonla()
        {
            var k = (OyunPlaniSecenekleri)MemberwiseClone();
            k.InsanMi = (bool[])InsanMi.Clone();
            k.ResimYollari = (string[])ResimYollari.Clone();
            return k;
        }
    }

    /// <summary>PBN dosyasından okunan tek bord.</summary>
    public class PbnBord
    {
        public string Ad = "";
        public bool DagiticiVar;
        public Player Dagitici;                      // fiziksel koltuk
        public string[] El = new string[4];          // fiziksel Kuzey, Doğu, Güney, Batı: "S.H.D.C"
    }

    /// <summary>Basit PBN okuyucu: [Deal], [Dealer], [Event] etiketlerini okur.</summary>
    public static class PbnOkuyucu
    {
        private static string _yol; private static DateTime _zaman; private static List<PbnBord> _onbellek;
        private static readonly Regex Etiket = new Regex("^\\[(\\w+)\\s+\"(.*)\"\\]\\s*$");

        public static List<PbnBord> Oku(string yol)
        {
            var liste = new List<PbnBord>();
            if (string.IsNullOrEmpty(yol) || !File.Exists(yol)) return liste;
            DateTime z = File.GetLastWriteTimeUtc(yol);
            if (_onbellek != null && _yol == yol && _zaman == z) return _onbellek;

            string ad = "", deal = null, dealer = null;
            foreach (var satir in File.ReadAllLines(yol))
            {
                var m = Etiket.Match(satir.Trim());
                if (!m.Success) continue;
                string tag = m.Groups[1].Value, deger = m.Groups[2].Value;
                if ((tag == "Event" || tag == "Deal") && deal != null)
                {
                    var b = Coz(deal, dealer, ad);
                    if (b != null) liste.Add(b);
                    ad = ""; deal = null; dealer = null;
                }
                if (tag == "Event") ad = deger;
                else if (tag == "Dealer") dealer = deger;
                else if (tag == "Deal") deal = deger;
            }
            if (deal != null)
            {
                var b = Coz(deal, dealer, ad);
                if (b != null) liste.Add(b);
            }
            _yol = yol; _zaman = z; _onbellek = liste;
            return liste;
        }

        private static int KoltukNo(char c)
        {
            switch (char.ToUpperInvariant(c)) { case 'N': return 0; case 'E': return 1; case 'S': return 2; case 'W': return 3; }
            return -1;
        }

        private static PbnBord Coz(string deal, string dealer, string ad)
        {
            if (deal.Length < 3 || deal[1] != ':') return null;
            int bas = KoltukNo(deal[0]);
            if (bas < 0) return null;
            var eller = deal.Substring(2).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (eller.Length != 4) return null;
            var b = new PbnBord { Ad = ad };
            var gorulen = new HashSet<string>();
            for (int j = 0; j < 4; j++)
            {
                var renkler = eller[j].Replace("10", "T").Split('.');
                if (renkler.Length != 4) return null;
                int n = 0;
                for (int r = 0; r < 4; r++)
                {
                    if (renkler[r] == "-") renkler[r] = "";
                    foreach (char k in renkler[r])
                    {
                        if ("AKQJT98765432".IndexOf(char.ToUpperInvariant(k)) < 0) return null;
                        if (!gorulen.Add(r.ToString() + char.ToUpperInvariant(k))) return null;
                        n++;
                    }
                }
                if (n != 13) return null;
                b.El[(bas + j) % 4] = string.Join(".", renkler).ToUpperInvariant();
            }
            if (!string.IsNullOrEmpty(dealer) && KoltukNo(dealer[0]) >= 0)
            {
                b.DagiticiVar = true;
                b.Dagitici = new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati }[KoltukNo(dealer[0])];
            }
            return b;
        }

        /// <summary>Bordu iç koltuk sırasıyla (Kuzey, Doğu, Güney, Batı) 52 kartlık desteye çevirir.</summary>
        public static List<string> Deste(PbnBord b)
        {
            var fizSira = new[] { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            string[] sembol = { "S", "H", "D", "C" };
            var deste = new List<string>(52);
            foreach (var ic in fizSira)
            {
                int idx = Array.IndexOf(fizSira, KoltukHaritasi.Fiz(ic));
                var renkler = b.El[idx].Split('.');
                for (int r = 0; r < 4; r++)
                    foreach (char k in renkler[r])
                        deste.Add((k == 'T' ? "10" : k.ToString()) + sembol[r] + ".png");
            }
            return deste;
        }
    }
}
