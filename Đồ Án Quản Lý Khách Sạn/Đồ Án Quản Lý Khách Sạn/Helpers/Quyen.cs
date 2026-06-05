namespace Đồ_Án_Quản_Lý_Khách_Sạn.Helpers
{
    public static class Quyen
    {
        public const string HuyHoaDon       = "HuyHoaDon";
        public const string XemBaoCao       = "XemBaoCao";
        public const string QuanLyLoaiPhong = "QuanLyLoaiPhong";
        public const string QuanLyDichVu    = "QuanLyDichVu";
        public const string QuanLyMaGiamGia = "QuanLyMaGiamGia";
        public const string QuanLyNhanVien  = "QuanLyNhanVien";
        public const string QuanLyLoaiKH    = "QuanLyLoaiKH";
        public const string CauHinhHeThong  = "CauHinhHeThong";

        // Tên hiển thị cho từng quyền
        public static readonly Dictionary<string, string> TenQuyen = new()
        {
            [HuyHoaDon]       = "Hủy hóa đơn",
            [XemBaoCao]       = "Xem báo cáo & thống kê",
            [QuanLyLoaiPhong] = "Quản lý loại phòng",
            [QuanLyDichVu]    = "Quản lý dịch vụ",
            [QuanLyMaGiamGia] = "Quản lý mã giảm giá",
            [QuanLyNhanVien]  = "Quản lý nhân viên",
            [QuanLyLoaiKH]    = "CRUD loại khách hàng & hệ số",
            [CauHinhHeThong]  = "Truy cập & chỉnh sửa cấu hình hệ thống",
        };

        // Quyền mặc định theo vai trò (dùng khi tạo nhân viên mới)
        public static readonly HashSet<string> MacDinhQuanLy = new()
        {
            HuyHoaDon, XemBaoCao, QuanLyLoaiPhong,
            QuanLyDichVu, QuanLyMaGiamGia, QuanLyNhanVien, QuanLyLoaiKH
            // CauHinhHeThong KHÔNG có trong default — phải cấp thủ công
        };

        public static readonly HashSet<string> MacDinhLeTan = new();
    }
}
