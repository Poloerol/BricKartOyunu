using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Cappelletti (Hamilton) — Rakip 1NT açışına müdahale konvansiyonu.
    /// 
    /// Michael Cappelletti / Fred Hamilton tarafından geliştirildi.
    /// 
    /// Yapı:
    ///   Dbl   = Ceza (15+ HP)
    ///   2♣    = Tek renkli el (6+ kart)
    ///   2♦    = Majörler (5-5 veya 5-4)
    ///   2♥    = ♥ + bilinmeyen bir minör
    ///   2♠    = ♠ + bilinmeyen bir minör
    ///   2NT   = Minörler (5-5)
    /// 
    /// Örnek:
    ///   (1NT) - 2♣ - (Pas) - 2♦ (röle) - (Pas) - 2♥/2♠/3♣/3♦ (uzun renk)
    ///   (1NT) - 2♦ - (Pas) - 2♥/2♠ (en uzun majör)
    ///   (1NT) - 2♥ - (Pas) - Pas (♥ toleransı) / 2NT (minör sorusu)
    /// </summary>
    public class Cappelletti : IKonvansiyon
    {
        public string Ad => "Cappelletti";
        public string Aciklama => "1NT açışına Cappelletti müdahalesi";
        public bool AktifMi { get; set; } = true;

        // Michaels (19)'dan sonra, Unusual2NT (23)'ten önce
        public int Oncelik => 12;   // Gerber'den (13) önce

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.Gecmis == null) return false;
            if (durum.AktifOyuncuEli == null) return false;

            var gercekTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            if (gercekTeklifler.Count < 1) return false;

            // ─── SENARYO A: Rakip 1NT açtı, biz müdahale ediyoruz ───
            // Son teklif rakip tarafından 1NT mi?
            var sonTeklif = gercekTeklifler.Last();
            if (sonTeklif.Teklif == "1NT"
                && gercekTeklifler.Count == 1
                && durum.Rakipler.Contains(sonTeklif.Oyuncu))
            {
                // Müdahale edebilmek için yeterli HP mi? (8+)
                int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
                if (hp >= 8)
                    return true;
            }

            // ─── SENARYO B: Partnerimiz Cappelletti ile müdahale etti, biz cevap veriyoruz ───
            // Geçmişte partnerimizin 2♣/2♦/2♥/2♠/2NT teklifi var mı?
            var partnerTeklifleri = durum.Gecmis
                .Where(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi)
                .ToList();

            if (partnerTeklifleri.Count >= 1)
            {
                var pTeklif = partnerTeklifleri.Last();

                // Partner 2♣/2♦/2♥/2♠/2NT dedi mi?
                if (pTeklif.Seviye == 2
                    && (pTeklif.Koz == "Sinek" || pTeklif.Koz == "Karo"
                        || pTeklif.Koz == "Kupa" || pTeklif.Koz == "Maça"
                        || pTeklif.Koz == "NT"))
                {
                    // Bu tekliften önce rakip 1NT açmış mı?
                    var oncekiRakipTeklif = gercekTeklifler
                        .Take(1)   // 1NT açılış olmalı (1♣-1♠-1NT gibi rebid değil)
                        .Where(h => durum.Rakipler.Contains(h.Oyuncu)
                                 && h.Teklif == "1NT")
                        .FirstOrDefault();

                    if (oncekiRakipTeklif != null)
                    {
                        // Partnerimizin 2 seviyesi teklifi Cappelletti müdahalesi olabilir.
                        // Ama emin olmak için partnerin 2♣'si Gerber vs. olmadığını kontrol et.
                        // Cappelletti'de 2♣ = tek renkli (6+), 2♦ = majörler, 2♥ = ♥+minör,
                        // 2♠ = ♠+minör, 2NT = minörler.
                        // Bu teklifleri ayırt etmek için partnerin önceki teklifine bak.
                        // Eğer partner daha önce konuşmadıysa (ilk teklifi 2 seviyesi ise)
                        // bu Cappelletti olabilir.

                        // Partnerin bu teklifi ilk gerçek teklifi mi?
                        var partnerIlkTeklif = gercekTeklifler
                            .FirstOrDefault(h => h.Oyuncu == durum.Partner && h.GercekTeklifMi);

                        if (partnerIlkTeklif == pTeklif)
                        {
                            // Partnerin ilk teklifi, 2 seviyesi ve rakip 1NT açmış → Cappelletti
                            return true;
                        }
                    }
                }
            }

            // ─── SENARYO C: Ben 2♣ (tek renkli) dedim, rakip röle seviyesine (2♦) girdi,
            //     partnerim kontra ile röle yaptı → uzun rengimi söylerim ───
            if (gercekTeklifler.Count >= 3
                && gercekTeklifler[0].Teklif == "1NT"
                && durum.Rakipler.Contains(gercekTeklifler[0].Oyuncu)
                && gercekTeklifler[1].Oyuncu == durum.AktifOyuncu
                && gercekTeklifler[1].Teklif == "2♣"
                && gercekTeklifler[2].Teklif == "2♦"
                && durum.Rakipler.Contains(gercekTeklifler[2].Oyuncu)
                && durum.Rakipler.Contains(gercekTeklifler.Last().Oyuncu)
                && durum.PartnerTeklifleri.Count > 0
                && durum.PartnerTeklifleri.Last() == "Dbl")
            {
                return true;
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VER
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null || el.Count != 13) return "Pas";

            var gercekTeklifler = durum.Gecmis
                .Where(h => h.GercekTeklifMi)
                .ToList();

            int n = gercekTeklifler.Count;
            var sonTeklif = gercekTeklifler[n - 1];

            // Rakip 1NT açtı, biz müdahale ediyoruz
            if (sonTeklif.Teklif == "1NT"
                && gercekTeklifler.Count == 1
                && durum.Rakipler.Contains(sonTeklif.Oyuncu))
            {
                return MudahaleTeklifi(durum, el);
            }

            // Partnerimiz Cappelletti müdahalesi yaptı, biz cevap veriyoruz
            if (durum.PartnerTeklifleri.Count > 0)
            {
                return PartnerCevabi(durum, el, gercekTeklifler);
            }

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // RAKİP 1NT'YE MÜDAHALE
        // ═══════════════════════════════════════════════════════════════════

        private string MudahaleTeklifi(IhaleDurumu durum, List<Card> el)
        {
            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);

            // ─── Dbl = Ceza (15+ HP dengeli) ───
            if (hp >= 15 && dengeli)
                return "Dbl";

            // 9-14 HP arası yapay müdahaleler
            if (hp < 8) return "Pas";

            // Renk uzunlukları
            int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            // ─── 2♦ = Majörler (5-5 veya 5-4) ───
            if (macca >= 5 && kupa >= 5)
                return "2♦";
            if (macca >= 5 && kupa >= 4)
                return "2♦";
            if (kupa >= 5 && macca >= 4)
                return "2♦";

            // ─── 2♥ = ♥ + bir minör (5-5) ───
            if (kupa >= 5)
            {
                if (karo >= 5 || sinek >= 5)
                    return "2♥";
            }

            // ─── 2♠ = ♠ + bir minör (5-5) ───
            if (macca >= 5)
            {
                if (karo >= 5 || sinek >= 5)
                    return "2♠";
            }

            // ─── 2NT = Minörler (5-5) ───
            if (karo >= 5 && sinek >= 5)
                return "2NT";

            // ─── 2♣ = Tek renkli el (6+ kart) ───
            if (macca >= 6) return "2♣";
            if (kupa >= 6) return "2♣";
            if (karo >= 6) return "2♣";
            if (sinek >= 6) return "2♣";

            return "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // PARTNERİN CAPPELLETTI MÜDAHALESİNE CEVAP
        // ═══════════════════════════════════════════════════════════════════

        private static int SuitRank(string koz)
        {
            return koz == "Sinek" ? 1 : koz == "Karo" ? 2 : koz == "Kupa" ? 3 : 4;
        }

        private string PartnerCevabi(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            var partnerTeklif = durum.PartnerTeklifleri.Last();

            // ─── Ben 2♣ (tek renkli) dedim, partner 2♦ röle dedi → uzun rengimi söylerim ───
            var benimGercekler = durum.KendiTeklifleri.Where(t => !string.IsNullOrEmpty(t) && t != "Pas" && t != "Dbl" && t != "RDbl").ToList();
            if ((partnerTeklif == "2♦" || (partnerTeklif == "Dbl" && gercekTeklifler.Count >= 3 && gercekTeklifler[2].Teklif == "2♦"))
                && benimGercekler.Count > 0 && benimGercekler.Last() == "2♣")
            {
                int m = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int k = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
                int en = Math.Max(Math.Max(m, k), Math.Max(d, c));
                // Rakip araya girdiyse (ör. 2♦ - Dbl - 3♦) uzun rengimi yasal en ucuz seviyede söylerim
                var son = gercekTeklifler.Last();
                int sonSv = son.Seviye;
                int sonRk = son.Koz == "NT" ? 5 : SuitRank(son.Koz);
                string renk = m == en ? "Maça" : k == en ? "Kupa" : c == en ? "Sinek" : "Karo";
                string sem = renk == "Maça" ? "♠" : renk == "Kupa" ? "♥" : renk == "Sinek" ? "♣" : "♦";
                int rk = SuitRank(renk);
                int sv = (rk > sonRk) ? sonSv : sonSv + 1;
                if (sv < 2) sv = 2;
                if (sv > 3) return "Pas";
                return sv + sem;
            }

            // ─── Partner 2♣ dedi (tek renkli) → 2♦ röle ───
            if (partnerTeklif == "2♣")
            {
                var son = gercekTeklifler.Last();
                if (son.Teklif == "2♣") return "2♦";                 // normal röle
                if (son.Teklif == "2♦" && durum.Rakipler.Contains(son.Oyuncu))
                    return "Dbl";                                    // rakip 2♦ dedi: kontra = röle
                return "Pas";                                        // başka müdahale: Temel Mantık karar versin
            }

            // ─── Partner 2♦ dedi (majörler) → en uzun majör ───
            if (partnerTeklif == "2♦")
            {
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                if (macca >= kupa) return "2♠";
                return "2♥";
            }

            // ─── Partner 2♥ dedi (♥ + minör) ───
            if (partnerTeklif == "2♥")
            {
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                if (kupa >= 3) return "Pas"; // ♥ toleransı
                return "2NT"; // minör sorusu
            }

            // ─── Partner 2♠ dedi (♠ + minör) ───
            if (partnerTeklif == "2♠")
            {
                int macca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                if (macca >= 3) return "Pas"; // ♠ toleransı
                return "2NT"; // minör sorusu
            }

            // ─── Partner 2NT dedi (minörler) → en uzun minör ───
            if (partnerTeklif == "2NT")
            {
                int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
                int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
                if (karo >= sinek) return "3♦";
                return "3♣";
            }

            return "Pas";
        }
    }

    /// <summary>
    /// Cappelletti 2♣ (tek renkli) sonrası rakip 2♦ dedi, ben Dbl ile röle yaptım, ortak rengini söyledi:
    /// kabul (Pas) — güçlüysem majörde oyun. Temel Mantığın kendi rengime (ör. 4♣) kaçmasını önler.
    /// </summary>
    public class CappellettiRoleDevam : IKonvansiyon, IBilincliPas
    {
        public string Ad => "Cappelletti Röle Devam";
        public string Aciklama => "Cappelletti röle (Dbl) sonrası ortağın söylediği renge karar";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 12;

        public bool UygunMu(IhaleDurumu durum) { return Bul(durum) != null; }
        public string TeklifVer(IhaleDurumu durum) { return Bul(durum) ?? "Pas"; }

        private static string Bul(IhaleDurumu d)
        {
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13) return null;
            if (d.IhaleBittiMi()) return null;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            var ortak = IhaleDurumu.PartnerBul(d.AktifOyuncu);
            if (g.Count < 4) return null;
            if (g[0].Teklif != "1NT" || !d.Rakipler.Contains(g[0].Oyuncu)) return null;
            if (g[1].Oyuncu != ortak || g[1].Teklif != "2♣") return null;
            if (g[2].Teklif != "2♦" || !d.Rakipler.Contains(g[2].Oyuncu)) return null;
            if (g.Any(h => h.Oyuncu == d.AktifOyuncu)) return null;
            if (!d.KendiTeklifleri.Contains("Dbl")) return null;
            var son = g[g.Count - 1];
            if (son.Oyuncu != ortak || g.Count < 4) return null;
            string t = son.Teklif;
            if (t.Length != 2 || !char.IsDigit(t[0])) return null;
            int sv = t[0] - '0';
            char renk = t[1];
            if (renk != '♥' && renk != '♠') return "Pas";
            string sz = renk == '♠' ? "Maça" : "Kupa";
            int hp = ElDegerlendirici.HCP(d.AktifOyuncuEli);
            int uz = ElDegerlendirici.RenkUzunlugu(d.AktifOyuncuEli, sz);
            if (uz >= 2 && hp >= 15 && sv <= 3) return "4" + renk;
            return "Pas";
        }
    }
}