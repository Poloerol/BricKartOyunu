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
    public static class TestRunner_OneDiamond_Gelisimler
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1♦ GELİŞİM TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            sonucEkle("── 1♦ ⇒ 1♥ — AÇANIN YENİDEN TEKLİFİ ──");
            Test("2♥ — 3♥ 15 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("Q2", "K32", "KJ32", "AQ32"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ — 4♦ 4♠ 11 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("KJ32", "2", "AJ32", "Q432"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Guney, "1♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT — 4♦ dengeli 13 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("Q32", "Q2", "KJ32", "AJ32"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Guney, "1NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — 5♦ 4♣ 11 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("Q2", "32", "AJ932", "KJ32"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Guney, "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 6♦ 12 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("Q2", "32", "KQ9432", "AJ2"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 6♦ 19 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("AK", "KQ", "AQJ432", "432"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("4♥ — 6♦ 4♥ 13 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("K2", "KJ32", "AQ9432", "2"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("4♣ — 6♦ 4♥ SNG♣ 14 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("K2", "KQ32", "AQ9432", "2"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 5♦ 4♥ 2♠ 2♣ 19 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("AK", "AQ32", "KQ932", "J2"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 5♦ 4♥ 16 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("A2", "AQ32", "KJ932", "Q2"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("Perspektif: aynı el, Batı açtı → 2♥", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("Q2", "K32", "KJ32", "AQ32"),
                new[] { "1♦", "Pas", "1♥", "Pas" }, Player.Bati, "2♥",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♦ ⇒ 1♥ — CEVAPÇININ DEVAMI ──");
            Test("1♠ sonrası 2♠ — 4♥ 4♠ 6 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("KJ32", "Q432", "32", "432"),
                new[] { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ sonrası 3♣ — 5♥ 4♦ 14 HCP FG", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("32", "KQ932", "AQ32", "K2"),
                new[] { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT sonrası 3NT — 12 HCP dengeli", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("KJ2", "QJ32", "K32", "Q32"),
                new[] { "1♦", "Pas", "1♥", "Pas", "1NT", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 3NT — 14 HCP ♠ stoper", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("AJ2", "KQ32", "Q32", "Q32"),
                new[] { "1♦", "Pas", "1♥", "Pas", "2♣", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 2♥ — 5♥ 6 HCP S.O", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("32", "KQ932", "432", "J32"),
                new[] { "1♦", "Pas", "1♥", "Pas", "2♦", "Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3NT — 4♥ dengeli 13 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("KJ2", "KQ32", "Q32", "Q32"),
                new[] { "1♦", "Pas", "1♥", "Pas", "2♥", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 2NT — Lebensohl (3 HCP)", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("J432", "432", "32", "Q432"),
                new[] { "1♦", "Pas", "1♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♦ ⇒ 1♥ — RÖLE CEVAPLARI ──");
            Test("2♣-2♠ → 3♣ — 5♦ 5♣ 12 HCP", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("2", "32", "AKJ32", "KJ932"),
                new[] { "1♦", "Pas", "1♥", "Pas", "2♣", "Pas", "2♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2♠ → 3♣ — 6♦ 3♥ min", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("Q2", "K32", "AQJ932", "32"),
                new[] { "1♦", "Pas", "1♥", "Pas", "2♦", "Pas", "2♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ → 3♣ — 3♥ min", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("K2", "Q32", "KQJ93", "432"),
                new[] { "1♦", "Pas", "1♥", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ → 2NT — 4♥ SNG", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("K32", "KQ32", "AJ932", "2"),
                new[] { "1♦", "Pas", "1♥", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠-2NT-3♣ → 3♦ — KK min", new OneDiamond_OneHeart(),
                TestHelpers.ElOlustur("K32", "KQ32", "AJ932", "2"),
                new[] { "1♦", "Pas", "1♥", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♦ ⇒ 1♠ ──");
            Test("2♠ — 3♠ 14 HCP", new OneDiamond_OneSpade(),
                TestHelpers.ElOlustur("KJ2", "32", "AQJ32", "K32"),
                new[] { "1♦", "Pas", "1♠", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 4♠ 3♦ davet (2♠ sonrası)", new OneDiamond_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "32", "Q32", "K432"),
                new[] { "1♦", "Pas", "1♠", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♣ — 4♠ SNG", new OneDiamond_OneSpade(),
                TestHelpers.ElOlustur("KQ32", "3", "AJ932", "K32"),
                new[] { "1♦", "Pas", "1♠", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♥ → 3♣ — 5♦ 5♣", new OneDiamond_OneSpade(),
                TestHelpers.ElOlustur("2", "32", "AKJ32", "KJ932"),
                new[] { "1♦", "Pas", "1♠", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2♥ → 2♠ — 6♦ 3♠ min", new OneDiamond_OneSpade(),
                TestHelpers.ElOlustur("K32", "32", "AQJ932", "32"),
                new[] { "1♦", "Pas", "1♠", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2♥-2♠ sonrası 2NT — 15 HCP", new OneDiamond_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "KQ2", "32", "AQ32"),
                new[] { "1♦", "Pas", "1♠", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2♥-2♠ sonrası 3♠ — 9 HCP davet", new OneDiamond_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "Q32", "32", "K432"),
                new[] { "1♦", "Pas", "1♠", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♦ ⇒ 1NT ──");
            Test("2♣ — 16+ HCP röle", new OneDiamond_OneNT(),
                TestHelpers.ElOlustur("AK32", "KQ2", "AJ32", "K3"),
                new[] { "1♦", "Pas", "1NT", "Pas" }, Player.Guney, "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 6♦ 11 HCP", new OneDiamond_OneNT(),
                TestHelpers.ElOlustur("K2", "Q2", "KQJ932", "432"),
                new[] { "1♦", "Pas", "1NT", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 5♦ 5♣ 15 HCP", new OneDiamond_OneNT(),
                TestHelpers.ElOlustur("A2", "3", "KQJ32", "KQ932"),
                new[] { "1♦", "Pas", "1NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 3♣ — 4♣ 4♦ max", new OneDiamond_OneNT(),
                TestHelpers.ElOlustur("K32", "32", "KJ32", "Q932"),
                new[] { "1♦", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2NT — Lebensohl", new OneDiamond_OneNT(),
                TestHelpers.ElOlustur("J432", "432", "432", "Q32"),
                new[] { "1♦", "Pas", "1NT", "Pas", "2♥", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — 10 HCP röle", new OneDiamond_OneNT(),
                TestHelpers.ElOlustur("KJ32", "Q32", "K32", "J32"),
                new[] { "1♦", "Pas", "1NT", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♦ ⇒ 2♣ ──");
            Test("2NT — 6♦ 19 HCP", new OneDiamond_TwoClub(),
                TestHelpers.ElOlustur("AK", "KQ", "AQJ432", "432"),
                new[] { "1♦", "Pas", "2♣", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5♦ 4♥", new OneDiamond_TwoClub(),
                TestHelpers.ElOlustur("32", "KQ32", "AQJ32", "32"),
                new[] { "1♦", "Pas", "2♣", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — min", new OneDiamond_TwoClub(),
                TestHelpers.ElOlustur("K32", "32", "AQJ32", "Q32"),
                new[] { "1♦", "Pas", "2♣", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 3♥ — 5♣ 4♠ SNG♥", new OneDiamond_TwoClub(),
                TestHelpers.ElOlustur("KJ32", "3", "32", "AQJ932"),
                new[] { "1♦", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 2NT — 4♣ dengeli", new OneDiamond_TwoClub(),
                TestHelpers.ElOlustur("KJ2", "Q32", "K32", "Q932"),
                new[] { "1♦", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2♥ → 2♠ — 5♦ 4♣", new OneDiamond_TwoClub(),
                TestHelpers.ElOlustur("K2", "32", "AQJ32", "K932"),
                new[] { "1♦", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2♥ → 3NT — 4441", new OneDiamond_TwoClub(),
                TestHelpers.ElOlustur("KJ32", "Q432", "J432", "2"),
                new[] { "1♦", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — röle", new OneDiamond_TwoClub(),
                TestHelpers.ElOlustur("K32", "Q32", "K32", "Q932"),
                new[] { "1♦", "Pas", "2♣", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣-3♦ → 3♥ — 6♦ SNG♣ (KK)", new OneDiamond_TwoClub(),
                TestHelpers.ElOlustur("KJ2", "Q32", "AKQ932", "2"),
                new[] { "1♦", "Pas", "2♣", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♦ ⇒ 2♦ / 2♥ / 2♠ ──");
            Test("2♦: 2NT FG röle — 20 HCP", new OneDiamond_TwoDiamond(),
                TestHelpers.ElOlustur("AKJ", "K32", "AQ32", "K32"),
                new[] { "1♦", "Pas", "2♦", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2NT: 3♦ — max 2♠ 2♥", new OneDiamond_TwoDiamond(),
                TestHelpers.ElOlustur("A2", "K2", "KJ932", "Q932"),
                new[] { "1♦", "Pas", "2♦", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2NT: 3♣ — min", new OneDiamond_TwoDiamond(),
                TestHelpers.ElOlustur("32", "K2", "QJ932", "J932"),
                new[] { "1♦", "Pas", "2♦", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: 2NT — 20 HCP davet+", new OneDiamond_TwoHeart(),
                TestHelpers.ElOlustur("AKJ", "K32", "AQ32", "K32"),
                new[] { "1♦", "Pas", "2♥", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: Pas — tercih", new OneDiamond_TwoHeart(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "AJ2"),
                new[] { "1♦", "Pas", "2♥", "Pas" }, Player.Guney, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: 2♠ — ♠ tercih", new OneDiamond_TwoHeart(),
                TestHelpers.ElOlustur("KJ32", "32", "KQ32", "Q32"),
                new[] { "1♦", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2NT: 3♦ — 5♠ 5♥", new OneDiamond_TwoHeart(),
                TestHelpers.ElOlustur("KQ932", "KJ932", "2", "32"),
                new[] { "1♦", "Pas", "2♥", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 2NT — 20 HCP", new OneDiamond_TwoSpade(),
                TestHelpers.ElOlustur("AK2", "K32", "AQ32", "K32"),
                new[] { "1♦", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT: 3♥ — 5/4 minör ♥ tek", new OneDiamond_TwoSpade(),
                TestHelpers.ElOlustur("32", "3", "KQ932", "AKJ32"),
                new[] { "1♦", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♦ ⇒ 2NT ──");
            Test("Pas — min", new OneDiamond_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "KQ32", "J32"),
                new[] { "1♦", "Pas", "2NT", "Pas" }, Player.Guney, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 15 HCP", new OneDiamond_TwoNT(),
                TestHelpers.ElOlustur("KQ2", "K32", "AJ32", "Q32"),
                new[] { "1♦", "Pas", "2NT", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 6♦ SNG♣ 19 HCP", new OneDiamond_TwoNT(),
                TestHelpers.ElOlustur("AKJ", "Q32", "AKQ932", "2"),
                new[] { "1♦", "Pas", "2NT", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ puppet sonrası 3♦", new OneDiamond_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "K32", "Q932"),
                new[] { "1♦", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣-3♦ → Pas — 6♦ S.O", new OneDiamond_TwoNT(),
                TestHelpers.ElOlustur("Q32", "K32", "KQJ932", "2"),
                new[] { "1♦", "Pas", "2NT", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Guney, "Pas",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("");
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        private static void Test(
            string ad,
            IAcilisGelisimi motor,
            List<Card> el,
            string[] gecmis,
            Player aktif,
            string beklenen,
            Action<string> sonucEkle,
            ref int gecen,
            ref int kalan)
        {
            sonucEkle($"▶ {ad}");

            var durum = TestHelpers.DurumOlustur(el, aktif, gecmis);
            if (!motor.UygunMu(el, durum))
            {
                sonucEkle("❌ HATA: UygunMu false döndü.");
                kalan++;
                return;
            }

            string sonuc = motor.TeklifVer(el, durum);
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
