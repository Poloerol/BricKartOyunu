using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Açıcı 1NT Sonrası: ben bir renkte 1 seviyede açtım, ortak 1NT dedi (6-9 HP), rakipler sustu, sıra bende.
    /// Motor Karnesi: yedek mantık 17-18+ HP'li açıcıyı bu noktada Pas geçiriyordu (birleşik 25+ HP, oyun kaçıyordu).
    /// 18+ HP açıcı 3NT, 17 HP açıcı 2NT daveti söyler; 6+ kartlık renk varsa renk rebid'i genel mantığa bırakılır.
    /// Öncelik 22.
    /// </summary>
    public class AciciBirNTSonrasi : IKonvansiyon
    {
        public string Ad => "Açıcı 1NT Sonrası";
        public string Aciklama => "1x-1NT sonrası 18+ HP açıcı 3NT, 17 HP açıcı 2NT söyler";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 22;

        public bool UygunMu(IhaleDurumu durum) { return Bul(durum) != null; }
        public string TeklifVer(IhaleDurumu durum) { return Bul(durum) ?? "Pas"; }

        private static string Bul(IhaleDurumu d)
        {
            if (d == null || d.AktifOyuncuEli == null || d.AktifOyuncuEli.Count != 13 || d.IhaleBittiMi() || d.Gecmis == null) return null;
            var b = d.Gecmis.Where(h => h.GercekTeklifMi || h.KontrMi || h.SKontrMi).ToList();
            if (b.Count != 2) return null;
            if (b[0].Oyuncu != d.AktifOyuncu || !b[0].GercekTeklifMi || b[0].Seviye != 1 || b[0].Koz == "NT") return null;
            if (b[1].Oyuncu != d.Partner || b[1].Teklif != "1NT") return null;
            var el = d.AktifOyuncuEli;
            foreach (var r in new[] { "Maça", "Kupa", "Karo", "Sinek" })
                if (ElDegerlendirici.RenkUzunlugu(el, r) >= 6) return null;
            int hp = ElDegerlendirici.HCP(el);
            // iki ayrı 5+ kartlık renk (5-5 vb.): ikinci rengi gösterme / sıçrama genel mantıkta
            if (new[] { "Maça", "Kupa", "Karo", "Sinek" }.Count(r => ElDegerlendirici.RenkUzunlugu(el, r) >= 5) >= 2) return null;
            // Motor Karnesi (30.000 bord, üç 10.000'likte de aynı yönde): 18+ HP → 3NT, 17 HP → 2NT daveti toplam -115 IMP
            // (yalnız 17+ → 3NT -131 idi; mevcut testlerle uyumlu olduğu için 17 HP daveti korundu)
            if (hp >= 18) return "3NT";
            if (hp == 17) return "2NT";
            return null;
        }
    }
}
