using System;
using System.Collections.Generic;
using System.Text;

namespace BricKartOyunu.Class
{
    /// <summary>Par sonucu (iki tarafın da çift dummy'de en iyi oynadığı ihalenin sonucu).</summary>
    public sealed class ParSonuc
    {
        /// <summary>K-G görüşüyle skor.</summary>
        public int NsSkor;
        public bool Pas;
        public int Seviye;          // 1..7
        public int Koz;             // 0=♠ 1=♥ 2=♦ 3=♣ 4=NT
        public int Taraf;           // 0 = K-G, 1 = D-B (kontratı oynayan)
        public int Deklaran;        // koltuk 0=N 1=E 2=S 3=W
        public bool Kontrlu;        // batan kontrat kontrlıdır
        public int Lov;             // deklaranın aldığı löve
        public bool Fedakar;        // batan (fedakârlık) kontrat mı
        public string KontratMetni; // örn. "4♠", "5♦ X"
        public string Metin;        // örn. "N/S 4♠ yapılır +420"
    }

    public static class ParHesabi
    {
        public static readonly string[] KozSembol = { "♠", "♥", "♦", "♣", "NT" };
        public static readonly string[] KoltukHarf = { "N", "E", "S", "W" };
        // ihale sırası (♣ < ♦ < ♥ < ♠ < NT) -> DDS koz numarası
        private static readonly int[] KozOf = { 3, 2, 1, 0, 4 };
        private static readonly int[] SiraOf = { 3, 2, 1, 0, 4 };   // koz -> ihale sırası (simetrik)

        public static string KontratMetni(int seviye, int koz, bool kontrlu)
        {
            return seviye + KozSembol[koz] + (kontrlu ? " X" : "");
        }

        public static string KontratMetni(int seviye, int koz, KontrDurumu k)
        {
            return seviye + KozSembol[koz] + (k == KontrDurumu.SKontr ? " XX" : k == KontrDurumu.Kontr ? " X" : "");
        }

        /// <summary>Deklaran açısından skor (batışta negatif).</summary>
        public static int Skor(int seviye, int koz, bool kontrlu, bool zonda, int lov)
        {
            return Skor(seviye, koz, kontrlu ? KontrDurumu.Kontr : KontrDurumu.Yok, zonda, lov);
        }

        public static int Skor(int seviye, int koz, KontrDurumu kontr, bool zonda, int lov)
        {
            return Skorlama.Hesapla(seviye, KozSembol[koz], kontr, zonda, lov).Toplam;
        }

        /// <summary>Kontrat yapılmıyorsa kontrlı sayılır (rakipler kontra eder), yapılıyorsa kontrasız.</summary>
        public static int OtomatikSkor(int seviye, int koz, bool zonda, int lov, out bool kontrlu)
        {
            kontrlu = lov < seviye + 6;
            return Skor(seviye, koz, kontrlu, zonda, lov);
        }

        public static ParSonuc Hesapla(DDTablo t, int dagitici, bool nsZon, bool ewZon)
        {
            const int N = 35;
            var sc = new int[N, 2];
            var lv = new int[N, 2];
            var dk = new int[N, 2];
            var kx = new bool[N, 2];
            for (int r = 0; r < N; r++)
            {
                int seviye = r / 5 + 1, koz = KozOf[r % 5];
                for (int s = 0; s < 2; s++)
                {
                    int d = t.TarafDeklaran(koz, s);
                    int l = t.Al(koz, d);
                    bool kontrlu;
                    sc[r, s] = OtomatikSkor(seviye, koz, s == 0 ? nsZon : ewZon, l, out kontrlu);
                    lv[r, s] = l; dk[r, s] = d; kx[r, s] = kontrlu;
                }
            }
            // V[r, s]: s tarafı r kontratını verdi, rakip sıradadır
            var V = new int[N, 2];
            var enIyiUst = new int[2] { int.MinValue, int.MinValue };   // r'den büyük kontratlarda tarafların en iyi V'si
            var enIyiUstR = new int[2] { -1, -1 };
            for (int r = N - 1; r >= 0; r--)
            {
                for (int s = 0; s < 2; s++)
                {
                    int o = 1 - s;
                    int v = sc[r, s];
                    if (enIyiUst[o] != int.MinValue) v = Math.Min(v, -enIyiUst[o]);
                    V[r, s] = v;
                }
                for (int s = 0; s < 2; s++)
                    if (V[r, s] > enIyiUst[s]) { enIyiUst[s] = V[r, s]; enIyiUstR[s] = r; }
            }
            // r = 0'dan başlayan en iyiler (tüm kontratlar)
            int[] best = { enIyiUst[0], enIyiUst[1] };
            int[] bestR = { enIyiUstR[0], enIyiUstR[1] };

            int dS = dagitici & 1, oS = 1 - dS;
            int bD = best[dS], bO = best[oS];
            int pasDegeri = -Math.Max(bO, 0);
            int basR = -1, basS = -1;
            if (bD >= pasDegeri && !(bD <= 0 && bO <= 0)) { basR = bestR[dS]; basS = dS; }
            else if (bO > 0) { basR = bestR[oS]; basS = oS; }

            var p = new ParSonuc();
            if (basR < 0)
            {
                p.Pas = true; p.NsSkor = 0; p.Metin = "Par: dört pas (0)";
                return p;
            }

            // zinciri izle: rakip ya geçer ya daha yüksek teklif verir
            int cr = basR, cs = basS;
            for (int guard = 0; guard < 40; guard++)
            {
                int o = 1 - cs;
                int br = -1, bv = int.MinValue;
                for (int r2 = cr + 1; r2 < N; r2++) if (V[r2, o] > bv) { bv = V[r2, o]; br = r2; }
                if (br >= 0 && -bv < sc[cr, cs] && V[cr, cs] == -bv) { cr = br; cs = o; }
                else break;
            }
            p.Seviye = cr / 5 + 1; p.Koz = KozOf[cr % 5]; p.Taraf = cs; p.Deklaran = dk[cr, cs];
            p.Lov = lv[cr, cs]; p.Kontrlu = kx[cr, cs]; p.Fedakar = p.Lov < p.Seviye + 6;
            int declSkor = sc[cr, cs];
            p.NsSkor = cs == 0 ? declSkor : -declSkor;
            p.KontratMetni = KontratMetni(p.Seviye, p.Koz, p.Kontrlu);
            p.Metin = "Par: " + (cs == 0 ? "N/S" : "E/W") + " " + p.KontratMetni + " (" + KoltukHarf[p.Deklaran] + ") " +
                      (p.Fedakar ? (p.Lov - p.Seviye - 6) + " (" + declSkor + ")" : "yapılır +" + declSkor);
            return p;
        }
    }
}
