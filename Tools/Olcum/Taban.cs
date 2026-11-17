using System;
using System.IO;
using System.Text;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;

// Taban çizgisi üretir: motorun GÜNCEL halinin her bord için ihalesi ve çift dummy sapması.
// Kullanım: Taban.exe <ilkBord> <bordSayisi> <cikti.tsv>     (çıktı: bord TAB ihale TAB sapma)
// Not: her bord çift dummy ile değerlendirildiği için uzun sürer (10.000 bord için onlarca dakika).
public static class Taban
{
    static string Sade(string s) { return s.Replace("\u2660", "S").Replace("\u2665", "H").Replace("\u2666", "D").Replace("\u2663", "C"); }
    public static int Main(string[] a)
    {
        if (a.Length < 3) { Console.WriteLine("Kullanim: Taban.exe <ilkBord> <bordSayisi> <cikti.tsv>"); return 1; }
        int ilk = int.Parse(a[0]), adet = int.Parse(a[1]);
        var mNS = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS)); var mEW = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.EW));
        var sb = new StringBuilder(); int yazilan = 0;
        for (int no = ilk; no < ilk + adet; no++)
        {
            try
            {
                var b = MotorKarnesi.Ihale(no, no, mNS, mEW);
                MotorKarnesi.Degerlendir(b);
                sb.AppendLine(no + "\t" + Sade(b.IhaleMetni) + "\t" + b.Degerlendirme.Sapma);
                yazilan++;
            }
            catch { }
            if (no % 500 == 0) Console.WriteLine("..." + no);
        }
        File.WriteAllText(a[2], sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine(yazilan + " bord yazildi: " + a[2]);
        return 0;
    }
}
