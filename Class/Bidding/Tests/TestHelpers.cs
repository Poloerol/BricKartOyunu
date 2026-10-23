using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;
using System;
using System.Collections.Generic;

namespace BricKartOyunu.Tests
{
    public static class TestHelpers
    {
        // "AKQJ..." gibi string'den el oluşturur (TestRunner.cs'deki mantığa benzer)
        public static List<Card> ElOlustur(string maca, string kupa, string karo, string sinek)
        {
            var el = new List<Card>();
            EkleRenk(el, "Maça", maca);
            EkleRenk(el, "Kupa", kupa);
            EkleRenk(el, "Karo", karo);
            EkleRenk(el, "Sinek", sinek);

            if (el.Count != 13)
                throw new ArgumentException($"El {el.Count} kart içeriyor, 13 olmalı! ({maca}/{kupa}/{karo}/{sinek})");
            return el;
        }

        private static void EkleRenk(List<Card> el, string suit, string cards)
        {
            if (string.IsNullOrEmpty(cards)) return;
            foreach (char c in cards)
            {
                int val = 0;
                switch (c)
                {
                    case 'A': val = 14; break;
                    case 'K': val = 13; break;
                    case 'Q': val = 12; break;
                    case 'J': val = 11; break;
                    case 'T': case 't': val = 10; break;
                    default:
                        if (char.IsDigit(c)) val = c - '0';
                        break;
                }
                if (val > 0) el.Add(new Card { Suit = suit, Value = val });
            }
        }

        // Belirli bir teklif geçmişi ile IhaleDurumu oluşturur
        public static IhaleDurumu DurumOlustur(List<Card> el, Player aktif, params string[] gecmisTeklifler)
        {
            var durum = new IhaleDurumu
            {
                AktifOyuncu = aktif,
                AktifOyuncuEli = el,
                ZonNS = false,
                ZonEW = false,
                Anlasma = OrtaklikAnlasmasi.Varsayilan()
            };

            // Aktif oyuncudan GERİYE doğru hesapla
            Player oyuncu = IhaleDurumu.OncekiOyuncu(aktif);
            for (int i = gecmisTeklifler.Length - 1; i >= 0; i--)
            {
                durum.Gecmis.Insert(0, new IhaleHamlesi
                {
                    Oyuncu = oyuncu,
                    Teklif = gecmisTeklifler[i],
                    Sira = i + 1
                });
                oyuncu = IhaleDurumu.OncekiOyuncu(oyuncu);
            }

            durum.AktifOyuncu = aktif;
            return durum;
        }
    }
}