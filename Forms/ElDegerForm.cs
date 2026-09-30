using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
// Projenizdeki Kart modelinin bulunduğu namespace'i ekleyin (Örn: BricKartOyunu.Models)
using BricKartOyunu.Class;

namespace BricKartOyunu.Forms
{
    public partial class ElDegerForm : Form
    {
        private ListView lstEvaluation;
        private Label lblExplanation;
        private ComboBox cmbExplanation;
        private TextBox txtExplanationDetails;
        private Button btnOK;

        public ElDegerForm()
        {
            InitializeComponent();
            SetupFormCustomControls();
        }

        // Güney'in 13 kartı parametre olarak kabul eden yeni yapıcı (Constructor)
        public ElDegerForm(List<Card> guneyElinKartlari) : this()
        {
            DegerleriYukle(guneyElinKartlari);
        }

        private void SetupFormCustomControls()
        {
            this.Text = "El Değerlendirme";
            this.Size = new Size(430, 430);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lstEvaluation = new ListView
            {
                Location = new Point(12, 12),
                Size = new Size(390, 150),
                View = View.Details,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                OwnerDraw = true,
                BorderStyle = BorderStyle.None,
                GridLines = false
            };

            lstEvaluation.Columns.Add("", 130, HorizontalAlignment.Left);
            lstEvaluation.Columns.Add("Trefl", 50, HorizontalAlignment.Right);
            lstEvaluation.Columns.Add("Karo", 60, HorizontalAlignment.Right);
            lstEvaluation.Columns.Add("Kupa", 50, HorizontalAlignment.Right);
            lstEvaluation.Columns.Add("Maça", 50, HorizontalAlignment.Right);
            lstEvaluation.Columns.Add("Total", 50, HorizontalAlignment.Right);

            lstEvaluation.DrawColumnHeader += LstEvaluation_DrawColumnHeader;
            lstEvaluation.DrawSubItem += LstEvaluation_DrawSubItem;

            lblExplanation = new Label
            {
                Text = "Açıklamalar",
                Location = new Point(12, 175),
                AutoSize = true,
                Font = new Font("Arial", 9F, FontStyle.Regular)
            };

            cmbExplanation = new ComboBox
            {
                Location = new Point(95, 172),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbExplanation.Items.AddRange(new string[] {
                "Onör Puanları",
                "Dağılım Puanları",
                "Destek Puanı",
                "Toplam Puanlar",
                "Hazır Löveler",
                "Löve Sayısı"
            });
            cmbExplanation.SelectedIndex = 0;
            cmbExplanation.SelectedIndexChanged += CmbExplanation_SelectedIndexChanged;

            txtExplanationDetails = new TextBox
            {
                Location = new Point(12, 202),
                Size = new Size(390, 140),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = "Her As için 4, her Rua için 3, her Dam için 2 ve her Vale için 1 onör puanı sayılır.",
                Font = new Font("Arial", 9.5F, FontStyle.Regular),
                BackColor = SystemColors.Window
            };

            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(160, 352),
                Size = new Size(90, 28),
                UseVisualStyleBackColor = true
            };
            btnOK.Click += (s, e) => this.Close();

            this.Controls.Add(lstEvaluation);
            this.Controls.Add(lblExplanation);
            this.Controls.Add(cmbExplanation);
            this.Controls.Add(txtExplanationDetails);
            this.Controls.Add(btnOK);
        }

