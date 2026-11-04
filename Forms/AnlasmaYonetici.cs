using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Forms
{
    /// <summary>Öğrenme modunun kalıcı ayarı.</summary>
    public sealed class OgrenmeAyari
    {
        public bool Acik { get; set; }
    }

    /// <summary>Adlandırılmış ortaklık profili (anlaşma + agresiflik + beceri).</summary>
    public sealed class AnlasmaProfil
    {
        public string Ad { get; set; } = "Varsayılan";
        public OrtaklikAnlasmasi Anlasma { get; set; } = OrtaklikAnlasmasi.Varsayilan();

        /// <summary>Hazır (sabit) profil: konvansiyon seçimleri değiştirilemez ve silinemez; her açılışta ön ayara göre yenilenir.</summary>
        public bool Sabit { get; set; }

        /// <summary>Profilde kayıtlı ihale sistemi adı ("İhale Sistemi:" satırında gösterilir).</summary>
        public string Sistem { get; set; }

        /// <summary>yeniAd verilirse kopya sabit değildir (kullanıcı profili olur); verilmezse aynı profilin bağımsız kopyasıdır.</summary>
        public AnlasmaProfil Kopya(string yeniAd = null)
        {
            return new AnlasmaProfil { Ad = yeniAd ?? Ad, Anlasma = Anlasma.Kopya(), Sabit = yeniAd == null && Sabit, Sistem = Sistem };
        }

        /// <summary>Sistem adını, anlaşmanın seçimleri bir ön ayara uyuyorsa onun adı, uymuyorsa "Özel" olarak belirler.</summary>
        public void SistemAdiniBelirle()
        {
            Sistem = AnlasmaForm.SistemAdiBul(Anlasma);
        }
    }

    /// <summary>Masanın iki tarafı: Kuzey-Güney ve Doğu-Batı. Her tarafın ayrı ortaklık profilleri vardır.</summary>
    public enum Taraf { NS, EW }

    public sealed class ProfilDeposu
    {
        public List<AnlasmaProfil> Profiller { get; set; } = new List<AnlasmaProfil>();
        public string Aktif { get; set; }
    }

    /// <summary>
    /// Ortaklık profillerini ve öğrenme modunu yükler / kaydeder (kullanıcı klasöründe, AyarDeposu aracılığıyla).
    /// NS ve EW için ayrı profil depoları vardır. Dosya yoksa tek profilli varsayılan depo kullanılır;
    /// eski tek depo veya tek anlaşma dosyası varsa ilk açılışta her iki tarafa kopyalanır.
    /// </summary>
    public static class AnlasmaYonetici
    {
        private const string EskiAnlasmaAdi = "ortaklik_anlasmasi";
        private const string EskiProfilAdi = "ortaklik_profilleri";
        private const string OgrenmeAdi = "ogrenme_modu";

        private static readonly Dictionary<Taraf, ProfilDeposu> _depolar = new Dictionary<Taraf, ProfilDeposu>();
        private static OgrenmeAyari _ogrenme;
        private static readonly object _kilit = new object();

        public static Taraf TarafOf(Player oyuncu)
        {
            return (oyuncu == Player.Kuzey || oyuncu == Player.Guney) ? Taraf.NS : Taraf.EW;
        }

        private static string DepoAdi(Taraf t)
        {
            return EskiProfilAdi + (t == Taraf.NS ? "_ns" : "_ew");
        }

        /// <summary>
        /// Hazır (sabit) profilleri listenin başına koyar ve ön ayara göre yeniler (agresiflik, beceri ve süre ayarları korunur).
        /// Aynı adlı bir kullanıcı profili varsa adı "(kullanıcı)" ile ayrılır. Sistem adı boş olan profillerin adı belirlenir.
        /// </summary>
        internal static void SabitleriGarantile(ProfilDeposu d)
        {
            var sabitler = new List<AnlasmaProfil>();
            foreach (var o in AnlasmaForm.OnAyarlar)
            {
                var var_ = d.Profiller.FirstOrDefault(p => p.Ad == o.Ad);
                if (var_ != null && !var_.Sabit)
                {
                    string yeniAd = var_.Ad + " (kullanıcı)";
                    if (d.Aktif == var_.Ad) d.Aktif = yeniAd;
                    var_.Ad = yeniAd;
                    var_ = null;
                }
                var uret = o.Uret();
                if (var_ == null) var_ = new AnlasmaProfil { Ad = o.Ad, Anlasma = uret };
                else AnlasmaForm.OnAyarKopyala(var_.Anlasma, uret);
                var_.Sabit = true;
                var_.Sistem = o.Ad;
                sabitler.Add(var_);
            }
            d.Profiller.RemoveAll(p => p.Sabit);
            d.Profiller.InsertRange(0, sabitler);
            foreach (var p in d.Profiller)
            {
                if (p.Sistem == "2/1 Game Force Temel") p.Sistem = "2/1 Game Force - Temel";   // eski ad
                if (string.IsNullOrEmpty(p.Sistem)) p.SistemAdiniBelirle();
            }
        }

        private static ProfilDeposu Yukle(string ad)
        {
            try
            {
                if (File.Exists(AyarDeposu.DosyaYolu(ad)))
                    return AyarDeposu.Yukle<ProfilDeposu>(ad);
            }
            catch { }
            return null;
        }

        private static ProfilDeposu Depo(Taraf t)
        {
            lock (_kilit)
            {
                ProfilDeposu d;
                if (_depolar.TryGetValue(t, out d)) return d;

                d = Yukle(DepoAdi(t)) ?? Yukle(EskiProfilAdi);
                if (d == null || d.Profiller == null || d.Profiller.Count == 0)
                {
                    d = new ProfilDeposu();
                    OrtaklikAnlasmasi eski = null;
                    try
                    {
                        if (File.Exists(AyarDeposu.DosyaYolu(EskiAnlasmaAdi)))
                            eski = AyarDeposu.Yukle<OrtaklikAnlasmasi>(EskiAnlasmaAdi);
                    }
                    catch { eski = null; }
                    d.Profiller.Add(new AnlasmaProfil { Ad = "Varsayılan", Anlasma = eski ?? OrtaklikAnlasmasi.Varsayilan() });
                    d.Aktif = "Varsayılan";
                    if (t == Taraf.EW) d.Aktif = EwVarsayilanProfil;   // yeni kurulumda E/W: 2/1 Game Force - Temel
                }
                foreach (var p in d.Profiller)
                    if (p.Anlasma == null) p.Anlasma = OrtaklikAnlasmasi.Varsayilan();
                SabitleriGarantile(d);
                if (d.Profiller.All(p => p.Ad != d.Aktif)) d.Aktif = d.Profiller[0].Ad;
                _depolar[t] = d;
                return d;
            }
        }

        /// <summary>Etkin profilin anlaşmasının kopyası (değiştirmek güvenlidir).</summary>
        public static OrtaklikAnlasmasi Mevcut(Taraf t)
        {
            lock (_kilit)
            {
                var d = Depo(t);
                return d.Profiller.First(p => p.Ad == d.Aktif).Anlasma.Kopya();
            }
        }

        public static string AktifProfilAdi(Taraf t)
        {
            lock (_kilit) { return Depo(t).Aktif; }
        }

        /// <summary>Bir tarafın tüm profillerinin kopyası (form bunlar üzerinde çalışır, Kaydet ile geri yazılır).</summary>
        public static List<AnlasmaProfil> Profiller(Taraf t)
        {
            lock (_kilit) { return Depo(t).Profiller.Select(p => p.Kopya()).ToList(); }
        }

        /// <summary>Profilleri ve etkin profili kaydeder; sonraki ihaleler etkin profili kullanır.</summary>
        public static bool Kaydet(Taraf t, List<AnlasmaProfil> profiller, string aktif)
        {
            if (profiller == null || profiller.Count == 0) return false;
            lock (_kilit)
            {
                Surum++;
                var d = new ProfilDeposu
                {
                    Profiller = profiller.Select(p => p.Kopya()).ToList(),
                    Aktif = profiller.Any(p => p.Ad == aktif) ? aktif : profiller[0].Ad
                };
                SabitleriGarantile(d);
                _depolar[t] = d;
                return AyarDeposu.Kaydet(DepoAdi(t), d);
            }
        }

        /// <summary>
        /// Ortaklık ayarlarını varsayılana döndürür: her iki tarafın profilleri silinip tek "Varsayılan" profil kalır,
        /// öğrenme modu kapanır. Değişiklik hemen kalıcı depoya da yazılır.
        /// </summary>
        /// <summary>Profiller her kaydedildiğinde/sıfırlandığında artar; açık formlar bununla anlaşmayı yeniden okumayı bilir.</summary>
        public static int Surum { get; private set; }

        /// <summary>E/W tarafının yeni kurulumda ve sıfırlamada etkin profili.</summary>
        private const string EwVarsayilanProfil = "2/1 Game Force - Temel";

        public static bool VarsayilanaDon()
        {
            bool tamam = true;
            lock (_kilit)
            {
                Surum++;
                foreach (var t in new[] { Taraf.NS, Taraf.EW })
                {
                    var d = new ProfilDeposu { Aktif = "Varsayılan" };
                    d.Profiller.Add(new AnlasmaProfil { Ad = "Varsayılan", Anlasma = OrtaklikAnlasmasi.Varsayilan() });
                    SabitleriGarantile(d);
                    if (t == Taraf.EW) d.Aktif = EwVarsayilanProfil;
                    _depolar[t] = d;
                    tamam &= AyarDeposu.Kaydet(DepoAdi(t), d);
                }
                _ogrenme = new OgrenmeAyari();
                tamam &= AyarDeposu.Kaydet(OgrenmeAdi, _ogrenme);
            }
            return tamam;
        }

        private static OgrenmeAyari Ogrenme()
        {
            lock (_kilit)
            {
                if (_ogrenme == null)
                {
                    try { _ogrenme = AyarDeposu.Yukle<OgrenmeAyari>(OgrenmeAdi); }
                    catch { _ogrenme = new OgrenmeAyari(); }
                }
                return _ogrenme;
            }
        }

        /// <summary>Öğrenme modu: ihalede teklifler motor önerisiyle karşılaştırılır, her teklifin anlamı açıklanır.</summary>
        public static bool OgrenmeModuAcik
        {
            get { return Ogrenme().Acik; }
            set
            {
                var o = Ogrenme();
                if (o.Acik == value) return;
                o.Acik = value;
                AyarDeposu.Kaydet(OgrenmeAdi, o);
            }
        }
    }
}
