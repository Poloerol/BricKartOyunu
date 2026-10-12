using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Namyats (Güçlü Majör Açılışları)
    /// 
    /// 4♣ = güçlü 4♥ açılışı (uzun ve sağlam kör)
    /// 4♦ = güçlü 4♠ açılışı (uzun ve sağlam pik)
    /// 
    /// Amaç: doğrudan 4♥/4♠ açmaya çok güçlü ama 2♣ açmaya yeterince
    /// güçlü olmayan elleri tanımlamak. Ortak, slam ilgisi gösterebilir.
    /// 
    /// "Güçlü" = 8+ löve, sağlam majör rengi (en fazla 1 kayıp),
    /// yan renklerde en fazla 1 As.
    /// 
    /// 4♥/4♠ normal preemptif açılışlar (Preempt.cs) olarak kalır.
    /// 
    /// ÖNEMLİ: Bu konvansiyon aktifken, Preempt.cs'teki 4♣/4♦
    /// devre dışı kalır (OrtaklikAnlasmasi.Namyats = true).
    /// </summary>
    public class Namyats : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════
        // IKonvansiyon ÜYELERİ
        // ═══════════════════════════════════════════════════════════════

        public string Ad => "Namyats";
        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 7: Preempt (9) ve Zayıf İki (8) öncesi çalışsın.
        /// Çünkü 4♣/4♦ Preempt'ten önce kontrol edilmeli.
        /// </summary>
        public int Oncelik => 7;

        // ═══════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            // Sadece açılış (geçmiş boş)
            if (durum == null || durum.Gecmis == null || durum.Gecmis.Count > 0)
                return false;

            if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13)
                return false;

            // 8+ kartlı majör renk var mı? (♥ veya ♠)
            int kupa = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Kupa");
            int maca = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Maça");

            if (kupa < 8 && maca < 8)
                return false;

            // HP aralığı: Namyats "güçlü" majör için ~10-15 HP
            // (Preempt 0-9 HP, bu yüzden çakışmaz)
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 10 || hp > 15)
                return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // TEKLİF
        // ═══════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");

            // Önce ♥ (daha uzun olanı tercih et, eşitse ♥ önce)
            if (kupa >= 8 && kupa >= maca)
                return "4♣";   // 4♣ = güçlü ♥

            if (maca >= 8)
                return "4♦";   // 4♦ = güçlü ♠

            return "Pas";
        }
    }
}