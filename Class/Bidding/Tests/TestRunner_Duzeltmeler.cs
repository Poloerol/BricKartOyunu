using System;
using System.Collections.Generic;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Tests
{
    /// <summary>
    /// Toplu denetimden çıkan düzeltmelerin tam motor testleri (rakipsiz diziler, Güney sıradaki oyuncu):
    /// 1M–1NT sonrası 17+ HP açıcı, Gerber sonrası devam, üçüncü renk forsingi sonrası 3 kartlık destek.
    /// Geçmiş listesi Güney'in hemen sağındaki rakibin Pas'ı ile biter.
    /// </summary>
    public static class TestRunner_Duzeltmeler
    {
        private static int _gecen, _kalan;
        private static Action<string> _yaz;

        public static int TumTestleriCalistir(Action<string> sonucEkle)
        {
            _gecen = 0; _kalan = 0; _yaz = sonucEkle;
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  DÜZELTME TESTLERİ (1M–1NT, Gerber, TSF)");
            sonucEkle("════════════════════════════════════════");

            // 1♥ – 1NT (pas geçmiş cevaplayan: forcing değil) – açıcı 17+ HP
            T("N1 1H-1NT 17hcp dengeli -> 2NT davet", TestHelpers.ElOlustur("K72", "AQJ95", "K43", "A4"), "2NT", "Pas", "Pas", "1♥", "Pas", "1NT", "Pas");
            T("N2 1H-1NT 6 kupa 17hcp -> 3H davet", TestHelpers.ElOlustur("K2", "AKJ952", "Q43", "A4"), "3♥", "Pas", "Pas", "1♥", "Pas", "1NT", "Pas");
            T("N3 1H-1NT asgari 13hcp -> Pas", TestHelpers.ElOlustur("Q72", "KJ985", "K43", "A4"), "Pas", "Pas", "Pas", "1♥", "Pas", "1NT", "Pas");

            // 1NT – 4♣ (Gerber) – cevap – soranın devamı
            T("G1 1NT-4C-4S: 4 as + 19hcp -> 6NT", TestHelpers.ElOlustur("AK2", "AQ3", "KJ4", "Q762"), "6NT", "1NT", "Pas", "4♣", "Pas", "4♠", "Pas");
            T("G2 1NT-4C-4D: as eksik -> 4NT (bitir)", TestHelpers.ElOlustur("A32", "KQ3", "KJ4", "Q762"), "4NT", "1NT", "Pas", "4♣", "Pas", "4♦", "Pas");

            sonucEkle("");
            sonucEkle("  ÖZET: ✅ " + _gecen + " geçti, ❌ " + _kalan + " başarısız");
            sonucEkle("════════════════════════════════════════");
            return _kalan;
        }

        private static void T(string ad, List<Card> el, string beklenen, params string[] gecmis)
        {
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            var motor = new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan());
            string r = motor.TeklifVer(durum);
            bool gecti = r == beklenen;
            if (gecti) _gecen++; else _kalan++;
            _yaz((gecti ? "✅ " : "❌ ") + ad + " → " + r + (gecti ? "" : "   [beklenen: " + beklenen + "]"));
        }
    }
}
