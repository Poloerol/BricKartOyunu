using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.Role2C;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_Role2C
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  2♣ RÖLE / 2NT RÖLE TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");
            int gecen = 0, kalan = 0;
            var m = new Role2C_Sistem();
            // 1♦ - 1♥ - 1♠ : açan Güney, cevapçı Kuzey
            string[] h3 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas" };
            sonucEkle("── Cevapçı: 2♣ röle / 2NT röle ──");
            T("2♣ röle (12 HCP)", m, TestHelpers.ElOlustur("K32", "AQ432", "Q3", "K32"), Player.Kuzey, h3, "2♣", sonucEkle, ref gecen, ref kalan);
            T("2NT röle (6♣ zayıf)", m, TestHelpers.ElOlustur("32", "432", "32", "KJ9432"), Player.Kuzey, h3, "2NT", sonucEkle, ref gecen, ref kalan);
            T("zayıf el: devreye girmez (null)", m, TestHelpers.ElOlustur("432", "Q432", "Q32", "432"), Player.Kuzey, h3, null, sonucEkle, ref gecen, ref kalan);
            T("♠ desteği (4♠) röle değil", m, TestHelpers.ElOlustur("KJ32", "AQ43", "Q32", "32"), Player.Kuzey, h3, null, sonucEkle, ref gecen, ref kalan);

            string[] h4 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2♣", "Pas" };
            sonucEkle("── Açan: röle cevapları ──");
            T("2♦ — ♥'de 3'lü tutuş", m, TestHelpers.ElOlustur("AKJ2", "Q32", "K432", "32"), Player.Guney, h4, "2♦", sonucEkle, ref gecen, ref kalan);
            T("2♥ — min, tutuş yok", m, TestHelpers.ElOlustur("AKJ2", "32", "K432", "J32"), Player.Guney, h4, "2♥", sonucEkle, ref gecen, ref kalan);
            T("2♠ — max dengesiz", m, TestHelpers.ElOlustur("AKJ2", "3", "AKQ32", "J32"), Player.Guney, h4, "2♠", sonucEkle, ref gecen, ref kalan);
            T("2NT — max dengeli", m, TestHelpers.ElOlustur("AKJ2", "32", "AQ32", "KJ2"), Player.Guney, h4, "2NT", sonucEkle, ref gecen, ref kalan);
            string[] hn = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2NT", "Pas" };
            T("2NT röle → açan 3♣", m, TestHelpers.ElOlustur("AKJ2", "32", "K432", "J32"), Player.Guney, hn, "3♣", sonucEkle, ref gecen, ref kalan);
            string[] hn2 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2NT", "Pas", "3♣", "Pas" };
            T("2NT röle → 3♣ → Pas", m, TestHelpers.ElOlustur("32", "432", "32", "KJ9432"), Player.Kuzey, hn2, "Pas", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── Cevapçı: açanın cevabından sonra ──");
            string[] g1 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2♣", "Pas", "2♦", "Pas" };
            T("2♦ sonrası 2♥ röle", m, TestHelpers.ElOlustur("K32", "AQ432", "Q3", "K32"), Player.Kuzey, g1, "2♥", sonucEkle, ref gecen, ref kalan);
            string[] g2 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2♣", "Pas", "2♥", "Pas" };
            T("2♥ sonrası 2♠ röle", m, TestHelpers.ElOlustur("K32", "AQ43", "Q32", "K32"), Player.Kuzey, g2, "2♠", sonucEkle, ref gecen, ref kalan);
            string[] g3 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2♣", "Pas", "2♠", "Pas" };
            T("2♠ (max) sonrası 3NT", m, TestHelpers.ElOlustur("K32", "AQ43", "Q32", "K32"), Player.Kuzey, g3, "3NT", sonucEkle, ref gecen, ref kalan);
            string[] g4 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2♣", "Pas", "2NT", "Pas" };
            T("2NT (max dengeli) sonrası 3NT", m, TestHelpers.ElOlustur("K32", "AQ43", "Q32", "K32"), Player.Kuzey, g4, "3NT", sonucEkle, ref gecen, ref kalan);
            T("2NT sonrası ♠ 4'lü: 4♠", m, TestHelpers.ElOlustur("KQ32", "AQ43", "Q32", "32"), Player.Kuzey, g4, "4♠", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── Açan: ikinci röle cevabı ──");
            string[] r1 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas" };
            T("2♦-2♥: min → 2♠", m, TestHelpers.ElOlustur("AKJ2", "Q32", "K432", "32"), Player.Guney, r1, "2♠", sonucEkle, ref gecen, ref kalan);
            T("2♦-2♥: max → 2NT", m, TestHelpers.ElOlustur("AKJ2", "Q32", "AK32", "K2"), Player.Guney, r1, "2NT", sonucEkle, ref gecen, ref kalan);
            string[] r2 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2♣", "Pas", "2♥", "Pas", "2♠", "Pas" };
            T("2♥-2♠: min → 2NT", m, TestHelpers.ElOlustur("AKJ2", "32", "K432", "J32"), Player.Guney, r2, "2NT", sonucEkle, ref gecen, ref kalan);
            T("2♥-2♠: max → 3♣", m, TestHelpers.ElOlustur("AKJ2", "32", "AK32", "KJ2"), Player.Guney, r2, "3♣", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── Cevapçı: son karar ──");
            string[] k1 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2NT", "Pas" };
            T("açan max, 12 HCP, ♥ 5'li → 4♥", m, TestHelpers.ElOlustur("K32", "AQ432", "Q3", "K32"), Player.Kuzey, k1, "4♥", sonucEkle, ref gecen, ref kalan);
            string[] k2 = { "1♦", "Pas", "1♥", "Pas", "1♠", "Pas", "2♣", "Pas", "2♦", "Pas", "2♥", "Pas", "2♠", "Pas" };
            T("açan min, 12 HCP → Pas", m, TestHelpers.ElOlustur("K32", "AQ432", "Q3", "J32"), Player.Kuzey, k2, "Pas", sonucEkle, ref gecen, ref kalan);

            sonucEkle("");
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        private static void T(string ad, IAcilisGelisimi motor, List<Card> el, Player aktif, string[] gecmis, string beklenen,
            Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, aktif, gecmis);
            bool uygun = motor.UygunMu(el, durum);
            string sonuc = uygun ? motor.TeklifVer(el, durum) : null;
            if (sonuc == beklenen) { sonucEkle($"✅ DOĞRU: {sonuc ?? "(devreye girmedi)"}"); gecen++; }
            else { sonucEkle($"❌ HATA: Beklenen {beklenen ?? "(devreye girmez)"}, gelen {sonuc ?? "(devreye girmedi)"}"); kalan++; }
        }
    }
}
