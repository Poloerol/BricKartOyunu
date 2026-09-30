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
        public ElleriDegistir()
        {
            InitializeComponent();

            // 🔹 Titreme önlemi: standart double-buffer kalıbı.
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }



        private void ElleriDegistir_Load(object sender, EventArgs e)
        {

        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (this.Owner is BricOyna bricOyna)
            {
                // 🔹 Seçili radio button'a göre Güney'in eli (kullanıcının eli), seçilen
                // yönün eliyle takas edilir. radioButton1=Batı, radioButton2=Kuzey,
                // radioButton3=Doğu (bkz. label1: "Hangi el Güney'in Eli ile değiştirilecek?").
                Player secilenYon;
                if (radioButton1.Checked) secilenYon = Player.Bati;
                else if (radioButton2.Checked) secilenYon = Player.Kuzey;
                else secilenYon = Player.Dogu;

                bricOyna.ElleriTakasEt(secilenYon);

                DeklarasyonForm deklarasyonForm = new DeklarasyonForm(bricOyna)
                {
                    StartPosition = FormStartPosition.Manual
                };

                // playZonePanel'in ekran üzerindeki merkezini bul
                Point panelCenterScreen = bricOyna.GetPlayZoneCenterScreen();

                // DeklarasyonForm'un merkezini playZonePanel merkezine getir
                deklarasyonForm.Location = new Point(
                    panelCenterScreen.X - deklarasyonForm.Width / 2,
                    panelCenterScreen.Y - deklarasyonForm.Height / 2
                );

                // BricOyna'nın sahibi olduğu form olarak aç
                deklarasyonForm.Show(bricOyna);
                deklarasyonForm.BringToFront();
                deklarasyonForm.Activate();

                // ElleriDegistir'i kapat
                this.Close();
            }
        }
    }
}
