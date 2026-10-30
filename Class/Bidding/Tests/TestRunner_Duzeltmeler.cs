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

            // 1♣ – 1♠ – 1NT – 2♦ (oyun zorlayıcı checkback) – 2♠ (3 kart destek): cevapçı oyuna kadar devam eder
            T("K1 checkback 2D-2S, 4 maca -> 3NT", TestHelpers.ElOlustur("AJ52", "T4", "AQ43", "Q82"), "3NT", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2♦", "Pas", "2♠", "Pas");
            T("K2 checkback 2D-2S, 5 maca -> 4S", TestHelpers.ElOlustur("AJ852", "T4", "AQ4", "Q82"), "4♠", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2♦", "Pas", "2♠", "Pas");

            // 1♣ – 1♠ – 1NT – 2NT (davet): açıcı 3 kartlık destekle düzeltir
            T("C1 1C-1S-1NT-2NT, 3 maca 13hcp -> 3S", TestHelpers.ElOlustur("A52", "J973", "A82", "AT3"), "3♠", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2NT", "Pas");
            T("C2 1C-1S-1NT-2NT, 3 maca 14hcp -> 4S", TestHelpers.ElOlustur("A52", "KJ73", "A82", "AT3"), "4♠", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2NT", "Pas");
            T("C3 1C-1S-1NT-2NT, 2 maca asgari -> Pas", TestHelpers.ElOlustur("K5", "K842", "K954", "A42"), "Pas", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2NT", "Pas");

            // Rekabet: ortak 1♣, rakip 1 seviye müdahale; dengeli güçlü el pas geçmemeli
            T("R1 1C-(1H) 18hcp dengeli, tutuk -> 3NT", TestHelpers.ElOlustur("832", "K93", "AKJ7", "AK2"), "3NT", "1♣", "1♥");
            T("R2 1C-(1D) 14hcp dengeli, tutuk -> 2NT", TestHelpers.ElOlustur("Q82", "K93", "KJ7", "AJ52"), "2NT", "1♣", "1♦");

            // Unusual 2NT sonrası 3. seviye: 18 HP + rakip rengi AKQ8 -> ceza kontrası
            T("U1 1C-(2NT)-P-(3H) 18hcp AKQ8 -> Dbl", TestHelpers.ElOlustur("JT97", "AKQ8", "A", "KJ82"), "Dbl", "1♣", "2NT", "Pas", "3♥");
            // 1♣-(1♦)-1♠-(Pas)-2♣: cevapçı 16 HP, 4 kupa -> 2♥ (pas değil)
            T("U2 1C-(1D)-1S-P-2C 16hcp 4H -> 2H", TestHelpers.ElOlustur("AQJ72", "AK54", "764", "Q"), "2♥", "1♣", "1♦", "1♠", "Pas", "2♣", "Pas");

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
