using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// TEMEL MANTIK (fallback) — doğal ihale motoru.
    ///
    /// Hiçbir özel konvansiyon uymadığında çalışır. Rastgele "bir sonraki teklif" yerine
    /// her adımda şu soruyu cevaplar: "Ortaklığın tahmini toplam gücü ve fiti ile
    /// hangi kontratta oynamalıyız, şu an hangi seviyedeyiz?"
    ///
    ///   1. Açılış (kimse açmadıysa)
    ///   2. Rakip açtı, ortak susuyor → doğal müdahale
    ///   3. Ortaklık: tahmini güç (IhaleKoruma.PartnerAraligi) + fit → hedef seviye
    ///        - hedef seviyeye ulaşıldıysa Pas
    ///        - hedefin altındaysa hedef kontratı (veya en ucuz yasal teklifi) ver
    ///   4. Rekabet: Toplam Kart Kanunu (LOTT) ile en fazla fit kadar yarış
    ///   5. Zorlayıcı durumlar (2♣ güçlü açılış, açıcıya yeni renk): Pas yasak
    /// </summary>
    public class TemelMantik : IKonvansiyon
    {
        public string Ad => "Temel Mantık (Fallback)";
        public string Aciklama => "Evrensel briç kuralları (fit, NT, puan)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 999;

        public bool UygunMu(IhaleDurumu durum) => true;

        private static readonly string[] Renkler = { "Sinek", "Karo", "Kupa", "Maça" };

        private static int RenkSira(string koz)
        {
            switch (koz)
            {
                case "Sinek": return 0;
                case "Karo": return 1;
                case "Kupa": return 2;
                case "Maça": return 3;
                case "NT": return 4;
            }
            return -1;
        }

        private static bool Major(string r) => r == "Kupa" || r == "Maça";

        // ═════════════════════════════════════════════════════════════════
        // ANA GİRİŞ
        // ═════════════════════════════════════════════════════════════════
        public string TeklifVer(IhaleDurumu d)
        {
            var el = d?.AktifOyuncuEli;
            if (el == null || el.Count != 13) return "Pas";

            var g = d.Gecmis ?? new List<IhaleHamlesi>();
            var gercek = g.Where(h => h.GercekTeklifMi).ToList();
            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // 1. AÇILIŞ
            if (gercek.Count == 0) return Acilis(el, hp, dengeli, g.Count);

            bool ortakKonustu = gercek.Any(h => h.Oyuncu == d.Partner);
            bool benKonustum = gercek.Any(h => h.Oyuncu == d.AktifOyuncu);
            bool rakipKonustu = gercek.Any(h => d.Rakipler.Contains(h.Oyuncu));

            // 2. Rakip açtı, bizim taraf henüz susuyor → doğal müdahale
            if (!ortakKonustu && !benKonustum && rakipKonustu)
                return Mudahale(d, el, hp, gercek);

            // 3. Genel yerleştirme motoru
            return Yerlestir(d, el, hp, dengeli, gercek);
        }

        // ═════════════════════════════════════════════════════════════════
        // 1. AÇILIŞ
        // ═════════════════════════════════════════════════════════════════
        private string Acilis(List<Card> el, int hp, bool dengeli, int onceki)
        {
            if (hp >= 22) return "2♣";
            if (hp >= 20 && hp <= 21 && dengeli) return "2NT";
            if (hp >= 15 && hp <= 17 && dengeli && ElDegerlendirici.RenkUzunlugu(el, "Maça") < 6 && ElDegerlendirici.RenkUzunlugu(el, "Kupa") < 6) return "1NT";
            if (ElDegerlendirici.AcilisGucuVarMi(el)) return EnUzunRengiAc(el);   // 12+ HCP veya 11 HCP + 20 kuralı
            // 3. / 4. el: 11 HCP + güçlü 5'li renk (Rule of 20 benzeri)
            if (onceki >= 2 && hp == 11 && EnUzunUzunluk(el) >= 5) return EnUzunRengiAc(el);
            return "Pas";
        }

        private static int EnUzunUzunluk(List<Card> el) =>
            Renkler.Max(r => ElDegerlendirici.RenkUzunlugu(el, r));

        private string EnUzunRengiAc(List<Card> el)
        {
            int ma = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int ku = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int ka = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int si = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            if (ma >= 5 && ma >= ku) return "1♠";
            if (ku >= 5) return "1♥";
            if (ma >= 5) return "1♠";
            if (ka >= 4 && ka >= si) return "1♦";
            return "1♣";
        }

        // ═════════════════════════════════════════════════════════════════
        // 2. DOĞAL MÜDAHALE (rakip açtı, ortak henüz yok)
        // ═════════════════════════════════════════════════════════════════
        private string Mudahale(IhaleDurumu d, List<Card> el, int hp, List<IhaleHamlesi> gercek)
        {
            var son = gercek.Last();
            if (!d.Rakipler.Contains(son.Oyuncu)) return "Pas";   // rakip desteklendi/yükseldi vs.
            int sev = son.Seviye;
            string rk = son.Koz;
            if (sev >= 3 || hp < 8) return "Pas";

            // En iyi 5+ renk (rakibin rengi hariç)
            string en = null; int enUz = 0;
            foreach (var r in Renkler)
            {
                if (r == rk) continue;
                int u = ElDegerlendirici.RenkUzunlugu(el, r);
                if (u >= 5 && (u > enUz || (u == enUz && RenkSira(r) > RenkSira(en)))) { en = r; enUz = u; }
            }
            if (en == null) return "Pas";
            int lvl = Cheapest(d, en);
            if (lvl == 1 && hp >= 8) return "1" + KozYardimcisi.KozSembolu(en);
            if (lvl == 2 && hp >= 11 && enUz >= 5 && IyiRenk(el, en)) return "2" + KozYardimcisi.KozSembolu(en);
            return "Pas";
        }

        private static bool IyiRenk(List<Card> el, string r) =>
            ElDegerlendirici.RenkUzunlugu(el, r) >= 6 || ElDegerlendirici.IyiRenk(el, r);

        // ═════════════════════════════════════════════════════════════════
        // 3. YERLEŞTİRME MOTORU
        // ═════════════════════════════════════════════════════════════════
        private string Yerlestir(IhaleDurumu d, List<Card> el, int hp, bool dengeli, List<IhaleHamlesi> gercek)
        {
            var son = gercek.Last();
            bool sonBizim = son.Oyuncu == d.Partner || son.Oyuncu == d.AktifOyuncu;
            bool sonOrtak = son.Oyuncu == d.Partner;
            int cur = son.Seviye;

            var ar = IhaleKoruma.PartnerAraligi(d);
            var rakipRenkleri = gercek.Where(h => d.Rakipler.Contains(h.Oyuncu) && h.Koz != "NT")
                                      .Select(h => h.Koz).Distinct().ToList();

            // Ortağın açılışı NT mi? (1NT/2NT sistemi: dar aralık, doğrudan HCP kuralları)
            var ilk = gercek.First();
            bool ortakNT = ilk.Oyuncu == d.Partner && ilk.Koz == "NT" && !gercek.Any(h => h.Oyuncu == d.AktifOyuncu);
            bool ortak2C = ilk.Oyuncu == d.Partner && ilk.Teklif == "2♣";
            bool benNTAcildi = ilk.Oyuncu == d.AktifOyuncu && ilk.Koz == "NT";

            // Ben 1NT açtım, ortak minör transferi yaptı → transferi tamamla
            if (benNTAcildi && ilk.Teklif == "1NT" && sonOrtak && d.Anlasma != null && d.Anlasma.MinorTransfer &&
                gercek.Count(h => h.Oyuncu == d.AktifOyuncu) == 1 && gercek.Count(h => h.Oyuncu == d.Partner) == 1 &&
                !gercek.Any(h => d.Rakipler.Contains(h.Oyuncu)))
            {
                if (son.Teklif == "2♠") return Seviye(d, "Sinek", 3);
                if (son.Teklif == "2NT") return Seviye(d, "Karo", 3);
            }

            // Ben minör transferi yaptım (1NT – 2♠/2NT), ortak tamamladı → güce göre devam
            {
                var benim = gercek.Where(h => h.Oyuncu == d.AktifOyuncu).ToList();
                var ortagim = gercek.Where(h => h.Oyuncu == d.Partner).ToList();
                if (ilk.Oyuncu == d.Partner && ilk.Teklif == "1NT" && sonOrtak &&
                    benim.Count == 1 && ortagim.Count == 2 &&
                    !gercek.Any(h => d.Rakipler.Contains(h.Oyuncu)) &&
                    ((benim[0].Teklif == "2♠" && son.Teklif == "3♣") || (benim[0].Teklif == "2NT" && son.Teklif == "3♦")))
                {
                    // Ortak 15-17: 10+ HP ile oyun (3NT), daha zayıfsa transferde dur
                    return hp >= 10 ? Seviye(d, "NT", 3) : "Pas";
                }
            }

            // Ortak 2♣ (güçlü) açtı, ben ilk kez cevap veriyorum
            if (ortak2C && !gercek.Any(h => h.Oyuncu == d.AktifOyuncu))
                return Cevap2C(d, el, hp, dengeli, rakipRenkleri);

            // Rakip son sözü söylemiş → rekabet
            if (!sonBizim)
            {
                string rk = Rekabet(d, el, hp, dengeli, ar, son, rakipRenkleri, gercek);
                if (rk == "Pas") rk = RekabetYeniRenk(d, el, hp, rakipRenkleri, gercek);
                return rk;
            }

            // Ortak rakip açılışına 2NT ile müdahale etti (Unusual: iki minör) → uzun minörü seç
            if (sonOrtak && son.Teklif == "2NT" && !gercek.Any(h => h.Oyuncu == d.AktifOyuncu) &&
                gercek.First().Oyuncu != d.Partner && gercek.First().Oyuncu != d.AktifOyuncu)
            {
                int ka = ElDegerlendirici.RenkUzunlugu(el, "Karo"), si = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
                string m = ka > si ? "Karo" : "Sinek";
                int c3 = Cheapest(d, m);
                return c3 <= 3 ? KozYardimcisi.TeklifOlustur(c3, m) : "Pas";
            }

            // Ortak, benim majörümü 3. seviyeye sıçrayarak destekledi (davet: 16-18 HP) →
            // kendi gücümle toplam 24+ ise oyunu (4M) kabul et: 1♣–1♥–(1♠)–3♥ sonrası 8 HP → 4♥.
            if (sonOrtak && son.Koz != "NT" && Major(son.Koz) && son.Seviye == 3 && cur == 3 &&
                gercek.Any(h => h.Oyuncu == d.AktifOyuncu && h.Koz == son.Koz) &&
                gercek.IndexOf(son) > 0 && ElDegerlendirici.RenkUzunlugu(el, son.Koz) >= 3 &&
                hp + ar.Min >= 24)
                return Seviye(d, son.Koz, 4);

            // ── Fit bul ──
            string fit = FitBul(d, el, gercek, rakipRenkleri, out int fitToplam);

            // ── Ortak NT açtı (1NT/2NT) ve bizde doğal bir devam gerekiyor ──
            if (ortakNT)
            {
                if (cur >= 3) return "Pas";
                if (ilk.Seviye == 1)
                {
                    if (fit != null && Major(fit) && fitToplam >= 8 && hp >= 10) return Seviye(d, fit, 4);
                    if (dengeli && hp >= 10) return Seviye(d, "NT", 3);
                    if (dengeli && hp >= 8 && cur == 1) return Seviye(d, "NT", 2);
                    return "Pas";
                }
                if (ilk.Seviye == 2) { if (hp >= 5 && cur <= 2) return Seviye(d, "NT", 3); return "Pas"; }
            }

            // ── Tahmini toplam güç ──
            double kismi = ar.Min + 0.35 * (ar.Max - ar.Min);
            double est = (fit != null && fit != "NT" ? ElDegerlendirici.ToplamPuan(el, fit) : hp) + kismi;
            if (ortak2C) est = Math.Max(est, 26);

            bool dar = false;   // (dar aralık ayarı denendi, ölçümde fayda sağlamadı)
            bool zorunlu = Zorunlu(d, el, gercek, ortak2C);
            if (fit != null && fit != "NT" && sonOrtak && son.Koz != fit && son.Koz != "NT" && son.Seviye >= 3 && Cheapest(d, fit) <= 5)
                zorunlu = true;   // ortak kontrol (cue) teklifi verdi: fit renginde oyuna git

            // Kendi 4NT (RKCB) sorumun cevabı geldi: toplam kilit kartı say, slam karar ver.
            if (sonOrtak && son.Seviye == 5 && son.Koz != "NT" &&
                gercek.Any(h => h.Oyuncu == d.AktifOyuncu && h.Teklif == "4NT"))
            {
                string rk = RkcbKarari(d, el, son, est);
                if (rk != null) return rk;
            }

            // Ortak 4. seviyede kontrol (cue) teklifi verdi ve majör koz anlaşıldı (ör. 1♠–2♦–2♠–3♥–3♠–4♦):
            // Pas yasak. Slam ilgisine karşılık: 13+ HP ile 4NT (RKCB), değilse 4M ile kapat.
            {
                string ak = d.AnlasilanKoz();
                if (sonOrtak && ak != null && Major(ak) && son.Seviye == 4 && son.Koz != "NT" && son.Koz != ak &&
                    RenkSira(son.Koz) < RenkSira(ak) &&
                    !gercek.Any(h => d.Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi) &&
                    !gercek.Any(h => h.Oyuncu == d.AktifOyuncu && h.Teklif == "4NT"))
                    return hp >= 13 ? "4NT" : Seviye(d, ak, 4);
            }

            // Tek taraflı koz: ortak majör rengini ≥2 kez söyledi (6+), bende o renkte onur var ve
            // güç slam bölgesinde → 4. seviyede kontrol teklifi (4M ile kapatma, slam ara).
            if (sonOrtak && est >= 31 && cur <= 3)
            {
                string cue = KontrolTeklifi(d, el, gercek);
                if (cue != null) return cue;
            }

            // Ters çevrilmiş minör (1m-2m): açıcı en az bir tur daha konuşmalı
            if (IMZorunlu(d, gercek)) return IMDevam(d, el, hp, dengeli, rakipRenkleri);

            // ── Fit varsa: fit rengiyle hedef ──
            if (fit != null && fit != "NT")
            {
                int T = HedefFit(fit, est, dengeli && NTOlur(el, rakipRenkleri), dar, fitToplam);
                if (T == -1) // 3NT tercih
                    return cur < 3 ? Seviye(d, "NT", 3) : (zorunlu ? EnUcuz(d, fit) : "Pas");
                // Ortağın son teklifi fit renginde ve hedefin altına ulaştık mı
                if (cur >= T && !(zorunlu && cur < OyunSeviyesi(fit))) return "Pas";
                int ucuz = Cheapest(d, fit);
                if (ucuz > T && !zorunlu) return "Pas";
                int hedef = Math.Max(T, ucuz);
                return Seviye(d, fit, Math.Min(hedef, 6));
            }

            // ── Fit yok: NT / uzun renk ──
            return FitsizDevam(d, el, hp, dengeli, est, cur, rakipRenkleri, gercek, zorunlu, benNTAcildi, dar);
        }

        /// <summary>
        /// Sorduğum 4NT'nin cevabı (5. seviye) geldi. Kilit kart toplamıma göre 6M / 5M / Pas.
        /// RKCB 1430 (varsayılan): 5♣=1 ya da 4, 5♦=0 ya da 3, 5♥=2 (Q yok), 5♠=2 (Q var).
        /// Belirsiz cevapta toplamı 5'i aşmayan yorum seçilir.
        /// </summary>
        private string RkcbKarari(IhaleDurumu d, List<Card> el, IhaleHamlesi cevap, double est)
        {
            string koz = d.AnlasilanKoz();
            if (koz == null || koz == "NT" || !Major(koz)) return null;
            bool k1430 = d.Anlasma == null || d.Anlasma.RKCB1430;
            int ben = ElDegerlendirici.KilitKartSayisi(el, koz);

            int a, b;   // olası kilit kart sayıları
            switch (cevap.Teklif)
            {
                case "5♣": a = k1430 ? 1 : 0; b = k1430 ? 4 : 3; break;
                case "5♦": a = k1430 ? 0 : 1; b = k1430 ? 3 : 4; break;
                case "5♥": case "5♠": a = b = 2; break;
                default: return null;
            }
            int ort = (ben + b <= 5) ? b : a;
            int toplam = ben + ort;

            // Cevap 5M'yi geçtiyse (ör. koz ♥, cevap 5♠) 6M dışında seçenek yok
            int ucuz5 = Cheapest(d, koz);
            if (toplam >= 5) return Seviye(d, koz, 6);
            if (toplam == 4) return est >= 32 ? Seviye(d, koz, 6) : (ucuz5 <= 5 ? Seviye(d, koz, 5) : "Pas");
            return ucuz5 <= 5 ? Seviye(d, koz, 5) : "Pas";
        }

        /// <summary>
        /// Ortak bir majörü en az iki kez söylediyse (6+ kart), ben o rengi hiç söylemedim ve o renkte
        /// A/K/Q taşıyorsam koz kabul edilir. 4M'nin altında, ilk turda kontrolüm olan (A veya
        /// kısalık) en ucuz renkte 4. seviye kontrol teklifi döndürür; yoksa null.
        /// </summary>
        private string KontrolTeklifi(IhaleDurumu d, List<Card> el, List<IhaleHamlesi> gercek)
        {
            string koz = null;
            foreach (var r in new[] { "Maça", "Kupa" })
            {
                int ort = gercek.Count(h => h.Oyuncu == d.Partner && h.Koz == r);
                bool benBu = gercek.Any(h => h.Oyuncu == d.AktifOyuncu && h.Koz == r);
                if (ort >= 2 && !benBu && el.Any(c => c.Suit == r && c.Value >= 12)) { koz = r; break; }
                // Ortak majörde sıçramalı yeni renk (1♣–2♠) yaptı, bende 3+ kart destek var
                if (!benBu && IhaleDurumu.SicramaliMajor(gercek, d.Partner, r) &&
                    ElDegerlendirici.RenkUzunlugu(el, r) >= 3) { koz = r; break; }
            }
            if (koz == null) return null;

            // Daha önce 4. seviyede kontrol teklifi vermişsem tekrarlama
            if (gercek.Any(h => h.Oyuncu == d.AktifOyuncu && h.Seviye == 4 && h.Koz != "NT")) return null;

            foreach (var r in Renkler)
            {
                if (r == koz || RenkSira(r) > RenkSira(koz)) continue;
                int uz = ElDegerlendirici.RenkUzunlugu(el, r);
                bool kontrol = uz == 0 || el.Any(c => c.Suit == r && c.Value == 14);
                if (!kontrol) continue;
                if (Cheapest(d, r) != 4) continue;
                return Seviye(d, r, 4);
            }
            // Sıçramalı majöre (1♣–2♠) 4. seviyede kontrol teklifi mümkün değilse: slam ilgisiyle
            // 3M (destek, forsing) göster; 4M ile kapatma. Ortak 4NT/kontrol ile devam eder.
            if (IhaleDurumu.SicramaliMajor(gercek, d.Partner, koz) && Cheapest(d, koz) == 3)
                return Seviye(d, koz, 3);
            return null;
        }

        // ── Ters çevrilmiş minör (1m–2m): açıcı rebid yapmak zorunda ──
        private bool IMZorunlu(IhaleDurumu d, List<IhaleHamlesi> gercek)
        {
            var ilk = gercek.First();
            if (ilk.Oyuncu != d.AktifOyuncu || ilk.Seviye != 1 || (ilk.Koz != "Karo" && ilk.Koz != "Sinek")) return false;
            if (gercek.Count(h => h.Oyuncu == d.AktifOyuncu) != 1) return false;
            var son = gercek.Last();
            return son.Oyuncu == d.Partner && son.Koz == ilk.Koz && son.Seviye == 2 &&
                   !gercek.Any(h => d.Rakipler.Contains(h.Oyuncu));
        }

        private string IMDevam(IhaleDurumu d, List<Card> el, int hp, bool dengeli, List<string> rakip)
        {
            if (dengeli && NTOlur(el, rakip)) return Seviye(d, "NT", hp >= 15 ? 3 : 2);
            // 4'lü majör
            foreach (var r in new[] { "Maça", "Kupa" })
                if (ElDegerlendirici.RenkUzunlugu(el, r) >= 4 && Cheapest(d, r) <= 2) return Seviye(d, r, Cheapest(d, r));
            int c = Cheapest(d, "NT");
            return c <= 3 ? KozYardimcisi.TeklifOlustur(Math.Max(c, 2), "NT") : "Pas";
        }

        // ── 2♣ açılışına ilk cevap: 2♦ bekleme; 8+ HCP ve iyi renk/dengeli ise pozitif ──
        private string Cevap2C(IhaleDurumu d, List<Card> el, int hp, bool dengeli, List<string> rakipRenkleri)
        {
            if (hp >= 8)
            {
                foreach (var r in new[] { "Maça", "Kupa", "Karo" })
                    if (!rakipRenkleri.Contains(r) && ElDegerlendirici.RenkUzunlugu(el, r) >= 5 &&
                        ElDegerlendirici.IyiRenk(el, r) && Cheapest(d, r) <= 2)
                        return Seviye(d, r, Cheapest(d, r));
                if (dengeli && NTOlur(el, rakipRenkleri) && hp >= 10) return Seviye(d, "NT", 2);
            }
            int c = Cheapest(d, "Karo");
            return c <= 2 ? KozYardimcisi.TeklifOlustur(2, "Karo") : "Pas";
        }

        // ── Fitsiz devam: NT yerleştirme, uzun renk tekrarı, yeni renk ──
        private string FitsizDevam(IhaleDurumu d, List<Card> el, int hp, bool dengeli, double est, int cur,
                                   List<string> rakipRenkleri, List<IhaleHamlesi> gercek, bool zorunlu, bool benNTAcildi, bool dar)
        {
            // İlk cevap: ortak (NT/2♣ dışında) açtı ve ben henüz konuşmadım → 1 seviyede 4+ kartlık
            // majör varsa NT davetinden/oyun gücünden ÖNCE o majörü göster (fit aranır).
            // Örn. 1♣ - ? ile 5♠ + 11 HP: 2NT değil 1♠.
            if (!gercek.Any(h => h.Oyuncu == d.AktifOyuncu) && gercek.Count > 0 &&
                gercek.First().Oyuncu == d.Partner && gercek.First().Koz != "NT" &&
                gercek.First().Teklif != "2♣" && hp >= 6)
            {
                int sL = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int hL = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                string major = null;
                if (sL >= 4 || hL >= 4)
                {
                    if (sL > hL) major = "Maça";
                    else if (hL > sL) major = "Kupa";
                    else major = sL >= 5 ? "Maça" : "Kupa";   // 5-5: ♠, 4-4: ♥ (en ucuzdan)
                }
                if (major != null && !rakipRenkleri.Contains(major) && Cheapest(d, major) == 1)
                    return Seviye(d, major, 1);
            }

            bool nt = dengeli || EnUzunUzunluk(el) <= 5;
            bool ntOlur = NTOlur(el, rakipRenkleri);
            // Ortak rakibin rengi söylendikten SONRA NT dediyse o rengi durdurduğunu vaat eder:
            // 1♣–1♥–(1♠)–2NT'de ortağın ♠ durdurucusu var, bende olmasa da 3NT oynanır.
            if (!ntOlur)
            {
                var ortakNTh = gercek.FirstOrDefault(h => h.Oyuncu == d.Partner && h.Koz == "NT" && h.Seviye >= 2);
                if (ortakNTh != null)
                {
                    int ix = gercek.IndexOf(ortakNTh);
                    ntOlur = rakipRenkleri.All(r =>
                        ElDegerlendirici.StopperVar(el, r) ||
                        gercek.Take(ix).Any(h => d.Rakipler.Contains(h.Oyuncu) && h.Koz == r));
                }
            }

            // Ortağın aralığı dar (örn. 1♦–1♥–2NT = 18-19) ve kendi elimle toplam en az 25 HP:
            // tahmin eşiğine takılmadan 3NT'ye taşı (18+9=27 HP'de Pas demek hatadır).
            var ortAr = IhaleKoruma.PartnerAraligi(d);
            if ((ortAr.Max - ortAr.Min) <= 3 && ortAr.Min + hp >= 25 && ntOlur && nt && cur <= 2 &&
                gercek.Any(h => h.Oyuncu == d.Partner && h.Koz == "NT" && h.Seviye == 2) &&
                !gercek.Any(h => d.Rakipler.Contains(h.Oyuncu) && h.Seviye >= 2))
                return Seviye(d, "NT", 3);

            // 2/1 sonrası ikinci renk: ortak açtı, ben yeni renkle (2. seviye) cevap verdim, ortak kendi
            // açış rengini tekrarladı (ör. 1♠–2♦–2♠). Oyun gücündeyim ve henüz söylemediğim 4+ kartlık
            // bir renk var → 3NT ile ihaleyi kapatmadan o rengi göster (fit/slam aranır).
            {
                var benim = gercek.Where(h => h.Oyuncu == d.AktifOyuncu).ToList();
                var ortagim = gercek.Where(h => h.Oyuncu == d.Partner).ToList();
                if (benim.Count == 1 && ortagim.Count == 2 && gercek.First().Oyuncu == d.Partner &&
                    gercek.First().Koz != "NT" && benim[0].Koz != "NT" && benim[0].Seviye == 2 &&
                    ortagim[1].Koz == ortagim[0].Koz && ortagim[1].Seviye == 2 &&
                    gercek.Last().Oyuncu == d.Partner && !gercek.Any(h => d.Rakipler.Contains(h.Oyuncu)) &&
                    est >= 26 && hp >= 12)
                {
                    string ikinci = null; int ikinciUz = 3;
                    foreach (var r in new[] { "Maça", "Kupa", "Sinek", "Karo" })
                    {
                        if (r == ortagim[0].Koz || r == benim[0].Koz) continue;
                        int rUz = ElDegerlendirici.RenkUzunlugu(el, r);
                        if (rUz > ikinciUz || (rUz == ikinciUz && rUz >= 4 && ikinci != null && Major(r) && !Major(ikinci))) { ikinci = r; ikinciUz = rUz; }
                    }
                    if (ikinci != null && ikinciUz >= 4 && Cheapest(d, ikinci) <= 3)
                        return Seviye(d, ikinci, Cheapest(d, ikinci));
                }
            }

            // Zorlayıcı durumda 6+ kartlık kendi rengim var ve ortağın son gösterdiği renkte durdurucum yok:
            // 3NT yerine rengi tekrarla (ör. 1♠–2♦–2♠–3♥ sonrası ♠AQ10874, ♥94 → 3♠).
            if (zorunlu && cur <= 3 && gercek.Last().Oyuncu == d.Partner && gercek.Last().Koz != "NT")
            {
                string ur = EnUzunRenk(el);
                if (ElDegerlendirici.RenkUzunlugu(el, ur) >= 6 && ur != gercek.Last().Koz &&
                    gercek.Any(h => h.Oyuncu == d.AktifOyuncu && h.Koz == ur) &&
                    !ElDegerlendirici.StopperVar(el, gercek.Last().Koz) &&
                    !rakipRenkleri.Contains(ur) && Cheapest(d, ur) <= 3)
                    return Seviye(d, ur, Cheapest(d, ur));
            }

            // Oyun gücü: 3NT (veya 6+ majörde 4)
            double oyunEsik = dar ? 26 : 27.5, davetEsik = dar ? 24 : 25.5;
            if (est >= oyunEsik)
            {
                string uzun = EnUzunRenk(el);
                if (ElDegerlendirici.RenkUzunlugu(el, uzun) >= 6 && Major(uzun) && cur <= 4) return Seviye(d, uzun, 4);
                if (ntOlur && cur <= 3) return Seviye(d, "NT", 3);
                if (zorunlu) return EnUcuz(d, UzunKoz(el, gercek, d));
            }
            if (est >= davetEsik && est < oyunEsik && ntOlur && cur <= 1 && nt && !benNTAcildi)
                return Seviye(d, "NT", 2);                        // davet
            if (cur >= 3 && !zorunlu) return "Pas";

            // 6+ kartlık renk tekrarı (ortak desteklemediyse)
            string uz = EnUzunRenk(el);
            int uzLen = ElDegerlendirici.RenkUzunlugu(el, uz);
            bool benBu = gercek.Any(h => h.Oyuncu == d.AktifOyuncu && h.Koz == uz);
            if (benBu && uzLen >= 6 && !rakipRenkleri.Contains(uz) && hp >= 6 && cur <= 2)
            {
                int u = Cheapest(d, uz);
                if (u <= 2 + (est >= 22 ? 1 : 0)) return Seviye(d, uz, u);
            }

            // Zorunlu: açıcıya yeni renk geldi vs. → doğal cevap
            if (zorunlu)
            {
                if (dengeli) return EnUcuzNT(d, hp);
                return EnUcuz(d, UzunKoz(el, gercek, d));
            }

            // Cevap veren, ortak açtı, ben ilk kez konuşuyorum
            bool benSusmus = !gercek.Any(h => h.Oyuncu == d.AktifOyuncu);
            if (benSusmus && gercek.Any(h => h.Oyuncu == d.Partner) && hp >= 6)
                return IlkCevap(d, el, hp, dengeli, cur, rakipRenkleri, ntOlur);

            return "Pas";
        }

        // ── Ortak açtı, ilk cevabım ──
        private string IlkCevap(IhaleDurumu d, List<Card> el, int hp, bool dengeli, int cur,
                                List<string> rakipRenkleri, bool ntOlur)
        {
            // Yeni renk: 4+ majör (1 seviyede), 5+ minör vs.
            foreach (var r in new[] { "Maça", "Kupa" })
            {
                if (rakipRenkleri.Contains(r)) continue;
                if (ElDegerlendirici.RenkUzunlugu(el, r) >= 4 && Cheapest(d, r) == 1) return Seviye(d, r, 1);
            }
            foreach (var r in new[] { "Karo", "Sinek" })
            {
                if (rakipRenkleri.Contains(r)) continue;
                int u = Cheapest(d, r);
                if (ElDegerlendirici.RenkUzunlugu(el, r) >= 5 && (u == 1 || (u == 2 && hp >= 10))) return Seviye(d, r, u);
            }
            if (dengeli && ntOlur)
            {
                if (hp >= 13) return Seviye(d, "NT", 3);
                if (hp >= 11) return Seviye(d, "NT", 2);
                if (cur == 1) return Seviye(d, "NT", 1);
            }
            foreach (var r in new[] { "Karo", "Sinek", "Kupa", "Maça" })
            {
                if (rakipRenkleri.Contains(r)) continue;
                int u = Cheapest(d, r);
                if (ElDegerlendirici.RenkUzunlugu(el, r) >= 4 && u <= 2 && hp >= 10) return Seviye(d, r, u);
            }
            return "Pas";
        }

        // ═════════════════════════════════════════════════════════════════
        // 4. REKABET (son söz rakipte)
        // ═════════════════════════════════════════════════════════════════
        private string Rekabet(IhaleDurumu d, List<Card> el, int hp, bool dengeli, IhaleKoruma.Aralik ar,
                               IhaleHamlesi son, List<string> rakipRenkleri, List<IhaleHamlesi> gercek)
        {
            string fit = FitBul(d, el, gercek, rakipRenkleri, out int toplam);
            if (fit == null || fit == "NT" || toplam < 8) return "Pas";

            // Toplam Kart Kanunu: seviye = toplam koz - 6
            int lott = toplam - 6;
            double kismi = ar.Min + 0.35 * (ar.Max - ar.Min);
            double est = ElDegerlendirici.ToplamPuan(el, fit) + kismi;

            int T = HedefFit(fit, est, false);
            int ucuz = Cheapest(d, fit);

            // Oyun gücü varsa oyuna git (rakip engel yapsa bile)
            if (est >= 26.5 && T >= 4 && ucuz <= T) return Seviye(d, fit, T);

            // Yarış: ancak yeterli puan + LOTT izin veriyorsa ve ortak zaten güç gösterdiyse
            bool ortakGuc = ar.Min >= 6;
            // Güçlü el (davet+): ortağın cevabını sıçramalı destekle (1♣–1♥–(1♠)–3♥), 2. seviyede kalma
            if (ortakGuc && Major(fit) && est >= 23 && ucuz == 2) return Seviye(d, fit, 3);
            if (ortakGuc && est >= 19 && ucuz <= lott && ucuz <= 3) return Seviye(d, fit, ucuz);
            return "Pas";
        }

        /// <summary>
        /// Ortak açtı, rakip müdahale etti, ben ilk kez konuşuyorum: fit yoksa 5+ kartlık
        /// yeni rengi doğal teklif et (1 seviyede 6+ HP, 2 seviyede 10+ HP).
        /// Ör: 1♦ - (1♥) - 1♠ (6 kartlı ♠, 8 HP).
        /// </summary>
        private string RekabetYeniRenk(IhaleDurumu d, List<Card> el, int hp, List<string> rakipRenkleri,
                                       List<IhaleHamlesi> gercek)
        {
            if (hp < 6) return "Pas";
            if (gercek.Any(h => h.Oyuncu == d.AktifOyuncu)) return "Pas";
            var ortakTeklif = gercek.FirstOrDefault(h => h.Oyuncu == d.Partner);
            if (ortakTeklif == null) return "Pas";

            string en = null; int enUz = 0;
            foreach (var r in Renkler)
            {
                if (r == ortakTeklif.Koz || rakipRenkleri.Contains(r)) continue;
                int u = ElDegerlendirici.RenkUzunlugu(el, r);
                if (u >= 5 && (u > enUz || (u == enUz && RenkSira(r) > RenkSira(en)))) { en = r; enUz = u; }
            }
            if (en == null) return "Pas";
            int lvl = Cheapest(d, en);
            if (lvl == 1) return Seviye(d, en, 1);
            if (lvl == 2 && hp >= 10) return Seviye(d, en, 2);
            return "Pas";
        }

        // ═════════════════════════════════════════════════════════════════
        // YARDIMCILAR
        // ═════════════════════════════════════════════════════════════════

        /// <summary>Zorlayıcı durum mu (Pas yasak sayılır)?</summary>
        private bool Zorunlu(IhaleDurumu d, List<Card> el, List<IhaleHamlesi> gercek, bool ortak2C)
        {
            var son = gercek.Last();
            if (son.Oyuncu != d.Partner) return false;
            if (ortak2C)
            {
                // 2♣ güçlü: oyun seviyesine kadar zorunlu (4M / 3NT / 5m) — 2♣ → 2♦ → …
                string k = son.Koz;
                int gs = k == "NT" ? 3 : (Major(k) ? 4 : 5);
                return son.Seviye < gs;
            }
            // Açıcıya yeni renk (cevap veren yeni renkte teklif verdi): zorlayıcı
            var benimIlk = gercek.FirstOrDefault(h => h.Oyuncu == d.AktifOyuncu);
            var ortakIlk = gercek.FirstOrDefault(h => h.Oyuncu == d.Partner);
            if (benimIlk != null && ortakIlk != null && gercek.IndexOf(benimIlk) < gercek.IndexOf(ortakIlk) &&
                son == ortakIlk && son.Koz != "NT" && son.Seviye <= 2 &&
                !gercek.Any(h => h.Oyuncu == d.AktifOyuncu && h.Koz == son.Koz) &&
                !gercek.Any(h => d.Rakipler.Contains(h.Oyuncu) && gercek.IndexOf(h) > gercek.IndexOf(ortakIlk)))
                return true;

            // 2/1 oyun zorlaması: ortak 2. seviyede yeni renkle cevap verdi, sonra 3. seviyede başka bir yeni
            // renk gösterdi (ör. 1♠–2♦–2♠–3♥) → oyuna ulaşana dek Pas yasak.
            if (benimIlk != null && ortakIlk != null && gercek.IndexOf(benimIlk) < gercek.IndexOf(ortakIlk) &&
                ortakIlk.Koz != "NT" && ortakIlk.Seviye == 2 && son != ortakIlk &&
                son.Koz != "NT" && son.Seviye <= 3 && son.Koz != ortakIlk.Koz &&
                !gercek.Any(h => h.Oyuncu == d.AktifOyuncu && h.Koz == son.Koz) &&
                !gercek.Any(h => d.Rakipler.Contains(h.Oyuncu)))
                return true;
            return false;
        }

        private static int OyunSeviyesi(string koz) => koz == "NT" ? 3 : (Major(koz) ? 4 : 5);

        /// <summary>Fit rengi ve kombine uzunluk. Yoksa null.</summary>
        private string FitBul(IhaleDurumu d, List<Card> el, List<IhaleHamlesi> gercek, List<string> rakipRenkleri,
                              out int toplamUzunluk)
        {
            toplamUzunluk = 0;
            string en = null; int enToplam = 0;
            foreach (var r in Renkler)
            {
                if (rakipRenkleri.Contains(r) && !gercek.Any(h => h.Oyuncu == d.Partner && h.Koz == r)) continue;
                int pu = PartnerUzunluk(d, gercek, r);
                if (pu == 0) continue;
                int toplam = ElDegerlendirici.RenkUzunlugu(el, r) + pu;
                if (toplam < 8) continue;
                bool dahaIyi = en == null ||
                               (Major(r) && !Major(en)) ||
                               (Major(r) == Major(en) && (toplam > enToplam || (toplam == enToplam && RenkSira(r) > RenkSira(en))));
                if (dahaIyi) { en = r; enToplam = toplam; }
            }
            toplamUzunluk = enToplam;
            return en;
        }

        /// <summary>Ortağın bu renkte tahmini minimum uzunluğu (0 = bilinmiyor).</summary>
        private int PartnerUzunluk(IhaleDurumu d, List<IhaleHamlesi> gercek, string renk)
        {
            int uz = 0;
            int partnerSayi = 0;
            bool benOnceBu = false;
            foreach (var h in gercek)
            {
                if (h.Koz != renk) continue;
                if (h.Oyuncu == d.AktifOyuncu) { benOnceBu = true; continue; }
                if (h.Oyuncu != d.Partner) continue;
                partnerSayi++;
                if (partnerSayi == 1)
                {
                    bool acilis = gercek.First() == h;
                    if (benOnceBu)
                    {
                        // Destek verdi. Ben ortağın AÇTIĞI bir sekansta bu rengi cevap olarak söylediysem ve
                        // ortak (açıcı) onu destekliyorsa 4+ kart vaat eder (1♦–1♥–2♥: açıcıda ♥4+);
                        // kendi açtığım/müdahale ettiğim rengi ortak destekliyorsa majörde 3+ yeter.
                        var ilkGercek = gercek.First();
                        bool benCevapladim = ilkGercek.Oyuncu == d.Partner && ilkGercek.Koz != renk;
                        uz = benCevapladim ? 4 : (Major(renk) ? 3 : 4);
                    }
                    else if (acilis) uz = renk == "Maça" || renk == "Kupa" ? 5 : (renk == "Karo" ? 4 : 3);
                    else uz = h.Seviye >= 2 ? 5 : 4;
                }
                else uz = Math.Min(7, uz + 1);
            }
            return uz;
        }

        /// <summary>Fit kontratı için hedef seviye (-1: 3NT tercih edilir).</summary>
        private int HedefFit(string fit, double est, bool ntSecenegi, bool dar = false, int fitToplam = 0)
        {
            est += dar ? 2.5 : 0;     // dar aralıkta tahmin güvenilir: eşikleri aşağı çek
            if (Major(fit))
            {
                if (est >= 36) return 6;
                if (est >= 28.5) return 4;
                if (est >= 25.5) return 3;
                return 2;
            }
            // minör
            if (est >= 36) return 6;
            // 8 kartlık minör uyumda dengeli el + durdurucular varken 3NT (9 el) 5m'den (11 el) çok daha kolay.
            // 5m ancak 9+ kartlık uyumda ya da slam'a yakın güçte (est>=34) seçilir.
            if (est >= 29) return (ntSecenegi && fitToplam <= 8 && est < 34) ? -1 : 5;
            if (est >= 27.5) return ntSecenegi ? -1 : 4;
            if (est >= 25.5) return 3;
            return 2;
        }

        private bool NTOlur(List<Card> el, List<string> rakipRenkleri)
        {
            foreach (var r in rakipRenkleri)
                if (!ElDegerlendirici.StopperVar(el, r)) return false;
            return true;
        }

        private string EnUzunRenk(List<Card> el)
        {
            string en = "Sinek"; int u = -1;
            foreach (var r in Renkler)
            {
                int x = ElDegerlendirici.RenkUzunlugu(el, r);
                if (x > u || (x == u && RenkSira(r) > RenkSira(en))) { en = r; u = x; }
            }
            return en;
        }

        /// <summary>Zorunlu bir teklifte kullanılacak koz: kendi teklif ettiğim en uzun renk, yoksa en uzun renk.</summary>
        private string UzunKoz(List<Card> el, List<IhaleHamlesi> gercek, IhaleDurumu d)
        {
            var benim = gercek.Where(h => h.Oyuncu == d.AktifOyuncu && h.Koz != "NT")
                              .Select(h => h.Koz)
                              .OrderByDescending(r => ElDegerlendirici.RenkUzunlugu(el, r))
                              .FirstOrDefault();
            if (benim != null && ElDegerlendirici.RenkUzunlugu(el, benim) >= 6) return benim;
            return EnUzunRenk(el);
        }

        // ── Teklif üretme ──
        private static bool SonrakindenBuyukMu(IhaleDurumu d, int seviye, string koz)
        {
            string son = d.SonGercekTeklif;
            if (string.IsNullOrEmpty(son)) return true;
            int ss = KozYardimcisi.TekliftenSeviyeCikar(son);
            string sk = KozYardimcisi.TekliftenKozCikar(son);
            if (seviye > ss) return true;
            if (seviye < ss) return false;
            return RenkSira(koz) > RenkSira(sk);
        }

        /// <summary>Bu strain'de verilebilecek en düşük yasal seviye.</summary>
        private static int Cheapest(IhaleDurumu d, string koz)
        {
            for (int s = 1; s <= 7; s++)
                if (SonrakindenBuyukMu(d, s, koz)) return s;
            return 8;
        }

        /// <summary>Hedef seviyede teklif; hedefe ulaşmak yasal değilse (ihale daha yukarıda) Pas.</summary>
        private static string Seviye(IhaleDurumu d, string koz, int hedef)
        {
            int c = Cheapest(d, koz);
            if (c > hedef || c > 7) return "Pas";
            return KozYardimcisi.TeklifOlustur(hedef, koz);
        }

        private static string EnUcuz(IhaleDurumu d, string koz)
        {
            int c = Cheapest(d, koz);
            return c > 7 ? "Pas" : KozYardimcisi.TeklifOlustur(c, koz);
        }

        private static string EnUcuzNT(IhaleDurumu d, int hp)
        {
            int c = Cheapest(d, "NT");
            return c > 3 ? "Pas" : KozYardimcisi.TeklifOlustur(Math.Max(c, hp >= 18 ? 2 : 1), "NT");
        }
    }
}
