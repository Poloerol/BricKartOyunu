using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.IkiRenk
{
    /// <summary>
    /// RAKİBİN 1NT AÇILIŞINA ARAYA GİRİŞLER (SAS kitabı s.106 alt tablo).
    /// 2. durum (doğrudan koltuk):
    ///   güçlü NT (15-17):  DBL ceza | 2♣ majörler 5/4 | 2♦ Multi (6+ majör) | 2♥ 5+♥ 4+minör | 2♠ 5+♠ 4+minör | 2NT 5♣5♦ | 3♣/3♦ doğal
    ///   zayıf NT (12-14):  DBL eşdeğer+ | 2♣ majörler 5/4 | 2♦ Multi 11+ | 2♥ 4+♥ 5+minör | 2♠ 4+♠ 5+minör | 2NT 5♣5♦ | 3♣/3♦ 6+ minör (hepsi 11-15 HCP)
    /// 4. durum (1NT-Pas-Pas): DBL = ♣ renk | 2♣ = 4+/4+ majörler | 2♦/2♥/2♠ doğal.
    /// Cevaplar (basit): 2♣'ye daha uzun majörü, Multi'ye 2♥ (araya giren majörü 2♠'ye düzeltir), 2NT'ye en uzun minörü.
    /// Basitleştirmeler: 'zon durumuna göre' ayrımı yok; ceza kontrasının cevapları, 2♥/2♠ girişlerine ve
    /// 3 seviyesi girişlere gelişimler standart motora bırakılır.
    /// </summary>
    public class IkiRenk_NTAraya : IAcilisGelisimi
    {
        public int Oncelik { get { return 24; } }
        public string Aciklama { get { return "Rakibin 1NT açılışına araya girişler (Multi, majörler, majör+minör, minörler)"; } }

        private static List<IhaleHamlesi> Gercek(IhaleDurumu d) { return d.Gecmis.Where(h => h.GercekTeklifMi).ToList(); }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = Gercek(d);
            if (g.Count < 1 || g.Count > 3) return null;
            if (g[0].Teklif != "1NT" || !d.Rakipler.Contains(g[0].Oyuncu)) return null;
            int ix = d.Gecmis.IndexOf(g[0]);
            if (ix < 0) return null;
            bool zayif = d.Anlasma != null && !d.Anlasma.StrongNT;
            var e = new OcEl(el);
            Player ortak = IhaleDurumu.PartnerBul(d.AktifOyuncu);

            if (g.Count == 1)
            {
                if (d.Gecmis.Count == ix + 1) return Dogrudan(e, zayif);                                   // 2. durum
                if (d.Gecmis.Count == ix + 3 && d.Gecmis[ix + 1].Teklif == "Pas" && d.Gecmis[ix + 2].Teklif == "Pas")
                    return Dorduncu(e);                                                                      // 4. durum
                return null;
            }

            string oc = g[1].Teklif;
            if (!ReferenceEquals(d.Gecmis.ElementAtOrDefault(ix + 1), g[1])) return null;

            // Cevapçı: 1NT - giriş - Pas - (ben)
            if (g.Count == 2)
            {
                if (g[1].Oyuncu != ortak || d.Gecmis.Count != ix + 3 || d.Gecmis[ix + 2].Teklif != "Pas") return null;
                return Cevap(new OcEl(el), oc);
            }

            // Cevapçı: 1NT - giriş (ortak) - RAKİP TEKLİFİ - (ben): yapay girişin ortağı pas geçmez, rakip müdahalesini hesaba katar
            if (g.Count == 3 && g[1].Oyuncu == ortak && d.Rakipler.Contains(g[2].Oyuncu)
                && d.Gecmis.Count == ix + 3 && ReferenceEquals(d.Gecmis[ix + 2], g[2]))
                return RakipliCevap(e, oc, g[2].Teklif);

            // Multi sonrası araya girenin düzeltmesi: 1NT - 2♦ - Pas - 2♥ - Pas - (ben)
            if (oc != "2♦" || g[1].Oyuncu != d.AktifOyuncu || g[2].Oyuncu != ortak || g[2].Teklif != "2♥") return null;
            if (d.Gecmis.Count != ix + 5 || d.Gecmis[ix + 2].Teklif != "Pas" || d.Gecmis[ix + 4].Teklif != "Pas") return null;
            return e.H >= 6 ? "Pas" : "2♠";
        }

        private static string Dogrudan(OcEl e, bool zayif)
        {
            int hp = e.Hcp;
            if ((zayif ? hp >= 15 : hp >= 16) && e.Dengeli) return "Dbl";                  // ceza / eşdeğer+
            if (hp < 8 || hp > 15) return null;
            if (zayif && hp < 11) return null;
            bool maj54 = (e.S >= 5 && e.H >= 4) || (e.H >= 5 && e.S >= 4);
            if (maj54) return "2♣";
            if ((e.S >= 6 && e.H <= 3) || (e.H >= 6 && e.S <= 3)) return "2♦";      // Multi: tek uzun majör
            int minUz = Math.Max(e.D, e.C);
            if (zayif)
            {
                if (e.S >= 4 && e.S < 6 && (e.D >= 5 || e.C >= 5) && e.S >= e.H) return "2♠";
                if (e.H >= 4 && e.H < 6 && (e.D >= 5 || e.C >= 5)) return "2♥";
            }
            else
            {
                if (e.S >= 5 && minUz >= 4 && e.S >= e.H) return "2♠";
                if (e.H >= 5 && minUz >= 4) return "2♥";
            }
            if (e.D >= 5 && e.C >= 5) return "2NT";
            if (e.C >= 6) return "3♣";
            if (e.D >= 6) return "3♦";
            return null;
        }

        private static string Dorduncu(OcEl e)
        {
            int hp = e.Hcp;
            if (hp < 8 || hp > 15) return null;
            if (e.S >= 4 && e.H >= 4) return "2♣";
            if (e.C >= 5 && e.C >= e.D && e.C >= e.H && e.C >= e.S) return "Dbl";
            var secim = new[] { Tuple.Create('♠', e.S), Tuple.Create('♥', e.H), Tuple.Create('♦', e.D) }.Where(t => t.Item2 >= 5).OrderByDescending(t => t.Item2).FirstOrDefault();
            return secim == null ? null : "2" + secim.Item1;
        }

        private static int Deger(string t)
        {
            if (t == null || t.Length < 2 || !char.IsDigit(t[0])) return -1;
            int idx = t.Substring(1) == "NT" ? 4 : "♣♦♥♠".IndexOf(t[1]);
            return idx < 0 ? -1 : (t[0] - '0') * 5 + idx;
        }

        /// <summary>Rakip müdahalesinden sonra en ucuz yasal seviyede renk teklifi (yoksa null).</summary>
        private static string UcuzRenk(char renk, string rakipTeklif, out int seviye)
        {
            seviye = 0;
            int rb = Deger(rakipTeklif);
            if (rb < 0) return null;
            int idx = "♣♦♥♠".IndexOf(renk);
            for (int l = 2; l <= 4; l++)
                if (l * 5 + idx > rb) { seviye = l; return l.ToString() + renk; }
            return null;
        }

        private static string RakipliCevap(OcEl e, string oc, string rb)
        {
            int lv;
            if (oc == "2♣")
            {
                char m = e.S > e.H ? '♠' : '♥';
                string t = UcuzRenk(m, rb, out lv);
                if (t == null) return "Pas";
                return (lv <= 2 || (lv == 3 && e.Hcp >= 9)) ? t : "Pas";
            }
            if (oc == "2NT")
            {
                char m = e.C >= e.D ? '♣' : '♦';
                string t = UcuzRenk(m, rb, out lv);
                if (t == null || lv > 3) return "Pas";
                return t;
            }
            return "Pas";                                                     // Multi: rakip girdi, pas = tutar
        }

        private static string Cevap(OcEl e, string oc)
        {
            if (oc == "2♣") return e.S > e.H ? "2♠" : "2♥";                  // daha uzun majör, eşitse ♥ (düzeltme mümkün)
            if (oc == "2♦") return "2♥";                                     // Multi: ♥ = pas ya da düzelt
            if (oc == "2NT") return e.C >= e.D ? "3♣" : "3♦";
            return null;
        }
    }
}
