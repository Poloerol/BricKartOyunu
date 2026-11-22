using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Plan
{
    /// <summary>Savunma yardımcısının girdisi (savunmacı gözüyle).</summary>
    public sealed class SavunmaGirdisi
    {
        public string Kontrat = "";
        public string Koz;                          // null = sanzatu
        public int Seviye;
        public Player Deklaran, Dummy, Ben, Ortak;
        public List<Card> Elim = new List<Card>();              // kalan kartlarım
        public List<Card> DummyEli = new List<Card>();          // dummy açıldıysa kalan kartları
        public bool DummyAcik;
        public List<(Player Oyuncu, Card Kart)> Masa = new List<(Player Oyuncu, Card Kart)>();
        public List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)> Gecmis = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>();
        public List<string> IhaleGecmisi = new List<string>();
        public Player IhaleBaslangici;
        public Func<Player, string> Ad = p => p.ToString();
        public OrtaklikAnlasmasi Anlasma;                        // kendi tarafımızın anlaşması
        public OyunBaglami Baglam;                               // sıra bendeyse motor önerisi için
    }

    public sealed class AtakSatiri
    {
        public string Renk, Sembol;
        public string Kartlar = "";
        public Card Kart;                  // önerilen atak kartı
        public string KartYazi = "";
        public string Kural = "";          // "Rua atağı" / "Dördüncü en iyi" ...
        public string Gosterir = "";       // ortağa verilen mesaj
        public double Puan;
        public string Notlar = "";
    }

    public sealed class SavunmaSonucu
    {
        public bool Gecerli;
        public string Hata = "";
        public bool AtakKarari;            // true: açılış atağı henüz yapılmadı ve sıra bende
        public string Ozet = "";
        public int Hedef, Kazanilan, Gerek;
        public List<AtakSatiri> Atak = new List<AtakSatiri>();
        public string Onerilen = "";       // kural/puan tabanlı atak önerisi (örn. "♠K")
        public string OnerilenSembol = "";
        public string Motor = "";          // motor önerisi (kart + neden)
        public string MotorKart = "";
        public List<string> Ilkeler = new List<string>();
        public List<string> Plan = new List<string>();
        public List<string> Sinyaller = new List<string>();
        public List<string> Rakipler = new List<string>();
        public List<TakipSatiri> Takip = new List<TakipSatiri>();
    }

    /// <summary>
    /// Savunma yardımcısı: açılış atağı seçimi (ihale + el + kontrat), atak sonrası savunma planı (dummy analizi,
    /// kesme tehdidi, uzun renk/giriş), "onbir kuralı", ortağın kart ve atma sinyallerinin yorumu.
    /// Kural kaynağı: AtakKurallari (Özdil), SinyalKurallari ve takımın işaretleşme anlaşması.
    /// </summary>
    public static class SavunmaPlanAnalizi
    {
        private static readonly string[] Renkler = { "Maça", "Kupa", "Karo", "Sinek" };

        private static string Sem(string r) { return OyunPlanAnalizi.Sembol(r); }
        private static string Rk(int v) { return OyunPlanAnalizi.Sira(v); }
        private static string KartYazisi(Card c) { return Sem(c.Suit) + Rk(c.Value); }
        private static List<int> Deger(List<Card> el, string renk)
        {
            return el.Where(c => c.Suit == renk).Select(c => c.Value).OrderByDescending(v => v).ToList();
        }
        private static string Dizi(IEnumerable<int> d)
        {
            var l = d.OrderByDescending(v => v).Select(Rk).ToList();
            return l.Count == 0 ? "—" : string.Join(" ", l);
        }

        public static SavunmaSonucu Coz(SavunmaGirdisi g)
        {
            var s = new SavunmaSonucu();
            if (g == null || g.Elim == null || g.Elim.Count == 0) { s.Hata = "Elim hazır değil."; return s; }

            bool nt = g.Koz == null;
            Player lider = OyunKurallari.Sonraki(g.Deklaran);
            bool atakYapildi = g.Gecmis.Count > 0 || g.Masa.Count > 0;
            s.AtakKarari = !atakYapildi && lider == g.Ben;

            s.Hedef = 8 - g.Seviye;
            s.Kazanilan = g.Gecmis.Where(x => x.Kazandi && x.Oyuncu != g.Deklaran && x.Oyuncu != g.Dummy)
                                  .Select(x => x.ElNo).Distinct().Count();
            s.Gerek = Math.Max(0, s.Hedef - s.Kazanilan);

            var oynanan = new List<(Player Oyuncu, Card Kart)>();
            foreach (var e in g.Gecmis) oynanan.Add((e.Oyuncu, e.Kart));
            foreach (var m in g.Masa) oynanan.Add(m);

            // İhale bilgisi: hangi renkleri kim söyledi
            var renkSoyleyen = IhaleRenkleri(g);

            // Kart takibi (benim eller + dummy dışındakiler rakipte/ortakta)
            var gorunen = new HashSet<string>(g.Elim.Select(c => c.Suit + "|" + c.Value));
            if (g.DummyAcik) foreach (var c in g.DummyEli) gorunen.Add(c.Suit + "|" + c.Value);
            var oynandi = new HashSet<string>(oynanan.Select(o => o.Kart.Suit + "|" + o.Kart.Value));
            var disari = new Dictionary<string, List<int>>();
            foreach (var r in Renkler)
            {
                var l = new List<int>();
                for (int v = 14; v >= 2; v--)
                {
                    string k = r + "|" + v;
                    if (!gorunen.Contains(k) && !oynandi.Contains(k)) l.Add(v);
                }
                disari[r] = l;
            }
            foreach (var r in Renkler)
            {
                var bz = Deger(g.Elim, r); if (g.DummyAcik) bz.AddRange(Deger(g.DummyEli, r));
                var oy = oynanan.Where(x => x.Kart.Suit == r).Select(x => x.Kart.Value).ToList();
                s.Takip.Add(new TakipSatiri
                {
                    Renk = r, Sembol = Sem(r), Bizde = Dizi(bz), Oynanan = Dizi(oy), Disarida = Dizi(disari[r]),
                    DisariAdet = disari[r].Count, EnBuyuk = disari[r].Count == 0 ? "-" : Rk(disari[r].Max())
                });
            }

            // Motor önerisi (sıra bendeyse)
            if (g.Baglam != null)
            {
                try
                {
                    string neden;
                    var c = OyunZekasi.KartSec(g.Baglam, out neden);
                    if (c != null) { s.MotorKart = KartYazisi(c); s.Motor = KartYazisi(c) + " — " + neden; }
                }
                catch (Exception ex) { s.Motor = "Motor önerisi alınamadı: " + ex.Message; }
            }

            Rakipler(g, s, renkSoyleyen);
            if (s.AtakKarari) AtakSecenekleri(g, s, nt, renkSoyleyen);
            else Savunma(g, s, nt, oynanan, disari);

            s.Ozet = g.Kontrat + " — " + g.Ad(g.Deklaran) + " oynuyor. Kontratı batırmak için " + s.Hedef + " löve gerek" +
                     (s.Kazanilan > 0 ? " (kazandığınız " + s.Kazanilan + ", kalan " + s.Gerek + ")" : "") + ".";
            s.Gecerli = true;
            return s;
        }

        // ── İhaleden renk bilgisi: doğal sayılan renk çağrıları (anlaşmaya göre yapay olabilir) ──
        private static Dictionary<string, List<Player>> IhaleRenkleri(SavunmaGirdisi g)
        {
            var d = new Dictionary<string, List<Player>>();
            foreach (var r in Renkler) d[r] = new List<Player>();
            if (g.IhaleGecmisi == null) return d;
            Player o = g.IhaleBaslangici;
            foreach (var c in g.IhaleGecmisi)
            {
                if (!string.IsNullOrEmpty(c) && c != "Pas" && c != "Dbl" && c != "RDbl" && !c.Contains("NT") && c.Length >= 2)
                {
                    string r = null;
                    switch (c[1]) { case '♠': r = "Maça"; break; case '♥': r = "Kupa"; break; case '♦': r = "Karo"; break; case '♣': r = "Sinek"; break; }
                    if (r != null && !d[r].Contains(o)) d[r].Add(o);
                }
                o = OyunKurallari.Sonraki(o);
            }
            return d;
        }

        private static void Rakipler(SavunmaGirdisi g, SavunmaSonucu s, Dictionary<string, List<Player>> renkSoyleyen)
        {
            int dummyHp = g.DummyAcik ? g.DummyEli.Sum(c => c.Value >= 11 ? c.Value - 10 : 0) : -1;
            int benHp = g.Elim.Sum(c => c.Value >= 11 ? c.Value - 10 : 0);
            // oynanan onörler de başlangıç HP'sine dahil
            var oynananBen = g.Gecmis.Where(x => x.Oyuncu == g.Ben).Select(x => x.Kart).Concat(g.Masa.Where(m => m.Oyuncu == g.Ben).Select(m => m.Kart));
            benHp += oynananBen.Sum(c => c.Value >= 11 ? c.Value - 10 : 0);
            if (dummyHp >= 0)
            {
                var oynananDum = g.Gecmis.Where(x => x.Oyuncu == g.Dummy).Select(x => x.Kart).Concat(g.Masa.Where(m => m.Oyuncu == g.Dummy).Select(m => m.Kart));
                dummyHp += oynananDum.Sum(c => c.Value >= 11 ? c.Value - 10 : 0);
                int kalan = 40 - benHp - dummyHp;
                s.Rakipler.Add("Sizde " + benHp + " HP, dummy'de " + dummyHp + " HP: ortak ile deklaran arasında " + kalan + " HP var.");
            }
            else s.Rakipler.Add("Sizde " + benHp + " HP var.");

            if (g.IhaleGecmisi != null && g.IhaleGecmisi.Count > 0)
            {
                var cagri = new Dictionary<Player, List<string>>();
                Player o = g.IhaleBaslangici;
                foreach (var c in g.IhaleGecmisi)
                {
                    if (c != "Pas") { if (!cagri.ContainsKey(o)) cagri[o] = new List<string>(); cagri[o].Add(c); }
                    o = OyunKurallari.Sonraki(o);
                }
                foreach (var p in new[] { g.Deklaran, g.Dummy, g.Ortak })
                {
                    List<string> l;
                    string ad = g.Ad(p) + (p == g.Deklaran ? " (deklaran)" : p == g.Dummy ? " (dummy)" : " (ortak)");
                    s.Rakipler.Add(ad + ": " + (cagri.TryGetValue(p, out l) ? string.Join(" – ", l) : "ihalede konuşmadı") +
                                   "  [renkler anlaşmaya göre yapay olabilir]");
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // AÇILIŞ ATAĞI
        // ══════════════════════════════════════════════════════════════════
        private static void AtakSecenekleri(SavunmaGirdisi g, SavunmaSonucu s, bool nt, Dictionary<string, List<Player>> renkSoyleyen)
        {
            var an = g.Anlasma ?? new OrtaklikAnlasmasi();
            int stil = nt ? an.LeadNTStyle : an.LeadSuitStyle;      // 0: 3./5., 1: 4. en iyi, 2: en düşük (sanzatu)
            int kozAdet = nt ? 0 : g.Elim.Count(c => c.Suit == g.Koz);

            s.Ilkeler.Add(nt
                ? "Sanzatu atağı: amaç kendi uzun rengini ortağın onörlerine doğru kurmak. Sırayla: ortağın söylediği renk → sıralı onörlü (As/Rua/Dam) renk → en uzun/ güçlü renk (genelde dördüncü en iyi). Rakibin söylediği renklerden ve onörün altından atak yapmaktan kaçının."
                : "Renkli kontrat atağı: amaç hızlı löve almak, ortağı bulmak ve rakibin kesme/çapraz kesme planını bozmak. Sırayla: ortağın söylediği renk → sıralı onörlü renk (AK, KQ…) → tekli/ikili (kesme için, kozunuz varsa) → kozlar (rakip iki renk söylemişse ya da dummy kısa ise). Kralı/Dam'ı kapsız atmaktan ve As'ı atak etmekten (AK'sız) kaçının.");

            foreach (var renk in Renkler)
            {
                var kartlar = g.Elim.Where(c => c.Suit == renk).OrderByDescending(c => c.Value).ToList();
                if (kartlar.Count == 0) continue;
                var sat = new AtakSatiri { Renk = renk, Sembol = Sem(renk), Kartlar = Dizi(kartlar.Select(c => c.Value)) };
                int n = kartlar.Count;
                var notlar = new List<string>();
                double puan = 0;

                bool ortakSoyledi = renkSoyleyen[renk].Contains(g.Ortak);
                bool rakipSoyledi = renkSoyleyen[renk].Contains(g.Deklaran) || renkSoyleyen[renk].Contains(g.Dummy);
                bool kozRengi = !nt && renk == g.Koz;
                int hcp = kartlar.Sum(c => c.Value >= 11 ? c.Value - 10 : 0);

                // Kart seçimi
                var kural = AtakKurallari.RenkAtagi(kartlar);
                if (kural != null)
                {
                    sat.Kart = kural.Kart; sat.Kural = kural.Kural; sat.Gosterir = kural.Gosterir;
                    puan += 3; notlar.Add("sıralı onör: " + kural.Kural);
                }
                else if (nt)
                {
                    sat.Kart = AtakKurallari.SanzatuKarti(kartlar, ortakSoyledi);
                    sat.Kural = ortakSoyledi ? "Ortağın rengi" : (n >= 4 ? "Dördüncü en iyi" : n == 1 ? "Tekli" : "Alçak kart / üstten");
                    sat.Gosterir = sat.Kart.Value <= 8 && n >= 4 ? "Genelde 4+ kartlık renk, onörlü (dördüncü en iyi)" : "Atılan karta göre yorumlanır";
                }
                else
                {
                    sat.Kart = KozKartiSec(kartlar, ortakSoyledi, stil, an, out sat.Kural, out sat.Gosterir);
                }
                sat.KartYazi = KartYazisi(sat.Kart);

                // Puanlama (açıklanabilir sezgiler)
                if (ortakSoyledi) { puan += 3; notlar.Add("ortağınızın söylediği renk"); }
                if (rakipSoyledi) { puan -= 2; notlar.Add("rakibin söylediği renk (onların gücü orada)"); }
                if (nt)
                {
                    if (n >= 5) { puan += 2.5; notlar.Add(n + " kartlık uzun renk (kurma şansı)"); }
                    else if (n == 4) { puan += 1.5; notlar.Add("4 kartlık renk"); }
                    else if (n <= 2 && kural == null) { puan -= 0.5; }
                    if (hcp >= 3 && n >= 4) { puan += 0.5; notlar.Add("onörlü"); }
                }
                else
                {
                    if (kozRengi)
                    {
                        puan += 0.5;
                        int rakipRenk = renkSoyleyen.Count(kv => kv.Key != g.Koz && (kv.Value.Contains(g.Deklaran) || kv.Value.Contains(g.Dummy)));
                        if (rakipRenk >= 2) { puan += 1; notlar.Add("rakip iki yan renk söyledi: koz atağı kesmeyi azaltır"); }
                        if (kartlar.Any(c => c.Value >= 12) && n <= 3) { puan -= 1; notlar.Add("kozda onörü başlatmak deklaran/dummy'ye löve verir"); }
                    }
                    else
                    {
                        if (n == 1 && kartlar[0].Value != 14 && kozAdet >= 2) { puan += 2; notlar.Add("tekli: ortağın As'ı varsa kesme şansı (kozunuz var)"); }
                        else if (n == 2 && kozAdet >= 3 && kural == null) { puan += 1; notlar.Add("ikili: kesme için kısa renk"); }
                        if (kural == null && kartlar[0].Value >= 12 && n >= 3)
                        { puan -= 1.5; notlar.Add("onörün altından atak: deklaranın empasını kolaylaştırır"); }
                        if (kartlar[0].Value == 14 && (kural == null) && n >= 2) { puan -= 1.5; notlar.Add("As'ı Rua'sız atmak löve kaybettirir (renk kapanmadan As atılmaz)"); }
                        if (n >= 4 && kural == null && !rakipSoyledi) { puan += 0.5; notlar.Add("söylenmemiş uzun renk"); }
                    }
                }
                sat.Puan = puan;
                sat.Notlar = string.Join("; ", notlar);
                s.Atak.Add(sat);
            }
            s.Atak = s.Atak.OrderByDescending(x => x.Puan).ToList();
            if (s.Atak.Count > 0)
            {
                var en = s.Atak[0];
                s.Onerilen = en.KartYazi; s.OnerilenSembol = en.Sembol;
                s.Plan.Add("Önerilen atak (kurallara göre): " + en.KartYazi + " — " + en.Kural +
                           (en.Notlar != "" ? " (" + en.Notlar + ")" : "") + ".");
                if (en.Gosterir != "") s.Plan.Add("Ortağa mesajı: " + en.Gosterir + ".");
                if (s.Atak.Count > 1) s.Plan.Add("İkinci seçenek: " + s.Atak[1].KartYazi + " (" + s.Atak[1].Kural + ").");
            }
            if (s.MotorKart != "")
                s.Plan.Add("Motorun önerisi: " + s.Motor + (s.MotorKart != s.Onerilen ? "  → kural tablosundan farklı: motor DD örneklemesiyle seçtiği için gerekçesine bakın." : ""));
            LeadNotlari(g, s, nt);
        }

        private static Card KozKartiSec(List<Card> k, bool ortakSoyledi, int stil, OrtaklikAnlasmasi an, out string kural, out string gosterir)
        {
            int n = k.Count;
            kural = ""; gosterir = "";
            if (n == 1) { kural = "Tekli"; gosterir = "Tek kart (kesme için olabilir)"; return k[0]; }
            if (n == 2) { kural = ortakSoyledi ? "Ortağın rengi (yüksek)" : "İkiliden yüksek"; gosterir = "İkili (yüksek-alçak) olabilir"; return k[0]; }
            bool onor = k[0].Value >= 11;
            if (ortakSoyledi) { kural = "Ortağın rengi"; var c = AtakKurallari.OrtaginRengiKarti(k); gosterir = "Ortağın renginde alçak/üst atak"; return c; }
            if (n >= 4 && (onor || n >= 5))
            {
                if (stil == 1) { kural = "Dördüncü en iyi"; gosterir = "Dördüncü en iyi: 4+ kartlık renk, onörlü (onbir kuralı uygulanır)"; return k[3]; }
                kural = "Üçüncü/beşinci"; gosterir = "Çift sayıda kartta üçüncü, tek sayıda beşinci en iyi"; return k[n % 2 == 0 ? 2 : Math.Min(4, n - 1)];
            }
            if (n == 3)
            {
                if (onor) { kural = "Üçlüden alçak"; gosterir = "Onörlü üçlü (alçak kart)"; return k[2]; }
                if (an.LeadSuitMud == 1) { kural = "MUD (ortanca)"; gosterir = "Onörsüz üçlü: ortanca kart"; return k[1]; }
                kural = "Onörsüzde tepeden"; gosterir = "Onörsüz üçlü: tepeden (top of nothing)"; return k[0];
            }
            kural = "Dördüncü/üstten"; gosterir = "Onörsüz dörtlü: genelde üstten"; return onor ? k[3] : k[0];
        }

        private static void LeadNotlari(SavunmaGirdisi g, SavunmaSonucu s, bool nt)
        {
            var an = g.Anlasma ?? new OrtaklikAnlasmasi();
            string stil = (nt ? an.LeadNTStyle : an.LeadSuitStyle) == 1 ? "dördüncü en iyi" : (nt && an.LeadNTStyle == 2 ? "en düşük" : "üçüncü/beşinci en iyi");
            s.Sinyaller.Add("Atak stiliniz (anlaşma): uzun renkten " + stil + (an.LeadSuitJournalist == 1 || an.LeadNTJournalist == 1 ? ", Journalist/dokuzlu-onlu atakları açık" : "") + ".");
        }

        // ══════════════════════════════════════════════════════════════════
        // ATAK SONRASI SAVUNMA
        // ══════════════════════════════════════════════════════════════════
        private static void Savunma(SavunmaGirdisi g, SavunmaSonucu s, bool nt, List<(Player Oyuncu, Card Kart)> oynanan, Dictionary<string, List<int>> disari)
        {
            var an = g.Anlasma ?? new OrtaklikAnlasmasi();
            if (!g.DummyAcik)
            {
                s.Plan.Add(g.Ortak == OyunKurallari.Sonraki(g.Deklaran)
                    ? "Atak ortağınızda. Atak yapılıp dummy açılınca plan ve ortağın atağının yorumu burada görünür."
                    : "Dummy henüz açılmadı.");
                return;
            }

            s.Plan.Add("Hedef: kontratı batırmak için " + s.Hedef + " löve; kazandığınız " + s.Kazanilan + ", gereken " + s.Gerek + ".");

            // 1) Hızlı löveler: benim elimde, daha yüksek kart dışarıda (rakipte/ortakta) olmayan üstler
            var hizli = new List<string>();
            foreach (var r in Renkler)
            {
                var ben = Deger(g.Elim, r);
                if (ben.Count == 0) continue;
                var dis = disari[r];
                int sayi = 0; string kartlar = "";
                for (int v = 14; v >= 2; v--)
                {
                    if (ben.Contains(v)) { sayi++; kartlar += Rk(v); }
                    else if (dis.Contains(v) || g.DummyEli.Any(c => c.Suit == r && c.Value == v)) break;
                }
                if (sayi > 0) hizli.Add(Sem(r) + kartlar + " (" + sayi + ")");
            }
            s.Plan.Add(hizli.Count > 0 ? "Sizde kesin hızlı löveler: " + string.Join(", ", hizli) + " — dummy ya da deklaran araya girmeden önce kullanılabilir (kozlu kontratta kesilmeden önce çıkarın)."
                                       : "Sizde üstten kesin hızlı löve görünmüyor; löveleri ortaktan ve uzun renkten beklemelisiniz.");

            // 2) Dummy analizi
            var dummyUzun = Renkler.Where(r => r != g.Koz && g.DummyEli.Count(c => c.Suit == r) >= 5).ToList();
            foreach (var r in dummyUzun)
            {
                int boy = g.DummyEli.Count(c => c.Suit == r);
                s.Plan.Add("Dummy'nin uzun rengi " + Sem(r) + " (" + boy + " kart): deklaran bunu kurup löve alır. Bu rengin onörlerini (sizde varsa) dummy'nin girişi yok edilene dek tutmayı, " +
                           "kurmasını zorlaştırmayı (sizde A varsa 'tutma') düşünün.");
            }
            // Dummy girişleri: kendi kesin üst kartları
            var girisler = new List<string>();
            foreach (var r in Renkler)
            {
                var d = Deger(g.DummyEli, r);
                if (d.Count == 0) continue;
                for (int v = 14; v >= 2; v--)
                {
                    if (d.Contains(v)) girisler.Add(Sem(r) + Rk(v));
                    else if (disari[r].Contains(v) || g.Elim.Any(c => c.Suit == r && c.Value == v)) break;
                }
            }
            if (girisler.Count > 0) s.Plan.Add("Dummy'nin kesin giriş kartları: " + string.Join(" ", girisler) +
                " — dummy'nin uzun rengi varsa girişleri bozmak (onları erken oynatıp tüketmek ya da tutarak bağlantıyı kesmek) planınızın parçasıdır.");

            if (!nt)
            {
                int dumKoz = g.DummyEli.Count(c => c.Suit == g.Koz);
                var kisa = Renkler.Where(r => r != g.Koz).Select(r => new { r, n = g.DummyEli.Count(c => c.Suit == r) }).Where(x => x.n <= 1).ToList();
                if (dumKoz >= 2 && kisa.Count > 0)
                    s.Plan.Add("Kesme tehdidi: dummy " + string.Join(", ", kisa.Select(x => Sem(x.r) + " (" + x.n + " kart)")) + " rengini kesebilir. " +
                               "Kozları oynayıp dummy'nin kozlarını tüketmek ya da o renkten löveleri hızla çıkarmak gerekir; kozda sizin kartınız yüksekse (uzun koz) bekleyin.");
                int benKoz = g.Elim.Count(c => c.Suit == g.Koz);
                if (benKoz >= 3) s.Plan.Add("Kozda " + benKoz + " kartınız var: deklaran kozları çekerken kozu kısaltmasına izin vermeyin, yan renkte kesme yapma şansınızı değerlendirin.");
            }
            else
            {
                var benUzun = Renkler.Select(r => new { r, n = g.Elim.Count(c => c.Suit == r) }).OrderByDescending(x => x.n).First();
                if (benUzun.n >= 5)
                    s.Plan.Add("Sizin uzun rengiz " + Sem(benUzun.r) + " (" + benUzun.n + " kart): sanzatuda löveleri bu renkten kurarak alırsınız; deklaranın girişlerinden önce o rengi çalıştırmak için ortağın girişini koruyun.");
                s.Plan.Add("Sanzatu savunması: tempo önemli. Deklaran sizden önce 9 löveye ulaşırsa kazanır; uzun rengi onun girişleri tükenmeden kurmanız gerekir (ortağınızın girişini koruyun).");
            }

            // 3) Ortağın atağı/kartlarının yorumu
            Sinyaller(g, s, nt, oynanan, disari, an);

            // 4) Şu an
            if (s.MotorKart != "") s.Plan.Add("Şu an sıra sizde. Motor önerisi: " + s.Motor);
        }

        private static string Yuksek(int v) { return v >= 8 ? "yüksek" : (v <= 5 ? "alçak" : "orta"); }

        private static void Sinyaller(SavunmaGirdisi g, SavunmaSonucu s, bool nt, List<(Player Oyuncu, Card Kart)> oynanan,
                                      Dictionary<string, List<int>> disari, OrtaklikAnlasmasi an)
        {
            // Anlaşma özeti
            string tutum = an.SigAttitudeHigh == 0 ? "yüksek kart = teşvik (devam et), alçak = caydır" : "alçak kart = teşvik, yüksek = caydır (ters)";
            var yontem = SinyalKurallari.Yontem(an);
            string atma;
            switch (yontem)
            {
                case SinyalKurallari.AtmaYontemi.UDCA: atma = "UDCA: atmada alçak = istiyorum, yüksek = istemiyorum"; break;
                case SinyalKurallari.AtmaYontemi.TekCift: atma = "Tek-Çift: tek kart = istiyorum, çift kart = istemiyorum"; break;
                case SinyalKurallari.AtmaYontemi.Lavinthal: atma = "Lavinthal: yüksek kart = yüksek renk istiyorum, alçak = alçak renk"; break;
                case SinyalKurallari.AtmaYontemi.Romen: atma = "Romen: tek kart = o renk; çift kart = koz ve atılan renk dışındaki renklerden küçük (2,4) / büyük (6,8)"; break;
                default: atma = "Standart: yüksek atma = istiyorum, alçak atma = istemiyorum"; break;
            }
            s.Sinyaller.Add("Anlaşmanız: renk takibinde " + tutum + "; atmada " + atma + ".");

            // Oynanan kartları sırayla ele al
            var tum = new List<(int ElNo, Player Oyuncu, Card Kart)>();
            foreach (var e in g.Gecmis) tum.Add((e.ElNo, e.Oyuncu, e.Kart));
            int mevcutEl = g.Gecmis.Count > 0 ? g.Gecmis.Max(x => x.ElNo) + 1 : 1;
            foreach (var m in g.Masa) tum.Add((mevcutEl, m.Oyuncu, m.Kart));

            foreach (var grup in tum.GroupBy(x => x.ElNo))
            {
                var l = grup.ToList();
                string atakRengi = l[0].Kart.Suit;
                foreach (var x in l)
                {
                    if (x.Oyuncu != g.Ortak) continue;
                    var c = x.Kart;
                    if (ReferenceEquals(x.Kart, l[0].Kart) && x.ElNo == 1)
                    {
                        AtakYorumu(g, s, nt, c, an);
                        continue;
                    }
                    if (l[0].Oyuncu == g.Ortak) continue;       // ortak elin açıcısıysa atak gibi
                    if (c.Suit == atakRengi)
                    {
                        bool ortakAtmisOnor = c.Value >= 11;
                        string t = "El " + x.ElNo + ": ortak " + Sem(atakRengi) + Rk(c.Value) + " verdi (" + Yuksek(c.Value) + ") → ";
                        if (ortakAtmisOnor) t += "onör: kazanmak/ bloke etmek için olabilir; sinyal sayılmaz.";
                        else if (an.SigPrimary == 1) t += (Yuksek(c.Value) == "alçak" ? (an.SigAttitudeHigh == 0 ? "bu rengi istemiyor (caydırıyor)." : "bu rengi istiyor (teşvik).")
                                                          : Yuksek(c.Value) == "yüksek" ? (an.SigAttitudeHigh == 0 ? "bu rengi istiyor (teşvik)." : "bu rengi istemiyor (caydırıyor).")
                                                          : "belirsiz (orta kart).");
                        else if (an.SigPrimary == 0) t += (Yuksek(c.Value) == "yüksek" ? "çift sayıda (genelde) kart gösteriyor, " : "tek sayıda kart gösteriyor, ") + "ilk renkten sayı verdi (anlaşmanızda sayı sinyali).";
                        else t += "tek/çift sinyali: " + (c.Value % 2 == 0 ? "çift (istemiyor/çift sayı)" : "tek (istiyor/tek sayı)") + ".";
                        s.Sinyaller.Add(t);
                    }
                    else if (g.Koz == null || c.Suit != g.Koz)
                    {
                        // atma
                        s.Sinyaller.Add("El " + x.ElNo + ": ortak " + Sem(c.Suit) + Rk(c.Value) + " attı → " + AtmaYorumu(c, yontem, an));
                    }
                    else
                    {
                        s.Sinyaller.Add("El " + x.ElNo + ": ortak " + Sem(c.Suit) + Rk(c.Value) + " ile kesti.");
                    }
                }
            }
            if (s.Sinyaller.Count == 1) s.Sinyaller.Add("Ortak henüz yorumlanacak bir kart oynamadı.");
        }

        private static string AtmaYorumu(Card c, SinyalKurallari.AtmaYontemi y, OrtaklikAnlasmasi an)
        {
            string r = Sem(c.Suit);
            switch (y)
            {
                case SinyalKurallari.AtmaYontemi.UDCA:
                    return c.Value <= 6 ? "alçak (" + Rk(c.Value) + "): " + r + " rengini İSTİYOR (UDCA)." : "yüksek (" + Rk(c.Value) + "): " + r + " rengini İSTEMİYOR (UDCA).";
                case SinyalKurallari.AtmaYontemi.TekCift:
                    return c.Value % 2 == 1 ? "tek kart: " + r + " rengini İSTİYOR (tek-çift)." : "çift kart: " + r + " rengini İSTEMİYOR (tek-çift).";
                case SinyalKurallari.AtmaYontemi.Lavinthal:
                    return c.Value >= 7 ? "yüksek kart: diğer renklerin yükseğini istiyor (Lavinthal)." : "alçak kart: diğer renklerin alçağını istiyor (Lavinthal).";
                case SinyalKurallari.AtmaYontemi.Romen:
                    return c.Value % 2 == 1 ? "tek kart: " + r + " rengini oynamanızı istiyor (Romen)." :
                           (c.Value <= 4 ? "küçük çift: kalan renklerden küçük rütbeliyi istiyor (Romen)." : "büyük çift: kalan renklerden büyük rütbeliyi istiyor (Romen).");
                default:
                    return (c.Value >= 8 ? "yüksek (" + Rk(c.Value) + "): " + r + " rengini İSTİYOR (standart)." : "alçak (" + Rk(c.Value) + "): " + r + " rengini İSTEMİYOR, başka renk ister (standart).");
            }
        }

        private static void AtakYorumu(SavunmaGirdisi g, SavunmaSonucu s, bool nt, Card c, OrtaklikAnlasmasi an)
        {
            string yaz = "Ortağın atağı " + KartYazisi(c) + ": ";
            string r = Sem(c.Suit);
            switch (c.Value)
            {
                case 14: yaz += "As atağı: genelde AK (ya da kozlu kontratta tek As) var; kart sayısı/ tutum sinyali isteyebilir."; break;
                case 13: yaz += "Rua atağı: K ile Q (ya da A K) var; dummy'den daha yüksek bir kart yoksa devam eder."; break;
                case 12: yaz += "Dam atağı: Q J (ya da K Q) sekansı var."; break;
                case 11: yaz += "Vale atağı: J 10 sekansı; üstte onör olmayabilir."; break;
                case 10: yaz += "Onlu atağı: 10 ile 9/J bağlantılı (Journalist) ya da onörsüz renkte üstten."; break;
                case 9: yaz += "Dokuzlu atağı (Journalist) ya da onörsüz renkte tepeden."; break;
                default:
                    yaz += "alçak kart: " + (nt ? (an.LeadNTStyle == 1 ? "dördüncü en iyi" : "uzun renkten alçak kart") : (an.LeadSuitStyle == 1 ? "dördüncü en iyi" : "üçüncü/beşinci en iyi")) +
                           " (4+ kartlık renk, üstte onör) — ya da tekli/ikili (kesme için).";
                    int stil = nt ? an.LeadNTStyle : an.LeadSuitStyle;
                    if (stil == 1)
                    {
                        int benUst = g.Elim.Count(x => x.Suit == c.Suit && x.Value > c.Value);
                        int dumUst = g.DummyAcik ? g.DummyEli.Count(x => x.Suit == c.Suit && x.Value > c.Value) : 0;
                        // Oynanmış yüksek kartlar da hesaba katılmalı
                        int oynanmisUst = g.Gecmis.Count(x => x.Kart.Suit == c.Suit && x.Kart.Value > c.Value && x.ElNo > 0 && x.Oyuncu != g.Ortak) +
                                          g.Masa.Count(x => x.Kart.Suit == c.Suit && x.Kart.Value > c.Value && x.Oyuncu != g.Ortak);
                        int dek = 11 - c.Value - benUst - dumUst;
                        // 11 kuralı yalnız açılış atağından hemen sonra geçerli; oynananları atak elinden sayma
                        if (g.Gecmis.Count == 0 || g.Gecmis.Count <= 4)
                            yaz += "  ONBİR KURALI: 11 − " + c.Value + " = " + (11 - c.Value) + " kart (ortak hariç üç elde) " + r + "'de atağın üstünde; " +
                                   "sizde " + benUst + ", dummy'de " + dumUst + " → deklaranda " + Math.Max(0, dek) + " kart " + Rk(c.Value) + "'in üstünde." +
                                   (dek <= 0 ? " Deklaranın bu renkte atağın üstünde kartı yok: üçüncü elde kazanmaya çalışırken rahat olun." : "");
                    }
                    break;
            }
            s.Sinyaller.Add(yaz);
        }
    }
}
