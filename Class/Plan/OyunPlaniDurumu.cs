namespace BricKartOyunu.Class.Plan
{
    /// <summary>Yardım penceresine verilen anlık görüntü: kullanıcı deklaran tarafındaysa Deklaran, savunmadaysa Savunma dolu gelir.</summary>
    public sealed class OyunPlaniDurumu
    {
        public int Mod;                 // 0: hazır değil (Mesaj), 1: deklaran, 2: savunma
        public string Mesaj = "";
        public PlanGirdisi Deklaran;
        public SavunmaGirdisi Savunma;
    }
}
