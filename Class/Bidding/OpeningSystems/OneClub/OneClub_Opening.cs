using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Interfaces;
using System.Collections.Generic;

public class OneClub_Opening : IAcilisGelisimi
{
    public int Oncelik => 4;
    // Açılışın anlamı (UI'da gösterilecek)
    public string Aciklama { get; private set; } = "1♣: 12-14 HCP dengeli, 2+♣, 5'li majör yok, 4+ karo yok";

    public bool UygunMu(List<Card> el, IhaleDurumu durum)
    {
        if (durum == null || el == null) return false;
        if (el.Count != 13) return false;
        if (!durum.IlkTeklifMi()) return false;

        int hp = ElDegerlendirici.HCP(el);
        if (hp < 12 || hp > 21) return false;
        if (ElDegerlendirici.BesliMajorVar(el)) return false;
        if (hp >= 15 && hp <= 17 && ElDegerlendirici.DengeliEl(el)) return false;

        return TeklifVer(el, durum) == "1♣";
    }

    public string TeklifVer(List<Card> el, IhaleDurumu durum)
    {
        if (el == null) return "Pas";

        int karo = ElDegerlendirici.RenkUzunlugu(el, "Karo");
        int sinek = ElDegerlendirici.RenkUzunlugu(el, "Sinek");
        int hp = ElDegerlendirici.HCP(el);

        if (karo >= 4 && karo >= sinek)
        {
            Aciklama = "1♦: 4+♦, dengesiz veya dengeli";
            return "1♦";
        }

        // 1♣ açılışı — anlamı belirle
        if (hp >= 12 && hp <= 14 && ElDegerlendirici.DengeliEl(el))
            Aciklama = "1♣: Dengeli 12-14 HCP, 2+♣, 5'li majör yok, 4+ karo yok";
        else if (sinek >= 5 && !ElDegerlendirici.DengeliEl(el))
            Aciklama = $"1♣: Dengesiz {hp} HCP, 5+♣ (iki renkli)";
        else if (sinek >= 6)
            Aciklama = $"1♣: {sinek}'li ♣, {hp} HCP (tek renkli)";
        else
            Aciklama = $"1♣: {hp} HCP, {sinek}+♣";

        return "1♣";
    }
}