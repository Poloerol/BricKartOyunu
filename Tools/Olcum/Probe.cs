using System;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;
using BricKartOyunu.Tests;

public static class Probe
{
    // kullanim: Probe.exe "S/H/D/C" gecmis1 gecmis2 ...   (Guney sirada; gecmis son rakip pas ile biter)
    public static int Main(string[] a)
    {
        var p = a[0].Split('/');
        var el = TestHelpers.ElOlustur(p[0], p[1], p[2], p[3]);
        var gecmis = a.Skip(1).Select(x => x.Length == 2 && "CDHS".IndexOf(x[1]) >= 0 ? x.Substring(0,1) + (x[1]=='C' ? ((char)0x2663).ToString() : x[1]=='D' ? ((char)0x2666).ToString() : x[1]=='H' ? ((char)0x2665).ToString() : ((char)0x2660).ToString()) : x).ToArray();
        var durum = TestHelpers.DurumOlustur(el, Player.Guney, gecmis);
        var m = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS));
        string t = m.TeklifVer(durum);
        Console.WriteLine(a[0] + "  [" + string.Join(" ", gecmis) + "]  ->  " + t + "   (" + (m.SonKonvansiyonAdi ?? "-") + ", " + ElDegerlendirici.HCP(el) + " HP)");
        return 0;
    }
}
