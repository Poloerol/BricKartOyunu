using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class.Bidding.Conventions
{
    /// <summary>Yeni konvansiyonların ortak tabanı.</summary>
    internal abstract class YeniKonvTaban : IKonvansiyon, IBilincliPas
    {
        public abstract string Ad { get; }
        public abstract string Aciklama { get; }
        public bool AktifMi { get; set; } = true;
        public virtual int Oncelik { get { return 6; } }

        /// <summary>null = bu durumda uygun değil.</summary>
        protected abstract string Karar(Dz z);

        public bool UygunMu(IhaleDurumu durum)
        {
            if (durum == null || durum.AktifOyuncuEli == null || durum.AktifOyuncuEli.Count != 13) return false;
            if (durum.IhaleBittiMi()) return false;
            try { return Karar(Dz.Al(durum)) != null; } catch (Exception) { return false; }
        }

        public string TeklifVer(IhaleDurumu durum)
        {
            return Karar(Dz.Al(durum));
        }
    }
}
