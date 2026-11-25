using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Müdahale Sonrası Güçlü Cevap: ortak 1 seviyede renk açtı, sağ rakip renkle müdahale etti, ilk konuşan benim
    /// ve elim güçlü (17+ HP) ve uzun (6+ kartlı) bir renk taşıyor.
    ///
    /// Kural (dünya standardı): oyun zorlayan sıçramalı yeni renk (jump shift) —
    ///   rakibin teklifinin üstünde en ucuz teklif seviyesinden bir seviye yukarıda yeni rengi teklif etmek.
    ///   Örnek: 1♦-(1♠)-3♥ (2♥ en ucuz seviyedir, 3♥ sıçramadır).
    /// En ucuz teklif zaten ortağın renginden ya da rakibin renginden başka bir renkte olmalı; sıçrama 3 seviyeyi
    /// aşıyorsa (örneğin 1♠-(2♥)-4♦) kural uygulanmaz, güçlü el başka yoldan devam eder.
    /// Güçlü ama uzun rengi olmayan eller, fit cue-bid ya da kontra gibi diğer yöntemlere bırakılır.
    /// Öncelik 19 (Negative Double'dan önce): 17+ HP ve 6+ kartlı renkle kontra yerine renk bildirilir.
    /// </summary>
    public class MudahaleSonrasiGucluCevap : IKonvansiyon
    {
        public string Ad => "Müdahale Sonrası Güçlü Cevap";
        public string Aciklama => "Ortağın 1 seviye açılışına rakip müdahale ettikten sonra 17+ HP ve 6+ kartlı renkle sıçramalı yeni renk (oyun zorlaması)";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 19;

        public bool UygunMu(IhaleDurumu durum) { return Bul(durum) != null; }
        public string TeklifVer(IhaleDurumu durum) { return Bul(durum) ?? "Pas"; }

        private static readonly string[] Sira = { "Sinek", "Karo", "Kupa", "Maça" };

        private static string Sem(string koz)
        {
            switch (koz) { case "Maça": return "♠"; case "Kupa": return "♥"; case "Karo": return "♦"; case "Sinek": return "♣"; }
            return null;
        }

        private static string Bul(IhaleDurumu d)
        {
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13 || d.IhaleBittiMi() || d.Gecmis == null) return null;

            // Gerçek teklifler / kontralar: tam olarak [ortak açılış, rakip müdahale] ve ben henüz konuşmadım
            var b = d.Gecmis.Where(h => h.GercekTeklifMi || h.KontrMi || h.SKontrMi).ToList();
            if (b.Count != 2) return null;
            var acilis = b[0]; var mud = b[1];
            if (!acilis.GercekTeklifMi || !mud.GercekTeklifMi) return null;
            if (acilis.Oyuncu != d.Partner || !d.Rakipler.Contains(mud.Oyuncu)) return null;
            if (acilis.Seviye != 1 || acilis.Koz == "NT") return null;
            if (mud.Koz == "NT" || mud.Seviye > 2 || mud.Koz == acilis.Koz) return null;

            var el = d.AktifOyuncuEli;
            if (ElDegerlendirici.HCP(el) < 17) return null;

            // En az 6 kartlı, açılış ve müdahale renginden farklı en uzun renk
            string renk = null; int uz = 0;
            foreach (var r in Sira)
            {
                if (r == acilis.Koz || r == mud.Koz) continue;
                int n = ElDegerlendirici.RenkUzunlugu(el, r);
                if (n >= 6 && n > uz) { renk = r; uz = n; }
            }
            if (renk == null) return null;

            // En ucuz seviye: rengim rakibin renginden yüksekse aynı seviye, değilse bir üstü; sıçrama = bir seviye daha
            int ucuz = Array.IndexOf(Sira, renk) > Array.IndexOf(Sira, mud.Koz) ? mud.Seviye : mud.Seviye + 1;
            int sicrama = ucuz + 1;
            if (sicrama > 3) return null;
            return sicrama + Sem(renk);
        }
    }
}
