using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>İhale dizisinin "ben / sol rakip / ortak / sağ rakip" gözüyle özeti (Pas'lar hariç).</summary>
    internal sealed class Dz
    {
        public IhaleDurumu D;
        public List<Card> El;
        public int Hp;
        /// <summary>Pas olmayan çağrılar, sırayla: "B:1♠" (ben), "L:" (sol rakip), "O:" (ortak), "R:" (sağ rakip).</summary>
        public List<string> Hepsi = new List<string>();

        private static readonly Player[] Cw = { Player.Guney, Player.Bati, Player.Kuzey, Player.Dogu };   // saat yönü

        public static Dz Al(IhaleDurumu d)
        {
            var z = new Dz { D = d, El = d.AktifOyuncuEli };
            z.Hp = ElDegerlendirici.HCP(z.El);
            int ben = Array.IndexOf(Cw, d.AktifOyuncu);
            foreach (var h in d.Gecmis)
            {
                if (h.PasMi) continue;
                int rel = (Array.IndexOf(Cw, h.Oyuncu) - ben + 4) % 4;
                z.Hepsi.Add("BLOR"[rel] + ":" + h.Teklif);
            }
            return z;
        }

        /// <summary>Pas olmayan çağrı dizisi tam olarak bu mu?</summary>
        public bool Es(params string[] s) { return Hepsi.SequenceEqual(s); }
        /// <summary>Dizi bununla mı başlıyor?</summary>
        public bool Basi(params string[] s) { return Hepsi.Count >= s.Length && Hepsi.Take(s.Length).SequenceEqual(s); }
        public int Uz(string renk) { return El.Count(c => c.Suit == renk); }
        public int Maca { get { return Uz("Maça"); } }
        public int Kupa { get { return Uz("Kupa"); } }
        public int Karo { get { return Uz("Karo"); } }
        public int Sinek { get { return Uz("Sinek"); } }
    }
}
