using System;
using System.Collections.Generic;

namespace BricKartOyunu.Class
{
    public enum IhaleHatasi
    {
        Yok,            // par ile aynı sonuç
        PasGecildi,     // ihale pas geçildi ama par farklı
        Batis,          // kontrat çift dummy'de yatıyor
        OyunKacirildi,
        SlamKacirildi,
        BuyukSlamKacirildi,
        YanlisKoz,      // daha iyi bir kozda daha yüksek skor vardı
        RakipFedakarlik,// rakip fedakârlık yapmalıydı / par rakipte
        ParUstu,        // par'dan iyi (rakip hata yaptı; nedeni sınıflanamadı)
        RakipBuyukSlam, // rakip büyük slam kaçırdı
        RakipSlam,      // rakip slam kaçırdı
        RakipOyun,      // rakip oyun kaçırdı
        RakipFedakarlik2, // rakip fedakârlık yapmadı
        RakipRekabet,   // rakip parskor rekabetinde geri kaldı
        KucukFark       // fazla löve / farklı deklaran gibi küçük fark
    }

    /// <summary>Gerçekleşen ihaleyi çift dummy tablosu ve par ile karşılaştırır.</summary>
    public sealed class IhaleDegerlendirme
    {
        public bool Pas;
        public int Seviye, Koz, Deklaran;
        public KontrDurumu Kontr;
        public bool Kontrlu { get { return Kontr != KontrDurumu.Yok; } }
        public int DdLove;
        public int DdNsSkor;       // çift dummy'de bu kontratın K-G skoru
        public int ParNsSkor;
        public ParSonuc Par;
        /// <summary>K-G görüşüyle IMP farkı (negatif = K-G par'ın gerisinde).</summary>
        public int ImpNs;
        /// <summary>Kontratı oynayan tarafın görüşüyle IMP farkı.</summary>
        public int ImpDeklaran;
        public IhaleHatasi Hata;
        /// <summary>Gerçek sonucun par'dan toplam IMP sapması (her zaman ≥ 0).</summary>
        public int Sapma { get { return Math.Abs(ImpNs); } }
        /// <summary>Par'ın gerisinde kalan taraf: 0 = K-G, 1 = D-B, -1 = yok.</summary>
        public int Sorumlu;
        public string Baslik;
        public readonly List<string> Notlar = new List<string>();

        public string KontratMetni { get { return Pas ? "Pas" : ParHesabi.KontratMetni(Seviye, Koz, Kontr) + " (" + ParHesabi.KoltukHarf[Deklaran] + ")"; } }

