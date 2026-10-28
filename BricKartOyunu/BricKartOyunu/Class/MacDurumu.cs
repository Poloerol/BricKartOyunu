using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BricKartOyunu.Class
{
    public enum MacPuanlama { IMP, BordaMac, ToplamPuan }

    /// <summary>
    /// Briç Maçı'nın o anki durumu: seçenekler, kaçıncı bordda olunduğu ve bord sonuçları.
    /// Yalnızca bellekte tutulur; maç bitince (ya da oyun penceresi kapanınca) Bitir() ile sıfırlanır
    /// ve bord sayısı standart değere (4) döner.
    /// </summary>
    /// <summary>Maçtaki tek bir bordun skor formunda gösterilen kaydı.</summary>
    public sealed class MacBordKaydi
    {
        public int Sira;
        /// <summary>Bordun dağıtım tohumu ("El No").</summary>
        public int Seed;
        /// <summary>"2NT, Batı" gibi; dört pas ise "Pas".</summary>
        public string Kontrat = "Pas";
        /// <summary>Deklaranın aldığı löve; kontrat yoksa -1.</summary>
        public int Love = -1;
        /// <summary>Kuzey-Güney tarafının skoru.</summary>
        public int NsSkor;
    }

    public static class MacDurumu
    {
        public const int VarsayilanBordSayisi = 4;
        /// <summary>Bord numaraları (kart dağıtıcı + zon döngüsü) 16'dan sonra 1'e döner.</summary>
        public const int EnFazlaBordNo = 16;

        public static bool Aktif { get; private set; }
        public static string OyuncuAdi { get; private set; } = "İnsan Oyuncu";
        /// <summary>Maçın bord sayısı. Maç bitince / iptal edilince 4'e döner.</summary>
        public static int BordSayisi { get; private set; } = VarsayilanBordSayisi;
        /// <summary>Maçtaki kaçıncı bord (1 tabanlı).</summary>
        public static int Sira { get; private set; } = 1;
        public static bool TopluTekrar { get; private set; }
        public static bool SeedRastgele { get; private set; } = true;
        public static int Seed { get; private set; }
        public static MacPuanlama Puanlama { get; private set; } = MacPuanlama.IMP;

        // Maç sırası → K-G tarafının skoru
        private static readonly Dictionary<int, int> _sonuclar = new Dictionary<int, int>();
        public static IReadOnlyDictionary<int, int> Sonuclar { get { return _sonuclar; } }

        // Maç sırası → skor formunda gösterilecek ayrıntılı kayıt
        private static readonly Dictionary<int, MacBordKaydi> _kayitlar = new Dictionary<int, MacBordKaydi>();

        /// <summary>Oynanan bordların kayıtları, bord sırasına göre.</summary>
        public static IList<MacBordKaydi> Kayitlar()
        {
            return _kayitlar.Values.OrderBy(k => k.Sira).ToList();
        }

        /// <summary>Maç son bordu bitirdi mi (sonraki bord yok)?</summary>
        public static bool SonBord { get { return Aktif && Sira >= BordSayisi; } }

        public static void Baslat(string oyuncuAdi, int bordSayisi, bool topluTekrar, bool seedRastgele, int seed, MacPuanlama puanlama)
        {
            _sonuclar.Clear();
            _kayitlar.Clear();
            Aktif = true;
            OyuncuAdi = string.IsNullOrWhiteSpace(oyuncuAdi) ? "İnsan Oyuncu" : oyuncuAdi.Trim();
            BordSayisi = bordSayisi < 1 ? VarsayilanBordSayisi : bordSayisi;
            Sira = 1;
            TopluTekrar = topluTekrar;
            SeedRastgele = seedRastgele;
            Seed = seed;
            Puanlama = puanlama;
        }

        /// <summary>Maçı kapatır ve bord sayısını standart değere (4) döndürür.</summary>
        public static void Bitir()
        {
            Aktif = false;
            BordSayisi = VarsayilanBordSayisi;
            Sira = 1;
            SeedRastgele = true;
            Seed = 0;
            TopluTekrar = false;
            Puanlama = MacPuanlama.IMP;
            _sonuclar.Clear();
            _kayitlar.Clear();
        }

        /// <summary>Şimdiki bordun sonucu (K-G tarafının skoru). Aynı bord tekrar oynanırsa sonuç değişir.</summary>
        public static void Tamamla(int nsSkor)
        {
            if (Aktif) _sonuclar[Sira] = nsSkor;
        }

        /// <summary>Şimdiki bordun sonucu ve skor formu ayrıntıları (kontrat, löve, tohum).</summary>
        public static void Tamamla(int nsSkor, string kontrat, int love, int seed)
        {
            if (!Aktif) return;
            _sonuclar[Sira] = nsSkor;
            _kayitlar[Sira] = new MacBordKaydi { Sira = Sira, Seed = seed, Kontrat = kontrat ?? "Pas", Love = love, NsSkor = nsSkor };
        }

        /// <summary>Bord için henüz sonuç yoksa yazar (dört pas / bordu atlama için).</summary>
        public static void SonucYoksaYaz(int nsSkor)
        {
            if (Aktif && !_sonuclar.ContainsKey(Sira)) _sonuclar[Sira] = nsSkor;
        }

        /// <summary>Dört pas / atlanan bord: skor 0, kontrat "Pas".</summary>
        public static void PasYaz(int seed)
        {
            if (Aktif && !_sonuclar.ContainsKey(Sira)) Tamamla(0, "Pas", -1, seed);
        }

        public static bool SonrakiBordVar { get { return Aktif && Sira < BordSayisi; } }

        public static void Ilerle() { if (SonrakiBordVar) Sira++; }

        /// <summary>Şimdiki bordun tohumu (yalnızca "Seçilen" tohum seçiliyse anlamlı): ilk bord = Seed, sonrakiler +1.</summary>
        public static int BordSeed() { return Seed + Sira - 1; }

        /// <summary>Bord numarasının 16'dan sonra 1'e dönen sıradaki değeri.</summary>
        public static int SonrakiBordNo(int mevcut)
        {
            return mevcut >= EnFazlaBordNo || mevcut < 0 ? 1 : mevcut + 1;
        }

        // ───────────────────────── Puanlama ─────────────────────────

        private static readonly int[] ImpEsikleri =
            { 0, 20, 50, 90, 130, 170, 220, 270, 320, 370, 430, 500, 600, 750, 900, 1100, 1300, 1500, 1750, 2000, 2250, 2500, 3000, 3500, 4000 };

        /// <summary>Standart IMP tablosu: puan farkı → IMP (işaret korunur).</summary>
        public static int Imp(int fark)
        {
            int m = Math.Abs(fark);
            int imp = ImpEsikleri.Count(e => e <= m) - 1;
            return fark < 0 ? -imp : imp;
        }

        /// <summary>IMP tablosunun bir satırı: IMP değeri ve puan farkı aralığı (ust &lt; 0 → sınırsız).</summary>
        public static void ImpAraligi(int imp, out int alt, out int ust)
        {
            alt = ImpEsikleri[imp];
            ust = imp + 1 < ImpEsikleri.Length ? ImpEsikleri[imp + 1] - 10 : -1;
            if (imp == 0) alt = 0;
        }

        public static int ImpSayisi { get { return ImpEsikleri.Length; } }

        /// <summary>Skor formundaki üçüncü sütun için bord değeri: IMP, BAM (1/½/0) ya da skor.</summary>
        public static string BordDegeri(int ns)
        {
            switch (Puanlama)
            {
                case MacPuanlama.BordaMac: return ns > 0 ? "1" : ns == 0 ? "½" : "0";
                case MacPuanlama.ToplamPuan: return ns.ToString();
                default: return Imp(ns).ToString();
            }
        }

        /// <summary>Maç özeti. IMP ve Board-a-Match için karşılaştırma masası olmadığından 0 puana (ortalamaya) göre hesaplanır.</summary>
        public static string OzetMetni()
        {
            var sb = new StringBuilder();
            sb.AppendLine(OyuncuAdi + " — " + BordSayisi + " bordluk maç bitti.");
            sb.AppendLine();
            int topNS = 0, imp = 0;
            double bam = 0;
            int oynanan = 0;
            for (int i = 1; i <= BordSayisi; i++)
            {
                int ns;
                if (!_sonuclar.TryGetValue(i, out ns)) { sb.AppendLine("Bord " + i + ": oynanmadı"); continue; }
                oynanan++;
                topNS += ns;
                imp += Imp(ns);
                bam += ns > 0 ? 1 : ns == 0 ? 0.5 : 0;
                sb.AppendLine("Bord " + i + ": K-G " + (ns > 0 ? "+" : "") + ns);
            }
            sb.AppendLine();
            switch (Puanlama)
            {
                case MacPuanlama.ToplamPuan:
                    sb.AppendLine("Toplam puan: K-G " + (topNS > 0 ? "+" : "") + topNS + "   D-B " + (-topNS > 0 ? "+" : "") + (-topNS));
                    break;
                case MacPuanlama.BordaMac:
                    sb.AppendLine("Board-a-Match: K-G " + bam.ToString("0.#") + " — D-B " + (oynanan - bam).ToString("0.#"));
                    sb.AppendLine("(Karşılaştırma masası olmadığından 0 puana göre hesaplandı.)");
                    break;
                default:
                    sb.AppendLine("IMP: K-G " + (imp > 0 ? "+" : "") + imp + "   D-B " + (-imp > 0 ? "+" : "") + (-imp));
                    sb.AppendLine("(Karşılaştırma masası olmadığından 0 puana göre hesaplandı.)");
                    break;
            }
            return sb.ToString().TrimEnd();
        }
    }
}
