using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class
{
    // ═══════════════════════════════════════════════════════════════════════
    // OYUN KURALLARI — el kazananı, yasal kartlar, oturma düzeni
    // BricOyna'daki SuAnkiTrickKazanani / ElKazananiniBelirle tek kaynak olarak
    // buradaki ElKazanani'yı kullanır.
    // ═══════════════════════════════════════════════════════════════════════
    public static class OyunKurallari
    {
        public static readonly string[] Renkler = { "Maça", "Kupa", "Sinek", "Karo" };

        public static Player Ortak(Player p)
        {
            switch (p)
            {
                case Player.Kuzey: return Player.Guney;
                case Player.Guney: return Player.Kuzey;
                case Player.Bati: return Player.Dogu;
                default: return Player.Bati;
            }
        }

        /// <summary>Saat yönü: Güney → Batı → Kuzey → Doğu → Güney.</summary>
        public static Player Sonraki(Player p)
        {
            switch (p)
            {
                case Player.Guney: return Player.Bati;
                case Player.Bati: return Player.Kuzey;
                case Player.Kuzey: return Player.Dogu;
                default: return Player.Guney;
            }
        }

        /// <summary>aday kart, o anki en iyi kartı geçer mi?</summary>
        public static bool KartiGecer(Card aday, Card mevcut, string koz)
        {
            if (aday.Suit == mevcut.Suit) return aday.Value > mevcut.Value;
            if (koz != null && aday.Suit == koz) return true;   // mevcut koz değil
            return false;
        }

        /// <summary>Masadaki kartlardan (oynanış sırasıyla) o an kazananı verir.</summary>
        public static (Player Oyuncu, Card Kart) ElKazanani(IList<(Player Oyuncu, Card Kart)> masa, string koz)
        {
            var en = masa[0];
            for (int i = 1; i < masa.Count; i++)
            {
                if (KartiGecer(masa[i].Kart, en.Kart, koz)) en = masa[i];
            }
            return en;
        }

        /// <summary>Renk takibi zorunluluğuna göre oynanabilecek kartlar.</summary>
        public static List<Card> LegalKartlar(List<Card> el, string atakRengi)
        {
            if (atakRengi == null) return new List<Card>(el);
            var ayni = el.Where(c => c.Suit == atakRengi).ToList();
            return ayni.Count > 0 ? ayni : new List<Card>(el);
        }
    }

    /// <summary>AI'ın karar verirken kullanabileceği bilgi (yalnızca meşru bilgi).</summary>
    public sealed class OyunBaglami
    {
        public Player Ben;
        public Player Deklaran;
        public string Koz;                                   // null = sanzatu
        public List<Card> Elim;                              // karar verdiğim elin kartları
        public List<Card> OrtakEli;                          // yalnız deklaran tarafı için: diğer el (deklaran↔dummy)
        public List<Card> DummyEli;                          // yalnız savunma için: açılmış dummy eli (atak sonrası)
        public List<(Player Oyuncu, Card Kart)> Masa = new List<(Player Oyuncu, Card Kart)>();
        public List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)> Gecmis
            = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>();
        public int ElNo;                                     // biten el sayısı (0 = açılış atağı eli)
        public List<string> IhaleGecmisi;                    // "1♠", "Pas", ...
        public string IhaleBaslangici;                       // "Bati" / "Kuzey" / "Dogu" / "Guney"
        public Func<Player, BricKartOyunu.Class.Bidding.OrtaklikAnlasmasi> AnlasmaOf; // koltuğun taraf anlaşması (yapay çağrıları yorumlamak için); null = hepsi doğal
        public int Seviye;                                   // kontrat seviyesi (1-7); 0 = bilinmiyor → hedef mantığı kapalı
    }

    // ═══════════════════════════════════════════════════════════════════════
    // OYUN ZEKASI
    // ═══════════════════════════════════════════════════════════════════════
    public static class OyunZekasi
    {
        /// <summary>Kontrat hedefi mantığı: deklaran / savunma tarafı için ayrı ayrı açılıp kapatılabilir.</summary>
        /// <summary>
        /// Beceri düzeyi 1-20 (20 = hatasız). Düşükse BeceriHatasi arada rastgele yasal kart seçer.
        /// Ortaklık anlaşmasındaki BeceriDuzeyi ile eşitlenir.
        /// </summary>
        public static int BeceriDuzeyi = 20;
        public static Random BeceriRnd = new Random();

        /// <summary>Hata olasılığı: (20 - beceri) × %1 (en fazla %19). Hata varsa rastgele yasal kart, yoksa null.</summary>
        public static Card BeceriHatasi(List<Card> legal)
        {
            if (legal == null || legal.Count < 2) return null;
            int b = Math.Max(1, Math.Min(20, BeceriDuzeyi));
            if (b >= 20) return null;
            if (BeceriRnd.NextDouble() >= (20 - b) * 0.01) return null;
            return legal[BeceriRnd.Next(legal.Count)];
        }

        /// <summary>
        /// Renk planlayıcı (Melih Özdil, "Kart Oyun Tekniği"): deklaran tarafı, empas / çift empas / düşürme gibi
        /// kombinasyonlar için dışardaki kartların olası dağılımlarını hesaplayıp en çok löve getirecek kartı seçer.
        /// Kapalıyken deklaran eski sezgisel kurallarla oynar.
        /// </summary>
        public static bool RenkPlanlayiciAktif = true;
        /// <summary>Planlayıcının bir renk için ek löve beklentisi bu eşiğin üstündeyse devreye girer.</summary>
        public static double RenkPlanKazancEsigi = 0.25;

        public static bool HedefDeklaran = true;
        public static bool HedefSavunma = true;
        /// <summary>true: yalnızca hedef tamamlanınca topla; false: "yeterli sağlam el var" tahminiyle de topla.</summary>
        public static bool YalnizcaTamamlaninca = true;

        // ───────────────────────────────────────────────────────────────────
        // GİRİŞ NOKTASI
        // ───────────────────────────────────────────────────────────────────
        public static Card KartSec(OyunBaglami b, out string neden)
        {
            neden = "";
            if (b == null || b.Elim == null || b.Elim.Count == 0) return null;

            string atak = (b.Masa != null && b.Masa.Count > 0) ? b.Masa[0].Kart.Suit : null;
            var legal = OyunKurallari.LegalKartlar(b.Elim, atak);

            if (legal.Count == 1)
            {
                neden = "Tek yasal kart";
                return legal[0];
            }

            var x = new Ctx(b);
            Card secilen;
            if (atak == null) secilen = Lider(x, out neden);
            else secilen = Takip(x, legal, atak, out neden);

            // Güvenlik ağı: her koşulda yasal kart döndür
            if (secilen == null || !legal.Contains(secilen))
            {
                secilen = legal.OrderBy(c => c.Value).First();
                neden = "Güvenlik ağı: en düşük yasal kart";
            }
            neden += x.HedefOzeti();
            return secilen;
        }

        // ───────────────────────────────────────────────────────────────────
        // BAĞLAM — görünmeyen kartlar, ihale bilgisi
        // ───────────────────────────────────────────────────────────────────
        private sealed class Ctx
        {
            public OyunBaglami B;
            public string Koz;
            public Player Ben, Ortak, Dummy;
            public bool DeklaranTarafi;
            public HashSet<string> Gorunmeyen = new HashSet<string>();
            public HashSet<string> OrtakRenkleri = new HashSet<string>();
            public HashSet<string> RakipRenkleri = new HashSet<string>();

            // ── Kontrat hedefi ──
            public bool HedefVar;
            public int DeklaranHedef;       // 6 + seviye
            public int SavunmaHedef;        // 8 − seviye (kontratı batırmak için)
            public int DeklaranEl, SavunmaEl;
            public int Gerek => DeklaranTarafi ? DeklaranHedef - DeklaranEl : SavunmaHedef - SavunmaEl;
            public int KalanEl => 13 - B.ElNo;

            public Ctx(OyunBaglami b)
            {
                B = b;
                Koz = b.Koz;
                Ben = b.Ben;
                Ortak = OyunKurallari.Ortak(b.Ben);
                Dummy = OyunKurallari.Ortak(b.Deklaran);
                DeklaranTarafi = (b.Ben == b.Deklaran || b.Ben == Dummy);

                // Görünmeyen kartlar = 52 − (elim + bilinen eller + masa + geçmiş)
                foreach (var s in OyunKurallari.Renkler)
                    for (int v = 2; v <= 14; v++)
                        Gorunmeyen.Add(s + "|" + v);

                Cikar(b.Elim);
                Cikar(b.OrtakEli);
                Cikar(b.DummyEli);
                if (b.Masa != null) foreach (var m in b.Masa) Cikar(m.Kart);
                if (b.Gecmis != null) foreach (var g in b.Gecmis) Cikar(g.Kart);

                IhaleRenkleriniCikar();
                HedefiHesapla();
            }

            private void HedefiHesapla()
            {
                if (B.Seviye < 1 || B.Seviye > 7) return;
                HedefVar = true;
                DeklaranHedef = 6 + B.Seviye;
                SavunmaHedef = 8 - B.Seviye;

                // Biten ellerin kazananı: Kazandi = true olan kayıt
                foreach (var el in B.Gecmis.Where(g => g.Kazandi))
                {
                    if (el.Oyuncu == B.Deklaran || el.Oyuncu == Dummy) DeklaranEl++;
                    else SavunmaEl++;
                }
            }

            public string HedefOzeti()
            {
                if (!HedefVar) return "";
                return DeklaranTarafi
                    ? $" [hedef: {DeklaranEl}/{DeklaranHedef} el]"
                    : $" [hedef: kontratı batır {SavunmaEl}/{SavunmaHedef} el]";
            }

            /// <summary>
            /// Üstten başlayarak kesintisiz bizim olan (ya da oynanmış) kart sayısı;
            /// rakipte olabilecek ilk kartta durur. Yalnızca verilen eller sayılır.
            /// </summary>
            public int UstSaglam(string suit, IEnumerable<Card> a, IEnumerable<Card> b)
            {
                int n = 0;
                for (int v = 14; v >= 2; v--)
                {
                    if (Gorunmeyen.Contains(suit + "|" + v)) break;
                    bool bizde = (a != null && a.Any(c => c.Suit == suit && c.Value == v)) ||
                                 (b != null && b.Any(c => c.Suit == suit && c.Value == v));
                    if (bizde) n++;
                }
                return n;
            }

            /// <summary>
            /// Güvenle toplanabilecek el sayısı (muhafazakâr tahmin).
            /// Renkli kontratta rakipte koz kaldıysa yan renklerden en çok 1 (As) sayılır:
            /// kesilme riski. Koz renginde üstten sağlam kartlar sayılır.
            /// </summary>
            public int GuvenliEl()
            {
                var a = B.Elim;
                var b = DeklaranTarafi ? B.OrtakEli : null;   // savunmada ortağın eli bilinmez
                int toplam = 0;
                bool rakipKozu = Koz != null && GorunmeyenSayi(Koz) > 0;

                foreach (var suit in OyunKurallari.Renkler)
                {
                    int uzA = a.Count(c => c.Suit == suit);
                    int uzB = b == null ? 0 : b.Count(c => c.Suit == suit);
                    int uzun = Math.Max(uzA, uzB);
                    if (uzun == 0) continue;

                    int w = Math.Min(UstSaglam(suit, a, b), uzun);
                    if (rakipKozu && suit != Koz) w = Math.Min(w, 1);
                    toplam += w;
                }
                return Math.Min(toplam, KalanEl);
            }

            private void Cikar(Card c) { if (c != null) Gorunmeyen.Remove(c.Suit + "|" + c.Value); }
            private void Cikar(IEnumerable<Card> l) { if (l != null) foreach (var c in l) Cikar(c); }

            public int GorunmeyenSayi(string suit)
            {
                int n = 0;
                for (int v = 2; v <= 14; v++) if (Gorunmeyen.Contains(suit + "|" + v)) n++;
                return n;
            }

            /// <summary>Kartın üstünde görünmeyen (rakip elinde olabilecek) kart var mı?</summary>
            public bool Boss(Card c)
            {
                for (int v = c.Value + 1; v <= 14; v++)
                    if (Gorunmeyen.Contains(c.Suit + "|" + v)) return false;
                return true;
            }

            public int UsttekiGorunmeyen(Card c)
            {
                int n = 0;
                for (int v = c.Value + 1; v <= 14; v++)
                    if (Gorunmeyen.Contains(c.Suit + "|" + v)) n++;
                return n;
            }

            public bool AradaGorunmeyenVar(string suit, int alt, int ust)
            {
                for (int v = alt + 1; v < ust; v++)
                    if (Gorunmeyen.Contains(suit + "|" + v)) return true;
                return false;
            }

            private void IhaleRenkleriniCikar()
            {
                if (B.IhaleGecmisi == null || string.IsNullOrEmpty(B.IhaleBaslangici)) return;

                Player baslangic;
                if (!Enum.TryParse(B.IhaleBaslangici, out baslangic)) return;

                // İhale sırası saat yönünde: Batı → Kuzey → Doğu → Güney
                Player[] sira = { Player.Bati, Player.Kuzey, Player.Dogu, Player.Guney };
                int bas = Array.IndexOf(sira, baslangic);
                if (bas < 0) return;

                for (int i = 0; i < B.IhaleGecmisi.Count; i++)
                {
                    string t = B.IhaleGecmisi[i];
                    if (string.IsNullOrEmpty(t) || t == "Pas") continue;

                    // Yapay (konvansiyon) çağrı: söylenen renk değil, gerçekte gösterilen renkler sayılır
                    var yorum = BricKartOyunu.Class.Bidding.IhaleYorumcusu.Yorumla(B.IhaleGecmisi, B.IhaleBaslangici, i, B.AnlasmaOf);
                    if (yorum != null)
                    {
                        Player ykoltuk = sira[(bas + i) % 4];
                        if (ykoltuk == Ben) continue;
                        foreach (var yr in yorum)
                        {
                            if (ykoltuk == Ortak) OrtakRenkleri.Add(yr);
                            else RakipRenkleri.Add(yr);
                        }
                        continue;
                    }

                    if (t == "Dbl" || t == "RDbl" || t.EndsWith("NT")) continue;

                    string renk = SembolRenk(t[t.Length - 1]);
                    if (renk == null) continue;

                    Player koltuk = sira[(bas + i) % 4];
                    if (koltuk == Ben) continue;
                    if (koltuk == Ortak) OrtakRenkleri.Add(renk);
                    else RakipRenkleri.Add(renk);
                }
            }

            private static string SembolRenk(char ch)
            {
                switch (ch)
                {
                    case '♠': return "Maça";
                    case '♥': return "Kupa";
                    case '♦': return "Karo";
                    case '♣': return "Sinek";
                    default: return null;
                }
            }
        }

        // ───────────────────────────────────────────────────────────────────
        // YARDIMCILAR
        // ───────────────────────────────────────────────────────────────────
        private static List<Card> RenkKartlari(IEnumerable<Card> el, string suit)
        {
            return el.Where(c => c.Suit == suit).OrderByDescending(c => c.Value).ToList();
        }

        private static int HcpPuan(Card c)
        {
            return c.Value >= 11 ? c.Value - 10 : 0;
        }

        /// <summary>
        /// Büyükten küçüğe sıralı aday kartlardan, en üsttekiyle "eşdeğer" (arasında
        /// görünmeyen kart olmayan) kartların en alçağını verir. Örn. KQJ → J.
        /// </summary>
        private static Card UstEsdegerinEnAlcagi(Ctx x, List<Card> adaylarDesc)
        {
            var cur = adaylarDesc[0];
            for (int i = 1; i < adaylarDesc.Count; i++)
            {
                var next = adaylarDesc[i];
                if (x.AradaGorunmeyenVar(cur.Suit, next.Value, cur.Value)) break;
                cur = next;
            }
            return cur;
        }

        // ═══════════════════════════════════════════════════════════════════
        // ATAK (el başlatma)
        // ═══════════════════════════════════════════════════════════════════
        private static Card Lider(Ctx x, out string neden)
        {
            if (x.B.ElNo == 0)
            {
                // Açılış atağı: dummy henüz görünmez
                return YeniRenkAtagi(x, false, out neden, "Açılış atağı");
            }
            return x.DeklaranTarafi ? DeklaranLideri(x, out neden) : SavunmaLideri(x, out neden);
        }

        /// <summary>
        /// Puanlama ile renk seçer. AtakKurallari (As/Rua/Dam atağı) kural kapsamındaki
        /// renkleri öne çıkarır. Açılış atağında ve savunmada yeni renk açarken kullanılır.
        /// </summary>
        private static Card YeniRenkAtagi(Ctx x, bool dummyBilinir, out string neden, string etiket)
        {
            var el = x.B.Elim;
            bool nt = x.Koz == null;
            int kozSayim = nt ? 0 : el.Count(c => c.Suit == x.Koz);

            Card enIyiKart = null;
            double enIyiPuan = double.MinValue;
            string enIyiNeden = "";

            foreach (var suit in OyunKurallari.Renkler)
            {
                var c = RenkKartlari(el, suit);
                if (c.Count == 0) continue;

                int n = c.Count;
                var kural = AtakKurallari.RenkAtagi(c);
                bool kozRengi = !nt && suit == x.Koz;
                bool ortakRengi = x.OrtakRenkleri.Contains(suit);
                bool rakipRengi = x.RakipRenkleri.Contains(suit);
                int pts = c.Sum(k => HcpPuan(k));
                bool A = c[0].Value == 14;
                bool K = c.Any(k => k.Value == 13);
                bool Q = c.Any(k => k.Value == 12);

                double s;
                if (nt)
                {
                    s = n * 10 + pts * 2;
                    if (n == 1) s -= 30; else if (n == 2) s -= 10; else if (n == 3) s -= 4;
                    if (kural != null) s += 25;
                }
                else if (kozRengi)
                {
                    s = -15 + (kural != null ? 20 : 0);          // koz atağı pasif seçenek
                }
                else if (n == 1)
                {
                    s = (kozSayim >= 2 && !A) ? 40 : 10;          // tekli: kesme umudu
                }
                else if (n == 2)
                {
                    s = (kozSayim >= 3 && !A && !K) ? 30 : 4;     // ikili: kesme umudu
                }
                else
                {
                    s = n * 6 + pts;
                    if (kural != null) s += 35;
                    else if (A && !K) s -= 20;                     // desteksiz As atılmaz
                    else if ((K || Q) && !A) s -= 8;               // onör altından atak
                }

                if (ortakRengi) s += 25;
                if (rakipRengi) s -= nt ? 10 : 18;
                if (dummyBilinir && !nt && !kozRengi && x.B.DummyEli != null &&
                    !x.B.DummyEli.Any(k => k.Suit == suit))
                    s -= 15;                                       // dummy'de boş: kesilir

                if (s > enIyiPuan || (s == enIyiPuan && enIyiKart != null && n > RenkKartlari(el, enIyiKart.Suit).Count))
                {
                    enIyiPuan = s;
                    enIyiKart = AtakKarti(c, nt, kural, ortakRengi);
                    enIyiNeden = $"{etiket}: {suit} ({n} kart)" +
                                 (kural != null ? " — " + kural.Kural : "") +
                                 (ortakRengi ? " — ortağın rengi" : "");
                }
            }

            neden = enIyiNeden;
            return enIyiKart;
        }

        /// <summary>Seçilen renkten hangi kartın atılacağı.</summary>
        private static Card AtakKarti(List<Card> c, bool nt, AtakKurallari.AtakOnerisi kural, bool ortakRengi)
        {
            if (kural != null) return kural.Kart;
            int n = c.Count;
            if (n <= 2) return c[0];                       // tekli / ikili: üstten
            if (n == 3)
            {
                bool onor = c[0].Value >= 11;
                if (onor)
                {
                    if (c[0].Value == 14 && !nt && !ortakRengi) return c[0];
                    return c[2];                           // onörlü üçlü: en küçük
                }
                return c[0];                               // üç küçük: üstten
            }
            return c[3];                                   // dördüncü en iyi
        }

        // ───────────────────────────────────────────────────────────────────
        // DEKLARAN TARAFI LİDERİ (deklaran ve dummy)
        // ───────────────────────────────────────────────────────────────────
        private static Card DeklaranLideri(Ctx x, out string neden)
        {
            var el = x.B.Elim;
            var ortak = x.B.OrtakEli ?? new List<Card>();
            string koz = x.Koz;

            // 0) HEDEF: toplanabilir el sayısı gereken eli karşılıyorsa riske girmeden topla
            if (x.HedefVar && HedefDeklaran)
            {
                int gerek = x.Gerek;
                if (gerek <= 0 || (!YalnizcaTamamlaninca && x.GuvenliEl() >= gerek))
                {
                    var topla = SaglamKartiTopla(x, el, true);
                    if (topla != null)
                    {
                        neden = gerek <= 0
                            ? "Kontrat tamam: sağlam eller toplanıyor"
                            : $"Hedef güvende ({gerek} el gerek, {x.GuvenliEl()} sağlam): toplanıyor";
                        return topla;
                    }
                }
            }

            // 1) Koz çek
            if (koz != null)
            {
                var kozum = RenkKartlari(el, koz);
                int kozOrtak = ortak.Count(c => c.Suit == koz);
                int rakipKoz = x.GorunmeyenSayi(koz);
                if (rakipKoz > 0 && kozum.Count > 0 && kozum.Count + kozOrtak > rakipKoz)
                {
                    var boss = kozum.FirstOrDefault(c => x.Boss(c));
                    neden = "Koz çekiliyor";
                    return boss ?? kozum[0];
                }
            }

            // 2) Sağlam (boss) kartları bozdur
            bool uzunOnce = false;
            if (koz == null && x.HedefVar)
            {
                int gerek2 = x.Gerek;
                if (x.GuvenliEl() < gerek2)
                {
                    var uz = OyunKurallari.Renkler
                        .Where(sv => el.Any(c => c.Suit == sv) &&
                                     el.Count(c => c.Suit == sv) + ortak.Count(c => c.Suit == sv) >= 7)
                        .FirstOrDefault();
                    if (uz != null) uzunOnce = true;
                }
            }
            var bossGrup = el.Where(c => c.Suit != koz && x.Boss(c))
                             .GroupBy(c => c.Suit)
                             .OrderByDescending(g => RenkKartlari(el, g.Key).Count + ortak.Count(c => c.Suit == g.Key))
                             .FirstOrDefault();
            if (bossGrup != null && !uzunOnce)
            {
                var enUst = bossGrup.OrderByDescending(c => c.Value).First();
                // Karşı elde (dummy) bu kartın üstünde bir kart varsa ve kısa ise (tek/çift),
                // üst kartı oynarsak dummy'nin sağlamı aynı ele yanar → alçak kart oyna.
                var ortakSuit = ortak.Where(c => c.Suit == bossGrup.Key).ToList();
                var elSuit = RenkKartlari(el, bossGrup.Key);
                if (ortakSuit.Count > 0 && ortakSuit.Count <= 2 && elSuit.Count > 1 &&
                    ortakSuit.Any(c => c.Value > enUst.Value))
                {
                    neden = "Dummy'nin sağlam kartı yanmasın: alçak kart (" + bossGrup.Key + ")";
                    return elSuit.Last();
                }
                neden = "Sağlam el bozduruluyor: " + bossGrup.Key;
                return enUst;
            }

            // 2b) Renk planlayıcı: empas / çift empas / düşürme için en iyi kart (sağlam kartlar toplandıktan sonra)
            if (RenkPlanlayiciAktif)
            {
                string planNeden;
                Card planKart = null;
                try { planKart = RenkPlaniKarti(x, out planNeden); }
                catch (Exception) { planNeden = ""; planKart = null; }   // planlayıcı hata verirse eski kurallara dön
                if (planKart != null && el.Contains(planKart))
                {
                    neden = planNeden;
                    return planKart;
                }
            }

            // 3) Ortağın sağlam kartına doğru alçak oyna
            var ortakBoss = ortak.Where(c => c.Suit != koz && x.Boss(c) &&
                                             el.Any(e => e.Suit == c.Suit) &&
                                             !(koz == null && el.Count(e => e.Suit == c.Suit) + ortak.Count(e => e.Suit == c.Suit) <= 5 && x.GorunmeyenSayi(c.Suit) >= 7))
                                 .GroupBy(c => c.Suit)
                                 .OrderByDescending(g => g.Count())
                                 .FirstOrDefault();
            if (ortakBoss != null)
            {
                neden = "Ortağın sağlam kartına doğru: " + ortakBoss.Key;
                return RenkKartlari(el, ortakBoss.Key).Last();
            }

            // 4) Birleşik en uzun renkten alçak kart
            var uzun = OyunKurallari.Renkler
                .Where(s => s != koz && el.Any(c => c.Suit == s))
                .OrderByDescending(s => el.Count(c => c.Suit == s) + ortak.Count(c => c.Suit == s))
                .FirstOrDefault();
            if (uzun != null)
            {
                neden = "Uzun renk geliştiriliyor: " + uzun;
                return RenkKartlari(el, uzun).Last();
            }

            neden = "Elde yalnız koz kaldı";
            return el.OrderBy(c => c.Value).First();
        }

        // ───────────────────────────────────────────────────────────────────
        // RENK PLANLAYICI BAĞLANTISI
        // ───────────────────────────────────────────────────────────────────

        /// <summary>Geçmişte rengi takip edemeyen (renksiz kalmış) oyuncular: oyuncu → renkler.</summary>
        private static Dictionary<Player, HashSet<string>> RenksizRakipler(OyunBaglami b)
        {
            var d = new Dictionary<Player, HashSet<string>>();
            if (b.Gecmis == null) return d;
            foreach (var grup in b.Gecmis.GroupBy(g => g.ElNo))
            {
                var l = grup.ToList();
                if (l.Count == 0) continue;
                string atak = l[0].Kart.Suit;
                foreach (var e in l)
                {
                    if (e.Kart.Suit == atak) continue;
                    HashSet<string> set;
                    if (!d.TryGetValue(e.Oyuncu, out set)) { set = new HashSet<string>(); d[e.Oyuncu] = set; }
                    set.Add(atak);
                }
            }
            return d;
        }

        private static int[] Degerler(List<Card> kartlar)
        {
            return kartlar.Select(c => c.Value).ToArray();
        }

        /// <summary>
        /// Deklaran tarafı (sıra bende): her renk için renk planlayıcıyı çalıştırır; en çok ek löve getiren rengin
        /// en iyi kartını verir. Yalnız sanzatu ya da rakipte koz kalmadığında (kesme riski yok) yan renklerde,
        /// koz renginde ise her zaman çalışır. Giriş (entry) sorunu çözücüde yoktur: iki elde de giriş varsayılır.
        /// </summary>
        private static Card RenkPlaniKarti(Ctx x, out string neden)
        {
            neden = "";
            var el = x.B.Elim;
            var ortak = x.B.OrtakEli;
            if (el == null || ortak == null) return null;
            int kalan = 13 - x.B.ElNo;
            if (kalan < 2) return null;

            Player sol = OyunKurallari.Sonraki(x.Ben);
            Player sag = OyunKurallari.Sonraki(OyunKurallari.Sonraki(sol));
            var renksiz = RenksizRakipler(x.B);
            bool rakipKozu = x.Koz != null && x.GorunmeyenSayi(x.Koz) > 0;
            int guvenli = x.HedefVar ? x.GuvenliEl() : 0;

            Card enKart = null;
            double enKazanc = RenkPlanKazancEsigi;
            string enNeden = "";

            foreach (var suit in OyunKurallari.Renkler)
            {
                if (rakipKozu && suit != x.Koz) continue;     // kesilme riski: planlayıcı kesmeyi bilmez
                var benim = RenkKartlari(el, suit);
                if (benim.Count == 0) continue;               // bu rengi ben başlatamam
                var ort = RenkKartlari(ortak, suit);
                var dis = new List<int>();
                for (int v = 2; v <= 14; v++) if (x.Gorunmeyen.Contains(suit + "|" + v)) dis.Add(v);
                if (dis.Count == 0 || dis.Count > RenkPlanlayici.EnFazlaDisariKart) continue;

                int sagl = Math.Min(x.UstSaglam(suit, el, ortak), Math.Max(benim.Count, ort.Count));
                int ihtiyac = 0;
                if (x.HedefVar && x.Gerek > 0) ihtiyac = Math.Max(0, x.Gerek - (guvenli - sagl));

                HashSet<string> sx, sy;
                bool xBos = renksiz.TryGetValue(sol, out sx) && sx.Contains(suit);
                bool yBos = renksiz.TryGetValue(sag, out sy) && sy.Contains(suit);

                var sonuc = RenkPlanlayici.Coz(Degerler(benim), Degerler(ort), dis.ToArray(), kalan, kalan, xBos, yBos, ihtiyac);
                if (!sonuc.Basarili) continue;

                double kazanc = sonuc.BeklenenLove - sagl;
                if (kazanc <= enKazanc) continue;

                // Sıra kartları: arada başka kart yoksa en alçak denk kartı oyna (alçak kart ortağın onörüne doğru gider)
                int rank = sonuc.Kart;
                if (rank <= 9)
                {
                    var tum = new HashSet<int>(dis);
                    foreach (var c in ort) tum.Add(c.Value);
                    int en = rank;
                    foreach (var c in benim.OrderByDescending(k => k.Value))
                    {
                        if (c.Value >= rank) continue;
                        bool arada = false;
                        for (int v = c.Value + 1; v < en; v++) if (tum.Contains(v)) { arada = true; break; }
                        if (arada) break;
                        en = c.Value;
                    }
                    rank = en;
                }
                var kart = benim.FirstOrDefault(c => c.Value == rank);
                if (kart == null) continue;

                enKazanc = kazanc;
                enKart = kart;
                enNeden = "Renk planı (" + suit + "): beklenen " + sonuc.BeklenenLove.ToString("0.0") + " löve" +
                          (ihtiyac > 0 ? ", " + ihtiyac + " löve için %" + (sonuc.YapmaSansi * 100).ToString("0") : "");
            }

            if (enKart == null) return null;
            neden = enNeden;
            return enKart;
        }

        // ───────────────────────────────────────────────────────────────────
        // SAVUNMA LİDERİ
        // ───────────────────────────────────────────────────────────────────
        private static Card SavunmaLideri(Ctx x, out string neden)
        {
            var el = x.B.Elim;
            string koz = x.Koz;

            // Geçmiş ataklardan ortağın ve benim attığım renkler (en yenisi başta)
            var ortakAtaklari = new List<string>();
            var benimAtaklarim = new List<string>();
            foreach (var grup in x.B.Gecmis.GroupBy(g => g.ElNo).OrderByDescending(g => g.Key))
            {
                var lider = grup.First();
                if (lider.Oyuncu == x.Ortak && !ortakAtaklari.Contains(lider.Kart.Suit))
                    ortakAtaklari.Add(lider.Kart.Suit);
                else if (lider.Oyuncu == x.Ben && !benimAtaklarim.Contains(lider.Kart.Suit))
                    benimAtaklarim.Add(lider.Kart.Suit);
            }

            // 0) HEDEF: kontratı batırmaya yetecek sağlam el varsa hemen topla
            if (x.HedefVar && HedefSavunma)
            {
                int gerek = x.Gerek;
                if (gerek <= 0 || (!YalnizcaTamamlaninca && x.GuvenliEl() >= gerek))
                {
                    var topla = SaglamKartiTopla(x, el, false);
                    if (topla != null)
                    {
                        neden = gerek <= 0
                            ? "Kontrat battı: sağlam eller toplanıyor"
                            : $"Kontrat batırılabilir ({gerek} el gerek, {x.GuvenliEl()} sağlam): hemen toplanıyor";
                        return topla;
                    }
                }
            }

            // A) Sağlam kartı bozdur (koz kontratında yan renkler; sanzatuda atak renkleri)
            var bossAday = el.Where(c => c.Suit != koz && x.Boss(c) &&
                                         (koz != null || ortakAtaklari.Contains(c.Suit) || benimAtaklarim.Contains(c.Suit)))
                             .OrderByDescending(c => RenkKartlari(el, c.Suit).Count)
                             .ThenByDescending(c => c.Value)
                             .FirstOrDefault();
            if (bossAday != null)
            {
                neden = "Sağlam el bozduruluyor: " + bossAday.Suit;
                return bossAday;
            }

            // B) Ortağın rengini geri oyna
            foreach (var s in ortakAtaklari)
            {
                var c = RenkKartlari(el, s);
                if (c.Count == 0) continue;
                neden = "Ortağın rengi geri oynanıyor: " + s;
                return GeriDonusKarti(x, c);
            }

            // C) Kendi atak rengime devam
            foreach (var s in benimAtaklarim)
            {
                var c = RenkKartlari(el, s);
                if (c.Count == 0) continue;
                neden = "Atak rengine devam: " + s;
                return GeriDonusKarti(x, c);
            }

            // D) Yeni renk (dummy görünür)
            return YeniRenkAtagi(x, true, out neden, "Savunma yeni renk");
        }

        /// <summary>
        /// Elimizdeki sağlam (boss) kartlardan, en çok sağlam eli olan renkten en yükseğini seçer.
        /// Renkli kontratta rakipte koz varsa önce koz sağlamları, sonra yalnız As'lar (kesilme riski).
        /// </summary>
        private static Card SaglamKartiTopla(Ctx x, List<Card> el, bool deklaranTarafi)
        {
            var ortak = deklaranTarafi ? x.B.OrtakEli : null;
            bool rakipKozu = x.Koz != null && x.GorunmeyenSayi(x.Koz) > 0;

            // Rakipte koz kaldıysa ve sağlam kozumuz varsa önce kozu çek
            if (rakipKozu)
            {
                var kozBoss = el.Where(c => c.Suit == x.Koz && x.Boss(c)).OrderByDescending(c => c.Value).FirstOrDefault();
                if (kozBoss != null) return kozBoss;
            }

            Card enIyi = null;
            int enIyiSayi = -1;
            foreach (var suit in OyunKurallari.Renkler)
            {
                var bossKart = el.Where(c => c.Suit == suit && x.Boss(c)).OrderByDescending(c => c.Value).FirstOrDefault();
                if (bossKart == null) continue;
                if (rakipKozu && suit != x.Koz && bossKart.Value != 14) continue;   // kesilebilir

                int w = x.UstSaglam(suit, el, ortak);
                if (w > enIyiSayi) { enIyiSayi = w; enIyi = bossKart; }
            }
            return enIyi;
        }

        private static Card GeriDonusKarti(Ctx x, List<Card> c)
        {
            if (c.Count == 1) return c[0];
            if (c.Count == 2) return c[0];                 // ikili kaldıysa üstten
            if (x.Boss(c[0])) return c[0];
            return c[c.Count - 1];                         // aksi halde en alçak
        }

        // ═══════════════════════════════════════════════════════════════════
        // TAKİP (başkası başlattı)
        // ═══════════════════════════════════════════════════════════════════
        private static Card Takip(Ctx x, List<Card> legal, string atak, out string neden)
        {
            var masa = x.B.Masa;
            int poz = masa.Count;                          // 1: 2. el, 2: 3. el, 3: 4. el
            var kazanan = OyunKurallari.ElKazanani(masa, x.Koz);
            bool ortakKazaniyor = kazanan.Oyuncu == x.Ortak;
            var best = kazanan.Kart;
            bool renkVar = legal.Any(c => c.Suit == atak);

            // ───────── RENK VAR ─────────
            if (renkVar)
            {
                var my = legal.OrderByDescending(c => c.Value).ToList();
                var en_alcak = my[my.Count - 1];

                if (x.Koz != null && atak != x.Koz && best.Suit == x.Koz)
                {
                    neden = "El kesildi, kazanamam → en alçak";
                    return en_alcak;
                }

                var kazananlar = my.Where(c => c.Value > best.Value).ToList();

                if (poz == 3)                              // dördüncü el
                {
                    if (ortakKazaniyor) { neden = "4. el: ortak kazanıyor → en alçak"; return en_alcak; }
                    if (kazananlar.Count == 0) { neden = "4. el: kazanamam → en alçak"; return en_alcak; }
                    neden = "4. el: kazanacak en alçak";
                    return kazananlar[kazananlar.Count - 1];
                }

                if (poz == 2)                              // üçüncü el
                {
                    if (ortakKazaniyor)
                    {
                        if (x.UsttekiGorunmeyen(best) == 0)
                        {
                            neden = "3. el: ortağın kartı geçilemez → en alçak";
                            return en_alcak;
                        }
                        if (kazananlar.Count > 0)
                        {
                            neden = "3. el yüksek: ortağı destekle";
                            return UstEsdegerinEnAlcagi(x, kazananlar);
                        }
                        neden = "3. el: ortağı geçemem → en alçak";
                        return en_alcak;
                    }
                    if (kazananlar.Count > 0)
                    {
                        neden = "3. el yüksek";
                        return UstEsdegerinEnAlcagi(x, kazananlar);
                    }
                    neden = "3. el: kazanamam → en alçak";
                    return en_alcak;
                }

                // poz == 1 → ikinci el: alçak, ama onöre sağlam kartla cevap ver
                if (best.Value >= 12)
                {
                    var bossler = kazananlar.Where(c => x.Boss(c)).ToList();
                    if (bossler.Count > 0)
                    {
                        neden = "2. el: onör sağlam kartla alınıyor";
                        return bossler[bossler.Count - 1];
                    }
                }
                neden = "2. el alçak";
                return en_alcak;
            }

            // ───────── RENK YOK ─────────
            var el = legal;
            var kozlar = x.Koz != null
                ? el.Where(c => c.Suit == x.Koz).OrderBy(c => c.Value).ToList()
                : new List<Card>();

            // Ortak kazanıyor görünse bile, arkada oynayacak rakip ortağın kartını geçebilir.
            // Üçüncü elde (ortak açtı/kazanıyor, sonra yalnız bir rakip kaldı) ve ortağın kartı
            // sağlam değilse kozla çakmak eli kurtarır: ortağın kartının üstünde görünmeyen
            // en az 2 kart varsa (rakip büyük ihtimalle birini taşır) çak.
            bool ortagiKurtarmakIcinKes = false;
            if (kozlar.Count > 0 && ortakKazaniyor && poz == 2 &&
                x.Koz != null && best.Suit != x.Koz && atak != x.Koz)
            {
                int ustte = x.UsttekiGorunmeyen(best);
                bool pahaliKoz = kozlar[0].Value >= 12 && ustte < 3;   // Dam/üstü kozu ucuza harcama
                int esik = kozlar[0].Value <= 10 ? 1 : 2;   // ucuz kozla tek büyük kart bile yeter
                if (ustte >= esik && !pahaliKoz) ortagiKurtarmakIcinKes = true;
            }

            if (kozlar.Count > 0 && (!ortakKazaniyor || ortagiKurtarmakIcinKes))
            {
                var kes = best.Suit == x.Koz ? kozlar.Where(c => c.Value > best.Value).ToList() : kozlar;
                if (kes.Count > 0)
                {
                    neden = ortagiKurtarmakIcinKes
                        ? "Ortağın kartı sağlam değil, arkadaki rakip geçebilir → koz ile kesiliyor"
                        : "Koz ile kesiliyor (en alçak kazanan koz)";
                    return kes[0];
                }
            }

            return AtmaKarti(x, el, out neden, ortakKazaniyor);
        }

        // ───────────────────────────────────────────────────────────────────
        // ATMA (renk yok, kesmiyor)
        // ───────────────────────────────────────────────────────────────────
        private static Card AtmaKarti(Ctx x, List<Card> el, out string neden, bool ortakKazaniyor)
        {
            var yanRenkler = el.Where(c => c.Suit != x.Koz).ToList();
            if (yanRenkler.Count == 0)
            {
                neden = "Yalnız koz kaldı → en alçak koz";
                return el.OrderBy(c => c.Value).First();
            }

            Card secilen = null;
            double enIyi = double.MinValue;
            foreach (var g in yanRenkler.GroupBy(c => c.Suit))
            {
                var c = g.OrderByDescending(k => k.Value).ToList();
                int n = c.Count;
                double s = n;                                         // uzun renkten at
                if (c.Any(k => x.Boss(k))) s -= 30;                   // sağlam kartı koru
                if ((c.Any(k => k.Value == 13) || c.Any(k => k.Value == 12)) && n <= 3) s -= 12;  // onör koruması
                s -= c.Count(k => k.Value >= 11) * 3;
                s -= c[n - 1].Value * 0.5;                            // küçük kartı atmayı tercih et

                if (x.B.DummyEli != null && x.B.DummyEli.Count(k => k.Suit == g.Key) >= 4 && n <= 3)
                    s -= 12;                                          // dummy'nin uzun rengini açıkta bırakma

                if (s > enIyi) { enIyi = s; secilen = c[n - 1]; }
            }

            neden = "Atma: " + secilen.Suit + " renginden en alçak";
            return secilen;
        }
    }
}
