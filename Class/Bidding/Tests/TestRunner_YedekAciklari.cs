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
            sonucEkle("  ÖZET: ✅ " + _gecen + " geçti, ❌ " + _kalan + " başarısız");
            sonucEkle("════════════════════════════════════════");
            return _kalan;
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
