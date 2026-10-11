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

        private string PartnerCevabi(IhaleDurumu durum, List<Card> el,
            List<IhaleHamlesi> gercekTeklifler)
        {
            var partnerTeklif = durum.PartnerTeklifleri.Last();

            // ─── Partner 2♣ dedi (tek renkli) → 2♦ röle ───
            if (partnerTeklif == "2♣")
            {
                // Zayıf el varsa 2♦ röle
                return "2♦";
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
}