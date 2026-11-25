using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class
{
    /// <summary>Akış şemasındaki bir soruya verilen otomatik cevap ve gerekçesi.</summary>
    public sealed class AkisCevap
    {
        public bool Evet;
        public string Neden;
        public AkisCevap(bool evet, string neden) { Evet = evet; Neden = neden; }
    }

    /// <summary>
    /// Akış şeması sorularını eldeki ele, ihale geçmişine ve ortaklık anlaşmasına bakarak cevaplar (Bridge Baron'daki
    /// "optimal yol" gibi). Yalnızca anlamı kesin olan sorular cevaplanır; emin olunamayan soru için null döner ve
    /// pencere cevabı kullanıcıdan ister. Kural eklemek için <see cref="Cevapla"/> içindeki anahtarlara bakın.
    /// </summary>
    public static class AkisYolBulucu
    {
        private static readonly HashSet<string> AcilisSemalari = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "opening", "aopening", "fopening", "lopening" };

        /// <summary>Soru etiketini karşılaştırma anahtarına çevirir: küçük harf, boşluk ve ? yok.</summary>
        public static string Anahtar(string etiket)
        {
            return Regex.Replace((etiket ?? "").ToLowerInvariant(), @"[\s\?]+", "");
        }

        /// <summary>d düğümündeki soruyu cevaplar; cevaplanamıyorsa null. semaAd şemanın dosya adıdır (ör. "opening").</summary>
        public static AkisCevap Cevapla(string semaAd, AkisSemasi.Dugum d, IhaleDurumu durum)
        {
            if (d == null || d.Yaprak || durum == null || durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13) return null;
            var el = durum.AktifOyuncuEli;
            string k = Anahtar(d.Kutu.Orijinal);
            string ac = d.Kutu.Aciklama ?? "";
            int hp = ElDegerlendirici.HCP(el);
            int maca = ElDegerlendirici.RenkUzunlugu(el, "Maça"), kupa = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo"), sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            int enUzun = Math.Max(Math.Max(maca, kupa), Math.Max(karo, sinek));
            var g = durum.Gecmis ?? new List<IhaleHamlesi>();

            // ── Her şemada geçerli, anlamı kesin sorular ──
            if (k == "fourthhand")
            {
                bool dorduncu = g.Count == 3 && g.All(h => h.PasMi);
                return new AkisCevap(dorduncu, dorduncu ? "ihaleyi son açma sırası sizde (3 pas)" : "dördüncü el değilsiniz");
            }

            // ── Açılış şemaları ──
            if (AcilisSemalari.Contains(semaAd ?? ""))
            {
                switch (k)
                {
                    case "canyouopen1nt":
                        { bool b = NTA.Acilis1NT(el, durum.Anlasma); return new AkisCevap(b, b ? "dengeli el, " + hp + " HP: 1NT aralığında" : "1NT için aralık/dağılım uygun değil (" + hp + " HP)"); }
                    case "canyouopen2nt":
                        { bool b = NTA.Acilis2NT(el, durum.Anlasma); return new AkisCevap(b, b ? "dengeli el, " + hp + " HP: 2NT aralığında" : "2NT için aralık/dağılım uygun değil (" + hp + " HP)"); }
                    case "seven-cardsuit":
                        return new AkisCevap(enUzun >= 7, "en uzun renk " + enUzun + " kart");
                    case "strongenoughfora1ofasuitbid":
                        { bool b = ElDegerlendirici.AcilisGucuVarMi(el); return new AkisCevap(b, b ? "açılış gücü var (" + hp + " HP)" : "açılış gücü yok (" + hp + " HP)"); }
                    case "22+hcpandbalanced":
                        { bool b = hp >= 22 && ElDegerlendirici.DengeliEl(el); return new AkisCevap(b, hp + " HP" + (ElDegerlendirici.DengeliEl(el) ? ", dengeli" : ", dengeli değil")); }
                    case "5+-cardmajor":
                    case "5cardmajor":
                    case "5-cardmajor":
                        { bool b = Math.Max(maca, kupa) >= 5; return new AkisCevap(b, "majör uzunlukları ♠" + maca + " ♥" + kupa); }
                    case "doyouhavetwo5+-cardmajors":
                        { bool b = maca >= 5 && kupa >= 5; return new AkisCevap(b, "♠" + maca + " ♥" + kupa); }
                    case "equallengthinminors":
                        return new AkisCevap(karo == sinek, "♦" + karo + " ♣" + sinek);
                    case "genuinesuits":
                        return new AkisCevap(karo >= 4 && sinek >= 4, "♦" + karo + " ♣" + sinek + " (iki minörde de 4+ kart aranır)");
                    case "weakhand,orbothsuitsthesamelength":
                        { int tp0 = ElDegerlendirici.ToplamPuan(el); bool b = tp0 < 18 || maca == kupa; return new AkisCevap(b, "toplam puan " + tp0 + ", ♠" + maca + " ♥" + kupa); }
                    case "4-4-4-1":
                        {
                            var u = new[] { maca, kupa, karo, sinek }.OrderBy(x => x).ToArray();
                            bool b = u[0] == 1 && u[1] == 4 && u[2] == 4 && u[3] == 4;
                            return new AkisCevap(b, "dağılım " + string.Join("-", u.Reverse()));
                        }
                    case "5-cardsuit":
                        // Açılış şemasında bu soru güçlü iki açılışı içindir: "en az 5 kartlı renk"
                        return new AkisCevap(enUzun >= 5, "en uzun renk " + enUzun + " kart");
                    case "2ofasuit":
                        {
                            // Güçlü iki: 23+ toplam puan; 21-22'de renk kalitesine bakılması gerekir (emin olunamaz)
                            int tp = ElDegerlendirici.ToplamPuan(el);
                            if (tp >= 23) return new AkisCevap(true, "toplam puan " + tp + " (güçlü iki için 23+)");
                            if (tp < 21) return new AkisCevap(false, "toplam puan " + tp + " (güçlü iki için en az 21)");
                            return null;
                        }
                    case "fourthseatweaktwo":
                        {
                            bool dorduncu = g.Count == 3 && g.All(h => h.PasMi);
                            if (!dorduncu) return new AkisCevap(false, "dördüncü el değilsiniz");
                            if (enUzun < 6 || hp < 10 || hp > 14) return new AkisCevap(false, "dördüncü elde zayıf iki: 6 kartlı renk ve 10-14 HP gerekir (sizde " + enUzun + " kart, " + hp + " HP)");
                            return null;      // renk kalitesine bağlı
                        }
                    case "weak2-bids":
                        {
                            bool var6 = Math.Max(Math.Max(maca, kupa), karo) >= 6;
                            bool oyn = durum.Anlasma != null && durum.Anlasma.ZayifIki;
                            return new AkisCevap(oyn && var6, !oyn ? "ortaklık anlaşmasında zayıf iki kapalı" : (var6 ? "♦/♥/♠'da 6+ kart var" : "♦/♥/♠'da 6 kartlı renk yok"));
                        }
                }
            }

            // ── Rakip müdahalesi ──
            if (k == "intervention" || k == "interference")
            {
                var son = g.LastOrDefault(h => !h.PasMi);
                bool sonRakip = son != null && durum.Rakipler.Contains(son.Oyuncu);
                if (ac.IndexOf("last bid or double", StringComparison.OrdinalIgnoreCase) >= 0)
                    return new AkisCevap(sonRakip, sonRakip ? "son teklif/kontra rakipten" : "son teklif/kontra rakipten değil");
                if (ac.IndexOf("right-hand opponent bid or double", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var sag = g.Count > 0 ? g[g.Count - 1] : null;
                    bool b = sag != null && !sag.PasMi && durum.Rakipler.Contains(sag.Oyuncu);
                    return new AkisCevap(b, b ? "sağ rakip teklif ya da kontra yaptı" : "sağ rakip pas dedi");
                }
                if (ac.IndexOf("right-hand opponent bid ?", StringComparison.OrdinalIgnoreCase) >= 0 || ac.IndexOf("right-hand opponent bid", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var sag = g.Count > 0 ? g[g.Count - 1] : null;
                    bool b = sag != null && sag.GercekTeklifMi && durum.Rakipler.Contains(sag.Oyuncu);
                    return new AkisCevap(b, b ? "sağ rakip teklif verdi" : "sağ rakip teklif vermedi");
                }
            }

            // ── Oyun için yeterli puan: kendi toplam puanım + ortağın bildirdiği aralık ──
            if (k == "enoughforgame")
            {
                var m = Regex.Match(ac, @"(\d+)\s+or more Total Points", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    int esik = int.Parse(m.Groups[1].Value);
                    int ben = ElDegerlendirici.ToplamPuan(el);
                    var a = IhaleKoruma.PartnerAraligi(durum);
                    if (ben + a.Min >= esik) return new AkisCevap(true, "sizde " + ben + " + ortakta en az " + a.Min + " ≥ " + esik);
                    if (a.Max < 40 && ben + a.Max + 3 < esik) return new AkisCevap(false, "sizde " + ben + " + ortakta en çok " + a.Max + " < " + esik);
                }
                return null;
            }

            // ── Konvansiyon soruları: ortaklık anlaşmasında kapalıysa cevap kesin Hayır ──
            var bayrak = KonvansiyonBayragi(durum.Anlasma, k);
            if (bayrak != null)
            {
                if (!bayrak.Value) return new AkisCevap(false, "ortaklık anlaşmasında kapalı");
                switch (k)
                {
                    case "flannery":
                        { bool b = maca == 4 && kupa == 5 && hp >= 11 && hp <= 15; return new AkisCevap(b, "♠" + maca + " ♥" + kupa + ", " + hp + " HP (Flannery: 4♠ 5♥, 11-15 HP)"); }
                }
            }
            return null;
        }

        /// <summary>Soru anahtarı bir konvansiyon adıysa anlaşmadaki açık/kapalı durumunu döndürür; tanınmazsa null.</summary>
        private static bool? KonvansiyonBayragi(OrtaklikAnlasmasi a, string k)
        {
            if (a == null) return null;
            string ad;
            if (!Takma.TryGetValue(k, out ad)) return null;
            var p = typeof(OrtaklikAnlasmasi).GetProperty(ad, BindingFlags.Public | BindingFlags.Instance);
            if (p == null || p.PropertyType != typeof(bool)) return null;
            return (bool)p.GetValue(a, null);
        }

        // Soru anahtarı → OrtaklikAnlasmasi özelliği
        private static readonly Dictionary<string, string> Takma = new Dictionary<string, string>
        {
            { "flannery", "Flannery" }, { "namyats", "Namyats" }, { "gambling3nt", "Gambling3NT" }, { "mini-roman2d", "MiniRoman2D" },
            { "benjaminweaktwos", "BenjaminTwoBids" }, { "texastransfer", "TexasTransfers" }, { "jacobytransfer", "JacobyTransfer" },
            { "jacobytransfers", "JacobyTransfer" }, { "drury", "Drury" }, { "stayman", "Stayman" }, { "splinterbid", "Splinter" },
            { "michaels", "Michaels" }, { "unusual2nt", "Unusual2NT" }, { "cappelletti", "Cappelletti" }, { "playingcappelletti", "Cappelletti" },
            { "playingd.o.n.t.", "Dont" }, { "minor-suitstayman", "MinorStayman" }, { "jacoby2ntbid", "Jacoby2NT" },
        };
    }
}
