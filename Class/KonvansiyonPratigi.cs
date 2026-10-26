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

        public static List<string> Konvansiyonlar()
        {
            return new IhaleMotoru(OrtaklikAnlasmasi.Varsayilan()).PratikKonvansiyonAdlari();
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

        private class Cagri { public Player Oyuncu; public string Teklif; public string Konvansiyon; }

        /// <summary>Dört eli de motorla oynatır; her çağrı ve onu üreten konvansiyon döner.</summary>
        private static List<Cagri> Oyna(IhaleMotoru motor, List<string> deste, int seed)
        {
            var eller = new Dictionary<Player, List<Card>>();
            for (int k = 0; k < 4; k++) eller[Sira[k]] = KartlarinElleri(deste, k);

            var anlasma = OrtaklikAnlasmasi.Varsayilan();
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
