using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Helpers
{
    /// <summary>
    /// Lớp tiện ích quản lý cấu hình và quy định của hệ thống khách sạn.
    /// Cung cấp các phương thức để đọc và ghi các tham số động từ cơ sở dữ liệu.
    /// </summary>
    public static class AppConfig
    {
        /// <summary>Khóa cấu hình cho sức chứa tối đa trong một phòng.</summary>
        private const string KEY_SUC_CHUA_MAX  = "SucChuaToiDa";
        /// <summary>Khóa cấu hình cho tỉ lệ phụ thu khi vượt sức chứa.</summary>
        private const string KEY_TI_LE_PHU_THU = "TiLePhuThu";

        /// <summary>
        /// Lấy giá trị cấu hình kiểu decimal từ cơ sở dữ liệu.
        /// </summary>
        /// <param name="key">Khóa cấu hình cần lấy</param>
        /// <param name="defaultVal">Giá trị mặc định nếu cấu hình không tồn tại hoặc lỗi định dạng</param>
        /// <returns>Giá trị cấu hình dạng decimal</returns>
        private static decimal GetDecimal(string key, decimal defaultVal)
        {
            using var ctx = new HotelDbContext();
            var val = ctx.CauHinhs.Find(key)?.ConfigValue;
            return decimal.TryParse(val, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : defaultVal;
        }

        /// <summary>
        /// Cập nhật hoặc thêm mới giá trị cấu hình dạng chuỗi vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="key">Khóa cấu hình cần đặt</param>
        /// <param name="value">Giá trị cấu hình mới dạng chuỗi</param>
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

        /// <summary>
        /// Lấy hệ số giá dựa theo mã loại khách hàng (MaLKH).
        /// </summary>
        /// <param name="maLKH">Mã loại khách hàng (int PK của LoaiKhachHang)</param>
        /// <returns>Hệ số giá tương ứng, mặc định là 1.0 nếu không tìm thấy</returns>
        public static decimal GetHeSoByMaLKH(int maLKH)
        {
            using var ctx = new HotelDbContext();
            return ctx.LoaiKhachHangs.Find(maLKH)?.HeSoGia ?? 1m;
        }

        /// <summary>
        /// Nhân tất cả hệ số của các loại khách (theo MaLKH) trong booking.
        /// </summary>
        /// <param name="maLKHs">Danh sách các mã loại khách hàng</param>
        /// <returns>Hệ số giá kết hợp sau khi nhân các hệ số</returns>
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

        /// <summary>
        /// Lấy số lượng khách tối đa được phép thuê trong một phòng.
        /// </summary>
        /// <returns>Sức chứa tối đa của hệ thống phòng, mặc định là 4</returns>
        public static int GetSucChuaToiDa()
        {
            using var ctx = new HotelDbContext();
            var val = ctx.CauHinhs.Find(KEY_SUC_CHUA_MAX)?.ConfigValue;
            return int.TryParse(val, out var d) ? d : 4;
        }

        /// <summary>
        /// Thiết lập số lượng khách tối đa được phép thuê trong một phòng.
        /// </summary>
        /// <param name="v">Sức chứa tối đa mới</param>
        public static void SetSucChuaToiDa(int v) => SetValue(KEY_SUC_CHUA_MAX, v.ToString());

        /// <summary>
        /// Lấy tỉ lệ phụ thu khi số lượng khách vượt quá sức chứa tiêu chuẩn của phòng.
        /// </summary>
        /// <returns>Tỉ lệ phụ thu (Ví dụ: 0.25 tương ứng với 25%)</returns>
        public static decimal GetTiLePhuThu() => GetDecimal(KEY_TI_LE_PHU_THU, 0.25m);

        /// <summary>
        /// Thiết lập tỉ lệ phụ thu khi số lượng khách vượt quá sức chứa tiêu chuẩn của phòng.
        /// </summary>
        /// <param name="v">Tỉ lệ phụ thu mới</param>
        public static void SetTiLePhuThu(decimal v) =>
            SetValue(KEY_TI_LE_PHU_THU, v.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
