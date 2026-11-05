using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Tests
{
    /// <summary>1♣ açılışının diğer gelişimleri (1♠, 1NT, 2♣, 2♦, 2♥, 2♠) ve 1♥ aşama B/C testleri.</summary>
    public static class TestRunner_OneClub_Gelisimler
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1♣ GELİŞİMLERİ (1♥ B/C, 1♠, 1NT, 2♣, 2♦, 2♥, 2♠)");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            Test("1♥ B: 1♠ sonrası 5♥ 4♣ 13 HCP → 2♦ (4. renk FG)", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("32", "AKJ32", "K2", "Q932"),
                new[] { "1♣", "Pas", "1♥", "Pas", "1♠", "Pas" }, Player.Kuzey, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ B: 1♠ sonrası 4♠ 7 HCP → 2♠", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("KJ32", "Q432", "432", "J2"),
                new[] { "1♣", "Pas", "1♥", "Pas", "1♠", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ B: 1♠ sonrası 5♥ 5 HCP → 2♥ S.O.", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("32", "KJ972", "432", "J32"),
                new[] { "1♣", "Pas", "1♥", "Pas", "1♠", "Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ B: 1NT sonrası 12 HCP dengeli → 3NT", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("K32", "KQ32", "A32", "432"),
                new[] { "1♣", "Pas", "1♥", "Pas", "1NT", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ B: 2♦ sonrası 5♥ 5 HCP → 2♥ S.O.", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("32", "KJ972", "432", "J32"),
                new[] { "1♣", "Pas", "1♥", "Pas", "2♦", "Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ B: 2♥ sonrası 4♥ 12 HCP dengeli → 3NT (COG)", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("K32", "KQ32", "A32", "432"),
                new[] { "1♣", "Pas", "1♥", "Pas", "2♥", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ B: 2NT sonrası 5 HCP → 3♣ (min)", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("32", "K972", "J432", "J32"),
                new[] { "1♣", "Pas", "1♥", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ B: 3♥ sonrası 13 HCP → 3♠ (röle)", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("K32", "A972", "KJ2", "Q32"),
                new[] { "1♣", "Pas", "1♥", "Pas", "3♥", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ C: 2♥-2♠ röle, 4♥ min SNG'siz → 3♥", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("K32", "AJ72", "Q32", "J42"),
                new[] { "1♣", "Pas", "1♥", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ C: 2♥-2♠ röle, 4♥ max SNG'siz → 3♠", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("K32", "AJ72", "Q32", "KQ4"),
                new[] { "1♣", "Pas", "1♥", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ C: 1♠ sonrası 2NTt → 3♣", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("KJ32", "Q2", "J2", "AQ843"),
                new[] { "1♣", "Pas", "1♥", "Pas", "1♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ A: 3♠ ile 15 HCP → 2♠", new OneClub_OneSpade(),
                TestHelpers.ElOlustur("K32", "Q2", "J32", "AKQ43"),
                new[] { "1♣", "Pas", "1♠", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ A: ♠ singleton dengesiz → 1NT", new OneClub_OneSpade(),
                TestHelpers.ElOlustur("2", "AQ32", "KJ32", "K432"),
                new[] { "1♣", "Pas", "1♠", "Pas" }, Player.Guney, "1NT",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ A: 5♣ 4♠ 15 HCP → 3♠", new OneClub_OneSpade(),
                TestHelpers.ElOlustur("KQ32", "2", "K32", "AQJ43"),
                new[] { "1♣", "Pas", "1♠", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ B: 2♠ sonrası 4♠ 14 HCP dengeli → 3NT (COG)", new OneClub_OneSpade(),
                TestHelpers.ElOlustur("KQ32", "Q32", "KJ2", "K32"),
                new[] { "1♣", "Pas", "1♠", "Pas", "2♠", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ C: 2♠-2NT röle, 3♠ min → 3♦", new OneClub_OneSpade(),
                TestHelpers.ElOlustur("K32", "Q2", "J32", "AJ432"),
                new[] { "1♣", "Pas", "1♠", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT A: 6♣ 13 HCP → 2♣", new OneClub_OneNT(),
                TestHelpers.ElOlustur("K2", "Q32", "J2", "AQJ943"),
                new[] { "1♣", "Pas", "1NT", "Pas" }, Player.Guney, "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT A: 6♣ 20 HCP → 2NT", new OneClub_OneNT(),
                TestHelpers.ElOlustur("K2", "A2", "K32", "AKQJ94"),
                new[] { "1♣", "Pas", "1NT", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT B: 2♦ röle sonrası 7 HCP → 2♥ (min)", new OneClub_OneNT(),
                TestHelpers.ElOlustur("J32", "K32", "Q432", "J32"),
                new[] { "1♣", "Pas", "1NT", "Pas", "2♦", "Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ A: dengeli 14 HCP → 2♦ (min)", new OneClub_TwoClub(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ2", "AJ32"),
                new[] { "1♣", "Pas", "2♣", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ B: 2♦ sonrası dengeli 14 HCP → 2NT", new OneClub_TwoClub(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ2", "AJ32"),
                new[] { "1♣", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ A: 19 HCP → 2NT (FG röle)", new OneClub_TwoDiamond(),
                TestHelpers.ElOlustur("AK2", "Q32", "K32", "AQJ4"),
                new[] { "1♣", "Pas", "2♦", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ A: 3♠ 13 HCP → 2♠ (tercih)", new OneClub_TwoHeart(),
                TestHelpers.ElOlustur("K32", "Q2", "432", "AKJ43"),
                new[] { "1♣", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ A: 6♣ dengesiz min → 3♣", new OneClub_TwoSpade(),
                TestHelpers.ElOlustur("2", "K32", "Q32", "AKJ943"),
                new[] { "1♣", "Pas", "2♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ A: dengeli → 2NT röle", new OneClub_TwoSpade(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ2", "AJ32"),
                new[] { "1♣", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ C: 2♣-2♦ röle, 6♣ 3♥ → 2♥", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("K2", "Q32", "J2", "AQJ943"),
                new[] { "1♣", "Pas", "1♥", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ C: 2NT-3♦ röle, 6♣ 4♥ → 4♣", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("K2", "AQ32", "2", "AKQJ94"),
                new[] { "1♣", "Pas", "1♥", "Pas", "2NT", "Pas", "3♦", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ C: 2♣-2♦ röle, 6♣ 4♥ max → 3♥", new OneClub_OneSpade(),
                TestHelpers.ElOlustur("2", "KQ32", "32", "AKQ943"),
                new[] { "1♣", "Pas", "1♠", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ D: 2♥-2♠-3♦ (3♥ max) sonrası 9 HCP → 4♥", new OneClub_OneHeart(),
                TestHelpers.ElOlustur("K32", "Q972", "J32", "Q32"),
                new[] { "1♣", "Pas", "1♥", "Pas", "2♥", "Pas", "2♠", "Pas", "3♦", "Pas" }, Player.Kuzey, "4♥",
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
