using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu
{
    /// <summary>
    /// Uygulama genelinde paylaşılan durum bilgilerini (state) tutar.
    /// AnaSayfa, BricOyna, KartDagitici ve diğer formlar tarafından kullanılır.
    /// 
    /// Not: Bu sınıf, daha önce AnaSayfa içinde tanımlı olan static alanların
    /// yeni evidir. Geçiş sürecinde AnaSayfa içinde facade property'ler bırakıldı,
    /// böylece mevcut kod kırılmadı.
    /// </summary>
    public static class AppState
    {
        /// <summary>
        /// Seçili kart seti. Değerler: "Classic", "Sembols" vb.
        /// Varsayılan: "Classic"
        /// </summary>
        public static string SeciliKartSeti { get; set; } = "Classic";

        /// <summary>
        /// Masa (oyun tahtası) arka plan rengi. Varsayılan: Yeşil.
        /// </summary>
        public static Color MasaRengi { get; set; } = Color.Green;

        // ── Kart arka yüzü: Resources\Cards\Backs içindeki iki resimden biri ──
        public const string ArkaYuzKirmizi = "Back.png";
        public const string ArkaYuzSiyah = "BackBlack.png";

        /// <summary>Seçili arka yüz dosya adı (yalnızca ArkaYuzKirmizi / ArkaYuzSiyah).</summary>
        public static string ArkaYuzDosya { get; set; } = ArkaYuzKirmizi;

        public static string ArkaYuzYolu(string dosya)
        {
            return Path.Combine(Application.StartupPath, "Resources", "Cards", "Backs", dosya);
        }

        /// <summary>Seçili arka yüz resminin tam yolu.</summary>
        public static string ArkaYuzYolu()
        {
            return ArkaYuzYolu(ArkaYuzDosya);
        }

        private static bool GecerliArkaYuz(string dosya)
        {
            return dosya == ArkaYuzKirmizi || dosya == ArkaYuzSiyah;   // dışarıdan gelen ada güvenme
        }

        public class GorunumAyari
        {
            public string ArkaYuz { get; set; } = ArkaYuzKirmizi;
        }

        /// <summary>Kayıtlı arka yüz seçimini yükler (yoksa/bozuksa kırmızı).</summary>
        public static void ArkaYuzuYukle()
        {
            try
            {
                var g = AyarDeposu.Yukle<GorunumAyari>("gorunum");
                ArkaYuzDosya = (g != null && GecerliArkaYuz(g.ArkaYuz)) ? g.ArkaYuz : ArkaYuzKirmizi;
            }
            catch { ArkaYuzDosya = ArkaYuzKirmizi; }
        }

        public static void ArkaYuzuKaydet()
        {
            try { AyarDeposu.Kaydet("gorunum", new GorunumAyari { ArkaYuz = ArkaYuzDosya }); }
            catch { }
        }
    }
}