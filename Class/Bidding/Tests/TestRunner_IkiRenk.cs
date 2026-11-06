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
            sonucEkle("── Zayıf NT versiyonu: 1♦ cevapları ve 1♦-1♥/1♠ 1NT rebidi (s.84-86) ──");
            var zdh = new BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond.OneDiamondZ_OneHeart();
            var zds = new BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond.OneDiamondZ_OneSpade();
            TW("açan: 5♦ 3♥ → 1NT", zdh, TestHelpers.ElOlustur("A32", "K32", "AQJ32", "32"), new[] { "1♦", "Pas", "1♥", "Pas" }, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("açan: 4♥ ise devreye girmez", zdh, TestHelpers.ElOlustur("A32", "K432", "AQJ32", "2"), new[] { "1♦", "Pas", "1♥", "Pas" }, null, sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1NT sonrası zayıf 5♥ → 2♥", zdh, TestHelpers.ElOlustur("432", "KJ432", "32", "432"), new[] { "1♦", "Pas", "1♥", "Pas", "1NT", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 5♥ 4♠ 13 HCP → 2♠ FG", zdh, TestHelpers.ElOlustur("KQ32", "AJ432", "K2", "32"), new[] { "1♦", "Pas", "1♥", "Pas", "1NT", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 5♥ 17 HCP dengeli → 2♣ röle", zdh, TestHelpers.ElOlustur("432", "AKJ32", "KQ2", "A2"), new[] { "1♦", "Pas", "1♥", "Pas", "1NT", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("açan: 2♣ röle, 6♦ 13 HCP → 2♦", zdh, TestHelpers.ElOlustur("A32", "K32", "KQJ932", "2"), new[] { "1♦", "Pas", "1♥", "Pas", "1NT", "Pas", "2♣", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("açan: 2♣ röle, 5♦4♠3♥1♣ → 2♠", zdh, TestHelpers.ElOlustur("AKJ2", "K32", "AQJ32", "2"), new[] { "1♦", "Pas", "1♥", "Pas", "1NT", "Pas", "2♣", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("açan: 5♦ 3♠ → 1NT", zds, TestHelpers.ElOlustur("K32", "A32", "AQJ32", "K2"), new[] { "1♦", "Pas", "1♠", "Pas" }, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1NT sonrası zayıf 5♠ → 2♠", zds, TestHelpers.ElOlustur("KJ432", "432", "32", "432"), new[] { "1♦", "Pas", "1♠", "Pas", "1NT", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TD("1♦ cevap: 5 HCP 4♥ → 1♥", TestHelpers.ElOlustur("432", "KQ32", "432", "432"), "1♥", sonucEkle, ref gecen, ref kalan);
            TD("1♦ cevap: 4 HCP 4♥ → Pas", TestHelpers.ElOlustur("432", "KJ32", "432", "432"), "Pas", sonucEkle, ref gecen, ref kalan);
            TD("1♦ cevap: 8 HCP dengeli → 1NT", TestHelpers.ElOlustur("K32", "432", "432", "KQ32"), "1NT", sonucEkle, ref gecen, ref kalan);
            TD("1♦ cevap: 4♦ 5 HCP majör yok → 3♦ blokaj", TestHelpers.ElOlustur("43", "432", "KJ32", "J432"), "3♦", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Zayıf NT versiyonu: 12-14 1NT açılışı cevapları ve gelişimleri (s.87-89) ──");
            var z2c = new BricKartOyunu.Class.Bidding.OpeningSystems.OneNT.OneNTZ_TwoClub();
            var z2d = new BricKartOyunu.Class.Bidding.OpeningSystems.OneNT.OneNTZ_TwoDiamond();
            var z2h = new BricKartOyunu.Class.Bidding.OpeningSystems.OneNT.OneNTZ_TwoHeart();
            var z2n = new BricKartOyunu.Class.Bidding.OpeningSystems.OneNT.OneNTZ_TwoNT();
            TN("1NT cevap: zayıf 5♠ → 2♠ (transfer yok)", TestHelpers.ElOlustur("KJ432", "432", "32", "432"), "2♠", sonucEkle, ref gecen, ref kalan);
            TN("1NT cevap: 15 dengeli majörsüz → 3NT", TestHelpers.ElOlustur("AK2", "KQ3", "Q432", "J32"), "3NT", sonucEkle, ref gecen, ref kalan);
            TN("1NT cevap: 15 HCP 4♥ → 2♦ FG Stayman", TestHelpers.ElOlustur("AK2", "KQ32", "Q32", "J32"), "2♦", sonucEkle, ref gecen, ref kalan);
            TN("1NT cevap: 11 dengeli majörsüz → 2NT davet", TestHelpers.ElOlustur("K32", "Q32", "KJ32", "Q32"), "2NT", sonucEkle, ref gecen, ref kalan);
            TN("1NT cevap: 11 HCP 5♥ → 2♣ davet Stayman", TestHelpers.ElOlustur("32", "KQJ32", "K32", "Q32"), "2♣", sonucEkle, ref gecen, ref kalan);
            TN("1NT cevap: 6♥ 13 HCP → 4♥", TestHelpers.ElOlustur("A2", "KQJ932", "K32", "32"), "4♥", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1NT-2♣ 4♥ → 2♥", z2c, TestHelpers.ElOlustur("432", "KJ32", "AQ2", "K32"), new[] { "1NT", "Pas", "2♣", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1NT-2♣-2♦ 5♥ davet → 2♥", z2c, TestHelpers.ElOlustur("32", "KQJ32", "K32", "Q32"), new[] { "1NT", "Pas", "2♣", "Pas", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 1NT-2♦-2♥ uyum 13 HCP → 4♥", z2d, TestHelpers.ElOlustur("A32", "KQ32", "K32", "J32"), new[] { "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, "4♥", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1NT-2NT 14 HCP → 3NT", z2n, TestHelpers.ElOlustur("AK32", "Q32", "K32", "Q32"), new[] { "1NT", "Pas", "2NT", "Pas" }, "3NT", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1NT-2♥ 14 HCP 3♥ → 3♥", z2h, TestHelpers.ElOlustur("A32", "KQ3", "K432", "Q32"), new[] { "1NT", "Pas", "2♥", "Pas" }, "3♥", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Zayıf NT versiyonu: 1NT-(Dbl) araya giriş (s.90) ──");
            var zar = new BricKartOyunu.Class.Bidding.OpeningSystems.OneNT.OneNTZ_Araya();
            string[] hDbl = { "1NT", "Dbl" };
            TW("1NT-(Dbl): zayıf dengeli → Pas", zar, TestHelpers.ElOlustur("K432", "Q32", "432", "J32"), hDbl, "Pas", sonucEkle, ref gecen, ref kalan);
            TW("1NT-(Dbl): 10 HCP → RDbl", zar, TestHelpers.ElOlustur("AQ32", "K32", "432", "J32"), hDbl, "RDbl", sonucEkle, ref gecen, ref kalan);
            TW("1NT-(Dbl): zayıf 6♠ → 2♠", zar, TestHelpers.ElOlustur("KJ9432", "32", "432", "32"), hDbl, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("1NT-(Dbl): 7♥ blokaj → 3♥", zar, TestHelpers.ElOlustur("32", "KJ98432", "43", "32"), hDbl, "3♥", sonucEkle, ref gecen, ref kalan);
            TW("1NT-(Dbl): 5-5 majör 13 HCP → 2NT", zar, TestHelpers.ElOlustur("AKJ32", "KQ932", "32", "3"), hDbl, "2NT", sonucEkle, ref gecen, ref kalan);
            string[] hPP = { "1NT", "Dbl", "Pas", "Pas" };
            TW("açan: 1NT-(Dbl)-Pas-(Pas) 5♥ → 2♥", zar, TestHelpers.ElOlustur("A32", "KQ432", "K32", "Q3"), hPP, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("açan: 5'li renk yok 14 HCP → Pas", zar, TestHelpers.ElOlustur("AK32", "Q32", "K32", "Q32"), hPP, "Pas", sonucEkle, ref gecen, ref kalan);
            TW("açan: 5'li renk yok 12 HCP → RDbl", zar, TestHelpers.ElOlustur("AQ32", "K32", "Q32", "J32"), hPP, "RDbl", sonucEkle, ref gecen, ref kalan);
            TW("RDbl sonrası cevapçı: en uzun renk 4♥ → 2♥", zar, TestHelpers.ElOlustur("432", "KJ98", "Q32", "432"), new[] { "1NT", "Dbl", "Pas", "Pas", "RDbl", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1NT-(Dbl)-2♠-(Pas) → Pas", zar, TestHelpers.ElOlustur("AK32", "Q32", "K32", "Q32"), new[] { "1NT", "Dbl", "2♠", "Pas" }, "Pas", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1NT-(Dbl)-2NT-(Pas) → 3♣ röle", zar, TestHelpers.ElOlustur("AK32", "Q32", "K32", "Q32"), new[] { "1NT", "Dbl", "2NT", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            TW("cevapçı: 2NT-3♣ röle → yüksek renk 3♠", zar, TestHelpers.ElOlustur("AKJ32", "KQ932", "32", "3"), new[] { "1NT", "Dbl", "2NT", "Pas", "3♣", "Pas" }, "3♠", sonucEkle, ref gecen, ref kalan);
            TW("açan: ...3♠ ve 3 maça → 4♠", zar, TestHelpers.ElOlustur("Q32", "K32", "AK32", "Q32"), new[] { "1NT", "Dbl", "2NT", "Pas", "3♣", "Pas", "3♠", "Pas" }, "4♠", sonucEkle, ref gecen, ref kalan);
            TW("açan: ...3♠ ve 2 maça → 3NT", zar, TestHelpers.ElOlustur("K2", "Q32", "AQ32", "K432"), new[] { "1NT", "Dbl", "2NT", "Pas", "3♣", "Pas", "3♠", "Pas" }, "3NT", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── 1♣-1♦-1NT-2♦ FG röle: açanın öncelik sırası ve cevapçı (s.92) ──");
            var oc1d = new BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OneClub_OneDiamond();
            string[] hRl = { "1♣", "Pas", "1♦", "Pas", "1NT", "Pas", "2♦", "Pas" };
            T("röle: 4♥ → 2♥", oc1d, TestHelpers.ElOlustur("K32", "Q432", "K32", "AJ2"), hRl, "2♥", sonucEkle, ref gecen, ref kalan);
            T("röle: 4♠ → 2♠", oc1d, TestHelpers.ElOlustur("Q432", "K32", "K32", "AJ2"), hRl, "2♠", sonucEkle, ref gecen, ref kalan);
            T("röle: 3♦ destek → 3♦", oc1d, TestHelpers.ElOlustur("K32", "Q32", "KJ2", "AJ32"), hRl, "3♦", sonucEkle, ref gecen, ref kalan);
            T("röle: iyi 5♣ → 3♣", oc1d, TestHelpers.ElOlustur("K32", "Q32", "32", "AKJ32"), hRl, "3♣", sonucEkle, ref gecen, ref kalan);
            T("röle: kötü 5♣ → 2NT", oc1d, TestHelpers.ElOlustur("K32", "Q32", "32", "AJ432"), hRl, "2NT", sonucEkle, ref gecen, ref kalan);
            T("röle sonrası: 2♥ + 4♥ → 4♥", oc1d, TestHelpers.ElOlustur("32", "KJ98", "AQ432", "K2"), new[] { "1♣", "Pas", "1♦", "Pas", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, "4♥", sonucEkle, ref gecen, ref kalan);
            T("röle sonrası: 2♥ + 3♥ → 3NT", oc1d, TestHelpers.ElOlustur("AK2", "432", "KQ432", "32"), new[] { "1♣", "Pas", "1♦", "Pas", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas" }, "3NT", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Zayıf NT: 1♣ açışına araya giriş cevapları (s.111-113) ──");
            var za = new BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OneClubZ_Araya();
            string[] aDbl = { "1♣", "Dbl" }, a1d = { "1♣", "1♦" }, a1h = { "1♣", "1♥" }, a1s = { "1♣", "1♠" }, a1n = { "1♣", "1NT" };
            TW("1♣-(Dbl): 5♥ 15 HCP → 2♦", za, TestHelpers.ElOlustur("K2", "AQJ32", "K32", "Q32"), aDbl, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(Dbl): 4♥ 9 HCP → 1♦", za, TestHelpers.ElOlustur("K32", "KJ98", "Q32", "432"), aDbl, "1♦", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(Dbl): majörsüz 8 HCP → 1♠", za, TestHelpers.ElOlustur("Q32", "Q32", "KJ32", "432"), aDbl, "1♠", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(Dbl): 10 HCP → RDbl", za, TestHelpers.ElOlustur("K32", "Q32", "AJ32", "432"), aDbl, "RDbl", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(Dbl): 5♣ zayıf → 1NT", za, TestHelpers.ElOlustur("32", "432", "432", "KJ932"), aDbl, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(Dbl): 4333 13 HCP → 3NT", za, TestHelpers.ElOlustur("K32", "Q32", "Q32", "AQ32"), aDbl, "3NT", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(Dbl): 5♦ zayıf → 2♣", za, TestHelpers.ElOlustur("32", "432", "KJ932", "432"), aDbl, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1♦): 5♠ 12 HCP → 2♥ transfer", za, TestHelpers.ElOlustur("AQ932", "32", "K32", "K32"), a1d, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1♦): 4♥ 9 HCP → Dbl", za, TestHelpers.ElOlustur("K32", "KJ98", "432", "Q32"), a1d, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1♦): dengeli 8 HCP ♦ stoperli → 1NT", za, TestHelpers.ElOlustur("K32", "Q32", "Q32", "J432"), a1d, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1♦): 5♥ 12 HCP → 2♦ cuebid transfer", za, TestHelpers.ElOlustur("32", "AQJ32", "432", "KQ2"), a1d, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1♦): 5♠5♥ 8 HCP → 2NT", za, TestHelpers.ElOlustur("KJ932", "KJ832", "32", "2"), a1d, "2NT", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1♥): 4♠ 8 HCP → Dbl", za, TestHelpers.ElOlustur("KJ98", "432", "Q32", "Q32"), a1h, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1♥): 5♦ 13 HCP → 2♣ transfer", za, TestHelpers.ElOlustur("K32", "32", "AKJ32", "Q32"), a1h, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1♥): dengeli 9 HCP ♥ stoperli → 1NT", za, TestHelpers.ElOlustur("32", "Q32", "KJ32", "K432"), a1h, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1♠): 4♥ 8 HCP → Dbl", za, TestHelpers.ElOlustur("432", "KJ98", "Q32", "Q32"), a1s, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1♠): 5♣ 11 HCP → 2♥ cuebid transfer", za, TestHelpers.ElOlustur("32", "432", "K32", "AKJ32"), a1s, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1NT): 5♠4♥ zayıf → 2♣ majörler", za, TestHelpers.ElOlustur("KJ932", "Q432", "32", "32"), a1n, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1NT): 5♦ zayıf → 2♦", za, TestHelpers.ElOlustur("32", "432", "KJ932", "432"), a1n, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(1NT): 6♠ 10 HCP → 2NT Lebensohl", za, TestHelpers.ElOlustur("AQJ932", "32", "K32", "32"), a1n, "2NT", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(2♥): 5♠ 9 HCP → 2♠ NF", za, TestHelpers.ElOlustur("AQJ32", "32", "K32", "432"), new[] { "1♣", "2♥" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(2♥): 15 HCP dengeli ♥ stoperli → 3NT", za, TestHelpers.ElOlustur("K32", "AQ2", "KJ32", "Q32"), new[] { "1♣", "2♥" }, "3NT", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(2♦): 4♥ 9 HCP → Dbl", za, TestHelpers.ElOlustur("KJ98", "Q432", "32", "K32"), new[] { "1♣", "2♦" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♣-(2♣ majörler): 5♦ 13 HCP → 2♠ FG", za, TestHelpers.ElOlustur("432", "32", "AKJ32", "KQ2"), new[] { "1♣", "2♣" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1♣-(1♥)-2♣ transfer → 2♦", za, TestHelpers.ElOlustur("AK32", "Q32", "K32", "AJ2"), new[] { "1♣", "1♥", "2♣", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1♣-(1♥)-2♥ transfer → 2♠", za, TestHelpers.ElOlustur("AK32", "Q32", "K32", "AJ2"), new[] { "1♣", "1♥", "2♥", "Pas" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1♣-(1♠)-2♥ cuebid transfer → 3♣", za, TestHelpers.ElOlustur("A32", "Q32", "K32", "AJ32"), new[] { "1♣", "1♠", "2♥", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Zayıf NT: 1♦ açışına araya giriş cevapları (s.113-115) ──");
            var zd1 = new BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond.OneDiamondZ_Araya();
            string[] d1h = { "1♦", "1♥" }, d1s = { "1♦", "1♠" }, d1n = { "1♦", "1NT" }, d1c = { "1♦", "2♣" };
            TW("1♦-(1♥): 4♠ 8 HCP → Dbl", zd1, TestHelpers.ElOlustur("KJ98", "432", "Q32", "Q32"), d1h, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1♥): 4♦ zayıf → 2♣ transfer", zd1, TestHelpers.ElOlustur("32", "432", "KJ32", "Q432"), d1h, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1♥): 5♠ 13 HCP → 2♥ cuebid transfer", zd1, TestHelpers.ElOlustur("AQJ32", "32", "K32", "K32"), d1h, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1♥): 5♣ 11 HCP → 2♦ transfer cuebid", zd1, TestHelpers.ElOlustur("32", "432", "K32", "AKJ32"), d1h, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1♥): dengeli 9 HCP ♥ stoperli → 1NT", zd1, TestHelpers.ElOlustur("K32", "Q32", "432", "KJ32"), d1h, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1♥): dengeli 13 HCP ♥ stoperli → 3NT", zd1, TestHelpers.ElOlustur("K32", "AQ2", "432", "KJ32"), d1h, "3NT", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1♠): 4♥ 8 HCP → Dbl", zd1, TestHelpers.ElOlustur("432", "KJ98", "Q32", "Q32"), d1s, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1♠): 5♥ 13 HCP → 2♦ transfer", zd1, TestHelpers.ElOlustur("32", "AQJ32", "K32", "K32"), d1s, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1♠): 5♣ 11 HCP → 2♥ transfer cuebid", zd1, TestHelpers.ElOlustur("32", "432", "K32", "AKJ32"), d1s, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1NT): 5♠4♥ zayıf → 2♣ majörler", zd1, TestHelpers.ElOlustur("KJ932", "Q432", "32", "32"), d1n, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1NT): 5♦ zayıf → 2♦", zd1, TestHelpers.ElOlustur("32", "432", "KJ932", "432"), d1n, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(1NT): 6♠ 10 HCP → 2NT Lebensohl", zd1, TestHelpers.ElOlustur("AQJ932", "32", "K32", "32"), d1n, "2NT", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(2♣): 5♠ 9 HCP → 2♠ NF", zd1, TestHelpers.ElOlustur("AQJ32", "32", "K32", "432"), d1c, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(2♣): 4♦ zayıf → 2♦", zd1, TestHelpers.ElOlustur("32", "432", "KJ32", "Q432"), d1c, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(2♣): dengeli 13 HCP ♣ stoperli → 3NT", zd1, TestHelpers.ElOlustur("K32", "Q32", "AJ32", "K32"), d1c, "3NT", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(2♠): 4♥ 10 HCP → Dbl", zd1, TestHelpers.ElOlustur("32", "KJ98", "AQ32", "432"), new[] { "1♦", "2♠" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♦-(2♥): 5♠ 9 HCP → 2♠ NF", zd1, TestHelpers.ElOlustur("AQJ32", "32", "K32", "432"), new[] { "1♦", "2♥" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1♦-(1♥)-2♣ transfer → 2♦", zd1, TestHelpers.ElOlustur("AK32", "Q32", "KJ32", "Q2"), new[] { "1♦", "1♥", "2♣", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1♦-(1♠)-2♦ transfer → 2♥", zd1, TestHelpers.ElOlustur("AK32", "Q32", "KJ32", "Q2"), new[] { "1♦", "1♠", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1♦-(1♠)-2♥ cuebid transfer → 3♣", zd1, TestHelpers.ElOlustur("AK32", "Q32", "KJ32", "Q2"), new[] { "1♦", "1♠", "2♥", "Pas" }, "3♣", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Zayıf NT: 1♥ açışına araya giriş cevapları (s.115-117) ──");
            var zh1 = new BricKartOyunu.Class.Bidding.OpeningSystems.OneHeart.OneHeartZ_Araya();
            TW("1♥-(Dbl): 4♠ 8 HCP → 1♠", zh1, TestHelpers.ElOlustur("KJ98", "32", "Q432", "Q32"), new[] { "1♥", "Dbl" }, "1♠", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(Dbl): 3♥ 8 HCP → 2♦ yapıcı", zh1, TestHelpers.ElOlustur("32", "Q32", "KJ432", "Q32"), new[] { "1♥", "Dbl" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(Dbl): 3♥ 5 HCP → 2♥ tutuş", zh1, TestHelpers.ElOlustur("432", "Q32", "J432", "Q32"), new[] { "1♥", "Dbl" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(Dbl): 5♣ 6 HCP → 1NT transfer", zh1, TestHelpers.ElOlustur("32", "32", "Q432", "KJ432"), new[] { "1♥", "Dbl" }, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(Dbl): 5♦ 6 HCP → 2♣ transfer", zh1, TestHelpers.ElOlustur("32", "32", "KJ432", "Q432"), new[] { "1♥", "Dbl" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(Dbl): 13 HCP ♥ yok → RDbl", zh1, TestHelpers.ElOlustur("K32", "2", "AQ32", "KJ432"), new[] { "1♥", "Dbl" }, "RDbl", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(Dbl): 4♥ 10 HCP dengeli → 2NT", zh1, TestHelpers.ElOlustur("K32", "KJ32", "Q32", "J32"), new[] { "1♥", "Dbl" }, "2NT", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(Dbl): 4♥ tek kart ♠ 12 HCP → 3♠", zh1, TestHelpers.ElOlustur("2", "AQ32", "KJ32", "Q432"), new[] { "1♥", "Dbl" }, "3♠", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(1♠): 3♥ 16 HCP → 2♠ cuebid FG", zh1, TestHelpers.ElOlustur("32", "KQ2", "AJ32", "AQ32"), new[] { "1♥", "1♠" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(1♠): 3♥ 8 HCP → 2♦ yapıcı", zh1, TestHelpers.ElOlustur("32", "Q32", "KJ432", "Q32"), new[] { "1♥", "1♠" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(1♠): ♥ yok 8 HCP → Dbl", zh1, TestHelpers.ElOlustur("32", "2", "KQ432", "QJ432"), new[] { "1♥", "1♠" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(1NT): 6♠ 8 HCP → 2NT Lebensohl", zh1, TestHelpers.ElOlustur("KQJ432", "32", "Q32", "32"), new[] { "1♥", "1NT" }, "2NT", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(1NT): 5♦ zayıf → 2♦", zh1, TestHelpers.ElOlustur("32", "432", "KJ932", "432"), new[] { "1♥", "1NT" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(1NT): 14 HCP ♥ yok dengeli → Dbl", zh1, TestHelpers.ElOlustur("KJ2", "32", "AQ32", "KJ32"), new[] { "1♥", "1NT" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(2♣): 3♥ 4 HCP → 2♥ tutuş", zh1, TestHelpers.ElOlustur("432", "Q32", "J432", "QJ2"), new[] { "1♥", "2♣" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(2♣): 5♠ 10 HCP → 2♠ NF", zh1, TestHelpers.ElOlustur("AQJ32", "2", "K432", "432"), new[] { "1♥", "2♣" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(2♣): 4♠ 9 HCP → Dbl", zh1, TestHelpers.ElOlustur("KJ98", "32", "Q432", "K32"), new[] { "1♥", "2♣" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(2♦): 3♥ 16 HCP → 3♦ cuebid FG", zh1, TestHelpers.ElOlustur("32", "KQ2", "AJ32", "AQ32"), new[] { "1♥", "2♦" }, "3♦", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(2♥ Michaels): 3♥ 16 HCP → 2♠ cuebid FG", zh1, TestHelpers.ElOlustur("32", "KQ2", "AJ32", "AQ32"), new[] { "1♥", "2♥" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(2♠): 3♥ 11 HCP → 2NT davet", zh1, TestHelpers.ElOlustur("432", "KJ2", "AQ32", "J32"), new[] { "1♥", "2♠" }, "2NT", sonucEkle, ref gecen, ref kalan);
            TW("1♥-(2♠): ♥ yok 13 HCP → Dbl", zh1, TestHelpers.ElOlustur("432", "32", "AQ32", "KJ32"), new[] { "1♥", "2♠" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1♥-(Dbl)-1NT transfer → 2♣", zh1, TestHelpers.ElOlustur("AK32", "KQ432", "32", "Q2"), new[] { "1♥", "Dbl", "1NT", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1♥-(1♠)-2♣ transfer → 2♦", zh1, TestHelpers.ElOlustur("A32", "KQ432", "32", "AQ2"), new[] { "1♥", "1♠", "2♣", "Pas" }, "2♦", sonucEkle, ref gecen, ref kalan);
            sonucEkle("── Zayıf NT: 1♠ açışına araya giriş cevapları (s.118-119) ──");
            var zs1 = new BricKartOyunu.Class.Bidding.OpeningSystems.OneSpade.OneSpadeZ_Araya();
            TW("1♠-(Dbl): 3♠ 8 HCP → 2♥ yapıcı", zs1, TestHelpers.ElOlustur("Q32", "32", "KJ432", "Q32"), new[] { "1♠", "Dbl" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(Dbl): 3♠ 4 HCP → 2♠ tutuş", zs1, TestHelpers.ElOlustur("Q32", "432", "J432", "J32"), new[] { "1♠", "Dbl" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(Dbl): 5♣ zayıf → 1NT transfer", zs1, TestHelpers.ElOlustur("32", "432", "Q32", "KJ432"), new[] { "1♠", "Dbl" }, "1NT", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(Dbl): 5♦ zayıf → 2♣ transfer", zs1, TestHelpers.ElOlustur("32", "432", "KJ432", "Q32"), new[] { "1♠", "Dbl" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(Dbl): 5♥ zayıf → 2♦ transfer", zs1, TestHelpers.ElOlustur("32", "KJ432", "432", "Q32"), new[] { "1♠", "Dbl" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(Dbl): ♠ yok 13 HCP → RDbl", zs1, TestHelpers.ElOlustur("2", "K32", "AQ32", "KJ432"), new[] { "1♠", "Dbl" }, "RDbl", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(Dbl): 4♠ dengeli 10 HCP → 2NT", zs1, TestHelpers.ElOlustur("AJ32", "Q32", "Q32", "J32"), new[] { "1♠", "Dbl" }, "2NT", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(Dbl): 4♠ tek kart ♥ 12 HCP → 4♥", zs1, TestHelpers.ElOlustur("AQ32", "2", "KJ32", "Q432"), new[] { "1♠", "Dbl" }, "4♥", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(1NT): 6♥ 8 HCP → 2NT Lebensohl", zs1, TestHelpers.ElOlustur("32", "KQJ432", "Q32", "32"), new[] { "1♠", "1NT" }, "2NT", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(1NT): 5♦ zayıf → 2♦", zs1, TestHelpers.ElOlustur("32", "432", "KJ932", "432"), new[] { "1♠", "1NT" }, "2♦", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(1NT): 15 HCP ♠ yok → Dbl ceza", zs1, TestHelpers.ElOlustur("K2", "Q32", "AQ32", "KJ32"), new[] { "1♠", "1NT" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(2♣): 3♠ 8 HCP → 2♠ tutuş", zs1, TestHelpers.ElOlustur("Q32", "Q32", "J432", "QJ2"), new[] { "1♠", "2♣" }, "2♠", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(2♣): 5♥ 11 HCP → 2♥", zs1, TestHelpers.ElOlustur("32", "AKQ32", "Q32", "432"), new[] { "1♠", "2♣" }, "2♥", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(2♦): 4♥ 9 HCP → Dbl", zs1, TestHelpers.ElOlustur("K2", "KJ98", "Q432", "432"), new[] { "1♠", "2♦" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(2♦): 3♠ 16 HCP → 3♦ cuebid FG", zs1, TestHelpers.ElOlustur("K32", "Q32", "AJ32", "AQ2"), new[] { "1♠", "2♦" }, "3♦", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(2♥): 3♠ 11 HCP → 2NT davet", zs1, TestHelpers.ElOlustur("KJ2", "432", "AQ32", "J32"), new[] { "1♠", "2♥" }, "2NT", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(2♥): ♠ yok 10 HCP → Dbl", zs1, TestHelpers.ElOlustur("2", "432", "AQ32", "KJ432"), new[] { "1♠", "2♥" }, "Dbl", sonucEkle, ref gecen, ref kalan);
            TW("1♠-(2♠ Michaels): 3♠ 16 HCP → 3♥ cuebid FG", zs1, TestHelpers.ElOlustur("K32", "Q32", "AJ32", "AQ2"), new[] { "1♠", "2♠" }, "3♥", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1♠-(Dbl)-1NT transfer → 2♣", zs1, TestHelpers.ElOlustur("AK432", "Q32", "K32", "Q2"), new[] { "1♠", "Dbl", "1NT", "Pas" }, "2♣", sonucEkle, ref gecen, ref kalan);
            TW("açan: 1♠-(Dbl)-2♦ transfer → 2♥", zs1, TestHelpers.ElOlustur("AK432", "Q32", "K32", "Q2"), new[] { "1♠", "Dbl", "2♦", "Pas" }, "2♥", sonucEkle, ref gecen, ref kalan);
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

        // 1♦ cevabı (IKonvansiyon): ortak 1♦ açtı, sağdaki Pas → ben (Güney), zayıf NT
        private static void TD(string ad, List<Card> el, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, new[] { "1♦", "Pas" });
            durum.Anlasma.StrongNT = false;
            var k = new BricKartOyunu.Class.Bidding.OpeningSystems.OneDiamond.OneDiamond_Answer();
            string sonuc = k.UygunMu(durum) ? k.TeklifVer(durum) : null;
            if (sonuc == beklenen) { sonucEkle($"✅ DOĞRU: {sonuc}"); gecen++; }
            else { sonucEkle($"❌ HATA: Beklenen {beklenen}, gelen {sonuc ?? "(devreye girmedi)"}"); kalan++; }
        }

        // 1NT cevabı (IKonvansiyon): ortak 1NT açtı, sağdaki Pas → ben (Güney), zayıf NT
        private static void TN(string ad, List<Card> el, string beklenen, Action<string> sonucEkle, ref int gecen, ref int kalan)
        {
            sonucEkle($"▶ {ad}");
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, new[] { "1NT", "Pas" });
            durum.Anlasma.StrongNT = false;
            var k = new BricKartOyunu.Class.Bidding.OpeningSystems.OneNT.OneNT_Answer();
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
