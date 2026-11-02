using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// Partnerin Zayıf İki açılışına cevap.
    ///   Uyum (3+) ve 14+ HP  : majörde 4M, minörde 3NT (15+) / 3♦
    ///   Majörde 5+ uyum      : 4M (preemptif / feda)
    ///   Uyum (3+) ve 6+ HP   : 3 seviyesine kaldır
    ///   Diğer her durum      : Pas  (yeni renk 3 seviyesinde ZORLAYICI olduğu için verilmez)
    /// 
    /// Kapsam dışı (ileride): 2NT (Ogust) ve açıcının 2NT cevabı, yeni renk zorlaması.
    /// </summary>
    public class ZayifIkiCevap : IKonvansiyon
    {
        public string Ad => "Zayıf İki Cevap";
        public string Aciklama => "Zayıf iki açılışına cevaplar";
        public bool AktifMi { get; set; } = true;
        public int Oncelik => 9;

        /// <summary>Multi 2♦ açıksa 2♦ burada ele alınmaz.</summary>
        public bool Multi2DAcik { get; set; } = false;

        public bool UygunMu(IhaleDurumu durum)
        {
            return Hesapla(durum) != null;
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return Hesapla(durum) ?? "Pas";
        }

        // null → konvansiyon uygulanmaz
        private string Hesapla(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null) return null;
            if (durum.AktifOyuncuEli.Count != 13) return null;
            if (durum.IhaleBittiMi()) return null;

            bool partnerMi, rakipMi;
            int pasSayisi;
            string acilis = ZayifIkiYardimci.ZayifIkiAcilisi(durum, out partnerMi, out rakipMi, out pasSayisi);
            if (acilis == null || !partnerMi) return null;
            if (acilis == "2♦" && Multi2DAcik) return null;

            // Açıcının hemen ardından rakip Pas demiş olmalı (tam 1 Pas)
            if (pasSayisi != 1) return null;

            char sembol = acilis[1];
            string renk = ZayifIkiYardimci.RenkAdi(sembol);
            bool majorMu = sembol == '♠' || sembol == '♥';

            var el = durum.AktifOyuncuEli;
            int hp = ElDegerlendirici.HCP(el);
            int uyum = ElDegerlendirici.RenkUzunlugu(el, renk);

            if (uyum >= 3 && hp >= 14)
            {
                if (majorMu) return "4" + sembol;
                return hp >= 15 ? "3NT" : "3" + sembol;
            }

            // Açıcı 6 kart vaat eder: majörde 2 kartlık (doubleton) destek bile 8'li fit yapar.
            // 15+ HP ile oyun: ortak en fazla 10 HP → toplam ~25, 8'li majör fiti.
            if (majorMu && uyum == 2 && hp >= 15)
                return "4" + sembol;

            if (majorMu && uyum >= 5)
                return "4" + sembol;

            if (uyum >= 3 && hp >= 6)
                return "3" + sembol;

            return "Pas";
        }
    }
}
