using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class
{
    /// <summary>Pratik listesindeki tek dağıtım: seed ve hangi konvansiyonun pratiği olduğu.</summary>
    public class PratikDagitim
    {
        public int Seed;
        public string Konvansiyon;
        /// <summary>Dağıtımın üretildiği profil (nadir konvansiyonlar için yönlendirilmiş dağıtım).</summary>
        public DagitimProfili Profil = DagitimProfili.Rastgele;
        /// <summary>0 = konvansiyonun kullanıldığı el; &gt;0 = o elin küçük değişikliğiyle üretilmiş yakın kaçış.</summary>
        public int Varyant;
        public bool YakinKacis { get { return Varyant > 0; } }
    }

    /// <summary>
    /// Konvansiyon pratiği için dağıtım üretimi. Genel yöntem: seed'den rastgele dağıtım üretilir,
    /// dört el de ihale motoruyla oynatılır; seçilen konvansiyon insan tarafında (Güney veya Kuzey)
    /// gerçekten teklif üretirse dağıtım "geçerli" sayılır. Dağıtıcı seed'den belirlenir.
    /// </summary>
    public static class KonvansiyonPratigi
    {
        private static readonly Player[] Sira = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };

        /// <summary>Seed'e göre iç dağıtıcı (Kuzey, Doğu, Güney, Batı döngüsü).</summary>
        public static Player Dagitici(int seed)
        {
            long m = ((long)seed % 4 + 4) % 4;
            return Sira[(int)m];
        }

        private static readonly object _kilit = new object();
        private static List<string> _tumAdlar;
        private static Dictionary<string, OrtaklikAnlasmasi> _anlasmaHaritasi;

        /// <summary>
        /// Projedeki TÜM konvansiyonlar: varsayılan anlaşmada kapalı olanlar dahil. Her bool anlaşma bayrağı tek tek çevrilerek
        /// (ve hepsi açık / hepsi kapalı denenerek) motorun aktif ettiği konvansiyonlar toplanır; her konvansiyon için
        /// onu etkinleştiren ilk anlaşma saklanır (pratik motoru bununla kurulur).
        /// </summary>
        private static void Hazirla()
        {
            lock (_kilit)
            {
                if (_tumAdlar != null) return;
                var adlar = new List<string>();
                var harita = new Dictionary<string, OrtaklikAnlasmasi>();
                Action<OrtaklikAnlasmasi> ekle = (an) =>
                {
                    List<string> l;
                    try { l = new IhaleMotoru(an).PratikKonvansiyonAdlari(); } catch { return; }
                    foreach (var ad in l)
                        if (!harita.ContainsKey(ad)) { harita[ad] = an; adlar.Add(ad); }
                };
                ekle(OrtaklikAnlasmasi.Varsayilan());
                var props = typeof(OrtaklikAnlasmasi).GetProperties()
                    .Where(pr => pr.PropertyType == typeof(bool) && pr.CanRead && pr.CanWrite).ToList();
                foreach (var pr in props)
                {
                    var an = OrtaklikAnlasmasi.Varsayilan();
                    pr.SetValue(an, !(bool)pr.GetValue(an, null), null);
                    ekle(an);
                }
                // Başka bir konvansiyonla çakışıp tek bayrak çevrilince açılmayanlar (Flannery, Minor Stayman, Rubensohl...):
                // varsayılan açık bayraklar kapatılarak da denenir. Yalnız henüz bulunamayan adlar eklenir.
                foreach (var pr in props)
                {
                    if ((bool)pr.GetValue(OrtaklikAnlasmasi.Varsayilan(), null)) continue;
                    var an = OrtaklikAnlasmasi.Varsayilan();
                    foreach (var o in props)
                        if ((bool)o.GetValue(an, null) && o.Name != "StrongNT") o.SetValue(an, false, null);
                    pr.SetValue(an, true, null);
                    ekle(an);
                }
                var hepsi = OrtaklikAnlasmasi.Varsayilan();
                foreach (var pr in props) pr.SetValue(hepsi, true, null);
                ekle(hepsi);
                _anlasmaHaritasi = harita;
                _tumAdlar = adlar;
            }
        }

        /// <summary>Pratik listesi kategorileri (gösterim sırası). Kategorideki sıra da bilinçlidir: temelden ileriye.</summary>
        public static readonly KeyValuePair<string, string[]>[] Kategoriler =
        {
            new KeyValuePair<string, string[]>("Açılışlar", new[] {
                "1♣ Açılış Sistemi", "1♦ Açılış Sistemi", "1♣ Cevap Sistemi", "5'li Majör", "Strong NT", "2♣ Güçlü", "Benjamin Two-Bids",
                "Zayıf İki", "Zayıf İki Cevap", "Zayıf İki Müdahale", "Multi 2♦", "Preempt", "Preempt Cevap", "Namyats",
                "Ogust", "Gambling 3NT", "Mini-Roman 2D", "Flannery" }),
            new KeyValuePair<string, string[]>("1NT Çevresi", new[] {
                "Stayman", "Jacoby Transfer", "Minor Transfer", "Puppet Stayman", "Smolen", "Extended Stayman", "Quantitative 4NT", "Texas Transfers", "Minor Stayman", "Lebensohl" }),
            new KeyValuePair<string, string[]>("Cevaplar ve Oyun Denemeleri", new[] {
                "Inverted Minors", "Forcing NT", "2/1 Game Force", "Jacoby 2NT", "Splinter", "Drury", "Bergen Raises", "Inverted Bergen Raises",
                "Trial Bid", "Long & Short Game Tries", "İki Yönlü Checkback", "New Minor Forcing", "Wolff Signoff", "Fit Cue-Bid",
                "XYZ (X-Y-Z)", "3. Renk Forsingi", "4. Renk Forsingi", "Reverse", "Weak Jump-Shift Resp.", "1m-2NT Invitational",
                "German 2C Relays", "Walsh", "Preemptive Reraises", "Truscott 2NT", "Kokish" }),
            new KeyValuePair<string, string[]>("Rekabet", new[] {
                "Takeout Double", "Takeout Double Cevap", "Negative Double", "Support Double", "Support Double Cevap",
                "Responsive Double", "Responsive Double Cevap", "Michaels", "Unusual 2NT", "Ghestem", "Cue Bid",
                "Lebensohl (Reverse)", "Lebensohl (Zayıf 2)", "Weak Jump Overcalls", "Maximal Overcall Dbl.", "Artif. 2C/2D Resp. to 1M-X",
                "Invisible Cue-Bids", "Limit-Plus Cue-Bids",
                "Cappelletti", "DONT", "Multi-Landy", "Hello over 1NT", "Robinson", "Sandwich 1NT", "Exit Transfer 1NT Runouts",
                "Rubensohl", "Leaping Michaels" }),
            new KeyValuePair<string, string[]>("Slam", new[] {
                "Blackwood", "RKCB", "Gerber", "Kickback", "Exclusion Blackwood", "Koz Q Sorusu", "Pick-a-Slam", "Grand Slam Force",
                "Serious 3NT", "Non-Serious 3NT", "DOPI-ROPI", "Specific King Asks", "Voluntary Five Major Bid",
                "Mathe Asking Bids", "Control Asking Bids", "Lightner Double", "Last Train" })
        };
        public const string DigerKategori = "Diğer";

        /// <summary>Konvansiyonun kategorisi; listede olmayan "Diğer".</summary>
        public static string Kategori(string ad)
        {
            foreach (var k in Kategoriler) if (Array.IndexOf(k.Value, ad) >= 0) return k.Key;
            return DigerKategori;
        }

        private static int KategoriSirasi(string ad)
        {
            for (int i = 0; i < Kategoriler.Length; i++)
            {
                int j = Array.IndexOf(Kategoriler[i].Value, ad);
                if (j >= 0) return i * 1000 + j;
            }
            return Kategoriler.Length * 1000;
        }

        public static List<string> Konvansiyonlar()
        {
            Hazirla();
            return _tumAdlar.OrderBy(a => KategoriSirasi(a)).ToList();   // kararlı sıralama: kategori dışılar motor sırasını korur
        }

        /// <summary>Konvansiyonu etkinleştiren ortaklık anlaşması (bulunamazsa varsayılan).</summary>
        public static OrtaklikAnlasmasi AnlasmaIcin(string ad)
        {
            Hazirla();
            OrtaklikAnlasmasi an;
            return ad != null && _anlasmaHaritasi.TryGetValue(ad, out an) ? an : OrtaklikAnlasmasi.Varsayilan();
        }

        [ThreadStatic] private static Dictionary<string, IhaleMotoru> _motorOnbellek;
        /// <summary>Pratik edilecek konvansiyon için motor: o konvansiyonun açık olduğu anlaşmayla kurulur.</summary>
        public static IhaleMotoru MotorIcin(string ad)
        {
            if (_motorOnbellek == null) _motorOnbellek = new Dictionary<string, IhaleMotoru>();
            string anahtar = ad ?? "";
            IhaleMotoru m;
            if (_motorOnbellek.TryGetValue(anahtar, out m)) return m;
            m = new IhaleMotoru(AnlasmaIcin(ad));
            _motorOnbellek[anahtar] = m;
            return m;
        }

        private static List<Card> KartlarinElleri(List<string> deste, int koltuk)
        {
            var el = new List<Card>();
            for (int i = 0; i < 13; i++)
            {
                string k = deste[koltuk * 13 + i];
                char s = k[k.Length - 5];
                string rank = k.Substring(0, k.Length - 5);
                int v;
                switch (rank)
                {
                    case "A": v = 14; break;
                    case "K": v = 13; break;
                    case "Q": v = 12; break;
                    case "J": v = 11; break;
                    default: v = int.Parse(rank); break;
                }
                string suit = s == 'S' ? "Maça" : s == 'H' ? "Kupa" : s == 'D' ? "Karo" : "Sinek";
                el.Add(new Card { Suit = suit, Value = v });
            }
            return el;
        }

        /// <summary>
        /// Seed'in dağıtımını motorla oynatır. Dönen sözlük: konvansiyon adı → insan tarafında
        /// (Güney/Kuzey) o konvansiyonla verilen teklif sayısı. İhale metni 'ihale' çıktısında.
        /// </summary>
        public static Dictionary<string, int> KullanilanKonvansiyonlar(IhaleMotoru motor, int seed, out List<string> ihale)
        {
            return KullanilanKonvansiyonlar(motor, seed, DagitimProfili.Rastgele, out ihale);
        }

        public static Dictionary<string, int> KullanilanKonvansiyonlar(IhaleMotoru motor, int seed, DagitimProfili profil, out List<string> ihale)
        {
            var cagrilar = Oyna(motor, DagitimUretici.Uret(profil, null, seed), seed);
            var sayac = new Dictionary<string, int>();
            ihale = new List<string>();
            foreach (var c in cagrilar)
            {
                ihale.Add(c.Teklif);
                if (c.Teklif != "Pas" && (c.Oyuncu == Player.Guney || c.Oyuncu == Player.Kuzey) && !string.IsNullOrEmpty(c.Konvansiyon))
                {
                    int n; sayac.TryGetValue(c.Konvansiyon, out n);
                    sayac[c.Konvansiyon] = n + 1;
                }
            }
            return sayac;
        }

        /// <summary>
        /// Dağıtımın ihalesini taraf başına ortaklık anlaşmasıyla motordan geçirir; motorun verdiği teklifler
        /// dağıtıcıdan başlayarak 'hedef' ihalenin tüm tekliflerine (kısmi ihale olabilir) eşitse true döner.
        /// Kart Dağıtıcı "İhale Belirle" koşulu için; ilk uyuşmazlıkta durur.
        /// </summary>
        public static bool IhaleEslesir(IhaleMotoru motorNS, IhaleMotoru motorEW, OrtaklikAnlasmasi anlasmaNS, OrtaklikAnlasmasi anlasmaEW,
                                        List<string> deste, int seed, List<string> hedef)
        {
            if (hedef == null || hedef.Count == 0) return true;
            var eller = new Dictionary<Player, List<Card>>();
            for (int k = 0; k < 4; k++) eller[Sira[k]] = KartlarinElleri(deste, k);
            var gecmis = new List<IhaleHamlesi>();
            int bas = Array.IndexOf(Sira, Dagitici(seed));
            for (int i = 0; i < hedef.Count; i++)
            {
                Player p = Sira[(bas + i) % 4];
                bool ns = p == Player.Kuzey || p == Player.Guney;
                var d = new IhaleDurumu
                {
                    AktifOyuncu = p,
                    AktifOyuncuEli = eller[p],
                    Anlasma = ns ? anlasmaNS : anlasmaEW,
                    Gecmis = new List<IhaleHamlesi>(gecmis)
                };
                string t = (ns ? motorNS : motorEW).TeklifVer(d);
                if (t != hedef[i]) return false;
                gecmis.Add(new IhaleHamlesi { Oyuncu = p, Teklif = t, Sira = i + 1, GecerliMi = true });
            }
            return true;
        }

        private class Cagri { public Player Oyuncu; public string Teklif; public string Konvansiyon; }

        /// <summary>Dört eli de motorla oynatır; her çağrı ve onu üreten konvansiyon döner.</summary>
        private static List<Cagri> Oyna(IhaleMotoru motor, List<string> deste, int seed)
        {
            var eller = new Dictionary<Player, List<Card>>();
            for (int k = 0; k < 4; k++) eller[Sira[k]] = KartlarinElleri(deste, k);

            var anlasma = motor.Anlasma ?? OrtaklikAnlasmasi.Varsayilan();
            var gecmis = new List<IhaleHamlesi>();
            var sonuc = new List<Cagri>();

            int bas = Array.IndexOf(Sira, Dagitici(seed));
            // Saat yönü: Kuzey → Doğu → Güney → Batı
            int pas = 0; bool teklifVar = false;
            for (int i = 0; i < 60; i++)
            {
                Player p = Sira[(bas + i) % 4];
                var d = new IhaleDurumu
                {
                    AktifOyuncu = p,
                    AktifOyuncuEli = eller[p],
                    Anlasma = anlasma,
                    Gecmis = new List<IhaleHamlesi>(gecmis)
                };
                string t = motor.TeklifVer(d);
                gecmis.Add(new IhaleHamlesi { Oyuncu = p, Teklif = t, Sira = i + 1, GecerliMi = true });
                sonuc.Add(new Cagri { Oyuncu = p, Teklif = t, Konvansiyon = motor.SonKonvansiyonAdi });

                if (t == "Pas") pas++; else { pas = 0; teklifVar = true; }
                if (!teklifVar && pas == 4) break;
                if (teklifVar && pas == 3) break;
            }
            return sonuc;
        }

        // ═════════════════════════════════════════════════════════════
        // YAKIN KAÇIŞLAR
        // ═════════════════════════════════════════════════════════════
        /// <summary>Pratik dağıtımının 52 kartı (yakın kaçışsa değişiklik uygulanmış hâliyle).</summary>
        public static List<string> Deste(PratikDagitim pd)
        {
            return Deste(pd.Seed, pd.Profil, pd.Varyant);
        }

        public static List<string> Deste(int seed, DagitimProfili profil, int varyant)
        {
            var deste = DagitimUretici.Uret(profil, null, seed);
            if (varyant > 0) Degistir(deste, seed, varyant);
            return deste;
        }

        /// <summary>Güney veya Kuzey elinden 1-2 kartı rakip elleriyle takas eder (seed + varyanttan deterministik).</summary>
        private static void Degistir(List<string> deste, int seed, int varyant)
        {
            var r = new Random(unchecked(seed * 7919 + varyant * 104729));
            int adet = r.Next(2) + 1;
            for (int n = 0; n < adet; n++)
            {
                int bizKoltuk = r.Next(2) == 0 ? 0 : 2;   // Kuzey veya Güney
                int rakipKoltuk = r.Next(2) == 0 ? 1 : 3; // Doğu veya Batı
                int i = bizKoltuk * 13 + r.Next(13);
                int j = rakipKoltuk * 13 + r.Next(13);
                string t = deste[i]; deste[i] = deste[j]; deste[j] = t;
            }
        }

        /// <summary>
        /// Varyant gerçek bir yakın kaçış mı? Orijinal el konvansiyonu kullanıyor; değiştirilmiş elde
        /// ihale konvansiyon çağrısına kadar aynı gidiyor, o noktada farklı bir teklif veriliyor ve
        /// Güney/Kuzey konvansiyonu hiç kullanmıyor.
        /// </summary>
        public static bool YakinKacis(IhaleMotoru motor, int seed, DagitimProfili profil, string konvansiyon, int varyant)
        {
            var a = Oyna(motor, DagitimUretici.Uret(profil, null, seed), seed);
            int k = a.FindIndex(c => c.Teklif != "Pas" && (c.Oyuncu == Player.Guney || c.Oyuncu == Player.Kuzey) && c.Konvansiyon == konvansiyon);
            if (k < 0) return false;
            var b = Oyna(motor, Deste(seed, profil, varyant), seed);
            if (b.Count <= k) return false;
            foreach (var c in b)
                if (c.Teklif != "Pas" && (c.Oyuncu == Player.Guney || c.Oyuncu == Player.Kuzey) && c.Konvansiyon == konvansiyon)
                    return false;
            for (int i = 0; i < k; i++)
                if (a[i].Teklif != b[i].Teklif) return false;
            return a[k].Teklif != b[k].Teklif;
        }

        /// <summary>Geçerli bir elin ilk yakın kaçış varyantını bulur; yoksa 0.</summary>
        public static int YakinKacisBul(IhaleMotoru motor, int seed, DagitimProfili profil, string konvansiyon)
        {
            for (int v = 1; v <= 60; v++)
                if (YakinKacis(motor, seed, profil, konvansiyon, v)) return v;
            return 0;
        }

        /// <summary>Seed bu konvansiyon için geçerli mi?</summary>
        public static bool Gecerli(IhaleMotoru motor, int seed, string konvansiyon)
        {
            return Gecerli(motor, seed, konvansiyon, DagitimProfili.Rastgele);
        }

        public static bool Gecerli(IhaleMotoru motor, int seed, string konvansiyon, DagitimProfili profil)
        {
            List<string> ihale;
            var kullanilan = KullanilanKonvansiyonlar(motor, seed, profil, out ihale);
            return kullanilan.ContainsKey(konvansiyon);
        }
    }
}
