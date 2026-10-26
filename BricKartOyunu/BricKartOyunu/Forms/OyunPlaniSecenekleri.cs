using System;
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
        public static bool KayitliAyariSil()
        {
            return AyarDeposu.Sil(DepoAdi);
        }

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
}
