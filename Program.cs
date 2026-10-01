using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BricKartOyunu
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new AnaSayfa());
            }
            finally
            {
                // 🔹 Uygulama kapanırken kart resim önbelleğini temizle
                try
                {
                    BricOyna.CacheTemizle();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[Program] Cache temizlenirken hata: {ex.Message}");
                }
            }
        }
    }
}
