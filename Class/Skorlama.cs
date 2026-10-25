using System;
using System.Collections.Generic;
using System.Text;

namespace BricKartOyunu.Class
{
    /// <summary>Kontrat kontr durumu.</summary>
    public enum KontrDurumu
    {
        Yok = 0,
        Kontr = 1,
        SKontr = 2
    }

    /// <summary>Bir bordun duplicate briç skoru (deklaran tarafı açısından).</summary>
    public class SkorSonucu
    {
        public bool PasGecildi;
        public int Seviye;
        public string Koz;            // "♣", "♦", "♥", "♠", "NT"
        public KontrDurumu Kontr;
        public bool Zonda;
        public int GerekenLove;       // 6 + seviye
        public int AlinanLove;
        public bool Yapildi;

        public int Anlasma;           // kontrat löveleri
        public int OyunBonusu;        // oyun (300/500) veya parsiyel (50)
        public int SlamBonusu;        // küçük/büyük slam
        public int KontrIkrami;       // kontrlı/sürkontrlı kontrat yapıldı
        public int FazlaLove;         // fazla löve (overtrick)
        public int Ceza;              // batış (pozitif sayı; toplamda negatiftir)
        public int Toplam;            // deklaran açısından (batışta negatif)

        public string KontrEki
        {
            get { return Kontr == KontrDurumu.SKontr ? "XX" : Kontr == KontrDurumu.Kontr ? "X" : ""; }
        }

        public string KontratMetni
        {
            get { return PasGecildi ? "Pas" : Seviye + Koz + KontrEki; }
        }
    }

    /// <summary>Duplicate briç skor hesabı.</summary>
    public static class Skorlama
    {
        public static SkorSonucu Hesapla(int seviye, string koz, KontrDurumu kontr, bool zonda, int alinanLove)
        {
            var s = new SkorSonucu
            {
                Seviye = seviye,
                Koz = koz,
                Kontr = kontr,
                Zonda = zonda,
                GerekenLove = 6 + seviye,
                AlinanLove = alinanLove
            };
            s.Yapildi = alinanLove >= s.GerekenLove;

            bool minor = koz == "♣" || koz == "♦";
            bool nt = koz == "NT";
            int carpan = kontr == KontrDurumu.SKontr ? 4 : kontr == KontrDurumu.Kontr ? 2 : 1;

            if (s.Yapildi)
            {
                // Kontrat löveleri
                int lovePuani = nt ? 40 + 30 * (seviye - 1) : (minor ? 20 : 30) * seviye;
                s.Anlasma = lovePuani * carpan;

                // Oyun / parsiyel bonusu
                s.OyunBonusu = s.Anlasma >= 100 ? (zonda ? 500 : 300) : 50;

                // Slam bonusu
                if (seviye == 6) s.SlamBonusu = zonda ? 750 : 500;
                else if (seviye == 7) s.SlamBonusu = zonda ? 1500 : 1000;

                // Kontr ikramı
                if (kontr == KontrDurumu.Kontr) s.KontrIkrami = 50;
                else if (kontr == KontrDurumu.SKontr) s.KontrIkrami = 100;

                // Fazla löveler
                int fazla = alinanLove - s.GerekenLove;
                if (kontr == KontrDurumu.Yok)
                    s.FazlaLove = fazla * (minor ? 20 : 30);
                else
                    s.FazlaLove = fazla * (zonda ? 200 : 100) * (kontr == KontrDurumu.SKontr ? 2 : 1);

                s.Toplam = s.Anlasma + s.OyunBonusu + s.SlamBonusu + s.KontrIkrami + s.FazlaLove;
            }
            else
            {
                int bati = s.GerekenLove - alinanLove;
                int ceza = 0;
                if (kontr == KontrDurumu.Yok)
                {
                    ceza = bati * (zonda ? 100 : 50);
                }
                else
                {
                    for (int i = 1; i <= bati; i++)
                    {
                        if (zonda) ceza += i == 1 ? 200 : 300;
                        else ceza += i == 1 ? 100 : (i <= 3 ? 200 : 300);
                    }
                    if (kontr == KontrDurumu.SKontr) ceza *= 2;
                }
                s.Ceza = ceza;
                s.Toplam = -ceza;
            }
            return s;
        }

        /// <summary>Skorun Türkçe açıklaması (deklaran tarafı için).</summary>
        public static string Aciklama(SkorSonucu s)
        {
            if (s == null || s.PasGecildi) return "Dört pas — bord oynanmadı, skor 0.";

            var sb = new StringBuilder();
            sb.AppendLine("Kontrat: " + s.KontratMetni + (s.Zonda ? "  (zonda)" : "  (zonsuz)"));
            sb.AppendLine("Gereken löve: " + s.GerekenLove + "   Alınan löve: " + s.AlinanLove);
            if (s.Yapildi)
            {
                int fazla = s.AlinanLove - s.GerekenLove;
                sb.AppendLine(fazla == 0 ? "Sonuç: tam yapıldı" : "Sonuç: " + fazla + " fazla löve ile yapıldı");
                sb.AppendLine();
                sb.AppendLine("Kontrat löveleri : " + s.Anlasma);
                sb.AppendLine((s.Anlasma >= 100 ? "Oyun bonusu      : " : "Parsiyel bonusu  : ") + s.OyunBonusu);
                if (s.SlamBonusu > 0) sb.AppendLine("Slam bonusu      : " + s.SlamBonusu);
                if (s.KontrIkrami > 0) sb.AppendLine("Kontr ikramı     : " + s.KontrIkrami);
                if (s.FazlaLove > 0) sb.AppendLine("Fazla löveler    : " + s.FazlaLove);
            }
            else
            {
                sb.AppendLine("Sonuç: " + (s.GerekenLove - s.AlinanLove) + " batık");
                sb.AppendLine();
                sb.AppendLine("Batış cezası     : " + s.Ceza);
            }
            sb.AppendLine();
            sb.AppendLine("Toplam (deklaran tarafı): " + (s.Toplam > 0 ? "+" : "") + s.Toplam);
            return sb.ToString();
        }
    }
}
