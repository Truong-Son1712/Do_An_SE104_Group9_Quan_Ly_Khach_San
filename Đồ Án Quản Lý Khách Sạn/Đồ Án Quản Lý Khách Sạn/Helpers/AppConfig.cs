using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Helpers
{
    /// Lớp tiện ích quản lý cấu hình và quy định của hệ thống khách sạn.
    /// Cung cấp các phương thức để đọc và ghi các tham số động từ cơ sở dữ liệu.
    public static class AppConfig
    {
        /// Khóa cấu hình cho sức chứa tối đa trong một phòng.
        private const string KEY_SUC_CHUA_MAX  = "SucChuaToiDa";
        /// Khóa cấu hình cho tỉ lệ phụ thu khi vượt sức chứa.
        private const string KEY_TI_LE_PHU_THU = "TiLePhuThu";

        /// Lấy giá trị cấu hình kiểu decimal từ cơ sở dữ liệu.
        private static decimal GetDecimal(string key, decimal defaultVal)
        {
            using var ctx = new HotelDbContext();
            var val = ctx.CauHinhs.Find(key)?.ConfigValue;
            return decimal.TryParse(val, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : defaultVal;
        }

        /// Cập nhật hoặc thêm mới giá trị cấu hình dạng chuỗi vào cơ sở dữ liệu.
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

        /// Lấy hệ số giá dựa theo mã loại khách hàng (MaLKH).
        public static decimal GetHeSoByMaLKH(int maLKH)
        {
            using var ctx = new HotelDbContext();
            return ctx.LoaiKhachHangs.Find(maLKH)?.HeSoGia ?? 1m;
        }
 
        /// Nhân tất cả hệ số của các loại khách (theo MaLKH) trong booking.
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

        /// Lấy số lượng khách tối đa được phép thuê trong một phòng.
        public static int GetSucChuaToiDa()
        {
            using var ctx = new HotelDbContext();
            var val = ctx.CauHinhs.Find(KEY_SUC_CHUA_MAX)?.ConfigValue;
            return int.TryParse(val, out var d) ? d : 4;
        }

        /// Thiết lập số lượng khách tối đa được phép thuê trong một phòng.
        public static void SetSucChuaToiDa(int v) => SetValue(KEY_SUC_CHUA_MAX, v.ToString());

        /// Lấy tỉ lệ phụ thu khi số lượng khách vượt quá sức chứa tiêu chuẩn của phòng.
        public static decimal GetTiLePhuThu() => GetDecimal(KEY_TI_LE_PHU_THU, 0.25m);

        /// Thiết lập tỉ lệ phụ thu khi số lượng khách vượt quá sức chứa tiêu chuẩn của phòng.
        public static void SetTiLePhuThu(decimal v) =>
            SetValue(KEY_TI_LE_PHU_THU, v.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
