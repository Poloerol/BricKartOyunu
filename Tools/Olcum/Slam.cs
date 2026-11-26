using System;
using System.IO;
using System.Linq;
using System.Text;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;

// Slam tanı aracı: motorun güncel ihalesini çift dummy ile değerlendirir; her bord için ihale, hata türü, par ve eller yazar.
// Kullanım: Slam.exe <ilkBord> <bordSayisi> <cikti.tsv>
// Çıktı: bord TAB ihale TAB sapma TAB hata TAB par TAB eller(N|E|S|W, "S.H.D.C") TAB dagitici TAB zon
public static class Slam
{
    static string Sade(string s) { return s.Replace("♠", "S").Replace("♥", "H").Replace("♦", "D").Replace("♣", "C"); }
    static string El(ulong m)
    {
        // maske: renk*16+sıra (renk 0=S,1=H,2=D,3=C ?) — MotorKarnesi.Eller kullanıldığı biçim
        var sb = new StringBuilder();
        for (int r = 0; r < 4; r++)
        {
            for (int v = 14; v >= 2; v--) if ((m & (1UL << (r * 16 + v - 2))) != 0) sb.Append(v == 14 ? 'A' : v == 13 ? 'K' : v == 12 ? 'Q' : v == 11 ? 'J' : v == 10 ? 'T' : (char)('0' + v));
            if (r < 3) sb.Append('.');
        }
        return sb.ToString();
    }
    public static int Main(string[] a)
    {
        int ilk = int.Parse(a[0]), adet = int.Parse(a[1]);
        var mNS = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS)); var mEW = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.EW));
        var sb = new StringBuilder();
        for (int no = ilk; no < ilk + adet; no++)
        {
            try
            {
                var b = MotorKarnesi.Ihale(no, no, mNS, mEW);
                MotorKarnesi.Degerlendir(b);
                string par = b.Degerlendirme.Par.Pas ? "0" : b.Degerlendirme.Par.Metin;
                sb.AppendLine(no + "\t" + Sade(b.IhaleMetni) + "\t" + b.Degerlendirme.Sapma + "\t" + MotorKarnesi.HataAdi(b.Degerlendirme.Hata) + "\t" + Sade(par)
                    + "\t" + string.Join("|", b.Eller.Select(El)) + "\t" + b.Dagitici + "\t" + (b.NsZon ? "N" : "") + (b.EwZon ? "E" : ""));
            }
            catch { }
            if (no % 500 == 0) Console.WriteLine("..." + no);
        }
        File.WriteAllText(a[2], sb.ToString(), new UTF8Encoding(false));
        return 0;
    }
}
