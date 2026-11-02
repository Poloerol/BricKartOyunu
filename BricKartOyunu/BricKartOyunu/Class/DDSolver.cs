using System;
using System.Collections.Generic;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Çift dummy çözücü: dört elin de açık olduğu pozisyonda, en iyi oyunla "çift sayılı koltukların"
    /// (0 ve 2) alacağı löve sayısını hesaplar. Alfa-beta + yer değiştirme tablosu; eşdeğer kartlar tek hamle sayılır.
    /// Kart kodu: renk * 16 + sıra (sıra 0 = iki ... 12 = As). El maskesi: aynı kod bit konumu.
    /// Koltuk sırası oyun sırasıdır: koltuk (k+1)&amp;3, k'dan sonra oynar.
    /// </summary>
    public sealed class DDSolver
    {
        private struct TTKey : IEquatable<TTKey>
        {
            public ulong A, B, C, D;
            public bool Equals(TTKey o) { return A == o.A && B == o.B && C == o.C && D == o.D; }
            public override bool Equals(object o) { return o is TTKey && Equals((TTKey)o); }
            public override int GetHashCode()
            {
                ulong x = A * 0x9E3779B97F4A7C15UL;
                x ^= (B + 0x7F4A7C15UL) * 0xC2B2AE3D27D4EB4FUL; x = (x << 27) | (x >> 37);
                x ^= C * 0x165667B19E3779F9UL; x = (x << 31) | (x >> 33);
                x ^= D * 0x27D4EB2F165667C5UL;
                return (int)(x ^ (x >> 32));
            }
        }

        private struct TTVal { public sbyte Lo, Hi; }

        private readonly int _koz;                 // -1 = sanzatu, 0..3 = koz rengi
        private readonly ulong[] _h = new ulong[4];
        private int _leader, _k;
        private readonly int[] _tc = new int[4];   // elde oynanmış kartlar (sırayla)
        private Dictionary<TTKey, TTVal> _tt = new Dictionary<TTKey, TTVal>();
        private readonly int[][] _mvCard = new int[64][];
        private readonly ulong[][] _mvGrp = new ulong[64][];
        public long Dugum;
        public int TabloSiniri = 1500000;
        public bool TabloAcik = true;
        public bool GrupAcik = true;

        public DDSolver(int koz)
        {
            _koz = koz;
            for (int i = 0; i < 64; i++) { _mvCard[i] = new int[16]; _mvGrp[i] = new ulong[16]; }
        }

        /// <summary>Yer değiştirme tablosunu boşaltır (yeni örnek / yeni pozisyon için).</summary>
        private static int Pop(ulong x)
        {
            x = x - ((x >> 1) & 0x5555555555555555UL);
            x = (x & 0x3333333333333333UL) + ((x >> 2) & 0x3333333333333333UL);
            x = (x + (x >> 4)) & 0x0F0F0F0F0F0F0F0FUL;
            return (int)((x * 0x0101010101010101UL) >> 56);
        }

        // ── Genel arayüz ───────────────────────────────────────────────────
        /// <summary>
        /// Pozisyondaki tüm yasal kartların değerini döndürür (çift sayılı koltukların bu karttan sonra, en iyi oyunla
        /// alacağı toplam löve; devam eden elin kendisi dahil).
        /// elleri: 4 koltuğun KALAN kartları (elde oynanmış kartlar hariç), lider: eli açan koltuk,
        /// el: elde oynanmış kartlar (kod), sırayla liderden başlayarak.
        /// </summary>
        public void Degerler(ulong[] eller, int lider, int[] el, int elSayisi, out int[] kartlar, out int[] degerler)
        {
            Degerler(eller, lider, el, elSayisi, -1, out kartlar, out degerler);
        }

        /// <summary>
        /// hedef >= 0 ise yalnızca "çift koltuklar en az hedef löve alır mı" sorusu çözülür (çok daha hızlı):
        /// dönen değer hedef (evet) ya da hedef-1 (hayır) olur; ortalama löve için kullanılamaz.
        /// </summary>
        public void Degerler(ulong[] eller, int lider, int[] el, int elSayisi, int hedef, out int[] kartlar, out int[] degerler)
        {
            for (int i = 0; i < 4; i++) _h[i] = eller[i];
            _leader = lider; _k = elSayisi;
            for (int i = 0; i < elSayisi; i++) _tc[i] = el[i];
            if (_tt.Count > TabloSiniri) _tt.Clear();

            int mover = (_leader + _k) & 3;
            int cnt = UretHamle(0, mover);
            var kl = new List<int>(); var dg = new List<int>();
            for (int i = 0; i < cnt; i++)
            {
                int card = _mvCard[0][i];
                ulong grp = _mvGrp[0][i];
                int v = hedef < 0 ? Oyna(card, mover, -1, 99, 1) : Oyna(card, mover, hedef - 1, hedef, 1);
                if (hedef >= 0) v = v >= hedef ? hedef : hedef - 1;
                for (int c = 0; c < 64; c++)
                    if (((grp >> c) & 1UL) != 0) { kl.Add(c); dg.Add(v); }
            }
            kartlar = kl.ToArray(); degerler = dg.ToArray();
        }

        /// <summary>Pozisyonun değeri (çift sayılı koltukların alacağı löve).</summary>
        public int Coz(ulong[] eller, int lider, int[] el, int elSayisi)
        {
            int[] k, d;
            Degerler(eller, lider, el, elSayisi, out k, out d);
            bool max = (((lider + elSayisi) & 3) & 1) == 0;
            int best = max ? -1 : 99;
            for (int i = 0; i < d.Length; i++) best = max ? Math.Max(best, d[i]) : Math.Min(best, d[i]);
            return best;
        }

        // ── Arama ──────────────────────────────────────────────────────────
        private int Oyna(int card, int mover, int alpha, int beta, int depth)
        {
            int sLeader = _leader, sK = _k;
            int t0 = _tc[0], t1 = _tc[1], t2 = _tc[2], t3 = _tc[3];
            ulong bit = 1UL << card;
            _h[mover] &= ~bit;
            _tc[_k] = card;
            int v;
            if (_k == 3)
            {
                int w = Kazanan();
                int gain = (w & 1) == 0 ? 1 : 0;
                _leader = w; _k = 0;
                v = gain + Rec(alpha - gain, beta - gain, depth);
            }
            else
            {
                _k++;
                v = Rec(alpha, beta, depth);
            }
            _h[mover] |= bit;
            _leader = sLeader; _k = sK;
            _tc[0] = t0; _tc[1] = t1; _tc[2] = t2; _tc[3] = t3;   // alt arama el kartlarını ezmiş olabilir
            return v;
        }

        private int Kazanan()
        {
            int best = 0;
            int bs = _tc[0] >> 4, br = _tc[0] & 15;
            for (int i = 1; i < 4; i++)
            {
                int s = _tc[i] >> 4, r = _tc[i] & 15;
                if (s == bs) { if (r > br) { best = i; br = r; } }
                else if (s == _koz && bs != _koz) { best = i; bs = s; br = r; }
            }
            return (_leader + best) & 3;
        }

        private int Rec(int alpha, int beta, int depth)
        {
            Dugum++;
            int mover = (_leader + _k) & 3;
            int tricksLeft = Pop(_h[mover]);
            if (tricksLeft == 0) return 0;

            TTKey key = default(TTKey);
            bool trickStart = _k == 0 && TabloAcik;
            if (trickStart)
            {
                key = AnahtarUret();
                TTVal tv;
                if (_tt.TryGetValue(key, out tv))
                {
                    if (tv.Lo >= beta) return tv.Lo;
                    if (tv.Hi <= alpha) return tv.Hi;
                    if (tv.Lo == tv.Hi) return tv.Lo;
                    if (tv.Lo > alpha) alpha = tv.Lo;
                    if (tv.Hi < beta) beta = tv.Hi;
                }
                // Basit sınırlar: 0..tricksLeft
                if (alpha < 0) { /* alt sınır 0 */ }
                if (tricksLeft <= alpha) return tricksLeft;       // alacağımızdan fazlası yok → üst sınır
                if (beta <= 0) return 0;

                if (_koz < 0)                                     // sanzatu: liderin hemen çekebileceği sağlam löveler
                {
                    int qt = HizliLove(mover);
                    if (qt > 0)
                    {
                        if ((mover & 1) == 0) { if (qt >= beta) return qt; }
                        else { int ub = tricksLeft - qt; if (ub <= alpha) return ub; }
                    }
                }
            }

            int an = alpha, bn = beta;
            int cnt = UretHamle(depth, mover);
            bool max = (mover & 1) == 0;
            int best = max ? -1 : 99;
            for (int i = 0; i < cnt; i++)
            {
                int v = Oyna(_mvCard[depth][i], mover, alpha, beta, depth + 1);
                if (max)
                {
                    if (v > best) best = v;
                    if (best > alpha) alpha = best;
                }
                else
                {
                    if (v < best) best = v;
                    if (best < beta) beta = best;
                }
                if (alpha >= beta) break;
            }

            if (trickStart)
            {
                TTVal tv;
                if (!_tt.TryGetValue(key, out tv)) { tv.Lo = 0; tv.Hi = (sbyte)tricksLeft; }
                if (best <= an) { if (best < tv.Hi) tv.Hi = (sbyte)best; }
                else if (best >= bn) { if (best > tv.Lo) tv.Lo = (sbyte)best; }
                else { tv.Lo = (sbyte)best; tv.Hi = (sbyte)best; }
                _tt[key] = tv;
            }
            return best;
        }

        /// <summary>Lider, kendi elinden üst üste çekebileceği en yüksek kartlar (hiçbir rakip karşı koyamaz).</summary>
        private int HizliLove(int lider)
        {
            ulong all = _h[0] | _h[1] | _h[2] | _h[3];
            ulong el = _h[lider];
            int toplam = 0;
            for (int s = 0; s < 4; s++)
            {
                int sh = s * 16;
                int u = (int)((all >> sh) & 0x1FFF);
                int m = (int)((el >> sh) & 0x1FFF);
                for (int b = 12; b >= 0; b--)
                {
                    if (((u >> b) & 1) == 0) continue;
                    if (((m >> b) & 1) != 0) toplam++; else break;
                }
            }
            return toplam;
        }

        private TTKey AnahtarUret()
        {
            ulong n0 = 0, n1 = 0, n2 = 0, n3 = 0;
            ulong all = _h[0] | _h[1] | _h[2] | _h[3];
            for (int s = 0; s < 4; s++)
            {
                int sh = s * 16;
                int u = (int)((all >> sh) & 0x1FFF);
                int j = 0;
                for (int b = 0; b < 13; b++)
                {
                    if (((u >> b) & 1) == 0) continue;
                    ulong m = 1UL << (sh + b);
                    ulong nb = 1UL << (sh + j);
                    if ((_h[0] & m) != 0) n0 |= nb;
                    else if ((_h[1] & m) != 0) n1 |= nb;
                    else if ((_h[2] & m) != 0) n2 |= nb;
                    else n3 |= nb;
                    j++;
                }
            }
            n0 |= (ulong)_leader << 61;      // lider bilgisi son bloğun boş bitlerinde
            return new TTKey { A = n0, B = n1, C = n2, D = n3 };
        }

        // ── Hamle üretimi ──────────────────────────────────────────────────
        private int UretHamle(int depth, int mover)
        {
            int[] cards = _mvCard[depth];
            ulong[] grps = _mvGrp[depth];
            int cnt = 0;
            ulong hand = _h[mover];
            ulong all = _h[0] | _h[1] | _h[2] | _h[3];
            ulong trickBits = 0;
            for (int i = 0; i < _k; i++) trickBits |= 1UL << _tc[i];
            ulong allPlus = all | trickBits;

            int ls = _k > 0 ? (_tc[0] >> 4) : -1;
            bool follow = ls >= 0 && (((hand >> (ls * 16)) & 0x1FFF) != 0);

            // Şu an kazanan kart (sıra, renk) — sıralama için
            int curBestS = -1, curBestR = -1;
            if (_k > 0)
            {
                curBestS = _tc[0] >> 4; curBestR = _tc[0] & 15;
                for (int i = 1; i < _k; i++)
                {
                    int s = _tc[i] >> 4, r = _tc[i] & 15;
                    if (s == curBestS) { if (r > curBestR) curBestR = r; }
                    else if (s == _koz && curBestS != _koz) { curBestS = s; curBestR = r; }
                }
            }

            int[] score = new int[16];
            for (int s = 0; s < 4; s++)
            {
                if (follow && s != ls) continue;
                int sh = s * 16;
                int hs = (int)((hand >> sh) & 0x1FFF);
                if (hs == 0) continue;
                int us = (int)((allPlus >> sh) & 0x1FFF);
                int topUnion = 31 - Lzcnt(us);
                int suitLen = PopI(hs);

                int groupLow = -1; ulong groupMask = 0; bool prevMine = false;
                for (int b = 0; b < 13; b++)
                {
                    if (((us >> b) & 1) == 0) continue;
                    bool mine = ((hs >> b) & 1) != 0;
                    if (!GrupAcik && mine) { prevMine = false; Ekle(cards, grps, score, ref cnt, sh + b, 1UL << (sh + b), s, b, hs, topUnion, suitLen, curBestS, curBestR); continue; }
                    if (mine)
                    {
                        if (!prevMine) { groupLow = b; groupMask = 0; }
                        groupMask |= 1UL << (sh + b);
                        prevMine = true;
                    }
                    else
                    {
                        if (prevMine) { Ekle(cards, grps, score, ref cnt, sh + groupLow, groupMask, s, groupLow, hs, topUnion, suitLen, curBestS, curBestR); }
                        prevMine = false;
                    }
                }
                if (prevMine) { Ekle(cards, grps, score, ref cnt, sh + groupLow, groupMask, s, groupLow, hs, topUnion, suitLen, curBestS, curBestR); }
            }
            // Sıralama (azalan skor)
            for (int i = 1; i < cnt; i++)
            {
                int c = cards[i]; ulong g = grps[i]; int sc = score[i]; int j = i - 1;
                while (j >= 0 && score[j] < sc) { cards[j + 1] = cards[j]; grps[j + 1] = grps[j]; score[j + 1] = score[j]; j--; }
                cards[j + 1] = c; grps[j + 1] = g; score[j + 1] = sc;
            }
            return cnt;
        }

        private void Ekle(int[] cards, ulong[] grps, int[] score, ref int cnt, int code, ulong mask, int suit, int rank,
                          int hs, int topUnion, int suitLen, int curBestS, int curBestR)
        {
            // grubun en yükseği (kazanma karşılaştırması için)
            int topInGroup = rank;
            for (int b = rank; b < 13; b++) if (((mask >> (suit * 16 + b)) & 1UL) != 0) topInGroup = b;
            int sc;
            if (_k == 0)
            {
                bool winner = topInGroup == topUnion;
                sc = winner ? 1000 + suitLen * 10 - rank : -rank;
            }
            else
            {
                bool wins = (suit == curBestS && topInGroup > curBestR) || (suit == _koz && curBestS != _koz);
                if (suit == _koz && curBestS == _koz && topInGroup <= curBestR) wins = false;
                sc = wins ? 500 - rank : -rank;
            }
            cards[cnt] = code; grps[cnt] = mask; score[cnt] = sc; cnt++;
        }

        private static int PopI(int x) { int c = 0; while (x != 0) { x &= x - 1; c++; } return c; }
        private static int Lzcnt(int x) { int n = 32; while (x != 0) { x >>= 1; n--; } return n; }
    }
}
