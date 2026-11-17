using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BricKartOyunu;

// Tum ihale testlerini arayuzsuz calistirir; sonuc satirlarini dosyaya yazar ve ozeti konsola basar.
// Kullanim: Testler.exe [cikti.txt]
public static class Testler
{
    public static int Main(string[] a)
    {
        var satirlar = new List<string>();
        TestRunner.SonucYaz = s => satirlar.Add(s);
        try { TestRunner.TumTestleriCalistir(); }
        catch (Exception ex) { satirlar.Add("ISTISNA: " + ex); }
        int gecen = 0, kalan = 0;
        foreach (var s in satirlar) { if (s.StartsWith("✅")) gecen++; else if (s.StartsWith("❌")) kalan++; }
        string yol = a.Length > 0 ? a[0] : "testler.txt";
        File.WriteAllLines(yol, satirlar.ToArray(), new UTF8Encoding(false));
        Console.WriteLine("gecen=" + gecen + " kalan=" + kalan + " (satir=" + satirlar.Count + ") -> " + yol);
        foreach (var s in satirlar) if (s.StartsWith("❌") || s.StartsWith("ISTISNA")) Console.WriteLine(s);
        return kalan == 0 ? 0 : 1;
    }
}
