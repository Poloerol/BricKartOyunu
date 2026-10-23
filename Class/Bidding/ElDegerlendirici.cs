using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// Briç elini değerlendiren yardımcı sınıf.
    /// 
    /// Bu sınıf şunları hesaplar:
    /// - HCP (High Card Points): A=4, K=3, Q=2, J=1
    /// - Dağılım puanı (Koz varsa Kısalık, yoksa Uzunluk)
    /// - Toplam puan: HCP + Dağılım
    /// - Renk sayıları (her renkten kaç kart)
    /// - Dengeli/dengesiz el kontrolü
    /// - Majör/minör uzunlukları
    /// - Stopper (durdurucu) kontrolü
    /// - Fit (Koz uyumu) kontrolü
    /// </summary>
    public static class ElDegerlendirici
    {
        // ═══════════════════════════════════════════════════════════════════
        // PUAN HESAPLAMA
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// HCP (High Card Points) hesaplar.
        /// As = 4, Papaz = 3, Kız = 2, Vale = 1
        /// </summary>
        public static int HCP(List<Card> el)
        {
            if (el == null) return 0;

            int puan = 0;
            foreach (var c in el)
            {
                switch (c.Value)
                {
                    case 14: puan += 4; break;  // As
                    case 13: puan += 3; break;  // Papaz
                    case 12: puan += 2; break;  // Kız
                    case 11: puan += 1; break;  // Vale
                }
            }
            return puan;
        }

        /// <summary>
        /// Dağılım puanı hesaplar.
        /// 
        /// kozRengi null veya boş ise: SADECE uzun renk puanı verilir (NT veya fit yok).
        ///   - 5'li renk = +1, 6'lı = +2, 7'li = +3, vs. (her fazla kart için +1)
        /// kozRengi belirli ise: KOZ DIŞINDAKİ renklerde kısalık puanı verilir.
        ///   - Void (şikan) = +3
        ///   - Singleton = +2
        ///   - Doubleton = +1
        ///   - Ayrıca koz rengi 5+ ise uzunluk puanı da eklenir.
        /// </summary>
        public static int DagilimPuani(List<Card> el, string kozRengi = null)
        {
            if (el == null) return 0;

            int puan = 0;
            var sayilar = RenkSayilari(el);

            if (string.IsNullOrEmpty(kozRengi))
            {
                // KOZ YOK (NT veya fit bulunmamış): Sadece uzun renk puanı
                foreach (var sayi in sayilar.Values)
                {
                    if (sayi >= 5) puan += (sayi - 4); // 5'li = +1, 6'lı = +2, vs.
                }
            }
            else
            {
                // KOZ VAR (fit bulunmuş): Koz dışındaki renklerde kısalık puanı
                foreach (var kv in sayilar)
                {
                    if (kv.Key == kozRengi) continue; // Koz rengine kısalık puanı verilmez

                    if (kv.Value == 0) puan += 3;       // Void
                    else if (kv.Value == 1) puan += 2;  // Singleton
                    else if (kv.Value == 2) puan += 1;  // Doubleton
                }

                // Koz renginde uzunluk puanı (5+ kart varsa)
                int kozSayisi = sayilar.ContainsKey(kozRengi) ? sayilar[kozRengi] : 0;
                if (kozSayisi >= 5) puan += (kozSayisi - 4);
            }

            return puan;
        }

        /// <summary>
        /// Toplam puan = HCP + Dağılım puanı.
        /// kozRengi null ise NT puanı, dolu ise fit puanı hesaplanır.
        /// </summary>
        public static int ToplamPuan(List<Card> el, string kozRengi = null)
        {
            return HCP(el) + DagilimPuani(el, kozRengi);
        }

        // ═══════════════════════════════════════════════════════════════════
        // RENK SAYILARI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Her renkten kaç kart olduğunu döndürür.
        /// Örnek: { "Maça": 5, "Kupa": 3, "Karo": 3, "Sinek": 2 }
        /// </summary>
        public static Dictionary<string, int> RenkSayilari(List<Card> el)
        {
            var sayilar = new Dictionary<string, int>
            {
                { "Maça", 0 },
                { "Kupa", 0 },
                { "Karo", 0 },
                { "Sinek", 0 }
            };

            if (el == null) return sayilar;

            foreach (var c in el)
            {
                if (sayilar.ContainsKey(c.Suit))
                    sayilar[c.Suit]++;
            }

            return sayilar;
        }

        /// <summary>
        /// Belirli bir renkten kaç kart olduğunu döndürür.
        /// </summary>
        public static int RenkUzunlugu(List<Card> el, string renk)
        {
            if (el == null || string.IsNullOrEmpty(renk)) return 0;
            return el.Count(c => c.Suit == renk);
        }

        /// <summary>
        /// En uzun rengi döndürür.
        /// </summary>
        public static string EnUzunRenk(List<Card> el)
        {
            var sayilar = RenkSayilari(el);
            return sayilar.OrderByDescending(kv => kv.Value).First().Key;
        }

        /// <summary>
        /// İkinci en uzun rengi döndürür.
        /// </summary>
        public static string IkinciEnUzunRenk(List<Card> el)
        {
            var sayilar = RenkSayilari(el);
            return sayilar.OrderByDescending(kv => kv.Value).Skip(1).First().Key;
        }

        /// <summary>
        /// Belirli bir renkte en az X kart var mı?
        /// </summary>
        public static bool RenkVar(List<Card> el, string renk, int minimumAdet)
        {
            return RenkUzunlugu(el, renk) >= minimumAdet;
        }

        // ═══════════════════════════════════════════════════════════════════
        // FIT (KOZ UYUMU) KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// İki oyuncunun ortak kozda fit olup olmadığını kontrol eder.
        /// Fit tanımı: Aynı renkten toplam en az 8 kart.
        /// 
        /// En iyi fit 4+4'tür (her iki taraf da kısalık puanı kazanır).
        /// 
        /// partnerKozSayisi: Partnerin deklare ettiği rengin minimum kart sayısı
        /// (örn. partner 1♥ açtıysa 5, partner 3♥ dediyse 3 gibi tahmin edilir).
        /// </summary>
        public static bool FitVarMi(List<Card> benimEl, string koz, int partnerKozSayisi)
        {
            if (benimEl == null || string.IsNullOrEmpty(koz)) return false;
            if (partnerKozSayisi <= 0) return false;

            int benimKozSayim = RenkUzunlugu(benimEl, koz);
            return (benimKozSayim + partnerKozSayisi) >= 8;
        }

        // ═══════════════════════════════════════════════════════════════════
        // DAĞILIM KONTROLÜ
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// El dengeli mi?
        /// Dengeli dağılımlar: 4-3-3-3, 4-4-3-2, 5-3-3-2, 5-4-2-2, 6-3-2-2
        /// </summary>
        public static bool DengeliEl(List<Card> el)
        {
            if (el == null || el.Count != 13) return false;

            var sayilar = RenkSayilari(el).Values.OrderByDescending(x => x).ToArray();

            // 4-3-3-3
            if (sayilar[0] == 4 && sayilar[1] == 3 && sayilar[2] == 3 && sayilar[3] == 3)
                return true;

            // 4-4-3-2
            if (sayilar[0] == 4 && sayilar[1] == 4 && sayilar[2] == 3 && sayilar[3] == 2)
                return true;

            // 5-3-3-2
            if (sayilar[0] == 5 && sayilar[1] == 3 && sayilar[2] == 3 && sayilar[3] == 2)
                return true;

            // 5-4-2-2
            if (sayilar[0] == 5 && sayilar[1] == 4 && sayilar[2] == 2 && sayilar[3] == 2)
                return true;

            // 6-3-2-2
            if (sayilar[0] == 6 && sayilar[1] == 3 && sayilar[2] == 2 && sayilar[3] == 2)
                return true;

            return false;
        }

        /// <summary>
        /// Elde 5'li majör var mı? (Maça veya Kupa 5+)
        /// </summary>
        public static bool BesliMajorVar(List<Card> el)
        {
            return RenkUzunlugu(el, "Maça") >= 5 ||
                   RenkUzunlugu(el, "Kupa") >= 5;
        }

        /// <summary>
        /// Elde 4'lü majör var mı? (Maça veya Kupa 4+)
        /// </summary>
        public static bool DortluMajorVar(List<Card> el)
        {
            return RenkUzunlugu(el, "Maça") >= 4 ||
                   RenkUzunlugu(el, "Kupa") >= 4;
        }

        /// <summary>
        /// Elde 5'li minör var mı? (Karo veya Sinek 5+)
        /// </summary>
        public static bool BesliMinorVar(List<Card> el)
        {
            return RenkUzunlugu(el, "Karo") >= 5 ||
                   RenkUzunlugu(el, "Sinek") >= 5;
        }

        // ═══════════════════════════════════════════════════════════════════
        // STOPPER KONTROLÜ (NT için önemli)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Belirli bir renkte stopper (durdurucu) var mı?
        /// Stopper: As, Kx, QJx, J10xx, vs.
        /// Bu basit versiyon sadece As, Kx ve QJ kontrol eder.
        /// </summary>
        public static bool StopperVar(List<Card> el, string renk)
        {
            if (el == null) return false;

            var renkKartlari = el.Where(c => c.Suit == renk)
                                 .OrderByDescending(c => c.Value)
                                 .ToList();

            if (renkKartlari.Count == 0) return false;

            // As var mı?
            if (renkKartlari[0].Value == 14) return true;

            // Kx var mı? (Papaz + en az 1 kart)
            if (renkKartlari[0].Value == 13 && renkKartlari.Count >= 2) return true;

            // QJ var mı?
            if (renkKartlari.Count >= 2 &&
                renkKartlari[0].Value == 12 &&
                renkKartlari[1].Value == 11) return true;

            return false;
        }

        /// <summary>
        /// Elde kaç renkte stopper var? (0-4)
        /// </summary>
        public static int StopperSayisi(List<Card> el)
        {
            int sayi = 0;
            if (StopperVar(el, "Maça")) sayi++;
            if (StopperVar(el, "Kupa")) sayi++;
            if (StopperVar(el, "Karo")) sayi++;
            if (StopperVar(el, "Sinek")) sayi++;
            return sayi;
        }

        /// <summary>
        /// NT (No Trump) için el uygun mu?
        /// Dengeli + 3-4 stopper
        /// </summary>
        public static bool NTUygun(List<Card> el)
        {
            return DengeliEl(el) && StopperSayisi(el) >= 3;
        }

        // ═══════════════════════════════════════════════════════════════════
        // HIZLI EL SAYISI (QUICK TRICKS)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Hızlı el sayısı tahmini.
        /// As = 1, Kx = 0.5, KQ = 1, QJx = 0.5
        /// </summary>
        public static double HizliElSayisi(List<Card> el)
        {
            if (el == null) return 0;

            double sayi = 0;
            var renkler = new[] { "Maça", "Kupa", "Karo", "Sinek" };

            foreach (var renk in renkler)
            {
                var renkKartlari = el.Where(c => c.Suit == renk)
                                     .OrderByDescending(c => c.Value)
                                     .ToList();

                if (renkKartlari.Count == 0) continue;

                // As = 1
                if (renkKartlari[0].Value == 14) sayi += 1;

                // Kx = 0.5
                if (renkKartlari[0].Value == 13 && renkKartlari.Count >= 2) sayi += 0.5;

                // KQ = 1
                if (renkKartlari.Count >= 2 &&
                    renkKartlari[0].Value == 13 &&
                    renkKartlari[1].Value == 12) sayi += 0.5; // KQ toplam 1

                // QJx = 0.5
                if (renkKartlari.Count >= 2 &&
                    renkKartlari[0].Value == 12 &&
                    renkKartlari[1].Value == 11) sayi += 0.5;
            }

            return sayi;
        }

        // ═══════════════════════════════════════════════════════════════════
        // AS / PAPAZ / KİLİT KART SAYIMI (Blackwood & RKCB için)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Elde kaç As var? (0-4)
        /// </summary>
        public static int AsSayisi(List<Card> el)
        {
            if (el == null) return 0;
            return el.Count(c => c.Value == 14);
        }

        /// <summary>
        /// Elde kaç Papaz var? (0-4)
        /// </summary>
        public static int PapazSayisi(List<Card> el)
        {
            if (el == null) return 0;
            return el.Count(c => c.Value == 13);
        }

        /// <summary>
        /// Toplam kontrol sayısı = As + Papaz (0-8).
        /// </summary>
        public static int KontrolSayisi(List<Card> el)
        {
            return AsSayisi(el) + PapazSayisi(el);
        }

        /// <summary>
        /// RKCB için kilit kart sayısı = 4 As + koz Papazı (0-5).
        /// Koz "NT" ise sadece As'lar sayılır (NT'de koz K'si yok).
        /// </summary>
        public static int KilitKartSayisi(List<Card> el, string koz)
        {
            if (el == null) return 0;

            int sayi = AsSayisi(el);

            // NT'de koz Papazı yok
            if (string.IsNullOrEmpty(koz) || koz == "NT") return sayi;

            // Koz rengindeki Papaz'ı bul
            bool kozPapaziVar = el.Any(c => c.Suit == koz && c.Value == 13);
            if (kozPapaziVar) sayi++;

            return sayi;
        }

        /// <summary>
        /// Belirli bir renkte Papaz (K) var mı?
        /// </summary>
        public static bool PapazVar(List<Card> el, string renk)
        {
            if (el == null || string.IsNullOrEmpty(renk)) return false;
            return el.Any(c => c.Suit == renk && c.Value == 13);
        }

        /// <summary>
        /// Belirli bir renkte Kız (Q) var mı?
        /// </summary>
        public static bool KizVar(List<Card> el, string renk)
        {
            if (el == null || string.IsNullOrEmpty(renk)) return false;
            return el.Any(c => c.Suit == renk && c.Value == 12);
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Elin özet metni (debug için).
        /// Örnek: "15 HCP, 17 TP (NT), Dengeli, Maça:5 Kupa:3 Karo:3 Sinek:2"
        /// </summary>
        public static string Ozet(List<Card> el, string kozRengi = null)
        {
            if (el == null) return "Boş el";

            var sayilar = RenkSayilari(el);
            string dagilim = $"Maça:{sayilar["Maça"]} Kupa:{sayilar["Kupa"]} " +
                            $"Karo:{sayilar["Karo"]} Sinek:{sayilar["Sinek"]}";

            string dengeli = DengeliEl(el) ? "Dengeli" : "Dengesiz";
            string kozBilgi = string.IsNullOrEmpty(kozRengi) ? "NT" : $"{kozRengi} kozu";

            return $"{HCP(el)} HCP, {ToplamPuan(el, kozRengi)} TP ({kozBilgi}), {dengeli}, {dagilim}";
        }

        // ═══════════════════════════════════════════════════════════════════
        // 1♣ CEVAP SİSTEMİ İÇİN EK YARDIMCILAR
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Belirli bir renkte "iyi renk" var mı?
        /// Kriter: En az 5 kart, en az 2 onör (A/K/Q), ve içinde A veya K var.
        /// </summary>
        public static bool IyiRenk(List<Card> el, string renk)
        {
            if (el == null) return false;
            var kartlar = el.Where(c => c.Suit == renk)
                            .OrderByDescending(c => c.Value)
                            .ToList();
            if (kartlar.Count < 5) return false;

            int onorSayisi = kartlar.Count(c => c.Value >= 12);
            bool asVeyaPapazVar = kartlar.Any(c => c.Value == 14 || c.Value == 13);

            return onorSayisi >= 2 && asVeyaPapazVar;
        }

        /// <summary>
        /// El tam olarak 4-3-3-3 dağılımında mı? (Belirli renk 4'lü, diğer üçü 3'erli)
        /// </summary>
        public static bool Dagilim4333(List<Card> el, string dortluRenk)
        {
            if (el == null || el.Count != 13) return false;
            var sayilar = RenkSayilari(el);
            if (sayilar[dortluRenk] != 4) return false;
            return sayilar.Values.Count(s => s == 3) == 3;
        }

        /// <summary>
        /// 5/4 veya 4/4 minör dağılımı (iki minörde toplam 9+ kart).
        /// </summary>
        public static bool IkiMinorDagilimi(List<Card> el, out int karo, out int sinek)
        {
            karo = RenkUzunlugu(el, "Karo");
            sinek = RenkUzunlugu(el, "Sinek");
            return karo >= 4 && sinek >= 4 && (karo + sinek) >= 9;
        }

        /// <summary>
        /// Belirli bir oyuncunun zonda olup olmadığını hesaplar (IhaleDurumu'ndan).
        /// NS oyuncuları: Kuzey/Güney. EW oyuncuları: Batı/Doğu.
        /// </summary>
        public static bool ZondaMi(bool zonNS, bool zonEW, Player oyuncu)
        {
            switch (oyuncu)
            {
                case Player.Kuzey:
                case Player.Guney:
                    return zonNS;
                case Player.Bati:
                case Player.Dogu:
                    return zonEW;
                default:
                    return false;
            }
        }
    }
}