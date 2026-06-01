using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Helpers
{
    public static class AppConfig
    {
        private const string KEY_HE_SO         = "HeSoNuocNgoai";
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

        // ── Hệ số giá khách nước ngoài (legacy – giữ để tương thích) ──────
        public static decimal GetHeSoNuocNgoai() => GetDecimal(KEY_HE_SO, 1.5m);
        public static void    SetHeSoNuocNgoai(decimal v) =>
            SetValue(KEY_HE_SO, v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        // ── Hệ số theo mã loại khách ────────────────────────────────────────
        public static decimal GetHeSoByCode(string maCode)
        {
            if (string.IsNullOrEmpty(maCode)) return 1m;
            using var ctx = new HotelDbContext();
            return ctx.LoaiKhachHangs.FirstOrDefault(l => l.MaCode == maCode)?.HeSoGia ?? 1m;
        }

        // Nhân tất cả hệ số của các loại khách trong booking lại với nhau
        public static decimal GetCombinedHeSo(IEnumerable<string> maCodes)
        {
            var codes = maCodes.Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            if (!codes.Any()) return 1m;
            using var ctx = new HotelDbContext();
            var heSos = ctx.LoaiKhachHangs
                .Where(l => codes.Contains(l.MaCode))
                .Select(l => l.HeSoGia)
                .ToList();
            return heSos.Any() ? heSos.Aggregate(1m, (acc, h) => acc * h) : 1m;
        }

        public static decimal GetMaxHeSo(IEnumerable<string> maCodes) => GetCombinedHeSo(maCodes);

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
