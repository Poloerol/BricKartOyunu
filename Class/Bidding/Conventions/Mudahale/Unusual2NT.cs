using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;   // KozYardimcisi için

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Unusual 2NT Konvansiyonu ("Diğer İki Renk" Yaklaşımı).
    /// 
    /// Kural:
    /// - Rakip 1 seviyesinde bir renk açtı (1♣/1♦/1♥/1♠)
    /// - Ben 2NT derim → "Rakip rengi hariç, kalan renklerden 
    ///                    EN KÜÇÜK ikisini 5+5 olarak gösteriyorum"
    /// 
    /// Rakip rengine göre anlamlar:
    ///   Rakip 1♣ → 2NT = 5♥ + 5♦
    ///   Rakip 1♦ → 2NT = 5♥ + 5♣
    ///   Rakip 1♥ → 2NT = 5♦ + 5♣
    ///   Rakip 1♠ → 2NT = 5♦ + 5♣
    /// 
    /// NOT: Bu konvansiyon, klasik Michaels ile ÇAKIŞMAZ.
    /// 
    /// Öncelik: 23
    /// </summary>
    public class Unusual2NT : IKonvansiyon
    {
        public string Ad => "Unusual 2NT";
        public string Aciklama => "İki minör renkli Unusual 2NT müdahalesi";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 23;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Ortağım Unusual 2NT yaptı: cevap ver
            if (CevapKozlari(durum) != null) return true;

            // Rakip 1 seviyesinde bir renk açtı mı?
            if (durum.RakipTeklifleri.Count == 0) return false;
            string rakipSon = durum.RakipTeklifleri.Last();
            if (rakipSon != "1♠" && rakipSon != "1♥" &&
                rakipSon != "1♦" && rakipSon != "1♣") return false;

            // Ben henüz konuşmadım
            if (durum.KendiGercekTeklifSayisi > 0) return false;
            // Partner gerçek teklif vermediyse (direkt/balancing oturuş)
            if (durum.PartnerTeklifleri.Any(t => t != "Pas" && t != "Dbl" && t != "Rdbl")) return false;
            // Rakip rengini çıkar
            string rakipKozu = KozYardimcisi.TekliftenKozCikar(rakipSon);
            if (string.IsNullOrEmpty(rakipKozu)) return false;

            // Rakip rengi hariç, kalan 3 renkten EN KÜÇÜK İKİSİNİ bul
            // Sıralama: Sinek (en küçük) → Karo → Kupa → Maça (en büyük)
            var el = durum.AktifOyuncuEli;
            var renkSiralama = new List<string> { "Sinek", "Karo", "Kupa", "Maça" };
            var adayRenkler = renkSiralama
                .Where(r => r != rakipKozu)
                .Take(2)
                .ToList();

            // İki aday renkte de 5+ kart olmalı
            foreach (var renk in adayRenkler)
            {
                if (ElDegerlendirici.RenkUzunlugu(el, renk) < 5) return false;
            }

            return adayRenkler.Count == 2;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var k = CevapKozlari(durum);
            if (k != null) return CevapTeklifi(durum, k);
            return "2NT";
        }

        // ═══════════════════════════════════════════════════════════════
        // CEVAP (ortak Unusual 2NT yaptı)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Ortağımın 2NT'si Unusual ise gösterdiği iki rengi döndürür (alçaktan yükseğe).
        /// Değilse null. Koşul: rakip 1 seviyesinde renk açtı, son gerçek teklif ortağın 2NT'si,
        /// ben henüz gerçek teklif vermedim.
        /// </summary>
        private static List<string> CevapKozlari(IhaleDurumu durum)
        {
            if (durum.KendiTeklifleri.Any(t => t != "Pas")) return null;
            if (durum.SonGercekTeklif != "2NT") return null;
            if (durum.SonGercekTeklifSahibi != durum.Partner) return null;

            var acilis = durum.Gecmis
                .Where(h => h.GercekTeklifMi && durum.Rakipler.Contains(h.Oyuncu))
                .Select(h => h.Teklif)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(acilis) || acilis.Length != 2 || acilis[0] != '1') return null;
            string rakipKozu = KozYardimcisi.TekliftenKozCikar(acilis);
            if (string.IsNullOrEmpty(rakipKozu) || rakipKozu == "NT") return null;

            return new List<string> { "Sinek", "Karo", "Kupa", "Maça" }
                .Where(r => r != rakipKozu).Take(2).ToList();
        }

        private static string CevapTeklifi(IhaleDurumu durum, List<string> kozlar)
        {
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            string alcak = kozlar[0], yuksek = kozlar[1];
            int la = ElDegerlendirici.RenkUzunlugu(el, alcak);
            int ly = ElDegerlendirici.RenkUzunlugu(el, yuksek);

            // Daha uzun (eşitse daha iyi destek = yüksek renk) rengi seç; ortak en az 5-5
            string secilen = ly >= la ? yuksek : alcak;
            int uzunluk = secilen == yuksek ? ly : la;
            string sembol = KozYardimcisi.KozSembolu(secilen);
            bool majorMu = secilen == "Kupa" || secilen == "Maça";

            // Güçlü el + majör uyumu → oyun
            if (majorMu && hp >= 13 && uzunluk >= 3) return "4" + sembol;
            // Güçlü el + minör: 5 seviye yerine 3NT için yer bırak (ortak zayıf olabilir)
            return "3" + sembol;
        }
    }
}