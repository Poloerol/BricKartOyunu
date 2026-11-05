using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.TwoSpade;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_TwoSpade
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  2♠ AÇILIŞ SİSTEMİ TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            sonucEkle("── 2♠ AÇILIŞ açışına cevaplar ──");
            TestA("3♠ — önleyici raise",
                TestHelpers.ElOlustur("J932", "432", "32", "5432"), "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestA("2NT — röle 13 HCP",
                TestHelpers.ElOlustur("Q2", "AK32", "K32", "J432"), "2NT",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2♠ ⇒ 2NT ──");
            TestG("açan 3♥ — SNG ♥", new TwoSpade_TwoNT(),
                TestHelpers.ElOlustur("KQJ932", "K", "432", "432"),
                new[] { "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3♦ — SNG ♦", new TwoSpade_TwoNT(),
                TestHelpers.ElOlustur("KQJ932", "432", "K", "432"),
                new[] { "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3♠ — SNG yok min", new TwoSpade_TwoNT(),
                TestHelpers.ElOlustur("QJ9532", "432", "K3", "32"),
                new[] { "2♠", "Pas", "2NT", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("2NT sonrası 3♦ röle", new TwoSpade_TwoNT(),
                TestHelpers.ElOlustur("Q2", "AK32", "K32", "J432"),
                new[] { "2♠", "Pas", "2NT", "Pas", "3♣", "Pas" }, Player.Kuzey, "3♦",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("");
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        private static void Rapor(string sonuc, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            if (sonuc == beklenen) { sonucEkle($"✅ DOĞRU: {sonuc}"); gecen++; }
            else { sonucEkle($"❌ HATA: Beklenen {beklenen}, gelen {sonuc}"); kalan++; }
        }

        private static void TestA(string ad, List<Card> el, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, "2♠", "Pas");
            var motor = new TwoSpade_Answer();
            if (!motor.UygunMu(durum)) { sonucEkle("❌ HATA: UygunMu false döndü."); kalan++; return; }
            Rapor(motor.TeklifVer(durum), beklenen, sonucEkle, ref gecen, ref kalan);
        }

        private static void TestG(string ad, IAcilisGelisimi motor, List<Card> el, string[] gecmis, Player aktif, string beklenen,
            Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, aktif, gecmis);
            if (!motor.UygunMu(el, durum)) { sonucEkle("❌ HATA: UygunMu false döndü."); kalan++; return; }
            Rapor(motor.TeklifVer(el, durum), beklenen, sonucEkle, ref gecen, ref kalan);
        }
    }
}
