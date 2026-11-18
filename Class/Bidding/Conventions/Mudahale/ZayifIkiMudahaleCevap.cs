using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Rakibin zayıf iki açılışına ortağımız doğal 2NT (15-18 HP, dengeli, rakip rengi tutuyor) ile müdahale etti,
    /// sağ rakip pas dedi: cevapçının doğal devamı (yapay Stayman/cue-bid yok).
    ///  • 18+ HP: 6NT; 8+ HP: 3NT (6+ kartlık majörde 4M); 5-7 HP: 6+ kartlık majörde 3M (oynamak için), aksi halde Pas.
    /// </summary>
    internal sealed class ZayifIkiMudahaleCevap : YeniKonvTaban
    {
        public override string Ad { get { return "Zayıf İki Müdahale Cevabı"; } }
        public override string Aciklama { get { return "Zayıf iki – (2NT müdahalesi) sonrası doğal cevap: 3NT / 4M / 6NT / Pas"; } }

        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count != 2) return null;
            string a = z.Hepsi[0];
            if (a != "L:2♦" && a != "L:2♥" && a != "L:2♠") return null;
            if (z.Hepsi[1] != "O:2NT") return null;
            string acilis = a.Substring(2);   // rakibin zayıf iki rengi (♦/♥/♠)

            int esik6 = 18;
            int esik3 = 8;
            string renk = null; int uz = 0;
            if (acilis != "2♠" && z.Maca > z.Kupa) { renk = "♠"; uz = z.Maca; }
            else if (acilis != "2♥" && z.Kupa >= z.Maca) { renk = "♥"; uz = z.Kupa; }
            else if (acilis != "2♠") { renk = "♠"; uz = z.Maca; }
            else if (acilis != "2♥") { renk = "♥"; uz = z.Kupa; }

            if (z.Hp >= esik6) return "6NT";
            if (z.Hp >= esik3)
                return (renk != null && uz >= 6) ? "4" + renk : "3NT";
            if (z.Hp >= 5 && renk != null && uz >= 6) return "3" + renk;
            return "Pas";
        }
    }
}
