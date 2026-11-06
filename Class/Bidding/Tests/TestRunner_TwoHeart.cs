using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.TwoHeart;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_TwoHeart
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  2♥ AÇILIŞ SİSTEMİ TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            sonucEkle("── 2♥ AÇILIŞ açışına cevaplar ──");
            TestA("4♥ — 3♥ 14+",
                TestHelpers.ElOlustur("K32", "KQ32", "AK3", "K32"), "4♥",
                sonucEkle, ref gecen, ref kalan);

            TestA("3♠ — 5♠ 3♥ 13",
                TestHelpers.ElOlustur("AKJ32", "Q32", "32", "K32"), "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestA("3♥ — önleyici raise 4♥",
                TestHelpers.ElOlustur("432", "J932", "32", "5432"), "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestA("Pas — 3♥ zayıf",
                TestHelpers.ElOlustur("432", "J93", "K32", "5432"), "Pas",
                sonucEkle, ref gecen, ref kalan);

            TestA("2♠ — röle 11 HCP",
                TestHelpers.ElOlustur("AK32", "Q2", "K32", "J432"), "2♠",
                sonucEkle, ref gecen, ref kalan);

            TestA("2NT — 5♠ F1R",
                TestHelpers.ElOlustur("AKJ32", "2", "K32", "J432"), "2NT",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♥ ⇒ 2♠ ──");
            TestG("açan 3♣ — SNG ♦", new TwoHeart_TwoSpade(),
                TestHelpers.ElOlustur("A32", "KQJ932", "K", "432"),
                new[] { "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3♦ — SNG ♠", new TwoHeart_TwoSpade(),
                TestHelpers.ElOlustur("K", "KQJ932", "K32", "432"),
                new[] { "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3♥ — SNG yok min", new TwoHeart_TwoSpade(),
                TestHelpers.ElOlustur("432", "QJ9532", "K3", "32"),
                new[] { "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 2NT — 4x", new TwoHeart_TwoSpade(),
                TestHelpers.ElOlustur("K32", "KQJ932", "Q432", ""),
                new[] { "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT sonrası 3♣ röle", new TwoHeart_TwoSpade(),
                TestHelpers.ElOlustur("AK32", "Q2", "K32", "J432"),
                new[] { "2♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            TestG("röle cevabı 3♦ — 4♣", new TwoHeart_TwoSpade(),
                TestHelpers.ElOlustur("K2", "KQJ932", "3", "Q432"),
                new[] { "2♥", "Pas", "2♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♥ ⇒ 2NT (s.70) ──");
            TestG("2NT açan 3NT — kapalı 6♥", new TwoHeart_TwoNT(),
                TestHelpers.ElOlustur("32", "AKQ943", "432", "32"),
                new[] { "2♥", "Pas", "2NT", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT açan 3♠ — 4♠ azami", new TwoHeart_TwoNT(),
                TestHelpers.ElOlustur("AK43", "Q98432", "Q2", "2"),
                new[] { "2♥", "Pas", "2NT", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT açan 4♦ — 6♥ 4♦ 1♠", new TwoHeart_TwoNT(),
                TestHelpers.ElOlustur("2", "KJ9832", "Q432", "54"),
                new[] { "2♥", "Pas", "2NT", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT açan 3♥ — 2♣ 2♦ 3♠", new TwoHeart_TwoNT(),
                TestHelpers.ElOlustur("Q32", "KJ9832", "43", "43"),
                new[] { "2♥", "Pas", "2NT", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT açan 3♣ — tercih", new TwoHeart_TwoNT(),
                TestHelpers.ElOlustur("Q2", "KJ9832", "32", "432"),
                new[] { "2♥", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT açan 3♦ — tercih", new TwoHeart_TwoNT(),
                TestHelpers.ElOlustur("Q2", "KJ9832", "432", "32"),
                new[] { "2♥", "Pas", "2NT", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT cevapçı: 3♣ sonrası 6♠ → 3♠", new TwoHeart_TwoNT(),
                TestHelpers.ElOlustur("AKQJ93", "2", "432", "432"),
                new[] { "2♥", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT cevapçı: 3♥ sonrası 5♠ → 3♠", new TwoHeart_TwoNT(),
                TestHelpers.ElOlustur("AKJ93", "2", "K32", "5432"),
                new[] { "2♥", "Pas", "2NT", "Pas", "3♥", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT cevapçı: 4♦ sonrası 5♠ → 4♠", new TwoHeart_TwoNT(),
                TestHelpers.ElOlustur("AKJ93", "2", "K32", "5432"),
                new[] { "2♥", "Pas", "2NT", "Pas", "4♦", "Pas" }, Player.Kuzey, "4♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT cevapçı: 4♦ sonrası minör → 4NT", new TwoHeart_TwoNT(),
                TestHelpers.ElOlustur("AKJ", "2", "K9432", "J432"),
                new[] { "2♥", "Pas", "2NT", "Pas", "4♦", "Pas" }, Player.Kuzey, "4NT",
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
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, "2♥", "Pas");
            var motor = new TwoHeart_Answer();
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
