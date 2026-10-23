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

            // ════════════════════════════════════════════════════════════════
            // TABLO 1: AÇICI İLK REBİD
            // ════════════════════════════════════════════════════════════════
            sonucEkle("── TABLO 1: Açıcı İlk Rebid ──");

            Test("T1.1a — 4♣ 4♥ 11-14 HCP → 1♥ (Güney açtı)",
                TestHelpers.ElOlustur("QJ", "QJ32", "Q32", "KJ32"),
                new[] { "1♣", "Pas", "1♦", "Pas" }, Player.Guney, "1♥",
                sonucEkle, ref gecen, ref kalan);

            Test("T1.1b — Aynı el, Batı açtı → 1♥ (Perspektif testi)",
                TestHelpers.ElOlustur("QJ", "QJ32", "Q32", "KJ32"),
                new[] { "1♣", "Pas", "1♦", "Pas" }, Player.Bati, "1♥",
                sonucEkle, ref gecen, ref kalan);

            Test("T1.2a — 4♣ 4♥ 15-17 HCP → 1♥ (Güney açtı)",
                TestHelpers.ElOlustur("QJ", "KQ32", "Q32", "AK32"),
                new[] { "1♣", "Pas", "1♦", "Pas" }, Player.Guney, "1♥",
                sonucEkle, ref gecen, ref kalan);

            Test("T1.2b — Aynı el, Doğu açtı → 1♥ (Perspektif testi)",
                TestHelpers.ElOlustur("QJ", "KQ32", "Q32", "AK32"),
                new[] { "1♣", "Pas", "1♦", "Pas" }, Player.Dogu, "1♥",
                sonucEkle, ref gecen, ref kalan);

            // ════════════════════════════════════════════════════════════════
            // TABLO 2: CEVAPÇI İLK DEVAM
            // ════════════════════════════════════════════════════════════════
            sonucEkle("");
            sonucEkle("── TABLO 2: Cevapçı İlk Devam ──");

            Test("T2.1a — Açıcı 1♥ → Cevapçı 2♦ (6♦ 9-11) (Güney açtı, Kuzey cevapçı)",
                TestHelpers.ElOlustur("32", "K32", "KQJ432", "32"),
                new[] { "1♣", "Pas", "1♦", "Pas", "1♥", "Pas" }, Player.Kuzey, "2♦",
                sonucEkle, ref gecen, ref kalan);

            Test("T2.1b — Aynı el, Batı açtı, Doğu cevapçı → 2♦ (Perspektif testi)",
                TestHelpers.ElOlustur("32", "K32", "KQJ432", "32"),
                new[] { "1♣", "Pas", "1♦", "Pas", "1♥", "Pas" }, Player.Dogu, "2♦",
                sonucEkle, ref gecen, ref kalan);

            // ════════════════════════════════════════════════════════════════
            // TABLO 2: RÖLE CEVAPLARI
            // ════════════════════════════════════════════════════════════════
            sonucEkle("");
            sonucEkle("── TABLO 2: Röle Cevapları ──");

            Test("T3.1a — Açıcı 2♦ röle sonrası 5431 14-16 → 2NT (Güney açtı)",
                TestHelpers.ElOlustur("2", "KQ32", "KQ3", "KQJ32"),
                new[] { "1♣", "Pas", "1♦", "Pas", "2♦", "Pas", "2♠", "Pas" },
                Player.Guney, "2NT", sonucEkle, ref gecen, ref kalan);

            Test("T3.1b — Aynı el, Batı açtı → 2NT (Perspektif testi)",
                TestHelpers.ElOlustur("2", "KQ32", "KQ3", "KQJ32"),
                new[] { "1♣", "Pas", "1♦", "Pas", "2♦", "Pas", "2♠", "Pas" },
                Player.Bati, "2NT", sonucEkle, ref gecen, ref kalan);

            // ════════════════════════════════════════════════════════════════
            // DÜNKÜ SENARYO TESTİ: Kuzey 1♣ açtı, Güney 1♥ dedi, sıra Kuzey'de
            // ════════════════════════════════════════════════════════════════
            sonucEkle("");
            sonucEkle("── SENARYO TESTİ: 1♣ - 1♥ - ? (Kuzey'in Sırası) ──");

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
            if (sonucSenaryo == "2♥")
            {
                sonucEkle("✅ DOĞRU: Kupa fiti tanındı, 2♥ döndü!");
                gecen++;
            }
            else
            {
                sonucEkle($"❌ HATA: Beklenen 2♥, gelen {sonucSenaryo}");
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