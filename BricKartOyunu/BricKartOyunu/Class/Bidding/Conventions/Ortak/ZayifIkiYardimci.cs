using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Zayıf İki açılışı (2♦/2♥/2♠) ile ilgili ortak yardımcılar.
    /// </summary>
    internal static class ZayifIkiYardimci
    {
        public static string RenkAdi(char sembol)
        {
            switch (sembol)
            {
                case '♠': return "Maça";
                case '♥': return "Kupa";
                case '♦': return "Karo";
                case '♣': return "Sinek";
                default: return null;
            }
        }

        // ♣ < ♦ < ♥ < ♠
        public static int RenkSirasi(char sembol)
        {
            switch (sembol)
            {
                case '♣': return 0;
                case '♦': return 1;
                case '♥': return 2;
                case '♠': return 3;
                default: return -1;
            }
        }

        public static bool ZayifIkiTeklifiMi(string teklif)
        {
            return teklif == "2♦" || teklif == "2♥" || teklif == "2♠";
        }

        /// <summary>
        /// Auction'daki TEK gerçek teklif bir Zayıf İki açılışı ise onu döner.
        /// partnerMi / rakipMi: açılışı kimin yaptığı.
        /// pasSayisi: açılıştan sonra gelen (hepsi Pas olmak zorunda) teklif sayısı;
        ///            açılıştan sonra Pas dışı bir şey varsa -1.
        /// </summary>
        public static string ZayifIkiAcilisi(IhaleDurumu durum,
            out bool partnerMi, out bool rakipMi, out int pasSayisi)
        {
            partnerMi = false;
            rakipMi = false;
            pasSayisi = -1;

            var liste = durum.Gecmis.ToList();
            var gercek = liste.Where(h => h.GercekTeklifMi).ToList();
            if (gercek.Count != 1) return null;

            var acilis = gercek[0];
            if (!ZayifIkiTeklifiMi(acilis.Teklif)) return null;

            int idx = liste.IndexOf(acilis);
            var sonrasi = liste.Skip(idx + 1).ToList();
            if (sonrasi.Any(h => h.Teklif != "Pas")) return null;

            partnerMi = acilis.Oyuncu == durum.Partner;
            rakipMi = durum.Rakipler.Contains(acilis.Oyuncu);
            pasSayisi = sonrasi.Count;
            return acilis.Teklif;
        }
    }
}
