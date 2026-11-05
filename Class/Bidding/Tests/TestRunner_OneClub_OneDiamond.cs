using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_OneClub_OneDiamond
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  1♣ ⇒ 1♦ GELİŞİM TESTLERİ (Çoklu Perspektif)");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            sonucEkle("── AÇANIN YENİDEN TEKLİFİ ──");
            Test("1♥ — 4♣ 4♥ 12 HCP (Güney açtı)",
                TestHelpers.ElOlustur("Q", "QJ32", "Q432", "KJ32"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Guney, "1♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ — aynı el, Batı açtı (perspektif)",
                TestHelpers.ElOlustur("Q", "QJ32", "Q432", "KJ32"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Bati, "1♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ — 4♣ 4♥ 17 HCP",
                TestHelpers.ElOlustur("Q", "KQ32", "Q432", "AK32"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Dogu, "1♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♠ — 4♣ 4♠ 12 HCP",
                TestHelpers.ElOlustur("KQ32", "2", "Q32", "AJ432"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Guney, "1♠",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦ — 4♣ 4♦ 11-14",
                TestHelpers.ElOlustur("K32", "Q2", "KJ32", "AQ32"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ — 6♣ 11-16",
                TestHelpers.ElOlustur("32", "K2", "Q32", "AKJ843"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Guney, "2♣",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ — 5♣ 4♥ 18+",
                TestHelpers.ElOlustur("A2", "KQJ3", "A2", "AKJ32"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Guney, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2NT — 18+ 6♣",
                TestHelpers.ElOlustur("AK", "K2", "432", "AKQ432"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3♣ — 6♣ 15-17",
                TestHelpers.ElOlustur("K2", "32", "K32", "AKQJ32"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Guney, "3♣",
                sonucEkle, ref gecen, ref kalan);

            Test("3♦ — 5♣ 4♦ 15-17",
                TestHelpers.ElOlustur("K2", "32", "AQ32", "AKJ32"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("3♥ — 4♣ 4♦ 4♠ 1♥ 18-21",
                TestHelpers.ElOlustur("AK32", "2", "AQ32", "AK32"),
                new[] { "1♣","Pas","1♦","Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("");
            sonucEkle("── CEVAPÇININ DEVAMI ──");
            Test("1♥ sonrası 2♦ — 6♦ 9 HCP",
                TestHelpers.ElOlustur("32", "K32", "KQJ432", "32"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas" }, Player.Kuzey, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ sonrası 2♦ — aynı el, Batı açtı (perspektif)",
                TestHelpers.ElOlustur("32", "K32", "KQJ432", "32"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas" }, Player.Dogu, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ sonrası 2♠ — 5♦ 4♣ 12 HCP (4. renk SI)",
                TestHelpers.ElOlustur("32", "32", "KQ432", "AQJ4"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas" }, Player.Kuzey, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ sonrası 2♥ — 4♦ 4♥ 10 HCP davet",
                TestHelpers.ElOlustur("32", "KJ32", "KJ32", "Q32"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas" }, Player.Kuzey, "2♥",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ sonrası 1♠ — 4♦ 4♠ 9 HCP F1R",
                TestHelpers.ElOlustur("KQ32", "32", "KJ32", "432"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas" }, Player.Kuzey, "1♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥ sonrası Pas — zayıf el",
                TestHelpers.ElOlustur("32", "432", "J432", "Q432"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            Test("2♣ sonrası 3♥ — 3+♣ ♥ tek FG",
                TestHelpers.ElOlustur("KQ32", "5", "Q32", "AJ932"),
                new[] { "1♣","Pas","1♦","Pas","2♣","Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            Test("2♥ sonrası 2NT — Lebensohl (zayıf)",
                TestHelpers.ElOlustur("5432", "432", "32", "Q432"),
                new[] { "1♣","Pas","1♦","Pas","2♥","Pas" }, Player.Kuzey, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("3NT sonrası 4♣ — ♥ transfer",
                TestHelpers.ElOlustur("432", "KJ432", "32", "Q32"),
                new[] { "1♣","Pas","1♦","Pas","3NT","Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("");
            sonucEkle("── AÇAN RÖLE CEVAPLARI / 1♥-1♠ DEVAMI ──");
            Test("1♥-1♠ → 2♦ — 5♣4♥3♦1♠ min",
                TestHelpers.ElOlustur("2", "KQ32", "Q43", "KQJ32"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas","1♠","Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥-1♠ → 2NT — 5♣4♥ 17 HCP",
                TestHelpers.ElOlustur("Q2", "KQ32", "K2", "AQJ32"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas","1♠","Pas" }, Player.Guney, "2NT",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥-2♣ → 2♦ — 3♦ min",
                TestHelpers.ElOlustur("2", "KJ32", "Q32", "AKJ32"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas","2♣","Pas" }, Player.Guney, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥-2♣-2♦-2♥ → 2♠ — dengeli 12 HCP",
                TestHelpers.ElOlustur("J2", "KQ32", "Q32", "KJ32"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas","2♣","Pas","2♦","Pas","2♥","Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            Test("1♥-1♠-2NT-3♣ → 3♦ — 5422",
                TestHelpers.ElOlustur("Q2", "KQ32", "K2", "AQJ32"),
                new[] { "1♣","Pas","1♦","Pas","1♥","Pas","1♠","Pas","2NT","Pas","3♣","Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            Test("2♦-2♥ röle → 2♠ — 5♣4♦ min",
                TestHelpers.ElOlustur("K2", "32", "KQ32", "QJ432"),
                new[] { "1♣","Pas","1♦","Pas","2♦","Pas","2♥","Pas" }, Player.Guney, "2♠",
                sonucEkle, ref gecen, ref kalan);

            // Kitapta "gelişim natürel" olan 3♣ yeniden teklifinde sınıf devreye girmemeli
            {
                var el = TestHelpers.ElOlustur("32", "432", "J432", "Q432");
                var d = TestHelpers.DurumOlustur(el, Player.Kuzey, "1♣", "Pas", "1♦", "Pas", "3♣", "Pas");
                sonucEkle("▶ 3♣ yeniden teklifi: sınıf devreye girmez");
                if (!new OneClub_OneDiamond().UygunMu(el, d)) { sonucEkle("✅ DOĞRU: UygunMu false"); gecen++; }
                else { sonucEkle("❌ HATA: UygunMu true döndü"); kalan++; }
            }

            // ════════════════════════════════════════════════════════════════
            // SENARYO TESTİ: 1♣ - 1♥ - ? (16 HP + Fit → 3♥ Davet)
            // ════════════════════════════════════════════════════════════════
            sonucEkle("");
            sonucEkle("── SENARYO TESTİ: 1♣ - 1♥ - ? (16 HP + Fit → 3♥ Davet) ──");

            var elKuzey = TestHelpers.ElOlustur("Q2", "AJT9", "72", "AKQT5");
            sonucEkle($"Kuzey'in Eli: HCP={ElDegerlendirici.HCP(elKuzey)}, " +
                      $"♠={ElDegerlendirici.RenkUzunlugu(elKuzey, "Maça")}, " +
                      $"♥={ElDegerlendirici.RenkUzunlugu(elKuzey, "Kupa")}, " +
                      $"♦={ElDegerlendirici.RenkUzunlugu(elKuzey, "Karo")}, " +
                      $"♣={ElDegerlendirici.RenkUzunlugu(elKuzey, "Sinek")}");

            var durumSenaryo = TestHelpers.DurumOlustur(elKuzey, Player.Kuzey, "1♣", "Pas", "1♥", "Pas");
            sonucEkle($"Geçmiş: 1♣ (Kuzey) - Pas (Doğu) - 1♥ (Güney) - Pas (Batı)");
            sonucEkle($"Aktif: {durumSenaryo.AktifOyuncu}, Partner: {durumSenaryo.Partner}");

            var motorSenaryo = new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan());
            string sonucSenaryo = motorSenaryo.TeklifVer(durumSenaryo);

            sonucEkle($"Motor Sonucu: {sonucSenaryo}");
            if (sonucSenaryo == "3♥")
            {
                sonucEkle("✅ DOĞRU: Kupa fiti tanındı, 3♥ (Davet) döndü!");
                gecen++;
            }
            else
            {
                sonucEkle($"❌ HATA: Beklenen 3♥, gelen {sonucSenaryo}");
                kalan++;
            }

            // ────────────────────────────────────────────────────────────

            sonucEkle("");
            sonucEkle("════════════════════════════════════════");
            sonucEkle($"  ÖZET: ✅ {gecen} geçti, ❌ {kalan} başarısız");
            sonucEkle("════════════════════════════════════════");
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
            var motor = new OneClub_OneDiamond();

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