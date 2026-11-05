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
    public static class TestRunner_TwoClub_Gelisimler
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  2♣ GELİŞİM TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            sonucEkle("── 2♣ ⇒ 2♦ ──");
            Test("açan 2♥ — 5♥", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("AK2", "AKQJ3", "AK2", "K3"),
                new[] { "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 2♥ — 25 dengeli", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("AKQ", "AKJ", "AKQ", "KQJ2"),
                new[] { "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 2♠ — 5♠", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("AKQJ3", "AK2", "AK2", "K3"),
                new[] { "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 3♦ — 6♦", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("AK2", "AK2", "AKQJ32", "K"),
                new[] { "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 3♣ — 5♣", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("AK", "A2", "K432", "AKJ32"),
                new[] { "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2♠ — 3 HCP", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("432", "432", "Q432", "J32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2NT — 5/4 majör 5 HCP", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("Q9432", "J432", "32", "32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♣ — 5♥ 4♦ 9 HCP", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("32", "KJ932", "KQ32", "32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♦ — 6♥ 9 HCP", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("32", "KJ9432", "KQ2", "32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♠ — 5♥ 4♣", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("32", "KJ932", "32", "KQ32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3NT — 5♥ 3-3-2", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "KJ932", "Q32", "32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 2NT — 4 HCP", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("432", "432", "Q432", "J32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♠", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ sonrası 3♦ röle", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("32", "K32", "Q32", "K9432"),
                new[] { "2♣", "Pas", "2♦", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ sonrası açan 2NT — 25 dengeli", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("AKQ", "AKJ", "AKQ", "KQJ2"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ sonrası açan 3♦ — 6♥", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("AK2", "AKQJ32", "AK2", "K"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ sonrası açan 3♥ — 5♥ 4♣", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("A2", "AKQJ3", "K2", "AK32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ sonrası açan 3NT — 5♥ 4♠", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("KQ92", "AQJ93", "K2", "A2"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası cevapçı 3♦ — açan 4♥ (3♥)", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("AKQJ3", "K32", "A2", "K32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♠", "Pas", "3♦", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣-3♦ röle sonrası açan 3NT", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("AK2", "AK", "AK2", "AKJ32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası cevapçı 3♠ — açan 4♠", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("AKQ32", "AK2", "K32", "K3"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2NT", "Pas", "3♠", "Pas" }, Player.Guney, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠-2NT sonrası cevapçı 3NT", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("432", "432", "Q432", "J32"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠-3♦ sonrası 3♠ — 1 kupa", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("KQ432", "2", "KJ32", "Q43"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠-3♦ sonrası 4♣ — 3 kupa", new TwoClub_TwoDiamond(),
                TestHelpers.ElOlustur("KQ32", "432", "KJ3", "Q43"),
                new[] { "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas", "3♦", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♣ ⇒ 2♥ ──");
            Test("açan 2♠ — 3♠ tutuş", new TwoClub_TwoHeart(),
                TestHelpers.ElOlustur("AQ2", "AK2", "AK32", "AKQ"),
                new[] { "2♣", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 2NT — dengeli 2♠ yok", new TwoClub_TwoHeart(),
                TestHelpers.ElOlustur("K2", "AKQ", "AKQ2", "AKQ3"),
                new[] { "2♣", "Pas", "2♥", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 3♦ — 5♦", new TwoClub_TwoHeart(),
                TestHelpers.ElOlustur("2", "AK2", "AKQJ32", "AKQ"),
                new[] { "2♣", "Pas", "2♥", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — 5♠ 4♦", new TwoClub_TwoHeart(),
                TestHelpers.ElOlustur("KJ932", "32", "KQ32", "32"),
                new[] { "2♣", "Pas", "2♥", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♦ — 5♠ 4♥", new TwoClub_TwoHeart(),
                TestHelpers.ElOlustur("KJ932", "Q432", "32", "32"),
                new[] { "2♣", "Pas", "2♥", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♥ — 6♠", new TwoClub_TwoHeart(),
                TestHelpers.ElOlustur("KJ9432", "32", "Q32", "32"),
                new[] { "2♣", "Pas", "2♥", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♠ — 5♠ 4♣", new TwoClub_TwoHeart(),
                TestHelpers.ElOlustur("KJ932", "32", "32", "Q432"),
                new[] { "2♣", "Pas", "2♥", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3NT — 5♠ 3-3-2", new TwoClub_TwoHeart(),
                TestHelpers.ElOlustur("KJ932", "Q32", "J32", "32"),
                new[] { "2♣", "Pas", "2♥", "Pas", "2NT", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♣ ⇒ 2♠ ──");
            Test("açan 2NT — dengeli", new TwoClub_TwoSpade(),
                TestHelpers.ElOlustur("AQ2", "KQ2", "KQ2", "KJ32"),
                new[] { "2♣", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 3♦ — 5♦", new TwoClub_TwoSpade(),
                TestHelpers.ElOlustur("AK", "K432", "AQJ432", "2"),
                new[] { "2♣", "Pas", "2♠", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — 6♣ SNG yok", new TwoClub_TwoSpade(),
                TestHelpers.ElOlustur("K32", "32", "32", "KQJ432"),
                new[] { "2♣", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♦ — 5♣ 5♦", new TwoClub_TwoSpade(),
                TestHelpers.ElOlustur("32", "2", "KQ932", "KJ932"),
                new[] { "2♣", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♥ — 5♣ 3-3-2", new TwoClub_TwoSpade(),
                TestHelpers.ElOlustur("K32", "Q32", "32", "KJ932"),
                new[] { "2♣", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♠ — 6♣ SNG ♥", new TwoClub_TwoSpade(),
                TestHelpers.ElOlustur("K32", "2", "Q32", "KQJ432"),
                new[] { "2♣", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3NT — 6♣ SNG ♠ zayıf", new TwoClub_TwoSpade(),
                TestHelpers.ElOlustur("2", "K32", "Q32", "KJ9432"),
                new[] { "2♣", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 4♣ — 6♣ SNG ♠ SI", new TwoClub_TwoSpade(),
                TestHelpers.ElOlustur("A", "AQ2", "Q32", "KQJ432"),
                new[] { "2♣", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♣ ⇒ 2NT ──");
            Test("açan 3♣ röle", new TwoClub_TwoNT(),
                TestHelpers.ElOlustur("AK2", "AK2", "AK32", "AKQ"),
                new[] { "2♣", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("açan 3♦ — 6♦", new TwoClub_TwoNT(),
                TestHelpers.ElOlustur("A", "AK2", "AKQJ32", "AK2"),
                new[] { "2♣", "Pas", "2NT", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ sonrası 3♦ — 5♠ 5♥", new TwoClub_TwoNT(),
                TestHelpers.ElOlustur("KJ932", "KJ932", "32", "2"),
                new[] { "2♣", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ sonrası 3♥ — 5♠ 4♥", new TwoClub_TwoNT(),
                TestHelpers.ElOlustur("KJ932", "Q432", "32", "32"),
                new[] { "2♣", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ sonrası 3♠ — 5♥ 4♠", new TwoClub_TwoNT(),
                TestHelpers.ElOlustur("Q432", "KJ932", "32", "32"),
                new[] { "2♣", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♣ ⇒ 3♣ ──");
            Test("açan 3♦ — 2+♦", new TwoClub_ThreeClub(),
                TestHelpers.ElOlustur("AK2", "AK2", "AK", "AKQ32"),
                new[] { "2♣", "Pas", "3♣", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ sonrası 3♥ — SNG yok", new TwoClub_ThreeClub(),
                TestHelpers.ElOlustur("K32", "32", "KQJ432", "32"),
                new[] { "2♣", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ sonrası 3♠ — SNG ♥", new TwoClub_ThreeClub(),
                TestHelpers.ElOlustur("K32", "2", "KQJ432", "Q32"),
                new[] { "2♣", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ sonrası 3NT — SNG ♠ zayıf", new TwoClub_ThreeClub(),
                TestHelpers.ElOlustur("2", "K32", "KJ9432", "Q32"),
                new[] { "2♣", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ sonrası 4♣ — SNG ♠ SI", new TwoClub_ThreeClub(),
                TestHelpers.ElOlustur("A", "K32", "KQJ432", "Q32"),
                new[] { "2♣", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♣ ⇒ 3♦ ──");
            Test("açan 3♥ — 4+♥", new TwoClub_ThreeDiamond(),
                TestHelpers.ElOlustur("AK2", "AKQJ", "AK32", "AK"),
                new[] { "2♣", "Pas", "3♦", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ sonrası 4♣ — 5♣", new TwoClub_ThreeDiamond(),
                TestHelpers.ElOlustur("32", "KJ32", "32", "KQJ32"),
                new[] { "2♣", "Pas", "3♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ sonrası 4♦ — 5♦", new TwoClub_ThreeDiamond(),
                TestHelpers.ElOlustur("32", "KJ32", "KQJ32", "32"),
                new[] { "2♣", "Pas", "3♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ sonrası 3♠ — ♠ kontrol", new TwoClub_ThreeDiamond(),
                TestHelpers.ElOlustur("K32", "KJ32", "Q32", "Q32"),
                new[] { "2♣", "Pas", "3♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ sonrası 3NT — ♠ kontrol yok", new TwoClub_ThreeDiamond(),
                TestHelpers.ElOlustur("Q32", "KJ32", "KQ2", "Q32"),
                new[] { "2♣", "Pas", "3♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♣ ⇒ 3♥ ──");
            Test("açan 3♠ — 4+♠", new TwoClub_ThreeHeart(),
                TestHelpers.ElOlustur("AKQJ", "AK2", "AK32", "AK"),
                new[] { "2♣", "Pas", "3♥", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ sonrası 4♣ — 5♣", new TwoClub_ThreeHeart(),
                TestHelpers.ElOlustur("KJ32", "32", "32", "KQJ32"),
                new[] { "2♣", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ sonrası 4♦ — 5♦", new TwoClub_ThreeHeart(),
                TestHelpers.ElOlustur("KJ32", "32", "KQJ32", "32"),
                new[] { "2♣", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Kuzey, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ sonrası 3NT — dengeli", new TwoClub_ThreeHeart(),
                TestHelpers.ElOlustur("KJ32", "K32", "Q32", "Q32"),
                new[] { "2♣", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♣ ⇒ 3♠ ──");
            Test("açan 3NT röle", new TwoClub_ThreeSpade(),
                TestHelpers.ElOlustur("AK2", "AK2", "AK2", "AKQ2"),
                new[] { "2♣", "Pas", "3♠", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT sonrası 4♣ — 5♣ 3-3-2", new TwoClub_ThreeSpade(),
                TestHelpers.ElOlustur("K32", "Q32", "32", "KJ932"),
                new[] { "2♣", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT sonrası 4♦ — 5♦ 3-3-2", new TwoClub_ThreeSpade(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ932", "32"),
                new[] { "2♣", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Kuzey, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT sonrası 4♥ — 5♦ 4♣ SNG ♥", new TwoClub_ThreeSpade(),
                TestHelpers.ElOlustur("K32", "2", "KJ932", "Q432"),
                new[] { "2♣", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Kuzey, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT sonrası 4♠ — 5♦ 4♣ SNG ♠", new TwoClub_ThreeSpade(),
                TestHelpers.ElOlustur("2", "K32", "KJ932", "Q432"),
                new[] { "2♣", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Kuzey, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT sonrası 4NT — 5♦ 4♣ 2-2", new TwoClub_ThreeSpade(),
                TestHelpers.ElOlustur("K2", "Q2", "KJ932", "Q432"),
                new[] { "2♣", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Kuzey, "4NT",
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
