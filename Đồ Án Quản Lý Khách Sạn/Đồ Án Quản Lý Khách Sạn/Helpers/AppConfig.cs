using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Helpers
{
    public static class AppConfig
    {
        private const string KEY_HE_SO = "HeSoNuocNgoai";

        public static decimal GetHeSoNuocNgoai()
        {
            using var ctx = new HotelDbContext();
            var val = ctx.CauHinhs.Find(KEY_HE_SO)?.GiaTri;
            return decimal.TryParse(val, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 1.5m;
        }

        public static void SetHeSoNuocNgoai(decimal value)
        {
            using var ctx = new HotelDbContext();
            var cfg = ctx.CauHinhs.Find(KEY_HE_SO);
            if (cfg == null)
                ctx.CauHinhs.Add(new CauHinh { Khoa = KEY_HE_SO, GiaTri = value.ToString(System.Globalization.CultureInfo.InvariantCulture) });
            else
                cfg.GiaTri = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            ctx.SaveChanges();
        }
    }
}
