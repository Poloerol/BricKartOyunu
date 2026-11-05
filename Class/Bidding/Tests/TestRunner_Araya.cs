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
            sonucEkle("  ARAYA GİRİŞE CEVAPLAR (1♣/1♦/1♥/1♠ açılışları)");
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
            sonucEkle("");
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
