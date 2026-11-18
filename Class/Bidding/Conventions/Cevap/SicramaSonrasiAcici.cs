using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Minör açılışa (1♣/1♦) ortağın sıçramalı yeni renk cevabı (1♣-2♦/2♥/2♠, 1♦-2♥/2♠; 18+ HP, BasitCevap.SicramaKontrolu)
    /// sonrası açıcının tekrarı. Bridge Baron akış şeması "Rebid after jump shift" (openrbd.fv):
    ///  (1) 3 kart destekle ortağın rengini bir seviye yükselt, (2) oynanmamış 4'lü renk(ler)de en ucuz teklif,
    ///  (3) 5'li minör (kendi rengi) ya da 6'lı majörde rengini tekrarla, (4) aksi halde en ucuz seviyede NT.
    /// Slam aramasını güçlü el olan cevapçı yürütür; açıcı 3NT/slam'a atlamaz.
    /// </summary>
    internal sealed class SicramaSonrasiAcici : YeniKonvTaban
    {
        public override string Ad { get { return "Sıçrama Sonrası Açıcı"; } }
        public override string Aciklama { get { return "1m–2x (sıçramalı yeni renk, 18+ HP) sonrası açıcı: destek / en ucuz 4'lü renk / kendi rengi / NT (BB akış şeması)"; } }
        public override int Oncelik { get { return 5; } }

        private static readonly string[] Semboller = { "♣", "♦", "♥", "♠" };
        private static readonly string[] Renkler = { "Sinek", "Karo", "Kupa", "Maça" };
        private static int Sira(string sembol) { return Array.IndexOf(Semboller, sembol); }

        protected override string Karar(Dz z)
        {
            if (z.D != null && z.D.Anlasma != null && z.D.Anlasma.WeakJumpShiftResp) return null;   // zayıf sıçrama anlaşması: sıçrama 18+ HP değil (WeakJumpShiftResp)
            if (z.Hepsi.Count != 2 || !z.Hepsi[0].StartsWith("B:1") || !z.Hepsi[1].StartsWith("O:2")) return null;
            string acis = z.Hepsi[0].Substring(3);
            string cevap = z.Hepsi[1].Substring(3);
            if (acis != "♣" && acis != "♦") return null;
            if (cevap == "NT" || Sira(cevap) <= Sira(acis)) return null;   // yalnızca sıçramalı yeni renk

            int y = Sira(cevap);
            // (1) 3+ kart destek: ortağın rengini bir seviye yükselt
            if (z.Uz(Renkler[y]) >= 3) return "3" + cevap;

            // (2) oynanmamış 4'lü renk(ler): en ucuz teklif (seviye, sıra)
            string enUcuz = null; int enUcuzSeviye = 9, enUcuzSira = 9;
            for (int s = 0; s < 4; s++)
            {
                if (Semboller[s] == acis || s == y || z.Uz(Renkler[s]) < 4) continue;
                int seviye = s > y ? 2 : 3;
                if (seviye < enUcuzSeviye || (seviye == enUcuzSeviye && s < enUcuzSira)) { enUcuz = Semboller[s]; enUcuzSeviye = seviye; enUcuzSira = s; }
            }
            if (enUcuz != null) return enUcuzSeviye + enUcuz;

            // (3) kendi rengini (5'li minör) tekrarla; 6'lı majör zaten oynanmamış 4'lü renk kuralında kalır
            if (z.Uz(Renkler[Sira(acis)]) >= 5) return "3" + acis;

            // (4) en ucuz seviyede NT
            return "2NT";
        }
    }
}
