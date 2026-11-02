using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;   // KozYardimcisi için

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Cue Bid (Kontrol Gösterimi) — Max Hardy'ye göre.
    ///
    /// Koz MAJÖR anlaşıldıktan sonra (3M/4M fit, oyun zorlayıcı), 4 seviyesinde ve 4M'nin ALTINDA
    /// yapılan yan renk teklifleri kontrol gösterir:
    ///  - Birinci tur kontrol: As ya da void. Önce birinci tur kontroller, en ucuz renkten yukarı doğru.
    ///  - İkinci tur kontrol (papaz ya da tek kart): ortak cue-bid yaptıktan sonra, birinci tur kontrol kalmadıysa.
    ///  - Bir rengi atlamak o renkte (o adımda) kontrol olmadığını gösterir; cue-bid'ler yalnızca yukarı doğru gider.
    ///  - Ortak cue-bid yaptığında cevap verilir: kontrolü olan en ucuz renk, yoksa 4M (slem ilgisizliği).
    ///  - Cue-bid başlatmak için slem değeri gerekir (15+ HP, ortak alt sınırı ile toplam 29+); cevap vermek için HP şartı yoktur.
    ///  - Zaten doğal teklif edilmiş renkler cue-bid sayılmaz.
    ///
    /// Öncelik 14 (Serious 3NT 13'ten sonra, Blackwood 15'ten önce).
    /// </summary>
    public class CueBid : IKonvansiyon
    {
        public string Ad => "Cue Bid";
        public string Aciklama => "Kontrol gösteren cue-bid";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 14;

        private static readonly string[] Siralama = { "Sinek", "Karo", "Kupa", "Maça" };

        private static int Sira(string renk) { return Array.IndexOf(Siralama, renk); }

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null) return false;
            if (durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;

            // 4NT (RKCB/Blackwood) sekansı: cue-bid yok
            if (durum.SonGercekTeklif == "4NT" && durum.SonGercekTeklifSahibi == durum.Partner) return false;
            if (durum.KendiTeklifleri.Contains("4NT")) return false;

            string koz = durum.AnlasilanKoz();
            if (koz != "Maça" && koz != "Kupa" && koz != "Karo" && koz != "Sinek") return false;
            bool minor = koz == "Karo" || koz == "Sinek";

            int seviye = durum.KacinciSeviye();
            if (seviye < 3 || seviye > 4) return false;
            // Minörde 4m (oyun) teklif edildiyse cue-bid devri geçti
            if (minor && seviye >= 4 && durum.Gecmis.Any(h => h.GercekTeklifMi && h.Koz == koz && h.Seviye >= 4)) return false;

            string secim = Sec(durum, koz);
            return secim != null;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            string koz = durum.AnlasilanKoz();
            string secim = Sec(durum, koz);
            return secim ?? "Pas";
        }

        // ═══════════════════════════════════════════════════════════════════
        // SEÇİM
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Bu elde yapılacak cue-bid ("4♣" gibi), 4M (slem ilgisizliği) ya da null (bu konvansiyon konuşmaz).</summary>
        private static string Sec(IhaleDurumu durum, string koz)
        {
            var el = durum.AktifOyuncuEli;
            var gercek = durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
            if (gercek.Count == 0) return null;
            var son = gercek[gercek.Count - 1];

            // Rakip müdahalesi varsa karışma
            if (gercek.Any(h => durum.Rakipler.Contains(h.Oyuncu))) return null;

            string kozSembol = KozYardimcisi.KozSembolu(koz);
            bool minor = koz == "Karo" || koz == "Sinek";
            // Cue-bid sınırı: majörde 4M'nin altı; minörde 4m'nin altı (3NT atlanır)
            int kozUz = ElDegerlendirici.RenkUzunlugu(el, koz);

            // Zaten doğal teklif edilmiş (3. seviye ve altı) renkler cue-bid sayılmaz
            int dogalSv = (koz == "Karo" || koz == "Sinek") ? 2 : 3;
            var dogal = new HashSet<string>(gercek.Where(h => h.Seviye <= dogalSv && h.Koz != "NT" && h.Koz != koz).Select(h => h.Koz));
            // Kendi cue-bid'lerim ve ortağınkiler (4 seviyesi, koz dışı, majörün altı)
            Func<IhaleHamlesi, bool> cueMi = h => h.Koz != "NT" && h.Koz != koz && !dogal.Contains(h.Koz) &&
                                                  (minor ? (h.Seviye == 3 || h.Seviye == 4) && (h.Seviye < 4 || Sira(h.Koz) < Sira(koz))
                                                         : h.Seviye == 4 && Sira(h.Koz) < Sira(koz));
            var benCue = gercek.Where(h => h.Oyuncu == durum.AktifOyuncu && cueMi(h)).Select(h => h.Koz).ToList();
            var ortakCue = gercek.Where(h => h.Oyuncu == durum.Partner && cueMi(h)).Select(h => h.Koz).ToList();

            bool ortakSonCue = son.Oyuncu == durum.Partner && cueMi(son);

            // Oyun seviyesinde koz teklif edildiyse (oyun sinyali) bu konvansiyon konuşmaz
            if (son.Seviye >= 4 && son.Koz == koz) return null;
            if (minor && son.Seviye >= 4 && son.Koz != "NT" && Sira(son.Koz) >= Sira(koz)) return null;

            int hp = ElDegerlendirici.HCP(el);

            // Başlatma: ortak henüz cue-bid yapmadı → slem değeri şart
            if (ortakCue.Count == 0)
            {
                if (hp < 15) return null;
                var ortAr = IhaleKoruma.PartnerAraligi(durum);
                if (hp + ortAr.Min < (minor ? 30 : 29)) return null;
            }
            else if (!ortakSonCue)
            {
                // Ortak daha önce cue-bid yaptı ama son hamle onun değil (ör. ben sonra konuştum): devam sadece slem değeriyle
                if (hp < 15) return null;
            }

            // Aday renkler: yukarı doğru, son gerçek teklifin üzerinde, majörün altında, daha önce cue-bid yapmadığım
            // Her aday (seviye, renk) çifti: majörde 4 seviyesi; minörde en ucuz yasal seviye (3 ya da 4), 3NT atlanır
            var adaylar = new List<string>();
            var aday_seviye = new Dictionary<string, int>();
            foreach (var r in Siralama)
            {
                if (r == koz || dogal.Contains(r) || benCue.Contains(r)) continue;
                if (!minor && Sira(r) > Sira(koz)) continue;
                int lv = -1;
                foreach (int l in minor ? new[] { 3, 4 } : new[] { 4 })
                    if (Yasal(son, l, r)) { lv = l; break; }
                if (lv < 0) continue;
                if (minor && lv == 4 && Sira(r) > Sira(koz)) continue;   // 4 seviyesinde 4m'nin üstü cue-bid değil
                adaylar.Add(r); aday_seviye[r] = lv;
            }

            // Önce birinci tur kontrol (As ya da void)
            foreach (var r in adaylar)
            {
                int uz = ElDegerlendirici.RenkUzunlugu(el, r);
                bool as_ = el.Any(c => c.Suit == r && c.Value == 14);
                if (as_ || uz == 0) return aday_seviye[r] + KozYardimcisi.KozSembolu(r);
            }

            // İkinci tur kontrol (papaz ya da tek kart): yalnızca ortağın cue-bid'ine cevap / devam ederken
            if (ortakCue.Count > 0)
            {
                foreach (var r in adaylar)
                {
                    int uz = ElDegerlendirici.RenkUzunlugu(el, r);
                    bool papaz = el.Any(c => c.Suit == r && c.Value == 13);
                    if (papaz || (uz == 1 && kozUz >= 3)) return aday_seviye[r] + KozYardimcisi.KozSembolu(r);
                }
                // Kontrolüm kalmadı: ortak cue-bid yaptıysa ve son söz onunsa slem ilgisizliği → 4M
                if (ortakSonCue && !minor && Yasal(son, 4, koz)) return "4" + kozSembol;
                if (ortakSonCue && minor)
                {
                    foreach (int l in new[] { 4, 5 })
                        if (Yasal(son, l, koz)) return l + kozSembol;
                }
            }

            return null;
        }

        /// <summary>"seviye renk" teklifi, son gerçek teklifin üzerinde mi?</summary>
        private static bool Yasal(IhaleHamlesi son, int seviye, string renk)
        {
            if (son.Seviye < seviye) return true;
            if (son.Seviye > seviye) return false;
            if (son.Koz == "NT") return false;
            return Sira(renk) > Sira(son.Koz);
        }
    }
}
