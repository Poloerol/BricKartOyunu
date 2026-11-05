using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using System.Collections.Generic;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    public class OneClub_Opening : IAcilisGelisimi
    {
        public int Oncelik => 4;
        // Açılışın anlamı (UI'da gösterilecek)
        public string Aciklama { get; private set; } = "1♣: 12-14 HCP dengeli, 2+♣, 5'li majör yok, 4+ karo yok";

        public bool UygunMu(List<Card> el, IhaleDurumu durum)
        {
            if (durum == null || el == null) return false;
            if (el.Count != 13) return false;
            if (!durum.IlkTeklifMi()) return false;

            int hp = ElDegerlendirici.HCP(el);
            if (hp > 21 || !ElDegerlendirici.AcilisGucuVarMi(el, durum.Anlasma != null ? durum.Anlasma.AcilisAgresifligi : 0)) return false;   // 12+ HCP veya 11 HCP + 20 kuralı
            if (ElDegerlendirici.BesliMajorVar(el)) return false;
            bool dengeliEl = ElDegerlendirici.DengeliEl(el);
            if (NTA.Acilis1NT(el, durum.Anlasma)) return false;   // 1NT açılışı (anlaşmadaki aralık)
            if (NTA.Acilis2NT(el, durum.Anlasma)) return false;   // 2NT açılışı (anlaşmadaki aralık)

            return TeklifVer(el, durum) == "1♣";
        }

        private static bool ZayifNT(IhaleDurumu d) { return d != null && d.Anlasma != null && !d.Anlasma.StrongNT; }

        /// <summary>
        /// Zayıf NT versiyonu (SAS kitabı s.75): 1♣ = dengeli ya da yarı dengeli (14)15-17 (5♣/5♦ 3-3-2 ve 4-2-2,
        /// kötü renkli 6♣/6♦ 3-2-2); karo uzunluğuna bakılmaksızın 1♣ açılır.
        /// </summary>
        public static bool ZayifNTSinekDengeli(List<Card> el)
        {
            if (ElDegerlendirici.DengeliEl(el)) return true;
            int[] u = { ElDegerlendirici.RenkUzunlugu(el, "Maça"), ElDegerlendirici.RenkUzunlugu(el, "Kupa"), ElDegerlendirici.RenkUzunlugu(el, "Karo"), ElDegerlendirici.RenkUzunlugu(el, "Sinek") };
            var s = new List<int>(u); s.Sort(); s.Reverse();
            bool minorUzun = u[2] == s[0] || u[3] == s[0];
            if (!minorUzun) return false;
            if (s[0] == 5 && ((s[1] == 3 && s[2] == 3 && s[3] == 2) || (s[1] == 4 && s[2] == 2 && s[3] == 2))) return true;
            if (s[0] == 6 && s[1] == 3 && s[2] == 2 && s[3] == 2)
            {
                string ad = u[2] == 6 ? "Karo" : "Sinek";
                int ust = 0;
                foreach (var k in el) if (k.Suit == ad && k.Value >= 12) ust++;     // A, K, Q sayısı (kötü renk: en çok 1)
                return ust <= 1;
            }
            return false;
        }

        /// <summary>1♦ mı 1♣ mı: en iyi minör / kısa sinek; zayıf NT versiyonunda 15-17 dengeli eller 1♣.</summary>
        public static bool KaroAcar(List<Card> el, IhaleDurumu durum)
        {
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int hp = ElDegerlendirici.HCP(el);
            if (ZayifNT(durum) && hp >= 14 && hp <= 17 && ZayifNTSinekDengeli(el)) return false;
            bool kisaSinek = durum != null && durum.Anlasma != null && durum.Anlasma.MinorShortClub == 1;
            return kisaSinek ? (karo >= 4 && karo >= sinek) : (karo > sinek || (karo == sinek && karo >= 4));   // en iyi minör / kısa sinek
        }

        public string TeklifVer(List<Card> el, IhaleDurumu durum)
        {
            if (el == null) return "Pas";

            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int hp = ElDegerlendirici.HCP(el);

            bool karoAc = KaroAcar(el, durum);
            if (karoAc)
            {
                Aciklama = "1♦: 4+♦, dengesiz veya dengeli";
                return "1♦";
            }

            // 1♣ açılışı — anlamı belirle
            if (ZayifNT(durum) && hp >= 14 && hp <= 17 && ZayifNTSinekDengeli(el))
                Aciklama = "1♣: Dengeli / yarı dengeli (14)15-17 HCP, 2+♣ (zayıf NT versiyonu)";
            else if (hp >= 12 && hp <= 14 && ElDegerlendirici.DengeliEl(el))
                Aciklama = "1♣: Dengeli 12-14 HCP, 2+♣, 5'li majör yok, 4+ karo yok";
            else if (sinek >= 5 && !ElDegerlendirici.DengeliEl(el))
                Aciklama = $"1♣: Dengesiz {hp} HCP, 5+♣ (iki renkli)";
            else if (sinek >= 6)
                Aciklama = $"1♣: {sinek}'li ♣, {hp} HCP (tek renkli)";
            else
                Aciklama = $"1♣: {hp} HCP, {sinek}+♣";

            return "1♣";
        }
    }
}
