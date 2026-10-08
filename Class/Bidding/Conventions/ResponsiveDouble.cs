using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Responsive Double (Tepkisel Kontr) Konvansiyonu (SAYC Standard).
    ///
    /// Kural:
    /// - Rakip 1 seviyede açtı, partner pas geçti ve rakip kendi rengini 2 seviyesinde destekledi.
    /// - Ben Dbl diyerek "diğer renklerde (özellikle majörlerde) değerim var" mesajı veririm.
    ///
    /// Örnek:
    ///   Rakip 1♠ - Partner Pas - Rakip 2♠ - Ben: Dbl
    ///   (Bu durum, elimde majör ve bir minör olduğunu veya iki majör olduğunu gösterir)
    ///
    /// Öncelik: 22
    /// </summary>
    public class ResponsiveDouble : IKonvansiyon
    {
        public string Ad => "Responsive Double";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 22;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Senaryo Kontrolü: Rakip 1X -> Partner Pas -> Rakip 2X
            var gecmis = durum.Gecmis.Where(h => h.GercekTeklifMi || h.PasMi).ToList();
            if (gecmis.Count < 3) return false;

            var son = gecmis.Last();
            var onceki = gecmis[gecmis.Count - 2];
            var ilk = gecmis[gecmis.Count - 3];

            // 1. Son teklif rakip tarafından 2 seviyesinde verilmiş olmalı
            if (!durum.Rakipler.Contains(son.Oyuncu) || TekliftenSeviyeCikar(son.Teklif) != 2)
                return false;

            // 2. Partner pas geçmiş olmalı
            if (onceki.Oyuncu != durum.Partner || !onceki.PasMi)
                return false;

            // 3. İlk teklif rakip tarafından 1 seviyesinde verilmiş olmalı
            if (!durum.Rakipler.Contains(ilk.Oyuncu) || TekliftenSeviyeCikar(ilk.Teklif) != 1)
                return false;

            // 4. Rakip aynı rengi desteklemiş olmalı
            if (TekliftenKozCikar(ilk.Teklif) != TekliftenKozCikar(son.Teklif))
                return false;

            // 5. Kendi tekliflerim boş olmalı
            if (durum.KendiTeklifleri.Count > 0) return false;

            // 6. El gücü ve dağılım kontrolü (En az 6-8 HP ve yan renklerde değer)
            var el = durum.AktifOyuncuEli;
            if (ElDegerlendirici.HCP(el) < 6) return false;

            // Yan renklerde en az bir tane 4+ kart olması beklenir
            string rakipKozu = TekliftenKozCikar(ilk.Teklif);
            bool yanRenkDegerli = false;
            foreach (var renk in new[] { "Maça", "Kupa", "Karo", "Sinek" })
            {
                if (renk == rakipKozu) continue;
                if (ElDegerlendirici.RenkUzunlugu(el, renk) >= 4)
                {
                    yanRenkDegerli = true;
                    break;
                }
            }

            return yanRenkDegerli;
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
