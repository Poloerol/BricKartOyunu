using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Preempt Cevap — ortağın 3 seviyesindeki bozucu açılışına (3♣/3♦/3♥/3♠) cevap.
    ///
    /// Neden gerekli:
    ///   Ortak 3♠ açtığında Basit Cevap (1 seviyesi açılış cevabı) devreye giriyor ve
    ///   "yeni renk" teklif ediyordu (ör. 7 HP + 5'li karo → 4♦). Bozucu açılışa yeni renk
    ///   söylenmez: ortak 7+ kartlı rengiyle zaten elini tarif etti.
    ///
    /// Kurallar (ortağın bozucu açılışından sonra, ben henüz konuşmadım):
    ///   Majör bozucu (3♥/3♠):
    ///     - 3+ destek                → 4M (bozucu/oyun yükseltmesi, 10+ koz)
    ///     - 2 destek ve 14+ HP       → 4M (oyun)
    ///     - aksi halde               → Pas
    ///   Minör bozucu (3♣/3♦):
    ///     - 15+ HP, 2+ destek, diğer üç renkte durdurucu → 3NT
    ///     - aksi halde               → Pas
    ///
    ///   Yeni renk ASLA söylenmez. Rakip kontra/Pas fark etmez; kural aynıdır.
    ///
    /// Öncelik 9: Preempt (açılış) ile aynı grup; Basit Cevap (100) öncesi.
    /// </summary>
    public class PreemptCevap : IKonvansiyon
    {
        public string Ad => "Preempt Cevap";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 9;

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.Gecmis == null) return false;
            if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // Ben henüz konuşmadım
            if (durum.KendiTeklifleri.Count > 0) return false;

            // İhalenin İLK gerçek teklifi ortağın 3 seviyesinde renk açılışı olmalı
            var ilk = durum.Gecmis.FirstOrDefault(h => h.GercekTeklifMi);
            if (ilk == null || ilk.Oyuncu != durum.Partner) return false;
            if (ilk.Seviye != 3) return false;

            string koz = KozYardimcisi.TekliftenKozCikar(ilk.Teklif);
            if (string.IsNullOrEmpty(koz) || koz == "NT") return false;

            // Rakip gerçek bir teklif verdiyse (müdahale) bu basit kural uygulanmaz.
            // Kontra/Pas serbest.
            if (durum.RakipActiMi()) return false;

            return true;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            var ilk = durum.Gecmis.First(h => h.GercekTeklifMi);
            string koz = KozYardimcisi.TekliftenKozCikar(ilk.Teklif);
            string sembol = KozYardimcisi.KozSembolu(koz);

            int hp = ElDegerlendirici.HCP(el);
            int destek = ElDegerlendirici.RenkUzunlugu(el, koz);

            bool major = koz == "Maça" || koz == "Kupa";

            if (major)
            {
                if (destek >= 3) return "4" + sembol;
                if (destek >= 2 && hp >= 14) return "4" + sembol;
                return "Pas";
            }

            // Minör bozucu: 3NT sadece güçlü elle ve tüm yan renklerde durdurucu varsa
            if (hp >= 15 && destek >= 2)
            {
                bool hepsiDurdurulur = new[] { "Maça", "Kupa", "Karo", "Sinek" }
                    .Where(r => r != koz)
                    .All(r => ElDegerlendirici.StopperVar(el, r));
                if (hepsiDurdurulur) return "3NT";
            }

            return "Pas";
        }
    }
}