using BricKartOyunu.Class;
using BricKartOyunu.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class.Bidding;
using BricKartOyunu.Class.Bidding.Conventions;


namespace BricKartOyunu
{
    // Bilgi panelleri, rozetler, seed paneli, durum çubuğu
    public partial class BricOyna
    {
        private void SetupInfoBoard()
        {
            // Infoboard boyutunu büyütüyoruz
            int size = 50;
            int panelX = 20;
            int panelY = _kuzeyY;   // ← Kuzey elinin Y koordinatı

            if (infoBoardPanel == null)
            {
                infoBoardPanel = new Panel { Location = new Point(panelX, panelY), Size = new Size(size * 3, size * 3), BackColor = Color.Transparent };
                EnableDoubleBuffer(infoBoardPanel);

                lblTour = new Label { Text = _currentTour.ToString(), Size = new Size(size, size), Location = new Point(size, size), BackColor = Color.LightGray, TextAlign = ContentAlignment.MiddleCenter, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Arial", 14F, FontStyle.Bold) };
                lblNorth = CreateInfoBox(size, size, 0);
                lblEast = CreateInfoBox(size, size * 2, size);
                lblSouth = CreateInfoBox(size, size, size * 2);
                lblWest = CreateInfoBox(size, 0, size);

                infoBoardPanel.Controls.Add(lblTour);
                infoBoardPanel.Controls.Add(lblNorth);
                infoBoardPanel.Controls.Add(lblEast);
                infoBoardPanel.Controls.Add(lblSouth);
                infoBoardPanel.Controls.Add(lblWest);
                this.Controls.Add(infoBoardPanel);
            }
            infoBoardPanel.Location = new Point(panelX, panelY);
            infoBoardPanel.BringToFront();
        }

        /// <summary>
        /// Sağ üst köşede oyun bilgi panelini (Dealer, Zon, Kontrat, Deklaran,
        /// Kazanılan El) oluşturur. Panel her zaman görünür.
        /// </summary>
        private void SetupOyunBilgiPanel()
        {
            if (oyunBilgiPanel != null) return;

            oyunBilgiPanel = new Panel
            {
                // Başlangıç boyutu, dinamik olarak güncellenecek
                Size = new Size(BilgiPanelGenislik, 300),
                BackColor = Color.FromArgb(245, 245, 220),
                BorderStyle = BorderStyle.FixedSingle
            };
            EnableDoubleBuffer(oyunBilgiPanel);

            // ─── Başlık ───
            var lblBaslik = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Text = "OYUN BİLGİSİ",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 90)
            };
            oyunBilgiPanel.Controls.Add(lblBaslik);

