using System;
using System.Collections.Generic;
using System.Text;

namespace BricKartOyunu.Class
{
    /// <summary>Duplicate dışındaki puanlama sistemleri.</summary>
    public enum PuanTuru { Rubber, ChicagoDuplicate, ChicagoRubber }

    /// <summary>Bitmiş bir bordun puan hesabı için gereken bilgi (hepsi FİZİKSEL koltuklara göre: K-G / D-B).</summary>
    public class BordGirdisi
    {
        public int BordNo;
        public bool Pas;                 // dört pas
        public Player Dagitici;          // fiziksel dağıtıcı (Chicago zonu için)
        public bool DeklaranNS;          // deklaran Kuzey-Güney mi
        public int Seviye;
        public string Koz;               // "♣", "♦", "♥", "♠", "NT"
        public KontrDurumu Kontr;
        public int AlinanLove;           // deklaran tarafının löveleri
    }

    /// <summary>Tek bordun puan tablosundaki satırı (K-G ve D-B açısından, çizginin altı/üstü).</summary>
    public class PuanSatiri
    {
        public int BordNo;
        public string Kontrat;           // "4♠X K" ya da "Pas"
        public string Sonuc;             // "+1", "=", "-2"
        public bool NsZon, EwZon;        // bu bordda geçerli zon
        public int NsAlt, EwAlt;         // çizginin altına yazılan (rubber/Chicago rubber) ya da bordun duplicate puanı
        public int NsUst, EwUst;         // çizginin üstüne yazılan
        public string Not = "";          // "Oyun", "Rubber bitti (+700)" gibi
    }

    /// <summary>Oturumun o ana kadarki puan durumu.</summary>
    public class PuanDurumu
    {
        public PuanTuru Tur;
        public List<PuanSatiri> Satirlar = new List<PuanSatiri>();
        public bool NsZon, EwZon;        // SIRADAKİ bord için zon
        public int NsOyun, EwOyun;       // güncel rubberda kazanılan oyun (yalnız Rubber)
        public int NsAlt, EwAlt;         // güncel oyunda çizgi altı (parsiyel)
        public int NsToplam, EwToplam;   // oturumun toplamı (biten rubberlar/gruplar + güncel)
        public int Biten;                // biten rubber ya da dört bordluk grup sayısı

        public string Ozet()
        {
            var sb = new StringBuilder();
            string tur = Tur == PuanTuru.Rubber ? "Rubber" : Tur == PuanTuru.ChicagoRubber ? "Chicago Rubber" : "Chicago Duplicate";
            sb.AppendLine(tur);
            sb.AppendLine("Toplam: K-G " + NsToplam + "   D-B " + EwToplam);
            if (Tur != PuanTuru.ChicagoDuplicate)
                sb.AppendLine("Çizgi altı (parsiyel): K-G " + NsAlt + "   D-B " + EwAlt);
            if (Tur == PuanTuru.Rubber)
                sb.AppendLine("Oyun: K-G " + NsOyun + "   D-B " + EwOyun);
            sb.AppendLine("Sıradaki bordda zon: " + (NsZon && EwZon ? "herkes" : NsZon ? "K-G" : EwZon ? "D-B" : "yok"));
            return sb.ToString();
        }
    }

