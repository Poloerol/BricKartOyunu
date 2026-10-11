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
                Anlasma = CappellettiAnlasmasi(),
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
        private static OrtaklikAnlasmasi CappellettiAnlasmasi()
        {
            var anlasma = OrtaklikAnlasmasi.Varsayilan();
            anlasma.Cappelletti = true;
            anlasma.Dont = false;   // ikisi aynı anda aktif olamaz
            return anlasma;
        }

        private static OrtaklikAnlasmasi MultiLandyAnlasmasi()
        {
            var anlasma = OrtaklikAnlasmasi.Varsayilan();
            anlasma.Cappelletti = false;
            anlasma.MultiLandy = true;
            anlasma.Dont = false;   // ikisi aynı anda aktif olamaz
            return anlasma;
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

        public static void Test_IkiliSinekGuclu_Dogru()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 18.1 - 2♣ açılışı — 19 HP + 6-4 dağılım");
            Yaz("════════════════════════════════════════");

            var doguEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 },  // A
        new Card { Suit = "Maça", Value = 13 },  // K
        new Card { Suit = "Kupa", Value = 14 },  // A
        new Card { Suit = "Kupa", Value = 12 },  // Q
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Karo", Value = 14 },  // A
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Dogu,
                AktifOyuncuEli = doguEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(doguEli)}");
            Yaz("Beklenen: 1♥ (2♣ DEĞİL)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc != "2♣" ? "✅ DOĞRU" : "❌ YANLIŞ");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 19.x — İki Yönlü Checkback (Two-Way Checkback)
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_TwoWay_ZayifMajor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 19.1 — TwoWayCheckback: Zayıf major tekrarı");
            Yaz("════════════════════════════════════════");
            Yaz("Senaryo: 1♣ - 1♥ - 1NT - ?  (5'li ♥, 6 HP)");
            Yaz("Beklenen: 2♥ (zayıf signoff)");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 0 (onör yok)

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 5, GecerliMi = true },
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
            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");

            var twcb = new TwoWayCheckback { AktifMi = true };
            Yaz($"TwoWayCheckback.UygunMu → {twcb.UygunMu(durum)}");
            if (twcb.UygunMu(durum))
                Yaz($"TwoWayCheckback.TeklifVer → {twcb.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TwoWay_DavetNT()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 19.2 — TwoWayCheckback: Natürel 2NT davet");
            Yaz("════════════════════════════════════════");
            Yaz("Senaryo: 1♣ - 1♥ - 1NT - ?  (10 HP dengeli)");
            Yaz("Beklenen: 2NT (davet)");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 4 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 2+3+1+4 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 5, GecerliMi = true },
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
            Yaz($"Dengeli: {ElDegerlendirici.DengeliEl(guneyEli)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TwoWay_GF()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 19.3 — TwoWayCheckback: 2♦ GF");
            Yaz("════════════════════════════════════════");
            Yaz("Senaryo: 1♦ - 1♠ - 1NT - ?  (13+ HP)");
            Yaz("Beklenen: 2♦ (GF checkback)");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+1+3+2+3 = 13

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 5, GecerliMi = true },
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

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TwoWay_ZayifMinor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 19.4 — TwoWayCheckback: Zayıf minor (2♣ puppet)");
            Yaz("════════════════════════════════════════");
            Yaz("Senaryo: 1♦ - 1♥ - 1NT - ?  (5'li ♦, 5 HP)");
            Yaz("Beklenen: 2♣ (puppet → 2♦'ye transfer)");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 2'li ♠
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 4 },
        // 5'li ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 3 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 8 },
    };
            // HCP: 2+3 = 5

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 5, GecerliMi = true },
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
            Yaz($"♦ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Karo")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TwoWay_PuppetCevap()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 19.5 — TwoWayCheckback: Açıcının 2♣'e cevabı");
            Yaz("════════════════════════════════════════");
            Yaz("Senaryo: 1♣ - 1♥ - 1NT - 2♣ - Pas - ?");
            Yaz("Beklenen: 2♦ (zorunlu puppet cevabı)");
            Yaz("────────────────────────────────────────");

            // Kuzey'in eli (açıcı): ne olursa olsun 2♦ demeli
            var kuzeyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 4 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 5 },
        // 4'lü ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3+2+4+1 = 10 (1NT rebid için aslında 15-17 olmalı ama test için önemsiz)

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TwoWay_AciciIkiKaroyaCevap()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 19.6 — TwoWayCheckback: Açıcının 2♦'ye cevabı");
            Yaz("════════════════════════════════════════");
            Yaz("Senaryo: 1♦ - 1♥ - 1NT - 2♦ - Pas - ?");
            Yaz("Açıcının elinde 3'lü ♥ var → 2♥ demeli");
            Yaz("Beklenen: 2♥ (3'lü destek)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 4 },
        // 3'lü ♥ (destek)
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 2 },
    };
            // HCP: 3+4+2+1 = 10 (test için önemsiz)

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♦",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 20.x — XYZ Konvansiyonu
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_XYZ_ZayifOzel()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 20.1 — XYZ: 1♦-1♥-1♠ sonrası 1NT signoff");
            Yaz("Beklenen: 1NT (oynamak için)");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Karo", Value = 2 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 6 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 5, GecerliMi = true },
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
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "1NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 1NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_XYZ_Zayif5liMajor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 20.2 — XYZ: 1♦-1♥-1♠ sonrası 2♥ (5+ ♥ signoff)");
            Yaz("Beklenen: 2♥");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 6 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 5, GecerliMi = true },
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
            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_XYZ_DavetPuppet()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 20.3 — XYZ: Davet eli (2♣ puppet)");
            Yaz("Beklenen: 2♣");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 2 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 3+1+4+1 = 9

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 5, GecerliMi = true },
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
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_XYZ_GF()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 20.4 — XYZ: 2♦ GF");
            Yaz("Beklenen: 2♦");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 2 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 7 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 4+1+3+2+3 = 13

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 5, GecerliMi = true },
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
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_XYZ_PuppetCevap()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 20.5 — XYZ: Açıcının 2♣'e cevabı");
            Yaz("Beklenen: 2♦ (zorunlu puppet cevabı)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 5'li ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 3 },
        // 2'li ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 6 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 4+3+2+1 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_XYZ_AciciIkiKaroyaCevap()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 20.6 — XYZ: Açıcının 2♦'ye cevabı");
            Yaz("Beklenen: 2♥ (3'lü ♥ desteği var)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 5'li ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥ (destek)
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 6 },
        // 1'li ♣
        new Card { Suit = "Sinek", Value = 9 },
    };
            // HCP: 4+3+2+1+4 = 14, 3'lü ♥

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♦",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Kupa")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 21.x — Trial Bid (Standart)
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_TrialBid_AciciBaslatma()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 21.1 — Trial Bid: Açıcı başlatıyor (1♥-2♥-2♠)");
            Yaz("Beklenen: 2♠ (♠ yardım sorusu)");
            Yaz("────────────────────────────────────────");

            // Kuzey 1♥ açtı, Güney 2♥ dedi, sıra Kuzey'de
            // Kuzey'in eli: 15 HP, 5'li ♥, ♠'de A/K yok
            var kuzeyEli = new List<Card>
    {
        // 5'li ♥ (iyi)
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        // 3'lü ♠ (A/K yok!)
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+2+4+3 = 13 (14 diyelim, biraz artıralım)
            kuzeyEli = new List<Card>
    {
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+2+4+3+1 = 14

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TrialBid_CevapciKabul()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 21.2 — Trial Bid: Cevapçı kabul ediyor (♠ A/K var)");
            Yaz("Beklenen: 4♥ (manş kabul)");
            Yaz("────────────────────────────────────────");

            // 1♥ - 2♥ - 2♠ - ? (Güney cevap verecek)
            // Güney'in eli: 3'lü ♥, ♠'de K var
            var guneyEli = new List<Card>
    {
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4'lü ♠ (K var!)
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♥", Sira = 3, GecerliMi = true },
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

            Yaz($"♠'de K var mı: {guneyEli.Any(c => c.Suit == "Maça" && c.Value == 13)}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TrialBid_CevapciRet()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 21.3 — Trial Bid: Cevapçı ret ediyor (♠'de yardım yok)");
            Yaz("Beklenen: 3♥ (signoff)");
            Yaz("────────────────────────────────────────");

            // 1♥ - 2♥ - 2♠ - ? (Güney cevap verecek)
            // Güney'in eli: 3'lü ♥, ♠'de 3 küçük kart
            var guneyEli = new List<Card>
    {
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♠ (küçük kartlar)
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♥", Sira = 3, GecerliMi = true },
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

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TrialBid_MacaAcici()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 21.4 — Trial Bid: Açıcı ♠ ile başlatıyor (1♠-2♠-3♦)");
            Yaz("Beklenen: 3♦ (♦ yardım sorusu)");
            Yaz("────────────────────────────────────────");

            // Kuzey 1♠ açtı, Güney 2♠ dedi, sıra Kuzey'de
            // Kuzey'in eli: 14 HP, 5'li ♠, ♥'de A/K var, ♦'da yok
            var kuzeyEli = new List<Card>
    {
        // 5'li ♠ (iyi)
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        // 3'lü ♥ (A var, yardım var)
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 4 },
        // 3'lü ♦ (yardım YOK)
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 3 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+2+4+3 = 13 → 14 için biraz artır
            kuzeyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 3 },
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+2+4+1+3 = 14

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"♥'de A/K var mı: {kuzeyEli.Any(c => c.Suit == "Kupa" && (c.Value == 14 || c.Value == 13))}");
            Yaz($"♦'da A/K var mı: {kuzeyEli.Any(c => c.Suit == "Karo" && (c.Value == 14 || c.Value == 13))}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TrialBid_CevapciMacaKabul()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 21.5 — Trial Bid: ♠ için cevapçı ♦ yardım gösteriyor");
            Yaz("Beklenen: 4♠ (manş kabul)");
            Yaz("────────────────────────────────────────");

            // 1♠ - 2♠ - 3♦ - ? (Güney cevap verecek)
            // Güney'in eli: 3'lü ♠, ♦'da A var
            var guneyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦ (A var!)
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TrialBid_CevapciMacaRet()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 21.6 — Trial Bid: ♠ için cevapçı ♦ yardım göstermiyor");
            Yaz("Beklenen: 3♠ (signoff)");
            Yaz("────────────────────────────────────────");

            // 1♠ - 2♠ - 3♦ - ? (Güney cevap verecek)
            // Güney'in eli: 3'lü ♠, ♦'da 3 küçük
            var guneyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦ (yardım YOK)
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 4 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♠)");
            Yaz("════════════════════════════════════════");
        }
        // ═══════════════════════════════════════════════════════════════════
        // TEST 22.x — 3. Renk Forsingi (Third Suit Forcing)
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_TSF_BasitBaslatma()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 22.1 — 3. Renk Forsingi: Cevapçı başlatıyor");
            Yaz("Senaryo: 1♣ - 1♥ - 2♣ - ?");
            Yaz("Beklenen: 2♦ (3. Renk Forsingi)");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 3+1+4 = 8 → 11 için biraz artır
            guneyEli = new List<Card>
    {
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 3+1+2+4+1 = 11 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♣", Sira = 5, GecerliMi = true },
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
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TSF_AciciDestek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 22.2 — 3. Renk Forsingi: Açıcı 3'lü ♥ desteği gösteriyor");
            Yaz("Senaryo: 1♣ - 1♥ - 2♣ - 2♦ - ?");
            Yaz("Beklenen: 2♥ (3'lü ♥ desteği)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 3'lü ♥ (destek)
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 4 },
        // 5'li ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 2 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 5 },
    };
            // HCP: 4+3+1+2 = 10 (açıcı minör tekrar etti, minimum el)

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♣", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♦", Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Kupa")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TSF_AciciDigerMajor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 22.3 — 3. Renk Forsingi: Açıcı 4'lü ♠ gösteriyor");
            Yaz("Senaryo: 1♣ - 1♥ - 2♣ - 2♦ - ?");
            Yaz("Beklenen: 2♠ (4'lü ♠, ♥ desteği yok)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 3 },
        // 2'li ♥ (destek YOK)
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 5 },
        // 5'li ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 6 },
    };
            // HCP: 4+1+3+2 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♣", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♦", Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Kupa")}");
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TSF_AciciNT()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 22.4 — 3. Renk Forsingi: Açıcı 2NT (dengeli)");
            Yaz("Senaryo: 1♦ - 1♠ - 2♦ - 3♣ - ?");
            Yaz("Beklenen: 2NT (dengeli, 11-13)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 3'lü ♠ (onörsüz)
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥ (onörsüz)
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 5'li ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 2 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+4 = 11

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♦", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣", Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 23.x — 4. Renk Forsingi (Fourth Suit Forcing)
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_4SF_BasitBaslatma()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 23.1 — 4SF: Cevapçı başlatıyor");
            Yaz("Senaryo: 1♦ - 1♥ - 2♣ - ?");
            Yaz("Beklenen: 2♠ (4. Renk Forsingi)");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 3+1+4 = 8 → 11 için artır
            guneyEli = new List<Card>
    {
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+1+2+3 = 10 → hâlâ düşük, bir onör daha
            guneyEli = new List<Card>
    {
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+1+3+2 = 10 → 11 için ♦ Q ekleyelim
            guneyEli = new List<Card>
    {
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+1+3+2+1 = 11 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♣", Sira = 5, GecerliMi = true },
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
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_4SF_AciciDestek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 23.2 — 4SF: Açıcı 3'lü ♥ desteği gösteriyor");
            Yaz("Senaryo: 1♦ - 1♥ - 2♣ - 2♠ - ?");
            Yaz("Beklenen: 2♥ veya 3♥ (3'lü ♥ desteği)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 4'lü ♠ (4SF rengi)
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♥ (destek)
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 4 },
        // 5'li ♦
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        // 1'li ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
    };
            // HCP: 4+3+1+2 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♣", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♠", Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Kupa")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            bool dogru = (sonuc == "2♥" || sonuc == "3♥");
            Yaz(dogru ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥ veya 3♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_4SF_AciciNT()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 23.3 — 4SF: Açıcı 2NT/3NT (stopper var)");
            Yaz("Senaryo: 1♦ - 1♥ - 2♣ - 2♠ - ?");
            Yaz("Beklenen: 2NT veya 3NT (♠'de stopper var)");
            Yaz("────────────────────────────────────────");
            var kuzeyEli = new List<Card>
    {
        // 3'lü ♠ (stopper: A)
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 3 },
        // 2'li ♥ (destek YOK)
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        // 5'li ♦
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+1+3+2 = 13 → dengeli, 2NT diyebilir

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♣", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♠", Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Kupa")}");
            Yaz($"♠ Stopper: {ElDegerlendirici.StopperVar(kuzeyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            bool dogru = (sonuc == "2NT" || sonuc == "3NT");
            Yaz(dogru ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT veya 3NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_4SF_XYZCakismasiYok()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 23.4 — 4SF: XYZ ile çakışma yok (1 seviyesi)");
            Yaz("Senaryo: 1♦ - 1♥ - 1♠ - ?");
            Yaz("Beklenen: 4SF DEĞİL, XYZ veya başka konvansiyon devrede");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        new Card { Suit = "Kupa", Value = 14 },
        new Card { Suit = "Kupa", Value = 11 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 2 },
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        new Card { Suit = "Sinek", Value = 12 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+1+2 = 7 → zayıf el

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // 4SF tetiklenmemeli
            var fsf = new FourthSuitForcing { AktifMi = true };
            bool fsfUygun = fsf.UygunMu(durum);
            Yaz($"4SF.UygunMu → {fsfUygun}");
            Yaz(!fsfUygun ? "✅ DOĞRU (4SF tetiklenmedi)" : "❌ YANLIŞ (4SF tetiklendi!)");

            // Motor başka bir konvansiyon döndürmeli
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 24.x — Bergen Raises
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_Bergen_3Sinek_Zayif()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 24.1 — Bergen: 3♣ (zayıf, 7-10 HP, 4'lü destek)");
            Yaz("Senaryo: 1♥ - ?");
            Yaz("Beklenen: 3♣");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 4'lü ♥ (destek)
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 2+1+3 = 6 → 7-10 arasına çıkar
            guneyEli = new List<Card>
    {
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 2+3+2 = 7 ✓

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
            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Bergen_3Karo_Davet()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 24.2 — Bergen: 3♦ (davet, 11-12 HP, 4'lü destek)");
            Yaz("Senaryo: 1♠ - ?");
            Yaz("Beklenen: 3♦");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 4'lü ♠ (destek)
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 2+4+3 = 9 → 11-12 için artır
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 2 },
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 3+4+4 = 11 ✓

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
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Bergen_3Major_Preemptive()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 24.3 — Bergen: 3♥ (preemptive, 0-6 HP, 4'lü destek)");
            Yaz("Senaryo: 1♥ - ?");
            Yaz("Beklenen: 3♥");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 4'lü ♥ (destek)
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 0

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
            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Bergen_4Major_Preemptive()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 24.4 — Bergen: 4♠ (preemptive, 0-6 HP, 5'li destek)");
            Yaz("Senaryo: 1♠ - ?");
            Yaz("Beklenen: 4♠");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♠ (destek)
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Maça", Value = 2 },
        // 2'li ♥
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 4 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 0

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
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Bergen_Acici3Karo()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 24.5 — Bergen: Açıcının 3♦ sorusu (3♣ sonrası)");
            Yaz("Senaryo: 1♥ - 3♣ - ?");
            Yaz("Beklenen: 3♦ (yapay soru)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+1+2 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Bergen_3luDestek_Yok()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 24.6 — Bergen: 3'lü destek ile Bergen DEĞİL");
            Yaz("Senaryo: 1♥ - ? (3'lü ♥, 10 HP)");
            Yaz("Beklenen: Bergen tetiklenmez (BasitCevap veya TemelMantik)");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 3'lü ♥ (destek)
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 2+3 = 5

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

            // Bergen tetiklenmemeli (3'lü destek)
            var bergen = new BergenRaises { AktifMi = true };
            bool bergenUygun = bergen.UygunMu(durum);
            Yaz($"Bergen.UygunMu → {bergenUygun}");

            // Motor başka bir konvansiyon döndürmeli
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 25.x — Cappelletti
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_Cappelletti_2Karo_Majors()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 25.1 — Cappelletti: 2♦ (majörler)");
            Yaz("Senaryo: (1NT) - ?");
            Yaz("Beklenen: 2♦");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♠
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
    };
            // HCP: 3+1+2 = 6 → 9-14 için artır
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
    };
            // HCP: 4+1+3 = 8 → hâlâ düşük, ♦ Q ekleyelim
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
    };
            // HCP: 4+1+3+2 = 10 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = CappellettiAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}, ♥: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Cappelletti_2Kupa_Minor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 25.2 — Cappelletti: 2♥ (♥ + minör)");
            Yaz("Senaryo: (1NT) - ?");
            Yaz("Beklenen: 2♥");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 5'li ♦
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
    };
            // HCP: 4+1+3 = 8 → 9-14 için artır
            guneyEli = new List<Card>
    {
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
    };
            // HCP: 4+1+3+2 = 10 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = CappellettiAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"♥: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")}, ♦: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Karo")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Cappelletti_2NT_Minors()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 25.3 — Cappelletti: 2NT (minörler)");
            Yaz("Senaryo: (1NT) - ?");
            Yaz("Beklenen: 2NT");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        // 5'li ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
    };
            // HCP: 3+1+2 = 6 → 9-14 için artır
            guneyEli = new List<Card>
    {
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
    };
            // HCP: 4+1+3+2 = 10 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = CappellettiAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"♣: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Sinek")}, ♦: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Karo")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Cappelletti_Double()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 25.4 — Cappelletti: Dbl (ceza, 15+ HP)");
            Yaz("Senaryo: (1NT) - ?");
            Yaz("Beklenen: Dbl");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+2+1 = 10 → 15+ için artır
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+4+2+1 = 14 → 15 için bir Q daha
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+4+3+2 = 16 ✓ (dengeli 4-4-3-2)

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = CappellettiAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(guneyEli)}");
            Yaz($"Dengeli: {ElDegerlendirici.DengeliEl(guneyEli)}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Dbl" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Dbl)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Cappelletti_PartnerCevap_2Karo()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 25.5 — Cappelletti: Partner 2♦'ye cevap (majör seçimi)");
            Yaz("Senaryo: (1NT) - 2♦ - (Pas) - ?");
            Yaz("Beklenen: 2♥ veya 2♠");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 11 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 12 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 13 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♦",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = CappellettiAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            bool dogru = (sonuc == "2♥" || sonuc == "2♠");
            Yaz(dogru ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥ veya 2♠)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 26.x — DONT
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_Dont_2Sinek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 26.1 — DONT: 2♣ (♣ + yüksek renk)");
            Yaz("Senaryo: (1NT) - ?");
            Yaz("Beklenen: 2♣");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        // 5'li ♠
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
    };
            // HCP: 3+1+2 = 6

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
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
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Dont_2Karo()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 26.2 — DONT: 2♦ (♦ + majör)");
            Yaz("Senaryo: (1NT) - ?");
            Yaz("Beklenen: 2♦");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 4+1+2 = 7

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Dont_2Kupa_Majors()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 26.3 — DONT: 2♥ (♥ + ♠)");
            Yaz("Senaryo: (1NT) - ?");
            Yaz("Beklenen: 2♥");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 5'li ♠
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3+1+2 = 6

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Dont_Double()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 26.4 — DONT: Dbl (tek renkli)");
            Yaz("Senaryo: (1NT) - ?");
            Yaz("Beklenen: Dbl");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 6'lı ♥ (tek renkli)
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 5 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 2 },
    };
            // HCP: 4+3+1 = 8

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Dbl" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Dbl)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Dont_PartnerCevap_Double()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 26.5 — DONT: Partner Dbl'e cevap (2♣ zorunlu)");
            Yaz("Senaryo: (1NT) - Dbl - (Pas) - ?");
            Yaz("Beklenen: 2♣");
            Yaz("────────────────────────────────────────");

            // Güney'in eli: dengeli 3-4-3-3, DONT müdahalesi için uygun DEĞİL.
            // (Partner Dbl yaptıysa cevap, el dağılımından bağımsız olarak 2♣'dir.)
            var guneyEli = new List<Card>
            {
                // 3'lü ♠
                new Card { Suit = "Maça",  Value = 10 },
                new Card { Suit = "Maça",  Value = 7 },
                new Card { Suit = "Maça",  Value = 3 },
                // 4'lü ♥
                new Card { Suit = "Kupa",  Value = 11 }, // J = 1
                new Card { Suit = "Kupa",  Value = 9 },
                new Card { Suit = "Kupa",  Value = 6 },
                new Card { Suit = "Kupa",  Value = 2 },
                // 3'lü ♦
                new Card { Suit = "Karo",  Value = 12 }, // Q = 2
                new Card { Suit = "Karo",  Value = 8 },
                new Card { Suit = "Karo",  Value = 4 },
                // 3'lü ♣
                new Card { Suit = "Sinek", Value = 13 }, // K = 3
                new Card { Suit = "Sinek", Value = 7 },
                new Card { Suit = "Sinek", Value = 5 },
            };
            // HCP: 1+2+3 = 6

            // Doğru sıra: Batı → Kuzey → Doğu → Güney (sıra şimdi Güney'de)
            var gecmis = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1NT", Sira = 1, GecerliMi = true }, // rakip açtı
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true }, // partner DONT Dbl
                new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true }, // rakip pas
            };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"AktifOyuncu: {durum.AktifOyuncu}");
            Yaz($"Partner: {durum.Partner}");
            Yaz($"Rakipler: [{string.Join(", ", durum.Rakipler)}]");
            Yaz($"Dağılım: ♠{ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")} " +
                $"♥{ElDegerlendirici.RenkUzunlugu(guneyEli, "Kupa")} " +
                $"♦{ElDegerlendirici.RenkUzunlugu(guneyEli, "Karo")} " +
                $"♣{ElDegerlendirici.RenkUzunlugu(guneyEli, "Sinek")}");
            Yaz("────────────────────────────────────────");

            var dont = new Dont();
            Yaz($"Dont.UygunMu → {dont.UygunMu(durum)}");
            Yaz($"Dont.TeklifVer → {dont.TeklifVer(durum)}");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♣" ? "✅ DOĞRU" : "❌ YANLIŞ (beklenen 2♣)");
            Yaz("════════════════════════════════════════");
        }


        public static void Test_Dont_PartnerCevap_2Sinek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 26.6 — DONT: Partner 2♣'e cevap (3+ ♣ → Pas)");
            Yaz("Senaryo: (1NT) - 2♣ - (Pas) - ?");
            Yaz("Beklenen: Pas (3+ ♣)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 11 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 12 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 13 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1NT", Sira = 1 },
