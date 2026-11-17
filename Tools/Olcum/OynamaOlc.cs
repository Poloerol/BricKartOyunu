using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;

// Oynama karnesi (arayüzsüz): ihale motorunun kontratını kart oynama motoruyla oynatır, DD löveyle karşılaştırır.
// Kullanım: OynamaOlc.exe <ilkBord> <bordSayisi> <cikti.tsv> [pimcMs] [dunya]
//   pimcMs: PIMC süre sınırı (ms; varsayılan oyun ayarı), dunya: PIMC dünya sayısı. 0 = PIMC kapalı (yalnız sezgisel kurallar).
// Çıktı: bord TAB kontrat TAB deklaran TAB atak TAB DD TAB gercek TAB fark TAB imp
public static class OynamaOlc
{
    static readonly string[] Sem = { "S", "H", "D", "C", "NT" };
    static readonly string[] Dek = { "N", "E", "S", "W" };
    public static int Main(string[] a)
    {
        if (a.Length < 3) { Console.WriteLine("Kullanim: OynamaOlc.exe <ilkBord> <bordSayisi> <cikti.tsv> [pimcMs] [dunya]"); return 1; }
        int ilk = int.Parse(a[0]), adet = int.Parse(a[1]);
        if (a.Length > 3) { int ms = int.Parse(a[3]); if (ms <= 0) PimcOyuncu.Aktif = false; else PimcOyuncu.ZamanSiniriMs = ms; }
        if (a.Length > 4) PimcOyuncu.DunyaSayisi = int.Parse(a[4]);
        var mNS = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS)); var mEW = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.EW));
        var sonuc = new List<OynamaBordu>(); var sb = new StringBuilder();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int no = ilk; no < ilk + adet; no++)
        {
            try
            {
                var b = MotorKarnesi.Ihale(no, no, mNS, mEW);
                MotorKarnesi.Degerlendir(b);
                var eller = OynamaKarnesi.ElleriYukle(no);
                var o = OynamaKarnesi.Oyna(b, eller, k => AnlasmaYonetici.Mevcut((k == Player.Kuzey || k == Player.Guney) ? Taraf.NS : Taraf.EW));
                if (o == null) continue;
                sonuc.Add(o);
                sb.AppendLine(no + "\t" + o.Sonuc.Seviye + Sem[o.Sonuc.Koz] + "\t" + Dek[o.Sonuc.Deklaran] + "\t" + o.AtakKarti + "\t" + o.DdLove + "\t" + o.GercekLove + "\t" + o.Fark + "\t" + o.Imp);
            }
            catch (Exception ex) { Console.WriteLine("bord " + no + ": " + ex.Message); }
            if ((no - ilk + 1) % 10 == 0) Console.WriteLine("..." + (no - ilk + 1) + " bord, " + (sw.ElapsedMilliseconds / 1000) + " sn");
        }
        File.WriteAllText(a[2], sb.ToString(), new UTF8Encoding(false));
        var oz = OynamaKarnesi.Ozetle(sonuc, adet);
        Console.WriteLine("bord=" + adet + " oynanan=" + oz.Oynanan + " tam=" + oz.Tam + " deklaranEksik=" + oz.Eksik + " savunmaEksik=" + oz.Fazla);
        Console.WriteLine("ortalama fark (gercek-DD löve)=" + oz.OrtalamaFark.ToString("0.000") + "  ortalama IMP (deklaran gözüyle)=" + oz.OrtalamaImp.ToString("0.00") + "  süre=" + (sw.ElapsedMilliseconds / 1000) + " sn");
        return 0;
    }
}
