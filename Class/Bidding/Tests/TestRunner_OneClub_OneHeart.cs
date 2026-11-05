using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_OneClub_OneHeart
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1♣ ⇒ 1♥ GELİŞİM TESTLERİ (Açanın yeniden teklifi)");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            Test("1♠ — 4♠ 5♣ 14 HCP",
                TestHelpers.ElOlustur("KQ32", "Q2", "J2", "AQ843"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "1♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1NT — dengeli 13 HCP, 4♣ 4♦ 2♥ 4♠ yok 1♠",
                TestHelpers.ElOlustur("KJ32", "Q2", "AJ32", "Q32"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "1NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — 7♣ 13 HCP",
                TestHelpers.ElOlustur("2", "Q2", "K32", "AKJ9643"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 5♣ 4♦ 17 HCP",
                TestHelpers.ElOlustur("K2", "32", "AQ32", "AKJ43"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 3♥ 14 HCP",
                TestHelpers.ElOlustur("Q2", "K32", "KJ32", "AJ32"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 4♥ 4♣ 13 HCP (14'ün altı)",
                TestHelpers.ElOlustur("Q2", "KJ32", "K32", "A432"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 4♥ 5♣ 14 HCP",
                TestHelpers.ElOlustur("2", "KQ32", "J32", "AKJ32"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 6♣ 16 HCP",
                TestHelpers.ElOlustur("K2", "32", "K32", "AKQJ32"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 7♣ 20 HCP",
                TestHelpers.ElOlustur("K2", "A2", "K3", "AKQJ432"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("2♠ — 5♣ 4♠ 20 HCP",
                TestHelpers.ElOlustur("AKQ2", "2", "K32", "AKJ32"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT — 5♣ 4♥ 2-2 20 HCP",
                TestHelpers.ElOlustur("K2", "KQ32", "A2", "AKJ32"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 4-4-4-1 (♦ tek) 18 HCP",
                TestHelpers.ElOlustur("KQ32", "AJ32", "2", "AKJ3"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♠ — 4♣4♥4♦ 1♠ 19 HCP",
                TestHelpers.ElOlustur("2", "AQ32", "KJ32", "AKQ3"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            Test("4♣ — 6♣4♥ ♦ singleton, ♣'da A ve K, 15 HCP",
                TestHelpers.ElOlustur("Q2", "KJ32", "2", "AKQ643"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            Test("4♦ — 6♣4♥ ♠ singleton, ♣'da A ve K, 15 HCP",
                TestHelpers.ElOlustur("2", "KJ32", "Q2", "AKQ643"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            Test("4♥ — 6♣4♥ ♣'da yalnız K, 16 HCP",
                TestHelpers.ElOlustur("AKJ", "KQ32", "", "K96432"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            // Perspektif: aynı el Batı'dan açılınca aynı sonuç
            Test("Perspektif — 3♥ (Batı açtı)",
                TestHelpers.ElOlustur("2", "KQ32", "J32", "AKJ32"),
                new[] { "1♣", "Pas", "1♥", "Pas" }, Player.Bati, "3♥",
                sonucEkle, ref gecen, ref kalan);

            // Rakip araya girdiyse bu gelişim devreye girmez
            sonucEkle("▶ Rakip 1♠ dedi → UygunMu false olmalı");
            var el = TestHelpers.ElOlustur("KQ32", "Q2", "J2", "AQ843");
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, "1♣", "1♠", "1♥", "Pas");
            if (!new OneClub_OneHeart().UygunMu(el, durum)) { sonucEkle("✅ DOĞRU"); gecen++; }
            else { sonucEkle("❌ HATA: Rakip müdahalesine rağmen UygunMu true."); kalan++; }

            sonucEkle("");
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        private static void Test(
            string ad,
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
            var motor = new OneClub_OneHeart();

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
