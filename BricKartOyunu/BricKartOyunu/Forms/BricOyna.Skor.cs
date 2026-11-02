using BricKartOyunu.Class;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;


namespace BricKartOyunu
{
    // Skor, bord özeti ve PBN
    public partial class BricOyna
    {
        /// <summary>
        /// Bitmiş bordun skorunu hesaplar (deklaran tarafı açısından).
        /// Kontrat yoksa (dört pas) PasGecildi işaretli sonuç döner.
        /// </summary>
        public SkorSonucu SkorHesapla()
        {
            if (string.IsNullOrEmpty(_kontrat))
                return new SkorSonucu { PasGecildi = true };

            int seviye = _kontrat[0] - '0';
            string koz = _kontrat.IndexOf("NT", StringComparison.OrdinalIgnoreCase) >= 0
                ? "NT" : _kontrat.Substring(1, 1);

            bool deklaranNS = _kontratDeklaran == Player.Kuzey || _kontratDeklaran == Player.Guney;
            bool nsZon, ewZon;
            ZonDurumu(out nsZon, out ewZon);
            bool zonda = deklaranNS ? nsZon : ewZon;
            int alinan = deklaranNS ? _kazanilanElNS : _kazanilanElEW;

            return Skorlama.Hesapla(seviye, koz, _kontrDurumu, zonda, alinan);
        }

        /// <summary>Skor penceresi için metin: deklaran tarafı ve N-S / E-W açısından toplam.</summary>
        public string SkorMetni()
        {
            var s = SkorHesapla();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Bord " + _currentTour);
            if (!s.PasGecildi)
                sb.AppendLine("Deklaran: " + PlayerIsmiTurkce(_kontratDeklaran));
            sb.AppendLine();
            sb.Append(Skorlama.Aciklama(s));

            if (!s.PasGecildi)
            {
                bool deklaranNS = _kontratDeklaran == Player.Kuzey || _kontratDeklaran == Player.Guney;
                int ns = deklaranNS ? s.Toplam : -s.Toplam;
                if (KoltukHaritasi.CiftlerTers) ns = -ns; // fiziksel K-G = iç D-B
                sb.AppendLine();
                sb.AppendLine("Kuzey-Güney: " + (ns > 0 ? "+" : "") + ns);
                sb.AppendLine("Doğu-Batı  : " + (-ns > 0 ? "+" : "") + (-ns));
            }
            return sb.ToString();
        }

        private static string KartMetniSatiri(List<Card> el, string renk)
        {
            var dizi = el.Where(c => c.Suit == renk).OrderByDescending(c => c.Value)
                         .Select(c => c.Value == 10 ? "10" : PbnDeger(c.Value).ToString());
            string metin = string.Join(" ", dizi);
            return string.IsNullOrEmpty(metin) ? "—" : metin;
        }

        /// <summary>Yazdırma için bordun düz metin özeti (eller, ihale, sonuç, skor).</summary>
        public string BordOzetiMetni()
        {
            var sb = new System.Text.StringBuilder();
            Player dagitici = _ellerIhaleSonrasiTakasli ? Karsi(_dealer) : _dealer;

            bool nsZon, ewZon;
            ZonDurumu(out nsZon, out ewZon);
            if (KoltukHaritasi.CiftlerTers) { bool g = nsZon; nsZon = ewZon; ewZon = g; }
            string zon = nsZon && ewZon ? "Herkes zonda" : nsZon ? "K-G zonda" : ewZon ? "D-B zonda" : "Zonsuz";

            sb.AppendLine("BRİÇ KART OYUNU — Bord " + _currentTour);
            sb.AppendLine("Dağıtıcı: " + PlayerIsmiTurkce(dagitici) + "   " + zon);
            sb.AppendLine(new string('─', 52));
            sb.AppendLine();

            string[] renkler = { "Maça", "Kupa", "Karo", "Sinek" };
            string[] semboller = { "♠", "♥", "♦", "♣" };

            Func<Player, List<string>> blok = oyuncu =>
            {
                var el = BaslangicEli(oyuncu);
                var satirlar = new List<string> { PlayerIsmiTurkce(oyuncu).ToUpper() };
                for (int i = 0; i < 4; i++)
                    satirlar.Add(semboller[i] + " " + KartMetniSatiri(el, renkler[i]));
                return satirlar;
            };

            // Fiziksel koltuğa denk gelen iç oyuncunun eli yazdırılır
            var kuzey = blok(KoltukHaritasi.Ic(Player.Kuzey));
            var bati = blok(KoltukHaritasi.Ic(Player.Bati));
            var dogu = blok(KoltukHaritasi.Ic(Player.Dogu));
            var guney = blok(KoltukHaritasi.Ic(Player.Guney));

            foreach (var satir in kuzey) sb.AppendLine(new string(' ', 20) + satir);
            sb.AppendLine();
            for (int i = 0; i < bati.Count; i++)
                sb.AppendLine(bati[i].PadRight(30) + dogu[i]);
            sb.AppendLine();
            foreach (var satir in guney) sb.AppendLine(new string(' ', 20) + satir);
            sb.AppendLine();
            sb.AppendLine(new string('─', 52));

            // İhale tablosu
            sb.AppendLine("İhale:");
            sb.AppendLine("Batı".PadRight(10) + "Kuzey".PadRight(10) + "Doğu".PadRight(10) + "Güney");
            var bids = _sonIhaleGecmisi ?? new List<string>();
            int sutun = 0;
            switch (KoltukHaritasi.Fiz(dagitici))
            {
                case Player.Bati: sutun = 0; break;
                case Player.Kuzey: sutun = 1; break;
                case Player.Dogu: sutun = 2; break;
                default: sutun = 3; break;
            }
            var satirSb = new System.Text.StringBuilder(new string(' ', sutun * 10));
            foreach (var b in bids)
            {
                satirSb.Append(b.PadRight(10));
                sutun++;
                if (sutun == 4)
                {
                    sb.AppendLine(satirSb.ToString().TrimEnd());
                    satirSb.Clear();
                    sutun = 0;
                }
            }
            if (satirSb.Length > 0) sb.AppendLine(satirSb.ToString().TrimEnd());
            sb.AppendLine();
            sb.AppendLine(new string('─', 52));
            sb.Append(SkorMetni());
            return sb.ToString();
        }

