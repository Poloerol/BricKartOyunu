using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// İhale sırasındaki tüm durum bilgisini taşır.
    /// 
    /// Bu sınıf, AI'ın her karar verirken ihtiyaç duyduğu bilgileri saklar:
    /// - Sıra kimde? (AktifOyuncu)
    /// - Ne teklif edildi? (Gecmis)
    /// - Son teklif ne? (SonGercekTeklif)
    /// - Partner ne dedi? (PartnerTeklifleri)
    /// - Rakip ne dedi? (RakipTeklifleri)
    /// - Kaç pas? (UstUstePasSayisi)
    /// - Zon durumu? (ZonNS, ZonEW)
    /// - Hangi anlaşma? (Anlasma)
    /// - Aktif oyuncunun eli? (AktifOyuncuEli)
    /// 
    /// Kullanım:
    ///   var durum = new IhaleDurumu
    ///   {
    ///       AktifOyuncu = Player.Guney,
    ///       Gecmis = new List<IhaleHamlesi>(),
    ///       Anlasma = OrtaklikAnlasmasi.Varsayilan(),
    ///       ZonNS = true,
    ///       ZonEW = false,
    ///       AktifOyuncuEli = guneyEl
    ///   };
    ///   if (durum.IlkTeklifMi()) { ... }
    /// </summary>
    public class IhaleDurumu
    {
        // ═══════════════════════════════════════════════════════════════════
        // TEMEL BİLGİLER
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Şu an ihale sırası kimde?
        /// </summary>
        public Player AktifOyuncu { get; set; }

        /// <summary>
        /// Kullanıcı (insan) oyuncu. Genelde Güney.
        /// </summary>
        public Player InsanOyuncu { get; set; } = Player.Guney;

        /// <summary>
        /// Aktif oyuncunun eli (13 kart).
        /// </summary>
        public List<Card> AktifOyuncuEli { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // İHALE GEÇMİŞİ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Tüm yapılan hamleler (sırayla).
        /// </summary>
        public List<IhaleHamlesi> Gecmis { get; set; } = new List<IhaleHamlesi>();

        // ═══════════════════════════════════════════════════════════════════
        // DIŞ BİLGİLER
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Kuzey/Güney zonda mı?
        /// </summary>
        public bool ZonNS { get; set; }

        /// <summary>
        /// Batı/Doğu zonda mı?
        /// </summary>
        public bool ZonEW { get; set; }

        /// <summary>
        /// Bord numarası (Tur numarası).
        /// </summary>
        public int TurNo { get; set; } = 1;

        /// <summary>
        /// Ortaklık anlaşması (hangi konvansiyonlar aktif?).
        /// </summary>
        public OrtaklikAnlasmasi Anlasma { get; set; } = OrtaklikAnlasmasi.Varsayilan();

        // ═══════════════════════════════════════════════════════════════════
        // TÜRETİLMİŞ BİLGİLER
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aktif oyuncunun partneri.
        /// </summary>
        public Player Partner => PartnerBul(AktifOyuncu);

        /// <summary>
        /// Aktif oyuncunun rakipleri (2 oyuncu).
        /// </summary>
        public List<Player> Rakipler => RakipleriBul(AktifOyuncu);

        /// <summary>
        /// Son gerçek teklif (Pas/Kontr/S.Kontr değil).
        /// Örnek: "1♠", "2NT", "3♥"
        /// </summary>
        public string SonGercekTeklif =>
            Gecmis.Where(h => h.GercekTeklifMi).LastOrDefault()?.Teklif;

        /// <summary>
        /// Son gerçek teklifi yapan oyuncu.
        /// </summary>
        public Player? SonGercekTeklifSahibi =>
            Gecmis.Where(h => h.GercekTeklifMi).LastOrDefault()?.Oyuncu;

        /// <summary>
        /// Son hamle (herhangi bir: Pas, teklif, Kontr, vs.).
        /// </summary>
        public IhaleHamlesi SonHamle =>
            Gecmis.LastOrDefault();

        /// <summary>
        /// Üst üste pas sayısı (sondan geriye doğru).
        /// </summary>
        public int UstUstePasSayisi
        {
            get
            {
                int sayi = 0;
                for (int i = Gecmis.Count - 1; i >= 0; i--)
                {
                    if (Gecmis[i].PasMi) sayi++;
                    else break;
                }
                return sayi;
            }
        }

        /// <summary>
        /// Son teklife Kontr atıldı mı?
        /// </summary>
        public bool SonTeklifeKontrVarMi
        {
            get
            {
                // Son hamle Kontr mı?
                if (SonHamle != null && SonHamle.KontrMi) return true;
                return false;
            }
        }

        /// <summary>
        /// Son teklife S.Kontr atıldı mı?
        /// </summary>
        public bool SonTeklifeSKontrVarMi
        {
            get
            {
                if (SonHamle != null && SonHamle.SKontrMi) return true;
                return false;
            }
        }

        /// <summary>
        /// Aktif oyuncunun kendi teklifleri (sırayla).
        /// </summary>
        public List<string> KendiTeklifleri =>
            Gecmis.Where(h => h.Oyuncu == AktifOyuncu)
                  .Select(h => h.Teklif)
                  .ToList();

        /// <summary>
        /// Aktif oyuncunun partnerinin teklifleri (sırayla).
        /// </summary>
        public List<string> PartnerTeklifleri =>
            Gecmis.Where(h => h.Oyuncu == Partner)
                  .Select(h => h.Teklif)
                  .ToList();

        /// <summary>
        /// Aktif oyuncunun rakiplerinin teklifleri (sırayla).
        /// </summary>
        public List<string> RakipTeklifleri =>
            Gecmis.Where(h => Rakipler.Contains(h.Oyuncu))
                  .Select(h => h.Teklif)
                  .ToList();

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI METOTLAR — DURUM KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Bu ilk teklif mi? (Henüz kimse konuşmadı veya herkes pas dedi.)
        /// </summary>
        public bool IlkTeklifMi()
        {
            return Gecmis.Count == 0 ||
                   Gecmis.All(h => h.PasMi);
        }

        /// <summary>
        /// Partner daha önce açtı mı? (Gerçek teklif verdi mi?)
        /// </summary>
        public bool PartnerActiMi()
        {
            return Gecmis.Any(h => h.Oyuncu == Partner && h.GercekTeklifMi);
        }

        /// <summary>
        /// Rakip daha önce açtı mı?
        /// </summary>
        public bool RakipActiMi()
        {
            return Gecmis.Any(h => Rakipler.Contains(h.Oyuncu) && h.GercekTeklifMi);
        }

        /// <summary>
        /// İhale ortasında mıyız? (İlk teklif değil, ama ihale de bitmemiş.)
        /// </summary>
        public bool OrtaMiyiz()
        {
            return !IlkTeklifMi() && !IhaleBittiMi();
        }

        /// <summary>
        /// İhale bitti mi? (3 pas veya 4 pas.)
        /// </summary>
        public bool IhaleBittiMi()
        {
            // 4 pas (hiç teklif verilmedi)
            if (IlkTeklifMi() && UstUstePasSayisi == 4) return true;

            // Son tekliften sonra 3 pas
            if (!IlkTeklifMi() && UstUstePasSayisi == 3) return true;

            return false;
        }

        /// <summary>
        /// Şu an hangi seviyedeyiz?
        /// (Son gerçek teklifin seviyesi. Hiç teklif yoksa 0.)
        /// </summary>
        public int KacinciSeviye()
        {
            var sonTeklif = Gecmis.Where(h => h.GercekTeklifMi).LastOrDefault();
            if (sonTeklif == null) return 0;
            return sonTeklif.Seviye;
        }

        /// <summary>
        /// Bu renk daha önce teklif edildi mi?
        /// </summary>
        public bool BuRenkTeklifEdildiMi(string renk)
        {
            if (string.IsNullOrEmpty(renk)) return false;
            return Gecmis.Any(h => h.GercekTeklifMi && h.Koz == renk);
        }

        /// <summary>
        /// Bu seviyede (örneğin 1 seviyesinde) bu renk teklif edildi mi?
        /// </summary>
        public bool BuSeviyedeRenkTeklifEdildiMi(int seviye, string renk)
        {
            return Gecmis.Any(h => h.GercekTeklifMi &&
                                   h.Seviye == seviye &&
                                   h.Koz == renk);
        }

        /// <summary>
        /// Aktif oyuncunun partneriyle bir renkte toplam uzunluğu ne?
        /// (Sadece "kaç kart" bilgisi verir, gerçek destek hesabı değil.)
        /// </summary>
        public int PartnerleToplamUzunluk(string renk)
        {
            if (AktifOyuncuEli == null) return 0;

            int benim = AktifOyuncuEli.Count(c => c.Suit == renk);
            // Partner elini bilmiyoruz, sadece benim uzunluğum
            return benim;
        }

        /// <summary>
        /// Partnerle ortak olarak anlaşılan kozu bulur.
        /// 
        /// Mantık:
        /// 1. Partner ve ben aynı rengi teklif ettiysek → o renk
        /// 2. Son majör teklifi (partnerden veya benden) → o majör
        /// 3. NT açılışı yapıldıysa → "NT"
        /// 4. Bulunamazsa → null
        /// 
        /// Örnek: 1♠ (Partner) - 3♠ (Ben) → "Maça"
        ///        1NT (Partner) - 2♣ (Ben, Stayman) → "NT"
        /// </summary>
        public string AnlasilanKoz()
        {
            if (Gecmis == null || Gecmis.Count == 0) return null;

            // 1. Ben ve partner aynı rengi teklif ettik mi?
            var benimRenkler = Gecmis
                .Where(h => h.Oyuncu == AktifOyuncu && h.GercekTeklifMi)
                .Select(h => h.Koz)
                .Where(k => !string.IsNullOrEmpty(k) && k != "NT")
                .ToList();

            var partnerRenkler = Gecmis
                .Where(h => h.Oyuncu == Partner && h.GercekTeklifMi)
                .Select(h => h.Koz)
                .Where(k => !string.IsNullOrEmpty(k) && k != "NT")
                .ToList();

            // Her iki tarafta da geçen renk (ortak koz)
            var ortakRenkler = benimRenkler.Intersect(partnerRenkler).ToList();
            if (ortakRenkler.Count > 0)
            {
                // En son anlaşılan rengi al
                return ortakRenkler.Last();
            }

            // 2. Sadece majör bir renk anlaşması var mı?
            //    (Örn. partner 1♠ açtı, ben 2♠ dedim — bu ortakRenkler'e girer)

            // 3. NT açılışı yapıldı mı?
            bool ntVar = Gecmis.Any(h => h.GercekTeklifMi && h.Koz == "NT");
            if (ntVar) return "NT";

            return null;
        }
        // ═══════════════════════════════════════════════════════════════════
        // GEÇMİŞ YÖNETİMİ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Yeni bir hamle ekler ve sırayı ilerletir.
        /// </summary>
        public void HamleEkle(string teklif)
        {
            var hamle = new IhaleHamlesi
            {
                Oyuncu = AktifOyuncu,
                Teklif = teklif,
                Sira = Gecmis.Count + 1,
                GecerliMi = true
            };

            Gecmis.Add(hamle);

            System.Diagnostics.Debug.WriteLine(
                $"[IhaleDurumu] Hamle eklendi: {hamle}");

            // Sırayı ilerlet
            AktifOyuncu = SonrakiOyuncu(AktifOyuncu);
        }

        /// <summary>
        /// Son hamleyi iptal eder (geri al).
        /// </summary>
        public bool SonHamleyiGeriAl()
        {
            if (Gecmis.Count == 0) return false;

            var son = Gecmis[Gecmis.Count - 1];
            Gecmis.RemoveAt(Gecmis.Count - 1);

            // Sırayı geri al
            AktifOyuncu = son.Oyuncu;

            System.Diagnostics.Debug.WriteLine(
                $"[IhaleDurumu] Hamle geri alındı: {son}");

            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI STATİK METOTLAR
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Bir oyuncunun partnerini döndürür.
        /// Kuzey↔Güney, Batı↔Doğu
        /// </summary>
        public static Player PartnerBul(Player oyuncu)
        {
            switch (oyuncu)
            {
                case Player.Kuzey: return Player.Guney;
                case Player.Guney: return Player.Kuzey;
                case Player.Bati: return Player.Dogu;
                case Player.Dogu: return Player.Bati;
                default: return Player.Guney;
            }
        }

        /// <summary>
        /// Bir oyuncunun rakiplerini döndürür (2 oyuncu).
        /// </summary>
        public static List<Player> RakipleriBul(Player oyuncu)
        {
            switch (oyuncu)
            {
                case Player.Kuzey:
                case Player.Guney:
                    return new List<Player> { Player.Bati, Player.Dogu };
                case Player.Bati:
                case Player.Dogu:
                    return new List<Player> { Player.Kuzey, Player.Guney };
                default:
                    return new List<Player>();
            }
        }

        /// <summary>
        /// Sıradaki oyuncuyu döndürür (saat yönünde).
        /// Güney → Batı → Kuzey → Doğu → Güney
        /// </summary>
        public static Player SonrakiOyuncu(Player mevcut)
        {
            switch (mevcut)
            {
                case Player.Guney: return Player.Bati;
                case Player.Bati: return Player.Kuzey;
                case Player.Kuzey: return Player.Dogu;
                case Player.Dogu: return Player.Guney;
                default: return Player.Guney;
            }
        }

        /// <summary>
        /// Önceki oyuncuyu döndürür (saat yönünün tersi).
        /// </summary>
        public static Player OncekiOyuncu(Player mevcut)
        {
            switch (mevcut)
            {
                case Player.Guney: return Player.Dogu;
                case Player.Dogu: return Player.Kuzey;
                case Player.Kuzey: return Player.Bati;
                case Player.Bati: return Player.Guney;
                default: return Player.Guney;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // DEBUG
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// İhale durumunu özet metin olarak yazdırır (debug için).
        /// </summary>
        public string Ozet()
        {
            var sonHamleler = Gecmis.Skip(Math.Max(0, Gecmis.Count - 4));
            string son = string.Join(" | ", sonHamleler.Select(h => h.ToString()));

            return $"Aktif: {AktifOyuncu}, Hamle sayısı: {Gecmis.Count}, " +
                   $"Son: {son}";
        }
    }
}