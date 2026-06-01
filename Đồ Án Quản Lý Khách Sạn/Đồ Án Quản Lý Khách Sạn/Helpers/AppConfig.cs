using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Helpers
{
    public static class AppConfig
    {
        private const string KEY_SUC_CHUA_MAX  = "SucChuaToiDa";
        private const string KEY_TI_LE_PHU_THU = "TiLePhuThu";

        // ── Helpers ────────────────────────────────────────────────────────
        private static decimal GetDecimal(string key, decimal defaultVal)
        {
            using var ctx = new HotelDbContext();
            var val = ctx.CauHinhs.Find(key)?.ConfigValue;
            return decimal.TryParse(val, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : defaultVal;
        }

        private static void SetValue(string key, string value)
        {
            using var ctx = new HotelDbContext();
            var cfg = ctx.CauHinhs.Find(key);
            if (cfg == null)
                ctx.CauHinhs.Add(new CauHinh { ConfigKey = key, ConfigValue = value });
            else
                cfg.ConfigValue = value;
            ctx.SaveChanges();
        }

        // ── Hệ số theo MaLKH (int PK của LoaiKhachHang) ───────────────────
        public static decimal GetHeSoByMaLKH(int maLKH)
        {
            using var ctx = new HotelDbContext();
            return ctx.LoaiKhachHangs.Find(maLKH)?.HeSoGia ?? 1m;
        }

        // Nhân tất cả hệ số của các loại khách (theo MaLKH) trong booking
        public static decimal GetCombinedHeSo(IEnumerable<int> maLKHs)
        {
            var ids = maLKHs.Where(id => id > 0).Distinct().ToList();
            if (!ids.Any()) return 1m;
            using var ctx = new HotelDbContext();
            var heSos = ctx.LoaiKhachHangs
                .Where(l => ids.Contains(l.MaLKH))
                .Select(l => l.HeSoGia)
                .ToList();
            return heSos.Any() ? heSos.Aggregate(1m, (acc, h) => acc * h) : 1m;
        }

        // ── Sức chứa tối đa ────────────────────────────────────────────────
        public static int GetSucChuaToiDa()
        {
            using var ctx = new HotelDbContext();
            var val = ctx.CauHinhs.Find(KEY_SUC_CHUA_MAX)?.ConfigValue;
            return int.TryParse(val, out var d) ? d : 4;
        }
        public static void SetSucChuaToiDa(int v) => SetValue(KEY_SUC_CHUA_MAX, v.ToString());

        // ── Tỷ lệ phụ thu vượt sức chứa ────────────────────────────────────
        public static decimal GetTiLePhuThu() => GetDecimal(KEY_TI_LE_PHU_THU, 0.25m);
        public static void    SetTiLePhuThu(decimal v) =>
            SetValue(KEY_TI_LE_PHU_THU, v.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
