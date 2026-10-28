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

        public AnlasmaProfil Kopya(string yeniAd = null)
        {
            return new AnlasmaProfil { Ad = yeniAd ?? Ad, Anlasma = Anlasma.Kopya() };
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
                }
                foreach (var p in d.Profiller)
                    if (p.Anlasma == null) p.Anlasma = OrtaklikAnlasmasi.Varsayilan();
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
                var d = new ProfilDeposu
                {
                    Profiller = profiller.Select(p => p.Kopya()).ToList(),
                    Aktif = profiller.Any(p => p.Ad == aktif) ? aktif : profiller[0].Ad
                };
                _depolar[t] = d;
                return AyarDeposu.Kaydet(DepoAdi(t), d);
            }
        }

        /// <summary>Etkin profile hazır basit anlaşmayı (OrtaklikAnlasmasi.Basit) uygular; agresiflik ve beceri korunur.</summary>
        public static bool BasitUygula(Taraf t)
        {
            var liste = Profiller(t);
            string aktif = AktifProfilAdi(t);
            var p = liste.First(x => x.Ad == aktif);
            var basit = OrtaklikAnlasmasi.Basit();
            basit.AcilisAgresifligi = p.Anlasma.AcilisAgresifligi;
            basit.MudahaleAgresifligi = p.Anlasma.MudahaleAgresifligi;
            basit.PreemptAgresifligi = p.Anlasma.PreemptAgresifligi;
            basit.BeceriDuzeyi = p.Anlasma.BeceriDuzeyi;
            p.Anlasma = basit;
            return Kaydet(t, liste, aktif);
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
