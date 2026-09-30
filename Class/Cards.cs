using System.Drawing;

namespace BricKartOyunu.Class
{
    public class Card
    {
        public string Suit { get; set; }   // Maça, Kupa, Sinek, Karo
        public int Value { get; set; }     // 2–14 (11=Vale, 12=Kız, 13=Papaz, 14=As)
        public Image Image { get; set; }   // Kart resmi
    }
}
