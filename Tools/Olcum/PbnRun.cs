using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Forms;
using BricKartOyunu.Tests;

public static class PbnRun
{
    static readonly Player[] Sira = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
    static string Sade(string s) { return s.Replace("\u2660","S").Replace("\u2665","H").Replace("\u2666","D").Replace("\u2663","C"); }
    static string R(int v) { return v==14?"A":v==13?"K":v==12?"Q":v==11?"J":v==10?"T":v.ToString(); }
    static string El(List<Card> el)
    {
        var sb = new StringBuilder();
        foreach (var su in new[]{"Maça","Kupa","Karo","Sinek"})
        {
            sb.Append(su=="Maça"?"S":su=="Kupa"?"H":su=="Karo"?"D":"C");
            var k = el.Where(c=>c.Suit==su).OrderByDescending(c=>c.Value).ToList();
            if (k.Count==0) sb.Append("-"); foreach (var c in k) sb.Append(R(c.Value));
            sb.Append(' ');
        }
        return sb.ToString().Trim();
    }
    public static int Main(string[] a)
    {
        string pbn = a[0]; var secili = new HashSet<int>(a.Skip(1).Select(int.Parse));
        var mNS = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS)); var mEW = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.EW));
        var lines = File.ReadAllLines(pbn); int board = 0; string deal = null, dealer = null, vul = null, field = null, contract = null, declarer = null, result = null;
        Action bitir = () => {
            if (board > 0 && deal != null && (secili.Count == 0 || secili.Contains(board))) Calis(board, deal, dealer, vul, field, contract, declarer, result, mNS, mEW);
        };
        foreach (var raw in lines)
        {
            var l = raw.Trim();
            if (l.StartsWith("[Board ")) { bitir(); board = int.Parse(l.Split('"')[1]); deal = dealer = vul = field = contract = declarer = result = null; }
            else if (l.StartsWith("[Deal ")) deal = l.Split('"')[1];
            else if (l.StartsWith("[Dealer ")) dealer = l.Split('"')[1];
            else if (l.StartsWith("[Vulnerable ")) vul = l.Split('"')[1];
            else if (l.StartsWith("[Contract ")) contract = l.Split('"')[1];
            else if (l.StartsWith("[Declarer ")) declarer = l.Split('"')[1];
            else if (l.StartsWith("[Result ")) result = l.Split('"')[1];
            else if (l.StartsWith("% Saha sonuclari")) field = l;
        }
        bitir();
        return 0;
    }
    static void Calis(int board, string deal, string dealer, string vul, string field, string contract, string declarer, string result, IhaleMotoru mNS, IhaleMotoru mEW)
    {
        // "N:S.H.D.C S.H.D.C ..." saat yonunde N'den baslar
        string body = deal.Substring(2); var parts = body.Split(' ');
        char first = deal[0]; string order = "NESW"; int fi = order.IndexOf(first);
        var eller = new Dictionary<Player, List<Card>>();
        for (int i = 0; i < 4; i++)
        {
            var s = parts[i].Split('.'); var el = TestHelpers.ElOlustur(s[0]=="-"?"":s[0], s[1]=="-"?"":s[1], s[2]=="-"?"":s[2], s[3]=="-"?"":s[3]);
            eller[Sira[(fi + i) % 4]] = el;
        }
        int di = order.IndexOf(dealer[0]);
        var b = new KarneBord { Seed = board, BordNo = board, Ihale = new List<string>() };
        b.Dagitici = di; b.Eller = DDMotor.Maskeler(eller);
        b.NsZon = vul == "NS" || vul == "All" || vul == "Both"; b.EwZon = vul == "EW" || vul == "All" || vul == "Both";
        var gecmis = new List<IhaleHamlesi>(); int pas = 0; bool tv = false;
        for (int i = 0; i < 60; i++)
        {
            Player p = Sira[(di + i) % 4]; bool ns = p == Player.Kuzey || p == Player.Guney; var m = ns ? mNS : mEW;
            var d = new IhaleDurumu { AktifOyuncu = p, AktifOyuncuEli = eller[p], Anlasma = m.Anlasma, Gecmis = new List<IhaleHamlesi>(gecmis) };
            string t = m.TeklifVer(d);
            gecmis.Add(new IhaleHamlesi { Oyuncu = p, Teklif = t, Sira = i + 1, GecerliMi = true });
            b.Ihale.Add(t + "[" + (m.SonKonvansiyonAdi ?? "-") + "]");
            if (t == "Pas") pas++; else { pas = 0; tv = true; }
            if (!tv && pas == 4) break; if (tv && pas == 3) break;
        }
        var temiz = b.Ihale.Select(x => x.Substring(0, x.IndexOf('['))).ToList();
        b.Sonuc = IhaleSonucu.Coz(temiz, di);
        MotorKarnesi.Degerlendir(b);
        Console.WriteLine("=== Bord " + board + "  dagitici " + dealer + "  vul " + vul + "   (sahadaki kontrat: " + contract + " " + declarer + " sonuc " + result + ")");
        foreach (var p in new[]{Player.Kuzey,Player.Dogu,Player.Guney,Player.Bati}) Console.WriteLine("   " + p + ": " + El(eller[p]) + " (" + ElDegerlendirici.HCP(eller[p]) + ")");
        Console.WriteLine("   ihale: " + Sade(string.Join(" ", temiz)));
        Console.WriteLine("   konv : " + Sade(string.Join(" ", b.Ihale.Where((x, k) => temiz[k] != "Pas").Select(x => x.Replace("[", " (").Replace("]", ")")))));
        Console.WriteLine("   kontrat: " + (b.Sonuc.Pas ? "pas" : b.Sonuc.Seviye + " " + b.Sonuc.Koz + " " + b.Sonuc.Deklaran) + "  DD=" + b.Degerlendirme.DdLove + "  par=" + (b.Degerlendirme.Par.Pas ? "0" : b.Degerlendirme.Par.Metin) + "  sapma=" + b.Degerlendirme.Sapma + "  " + MotorKarnesi.HataAdi(b.Degerlendirme.Hata));
        if (field != null) Console.WriteLine("   " + field);
    }
}