        public void DegerleriYukle(List<Card> kartlar)
        {
            lstEvaluation.Items.Clear();

            if (kartlar == null || kartlar.Count == 0) return;

            // Renklere göre ayırma (Trefl: ♣, Karo: ♦, Kupa: ♥, Maça: ♠)
            // Lütfen Kart sınıfınızdaki Renk/Suit veya Deger/Rank özellik adlarına göre adapte edin.
            var cKart = kartlar.Where(k => k.Suit == "Sinek").ToList(); // Trefl (♣)
            var dKart = kartlar.Where(k => k.Suit == "Karo").ToList();  // Karo (♦)
            var hKart = kartlar.Where(k => k.Suit == "Kupa").ToList();  // Kupa (♥)
            var sKart = kartlar.Where(k => k.Suit == "Maça").ToList();  // Maça (♠)

            // 1. Onör Puanları (HCP)
            int cHcp = HesaplaOnorPuan(cKart);
            int dHcp = HesaplaOnorPuan(dKart);
            int hHcp = HesaplaOnorPuan(hKart);
            int sHcp = HesaplaOnorPuan(sKart);
            int totalHcp = cHcp + dHcp + hHcp + sHcp;

            // 2. Dağılım Puanları (Distribution)
            int cDist = HesaplaDagilimPuan(cKart);
            int dDist = HesaplaDagilimPuan(dKart);
            int hDist = HesaplaDagilimPuan(hKart);
            int sDist = HesaplaDagilimPuan(sKart);
            int totalDist = cDist + dDist + hDist + sDist;

            // 3. Destek/Deklarasyon Puanları (Bidding/Support)
            int cBidding = 0, dBidding = 0, hBidding = 0, sBidding = 0;
            int totalBidding = 0;

            // 4. Toplam Puanlar
            int cTotal = cHcp + cDist + cBidding;
            int dTotal = dHcp + dDist + dBidding;
            int hTotal = hHcp + hDist + hBidding;
            int sTotal = sHcp + sDist + sBidding;
            int grandTotal = totalHcp + totalDist + totalBidding;

            // 5. Hazır Löveler (Quick Tricks)
            double cQt = HesaplaCabukEl(cKart);
            double dQt = HesaplaCabukEl(dKart);
            double hQt = HesaplaCabukEl(hKart);
            double sQt = HesaplaCabukEl(sKart);
            double totalQt = cQt + dQt + hQt + sQt;

            // 6. Löve Sayısı (Playing Tricks)
            double cPt = HesaplaOyunEli(cKart);
            double dPt = HesaplaOyunEli(dKart);
            double hPt = HesaplaOyunEli(hKart);
            double sPt = HesaplaOyunEli(sKart);
            double totalPt = cPt + dPt + hPt + sPt;

            // Tabloya Ekleme
            AddRow("Onör Puanları", cHcp.ToString(), dHcp.ToString(), hHcp.ToString(), sHcp.ToString(), totalHcp.ToString());
            AddRow("Dağılım Puanları", cDist.ToString(), dDist.ToString(), hDist.ToString(), sDist.ToString(), totalDist.ToString());
            AddRow("Destek Puanı", cBidding.ToString(), dBidding.ToString(), hBidding.ToString(), sBidding.ToString(), totalBidding.ToString());
            AddRow("Toplam Puanlar", cTotal.ToString(), dTotal.ToString(), hTotal.ToString(), sTotal.ToString(), grandTotal.ToString());
            AddRow("Hazır Löveler", FormatDouble(cQt), FormatDouble(dQt), FormatDouble(hQt), FormatDouble(sQt), FormatDouble(totalQt));
            AddRow("Löve Sayısı", FormatDouble(cPt), FormatDouble(dPt), FormatDouble(hPt), FormatDouble(sPt), FormatDouble(totalPt));
        }

        private int HesaplaOnorPuan(List<Card> renkKartlari)
        {
            int p = 0;
            foreach (var k in renkKartlari)
            {
                if (k.Value == 14) p += 4;      // As
                else if (k.Value == 13) p += 3; // Papaz (Rua)
                else if (k.Value == 12) p += 2; // Kız (Dam)
                else if (k.Value == 11) p += 1; // Vale
            }
            return p;
        }

