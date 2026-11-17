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
            sonucEkle("  ÖZET: ✅ " + _gecen + " geçti, ❌ " + _kalan + " başarısız");
            sonucEkle("════════════════════════════════════════");
            return _kalan;
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
