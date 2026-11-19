using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Ortağın doğal 1NT müdahalesinden (rakip 1 seviye renk açtı, 15-18 HP dengeli, rakip rengi tutuyor) sonra
    /// "sistem açık": Jacoby transfer. Sağ rakip pas dedi.
    ///  • 2♦ = 5+♥ (rakip ♣ ya da ♠ açtıysa), 2♥ = 5+♠ (rakip ♣ açtıysa). Rakibin rengi transfer rengi olamaz.
    ///  • Müdahaleci transferi tamamlar (2♦→2♥, 2♥→2♠); cevapçı sonra 10+ HP ile 5 kartta 3NT, 6+ kartta 4M, aksi halde Pas.
    /// </summary>
    internal sealed class NTMudahaleTransfer : YeniKonvTaban
    {
        public override string Ad { get { return "NT Müdahale Transfer"; } }
        public override string Aciklama { get { return "Ortağın 1NT müdahalesine Jacoby transfer (2♦ = ♥, 2♥ = ♠)"; } }

        // Rakibin açılış rengi (♣/♦/♥/♠) ya da null
        private static string Acilis(string cagri)
        {
            if (cagri == null || cagri.Length != 4) return null;
            if (!(cagri.StartsWith("L:1") || cagri.StartsWith("R:1"))) return null;
            char s = cagri[3];
            return (s == '♣' || s == '♦' || s == '♥' || s == '♠') ? s.ToString() : null;
        }

        protected override string Karar(Dz z)
        {
            int minHp = 4;   // 0-3 HP ile 2 seviyede transfer yapılmaz (ölçüm: -21 IMP)

            // Cevapçı: L:1x – (O:1NT) – (sağ rakip pas)
            if (z.Hepsi.Count == 2 && z.Hepsi[1] == "O:1NT" && z.Hepsi[0].StartsWith("L:"))
            {
                string a = Acilis(z.Hepsi[0]);
                if (a == null || z.Hp < minHp) return null;
                bool kupaOlur = a == "♣" || a == "♠";
                bool macaOlur = a == "♣";
                if (kupaOlur && z.Kupa >= 5 && z.Kupa >= z.Maca) return "2♦";
                if (macaOlur && z.Maca >= 5 && z.Maca > z.Kupa) return "2♥";
                return null;
            }

            // Müdahaleci (ben): R:1x – B:1NT – (sol rakip pas) – O:2♦/2♥ = transfer
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("R:") && z.Hepsi[1] == "B:1NT")
            {
                string a = Acilis(z.Hepsi[0]);
                if (a == null) return null;
                if (z.Hepsi[2] == "O:2♦" && (a == "♣" || a == "♠")) return "2♥";
                if (z.Hepsi[2] == "O:2♥" && a == "♣") return "2♠";
                return null;
            }

            // Cevapçı, tamamlamadan sonra: L:1x – O:1NT – B:2♦ – O:2♥ (ya da B:2♥ – O:2♠)
            if (z.Hepsi.Count == 4 && z.Hepsi[0].StartsWith("L:") && z.Hepsi[1] == "O:1NT")
            {
                string a = Acilis(z.Hepsi[0]);
                if (a == null) return null;
                int uz; string renk;
                if (z.Hepsi[2] == "B:2♦" && z.Hepsi[3] == "O:2♥") { uz = z.Kupa; renk = "♥"; }
                else if (z.Hepsi[2] == "B:2♥" && z.Hepsi[3] == "O:2♠") { uz = z.Maca; renk = "♠"; }
                else return null;
                if (z.Hp < 10) return "Pas";
                return uz >= 6 ? "4" + renk : "3NT";
            }
            return null;
        }
    }
}
