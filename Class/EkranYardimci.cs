using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        // ---------- Acik alt formlarin ana pencereyi takip etmesi ----------
        // Ana pencere (BricOyna / AnaSayfa) tasinirsa ona bagli (Owner) acik formlar ayni miktarda tasinir;
        // pencere kucultulup buyutulurse formlar, sahibin penceresindeki goreli konumlarini korur ve
        // calisma alani (gorev cubugu haric ekran) disina tasmaz. Form boyutlari degistirilmez.
        // DekBasForm kendi Hizala() metoduyla masa ortasina hizalandigi icin burada atlanir.
        private static readonly Dictionary<Form, Rectangle> _sahipSon = new Dictionary<Form, Rectangle>();
        private static bool _bosta;

        public static void AcikFormlariTakipEt(Form sahip)
        {
            if (sahip == null) return;
            sahip.Shown += (s, e) => { _sahipSon[sahip] = sahip.Bounds; };
            EventHandler degisti = (s, e) => SahipKontrol(sahip);
            sahip.LocationChanged += degisti;
            sahip.SizeChanged += degisti;
            sahip.FormClosed += (s, e) => _sahipSon.Remove(sahip);
            // Guvenlik agi: surukleme/boyutlandirma dongusu bittiginde (olaylar kacmis olsa bile) bir kez daha kontrol et
            if (!_bosta)
            {
                _bosta = true;
                Application.Idle += (s, e) =>
                {
                    foreach (Form f in _sahipSon.Keys.ToArray()) SahipKontrol(f);
                };
            }
        }

        private static void SahipKontrol(Form sahip)
        {
            try
            {
                Rectangle eski;
                if (sahip.IsDisposed || !_sahipSon.TryGetValue(sahip, out eski)) return;
                if (sahip.WindowState == FormWindowState.Minimized || !sahip.Visible) return;
                Rectangle simdi = sahip.Bounds;
                if (simdi == eski) return;
                _sahipSon[sahip] = simdi;
                if (eski.Width <= 0 || eski.Height <= 0) return;
                SahipDegisti(sahip, eski, simdi);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[EkranYardimci] Form takibi hatasi: " + ex.Message);
            }
        }
        private static void SahipDegisti(Form sahip, Rectangle eski, Rectangle simdi)
        {
            Rectangle wa = Screen.FromRectangle(simdi).WorkingArea;
            bool sadeceTasindi = eski.Size == simdi.Size;
            int dx = simdi.Left - eski.Left, dy = simdi.Top - eski.Top;
            foreach (Form f in sahip.OwnedForms.Cast<Form>().ToArray())
            {
                if (f.IsDisposed || !f.Visible || f.GetType().Name == "DekBasForm") continue;
                // Masa ortasinda acilan formlar (Deklarasyon, Elleri Degistir, Bord Sonucu): pencere boyutu
                // degisince yeniden masanin (oyun alaninin) ortasina alinir; ekran disina tasmaz.
                BricOyna oyna = sahip as BricOyna;
                if (!sadeceTasindi && oyna != null && (f is BricKartOyunu.Forms.DeklarasyonForm
                    || f is BricKartOyunu.Forms.ElleriDegistir || f is BricKartOyunu.Forms.DealCompleteForm))
                {
                    Point m = oyna.GetPlayZoneCenterScreen();
                    FormuTasi(f, new Point(m.X - f.Width / 2, m.Y - f.Height / 2), wa, true);
                    continue;
                }
                Point hedef;
                if (sadeceTasindi)
                    hedef = new Point(f.Left + dx, f.Top + dy);
                else
                {
                    double fx = (f.Left + f.Width / 2.0 - eski.Left) / eski.Width;
                    double fy = (f.Top + f.Height / 2.0 - eski.Top) / eski.Height;
                    hedef = new Point(
                        (int)Math.Round(simdi.Left + fx * simdi.Width - f.Width / 2.0),
                        (int)Math.Round(simdi.Top + fy * simdi.Height - f.Height / 2.0));
                }
                FormuTasi(f, hedef, wa, !sadeceTasindi);
            }
        }

        private static void FormuTasi(Form f, Point hedef, Rectangle wa, bool sigdir)
        {
            if (sigdir)
            {
                hedef.X = Math.Max(wa.Left, Math.Min(hedef.X, wa.Right - f.Width));
                hedef.Y = Math.Max(wa.Top, Math.Min(hedef.Y, wa.Bottom - f.Height));
            }
            int dx = hedef.X - f.Left, dy = hedef.Y - f.Top;
            if (dx == 0 && dy == 0) return;
            f.Location = hedef;
            foreach (Form o in f.OwnedForms.Cast<Form>().ToArray())
                if (!o.IsDisposed && o.Visible)
                    FormuTasi(o, new Point(o.Left + dx, o.Top + dy), wa, sigdir);
        }
    }
}
