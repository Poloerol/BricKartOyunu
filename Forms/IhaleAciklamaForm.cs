using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Teklif Açıklamaları: oynanan ihaleyi tüm ellerle yeniden oynatır; her teklif için motorun o elle hangi konvansiyondan
    /// (ya da yedek mantıktan) ne tekliflediğini, o konvansiyonun açıklamasını ve elin özetini (HP, dağılım) gösterir.
    /// Gerçek teklif motorun önerisinden farklıysa (ör. insan oyuncu) satır sarıyla işaretlenir ve motorun önerisi yazılır;
    /// bu durumda "anlamı" motorun o elle vereceği çağrıya aittir, gerçek çağrıya değil.
    /// </summary>
    public sealed class IhaleAciklamaForm : Form
    {
        private static readonly Player[] Sira = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };

        public IhaleAciklamaForm(List<string> teklifler, Player ilkOyuncu, Dictionary<Player, List<Card>> eller,
                                 Func<Player, string> ad, string baslik)
        {
            Text = "Teklif Açıklamaları — " + baslik;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(1040, 560);
            MinimumSize = new Size(820, 380);
            ShowInTaskbar = false;

            var not = new Label
            {
                Dock = DockStyle.Top, Height = 38, Padding = new Padding(12, 8, 12, 0),
                Text = "Her teklif, motorun o elle verdiği çağrıya göre açıklanır. Sarı satır: gerçek teklif motorun önerisinden farklı (açıklama motorun önerisine aittir)."
            };
            var liste = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, HideSelection = false };
            foreach (var h in new[] { "#", "Oyuncu", "Teklif", "El (HP, dağılım)", "Konvansiyon", "Anlamı", "Motor önerisi" })
                liste.Columns.Add(h, h == "#" ? 36 : h == "Oyuncu" ? 70 : h == "Teklif" ? 60 : h == "El (HP, dağılım)" ? 120 : h == "Konvansiyon" ? 170 : h == "Anlamı" ? 420 : 100, HorizontalAlignment.Left);
            Controls.Add(liste);
            Controls.Add(not);

            var motorNS = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.NS));
            var motorEW = new IhaleMotoru(AnlasmaYonetici.Mevcut(Taraf.EW));
            int bas = Array.IndexOf(Sira, ilkOyuncu);
            var gecmis = new List<IhaleHamlesi>();
            for (int i = 0; i < teklifler.Count; i++)
            {
                Player p = Sira[(bas + i) % 4];
                var motor = (p == Player.Kuzey || p == Player.Guney) ? motorNS : motorEW;
                string gercek = teklifler[i];
                string oneri = null, kural = null, anlam = null;
                try
                {
                    var d = new IhaleDurumu
                    {
                        AktifOyuncu = p, AktifOyuncuEli = eller[p], Anlasma = motor.Anlasma,
                        Gecmis = new List<IhaleHamlesi>(gecmis)
                    };
                    oneri = motor.TeklifVer(d);
                    kural = motor.SonKonvansiyonAdi;
                    anlam = motor.KonvansiyonAciklamasi(kural);
                }
                catch (Exception ex) { anlam = "(açıklanamadı: " + ex.Message + ")"; }
                gecmis.Add(new IhaleHamlesi { Oyuncu = p, Teklif = gercek, Sira = i + 1, GecerliMi = true });

                bool ayni = oneri == gercek;
                var it = new ListViewItem(new[]
                {
                    (i + 1).ToString(), ad(p), gercek, ElOzeti(eller[p]),
                    kural ?? "-", string.IsNullOrEmpty(anlam) ? (kural != null && kural.IndexOf("Fallback", StringComparison.OrdinalIgnoreCase) >= 0 ? "Genel değerlendirme (özel konvansiyon yok): güç ve dağılıma göre." : "") : anlam,
                    ayni ? "" : (oneri ?? "-")
                });
                if (!ayni) it.BackColor = Color.FromArgb(255, 247, 200);
                liste.Items.Add(it);
            }
        }

        private static string ElOzeti(List<Card> el)
        {
            int hp = ElDegerlendirici.HCP(el);
            var d = new[] { "Maça", "Kupa", "Karo", "Sinek" }.Select(r => ElDegerlendirici.RenkUzunlugu(el, r));
            return hp + " HP, " + string.Join("-", d);
        }
    }
}
