using System;
using System.Collections.Generic;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Tests
{
    /// <summary>
    /// Toplu denetimden çıkan düzeltmelerin tam motor testleri (rakipsiz diziler, Güney sıradaki oyuncu):
    /// 1M–1NT sonrası 17+ HP açıcı, Gerber sonrası devam, üçüncü renk forsingi sonrası 3 kartlık destek.
    /// Geçmiş listesi Güney'in hemen sağındaki rakibin Pas'ı ile biter.
    /// </summary>
    public static class TestRunner_Duzeltmeler
    {
        private static int _gecen, _kalan;
        private static Action<string> _yaz;

        public static int TumTestleriCalistir(Action<string> sonucEkle)
        {
            _gecen = 0; _kalan = 0; _yaz = sonucEkle;
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  DÜZELTME TESTLERİ (1M–1NT, Gerber, TSF)");
            sonucEkle("════════════════════════════════════════");

            // 1♥ – 1NT (pas geçmiş cevaplayan: forcing değil) – açıcı 17+ HP
            T("N1 1H-1NT 17hcp dengeli -> 2NT davet", TestHelpers.ElOlustur("K72", "AQJ95", "K43", "A4"), "2NT", "Pas", "Pas", "1♥", "Pas", "1NT", "Pas");
            T("N2 1H-1NT 6 kupa 17hcp -> 3H davet", TestHelpers.ElOlustur("K2", "AKJ952", "Q43", "A4"), "3♥", "Pas", "Pas", "1♥", "Pas", "1NT", "Pas");
            T("N3 1H-1NT asgari 13hcp -> Pas", TestHelpers.ElOlustur("Q72", "KJ985", "K43", "A4"), "Pas", "Pas", "Pas", "1♥", "Pas", "1NT", "Pas");

            // 1NT – 4♣ (Gerber) – cevap – soranın devamı
            T("G1 1NT-4C-4S: 4 as + 19hcp -> 6NT", TestHelpers.ElOlustur("AK2", "AQ3", "KJ4", "Q762"), "6NT", "1NT", "Pas", "4♣", "Pas", "4♠", "Pas");
            T("G2 1NT-4C-4D: as eksik -> 4NT (bitir)", TestHelpers.ElOlustur("A32", "KQ3", "KJ4", "Q762"), "4NT", "1NT", "Pas", "4♣", "Pas", "4♦", "Pas");

            // 1♣ – 1♠ – 1NT – 2♦ (oyun zorlayıcı checkback) – 2♠ (3 kart destek): cevapçı oyuna kadar devam eder
            T("K1 checkback 2D-2S, 4 maca -> 3NT", TestHelpers.ElOlustur("AJ52", "T4", "AQ43", "Q82"), "3NT", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2♦", "Pas", "2♠", "Pas");
            T("K2 checkback 2D-2S, 5 maca -> 4S", TestHelpers.ElOlustur("AJ852", "T4", "AQ4", "Q82"), "4♠", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2♦", "Pas", "2♠", "Pas");

            // 1♣ – 1♠ – 1NT – 2NT (davet): açıcı 3 kartlık destekle düzeltir
            T("C1 1C-1S-1NT-2NT, 3 maca 13hcp -> Pas (4-3 fit riskli)", TestHelpers.ElOlustur("A52", "J973", "A82", "AT3"), "Pas", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2NT", "Pas");
            T("C2 1C-1S-1NT-2NT, 3 maca 14hcp -> 3NT", TestHelpers.ElOlustur("A52", "KJ73", "A82", "AT3"), "3NT", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2NT", "Pas");
            T("C3 1C-1S-1NT-2NT, 2 maca asgari -> Pas", TestHelpers.ElOlustur("K5", "K842", "K954", "A42"), "Pas", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2NT", "Pas");

            // Rekabet: ortak 1♣, rakip 1 seviye müdahale; dengeli güçlü el pas geçmemeli
            T("R1 1C-(1H) 18hcp dengeli, tutuk -> 3NT", TestHelpers.ElOlustur("832", "K93", "AKJ7", "AK2"), "3NT", "1♣", "1♥");
            T("R2 1C-(1D) 14hcp dengeli, tutuk -> 2NT", TestHelpers.ElOlustur("Q82", "K93", "KJ7", "AJ52"), "2NT", "1♣", "1♦");

            // Unusual 2NT sonrası 3. seviye: 18 HP + rakip rengi AKQ8 -> ceza kontrası
            T("U1 1C-(2NT)-P-(3H) 18hcp AKQ8 -> Dbl", TestHelpers.ElOlustur("JT97", "AKQ8", "A", "KJ82"), "Dbl", "1♣", "2NT", "Pas", "3♥");
            // 1♣-(1♦)-1♠-(Pas)-2♣: cevapçı 16 HP, 4 kupa -> 2♥ (pas değil)
            T("U2 1C-(1D)-1S-P-2C 16hcp 4H -> 2H", TestHelpers.ElOlustur("AQJ72", "AK54", "764", "Q"), "2♥", "1♣", "1♦", "1♠", "Pas", "2♣", "Pas");

            // 1♦ – Pas: 9 HP ve 7 kartlı sinek -> 2♣ (ortağın renginden düşük renk 1 seviyede söylenemez)
            T("D1 1D-(P) 9hcp 7 sinek -> 2C", TestHelpers.ElOlustur("A42", "53", "K", "QT87642"), "2♣", "1♦", "Pas");
            T("D2 1D-(P) 5hcp 7 sinek -> Pas", TestHelpers.ElOlustur("432", "53", "K", "QT87642"), "Pas", "1♦", "Pas");

            // 3. renk forsingi zinciri: 1♦-1♠-2♦-3♣-3♥ (4. renk sorusu) -> cevapçı tutukla 3NT
            T("F1 1D-1S-2D-3C-3H, kupa AQT2 -> 3NT", TestHelpers.ElOlustur("9764", "AQT2", "Q42", "A9"), "3NT", "1♦", "Pas", "1♠", "Pas", "2♦", "Pas", "3♣", "Pas", "3♥", "Pas");
            T("F2 1D-1S-2D-3C-3H, kupa tutuk yok -> 3NT degil", TestHelpers.ElOlustur("A9764", "9532", "Q42", "A"), "3♠", "1♦", "Pas", "1♠", "Pas", "2♦", "Pas", "3♣", "Pas", "3♥", "Pas");

            // 1♥ – 1♠ – 2♥ (ortak majörü tekrarladı): cevapçı 11+ HP ile pas geçmez
            T("M1 1H-1S-2H, 12hcp dengeli 2 kupa -> 2NT", TestHelpers.ElOlustur("Q8765", "A7", "KJ7", "QT5"), "2NT", "1♥", "Pas", "1♠", "Pas", "2♥", "Pas");
            T("M2 1H-1S-2H, 14hcp 3 kupa -> 4H", TestHelpers.ElOlustur("AKQ86", "J42", "KJ4", "43"), "4♥", "1♥", "Pas", "1♠", "Pas", "2♥", "Pas");
            T("M3 1H-1S-2H, 8hcp -> Pas", TestHelpers.ElOlustur("Q8765", "J42", "KJ4", "43"), "Pas", "1♥", "Pas", "1♠", "Pas", "2♥", "Pas");

            // Jacoby 2NT: açıcı cevabı ve cevapçının devamı
            T("J1 1S-2NT, 11hcp tek kupa -> 3H (kisalik)", TestHelpers.ElOlustur("AQ962", "9", "K92", "Q832"), "3♥", "1♠", "Pas", "2NT", "Pas");
            T("J2 1S-2NT, 12hcp dengeli 5332 -> 4S (asgari)", TestHelpers.ElOlustur("AQ962", "K92", "Q83", "J3"), "4♠", "1♠", "Pas", "2NT", "Pas");
            T("J3 1S-2NT-3H: cevapci 4S", TestHelpers.ElOlustur("KT73", "AQ74", "J4", "KJ6"), "4♠", "1♠", "Pas", "2NT", "Pas", "3♥", "Pas");

            // 1♠ – 1NT (forcing): 6'lı majör ve 21 HP -> 4♠ (2♠ ile durma)
            T("N4 1S-1NT, 6 maca 21hcp -> 4S", TestHelpers.ElOlustur("KQJ732", "AQ7", "K", "AQ8"), "4♠", "1♠", "Pas", "1NT", "Pas");
            T("N5 1S-1NT, 6 maca 12hcp -> 2S", TestHelpers.ElOlustur("KQJ732", "A87", "65", "Q8"), "2♠", "1♠", "Pas", "1NT", "Pas");

            // 1NT – 2♦ – 2♥: 5 kupa ve 9 HP -> 2NT (davet); açıcı 3 kupa asgari -> 3♥
            T("Q1 1NT-2D-2H, 5 kupa 9hcp -> 2NT", TestHelpers.ElOlustur("A42", "KJ972", "J65", "T4"), "2NT", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas");
            T("Q2 1NT-2D-2H-2NT, 3 kupa 15hcp -> 3H", TestHelpers.ElOlustur("KQ4", "A83", "KJ52", "Q64"), "3♥", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "2NT", "Pas");
            T("Q3 1NT-2D-2H-2NT, 2 kupa 15hcp -> Pas", TestHelpers.ElOlustur("KQ64", "A8", "KJ52", "Q64"), "Pas", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "2NT", "Pas");

            // 1NT açılışından sonra slam araması: 6 kartlı majör + 16 HP transferden sonra 4NT
            T("S1 1NT-2D-2H, 6 kupa 16hcp -> 4NT", TestHelpers.ElOlustur("AK3", "AKJ974", "Q5", "42"), "4NT", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas");
            T("S2 1NT-2D-2H-4NT, 2 anahtar kart + kupa Q -> 5S", TestHelpers.ElOlustur("Q52", "KQ3", "AJ4", "K962"), "5♠", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "4NT", "Pas");

            T("S3 1NT-2D-2H-4NT-5S: toplam 5 anahtar kart -> 5NT (kral sorusu)", TestHelpers.ElOlustur("AK3", "AKJ974", "Q5", "42"), "5NT", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "4NT", "Pas", "5♠", "Pas");

            // 1M – 1NT (forcing): açıcı 17+ HP ile 2 seviyesinde asgari rebid yapmaz
            T("A1 1H-1NT, 20hcp 5-4-3-1 -> 3D (sicrama)", TestHelpers.ElOlustur("K95", "AKJT5", "AQ74", "K"), "3♦", "1♥", "Pas", "1NT", "Pas");
            T("A2 1H-1NT, 13hcp 5-4-3-1 -> 2D (asgari)", TestHelpers.ElOlustur("K95", "AQJT5", "KQ74", "2"), "2♦", "1♥", "Pas", "1NT", "Pas");
            T("A3 1S-1NT, 18hcp dengeli 5332 -> 2NT", TestHelpers.ElOlustur("AKJ85", "KQ3", "A74", "Q2"), "2NT", "1♠", "Pas", "1NT", "Pas");

            // 1♦ – 1♠: açıcı 17-18 HP + 6 kartlık karo → 3♦ sıçrama (asgari 2♦ değil)
            T("B1 1D-1S, 18hcp 6 karo -> 3D", TestHelpers.ElOlustur("K87", "AK9", "AKJ632", "6"), "3♦", "1♦", "Pas", "1♠", "Pas");
            T("B2 1D-1S, 13hcp 6 karo -> 2D", TestHelpers.ElOlustur("K87", "A92", "KQJ632", "6"), "2♦", "1♦", "Pas", "1♠", "Pas");

            // 1♦ – 1♠ – 3♦ (17-18 HP sıçrama): cevapçı 9 HP ile pas geçmez
            T("B3 1D-1S-3D, 9hcp 5 maca -> 4S", TestHelpers.ElOlustur("A9432", "T4", "T4", "KQ87"), "4♠", "1♦", "Pas", "1♠", "Pas", "3♦", "Pas");
            T("B4 1D-1S-3D, 5hcp -> Pas", TestHelpers.ElOlustur("K9432", "T4", "T64", "J87"), "Pas", "1♦", "Pas", "1♠", "Pas", "3♦", "Pas");
            T("B5 1D-1S-3D, 10hcp 4 maca dengeli -> 3NT", TestHelpers.ElOlustur("KJ43", "Q4", "T64", "AQ87"), "3NT", "1♦", "Pas", "1♠", "Pas", "3♦", "Pas");

            // 1♥ açılışına 4+ maça varsa 1NT (forcing) değil 1♠
            T("E1 1H-?, 5 maca 10hcp -> 1S", TestHelpers.ElOlustur("AKT73", "6", "Q84", "9875"), "1♠", "1♥", "Pas");
            T("E2 1H-?, 4 maca 7hcp -> 1S", TestHelpers.ElOlustur("KJ73", "82", "Q842", "J95"), "1♠", "1♥", "Pas");
            T("E3 1H-?, 2 kupa 2 maca 7hcp -> 1NT", TestHelpers.ElOlustur("Q4", "82", "KJ842", "J953"), "1NT", "1♥", "Pas");

            // 1♦ – (?): 15 HP dengeli, karoda durdurucu (A104) → 1NT müdahalesi
            T("Y1 (1D)-? 15hcp 3-4-4-2 karo durdurucu -> 1NT", TestHelpers.ElOlustur("K93", "AKJ2", "AT43", "T8"), "1NT", "1♦");
            T("Y2 (1D)-? 15hcp dengeli, karoda durdurucu yok -> kontra", TestHelpers.ElOlustur("K93", "AKJ2", "432", "AT8"), "Dbl", "1♦");

            // Zayıf iki açılışına (2♥) karşı 15-18 HP dengeli: durdurucu varsa 2NT, yoksa takeout kontra
            T("Y3 (2H)-? 17hcp dengeli, kupada durdurucu -> 2NT", TestHelpers.ElOlustur("AQ3", "KJ3", "AJ42", "QT8"), "2NT", "2♥");
            T("Y4 (2H)-? 15hcp dengeli, kupada durdurucu yok -> Dbl", TestHelpers.ElOlustur("K93", "432", "AKJ2", "AT8"), "Dbl", "2♥");
            T("Y5 (2H)-? 12hcp dengeli -> Pas", TestHelpers.ElOlustur("K93", "432", "AQJ2", "T98"), "Pas", "2♥");

            // 3♦ engel açılışı, iki pas: pass-out koltuğunda kısa karo + diğer renklerde destek → takeout kontra
            T("P1 (3D)-P-P-? 12hcp 4-5-1-3 -> Dbl", TestHelpers.ElOlustur("A652", "AQT98", "9", "Q54"), "Dbl", "3♦", "Pas", "Pas");
            T("P2 (3D)-P-P-? 5hcp zayif -> Pas", TestHelpers.ElOlustur("J652", "QT98", "9", "Q542"), "Pas", "3♦", "Pas", "Pas");
            T("P3 (3D)-? 14hcp 4-4-1-4 dogrudan -> Dbl", TestHelpers.ElOlustur("KQ74", "AJ95", "2", "KJ96"), "Dbl", "3♦");
            T("P4 (3D)-? 11hcp dogrudan -> Pas", TestHelpers.ElOlustur("KQ74", "AJ95", "2", "J986"), "Pas", "3♦");

            // Ortağın 3♦ engeline takeout kontrası: cevap
            T("P5 (3D)-X-P-? 5 maca zayif -> 3S", TestHelpers.ElOlustur("Q9652", "T98", "9", "J542"), "3♠", "3♦", "Dbl", "Pas");
            T("P6 (3D)-X-P-? karo uzun+onor -> Pas (ceza)", TestHelpers.ElOlustur("J2", "T98", "KQJT96", "Q5"), "Pas", "3♦", "Dbl", "Pas");

            // Gerçek el: Doğu 3♦-P-P'de kontra yaptı, Batı (14 HP, 5 sinek) kontrayı cezaya çevirmez
            T("P7 (3D)-X-P-? 14hcp 4 maca 5 sinek -> 4S (oyun)", TestHelpers.ElOlustur("KQ74", "K", "Q84", "KJT96"), "4♠", "3♦", "Dbl", "Pas");
            T("P8 (3D)-X-P-? 9hcp 4 maca -> 3S (asgari)", TestHelpers.ElOlustur("KJ74", "Q42", "9", "J9654"), "3♠", "3♦", "Dbl", "Pas");

            // Zayıf İki cevabı: açıcı 6 kart vaat eder, 2 kartlık majör desteği + 15 HP → 4M
            TZ("Z1 2H-P-? 15hcp 2 kupa -> 4H", TestHelpers.ElOlustur("A764", "97", "AKQ8", "Q85"), "4♥", "2♥", "Pas");
            TZ("Z2 2H-P-? 14hcp 2 kupa -> Pas", TestHelpers.ElOlustur("A764", "97", "AKJ8", "Q85"), "Pas", "2♥", "Pas");
            TZ("Z3 2H-P-? 15hcp 3 kupa -> 4H", TestHelpers.ElOlustur("A74", "974", "AKQ8", "Q85"), "4♥", "2♥", "Pas");

            // Vugraph Bord 1: 1♠–2♠–3♣(deneme)–4♠ sonrası açıcı (17 HP) Blackwood sormaz; ortak 6-10 HP'nin üst yarısı
            T("V1 1S-2S-3C-4S 17hcp -> Pas (slam yok)", TestHelpers.ElOlustur("AKQJT6", "3", "A74", "QJ2"), "Pas", "Pas", "Pas", "1♠", "Pas", "2♠", "Pas", "3♣", "Pas", "4♠", "Pas");

            // Vugraph Bord 2: 1NT–2NT (karo transferi)–3♦: 9 HP + QJT54 karo = 3NT, 8 HP tek onör zayıf → Pas
            T("V2 1NT-2NT-3D 9hcp QJT54 karo -> 3NT", TestHelpers.ElOlustur("Q92", "K8", "QJT54", "J73"), "3NT", "1NT", "Pas", "2NT", "Pas", "3♦", "Pas");
            T("V3 1NT-2NT-3D 8hcp 5 karo -> Pas", TestHelpers.ElOlustur("J92", "K8", "QJT54", "J73"), "Pas", "1NT", "Pas", "2NT", "Pas", "3♦", "Pas");
            T("V4 1NT-2S-3C 9hcp QJT54 sinek -> 3NT", TestHelpers.ElOlustur("Q92", "K8", "J73", "QJT54"), "3NT", "1NT", "Pas", "2♠", "Pas", "3♣", "Pas");

            // Vugraph Bord 4: (1♥)–? 18 HP, 4-3-2-4, kupada durdurucu yok → güçlü takeout kontra (Pas değil)
            T("V5 (1H)-? 18hcp 4S-3H-2D-4C -> Dbl", TestHelpers.ElOlustur("AJ53", "J86", "KQ", "AK93"), "Dbl", "Pas", "Pas", "1♥");
            // Kontraya 1♠ cevabı 0-8 HP: 18 HP ile 4♠ değil, 2♠ (çift açık 9 el)
            T("V6 (1H)-X-P-1S-P-? 18hcp 4 maca -> 3S (davet, 4S degil)", TestHelpers.ElOlustur("AJ53", "J86", "KQ", "AK93"), "3♠", "Pas", "Pas", "1♥", "Dbl", "Pas", "1♠", "Pas");

            // Vugraph Bord 5: 1♥–1♠ sonrası 16 HP + 7'li kupa (AKQT832) → sıçrama rebid 3♥ (16 HP + 1 uzunluk puanı)
            T("V7 1H-P-1S-P-? 16hcp 7 kupa -> 3H", TestHelpers.ElOlustur("A8", "AKQT832", "K63", "8"), "3♥", "1♥", "Pas", "1♠", "Pas");
            T("V8 1H-P-1S-P-? 14hcp 7 kupa -> 2H (sicrama yok)", TestHelpers.ElOlustur("A8", "KQJT832", "K63", "8"), "2♥", "1♥", "Pas", "1♠", "Pas");

            // Vugraph Bord 8: 1NT–2♦–2♥–3NT (5'li kupa + oyun) sonrası açıcı 3 kartlık destekle 4♥'ye düzeltir
            T("V9 1NT-2D-2H-3NT 15hcp 3 kupa -> 4H", TestHelpers.ElOlustur("AJT7", "K62", "T873", "AK"), "4♥", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "3NT", "Pas");
            T("V10 1NT-2D-2H-3NT 2 kupa -> Pas", TestHelpers.ElOlustur("AJT7", "K6", "T8732", "AK"), "Pas", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "3NT", "Pas");
            // Vugraph Bord 11: 1♣–1♥–2♥ ortağın desteği, reverse değil → 8 HP ile 3♥ yok
            T("V11 1C-1H-2H-? 8hcp 5 kupa -> Pas", TestHelpers.ElOlustur("J73", "AQJ54", "T42", "42"), "Pas", "1♣", "Pas", "1♥", "Pas", "2♥", "Pas");
            // Vugraph Bord 12: 1♥–1♠–2♥ misfit: tekil kupa + 5'li maça → 2♠ (Pas değil)
            T("V12 1H-1S-2H-? 8hcp 5 maca tekil kupa -> 2S", TestHelpers.ElOlustur("KQ972", "9", "QT532", "J5"), "2♠", "1♥", "Pas", "1♠", "Pas", "2♥", "Pas");

            // Vugraph Bord 19: minör transferi açıkken 8-9 HP düz el 2NT (= karo transferi) demez; Stayman (2♣) ile davet eder
            T("V13 1NT-? 8hcp duz el, minor transferi acik -> 2C", TestHelpers.ElOlustur("975", "AT7", "AT62", "986"), "2♣", "1NT", "Pas");
            T("V14 1NT-2C-2D-? 8hcp duz el -> 2NT (davet)", TestHelpers.ElOlustur("975", "AT7", "AT62", "986"), "2NT", "1NT", "Pas", "2♣", "Pas", "2♦", "Pas");
            T("V15 1C-1D-? 12hcp 4 maca -> 1S (majör 4-4 ara)", TestHelpers.ElOlustur("AT75", "AQT", "74", "QT75"), "1♠", "1♣", "Pas", "1♦", "Pas");
            T("V16 1D-1S-2H(reverse)-? 10hcp 4 kupa 6 maca -> 3H (fit)", TestHelpers.ElOlustur("AKT874", "KT93", "9", "T8"), "3♥", "1♦", "Pas", "1♠", "Pas", "2♥", "Pas");
            T("V17 1D-1S-2H-3H-? 17hcp 4 kupa -> 4H (pas degil)", TestHelpers.ElOlustur("J", "QJ72", "AKT73", "AQ4"), "4♥", "1♦", "Pas", "1♠", "Pas", "2♥", "Pas", "3♥", "Pas");
            T("V18 (1H)-Dbl-P-1S-P-? 18hcp 4 maca -> 3S (davet sicramasi)", TestHelpers.ElOlustur("AJ53", "J86", "KQ", "AK93"), "3♠", "1♥", "Dbl", "Pas", "1♠", "Pas");
            T("V19 (1H)-Dbl-P-1S-P-? 14hcp 4 maca -> 2S (asgari)", TestHelpers.ElOlustur("AJ53", "J86", "KQ", "8K93".Replace("8K","K8")), "2♠", "1♥", "Dbl", "Pas", "1♠", "Pas");

            sonucEkle("");
            sonucEkle("  ÖZET: ✅ " + _gecen + " geçti, ❌ " + _kalan + " başarısız");
            sonucEkle("════════════════════════════════════════");
            return _kalan;
        }

        private static void TZ(string ad, List<Card> el, string beklenen, params string[] gecmis)
        {
            var an = OrtaklikAnlasmasi.Varsayilan();
            an.ZayifIki = true;
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            durum.Anlasma = an;
            var motor = new IhaleMotoru(an);
            string r = motor.TeklifVer(durum);
            bool gecti = r == beklenen;
            if (gecti) _gecen++; else _kalan++;
            _yaz((gecti ? "✅ " : "❌ ") + ad + " → " + r + (gecti ? "" : "   [beklenen: " + beklenen + "]"));
        }

        private static void T(string ad, List<Card> el, string beklenen, params string[] gecmis)
        {
            var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
            var motor = new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan());
            string r = motor.TeklifVer(durum);
            bool gecti = r == beklenen;
            if (gecti) _gecen++; else _kalan++;
            _yaz((gecti ? "✅ " : "❌ ") + ad + " → " + r + (gecti ? "" : "   [beklenen: " + beklenen + "]"));
        }
    }
}
