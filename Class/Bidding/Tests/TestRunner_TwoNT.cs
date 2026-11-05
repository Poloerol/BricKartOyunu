using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.TwoNT;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_TwoNT
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  2NT AÇILIŞ SİSTEMİ TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");

            int gecen = 0, kalan = 0;

            sonucEkle("── 2NT AÇILIŞ açışına cevaplar ──");
            TestA("3♦ — 5♥ transfer",
                TestHelpers.ElOlustur("432", "J9532", "32", "K32"), "3♦",
                sonucEkle, ref gecen, ref kalan);

            TestA("3♥ — 5♠ transfer",
                TestHelpers.ElOlustur("J9532", "432", "32", "K32"), "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestA("3♣ — Stayman",
                TestHelpers.ElOlustur("KJ32", "Q32", "Q32", "432"), "3♣",
                sonucEkle, ref gecen, ref kalan);

            TestA("3NT — oyun",
                TestHelpers.ElOlustur("K32", "Q32", "Q32", "K432"), "3NT",
                sonucEkle, ref gecen, ref kalan);

            TestA("4♦ — 5/5 majör",
                TestHelpers.ElOlustur("AQ932", "AJ932", "3", "32"), "4♦",
                sonucEkle, ref gecen, ref kalan);

            TestA("4NT — niceliksel",
                TestHelpers.ElOlustur("AQ2", "KJ2", "Q32", "K432"), "4NT",
                sonucEkle, ref gecen, ref kalan);

            TestA("Pas — 1 HCP",
                TestHelpers.ElOlustur("432", "432", "432", "J432"), "Pas",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2NT ⇒ 3♣ ──");
            TestG("açan 3♠ — 4♠", new TwoNT_ThreeClub(),
                TestHelpers.ElOlustur("KQJ2", "Q32", "K32", "K32"),
                new[] { "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3♥ — 4♥", new TwoNT_ThreeClub(),
                TestHelpers.ElOlustur("K32", "KQJ2", "K32", "Q32"),
                new[] { "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3♦ — majör yok", new TwoNT_ThreeClub(),
                TestHelpers.ElOlustur("K32", "K32", "KQ32", "Q32"),
                new[] { "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3♦",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3NT — 4♥4♠", new TwoNT_ThreeClub(),
                TestHelpers.ElOlustur("KQ32", "KJ32", "K32", "Q3"),
                new[] { "2NT", "Pas", "3♣", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♦ sonrası 3♥ Smolen", new TwoNT_ThreeClub(),
                TestHelpers.ElOlustur("KQ932", "J932", "3", "432"),
                new[] { "2NT", "Pas", "3♣", "Pas", "3♦", "Pas" }, Player.Kuzey, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♥ sonrası 3♠ — SI", new TwoNT_ThreeClub(),
                TestHelpers.ElOlustur("A32", "KJ32", "KQ3", "K32"),
                new[] { "2NT", "Pas", "3♣", "Pas", "3♥", "Pas" }, Player.Kuzey, "3♠",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♥ sonrası 4♥ — oyun", new TwoNT_ThreeClub(),
                TestHelpers.ElOlustur("432", "KJ32", "K32", "432"),
                new[] { "2NT", "Pas", "3♣", "Pas", "3♥", "Pas" }, Player.Kuzey, "4♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("3NT sonrası 4♣ transfer", new TwoNT_ThreeClub(),
                TestHelpers.ElOlustur("432", "KJ32", "K32", "432"),
                new[] { "2NT", "Pas", "3♣", "Pas", "3NT", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            TestG("transfer kabulü 4♥", new TwoNT_ThreeClub(),
                TestHelpers.ElOlustur("KQ32", "KJ32", "K32", "Q3"),
                new[] { "2NT", "Pas", "3♣", "Pas", "3NT", "Pas", "4♣", "Pas" }, Player.Guney, "4♥",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2NT ⇒ 3♦ ──");
            TestG("açan 3♥ — 3♥", new TwoNT_ThreeDiamond(),
                TestHelpers.ElOlustur("K32", "Q32", "Q32", "K432"),
                new[] { "2NT", "Pas", "3♦", "Pas" }, Player.Guney, "3♥",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3NT — 2♥", new TwoNT_ThreeDiamond(),
                TestHelpers.ElOlustur("K32", "K2", "Q432", "KQ32"),
                new[] { "2NT", "Pas", "3♦", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♥ sonrası Pas — zayıf", new TwoNT_ThreeDiamond(),
                TestHelpers.ElOlustur("432", "432", "432", "J432"),
                new[] { "2NT", "Pas", "3♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "Pas",
                sonucEkle, ref gecen, ref kalan);

            TestG("3♥ sonrası 4♥", new TwoNT_ThreeDiamond(),
                TestHelpers.ElOlustur("432", "KQ932", "K32", "32"),
                new[] { "2NT", "Pas", "3♦", "Pas", "3♥", "Pas" }, Player.Kuzey, "4♥",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2NT ⇒ 3♠ ──");
            TestG("açan 4♣ — ♣ uygun", new TwoNT_ThreeSpade(),
                TestHelpers.ElOlustur("K32", "Q32", "Q32", "K432"),
                new[] { "2NT", "Pas", "3♠", "Pas" }, Player.Guney, "4♣",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 3NT — uygunluk yok", new TwoNT_ThreeSpade(),
                TestHelpers.ElOlustur("K32", "Q432", "K432", "32"),
                new[] { "2NT", "Pas", "3♠", "Pas" }, Player.Guney, "3NT",
                sonucEkle, ref gecen, ref kalan);

            TestG("3NT sonrası 4♣ — 6♣ SNG ♠", new TwoNT_ThreeSpade(),
                TestHelpers.ElOlustur("2", "K32", "K32", "KQJ932"),
                new[] { "2NT", "Pas", "3♠", "Pas", "3NT", "Pas" }, Player.Kuzey, "4♣",
                sonucEkle, ref gecen, ref kalan);

            sonucEkle("── 2NT ⇒ 4♣ ──");
            TestG("açan 4♦", new TwoNT_FourClub(),
                TestHelpers.ElOlustur("K32", "Q32", "Q432", "K32"),
                new[] { "2NT", "Pas", "4♣", "Pas" }, Player.Guney, "4♦",
                sonucEkle, ref gecen, ref kalan);

            TestG("açan 4NT", new TwoNT_FourClub(),
                TestHelpers.ElOlustur("K432", "Q432", "Q2", "K32"),
                new[] { "2NT", "Pas", "4♣", "Pas" }, Player.Guney, "4NT",
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
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, "2NT", "Pas");
            var motor = new TwoNT_Answer();
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
