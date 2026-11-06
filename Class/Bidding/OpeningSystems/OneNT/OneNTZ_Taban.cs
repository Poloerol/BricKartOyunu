using System.Collections.Generic;
using System.Linq;
using BricKartOyunu.Class;
using BricKartOyunu.Class.Bidding.Interfaces;
using BricKartOyunu.Class.Bidding.OpeningSystems.OneClub;
using static BricKartOyunu.Class.Bidding.OpeningSystems.OneClub.OcTablo;

namespace BricKartOyunu.Class.Bidding.OpeningSystems.OneNT
{
    /// <summary>
    /// ZAYIF NT VERSİYONU: 12-14 1NT AÇILIŞININ GELİŞİMLERİ (SAS kitabı s.87-89). OneNT_AnswerSistemi açık ve StrongNT=false iken.
    ///   1NT-2♣ (davet Stayman):  açan 2♦ (4'lü majör yok) / 2♥ (4♥, 4♠ olabilir) / 2♠ (4♠); cevapçı davet devamları
    ///   1NT-2♦ (FG Stayman):     açan 2♥ / 2♠ / 2NT; cevapçı uyum varsa 4M (16+ HCP ile 3M slam ilgisi), 5+♠ ise 2♠, aksi 3NT
    ///   1NT-2♥ / 2♠ (doğal NF):  açan 14 HCP ve 3+ destekle 3M, değilse Pas
    ///   1NT-2NT (davet):         açan 13+ ile 3NT, 12 HCP ve 5 kartlı ♣/♦ ile 3♣/3♦, değilse Pas
    /// Basitleştirmeler: "KK/BK" ve slam ilgisi (SI) alt dalları, 3 seviyesi cevapçı devamlarından sonra açanın üçüncü turu genel motora bırakıldı.
    /// </summary>
    public abstract class OneNTZ_Taban : OneClub_GelisimTabani
    {
        protected override string Acilis { get { return "1NT"; } }
        public override int Oncelik { get { return 42; } }
        internal static int Uz(OcEl e, char c) { return c == '♠' ? e.S : c == '♥' ? e.H : c == '♦' ? e.D : e.C; }
        internal override bool DevamTanimli(string rebid) { return true; }
        internal override string AciciRoleCevap(OcEl e, string rebid, string devam) { return null; }
    }

    public class OneNTZ_TwoClub : OneNTZ_Taban
    {
        protected override string Cevap { get { return "2♣"; } }
        public override string Aciklama { get { return "Zayıf NT: 1NT-2♣ davet Stayman (kitap s.88)"; } }

        internal override string AciciRebid(OcEl e) { return e.H >= 4 ? "2♥" : (e.S >= 4 ? "2♠" : "2♦"); }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int s = e.S, h = e.H, c = e.C, d = e.D;
            switch (rebid)
            {
                case "2♦":   // 4'lü majör yok
                    return Sec(new[] {
                        K("3♥", x => h >= 6), K("3♠", x => s >= 6),
                        K("2♥", x => h >= 5), K("2♠", x => s >= 5),
                        K("3♣", x => c >= 5 && (h == 4 || s == 4)),
                        K("3♦", x => d >= 5 && (h == 4 || s == 4)),
                    }, e, "2NT");
                case "2♥":   // 4♥ (4♠ olabilir)
                    return Sec(new[] {
                        K("3♥", x => h >= 4), K("3♠", x => s >= 6), K("2♠", x => s >= 5),
                        K("3♣", x => s == 4 && c >= 5), K("3♦", x => s == 4 && d >= 5),
                    }, e, "2NT");
                case "2♠":   // 4♠
                    return Sec(new[] {
                        K("3♠", x => s >= 4), K("3♥", x => h >= 6),
                        K("3♣", x => h == 4 && c >= 5), K("3♦", x => h == 4 && d >= 5),
                    }, e, "2NT");               // 2NT = 4/5♥ davet
                default: return "Pas";
            }
        }
    }

    public class OneNTZ_TwoDiamond : OneNTZ_Taban
    {
        protected override string Cevap { get { return "2♦"; } }
        public override string Aciklama { get { return "Zayıf NT: 1NT-2♦ FG Stayman (kitap s.89)"; } }

        internal override string AciciRebid(OcEl e) { return e.H >= 4 ? "2♥" : (e.S >= 4 ? "2♠" : "2NT"); }

        internal override string CevapciDevam(OcEl e, string rebid)
        {
            int hp = e.Hcp;
            if (rebid == "2♥")
            {
                if (e.H >= 4) return hp >= 16 ? "3♥" : "4♥";                               // uyum: SI / oynamak için
                if (e.S >= 5) return "2♠";                                                  // 5+♠ FG
                return "3NT";
            }
            if (rebid == "2♠")
            {
                if (e.S >= 4) return hp >= 16 ? "3♠" : "4♠";
                if (e.H >= 5) return "3♥";                                                  // 5+♥
                return "3NT";
            }
            return "3NT";                                                                   // majör yok: oyun
        }
    }

    public abstract class OneNTZ_DoganMajor : OneNTZ_Taban
    {
        protected abstract char M { get; }
        internal override string AciciRebid(OcEl e) { return e.Hcp >= 14 && Uz(e, M) >= 3 ? "3" + M : "Pas"; }
        internal override string CevapciDevam(OcEl e, string rebid) { return "Pas"; }
    }

    public class OneNTZ_TwoHeart : OneNTZ_DoganMajor
    {
        protected override char M { get { return '♥'; } }
        protected override string Cevap { get { return "2♥"; } }
        public override string Aciklama { get { return "Zayıf NT: 1NT-2♥ doğal NF (kitap s.87)"; } }
    }

    public class OneNTZ_TwoSpade : OneNTZ_DoganMajor
    {
        protected override char M { get { return '♠'; } }
        protected override string Cevap { get { return "2♠"; } }
        public override string Aciklama { get { return "Zayıf NT: 1NT-2♠ doğal NF (kitap s.87)"; } }
    }

    public class OneNTZ_TwoNT : OneNTZ_Taban
    {
        protected override string Cevap { get { return "2NT"; } }
        public override string Aciklama { get { return "Zayıf NT: 1NT-2NT davet (kitap s.89)"; } }

        internal override string AciciRebid(OcEl e)
        {
            if (e.Hcp >= 13) return "3NT";
            if (e.C >= 5) return "3♣";
            if (e.D >= 5) return "3♦";
            return "Pas";
        }
        internal override string CevapciDevam(OcEl e, string rebid) { return rebid == "3♣" || rebid == "3♦" ? (e.Hcp >= 12 ? "3NT" : "Pas") : "Pas"; }
    }
}
