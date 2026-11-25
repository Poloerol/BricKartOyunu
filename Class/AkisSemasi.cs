using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Bridge Baron akış şeması dosyası (.fv) okuyucusu.
    ///
    /// Dosya düzeni (her satır CRLF, metinler İngilizce):
    ///   kısa ad / başlık / giriş satırı sayısı N / N satır giriş metni /
    ///   kutu sayısı K / K kutu (etiket satırı sayısı, etiketler, açıklama satırı sayısı, açıklamalar, boş satır) /
    ///   düzen satırı sayısı R / R satır: "kutuNo x y evetKonum hayırKonum çizim çizim" (konum = bu listedeki sıra, -1 = yok).
    /// Düzen listesinin ilk satırı kökdür; bir kutu birden fazla yoldan gelebilir (ağaç değil, yönlü graf).
    /// Metinde $C $D $H $S renk simgeleri; #A #B gibi kodlar anlaşma seçeneğidir (olduğu gibi bırakılır).
    /// </summary>
    public class AkisSemasi
    {
        public class Kutu
        {
            public string Orijinal;      // çevrilmemiş İngilizce etiket (otomatik yol bulucu soruyu buradan tanır)
            public string Etiket;        // kısa başlık (satırlar boşlukla birleştirilmiş)
            public string Aciklama;      // açıklama (paragraflar \n ile ayrılmış)
            public string Hedef;         // başka bir şemaya geçiş: hedef şemanın kısa adı (yoksa null)
        }

        public class Dugum
        {
            public Kutu Kutu;
            public Dugum Evet;           // null = yok
            public Dugum Hayir;
            public bool Yaprak { get { return Evet == null && Hayir == null; } }
        }

        public string DosyaAdi;
        public string KisaAd;
        public string Baslik;
        public string Giris;             // giriş metni (paragraflar \n ile ayrılmış)
        public Dugum Kok;

        /// <summary>Dosyayı okur; bozuksa istisna fırlatır.</summary>
        public static AkisSemasi Yukle(string yol)
        {
            string[] s = File.ReadAllLines(yol, Encoding.GetEncoding(1252));
            int i = 0;
            Func<string> Sat = delegate
            {
                if (i >= s.Length) throw new InvalidDataException("Dosya beklenenden erken bitti: " + Path.GetFileName(yol));
                return s[i++];
            };
            Func<int> Say = delegate
            {
                int v;
                if (!int.TryParse(Sat().Trim(), out v)) throw new InvalidDataException("Sayı bekleniyordu (satır " + i + "): " + Path.GetFileName(yol));
                return v;
            };

            var sema = new AkisSemasi();
            sema.DosyaAdi = Path.GetFileNameWithoutExtension(yol);
            sema.KisaAd = Sat().Trim();
            sema.Baslik = Cevir(Sat().Trim());
            int girisSatir = Say();
            var giris = new List<string>();
            for (int k = 0; k < girisSatir; k++) giris.Add(Sat());
            sema.Giris = CevirParagraflar(Paragraflar(giris));

            int kutuSayisi = Say();
            var kutular = new List<Kutu>();
            for (int k = 0; k < kutuSayisi; k++)
            {
                int e = Say();
                var etiket = new List<string>();
                for (int j = 0; j < e; j++) etiket.Add(Sat().Trim());
                int a = Say();
                var acik = new List<string>();
                for (int j = 0; j < a; j++) acik.Add(Sat());
                // "Switch Flowcharts" kutusu: açıklamadan sonra hedef şemanın kısa adı gelir (sayı olmayan satır)
                string hedef = null;
                int tmp;
                if (i < s.Length && s[i].Trim().Length > 0 && !int.TryParse(s[i].Trim(), out tmp)) hedef = s[i++].Trim();
                if (i < s.Length && s[i].Trim().Length == 0) i++;      // kutu ayırıcı boş satır
                string orijinalEtiket = string.Join(" ", etiket.Where(x => x.Length > 0));
                kutular.Add(new Kutu { Orijinal = orijinalEtiket, Etiket = Cevir(orijinalEtiket), Aciklama = CevirParagraflar(Paragraflar(acik)), Hedef = hedef });
            }

            int satirSayisi = Say();
            var no = new int[satirSayisi];
            var evet = new int[satirSayisi];
            var hayir = new int[satirSayisi];
            for (int k = 0; k < satirSayisi; k++)
            {
                string[] p = Sat().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length < 5) throw new InvalidDataException("Düzen satırı bozuk (satır " + i + "): " + Path.GetFileName(yol));
                no[k] = int.Parse(p[0]); evet[k] = int.Parse(p[3]); hayir[k] = int.Parse(p[4]);
                if (no[k] < 0 || no[k] >= kutular.Count) throw new InvalidDataException("Geçersiz kutu no " + no[k] + ": " + Path.GetFileName(yol));
            }

            var dugumler = new Dugum[satirSayisi];
            for (int k = 0; k < satirSayisi; k++) dugumler[k] = new Dugum { Kutu = kutular[no[k]] };
            for (int k = 0; k < satirSayisi; k++)
            {
                if (evet[k] >= 0 && evet[k] < satirSayisi) dugumler[k].Evet = dugumler[evet[k]];
                if (hayir[k] >= 0 && hayir[k] < satirSayisi) dugumler[k].Hayir = dugumler[hayir[k]];
            }
            if (satirSayisi == 0) throw new InvalidDataException("Düzen boş: " + Path.GetFileName(yol));
            sema.Kok = dugumler[0];
            return sema;
        }

        /// <summary>Sabit genişlikte kırılmış satırları paragraflara toplar: boş satır paragraf sonu,
        /// "(1)", "-" ya da girintili satır yeni madde başlatır, diğerleri önceki satıra eklenir.</summary>
        private static string Paragraflar(IEnumerable<string> satirlar)
        {
            var sonuc = new List<string>();
            var cur = new StringBuilder();
            Action Bitir = delegate { if (cur.Length > 0) { sonuc.Add(cur.ToString()); cur.Length = 0; } };
            foreach (string ham in satirlar)
            {
                string t = ham.Trim();
                if (t.Length == 0) { Bitir(); continue; }
                bool yeni = ham.StartsWith("   ") || t.StartsWith("(") || t.StartsWith("-") || t.StartsWith("*");
                if (yeni) Bitir();
                if (cur.Length > 0) cur.Append(' ');
                cur.Append(t);
            }
            Bitir();
            return string.Join("\n", sonuc);
        }

        // ── Türkçe çeviri sözlüğü: Resources\AkisSemasi_TR.tsv (İngilizce metin TAB Türkçe metin; satır başına bir parça).
        // Anahtar, ayrıştırılmış parçadır (başlık, kutu etiketi, giriş/açıklama paragrafı). Sözlükte yoksa metin İngilizce kalır.
        private static Dictionary<string, string> _sozluk;
        public static bool TurkceKullan = true;

        private static Dictionary<string, string> SozlukYukle()
        {
            var d = new Dictionary<string, string>(StringComparer.Ordinal);
            try
            {
                string yol = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "AkisSemasi_TR.tsv");
                if (File.Exists(yol))
                    foreach (string satir in File.ReadAllLines(yol, Encoding.UTF8))
                    {
                        int t = satir.IndexOf('\t');
                        if (t <= 0 || t == satir.Length - 1) continue;
                        d[satir.Substring(0, t)] = satir.Substring(t + 1);
                    }
            }
            catch { }
            return d;
        }

        public static string Cevir(string metin)
        {
            if (!TurkceKullan || string.IsNullOrEmpty(metin)) return metin;
            if (_sozluk == null) _sozluk = SozlukYukle();
            string tr;
            return _sozluk.TryGetValue(metin, out tr) ? tr : metin;
        }

        private static string CevirParagraflar(string paragraflar)
        {
            if (string.IsNullOrEmpty(paragraflar)) return paragraflar;
            return string.Join("\n", paragraflar.Split('\n').Select(Cevir));
        }

        /// <summary>$C $D $H $S → ♣ ♦ ♥ ♠ (ekranda gösterim için).</summary>
        public static string Simgeler(string metin)
        {
            if (string.IsNullOrEmpty(metin)) return metin ?? "";
            return metin.Replace("$C", "♣").Replace("$D", "♦").Replace("$H", "♥").Replace("$S", "♠");
        }
    }
}
