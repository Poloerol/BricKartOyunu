using System;
using System.Collections.Generic;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;

namespace BricKartOyunu.Tests
{
    /// <summary>
    /// Müdahale Sonrası Güçlü Cevap testleri: ortak 1 seviye açtı, sağ rakip müdahale etti, Güney 17+ HP ve 6+ kartlı renkle
    /// sıçramalı yeni renk (oyun zorlaması) verir. Geçmiş listesi: ortak açılışı, rakip müdahalesi (Güney sıradadır).
    /// </summary>
    public static class TestRunner_MudahaleGuclu
    {
        private static int _gecen, _kalan;
        private static Action<string> _yaz;

        public static int TumTestleriCalistir(Action<string> sonucEkle)
        {
            _gecen = 0; _kalan = 0; _yaz = sonucEkle;
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  MÜDAHALE SONRASI GÜÇLÜ CEVAP TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            T("MG1 1♦-(1♠) 22 HP 7 kupa -> 3♥", TestHelpers.ElOlustur("AQ", "AKQ9872", "6", "AK7"), "3♥", "1♦", "1♠");
            T("MG2 1♥-(1♠) 17 HP 6 sinek -> 3♣", TestHelpers.ElOlustur("32", "A2", "K32", "AKQJ98"), "3♣", "1♥", "1♠");
            T("MG3 1♣-(1♦) 17 HP 6 maça -> 2♠", TestHelpers.ElOlustur("AKQJ98", "A2", "32", "K32"), "2♠", "1♣", "1♦");
            T("MG4 16 HP -> uygun değil", TestHelpers.ElOlustur("AK", "AKJ982", "32", "J32"), null, "1♦", "1♠");
            T("MG5 5 kartlı renk -> uygun değil", TestHelpers.ElOlustur("AK", "AKQ98", "32", "KJ32"), null, "1♦", "1♠");
            T("MG6 sıçrama 3 seviyeyi aşar (1♠-(2♥)-4♦) -> uygun değil", TestHelpers.ElOlustur("A2", "A32", "AKQ987", "K2"), null, "1♠", "2♥");
            T("MG7 rakip müdahale etmedi -> uygun değil", TestHelpers.ElOlustur("AQ", "AKQ9872", "6", "AK7"), null, "1♦", "Pas");
            T("MG8 uzun renk rakibin rengi -> uygun değil", TestHelpers.ElOlustur("AKQJ98", "A2", "32", "K32"), null, "1♦", "1♠");

            sonucEkle("");
            sonucEkle("  ÖZET: ✅ " + _gecen + " geçti, ❌ " + _kalan + " başarısız");
            sonucEkle("════════════════════════════════════════");
            return _kalan;
        }

        private static void T(string ad, List<Card> el, string beklenen, params string[] gecmis)
        {
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            var k = new MudahaleSonrasiGucluCevap();
            string r = k.UygunMu(durum) ? k.TeklifVer(durum) : null;
            bool gecti = r == beklenen;
            if (gecti) _gecen++; else _kalan++;
            _yaz((gecti ? "✅ " : "❌ ") + ad + " → " + (r ?? "(uygun değil)") + (gecti ? "" : "   [beklenen: " + (beklenen ?? "(uygun değil)") + "]"));
        }
    }
}
