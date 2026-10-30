using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
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
        public string Aciklama => "Zayıf iki açışına müdahale";
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
