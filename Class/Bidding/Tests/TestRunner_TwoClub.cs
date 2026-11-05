using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.TwoClub;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_TwoClub
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  2♣ CEVAP SİSTEMİ TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            Test("2♦ — 5♥ 9 HCP",
                TestHelpers.ElOlustur("32", "KQJ32", "Q32", "K32"), "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — röle zayıf 3 HCP",
                TestHelpers.ElOlustur("432", "Q432", "J32", "432"), "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5♠ 8 HCP",
                TestHelpers.ElOlustur("KJ932", "32", "Q32", "K32"), "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 6♣ 9 HCP",
                TestHelpers.ElOlustur("32", "K32", "32", "KQJ432"), "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 5♠ 4♥ 8 HCP",
                TestHelpers.ElOlustur("KQ932", "Q432", "32", "32"), "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 5♥ 4♠ 8 HCP",
                TestHelpers.ElOlustur("Q432", "KQ932", "32", "32"), "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 6♦ 9 HCP",
                TestHelpers.ElOlustur("32", "32", "KQJ432", "K32"), "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 4♥ FG 9 HCP",
                TestHelpers.ElOlustur("32", "KJ32", "KQ32", "Q32"), "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 4♠ FG 9 HCP",
                TestHelpers.ElOlustur("KJ32", "32", "KQ32", "Q32"), "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 5♣ 3-3-2 FG",
                TestHelpers.ElOlustur("K32", "32", "Q32", "KJ432"), "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 5♦ 4♣ FG",
                TestHelpers.ElOlustur("32", "K2", "KQJ32", "Q432"), "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 5♣ 5♦ 4 HCP",
                TestHelpers.ElOlustur("32", "2", "J9432", "Q9432"), "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("4♣ — 7♥ + A",
                TestHelpers.ElOlustur("32", "AQJ9432", "432", "2"), "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("4♦ — 7♠ + KJ",
                TestHelpers.ElOlustur("KJ98432", "32", "432", "2"), "4♦",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("");
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        private static void Test(string ad, List<Card> el, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");

            var durum = TestHelpers.DurumOlustur(el, Player.Guney, "2♣", "Pas");
            var motor = new TwoClub_Answer();

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