            // ─── İçerik paneli ───
            var icerik = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 6, 8, 6),
                BackColor = Color.Transparent
            };
            oyunBilgiPanel.Controls.Add(icerik);
            icerik.BringToFront();

            // ─── Bilgi satırları ───
            int y = 0;
            const int satirYuksekligi = 22;

            lblBilgiTur = BilgiSatiriOlustur(icerik, "Tur:", y); y += satirYuksekligi;
            lblBilgiDealer = BilgiSatiriOlustur(icerik, "Dealer:", y); y += satirYuksekligi;
            lblBilgiZon = BilgiSatiriOlustur(icerik, "Zon:", y); y += satirYuksekligi;
            lblBilgiKontrat = BilgiSatiriOlustur(icerik, "Kontrat:", y); y += satirYuksekligi;
            lblBilgiDeklaran = BilgiSatiriOlustur(icerik, "Deklaran:", y); y += satirYuksekligi + 4;
            lblBilgiSeed = BilgiSatiriOlustur(icerik, "Seed:", y); y += satirYuksekligi;

            // ─── Kazanılan El başlığı ───
            var lblKazanilanBaslik = new Label
            {
                Location = new Point(8, y),
                Size = new Size(icerik.Width - 16, 20),
                Text = "Kazanılan El",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
            icerik.Controls.Add(lblKazanilanBaslik);
            y += 22;

            lblBilgiKazanilanEl = new Label
            {
                Location = new Point(8, y),
                Size = new Size(icerik.Width - 16, 22),
                Text = "NS: 0   •   EW: 0",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                TextAlign = ContentAlignment.MiddleCenter
            };
            icerik.Controls.Add(lblBilgiKazanilanEl);
            y += lblBilgiKazanilanEl.Height; // 🔹 En alt elemanın yüksekliğini de ekle

            // ═══════════════════════════════════════════════════════════════
            // DÜZELTME: Paneli içeriğin gerçek yüksekliğine göre boyutlandır
            // ═══════════════════════════════════════════════════════════════
            int icerikGercekYuksekligi = y + icerik.Padding.Top + icerik.Padding.Bottom;
            int yeniPanelYuksekligi = lblBaslik.Height + icerikGercekYuksekligi + 4; // +4 kenarlık payı

            oyunBilgiPanel.Height = yeniPanelYuksekligi;

            this.Controls.Add(oyunBilgiPanel);
            oyunBilgiPanel.BringToFront();

            // 🔹 SEED KISMI BURADAN TAMAMEN SİLİNDİ. 
            // SetupSeedPanel() zaten kendi panelini oluşturuyor.
        }

        /// <summary>
        /// Oyun bilgi panelinin hemen altına Seed Yönetimi panelini koyar.
        /// </summary>
        private void SetupSeedPanel()
        {
            if (seedPanel != null) return;

            seedPanel = new Panel
            {
                Size = new Size(BilgiPanelGenislik, 90),
                BackColor = Color.FromArgb(245, 245, 220),
                BorderStyle = BorderStyle.FixedSingle
            };
            EnableDoubleBuffer(seedPanel);

            // ─── Başlık ───
            var lblBaslik = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Text = "SEED YÖNETİMİ",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 90)
            };
            seedPanel.Controls.Add(lblBaslik);

            // ─── İçerik paneli ───
            var icerik = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(6, 4, 6, 4),
                BackColor = Color.Transparent
            };
            seedPanel.Controls.Add(icerik);
            icerik.BringToFront();

            // ─── ComboBox ───
            int y = 4;
            var lblSeed = new Label
            {
                Location = new Point(6, y),
                Size = new Size(42, 22),
                Text = "Seed:",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
            icerik.Controls.Add(lblSeed);

            cmbSeed = new ComboBox
            {
                Location = new Point(50, y),
                Size = new Size(155, 22),
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            icerik.Controls.Add(cmbSeed);

            // ─── Butonlar (ComboBox altında) ───
            y += 30;

            btnSeedSil = new Button
            {
                Location = new Point(6, y),
                Size = new Size(70, 26),
                Text = "Sil",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true
            };
            btnSeedSil.Click += BtnSeedSil_Click;
            icerik.Controls.Add(btnSeedSil);

            btnSeedDagit = new Button
            {
                Location = new Point(82, y),
                Size = new Size(90, 26),
                Text = "Dağıt",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true
            };
            btnSeedDagit.Click += BtnSeedDagit_Click;
            icerik.Controls.Add(btnSeedDagit);

            this.Controls.Add(seedPanel);
            seedPanel.BringToFront();

            System.Diagnostics.Debug.WriteLine(
                "[SetupSeedPanel] Seed paneli oluşturuldu.");
        }

        /// <summary>
        /// ComboBox'ı günceller (son seed'ler).
        /// </summary>
        private void ComboBoxGuncelle()
        {
            if (cmbSeed == null) return;

            cmbSeed.Items.Clear();
            foreach (var seed in _seedHafizasi)
            {
                cmbSeed.Items.Add(seed);
            }

            if (cmbSeed.Items.Count > 0)
                cmbSeed.SelectedIndex = cmbSeed.Items.Count - 1;
        }



        /// <summary>
        /// Dosyadan seed geçmişini okur.
        /// </summary>
        private void SeedHafizasiniYukle()
        {
            if (!File.Exists(SeedDosyaYolu)) return;

            try
            {
                foreach (var satir in File.ReadAllLines(SeedDosyaYolu))
                {
                    if (int.TryParse(satir.Trim(), out int seed))
                    {
                        if (!_seedHafizasi.Contains(seed))
                        {
                            if (_seedHafizasi.Count >= MaxSeedHafiza)
                                _seedHafizasi.Dequeue();
                            _seedHafizasi.Enqueue(seed);
                        }
                    }
                }
                ComboBoxGuncelle();
                System.Diagnostics.Debug.WriteLine($"[SeedYukle] {_seedHafizasi.Count} seed dosyadan yüklendi.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SeedYukle] Hata: {ex.Message}");
            }
        }




        /// <summary>
        /// Bellekteki seed geçmişini dosyaya yazar.
        /// </summary>
        private void SeedHafizasiniKaydet()
        {
            try
            {
                File.WriteAllLines(SeedDosyaYolu, _seedHafizasi.Select(s => s.ToString()));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SeedKaydet] Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Seed'i hafızaya ekler (FIFO, max 10).
        /// </summary>
        private void SeedHafizayaEkle(int seed)
        {
            if (_seedHafizasi.Contains(seed)) return;

            if (_seedHafizasi.Count >= MaxSeedHafiza)
                _seedHafizasi.Dequeue();

            _seedHafizasi.Enqueue(seed);
            ComboBoxGuncelle();

            // 🔹 EKLE: Her yeni seed eklendiğinde dosyaya kaydet
            SeedHafizasiniKaydet();

            System.Diagnostics.Debug.WriteLine(
                $"[SeedHafizasi] Eklendi: {seed} (toplam: {_seedHafizasi.Count})");
        }

        /// <summary>
        /// "Sil" butonu: seçili seed'i siler.
        /// </summary>
        private void BtnSeedSil_Click(object sender, EventArgs e)
        {
            if (cmbSeed == null || string.IsNullOrWhiteSpace(cmbSeed.Text)) return;

            if (!int.TryParse(cmbSeed.Text.Trim(), out int seciliSeed)) return;

            var gecici = _seedHafizasi.Where(s => s != seciliSeed).ToList();
            _seedHafizasi.Clear();
            foreach (var s in gecici)
                _seedHafizasi.Enqueue(s);

            ComboBoxGuncelle();
            SeedHafizasiniKaydet();

            System.Diagnostics.Debug.WriteLine(
                $"[SeedSil] Silindi: {seciliSeed} (kalan: {_seedHafizasi.Count})");
        }

        /// <summary>
        /// "Dağıt" butonu: seçili seed ile el dağıtır.
        /// </summary>
        private void BtnSeedDagit_Click(object sender, EventArgs e)
        {
            if (cmbSeed == null || string.IsNullOrWhiteSpace(cmbSeed.Text))
            {
                MessageBox.Show("Lütfen bir seed değeri girin veya seçin.", "Seed Dağıt",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 🔹 Değişiklik: Sadece seçili elemanı değil, kutudaki metni oku (manuel giriş için)
            if (!int.TryParse(cmbSeed.Text.Trim(), out int seciliSeed))
            {
                MessageBox.Show("Geçersiz seed formatı! Lütfen sadece sayı girin.", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var cevap = MessageBox.Show(
                $"Seed: {seciliSeed}\n\n" +
                "Mevcut el kaybolacak ve seçili seed ile yeni el dağıtılacak.\n" +
                "Devam etmek istiyor musunuz?",
                "Seed ile Dağıt",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (cevap != DialogResult.Yes) return;

            KartlariDagitSeedIle(AnaSayfa.SeciliKartSeti, seciliSeed);

            _oyunAsamasi = OyunAsamasi.Baslangic;
            _currentTour++;
            DagiticiyiAyarla();
            activePlayer = _dealer;
            _declarer = _dealer;
            _atakYapacakOyuncu = _dealer;
            _kontrat = null;
            _kontratDeklaran = _dealer;

            ButonlariInaktifYap();
            UpdateInfoBoard(_dealer);
            OyunBilgiPaneliniGuncelle();
            YeniDekBasFormAc();

            System.Diagnostics.Debug.WriteLine(
                $"[SeedDagit] Manuel giriş/Seçim ile dağıtıldı: {seciliSeed}");
        }

        /// <summary>
        /// OYUN BİLGİSİ panelinin 10px altında yer alan el numarası panelini oluşturur.
        /// </summary>
        /// <summary>
        /// Etiket + değer satırı oluşturur (örn. "Dealer: Kuzey").
        /// </summary>
        private Label BilgiSatiriOlustur(Panel parent, string baslik, int y)
        {
            var lblBaslik = new Label
            {
                Location = new Point(8, y),
                Size = new Size(70, 20),
                Text = baslik,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(lblBaslik);

            var lblDeger = new Label
            {
                Location = new Point(80, y),
                Size = new Size(parent.Width - 88, 20),
                Text = "—",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(lblDeger);

            return lblDeger;
        }

        /// <summary>
        /// Oyun bilgi panelini günceller.
        /// </summary>
        /// <summary>Durum çubuğu masa rengini almasın: üstteki menü çubuğuyla aynı görünüm/renk.</summary>
        /// <summary>Durum çubuğunu masa renginden bağımsız, sabit açık gri çizer.</summary>
        private class SabitDurumCubuguRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (var br = new SolidBrush(SystemColors.Control))
                    e.Graphics.FillRectangle(br, e.AffectedBounds);
            }
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                e.Graphics.DrawLine(SystemPens.ControlDark, 0, 0, e.ToolStrip.Width, 0);
            }
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = Color.Black;
                base.OnRenderItemText(e);
            }
        }

        /// <summary>Oyun alanı (playZonePanel) masa renginin biraz koyusu olur; masa rengi değişince birlikte değişir.</summary>
        private void PlayZoneRenginiGuncelle()
        {
            if (playZonePanel == null) return;
            Color m = AnaSayfa.MasaRengi;
            const double k = 0.78;   // varsayılan masa (0,128,0) → panel (0,100,0) oranı
            playZonePanel.BackColor = Color.FromArgb((int)(m.R * k), (int)(m.G * k), (int)(m.B * k));
        }

        private void DurumCubuguRenginiSabitle()
        {
            if (statusStrip1 == null) return;
            // menuStrip1.BackColor formdan (masa rengi) devralındığı için ondan okunmaz;
            // menünün sistem görünümüyle aynı sabit renkler kullanılır.
            statusStrip1.Dock = DockStyle.Bottom;
            statusStrip1.Renderer = new SabitDurumCubuguRenderer();
            statusStrip1.BackColor = SystemColors.Control;
            statusStrip1.ForeColor = Color.Black;
            statusStrip1.BringToFront();
            foreach (ToolStripItem it in statusStrip1.Items)
            {
                it.BackColor = SystemColors.Control;
                it.ForeColor = Color.Black;
            }
        }

        private ToolStripStatusLabel _sbSira, _sbEl, _sbDealer, _sbZon, _sbKontrat, _sbNS, _sbEW;

        private void DurumCubuguGuncelle()
        {
            if (statusStrip1 == null) return;
            if (_sbSira == null)
            {
                Func<ToolStripStatusLabel> kutu = () => new ToolStripStatusLabel { BorderSides = ToolStripStatusLabelBorderSides.Left };
                _sbSira = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
                _sbEl = kutu(); _sbDealer = kutu(); _sbZon = kutu();
                _sbKontrat = kutu(); _sbNS = kutu(); _sbEW = kutu();
                statusStrip1.Items.AddRange(new ToolStripItem[] { _sbSira, _sbEl, _sbDealer, _sbZon, _sbKontrat, _sbNS, _sbEW });
                DurumCubuguRenginiSabitle();
            }

            string oyuncu = PlayerIsmiTurkce(_activePlayer);
            if (_oyunAsamasi == OyunAsamasi.Oyun)
            {
                bool atak = _kazanilanElNS + _kazanilanElEW == 0 && _activePlayer == _atakYapacakOyuncu;
                _sbSira.Text = atak ? oyuncu + " atakta" : "Sıra: " + oyuncu;
            }
            else if (_oyunAsamasi == OyunAsamasi.Deklarasyon)
                _sbSira.Text = "Teklif sırası: " + oyuncu;
            else
                _sbSira.Text = "";

            _sbEl.Text = "El no: " + _seed;
            _sbDealer.Text = "Dağıtıcı: " + PlayerIsmiTurkce(_dealer);
            _sbZon.Text = "Zon: " + ZonDurumuMetni();
            _sbKontrat.Text = string.IsNullOrEmpty(_kontrat)
                ? "Kontrat: —"
                : _kontrat + KontrEki() + " – " + PlayerIsmiTurkce(_kontratDeklaran);
            _sbNS.Text = "Alınan K/G: " + _kazanilanElNS;
            _sbEW.Text = "Alınan D/B: " + _kazanilanElEW;
        }

        private void OyunBilgiPaneliniGuncelle()
        {
            DurumCubuguGuncelle();
            if (oyunBilgiPanel == null) return;

            // 🔹 Tur
            if (lblBilgiTur != null)
                lblBilgiTur.Text = _currentTour.ToString();

            // Dealer
            if (lblBilgiDealer != null)
                lblBilgiDealer.Text = PlayerIsmiTurkce(_dealer);

            // Zon
            if (lblBilgiZon != null)
                lblBilgiZon.Text = ZonDurumuMetni();

            // Kontrat
            if (lblBilgiKontrat != null)
                lblBilgiKontrat.Text = string.IsNullOrEmpty(_kontrat) ? "—" : _kontrat + KontrEki();

            // Deklaran
            if (lblBilgiDeklaran != null)
                lblBilgiDeklaran.Text = string.IsNullOrEmpty(_kontrat)
                    ? "—"
                    : PlayerIsmiTurkce(_kontratDeklaran);

            // 🔹 Seed
            if (lblBilgiSeed != null)
                lblBilgiSeed.Text = _seed.ToString();

            // Kazanılan El
            if (lblBilgiKazanilanEl != null)
                lblBilgiKazanilanEl.Text = $"NS: {_kazanilanElNS}   •   EW: {_kazanilanElEW}";

            oyunBilgiPanel.BringToFront();
        }

        private Label CreateInfoBox(int size, int x, int y)
        {
            return new Label { Size = new Size(size, size), Location = new Point(x, y), TextAlign = ContentAlignment.MiddleCenter, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Arial", 12F, FontStyle.Bold) };
        }

        public void UpdateInfoBoard(Player deklaran)
        {
            _declarer = deklaran;
            if (lblTour == null || lblNorth == null || lblEast == null ||
                lblSouth == null || lblWest == null) return;

            lblTour.Text = _currentTour.ToString();

            // 16 Bordluk Standart Briç Zon Döngüsü Hesaplaması:
            // _currentTour değerinin 1-16 arası modunu alıyoruz.
            int boardMod = ((_currentTour - 1) % 16) + 1;

            // Standard Briç Zon Tanımları (16'lı Bord Döngüsü):
            // Zonsuz (Nobody): 1, 8, 11, 14
            // N-S Zonda: 2, 5, 12, 15
            // E-W Zonda: 3, 6, 9, 13
            // Herkes Zonda (Both): 4, 7, 10, 16
            bool nsZonda = (boardMod == 2 || boardMod == 5 || boardMod == 12 || boardMod == 15 ||
                            boardMod == 4 || boardMod == 7 || boardMod == 10 || boardMod == 16);

            bool ewZonda = (boardMod == 3 || boardMod == 6 || boardMod == 9 || boardMod == 13 ||
                            boardMod == 4 || boardMod == 7 || boardMod == 10 || boardMod == 16);

            // "D" harfi HER ZAMAN dağıtıcıyı (dealer) gösterir
            // Kutular fiziksel pusulayı gösterir (zon bord numarasından, dağıtıcı eşlemeden)
            Player fizDagitici = KoltukHaritasi.Fiz(_dealer);
            UpdateBox(lblNorth, nsZonda, fizDagitici == Player.Kuzey);
            UpdateBox(lblSouth, nsZonda, fizDagitici == Player.Guney);
            UpdateBox(lblEast, ewZonda, fizDagitici == Player.Dogu);
            UpdateBox(lblWest, ewZonda, fizDagitici == Player.Bati);

            // 🔹 YENİ: Oyun bilgi panelindeki Zon ve Dealer'ı da güncelle
            OyunBilgiPaneliniGuncelle();
        }

        private void UpdateBox(Label lbl, bool isZonActive, bool dagiticiMi = false)
        {
            // Dağıtıcının kutusunda "D" harfi gösterilir
            lbl.Text = dagiticiMi ? "D" : string.Empty;

            // Zonda ise Kırmızı, zonsuz ise Beyaz yapıyoruz
            lbl.BackColor = isZonActive ? Color.Red : Color.White;
            lbl.ForeColor = isZonActive ? Color.White : Color.Black;
        }

        private void SetupPlayerBadges()
        {
            // 🔹 İlk çağrıda Label'ları oluştur (SADECE BİR KEZ)
            // Sonraki çağrılarda yeniden yaratma, sadece konumlarını güncelle.
            // Bu, her Resize'da 8 Label × (handle + GDI) sızıntısını önler.
            if (lblN_Dir == null)
            {
                CreateBadge(out lblN_Dir, out lblN_Name, "N", "Oyuncu 3", 0, 0);
                CreateBadge(out lblS_Dir, out lblS_Name, "G", "Siz", 0, 0);
                CreateBadge(out lblW_Dir, out lblW_Name, "B", "Oyuncu 2", 0, 0);
                CreateBadge(out lblE_Dir, out lblE_Name, "E", "Oyuncu 4", 0, 0);
            }

            int bosluk = 1;

            // ─── KUZEY ───
            int kuzeyY_Alt = _kuzeyY + _kartYukseklik + bosluk;
            int kuzeyX_Baslangic = 250;
            lblN_Dir.Location = new Point(kuzeyX_Baslangic, kuzeyY_Alt);
            lblN_Name.Location = new Point(kuzeyX_Baslangic + 22, kuzeyY_Alt);

            // ─── GÜNEY ───
            int guneyY_Alt = _guneyY + _kartYukseklik + bosluk;
            int guneyX_Baslangic = 250;
            lblS_Dir.Location = new Point(guneyX_Baslangic, guneyY_Alt);
            lblS_Name.Location = new Point(guneyX_Baslangic + 22, guneyY_Alt);

            // ─── BATI ve DOĞU ortak Y ───
            int ewStartingY = GetCenteredEWStartingY();
            int suitBoslugu = 2;
            int maxEWToplamYukseklik = (4 * _kartYukseklik) + (3 * suitBoslugu);
            int gercekEwY_Alt = ewStartingY + maxEWToplamYukseklik + bosluk;

            // ─── BATI ───
            int batiX = 40;
            lblW_Dir.Location = new Point(batiX, gercekEwY_Alt);
            lblW_Name.Location = new Point(batiX + 22, gercekEwY_Alt);

            // ─── DOĞU ───
            int doguX = doguKartlar[0] != null ? doguKartlar[0].Left : 930;
            lblE_Dir.Location = new Point(doguX, gercekEwY_Alt);
            lblE_Name.Location = new Point(doguX + 22, gercekEwY_Alt);

            RozetHarfleriniGuncelle();

            // Aktif oyuncunun rengini güncelle
            UpdatePlayerBadges();
        }

        /// <summary>Ekrandaki yön harflerini insanın fiziksel koltuğuna göre yazar.</summary>
        private void RozetHarfleriniGuncelle()
        {
            if (lblN_Dir == null) return;
            lblN_Dir.Text = KoltukHaritasi.Harf(KoltukHaritasi.Fiz(Player.Kuzey));
            lblS_Dir.Text = KoltukHaritasi.Harf(KoltukHaritasi.Fiz(Player.Guney));
            lblW_Dir.Text = KoltukHaritasi.Harf(KoltukHaritasi.Fiz(Player.Bati));
            lblE_Dir.Text = KoltukHaritasi.Harf(KoltukHaritasi.Fiz(Player.Dogu));
        }

        /// <summary>Oyun Planı'ndaki insan koltuğunu uygular (yeni bord başında / açılışta).</summary>
        private void OyunPlaniniUygula()
        {
            KoltukHaritasi.Uygula(AnaSayfa.OyunPlani == null ? null : AnaSayfa.OyunPlani.InsanMi);
            RozetHarfleriniGuncelle();
        }

        private void CreateBadge(out Label dirLbl, out Label nameLbl, string dir, string name, int x, int y)
        {
            dirLbl = new Label
            {
                Text = dir,
                Size = new Size(20, 20),
                Location = new Point(x, y),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                Font = _badgeDirFont  // 🔹 Static font
            };

            nameLbl = new Label
            {
                Text = name,
                Size = new Size(80, 20),
                Location = new Point(x + 22, y),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                Font = _badgeNameFont  // 🔹 Static font
            };

            this.Controls.Add(dirLbl);
            this.Controls.Add(nameLbl);
            dirLbl.BringToFront();
            nameLbl.BringToFront();
        }

        private void UpdatePlayerBadges()
        {
            Label[] names = { lblN_Name, lblE_Name, lblS_Name, lblW_Name };
            Label[] dirs = { lblN_Dir, lblE_Dir, lblS_Dir, lblW_Dir };
            Player[] players = { Player.Kuzey, Player.Dogu, Player.Guney, Player.Bati };

            for (int i = 0; i < 4; i++)
            {
                if (names[i] == null || dirs[i] == null) continue;
                bool isActive = (activePlayer == players[i]);
                names[i].BackColor = isActive ? Color.Yellow : Color.Gray;
                names[i].ForeColor = isActive ? Color.Black : Color.White;
                dirs[i].BackColor = isActive ? Color.Yellow : Color.Gray;
                dirs[i].ForeColor = isActive ? Color.Black : Color.White;
            }
        }

        private void SetupPlayZone()
        {
            if (playZonePanel == null)
            {
                playZonePanel = new Panel
                {
                    BackColor = Color.FromArgb(0, 100, 0),
                    BorderStyle = BorderStyle.FixedSingle,
                    Size = new Size(Ol(500), Ol(500)),
                    Cursor = Cursors.Hand   // 🔹 Tıklanabilir hissi
                };
                EnableDoubleBuffer(playZonePanel);
                this.Controls.Add(playZonePanel);
                PlayZoneRenginiGuncelle();

                // 🔹 YENİ: Panel tıklaması — el bittiğinde yeni el başlatır
                playZonePanel.Click += PlayZonePanel_Click;
            }

            playZonePanel.Left = ((this.ClientSize.Width - playZonePanel.Width) / 6) + 30;
            playZonePanel.Top = (_kuzeyY + _guneyY) / 2 - (playZonePanel.Height / 3);
            playZonePanel.BringToFront();
        }

    }
}
