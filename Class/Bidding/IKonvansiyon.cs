using System.Collections.Generic;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// Tüm ihale konvansiyonlarının uygulayacağı arayüz.
    /// 
    /// Her konvansiyon (BesliMajor, Stayman, JacobyTransfer, vs.) bu arayüzü
    /// uygular. IhaleMotoru, aktif konvansiyonları sırayla dener ve
    /// hangisi uygunsa ondan teklif alır.
    /// 
    /// Örnek kullanım:
    ///   public class BesliMajor : IKonvansiyon
    ///   {
    ///       public string Ad => "5'li Majör";
    ///       public bool AktifMi { get; set; } = true;
    ///       
    ///       public bool UygunMu(IhaleDurumu durum) 
    ///       { 
    ///           return durum.IlkTeklifMi() && 
    ///                  ElDegerlendirici.BesliMajorVar(durum.AktifOyuncuEli);
    ///       }
    ///       
    ///       public string TeklifVer(IhaleDurumu durum) 
    ///       {
    ///           var el = durum.AktifOyuncuEli;
    ///           int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
    ///           int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
    ///           
    ///           // Önce Maça, sonra Kupa
    ///           if (macca >= kupa) return "1♠";
    ///           return "1♥";
    ///       }
    ///   }
    /// </summary>
    public interface IKonvansiyon
    {
        /// <summary>
        /// Konvansiyonun adı (kullanıcıya gösterilir).
        /// Örnek: "Stayman", "Jacoby Transfer", "Blackwood"
        /// </summary>
        string Ad { get; }

        /// <summary>
        /// Bu konvansiyon şu an aktif mi?
        /// Ortaklık anlaşmasına göre değişir.
        /// </summary>
        bool AktifMi { get; set; }

        /// <summary>
        /// Bu konvansiyon mevcut durumda geçerli mi?
        /// Örneğin Stayman sadece 1NT açılışına cevap verirken geçerlidir.
        /// </summary>
        bool UygunMu(IhaleDurumu durum);

        /// <summary>
        /// Bu durumda verilecek teklifi döndürür.
        /// Örnek: "2♣", "1♠", "Pas", "Dbl", "4NT"
        /// 
        /// NOT: Bu metot sadece UygunMu() true döndüğünde çağrılır.
        /// </summary>
        string TeklifVer(IhaleDurumu durum);

        /// <summary>
        /// Konvansiyonun önceliği (küçük sayı = önce dener).
        /// Örneğin Stayman (10) Blackwood'dan (50) önce denenmeli.
        /// Varsayılan: 100
        /// </summary>
        int Oncelik { get; }
    }
}