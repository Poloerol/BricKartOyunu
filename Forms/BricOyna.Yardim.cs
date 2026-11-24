using System;
using BricKartOyunu.Forms;

namespace BricKartOyunu
{
    // Yardım menüsü: Yardim\*.md konularını gösteren pencere (YardimForm).
    public partial class BricOyna
    {
        private void YardimKonulariToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new YardimForm()) f.ShowDialog(this);
        }

        private void HakkindaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new YardimForm("hakkinda")) f.ShowDialog(this);
        }
    }
}
