using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.Araya;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_Araya
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  ARAYA GİRİŞE CEVAPLAR (1♣/1♦/1♥/1♠ açılışları, Rubensohl, 1NT girişi)");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");
            int gecen = 0, kalan = 0;
            var m = new Araya_Cevap();
            // Rakip (Batı) 1♣, ortak (Kuzey) araya girdi, Doğu Pas, ben Güney
            string[] hD = { "1♣", "1♦", "Pas" };
            string[] hH = { "1♣", "1♥", "Pas" };
            string[] hS = { "1♣", "1♠", "Pas" };
            sonucEkle("── 1♣ - (1♦) - Pas - ? ──");
            T("1♠ F1R: 4♠ 7 HCP", m, TestHelpers.ElOlustur("AJ32", "432", "432", "Q43"), hD, "1♠", sonucEkle, ref gecen, ref kalan);
            T("1NT: 9 HCP dengeli ♣ stoper", m, TestHelpers.ElOlustur("Q32", "K32", "32", "KJ432"), hD, "1NT", sonucEkle, ref gecen, ref kalan);
            T("2♦: 3♦ tutuş 6 HCP", m, TestHelpers.ElOlustur("Q32", "432", "KJ2", "5432"), hD, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2♣ cuebid: 3♦ 11 HCP", m, TestHelpers.ElOlustur("A32", "432", "KJ2", "KJ32"), hD, "2♣", sonucEkle, ref gecen, ref kalan);
            T("2NT: 13 HCP ♣ stoper", m, TestHelpers.ElOlustur("KJ2", "Q32", "K2", "AJ432"), hD, "2NT", sonucEkle, ref gecen, ref kalan);
            T("3NT: 16 HCP dengeli", m, TestHelpers.ElOlustur("AK2", "Q32", "KJ2", "AJ32"), hD, "3NT", sonucEkle, ref gecen, ref kalan);
            T("2♠: 6♠ 7 HCP", m, TestHelpers.ElOlustur("KQ9432", "432", "32", "32"), hD, "2♠", sonucEkle, ref gecen, ref kalan);
            T("3♦ blokatif: 5♦ 3 HCP", m, TestHelpers.ElOlustur("432", "32", "QJ432", "432"), hD, "3♦", sonucEkle, ref gecen, ref kalan);
            T("Pas: 3 HCP", m, TestHelpers.ElOlustur("432", "432", "432", "Q432"), hD, null, sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 1♣ - (1♥) - Pas - ? ──");
            T("1♠ F1R: 4♠ 7 HCP", m, TestHelpers.ElOlustur("AJ32", "42", "432", "Q432"), hH, "1♠", sonucEkle, ref gecen, ref kalan);
            T("2♣ transfer: 5♦ 8 HCP", m, TestHelpers.ElOlustur("32", "42", "AJ932", "Q432"), hH, "2♣", sonucEkle, ref gecen, ref kalan);
            T("2♥: 3♥ 6 HCP", m, TestHelpers.ElOlustur("Q32", "KJ2", "432", "5432"), hH, "2♥", sonucEkle, ref gecen, ref kalan);
            T("2♦: 3♥ 10 HCP", m, TestHelpers.ElOlustur("A32", "KJ2", "432", "KJ32"), hH, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2NT: 4♥ 12 HCP", m, TestHelpers.ElOlustur("K32", "KJ32", "A32", "J32"), hH, "2NT", sonucEkle, ref gecen, ref kalan);
            T("3♣ mixed raise: 4♥ 10 HCP", m, TestHelpers.ElOlustur("K32", "KJ32", "J432", "Q3"), hH, "3♣", sonucEkle, ref gecen, ref kalan);
            T("3♠ splinter: 4♥ 11 HCP ♠ tek", m, TestHelpers.ElOlustur("2", "AKJ32", "K432", "J32"), hH, "3♠", sonucEkle, ref gecen, ref kalan);
            T("3♥ blokatif: 5♥ 3 HCP", m, TestHelpers.ElOlustur("432", "QJ432", "32", "432"), hH, "3♥", sonucEkle, ref gecen, ref kalan);
            T("1NT: 9 HCP dengeli", m, TestHelpers.ElOlustur("Q32", "32", "KJ32", "KJ32"), hH, "1NT", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 1♣ - (1♠) - Pas - ? ──");
            T("2♣ transfer ♦", m, TestHelpers.ElOlustur("32", "42", "AJ932", "Q432"), hS, "2♣", sonucEkle, ref gecen, ref kalan);
            T("2♦ transfer ♥", m, TestHelpers.ElOlustur("32", "AJ932", "42", "Q432"), hS, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2♠: 3♠ 6 HCP", m, TestHelpers.ElOlustur("KQ2", "432", "432", "5432"), hS, "2♠", sonucEkle, ref gecen, ref kalan);
            T("2♥: 3♠ 10 HCP", m, TestHelpers.ElOlustur("KJ2", "432", "A432", "KJ3"), hS, "2♥", sonucEkle, ref gecen, ref kalan);
            T("2NT: 4♠ 12 HCP", m, TestHelpers.ElOlustur("KJ32", "K32", "A32", "J32"), hS, "2NT", sonucEkle, ref gecen, ref kalan);
            T("3♣ mixed raise: 4♠ 10 HCP", m, TestHelpers.ElOlustur("KJ32", "K32", "J432", "Q3"), hS, "3♣", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── Araya girenin transfer kabulü ──");
            string[] k1 = { "1♣", "1♥", "Pas", "2♣", "Pas" };
            string[] k2 = { "1♣", "1♠", "Pas", "2♦", "Pas" };
            string[] k3 = { "1♣", "1♠", "Pas", "2♣", "Pas" };
            TK("1♥ sonrası 2♣ → 2♦", m, TestHelpers.ElOlustur("AKJ32", "K32", "32", "432"), k1, "2♦", sonucEkle, ref gecen, ref kalan);
            TK("1♠ sonrası 2♦ → 2♥", m, TestHelpers.ElOlustur("AKJ32", "K32", "32", "432"), k2, "2♥", sonucEkle, ref gecen, ref kalan);
            TK("1♠ sonrası 2♣ → 2♦", m, TestHelpers.ElOlustur("AKJ32", "K32", "32", "432"), k3, "2♦", sonucEkle, ref gecen, ref kalan);


            sonucEkle("── 1♦ - (1♥) - Pas - ? ──");
            string[] dH = { "1♦", "1♥", "Pas" };
            T("1♠ F1R", m, TestHelpers.ElOlustur("AJ32", "42", "432", "Q432"), dH, "1♠", sonucEkle, ref gecen, ref kalan);
            T("2♣ F1R: 5♣ 10 HCP", m, TestHelpers.ElOlustur("Q32", "42", "432", "AKJ32"), dH, "2♣", sonucEkle, ref gecen, ref kalan);
            T("2♦ cuebid: 3♥ 10 HCP", m, TestHelpers.ElOlustur("A32", "KJ2", "432", "Q432"), dH, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2♠ FSJS: 4♥ 5♠ 10 HCP", m, TestHelpers.ElOlustur("AKJ32", "Q432", "32", "32"), dH, "2♠", sonucEkle, ref gecen, ref kalan);
            T("2NT: 4♥ 12 HCP", m, TestHelpers.ElOlustur("K32", "KJ32", "A32", "J32"), dH, "2NT", sonucEkle, ref gecen, ref kalan);
            T("3♦ mixed raise: 4♥ 10 HCP", m, TestHelpers.ElOlustur("K32", "KJ32", "J432", "Q3"), dH, "3♦", sonucEkle, ref gecen, ref kalan);
            T("3♠ splinter", m, TestHelpers.ElOlustur("2", "AKJ32", "K432", "J32"), dH, "3♠", sonucEkle, ref gecen, ref kalan);
            T("3♥ blokatif", m, TestHelpers.ElOlustur("432", "QJ432", "32", "432"), dH, "3♥", sonucEkle, ref gecen, ref kalan);
            T("1NT: ♦ stoper 10 HCP", m, TestHelpers.ElOlustur("Q32", "32", "KJ32", "KJ43"), dH, "1NT", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 1♦ - (1♠) - Pas - ? ──");
            string[] dS = { "1♦", "1♠", "Pas" };
            T("2♦ transfer ♥", m, TestHelpers.ElOlustur("32", "AJ932", "42", "Q432"), dS, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2♣ F1R: 5♣ 12 HCP", m, TestHelpers.ElOlustur("32", "K42", "432", "AKQ32"), dS, "2♣", sonucEkle, ref gecen, ref kalan);
            T("2♥ cuebid: 3♠ 12 HCP", m, TestHelpers.ElOlustur("A32", "K32", "432", "KQ32"), dS, "2♥", sonucEkle, ref gecen, ref kalan);
            T("2♠ raise: 3♠ 5 HCP", m, TestHelpers.ElOlustur("KQ2", "432", "432", "5432"), dS, "2♠", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 1♦ - (2♣) - Pas - ? ──");
            string[] dC = { "1♦", "2♣", "Pas" };
            T("2♦ transfer ♥", m, TestHelpers.ElOlustur("32", "AJ932", "42", "Q432"), dC, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2♥ transfer ♠", m, TestHelpers.ElOlustur("AJ932", "32", "42", "Q432"), dC, "2♥", sonucEkle, ref gecen, ref kalan);
            T("2♠ cuebid: 4♣ 11 HCP", m, TestHelpers.ElOlustur("A32", "K32", "432", "KJ32"), dC, "2♠", sonucEkle, ref gecen, ref kalan);
            T("2NT: 9 HCP ♦ stoper", m, TestHelpers.ElOlustur("Q32", "K32", "J5432", "KJ"), dC, "2NT", sonucEkle, ref gecen, ref kalan);
            T("3NT: 17 HCP", m, TestHelpers.ElOlustur("AK2", "KQ2", "AJ32", "432"), dC, "3NT", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 1♥ - (1♠) - Pas - ? ──");
            string[] hSp = { "1♥", "1♠", "Pas" };
            T("2♣ F1R: 5♣ 10 HCP", m, TestHelpers.ElOlustur("32", "42", "5432", "AKQJ3"), hSp, "2♣", sonucEkle, ref gecen, ref kalan);
            T("2♥ cuebid: 3♠ 12 HCP", m, TestHelpers.ElOlustur("A32", "K32", "432", "KQ32"), hSp, "2♥", sonucEkle, ref gecen, ref kalan);
            T("3♥ mixed raise: 4♠ 10 HCP", m, TestHelpers.ElOlustur("KJ32", "K32", "J432", "Q3"), hSp, "3♥", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 1♥ - (2♣) / (2♦) - Pas - ? ──");
            string[] hC = { "1♥", "2♣", "Pas" };
            string[] hD2 = { "1♥", "2♦", "Pas" };
            T("2♥ transfer ♠ (2♣ girişi)", m, TestHelpers.ElOlustur("AJ932", "42", "432", "Q32"), hC, "2♥", sonucEkle, ref gecen, ref kalan);
            T("2♦ F1R: 5♦ 10 HCP", m, TestHelpers.ElOlustur("432", "432", "AKJ32", "Q2"), hC, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2♠ cuebid: 4♣ 12 HCP", m, TestHelpers.ElOlustur("A32", "K32", "432", "KQ32"), hC, "2♠", sonucEkle, ref gecen, ref kalan);
            T("2♥ transfer ♠ (2♦ girişi)", m, TestHelpers.ElOlustur("AJ932", "42", "32", "Q432"), hD2, "2♥", sonucEkle, ref gecen, ref kalan);
            T("2♠ transfer ♣ (2♦ girişi)", m, TestHelpers.ElOlustur("32", "42", "5432", "AKJ32"), hD2, "2♠", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 1♠ - (2♣) / (2♦) / (2♥) - Pas - ? ──");
            string[] sC = { "1♠", "2♣", "Pas" };
            string[] sD = { "1♠", "2♦", "Pas" };
            string[] sH = { "1♠", "2♥", "Pas" };
            T("2♦ F1R: 5♦ 10 HCP (2♣ girişi)", m, TestHelpers.ElOlustur("432", "432", "AKJ32", "Q2"), sC, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2♠ cuebid: 4♣ 12 HCP (2♣ girişi)", m, TestHelpers.ElOlustur("A32", "K32", "432", "KQ32"), sC, "2♠", sonucEkle, ref gecen, ref kalan);
            T("2♠ transfer ♣ (2♦ girişi)", m, TestHelpers.ElOlustur("32", "42", "5432", "AKJ32"), sD, "2♠", sonucEkle, ref gecen, ref kalan);
            T("2♥ F1R: 5♥ 10 HCP (2♦ girişi)", m, TestHelpers.ElOlustur("32", "AKJ32", "42", "Q432"), sD, "2♥", sonucEkle, ref gecen, ref kalan);
            T("3♣ cuebid: 4♦ 10 HCP (2♦ girişi)", m, TestHelpers.ElOlustur("A32", "K32", "K432", "432"), sD, "3♣", sonucEkle, ref gecen, ref kalan);
            T("2♠ transfer ♣ (2♥ girişi)", m, TestHelpers.ElOlustur("32", "42", "5432", "AKJ32"), sH, "2♠", sonucEkle, ref gecen, ref kalan);
            T("3♣ transfer ♦ (2♥ girişi)", m, TestHelpers.ElOlustur("32", "42", "AKJ32", "5432"), sH, "3♣", sonucEkle, ref gecen, ref kalan);
            T("2NT: 4♥ 12 HCP (2♥ girişi)", m, TestHelpers.ElOlustur("K32", "KJ32", "A32", "J32"), sH, "2NT", sonucEkle, ref gecen, ref kalan);
            T("3♦ cuebid: 3♥ 12 HCP (2♥ girişi)", m, TestHelpers.ElOlustur("A32", "K32", "432", "KQ32"), sH, "3♦", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── Transfer kabulleri (diğer açılışlar) ──");
            TK("1♦-(1♠) 2♦ → 2♥", m, TestHelpers.ElOlustur("AKJ32", "K32", "32", "432"), new[] { "1♦", "1♠", "Pas", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TK("1♦-(2♣) 2♦ → 2♥", m, TestHelpers.ElOlustur("32", "K32", "32", "AKJ432"), new[] { "1♦", "2♣", "Pas", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TK("1♦-(2♣) 2♥ → 2♠", m, TestHelpers.ElOlustur("32", "K32", "32", "AKJ432"), new[] { "1♦", "2♣", "Pas", "2♥", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TK("1♥-(2♣) 2♥ → 2♠", m, TestHelpers.ElOlustur("32", "K32", "32", "AKJ432"), new[] { "1♥", "2♣", "Pas", "2♥", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TK("1♥-(2♦) 2♠ → 3♣", m, TestHelpers.ElOlustur("32", "K32", "AKJ432", "32"), new[] { "1♥", "2♦", "Pas", "2♠", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            TK("1♠-(2♥) 3♣ → 3♦", m, TestHelpers.ElOlustur("32", "AKJ432", "K32", "32"), new[] { "1♠", "2♥", "Pas", "3♣", "Pas" }, "3♦", sonucEkle, ref gecen, ref kalan);
            TK("1♠-(2♦) 2♠ → 3♣", m, TestHelpers.ElOlustur("32", "K32", "AKJ432", "32"), new[] { "1♠", "2♦", "Pas", "2♠", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── RUBENSOHL: 1♣ - (1♥) - 1♠ - ? ──");
            var rb = new Araya_Rubensohl();
            T("DBL: 4♦ 8 HCP", rb, TestHelpers.ElOlustur("432", "42", "AJ32", "K432"), new[] { "1♣", "1♥", "1♠" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            T("1NT doğal", rb, TestHelpers.ElOlustur("AJ3", "42", "K32", "Q9432"), new[] { "1♣", "1♥", "1♠" }, "1NT", sonucEkle, ref gecen, ref kalan);
            T("2♣ ekonomik cue: 3♥ 9 HCP", rb, TestHelpers.ElOlustur("K32", "KJ2", "432", "Q432"), new[] { "1♣", "1♥", "1♠" }, "2♣", sonucEkle, ref gecen, ref kalan);
            T("2♦ NF: 5♦ 7 HCP", rb, TestHelpers.ElOlustur("432", "42", "AJ932", "K43"), new[] { "1♣", "1♥", "1♠" }, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2♥ yükseltme: 3♥ 6 HCP", rb, TestHelpers.ElOlustur("432", "K32", "432", "QJ32"), new[] { "1♣", "1♥", "1♠" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("2♠ pahalı cue: 4♥ 12 HCP", rb, TestHelpers.ElOlustur("32", "AJ32", "K432", "KJ3"), new[] { "1♣", "1♥", "1♠" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("2NT transfer: mixed raise 4♥ 8 HCP", rb, TestHelpers.ElOlustur("32", "KJ32", "432", "KJ32"), new[] { "1♣", "1♥", "1♠" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("3♣ transfer: 6♦ davet+", rb, TestHelpers.ElOlustur("K432", "42", "AKQ932", "4"), new[] { "1♣", "1♥", "1♠" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("3♥ blokatif: 5♥ 4 HCP", rb, TestHelpers.ElOlustur("32", "KJ932", "432", "432"), new[] { "1♣", "1♥", "1♠" }, "3♥", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── 1♦ - (1♠) - 2♣ - ? ──");
            T("2♦ ekonomik cue: 3♠ 9 HCP", rb, TestHelpers.ElOlustur("K32", "KQ2", "432", "J432"), new[] { "1♦", "1♠", "2♣" }, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2♥ NF: 5♥ 8 HCP", rb, TestHelpers.ElOlustur("32", "AJ932", "432", "K32"), new[] { "1♦", "1♠", "2♣" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("2♠ yükseltme 3♠ 6 HCP", rb, TestHelpers.ElOlustur("Q32", "432", "432", "KJ32"), new[] { "1♦", "1♠", "2♣" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("2NT transfer: pahalı cue 4♠ 10 HCP", rb, TestHelpers.ElOlustur("AJ32", "K32", "32", "Q432"), new[] { "1♦", "1♠", "2♣" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("3♣ transfer: mixed raise 4♠ 7 HCP", rb, TestHelpers.ElOlustur("KJ32", "432", "432", "K43"), new[] { "1♦", "1♠", "2♣" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("3♦ transfer: 6♥ davet+", rb, TestHelpers.ElOlustur("K2", "AKQ932", "432", "43"), new[] { "1♦", "1♠", "2♣" }, "3♦", sonucEkle, ref gecen, ref kalan);
            T("3♠ blokatif", rb, TestHelpers.ElOlustur("KJ932", "432", "432", "32"), new[] { "1♦", "1♠", "2♣" }, "3♠", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── 1♣ - (1♥) - 2♦ - ? ──");
            T("DBL: 3♥ 9 HCP (ekonomik cue 3 seviyesinde)", rb, TestHelpers.ElOlustur("K32", "KQ2", "432", "J432"), new[] { "1♣", "1♥", "2♦" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            T("2NT transfer: mixed raise 4♥ 7 HCP", rb, TestHelpers.ElOlustur("K32", "KJ32", "432", "432"), new[] { "1♣", "1♥", "2♦" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("3♣ transfer: pahalı cue 4♥ 14 HCP", rb, TestHelpers.ElOlustur("K32", "AJ32", "Q32", "KJ3"), new[] { "1♣", "1♥", "2♦" }, "3♣", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── 1♥ - (2♣) - 2♦ - ? ──");
            T("2NT transfer: 4♣ 7 HCP mixed raise", rb, TestHelpers.ElOlustur("K32", "432", "432", "KJ32"), new[] { "1♥", "2♣", "2♦" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("2♥ ekonomik cue: 3♣ 9 HCP", rb, TestHelpers.ElOlustur("K32", "5432", "Q32", "KJ2"), new[] { "1♥", "2♣", "2♦" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("3♣ blokatif: 5♣ 4 HCP", rb, TestHelpers.ElOlustur("32", "432", "432", "KJ932"), new[] { "1♥", "2♣", "2♦" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("3♥ transfer: 6♠ davet+", rb, TestHelpers.ElOlustur("AKJ932", "432", "32", "K4"), new[] { "1♥", "2♣", "2♦" }, "3♥", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── 1♣ - (1♥) - 1NT - ? ──");
            T("2♦ NF: 5♦ 8 HCP", rb, TestHelpers.ElOlustur("432", "42", "AJ932", "K43"), new[] { "1♣", "1♥", "1NT" }, "2♦", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Rubensohl: transfer kabulü (araya giren) ve cevapçının ikinci hamlesi ──");
            TK("1♣-(1♥)-1♠-2NT → 3♣", rb, TestHelpers.ElOlustur("K32", "AKJ32", "32", "432"), new[] { "1♣", "1♥", "1♠", "2NT", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            TK("1♦-(1♠)-2♣-3♦ → 3♥", rb, TestHelpers.ElOlustur("32", "K32", "432", "AKJ32"), new[] { "1♦", "1♠", "2♣", "3♦", "Pas" }, "3♥", sonucEkle, ref gecen, ref kalan);
            TK("1♦-(1♠)-2♣-3♣ → 3♦", rb, TestHelpers.ElOlustur("AKJ32", "32", "432", "K32"), new[] { "1♦", "1♠", "2♣", "3♣", "Pas" }, "3♦", sonucEkle, ref gecen, ref kalan);
            TK("3♠ blokatif transfer değil", rb, TestHelpers.ElOlustur("AKJ32", "32", "432", "K32"), new[] { "1♦", "1♠", "2♣", "3♠", "Pas" }, null, sonucEkle, ref gecen, ref kalan);
            TK("3♥ blokatif transfer değil", rb, TestHelpers.ElOlustur("32", "AKJ32", "432", "K32"), new[] { "1♣", "1♥", "1♠", "3♥", "Pas" }, null, sonucEkle, ref gecen, ref kalan);
            T("mixed raise sonrası 3♥", rb, TestHelpers.ElOlustur("K32", "KJ32", "432", "432"), new[] { "1♣", "1♥", "1♠", "2NT", "Pas", "3♣", "Pas" }, "3♥", sonucEkle, ref gecen, ref kalan);
            T("6♥ transferi sonrası kuvvetliyse 4♥", rb, TestHelpers.ElOlustur("A2", "AKQ932", "432", "43"), new[] { "1♦", "1♠", "2♣", "3♦", "Pas", "3♥", "Pas" }, "4♥", sonucEkle, ref gecen, ref kalan);
            T("pahalı cue sonrası 4♠", rb, TestHelpers.ElOlustur("AJ32", "KQ2", "32", "Q432"), new[] { "1♦", "1♠", "2♣", "2NT", "Pas", "3♣", "Pas" }, "4♠", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 1NT araya giriş (15-18): cevaplar ──");
            var nt = new Araya_NTCevap();
            T("1♣: Stayman 4♠ 8 HCP", nt, TestHelpers.ElOlustur("AJ32", "432", "K32", "432"), new[] { "1♣", "1NT", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            T("1♣: zayıf 5♥ → 2♦ transfer", nt, TestHelpers.ElOlustur("32", "KQ932", "432", "432"), new[] { "1♣", "1NT", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            T("1♣: zayıf 5♠ → 2♥ transfer", nt, TestHelpers.ElOlustur("KQ932", "32", "432", "432"), new[] { "1♣", "1NT", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("1♣: zayıf 6♦ → 2♠ transfer", nt, TestHelpers.ElOlustur("32", "32", "KJ9432", "432"), new[] { "1♣", "1NT", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("1♣: Texas 6♥ 11 HCP", nt, TestHelpers.ElOlustur("K2", "AKJ932", "432", "43"), new[] { "1♣", "1NT", "Pas" }, "4♦", sonucEkle, ref gecen, ref kalan);
            T("1♣: 3NT dengeli 13 HCP stoperli", nt, TestHelpers.ElOlustur("K32", "K32", "Q32", "KQ32"), new[] { "1♣", "1NT", "Pas" }, "3NT", sonucEkle, ref gecen, ref kalan);
            T("1♣: 2NT dengeli 9 HCP", nt, TestHelpers.ElOlustur("Q32", "K32", "J32", "K432"), new[] { "1♣", "1NT", "Pas" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("1♥: 2♦ = 4♠ davet+", nt, TestHelpers.ElOlustur("AJ32", "32", "K32", "5432"), new[] { "1♥", "1NT", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            T("1♥: zayıf 6♦ → 2♣ transfer", nt, TestHelpers.ElOlustur("32", "32", "KQ9432", "432"), new[] { "1♥", "1NT", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            T("1♥: zayıf 5♠ → 2♥ transfer", nt, TestHelpers.ElOlustur("KQ932", "32", "432", "432"), new[] { "1♥", "1NT", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("1♠: 2♠ = 4♥ davet+", nt, TestHelpers.ElOlustur("32", "AJ32", "K32", "5432"), new[] { "1♠", "1NT", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("1♠: zayıf 6♣ → 2♥ transfer", nt, TestHelpers.ElOlustur("32", "32", "432", "KJ9432"), new[] { "1♠", "1NT", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("1♠: zayıf 5♥ → 2♦ transfer", nt, TestHelpers.ElOlustur("32", "KQ932", "432", "432"), new[] { "1♠", "1NT", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            T("1♦: zayıf 6♣ → 2♠ transfer", nt, TestHelpers.ElOlustur("32", "32", "432", "KJ9432"), new[] { "1♦", "1NT", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("1♦: Stayman 2♣", nt, TestHelpers.ElOlustur("AJ32", "432", "K32", "432"), new[] { "1♦", "1NT", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── 1NT araya giriş: araya girenin cevabı ──");
            TK("1♣: 2♦ → 2♥", nt, TestHelpers.ElOlustur("K32", "AQ2", "KJ32", "Q32"), new[] { "1♣", "1NT", "Pas", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TK("1♣: 2♥ → 2♠", nt, TestHelpers.ElOlustur("K32", "AQ2", "KJ32", "Q32"), new[] { "1♣", "1NT", "Pas", "2♥", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TK("1♣: 2♠ → 3♦", nt, TestHelpers.ElOlustur("K32", "AQ2", "KJ32", "Q32"), new[] { "1♣", "1NT", "Pas", "2♠", "Pas" }, "3♦", sonucEkle, ref gecen, ref kalan);
            TK("1♣: Stayman, 4♥ → 2♥", nt, TestHelpers.ElOlustur("K32", "AQ32", "KJ2", "QJ2"), new[] { "1♣", "1NT", "Pas", "2♣", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TK("1♣: Stayman, majör yok → 2♦", nt, TestHelpers.ElOlustur("K32", "AQ2", "KJ32", "QJ2"), new[] { "1♣", "1NT", "Pas", "2♣", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TK("1♥: 2♣ → 2♦", nt, TestHelpers.ElOlustur("K32", "AQ2", "KJ32", "QJ2"), new[] { "1♥", "1NT", "Pas", "2♣", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TK("1♠: 2♥ → 3♣", nt, TestHelpers.ElOlustur("K32", "AQ2", "KJ32", "QJ2"), new[] { "1♠", "1NT", "Pas", "2♥", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            TK("Texas 4♦ → 4♥", nt, TestHelpers.ElOlustur("K32", "AQ2", "KJ32", "QJ2"), new[] { "1♣", "1NT", "Pas", "4♦", "Pas" }, "4♥", sonucEkle, ref gecen, ref kalan);
            TK("1♠: 2♠ cue, 4♥ + 18 HCP → 4♥", nt, TestHelpers.ElOlustur("K2", "AQ32", "KJ2", "KQ32"), new[] { "1♠", "1NT", "Pas", "2♠", "Pas" }, "4♥", sonucEkle, ref gecen, ref kalan);
            TK("1♠: 2♠ cue, 4♥ + 15 HCP → 3♥", nt, TestHelpers.ElOlustur("K2", "AQ32", "K32", "Q432"), new[] { "1♠", "1NT", "Pas", "2♠", "Pas" }, "3♥", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── 1NT araya giriş: cevapçının ikinci hamlesi ──");
            T("transfer sonrası 5♥ 10 HCP → 4♥", nt, TestHelpers.ElOlustur("32", "AQJ32", "K32", "432"), new[] { "1♣", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, "4♥", sonucEkle, ref gecen, ref kalan);
            T("transfer sonrası zayıf → Pas", nt, TestHelpers.ElOlustur("32", "KQ932", "432", "432"), new[] { "1♣", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, "Pas", sonucEkle, ref gecen, ref kalan);
            T("Stayman 2♠ sonrası 4♠ 8 HCP → 3♠", nt, TestHelpers.ElOlustur("AJ32", "432", "K32", "432"), new[] { "1♣", "1NT", "Pas", "2♣", "Pas", "2♠", "Pas" }, "3♠", sonucEkle, ref gecen, ref kalan);
            T("Stayman 2♠ sonrası 4♠ 12 HCP → 4♠", nt, TestHelpers.ElOlustur("AKQ2", "432", "K32", "432"), new[] { "1♣", "1NT", "Pas", "2♣", "Pas", "2♠", "Pas" }, "4♠", sonucEkle, ref gecen, ref kalan);
            T("Stayman 2♦ sonrası 8 HCP → 2NT", nt, TestHelpers.ElOlustur("AJ32", "432", "K32", "432"), new[] { "1♣", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, "2NT", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── 1NT araya giriş: araya girenin üçüncü hamlesi ──");
            TK("2NT daveti, 17 HCP → 3NT", nt, TestHelpers.ElOlustur("K32", "AQ2", "AJ32", "QJ2"), new[] { "1♦", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas", "2NT", "Pas" }, "3NT", sonucEkle, ref gecen, ref kalan);
            TK("2NT daveti, 16 HCP → Pas", nt, TestHelpers.ElOlustur("K32", "AQ2", "KJ32", "QJ2"), new[] { "1♦", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas", "2NT", "Pas" }, "Pas", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── T/O kontraya transferli cevaplar ──");
            var to = new Araya_TOCevap();
            T("1♣: zayıf 4♠ → 1♠", to, TestHelpers.ElOlustur("Q432", "432", "432", "432"), new[] { "1♣", "Dbl", "Pas" }, "1♠", sonucEkle, ref gecen, ref kalan);
            T("1♣: 4♥ 9 HCP → 2♦ transfer", to, TestHelpers.ElOlustur("K2", "AQ32", "432", "5432"), new[] { "1♣", "Dbl", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            T("1♣: 5♦ 10 HCP → 2♣ transfer", to, TestHelpers.ElOlustur("K432", "32", "AQJ32", "43"), new[] { "1♣", "Dbl", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            T("1♣: dengeli 10 HCP → 1NT", to, TestHelpers.ElOlustur("Q32", "K32", "Q432", "K32"), new[] { "1♣", "Dbl", "Pas" }, "1NT", sonucEkle, ref gecen, ref kalan);
            T("1♣: dengeli 12 HCP → 2NT", to, TestHelpers.ElOlustur("Q32", "Q32", "K32", "AJ32"), new[] { "1♣", "Dbl", "Pas" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("1♣: dengeli 13 HCP → 2♠ transfer cuebid", to, TestHelpers.ElOlustur("AK2", "K32", "Q32", "J432"), new[] { "1♣", "Dbl", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("1♣: 5♦ 4♥ FG → 3♣", to, TestHelpers.ElOlustur("32", "KJ32", "AKQ32", "43"), new[] { "1♣", "Dbl", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("1♦: 4♠ 8 HCP → 2♥ transfer", to, TestHelpers.ElOlustur("KQ32", "32", "432", "K432"), new[] { "1♦", "Dbl", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("1♥: 5♣ 8 HCP → 2♠ transfer", to, TestHelpers.ElOlustur("432", "32", "432", "AKJ32"), new[] { "1♥", "Dbl", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("1♠: 5♦ 8 HCP → 3♣ transfer", to, TestHelpers.ElOlustur("432", "32", "AKJ32", "432"), new[] { "1♠", "Dbl", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("1♠: zayıf 4♥ → 2♥", to, TestHelpers.ElOlustur("432", "Q432", "432", "432"), new[] { "1♠", "Dbl", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("Kontra yoksa devreye girmez", to, TestHelpers.ElOlustur("432", "Q432", "432", "432"), new[] { "1♠", "Pas", "Pas" }, null, sonucEkle, ref gecen, ref kalan);
            TK("1♣: 2♦ → 2♥", to, TestHelpers.ElOlustur("K32", "AQ32", "K32", "Q32"), new[] { "1♣", "Dbl", "Pas", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TK("1♣: 2♣ → 2♦", to, TestHelpers.ElOlustur("K32", "AQ32", "K32", "Q32"), new[] { "1♣", "Dbl", "Pas", "2♣", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TK("1♣: 2♠ (transfer cuebid) → 3♣", to, TestHelpers.ElOlustur("K32", "AQ32", "K32", "Q32"), new[] { "1♣", "Dbl", "Pas", "2♠", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            TK("1♠: 3♣ → 3♦", to, TestHelpers.ElOlustur("K32", "AQ32", "K32", "Q32"), new[] { "1♠", "Dbl", "Pas", "3♣", "Pas" }, "3♦", sonucEkle, ref gecen, ref kalan);
            TK("1♥: 2♠ → 3♣", to, TestHelpers.ElOlustur("K32", "AQ32", "K32", "Q32"), new[] { "1♥", "Dbl", "Pas", "2♠", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("transfer sonrası 15 HCP → 4♥", to, TestHelpers.ElOlustur("AK2", "AJ32", "K32", "432"), new[] { "1♣", "Dbl", "Pas", "2♦", "Pas", "2♥", "Pas" }, "4♥", sonucEkle, ref gecen, ref kalan);
            T("transfer sonrası 11 HCP → 3♥", to, TestHelpers.ElOlustur("AK2", "KJ32", "432", "432"), new[] { "1♣", "Dbl", "Pas", "2♦", "Pas", "2♥", "Pas" }, "3♥", sonucEkle, ref gecen, ref kalan);
            T("transfer sonrası 8 HCP → Pas", to, TestHelpers.ElOlustur("K32", "KQ32", "432", "432"), new[] { "1♣", "Dbl", "Pas", "2♦", "Pas", "2♥", "Pas" }, "Pas", sonucEkle, ref gecen, ref kalan);
            var tor = new Araya_TORubensohl();
            var bal = new Araya_Balans();
            sonucEkle("── Balancing kontra cevapları (+2 HCP) ──");
            T("balancing: 10 HCP 4♥ → 2♦ transfer", to, TestHelpers.ElOlustur("432", "AQ52", "432", "KJ2"), new[] { "1♣", "Pas", "Pas", "Dbl", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            T("balancing: 9 HCP 4♥ → 1♥ (normalde transfer)", to, TestHelpers.ElOlustur("K32", "AQ52", "432", "432"), new[] { "1♣", "Pas", "Pas", "Dbl", "Pas" }, "1♥", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Açanın ortağı konuşunca (Rubensohl) ──");
            T("zayıf 4♠ → 1♠", tor, TestHelpers.ElOlustur("Q432", "432", "432", "432"), new[] { "1♣", "Dbl", "1♥" }, "1♠", sonucEkle, ref gecen, ref kalan);
            T("5 HCP 4♠ (1♦-Dbl-2♣) → 2♠", tor, TestHelpers.ElOlustur("Q432", "432", "432", "432"), new[] { "1♦", "Dbl", "2♣" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("9 HCP 5♠ → 3♥ transfer", tor, TestHelpers.ElOlustur("AQ432", "32", "432", "K32"), new[] { "1♣", "Dbl", "1♥" }, "3♥", sonucEkle, ref gecen, ref kalan);
            T("R'de 4 kart → ceza kontrası", tor, TestHelpers.ElOlustur("432", "KJ52", "K32", "432"), new[] { "1♣", "Dbl", "1♥" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            T("5♦ 9 HCP → 3♣ transfer", tor, TestHelpers.ElOlustur("432", "32", "AQ432", "K32"), new[] { "1♣", "Dbl", "1♥" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("iki açılmamış majör 4-4 → ekonomik cue 2♣", tor, TestHelpers.ElOlustur("KJ32", "Q432", "32", "432"), new[] { "1♣", "Dbl", "1♦" }, "2♣", sonucEkle, ref gecen, ref kalan);
            T("1NT 8 HCP dengeli", tor, TestHelpers.ElOlustur("432", "J32", "KJ2", "K432"), new[] { "1♣", "Dbl", "1♦" }, "1NT", sonucEkle, ref gecen, ref kalan);
            TK("kontra yapan: 3♥ transferi → 3♠", tor, TestHelpers.ElOlustur("K32", "AQ32", "K32", "Q32"), new[] { "1♣", "Dbl", "1♥", "3♥", "Pas" }, "3♠", sonucEkle, ref gecen, ref kalan);
            T("transfer sonrası 16 HCP → 4♠", tor, TestHelpers.ElOlustur("AKQ32", "32", "K32", "A32"), new[] { "1♣", "Dbl", "1♥", "3♥", "Pas", "3♠", "Pas" }, "4♠", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Uyandırma (balancing) ──");
            T("dengeli 12 HCP → 1NT", bal, TestHelpers.ElOlustur("AQ32", "K32", "Q32", "J32"), new[] { "1♥", "Pas", "Pas" }, "1NT", sonucEkle, ref gecen, ref kalan);
            T("dengeli 15 HCP minör açılış → Dbl", bal, TestHelpers.ElOlustur("AQ32", "KJ3", "AQ2", "432"), new[] { "1♦", "Pas", "Pas" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            T("4-4-4-1 9 HCP → Dbl", bal, TestHelpers.ElOlustur("KJ32", "Q432", "K432", "2"), new[] { "1♣", "Pas", "Pas" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            T("5♠ 9 HCP → 1♠", bal, TestHelpers.ElOlustur("AQ432", "32", "432", "K32"), new[] { "1♣", "Pas", "Pas" }, "1♠", sonucEkle, ref gecen, ref kalan);
            T("5♦5♥ 11 HCP → cue 2♣", bal, TestHelpers.ElOlustur("32", "AKJ32", "QJ432", "2"), new[] { "1♣", "Pas", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            T("dengeli 19 HCP → 2NT", bal, TestHelpers.ElOlustur("AK2", "AQ2", "KJ32", "Q32"), new[] { "1♦", "Pas", "Pas" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("6♠ 11 HCP → 2♠ atlamalı", bal, TestHelpers.ElOlustur("AKJ932", "32", "432", "K2"), new[] { "1♦", "Pas", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("7♥ 6 HCP → 3♥", bal, TestHelpers.ElOlustur("432", "KQJ9432", "32", "2"), new[] { "1♣", "Pas", "Pas" }, "3♥", sonucEkle, ref gecen, ref kalan);
            T("rakip açmadıysa devreye girmez", bal, TestHelpers.ElOlustur("432", "KQJ9432", "32", "2"), new[] { "Pas", "Pas", "Pas" }, null, sonucEkle, ref gecen, ref kalan);
            T("Pas-Pas yoksa devreye girmez", bal, TestHelpers.ElOlustur("432", "KQJ9432", "32", "2"), new[] { "1♣", "Pas" }, null, sonucEkle, ref gecen, ref kalan);
            sonucEkle("");
            sonucEkle("── 2NT Lebensohl (s.93-94) ──");
            var leb = new Araya_Leben2NT();
            string[] h2 = { "2♥", "Dbl", "Pas" };
            T("Leb: 0-7 4♠ → 2♠", leb, TestHelpers.ElOlustur("KJ32", "432", "432", "432"), h2, "2♠", sonucEkle, ref gecen, ref kalan);
            T("Leb: 8-10 5♠ → 3♠ atlamalı", leb, TestHelpers.ElOlustur("AQ932", "32", "K32", "432"), h2, "3♠", sonucEkle, ref gecen, ref kalan);
            T("Leb: 10 HCP renk yok → 2NT davet", leb, TestHelpers.ElOlustur("K32", "432", "AQ32", "J32"), h2, "2NT", sonucEkle, ref gecen, ref kalan);
            T("Leb: 13 HCP dengeli ♥ stoperli → 3NT", leb, TestHelpers.ElOlustur("K32", "KQ2", "AJ32", "432"), h2, "3NT", sonucEkle, ref gecen, ref kalan);
            T("Leb: FG stopersiz → cuebid 3♥", leb, TestHelpers.ElOlustur("AK32", "432", "AQ2", "K32"), h2, "3♥", sonucEkle, ref gecen, ref kalan);
            T("Leb: zayıf 6♣ → 2NT", leb, TestHelpers.ElOlustur("32", "32", "432", "KJ9432"), h2, "2NT", sonucEkle, ref gecen, ref kalan);
            T("Leb: 0-7 4♦ → 3♦", leb, TestHelpers.ElOlustur("32", "432", "KJ98", "5432"), h2, "3♦", sonucEkle, ref gecen, ref kalan);
            T("Leb: (1♥)-Dbl-(2♥) 0-7 4♠ → 2♠", leb, TestHelpers.ElOlustur("KJ32", "432", "432", "432"), new[] { "1♥", "Dbl", "2♥" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("Leb: (1♣)-Dbl-(Pas) bağlam dışı → devreye girmez", leb, TestHelpers.ElOlustur("KJ32", "432", "432", "432"), new[] { "1♣", "Dbl", "Pas" }, null, sonucEkle, ref gecen, ref kalan);
            string[] h2n = { "2♥", "Dbl", "Pas", "2NT", "Pas" };
            T("Leb dbl: minimum → 3♣", leb, TestHelpers.ElOlustur("AQ32", "3", "KJ32", "Q432"), h2n, "3♣", sonucEkle, ref gecen, ref kalan);
            T("Leb dbl: max 5♠ → 3♠", leb, TestHelpers.ElOlustur("AKJ32", "3", "AQ32", "K32"), h2n, "3♠", sonucEkle, ref gecen, ref kalan);
            T("Leb dbl: max 5♦ → 3♦", leb, TestHelpers.ElOlustur("AK32", "3", "AQJ32", "K32"), h2n, "3♦", sonucEkle, ref gecen, ref kalan);
            T("Leb dbl: max dengeli stoperli → 3NT", leb, TestHelpers.ElOlustur("AK32", "KQ2", "Q32", "KJ2"), h2n, "3NT", sonucEkle, ref gecen, ref kalan);
            string[] h3c = { "2♥", "Dbl", "Pas", "2NT", "Pas", "3♣", "Pas" };
            T("Leb 2.: 3♣ sonrası zayıf ♣ → Pas", leb, TestHelpers.ElOlustur("32", "32", "432", "KJ9432"), h3c, "Pas", sonucEkle, ref gecen, ref kalan);
            T("Leb 2.: 3♣ sonrası 5♦ davet → 3♦", leb, TestHelpers.ElOlustur("K32", "32", "AQ932", "432"), h3c, "3♦", sonucEkle, ref gecen, ref kalan);
            T("Leb 2.: 3♣ sonrası 4♠ 9 HCP → 3♠", leb, TestHelpers.ElOlustur("KJ32", "32", "Q432", "K32"), h3c, "3♠", sonucEkle, ref gecen, ref kalan);
            T("Leb 2.: 3♣ sonrası 4♠ 10 HCP → 3♥ cuebid", leb, TestHelpers.ElOlustur("KJ32", "32", "QJ32", "K43"), h3c, "3♥", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── 2NT Goodbad (s.95-96) ──");
            var gb = new Araya_GoodBad();
            string[] gA = { "1♦", "Pas", "1♠", "2♥" };
            T("GB-A: minimum → Pas", gb, TestHelpers.ElOlustur("32", "432", "KJ32", "AQ32"), gA, "Pas", sonucEkle, ref gecen, ref kalan);
            T("GB-A: 17 HCP düz → Dbl", gb, TestHelpers.ElOlustur("32", "A32", "AKQ2", "KJ32"), gA, "Dbl", sonucEkle, ref gecen, ref kalan);
            T("GB-A: 4♠ 11 HCP → 2♠", gb, TestHelpers.ElOlustur("KQ32", "32", "AQ32", "432"), gA, "2♠", sonucEkle, ref gecen, ref kalan);
            T("GB-A: 4♠ 17 HCP → 3♠", gb, TestHelpers.ElOlustur("KQ32", "A2", "AKJ2", "432"), gA, "3♠", sonucEkle, ref gecen, ref kalan);
            T("GB-A: 5♦5♣ 10 HCP → 2NT", gb, TestHelpers.ElOlustur("32", "3", "KQ932", "AJ432"), gA, "2NT", sonucEkle, ref gecen, ref kalan);
            T("GB-A: 5♦5♣ 17 HCP → 3♣", gb, TestHelpers.ElOlustur("A", "32", "AKJ32", "KQ432"), gA, "3♣", sonucEkle, ref gecen, ref kalan);
            T("GB-A: 6♦ zayıf → 3♦", gb, TestHelpers.ElOlustur("32", "32", "KQJ932", "432"), gA, "3♦", sonucEkle, ref gecen, ref kalan);
            T("GB-A: 6♦ 16 HCP → 2NT", gb, TestHelpers.ElOlustur("K2", "A2", "AKQ932", "432"), gA, "2NT", sonucEkle, ref gecen, ref kalan);
            T("GB-A: 6♦ 20 HCP ♥ stopersiz → 3♥ cuebid", gb, TestHelpers.ElOlustur("AK", "32", "AKQJ32", "K32"), gA, "3♥", sonucEkle, ref gecen, ref kalan);
            T("GB-A: 6♦ 21 HCP ♥ stoperli → 2NT", gb, TestHelpers.ElOlustur("AK", "KQ2", "AKQ932", "32"), gA, "2NT", sonucEkle, ref gecen, ref kalan);
            string[] gA2 = { "1♦", "Pas", "1♠", "2♥", "2NT", "Pas" };
            T("GB-A cevapçı: 13 HCP → 3♥ cuebid FG", gb, TestHelpers.ElOlustur("AKJ32", "32", "Q32", "K32"), gA2, "3♥", sonucEkle, ref gecen, ref kalan);
            T("GB-A cevapçı: 6♠ 10 HCP → 3♠", gb, TestHelpers.ElOlustur("AQJ932", "32", "K32", "32"), gA2, "3♠", sonucEkle, ref gecen, ref kalan);
            T("GB-A cevapçı: zayıf ♣ ağırlıklı → 3♣", gb, TestHelpers.ElOlustur("KJ32", "32", "432", "J432"), gA2, "3♣", sonucEkle, ref gecen, ref kalan);
            T("GB-A cevapçı: zayıf ♦ ağırlıklı → 3♦", gb, TestHelpers.ElOlustur("KJ32", "32", "J432", "432"), gA2, "3♦", sonucEkle, ref gecen, ref kalan);
            T("GB-A cevapçı: 10 HCP → 3♣ tercih", gb, TestHelpers.ElOlustur("KJ32", "K32", "Q32", "J32"), gA2, "3♣", sonucEkle, ref gecen, ref kalan);
            string[] gA3 = { "1♦", "Pas", "1♠", "2♥", "2NT", "Pas", "3♣", "Pas" };
            T("GB-A açan 2.: 5♦5♣ rekabetçi → Pas", gb, TestHelpers.ElOlustur("32", "3", "KQ932", "AJ432"), gA3, "Pas", sonucEkle, ref gecen, ref kalan);
            T("GB-A açan 2.: 6♦ 16 → 3♦", gb, TestHelpers.ElOlustur("K2", "A2", "AKQ932", "432"), gA3, "3♦", sonucEkle, ref gecen, ref kalan);
            T("GB-A açan 2.: 5♦5♣ 20 stoperli → 3♥", gb, TestHelpers.ElOlustur("3", "AK", "AKJ32", "KQ432"), gA3, "3♥", sonucEkle, ref gecen, ref kalan);
            T("GB-A açan 2.: 6♦ 21 stoperli → 3NT", gb, TestHelpers.ElOlustur("AK", "KQ2", "AKQ932", "32"), gA3, "3NT", sonucEkle, ref gecen, ref kalan);
            T("GB-A açan 2.: 5♦ 4♠ 20 HCP → 3♠", gb, TestHelpers.ElOlustur("AKQ2", "3", "AKJ32", "K32"), gA3, "3♠", sonucEkle, ref gecen, ref kalan);
            string[] gB = { "1♦", "1♠", "Dbl", "2♠" };
            T("GB-B: 4♥ 4♦ 14 HCP → 3♥", gb, TestHelpers.ElOlustur("32", "AQ32", "AJ32", "K32"), gB, "3♥", sonucEkle, ref gecen, ref kalan);
            T("GB-B: 4♥ 4♦ 17 HCP → 2NT", gb, TestHelpers.ElOlustur("2", "AKQ2", "AJ32", "K432"), gB, "2NT", sonucEkle, ref gecen, ref kalan);
            T("GB-B: 5♦5♣ 17 HCP → 3♣", gb, TestHelpers.ElOlustur("A", "32", "AKJ32", "KQ432"), gB, "3♣", sonucEkle, ref gecen, ref kalan);
            string[] gB2 = { "1♦", "1♠", "Dbl", "2♠", "2NT", "Pas" };
            T("GB-B cevapçı: 13 HCP → 3♠ cuebid", gb, TestHelpers.ElOlustur("32", "AKJ2", "Q32", "K432"), gB2, "3♠", sonucEkle, ref gecen, ref kalan);
            T("GB-B cevapçı: 6♥ 6 HCP → 3♥", gb, TestHelpers.ElOlustur("32", "KQJ932", "432", "32"), gB2, "3♥", sonucEkle, ref gecen, ref kalan);
            string[] gB3 = { "1♦", "1♠", "Dbl", "2♠", "2NT", "Pas", "3♣", "Pas" };
            T("GB-B açan 2.: 5♦5♣ 20 ♠ stoperli → 3♠", gb, TestHelpers.ElOlustur("AK", "3", "AKJ32", "KQ432"), gB3, "3♠", sonucEkle, ref gecen, ref kalan);
            T("GB-B açan 2.: 4♥ 4♦ 17 → 3♥", gb, TestHelpers.ElOlustur("2", "AKQ2", "AJ32", "K432"), gB3, "3♥", sonucEkle, ref gecen, ref kalan);
            T("GB: bağlam dışı (1♦-Pas-1♠-2♣) → devreye girmez", gb, TestHelpers.ElOlustur("32", "432", "KJ32", "AQ32"), new[] { "1♦", "Pas", "1♠", "2♣" }, null, sonucEkle, ref gecen, ref kalan);
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        // Rakip Batı: gecmis[0]; ortak Kuzey; ben Güney. Sıra: Batı, Kuzey, Doğu, Güney → [W 1♣, N oc, E Pas] -> Güney
        private static void T(string ad, IAcilisGelisimi motor, List<Card> el, string[] gecmis, string beklenen,
            Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            bool uygun = motor.UygunMu(el, durum);
            string sonuc = uygun ? motor.TeklifVer(el, durum) : null;
            if (sonuc == beklenen) { sonucEkle($"✅ DOĞRU: {sonuc ?? "(devreye girmedi)"}"); gecen++; }
            else { sonucEkle($"❌ HATA: Beklenen {beklenen ?? "(devreye girmez)"}, gelen {sonuc ?? "(devreye girmedi)"}"); kalan++; }
        }

        // Transfer kabulü: Batı 1♣, Kuzey araya girdi, Doğu Pas, Güney transfer, Batı Pas → Kuzey (aktif)
        private static void TK(string ad, IAcilisGelisimi motor, List<Card> el, string[] gecmis, string beklenen,
            Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, Player.Kuzey, gecmis);
            bool uygun = motor.UygunMu(el, durum);
            string sonuc = uygun ? motor.TeklifVer(el, durum) : null;
            if (sonuc == beklenen) { sonucEkle($"✅ DOĞRU: {sonuc ?? "(devreye girmedi)"}"); gecen++; }
            else { sonucEkle($"❌ HATA: Beklenen {beklenen ?? "(devreye girmez)"}, gelen {sonuc ?? "(devreye girmedi)"}"); kalan++; }
        }
    }
}
