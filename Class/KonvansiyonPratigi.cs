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
            var deste = DagitimUretici.Uret(DagitimProfili.Rastgele, null, seed);
            var eller = new Dictionary<Player, List<Card>>();
            for (int k = 0; k < 4; k++) eller[Sira[k]] = KartlarinElleri(deste, k);

            var anlasma = OrtaklikAnlasmasi.Varsayilan();
            var gecmis = new List<IhaleHamlesi>();
            var sayac = new Dictionary<string, int>();
            ihale = new List<string>();

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
                ihale.Add(t);

                if (t != "Pas" && (p == Player.Guney || p == Player.Kuzey) && !string.IsNullOrEmpty(motor.SonKonvansiyonAdi))
                {
                    int c; sayac.TryGetValue(motor.SonKonvansiyonAdi, out c);
                    sayac[motor.SonKonvansiyonAdi] = c + 1;
                }

                if (t == "Pas") pas++; else { pas = 0; teklifVar = true; }
                if (!teklifVar && pas == 4) break;
                if (teklifVar && pas == 3) break;
            }
            return sayac;
        }

        /// <summary>Seed bu konvansiyon için geçerli mi?</summary>
        public static bool Gecerli(IhaleMotoru motor, int seed, string konvansiyon)
        {
            List<string> ihale;
            var kullanilan = KullanilanKonvansiyonlar(motor, seed, out ihale);
            return kullanilan.ContainsKey(konvansiyon);
        }
    }
}
