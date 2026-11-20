using System;
using System.Drawing;
using System.Windows.Forms;
using BricKartOyunu.Class;

namespace BricKartOyunu
{
    // Konvansiyon pratiği: liste penceresi — dağıtım numaralarını (seed) kopyalama ve eski bir bordu yeniden oynama
    public partial class BricOyna
    {
        private int _pratikHedef = -1;   // ≥0: bir sonraki PratikIlerlet bu indekse atlar (listeden "Bu bordu oyna")

        /// <summary>Eylemler menüsünden: pratik listesini gösterir. Pratik yoksa bilgi verir.</summary>
        private void PratikListesiAc()
        {
            if (PratikDagitimlari == null || PratikDagitimlari.Count == 0)
            {
                MessageBox.Show(this, "Şu anda bir konvansiyon pratiği yok (Konvansiyonlar > Pratik ile başlatılır).",
                    "Pratik Listesi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var f = new Form
            {
                Text = "Pratik Listesi — " + PratikDagitimlari[0].Konvansiyon,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(520, 420),
                MinimizeBox = false,
                MaximizeBox = false,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ShowInTaskbar = false
            };
            var lv = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false,
                GridLines = true,
                Location = new Point(10, 10),
                Size = new Size(488, 320)
            };
            lv.Columns.Add("Bord", 55);
            lv.Columns.Add("Dağıtıcı", 80);
            lv.Columns.Add("Dağıtım no (seed)", 140);
            lv.Columns.Add("Tür", 190);
            for (int i = 0; i < PratikDagitimlari.Count; i++)
            {
                var p = PratikDagitimlari[i];
                var it = new ListViewItem((i + 1).ToString());
                it.SubItems.Add(PlayerIsmiTurkce(KonvansiyonPratigi.Dagitici(p.Seed)));
                it.SubItems.Add(p.Seed.ToString());
                it.SubItems.Add(p.YakinKacis ? "Yakın kaçış (varyant " + p.Varyant + ")" : "Konvansiyon kullanılıyor");
                it.Tag = i;
                if (i == _pratikIndeks) { it.Font = new Font(lv.Font, FontStyle.Bold); it.Selected = true; it.BackColor = Color.LightYellow; }
                lv.Items.Add(it);
            }

            var kopyala = new Button { Text = "Seed Kopyala", Location = new Point(10, 342), Size = new Size(110, 30) };
            var oyna = new Button { Text = "Bu Bordu Oyna", Location = new Point(130, 342), Size = new Size(120, 30) };
            var kapat = new Button { Text = "Kapat", Location = new Point(398, 342), Size = new Size(100, 30), DialogResult = DialogResult.Cancel };
            f.CancelButton = kapat;

            Action kopyalaEylemi = () =>
            {
                if (lv.SelectedItems.Count == 0) return;
                string seed = PratikDagitimlari[(int)lv.SelectedItems[0].Tag].Seed.ToString();
                try { Clipboard.SetText(seed); kopyala.Text = "Kopyalandı: " + seed; }
                catch (Exception) { MessageBox.Show(f, "Panoya kopyalanamadı. Seed: " + seed, "Pratik Listesi"); }
            };
            Action oynaEylemi = () =>
            {
                if (lv.SelectedItems.Count == 0) return;
                int idx = (int)lv.SelectedItems[0].Tag;
                f.Close();
                PratikBordunaGit(idx);
            };
            kopyala.Click += (s, e) => kopyalaEylemi();
            oyna.Click += (s, e) => oynaEylemi();
            lv.DoubleClick += (s, e) => oynaEylemi();
            lv.KeyDown += (s, e) => { if (e.Control && e.KeyCode == Keys.C) { kopyalaEylemi(); e.Handled = true; } };

            f.Controls.Add(lv); f.Controls.Add(kopyala); f.Controls.Add(oyna); f.Controls.Add(kapat);
            f.Show(this);
        }

        /// <summary>Pratik listesinde seçilen bordu yeniden dağıtır (geriye dönüp eski bordu oynamak için).</summary>
        private void PratikBordunaGit(int indeks)
        {
            if (PratikDagitimlari == null || indeks < 0 || indeks >= PratikDagitimlari.Count) return;
            if (indeks == _pratikIndeks) return;   // zaten bu bordtayız
            var cevap = MessageBox.Show(this,
                "Bord " + (indeks + 1) + "/" + PratikDagitimlari.Count + " yeniden dağıtılacak; mevcut bord bırakılır.\nDevam edilsin mi?",
                "Pratik Listesi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (cevap != DialogResult.Yes) return;
            _pratikHedef = indeks;
            YeniBordBaslat();
            _pratikHedef = -1;
        }
    }
}
