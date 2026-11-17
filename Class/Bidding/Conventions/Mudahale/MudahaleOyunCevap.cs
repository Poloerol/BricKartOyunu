using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Müdahale Oyun Cevabı: rakip 2 seviyede renk açtı (zayıf iki), ortağım müdahale etti, sağ rakip pas dedi, sıra bende.
    ///
    /// Motor Karnesi (30.000 bord): bu dizilerde yedek mantık (Temel Mantık) 10-13 HP'li elle pas geçiyordu; ortağın
    /// 3 seviyedeki müdahalesi 15+ HP gösterdiğinden birleşik 25+ HP ile oyun kaçıyordu (O:2H P:3C, O:2S P:3D ...).
    ///
    /// Kurallar:
    ///   • Ortak 3 seviyede minörle müdahale etti: açılış rengimde durdurucu ve yeterli HP → 3NT;
    ///     durdurucu yoksa minörde 3+ kart destek ve 9+ HP → 5m.
    ///   • Ortak 3 seviyede majörle müdahale etti: 3+ destek ve 8+ HP ile 4M.
    ///   • Ben zayıf iki açılışına takeout kontra yaptım, ortak 3 seviyede cevap verdi: 12+ HP ve rakibin renginde durdurucuyla 3NT.
    ///   (2 seviyede majör müdahalesi için ölçümde kazanç çıkmadı; kural eklenmedi.)
    /// Öncelik 22.
    /// </summary>
    public class MudahaleOyunCevap : IKonvansiyon
    {
        public string Ad => "Müdahale Oyun Cevabı";
        public string Aciklama => "Zayıf iki açılışına ortağın müdahalesinden sonra yeterli güçle oyun teklif eder";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 22;

        public bool UygunMu(IhaleDurumu durum) { return Bul(durum) != null; }
        public string TeklifVer(IhaleDurumu durum) { return Bul(durum) ?? "Pas"; }

        private static string Sem(string koz)
        {
            switch (koz) { case "Maça": return "♠"; case "Kupa": return "♥"; case "Karo": return "♦"; case "Sinek": return "♣"; }
            return null;
        }

        private static string Bul(IhaleDurumu d)
        {
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13 || d.IhaleBittiMi() || d.Gecmis == null) return null;
            var b = d.Gecmis.Where(h => h.GercekTeklifMi || h.KontrMi || h.SKontrMi).ToList();
            var el = d.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);

            // [rakip zayıf iki] - [ben Dbl] - [ortak 3 seviye cevap] - Pas: takeout kontra yapan güçlü elle oyunu bulur
            if (b.Count == 3)
            {
                var a3 = b[0]; var k3 = b[1]; var c3 = b[2];
                if (!a3.GercekTeklifMi || !k3.KontrMi || !c3.GercekTeklifMi) return null;
                if (!d.Rakipler.Contains(a3.Oyuncu) || k3.Oyuncu != d.AktifOyuncu || c3.Oyuncu != d.Partner) return null;
                if (a3.Seviye != 2 || a3.Koz == "NT" || a3.Koz == "Sinek") return null;
                if (c3.Seviye != 3 || c3.Koz == "NT" || c3.Koz == a3.Koz) return null;
                // Motor Karnesi (30.000 bord, üç 10.000'likte de aynı yönde): 12+ HP ve rakibin renginde durdurucuyla 3NT -190 IMP
                // (13 → -187, 14 → -172, 16 → -129, 18 → -41). 5m (3+ destek) +68 IMP kötüleştirdi, 4M hiç tetiklenmedi: eklenmedi.
                if (hp >= 12 && ElDegerlendirici.StopperVar(el, a3.Koz)) return "3NT";
                return null;
            }

            if (b.Count != 2) return null;
            var a = b[0]; var o = b[1];
            if (!a.GercekTeklifMi || !o.GercekTeklifMi) return null;
            if (!d.Rakipler.Contains(a.Oyuncu) || o.Oyuncu != d.Partner) return null;
            if (a.Seviye != 2 || a.Koz == "NT" || a.Koz == "Sinek") return null;      // zayıf iki (2♦/2♥/2♠)
            if (o.Koz == "NT" || o.Koz == a.Koz) return null;

            int destek = ElDegerlendirici.RenkUzunlugu(el, o.Koz);
            bool minor = o.Koz == "Sinek" || o.Koz == "Karo";

            // Motor Karnesi (30.000 bord, üç 10.000'likte de aynı yönde): eşik 9 HP → -352 IMP (8 → -416, 10 → -301, 12 → -259).
            if (o.Seviye == 3 && minor)
            {
                if (hp >= 9 && ElDegerlendirici.StopperVar(el, a.Koz)) return "3NT";
                if (hp >= 9 && destek >= 3) return "5" + Sem(o.Koz);
            }
            // Ortağın 3 seviyede majör müdahalesine 3+ destek ve 8+ HP ile 4M (Motor Karnesi: 8 → -82 IMP; 7 → -65, 9 → -45, 11 → -34)
            if (o.Seviye == 3 && !minor && hp >= 8 && destek >= 3) return "4" + Sem(o.Koz);
            // 2 seviyede majör müdahalesine 4M (3+ destek): ölçümde anlamlı kazanç çıkmadı (-8 IMP), eklenmedi.
            return null;
        }
    }
}
