using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace BricKartOyunu
{
    /// <summary>
    /// Geçici test sınıfı — konvansiyonları doğrulamak için.
    /// Testler tamamlandıktan sonra silinebilir.
    /// </summary>
    public static class TestRunner
    {
        /// <summary>
        /// Test sonuçlarını yakalayan callback (form tarafından atanır).
        /// </summary>
        public static Action<string> SonucYaz { get; set; }

        /// <summary>
        /// Hem Debug'a hem callback'e yazan yardımcı.
        /// </summary>
        private static void Yaz(string mesaj)
        {
            Debug.WriteLine(mesaj);
            SonucYaz?.Invoke(mesaj);
        }
        public static void Test_Gerber_SoruSorma()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 1.1 — Gerber Soru Sorma");
            Yaz("════════════════════════════════════════");

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
            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Dengeli mi?: {ElDegerlendirici.DengeliEl(guneyEli)}");
            Yaz($"AnlasilanKoz: {durum.AnlasilanKoz() ?? "null"}");
            Yaz($"KacinciSeviye: {durum.KacinciSeviye()}");
            Yaz($"SonGercekTeklif: {durum.SonGercekTeklif ?? "null"}");
            Yaz($"SonGercekTeklifSahibi: {durum.SonGercekTeklifSahibi}");
            Yaz($"Partner: {durum.Partner}");
            Yaz($"KendiTeklifleri: [{string.Join(", ", durum.KendiTeklifleri)}]");
            Yaz("────────────────────────────────────────");

            // ── Gerber'i doğrudan test et ─────────────────────────────────
            var gerber = new Gerber();
            gerber.AktifMi = true;

            bool uygun = gerber.UygunMu(durum);
            Yaz($"Gerber.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = gerber.TeklifVer(durum);
                Yaz($"Gerber.TeklifVer → {teklif}");
                Yaz(teklif == "4♣" ? "✅ BEKLENEN: 4♣" : $"❌ BEKLENEN: 4♣, GELEN: {teklif}");
            }
            else
            {
                Yaz("❌ Gerber uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif == "4♣" ? "✅ MOTOR: 4♣" : $"❌ MOTOR: 4♣ beklendi, gelen {motorTeklif}");

            Yaz("════════════════════════════════════════");
        }

        public static void Test_Gerber_Cevap()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 1.2 — Gerber Cevap Verme");
            Yaz("════════════════════════════════════════");

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
            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"AsSayisi: {ElDegerlendirici.AsSayisi(kuzeyEli)}");
            Yaz($"SonGercekTeklif: {durum.SonGercekTeklif}");
            Yaz($"SonGercekTeklifSahibi: {durum.SonGercekTeklifSahibi}");
            Yaz($"Partner (Guney): {durum.Partner}");
            Yaz($"AnlasilanKoz: {durum.AnlasilanKoz() ?? "null"}");
            Yaz("────────────────────────────────────────");

            // ── Gerber'i doğrudan test et ─────────────────────────────────
            var gerber = new Gerber();
            gerber.AktifMi = true;

            bool uygun = gerber.UygunMu(durum);
            Yaz($"Gerber.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = gerber.TeklifVer(durum);
                Yaz($"Gerber.TeklifVer → {teklif}");
                Yaz(teklif == "4♠" ? "✅ BEKLENEN: 4♠" : $"❌ BEKLENEN: 4♠, GELEN: {teklif}");
            }
            else
            {
                Yaz("❌ Gerber uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif == "4♠" ? "✅ MOTOR: 4♠" : $"❌ MOTOR: 4♠ beklendi, gelen {motorTeklif}");

            Yaz("════════════════════════════════════════");
        }
        public static void Test_Gerber_Negatif()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 1.3 — Gerber Negatif (Koz Maça)");
            Yaz("════════════════════════════════════════");

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
            Yaz($"AnlasilanKoz: {durum.AnlasilanKoz() ?? "null"}");
            Yaz($"KacinciSeviye: {durum.KacinciSeviye()}");
            Yaz("────────────────────────────────────────");

            // ── Gerber uygun OLMAMALI ─────────────────────────────────────
            var gerber = new Gerber();
            gerber.AktifMi = true;

            bool uygun = gerber.UygunMu(durum);
            Yaz($"Gerber.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ BEKLENEN: False (koz Maça, Gerber devre dışı)"
                                    : "❌ HATA: Gerber uygun olmamalıydı!");

            // ── Motor Gerber kullanmamalı ─────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif != "4♣"
                ? "✅ MOTOR: 4♣ DEĞİL (doğru)"
                : "❌ MOTOR: 4♣ döndü — Gerber yanlış tetiklendi!");

            Yaz("════════════════════════════════════════");
        }
        public static void Test_Jacoby2NT_Pozitif()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 2.1 — Jacoby 2NT Pozitif");
            Yaz("════════════════════════════════════════");

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
            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz($"PartnerTeklifleri: [{string.Join(", ", durum.PartnerTeklifleri)}]");
            Yaz($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Yaz($"KendiTeklifleri: [{string.Join(", ", durum.KendiTeklifleri)}]");
            Yaz("────────────────────────────────────────");

            // ── Jacoby2NT'yi doğrudan test et ─────────────────────────────
            var jacoby = new Jacoby2NT();
            jacoby.AktifMi = true;

            bool uygun = jacoby.UygunMu(durum);
            Yaz($"Jacoby2NT.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = jacoby.TeklifVer(durum);
                Yaz($"Jacoby2NT.TeklifVer → {teklif}");
                Yaz(teklif == "2NT" ? "✅ BEKLENEN: 2NT" : $"❌ BEKLENEN: 2NT, GELEN: {teklif}");
            }
            else
            {
                Yaz("❌ Jacoby2NT uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif == "2NT" ? "✅ MOTOR: 2NT" : $"❌ MOTOR: 2NT beklendi, gelen {motorTeklif}");

            Yaz("════════════════════════════════════════");
        }
        public static void Test_Jacoby2NT_Negatif_3luDestek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 2.2 — Jacoby 2NT Negatif (3'lü destek)");
            Yaz("════════════════════════════════════════");

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

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz("────────────────────────────────────────");

            var jacoby = new Jacoby2NT();
            jacoby.AktifMi = true;

            bool uygun = jacoby.UygunMu(durum);
            Yaz($"Jacoby2NT.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ BEKLENEN: False (3'lü destek)"
                                    : "❌ HATA: Jacoby2NT uygun olmamalıydı!");

            // ── Motor testi ───────────────────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif != "2NT"
                ? "✅ MOTOR: 2NT DEĞİL (doğru)"
                : "❌ MOTOR: 2NT döndü — Jacoby yanlış tetiklendi!");

            Yaz("════════════════════════════════════════");
        }
        public static void Test_Jacoby2NT_Negatif_RakipMudahalesi()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 2.3 — Jacoby 2NT Negatif (Rakip müdahalesi)");
            Yaz("════════════════════════════════════════");

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

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Yaz("────────────────────────────────────────");

            var jacoby = new Jacoby2NT();
            jacoby.AktifMi = true;

            bool uygun = jacoby.UygunMu(durum);
            Yaz($"Jacoby2NT.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ BEKLENEN: False (rakip müdahale etti)"
                                    : "❌ HATA: Jacoby2NT uygun olmamalıydı!");

            Yaz("════════════════════════════════════════");
        }
        public static void Test_Splinter_Maca_Sinek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 3.1 — Splinter (Koz Maça, Kısa Sinek)");
            Yaz("════════════════════════════════════════");

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
            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            var sayilar = ElDegerlendirici.RenkSayilari(guneyEli);
            Yaz($"Dağılım: ♠{sayilar["Maça"]} ♥{sayilar["Kupa"]} ♦{sayilar["Karo"]} ♣{sayilar["Sinek"]}");
            Yaz($"PartnerTeklifleri: [{string.Join(", ", durum.PartnerTeklifleri)}]");
            Yaz($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Yaz("────────────────────────────────────────");

            var splinter = new Splinter();
            splinter.AktifMi = true;

            bool uygun = splinter.UygunMu(durum);
            Yaz($"Splinter.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = splinter.TeklifVer(durum);
                Yaz($"Splinter.TeklifVer → {teklif}");
                Yaz(teklif == "4♣" ? "✅ BEKLENEN: 4♣" : $"❌ BEKLENEN: 4♣, GELEN: {teklif}");
            }
            else
            {
                Yaz("❌ Splinter uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif == "4♣" ? "✅ MOTOR: 4♣" : $"❌ MOTOR: 4♣ beklendi, gelen {motorTeklif}");

            Yaz("════════════════════════════════════════");
        }
        public static void Test_Splinter_Kupa_Maca()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 3.2 — Splinter (Koz Kupa, Kısa Maça)");
            Yaz("════════════════════════════════════════");

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

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Kupa uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            var sayilar = ElDegerlendirici.RenkSayilari(guneyEli);
            Yaz($"Dağılım: ♠{sayilar["Maça"]} ♥{sayilar["Kupa"]} ♦{sayilar["Karo"]} ♣{sayilar["Sinek"]}");
            Yaz("────────────────────────────────────────");

            var splinter = new Splinter();
            splinter.AktifMi = true;

            bool uygun = splinter.UygunMu(durum);
            Yaz($"Splinter.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = splinter.TeklifVer(durum);
                Yaz($"Splinter.TeklifVer → {teklif}");
                Yaz(teklif == "3♠" ? "✅ BEKLENEN: 3♠" : $"❌ BEKLENEN: 3♠, GELEN: {teklif}");
            }
            else
            {
                Yaz("❌ Splinter uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif == "3♠" ? "✅ MOTOR: 3♠" : $"❌ MOTOR: 3♠ beklendi, gelen {motorTeklif}");

            Yaz("════════════════════════════════════════");
        }
        public static void Test_Michaels_RakipMaca()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 4.1 — Michaels (Rakip Maça açtı)");
            Yaz("════════════════════════════════════════");

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
            Yaz($"AktifOyuncu: {durum.AktifOyuncu}");
            Yaz($"Rakipler: [{string.Join(", ", durum.Rakipler)}]");
            Yaz($"Partner: {durum.Partner}");
            Yaz($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Yaz($"PartnerTeklifleri: [{string.Join(", ", durum.PartnerTeklifleri)}]");
            Yaz($"KendiTeklifleri: [{string.Join(", ", durum.KendiTeklifleri)}]");
            Yaz($"RakipActiMi: {durum.RakipActiMi()}");
            Yaz("────────────────────────────────────────");

            var michaels = new Michaels();
            michaels.AktifMi = true;

            bool uygun = michaels.UygunMu(durum);
            Yaz($"Michaels.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = michaels.TeklifVer(durum);
                Yaz($"Michaels.TeklifVer → {teklif}");
                Yaz(teklif == "2♠" ? "✅ BEKLENEN: 2♠" : $"❌ BEKLENEN: 2♠, GELEN: {teklif}");
            }
            else
            {
                Yaz("❌ Michaels uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif == "2♠" ? "✅ MOTOR: 2♠" : $"❌ MOTOR: 2♠ beklendi, gelen {motorTeklif}");

            Yaz("════════════════════════════════════════");
        }
        public static void Test_Michaels_RakipKaro()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 4.2 — Michaels (Rakip Karo açtı)");
            Yaz("════════════════════════════════════════");

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
            Yaz($"Rakipler: [{string.Join(", ", durum.Rakipler)}]");
            Yaz($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Yaz($"PartnerTeklifleri: [{string.Join(", ", durum.PartnerTeklifleri)}]");
            Yaz($"RakipActiMi: {durum.RakipActiMi()}");
            Yaz($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz($"Kupa uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Yaz("────────────────────────────────────────");

            var michaels = new Michaels();
            michaels.AktifMi = true;

            bool uygun = michaels.UygunMu(durum);
            Yaz($"Michaels.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = michaels.TeklifVer(durum);
                Yaz($"Michaels.TeklifVer → {teklif}");
                Yaz(teklif == "2♦" ? "✅ BEKLENEN: 2♦" : $"❌ BEKLENEN: 2♦, GELEN: {teklif}");
            }
            else
            {
                Yaz("❌ Michaels uygun değil! Bekleniyordu: uygun");
            }

            // ── Motor üzerinden test ──────────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif == "2♦" ? "✅ MOTOR: 2♦" : $"❌ MOTOR: 2♦ beklendi, gelen {motorTeklif}");

            Yaz("════════════════════════════════════════");
        }
        public static void Test_Michaels_Negatif()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 4.3 — Michaels Negatif (5-5 yok)");
            Yaz("════════════════════════════════════════");

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
            Yaz($"Maça uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz($"Kupa uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Yaz($"Karo uzunluğu: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Karo")}");
            Yaz("────────────────────────────────────────");

            var michaels = new Michaels();
            michaels.AktifMi = true;

            bool uygun = michaels.UygunMu(durum);
            Yaz($"Michaels.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ BEKLENEN: False (5♥ yok)"
                                    : "❌ HATA: Michaels uygun olmamalıydı!");

            // ── Motor testi ───────────────────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif != "2♠"
                ? "✅ MOTOR: 2♠ DEĞİL (doğru — Michaels devre dışı)"
                : "❌ MOTOR: 2♠ döndü — Michaels yanlış tetiklendi!");

            Yaz("════════════════════════════════════════");
        }
        public static void Test_TemelMantik_YeniRenk()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 5.1 — Partner 1♣ açtı, benim 4♠ var");
            Yaz("════════════════════════════════════════");

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

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Beklenen: 1♠ (yeni renk)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string teklif = motor.TeklifVer(durum);
            Yaz($"Sonuç: {teklif}");
            Yaz(teklif == "1♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 1♠)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_TemelMantik_NTInvite()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 5.2 — Partner 1♠ açtı, dengeli 10 HP");
            Yaz("════════════════════════════════════════");

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

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Dengeli: {ElDegerlendirici.DengeliEl(guneyEli)}");
            Yaz($"Beklenen: 2NT (invite)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string teklif = motor.TeklifVer(durum);
            Yaz($"Sonuç: {teklif}");
            Yaz(teklif == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_TemelMantik_RakipMudahale()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 5.3 — Rakip 1♥ açtı, benim 5♠ var");
            Yaz("════════════════════════════════════════");

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

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Beklenen: 1♠ (müdahale)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string teklif = motor.TeklifVer(durum);
            Yaz($"Sonuç: {teklif}");
            Yaz(teklif == "1♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 1♠)");
            Yaz("════════════════════════════════════════");
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
            Yaz("════════════════════════════════════════");
            Yaz("TEST 6.2 — Support Double (Partner 1♥, Rakip 1♠)");
            Yaz("════════════════════════════════════════");

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

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Yaz("Beklenen: Dbl");
            Yaz("────────────────────────────────────────");

            var sd = new SupportDouble { AktifMi = true };
            Yaz($"SupportDouble.UygunMu → {sd.UygunMu(durum)}");
            if (sd.UygunMu(durum))
                Yaz($"SupportDouble.TeklifVer → {sd.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Dbl" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Dbl)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_ResponsiveDouble()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 6.3 — Responsive Double (Rakip 1♠ - Pas - 2♠)");
            Yaz("════════════════════════════════════════");

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

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz("Beklenen: Dbl");
            Yaz("────────────────────────────────────────");

            var rd = new ResponsiveDouble { AktifMi = true };
            Yaz($"ResponsiveDouble.UygunMu → {rd.UygunMu(durum)}");
            if (rd.UygunMu(durum))
                Yaz($"ResponsiveDouble.TeklifVer → {rd.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Dbl" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Dbl)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Unusual2NT_1Sinek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 6.1a — Unusual 2NT (Rakip 1♣)");
            Yaz("════════════════════════════════════════");

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

            Yaz("Beklenen: 2NT (5♥ + 5♦)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Unusual2NT_1Karo()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 6.1b — Unusual 2NT (Rakip 1♦)");
            Yaz("════════════════════════════════════════");

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

            Yaz("Beklenen: 2NT (5♥ + 5♣)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Unusual2NT_1Kupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 6.1c — Unusual 2NT (Rakip 1♥)");
            Yaz("════════════════════════════════════════");

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

            Yaz("Beklenen: 2NT (5♦ + 5♣)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Unusual2NT_1Maca()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 6.1d — Unusual 2NT (Rakip 1♠)");
            Yaz("════════════════════════════════════════");

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

            Yaz("Beklenen: 2NT (5♦ + 5♣)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_MinorTransfer_Sinek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 7.1 — Minor Transfer (Sinek)");
            Yaz("════════════════════════════════════════");

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

            Yaz($"Sinek: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Sinek")}");
            Yaz("Beklenen: 2♠");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♠)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_MinorTransfer_Karo()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 7.2 — Minor Transfer (Karo)");
            Yaz("════════════════════════════════════════");

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

            Yaz($"Karo: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Karo")}");
            Yaz("Beklenen: 2NT");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Drury()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 7.3 — Drury (Pas - 1♠ - Pas - 2♣)");
            Yaz("════════════════════════════════════════");

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

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Maça: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz("Beklenen: 2♣");
            Yaz("────────────────────────────────────────");

            // Drury'i doğrudan test et
            var drury = new Drury { AktifMi = true };
            Yaz($"Drury.UygunMu → {drury.UygunMu(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♣)");
            Yaz("════════════════════════════════════════");
        }
        
        public static void Test_Smolen_5Maca_4Kupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 8.1 — Smolen (5♠ + 4♥)");
            Yaz("════════════════════════════════════════");

            // Partner 1NT - Ben 2♣ - Partner 2♦ - Ben 3♥
            var guneyEli = new List<Card>
    {
        // 5♠
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 4♥
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Kupa", Value = 3 },
        // 2♦
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        // 2♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // Toplam: 3+1+4 = 8 HP

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Maça: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz("Beklenen: 3♥");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♥)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Smolen_5Kupa_4Maca()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 8.2 — Smolen (5♥ + 4♠)");
            Yaz("════════════════════════════════════════");

            // Partner 1NT - Ben 2♣ - Partner 2♦ - Ben 3♠
            var guneyEli = new List<Card>
    {
        // 4♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 5♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Kupa", Value = 3 },
        // 2♦
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        // 2♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // Toplam: 4+3+1 = 8 HP

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Maça: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz("Beklenen: 3♠");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♠)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_PuppetStayman()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.1 — Puppet Stayman Soru (2NT - 3♣)");
            Yaz("════════════════════════════════════════");

            // Partner 2NT açtı, ben 4'lü majör ile 3♣ diyeceğim
            var guneyEli = new List<Card>
    {
        // 4♠
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3♥
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4♦
        new Card { Suit = "Karo", Value = 11 },
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 3 },
        // 2♣
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 4 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Maça: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Yaz("Beklenen: 3♣");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♣)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Puppet_5liKupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.2 — Puppet: Açıcı 5'li ♥ ile cevap");
            Yaz("════════════════════════════════════════");

            // Kuzey 2NT açtı, Güney 3♣ sordu, sıra Kuzey'de
            var kuzeyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 13 },
        new Card { Suit = "Kupa", Value = 11 },
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 12 },
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 2♦
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 6 },
        // 2♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 4 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Maça: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Kupa")}");
            Yaz("Beklenen: 3♥ (5'li ♥ var)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♥)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Puppet_4luMajor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.3 — Puppet: Açıcı 3♦ (4'lü majör var)");
            Yaz("════════════════════════════════════════");

            // Kuzey 2NT açtı, Güney 3♣ sordu, sıra Kuzey'de
            // Kuzey'in eli: 4'lü ♠ + 4'lü ♥ + 5'li ♦
            var kuzeyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 11 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Maça: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Kupa")}");
            Yaz("Beklenen: 3♦ (4'lü majör var, 5'li yok)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♦)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Puppet_2Tur_4luMaca()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.4 — Puppet: 3♦ sonrası 3♥ (4'lü ♠ sorusu)");
            Yaz("════════════════════════════════════════");

            // 2NT - 3♣ - 3♦ - ? (Güney'in sırası)
            // Güney'in eli: 4'lü ♠ + dengeli, 4'lü ♥ yok
            var guneyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 14 },
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 13 },
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 4 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Maça: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Yaz("Beklenen: 3♥ (4'lü ♠ sorusu)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♥)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Puppet_Aşama3_TutuşVar()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.5 — Puppet Aşama 3: 4'lü ♠ tutuş VAR");
            Yaz("════════════════════════════════════════");

            // 2NT (Kuzey) - 3♣ (Güney) - 3♦ (Kuzey) - 3♥ (Güney) - ? (Kuzey)
            // Kuzey'in eli: 4'lü ♠ + 4'lü ♥ + 5'li ♦
            var kuzeyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 11 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♥",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Maça: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Kupa")}");
            Yaz("Beklenen: 3♠ (4'lü ♠ tutuş var)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Puppet_Aşama3_TutuşYok()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.6 — Puppet Aşama 3: 4'lü ♠ tutuş YOK");
            Yaz("════════════════════════════════════════");

            // 2NT (Kuzey) - 3♣ (Güney) - 3♦ (Kuzey) - 3♥ (Güney) - ? (Kuzey)
            // Kuzey'in eli: 3'lü ♠ + 4'lü ♥ (yani ♠ yok, sadece ♥ var)
            var kuzeyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 2 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♥",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Maça: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Kupa")}");
            Yaz("Beklenen: 3NT (4'lü ♠ tutuş yok)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3NT)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Puppet_Aşama4_Kontrol()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.7 — Puppet Aşama 4: Kontrol gösterme");
            Yaz("════════════════════════════════════════");

            // 2NT (Kuzey) - 3♣ (Güney) - 3♦ (Kuzey) - 3♥ (Güney) - 3♠ (Kuzey) - ? (Güney)
            // Güney'in eli: 4'lü ♠ + ♣ A var
            var guneyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 11 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣ (A)
        new Card { Suit = "Sinek", Value = 14 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♥",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 9, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 10, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz("Beklenen: 4♣ (♣ A kontrol)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♣)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Puppet_5liKupa_Kontrol()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.8 — Puppet 5'li ♥ sonrası 4♣ (♣ kontrol)");
            Yaz("════════════════════════════════════════");

            // 2NT (Kuzey) - 3♣ (Güney) - 3♥ (Kuzey 5'li ♥) - ? (Güney)
            // Güney'in eli: 3'lü ♥ + ♣ A
            var guneyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 11 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♣ (A)
        new Card { Suit = "Sinek", Value = 14 }, // A
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♥",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Yaz("Beklenen: 4♣ (♣ A kontrol)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Puppet_5liKupa_SignOff()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.9 — Puppet 5'li ♥ sonrası 3NT (fit yok)");
            Yaz("════════════════════════════════════════");

            // 2NT (Kuzey) - 3♣ (Güney) - 3♥ (Kuzey 5'li ♥) - ? (Güney)
            // Güney'in eli: 2'li ♥ (fit yok)
            var guneyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 13 },
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 2'li ♥ (fit YOK)
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 4 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 11 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 14 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♥",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Yaz("Beklenen: 3NT (fit yok)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3NT)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Puppet_KeyCard_2()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.10 — Puppet Key-Card: 2 Key-Card");
            Yaz("════════════════════════════════════════");

            // 2NT (Kuzey) - 3♣ (Güney) - 3♦ (Kuzey) - 4♣ (Güney) - ? (Kuzey)
            // Kuzey'in eli: 1 As + 1 majör K = 2 Key-Card
            var kuzeyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 14 }, // A
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 13 }, // K
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 6 },
    };
            // 6 HP, 2 Key-Card (♠ A + ♥ K)

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♣",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"As: {ElDegerlendirici.AsSayisi(kuzeyEli)}");
            Yaz("Beklenen: 4♦ (2 Key-Card)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Puppet_KeyCard_3()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.11 — Puppet Key-Card: 3 Key-Card");
            Yaz("════════════════════════════════════════");

            // Kuzey'in eli: 2 As + 1 majör K = 3 Key-Card
            var kuzeyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 14 }, // A
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 13 }, // K (minör, sayılmaz)
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 6 },
    };
            // 2 As + 1 majör K (yok, çünkü K Karo'da) = 2 Key-Card
            // Ama biz 3 istiyoruz: 2 As + 1 majör K

            // Yeniden yapalım: 2 As + 1 majör K (♠ K)
            kuzeyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A
        new Card { Suit = "Maça", Value = 13 }, // K (majör)
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 14 }, // A
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 6 },
    };
            // 2 As + 1 majör K = 3 Key-Card

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♣",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"As: {ElDegerlendirici.AsSayisi(kuzeyEli)}");
            Yaz("Beklenen: 4♠ (3 Key-Card)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Puppet_KeyCard_4_1Q()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.12 — Puppet Key-Card: 4 Key-Card + 1 Q");
            Yaz("════════════════════════════════════════");

            // Kuzey'in eli: 3 As + 1 majör K + 1 majör Q = 4 Key-Card + 1 Q
            var kuzeyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A
        new Card { Suit = "Maça", Value = 12 }, // Q (majör)
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 14 }, // A
        new Card { Suit = "Kupa", Value = 13 }, // K (majör)
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 14 }, // A
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 6 },
    };
            // 3 As + 1 majör K = 4 Key-Card
            // + 1 majör Q (♠ Q) = 1 Q

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♣",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"As: {ElDegerlendirici.AsSayisi(kuzeyEli)}");
            Yaz("Beklenen: 5♣ (4 Key-Card + 1 Q)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♣)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_GSF_Soru()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.3 — Grand Slam Force (Soru)");
            Yaz("════════════════════════════════════════");

            // Ben Blackwood 4NT dedim, partner 5♠ (2 As) dedi, ben 5NT diyeceğim
            var guneyEli = new List<Card>
    {
        // 5♠ (kozal)
        new Card { Suit = "Maça", Value = 14 }, // A
        new Card { Suit = "Maça", Value = 13 }, // K
        new Card { Suit = "Maça", Value = 12 }, // Q
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 3 },
        // 4♥
        new Card { Suit = "Kupa", Value = 14 }, // A
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        // 2♦
        new Card { Suit = "Karo", Value = 13 }, // K
        new Card { Suit = "Karo", Value = 6 },
        // 2♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3, GecerliMi = true }, // destek
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4♠", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 7, GecerliMi = true }, // Blackwood
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "5♠", Sira = 9, GecerliMi = true }, // 2 As
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 10, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz("Beklenen: 5NT");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5NT)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_GSF_Cevap()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.4 — Grand Slam Force (Cevap)");
            Yaz("════════════════════════════════════════");

            // Partner 5NT dedi, ben Papaz sayısını söyleyeceğim
            var kuzeyEli = new List<Card>
    {
        // 2 Papaz
        new Card { Suit = "Maça", Value = 13 }, // K
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 13 }, // K
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // 2 Papaz → 6♥

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4♠", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "5♣", Sira = 9, GecerliMi = true }, // 0/3 As
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 10, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5NT", Sira = 11, GecerliMi = true }, // GSF
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 12, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"Papaz sayısı: {ElDegerlendirici.PapazSayisi(kuzeyEli)}");
            Yaz("Beklenen: 6♥ (2 Papaz)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "6♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 6♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Stayman_2Tur_NTInvite()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 11.1 — Stayman 2. tur: 2♦ sonrası 2NT (invite)");
            Yaz("════════════════════════════════════════");

            // 1NT (Kuzey) - 2♣ (Güney) - 2♦ (Kuzey) - ? (Güney)
            // Güney: 4'lü ♠ + 4'lü ♥ + 8-9 HP → 2NT invite
            var guneyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 2+1+3 = 6 HP... invite için 8-9 lazım
            // Değiştir: ♦ K yerine ♦ A (4 HP), ♣ 10 yerine ♣ Q (2 HP)
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 2+1+4+2 = 9 HP ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz("Beklenen: 2NT (invite)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Stayman_2Tur_NTGame()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 11.2 — Stayman 2. tur: 2♦ sonrası 3NT (game)");
            Yaz("════════════════════════════════════════");

            // Güney: 4'lü majör + 10+ HP → 3NT
            var guneyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 3+2+4+1 = 10 HP ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz("Beklenen: 3NT (game)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Stayman_2Tur_FitBulundu()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 11.3 — Stayman 2. tur: 2♥ sonrası 4♥ (fit + game)");
            Yaz("════════════════════════════════════════");

            // 1NT (Kuzey) - 2♣ (Güney) - 2♥ (Kuzey) - ? (Güney)
            // Güney: 4'lü ♥ fit + 10+ HP → 4♥
            var guneyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 3+1+4+2 = 10 HP ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♥",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Kupa: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            Yaz("Beklenen: 4♥ (fit + game)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_Basit()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 12.1 — Lebensohl: 1NT - 2♥ - 2NT (zayıf)");
            Yaz("════════════════════════════════════════");

            // Partner 1NT açtı, rakip 2♥ girdi
            // Güney: zayıf (0-7 OP), 2NT Lebensohl diyecek
            var guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 3 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // ~3 HP, zayıf

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "2♥",  Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz("Beklenen: 2NT (Lebensohl)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_3NT_DurdurucuVar()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 12.2 — Lebensohl: 1NT - 2♥ - 3NT (durdurucu var)");
            Yaz("════════════════════════════════════════");

            // Partner 1NT, rakip 2♥, Güney 10+ HP + ♥'de durdurucu + 4'lü majör YOK
            var guneyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        // 3'lü ♥ (durdurucu: K)
        new Card { Suit = "Kupa", Value = 13 }, // K
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // 3+4+2 = 9 HP → 10+ lazım, biraz artıralım
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 4 },
    };
            // 3+4+2+1 = 10 HP ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "2♥",  Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"4'lü ♠: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça") >= 4}");
            Yaz($"4'lü ♥: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa") >= 4}");
            Yaz($"♥ Durdurucu: {ElDegerlendirici.StopperVar(guneyEli, "Kupa")}");
            Yaz("Beklenen: 3NT");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_3Hearts_CueBid()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 12.3 — Lebensohl: 1NT - 2♥ - 3♥ (Cue-bid Stayman)");
            Yaz("════════════════════════════════════════");

            // Partner 1NT, rakip 2♥, Güney 10+ HP + 4'lü ♠ + ♥'de durdurucu YOK
            var guneyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 2'li ♥ (durdurucu YOK)
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 4+3+2 = 9 HP → 10+ lazım
            // ♣ Q'yu A yapalım
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 4+3+2+1 = 10 HP ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "2♥",  Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"4'lü ♠: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça") >= 4}");
            Yaz($"♥ Durdurucu: {ElDegerlendirici.StopperVar(guneyEli, "Kupa")}");
            Yaz("Beklenen: 3♥ (Cue-bid Stayman)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_4Hearts_Transfer()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 12.4 — Lebensohl: 1NT - 2♥ - 4♥ (Texas transfer)");
            Yaz("════════════════════════════════════════");

            // 6+ ♠ + 10+ HP
            var guneyEli = new List<Card>
    {
        // 6'lı ♠
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 2'li ♥
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 6 },
    };
            // 3+2+4+1 = 10 HP ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "2♥",  Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz("Beklenen: 4♥ (Texas → 4♠)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_Rakip2Club()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 13.1 — Lebensohl: 1NT - 2♣ - ?");
            Yaz("════════════════════════════════════════");

            // Rakip 2♣, ben 10+ HP + 4'lü ♠ + ♣ durdurucu YOK → 3♣ Cue-bid
            var guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 13 }, // K
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 11 }, // J
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 4+3+2+1 = 10 HP, ♣ durdurucu: QJ → STOPPER VAR!
            // Aslında ♣'de QJ var, stopper VAR.
            // Test için: stopper YOK yapalım, QJ yerine 7-4 yapalım
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 4+3 = 7 HP → 10+ lazım, biraz artıralım
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 9 }, // ♣'de stopper YOK
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 4+1+3 = 8 HP → 10+ lazım, bir tane daha ekleyelim
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 9 }, // ♣'de stopper YOK
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 4+2+3 = 9 HP → hâlâ 10 değil, bir As daha ekleyelim
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // 4+2+3+4 = 13 HP ✓, ♣ stopper YOK

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "2♣",  Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"4'lü ♠: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça") >= 4}");
            Yaz($"♣ Durdurucu: {ElDegerlendirici.StopperVar(guneyEli, "Sinek")}");
            Yaz("Beklenen: 3♣ (Cue-bid Stayman)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_Rakip2Diamond()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 13.2 — Lebensohl: 1NT - 2♦ - 3♦ (Cue-bid)");
            Yaz("════════════════════════════════════════");

            // Rakip 2♦, ben 10+ HP + 4'lü ♥ + ♦ durdurucu YOK
            var guneyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♦ (durdurucu YOK)
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 7 },
    };
            // 4+3+2 = 9 HP → 10+ lazım
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 7 },
    };
            // 4+2+3+2 = 11 HP ✓, ♦ durdurucu YOK

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "2♦",  Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"4'lü ♥: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa") >= 4}");
            Yaz($"♦ Durdurucu: {ElDegerlendirici.StopperVar(guneyEli, "Karo")}");
            Yaz("Beklenen: 3♦ (Cue-bid Stayman)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_WeakTwo_Zayif()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 14.1 — Lebensohl Zayıf 2: 2♥ - Dbl - 2NT (zayıf)");
            Yaz("════════════════════════════════════════");

            // Rakip 2♥, partner DBL, ben 0-6 OP → 2NT Lebensohl
            var guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 3 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // ~2 HP, zayıf

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "2♥",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz("Beklenen: 2NT (Lebensohl)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_WeakTwo_Davet()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 14.2 — Lebensohl Zayıf 2: 2♥ - Dbl - 3♦ (davet)");
            Yaz("════════════════════════════════════════");

            // Rakip 2♥, partner DBL, ben 7-11 OP + 6'lı ♦ → 3♦ davet
            var guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 3 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 6 },
    };
            // 4+3+2 = 9 HP ✓, 6'lı ♦

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "2♥",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"♦ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Karo")}");
            Yaz("Beklenen: 3♦ (davet)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_WeakTwo_CueBid()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 14.3 — Lebensohl Zayıf 2: 2♥ - Dbl - 3♥ (Cue-bid 12+ OP)");
            Yaz("════════════════════════════════════════");

            // Rakip 2♥, partner DBL, ben 12+ OP + 4'lü ♠ → 3♥ Cue-bid
            var guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 7 },
    };
            // 4+1+3+2+1 = 11 HP → 12 yapalım
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 7 },
    };
            // 4+2+3+2+1 = 12 HP ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "2♥",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"4'lü ♠: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça") >= 4}");
            Yaz("Beklenen: 3♥ (Cue-bid, 12+ OP)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_Reverse_Zayif()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 15.1 — Lebensohl Reverse: 1♣-1♠-2♠-2NT (zayıf)");
            Yaz("════════════════════════════════════════");

            // 1♣ (Kuzey) - 1♠ (Güney) - 2♠ (Kuzey) - ? (Güney)
            // Güney: 5-8 OP → 2NT Lebensohl
            var guneyEli = new List<Card>
    {
        // 5'li ♠
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // 3 HP → 5-8 arası, zayıf

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♠", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz("Beklenen: 2NT (Lebensohl zayıf)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Lebensohl_Reverse_Guclu()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 15.2 — Lebensohl Reverse: 1♦-1♠-2♥-3♣ (9+ OP)");
            Yaz("════════════════════════════════════════");

            // 1♦ (Kuzey) - 1♠ (Güney) - 2♥ (Kuzey reverse) - ? (Güney)
            // Güney: 9+ OP + 4+ ♣ → 3♣ forcing
            var guneyEli = new List<Card>
    {
        // 5'li ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 4 },
        new Card { Suit = "Maça", Value = 2 },
        // 2'li ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 3 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        // 4'lü ♣ (KQJ + küçük) ← 4'LÜ YAPTIK
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 4 },
    };
            // 4+3+2+1 = 10 HP ✓, 4'lü ♣, 5'li ♠

            // ... geri kalanı aynı ...
        }

        public static void Test_Lebensohl_Reverse_SignOff()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 15.3 — Lebensohl Reverse: 2NT sonrası 3♠ sign-off");
            Yaz("════════════════════════════════════════");

            // 1♣ - 1♠ - 2♠ - 2NT - Pas - 3♣ - Pas - ? (Güney)
            // Güney: 5'li ♠ ile 3♠ sign-off
            var guneyEli = new List<Card>
    {
        // 5'li ♠
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♠",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2NT", Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♣",  Sira = 9, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 10, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            Yaz("Beklenen: 3♠ (sign-off)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_BasitCevap_6liDestek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 16.1 — BasitCevap: 6'lı ♣ desteği");
            Yaz("════════════════════════════════════════");

            // Aynı el (8 HP, 6'lı ♣)
            var guneyEli = new List<Card>
    {
        new Card { Suit = "Sinek", Value = 14 },
        new Card { Suit = "Sinek", Value = 13 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Karo", Value = 8 },
    };

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

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"♣ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Sinek")}");
            Yaz("Beklenen: 2♣ (basit destek, 6-9 HP)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_OncelikSirasi()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 17.1 - Konvansiyon Öncelik Sırası");
            Yaz("════════════════════════════════════════");

            var anlasma = OrtaklikAnlasmasi.Varsayilan();
            var motor = new IhaleMotoru(anlasma);

            var aktifler = motor.AktifKonvansiyonlar();

            Yaz("Aktif Konvansiyonlar (Oncelik sırasına göre):");
            Yaz("────────────────────────────────────────");

            // Her konvansiyonun Oncelik değerini al
            // (Motor'daki _konvansiyonlar private, o yüzden yansıma kullanacağız)
            var tip = typeof(IhaleMotoru);
            var alan = tip.GetField("_konvansiyonlar",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (alan == null)
            {
                Yaz("❌ _konvansiyonlar alanı bulunamadı!");
                return;
            }

            var konvansiyonlar = (List<IKonvansiyon>)alan.GetValue(motor);

            foreach (var k in konvansiyonlar.Where(x => x.AktifMi).OrderBy(x => x.Oncelik))
            {
                Yaz($"  [{k.Oncelik,3}] {k.Ad}");
            }

            Yaz("════════════════════════════════════════");
        }

        public static void Test_OncelikCakismalari()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 17.2 Öncelik Çakışmaları");
            Yaz("════════════════════════════════════════");

            // ═══════════════════════════════════════════════════════════════
            // ÇAKIŞMA 1: 1NT - Gerber vs Stayman
            // ═══════════════════════════════════════════════════════════════
            Yaz("─── ÇAKIŞMA 1: 1NT - 4'lü majör + 16 HP ───");
            var el1 = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 },  // A
