using BricKartOyunu.Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BricKartOyunu.Forms
{
    /// <summary>
    /// Bu bordda oynanan tüm elleri 4 sütunlu tablo halinde gösterir.
    /// Her satır bir el (tur), her sütun bir oyuncu.
    /// Kazanan kart "*" ile işaretlenir.
    /// </summary>
    public partial class OynananElleriGosterForm : Form
    {
        private readonly List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)> _eller;

        public OynananElleriGosterForm(
            List<(int ElNo, Player Oyuncu, Card Kart, bool Kazandi)> eller)
        {
            InitializeComponent();
            _eller = eller;

            this.ShowInTaskbar = false;

            ListeyiDoldur();
        }

        private void OynananElleriGosterForm_Load(object sender, EventArgs e)
        {
            // Başlık
            this.Text = "Oynanan Eller";
            this.StartPosition = FormStartPosition.CenterParent;
        }

        /// <summary>
        /// Oynanan eller listesini ListView'e doldurur.
        /// Her satır: El No, Batı, Kuzey, Doğu, Güney
        /// </summary>
        private void ListeyiDoldur()
        {
            listView1.Items.Clear();
            listView1.Columns.Clear();

            // Sütunlar
            listView1.Columns.Add("Tur", 40, HorizontalAlignment.Center);
            listView1.Columns.Add("Batı", 60, HorizontalAlignment.Center);
            listView1.Columns.Add("Kuzey", 60, HorizontalAlignment.Center);
            listView1.Columns.Add("Doğu", 60, HorizontalAlignment.Center);
            listView1.Columns.Add("Güney", 60, HorizontalAlignment.Center);

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listView1.OwnerDraw = true;
            listView1.DrawColumnHeader += ListView1_DrawColumnHeader;
            listView1.DrawSubItem += ListView1_DrawSubItem;

            // El numaralarına göre grupla
            var gruplar = new Dictionary<int, List<(Player Oyuncu, Card Kart, bool Kazandi)>>();
            foreach (var e in _eller)
            {
                if (!gruplar.ContainsKey(e.ElNo))
                    gruplar[e.ElNo] = new List<(Player, Card, bool)>();
                gruplar[e.ElNo].Add((e.Oyuncu, e.Kart, e.Kazandi));
            }

            // Her el için satır ekle
            for (int elNo = 1; elNo <= 13; elNo++)
            {
                if (!gruplar.ContainsKey(elNo)) continue;

                var el = gruplar[elNo];

                // Oyuncu sırasına göre kartları bul: Batı, Kuzey, Doğu, Güney
                string bati = KartMetniBul(el, Player.Bati);
                string kuzey = KartMetniBul(el, Player.Kuzey);
                string dogu = KartMetniBul(el, Player.Dogu);
                string guney = KartMetniBul(el, Player.Guney);

                var item = new ListViewItem(elNo.ToString());
                item.SubItems.Add(bati);
                item.SubItems.Add(kuzey);
                item.SubItems.Add(dogu);
                item.SubItems.Add(guney);
                item.Tag = el;   // 🔹 Renk kodlaması için sakla
                listView1.Items.Add(item);
            }

            // Boyut
            listView1.Width = 40 + 60 * 4 + 25;
            listView1.Height = Math.Min(13 * 22 + 30, 400);
        }

        /// <summary>
        /// Belirtilen elde, belirtilen oyuncunun kart metnini döndürür.
        /// Kazanan ise başına "*" ekler.
        /// </summary>
        private string KartMetniBul(
            List<(Player Oyuncu, Card Kart, bool Kazandi)> el, Player oyuncu)
        {
            foreach (var (o, k, kazandi) in el)
            {
                if (o == oyuncu)
                {
                    string kartMetni = KartMetni(k);
                    return kazandi ? "*" + kartMetni : kartMetni;
                }
            }
            return "";
        }

        /// <summary>
        /// Bir Card nesnesini "A♣" formatına çevirir.
        /// </summary>
        private string KartMetni(Card card)
        {
            if (card == null) return "";

            string rank;
            switch (card.Value)
            {
                case 14: rank = "A"; break;
                case 13: rank = "K"; break;
                case 12: rank = "Q"; break;
                case 11: rank = "J"; break;
                default: rank = card.Value.ToString(); break;
            }

            string koz;
            switch (card.Suit)
            {
                case "Maça": koz = "♠"; break;
                case "Kupa": koz = "♥"; break;
                case "Karo": koz = "♦"; break;
                case "Sinek": koz = "♣"; break;
                default: koz = "?"; break;
            }

            return rank + koz;
        }

        // ═══════════════════════════════════════════════════════════════
        // OWNER-DRAW — RENK KODLAMASI
        // ═══════════════════════════════════════════════════════════════

        private void ListView1_DrawColumnHeader(object sender,
            DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListView1_DrawSubItem(object sender,
            DrawListViewSubItemEventArgs e)
        {
            // El No sütunu (0. sütun) → default
            if (e.ColumnIndex == 0)
            {
                e.DrawDefault = true;
                return;
            }

            // Arka plan
            e.DrawBackground();

            string text = e.SubItem.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // Renk belirle
            Color foreColor = Color.Black;
            if (text.Contains("♥") || text.Contains("♦"))
                foreColor = Color.FromArgb(200, 0, 0);   // Kırmızı
            else
                foreColor = Color.Black;

            // Font
            Font font = new Font("Segoe UI", 9F,
                text.StartsWith("*") ? FontStyle.Bold : FontStyle.Regular);

            // Metin çiz
            using (var brush = new SolidBrush(foreColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                e.Graphics.DrawString(text, font, brush, e.Bounds, sf);
            }

            font.Dispose();
        }
    }
}