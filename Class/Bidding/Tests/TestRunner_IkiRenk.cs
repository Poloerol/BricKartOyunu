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
            sonucEkle("── Zayıf NT versiyonu: açılış tanımları (s.75) ──");
            var oc = new BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OneClub_Opening();
            var od = new BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond.OneDiamond_Opening();
            TW("15 HCP dengeli, 4♦ → 1♣", oc, TestHelpers.ElOlustur("AQ3", "K32", "KJ32", "Q32"), new string[0], "1♣", sonucEkle, ref gecen, ref kalan);
            TW("15 HCP dengeli, 4♦ → 1♦ açmaz", od, TestHelpers.ElOlustur("AQ3", "K32", "KJ32", "Q32"), new string[0], null, sonucEkle, ref gecen, ref kalan);
            TW("16 HCP 5♦ 3-3-2 → 1♣", oc, TestHelpers.ElOlustur("AQ3", "K32", "KJ432", "Q3"), new string[0], "1♣", sonucEkle, ref gecen, ref kalan);
            TW("16 HCP 5♦ 4♥ 2-2 → 1♣", oc, TestHelpers.ElOlustur("A3", "KQ32", "KJ432", "Q3"), new string[0], "1♣", sonucEkle, ref gecen, ref kalan);
            TW("16 HCP 5♦ 4♥ dengesiz → 1♦", od, TestHelpers.ElOlustur("AQ2", "KQ32", "KJ432", "3"), new string[0], "1♦", sonucEkle, ref gecen, ref kalan);
            TW("4♦4♥4♠1♣ 13 HCP → 1♦", od, TestHelpers.ElOlustur("KQ32", "KJ32", "AJ32", "3"), new string[0], "1♦", sonucEkle, ref gecen, ref kalan);
            TW("6♦ 3-2-2 kötü renk 15 HCP → 1♣", oc, TestHelpers.ElOlustur("AK3", "K2", "J98743", "A2"), new string[0], "1♣", sonucEkle, ref gecen, ref kalan);
            TW("6♦ tek renkli 12 HCP → 1♦", od, TestHelpers.ElOlustur("A32", "3", "KQJ987", "K32"), new string[0], "1♦", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Zayıf NT versiyonu: 1♣ cevapları ve 1♣-1♦ (4+♥) gelişimi (s.76-78) ──");
            var zo = new BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OneClubZ_OneDiamond();
            TA("cevap: 4♥ → 1♦ (transfer)", TestHelpers.ElOlustur("K32", "KJ32", "432", "432"), "1♦", sonucEkle, ref gecen, ref kalan);
            TA("cevap: 4♠ → 1♥ (transfer)", TestHelpers.ElOlustur("KJ32", "432", "432", "K32"), "1♥", sonucEkle, ref gecen, ref kalan);
            TA("cevap: 4♦ → 1♠ (transfer)", TestHelpers.ElOlustur("432", "432", "KJ32", "K32"), "1♠", sonucEkle, ref gecen, ref kalan);
            TA("cevap: 4♣ 9 HCP → 1NT", TestHelpers.ElOlustur("432", "432", "AJ2", "KJ32"), "1NT", sonucEkle, ref gecen, ref kalan);
            TW("açan: 15 dengeli 4♥ → 2♥", zo, TestHelpers.ElOlustur("KJ3", "AQ32", "K32", "Q32"), new[] { "1♣", "Pas", "1♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("açan: 15 dengeli 3♥ → 1NT", zo, TestHelpers.ElOlustur("KJ32", "AQ3", "K32", "Q32"), new[] { "1♣", "Pas", "1♦", "Pas" }, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("açan: 6♣ 4♥ dengesiz → 1♥", zo, TestHelpers.ElOlustur("A2", "KJ32", "3", "AQJ432"), new[] { "1♣", "Pas", "1♦", "Pas" }, "1♥", sonucEkle, ref gecen, ref kalan);
            TW("açan: 5♣ 4♠ 1♥ → 1♠", zo, TestHelpers.ElOlustur("KQ32", "2", "432", "AQJ32"), new[] { "1♣", "Pas", "1♦", "Pas" }, "1♠", sonucEkle, ref gecen, ref kalan);
            TW("açan: 6♣ 12 HCP → 2♣", zo, TestHelpers.ElOlustur("432", "3", "K32", "AKQ432"), new[] { "1♣", "Pas", "1♦", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1♥ sonrası 14 HCP → 2♦ röle", zo, TestHelpers.ElOlustur("AQ2", "KQ32", "K32", "432"), new[] { "1♣", "Pas", "1♦", "Pas", "1♥", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1♥ sonrası zayıf 5♥ → 2♥", zo, TestHelpers.ElOlustur("432", "KJ432", "32", "432"), new[] { "1♣", "Pas", "1♦", "Pas", "1♥", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1NT sonrası 5♥ FG → 2♣ puppet", zo, TestHelpers.ElOlustur("A2", "AKJ32", "K32", "432"), new[] { "1♣", "Pas", "1♦", "Pas", "1NT", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1NT sonrası zayıf 5♥ → 2♦ transfer", zo, TestHelpers.ElOlustur("432", "KJ432", "32", "432"), new[] { "1♣", "Pas", "1♦", "Pas", "1NT", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1NT-2♣ puppet → 2♦", zo, TestHelpers.ElOlustur("KJ32", "AQ3", "K32", "Q32"), new[] { "1♣", "Pas", "1♦", "Pas", "1NT", "Pas", "2♣", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1NT-2♦ transfer → 2♥", zo, TestHelpers.ElOlustur("KJ32", "AQ3", "K32", "Q32"), new[] { "1♣", "Pas", "1♦", "Pas", "1NT", "Pas", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Zayıf NT versiyonu: 1♣-1♥ (4+♠) gelişimi (s.79-81) ──");
            var zh = new BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OneClubZ_OneHeart();
            TW("açan: 3♠ dengesiz → 1♠", zh, TestHelpers.ElOlustur("KJ3", "2", "5432", "AQJ32"), new[] { "1♣", "Pas", "1♥", "Pas" }, "1♠", sonucEkle, ref gecen, ref kalan);
            TW("açan: 15 dengeli 4♠ → 2♠", zh, TestHelpers.ElOlustur("KJ32", "AQ3", "K32", "Q32"), new[] { "1♣", "Pas", "1♥", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("açan: 15 dengeli 3♠ → 1NT", zh, TestHelpers.ElOlustur("KJ3", "AQ32", "K32", "Q32"), new[] { "1♣", "Pas", "1♥", "Pas" }, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("açan: 6♣ ♠ kısa → 2♣", zh, TestHelpers.ElOlustur("43", "32", "K32", "AKQ432"), new[] { "1♣", "Pas", "1♥", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1♠ sonrası 14 HCP 5♠ → 2♦ röle", zh, TestHelpers.ElOlustur("KQ432", "K32", "AQ2", "32"), new[] { "1♣", "Pas", "1♥", "Pas", "1♠", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1♠ sonrası zayıf 5♠ → 2♠", zh, TestHelpers.ElOlustur("KJ432", "432", "32", "432"), new[] { "1♣", "Pas", "1♥", "Pas", "1♠", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1NT sonrası 5♠ FG → 2♣ puppet", zh, TestHelpers.ElOlustur("AKJ32", "A2", "K32", "432"), new[] { "1♣", "Pas", "1♥", "Pas", "1NT", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1NT sonrası zayıf 5♠ → 2♥ transfer", zh, TestHelpers.ElOlustur("KJ432", "432", "32", "432"), new[] { "1♣", "Pas", "1♥", "Pas", "1NT", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1NT-2♥ transfer → 2♠", zh, TestHelpers.ElOlustur("KJ32", "AQ3", "K32", "Q32"), new[] { "1♣", "Pas", "1♥", "Pas", "1NT", "Pas", "2♥", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("açan: 2♠-2NT röle 15 HCP min → 3♠", zh, TestHelpers.ElOlustur("KJ32", "AQ3", "K32", "Q32"), new[] { "1♣", "Pas", "1♥", "Pas", "2♠", "Pas", "2NT", "Pas" }, "3♠", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Zayıf NT versiyonu: 1♣-1♠ (4+♦) gelişimi (s.83) ──");
            var zs = new BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OneClubZ_OneSpade();
            TW("açan: 15 dengeli → 1NT", zs, TestHelpers.ElOlustur("KJ32", "AQ3", "K32", "Q32"), new[] { "1♣", "Pas", "1♠", "Pas" }, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("açan: 5♣ dengesiz 12 HCP → 2♣", zs, TestHelpers.ElOlustur("KQ32", "432", "2", "AQJ32"), new[] { "1♣", "Pas", "1♠", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1NT sonrası 6♦ FG → 2♣ puppet", zs, TestHelpers.ElOlustur("32", "3", "AKQ432", "K432"), new[] { "1♣", "Pas", "1♠", "Pas", "1NT", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1NT sonrası 4♦ dengeli 11 HCP → 2NT", zs, TestHelpers.ElOlustur("K32", "Q32", "KJ32", "Q32"), new[] { "1♣", "Pas", "1♠", "Pas", "1NT", "Pas" }, "2NT", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1NT-2♦ transfer → 2♥", zs, TestHelpers.ElOlustur("KJ32", "AQ3", "K32", "Q32"), new[] { "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1NT-2♠ transfer → 3♣", zs, TestHelpers.ElOlustur("KJ32", "AQ3", "K32", "Q32"), new[] { "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2♠", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            TW("açan: 2♣-2♦ röle 5♣4♠ → 2♠", zs, TestHelpers.ElOlustur("KQ32", "432", "2", "AQJ32"), new[] { "1♣", "Pas", "1♠", "Pas", "2♣", "Pas", "2♦", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
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

        // 1♣ cevabı (IKonvansiyon): ortak 1♣ açtı, sağdaki Pas → ben (Güney)
        private static void TA(string ad, List<Card> el, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, new[] { "1♣", "Pas" });
            durum.Anlasma.StrongNT = false;
            var k = new BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OneClub_Answer();
            string sonuc = k.UygunMu(durum) ? k.TeklifVer(durum) : null;
            if (sonuc == beklenen) { sonucEkle($"✅ DOĞRU: {sonuc}"); gecen++; }
            else { sonucEkle($"❌ HATA: Beklenen {beklenen}, gelen {sonuc ?? "(devreye girmedi)"}"); kalan++; }
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
