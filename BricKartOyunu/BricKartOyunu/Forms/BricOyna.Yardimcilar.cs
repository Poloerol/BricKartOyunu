using BricKartOyunu.Class;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;


namespace BricKartOyunu
{

    /// <summary>
    /// "Otomatik oynatma" onay penceresi: başlık satırı, açıklama satırı ve Evet / Hayır butonları.
    /// Evet → DialogResult.Yes, Hayır → DialogResult.No, X ile kapatma → DialogResult.Cancel.
    /// </summary>
    internal sealed class OtoOynaOnayForm : Form
    {
        public Button BtnEvet { get; private set; }
        public Button BtnHayir { get; private set; }

        public OtoOynaOnayForm()
        {
            Text = "Otomatik oynatma";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(340, 130);

            var lbl1 = new Label
            {
                Text = "Otomatik oynatma",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 14),
                Size = new Size(320, 28)
            };
            var lbl2 = new Label
            {
                Text = "(Oynanan eller arasında duraklama yok)",
                Font = new Font("Segoe UI", 9.5F),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 46),
                Size = new Size(320, 24)
            };

            BtnEvet = new Button
            {
                Name = "BtnEvet",
                Text = "Evet",
                DialogResult = DialogResult.Yes,
                Location = new Point(70, 84),
                Size = new Size(90, 30)
            };
            BtnHayir = new Button
            {
                Name = "BtnHayir",
                Text = "Hayır",
                DialogResult = DialogResult.No,
                Location = new Point(180, 84),
                Size = new Size(90, 30)
            };