    /// <summary>
    /// Rubber, Chicago Rubber ve Chicago Duplicate puanlama. Deklaran tarafının löve/bonus değerleri duplicate ile aynıdır
    /// (<see cref="Skorlama.Hesapla"/>); fark, bonusların ve zonun nasıl bulunduğudur:
    ///  • Rubber: löve puanı çizginin altına yazılır, 100'e ulaşan taraf oyunu alır, iki oyunu alan rubberı bitirir
    ///    (rakipte oyun yoksa +700, varsa +500). Zonda olmak = o tarafın bir oyunu olması. Yarım kalan oyunlar sonraki bordlara taşınır.
    ///    Onör (honör) puanları hesaba katılmaz.
    ///  • Chicago (4 bord): 1. bord zonsuz, 2. ve 3. bord dağıtıcının tarafı zonda, 4. bord herkes zonda.
    ///    Chicago Rubber: çizgi altı parsiyel taşınır, oyun bonusu 300 (zonsuz) / 500 (zonda), dördüncü bordun sonunda
    ///    parsiyeli olan taraf +100 alır. Chicago Duplicate: her bord duplicate puanıyla yazılır, zon yalnız Chicago döngüsünden gelir.
    /// </summary>
    public static class KlasikSkor
    {
        /// <summary>Girdileri sırayla işler. <paramref name="siradakiDagitici"/> sıradaki bordun zonu için (Chicago) kullanılır.</summary>
        public static PuanDurumu Hesapla(PuanTuru tur, IList<BordGirdisi> girdiler, Player siradakiDagitici)
        {
            var d = new PuanDurumu { Tur = tur };
            int[] ust = new int[2], alt = new int[2], bitenAlt = new int[2], oyun = new int[2], tamam = new int[2];
            int[] cdPuan = new int[2];       // Chicago duplicate: güncel grubun toplamı
            int grupBord = 0;

            for (int i = 0; i < girdiler.Count; i++)
            {
                var g = girdiler[i];
                bool[] zon = ZonAl(tur, oyun, grupBord, g.Dagitici);
                var satir = new PuanSatiri { BordNo = g.BordNo, NsZon = zon[0], EwZon = zon[1] };
                d.Satirlar.Add(satir);
                grupBord++;

                if (g.Pas)
                {
                    satir.Kontrat = "Pas"; satir.Sonuc = "";
                }
                else
                {
                    int dek = g.DeklaranNS ? 0 : 1;
                    var s = Skorlama.Hesapla(g.Seviye, g.Koz, g.Kontr, zon[dek], g.AlinanLove);
                    satir.Kontrat = s.KontratMetni + (g.DeklaranNS ? " K-G" : " D-B");
                    int fark = g.AlinanLove - s.GerekenLove;
                    satir.Sonuc = fark == 0 ? "=" : fark > 0 ? "+" + fark : fark.ToString();

                    if (tur == PuanTuru.ChicagoDuplicate)
                    {
                        int ns = g.DeklaranNS ? s.Toplam : -s.Toplam;
                        satir.NsAlt = ns > 0 ? ns : 0; satir.EwAlt = ns < 0 ? -ns : 0;
                        cdPuan[0] += ns; cdPuan[1] -= ns;
                    }
                    else
                    {
                        if (s.Yapildi)
                        {
                            alt[dek] += s.Anlasma;
                            Ekle(satir, dek, true, s.Anlasma);
                            int ustPuan = s.SlamBonusu + s.KontrIkrami + s.FazlaLove;
                            ust[dek] += ustPuan; Ekle(satir, dek, false, ustPuan);
                        }
                        else
                        {
                            ust[1 - dek] += s.Ceza; Ekle(satir, 1 - dek, false, s.Ceza);
                        }

                        if (s.Yapildi && alt[dek] >= 100)
                        {
                            satir.Not = "Oyun";
                            if (tur == PuanTuru.Rubber)
                            {
                                oyun[dek]++;
                                bitenAlt[0] += alt[0]; bitenAlt[1] += alt[1];
                                alt[0] = alt[1] = 0;
                                if (oyun[dek] == 2)
                                {
                                    int bonus = oyun[1 - dek] == 0 ? 700 : 500;
                                    ust[dek] += bonus; Ekle(satir, dek, false, bonus);
                                    satir.Not = "Rubber bitti (+" + bonus + ")";
                                    tamam[0] += ust[0] + bitenAlt[0]; tamam[1] += ust[1] + bitenAlt[1];
                                    ust[0] = ust[1] = bitenAlt[0] = bitenAlt[1] = 0;
                                    oyun[0] = oyun[1] = 0;
                                    d.Biten++;
                                }
                            }
                            else
                            {
                                int bonus = zon[dek] ? 500 : 300;
                                ust[dek] += bonus; Ekle(satir, dek, false, bonus);
                                satir.Not = "Oyun (+" + bonus + ")";
                                bitenAlt[0] += alt[0]; bitenAlt[1] += alt[1];
                                alt[0] = alt[1] = 0;
                            }
                        }
                    }
                }

                // Chicago grubu (4 bord) bitti
                if (tur != PuanTuru.Rubber && grupBord == 4)
                {
                    if (tur == PuanTuru.ChicagoRubber)
                    {
                        for (int k = 0; k < 2; k++)
                            if (alt[k] > 0) { ust[k] += 100; Ekle(satir, k, false, 100); satir.Not = (satir.Not.Length > 0 ? satir.Not + "; " : "") + (k == 0 ? "K-G" : "D-B") + " parsiyel +100"; }
                        tamam[0] += ust[0] + bitenAlt[0] + alt[0]; tamam[1] += ust[1] + bitenAlt[1] + alt[1];
                        ust[0] = ust[1] = bitenAlt[0] = bitenAlt[1] = alt[0] = alt[1] = 0;
                    }
                    else
                    {
                        tamam[0] += cdPuan[0]; tamam[1] += cdPuan[1];
                        cdPuan[0] = cdPuan[1] = 0;
                    }
                    grupBord = 0;
                    d.Biten++;
                }
            }

            bool[] sonraki = ZonAl(tur, oyun, grupBord, siradakiDagitici);
            d.NsZon = sonraki[0]; d.EwZon = sonraki[1];
            d.NsOyun = oyun[0]; d.EwOyun = oyun[1];
            d.NsAlt = alt[0]; d.EwAlt = alt[1];
            if (tur == PuanTuru.ChicagoDuplicate)
            {
                d.NsToplam = tamam[0] + cdPuan[0]; d.EwToplam = tamam[1] + cdPuan[1];
            }
            else
            {
                d.NsToplam = tamam[0] + ust[0] + bitenAlt[0] + alt[0];
                d.EwToplam = tamam[1] + ust[1] + bitenAlt[1] + alt[1];
            }
            return d;
        }

