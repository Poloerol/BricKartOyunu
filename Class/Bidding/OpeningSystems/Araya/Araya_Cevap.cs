using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.Araya
{
    /// <summary>
    /// ORTAĞIN ARAYA GİRİŞİNE CEVAPLAR (SAS kitabı s.97-102), 1♣ açılışına 1 seviyesinde renkle araya girişler:
    ///   1♣ - (1♦) - Pas - ?   transfer edilecek renk yok
    ///   1♣ - (1♥) - Pas - ?   transfer edilecek renk ♦ (2♣)
    ///   1♣ - (1♠) - Pas - ?   transfer edilecek renkler ♦ (2♣) ve ♥ (2♦)
    /// Ortak araya girmiş, sağımdaki rakip Pas demiş. Cevapçı: doğal F1R, 1NT 8-11, transferler, yükseltmeler
    /// (2. kademe, cuebid, mixed raise, splinter, blokatif). Araya girenin transfer kabulü: transfer rengini en ucuz söyler.
    /// Basitleştirmeler: 12+ HCP eşiği ve blokatif kararları sabit; 1♦/1♥/1♠ açılışlarına araya girişler henüz yok.
    /// </summary>
    public class Araya_Cevap : IAcilisGelisimi
    {
        public int Oncelik { get { return 20; } }
        public string Aciklama { get { return "Ortağın araya girişine cevaplar (transferler, mixed raise, splinter)"; } }

        private static List<IhaleHamlesi> Gercek(IhaleDurumu d) { return d.Gecmis.Where(h => h.GercekTeklifMi).ToList(); }
        private static string Kisa(char c) { return c == '♠' ? "Maça" : c == '♥' ? "Kupa" : c == '♦' ? "Karo" : "Sinek"; }
        private static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }

        public bool UygunMu(List<Card> el, IhaleDurumu d) { return Bul(el, d) != null; }
        public string TeklifVer(List<Card> el, IhaleDurumu d) { return Bul(el, d) ?? "Pas"; }

        private string Bul(List<Card> el, IhaleDurumu d)
        {
            if (d == null || el == null || el.Count != 13) return null;
            var g = Gercek(d);
            if (g.Count < 2 || g.Count > 3) return null;
            if (g[0].Teklif != "1♣" || !d.Rakipler.Contains(g[0].Oyuncu)) return null;
            if (g[1].Oyuncu != IhaleDurumu.PartnerBul(d.AktifOyuncu) && g[1].Oyuncu != d.AktifOyuncu) return null;
            string oc = g[1].Teklif;                                   // araya giriş
            if (oc != "1♦" && oc != "1♥" && oc != "1♠") return null;
            // Ardışık sekans: 1♣ - araya giriş - Pas - (ben)  veya  1♣ - araya giriş - Pas - transfer - Pas - (ben)
            int ix = d.Gecmis.IndexOf(g[0]);
            if (ix < 0 || ix + 1 >= d.Gecmis.Count || !ReferenceEquals(d.Gecmis[ix + 1], g[1])) return null;
            if (ix + 2 >= d.Gecmis.Count || d.Gecmis[ix + 2].Teklif != "Pas") return null;
            if (g.Count == 2 && d.Gecmis.Count != ix + 3) return null;
            if (g.Count == 3 && (ix + 4 >= d.Gecmis.Count || !ReferenceEquals(d.Gecmis[ix + 3], g[2]) || d.Gecmis.Count != ix + 5)) return null;
            var e = new OcEl(el);
            char y = oc[1];

            // ── Araya girenin transfer kabulü: 1♣ (rakip) - 1y (ben) - Pas - 2♣/2♦ (ortak transfer) - Pas - ? ──
            if (g.Count == 3)
            {
                if (g[1].Oyuncu != d.AktifOyuncu || g[2].Oyuncu != IhaleDurumu.PartnerBul(d.AktifOyuncu)) return null;
                string t = g[2].Teklif;
                if (!SonPas(d)) return null;
                if (t == "2♣" && (y == '♥' || y == '♠')) return "2♦";
                if (t == "2♦" && y == '♠') return "2♥";
                return null;
            }

            // ── Cevapçı: 2 gerçek teklif; son sözü rakip değil, Pas ──
            if (g[1].Oyuncu != IhaleDurumu.PartnerBul(d.AktifOyuncu)) return null;
            if (!SonPas(d)) return null;
            return Cevap(e, y, d);
        }

        /// <summary>Son teklif (kayıtlı geçmişte) rakibin Pas'ı mı (araya girenin sağındaki rakip konuşmadı).</summary>
        private static bool SonPas(IhaleDurumu d)
        {
            if (d.Gecmis == null || d.Gecmis.Count == 0) return false;
            var son = d.Gecmis[d.Gecmis.Count - 1];
            return son.Teklif == "Pas";
        }

        private static string Cevap(OcEl e, char y, IhaleDurumu d)
        {
            int hp = e.Hcp;
            bool stp = e.Stopper("Sinek");
            int s = Uz(e, y);                                          // ortağın rengindeki uzunluk
            bool major = y != '♦';

            // 3NT: oynamak için (15+ dengeli ve ♣ stoperi)
            if (hp >= 15 && e.Dengeli && stp) return "3NT";

            if (major)
            {
                // Yükseltmeler (ortağın majöründe 3+ kart)
                string raise2 = y == '♥' ? "2♥" : "2♠";               // 5-9 HCP
                string cue9 = y == '♥' ? "2♦" : "2♥";                 // 9+ HCP (3+ tutuş)
                if (s >= 5 && hp <= 7) return "3" + y;                 // blokatif
                if (s >= 4)
                {
                    string spl = Splinter(e, y);
                    if (spl != null && hp >= 10) return spl;
                    if (hp >= 12) return "2NT";                        // 4+ tutuş 12+
                    if (hp >= 10) return "3♣";                         // mixed raise
                    if (hp >= 9) return cue9;
                    if (hp >= 5) return raise2;
                }
                else if (s == 3)
                {
                    if (hp >= 9) return cue9;
                    if (hp >= 5) return raise2;
                }
                // Transferler (5+ renk)
                if (hp >= 7 && e.D >= 5) return "2♣";
                if (y == '♠' && hp >= 7 && e.H >= 5) return "2♦";
                // ♥ araya girişinde ♠ F1R: 4+♠ 7+ HCP, 3 kartlık ♥ desteği yok
                if (y == '♥' && hp >= 7 && e.S >= 4) return "1♠";
                if (hp >= 8 && hp <= 11 && e.Dengeli && stp) return "1NT";
                return null;
            }

            // y == ♦ : ortağın araya girişi ♦ (transfer yok)
            if (s >= 5 && hp <= 7) return "3♦";                        // blokatif
            if (s >= 3 && hp >= 10) return "2♣";                       // cuebid: davet+
            if (s >= 3 && hp >= 5 && !(hp >= 7 && (e.H >= 4 || e.S >= 4))) return "2♦";
            if (hp >= 12 && hp <= 14 && e.Dengeli && stp && e.H < 4 && e.S < 4) return "2NT";
            if (hp >= 7 && (e.H >= 4 || e.S >= 4))
                return e.S > e.H ? "1♠" : "1♥";
            if (hp >= 8 && hp <= 11 && e.Dengeli && stp) return "1NT";
            if (hp >= 5 && hp <= 11 && e.H >= 6) return "2♥";
            if (hp >= 5 && hp <= 11 && e.S >= 6) return "2♠";
            if (hp >= 5 && hp <= 12 && e.H >= 7) return "3♥";
            if (hp >= 5 && hp <= 12 && e.S >= 7) return "3♠";
            return null;
        }

        /// <summary>Splinter: tutuş + kısalık (tek kart / void). ♠ kısalığı 3♠ (♥ araya girişinde), diğerleri 4. seviye.</summary>
        private static string Splinter(OcEl e, char y)
        {
            if (y == '♥' && e.S <= 1) return "3♠";
            if (e.C <= 1) return "4♣";
            if (e.D <= 1) return "4♦";
            if (y == '♠' && e.H <= 1) return "4♥";
            return null;
        }
    }
}
