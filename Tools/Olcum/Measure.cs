using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;

public static class Measure
{
    static string Sade(string s) { return s.Replace("\u2660","S").Replace("\u2665","H").Replace("\u2666","D").Replace("\u2663","C"); }
    public static int Main(string[] a)
    {
        string baseFile = a[0], outFile = a[1];
        var bs = new Dictionary<int, KeyValuePair<string,int>>();
        foreach (var l in File.ReadAllLines(baseFile)) { var f = l.Split('\t'); bs[int.Parse(f[0])] = new KeyValuePair<string,int>(f[1], int.Parse(f[2])); }
        var mNS = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS)); var mEW = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.EW));
        var degisen = new List<KarneBord>(); int toplam = 0;
        foreach (var no in bs.Keys.OrderBy(x => x))
        {
            KarneBord b; try { b = MotorKarnesi.Ihale(no, no, mNS, mEW); } catch { continue; }
            toplam++;
            if (Sade(b.IhaleMetni) != bs[no].Key) degisen.Add(b);
        }
        Console.WriteLine("bord=" + toplam + " degisen=" + degisen.Count);
        var sb = new StringBuilder(); long eski = 0, yeni = 0; int iy = 0, ko = 0, ayni = 0;
        foreach (var b in degisen)
        {
            MotorKarnesi.Degerlendir(b);
            int es = bs[b.BordNo].Value, ys = b.Degerlendirme.Sapma;
            eski += es; yeni += ys; if (ys < es) iy++; else if (ys > es) ko++; else ayni++;
            sb.AppendLine(b.BordNo + "\t" + bs[b.BordNo].Key + "\t" + Sade(b.IhaleMetni) + "\t" + es + "\t" + ys + "\t" + MotorKarnesi.HataAdi(b.Degerlendirme.Hata));
        }
        File.WriteAllText(outFile, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine("degisen bordlarda toplam sapma: eski=" + eski + " yeni=" + yeni + " | iyilesen=" + iy + " kotulesen=" + ko + " ayni=" + ayni);
        return 0;
    }
}