        /// <summary>Zon: Rubber'da oyun kazanmış taraf; Chicago'da grup içi bord sırası ve dağıtıcıya göre.</summary>
        private static bool[] ZonAl(PuanTuru tur, int[] oyun, int grupBord, Player dagitici)
        {
            if (tur == PuanTuru.Rubber) return new[] { oyun[0] > 0, oyun[1] > 0 };
            bool dagiticiNS = dagitici == Player.Kuzey || dagitici == Player.Guney;
            switch (grupBord % 4)
            {
                case 0: return new[] { false, false };
                case 3: return new[] { true, true };
                default: return new[] { dagiticiNS, !dagiticiNS };
            }
        }

        private static void Ekle(PuanSatiri s, int taraf, bool alt, int puan)
        {
            if (puan == 0) return;
            if (alt) { if (taraf == 0) s.NsAlt += puan; else s.EwAlt += puan; }
            else { if (taraf == 0) s.NsUst += puan; else s.EwUst += puan; }
        }
    }

    /// <summary>Oturum boyunca oynanan bordların puan listesi (Rubber / Chicago); oyun türü değişince sıfırlanır.</summary>
    public static class PuanOturumu
    {
        private static readonly List<BordGirdisi> _girdiler = new List<BordGirdisi>();
        private static PuanTuru? _tur;

        public static int BordSayisi { get { return _girdiler.Count; } }

        public static void Temizle() { _girdiler.Clear(); }

        private static void TurKontrol(PuanTuru tur)
        {
            if (_tur != tur) { _girdiler.Clear(); _tur = tur; }
        }

        /// <summary>Bordu kaydeder; aynı bord tekrar oynanırsa (Replay) sonuncu kayıt yenisiyle değişir.</summary>
        public static void Kaydet(PuanTuru tur, BordGirdisi g)
        {
            TurKontrol(tur);
            if (_girdiler.Count > 0 && _girdiler[_girdiler.Count - 1].BordNo == g.BordNo)
                _girdiler[_girdiler.Count - 1] = g;
            else
                _girdiler.Add(g);
        }

        /// <summary><paramref name="bordNo"/> borduna BAŞLARKEN geçerli zon (fiziksel K-G / D-B).</summary>
        public static void Zon(PuanTuru tur, int bordNo, Player dagitici, out bool ns, out bool ew)
        {
            TurKontrol(tur);
            var onceki = new List<BordGirdisi>(_girdiler);
            if (onceki.Count > 0 && onceki[onceki.Count - 1].BordNo == bordNo) onceki.RemoveAt(onceki.Count - 1);
            var d = KlasikSkor.Hesapla(tur, onceki, dagitici);
            ns = d.NsZon; ew = d.EwZon;
        }

        /// <summary>Tüm kayıtlı bordların durumu (sıradaki dağıtıcı zon göstergesi için).</summary>
        public static PuanDurumu Durum(PuanTuru tur, Player siradakiDagitici)
        {
            TurKontrol(tur);
            return KlasikSkor.Hesapla(tur, _girdiler, siradakiDagitici);
        }
    }
}
