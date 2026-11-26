using System;
using System.Collections.Generic;
using BricKartOyunu.Class;

// Tek pozisyonun çift dummy değerleri: 4♠ Güney deklaran, Batı ♥3 attı, sıra Kuzey'de (dummy).
public static class Poz
{
    static ulong El(string s, string h, string d, string c)
    {
        ulong m = 0;
        Action<string, string> ekle = (renk, kartlar) => { foreach (char ch in kartlar) { int v = ch == 'A' ? 14 : ch == 'K' ? 13 : ch == 'Q' ? 12 : ch == 'J' ? 11 : ch == 'T' ? 10 : ch - '0'; m |= 1UL << PimcOyuncu.Kod(new Card { Suit = renk, Value = v }); } };
        ekle("Maça", s); ekle("Kupa", h); ekle("Karo", d); ekle("Sinek", c);
        return m;
    }
    public static int Main(string[] a)
    {
        var eller = new ulong[4];
        eller[PimcOyuncu.Koltuk(Player.Kuzey)] = El("A953", "AQJ9", "T6", "AQJ");
        eller[PimcOyuncu.Koltuk(Player.Guney)] = El("QT862", "5", "AKJ87", "96");
        eller[PimcOyuncu.Koltuk(Player.Bati)] = El("KJ74", "32", "Q95", "T732");
        eller[PimcOyuncu.Koltuk(Player.Dogu)] = El("", "KT8764", "432", "K854");
        int lider = PimcOyuncu.Koltuk(Player.Bati);
        int h3 = PimcOyuncu.Kod(new Card { Suit = "Kupa", Value = 3 });
        eller[lider] &= ~(1UL << h3);
        var masa = new int[4]; masa[0] = h3;
        int kozNo = Array.IndexOf(OyunKurallari.Renkler, "Maça");
        var cz = new DDSolver(kozNo);
        foreach (int v in new[] { 14, 12, 11, 9 })
        {
            int kod = PimcOyuncu.Kod(new Card { Suit = "Kupa", Value = v });
            int en = 0;
            for (int hedef = 1; hedef <= 13; hedef++) if (cz.KartHedefeUlasir(eller, lider, masa, 1, kod, hedef)) en = hedef;
            Console.WriteLine("Kuzey ♥" + v + " -> K-G toplam löve (bu elden sonra kalan 13) = " + en);
        }
        return 0;
    }
}
