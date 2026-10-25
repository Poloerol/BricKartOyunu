using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    public class GrandSlamForce : IKonvansiyon
    {
        public string Ad => "Grand Slam Force";
        public string Aciklama => "Grand Slam Force ile koz onörü sorusu";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 26;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            if (durum.PartnerTeklifleri.Contains("1NT")) return false;

            // Partner 5NT dedi → cevap veriyoruz
            if (durum.SonGercekTeklif == "5NT" && durum.SonGercekTeklifSahibi == durum.Partner)
            {
                // 4NT / Kickback sonrası 5NT = standart PAPAZ SORUSU → KozQSorusu.cs cevaplar.
                bool oncekiSoruVar = durum.PartnerTeklifleri.Any(t =>
                    t == "4NT" || t == "4♠" || t == "4♥" || t == "4♦");
                if (oncekiSoruVar) return false;

                // Aksi halde DOĞRUDAN Grand Slam Force: koz anlaşılmış olmalı
                string kozGsf = durum.AnlasilanKoz();
                return !string.IsNullOrEmpty(kozGsf) && kozGsf != "NT";
            }

            int hp = ElDegerlendirici.HCP(durum.AktifOyuncuEli);
            if (hp < 16) return false;

            int seviye = 0;
            if (!string.IsNullOrEmpty(durum.SonGercekTeklif))
                seviye = KozYardimcisi.TekliftenSeviyeCikar(durum.SonGercekTeklif);
            if (seviye < 4) return false;

            if (durum.KendiTeklifleri.Contains("5NT")) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;

            if (durum.SonGercekTeklif == "5NT" && durum.SonGercekTeklifSahibi == durum.Partner)
            {
                // Grand Slam Force: kozun en üst 3 onöründen (A, K, Q) en az ikisi → 7 koz, aksi halde 6 koz
                string koz = durum.AnlasilanKoz();
                int onor = 0;
                foreach (var c in el.Where(c => c.Suit == koz))
                    if (c.Value >= 12) onor++;
                return (onor >= 2 ? "7" : "6") + KozYardimcisi.KozSembolu(koz);
            }

            return "5NT";
        }
    }
}