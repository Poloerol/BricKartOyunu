using System;
using BricKartOyunu.Class;
using BricKartOyunu.Forms;

namespace BricKartOyunu
{
    // Rubber / Chicago puanlama: oyun planındaki oyun türüne göre zon ve skor (Class/KlasikSkor.cs)
    public partial class BricOyna
    {
        /// <summary>Oyun planındaki tür Rubber ya da Chicago ise ilgili puanlama; Duplicate, Tek El ve Briç Maçı için null.</summary>
        public PuanTuru? AktifPuanTuru()
        {
            if (MacDurumu.Aktif) return null;
            var plan = AnaSayfa.OyunPlani;
            if (plan == null) return null;
            switch (plan.OyunTuru)
            {
                case OyunTuru.Rubber: return PuanTuru.Rubber;
                case OyunTuru.ChicagoRubber: return PuanTuru.ChicagoRubber;
                case OyunTuru.ChicagoDuplicate: return PuanTuru.ChicagoDuplicate;
            }
            return null;
        }

        /// <summary>Oyun planında "Tek El" seçili mi (bordlar tek tek oynanır, duplicate puanı, puan taşınmaz).</summary>
        public bool TekElMi()
        {
            var plan = AnaSayfa.OyunPlani;
            return plan != null && plan.OyunTuru == OyunTuru.TekEl && !MacDurumu.Aktif;
        }

        /// <summary>Sıradaki bordun fiziksel dağıtıcısı (Chicago zonu için).</summary>
        public Player SiradakiFizikselDagitici()
        {
            var plan = AnaSayfa.OyunPlani ?? new OyunPlaniSecenekleri();
            return plan.FizikselDagitici(_currentTour + 1);
        }

        /// <summary>Biten bordu puan oturumuna yazar (Replay'de aynı bord üzerine yazılır).</summary>
        private void PuanOturumunaKaydet()
        {
            var tur = AktifPuanTuru();
            if (!tur.HasValue) return;
            try
            {
                var g = new BordGirdisi { BordNo = _currentTour, Dagitici = KoltukHaritasi.Fiz(_dealer) };
                if (string.IsNullOrEmpty(_kontrat))
                {
                    g.Pas = true;
                }
                else
                {
                    g.Seviye = _kontrat[0] - '0';
                    g.Koz = _kontrat.IndexOf("NT", StringComparison.OrdinalIgnoreCase) >= 0 ? "NT" : _kontrat.Substring(1, 1);
                    g.Kontr = _kontrDurumu;
                    bool icNS = _kontratDeklaran == Player.Kuzey || _kontratDeklaran == Player.Guney;
                    g.AlinanLove = icNS ? _kazanilanElNS : _kazanilanElEW;
                    Player fiz = KoltukHaritasi.Fiz(_kontratDeklaran);
                    g.DeklaranNS = fiz == Player.Kuzey || fiz == Player.Guney;
                }
                PuanOturumu.Kaydet(tur.Value, g);
            }
            catch { /* kayıt alınamazsa bord puan listesine girmez */ }
        }
    }
}
