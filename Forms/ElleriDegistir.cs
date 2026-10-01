using BricKartOyunu.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BricKartOyunu.Forms
{
    public partial class ElleriDegistir : Form
    {
        // 🔹 Programatik kapatma bayrağı
        private bool _programatikKapatma = false;
        public ElleriDegistir()
        {
            InitializeComponent();

            // 🔹 X'e basınca tüm formları kapat
            this.FormClosing += ElleriDegistir_FormClosing;

            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        /// <summary>
        /// X (kapatma) butonuna basıldığında tüm ilgili formları kapatır
        /// ve AnaSayfa'ya dönüşü tetikler.
        /// </summary>
        private void ElleriDegistir_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Uygulama tamamen kapanıyorsa engel koyma
            if (e.CloseReason == CloseReason.ApplicationExitCall ||
                e.CloseReason == CloseReason.WindowsShutDown ||
                e.CloseReason == CloseReason.TaskManagerClosing)
            {
                return;
            }

            // 🔹 Programatik kapatma (BtnOK_Click) → sadece bu form kapansın
            if (_programatikKapatma)
            {
                return;
            }

            // 🔹 Buraya geldiysek: Kullanıcı X'e bastı
            // Tüm ilgili formları kapat

            // 1) Açık DeklarasyonForm'ları kapat
            foreach (Form f in Application.OpenForms.Cast<Form>()
                        .Where(f => f is DeklarasyonForm)
                        .ToArray())
            {
                if (f is DeklarasyonForm dek)
                {
                    dek.IzinliKapat();
                }
            }

            // 2) Açık DekBasForm'ları kapat
            foreach (Form f in Application.OpenForms.Cast<Form>()
                        .Where(f => f is DekBasForm)
                        .ToArray())
            {
                f.Close();
            }

            // 3) En son BricOyna'yı kapat
            if (this.Owner is BricOyna bric)
            {
                bric.Close();
            }
            else
            {
                var bricFromOpen = Application.OpenForms.OfType<BricOyna>().FirstOrDefault();
                bricFromOpen?.Close();
            }
        }

        private void ElleriDegistir_Load(object sender, EventArgs e)
        {

        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (this.Owner is BricOyna bricOyna)
            {
                Player secilenYon;
                if (radioButton1.Checked) secilenYon = Player.Bati;
                else if (radioButton2.Checked) secilenYon = Player.Kuzey;
                else secilenYon = Player.Dogu;

                bricOyna.ElleriTakasEt(secilenYon);

                DeklarasyonForm deklarasyonForm = new DeklarasyonForm(bricOyna)
                {
                    StartPosition = FormStartPosition.Manual
                };

                Point panelCenterScreen = bricOyna.GetPlayZoneCenterScreen();
                deklarasyonForm.Location = new Point(
                    panelCenterScreen.X - deklarasyonForm.Width / 2,
                    panelCenterScreen.Y - deklarasyonForm.Height / 2
                );

                deklarasyonForm.Show(bricOyna);
                deklarasyonForm.BringToFront();
                deklarasyonForm.Activate();

                // 🔹 Programatik kapatma — event bunu X ile karıştırmasın
                _programatikKapatma = true;
                this.Close();
            }
        }
    }
}
