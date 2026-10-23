using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>
    /// 1♣ ⇒ 1♦ GELİŞİMİ (Tablo 1, Tablo 2 ve Röle Cevapları)
    /// Kitaptaki SAS sistemine birebir uyumludur.
    /// </summary>
    public class OneClub_OneDiamond : IAcilisGelisimi
    {
        public string Aciklama => "1♣-1♦ gelişimi (Tablo 1 ve Tablo 2)";
        public int Oncelik => 40;

        public bool UygunMu(List<Card> el, IhaleDurumu durum)
        {
            if (durum == null || el == null) return false;
            if (el.Count != 13) return false;

            var gecmis = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (gecmis.Count < 2) return false;

            // 1) İlk iki teklif: 1♣ - 1♦
            if (gecmis[0].Teklif != "1♣" || gecmis[1].Teklif != "1♦") return false;

            // 2) Bu ikisi partner olmalı
            Player acici = gecmis[0].Oyuncu;
            Player cevapci = gecmis[1].Oyuncu;
            if (IhaleDurumu.PartnerBul(acici) != cevapci) return false;

            // 3) Aktif oyuncu bu ikisinden biri olmalı
            bool aktifAcici = (durum.AktifOyuncu == acici);
            bool aktifCevapci = (durum.AktifOyuncu == cevapci);
            if (!aktifAcici && !aktifCevapci) return false;

            // 4) Rakip araya girmemiş (sadece Pas olabilir)
            bool rakipArayaGirdi = gecmis
                .Where(h => h.Oyuncu != acici && h.Oyuncu != cevapci)
                .Any(h => h.Teklif != "Pas");
            if (rakipArayaGirdi) return false;

            // 5) Rolü belirle
            if (aktifAcici && gecmis.Count == 2) return true;   // Tablo 1
            if (aktifCevapci && gecmis.Count == 3 && gecmis[2].Oyuncu == acici) return true;  // Tablo 2
            if (aktifAcici && gecmis.Count == 4 && gecmis[3].Oyuncu == cevapci) return true;  // Tablo 2 röle cevabı

            return false;
        }

        public string TeklifVer(List<Card> el, IhaleDurumu durum)
        {
            if (el == null) return "Pas";

            var gecmis = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (gecmis.Count < 2) return "Pas";

            Player acici = gecmis[0].Oyuncu;
            Player cevapci = gecmis[1].Oyuncu;

            // Rol 1: Açan rebid (Tablo 1)
            if (durum.AktifOyuncu == acici && gecmis.Count == 2)
                return Tablo1Teklif(el);

            // Rol 2: Cevapçı devam (Tablo 2)
            if (durum.AktifOyuncu == cevapci && gecmis.Count == 3 && gecmis[2].Oyuncu == acici)
                return Tablo2IlkDevam(el, gecmis[2].Teklif);

            // Rol 3: Açan röle cevabı (Tablo 2)
            if (durum.AktifOyuncu == acici && gecmis.Count == 4 && gecmis[3].Oyuncu == cevapci)
                return Tablo2RoleCevabi(el, gecmis[2].Teklif, gecmis[3].Teklif);

            return "Pas";
        }

        // =========================================================================
        // TABLO 1 METODU
        // =========================================================================
        private string Tablo1Teklif(List<Card> el)
        {
            int hcp = ElDegerlendirici.HCP(el);
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            bool dengeli = ElDegerlendirici.DengeliEl(el);
            bool besliMajorVar = (s >= 5 || h >= 5);

            // 1. 18-21 HP özel dağılımlar (4-4-4-1)
            if (hcp >= 18 && hcp <= 21 && c >= 4)
            {
                if (d >= 4 && s >= 4 && h == 1) return "3♥";
                if (d >= 4 && h >= 4 && s == 1) return "3♠";
                if (h >= 4 && s >= 4 && d == 1) return "3NT";
            }

            // 2. 18+ HP, 5+♣ iki renkli
            if (hcp >= 18 && c >= 5)
            {
                if (h >= 4 && s < 4) return "2♥";
                if (s >= 4 && h < 4) return "2♠";
                if (c >= 6 || d >= 4) return "2NT";
            }

            // 3. 15-17 HP, 6+♣ veya 5♣4♦
            if (hcp >= 15 && hcp <= 17)
            {
                if (c >= 6) return "3♣";
                if (c >= 5 && d >= 4) return "3♦";
            }

            // 4. 1♥ (4+♣ 4+♥, 11-17 HP)
            if (hcp >= 11 && hcp <= 17 && c >= 4 && h >= 4)
                return "1♥";

            // 5. 1♠ (4+♣ 4+♠, 11-17 HP)
            if (hcp >= 11 && hcp <= 17 && c >= 4 && s >= 4)
                return "1♠";

            // 6. 2♣ (5+♣, 11-14 HP) — 6'lı ♣ varsa, dengeli olsa bile 2♣
            if (hcp >= 11 && hcp <= 14 && c >= 6)
                return "2♣";

            // 6b. 2♣ (5+♣, 11-14 HP, dengesiz)
            if (hcp >= 11 && hcp <= 14 && c >= 5 && !dengeli)
                return "2♣";

            // 7. 1NT (11-14 HP dengeli, 5'li majör yok)
            if (hcp >= 11 && hcp <= 14 && dengeli && !besliMajorVar)
                return "1NT";

            // 8. 2♦ (5-9 HP, 6+♦ — zayıf, dağılımsal)
            if (hcp >= 5 && hcp <= 9 && d >= 6)
                return "2♦";

            // 9. FALLBACK
            return "1NT";
        }

        // =========================================================================
        // TABLO 2 İLK DEVAM METODU (Cevapçının İlk Cevabı)
        // =========================================================================
        private string Tablo2IlkDevam(List<Card> el, string aciciRebid)
        {
            int hcp = ElDegerlendirici.HCP(el);
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");

            switch (aciciRebid)
            {
                case "1♥":
                    if (hcp >= 9 && hcp <= 11 && d >= 6) return "2♦"; // 6♦ 9-11
                    if (hcp >= 8 && ElDegerlendirici.DengeliEl(el)) return "2NT"; // Limit
                    if (hcp >= 12) return "2♣"; // Röle SI (Gelişmeler Natürel)
                    break;

                case "2♣":
                    if (hcp >= 10 && hcp <= 11 && d >= 6 && s >= 4) return "2♦";
                    if (hcp >= 10 && hcp <= 11 && d >= 5 && s >= 4 && h >= 3 && c == 1) return "2♥";
                    return "2♠"; // Röle

                case "2♦":
                    return "2♠"; // Röle

                case "2♥":
                    return "2♠"; // Röle

                case "2♠":
                    // 4♣ 4♥ 4♠ 1♦ (11-14) - pas veya 4♠ oynama arzusu
                    return "Pas";

                case "2NT":
                    return "3♣"; // Röle

                case "3♣":
                case "3♦":
                case "3♠":
                    return "Pas"; // Oynama arzusu
            }

            return "Pas";
        }

        // =========================================================================
        // TABLO 2 RÖLE CEVAPLARI METODU (Açıcının Röleye Cevabı)
        // =========================================================================
        private string Tablo2RoleCevabi(List<Card> el, string aciciRebid, string cevapciRole)
        {
            int hcp = ElDegerlendirici.HCP(el);
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");

            // Eğer Cevapçı Röle dediyse, Açıcı'nın (bizim) vereceği cevaplar:
            if (cevapciRole == "2♠")
            {
                if (aciciRebid == "2♣") // Açıcının 2♣ Rölesine cevap
                {
                    if (ElDegerlendirici.RenkUzunlugu(el, "Karo") == 1) return "2NT"; // SNG ♦
                    if (ElDegerlendirici.RenkUzunlugu(el, "Maça") == 1) return "3♣"; // SNG ♠
                }
                else if (aciciRebid == "2♦") // 5♣4♥3♦1♠ veya 4♣4♥4♦1♠
                {
                    if (c == 5 && h == 4 && d == 3 && s == 1 && hcp >= 14) return "2NT"; // 5431 14-16
                    if (c == 5 && h == 4 && d == 3 && s == 1 && hcp >= 13) return "3♣"; // 5431 13-14
                    if ((c == 5 && h == 4 && d == 3 && s == 1) || (c == 4 && h == 4 && d == 4 && s == 1)) return "3♦"; // 5431 veya 4441 min
                }
                else if (aciciRebid == "2♥") // 6+♣ 4♥ 14-17
                {
                    if (ElDegerlendirici.RenkUzunlugu(el, "Karo") == 1) return "2NT"; // SNG ♦
                    if (ElDegerlendirici.RenkUzunlugu(el, "Maça") == 1) return "3♣"; // SNG ♠
                }
            }
            else if (cevapciRole == "3♣" && aciciRebid == "2NT") // 5♣ 4♥ 15-17 Röle cevabı
            {
                if (c == 5 && h == 4 && d == 2 && s == 2) return "3♦"; // 5422
                // Kontrol sayısı metodunuz projede farklı isimdeyse burayı güncelleyin (Örn: ElDegerlendirici.KontrolSayisi)
                if (ElDegerlendirici.KontrolSayisi(el) >= 2) return "3♥"; // KK (Küçük Kontrol)
                return "3♠"; // BK (Büyük Kontrol)
            }

            return "Pas";
        }
    }
}