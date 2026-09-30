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
    public partial class KartDagitici : Form
    {
        public KartDagitici()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["AnaSayfa"] is AnaSayfa anaSayfa)
            {
                anaSayfa.ResetToInitialState(); // AnaSayfa’yı ilk haline döndür
                anaSayfa.Show();
            }
            this.Close();
        }
    }
}
