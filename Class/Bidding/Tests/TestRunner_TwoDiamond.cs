using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.TwoDiamond;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_TwoDiamond
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  2♦ AÇILIŞ SİSTEMİ TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            sonucEkle("── 2♦ AÇILIŞ açışına cevaplar ──");
            TestA("Pas — 4♦ zayıf",
                TestHelpers.ElOlustur("J32", "J32", "J432", "432"), "Pas",
                sonucEkle, ref gecen, ref kalan);

            TestA("2♥ — 5♠ zayıf",
                TestHelpers.ElOlustur("Q9532", "432", "32", "432"), "2♥",
                sonucEkle, ref gecen, ref kalan);

            TestA("2NT — 5♥ zayıf (transfer)",
                TestHelpers.ElOlustur("432", "J9532", "32", "432"), "2NT",
                sonucEkle, ref gecen, ref kalan);

            TestA("2♠ — dengeli 5 HCP",
                TestHelpers.ElOlustur("K32", "Q32", "432", "5432"), "2♠",
                sonucEkle, ref gecen, ref kalan);

            TestA("3♦ — 5♠ 4♥ FG",
                TestHelpers.ElOlustur("KQ932", "K432", "32", "32"), "3♦",
                sonucEkle, ref gecen, ref kalan);

            TestA("3NT — 5/5 majör",
                TestHelpers.ElOlustur("KQ932", "KJ932", "3", "32"), "3NT",
                sonucEkle, ref gecen, ref kalan);

            TestA("3♣ — 5♥ 4♠ Stayman",
                TestHelpers.ElOlustur("KJ93", "KQ932", "32", "32"), "3♣",
                sonucEkle, ref gecen, ref kalan);

            TestA("2♥ — 4♠ FG",
                TestHelpers.ElOlustur("KJ93", "K32", "Q32", "432"), "2♥",
                sonucEkle, ref gecen, ref kalan);

            TestA("2♠ — 4♥ FG",
                TestHelpers.ElOlustur("K32", "KJ93", "Q32", "432"), "2♠",
                sonucEkle, ref gecen, ref kalan);

            TestA("3♥ — 5♦ SNG ♥",
                TestHelpers.ElOlustur("K32", "5", "KQ932", "K432"), "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestA("2NT — 6♣ SI",
                TestHelpers.ElOlustur("AK2", "K3", "32", "AQJ432"), "2NT",
                sonucEkle, ref gecen, ref kalan);

            TestA("3♣ — 5♦ 4♠ SI",
                TestHelpers.ElOlustur("AKJ3", "32", "AQ932", "K2"), "3♣",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♦ ⇒ 2♥ ──");
            TestG("açan 2NT — 4♠", new TwoDiamond_TwoHeart(),
                TestHelpers.ElOlustur("AKJ3", "KQ2", "Q32", "K32"),
                new[] { "2♦", "Pas", "2♥", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 2♠ — 3♠", new TwoDiamond_TwoHeart(),
                TestHelpers.ElOlustur("AK2", "KQ32", "Q32", "K32"),
                new[] { "2♦", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("2♠ sonrası Pas — 5♠ zayıf", new TwoDiamond_TwoHeart(),
                TestHelpers.ElOlustur("Q9532", "432", "32", "432"),
                new[] { "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            TestG("2♠ sonrası 3♥ — 4♠4♥", new TwoDiamond_TwoHeart(),
                TestHelpers.ElOlustur("KJ93", "Q932", "32", "432"),
                new[] { "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("2♠ sonrası 3♣ — 5♠ 4♣", new TwoDiamond_TwoHeart(),
                TestHelpers.ElOlustur("KQ932", "32", "32", "KJ43"),
                new[] { "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT sonrası 3♥ transfer — 5♠ zayıf", new TwoDiamond_TwoHeart(),
                TestHelpers.ElOlustur("Q9532", "432", "32", "432"),
                new[] { "2♦", "Pas", "2♥", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♥ transfer kabulü 3♠", new TwoDiamond_TwoHeart(),
                TestHelpers.ElOlustur("AKJ3", "KQ2", "Q32", "K32"),
                new[] { "2♦", "Pas", "2♥", "Pas", "2NT", "Pas", "3♥", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("transfer sonrası Pas — zayıf", new TwoDiamond_TwoHeart(),
                TestHelpers.ElOlustur("Q9532", "432", "32", "432"),
                new[] { "2♦", "Pas", "2♥", "Pas", "2NT", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            TestG("transfer sonrası 4♠ — 8 HCP", new TwoDiamond_TwoHeart(),
                TestHelpers.ElOlustur("KQ932", "Q32", "32", "432"),
                new[] { "2♦", "Pas", "2♥", "Pas", "2NT", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Kuzey, "4♠",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♦ ⇒ 2♠ ──");
            TestG("açan 2NT", new TwoDiamond_TwoSpade(),
                TestHelpers.ElOlustur("AK2", "KQ32", "Q32", "K32"),
                new[] { "2♦", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT sonrası Pas — zayıf dengeli", new TwoDiamond_TwoSpade(),
                TestHelpers.ElOlustur("K32", "Q32", "432", "5432"),
                new[] { "2♦", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT sonrası 3♣ — 5♥ 4♣", new TwoDiamond_TwoSpade(),
                TestHelpers.ElOlustur("32", "KQ932", "32", "KJ43"),
                new[] { "2♦", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT sonrası 3♥ — 4♥ dengeli SI", new TwoDiamond_TwoSpade(),
                TestHelpers.ElOlustur("A32", "KQ32", "K32", "K32"),
                new[] { "2♦", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♥ transfer kabulü 3NT", new TwoDiamond_TwoSpade(),
                TestHelpers.ElOlustur("AK2", "KQ32", "Q32", "K32"),
                new[] { "2♦", "Pas", "2♠", "Pas", "2NT", "Pas", "3♥", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♦ ⇒ 2NT ──");
            TestG("açan 3♣", new TwoDiamond_TwoNT(),
                TestHelpers.ElOlustur("AK2", "KQ32", "Q32", "K32"),
                new[] { "2♦", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♣ sonrası Pas — 5♣ zayıf", new TwoDiamond_TwoNT(),
                TestHelpers.ElOlustur("432", "32", "432", "J9532"),
                new[] { "2♦", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♣ sonrası 3♦ — 5♥ zayıf", new TwoDiamond_TwoNT(),
                TestHelpers.ElOlustur("432", "J9532", "32", "432"),
                new[] { "2♦", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♣ sonrası 3♦ — 6♥ SI", new TwoDiamond_TwoNT(),
                TestHelpers.ElOlustur("A2", "KQJ932", "K32", "32"),
                new[] { "2♦", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♦ transfer kabulü 3♥", new TwoDiamond_TwoNT(),
                TestHelpers.ElOlustur("AK2", "KQ32", "Q32", "K32"),
                new[] { "2♦", "Pas", "2NT", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♦ ⇒ 3♣ ──");
            TestG("açan 3♥ — 4♥", new TwoDiamond_ThreeClub(),
                TestHelpers.ElOlustur("AK2", "KQ32", "Q32", "K32"),
                new[] { "2♦", "Pas", "3♣", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3♠ — 4♠", new TwoDiamond_ThreeClub(),
                TestHelpers.ElOlustur("KQ32", "AK2", "Q32", "K32"),
                new[] { "2♦", "Pas", "3♣", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3♦ — majör yok", new TwoDiamond_ThreeClub(),
                TestHelpers.ElOlustur("AK2", "KQ2", "KQ32", "K32"),
                new[] { "2♦", "Pas", "3♣", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♦ sonrası 3♥ — 4♥4♠ SO", new TwoDiamond_ThreeClub(),
                TestHelpers.ElOlustur("KJ93", "Q932", "32", "432"),
                new[] { "2♦", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♦ sonrası 3♠ — 5♥ 4♠", new TwoDiamond_ThreeClub(),
                TestHelpers.ElOlustur("KJ93", "KQ932", "32", "32"),
                new[] { "2♦", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♥ sonrası Pas — zayıf", new TwoDiamond_ThreeClub(),
                TestHelpers.ElOlustur("432", "Q432", "J32", "432"),
                new[] { "2♦", "Pas", "3♣", "Pas", "3♥", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♥ sonrası 4♥", new TwoDiamond_ThreeClub(),
                TestHelpers.ElOlustur("432", "KJ32", "KQ3", "K43"),
                new[] { "2♦", "Pas", "3♣", "Pas", "3♥", "Pas" }, Player.Kuzey, "4♥",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♦ ⇒ 3♦ ──");
            TestG("açan 3♠ MAX", new TwoDiamond_ThreeDiamond(),
                TestHelpers.ElOlustur("AKJ", "KQ32", "AK32", "K2"),
                new[] { "2♦", "Pas", "3♦", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 4♠ MIN", new TwoDiamond_ThreeDiamond(),
                TestHelpers.ElOlustur("K32", "KQ32", "Q32", "K32"),
                new[] { "2♦", "Pas", "3♦", "Pas" }, Player.Guney, "4♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3NT", new TwoDiamond_ThreeDiamond(),
                TestHelpers.ElOlustur("K2", "Q32", "AK32", "KJ32"),
                new[] { "2♦", "Pas", "3♦", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♦ ⇒ 3♥/3♠ ──");
            TestG("açan 4♣ — iyi el ♣", new TwoDiamond_ThreeHeart(),
                TestHelpers.ElOlustur("AK2", "KQ2", "AQ3", "KQ32"),
                new[] { "2♦", "Pas", "3♥", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3NT", new TwoDiamond_ThreeHeart(),
                TestHelpers.ElOlustur("K32", "Q32", "Q32", "K432"),
                new[] { "2♦", "Pas", "3♥", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 4♥ — 4-3 oynama", new TwoDiamond_ThreeSpade(),
                TestHelpers.ElOlustur("K32", "Q432", "Q32", "K32"),
                new[] { "2♦", "Pas", "3♠", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♦ ⇒ 3NT ──");
            TestG("açan 4♥ MIN", new TwoDiamond_ThreeNT(),
                TestHelpers.ElOlustur("K32", "Q32", "Q32", "K432"),
                new[] { "2♦", "Pas", "3NT", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 4♣ MAX", new TwoDiamond_ThreeNT(),
                TestHelpers.ElOlustur("AK2", "KQ2", "AQ3", "KQ32"),
                new[] { "2♦", "Pas", "3NT", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            TestG("4♣ sonrası 4♦ transfer", new TwoDiamond_ThreeNT(),
                TestHelpers.ElOlustur("KQ932", "KJ932", "3", "32"),
                new[] { "2♦", "Pas", "3NT", "Pas", "4♣", "Pas" }, Player.Kuzey, "4♦",
                sonucEkle, ref gecen, ref kalan);

            TestG("transfer kabulü 4♥", new TwoDiamond_ThreeNT(),
                TestHelpers.ElOlustur("AK2", "KQ2", "AQ3", "KQ32"),
                new[] { "2♦", "Pas", "3NT", "Pas", "4♣", "Pas", "4♦", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("");
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        private static void Rapor(string sonuc, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            if (sonuc == beklenen) { sonucEkle($"✅ DOĞRU: {sonuc}"); gecen++; }
            else { sonucEkle($"❌ HATA: Beklenen {beklenen}, gelen {sonuc}"); kalan++; }
        }

        private static void TestA(string ad, List<Card> el, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, "2♦", "Pas");
            var motor = new TwoDiamond_Answer();
            if (!motor.UygunMu(durum)) { sonucEkle("❌ HATA: UygunMu false döndü."); kalan++; return; }
            Rapor(motor.TeklifVer(durum), beklenen, sonucEkle, ref gecen, ref kalan);
        }

        private static void TestG(string ad, IAcilisGelisimi motor, List<Card> el, string[] gecmis, Player aktif, string beklenen,
            Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, aktif, gecmis);
            if (!motor.UygunMu(el, durum)) { sonucEkle("❌ HATA: UygunMu false döndü."); kalan++; return; }
            Rapor(motor.TeklifVer(el, durum), beklenen, sonucEkle, ref gecen, ref kalan);
        }
    }
}
