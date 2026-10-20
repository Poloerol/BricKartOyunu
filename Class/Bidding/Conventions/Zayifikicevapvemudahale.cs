using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Zayıf İki açılışı (2♦/2♥/2♠) ile ilgili ortak yardımcılar.
    /// </summary>
    internal static class ZayifIkiYardimci
    {
        public static string RenkAdi(char sembol)
        {
            switch (sembol)
            {
                case '♠': return "Maça";
                case '♥': return "Kupa";
                case '♦': return "Karo";
                case '♣': return "Sinek";
                default: return null;
            }
        }

        // ♣ < ♦ < ♥ < ♠
        public static int RenkSirasi(char sembol)
        {
            switch (sembol)
            {
                case '♣': return 0;
                case '♦': return 1;
                case '♥': return 2;
                case '♠': return 3;
                default: return -1;
            }
        }

        public static bool ZayifIkiTeklifiMi(string teklif)
        {
            return teklif == "2♦" || teklif == "2♥" || teklif == "2♠";
        }

        /// <summary>
        /// Auction'daki TEK gerçek teklif bir Zayıf İki açılışı ise onu döner.
        /// partnerMi / rakipMi: açılışı kimin yaptığı.
        /// pasSayisi: açılıştan sonra gelen (hepsi Pas olmak zorunda) teklif sayısı;
        ///            açılıştan sonra Pas dışı bir şey varsa -1.
        /// </summary>
        public static string ZayifIkiAcilisi(IhaleDurumu durum,
            out bool partnerMi, out bool rakipMi, out int pasSayisi)
        {
            partnerMi = false;
            rakipMi = false;
            pasSayisi = -1;

            var liste = durum.Gecmis.ToList();
            var gercek = liste.Where(h => h.GercekTeklifMi).ToList();
            if (gercek.Count != 1) return null;

            var acilis = gercek[0];
            if (!ZayifIkiTeklifiMi(acilis.Teklif)) return null;

            int idx = liste.IndexOf(acilis);
            var sonrasi = liste.Skip(idx + 1).ToList();
            if (sonrasi.Any(h => h.Teklif != "Pas")) return null;

            partnerMi = acilis.Oyuncu == durum.Partner;
            rakipMi = durum.Rakipler.Contains(acilis.Oyuncu);
            pasSayisi = sonrasi.Count;
            return acilis.Teklif;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ZAYIF İKİ CEVABI (partner 2♦/2♥/2♠ açtı, rakip Pas dedi)
    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Partnerin Zayıf İki açılışına cevap.
    ///   Uyum (3+) ve 14+ HP  : majörde 4M, minörde 3NT (15+) / 3♦
    ///   Majörde 5+ uyum      : 4M (preemptif / feda)
    ///   Uyum (3+) ve 6+ HP   : 3 seviyesine kaldır
    ///   Diğer her durum      : Pas  (yeni renk 3 seviyesinde ZORLAYICI olduğu için verilmez)
    /// 
    /// Kapsam dışı (ileride): 2NT (Ogust) ve açıcının 2NT cevabı, yeni renk zorlaması.
    /// </summary>
    public class ZayifIkiCevap : IKonvansiyon
    {
        public string Ad => "Zayıf İki Cevap";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 9;

        /// <summary>Multi 2♦ açıksa 2♦ burada ele alınmaz.</summary>
        public bool Multi2DAcik { get; set; } = false;

        public bool UygunMu(IhaleDurumu durum)
        {
            return Hesapla(durum) != null;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return Hesapla(durum) ?? "Pas";
        }

        // null → konvansiyon uygulanmaz
        private string Hesapla(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return null;
            if (durum.AktifOyuncuEli.Count != 13) return null;
            if (durum.IhaleBittiMi()) return null;

            bool partnerMi, rakipMi;
            int pasSayisi;
            string acilis = ZayifIkiYardimci.ZayifIkiAcilisi(durum, out partnerMi, out rakipMi, out pasSayisi);
            if (acilis == null || !partnerMi) return null;
            if (acilis == "2♦" && Multi2DAcik) return null;

            // Açıcının hemen ardından rakip Pas demiş olmalı (tam 1 Pas)
            if (pasSayisi != 1) return null;

            char sembol = acilis[1];
            string renk = ZayifIkiYardimci.RenkAdi(sembol);
            bool majorMu = sembol == '♠' || sembol == '♥';

            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            int uyum = ElDegerlendirici.RenkUzunlugu(el, renk);

            if (uyum >= 3 && hp >= 14)
            {
                if (majorMu) return "4" + sembol;
                return hp >= 15 ? "3NT" : "3" + sembol;
            }

            if (majorMu && uyum >= 5)
                return "4" + sembol;

            if (uyum >= 3 && hp >= 6)
                return "3" + sembol;

            return "Pas";
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ZAYIF İKİ'YE MÜDAHALE (rakip 2♦/2♥/2♠ açtı)
    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Rakibin Zayıf İki açılışına doğal müdahale.
    ///   5+ kartlık renk: 2 seviyesinde 11+ HP, 3 seviyesinde 15+ HP
    ///   (balancing/pass-out seat'te eşik 3 HP düşer)
    ///   Dengeli 15-18 HP + rakip renginde tutuş: 2NT
    ///   Kontra'ya uygun dağılım (rakip rengi ≤2, diğer üç renk ≥3) → TakeoutDouble'a bırakılır.
    /// 
    /// Not: "tutuş" için ElDegerlendirici'de uygun bir metot bilmediğimden
    /// rakip rengindeki 3+ kart yaklaşık değer olarak kullanıldı; elinizdeki
    /// durdurucu kontrolü varsa ona bağlayın.
    /// </summary>
    public class ZayifIkiMudahale : IKonvansiyon
    {
        public string Ad => "Zayıf İki Müdahale";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 21;

        public bool Multi2DAcik { get; set; } = false;

        public bool UygunMu(IhaleDurumu durum)
        {
            return Hesapla(durum) != null;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return Hesapla(durum) ?? "Pas";
        }

        private string Hesapla(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return null;
            if (durum.AktifOyuncuEli.Count != 13) return null;
            if (durum.IhaleBittiMi()) return null;

            bool partnerMi, rakipMi;
            int pasSayisi;
            string acilis = ZayifIkiYardimci.ZayifIkiAcilisi(durum, out partnerMi, out rakipMi, out pasSayisi);
            if (acilis == null || !rakipMi) return null;
            if (acilis == "2♦" && Multi2DAcik) return null;

            // Sıra bizde: direkt (0 Pas) veya pass-out seat (2 Pas)
            if (pasSayisi != 0 && pasSayisi != 2) return null;
            bool balancing = pasSayisi == 2;

            char rakipSembol = acilis[1];
            int rakipSira = ZayifIkiYardimci.RenkSirasi(rakipSembol);

            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);

            var uzunluk = new Dictionary<char, int>
            {
                { '♠', ElDegerlendirici.RenkUzunlugu(el, "Maça") },
                { '♥', ElDegerlendirici.RenkUzunlugu(el, "Kupa") },
                { '♦', ElDegerlendirici.RenkUzunlugu(el, "Karo") },
                { '♣', ElDegerlendirici.RenkUzunlugu(el, "Sinek") }
            };

            int rakipUzunluk = uzunluk[rakipSembol];

            // Kontra dağılımı → TakeoutDouble konvansiyonu karar versin
            bool kontraDagilimi = rakipUzunluk <= 2 &&
                uzunluk.Where(k => k.Key != rakipSembol).All(k => k.Value >= 3);
            if (kontraDagilimi) return null;

            int indirim = balancing ? 3 : 0;

            // ─── Doğal renk müdahalesi: en uzun (eşitlikte yüksek) 5+ renk ───
            var aday = uzunluk
                .Where(k => k.Key != rakipSembol && k.Value >= 5)
                .OrderByDescending(k => k.Value)
                .ThenByDescending(k => ZayifIkiYardimci.RenkSirasi(k.Key))
                .Select(k => (char?)k.Key)
                .FirstOrDefault();

            if (aday.HasValue)
            {
                int seviye = ZayifIkiYardimci.RenkSirasi(aday.Value) > rakipSira ? 2 : 3;
                int esik = (seviye == 2 ? 11 : 15) - indirim;
                if (hp >= esik)
                    return seviye.ToString() + aday.Value;
            }

            // ─── 2NT: dengeli 15-18 + rakip renginde tutuş (yaklaşık) ───
            bool singletonYok = uzunluk.Values.All(v => v >= 2);
            if (hp >= 15 - indirim && hp <= 18 && singletonYok && rakipUzunluk >= 3)
                return "2NT";

            // Durum bize ait ama teklif yok → bilinçli Pas (fallback'e düşmesin)
            return "Pas";
        }
    }
}