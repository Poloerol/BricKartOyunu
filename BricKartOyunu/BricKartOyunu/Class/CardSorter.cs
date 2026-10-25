using System;
using System.Collections.Generic;
using System.Linq;

namespace BricKartOyunu.Class
{
    public static class CardSorter
    {
        public static List<Card> SiraliEl(List<Card> el)
        {
            // 🔹 Tam olarak istediğin sıra: Maça (0), Kupa (1), Sinek (2), Karo (3)
            string[] suitOrder = { "Maça", "Kupa", "Sinek", "Karo" };

            return el
                .OrderBy(c => Array.IndexOf(suitOrder, c.Suit)) // Önce renk sırasına göre (Maça -> Kupa -> Sinek -> Karo)
                .ThenByDescending(c => c.Value)                 // Sonra renk içinde büyükten küçüğe (A, K, Q, J, 10...)
                .ToList();
        }
    }
}