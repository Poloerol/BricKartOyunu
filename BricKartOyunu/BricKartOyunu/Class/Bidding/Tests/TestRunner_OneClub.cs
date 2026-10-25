using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_OneClub
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1♣ AÇIŞ SİSTEMİ TESTLERİ");
            sonucEkle("════════════════════════════════════════");

            int gecen = 0, kalan = 0;

            // Testler
            Test("44.7 — 12 HCP dengeli 4♣", TestHelpers.ElOlustur("QJ2", "QJ3", "QJ3", "QJ32"), "3NT", sonucEkle, ref gecen, ref kalan);
            Test("44.8 — 12 HCP 5♣ dengesiz", TestHelpers.ElOlustur("QJ2", "QJ3", "J2", "AQJ32"), "2♣", sonucEkle, ref gecen, ref kalan);
            Test("44.9 — 12 HCP 4♣ 3-3-3 0-3 kontrol", TestHelpers.ElOlustur("QJ2", "QJ3", "QJ3", "QJ32"), "3NT", sonucEkle, ref gecen, ref kalan);
            // 44.10: 11 HCP dengeli majörsüz → 2NT
            // Eski (12 kart): TestHelpers.ElOlustur("QJ2", "QJ3", "QJ3", "QJ2")
            // Yeni (13 kart): 
            Test("44.10 — 11 HCP dengeli majörsüz", TestHelpers.ElOlustur("QJ2", "QJ3", "QJ3", "QJ32"), "3NT", sonucEkle, ref gecen, ref kalan);
            Test("44.11 — 10 HCP 6♣ iyi renk", TestHelpers.ElOlustur("QJ2", "J3", "J3", "AKQJ32"), "2♠", sonucEkle, ref gecen, ref kalan);
            // 44.12: 12 HCP 5♣4♦ → 2♠
            // Eski (14 kart): TestHelpers.ElOlustur("QJ2", "J3", "KQJ2", "AQJ32")
            // Yeni (13 kart):
            Test("44.12 — 12 HCP 5♣4♦", TestHelpers.ElOlustur("Q2", "J3", "QJ32", "KQJ32"), "2♠", sonucEkle, ref gecen, ref kalan); Test("44.13 — 5 HCP 5♠4♥", TestHelpers.ElOlustur("QJ432", "QJ32", "32", "32"), "2♥", sonucEkle, ref gecen, ref kalan);
            Test("44.14 — 8 HCP 5♣4♦", TestHelpers.ElOlustur("Q2", "J3", "QJ32", "QJ432"), "2♦", sonucEkle, ref gecen, ref kalan);
            // 44.17: 5 HCP 6♣ (NV) → 3♣
            // Eski (12 kart): TestHelpers.ElOlustur("Q2", "J3", "32", "QJ5432")
            // Yeni (13 kart):
            Test("44.17 — 5 HCP 6♣ (NV)", TestHelpers.ElOlustur("Q32", "J3", "32", "QJ5432"), "3♣", sonucEkle, ref gecen, ref kalan);

            sonucEkle("");
            sonucEkle("════════════════════════════════════════");
            sonucEkle($"  ÖZET: ✅ {gecen} geçti, ❌ {kalan} başarısız");
            sonucEkle("════════════════════════════════════════");
        }

        private static void Test(string ad, List<Card> el, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");

            var durum = TestHelpers.DurumOlustur(el, Player.Guney, "1♣", "Pas");
            var motor = new OneClub_Answer();

            if (!motor.UygunMu(durum))
            {
                sonucEkle("❌ HATA: UygunMu false döndü.");
                kalan++;
                return;
            }

            string sonuc = motor.TeklifVer(durum);
            if (sonuc == beklenen)
            {
                sonucEkle($"✅ DOĞRU: {sonuc}");
                gecen++;
            }
            else
            {
                sonucEkle($"❌ HATA: Beklenen {beklenen}, gelen {sonuc}");
                kalan++;
            }
        }
    }
}