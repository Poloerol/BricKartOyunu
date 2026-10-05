using System;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// Tek bir ihale hamlesini temsil eder.
    /// Örnek: "Güney 1♠ dedi", "Batı Pas dedi", "Kuzey Dbl dedi"
    /// 
    /// Kullanım:
    ///   var hamle = new IhaleHamlesi
    ///   {
    ///       Oyuncu = Player.Guney,
    ///       Teklif = "1♠",
    ///       Sira = 1
    ///   };
    /// </summary>
    public class IhaleHamlesi
    {
        /// <summary>
        /// Hamleyi yapan oyuncu.
        /// </summary>
        public Player Oyuncu { get; set; }

        /// <summary>
        /// Teklif metni.
        /// Geçerli değerler: "Pas", "1♣", "1♦", "1♥", "1♠", "1NT",
        /// "2♣", ..., "7NT", "Dbl" (Kontr), "RDbl" (S.Kontr)
        /// </summary>
        public string Teklif { get; set; }

        /// <summary>
        /// Bu hamlenin ihale sırasındaki numarası (1'den başlar).
        /// </summary>
        public int Sira { get; set; }

        /// <summary>
        /// Bu hamle hâlâ geçerli mi?
        /// (Kontr/S.Kontr sonrası eski teklifler geçersiz olabilir.)
        /// </summary>
        public bool GecerliMi { get; set; } = true;

        /// <summary>
        /// Bu hamle "Pas" mı?
        /// </summary>
        public bool PasMi => Teklif == "Pas" || Teklif == "PAS";

        /// <summary>
        /// Bu hamle "Kontr" mu?
        /// </summary>
        public bool KontrMi => Teklif == "Dbl" || Teklif == "Kontr";

        /// <summary>
        /// Bu hamle "S.Kontr" mu?
        /// </summary>
        public bool SKontrMi => Teklif == "RDbl" || Teklif == "S.Kontr";

        /// <summary>
        /// Bu hamle "gerçek bir teklif" mi?
        /// (Pas, Kontr, S.Kontr değilse gerçek tekliftir.)
        /// </summary>
        public bool GercekTeklifMi => !PasMi && !KontrMi && !SKontrMi;

        /// <summary>
        /// Teklifin seviyesi (1-7).
        /// Örnek: "3NT" → 3. "Pas"/"Dbl"/"RDbl" → 0.
        /// </summary>
        public int Seviye
        {
            get
            {
                if (!GercekTeklifMi) return 0;
                if (string.IsNullOrEmpty(Teklif)) return 0;

                if (int.TryParse(Teklif.Substring(0, 1), out int seviye))
                    return seviye;

                return 0;
            }
        }

        /// <summary>
        /// Teklifin kozu (renk).
        /// Örnek: "3NT" → "NT", "1♠" → "Maça", "Pas" → null.
        /// </summary>
        public string Koz
        {
            get
            {
                if (!GercekTeklifMi) return null;
                if (string.IsNullOrEmpty(Teklif)) return null;

                char sonKarakter = Teklif[Teklif.Length - 1];

                switch (sonKarakter)
                {
                    case '♠': return "Maça";
                    case '♥': return "Kupa";
                    case '♦': return "Karo";
                    case '♣': return "Sinek";
                    case 'T':
                    case 't': return "NT";
                    default: return null;
                }
            }
        }

        /// <summary>
        /// Metin gösterimi (debug için).
        /// </summary>
        public override string ToString()
        {
            return $"{Sira}. {Oyuncu}: {Teklif}";
        }
    }
}