new Card { Suit = "Maça", Value = 13 },  // K
new Card { Suit = "Maça", Value = 12 },  // Q
new Card { Suit = "Maça", Value = 5 },   // ← 4'lü ♠
new Card { Suit = "Kupa", Value = 13 },  // K
new Card { Suit = "Kupa", Value = 12 },  // Q
new Card { Suit = "Kupa", Value = 11 },  // J
new Card { Suit = "Kupa", Value = 3 },   // ← 4'lü ♥
new Card { Suit = "Karo", Value = 14 },  // A
new Card { Suit = "Karo", Value = 5 },
new Card { Suit = "Karo", Value = 4 },
new Card { Suit = "Sinek", Value = 9 },
new Card { Suit = "Sinek", Value = 3 },
    };
            var gecmis1 = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };
            var durum1 = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el1,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis1,
            };
            var motor1 = new IhaleMotoru(durum1.Anlasma);
            Yaz($"Motor → {motor1.TeklifVer(durum1)}");

            // ═══════════════════════════════════════════════════════════════
            // ÇAKIŞMA 2: Rakip 1♠ - Michaels vs NegativeDouble
            // ═══════════════════════════════════════════════════════════════
            Yaz("─── ÇAKIŞMA 2: Rakip 1♠ + 5♥+5♦ ───");
            var el2 = new List<Card>
    {
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Kupa", Value = 14 },
        new Card { Suit = "Kupa", Value = 13 },
        new Card { Suit = "Kupa", Value = 11 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 12 },
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        new Card { Suit = "Sinek", Value = 7 },
    };
            var gecmis2 = new List<IhaleHamlesi>
{
    new IhaleHamlesi
{
    Oyuncu = Player.Bati,  // ← DOĞRU (rakip)
    Teklif = "1♠",
    Sira = 1,
    GecerliMi = true
},
new IhaleHamlesi
{
    Oyuncu = Player.Kuzey,  // ← DOĞRU (partner)
    Teklif = "Pas",
    Sira = 2,
    GecerliMi = true
},
};
            var durum2 = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el2,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis2,
            };
            var motor2 = new IhaleMotoru(durum2.Anlasma);
            Yaz($"Motor → {motor2.TeklifVer(durum2)}");

            // ═══════════════════════════════════════════════════════════════
            // ÇAKIŞMA 3: 1NT - StrongNT vs BesliMajor
            // ═══════════════════════════════════════════════════════════════
            Yaz("─── ÇAKIŞMA 3: 15 HP dengeli + 5'li ♠ ───");
            var el3 = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A
        new Card { Suit = "Maça", Value = 13 }, // K
        new Card { Suit = "Maça", Value = 12 }, // Q
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 14 }, // A
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 12 }, // Q
        new Card { Suit = "Karo", Value = 11 }, // J
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 9 },
    };
            var durum3 = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el3,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = new List<IhaleHamlesi>(),
            };
            var motor3 = new IhaleMotoru(durum3.Anlasma);
            Yaz($"Motor → {motor3.TeklifVer(durum3)}");

            Yaz("════════════════════════════════════════");
        }
        /// <summary>
        /// Tüm testleri sırayla çalıştırır.
        /// </summary>
        public static void TumTestleriCalistir()
        {
            Yaz("▶ Test 1.1: Gerber Soru Sorma");
            Test_Gerber_SoruSorma();

            Yaz("▶ Test 1.2: Gerber Cevap");
            Test_Gerber_Cevap();

            Yaz("▶ Test 1.3: Gerber Negatif");
            Test_Gerber_Negatif();

            Yaz("▶ Test 2.1: Jacoby 2NT Pozitif");
            Test_Jacoby2NT_Pozitif();

            Yaz("▶ Test 2.2: Jacoby 2NT Negatif (3'lü destek)");
            Test_Jacoby2NT_Negatif_3luDestek();

            Yaz("▶ Test 2.3: Jacoby 2NT Negatif (Rakip)");
            Test_Jacoby2NT_Negatif_RakipMudahalesi();

            Yaz("▶ Test 3.1: Splinter (Koz Maça)");
            Test_Splinter_Maca_Sinek();

            Yaz("▶ Test 3.2: Splinter (Koz Kupa)");
            Test_Splinter_Kupa_Maca();

            Yaz("▶ Test 4.1: Michaels (Rakip Maça)");
            Test_Michaels_RakipMaca();

            Yaz("▶ Test 4.2: Michaels (Rakip Karo)");
            Test_Michaels_RakipKaro();

            Yaz("▶ Test 4.3: Michaels Negatif");
            Test_Michaels_Negatif();

            Yaz("▶ Test 5.1: TemelMantik Yeni Renk");
            Test_TemelMantik_YeniRenk();

            Yaz("▶ Test 5.2: TemelMantik NT Invite");
            Test_TemelMantik_NTInvite();

            Yaz("▶ Test 5.3: TemelMantik Rakip Müdahale");
            Test_TemelMantik_RakipMudahale();

            Yaz("▶ Test 6.1: Unusual 2NT");
            Test_Unusual2NT_1Sinek();
            Test_Unusual2NT_1Karo();
            Test_Unusual2NT_1Kupa();
            Test_Unusual2NT_1Maca();

            Yaz("▶ Test 6.2: Support Double");
            Test_SupportDouble();

            Yaz("▶ Test 6.3: Responsive Double");
            Test_ResponsiveDouble();

            Yaz("▶ Test 7.1: Minor Transfer Sinek");
            Test_MinorTransfer_Sinek();

            Yaz("▶ Test 7.2: Minor Transfer Karo");
            Test_MinorTransfer_Karo();

            Yaz("▶ Test 7.3: Drury");
            Test_Drury();

            Yaz("▶ Test 8.1: Smolen 5♠+4♥");
            Test_Smolen_5Maca_4Kupa();

            Yaz("▶ Test 8.2: Smolen 5♥+4♠");
            Test_Smolen_5Kupa_4Maca();

            Yaz("▶ Test 9.1-9.12: Puppet Stayman (12 test)");
            Test_PuppetStayman();
            Test_Puppet_5liKupa();
            Test_Puppet_4luMajor();
            Test_Puppet_2Tur_4luMaca();
            Test_Puppet_Aşama3_TutuşVar();
            Test_Puppet_Aşama3_TutuşYok();
            Test_Puppet_Aşama4_Kontrol();
            Test_Puppet_5liKupa_Kontrol();
            Test_Puppet_5liKupa_SignOff();
            Test_Puppet_KeyCard_2();
            Test_Puppet_KeyCard_3();
            Test_Puppet_KeyCard_4_1Q();

            Yaz("▶ Test 10.1: Grand Slam Force Soru");
            Test_GSF_Soru();

            Yaz("▶ Test 10.2: Grand Slam Force Cevap");
            Test_GSF_Cevap();

            Yaz("▶ Test 11.1-11.3: Stayman Cevap Sistemi");
            Test_Stayman_2Tur_NTInvite();
            Test_Stayman_2Tur_NTGame();
            Test_Stayman_2Tur_FitBulundu();

            Yaz("▶ Test 12.1: Lebensohl Basit");
            Test_Lebensohl_Basit();
            Test_Lebensohl_3NT_DurdurucuVar();
            Test_Lebensohl_3Hearts_CueBid();
            Test_Lebensohl_4Hearts_Transfer();

            Yaz("▶ Test 13.1-13.2: Lebensohl Rakip 2♣/2♦");
            Test_Lebensohl_Rakip2Club();
            Test_Lebensohl_Rakip2Diamond();

            Yaz("▶ Test 14.1-14.3: Lebensohl Zayıf 2");
            Test_Lebensohl_WeakTwo_Zayif();
            Test_Lebensohl_WeakTwo_Davet();
            Test_Lebensohl_WeakTwo_CueBid();

            Yaz("▶ Test 15.1-15.3: Lebensohl Reverse");
            Test_Lebensohl_Reverse_Zayif();
            Test_Lebensohl_Reverse_Guclu();
            Test_Lebensohl_Reverse_SignOff();

            Yaz("▶ Test 16.1: BasitCevap 6'lı destek");
            Test_BasitCevap_6liDestek();

            Yaz("▶ Test 17.1: Konvansiyon Öncelik Sırası");
            Test_OncelikSirasi();

            Yaz("▶ Test 17.1: Konvansiyon Öncelik Çakışmaları");
            Test_OncelikCakismalari();

            Yaz("");
            Yaz("✅ TÜM TESTLER TAMAMLANDI!");
        }

    }
}