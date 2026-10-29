using System;
using System.Collections.Generic;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Görünmeyen kartları iki gizli oyuncuya, bilinen kısıtlarla (kart sayıları, renksiz kalma bilgisi)
    /// düzgün (uniform) dağıtır. Kart kodu: renk * 16 + sıra (DDSolver ile aynı).
    /// </summary>
    public static class DunyaOrnekleyici
    {
        /// <summary>
        /// unseen: görünmeyen kartlar; adetA/adetB: oyuncuların elindeki kart sayısı (toplamları unseen sayısına eşit olmalı);
        /// renksizA/renksizB[renk]: oyuncunun o renkte kartı kalmadığı bilgisi. Başarısızsa false (kısıtlar çelişiyor).
        /// </summary>
        public static bool Uret(Random rnd, int[] unseen, int adetA, int adetB,
                                bool[] renksizA, bool[] renksizB, out ulong elA, out ulong elB)
        {
            elA = 0; elB = 0;
            if (unseen.Length != adetA + adetB) return false;

            int forcedA = 0, forcedB = 0;
            var free = new int[unseen.Length];
            int fc = 0;
            for (int i = 0; i < unseen.Length; i++)
            {
                int c = unseen[i];
                int s = c >> 4;
                bool a = !renksizA[s], b = !renksizB[s];
                if (!a && !b) return false;
                if (a && b) { free[fc++] = c; }
                else if (a) { elA |= 1UL << c; forcedA++; }
                else { elB |= 1UL << c; forcedB++; }
            }
            int needA = adetA - forcedA, needB = adetB - forcedB;
            if (needA < 0 || needB < 0 || needA + needB != fc) return false;

            for (int i = fc - 1; i > 0; i--)          // Fisher-Yates
            {
                int j = rnd.Next(i + 1);
                int t = free[i]; free[i] = free[j]; free[j] = t;
            }
            for (int i = 0; i < fc; i++)
            {
                if (i < needA) elA |= 1UL << free[i];
                else elB |= 1UL << free[i];
            }
            return true;
        }
    }
}