            Controls.AddRange(new Control[] { lbl1, lbl2, BtnEvet, BtnHayir });
            AcceptButton = BtnEvet;
        }
    }


    /// <summary>
    /// Claim onay penceresi: "n el daha alacağım. Kabul ediyor musun?" + Evet / Hayır.
    /// Evet → DialogResult.Yes, Hayır → DialogResult.No, X ile kapatma → DialogResult.Cancel.
    /// </summary>
    internal sealed class ClaimOnayForm : Form
    {
        public Button BtnEvet { get; private set; }
        public Button BtnHayir { get; private set; }

        public ClaimOnayForm(int n)
        {
            Text = "Claim";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(360, 120);

            var lbl = new Label
            {
                Text = n + " el daha alacağım. Kabul ediyor musun?",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 14),
                Size = new Size(340, 36)
            };

            BtnEvet = new Button
            {
                Name = "BtnEvet",
                Text = "Evet",
                DialogResult = DialogResult.Yes,
                Location = new Point(80, 72),
                Size = new Size(90, 30)
            };
            BtnHayir = new Button
            {
                Name = "BtnHayir",
                Text = "Hayır",
                DialogResult = DialogResult.No,
                Location = new Point(190, 72),
                Size = new Size(90, 30)
            };

            Controls.AddRange(new Control[] { lbl, BtnEvet, BtnHayir });
            AcceptButton = BtnEvet;
        }
    }

    /// <summary>
    /// Çift açık (double dummy) el çözücü: dört elin de açık olduğu varsayımıyla, iki tarafın
    /// da en iyi oynaması durumunda "maksimum taraf"ın kalan elleri kaç tane alacağını hesaplar.
    /// Koltuklar saat yönünde: 0=Batı, 1=Kuzey, 2=Doğu, 3=Güney (taraf = koltuk &amp; 1).
    /// Kart numarası = renk*13 + (değer-2); renk: 0=Maça, 1=Kupa, 2=Karo, 3=Sinek.
    /// </summary>
    internal sealed class ClaimCozucu
    {
        private struct Anahtar : IEquatable<Anahtar>
        {
            public ulong A, B, C, D;
            public int Lider;
            public bool Equals(Anahtar o) { return A == o.A && B == o.B && C == o.C && D == o.D && Lider == o.Lider; }
            public override bool Equals(object o) { return o is Anahtar && Equals((Anahtar)o); }
            public override int GetHashCode()
            {
                unchecked
                {
                    ulong h = A * 0x9E3779B97F4A7C15UL ^ (B + 0x7F4A7C15UL) * 0xC2B2AE3D27D4EB4FUL
                              ^ C * 0x165667B19E3779F9UL ^ D * 0x85EBCA77C2B2AE63UL ^ (ulong)Lider;
                    return (int)(h ^ (h >> 32));
                }
            }
        }

        private readonly ulong[] _el = new ulong[4];
        private readonly int[] _masa = new int[4];
        private int _masaSayisi, _lider, _ilkRenk;
        private readonly int _koz;          // -1 = kozsuz
        private readonly int _maxTaraf;     // 0: Batı/Doğu, 1: Kuzey/Güney
        private readonly Dictionary<Anahtar, int> _tt = new Dictionary<Anahtar, int>();
        private long _dugum;
        private readonly long _limit;

        /// <summary>Düğüm sınırı aşıldıysa sonuç güvenilmezdir.</summary>
        public bool Asildi { get; private set; }

        public ClaimCozucu(ulong[] eller, int lider, int koz, int maxTaraf, long dugumSiniri)
        {
            for (int i = 0; i < 4; i++) _el[i] = eller[i];
            _lider = lider; _koz = koz; _maxTaraf = maxTaraf; _limit = dugumSiniri;
        }

        private static int Say(ulong x)
        {
            int n = 0;
            while (x != 0) { x &= x - 1; n++; }
            return n;
        }

        /// <summary>El başında (masa boş) maksimum tarafın alacağı el sayısı.</summary>
        public int Coz()
        {
            int kalan = Say(_el[_lider]);
            return Ara(-1, kalan + 1);
        }

        private bool Yener(int c, int w)
        {
            int cs = c / 13, ws = w / 13;
            if (cs == ws) return c > w;
            return cs == _koz;
        }

        private int Ara(int alfa, int beta)
        {
            if (++_dugum > _limit) { Asildi = true; return 0; }

            int koltuk = (_lider + _masaSayisi) & 3;
            bool sinir = _masaSayisi == 0;
            Anahtar k = default(Anahtar);
            int lo = 0, hi = 0;
            int alfa0 = alfa, beta0 = beta;

            if (sinir)
            {
                if ((_el[0] | _el[1] | _el[2] | _el[3]) == 0) return 0;
                k = new Anahtar { A = _el[0], B = _el[1], C = _el[2], D = _el[3], Lider = _lider };
                int kalan = Say(_el[koltuk]);
                lo = 0; hi = kalan;
                int v;
                if (_tt.TryGetValue(k, out v))
                {
                    lo = v & 0xFF; hi = (v >> 8) & 0xFF;
                    if (lo >= beta) return lo;
                    if (hi <= alfa) return hi;
                    if (lo == hi) return lo;
                    if (lo > alfa) alfa = lo;
                    if (hi < beta) beta = hi;
                }
                alfa0 = alfa; beta0 = beta;
            }

            // Yasal kartlar
            ulong el = _el[koltuk];
            ulong yasal = el;
            if (_masaSayisi > 0)
            {
                ulong renkMaske = 0x1FFFUL << (_ilkRenk * 13);
                if ((el & renkMaske) != 0) yasal = el & renkMaske;
            }

            // Masadaki kartlar da "arada kart var mı" kontrolüne katılır
            ulong hepsi = _el[0] | _el[1] | _el[2] | _el[3];
            for (int i = 0; i < _masaSayisi; i++) hepsi |= 1UL << _masa[i];

            // Denk kartları ele: aralarında başka kart olmayan bitişik kartlardan yalnızca en yükseği denenir
            int[] hamle = new int[13];
            int hs = 0;
            for (int renk = 0; renk < 4; renk++)
            {
                int m = (int)((yasal >> (renk * 13)) & 0x1FFF);
                if (m == 0) continue;
                int hm = (int)((hepsi >> (renk * 13)) & 0x1FFF);
                int onceki = -1;
                for (int b = 12; b >= 0; b--)
                {
                    if ((m & (1 << b)) == 0) continue;
                    if (onceki >= 0)
                    {
                        int arasi = ((1 << onceki) - 1) & ~((1 << (b + 1)) - 1);   // b+1 .. onceki-1
                        if ((hm & arasi) == 0) { onceki = b; continue; }          // öncekiyle denk
                    }
                    hamle[hs++] = renk * 13 + b;
                    onceki = b;
                }
            }

            // Sıralama: ön tarafta yüksek (açarken), takip ederken kazananın en düşüğü sonra düşükler
            if (_masaSayisi == 0)
            {
                Array.Sort(hamle, 0, hs, DescComparer.Ornek);
            }
            else
            {
                int kazanan = _masa[0];
                for (int i = 1; i < _masaSayisi; i++) if (Yener(_masa[i], kazanan)) kazanan = _masa[i];
                int[] skor = new int[hs];
                for (int i = 0; i < hs; i++) skor[i] = (Yener(hamle[i], kazanan) ? 0 : 100) + (hamle[i] % 13);
                Array.Sort(skor, hamle, 0, hs);
                // skor dizisi tam uzunlukta; Array.Sort(keys, items, index, length) hs kadarını sıralar
            }

            bool max = (koltuk & 1) == _maxTaraf;
            int best = max ? int.MinValue : int.MaxValue;
            int a = alfa, bb = beta;

            for (int hi2 = 0; hi2 < hs; hi2++)
            {
                int c = hamle[hi2];
                ulong bit = 1UL << c;
                _el[koltuk] ^= bit;
                int eskiSayi = _masaSayisi, eskiIlk = _ilkRenk, eskiLider = _lider;
                int m0 = _masa[0], m1 = _masa[1], m2 = _masa[2], m3 = _masa[3];
                _masa[_masaSayisi++] = c;
                if (_masaSayisi == 1) _ilkRenk = c / 13;

                int val;
                if (_masaSayisi == 4)
                {
                    int bi = 0;
                    for (int i = 1; i < 4; i++) if (Yener(_masa[i], _masa[bi])) bi = i;
                    int w = (_lider + bi) & 3;
                    int puan = ((w & 1) == _maxTaraf) ? 1 : 0;
                    _lider = w; _masaSayisi = 0;
                    val = puan + Ara(a - puan, bb - puan);
                }
                else
                {
                    val = Ara(a, bb);
                }

                _lider = eskiLider; _masaSayisi = eskiSayi; _ilkRenk = eskiIlk;
                _masa[0] = m0; _masa[1] = m1; _masa[2] = m2; _masa[3] = m3;
                _el[koltuk] ^= bit;

                if (Asildi) return 0;

                if (max) { if (val > best) best = val; if (best > a) a = best; }
                else { if (val < best) best = val; if (best < bb) bb = best; }
                if (a >= bb) break;
            }

            if (sinir)
            {
                int nlo = lo, nhi = hi;
                if (best <= alfa0) nhi = Math.Min(nhi, best);
                else if (best >= beta0) nlo = Math.Max(nlo, best);
                else { nlo = best; nhi = best; }
                if (_tt.Count > 4000000) _tt.Clear();
                _tt[k] = nlo | (nhi << 8);
            }
            return best;
        }

        private sealed class DescComparer : System.Collections.Generic.IComparer<int>
        {
            public static readonly DescComparer Ornek = new DescComparer();
            public int Compare(int x, int y) { return (y % 13).CompareTo(x % 13); }
        }
    }
}
