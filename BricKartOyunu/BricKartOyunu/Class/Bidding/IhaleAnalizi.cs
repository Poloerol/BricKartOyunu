using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>Bir koltuğun şimdiye kadarki ihalesinden çıkarılan el bilgisi (Bridge Baron "Auction Interpretation" gibi).</summary>
    public sealed class KoltukYorumu
    {
        public Player Koltuk;
        public bool KonustuMu;                         // bu koltuk en az bir çağrı yaptı mı
        public int HcMin, HcMax;                       // HCP aralığı (HcMax >= 40: üst sınır yok)
        public int ToplamMin, ToplamMax;               // HCP + dağılım puanı tahmini
        public int[] MinUz = new int[4];               // IhaleAnalizi.Renkler sırasında alt uzunluk sınırı
        public bool Dengeli;                           // 1NT / 2NT açılışı gibi dengeli el
        public string EnIyiUyum;                       // ortaklıkla bilinen en iyi uyum (null: yok)
        public string SonCagri;                        // koltuğun son çağrısı
        public string SonAciklama;                     // son çağrının kısa anlamı (Naturel / Yapay / Kontr ...)

        /// <summary>Listede gösterilecek satırlar.</summary>
        public List<string> Satirlar()
        {
            var l = new List<string>();
            if (!KonustuMu) return l;
            l.Add(Aralik(ToplamMin, ToplamMax) + " Toplam Puan");
            l.Add(Aralik(HcMin, HcMax) + " HCP");
            if (Dengeli) l.Add("Dengeli el");
            for (int r = 0; r < 4; r++)
                if (MinUz[r] >= 3) l.Add(MinUz[r] + "+ " + IhaleAnalizi.Renkler[r]);
            if (EnIyiUyum != null) l.Add("En iyi bilinen uyum: " + EnIyiUyum);
            return l;
        }

        private static string Aralik(int min, int max)
        {
            if (max >= 40) return min + "+";
            return min + "-" + max;
        }
    }

    /// <summary>
    /// İhale dizisinden her koltuğun gösterdiği HCP aralığını, renk uzunluklarını ve ortaklığın en iyi uyumunu çıkarır.
    /// HCP aralığı IhaleKoruma.PartnerAraligi'ndan, yapay çağrılar IhaleYorumcusu'ndan gelir; yani ihale motoru ve
    /// oyun yapay zekâsıyla aynı varsayımları kullanır. Sonuçlar tahmindir (kesin el bilgisi değildir).
    /// </summary>
    public static class IhaleAnalizi
    {
        public static readonly string[] Renkler = { "Maça", "Kupa", "Karo", "Sinek" };
        private static readonly Player[] IhaleSirasi = { Player.Bati, Player.Kuzey, Player.Dogu, Player.Guney };

        private static int RenkNo(string r) { return Array.IndexOf(Renkler, r); }

        private static readonly string[] Yok = new string[0];

        /// <summary>
        /// Varsayılan (çekirdek) konvansiyonların yapay çağrıları: Stayman, Jacoby transfer, Blackwood, güçlü 2♣, Bergen / Splinter / Jacoby 2NT.
        /// null = bu çağrı çekirdek konvansiyon değil; boş dizi = yapay, renk göstermez; dolu dizi = yapay, bu renkleri gösterir.
        /// IhaleYorumcusu yalnızca ek konvansiyonları bilir, bu yüzden doğal sayılmaması için burada ayrıca ele alınır.
        /// </summary>
        private static string[] Cekirdek(List<IhaleHamlesi> g, int i, OrtaklikAnlasmasi an)
        {
            if (an == null) return null;
            var h = g[i];
            var ortak = IhaleDurumu.PartnerBul(h.Oyuncu);
            var onceki = g.Take(i).Where(x => x.GercekTeklifMi).ToList();
            var benim = onceki.Where(x => x.Oyuncu == h.Oyuncu).ToList();
            var ortagim = onceki.Where(x => x.Oyuncu == ortak).ToList();
            bool rakipYok = onceki.All(x => x.Oyuncu == h.Oyuncu || x.Oyuncu == ortak);
            string t = h.Teklif;
            int lv = h.Seviye;

            // Blackwood ve cevapları
            if (t == "4NT" && an.Blackwood && ortagim.Any(x => x.Koz != "NT")) return Yok;
            if (onceki.Count > 0 && onceki[onceki.Count - 1].Teklif == "4NT" && onceki[onceki.Count - 1].Oyuncu == ortak
                && an.Blackwood && lv == 5 && t != "5NT") return Yok;

            // Güçlü 2♣ açılışı ve 2♦ bekleme cevabı
            if (onceki.Count == 0 && t == "2♣" && an.IkiliSinekGuclu) return Yok;
            if (ortagim.Count == 1 && onceki.Count == 1 && ortagim[0].Teklif == "2♣" && an.IkiliSinekGuclu && t == "2♦") return Yok;

            if (!rakipYok) return null;

            // 1NT açılışına cevap: Stayman / Jacoby transfer
            if (ortagim.Count == 1 && benim.Count == 0 && onceki.Count == 1 && ortagim[0].Teklif == "1NT")
            {
                if (t == "2♣" && an.Stayman) return Yok;
                if (t == "2♦" && an.JacobyTransfer) return new[] { "Kupa" };
                if (t == "2♥" && an.JacobyTransfer) return new[] { "Maça" };
            }
            // 1NT açan, Stayman / transfer cevabını verirken
            if (benim.Count == 1 && ortagim.Count == 1 && onceki.Count == 2 && benim[0].Teklif == "1NT")
            {
                string c = ortagim[0].Teklif;
                if (c == "2♣" && an.Stayman && t == "2♦") return Yok;
                if (c == "2♦" && an.JacobyTransfer && t == "2♥") return Yok;
                if (c == "2♥" && an.JacobyTransfer && t == "2♠") return Yok;
            }

            // 1 majör açılışına cevap: Jacoby 2NT, Bergen 3♣/3♦, Splinter
            if (ortagim.Count == 1 && benim.Count == 0 && onceki.Count == 1 && ortagim[0].Seviye == 1
                && (ortagim[0].Koz == "Maça" || ortagim[0].Koz == "Kupa"))
            {
                string m = ortagim[0].Koz;
                if (t == "2NT" && an.Jacoby2NT) return new[] { m };
                if (an.BergenRaises && (t == "3♣" || t == "3♦")) return new[] { m };
                if (an.Splinter && lv == 4 && h.Koz != m && h.Koz != "NT") return new[] { m };
            }
            return null;
        }

        public static Dictionary<Player, KoltukYorumu> Analiz(List<string> ihale, string baslangic, Func<Player, OrtaklikAnlasmasi> anlasmaOf)
        {
            var sonuc = new Dictionary<Player, KoltukYorumu>();
            foreach (var p in IhaleSirasi) sonuc[p] = new KoltukYorumu { Koltuk = p, HcMin = 0, HcMax = 40 };
            if (ihale == null || ihale.Count == 0) return sonuc;
            Player bas;
            if (!Enum.TryParse(baslangic, out bas)) return sonuc;
            int b0 = Array.IndexOf(IhaleSirasi, bas);
            if (b0 < 0) return sonuc;

            var gecmis = new List<IhaleHamlesi>();
            for (int i = 0; i < ihale.Count; i++)
                gecmis.Add(new IhaleHamlesi { Oyuncu = IhaleSirasi[(b0 + i) % 4], Teklif = ihale[i], Sira = i + 1, GecerliMi = true });

            // ── 1) HCP aralıkları (ortağın gözüyle)
            foreach (var p in IhaleSirasi)
            {
                var y = sonuc[p];
                y.KonustuMu = gecmis.Any(h => h.Oyuncu == p);
                if (!y.KonustuMu) continue;
                try
                {
                    var an = (anlasmaOf != null ? anlasmaOf(IhaleDurumu.PartnerBul(p)) : null) ?? OrtaklikAnlasmasi.Varsayilan();
                    // Koltuğun son GERÇEK çağrısından sonraki pasları yok say: sonradan pas demek,
                    // ihalenin gidişatına bağlı olduğundan HCP için güvenilir bir üst sınır vermez.
                    int sonGercek = -1;
                    for (int i = 0; i < gecmis.Count; i++)
                        if (gecmis[i].Oyuncu == p && !gecmis[i].PasMi) sonGercek = i;
                    if (sonGercek < 0)
                    {
                        // Hiç konuşmadı, yalnız pas dedi: ancak ilk turda, kimse konuşmadan pas demişse
                        // açılış gücü (12-13+) yoktur; sonrası için bilgi yok.
                        int ilk = gecmis.FindIndex(h => h.Oyuncu == p);
                        bool oncekiHepPas = ilk >= 0 && gecmis.Take(ilk).All(h => h.PasMi);
                        y.HcMin = 0;
                        y.HcMax = oncekiHepPas ? 13 : 40;
                        continue;
                    }
                    var d = new IhaleDurumu
                    {
                        AktifOyuncu = IhaleDurumu.PartnerBul(p),
                        AktifOyuncuEli = new List<Card>(),
                        Gecmis = gecmis.Take(sonGercek + 1).ToList(),
                        Anlasma = an
                    };
                    var ar = IhaleKoruma.PartnerAraligi(d);
                    y.HcMin = Math.Max(0, ar.Min);
                    y.HcMax = Math.Min(40, ar.Max);

                    // Kalibrasyon (gerçek ellere karşı ölçüldü; PartnerAraligi ihale koruması için kaba bir tahmindir):
                    var son = gecmis[sonGercek];
                    var ortakP = IhaleDurumu.PartnerBul(p);
                    int ilkGercek = gecmis.FindIndex(h => h.GercekTeklifMi);
                    // a) Kontr: takeout / negatif / responsive kontrlar 8-10 HCP ile de yapılır.
                    if (son.KontrMi || son.SKontrMi) y.HcMin = Math.Min(y.HcMin, 8);
                    // b) Rakip açtıktan sonra ortağın doğal rengine destek (yarışmacı raise): zayıf da olabilir.
                    else if (son.GercekTeklifMi && son.Koz != "NT" && ilkGercek >= 0 && ilkGercek < sonGercek
                             && gecmis[ilkGercek].Oyuncu != p && gecmis[ilkGercek].Oyuncu != ortakP
                             && gecmis.Take(sonGercek).Any(g => g.Oyuncu == ortakP && g.GercekTeklifMi && g.Koz == son.Koz))
                        y.HcMin = Math.Min(y.HcMin, 3);
                    // c) Açıcının sonraki tekliflerinde "reverse / atlama = 16+" varsayımı bot ve insanlarda sık tutmuyor.
                    else if (ilkGercek >= 0 && ilkGercek != sonGercek && gecmis[ilkGercek].Oyuncu == p
                             && gecmis[ilkGercek].Koz != "NT" && gecmis[ilkGercek].Seviye == 1)
                        y.HcMin = Math.Min(y.HcMin, 12);
                    // d) Ortağın önleyici (2+ seviye) açılışına cevap: üst sınır yoktur (4M'e çıkış 15-19 HCP ile de olur).
                    if (ilkGercek >= 0 && ilkGercek < sonGercek && gecmis[ilkGercek].Oyuncu == ortakP && gecmis[ilkGercek].Seviye >= 2)
                        y.HcMax = 40;
                    // e) 1 majör - 3 majör: davet (limit) yükseltmesi 10-12 HCP gösterebilir.
                    else if (ilkGercek >= 0 && ilkGercek < sonGercek && gecmis[ilkGercek].Oyuncu == ortakP && gecmis[ilkGercek].Seviye == 1
                             && (gecmis[ilkGercek].Koz == "Maça" || gecmis[ilkGercek].Koz == "Kupa")
                             && son.GercekTeklifMi && son.Seviye == 3 && son.Koz == gecmis[ilkGercek].Koz)
                        y.HcMax = Math.Max(y.HcMax, 13);
                    // f) Rakip açtıktan sonra 3. seviye ve üstü müdahale: önleyici de, güçlü de olabilir.
                    else if (ilkGercek >= 0 && ilkGercek < sonGercek && gecmis[ilkGercek].Oyuncu != p && gecmis[ilkGercek].Oyuncu != ortakP
                             && son.GercekTeklifMi && son.Seviye >= 3)
                        y.HcMax = Math.Max(y.HcMax, 17);
                    if (y.HcMax < y.HcMin) y.HcMax = y.HcMin;
                }
                catch { }
            }

            // ── 2) Renk uzunlukları (çağrıları sırayla işle)
            bool birisiAcildi = false;
            var sayi = new Dictionary<Player, int[]>();
            var renkTeklif = new Dictionary<Player, bool[]>();   // koltuk bu rengi doğal olarak söyledi mi
            foreach (var p in IhaleSirasi) { sayi[p] = new int[4]; renkTeklif[p] = new bool[4]; }

            for (int i = 0; i < gecmis.Count; i++)
            {
                var h = gecmis[i];
                var y = sonuc[h.Oyuncu];
                y.SonCagri = h.Teklif;
                string[] yapay = null;
                if (h.GercekTeklifMi)
                {
                    try
                    {
                        yapay = Cekirdek(gecmis, i, anlasmaOf != null ? anlasmaOf(h.Oyuncu) : null);
                        if (yapay == null) yapay = IhaleYorumcusu.Yorumla(ihale, baslangic, i, anlasmaOf);
                    }
                    catch { yapay = null; }
                }
                y.SonAciklama = h.PasMi ? "Pas" : h.KontrMi ? "Kontr" : h.SKontrMi ? "S.Kontr"
                              : yapay != null ? "Yapay (konvansiyon)" : "Naturel";
                if (!h.GercekTeklifMi) continue;

                bool acilis = !birisiAcildi;
                birisiAcildi = true;

                if (yapay != null)
                {
                    foreach (var r in yapay) { int k = RenkNo(r); if (k >= 0) y.MinUz[k] = Math.Max(y.MinUz[k], 4); }
                    continue;
                }

                string koz = h.Koz;
                int lv = h.Seviye;
                if (koz == "NT")
                {
                    if (acilis && (h.Teklif == "1NT" || h.Teklif == "2NT")) y.Dengeli = true;
                    continue;
                }
                int rn = RenkNo(koz);
                if (rn < 0) continue;

                var ortak = IhaleDurumu.PartnerBul(h.Oyuncu);
                sayi[h.Oyuncu][rn]++;
                bool kendiRenk = renkTeklif[h.Oyuncu][rn];
                bool ortakRengi = renkTeklif[ortak][rn] && !kendiRenk;
                bool ortakKonustu = gecmis.Take(i).Any(g => g.Oyuncu == ortak && g.GercekTeklifMi);
                bool rakipKonustu = gecmis.Take(i).Any(g => g.Oyuncu != h.Oyuncu && g.Oyuncu != ortak && g.GercekTeklifMi);
                bool majorMu = rn <= 1;

                if (ortakRengi)
                {
                    // ortağın doğal rengine destek
                    y.MinUz[rn] = Math.Max(y.MinUz[rn], 3);    // ölçüm: minör desteği 3 kartla da veriliyor
                }
                else if (acilis)
                {
                    if (lv == 1 && majorMu) y.MinUz[rn] = Math.Max(y.MinUz[rn], 5);
                    else if (lv == 1) y.MinUz[rn] = Math.Max(y.MinUz[rn], 3);
                    else if (lv == 2 && h.Teklif != "2♣") y.MinUz[rn] = Math.Max(y.MinUz[rn], 6);
                    else if (lv >= 3) y.MinUz[rn] = Math.Max(y.MinUz[rn], 7);
                }
                else if (sayi[h.Oyuncu][rn] == 1)
                {
                    bool oncedenKonustu = gecmis.Take(i).Any(g => g.Oyuncu == h.Oyuncu && g.GercekTeklifMi);
                    // NT dizilerinde (1NT-2NT-3♦, Stayman sonrası vb.) yeni renk çoğunlukla yapay / soru olduğundan uzunluk varsaymayız.
                    bool ntBaglam = gecmis.Take(i).Any(g => (g.Oyuncu == h.Oyuncu || g.Oyuncu == ortak) && g.GercekTeklifMi && g.Koz == "NT");
                    if (ntBaglam) { }
                    else if (rakipKonustu && !ortakKonustu) y.MinUz[rn] = Math.Max(y.MinUz[rn], lv <= 3 ? 5 : 6);   // müdahale
                    else if (oncedenKonustu) y.MinUz[rn] = Math.Max(y.MinUz[rn], 3);                           // ikinci tur yeni renk: çoğu kez checkback / yapay
                    else y.MinUz[rn] = Math.Max(y.MinUz[rn], 4);                                                // ilk cevap
                }
                if (sayi[h.Oyuncu][rn] >= 2 && renkTeklif[ortak][rn]) y.MinUz[rn] = Math.Max(y.MinUz[rn], 4);   // ortak destekledi: oyun denemesi
                else if (sayi[h.Oyuncu][rn] >= 2) y.MinUz[rn] = Math.Max(y.MinUz[rn], 5);                        // aynı rengi tekrar: en az 5 (ölçüm: 6 çok iddialı)
                renkTeklif[h.Oyuncu][rn] = true;
            }

            // ── 3) Toplam puan = HCP + uzun renk puanları (üst sınıra en çok +2 kısa renk puanı)
            foreach (var p in IhaleSirasi)
            {
                var y = sonuc[p];
                if (!y.KonustuMu) continue;
                int dp = 0;
                for (int r = 0; r < 4; r++) if (y.MinUz[r] > 4) dp += y.MinUz[r] - 4;
                bool uzun = dp > 0;
                y.ToplamMin = y.HcMin + dp;
                y.ToplamMax = y.HcMax >= 40 ? 40 : y.HcMax + (uzun ? dp + 2 : 0);
            }

            // ── 4) Ortaklığın en iyi bilinen uyumu (alt sınırlar toplamı >= 8)
            foreach (var p in IhaleSirasi)
            {
                var y = sonuc[p];
                var o = sonuc[IhaleDurumu.PartnerBul(p)];
                if (!y.KonustuMu || !o.KonustuMu) continue;
                int en = 7, eniyi = -1;
                for (int r = 0; r < 4; r++)
                {
                    int t = y.MinUz[r] + o.MinUz[r];
                    if (t > en) { en = t; eniyi = r; }    // eşitlikte majör (önce gelen) kalır
                }
                if (eniyi >= 0 && en >= 8) y.EnIyiUyum = Renkler[eniyi];
            }
            return sonuc;
        }
    }
}
