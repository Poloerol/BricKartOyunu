using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Ekran titremesini azaltan yardımcılar:
    /// - CizimiDondur / CizimiAc : toplu değişiklikler (kart dağıtma, el açma/kapama) sırasında
    ///   pencere çizimini durdurur, iş bitince tek seferde çizer. İç içe çağrılabilir.
    /// - TumKontrollerdeCiftTampon : Panel, ListView vb. kontrollerde DoubleBuffered'ı açar.
    /// </summary>
    public static class EkranYardimci
    {
        private const int WM_SETREDRAW = 0x000B;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, bool wParam, IntPtr lParam);

        private static readonly Dictionary<Control, int> _sayac = new Dictionary<Control, int>();
        private static readonly PropertyInfo _doubleBuffered =
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void CizimiDondur(Control c)
        {
            if (c == null || c.IsDisposed || !c.IsHandleCreated) return;
            int n;
            _sayac.TryGetValue(c, out n);
            if (n == 0) SendMessage(c.Handle, WM_SETREDRAW, false, IntPtr.Zero);
            _sayac[c] = n + 1;
        }

        public static void CizimiAc(Control c)
        {
            if (c == null) return;
            int n;
            if (!_sayac.TryGetValue(c, out n)) return;
            n--;
            if (n > 0) { _sayac[c] = n; return; }

            _sayac.Remove(c);
            if (c.IsDisposed || !c.IsHandleCreated) return;
            SendMessage(c.Handle, WM_SETREDRAW, true, IntPtr.Zero);
            c.Invalidate(true);
        }

        /// <summary>Kökten başlayarak tüm alt kontrollerde çift tamponlamayı açar.</summary>
        public static void TumKontrollerdeCiftTampon(Control kok)
        {
            if (kok == null || _doubleBuffered == null) return;
            try
            {
                if (!(kok is ToolStrip)) _doubleBuffered.SetValue(kok, true, null);
            }
            catch { }
            foreach (Control c in kok.Controls)
                TumKontrollerdeCiftTampon(c);
        }
    }
}
