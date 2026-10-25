using BricKartOyunu.Class.Bidding;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Zayıf İki (Weak 2-Bid)
    /// 2♦, 2♥, 2♠ açılışları 5-10 HP + 6'lı renk gösterir.
    /// Amaç: bozucu (preemptive) ve tanımlayıcı.
    /// 2♣ güçlü yapay açılış olarak ayrı kalır (IkiliSinekGuclu).
    /// </summary>
    public class ZayifIki : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════
        // IKonvansiyon ÜYELERİ
        // ═══════════════════════════════════════════════════════════════

        public string Ad => "Zayıf İki";   // ← Motor case etiketiyle eşleşmeli
        public string Aciklama => "6'lı renkle zayıf iki açışı";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 8;

        public bool UygunMu(IhaleDurumu durum)
        {
            // IlkTeklifMi: boş geçmiş VEYA yalnızca paslar (3./4. koltuk zayıf iki)
            if (durum == null || !durum.IlkTeklifMi())
                return false;

            if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13)
                return false;

            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);

            if (hp >= 22) return false;   // 2♣ güçlü açılışa bırak
            if (hp < 5 || hp > 10) return false;

            if (hp >= 15 && ElDegerlendirici.DengeliEl(durum.AktifOyuncuEli))
                return false;              // 1NT açılışına bırak

            return ZayifRenkVarMi(durum.AktifOyuncuEli);
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            if (ElDegerlendirici.RenkUzunlugu(el, "Maça") == 6)
                return KozYardimcisi.TeklifOlustur(2, "Maça");
            if (ElDegerlendirici.RenkUzunlugu(el, "Kupa") == 6)
                return KozYardimcisi.TeklifOlustur(2, "Kupa");
            if (ElDegerlendirici.RenkUzunlugu(el, "Karo") == 6)
                return KozYardimcisi.TeklifOlustur(2, "Karo");

            return "Pas";
        }

        private static bool ZayifRenkVarMi(List<Card> el)
        {
            return ElDegerlendirici.RenkUzunlugu(el, "Maça") == 6
                || ElDegerlendirici.RenkUzunlugu(el, "Kupa") == 6
                || ElDegerlendirici.RenkUzunlugu(el, "Karo") == 6;
        }
    }
}