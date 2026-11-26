using System;
using System.Linq;
using System.Collections.Generic;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Tests;

public static class AtakProbe
{
    // kullanim: AtakProbe.exe pimc(0/1)   -- bord 6: Bati 9842/43/1072/KQJ5, 3NT Guney deklaran, ihale Bati'dan
    public static int Main(string[] a)
    {
        PimcOyuncu.Aktif = a.Length > 0 && a[0] == "1";
        var el = TestHelpers.ElOlustur("9842", "43", "1072", "KQJ5");
        var ihale = new List<string> { "Pas", "Pas", "Pas", "1♣", "Pas", "1♠", "Pas", "1NT", "Pas", "2♦", "Pas", "2♥", "Pas", "3NT", "Pas", "Pas", "Pas" };
        var b = new OyunBaglami
        {
            Ben = Player.Bati, Deklaran = Player.Guney, Koz = null,
            Elim = new List<Card>(el), ElNo = 0,
            IhaleGecmisi = ihale, IhaleBaslangici = "Bati", Seviye = 3
        };
        string neden;
        var c = OyunZekasi.KartSec(b, out neden);
        Console.WriteLine("PIMC=" + PimcOyuncu.Aktif + " atak: " + OyunAnalizi.KartMetni(c) + "  neden: " + neden);
        return 0;
    }
}
