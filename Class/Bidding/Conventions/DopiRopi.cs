using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// DOPI / ROPI — Partnerin Blackwood (4NT, Roman Key Card) sorusuna rakip
    /// müdahale ettiğinde cevap sistemi.
    /// 
    /// DOPI = Double = zerO, Pass = 1
    ///   Rakip 4NT'nin ardından 5 seviyesinde, koz renginden DÜŞÜK bir renk söylerse
    ///   (ör. koz ♠, rakip 5♦):
    ///     Dbl      = 0 (veya 3) anahtar kart
    ///     Pas      = 1 (veya 4) anahtar kart
    ///     1. adım  = 2 anahtar kart, koz kızı YOK  (müdahaleden sonraki en ucuz teklif)
    ///     2. adım  = 2 anahtar kart, koz kızı VAR  (bir sonraki teklif)
    /// 
    /// ROPI = Redouble = zerO, Pass = 1
    ///   Rakip 4NT'ye kontr verdiyse:
    ///     Rdbl     = 0 (veya 3) anahtar kart
    ///     Pas      = 1 (veya 4) anahtar kart
    ///     5♣       = 2 anahtar kart, koz kızı YOK
    ///     5♦       = 2 anahtar kart, koz kızı VAR
    /// 
    /// Anahtar kart = 4 As + kozun papazı (5 adet).
    /// 
    /// Kapsam: yalnızca CEVAP veren taraf (partner 4NT dedi, hemen ardından rakip
    /// müdahale etti, sıra bizde). Müdahale koz renginin 5 seviyesinden YÜKSEKSE
    /// (DEPO gerekir) bu konvansiyon devreye girmez.
    /// </summary>
    public class DopiRopi : IKonvansiyon
    {
        public string Ad => "DOPI-ROPI";
        public bool AktifMi { get; set; } = true;

        // Blackwood (15) ve Gerber (13)'ten ÖNCE gelmeli: rakip kontr verdiğinde
        // (Dbl "gerçek teklif" sayılmadığı için) son gerçek teklif hâlâ 4NT'dir ve
        // Blackwood müdahaleyi yok sayıp normal cevap verirdi.
        public int Oncelik => 9;

        // ─── Projedeki teklif jetonları — farklıysa BURADAN değiştirin ───
        private const string PAS = "Pas";
        private const string KONTR = "Dbl";
        private const string REKONTR = "Rdbl";

        // Renk sırası (alçaktan yükseğe) ve sembolleri
        private static readonly string[] RenkSirasi = { "Sinek", "Karo", "Kupa", "Maça" };
        private static readonly string[] Semboller = { "♣", "♦", "♥", "♠" };

        private enum MudahaleTuru { Dopi, Ropi }

        private class Analiz
        {
            public string Koz;
            public int KozSirasi;          // RenkSirasi içindeki indeks
            public MudahaleTuru Tur;
            public int MudahaleRengiSirasi; // yalnızca DOPI'de anlamlı
        }

        // ═══════════════════════════════════════════════════════════════════
        // DURUM ÇÖZÜMLEME
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Geçmişin sonu şu biçimde olmalı:  ... Partner 4NT → Rakip (Dbl | 5♣..5♥ koz altı renk) → SIRA BİZDE.
        /// Uygun değilse null döner.
        /// </summary>
        private Analiz Cozumle(IhaleDurumu durum)
        {
            if (durum == null || durum.Gecmis == null || durum.AktifOyuncuEli == null)
                return null;

            var liste = durum.Gecmis.ToList();
            if (liste.Count < 3) return null;

            var son = liste[liste.Count - 1];      // rakibin müdahalesi
            var onceki = liste[liste.Count - 2];   // partnerin 4NT'si

            if (onceki.Teklif != "4NT" || onceki.Oyuncu != durum.Partner) return null;
            if (!durum.Rakipler.Contains(son.Oyuncu)) return null;

            // Anlaşılan koz: 4NT'DEN ÖNCEKİ ihaleye göre belirlenir
            // (rakibin müdahale rengi koz sanılmasın).
            var oncekiDurum = new IhaleDurumu
            {
                AktifOyuncu = durum.AktifOyuncu,
                AktifOyuncuEli = durum.AktifOyuncuEli,
                Anlasma = durum.Anlasma,
                Gecmis = liste.Take(liste.Count - 2).ToList(),
            };

            string koz = oncekiDurum.AnlasilanKoz();
            int kozSirasi = Array.IndexOf(RenkSirasi, koz);
            if (kozSirasi < 0) return null; // NT veya koz yok → Blackwood değil

            // ─── ROPI: rakip 4NT'ye kontr verdi ───
            if (son.Teklif == KONTR)
            {
                return new Analiz { Koz = koz, KozSirasi = kozSirasi, Tur = MudahaleTuru.Ropi };
            }

            // ─── DOPI: rakip 5 seviyesinde, koz renginden düşük bir renk söyledi ───
            if (!string.IsNullOrEmpty(son.Teklif) && son.Teklif[0] == '5')
            {
                string mudahaleRengi = KozYardimcisi.TekliftenKozCikar(son.Teklif);
                int mudahaleSirasi = Array.IndexOf(RenkSirasi, mudahaleRengi);

                if (mudahaleSirasi >= 0 && mudahaleSirasi < kozSirasi)
                {
                    return new Analiz
                    {
                        Koz = koz,
                        KozSirasi = kozSirasi,
                        Tur = MudahaleTuru.Dopi,
                        MudahaleRengiSirasi = mudahaleSirasi
                    };
                }
            }

            // Pas, 6 seviyesi, koz üstü müdahale vb. → bu konvansiyon değil
            return null;
        }

        // ═══════════════════════════════════════════════════════════════════
        // UYGUNLUK
        // ═══════════════════════════════════════════════════════════════════

        public bool UygunMu(IhaleDurumu durum)
        {
            return Cozumle(durum) != null;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TEKLİF VER
        // ═══════════════════════════════════════════════════════════════════

        public string TeklifVer(IhaleDurumu durum)
        {
            var el = durum.AktifOyuncuEli;
            if (el == null || el.Count != 13) return null;

            var analiz = Cozumle(durum);
            if (analiz == null) return null; // motor sıradaki konvansiyona geçsin

            int anahtarKart = AnahtarKartSayisi(el, analiz.Koz);
            bool kozKizi = el.Any(c => c.Suit == analiz.Koz && c.Value == 12);

            // 0|3 → 1. küme, 1|4 → 2. küme, 2|5 → 3. küme
            switch (anahtarKart % 3)
            {
                case 0:
                    return analiz.Tur == MudahaleTuru.Ropi ? REKONTR : KONTR;

                case 1:
                    return PAS;

                default: // 2 anahtar kart
                    string[] adimlar = IkiKartAdimlari(analiz);
                    return kozKizi ? adimlar[1] : adimlar[0];
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // YARDIMCILAR
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// "2 anahtar kart" cevapları: [0] = koz kızı yok, [1] = koz kızı var.
        /// </summary>
        private string[] IkiKartAdimlari(Analiz analiz)
        {
            if (analiz.Tur == MudahaleTuru.Ropi)
            {
                return new[] { "5♣", "5♦" };
            }

            // DOPI: müdahaleden sonraki en ucuz iki 5-seviyesi renk teklifi
            var mevcut = new List<string>();
            for (int r = analiz.MudahaleRengiSirasi + 1; r < RenkSirasi.Length; r++)
            {
                mevcut.Add("5" + Semboller[r]);
            }

            // Koz ♠ ve rakip 5♥ ise tek teklif kalır (5♠): kız bilgisi gösterilemez.
            string ilk = mevcut[0];
            string ikinci = mevcut.Count > 1 ? mevcut[1] : mevcut[0];
            return new[] { ilk, ikinci };
        }

        /// <summary>
        /// Anahtar kart sayısı: 4 As + koz papazı.
        /// </summary>
        private int AnahtarKartSayisi(List<Card> el, string koz)
        {
            int asSayisi = el.Count(c => c.Value == 14);
            int kozPapazi = el.Count(c => c.Suit == koz && c.Value == 13);
            return asSayisi + kozPapazi;
        }
    }
}
