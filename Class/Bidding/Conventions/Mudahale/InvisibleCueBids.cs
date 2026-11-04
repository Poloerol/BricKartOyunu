using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Invisible Cue-Bids: 1m–(1M) sonrası rakip majörünün cue-bid'i = minörde oyun zorlayan raise (4+ destek, 12+ HP).</summary>
    internal sealed class InvisibleCueBids : YeniKonvTaban
    {
        public override string Ad { get { return "Invisible Cue-Bids"; } }
        public override string Aciklama { get { return "1m–(1M)–2M: minör açılışına oyun zorlayan raise (4+ destek, 12+ HP)"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("R:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                string c = YK.Cagri(z.Hepsi[1]);
                string M = YK.KozOf(c);
                if ((m != "Sinek" && m != "Karo") || YK.Sev(c) != 1 || !YK.Major(M)) return null;
                return (z.Uz(m) >= 4 && z.Hp >= 12) ? YK.T(2, M) : null;
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[1].StartsWith("L:") && z.Hepsi[2].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                string c = YK.Cagri(z.Hepsi[1]); string M = YK.KozOf(c);
                if ((m != "Sinek" && m != "Karo") || YK.Sev(c) != 1 || !YK.Major(M)) return null;
                if (YK.Cagri(z.Hepsi[2]) != YK.T(2, M)) return null;
                return ElDegerlendirici.StopperVar(z.El, M) ? "3NT" : YK.T(3, m);
            }
            return null;
        }
    }
    /// <summary>
    /// Rakip rengi cue-bid (Batı / Doğu tarzı): ortak 1x açtı, sağ rakip doğal x renk müdahale etti; cevaplayan 12+ HP, fitsiz elle cue-bid yapar.
    /// Batı: ceplerinde durdurucu YOK, ortaktan durdurucu sorar. Doğu: durdurucu VAR, ortak (yarım) durdurucuyla 3NT der.
    /// Açıcı: durdurucuyla 3NT, yoksa kendi rengi.
    /// </summary>
    internal sealed class RakipCueBid : YeniKonvTaban
    {
        public override string Ad { get { return "Rakip Renk Cue-Bid (Batı/Doğu)"; } }
        public override string Aciklama { get { return "Ortak 1x, rakip y müdahale: y cue-bid = oyun gücü; Batı durdurucu sorar, Doğu durdurucu gösterir"; } }
        protected override string Karar(Dz z)
        {
            var an = z.D.Anlasma; if (an == null || (an.CueStyle != 1 && an.CueStyle != 2)) return null;
            bool bati = an.CueStyle == 1;
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("R:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O"); string c = YK.Cagri(z.Hepsi[1]); string x = YK.KozOf(c);
                if (m == null || x == null || x == "NT" || x == m || YK.Sev(c) > 2 || YK.Sev(z.Hepsi[0].Substring(2)) != 1) return null;
                if (z.Hp < 12 || z.Uz(m) >= 3 && YK.Major(m) || z.Uz(m) >= 4 || YK.EnUzun(z.El) != x && z.Uz(YK.EnUzun(z.El)) >= 5) return null;
                bool durdurucu = ElDegerlendirici.StopperVar(z.El, x);
                if (bati == durdurucu) return null;   // Batı: durdurucusuz; Doğu: durdurucuyla
                return YK.T(YK.Sev(c) + 1, x);
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[1].StartsWith("L:") && z.Hepsi[2].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B"); string c = YK.Cagri(z.Hepsi[1]); string x = YK.KozOf(c);
                if (m == null || x == null || x == "NT" || x == m || YK.Sev(c) > 2) return null;
                if (YK.Cagri(z.Hepsi[2]) != YK.T(YK.Sev(c) + 1, x)) return null;
                bool tamDurdurucu = ElDegerlendirici.StopperVar(z.El, x);
                bool yarim = z.Uz(x) >= 2 || tamDurdurucu;
                if (bati ? tamDurdurucu : yarim) return "3NT";
                string en = YK.EnUzun(z.El);
                return YK.T(Math.Max(3, YK.Sev(c) + 2), en == x ? m : en);
            }
            return null;
        }
    }

    /// <summary>Unusual over Unusual: 1M–(2NT = minörler): 3♣ = limit raise+, 3♦ = oyun zorlayan raise; açıcı 3M / 4M.</summary>
    internal sealed class UnusualOverUnusual : YeniKonvTaban
    {
        public override string Ad { get { return "Unusual over Unusual"; } }
        public override string Aciklama { get { return "1M–(2NT): 3♣ limit raise+ (3+ destek), 3♦ oyun zorlayan raise"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1♥", "R:2NT") || z.Es("O:1♠", "R:2NT"))
            {
                string m = z.Hepsi[0] == "O:1♥" ? "Kupa" : "Maça";
                if (z.Uz(m) < 3 || z.Hp < 10) return null;
                return z.Hp >= 13 ? "3♦" : "3♣";
            }
            if (z.Es("B:1♥", "L:2NT", "O:3♣") || z.Es("B:1♠", "L:2NT", "O:3♣"))
            { string sm = z.Hepsi[0] == "B:1♥" ? "♥" : "♠"; return z.Hp >= 14 ? "4" + sm : "3" + sm; }
            if (z.Es("B:1♥", "L:2NT", "O:3♦")) return "4♥";
            if (z.Es("B:1♠", "L:2NT", "O:3♦")) return "4♠";
            return null;
        }
    }

    /// <summary>Atak yönlendiren kontra: rakibin yapay Stayman/transfer çağrısına (1NT–2♣/2♦/2♥) kontra = o renkte güçlü uzunluk (lead isteği).</summary>
    internal sealed class LeadDirectingDouble : YeniKonvTaban
    {
        public override string Ad { get { return "Lead-Directing Double"; } }
        public override string Aciklama { get { return "Rakip 1NT–2♣/2♦/2♥ (Stayman/transfer): kontra = o rengi at (5+ kart, 2+ üst honör)"; } }
        protected override string Karar(Dz z)
        {
            if (!(z.Es("L:1NT", "R:2♣") || z.Es("L:1NT", "R:2♦") || z.Es("L:1NT", "R:2♥"))) return null;
            string x = YK.KozOf(YK.Cagri(z.Hepsi[1]));
            if (z.Uz(x) < 5 || YK.ÜstHonor(z.El, x) < 2) return null;
            return "Dbl";
        }
    }
}
