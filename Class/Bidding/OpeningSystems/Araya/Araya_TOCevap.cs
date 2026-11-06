using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Araya
{
    /// <summary>
    /// T/O KONTRAYA TRANSFERLİ CEVAPLAR (SAS kitabı s.107-108): rakip 1x açtı, ortak kontra yaptı, sağdaki rakip Pas.
    ///   - Basit renk (cuebid'in altında kalan en ucuz teklif) = doğal 4+ kart, 0-8 HCP.
    ///   - Transferler: x'in cuebid'i → x'ten sonraki ilk renk, her renk bir sonrakine, son renk → x'in cuebid'i
    ///     (dengeli, oyun zorlayıcı "transfer cuebid"). 8+ HCP; majörde 4+, minörde 5+ kart.
    ///   - 1NT doğal (x'te stoper) 7-10; 2NT doğal (stoper) 11-12; 3 seviyesinde iki renkli oyun zorlayıcı eller.
    /// Kontra yapan transferi en ucuz seviyeden tamamlar; güçlü ellerde atlamalı kabul / yeni renk / cuebid seçenekleri vardır (KontraciSecim). Cevapçı tamamlamadan sonra 11-12 ile davet, 13+ ile oyun söyler.
    /// Balancing kontraya (1x-Pas-Pas-Dbl-Pas) cevaplar aynıdır, yalnızca 2 HCP eklenir (kitap s.109).
    /// Basitleştirmeler: davet/süper kabul ve atlamalı transfer yok (açanın ortağı konuşunca: Araya_TORubensohl).
    /// </summary>
    public class Araya_TOCevap : IAcilisGelisimi
    {
        public int Oncelik { get { return 25; } }
        public string Aciklama { get { return "T/O kontraya transferli cevaplar (SAS)"; } }

        private const string Sira = "♣♦♥♠";
        private static List<IhaleHamlesi> Gercek(IhaleDurumu d) { return d.Gecmis.Where(h => h.GercekTeklifMi).ToList(); }
        private static int Ri(char c) { return Sira.IndexOf(c); }
        private static char Sonraki(char c) { return Sira[(Ri(c) + 1) % 4]; }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        private static string Ad(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }
        private static bool Major(char c) { return c == '♥' || c == '♠'; }

        /// <summary>x'in 1 seviyesinden sonra z renginin en ucuz transfer-kaynağı çağrısı (2 seviyesi ya da 3).</summary>
        private static string Kaynak(char x, char z) { return (Ri(z) > Ri(x) || z == x ? 2 : 3).ToString() + z; }

        /// <summary>Transfer sırası: kaynak çağrı → hedef (renk ya da x'in cuebid'i için x).</summary>
        private static List<KeyValuePair<string, char>> Transferler(char x)
        {
            var r = new List<KeyValuePair<string, char>>();
            char kaynak = x;
            for (int i = 0; i < 4; i++)
            {
                char hedef = i < 3 ? Sonraki(kaynak) : x;
                r.Add(new KeyValuePair<string, char>(Kaynak(x, kaynak), hedef));
                kaynak = Sonraki(kaynak);
            }
            return r;
        }

        private static string Ucuz(int lv, char son, char z) { return (Ri(z) > Ri(son) ? lv : lv + 1).ToString() + z; }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = Gercek(d);
            if (g.Count < 1 || g.Count > 3) return null;
            if (!d.Rakipler.Contains(g[0].Oyuncu)) return null;
            string a = g[0].Teklif;
            if (a == null || a.Length != 2 || a[0] != '1' || !Sira.Contains(a[1])) return null;
            char x = a[1];
            int ix = d.Gecmis.IndexOf(g[0]);
            if (ix < 0 || ix + 2 >= d.Gecmis.Count) return null;
            // Normal T/O kontra: x - Dbl - Pas ...   Balancing kontra: x - Pas - Pas - Dbl - Pas ... (cevaplar +2 HCP ile)
            int dbl; int dusur;
            if (d.Gecmis[ix + 1].Teklif == "Dbl") { dbl = ix + 1; dusur = 0; }
            else if (ix + 3 < d.Gecmis.Count && d.Gecmis[ix + 1].Teklif == "Pas" && d.Gecmis[ix + 2].Teklif == "Pas" && d.Gecmis[ix + 3].Teklif == "Dbl") { dbl = ix + 3; dusur = 2; }
            else return null;
            if (dbl + 1 >= d.Gecmis.Count || d.Gecmis[dbl + 1].Teklif != "Pas") return null;
            Player ortak = IhaleDurumu.PartnerBul(d.AktifOyuncu);
            var e = new OcEl(el);

            // Cevapçı: ... Dbl(ortak) - Pas - (ben)
            if (g.Count == 1)
            {
                if (d.Gecmis[dbl].Oyuncu != ortak || d.Gecmis.Count != dbl + 2) return null;
                return Cevap(e, x, dusur);
            }

            // Kontra yapan: ... Dbl(ben) - Pas - transfer - Pas - (ben)
            if (g.Count == 2)
            {
                if (d.Gecmis[dbl].Oyuncu != d.AktifOyuncu || g[1].Oyuncu != ortak) return null;
                if (d.Gecmis.Count != dbl + 4 || !ReferenceEquals(d.Gecmis[dbl + 2], g[1]) || d.Gecmis[dbl + 3].Teklif != "Pas") return null;
                var tr = Transferler(x).Where(t => t.Key == g[1].Teklif).ToList();
                if (tr.Count == 0) return null;
                char hedef = tr[0].Value;
                char kaynakRenk = g[1].Teklif[1];
                return KontraciSecim(e, x, hedef, g[1].Teklif[0] - '0', kaynakRenk);
            }

            // Cevapçı ikinci hamle: ... Dbl - Pas - transfer(ben) - Pas - tamamlama - Pas - (ben)
            if (d.Gecmis[dbl].Oyuncu != ortak || g[1].Oyuncu != d.AktifOyuncu || g[2].Oyuncu != ortak) return null;
            if (d.Gecmis.Count != dbl + 6 || !ReferenceEquals(d.Gecmis[dbl + 2], g[1]) || d.Gecmis[dbl + 3].Teklif != "Pas"
                || !ReferenceEquals(d.Gecmis[dbl + 4], g[2]) || d.Gecmis[dbl + 5].Teklif != "Pas") return null;
            var tr2 = Transferler(x).Where(t => t.Key == g[1].Teklif).ToList();
            if (tr2.Count == 0) return null;
            return IkinciHamle(e, x, tr2[0].Value, g[2].Teklif, dusur);
        }

        /// <summary>
        /// T/O kontra atanın transfer cevabından sonraki seçimi (kitap s.107): transferin kabulü = minimum T/O kontra (en ucuz seviye) ·
        /// atlamalı kabul = davet (16+ HCP, hedefte 4+ kart) · yeni renk = doğal, kuvvetli T/O kontra (18+ HCP, 5+ kart) ·
        /// atlamalı yeni renk = 5+ kart, hedefte 4'lü tutuş, oyun zorlar · cuebid = maksimum dengeli (17+ HCP, hedefte en çok 2 kart), stoper sorar.
        /// "Atlamalı iyi tutuş SNG cuebid" satırı kitapta belirsiz olduğundan kodlanmadı. Transfer cuebid (hedef = x) her zaman en ucuz tamamlanır.
        /// </summary>
        private static string KontraciSecim(OcEl e, char x, char hedef, int lv, char kaynak)
        {
            string normal = Ucuz(lv, kaynak, hedef);
            if (hedef == x) return normal;
            int hp = e.Hcp;
            int ucHedef = Ri(hedef) > Ri(kaynak) ? lv : lv + 1;
            var yeni = Sira.Where(c => c != x && c != hedef && Uz(e, c) >= 5).OrderByDescending(c => Uz(e, c)).ThenByDescending(c => Ri(c)).ToList();
            if (hp >= 18 && yeni.Count > 0)
            {
                char y = yeni[0];
                int ucY = Ri(y) > Ri(kaynak) ? lv : lv + 1;
                if (Uz(e, hedef) >= 4 && ucY + 1 <= 4) return (ucY + 1).ToString() + y;     // atlamalı yeni renk
                return ucY.ToString() + y;                                                  // doğal, kuvvetli
            }
            if (hp >= 16 && Uz(e, hedef) >= 4 && ucHedef + 1 <= 3) return (ucHedef + 1).ToString() + hedef;   // atlamalı kabul: davet
            if (hp >= 17 && e.Dengeli && Uz(e, hedef) <= 2)
            {
                int ucX = Ri(x) > Ri(kaynak) ? lv : lv + 1;
                return ucX.ToString() + x;                                                  // cuebid
            }
            return normal;
        }

        private static readonly Dictionary<char, Dictionary<string, char[]>> FG = new Dictionary<char, Dictionary<string, char[]>>
        {
            { '♣', new Dictionary<string, char[]> { { "3♣", new[] { '♦', '♥' } }, { "3♦", new[] { '♦', '♠' } }, { "3♥", new[] { '♠', '♥' } }, { "3♠", new[] { '♥', '♠' } } } },
            { '♦', new Dictionary<string, char[]> { { "3♦", new[] { '♣', '♥' } }, { "3♥", new[] { '♠', '♥' } }, { "3♠", new[] { '♥', '♠' } } } },
            { '♥', new Dictionary<string, char[]> { { "3♥", new[] { '♣', '♠' } }, { "3♠", new[] { '♦', '♠' } } } },
            { '♠', new Dictionary<string, char[]> { { "3♠", new[] { '♣', '♥' } }, { "3NT", new[] { '♦', '♥' } } } },
        };

        private static string Cevap(OcEl e, char x, int dusur)
        {
            int hp = e.Hcp - dusur;
            var tr = Transferler(x);

            // 12+ HCP: iki renkli oyun zorlayıcı el (ilk renk 5, ikincisi 4 kart)
            if (hp >= 12)
                foreach (var kv in FG[x])
                    if (Uz(e, kv.Value[0]) >= 5 && Uz(e, kv.Value[1]) >= 4) return kv.Key;

            // 11-12 dengeli, x'te stoper: 2NT
            if (hp >= 11 && hp <= 12 && e.Dengeli && e.Stopper(Ad(x)) && !AdayRenk(e, x).Any()) return "2NT";
            // 13+ dengeli: transfer cuebid
            if (hp >= 13 && e.Dengeli && !AdayRenk(e, x).Any()) return tr.First(t => t.Value == x).Key;

            // 8+ HCP: uzun renge transfer
            if (hp >= 8)
            {
                var ad = AdayRenk(e, x).ToList();
                if (ad.Count > 0)
                {
                    char en = ad.OrderByDescending(c => Uz(e, c)).ThenByDescending(c => Ri(c)).First();
                    return tr.First(t => t.Value == en).Key;
                }
            }
            // 7-10 dengeli stoperli: 1NT
            if (hp >= 7 && hp <= 10 && e.Dengeli && e.Stopper(Ad(x))) return "1NT";

            // Zayıf: doğal 4+ kartlı renk (majör öncelikli, uzun), yoksa en uzun renk
            var rn = Sira.Where(c => c != x).OrderByDescending(c => Uz(e, c)).ThenByDescending(c => Major(c) ? 1 : 0).ThenByDescending(c => Ri(c)).ToList();
            char s = rn[0];
            return Ucuz(1, x, s);
        }

        /// <summary>Transfer ile gösterilebilecek renkler: majörde 4+, minörde 5+ (x hariç).</summary>
        private static IEnumerable<char> AdayRenk(OcEl e, char x)
        {
            return Sira.Where(c => c != x && Uz(e, c) >= (Major(c) ? 4 : 5));
        }

        private static string IkinciHamle(OcEl e, char x, char hedef, string tamamlama, int dusur)
        {
            int hp = e.Hcp - dusur;
            if (hedef == x)                                   // transfer cuebid (dengeli oyun zorlayıcı)
                return hp >= 13 ? "3NT" : null;
            if (hp >= 13) return Major(hedef) ? "4" + hedef : (e.Stopper(Ad(x)) ? "3NT" : "5" + hedef);
            if (hp >= 11 && Major(hedef)) return (tamamlama[0] - '0' + 1).ToString() + hedef;   // davet: bir seviye yukarı
            return "Pas";
        }
    }
}
