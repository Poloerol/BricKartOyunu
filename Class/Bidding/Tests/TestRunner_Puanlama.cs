using System;
using System.Collections.Generic;
using BricKartOyunu.Class;

namespace BricKartOyunu.Tests
{
    /// <summary>Rubber, Chicago Rubber ve Chicago Duplicate puanlama testleri (Class/KlasikSkor.cs).</summary>
    public static class TestRunner_Puanlama
    {
        private static int _gecen, _kalan;
        private static Action<string> _yaz;

        public static int TumTestleriCalistir(Action<string> sonucEkle)
        {
            _gecen = 0; _kalan = 0; _yaz = sonucEkle;
            sonucEkle("════════════════════════════════════════");
            sonucEkle("  PUANLAMA TESTLERİ (Rubber / Chicago)");
            sonucEkle("════════════════════════════════════════");

            // ── Rubber ──
            var r = Hesapla(PuanTuru.Rubber, G(1, true, 4, "♠", 10));
            E("PU1 Rubber: 4♠ K-G tam yapıldı -> çizgi altı yok, oyun 1, toplam 120", r.NsOyun == 1 && r.NsAlt == 0 && r.NsToplam == 120, r);
            E("PU2 Rubber: oyundan sonra K-G zonda, D-B değil", r.NsZon && !r.EwZon, r);

            r = Hesapla(PuanTuru.Rubber, G(1, true, 3, "NT", 10));
            E("PU3 Rubber: 3NT+1 -> 100 altı + 30 üstü, oyun 1, toplam 130", r.NsOyun == 1 && r.NsToplam == 130, r);

            r = Hesapla(PuanTuru.Rubber, G(1, true, 2, "♠", 8), G(2, true, 2, "♥", 8));
            E("PU4 Rubber: 2♠ ve 2♥ yapıldı (60+60) -> parsiyel taşınır, ikinci elde oyun", r.NsOyun == 1 && r.NsAlt == 0 && r.NsToplam == 120, r);

            r = Hesapla(PuanTuru.Rubber, G(1, true, 2, "♠", 8));
            E("PU5 Rubber: tek parsiyel -> çizgi altı 60, oyun yok, zon yok", r.NsAlt == 60 && r.NsOyun == 0 && !r.NsZon, r);

            r = Hesapla(PuanTuru.Rubber, G(1, true, 4, "♠", 10), G(2, true, 4, "♠", 10));
            E("PU6 Rubber: iki oyun, rakipte oyun yok -> +700, toplam 120+120+700=940, rubber bitti", r.Biten == 1 && r.NsToplam == 940 && r.NsOyun == 0 && !r.NsZon, r);

            r = Hesapla(PuanTuru.Rubber, G(1, false, 4, "♠", 10), G(2, true, 4, "♠", 10), G(3, true, 4, "♠", 10));
            E("PU7 Rubber: rakipte bir oyun var -> rubber bonusu 500", r.Biten == 1 && r.NsToplam == 120 + 120 + 500 && r.EwToplam == 120, r);

            r = Hesapla(PuanTuru.Rubber, G(1, true, 4, "♠", 8));
            E("PU8 Rubber: 4♠ iki batık zonsuz -> D-B çizgi üstü 100", r.EwToplam == 100 && r.NsToplam == 0, r);

            r = Hesapla(PuanTuru.Rubber, G(1, true, 4, "♠", 10), G(2, true, 4, "♠", 9));
            E("PU9 Rubber: zondaki taraf 4♠ bir batık -> D-B 100 (zonda), K-G oyunu korur", r.EwToplam == 100 && r.NsOyun == 1, r);

            r = Hesapla(PuanTuru.Rubber, G(1, true, 6, "NT", 12));
            E("PU10 Rubber: 6NT tam -> 190 altı + 500 slam bonusu üstü", r.NsOyun == 1 && r.NsToplam == 190 + 500, r);

            // ── Chicago zonları (dağıtıcı sırası K, D, G, B) ──
            var tek = Hesapla(PuanTuru.ChicagoRubber, new List<BordGirdisi>(), Player.Kuzey);
            E("PU11 Chicago: 1. bord zonsuz", !tek.NsZon && !tek.EwZon, tek);
            tek = Hesapla(PuanTuru.ChicagoRubber, new List<BordGirdisi> { P(1, Player.Kuzey) }, Player.Dogu);
            E("PU12 Chicago: 2. bord dağıtıcı Doğu -> D-B zonda", !tek.NsZon && tek.EwZon, tek);
            tek = Hesapla(PuanTuru.ChicagoRubber, new List<BordGirdisi> { P(1, Player.Kuzey), P(2, Player.Dogu) }, Player.Guney);
            E("PU13 Chicago: 3. bord dağıtıcı Güney -> K-G zonda", tek.NsZon && !tek.EwZon, tek);
            tek = Hesapla(PuanTuru.ChicagoRubber, new List<BordGirdisi> { P(1, Player.Kuzey), P(2, Player.Dogu), P(3, Player.Guney) }, Player.Bati);
            E("PU14 Chicago: 4. bord herkes zonda", tek.NsZon && tek.EwZon, tek);
            tek = Hesapla(PuanTuru.ChicagoRubber, new List<BordGirdisi> { P(1, Player.Kuzey), P(2, Player.Dogu), P(3, Player.Guney), P(4, Player.Bati) }, Player.Kuzey);
            E("PU15 Chicago: 5. bord yeni grup, zonsuz", !tek.NsZon && !tek.EwZon, tek);

            // ── Chicago Rubber ──
            r = HesaplaC(PuanTuru.ChicagoRubber, G(1, true, 3, "NT", 9));
            E("PU16 Chicago Rubber: 3NT zonsuz -> 100 + 300 oyun bonusu = 400", r.NsToplam == 400 && r.NsAlt == 0, r);

            r = HesaplaC(PuanTuru.ChicagoRubber, G(1, true, 2, "♠", 8), P(2, Player.Dogu), P(3, Player.Guney), P(4, Player.Bati));
            E("PU17 Chicago Rubber: dördüncü bordun sonunda parsiyeli olan taraf +100 -> 160", r.NsToplam == 160 && r.Biten == 1, r);

            r = HesaplaC(PuanTuru.ChicagoRubber, G(1, true, 2, "♠", 8), G(2, true, 2, "♥", 8));
            E("PU18 Chicago Rubber: 60 + 60 parsiyel taşınır, ikinci bordda oyun (zonsuz) 120 + 300", r.NsToplam == 120 + 300, r);

            r = HesaplaC(PuanTuru.ChicagoRubber, P(1, Player.Kuzey), P(2, Player.Dogu), G(3, true, 4, "♠", 10));
            E("PU19 Chicago Rubber: 3. bordda K-G zonda 4♠ -> 120 + 500", r.NsToplam == 120 + 500, r);

            // ── Chicago Duplicate ──
            r = HesaplaC(PuanTuru.ChicagoDuplicate, G(1, true, 4, "♠", 10));
            E("PU20 Chicago Duplicate: 4♠ zonsuz -> 420", r.NsToplam == 420 && r.EwToplam == -420, r);

            r = HesaplaC(PuanTuru.ChicagoDuplicate, P(1, Player.Kuzey), G(2, false, 4, "♠", 10));
            E("PU21 Chicago Duplicate: 2. bord dağıtıcı Doğu -> D-B zonda 4♠ = 620", r.EwToplam == 620 && r.NsToplam == -620, r);

            // ── Oturum deposu ──
            PuanOturumu.Temizle();
            PuanOturumu.Kaydet(PuanTuru.Rubber, G(1, true, 4, "♠", 10));
            bool ns, ew;
            PuanOturumu.Zon(PuanTuru.Rubber, 2, Player.Dogu, out ns, out ew);
            E("PU22 Oturum: oyundan sonra 2. bord K-G zonda", ns && !ew, null);
            PuanOturumu.Zon(PuanTuru.Rubber, 1, Player.Kuzey, out ns, out ew);
            E("PU23 Oturum: 1. bord kendi sonucundan etkilenmez (zonsuz)", !ns && !ew, null);
            PuanOturumu.Kaydet(PuanTuru.Rubber, G(1, true, 2, "♠", 8));   // tekrar oynanan bord
            E("PU24 Oturum: aynı bord tekrar kaydedilirse üzerine yazılır", PuanOturumu.BordSayisi == 1, null);
            PuanOturumu.Zon(PuanTuru.ChicagoRubber, 1, Player.Kuzey, out ns, out ew);
            E("PU25 Oturum: oyun türü değişince liste sıfırlanır", PuanOturumu.BordSayisi == 0, null);
            PuanOturumu.Temizle();

            sonucEkle("  Puanlama: " + _gecen + " geçti, " + _kalan + " kaldı");
            return _kalan;
        }

