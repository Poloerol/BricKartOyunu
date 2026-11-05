using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.IkiRenk;

namespace BricKartOyunu.Tests
{
    public static class TestRunner_IkiRenk
    {
        public static void TumTestleriCalistir(RichTextBox txtSonuclar, Action<string> sonucEkle)
        {
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  İKİ RENKLİ ARAYA GİRİŞLER");
            sonucEkle("════════════════════════════════════════");
            sonucEkle("");
            int gecen = 0, kalan = 0;
            var m = new IkiRenk_Araya();
            sonucEkle("── Doğrudan koltuk: iki renkli girişler ──");
            T("1♦: ♠♥ 5-5 → 2♦", m, TestHelpers.ElOlustur("AJ932", "KQ932", "4", "43"), new[] { "1♦" }, "2♦", sonucEkle, ref gecen, ref kalan);
            T("1♦: ♠♣ 5-5 → 2NT", m, TestHelpers.ElOlustur("AKJ32", "4", "43", "QJ932"), new[] { "1♦" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("1♦: ♥♣ 5-5 → 3♣", m, TestHelpers.ElOlustur("4", "AKJ32", "43", "QJ932"), new[] { "1♦" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("1♣: majörler 5-4 → 2♣", m, TestHelpers.ElOlustur("AKJ32", "QJ32", "43", "43"), new[] { "1♣" }, "2♣", sonucEkle, ref gecen, ref kalan);
            T("1♣: majörler 5-5 → 2♦", m, TestHelpers.ElOlustur("AKJ32", "QJ932", "4", "43"), new[] { "1♣" }, "2♦", sonucEkle, ref gecen, ref kalan);
            T("1♥: ♠♦ 5-5 → 2♥", m, TestHelpers.ElOlustur("AKJ32", "4", "QJ932", "43"), new[] { "1♥" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("1♥: ♠♣ 5-5 → 2NT", m, TestHelpers.ElOlustur("AKJ32", "4", "43", "QJ932"), new[] { "1♥" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("1♠: ♥♦ 5-5 → 2♠", m, TestHelpers.ElOlustur("4", "AKJ32", "QJ932", "43"), new[] { "1♠" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("1♠: ♦♣ 5-5 → 3♣", m, TestHelpers.ElOlustur("4", "43", "AKJ32", "QJ932"), new[] { "1♠" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("5-5 ama 4 HCP: devreye girmez", m, TestHelpers.ElOlustur("J9532", "J9532", "4", "43"), new[] { "1♦" }, null, sonucEkle, ref gecen, ref kalan);
            T("5-4: devreye girmez", m, TestHelpers.ElOlustur("AKJ32", "QJ32", "43", "43"), new[] { "1♦" }, null, sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Cevapçı ──");
            T("1♦-2♦: ♥ tercih", m, TestHelpers.ElOlustur("32", "Q432", "432", "5432"), new[] { "1♦", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("1♦-2♦: ♠ tercih", m, TestHelpers.ElOlustur("Q432", "32", "432", "5432"), new[] { "1♦", "2♦", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("1♦-2♦: 5♠ zayıf → blokatif 3♠", m, TestHelpers.ElOlustur("QJ932", "32", "43", "5432"), new[] { "1♦", "2♦", "Pas" }, "3♠", sonucEkle, ref gecen, ref kalan);
            T("1♦-2♦: 10 HCP tutuş → 2NT davet", m, TestHelpers.ElOlustur("K32", "432", "KQ2", "Q432"), new[] { "1♦", "2♦", "Pas" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("1♦-2♦: 15 HCP ♠ tutuş → 4♠", m, TestHelpers.ElOlustur("AK3", "432", "KQ2", "QJ43"), new[] { "1♦", "2♦", "Pas" }, "4♠", sonucEkle, ref gecen, ref kalan);
            T("1♦-2♦: F1R 5♣ 10 HCP", m, TestHelpers.ElOlustur("32", "43", "5432", "AKQJ3"), new[] { "1♦", "2♦", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("1♦-2NT: ♣ tercih", m, TestHelpers.ElOlustur("32", "K432", "432", "Q432"), new[] { "1♦", "2NT", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("1♦-2NT: 10 HCP tutuş → 3♦ davet", m, TestHelpers.ElOlustur("K32", "432", "QJ2", "KJ32"), new[] { "1♦", "2NT", "Pas" }, "3♦", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Araya girenin davet cevabı ──");
            TK("maksimum → 4♠", m, TestHelpers.ElOlustur("AKQ32", "KJ932", "4", "43"), new[] { "1♦", "2♦", "Pas", "2NT", "Pas" }, "4♠", sonucEkle, ref gecen, ref kalan);
            TK("minimum → 3♠", m, TestHelpers.ElOlustur("K9532", "J9532", "4", "43"), new[] { "1♦", "2♦", "Pas", "2NT", "Pas" }, "3♠", sonucEkle, ref gecen, ref kalan);

            sonucEkle("── Rakibin 1NT açılışına araya girişler ──");
            var nt = new IkiRenk_NTAraya();
            T("güçlü NT: Dbl ceza 19 HCP", nt, TestHelpers.ElOlustur("AK3", "KQ2", "AJ32", "Q32"), new[] { "1NT" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            T("güçlü NT: 2♣ majörler 5-4", nt, TestHelpers.ElOlustur("AQJ32", "K432", "43", "43"), new[] { "1NT" }, "2♣", sonucEkle, ref gecen, ref kalan);
            T("güçlü NT: 2♦ Multi", nt, TestHelpers.ElOlustur("AQJ932", "43", "K32", "43"), new[] { "1NT" }, "2♦", sonucEkle, ref gecen, ref kalan);
            T("güçlü NT: 2♥ 5♥ + 4 minör", nt, TestHelpers.ElOlustur("43", "AQJ32", "K432", "43"), new[] { "1NT" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("güçlü NT: 2♠ 5♠ + 4 minör", nt, TestHelpers.ElOlustur("AQJ32", "43", "43", "K432"), new[] { "1NT" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("güçlü NT: 2NT minörler", nt, TestHelpers.ElOlustur("4", "43", "AKJ32", "QJ932"), new[] { "1NT" }, "2NT", sonucEkle, ref gecen, ref kalan);
            T("güçlü NT: 3♣ doğal", nt, TestHelpers.ElOlustur("543", "43", "43", "AKQJ32"), new[] { "1NT" }, "3♣", sonucEkle, ref gecen, ref kalan);
            T("güçlü NT: 4♥ + 5♦ girmez (zayıf NT sistemi)", nt, TestHelpers.ElOlustur("43", "KQJ2", "AQ932", "43"), new[] { "1NT" }, null, sonucEkle, ref gecen, ref kalan);
            TW("zayıf NT: 2♥ 4♥ + 5 minör", nt, TestHelpers.ElOlustur("43", "KQJ2", "AQ932", "43"), new[] { "1NT" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("4. durum: 2♣ majörler 4-4", nt, TestHelpers.ElOlustur("KJ32", "AQ32", "43", "432"), new[] { "1NT", "Pas", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            T("4. durum: Dbl = ♣", nt, TestHelpers.ElOlustur("543", "43", "K32", "AQJ32"), new[] { "1NT", "Pas", "Pas" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            T("4. durum: 2♠ doğal", nt, TestHelpers.ElOlustur("AQJ32", "432", "K32", "43"), new[] { "1NT", "Pas", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("cevap: 2♣ → daha uzun majör 2♠", nt, TestHelpers.ElOlustur("Q432", "J32", "432", "432"), new[] { "1NT", "2♣", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            T("cevap: 2♣ → eşitse 2♥", nt, TestHelpers.ElOlustur("432", "432", "K432", "432"), new[] { "1NT", "2♣", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("cevap: Multi → 2♥", nt, TestHelpers.ElOlustur("432", "432", "K432", "432"), new[] { "1NT", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            T("cevap: 2NT → uzun minör 3♦", nt, TestHelpers.ElOlustur("Q432", "32", "5432", "432"), new[] { "1NT", "2NT", "Pas" }, "3♦", sonucEkle, ref gecen, ref kalan);
            TK("Multi: 6♠ → 2♠ düzelt", nt, TestHelpers.ElOlustur("AQJ932", "43", "K32", "43"), new[] { "1NT", "2♦", "Pas", "2♥", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TK("Multi: 6♥ → Pas", nt, TestHelpers.ElOlustur("43", "AQJ932", "K32", "43"), new[] { "1NT", "2♦", "Pas", "2♥", "Pas" }, "Pas", sonucEkle, ref gecen, ref kalan);
            sonucEkle("");
            sonucEkle($"  SONUÇ: {gecen} geçti, {kalan} kaldı");
        }

        // Aktif: Güney (açılış Doğu'da, ortak Kuzey)
        private static void T(string ad, IAcilisGelisimi motor, List<Card> el, string[] gecmis, string beklenen,
            Action<string> sonucEkle, ref int gecen, ref int kalan)
        { Calistir(ad, motor, el, Player.Guney, gecmis, beklenen, sonucEkle, ref gecen, ref kalan); }

        // Aktif: Kuzey (araya giren), Güney davet etti
        private static void TK(string ad, IAcilisGelisimi motor, List<Card> el, string[] gecmis, string beklenen,
            Action<string> sonucEkle, ref int gecen, ref int kalan)
        { Calistir(ad, motor, el, Player.Kuzey, gecmis, beklenen, sonucEkle, ref gecen, ref kalan); }

        // Zayıf NT (12-14) anlaşması: aktif Güney
        private static void TW(string ad, IAcilisGelisimi motor, List<Card> el, string[] gecmis, string beklenen,
            Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            durum.Anlasma.StrongNT = false;
            bool uygun = motor.UygunMu(el, durum);
            string sonuc = uygun ? motor.TeklifVer(el, durum) : null;
            if (sonuc == beklenen) { sonucEkle($"✅ DOĞRU: {sonuc ?? "(devreye girmedi)"}"); gecen++; }
            else { sonucEkle($"❌ HATA: Beklenen {beklenen ?? "(devreye girmez)"}, gelen {sonuc ?? "(devreye girmedi)"}"); kalan++; }
        }

        private static void Calistir(string ad, IAcilisGelisimi motor, List<Card> el, Player aktif, string[] gecmis, string beklenen,
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
