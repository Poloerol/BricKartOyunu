using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Takeout Double Cevabı (Advancer) — Rakip 1 seviyesinde renk açtı, partner Dbl
    /// (takeout) yaptı, sağdaki rakip Pas dedi, sıra bende.
    ///
    /// Cevap tablosu:
    ///   Ceza pası : Rakip renginde 5+ kart ve en az 3 üst onör (A K Q J 10) → Pas
    ///   12+ HP    : 13+ dengeli + rakip rengi durdurucu + 4'lü yan majör yok → 3NT
    ///               aksi halde rakip rengine cue-bid
    ///   9-11 HP   : 4+ yan majör → atlayarak davet; dengeli + durdurucu → 2NT;
    ///               4+ yan minör → atlayarak davet; aksi halde 2NT / cue-bid
    ///   0-8 HP    : 6+ HP, dengeli, durdurucu, 4'lü yan majör yok → 1NT;
    ///               aksi halde en uzun yan renk, en ucuz seviyede
    ///
    /// Öncelik 4: Sadece çok dar bir ihale dizisinde (Rakip 1x - Partner Dbl - Pas)
    /// uygun olduğu için diğer konvansiyonlarla çakışmaz.
    /// </summary>
    public class TakeoutDoubleCevap : IKonvansiyon
    {
        public string Ad => "Takeout Double Cevap";
        public string Aciklama => "Partnerin takeout double'ına cevap (ceza pası, cue-bid, davet, zayıf renk)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 4;

        // Düşükten yükseğe renk sırası
        private static readonly string[] Renkler = { "Sinek", "Karo", "Kupa", "Maça" };

        private static int Sira(string renk)
        {
            return Array.IndexOf(Renkler, renk);
        }

        private static string Sembol(string renk)
        {
            switch (renk)
            {
                case "Sinek": return "♣";
                case "Karo": return "♦";
                case "Kupa": return "♥";
                case "Maça": return "♠";
                default: return "";
            }
        }

        /// <summary>
        /// Dizi: [Pas...] Rakip 1x - Partner Dbl - Rakip Pas - ?
        /// Uygunsa rakip rengini döner.
        /// </summary>
        private bool DiziUygunMu(IhaleDurumu durum, out string rakipRenk)
        {
            rakipRenk = null;
            if (durum.Gecmis == null) return false;

            var liste = durum.Gecmis.ToList();
            if (liste.Count < 2) return false;

            // Benden hemen önceki teklif: sağdaki rakibin Pas'ı
            var son = liste[liste.Count - 1];
            if (son.Teklif != "Pas" || !durum.Rakipler.Contains(son.Oyuncu)) return false;

            // Pas olmayan teklifler tam olarak 2 tane: Rakip açılışı + Partner Dbl
            var gercek = liste.Where(h => h.Teklif != "Pas").ToList();
            if (gercek.Count != 2) return false;

            if (!durum.Rakipler.Contains(gercek[0].Oyuncu)) return false;
            if (gercek[1].Oyuncu != durum.Partner || gercek[1].Teklif != "Dbl") return false;

            if (KozYardimcisi.TekliftenSeviyeCikar(gercek[0].Teklif) != 1) return false;

            string koz = KozYardimcisi.TekliftenKozCikar(gercek[0].Teklif);
            if (string.IsNullOrEmpty(koz) || koz == "NT") return false;
            if (Sira(koz) < 0) return false;

            rakipRenk = koz;
            return true;
        }

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            string rakipRenk;
            return DiziUygunMu(durum, out rakipRenk);
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            string rakipRenk;
            if (!DiziUygunMu(durum, out rakipRenk)) return "Pas";

            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            bool dengeli = ElDegerlendirici.DengeliEl(el);
            bool durdurucu = ElDegerlendirici.StopperVar(el, rakipRenk);

            // ─── Ceza pası: rakip renginde 5+ kart ve 3+ üst onör ───
            int rakipUzunluk = ElDegerlendirici.RenkUzunlugu(el, rakipRenk);
            int ustOnor = el.Count(c => c.Suit == rakipRenk && c.Value >= 10);
            if (rakipUzunluk >= 5 && ustOnor >= 3)
                return "Pas";

            // Yan renkler (rakip rengi hariç)
            var yanRenkler = Renkler.Where(r => r != rakipRenk).ToList();
            bool yanMajor4 = yanRenkler.Any(r => (r == "Kupa" || r == "Maça")
                                              && ElDegerlendirici.RenkUzunlugu(el, r) >= 4);

            // ─── 12+ HP: 3NT veya cue-bid ───
            if (hp >= 12)
            {
                if (hp >= 13 && dengeli && durdurucu && !yanMajor4)
                    return "3NT";
                return CueBid(rakipRenk);
            }

            // ─── 9-11 HP: davet ───
            if (hp >= 9)
            {
                string majorRenk = EnUzunYanRenk(el, rakipRenk, true);
                if (majorRenk != null && ElDegerlendirici.RenkUzunlugu(el, majorRenk) >= 4)
                    return Teklif(SeviyeEnUcuz(majorRenk, rakipRenk) + 1, majorRenk);

                if (dengeli && durdurucu)
                    return "2NT";

                string renk = EnUzunYanRenk(el, rakipRenk, false);
                if (renk != null && ElDegerlendirici.RenkUzunlugu(el, renk) >= 4)
                    return Teklif(SeviyeEnUcuz(renk, rakipRenk) + 1, renk);

                return durdurucu ? "2NT" : CueBid(rakipRenk);
            }

            // ─── 0-8 HP: zayıf ───
            if (hp >= 6 && dengeli && durdurucu && !yanMajor4)
                return "1NT";

            string zayifRenk = EnUzunYanRenk(el, rakipRenk, false);
            if (zayifRenk == null) return "Pas";
            return Teklif(SeviyeEnUcuz(zayifRenk, rakipRenk), zayifRenk);
        }

        private static string CueBid(string rakipRenk)
        {
            return "2" + Sembol(rakipRenk);
        }

        private static string Teklif(int seviye, string renk)
        {
            return seviye + Sembol(renk);
        }

        /// <summary>Rakibin 1x açılışı üzerine bu rengi en ucuz hangi seviyede söyleriz?</summary>
        private static int SeviyeEnUcuz(string renk, string rakipRenk)
        {
            return Sira(renk) > Sira(rakipRenk) ? 1 : 2;
        }

        /// <summary>
        /// Rakip rengi dışındaki en uzun renk. Eşitlikte yüksek renk tercih edilir.
        /// sadeceMajor=true ise yalnız ♥/♠ içinden seçer.
        /// </summary>
        private static string EnUzunYanRenk(List<Card> el, string rakipRenk, bool sadeceMajor)
        {
            return Renkler
                .Where(r => r != rakipRenk)
                .Where(r => !sadeceMajor || r == "Kupa" || r == "Maça")
                .OrderByDescending(r => ElDegerlendirici.RenkUzunlugu(el, r))
                .ThenByDescending(r => Sira(r))
                .FirstOrDefault();
        }
    }
}
