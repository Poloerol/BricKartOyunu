using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// İhale koruma katmanı: konvansiyonların ürettiği teklifi vermeden önce
    /// "bu bağlamda mantıklı mı?" diye denetler. Konvansiyon kodlarına dokunmadan
    /// kontrolsüz slam / yanlış bağlamda çıkan konvansiyon tekliflerini engeller.
    ///
    /// Ortaklığın puan aralığı, ortağın şimdiye kadarki tekliflerinden tahmin edilir
    /// (PartnerAraligi). Toplam tahmin = kendi HCP + ortağın alt sınırı + aralığın 1/4'ü.
    /// </summary>
    public static class IhaleKoruma
    {
        // ───────────────────────── Aralık tahmini ─────────────────────────

        public struct Aralik { public int Min, Max; }

        private static bool Gercek(string t) =>
            !string.IsNullOrEmpty(t) && t.Length >= 2 && char.IsDigit(t[0]);

        private static int Sev(string t) => t[0] - '0';

        /// <summary>Ortağın teklifleri bugüne kadar ne kadar HCP gösteriyor.</summary>
        public static Aralik PartnerAraligi(IhaleDurumu d)
        {
            var ort = d.Partner;
            var ben = d.AktifOyuncu;
            var a = new Aralik { Min = 0, Max = 40 };
            bool birisiAcildi = false;      // ortadan bir açılış yapıldı mı
            bool ortakAcilis = false;       // açılışı ortak yaptı
            bool benAcilis = false;         // açılışı ben yaptım
            string acilisRengi = null;
            bool ortakBirsey = false;
            bool ortakBirsekacildi = false;
            string acilisYapisi = null;
            bool acilisBir = false;           // ortak 1 seviye suit açılışı yaptı       // 1NT / 2NT / 2♣ (yapay cevap dizileri)   // ortağın açılış sonrası ilk sınırı verildi mi

            var g = d.Gecmis ?? new List<IhaleHamlesi>();
            for (int i = 0; i < g.Count; i++)
            {
                var h = g[i];
                string t = h.Teklif;
                bool gercek = Gercek(t);
                if (!birisiAcildi)
                {
                    if (gercek)
                    {
                        birisiAcildi = true;
                        acilisRengi = KozYardimcisi.TekliftenKozCikar(t);
                        if (h.Oyuncu == ort)
                        {
                            ortakAcilis = true; ortakBirsey = true;
                            a = AcilisAraligi(t);
                            acilisBir = Sev(t) == 1 && KozYardimcisi.TekliftenKozCikar(t) != "NT";
                        }
                        else if (h.Oyuncu == ben)
                        {
                            benAcilis = true;
                            if (t == "1NT" || t == "2NT" || t == "2♣") acilisYapisi = t;
                        }
                    }
                    else if (h.Oyuncu == ort) { a = new Aralik { Min = 0, Max = 11 }; }
                    continue;
                }
                if (h.Oyuncu != ort) continue;

                // Ortağın açılış sonrası hamleleri
                if (!gercek)
                {
                    if (t == "Dbl" || t == "Kontr") { a.Min = Math.Max(a.Min, 11); ortakBirsey = true; }
                    else if (t == "RDbl" || t == "S.Kontr") { a.Min = Math.Max(a.Min, 10); ortakBirsey = true; }
                    else if (ortakBirsey)
                    {
                        // Ortak daha önce konuştu, şimdi Pas: aralığı daralt
                        a.Max = Math.Min(a.Max, a.Min + 3);
                    }
                    else if (!ortakBirsey)
                    {
                        // Pas: ortak ya açmadı ya da cevap vermedi
                        a = new Aralik { Min = 0, Max = (ortakAcilis ? a.Max : benAcilis ? 5 : 9) };
                    }
                    continue;
                }
                ortakBirsey = true;
                int s = Sev(t);
                string renk = KozYardimcisi.TekliftenKozCikar(t);
                int lo;
                if (benAcilis && acilisYapisi != null)
                {
                    // NT / 2♣ yapısı: yalnızca ilk cevap sınır verir (sonrası yapay)
                    if (!ortakBirsekacildi)
                    {
                        ortakBirsekacildi = true;
                        var ra = NTCevapAraligi(acilisYapisi, t);
                        a.Min = ra.Min; a.Max = ra.Max;
                    }
                }
                else if (benAcilis)
                {
                    // Ortak cevap veriyor: (alt, üst) sınır
                    int hi;
                    bool ayniRenk = renk == acilisRengi;
                    bool majAcilis = acilisRengi == "Kupa" || acilisRengi == "Maça";
                    bool bergen = d.Anlasma != null && d.Anlasma.BergenRaises && majAcilis && !ortakBirsekacildi;
                    if (bergen && s == 3 && !ayniRenk && renk == "Sinek") { lo = 7; hi = 10; }                // 3♣ = 7-10 (4'lü destek)
                    else if (bergen && s == 3 && !ayniRenk && renk == "Karo") { lo = 11; hi = 12; }           // 3♦ = 11-12
                    else if (bergen && s == 3 && ayniRenk) { lo = 0; hi = 6; }                                // 3M = önleyici
                    else if (majAcilis && s == 4 && ayniRenk && !ortakBirsekacildi) { lo = 3; hi = 11; }      // 4M = oynamak için / önleyici
                    else if (s == 1) { lo = 6; hi = renk == "NT" ? 10 : 14; }
                    else if (s == 2) { lo = ayniRenk ? 6 : (renk == "NT" ? 11 : 10); hi = ayniRenk ? 10 : (renk == "NT" ? 12 : 17); }
                    else if (s == 3 && ayniRenk && !(renk == "Kupa" || renk == "Maça")) { lo = 4; hi = 10; }   // minör 3 seviye sıçrama: zayıf
                    else if (s == 3) { lo = (renk == "NT") ? 13 : 10; hi = ayniRenk ? 12 : 17; }
                    else if (s == 4) { lo = (renk == "NT") ? 15 : 12; hi = ayniRenk ? 17 : 21; }
                    else { lo = 15; hi = 40; }
                    if (a.Min < 6 || a.Max >= 40 || !ortakBirsekacildi) { a.Min = Math.Max(a.Min, lo); a.Max = hi; }
                    else if (s >= 4 && renk != "NT") a.Min = Math.Max(a.Min, Math.Min(lo, 12));   // sonraki teklifler yeni güç göstermez
                    ortakBirsekacildi = true;
                }
                else if (ortakAcilis)
                {
                    // Ortağın (açıcının) ikinci / sonraki teklifleri: aralığı daralt
                    int rlo = a.Min, rhi = a.Max;
                    if (acilisBir)
                    {
                        // Bu teklif öncesi ihale seviyesi (atlama var mı?)
                        int pL = 0, pR = -1;
                        for (int j = i - 1; j >= 0; j--)
                            if (Gercek(g[j].Teklif)) { pL = Sev(g[j].Teklif); pR = KozSiraAl(KozYardimcisi.TekliftenKozCikar(g[j].Teklif)); break; }
                        int rk = KozSiraAl(renk);
                        int ucuz = renk == "NT" ? pL : pL + (rk > pR ? 0 : 1);
                        int atlama = s - ucuz;
                        if (renk == "NT")
                        {
                            if (atlama <= 0) { rlo = 12; rhi = 14; }
                            else if (atlama == 1) { rlo = 18; rhi = 19; }
                            else { rlo = 18; rhi = 21; }
                        }
                        else if (renk == acilisRengi || ortakBizimRenkMi(renk, g, ben))
                        {
                            if (atlama <= 0) { rlo = 12; rhi = 15; }
                            else if (atlama == 1) { rlo = 16; rhi = 18; }
                            else { rlo = 15; rhi = 21; }
                        }
                        else
                        {
                            if (atlama >= 1) { rlo = 17; rhi = 21; }
                            else if (s >= 2 && rk > KozSiraAl(acilisRengi)) { rlo = 16; rhi = 21; }    // reverse
                            else { rlo = 12; rhi = 21; }
                        }
                    }
                    else if (s >= 3) rlo = 15;
                    a.Min = Math.Max(a.Min, rlo);
                    a.Max = Math.Min(a.Max, Math.Max(rhi, a.Min));
                }
                else
                {
                    // Rakip açtı, ortak müdahale/cevap
                    int hi;
                    if (s == 1) { lo = 8; hi = 16; }
                    else if (s == 2) { lo = 10; hi = 17; }
                    else if (s == 3) { lo = 5; hi = 11; }
                    else { lo = 5; hi = 12; }
                    if (renk == "NT") { if (s == 1) { lo = 15; hi = 18; } else { lo = 5; hi = 11; } }   // 2NT müdahale: Unusual (5-5 minör)
                    if (!ortakBirsekacildi) { a.Min = lo; a.Max = hi; ortakBirsekacildi = true; }
                    else a.Min = Math.Max(a.Min, lo);
                }
                if (a.Max < a.Min) a.Max = Math.Min(40, a.Min + 6);
            }
            if (a.Max < a.Min) a.Max = a.Min;
            return a;
        }

        /// <summary>1NT / 2NT / 2♣ açılışına ortağın İLK cevabının gösterdiği aralık.</summary>
        private static Aralik NTCevapAraligi(string acilis, string cevap)
        {
            int s = Sev(cevap);
            string k = KozYardimcisi.TekliftenKozCikar(cevap);
            if (acilis == "1NT")
            {
                if (cevap == "2NT") return new Aralik { Min = 8, Max = 9 };
                if (cevap == "3NT") return new Aralik { Min = 10, Max = 15 };
                if (s >= 4) return new Aralik { Min = 10, Max = 17 };
                if (s == 3) return new Aralik { Min = 10, Max = 15 };
                return new Aralik { Min = 0, Max = 13 };        // 2♣/2♦/2♥/2♠: Stayman / transfer / zayıf
            }
            if (acilis == "2NT")
            {
                if (cevap == "3NT") return new Aralik { Min = 4, Max = 11 };
                if (s >= 4) return new Aralik { Min = 11, Max = 17 };
                return new Aralik { Min = 0, Max = 12 };        // 3♣ Puppet / transfer
            }
            // 2♣ güçlü
            if (cevap == "2♦") return new Aralik { Min = 0, Max = 7 };
            return new Aralik { Min = 8, Max = 17 };
        }

        private static int KozSiraAl(string k)
        {
            switch (k) { case "Sinek": return 0; case "Karo": return 1; case "Kupa": return 2; case "Maça": return 3; }
            return 4;
        }

        private static bool ortakBizimRenkMi(string renk, List<IhaleHamlesi> g, Player ben) =>
            g.Any(h => h.Oyuncu == ben && Gercek(h.Teklif) && KozYardimcisi.TekliftenKozCikar(h.Teklif) == renk);

        private static Aralik AcilisAraligi(string t)
        {
            int s = Sev(t);
            string k = KozYardimcisi.TekliftenKozCikar(t);
            if (k == "NT")
            {
                if (s == 1) return new Aralik { Min = 15, Max = 17 };
                if (s == 2) return new Aralik { Min = 20, Max = 21 };
                return new Aralik { Min = 25, Max = 27 };
            }
            if (s == 1) return new Aralik { Min = 12, Max = 21 };
            if (s == 2 && k == "Sinek") return new Aralik { Min = 22, Max = 40 };
            if (s == 2) return new Aralik { Min = 5, Max = 10 };
            return new Aralik { Min = 4, Max = 11 };    // 3+ seviyesi: preempt
        }

        // ───────────────────────── Yardımcılar ─────────────────────────

        private static IhaleHamlesi SonGercek(IhaleDurumu d, bool ortakMi)
        {
            var g = d.Gecmis ?? new List<IhaleHamlesi>();
            for (int i = g.Count - 1; i >= 0; i--)
            {
                if (!Gercek(g[i].Teklif)) continue;
                if ((g[i].Oyuncu == d.Partner) == ortakMi && (g[i].Oyuncu != d.AktifOyuncu || !ortakMi)) return g[i];
            }
            return null;
        }

        private static int RakipSeviyesi(IhaleDurumu d)
        {
            int max = 0;
            foreach (var h in d.Gecmis ?? new List<IhaleHamlesi>())
            {
                if (!Gercek(h.Teklif)) continue;
                if (h.Oyuncu == d.AktifOyuncu || h.Oyuncu == d.Partner) continue;
                max = Math.Max(max, Sev(h.Teklif));
            }
            return max;
        }

        private static bool BizimTarafBirSeyDediMi(IhaleDurumu d)
        {
            foreach (var h in d.Gecmis ?? new List<IhaleHamlesi>())
                if (Gercek(h.Teklif) && (h.Oyuncu == d.AktifOyuncu || h.Oyuncu == d.Partner)) return true;
            return false;
        }

        private static bool OrtakDbl(IhaleDurumu d)
        {
            // son gerçek olmayan-pas hamle ortağın dbl'ı mı
            var g = d.Gecmis ?? new List<IhaleHamlesi>();
            for (int i = g.Count - 1; i >= 0; i--)
            {
                if (g[i].Teklif == "Pas") continue;
                return g[i].Oyuncu == d.Partner && (g[i].Teklif == "Dbl" || g[i].Teklif == "Kontr");
            }
            return false;
        }

        private static readonly HashSet<string> SoruKonv = new HashSet<string>
        {
            "Blackwood","RKCB","Gerber","Kickback","Exclusion Blackwood","Koz Q Sorusu",
            "Grand Slam Force","Pick-a-Slam","Serious 3NT","Non-Serious 3NT"
        };

        private static readonly HashSet<string> RekabetKonv = new HashSet<string>
        {
            "Michaels","Unusual 2NT","Cappelletti","DONT","Multi-Landy"
        };

        // ───────────────────────── Bağlam kapıları ─────────────────────────

        private static IhaleHamlesi IlkGercek(IhaleDurumu d) =>
            (d.Gecmis ?? new List<IhaleHamlesi>()).FirstOrDefault(h => Gercek(h.Teklif));

        private static bool BizimTaraf(IhaleDurumu d, Player p) => p == d.AktifOyuncu || p == d.Partner;

        /// <summary>Bu konvansiyon bu dizide anlamlı mı?</summary>
        private static bool BaglamUygun(IhaleDurumu d, string ad, string teklif)
        {
            var ilk = IlkGercek(d);
            switch (ad)
            {
                // NT açılışı üzerine kurulu yapılar: ilk gerçek teklif bizim 1NT/2NT açılışımız olmalı
                case "Stayman":
                case "Puppet Stayman":
                case "Jacoby Transfer":
                case "Smolen":
                case "Minor Transfer":
                case "Gerber":
                case "Lebensohl":
                    return ilk != null && BizimTaraf(d, ilk.Oyuncu) && ilk.Koz == "NT" && Sev(ilk.Teklif) <= 2;

                // 1 majör açılışı üzerine
                case "Bergen Raises":
                case "Forcing NT":
                case "Splinter":
                case "Jacoby 2NT":
                case "Drury":
                case "Trial Bid":
                    return ilk != null && BizimTaraf(d, ilk.Oyuncu) && Sev(ilk.Teklif) == 1 &&
                           (ilk.Koz == "Kupa" || ilk.Koz == "Maça");

                // 1 minör açılışı üzerine
                case "Inverted Minors":
                    return ilk != null && BizimTaraf(d, ilk.Oyuncu) && Sev(ilk.Teklif) == 1 &&
                           (ilk.Koz == "Karo" || ilk.Koz == "Sinek");

                // Rakip açılışına karşı kullanılan ekler
                case "Takeout Double":
                    return ilk != null && !BizimTaraf(d, ilk.Oyuncu);
            }
            return true;
        }

        /// <summary>
        /// Aktif oyuncu, ortağın yapay sorusuna / konvansiyon teklifine CEVAP mı veriyor?
        /// (Cevaplar zorunludur; ortaklık gücü yetmese de verilmelidir.)
        /// </summary>
        private static bool CevapVeriyorMu(IhaleDurumu d, string ad)
        {
            var g = d.Gecmis ?? new List<IhaleHamlesi>();
            var ilk = IlkGercek(d);
            var ortSon = SonGercek(d, true);
            var benSon = g.LastOrDefault(h => h.Oyuncu == d.AktifOyuncu && Gercek(h.Teklif));
            bool benAcilis = ilk != null && ilk.Oyuncu == d.AktifOyuncu;
            // ortağın son teklifi benim son teklifimden SONRA mı?
            bool ortakSonra = ortSon != null && (benSon == null || g.IndexOf(ortSon) > g.IndexOf(benSon));

            switch (ad)
            {
                // Tek başına zayıf / önleyici ya da yalnızca cevap niteliğinde konvansiyonlar:
                // kendi UygunMu denetimleri yeterli
                case "Namyats":
                case "Multi 2♦":
                case "Bergen Raises":
                case "Inverted Minors":
                    return true;

                case "Gerber":
                    return ortakSonra && ortSon.Teklif == "4♣" && ilk != null && ilk.Koz == "NT";

                // NT açıcısı olarak yapay sorulara cevap
                case "Puppet Stayman":
                case "Stayman":
                case "Jacoby Transfer":
                case "Minor Transfer":
                    return benAcilis && ortakSonra;

                case "Trial Bid":
                    return ortakSonra && ilk != null && ortSon.Koz != "NT" && ortSon.Koz != ilk.Koz && Sev(ortSon.Teklif) <= 3;

                case "Exclusion Blackwood":
                    // Exclusion sorusu: koz kararlaştırılmış, ortak daha önce hiç söylenmemiş bir renge 5. seviyeye sıçradı
                    if (!(ortakSonra && ilk != null && benSon != null && benSon.Koz == ilk.Koz &&
                          Sev(ortSon.Teklif) == 5 && ortSon.Koz != "NT" && ortSon.Koz != ilk.Koz)) return false;
                    return !g.Any(h => h != ortSon && Gercek(h.Teklif) && h.Koz == ortSon.Koz);

                case "Kickback":
                    // Koz kararlaştırılmış olmalı: açılış rengine ben de destek verdim,
                    // ortak ise kozun ÜSTÜNDE 4. seviye bir renk dedi
                    return ortakSonra && ilk != null && benSon != null && benSon.Koz == ilk.Koz &&
                           Sev(ortSon.Teklif) == 4 && ortSon.Koz != "NT" &&
                           KozSiraAl(ortSon.Koz) > KozSiraAl(ilk.Koz);

                // Lebensohl: 2NT rölesinden sonra ortağın 3♣'ine sign-off / düzeltme
                case "Lebensohl":
                case "Lebensohl (Reverse)":
                case "Lebensohl (Zayıf 2)":
                    return ortakSonra && benSon != null && benSon.Teklif == "2NT";

                // 3. / 4. renk forsingine AÇICI olarak cevap
                case "3. Renk Forsingi":
                case "4. Renk Forsingi":
                    return benAcilis && ortakSonra;

                case "Blackwood":
                case "RKCB":
                    return ortakSonra && (ortSon.Teklif == "4NT" || ortSon.Teklif == "5NT");

                case "Koz Q Sorusu":
                case "Grand Slam Force":
                case "Pick-a-Slam":
                    // yalnızca ortağın 5NT sorusuna CEVAP muaf; kendi sorularım güç eşiğinden geçer
                    return ortakSonra && ortSon.Teklif == "5NT";
            }
            return false;
        }

        private const double CevapSlamEsigi = 26;

        // ───────────────────────── Ana denetim ─────────────────────────

        /// <summary>true → teklif verilebilir.</summary>
        public static bool Izin(IhaleDurumu d, string teklif, string konvAd)
        {
            if (d == null || string.IsNullOrEmpty(teklif)) return true;
            string ad = konvAd ?? "";

            // ── Çiftler (Dbl) ──
            if (teklif == "Dbl" || teklif == "Kontr")
            {
                if (ad == "Negative Double")
                {
                    // Ortak açtı, rakip müdahale etti, bizim taraf başka bir şey demedi
                    var ort = SonGercek(d, true); var rak = SonGercek(d, false);
                    if (ort == null || rak == null) return false;
                    var g = d.Gecmis;
                    var ilk = g.FirstOrDefault(h => Gercek(h.Teklif));
                    if (ilk == null || ilk.Oyuncu != d.Partner) return false;
                    if (g.Any(h => h.Oyuncu == d.AktifOyuncu && Gercek(h.Teklif))) return false;
                    if (Sev(rak.Teklif) > 3) return false;
                    if (ElDegerlendirici.HCP(d.AktifOyuncuEli) < 6) return false;
                }
                else if (ad == "Responsive Double")
                {
                    // Desen (rakip 1x - ortak Pas/Dbl - rakip 2x) konvansiyonun kendi UygunMu'sunda denetlenir;
                    // burada yalnızca seviye ve güç sınırı aranır.
                    var rak = SonGercek(d, false);
                    if (rak == null || Sev(rak.Teklif) > 4) return false;
                    if (ElDegerlendirici.HCP(d.AktifOyuncuEli) < 6) return false;
                }
                return true;
            }

            if (!Gercek(teklif)) return true;       // Pas / RDbl: dokunma

            // Bozucu / zayıf iki cevapları ve müdahalesi dağılıma (koz uzunluğu) dayanır; toplam HCP
            // tahmini burada anlamsız (ör. 3♠ bozucuya 3 kart destekle 4♠). Kendi kuralları yeterli.
            if (ad == "Preempt Cevap" || ad == "Zayıf İki Cevap" || ad == "Zayıf İki Müdahale")
                return true;

            int sev = Sev(teklif);
            string renk = KozYardimcisi.TekliftenKozCikar(teklif);
            int ben = ElDegerlendirici.HCP(d.AktifOyuncuEli);

            // ── Rakip müdahalesi konvansiyonları sadece rakip açıp bizimkiler susmuşken ──
            if (RekabetKonv.Contains(ad))
            {
                var g = d.Gecmis ?? new List<IhaleHamlesi>();
                var ilk = g.FirstOrDefault(h => Gercek(h.Teklif));
                if (ilk == null) return false;
                // Ortak müdahale etti (Cappelletti 2♦ / Landy 2♣ ...), ben henüz konuşmadım → cevap veriyorum
                bool benKonustu = g.Any(h => Gercek(h.Teklif) && h.Oyuncu == d.AktifOyuncu);
                bool ortakKonustu = g.Any(h => Gercek(h.Teklif) && h.Oyuncu == d.Partner);
                if (!benKonustu && ortakKonustu && !BizimTaraf(d, ilk.Oyuncu)) return true;
                // Müdahaleci devamı: kendi yapay müdahalemin ardından ortağın cevabı (pass-or-correct)
                if (benKonustu && ortakKonustu && !BizimTaraf(d, ilk.Oyuncu))
                {
                    var ortSonR = g.LastOrDefault(h => h.Oyuncu == d.Partner && Gercek(h.Teklif));
                    var benSonR = g.LastOrDefault(h => h.Oyuncu == d.AktifOyuncu && Gercek(h.Teklif));
                    if (ortSonR != null && benSonR != null && g.IndexOf(ortSonR) > g.IndexOf(benSonR) &&
                        Sev(ortSonR.Teklif) <= 2) return true;
                }
                if (benKonustu || ortakKonustu) return false;
                return true;
            }

            // ── Bağlam kapıları: yapıya özgü konvansiyonlar yalnızca ilgili dizilerde ──
            if (!BaglamUygun(d, ad, teklif)) return false;

            // Açılışta (bizim taraf ilk kez konuşuyor) bütün açılış sistemi serbest
            if (!BizimTarafBirSeyDediMi(d) && !(d.Gecmis ?? new List<IhaleHamlesi>()).Any(h => Gercek(h.Teklif)))
                return true;
            var ar = PartnerAraligi(d);
            double est = ben + ar.Min + (ar.Max - ar.Min) * 0.25;
            // Uzun (6+) renk: söylediğim renk en uzun rengimse fazladan kartlar dağılım puanı sayılır
            if (renk != "NT" && !string.IsNullOrEmpty(renk))
            {
                int uzunluk = d.AktifOyuncuEli.Count(c => c.Suit == renk);
                if (uzunluk >= 7) est += uzunluk - 5;
            }
            int rakSev = RakipSeviyesi(d);

            // Ortağın sorusuna zorunlu cevap: ortağın son teklifi soru gibiyse seviye sınırı yok
            var ortSon = SonGercek(d, true);
            if (ortSon != null && SoruKonv.Contains(ad) && Sev(ortSon.Teklif) >= 4 && sev <= Sev(ortSon.Teklif) + 1)
            {
                string oRenk = KozYardimcisi.TekliftenKozCikar(ortSon.Teklif);
                if (ortSon.Teklif == "4NT" || ortSon.Teklif == "5NT") return true;
            }

            // Ortağın yapay sorusuna CEVAP veriyorsam güç eşiği uygulanmaz
            // (yalnızca slam seviyesindeki cevaplar için asgari toplam güç aranır)
            if (CevapVeriyorMu(d, ad)) return sev < 6 || est >= CevapSlamEsigi;

            // Soru bidleri (slam arayışı) ancak güçlü toplam ile
            if (SoruKonv.Contains(ad))
            {
                if (ad == "Serious 3NT" || ad == "Non-Serious 3NT") return est >= 28;
                if (ad == "Koz Q Sorusu")
                {
                    // Kendi 4NT'm / 4. seviye sorum cevaplandı, şimdi kız soruyorum: slam eşiği biraz düşük
                    var benSon2 = (d.Gecmis ?? new List<IhaleHamlesi>()).LastOrDefault(h => h.Oyuncu == d.AktifOyuncu && Gercek(h.Teklif));
                    if (benSon2 != null && Sev(benSon2.Teklif) >= 4) return est >= 30;
                    return est >= 30;
                }
                if (ad == "Grand Slam Force") return est >= 34;
                if (ad == "Pick-a-Slam") return est >= 29;
                return est >= 30;
            }

            // Seviye tavanı
            if (sev >= 7) return est >= 36;
            if (sev == 6) return est >= 32;
            if (sev == 5) return est >= 27 || rakSev >= 4 || (rakSev >= 3 && est >= 22);
            if (sev == 4)
            {
                bool oyunSeviyesi = renk == "Maça" || renk == "Kupa";
                if (!oyunSeviyesi) return est >= 25 || rakSev >= 3;
                return est >= 24 || rakSev >= 3;
            }
            if (sev == 3)
            {
                if (renk == "NT") return est >= 24 || rakSev >= 3;
                return est >= 20 || rakSev >= 2;
            }
            return true;
        }
    }
}
