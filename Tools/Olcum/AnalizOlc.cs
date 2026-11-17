using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;

// Oyun analizi (arayüzsüz): bir bordun ihalesini + oynanışını motorla üretir, hamle bazlı çift dummy analizini yazar.
// Kullanım: AnalizOlc.exe <bordNo> [pimcMs]   (pimcMs 0 = PIMC kapalı)
public static class AnalizOlc
{
    static readonly string[] KozAd = { "Maça", "Kupa", "Karo", "Sinek" };
    public static int Main(string[] a)
    {
        int no = int.Parse(a[0]);
        if (a.Length > 1) { int ms = int.Parse(a[1]); if (ms <= 0) PimcOyuncu.Aktif = false; else PimcOyuncu.ZamanSiniriMs = ms; }
        var mNS = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS)); var mEW = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.EW));
        var b = MotorKarnesi.Ihale(no, no, mNS, mEW);
        MotorKarnesi.Degerlendir(b);
        var eller = OynamaKarnesi.ElleriYukle(no);
        var o = OynamaKarnesi.Oyna(b, eller, k => AnlasmaYonetici.Mevcut((k == Player.Kuzey || k == Player.Guney) ? Taraf.NS : Taraf.EW));
        if (o == null) { Console.WriteLine("pas: oynanacak kontrat yok"); return 0; }
        string koz = b.Sonuc.Koz == 4 ? null : KozAd[b.Sonuc.Koz];
        Console.WriteLine("bord " + no + " ihale: " + b.IhaleMetni + "  DD=" + o.DdLove + " gercek=" + o.GercekLove);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var r = OyunAnalizi.Analiz(eller, o.Kartlar, koz);
        Console.WriteLine("analiz suresi: " + sw.ElapsedMilliseconds + " ms; baslangic DD (K-G)=" + r.BaslangicDeger + "; NS kayip=" + r.NSKayip + " (" + r.NSKayipHamle + " hamle); EW kayip=" + r.EWKayip + " (" + r.EWKayipHamle + " hamle)");
        int nsGercek = (b.Sonuc.Deklaran & 1) == 0 ? o.GercekLove : 13 - o.GercekLove;
        int beklenen = r.BaslangicDeger + r.EWKayip - r.NSKayip;
        Console.WriteLine("  tutarlilik: baslangic " + r.BaslangicDeger + " + EW kayip " + r.EWKayip + " - NS kayip " + r.NSKayip + " = " + beklenen + "  gercek NS lövesi=" + nsGercek + "  -> " + (beklenen == nsGercek ? "TUTARLI" : "TUTARSIZ"));
        foreach (var s in r.Adimlar.Where(x => x.Kayip > 0))
            Console.WriteLine("  el " + (s.ElNo + 1) + " " + s.Oyuncu + " " + OyunAnalizi.KartMetni(s.Kart) + "  kayip " + s.Kayip + "  en iyi: " + string.Join(" ", s.EnIyiler.Select(OyunAnalizi.KartMetni)));
        return 0;
    }
}
