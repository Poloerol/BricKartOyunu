using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneNT;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_OneNT
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1NT CEVAP SİSTEMİ TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            Test("2♣ — Stayman 9 HCP 4-4 majör",
                TestHelpers.ElOlustur("KJ32", "Q432", "K32", "32"), "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — zayıf 4-4 majör S.O",
                TestHelpers.ElOlustur("J432", "Q432", "432", "32"), "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — 5♠ davet 8 HCP",
                TestHelpers.ElOlustur("KQJ32", "32", "Q32", "432"), "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — dengeli davet 9 HCP",
                TestHelpers.ElOlustur("K32", "32", "KJ32", "Q432"), "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 5♥ zayıf (♥ transferi)",
                TestHelpers.ElOlustur("32", "KJ932", "Q32", "432"), "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5♠ zayıf (♠ transferi)",
                TestHelpers.ElOlustur("KJ932", "32", "Q32", "432"), "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5/5 majör",
                TestHelpers.ElOlustur("KJ932", "QJ932", "2", "32"), "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("4♦ — Texas ♥ 10 HCP",
                TestHelpers.ElOlustur("32", "AKJ932", "Q32", "32"), "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("4♥ — Texas ♠ 10 HCP",
                TestHelpers.ElOlustur("AKJ932", "32", "Q32", "32"), "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 6♦ FG SNG♠",
                TestHelpers.ElOlustur("2", "K32", "AKJ932", "Q32"), "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 5/5 minör SNG♥",
                TestHelpers.ElOlustur("K3", "2", "AQ932", "KJ932"), "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 5/5 minör SNG♠",
                TestHelpers.ElOlustur("2", "K3", "AQ932", "KJ932"), "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 6♣ FG SNG♠",
                TestHelpers.ElOlustur("2", "K32", "Q32", "AKJ932"), "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 5♦ S.O",
                TestHelpers.ElOlustur("32", "32", "Q9432", "J932"), "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♣ S.O (aralık sorusu)",
                TestHelpers.ElOlustur("32", "32", "Q932", "J9432"), "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 5'li majör sorusu (3♥ 3♠) 10 HCP",
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "J32"), "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — dengeli 11 HCP majör yok",
                TestHelpers.ElOlustur("K2", "Q2", "KJ32", "Q9432"), "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("4NT — dengeli 16 HCP",
                TestHelpers.ElOlustur("AK", "K2", "KQ32", "J9432"), "4NT",
                sonucEkle, ref gecen, ref kalan);

            Test("6NT — dengeli 21 HCP",
                TestHelpers.ElOlustur("AK", "KQ", "AQ32", "QJ432"), "6NT",
                sonucEkle, ref gecen, ref kalan);

            Test("Pas — 3 HCP",
                TestHelpers.ElOlustur("32", "432", "Q432", "J432"), "Pas",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("");
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        private static void Test(string ad, List<Card> el, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");

            var durum = TestHelpers.DurumOlustur(el, Player.Guney, "1NT", "Pas");
            var motor = new OneNT_Answer();

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
