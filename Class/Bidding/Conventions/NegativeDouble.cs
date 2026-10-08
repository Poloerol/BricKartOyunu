using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Negative Double (Negatif Kontr) Konvansiyonu (SAYC Standard).
    ///
    /// Kural:
    /// - Rakip 1 seviyede bir renk açtı.
    /// - Partner pas geçti (veya henüz konuşmadı).
    /// - Elimizde rakip rengi dışında, özellikle 4'lü bir majör var.
    /// - Dbl diyerek "diğer renklerde (özellikle majörlerde) değerim var" mesajı verilir.
    ///
    /// Öncelik: 20 (SAYC standartlarında müdahale cevapları arasında)
    /// </summary>
    public class NegativeDouble : IKonvansiyon
    {
        public string Ad => "Negative Double";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 20;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. Rakip bir renk açmış olmalı
            if (!durum.RakipActiMi()) return false;

            // 2. Son teklif rakibe ait ve 1 seviyede olmalı
            string sonRakipTeklifi = durum.RakipTeklifleri.LastOrDefault();
            if (string.IsNullOrEmpty(sonRakipTeklifi) || TekliftenSeviyeCikar(sonRakipTeklifi) != 1)
                return false;

            // 3. Kendi tekliflerim boş olmalı
            if (durum.KendiTeklifleri.Count > 0) return false;

            // 4. Partner konuşmamış olmalı (SAYC'de partner açtıysa Negatif Kontr yapılmaz)
            if (durum.PartnerTeklifleri.Count > 0) return false;

            // 5. El gücü kontrolü (Genellikle 6+ HCP)
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 6) return false;

            // 6. Majör kontrolü (SAYC: En az bir 4'lü majör veya kuvvetli bir el)
            string rakipKozu = TekliftenKozCikar(sonRakipTeklifi);
            if (string.IsNullOrEmpty(rakipKozu)) return false;

            int macaUzun = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Maça");
            int kupaUzun = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, "Kupa");

            // Rakip Maça açtıysa -> 4+ Kupa aranır
            if (rakipKozu == "Maça" && kupaUzun < 4) return false;
            // Rakip Kupa açtıysa -> 4+ Maça aranır
            if (rakipKozu == "Kupa" && macaUzun < 4) return false;
            // Rakip minör açtıysa -> En az bir 4'lü majör aranır
            if (rakipKozu != "Maça" && rakipKozu != "Kupa" && macaUzun < 4 && kupaUzun < 4)
                return false;

            // 7. Michaels kontrolü: 5-5 majör varsa Michaels'a bırak
            if (macaUzun >= 5 && kupaUzun >= 5) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "Dbl";
        }

        private string TekliftenKozCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif)) return null;
            char son = teklif[teklif.Length - 1];
            return son switch { '♠' => "Maça", '♥' => "Kupa", '♦' => "Karo", '♣' => "Sinek", 'T' => "NT", 't' => "NT", _ => null };
        }

        private int TekliftenSeviyeCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2) return 0;
            if (int.TryParse(teklif.Substring(0, teklif.Length - 1), out int s)) return s;
            return 0;
        }
    }
}
