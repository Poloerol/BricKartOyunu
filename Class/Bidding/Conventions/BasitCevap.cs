using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;   // KozYardimcisi için

namespace BricKartOyunu.Class.Bidding.Conventions
{
    public class BasitCevap : IKonvansiyon
    {
        public string Ad => "Basit Cevap";
        public string Aciklama => "Basit açılış cevapları";
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

            string partnerSon = durum.PartnerTeklifleri.LastOrDefault();
            int partnerSeviye = KozYardimcisi.TekliftenSeviyeCikar(partnerSon ?? "");
            if (partnerSeviye != 1) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);

            string partnerTeklifi = durum.PartnerTeklifleri.LastOrDefault();
            string partnerKozu = KozYardimcisi.TekliftenKozCikar(partnerTeklifi);
            int partnerSeviye = KozYardimcisi.TekliftenSeviyeCikar(partnerTeklifi);

            // ─── 0-5 HP → Pas ───
            if (hp < 6)
                return "Pas";

            // ─── 6-9 HP → Basit destek veya yeni renk ───
            if (hp < 10)
            {
                // Inverted Minors AÇIKSA 1m - 2m "10+ forcing" demektir; 6-9 HP ile
                // 3-4 kartlık minör desteği 2m ile gösterilmez (yanlış anlam verir ve
                // elde 4'lü yan renk varken onu atlar). Bu durumda doğrudan yeni renge bak.
                bool inverted = durum.Anlasma != null && durum.Anlasma.InvertedMinors;
                bool partnerMinor = partnerKozu == "Sinek" || partnerKozu == "Karo";
                bool minorDestekAtla = inverted && partnerMinor;

                // 1) Partnerin rengine 3+ destek varsa → 2 seviyesinde basit destek
                //    (majörde her zaman; minörde yalnızca Inverted Minors kapalıysa)
                if (!minorDestekAtla && !string.IsNullOrEmpty(partnerKozu) && partnerSeviye == 1)
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 3)
                    {
                        string kozSembol = KozYardimcisi.KozSembolu(partnerKozu);
                        return "2" + kozSembol;
                    }
                }

                // 2) Yeni renk (1 seviye) — 4+ kart, majör öncelikli
                string yeniRenk = YeniRenkBul(el, partnerKozu, 4, "1");
                if (!string.IsNullOrEmpty(yeniRenk)) return yeniRenk;

                // 3) Yeni renk yoksa → 1NT (dengeli)
                if (ElDegerlendirici.DengeliEl(el)) return "1NT";

                return "Pas";
            }

            // ─── 10-15 HP → limit raise, yeni renk, sonra 2NT ───
            if (hp < 16)
            {
                if (partnerKozu != "Maça" && ElDegerlendirici.RenkUzunlugu(el, "Maça") >= 4)
                    return "1♠";
                if (partnerKozu != "Kupa" && ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 4)
                    return "1♥";

                if (!string.IsNullOrEmpty(partnerKozu) && partnerSeviye == 1)
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 5)
                    {
                        string kozSembol = KozYardimcisi.KozSembolu(partnerKozu);
                        return "3" + kozSembol;
                    }
                }

                if (!string.IsNullOrEmpty(partnerKozu) &&
                    (partnerKozu == "Maça" || partnerKozu == "Kupa"))
                {
                    int destek = ElDegerlendirici.RenkUzunlugu(el, partnerKozu);
                    if (destek >= 4 && partnerSeviye == 1)
                    {
                        string kozSembol = KozYardimcisi.KozSembolu(partnerKozu);
                        return "3" + kozSembol;
                    }
                }

                if (partnerKozu != "Kupa" && ElDegerlendirici.RenkUzunlugu(el, "Kupa") >= 5)
                    return "2♥";
                if (partnerKozu != "Karo" && ElDegerlendirici.RenkUzunlugu(el, "Karo") >= 5)
                    return "2♦";
                if (partnerKozu != "Sinek" && ElDegerlendirici.RenkUzunlugu(el, "Sinek") >= 5)
                    return "2♣";

                if (ElDegerlendirici.DengeliEl(el) && hp <= 12)
                    return "2NT";

                if (ElDegerlendirici.DengeliEl(el) && hp >= 13)
                    return "3NT";

                return "2NT";
            }

            // ─── 16+ HP → 2/1 Game Force ───
            string gfRenk = YeniRenkBul(el, partnerKozu, 5, "2");
            if (!string.IsNullOrEmpty(gfRenk)) return gfRenk;

            gfRenk = YeniRenkBul(el, partnerKozu, 4, "2");
            if (!string.IsNullOrEmpty(gfRenk)) return gfRenk;

            if (!string.IsNullOrEmpty(partnerKozu))
            {
                string kozSembol = KozYardimcisi.KozSembolu(partnerKozu);
                return "3" + kozSembol;
            }

            return "2NT";
        }

        private string YeniRenkBul(List<Card> el, string partnerKozu, int minUzunluk, string seviye)
        {
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");

            if (maca >= 5 && kupa >= 4 && maca > kupa && partnerKozu != "Maça" && minUzunluk <= maca)
                return seviye + "♠";
            if (kupa >= 5 && maca >= 4 && kupa > maca && partnerKozu != "Kupa" && minUzunluk <= kupa)
                return seviye + "♥";

            if (karo >= 5 && sinek >= 4 && karo > sinek && partnerKozu != "Karo" && minUzunluk <= karo)
                return seviye + "♦";
            if (sinek >= 5 && karo >= 4 && sinek > karo && partnerKozu != "Sinek" && minUzunluk <= sinek)
                return seviye + "♣";

            if (maca >= 5 && kupa >= 5 && maca == kupa)
            {
                if (partnerKozu != "Maça") return seviye + "♠";
                if (partnerKozu != "Kupa") return seviye + "♥";
            }
            if (karo >= 5 && sinek >= 5 && karo == sinek)
            {
                if (partnerKozu != "Karo") return seviye + "♦";
                if (partnerKozu != "Sinek") return seviye + "♣";
            }

            if (maca >= 4 && kupa >= 4 && maca == kupa)
            {
                if (partnerKozu != "Kupa") return seviye + "♥";
                if (partnerKozu != "Maça") return seviye + "♠";
            }
            if (karo >= 4 && sinek >= 4 && karo == sinek)
            {
                if (partnerKozu != "Sinek") return seviye + "♣";
                if (partnerKozu != "Karo") return seviye + "♦";
            }

            var renkler = new (string Renk, string Sembol, int Uzunluk)[]
            {
                ("Maça", "♠", maca),
                ("Kupa", "♥", kupa),
                ("Karo", "♦", karo),
                ("Sinek", "♣", sinek),
            };

            var sirali = renkler
                .Where(r => r.Renk != partnerKozu && r.Uzunluk >= minUzunluk)
                .OrderByDescending(r => r.Uzunluk)
                .ThenBy(r => Array.IndexOf(new[] { "Maça", "Kupa", "Karo", "Sinek" }, r.Renk))
                .ToList();

            if (sirali.Count > 0)
                return seviye + sirali[0].Sembol;

            return null;
        }
    }
}