        public static IhaleDegerlendirme Degerlendir(DDTablo t, int dagitici, bool nsZon, bool ewZon,
                                                     bool pas, int seviye, int koz, int deklaran, KontrDurumu kontr)
        {
            var r = new IhaleDegerlendirme { Pas = pas, Seviye = seviye, Koz = koz, Deklaran = deklaran, Kontr = kontr };
            r.Par = ParHesabi.Hesapla(t, dagitici, nsZon, ewZon);
            r.ParNsSkor = r.Par.NsSkor;

            if (pas)
            {
                r.DdNsSkor = 0;
                r.ImpNs = MacDurumu.Imp(0 - r.ParNsSkor);
                r.ImpDeklaran = -Math.Abs(r.ImpNs);
                r.Sorumlu = r.ParNsSkor > 0 ? 0 : r.ParNsSkor < 0 ? 1 : -1;
                if (r.ParNsSkor == 0) { r.Hata = IhaleHatasi.Yok; r.Baslik = "Doğru: dört pas par sonucu"; }
                else
                {
                    r.Hata = IhaleHatasi.PasGecildi;
                    r.Baslik = "Pas geçildi, ihale vardı";
                    r.Notlar.Add(r.Par.Metin);
                }
                return r;
            }

            int taraf = deklaran & 1;
            bool zonda = taraf == 0 ? nsZon : ewZon;
            r.DdLove = t.Al(koz, deklaran);
            int gereken = seviye + 6;
            int declSkor = ParHesabi.Skor(seviye, koz, kontr, zonda, r.DdLove);
            r.DdNsSkor = taraf == 0 ? declSkor : -declSkor;
            r.ImpNs = MacDurumu.Imp(r.DdNsSkor - r.ParNsSkor);
            r.ImpDeklaran = taraf == 0 ? r.ImpNs : -r.ImpNs;
            r.Sorumlu = r.DdNsSkor < r.ParNsSkor ? 0 : r.DdNsSkor > r.ParNsSkor ? 1 : -1;

            bool yapilir = r.DdLove >= gereken;
            r.Notlar.Add("Çift dummy: " + ParHesabi.KoltukHarf[deklaran] + " oynarsa " + r.DdLove + " löve" +
                         (yapilir ? " → yapılır (" + (r.DdLove > gereken ? "+" + (r.DdLove - gereken) : "tam") + ")" : " → " + (gereken - r.DdLove) + " batar"));
            r.Notlar.Add(r.Par.Metin);

            // taraf için en iyi yapılan kontratlar
            int bestSkor = int.MinValue, bestKoz = -1, bestSev = 0;
            for (int k = 0; k < 5; k++)
            {
                int l = t.TarafLove(k, taraf);
                int sev = Math.Min(7, l - 6);
                if (sev < 1) continue;
                bool kx;
                int sk = ParHesabi.OtomatikSkor(sev, k, zonda, l, out kx);
                // aynı löve için en iyi pratik kontrat: oyun/slam eşiğine göre seviye
                if (sk > bestSkor) { bestSkor = sk; bestKoz = k; bestSev = sev; }
            }

            // sonuç par ile aynı mı?
            int parDekl = taraf == 0 ? r.ParNsSkor : -r.ParNsSkor;   // deklaran tarafının gözüyle par
            if (declSkor == parDekl) { r.Hata = IhaleHatasi.Yok; r.Baslik = "Optimum: par sonucu"; return r; }
            if (declSkor > parDekl)
            {
                // sorumlu karşı taraf: par'ı onun kontratı mı?
                int rk = 1 - taraf;
                r.Hata = IhaleHatasi.ParUstu; r.Baslik = "Par'ın üstünde (rakipler fırsatı kaçırdı)";
                if (!r.Par.Pas && r.Par.Taraf == rk)
                {
                    int ps = r.Par.Seviye;
                    if (r.Par.Fedakar) { r.Hata = IhaleHatasi.RakipFedakarlik2; r.Baslik = "Rakip fedakârlık yapmalıydı"; }
                    else if (ps == 7) { r.Hata = IhaleHatasi.RakipBuyukSlam; r.Baslik = "Rakip büyük slamı kaçırdı"; }
                    else if (ps == 6) { r.Hata = IhaleHatasi.RakipSlam; r.Baslik = "Rakip slamı kaçırdı"; }
                    else if (SeviyeOyunMu(ps, r.Par.Koz)) { r.Hata = IhaleHatasi.RakipOyun; r.Baslik = "Rakip oyunu kaçırdı"; }
                    else { r.Hata = IhaleHatasi.RakipRekabet; r.Baslik = "Rakip parskor rekabetinde geri kaldı"; }
                }
                return r;
            }

            if (!yapilir)
            {
                r.Hata = IhaleHatasi.Batis;
                r.Baslik = "Fazla yüksek: kontrat yatıyor";
                if (bestKoz >= 0)
                    r.Notlar.Add("Bu taraf " + ParHesabi.KontratMetni(bestSev, bestKoz, false) + " yapabilirdi (+" + bestSkor + ")");
                else
                    r.Notlar.Add("Bu tarafın yapılan kontratı yok; rakip kontratına karşı savunma/fedakârlık gerekirdi");
                return r;
            }

            // yapılıyor ama par'dan düşük
            if (r.Par.Taraf != taraf && !r.Par.Pas)
            {
                r.Hata = IhaleHatasi.RakipFedakarlik;
                r.Baslik = "Rakibin kontratı par'dı";
                r.Notlar.Add("Par kontratını " + (r.Par.Taraf == 0 ? "N/S" : "E/W") + " oynuyor; bu taraf kontratı yapsa da par'ın gerisinde kalıyor");
                return r;
            }
            int oyunSev = bestKoz == 4 ? 3 : (bestKoz == 0 || bestKoz == 1) ? 4 : 5;
            if (bestSev == 7 && seviye < 7)
            {
                r.Hata = IhaleHatasi.BuyukSlamKacirildi; r.Baslik = "Büyük slam kaçırıldı";
                r.Notlar.Add(ParHesabi.KontratMetni(7, bestKoz, false) + " yapılırdı");
            }
            else if (bestSev >= 6 && seviye < 6)
            {
                r.Hata = IhaleHatasi.SlamKacirildi; r.Baslik = "Slam kaçırıldı";
                r.Notlar.Add(ParHesabi.KontratMetni(6, bestKoz, false) + " yapılırdı");
            }
            else if (bestSev >= oyunSev && !SeviyeOyunMu(seviye, koz))
            {
                r.Hata = IhaleHatasi.OyunKacirildi; r.Baslik = "Oyun kaçırıldı";
                r.Notlar.Add(ParHesabi.KontratMetni(oyunSev, bestKoz, false) + " yapılırdı");
            }
            else if (bestKoz != koz && bestSkor > declSkor)
            {
                r.Hata = IhaleHatasi.YanlisKoz; r.Baslik = "Daha iyi koz vardı";
                r.Notlar.Add(ParHesabi.KozSembol[bestKoz] + " kozunda " + t.TarafLove(bestKoz, taraf) + " löve (+" + bestSkor + ")");
            }
            else
            {
                r.Hata = IhaleHatasi.KucukFark; r.Baslik = "Küçük fark";
                if (bestKoz >= 0 && bestSkor > declSkor) r.Notlar.Add("En iyi pratik sonuç " + ParHesabi.KontratMetni(bestSev, bestKoz, false) + " (+" + bestSkor + ")");
            }
            return r;
        }

        private static bool SeviyeOyunMu(int seviye, int koz)
        {
            if (seviye >= 6) return true;
            if (koz == 4) return seviye >= 3;
            if (koz == 0 || koz == 1) return seviye >= 4;
            return seviye >= 5;
        }
    }
}
