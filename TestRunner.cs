using System.Collections.Generic;
using System.Diagnostics;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;

namespace BricKartOyunu
{
    /// <summary>
    /// Geçici test sınıfı — konvansiyonları doğrulamak için.
    /// Testler tamamlandıktan sonra silinebilir.
    /// </summary>
    public static class TestRunner
    {
        public static void Test_Gerber_SoruSorma()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 1.1 — Gerber Soru Sorma");
            Debug.WriteLine("════════════════════════════════════════");

            // ── Güney'in eli: 16 HP, dengeli ──────────────────────────────
            var guneyEli = new List<Card>
            {
                // ♠ AKQ = 4+3+2 = 9 HP
                new Card { Suit = "Maça", Value = 14 },
                new Card { Suit = "Maça", Value = 13 },
                new Card { Suit = "Maça", Value = 12 },
                // ♥ KJx = 3+1 = 4 HP
                new Card { Suit = "Kupa", Value = 13 },
                new Card { Suit = "Kupa", Value = 11 },
                new Card { Suit = "Kupa", Value = 5  },
                // ♦ QJx = 2+1 = 3 HP
                new Card { Suit = "Karo", Value = 12 },
                new Card { Suit = "Karo", Value = 11 },
                new Card { Suit = "Karo", Value = 4  },
                // ♣ xxxx = 0 HP
                new Card { Suit = "Sinek", Value = 9 },
                new Card { Suit = "Sinek", Value = 7 },
                new Card { Suit = "Sinek", Value = 5 },
                new Card { Suit = "Sinek", Value = 2 },
            };

            // ── Geçmiş: Kuzey 1NT, Doğu Pas ───────────────────────────────
            var gecmis = new List<IhaleHamlesi>
            {
                new IhaleHamlesi
                {
                    Oyuncu = Player.Kuzey,
                    Teklif = "1NT",
                    Sira = 1,
                    GecerliMi = true
                },
                new IhaleHamlesi
                {
                    Oyuncu = Player.Dogu,
                    Teklif = "Pas",
                    Sira = 2,
                    GecerliMi = true
                },
            };

            // ── Durumu oluştur ────────────────────────────────────────────
            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // ── Ön kontroller (debug) ─────────────────────────────────────
            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Dengeli mi?: {ElDegerlendirici.DengeliEl(guneyEli)}");
            Debug.WriteLine($"AnlasilanKoz: {durum.AnlasilanKoz() ?? "null"}");
            Debug.WriteLine($"KacinciSeviye: {durum.KacinciSeviye()}");
            Debug.WriteLine($"SonGercekTeklif: {durum.SonGercekTeklif ?? "null"}");
            Debug.WriteLine($"SonGercekTeklifSahibi: {durum.SonGercekTeklifSahibi}");
            Debug.WriteLine($"Partner: {durum.Partner}");
            Debug.WriteLine($"KendiTeklifleri: [{string.Join(", ", durum.KendiTeklifleri)}]");
            Debug.WriteLine("────────────────────────────────────────");

            // ── Gerber'i doğrudan test et ─────────────────────────────────
            var gerber = new Gerber();
            gerber.AktifMi = true;

