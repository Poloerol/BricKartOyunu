using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BricKartOyunu.Class
{
    // ═══════════════════════════════════════════════════════════════════════
    // clubs.vugraph.com (Vugraph Briç Kulüpleri) sayfalarını ayrıştırır ve PBN üretir.
    //   /?lang=tr                         → kulüp rehberi (şehir bölümleri, <a href="/kod/">)
    //   /<kod>/calendar/yyyy-MM           → turnuva bağlantıları (/<kod>/results/yyyy-MM-dd[-n])
    //   /<kod>/results/yyyy-MM-dd[-n]     → turnuvanın TAMAMI sayfaya gömülü JSON'dur (script id="event-data"):
    //        pairs[]  : no, players[2], place, score
    //        boards[] : no, dealer (0..3 = K,D,G,B), vul [K-G, D-B], hands [K,D,G,B] "S.H.D.C", lines[]
    //        lines[]  : ns, ew (çift no), c (kontrat, pas/hakem skorunda boş), by, r ("=", "+1", "−2"),
    //                   s [K-G skoru, D-B skoru], a [K-G %, D-B %]
    // ═══════════════════════════════════════════════════════════════════════

    public class KulupOge
    {
        public string Kod;    // clubs.vugraph.com/<kod>/
        public string Ad;
        public string Yer;
        public override string ToString() { return string.IsNullOrEmpty(Yer) ? Ad : Ad + "  -  " + Yer; }
    }

    public class TurnuvaOge
    {
        public string EventId;    // "2026-11-24" ya da "2026-11-07-2"
        public string Ad;
        public DateTime Gun;
        public override string ToString() { return Gun.ToString("dd.MM.yyyy") + "   " + Ad; }
    }

    public class TurnuvaBilgisi
    {
        public string EventId;
        public string Kulup = "";
        public string Ad = "";
        public DateTime? Tarih;
        public string Saat = "";
        public string Puanlama = "MP";
        public List<CiftOge> Ciftler = new List<CiftOge>();
        public List<TurnuvaBordu> Bordlar = new List<TurnuvaBordu>();
    }

    public class CiftOge
    {
        public int No;
        public string Sira = "";
        public string[] Oyuncular = new string[0];
        public string Ad { get { return string.Join(" - ", Oyuncular); } }
        public string Skor = "";
        public string Yon = "";    // "NS" / "EW"; sayfa vermiyorsa boş

        public override string ToString()
        {
            string yon = Yon == "NS" ? " [K-G]" : Yon == "EW" ? " [D-B]" : "";
            string sira = Sira.Length > 0 ? Sira + ".  " : "";
            string skor = Skor.Length > 0 ? "   -   " + Skor : "";
            return sira + Ad + skor + yon;
        }
    }

    public class TurnuvaBordu
    {
        public int No;
        public char Dagitici = 'N';
        public string Zon = "None";
        public string[] El;                                       // N, E, S, W; sayfada el yoksa null
        public List<SahaSonucu> Satirlar = new List<SahaSonucu>();
    }

    public class SahaSonucu
    {
        public int NsCift, EwCift;
        public string Kontrat = "";   // "4S", "3NT", "4SX", "Pass" ya da hakem skoru ("A=")
        public int Seviye;            // 0 = kontrat yok (pas / hakem skoru)
        public string Deklaran = "";  // N / E / S / W
        public int? Love;             // deklaranın aldığı löve
        public int? NsSkor;           // Kuzey-Güney açısından puan
        public string Ham = "";       // sonuç ("+2", "=", "-1")
        public bool Gecerli { get { return Seviye > 0 && Deklaran.Length == 1; } }
    }

    public class BordDetayi
    {
        public int No;
        public string[] El;                     // N, E, S, W  → "S.H.D.C"
        public string[] Isim = new string[4];   // N, E, S, W
        public char Dagitici = 'N';
        public string Zon = "None";
        public SahaSonucu Secili;
        public List<SahaSonucu> Saha = new List<SahaSonucu>();
    }

    public static class Vugraph
    {
        private const RegexOptions SL = RegexOptions.Singleline | RegexOptions.IgnoreCase;

        // ───────────────────────── Yardımcılar ─────────────────────────

        public static string Temiz(string html)
        {
            if (html == null) return "";
            html = Regex.Replace(html, @"<br\s*/?>", " ", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<[^>]*>", "", RegexOptions.Singleline);
            html = WebUtility.HtmlDecode(html);
            return Regex.Replace(html, @"\s+", " ").Trim();
        }

        /// <summary>Türkçe harfleri ve aksanları kaldırır; PBN dosyası yalnızca ASCII içerir.</summary>
        public static string Ascii(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace('ı', 'i').Replace('İ', 'I').Replace('ğ', 'g').Replace('Ğ', 'G').Replace('ş', 's').Replace('Ş', 'S')
                 .Replace('ç', 'c').Replace('Ç', 'C').Replace('ö', 'o').Replace('Ö', 'O').Replace('ü', 'u').Replace('Ü', 'U');
            var d = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (c == '"' || c == '[' || c == ']' || c < 32) continue;
                sb.Append(c < 128 ? c : '?');
            }
            return sb.ToString().Trim();
        }

        public static string DosyaAdi(string s)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) s = s.Replace(c.ToString(), "");
            return s.Trim();
        }

        // ───────────────────────── Kulüp rehberi ─────────────────────────

        public static List<KulupOge> KulupListesiAyristir(string html)
        {
            var liste = new List<KulupOge>();
            var gorulen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match bolum in Regex.Matches(html, @"<section\s+class=""city"">\s*<h2>(.*?)</h2>(.*?)</section>", SL))
            {
                string yer = Temiz(bolum.Groups[1].Value);
                foreach (Match m in Regex.Matches(bolum.Groups[2].Value, @"<a\s+href=""/([^/""?#]+)/""[^>]*>(.*?)</a>", SL))
                {
                    string kod = m.Groups[1].Value;
                    if (!gorulen.Add(kod)) continue;
                    // Bazı kulüp adları "<<< Ad >>>" biçiminde süslü
                    string ad = Temiz(m.Groups[2].Value).Trim('<', '>', '-', ' ');
                    if (ad.Length == 0) ad = kod;
                    liste.Add(new KulupOge { Kod = kod, Ad = ad, Yer = yer });
                }
            }
            return liste.OrderBy(k => k.Ad, StringComparer.Create(new CultureInfo("tr-TR"), true)).ToList();
        }

        // ───────────────────────── Takvim ─────────────────────────

        private static DateTime? KimliktenTarih(string eventId)
        {
            DateTime t;
            if (eventId != null && eventId.Length >= 10 &&
                DateTime.TryParseExact(eventId.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out t))
                return t;
            return null;
        }

        public static List<TurnuvaOge> TakvimAyristir(string html, int ay, int yil)
        {
            var sonuc = new List<TurnuvaOge>();
            var gorulen = new HashSet<string>();
            // Sayfada her turnuva iki kez geçer (geniş ve dar ekran görünümü)
            foreach (Match m in Regex.Matches(html, @"<a\s+href=""/[^/""]+/results/([^/""?#]+)""[^>]*>(.*?)</a>", SL))
            {
                string id = m.Groups[1].Value;
                var gun = KimliktenTarih(id);
                if (!gun.HasValue || gun.Value.Month != ay || gun.Value.Year != yil || !gorulen.Add(id)) continue;
                sonuc.Add(new TurnuvaOge { EventId = id, Ad = Temiz(m.Groups[2].Value), Gun = gun.Value });
            }
            return sonuc.OrderBy(t => t.Gun).ThenBy(t => t.EventId.Length).ThenBy(t => t.EventId, StringComparer.Ordinal).ToList();
        }

        // ───────────────────────── Turnuva (çiftler + bordlar) ─────────────────────────

        public static TurnuvaBilgisi TurnuvaAyristir(string html, string eventId)
        {
            var veri = Regex.Match(html, @"<script[^>]*id=""event-data""[^>]*>(.*?)</script>", SL);
            if (!veri.Success) throw new Exception("Sayfada turnuva verisi bulunamadı (sonuçlar henüz yayınlanmamış olabilir).");
            var kok = JsonConvert.DeserializeObject<JObject>(veri.Groups[1].Value);

            var t = new TurnuvaBilgisi { EventId = eventId, Ad = Metin(kok["crumb"]), Tarih = KimliktenTarih(eventId) };
            t.Puanlama = Metin(kok["award"]) == "%" ? "MP" : "IMP";
            var bilgi = Regex.Match(html, @"<p\s+class=""meta"">(.*?)</p>", SL);
            if (bilgi.Success) t.Saat = Regex.Match(bilgi.Groups[1].Value, @"\b\d{1,2}:\d{2}\b").Value;

            foreach (var p in Dizi(kok["pairs"]))
            {
                var c = new CiftOge
                {
                    No = (int)p["no"],
                    Sira = Metin(p["place"]),
                    Skor = Metin(p["score"]),
                    Yon = Metin(p["dir"]).ToUpperInvariant(),
                    Oyuncular = Dizi(p["players"]).Select(x => Metin(x)).Where(x => x.Length > 0).ToArray()
                };
                if (c.Oyuncular.Length > 0) t.Ciftler.Add(c);
            }
            // Listeyi sıralamaya göre ver ("5=" gibi eşitlikler dahil; sırası olmayanlar numaraya göre sonda)
            t.Ciftler = t.Ciftler.OrderBy(c => SiraSayisi(c.Sira)).ThenBy(c => c.No).ToList();

            foreach (var b in Dizi(kok["boards"]))
            {
                var bord = new TurnuvaBordu { No = (int)b["no"] };
                int dagitici = (int?)b["dealer"] ?? 0;
                bord.Dagitici = "NESW"[((dagitici % 4) + 4) % 4];
                var zon = Dizi(b["vul"]).Select(x => (bool)x).ToArray();
                bool ns = zon.Length > 0 && zon[0], ew = zon.Length > 1 && zon[1];
                bord.Zon = ns && ew ? "All" : ns ? "NS" : ew ? "EW" : "None";
                var eller = Dizi(b["hands"]).Select(x => Metin(x).ToUpperInvariant().Replace("-", "").Replace("10", "T")).ToArray();
                if (eller.Length == 4) bord.El = eller;
                foreach (var s in Dizi(b["lines"])) bord.Satirlar.Add(SonucAyristir(s));
                t.Bordlar.Add(bord);
            }
            return t;
        }

        private static IEnumerable<JToken> Dizi(JToken t) { return t as JArray ?? Enumerable.Empty<JToken>(); }

        private static string Metin(JToken t) { return t == null || t.Type == JTokenType.Null ? "" : t.ToString().Trim(); }

        private static int SiraSayisi(string sira)
        {
            var m = Regex.Match(sira ?? "", @"\d+");
            int v;
            return m.Success && int.TryParse(m.Value, out v) ? v : int.MaxValue;
        }

        /// <summary>Sitedeki uzun eksi işaretini normal eksiye çevirir.</summary>
        private static string Eksi(string s) { return s.Replace('−', '-').Replace('–', '-').Replace(" ", ""); }

        private static int? Tam(string s)
        {
            int v;
            return int.TryParse(Eksi(s), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out v) ? (int?)v : null;
        }

        private static SahaSonucu SonucAyristir(JToken s)
        {
            var r = new SahaSonucu { NsCift = (int?)s["ns"] ?? 0, EwCift = (int?)s["ew"] ?? 0 };
            var skor = Dizi(s["s"]).Select(x => Metin(x)).ToArray();
            string nsMetin = skor.Length > 0 ? skor[0] : "";
            string ewMetin = skor.Length > 1 ? skor[1] : "";

            string k = Metin(s["c"]).Replace(" ", "").ToUpperInvariant();
            var km = Regex.Match(k, @"^([1-7])(NT|N|S|H|D|C)(XX|X)?$");
            if (km.Success)
            {
                r.Seviye = int.Parse(km.Groups[1].Value);
                string koz = km.Groups[2].Value == "N" ? "NT" : km.Groups[2].Value;
                r.Kontrat = r.Seviye + koz + km.Groups[3].Value;
            }
            else if (k.Length > 0) r.Kontrat = k;
            else r.Kontrat = nsMetin.StartsWith("Pas", StringComparison.OrdinalIgnoreCase) ? "Pass" : nsMetin;

            string d = Metin(s["by"]).ToUpperInvariant();
            if (d.Length > 0)
            {
                switch (d[0])
                {
                    case 'N': case 'K': r.Deklaran = "N"; break;
                    case 'E': case 'D': r.Deklaran = "E"; break;
                    case 'S': case 'G': r.Deklaran = "S"; break;
                    case 'W': case 'B': r.Deklaran = "W"; break;
                }
            }

            r.Ham = Eksi(Metin(s["r"]));
            if (r.Seviye > 0)
            {
                if (r.Ham == "=") r.Love = 6 + r.Seviye;
                else
                {
                    int? fark = Tam(r.Ham);
                    if (fark.HasValue) r.Love = 6 + r.Seviye + fark.Value;
                }
            }

            int? ns = Tam(nsMetin), ew = Tam(ewMetin);
            if (ns.HasValue) r.NsSkor = ns.Value;
            else if (ew.HasValue) r.NsSkor = -ew.Value;
            else if (r.Kontrat == "Pass") r.NsSkor = 0;
            return r;
        }

        // ───────────────────────── Seçili çiftin bordları ─────────────────────────

        /// <summary>
        /// Çiftin oynadığı bordlar. Elleri sayfada olmayan ya da hatalı olan bordlar listeye alınmaz,
        /// nedeni <paramref name="atlananlar"/> listesine yazılır.
        /// </summary>
        public static List<BordDetayi> CiftinBordlari(TurnuvaBilgisi t, CiftOge cift, List<string> atlananlar)
        {
            var liste = new List<BordDetayi>();
            var ciftler = t.Ciftler.ToDictionary(c => c.No);
            foreach (var bord in t.Bordlar.OrderBy(x => x.No))
            {
                var secili = bord.Satirlar.FirstOrDefault(x => x.NsCift == cift.No || x.EwCift == cift.No);
                if (secili == null) continue;   // çift bu bordu oynamamış
                if (bord.El == null) { atlananlar.Add("Bord " + bord.No + ": eller yayınlanmamış."); continue; }
                try { DogrulaEller(bord.El); }
                catch (Exception ex) { atlananlar.Add("Bord " + bord.No + ": " + ex.Message); continue; }

                var b = new BordDetayi
                {
                    No = bord.No, El = bord.El, Dagitici = bord.Dagitici, Zon = bord.Zon, Secili = secili,
                    Saha = bord.Satirlar.Where(x => x != secili).ToList()
                };
                // Sitedeki oturuş: Kuzey/Güney = K-G çiftinin 1./2. oyuncusu, Doğu/Batı = D-B çiftinin 1./2. oyuncusu
                b.Isim[0] = Oyuncu(ciftler, secili.NsCift, 0);
                b.Isim[1] = Oyuncu(ciftler, secili.EwCift, 0);
                b.Isim[2] = Oyuncu(ciftler, secili.NsCift, 1);
                b.Isim[3] = Oyuncu(ciftler, secili.EwCift, 1);
                liste.Add(b);
            }
            return liste;
        }

        private static string Oyuncu(Dictionary<int, CiftOge> ciftler, int no, int sira)
        {
            CiftOge c;
            return ciftler.TryGetValue(no, out c) && sira < c.Oyuncular.Length ? Ascii(c.Oyuncular[sira]) : "";
        }

        private static void DogrulaEller(string[] el)
        {
            var gorulen = new HashSet<string>();
            for (int i = 0; i < 4; i++)
            {
                var renkler = el[i].Split('.');
                if (renkler.Length != 4) throw new Exception("El biçimi hatalı: " + el[i]);
                int n = 0;
                for (int r = 0; r < 4; r++)
                    foreach (char k in renkler[r])
                    {
                        if ("AKQJT98765432".IndexOf(k) < 0) throw new Exception("Geçersiz kart '" + k + "': " + el[i]);
                        if (!gorulen.Add(r + "" + k)) throw new Exception("Aynı kart iki elde: " + k);
                        n++;
                    }
                if (n != 13) throw new Exception("Elde " + n + " kart var.");
            }
        }

        // ───────────────────────── PBN üretimi ─────────────────────────

        private static string Isaretli(int v) { return v.ToString(CultureInfo.InvariantCulture); }

        public static string KisaSonuc(SahaSonucu r)
        {
            if (r == null) return "?";
            if (r.Kontrat == "Pass") return "Pass";
            if (!r.Gecerli) return r.Kontrat;
            string s = r.Kontrat + " " + r.Deklaran + " " + r.Ham;
            if (r.NsSkor.HasValue) s += " NS " + Isaretli(r.NsSkor.Value);
            return s;
        }

        public static string PbnBord(TurnuvaBilgisi t, CiftOge cift, BordDetayi b, bool sahaYorumu)
        {
            var sb = new StringBuilder();
            string tarih = t.Tarih.HasValue ? t.Tarih.Value.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture) : "????.??.??";
            string tarihTr = t.Tarih.HasValue ? t.Tarih.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) : "";
            sb.AppendLine("[Event \"" + Ascii(t.Ad) + " " + tarihTr + "\"]");
            sb.AppendLine("[Site \"" + Ascii(t.Kulup) + "\"]");
            sb.AppendLine("[Date \"" + tarih + "\"]");
            sb.AppendLine("[Board \"" + b.No + "\"]");
            sb.AppendLine("[West \"" + b.Isim[3] + "\"]");
            sb.AppendLine("[North \"" + b.Isim[0] + "\"]");
            sb.AppendLine("[East \"" + b.Isim[1] + "\"]");
            sb.AppendLine("[South \"" + b.Isim[2] + "\"]");
            sb.AppendLine("[Dealer \"" + b.Dagitici + "\"]");
            sb.AppendLine("[Vulnerable \"" + b.Zon + "\"]");
            sb.AppendLine("[Deal \"N:" + b.El[0] + " " + b.El[1] + " " + b.El[2] + " " + b.El[3] + "\"]");
            sb.AppendLine("[Scoring \"" + t.Puanlama + "\"]");

            var s = b.Secili;
            if (s != null && s.Gecerli)
            {
                sb.AppendLine("[Declarer \"" + s.Deklaran + "\"]");
                sb.AppendLine("[Contract \"" + s.Kontrat + "\"]");
                if (s.Love.HasValue) sb.AppendLine("[Result \"" + s.Love.Value + "\"]");
                if (s.NsSkor.HasValue) sb.AppendLine("[Score \"NS " + Isaretli(s.NsSkor.Value) + "\"]");
            }
            else if (s != null && s.Kontrat == "Pass")
            {
                sb.AppendLine("[Contract \"Pass\"]");
                sb.AppendLine("[Score \"NS 0\"]");
            }
            string taraf = s == null ? "" : s.NsCift == cift.No ? " (K-G)" : " (D-B)";
            sb.AppendLine("[Description \"" + Ascii(cift.Ad) + taraf + ": " + Ascii(KisaSonuc(s)) + "\"]");

            if (sahaYorumu && b.Saha.Count > 0)
            {
                var parcalar = new List<string>();
                if (s != null) parcalar.Add("[" + KisaSonuc(s) + "]");
                foreach (var r in b.Saha) parcalar.Add(KisaSonuc(r));
                sb.AppendLine("% Saha sonuclari (NS acisindan, sonuncu = secili cift): " + string.Join(" | ", parcalar));
            }
            sb.AppendLine();
            return sb.ToString();
        }

        public static string PbnBaslik(TurnuvaBilgisi t, CiftOge cift, int bordSayisi)
        {
            var sb = new StringBuilder();
            sb.AppendLine("% PBN 2.1");
            sb.AppendLine("% EXPORT");
            if (!string.IsNullOrEmpty(t.Kulup)) sb.AppendLine("% Kulup: " + Ascii(t.Kulup));
            sb.AppendLine("% Turnuva: " + Ascii(t.Ad) + (t.Tarih.HasValue ? " " + t.Tarih.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) + " " + t.Saat : ""));
            sb.AppendLine("% Cift: " + Ascii(cift.Ad) + " | Katilan cift: " + t.Ciftler.Count + " | Bord sayisi: " + bordSayisi);
            sb.AppendLine("% Kaynak: clubs.vugraph.com event=" + t.EventId);
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
