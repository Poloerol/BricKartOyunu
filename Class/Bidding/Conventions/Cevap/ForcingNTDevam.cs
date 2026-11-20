using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Forcing 1NT'den sonra cevapçının İKİNCİ teklifi: 1M – 1NT – 2x (açıcı yeni renk söyledi, 2 seviye).
    ///  • Açıcının ikinci rengi x'te 3+ kart (uyum): Pas.
    ///  • Aksi halde açıcının ilk rengi M'de 2+ kart: 2M (tercih; en az riskli).
    ///  • Hiçbiri yoksa konvansiyon devreye girmez; genel mantık sürdürür.
    /// Cevapçı 1NT ile 6-10 HP söylemişti: 2 seviyede kalmak yeterli.
    /// </summary>
    internal sealed class ForcingNTDevam : YeniKonvTaban
    {
        public override string Ad { get { return "Forcing NT Devam"; } }
        public override string Aciklama { get { return "1M-1NT-2x sonrası cevapçı: uyumda Pas, değilse ilk renge 2M tercih"; } }
        public override int Oncelik { get { return 11; } }

        private static string RenkAdi(string s)
        {
            switch (s) { case "♣": return "Sinek"; case "♦": return "Karo"; case "♥": return "Kupa"; case "♠": return "Maça"; }
            return null;
        }

        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count != 3 || z.Hepsi[1] != "B:1NT") return null;
            if (z.Hepsi[0] != "O:1♠" && z.Hepsi[0] != "O:1♥") return null;
            if (!z.Hepsi[2].StartsWith("O:2") || z.Hepsi[2].Length != 4) return null;
            string m = z.Hepsi[0].Substring(3), x = z.Hepsi[2].Substring(3);
            string mAd = RenkAdi(m), xAd = RenkAdi(x);
            if (mAd == null || xAd == null || x == m) return null;
            if (m == "♥" && x == "♠") return null;   // 2♠'dan sonra 2♥ yasal değil (3♥ gerekir): genel mantığa bırak

            if (z.Uz(xAd) >= 3) return "Pas";          // ortağın ikinci rengi uyumlu
            if (z.Uz(mAd) >= 2) return "2" + m;        // ilk renge tercih
            return null;
        }
    }
}
