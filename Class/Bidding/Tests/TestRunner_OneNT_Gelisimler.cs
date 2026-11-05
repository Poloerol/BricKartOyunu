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
    public static class TestRunner_OneNT_Gelisimler
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1NT GELİŞİM TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            sonucEkle("── 1NT ⇒ 2♣ (Stayman) ──");
            Test("açan 2♥ — 4♥", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("AQ32", "K432", "Q32", "K2"),
                new[] { "1NT", "Pas", "2♣", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 2♠ — 4♠ 3♥", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("KQ32", "K32", "Q432", "K2"),
                new[] { "1NT", "Pas", "2♣", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 2♦ — majör yok", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("KQ2", "K32", "Q432", "KJ2"),
                new[] { "1NT", "Pas", "2♣", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 2♥ — zayıf 4-4", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("J432", "Q432", "432", "32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 2♠ — 5♠ davet", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("KQJ32", "32", "Q32", "432"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası Pas — zayıf", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("Q432", "J32", "K32", "432"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 2NT — davet 9 HCP", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("K432", "J32", "K32", "Q32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 3NT — 11 HCP", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("K432", "Q32", "KJ2", "Q32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 4NT — 16 HCP", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("AK32", "K32", "KJ2", "Q32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "4NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 6NT — 20 HCP", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("AK32", "AK2", "KJ2", "Q32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "6NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♥ — fit davet 9 HCP", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("K32", "Q432", "K32", "J32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 4♥ — fit oyun 12 HCP", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("K32", "KQ32", "K32", "J32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası Pas — fit zayıf", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("Q32", "Q432", "J32", "J32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2♠ — 5♠ davet", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("KQJ32", "32", "Q32", "432"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♠ — 4♠ SNG sorusu", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("KQ32", "32", "K32", "Q432"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2NT — davet", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("K32", "32", "KQ32", "J432"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3NT — oyun", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("K32", "32", "KQ32", "QJ32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♠ — fit davet", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("K432", "32", "K32", "Q432"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 4♠ — fit oyun", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("KJ32", "32", "K32", "KQ32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♠", "Pas" }, Player.Kuzey, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-3♠ → 3NT (SNG♣)", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("KQ2", "AQ432", "KJ32", "3"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas", "3♠", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-3♠ → 4♣ (SNG♦)", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("KQ2", "AQ432", "3", "KJ32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas", "3♠", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-3♠ → 4♥ (SNG♠)", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("2", "AQ432", "KJ32", "KQ3"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas", "3♠", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-3♠ → 4♠ (4♠, SNG yok)", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("KQ32", "AQ32", "K32", "32"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas", "3♠", "Pas" }, Player.Guney, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-3♠ → 3NT (SNG yok, ♠ yok)", new OneNT_TwoClub(),
                TestHelpers.ElOlustur("KQ2", "AQ32", "KJ32", "K2"),
                new[] { "1NT", "Pas", "2♣", "Pas", "2♥", "Pas", "3♠", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1NT ⇒ 2♦ (♥ transferi) ──");
            Test("açan 3♥ — 4♥ MAX", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("AK2", "AKJ2", "Q32", "K32"),
                new[] { "1NT", "Pas", "2♦", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 2NT — 3♥ MAX", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("AK2", "AKJ", "Q432", "K32"),
                new[] { "1NT", "Pas", "2♦", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 2♥ — kabul", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("KQ2", "K32", "Q432", "KJ2"),
                new[] { "1NT", "Pas", "2♦", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası Pas — 5♥ zayıf", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("32", "KJ932", "Q32", "432"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♦ — 5♥ 4♠ FG", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("AJ32", "KQ932", "32", "32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2NT — 5♥ 4♣ FG", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("32", "AQ932", "32", "KJ32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♣ — 5♥ 4♦ FG", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("32", "AQ932", "KJ32", "32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2♠ — 5♥ davet", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("32", "KQ932", "K32", "J32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♥ — 6♥ davet", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("2", "KQJ932", "Q32", "432"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3NT — 5♥ dengeli", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "AQ932", "K32", "32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 4♥ — dengesiz oyun", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("2", "AKQ932", "J32", "K32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ → 4♥ (MAX 3♥)", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("AK2", "AKJ", "Q432", "K32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ → 3NT (MAX 2♥)", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("AKQ", "AK", "Q432", "KJ32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ → 3♥ (MIN 3♥)", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("KQ2", "K32", "Q432", "KJ2"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ → 2NT (MIN 2♥)", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("KQ2", "K2", "Q432", "KJ32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-3♥ → 4♥ (MAX)", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("AK2", "AKJ", "Q432", "K32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "3♥", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-3♥ → 4♥ (15 HCP 3♥)", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("KQ2", "K32", "AJ32", "Q32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "3♥", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-3♥ → Pas (MIN)", new OneNT_TwoDiamond(),
                TestHelpers.ElOlustur("KQ2", "32", "AQ32", "KJ32"),
                new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "3♥", "Pas" }, Player.Guney, "Pas",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1NT ⇒ 2♥ (♠ transferi) ──");
            Test("açan 3♠ — 4♠ MAX", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("AKJ2", "AK2", "Q32", "K32"),
                new[] { "1NT", "Pas", "2♥", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 2NT — 3♠ MAX", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("AK2", "AKJ2", "Q32", "K32"),
                new[] { "1NT", "Pas", "2♥", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 2♠ — kabul", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("KQ2", "K32", "Q432", "KJ2"),
                new[] { "1NT", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası Pas — 5♠ zayıf", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("KJ932", "32", "Q32", "432"),
                new[] { "1NT", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♥ — 5/5 FG", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("AJ932", "KQ932", "32", "3"),
                new[] { "1NT", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♦ — 5♠ 4♥ FG", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("AJ932", "KQ32", "32", "32"),
                new[] { "1NT", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 2NT — 5♠ 4♣ FG", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("AJ932", "32", "32", "KQ32"),
                new[] { "1NT", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♣ — 5♠ 4♦ FG", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("AJ932", "32", "KQ32", "32"),
                new[] { "1NT", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♠ — 6♠ davet", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("KQJ932", "32", "Q32", "32"),
                new[] { "1NT", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3NT — 5♠ dengeli", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("AJ932", "K32", "KQ2", "32"),
                new[] { "1NT", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 4♠ — 6♠ oyun", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("AKQ932", "32", "J32", "32"),
                new[] { "1NT", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-3♠ → 4♠ (MAX)", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("AK2", "AKJ", "Q432", "K32"),
                new[] { "1NT", "Pas", "2♥", "Pas", "2♠", "Pas", "3♠", "Pas" }, Player.Guney, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-3♠ → Pas (MIN)", new OneNT_TwoHeart(),
                TestHelpers.ElOlustur("K2", "KQ32", "Q432", "KJ2"),
                new[] { "1NT", "Pas", "2♥", "Pas", "2♠", "Pas", "3♠", "Pas" }, Player.Guney, "Pas",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1NT ⇒ 2♠ / 2NT ──");
            Test("2♠: 3♣ — MAX", new OneNT_TwoSpade(),
                TestHelpers.ElOlustur("AK2", "KQ32", "KJ32", "Q2"),
                new[] { "1NT", "Pas", "2♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 2NT — MIN", new OneNT_TwoSpade(),
                TestHelpers.ElOlustur("KQ2", "K32", "Q432", "KJ2"),
                new[] { "1NT", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT: 3♣ — 5♣ S.O", new OneNT_TwoSpade(),
                TestHelpers.ElOlustur("32", "32", "Q932", "J9432"),
                new[] { "1NT", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-3♣: Pas", new OneNT_TwoSpade(),
                TestHelpers.ElOlustur("32", "32", "Q932", "J9432"),
                new[] { "1NT", "Pas", "2♠", "Pas", "3♣", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♥ — 5♥", new OneNT_TwoNT(),
                TestHelpers.ElOlustur("K32", "AQJ32", "K32", "Q2"),
                new[] { "1NT", "Pas", "2NT", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♠ — 5♠", new OneNT_TwoNT(),
                TestHelpers.ElOlustur("AQJ32", "K32", "K32", "Q2"),
                new[] { "1NT", "Pas", "2NT", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♣ — 5'li majör yok", new OneNT_TwoNT(),
                TestHelpers.ElOlustur("KQ2", "K32", "KJ32", "Q32"),
                new[] { "1NT", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 3NT", new OneNT_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "J32", "KQ32"),
                new[] { "1NT", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♥: 4♥ — 3♥ destek", new OneNT_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "J432", "Q32"),
                new[] { "1NT", "Pas", "2NT", "Pas", "3♥", "Pas" }, Player.Kuzey, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♥: 3NT — 2♥", new OneNT_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q2", "J432", "KQ32"),
                new[] { "1NT", "Pas", "2NT", "Pas", "3♥", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♠: 4♠ — 3♠ destek", new OneNT_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "J432", "Q32"),
                new[] { "1NT", "Pas", "2NT", "Pas", "3♠", "Pas" }, Player.Kuzey, "4♠",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1NT ⇒ 3♣ / 3♦ / 3♥ / 3♠ / Texas ──");
            Test("3♣: 3NT — ♦ fiti MAX", new OneNT_ThreeClub(),
                TestHelpers.ElOlustur("AK2", "KQ2", "KJ32", "QJ2"),
                new[] { "1NT", "Pas", "3♣", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣: 3♦ — zorunlu", new OneNT_ThreeClub(),
                TestHelpers.ElOlustur("KQ2", "K32", "KJ32", "Q32"),
                new[] { "1NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣-3♦: Pas — 5♦ S.O", new OneNT_ThreeClub(),
                TestHelpers.ElOlustur("32", "32", "Q9432", "J932"),
                new[] { "1NT", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣-3♦: 3♥ — 6♣ SNG♥", new OneNT_ThreeClub(),
                TestHelpers.ElOlustur("K32", "2", "Q32", "AKJ932"),
                new[] { "1NT", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣-3♦: 3♠ — 6♣ SNG♠", new OneNT_ThreeClub(),
                TestHelpers.ElOlustur("2", "K32", "Q32", "AKJ932"),
                new[] { "1NT", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣-3♦: 3NT — 6♣ dengeli", new OneNT_ThreeClub(),
                TestHelpers.ElOlustur("K2", "K32", "Q3", "AQJ932"),
                new[] { "1NT", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣-3♦: 4♣ — 6♣ SI", new OneNT_ThreeClub(),
                TestHelpers.ElOlustur("A2", "K32", "Q3", "AKJ932"),
                new[] { "1NT", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦: 3NT — majörlerde problem yok", new OneNT_ThreeDiamond(),
                TestHelpers.ElOlustur("KQ32", "KJ32", "Q32", "K2"),
                new[] { "1NT", "Pas", "3♦", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦: 3♥ — iyi ♥ problem ♠", new OneNT_ThreeDiamond(),
                TestHelpers.ElOlustur("432", "KJ32", "AQ32", "K2"),
                new[] { "1NT", "Pas", "3♦", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦: 3♠ — iyi ♠ problem ♥", new OneNT_ThreeDiamond(),
                TestHelpers.ElOlustur("KJ32", "432", "AQ32", "K2"),
                new[] { "1NT", "Pas", "3♦", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦: 4♦ — iki majörde problem", new OneNT_ThreeDiamond(),
                TestHelpers.ElOlustur("432", "432", "AKQ2", "KQ2"),
                new[] { "1NT", "Pas", "3♦", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦-3♥: 4♦ — SNG♠", new OneNT_ThreeDiamond(),
                TestHelpers.ElOlustur("2", "K32", "AKJ932", "Q32"),
                new[] { "1NT", "Pas", "3♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦-3♥: 3NT — SNG♥", new OneNT_ThreeDiamond(),
                TestHelpers.ElOlustur("K32", "3", "AKJ932", "Q32"),
                new[] { "1NT", "Pas", "3♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦-3♠: 4♦ — SNG♥", new OneNT_ThreeDiamond(),
                TestHelpers.ElOlustur("K32", "3", "AKJ932", "Q32"),
                new[] { "1NT", "Pas", "3♦", "Pas", "3♠", "Pas" }, Player.Kuzey, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦-3♠: 3NT — SNG♠", new OneNT_ThreeDiamond(),
                TestHelpers.ElOlustur("3", "K32", "AKJ932", "Q32"),
                new[] { "1NT", "Pas", "3♦", "Pas", "3♠", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥: 3NT — ♥ durdurucu", new OneNT_ThreeHeart(),
                TestHelpers.ElOlustur("KQ2", "KJ32", "Q32", "K32"),
                new[] { "1NT", "Pas", "3♥", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥: 4♦ — ♥ durdurucu yok", new OneNT_ThreeHeart(),
                TestHelpers.ElOlustur("KQ2", "432", "AQJ2", "K32"),
                new[] { "1NT", "Pas", "3♥", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥: 4♣ — ♥ durdurucu yok", new OneNT_ThreeHeart(),
                TestHelpers.ElOlustur("KQ2", "432", "AQ2", "KJ32"),
                new[] { "1NT", "Pas", "3♥", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠: 3NT — ♠ durdurucu", new OneNT_ThreeSpade(),
                TestHelpers.ElOlustur("KJ32", "K32", "Q32", "KQ2"),
                new[] { "1NT", "Pas", "3♠", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠: 4♦ — ♠ durdurucu yok", new OneNT_ThreeSpade(),
                TestHelpers.ElOlustur("432", "KQ2", "AQJ2", "K32"),
                new[] { "1NT", "Pas", "3♠", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("4♦ Texas: 4♥ ile kabul", new OneNT_FourDiamond(),
                TestHelpers.ElOlustur("KQ2", "K32", "Q432", "KJ2"),
                new[] { "1NT", "Pas", "4♦", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("4♥ Texas: 4♠ ile kabul", new OneNT_FourHeart(),
                TestHelpers.ElOlustur("KQ2", "K32", "Q432", "KJ2"),
                new[] { "1NT", "Pas", "4♥", "Pas" }, Player.Guney, "4♠",
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
