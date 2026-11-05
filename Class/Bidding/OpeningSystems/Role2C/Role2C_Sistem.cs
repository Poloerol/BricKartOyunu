using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Role2C
{
    /// <summary>
    /// 2♣ RÖLE ve 2NT RÖLE (SAS kitabı s.91-92). 1x - 1y - 1z (açan 1 seviyesinde yeni renk rebid) sonrası:
    ///   Cevapçı 2♣ = röle (9+ HCP, davet ve üstü). Açanın cevapları: 2♦ = y'de 3'lü tutuş (min/max), 2♥ = min, 3'lü tutuş yok,
    ///   2♠ = max dengesiz, 3'lü tutuş yok, 2NT = max dengeli, 3'lü tutuş yok.
    ///   2♦ ve 2♥ cevaplarından sonra 2♥ / 2♠ ikinci röle: açan en ucuz basamakla min, bir üst basamakla max anlatır.
    ///   Cevapçı 2NT = röle transfer 3♣ (zayıf 6+♣ sign-off).
    /// Basitleştirmeler: açanın "min" eşiği 13 HCP, slam araştırması ve özel dağılımlar (4 step dışı) kodlanmadı.
    /// Kitapta ilgili sekansı kendi açılış bölümü (1♣/1♦/1♥/1♠ sistemleri) tanımlıyorsa o sistem önceliklidir.
    /// </summary>
    public class Role2C_Sistem : IAcilisGelisimi
    {
        public int Oncelik { get { return 20; } }
        public string Aciklama { get { return "2♣ röle (açanın 1 seviyesinde rebidine) / 2NT röle (3♣ transfer)"; } }

        private static List<IhaleHamlesi> Gercek(IhaleDurumu d) { return d.Gecmis.Where(h => h.GercekTeklifMi).ToList(); }
        private static char Renk(string t) { return t[t.Length - 1]; }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static int Sira(char c) { return c == '♣' ? 0 : c == '♦' ? 1 : c == '♥' ? 2 : 3; }
        private static bool Major(char c) { return c == '♥' || c == '♠'; }

        public bool UygunMu(List<Card> el, IhaleDurumu durum) { return TeklifBul(el, durum) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu durum) { return TeklifBul(el, durum) ?? "Pas"; }

        private string TeklifBul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = Gercek(d);
            if (g.Count < 3 || g.Count > 8) return null;
            if (g.Any(h => d.Rakipler.Contains(h.Oyuncu))) return null;
            var e = new OcEl(el);

            Player acici = g[0].Oyuncu, cevapci = g[1].Oyuncu;
            if (IhaleDurumu.PartnerBul(acici) != cevapci) return null;
            string a = g[0].Teklif, b = g[1].Teklif, c = g[2].Teklif;
            if (a.Length != 2 || b.Length != 2 || c.Length != 2) return null;
            if (a[0] != '1' || b[0] != '1' || c[0] != '1') return null;
            if (g[2].Oyuncu != acici) return null;
            char x = Renk(a), y = Renk(b), z = Renk(c);
            if (!"♣♦♥♠".Contains(x) || !"♣♦♥♠".Contains(y) || !"♣♦♥♠".Contains(z)) return null;
            if (Sira(y) <= Sira(x) || Sira(z) <= Sira(y)) return null;   // 1x-1y-1z: yeni renk, 1 seviyesinde

            bool ben = d.AktifOyuncu == cevapci;
            int n = g.Count;

            // ── 3 teklif: cevapçı ──
            if (n == 3)
            {
                if (!ben) return null;
                if (e.Hcp >= 9 && e.Hcp <= 15 && Uz(e, y) < 6 && Uz(e, z) < 4) return "2♣";
                if (e.Hcp <= 9 && e.C >= 6 && Uz(e, y) < 6) return "2NT";
                return null;
            }
            string r1 = g[3].Teklif;
            if (g[3].Oyuncu != cevapci) return null;

            // ── 4 teklif: açan ──
            if (n == 4 && !ben)
            {
                if (r1 == "2NT") return "3♣";
                if (r1 != "2♣") return null;
                bool min = e.Hcp <= 13;
                if (Uz(e, y) >= 3) return "2♦";
                if (min) return "2♥";
                if (!e.Dengeli) return "2♠";
                return "2NT";
            }
            if (r1 == "2NT")
            {
                // 2NT → 3♣ → Pas (zayıf ♣ sign-off)
                if (n == 5 && ben && g[4].Teklif == "3♣") return "Pas";
                return null;
            }
            if (r1 != "2♣") return null;
            if (n < 5) return null;
            string o1 = g[4].Teklif;
            if (g[4].Oyuncu != acici) return null;

            // ── 5 teklif: cevapçı (açanın röle cevabından sonra) ──
            if (n == 5 && ben)
            {
                if (o1 == "2♦") return "2♥";                       // 2. röle: min / max
                if (o1 == "2♥") return "2♠";                       // 2. röle: min / max
                return OyunKarari(e, y, z, o1);
            }
            // ── 6 teklif: açan ikinci röle cevabı ──
            if (n == 6 && !ben)
            {
                string r2 = g[5].Teklif;
                bool min = e.Hcp <= 13;
                if (o1 == "2♦" && r2 == "2♥") return min ? "2♠" : "2NT";
                if (o1 == "2♥" && r2 == "2♠") return min ? "2NT" : "3♣";
                return null;
            }
            // ── 7 teklif: cevapçı karar ──
            if (n == 7 && ben)
            {
                string o2 = g[6].Teklif;
                bool acMin = (o1 == "2♦" && o2 == "2♠") || (o1 == "2♥" && o2 == "2NT");
                return OyunKarari(e, y, z, null, acMin);
            }
            return null;
        }

        /// <summary>Cevapçının nihai kararı: açanın cevabına göre oyun ya da Pas.</summary>
        private static string OyunKarari(OcEl e, char y, char z, string aciciCevap, bool acMin = false)
        {
            // açan max: 2♠ (dengesiz), 2NT (dengeli); ikinci röleden sonra acMin bilgisi
            bool max = aciciCevap == "2♠" || aciciCevap == "2NT" || (aciciCevap == null && !acMin);
            bool oyun = max ? e.Hcp >= 9 : e.Hcp >= 13;
            if (!oyun) return "Pas";
            if (Major(y) && Uz(e, y) >= 5 && aciciCevap != "2♠" && aciciCevap != "2NT")
                return "4" + y;                                     // y'de 3'lü tutuş + 5'li renk
            if (Major(z) && Uz(e, z) >= 4) return "4" + z;
            return "3NT";
        }
    }
}
