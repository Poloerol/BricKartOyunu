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
    public static class TestRunner_OneHeart_Gelisimler
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1♥ GELİŞİM TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            sonucEkle("── 1♥ ⇒ 1♠ — AÇANIN YENİDEN TEKLİFİ ──");
            Test("1NT — 5♥ dengeli 11 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("K32", "KJ932", "Q32", "Q3"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "1NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♥ 4♠ 11 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KQ32", "KJ932", "Q32", "3"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 5♥ 4♠ 15 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AQ32", "AK932", "Q32", "3"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("4♠ — 6♥ 4♠ 13 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AQ32", "KJ9432", "K", "32"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("4♣ — 6♥ 4♠ KK 14 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AQ32", "AJ9432", "K32", ""),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("4♦ — 6♥ 4♠ BK 14 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AQ32", "AJ9432", "", "K32"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 6♥ 4♣ 17 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("A2", "AKJ932", "3", "KQ32"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 6♥ 15 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("K2", "AKJ932", "Q32", "Q3"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 5♥ 5♣ 17 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("A2", "AKJ32", "3", "KQ932"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 5♥ 5♦ 17 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("A2", "AKJ32", "KQ932", "3"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 6♥ 11 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("32", "KQJ932", "Q32", "K3"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — 5♥ 3♣ 14 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("A", "KQ932", "K32", "Q432"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 5♥ 4♦ 14 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("A3", "KQ932", "K432", "Q2"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("Perspektif: aynı el, Batı açtı → 2♦", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("A3", "KQ932", "K432", "Q2"),
                new[] { "1♥", "Pas", "1♠", "Pas" }, Player.Bati, "2♦",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♥ ⇒ 1♠ — CEVAPÇININ DEVAMI ──");
            Test("1NT sonrası 4♣ — SNG♣ SI", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AQJ32", "K32", "KQ32", "2"),
                new[] { "1♥", "Pas", "1♠", "Pas", "1NT", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT sonrası 3♠ — 6♠ FG", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AKJ932", "32", "Q32", "K3"),
                new[] { "1♥", "Pas", "1♠", "Pas", "1NT", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT sonrası 3NT — dengeli 14 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("K32", "K32", "AQ32", "Q32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "1NT", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT sonrası 2♦ — röle FG", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KQJ2", "3", "AQ32", "K932"),
                new[] { "1♥", "Pas", "1♠", "Pas", "1NT", "Pas" }, Player.Kuzey, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT sonrası 2NT — 6♣ zayıf (2NTt)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("32", "32", "432", "KJ9432"),
                new[] { "1♥", "Pas", "1♠", "Pas", "1NT", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT sonrası 2♣ — 5♦ zayıf (puppet)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("32", "32", "KJ932", "Q432"),
                new[] { "1♥", "Pas", "1♠", "Pas", "1NT", "Pas" }, Player.Kuzey, "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT sonrası 2♠ — 6♠ zayıf", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("QJ9432", "32", "432", "32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "1NT", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT sonrası 2♥ — 3♥ zayıf", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("J32", "Q32", "J432", "432"),
                new[] { "1♥", "Pas", "1♠", "Pas", "1NT", "Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 2♦ — röle 13 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("A32", "K32", "Q32", "KJ32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♣", "Pas" }, Player.Kuzey, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 2NT — 4♠ dengeli 11 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "Q2", "K432", "Q32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♣", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 3♥ — 4♠ 3♥ davet", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "Q32", "K32", "Q32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♣", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 3♠ — 6♠ davet", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KQJ932", "32", "K32", "32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♣", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 2♠ — 5♠ zayıf", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("QJ932", "32", "432", "J32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♣", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 2♥ — zayıf S.O", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("J432", "32", "432", "Q432"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♦", "Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 3♣ — 4. renk FG", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "2", "AQ32", "KQ32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♦", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♣ — 5♠ 3+♣ FG", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AQJ32", "32", "K2", "KJ32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 2NT — röle 13 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "K32", "AQ32", "32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♠", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♠ — 10 HCP davet", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "Q32", "K32", "J32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — röle FG", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "K32", "Q32", "Q32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ sonrası 3NT — röle 13 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "K32", "AQ32", "32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "3♠", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ sonrası 4♠ — 10 HCP", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KJ32", "Q32", "K32", "J32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "3♠", "Pas" }, Player.Kuzey, "4♠",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♥ ⇒ 1♠ — RÖLE CEVAPLARI ──");
            Test("1NT-2♣ → 2♦ (puppet)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("K32", "KJ932", "Q32", "Q3"),
                new[] { "1♥", "Pas", "1♠", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♣ (KK)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KQ32", "KJ932", "Q32", "3"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♦ (BK)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KQ32", "KJ932", "3", "Q32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♥ (DNG max)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("KQ32", "AJ932", "Q2", "Q3"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 3♦ (6♥4♣ 15-17)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("A2", "AKJ932", "3", "KQ32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠-3NT → 4♦ (KK)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AQ32", "AJ932", "KQ32", ""),
                new[] { "1♥", "Pas", "1♠", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠-3NT → 4♥ (BK)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AQ32", "AJ932", "", "KQ32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠-3NT → 4♣ (DNG max)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AQ32", "AJ932", "K3", "Q2"),
                new[] { "1♥", "Pas", "1♠", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠-3NT → 4♠ (DNG min)", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("AQ32", "KJ932", "K3", "32"),
                new[] { "1♥", "Pas", "1♠", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Guney, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT-2♣-2♦ sonrası Pas — S.O", new OneHeart_OneSpade(),
                TestHelpers.ElOlustur("32", "32", "KJ932", "Q432"),
                new[] { "1♥", "Pas", "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♥ ⇒ 1NT ──");
            Test("2♣ — 5♥ 3♣ 14 HCP", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("A", "KQ932", "K32", "Q432"),
                new[] { "1♥", "Pas", "1NT", "Pas" }, Player.Guney, "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 5♥ 4♦ 14 HCP", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("A3", "KQ932", "K432", "Q2"),
                new[] { "1♥", "Pas", "1NT", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 6♥ 11 HCP", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("32", "KQJ932", "Q32", "K3"),
                new[] { "1♥", "Pas", "1NT", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♥ 4m 19+ FG", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("A", "AKQ32", "AK32", "J32"),
                new[] { "1♥", "Pas", "1NT", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 6♥ 4♣ 17 HCP", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("A2", "AKJ932", "3", "KQ32"),
                new[] { "1♥", "Pas", "1NT", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 5♥ 5♣ 17 HCP", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("A2", "AKJ32", "3", "KQ932"),
                new[] { "1♥", "Pas", "1NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 5♥ dengeli 20 HCP", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("AK2", "AKQ32", "QJ2", "J3"),
                new[] { "1♥", "Pas", "1NT", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("4♥ — 7♥", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("2", "AKQJ932", "432", "32"),
                new[] { "1♥", "Pas", "1NT", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("4♣ — 6♥ 5♣ 11 HCP", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("2", "KQJ932", "3", "AJ432"),
                new[] { "1♥", "Pas", "1NT", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 3♥ — 3♥ davet 9 HCP", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("K32", "Q32", "K32", "J432"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 2♠ — 6♣ puppet", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("32", "32", "K32", "KQJ932"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 2♦ — röle 12 HCP", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("K32", "32", "AQ32", "K432"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 3♣ — 6♣ zayıf", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("32", "32", "432", "KJ9432"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♥ — davet 9 HCP", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("K32", "Q32", "K32", "J432"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♥ — şaka, ortak Pas", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("J32", "Q32", "432", "J432"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 2NT — röle", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("K32", "32", "AQ32", "K432"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♠", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — röle FG", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("K32", "32", "AQ32", "K432"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♦ — S.O", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("J432", "32", "432", "J432"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦ → 2♥ (min)", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("A", "KQ932", "K32", "Q432"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♣ (5♥4♣)", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("A", "AKQ32", "J32", "KJ32"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♥ (5♥5♣)", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("A", "AKQ32", "32", "KJ932"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 3♥ (6♥4♦ 15-17)", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("A2", "AKJ932", "KQ32", "3"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 4♣ (6♥5♣)", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("2", "AKJ932", "3", "KQ432"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♠-2NT sonrası 3♣ — 6♣ puppet", new OneHeart_OneNT(),
                TestHelpers.ElOlustur("32", "32", "K32", "KQJ932"),
                new[] { "1♥", "Pas", "1NT", "Pas", "2♣", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♥ ⇒ 2♣ (FG röle) ──");
            Test("2♦ — min 11 HCP", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K32", "KJ932", "Q32", "Q3"),
                new[] { "1♥", "Pas", "2♣", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5♥ 4♠ 11 HCP", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("KJ32", "KQ932", "Q32", "3"),
                new[] { "1♥", "Pas", "2♣", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♥ 4♣ 16 HCP", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("A2", "AKJ32", "32", "KJ32"),
                new[] { "1♥", "Pas", "2♣", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 5♥ 17 HCP", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("A2", "AKJ32", "K32", "Q32"),
                new[] { "1♥", "Pas", "2♣", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 2♥ — röle", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("AQ2", "K32", "Q432", "K32"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 3♠ — SNG♠ SI", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("2", "K32", "A32", "AKJ932"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 3♣ — 6♣ davet", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("32", "K2", "432", "KQJ932"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2NT — SAS w 4♠", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("KJ32", "32", "Q32", "AQ32"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2♠ — SAS", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K32", "32", "Q432", "AQ32"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 2NT — SAS", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K32", "32", "Q432", "AQ32"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♠", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — röle", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K32", "32", "Q432", "AQ32"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♥ — tutuş SI", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K32", "32", "AQ32", "AK32"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2♥ → 2♠ (5♥ 4♣)", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K32", "KJ932", "3", "Q432"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2♥ → 3♦ (6♥ 4♦)", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K", "KQJ932", "Q432", "32"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2♥ → 2NT (tek renk)", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K32", "KQJ932", "32", "Q3"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ → 3♣ (5♥4♠ min)", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("KJ32", "KQ932", "Q32", "3"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ → 3♦ (6♥4♠)", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("KJ32", "KQJ932", "Q3", "3"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥-2♠ → 2NT (6♥5♠)", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("KJ932", "KQJ932", "3", "2"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♣ (5♥4♣ 16+)", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("A2", "AKJ32", "32", "KJ32"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 3♥ (6♥ SNG yok)", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K32", "AKJ932", "Q3", "Q2"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 3♦ (6♥ SNG)", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K32", "AKJ932", "Q32", "3"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 3NT (5♥ 332 15-17)", new OneHeart_TwoClub(),
                TestHelpers.ElOlustur("K32", "AKJ32", "Q32", "Q3"),
                new[] { "1♥", "Pas", "2♣", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♥ ⇒ 2♦ (5+♦ FG) ──");
            Test("4♣ — 5♥ 5♦ 17 HCP", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("A", "AKQ32", "KJ932", "32"),
                new[] { "1♥", "Pas", "2♦", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("4♦ — 5♥ 5♦ 13 HCP", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("2", "AKJ32", "KQ932", "32"),
                new[] { "1♥", "Pas", "2♦", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 5♥ 4♦", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K2", "KQ932", "AJ32", "32"),
                new[] { "1♥", "Pas", "2♦", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 6♥ 4♦", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K", "KQJ932", "AJ32", "32"),
                new[] { "1♥", "Pas", "2♦", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 6♥ 3♦", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K2", "KQJ932", "AJ3", "32"),
                new[] { "1♥", "Pas", "2♦", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 5♥ 3♦ 332 15 HCP", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("AK2", "KQ932", "Q32", "J3"),
                new[] { "1♥", "Pas", "2♦", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 5♥ 3♦ 11 HCP", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K2", "KQ932", "Q32", "J32"),
                new[] { "1♥", "Pas", "2♦", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♥ 4♠", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("KJ32", "KQ932", "3", "Q32"),
                new[] { "1♥", "Pas", "2♦", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5♥ 4♣", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "KQ932", "3", "Q432"),
                new[] { "1♥", "Pas", "2♦", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 6♥ w/o 3♦", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "KQJ932", "3", "Q32"),
                new[] { "1♥", "Pas", "2♦", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2♠ — SAS", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "32", "AQJ32", "K32"),
                new[] { "1♥", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♥ — 5♦ 3♥ SI", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("32", "Q32", "AKJ32", "KQ2"),
                new[] { "1♥", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2NT — SAS w 4♣", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K3", "32", "AQJ32", "K432"),
                new[] { "1♥", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♠ — 5♦ 4♠ SI", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("AKQ2", "32", "AJ932", "32"),
                new[] { "1♥", "Pas", "2♦", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — röle", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "32", "AQJ32", "K32"),
                new[] { "1♥", "Pas", "2♦", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ sonrası 3♠ — röle", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "3", "AQJ432", "K32"),
                new[] { "1♥", "Pas", "2♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥-3♠ → 3NT (DNG)", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K2", "KQJ932", "AJ3", "32"),
                new[] { "1♥", "Pas", "2♦", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥-3♠ → 4♣ (KK)", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("K2", "KQJ932", "AJ32", "3"),
                new[] { "1♥", "Pas", "2♦", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥-3♠ → 4♦ (BK)", new OneHeart_TwoDiamond(),
                TestHelpers.ElOlustur("3", "KQJ932", "AJ3", "K32"),
                new[] { "1♥", "Pas", "2♦", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♥ ⇒ 2♥ / 2♠ / 2NT ──");
            Test("2♥: Pas — 11 HCP", new OneHeart_TwoHeart(),
                TestHelpers.ElOlustur("K2", "KQ932", "Q32", "J32"),
                new[] { "1♥", "Pas", "2♥", "Pas" }, Player.Guney, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: 3♠ — 6♥ 5♠ SI", new OneHeart_TwoHeart(),
                TestHelpers.ElOlustur("KJ932", "AKQ932", "3", "2"),
                new[] { "1♥", "Pas", "2♥", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: 4♣ — 5♥ 5♣ SI", new OneHeart_TwoHeart(),
                TestHelpers.ElOlustur("2", "AKQ32", "32", "AKJ32"),
                new[] { "1♥", "Pas", "2♥", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: 4♦ — 5♥ 5♦ SI", new OneHeart_TwoHeart(),
                TestHelpers.ElOlustur("2", "AKQ32", "AKJ32", "32"),
                new[] { "1♥", "Pas", "2♥", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: 2♠ — 5♥ 4♠ deneme", new OneHeart_TwoHeart(),
                TestHelpers.ElOlustur("KQ32", "AKJ32", "32", "K2"),
                new[] { "1♥", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: 2NT — dengeli 18 HCP", new OneHeart_TwoHeart(),
                TestHelpers.ElOlustur("AK2", "KQJ32", "Q32", "K3"),
                new[] { "1♥", "Pas", "2♥", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: 4♥ — 17 HCP", new OneHeart_TwoHeart(),
                TestHelpers.ElOlustur("A2", "AKQJ32", "K32", "32"),
                new[] { "1♥", "Pas", "2♥", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: 3♣ — yardımcı renk denemesi", new OneHeart_TwoHeart(),
                TestHelpers.ElOlustur("3", "KQ932", "Q32", "AQ32"),
                new[] { "1♥", "Pas", "2♥", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥: 3♦ — yardımcı renk denemesi", new OneHeart_TwoHeart(),
                TestHelpers.ElOlustur("3", "KQ932", "AQ32", "Q32"),
                new[] { "1♥", "Pas", "2♥", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 3♥ — 6♥ iyi el", new OneHeart_TwoSpade(),
                TestHelpers.ElOlustur("K2", "AKQJ32", "32", "Q32"),
                new[] { "1♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 2NT — röle 16 HCP", new OneHeart_TwoSpade(),
                TestHelpers.ElOlustur("AK2", "KQ932", "32", "KJ3"),
                new[] { "1♥", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 3♣ — tercih", new OneHeart_TwoSpade(),
                TestHelpers.ElOlustur("K2", "KQJ32", "32", "Q432"),
                new[] { "1♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 3♦ — tercih", new OneHeart_TwoSpade(),
                TestHelpers.ElOlustur("K2", "KQJ32", "Q432", "32"),
                new[] { "1♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT: 3♥ — SNG♥", new OneHeart_TwoSpade(),
                TestHelpers.ElOlustur("32", "2", "K9432", "Q9432"),
                new[] { "1♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT: 3♠ — SNG♠", new OneHeart_TwoSpade(),
                TestHelpers.ElOlustur("3", "32", "K9432", "Q9432"),
                new[] { "1♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT: 3♦ — 6/5 minör 1-1", new OneHeart_TwoSpade(),
                TestHelpers.ElOlustur("2", "3", "KQ9432", "Q9432"),
                new[] { "1♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT: 3♣ — min", new OneHeart_TwoSpade(),
                TestHelpers.ElOlustur("32", "K2", "K9432", "Q932"),
                new[] { "1♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♥ — min 11 HCP", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("K2", "KQ932", "Q32", "J32"),
                new[] { "1♥", "Pas", "2NT", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♦ — davet 14 HCP", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("K2", "AQ932", "Q32", "K32"),
                new[] { "1♥", "Pas", "2NT", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3NT — SNG♣ SI", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("A32", "AKQ32", "KJ32", "3"),
                new[] { "1♥", "Pas", "2NT", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 4♣ — SNG♦ SI", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("A32", "AKQ32", "3", "KJ32"),
                new[] { "1♥", "Pas", "2NT", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 4♦ — SNG♠ SI", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("3", "AKQ32", "A32", "KJ32"),
                new[] { "1♥", "Pas", "2NT", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♠ — 5♥ 4♠ SI", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("AQ32", "AKJ32", "32", "K2"),
                new[] { "1♥", "Pas", "2NT", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♣ — röle SI", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("AK2", "AKJ32", "Q32", "K3"),
                new[] { "1♥", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 3♥ — SNG♣", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("K432", "Q432", "KJ32", "3"),
                new[] { "1♥", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 3♠ — SNG♦", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("K432", "Q432", "3", "KJ32"),
                new[] { "1♥", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 3NT — SNG♠", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("3", "Q32", "K32", "KJ4321"),
                new[] { "1♥", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 4♣ — 5♣ 3♥", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("K2", "Q32", "K32", "J9432"),
                new[] { "1♥", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 4♦ — 5♦ 3♥", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("K2", "Q32", "J9432", "K32"),
                new[] { "1♥", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 4♥ — min", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "J32"),
                new[] { "1♥", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 3♦ — max 11 HCP", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "Q32"),
                new[] { "1♥", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♦ davet: 3♥ — min", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "J32"),
                new[] { "1♥", "Pas", "2NT", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♦ davet: 4♥ — max", new OneHeart_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "Q32"),
                new[] { "1♥", "Pas", "2NT", "Pas", "3♦", "Pas" }, Player.Kuzey, "4♥",
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
