using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BricKartOyunu
{
    public partial class Konvansiyon : Form
    {
        public Konvansiyon()
        {
            InitializeComponent();
        }

        private void BtnGeri_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["AnaSayfa"] is AnaSayfa anaSayfa)
            {
                anaSayfa.ResetToInitialState(); // AnaSayfa’yı ilk haline döndür
                anaSayfa.Show();                // AnaSayfa’yı görünür yap
            }
            this.Close(); // BricOyna’yı kapat
        }
    }
}
