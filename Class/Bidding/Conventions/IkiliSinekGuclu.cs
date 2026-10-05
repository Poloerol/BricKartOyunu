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
    /// - 9 tricks (yaklaşık 22+ HP veya çok kuvvetli dağılım)
    /// 
    /// Amaç: Çok güçlü elleri tek seferde göstermek.
    /// 
    /// Öncelik: 5 (tüm açılışlardan önce kontrol edilir)
    /// 
    /// Örnek:
    ///   El: ♠AKQ54 ♥AKQ3 ♦AK ♣T87  → 22+ HP → "2♣"
    ///   El: ♠AKQJ54 ♥AK ♦KQ2 ♣A87  → 23 HP → "2♣"
    ///   El: ♠AKQJ54 ♥AKQ3 ♦KQ2 ♣-  → 23 HP + void → "2♣"
    /// </summary>
    public class IkiliSinekGuclu : IKonvansiyon
    {
        // ═══════════════════════════════════════════════════════════════════
        // ARAYÜZ PROPERTYLERİ
        // ═══════════════════════════════════════════════════════════════════

        public string Ad => "2♣ Güçlü";

        public bool AktifMi { get; set; } = true;

        /// <summary>
        /// Öncelik 5 — tüm açılışlardan önce kontrol edilir.
        /// </summary>
        public int Oncelik => 5;

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null) return false;
            if (durum.AktifOyuncuEli.Count != 13) return false;

            // 1. Sadece ilk teklifte geçerli
            if (!durum.IlkTeklifMi()) return false;

            var el = durum.AktifOyuncuEli;

            // 2. HP kontrolü
            int hp = ElDegerlendirici.HCP(el);

            // 22+ HP → 2♣
            if (hp >= 22) return true;

            // 20-21 HP + dengeli → 2NT (bu konvansiyonda değil — StrongNT 20-21 için ayrı)
            // NOT: Weak NT veya 20-21 için ayrı konvansiyon eklenebilir.
            // Şimdilik 2NT'yi es geçiyoruz.

            // 3. 9 tricks kontrolü (basit)
            //    - 20+ HP + 5-5 dağılım → yaklaşık 9 tricks
            //    - 19+ HP + 6-4 dağılım → yaklaşık 9 tricks
            if (hp >= 19)
            {
                if (YuksekDagilimTrickVarMi(el)) return true;
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

            // 5-4-4-0
            if (sayilar[0] == 5 && sayilar[1] == 4 && sayilar[2] == 4) return true;

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VERME
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            // 2♣ yapay güçlü açılış
            return "2♣";
        }
    }
}