new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2 },
new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3 },
new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣", Sira = 4 },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Pas" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Pas)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 27.x — DOPI / ROPI
        // TestRunner sınıfının içine yapıştırın (using System.Linq gerekir).
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Tek renkten kart listesi üretir: K("Maça", 13, 12, 7)</summary>
        private static IEnumerable<Card> K(string renk, params int[] degerler)
        {
            return degerler.Select(d => new Card { Suit = renk, Value = d });
        }

        /// <summary>
        /// Kurulum: Güney 1M açar, Kuzey 3M (limit raise), Güney 4NT (Blackwood),
        /// Batı müdahale eder, sıra Kuzey'de (cevap veren).
        /// </summary>
        private static IhaleDurumu DopiDurumuKur(List<Card> kuzeyEli, string acilis, string mudahale)
        {
            string destek = "3" + acilis.Substring(1);

            var gecmis = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = acilis,   Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas",    Sira = 2, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = destek,   Sira = 3, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas",    Sira = 4, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT",    Sira = 5, GecerliMi = true }, // Blackwood
                new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = mudahale, Sira = 6, GecerliMi = true }, // müdahale
            };

            return new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };
        }

        private static void DopiTestCalistir(
            string baslik, string senaryo, string beklenen,
            List<Card> el, string acilis, string mudahale)
        {
            Yaz("════════════════════════════════════════");
            Yaz(baslik);
            Yaz($"Senaryo: {senaryo}");
            Yaz($"Beklenen: {beklenen}");
            Yaz("────────────────────────────────────────");

            var durum = DopiDurumuKur(el, acilis, mudahale);

            int asSayisi = el.Count(c => c.Value == 14);
            Yaz($"As sayısı: {asSayisi}");

            var konv = new DopiRopi();
            Yaz($"DopiRopi.UygunMu → {konv.UygunMu(durum)}");
            Yaz($"DopiRopi.TeklifVer → {konv.TeklifVer(durum)}");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            motor.HatalardaFirlat = true;
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == beklenen ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen {beklenen})");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Dopi_SifirAnahtarKart()
        {
            // ♠ koz, anahtar kart: 0 (As yok, ♠K yok)
            var el = K("Maça", 9, 7, 5, 3).Concat(K("Kupa", 12, 8, 6))
                     .Concat(K("Karo", 11, 9, 4)).Concat(K("Sinek", 10, 6, 2)).ToList();

            DopiTestCalistir("TEST 27.1 — DOPI: 0 anahtar kart → Dbl",
                "1♠ - 3♠ - 4NT - (5♦) - ?", "Dbl", el, "1♠", "5♦");
        }

        public static void Test_Dopi_BirAnahtarKart()
        {
            // ♠ koz, anahtar kart: 1 (♦A)
            var el = K("Maça", 9, 7, 5, 3).Concat(K("Kupa", 12, 8, 6))
                     .Concat(K("Karo", 14, 9, 4)).Concat(K("Sinek", 10, 6, 2)).ToList();

            DopiTestCalistir("TEST 27.2 — DOPI: 1 anahtar kart → Pas",
                "1♠ - 3♠ - 4NT - (5♦) - ?", "Pas", el, "1♠", "5♦");
        }

        public static void Test_Dopi_IkiKart_KizYok()
        {
            // ♠ koz, anahtar kart: 2 (♠K, ♥A), ♠ kızı YOK → müdahaleden sonraki ilk teklif: 5♥
            var el = K("Maça", 13, 9, 7, 3).Concat(K("Kupa", 14, 8, 6))
                     .Concat(K("Karo", 9, 5, 4)).Concat(K("Sinek", 10, 6, 2)).ToList();

            DopiTestCalistir("TEST 27.3 — DOPI: 2 anahtar kart, kız yok → 5♥",
                "1♠ - 3♠ - 4NT - (5♦) - ?", "5♥", el, "1♠", "5♦");
        }

        public static void Test_Dopi_IkiKart_KizVar()
        {
            // ♠ koz, anahtar kart: 2 (♠K, ♥A), ♠ kızı VAR → ikinci teklif: 5♠
            var el = K("Maça", 13, 12, 7, 3).Concat(K("Kupa", 14, 8, 6))
                     .Concat(K("Karo", 9, 5, 4)).Concat(K("Sinek", 10, 6, 2)).ToList();

            DopiTestCalistir("TEST 27.4 — DOPI: 2 anahtar kart + kız → 5♠",
                "1♠ - 3♠ - 4NT - (5♦) - ?", "5♠", el, "1♠", "5♦");
        }

        public static void Test_Ropi_SifirAnahtarKart()
        {
            // ♠ koz, rakip 4NT'ye kontr verdi, anahtar kart: 0 → Rdbl
            var el = K("Maça", 9, 7, 5, 3).Concat(K("Kupa", 12, 8, 6))
                     .Concat(K("Karo", 11, 9, 4)).Concat(K("Sinek", 10, 6, 2)).ToList();

            DopiTestCalistir("TEST 27.5 — ROPI: 0 anahtar kart → Rdbl",
                "1♠ - 3♠ - 4NT - (Dbl) - ?", "Rdbl", el, "1♠", "Dbl");
        }

        public static void Test_Ropi_BirAnahtarKart()
        {
            // ♠ koz, rakip kontr, anahtar kart: 1 → Pas
            var el = K("Maça", 9, 7, 5, 3).Concat(K("Kupa", 12, 8, 6))
                     .Concat(K("Karo", 14, 9, 4)).Concat(K("Sinek", 10, 6, 2)).ToList();

            DopiTestCalistir("TEST 27.6 — ROPI: 1 anahtar kart → Pas",
                "1♠ - 3♠ - 4NT - (Dbl) - ?", "Pas", el, "1♠", "Dbl");
        }

        public static void Test_Dopi_Negatif_YuksekMudahale()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 27.7 — DOPI Negatif: koz ♥, rakip 5♠ (koz üstü)");
            Yaz("Senaryo: 1♥ - 3♥ - 4NT - (5♠) - ?");
            Yaz("Beklenen: DopiRopi.UygunMu → False (DEPO gerekir)");
            Yaz("────────────────────────────────────────");

            var el = K("Maça", 9, 7, 5).Concat(K("Kupa", 12, 8, 6, 3))
                     .Concat(K("Karo", 14, 9, 4)).Concat(K("Sinek", 10, 6, 2)).ToList();

            var durum = DopiDurumuKur(el, "1♥", "5♠");
            bool uygun = new DopiRopi().UygunMu(durum);

            Yaz($"DopiRopi.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU" : "❌ YANLIŞ (beklenen False)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Dopi_Negatif_Muddahalesiz()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 27.8 — DOPI Negatif: müdahale yok (Pas)");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - (Pas) - ?");
            Yaz("Beklenen: DopiRopi.UygunMu → False (normal Blackwood)");
            Yaz("────────────────────────────────────────");

            var el = K("Maça", 9, 7, 5, 3).Concat(K("Kupa", 12, 8, 6))
                     .Concat(K("Karo", 14, 9, 4)).Concat(K("Sinek", 10, 6, 2)).ToList();

            var durum = DopiDurumuKur(el, "1♠", "Pas");
            bool uygun = new DopiRopi().UygunMu(durum);

            Yaz($"DopiRopi.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU" : "❌ YANLIŞ (beklenen False)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 28.x — Forcing NT
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_FNT_Cevapci1NT()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 28.1 — FNT: Cevapçı 1NT veriyor (5-12 OP)");
            Yaz("Senaryo: 1♠ - ?");
            Yaz("Beklenen: 1NT");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 2'li ♠ (4+ destek yok)
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 11 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 3 },
        // 4'lü ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 2+1+3 = 6 ✓

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
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "1NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 1NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_FNT_Acici2M()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 28.2 — FNT: Açıcı 6'lı majör, min → 2♠");
            Yaz("Senaryo: 1♠ - 1NT - ?");
            Yaz("Beklenen: 2♠ (6'lı, 12-15 OP)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 6'lı ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 5 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+1+2 = 10 → min için 12'ye çıkar
            kuzeyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+1+2+1+1 = 12 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1NT", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_FNT_Acici3M()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 28.3 — FNT: Açıcı 6'lı majör, 15-18 → 3♠");
            Yaz("Senaryo: 1♠ - 1NT - ?");
            Yaz("Beklenen: 3♠");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 6'lı ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 5 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+1+3+4 = 15 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1NT", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_FNT_Acici2NT()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 28.4 — FNT: Açıcı 18-19 dengeli → 2NT");
            Yaz("Senaryo: 1♠ - 1NT - ?");
            Yaz("Beklenen: 2NT");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 5'li ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 2 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+2+4+3+4 = 17 → 18 için bir Q daha
            kuzeyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 2 },
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+2+4+3+2+3 = 18 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1NT", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_FNT_Acici2Sinek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 28.5 — FNT: Açıcı 5-3-3-2, minör gösterir");
            Yaz("Senaryo: 1♠ - 1NT - ?");
            Yaz("Beklenen: 2♣ veya 2♦ (3'lü minör)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 5'li ♠
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 2 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 3+1+4+2 = 10 → 5-3-3-2 dağılımı (♠5, ♥3, ♦3, ♣2)

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1NT", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}, ♦: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Karo")}, ♣: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Sinek")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            bool dogru = (sonuc == "2♣" || sonuc == "2♦");
            Yaz(dogru ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♣ veya 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_FNT_Acici4M()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 28.6 — FNT: Açıcı 7'li güzel majör → 4♠");
            Yaz("Senaryo: 1♠ - 1NT - ?");
            Yaz("Beklenen: 4♠");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 7'li ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 5 },
        // 1'li ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
    };
            // HCP: 4+3+1+2+4 = 14 → 15 için artır
            kuzeyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
    };
            // HCP: 4+3+1+3+4 = 15 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1NT", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♠)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 29.x — Inverted Minors
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_Inverted_2Sinek_Guclu()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 29.1 — Inverted: 1♣ - 2♣ (10+ HP, Forcing)");
            Yaz("Beklenen: 2♣");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♣ (destek)
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        // 3'lü ♠ (4'lü majör YOK)
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 11 },
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 6 },
    };
            // HCP: 3+1+1+2 = 7 → 10+ için artır
            guneyEli = new List<Card>
    {
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 6 },
    };
            // HCP: 4+1+3+2 = 10 ✓

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
            Yaz($"♣: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Sinek")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Inverted_3Sinek_Zayif()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 29.2 — Inverted: 1♣ - 3♣ (0-7 HP, Preemptive)");
            Yaz("Beklenen: 3♣");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♣ (destek)
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 4 },
        new Card { Suit = "Sinek", Value = 3 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 8 },
        new Card { Suit = "Maça", Value = 5 },
        new Card { Suit = "Maça", Value = 2 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
    };
            // HCP: 1

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
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Inverted_2Karo_Guclu()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 29.3 — Inverted: 1♦ - 2♦ (10+ HP, Forcing)");
            Yaz("Beklenen: 2♦");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♦ (destek)
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 9 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        // 3'lü ♠ (4'lü majör YOK)
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 6 },
    };
            // HCP: 4+1+3+2 = 10 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
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
            Yaz($"♦: {ElDegerlendirici.RenkUzunlugu(guneyEli, "Karo")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Inverted_MajorVarsaTetiklenmez()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 29.4 — Inverted: 4'lü majör varsa tetiklenmez");
            Yaz("Senaryo: 1♣ - ? (4'lü ♥, 5'li ♣)");
            Yaz("Beklenen: Inverted tetiklenmez (1♥ öncelikli)");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        // 4'lü ♥ (majör var!)
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        new Card { Suit = "Kupa", Value = 2 },
        // 2'li ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 6 },
    };
            // HCP: 4+1+3+2 = 10

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

            // Inverted tetiklenmemeli (4'lü ♥ var)
            var inverted = new InvertedMinors { AktifMi = true };
            bool uygun = inverted.UygunMu(durum);
            Yaz($"Inverted.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 30.x — Reverse
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_Reverse_Basit()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 30.1 — Reverse: 1♣ - 1♠ - 2♦");
            Yaz("Beklenen: 2♦ (Reverse)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 5'li ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        // 4'lü ♦ (Reverse rengi)
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 1'li ♠
        new Card { Suit = "Maça", Value = 10 },
    };
            // HCP: 4+3+4+1+2 = 14 → 16 için artır
            kuzeyEli = new List<Card>
    {
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Maça", Value = 10 },
    };
            // HCP: 4+3+4+2+3 = 16 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"♣: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Sinek")}, ♦: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Karo")}");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Reverse_Minimum()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 30.2 — Reverse: 15 HP ile yapılmaz");
            Yaz("Beklenen: Reverse DEĞİL");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 5'li ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 1'li ♠
        new Card { Suit = "Maça", Value = 10 },
    };
            // HCP: 3+1+2+4 = 10 → 15 için artır
            kuzeyEli = new List<Card>
    {
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        new Card { Suit = "Karo", Value = 14 }, // A = 4
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
    };
            // HCP: 3+1+4+2+3+2 = 15 → Reverse için 16 gerekli

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // Reverse tetiklenmemeli
            var reverse = new Reverse { AktifMi = true };
            bool uygun = reverse.UygunMu(durum);
            Yaz($"Reverse.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Reverse_Degil_Normal()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 30.3 — Reverse DEĞİL: 1♥ - 1♠ - 2♣");
            Yaz("Beklenen: Reverse tetiklenmez (♥ > ♣)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 4'lü ♣
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 4 },
        // 1'li ♠
        new Card { Suit = "Maça", Value = 9 },
    };
            // HCP: 4+2+3+1 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // Reverse tetiklenmemeli (♥ > ♣)
            var reverse = new Reverse { AktifMi = true };
            bool uygun = reverse.UygunMu(durum);
            Yaz($"Reverse.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Reverse_Degil_1NT()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 30.4 — Reverse DEĞİL: 1♠ - 1NT - 2♥");
            Yaz("Beklenen: Reverse tetiklenmez (Forcing NT sonrası)");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 5'li ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 12 }, // Q = 2
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 11 }, // J = 1
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 5 },
        // 1'li ♣
        new Card { Suit = "Sinek", Value = 9 },
    };
            // HCP: 4+3+2+1 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1NT", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // Reverse tetiklenmemeli (1NT cevabı)
            var reverse = new Reverse { AktifMi = true };
            bool uygun = reverse.UygunMu(durum);
            Yaz($"Reverse.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Reverse_CevapciFit()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 30.5 — Reverse sonrası cevapçı fit gösterir");
            Yaz("Senaryo: 1♣ - 1♠ - 2♦ - ?");
            Yaz("Beklenen: 3♣ veya 4♣ (fit)");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        // 3'lü ♣ (fit)
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 8 },
        new Card { Suit = "Kupa", Value = 5 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 3 },
    };
            // HCP: 4+2+1 = 7

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 3, GecerliMi = true },
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

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            bool dogru = (sonuc == "3♣" || sonuc == "4♣");
            Yaz(dogru ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♣ veya 4♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Reverse_Degil_AyniRenk()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 30.6 — Reverse DEĞİL: Aynı renk tekrarı");
            Yaz("Senaryo: 1♣ - 1♠ - 2♣");
            Yaz("Beklenen: Reverse tetiklenmez");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 6'lı ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 4 },
        // 2'li ♥
        new Card { Suit = "Kupa", Value = 12 },
        new Card { Suit = "Kupa", Value = 5 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 11 },
        new Card { Suit = "Karo", Value = 5 },
    };
            // HCP: 4+3+1+2 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♣",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            // Reverse tetiklenmemeli (aynı renk)
            var reverse = new Reverse { AktifMi = true };
            bool uygun = reverse.UygunMu(durum);
            Yaz($"Reverse.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 31.x — Multi-Landy
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_MultiLandy_2Sinek_Majors()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 31.1 — Multi-Landy: 2♣ (majörler)");
            Yaz("Beklenen: 2♣");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
{
    // 5'li ♠
    new Card { Suit = "Maça", Value = 13 }, // K = 3
    new Card { Suit = "Maça", Value = 11 }, // J = 1
    new Card { Suit = "Maça", Value = 9 },
    new Card { Suit = "Maça", Value = 6 },
    new Card { Suit = "Maça", Value = 3 },
    // 5'li ♥
    new Card { Suit = "Kupa", Value = 12 }, // Q = 2
    new Card { Suit = "Kupa", Value = 10 },
    new Card { Suit = "Kupa", Value = 7 },
    new Card { Suit = "Kupa", Value = 5 },
    new Card { Suit = "Kupa", Value = 2 },
    // 3'lü ♦
    new Card { Suit = "Karo", Value = 13 }, // K = 3  ← YENİ
    new Card { Suit = "Karo", Value = 6 },
    new Card { Suit = "Karo", Value = 3 },
};
            // HCP: 3+1+2+3 = 9 ✓ (8+ oldu)

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = MultiLandyAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_MultiLandy_2Karo_TekMajor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 31.2 — Multi-Landy: 2♦ (tek majör)");
            Yaz("Beklenen: 2♦");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 6'lı ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 11 }, // J = 1
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 2'li ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 5 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+1+2 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = MultiLandyAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_MultiLandy_2Kupa_Minor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 31.3 — Multi-Landy: 2♥ (♥ + minör)");
            Yaz("Beklenen: 2♥");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♥
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 11 }, // J = 1
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 3 },
        // 5'li ♦
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
    };
            // HCP: 4+1+3+2 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = MultiLandyAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_MultiLandy_2NT_Minors()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 31.4 — Multi-Landy: 2NT (minörler)");
            Yaz("Beklenen: 2NT");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 5'li ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
        // 5'li ♦
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 10 },
        new Card { Suit = "Karo", Value = 7 },
        new Card { Suit = "Karo", Value = 5 },
        new Card { Suit = "Karo", Value = 2 },
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 12 }, // Q = 2
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
    };
            // HCP: 4+1+3+2 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = MultiLandyAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_MultiLandy_PartnerCevap()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 31.5 — Multi-Landy: Partner 2♣'e cevap");
            Yaz("Senaryo: (1NT) - 2♣ - (Pas) - ?");
            Yaz("Beklenen: 2♥/2♠/2♦");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 10 },
        new Card { Suit = "Maça", Value = 7 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 11 },
        new Card { Suit = "Kupa", Value = 9 },
        new Card { Suit = "Kupa", Value = 6 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 12 },
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 13 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = MultiLandyAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            bool dogru = (sonuc == "2♥" || sonuc == "2♠" || sonuc == "2♦");
            Yaz(dogru ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥/2♠/2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_MultiLandy_Dbl_Ceza()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 31.6 — Multi-Landy: Dbl (ceza, 16+ HP)");
            Yaz("Beklenen: Dbl");
            Yaz("────────────────────────────────────────");

            var guneyEli = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 4'lü ♥
        new Card { Suit = "Kupa", Value = 13 }, // K = 3
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo", Value = 12 }, // Q = 2
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 14 }, // A = 4
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+2+4 = 13 → 16 için artır
            guneyEli = new List<Card>
    {
        new Card { Suit = "Maça", Value = 14 }, // A = 4
        new Card { Suit = "Maça", Value = 13 }, // K = 3
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        new Card { Suit = "Kupa", Value = 14 }, // A = 4
        new Card { Suit = "Kupa", Value = 10 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 2 },
        new Card { Suit = "Karo", Value = 13 }, // K = 3
        new Card { Suit = "Karo", Value = 8 },
        new Card { Suit = "Karo", Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+4+3+2 = 16 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = guneyEli,
                Anlasma = MultiLandyAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Dbl" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Dbl)");
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

            Yaz("▶ TEST 18.1 - 2♣ açılışı — 19 HP + 6-4 dağılım");
            Test_IkiliSinekGuclu_Dogru();

            Yaz("▶ TEST 19.1-19.6: Two Way Checkback");
            Test_TwoWay_ZayifMajor();
            Test_TwoWay_DavetNT();
            Test_TwoWay_GF();
            Test_TwoWay_ZayifMinor();
            Test_TwoWay_PuppetCevap();
            Test_TwoWay_AciciIkiKaroyaCevap();

            Yaz("▶ TEST 20.1-20.6: XYZ Konvansiyonu");
            Test_XYZ_ZayifOzel();
            Test_XYZ_Zayif5liMajor();
            Test_XYZ_DavetPuppet();
            Test_XYZ_GF();
            Test_XYZ_PuppetCevap();
            Test_XYZ_AciciIkiKaroyaCevap();

            Yaz("▶ TEST 21.1-21.6: Trial Bid");
            Test_TrialBid_AciciBaslatma();
            Test_TrialBid_CevapciKabul();
            Test_TrialBid_CevapciRet();
            Test_TrialBid_MacaAcici();
            Test_TrialBid_CevapciMacaKabul();
            Test_TrialBid_CevapciMacaRet();

            Yaz("▶ TEST 22.1-22.4: 3. Renk Forsingi");
            Test_TSF_BasitBaslatma();
            Test_TSF_AciciDestek();
            Test_TSF_AciciDigerMajor();
            Test_TSF_AciciNT();

            Yaz("▶ TEST 23.1-23.4: 4. Renk Forsingi");
            Test_4SF_BasitBaslatma();
            Test_4SF_AciciDestek();
            Test_4SF_AciciNT();
            Test_4SF_XYZCakismasiYok();

            Yaz("▶ TEST 24.1-24.6: Bergen Raises");
            Test_Bergen_3Sinek_Zayif();
            Test_Bergen_3Karo_Davet();
            Test_Bergen_3Major_Preemptive();
            Test_Bergen_4Major_Preemptive();
            Test_Bergen_Acici3Karo();
            Test_Bergen_3luDestek_Yok();

            Yaz("▶ TEST 25.1-25.5: Cappelletti");
            Test_Cappelletti_2Karo_Majors();
            Test_Cappelletti_2Kupa_Minor();
            Test_Cappelletti_2NT_Minors();
            Test_Cappelletti_Double();
            Test_Cappelletti_PartnerCevap_2Karo();

            Yaz("▶ TEST 26.1-26.6: DONT");
            Test_Dont_2Sinek();
            Test_Dont_2Karo();
            Test_Dont_2Kupa_Majors();
            Test_Dont_Double();
            Test_Dont_PartnerCevap_Double();
            Test_Dont_PartnerCevap_2Sinek();

            // ─── TumTestleriCalistir() içine, Test 26.6'dan sonra ekleyin: ───
            //
            Yaz("▶ TEST 27.1-27.8: DOPI / ROPI");
            Test_Dopi_SifirAnahtarKart();
            Test_Dopi_BirAnahtarKart();
            Test_Dopi_IkiKart_KizYok();
            Test_Dopi_IkiKart_KizVar();
            Test_Ropi_SifirAnahtarKart();
            Test_Ropi_BirAnahtarKart();
            Test_Dopi_Negatif_YuksekMudahale();
            Test_Dopi_Negatif_Muddahalesiz();

            Yaz("▶ TEST 28.1-28.6: Forcing NT");
            Test_FNT_Cevapci1NT();
            Test_FNT_Acici2M();
            Test_FNT_Acici3M();
            Test_FNT_Acici2NT();
            Test_FNT_Acici2Sinek();
            Test_FNT_Acici4M();

            Yaz("▶ TEST 29.1-29.4: Inverted Minors");
            Test_Inverted_2Sinek_Guclu();
            Test_Inverted_3Sinek_Zayif();
            Test_Inverted_2Karo_Guclu();
            Test_Inverted_MajorVarsaTetiklenmez();

            Yaz("▶ TEST 30.1-30.6: Reverse");
            Test_Reverse_Basit();
            Test_Reverse_Minimum();
            Test_Reverse_Degil_Normal();
            Test_Reverse_Degil_1NT();
            Test_Reverse_CevapciFit();
            Test_Reverse_Degil_AyniRenk();

            Yaz("▶ TEST 31.1-31.6: Multi-Landy");
            Test_MultiLandy_2Sinek_Majors();
            Test_MultiLandy_2Karo_TekMajor();
            Test_MultiLandy_2Kupa_Minor();
            Test_MultiLandy_2NT_Minors();
            Test_MultiLandy_PartnerCevap();
            Test_MultiLandy_Dbl_Ceza();

            Yaz("");
            Yaz("✅ TÜM TESTLER TAMAMLANDI!");
        }

    }
}