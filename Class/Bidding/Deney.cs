using System;

namespace BricKartOyunu.Class.Bidding
{
    /// <summary>
    /// Ölçüm deneyleri için geçici eşik anahtarları: ortam değişkeninden okunur, yoksa varsayılan döner.
    /// Eşik netleşince değer koda yazılır ve ilgili çağrı kaldırılır (üretimde ortam değişkeni kullanılmaz).
    /// </summary>
    public static class Deney
    {
        public static int Al(string ad, int varsayilan)
        {
            string s = Environment.GetEnvironmentVariable(ad);
            int v;
            return !string.IsNullOrEmpty(s) && int.TryParse(s, out v) ? v : varsayilan;
        }
    }
}