        private int HesaplaDagilimPuan(List<Card> renkKartlari)
        {
            int count = renkKartlari.Count;
            if (count == 0) return 3; // Şikan
            if (count == 1)
            {
                var single = renkKartlari[0];
                if (single.Value == 13 || single.Value == 12) return 1; // Sek Rua veya Dam
                return 2; // Singleton
            }
            if (count == 2)
            {
                bool hasQorJ = renkKartlari.Any(k => k.Value == 12 || k.Value == 11);
                if (hasQorJ) return 0; // Sek Q/J
                return 1; // Doubleton
            }
            if (count >= 6) return count - 5;
            return 0;
        }

        private double HesaplaCabukEl(List<Card> renkKartlari)
        {
            bool hasA = renkKartlari.Any(k => k.Value == 14);
            bool hasK = renkKartlari.Any(k => k.Value == 13);
            bool hasQ = renkKartlari.Any(k => k.Value == 12);

            if (hasA && hasK) return 2.0;
            if (hasA && hasQ) return 1.5;
            if (hasA) return 1.0;
            if (hasK && hasQ) return 1.0;
            if (hasK && renkKartlari.Count >= 2) return 0.5;

            return 0.0;
        }

        private double HesaplaOyunEli(List<Card> renkKartlari)
        {
            int count = renkKartlari.Count;
            if (count == 0) return 0;

            double pt = 0;

            if (renkKartlari.Any(k => k.Value == 14)) pt += 1.0;
            if (renkKartlari.Any(k => k.Value == 13) && count >= 2) pt += 1.0;
            if (renkKartlari.Any(k => k.Value == 12) && count >= 3) pt += 1.0;

            if (count > 3) pt += (count - 3); // Uzunluk löveleri

            return pt;
        }

        private string FormatDouble(double val)
        {
            if (val == 0) return "0";
            int intPart = (int)val;
            double frac = val - intPart;

            if (Math.Abs(frac - 0.5) < 0.01)
            {
                return intPart == 0 ? "1/2" : $"{intPart} 1/2";
            }
            return val.ToString("0.#");
        }

        private void AddRow(string title, string c, string d, string h, string s, string total)
        {
            ListViewItem item = new ListViewItem(title);
            item.SubItems.Add(c);
            item.SubItems.Add(d);
            item.SubItems.Add(h);
            item.SubItems.Add(s);
            item.SubItems.Add(total);
            lstEvaluation.Items.Add(item);
        }

        private void LstEvaluation_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawBackground();
            using (Font headerFont = new Font("Arial", 8.5F, FontStyle.Regular))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = e.Header.TextAlign == HorizontalAlignment.Right ? StringAlignment.Far : StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(e.Header.Text, headerFont, Brushes.Black, e.Bounds, sf);
            }

