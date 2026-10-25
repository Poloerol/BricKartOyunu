using System.Collections.Generic;
using BricKartOyunu.Class;

namespace BricKartOyunu.Class.Bidding.Interfaces
{
    public interface IAcilisGelisimi
    {
        bool UygunMu(List<Card> el, IhaleDurumu durum);
        string TeklifVer(List<Card> el, IhaleDurumu durum);
        int Oncelik { get; }

        // 🆕 Verilen teklifin anlamı (UI'da gösterilecek)
        string Aciklama { get; }
    }
}