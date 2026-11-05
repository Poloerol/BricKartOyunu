using System;
using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneClub
{
    /// <summary>Bir elin kural tablolarında kullanılan özeti (HCP, renk uzunlukları, kısalıklar).</summary>
    internal sealed class OcEl
    {
        public List<Card> El;
        public int Hcp, S, H, D, C;
        public bool Dengeli;

        public OcEl(List<Card> el)
        {
            El = el;
            Hcp = ElDegerlendirici.HCP(el);
            S = ElDegerlendirici.RenkUzunlugu(el, "Maça");
            H = ElDegerlendirici.RenkUzunlugu(el, "Kupa");
            D = ElDegerlendirici.RenkUzunlugu(el, "Karo");
            C = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
            Dengeli = ElDegerlendirici.DengeliEl(el);
        }

        public bool Stopper(string renk) { return ElDegerlendirici.StopperVar(El, renk); }
        public bool Singleton { get { return S == 1 || H == 1 || D == 1 || C == 1; } }
    }

    /// <summary>Sıralı kural satırı: koşul doğruysa teklif verilir (kitaptaki tablo satırı).</summary>
    internal sealed class OcKural
    {
        public string Teklif; public Func<OcEl, bool> Kosul;
        public OcKural(string teklif, Func<OcEl, bool> kosul) { Teklif = teklif; Kosul = kosul; }
    }

    internal static class OcTablo
    {
        public static string Sec(IEnumerable<OcKural> kurallar, OcEl e, string varsayilan)
        {
            foreach (var k in kurallar) if (k.Kosul(e)) return k.Teklif;
            return varsayilan;
        }
        public static OcKural K(string teklif, Func<OcEl, bool> kosul) { return new OcKural(teklif, kosul); }
    }

    /// <summary>
    /// 1♣ açılışından sonra cevapçının ilk cevabına göre kurulan gelişim tabanı (SAS kitabı s.15-23).
    /// Üç rolü karşılar (rakip yalnızca Pas demiş olmalı):
    ///   Aşama A  (2 gerçek teklif): 1♣ (biz) - X (partner)            → AÇANIN YENİDEN TEKLİFİ
    ///   Aşama B  (3 gerçek teklif): 1♣ (partner) - X (biz) - R        → CEVAPÇININ DEVAMI
    ///   Aşama C  (4 gerçek teklif): 1♣ (biz) - X - R (biz) - Y        → AÇANIN RÖLE CEVABI
    ///   Aşama D  (5 gerçek teklif): 1♣ - X - R - Y - Z (açanın röle cevabı)  → CEVAPÇININ KARARI
    /// </summary>
    public abstract class OneClub_GelisimTabani : IAcilisGelisimi
    {
        protected abstract string Cevap { get; }                       // ör. "1♥"
        /// <summary>Açılış teklifi (1♣ gelişimleri için "1♣", 1♦ gelişimleri için "1♦").</summary>
        protected virtual string Acilis { get { return "1♣"; } }
        /// <summary>Açanın yeniden teklifi kitapta bu elde tanımlı mı (değilse sınıf devreye girmez, genel motor sürdürür).</summary>
        internal virtual bool AciciRebidTanimli(OcEl e) { return true; }
        internal abstract string AciciRebid(OcEl e);
        internal abstract string CevapciDevam(OcEl e, string rebid);
        internal virtual string AciciRoleCevap(OcEl e, string rebid, string devam) { return null; }
        /// <summary>Aşama D: açanın röle cevabından sonra cevapçının kararı (tanımlı değilse null).</summary>
        internal virtual string CevapciKarar(OcEl e, string rebid, string devam, string roleCevap) { return null; }
        /// <summary>Röle sekansı (rebid, devam) cevapçı kararına kadar tanımlıysa true: sonrasında açan Pas der.</summary>
        internal virtual bool KararTanimli(string rebid, string devam) { return false; }
        /// <summary>Aşama E (6. gerçek teklif): cevapçının kararından sonra açanın cevabı (tanımsızsa null).</summary>
        internal virtual string AciciSon(OcEl e, string rebid, string devam, string roleCevap, string karar) { return null; }
        /// <summary>Açanın yeniden teklifine cevapçı devamı kitapta tanımlı mı (değilse "gelişim natürel": sınıf devreye girmez).</summary>
        internal virtual bool DevamTanimli(string rebid) { return true; }

        public abstract string Aciklama { get; }
        public virtual int Oncelik { get { return 41; } }

        public bool UygunMu(List<Card> el, IhaleDurumu durum)
        {
            return Rol(el, durum) != 0;
        }

        public string TeklifVer(List<Card> el, IhaleDurumu durum)
        {
            if (el == null) return "Pas";
            var e = new OcEl(el);
            var g = Gercek(durum);
            switch (Rol(el, durum))
            {
                case 1: return AciciRebid(e);
                case 2: return CevapciDevam(e, g[2].Teklif);
                case 3: return AciciRoleCevap(e, g[2].Teklif, g[3].Teklif) ?? "Pas";
                case 4: return CevapciKarar(e, g[2].Teklif, g[3].Teklif, g[4].Teklif) ?? "Pas";
                case 5: return AciciSon(e, g[2].Teklif, g[3].Teklif, g[4].Teklif, g[5].Teklif) ?? "Pas";   // tanımsızsa açan sözleşmeyi kabul eder
                default: return "Pas";
            }
        }

        private static List<IhaleHamlesi> Gercek(IhaleDurumu durum)
        {
            return durum.Gecmis.Where(h => h.GercekTeklifMi).ToList();
        }

        /// <summary>0: uygun değil, 1: açıcı rebid, 2: cevapçı devam, 3: açıcı röle cevabı, 4: cevapçı kararı.</summary>
        private int Rol(List<Card> el, IhaleDurumu durum)
        {
            if (durum == null || el == null || el.Count != 13) return 0;
            var g = Gercek(durum);
            if (g.Count < 2 || g.Count > 6) return 0;
            if (g.Any(h => durum.Rakipler.Contains(h.Oyuncu))) return 0;
            if (g[0].Teklif != Acilis || g[1].Teklif != Cevap) return 0;
            if (IhaleDurumu.PartnerBul(g[0].Oyuncu) != g[1].Oyuncu) return 0;

            Player acici = g[0].Oyuncu, cevapci = g[1].Oyuncu;
            if (g.Count == 2 && durum.AktifOyuncu == acici) return AciciRebidTanimli(new OcEl(el)) ? 1 : 0;
            if (g.Count == 3 && g[2].Oyuncu == acici && durum.AktifOyuncu == cevapci) return DevamTanimli(g[2].Teklif) ? 2 : 0;
            if (g.Count == 4 && g[3].Oyuncu == cevapci && durum.AktifOyuncu == acici)
            {
                // Kitapta cevabı tanımlı olmayan röle/SAS devamlarında bu sınıf devreye girmez (genel mantık sürdürür).
                return AciciRoleCevap(new OcEl(el), g[2].Teklif, g[3].Teklif) != null ? 3 : 0;
            }
            if (g.Count == 5 && g[4].Oyuncu == acici && durum.AktifOyuncu == cevapci)
                return CevapciKarar(new OcEl(el), g[2].Teklif, g[3].Teklif, g[4].Teklif) != null ? 4 : 0;
            if (g.Count == 6 && g[5].Oyuncu == cevapci && durum.AktifOyuncu == acici
                && (KararTanimli(g[2].Teklif, g[3].Teklif) || AciciSon(new OcEl(el), g[2].Teklif, g[3].Teklif, g[4].Teklif, g[5].Teklif) != null))
                return 5;
            return 0;
        }
    }
}