            bool uygun = gerber.UygunMu(durum);
            Debug.WriteLine($"Gerber.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = gerber.TeklifVer(durum);
                Debug.WriteLine($"Gerber.TeklifVer → {teklif}");
                Debug.WriteLine(teklif == "4♣" ? "✅ BEKLENEN: 4♣" : $"❌ BEKLENEN: 4♣, GELEN: {teklif}");
            }
            else
            {
                Debug.WriteLine("❌ Gerber uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Debug.WriteLine("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Debug.WriteLine($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Debug.WriteLine(motorTeklif == "4♣" ? "✅ MOTOR: 4♣" : $"❌ MOTOR: 4♣ beklendi, gelen {motorTeklif}");

            Debug.WriteLine("════════════════════════════════════════");
        }

        public static void Test_Gerber_Cevap()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 1.2 — Gerber Cevap Verme");
            Debug.WriteLine("════════════════════════════════════════");

            // ── Kuzey'in eli: 2 As ────────────────────────────────────────
            var kuzeyEli = new List<Card>
    {
        // ♠ Axxx = 4 HP
        new Card { Suit = "Maça", Value = 14 },  // A
        new Card { Suit = "Maça", Value = 7  },
        new Card { Suit = "Maça", Value = 5  },
        new Card { Suit = "Maça", Value = 2  },
        // ♥ Axx = 4 HP
        new Card { Suit = "Kupa", Value = 14 },  // A
        new Card { Suit = "Kupa", Value = 6  },
        new Card { Suit = "Kupa", Value = 3  },
        // ♦ Kxx = 3 HP
        new Card { Suit = "Karo", Value = 13 },  // K
        new Card { Suit = "Karo", Value = 8  },
        new Card { Suit = "Karo", Value = 4  },
        // ♣ Qxx = 2 HP
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 9  },
        new Card { Suit = "Sinek", Value = 6  },
    };
            // Toplam: 4+4+3+2 = 13 HP, 2 As ✓

            // ── Geçmiş: 1NT - Pas - 4♣ - Pas ─────────────────────────────
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // ── Ön kontroller ─────────────────────────────────────────────
            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Debug.WriteLine($"AsSayisi: {ElDegerlendirici.AsSayisi(kuzeyEli)}");
            Debug.WriteLine($"SonGercekTeklif: {durum.SonGercekTeklif}");
            Debug.WriteLine($"SonGercekTeklifSahibi: {durum.SonGercekTeklifSahibi}");
            Debug.WriteLine($"Partner (Guney): {durum.Partner}");
            Debug.WriteLine($"AnlasilanKoz: {durum.AnlasilanKoz() ?? "null"}");
            Debug.WriteLine("────────────────────────────────────────");

            // ── Gerber'i doğrudan test et ─────────────────────────────────
            var gerber = new Gerber();
            gerber.AktifMi = true;

            bool uygun = gerber.UygunMu(durum);
            Debug.WriteLine($"Gerber.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = gerber.TeklifVer(durum);
                Debug.WriteLine($"Gerber.TeklifVer → {teklif}");
                Debug.WriteLine(teklif == "4♠" ? "✅ BEKLENEN: 4♠" : $"❌ BEKLENEN: 4♠, GELEN: {teklif}");
            }
            else
            {
                Debug.WriteLine("❌ Gerber uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Debug.WriteLine("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Debug.WriteLine($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Debug.WriteLine(motorTeklif == "4♠" ? "✅ MOTOR: 4♠" : $"❌ MOTOR: 4♠ beklendi, gelen {motorTeklif}");

            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Gerber_Negatif()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 1.3 — Gerber Negatif (Koz Maça)");
            Debug.WriteLine("════════════════════════════════════════");

            // ── Kuzey'in eli: 16 HP ───────────────────────────────────────
            var kuzeyEli = new List<Card>
    {
        // ♠ KQxxx = 3+2 = 5 HP
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 12 },
        new Card { Suit = "Maça", Value = 7  },
        new Card { Suit = "Maça", Value = 5  },
        new Card { Suit = "Maça", Value = 2  },
        // ♥ Axx = 4 HP
        new Card { Suit = "Kupa", Value = 14 },
        new Card { Suit = "Kupa", Value = 6  },
        new Card { Suit = "Kupa", Value = 3  },
        // ♦ Axx = 4 HP
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 8  },
        new Card { Suit = "Karo", Value = 4  },
        // ♣ Kxx = 3 HP
        new Card { Suit = "Sinek", Value = 13 },
        new Card { Suit = "Sinek", Value = 9  },
        new Card { Suit = "Sinek", Value = 6  },
    };
            // Toplam: 5+4+4+3 = 16 HP

            // ── Geçmiş: 1♠ - Pas - 3♠ - Pas ──────────────────────────────
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // ── Ön kontroller ─────────────────────────────────────────────
            Debug.WriteLine($"AnlasilanKoz: {durum.AnlasilanKoz() ?? "null"}");
            Debug.WriteLine($"KacinciSeviye: {durum.KacinciSeviye()}");
            Debug.WriteLine("────────────────────────────────────────");

            // ── Gerber uygun OLMAMALI ─────────────────────────────────────
            var gerber = new Gerber();
            gerber.AktifMi = true;

            bool uygun = gerber.UygunMu(durum);
            Debug.WriteLine($"Gerber.UygunMu → {uygun}");
            Debug.WriteLine(!uygun ? "✅ BEKLENEN: False (koz Maça, Gerber devre dışı)"
                                    : "❌ HATA: Gerber uygun olmamalıydı!");

            // ── Motor Gerber kullanmamalı ─────────────────────────────────
            Debug.WriteLine("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Debug.WriteLine($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Debug.WriteLine(motorTeklif != "4♣"
                ? "✅ MOTOR: 4♣ DEĞİL (doğru)"
                : "❌ MOTOR: 4♣ döndü — Gerber yanlış tetiklendi!");

            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Jacoby2NT_Pozitif()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 2.1 — Jacoby 2NT Pozitif");
            Debug.WriteLine("════════════════════════════════════════");

            // ── Güney'in eli: 4♠ + 14 HP ──────────────────────────────────
            var guneyEli = new List<Card>
    {
        // ♠ KQxx = 3+2 = 5 HP
        new Card { Suit = "Maça", Value = 13 }, // K
        new Card { Suit = "Maça", Value = 12 }, // Q
        new Card { Suit = "Maça", Value = 7  },
        new Card { Suit = "Maça", Value = 5  },
        // ♥ Axx = 4 HP
        new Card { Suit = "Kupa", Value = 14 }, // A
        new Card { Suit = "Kupa", Value = 6  },
        new Card { Suit = "Kupa", Value = 3  },
        // ♦ KJx = 3+1 = 4 HP
        new Card { Suit = "Karo", Value = 13 }, // K
        new Card { Suit = "Karo", Value = 11 }, // J
        new Card { Suit = "Karo", Value = 4  },
        // ♣ Qxx = 2 HP
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 8  },
        new Card { Suit = "Sinek", Value = 2  },
    };
            // Toplam: 5+4+4+2 = 15 HP, 4♠ destek ✓

            // ── Geçmiş: Kuzey 1♠, Doğu Pas ────────────────────────────────
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // ── Ön kontroller ─────────────────────────────────────────────
            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Debug.WriteLine($"PartnerTeklifleri: [{string.Join(", ", durum.PartnerTeklifleri)}]");
            Debug.WriteLine($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Debug.WriteLine($"KendiTeklifleri: [{string.Join(", ", durum.KendiTeklifleri)}]");
            Debug.WriteLine("────────────────────────────────────────");

            // ── Jacoby2NT'yi doğrudan test et ─────────────────────────────
            var jacoby = new Jacoby2NT();
            jacoby.AktifMi = true;

            bool uygun = jacoby.UygunMu(durum);
            Debug.WriteLine($"Jacoby2NT.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = jacoby.TeklifVer(durum);
                Debug.WriteLine($"Jacoby2NT.TeklifVer → {teklif}");
                Debug.WriteLine(teklif == "2NT" ? "✅ BEKLENEN: 2NT" : $"❌ BEKLENEN: 2NT, GELEN: {teklif}");
            }
            else
            {
                Debug.WriteLine("❌ Jacoby2NT uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Debug.WriteLine("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Debug.WriteLine($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Debug.WriteLine(motorTeklif == "2NT" ? "✅ MOTOR: 2NT" : $"❌ MOTOR: 2NT beklendi, gelen {motorTeklif}");

            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Jacoby2NT_Negatif_3luDestek()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 2.2 — Jacoby 2NT Negatif (3'lü destek)");
            Debug.WriteLine("════════════════════════════════════════");

            // ── Güney'in eli: sadece 3♠ ───────────────────────────────────
            var guneyEli = new List<Card>
    {
        // ♠ KQx = 3+2 = 5 HP (3'lü!)
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 12 },
        new Card { Suit = "Maça", Value = 7  },
        // ♥ Axx = 4 HP
        new Card { Suit = "Kupa", Value = 14 },
        new Card { Suit = "Kupa", Value = 6  },
        new Card { Suit = "Kupa", Value = 3  },
        // ♦ KJx = 4 HP
        new Card { Suit = "Karo", Value = 13 },
        new Card { Suit = "Karo", Value = 11 },
        new Card { Suit = "Karo", Value = 4  },
        // ♣ Qxx = 2 HP
        new Card { Suit = "Sinek", Value = 12 },
        new Card { Suit = "Sinek", Value = 8  },
        new Card { Suit = "Sinek", Value = 2  },
        // Ekstra bir kart (13 kart tamamlamak için)
        new Card { Suit = "Kupa", Value = 9  },
    };
            // HCP: 5+4+4+2 = 15 HP, Maça 3'lü

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Debug.WriteLine("────────────────────────────────────────");

            var jacoby = new Jacoby2NT();
            jacoby.AktifMi = true;

            bool uygun = jacoby.UygunMu(durum);
            Debug.WriteLine($"Jacoby2NT.UygunMu → {uygun}");
            Debug.WriteLine(!uygun ? "✅ BEKLENEN: False (3'lü destek)"
                                    : "❌ HATA: Jacoby2NT uygun olmamalıydı!");

            // ── Motor testi ───────────────────────────────────────────────
            Debug.WriteLine("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Debug.WriteLine($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Debug.WriteLine(motorTeklif != "2NT"
                ? "✅ MOTOR: 2NT DEĞİL (doğru)"
                : "❌ MOTOR: 2NT döndü — Jacoby yanlış tetiklendi!");

            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Jacoby2NT_Negatif_RakipMudahalesi()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 2.3 — Jacoby 2NT Negatif (Rakip müdahalesi)");
            Debug.WriteLine("════════════════════════════════════════");

            var guneyEli = new List<Card>
    {
        // ♠ KQxx = 5 HP
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 12 },
        new Card { Suit = "Maça", Value = 7  },
        new Card { Suit = "Maça", Value = 5  },
        // ♥ Axx = 4 HP
        new Card { Suit = "Kupa", Value = 14 },
        new Card { Suit = "Kupa", Value = 6  },
        new Card { Suit = "Kupa", Value = 3  },
        // ♦ KJx = 4 HP
        new Card { Suit = "Karo", Value = 13 },
        new Card { Suit = "Karo", Value = 11 },
        new Card { Suit = "Karo", Value = 4  },
        // ♣ Qxx = 2 HP
        new Card { Suit = "Sinek", Value = 12 },
        new Card { Suit = "Sinek", Value = 8  },
        new Card { Suit = "Sinek", Value = 2  },
    };

            // ── Geçmiş: 1♠ - 2♥ (rakip gerçek teklif) ─────────────────────
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "2♥", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Debug.WriteLine($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Debug.WriteLine("────────────────────────────────────────");

            var jacoby = new Jacoby2NT();
            jacoby.AktifMi = true;

            bool uygun = jacoby.UygunMu(durum);
            Debug.WriteLine($"Jacoby2NT.UygunMu → {uygun}");
            Debug.WriteLine(!uygun ? "✅ BEKLENEN: False (rakip müdahale etti)"
                                    : "❌ HATA: Jacoby2NT uygun olmamalıydı!");

            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Splinter_Maca_Sinek()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 3.1 — Splinter (Koz Maça, Kısa Sinek)");
            Debug.WriteLine("════════════════════════════════════════");

            // ── Güney'in eli: 4♠ + 14 HP + singleton ♣ ────────────────────
            var guneyEli = new List<Card>
    {
        // ♠ KQxx = 5 HP
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 12 },
        new Card { Suit = "Maça", Value = 7  },
        new Card { Suit = "Maça", Value = 5  },
        // ♥ Axxx = 4 HP
        new Card { Suit = "Kupa", Value = 14 },
        new Card { Suit = "Kupa", Value = 7  },
        new Card { Suit = "Kupa", Value = 4  },
        new Card { Suit = "Kupa", Value = 2  },
        // ♦ KJxx = 4 HP
        new Card { Suit = "Karo", Value = 13 },
        new Card { Suit = "Karo", Value = 11 },
        new Card { Suit = "Karo", Value = 8  },
        new Card { Suit = "Karo", Value = 3  },
        // ♣ x = singleton (0 HP)
        new Card { Suit = "Sinek", Value = 6 },
    };
            // HCP: 5+4+4 = 13 HP, Dağılım: 4-4-4-1 ✓ singleton Sinek

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // ── Ön kontroller ─────────────────────────────────────────────
            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            var sayilar = ElDegerlendirici.RenkSayilari(guneyEli);
            Debug.WriteLine($"Dağılım: ♠{sayilar["Maça"]} ♥{sayilar["Kupa"]} ♦{sayilar["Karo"]} ♣{sayilar["Sinek"]}");
            Debug.WriteLine($"PartnerTeklifleri: [{string.Join(", ", durum.PartnerTeklifleri)}]");
            Debug.WriteLine($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Debug.WriteLine("────────────────────────────────────────");

            var splinter = new Splinter();
            splinter.AktifMi = true;

            bool uygun = splinter.UygunMu(durum);
            Debug.WriteLine($"Splinter.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = splinter.TeklifVer(durum);
                Debug.WriteLine($"Splinter.TeklifVer → {teklif}");
                Debug.WriteLine(teklif == "4♣" ? "✅ BEKLENEN: 4♣" : $"❌ BEKLENEN: 4♣, GELEN: {teklif}");
            }
            else
            {
                Debug.WriteLine("❌ Splinter uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Debug.WriteLine("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Debug.WriteLine($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Debug.WriteLine(motorTeklif == "4♣" ? "✅ MOTOR: 4♣" : $"❌ MOTOR: 4♣ beklendi, gelen {motorTeklif}");

            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Splinter_Kupa_Maca()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 3.2 — Splinter (Koz Kupa, Kısa Maça)");
            Debug.WriteLine("════════════════════════════════════════");

            // ── Güney'in eli: 4♥ + 14 HP + singleton ♠ ────────────────────
            var guneyEli = new List<Card>
    {
        // ♠ x = singleton
        new Card { Suit = "Maça", Value = 6 },
        // ♥ KQxx = 5 HP
        new Card { Suit = "Kupa", Value = 13 },
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 7  },
        new Card { Suit = "Kupa", Value = 5  },
        // ♦ Axxx = 4 HP
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 7  },
        new Card { Suit = "Karo", Value = 4  },
        new Card { Suit = "Karo", Value = 2  },
        // ♣ KJxx = 4 HP
        new Card { Suit = "Sinek", Value = 13 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8  },
        new Card { Suit = "Sinek", Value = 3  },
    };
            // HCP: 5+4+4 = 13 HP, Dağılım: 1-4-4-4 ✓ singleton Maça

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Kupa uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            var sayilar = ElDegerlendirici.RenkSayilari(guneyEli);
            Debug.WriteLine($"Dağılım: ♠{sayilar["Maça"]} ♥{sayilar["Kupa"]} ♦{sayilar["Karo"]} ♣{sayilar["Sinek"]}");
            Debug.WriteLine("────────────────────────────────────────");

            var splinter = new Splinter();
            splinter.AktifMi = true;

            bool uygun = splinter.UygunMu(durum);
            Debug.WriteLine($"Splinter.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = splinter.TeklifVer(durum);
                Debug.WriteLine($"Splinter.TeklifVer → {teklif}");
                Debug.WriteLine(teklif == "3♠" ? "✅ BEKLENEN: 3♠" : $"❌ BEKLENEN: 3♠, GELEN: {teklif}");
            }
            else
            {
                Debug.WriteLine("❌ Splinter uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Debug.WriteLine("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Debug.WriteLine($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Debug.WriteLine(motorTeklif == "3♠" ? "✅ MOTOR: 3♠" : $"❌ MOTOR: 3♠ beklendi, gelen {motorTeklif}");

            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Michaels_RakipMaca()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 4.1 — Michaels (Rakip Maça açtı)");
            Debug.WriteLine("════════════════════════════════════════");

            // ── Güney'in eli: 5♥ + 5♦ ────────────────────────────────────
            var guneyEli = new List<Card>
    {
        // ♠ x
        new Card { Suit = "Maça", Value = 6 },
        // ♥ KQxxx = 5 HP
        new Card { Suit = "Kupa", Value = 13 },
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 7  },
        new Card { Suit = "Kupa", Value = 5  },
        new Card { Suit = "Kupa", Value = 3  },
        // ♦ AQxxx = 4+2 = 6 HP
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 12 },
        new Card { Suit = "Karo", Value = 8  },
        new Card { Suit = "Karo", Value = 5  },
        new Card { Suit = "Karo", Value = 3  },
        // ♣ xx
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 2 },
    };
            // HCP: 5+6 = 11 HP, Dağılım: 1-5-5-2 ✓

            // ── Geçmiş: BATI 1♠ (rakip!), Kuzey Pas ───────────────────────
            // Sıra: Güney'de
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // ── Ön kontroller ─────────────────────────────────────────────
            Debug.WriteLine($"AktifOyuncu: {durum.AktifOyuncu}");
            Debug.WriteLine($"Rakipler: [{string.Join(", ", durum.Rakipler)}]");
            Debug.WriteLine($"Partner: {durum.Partner}");
            Debug.WriteLine($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Debug.WriteLine($"PartnerTeklifleri: [{string.Join(", ", durum.PartnerTeklifleri)}]");
            Debug.WriteLine($"KendiTeklifleri: [{string.Join(", ", durum.KendiTeklifleri)}]");
            Debug.WriteLine($"RakipActiMi: {durum.RakipActiMi()}");
            Debug.WriteLine("────────────────────────────────────────");

            var michaels = new Michaels();
            michaels.AktifMi = true;

            bool uygun = michaels.UygunMu(durum);
            Debug.WriteLine($"Michaels.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = michaels.TeklifVer(durum);
                Debug.WriteLine($"Michaels.TeklifVer → {teklif}");
                Debug.WriteLine(teklif == "2♠" ? "✅ BEKLENEN: 2♠" : $"❌ BEKLENEN: 2♠, GELEN: {teklif}");
            }
            else
            {
                Debug.WriteLine("❌ Michaels uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Debug.WriteLine("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Debug.WriteLine($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Debug.WriteLine(motorTeklif == "2♠" ? "✅ MOTOR: 2♠" : $"❌ MOTOR: 2♠ beklendi, gelen {motorTeklif}");

            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Michaels_RakipKaro()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 4.2 — Michaels (Rakip Karo açtı)");
            Debug.WriteLine("════════════════════════════════════════");

            // ── Güney'in eli: 5♠ + 5♥ ────────────────────────────────────
            var guneyEli = new List<Card>
    {
        // ♠ KQxxx = 5 HP
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 12 },
        new Card { Suit = "Maça", Value = 7  },
        new Card { Suit = "Maça", Value = 5  },
        new Card { Suit = "Maça", Value = 3  },
        // ♥ AQxxx = 6 HP
        new Card { Suit = "Kupa", Value = 14 },
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 8  },
        new Card { Suit = "Kupa", Value = 5  },
        new Card { Suit = "Kupa", Value = 2  },
        // ♦ xx
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // ♣ x
        new Card { Suit = "Sinek", Value = 6 },
    };
            // HCP: 5+6 = 11 HP, Dağılım: 5-5-2-1 ✓

            // ── Geçmiş: BATI 1♦ (rakip), Kuzey Pas ────────────────────────
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // ── Ön kontroller ─────────────────────────────────────────────
            Debug.WriteLine($"Rakipler: [{string.Join(", ", durum.Rakipler)}]");
            Debug.WriteLine($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Debug.WriteLine($"PartnerTeklifleri: [{string.Join(", ", durum.PartnerTeklifleri)}]");
            Debug.WriteLine($"RakipActiMi: {durum.RakipActiMi()}");
            Debug.WriteLine($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Debug.WriteLine($"Kupa uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Debug.WriteLine("────────────────────────────────────────");

            var michaels = new Michaels();
            michaels.AktifMi = true;

            bool uygun = michaels.UygunMu(durum);
            Debug.WriteLine($"Michaels.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = michaels.TeklifVer(durum);
                Debug.WriteLine($"Michaels.TeklifVer → {teklif}");
                Debug.WriteLine(teklif == "2♦" ? "✅ BEKLENEN: 2♦" : $"❌ BEKLENEN: 2♦, GELEN: {teklif}");
            }
            else
            {
                Debug.WriteLine("❌ Michaels uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Debug.WriteLine("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Debug.WriteLine($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Debug.WriteLine(motorTeklif == "2♦" ? "✅ MOTOR: 2♦" : $"❌ MOTOR: 2♦ beklendi, gelen {motorTeklif}");

            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Michaels_Negatif()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 4.3 — Michaels Negatif (5-5 yok)");
            Debug.WriteLine("════════════════════════════════════════");

            // ── Güney'in eli: 4♥ + 5♦ (Michaels için yetersiz) ────────────
            var guneyEli = new List<Card>
    {
        // ♠ xx
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 3 },
        // ♥ KQxx = 3+2 = 5 HP (sadece 4'lü!)
        new Card { Suit = "Kupa", Value = 13 },
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 7  },
        new Card { Suit = "Kupa", Value = 5  },
        // ♦ AQxxx = 4+2 = 6 HP
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 12 },
        new Card { Suit = "Karo", Value = 8  },
        new Card { Suit = "Karo", Value = 5  },
        new Card { Suit = "Karo", Value = 3  },
        // ♣ xx
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 5+6 = 11 HP, Maça: 2, Kupa: 4, Karo: 5, Sinek: 2

            // ── Geçmiş: BATI 1♠ (rakip), Kuzey Pas ────────────────────────
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // ── Ön kontroller ─────────────────────────────────────────────
            Debug.WriteLine($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Debug.WriteLine($"Kupa uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Debug.WriteLine($"Karo uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Karo")}");
            Debug.WriteLine("────────────────────────────────────────");

            var michaels = new Michaels();
            michaels.AktifMi = true;

            bool uygun = michaels.UygunMu(durum);
            Debug.WriteLine($"Michaels.UygunMu → {uygun}");
            Debug.WriteLine(!uygun ? "✅ BEKLENEN: False (5♥ yok)"
                                    : "❌ HATA: Michaels uygun olmamalıydı!");

            // ── Motor testi ───────────────────────────────────────────────
            Debug.WriteLine("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Debug.WriteLine($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Debug.WriteLine(motorTeklif != "2♠"
                ? "✅ MOTOR: 2♠ DEĞİL (doğru — Michaels devre dışı)"
                : "❌ MOTOR: 2♠ döndü — Michaels yanlış tetiklendi!");

            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_TemelMantik_YeniRenk()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 5.1 — Partner 1♣ açtı, benim 4♠ var");
            Debug.WriteLine("════════════════════════════════════════");

            var guneyEli = new List<Card>
    {
        // ♠ KQxx = 5 HP
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 12 },
        new Card { Suit = "Maça", Value = 7  },
        new Card { Suit = "Maça", Value = 5  },
        // ♥ xxx = 0
        new Card { Suit = "Kupa", Value = 7  },
        new Card { Suit = "Kupa", Value = 5  },
        new Card { Suit = "Kupa", Value = 3  },
        // ♦ Axx = 4 HP
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 6  },
        new Card { Suit = "Karo", Value = 3  },
        // ♣ Qxx = 2 HP
        new Card { Suit = "Sinek", Value = 12 },
        new Card { Suit = "Sinek", Value = 8  },
        new Card { Suit = "Sinek", Value = 4  },
    };
            // HCP: 5+0+4+2 = 11 HP

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Beklenen: 1♠ (yeni renk)");
            Debug.WriteLine("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string teklif = motor.TeklifVer(durum);
            Debug.WriteLine($"Sonuç: {teklif}");
            Debug.WriteLine(teklif == "1♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 1♠)");
            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_TemelMantik_NTInvite()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 5.2 — Partner 1♠ açtı, dengeli 10 HP");
            Debug.WriteLine("════════════════════════════════════════");

            var guneyEli = new List<Card>
    {
        // ♠ xx
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        // ♥ QJx = 3 HP
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 11 },
        new Card { Suit = "Kupa", Value = 4  },
        // ♦ Axxx = 4 HP
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 7  },
        new Card { Suit = "Karo", Value = 5  },
        new Card { Suit = "Karo", Value = 3  },
        // ♣ Kxx = 3 HP
        new Card { Suit = "Sinek", Value = 13 },
        new Card { Suit = "Sinek", Value = 9  },
        new Card { Suit = "Sinek", Value = 6  },
        // ♠ xx (devam)
        new Card { Suit = "Maça", Value = 3  },
    };
            // HCP: 3+4+3 = 10 HP, dengeli 2-3-4-3

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Dengeli: {ElDegerlendirici.DengeliEl(guneyEli)}");
            Debug.WriteLine($"Beklenen: 2NT (invite)");
            Debug.WriteLine("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string teklif = motor.TeklifVer(durum);
            Debug.WriteLine($"Sonuç: {teklif}");
            Debug.WriteLine(teklif == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_TemelMantik_RakipMudahale()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 5.3 — Rakip 1♥ açtı, benim 5♠ var");
            Debug.WriteLine("════════════════════════════════════════");

            var guneyEli = new List<Card>
    {
        // ♠ KQJxx = 5 HP
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 12 },
        new Card { Suit = "Maça", Value = 11 },
        new Card { Suit = "Maça", Value = 7  },
        new Card { Suit = "Maça", Value = 4  },
        // ♥ xx
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 3 },
        // ♦ Axxx = 4 HP
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 7  },
        new Card { Suit = "Karo", Value = 5  },
        new Card { Suit = "Karo", Value = 2  },
        // ♣ Qx = 2 HP
        new Card { Suit = "Sinek", Value = 12 },
        new Card { Suit = "Sinek", Value = 6  },
    };
            // HCP: 5+0+4+2 = 11 HP

            // Rakip BATI 1♥ açtı
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Beklenen: 1♠ (müdahale)");
            Debug.WriteLine("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string teklif = motor.TeklifVer(durum);
            Debug.WriteLine($"Sonuç: {teklif}");
            Debug.WriteLine(teklif == "1♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 1♠)");
            Debug.WriteLine("════════════════════════════════════════");
        }
        // ═══════════════════════════════════════════════════════════════════
        // TEST 6.1 — Unusual 2NT (4 varyant: 1♣, 1♦, 1♥, 1♠)
        // ═══════════════════════════════════════════════════════════════════

        private static List<Card> ElOlustur(
            int macaAdet, int kupaAdet, int karoAdet, int sinekAdet)
        {
            // Basit yardımcı: 13 kart oluştur (düşük kartlarla doldurur)
            var el = new List<Card>();
            for (int i = 0; i < macaAdet; i++)
                el.Add(new Card { Suit = "Maça", Value = 8 - i });
            for (int i = 0; i < kupaAdet; i++)
                el.Add(new Card { Suit = "Kupa", Value = 8 - i });
            for (int i = 0; i < karoAdet; i++)
                el.Add(new Card { Suit = "Karo", Value = 8 - i });
            for (int i = 0; i < sinekAdet; i++)
                el.Add(new Card { Suit = "Sinek", Value = 8 - i });
            return el;
        }

        public static void Test_SupportDouble()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 6.2 — Support Double (Partner 1♥, Rakip 1♠)");
            Debug.WriteLine("════════════════════════════════════════");

            var guneyEli = new List<Card>
    {
        // ♠ xx
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 3 },
        // ♥ Kxx = 3 HP (TAM 3'lü!)
        new Card { Suit = "Kupa", Value = 13 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // ♦ Axxx = 4 HP
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        // ♣ Qxxx = 2 HP
        new Card { Suit = "Sinek", Value = 12 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 2 },
    };
            // 9 HP, 3'lü Kupa desteği

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "1♠", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Kupa: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Debug.WriteLine("Beklenen: Dbl");
            Debug.WriteLine("────────────────────────────────────────");

            var sd = new SupportDouble { AktifMi = true };
            Debug.WriteLine($"SupportDouble.UygunMu → {sd.UygunMu(durum)}");
            if (sd.UygunMu(durum))
                Debug.WriteLine($"SupportDouble.TeklifVer → {sd.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Debug.WriteLine($"Motor → {sonuc}");
            Debug.WriteLine(sonuc == "Dbl" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Dbl)");
            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Unusual2NT_1Sinek()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 6.1a — Unusual 2NT (Rakip 1♣)");
            Debug.WriteLine("════════════════════════════════════════");

            // Rakip 1♣ → 2NT = 5♥ + 5♦ (2♠ + 5♥ + 5♦ + 1♣)
            var guneyEli = ElOlustur(macaAdet: 2, kupaAdet: 5, karoAdet: 5, sinekAdet: 1);

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine("Beklenen: 2NT (5♥ + 5♦)");
            Debug.WriteLine("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Debug.WriteLine($"Motor → {sonuc}");
            Debug.WriteLine(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Debug.WriteLine("════════════════════════════════════════");
        }

        public static void Test_Unusual2NT_1Karo()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 6.1b — Unusual 2NT (Rakip 1♦)");
            Debug.WriteLine("════════════════════════════════════════");

            // Rakip 1♦ → 2NT = 5♥ + 5♣ (2♠ + 5♥ + 5♣ + 1♦)
            var guneyEli = ElOlustur(macaAdet: 2, kupaAdet: 5, karoAdet: 1, sinekAdet: 5);

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine("Beklenen: 2NT (5♥ + 5♣)");
            Debug.WriteLine("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Debug.WriteLine($"Motor → {sonuc}");
            Debug.WriteLine(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Debug.WriteLine("════════════════════════════════════════");
        }

        public static void Test_Unusual2NT_1Kupa()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 6.1c — Unusual 2NT (Rakip 1♥)");
            Debug.WriteLine("════════════════════════════════════════");

            // Rakip 1♥ → 2NT = 5♦ + 5♣ (3♠ + 1♥ + 5♦ + 5♣)
            var guneyEli = ElOlustur(macaAdet: 2, kupaAdet: 1, karoAdet: 5, sinekAdet: 5);

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine("Beklenen: 2NT (5♦ + 5♣)");
            Debug.WriteLine("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Debug.WriteLine($"Motor → {sonuc}");
            Debug.WriteLine(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Debug.WriteLine("════════════════════════════════════════");
        }

        public static void Test_Unusual2NT_1Maca()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 6.1d — Unusual 2NT (Rakip 1♠)");
            Debug.WriteLine("════════════════════════════════════════");

            // Rakip 1♠ → 2NT = 5♦ + 5♣ (2♠ + 1♥ + 5♦ + 5♣)
            var guneyEli = ElOlustur(macaAdet: 2, kupaAdet: 1, karoAdet: 5, sinekAdet: 5);

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine("Beklenen: 2NT (5♦ + 5♣)");
            Debug.WriteLine("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Debug.WriteLine($"Motor → {sonuc}");
            Debug.WriteLine(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_MinorTransfer_Sinek()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 7.1 — Minor Transfer (Sinek)");
            Debug.WriteLine("════════════════════════════════════════");

            // Partner 1NT açtı, ben 5+ Sinek
            var guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 4 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        new Card { Suit = "Sinek", Value = 12 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 4 },
        new Card { Suit = "Sinek", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"Sinek: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Sinek")}");
            Debug.WriteLine("Beklenen: 2♠");
            Debug.WriteLine("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Debug.WriteLine($"Motor → {sonuc}");
            Debug.WriteLine(sonuc == "2♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♠)");
            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_MinorTransfer_Karo()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 7.2 — Minor Transfer (Karo)");
            Debug.WriteLine("════════════════════════════════════════");

            // Partner 1NT açtı, ben 5+ Karo
            var guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 4 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 2 },
        new Card { Suit = "Karo", Value = 12 },
        new Card { Suit = "Karo", Value = 11 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Karo", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"Karo: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Karo")}");
            Debug.WriteLine("Beklenen: 2NT");
            Debug.WriteLine("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Debug.WriteLine($"Motor → {sonuc}");
            Debug.WriteLine(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_Drury()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 7.3 — Drury (Pas - 1♠ - Pas - 2♣)");
            Debug.WriteLine("════════════════════════════════════════");

            // Batı Pas - Kuzey 1♠ - Doğu Pas - Güney 2♣ (Drury)
            var guneyEli = new List<Card>
    {
        // 3'lü Maça desteği + 11 HP
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 12 }, // Q = 2  ← Q eklendi
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // Toplam: 2+4+3+2 = 11 HP ✅

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine($"Maça: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Debug.WriteLine("Beklenen: 2♣");
            Debug.WriteLine("────────────────────────────────────────");

            // Drury'i doğrudan test et
            var drury = new Drury { AktifMi = true };
            Debug.WriteLine($"Drury.UygunMu → {drury.UygunMu(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Debug.WriteLine($"Motor → {sonuc}");
            Debug.WriteLine(sonuc == "2♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♣)");
            Debug.WriteLine("════════════════════════════════════════");
        }
        public static void Test_ResponsiveDouble()
        {
            Debug.WriteLine("════════════════════════════════════════");
            Debug.WriteLine("TEST 6.3 — Responsive Double (Rakip 1♠ - Pas - 2♠)");
            Debug.WriteLine("════════════════════════════════════════");

            var guneyEli = new List<Card>
    {
        // ♠ xx
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 3 },
        // ♥ KQxx = 5 HP
        new Card { Suit = "Kupa", Value = 13 },
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        // ♦ Axxxx = 4 HP (5'li)
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Karo", Value = 2 },
        // ♣ xx
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
    };
            // 9 HP, 4♥ + 5♦

            // Rakip (Batı) 1♠ açtı, partner (Kuzey) Pas, rakip (Doğu) 2♠ dedi
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "2♠", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Debug.WriteLine($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Debug.WriteLine("Beklenen: Dbl");
            Debug.WriteLine("────────────────────────────────────────");

            var rd = new ResponsiveDouble { AktifMi = true };
            Debug.WriteLine($"ResponsiveDouble.UygunMu → {rd.UygunMu(durum)}");
            if (rd.UygunMu(durum))
                Debug.WriteLine($"ResponsiveDouble.TeklifVer → {rd.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Debug.WriteLine($"Motor → {sonuc}");
            Debug.WriteLine(sonuc == "Dbl" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Dbl)");
            Debug.WriteLine("════════════════════════════════════════");
        }
    }
}