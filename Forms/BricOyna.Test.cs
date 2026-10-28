using BricKartOyunu.Class;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;


namespace BricKartOyunu
{
    // Konvansiyon/ihale motoru deneme ve yardımcı kodları
    public partial class BricOyna
    {
        private void TestIhaleMotoru()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("═══════════════════════");
                System.Diagnostics.Debug.WriteLine("[TEST] İhale Motoru Testi");
                System.Diagnostics.Debug.WriteLine("═══════════════════════");

                var anlasma = OrtaklikAnlasmasi.Varsayilan();
                var motor = new IhaleMotoru(anlasma);

                System.Diagnostics.Debug.WriteLine($"[TEST] {motor.Ozet()}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 1: Gerçek eller
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- GERÇEK ELLER ---");

                Player[] oyuncular = { Player.Guney, Player.Bati, Player.Kuzey, Player.Dogu };
                foreach (var oyuncu in oyuncular)
                {
                    var el = OyuncuEli(oyuncu);
                    if (el == null || el.Count != 13) continue;

                    var durum = new IhaleDurumu
                    {
                        AktifOyuncu = oyuncu,
                        AktifOyuncuEli = el,
                        Anlasma = anlasma
                    };

                    string teklif = motor.TeklifVer(durum);
                    string ozet = ElDegerlendirici.Ozet(el);

                    System.Diagnostics.Debug.WriteLine(
                        $"[TEST] {oyuncu} ({ozet}) → {teklif}");
                }

                // ═══════════════════════════════════════════════════════════════
                // TEST 2: Yapay eller
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- YAPAY ELLER ---");

                // El 1: 15 HP + 5'li Maça → 1♠ beklenir
                var el1 = YapayElOlustur(
                    "♠AKQ54 ♥KJ3 ♦Q2 ♣T87");
                TestTekEl(motor, el1, "Yapay-1: 15 HP + 5'li Maça");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♠, Gelen: {motor.TeklifVer(DurumOlustur(el1, anlasma))}");

                // El 2: 15 HP + 5'li Kupa → 1♥ beklenir
                var el2 = YapayElOlustur(
                    "♠KJ3 ♥AKQ54 ♦Q2 ♣T87");
                TestTekEl(motor, el2, "Yapay-2: 15 HP + 5'li Kupa");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♥, Gelen: {motor.TeklifVer(DurumOlustur(el2, anlasma))}");

                // El 3: 17 HP dengeli → 1NT beklenir
                var el3 = YapayElOlustur(
                    "♠KJ3 ♥QJ3 ♦AKQ5 ♣J87");
                TestTekEl(motor, el3, "Yapay-3: 17 HP dengeli (StrongNT aralığı)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1NT, Gelen: {motor.TeklifVer(DurumOlustur(el3, anlasma))}");

                // El 4: 8 HP → Pas beklenir
                var el4 = YapayElOlustur(
                    "♠KJ3 ♥Q432 ♦J52 ♣J87");
                TestTekEl(motor, el4, "Yapay-4: 8 HP");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: Pas, Gelen: {motor.TeklifVer(DurumOlustur(el4, anlasma))}");

                // El 5: 16 HP dengeli, 5'li majör yok → 1NT beklenir
                var el5 = YapayElOlustur(
                    "♠KJ3 ♥QJ3 ♦AKQ5 ♣J87");
                TestTekEl(motor, el5, "Yapay-5: 16 HP dengeli, 5'li majör yok");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1NT, Gelen: {motor.TeklifVer(DurumOlustur(el5, anlasma))}");

                // El 6: 15 HP + 5'li Maça → 1♠ beklenir
                var el6 = YapayElOlustur(
                    "♠AKQ54 ♥QJ3 ♦Q2 ♣J87");
                TestTekEl(motor, el6, "Yapay-6: 15 HP + 5'li Maça (BesliMajor önce)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♠, Gelen: {motor.TeklifVer(DurumOlustur(el6, anlasma))}");

                // El 7: 22+ HP → 2♣ beklenir
                var el7 = YapayElOlustur(
                    "♠AKQ54 ♥AKQ3 ♦AK ♣T8");
                TestTekEl(motor, el7, "Yapay-7: 22 HP → 2♣ (yapay güçlü)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 2♣, Gelen: {motor.TeklifVer(DurumOlustur(el7, anlasma))}");

                // El 8: 24 HP + 5-5 → 2♣ beklenir
                var el8 = YapayElOlustur(
                    "♠AKQ54 ♥AKQJ3 ♦KQ ♣T");
                TestTekEl(motor, el8, "Yapay-8: 24 HP + 5-5 → 2♣ (9 tricks)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 2♣, Gelen: {motor.TeklifVer(DurumOlustur(el8, anlasma))}");

                // El 9: 20 HP + 5'li Maça → 1♠ beklenir
                var el9 = YapayElOlustur(
                    "♠AKQ54 ♥KQ3 ♦KQ2 ♣T8");
                TestTekEl(motor, el9, "Yapay-9: 20 HP + 5'li Maça → 1♠ (2♣ değil)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♠, Gelen: {motor.TeklifVer(DurumOlustur(el9, anlasma))}");

                // El 10: 12 HP dengeli, 4'lü Karo → 1♦ beklenir
                var el10 = YapayElOlustur(
                    "♠KJ3 ♥Q43 ♦KQ52 ♣J87");
                TestTekEl(motor, el10, "Yapay-10: 12 HP dengeli, 4'lü Karo");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♦, Gelen: {motor.TeklifVer(DurumOlustur(el10, anlasma))}");

                // El 11: 14 HP + 4'lü Karo → 1♦ beklenir
                var el11 = YapayElOlustur(
                    "♠KJ3 ♥Q43 ♦QJ52 ♣AJ8");
                TestTekEl(motor, el11, "Yapay-11: 14 HP, 4'lü Karo");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♦, Gelen: {motor.TeklifVer(DurumOlustur(el11, anlasma))}");

                // El 12: 13 HP + 4'lü Karo, 4'lü Sinek → 1♦ beklenir
                var el12 = YapayElOlustur(
                    "♠KJ3 ♥QJ4 ♦KQ52 ♣J87");
                TestTekEl(motor, el12, "Yapay-12: 13 HP, 4'lü Karo, 4'lü Sinek");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Beklenen: 1♦, Gelen: {motor.TeklifVer(DurumOlustur(el12, anlasma))}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 3: BasitCevap Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- BASİT CEVAP TESTLERİ ---");

                // Yapay-13: Partner 1♠ açtı, ben 8 HP + 3 Maça → 2♠ beklenir
                var el13 = YapayElOlustur("♠J43 ♥Q43 ♦J52 ♣QJ87");
                var durum13 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el13,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-13: Partner 1♠, ben 8 HP + 3 Maça → 2♠ beklenir");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el13)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum13)}");

