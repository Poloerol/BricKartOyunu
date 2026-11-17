﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Class
{
    /// <summary>Tek bir bordun oynama sonucu: kontratın çift dummy lövesi ile motorun gerçekte aldığı löve karşılaştırılır.</summary>
    public sealed class OynamaBordu
    {
        public int BordNo;
        public IhaleSonucu Sonuc;
        public int DdLove;          // çift dummy: deklaranın toplam löve sayısı
        public int GercekLove;      // motorun oynayışında deklaran tarafının aldığı löve
        public int Fark { get { return GercekLove - DdLove; } }   // < 0: deklaran tarafı eksik (atak/deklaran hatası), > 0: savunma hatası
        public int SkorDd, SkorGercek;   // deklaran gözüyle skorlar
        public int Imp;             // deklaran gözüyle IMP farkı (SkorGercek - SkorDd); negatif = deklaran tarafı kaybetti
        public string Oynanis;      // oynanan kartlar (el el, "K:♠A D:..." biçiminde)
        public string AtakKarti;    // açılış atağı
    }

    /// <summary>Oynama karnesi toplamı.</summary>
    public sealed class OynamaOzeti
    {
        public int Bord, Oynanan;               // oynanan = kontratlı bord sayısı
        public int Tam, Eksik, Fazla;           // fark = 0 / < 0 / > 0
        public double ToplamFark, ToplamImp;    // löve farkı ve IMP toplamı (deklaran gözüyle)
        public double ToplamEksikLove, ToplamFazlaLove;
        public double OrtalamaFark { get { return Oynanan == 0 ? 0 : ToplamFark / Oynanan; } }
        public double OrtalamaImp { get { return Oynanan == 0 ? 0 : ToplamImp / Oynanan; } }
    }

    /// <summary>
    /// Oynama Karnesi (Motor Karnesi'nin kart oynama karşılığı): ihale motorunun ulaştığı kontratı, kart oynama motoru
    /// (OyunZekasi + PIMC) dört elde de oynar; deklaran tarafının aldığı löveyi çift dummy lövesiyle karşılaştırır.
    /// Sapma = gerçek löve - DD löve. Negatif: deklaran/atak tarafı DD'nin gerisinde; pozitif: savunma DD'nin gerisinde.
    /// </summary>
    public static class OynamaKarnesi
    {
        private static readonly Player[] Sira = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
        private static readonly string[] KozAd = { "Maça", "Kupa", "Karo", "Sinek" };   // IhaleSonucu.Koz: 0=♠ 1=♥ 2=♦ 3=♣ (4 = NT)

        /// <summary>Tohumdan eller (MotorKarnesi.Ihale ile aynı dağıtım).</summary>
        public static Dictionary<Player, List<Card>> ElleriYukle(int seed)
        {
            var deste = DagitimUretici.Uret(DagitimProfili.Rastgele, null, seed);
            return KonvansiyonPratigi.DestedenEller(deste);
        }

        /// <summary>
        /// KarneBord'un kontratını oynar. b.Tablo (çift dummy) daha önce MotorKarnesi.Degerlendir ile hesaplanmış olmalı.
        /// Pas geçilmiş bordlar için null döner.
        /// </summary>
        public static OynamaBordu Oyna(KarneBord b, Dictionary<Player, List<Card>> ellerOrijinal, Func<Player, OrtaklikAnlasmasi> anlasmaOf = null)
        {
            var s = b.Sonuc;
            if (s == null || s.Pas || b.Tablo == null) return null;

            var eller = new Dictionary<Player, List<Card>>();
            foreach (var kv in ellerOrijinal) eller[kv.Key] = new List<Card>(kv.Value);

            Player dekl = Sira[s.Deklaran];
            Player dummy = IhaleDurumu.PartnerBul(dekl);
            string koz = s.Koz == 4 ? null : KozAd[s.Koz];
            Player lider = OyunKurallari.Sonraki(dekl);

            var gecmis = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>();
            int deklLove = 0;
            var oynanis = new StringBuilder();
            string atakKarti = null;

            for (int el = 0; el < 13; el++)
            {
                var masa = new List<(Player Oyuncu, Card Kart)>();
                Player p = lider;
                string atakRengi = null;
                for (int k = 0; k < 4; k++)
                {
                    var elim = eller[p];
                    var legal = OyunKurallari.LegalKartlar(elim, atakRengi);
                    Card sec = null;
                    if (legal.Count == 1) sec = legal[0];
                    else
                    {
                        var ctx = new OyunBaglami
                        {
                            Ben = p, Deklaran = dekl, Koz = koz,
                            Elim = new List<Card>(elim),
                            Masa = new List<(Player Oyuncu, Card Kart)>(masa),
                            Gecmis = new List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)>(gecmis),
                            ElNo = el,
                            IhaleGecmisi = b.Ihale,
                            IhaleBaslangici = Sira[b.Dagitici].ToString(),
                            AnlasmaOf = anlasmaOf,
                            Seviye = s.Seviye
                        };
                        if (p == dekl || p == dummy) ctx.OrtakEli = new List<Card>(eller[IhaleDurumu.PartnerBul(p)]);
                        else if (el > 0 || masa.Count > 0) ctx.DummyEli = new List<Card>(eller[dummy]);
                        try { string neden; sec = OyunZekasi.KartSec(ctx, out neden); } catch { sec = null; }
                        if (sec == null || !legal.Any(x => x.Suit == sec.Suit && x.Value == sec.Value)) sec = legal[0];
                        else sec = legal.First(x => x.Suit == sec.Suit && x.Value == sec.Value);
                    }
                    elim.Remove(sec);
                    masa.Add((p, sec));
                    if (k == 0) atakRengi = sec.Suit;
                    if (el == 0 && k == 0) atakKarti = KartMetni(sec);
                    p = OyunKurallari.Sonraki(p);
                }
                var kazanan = OyunKurallari.ElKazanani(masa, koz);
                foreach (var m in masa) gecmis.Add((el, m.Oyuncu, m.Kart, m.Oyuncu == kazanan.Oyuncu));
                oynanis.Append(string.Join(" ", masa.Select(m => m.Oyuncu.ToString().Substring(0, 1) + ":" + KartMetni(m.Kart)))).Append(" | ");
                lider = kazanan.Oyuncu;
                if (kazanan.Oyuncu == dekl || kazanan.Oyuncu == dummy) deklLove++;
            }

            int taraf = s.Deklaran & 1;
            bool zonda = taraf == 0 ? b.NsZon : b.EwZon;
            int ddLove = b.Tablo.Al(s.Koz, s.Deklaran);
            // ParHesabi.Skor: lov = deklaranın toplam löve sayısı (çift dummy ile aynı ölçek)
            int skorDd = ParHesabi.Skor(s.Seviye, s.Koz, s.Kontr, zonda, ddLove);
            int skorGercek = ParHesabi.Skor(s.Seviye, s.Koz, s.Kontr, zonda, deklLove);
            int fark = skorGercek - skorDd;
            return new OynamaBordu
            {
                BordNo = b.BordNo, Sonuc = s, DdLove = ddLove, GercekLove = deklLove,
                SkorDd = skorDd, SkorGercek = skorGercek,
                Imp = MacDurumu.Imp(fark),
                Oynanis = oynanis.ToString(), AtakKarti = atakKarti
            };
        }

        private static string KartMetni(Card c)
        {
            string r = c.Value == 14 ? "A" : c.Value == 13 ? "K" : c.Value == 12 ? "Q" : c.Value == 11 ? "J" : c.Value == 10 ? "T" : c.Value.ToString();
            string su = c.Suit == "Maça" ? "♠" : c.Suit == "Kupa" ? "♥" : c.Suit == "Karo" ? "♦" : "♣";
            return su + r;
        }

        public static OynamaOzeti Ozetle(IEnumerable<OynamaBordu> bordlar, int toplamBord)
        {
            var o = new OynamaOzeti { Bord = toplamBord };
            foreach (var b in bordlar)
            {
                if (b == null) continue;
                o.Oynanan++;
                o.ToplamFark += b.Fark; o.ToplamImp += b.Imp;
                if (b.Fark == 0) o.Tam++;
                else if (b.Fark < 0) { o.Eksik++; o.ToplamEksikLove += -b.Fark; }
                else { o.Fazla++; o.ToplamFazlaLove += b.Fark; }
            }
            return o;
        }
    }
}
