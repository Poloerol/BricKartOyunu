using System;
using System.Collections.Generic;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;

namespace BricKartOyunu.Tests
{
    /// <summary>
    /// Temel Mantık yedeğinin paylaştığı açıkları kapatan kuralların testleri (Motor Karnesi, 30.000 bord):
    ///  • 1x-1Y-2x (açıcının minimum tekrarı) sonrası cevapçı 12-13 HP ile Pas yerine 3NT (-310 IMP);
    ///  • 1M-1NT-3M (açıcının daveti) sonrası cevapçı 7+ HP ile 4M (-54 IMP);
    ///  • zayıf iki açılışına ortağın 3 seviyede minör müdahalesi sonrası 9+ HP ile 3NT / 5m (-352 IMP).
    /// Güney daima sıradaki oyuncudur; geçmiş Güney'in sağındaki oyuncuyla biter.
    /// </summary>
    public static class TestRunner_YedekAciklari
    {
        private static int _gecen, _kalan;
        private static Action<string> _yaz;

        public static int TumTestleriCalistir(Action<string> sonucEkle)
        {
            _gecen = 0; _kalan = 0; _yaz = sonucEkle;
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  YEDEK AÇIKLARI TESTLERİ");
            sonucEkle("════════════════════════════════════════");
            // Cevapçı İkinci Tur: 1♦ (ortak) - 1♠ (ben) - 2♦ (ortak)
            U("YA1 1D-1S-2D 13hcp dengeli -> 3NT", new CevapciIkinciTur(), TestHelpers.ElOlustur("AQ874", "K98", "32", "KJ3"), "3NT", "1♦", "Pas", "1♠", "Pas", "2♦", "Pas");
            U("YA2 1D-1S-2D 12hcp dengeli -> 3NT", new CevapciIkinciTur(), TestHelpers.ElOlustur("AJ874", "K98", "32", "KJ3"), "3NT", "1♦", "Pas", "1♠", "Pas", "2♦", "Pas");
            U("YA3 1D-1S-2D 11hcp -> davet yok (Pas)", new CevapciIkinciTur(), TestHelpers.ElOlustur("AT874", "QJ98", "2", "A32"), null, "1♦", "Pas", "1♠", "Pas", "2♦", "Pas");
            U("YA4 1H-1S-2H 12hcp 3 kupa -> 3H (uyum önce)", new CevapciIkinciTur(), TestHelpers.ElOlustur("AJ874", "K98", "32", "KJ3"), "3♥", "1♥", "Pas", "1♠", "Pas", "2♥", "Pas");
            // Oyun Daveti Kabulü: 1♠ (ortak) - 1NT (ben, 6-9) - 3♠ (ortak, davet)
            U("YA5 1S-1NT-3S 8hcp -> 4S", new OyunDavetiKabul(), TestHelpers.ElOlustur("K72", "Q82", "J762", "Q53"), "4♠", "1♠", "Pas", "1NT", "Pas", "3♠", "Pas");
            U("YA6 1S-1NT-3S 6hcp -> Pas", new OyunDavetiKabul(), TestHelpers.ElOlustur("K72", "982", "J762", "Q53"), null, "1♠", "Pas", "1NT", "Pas", "3♠", "Pas");
            // Müdahale Oyun Cevabı: rakip 2♠, ortak 3♦, sağ rakip Pas
            U("YA7 (2S)-3D-P 13hcp ♠ durdurucu -> 3NT", new MudahaleOyunCevap(), TestHelpers.ElOlustur("AT98", "AJ84", "T82", "A5"), "3NT", "2♠", "3♦", "Pas");
            U("YA8 (2S)-3D-P 14hcp ♠ yok, 4 karo -> 5D", new MudahaleOyunCevap(), TestHelpers.ElOlustur("432", "AKJ4", "KJ82", "Q5"), "5♦", "2♠", "3♦", "Pas");
            U("YA9 (2S)-3D-P 5hcp -> Pas", new MudahaleOyunCevap(), TestHelpers.ElOlustur("T32", "J842", "Q82", "A54"), null, "2♠", "3♦", "Pas");
            U("YA10 (2H)-3C-P 10hcp ♥ durdurucu -> 3NT", new MudahaleOyunCevap(), TestHelpers.ElOlustur("J974", "KQJ2", "A632", "3"), "3NT", "2♥", "3♣", "Pas");
            // 3 seviye majör müdahalesi: (2♠) - 3♥ (ortak) - Pas
            U("YA11 (2S)-3H-P 9hcp 3 kupa -> 4H", new MudahaleOyunCevap(), TestHelpers.ElOlustur("T32", "KJ4", "Q852", "A94"), "4♥", "2♠", "3♥", "Pas");
            U("YA12 (2S)-3H-P 9hcp 2 kupa -> Pas", new MudahaleOyunCevap(), TestHelpers.ElOlustur("T32", "K4", "QJ852", "A94"), null, "2♠", "3♥", "Pas");
            // Zayıf ikiye takeout kontra: (2♥) - Dbl (ben) - Pas - 3♣ (ortak) - Pas
            U("YA13 (2H)-X-P-3C-P 17hcp ♥ durdurucu -> 3NT", new MudahaleOyunCevap(), TestHelpers.ElOlustur("AQ98", "KJ4", "AT7", "K82"), "3NT", "2♥", "Dbl", "Pas", "3♣", "Pas");
            U("YA14 (2H)-X-P-3C-P 17hcp ♥ yok -> uygun değil", new MudahaleOyunCevap(), TestHelpers.ElOlustur("AQJ8", "432", "AKT", "K82"), null, "2♥", "Dbl", "Pas", "3♣", "Pas");
            // Açıcı 1NT sonrası: 1♦ - 1NT (ortak) - Pas
            U("YA15 1D-1NT 20hcp dengeli -> 3NT", new AciciBirNTSonrasi(), TestHelpers.ElOlustur("AQ4", "KJ5", "AKJ3", "Q82"), "3NT", "1♦", "Pas", "1NT", "Pas");
            U("YA16 1D-1NT 14hcp -> Pas", new AciciBirNTSonrasi(), TestHelpers.ElOlustur("KQ4", "J95", "AQJ32", "82"), null, "1♦", "Pas", "1NT", "Pas");
            // 4NT'ye standart RKCB 1430 cevabı: void kilit kart sayılmaz, cevabı değiştirmez (1♠-2♣-2♥-3♠-4♣-4NT, Güney sinekte void)
            M("YA17 4NT'ye 1 kilit kart (♠A) + ♣ void -> 5♣ (1430: 1/4)", TestHelpers.ElOlustur("A8765", "KQT87", "QJ2", ""), "5♣", "1♠", "Pas", "2♣", "Pas", "2♥", "Pas", "3♠", "Pas", "4♣", "Pas", "4NT", "Pas");
            M("YA18 4NT'ye 2 kilit kart (♠A, ♠K; ♠Q yok) + ♣ void -> 5♥", TestHelpers.ElOlustur("AK765", "KQT87", "QJ2", ""), "5♥", "1♠", "Pas", "2♣", "Pas", "2♥", "Pas", "3♠", "Pas", "4♣", "Pas", "4NT", "Pas");
            M("YA19 4NT'ye 3 kilit kart (♠A, ♠K, ♥A) + ♣ void -> 5♦ (1430: 0/3)", TestHelpers.ElOlustur("AK765", "AQT87", "QJ2", ""), "5♦", "1♠", "Pas", "2♣", "Pas", "2♥", "Pas", "3♠", "Pas", "4♣", "Pas", "4NT", "Pas");
            // Stayman devamı: majör fiti yoksa 17+ HP ile 3NT yerine 6NT (1NT 15-17 ile birleşik şlem bölgesi)
            M("YA21 1NT-2♣-2♥, 18 HP kupa fiti yok -> 6NT", TestHelpers.ElOlustur("A973", "AK9", "K843", "A7"), "6NT", "1NT", "Pas", "2♣", "Pas", "2♥", "Pas");
            M("YA22 1NT-2♣-2♥, 17 HP kupa fiti yok -> 6NT", TestHelpers.ElOlustur("A973", "AK9", "K843", "K7"), "6NT", "1NT", "Pas", "2♣", "Pas", "2♥", "Pas");
            M("YA23 1NT-2♣-2♥, 16 HP kupa fiti yok -> 3NT", TestHelpers.ElOlustur("A973", "AK9", "K843", "Q7"), "3NT", "1NT", "Pas", "2♣", "Pas", "2♥", "Pas");
            // Ortak 2NT (18-19) dedi, fit minörde ama el dengeli: 6♦ yerine 6NT (aynı löve, daha çok puan)
            M("YA24 1♦-1♥-2NT, 17 HP dengeli karo fiti -> 6NT (6♦ değil)", TestHelpers.ElOlustur("K2", "QJT3", "AQJT", "KJ6"), "6NT", "1♦", "Pas", "1♥", "Pas", "2NT", "Pas");
            // 2NT açılışına uzun majörlü cevaplar (açıcıda her renkte en az 2 kart): 5'li = doğal 3M, 6+ = fit (slam değerinde 6M)
            M("YA25 2NT-?, 5'li kupa 8 HP -> 3♦ (Jacoby transfer)", TestHelpers.ElOlustur("2", "AKQ96", "J87", "KJ76"), "3♦", "2NT", "Pas");
            M("YA26 2NT-?, 7'li kupa 14 HP -> 6♥ (fit bulundu, slam)", TestHelpers.ElOlustur("", "AKQ9632", "J8", "KJ76"), "6♥", "2NT", "Pas");
            M("YA27 2NT-3♦ (transfer), açıcı 3♥ ile kabul eder", TestHelpers.ElOlustur("A82", "K32", "J87", "KJ76"), "3♥", "Pas", "Pas", "2NT", "Pas", "3♦", "Pas");
            M("YA28 2NT-3♦-3♥, 5'li kupa 8 HP -> 3NT (açıcı seçer)", TestHelpers.ElOlustur("2", "AKQ96", "J87", "KJ76"), "3NT", "2NT", "Pas", "3♦", "Pas", "3♥", "Pas");
            M("YA29 2NT-3♦-3♥-3NT, açıcıda 3 kupa -> 4♥", TestHelpers.ElOlustur("A82", "K32", "J87", "KJ76"), "4♥", "Pas", "Pas", "2NT", "Pas", "3♦", "Pas", "3♥", "Pas", "3NT", "Pas");
            M("YA49 2NT-3♥ (maça transferi), açıcı 3♠ ile kabul eder", TestHelpers.ElOlustur("A82", "A32", "J87", "KJ76"), "3♠", "Pas", "Pas", "2NT", "Pas", "3♥", "Pas");
            M("YA50 2NT-3♦-3♥-3NT, açıcıda 2 kupa -> Pas", TestHelpers.ElOlustur("A82", "K3", "J872", "KJ76"), "Pas", "Pas", "Pas", "2NT", "Pas", "3♦", "Pas", "3♥", "Pas", "3NT", "Pas");
            // 2♣-2♦-2NT (açıcı dengeli 23+, oyuna kadar zorlayıcı): 5'li majör = doğal 3M, 6+ majör = oyun (4M) ya da 9+ HP ile 6M
            M("YA30 2♣-2♦-2NT, 5'li kupa -> 3♥", TestHelpers.ElOlustur("J8", "QJ974", "832", "J76"), "3♥", "2♣", "Pas", "2♦", "Pas", "2NT", "Pas");
            M("YA31 2♣-2♦-2NT, 7'li kupa 3 HP -> 4♥ (fit bulundu, oyun)", TestHelpers.ElOlustur("J87", "QJ9742", "32", "J7"), "4♥", "2♣", "Pas", "2♦", "Pas", "2NT", "Pas");
            M("YA32 2♣-2♦-2NT-3♥, açıcıda 3 kupa -> 4♥", TestHelpers.ElOlustur("AK3", "KQ2", "AQ4", "QJ52"), "4♥", "2♣", "Pas", "2♦", "Pas", "2NT", "Pas", "3♥", "Pas");
            // 2♣-2♦ sonrası açıcının tekrarı: 2M = 5+ majör + 23+ (dengesiz), 3M = 17-22 HP uzun majör, dengeli = 2NT
            M("YA33 2♣-2♦, 25 HP 5-4-3-1 5♠ -> 2♠", TestHelpers.ElOlustur("AKQ95", "AKQ4", "AK3", "6"), "2♠", "2♣", "Pas", "2♦", "Pas");
            M("YA34 2♣-2♦, 25 HP 5-4-3-1 5♥ -> 2♥", TestHelpers.ElOlustur("6", "AKQ95", "AK3", "AKQ4"), "2♥", "2♣", "Pas", "2♦", "Pas");
            M("YA35 2♣-2♦, 17 HP 6♠ (AKQJ96) -> 3♠", TestHelpers.ElOlustur("AKQJ96", "3", "K42", "A76"), "3♠", "2♣", "Pas", "2♦", "Pas");
            M("YA36 2♣-2♦, 24 HP 5-3-3-2 5♠ dengeli -> 2NT", TestHelpers.ElOlustur("AKQ95", "AKJ", "Q43", "KQ"), "2NT", "2♣", "Pas", "2♦", "Pas");
            // 1♣/1♦ + sıçramalı yeni renk (18+ HP) sonrası açıcı: BB akış şeması "Rebid after jump shift"
            M("YA37 1♣-2♠, 19 HP oynanmamış 4'lü karo -> 3♦ (3NT değil)", TestHelpers.ElOlustur("KJ", "K", "QJ83", "AKQT43"), "3♦", "1♣", "Pas", "2♠", "Pas");
            M("YA38 1♣-2♠, 3 maça destek -> 3♠", TestHelpers.ElOlustur("KJ4", "K", "QJ8", "AKQT43"), "3♠", "1♣", "Pas", "2♠", "Pas");
            M("YA39 1♣-2♠, destek yok, 4'lü renk yok, 6'lı sinek -> 3♣", TestHelpers.ElOlustur("K2", "Q3", "J32", "AKQ943"), "3♣", "1♣", "Pas", "2♠", "Pas");
            // Rakibin zayıf iki açılışına ortağın 2NT müdahalesi sonrası doğal cevap
            M("YA40 (2♦)-2NT-(Pas), 13 HP dengeli -> 3NT", TestHelpers.ElOlustur("KQ85", "AQ4", "873", "Q94"), "3NT", "Pas", "2♦", "2NT", "Pas");
            M("YA41 (2♦)-2NT-(Pas), 11 HP 6'lı maça -> 4♠", TestHelpers.ElOlustur("KJT984", "AT962", "K", "5"), "4♠", "Pas", "2♦", "2NT", "Pas");
            M("YA42 (2♥)-2NT-(Pas), 5 HP dengeli -> Pas", TestHelpers.ElOlustur("9852", "Q94", "K862", "Q9"), "Pas", "Pas", "2♥", "2NT", "Pas");
            M("YA43 (2♠)-2NT-(Pas), 19 HP dengeli -> 6NT", TestHelpers.ElOlustur("A5", "AKQ4", "KJ86", "KQ4"), "6NT", "Pas", "2♠", "2NT", "Pas");
            // Negatif kontradan sonra rakip müdahale rengini yükseltince açıcı, kontranın gösterdiği majöre fit varsa onu söyler
            M("YA44 1♦-(1♠)-Kontra-(2♠), 19 HP 4'lü kupa -> 4♥", TestHelpers.ElOlustur("QJT", "AKQT", "AKT8", "32"), "4♥", "1♦", "1♠", "Dbl", "2♠");
            M("YA45 1♦-(1♠)-Kontra-(2♠), 13 HP 4'lü kupa -> 3♥", TestHelpers.ElOlustur("QJ5", "KT74", "AKT82", "3"), "3♥", "1♦", "1♠", "Dbl", "2♠");
            // 1m-(Pas)-1M-(1NT): açıcı 6+ kartlı rengini tekrarlar; 7'li renk ve 14+ HP ile 3 seviyede davet (Pas felaketti)
            M("YA51 1♣-Pas-1♥-(1NT), 14 HP 7'li sinek -> 3♣", TestHelpers.ElOlustur("T3", "A4", "A7", "KQJ9875"), "3♣", "1♣", "Pas", "1♥", "1NT");
            M("YA52 1♣-Pas-1♥-(1NT), 13 HP 6'lı sinek -> 2♣", TestHelpers.ElOlustur("T3", "K4", "A73", "KQJ985"), "2♣", "1♣", "Pas", "1♥", "1NT");
            // Daha önce pas geçmiş cevapçı da ortağın 2NT açılışına Puppet Stayman (3♣) ile devam eder (KendiTeklifleri Pas'ı da sayıyordu)
            M("YA46 Pas-Pas-2NT-Pas, 4'lü maça 7 HP -> 3♣ (Puppet)", TestHelpers.ElOlustur("AJ84", "96", "653", "Q965"), "3♣", "Pas", "Pas", "2NT", "Pas");
            // Ortağın 1NT müdahalesine (rakip 1♠ açtı) Jacoby transfer: 5'li kupa → 2♦, müdahaleci 2♥ ile tamamlar
            M("YA47 (1♠)-1NT-(Pas), 5'li kupa 4 HP -> 2♦ transfer", TestHelpers.ElOlustur("942", "A9852", "843", "75"), "2♦", "Pas", "1♠", "1NT", "Pas");
            M("YA48 (1♠)-1NT-(Pas)-2♦: müdahaleci 2♥ ile tamamlar", TestHelpers.ElOlustur("AQT", "QJ7", "T965", "KQJ"), "2♥", "Pas", "Pas", "Pas", "1♠", "1NT", "Pas", "2♦", "Pas");
            // Ortağın 1NT müdahalesine Stayman (2♣): 4'lü majör, 9 HP; müdahaleci 4'lü majörünü söyler; uyumda 3M/4M
            M("YA53 (1♦)-1NT-(Pas), 4'lü maça 8 HP -> 2♣ Stayman", TestHelpers.ElOlustur("KJ84", "962", "A53", "654"), "2♣", "Pas", "1♦", "1NT", "Pas");
            M("YA54 (1♦)-1NT-(Pas)-2♣: 4'lü kupa -> 2♥", TestHelpers.ElOlustur("AQT", "KJ72", "KQ5", "J96"), "2♥", "Pas", "Pas", "Pas", "1♦", "1NT", "Pas", "2♣", "Pas");
            M("YA55 (1♦)-1NT-(Pas)-2♣-2♠: 4'lü maça 11 HP -> 4♠", TestHelpers.ElOlustur("KJ84", "962", "A53", "KQ4"), "4♠", "Pas", "1♦", "1NT", "Pas", "2♣", "Pas", "2♠", "Pas");
            // 1♠-1NT-2♥ sonrası cevapçı: ♥'da 1 kart, ♠'da 2 kart → 2♠ (ilk renge tercih; Pas 4-1 uyumda kalmaktı)
            M("YA57 1♠-1NT-2♥, ♥ tek ♠ çift, 6♣ 9 HP -> 2♠ (tercih)", TestHelpers.ElOlustur("T5", "T", "Q976", "AQJT32"), "2♠", "Pas", "1♠", "Pas", "1NT", "Pas", "2♥", "Pas");
            M("YA58 1♠-1NT-2♥, 3 kupa -> Pas (uyum)", TestHelpers.ElOlustur("T5", "QT7", "Q97", "AQJ32"), "Pas", "Pas", "1♠", "Pas", "1NT", "Pas", "2♥", "Pas");
            M("YA59 1♥-1♠ sonrası açıcı 5-5 (♥+♦) -> 2♦ (Forcing NT cevapçı bloğuna düşmez)", TestHelpers.ElOlustur("76", "Q10965", "AKJ107", "Q"), "2♦", "1♥", "Pas", "1♠", "Pas");
            M("YA60 1♥-2♥-2♠(deneme)-3♥ (yardım yok) -> açıcı Pas (4♥ değil)", TestHelpers.ElOlustur("T86", "AQJ97", "AQ", "A42"), "Pas", "1♥", "Pas", "2♥", "Pas", "2♠", "Pas", "3♥", "Pas");
            var kozDurum = TestHelpers.DurumOlustur(TestHelpers.ElOlustur("A8765", "KQT87", "QJ2", ""), Player.Guney, "1♠", "Pas", "2♣", "Pas", "2♥", "Pas", "3♠", "Pas", "4♣", "Pas");
            string koz = kozDurum.AnlasilanKoz();
            bool kozTamam = koz == "Maça";
            if (kozTamam) _gecen++; else _kalan++;
            _yaz((kozTamam ? "✅ " : "❌ ") + "YA20 4. seviye 4♣ (ortağın doğal rengi) kozu değiştirmez → " + koz + (kozTamam ? "" : "   [beklenen: Maça]"));
            sonucEkle("  ÖZET: ✅ " + _gecen + " geçti, ❌ " + _kalan + " başarısız");
            sonucEkle("════════════════════════════════════════");
            return _kalan;
        }

        private static void M(string ad, List<Card> el, string beklenen, params string[] gecmis)
        {
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            string r = new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan()).TeklifVer(durum);
            bool gecti = r == beklenen;
            if (gecti) _gecen++; else _kalan++;
            _yaz((gecti ? "✅ " : "❌ ") + ad + " → " + (r ?? "(null)") + (gecti ? "" : "   [beklenen: " + beklenen + "]"));
        }

        private static void U(string ad, IKonvansiyon k, List<Card> el, string beklenen, params string[] gecmis)
        {
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            string r = k.UygunMu(durum) ? k.TeklifVer(durum) : null;
            bool gecti = r == beklenen;
            if (gecti) _gecen++; else _kalan++;
            _yaz((gecti ? "✅ " : "❌ ") + ad + " → " + (r ?? "(uygun değil)") + (gecti ? "" : "   [beklenen: " + (beklenen ?? "(uygun değil)") + "]"));
        }
    }
}