        private static string PbnTemiz(string t)
        {
            return (t ?? "").Replace("\"", "'").Replace("\r", " ").Replace("\n", " ");
        }

        private static char PbnKoltuk(Player p)
        {
            switch (KoltukHaritasi.Fiz(p))
            {
                case Player.Kuzey: return 'N';
                case Player.Dogu: return 'E';
                case Player.Guney: return 'S';
                default: return 'W';
            }
        }

        private static Player Karsi(Player p)
        {
            switch (p)
            {
                case Player.Kuzey: return Player.Guney;
                case Player.Guney: return Player.Kuzey;
                case Player.Dogu: return Player.Bati;
                default: return Player.Dogu;
            }
        }

        private static char PbnRenk(string suit)
        {
            switch (suit)
            {
                case "Maça": return 'S';
                case "Kupa": return 'H';
                case "Karo": return 'D';
                default: return 'C';
            }
        }

        private static char PbnDeger(int v)
        {
            switch (v)
            {
                case 14: return 'A';
                case 13: return 'K';
                case 12: return 'Q';
                case 11: return 'J';
                case 10: return 'T';
                default: return (char)('0' + v);
            }
        }

        private static string PbnTeklif(string t)
        {
            if (string.IsNullOrEmpty(t)) return "Pass";
            if (t == "Pas" || t == "PAS") return "Pass";
            if (t == "Dbl") return "X";
            if (t == "RDbl") return "XX";
            // "1♦" → "1D", "3NT" → "3NT"
            if (t.EndsWith("♣")) return t.Substring(0, t.Length - 1) + "C";
            if (t.EndsWith("♦")) return t.Substring(0, t.Length - 1) + "D";
            if (t.EndsWith("♥")) return t.Substring(0, t.Length - 1) + "H";
            if (t.EndsWith("♠")) return t.Substring(0, t.Length - 1) + "S";
            return t;
        }

        /// <summary>
        /// Başlangıç (13 kartlık) eli: elde kalan + oynanmış kartlar.
        /// </summary>
        private List<Card> BaslangicEli(Player oyuncu)
        {
            var kartlar = new List<Card>(OyuncuEli(oyuncu) ?? new List<Card>());
            foreach (var k in _oynananTumEller.Where(t => t.Oyuncu == oyuncu).Select(t => t.Kart))
                if (!kartlar.Any(c => c.Suit == k.Suit && c.Value == k.Value)) kartlar.Add(k);
            return kartlar;
        }

        private string PbnEl(Player oyuncu)
        {
            var el = BaslangicEli(oyuncu);
            var parcalar = new List<string>();
            foreach (var renk in new[] { "Maça", "Kupa", "Karo", "Sinek" })
            {
                var dizi = el.Where(c => c.Suit == renk).OrderByDescending(c => c.Value)
                             .Select(c => PbnDeger(c.Value).ToString());
                parcalar.Add(string.Concat(dizi));
            }
            return string.Join(".", parcalar);
        }

