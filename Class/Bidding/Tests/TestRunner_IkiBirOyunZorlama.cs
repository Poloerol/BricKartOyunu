using System;
using System.Collections.Generic;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;

namespace BricKartOyunu.Tests
{
    /// <summary>
    /// 2/1 Game Force testleri (rakipsiz diziler). Kaynak: Max Hardy "Two Over One Game Force".
    /// Güney daima sıradaki oyuncudur; geçmiş listesi Güney'in hemen sağındaki rakip Pas ile biter.
    /// Kullanım: TestRunner_IkiBirOyunZorlama.TumTestleriCalistir(mesaj => ...)  → başarısız test sayısı döner.
    /// </summary>
    public static class TestRunner_IkiBirOyunZorlama
    {
        private static int _gecen, _kalan;

        public static int TumTestleriCalistir(Action<string> sonucEkle)
        {
            _gecen = 0; _kalan = 0; _yaz = sonucEkle;
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  2/1 GAME FORCE TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            T("R1 1S-2C 4+C 13hcp", TestHelpers.ElOlustur("A3","K42","Q83","AKJ75"), "2♣", "1♠","Pas");   // ortak S açtı, sağ rakip pas -> gecmis: 1S (ortak), Pas(sağ?) 
            T("R1 sadece 1S, 5H ile 2H", TestHelpers.ElOlustur("K3","AQJ85","K32","J52"), "2♥", "1♠","Pas");
            T("R1 11hcp null", TestHelpers.ElOlustur("A3","K42","Q83","J9875"), null, "1♠","Pas");
            T("R1 4+S destek null (Jacoby)", TestHelpers.ElOlustur("KJ32","K4","A832","KQ5"), null, "1♠","Pas");
            T("R1 1H, 4S -> null (1S)", TestHelpers.ElOlustur("AJ98","32","KQ3","AKJ2"), null, "1♥","Pas");
            T("R1 1H 2D 5D", TestHelpers.ElOlustur("A3","32","AKJ85","K982"), "2♦", "1♥","Pas");
            T("A1 2H 3 kupa min -> 4H", TestHelpers.ElOlustur("AQ842","K93","85","J72"), "4♥", "1♠","Pas","2♥","Pas");
            T("A2 2H 3 kupa 16hcp -> 3H", TestHelpers.ElOlustur("AKJ42","Q93","85","AJ2"), "3♥", "1♠","Pas","2♥","Pas");
            T("A3 2C 4 club destek -> 3C", TestHelpers.ElOlustur("AQ842","K9","85","KJ72"), "3♣", "1♠","Pas","2♣","Pas");
            T("A4 2D 6 pik -> 2S", TestHelpers.ElOlustur("AQJ842","K93","85","J2"), "2♠", "1♠","Pas","2♦","Pas");
            T("A5 2C 4 kupa -> 2H", TestHelpers.ElOlustur("AQ842","KJ93","85","J2"), "2♥", "1♠","Pas","2♣","Pas");
            T("A6 2D 4 sinek 5-3-2-4... 3C", TestHelpers.ElOlustur("AQ842","93","85","KJ72"), "3♣", "1♠","Pas","2♦","Pas");
            T("A7 dengeli stoplu 2NT", TestHelpers.ElOlustur("AQ842","K93","85","QJ2"), "2NT", "1♠","Pas","2♦","Pas");
            T("A8 pasli cevaplayan null", TestHelpers.ElOlustur("AQ842","K93","85","J72"), null, "Pas","Pas","1♠","Pas","2♦","Pas");
            T("A9 rakip mudahale null", TestHelpers.ElOlustur("AQ842","K93","85","J72"), null, "1♠","2♥","2♦","Pas");
            T("C1 ortak 2S, 3 pik min -> 4S", TestHelpers.ElOlustur("K32","A42","Q83","AJ97"), "4♠", "1♠","Pas","2♣","Pas","2♠","Pas");
            T("C2 ortak 2NT, 3 pik -> 4S", TestHelpers.ElOlustur("K32","A42","Q83","AJ97"), "4♠", "1♠","Pas","2♣","Pas","2NT","Pas");
            T("C3 ortak 2D, 3 pik min -> 2S", TestHelpers.ElOlustur("K32","A42","Q83","AJ97"), "2♠", "1♠","Pas","2♣","Pas","2♦","Pas");
            T("C4 ortak 2D, 3 pik 16hcp -> 3S", TestHelpers.ElOlustur("KJ2","A42","AJ3","K987"), "3♠", "1♠","Pas","2♣","Pas","2♦","Pas");
            T("C5 ortak 2D, pik yok minimum -> 2NT", TestHelpers.ElOlustur("32","KQ","K83","AQJ975"), "3♣", "1♠","Pas","2♣","Pas","2♦","Pas");   // 6 sinek -> 3C
            T("C6 ortak 2H, 4 kupa -> 4H", TestHelpers.ElOlustur("32","KJ42","K83","AQ97"), "4♥", "1♠","Pas","2♣","Pas","2♥","Pas");
            T("C7 ortak 3C raise, 3 pik -> 4S", TestHelpers.ElOlustur("K32","A42","Q83","AJ97"), "4♠", "1♠","Pas","2♣","Pas","3♣","Pas");
            T("C8 ortak 3NT -> Pas", TestHelpers.ElOlustur("32","KQ","K83","AQJ975"), "Pas", "1♠","Pas","2♣","Pas","3NT","Pas");
            T("C9 dengeli piksiz 2NT", TestHelpers.ElOlustur("32","KQ2","KJ8","AJ975"), "2NT", "1♠","Pas","2♣","Pas","2♥","Pas");
            T("C10 acici 2S tekrar, 2 pik 14hcp -> 2NT", TestHelpers.ElOlustur("32","KQ2","KJ8","AJ975"), "2NT", "1♠","Pas","2♣","Pas","2♠","Pas");
            T("C11 ortak 4S, 20+ HP -> null", TestHelpers.ElOlustur("AQ","AKQ","KQ3","AKJ52"), null, "1♠","Pas","2♣","Pas","4♠","Pas");
            T("D1 cevaplayan 2NT, fit yok -> 3NT", TestHelpers.ElOlustur("AQ842","K93","Q85","J2"), "3NT", "1♠","Pas","2♦","Pas","2♥","Pas","2NT","Pas");
            T("D2 cevaplayan 3S destek -> 4S", TestHelpers.ElOlustur("AQ842","K93","85","J72"), "4♠", "1♠","Pas","2♦","Pas","2♥","Pas","3♠","Pas");
            T("D3 cevaplayan 3NT -> Pas", TestHelpers.ElOlustur("AQ842","K93","85","J72"), "Pas", "1♠","Pas","2♦","Pas","2♥","Pas","3NT","Pas");
            T("X1 1H-2S sicramali null", TestHelpers.ElOlustur("AQ842","32","KQ3","AJ2"), null, "1♥","Pas");
            T("X2 1NT forcing yolu null", TestHelpers.ElOlustur("32","KQ2","KJ8","AJ975"), null, "1♠","Pas","1NT","Pas");
            T("X3 ortak 2H, 2 kupa 5-4... ikinci renk", TestHelpers.ElOlustur("AQ842","93","KQ85","J2"), "2♦", "1♠","Pas","2♣","Pas");
            T("X4 1H-2C 6 kupa tekrar 2H", TestHelpers.ElOlustur("32","AKJ842","85","QJ2"), "2♥", "1♥","Pas","2♣","Pas");
            T("X5 6 sinek rebid ben cevaplayan 3C", TestHelpers.ElOlustur("32","KQ","K83","AQJ975"), "3♣", "1♠","Pas","2♣","Pas","2♥","Pas");

            sonucEkle("");
            sonucEkle("  ÖZET: ✅ " + _gecen + " geçti, ❌ " + _kalan + " başarısız");
            sonucEkle("════════════════════════════════════════");
            return _kalan;
        }

        private static Action<string> _yaz;

        private static void T(string ad, List<Card> el, string beklenen, params string[] gecmis)
        {
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            var k = new IkiBirOyunZorlama();
            string r = k.UygunMu(durum) ? k.TeklifVer(durum) : null;
            bool gecti = r == beklenen;
            if (gecti) _gecen++; else _kalan++;
            _yaz((gecti ? "✅ " : "❌ ") + ad + " → " + (r ?? "(uygun değil)") + (gecti ? "" : "   [beklenen: " + (beklenen ?? "(uygun değil)") + "]"));
        }
    }
}
