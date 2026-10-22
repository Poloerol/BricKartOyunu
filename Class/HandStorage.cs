using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BricKartOyunu.Class
{
    public static class HandStorage
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hatali_eller.txt");

        /// <summary>
        /// Verilen el numarasına (Seed) göre 52 kartı karıştırır ve 4 oyuncuya dağıtır.
        /// Aynı el numarası her zaman yüzde yüz aynı kart dağılımını üretir.
        /// </summary>
        public static Dictionary<Player, List<Card>> ElGetir(long dealNumber)
        {
            Random rng = new Random((int)(dealNumber % int.MaxValue));

            List<Card> deste = new List<Card>();
            string[] suits = { "Maça", "Kupa", "Sinek", "Karo" };

            foreach (var suit in suits)
            {
                for (int value = 2; value <= 14; value++)
                {
                    deste.Add(new Card
                    {
                        Suit = suit,
                        Value = value,
                        Image = null
                    });
                }
            }

            // Fisher-Yates Algoritması ile Desteyi Karıştır
            for (int i = deste.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = deste[i];
                deste[i] = deste[j];
                deste[j] = temp;
            }

            var elDagitimi = new Dictionary<Player, List<Card>>
            {
                { Player.Kuzey, new List<Card>() },
                { Player.Dogu,  new List<Card>() },
                { Player.Guney, new List<Card>() },
                { Player.Bati,  new List<Card>() }
            };

            for (int i = 0; i < 52; i++)
            {
                Player oyuncu = (Player)(i % 4);
                elDagitimi[oyuncu].Add(deste[i]);
            }

            foreach (var oyuncu in elDagitimi.Keys.ToList())
            {
                elDagitimi[oyuncu] = CardSorter.SiraliEl(elDagitimi[oyuncu]);
            }

            return elDagitimi;
        }

        /// <summary>
        /// Yeni ve rastgele bir El Numarası (Deal ID) üretir.
        /// </summary>
        public static long YeniElNumarasiUret()
        {
            Random rand = new Random();
            long val1 = rand.Next(10000, 99999);
            long val2 = rand.Next(10000, 99999);
            return (val1 * 100000) + val2;
        }

        /// <summary>
        /// Hatalı ya da test edilmek istenen el numarasını açıklamasıyla kaydeder.
        /// </summary>
        public static void HataliEliKaydet(long dealNumber, string aciklama = "")
        {
            try
            {
                string satir = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Deal #{dealNumber} | Not: {aciklama}";
                File.AppendAllLines(LogFilePath, new[] { satir });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Kayıt hatası: {ex.Message}");
            }
        }
    }
}