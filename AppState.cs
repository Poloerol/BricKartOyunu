using System.Drawing;

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
    }
}