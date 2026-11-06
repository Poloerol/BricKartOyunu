using System;
using System.Collections.Generic;
using BricKartOyunu.Class;

namespace BricKartOyunu.Tests
{
    /// <summary>Çift dummy modülü testleri: ihale ayrıştırma, tablo/par, değerlendirme, yerleşik çözücü ≡ DDS.</summary>
    public static class TestRunner_DD
    {
        private static int _gecen, _kalan;
        private static Action<string> _yaz;

        private static void K(string ad, bool ok, string ayrinti = "")
        {
            if (ok) { _gecen++; _yaz("  ✓ " + ad); }
            else { _kalan++; _yaz("  ✗ YANLIŞ " + ad + " " + ayrinti); }
        }

        private static ulong Maske(string maca, string kupa, string karo, string sinek)
        {
            ulong m = 0; string[] r = { maca, kupa, karo, sinek };
            for (int s = 0; s < 4; s++)
                foreach (char c in r[s].Replace("-", ""))
                    m |= 1UL << (s * 16 + ("23456789TJQKA".IndexOf(c)));
            return m;
        }

        public static int TumTestleriCalistir(Action<string> sonucEkle)
        {
            _gecen = 0; _kalan = 0; _yaz = sonucEkle;
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  ÇİFT DUMMY TESTLERİ");
            sonucEkle("════════════════════════════════════════");

            // ihale ayrıştırma (dağıtıcı N)
            var a = IhaleSonucu.Coz(new List<string> { "1♠", "Pas", "4♦", "Pas", "Pas", "Pas" }, 0);
            K("DD1 1♠-P-4♦-P-P-P: 4♦ , deklaran Güney(2)", !a.Pas && a.Seviye == 4 && a.Koz == 2 && a.Deklaran == 2, a.Seviye + "/" + a.Koz + "/" + a.Deklaran);
            var b = IhaleSonucu.Coz(new List<string> { "Pas", "1♥", "Dbl", "Pas", "2♥", "Pas", "Pas", "Pas" }, 3);
            K("DD2 kontra sonrası kontrat 2♥, Batı dağıtıcı", !b.Pas && b.Seviye == 2 && b.Koz == 1 && b.Kontr == KontrDurumu.Yok, b.Seviye + "/" + b.Koz);
            var c = IhaleSonucu.Coz(new List<string> { "1NT", "Dbl", "RDbl", "Pas", "Pas", "Pas" }, 2);
            K("DD3 1NT Dbl RDbl: Güney 1NT, XX", c.Deklaran == 2 && c.Kontr == KontrDurumu.SKontr);
            K("DD4 dört pas", IhaleSonucu.Coz(new List<string> { "Pas", "Pas", "Pas", "Pas" }, 1).Pas);
            var d = IhaleSonucu.Coz(new List<string> { "1♣", "Pas", "1♠", "Pas", "2♣", "Pas", "3♣", "Pas", "Pas", "Pas" }, 0);
            K("DD5 ilk söyleyen deklaran (♣: Kuzey)", d.Koz == 3 && d.Deklaran == 0 && d.Seviye == 3);

            // örnek bord (K-G vul): 6♠ yapılır
            var eller = new ulong[]
            {
                Maske("AKQ32", "Q97", "J82T".Replace("T", "T"), "3"),
                Maske("4", "AT63", "A7654", "T94"),
                Maske("JT87", "KJ42", "-", "AKQ82"),
                Maske("965", "85", "KQ93", "J765")
            };
            // not: karo J10 8 2 → "JT82"
            eller[0] = Maske("AKQ32", "Q97", "JT82", "3");
            if (!DDMotor.DdsVar)
            {
                sonucEkle("  (DDS yüklenemedi: " + DDMotor.DdsHata + " — tablo testleri atlandı)");
            }
            else
            {
                var t = DDMotor.Hesapla(eller);
                K("DD6 kaynak DDS", t.Kaynak == "DDS");
                K("DD7 Güney ♠ löve = 12", t.Al(0, 2) == 12, t.Al(0, 2).ToString());
                K("DD8 Güney ♦ kozunda 4♦ batar (<10)", t.Al(2, 2) < 10, t.Al(2, 2).ToString());
                var par = ParHesabi.Hesapla(t, 0, false, false);
                K("DD9 par K-G 6♠ +980", !par.Pas && par.Seviye == 6 && par.Koz == 0 && par.Taraf == 0 && par.NsSkor == 980, par.Metin);
                var ev = IhaleDegerlendirme.Degerlendir(t, 0, false, false, false, 4, 2, 2, KontrDurumu.Yok);
                K("DD10 4♦ Güney: batış, IMP ≤ -10", ev.Hata == IhaleHatasi.Batis && ev.ImpNs <= -10, ev.Hata + " " + ev.ImpNs);
                var ev2 = IhaleDegerlendirme.Degerlendir(t, 0, false, false, false, 6, 0, 2, KontrDurumu.Yok);
                K("DD11 6♠ = par, IMP 0", ev2.Hata == IhaleHatasi.Yok && ev2.ImpNs == 0, ev2.Hata + " " + ev2.ImpNs);
                var ev3 = IhaleDegerlendirme.Degerlendir(t, 0, false, false, false, 4, 0, 2, KontrDurumu.Yok);
                K("DD12 4♠ yapılır ama slam kaçırıldı", ev3.Hata == IhaleHatasi.SlamKacirildi, ev3.Hata.ToString());

                // yerleşik çözücü ≡ DDS (kolay bir el: kısa süre sınırı, hesaplanan hücreler eşleşmeli)
                var rnd = new Random(11); int fark = 0, karsi = 0;
                for (int i = 0; i < 3; i++)
                {
                    var h = DDMotor.RastgeleDagitim(rnd);
                    var t1 = DDMotor.Hesapla(h);
                    DDMotor.DdsKapali = true; int eski = DDMotor.YedekHucreSiniriMs; DDMotor.YedekHucreSiniriMs = 1500;
                    var t2 = DDMotor.Hesapla(h);
                    DDMotor.DdsKapali = false; DDMotor.YedekHucreSiniriMs = eski;
                    for (int k = 0; k < 20; k++) if (t2.Lov[k] >= 0) { karsi++; if (t2.Lov[k] != t1.Lov[k]) fark++; }
                }
                K("DD13 yerleşik çözücü DDS ile aynı (" + karsi + " hücre)", fark == 0, fark + " fark");
            }

            // par: tablo uydurma (N/S her koz 13 löve)
            var tt = new DDTablo();
            for (int k = 0; k < 20; k++) tt.Lov[k] = (k & 1) == 0 ? 13 : 0;   // N,S 13; E,W 0
            var p7 = ParHesabi.Hesapla(tt, 0, false, false);
            K("DD14 N/S her yerde 13: par 7NT", !p7.Pas && p7.Seviye == 7 && p7.Koz == 4 && p7.NsSkor == 1520, p7.Metin);
            var tp = new DDTablo();
            for (int k = 0; k < 20; k++) tp.Lov[k] = 6;
            K("DD15 herkes 6: par dört pas", ParHesabi.Hesapla(tp, 0, false, false).Pas);

            sonucEkle("  Çift dummy: " + _gecen + " geçti, " + _kalan + " kaldı");
            return _kalan;
        }
    }
}