            if (e.ColumnIndex > 0)
            {
                int lineY = e.Bounds.Bottom - 1;
                using (Pen pen = new Pen(Color.Gray, 1))
                {
                    e.Graphics.DrawLine(pen, e.Bounds.Left, lineY, e.Bounds.Right, lineY);
                }
            }
        }

        private void LstEvaluation_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();
            using (Font font = new Font("Arial", 8.5F, FontStyle.Regular))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = e.ColumnIndex == 0 ? StringAlignment.Near : StringAlignment.Far;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(e.SubItem.Text, font, Brushes.Black, e.Bounds, sf);
            }
        }

        private void CmbExplanation_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbExplanation.SelectedItem?.ToString())
            {
                case "Onör Puanları":
                    txtExplanationDetails.Text = "Her As için 4, her Rua için 3, her Dam için 2 ve her Vale için 1 onör puanı sayılır.";
                    break;
                case "Dağılım Puanları":
                    txtExplanationDetails.Text = "Şikana 3 dağılım puanı verin. Singltona 2 dağılım puanı verin; ancak tekli Rua veya Dam için bundan 1 puan düşün. Doubltona 1 dağılım puanı verin; ancak sek Dam (Q-x) veya sek Vale (J-x) için dağılım puanı saymayın. Beşli Majör oynarken 6 kartlı renk için 1 puan, 7 kartlı renk için 2 puan vb. sayın; sek Rua veya Dam için 0 dağılım puanı, sek Vale için ise 1 dağılım puanı sayın; iki adet kuvvetli 5 kartlı renkle ilave 1 dağılım puanı veya bir adet kuvvetli 6 kartlı renk ile bir adet kuvvetli 5 kartlı renkle (veya daha iyisiyle!) ilave 2 dağılım puanı sayın.";
                    break;
                case "Destek Puanı":
                    txtExplanationDetails.Text = "Ortak bir renkte 4 veya daha fazla kart gösterirse, o renkteki tüm kısa renk dağılım puanlarını düşün (başka bir renkte 8 kartlı fitiniz yoksa). Siz ve ortağınız 8 kartlı bir fite sahipseniz ve sizin 4 veya daha fazla kozunuz varsa, her şikan için 2 puan ve her teklik için 1 puan ekleyin (fitiniz ortağın rengindeyse ve sizde o renkten 3'ten az kart varsa hariç). Ortağınızla bir majör renkte 8 karttan fazla bir fite sahipseniz, 8. karttan sonraki her kart için bir puan ekleyin. Ortağınızla bir minör renkte 9 karttan fazla bir fite sahipseniz, 9. karttan sonraki her kart için bir puan ekleyin. Siz ve ortağınız 8 kartlı bir fite sahipseniz ve yan renginiz sağlamsa (solid side suit), yan rengin 5. kartı için 1 puan ve sonraki her kart için 2 puan ekleyin. Beşli Majör (La Majeure Cinquième) oynarken, ortağın rengindeki As, Rua, Ru-Vale, Dam veya Dam-Vale için 1 puan sayın; ortağın majörüne 4 kartlı destek ile teklik için ilave 1 puan ve şikan için ilave 2 puan sayın; ortakla fit içerisinde dokuz veya daha fazla kartınız varsa, dokuzuncu ve sonraki her kart için ilave 1 puan sayın.";
                    break;
                case "Toplam Puanlar":
                    txtExplanationDetails.Text = "Elin toplam puanı; onör puanları, dağılım puanları ve deklarasyon puanlarının toplamıdır.";
                    break;
                case "Hazır Löveler":
                    txtExplanationDetails.Text = "Hazır Löveler (Çabuk eller), elinizin defansta ne kadar faydalı olabileceğinin bir ölçüsüdür. Çabuk el sayınızı hesaplamak için aşağıdakileri kullanın:\r\n\r\n- A-K (As-Rua): 2 çabuk el\r\n\r\n- A-Q (As-Dam): 1 1/2 çabuk el\r\n\r\n- A (As) veya K-Q (Rua-Dam): 1 çabuk el\r\n\r\n- K-x (Sek Rua / Korunan Rua): 1/2 çabuk el";
                    break;
                case "Löve Sayısı":
                    txtExplanationDetails.Text = "Oyun elleri (oyun löveleri) şu şekilde hesaplanır: Dışarıdaki kayıp kart sayısı çift ise, hiçbir defans oyuncusunun bu kartların yarısından fazlasına sahip olmayacağı varsayılır. Dışarıda tek sayıda (2n+1) kayıp kart olduğu varsayılsın. Bu durumda, hiçbir defans oyuncusunun bu kartların (n+1) tanesinden fazlasına sahip olmayacağı ve en yüksek (n) kartını oynadıktan sonra eldeki en yüksek kalan kartın düşmüş olacağı varsayılır. Buna ek olarak, renkte bir empas yapabileceğiniz varsayılır ve başarısı empasa bağlı olan bir löve için yarım (1/2) oyun eli sayılır.";
                    break;
                default:
                    txtExplanationDetails.Text = cmbExplanation.SelectedItem?.ToString();
                    break;
            }
        }
    }
}