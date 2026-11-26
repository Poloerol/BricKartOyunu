using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;

// Görünmeyen elleri rastgele dağıtıp (Batı ♥3 attı, kupa dağılımı Batı'da kalan ♥ = rastgele) her dummy kartının ortalama löve değerini bulur.
public static class Poz2
{
    static int V(char ch) { return ch == 'A' ? 14 : ch == 'K' ? 13 : ch == 'Q' ? 12 : ch == 'J' ? 11 : ch == 'T' ? 10 : ch - '0'; }
    static ulong Maske(string renk, string k) { ulong m = 0; foreach (char c in k) m |= 1UL << PimcOyuncu.Kod(new Card { Suit = renk, Value = V(c) }); return m; }
    public static int Main(string[] a)
    {
        int n = int.Parse(a[0]);
        ulong kuz = Maske("Maça", "A953") | Maske("Kupa", "AQJ9") | Maske("Karo", "T6") | Maske("Sinek", "AQJ");
        ulong gun = Maske("Maça", "QT862") | Maske("Kupa", "5") | Maske("Karo", "AKJ87") | Maske("Sinek", "96");
        string[] renkler = { "Maça", "Kupa", "Karo", "Sinek" };
        // görünmeyen kartlar: tüm 52 - kuzey - güney - ♥3 (oynandı)
        var kalan = new List<int>();
        foreach (var r in renkler) for (int v = 2; v <= 14; v++) { int kod = PimcOyuncu.Kod(new Card { Suit = r, Value = v }); if (((kuz | gun) & (1UL << kod)) == 0) kalan.Add(kod); }
        int h3 = PimcOyuncu.Kod(new Card { Suit = "Kupa", Value = 3 }); kalan.Remove(h3);
        int bati = PimcOyuncu.Koltuk(Player.Bati), dogu = PimcOyuncu.Koltuk(Player.Dogu);
        var rnd = new Random(7);
        var toplam = new double[4]; int[] kartlar = { 14, 12, 11, 9 };
        var cz = new DDSolver(0);
        int uygun = 0;
        while (uygun < n)
        {
            var kar = kalan.OrderBy(x => rnd.Next()).ToList();
            ulong w = 0, e = 0;
            for (int i = 0; i < 12; i++) w |= 1UL << kar[i];
            for (int i = 12; i < 25; i++) e |= 1UL << kar[i];
            // Batı ♥3'ü attı: ♥ dağılımı rastgele zaten (3 dağılım bilgisi yok); doğuda ♠ yok kısıtı yok.
            var eller = new ulong[4];
            eller[PimcOyuncu.Koltuk(Player.Kuzey)] = kuz; eller[PimcOyuncu.Koltuk(Player.Guney)] = gun; eller[bati] = w; eller[dogu] = e;
            int lider = bati; var masa = new int[4]; masa[0] = h3;
            for (int ci = 0; ci < 4; ci++)
            {
                int kod = PimcOyuncu.Kod(new Card { Suit = "Kupa", Value = kartlar[ci] });
                int en = 0;
                for (int hedef = 1; hedef <= 13; hedef++) if (cz.KartHedefeUlasir(eller, lider, masa, 1, kod, hedef)) en = hedef;
                toplam[ci] += en;
            }
            uygun++;
        }
        for (int ci = 0; ci < 4; ci++) Console.WriteLine("♥" + kartlar[ci] + " ortalama K-G löve: " + (toplam[ci] / n).ToString("0.000"));
        return 0;
    }
}
