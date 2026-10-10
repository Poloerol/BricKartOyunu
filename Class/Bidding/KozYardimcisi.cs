using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// Koz (renk) ile ilgili ortak yardımcı metotlar.
    /// 
    /// Tüm konvansiyonlar bu sınıfı kullanır.
    /// Kod tekrarını önler.
    /// </summary>
    public static class KozYardimcisi
    {
        // ═══════════════════════════════════════════════════════════════════
        // TEKLİFTEN BİLGİ ÇIKARMA
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Tekliften kozu çıkarır.
        /// "1♠" → "Maça"
        /// "3NT" → "NT"
        /// </summary>
        public static string TekliftenKozCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif)) return null;

            // NT özel durumu: "1NT", "3NT", vs.
            if (teklif.EndsWith("NT"))
                return "NT";

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
        /// "1♠" → 1
        /// "1NT" → 1
        /// "3NT" → 3
        /// </summary>
        public static int TekliftenSeviyeCikar(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2) return 0;

            string seviyeStr;

            // NT özel durumu: "1NT" → "1"
            if (teklif.EndsWith("NT"))
            {
                seviyeStr = teklif.Substring(0, teklif.Length - 2);
            }
            else
            {
                seviyeStr = teklif.Substring(0, teklif.Length - 1);
            }

            if (int.TryParse(seviyeStr, out int seviye)) return seviye;
            return 0;
        }

        // ═══════════════════════════════════════════════════════════════════
        // KOZ SEMBOLÜ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Koz adını sembole çevirir.
        /// "Maça" → "♠"
        /// "NT" → "NT"
        /// </summary>
        public static string KozSembolu(string koz)
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

        /// <summary>
        /// Sembolden koz adını çıkarır.
        /// "♠" → "Maça"
        /// </summary>
        public static string SemboldenKoz(string sembol)
        {
            switch (sembol)
            {
                case "♠": return "Maça";
                case "♥": return "Kupa";
                case "♦": return "Karo";
                case "♣": return "Sinek";
                case "NT": return "NT";
                default: return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF OLUŞTURMA
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Seviye ve kozdan teklif metni oluşturur.
        /// (3, "Maça") → "3♠"
        /// </summary>
        public static string TeklifOlustur(int seviye, string koz)
        {
            return seviye + KozSembolu(koz);
        }

        // ═══════════════════════════════════════════════════════════════════
        // KOZ KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Belirtilen koz majör mü? (Maça veya Kupa)
        /// </summary>
        public static bool MajorMu(string koz)
        {
            return koz == "Maça" || koz == "Kupa";
        }

        /// <summary>
        /// Belirtilen koz minör mü? (Karo veya Sinek)
        /// </summary>
        public static bool MinorMu(string koz)
        {
            return koz == "Karo" || koz == "Sinek";
        }

        /// <summary>
        /// Belirtilen koz NT mi?
        /// </summary>
        public static bool NTMi(string koz)
        {
            return koz == "NT";
        }

        // ═══════════════════════════════════════════════════════════════════
        // SABİTLER
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Tüm renkler (sıralı).
        /// </summary>
        public static readonly string[] TumRenkler =
            { "Maça", "Kupa", "Karo", "Sinek" };

        /// <summary>
        /// Tüm kozlar (NT dahil).
        /// </summary>
        public static readonly string[] TumKozlar =
            { "Maça", "Kupa", "Karo", "Sinek", "NT" };

        /// <summary>
        /// Majör renkler.
        /// </summary>
        public static readonly string[] MajorRenkler =
            { "Maça", "Kupa" };

        /// <summary>
        /// Minör renkler.
        /// </summary>
        public static readonly string[] MinorRenkler =
            { "Karo", "Sinek" };
    }
}