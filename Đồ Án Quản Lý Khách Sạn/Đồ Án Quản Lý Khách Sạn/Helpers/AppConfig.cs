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
        private const string KEY_TI_LE_PHU_THU = "TiLePhuThu";
        private const string KEY_THUE_VAT      = "ThueSuatVAT";
        private const string KEY_TI_LE_BO_SUNG    = "TiLeBoSungGia";
        private const string KEY_TI_LE_BO_SUNG_DV = "TiLeBoSungGiaDV";

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
 
        /// Lấy hệ số cao nhất trong danh sách loại khách (theo MaLKH) trong booking.
        /// Chỉ áp dụng một hệ số duy nhất — hệ số cao nhất — không nhân dồn nhiều loại.
        public static decimal GetCombinedHeSo(IEnumerable<int> maLKHs)
        {
            var ids = maLKHs.Where(id => id > 0).Distinct().ToList();
            if (!ids.Any()) return 1m;
            using var ctx = new HotelDbContext();
            var heSos = ctx.LoaiKhachHangs
                .Where(l => ids.Contains(l.MaLKH))
                .Select(l => l.HeSoGia)
                .ToList();
            return heSos.Any() ? heSos.Max() : 1m;
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

        public static void SetTiLePhuThu(decimal v) =>
            SetValue(KEY_TI_LE_PHU_THU, v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        /// Lấy thuế suất VAT (%). Mặc định 10.
        public static decimal GetVAT() => GetDecimal(KEY_THUE_VAT, 10m);

        /// Thiết lập thuế suất VAT (%).
        public static void SetVAT(decimal v) =>
            SetValue(KEY_THUE_VAT, v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        /// Lấy tỉ lệ bổ sung giá phòng (%). 0 = không đổi, +20 = tăng 20%, -10 = giảm 10%.
        public static decimal GetTiLeBoSungGia() => GetDecimal(KEY_TI_LE_BO_SUNG, 0m);

        public static void SetTiLeBoSungGia(decimal v) =>
            SetValue(KEY_TI_LE_BO_SUNG, v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        /// Tính giá phòng hiện tại sau khi áp dụng tỉ lệ bổ sung.
        public static decimal GetGiaPhongHienTai(decimal giaGoc)
        {
            decimal tiLe = GetTiLeBoSungGia();
            return Math.Round(giaGoc * (1 + tiLe / 100), 0);
        }

        /// Lấy tỉ lệ bổ sung giá dịch vụ (%). 0 = không đổi, +20 = tăng 20%, -10 = giảm 10%.
        public static decimal GetTiLeBoSungGiaDV() => GetDecimal(KEY_TI_LE_BO_SUNG_DV, 0m);

        public static void SetTiLeBoSungGiaDV(decimal v) =>
            SetValue(KEY_TI_LE_BO_SUNG_DV, v.ToString(System.Globalization.CultureInfo.InvariantCulture));

        /// Tính giá dịch vụ hiện tại sau khi áp dụng tỉ lệ bổ sung.
        public static decimal GetGiaDichVuHienTai(decimal donGiaGoc)
        {
            decimal tiLe = GetTiLeBoSungGiaDV();
            return Math.Round(donGiaGoc * (1 + tiLe / 100), 0);
        }
    }
}
