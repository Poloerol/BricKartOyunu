using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class
{
    /// <summary>Tek bir kontrat bilgisi: kontrat, löve, deklaran açısından skor, deklaran(lar).</summary>
    public sealed class KontratBilgisi
    {
        public bool Var;
        public string Kontrat = "-";   // örn. "4 Sanzatu", "5 Kupa X"
        public int Lov;
        public int Skor;               // deklaran tarafı açısından (batışta negatif)
        public string Deklaran = "-";  // "Güney" ya da "Doğu/Batı" (iki koltuk eşit löve alıyorsa)
    }

    /// <summary>
    /// İhale Analizi (Bridge Baron "Par Contract" penceresinin hesabı): çift dummy tablosundan par kontratı,
    /// en yüksek yapılan kontrat, en iyi skorlu kontrat (her iki taraf, yalnız K-G, yalnız D-B).
    /// </summary>
    public sealed class ParAnalizi
    {
        public static readonly string[] KozAdi = { "Maça", "Kupa", "Karo", "Sinek", "Sanzatu" };
        public static readonly string[] KoltukAdi = { "Kuzey", "Doğu", "Güney", "Batı" };
        /// <summary>Tabloda soldan sağa koz sırası: ♣ ♦ ♥ ♠ NT (DDS koz numarası: 0=♠ 1=♥ 2=♦ 3=♣ 4=NT).</summary>
        public static readonly int[] KozSutunu = { 3, 2, 1, 0, 4 };
        // İhale sırası (düşükten yükseğe): ♣ < ♦ < ♥ < ♠ < NT
        private static readonly int[] SiraDeger = { 3, 2, 1, 0, 4 };

        public DDTablo Tablo;
        public ParSonuc Par;
        public KontratBilgisi ParKontrati = new KontratBilgisi();
        public string AlternatifDeklaran = "-";
        public KontratBilgisi EnYuksek, EnIyiSkor, KGEnYuksek, KGEnIyiSkor, DBEnYuksek, DBEnIyiSkor;

        private static string KoltuklarMetni(DDTablo t, int koz, int taraf)
        {
            return t.Al(koz, taraf) == t.Al(koz, taraf + 2)
                ? KoltukAdi[taraf] + "/" + KoltukAdi[taraf + 2]
                : KoltukAdi[t.TarafDeklaran(koz, taraf)];
        }

        public static string KontratMetni(int seviye, int koz, bool kontrlu)
        {
            return seviye + " " + KozAdi[koz] + (kontrlu ? " X" : "");
        }

        private sealed class Aday { public int Taraf, Koz, Seviye, Lov, Skor; }

        /// <summary>Her (taraf, koz) için yapılabilen en yüksek seviyedeki kontrat ve skoru.</summary>
        private static List<Aday> Adaylar(DDTablo t, bool nsZon, bool ewZon)
        {
            var l = new List<Aday>();
            for (int taraf = 0; taraf < 2; taraf++)
                for (int koz = 0; koz < 5; koz++)
                {
                    int lov = t.TarafLove(koz, taraf);
                    int seviye = lov - 6;
                    if (seviye < 1) continue;
                    if (seviye > 7) seviye = 7;
                    l.Add(new Aday
                    {
                        Taraf = taraf, Koz = koz, Seviye = seviye, Lov = lov,
                        Skor = ParHesabi.Skor(seviye, koz, KontrDurumu.Yok, taraf == 0 ? nsZon : ewZon, lov)
                    });
                }
            return l;
        }

        private static KontratBilgisi Bilgi(DDTablo t, Aday a)
        {
            if (a == null) return new KontratBilgisi();
            return new KontratBilgisi
            {
                Var = true, Kontrat = KontratMetni(a.Seviye, a.Koz, false), Lov = a.Lov, Skor = a.Skor,
                Deklaran = KoltuklarMetni(t, a.Koz, a.Taraf)
            };
        }

        private static Aday EnYuksekAday(IEnumerable<Aday> l)
        {
            return l.OrderByDescending(a => a.Seviye).ThenByDescending(a => SiraDeger[a.Koz]).ThenByDescending(a => a.Skor).FirstOrDefault();
        }

        private static Aday EnIyiSkorAday(IEnumerable<Aday> l)
        {
            return l.OrderByDescending(a => a.Skor).ThenByDescending(a => a.Seviye).ThenByDescending(a => SiraDeger[a.Koz]).FirstOrDefault();
        }

        public static ParAnalizi Hesapla(DDTablo tablo, int dagitici, bool nsZon, bool ewZon)
        {
            var r = new ParAnalizi { Tablo = tablo };
            var aday = Adaylar(tablo, nsZon, ewZon);
            r.EnYuksek = Bilgi(tablo, EnYuksekAday(aday));
            r.EnIyiSkor = Bilgi(tablo, EnIyiSkorAday(aday));
            r.KGEnYuksek = Bilgi(tablo, EnYuksekAday(aday.Where(a => a.Taraf == 0)));
            r.KGEnIyiSkor = Bilgi(tablo, EnIyiSkorAday(aday.Where(a => a.Taraf == 0)));
            r.DBEnYuksek = Bilgi(tablo, EnYuksekAday(aday.Where(a => a.Taraf == 1)));
            r.DBEnIyiSkor = Bilgi(tablo, EnIyiSkorAday(aday.Where(a => a.Taraf == 1)));

            r.Par = ParHesabi.Hesapla(tablo, dagitici, nsZon, ewZon);
            if (!r.Par.Pas)
            {
                int declSkor = r.Par.Taraf == 0 ? r.Par.NsSkor : -r.Par.NsSkor;
                r.ParKontrati = new KontratBilgisi
                {
                    Var = true, Kontrat = KontratMetni(r.Par.Seviye, r.Par.Koz, r.Par.Kontrlu), Lov = r.Par.Lov,
                    Skor = declSkor, Deklaran = KoltukAdi[r.Par.Deklaran]
                };
                int ortak = (r.Par.Deklaran + 2) & 3;
                if (tablo.Al(r.Par.Koz, ortak) == tablo.Al(r.Par.Koz, r.Par.Deklaran)) r.AlternatifDeklaran = KoltukAdi[ortak];
            }
            else
            {
                r.ParKontrati = new KontratBilgisi { Var = false, Kontrat = "Dört pas", Lov = 0, Skor = 0, Deklaran = "-" };
            }
            return r;
        }
    }
}
