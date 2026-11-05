using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneSpade;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_OneSpade_Gelisimler
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1♠ GELİŞİM TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            sonucEkle("── 1♠ ⇒ 1NT — AÇANIN YENİDEN TEKLİFİ ──");
            Test("3NT — 5♠ dengeli 22 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "AK2", "QJ2", "K2"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("4♠ — 7♠", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQJ932", "K2", "32", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("4♣ — 6♠ 5♣ 12 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AQJ932", "K2", "", "Q9432"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("4♦ — 6♠ 5♦ 12 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AQJ932", "K2", "Q9432", ""),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("4♥ — 6♠ 5♥ 12 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AQJ932", "Q9432", "K2", ""),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 6♠ 4♣ 17 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQJ32", "3", "32", "AK32"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 6♠ 17 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQJ32", "K32", "A2", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 5♠ 5♣ 16 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "32", "3", "AK932"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 5♠ 5♦ 16 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "32", "AK932", "3"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 5♠ 5♥ 16 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "AK932", "3", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 6♠ 8 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("KQJ932", "32", "Q32", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5♠ 4♥ 12 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("KQJ32", "KJ32", "32", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 5♠ 4♦ 12 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("KQJ32", "32", "KJ32", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — 5♠ 3♣ 12 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("KQJ32", "K32", "Q32", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas" }, Player.Guney, "2♣",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♠ ⇒ 1NT — CEVAPÇININ DEVAMI ──");
            Test("2♣ sonrası 3♠ — 3♠ davet 11 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "Q32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 2♦ — röle 12 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("Q2", "K32", "KQ32", "Q432"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 3♣ — 6♣ zayıf", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("32", "32", "432", "KJ9432"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 2NT — 5/4 minör SNG♠", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("2", "J32", "Q432", "J9432"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 2♥ — 5♥ zayıf", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("2", "Q9432", "Q432", "J32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 2♠ — zayıf", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("Q32", "32", "Q432", "J432"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 4♠ — 14 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("K2", "K32", "AQ32", "Q432"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♠", "Pas" }, Player.Kuzey, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası Pas — 3 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("32", "432", "Q432", "J432"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♠", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — röle 9 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "J32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♥ — 6♥ S.O", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("32", "QJ9432", "432", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦ → 2♠ — 11 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("KQJ32", "K32", "Q32", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦ → 2NT — dengeli 16 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKJ32", "K32", "KQ2", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦ → 3♣ — 5♠ 4♣ 17 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "A2", "32", "KJ32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦ → 3♦ — 5♠ 4♦ 17 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "A2", "KJ32", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦ → 3♥ — 5♠ 5♥", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "AKJ32", "3", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦ → 3♠ — 6♠ 17 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQJ32", "AK2", "32", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦ → 3NT — 5♠ 2♥ 20 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "AK", "KJ32", "32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦ → 2♥ — şekil sorusu 20 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "AK2", "KJ32", "3"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦-2♥ → 2♠ (şekil sorusu)", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "Q32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦-2♠ → 2NT — 11 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "Q32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas", "2♠", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦-2♠ → 3♣ — 5♣ S.O", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("Q32", "J32", "32", "KJ932"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣-2♦-2♠ → Pas — 6 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("Q32", "J32", "J432", "Q32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas", "2♠", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("şekil: 3♥ — 5♠ 5♣", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "3", "32", "AKJ32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("şekil: 3♦ — 5♠ 4♦", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "AK2", "KJ32", "3"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("şekil: 3♣ — 5♠ 4♣", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "AK2", "3", "KJ32"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("şekil: 2NT — 5♠ 4♥", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQ32", "AKJ2", "32", "K2"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 4♣ — 6♠ 5♣", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQJ32", "3", "3", "AQ932"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 3NT — 6♠ 4♦ 19 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQJ32", "K32", "AQ32", ""),
                new[] { "1♠", "Pas", "1NT", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 3♥ — 6♠ 4♦ 17 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQJ32", "2", "KJ32", "K2"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 3♠ — 6♠ 20 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQJ32", "AK2", "32", "K2"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣ → 3♦ — 6♠ 17 HCP", new OneSpade_OneNT(),
                TestHelpers.ElOlustur("AKQJ32", "K32", "32", "A2"),
                new[] { "1♠", "Pas", "1NT", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♠ ⇒ 2♣ (FG röle) ──");
            Test("3♦ — 5♠ 5♦ 16 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("AKJ32", "32", "AKJ32", "3"),
                new[] { "1♠", "Pas", "2♣", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 5♠ 4♦ 2♥ 2♣ 20 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("AKQ32", "K2", "AKJ2", "32"),
                new[] { "1♠", "Pas", "2♣", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 5♠ 4♦ 3♥ 1♣ 20 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("AKQ32", "K32", "AKJ2", "3"),
                new[] { "1♠", "Pas", "2♣", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 5♠ 4♦ 3♣ 19 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("AKQ32", "3", "AKJ2", "Q32"),
                new[] { "1♠", "Pas", "2♣", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("4♣ — 5♠ 4♦ 3♣ 22 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("AKQ32", "3", "AKQJ", "K32"),
                new[] { "1♠", "Pas", "2♣", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 5♠ 4♦ 16 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("AKJ32", "K2", "KQ32", "32"),
                new[] { "1♠", "Pas", "2♣", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5♠ 4♥ 12 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("KQJ32", "AQ32", "32", "32"),
                new[] { "1♠", "Pas", "2♣", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♠ 4♣ 17 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("AKQ32", "32", "32", "AKJ2"),
                new[] { "1♠", "Pas", "2♣", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — dengeli 20 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("AK2", "KQ2", "KQ32", "K32"),
                new[] { "1♠", "Pas", "2♣", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — minimum 11 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("KQ32", "K32", "Q32", "J32"),
                new[] { "1♠", "Pas", "2♣", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 3♣ — 6♣ davet", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("32", "32", "K32", "KQJ932"),
                new[] { "1♠", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 4♠ — 3♠ destek 13 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("KJ2", "K32", "Q32", "KJ32"),
                new[] { "1♠", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 3NT — dengeli 14 HCP", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("32", "KQ2", "KJ32", "KQ32"),
                new[] { "1♠", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ sonrası 2♥ — röle SI", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("2", "KJ32", "AQ32", "KQ32"),
                new[] { "1♠", "Pas", "2♣", "Pas", "2♦", "Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2NT — 4♥", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("K2", "KQ32", "Q432", "J32"),
                new[] { "1♠", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2♠ — SI", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("K32", "K2", "AKJ32", "K32"),
                new[] { "1♠", "Pas", "2♣", "Pas", "2♥", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♦ — 7♣ SI", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("2", "32", "K32", "KQJ9432"),
                new[] { "1♠", "Pas", "2♣", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — röle", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("K32", "K32", "AQ32", "Q32"),
                new[] { "1♠", "Pas", "2♣", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ sonrası 3♦ — röle", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("K32", "K32", "AQ32", "Q32"),
                new[] { "1♠", "Pas", "2♣", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ sonrası 3♥ — röle", new OneSpade_TwoClub(),
                TestHelpers.ElOlustur("K32", "K32", "AQ32", "Q32"),
                new[] { "1♠", "Pas", "2♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♠ ⇒ 2♦ ──");
            Test("4♣ — 5♠ 5♦ 16 HCP", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("AKJ32", "3", "AKJ32", "32"),
                new[] { "1♠", "Pas", "2♦", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("4♦ — 5♠ 5♦ 9 HCP", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQ932", "3", "KJ932", "32"),
                new[] { "1♠", "Pas", "2♦", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 6♠ 4♦", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQJ932", "3", "AQ32", "32"),
                new[] { "1♠", "Pas", "2♦", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 5♠ 4♦", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQJ32", "3", "AQ32", "K32"),
                new[] { "1♠", "Pas", "2♦", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 6♠ 3♦", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQJ932", "32", "Q32", "K2"),
                new[] { "1♠", "Pas", "2♦", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 5♠ 3♦ dengeli 15 HCP", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("AQJ32", "K32", "KQ2", "32"),
                new[] { "1♠", "Pas", "2♦", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 5♠ 3♦ minimum", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQJ32", "32", "Q32", "K32"),
                new[] { "1♠", "Pas", "2♦", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5♠ 4♥", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQJ32", "AQ32", "32", "32"),
                new[] { "1♠", "Pas", "2♦", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♠ 4♣", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQJ32", "32", "32", "AQ32"),
                new[] { "1♠", "Pas", "2♦", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 6♠ 3♣", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQJ932", "32", "32", "K32"),
                new[] { "1♠", "Pas", "2♦", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 3♦ — 6♦ zayıf", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("2", "K2", "KJ9432", "Q932"),
                new[] { "1♠", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2NT — 4♥", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("32", "KQ32", "AK32", "J32"),
                new[] { "1♠", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2♠ — SI", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "32", "AKJ32", "K32"),
                new[] { "1♠", "Pas", "2♦", "Pas", "2♥", "Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ sonrası 3♠ — 5♦ 3♠ 16 HCP", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "32", "AKJ32", "KQ2"),
                new[] { "1♠", "Pas", "2♦", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ sonrası 3♦ — destek", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "KJ2"),
                new[] { "1♠", "Pas", "2♦", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ sonrası 3♥ — röle", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "KJ2"),
                new[] { "1♠", "Pas", "2♦", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ sonrası 3NT — DNG min", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "J32"),
                new[] { "1♠", "Pas", "2♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ sonrası 3♠ — röle", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("AK32", "K32", "AQ32", "J2"),
                new[] { "1♠", "Pas", "2♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥-3♠ → 4♦ (♣ KK)", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQJ932", "K432", "Q32", ""),
                new[] { "1♠", "Pas", "2♦", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥-3♠ → 4♥ (♥ BK)", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQJ932", "", "Q32", "K432"),
                new[] { "1♠", "Pas", "2♦", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥-3♠ → 4♣ (max)", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("AKQJ32", "32", "Q32", "K2"),
                new[] { "1♠", "Pas", "2♦", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥-3♠ → 3NT (min)", new OneSpade_TwoDiamond(),
                TestHelpers.ElOlustur("KQJ932", "32", "Q32", "K2"),
                new[] { "1♠", "Pas", "2♦", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♠ ⇒ 2♥ ──");
            Test("4♣ — 5/5 SNG♣ 16 HCP", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("AKJ32", "AQJ32", "K32", ""),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("4♦ — 5/5 SNG♦ 16 HCP", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("AKJ32", "AQJ32", "3", "32"),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("4♥ — 5/5 minimum", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQ932", "KJ932", "32", "3"),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 6♠ 4♥", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ932", "K432", "3", "32"),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 5♠ 4♥", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ32", "K432", "32", "32"),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 6♠ 3♥", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ932", "K32", "32", "32"),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 5♠ 3♥ dengeli 16 HCP", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("AQJ32", "K32", "AQ2", "32"),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♠ 3♥ minimum", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ32", "K32", "Q32", "32"),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 5♠ 3♥ 15 HCP", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("AQJ32", "K32", "KQ2", "32"),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♠ 4♣ 2♥", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ32", "32", "32", "AQ32"),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 5♠ 2♥ 3-3 minör", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ32", "32", "K32", "Q32"),
                new[] { "1♠", "Pas", "2♥", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♥ — 6♥ zayıf", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("2", "KJ9432", "432", "432"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♣ — 5♥ 5♣ 13 HCP", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("2", "AKJ32", "32", "KQ932"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 3♦ — 5♥ 5♦ 13 HCP", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("2", "AKJ32", "KQ932", "32"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ sonrası 2NT — röle", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "KJ2"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT sonrası 3♣ — röle", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "KJ2"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2NT", "Pas" }, Player.Kuzey, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ sonrası 3♦ — SAS", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "KJ2"),
                new[] { "1♠", "Pas", "2♥", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ sonrası 3♥ — röle", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "KJ2"),
                new[] { "1♠", "Pas", "2♥", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ sonrası 3♠ — röle", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "KJ2"),
                new[] { "1♠", "Pas", "2♥", "Pas", "3♥", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3NT — dengeli 3♥ min", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ32", "K32", "Q32", "32"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♥ — 3♥ min", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ32", "K32", "Q432", "3"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♠ — 3♥ güçlü", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("AKJ32", "K32", "AQ32", "3"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♣ — 4♣", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ32", "32", "32", "AQ32"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT → 3♦ — 4♦", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ32", "32", "AQ32", "32"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT-3♥ → 3♠ (KK)", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "KJ2"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas", "3♥", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠-2NT-3♠ → 3NT (BK)", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("K32", "K32", "Q432", "KJ2"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas", "3♠", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("KK: 4♣ — 1♣", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("KQJ32", "K32", "Q432", "3"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas", "3♥", "Pas", "3♠", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("BK: 4♦ — 4♣ 3♥", new OneSpade_TwoHeart(),
                TestHelpers.ElOlustur("AKJ32", "K32", "3", "AQ32"),
                new[] { "1♠", "Pas", "2♥", "Pas", "2♠", "Pas", "2NT", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♠ ⇒ 2♠ / 2NT ──");
            Test("2♠: Pas — 11 HCP", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("KQJ32", "K32", "Q32", "32"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 4♥ — 5/5 SI", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("AKJ32", "AQJ32", "K32", ""),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 4♣ — 5♠ 5♣ SI", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("AKJ32", "3", "32", "AQJ32"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 4♦ — 5♠ 5♦ SI", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("AKJ32", "3", "AQJ32", "32"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 3♥ — 5♠ 4♥ 17 HCP", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("AKJ32", "AQ32", "32", "K2"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 2NT — dengeli 17 HCP", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("AKQ32", "K32", "KQ2", "32"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 4♠ — 15 HCP", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("AKQJ32", "K32", "Q32", "3"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 3♣ — yardımcı renk", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("KQJ32", "32", "32", "AQ32"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 3♦ — yardımcı renk", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("KQJ32", "32", "AQ32", "32"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 3♥ — yardımcı renk", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("KQJ32", "AQ32", "32", "32"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: 2NT — dengeli 12 HCP", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("KQJ32", "K32", "Q32", "J2"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠: Pas — dengesiz 13 HCP", new OneSpade_TwoSpade(),
                TestHelpers.ElOlustur("AKJ932", "K3", "Q32", "32"),
                new[] { "1♠", "Pas", "2♠", "Pas" }, Player.Guney, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♠ — 11 HCP", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("KQJ32", "K32", "Q32", "32"),
                new[] { "1♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♦ — davet 14 HCP", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("KQJ32", "K32", "Q32", "K2"),
                new[] { "1♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3NT — SNG♣ SI", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("AKQ32", "K32", "AQ32", "3"),
                new[] { "1♠", "Pas", "2NT", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 4♣ — SNG♦ SI", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("AKQ32", "K32", "3", "AQ32"),
                new[] { "1♠", "Pas", "2NT", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 4♦ — SNG♥ SI", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("AKQ32", "3", "K432", "AQ2"),
                new[] { "1♠", "Pas", "2NT", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♥ — 5♠ 4♥ SI", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("AKQ32", "AQ32", "32", "K2"),
                new[] { "1♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT: 3♣ — röle SI", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("AKQ32", "K32", "KQ2", "A2"),
                new[] { "1♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♦ davet: 4♠ — max", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "Q32"),
                new[] { "1♠", "Pas", "2NT", "Pas", "3♦", "Pas" }, Player.Kuzey, "4♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♦ davet: 3♠ — min", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("Q32", "J32", "KJ32", "Q32"),
                new[] { "1♠", "Pas", "2NT", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 3♥ — SNG♣", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("K432", "Q432", "KJ32", "3"),
                new[] { "1♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 3♠ — SNG♦", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("K432", "Q432", "3", "KJ32"),
                new[] { "1♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 3NT — SNG♥", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("K432", "3", "Q432", "KJ32"),
                new[] { "1♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 4♣ — 5♣", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q2", "K32", "J9432"),
                new[] { "1♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 4♦ — 5♦", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q2", "J9432", "K32"),
                new[] { "1♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 4♥ — 5♥", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("K32", "J9432", "K2", "Q32"),
                new[] { "1♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "4♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 3♦ — max", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("K32", "Q32", "KJ32", "Q32"),
                new[] { "1♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT-3♣: 4♠ — min", new OneSpade_TwoNT(),
                TestHelpers.ElOlustur("Q32", "J32", "KJ32", "J32"),
                new[] { "1♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "4♠",
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
