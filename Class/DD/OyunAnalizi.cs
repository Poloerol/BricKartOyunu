using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BricKartOyunu.Class
{
    /// <summary>Oynanan tek bir kartın çift dummy değerlendirmesi.</summary>
    public sealed class OyunAdimi
    {
        public int ElNo;              // 0-12
        public int Sira;              // elde kaçıncı (0-3)
        public Player Oyuncu;
        public Card Kart;
        public int Kayip;             // bu kart yüzünden oynayan tarafın kaybettiği löve (çift dummy'ye göre en iyi kartla farkı)
        public List<Card> EnIyiler = new List<Card>();   // aynı en iyi değeri veren yasal kartlar (oynanan dahil olabilir)
        public int OynananDeger;      // bu karttan sonra K-G'nin (çift koltukların) alacağı toplam löve
        public int EnIyiDeger;
        public bool NSOynadi;
    }

    /// <summary>Oyun analizi toplamı.</summary>
    public sealed class OyunAnaliziSonuc
    {
        public List<OyunAdimi> Adimlar = new List<OyunAdimi>();
        public int NSKayip, EWKayip;          // tarafların hamle hatalarından toplam kaybı (löve)
        public int NSKayipHamle, EWKayipHamle; // kayıplı hamle sayısı
        public int BaslangicDeger;            // açılış atağından önce K-G'nin DD lövesi
        public bool Tamam;
    }

    /// <summary>
    /// Oyun sonrası analiz: oynanan her kartı çift dummy ile değerlendirir. Kayıp = o pozisyonda en iyi yasal kartın
    /// değeri ile oynanan kartın değeri arasındaki fark (oynayan tarafın lövesiyle). Savunmanın ve deklaranın "yanlışları"
    /// çift dummy'ye göredir: DD tüm elleri bildiğinden insan/motor için her zaman ulaşılabilir bir ölçü değildir.
    /// </summary>
    public static class OyunAnalizi
    {
        /// <param name="baslangicEller">Oyun başlamadan önceki 13'er kartlık eller.</param>
        /// <param name="oynananlar">Oynanan kartlar, oynanış sırasıyla (atak ilk kart).</param>
        /// <param name="koz">"Maça","Kupa","Karo","Sinek" ya da null (NT).</param>
        /// <param name="ilerleme">Her oynanan kart değerlendirildikçe (kart sayısı, toplam) bildirilir.</param>
        public static OyunAnaliziSonuc Analiz(Dictionary<Player, List<Card>> baslangicEller, IList<Tuple<Player, Card>> oynananlar,
                                              string koz, Action<int, int> ilerleme = null, System.Threading.CancellationToken iptal = default(System.Threading.CancellationToken))
        {
            var sonuc = new OyunAnaliziSonuc();
            if (oynananlar == null || oynananlar.Count == 0) return sonuc;

            var eller = new ulong[4];
            foreach (var kv in baslangicEller)
                foreach (var c in kv.Value) eller[PimcOyuncu.Koltuk(kv.Key)] |= 1UL << PimcOyuncu.Kod(c);

            int kozNo = koz == null ? -1 : Array.IndexOf(OyunKurallari.Renkler, koz);
            var cozucu = new DDSolver(kozNo);
            int lider = PimcOyuncu.Koltuk(oynananlar[0].Item1);

            // Açılış atağından önceki değer (K-G'nin alacağı löve): çift dummy tablosundan (deklaran = atağın sağındaki koltuk)
            int deklaranKoltuk = (lider + 3) & 3;
            int kozTablo = koz == null ? 4 : (koz == "Maça" ? 0 : koz == "Kupa" ? 1 : koz == "Karo" ? 2 : 3);
            var tablo = DDMotor.Hesapla(baslangicEller);
            int deklaranLove = tablo.Al(kozTablo, deklaranKoltuk);
            int en = (deklaranKoltuk & 1) == 0 ? deklaranLove : 13 - deklaranLove;   // K-G'nin değeri
            sonuc.BaslangicDeger = en;

            var masa = new int[4];
            var masaOyuncu = new Player[4];
            var masaKart = new Card[4];
            int k = 0, elNo = 0;
            int nsKazanilan = 0;                              // tamamlanan ellerde K-G'nin aldığı löve (çözücü kalan löveleri sayar)

            for (int i = 0; i < oynananlar.Count; i++)
            {
                if (iptal.IsCancellationRequested) return sonuc;
                var oyuncu = oynananlar[i].Item1; var kart = oynananlar[i].Item2;
                int koltuk = PimcOyuncu.Koltuk(oyuncu);
                int kod = PimcOyuncu.Kod(kart);
                bool max = (koltuk & 1) == 0;             // K-G çift koltuklar: en yüksek değeri ister; D-B en düşüğü

                // en iyi değer 'en' biliniyor (önceki kartın değeri). Oynanan kart bunu veriyor mu?
                int enG = en - nsKazanilan;                  // bu pozisyonun (kalan löveler cinsinden) en iyi değeri
                bool kartKN = cozucu.KartHedefeUlasir(eller, lider, masa, k, kod, max ? enG : enG + 1);
                bool eniyi = max ? kartKN : !kartKN;
                int degerG = enG;
                if (!eniyi)
                {
                    if (max) { degerG = enG - 1; while (degerG > 0 && !cozucu.KartHedefeUlasir(eller, lider, masa, k, kod, degerG)) degerG--; }
                    else { degerG = enG + 1; while (degerG < 13 && cozucu.KartHedefeUlasir(eller, lider, masa, k, kod, degerG + 1)) degerG++; }
                }
                int deger = degerG + nsKazanilan;

                var adim = new OyunAdimi { ElNo = elNo, Sira = k, Oyuncu = oyuncu, Kart = kart, OynananDeger = deger, EnIyiDeger = en, NSOynadi = max };
                adim.Kayip = max ? en - deger : deger - en;
                if (adim.Kayip > 0)
                {
                    // en iyi kartları bul: hedef kipinde tüm yasal kartlar (hızlı)
                    int hedef = max ? enG : enG + 1;
                    int[] kartlar, degerler;
                    cozucu.Degerler(eller, lider, masa, k, hedef, out kartlar, out degerler);
                    for (int j = 0; j < kartlar.Length; j++)
                        if (max ? degerler[j] == hedef : degerler[j] == hedef - 1) adim.EnIyiler.Add(KartOlustur(kartlar[j]));
                    if (max) { sonuc.NSKayip += adim.Kayip; sonuc.NSKayipHamle++; } else { sonuc.EWKayip += adim.Kayip; sonuc.EWKayipHamle++; }
                }
                sonuc.Adimlar.Add(adim);
                en = deger;                                  // sonraki pozisyonun en iyi değeri

                eller[koltuk] &= ~(1UL << kod);
                masa[k] = kod; masaOyuncu[k] = oyuncu; masaKart[k] = kart;
                k++;
                if (k == 4)
                {
                    var vl = new List<(Player Oyuncu, Card Kart)>();
                    for (int q = 0; q < 4; q++) vl.Add((masaOyuncu[q], masaKart[q]));
                    var kazananOyuncu = OyunKurallari.ElKazanani(vl, koz).Oyuncu;
                    lider = PimcOyuncu.Koltuk(kazananOyuncu);
                    if ((lider & 1) == 0) nsKazanilan++;
                    k = 0; elNo++;
                }
                if (ilerleme != null) ilerleme(i + 1, oynananlar.Count);
            }
            sonuc.Tamam = true;
            return sonuc;
        }

        private static Card KartOlustur(int kod)
        {
            int renkNo = kod / 16, deger = kod % 16 + 2;
            // PimcOyuncu.RenkNo ile aynı sıra: OyunKurallari.Renkler
            return new Card { Suit = OyunKurallari.Renkler[renkNo], Value = deger };
        }

        public static string KartMetni(Card c)
        {
            string r = c.Value == 14 ? "A" : c.Value == 13 ? "K" : c.Value == 12 ? "Q" : c.Value == 11 ? "J" : c.Value == 10 ? "T" : c.Value.ToString();
            string su = c.Suit == "Maça" ? "♠" : c.Suit == "Kupa" ? "♥" : c.Suit == "Karo" ? "♦" : "♣";
            return su + r;
        }
    }
}
