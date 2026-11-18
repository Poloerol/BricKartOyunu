using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Negatif kontradan sonra rakip müdahale rengini yükseltince açıcının devamı:
    /// 1x – (1M) – Kontra – (2M). Negatif kontra rakibin söylemediği diğer majörde 4+ kart gösterir.
    /// Açıcının o majörde 4+ kartı varsa fit bulunmuştur:
    ///  • 18+ HP: doğrudan 4 majör; daha azında 3 majör (rekabet). Eşikler 30.000 bordda ölçüldü: 3M için 15 HP −44, 12 HP −207 IMP.
    /// (Yedek mantık bu durumda 19 HP elle bile uzun minörü 3 seviyede tekrarlıyordu.)
    /// </summary>
    internal sealed class NegatifKontraRekabet : YeniKonvTaban
    {
        public override string Ad { get { return "Negatif Kontra Rekabet"; } }
        public override string Aciklama { get { return "1x–(1M)–Kontra–(2M): açıcı kontranın gösterdiği majörde 4+ kartla 3M / 4M"; } }

        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count != 4) return null;
            string acis = z.Hepsi[0], mud = z.Hepsi[1];
            if (!acis.StartsWith("B:1") || acis.EndsWith("NT")) return null;
            if (z.Hepsi[2] != "O:Dbl") return null;
            string ma = "♠", ku = "♥";
            string major;
            if (mud == "L:1♠" && z.Hepsi[3] == "R:2♠") { major = ku; }
            else if (mud == "L:1♥" && z.Hepsi[3] == "R:2♥") { major = ma; }
            else return null;
            if (acis.EndsWith(major)) return null;   // kendi açtığım majör: negatif kontra zaten o rengi göstermez
            int uz = major == ku ? z.Kupa : z.Maca;
            if (uz < 4) return null;
            return z.Hp >= 18 ? "4" + major : "3" + major;
        }
    }
}
