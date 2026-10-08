using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 2♣ Yapay Güçlü Açılış Konvansiyonu.
    ///
    /// Kural:
    /// - 22+ HP VEYA
    /// - Tek başına 9+ trick alabilecek kadar kuvvetli el (HCP + Dağılım + Quick Tricks)
    ///
    /// Amaç: Çok güçlü elleri tek seferde göstermek ve partneriyle slam/grand slam planlamak.
    ///
    /// Öncelik: 5 (tüm açılışlardan önce kontrol edilir)
    /// </summary>
    public class IkiliSinekGuclu : IKonvansiyon
    {
        public string Ad => "2♣ Güçlü";
        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 5 — tüm açılışlardan önce kontrol edilir.
        /// </summary>
        public int Oncelik => 5;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // Sadece ilk teklifte geçerli
            if (!durum.IlkTeklifMi()) return false;

            var el = durum.AktifOyuncuEli;

            // 1. Saf HP Kontrolü: 22+ HP her zaman 2♣ açar
            int hp = ElDegerlendirici.HCP(el);
            if (hp >= 22) return true;

            // 2. Trick Potansiyeli Kontrolü (Yaklaşık 9+ trick)
            // Modern yaklaşım: Quick Tricks + Dağılım Puanı + HCP ağırlığı
            double quickTricks = ElDegerlendirici.HizliElSayisi(el);
            int dagilimPuani = ElDegerlendirici.DagilimPuani(el);

            // Kural: Quick Tricks + (Dağılım/2) + (HCP/4) yaklaşık 9'u geçiyorsa
            // Veya daha basitçe: Quick Tricks >= 7 ve HP >= 19 ise dağılıma bak
            if (hp >= 19)
            {
                if (quickTricks >= 6.5 || YuksekDagilimTrickVarMi(el))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Elin dağılımı "trick üretici" mi?
        /// 5-5, 6-4, 6-5, 5-4-4-0, vs.
        /// </summary>
        private bool YuksekDagilimTrickVarMi(List<Card> el)
        {
            var sayilar = ElDegerlendirici.RenkSayilari(el)
                .Values.OrderByDescending(x => x).ToArray();

            // 6-4 veya daha uzun iki renk
            if (sayilar[0] >= 6 && sayilar[1] >= 4) return true;

            // 5-5 iki renk
            if (sayilar[0] >= 5 && sayilar[1] >= 5) return true;

            // 5-4-4-0 veya benzeri aşırı dengesiz güçlü eller
            if (sayilar[0] >= 5 && sayilar[1] >= 4 && sayilar[2] >= 4) return true;
            if (sayilar[3] == 0 && ElDegerlendirici.HCP(el) >= 20) return true;

            return false;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            // 2♣ yapay güçlü açılış
            return "2♣";
        }
    }
}
