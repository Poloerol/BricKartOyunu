using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>
    /// 2/1 Game Force (Two Over One) — beşli majör açılışlarına (1♥/1♠) 2 seviyesinde, sıçramasız yeni renk
    /// cevabı OYUNA KADAR zorlayıcıdır (12+ HP). Kaynak: Max Hardy "Two Over One Game Force", Bekir Dural "Two Over One".
    ///
    /// Cevaplayanın ilk teklifi (rakipsiz dizi):
    ///   1♠–2♣/2♦ (4+ minör), 1♠–2♥ (5+ kupa), 1♥–2♣/2♦ (4+ minör). 12 HP'de 5+ kartlık renk gerekir.
    ///   Majörde 4+ destek varsa 2/1 değil Jacoby 2NT / Splinter kullanılır (burada null).
    ///   1♥ açılışında 4+ maça varsa doğal 1♠ söylenir (burada null).
    ///   Pas geçmiş (passed hand) cevaplayan için 2/1 oyun zorlayıcı DEĞİLDİR (burada null).
    ///
    /// Açıcının rebid'i (önceliğe göre):
    ///   1) Cevaplayanın majörüne (2♥) 3+ destek: minimum (≤14 HP) 4♥, fazla değerle 3♥.
    ///   2) Cevaplayanın minörüne 4+ destek (ve altılı majör yok): 3 seviyesinde destek.
    ///   3) Altılı majör: 2 seviyesinde majörü tekrar.
    ///   4) 4+ kartlık ikinci renk (reverse sayılmaz, oyun zorlayıcı olduğu için).
    ///   5) Dengeli + açılmayan renklerde durdurucu: 2NT (15+ HP: 3NT).
    ///   6) Aksi halde beşli majörü tekrar.
    ///
    /// Devam turları: fit (toplam 8+ kart) bulununca majörde 4M, minörde 3NT; fit yoksa 3NT.
    /// Cevaplayan 2. tur: açıcının majöründe 3 kartlı destek varsa 2M (min) / 3M (fazla), açıcı limit ettiyse (2M veya 2NT) 4M;
    /// destek yoksa 6+ renk tekrarı, 5+/4+ ikinci renk, 2NT (12-14, minimum) ya da 3NT (15+).
    /// Hiçbir şart altında oyun seviyesinin altında pas verilmez (Hepsi dizisi rakipsiz olduğu sürece).
    ///
    /// Sınırlar: slam araştırması (cue-bid, Blackwood) bu sınıfta yok; 20+ HP ile oyun seviyesi ihaleleri diğer
    /// konvansiyonlara bırakılır (null). Rakip müdahale ederse tüm 2/1 mantığı devre dışıdır.
    /// </summary>
    internal sealed class IkiBirOyunZorlama : YeniKonvTaban
    {
        public override string Ad { get { return "2/1 Game Force"; } }
        public override string Aciklama { get { return "1♥/1♠–2x (sıçramasız yeni renk) = oyuna kadar zorlayıcı, 12+ HP; açıcı betimler, oyun seviyesinden önce durulmaz"; } }

        private static readonly string[] Renkler = { "Maça", "Kupa", "Karo", "Sinek" };

        protected override string Karar(Dz z)
        {
            if (z.Hepsi.Count == 0) return null;

            // Yalnızca rakipsiz (rakip pas dışında konuşmamış) diziler
            foreach (string h in z.Hepsi)
                if (h.StartsWith("L:") || h.StartsWith("R:")) return null;

            bool benAcici = z.Hepsi[0].StartsWith("B:");
            if (!benAcici && !z.Hepsi[0].StartsWith("O:")) return null;

            string m = YK2.BirRenk(z.Hepsi[0], benAcici ? "B" : "O");
            if (m == null || !YK.Major(m)) return null;

            // Cevaplayan daha önce pas geçtiyse (passed hand) 2/1 oyun zorlayıcı değildir
            Player cevaplayan = benAcici ? z.D.Partner : z.D.AktifOyuncu;
            if (z.D.Gecmis.Any(h => h.PasMi && h.Oyuncu == cevaplayan)) return null;

            // 1) Cevaplayanın ilk teklifi
            if (z.Hepsi.Count == 1)
                return benAcici ? null : IlkCevap(z, m);

            // 2) Oyun zorlayıcı 2/1 cevabı gerçekten verilmiş mi?
            if (!z.Hepsi[1].StartsWith(benAcici ? "O:" : "B:")) return null;
            string t1 = YK.Cagri(z.Hepsi[1]);
            if (YK.Sev(t1) != 2) return null;
            string s = YK.KozOf(t1);
            if (s == null || s == "NT" || s == m) return null;
            if (!(s == "Karo" || s == "Sinek" || (m == "Maça" && s == "Kupa"))) return null;   // 1♥–2♠ sıçramalıdır

            // Sıra bende: ortağın son çağrısı olmalı
            if (!z.Hepsi[z.Hepsi.Count - 1].StartsWith("O:")) return null;

            if (benAcici && z.Hepsi.Count == 2) return AciciRebid(z, m, s);
            return Devam(z, benAcici, m, s);
        }

        // ═══════════════════════════════════════════════════════════════════
        // CEVAPLAYANIN İLK TEKLİFİ
        // ═══════════════════════════════════════════════════════════════════

        private static string IlkCevap(Dz z, string m)
        {
            int uzun = Math.Max(Math.Max(z.Maca, z.Kupa), Math.Max(z.Karo, z.Sinek));
            if (z.Hp < 12 || (z.Hp == 12 && uzun < 5)) return null;
            if (z.Uz(m) >= 4) return null;                 // 4+ destek: Jacoby 2NT / Splinter
            if (m == "Kupa" && z.Maca >= 4) return null;   // 1♥–1♠ doğal ve zorlayıcıdır

            string en = null;
            int enUz = 0;
            // Düşükten yükseğe dolaşılır; eşit uzunlukta yüksek renk kalır
            foreach (string r in new[] { "Sinek", "Karo", "Kupa" })
            {
                if (r == "Kupa" && m != "Maça") continue;  // 1♥–2♥ değil; 1♠–2♥ 5+ ister
                int min = (r == "Kupa") ? 5 : 4;
                int u = z.Uz(r);
                if (u < min) continue;
                if (u >= enUz) { en = r; enUz = u; }
            }
            return en == null ? null : YK.T(2, en);
        }

        // ═══════════════════════════════════════════════════════════════════
        // AÇICININ REBID'İ (1M – 2x – ?)
        // ═══════════════════════════════════════════════════════════════════

        private static string AciciRebid(Dz z, string m, string s)
        {
            int mUz = z.Uz(m);
            int sUz = z.Uz(s);
            int hp = z.Hp;
            bool sMajor = YK.Major(s);
            string ortSon = YK.T(2, s);

            if (sMajor && sUz >= 3)
                return hp <= 14 ? YK.T(4, s) : YK.T(3, s);          // hızlı varış / fazla değerli destek

            if (!sMajor && sUz >= 4 && mUz < 6)
                return YK.T(3, s);                                  // minör desteği

            if (mUz >= 6)
                return YK.T(2, m);                                  // altılı majör

            string y = null;
            int yUz = 0, ySev = 9;
            foreach (string r in Renkler)
            {
                if (r == m || r == s) continue;
                int u = z.Uz(r);
                if (u < 4) continue;
                int sev = YK2.Yuksek(YK.T(2, r), ortSon) ? 2 : 3;
                if (u > yUz || (u == yUz && sev < ySev)) { y = r; yUz = u; ySev = sev; }
            }
            if (y != null) return YK.T(ySev, y);                    // doğal ikinci renk

            if (ElDegerlendirici.DengeliEl(z.El) && Durdurucular(z, m, s))
                return hp >= 15 ? "3NT" : "2NT";

            return mUz >= 5 ? YK.T(2, m) : "2NT";                   // son çare: beşli majörü tekrar
        }

        private static bool Durdurucular(Dz z, string m, string s)
        {
            foreach (string r in Renkler)
            {
                if (r == m || r == s) continue;
                if (!ElDegerlendirici.StopperVar(z.El, r)) return false;
            }
            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // DEVAM TURLARI (iki taraf da oyuna kadar konuşur)
        // ═══════════════════════════════════════════════════════════════════

        private static string Devam(Dz z, bool benAcici, string m, string s)
        {
            string son = YK.Cagri(z.Hepsi[z.Hepsi.Count - 1]);
            if (son == "3NT" || YK.Sev(son) >= 4)
            {
                // Ortak oyun seviyesini söyledi. Slam değeri yoksa (20+ HP) ve başka bir 4+ seviye çağrı yapılmadıysa pas:
                // bilinçli pas (IBilincliPas), böylece Blackwood vb. gereksiz yere araya girmez.
                bool oyunCagrisi = son == "3NT" || (YK.Sev(son) == 4 && YK.Major(YK.KozOf(son)));
                int yuksekCagri = z.Hepsi.Count(h => YK.Sev(YK.Cagri(h)) >= 4 || YK.Cagri(h) == "3NT");
                if (oyunCagrisi && yuksekCagri == 1 && z.Hp < 20) return "Pas";
                return null;
            }
            // Slam değeri (18+ HP): çok turlu GF devamı, Serious 3NT / kontrol çağrısı / Blackwood gibi konvansiyonlara bırakılır
            if (z.Hp >= 18) return null;
            string sonRenk = YK.KozOf(son);
            int hp = z.Hp;
            int sonSev = YK.Sev(son);

            Dictionary<string, int> pm = OrtakUzunluk(z, benAcici, m);

            string fit = null;
            foreach (string r in Renkler)
                if (pm[r] > 0 && z.Uz(r) + pm[r] >= 8) { fit = r; break; }

            // Cevaplayanın 2. turu: açıcının majöründe fit
            if (!benAcici && z.Hepsi.Count == 3 && fit == m)
            {
                if (hp >= 20) return null;
                if (sonRenk == m && sonSev >= 3) return YK.T(4, m);
                bool limit = (sonRenk == m) || son == "2NT";        // açıcı elini limit etti
                if (limit) return hp <= 15 ? YK.T(4, m) : YK.T(3, m);
                string iki = YK.T(2, m);
                if (sonSev == 2 && YK2.Yuksek(iki, son)) return hp <= 14 ? iki : YK.T(3, m);
                return hp <= 14 ? YK.T(4, m) : YK.T(3, m);
            }

            // Genel fit: majörde 4M, minörde 3NT
            if (fit != null)
            {
                if (hp >= 20) return null;                          // slam bölgesi: diğer konvansiyonlara bırakılır
                return YK.Major(fit) ? YK.T(4, fit) : "3NT";
            }

            // Kendi 6+ rengimi (bir ya da iki kez söylemişsem) 3 seviyesinde tekrar
            foreach (string r in Renkler)
            {
                if (z.Uz(r) < 6 || BenimSay(z, r) < 1 || BenimSay(z, r) > 2) continue;
                string aday = YK.T(3, r);
                if (YK2.Yuksek(aday, son)) return aday;
            }

            // Cevaplayan: 5+ renk ve 4+ ikinci renk
            if (!benAcici && z.Hepsi.Count == 3 && z.Uz(s) >= 5)
            {
                foreach (string r in Renkler)
                {
                    if (r == m || r == s || pm[r] > 0 || z.Uz(r) < 4) continue;
                    string iki = YK.T(2, r);
                    if (YK2.Yuksek(iki, son)) return iki;
                    string uc = YK.T(3, r);
                    if (YK2.Yuksek(uc, son)) return uc;
                }
            }

            // Fit yok: minimum cevaplayan 2NT, aksi halde 3NT
            if (!benAcici && z.Hepsi.Count == 3 && hp <= 14 && son != "2NT" && sonSev == 2)
                return "2NT";
            return "3NT";
        }

        /// <summary>Ortağın çağrılarından her renkte gösterdiği asgari uzunluk.</summary>
        private static Dictionary<string, int> OrtakUzunluk(Dz z, bool benAcici, string m)
        {
            var pm = new Dictionary<string, int>();
            var sayi = new Dictionary<string, int>();
            var benim = new HashSet<string>();
            foreach (string r in Renkler) { pm[r] = 0; sayi[r] = 0; }

            foreach (string h in z.Hepsi)
            {
                string r = YK.KozOf(YK.Cagri(h));
                if (r == null || r == "NT") continue;
                if (h.StartsWith("B:")) { benim.Add(r); continue; }

                sayi[r]++;
                int min;
                if (benim.Contains(r)) min = YK.Major(r) ? 3 : 4;                  // benim rengime destek
                else if (sayi[r] >= 2) min = benAcici ? 6 : 5;                     // aynı rengi tekrar: cevaplayan 6+, açıcı 5+ (Hardy)
                else if (r == m && !benAcici) min = 5;                             // açıcının majörü
                else if (r == "Kupa" && m == "Maça" && benAcici) min = 5;          // 1♠–2♥
                else min = 4;
                if (min > pm[r]) pm[r] = min;
            }
            return pm;
        }

        private static int BenimSay(Dz z, string renk)
        {
            return z.Hepsi.Count(h => h.StartsWith("B:") && YK.KozOf(YK.Cagri(h)) == renk);
        }
    }
}
