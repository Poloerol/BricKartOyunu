using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Takeout Double (T/O DBL) — Rakip 1 seviyesinde bir renk açtığında
    /// yapılan yapıcı (takeout) kontr.
    /// 
    /// Kural:
    /// - Rakip 1♣/1♦/1♥/1♠ açtı.
    /// - Benim 12+ HP'm var.
    /// - Rakip renkte 0-2 kart (kısa).
    /// - Diğer 3 renkte en az 3 kart (tercihen 4+).
    /// - 5'li majör varsa doğal müdahale tercih edilir (DBL yerine).
    /// 
    /// Öncelik: 24 (Negative Double 20'den sonra).
    /// </summary>
    public class TakeoutDouble : IKonvansiyon
    {
        public string Ad => "Takeout Double";
        public string Aciklama => "Rakip açışına yapıcı kontr (12+ HP, kısa rakip renk)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 24;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. İhale bitmemiş olmalı
            if (durum.IhaleBittiMi()) return false;

            // 1b. Partner zaten Dbl yaptıysa ben cevap veren taraftayım (TakeoutDoubleCevap).
            //     Dbl "gerçek teklif" sayılmadığı için Gecmis üzerinden aranır.
            if (durum.Gecmis != null &&
                durum.Gecmis.Any(h => h.Oyuncu == durum.Partner && h.Teklif == "Dbl"))
                return false;

            // 2. Rakip açtı mı?
            if (!durum.RakipActiMi()) return false;

            // 3. Ben henüz konuşmadım mı?
            if (durum.KendiTeklifleri.Count > 0) return false;

            // 4. Son gerçek teklif rakipten mi?
            if (durum.SonGercekTeklifSahibi == null) return false;
            if (!durum.Rakipler.Contains(durum.SonGercekTeklifSahibi.Value)) return false;

            // 5. Son teklif 1 seviyesinde mi? (2+ seviyesinde T/O DBL genelde farklı)
            int sonSeviye = durum.KacinciSeviye();
            if (sonSeviye != 1) return false;

            string rakipKozu = KozYardimcisi.TekliftenKozCikar(durum.SonGercekTeklif);
            if (string.IsNullOrEmpty(rakipKozu) || rakipKozu == "NT") return false;

            // 6. 12+ HP kontrolü (T/O DBL için minimum)
            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 12) return false;

            // 7. Rakip renkte 0-2 kart (kısa) olmalı
            int rakipRenkUzunluk = ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, rakipKozu);
            if (rakipRenkUzunluk > 2) return false;

            // 8. Şikan (void) veya 5'li yan renk varsa → DBL (doğal müdahale yerine)
            //    Aksi halde 5'li yan renk varsa doğal müdahale tercih edilir.
            if (rakipRenkUzunluk == 0) return true; // Şikan → DBL her zaman

            // 9. 5'li yan renk varsa doğal müdahale tercih edilir (17+ HP hariç:
            //    güçlü elde önce Dbl, sonra renk gösterilir)
            if (hp < 17)
            {
                foreach (var renk in new[] { "Maça", "Kupa", "Karo", "Sinek" })
                {
                    if (renk == rakipKozu) continue;
                    if (ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, renk) >= 5)
                        return false;
                }
            }

            // 10. Diğer renklerde en az 3 kart (tercihen 4+)
            int yeterliRenkSayisi = 0;
            foreach (var renk in new[] { "Maça", "Kupa", "Karo", "Sinek" })
            {
                if (renk == rakipKozu) continue;
                if (ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, renk) >= 3)
                    yeterliRenkSayisi++;
            }
            if (yeterliRenkSayisi < 3) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return "Dbl";
        }
    }
}