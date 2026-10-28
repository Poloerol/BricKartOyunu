using System;
using System.IO;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Forms
{
    /// <summary>Öğrenme modunun kalıcı ayarı.</summary>
    public sealed class OgrenmeAyari
    {
        public bool Acik { get; set; }
    }

    /// <summary>
    /// Oyuncunun seçtiği ortaklık anlaşmasını ve öğrenme modunu yükler / kaydeder
    /// (kullanıcı klasöründe, AyarDeposu aracılığıyla). Dosya yoksa varsayılan anlaşma kullanılır.
    /// </summary>
    public static class AnlasmaYonetici
    {
        private const string AnlasmaAdi = "ortaklik_anlasmasi";
        private const string OgrenmeAdi = "ogrenme_modu";

        private static OrtaklikAnlasmasi _mevcut;
        private static OgrenmeAyari _ogrenme;
        private static readonly object _kilit = new object();

        /// <summary>Geçerli ortaklık anlaşmasının kopyası (değiştirmek güvenlidir).</summary>
        public static OrtaklikAnlasmasi Mevcut()
        {
            lock (_kilit)
            {
                if (_mevcut == null)
                {
                    try
                    {
                        _mevcut = File.Exists(AyarDeposu.DosyaYolu(AnlasmaAdi))
                            ? AyarDeposu.Yukle<OrtaklikAnlasmasi>(AnlasmaAdi)
                            : OrtaklikAnlasmasi.Varsayilan();
                    }
                    catch
                    {
                        _mevcut = OrtaklikAnlasmasi.Varsayilan();
                    }
                }
                return _mevcut.Kopya();
            }
        }

        /// <summary>Anlaşmayı kaydeder ve geçerli yapar. Sonraki ihale oturumları bunu kullanır.</summary>
        public static bool Kaydet(OrtaklikAnlasmasi anlasma)
        {
            if (anlasma == null) return false;
            lock (_kilit)
            {
                _mevcut = anlasma.Kopya();
                return AyarDeposu.Kaydet(AnlasmaAdi, anlasma);
            }
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
