using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_OneDiamond
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1♦ CEVAP SİSTEMİ TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            Test("1♥ — 4♥ 6 HCP",
                TestHelpers.ElOlustur("K32", "QJ32", "432", "432"), "1♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ — 4♠ 7 HCP",
                TestHelpers.ElOlustur("KJ32", "32", "Q432", "J32"), "1♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT — 8 HCP dengeli, 4'lü majör yok",
                TestHelpers.ElOlustur("Q32", "K32", "J432", "Q32"), "1NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — 13 HCP dengeli 4♥ (12+)",
                TestHelpers.ElOlustur("KQ2", "KJ32", "Q32", "Q32"), "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 13 HCP 4♦ 3-3-3 0-3 kontrol",
                TestHelpers.ElOlustur("KQ2", "KJ2", "Q432", "Q32"), "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5♠ 4♥ 7 HCP",
                TestHelpers.ElOlustur("QJ432", "KJ32", "32", "32"), "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 5♣ 4♦ 8 HCP",
                TestHelpers.ElOlustur("A2", "32", "QJ32", "J9432"), "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5+♦ davet 9 HCP",
                TestHelpers.ElOlustur("K2", "32", "KJ932", "Q432"), "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 11 HCP dengeli",
                TestHelpers.ElOlustur("K32", "QJ2", "K432", "Q32"), "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 6♣ önleyici 5 HCP",
                TestHelpers.ElOlustur("32", "432", "32", "KQ9432"), "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("Pas — 1 HCP",
                TestHelpers.ElOlustur("432", "432", "432", "J432"), "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — 14 HCP 5♦ (4+♦ FG)",
                TestHelpers.ElOlustur("A2", "K32", "KQ432", "Q32"), "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♦ 4♣ 14 HCP (5/4 minör FG)",
                TestHelpers.ElOlustur("A2", "32", "KQ432", "KQ32"), "2♠",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("");
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        private static void Test(string ad, List<Card> el, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");

            var durum = TestHelpers.DurumOlustur(el, Player.Guney, "1♦", "Pas");
            var motor = new OneDiamond_Answer();

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