        private static BordGirdisi G(int no, bool ns, int seviye, string koz, int love)
        {
            return new BordGirdisi { BordNo = no, DeklaranNS = ns, Seviye = seviye, Koz = koz, AlinanLove = love, Dagitici = Player.Kuzey };
        }

        private static BordGirdisi P(int no, Player dagitici)
        {
            return new BordGirdisi { BordNo = no, Pas = true, Dagitici = dagitici };
        }

        private static PuanDurumu Hesapla(PuanTuru tur, params BordGirdisi[] g)
        {
            return KlasikSkor.Hesapla(tur, new List<BordGirdisi>(g), Player.Kuzey);
        }

        private static PuanDurumu Hesapla(PuanTuru tur, List<BordGirdisi> g, Player siradaki)
        {
            return KlasikSkor.Hesapla(tur, g, siradaki);
        }

        // Chicago testlerinde bordlara dağıtıcı K, D, G, B sırasıyla atanır
        private static PuanDurumu HesaplaC(PuanTuru tur, params BordGirdisi[] g)
        {
            Player[] sira = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            for (int i = 0; i < g.Length; i++) g[i].Dagitici = sira[i % 4];
            return KlasikSkor.Hesapla(tur, new List<BordGirdisi>(g), sira[g.Length % 4]);
        }

        private static void E(string ad, bool dogru, PuanDurumu d)
        {
            if (dogru) { _gecen++; _yaz("  ✓ " + ad); }
            else
            {
                _kalan++;
                _yaz("  ✗ " + ad + (d != null ? "   [K-G " + d.NsToplam + " / D-B " + d.EwToplam + ", alt " + d.NsAlt + "/" + d.EwAlt + ", oyun " + d.NsOyun + "/" + d.EwOyun + ", biten " + d.Biten + "]" : ""));
            }
        }
    }
}
