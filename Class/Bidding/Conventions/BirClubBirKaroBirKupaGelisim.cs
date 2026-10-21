using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    public class BirClubBirKaroBirKupaGelisim : IKonvansiyon
    {
        public string Ad => "1♣-1♦-1♥ Gelişimi";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 7;

        private static readonly Dictionary<string, string> Aciklamalar = new Dictionary<string, string>
{
    // Cevapçı açıklamaları
    { "3NT_Cevapci", "13+ HCP dengeli, ♠ stopper, majör yok, 6♦ değil" },
    { "1♠_Cevapci", "4+♦ 4♠ F1R (9-12 HCP, 3♦ yok)" },
    { "3♦_Cevapci", "6♦ 4♠ FG (12+ HCP)" },
    { "2♣_Cevapci", "Röle SI (12+ HCP, 4+♦)" },
    { "2NT_Cevapci", "Limit (11-12 dengeli)" },
    { "3♣_Cevapci", "5♦4♠3♣1♥ (9-11 HCP)" },
    { "2♦_Cevapci", "6♦4♠ (9-11 HCP)" },
    { "1NT_Cevapci", "6-9 HCP dengeli, majör yok, ♠ stopper" },
    { "Pas_Cevapci", "Yetersiz güç veya uygun dağılım yok" },

    // Açıcı açıklamaları
    { "3♥_Acici", "Açıcı: 4♦ + 4♥, 13+ HCP (SI)" },
    { "3♦_Acici", "Açıcı: 5♦, 10-11 HCP (MIN)" },
    { "2NT_Acici", "Açıcı: 4♦, 11-12 HCP (MIN)" },
    { "2♠_Acici", "Açıcı: 4♠, 11-14 HCP" },
    { "3♣_Acici", "Açıcı: 4♥ + 4♣, 10-11 HCP" },
    { "2♦_Acici", "Açıcı: 3♦, MIN" },
    { "2♥_Acici", "Açıcı: 3♦, MAX" }
};




        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // Aşama A: 1♣ (partner) - 1♦ (biz) - 1♥ (partner) - sıra bizde
            // AktifOyuncu = cevapçı (biz 1♦ dedik)
            if (durum.AktifOyuncu == Player.Guney)
            {
                if (durum.KendiTeklifleri.Count == 1
                    && durum.KendiTeklifleri[0] == "1♦"
                    && durum.PartnerTeklifleri.Count == 2
                    && durum.PartnerTeklifleri[0] == "1♣"
                    && durum.PartnerTeklifleri[1] == "1♥"
                    && durum.SonGercekTeklif == "1♥")
                    return true;
            }

            // Aşama B: 1♣ (biz) - 1♦ (partner) - 1♥ (biz) - 2♣ (partner) - sıra bizde
            // AktifOyuncu = açıcı (biz 1♣ ve 1♥ dedik)
            if (durum.AktifOyuncu == Player.Kuzey)
            {
                if (durum.KendiTeklifleri.Count == 2
                    && durum.KendiTeklifleri[0] == "1♣"
                    && durum.KendiTeklifleri[1] == "1♥"
                    && durum.PartnerTeklifleri.Count == 2
                    && durum.PartnerTeklifleri[0] == "1♦"
                    && durum.PartnerTeklifleri[1] == "2♣"
                    && durum.SonGercekTeklif == "2♣")
                    return true;
            }

            return false;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            string teklif = (durum.SonGercekTeklif == "2♣")
                ? AciciRoleCevap(durum)
                : CevapciDevam(durum);

            if (Aciklamalar.ContainsKey(teklif))
                return $"{teklif} → {Aciklamalar[teklif]}";

            return teklif;
        }


        private string CevapciDevam(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            int hcp = ElDegerlendirici.HCP(el);
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            bool dengeli = ElDegerlendirici.DengeliEl(el);
            bool sStopper = ElDegerlendirici.StopperVar(el, "Maça");

            // 1) 3NT: 13+ HCP dengeli, ♠ stopper, majör yok, 6♦ DEĞİL
            if (hcp >= 13 && dengeli && sStopper && s < 4 && d <= 4) return "3NT";

            // 2) 1♠: 4+♦ 4♠ F1R (9-12 HCP, 3♦ yok)
            if (d >= 4 && s >= 4 && hcp >= 9 && hcp <= 12 && d <= 4 && c >= 3) return "1♠";
            // 5♦ varsa 1♠ değil, 3♦ / 2♦

            // 3) 3♦: 6♦ 4♠ FG (12+ HCP)
            if (d >= 6 && s >= 4 && hcp >= 12) return "3♦";

            // 4) 2♣: Röle SI (12+ HCP, 4+♦)
            if (hcp >= 12 && d >= 4) return "2♣";

            // 5) 2NT: Limit (11-12 dengeli)
            if (hcp >= 11 && hcp <= 12 && dengeli && s < 4) return "2NT";

            // 6) 3♣: 5♦4♠3♣1♥ (9-11 HCP)
            if (d >= 5 && s >= 4 && c >= 3 && h >= 1 && hcp >= 9 && hcp <= 11) return "3♣";

            // 7) 2♦: 6♦4♠ (9-11 HCP)
            if (d >= 6 && s >= 4 && hcp >= 9 && hcp <= 11) return "2♦";

            // 8) 1NT: 6-9 HCP dengeli, majör yok, ♠ stopper
            if (hcp >= 6 && hcp <= 9 && dengeli && s < 4 && sStopper) return "1NT";

            // Fallback
            return "1NT";
        }

        private string AciciRoleCevap(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null) return "Pas";

            int hcp = ElDegerlendirici.HCP(el);
            int s = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            int h = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int d = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            int c = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            bool max = hcp >= 16;

            string teklif;

            if (d >= 4 && h >= 4 && hcp >= 13) teklif = "3♥";          // Açıcı: 4♦ + 4♥, 13+ HCP (SI)
            else if (d >= 5 && hcp >= 10 && hcp <= 11) teklif = "3♦";   // Açıcı: 5♦, 10-11 HCP (MIN)
            else if (d >= 4 && hcp >= 11 && hcp <= 12) teklif = "2NT";  // Açıcı: 4♦, 11-12 HCP (MIN)
            else if (s >= 4 && hcp >= 11 && hcp <= 14) teklif = "2♠";   // Açıcı: 4♠, 11-14 HCP
            else if (h >= 4 && c >= 4 && hcp >= 10 && hcp <= 11) teklif = "3♣"; // Açıcı: 4♥ + 4♣, 10-11 HCP
            else if (d >= 3 && !max) teklif = "2♦";                     // Açıcı: 3♦, MIN
            else if (d >= 3 && max) teklif = "2♥";                      // Açıcı: 3♦, MAX
            else teklif = "Pas";                                        // Default fallback

            if (Aciklamalar.ContainsKey(teklif))
                return $"{teklif} → {Aciklamalar[teklif]}";

            return teklif;
        }

    }
}
