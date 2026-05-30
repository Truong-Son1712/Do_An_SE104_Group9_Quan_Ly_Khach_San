using System;

namespace WpfApp1.DTO
{
    /// <summary>
    /// Đối tượng DTO chứa các thống kê tổng quan cho Dashboard.
    /// File này được tạo mới cho nhiệm vụ của Thành viên 5.
    /// </summary>
    public class DashboardDTO
    {
        public int TongPhong { get; set; }
        public int PhongTrong { get; set; }
        public int PhongDangSuDung { get; set; }
        public int PhongBaoDuong { get; set; }
        public int DatPhongHomNay { get; set; }
        public int TraPhongHomNay { get; set; }
        public int TongKhachHang { get; set; }
        public decimal DoanhThuThangHienTai { get; set; }
    }
}
