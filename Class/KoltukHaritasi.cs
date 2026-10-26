using System;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// İç mantık her zaman "insan = Güney (ekranın altı)" varsayar.
    /// Oyun Planı'nda insan başka bir koltuğa (Kuzey/Doğu/Batı) atanırsa,
    /// yalnızca GÖRÜNEN adlar, harfler, zon ve dağıtıcı bu sınıfla çevrilir.
    /// Saat yönü: Güney → Batı → Kuzey → Doğu.
    /// </summary>
    public static class KoltukHaritasi
    {
        private static readonly Player[] Sira = { Player.Guney, Player.Bati, Player.Kuzey, Player.Dogu };
        private static int _kayma; // insanın fiziksel koltuğunun Sira'daki indeksi

        public static Player Insan
        {
            get { return Sira[_kayma]; }
            set { _kayma = Array.IndexOf(Sira, value); }
        }

        /// <summary>Kuzey, Doğu, Güney, Batı sırasındaki InsanMi dizisinden uygular.</summary>
        public static void Uygula(bool[] insanMi)
        {
            Player[] ayarSirasi = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
            Insan = Player.Guney;
            if (insanMi == null) return;
            for (int i = 0; i < 4 && i < insanMi.Length; i++)
                if (insanMi[i]) { Insan = ayarSirasi[i]; return; }
        }

        /// <summary>İç koltuk → fiziksel (görünen) koltuk.</summary>
        public static Player Fiz(Player ic)
        {
            return Sira[(Array.IndexOf(Sira, ic) + _kayma) % 4];
        }

        /// <summary>Fiziksel koltuk → iç koltuk.</summary>
        public static Player Ic(Player fiz)
        {
            return Sira[(Array.IndexOf(Sira, fiz) - _kayma + 4) % 4];
        }

        /// <summary>İnsan Doğu/Batı ise iç K-G çifti fiziksel D-B çiftidir.</summary>
        public static bool CiftlerTers
        {
            get { return _kayma % 2 == 1; }
        }

        /// <summary>İç koltuğun ekranda görünen Türkçe adı.</summary>
        public static string Ad(Player ic)
        {
            switch (Fiz(ic))
            {
                case Player.Kuzey: return "Kuzey";
                case Player.Guney: return "Güney";
                case Player.Bati: return "Batı";
                default: return "Doğu";
            }
        }

        public static string Harf(Player fiz)
        {
            switch (fiz)
            {
                case Player.Kuzey: return "N";
                case Player.Dogu: return "E";
                case Player.Guney: return "G";
                default: return "B";
            }
        }
    }
}
