using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

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

            try
            {
                if (SonucYaz != null)
                {
                    // Eğer callback bir Form kontrolüne bağlıysa güvenli çağrı yapalım
                    SonucYaz.Invoke(mesaj);
                }
            }
            catch (ObjectDisposedException)
            {
                // Form veya RichTextBox kapatılmış, hatayı yut ve döngüyü/çalışmayı koru
            }
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

        private static OrtaklikAnlasmasi ZayifIkiAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.ZayifIki = true;
            return a;
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

        public static void Test_Gerber_3As()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 1.4 — Gerber Cevap: 3 As");
            Yaz("Beklenen: 4NT");
            Yaz("────────────────────────────────────────");

            var kuzeyEli = new List<Card>
    {
        // 3 As
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 4 },
    };

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

            Yaz($"AsSayisi: {ElDegerlendirici.AsSayisi(kuzeyEli)}");

            var gerber = new Gerber();
            gerber.AktifMi = true;

            bool uygun = gerber.UygunMu(durum);
            Yaz($"Gerber.UygunMu → {uygun}");

            if (uygun)
            {
                string teklif = gerber.TeklifVer(durum);
                Yaz($"Gerber.TeklifVer → {teklif}");
                Yaz(teklif == "4NT" ? "✅ BEKLENEN: 4NT" : $"❌ BEKLENEN: 4NT, GELEN: {teklif}");
            }

            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif == "4NT" ? "✅ MOTOR: 4NT" : $"❌ MOTOR: 4NT beklendi, gelen {motorTeklif}");
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
        // ♠ KQJxx = 6 HP
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
            // HCP: 6+0+4+2 = 12 HP

            // Sıra saat yönünde: Kuzey → Doğu → Güney → Batı.
            // Doğu (Güney'in hemen sağındaki rakip) 1♥ açtı → Güney DİREKT müdahale ediyor.
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "1♥", Sira = 2, GecerliMi = true },
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
        new Card { Suit = "Karo", Value = 13 },
        new Card { Suit = "Karo", Value = 6 },
        new Card { Suit = "Karo", Value = 3 },
        // ♣ Qxxx = 2 HP
        new Card { Suit = "Sinek", Value = 12 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 2 },
    };
            // 12 HP açıcı, 3'lü Kupa desteği

            var gecmis = new List<IhaleHamlesi>
    {
        // Support Double: Güney (açıcı) 1♦, Kuzey 1♥, Doğu 1♠ → Güney 3'lü ♥ ile Dbl
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "1♠", Sira = 4, GecerliMi = true },
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
        // Drury: Güney (cevapçı) daha önce Pas demiş olmalı (passed hand)
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "Pas", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
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
        new Card { Suit = "Karo", Value = 12 },
        new Card { Suit = "Karo", Value = 6 },
        // 2♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // Toplam: 3+1+4 +2 (Q) = 10 HP (Smolen GF)

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
        new Card { Suit = "Karo", Value = 12 },
        new Card { Suit = "Karo", Value = 6 },
        // 2♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // Toplam: 4+3+1 +2 (Q) = 10 HP (Smolen GF)

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
            // Güney'in eli: 3'lü ♥ + ♣ A (12 HP, slam ilgisi)
            var guneyEli = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça", Value = 9 },
        new Card { Suit = "Maça", Value = 6 },
        new Card { Suit = "Maça", Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa", Value = 13 },
        new Card { Suit = "Kupa", Value = 7 },
        new Card { Suit = "Kupa", Value = 4 },
        // 4'lü ♦
        new Card { Suit = "Karo", Value = 14 },
        new Card { Suit = "Karo", Value = 11 },
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

        public static void Test_Puppet_3luMajor_Fit()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.11 — Puppet: 3'lü ♠ + oyun gücü → 3♣ (5'li majör ara), fit bulununca 4♠");
            Yaz("════════════════════════════════════════");

            var kuzeyEli = new List<Card>
        {
            new Card { Suit = "Maça", Value = 13 },
            new Card { Suit = "Maça", Value = 5 },
            new Card { Suit = "Maça", Value = 4 },
            new Card { Suit = "Kupa", Value = 11 },
            new Card { Suit = "Kupa", Value = 6 },
            new Card { Suit = "Karo", Value = 8 },
            new Card { Suit = "Karo", Value = 5 },
            new Card { Suit = "Karo", Value = 3 },
            new Card { Suit = "Sinek", Value = 10 },
            new Card { Suit = "Sinek", Value = 11 },
            new Card { Suit = "Sinek", Value = 9 },
            new Card { Suit = "Sinek", Value = 7 },
            new Card { Suit = "Sinek", Value = 3 }
        };

            var gecmis1 = new List<IhaleHamlesi>
        {
            new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2NT", Sira = 1, GecerliMi = true },
            new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        };
            var d1 = new IhaleDurumu { AktifOyuncu = Player.Kuzey, AktifOyuncuEli = kuzeyEli, Anlasma = OrtaklikAnlasmasi.Varsayilan(), Gecmis = gecmis1 };
            string s1 = new IhaleMotoru(d1.Anlasma).TeklifVer(d1);
            Yaz($"2NT - Pas - ? → {s1} (beklenen 3♣)");
            Yaz(s1 == "3♣" ? "✅ DOĞRU" : "❌ YANLIŞ (beklenen 3♣)");

            var gecmis2 = new List<IhaleHamlesi>
        {
            new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2NT", Sira = 1, GecerliMi = true },
            new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
            new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♣",  Sira = 3, GecerliMi = true },
            new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
            new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠",  Sira = 5, GecerliMi = true },
            new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        };
            var d2 = new IhaleDurumu { AktifOyuncu = Player.Kuzey, AktifOyuncuEli = kuzeyEli, Anlasma = OrtaklikAnlasmasi.Varsayilan(), Gecmis = gecmis2 };
            string s2 = new IhaleMotoru(d2.Anlasma).TeklifVer(d2);
            Yaz($"2NT - Pas - 3♣ - Pas - 3♠ - Pas - ? → {s2} (beklenen 4♠)");
            Yaz(s2 == "4♠" ? "✅ DOĞRU" : "❌ YANLIŞ (beklenen 4♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_NegatifKontr_5liMajor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 9.12 — Negatif Kontr: 5+ ♠ varsa Dbl değil doğal 1♠; tam 4'lü ♠ ise Dbl");
            Yaz("════════════════════════════════════════");
            {
                var el = new List<Card>
            {
            new Card { Suit = "Maça", Value = 12 },
            new Card { Suit = "Maça", Value = 11 },
            new Card { Suit = "Maça", Value = 9 },
            new Card { Suit = "Maça", Value = 8 },
            new Card { Suit = "Maça", Value = 7 },
            new Card { Suit = "Maça", Value = 4 },
            new Card { Suit = "Kupa", Value = 13 },
            new Card { Suit = "Kupa", Value = 11 },
            new Card { Suit = "Kupa", Value = 10 },
            new Card { Suit = "Karo", Value = 9 },
            new Card { Suit = "Karo", Value = 8 },
            new Card { Suit = "Sinek", Value = 11 },
            new Card { Suit = "Sinek", Value = 8 }
            };
                var g = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♦", Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 2, GecerliMi = true },
            };
                var d = new IhaleDurumu { AktifOyuncu = Player.Dogu, AktifOyuncuEli = el, Anlasma = OrtaklikAnlasmasi.Varsayilan(), Gecmis = g };
                string r = new IhaleMotoru(d.Anlasma).TeklifVer(d);
                Yaz($"1♦-(1♥) ile ♠QJ9874 (6'lı) → {r} (beklenen 1♠)");
                Yaz(r == "1♠" ? "✅ DOĞRU" : "❌ YANLIŞ (beklenen 1♠)");
            }
            {
                var el = new List<Card>
            {
            new Card { Suit = "Maça", Value = 12 },
            new Card { Suit = "Maça", Value = 11 },
            new Card { Suit = "Maça", Value = 9 },
            new Card { Suit = "Maça", Value = 8 },
            new Card { Suit = "Kupa", Value = 13 },
            new Card { Suit = "Kupa", Value = 4 },
            new Card { Suit = "Kupa", Value = 2 },
            new Card { Suit = "Karo", Value = 11 },
            new Card { Suit = "Karo", Value = 8 },
            new Card { Suit = "Karo", Value = 7 },
            new Card { Suit = "Sinek", Value = 12 },
            new Card { Suit = "Sinek", Value = 8 },
            new Card { Suit = "Sinek", Value = 5 }
            };
                var g = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♦", Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 2, GecerliMi = true },
            };
                var d = new IhaleDurumu { AktifOyuncu = Player.Dogu, AktifOyuncuEli = el, Anlasma = OrtaklikAnlasmasi.Varsayilan(), Gecmis = g };
                string r = new IhaleMotoru(d.Anlasma).TeklifVer(d);
                Yaz($"1♦-(1♥) ile ♠QJ98 (4'lü) → {r} (beklenen Dbl)");
                Yaz(r == "Dbl" ? "✅ DOĞRU" : "❌ YANLIŞ (beklenen Dbl)");
            }
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

        public static void Test_Cevap_BesliMajor_1MacaYerine2NT()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 11.1 — 1♣ sonrası 5♠ + 11 HP dengeli: 2NT değil 1♠");
            Yaz("════════════════════════════════════════");

            // ♠10 9 8 6 3  ♥Q 10 6  ♦K 5  ♣A Q 10  (11 HP, 5-3-2-3)
            var kuzeyEli = new List<Card>
            {
                new Card { Suit = "Maça", Value = 10 }, new Card { Suit = "Maça", Value = 9 },
                new Card { Suit = "Maça", Value = 8 },  new Card { Suit = "Maça", Value = 6 },
                new Card { Suit = "Maça", Value = 3 },
                new Card { Suit = "Kupa", Value = 12 }, new Card { Suit = "Kupa", Value = 10 },
                new Card { Suit = "Kupa", Value = 6 },
                new Card { Suit = "Karo", Value = 13 }, new Card { Suit = "Karo", Value = 5 },
                new Card { Suit = "Sinek", Value = 14 }, new Card { Suit = "Sinek", Value = 12 },
                new Card { Suit = "Sinek", Value = 10 },
            };
            var gecmis = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♣",  Sira = 3, GecerliMi = true },
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
            Yaz(sonuc == "1♠" ? "✅ DOĞRU (1♠)" : $"❌ YANLIŞ (beklenen 1♠, gelen {sonuc})");
            Yaz("════════════════════════════════════════");
        }

        // ── 11.2 – 11.4: önceden motora bağlı olmayan konvansiyonların tetiklenme testleri ──
        private static void SeviyeTesti(string baslik, List<Card> el, Player aktif,
                                        List<IhaleHamlesi> gecmis, string beklenen)
        {
            Yaz("════════════════════════════════════════");
            Yaz(baslik);
            Yaz("════════════════════════════════════════");
            var durum = new IhaleDurumu
            {
                AktifOyuncu = aktif,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == beklenen ? $"✅ DOĞRU ({beklenen})"
                                  : $"❌ YANLIŞ (beklenen {beklenen}, gelen {sonuc})");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_DagitimUretici()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 12.2 — Dağıtım profilleri ve en iyi el");
            Yaz("════════════════════════════════════════");
            bool ok = true;
            foreach (DagitimProfili pr in Enum.GetValues(typeof(DagitimProfili)))
                foreach (var iz in new bool[][] { null, new[] { true, false, true, false }, new[] { false, false, true, false } })
                    for (int s = 1; s <= 20; s++)
                    {
                        var d = DagitimUretici.Uret(pr, iz, s);
                        var d2 = DagitimUretici.Uret(pr, iz, s);
                        if (d.Count != 52 || d.Distinct().Count() != 52 || !d.SequenceEqual(d2) || !DagitimUretici.Uygun(d, pr, iz)) ok = false;
                    }
            Yaz(ok ? "✅ DOĞRU" : "❌ YANLIŞ (dağıtım profili)");
            Yaz("════════════════════════════════════════");
        }

        /// <summary>Bridge Baron listesinden eklenen 29 konvansiyonun birim ve tümleşik testleri.</summary>
        public static void Test_YeniKonvansiyonlar()
        {
            Func<string, List<Card>> mk = (spec) =>
            {
                var l = new List<Card>();
                foreach (var g in spec.Split(' '))
                {
                    string suit = g[0] == 'S' ? "Maça" : g[0] == 'H' ? "Kupa" : g[0] == 'D' ? "Karo" : "Sinek";
                    foreach (char ch in g.Substring(2))
                    {
                        int v = ch == 'T' ? 10 : ch == 'J' ? 11 : ch == 'Q' ? 12 : ch == 'K' ? 13 : ch == 'A' ? 14 : ch - '0';
                        l.Add(new Card { Suit = suit, Value = v });
                    }
                }
                return l;
            };
            // Dizi gösterimi: B = ben (Güney), L = sol rakip (Batı), O = ortak (Kuzey), R = sağ rakip (Doğu); Pas'lar yazılmaz.
            Func<string, List<IhaleHamlesi>> dizi = (s) =>
            {
                var l = new List<IhaleHamlesi>();
                if (string.IsNullOrWhiteSpace(s)) return l;
                foreach (var p in s.Split(' '))
                {
                    Player o = p[0] == 'B' ? Player.Guney : p[0] == 'L' ? Player.Bati : p[0] == 'O' ? Player.Kuzey : Player.Dogu;
                    l.Add(new IhaleHamlesi { Oyuncu = o, Teklif = p.Substring(2), Sira = l.Count + 1, GecerliMi = true });
                }
                return l;
            };
            int toplam = 0, hata = 0;
            Action<string, string, string, string, Action<OrtaklikAnlasmasi>> t = (ad, el, seq, beklenen, ayar) =>
            {
                var an = OrtaklikAnlasmasi.Varsayilan();
                if (ayar != null) ayar(an);
                var d = new IhaleDurumu { AktifOyuncu = Player.Guney, AktifOyuncuEli = mk(el), Anlasma = an, Gecmis = dizi(seq) };
                string r = new IhaleMotoru(an).TeklifVer(d);
                bool ok = r == beklenen;
                toplam++; if (!ok) hata++;
                Yaz("════════════════════════════════════════");
                Yaz("TEST 14." + toplam + " — " + ad + " [" + (seq == "" ? "açılış" : seq) + "] → " + r);
                Yaz(ok ? "✅ DOĞRU (" + beklenen + ")" : "❌ YANLIŞ (beklenen " + beklenen + ", gelen " + r + ")");
                Yaz("════════════════════════════════════════");
            };

            // 1NT çevresi
            t("Extended Stayman: zayıf 4-4 majör → 2♣", "S:KJ52 H:Q984 D:743 C:62", "O:1NT", "2♣", a => a.ExtendedStayman = true);
            t("Extended Stayman: 2♦ cevabı → 2♥", "S:KJ52 H:Q984 D:743 C:62", "O:1NT B:2♣ O:2♦", "2♥", a => a.ExtendedStayman = true);
            t("Extended Stayman: 2♥ cevabı → Pas", "S:KJ52 H:Q984 D:743 C:62", "O:1NT B:2♣ O:2♥", "Pas", a => a.ExtendedStayman = true);
            t("Hello: ♠5 ♥4 → 2♣ (iki majör)", "S:AQJ52 H:KT83 D:42 C:52", "R:1NT", "2♣", a => a.HelloOver1NT = true);
            t("Hello: ilerletici majörlerden uzun olanı (♥) seçer", "S:K52 H:QJ54 D:742 C:K52", "L:1NT O:2♣", "2♥", a => a.HelloOver1NT = true);
            t("Sandwich 1NT: (1♥)-P-(1♠) ♦5 ♣4 → 1NT", "S:32 H:42 D:AKJ85 C:Q843", "L:1♥ R:1♠", "1NT", a => a.Sandwich1NT = true);
            t("Sandwich 1NT: ilerletici uzun renk ♣ → 2♣", "S:J85 H:K42 D:Q43 C:KJ74", "R:1♥ L:1♠ O:1NT", "2♣", a => a.Sandwich1NT = true);
            t("Robinson: 5♠ → 2♥ transfer", "S:AJ743 H:62 D:853 C:Q42", "O:1NT R:Dbl", "2♥", a => a.Robinson = true);
            t("Robinson: açıcı 2♥ transferini 2♠ ile tamamlar", "S:KQ4 H:AQ52 D:KJ4 C:Q75", "B:1NT L:Dbl O:2♥", "2♠", a => a.Robinson = true);
            t("Exit Transfer: zayıf 5♥ → 2♦", "S:5 H:Q8643 D:J74 C:9632", "O:1NT R:Dbl", "2♦", a => a.ExitTransfer = true);
            t("Exit Transfer: açıcı 2♦ → 2♥", "S:KQ4 H:AQ52 D:KJ4 C:Q75", "B:1NT L:Dbl O:2♦", "2♥", a => a.ExitTransfer = true);
            t("Gambling 3NT: AKQJ963 ♣ → 3NT", "S:5 H:J4 D:432 C:AKQJ963", "", "3NT", a => a.Gambling3NT = true);
            t("Gambling 3NT: durdurucusuz cevaplayan → 4♣", "S:J42 H:T32 D:982 C:Q543", "O:3NT", "4♣", a => a.Gambling3NT = true);
            t("Gambling 3NT: 3 renkte durdurucu → Pas", "S:KQ4 H:A32 D:K62 C:J54", "O:3NT", "Pas", a => a.Gambling3NT = true);
            t("Wolff Signoff: zayıf 6♣ → 2♣ röle", "S:KJ872 H:42 C:Q98643", "O:1♦ B:1♠ O:1NT", "2♣", a => a.WolffSignoff = true);
            t("Wolff Signoff: açıcı 2♦ ile röleyi kabul eder", "S:Q84 H:K92 D:AJ932 C:Q5", "B:1♦ O:1♠ B:1NT O:2♣", "2♦", a => a.WolffSignoff = true);
            t("Wolff Signoff: 2♦ sonrası 3♣", "S:KJ872 H:42 C:Q98643", "O:1♦ B:1♠ O:1NT B:2♣ O:2♦", "3♣", a => a.WolffSignoff = true);

            // Açılış ve cevaplar
            t("Benjamin: 26 HP → 2♣", "S:AKQJ7 H:AK D:A92 C:KQ2", "", "2♣", a => a.BenjaminTwoBids = true);
            t("Benjamin: 18 HP 6♠ → 2♦", "S:AKQ972 H:KJ D:KQ43 C:3", "", "2♦", a => a.BenjaminTwoBids = true);
            t("Benjamin: zayıf el 2♣'e → 2♦", "S:J72 H:Q85 D:9843 C:T62", "O:2♣", "2♦", a => a.BenjaminTwoBids = true);
            t("Benjamin: 10 HP 5♠ olumlu → 2♠", "S:KQJ95 H:84 D:K32 C:J72", "O:2♣", "2♠", a => a.BenjaminTwoBids = true);
            t("Benjamin: 2♣–2♦ sonrası dengeli 26 → 3NT", "S:AKQJ7 H:AK D:A92 C:KQ2", "B:2♣ O:2♦", "3NT", a => a.BenjaminTwoBids = true);
            t("Weak Jump-Shift: 1♦–2♠ (6♠, 4 HP)", "S:KJ9854 H:42 D:853 C:62", "O:1♦", "2♠", a => a.WeakJumpShiftResp = true);
            t("Weak Jump-Shift: açıcı geçer", "S:Q74 H:K65 D:AQ932 C:K5", "B:1♦ O:2♠", "Pas", a => a.WeakJumpShiftResp = true);
            t("Ogust: 22 HP → 2NT", "S:K5 H:AQ4 D:AKJ3 C:KQ72", "O:2♠", "2NT", a => a.Ogust = true);
            t("Ogust: min iyi renk → 3♦", "S:KQ9743 H:42 D:853 C:62", "B:2♠ O:2NT", "3♦", a => a.Ogust = true);
            t("Ogust: maks zayıf renk → 3♥", "S:K98743 H:AQ D:J53 C:62", "B:2♠ O:2NT", "3♥", a => a.Ogust = true);
            t("Ogust: AKQ → 3NT", "S:AKQ843 H:J4 D:853 C:62", "B:2♠ O:2NT", "3NT", a => a.Ogust = true);
            t("Ogust: 16 HP + maks-iyi cevap → 4♠", "S:AJ4 H:AKQ D:K853 C:A62", "O:2♠ B:2NT O:3♠", "4♠", a => a.Ogust = true);
            t("Inverted Bergen: limit (11 HP) → 3♣", "S:KJ52 H:A84 D:Q75 C:J42", "O:1♠", "3♣", a => a.InvertedBergenRaises = true);
            t("Inverted Bergen: 8 HP → 3♦", "S:KJ52 H:984 D:Q75 C:Q42", "O:1♠", "3♦", a => a.InvertedBergenRaises = true);
            t("Inverted Bergen: açıcı 15 HP limit'e → 4♠", "S:AQ9764 H:K5 D:AJ4 C:Q3", "B:1♠ O:3♣", "4♠", a => a.InvertedBergenRaises = true);
            t("1m-2NT: 12 HP dengeli → 2NT", "S:KJ5 H:Q85 D:AJ52 C:J84", "O:1♣", "2NT", a => a.OneMinorTwoNTInv = true);
            t("1m-2NT: açıcı 16 HP → 3NT", "S:AQ5 H:K65 D:Q4 C:AJ743", "B:1♣ O:2NT", "3NT", a => a.OneMinorTwoNTInv = true);
            t("1m-2NT: açıcı 12 HP → Pas", "S:AQ5 H:K65 D:Q42 C:KJ743", "B:1♣ O:2NT", "Pas", a => a.OneMinorTwoNTInv = true);
            t("German 2♣: 4♥ → 3♣ Stayman", "S:K52 H:QJ84 D:J53 C:742", "O:2♣ B:2♦ O:2NT", "3♣", a => a.German2CRelays = true);
            t("German 2♣: 5♠ → 3♥ transfer", "S:QJ752 H:84 D:J53 C:742", "O:2♣ B:2♦ O:2NT", "3♥", a => a.German2CRelays = true);
            t("German 2♣: açıcı Stayman'a 3♥", "S:AK2 H:AKJ4 D:KQ3 C:AQ2", "B:2♣ O:2♦ B:2NT O:3♣", "3♥", a => a.German2CRelays = true);
            t("Walsh: 1♣–1♦ (♠4 ♦4, 6 HP)", "S:KJ84 H:73 D:Q983 C:962", "O:1♣", "1♦", a => a.Walsh = true);
            t("Walsh: 4-4 majör → 1♥", "S:KJ84 H:Q983 D:73 C:962", "O:1♣", "1♥", a => a.Walsh = true);
            t("Mini-Roman: ♦ tekil, 3 dörtlü → 2♦", "S:AQ84 H:KJ92 D:5 C:Q984", "", "2♦", a => a.MiniRoman2D = true);
            t("Mini-Roman: cevap en uzun renk ♥ → 2♥", "S:K5 H:Q8643 D:J72 C:642", "O:2♦", "2♥", a => a.MiniRoman2D = true);
            t("New Minor Forcing: 1♦–1♠–1NT–2♣", "S:KJ52 H:A84 D:Q75 C:KJ2", "O:1♦ B:1♠ O:1NT", "2♣", a => a.NewMinorForcing = true);
            t("New Minor Forcing: açıcı 3'lü ♠ ile 2♠", "S:Q84 H:K92 D:AJ932 C:Q5", "B:1♦ O:1♠ B:1NT O:2♣", "2♠", a => a.NewMinorForcing = true);
            t("Truscott 2NT: 1♠–(Dbl)–2NT", "S:KJ52 H:A84 D:Q75 C:J42", "O:1♠ R:Dbl", "2NT", a => a.Truscott2NT = true);
            t("Truscott 2NT: açıcı 12 HP → 3♠", "S:AQ974 H:K5 D:J64 C:Q53", "B:1♠ L:Dbl O:2NT", "3♠", a => a.Truscott2NT = true);
            t("Truscott 2NT: açıcı 16 HP → 4♠", "S:AQ974 H:KQ D:AJ4 C:Q53", "B:1♠ L:Dbl O:2NT", "4♠", a => a.Truscott2NT = true);
            t("Artif. 2♣: 3'lü destek limit → 2♣", "S:Q74 H:KJ5 D:A932 C:J84", "O:1♥ R:Dbl", "2♣", a => a.Artificial2C2D = true);
            t("Artif. 2♦: 3'lü destek oyun zorlayan → 2♦", "S:Q74 H:KJ5 D:AK32 C:J84", "O:1♥ R:Dbl", "2♦", a => a.Artificial2C2D = true);
            t("Preemptive Reraise: 6♠ 13 HP → 4♠", "S:AQ9754 H:K4 D:5 C:KJ62", "B:1♠ O:2♠ R:3♦", "4♠", a => a.PreemptiveReraises = true);
            t("Long & Short: ♥ tekil → 3♥ kısalık denemesi", "S:AKJ97 H:5 D:KJ84 C:K52", "B:1♠ O:2♠", "3♥", a => a.LongShortGameTries = true);
            t("Long & Short: boşa giden değer yok → 4♠", "S:Q842 H:732 D:K96 C:J84", "O:1♠ B:2♠ O:3♥", "4♠", a => a.LongShortGameTries = true);
            t("Long & Short: ♥'de honör var → 3♠", "S:Q842 H:KJ2 D:962 C:J84", "O:1♠ B:2♠ O:3♥", "3♠", a => a.LongShortGameTries = true);

            // Rekabet
            t("Weak Jump Overcall: (1♣)–2♠ 6♠", "S:KQJ974 H:84 D:853 C:62", "R:1♣", "2♠", a => a.WeakJumpOvercalls = true);
            t("Weak Jump Overcall: ilerletici ♠ desteği → 3♠", "S:J85 H:K42 D:Q43 C:K742", "L:1♣ O:2♠", "3♠", a => a.WeakJumpOvercalls = true);
            t("Maximal Overcall Dbl: açıcı 19 HP → Dbl", "S:K4 H:AQJ52 D:J5 C:AKJ4", "B:1♥ O:2♥ R:3♦", "Dbl", a => a.MaximalOvercallDbl = true);
            t("Maximal Overcall Dbl: raise eden 9 HP → 4♥", "S:J85 H:K42 D:Q43 C:Q742", "O:1♥ B:2♥ L:3♦ O:Dbl", "4♥", a => a.MaximalOvercallDbl = true);
            t("Maximal Overcall Dbl: raise eden 6 HP → 3♥", "S:J85 H:K42 D:943 C:Q742", "O:1♥ B:2♥ L:3♦ O:Dbl", "3♥", a => a.MaximalOvercallDbl = true);
            t("Ghestem: (1♦) ♥5 ♠5 → 2♦ cue", "S:KQ874 H:AJ985 D:3 C:42", "R:1♦", "2♦", a => a.Ghestem = true);
            t("Ghestem: ilerletici ♠ → 2♠", "S:J854 H:K42 D:Q43 C:K72", "L:1♦ O:2♦", "2♠", a => a.Ghestem = true);
            t("Invisible Cue-Bid: 1♣–(1♠)–2♠", "S:84 H:AQ5 D:K83 C:AJ942", "O:1♣ R:1♠", "2♠", a => a.InvisibleCueBids = true);
            t("Invisible Cue-Bid: açıcı ♠ durdurucusu → 3NT", "S:KQ5 H:K4 D:J42 C:AQ843", "B:1♣ L:1♠ O:2♠", "3NT", a => a.InvisibleCueBids = true);
            t("Limit-Plus Cue-Bid: 1♥–(1♠)–2♠", "S:Q74 H:KJ5 D:A932 C:J84", "O:1♥ R:1♠", "2♠", a => a.LimitPlusCueBids = true);
            t("Limit-Plus Cue-Bid: açıcı 12 HP → 3♥", "S:K4 H:AQ952 D:J54 C:Q73", "B:1♥ L:1♠ O:2♠", "3♥", a => a.LimitPlusCueBids = true);

            // Slam ve kontrol
            t("Mathe: 1♥–3♥ sonrası 3♠ soru", "S:AK84 H:AKJ95 D:Q4 C:A3", "B:1♥ O:3♥", "3♠", a => a.MatheAskingBids = true);
            t("Mathe: ♣ tekil → 4♣", "S:Q85 H:Q842 D:J9753 C:2", "O:1♥ B:3♥ O:3♠", "4♣", a => a.MatheAskingBids = true);
            t("Mathe: kısalık yok → 4♥", "S:Q85 H:Q842 D:J97 C:J32", "O:1♥ B:3♥ O:3♠", "4♥", a => a.MatheAskingBids = true);
            t("Control Asking: 1♠–3♠–4♣", "S:AKJ97 H:AK D:KQ8 C:Q52", "B:1♠ O:3♠", "4♣", a => a.ControlAskingBids = true);
            t("Control Asking: ♣'de As → 4♦ (var)", "S:Q842 H:J72 D:963 C:AJ3", "O:1♠ B:3♠ O:4♣", "4♦", a => a.ControlAskingBids = true);
            t("Control Asking: ♣ kontrolü yok → 4♠", "S:Q842 H:J72 D:963 C:J73", "O:1♠ B:3♠ O:4♣", "4♠", a => a.ControlAskingBids = true);
            t("Specific King Ask: 4NT–5♦ sonrası 5♥", "S:AKQJ7 H:A5 D:KQ3 C:A52", "B:1♠ O:3♠ B:4NT O:5♦", "5♥", a => a.SpecificKingAsks = true);
            t("Specific King Ask: papaz var → 6♠", "S:Q842 H:K72 D:963 C:J73", "O:1♠ B:3♠ O:4NT B:5♦ O:5♥", "6♠", a => a.SpecificKingAsks = true);
            t("Specific King Ask: papaz yok → 5♠", "S:Q842 H:J72 D:963 C:K73", "O:1♠ B:3♠ O:4NT B:5♦ O:5♥", "5♠", a => a.SpecificKingAsks = true);
            t("Voluntary Five Major: (4♥) karşısında 5♠", "S:Q8542 H:7 D:J932 C:J83", "O:1♠ R:4♥", "5♠", a => a.VoluntaryFiveMajor = true);

            // Kapalıyken (varsayılan anlaşma) hiçbiri devreye girmemeli
            var ac = OrtaklikAnlasmasi.Varsayilan();
            bool hepsiKapali = !(ac.BenjaminTwoBids || ac.ExtendedStayman || ac.HelloOver1NT || ac.Truscott2NT || ac.PreemptiveReraises ||
                ac.LongShortGameTries || ac.NewMinorForcing || ac.WeakJumpShiftResp || ac.Ogust || ac.Gambling3NT || ac.SpecificKingAsks ||
                ac.VoluntaryFiveMajor || ac.MatheAskingBids || ac.ControlAskingBids || ac.Ghestem || ac.InvisibleCueBids || ac.LimitPlusCueBids ||
                ac.WeakJumpOvercalls || ac.Sandwich1NT || ac.MaximalOvercallDbl || ac.OneMinorTwoNTInv || ac.German2CRelays || ac.Walsh ||
                ac.MiniRoman2D || ac.Robinson || ac.InvertedBergenRaises || ac.ExitTransfer || ac.Artificial2C2D || ac.WolffSignoff);
            toplam++; if (!hepsiKapali) hata++;
            Yaz("════════════════════════════════════════");
            Yaz("TEST 14." + toplam + " — yeni konvansiyonlar varsayılan anlaşmada kapalı");
            Yaz(hepsiKapali ? "✅ DOĞRU" : "❌ YANLIŞ (varsayılan açık)");
            Yaz("════════════════════════════════════════");

            // Tümleşik: tüm yeni konvansiyonlar AÇIKKEN yüzlerce dağıtımda ihale tutarlı ve hatasız bitmeli
            var tum = OrtaklikAnlasmasi.Varsayilan();
            foreach (var pi in typeof(OrtaklikAnlasmasi).GetProperties())
            {
                if (pi.PropertyType != typeof(bool) || !pi.CanWrite) continue;
                foreach (var ad in new[] { "BenjaminTwoBids","ExtendedStayman","HelloOver1NT","Truscott2NT","PreemptiveReraises","LongShortGameTries","NewMinorForcing",
                    "WeakJumpShiftResp","Ogust","Gambling3NT","SpecificKingAsks","VoluntaryFiveMajor","MatheAskingBids","ControlAskingBids","Ghestem","InvisibleCueBids",
                    "LimitPlusCueBids","WeakJumpOvercalls","Sandwich1NT","MaximalOvercallDbl","OneMinorTwoNTInv","German2CRelays","Walsh","MiniRoman2D","Robinson",
                    "InvertedBergenRaises","Artificial2C2D","WolffSignoff" })
                    if (pi.Name == ad) pi.SetValue(tum, true, null);
            }
            var motorTum = new IhaleMotoru(tum);
            motorTum.HatalardaFirlat = true;
            int sorun = 0, kullanilan = 0; var gorulen = new HashSet<string>();
            for (int seed = 1; seed <= 400; seed++)
            {
                try
                {
                    List<string> ihale;
                    var sayac = KonvansiyonPratigi.KullanilanKonvansiyonlar(motorTum, seed, out ihale);
                    if (ihale.Count == 0 || ihale.Count > 60) sorun++;
                    foreach (var kv in sayac) if (YeniKonvansiyonlar.Mu(kv.Key)) { kullanilan += kv.Value; gorulen.Add(kv.Key); }
                }
                catch (Exception ex) { sorun++; Yaz("İstisna seed " + seed + ": " + ex.Message); }
            }
            toplam++; if (sorun > 0) hata++;
            Yaz("════════════════════════════════════════");
            Yaz("TEST 14." + toplam + " — tüm yeni konvansiyonlar açık, 400 dağıtım: istisna/sorun=" + sorun + ", yeni konvansiyon teklifi=" + kullanilan + " (" + gorulen.Count + " farklı)");
            Yaz(sorun == 0 ? "✅ DOĞRU" : "❌ YANLIŞ (ihale sorunları)");
            Yaz("════════════════════════════════════════");
        }

        /// <summary>Yeni konvansiyonların yer oyunu (kart oyunu) entegrasyonu: yapay çağrılar doğru renk bilgisine çevrilir ve atağı etkiler.</summary>
        public static void Test_IhaleAnalizi()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 13.9 — İhale Yorumu: koltuk analizi (HCP, uzunluk, uyum)");
            Yaz("════════════════════════════════════════");
            int hata = 0;
            Func<Player, OrtaklikAnlasmasi> an = (k) => OrtaklikAnlasmasi.Varsayilan();
            Action<bool, string> kos = (sart, ad) => { if (!sart) { hata++; Yaz("❌ " + ad); } };
            int kupa = Array.IndexOf(IhaleAnalizi.Renkler, "Kupa");

            // 1♥ - Pas - 2♥ - Pas - 4♥ (Güney açar)
            var a = IhaleAnalizi.Analiz(new List<string> { "1♥", "Pas", "2♥", "Pas", "4♥", "Pas", "Pas", "Pas" }, "Guney", an);
            kos(a[Player.Guney].MinUz[kupa] >= 5, "açan: 5+ kupa");
            kos(a[Player.Guney].HcMin >= 12, "açan: en az 12 HCP");
            kos(a[Player.Kuzey].MinUz[kupa] >= 3 && a[Player.Kuzey].HcMax <= 10, "destek veren: 3+ kupa, en çok 10 HCP");
            kos(a[Player.Guney].EnIyiUyum == "Kupa" && a[Player.Kuzey].EnIyiUyum == "Kupa", "ortaklık uyumu: Kupa");
            kos(a[Player.Guney].Satirlar().Contains("En iyi bilinen uyum: Kupa"), "satır: en iyi uyum");
            kos(a[Player.Bati].EnIyiUyum == null, "rakipte uyum yok");

            // 1NT - Pas - 2♣ (Stayman) - Pas - 2♠: Stayman renk göstermez; 2♦ transfer kupa gösterir
            var s = IhaleAnalizi.Analiz(new List<string> { "1NT", "Pas", "2♣", "Pas", "2♠" }, "Kuzey", an);
            kos(s[Player.Guney].SonAciklama.StartsWith("Yapay"), "Stayman 2♣ yapay");
            kos(s[Player.Guney].MinUz.All(u => u < 3), "Stayman renk göstermez");
            kos(s[Player.Kuzey].Dengeli, "1NT açan dengeli");
            var j = IhaleAnalizi.Analiz(new List<string> { "1NT", "Pas", "2♦" }, "Kuzey", an);
            kos(j[Player.Guney].SonAciklama.StartsWith("Yapay") && j[Player.Guney].MinUz[kupa] >= 4, "2♦ transfer: kupa gösterir");

            // Boş ihale: satır yok; konuşmayan koltuk: satır yok
            var b = IhaleAnalizi.Analiz(new List<string>(), "Kuzey", an);
            kos(b.Values.All(y => y.Satirlar().Count == 0), "boş ihale satır üretmez");
            var k1 = IhaleAnalizi.Analiz(new List<string> { "1♠" }, "Kuzey", an);
            kos(k1[Player.Dogu].Satirlar().Count == 0 && k1[Player.Kuzey].Satirlar().Count > 0, "yalnız konuşan koltuk yorumlanır");

            Yaz(hata == 0 ? "✅ DOĞRU" : "❌ YANLIŞ (" + hata + " hata)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_YeniKonvOyunEntegrasyonu()
        {
            int hata = 0;
            Func<string, List<Card>> mk = (spec) =>
            {
                var l = new List<Card>();
                foreach (var g in spec.Split(' '))
                {
                    string suit = g[0] == 'S' ? "Maça" : g[0] == 'H' ? "Kupa" : g[0] == 'D' ? "Karo" : "Sinek";
                    foreach (char ch in g.Substring(2))
                    {
                        int v = ch == 'T' ? 10 : ch == 'J' ? 11 : ch == 'Q' ? 12 : ch == 'K' ? 13 : ch == 'A' ? 14 : ch - '0';
                        l.Add(new Card { Suit = suit, Value = v });
                    }
                }
                return l;
            };
            Func<OrtaklikAnlasmasi, Func<Player, OrtaklikAnlasmasi>> ns = (a) => (k) => (k == Player.Kuzey || k == Player.Guney) ? a : OrtaklikAnlasmasi.Varsayilan();
            Action<string, string[], string[]> kontrol = (ad, bekle, bulunan) =>
            {
                string b = bekle == null ? "null" : string.Join(",", bekle);
                string f = bulunan == null ? "null" : string.Join(",", bulunan);
                if (b != f) { hata++; Yaz("❌ " + ad + ": beklenen [" + b + "] bulunan [" + f + "]"); }
            };
            var on = new Func<Action<OrtaklikAnlasmasi>, OrtaklikAnlasmasi>((ayar) => { var a = OrtaklikAnlasmasi.Varsayilan(); ayar(a); return a; });

            // Hello over 1NT: Batı 1NT, Kuzey 2♣ → ♠♥ ; kapalıyken doğal (null)
            var h = new List<string> { "1NT", "2♣" };
            kontrol("Hello 2♣ açık", new[] { "Maça", "Kupa" }, IhaleYorumcusu.Yorumla(h, "Bati", 1, ns(on(a => a.HelloOver1NT = true))));
            kontrol("Hello 2♣ kapalı", null, IhaleYorumcusu.Yorumla(h, "Bati", 1, ns(OrtaklikAnlasmasi.Varsayilan())));
            kontrol("Hello 2NT", new[] { "Karo", "Sinek" }, IhaleYorumcusu.Yorumla(new List<string> { "1NT", "2NT" }, "Bati", 1, ns(on(a => a.HelloOver1NT = true))));
            // Extended Stayman
            kontrol("Ext.Stayman 2♣", new[] { "Maça", "Kupa" }, IhaleYorumcusu.Yorumla(new List<string> { "1NT", "Pas", "2♣" }, "Kuzey", 2, ns(on(a => a.ExtendedStayman = true))));
            // Ogust: Kuzey 2♥, Güney 2NT, Kuzey 3♣ → ♥
            var og = new List<string> { "2♥", "Pas", "2NT", "Pas", "3♣" };
            kontrol("Ogust cevabı", new[] { "Kupa" }, IhaleYorumcusu.Yorumla(og, "Kuzey", 4, ns(on(a => a.Ogust = true))));
            kontrol("Ogust 2NT sorusu (yapay değil → doğal sayılır)", null, IhaleYorumcusu.Yorumla(og, "Kuzey", 2, ns(on(a => a.Ogust = true))));
            // Mini-Roman 2♦
            kontrol("Mini-Roman 2♦", new[] { "Maça", "Kupa", "Sinek" }, IhaleYorumcusu.Yorumla(new List<string> { "2♦" }, "Kuzey", 0, ns(on(a => a.MiniRoman2D = true))));
            // Benjamin 2♣ → renk yok
            kontrol("Benjamin 2♣", new string[0], IhaleYorumcusu.Yorumla(new List<string> { "2♣" }, "Kuzey", 0, ns(on(a => a.BenjaminTwoBids = true))));
            // Robinson 1NT–X–2♦ = ♥ transferi
            kontrol("Robinson 2♦", new[] { "Kupa" }, IhaleYorumcusu.Yorumla(new List<string> { "1NT", "Dbl", "2♦" }, "Kuzey", 2, ns(on(a => a.Robinson = true))));
            // Sandwich 1NT: 1♣ (L) – 1♥ (R) – 1NT (B) → ♠♦
            var sw = new List<string> { "1♣", "Pas", "1♥", "1NT" };
            kontrol("Sandwich 1NT", new[] { "Maça", "Karo" }, IhaleYorumcusu.Yorumla(sw, "Bati", 3, ns(on(a => a.Sandwich1NT = true))));
            // Gerçek çağrı doğal kalır
            kontrol("Doğal 1♥", null, IhaleYorumcusu.Yorumla(new List<string> { "1♥" }, "Kuzey", 0, ns(on(a => a.Ogust = true))));
            // Atak: Doğu (ortak) 1NT'ye Hello 2♣ dedi (♠♥). Açık anlaşmada ♣ değil, ortağın majörü atılmalı; kapalıyken 2♣ doğal sayılır.
            Func<OrtaklikAnlasmasi, string> atak = (an) =>
            {
                var bag = new OyunBaglami
                {
                    Ben = Player.Bati, Deklaran = Player.Guney, Koz = null, Seviye = 3, ElNo = 0,
                    Elim = mk("S:T4 H:J9852 D:73 C:Q962"),
                    IhaleGecmisi = new List<string> { "1NT", "2♣", "3NT", "Pas", "Pas", "Pas" },
                    IhaleBaslangici = "Kuzey",
                    AnlasmaOf = (k) => (k == Player.Dogu || k == Player.Bati) ? an : OrtaklikAnlasmasi.Varsayilan()
                };
                string neden; var c = OyunZekasi.KartSec(bag, out neden);
                return c == null ? "-" : c.Suit;
            };
            string acik = atak(on(x => x.HelloOver1NT = true)), kapali = atak(OrtaklikAnlasmasi.Varsayilan());
            Yaz("   atak: Hello açık=" + acik + ", kapalı=" + kapali);
            if (acik == "Sinek" || kapali != "Sinek") { hata++; Yaz("❌ Hello atağı: açık=" + acik + " kapalı=" + kapali); }
            // Pratik listesi: her konvansiyon bir kategoride, tekrar yok
            var tumListe = KonvansiyonPratigi.Konvansiyonlar();
            var digerler = tumListe.Where(x => KonvansiyonPratigi.Kategori(x) == KonvansiyonPratigi.DigerKategori).ToList();
            var tekrar = KonvansiyonPratigi.Kategoriler.SelectMany(k => k.Value).GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            var yok = KonvansiyonPratigi.Kategoriler.SelectMany(k => k.Value).Where(x => !tumListe.Contains(x)).ToList();
            if (digerler.Count > 0 || tekrar.Count > 0 || yok.Count > 0)
            { hata++; Yaz("❌ Kategori: kategorisiz=[" + string.Join(",", digerler) + "] tekrar=[" + string.Join(",", tekrar) + "] listede olmayan=[" + string.Join(",", yok) + "]"); }
            Yaz("════════════════════════════════════════");
            Yaz("TEST 14.Y1 — IhaleYorumcusu: yapay çağrıların gösterdiği renkler");
            Yaz(hata == 0 ? "✅ DOĞRU" : "❌ YANLIŞ (" + hata + ")");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_IhaleKosulu()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 12.3 — Kart Dağıtıcı: ihale koşulu eşleşmesi");
            Yaz("════════════════════════════════════════");
            bool ok = true; int sinanan = 0;
            var an = OrtaklikAnlasmasi.Varsayilan();
            var motor = new IhaleMotoru(an);
            var motor2 = new IhaleMotoru(an);
            for (int seed = 1; seed <= 60; seed++)
            {
                List<string> ihale;
                KonvansiyonPratigi.KullanilanKonvansiyonlar(motor, seed, out ihale);
                var deste = DagitimUretici.Uret(DagitimProfili.Rastgele, null, seed);
                int n = Math.Min(3, ihale.Count);
                var hedef = ihale.Take(n).ToList();
                if (!KonvansiyonPratigi.IhaleEslesir(motor2, motor2, an, an, deste, seed, hedef)) ok = false;
                if (n > 0)
                {
                    var yanlis = new List<string>(hedef);
                    yanlis[n - 1] = hedef[n - 1] == "7NT" ? "Pas" : "7NT";
                    if (KonvansiyonPratigi.IhaleEslesir(motor2, motor2, an, an, deste, seed, yanlis)) ok = false;
                    sinanan++;
                }
            }
            if (!KonvansiyonPratigi.IhaleEslesir(motor2, motor2, an, an, DagitimUretici.Uret(DagitimProfili.Rastgele, null, 1), 1, new List<string>())) ok = false;
            Yaz(ok && sinanan > 0 ? "✅ DOĞRU" : "❌ YANLIŞ (ihale koşulu)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_KonvansiyonPratigi()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 13.1 — Konvansiyon pratiği: dağıtım üretimi");
            Yaz("════════════════════════════════════════");
            bool ok = true;
            var adlar = KonvansiyonPratigi.Konvansiyonlar();
            foreach (var gerekli in new[] { "Stayman", "Jacoby Transfer", "Blackwood" })
                if (!adlar.Contains(gerekli)) ok = false;
            if (adlar.Contains("Temel Mantık (Fallback)") || adlar.Contains("Basit Cevap")) ok = false;

            var motor = new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan());
            foreach (var ad in new[] { "Stayman", "Jacoby Transfer" })
            {
                var bulunan = new List<int>();
                for (int seed = 958097384; seed < 958097384 + 3000 && bulunan.Count < 3; seed++)
                    if (KonvansiyonPratigi.Gecerli(motor, seed, ad)) bulunan.Add(seed);
                if (bulunan.Count < 3) ok = false;
                // Tekrarlanabilirlik: aynı seed aynı sonucu vermeli
                foreach (int s in bulunan)
                    if (!KonvansiyonPratigi.Gecerli(new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan()), s, ad)) ok = false;
            }
            Yaz(ok ? "✅ DOĞRU" : "❌ YANLIŞ (konvansiyon pratiği)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_YakinKacis()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 13.2 — Konvansiyon pratiği: yakın kaçışlar");
            Yaz("════════════════════════════════════════");
            bool ok = true; int bulunanYakin = 0;
            var motor = new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan());
            foreach (var ad in new[] { "Stayman", "Jacoby Transfer" })
            {
                int gecerli = 0;
                for (int seed = 958097384; seed < 958097384 + 3000 && gecerli < 3; seed++)
                {
                    if (!KonvansiyonPratigi.Gecerli(motor, seed, ad)) continue;
                    gecerli++;
                    int v = KonvansiyonPratigi.YakinKacisBul(motor, seed, DagitimProfili.Rastgele, ad);
                    if (v == 0) continue;
                    bulunanYakin++;
                    // Yakın kaçışta konvansiyon kullanılmamalı; aynı varyant aynı desteyi vermeli
                    var m2 = new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan());
                    var d1 = KonvansiyonPratigi.Deste(seed, DagitimProfili.Rastgele, v);
                    var d2 = KonvansiyonPratigi.Deste(seed, DagitimProfili.Rastgele, v);
                    if (!d1.SequenceEqual(d2) || d1.Distinct().Count() != 52) ok = false;
                    if (!KonvansiyonPratigi.YakinKacis(m2, seed, DagitimProfili.Rastgele, ad, v)) ok = false;
                }
            }
            if (bulunanYakin == 0) ok = false;
            Yaz("Bulunan yakın kaçış: " + bulunanYakin);
            Yaz(ok ? "✅ DOĞRU" : "❌ YANLIŞ (yakın kaçış)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Slam_Dizisi_2_1()
        {
            // Güney ♠AQ10874 ♥94 ♣AK2 ♦Q4  /  Kuzey ♠K ♥AKQ3 ♣J74 ♦AK972
            // 1♠ – Pas – 2♦ – Pas – 2♠ – Pas – 3♥ – Pas – 3♠ – Pas – 4♦ – Pas – 4NT – Pas – 5♦ – Pas – 6♠
            Func<string, List<Card>> el = s =>
            {
                var l = new List<Card>();
                foreach (var g in s.Split(' '))
                {
                    string suit = g[0] == 'S' ? "Maça" : g[0] == 'H' ? "Kupa" : g[0] == 'D' ? "Karo" : "Sinek";
                    string r = g.Substring(2);
                    for (int i = 0; i < r.Length; i++)
                    {
                        int v;
                        if (r[i] == '1') { v = 10; i++; }
                        else v = "23456789".IndexOf(r[i]) >= 0 ? r[i] - '0' : r[i] == 'J' ? 11 : r[i] == 'Q' ? 12 : r[i] == 'K' ? 13 : 14;
                        l.Add(new Card { Suit = suit, Value = v });
                    }
                }
                return l;
            };
            var guney = el("S:AQ10874 H:94 C:AK2 D:Q4");
            var kuzey = el("S:K H:AKQ3 C:J74 D:AK972");
            string[] seq = { "1♠", "Pas", "2♦", "Pas", "2♠", "Pas", "3♥", "Pas", "3♠", "Pas", "4♦", "Pas", "4NT", "Pas", "5♦", "Pas", "6♠" };
            Player[] sira = { Player.Guney, Player.Bati, Player.Kuzey, Player.Dogu };
            int[] kontrol = { 4, 6, 8, 10, 14, 16 };   // Güney ve Kuzey'in kendi motor kararlarını denetle
            int[] ayni = { 4, 6, 8, 10, 12, 14, 16 };
            foreach (int i in new[] { 4, 6, 8, 10, 12, 16 })
            {
                var gecmis = new List<IhaleHamlesi>();
                for (int k = 0; k < i; k++)
                    gecmis.Add(new IhaleHamlesi { Oyuncu = sira[k % 4], Teklif = seq[k], Sira = k + 1, GecerliMi = true });
                var oy = sira[i % 4];
                SeviyeTesti("TEST 11.7." + i + " — slam dizisi: " + oy + " " + seq[i] + " demeli",
                    oy == Player.Guney ? guney : kuzey, oy, gecmis, seq[i]);
            }
        }

        public static void Test_Ikinci_Renk_2_1_Sonrasi()
        {
            // ♠K  ♥A K Q 3  ♣J 7 4  ♦A K 9 7 2 (20 HP) — 1♠ – Pas – 2♦ – Pas – 2♠ – Pas → 3♥ (3NT değil)
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 13 },
                new Card { Suit = "Kupa", Value = 14 }, new Card { Suit = "Kupa", Value = 13 }, new Card { Suit = "Kupa", Value = 12 }, new Card { Suit = "Kupa", Value = 3 },
                new Card { Suit = "Sinek", Value = 11 }, new Card { Suit = "Sinek", Value = 7 }, new Card { Suit = "Sinek", Value = 4 },
                new Card { Suit = "Karo", Value = 14 }, new Card { Suit = "Karo", Value = 13 }, new Card { Suit = "Karo", Value = 9 }, new Card { Suit = "Karo", Value = 7 }, new Card { Suit = "Karo", Value = 2 }
            };
            SeviyeTesti("TEST 11.6 — 1♠–2♦–2♠ sonrası 20 HP, 5♦ 4♥ → 3♥ (3NT ile kapatma)", el, Player.Kuzey,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♦",  Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♠",  Sira = 5, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
                }, "3♥");
        }

        public static void Test_Destek_Reverse_Degil()
        {
            // ♠J 9 7 3 2  ♥3 2  ♣A Q 5  ♦10 8 4  (7 HP) — 1♣–1♠–2♠ (basit destek) sonrası: Lebensohl 2NT DEĞİL, Pas
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 11 },
                new Card { Suit = "Maça", Value = 9 },
                new Card { Suit = "Maça", Value = 7 },
                new Card { Suit = "Maça", Value = 3 },
                new Card { Suit = "Maça", Value = 2 },
                new Card { Suit = "Kupa", Value = 3 },
                new Card { Suit = "Kupa", Value = 2 },
                new Card { Suit = "Sinek", Value = 14 },
                new Card { Suit = "Sinek", Value = 12 },
                new Card { Suit = "Sinek", Value = 5 },
                new Card { Suit = "Karo", Value = 10 },
                new Card { Suit = "Karo", Value = 8 },
                new Card { Suit = "Karo", Value = 4 }
            };
            SeviyeTesti("TEST 11.8 — 1♣–1♠–2♠ (destek, reverse değil) sonrası 7 HP → Pas", el, Player.Kuzey,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♣",  Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠",  Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 5, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♠",  Sira = 6, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 7, GecerliMi = true },
                }, "Pas");
        }

        public static void Test_4SF_XYZ_Sonrasi_Degil()
        {
            // 1♦–1♥–1♠–2♣(XYZ)–2♦ sonrası: son dört teklif 4SF cevabı sayılmamalı (3♣ çıkmamalı)
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 12 },
                new Card { Suit = "Maça", Value = 10 },
                new Card { Suit = "Maça", Value = 2 },
                new Card { Suit = "Kupa", Value = 13 },
                new Card { Suit = "Kupa", Value = 6 },
                new Card { Suit = "Kupa", Value = 5 },
                new Card { Suit = "Kupa", Value = 2 },
                new Card { Suit = "Karo", Value = 8 },
                new Card { Suit = "Karo", Value = 6 },
                new Card { Suit = "Karo", Value = 2 },
                new Card { Suit = "Sinek", Value = 14 },
                new Card { Suit = "Sinek", Value = 9 },
                new Card { Suit = "Sinek", Value = 5 }
            };
            var g = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♦",  Sira = 2, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 3, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥",  Sira = 4, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 5, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 6, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 7, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♣",  Sira = 8, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 9, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♦",  Sira = 10, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 11, GecerliMi = true },
            };
            var durum = new IhaleDurumu { AktifOyuncu = Player.Kuzey, AktifOyuncuEli = el, Anlasma = OrtaklikAnlasmasi.Varsayilan(), Gecmis = g };
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz("TEST 11.9 — 1♦–1♥–1♠–2♣–2♦ sonrası 9 HP: motor → " + sonuc + " (" + motor.SonKonvansiyonAdi + ")");
            Yaz(motor.SonKonvansiyonAdi != "4. Renk Forsingi" ? "✅ DOĞRU (4SF değil)" : "❌ YANLIŞ (4SF olarak yorumlandı)");
        }

        public static void Test_Acici_Cevapcinin_Majorunu_Destekledi()
        {
            // ♠A52 ♥QJ96 ♣AQ104 ♦Q7 (15 HP) — 1♦ – 1♥ – (1♠) – 2♥ (açıcı 4+ ♥ destek): 8 kartlık majör uyum → 4♥, 3NT değil
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 14 },
                new Card { Suit = "Maça", Value = 5 },
                new Card { Suit = "Maça", Value = 2 },
                new Card { Suit = "Kupa", Value = 12 },
                new Card { Suit = "Kupa", Value = 11 },
                new Card { Suit = "Kupa", Value = 9 },
                new Card { Suit = "Kupa", Value = 6 },
                new Card { Suit = "Sinek", Value = 14 },
                new Card { Suit = "Sinek", Value = 12 },
                new Card { Suit = "Sinek", Value = 10 },
                new Card { Suit = "Sinek", Value = 4 },
                new Card { Suit = "Karo", Value = 12 },
                new Card { Suit = "Karo", Value = 7 }
            };
            SeviyeTesti("TEST 11.10 — 1♦–1♥–(1♠)–2♥ sonrası 15 HP, 4 kupa → 4♥", el, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦",  Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠",  Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♥",  Sira = 5, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
                }, "4♥");
        }

        public static void Test_MinorTransfer_Tamamla_Kontra_Sonrasi()
        {
            // 1NT – (Kontra) – 2♠ (sinek transferi) – Pas: açıcı 3♣ ile transferi tamamlar, Pas demez
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 14 },
                new Card { Suit = "Maça", Value = 4 },
                new Card { Suit = "Kupa", Value = 5 },
                new Card { Suit = "Kupa", Value = 4 },
                new Card { Suit = "Kupa", Value = 3 },
                new Card { Suit = "Sinek", Value = 14 },
                new Card { Suit = "Sinek", Value = 8 },
                new Card { Suit = "Sinek", Value = 3 },
                new Card { Suit = "Karo", Value = 14 },
                new Card { Suit = "Karo", Value = 13 },
                new Card { Suit = "Karo", Value = 11 },
                new Card { Suit = "Karo", Value = 8 },
                new Card { Suit = "Karo", Value = 7 }
            };
            SeviyeTesti("TEST 11.11 — 1NT–(Dbl)–2♠–Pas: minör transferi tamamla → 3♣", el, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1NT", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Dbl", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♠",  Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
                }, "3♣");
        }

        public static void Test_Cevap_16HP_Sicrama_Degil()
        {
            // ♠A4 ♥543 ♣A83 ♦AKJ87 (16 HP) — 1♣ sonrası: 2♦ sıçramalı yeni renk 18+ HP ister, 16 HP'de 1♦
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 14 },
                new Card { Suit = "Maça", Value = 4 },
                new Card { Suit = "Kupa", Value = 5 },
                new Card { Suit = "Kupa", Value = 4 },
                new Card { Suit = "Kupa", Value = 3 },
                new Card { Suit = "Sinek", Value = 14 },
                new Card { Suit = "Sinek", Value = 8 },
                new Card { Suit = "Sinek", Value = 3 },
                new Card { Suit = "Karo", Value = 14 },
                new Card { Suit = "Karo", Value = 13 },
                new Card { Suit = "Karo", Value = 11 },
                new Card { Suit = "Karo", Value = 8 },
                new Card { Suit = "Karo", Value = 7 }
            };
            SeviyeTesti("TEST 11.12 — 1♣ sonrası 16 HP, 5 karo → 1♦ (2♦ sıçrama değil)", el, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣",  Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
                }, "1♦");
        }

        public static void Test_Slam_JumpShift_Destek()
        {
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 10 },
                new Card { Suit = "Maça", Value = 7 },
                new Card { Suit = "Maça", Value = 5 },
                new Card { Suit = "Kupa", Value = 14 },
                new Card { Suit = "Kupa", Value = 13 },
                new Card { Suit = "Kupa", Value = 5 },
                new Card { Suit = "Sinek", Value = 14 },
                new Card { Suit = "Sinek", Value = 13 },
                new Card { Suit = "Sinek", Value = 12 },
                new Card { Suit = "Sinek", Value = 8 },
                new Card { Suit = "Sinek", Value = 3 },
                new Card { Suit = "Karo", Value = 13 },
                new Card { Suit = "Karo", Value = 6 }
            };
            SeviyeTesti("TEST 11.13 — 1♣–2♠ (jump shift) sonrası 19 HP, 3 maça → 3♠ (4♠ ile kapatma yok)", el, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♣", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♠", Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 5, GecerliMi = true },
                }, "3♠");
        }

        public static void Test_Acilis_6liMajor_15HP_1NT_Degil()
        {
            // ♠KQJ843 ♥A5 ♣K3 ♦Q42 → 15 HP, 6-2-2-3: 1♠ açılır, 1NT değil
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 13 }, new Card { Suit = "Maça", Value = 12 },
                new Card { Suit = "Maça", Value = 11 }, new Card { Suit = "Maça", Value = 8 },
                new Card { Suit = "Maça", Value = 4 }, new Card { Suit = "Maça", Value = 3 },
                new Card { Suit = "Kupa", Value = 14 }, new Card { Suit = "Kupa", Value = 5 },
                new Card { Suit = "Sinek", Value = 13 }, new Card { Suit = "Sinek", Value = 3 },
                new Card { Suit = "Karo", Value = 12 }, new Card { Suit = "Karo", Value = 4 },
                new Card { Suit = "Karo", Value = 2 }
            };
            SeviyeTesti("TEST 11.15 — 6'lı majör + 15 HP → 1♠ (1NT değil)", el, Player.Kuzey,
                new List<IhaleHamlesi>(), "1♠");
        }

        public static void Test_MinorTransfer_Sonrasi_Devam()
        {
            var guclu = new List<Card>
            {
                new Card { Suit = "Sinek", Value = 13 },
                new Card { Suit = "Sinek", Value = 12 },
                new Card { Suit = "Sinek", Value = 10 },
                new Card { Suit = "Sinek", Value = 9 },
                new Card { Suit = "Sinek", Value = 2 },
                new Card { Suit = "Kupa", Value = 13 },
                new Card { Suit = "Kupa", Value = 6 },
                new Card { Suit = "Kupa", Value = 3 },
                new Card { Suit = "Maça", Value = 12 },
                new Card { Suit = "Maça", Value = 10 },
                new Card { Suit = "Maça", Value = 5 },
                new Card { Suit = "Karo", Value = 11 },
                new Card { Suit = "Karo", Value = 2 }
            };
            SeviyeTesti("TEST 11.16 — Minör transfer + 11 HP, ortak tamamladı → 3NT", guclu, Player.Kuzey,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1NT", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♠", Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 5, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣", Sira = 6, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 7, GecerliMi = true },
                }, "3NT");
            var zayif = new List<Card>
            {
                new Card { Suit = "Sinek", Value = 13 },
                new Card { Suit = "Sinek", Value = 11 },
                new Card { Suit = "Sinek", Value = 9 },
                new Card { Suit = "Sinek", Value = 8 },
                new Card { Suit = "Sinek", Value = 5 },
                new Card { Suit = "Sinek", Value = 2 },
                new Card { Suit = "Kupa", Value = 11 },
                new Card { Suit = "Kupa", Value = 6 },
                new Card { Suit = "Kupa", Value = 3 },
                new Card { Suit = "Maça", Value = 8 },
                new Card { Suit = "Maça", Value = 4 },
                new Card { Suit = "Karo", Value = 12 },
                new Card { Suit = "Karo", Value = 2 }
            };
            SeviyeTesti("TEST 11.17 — Minör transfer + 7 HP, ortak tamamladı → Pas", zayif, Player.Kuzey,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1NT", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♠", Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 5, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♣", Sira = 6, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 7, GecerliMi = true },
                }, "Pas");
        }

        public static void Test_Mudahale_Sonrasi_2NT_ve_Destek()
        {
            var guney = new List<Card>
            {
                new Card { Suit = "Maça", Value = 13 },
                new Card { Suit = "Maça", Value = 12 },
                new Card { Suit = "Maça", Value = 5 },
                new Card { Suit = "Maça", Value = 3 },
                new Card { Suit = "Kupa", Value = 14 },
                new Card { Suit = "Kupa", Value = 13 },
                new Card { Suit = "Kupa", Value = 3 },
                new Card { Suit = "Kupa", Value = 2 },
                new Card { Suit = "Sinek", Value = 12 },
                new Card { Suit = "Sinek", Value = 8 },
                new Card { Suit = "Sinek", Value = 4 },
                new Card { Suit = "Karo", Value = 14 },
                new Card { Suit = "Karo", Value = 9 }
            };
            SeviyeTesti("TEST 11.18 — 1♣–1♥–(1♠) 18 HP + 4'lü ♥ desteği → 3♥ (sıçramalı)", guney, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♣", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "1♠", Sira = 5, GecerliMi = true },
                }, "3♥");
            var kuzey = new List<Card>
            {
                new Card { Suit = "Maça", Value = 9 },
                new Card { Suit = "Maça", Value = 8 },
                new Card { Suit = "Kupa", Value = 12 },
                new Card { Suit = "Kupa", Value = 10 },
                new Card { Suit = "Kupa", Value = 6 },
                new Card { Suit = "Kupa", Value = 4 },
                new Card { Suit = "Sinek", Value = 13 },
                new Card { Suit = "Sinek", Value = 10 },
                new Card { Suit = "Sinek", Value = 9 },
                new Card { Suit = "Sinek", Value = 2 },
                new Card { Suit = "Karo", Value = 13 },
                new Card { Suit = "Karo", Value = 10 },
                new Card { Suit = "Karo", Value = 6 }
            };
            SeviyeTesti("TEST 11.19 — 1♣–1♥–(1♠)–2NT (18-19), 8 HP, ♠ durdurucum yok → 3NT", kuzey, Player.Kuzey,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♣", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "1♠", Sira = 5, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2NT", Sira = 6, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 7, GecerliMi = true },
                }, "3NT");
        }

        public static void Test_ZayifIki_Karsi_Takeout()
        {
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 13 },
                new Card { Suit = "Maça", Value = 12 },
                new Card { Suit = "Maça", Value = 5 },
                new Card { Suit = "Maça", Value = 3 },
                new Card { Suit = "Kupa", Value = 14 },
                new Card { Suit = "Kupa", Value = 13 },
                new Card { Suit = "Kupa", Value = 3 },
                new Card { Suit = "Kupa", Value = 2 },
                new Card { Suit = "Sinek", Value = 12 },
                new Card { Suit = "Sinek", Value = 8 },
                new Card { Suit = "Sinek", Value = 4 },
                new Card { Suit = "Karo", Value = 14 },
                new Card { Suit = "Karo", Value = 9 }
            };
            SeviyeTesti("TEST 11.20 — (2♦)–Pas–Pas–? 18 HP, 4-4 majör, ♦ kısa → Dbl", el, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "2♦", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 3, GecerliMi = true },
                }, "Dbl");
        }

        public static void Test_Sicramali_Destek_Kabul()
        {
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 9 },
                new Card { Suit = "Maça", Value = 8 },
                new Card { Suit = "Kupa", Value = 12 },
                new Card { Suit = "Kupa", Value = 10 },
                new Card { Suit = "Kupa", Value = 6 },
                new Card { Suit = "Kupa", Value = 4 },
                new Card { Suit = "Sinek", Value = 13 },
                new Card { Suit = "Sinek", Value = 10 },
                new Card { Suit = "Sinek", Value = 9 },
                new Card { Suit = "Sinek", Value = 2 },
                new Card { Suit = "Karo", Value = 13 },
                new Card { Suit = "Karo", Value = 10 },
                new Card { Suit = "Karo", Value = 6 }
            };
            SeviyeTesti("TEST 11.21 — 1♣–1♥–(1♠)–3♥ (16-18) sonrası 8 HP + 4'lü ♥ → 4♥", el, Player.Kuzey,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♣", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "1♠", Sira = 5, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♥", Sira = 6, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 7, GecerliMi = true },
                }, "4♥");
        }

        public static void Test_Deklaran_Dummy_Tek_Kral_Yakma()
        {
            Func<string, string, List<Card>> mk = (suit, r) =>
            {
                var l = new List<Card>();
                foreach (char ch in r)
                {
                    int v = ch == 'T' ? 10 : ch == 'J' ? 11 : ch == 'Q' ? 12 : ch == 'K' ? 13 : ch == 'A' ? 14 : ch - '0';
                    l.Add(new Card { Suit = suit, Value = v });
                }
                return l;
            };
            var guney = new List<Card>();
            guney.AddRange(mk("Maça", "Q98")); guney.AddRange(mk("Kupa", "9"));
            guney.AddRange(mk("Sinek", "6543")); guney.AddRange(mk("Karo", "AJ"));
            var kuzey = new List<Card>();
            kuzey.AddRange(mk("Maça", "K")); kuzey.AddRange(mk("Kupa", "QT"));
            kuzey.AddRange(mk("Sinek", "QJT92")); kuzey.AddRange(mk("Karo", "K7"));
            var b = new OyunBaglami
            {
                Ben = Player.Guney, Deklaran = Player.Guney, Koz = null,
                Elim = guney, OrtakEli = kuzey, ElNo = 3, Seviye = 0
            };
            b.Gecmis.Add((1, Player.Bati, new Card { Suit = "Maça", Value = 14 }, false));
            string neden;
            var k = OyunZekasi.KartSec(b, out neden);
            Yaz("════════════════════════════════════════");
            Yaz("TEST 11.22 — Dummy'de tek ♠K varken deklaran ♠Q ile çıkmaz");
            Yaz(k != null && !(k.Suit == "Maça" && k.Value == 12)
                ? $"✅ DOĞRU ({k.Suit} {k.Value}: {neden})"
                : $"❌ YANLIŞ (♠Q oynandı: {neden})");
        }

        public static void Test_Jacoby_Transfer_Onceki_Pas()
        {
            Func<string, List<Card>> mk = (spec) =>
            {
                var l = new List<Card>();
                foreach (var g in spec.Split(' '))
                {
                    string suit = g[0] == 'S' ? "Maça" : g[0] == 'H' ? "Kupa" : g[0] == 'D' ? "Karo" : "Sinek";
                    foreach (char ch in g.Substring(2))
                    {
                        int v = ch == 'T' ? 10 : ch == 'J' ? 11 : ch == 'Q' ? 12 : ch == 'K' ? 13 : ch == 'A' ? 14 : ch - '0';
                        l.Add(new Card { Suit = suit, Value = v });
                    }
                }
                return l;
            };
            var el = mk("S:AKT54 H:J4 D:K84 C:732");
            var gecmis1 = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "Pas", Sira = 2, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 3, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1NT", Sira = 4, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 5, GecerliMi = true },
            };
            SeviyeTesti("TEST 11.23 — Önce Pas, sonra 1NT: 5♠ + 11 HP → 2♥ (♠ transferi)", el, Player.Guney, gecmis1, "2♥");
            var gecmis2 = new List<IhaleHamlesi>(gecmis1)
            {
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♥", Sira = 6, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 7, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♠", Sira = 8, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 9, GecerliMi = true },
            };
            SeviyeTesti("TEST 11.24 — 1NT–2♥–2♠ sonrası 5♠ + 11 HP, tekil yok → 3NT", el, Player.Guney, gecmis2, "3NT");
        }

        public static void Test_Unusual2NT_Cevap()
        {
            Func<string, List<Card>> mk = (spec) =>
            {
                var l = new List<Card>();
                foreach (var g in spec.Split(' '))
                {
                    string suit = g[0] == 'S' ? "Maça" : g[0] == 'H' ? "Kupa" : g[0] == 'D' ? "Karo" : "Sinek";
                    foreach (char ch in g.Substring(2))
                    {
                        int v = ch == 'T' ? 10 : ch == 'J' ? 11 : ch == 'Q' ? 12 : ch == 'K' ? 13 : ch == 'A' ? 14 : ch - '0';
                        l.Add(new Card { Suit = suit, Value = v });
                    }
                }
                return l;
            };
            var el = mk("S:KQ75 H:KT8 D:AKT C:742");
            SeviyeTesti("TEST 11.25 — (Pas) 1♣ (2NT Unusual ♦♥): 15 HP, ♥ ve ♦ tutuk → 3NT", el, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "2NT", Sira = 3, GecerliMi = true },
                }, "3NT");
        }

        /// <summary>Agresiflik ayarları (açılış / müdahale / önleyici) ve beceri düzeyi.</summary>
        public static void Test_Agresiflik_Ve_Beceri()
        {
            Func<string, List<Card>> mk = (spec) =>
            {
                var l = new List<Card>();
                foreach (var g in spec.Split(' '))
                {
                    string suit = g[0] == 'S' ? "Maça" : g[0] == 'H' ? "Kupa" : g[0] == 'D' ? "Karo" : "Sinek";
                    foreach (char ch in g.Substring(2))
                    {
                        int v = ch == 'T' ? 10 : ch == 'J' ? 11 : ch == 'Q' ? 12 : ch == 'K' ? 13 : ch == 'A' ? 14 : ch - '0';
                        l.Add(new Card { Suit = suit, Value = v });
                    }
                }
                return l;
            };
            Func<Player, string, int, IhaleHamlesi> h = (o, t, sr) => new IhaleHamlesi { Oyuncu = o, Teklif = t, Sira = sr, GecerliMi = true };
            Func<List<Card>, Player, List<IhaleHamlesi>, Action<OrtaklikAnlasmasi>, string> teklif = (el, aktif, gecmis, ayar) =>
            {
                var an = OrtaklikAnlasmasi.Varsayilan();
                if (ayar != null) ayar(an);
                var d = new IhaleDurumu { AktifOyuncu = aktif, AktifOyuncuEli = el, Anlasma = an, Gecmis = gecmis };
                return new IhaleMotoru(an).TeklifVer(d);
            };
            Action<string, string, Func<string, bool>, string> kontrol = (baslik, sonuc, uygun, beklenen) =>
            {
                Yaz("════════════════════════════════════════");
                Yaz(baslik);
                Yaz("Motor → " + sonuc);
                Yaz(uygun(sonuc) ? "✅ DOĞRU (" + beklenen + ")" : "❌ YANLIŞ (beklenen " + beklenen + ", gelen " + sonuc + ")");
                Yaz("════════════════════════════════════════");
            };
            var bos = new List<IhaleHamlesi>();

            // Açılış agresifliği: 10 HP + 5-5 (20 kuralı)
            var el10 = mk("S:KQ754 H:KJ854 D:J2 C:3");
            kontrol("TEST 11.42 — 10 HP 5-5, varsayılan → Pas", teklif(el10, Player.Guney, bos, null), r => r == "Pas", "Pas");
            kontrol("TEST 11.43 — 10 HP 5-5, atak açılış → 1 seviye açar", teklif(el10, Player.Guney, bos, a => a.AcilisAgresifligi = 1), r => r != null && r.StartsWith("1"), "1 seviye açılış");

            // Açılış temkinli: 11 HP + 5-4 (20 kuralı) açılmaz
            var el11 = mk("S:KQ754 H:K985 D:QJ C:32");
            kontrol("TEST 11.44 — 11 HP 5-4, varsayılan → 1♠", teklif(el11, Player.Guney, bos, null), r => r == "1♠", "1♠");
            kontrol("TEST 11.45 — 11 HP 5-4, temkinli açılış → Pas", teklif(el11, Player.Guney, bos, a => a.AcilisAgresifligi = -1), r => r == "Pas", "Pas");

            // Müdahale: Bati 1♥, Kuzey karar verir
            var g1 = new List<IhaleHamlesi> { h(Player.Bati, "1♥", 1) };
            var el7 = mk("S:KJ985 H:63 D:K82 C:743");
            var el8 = mk("S:KJ985 H:63 D:KQ2 C:743");
            kontrol("TEST 11.46 — 7 HP 5♠, varsayılan → Pas", teklif(el7, Player.Kuzey, g1, null), r => r == "Pas", "Pas");
            kontrol("TEST 11.47 — 7 HP 5♠, atak müdahale → 1♠", teklif(el7, Player.Kuzey, g1, a => a.MudahaleAgresifligi = 1), r => r == "1♠", "1♠");
            kontrol("TEST 11.48 — 8 HP 5♠, varsayılan → 1♠", teklif(el8, Player.Kuzey, g1, null), r => r == "1♠", "1♠");
            kontrol("TEST 11.49 — 8 HP 5♠, temkinli müdahale → Pas", teklif(el8, Player.Kuzey, g1, a => a.MudahaleAgresifligi = -1), r => r == "Pas", "Pas");

            // Önleyici açılış
            var el10p = mk("S:AKJ9754 H:32 D:Q43 C:2");
            var el5p = mk("S:K987654 H:32 D:Q43 C:2");
            kontrol("TEST 11.50 — 10 HP 7♠, varsayılan → 3♠ değil", teklif(el10p, Player.Guney, bos, null), r => r != "3♠", "3♠ dışında");
            kontrol("TEST 11.51 — 10 HP 7♠, atak önleyici → 3♠", teklif(el10p, Player.Guney, bos, a => a.PreemptAgresifligi = 1), r => r == "3♠", "3♠");
            kontrol("TEST 11.52 — 5 HP K987654, varsayılan → 3♠", teklif(el5p, Player.Guney, bos, null), r => r == "3♠", "3♠");
            kontrol("TEST 11.53 — 5 HP tek honörlü 7♠, temkinli önleyici → 3♠ değil", teklif(el5p, Player.Guney, bos, a => a.PreemptAgresifligi = -1), r => r != "3♠", "3♠ dışında");

            // Beceri düzeyi: 20 hatasız, 1 sık hata
            var legal = new List<Card>();
            foreach (var c in mk("S:AKQJ7")) legal.Add(c);
            int eski = OyunZekasi.BeceriDuzeyi;
            var eskiRnd = OyunZekasi.BeceriRnd;
            OyunZekasi.BeceriRnd = new Random(12345);
            OyunZekasi.BeceriDuzeyi = 20;
            int hata20 = 0;
            for (int i = 0; i < 2000; i++) if (OyunZekasi.BeceriHatasi(legal) != null) hata20++;
            OyunZekasi.BeceriDuzeyi = 1;
            int hata1 = 0;
            for (int i = 0; i < 2000; i++) if (OyunZekasi.BeceriHatasi(legal) != null) hata1++;
            OyunZekasi.BeceriDuzeyi = eski;
            OyunZekasi.BeceriRnd = eskiRnd;
            kontrol("TEST 11.54 — beceri 20 → hiç hata yok", hata20.ToString(), r => r == "0", "0 hata");
            kontrol("TEST 11.55 — beceri 1 → ~%19 hata", hata1.ToString(), r => { int n = int.Parse(r); return n > 250 && n < 500; }, "250–500 hata / 2000");
        }

        public static void Test_Kontra_Cevaplari()
        {
            Func<string, List<Card>> mk = (spec) =>
            {
                var l = new List<Card>();
                foreach (var g in spec.Split(' '))
                {
                    string suit = g[0] == 'S' ? "Maça" : g[0] == 'H' ? "Kupa" : g[0] == 'D' ? "Karo" : "Sinek";
                    foreach (char ch in g.Substring(2))
                    {
                        int v = ch == 'T' ? 10 : ch == 'J' ? 11 : ch == 'Q' ? 12 : ch == 'K' ? 13 : ch == 'A' ? 14 : ch - '0';
                        l.Add(new Card { Suit = suit, Value = v });
                    }
                }
                return l;
            };
            Func<Player, string, int, IhaleHamlesi> h = (o, t, s) => new IhaleHamlesi { Oyuncu = o, Teklif = t, Sira = s, GecerliMi = true };

            // 11.37 — Support Double'dan sonra cevapçı rakibi kontrada bırakmaz: majörünü söyler
            // Bati 1♣, Kuzey Pas, Dogu 1♥, Guney 1♠, Bati Dbl (support), Kuzey Pas → Dogu: 2♥
            SeviyeTesti("TEST 11.37 — 1♣ Pas 1♥ (1♠) Dbl Pas: cevapçı 5 HP, 4♥ → 2♥ (Pas değil)",
                mk("S:Q4 H:J954 D:T83 C:9854"), Player.Dogu,
                new List<IhaleHamlesi>
                {
                    h(Player.Bati, "1♣", 1), h(Player.Kuzey, "Pas", 2), h(Player.Dogu, "1♥", 3), h(Player.Guney, "1♠", 4),
                    h(Player.Bati, "Dbl", 5), h(Player.Kuzey, "Pas", 6)
                }, "2♥");

            // 11.38 — Rakibin rengi güçlüyse (♠ AKQJ5) ceza pası
            SeviyeTesti("TEST 11.38 — aynı dizi, ♠ KQJT5 elimde → Pas (ceza)",
                mk("S:KQJT5 H:J954 D:T3 C:854"), Player.Dogu,
                new List<IhaleHamlesi>
                {
                    h(Player.Bati, "1♣", 1), h(Player.Kuzey, "Pas", 2), h(Player.Dogu, "1♥", 3), h(Player.Guney, "1♠", 4),
                    h(Player.Bati, "Dbl", 5), h(Player.Kuzey, "Pas", 6)
                }, "Pas");

            // 11.39 — Responsive Double'a cevap: 1♥ (1♠) 2♥ Dbl Pas → advancer ♣/♦ arasından uzununu söyler
            SeviyeTesti("TEST 11.39 — (1♥) 1♠ (2♥) Dbl (Pas): 5 karo → 3♦ (Pas değil)",
                mk("S:AQJ95 H:63 D:K8652 C:4"), Player.Kuzey,
                new List<IhaleHamlesi>
                {
                    h(Player.Bati, "1♥", 1), h(Player.Kuzey, "1♠", 2), h(Player.Dogu, "2♥", 3), h(Player.Guney, "Dbl", 4),
                    h(Player.Bati, "Pas", 5)
                }, "3♦");

            // 11.40 — Responsive Double NT açılışında yoktur: 1NT – Pas – 2NT (minör transfer) sonrası Dbl yapılmaz
            // (güvenlik: çifte karşı el kontr demez)
            SeviyeTesti("TEST 11.40 — 1NT Pas 2NT: Responsive Double devreye girmez (zayıf el → Pas)",
                mk("S:K9754 H:Q863 D:J72 C:4"), Player.Dogu,
                new List<IhaleHamlesi>
                {
                    h(Player.Bati, "1NT", 1), h(Player.Kuzey, "Pas", 2), h(Player.Guney, "2NT", 3)
                }, "Pas");

            // 11.41 — Ortağın 1 seviye müdahalesine (1♣ – 1♠) 3 kart destek + 8 HP: 1NT değil 2♠
            SeviyeTesti("TEST 11.41 — (1♣) 1♠ (Pas): 3 kart ♠ + 8 HP → 2♠ (1NT değil)",
                mk("S:KQ3 H:K94 D:J96 C:9732"), Player.Guney,
                new List<IhaleHamlesi>
                {
                    h(Player.Bati, "1♣", 1), h(Player.Kuzey, "1♠", 2), h(Player.Dogu, "Pas", 3)
                }, "2♠");
        }

        public static void Test_Mudahale_Advancer_Cevap()
        {
            Func<string, List<Card>> mk = (spec) =>
            {
                var l = new List<Card>();
                foreach (var g in spec.Split(' '))
                {
                    string suit = g[0] == 'S' ? "Maça" : g[0] == 'H' ? "Kupa" : g[0] == 'D' ? "Karo" : "Sinek";
                    foreach (char ch in g.Substring(2))
                    {
                        int v = ch == 'T' ? 10 : ch == 'J' ? 11 : ch == 'Q' ? 12 : ch == 'K' ? 13 : ch == 'A' ? 14 : ch - '0';
                        l.Add(new Card { Suit = suit, Value = v });
                    }
                }
                return l;
            };
            Func<Player, string, int, IhaleHamlesi> h = (o, t, s) => new IhaleHamlesi { Oyuncu = o, Teklif = t, Sira = s, GecerliMi = true };

            // 11.27 — Pas geçmiş el, ortağın Michaels'ine (1♥ – 2♥) cevap verir; rakip rengi asla oynanmaz
            SeviyeTesti("TEST 11.27 — Pas, 1♥, 2♥ (Michaels ♠+minör), Pas: ♠ fit yok, 6 HP, 5 karo → 3♦ (Pas değil)",
                mk("S:K5 H:865 D:J7542 C:Q43"), Player.Guney,
                new List<IhaleHamlesi>
                {
                    h(Player.Guney, "Pas", 1), h(Player.Bati, "1♥", 2), h(Player.Kuzey, "2♥", 3), h(Player.Dogu, "Pas", 4)
                }, "3♦");

            // 11.30 — Cue bid rakibin 2. rengine (1♣ – Pas – 1♥ – 2♥): ortak yine Pas demez
            SeviyeTesti("TEST 11.30 — Pas, 1♣, Pas, 1♥, 2♥ (Michaels), Pas: ♠ fit yok, 6 HP, 5 karo → 3♦",
                mk("S:K5 H:865 D:J7542 C:Q43"), Player.Guney,
                new List<IhaleHamlesi>
                {
                    h(Player.Kuzey, "Pas", 1), h(Player.Dogu, "1♣", 2), h(Player.Guney, "Pas", 3),
                    h(Player.Bati, "1♥", 4), h(Player.Kuzey, "2♥", 5), h(Player.Dogu, "Pas", 6)
                }, "3♦");

            // 11.28 — Ortağın Unusual 2NT'sine (1♦ – 2NT = ♣+♥) cevap: eşit uzunlukta yüksek renk
            SeviyeTesti("TEST 11.28 — 1♦, 2NT (Unusual ♣+♥), Pas: ♥ 4 + ♣ 4, 11 HP → 3♥",
                mk("S:6 H:J875 D:QJ75 C:AK72"), Player.Dogu,
                new List<IhaleHamlesi>
                {
                    h(Player.Guney, "1♦", 1), h(Player.Bati, "2NT", 2), h(Player.Kuzey, "Pas", 3)
                }, "3♥");

            // 11.29 — Güçlü el + majör uyumu → oyun
            SeviyeTesti("TEST 11.29 — 1♣, 2NT (Unusual ♦+♥), Pas: 4 kupa, 13 HP → 4♥",
                mk("S:Q5 H:AKJ6 D:T9 C:K8732"), Player.Guney,
                new List<IhaleHamlesi>
                {
                    h(Player.Dogu, "1♣", 1), h(Player.Kuzey, "2NT", 2), h(Player.Bati, "Pas", 3)
                }, "4♥");
        }

        public static void Test_Blackwood_Soran_Karar()
        {
            Func<string, List<Card>> mk = (spec) =>
            {
                var l = new List<Card>();
                foreach (var g in spec.Split(' '))
                {
                    string suit = g[0] == 'S' ? "Maça" : g[0] == 'H' ? "Kupa" : g[0] == 'D' ? "Karo" : "Sinek";
                    foreach (char ch in g.Substring(2))
                    {
                        int v = ch == 'T' ? 10 : ch == 'J' ? 11 : ch == 'Q' ? 12 : ch == 'K' ? 13 : ch == 'A' ? 14 : ch - '0';
                        l.Add(new Card { Suit = suit, Value = v });
                    }
                }
                return l;
            };
            Func<Player, string, int, IhaleHamlesi> h = (o, t, s) => new IhaleHamlesi { Oyuncu = o, Teklif = t, Sira = s, GecerliMi = true };
            Func<string, List<IhaleHamlesi>> gecmis = (cevap) => new List<IhaleHamlesi>
            {
                h(Player.Dogu, "Pas", 1), h(Player.Kuzey, "1♠", 2), h(Player.Bati, "Pas", 3), h(Player.Guney, "3♠", 4),
                h(Player.Dogu, "Pas", 5), h(Player.Kuzey, "4♠", 6), h(Player.Bati, "Pas", 7), h(Player.Guney, "4NT", 8),
                h(Player.Bati, "Pas", 9), h(Player.Kuzey, cevap, 10), h(Player.Dogu, "Pas", 11)
            };

            // 11.35 / 11.36 — NT sonrası 4NT niceliksel davettir (kilit kart sorusu değil)
            var ntGecmis = new List<IhaleHamlesi>
            {
                h(Player.Dogu, "Pas", 1), h(Player.Guney, "1NT", 2), h(Player.Bati, "Pas", 3),
                h(Player.Kuzey, "4NT", 4), h(Player.Dogu, "Pas", 5)
            };
            SeviyeTesti("TEST 11.35 — 1NT – 4NT (niceliksel), 19 HP (üst sınır) → 6NT",
                mk("S:AQ5 H:KJ4 D:KQ73 C:A98"), Player.Guney, ntGecmis, "6NT");
            SeviyeTesti("TEST 11.36 — 1NT – 4NT (niceliksel), 15 HP (alt sınır) → Pas",
                mk("S:K54 H:KQ4 D:QJ73 C:A98"), Player.Guney, ntGecmis, "Pas");

            // 11.31 — 3 kilit kart bende, ortak 5♦ (0 ya da 3): toplam 3 (2 eksik) → 5♠'a kapat
            SeviyeTesti("TEST 11.31 — 4NT sonrası ortak 5♦ (0/3), bende 3 kilit kart → 2 eksik: 5♠'a kapat",
                mk("S:AKJ95 H:A83 D:K4 C:KQ7"), Player.Guney, gecmis("5♦"), "5♠");

            // 11.32 — 4 kilit kart bende, ortak 5♣ (1 ya da 4): toplam 5, kız bilinmiyor → 6♠
            SeviyeTesti("TEST 11.32 — 4NT sonrası ortak 5♣, bende 4 kilit kart, koz kızı bilinmiyor → 6♠",
                mk("S:AKJ95 H:A83 D:A4 C:K72"), Player.Guney, gecmis("5♣"), "6♠");

            // 11.33 — 4 kilit kart + koz kızı bende, ortak 5♣: hepsi bizde, kız var → papaz sor 5NT
            SeviyeTesti("TEST 11.33 — 4NT sonrası ortak 5♣, bende 4 kilit kart + koz kızı → 5NT (papaz sor)",
                mk("S:AKQ95 H:A83 D:A4 C:K72"), Player.Guney, gecmis("5♣"), "5NT");

            // 11.34 — 3 kilit kart bende, ortak 5♣ (1/4): toplam 4 → 1 eksik → 6♠
            SeviyeTesti("TEST 11.34 — 4NT sonrası ortak 5♣ (1/4), bende 3 kilit kart → 1 eksik: 6♠",
                mk("S:AKJ95 H:A83 D:K4 C:KQ7"), Player.Guney, gecmis("5♣"), "6♠");
        }

        public static void Test_Slam_JumpShift_Buyuk()
        {
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 12 },
                new Card { Suit = "Maça", Value = 7 },
                new Card { Suit = "Maça", Value = 5 },
                new Card { Suit = "Kupa", Value = 14 },
                new Card { Suit = "Kupa", Value = 13 },
                new Card { Suit = "Kupa", Value = 5 },
                new Card { Suit = "Sinek", Value = 14 },
                new Card { Suit = "Sinek", Value = 13 },
                new Card { Suit = "Sinek", Value = 12 },
                new Card { Suit = "Sinek", Value = 8 },
                new Card { Suit = "Sinek", Value = 3 },
                new Card { Suit = "Karo", Value = 13 },
                new Card { Suit = "Karo", Value = 6 }
            };
            SeviyeTesti("TEST 11.14 — 5NT papaz sorusu cevabı 6♣ (0 papaz) + benim 3 papazım + koz kızı elimde → 7♠", el, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♣", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♠", Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 5, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠", Sira = 6, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 7, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4♦", Sira = 8, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 9, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 10, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 11, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "5♦", Sira = 12, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 13, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5NT", Sira = 14, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "Pas", Sira = 15, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "6♣", Sira = 16, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "Pas", Sira = 17, GecerliMi = true },
                }, "7♠");
        }

        public static void Test_Rebid2NT_Cevap()
        {
            // ♠J 9  ♥10 8 7 4  ♣A J 10 4  ♦Q J 4  (9 HP)  — 1♦ (18 HP) – 1♥ – 2NT (18-19) sonrası
            var el = new List<Card>
            {
                new Card { Suit = "Maça", Value = 11 }, new Card { Suit = "Maça", Value = 9 },
                new Card { Suit = "Kupa", Value = 10 }, new Card { Suit = "Kupa", Value = 8 }, new Card { Suit = "Kupa", Value = 7 }, new Card { Suit = "Kupa", Value = 4 },
                new Card { Suit = "Sinek", Value = 14 }, new Card { Suit = "Sinek", Value = 11 }, new Card { Suit = "Sinek", Value = 10 }, new Card { Suit = "Sinek", Value = 4 },
                new Card { Suit = "Karo", Value = 12 }, new Card { Suit = "Karo", Value = 11 }, new Card { Suit = "Karo", Value = 4 }
            };
            SeviyeTesti("TEST 11.5 — 1♦–1♥–2NT (18-19) sonrası 9 HP → 3NT", el, Player.Kuzey,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♦",  Sira = 3, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥",  Sira = 5, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2NT", Sira = 7, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
                }, "3NT");
        }

        public static void Test_KoltukHaritasi()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 12.1 — İnsan koltuğu eşlemesi (Fiz/Ic)");
            Yaz("════════════════════════════════════════");
            Player[] hepsi = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            bool ok = true;
            foreach (var insan in hepsi)
            {
                KoltukHaritasi.Insan = insan;
                if (KoltukHaritasi.Fiz(Player.Guney) != insan) ok = false;
                foreach (var p in hepsi)
                    if (KoltukHaritasi.Ic(KoltukHaritasi.Fiz(p)) != p) ok = false;
                // Ortak karşıda kalmalı
                if (KoltukHaritasi.Fiz(Player.Kuzey) == insan) ok = false;
            }
            KoltukHaritasi.Insan = Player.Kuzey;
            if (KoltukHaritasi.Fiz(Player.Kuzey) != Player.Guney) ok = false;   // ortak karşıda
            if (KoltukHaritasi.Fiz(Player.Bati) != Player.Dogu) ok = false;
            if (KoltukHaritasi.CiftlerTers) ok = false;
            KoltukHaritasi.Insan = Player.Dogu;
            if (!KoltukHaritasi.CiftlerTers) ok = false;
            if (KoltukHaritasi.Fiz(Player.Kuzey) != Player.Bati) ok = false;    // saat yönü: G→D, B→K ... Kuzey→Batı
            KoltukHaritasi.Uygula(new bool[] { false, false, true, false });   // varsayılan: Güney
            if (KoltukHaritasi.Insan != Player.Guney || KoltukHaritasi.Fiz(Player.Bati) != Player.Bati) ok = false;
            KoltukHaritasi.Uygula(new bool[] { false, false, false, true });   // Batı
            if (KoltukHaritasi.Insan != Player.Bati) ok = false;
            KoltukHaritasi.Uygula(new bool[] { false, false, true, false });
            Yaz(ok ? "✅ DOĞRU" : "❌ YANLIŞ (koltuk eşlemesi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Preempt_Cevap_Ve_ZayifIki()
        {
            // ♠K 7 4  ♥Q 8 5 3  ♦A 6 2  ♣9 7 5  (9 HP, 3 maça)
            var el1 = new List<Card> { new Card { Suit = "Maça", Value = 13 }, new Card { Suit = "Maça", Value = 7 }, new Card { Suit = "Maça", Value = 4 }, new Card { Suit = "Kupa", Value = 12 }, new Card { Suit = "Kupa", Value = 8 }, new Card { Suit = "Kupa", Value = 5 }, new Card { Suit = "Kupa", Value = 3 }, new Card { Suit = "Karo", Value = 14 }, new Card { Suit = "Karo", Value = 6 }, new Card { Suit = "Karo", Value = 2 }, new Card { Suit = "Sinek", Value = 9 }, new Card { Suit = "Sinek", Value = 7 }, new Card { Suit = "Sinek", Value = 5 } };

            SeviyeTesti("TEST 11.2 — Ortak 3♠ bozucu açtı, 3 maça destek → 4♠", el1, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
                }, "4♠");

            SeviyeTesti("TEST 11.3 — Ortak 2♠ (zayıf iki) açtı, 3 maça + 9 HP → 3♠", el1, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♠",  Sira = 1, GecerliMi = true },
                    new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
                }, "3♠");

            // ♠A K J 8 5  ♥7 3  ♦K 6 4  ♣Q 7 5  (13 HP, 5 maça)
            var el2 = new List<Card> { new Card { Suit = "Maça", Value = 14 }, new Card { Suit = "Maça", Value = 13 }, new Card { Suit = "Maça", Value = 11 }, new Card { Suit = "Maça", Value = 8 }, new Card { Suit = "Maça", Value = 5 }, new Card { Suit = "Kupa", Value = 7 }, new Card { Suit = "Kupa", Value = 3 }, new Card { Suit = "Karo", Value = 13 }, new Card { Suit = "Karo", Value = 6 }, new Card { Suit = "Karo", Value = 4 }, new Card { Suit = "Sinek", Value = 12 }, new Card { Suit = "Sinek", Value = 7 }, new Card { Suit = "Sinek", Value = 5 } };

            SeviyeTesti("TEST 11.4 — Rakip 2♥ (zayıf iki) açtı, 5 maça + 13 HP → 2♠ müdahale", el2, Player.Guney,
                new List<IhaleHamlesi>
                {
                    new IhaleHamlesi { Oyuncu = Player.Dogu, Teklif = "2♥", Sira = 1, GecerliMi = true },
                }, "2♠");
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
            Yaz("TEST 10.1 — Grand Slam Force (Soru)");
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
        new Card { Suit = "Karo", Value = 12 }, // Q  (toplam güç 34+ olsun diye)
        // 2♣
        new Card { Suit = "Sinek", Value = 14 }, // A
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
            Yaz("TEST 10.2 — Grand Slam Force (Cevap)");
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
            Yaz("Beklenen: 6♦ (1 yan papaz; koz papazı sayılmaz)");
            Yaz("────────────────────────────────────────");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "6♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 6♦)");
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
            Yaz("TEST 14.1 — Lebensohl Zayıf İki: 2♥ - Dbl - 2NT (zayıf)");
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
            Yaz("TEST 14.2 — Lebensohl Zayıf İki: 2♥ - Dbl - 3♦ (davet)");
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
            Yaz("TEST 14.3 — Lebensohl Zayıf İki: 2♥ - Dbl - 3♥ (Cue-bid 12+ OP)");
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
            Yaz("TEST 15.1 — Lebensohl Reverse: 1♦-1♠-2♥-2NT (zayıf)");
            Yaz("════════════════════════════════════════");

            // 1♦ (Kuzey) - 1♠ (Güney) - 2♥ reverse (Kuzey) - ? (Güney)
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
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♥", Sira = 5, GecerliMi = true },
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

            // 1♦ - 1♠ - 2♥ - 2NT - Pas - 3♣ - Pas - ? (Güney)
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
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♥",  Sira = 5, GecerliMi = true },
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

        private static OrtaklikAnlasmasi BasitCevapAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.InvertedMinors = false;   // BasitCevap'ı test etmek için Inverted Minors kapalı
            return a;
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
                Anlasma = BasitCevapAnlasmasi(),
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
            Yaz("TEST 22.4 — 3. Renk Forsingi: Açıcı 2NT/3NT (dengeli)");
            Yaz("Senaryo: 1♦ - 1♠ - 2♦ - 3♣ - ?");
            Yaz("Beklenen: 2NT veya 3NT (3♣ sonrası 2NT yasal değil → 3NT)");
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
            bool dogru22 = (sonuc == "2NT" || sonuc == "3NT");
            Yaz(dogru22 ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT veya 3NT)");
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

            // Direct seat → min 9 HP (DONT HP eşiği, 2026-10-14)
            var guneyEli = new List<Card>
            {
                // 5'li ♣  (K J = 4)
                new Card { Suit = "Sinek", Value = 13 }, // K = 3
                new Card { Suit = "Sinek", Value = 11 }, // J = 1
                new Card { Suit = "Sinek", Value = 9 },
                new Card { Suit = "Sinek", Value = 6 },
                new Card { Suit = "Sinek", Value = 3 },
                // 5'li ♠  (A Q = 6)
                new Card { Suit = "Maça", Value = 14 }, // A = 4
                new Card { Suit = "Maça", Value = 12 }, // Q = 2
                new Card { Suit = "Maça", Value = 7 },
                new Card { Suit = "Maça", Value = 5 },
                new Card { Suit = "Maça", Value = 2 },
                // 3'lü ♦
                new Card { Suit = "Karo", Value = 9 },
                new Card { Suit = "Karo", Value = 6 },
                new Card { Suit = "Karo", Value = 3 },
            };
            // HCP: 3+1+4+2 = 10  (≥ 9 direct)

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

            // Direct seat → min 9 HP
            var guneyEli = new List<Card>
            {
                // 5'li ♦  (A K = 7)
                new Card { Suit = "Karo", Value = 14 }, // A = 4
                new Card { Suit = "Karo", Value = 13 }, // K = 3
                new Card { Suit = "Karo", Value = 9 },
                new Card { Suit = "Karo", Value = 6 },
                new Card { Suit = "Karo", Value = 3 },
                // 5'li ♥  (Q = 2)
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
            // HCP: 4+3+2 = 9  (≥ 9 direct)

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

            // Direct seat → min 9 HP
            var guneyEli = new List<Card>
            {
                // 5'li ♥  (K J = 4)
                new Card { Suit = "Kupa", Value = 13 }, // K = 3
                new Card { Suit = "Kupa", Value = 11 }, // J = 1
                new Card { Suit = "Kupa", Value = 9 },
                new Card { Suit = "Kupa", Value = 6 },
                new Card { Suit = "Kupa", Value = 3 },
                // 5'li ♠  (A Q = 6)
                new Card { Suit = "Maça", Value = 14 }, // A = 4
                new Card { Suit = "Maça", Value = 12 }, // Q = 2
                new Card { Suit = "Maça", Value = 7 },
                new Card { Suit = "Maça", Value = 5 },
                new Card { Suit = "Maça", Value = 2 },
                // 3'lü ♣
                new Card { Suit = "Sinek", Value = 9 },
                new Card { Suit = "Sinek", Value = 6 },
                new Card { Suit = "Sinek", Value = 3 },
            };
            // HCP: 3+1+4+2 = 10  (≥ 9 direct)

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

            // Direct seat → min 9 HP; 6'lı tek renk → Dbl
            var guneyEli = new List<Card>
            {
                // 6'lı ♥ (A K Q = 9)
                new Card { Suit = "Kupa", Value = 14 }, // A = 4
                new Card { Suit = "Kupa", Value = 13 }, // K = 3
                new Card { Suit = "Kupa", Value = 12 }, // Q = 2
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
            // HCP: 4+3+2 = 9  (≥ 9 direct)

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

        // ═══════════════════════════════════════════════════════════════════
        // TEST 32.x — Zayıf İki
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_ZayifIki_Maca()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 32.1 — Zayıf İki: 2♠ (6'lı ♠, 8 HP)");
            Yaz("Beklenen: 2♠");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A = 4
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 10 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+2+2 = 8 ✓

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Maça")}");

            var w2b = new ZayifIki { AktifMi = true };
            Yaz($"ZayifIki.UygunMu → {w2b.UygunMu(durum)}");
            if (w2b.UygunMu(durum))
                Yaz($"ZayifIki.TeklifVer → {w2b.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_ZayifIki_Kupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 32.2 — Zayıf İki: 2♥ (6'lı ♥, 9 HP)");
            Yaz("Beklenen: 2♥");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Kupa",  Value = 14 }, // A = 4
        new Card { Suit = "Kupa",  Value = 11 }, // J = 1
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 4+1+2+2 = 9 ✓

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_ZayifIki_Karo()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 32.3 — Zayıf İki: 2♦ (6'lı ♦, 8 HP)");
            Yaz("Beklenen: 2♦");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Karo",  Value = 13 }, // K = 3
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 3 },
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 11 }, // J = 1
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 3+2+1+2 = 8 ✓

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♦ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Karo")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_ZayifIki_Negatif_YuksekHP()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 32.4 — Zayıf İki Negatif: 15 HP (çok güçlü)");
            Yaz("Beklenen: 2♠ DEĞİL (normal açılış)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A = 4
        new Card { Suit = "Maça",  Value = 13 }, // K = 3
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A = 4
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 4+3+2+4+2 = 15 → Weak 2-Bid aralığının dışında

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");

            var w2b = new ZayifIki { AktifMi = true };
            bool uygun = w2b.UygunMu(durum);
            Yaz($"ZayifIki.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_ZayifIki_Negatif_5liRenk()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 32.5 — Zayıf İki Negatif: 5'li renk");
            Yaz("Beklenen: 2♠ DEĞİL (6'lı değil)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 }, // K = 3
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },   // sadece 5'li!
        new Card { Suit = "Kupa",  Value = 11 }, // J = 1
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 3+2+1+2 = 8, ama ♠ sadece 5'li

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Maça")}");

            var w2b = new ZayifIki { AktifMi = true };
            bool uygun = w2b.UygunMu(durum);
            Yaz($"ZayifIki.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_ZayifIki_Negatif_GecmisVar()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 32.6 — Zayıf İki Negatif: açılış değil");
            Yaz("Senaryo: Rakip 1♥ açtı, sıra bizde");
            Yaz("Beklenen: Zayıf İki tetiklenmez");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A = 4
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 10 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1♥", Sira = 1, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Maça")}");

            var w2b = new ZayifIki { AktifMi = true };
            bool uygun = w2b.UygunMu(durum);
            Yaz($"ZayifIki.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi — açılış değil)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 33.x — Preempt (Bozucu Açılışlar)
        // ═══════════════════════════════════════════════════════════════════

        private static OrtaklikAnlasmasi PreemptAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.Preempt = true;
            return a;
        }

        public static void Test_Preempt_3Maca()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 33.1 — Preempt: 3♠ (7'li ♠, 5 HP)");
            Yaz("Beklenen: 3♠");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 }, // K = 3
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 4 },
        new Card { Suit = "Maça",  Value = 2 },   // 7'li ♠
        new Card { Suit = "Kupa",  Value = 10 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3+2 = 5 ✓

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = PreemptAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Maça")}");

            var p = new Preempt { AktifMi = true };
            Yaz($"Preempt.UygunMu → {p.UygunMu(durum)}");
            if (p.UygunMu(durum))
                Yaz($"Preempt.TeklifVer → {p.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Preempt_3Kupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 33.2 — Preempt: 3♥ (7'li ♥, 6 HP)");
            Yaz("Beklenen: 3♥");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Kupa",  Value = 12 }, // Q = 2
        new Card { Suit = "Kupa",  Value = 11 }, // J = 1
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 2 },   // 7'li ♥
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 2+1+3 = 6 ✓

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = PreemptAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Preempt_3Sinek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 33.3 — Preempt: 3♣ (7'li ♣, 4 HP)");
            Yaz("Beklenen: 3♣");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Sinek", Value = 13 }, // K = 3
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 4 },
        new Card { Suit = "Sinek", Value = 2 },   // 7'li ♣
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Karo",  Value = 11 }, // J = 1
        new Card { Suit = "Karo",  Value = 5 },
    };
            // HCP: 3+1 = 4 ✓

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = PreemptAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♣ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Sinek")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Preempt_4Karo()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 33.4 — Preempt: 4♦ (8'li ♦, 7 HP)");
            Yaz("Beklenen: 4♦");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Karo",  Value = 14 }, // A = 4
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Karo",  Value = 3 },
        new Card { Suit = "Karo",  Value = 2 },   // 8'li ♦
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 12 }, // Q = 2
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
    };
            // HCP: 4+2+1 = 7 ✓

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = PreemptAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♦ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Karo")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Preempt_Negatif_GucluEl()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 33.5 — Preempt Negatif: 15 HP (çok güçlü)");
            Yaz("Beklenen: Preempt tetiklenmez");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A = 4
        new Card { Suit = "Maça",  Value = 13 }, // K = 3
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },   // 7'li ♠ ama çok güçlü
        new Card { Suit = "Kupa",  Value = 14 }, // A = 4
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Sinek", Value = 9 },
    };
            // HCP: 4+3+2+4+2 = 15

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = PreemptAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");

            var p = new Preempt { AktifMi = true };
            bool uygun = p.UygunMu(durum);
            Yaz($"Preempt.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Preempt_Negatif_KisaRenk()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 33.6 — Preempt Negatif: 5'li renk");
            Yaz("Beklenen: Preempt tetiklenmez (7+ lazım)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 }, // K = 3
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },   // sadece 5'li
        new Card { Suit = "Kupa",  Value = 11 }, // J = 1
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 3+2+1+2 = 8, ♠ 5'li, ♥ 4'lü

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = PreemptAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Maça")}");

            var p = new Preempt { AktifMi = true };
            bool uygun = p.UygunMu(durum);
            Yaz($"Preempt.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 34.x — Namyats (Güçlü Majör Açılışları)
        // ═══════════════════════════════════════════════════════════════════

        private static OrtaklikAnlasmasi NamyatsAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.Namyats = true;
            a.Preempt = true;   // 3 seviyesi preempt hâlâ aktif
            return a;
        }

        public static void Test_Namyats_4Sinek_GucluKupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 34.1 — Namyats: 4♣ (güçlü ♥, 8'li ♥)");
            Yaz("Beklenen: 4♣");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        // 8'li ♥ (sağlam)
        new Card { Suit = "Kupa",  Value = 14 }, // A = 4
        new Card { Suit = "Kupa",  Value = 13 }, // K = 3
        new Card { Suit = "Kupa",  Value = 11 }, // J = 1
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 4 },
        // 2'li ♠
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 6 },
    };
            // HCP: 4+3+1+2 = 10 ✓ (Namyats aralığı 10-15)

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")}");

            var n = new Namyats { AktifMi = true };
            Yaz($"Namyats.UygunMu → {n.UygunMu(durum)}");
            if (n.UygunMu(durum))
                Yaz($"Namyats.TeklifVer → {n.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Namyats_4Karo_GucluMaca()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 34.2 — Namyats: 4♦ (güçlü ♠, 8'li ♠)");
            Yaz("Beklenen: 4♦");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        // 8'li ♠ (sağlam)
        new Card { Suit = "Maça",  Value = 14 }, // A = 4
        new Card { Suit = "Maça",  Value = 13 }, // K = 3
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Maça",  Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 4 },
        // 2'li ♥
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
    };
            // HCP: 4+3+2 = 9... biraz artıralım
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A = 4
        new Card { Suit = "Maça",  Value = 13 }, // K = 3
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 11 }, // J = 1
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Maça",  Value = 2 },
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
    };
            // HCP: 4+3+2+1 = 10 ✓

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♠ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Maça")}");

            var n = new Namyats { AktifMi = true };
            Yaz($"Namyats.UygunMu → {n.UygunMu(durum)}");
            if (n.UygunMu(durum))
                Yaz($"Namyats.TeklifVer → {n.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Namyats_Negatif_ZayifEl()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 34.3 — Namyats Negatif: 5 HP (çok zayıf)");
            Yaz("Beklenen: Namyats tetiklenmez (Preempt alır)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Kupa",  Value = 13 }, // K = 3
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 2 },   // 7'li ♥
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 3+2 = 5 → Namyats aralığı dışı

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")}");

            var n = new Namyats { AktifMi = true };
            bool uygun = n.UygunMu(durum);
            Yaz($"Namyats.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Namyats_PreemptCakismasiYok()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 34.4 — Namyats + Preempt: 8'li ♣ (Namyats almalı mı?)");
            Yaz("Beklenen: 8'li minör → Preempt (Namyats değil)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        // 8'li ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
        new Card { Suit = "Sinek", Value = 2 },
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Karo",  Value = 8 },
    };
            // HCP: 2+1 = 3 → Namyats aralığı dışı (10-15)

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♣ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Sinek")}");

            var n = new Namyats { AktifMi = true };
            bool uygun = n.UygunMu(durum);
            Yaz($"Namyats.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (Namyats tetiklenmedi)" : "❌ YANLIŞ");

            // Motor Preempt veya 3♣ demeli (Namyats 4♣ DEĞİL)
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc != "4♣" ? "✅ DOĞRU (4♣ Namyats'a ait, doğru değil)"
                               : "❌ YANLIŞ (Namyats yanlış tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Namyats_Negatif_KisaRenk()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 34.5 — Namyats Negatif: 6'lı ♥ (8+ lazım)");
            Yaz("Beklenen: Namyats tetiklenmez");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Kupa",  Value = 14 }, // A = 4
        new Card { Suit = "Kupa",  Value = 13 }, // K = 3
        new Card { Suit = "Kupa",  Value = 11 }, // J = 1
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },   // sadece 6'lı
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Karo",  Value = 12 }, // Q = 2
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 4+3+1+2 = 10, ama ♥ sadece 6'lı

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥ Uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")}");

            var n = new Namyats { AktifMi = true };
            bool uygun = n.UygunMu(durum);
            Yaz($"Namyats.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 35.x — Namyats Cevapları
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_NamyatsCevap_SignOff()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 35.1 — Namyats Cevap: Sign-off (0-7 HP)");
            Yaz("Senaryo: Partner 4♣ (güçlü ♥), sıra bizde");
            Yaz("Beklenen: 4♥ (oynamak için)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },   // 3'lü ♥
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 0 → sign-off

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_NamyatsCevap_AraAdim()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 35.2 — Namyats Cevap: Ara adım (slam ilgisi)");
            Yaz("Senaryo: Partner 4♣ (güçlü ♥), sıra bizde");
            Yaz("Beklenen: 4♦ (ara adım)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Kupa",  Value = 10 },
        new Card { Suit = "Kupa",  Value = 7 },   // 2'li ♥
        new Card { Suit = "Maça",  Value = 14 }, // A = 4
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Karo",  Value = 13 }, // K = 3
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
        new Card { Suit = "Maça",  Value = 3 },
    };
            // HCP: 4+3+2 = 9 → 8+ HP, 3 As yok → ara adım

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"As: {ElDegerlendirici.AsSayisi(el)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_NamyatsCevap_Blackwood()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 35.3 — Namyats Cevap: Blackwood (3+ As)");
            Yaz("Senaryo: Partner 4♣ (güçlü ♥), sıra bizde");
            Yaz("Beklenen: 4NT");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Kupa",  Value = 10 },
        new Card { Suit = "Kupa",  Value = 7 },   // 2'li ♥
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 14 }, // A
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Karo",  Value = 4 },
    };
            // 3 As → Blackwood

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"As: {ElDegerlendirici.AsSayisi(el)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_NamyatsCevap_SignOff_Maca()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 35.4 — Namyats Cevap: 4♦ açılışına sign-off");
            Yaz("Senaryo: Partner 4♦ (güçlü ♠), sıra bizde");
            Yaz("Beklenen: 4♠ (oynamak için)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },   // 3'lü ♠
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 0 → sign-off

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_NamyatsCevap_AraAdimSonrasi_Aciyor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 35.5 — Namyats: Ara adım sonrası açıcı cevabı");
            Yaz("Senaryo: 4♣ - Pas - 4♦ - Pas - ? (açıcı)");
            Yaz("Beklenen: 4♥ veya 5♣/5♦/5NT");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        // Açıcının eli (8'li ♥)
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 11 }, // J
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Karo",  Value = 12 }, // Q
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 9 },
    };

            // Geçmiş: 4♣ (Güney) - Pas (Batı) - 4♦ (Kuzey) - Pas (Doğu)
            // Sıra: Güney (açıcı)
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4♦", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = gecmis,
            };

            // Önce UygunMu kontrolü
            var n = new Namyats { AktifMi = true };
            Yaz($"Namyats.UygunMu → {n.UygunMu(durum)}");
            if (n.UygunMu(durum))
                Yaz($"Namyats.TeklifVer → {n.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            bool dogru = (sonuc == "4♥" || sonuc == "5♣" || sonuc == "5♦" || sonuc == "5NT");
            Yaz(dogru ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♥/5♣/5♦/5NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_NamyatsCevap_Negatif_AcilisDegil()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 35.6 — Namyats Cevap Negatif: açılış değil");
            Yaz("Senaryo: Rakip 1♥ açtı, sıra bizde");
            Yaz("Beklenen: Namyats tetiklenmez");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1♥", Sira = 1, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = NamyatsAnlasmasi(),
                Gecmis = gecmis,
            };

            var n = new Namyats { AktifMi = true };
            bool uygun = n.UygunMu(durum);
            Yaz($"Namyats.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 36.x — Roman Key Card Blackwood (RKCB 1430)
        // ═══════════════════════════════════════════════════════════════════

        private static OrtaklikAnlasmasi RKCBAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.RKCB = true;
            a.RKCB1430 = true;
            a.Blackwood = false;  // Çakışmasın
            return a;
        }

        public static void Test_RKCB_1KilitKart()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 36.1 — RKCB: 1 kilit kart → 5♣");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - ? (koz ♠)");
            Yaz("Beklenen: 5♣");
            Yaz("────────────────────────────────────────");

            // ♠ koz, 1 kilit kart (♠K), koz Q yok
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 }, // K (koz K = kilit kart)
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = RKCBAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"Kilit kart: {ElDegerlendirici.KilitKartSayisi(el, "Maça")}");
            Yaz($"Koz Q: {ElDegerlendirici.KizVar(el, "Maça")}");

            var rkcb = new RKCB { AktifMi = true };
            Yaz($"RKCB.UygunMu → {rkcb.UygunMu(durum)}");
            if (rkcb.UygunMu(durum))
                Yaz($"RKCB.TeklifVer → {rkcb.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_RKCB_0KilitKart()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 36.2 — RKCB: 0 kilit kart → 5♦");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - ? (koz ♠)");
            Yaz("Beklenen: 5♦");
            Yaz("────────────────────────────────────────");

            // ♠ koz, 0 kilit kart (As yok, ♠K yok)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = RKCBAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"Kilit kart: {ElDegerlendirici.KilitKartSayisi(el, "Maça")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_RKCB_2KilitKart_QYok()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 36.3 — RKCB: 2 kilit kart, koz Q yok → 5♥");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - ? (koz ♠)");
            Yaz("Beklenen: 5♥");
            Yaz("────────────────────────────────────────");

            // ♠ koz, 2 kilit kart (♦A + ♥A), ♠Q yok
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A (kilit kart)
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Karo",  Value = 14 }, // A (kilit kart)
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = RKCBAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"Kilit kart: {ElDegerlendirici.KilitKartSayisi(el, "Maça")}");
            Yaz($"Koz Q: {ElDegerlendirici.KizVar(el, "Maça")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_RKCB_2KilitKart_QVar()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 36.4 — RKCB: 2 kilit kart + koz Q → 5♠");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - ? (koz ♠)");
            Yaz("Beklenen: 5♠");
            Yaz("────────────────────────────────────────");

            // ♠ koz, 2 kilit kart (♦A + ♥A), ♠Q var
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 12 }, // Q (koz Q)
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A (kilit kart)
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Karo",  Value = 14 }, // A (kilit kart)
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = RKCBAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"Kilit kart: {ElDegerlendirici.KilitKartSayisi(el, "Maça")}");
            Yaz($"Koz Q: {ElDegerlendirici.KizVar(el, "Maça")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_RKCB_3KilitKart()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 36.5 — RKCB: 3 kilit kart → 5♦");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - ? (koz ♠)");
            Yaz("Beklenen: 5♦");
            Yaz("────────────────────────────────────────");

            // ♠ koz, 3 kilit kart (♠K + ♥A + ♦A)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 }, // K (koz K = kilit kart)
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = RKCBAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"Kilit kart: {ElDegerlendirici.KilitKartSayisi(el, "Maça")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_RKCB_4KilitKart()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 36.6 — RKCB: 4 kilit kart → 5♣");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - ? (koz ♠)");
            Yaz("Beklenen: 5♣");
            Yaz("────────────────────────────────────────");

            // ♠ koz, 4 kilit kart (♠K + ♥A + ♦A + ♣A)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 }, // K (koz K)
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 14 }, // A
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = RKCBAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"Kilit kart: {ElDegerlendirici.KilitKartSayisi(el, "Maça")}");

            var motor = new IhaleMotoru(durum.Anlasma);

            // ─── DEBUG: Hangi konvansiyonlar uygun? ────────────────────────
            Yaz("─── Uygun konvansiyonlar ───");
            Yaz(motor.CakismaRaporu(durum));
            Yaz("────────────────────────────────────────");
            // ───────────────────────────────────────────────────────────────

            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♣)");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_RKCB_0314_1KilitKart()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 36.7 — RKCB 0314: 1 kilit kart → 5♦");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - ? (0314 sistemi)");
            Yaz("Beklenen: 5♦");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 }, // K (koz K = kilit kart)
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 12 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var anlasma = OrtaklikAnlasmasi.Varsayilan();
            anlasma.RKCB = true;
            anlasma.RKCB1430 = false;   // 0314
            anlasma.Blackwood = false;

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = anlasma,
                Gecmis = gecmis,
            };

            Yaz($"Sistem: 0314 (RKCB1430 = false)");
            Yaz($"Kilit kart: {ElDegerlendirici.KilitKartSayisi(el, "Maça")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_RKCB_Negatif_NTKoz()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 36.8 — RKCB Negatif: NT kozu");
            Yaz("Senaryo: 1NT - 4NT (NT koz, klasik Blackwood)");
            Yaz("Beklenen: RKCB tetiklenmez");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 },
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 14 },
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 13 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 2 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1NT", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "4NT", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = RKCBAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"AnlasilanKoz: {durum.AnlasilanKoz() ?? "null"}");

            var rkcb = new RKCB { AktifMi = true };
            bool uygun = rkcb.UygunMu(durum);
            Yaz($"RKCB.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (NT kozu, RKCB değil)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 37.x — Exclusion Blackwood
        // ═══════════════════════════════════════════════════════════════════

        private static OrtaklikAnlasmasi ExclusionAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.RKCB = true;
            a.RKCB1430 = true;
            a.Blackwood = false;
            return a;
        }

        public static void Test_Exclusion_5Karo_0Kilit()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 37.1 — Exclusion: 5♦ (♦ void), 0 kilit → 5♥");
            Yaz("Senaryo: 1♠ - 3♠ - Pas - 5♦ (♦ void) - Pas");
            Yaz("Beklenen: 5♥");
            Yaz("────────────────────────────────────────");

            // ♠ koz, ♦ void, 0 kilit kart
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 10 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // ♦ void, ♠K yok, ♥/♣ As yok → 0 kilit kart

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5♦", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = ExclusionAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"♦ void: {ElDegerlendirici.RenkUzunlugu(el, "Karo") == 0}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Exclusion_5Karo_3Kilit()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 37.2 — Exclusion: 5♦ (♦ void), 3 kilit → 5♥");
            Yaz("Senaryo: 1♠ - 3♠ - Pas - 5♦ (♦ void) - Pas");
            Yaz("Beklenen: 5♥ (0/3 → 1. kademe)");
            Yaz("────────────────────────────────────────");

            // ♠ koz, ♦ void, 3 kilit kart (♠K + ♥A + ♣A)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 }, // ♠K (koz K)
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // ♥A
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Sinek", Value = 14 }, // ♣A
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5♦", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = ExclusionAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Exclusion_5Karo_1Kilit()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 37.3 — Exclusion: 5♦ (♦ void), 1 kilit → 5♠");
            Yaz("Senaryo: 1♠ - 3♠ - Pas - 5♦ (♦ void) - Pas");
            Yaz("Beklenen: 5♠ (1/4 → 2. kademe)");
            Yaz("────────────────────────────────────────");

            // ♠ koz, ♦ void, 1 kilit kart (sadece ♥A)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // ♥A
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5♦", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = ExclusionAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Exclusion_5Karo_2Kilit_QYok()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 37.4 — Exclusion: 5♦ (♦ void), 2 kilit, Q yok → 5NT");
            Yaz("Senaryo: 1♠ - 3♠ - Pas - 5♦ (♦ void) - Pas");
            Yaz("Beklenen: 5NT (2, Q yok → 3. kademe)");
            Yaz("────────────────────────────────────────");

            // ♠ koz, ♦ void, 2 kilit kart (♥A + ♣A), ♠Q yok
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // ♥A
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Sinek", Value = 14 }, // ♣A
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5♦", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = ExclusionAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Exclusion_5Karo_2Kilit_QVar()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 37.5 — Exclusion: 5♦ (♦ void), 2 kilit + Q → 6♣");
            Yaz("Senaryo: 1♠ - 3♠ - Pas - 5♦ (♦ void) - Pas");
            Yaz("Beklenen: 6♣ (2 + Q → 4. kademe)");
            Yaz("────────────────────────────────────────");

            // ♠ koz, ♦ void, 2 kilit kart (♥A + ♣A), ♠Q var
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 12 }, // ♠Q (koz Q)
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // ♥A
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Sinek", Value = 14 }, // ♣A
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5♦", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = ExclusionAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "6♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 6♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Exclusion_SikanYok()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 37.6 — Exclusion Negatif: Şikan yok");
            Yaz("Senaryo: 1♠ - 3♠ - Pas - ? (♦'da şikan yok)");
            Yaz("Beklenen: Exclusion tetiklenmez");
            Yaz("────────────────────────────────────────");

            // ♦'da şikan yok (♦ kartı var) → Exclusion olmamalı
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 },
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Karo",  Value = 10 }, // ♦ kartı VAR
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Sinek", Value = 14 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = ExclusionAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"♦ uzunluk: {ElDegerlendirici.RenkUzunlugu(el, "Karo")}");

            var excl = new ExclusionBlackwood { AktifMi = true };
            bool uygun = excl.UygunMu(durum);
            Yaz($"ExclusionBlackwood.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Exclusion_1Kademe_5Sinek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 37.7 — Exclusion: 5♣ (♣ void), 3 kilit → 5♦");
            Yaz("Senaryo: 1♥ - 3♥ - Pas - 5♣ (♣ void) - Pas");
            Yaz("Beklenen: 5♦ (0/3 → 1. kademe)");
            Yaz("────────────────────────────────────────");

            // ♥ koz, ♣ void, 3 kilit kart (♥K + ♠A + ♦A)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // ♠A
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 13 }, // ♥K (koz K)
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 14 }, // ♦A
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Karo",  Value = 2 },
        new Card { Suit = "Maça",  Value = 3 },
    };
            // ♣ void, ♥K + ♠A + ♦A = 3 kilit kart

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5♣", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = ExclusionAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"♣ void: {ElDegerlendirici.RenkUzunlugu(el, "Sinek") == 0}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Exclusion_0314_5Karo_1Kilit()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 37.8 — Exclusion 0314: 5♦ (♦ void), 1 kilit → 5♥");
            Yaz("Senaryo: 1♠ - 3♠ - Pas - 5♦ - Pas (0314)");
            Yaz("Beklenen: 5♥ (1/4 → 1. kademe, 0314)");
            Yaz("────────────────────────────────────────");

            // ♠ koz, ♦ void, 1 kilit kart (♥A)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // ♥A
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5♦", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var anlasma = OrtaklikAnlasmasi.Varsayilan();
            anlasma.RKCB = true;
            anlasma.RKCB1430 = false;   // 0314
            anlasma.Blackwood = false;

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = anlasma,
                Gecmis = gecmis,
            };

            Yaz($"Sistem: 0314 (RKCB1430 = false)");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♥)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 38.x — Kickback
        // ═══════════════════════════════════════════════════════════════════

        private static OrtaklikAnlasmasi KickbackAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.RKCB = true;
            a.RKCB1430 = true;
            a.Blackwood = false;
            return a;
        }

        public static void Test_Kickback_1Kilit_Kupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 38.1 — Kickback: ♥ koz, 4♠ sorusu, 1 kilit → 5♣");
            Yaz("Senaryo: 1♥ - 3♥ - Pas - 4♠ (Kickback) - Pas");
            Yaz("Beklenen: 5♣ (1/4 → 2. kademe)");
            Yaz("────────────────────────────────────────");

            // ♥ koz, 1 kilit kart (♦A)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 14 }, // ♦A
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♠", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = KickbackAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"Kilit kart: {ElDegerlendirici.KilitKartSayisi(el, "Kupa")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Kickback_0Kilit_Kupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 38.2 — Kickback: ♥ koz, 0 kilit → 4NT");
            Yaz("Senaryo: 1♥ - 3♥ - Pas - 4♠ (Kickback) - Pas");
            Yaz("Beklenen: 4NT (0/3 → 1. kademe)");
            Yaz("────────────────────────────────────────");

            // ♥ koz, 0 kilit kart (♥K yok, As yok)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♠", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = KickbackAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Kickback_2Kilit_QYok_Kupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 38.3 — Kickback: ♥ koz, 2 kilit, Q yok → 5♦");
            Yaz("Senaryo: 1♥ - 3♥ - Pas - 4♠ (Kickback) - Pas");
            Yaz("Beklenen: 5♦ (2, Q yok → 3. kademe)");
            Yaz("────────────────────────────────────────");

            // ♥ koz, 2 kilit (♠A + ♦A), ♥Q yok
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // ♠A
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 14 }, // ♦A
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♠", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = KickbackAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Kickback_2Kilit_QVar_Kupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 38.4 — Kickback: ♥ koz, 2 kilit + Q → 5♥");
            Yaz("Senaryo: 1♥ - 3♥ - Pas - 4♠ (Kickback) - Pas");
            Yaz("Beklenen: 5♥ (2 + Q → 4. kademe)");
            Yaz("────────────────────────────────────────");

            // ♥ koz, 2 kilit (♠A + ♦A), ♥Q var
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // ♠A
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 12 }, // ♥Q
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 14 }, // ♦A
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♠", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = KickbackAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Kickback_Karo_Koz()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 38.5 — Kickback: ♦ koz, 4♥ sorusu, 0 kilit → 4♠");
            Yaz("Senaryo: 1♦ - 3♦ - Pas - 4♥ (Kickback) - Pas");
            Yaz("Beklenen: 4♠ (0/3 → 1. kademe)");
            Yaz("────────────────────────────────────────");

            // ♦ koz, 0 kilit (♦K yok, As yok)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 10 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Karo",  Value = 3 },
        new Card { Suit = "Sinek", Value = 11 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♦", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♥", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = KickbackAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♠" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♠)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Kickback_Sinek_Koz()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 38.6 — Kickback: ♣ koz, 4♦ sorusu, 3 kilit → 4♥");
            Yaz("Senaryo: 1♣ - 3♣ - Pas - 4♦ (Kickback) - Pas");
            Yaz("Beklenen: 4♥ (0/3 → 1. kademe)");
            Yaz("────────────────────────────────────────");

            // ♣ koz, 3 kilit (♣K + ♥A + ♦A)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // ♥A
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Karo",  Value = 14 }, // ♦A
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 13 }, // ♣K (koz K)
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♣", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♣", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♦", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = KickbackAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4♥" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Kickback_Negatif_MacaKoz()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 38.7 — Kickback Negatif: ♠ koz (RKCB kullanılır)");
            Yaz("Senaryo: 1♠ - 3♠ - Pas - ? (♠ koz)");
            Yaz("Beklenen: Kickback tetiklenmez");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 },
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Karo",  Value = 14 },
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = KickbackAnlasmasi(),
                Gecmis = gecmis,
            };

            var kb = new Kickback { AktifMi = true };
            bool uygun = kb.UygunMu(durum);
            Yaz($"Kickback.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (♠ koz, Kickback değil)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Kickback_0314_Kupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 38.8 — Kickback 0314: ♥ koz, 4♠, 1 kilit → 4NT");
            Yaz("Senaryo: 1♥ - 3♥ - Pas - 4♠ - Pas (0314)");
            Yaz("Beklenen: 4NT (1/4 → 1. kademe, 0314)");
            Yaz("────────────────────────────────────────");

            // ♥ koz, 1 kilit (♦A)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 14 }, // ♦A
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Sinek", Value = 11 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♥", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♠", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var anlasma = OrtaklikAnlasmasi.Varsayilan();
            anlasma.RKCB = true;
            anlasma.RKCB1430 = false;   // 0314
            anlasma.Blackwood = false;

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = anlasma,
                Gecmis = gecmis,
            };

            Yaz($"Sistem: 0314");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "4NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 4NT)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 39.x — Koz Q Sorusu
        // ═══════════════════════════════════════════════════════════════════

        private static OrtaklikAnlasmasi KozQAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.RKCB = true;
            a.RKCB1430 = true;
            a.Blackwood = false;
            return a;
        }

        public static void Test_KozQ_Soruluyor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 39.1 — Koz Q Sorusu: 5NT soruluyor");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - 5♥ - Pas - ?");
            Yaz("Beklenen: 5NT (Koz Q sorusu)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 13 }, // K
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 14 }, // A (slam sorusu için yeterli toplam güç)
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "5♥",  Sira = 7, GecerliMi = true }, // 2, Q yok
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = KozQAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_KozQ_Cevap_QYok_SinekK()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 39.2 — 5NT papaz sorusu: 1 yan papaz (♣K) → 6♦");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - 5♥ - Pas - 5NT - Pas - ?");
            Yaz("Beklenen: 6♦ (1 yan papaz)");
            Yaz("────────────────────────────────────────");

            // ♠ koz, ♠Q yok, ♣K var
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // ♣K
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "5♥",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 8, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5NT", Sira = 9, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 10, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = KozQAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"♠Q var: {ElDegerlendirici.KizVar(el, "Maça")}");
            Yaz($"♣K var: {ElDegerlendirici.PapazVar(el, "Sinek")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "6♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 6♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_KozQ_Cevap_QVar_YanKYok()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 39.3 — 5NT papaz sorusu: yan papaz yok → 6♣");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - 5♥ - Pas - 5NT - Pas - ?");
            Yaz("Beklenen: 6♣ (0 yan papaz)");
            Yaz("────────────────────────────────────────");

            // ♠ koz, ♠Q var, yan K yok (sadece As'lar var)
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 12 }, // ♠Q
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 14 }, // A
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "5♥",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 8, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5NT", Sira = 9, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 10, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = KozQAnlasmasi(),
                Gecmis = gecmis,
            };

            Yaz($"♠Q var: {ElDegerlendirici.KizVar(el, "Maça")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "6♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 6♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_KozQ_Cevap_QVar_YanKVar()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 39.4 — 5NT papaz sorusu: 1 yan papaz (♣K) → 6♦");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - 5♥ - Pas - 5NT - Pas - ?");
            Yaz("Beklenen: 6♦ (1 yan papaz)");
            Yaz("────────────────────────────────────────");

            // ♠ koz, ♠Q var, ♣K var
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 12 }, // ♠Q
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // ♣K
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "5♥",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 8, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5NT", Sira = 9, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 10, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = KozQAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "6♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 6♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_KozQ_Kickback_Sonrasi()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 39.5 — Koz Q Sorusu: Kickback sonrası");
            Yaz("Senaryo: 1♥ - 3♥ - 4♠ (Kickback) - 5♦ - Pas - ?");
            Yaz("Beklenen: 5NT (Koz Q sorusu)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 14 }, // A (slam sorusu için yeterli toplam güç)
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4♠",  Sira = 5, GecerliMi = true }, // Kickback
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "5♦",  Sira = 7, GecerliMi = true }, // 2, Q yok
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = KozQAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_KozQ_Negatif_MuddahaleYok()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 39.6 — Koz Q Negatif: RKCB/Kickback yok");
            Yaz("Senaryo: 1♠ - 3♠ - Pas - ?");
            Yaz("Beklenen: KozQ tetiklenmez");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 },
        new Card { Suit = "Maça",  Value = 13 },
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 13 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠", Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠", Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = KozQAnlasmasi(),
                Gecmis = gecmis,
            };

            var kq = new KozQSorusu { AktifMi = true };
            bool uygun = kq.UygunMu(durum);
            Yaz($"KozQSorusu.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 40.x — 5NT Pick-a-Slam
        // ═══════════════════════════════════════════════════════════════════

        private static OrtaklikAnlasmasi PickASlamAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.RKCB = true;
            a.RKCB1430 = true;
            a.Blackwood = false;
            return a;
        }

        public static void Test_PickASlam_Soruluyor()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 40.1 — Pick-a-Slam: 5NT soruluyor");
            Yaz("Senaryo: 1♦ - 1♥ - 3♦ - Pas - ?");
            Yaz("Beklenen: 5NT (Pick-a-Slam)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = PickASlamAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "5NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 5NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_PickASlam_Cevap_Sinek()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 40.2 — Pick-a-Slam Cevap: 6♣ (en uzun minör)");
            Yaz("Senaryo: 1♦ - 1♥ - 3♦ - Pas - 5NT - Pas - ?");
            Yaz("Beklenen: 6♣");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 },
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 13 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // K
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 11 }, // J
        new Card { Suit = "Sinek", Value = 5 },
    };
            // ♣ 4'lü, ♦ 3'lü → 6♣

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♦",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5NT", Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = PickASlamAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "6♣" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 6♣)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_PickASlam_Cevap_Karo()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 40.3 — Pick-a-Slam Cevap: 6♦ (en uzun minör)");
            Yaz("Senaryo: 1♣ - 1♠ - 3♣ - Pas - 5NT - Pas - ?");
            Yaz("Beklenen: 6♦");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 },
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 11 }, // J
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Sinek", Value = 12 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // ♦ 4'lü, ♣ 3'lü → 6♦

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♣",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5NT", Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = PickASlamAnlasmasi(),
                Gecmis = gecmis,
            };

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "6♦" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 6♦)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_PickASlam_Negatif_RKCBSekansi()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 40.4 — Pick-a-Slam Negatif: RKCB sekansı");
            Yaz("Senaryo: 1♠ - 3♠ - 4NT - 5♥ - Pas - 5NT - Pas - ?");
            Yaz("Beklenen: Pick-a-Slam tetiklenmez (KozQ)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 },
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Karo",  Value = 13 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 3 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "4NT", Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "5♥",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 8, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "5NT", Sira = 9, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 10, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = PickASlamAnlasmasi(),
                Gecmis = gecmis,
            };

            var pas = new PickASlam { AktifMi = true };
            bool uygun = pas.UygunMu(durum);
            Yaz($"PickASlam.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (RKCB sekansı, Pick-a-Slam değil)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_PickASlam_Negatif_DusukSeviye()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 40.5 — Pick-a-Slam Negatif: düşük seviye");
            Yaz("Senaryo: 1♦ - 1♥ - Pas - ?");
            Yaz("Beklenen: Pick-a-Slam tetiklenmez");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 },
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 },
        new Card { Suit = "Kupa",  Value = 12 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 13 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♦",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♥",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = PickASlamAnlasmasi(),
                Gecmis = gecmis,
            };

            var pas = new PickASlam { AktifMi = true };
            bool uygun = pas.UygunMu(durum);
            Yaz($"PickASlam.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (seviye düşük)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_PickASlam_Negatif_MinorYok()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 40.6 — Pick-a-Slam Negatif: ne minör ne iki majör");
            Yaz("Senaryo: 1♥ - 1♠ - 3♥ - Pas - ? (6♠+3♥+2♦+2♣)");
            Yaz("Beklenen: Pick-a-Slam tetiklenmez (minör yok, iki majör yok)");
            Yaz("────────────────────────────────────────");

            // 6♠ + 3♥ + 2♦ + 2♣ — ne minör 4+ ne iki majör 4+
            var el = new List<Card>
    {
        // 6'lı ♠
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 13 }, // K
        new Card { Suit = "Maça",  Value = 11 }, // J
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        // 2'li ♦
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 7 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };

            // 1♥ - 1♠ - 3♥ - Pas - ? (sıra Güney'de)
            // Not: 3♥ = açıcının 6+ ♥ ve iyi el göstergesi;
            //      cevapçının Pick-a-Slam'a uygun olup olmadığını test ediyoruz.
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♠",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "3♥",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = PickASlamAnlasmasi(),
                Gecmis = gecmis,
            };

            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            Yaz($"Dağılım: ♠{maca} ♥{kupa} ♦{karo} ♣{sinek}");
            Yaz($"Minör 4+ var mı: {(karo >= 4 || sinek >= 4)}");
            Yaz($"İki majör 4+ var mı: {(kupa >= 4 && maca >= 4)}");
            Yaz("────────────────────────────────────────");

            var pas = new PickASlam { AktifMi = true };
            bool uygun = pas.UygunMu(durum);
            Yaz($"PickASlam.UygunMu → {uygun}");
            Yaz(!uygun
                ? "✅ DOĞRU (ne minör ne iki majör — tetiklenmedi)"
                : "❌ YANLIŞ (tetiklendi)");

            // Motor başka bir konvansiyona düşmeli, Pick-a-Slam 5NT vermemeli
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc != "5NT"
                ? "✅ MOTOR: 5NT DEĞİL (doğru)"
                : "❌ MOTOR: 5NT döndü — Pick-a-Slam yanlış tetiklendi!");

            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 41.x — Serious / Non-Serious 3NT
        // ═══════════════════════════════════════════════════════════════════

        private static OrtaklikAnlasmasi Serious3NTAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.Serious3NT = true;
            a.NonSerious3NT = false;
            return a;
        }

        private static OrtaklikAnlasmasi NonSerious3NTAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.Serious3NT = false;
            a.NonSerious3NT = true;
            return a;
        }

        public static void Test_Serious3NT_Pozitif()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 41.1 — Serious 3NT: Ciddi slam ilgisi");
            Yaz("Senaryo: 1♠ - 2♣ - 2♠ - 3♠ - ? (Kuzey)");
            Yaz("Kuzey'in eli: 5♠ + 19 HP + Aslar");
            Yaz("Beklenen: 3NT (Serious)");
            Yaz("────────────────────────────────────────");

            // ─── Kuzey'in eli: 5♠ + 19 HP (açıcı, ciddi slam ilgisi) ────
            var kuzeyEli = new List<Card>
    {
        // 5'li ♠
        new Card { Suit = "Maça",  Value = 14 }, // A = 4
        new Card { Suit = "Maça",  Value = 13 }, // K = 3
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa",  Value = 14 }, // A = 4
        new Card { Suit = "Kupa",  Value = 11 }, // J = 1
        new Card { Suit = "Kupa",  Value = 7 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 13 }, // K = 3
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q = 2
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+2+4+1+3+2 = 19 HP

            // ─── Geçmiş: 1♠ (K) - P - 2♣ (G) - P - 2♠ (K) - P - 3♠ (G) - P ─
            // Sıra Kuzey'de (açıcı). 3NT = Serious
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣",  Sira = 3, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 4, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "2♠",  Sira = 5, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 6, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "3♠",  Sira = 7, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 8, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = Serious3NTAnlasmasi(),
                Gecmis = gecmis,
            };

            // ─── Ön kontroller ─────────────────────────────────────────
            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"♠ uzunluk: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            Yaz($"Partner: {durum.Partner}");
            Yaz($"Rakipler: [{string.Join(", ", durum.Rakipler)}]");
            Yaz($"KendiTeklifleri: [{string.Join(", ", durum.KendiTeklifleri)}]");
            Yaz($"PartnerTeklifleri: [{string.Join(", ", durum.PartnerTeklifleri)}]");

            var sonGercek = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .LastOrDefault();
            Yaz($"Son gerçek teklif: {sonGercek?.Teklif} ({sonGercek?.Oyuncu})");
            Yaz("────────────────────────────────────────");

            // ─── Serious3NT'yi doğrudan test et ────────────────────────
            var s3nt = new Serious3NT { AktifMi = true };
            bool uygun = s3nt.UygunMu(durum);
            Yaz($"Serious3NT.UygunMu → {uygun}");
            if (uygun)
            {
                string teklif = s3nt.TeklifVer(durum);
                Yaz($"Serious3NT.TeklifVer → {teklif}");
                Yaz(teklif == "3NT" ? "✅ BEKLENEN: 3NT"
                                    : $"❌ BEKLENEN: 3NT, GELEN: {teklif}");
            }
            else
            {
                Yaz("❌ Serious3NT uygun değil! Bekleniyordu: uygun");
            }

            // ─── Motor üzerinden test ──────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");
            Yaz(motorTeklif == "3NT" ? "✅ MOTOR: 3NT"
                                     : $"❌ MOTOR: 3NT beklendi, gelen {motorTeklif}");
            Yaz("════════════════════════════════════════");
        }
        public static void Test_Serious3NT_Negatif_RakipMudahalesi()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 41.2 — Serious 3NT Negatif: Rakip müdahalesi");
            Yaz("Senaryo: 1♠ - (2♥) - 3♠ - Pas - ?");
            Yaz("Beklenen: Serious3NT tetiklenmez (3NT doğal olur)");
            Yaz("────────────────────────────────────────");

            // ─── Kuzey'in eli: 5♠ + 17 HP + ♥'de durdurucu ─────────────
            // Açıcı, 2♥ müdahalesine rağmen 3♠ fit gösterdi.
            // El güçlü ama rakip müdahale ettiği için Serious3NT devre dışı.
            var kuzeyEli = new List<Card>
    {
        // 5'li ♠
        new Card { Suit = "Maça",  Value = 14 }, // A = 4
        new Card { Suit = "Maça",  Value = 13 }, // K = 3
        new Card { Suit = "Maça",  Value = 12 }, // Q = 2
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 3 },
        // 3'lü ♥ (rakip rengi, durdurucu var: A)
        new Card { Suit = "Kupa",  Value = 14 }, // A = 4 (durdurucu)
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 4 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 13 }, // K = 3
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 5 },
        // 2'li ♣
        new Card { Suit = "Sinek", Value = 11 }, // J = 1
        new Card { Suit = "Sinek", Value = 6 },
    };
            // HCP: 4+3+2+4+3+1 = 17 HP

            // ─── Geçmiş: 1♠ (Kuzey) - 2♥ (Doğu, rakip) - 3♠ (Güney) - Pas (Batı) ─
            // Sıra Kuzey'de (açıcı). 3NT derse, bu doğal bir teklif olmalı,
            // Serious3NT tetiklenmemeli.
            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi
        {
            Oyuncu = Player.Kuzey,
            Teklif = "1♠",
            Sira = 1,
            GecerliMi = true
        },
        new IhaleHamlesi
        {
            Oyuncu = Player.Dogu,  // RAKİP
            Teklif = "2♥",
            Sira = 2,
            GecerliMi = true
        },
        new IhaleHamlesi
        {
            Oyuncu = Player.Guney, // PARTNER
            Teklif = "3♠",
            Sira = 3,
            GecerliMi = true
        },
        new IhaleHamlesi
        {
            Oyuncu = Player.Bati,  // RAKİP
            Teklif = "Pas",
            Sira = 4,
            GecerliMi = true
        },
    };

            // ─── Durum ─────────────────────────────────────────────────
            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = Serious3NTAnlasmasi(),
                Gecmis = gecmis,
            };

            // ─── Ön kontroller ─────────────────────────────────────────
            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"♠ uzunluk: {ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")}");
            Yaz($"Rakipler: [{string.Join(", ", durum.Rakipler)}]");
            Yaz($"Partner: {durum.Partner}");
            Yaz($"RakipTeklifleri: [{string.Join(", ", durum.RakipTeklifleri)}]");
            Yaz($"KendiTeklifleri: [{string.Join(", ", durum.KendiTeklifleri)}]");
            Yaz($"PartnerTeklifleri: [{string.Join(", ", durum.PartnerTeklifleri)}]");

            // Rakip müdahalesi tespiti
            bool rakipMudahalesiVar = durum.Gecmis.Any(h =>
                durum.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
            Yaz($"Rakip müdahalesi var mı: {rakipMudahalesiVar}");
            Yaz("────────────────────────────────────────");

            // ─── Serious3NT'yi doğrudan test et ────────────────────────
            var s3nt = new Serious3NT { AktifMi = true };

            bool uygun = s3nt.UygunMu(durum);
            Yaz($"Serious3NT.UygunMu → {uygun}");
            Yaz(!uygun
                ? "✅ BEKLENEN: False (rakip müdahalesi var)"
                : "❌ HATA: Serious3NT uygun olmamalıydı!");

            // ─── Motor üzerinden test ──────────────────────────────────
            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string motorTeklif = motor.TeklifVer(durum);
            Yaz($"IhaleMotoru.TeklifVer → {motorTeklif}");

            // Motor 3NT dönebilir ama bu DOĞAL 3NT olmalı (Serious değil).
            // Dolayısıyla biz sadece Serious3NT'nin tetiklenmediğini doğruluyoruz.
            // Motor başka bir konvansiyon (ör. CueBid, BasitCevap) dönmeli.
            Yaz(!uygun
                ? "✅ MOTOR: Serious3NT devre dışı (doğru)"
                : "❌ MOTOR: Serious3NT yanlış tetiklendi!");

            Yaz("════════════════════════════════════════");
        }
        // ═══════════════════════════════════════════════════════════════════
        // TEST 42.x — Multi 2♦ (Zayıf İki'ye alternatif)
        // ═══════════════════════════════════════════════════════════════════

        private static OrtaklikAnlasmasi Multi2DAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.Multi2D = true;
            a.ZayifIki = false;   // Multi, Zayıf İki'nin yerini alır
            return a;
        }

        private static int KartDegeri(char c)
        {
            switch (c)
            {
                case 'A': return 14;
                case 'K': return 13;
                case 'Q': return 12;
                case 'J': return 11;
                case 'T': return 10;
                default: return c - '0';
            }
        }

        /// <summary>El'i "AKJ953" gibi renk metinlerinden kurar (maça, kupa, karo, sinek).</summary>
        private static List<Card> ElMetinden(string maca, string kupa, string karo, string sinek)
        {
            var el = new List<Card>();
            foreach (char c in maca) el.Add(new Card { Suit = "Maça", Value = KartDegeri(c) });
            foreach (char c in kupa) el.Add(new Card { Suit = "Kupa", Value = KartDegeri(c) });
            foreach (char c in karo) el.Add(new Card { Suit = "Karo", Value = KartDegeri(c) });
            foreach (char c in sinek) el.Add(new Card { Suit = "Sinek", Value = KartDegeri(c) });
            return el;
        }

        private static IhaleHamlesi MultiHamle(Player oyuncu, string teklif, int sira)
        {
            return new IhaleHamlesi { Oyuncu = oyuncu, Teklif = teklif, Sira = sira, GecerliMi = true };
        }

        // Güney açıcı: 2♦ (Güney) - Pas (Batı) - cevap (Kuzey) - Pas (Doğu)
        private static List<IhaleHamlesi> MultiAciciGecmisi(string partnerCevabi)
        {
            return new List<IhaleHamlesi>
            {
                MultiHamle(Player.Guney, "2♦", 1),
                MultiHamle(Player.Bati, "Pas", 2),
                MultiHamle(Player.Kuzey, partnerCevabi, 3),
                MultiHamle(Player.Dogu, "Pas", 4),
            };
        }

        // Güney cevapçı: 2♦ (Kuzey) - Pas (Doğu)
        private static List<IhaleHamlesi> MultiCevapciGecmisi()
        {
            return new List<IhaleHamlesi>
            {
                MultiHamle(Player.Kuzey, "2♦", 1),
                MultiHamle(Player.Dogu, "Pas", 2),
            };
        }

        private static void Multi2DElBilgisi(List<Card> el)
        {
            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"Dağılım: ♠{ElDegerlendirici.RenkUzunlugu(el, "Maça")} " +
                $"♥{ElDegerlendirici.RenkUzunlugu(el, "Kupa")} " +
                $"♦{ElDegerlendirici.RenkUzunlugu(el, "Karo")} " +
                $"♣{ElDegerlendirici.RenkUzunlugu(el, "Sinek")}");
        }

        /// <summary>Pozitif test: hem Multi2D hem motor beklenen teklifi vermeli.</summary>
        private static void Multi2DKos(
            string baslik, string senaryo, string beklenen,
            List<Card> el, List<IhaleHamlesi> gecmis)
        {
            Yaz("════════════════════════════════════════");
            Yaz(baslik);
            Yaz($"Senaryo: {senaryo}");
            Yaz($"Beklenen: {beklenen}");
            Yaz("────────────────────────────────────────");

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = Multi2DAnlasmasi(),
                Gecmis = gecmis,
            };

            Multi2DElBilgisi(el);

            var multi = new Multi2D { AktifMi = true };
            bool uygun = multi.UygunMu(durum);
            Yaz($"Multi2D.UygunMu → {uygun}");
            if (uygun)
            {
                string t = multi.TeklifVer(durum);
                Yaz($"Multi2D.TeklifVer → {t}");
                Yaz(t == beklenen ? $"✅ BEKLENEN: {beklenen}"
                                  : $"❌ BEKLENEN: {beklenen}, GELEN: {t}");
            }
            else
            {
                Yaz("❌ Multi2D uygun değil! Bekleniyordu: uygun");
            }

            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string m = motor.TeklifVer(durum);
            Yaz($"Motor → {m}");
            Yaz(m == beklenen ? $"✅ MOTOR: {beklenen}"
                              : $"❌ MOTOR: {beklenen} beklendi, gelen {m}");
            Yaz("════════════════════════════════════════");
        }

        /// <summary>Negatif test: Multi2D tetiklenmemeli.</summary>
        private static void Multi2DNegatifKos(
            string baslik, string senaryo, List<Card> el, List<IhaleHamlesi> gecmis)
        {
            Yaz("════════════════════════════════════════");
            Yaz(baslik);
            Yaz($"Senaryo: {senaryo}");
            Yaz("Beklenen: Multi2D tetiklenmez");
            Yaz("────────────────────────────────────────");

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = Multi2DAnlasmasi(),
                Gecmis = gecmis,
            };

            Multi2DElBilgisi(el);

            var multi = new Multi2D { AktifMi = true };
            bool uygun = multi.UygunMu(durum);
            Yaz($"Multi2D.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (tetiklenmedi)" : "❌ YANLIŞ (Multi2D tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        // ─── AÇILIŞ ──────────────────────────────────────────────────────

        public static void Test_Multi2D_Acilis_Maca()
        {
            Multi2DKos("TEST 42.1 — Multi 2♦ Açılış (6'lı ♠, 9 HP)",
                "Açılış (Multi anlaşması açık)", "2♦",
                ElMetinden("KJ9753", "Q84", "K72", "6"),
                new List<IhaleHamlesi>());
        }

        public static void Test_Multi2D_Acilis_Kupa()
        {
            Multi2DKos("TEST 42.2 — Multi 2♦ Açılış (6'lı ♥, 7 HP)",
                "Açılış (Multi anlaşması açık)", "2♦",
                ElMetinden("Q84", "AJ9753", "962", "7"),
                new List<IhaleHamlesi>());
        }

        public static void Test_Multi2D_Kapali_ZayifIkiKorunur()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 42.3 — Multi kapalıyken (varsayılan) Zayıf İki aynen çalışır");
            Yaz("Beklenen: 2♠ (Zayıf İki), 2♦ DEĞİL");
            Yaz("────────────────────────────────────────");

            var el = ElMetinden("AQ9753", "T64", "Q8", "95");   // Test 32.1 eli
            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = new List<IhaleHamlesi>(),
            };

            Multi2DElBilgisi(el);
            var motor = new IhaleMotoru(durum.Anlasma);
            string m = motor.TeklifVer(durum);
            Yaz($"Motor → {m}");
            Yaz(m == "2♠" ? "✅ DOĞRU (Multi kapalı, Zayıf İki 2♠ verdi)"
                          : $"❌ YANLIŞ (beklenen 2♠, gelen {m})");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_Multi2D_Negatif_BesliMajor()
        {
            Multi2DNegatifKos("TEST 42.4 — Multi Negatif: 5'li majör (6 lazım)",
                "Açılış", ElMetinden("KJ975", "Q84", "K72", "63"),
                new List<IhaleHamlesi>());
        }

        public static void Test_Multi2D_Negatif_GucluEl()
        {
            Multi2DNegatifKos("TEST 42.5 — Multi Negatif: 15 HP (çok güçlü)",
                "Açılış", ElMetinden("AKQ953", "KJ4", "Q72", "6"),
                new List<IhaleHamlesi>());
        }

        public static void Test_Multi2D_Negatif_DigerMajor4()
        {
            Multi2DNegatifKos("TEST 42.6 — Multi Negatif: 6♠ + 4'lü ♥",
                "Açılış", ElMetinden("KJ9753", "Q842", "73", "6"),
                new List<IhaleHamlesi>());
        }

        // ─── CEVAPÇI ─────────────────────────────────────────────────────

        public static void Test_Multi2D_Cevap_Kupa()
        {
            Multi2DKos("TEST 42.7 — Multi Cevap: 2♥ (pas ya da düzelt, ♥ ≥ ♠)",
                "(Kuzey 2♦) - Pas - ?", "2♥",
                ElMetinden("84", "KJ95", "Q73", "9742"),
                MultiCevapciGecmisi());
        }

        public static void Test_Multi2D_Cevap_Maca()
        {
            Multi2DKos("TEST 42.8 — Multi Cevap: 2♠ (pas ya da düzelt, ♠ > ♥)",
                "(Kuzey 2♦) - Pas - ?", "2♠",
                ElMetinden("KJ975", "83", "962", "874"),
                MultiCevapciGecmisi());
        }

        public static void Test_Multi2D_Cevap_Soru2NT()
        {
            Multi2DKos("TEST 42.9 — Multi Cevap: 2NT (12 HP, soru)",
                "(Kuzey 2♦) - Pas - ?", "2NT",
                ElMetinden("AQ4", "KJ5", "Q73", "8742"),
                MultiCevapciGecmisi());
        }

        // ─── AÇICI REBID ─────────────────────────────────────────────────

        public static void Test_Multi2D_Acici_Kupa_Pas()
        {
            Multi2DKos("TEST 42.10 — Multi Açıcı: 2♦ - 2♥ → ♥ var → Pas",
                "2♦ - Pas - 2♥ - Pas - ?", "Pas",
                ElMetinden("Q84", "AJ9753", "962", "7"),
                MultiAciciGecmisi("2♥"));
        }

        public static void Test_Multi2D_Acici_Maca_Duzelt()
        {
            Multi2DKos("TEST 42.11 — Multi Açıcı: 2♦ - 2♥ → ♠ var → 2♠ (düzelt)",
                "2♦ - Pas - 2♥ - Pas - ?", "2♠",
                ElMetinden("AQ9753", "T64", "Q8", "95"),
                MultiAciciGecmisi("2♥"));
        }

        public static void Test_Multi2D_Acici_Kupa_Duzelt()
        {
            Multi2DKos("TEST 42.12 — Multi Açıcı: 2♦ - 2♠ → ♥ var → 3♥ (düzelt)",
                "2♦ - Pas - 2♠ - Pas - ?", "3♥",
                ElMetinden("Q84", "AJ9753", "962", "7"),
                MultiAciciGecmisi("2♠"));
        }

        public static void Test_Multi2D_Acici_Maca_Pas()
        {
            Multi2DKos("TEST 42.13 — Multi Açıcı: 2♦ - 2♠ → ♠ var → Pas",
                "2♦ - Pas - 2♠ - Pas - ?", "Pas",
                ElMetinden("AQ9753", "T64", "Q8", "95"),
                MultiAciciGecmisi("2♠"));
        }

        public static void Test_Multi2D_Acici_2NT_MinimumMaca()
        {
            Multi2DKos("TEST 42.14 — Multi Açıcı: 2♦ - 2NT → minimum ♠ (5 HP) → 3♦",
                "2♦ - Pas - 2NT - Pas - ?", "3♦",
                ElMetinden("Q97532", "K84", "962", "7"),
                MultiAciciGecmisi("2NT"));
        }

        public static void Test_Multi2D_Acici_2NT_MaksimumKupa()
        {
            Multi2DKos("TEST 42.15 — Multi Açıcı: 2♦ - 2NT → maksimum ♥ (10 HP) → 3♥",
                "2♦ - Pas - 2NT - Pas - ?", "3♥",
                ElMetinden("Q84", "AKJ753", "962", "7"),
                MultiAciciGecmisi("2NT"));
        }

        // ─── NEGATİFLER ──────────────────────────────────────────────────

        public static void Test_Multi2D_Negatif_RakipMudahalesi()
        {
            Multi2DNegatifKos("TEST 42.16 — Multi Negatif: partner 2♦, rakip 3♣ müdahale etti",
                "(Kuzey 2♦) - (Doğu 3♣) - ?",
                ElMetinden("84", "KJ95", "Q73", "9742"),
                new List<IhaleHamlesi>
                {
                    MultiHamle(Player.Kuzey, "2♦", 1),
                    MultiHamle(Player.Dogu, "3♣", 2),
                });
        }

        public static void Test_Multi2D_Negatif_RakipActi()
        {
            Multi2DNegatifKos("TEST 42.17 — Multi Negatif: 2♦'yi RAKİP açtı",
                "Pas - (Doğu 2♦) - ?",
                ElMetinden("84", "KJ95", "Q73", "9742"),
                new List<IhaleHamlesi>
                {
                    MultiHamle(Player.Kuzey, "Pas", 1),
                    MultiHamle(Player.Dogu, "2♦", 2),
                });
        }
        // ═══════════════════════════════════════════════════════════════════
        // TEST 43.x — Takeout Double
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_TakeoutDouble_Kontra_Maca()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.1 — Takeout Double Kontra (rakip 1♠ açtı)");
            Yaz("Beklenen: Dbl");
            Yaz("────────────────────────────────────────");

            // 12 HP, 1'li ♠ (kısalık), diğer renklerde 4-4-4
            var el = new List<Card>
    {
        // 1'li ♠
        new Card { Suit = "Maça",  Value = 5 },
        // 4'lü ♥ (K)
        new Card { Suit = "Kupa",  Value = 13 },
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        // 4'lü ♦ (A)
        new Card { Suit = "Karo",  Value = 14 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Karo",  Value = 2 },
        // 4'lü ♣ (A Q)
        new Card { Suit = "Sinek", Value = 14 },
        new Card { Suit = "Sinek", Value = 12 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3 + 4 + 4+2 = 13 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1♠", Sira = 1, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(el, "Maça")}, " +
                $"♥: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")}, " +
                $"♦: {ElDegerlendirici.RenkUzunlugu(el, "Karo")}, " +
                $"♣: {ElDegerlendirici.RenkUzunlugu(el, "Sinek")}");

            var td = new TakeoutDouble { AktifMi = true };
            Yaz($"TakeoutDouble.UygunMu → {td.UygunMu(durum)}");
            if (td.UygunMu(durum))
                Yaz($"TakeoutDouble.TeklifVer → {td.TeklifVer(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Dbl" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Dbl)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Kontra_Kupa()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.2 — Takeout Double Kontra (rakip 1♥ açtı)");
            Yaz("Beklenen: Dbl");
            Yaz("────────────────────────────────────────");

            // 1'li ♥ (kısalık), 4-4-4 dağılım, 13 HP
            var el = new List<Card>
    {
        // 4'lü ♠ (A Q)
        new Card { Suit = "Maça",  Value = 14 },
        new Card { Suit = "Maça",  Value = 12 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Maça",  Value = 3 },
        // 1'li ♥
        new Card { Suit = "Kupa",  Value = 5 },
        // 4'lü ♦ (K J)
        new Card { Suit = "Karo",  Value = 13 },
        new Card { Suit = "Karo",  Value = 11 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 2 },
        // 4'lü ♣ (K)
        new Card { Suit = "Sinek", Value = 13 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 4+2+3+1+3 = 13 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1♥", Sira = 1, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")} (kısa olmalı)");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Dbl" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Dbl)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Kontra_KisaRakipRengi()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.3 — Takeout Dbl: rakip renginde 0 kart (şikan)");
            Yaz("Beklenen: Dbl (5'li yan renk olsa bile)");
            Yaz("────────────────────────────────────────");

            // Rakip 1♠ açtı, bizim elimizde ♠ YOK (void), 5'li ♥ var, 12 HP
            var el = new List<Card>
    {
        // 0 ♠ (şikan)
        // 5'li ♥
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        // 4'lü ♦
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Karo",  Value = 2 },
        // 4'lü ♣
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3+2+4 = 9 → 12'ye çıkarmak için biraz değiştirelim
            el = new List<Card>
    {
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 12 }, // Q
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Karo",  Value = 2 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 4+3+4+2 = 13 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1♠", Sira = 1, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(el, "Maça")} (şikan)");
            Yaz($"♥: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")} (5'li)");

            var td = new TakeoutDouble { AktifMi = true };
            Yaz($"TakeoutDouble.UygunMu → {td.UygunMu(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Dbl" ? "✅ DOĞRU (şikan varsa 5'li yan renk olsa bile Dbl)"
                               : $"❌ YANLIŞ (beklenen Dbl)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Kontra_Guclu()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.4 — Takeout Double: 17+ HP güçlü el");
            Yaz("Beklenen: Dbl (5'li yan renk olsa bile)");
            Yaz("────────────────────────────────────────");

            // 17 HP, 5'li ♥, rakip 1♠ açtı, ♠ 2'li
            var el = new List<Card>
    {
        // 2'li ♠
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 3 },
        // 5'li ♥
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 2 },
    };
            // HCP: 4+3+2+4+2 = 15 → 17 için artır
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 2 },
    };
            // HCP: 4+3+2+4+3+2 = 18 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1♠", Sira = 1, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Dbl" ? "✅ DOĞRU (17+ güçlü, 5'li renk olsa da Dbl)"
                               : $"❌ YANLIŞ (beklenen Dbl)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Kontra_RakipKaro()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.5 — Takeout Double Kontra (rakip 1♦ açtı)");
            Yaz("Beklenen: Dbl (her iki majör destekli)");
            Yaz("────────────────────────────────────────");

            // 4'lü ♠ + 4'lü ♥, 13 HP, 2'li ♦
            var el = new List<Card>
    {
        // 4'lü ♠ (K Q)
        new Card { Suit = "Maça",  Value = 13 },
        new Card { Suit = "Maça",  Value = 12 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },
        // 4'lü ♥ (A)
        new Card { Suit = "Kupa",  Value = 14 },
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 2 },
        // 2'li ♦
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 4 },
        // 3'lü ♣ (A)
        new Card { Suit = "Sinek", Value = 14 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3+2+4+4 = 13 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1♦", Sira = 1, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♦: {ElDegerlendirici.RenkUzunlugu(el, "Karo")} (kısa olmalı)");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Dbl" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen Dbl)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // KONTRA NEGATİFLERİ
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_TakeoutDouble_Negatif_YetersizHP()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.6 — Takeout Dbl Negatif: 9 HP (yetersiz)");
            Yaz("Beklenen: TakeoutDouble tetiklenmez");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Karo",  Value = 11 }, // J
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Karo",  Value = 3 },
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 2 },
    };
            // HCP: 2+1 = 3... daha yükseğe çıkaralım
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Karo",  Value = 12 }, // Q
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Karo",  Value = 3 },
        new Card { Suit = "Sinek", Value = 11 }, // J
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 2 },
    };
            // HCP: 3+2+1 = 6... 9 için:
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Karo",  Value = 12 }, // Q
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 5 },
        new Card { Suit = "Karo",  Value = 3 },
        new Card { Suit = "Sinek", Value = 13 }, // K
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 5 },
        new Card { Suit = "Sinek", Value = 2 },
    };
            // HCP: 4+2+3 = 9 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1♠", Sira = 1, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");

            var td = new TakeoutDouble { AktifMi = true };
            bool uygun = td.UygunMu(durum);
            Yaz($"TakeoutDouble.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (yetersiz HP)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Negatif_RakipRengiUzun()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.7 — Takeout Dbl Negatif: rakip renginde 3+ kart");
            Yaz("Beklenen: TakeoutDouble tetiklenmez");
            Yaz("────────────────────────────────────────");

            // Rakip 1♠ açtı, bizim elimizde 4'lü ♠ var → Takeout Dbl uygun değil
            var el = new List<Card>
    {
        // 4'lü ♠
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 4 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 12 }, // Q
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 5 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 11 }, // J
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 6 },
    };
            // HCP: 4+3+2+1 = 10

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1♠", Sira = 1, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(el, "Maça")} (3+ ise Dbl olmaz)");

            var td = new TakeoutDouble { AktifMi = true };
            bool uygun = td.UygunMu(durum);
            Yaz($"TakeoutDouble.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (rakip rengi uzun)" : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Negatif_5liYanRenk()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.8 — Takeout Dbl Negatif: 5'li yan renk + orta HP");
            Yaz("Beklenen: TakeoutDouble tetiklenmez (doğal müdahale)");
            Yaz("────────────────────────────────────────");

            // 5'li ♥ + 13 HP, rakip 1♠ açtı, ♠ 2'li
            var el = new List<Card>
    {
        // 2'li ♠
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 4 },
        // 5'li ♥
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 2 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 11 }, // J
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+2+3+1 = 10 → 13 yapalım
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 2 },
        new Card { Suit = "Sinek", Value = 11 }, // J
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+3+1 = 11... 13 için:
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 2 },
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+3+2 = 12 → 13 için biraz daha
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 2 },
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+4+2 = 13 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati, Teklif = "1♠", Sira = 1, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")} (5'li)");

            var td = new TakeoutDouble { AktifMi = true };
            bool uygun = td.UygunMu(durum);
            Yaz($"TakeoutDouble.UygunMu → {uygun}");
            Yaz(!uygun ? "✅ DOĞRU (doğal müdahale tercih edilir)"
                       : "❌ YANLIŞ (tetiklendi)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAP: 3NT
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_TakeoutDouble_Cevap_3NT()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.9 — Takeout Dbl Cevap: 3NT (13+ HP, dengeli, durdurucu)");
            Yaz("Senaryo: 1♠ - Dbl - Pas - ?");
            Yaz("Beklenen: 3NT");
            Yaz("────────────────────────────────────────");

            // 13 HP dengeli, 4'lü ♠ durdurucu, yan majör yok
            var el = new List<Card>
    {
        // 4'lü ♠ (A K)
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 13 }, // K
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Maça",  Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 12 }, // Q
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 11 }, // J
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+2+1 = 10 → 13 için artır
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 13 }, // K
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 12 }, // Q
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 11 }, // J
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+2+2+1 = 12 → 13 için:
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 13 }, // K
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 11 }, // J
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // K
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+3+2+1+3 = 13 ✓, dengeli 4-3-3-3

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"Dengeli: {ElDegerlendirici.DengeliEl(el)}");
            Yaz($"♠ durdurucu: {ElDegerlendirici.StopperVar(el, "Maça")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 3NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Cevap_3NT_YanMajorVar()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.10 — Takeout Cevap: 3NT DEĞİL (4'lü yan majör var)");
            Yaz("Beklenen: 2♠ cue-bid (12+)");
            Yaz("────────────────────────────────────────");

            // 4'lü ♥ var → 3NT yerine cue-bid
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 13 }, // K
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 10 },
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Karo",  Value = 12 }, // Q
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 9 },
    };
            // HCP: 4+3+2+2 = 11 → 12 için:
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 13 }, // K
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 11 }, // J
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Karo",  Value = 12 }, // Q
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // K
        new Card { Suit = "Sinek", Value = 9 },
    };
            // HCP: 4+3+1+2+3 = 13 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")} (4'lü)");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♠" ? "✅ DOĞRU (cue-bid)"
                              : $"❌ YANLIŞ (beklenen 2♠)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAP: CUE-BID (12+ HP)
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_TakeoutDouble_Cevap_CueBid()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.11 — Takeout Cevap: Cue-bid (12+ HP)");
            Yaz("Senaryo: 1♥ - Dbl - Pas - ?");
            Yaz("Beklenen: 2♥ (cue-bid)");
            Yaz("────────────────────────────────────────");

            // 12 HP, dengeli değil, 4'lü ♠ yok
            var el = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça",  Value = 11 }, // J
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        // 4'lü ♥ (rakip rengi)
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 6 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 13 }, // K
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 1+4+3+3 = 11 → 12 için biraz
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 12 }, // Q
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 4 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Sinek", Value = 13 }, // K
        new Card { Suit = "Sinek", Value = 10 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 2+4+3+3 = 12 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♥",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥ (rakip rengi): {ElDegerlendirici.RenkUzunlugu(el, "Kupa")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU (cue-bid)"
                              : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Cevap_CueBid_15HP()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.12 — Takeout Cevap: Cue-bid (15+ HP)");
            Yaz("Senaryo: 1♦ - Dbl - Pas - ?");
            Yaz("Beklenen: 2♦ (cue-bid)");
            Yaz("────────────────────────────────────────");

            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 2 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 11 }, // J
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // K
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+4+1+3+2 = 14 → 15 için biraz
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 2 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 11 }, // J
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // K
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+4+1+3+2 = 14... hmm
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 8 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 2 },
        new Card { Suit = "Kupa",  Value = 14 }, // A
        new Card { Suit = "Kupa",  Value = 11 }, // J
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // K
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 4+4+1+3+3+2 = 17 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♦",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♦" ? "✅ DOĞRU (cue-bid)"
                              : $"❌ YANLIŞ (beklenen 2♦)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAP: DAVET (9-11 HP)
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_TakeoutDouble_Cevap_Davet()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.13 — Takeout Cevap: Davet (9-11 HP, atlayarak)");
            Yaz("Senaryo: 1♠ - Dbl - Pas - ?");
            Yaz("Beklenen: 3♥ (davet)");
            Yaz("────────────────────────────────────────");

            // 10 HP, 4'lü ♥, atlayarak davet
            var el = new List<Card>
    {
        // 3'lü ♠
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },
        // 4'lü ♥ (davet edilecek)
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 2 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 12 }, // Q
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 11 }, // J
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3+2+1 = 6... 10 için:
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3+3+2 = 8... 
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 10 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 2 },
        new Card { Suit = "Karo",  Value = 14 }, // A
        new Card { Suit = "Karo",  Value = 8 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 12 }, // Q
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3+4+2 = 9 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "3♥" ? "✅ DOĞRU (davet)"
                              : $"❌ YANLIŞ (beklenen 3♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Cevap_Davet_2NT()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.14 — Takeout Cevap: 2NT davet (9-11 HP dengeli)");
            Yaz("Senaryo: 1♠ - Dbl - Pas - ?");
            Yaz("Beklenen: 2NT");
            Yaz("────────────────────────────────────────");

            // 10 HP dengeli, ♠ durdurucu var, yan majör yok
            var el = new List<Card>
    {
        // 4'lü ♠ (Q J)
        new Card { Suit = "Maça",  Value = 12 }, // Q
        new Card { Suit = "Maça",  Value = 11 }, // J
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Maça",  Value = 3 },
        // 3'lü ♥
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        // 3'lü ♦
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        // 3'lü ♣
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 2+1+3 = 6... 
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 12 }, // Q
        new Card { Suit = "Maça",  Value = 11 }, // J
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 13 }, // K
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 13 }, // K
        new Card { Suit = "Sinek", Value = 7 },
        new Card { Suit = "Sinek", Value = 4 },
    };
            // HCP: 2+1+3+3 = 9 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"Dengeli: {ElDegerlendirici.DengeliEl(el)}");
            Yaz($"♠ durdurucu: {ElDegerlendirici.StopperVar(el, "Maça")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 2NT)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Cevap_1NT()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.15 — Takeout Cevap: 1NT (6-8 HP dengeli + durdurucu)");
            Yaz("Senaryo: 1♠ - Dbl - Pas - ?");
            Yaz("Beklenen: 1NT");
            Yaz("────────────────────────────────────────");

            // 7 HP dengeli, ♠ durdurucu
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 12 }, // Q
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 11 }, // J
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 2+1 = 3... yükseltelim
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 }, // K
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 6 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 11 }, // J
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3+2+1 = 6 ✓

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"Dengeli: {ElDegerlendirici.DengeliEl(el)}");
            Yaz($"♠ durdurucu: {ElDegerlendirici.StopperVar(el, "Maça")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "1NT" ? "✅ DOĞRU" : $"❌ YANLIŞ (beklenen 1NT)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAP: ZAYIF (0-8 HP)
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_TakeoutDouble_Cevap_Zayif_EnIyiRenk()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.16 — Takeout Cevap: Zayıf (4 HP) → en iyi renk");
            Yaz("Senaryo: 1♠ - Dbl - Pas - ?");
            Yaz("Beklenen: 2♥ (en iyi yan majör)");
            Yaz("────────────────────────────────────────");

            // 4 HP, 5'li ♥ → en iyi renk ♥, 2 seviyesinde
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 11 }, // J
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 3 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 1 = 1... 4 için:
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 12 }, // Q
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 11 }, // J
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 3 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 2+1 = 3... 4-5 arası iyi
            el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 13 }, // K
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 7 },
        new Card { Suit = "Kupa",  Value = 5 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 10 },
        new Card { Suit = "Karo",  Value = 6 },
        new Card { Suit = "Karo",  Value = 3 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
        new Card { Suit = "Sinek", Value = 3 },
    };
            // HCP: 3 = 3, biraz düşük ama test için uygun (0-8 aralığında)

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♥: {ElDegerlendirici.RenkUzunlugu(el, "Kupa")} (5'li)");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "2♥" ? "✅ DOĞRU (en iyi yan majör)"
                              : $"❌ YANLIŞ (beklenen 2♥)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Cevap_Zayif_EnUcuzSeviye()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.17 — Takeout Cevap: Zayıf, en ucuz seviye");
            Yaz("Senaryo: 1♥ - Dbl - Pas - ?");
            Yaz("Beklenen: 1♠ (en ucuz, çünkü ♠ > ♥ sıralı)");
            Yaz("────────────────────────────────────────");

            // 5 HP, 5'li ♠, rakip 1♥ → ♠ 1 seviyesinde (Sira(♠) > Sira(♥))
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 13 }, // K
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 8 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 8 },
        new Card { Suit = "Sinek", Value = 5 },
    };
            // HCP: 3 = 3

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♥",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(el, "Maça")} (5'li)");
            Yaz($"♥ (rakip): {ElDegerlendirici.RenkUzunlugu(el, "Kupa")}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "1♠" ? "✅ DOĞRU (1 seviyesi, çünkü ♠ > ♥)"
                              : $"❌ YANLIŞ (beklenen 1♠)");
            Yaz("════════════════════════════════════════");
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAP: CEZA PASI
        // ═══════════════════════════════════════════════════════════════════

        public static void Test_TakeoutDouble_Cevap_CezaPasi()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.18 — Takeout Cevap: Ceza pası (5+ ♠ + 3 üst onör)");
            Yaz("Senaryo: 1♠ - Dbl - Pas - ?");
            Yaz("Beklenen: Pas (ceza)");
            Yaz("────────────────────────────────────────");

            // 5'li ♠, 3 üst onör (A K Q), ceza pası
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 14 }, // A
        new Card { Suit = "Maça",  Value = 13 }, // K
        new Card { Suit = "Maça",  Value = 12 }, // Q
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
    };
            // HCP: 4+3+2 = 9

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"♠: {ElDegerlendirici.RenkUzunlugu(el, "Maça")} (5+)");
            Yaz($"♠ üst onör sayısı (A K Q J 10): " +
                $"{el.Count(c => c.Suit == "Maça" && c.Value >= 10)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Pas" ? "✅ DOĞRU (ceza pası)"
                               : $"❌ YANLIŞ (beklenen Pas)");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_TakeoutDouble_Cevap_CezaPasi_YetersizOnör()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 43.19 — Takeout Cevap: 5'li ♠ ama sadece 1 üst onör");
            Yaz("Beklenen: Pas DEĞİL (cevap vermeli)");
            Yaz("────────────────────────────────────────");

            // 5'li ♠, sadece 1 üst onör (Q) → ceza pası değil, normal cevap
            var el = new List<Card>
    {
        new Card { Suit = "Maça",  Value = 12 }, // Q
        new Card { Suit = "Maça",  Value = 9 },
        new Card { Suit = "Maça",  Value = 7 },
        new Card { Suit = "Maça",  Value = 5 },
        new Card { Suit = "Maça",  Value = 3 },
        new Card { Suit = "Kupa",  Value = 9 },
        new Card { Suit = "Kupa",  Value = 6 },
        new Card { Suit = "Kupa",  Value = 3 },
        new Card { Suit = "Karo",  Value = 9 },
        new Card { Suit = "Karo",  Value = 7 },
        new Card { Suit = "Karo",  Value = 4 },
        new Card { Suit = "Sinek", Value = 9 },
        new Card { Suit = "Sinek", Value = 6 },
    };
            // HCP: 2 = 2

            var gecmis = new List<IhaleHamlesi>
    {
        new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "1♠",  Sira = 1, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Dbl", Sira = 2, GecerliMi = true },
        new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas", Sira = 3, GecerliMi = true },
    };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"♠ üst onör sayısı: " +
                $"{el.Count(c => c.Suit == "Maça" && c.Value >= 10)} (yetersiz)");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc != "Pas" ? "✅ DOĞRU (ceza pası değil)"
                               : "❌ YANLIŞ (Pas döndü, cevap vermeli)");
            Yaz("════════════════════════════════════════");
        }

        /// <summary>
        /// Tüm testleri sırayla çalıştırır.
        /// </summary>

        /// <summary>
        /// Regresyon: 6 HP + 4-4 majör, balancing seat → Pas (eski hatalı 2♥ senaryosu).
        /// Auction: Batı Pas / Kuzey Pas / Doğu 1NT / Güney Pas / Batı Pas / Kuzey ?
        /// </summary>
        public static void Test_Dont_Balancing_ZayifEl_Pas()
        {
            Yaz("════════════════════════════════════════");
            Yaz("TEST 26.7 — DONT: Balancing 6 HP → Pas (regresyon)");
            Yaz("Senaryo: Pas - Pas - 1NT - Pas - Pas - ?");
            Yaz("Beklenen: Pas (6 HP, eşiğin altında)");
            Yaz("────────────────────────────────────────");

            // Orijinal hata eli: ♠10853 ♥QJ83 ♦1093 ♣K6 ≈ 6 HP, 4-4 majör
            var kuzeyEli = new List<Card>
            {
                new Card { Suit = "Maça",  Value = 10 },
                new Card { Suit = "Maça",  Value = 8 },
                new Card { Suit = "Maça",  Value = 5 },
                new Card { Suit = "Maça",  Value = 3 },
                new Card { Suit = "Kupa",  Value = 12 }, // Q = 2
                new Card { Suit = "Kupa",  Value = 11 }, // J = 1
                new Card { Suit = "Kupa",  Value = 8 },
                new Card { Suit = "Kupa",  Value = 3 },
                new Card { Suit = "Karo",  Value = 10 },
                new Card { Suit = "Karo",  Value = 9 },
                new Card { Suit = "Karo",  Value = 3 },
                new Card { Suit = "Sinek", Value = 13 }, // K = 3
                new Card { Suit = "Sinek", Value = 6 },
            };
            // HCP: 2+1+3 = 6

            var gecmis = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "Pas", Sira = 2, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "1NT", Sira = 3, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "Pas", Sira = 4, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Bati,  Teklif = "Pas", Sira = 5, GecerliMi = true },
            };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = kuzeyEli,
                Anlasma = OrtaklikAnlasmasi.Varsayilan(),
                Gecmis = gecmis,
            };

            Yaz($"HCP: {ElDegerlendirici.HCP(kuzeyEli)}");
            Yaz($"UstUstePasSayisi: {durum.UstUstePasSayisi} (balancing={durum.UstUstePasSayisi >= 2})");
            Yaz($"♠{ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Maça")} ♥{ElDegerlendirici.RenkUzunlugu(kuzeyEli, "Kupa")}");

            var dont = new Dont();
            Yaz($"Dont.UygunMu → {dont.UygunMu(durum)}");

            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == "Pas" ? "✅ DOĞRU (zayıf balancing eli Pas)" : $"❌ YANLIŞ (beklenen Pas, gelen {sonuc})");
            Yaz("════════════════════════════════════════");
        }
        // ═══════════════════════════════════════════════════════════════════
        // TEST 44.x — 1♣ Cevap Sistemi
        // ═══════════════════════════════════════════════════════════════════

        private static OrtaklikAnlasmasi BirClubAnlasmasi()
        {
            var a = OrtaklikAnlasmasi.Varsayilan();
            a.OneClub_AnswerSistemi = true;
            return a;
        }

        private static void TestBirClub(string baslik, string senaryo, string beklenen,
            List<Card> el, bool zonNS = false, bool zonEW = false, string partnerTeklif = "1♣")
        {
            Yaz("════════════════════════════════════════");
            Yaz(baslik);
            Yaz($"Senaryo: {senaryo}");
            Yaz($"Beklenen: {beklenen}");
            Yaz("────────────────────────────────────────");

            var gecmis = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = partnerTeklif, Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Dogu,  Teklif = "Pas",        Sira = 2, GecerliMi = true },
            };

            var durum = new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = BirClubAnlasmasi(),
                Gecmis = gecmis,
                ZonNS = zonNS,
                ZonEW = zonEW,
            };

            int hcp = ElDegerlendirici.HCP(el);
            Yaz($"HCP: {hcp}");
            Yaz($"Dengeli: {ElDegerlendirici.DengeliEl(el)}");
            Yaz($"Dağılım: ♠{ElDegerlendirici.RenkUzunlugu(el, "Maça")} " +
                $"♥{ElDegerlendirici.RenkUzunlugu(el, "Kupa")} " +
                $"♦{ElDegerlendirici.RenkUzunlugu(el, "Karo")} " +
                $"♣{ElDegerlendirici.RenkUzunlugu(el, "Sinek")}");
            Yaz($"Zon: NS={zonNS}, EW={zonEW}");
            Yaz("────────────────────────────────────────");


            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == beklenen ? $"✅ DOĞRU ({beklenen})"
                                  : $"❌ YANLIŞ (beklenen {beklenen}, gelen {sonuc})");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_BirClub_FG_Relay()
        {
            // ♠KQxx ♥Axx ♦Kxx ♣Jxx → 13 HCP dengeli → 2♣
            var el = ElMetinden("KQ76", "A54", "K32", "J85");
            TestBirClub("TEST 44.1 — 1♣ Cevap: 13 HCP dengeli → 2♣",
                "1♣ - Pas - ?", "2♣", el);
        }

        public static void Test_BirClub_Major()
        {
            // ♠xx ♥KJxxx ♦Qxx ♣xxx → 6 HCP, 5♥ → 1♥
            var el = ElMetinden("85", "KJ432", "Q65", "863");
            TestBirClub("TEST 44.2 — 1♣ Cevap: 6 HCP 5♥ → 1♥",
                "1♣ - Pas - ?", "1♥", el);
        }

        public static void Test_BirClub_Preempt_NV()
        {
            // ♠xx ♥xx ♦KQJxxx ♣xxx → 6 HCP, 6♦, NV → 3♦
            var el = ElMetinden("85", "73", "KQJ654", "832");
            TestBirClub("TEST 44.3 — 1♣ Cevap: NV 6♦ → 3♦",
                "1♣ - Pas - ? (NV)", "3♦", el, zonNS: false, zonEW: false);
        }

        public static void Test_BirClub_Preempt_V()
        {
            // Aynı el, V zonda → iyi renk kontrolü (KQJ var, 2 onör) → 3♦
            var el = ElMetinden("85", "73", "KQJ654", "832");
            TestBirClub("TEST 44.4 — 1♣ Cevap: V 6♦ iyi renk → 3♦",
                "1♣ - Pas - ? (V)", "3♦", el, zonNS: true, zonEW: true);
        }

        public static void Test_BirClub_TersFlannery()
        {
            // ♠KJ543 ♥KJ62 ♦85 ♣73 → 7 HCP, 5♠ 4♥
            var el = ElMetinden("KJ543", "KJ62", "85", "73");
            TestBirClub("TEST 44.5 — 1♣ Cevap: 7 HCP 5♠4♥ → 2♥",
                "1♣ - Pas - ?", "2♥", el);
        }

        public static void Test_BirClub_1NT()
        {
            // ♠Qxx ♥Jxx ♦Kxx ♣Jxxx → 8 HCP, dengeli, majör yok → 1NT
            var el = ElMetinden("Q85", "J73", "K62", "J954");
            TestBirClub("TEST 44.6 — 1♣ Cevap: 8 HCP dengeli → 1NT",
                "1♣ - Pas - ?", "1NT", el);
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEST 45.x — 1♣ ⇒ 1♦ Gelişimi (Aşama 1: Açıcı Rebid)
        // ═══════════════════════════════════════════════════════════════════

        private static IhaleDurumu BirKaroGelisimDurumu(List<Card> el)
        {
            var gecmis = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♦", Sira = 2, GecerliMi = true },
            };
            return new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = BirClubAnlasmasi(),
                Gecmis = gecmis,
            };
        }

        private static void TestBirClubBirKaro(string baslik, string senaryo, string beklenen, List<Card> el)
        {
            Yaz("════════════════════════════════════════");
            Yaz(baslik);
            Yaz($"Senaryo: {senaryo}");
            Yaz($"Beklenen: {beklenen}");
            Yaz("────────────────────────────────────────");
            var durum = BirKaroGelisimDurumu(el);
            Yaz($"HCP: {ElDegerlendirici.HCP(el)}");
            Yaz($"Dengeli: {ElDegerlendirici.DengeliEl(el)}");
            Yaz($"Dağılım: ♠{ElDegerlendirici.RenkUzunlugu(el, "Maça")} " +
                $"♥{ElDegerlendirici.RenkUzunlugu(el, "Kupa")} " +
                $"♦{ElDegerlendirici.RenkUzunlugu(el, "Karo")} " +
                $"♣{ElDegerlendirici.RenkUzunlugu(el, "Sinek")}");
            Yaz("────────────────────────────────────────");


            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == beklenen ? $"✅ DOĞRU ({beklenen})"
                                  : $"❌ YANLIŞ (beklenen {beklenen}, gelen {sonuc})");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_BirClubBirKaro_Acilis_1Heart()
        {
            var el = ElMetinden("Q7", "KJ85", "65", "AK952");
            TestBirClubBirKaro("TEST 45.1 — 1♣-1♦-?: 4+♣ 4♥, 14 HCP → 1♥",
                "1♣ - 1♦ - ?", "1♥", el);
        }
        public static void Test_BirClubBirKaro_Acilis_1Spade()
        {
            var el = ElMetinden("KJ85", "76", "Q5", "AQ852");
            TestBirClubBirKaro("TEST 45.2 — 1♣-1♦-?: 4+♣ 4♠, 13 HCP → 1♠",
                "1♣ - 1♦ - ?", "1♠", el);
        }
        public static void Test_BirClubBirKaro_Acilis_1NT()
        {
            var el = ElMetinden("Q75", "J84", "K93", "AQ52");
            TestBirClubBirKaro("TEST 45.3 — 1♣-1♦-?: 13 HCP dengeli → 1NT",
                "1♣ - 1♦ - ?", "1NT", el);
        }
        public static void Test_BirClubBirKaro_Acilis_2Club()
        {
            var el = ElMetinden("Q7", "KJ5", "84", "AJ9752");
            TestBirClubBirKaro("TEST 45.4 — 1♣-1♦-?: 6+♣, 13 HCP → 2♣",
                "1♣ - 1♦ - ?", "2♣", el);
        }
        public static void Test_BirClubBirKaro_Acilis_2Diamond()
        {
            var el = ElMetinden("Q7", "KJ5", "A874", "Q985");
            TestBirClubBirKaro("TEST 45.5 — 1♣-1♦-?: 4+♣ 4♦, 12 HCP → 1NT",
                "1♣ - 1♦ - ?", "1NT", el);
        }
        public static void Test_BirClubBirKaro_Acilis_2Heart_Strong()
        {
            var el = ElMetinden("AQ", "AKJ5", "65", "KQJ85");
            TestBirClubBirKaro("TEST 45.6 — 1♣-1♦-?: 5+♣ 4♥, 19 HCP → 2♥",
                "1♣ - 1♦ - ?", "2♥", el);
        }
        public static void Test_BirClubBirKaro_Acilis_2Spade_Strong()
        {
            var el = ElMetinden("AKJ5", "76", "A5", "KQJ85");
            TestBirClubBirKaro("TEST 45.7 — 1♣-1♦-?: 5+♣ 4♠, 19 HCP → 2♠",
                "1♣ - 1♦ - ?", "2♠", el);
        }
        public static void Test_BirClubBirKaro_Acilis_2NT_Strong()
        {
            var el = ElMetinden("A", "KJ5", "Q84", "AKQJ95");
            TestBirClubBirKaro("TEST 45.8 — 1♣-1♦-?: 6+♣, 19 HCP → 2NT",
                "1♣ - 1♦ - ?", "2NT", el);
        }
        public static void Test_BirClubBirKaro_Acilis_3Club()
        {
            var el = ElMetinden("Q7", "KJ5", "84", "AKQJ97");
            TestBirClubBirKaro("TEST 45.9 — 1♣-1♦-?: 6+♣, 16 HCP → 3♣",
                "1♣ - 1♦ - ?", "3♣", el);
        }
        public static void Test_BirClubBirKaro_Acilis_3Diamond()
        {
            var el = ElMetinden("Q7", "52", "A832", "AKQJ5");
            TestBirClubBirKaro("TEST 45.10 — 1♣-1♦-?: 5+♣ 4♦, 16 HCP → 3♦",
                "1♣ - 1♦ - ?", "3♦", el);
        }
        public static void Test_BirClubBirKaro_Acilis_3Heart_Splinter()
        {
            var el = ElMetinden("AKJ5", "7", "AQJ8", "KQJ5");
            TestBirClubBirKaro("TEST 45.11 — 1♣-1♦-?: 4♣4♦4♠1♥, 19 HCP → 3♥",
                "1♣ - 1♦ - ?", "3♥", el);
        }
        public static void Test_BirClubBirKaro_Acilis_3Spade_Splinter()
        {
            var el = ElMetinden("7", "AKJ5", "AQJ8", "KQJ5");
            TestBirClubBirKaro("TEST 45.12 — 1♣-1♦-?: 4♣4♦4♥1♠, 19 HCP → 3♠",
                "1♣ - 1♦ - ?", "3♠", el);
        }
        public static void Test_BirClubBirKaro_Acilis_3NT_Void()
        {
            var el = ElMetinden("AKJ5", "AQJ8", "7", "KQJ5");
            TestBirClubBirKaro("TEST 45.13 — 1♣-1♦-?: 4♣4♥4♠1♦, 19 HCP → 3NT",
                "1♣ - 1♦ - ?", "3NT", el);
        }


        // ═══════════════════════════════════════════════════════════════════
        // TEST 46.x — 1♣ ⇒ 1♦ ⇒ 1♥ Gelişimi (Aşama A: Cevapçı Devam)
        // ═══════════════════════════════════════════════════════════════════

        private static IhaleDurumu BirKupaGelisimDurumuA(List<Card> el)
        {
            var gecmis = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♦", Sira = 2, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 3, GecerliMi = true },
            };
            return new IhaleDurumu
            {
                AktifOyuncu = Player.Guney,
                AktifOyuncuEli = el,
                Anlasma = BirClubAnlasmasi(),
                Gecmis = gecmis,
            };
        }

        private static IhaleDurumu BirKupaGelisimDurumuB(List<Card> el)
        {
            var gecmis = new List<IhaleHamlesi>
            {
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♣", Sira = 1, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "1♦", Sira = 2, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Kuzey, Teklif = "1♥", Sira = 3, GecerliMi = true },
                new IhaleHamlesi { Oyuncu = Player.Guney, Teklif = "2♣", Sira = 4, GecerliMi = true },
            };
            return new IhaleDurumu
            {
                AktifOyuncu = Player.Kuzey,
                AktifOyuncuEli = el,
                Anlasma = BirClubAnlasmasi(),
                Gecmis = gecmis,
            };
        }

        private static void TestBirKupaGelisim(string baslik, string senaryo, string beklenen,
            List<Card> el, bool asamaB = false)
        {
            Yaz("════════════════════════════════════════");
            Yaz(baslik);
            Yaz($"Senaryo: {senaryo}");
            Yaz($"Beklenen: {beklenen}");
            Yaz("────────────────────────────────────────");

            if (el.Count != 13)
            {
                Yaz($"❌ HATA: El {el.Count} kart, 13 olmalı!");
                return;
            }

            int hcp = ElDegerlendirici.HCP(el);
            Yaz($"HCP: {hcp}");
            Yaz($"Dengeli: {ElDegerlendirici.DengeliEl(el)}");
            Yaz($"Dağılım: ♠{ElDegerlendirici.RenkUzunlugu(el, "Maça")} " +
                $"♥{ElDegerlendirici.RenkUzunlugu(el, "Kupa")} " +
                $"♦{ElDegerlendirici.RenkUzunlugu(el, "Karo")} " +
                $"♣{ElDegerlendirici.RenkUzunlugu(el, "Sinek")}");
            Yaz("────────────────────────────────────────");

            var durum = asamaB ? BirKupaGelisimDurumuB(el) : BirKupaGelisimDurumuA(el);

            Yaz("────────────────────────────────────────");
            var motor = new IhaleMotoru(durum.Anlasma);
            string sonuc = motor.TeklifVer(durum);
            Yaz($"Motor → {sonuc}");
            Yaz(sonuc == beklenen ? $"✅ DOĞRU ({beklenen})"
                                  : $"❌ YANLIŞ (beklenen {beklenen}, gelen {sonuc})");
            Yaz("════════════════════════════════════════");
        }

        public static void Test_BirClubBirKaroBirKupa_Cevapci_1Spade()
        {
            var el = ElMetinden("KQ85", "32", "AQJ4", "852");
            TestBirKupaGelisim("TEST 46.1 — 1♣-1♦-1♥-?: 4+♦ 4♠ F1R → 1♠",
                "1♣ - 1♦ - 1♥ - ?", "1♠", el);
        }

        public static void Test_BirClubBirKaroBirKupa_Cevapci_1NT()
        {
            var el = ElMetinden("Q32", "J32", "K32", "J432");
            TestBirKupaGelisim("TEST 46.2 — 1♣-1♦-1♥-?: 6 HCP dengeli → 1NT",
                "1♣ - 1♦ - 1♥ - ?", "1NT", el);
        }

        public static void Test_BirClubBirKaroBirKupa_Cevapci_2Club()
        {
            var el = ElMetinden("A5", "32", "AQJ432", "K52");
            TestBirKupaGelisim("TEST 46.3 — 1♣-1♦-1♥-?: 13 HCP 6♦ röle → 2♣",
                "1♣ - 1♦ - 1♥ - ?", "2♣", el);
        }

        public static void Test_BirClubBirKaroBirKupa_Cevapci_2Diamond()
        {
            var el = ElMetinden("KQ85", "32", "AJ5432", "5");  // 10 HCP, 6♦4♠
            TestBirKupaGelisim("TEST 46.4 — 1♣-1♦-1♥-?: 6♦4♠ 10 HCP → 2♦",
                "1♣ - 1♦ - 1♥ - ?", "2♦", el);
        }

        public static void Test_BirClubBirKaroBirKupa_Cevapci_2NT()
        {
            var el = ElMetinden("A52", "J32", "KQ32", "J52");
            TestBirKupaGelisim("TEST 46.5 — 1♣-1♦-1♥-?: 12 HCP dengeli → 2NT",
                "1♣ - 1♦ - 1♥ - ?", "2NT", el);
        }

        public static void Test_BirClubBirKaroBirKupa_Cevapci_3Club()
        {
            var el = ElMetinden("KQ85", "3", "AJ943", "852");  // 10 HCP, 5♦4♠3♣1♥
            TestBirKupaGelisim("TEST 46.6 — 1♣-1♦-1♥-?: 5♦4♠3♣1♥ 10 HCP → 3♣",
                "1♣ - 1♦ - 1♥ - ?", "3♣", el);
        }

        public static void Test_BirClubBirKaroBirKupa_Cevapci_3Diamond()
        {
            var el = ElMetinden("KQ85", "32", "AKQ432", "5");
            TestBirKupaGelisim("TEST 46.7 — 1♣-1♦-1♥-?: 6♦4♠ FG → 3♦",
                "1♣ - 1♦ - 1♥ - ?", "3♦", el);
        }

        public static void Test_BirClubBirKaroBirKupa_Cevapci_3NT()
        {
            var el = ElMetinden("A52", "J32", "KQ32", "AJ2");
            TestBirKupaGelisim("TEST 46.8 — 1♣-1♦-1♥-?: 14 HCP dengeli durdurucu → 3NT",
                "1♣ - 1♦ - 1♥ - ?", "3NT", el);
        }

        public static void Test_BirClubBirKaroBirKupa_Acici_2Diamond_MIN()
        {
            var el = ElMetinden("A5", "KQ32", "Q52", "KJ52");
            TestBirKupaGelisim("TEST 47.1 — 1♣-1♦-1♥-2♣-?: 3♦ MIN → 2♦",
                "1♣ - 1♦ - 1♥ - 2♣ - ?", "2♦", el, asamaB: true);
        }

        public static void Test_BirClubBirKaroBirKupa_Acici_2Heart_MAX()
        {
            var el = ElMetinden("A5", "KQ32", "Q52", "AK52");
            TestBirKupaGelisim("TEST 47.2 — 1♣-1♦-1♥-2♣-?: 3♦ MAX → 2♥",
                "1♣ - 1♦ - 1♥ - 2♣ - ?", "2♥", el, asamaB: true);
        }

        public static void Test_BirClubBirKaroBirKupa_Acici_2Spade()
        {
            var el = ElMetinden("KQ85", "AQ32", "52", "K52");
            TestBirKupaGelisim("TEST 47.3 — 1♣-1♦-1♥-2♣-?: 4♠ → 2♠",
                "1♣ - 1♦ - 1♥ - 2♣ - ?", "2♠", el, asamaB: true);
        }

        public static void Test_BirClubBirKaroBirKupa_Acici_2NT()
        {
            var el = ElMetinden("52", "KQ32", "QJ52", "K52");
            TestBirKupaGelisim("TEST 47.4 — 1♣-1♦-1♥-2♣-?: 4♦ 12 HCP → 2NT",
                "1♣ - 1♦ - 1♥ - 2♣ - ?", "2NT", el, asamaB: true);
        }

        public static void Test_BirClubBirKaroBirKupa_Acici_3Club()
        {
            var el = ElMetinden("52", "KQ32", "52", "AQ852");
            TestBirKupaGelisim("TEST 47.5 — 1♣-1♦-1♥-2♣-?: 4♥4♣ 11 HCP → 3♣",
                "1♣ - 1♦ - 1♥ - 2♣ - ?", "3♣", el, asamaB: true);
        }

        public static void Test_BirClubBirKaroBirKupa_Acici_3Diamond()
        {
            var el = ElMetinden("A5", "Q32", "KJ852", "532");  // 10 HCP, 13 kart
            TestBirKupaGelisim("TEST 47.6 — 1♣-1♦-1♥-2♣-?: 5♦ 10 HCP → 3♦",
                "1♣ - 1♦ - 1♥ - 2♣ - ?", "3♦", el, asamaB: true);
        }

        public static void Test_BirClubBirKaroBirKupa_Acici_3Heart()
        {
            var el = ElMetinden("A5", "AQ32", "KQ52", "852");  // 16 HCP, 4♦4♥
            TestBirKupaGelisim("TEST 47.7 — 1♣-1♦-1♥-2♣-?: 4♦4♥ SI → 3♥",
                "1♣ - 1♦ - 1♥ - 2♣ - ?", "3♥", el, asamaB: true);
        }

        public static void TumTestleriCalistir()
        {
            Yaz("▶ Test 1.1: Gerber Soru Sorma");
            Test_Gerber_SoruSorma();

            Yaz("▶ Test 1.2: Gerber Cevap");
            Test_Gerber_Cevap();

            Yaz("▶ Test 1.3: Gerber Negatif");
            Test_Gerber_Negatif();

            Yaz("▶ Test 1.4: Gerber 3 As");
            Test_Gerber_3As();

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
            Test_Puppet_3luMajor_Fit();
            Test_NegatifKontr_5liMajor();
            Test_Puppet_5liKupa_SignOff();
            Test_Puppet_KeyCard_2();
            Test_Puppet_KeyCard_3();
            Test_Puppet_KeyCard_4_1Q();
            Test_Cevap_BesliMajor_1MacaYerine2NT();
            Test_Preempt_Cevap_Ve_ZayifIki();
            Test_KoltukHaritasi();
            Test_Rebid2NT_Cevap();
            Test_Destek_Reverse_Degil();
            Test_Slam_JumpShift_Destek();
            Test_Slam_JumpShift_Buyuk();
            Test_Acilis_6liMajor_15HP_1NT_Degil();
            Test_Mudahale_Sonrasi_2NT_ve_Destek();
            Test_ZayifIki_Karsi_Takeout();
            Test_Sicramali_Destek_Kabul();
            Test_Deklaran_Dummy_Tek_Kral_Yakma();
            Test_Jacoby_Transfer_Onceki_Pas();
            Test_Unusual2NT_Cevap();
            Test_Mudahale_Advancer_Cevap();
            Test_Kontra_Cevaplari();
            Test_Agresiflik_Ve_Beceri();
            Test_Blackwood_Soran_Karar();
            Test_MinorTransfer_Sonrasi_Devam();
            Test_Cevap_16HP_Sicrama_Degil();
            Test_MinorTransfer_Tamamla_Kontra_Sonrasi();
            Test_Acici_Cevapcinin_Majorunu_Destekledi();
            Test_4SF_XYZ_Sonrasi_Degil();
            Test_Ikinci_Renk_2_1_Sonrasi();
            Test_Slam_Dizisi_2_1();
            Test_KonvansiyonPratigi();
            Test_YakinKacis();
            Test_DagitimUretici();
            Test_IhaleKosulu();
            Test_YeniKonvansiyonlar();
            Test_YeniKonvOyunEntegrasyonu();
            Test_IhaleAnalizi();

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

            Yaz("▶ Test 14.1-14.3: Lebensohl Zayıf İki");
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

            Yaz("▶ TEST 26.1-26.7: DONT");
            Test_Dont_2Sinek();
            Test_Dont_2Karo();
            Test_Dont_2Kupa_Majors();
            Test_Dont_Double();
            Test_Dont_PartnerCevap_Double();
            Test_Dont_PartnerCevap_2Sinek();
            Test_Dont_Balancing_ZayifEl_Pas();

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

            Yaz("▶ TEST 32.1-32.6: Zayıf İki");
            Test_ZayifIki_Maca();
            Test_ZayifIki_Kupa();
            Test_ZayifIki_Karo();
            Test_ZayifIki_Negatif_YuksekHP();
            Test_ZayifIki_Negatif_5liRenk();
            Test_ZayifIki_Negatif_GecmisVar();

            Yaz("▶ TEST 33.1-33.6: Preempt");
            Test_Preempt_3Maca();
            Test_Preempt_3Kupa();
            Test_Preempt_3Sinek();
            Test_Preempt_4Karo();
            Test_Preempt_Negatif_GucluEl();
            Test_Preempt_Negatif_KisaRenk();

            Yaz("▶ TEST 34.1-34.5: Namyats");
            Test_Namyats_4Sinek_GucluKupa();
            Test_Namyats_4Karo_GucluMaca();
            Test_Namyats_Negatif_ZayifEl();
            Test_Namyats_PreemptCakismasiYok();
            Test_Namyats_Negatif_KisaRenk();

            Yaz("▶ TEST 35.1-35.6: Namyats Cevapları");
            Test_NamyatsCevap_SignOff();
            Test_NamyatsCevap_AraAdim();
            Test_NamyatsCevap_Blackwood();
            Test_NamyatsCevap_SignOff_Maca();
            Test_NamyatsCevap_AraAdimSonrasi_Aciyor();
            Test_NamyatsCevap_Negatif_AcilisDegil();

            Yaz("▶ TEST 36.1-36.8: RKCB (1430/0314)");
            Test_RKCB_1KilitKart();
            Test_RKCB_0KilitKart();
            Test_RKCB_2KilitKart_QYok();
            Test_RKCB_2KilitKart_QVar();
            Test_RKCB_3KilitKart();
            Test_RKCB_4KilitKart();
            Test_RKCB_0314_1KilitKart();
            Test_RKCB_Negatif_NTKoz();

            Yaz("▶ TEST 37.1-37.8: Exclusion Blackwood");
            Test_Exclusion_5Karo_0Kilit();
            Test_Exclusion_5Karo_3Kilit();
            Test_Exclusion_5Karo_1Kilit();
            Test_Exclusion_5Karo_2Kilit_QYok();
            Test_Exclusion_5Karo_2Kilit_QVar();
            Test_Exclusion_SikanYok();
            Test_Exclusion_1Kademe_5Sinek();
            Test_Exclusion_0314_5Karo_1Kilit();

            Yaz("▶ TEST 38.1-38.8: Kickback");
            Test_Kickback_1Kilit_Kupa();
            Test_Kickback_0Kilit_Kupa();
            Test_Kickback_2Kilit_QYok_Kupa();
            Test_Kickback_2Kilit_QVar_Kupa();
            Test_Kickback_Karo_Koz();
            Test_Kickback_Sinek_Koz();
            Test_Kickback_Negatif_MacaKoz();
            Test_Kickback_0314_Kupa();

            Yaz("▶ TEST 39.1-39.6: Koz Q Sorusu");
            Test_KozQ_Soruluyor();
            Test_KozQ_Cevap_QYok_SinekK();
            Test_KozQ_Cevap_QVar_YanKYok();
            Test_KozQ_Cevap_QVar_YanKVar();
            Test_KozQ_Kickback_Sonrasi();
            Test_KozQ_Negatif_MuddahaleYok();

            Yaz("▶ TEST 40.1-40.6: Pick-a-Slam");
            Test_PickASlam_Soruluyor();
            Test_PickASlam_Cevap_Sinek();
            Test_PickASlam_Cevap_Karo();
            Test_PickASlam_Negatif_RKCBSekansi();
            Test_PickASlam_Negatif_DusukSeviye();
            Test_PickASlam_Negatif_MinorYok();

            Yaz("▶ TEST 41.1-41.2: Serious 3NT");
            Test_Serious3NT_Pozitif();
            Test_Serious3NT_Negatif_RakipMudahalesi();

            Yaz("▶ TEST 42.1-42.17: Multi 2♦");
            Test_Multi2D_Acilis_Maca();
            Test_Multi2D_Acilis_Kupa();
            Test_Multi2D_Kapali_ZayifIkiKorunur();
            Test_Multi2D_Negatif_BesliMajor();
            Test_Multi2D_Negatif_GucluEl();
            Test_Multi2D_Negatif_DigerMajor4();
            Test_Multi2D_Cevap_Kupa();
            Test_Multi2D_Cevap_Maca();
            Test_Multi2D_Cevap_Soru2NT();
            Test_Multi2D_Acici_Kupa_Pas();
            Test_Multi2D_Acici_Maca_Duzelt();
            Test_Multi2D_Acici_Kupa_Duzelt();
            Test_Multi2D_Acici_Maca_Pas();
            Test_Multi2D_Acici_2NT_MinimumMaca();
            Test_Multi2D_Acici_2NT_MaksimumKupa();
            Test_Multi2D_Negatif_RakipMudahalesi();
            Test_Multi2D_Negatif_RakipActi();

            // ═══════════════════════════════════════════════════════════════════
            // TEST 43.x — Takeout Double
            // ═══════════════════════════════════════════════════════════════════
            Yaz("▶ TEST 43.1-43.19: Takeout Double");
            Test_TakeoutDouble_Kontra_Maca();
            Test_TakeoutDouble_Kontra_Kupa();
            Test_TakeoutDouble_Kontra_KisaRakipRengi();
            Test_TakeoutDouble_Kontra_Guclu();
            Test_TakeoutDouble_Kontra_RakipKaro();
            Test_TakeoutDouble_Negatif_YetersizHP();
            Test_TakeoutDouble_Negatif_RakipRengiUzun();
            Test_TakeoutDouble_Negatif_5liYanRenk();
            Test_TakeoutDouble_Cevap_3NT();
            Test_TakeoutDouble_Cevap_3NT_YanMajorVar();
            Test_TakeoutDouble_Cevap_CueBid();
            Test_TakeoutDouble_Cevap_CueBid_15HP();
            Test_TakeoutDouble_Cevap_Davet();
            Test_TakeoutDouble_Cevap_Davet_2NT();
            Test_TakeoutDouble_Cevap_1NT();
            Test_TakeoutDouble_Cevap_Zayif_EnIyiRenk();
            Test_TakeoutDouble_Cevap_Zayif_EnUcuzSeviye();
            Test_TakeoutDouble_Cevap_CezaPasi();
            Test_TakeoutDouble_Cevap_CezaPasi_YetersizOnör();

            Yaz("▶ TEST 44.1-44.6: 1♣ Cevap Sistemi");
            Test_BirClub_FG_Relay();
            Test_BirClub_Major();
            Test_BirClub_Preempt_NV();
            Test_BirClub_Preempt_V();
            Test_BirClub_TersFlannery();
            Test_BirClub_1NT();

            Yaz("▶ TEST 45.1-45.13: 1♣ ⇒ 1♦ Gelişimi (Aşama 1)");
            Test_BirClubBirKaro_Acilis_1Heart();
            Test_BirClubBirKaro_Acilis_1Spade();
            Test_BirClubBirKaro_Acilis_1NT();
            Test_BirClubBirKaro_Acilis_2Club();
            Test_BirClubBirKaro_Acilis_2Diamond();
            Test_BirClubBirKaro_Acilis_2Heart_Strong();
            Test_BirClubBirKaro_Acilis_2Spade_Strong();
            Test_BirClubBirKaro_Acilis_2NT_Strong();
            Test_BirClubBirKaro_Acilis_3Club();
            Test_BirClubBirKaro_Acilis_3Diamond();
            Test_BirClubBirKaro_Acilis_3Heart_Splinter();
            Test_BirClubBirKaro_Acilis_3Spade_Splinter();
            Test_BirClubBirKaro_Acilis_3NT_Void();


            Yaz("");

            Yaz("▶ TEST 46.1-46.8: 1♣ ⇒ 1♦ ⇒ 1♥ Gelişimi (Cevapçı Devam)");
            Test_BirClubBirKaroBirKupa_Cevapci_1Spade();
            Test_BirClubBirKaroBirKupa_Cevapci_1NT();
            Test_BirClubBirKaroBirKupa_Cevapci_2Club();
            Test_BirClubBirKaroBirKupa_Cevapci_2Diamond();
            Test_BirClubBirKaroBirKupa_Cevapci_2NT();
            Test_BirClubBirKaroBirKupa_Cevapci_3Club();
            Test_BirClubBirKaroBirKupa_Cevapci_3Diamond();
            Test_BirClubBirKaroBirKupa_Cevapci_3NT();

            Yaz("▶ TEST 47.1-47.7: 1♣ ⇒ 1♦ ⇒ 1♥ ⇒ 2♣ Gelişimi (Açıcı Röle Cevabı)");
            Test_BirClubBirKaroBirKupa_Acici_2Diamond_MIN();
            Test_BirClubBirKaroBirKupa_Acici_2Heart_MAX();
            Test_BirClubBirKaroBirKupa_Acici_2Spade();
            Test_BirClubBirKaroBirKupa_Acici_2NT();
            Test_BirClubBirKaroBirKupa_Acici_3Club();
            Test_BirClubBirKaroBirKupa_Acici_3Diamond();
            Test_BirClubBirKaroBirKupa_Acici_3Heart();

            Yaz("✅ TÜM TESTLER TAMAMLANDI!");
        }

    }
}