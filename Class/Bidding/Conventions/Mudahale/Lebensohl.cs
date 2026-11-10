using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Lebensohl Konvansiyonu (1NT Overcall).
    /// 
    /// SENARYO: 1NT (Partner) - 2♣/2♦/2♥/2♠ (Rakip) - ?
    /// 
    /// AMAÇ:
    /// 1. Zayıf (0-7 OP) ve davet (8-9 OP) ve kuvvetli (10+ OP) elleri ayırt etmek
    /// 2. Rakip renginde durdurucu olup olmadığını göstermek
    /// 3. Stayman'ı rakibin renginde durdurucu ile birlikte yapmak
    /// 
    /// Öncelik: 8 (Stayman'dan önce)
    /// </summary>
    public class Lebensohl : IKonvansiyon
    {
        public string Ad => "Lebensohl";
        public string Aciklama => "1NT müdahalesine Lebensohl cevapları";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 8;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // 1. Partner 1NT açtı mı?
            if (durum.PartnerTeklifleri.Count == 0) return false;
            if (durum.PartnerTeklifleri.LastOrDefault() != "1NT") return false;

            // 2. Rakip 2♣/2♦/2♥/2♠ ile araya girdi mi?
            if (durum.RakipTeklifleri.Count == 0) return false;
            string rakipSon = durum.RakipTeklifleri.LastOrDefault();
            if (rakipSon != "2♥" && rakipSon != "2♠" &&
                rakipSon != "2♣" && rakipSon != "2♦") return false;

            // 3. Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            string rakipSon = durum.RakipTeklifleri.LastOrDefault();
            int hp = ElDegerlendirici.HCP(el);

            string rakipKozu = rakipSon == "2♥" ? "Kupa" :
                               rakipSon == "2♠" ? "Maça" :
                               rakipSon == "2♣" ? "Sinek" : "Karo";

            // ═══════════════════════════════════════════════════════════════
            // DURUM 1: İlk cevabım
            // ═══════════════════════════════════════════════════════════════
            if (durum.KendiTeklifleri.Count == 0)
            {
                int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                bool durdurucuVar = ElDegerlendirici.StopperVar(el, rakipKozu);
                bool besliMaca = maca >= 5;
                bool besliKupa = kupa >= 5;

                // ───────────────────────────────────────────────────────────
                // ALT DURUM: Rakip 2♣
                // ───────────────────────────────────────────────────────────
                if (rakipSon == "2♣")
                {
                    // Texas transfer (6+ majör)
                    if (maca >= 6 && hp >= 10) return "4♥";
                    if (kupa >= 6 && hp >= 10) return "4♦";

                    // 5'li majör + 10+ OP (forcing)
                    if (hp >= 10)
                    {
                        if (besliMaca) return "3♠";
                        if (besliKupa) return "3♥";
                    }

                    // 0-7 OP
                    if (hp <= 7)
                    {
                        if (besliKupa) return "2♥";
                        if (besliMaca) return "2♠";
                        if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 5) return "2♦";
                        return "2NT";
                    }

                    // 8-9 OP davet
                    if (hp >= 8 && hp <= 9)
                    {
                        if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 6) return "3♦";
                        return "3♦";
                    }

                    // 10+ OP kuvvetli
                    if (hp >= 10)
                    {
                        bool dortluMaca = maca >= 4;
                        bool dortluKupa = kupa >= 4;

                        if (durdurucuVar && !dortluMaca && !dortluKupa)
                            return "3NT";

                        if (!durdurucuVar)
                            return "3♣";  // Cue-bid Stayman

                        return "3NT";
                    }

                    return "2NT";
                }

                // ───────────────────────────────────────────────────────────
                // ALT DURUM: Rakip 2♦
                // ───────────────────────────────────────────────────────────
                if (rakipSon == "2♦")
                {
                    // Texas transfer
                    if (maca >= 6 && hp >= 10) return "4♥";
                    if (kupa >= 6 && hp >= 10) return "4♦";

                    // 5'li majör + 10+ OP
                    if (hp >= 10)
                    {
                        if (besliMaca) return "3♠";
                        if (besliKupa) return "3♥";
                    }

                    // 0-7 OP
                    if (hp <= 7)
                    {
                        if (besliKupa) return "2♥";
                        if (besliMaca) return "2♠";
                        return "2NT";
                    }

                    // 8-9 OP davet
                    if (hp >= 8 && hp <= 9)
                    {
                        return "3♣";
                    }

                    // 10+ OP kuvvetli
                    if (hp >= 10)
                    {
                        bool dortluMaca = maca >= 4;
                        bool dortluKupa = kupa >= 4;

                        if (durdurucuVar && !dortluMaca && !dortluKupa)
                            return "3NT";

                        if (!durdurucuVar)
                            return "3♦";  // Cue-bid Stayman

                        return "3NT";
                    }

                    return "2NT";
                }

                // ───────────────────────────────────────────────────────────
                // ALT DURUM: Rakip 2♥ veya 2♠ (eski kod)
                // ───────────────────────────────────────────────────────────

                // Texas transfer (6+ majör)
                if (maca >= 6 && hp >= 10) return "4♥";
                if (kupa >= 6 && hp >= 10) return "4♦";

                // 5'li majör + 10+ OP (forcing)
                if (hp >= 10)
                {
                    if (rakipSon == "2♥" && besliMaca) return "3♠";
                    if (rakipSon == "2♠" && besliKupa) return "3♥";
                }

                // 0-7 OP: Zayıf natürel
                if (hp <= 7)
                {
                    if (rakipSon == "2♥" && maca >= 5) return "2♠";
                    return "2NT";
                }

                // 8-9 OP davet
                if (hp >= 8 && hp <= 9)
                {
                    if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 6) return "3♦";
                    if (ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 6) return "3♣";
                    return "3♣";
                }

                // 10+ OP kuvvetli
                if (hp >= 10)
                {
                    bool dortluMaca = maca >= 4;
                    bool dortluKupa = kupa >= 4;

                    if (durdurucuVar && !dortluMaca && !dortluKupa)
                        return "3NT";

                    if (!durdurucuVar)
                    {
                        if (rakipSon == "2♥" && dortluMaca) return "3♥";
                        if (rakipSon == "2♠" && dortluKupa) return "3♠";
                    }

                    return "3NT";
                }

                return "2NT";
            }

            // ═══════════════════════════════════════════════════════════════
            // DURUM 2: Lebensohl 2NT sonrası (partner 3♣ dedi)
            // ═══════════════════════════════════════════════════════════════
            if (durum.KendiTeklifleri.Contains("2NT") &&
                durum.PartnerTeklifleri.LastOrDefault() == "3♣")
            {
                if (hp <= 7)
                {
                    int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                    int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                    if (rakipSon == "2♥" && maca >= 5) return "3♠";
                    if (rakipSon == "2♠" && kupa >= 5) return "3♥";
                    if (rakipSon == "2♣" && (maca >= 5 || kupa >= 5))
                        return maca >= 5 ? "3♠" : "3♥";
                    if (rakipSon == "2♦" && (maca >= 5 || kupa >= 5))
                        return maca >= 5 ? "3♠" : "3♥";

                    if (ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 6) return "3♦";
                    return "Pas";
                }

                if (hp >= 8 && hp <= 9)
                {
                    int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                    int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");

                    if (rakipSon == "2♥" && maca >= 5) return "3♠";
                    if (rakipSon == "2♠" && kupa >= 5) return "3♥";
                    if (rakipSon == "2♣" && (maca >= 5 || kupa >= 5))
                        return maca >= 5 ? "3♠" : "3♥";
                    if (rakipSon == "2♦" && (maca >= 5 || kupa >= 5))
                        return maca >= 5 ? "3♠" : "3♥";

                    return "3NT";
                }

                if (hp >= 10)
                {
                    int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
                    int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
                    bool durdurucuVar = ElDegerlendirici.StopperVar(el, rakipKozu);

                    if (durdurucuVar && maca < 4 && kupa < 4) return "3NT";

                    if (durdurucuVar)
                    {
                        if (rakipSon == "2♥" && maca >= 4) return "3♥";
                        if (rakipSon == "2♠" && kupa >= 4) return "3♠";
                        if (rakipSon == "2♣" && (maca >= 4 || kupa >= 4))
                            return maca >= 4 ? "3♠" : "3♥";
                        if (rakipSon == "2♦" && (maca >= 4 || kupa >= 4))
                            return maca >= 4 ? "3♠" : "3♥";
                    }

                    return "3NT";
                }
            }

            return "Pas";
        }
    }

    /// <summary>
    /// 1NT açıcısı: rakip 2 seviyesinde araya girdi, ortak 2 seviyesinde bir renk söyledi (Lebensohl: doğal, zayıf, oynamak için).
    /// Bu çağrı transfer değildir ve zorlayıcı değildir → açıcı Pas der.
    /// </summary>
    public class LebensohlDevam : IKonvansiyon, IBilincliPas
    {
        public string Ad => "Lebensohl Devam";
        public string Aciklama => "Lebensohl: ortağın 2 seviyesindeki doğal/zayıf cevabına açıcı Pas der";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 9;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;
            var g = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            var ortak = IhaleDurumu.PartnerBul(durum.AktifOyuncu);
            if (g.Count != 3) return false;
            if (g[0].Oyuncu != durum.AktifOyuncu || g[0].Teklif != "1NT") return false;
            if (!durum.Rakipler.Contains(g[1].Oyuncu) || g[1].Seviye != 2 || g[1].Koz == "NT") return false;
            if (g[2].Oyuncu != ortak || g[2].Seviye != 2 || g[2].Koz == "NT" || g[2].Koz == g[1].Koz) return false;
            // 4+ kart desteğim varsa rekabete devam etmek serbest (Temel Mantık karar versin)
            if (ElDegerlendirici.RenkUzunlugu(durum.AktifOyuncuEli, g[2].Koz) >= 4) return false;
            // Son hamle ortağın teklifinden sonra rakibin Pas'ı (ya da ortağın teklifi hemen önceki)
            var son = durum.Gecmis[durum.Gecmis.Count - 1];
            if (son.Oyuncu == ortak) return true;
            return son.PasMi && durum.Rakipler.Contains(son.Oyuncu);
        }

        public string TeklifVer(IhaleDurumu durum) { return "Pas"; }
    }
}