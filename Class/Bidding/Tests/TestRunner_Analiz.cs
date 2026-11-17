using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;

namespace BricKartOyunu.Tests
{
    /// <summary>
    /// Oyun Analizi ve Oynama Karnesi doğruluk testleri. Kimlik: atak öncesi çift dummy (K-G) + savunmanın kaybı - K-G'nin kaybı
    /// = oyunun gerçek sonucu (K-G lövesi). Motor oynanışı PIMC kapalı üretilir (hızlı ve deterministik).
    /// </summary>
    public static class TestRunner_Analiz
    {
        private static int _gecen, _kalan;
        private static Action<string> _yaz;
        private static readonly string[] KozAd = { "Maça", "Kupa", "Karo", "Sinek" };

        public static int TumTestleriCalistir(Action<string> sonucEkle)
        {
            _gecen = 0; _kalan = 0; _yaz = sonucEkle;
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  OYUN ANALİZİ TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            bool eskiPimc = PimcOyuncu.Aktif;
            PimcOyuncu.Aktif = false;
            try
            {
                foreach (int bord in new[] { 6, 7, 11 })
                    Kimlik("AN" + bord + " bord " + bord + ": başlangıç DD + EW kayıp - NS kayıp = gerçek K-G löve", bord);
            }
            finally { PimcOyuncu.Aktif = eskiPimc; }
            sonucEkle("  ÖZET: ✅ " + _gecen + " geçti, ❌ " + _kalan + " başarısız");
            sonucEkle("════════════════════════════════════════");
            return _kalan;
        }

        private static void Kimlik(string ad, int bord)
        {
            try
            {
                var mNS = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS)); var mEW = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.EW));
                var b = MotorKarnesi.Ihale(bord, bord, mNS, mEW);
                MotorKarnesi.Degerlendir(b);
                var eller = OynamaKarnesi.ElleriYukle(bord);
                var o = OynamaKarnesi.Oyna(b, eller, null);
                if (o == null) { Sonuc(ad, false, "kontrat yok (pas)"); return; }
                string koz = b.Sonuc.Koz == 4 ? null : KozAd[b.Sonuc.Koz];
                var r = OyunAnalizi.Analiz(eller, o.Kartlar, koz);
                int nsGercek = (b.Sonuc.Deklaran & 1) == 0 ? o.GercekLove : 13 - o.GercekLove;
                int beklenen = r.BaslangicDeger + r.EWKayip - r.NSKayip;
                bool ok = r.Tamam && r.Adimlar.Count == 52 && beklenen == nsGercek;
                Sonuc(ad, ok, "başlangıç " + r.BaslangicDeger + " + " + r.EWKayip + " - " + r.NSKayip + " = " + beklenen + ", gerçek " + nsGercek);
            }
            catch (Exception ex) { Sonuc(ad, false, "istisna: " + ex.Message); }
        }

        private static void Sonuc(string ad, bool gecti, string aciklama)
        {
            if (gecti) _gecen++; else _kalan++;
            _yaz((gecti ? "✅ " : "❌ ") + ad + " → " + aciklama);
        }
    }
}
