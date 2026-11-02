using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Lightner kontra: rakibin serbest teklif ettiği 6 seviye renk kontratına kontra = olağandışı atak isteği.
    /// Kontrayı atağı yapacak oyuncunun ORTAĞI verir (deklaranın sağındaki savunmacı). Kontracı yan renkte void'e
    /// ve en az 2 kozlu ve 7+ HP'ye sahiptir; ortak, dummy'nin ilk söylediği yan rengi (yoksa kontracının
    /// söylemediği rengi) açar. Yalnız ihale bu teklifle bitecekken (iki pas sonrası) verilir.
    /// </summary>
    internal sealed class LightnerDouble : YeniKonvTaban
    {
        public override string Ad { get { return "Lightner Double"; } }
        public override string Aciklama { get { return "6 seviye renk kontratına kontra: olağandışı atak (void) iste"; } }

        protected override string Karar(Dz z)
        {
            var d = z.D;
            var g = d.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (g.Count == 0) return null;
            var son = g[g.Count - 1];
            // kontra / s.kontra geldiyse artık değil
            var tum = d.Gecmis.Where(h => !h.PasMi).ToList();
            if (tum.Count > 0 && (tum[tum.Count - 1].KontrMi || tum[tum.Count - 1].SKontrMi)) return null;
            string koz = YK.KozOf(son.Teklif);
            if (koz == null || koz == "NT" || YK.Sev(son.Teklif) != 6) return null;
            // son teklifi verenin takımı rakibimiz mi?
            Player ben = d.AktifOyuncu;
            if (son.Oyuncu == ben || son.Oyuncu == IhaleDurumu.PartnerBul(ben)) return null;
            // deklaran: o kozu ilk söyleyen rakip
            Player rakipA = son.Oyuncu, rakipB = IhaleDurumu.PartnerBul(son.Oyuncu);
            Player deklaran = rakipA;
            foreach (var h in g)
            {
                if ((h.Oyuncu == rakipA || h.Oyuncu == rakipB) && YK.KozOf(h.Teklif) == koz) { deklaran = h.Oyuncu; break; }
            }
            Player atakci = IhaleDurumu.SonrakiOyuncu(deklaran);
            if (IhaleDurumu.PartnerBul(atakci) != ben) return null;   // atağı yapacak ben değilim, ortağım olmalı
            if (z.Hp < 7 || z.Uz(koz) < 2) return null;
            // bizim söylediğimiz renkler
            var bizimRenkler = new HashSet<string>();
            foreach (var h in g)
                if (h.Oyuncu == ben || h.Oyuncu == IhaleDurumu.PartnerBul(ben)) { string k = YK.KozOf(h.Teklif); if (k != null && k != "NT") bizimRenkler.Add(k); }
            foreach (var r in YK.Renk)
                if (r != koz && z.Uz(r) == 0 && !bizimRenkler.Contains(r)) return "Dbl";
            return null;
        }
    }
}
