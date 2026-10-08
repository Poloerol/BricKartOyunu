using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Basit Cevap Konvansiyonu (SAYC Standard).
    ///
    /// Partner bir renk açtığında (1♠/1♥/1♦/1♣), bu konvansiyon cevabı belirler.
    ///
    /// Genel Kurallar:
    /// - 0-5 HP → Pas
    /// - 6-9 HP → Basit destek (2 seviyesi) veya yeni renk (1 seviyesi)
    /// - 10-12 HP → 2NT (dengeli invite) veya limit raise (3 seviyesi majör) veya yeni renk
    /// - 13+ HP → Yeni renk 2 seviyesinde (2/1 Game Force) veya kuvvetli destek
    /// </summary>
    public class BasitCevap : IKonvansiyon
    {
        public string Ad => "Basit Cevap";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 100;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            if (!durum.PartnerActiMi()) return false;
            if (durum.RakipActiMi()) return false;
            if (durum.KendiTeklifleri.Count > 0) return false;
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);

            string partnerTeklifi = durum.PartnerTeklifleri.LastOrDefault();
            string partnerKozu = TekliftenKozCikar(partnerTeklifi);
            int partnerSeviye = TekliftenSeviyeCikar(partnerTeklifi);

            // ─── 0-5 HP → Pas ───
            if (hp < 6) return "Pas";

            // ─── 6-9 HP → Basit Destek / Yeni Renk ───
            if (hp < 10)
            {
                // Partner majör açtıysa ve 3+ destek varsa → basit destek
                if (!string.IsNullOrEmpty(partnerKozu) && partnerSeviye == 1)
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 3)
                    {
                        return "2" + KozSembolu(partnerKozu);
                    }
                }

                // Yeni renk (1 seviye) — 4+ kart, majör öncelikli
                string yeniRenk = YeniRenkBul(el, partnerKozu, 4, "1");
                if (!string.IsNullOrEmpty(yeniRenk)) return yeniRenk;

                // Dengeli/Yarı-Dengeli ise → 1NT (zayıf/orta)
                if (ElDegerlendirici.DengeliEl(el) || ElDegerlendirici.YariDengeliEl(el))
                    return "1NT";

                // Son çare
                if (!string.IsNullOrEmpty(partnerKozu))
                    return "2" + KozSembolu(partnerKozu);

                return "Pas";
            }

            // ─── 10-15 HP → Invitational / Limit ───
            if (hp < 16)
            {
                // 1. ÖNCE 4'lü MAJÖR göster (SAYC önceliği)
                if (partnerKozu != "Maça" && ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4)
                    return "1♠";
                if (partnerKozu != "Kupa" && ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4)
                    return "1♥";

                // 2. Partnerin majörüne kuvvetli destek (4+ kart) → Limit Raise
                if (!string.IsNullOrEmpty(partnerKozu) && (partnerKozu == "Maça" || partnerKozu == "Kupa"))
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 4 && partnerSeviye == 1)
                    {
                        return "3" + KozSembolu(partnerKozu);
                    }
                }

                // 3. Dengeli/Yarı-Dengeli ise → 2NT (Invite)
                if (ElDegerlendirici.DengeliEl(el) || ElDegerlendirici.YariDengeliEl(el))
                    return "2NT";

                // 4. 5+ minör varsa → 2 seviyesi (forcing)
                string minör = YeniRenkBul(el, partnerKozu, 5, "2");
                if (!string.IsNullOrEmpty(minör)) return minör;

                return "2NT";
            }

            // ─── 16+ HP → 2/1 Game Force ───
            // 1. 5+ majör öncelikli
            string gfRenk = YeniRenkBul(el, partnerKozu, 5, "2");
            if (!string.IsNullOrEmpty(gfRenk)) return gfRenk;

            // 2. 4+ herhangi bir renk
            gfRenk = YeniRenkBul(el, partnerKozu, 4, "2");
            if (!string.IsNullOrEmpty(gfRenk)) return gfRenk;

            // 3. Çok kuvvetli destek → 3 seviyesi forcing
            if (!string.IsNullOrEmpty(partnerKozu))
            {
                return "3" + KozSembolu(partnerKozu);
            }

            return "2NT";
        }

        private string YeniRenkBul(List<Card> el, string partnerKozu, int minUzunluk, string seviye)
        {
            string[] oncelik = { "Maça", "Kupa", "Karo", "Sinek" };
            string[] semboller = { "♠", "♥", "♦", "♣" };

            for (int i = 0; i < oncelik.Length; i++)
            {
                if (oncelik[i] == partnerKozu) continue;
                if (ElDegerlendirici.RenkUzunlugu(el, oncelik[i]) >= minUzunluk)
                {
                    return seviye + semboller[i];
                }
            }
            return null;
        }

        private string TekliftenKozCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2) return null;
            char son = teklif[teklif.Length - 1];
            return son switch { '♠' => "Maça", '♥' => "Kupa", '♦' => "Karo", '♣' => "Sinek", 'T' => "NT", 't' => "NT", _ => null };
        }

        private int TekliftenSeviyeCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2) return 0;
            if (int.TryParse(teklif.Substring(0, teklif.Length - 1), out int s)) return s;
            return 0;
        }

        private string KozSembolu(string koz)
        {
            return koz switch { "Maça" => "♠", "Kupa" => "♥", "Karo" => "♦", "Sinek" => "♣", "NT" => "NT", _ => "?" };
        }
    }
}