        /// <summary>
        /// Bu bordu PBN metni olarak üretir.
        /// </summary>
        public string PbnUret(string ad, string aciklama, string yorum)
        {
            var sb = new System.Text.StringBuilder();
            string nlx = "\r\n";

            // Eller 180° takaslıysa dağıtıcı ve ihale koltukları da döndürülür
            Player dagitici = _ellerIhaleSonrasiTakasli ? Karsi(_dealer) : _dealer;

            // Zon (Vulnerable): 16'lı bord döngüsü
            bool ns, ew;
            FizikselZon(out ns, out ew);
            string zon = ns && ew ? "All" : ns ? "NS" : ew ? "EW" : "None";

            sb.Append("[Event \"" + PbnTemiz(string.IsNullOrWhiteSpace(ad) ? "BricKartOyunu" : ad) + "\"]" + nlx);
            sb.Append("[Site \"BricKartOyunu\"]" + nlx);
            sb.Append("[Date \"" + DateTime.Now.ToString("yyyy.MM.dd") + "\"]" + nlx);
            sb.Append("[Board \"" + _currentTour + "\"]" + nlx);
            sb.Append("[Dealer \"" + PbnKoltuk(dagitici) + "\"]" + nlx);
            sb.Append("[Vulnerable \"" + zon + "\"]" + nlx);

            // Deal: ilk elin koltuğu N, saat yönünde N E S W
            sb.Append("[Deal \"N:" + PbnEl(KoltukHaritasi.Ic(Player.Kuzey)) + " " + PbnEl(KoltukHaritasi.Ic(Player.Dogu)) + " " +
                      PbnEl(KoltukHaritasi.Ic(Player.Guney)) + " " + PbnEl(KoltukHaritasi.Ic(Player.Bati)) + "\"]" + nlx);

            // İhale
            var bids = _sonIhaleGecmisi ?? new List<string>();
            sb.Append("[Auction \"" + PbnKoltuk(dagitici) + "\"]" + nlx);
            for (int i = 0; i < bids.Count; i++)
            {
                sb.Append(PbnTeklif(bids[i]));
                sb.Append((i % 4 == 3 || i == bids.Count - 1) ? nlx : " ");
            }

            // Kontrat
            if (string.IsNullOrEmpty(_kontrat))
            {
                sb.Append("[Contract \"Pass\"]" + nlx);
            }
            else
            {
                string kontrat = PbnTeklif(_kontrat);
                int sonGercek = -1;
                for (int i = bids.Count - 1; i >= 0; i--)
                    if (bids[i] != "Pas" && bids[i] != "PAS" && bids[i] != "Dbl" && bids[i] != "RDbl") { sonGercek = i; break; }
                if (sonGercek >= 0)
                {
                    var sonrasi = bids.Skip(sonGercek + 1).ToList();
                    if (sonrasi.Contains("RDbl")) kontrat += "XX";
                    else if (sonrasi.Contains("Dbl")) kontrat += "X";
                }
                sb.Append("[Contract \"" + kontrat + "\"]" + nlx);
                sb.Append("[Declarer \"" + PbnKoltuk(_kontratDeklaran) + "\"]" + nlx);

                bool deklaranNS = _kontratDeklaran == Player.Kuzey || _kontratDeklaran == Player.Guney;
                sb.Append("[Result \"" + (deklaranNS ? _kazanilanElNS : _kazanilanElEW) + "\"]" + nlx);
            }

            // Oyun: açılış atağı yapan + her el için dört kart (oynanış sırasıyla)
            if (_oynananTumEller.Count > 0)
            {
                var gruplar = _oynananTumEller.GroupBy(t => t.ElNo).OrderBy(g => g.Key).ToList();
                sb.Append("[Play \"" + PbnKoltuk(gruplar[0].First().Oyuncu) + "\"]" + nlx);
                foreach (var grup in gruplar)
                {
                    var kartlar = grup.Select(t => "" + PbnRenk(t.Kart.Suit) + PbnDeger(t.Kart.Value)).ToList();
                    while (kartlar.Count < 4) kartlar.Add("-");
                    sb.Append(string.Join(" ", kartlar) + nlx);
                }
            }

            // Yorum bloğu (ad, açıklama, yorum)
            var yorumSatirlari = new List<string>();
            if (!string.IsNullOrWhiteSpace(aciklama)) yorumSatirlari.Add(aciklama.Trim());
            if (!string.IsNullOrWhiteSpace(yorum)) yorumSatirlari.Add(yorum.Trim());
            if (yorumSatirlari.Count > 0)
                sb.Append("{" + string.Join(" — ", yorumSatirlari).Replace("{", "(").Replace("}", ")") + "}" + nlx);

            return sb.ToString();
        }

        public void IhaleGecmisiniKaydet(List<string> bids, string baslangicOyuncusu)
        {
            _sonIhaleGecmisi = new List<string>(bids);
            _ihaleBaslangicOyuncusu = baslangicOyuncusu;
            IhaleGecmisiFormlariniGuncelle();
        }

    }
}
