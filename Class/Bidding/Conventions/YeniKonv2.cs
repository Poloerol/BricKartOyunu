using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    // ═══════════════════════════════════════════════════════════════════════
    // AÇILIŞLAR VE ORTAĞIN AÇILIŞINA CEVAPLAR
    // ═══════════════════════════════════════════════════════════════════════

    internal static class YK2
    {
        /// <summary>Renk sırası: Sinek 0 &lt; Karo 1 &lt; Kupa 2 &lt; Maça 3.</summary>
        public static int Sira(string renk) { return Array.IndexOf(new[] { "Sinek", "Karo", "Kupa", "Maça" }, renk); }

        /// <summary>"O:1♠" gibi etiketli çağrı, verilen kimde ve 1 seviyesinde bir renk açılışı mı? Renk adını döndürür (yoksa null).</summary>
        public static string BirRenk(string etiketli, string kim)
        {
            if (etiketli == null || !etiketli.StartsWith(kim + ":")) return null;
            string t = YK.Cagri(etiketli);
            if (YK.Sev(t) != 1) return null;
            string k = YK.KozOf(t);
            return k == "NT" ? null : k;
        }

        /// <summary>İki çağrı arasında 'sonraki' çağrı ('t' seviye+renk) önceki çağrıdan yüksek mi?</summary>
        public static bool Yuksek(string yeni, string onceki)
        {
            int ys = YK.Sev(yeni), os = YK.Sev(onceki);
            if (ys != os) return ys > os;
            string yk = YK.KozOf(yeni), ok = YK.KozOf(onceki);
            int yr = yk == "NT" ? 4 : Sira(yk), orr = ok == "NT" ? 4 : Sira(ok);
            return yr > orr;
        }
    }

    /// <summary>Benjamin Two-Bids: 2♣ = çok güçlü (23+ HP ya da 20+ HP ve 7+ renk), 2♦ = Acol tipi güçlü iki (17-22 HP, 6+ renk ya da 5-5); 2♥/2♠ zayıf.</summary>
    internal sealed class BenjaminTwoBids : YeniKonvTaban
    {
        public override string Ad { get { return "Benjamin Two-Bids"; } }
        public override string Aciklama { get { return "2♣ çok güçlü, 2♦ güçlü iki (Acol), 2♥/2♠ zayıf"; } }

        private static string Dogal(Dz z, bool ikiKaroAcildi)
        {
            // Açıcının/olumlu cevabın doğal rengi: en uzun renk, mümkün olan en düşük seviyede
            string en = YK.EnUzun(z.El);
            if (ikiKaroAcildi && en == "Karo")
            {
                // 2♦'nin ikinci rengi
                en = YK.Renk.Where(r => r != "Karo").OrderByDescending(r => z.Uz(r)).ThenBy(r => Array.IndexOf(YK.Renk, r)).First();
            }
            return en;
        }

        protected override string Karar(Dz z)
        {
            int en = YK.Renk.Max(r => z.Uz(r));
            // ── Açılış
            if (z.Hepsi.Count == 0)
            {
                if (z.Hp >= 23 || (z.Hp >= 20 && en >= 7)) return "2♣";
                bool ikiBes = YK.Renk.Count(r => z.Uz(r) >= 5) >= 2;
                if (z.Hp >= 17 && z.Hp <= 22 && (en >= 6 || ikiBes) && !ElDegerlendirici.DengeliEl(z.El)) return "2♦";
                return null;
            }
            // ── 2♣'e cevap
            if (z.Es("O:2♣"))
            {
                if (z.Hp < 8) return "2♦";                       // bekleme / negatif
                if (z.Kupa >= 5 && z.Kupa >= z.Maca) return "2♥";
                if (z.Maca >= 5) return "2♠";
                if (z.Sinek >= 5 && z.Sinek >= z.Karo) return "3♣";
                if (z.Karo >= 5) return "3♦";
                return "2NT";
            }
            // ── 2♦'e cevap
            if (z.Es("O:2♦"))
            {
                if (z.Hp < 8) return "2♥";                       // negatif
                if (z.Maca >= 5) return "2♠";
                if (z.Sinek >= 5) return "3♣";
                if (z.Kupa >= 5) return "3♥";
                return "2NT";
            }
            // ── Açıcının ikinci teklifi
            if (z.Es("B:2♣", "O:2♦"))
            {
                if (ElDegerlendirici.DengeliEl(z.El)) return z.Hp >= 25 ? "3NT" : "2NT";
                string r = Dogal(z, false);
                if (r == "Karo") return "3♦";
                if (r == "Sinek") return "3♣";
                return YK.T(2, r);
            }
            if (z.Es("B:2♦", "O:2♥"))
            {
                string r = Dogal(z, true);
                if (r == "Maça") return "2♠";
                if (r == "Kupa") return "3♥";
                if (r == "Sinek") return "3♣";
                return "3♦";
            }
            return null;
        }
    }

    /// <summary>Weak Jump-Shift Responses: ortağın 1x açılışına sıçramalı yeni renk (2♦/2♥/2♠) = 6+ renk, 0-6 HP; açıcı geçer.</summary>
    internal sealed class WeakJumpShiftResp : YeniKonvTaban
    {
        public override string Ad { get { return "Weak Jump-Shift Resp."; } }
        public override string Aciklama { get { return "1x–2y (sıçrama) = zayıf, 6+ y rengi"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 1)
            {
                string x = YK2.BirRenk(z.Hepsi[0], "O");
                if (x == null || z.Hp > 6) return null;
                foreach (var y in new[] { "Maça", "Kupa", "Karo" })
                    if (YK2.Sira(y) > YK2.Sira(x) && z.Uz(y) >= 6 && YK2.Sira(y) - YK2.Sira(x) >= 1)
                    {
                        // sıçrama: 1 seviyede o renk mümkündü (zaten 'yüksek'), 2 seviyeden söylenir
                        return YK.T(2, y);
                    }
                return null;
            }
            if (z.Hepsi.Count == 2)
            {
                string x = YK2.BirRenk(z.Hepsi[0], "B");
                if (x == null) return null;
                string t = YK.Cagri(z.Hepsi[1]);
                if (!z.Hepsi[1].StartsWith("O:") || YK.Sev(t) != 2) return null;
                string y = YK.KozOf(t);
                if (y == null || y == "NT" || YK2.Sira(y) <= YK2.Sira(x)) return null;
                return z.Hp >= 19 ? null : "Pas";                 // açıcı: zayıf sıçrama, geçer
            }
            return null;
        }
    }

    /// <summary>Ogust: zayıf iki açılışına 2NT = soru; cevap 3♣ min-zayıf renk, 3♦ min-iyi renk, 3♥ maks-zayıf, 3♠ maks-iyi, 3NT = AKQ'lu.</summary>
    internal sealed class Ogust : YeniKonvTaban
    {
        public override string Ad { get { return "Ogust"; } }
        public override string Aciklama { get { return "Zayıf iki + 2NT: 3♣ min/zayıf, 3♦ min/iyi, 3♥ maks/zayıf, 3♠ maks/iyi, 3NT sağlam"; } }

        private static string ZayifIkiRengi(string t)
        {
            if (t == "2♦") return "Karo"; if (t == "2♥") return "Kupa"; if (t == "2♠") return "Maça"; return null;
        }

        private static bool Benjamin2D(Dz z, string x) { return x == "Karo" && z.D.Anlasma != null && z.D.Anlasma.BenjaminTwoBids; }

        protected override string Karar(Dz z)
        {
            // Soru: ortak 2x açtı, ben güçlüyüm
            if (z.Hepsi.Count == 1 && z.Hepsi[0].StartsWith("O:"))
            {
                string x = ZayifIkiRengi(YK.Cagri(z.Hepsi[0]));
                if (x == null || Benjamin2D(z, x)) return null;
                return (z.Hp >= 15 || (z.Hp >= 14 && z.Uz(x) >= 3)) ? "2NT" : null;
            }
            // Cevap: ben 2x açtım, ortak 2NT
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[1] == "O:2NT")
            {
                string x = ZayifIkiRengi(YK.Cagri(z.Hepsi[0]));
                if (x == null || Benjamin2D(z, x)) return null;
                int ust = YK.ÜstHonor(z.El, x);
                bool maks = z.Hp >= 8 || (z.Hp >= 7 && ust >= 2);
                if (YK.AKQ(z.El, x)) return "3NT";
                bool iyi = ust >= 2;
                if (!maks) return iyi ? "3♦" : "3♣";
                return iyi ? "3♠" : "3♥";
            }
            // Cevaptan sonra karar (soran)
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1] == "B:2NT" && z.Hepsi[2].StartsWith("O:"))
            {
                string x = ZayifIkiRengi(YK.Cagri(z.Hepsi[0]));
                string c = YK.Cagri(z.Hepsi[2]);
                if (x == null || Benjamin2D(z, x)) return null;
                int acici = c == "3♣" ? 6 : c == "3♦" ? 7 : c == "3♥" ? 9 : c == "3♠" ? 10 : c == "3NT" ? 10 : 0;
                if (acici == 0) return null;
                int toplam = z.Hp + acici;
                bool major = YK.Major(x);
                if (c == "3NT") return major ? YK.T(4, x) : "Pas";
                if (major && toplam >= 25) return YK.T(4, x);
                if (!major && toplam >= 27) return "3NT";
                // Oyun yok: x rengini 3 seviyesinde bırak (cevaptan yüksekse), değilse geç
                string sign = YK.T(3, x);
                return YK2.Yuksek(sign, c) ? sign : "Pas";
            }
            return null;
        }
    }

    /// <summary>Inverted Bergen Raises: 1M–3♣ = 4'lü destek 10-12 HP (limit), 1M–3♦ = 4'lü destek 7-9 HP.</summary>
    internal sealed class InvertedBergenRaises : YeniKonvTaban
    {
        public override string Ad { get { return "Inverted Bergen Raises"; } }
        public override string Aciklama { get { return "1M–3♣: limit (10-12), 1M–3♦: kurucu (7-9), ikisi de 4'lü destek"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 1 && z.Hepsi[0].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                if (!YK.Major(m) || z.Uz(m) < 4) return null;
                if (z.Hp >= 10 && z.Hp <= 12) return "3♣";
                if (z.Hp >= 7 && z.Hp <= 9) return "3♦";
                return null;
            }
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("B:") && z.Hepsi[1].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                string c = YK.Cagri(z.Hepsi[1]);
                if (!YK.Major(m) || (c != "3♣" && c != "3♦")) return null;
                if (c == "3♣") return z.Hp >= 14 ? YK.T(4, m) : YK.T(3, m);
                return z.Hp >= 17 ? YK.T(4, m) : YK.T(3, m);
            }
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("O:") && z.Hepsi[1].StartsWith("B:") && z.Hepsi[2].StartsWith("O:"))
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                string b = YK.Cagri(z.Hepsi[1]);
                if (!YK.Major(m) || (b != "3♣" && b != "3♦")) return null;
                if (YK.Cagri(z.Hepsi[2]) == YK.T(3, m)) return "Pas";
            }
            return null;
        }
    }

    /// <summary>1m–2NT Invitational: minör açılışına 2NT = 11-12 HP dengeli, 4'lü majör yok; açıcı 15+ ile 3NT, aksi hâlde geçer.</summary>
    internal sealed class OneMinorTwoNTInv : YeniKonvTaban
    {
        public override string Ad { get { return "1m-2NT Invitational"; } }
        public override string Aciklama { get { return "1♣/1♦–2NT: 11-12 HP dengeli, majör yok (davet)"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 1)
            {
                string m = YK2.BirRenk(z.Hepsi[0], "O");
                if (m != "Sinek" && m != "Karo") return null;
                if (z.Hp < 11 || z.Hp > 12 || !ElDegerlendirici.DengeliEl(z.El) || z.Maca >= 4 || z.Kupa >= 4) return null;
                return "2NT";
            }
            if (z.Hepsi.Count == 2 && z.Hepsi[1] == "O:2NT")
            {
                string m = YK2.BirRenk(z.Hepsi[0], "B");
                if (m != "Sinek" && m != "Karo") return null;
                return z.Hp >= 15 ? "3NT" : "Pas";
            }
            return null;
        }
    }

    /// <summary>German 2♣ Relays: 2♣–2♦–2NT (22-24) sonrası Stayman/transfer sistemi açık: 3♣ Stayman, 3♦/3♥ majör transferi.</summary>
    internal sealed class German2CRelays : YeniKonvTaban
    {
        public override string Ad { get { return "German 2C Relays"; } }
        public override string Aciklama { get { return "2♣–2♦–2NT sonrası: 3♣ Stayman, 3♦→♥, 3♥→♠ transferleri"; } }
        protected override string Karar(Dz z)
        {
            // Cevaplayan
            if (z.Es("O:2♣", "B:2♦", "O:2NT"))
            {
                if (z.Kupa >= 5 && z.Kupa >= z.Maca) return "3♦";
                if (z.Maca >= 5) return "3♥";
                if (z.Kupa >= 4 || z.Maca >= 4) return "3♣";
                return "3NT";
            }
            // Açıcı
            if (z.Es("B:2♣", "O:2♦", "B:2NT", "O:3♣")) return z.Kupa >= 4 ? "3♥" : z.Maca >= 4 ? "3♠" : "3♦";
            if (z.Es("B:2♣", "O:2♦", "B:2NT", "O:3♦")) return "3♥";
            if (z.Es("B:2♣", "O:2♦", "B:2NT", "O:3♥")) return "3♠";
            // Cevaplayanın devamı
            if (z.Es("O:2♣", "B:2♦", "O:2NT", "B:3♣", "O:3♦")) return "3NT";
            if (z.Es("O:2♣", "B:2♦", "O:2NT", "B:3♣", "O:3♥")) return z.Kupa >= 4 ? "4♥" : "3NT";
            if (z.Es("O:2♣", "B:2♦", "O:2NT", "B:3♣", "O:3♠")) return z.Maca >= 4 ? "4♠" : "3NT";
            if (z.Es("O:2♣", "B:2♦", "O:2NT", "B:3♦", "O:3♥")) return "4♥";
            if (z.Es("O:2♣", "B:2♦", "O:2NT", "B:3♥", "O:3♠")) return "4♠";
            return null;
        }
    }

    /// <summary>Walsh: 1♣ açılışına 4♥+4♠ ile 1♥; tek 4'lü majör, 5'li majör yok, 4+♦ ve 6-10 HP ile majör atlanıp 1♦ denir.</summary>
    internal sealed class Walsh : YeniKonvTaban
    {
        public override string Ad { get { return "Walsh"; } }
        public override string Aciklama { get { return "1♣–1♦ majör atlayarak (4+♦, 6-10 HP); 4-4 majörde 1♥"; } }
        protected override string Karar(Dz z)
        {
            if (!z.Es("O:1♣") || z.Hp < 6) return null;
            if (z.Maca >= 5 || z.Kupa >= 5) return null;
            if (z.Maca == 4 && z.Kupa == 4) return "1♥";
            bool birDortlu = (z.Maca == 4) != (z.Kupa == 4);
            if (birDortlu && z.Karo >= 4 && z.Hp <= 10) return "1♦";
            return null;
        }
    }

    /// <summary>Mini-Roman 2♦: ♦ tekil/void, ♠♥♣ 4'lü (11-15 HP); cevaplayan en uzun renge geçer/düzeltir, açıcı geçer.</summary>
    internal sealed class MiniRoman2D : YeniKonvTaban
    {
        public override string Ad { get { return "Mini-Roman 2D"; } }
        public override string Aciklama { get { return "2♦ açılışı: ♦ tekil/void, ♠♥♣ 4+ (11-15 HP); cevap: geç/düzelt"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 0)
                return (z.Hp >= 11 && z.Hp <= 15 && z.Karo <= 1 && z.Maca >= 4 && z.Kupa >= 4 && z.Sinek >= 4) ? "2♦" : null;
            if (z.Es("O:2♦"))
            {
                int s = z.Maca, h = z.Kupa, c = z.Sinek;
                if (h >= s && h >= c) return "2♥";
                if (s >= c) return "2♠";
                return "3♣";
            }
            if (z.Es("B:2♦", "O:2♥") || z.Es("B:2♦", "O:2♠") || z.Es("B:2♦", "O:3♣")) return "Pas";
            return null;
        }
    }

    /// <summary>New Minor Forcing: 1x–1y–1NT sonrası yeni minör (2♣, açılış ♣ ise 2♦) = 11+ HP, y desteği / dörtlü majör sorusu.</summary>
    internal sealed class NewMinorForcing : YeniKonvTaban
    {
        public override string Ad { get { return "New Minor Forcing"; } }
        public override string Aciklama { get { return "1x–1y–1NT sonrası yeni minör: oyun daveti+, y'de 3'lü destek / diğer majörde 4'lü sorusu"; } }

        private static string Yeni(string acilis) { return acilis == "Sinek" ? "2♦" : "2♣"; }

        protected override string Karar(Dz z)
        {
            // Cevaplayan: ortak x açtı, ben y dedim, ortak 1NT
            if (z.Hepsi.Count == 3 && z.Hepsi[2] == "O:1NT")
            {
                string x = YK2.BirRenk(z.Hepsi[0], "O"), y = YK2.BirRenk(z.Hepsi[1], "B");
                if (x == null || y == null || !YK.Major(y) || x == y || z.Hp < 11) return null;
                return Yeni(x);
            }
            // Açıcı cevabı
            if (z.Hepsi.Count == 4 && z.Hepsi[2] == "B:1NT")
            {
                string x = YK2.BirRenk(z.Hepsi[0], "B"), y = YK2.BirRenk(z.Hepsi[1], "O");
                if (x == null || y == null || !YK.Major(y) || x == y) return null;
                if (YK.Cagri(z.Hepsi[3]) != Yeni(x)) return null;
                if (z.Uz(y) >= 3) return YK.T(2, y);
                string diger = y == "Kupa" ? "Maça" : "Kupa";
                if (z.Uz(diger) >= 4 && YK2.Yuksek(YK.T(2, diger), Yeni(x))) return YK.T(2, diger);
                return "2NT";
            }
            // Cevaplayanın devamı
            if (z.Hepsi.Count == 5 && z.Hepsi[2] == "O:1NT" && z.Hepsi[3].StartsWith("B:"))
            {
                string x = YK2.BirRenk(z.Hepsi[0], "O"), y = YK2.BirRenk(z.Hepsi[1], "B");
                if (x == null || y == null || !YK.Major(y) || YK.Cagri(z.Hepsi[3]) != Yeni(x)) return null;
                string c = YK.Cagri(z.Hepsi[4]);
                if (c == YK.T(2, y)) return z.Hp >= 13 ? YK.T(4, y) : YK.T(3, y);
                string diger = y == "Kupa" ? "Maça" : "Kupa";
                if (c == YK.T(2, diger)) return z.Uz(diger) >= 4 ? YK.T(4, diger) : "3NT";
                if (c == "2NT") return z.Hp >= 13 ? "3NT" : "Pas";
            }
            return null;
        }
    }

    /// <summary>Truscott 2NT: 1M–(Dbl)–2NT = 4+ destekli limit raise (10-12 HP); açıcı 15+ ile oyun, aksi hâlde 3M.</summary>
    internal sealed class Truscott2NT : YeniKonvTaban
    {
        public override string Ad { get { return "Truscott 2NT"; } }
        public override string Aciklama { get { return "1M–(Dbl)–2NT: 4+ destekli limit raise (10-12 HP)"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1♠", "R:Dbl") || z.Es("O:1♥", "R:Dbl"))
            {
                string m = z.Hepsi[0] == "O:1♠" ? "Maça" : "Kupa";
                return (z.Uz(m) >= 4 && z.Hp >= 10 && z.Hp <= 12) ? "2NT" : null;
            }
            if (z.Es("B:1♠", "L:Dbl", "O:2NT") || z.Es("B:1♥", "L:Dbl", "O:2NT"))
            {
                string m = z.Hepsi[0] == "B:1♠" ? "Maça" : "Kupa";
                return z.Hp >= 15 ? YK.T(4, m) : YK.T(3, m);
            }
            return null;
        }
    }

    /// <summary>Artif. 2C/2D Resp. to 1M-X: 1M–(Dbl) sonrası 2♣ = 3'lü destekle limit raise (10-12), 2♦ = 3+ destekle oyun zorlayan raise (13+).</summary>
    internal sealed class Artificial2C2D : YeniKonvTaban
    {
        public override string Ad { get { return "Artif. 2C/2D Resp. to 1M-X"; } }
        public override string Aciklama { get { return "1M–(Dbl)–2♣ 3'lü destekli limit, 2♦ oyun zorlayan raise"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1♠", "R:Dbl") || z.Es("O:1♥", "R:Dbl"))
            {
                string m = z.Hepsi[0] == "O:1♠" ? "Maça" : "Kupa";
                int u = z.Uz(m);
                if (u >= 3 && z.Hp >= 13) return "2♦";
                if (u == 3 && z.Hp >= 10 && z.Hp <= 12) return "2♣";
                return null;
            }
            if (z.Es("B:1♠", "L:Dbl", "O:2♣") || z.Es("B:1♥", "L:Dbl", "O:2♣"))
            {
                string m = z.Hepsi[0] == "B:1♠" ? "Maça" : "Kupa";
                return z.Hp >= 15 ? YK.T(4, m) : YK.T(3, m);
            }
            if (z.Es("B:1♠", "L:Dbl", "O:2♦") || z.Es("B:1♥", "L:Dbl", "O:2♦"))
            {
                string m = z.Hepsi[0] == "B:1♠" ? "Maça" : "Kupa";
                return YK.T(4, m);
            }
            return null;
        }
    }

    /// <summary>Preemptive Reraises: 1M–2M raise sonrası rakip 3 seviyesine kadar yarışırsa, 6+ kozlu ve ≤14 HP açıcı 4M ile önler.</summary>
    internal sealed class PreemptiveReraises : YeniKonvTaban
    {
        public override string Ad { get { return "Preemptive Reraises"; } }
        public override string Aciklama { get { return "1M–2M, rakip yarışınca 6+ kozla önleyici 4M"; } }
        protected override string Karar(Dz z)
        {
            int n = z.Hepsi.Count;
            if (n != 3 && n != 4) return null;
            if (!z.Hepsi[0].StartsWith("B:")) return null;
            string m = YK2.BirRenk(z.Hepsi[0], "B");
            if (!YK.Major(m)) return null;
            string raise = YK.T(2, m);
            string son = z.Hepsi[n - 1];
            if (!son.StartsWith("R:")) return null;
            string ortak = z.Hepsi[n - 2];
            if (ortak != "O:" + raise) return null;
            if (n == 4 && !z.Hepsi[1].StartsWith("L:")) return null;
            string c = YK.Cagri(son);
            if (YK.Sev(c) < 2 || YK.Sev(c) > 3) return null;
            return (z.Uz(m) >= 6 && z.Hp <= 14) ? YK.T(4, m) : null;
        }
    }

    /// <summary>Long &amp; Short Game Tries: 1M–2M sonrası açıcının 3. seviyede söylediği yan renk = kısa renk (tekil/void) denemesi; cevaplayan o renkteki boşa giden değerlere göre karar verir.</summary>
    internal sealed class LongShortGameTries : YeniKonvTaban
    {
        public override string Ad { get { return "Long & Short Game Tries"; } }
        public override string Aciklama { get { return "1M–2M–3x: x'te kısalık denemesi; boşa giden değer varsa 3M, yoksa 4M"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("B:1♠", "O:2♠") || z.Es("B:1♥", "O:2♥"))
            {
                string m = z.Hepsi[0] == "B:1♠" ? "Maça" : "Kupa";
                if (z.Hp < 15 || z.Hp > 17 || z.Uz(m) < 5) return null;
                // kısa renk: tekil ya da void (en düşük renk)
                foreach (var r in new[] { "Sinek", "Karo", "Kupa", "Maça" })
                    if (r != m && z.Uz(r) <= 1) return YK.T(3, r);
                return null;
            }
            if (z.Hepsi.Count == 3 && (z.Basi("O:1♠", "B:2♠") || z.Basi("O:1♥", "B:2♥")))
            {
                string m = z.Hepsi[0] == "O:1♠" ? "Maça" : "Kupa";
                string c = YK.Cagri(z.Hepsi[2]);
                string x = YK.KozOf(c);
                if (YK.Sev(c) != 3 || x == null || x == "NT" || x == m) return null;
                int boş = YK.Hcp(z.El, x);
                return (boş <= 1 && z.Hp >= 6) ? YK.T(4, m) : YK.T(3, m);
            }
            return null;
        }
    }
}
