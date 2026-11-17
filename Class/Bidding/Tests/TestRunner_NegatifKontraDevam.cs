using System;
using System.Collections.Generic;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;

namespace BricKartOyunu.Tests
{
    /// <summary>
    /// Negatif Kontra Devam testleri: ben açtım, sol rakip müdahale etti, ortak negatif kontra yaptı, sağ rakip Pas.
    /// Güney daima sıradaki oyuncudur (geçmiş: Güney açış, Batı müdahale, Kuzey Dbl, Doğu Pas).
    /// Motor Karnesi: rakip renginde 3 ya da daha az kartla kontrayı pas geçmek ortalama 8,3 IMP kaybettiriyordu.
    /// </summary>
    public static class TestRunner_NegatifKontraDevam
    {
        private static int _gecen, _kalan;
        private static Action<string> _yaz;

        public static int TumTestleriCalistir(Action<string> sonucEkle)
        {
            _gecen = 0; _kalan = 0; _yaz = sonucEkle;
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  NEGATİF KONTRA DEVAM TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            T("N1 1C-(1H)-X-(P) 4 maça -> 1S", TestHelpers.ElOlustur("AQ84", "32", "K72", "KJ95"), "1♠", "1♣", "1♥", "Dbl", "Pas");
            T("N2 1D-(1S)-X-(P) 6 karo -> 2D", TestHelpers.ElOlustur("32", "K4", "AQJ974", "862"), "2♦", "1♦", "1♠", "Dbl", "Pas");
            T("N3 1C-(1D)-X-(P) 15hcp dengeli ♦ durdurucu -> 2NT", TestHelpers.ElOlustur("K84", "Q93", "KJ5", "AQ92"), "2NT", "1♣", "1♦", "Dbl", "Pas");
            T("N4 1C-(1H)-X-(P) kupada 4 kart -> uygun değil (ceza pası serbest)", TestHelpers.ElOlustur("A84", "KQJ3", "K72", "J53"), null, "1♣", "1♥", "Dbl", "Pas");
            T("N5 1C-(1H)-P-(P) kontra yok -> uygun değil", TestHelpers.ElOlustur("AQ84", "32", "K72", "KJ95"), null, "1♣", "1♥", "Pas", "Pas");
            // Vugraph simultane bord 14: 3♦ - Dbl (20 HP) - Pas - 3♠ (0 HP) - Pas: kontra yapan oyuna çıkar
            Y("TY1 3D-X-P-3S-P 20hcp 4 maça -> 4S", TestHelpers.ElOlustur("KT92", "A76", "A", "AKQ72"), "4♠", "3♦", "Dbl", "Pas", "3♠", "Pas");
            Y("TY2 1D-X-P-1S-P 13hcp -> uygun değil", TestHelpers.ElOlustur("KT92", "Q65", "7", "AKJ72"), null, "1♦", "Dbl", "Pas", "1♠", "Pas");
            Y("TY3 1D-X-P-1S-P 18hcp 4 maça -> 3S (davet)", TestHelpers.ElOlustur("KT92", "Q65", "A", "AKQ72"), "3♠", "1♦", "Dbl", "Pas", "1♠", "Pas");
            // Vugraph simultane bord 12: maçada void, 7'li sinek, 12 HP, rakip 1♠: kontra değil 2♣ müdahalesi
            bool td = new TakeoutDouble().UygunMu(TestHelpers.DurumOlustur(TestHelpers.ElOlustur("", "1097", "A107", "AKJ10973"), Player.Guney, "Pas", "Pas", "1♠"));
            if (td) _kalan++; else _gecen++;
            _yaz((td ? "❌ " : "✅ ") + "TD1 ♠ void + 7'li sinek 12hcp (1♠ sonrası) → Takeout Double uygun değil");
            sonucEkle("  ÖZET: ✅ " + _gecen + " geçti, ❌ " + _kalan + " başarısız");
            sonucEkle("════════════════════════════════════════");
            return _kalan;
        }

        private static void Y(string ad, List<Card> el, string beklenen, params string[] gecmis)
        {
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            var k = new TakeoutKontraYukseltme();
            string r = k.UygunMu(durum) ? k.TeklifVer(durum) : null;
            bool gecti = r == beklenen;
            if (gecti) _gecen++; else _kalan++;
            _yaz((gecti ? "✅ " : "❌ ") + ad + " → " + (r ?? "(uygun değil)") + (gecti ? "" : "   [beklenen: " + (beklenen ?? "(uygun değil)") + "]"));
        }

        private static void T(string ad, List<Card> el, string beklenen, params string[] gecmis)
        {
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            var k = new NegatifKontraDevam();
            string r = k.UygunMu(durum) ? k.TeklifVer(durum) : null;
            bool gecti = r == beklenen;
            if (gecti) _gecen++; else _kalan++;
            _yaz((gecti ? "✅ " : "❌ ") + ad + " → " + (r ?? "(uygun değil)") + (gecti ? "" : "   [beklenen: " + (beklenen ?? "(uygun değil)") + "]"));
        }
    }
}
