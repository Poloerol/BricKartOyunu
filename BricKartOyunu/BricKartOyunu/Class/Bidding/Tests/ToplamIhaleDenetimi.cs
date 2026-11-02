using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Tests
{
    /// <summary>
    /// Toplu ihale denetimi: rastgele dağıtımları dört el de ihale motoruyla oynatır ve "şüpheli" ihaleleri işaretler.
    /// Beklenen sonuç bulmaz; sistematik hataları görünür kılar (ör. cevabı olmayan yapay teklif, kaçan majör uyumu).
    /// Her kategori, hatalı son teklifi veren konvansiyonun adıyla gruplanır. Bazı işaretler doğal ihaleleri de
    /// yakalayabilir; örnekleri okuyup karar verin. Çıktı satırları ✅/❌ içermez, test sayaçlarını etkilemez.
    /// Kullanım: ToplamIhaleDenetimi.Calistir(5000, mesaj => ...)
    /// </summary>
    public static class ToplamIhaleDenetimi
    {
        private static readonly string[] Renkler = { "Maça", "Kupa", "Karo", "Sinek" };

        private static string Sem(string s) { return s == "Maça" ? "♠" : s == "Kupa" ? "♥" : s == "Karo" ? "♦" : "♣"; }

        private static string El(List<Card> e)
        {
            var p = new List<string>();
            foreach (var s in Renkler)
            {
                var k = e.Where(c => c.Suit == s).OrderByDescending(c => c.Value)
                         .Select(c => c.Value == 14 ? "A" : c.Value == 13 ? "K" : c.Value == 12 ? "Q" : c.Value == 11 ? "J" : c.Value == 10 ? "T" : c.Value.ToString());
                p.Add(Sem(s) + string.Join("", k));
            }
            return string.Join(" ", p);
        }

        public static void Calistir(int dagitimSayisi, Action<string> yaz)
        {
            var koltuk = new[] { Player.Guney, Player.Bati, Player.Kuzey, Player.Dogu };
            var anlasma = OrtaklikAnlasmasi.Varsayilan();
            var say = new Dictionary<string, int>();
            var ornek = new Dictionary<string, List<string>>();
            Action<string, string> isle = (k, o) =>
            {
                if (!say.ContainsKey(k)) { say[k] = 0; ornek[k] = new List<string>(); }
                say[k]++;
                if (ornek[k].Count < 3) ornek[k].Add(o);
            };
            int istisna = 0;

            for (int seed = 1; seed <= dagitimSayisi; seed++)
            {
                var r = new Random(seed * 7919);
                var deste = new List<Card>();
                foreach (var s in Renkler) for (int v = 2; v <= 14; v++) deste.Add(new Card { Suit = s, Value = v });
                deste = deste.OrderBy(x => r.Next()).ToList();
                var eller = new Dictionary<Player, List<Card>>();
                for (int i = 0; i < 4; i++) eller[koltuk[i]] = deste.Skip(i * 13).Take(13).ToList();
                int dagitici = seed % 4;
                try
                {
                    var motor = new IhaleMotoru(anlasma);
                    var d = new IhaleDurumu { AktifOyuncu = koltuk[dagitici], Anlasma = anlasma };
                    var liste = new List<string>();
                    string sonKn = "?"; int g = 0;
                    while (!d.IhaleBittiMi() && g++ < 80)
                    {
                        d.AktifOyuncuEli = eller[d.AktifOyuncu];
                        string t = motor.TeklifVer(d);
                        string kn = motor.SonKonvansiyonAdi ?? "?";
                        liste.Add(d.AktifOyuncu.ToString().Substring(0, 1) + ":" + t);
                        if (t != "Pas") sonKn = kn;
                        d.HamleEkle(t);
                    }
                    string tanim = "#" + seed + " D=" + koltuk[dagitici] + " " + string.Join(" ", liste) + "\n    K " + El(eller[Player.Kuzey]) +
                                   " | G " + El(eller[Player.Guney]) + " | B " + El(eller[Player.Bati]) + " | D " + El(eller[Player.Dogu]);
                    if (g >= 80) { isle("çok uzun / bitmeyen ihale", tanim); continue; }
                    var gercek = d.Gecmis.Where(h => h.GercekTeklifMi).ToList();
                    if (gercek.Count == 0) continue;

                    var son = gercek[gercek.Count - 1];
                    Player ort = son.Oyuncu;
                    Player pr = ort == Player.Kuzey ? Player.Guney : ort == Player.Guney ? Player.Kuzey : ort == Player.Bati ? Player.Dogu : Player.Bati;
                    int hcp = ElDegerlendirici.HCP(eller[ort]) + ElDegerlendirici.HCP(eller[pr]);
                    string koz = son.Koz; int sev = son.Seviye;

                    if (koz != "NT" && sev >= 3)
                    {
                        int uz = eller[ort].Count(c => c.Suit == koz) + eller[pr].Count(c => c.Suit == koz);
                        if (uz < 8) isle("kozda 8 karttan az uyum, seviye 3+ [" + sonKn + "]", tanim);
                    }
                    int gerek = sev >= 6 ? 30 : (sev >= 4 ? 23 : 0);
                    if (gerek > 0 && hcp < gerek - 3 && sev >= 4) isle("seviye 4+ için çok zayıf toplam HCP [" + sonKn + "]", tanim);
                    foreach (var m in new[] { "Maça", "Kupa" })
                    {
                        int uz = eller[ort].Count(c => c.Suit == m) + eller[pr].Count(c => c.Suit == m);
                        if (uz >= 9 && koz != m && sev >= 2 && hcp >= 24) isle("9+ kartlık majör uyumu kaçtı [" + sonKn + "]", tanim);
                    }
                    int ns = ElDegerlendirici.HCP(eller[Player.Kuzey]) + ElDegerlendirici.HCP(eller[Player.Guney]);
                    if (ns >= 27 && (ort == Player.Bati || ort == Player.Dogu || sev < 3)) isle("27+ HCP'li taraf oyuna gidemedi [" + sonKn + "]", tanim);
                    // Açıcının yapay 3♦ (Bergen) bidine cevapsız kalması
                    if (sev == 3 && koz == "Karo" && gercek.Count >= 2 && gercek[0].Oyuncu != ort &&
                        (gercek[0].Teklif == "1♥" || gercek[0].Teklif == "1♠") && gercek.Count(h => h.Oyuncu == ort) == 1)
                        isle("yapay 3♦ ile bitti [" + sonKn + "]", tanim);
                }
                catch (Exception e)
                {
                    istisna++;
                    isle("İSTİSNA: " + e.GetType().Name + " " + e.Message.Split('\n')[0], "#" + seed);
                }
            }

            yaz("════════════════════════════════════════");
            yaz("TOPLU İHALE DENETİMİ (bilgi amaçlı): " + dagitimSayisi + " dağıtım, istisna: " + istisna);
            yaz("════════════════════════════════════════");
            foreach (var k in say.OrderByDescending(x => x.Value).Take(25))
            {
                yaz("▸ " + k.Key + ": " + k.Value + " (%" + (100.0 * k.Value / dagitimSayisi).ToString("0.0") + ")");
                foreach (var o in ornek[k.Key]) yaz("    " + o);
            }
            if (say.Count == 0) yaz("Şüpheli ihale bulunmadı.");
        }
    }
}
