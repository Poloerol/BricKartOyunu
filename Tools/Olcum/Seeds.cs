using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;

public static class Seeds
{
    static string Sade(string s) { return s.Replace("\u2660","S").Replace("\u2665","H").Replace("\u2666","D").Replace("\u2663","C"); }
    static string R(int v) { return v==14?"A":v==13?"K":v==12?"Q":v==11?"J":v==10?"T":v.ToString(); }
    static string El(List<Card> el)
    {
        var sb = new StringBuilder();
        foreach (var su in new[]{"Maça","Kupa","Karo","Sinek"})
        {
            sb.Append(su=="Maça"?"S":su=="Kupa"?"H":su=="Karo"?"D":"C");
            foreach (var c in el.Where(c=>c.Suit==su).OrderByDescending(c=>c.Value)) sb.Append(R(c.Value));
            sb.Append(' ');
        }
        return sb.ToString().Trim();
    }
    public static int Main(string[] a)
    {
        var mNS = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS)); var mEW = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.EW));
        for (int i = 0; i + 1 < a.Length; i += 2)
        {
            int seed = int.Parse(a[i]), tur = int.Parse(a[i+1]);
            var deste = DagitimUretici.Uret(DagitimProfili.Rastgele, null, seed);
            var eller = KonvansiyonPratigi.DestedenEller(deste);
            Console.WriteLine("=== seed " + seed + " tur " + tur + " dagitici " + KonvansiyonPratigi.Dagitici(seed));
            foreach (var p in new[]{Player.Kuzey,Player.Dogu,Player.Guney,Player.Bati}) Console.WriteLine("  " + p + ": " + El(eller[p]) + "  (" + ElDegerlendirici.HCP(eller[p]) + " HP)");
            var b = MotorKarnesi.Ihale(seed, tur, mNS, mEW);
            MotorKarnesi.Degerlendir(b);
            Console.WriteLine("  ihale: " + Sade(b.IhaleMetni));
            Console.WriteLine("  kontrat: " + (b.Sonuc.Pas ? "pas" : b.Sonuc.Seviye + " " + b.Sonuc.Koz + " " + b.Sonuc.Deklaran) + "  DD=" + b.Degerlendirme.DdLove + "  par=" + (b.Degerlendirme.Par.Pas ? "0" : b.Degerlendirme.Par.Metin) + "  sapma=" + b.Degerlendirme.Sapma + "  " + MotorKarnesi.HataAdi(b.Degerlendirme.Hata));
        }
        return 0;
    }
}
