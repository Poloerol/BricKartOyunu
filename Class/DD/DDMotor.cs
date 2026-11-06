using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Çift dummy tablosu: 5 koz (♠ ♥ ♦ ♣ NT) x 4 deklaran (N E S W) için deklaranın alacağı löve sayısı.
    /// Koz sırası DDS ile aynıdır (0=♠ 1=♥ 2=♦ 3=♣ 4=NT); koltuk sırası 0=N 1=E 2=S 3=W.
    /// Hesaplanamayan hücre -1'dir (yalnızca yerleşik çözücüde zaman aşımı olursa).
    /// </summary>
    public sealed class DDTablo
    {
        public readonly int[] Lov = new int[20];
        /// <summary>"DDS" ya da "Yerleşik çözücü".</summary>
        public string Kaynak;
        public double Ms;

        public DDTablo() { for (int i = 0; i < 20; i++) Lov[i] = -1; }

        public int Al(int koz, int koltuk) { return Lov[koz * 4 + koltuk]; }

        /// <summary>Taraf (0 = K-G, 1 = D-B) için o kozda iki deklarandan iyisi.</summary>
        public int TarafLove(int koz, int taraf)
        {
            return Math.Max(Al(koz, taraf), Al(koz, taraf + 2));
        }

        /// <summary>Taraf için o kozda en iyi deklaran koltuğu (eşitlikte küçük koltuk).</summary>
        public int TarafDeklaran(int koz, int taraf)
        {
            return Al(koz, taraf) >= Al(koz, taraf + 2) ? taraf : taraf + 2;
        }

        public bool Tam { get { foreach (var v in Lov) if (v < 0) return false; return true; } }
    }

    internal static class DdsNative
    {
        [DllImport("dds", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr dds_c_create_solvercontext_default();
        [DllImport("dds", CallingConvention = CallingConvention.Cdecl)]
        private static extern void dds_c_destroy_solvercontext(IntPtr ctx);
        [DllImport("dds", CallingConvention = CallingConvention.Cdecl)]
        private static extern int dds_c_calc_dd_table(IntPtr ctx, uint[] deal, int[] sonuc);

        [ThreadStatic] private static IntPtr _ctx;
        private static int _durum;   // 0 = denenmedi, 1 = var, -1 = yok
        private static readonly object _kilit = new object();
        public static string Hata;

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryW(string dosya);

        /// <summary>Windows'ta süreç mimarisine uygun dds.dll'i (Native\x86 ya da Native\x64) önceden yükler.</summary>
        private static void YoluHazirla()
        {
            try
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT) return;
                string kok = AppDomain.CurrentDomain.BaseDirectory;
                string mimari = IntPtr.Size == 8 ? "x64" : "x86";   // süreç 32 bit mi 64 bit mi: ona uygun DLL yüklenir
                string[] adaylar =
                {
                    System.IO.Path.Combine(kok, "Native", mimari, "dds.dll"),
                    System.IO.Path.Combine(kok, mimari, "dds.dll"),
                    System.IO.Path.Combine(kok, "dds.dll")
                };
                foreach (string yol in adaylar)
                {
                    if (!System.IO.File.Exists(yol)) continue;
                    if (LoadLibraryW(yol) != IntPtr.Zero) return;
                }
            }
            catch { }
        }

        public static bool Var
        {
            get
            {
                if (_durum != 0) return _durum > 0;
                lock (_kilit)
                {
                    if (_durum != 0) return _durum > 0;
                    YoluHazirla();
                    try
                    {
                        IntPtr c = dds_c_create_solvercontext_default();
                        if (c == IntPtr.Zero) { _durum = -1; Hata = "DDS bağlamı oluşturulamadı"; }
                        else { dds_c_destroy_solvercontext(c); _durum = 1; }
                    }
                    catch (Exception ex) { _durum = -1; Hata = ex.GetType().Name + ": " + ex.Message; }
                }
                return _durum > 0;
            }
        }

        /// <summary>cards[el*4 + renk]: bit 2..14 = sıra. Dönen: sonuc[koz*4 + deklaran]. Hata olursa false.</summary>
        public static bool Tablo(uint[] cards, int[] sonuc)
        {
            if (!Var) return false;
            try
            {
                if (_ctx == IntPtr.Zero) _ctx = dds_c_create_solvercontext_default();
                if (_ctx == IntPtr.Zero) return false;
                return dds_c_calc_dd_table(_ctx, cards, sonuc) == 1;
            }
            catch { return false; }
        }
    }

    /// <summary>Çift dummy tablosunu hesaplar: DDS (dds.dll) varsa onu, yoksa yerleşik C# çözücüyü kullanır.</summary>
    public static class DDMotor
    {
        /// <summary>DDS kitaplığı yüklenebiliyor mu?</summary>
        public static bool DdsVar { get { return DdsNative.Var; } }
        public static string DdsHata { get { return DdsNative.Hata; } }

        /// <summary>Test için: true ise DDS kullanılmaz.</summary>
        public static bool DdsKapali;

        /// <summary>Yerleşik çözücüde 5 kozu paralel çöz. Toplu işlerde (Motor Karnesi) kapatılır; dış döngü zaten paralel.</summary>
        public static bool IcParalel = true;

        /// <summary>Yerleşik çözücüde hücre başına zaman sınırı (ms).</summary>
        public static int YedekHucreSiniriMs = 20000;

        /// <summary>eller: [koltuk] -> kart kodları (renk*16 + sıra 0..12). Renk sırası Maça, Kupa, Karo, Sinek.</summary>
        public static DDTablo Hesapla(ulong[] eller)
        {
            var sw = Stopwatch.StartNew();
            var t = new DDTablo();
            bool ok = false;
            if (!DdsKapali && DdsNative.Var)
            {
                var cards = new uint[16];
                for (int h = 0; h < 4; h++)
                    for (int r = 0; r < 4; r++)
                        cards[h * 4 + r] = (uint)(((eller[h] >> (r * 16)) & 0x1FFF) << 2);   // sıra 0 (iki) -> bit 2
                var sonuc = new int[20];
                if (DdsNative.Tablo(cards, sonuc))
                {
                    for (int i = 0; i < 20; i++) t.Lov[i] = sonuc[i];
                    t.Kaynak = "DDS";
                    ok = true;
                }
            }
            if (!ok)
            {
                t.Kaynak = "Yerleşik çözücü";
                // her koz bağımsız (kendi tablosu) → 5 iş parçacığı
                var po = new ParallelOptions { MaxDegreeOfParallelism = IcParalel ? Math.Max(1, Environment.ProcessorCount / 2) : 1 };
                Parallel.For(0, 5, po, koz =>
                {
                    var c = new HizliDD(koz == 4 ? -1 : koz) { ZamanSiniriMs = YedekHucreSiniriMs };
                    int tahmin = -1;
                    for (int d = 0; d < 4; d++)
                    {
                        int v = c.Coz((ulong[])eller.Clone(), (d + 1) & 3, tahmin);
                        if (v < 0) { t.Lov[koz * 4 + d] = -1; continue; }
                        tahmin = v;
                        t.Lov[koz * 4 + d] = (d & 1) == 0 ? v : 13 - v;
                    }
                });
            }
            t.Ms = sw.Elapsed.TotalMilliseconds;
            return t;
        }

        public static DDTablo Hesapla(IDictionary<Player, List<Card>> eller)
        {
            return Hesapla(Maskeler(eller));
        }

        private static readonly string[] Renkler = { "Maça", "Kupa", "Karo", "Sinek" };

        public static ulong[] Maskeler(IDictionary<Player, List<Card>> eller)
        {
            var m = new ulong[4];
            Player[] sira = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            for (int k = 0; k < 4; k++)
                foreach (var c in eller[sira[k]])
                    m[k] |= 1UL << (Array.IndexOf(Renkler, c.Suit) * 16 + (c.Value - 2));
            return m;
        }

        /// <summary>Rastgele (seed'li) tam deste → 4 el maskesi (test ve toplu analiz için).</summary>
        public static ulong[] RastgeleDagitim(Random rnd)
        {
            var d = new int[52];
            for (int i = 0; i < 52; i++) d[i] = (i / 13) * 16 + i % 13;
            for (int i = 51; i > 0; i--) { int j = rnd.Next(i + 1); int t = d[i]; d[i] = d[j]; d[j] = t; }
            var h = new ulong[4];
            for (int i = 0; i < 52; i++) h[i / 13] |= 1UL << d[i];
            return h;
        }
    }
}
