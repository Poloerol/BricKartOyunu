using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// BB ve BKO'nun aynı bordlardaki sonuçlarını karşılaştırır (IMP farkı).
// Kullanım: Ozet.exe [kiyas.tsv]
// Sütunlar (TAB ile): bord, bko_skor_NS, bb_skor_NS, kategori, not   ('#' ile başlayan satırlar yok sayılır)
//   Skorlar KUZEY-GÜNEY açısından yazılır (NS lehine +, aleyhine −). İki programda da aynı tarafta oturun.
//   kategori: ihale, atak, oynama, savunma, slam, rekabet, diger  (kayıp/kazancın asıl sebebi; boş olabilir)
public static class Ozet
{
    static readonly int[] Esik = { 0, 20, 50, 90, 130, 170, 220, 270, 320, 370, 430, 500, 600, 750, 900, 1100, 1300, 1500, 1750, 2000, 2250, 2500, 3000, 3500, 4000 };
    static int Imp(int fark)
    {
        int a = Math.Abs(fark), imp = 0;
        for (int i = 0; i < Esik.Length; i++) if (a >= Esik[i]) imp = i;
        return fark < 0 ? -imp : imp;
    }
    public static int Main(string[] args)
    {
        string yol = args.Length > 0 ? args[0] : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kiyas.tsv");
        if (!File.Exists(yol)) { Console.WriteLine("Dosya yok: " + yol); return 1; }
        var satirlar = new List<string[]>();
        foreach (var s in File.ReadAllLines(yol))
        {
            if (string.IsNullOrWhiteSpace(s) || s.TrimStart().StartsWith("#")) continue;
            var p = s.Split('\t');
            if (p.Length < 3 || p[0].ToLowerInvariant() == "bord") continue;
            int a, b;
            if (!int.TryParse(p[1].Trim(), out a) || !int.TryParse(p[2].Trim(), out b)) { Console.WriteLine("Atlandı (skor sayı değil): " + s); continue; }
            satirlar.Add(new[] { p[0].Trim(), a.ToString(), b.ToString(), p.Length > 3 ? p[3].Trim() : "", p.Length > 4 ? p[4].Trim() : "" });
        }
        if (satirlar.Count == 0) { Console.WriteLine("Hiç bord yok. kiyas.tsv'ye satır ekleyin."); return 0; }

        int top = 0, kazan = 0, kayip = 0, berabere = 0;
        var kat = new Dictionary<string, int[]>();   // [bord, toplam imp, kayıp bord sayısı]
        Console.WriteLine("Bord\tBKO\tBB\tFark(IMP)\tKategori\tNot");
        foreach (var r in satirlar)
        {
            int bko = int.Parse(r[1]), bb = int.Parse(r[2]);
            int imp = Imp(bko - bb);      // + : BKO iyi, − : BB iyi
            top += imp;
            if (imp > 0) kazan++; else if (imp < 0) kayip++; else berabere++;
            string k = r[3] == "" ? "(belirtilmedi)" : r[3];
            if (!kat.ContainsKey(k)) kat[k] = new int[3];
            kat[k][0]++; kat[k][1] += imp; if (imp < 0) kat[k][2]++;
            Console.WriteLine(r[0] + "\t" + bko + "\t" + bb + "\t" + (imp > 0 ? "+" : "") + imp + "\t" + k + "\t" + r[4]);
        }
        Console.WriteLine();
        Console.WriteLine("Bord: " + satirlar.Count + "   BKO daha iyi: " + kazan + "   BB daha iyi: " + kayip + "   beraber: " + berabere);
        Console.WriteLine("Toplam IMP (BKO − BB): " + (top > 0 ? "+" : "") + top + "   bord başına: " + ((double)top / satirlar.Count).ToString("0.00"));
        Console.WriteLine();
        Console.WriteLine("Kategori bazında (BKO − BB):");
        foreach (var kv in kat.OrderBy(x => x.Value[1]))
            Console.WriteLine("  " + kv.Key.PadRight(16) + " bord " + kv.Value[0] + "   IMP " + (kv.Value[1] > 0 ? "+" : "") + kv.Value[1] + "   BB'nin kazandığı " + kv.Value[2]);
        return 0;
    }
}