                // Yapay-14: Partner 1♠, ben 14 HP + 5 Kupa dengeli → 2NT beklenir
                var el14 = YapayElOlustur("♠J43 ♥AKQ54 ♦QJ2 ♣87");
                var durum14 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el14,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-14: Partner 1♠, ben 14 HP + 5 Kupa dengeli → 2NT beklenir");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el14)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum14)}");

                // Yapay-15: Partner 1♠, ben 12 HP dengeli → 2NT beklenir
                var el15 = YapayElOlustur("♠J43 ♥QJ4 ♦KQ52 ♣AJ8");
                var durum15 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el15,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-15: Partner 1♠, ben 12 HP dengeli → 2NT beklenir");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el15)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum15)}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 4: Stayman Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- STAYMAN TESTLERİ ---");

                // Yapay-16: Partner 1NT, ben 4'lü Maça + 10 HP → 2♣ (Stayman sorusu)
                var el16 = YapayElOlustur("♠KJ43 ♥Q43 ♦J52 ♣QJ8");
                var durum16 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el16,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-16: Partner 1NT, ben 4'lü Maça + 10 HP → 2♣ (Stayman)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el16)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum16)}");

                // Yapay-17: Ben 1NT açtım, partner 2♣ dedi, 4'lü Maça → 2♠
                var el17 = YapayElOlustur("♠KJ43 ♥Q43 ♦J52 ♣QJ8");
                var durum17 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Kuzey,
                    AktifOyuncuEli = el17,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-17: Ben 1NT, partner 2♣ (Stayman), 4'lü Maça → 2♠");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el17)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum17)}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 5: JacobyTransfer Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- JACOBY TRANSFER TESTLERİ ---");

                // Yapay-18: Partner 1NT, ben 5+ Kupa → 2♦ (Kupa transferi)
                var el18 = YapayElOlustur("♠Q43 ♥KJ543 ♦J52 ♣QJ");
                var durum18 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el18,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-18: Partner 1NT, ben 5+ Kupa → 2♦ (Kupa transferi)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el18)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum18)}");

                // Yapay-19: Partner 1NT, ben 5+ Maça → 2♥ (Maça transferi)
                var el19 = YapayElOlustur("♠KJ543 ♥Q43 ♦J52 ♣QJ");
                var durum19 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el19,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-19: Partner 1NT, ben 5+ Maça → 2♥ (Maça transferi)");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el19)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum19)}");

                // Yapay-20: Ben 1NT, partner 2♦ (Kupa transferi) → 2♥ kabul
                var el20 = YapayElOlustur("♠KJ3 ♥Q43 ♦KQ52 ♣AJ8");
                var durum20 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Kuzey,
                    AktifOyuncuEli = el20,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♦", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-20: Ben 1NT, partner 2♦ (Kupa transferi) → 2♥ kabul");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el20)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum20)}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 6: NegativeDouble Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- NEGATİF KONTR TESTLERİ ---");

                // Yapay-21: Rakip 1♠ açtı, ben 4'lü Kupa + 10 HP → Dbl
                var el21 = YapayElOlustur("♠Q43 ♥KJ43 ♦QJ2 ♣Q87");
                var durum21 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el21,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "1♠", Sira = 1 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-21: Rakip 1♠ açtı, ben 4'lü Kupa + 10 HP → Dbl");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el21)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum21)}");

                // Yapay-22: Rakip 1♥ açtı, ben 4'lü Maça + 10 HP → Dbl
                var el22 = YapayElOlustur("♠KJ43 ♥Q43 ♦QJ2 ♣Q87");
                var durum22 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el22,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "1♥", Sira = 1 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-22: Rakip 1♥ açtı, ben 4'lü Maça + 10 HP → Dbl");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el22)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum22)}");

                // Yapay-23: Rakip 1♦ açtı, ben 4'lü Maça + 4'lü Kupa → Dbl
                var el23 = YapayElOlustur("♠KJ43 ♥QJ43 ♦Q2 ♣Q87");
                var durum23 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el23,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "1♦", Sira = 1 }
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-23: Rakip 1♦ açtı, ben 4'lü Maça + 4'lü Kupa → Dbl");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el23)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum23)}");

                // ═══════════════════════════════════════════════════════════════
                // TEST 7: Blackwood / RKCB Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- BLACKWOOD / RKCB TESTLERİ ---");

                // Yapay-31: Partner 1♠ açtı, ben 3♠ dedim, partner Pas → ben 4NT soracağım
                var el31 = YapayElOlustur("♠KQ54 ♥AJ3 ♦KQ2 ♣K32");  // ♣A K 2 = 3 kart
                var durum31 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el31,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 5 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 6 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-31: Partner 1♠, ben 3♠, partner Pas → ben Blackwood sorusu → 4NT");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el31)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el31)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum31)}");

                // Yapay-32: Partner 4NT dedi, ben 0 As → 5♣
                var el32 = YapayElOlustur("♠Q43 ♥KJ43 ♦QJ2 ♣Q87");
                var durum32 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el32,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4NT", Sira = 5 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 6 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-32: Partner 4NT dedi, ben 0 As → 5♣");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el32)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el32)}, " +
                    $"Kilit kart (♠): {ElDegerlendirici.KilitKartSayisi(el32, "Maça")}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum32)}");

                // Yapay-33: Partner 4NT dedi, ben 1 As → 5♦
                var el33 = YapayElOlustur("♠A43 ♥KJ43 ♦QJ2 ♣Q87");
                var durum33 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el33,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4NT", Sira = 5 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 6 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-33: Partner 4NT dedi, ben 1 As → 5♦");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el33)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el33)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum33)}");

                // Yapay-34: Partner 4NT dedi, ben 2 As → 5♥
                var el34 = YapayElOlustur("♠A43 ♥AJ43 ♦QJ2 ♣Q87");
                var durum34 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el34,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4NT", Sira = 5 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 6 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-34: Partner 4NT dedi, ben 2 As → 5♥");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el34)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el34)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum34)}");

                // Yapay-35: Partner 4NT dedi, ben 2 kilit kart (1 As + koz K) → 5♠
                var el35 = YapayElOlustur("♠KJ43 ♥AJ43 ♦QJ2 ♣Q8");
                var durum35 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el35,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4NT", Sira = 5 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 6 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-35: Partner 4NT dedi, ben 2 kilit kart (1 As + ♠K) → 5♠");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el35)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el35)}, " +
                    $"Papaz: {ElDegerlendirici.PapazSayisi(el35)}, " +
                    $"Kilit kart (♠): {ElDegerlendirici.KilitKartSayisi(el35, "Maça")}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum35)}");
                System.Diagnostics.Debug.WriteLine("");

                // ═══════════════════════════════════════════════════════════════
                // TEST 8: Cue Bid Testleri
                // ═══════════════════════════════════════════════════════════════
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("[TEST] --- CUE BID TESTLERİ ---");

                // Yapay-41: 1♠-3♠ sonrası, elimde ♣A var → 4♣ cue bid
                var el41 = YapayElOlustur("♠KQ54 ♥AJ3 ♦KQ2 ♣A32");
                var durum41 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el41,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-41: Partner 1♠, ben 3♠, elimde ♣A → 4♣ cue bid");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el41)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   As: {ElDegerlendirici.AsSayisi(el41)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum41)}");

                // Yapay-42: 1♥-3♥ sonrası, elimde ♣K var → 4♣ cue bid
                var el42 = YapayElOlustur("♠AJ3 ♥KQ54 ♦KQ2 ♣K32");
                var durum42 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el42,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♥", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-42: Partner 1♥, ben 3♥, elimde ♣K → 4♣ cue bid");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el42)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum42)}");

                // Yapay-43: 1♠-3♠ sonrası, ♣'te kontrol yok ama ♦'da A var → 4♦ cue bid
                var el43 = YapayElOlustur("♠KQ54 ♥AJ3 ♦A32 ♣Q87");
                var durum43 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el43,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-43: Partner 1♠, ben 3♠, ♣'te kontrol yok ama ♦'da A → 4♦ cue bid");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el43)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum43)}");

                // Yapay-44: 1♠-3♠ sonrası, sadece 12 HP → cue bid değil, Blackwood da değil
                var el44 = YapayElOlustur("♠KQ54 ♥QJ3 ♦QJ2 ♣Q87");
                var durum44 = new IhaleDurumu
                {
                    AktifOyuncu = Player.Guney,
                    AktifOyuncuEli = el44,
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>
                    {
                        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1 },
                        new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 2 },
                        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3 },
                        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 4 },
                    }
                };
                System.Diagnostics.Debug.WriteLine(
                    "[TEST] Yapay-44: Partner 1♠, ben 3♠, sadece 12 HP → cue bid yapılmaz");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   {ElDegerlendirici.Ozet(el44)}");
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST]   → Gelen: {motor.TeklifVer(durum44)}");
                System.Diagnostics.Debug.WriteLine("");

                System.Diagnostics.Debug.WriteLine("═══════════════════════");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[TEST] HATA: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Test için tek el çalıştırır.
        /// </summary>
        private void TestTekEl(IhaleMotoru motor, List<Card> el, string aciklama)
        {
            System.Diagnostics.Debug.WriteLine($"[TEST] {aciklama}");
            System.Diagnostics.Debug.WriteLine($"       {ElDegerlendirici.Ozet(el)}");
        }

        /// <summary>
        /// Test için el oluşturur.
        /// Format: "♠AKQ54 ♥KJ3 ♦Q2 ♣T87"
        /// </summary>
        private List<Card> YapayElOlustur(string metin)
        {
            var el = new List<Card>();
            string[] renkGruplari = metin.Split(' ');

            foreach (var grup in renkGruplari)
            {
                if (string.IsNullOrEmpty(grup) || grup.Length < 2) continue;

                char renkSembolu = grup[0];
                string renk = "";
                switch (renkSembolu)
                {
                    case '♠': renk = "Maça"; break;
                    case '♥': renk = "Kupa"; break;
                    case '♦': renk = "Karo"; break;
                    case '♣': renk = "Sinek"; break;
                    default: continue;
                }

                string kartlar = grup.Substring(1);
                foreach (char k in kartlar)
                {
                    int deger;
                    switch (k)
                    {
                        case 'A': deger = 14; break;
                        case 'K': deger = 13; break;
                        case 'Q': deger = 12; break;
                        case 'J': deger = 11; break;
                        case 'T': deger = 10; break;
                        default:
                            if (!int.TryParse(k.ToString(), out deger)) continue;
                            break;
                    }
                    el.Add(new Card { Suit = renk, Value = deger });
                }
            }

            // 🔹 Kontrol: 13 kart olmalı
            if (el.Count != 13)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[YapayElOlustur] UYARI: El {el.Count} kart! " +
                    $"Beklenen: 13. Girdi: '{metin}'");
            }

            return el;
        }

        /// <summary>
        /// Test için ihale durumu oluşturur.
        /// </summary>
        private IhaleDurumu DurumOlustur(List<Card> el, OrtaklikAnlasmasi anlasma)
        {
            return new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = anlasma
            };
        }

    }
}
