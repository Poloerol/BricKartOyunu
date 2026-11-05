using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneHeart;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_OneHeart
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1♥ CEVAP SİSTEMİ TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            Test("1♠ — 4♠ 9 HCP",
                TestHelpers.ElOlustur("KJ32", "Q2", "Q432", "J32"), "1♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT — 8 HCP, 3♥ yok",
                TestHelpers.ElOlustur("Q32", "32", "KJ32", "Q432"), "1NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 3+♥ 7 HCP",
                TestHelpers.ElOlustur("432", "KJ32", "Q32", "J32"), "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 4♥ dengeli davet 10 HCP",
                TestHelpers.ElOlustur("K32", "KJ32", "Q32", "J32"), "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 4♥ SNG 8 HCP",
                TestHelpers.ElOlustur("KJ32", "Q432", "Q432", "2"), "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 4♥ SNG 12 HCP",
                TestHelpers.ElOlustur("AJ32", "KQ32", "Q432", "2"), "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 4♥ void♣ 9 HCP",
                TestHelpers.ElOlustur("KJ432", "QJ32", "Q432", ""), "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("4♥ — 7♥ önleyici",
                TestHelpers.ElOlustur("32", "KQJ9432", "32", "32"), "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("4♠ — 7♠ önleyici",
                TestHelpers.ElOlustur("KQJ9432", "32", "32", "32"), "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 6♥ önleyici 4 HCP",
                TestHelpers.ElOlustur("32", "KJ9432", "432", "32"), "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5/5 minör 9 HCP",
                TestHelpers.ElOlustur("2", "K2", "KJ932", "Q9432"), "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 5♦ FG 14 HCP",
                TestHelpers.ElOlustur("A32", "32", "AQJ32", "K32"), "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ — 4♠ FG 13 HCP",
                TestHelpers.ElOlustur("AKJ3", "32", "Q32", "K432"), "1♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — dengeli FG röle 13 HCP",
                TestHelpers.ElOlustur("AK3", "K32", "Q432", "J32"), "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 3♥ davet 11 HCP",
                TestHelpers.ElOlustur("K32", "QJ2", "K432", "Q32"), "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 6♦ davet 10 HCP",
                TestHelpers.ElOlustur("A2", "32", "KQJ932", "432"), "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("Pas — 3 HCP",
                TestHelpers.ElOlustur("432", "32", "J432", "Q432"), "Pas",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("");
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        private static void Test(string ad, List<Card> el, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");

            var durum = TestHelpers.DurumOlustur(el, Player.Guney, "1♥", "Pas");
            var motor = new OneHeart_Answer();

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
