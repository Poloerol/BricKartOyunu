using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BricKartOyunu.Class
{
    /// <summary>
    /// Kalıcı ayar deposu. Ayarlar kullanıcının AppData klasöründe JSON dosyaları olarak saklanır:
    ///   %AppData%\BricKartOyunu\&lt;ad&gt;.json
    /// - Uygulama güncellense/yeniden derlense bile ayarlar korunur (program klasörüne yazılmaz).
    /// - Yazma güvenlidir: önce geçici dosyaya yazılır, sonra yerine konur, eski sürüm .bak olarak kalır.
    /// - Bozuk bir dosya bulunursa silinmez, ".bozuk-tarih" adıyla kenara alınır ve varsayılan kullanılır.
    /// - Dosyada eksik alan varsa o alan varsayılan değerinde kalır (yeni sürümlerle uyumlu).
    /// Başka ayar türleri (konvansiyon kartları vb.) de aynı depoyu ad vererek kullanabilir.
    /// </summary>
    public static class AyarDeposu
    {
        private static readonly object _kilit = new object();

        /// <summary>Ayar dosyalarının bulunduğu klasör.</summary>
        public static string KlasorYolu
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BricKartOyunu");
            }
        }

        /// <summary>Verilen ayar adının tam dosya yolu (uzantısız ad verilir).</summary>
        public static string DosyaYolu(string ad)
        {
            return Path.Combine(KlasorYolu, ad + ".json");
        }

        private static JsonSerializerSettings Ayarlar()
        {
            var a = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ObjectCreationHandling = ObjectCreationHandling.Replace,   // diziler eklenmesin, yerine konsun
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            a.Converters.Add(new StringEnumConverter());                    // enum'lar okunaklı metin olarak
            return a;
        }

        /// <summary>
        /// Ayarı yükler. Dosya yoksa ya da okunamazsa yeni (varsayılan) nesne döner.
        /// </summary>
        public static T Yukle<T>(string ad) where T : class, new()
        {
            lock (_kilit)
            {
                string yol = DosyaYolu(ad);
                try
                {
                    if (!File.Exists(yol)) return new T();

                    string json = File.ReadAllText(yol, Encoding.UTF8);
                    var nesne = JsonConvert.DeserializeObject<T>(json, Ayarlar());
                    return nesne ?? new T();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[AyarDeposu] Yükleme hatası (" + ad + "): " + ex.Message);
                    BozukDosyayiKenaraAl(yol);
                    return new T();
                }
            }
        }

        /// <summary>Ayarı kaydeder. Başarılıysa true döner.</summary>
        public static bool Kaydet<T>(string ad, T nesne) where T : class
        {
            if (nesne == null) return false;
            lock (_kilit)
            {
                string yol = DosyaYolu(ad);
                string gecici = yol + ".tmp";
                try
                {
                    Directory.CreateDirectory(KlasorYolu);
                    string json = JsonConvert.SerializeObject(nesne, Ayarlar());
                    File.WriteAllText(gecici, json, new UTF8Encoding(false));

                    if (File.Exists(yol)) File.Replace(gecici, yol, yol + ".bak");
                    else File.Move(gecici, yol);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[AyarDeposu] Kaydetme hatası (" + ad + "): " + ex.Message);
                    try { if (File.Exists(gecici)) File.Delete(gecici); } catch { }
                    return false;
                }
            }
        }

        /// <summary>Kayıtlı ayar dosyasını siler (bir sonraki yüklemede varsayılan gelir).</summary>
        private static void BozukDosyayiKenaraAl(string yol)
        {
            try
            {
                if (!File.Exists(yol)) return;
                string hedef = yol + ".bozuk-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                File.Move(yol, hedef);
            }
            catch { /* kenara alınamadıysa sessizce geç */ }
        }
    }
}
