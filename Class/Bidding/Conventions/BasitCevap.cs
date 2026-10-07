using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Basit Cevap Konvansiyonu.
    /// 
    /// Partner bir renk açtığında (1♠/1♥/1♦/1♣), bu konvansiyon cevabı belirler.
    /// 
    /// Kural:
    /// - 0-5 HP → Pas
    /// - 6-9 HP → Basit destek (2♠/2♥/2♦/2♣) veya yeni renk (1♠/1♥/1♦/1♣)
    /// - 10-12 HP → 2NT (dengeli) veya 3♠/3♥ (limit raise) veya yeni renk
    /// - 13+ HP → Yeni renk 2 seviyesinde (2/1 game force)
    /// 
    /// Öncelik: 100 (fallback'ten önce)
    /// 
    /// Örnek:
    ///   Partner: 1♠
    ///   Ben: 6-9 HP + 3+ Maça → 2♠
    ///   Ben: 6-9 HP + 5+ Kupa → 2♥ (yeni renk)
    ///   Ben: 10-12 HP dengeli → 2NT
    ///   Ben: 13+ HP + 5+ Kupa → 2♥ (2/1 GF)
    /// </summary>
    public class BasitCevap : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "Basit Cevap";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 100;   // Fallback'ten önce

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. Partner açtı mı?
            if (!durum.PartnerActiMi()) return false;

            // 2. Rakip müdahale etti mi? (Etmediyse basit cevap)
            if (durum.RakipActiMi()) return false;

            // 3. Ben henüz cevap vermedim mi?
            if (durum.KendiTeklifleri.Count > 0) return false;

            // 4. Son teklif partnerin mi?
            if (durum.SonGercekTeklifSahibi != durum.Partner) return false;

            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);

            // Partnerin son teklifi
            string partnerTeklifi = durum.PartnerTeklifleri.LastOrDefault();
            string partnerKozu = TekliftenKozCikar(partnerTeklifi);
            int partnerSeviye = TekliftenSeviyeCikar(partnerTeklifi);

            // ─── 0-5 HP → Pas ───
            if (hp < 6)
            {
                return "Pas";
            }

            // ─── 6-9 HP → Basit destek veya yeni renk ───
            if (hp < 10)
            {
                // Partner majör açtıysa ve 3+ destek varsa → basit destek
                if (!string.IsNullOrEmpty(partnerKozu) && partnerSeviye == 1)
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 3)
                    {
                        string kozSembol = KozSembolu(partnerKozu);
                        return "2" + kozSembol;
                    }
                }

                // Yeni renk (1 seviye) — 4+ kart, majör öncelikli
                string yeniRenk = YeniRenkBul(el, partnerKozu, 4, "1");
                if (!string.IsNullOrEmpty(yeniRenk)) return yeniRenk;

                // Yeni renk yoksa → 1NT (dengeli)
                if (ElDegerlendirici.DengeliEl(el)) return "1NT";

                // Son çare: partnerin rengine 2 seviye destek
                if (!string.IsNullOrEmpty(partnerKozu))
                {
                    string kozSembol = KozSembolu(partnerKozu);
                    return "2" + kozSembol;
                }

                return "Pas";
            }

            // ─── 10-12 HP → 2NT, limit raise veya yeni renk ───
            if (hp < 16)
            {
                // Dengeli → 2NT
                if (ElDegerlendirici.DengeliEl(el)) return "2NT";

                // Destek varsa → 3♠ (limit raise)
                if (!string.IsNullOrEmpty(partnerKozu))
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 4 && partnerSeviye == 1)
                    {
                        string kozSembol = KozSembolu(partnerKozu);
                        return "3" + kozSembol;
                    }
                }

                // Yeni renk
                string yeniRenk = YeniRenkBul(el, partnerKozu, 4, "1");
                if (!string.IsNullOrEmpty(yeniRenk)) return yeniRenk;

                return "2NT";
            }

            // ─── 13+ HP → 2/1 Game Force (yeni renk 2 seviyesinde) ───
            // Öncelik 1: 5+ majör → 2♠ veya 2♥
            string gfRenk = YeniRenkBul(el, partnerKozu, 5, "2");
            if (!string.IsNullOrEmpty(gfRenk)) return gfRenk;

            // Öncelik 2: 4+ minör → 2♣ veya 2♦
            gfRenk = YeniRenkBul(el, partnerKozu, 4, "2");
            if (!string.IsNullOrEmpty(gfRenk)) return gfRenk;

            // Öncelik 3: Destek varsa → 3♠/3♥ (forcing raise)
            if (!string.IsNullOrEmpty(partnerKozu))
            {
                string kozSembol = KozSembolu(partnerKozu);
                return "3" + kozSembol;
            }

            // Son çare: 2NT
            return "2NT";
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI METOTLAR
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Yeni renk bulur (partnerin rengi hariç).
        /// Majör öncelikli, sonra minör.
        /// </summary>
        private string YeniRenkBul(List<Card> el, string partnerKozu, int minUzunluk, string seviye)
        {
            // Majör öncelikli
            string[] oncelik = { "Maça", "Kupa", "Karo", "Sinek" };
            string[] semboller = { "♠", "♥", "♦", "♣" };

            for (int i = 0; i < oncelik.Length; i++)
            {
                // Partnerin rengi ise atla
                if (oncelik[i] == partnerKozu) continue;

                int uzunluk = ElDegerlendirici.RenkUzunlugu(el, oncelik[i]);
                if (uzunluk >= minUzunluk)
                {
                    return seviye + semboller[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Tekliften kozu çıkarır.
        /// "1♠" → "Maça"
        /// </summary>
        private string TekliftenKozCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2) return null;

            char sonKarakter = teklif[teklif.Length - 1];
            switch (sonKarakter)
            {
                case '♠': return "Maça";
                case '♥': return "Kupa";
                case '♦': return "Karo";
                case '♣': return "Sinek";
                case 'T':
                case 't': return "NT";
                default: return null;
            }
        }

        /// <summary>
        /// Tekliften seviyeyi çıkarır.
        /// "2♠" → 2
        /// </summary>
        private int TekliftenSeviyeCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2) return 0;

            string seviyeStr = teklif.Substring(0, teklif.Length - 1);
            if (int.TryParse(seviyeStr, out int seviye)) return seviye;
            return 0;
        }

        /// <summary>
        /// Koz adını sembole çevirir.
        /// "Maça" → "♠"
        /// </summary>
        private string KozSembolu(string koz)
        {
            switch (koz)
            {
                case "Maça": return "♠";
                case "Kupa": return "♥";
                case "Karo": return "♦";
                case "Sinek": return "♣";
                case "NT": return "NT";
                default: return "?";
            }
        }
    }
}