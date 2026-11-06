using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class
{
    /// <summary>Motorun oynadığı tek bir bord: eller, ihale, çift dummy tablosu ve değerlendirme.</summary>
    public sealed class KarneBord
    {
        public int Seed;
        public int BordNo;
        public int Dagitici;                 // koltuk 0=N 1=E 2=S 3=W
        public bool NsZon, EwZon;
        public ulong[] Eller;                // [koltuk] kart maskesi (renk*16+sıra)
        public List<string> Ihale;
        public IhaleSonucu Sonuc;
        public DDTablo Tablo;
        public IhaleDegerlendirme Degerlendirme;

        public string IhaleMetni { get { return string.Join(" ", Ihale); } }
    }

    public sealed class KarneOzeti
    {
        public int Bord, ParAlan, PasPar;
        public double ToplamImpKaybi;          // deklaran tarafının IMP kaybı toplamı
        public double ToplamSapma, NsSapma, EwSapma;   // par'dan toplam sapma; sorumlu tarafa göre
        public readonly Dictionary<IhaleHatasi, int> Hatalar = new Dictionary<IhaleHatasi, int>();
        public double OrtalamaImpKaybi { get { return Bord == 0 ? 0 : ToplamSapma / Bord; } }   // bord başına par'dan sapma
        public double DdMs;
    }

    /// <summary>
    /// Motor Karnesi: motorun (iki tarafın anlaşmasıyla) oynadığı ihaleleri çift dummy par'ı ile karşılaştırır.
    /// İhaleler sırayla üretilir (motor tek iş parçacığında), çift dummy hesapları paralel yürütülür.
    /// </summary>
    public static class MotorKarnesi
    {
        private static readonly Player[] Sira = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };

        /// <summary>Tek bir bordun ihalesini motorla oynatır (çift dummy hesaplanmaz).</summary>
        public static KarneBord Ihale(int seed, int bordNo, IhaleMotoru motorNS, IhaleMotoru motorEW)
        {
            var deste = DagitimUretici.Uret(DagitimProfili.Rastgele, null, seed);
            var eller = KonvansiyonPratigi.DestedenEller(deste);
            var b = new KarneBord { Seed = seed, BordNo = bordNo, Ihale = new List<string>() };
            Player dag = KonvansiyonPratigi.Dagitici(seed);
            b.Dagitici = Array.IndexOf(Sira, dag);
            DDZon.Hesapla(bordNo, out b.NsZon, out b.EwZon);
            b.Eller = DDMotor.Maskeler(eller);

            var gecmis = new List<IhaleHamlesi>();
            int pas = 0; bool teklifVar = false;
            for (int i = 0; i < 60; i++)
            {
                Player p = Sira[(b.Dagitici + i) % 4];
                bool ns = p == Player.Kuzey || p == Player.Guney;
                var m = ns ? motorNS : motorEW;
                var d = new IhaleDurumu
                {
                    AktifOyuncu = p,
                    AktifOyuncuEli = eller[p],
                    Anlasma = m.Anlasma,
                    Gecmis = new List<IhaleHamlesi>(gecmis)
                };
                string t = m.TeklifVer(d);
                gecmis.Add(new IhaleHamlesi { Oyuncu = p, Teklif = t, Sira = i + 1, GecerliMi = true });
                b.Ihale.Add(t);
                if (t == "Pas") pas++; else { pas = 0; teklifVar = true; }
                if (!teklifVar && pas == 4) break;
                if (teklifVar && pas == 3) break;
            }
            b.Sonuc = IhaleSonucu.Coz(b.Ihale, b.Dagitici);
            return b;
        }

        /// <summary>Çift dummy tablosunu ve değerlendirmeyi hesaplar.</summary>
        public static void Degerlendir(KarneBord b)
        {
            b.Tablo = DDMotor.Hesapla(b.Eller);
            var s = b.Sonuc;
            b.Degerlendirme = IhaleDegerlendirme.Degerlendir(b.Tablo, b.Dagitici, b.NsZon, b.EwZon,
                s.Pas, s.Seviye, s.Koz, s.Deklaran, s.Kontr);
        }

        /// <summary>
        /// ilkBord'dan başlayarak adet bordu çalıştırır (seed = bord numarası). ilerleme her bord bitince çağrılır
        /// (iş parçacığı belirsiz; arayüz için Invoke kullanın).
        /// </summary>
        public static List<KarneBord> Calistir(int ilkBord, int adet, OrtaklikAnlasmasi anlasmaNS, OrtaklikAnlasmasi anlasmaEW,
                                               Action<KarneBord, int> ilerleme, CancellationToken iptal)
        {
            var motorNS = new IhaleMotoru(anlasmaNS);
            var motorEW = new IhaleMotoru(anlasmaEW);
            var bordlar = new List<KarneBord>();
            for (int i = 0; i < adet; i++)
            {
                if (iptal.IsCancellationRequested) break;
                int no = ilkBord + i;
                try { bordlar.Add(Ihale(no, no, motorNS, motorEW)); } catch { }
            }
            int bitti = 0;
            var opt = new ParallelOptions { CancellationToken = iptal, MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2) };
            bool eskiIc = DDMotor.IcParalel; DDMotor.IcParalel = false;
            try
            {
                Parallel.ForEach(bordlar, opt, b =>
                {
                    Degerlendir(b);
                    int n = Interlocked.Increment(ref bitti);
                    if (ilerleme != null) ilerleme(b, n);
                });
            }
            catch (OperationCanceledException) { }
            finally { DDMotor.IcParalel = eskiIc; }
            return bordlar.Where(b => b.Degerlendirme != null).ToList();
        }

        public static KarneOzeti Ozetle(IEnumerable<KarneBord> bordlar)
        {
            var o = new KarneOzeti();
            foreach (var b in bordlar)
            {
                if (b.Degerlendirme == null) continue;
                o.Bord++;
                var h = b.Degerlendirme.Hata;
                int n; o.Hatalar.TryGetValue(h, out n); o.Hatalar[h] = n + 1;
                int imp = b.Degerlendirme.ImpDeklaran;
                if (b.Sonuc.Pas) imp = -Math.Abs(b.Degerlendirme.ImpNs);
                if (imp < 0) o.ToplamImpKaybi += -imp;
                o.ToplamSapma += b.Degerlendirme.Sapma;
                if (b.Degerlendirme.Sorumlu == 0) o.NsSapma += b.Degerlendirme.Sapma;
                else if (b.Degerlendirme.Sorumlu == 1) o.EwSapma += b.Degerlendirme.Sapma;
                if (b.Degerlendirme.Hata == IhaleHatasi.Yok) o.ParAlan++;
                o.DdMs += b.Tablo.Ms;
            }
            return o;
        }

        public static string HataAdi(IhaleHatasi h)
        {
            switch (h)
            {
                case IhaleHatasi.Yok: return "Par";
                case IhaleHatasi.PasGecildi: return "Pas geçildi";
                case IhaleHatasi.Batis: return "Fazla yüksek (batış)";
                case IhaleHatasi.OyunKacirildi: return "Oyun kaçırıldı";
                case IhaleHatasi.SlamKacirildi: return "Slam kaçırıldı";
                case IhaleHatasi.BuyukSlamKacirildi: return "Büyük slam kaçırıldı";
                case IhaleHatasi.YanlisKoz: return "Yanlış koz";
                case IhaleHatasi.RakipFedakarlik: return "Rakip kontratı par";
                case IhaleHatasi.ParUstu: return "Rakip hatası (diğer)";
                case IhaleHatasi.RakipBuyukSlam: return "Rakip büyük slamı kaçırdı";
                case IhaleHatasi.RakipSlam: return "Rakip slamı kaçırdı";
                case IhaleHatasi.RakipOyun: return "Rakip oyunu kaçırdı";
                case IhaleHatasi.RakipFedakarlik2: return "Rakip fedakârlık yapmadı";
                case IhaleHatasi.RakipRekabet: return "Rakip rekabette geri";
                default: return "Küçük fark";
            }
        }
    }
}
