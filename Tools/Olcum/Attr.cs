using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;

public static class Attr
{
    static readonly Player[] Sira = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
    static string Sade(string s) { return s.Replace("\u2660","S").Replace("\u2665","H").Replace("\u2666","D").Replace("\u2663","C"); }
    public static int Main(string[] a)
    {
        int ilk = int.Parse(a[0]), adet = int.Parse(a[1]); string outFile = a[2];
        var mNS = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS)); var mEW = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.EW));
        var sb = new StringBuilder();
        for (int no = ilk; no < ilk + adet; no++)
        {
            try {
            var deste = DagitimUretici.Uret(DagitimProfili.Rastgele, null, no);
            var eller = KonvansiyonPratigi.DestedenEller(deste);
            Player dag = KonvansiyonPratigi.Dagitici(no); int di = Array.IndexOf(Sira, dag);
            var gecmis = new List<IhaleHamlesi>(); var tk = new List<string>(); var kv = new List<string>();
            int pas = 0; bool tv = false;
            for (int i = 0; i < 60; i++)
            {
                Player p = Sira[(di + i) % 4]; bool ns = p == Player.Kuzey || p == Player.Guney; var m = ns ? mNS : mEW;
                var d = new IhaleDurumu { AktifOyuncu = p, AktifOyuncuEli = eller[p], Anlasma = m.Anlasma, Gecmis = new List<IhaleHamlesi>(gecmis) };
                string t = m.TeklifVer(d);
                gecmis.Add(new IhaleHamlesi { Oyuncu = p, Teklif = t, Sira = i + 1, GecerliMi = true });
                tk.Add(Sade(t)); kv.Add(m.SonKonvansiyonAdi ?? "-");
                if (t == "Pas") pas++; else { pas = 0; tv = true; }
                if (!tv && pas == 4) break; if (tv && pas == 3) break;
            }
            sb.AppendLine(no + "\t" + string.Join(" ", tk) + "\t" + string.Join("|", kv));
            } catch { }
        }
        File.WriteAllText(outFile, sb.ToString(), new UTF8Encoding(false));
        return 0;
    }
}
