using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Plan
{
    /// <summary>Plan analizinin girdisi: dummy açıldığı andaki masa durumu (deklaran tarafı gözüyle).</summary>
    public sealed class PlanGirdisi
    {
        public string Kontrat = "";                 // "4♠", "3NT", "4♥X" ...
        public string Koz;                          // null = sanzatu
        public int Seviye;                          // 1-7
        public Player Deklaran;
        public Player Dummy;
        public List<Card> DeklaranEli = new List<Card>();   // kalan kartlar
        public List<Card> DummyEli = new List<Card>();      // kalan kartlar
        public List<(Player Oyuncu, Card Kart)> Masa = new List<(Player Oyuncu, Card Kart)>();
        public List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)> Gecmis = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>();
        public List<string> IhaleGecmisi = new List<string>();
        public Player IhaleBaslangici;              // ihaleyi başlatan (dağıtıcı)
        public Func<Player, string> Ad = p => p.ToString();
    }

    public sealed class RenkSatiri
    {
        public string Renk;                 // Maça/Kupa/Karo/Sinek
        public string Sembol;
        public bool KozMu;
        public string DeklaranKartlari = "";
        public string DummyKartlari = "";
        public int DeklaranBoy, DummyBoy;
        public int Kesin;                   // üstten sağlam löve
        public bool Hesaplandi;             // planlayıcı çalıştı mı
        public double Beklenen;             // beklenen löve (kesin dahil)
        public double TekBasinaSans = -1;   // eksik löveyi tek başına tamamlama şansı (0..1)
        public string IlkKart = "";         // planlayıcının önerdiği ilk kart
        public Player? IlkEl;               // o kartın oynanacağı el
        public string Not = "";
        public int Kesme;                   // kısa el kesmesi (koz kontratında yan renk)
        public double Kazanc { get { return Hesaplandi ? Beklenen - Kesin : 0; } }
    }

    public sealed class TakipSatiri
    {
        public string Renk, Sembol;
        public string Bizde = "";
        public string Oynanan = "";
        public string Disarida = "";
        public int DisariAdet;
        public string EnBuyuk = "-";
    }

    public sealed class PlanSonucu
    {
        public bool Gecerli;
        public string Hata = "";
        public int Hedef, Kazanilan, Kalan, KesinToplam, KalanEl;
        public double BeklenenToplam;
        public int KesmeToplam;
        public string Ozet = "";
        public List<RenkSatiri> Renkler = new List<RenkSatiri>();
        public List<TakipSatiri> Takip = new List<TakipSatiri>();
        public Player AnaEl;
        public string AnaElNeden = "";
        public string IlkRenk = "";             // önerilen ilk renk (kaynak)
        public List<string> Plan = new List<string>();
        public List<string> KozKarari = new List<string>();
        public List<string> Girisler = new List<string>();
        public List<string> Rakipler = new List<string>();
        public List<string> KalanKartlar = new List<string>();
    }

    /// <summary>
    /// Deklaran oyun planı yardımcısı: dummy açıldığında kesin löveleri sayar, eksik löveyi bulur, kaynakları
    /// (uzun renk, empas, kesme) sıralar ve ana el / koz çekme kararı için gerekçeli öneri üretir.
    /// Beklenen löve hesabı RenkPlanlayici (empas, düşürme ihtimalleri) ile yapılır; giriş sorunu ve rakip kesmesi hesaba katılmaz.
    /// </summary>
    public static class OyunPlanAnalizi
    {
        private static readonly string[] Renkler = { "Maça", "Kupa", "Karo", "Sinek" };

        public static string Sembol(string renk)
        {
            switch (renk) { case "Maça": return "♠"; case "Kupa": return "♥"; case "Karo": return "♦"; case "Sinek": return "♣"; default: return "NT"; }
        }

        public static string Sira(int v)
        {
            switch (v) { case 14: return "A"; case 13: return "K"; case 12: return "Q"; case 11: return "J"; default: return v.ToString(); }
        }

        private static string Dizi(IEnumerable<int> degerler)
        {
            var l = degerler.OrderByDescending(v => v).Select(Sira).ToList();
            return l.Count == 0 ? "—" : string.Join(" ", l);
        }

        private static List<int> Kartlar(List<Card> el, string renk)
        {
            return el.Where(c => c.Suit == renk).Select(c => c.Value).OrderByDescending(v => v).ToList();
        }

        private static int Hcp(Card c) { return c.Value >= 11 ? c.Value - 10 : 0; }

        public static PlanSonucu Coz(PlanGirdisi g, bool planlayiciKullan = true)
        {
            var s = new PlanSonucu();
            if (g == null || g.DeklaranEli == null || g.DummyEli == null || g.DeklaranEli.Count == 0 && g.DummyEli.Count == 0)
            { s.Hata = "Eller henüz hazır değil."; return s; }

            Player dek = g.Deklaran, dum = g.Dummy;
            bool nt = g.Koz == null;

            // ── Oynananlar ve dışarıdaki kartlar ──
            var oynanan = new List<(Player Oyuncu, Card Kart)>();
            foreach (var e in g.Gecmis) oynanan.Add((e.Oyuncu, e.Kart));
            foreach (var m in g.Masa) oynanan.Add(m);

            var bizde = new HashSet<string>();
            foreach (var c in g.DeklaranEli) bizde.Add(c.Suit + "|" + c.Value);
            foreach (var c in g.DummyEli) bizde.Add(c.Suit + "|" + c.Value);
            var oynandi = new HashSet<string>(oynanan.Select(o => o.Kart.Suit + "|" + o.Kart.Value));

            var disari = new Dictionary<string, List<int>>();
            foreach (var r in Renkler)
            {
                var l = new List<int>();
                for (int v = 14; v >= 2; v--)
                {
                    string k = r + "|" + v;
                    if (!bizde.Contains(k) && !oynandi.Contains(k)) l.Add(v);
                }
                disari[r] = l;
            }

            int bitenEl = g.Gecmis.Select(x => x.ElNo).Distinct().Count();
            s.KalanEl = 13 - bitenEl;
            s.Hedef = 6 + g.Seviye;
            s.Kazanilan = g.Gecmis.Where(x => x.Kazandi && (x.Oyuncu == dek || x.Oyuncu == dum)).Select(x => x.ElNo).Distinct().Count();
            s.Kalan = Math.Max(0, s.Hedef - s.Kazanilan);

            Player opp1 = OyunKurallari.Sonraki(dek);
            Player opp2 = OyunKurallari.Sonraki(dum);
            if (opp1 == dum) { opp1 = OyunKurallari.Sonraki(dum); opp2 = OyunKurallari.Sonraki(dek); }
            var rakipler = new[] { opp1, opp2 };

            // ── Rakiplerin kalan kartları ve renksizlikleri ──
            var kalanKart = new Dictionary<Player, int>();
            var renksiz = new Dictionary<Player, HashSet<string>>();
            foreach (var o in rakipler)
            {
                kalanKart[o] = 13 - oynanan.Count(x => x.Oyuncu == o);
                renksiz[o] = new HashSet<string>();
            }
            foreach (var grup in g.Gecmis.GroupBy(x => x.ElNo))
            {
                var l = grup.ToList();
                string atak = l[0].Kart.Suit;
                foreach (var e in l)
                    if (e.Kart.Suit != atak && renksiz.ContainsKey(e.Oyuncu)) renksiz[e.Oyuncu].Add(atak);
            }
            if (g.Masa.Count > 0)
            {
                string atak = g.Masa[0].Kart.Suit;
                foreach (var m in g.Masa)
                    if (m.Kart.Suit != atak && renksiz.ContainsKey(m.Oyuncu)) renksiz[m.Oyuncu].Add(atak);
            }

            // ── Renk renk analiz ──
            int rakipKoz = nt ? 0 : disari[g.Koz].Count;
            foreach (var renk in Renkler)
            {
                var d = Kartlar(g.DeklaranEli, renk);
                var m = Kartlar(g.DummyEli, renk);
                var sat = new RenkSatiri
                {
                    Renk = renk, Sembol = Sembol(renk), KozMu = renk == g.Koz,
                    DeklaranKartlari = Dizi(d), DummyKartlari = Dizi(m), DeklaranBoy = d.Count, DummyBoy = m.Count
                };
                var dis = disari[renk];
                var birlesik = new HashSet<int>(d.Concat(m));
                int kesin = 0;
                for (int v = 14; v >= 2; v--)
                {
                    if (birlesik.Contains(v)) kesin++;
                    else if (dis.Contains(v)) break;
                }
                kesin = Math.Min(kesin, Math.Max(d.Count, m.Count));
                sat.Kesin = kesin;

                int enUzun = Math.Max(d.Count, m.Count);
                if (enUzun == 0)
                {
                    sat.Not = "İki elde de kart yok.";
                }
                else if (dis.Count == 0)
                {
                    sat.Hesaplandi = true; sat.Beklenen = enUzun; sat.Not = "Rakipte kart kalmadı: hepsi sizin.";
                }
                else if (planlayiciKullan && dis.Count <= RenkPlanlayici.EnFazlaDisariKart)
                {
                    bool dekBasla = d.Count > 0;
                    var a = (dekBasla ? d : m).ToArray();
                    var b = (dekBasla ? m : d).ToArray();
                    bool xBos = renksiz[opp1].Contains(renk), yBos = renksiz[opp2].Contains(renk);
                    int kx = Math.Max(0, kalanKart[opp1]), ky = Math.Max(0, kalanKart[opp2]);
                    var r0 = RenkPlanlayici.Coz(a, b, dis.ToArray(), kx, ky, xBos, yBos, 0);
                    if (r0.Basarili)
                    {
                        sat.Hesaplandi = true;
                        sat.Beklenen = Math.Max(kesin, r0.BeklenenLove);
                        sat.IlkKart = Sira(r0.Kart);
                        sat.IlkEl = dekBasla ? dek : dum;
                    }
                    else sat.Not = r0.Neden;
                }
                else if (dis.Count > RenkPlanlayici.EnFazlaDisariKart)
                {
                    sat.Not = "Dışarıda " + dis.Count + " kart var; kesin löveler sayıldı.";
                }
                if (!sat.Hesaplandi) sat.Beklenen = kesin;
                s.Renkler.Add(sat);
            }

            // ── Koz kontratında kesme fırsatları ──
            if (!nt)
            {
                int dekKoz = Kartlar(g.DeklaranEli, g.Koz).Count, dumKoz = Kartlar(g.DummyEli, g.Koz).Count;
                bool dumKisaKoz = dumKoz < dekKoz;       // kozu kısa olan el kesen eldir
                int kesenKoz = Math.Min(dekKoz, dumKoz);
                foreach (var sat in s.Renkler.Where(x => !x.KozMu))
                {
                    int kisa = dumKisaKoz ? sat.DummyBoy : sat.DeklaranBoy;
                    int uzun = dumKisaKoz ? sat.DeklaranBoy : sat.DummyBoy;
                    if (dekKoz == dumKoz) { kisa = Math.Min(sat.DeklaranBoy, sat.DummyBoy); uzun = Math.Max(sat.DeklaranBoy, sat.DummyBoy); }
                    if (uzun > kisa) sat.Kesme = uzun - kisa;
                }
                s.KesmeToplam = Math.Min(s.Renkler.Where(x => !x.KozMu).Sum(x => x.Kesme), Math.Max(0, kesenKoz));
            }

            s.KesinToplam = Math.Min(s.Renkler.Sum(x => x.Kesin), s.KalanEl);
            s.BeklenenToplam = Math.Min(s.Renkler.Sum(x => x.Beklenen), s.KalanEl);
            int eksik = s.Kalan - s.KesinToplam;

            // Kaynak başına "tek başına yeterlilik" şansı (yalnızca eksik varsa)
            if (planlayiciKullan && eksik > 0)
            {
                foreach (var sat in s.Renkler.Where(x => x.Hesaplandi && x.Kazanc > 0.05 && x.IlkEl.HasValue))
                {
                    var d = Kartlar(g.DeklaranEli, sat.Renk); var m = Kartlar(g.DummyEli, sat.Renk);
                    bool dekBasla = sat.IlkEl.Value == dek;
                    var dis = disari[sat.Renk];
                    if (dis.Count == 0 || dis.Count > RenkPlanlayici.EnFazlaDisariKart) continue;
                    var r1 = RenkPlanlayici.Coz((dekBasla ? d : m).ToArray(), (dekBasla ? m : d).ToArray(), dis.ToArray(),
                        Math.Max(0, kalanKart[opp1]), Math.Max(0, kalanKart[opp2]),
                        renksiz[opp1].Contains(sat.Renk), renksiz[opp2].Contains(sat.Renk), sat.Kesin + eksik);
                    if (r1.Basarili) sat.TekBasinaSans = r1.YapmaSansi;
                }
            }

            // ── Kart takibi ──
            foreach (var renk in Renkler)
            {
                var bz = new List<int>(Kartlar(g.DeklaranEli, renk)); bz.AddRange(Kartlar(g.DummyEli, renk));
                var oy = oynanan.Where(x => x.Kart.Suit == renk).Select(x => x.Kart.Value).ToList();
                var dis = disari[renk];
                s.Takip.Add(new TakipSatiri
                {
                    Renk = renk, Sembol = Sembol(renk),
                    Bizde = Dizi(bz), Oynanan = Dizi(oy), Disarida = Dizi(dis), DisariAdet = dis.Count,
                    EnBuyuk = dis.Count == 0 ? "-" : Sira(dis.Max())
                });
            }

            // ── Ana el ──
            var dekGirisi = Girisler(g.DeklaranEli, g.DummyEli, disari);
            var dumGirisi = Girisler(g.DummyEli, g.DeklaranEli, disari);
            AnaElSec(g, s, nt, dek, dum, dekGirisi, dumGirisi);

            s.Girisler.Add(g.Ad(dek) + " (deklaran) girişleri: " + (dekGirisi.Count == 0 ? "yok" : string.Join(", ", dekGirisi)));
            s.Girisler.Add(g.Ad(dum) + " (dummy) girişleri: " + (dumGirisi.Count == 0 ? "yok" : string.Join(", ", dumGirisi)));

            // ── Plan ──
            var kaynaklar = s.Renkler.Where(x => x.Kazanc > 0.05).OrderByDescending(x => x.Kazanc).ToList();
            s.IlkRenk = kaynaklar.Count > 0 ? kaynaklar[0].Renk : "";
            PlanYaz(g, s, nt, eksik, kaynaklar, rakipKoz, dek, dum);
            if (!nt) KozKarariYaz(g, s, disari, rakipKoz);
            RakipYaz(g, s, rakipler, kalanKart, renksiz, oynanan, disari);

            s.Ozet = g.Kontrat + " — " + g.Ad(dek) + " oynuyor. Hedef " + s.Hedef + " löve; kazanılan " + s.Kazanilan +
                     "; hedefe " + s.Kalan + " löve kaldı (kalan el: " + s.KalanEl + ").";
            s.Gecerli = true;
            return s;
        }

        private static List<string> Girisler(List<Card> el, List<Card> diger, Dictionary<string, List<int>> disari)
        {
            // Bu elin kesin löve kartları (diğer elde o renkten kartı olan renklerde bu eli açan giriş)
            var sonuc = new List<string>();
            foreach (var renk in Renkler)
            {
                var benim = Kartlar(el, renk);
                var oteki = Kartlar(diger, renk);
                if (benim.Count == 0) continue;
                var birlesik = new HashSet<int>(benim.Concat(oteki));
                var dis = disari[renk];
                for (int v = 14; v >= 2; v--)
                {
                    if (birlesik.Contains(v))
                    {
                        if (benim.Contains(v)) sonuc.Add(Sembol(renk) + Sira(v));
                    }
                    else if (dis.Contains(v)) break;
                }
            }
            return sonuc;
        }

        private static void AnaElSec(PlanGirdisi g, PlanSonucu s, bool nt, Player dek, Player dum, List<string> dekG, List<string> dumG)
        {
            if (!nt)
            {
                int dk = Kartlar(g.DeklaranEli, g.Koz).Count, dm = Kartlar(g.DummyEli, g.Koz).Count;
                if (dk != dm)
                {
                    s.AnaEl = dk > dm ? dek : dum;
                    s.AnaElNeden = "Kozu daha uzun olan el (" + Math.Max(dk, dm) + " koz, diğerinde " + Math.Min(dk, dm) +
                        ") ana el olur: rakiplerin kozlarını çekerken kontrol bu elde kalır; kısa koz eli kesme eli olur.";
                }
                else
                {
                    s.AnaEl = dekG.Count >= dumG.Count ? dek : dum;
                    s.AnaElNeden = "Koz uzunlukları eşit (" + dk + "-" + dm + "); daha çok giriş kartı olan el ana el seçildi.";
                }
            }
            else
            {
                // Sanzatuda: geliştirilecek uzun rengin elini ana el yap, giriş kartları diğer elde dursun
                var kaynak = s.Renkler.Where(x => x.Kazanc > 0.05).OrderByDescending(x => x.Kazanc).FirstOrDefault();
                if (kaynak != null && kaynak.DeklaranBoy != kaynak.DummyBoy)
                {
                    s.AnaEl = kaynak.DeklaranBoy > kaynak.DummyBoy ? dek : dum;
                    s.AnaElNeden = "Geliştirilecek ana renk " + kaynak.Sembol + " ve uzun tarafı bu elde (" +
                        Math.Max(kaynak.DeklaranBoy, kaynak.DummyBoy) + " kart): uzun löveleri bu elden çekersiniz. " +
                        "Diğer eldeki girişleri (" + (s.AnaEl == dek ? dumG : dekG).Count + " adet) harcamadan korumaya bakın.";
                }
                else
                {
                    s.AnaEl = dekG.Count >= dumG.Count ? dek : dum;
                    s.AnaElNeden = "Belirgin tek bir uzun renk yok; daha çok giriş kartı olan el ana el seçildi.";
                }
            }
        }

        private static void PlanYaz(PlanGirdisi g, PlanSonucu s, bool nt, int eksik, List<RenkSatiri> kaynaklar, int rakipKoz, Player dek, Player dum)
        {
            s.Plan.Add("1) SAY: Hedef " + s.Hedef + " löve, kazanılan " + s.Kazanilan + " → daha " + s.Kalan + " löve gerek. " +
                       "Üstten kesin löveler: " + s.KesinToplam + ".");
            if (s.Kalan <= 0)
            {
                s.Plan.Add("2) Hedefe ulaştınız; kalanlarda risk almadan kesinleri toplayın.");
                return;
            }
            if (eksik <= 0)
            {
                s.Plan.Add("2) Kesin löveler hedefe yetiyor (" + s.KesinToplam + " ≥ " + s.Kalan + "). Plan: risksiz toplamak. " +
                           "Rakiplere löve verecek hiçbir empas/deneme yapmayın; önce rakibin sizi kesebileceği/geçebileceği tehlikeyi kapatın" +
                           (nt ? " (duruşlarınızı koruyun)." : " (kozları çekin)."));
                return;
            }
            s.Plan.Add("2) EKSİK: " + eksik + " löve daha bulmanız gerek. Kaynak adayları (beklenen kazanç):");
            if (kaynaklar.Count == 0)
            {
                s.Plan.Add("   – Üst kartların dışında kazanç görünmüyor; kesme, düşürme ya da rakibi sıkıştırma (el sonu) aranmalı.");
            }
            foreach (var k in kaynaklar)
            {
                string t = "   – " + k.Sembol + ": +" + k.Kazanc.ToString("0.0") + " löve";
                if (k.IlkEl.HasValue && k.IlkKart != "") t += ", ilk kart " + k.Sembol + k.IlkKart + " (" + g.Ad(k.IlkEl.Value) + "'den)";
                if (k.TekBasinaSans > 0.005) t += ", tek başına yeterli olma şansı %" + Math.Round(k.TekBasinaSans * 100);
                else if (k.TekBasinaSans >= 0) t += ", tek başına eksik löveyi tamamlayamaz";
                s.Plan.Add(t);
            }
            if (!nt && s.KesmeToplam > 0)
                s.Plan.Add("   – Kesme: kısa koz elinde yan renkten toplam " + s.KesmeToplam + " kesme denenebilir (ek löve; ayrıntı 'Koz' bölümünde).");

            double toplam = s.KesinToplam + kaynaklar.Sum(x => x.Kazanc) + (nt ? 0 : s.KesmeToplam);
            s.Plan.Add("3) Toplam beklenti: " + toplam.ToString("0.0") + " löve (hedef " + s.Kalan + "). " +
                       (toplam >= s.Kalan ? "Plan yeterli görünüyor; en yüksek kazançlı kaynaktan başlayın, ancak önce girişleri koruyun."
                                          : "Tek kaynak yetmiyor; birkaç kaynağı birleştirmek (ve rakibin yanlış dağılımına güvenmek) gerekecek."));
            if (nt)
                s.Plan.Add("4) Sanzatuda ilke: rakipler önce kendi uzun rengini kurar. Rakibin atak rengini durdurma sayısını koruyun (ona löve " +
                           "verecekseniz 'bekleme' ile rakip bağlantısını kesin) ve kaynak rengi geliştirirken girişleri harcamayın.");
        }

        private static void KozKarariYaz(PlanGirdisi g, PlanSonucu s, Dictionary<string, List<int>> disari, int rakipKoz)
        {
            int dk = Kartlar(g.DeklaranEli, g.Koz).Count, dm = Kartlar(g.DummyEli, g.Koz).Count;
            var kozDis = disari[g.Koz];
            string sem = Sembol(g.Koz);
            s.KozKarari.Add("Bizde " + sem + " : " + dk + " + " + dm + " = " + (dk + dm) + " kart; rakiplerde " + rakipKoz +
                            " kalmış" + (rakipKoz > 0 ? " (en büyüğü " + Sira(kozDis.Max()) + ")" : "") + ".");
            if (rakipKoz == 0)
            {
                s.KozKarari.Add("Rakipte koz kalmadı: kozları çekmeye gerek yok, yan renkleri rahatça oynayın.");
                return;
            }
            var kesmeler = s.Renkler.Where(x => !x.KozMu && x.Kesme > 0).ToList();
            var kesenEl = dm < dk ? g.Dummy : (dk < dm ? g.Deklaran : (Player?)null);
            int kozEksik = Math.Max(0, kozDis.Count);
            if (kesenEl.HasValue && kesmeler.Count > 0 && s.KesmeToplam > 0)
            {
                s.KozKarari.Add("KARAR: Önce kozları çekmeyin. " + g.Ad(kesenEl.Value) + " eli kısa koz elidir; şu renklerde kesme var: " +
                                string.Join(", ", kesmeler.Select(x => x.Sembol + " (" + x.Kesme + ")")) +
                                ". Kozları çekerseniz kesme kozlarını da harcarsınız. Önce yan renklerden kaybedeceğiniz kartları verin/kesin.");
                s.KozKarari.Add("Uyarı: kesen elin kozu rakibinkinden küçükse (üstüne basılabilir) ya da rakipte uzun koz varsa kesme tehlikelidir; " +
                                "ayrıca rakip yan renkte sizi kesebilecek kadar kısaysa önce kozu sayıp çekin.");
            }
            else
            {
                int enAz = Math.Max(1, (int)Math.Ceiling(rakipKoz / 2.0));
                s.KozKarari.Add("KARAR: Önce kozları çekin. Kesme kaynağı yok (ya da gerekmiyor); beklemek rakibe kozla kesme ve yan renk As'larıyla " +
                                "hızlı löve fırsatı verir. Rakiplerde " + rakipKoz + " koz var: kozlar eşit bölünürse en az " + enAz + " tur gerekir.");
            }
            var hizliAs = Renkler.Where(r => r != g.Koz && disari[r].Contains(14) && Kartlar(g.DeklaranEli, r).Count + Kartlar(g.DummyEli, r).Count > 0)
                                 .Select(Sembol).ToList();
            if (hizliAs.Count > 0)
                s.KozKarari.Add("Rakiplerde dışarıda As'lar var: " + string.Join(" ", hizliAs) + " → bu renklerde kaybedeceğiniz löveleri " +
                                "kozdan önce ya da kozla beraber planlayın.");
        }

        private static void RakipYaz(PlanGirdisi g, PlanSonucu s, Player[] rakipler, Dictionary<Player, int> kalanKart,
                                     Dictionary<Player, HashSet<string>> renksiz, List<(Player Oyuncu, Card Kart)> oynanan,
                                     Dictionary<string, List<int>> disari)
        {
            // Rakiplerin toplam HCP'si: 40 − bizim iki elin başlangıç HCP'si
            int bizHcp = g.DeklaranEli.Sum(Hcp) + g.DummyEli.Sum(Hcp)
                         + oynanan.Where(x => x.Oyuncu == g.Deklaran || x.Oyuncu == g.Dummy).Sum(x => Hcp(x.Kart));
            int rakipHcp = 40 - bizHcp;
            s.Rakipler.Add("Rakiplerin toplam gücü: " + rakipHcp + " HP (sizde " + bizHcp + ").");

            // İhale: kim ne söyledi
            var cagrilar = new Dictionary<Player, List<string>>();
            foreach (var r in rakipler) cagrilar[r] = new List<string>();
            Player? ilkAcan = null; string acilis = null;
            if (g.IhaleGecmisi != null)
            {
                Player o = g.IhaleBaslangici;
                foreach (var c in g.IhaleGecmisi)
                {
                    bool pas = c == "Pas";
                    if (cagrilar.ContainsKey(o) && !pas) cagrilar[o].Add(c);
                    if (!pas && !ilkAcan.HasValue && (o == rakipler[0] || o == rakipler[1])) { ilkAcan = o; acilis = c; }
                    else if (!pas && !ilkAcan.HasValue) { ilkAcan = o; acilis = c; }
                    o = OyunKurallari.Sonraki(o);
                }
            }
            foreach (var r in rakipler)
            {
                var l = cagrilar[r];
                s.Rakipler.Add(g.Ad(r) + ": " + (l.Count == 0 ? "ihalede hiç konuşmadı (hep Pas); ihaleye karışacak kadar güçlü/dağılımlı değil."
                                                              : "söyledikleri: " + string.Join(" – ", l) + " (renkler anlaşmaya göre doğal olmayabilir)."));
            }
            if (ilkAcan.HasValue && (ilkAcan.Value == rakipler[0] || ilkAcan.Value == rakipler[1]) && acilis != null && acilis.Length >= 2)
            {
                int asgari = acilis.EndsWith("NT") && acilis[0] == '1' ? 15 : (acilis[0] == '1' ? 12 : 0);
                if (asgari > 0)
                {
                    var diger = ilkAcan.Value == rakipler[0] ? rakipler[1] : rakipler[0];
                    int enFazla = Math.Max(0, rakipHcp - asgari);
                    s.Rakipler.Add(g.Ad(ilkAcan.Value) + " " + acilis + " açtı: en az " + asgari + " HP bekleyin; " +
                                   g.Ad(diger) + "'de en çok " + enFazla + " HP kalır. Önemli onörleri (K, Q) açıcıda arayın.");
                }
            }
            foreach (var r in rakipler)
            {
                if (renksiz[r].Count > 0)
                    s.Rakipler.Add(g.Ad(r) + " renk veremedi: " + string.Join(" ", renksiz[r].Select(Sembol)) + " (artık bu rengi kesebilir/atabilir).");
                s.Rakipler.Add(g.Ad(r) + " elinde " + kalanKart[r] + " kart kaldı.");
            }
            foreach (var renk in Renkler)
            {
                int say = disari[renk].Count;
                if (say > 0) s.KalanKartlar.Add(Sembol(renk) + ": " + say + " kart dışarıda (" + Dizi(disari[renk]) + ")");
            }
        }
    }
}
