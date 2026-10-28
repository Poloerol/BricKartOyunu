using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    // ═══════════════════════════════════════════════════════════════════════
    // YENİ KONVANSİYONLAR — ortak altyapı
    // Her konvansiyon "Karar(Dz)" verir: null = bu dizide konuşmam (motor sıradakine geçer),
    // "Pas" dahil her metin gerçek karardır (IBilincliPas).
    // Tüm yeni konvansiyonlar ortaklık anlaşmasında varsayılan KAPALIDIR.
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>İhale dizisinin "ben / sol rakip / ortak / sağ rakip" gözüyle özeti (Pas'lar hariç).</summary>
    internal sealed class Dz
    {
        public IhaleDurumu D;
        public List<Card> El;
        public int Hp;
        /// <summary>Pas olmayan çağrılar, sırayla: "B:1♠" (ben), "L:" (sol rakip), "O:" (ortak), "R:" (sağ rakip).</summary>
        public List<string> Hepsi = new List<string>();

        private static readonly Player[] Cw = { Player.Guney, Player.Bati, Player.Kuzey, Player.Dogu };   // saat yönü

        public static Dz Al(IhaleDurumu d)
        {
            var z = new Dz { D = d, El = d.AktifOyuncuEli };
            z.Hp = ElDegerlendirici.HCP(z.El);
            int ben = Array.IndexOf(Cw, d.AktifOyuncu);
            foreach (var h in d.Gecmis)
            {
                if (h.PasMi) continue;
                int rel = (Array.IndexOf(Cw, h.Oyuncu) - ben + 4) % 4;
                z.Hepsi.Add("BLOR"[rel] + ":" + h.Teklif);
            }
            return z;
        }

        /// <summary>Pas olmayan çağrı dizisi tam olarak bu mu?</summary>
        public bool Es(params string[] s) { return Hepsi.SequenceEqual(s); }
        /// <summary>Dizi bununla mı başlıyor?</summary>
        public bool Basi(params string[] s) { return Hepsi.Count >= s.Length && Hepsi.Take(s.Length).SequenceEqual(s); }
        public int Uz(string renk) { return El.Count(c => c.Suit == renk); }
        public int Maca { get { return Uz("Maça"); } }
        public int Kupa { get { return Uz("Kupa"); } }
        public int Karo { get { return Uz("Karo"); } }
        public int Sinek { get { return Uz("Sinek"); } }
    }

    /// <summary>Yeni konvansiyonlar için küçük yardımcılar.</summary>
    internal static class YK
    {
        public static readonly string[] Renk = { "Maça", "Kupa", "Karo", "Sinek" };
        public static readonly string[] Sem = { "♠", "♥", "♦", "♣" };

        public static string S(string renk) { int i = Array.IndexOf(Renk, renk); return i < 0 ? "" : Sem[i]; }
        public static string T(int sev, string renk) { return sev + (renk == "NT" ? "NT" : S(renk)); }

        /// <summary>"1♠" → "Maça", "2NT" → "NT"; çağrı değilse null.</summary>
        public static string KozOf(string teklif)
        {
            if (string.IsNullOrEmpty(teklif) || teklif.Length < 2 || !char.IsDigit(teklif[0])) return null;
            if (teklif.EndsWith("NT")) return "NT";
            int i = Array.IndexOf(Sem, teklif.Substring(teklif.Length - 1));
            return i < 0 ? null : Renk[i];
        }
        public static int Sev(string teklif) { return string.IsNullOrEmpty(teklif) || !char.IsDigit(teklif[0]) ? 0 : teklif[0] - '0'; }
        /// <summary>"O:1♠" gibi etiketli çağrıdan çağrının kendisi.</summary>
        public static string Cagri(string etiketli) { return etiketli.Substring(2); }

        public static bool Major(string renk) { return renk == "Maça" || renk == "Kupa"; }

        /// <summary>En uzun renk (eşitlikte Maça, Kupa, Karo, Sinek sırası).</summary>
        public static string EnUzun(List<Card> el)
        {
            string en = Renk[0]; int n = -1;
            foreach (var r in Renk) { int u = el.Count(c => c.Suit == r); if (u > n) { n = u; en = r; } }
            return en;
        }
        public static int Uz(List<Card> el, string renk) { return el.Count(c => c.Suit == renk); }

        /// <summary>Verilen renkte A, K, Q'dan kaçı var.</summary>
        public static int ÜstHonor(List<Card> el, string renk) { return el.Count(c => c.Suit == renk && c.Value >= 12); }
        public static bool AKQ(List<Card> el, string renk) { return ÜstHonor(el, renk) == 3; }
        /// <summary>Renkteki A/K sayısı.</summary>
        public static int AK(List<Card> el, string renk) { return el.Count(c => c.Suit == renk && c.Value >= 13); }
        public static int Hcp(List<Card> el, string renk)
        {
            int p = 0;
            foreach (var c in el) if (c.Suit == renk && c.Value >= 11) p += c.Value - 10;
            return p;
        }
    }

    /// <summary>Yeni konvansiyonların ortak tabanı.</summary>
    internal abstract class YeniKonvTaban : IKonvansiyon, IBilincliPas
    {
        public abstract string Ad { get; }
        public abstract string Aciklama { get; }
        public bool AktifMi { get; set; } = true;
        public virtual int Oncelik { get { return 6; } }

        /// <summary>null = bu durumda uygun değil.</summary>
        protected abstract string Karar(Dz z);

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;
            try { return Karar(Dz.Al(durum)) != null; } catch (Exception) { return false; }
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return Karar(Dz.Al(durum));
        }
    }

    /// <summary>Yeni konvansiyon adlarının listesi (koruma katmanının güç tahmini denetiminden muaf tutulurlar).</summary>
    public static class YeniKonvansiyonlar
    {
        public static readonly string[] Adlar =
        {
            "Benjamin Two-Bids", "Extended Stayman", "Hello over 1NT", "Truscott 2NT", "Preemptive Reraises",
            "Long & Short Game Tries", "New Minor Forcing", "Weak Jump-Shift Resp.", "Ogust", "Gambling 3NT",
            "Specific King Asks", "Voluntary Five Major Bid", "Mathe Asking Bids", "Control Asking Bids", "Ghestem",
            "Invisible Cue-Bids", "Limit-Plus Cue-Bids", "Weak Jump Overcalls", "Sandwich 1NT", "Maximal Overcall Dbl.",
            "1m-2NT Invitational", "German 2C Relays", "Walsh", "Mini-Roman 2D", "Robinson",
            "Inverted Bergen Raises", "Exit Transfer 1NT Runouts", "Artif. 2C/2D Resp. to 1M-X", "Wolff Signoff"
        };
        public static bool Mu(string ad) { return ad != null && Array.IndexOf(Adlar, ad) >= 0; }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 1NT ÇEVRESİ
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Extended Stayman: 1NT'ye karşı zayıf (0-7 HP) 4-4 majörlü el 2♣ ile başlar; 2♦ cevabına 2♥ (geçer/düzelt) der.</summary>
    internal sealed class ExtendedStayman : YeniKonvTaban
    {
        public override string Ad { get { return "Extended Stayman"; } }
        public override string Aciklama { get { return "1NT–2♣ ile zayıf 4-4 majör; 2♦ sonrası 2♥ bırakma teklifi"; } }
        protected override string Karar(Dz z)
        {
            bool dortDort = z.Maca == 4 && z.Kupa == 4 && z.Hp <= 7;
            if (z.Es("O:1NT")) return dortDort ? "2♣" : null;
            if (!dortDort && !z.Es("B:1NT", "O:2♣", "B:2♦", "O:2♥")) return null;
            if (z.Es("O:1NT", "B:2♣", "O:2♦")) return "2♥";
            if (z.Es("O:1NT", "B:2♣", "O:2♥") || z.Es("O:1NT", "B:2♣", "O:2♠")) return "Pas";
            if (z.Es("B:1NT", "O:2♣", "B:2♦", "O:2♥")) return "Pas";   // açıcı: ortağın zayıf bırakması
            return null;
        }
    }

    /// <summary>Hello over 1NT: rakibin 1NT'sine karşı müdahale (2♣ iki majör, 2♦ tek renkli majör, 2♥/2♠ majör+minör, 2NT iki minör).</summary>
    internal sealed class HelloOver1NT : YeniKonvTaban
    {
        public override string Ad { get { return "Hello over 1NT"; } }
        public override string Aciklama { get { return "Rakip 1NT'sine: 2♣ iki majör, 2♦ uzun majör, 2♥/2♠ majör+minör, 2NT iki minör"; } }
        protected override string Karar(Dz z)
        {
            int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
            if (z.Es("R:1NT") || z.Es("L:1NT"))
            {
                if (z.Hp < 8 || z.Hp > 15) return null;
                if (s >= 4 && h >= 4 && (s >= 5 || h >= 5)) return "2♣";
                if (s >= 6 || h >= 6) return "2♦";
                if (s >= 5 && (d >= 4 || c >= 4)) return "2♠";
                if (h >= 5 && (d >= 4 || c >= 4)) return "2♥";
                if (d >= 5 && c >= 5) return "2NT";
                return null;
            }
            // Ortak müdahale etti, sıra bende (ilerletici)
            if (z.Es("L:1NT", "O:2♣"))
            {
                string m = h >= s ? "Kupa" : "Maça";
                int u = z.Uz(m);
                if (z.Hp >= 10 && u >= 4) return YK.T(3, m);
                return YK.T(2, m);
            }
            if (z.Es("L:1NT", "O:2♦")) return "2♥";                       // geçer ya da düzelt
            if (z.Es("L:1NT", "O:2NT")) return d >= c ? "3♦" : "3♣";
            // Müdahaleci: 2♦ → 2♥ cevabından sonra
            if (z.Es("R:1NT", "B:2♦", "O:2♥") || z.Es("L:1NT", "B:2♦", "O:2♥")) return h >= 6 ? "Pas" : "2♠";
            return null;
        }
    }

    /// <summary>Sandwich 1NT: (1x)–Pas–(1y)–1NT = kullanılmamış iki rengi gösterir (5-4); ilerletici uzun olanı seçer.</summary>
    internal sealed class Sandwich1NT : YeniKonvTaban
    {
        public override string Ad { get { return "Sandwich 1NT"; } }
        public override string Aciklama { get { return "(1x)–Pas–(1y) sonrası 1NT: kalan iki renk (5-4+), 8-15 HP"; } }

        private static List<string> Bos(string x, string y)
        {
            return YK.Renk.Where(r => r != x && r != y).ToList();
        }

        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 2 && z.Hepsi[0].StartsWith("L:") && z.Hepsi[1].StartsWith("R:"))
            {
                string a = YK.Cagri(z.Hepsi[0]), b = YK.Cagri(z.Hepsi[1]);
                string x = YK.KozOf(a), y = YK.KozOf(b);
                if (YK.Sev(a) != 1 || YK.Sev(b) != 1 || x == null || y == null || x == "NT" || y == "NT" || x == y) return null;
                if (z.Hp < 8 || z.Hp > 15) return null;
                var bos = Bos(x, y);
                int u1 = z.Uz(bos[0]), u2 = z.Uz(bos[1]);
                if (Math.Max(u1, u2) >= 5 && Math.Min(u1, u2) >= 4) return "1NT";
                return null;
            }
            // İlerletici (ortak 1NT dedi)
            if (z.Hepsi.Count == 3 && z.Hepsi[0].StartsWith("R:") && z.Hepsi[1].StartsWith("L:") && z.Es(z.Hepsi[0], z.Hepsi[1], "O:1NT"))
            {
                string a = YK.Cagri(z.Hepsi[0]), b = YK.Cagri(z.Hepsi[1]);
                string x = YK.KozOf(a), y = YK.KozOf(b);
                if (YK.Sev(a) != 1 || YK.Sev(b) != 1 || x == null || y == null || x == "NT" || y == "NT" || x == y) return null;
                var bos = Bos(x, y);
                string sec = z.Uz(bos[0]) >= z.Uz(bos[1]) ? bos[0] : bos[1];
                if (z.Uz(sec) < 3) return null;
                return YK.T(2, sec);
            }
            return null;
        }
    }

    /// <summary>Robinson: ortağın 1NT'si kontra edilince 2♣ Stayman, 2♦/2♥ transfer, S.Kontra = ♣ transferi.</summary>
    internal sealed class Robinson : YeniKonvTaban
    {
        public override string Ad { get { return "Robinson"; } }
        public override string Aciklama { get { return "1NT–(Dbl) sonrası: 2♣ Stayman, 2♦/2♥ majör transferi, RDbl = ♣ transferi"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1NT", "R:Dbl"))
            {
                if (z.Kupa >= 5 && z.Kupa >= z.Maca) return "2♦";
                if (z.Maca >= 5) return "2♥";
                if ((z.Maca >= 4 || z.Kupa >= 4) && z.Hp >= 8) return "2♣";
                if (z.Sinek >= 5 && z.Hp < 8) return "RDbl";
                return null;
            }
            if (z.Es("B:1NT", "L:Dbl", "O:2♦")) return "2♥";
            if (z.Es("B:1NT", "L:Dbl", "O:2♥")) return "2♠";
            if (z.Es("B:1NT", "L:Dbl", "O:RDbl")) return "2♣";
            if (z.Es("B:1NT", "L:Dbl", "O:2♣")) return z.Kupa >= 4 ? "2♥" : z.Maca >= 4 ? "2♠" : "2♦";
            return null;
        }
    }

    /// <summary>Exit Transfers: 1NT kontra edilince zayıf (0-7 HP) 5+ renkli ele "bir alttaki renk" transferi; RDbl = oynamalık güç.</summary>
    internal sealed class ExitTransfer : YeniKonvTaban
    {
        public override string Ad { get { return "Exit Transfer 1NT Runouts"; } }
        public override string Aciklama { get { return "1NT–(Dbl): 2♣→♦, 2♦→♥, 2♥→♠, 2♠→♣ zayıf kaçış transferleri; RDbl 9+ HP"; } }
        protected override string Karar(Dz z)
        {
            if (z.Es("O:1NT", "R:Dbl"))
            {
                if (z.Hp >= 9) return "RDbl";
                if (z.Hp > 7) return null;
                int s = z.Maca, h = z.Kupa, d = z.Karo, c = z.Sinek;
                int en = Math.Max(Math.Max(s, h), Math.Max(d, c));
                if (en < 5) return null;
                if (h == en) return "2♦";
                if (s == en) return "2♥";
                if (d == en) return "2♣";
                return "2♠";   // ♣
            }
            if (z.Es("B:1NT", "L:Dbl", "O:2♣")) return "2♦";
            if (z.Es("B:1NT", "L:Dbl", "O:2♦")) return "2♥";
            if (z.Es("B:1NT", "L:Dbl", "O:2♥")) return "2♠";
            if (z.Es("B:1NT", "L:Dbl", "O:2♠")) return "3♣";
            return null;
        }
    }

    /// <summary>Gambling 3NT: sağlam (AKQ) 7+ kartlık minör, dışarıda en çok bir A/K; ortak durdurucu yoksa 4♣ (geç/düzelt) der.</summary>
    internal sealed class Gambling3NT : YeniKonvTaban
    {
        public override string Ad { get { return "Gambling 3NT"; } }
        public override string Aciklama { get { return "3NT açılışı: sağlam 7+ minör, dışarıda A/K yok (en çok bir)"; } }
        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 0)
            {
                foreach (var m in new[] { "Sinek", "Karo" })
                {
                    if (z.Uz(m) < 7 || !YK.AKQ(z.El, m)) continue;
                    int disAK = z.El.Count(c => c.Suit != m && c.Value >= 13);
                    if (disAK <= 1 && z.Hp <= 13) return "3NT";
                }
                return null;
            }
            if (z.Es("O:3NT"))
            {
                int dur = YK.Renk.Count(r => ElDegerlendirici.StopperVar(z.El, r));
                return dur >= 3 ? "Pas" : "4♣";
            }
            if (z.Es("B:3NT", "O:4♣")) return z.Sinek >= 7 ? "Pas" : "5♦";
            return null;
        }
    }

    /// <summary>Wolff Signoff: 1x–1y–1NT sonrası 2♣ = 2♦'e röle; zayıf, uzun minörlü el oradan oturur.</summary>
    internal sealed class WolffSignoff : YeniKonvTaban
    {
        public override string Ad { get { return "Wolff Signoff"; } }
        public override string Aciklama { get { return "1x–1y–1NT sonrası 2♣: 2♦ röle, zayıf uzun minörle bırakma"; } }

        private static bool Bir(string etiketli, string kim)
        {
            if (!etiketli.StartsWith(kim + ":")) return false;
            string t = YK.Cagri(etiketli);
            return YK.Sev(t) == 1 && YK.KozOf(t) != null && YK.KozOf(t) != "NT";
        }

        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 3 && Bir(z.Hepsi[0], "O") && Bir(z.Hepsi[1], "B") && z.Hepsi[2] == "O:1NT")
                return (z.Hp <= 7 && (z.Karo >= 6 || z.Sinek >= 6)) ? "2♣" : null;
            if (z.Hepsi.Count == 4 && Bir(z.Hepsi[0], "B") && Bir(z.Hepsi[1], "O") && z.Hepsi[2] == "B:1NT" && z.Hepsi[3] == "O:2♣")
                return "2♦";
            if (z.Hepsi.Count == 5 && Bir(z.Hepsi[0], "O") && Bir(z.Hepsi[1], "B") && z.Hepsi[2] == "O:1NT" &&
                z.Hepsi[3] == "B:2♣" && z.Hepsi[4] == "O:2♦")
                return z.Karo >= 6 ? "Pas" : "3♣";
            return null;
        }
    }
}
