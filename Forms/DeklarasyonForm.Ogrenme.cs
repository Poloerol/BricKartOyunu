using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding;

namespace BricKartOyunu.Forms
{
    // Öğrenme modu: insan teklifi motor önerisiyle karşılaştırılır, her teklifin anlamı notlara yazılır.
    public partial class DeklarasyonForm
    {
        private OgrenmeNotlariForm _ogrenmeNotlari;
        private int _ogrenmeDogru;
        private int _ogrenmeToplam;

        private static string OgrenmeNormalize(string t)
        {
            if (string.IsNullOrEmpty(t)) return "Pas";
            if (t == "PAS" || t == "pas") return "Pas";
            if (t == "Kontr") return "Dbl";
            if (t == "S.Kontr") return "RDbl";
            return t;
        }

        private static string OgrenmeBicim(string t)
        {
            switch (OgrenmeNormalize(t))
            {
                case "Pas": return "Pas";
                case "Dbl": return "Kontr";
                case "RDbl": return "S.Kontr";
                default: return t;
            }
        }

        private OgrenmeNotlariForm OgrenmeNotlari()
        {
            if (_ogrenmeNotlari == null || _ogrenmeNotlari.IsDisposed)
            {
                _ogrenmeNotlari = new OgrenmeNotlariForm();
                _ogrenmeNotlari.Owner = this;
                var ekran = Screen.FromControl(this).WorkingArea;
                int x = Math.Min(Right + 4, ekran.Right - _ogrenmeNotlari.Width);
                _ogrenmeNotlari.Location = new Point(Math.Max(ekran.Left, x), Math.Max(ekran.Top, Top));
            }
            if (!_ogrenmeNotlari.Visible) _ogrenmeNotlari.Show(this);
            return _ogrenmeNotlari;
        }

        private void OgrenmeKapat()
        {
            try
            {
                if (_ogrenmeNotlari != null && !_ogrenmeNotlari.IsDisposed)
                {
                    _ogrenmeNotlari.Owner = null;
                    _ogrenmeNotlari.Close();
                    _ogrenmeNotlari.Dispose();
                }
            }
            catch { }
            _ogrenmeNotlari = null;
        }

        /// <summary>Aktif oyuncu için motorun önerisi (yasadışıysa Pas).</summary>
        private bool OgrenmeOneri(out string oneri, out string kural, out string aciklama)
        {
            oneri = null; kural = null; aciklama = null;
            try
            {
                var aktifP = OyuncuyaCevir(_aktifOyuncu);
                var el = _bricOyna?.GetOyuncuEli(aktifP);
                if (el == null || el.Count != 13) return false;

                SyncHumanVsAiSession();
                var durum = IhaleDurumuOlustur();
                if (_ihaleMotoru == null)
                    _ihaleMotoru = new IhaleMotoru(_anlasma);

                string t = OgrenmeNormalize(_ihaleMotoru.TeklifVer(durum));
                kural = _ihaleMotoru.SonKonvansiyonAdi;
                aciklama = _ihaleMotoru.KonvansiyonAciklamasi(kural);
                try
                {
                    if (t != "Pas" && _humanVsAiSession != null &&
                        !BidValidator.IsLegal(_humanVsAiSession.State, aktifP, Bid.Parse(t)))
                        t = "Pas";
                }
                catch { t = "Pas"; }
                oneri = t;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Öğrenme] öneri hatası: " + ex.Message);
                return false;
            }
        }

        /// <summary>Teklifi yapanın bu tekliften sonra gösterdiği tahmini HP aralığı.</summary>
        private string OgrenmeHpMetni(string teklif)
        {
            try
            {
                var yapan = OyuncuyaCevir(_aktifOyuncu);
                var gecmis = new List<IhaleHamlesi>(_ihaleHamleleri);
                gecmis.Add(new IhaleHamlesi { Oyuncu = yapan, Teklif = OgrenmeNormalize(teklif), Sira = gecmis.Count + 1 });
                var d = new IhaleDurumu
                {
                    AktifOyuncu = OyunKurallari.Ortak(yapan),
                    Anlasma = _anlasma,
                    TurNo = 1,
                    Gecmis = gecmis
                };
                var ar = IhaleKoruma.PartnerAraligi(d);
                if (ar.Max >= 40 && ar.Min <= 0) return null;
                if (ar.Max >= 40) return ar.Min + "+ HP";
                return ar.Min == ar.Max ? ar.Min + " HP" : ar.Min + "–" + ar.Max + " HP";
            }
            catch { return null; }
        }

        private void OgrenmeNotYaz(string baslik, string kural, string aciklama, string teklif, Color renk)
        {
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(kural)) sb.AppendLine("Kural: " + kural);
            if (!string.IsNullOrEmpty(aciklama)) sb.AppendLine(aciklama);
            string hp = OgrenmeHpMetni(teklif);
            if (hp != null) sb.AppendLine("Gösterdiği güç: " + hp);
            var n = OgrenmeNotlari();
            n.Text = "Öğrenme Notları  (" + _ogrenmeDogru + "/" + _ogrenmeToplam + " öneriyle aynı)";
            n.Yaz(baslik, sb.ToString().TrimEnd(), renk);
        }

        /// <summary>
        /// İnsan teklif vermeden önce çağrılır. true: teklif verilebilir; false: oyuncu vazgeçti.
        /// </summary>
        private bool OgrenmeInsanTeklifiOnayla(string teklif)
        {
            if (!AnlasmaYonetici.OgrenmeModuAcik) return true;

            string oneri, kural, aciklama;
            if (!OgrenmeOneri(out oneri, out kural, out aciklama)) return true;

            string isim = OyuncuIsmiTurkce(_aktifOyuncu);
            string benim = OgrenmeNormalize(teklif);
            _ogrenmeToplam++;

            if (benim == oneri)
            {
                _ogrenmeDogru++;
                OgrenmeNotYaz("✔ " + isim + ": " + OgrenmeBicim(benim) + " — motor önerisiyle aynı",
                              kural, aciklama, benim, Color.LightGreen);
                return true;
            }

            var sonuc = MessageBox.Show(this,
                "Motorun önerisi: " + OgrenmeBicim(oneri) +
                (string.IsNullOrEmpty(kural) ? "" : "\nKural: " + kural) +
                (string.IsNullOrEmpty(aciklama) ? "" : "\n" + aciklama) +
                "\n\nSeçtiğiniz teklif: " + OgrenmeBicim(benim) +
                "\n\nYine de bu teklifi oynamak istiyor musunuz?\n(Hayır: teklifi geri al)",
                "Öğrenme modu", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (sonuc != DialogResult.Yes)
            {
                _ogrenmeToplam--;
                return false;
            }

            OgrenmeNotYaz("✘ " + isim + ": " + OgrenmeBicim(benim) + " — öneri: " + OgrenmeBicim(oneri),
                          kural, aciklama, oneri, Color.Orange);
            return true;
        }

        /// <summary>Yapay zekâ teklifinin anlamını notlara yazar.</summary>
        private void OgrenmeAIKaydet(string teklif, string kural)
        {
            if (!AnlasmaYonetici.OgrenmeModuAcik) return;
            try
            {
                string aciklama = _ihaleMotoru != null ? _ihaleMotoru.KonvansiyonAciklamasi(kural) : null;
                OgrenmeNotYaz(OyuncuIsmiTurkce(_aktifOyuncu) + ": " + OgrenmeBicim(teklif),
                              kural, aciklama, teklif, Color.LightSkyBlue);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Öğrenme] not hatası: " + ex.Message);
            }
        }
    }
}
