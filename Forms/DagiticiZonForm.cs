using System;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// El Girişi'nden sonra açılır: dağıtıcıyı ve zonu (risk durumu) seçtirir.
    /// Koltuklar fiziksel pusulaya göredir. Zon: 0 = zonsuz, 1 = K/G, 2 = D/B, 3 = herkes.
    /// </summary>
    public class DagiticiZonForm : Form
    {
        private static readonly Player[] Koltuklar = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };
        private static readonly string[] KoltukAdlari = { "Kuzey", "Doğu", "Güney", "Batı" };
        private static readonly string[] ZonAdlari = { "Zonsuz", "K/G", "D/B", "Herkes" };

        private readonly RadioButton[] _dag = new RadioButton[4];
        private readonly RadioButton[] _zon = new RadioButton[4];

        public Player Dagitici
        {
            get
            {
                for (int i = 0; i < 4; i++) if (_dag[i].Checked) return Koltuklar[i];
                return Player.Kuzey;
            }
        }

        public int Zon
        {
            get
            {
                for (int i = 0; i < 4; i++) if (_zon[i].Checked) return i;
                return 0;
            }
        }

        public DagiticiZonForm(Player dagitici, int zon)
        {
            Text = "Dağıtıcı ve Zon Seçimi";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false; ControlBox = false; ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(244, 160);

            var gDag = new GroupBox { Text = "Dağıtıcı", Left = 10, Top = 8, Width = 104, Height = 104 };
            var gZon = new GroupBox { Text = "Zon", Left = 126, Top = 8, Width = 108, Height = 104 };
            for (int i = 0; i < 4; i++)
            {
                _dag[i] = new RadioButton { Text = KoltukAdlari[i], Left = 10, Top = 18 + i * 21, Width = 90, Height = 20, Checked = Koltuklar[i] == dagitici };
                _zon[i] = new RadioButton { Text = ZonAdlari[i], Left = 10, Top = 18 + i * 21, Width = 94, Height = 20, Checked = i == zon };
                gDag.Controls.Add(_dag[i]);
                gZon.Controls.Add(_zon[i]);
            }
            Controls.Add(gDag);
            Controls.Add(gZon);

            var btnOk = new Button { Text = "OK", Left = 82, Top = 124, Width = 80, Height = 25, DialogResult = DialogResult.OK };
            Controls.Add(btnOk);
            AcceptButton = btnOk;
        }
    }
}
